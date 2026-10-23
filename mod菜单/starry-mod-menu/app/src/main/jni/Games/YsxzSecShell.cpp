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

#define _GNU_SOURCE 1   /* process_vm_readv 在 bionic 里受 __USE_GNU 保护 */
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
#include <fcntl.h>
#include <stdlib.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <sys/uio.h>
#include <sys/syscall.h>

#include "Includes/Logger.h"
#include "Dobby/dobby.h"
#include <stdarg.h>

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

// ===================== 诊断：把内存里的匿名 dex 落到外部媒体目录 =====================
// 目的（2026-10-24）：壳把保护 payload（其中含 TapSDK 的类）解密后只在内存里以
// Anonymous-DexFile 装载，非 root 机上取不到。为了定位「TapSDK 到底从哪儿读签名」，
// 需要拿到它自己的代码。这里从进程自身的 /proc/self/mem 把「以 dex 魔数开头的可读段」
// 原样写到 /storage/emulated/0/Android/media/<pkg>/ysxz_dump/（adb shell 可读）。
// **仅在开关文件 yszx_dump_flag 存在时执行一次**；交付包不下发开关就不会触发。
static void YsxzReadPkgName(char *out, size_t n) {
    out[0] = '\0';
    FILE *f = fopen("/proc/self/cmdline", "rb");
    if (!f) return;
    size_t got = fread(out, 1, n - 1, f);
    fclose(f);
    out[got] = '\0';
    for (size_t i = 0; i < got; ++i) {
        if (out[i] == '\0') { out[i] = '\0'; break; }
    }
}

// 读自身进程内存：/proc/self/mem 在 Android 上被 SELinux 拒（EPERM 实测），
// process_vm_readv 对自身进程可用，且越界返回 EFAULT 而不会 SIGSEGV。
static ssize_t YsxzReadMem(void *dst, uintptr_t src, size_t n) {
    struct iovec local;
    struct iovec remote;
    local.iov_base = dst;
    local.iov_len = n;
    remote.iov_base = reinterpret_cast<void *>(src);
    remote.iov_len = n;
    /* 直接走 syscall：bionic 只在 API>=23 导出 process_vm_readv，本工程 APP_PLATFORM 更低 */
    return (ssize_t)syscall(__NR_process_vm_readv, getpid(), &local,
                            (unsigned long)1, &remote, (unsigned long)1,
                            (unsigned long)0);
}

// ===================== 诊断：谁在打开 APK（定位 TapSDK 读签名的路径）=====================
// 开关文件：/storage/emulated/0/Android/media/<pkg>/ysxz_trace_open
// 命中就打印「路径 + 调用者库/偏移(dladdr)」，用于判断 TapSDK 是自读 base.apk 还是走 Java PM。
static int (*g_origOpen)(const char *, int, ...) = nullptr;
static int (*g_origOpenat)(int, const char *, int, ...) = nullptr;
static std::atomic<int> g_openHits{0};

static void TraceWho(const char *tag, const char *path, void *caller) {
    if (!path) return;
    if (!strstr(path, ".apk") && !strstr(path, "apksecure")) return;
    const int n = ++g_openHits;
    if (n > 40) return;                       /* 封顶，绝不刷屏 */
    Dl_info info;
    memset(&info, 0, sizeof(info));
    const char *lib = "?";
    unsigned long off = 0;
    if (dladdr(caller, &info) && info.dli_fname) {
        lib = info.dli_fname;
        if (info.dli_fbase) off = (unsigned long)((uintptr_t)caller - (uintptr_t)info.dli_fbase);
    }
    YSXZ_SEC_LOGI("[open] %s %s  <- %s+0x%lx", tag, path, lib, off);
}

static int HookOpen(const char *path, int flags, ...) {
    mode_t mode = 0;
    if (flags & O_CREAT) {
        va_list ap;
        va_start(ap, flags);
        mode = (mode_t)va_arg(ap, int);
        va_end(ap);
        TraceWho("open", path, __builtin_return_address(0));
        return g_origOpen ? g_origOpen(path, flags, mode) : -1;
    }
    TraceWho("open", path, __builtin_return_address(0));
    return g_origOpen ? g_origOpen(path, flags) : -1;
}

static int HookOpenat(int dirfd, const char *path, int flags, ...) {
    mode_t mode = 0;
    if (flags & O_CREAT) {
        va_list ap;
        va_start(ap, flags);
        mode = (mode_t)va_arg(ap, int);
        va_end(ap);
        TraceWho("openat", path, __builtin_return_address(0));
        return g_origOpenat ? g_origOpenat(dirfd, path, flags, mode) : -1;
    }
    TraceWho("openat", path, __builtin_return_address(0));
    return g_origOpenat ? g_origOpenat(dirfd, path, flags) : -1;
}

static void YsxzTraceApkOpenMaybe() {
    char pkg[192] = {0};
    YsxzReadPkgName(pkg, sizeof(pkg));
    if (!pkg[0]) return;
    char flagPath[512];
    snprintf(flagPath, sizeof(flagPath),
             "/storage/emulated/0/Android/media/%s/ysxz_trace_open", pkg);
    if (access(flagPath, F_OK) != 0) return;
    void *p1 = nullptr, *p2 = nullptr;
    void *sym_open = dlsym(RTLD_DEFAULT, "open");
    void *sym_openat = dlsym(RTLD_DEFAULT, "openat");
    if (sym_open && DobbyHook(sym_open, (dobby_dummy_func_t)HookOpen, (dobby_dummy_func_t *)&g_origOpen) == 0) {
        p1 = sym_open;
    }
    if (sym_openat && DobbyHook(sym_openat, (dobby_dummy_func_t)HookOpenat, (dobby_dummy_func_t *)&g_origOpenat) == 0) {
        p2 = sym_openat;
    }
    YSXZ_SEC_LOGI("[open] trace armed open=%p openat=%p", p1, p2);
}

static uintptr_t g_dumpedAddr[64];
static int g_dumpedN = 0;

static bool YsxzAlreadyDumped(uintptr_t a) {
    for (int i = 0; i < g_dumpedN; ++i) if (g_dumpedAddr[i] == a) return true;
    return false;
}

static int YsxzDumpAnonDexOnce() {
    char pkg[192] = {0};
    YsxzReadPkgName(pkg, sizeof(pkg));
    if (!pkg[0]) return -1;
    char flagPath[512];
    snprintf(flagPath, sizeof(flagPath),
             "/storage/emulated/0/Android/media/%s/ysxz_dump_flag", pkg);
    if (access(flagPath, F_OK) != 0) return -1;          /* 开关不在 ⇒ 什么都不做 */
    char outDir[512];
    snprintf(outDir, sizeof(outDir), "/storage/emulated/0/Android/media/%s/ysxz_dump", pkg);
    mkdir(outDir, 0770);

    FILE *maps = fopen("/proc/self/maps", "r");
    if (!maps) { YSXZ_SEC_LOGE("dump: open maps failed"); return -1; }

    char line[512];
    int dumped = 0;
    while (fgets(line, sizeof(line), maps)) {
        unsigned long start = 0, end = 0;
        char perms[8] = {0};
        if (sscanf(line, "%lx-%lx %7s", &start, &end, perms) != 3) continue;
        if (perms[0] != 'r') continue;
        const unsigned long size = end - start;
        if (size < 4096UL || size > 96UL * 1024UL * 1024UL) continue;
        unsigned char magic[8] = {0};
        if (YsxzReadMem(magic, (uintptr_t)start, sizeof(magic)) != (ssize_t)sizeof(magic)) continue;
        /* dex 魔数："dex\n035\0" / "cdex001\0" */
        const bool isDex = (memcmp(magic, "dex\n", 4) == 0) || (memcmp(magic, "cdex", 4) == 0);
        if (!isDex) continue;
        if (YsxzAlreadyDumped((uintptr_t)start)) continue;
        char path[600];
        snprintf(path, sizeof(path), "%s/dex_%lx_%lx.dex", outDir, start, end);
        const int out = open(path, O_WRONLY | O_CREAT | O_TRUNC, 0666);
        if (out < 0) continue;
        unsigned long done = 0;
        char buf[65536];
        bool ok = true;
        while (done < size) {
            size_t want = sizeof(buf);
            if (size - done < want) want = (size_t)(size - done);
            ssize_t r = YsxzReadMem(buf, (uintptr_t)(start + done), want);
            if (r <= 0) { ok = false; break; }
            if (write(out, buf, (size_t)r) != r) { ok = false; break; }
            done += (unsigned long)r;
        }
        close(out);
        if (ok) {
            ++dumped;
            if (g_dumpedN < (int)(sizeof(g_dumpedAddr) / sizeof(g_dumpedAddr[0]))) {
                g_dumpedAddr[g_dumpedN++] = (uintptr_t)start;
            }
            YSXZ_SEC_LOGI("dump: %s (%lu bytes)", path, size);
        }
    }
    fclose(maps);
    YSXZ_SEC_LOGI("dump: done, %d dex region(s) -> %s", dumped, outDir);
    return dumped;
}

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

void *DumpPollThread(void *) {
    for (int i = 0; i < 90; ++i) {          /* 最多 ~3 分钟，每 2s 扫一遍 */
        const int n = YsxzDumpAnonDexOnce();
        if (n < 0) return nullptr;          /* 没有开关文件 ⇒ 立即退出 */
        if (g_dumpedN > 0 && i > 20 && n == 0) break;
        usleep(2 * 1000 * 1000);
    }
    return nullptr;
}

void *WatchThread(void *) {
    static std::atomic<bool> dumped{false};
    bool expect = false;
    if (dumped.compare_exchange_strong(expect, true)) {
        /* 诊断：壳的 payload dex 是启动后才载入内存的，单次扫描必然扑空 ⇒ 起轮询线程。
         * 无开关文件时第一轮就返回，代价可忽略。 */
        pthread_t dt;
        if (pthread_create(&dt, nullptr, DumpPollThread, nullptr) == 0) pthread_detach(dt);
        YsxzTraceApkOpenMaybe();   /* 诊断：无开关文件时立即返回 */
    }
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
    const char *dirty_tag = "?";
    const char *failed_tag = "?";
    const uintptr_t is_dirty_fn = ResolveStateWord(
            base, "SecShell_IsDirty", kIsDirtyCandidates,
            sizeof(kIsDirtyCandidates) / sizeof(kIsDirtyCandidates[0]), &dirty, &dirty_tag);
    const uintptr_t failed_fn = ResolveStateWord(
            base, "SecShell_FailedMask", kFailedMaskCandidates,
            sizeof(kFailedMaskCandidates) / sizeof(kFailedMaskCandidates[0]), &failed, &failed_tag);
    if (is_dirty_fn == 0 || failed_fn == 0) {
        // 拿不到可信状态字就什么都不写（凭旧 RVA 硬写会打到无关内存，比不净化更危险）。
        YSXZ_SEC_LOGE("state word resolve failed (isDirty=%d failedMask=%d); watchdog abort",
                      is_dirty_fn != 0, failed_fn != 0);
        return nullptr;
    }
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
