// Games/YsxzSecShell.cpp —— 勇士之心 1.0.4 SecShell（libsecret.so）运行态净化看门狗
//
// 背景（Frida 实证，20260930～20261001）：
//   本包加固壳 libsecret.so 在启动 413~483ms 跑完整性自检，本作因「重签名 + 外科改动」
//   有 3 项检查失败（failedMask bit0x2/0x10/0x200），壳把 dirty 置 1（dirty @ 模块 RVA
//   0x64e98，failedMask @ 0x64ea4，checkedMask @ 0x64ea0），随后 ~649ms 的 unshell 流程
//   只给真实 Application 调 attachBaseContext，**跳过 onCreate** ⇒
//   BingoApplication.onCreate 不跑 ⇒ PluginHelper.initWithApplication 不跑 ⇒
//   PluginHelper.app == null ⇒ AppActivity 启动时 AppInfo.parameters NPE 闪退。
//   用 Frida 每 1ms 把 dirty/failedMask 清零 ⇒ 壳在 651ms 记录
//   "### Application.onCreate CALLED on org.cocos2dx.javascript.WarriorApplication"，
//   NPE 消失、游戏进主界面、悬浮菜单弹出。壳会周期性重检（707ms/3.87s 会再置位）
//   ⇒ 必须持续清零。
//
// 本文件把该实验生产化：一个后台线程持续把 dirty/failedMask 归零。
// 启动时机是整个方案的关键：必须在壳自检（~413ms）之前。libstarry_magic.so 由主
// provider 加载时已 ~651ms（太晚），而 appComponentFactory
// （com.starry.magic.ysxz.YsxzAppComponentFactory）实测 ~365ms 就被框架调用，
// 因此 JNI 入口挂在它的静态方法上。Ysxz_OnLoad 里再补一次调用（幂等）兜底。
//
// 净化为「只写运行时状态字、不改指令、不重放生命周期」：不触碰壳的代码段，
// 也不自己调用真实 Application.onCreate（那会重入同一启动路径并死锁主线程）。

#include "YsxzSecShell.h"

#include <jni.h>
#include <pthread.h>
#include <unistd.h>
#include <dlfcn.h>
#include <stdint.h>
#include <stdio.h>
#include <string.h>
#include <atomic>
#include <time.h>

#include "Includes/Logger.h"

#define YSXZ_SEC_LOGI(...) LOGI("[YsxzSec] " __VA_ARGS__)
#define YSXZ_SEC_LOGW(...) LOGW("[YsxzSec] " __VA_ARGS__)
#define YSXZ_SEC_LOGE(...) LOGE("[YsxzSec] " __VA_ARGS__)

namespace {

constexpr const char *kSecLib = "libsecret.so";

// 导出符号兜底 RVA。壳库随版本重编，导出地址会整体位移，故改成**候选表 + 验证**：
// 逐个试 RVA，命中判据 = 能从该函数序言反推出「可写的」状态字，而不是凭 RVA 硬认。
// 1.0.6 的值取自官方 base.apk 的 libsecret.so 导出表（nm -D --defined-only）：
//   T SecShell_IsDirty 0x54130 / T SecShell_FailedMask 0x541c8（CheckedMask 0x54184）。
struct AccessorCandidate {
    uintptr_t rva;
    const char *ver;
};
constexpr AccessorCandidate kIsDirtyCandidates[] = {
    {0x54130, "1.0.6"},
    {0x4e5cc, "1.0.4"},
};
constexpr AccessorCandidate kFailedMaskCandidates[] = {
    {0x541c8, "1.0.6"},
    {0x4e664, "1.0.4"},
};

// checkedMask 仅供诊断记录（本进程不写它）。
// dirty / failedMask 现在一律由 ResolveStateWord 反推得到，不再用「硬 RVA 兜底」——
// 旧值 0x64e98 / 0x64ea4 只对 1.0.4 的 libsecret.so 成立，换版本会打错内存。
constexpr uintptr_t kCheckedRvaFallback = 0x64ea0;

std::atomic<bool> g_started{false};
std::atomic<uintptr_t> g_dirtyWord{0};
std::atomic<uintptr_t> g_failedWord{0};
std::atomic<uintptr_t> g_checkedWord{0};


// 收集 libsecret.so 的**全部** offset==0 映射基址（升序，guest 镜像优先）。
// 为什么需要多个：雷电是 x86_64 + Houdini，壳库既能以 guest(ARM64) 镜像出现在
// 0x40xx_xxxx_xxxx 段，也会被宿主 x86_64 加载器以大地址映射一份（实测 0x7252_…）。
// 旧实现只取「跨度最大」单条，实测会抽中宿主那份：ARM64 指令解码拿到乱码 ⇒
// 状态字反推失败 ⇒ 看门狗退出 ⇒ 壳自检 dirty=1 ⇒ AppInfo.parameters NPE 闪退。
// 现在把候选全收上来，由 WatchThread 逐个验证（验证通过才写）。
int CollectSecLibBases(uintptr_t *out, int max) {
    FILE *fp = fopen("/proc/self/maps", "r");
    if (!fp) {
        return 0;
    }
    char line[512];
    int n = 0;
    while (fgets(line, sizeof(line), fp)) {
        if (!strstr(line, kSecLib)) {
            continue;
        }
        unsigned long start = 0, end = 0, off = 0;
        if (sscanf(line, "%lx-%lx %*4s %lx", &start, &end, &off) != 3) {
            continue;
        }
        if (off != 0 || end <= start) {
            continue;
        }
        const uintptr_t b = static_cast<uintptr_t>(start);
        bool dup = false;
        for (int i = 0; i < n; ++i) {
            if (out[i] == b) {
                dup = true;
                break;
            }
        }
        if (dup || n >= max) {
            continue;
        }
        out[n++] = b;
    }
    fclose(fp);
    for (int i = 1; i < n; ++i) {  // 升序：guest 0x40xx… 排在宿主 0x7xxx… 之前
        const uintptr_t v = out[i];
        int j = i - 1;
        while (j >= 0 && out[j] > v) {
            out[j + 1] = out[j];
            --j;
        }
        out[j + 1] = v;
    }
    return n;
}

// 地址是否落在带 w 权限的映射内（越界写会 SIGSEGV，必须先校验）。
bool AddrIsWritable(uintptr_t addr) {
    FILE *fp = fopen("/proc/self/maps", "r");
    if (!fp) {
        return false;
    }
    char line[512];
    bool ok = false;
    while (fgets(line, sizeof(line), fp)) {
        unsigned long start = 0, end = 0;
        char perms[8] = {0};
        if (sscanf(line, "%lx-%lx %7s", &start, &end, perms) != 3) {
            continue;
        }
        if (addr >= start && addr < end) {
            ok = (perms[1] == 'w');
            break;
        }
    }
    fclose(fp);
    return ok;
}

bool AddressIsReadableCode(uintptr_t addr) {
    FILE *fp = fopen("/proc/self/maps", "r");
    if (!fp) {
        return false;
    }
    char line[512];
    bool ok = false;
    while (fgets(line, sizeof(line), fp)) {
        unsigned long start = 0, end = 0;
        char perms[8] = {0};
        if (sscanf(line, "%lx-%lx %7s", &start, &end, perms) != 3) {
            continue;
        }
        if (addr >= start && addr + 4 <= end) {
            ok = (perms[2] == 'x' || perms[0] == 'r');
            break;
        }
    }
    fclose(fp);
    return ok;
}

// 从访问器函数序言里反推它读/写的全局字地址：找第一个 ADRP Xd,#page，
// 其后第一个基址寄存器 == Xd 的 LDR Wt,[Xn,#imm12*4] 即目标。
// 这样拿到的是「本包 libsecret.so 的实际布局」，不依赖硬编码偏移。
bool DecodeGlobalWord(uintptr_t fn, uintptr_t *out) {
    if (!fn || !AddressIsReadableCode(fn)) {
        return false;
    }
    const uint32_t *code = reinterpret_cast<const uint32_t *>(fn);
    uintptr_t page = 0;
    int rd = -1;
    for (int i = 0; i < 32; ++i) {
        const uint32_t insn = code[i];
        if (insn == 0xD65F03C0u) {  // ret
            break;
        }
        if ((insn & 0x9F000000u) == 0x90000000u) {  // ADRP Xd, #imm
            int64_t imm = static_cast<int64_t>((insn >> 5) & 0x7FFFF) << 2;
            imm |= (insn >> 29) & 0x3;
            if (imm & (1LL << 20)) {
                imm -= (1LL << 21);
            }
            const uintptr_t pc_page = reinterpret_cast<uintptr_t>(&code[i]) & ~static_cast<uintptr_t>(0xFFF);
            page = static_cast<uintptr_t>(static_cast<int64_t>(pc_page) + (imm << 12));
            rd = static_cast<int>(insn & 0x1F);
            continue;
        }
        if (rd >= 0 && (insn & 0xFFC00000u) == 0xB9400000u) {  // LDR Wt,[Xn,#imm12*4]
            const int rn = static_cast<int>((insn >> 5) & 0x1F);
            if (rn == rd) {
                *out = page + static_cast<uintptr_t>(((insn >> 10) & 0xFFFu) * 4);
                return true;
            }
        }
    }
    return false;
}

// 解析一个访问器导出，并直接反推出它的状态字地址。
// 顺序：① dlsym(RTLD_DEFAULT) ② 候选 RVA 表逐个验证。
// 命中判据统一为「DecodeGlobalWord 成功且该字可写」——1.0.4/1.0.6 共用同一份逻辑，
// 换版本只需在候选表里加一行，不需要为版本写分支。
uintptr_t ResolveStateWord(uintptr_t base, const char *symbol,
                           const AccessorCandidate *cands, size_t cand_count,
                           uintptr_t *out_word, const char **out_tag) {
    void *p = dlsym(RTLD_DEFAULT, symbol);
    if (p) {
        uintptr_t w = 0;
        if (DecodeGlobalWord(reinterpret_cast<uintptr_t>(p), &w) && AddrIsWritable(w)) {
            *out_word = w;
            if (out_tag) {
                *out_tag = "dlsym";
            }
            return reinterpret_cast<uintptr_t>(p);
        }
    }
    // APKSecure 私有命名空间下 dlsym 可能看不到导出，退回基址 + 候选 RVA。
    for (size_t i = 0; i < cand_count; ++i) {
        const uintptr_t fn = base + cands[i].rva;
        uintptr_t w = 0;
        if (DecodeGlobalWord(fn, &w) && AddrIsWritable(w)) {
            *out_word = w;
            if (out_tag) {
                *out_tag = cands[i].ver;
            }
            return fn;
        }
    }
    return 0;
}

void *WatchThread(void *) {
    // 解析状态字：**边等边验**，直到两个访问器能从同一份镜像反推出可写状态字。
    // 为什么不能拿到第一个 base 就收工：雷电上 libsecret.so 会先以【宿主 x86_64 映射】
    // 出现（0x7xxx…，ARM64 指令解不出来），guest(ARM64) 主镜像稍后才挂上（0x40xx…）。
    // 实测 21:38/21:41 两次都抽中宿主那份 ⇒ 反推失败 ⇒ 看门狗退出 ⇒ 壳自检 dirty=1
    // ⇒ AppInfo.parameters NPE 闪退；而 21:09 那次刚好先看到 guest 镜像就正常。
    // 所以改成每 20ms 重扫一次、重验一次，最多 ~4s（壳首轮自检在 413~483ms）。
    uintptr_t dirty = 0, failed = 0, checked = 0, base = 0;
    const char *dirty_tag = "?";
    const char *failed_tag = "?";
    int seen = 0;
    for (int round = 0; round < 200 && base == 0; ++round) {
        uintptr_t bases[8] = {0};
        const int nb = CollectSecLibBases(bases, 8);
        if (nb > seen) {
            seen = nb;
            YSXZ_SEC_LOGI("libsecret.so candidates=%d first=%p (round=%d)",
                          nb, reinterpret_cast<void *>(bases[0]), round);
        }
        for (int bi = 0; bi < nb && base == 0; ++bi) {
            uintptr_t d = 0, f = 0;
            const char *dt = "?";
            const char *ft = "?";
            const uintptr_t dfn = ResolveStateWord(
                    bases[bi], "SecShell_IsDirty", kIsDirtyCandidates,
                    sizeof(kIsDirtyCandidates) / sizeof(kIsDirtyCandidates[0]), &d, &dt);
            const uintptr_t ffn = ResolveStateWord(
                    bases[bi], "SecShell_FailedMask", kFailedMaskCandidates,
                    sizeof(kFailedMaskCandidates) / sizeof(kFailedMaskCandidates[0]), &f, &ft);
            if (dfn != 0 && ffn != 0 && AddrIsWritable(d) && AddrIsWritable(f)) {
                base = bases[bi];
                dirty = d;
                failed = f;
                dirty_tag = dt;
                failed_tag = ft;
                break;
            }
            if (round == 0 || round % 50 == 49) {   // 有界日志：首轮 + 每 1s 一次
                YSXZ_SEC_LOGW("candidate base=%p rejected (isDirty=%d failedMask=%d, round=%d)",
                               reinterpret_cast<void *>(bases[bi]), dfn != 0, ffn != 0, round);
            }
        }
        if (base == 0) {
            usleep(20 * 1000);
        }
    }
    if (base == 0) {
        YSXZ_SEC_LOGE("state word resolve failed after ~4s (candidates=%d); watchdog abort", seen);
        return nullptr;
    }
    YSXZ_SEC_LOGI("libsecret.so base=%p (candidates=%d)", reinterpret_cast<void *>(base), seen);
    checked = base + kCheckedRvaFallback;
    YSXZ_SEC_LOGI("state words dirty=%p(tag=%s) failed=%p(tag=%s) checked=%p",
                  reinterpret_cast<void *>(dirty), dirty_tag,
                  reinterpret_cast<void *>(failed), failed_tag,
                  reinterpret_cast<void *>(checked));

    if (!AddrIsWritable(dirty) || !AddrIsWritable(failed)) {
        YSXZ_SEC_LOGE("state words not writable; watchdog abort");
        return nullptr;
    }

    g_dirtyWord.store(dirty);
    g_failedWord.store(failed);
    g_checkedWord.store(checked);

    // 壳在 413~483ms 完成首轮自检，707ms / 3.87s 还会再检；前 6 秒用 1ms 粒度（对齐
    // Frida 成功实验），之后 20ms 长跑（同 DwdlmMod.cpp 的守卫线程策略）。
    int cleared = 0;
    int last_failed = -1;
    for (long i = 0; i < 120000; ++i) {
        volatile uint32_t *d = reinterpret_cast<volatile uint32_t *>(dirty);
        volatile uint32_t *f = reinterpret_cast<volatile uint32_t *>(failed);
        const int prev_f = static_cast<int>(*f);
        if (*d != 0) {
            *d = 0;
        }
        if (*f != 0) {
            *f = 0;
        }
        if (prev_f != 0 && prev_f != last_failed && cleared < 20) {
            last_failed = prev_f;
            ++cleared;
            YSXZ_SEC_LOGI("normalized failedMask=0x%x @iter=%ld", prev_f, i);
        }
        if (i == 6000) {
            YSXZ_SEC_LOGI("first 6s done; switch to 20ms interval");
        }
        usleep(i < 6000 ? 1000 : 20000);
    }
    YSXZ_SEC_LOGW("watchdog loop finished");
    return nullptr;
}

}  // namespace

void YsxzSecShell_Start() {
    if (g_started.exchange(true)) {
        return;
    }
    pthread_t tid;
    if (pthread_create(&tid, nullptr, WatchThread, nullptr) != 0) {
        g_started.store(false);
        YSXZ_SEC_LOGE("pthread_create failed");
        return;
    }
    pthread_detach(tid);
    YSXZ_SEC_LOGI("sec shell watchdog started");
}

// com.starry.magic.ysxz.YsxzAppComponentFactory.nativePrepareSecureShell()
extern "C" JNIEXPORT void JNICALL
Java_com_starry_magic_ysxz_YsxzAppComponentFactory_nativePrepareSecureShell(JNIEnv *, jclass) {
    YsxzSecShell_Start();
}
