// ================================================================
//  LvmengCage.cpp — 绿梦「反自杀笼」（2026-10-27 新路线）
//
//  实测依据（本机雷电 14 / Android 14 / x86_64 + houdini 翻译，2026-10-27）：
//    · 官方签名包：冷启稳定，不杀；
//    · 内容逐字节相同、仅换签名：t≈12–47s 进程自己 exit_group(0) 干净退出
//      （Zygote: Process exited cleanly (0) / ApplicationExitInfo reason=1 EXIT_SELF），
//      无 tombstone、无 crash —— 即「自杀 syscall」，不是崩溃、不是外部信号；
//    · ptrace 外部验证（04_scripts/绿梦/fg_cage.c）：只把**非主线程**的 exit_group 改写掉，
//      进程活过 900s 观察窗、0 线程掉线、Unity 正常起（Product Name: Lumin）；
//      放手即死 ⇒ 判定方是常驻轮询，拦必须**常驻**。
//    · 杀人线程自称 `getprop`（伪装成系统线程名）；同轮 1726 次 tgkill 全是 sig=0（权限探测），
//      不是致命投递 ⇒ 过滤面可以做得极窄，不误伤 ART。
//
//  本模块只做一件事：**在 seccomp 层把 exit_group 变成"停住调用线程"，别的都不动**。
//    · 不碰 FairGuard 的任何代码/数据，不需要知道它检测了什么；
//    · 只改内存策略、不改文件 ⇒ 不触发它自己的文件完整性校验；
//    · SECCOMP_FILTER_FLAG_TSYNC ⇒ 一次性覆盖进程内**所有已存在线程**，之后新建线程继承；
//    · SIGSYS 处理器只 write() 落盘 + 永久挂起，不做格式化/malloc/加锁。
//
//  已知代价（必须写进交付说明）：游戏自己"退出"时也会被拦 ⇒ 正常退出会变成"卡住"，
//    用户请用 Home / 多任务划掉 / 强停。这是本路线唯一的功能性副作用。
//
//  开关：property debug.starry.lvmeng_cage = 0 关闭（默认开）。日志：logcat tag StarryLvmeng。
// ================================================================
#include <android/log.h>
#include <dlfcn.h>
#include <errno.h>
#include <fcntl.h>
#include <signal.h>
#include <stddef.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>
#include <sys/mman.h>
#include <sys/prctl.h>
#include <pthread.h>
#include <sys/syscall.h>
#include <sys/system_properties.h>
#include <time.h>
#include <unistd.h>

#include <linux/audit.h>
#include <linux/filter.h>
#include <linux/seccomp.h>

#include "dobby.h"

#define CAGE_TAG "StarryLvmeng"
#define CAGE_LOGI(...) __android_log_print(ANDROID_LOG_INFO, CAGE_TAG, __VA_ARGS__)
#define CAGE_LOGW(...) __android_log_print(ANDROID_LOG_WARN, CAGE_TAG, __VA_ARGS__)

#ifndef SECCOMP_FILTER_FLAG_TSYNC
#define SECCOMP_FILTER_FLAG_TSYNC 1
#endif
#ifndef SYS_SECCOMP
#define SYS_SECCOMP 1
#endif
#ifndef __NR_seccomp
#define __NR_seccomp 277 /* arm64 */
#endif

/* exit_group 在两个 ABI 下的编号（雷电里 arm64 so 经 houdini 翻译后内核看到的是 x86_64） */
#define NR_EXIT_GROUP_AARCH64 94
#define NR_EXIT_GROUP_X86_64 231

static int g_cage_fd = -1;
static volatile int g_cage_hits = 0;

/* ---- 落盘：信号上下文里只用 write()，不 malloc / 不 snprintf ---- */
static void cage_write_str(const char *s) {
    if (g_cage_fd >= 0 && s) {
        size_t n = 0;
        while (s[n]) n++;
        ssize_t r = write(g_cage_fd, s, n);
        (void)r;
    }
}

static void cage_write_dec(long v) {
    char b[24];
    int i = 0;
    if (v < 0) { cage_write_str("-"); v = -v; }
    if (v == 0) { cage_write_str("0"); return; }
    while (v > 0 && i < 20) { b[i++] = (char)('0' + (v % 10)); v /= 10; }
    while (i-- > 0) {
        ssize_t r = write(g_cage_fd, &b[i], 1);
        (void)r;
    }
}

/* 永久挂起当前线程：任何信号打断都继续睡，绝不返回调用点（返回就会落到 svc 后面的垃圾指令上） */
static void cage_park_forever(void) {
    struct timespec ts;
    ts.tv_sec = 3600;
    ts.tv_nsec = 0;
    for (;;) {
        syscall(SYS_nanosleep, &ts, NULL);
    }
}

/* the game process itself = cmdline equals the package name (no ":sub" suffix).
 * seccomp filters survive exec(), so every child we fork+exec (getprop / pm path /
 * logcat ...) inherits the filter; when such a child exits normally it hits
 * exit_group -> TRAP -> nobody parks it -> dies with signal 31 (measured 2026-10-28:
 * "Cmdline: getprop ro.product.cpu.abi"). Only the game process should park. */
static bool cage_is_game_process(void) {
    char b[160];
    memset(b, 0, sizeof(b));
    int fd = open("/proc/self/cmdline", O_RDONLY | O_CLOEXEC);
    if (fd < 0) return false;
    ssize_t n = read(fd, b, sizeof(b) - 1);
    close(fd);
    if (n <= 0) return false;
    if (strncmp(b, "com.zhuziplay.lvmengnew.op", 26) != 0) return false;
    return strchr(b, ':') == nullptr;   /* ":push" style subprocess -> not the game */
}

static void cage_sigsys_handler(int sig, siginfo_t *si, void *uc) {
    (void)sig;
    long nr = si ? (long)si->si_syscall : -1;
    if (nr == 94 || nr == 231 || nr == 93 || nr == 60) {
        if (cage_is_game_process()) {
            char line[128];
            snprintf(line, sizeof(line), "TRAP tid=%ld si_code=%d nr=%ld",
                     (long)syscall(SYS_gettid), si ? (int)si->si_code : -1, nr);
            CAGE_LOGW("[cage] %s", line);
            cage_write_str("[cage] ");
            cage_write_str(line);
            cage_write_str("\n");
            cage_park_forever();
        }
        /* helper child (exec'd shell tool): let it die the way it asked to, using the
         * untrapped exit syscall so we do not recurse into the filter. */
        long status = 0;
        if (uc) {
            ucontext_t *u = (ucontext_t *)uc;
            status = (long)u->uc_mcontext.regs[0];
        }
        syscall(SYS_exit, (int)status);
    }
    cage_park_forever();
}

static void cage_install_handler(void) {
    struct sigaction sa;
    memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = cage_sigsys_handler;
    sa.sa_flags = SA_SIGINFO | SA_NODEFER;
    sigemptyset(&sa.sa_mask);
    int rc = sigaction(SIGSYS, &sa, NULL);
    struct sigaction cur;
    memset(&cur, 0, sizeof(cur));
    sigaction(SIGSYS, NULL, &cur);
    CAGE_LOGI("cage: SIGSYS handler rc=%d now=%p (ours=%p)", rc, (void *)cur.sa_sigaction, (void *)cage_sigsys_handler);
}

/*
 * 两段独立过滤器（各自只认自己那个 arch，别的 arch 一律 ALLOW），叠加生效：
 *   x86_64  : 231(exit_group) / 60(exit)
 *   aarch64 : 94(exit_group) / 93(exit)
 * 说明：houdini 翻译环境里 guest 的 exit_group 实测不走可被 seccomp 看见的 syscall
 *      （2026-10-27 cagetest：arm64 版装完过滤器仍整进程退出，同源码 x86_64 原生版正确 TRAP），
 *      所以这一层是给**真机/原生环境**用的；翻译环境要靠 libc 钩子那一层。两层都装，互不冲突。
 */
#define CAGE_ARCH_X86_64 0xC000003EU
#define CAGE_ARCH_ARM64 0xC00000B7U
/* 2026-10-28 [lvmeng] 雷电(LDPlayer14) houdini 走 32 位 i386 ABI：exit_group=252，
 * 只匹配 x86_64(231)/arm64(94) 时整进程照样被 FG 带走（实测 33 s 干净退出）。 */
#define CAGE_ARCH_I386   0x40000003U
#define CAGE_MKFILTER(A, G1, G2) {                                            \
    BPF_STMT(BPF_LD | BPF_W | BPF_ABS, offsetof(struct seccomp_data, arch)),  \
    BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, (A), 0, 4),                           \
    BPF_STMT(BPF_LD | BPF_W | BPF_ABS, offsetof(struct seccomp_data, nr)),    \
    BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, (G1), 1, 0),                          \
    BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, (G2), 0, 1),                          \
    BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_TRAP),                              \
    BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW) }

/* ONLY exit_group: bionic implements *thread* exit with exit(60/93), so trapping
   exit() SIGSYS-kills every terminating thread (real device crash loop 2026-10-28). */
static struct sock_filter g_cage_fx64[] = CAGE_MKFILTER(CAGE_ARCH_X86_64, 231, 231);
static struct sock_filter g_cage_farm[] = CAGE_MKFILTER(CAGE_ARCH_ARM64, 94, 94);
static struct sock_filter g_cagefi386[] = CAGE_MKFILTER(CAGE_ARCH_I386, 252, 252);

static int cage_install_one(struct sock_filter *f, unsigned n, const char *tag) {
    struct sock_fprog prog;
    prog.len = (unsigned short)n;
    prog.filter = f;
    /* 只用 prctl：Android 14 上 zygote 已带 seccomp(filter)，再走 seccomp(2) TSYNC 会撞策略 trap
     * 直接 SIGSYS 崩（2026-10-28 真机实测 crash loop，signal 31）。prctl 是逐线程生效。 */
    errno = 0;
    if (prctl(PR_SET_SECCOMP, SECCOMP_MODE_FILTER, &prog) == 0) return 0;
    CAGE_LOGW("cage: %s prctl(SET_SECCOMP) failed errno=%d", tag, errno);
    return -1;
}

static int cage_install_filter(void) {
    if (prctl(PR_SET_NO_NEW_PRIVS, 1, 0, 0, 0) != 0) {
        CAGE_LOGW("cage: PR_SET_NO_NEW_PRIVS failed errno=%d", errno);
        return -1;
    }
    int rc = 0;
    rc |= cage_install_one(g_cage_fx64, (unsigned)(sizeof(g_cage_fx64) / sizeof(g_cage_fx64[0])), "x86_64");
    rc |= cage_install_one(g_cage_farm, (unsigned)(sizeof(g_cage_farm) / sizeof(g_cage_farm[0])), "aarch64");
    rc |= cage_install_one(g_cagefi386, (unsigned)(sizeof(g_cagefi386) / sizeof(g_cagefi386[0])), "i386");
    CAGE_LOGI("cage: per-thread filters installed rc=%d", rc);
    return rc;
}

/* 给每个新建线程装过滤器：钩 pthread_create，包一层启动函数 */
typedef void *(*pthread_fn)(void *);
struct CageStartArg { pthread_fn fn; void *arg; };
static CageStartArg g_startArgs[128];

static void *cage_thread_tramp(void *p) {
    CageStartArg *a = (CageStartArg *)p;
    pthread_fn fn = a->fn;
    void *arg = a->arg;
    a->fn = nullptr; a->arg = nullptr;
    cage_install_filter();     /* 该线程自己装一份（NNP 已在主线程设过，进程级生效） */
    return fn(arg);
}

static int (*o_pthread_create)(pthread_t *, const void *, pthread_fn, void *) = nullptr;
static int cage_my_pthread_create(pthread_t *t, const void *attr, pthread_fn fn, void *arg) {
    for (int i = 0; i < 128; i++) {
        if (g_startArgs[i].fn == nullptr) {
            g_startArgs[i].fn = fn;
            g_startArgs[i].arg = arg;
            int r = o_pthread_create(t, attr, cage_thread_tramp, &g_startArgs[i]);
            if (r != 0) { g_startArgs[i].fn = nullptr; g_startArgs[i].arg = nullptr; }
            return r;
        }
    }
    return o_pthread_create(t, attr, fn, arg);
}

static void cage_install_pthread_hook(void) {
    void *p = dlsym(RTLD_DEFAULT, "pthread_create");
    if (!p) { CAGE_LOGW("cage: pthread_create not found"); return; }
    int r = DobbyHook(p, (void *)cage_my_pthread_create, (void **)&o_pthread_create);
    CAGE_LOGI("cage: pthread_create hook rc=%d", r);
}

static int cage_read_self_int(const char *key) {
    /* 读 /proc/self/status 里的 Seccomp_filters / Seccomp / NoNewPrivs，用于自证"过滤器真的装上了" */
    int fd = open("/proc/self/status", O_RDONLY);
    if (fd < 0) return -1;
    char buf[4096];
    ssize_t n = read(fd, buf, sizeof(buf) - 1);
    close(fd);
    if (n <= 0) return -1;
    buf[n] = 0;
    char *p = strstr(buf, key);
    if (!p) return -1;
    p += strlen(key);
    while (*p == ':' || *p == ' ' || *p == '\t') p++;
    return atoi(p);
}

/* ---- 第三层：内联 svc 退出点扫描与就地挂住 ----------------------------------
 * 背景（2026-10-27 实测）：雷电用 houdini 把 arm64 翻译成 x86_64 执行，guest 的
 * exit_group 不走可被 seccomp 看见的通路（隔离实验：同代码 x86_64 原生版正确
 * TRAP，arm64 翻译版不触发），而 Dobby 的内存改写是有效的（fgspoof 的 FG
 * open(SPOOF) 已实测命中）。⇒ 直接改 guest 机器码：
 *   在 libFairGuard 的可执行映射里找「mov w8/x8,#94(exit_group) 或 #93(exit)」
 *   之后紧跟的 svc #0，把该 svc 改写成 b .（就地死循环）——那条线程就再也退不出去。
 * 只动 FairGuard 自己的退出点，不碰任何别的 so，也不改磁盘文件。
 * ------------------------------------------------------------------------- */
static int g_svc_patched = 0;
#ifndef CAGE_PATCH_ALL_SVC
#define CAGE_PATCH_ALL_SVC 0
#endif

static void cage_patch_svc(uint32_t *p, const char *tag, uint32_t *movsite, int nr) {
    long pg = (long)p & ~0xFFFL;
    if (mprotect((void *)pg, 0x2000, PROT_READ | PROT_WRITE | PROT_EXEC) != 0) {
        CAGE_LOGW("cage: mprotect failed at %p errno=%d", (void *)p, errno);
        return;
    }
    *p = 0x14000000u; /* b . */
    __builtin___clear_cache((char *)p, (char *)p + 4);
    CAGE_LOGW("cage: patched %s svc@%p <- mov#%d@%p", tag, (void *)p, nr, (void *)movsite);
    cage_write_str("[cage] patched svc site nr=");
    cage_write_dec(nr);
    cage_write_str("\n");
    g_svc_patched++;
}

static int cage_scan_range(unsigned long lo, unsigned long hi, const char *tag) {
    uint32_t *base = (uint32_t *)lo;
    unsigned long n = (hi - lo) / 4;
    int hits = 0;
    for (unsigned long i = 0; i + 2 < n; i++) {
        uint32_t w = base[i];
        int nr = -1;
        if (w == 0x52800BC8u || w == 0xD2800BC8u) nr = 94;      /* mov w8/x8, #94 */
        else if (w == 0x52800BA8u || w == 0xD2800BA8u) nr = 93;  /* mov w8/x8, #93 */
        else if (w == 0x52801028u || w == 0xD2801028u) nr = 129; /* mov w8/x8, #129 kill */
        else if (w == 0x52801068u || w == 0xD2801068u) nr = 131; /* mov w8/x8, #131 tgkill */
        if (nr < 0) continue;
        for (unsigned long j = i + 1; j <= i + 17 && j < n; j++) {
            if (base[j] == 0xD4000001u) {                        /* svc #0 */
                cage_patch_svc(&base[j], tag, &base[i], nr);
                hits++;
                break;
            }
        }
    }
    if (CAGE_PATCH_ALL_SVC && strstr(tag, "FairGuard")) {
        /* 兜底档：不依赖明文 syscall 编号，把 FairGuard 里所有 svc 指令一律改成 b . ，
         * 它的常驻判定线程会逐一冻在自己下一条 syscall 上。 */
        for (unsigned long j = 0; j < n; j++) {
            if (base[j] == 0xD4000001u) {
                cage_patch_svc(&base[j], tag, &base[j], -1);
                hits++;
            }
        }
    }
    return hits;
}

static unsigned long g_seen_lo[64];
static unsigned long g_seen_hi[64];
static int g_seen_n = 0;

static int cage_seen(unsigned long lo, unsigned long hi, int mark) {
    for (int i = 0; i < g_seen_n; i++) if (g_seen_lo[i] == lo && g_seen_hi[i] == hi) return 1;
    if (mark && g_seen_n < 64) { g_seen_lo[g_seen_n] = lo; g_seen_hi[g_seen_n] = hi; g_seen_n++; }
    return 0;
}

static int cage_scan_maps_once(void) {
    int total = 0, ranges = 0;
    FILE *f = fopen("/proc/self/maps", "r");
    if (!f) return 0;
    char line[512];
    while (fgets(line, sizeof(line), f)) {
        /* 第一阶段：扫所有 guest arm64 so（houdini 下它们的代码段是 rw-p 数据），
         * 不只 FairGuard —— 退出点可能落在别的库里。 */
        if (!strstr(line, "libFairGuard") && !strstr(line, "libil2cpp")) continue;
        unsigned long lo = 0, hi = 0;
        char perms[8] = {0};
        if (sscanf(line, "%lx-%lx %7s", &lo, &hi, perms) != 3) continue;
        /* 关键：houdini 下 guest so 的代码段是 **rw-p（数据）**，不是 r-xp；
         * 真正的可执行段是宿主侧的。所以这里只要求可读，不要求 x。 */
        if (perms[0] != 'r') continue;
        if (cage_seen(lo, hi, 0)) continue;   /* 每个区间只扫一次，避免 100+MB 库反复扫 */
        cage_seen(lo, hi, 1);
        ranges++;
        const char *nm = strrchr(line, '/');
        nm = nm ? nm + 1 : "lib";
        total += cage_scan_range(lo, hi, nm);
    }
    fclose(f);
    if (ranges == 0) {
        static int warned = 0;
        if (!warned) { warned = 1; cage_write_str("[cage] no libFairGuard rw range yet\n"); }
    }
    return total;
}

static void *cage_scan_worker(void *) {
    for (int i = 0; i < 120; i++) {          /* 60s 内每 500ms 一轮（覆盖晚映射/晚解密） */
        cage_scan_maps_once();
        usleep(500 * 1000);
    }
    CAGE_LOGI("cage: svc scan finished, patched=%d", g_svc_patched);
    return nullptr;
}

static void cage_start_scan(void) {
    pthread_t th;
    if (pthread_create(&th, nullptr, cage_scan_worker, nullptr) == 0) pthread_detach(th);
    else CAGE_LOGW("cage: scan thread create failed");
}

static void cage_open_log(void) {    /* app 私有目录（取证/复盘用）；失败不影响笼子本身 */
    const char *paths[] = {
        "/data/data/com.zhuziplay.lvmengnew.op/files/starry_cage.log",
        "/sdcard/Android/data/com.zhuziplay.lvmengnew.op/files/starry_cage.log",
    };
    for (unsigned i = 0; i < sizeof(paths) / sizeof(paths[0]); i++) {
        int fd = open(paths[i], O_WRONLY | O_CREAT | O_APPEND, 0600);
        if (fd >= 0) {
            g_cage_fd = fd;
            return;
        }
    }
}

/* SIGSYS 守护：FairGuard/CrashSight 会覆盖信号处理器（SIGILL 已被实测覆盖过），
 * 2026-10-28 真机实测：我们的 seccomp TRAP 打中了自杀 syscall，但 SIGSYS 处理器
 * 被顶掉 -> 进程以 signal 31 死。这里每 500ms 抢回一次。 */
static void *cage_sigsys_watchdog(void *) {
    for (;;) {
        struct sigaction cur;
        if (sigaction(SIGSYS, nullptr, &cur) == 0) {
            if ((void *)cur.sa_sigaction != (void *)cage_sigsys_handler) {
                cage_install_handler();
                CAGE_LOGW("cage: SIGSYS handler overwritten (now=%p) -> reclaimed", (void *)cur.sa_sigaction);
            }
        }
        usleep(500 * 1000);
    }
    return nullptr;
}
static void cage_start_sigsys_watchdog(void) {
    pthread_t th;
    if (pthread_create(&th, nullptr, cage_sigsys_watchdog, nullptr) == 0) pthread_detach(th);
    else CAGE_LOGW("cage: sigsys watchdog thread create failed");
}

/* ---- libc exit 家族钩子（第二层兜底） ---- */
static int  (*o_libc_exit_group)(int) = nullptr;
static void (*o_libc__exit)(int) = nullptr;
static void (*o_libc_exit_std)(int) = nullptr;
static void (*o_libc_Exit)(int) = nullptr;
static void (*o_libc_abort)(void) = nullptr;
static long (*o_libc_syscall)(long, long, long, long, long, long, long) = nullptr;

static void cage_park_log(const char *what) {
    cage_write_str("[cage] ");
    cage_write_str(what);
    cage_write_str(" hooked, parked\n");
    CAGE_LOGW("cage: %s hooked -> parked", what);
}
static int  cage_my_exit_group(int st) { (void)st; cage_park_log("exit_group"); cage_park_forever(); return 0; }
static void cage_my__exit(int st) { (void)st; cage_park_log("_exit"); cage_park_forever(); }
static void cage_my_exit_std(int st) { (void)st; cage_park_log("exit"); cage_park_forever(); }
static void cage_my_Exit(int st) { (void)st; cage_park_log("_Exit"); cage_park_forever(); }
static void cage_my_abort(void) { cage_park_log("abort"); cage_park_forever(); }
static long cage_my_syscall(long nr, long a1, long a2, long a3, long a4, long a5, long a6) {
    if (nr == 94 || nr == 93 || nr == 231 || nr == 60) { cage_park_log("syscall(exit)"); cage_park_forever(); }
    return o_libc_syscall(nr, a1, a2, a3, a4, a5, a6);
}

static void cage_install_libc_hooks(void) {
    void *p1 = dlsym(RTLD_DEFAULT, "exit_group");
    void *p2 = dlsym(RTLD_DEFAULT, "_exit");
    void *p3 = dlsym(RTLD_DEFAULT, "exit");
    void *p4 = dlsym(RTLD_DEFAULT, "_Exit");
    void *p5 = dlsym(RTLD_DEFAULT, "abort");
    void *p6 = dlsym(RTLD_DEFAULT, "syscall");
    if (p1) DobbyHook(p1, (void *)cage_my_exit_group, (void **)&o_libc_exit_group);
    if (p2) DobbyHook(p2, (void *)cage_my__exit, (void **)&o_libc__exit);
    if (p3) DobbyHook(p3, (void *)cage_my_exit_std, (void **)&o_libc_exit_std);
    if (p4) DobbyHook(p4, (void *)cage_my_Exit, (void **)&o_libc_Exit);
    if (p5) DobbyHook(p5, (void *)cage_my_abort, (void **)&o_libc_abort);
    if (p6) DobbyHook(p6, (void *)cage_my_syscall, (void **)&o_libc_syscall);
    CAGE_LOGI("cage: libc exit hooks done (eg=%p _exit=%p exit=%p abort=%p syscall=%p)", p1, p2, p3, p5, p6);
}

static void cage_start_once(void) {
    char v[PROP_VALUE_MAX] = {0};
    if (__system_property_get("debug.starry.lvmeng_cage", v) > 0 && v[0] == '0') {
        CAGE_LOGI("cage: disabled by property");
        return;
    }
    cage_open_log();
    cage_install_handler();
    int before = cage_read_self_int("Seccomp_filters");
    int mode_before = cage_read_self_int("Seccomp");
    if (cage_install_filter() == 0) {
        int after = cage_read_self_int("Seccomp_filters");
        int nnp = cage_read_self_int("NoNewPrivs");
        cage_write_str("[cage] installed (exit_group -> park)\n");
        CAGE_LOGI("cage: installed filters %d->%d (mode_before=%d nnp=%d) pid=%d",
                  before, after, mode_before, nnp, (int)getpid());
        cage_write_str("[cage] filters before/after = ");
        cage_write_dec(before);
        cage_write_str("/");
        cage_write_dec(after);
        cage_write_str(" nnp=");
        cage_write_dec(nnp);
        cage_write_str("\n");
    } else {
        cage_write_str("[cage] install failed\n");
    }
    cage_start_sigsys_watchdog();
    /* seccomp 过滤器由 fork/clone 继承，主线程装一次即可覆盖后续所有线程；
     * 钩 pthread_create 反而在 Android 14 上把 ART 线程创建搞坏（2026-10-28 真机 crash loop）。 */
    cage_install_libc_hooks();
}

/* so 被 dlopen 的那一刻就装：必须早于 FairGuard 的常驻判定线程（实测 12–47s 才发作） */
__attribute__((constructor)) static void Lvmeng_CageCtor(void) {
    static int once = 0;
    if (once) return;
    once = 1;
    cage_start_once();
}

extern "C" void LvmengCage_Install(void) { cage_start_once(); }
extern "C" int LvmengCage_Hits(void) { return g_cage_hits; }
