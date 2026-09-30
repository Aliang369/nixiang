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

// 导出符号（Frida 实测 RVA，仅作 dlsym 失败时的兜底）
constexpr uintptr_t kIsDirtyRvaFallback = 0x4e5cc;   // SecShell_IsDirty
constexpr uintptr_t kFailedMaskRvaFallback = 0x4e664;  // SecShell_FailedMask

// 状态字 RVA（由导出函数序言的 adrp+ldr 反推；实测值作为兜底）
constexpr uintptr_t kDirtyRvaFallback = 0x64e98;
constexpr uintptr_t kCheckedRvaFallback = 0x64ea0;
constexpr uintptr_t kFailedRvaFallback = 0x64ea4;

std::atomic<bool> g_started{false};
std::atomic<uintptr_t> g_dirtyWord{0};
std::atomic<uintptr_t> g_failedWord{0};
std::atomic<uintptr_t> g_checkedWord{0};

// 扫描 /proc/self/maps，返回 libsecret.so 的主镜像基址。
// 选 file offset == 0 且**跨度最大**的那条映射：加固壳在 APKSecure 私有命名空间下
// 可能留下一个 dlopen 兜底副本，其映射比主镜像小，用最大跨度可稳定拿到主镜像。
uintptr_t FindSecLibBase() {
    FILE *fp = fopen("/proc/self/maps", "r");
    if (!fp) {
        return 0;
    }
    char line[512];
    uintptr_t best_base = 0;
    uintptr_t best_span = 0;
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
        const uintptr_t span = static_cast<uintptr_t>(end - start);
        if (span > best_span) {
            best_span = span;
            best_base = static_cast<uintptr_t>(start);
        }
    }
    fclose(fp);
    return best_base;
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

uintptr_t ResolveAccessor(uintptr_t base, const char *symbol, uintptr_t rva_fallback) {
    void *p = dlsym(RTLD_DEFAULT, symbol);
    if (!p) {
        // APKSecure 私有命名空间下 dlsym 可能看不到导出，退回基址 + 实测 RVA。
        p = reinterpret_cast<void *>(base + rva_fallback);
    }
    return reinterpret_cast<uintptr_t>(p);
}

void *WatchThread(void *) {
    uintptr_t base = 0;
    for (int i = 0; i < 2000 && !base; ++i) {  // 最多等 ~2s
        base = FindSecLibBase();
        if (!base) {
            usleep(10 * 1000);
        }
    }
    if (!base) {
        YSXZ_SEC_LOGW("libsecret.so not found; watchdog idle");
        return nullptr;
    }
    YSXZ_SEC_LOGI("libsecret.so base=%p", reinterpret_cast<void *>(base));

    uintptr_t dirty = 0, failed = 0, checked = 0;
    const uintptr_t is_dirty_fn = ResolveAccessor(base, "SecShell_IsDirty", kIsDirtyRvaFallback);
    const uintptr_t failed_fn = ResolveAccessor(base, "SecShell_FailedMask", kFailedMaskRvaFallback);
    const bool dec_dirty = DecodeGlobalWord(is_dirty_fn, &dirty);
    const bool dec_failed = DecodeGlobalWord(failed_fn, &failed);
    if (!dec_dirty) {
        dirty = base + kDirtyRvaFallback;
    }
    if (!dec_failed) {
        failed = base + kFailedRvaFallback;
    }
    checked = base + kCheckedRvaFallback;
    YSXZ_SEC_LOGI("state words dirty=%p(dec=%d) failed=%p(dec=%d) checked=%p",
                  reinterpret_cast<void *>(dirty), dec_dirty ? 1 : 0,
                  reinterpret_cast<void *>(failed), dec_failed ? 1 : 0,
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
