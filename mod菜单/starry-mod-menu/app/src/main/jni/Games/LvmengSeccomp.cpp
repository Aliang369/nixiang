/* ================================================================
 *  LvmengSeccomp.cpp — 绿梦 / FairGuard「tgkill 族」seccomp 诊断闸
 *  第一刀 RET_LOG：10_docs/绿梦/killer_pc_seccomp_A_20261020.md
 *  第二刀 mode 2 RET_TRAP + SIGSYS：见 10_docs/绿梦/killer_pc_seccomp_A2_20261020.md
 *
 *  实测背景：37.8 s（或 101–157 s）FairGuard 用 tgkill 给 UnityMain 送 SIGILL 造
 *  「伪造 native 崩溃」。RET_LOG 的 audit ip= 落在 libc（实测 libc+0xaffe4 <syscall>
 *  与 libc+0xef44c tgkill）⇒ 拿不到调用者。改 RET_TRAP 取寄存器：
 *  bionic 的 syscall()/tgkill() 都是叶子函数（无 stp x29,x30）⇒ 拦下时 x30(lr) 仍是
 *  调用者返回地址 = FairGuard/il2cpp 的调用点。
 *
 *  ★ 第一版 mode 2（全族 TRAP、就地 -EPERM）实测把游戏打死在 5.8 s（FairGuard 的启动
 *    期信号探针被 EPERM ⇒ 直接静默退出，连记录都没来得及 flush）⇒ 本版两点修正：
 *    ① 只拦 **sig == SIGILL(4)** 的投递（BPF 里校验 args 里的信号号），其余 tgkill
 *       原样放行 ⇒ 早期探针行为不变、游戏能活到 37.8 s；
 *    ② SIGSYS 处理器内**用预开 fd + write() 直接落盘**（纯手写 hex，不 snprintf、
 *       不 malloc、不 fopen）⇒ 即使进程 1 ms 后就被打死，记录也已经在文件里。
 *    ③ 轮询线程每 500 ms 快照一次 /proc/self/maps ⇒ 进程死了也能离线把 lr 归一成
 *       「模块+RVA」（ASLR 只改基址，不改库内布局）。
 *
 *  档位 prop debug.starry.lvmeng_tgkillguard：0/未设=不装；1=RET_LOG 全族记账；
 *    2=RET_TRAP 只拦 SIGILL 投递（取 lr/fp/sp/栈字，并把该次 tgkill 置 -EPERM）；
 *    5=il2cpp 影副本页写陷阱；6=perf 写断点；7=v2 块页 PROT_NONE；8=RET_LOG 记账
 *    「APK 尾段读取」（pread64/preadv/preadv2/lseek/mmap 的 offset>=256 MiB）。
 *  文件（app 私有，取证用）：
 *    files/starry_lvmeng_seccomp.log       命中记录（信号上下文 write 落盘）
 *    files/starry_lvmeng_maps_last.txt     最近一次 maps 快照（500 ms 刷新）
 *
 *  安全：信号上下文只写定长栈缓冲 + write()；格式化/归一/抢回 SIGSYS 处理器全在
 *    轮询线程；纯追加，不碰 SIGILL 吞信号 / fgspoof / StateMirror。
 * ================================================================ */
#include <android/log.h>
#include <errno.h>
#include <fcntl.h>
#include <signal.h>
#include <stddef.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ucontext.h>
#include <unistd.h>
#include <pthread.h>
#include <sys/prctl.h>
#include <sys/syscall.h>
#include <sys/system_properties.h>

#include <linux/audit.h>
#include <linux/filter.h>
#include <linux/seccomp.h>

#define LV_SE_TAG "StarryLvmeng"
#define SE_LOGI(...) __android_log_print(ANDROID_LOG_INFO,  LV_SE_TAG, __VA_ARGS__)
#define SE_LOGW(...) __android_log_print(ANDROID_LOG_WARN,  LV_SE_TAG, __VA_ARGS__)

#ifndef __NR_tgkill
#define __NR_tgkill               131
#endif
#ifndef __NR_tkill
#define __NR_tkill                130
#endif
#ifndef __NR_rt_tgsigqueueinfo
#define __NR_rt_tgsigqueueinfo    240
#endif
#ifndef __NR_kill
#define __NR_kill                 129
#endif
#ifndef __NR_pidfd_send_signal
#define __NR_pidfd_send_signal    424
#endif

static volatile int g_lvSeccompMode = 0;
static volatile int g_lvSeccompTried = 0;
static volatile int g_lvTsyncOk = 0;
static int g_trapMode = 0;   /* mode 5：页写陷阱（实现在文末） */
static void LvTrapTick(void);
static void LvArmSegv(void);
static void LvPerfTick(void);
static int g_perfMode;
static void LvV2Tick(void);
static volatile int g_v2Mode;
static void LvR9Flush(void);            /* mode 9 */
static volatile int g_r9Mode;
static struct sigaction g_sa9;          /* mode 9 的 SIGSYS 处理器（供轮询线程抢回） */
static volatile int g_hits = 0;

static const char* kSeclogPath = "/data/data/com.zhuziplay.lvmengnew.op/files/starry_lvmeng_seccomp.log";
static const char* kMapsPath   = "/data/data/com.zhuziplay.lvmengnew.op/files/starry_lvmeng_maps_last.txt";
static int g_fd = -1;

static const int kRecMax = 8;
static const int kStkW   = 16;
struct LvRec {
    uintptr_t pc, lr, fp, sp, tid, x0, x1, x2, x8;
    uintptr_t stk[kStkW];
};
static LvRec g_rec[kRecMax];
static volatile uint32_t g_seq[kRecMax];
static volatile uint32_t g_n = 0;
static volatile uint32_t g_flushed = 0;
static volatile int g_rearmed = 0;
static struct sigaction g_saSigsys;

/* ---- 异步信号安全的小工具：手写 hex / 定长串（不用 snprintf） ---- */
static char* lv_hex(char* p, uintptr_t v) {
    static const char d[] = "0123456789abcdef";
    *p++ = '0'; *p++ = 'x';
    int started = 0;
    for (int i = 15; i >= 0; --i) {
        const unsigned nib = (unsigned)((v >> (i * 4)) & 0xf);
        if (nib || started || i == 0) { *p++ = d[nib]; started = 1; }
    }
    return p;
}
static char* lv_str(char* p, const char* s) { while (*s) *p++ = *s++; return p; }

static void LvOnSigsys(int, siginfo_t*, void* uc_) {
    ucontext_t* uc = (ucontext_t*)uc_;
    const uintptr_t pc  = (uintptr_t)uc->uc_mcontext.pc;
    const uintptr_t lr  = (uintptr_t)uc->uc_mcontext.regs[30];
    const uintptr_t fp  = (uintptr_t)uc->uc_mcontext.regs[29];
    const uintptr_t sp  = (uintptr_t)uc->uc_mcontext.sp;
    const uintptr_t tid = (uintptr_t)syscall(__NR_gettid);
    const uintptr_t x0  = (uintptr_t)uc->uc_mcontext.regs[0];
    const uintptr_t x1  = (uintptr_t)uc->uc_mcontext.regs[1];
    const uintptr_t x2  = (uintptr_t)uc->uc_mcontext.regs[2];
    const uintptr_t x8  = (uintptr_t)uc->uc_mcontext.regs[8];
    const uintptr_t* stk = (const uintptr_t*)sp;
    uintptr_t words[kStkW];
    for (int k = 0; k < kStkW; ++k) words[k] = stk[k];

    const uint32_t i = __atomic_fetch_add(&g_n, 1, __ATOMIC_RELAXED);
    __atomic_fetch_add(&g_hits, 1, __ATOMIC_RELAXED);
    if (i < (uint32_t)kRecMax) {
        LvRec* r = &g_rec[i];
        r->pc = pc; r->lr = lr; r->fp = fp; r->sp = sp; r->tid = tid;
        r->x0 = x0; r->x1 = x1; r->x2 = x2; r->x8 = x8;
        for (int k = 0; k < kStkW; ++k) r->stk[k] = words[k];
        __atomic_store_n(&g_seq[i], i + 1, __ATOMIC_RELEASE);
    }

    const int fd = g_fd;
    if (fd >= 0) {
        char b[1024];
        char* p = b;
        p = lv_str(p, "HIT #"); p = lv_hex(p, (uintptr_t)(i + 1));
        p = lv_str(p, " tid="); p = lv_hex(p, tid);
        p = lv_str(p, " nr=");  p = lv_hex(p, x8);
        p = lv_str(p, " sig="); p = lv_hex(p, x2);
        p = lv_str(p, " lr=");  p = lv_hex(p, lr);
        p = lv_str(p, " pc=");  p = lv_hex(p, pc);
        p = lv_str(p, " fp=");  p = lv_hex(p, fp);
        p = lv_str(p, " sp=");  p = lv_hex(p, sp);
        p = lv_str(p, " x0=");  p = lv_hex(p, x0);
        p = lv_str(p, " x1=");  p = lv_hex(p, x1);
        p = lv_str(p, " x2=");  p = lv_hex(p, x2);
        p = lv_str(p, " stk=");
        for (int k = 0; k < kStkW && (p - b) < (int)sizeof(b) - 32; ++k) {
            p = lv_hex(p, words[k]);
            *p++ = ' ';
        }
        *p++ = 0x0a;
        ssize_t wr = write(fd, b, (size_t)(p - b));
        (void)wr;
    }
    uc->uc_mcontext.regs[0] = (uint64_t)(-EPERM);
}

static void LvResolve(uintptr_t pc, char* out, size_t cap) {
    out[0] = 0;
    FILE* f = fopen("/proc/self/maps", "re");
    if (!f) { snprintf(out, cap, "%p(maps NA)", (void*)pc); return; }
    char line[512];
    while (fgets(line, sizeof(line), f)) {
        unsigned long long s = 0, e = 0, off = 0; char perms[8] = {0};
        if (sscanf(line, "%llx-%llx %7s %llx", &s, &e, perms, &off) != 4) continue;
        if (pc < s || pc >= e) continue;
        const char* slash = strrchr(line, '/');
        if (slash) {
            char name[128];
            snprintf(name, sizeof(name), "%s", slash + 1);
            char* nl = strchr(name, '\n'); if (nl) *nl = 0;
            snprintf(out, cap, "%s+%#llx [%s @%#llx]", name,
                     (unsigned long long)(off + (pc - s)), perms, s);
        } else {
            snprintf(out, cap, "[anon]+%#llx [%s @%#llx]",
                     (unsigned long long)(pc - s), perms, s);
        }
        break;
    }
    fclose(f);
    if (!out[0]) snprintf(out, cap, "%p(no mapping)", (void*)pc);
}

static void LvReadComm(uintptr_t tid, char* out, size_t cap) {
    out[0] = 0;
    char p[96];
    snprintf(p, sizeof(p), "/proc/self/task/%lu/comm", (unsigned long)tid);
    FILE* f = fopen(p, "re");
    if (!f) return;
    if (fgets(out, (int)cap, f)) { char* nl = strchr(out, '\n'); if (nl) *nl = 0; }
    fclose(f);
}

static void LvFlush(const char* why) {
    const uint32_t n = __atomic_load_n(&g_n, __ATOMIC_RELAXED);
    const uint32_t f = __atomic_load_n(&g_flushed, __ATOMIC_RELAXED);
    static volatile int armedLogged = 0;
    if (n == f) {
        if (!armedLogged && g_lvTsyncOk) {
            armedLogged = 1;
            SE_LOGI("tgkill 闸 mode=%d：已就位（TSYNC 全线程），尚未命中", g_lvSeccompMode);
        }
        return;
    }
    const uint32_t cap = (n < (uint32_t)kRecMax) ? n : (uint32_t)kRecMax;
    for (uint32_t i = f; i < cap; i++) {
        if (__atomic_load_n(&g_seq[i], __ATOMIC_ACQUIRE) != i + 1) break;
        __atomic_store_n(&g_flushed, i + 1, __ATOMIC_RELAXED);
        const LvRec* r = &g_rec[i];
        char mpc[192] = {0}, mlr[192] = {0}, mfp[192] = {0}, comm[64] = {0};
        LvResolve(r->pc, mpc, sizeof(mpc));
        LvResolve(r->lr, mlr, sizeof(mlr));
        LvResolve(r->fp, mfp, sizeof(mfp));
        LvReadComm(r->tid, comm, sizeof(comm));
        SE_LOGW("==== %s tgkill 拦下 #%u tid=%lu comm=%s nr=%lu sig=%lu 累计=%d ====",
                why, i + 1, (unsigned long)r->tid, comm[0] ? comm : "?",
                (unsigned long)r->x8, (unsigned long)r->x2, g_hits);
        SE_LOGW("  CALLER lr(x30)=%p -> %s", (void*)r->lr, mlr);
        SE_LOGW("  stub pc=%p -> %s / fp(x29)=%p -> %s / sp=%p",
                (void*)r->pc, mpc, (void*)r->fp, mfp, (void*)r->sp);
        char buf[768]; int off = 0, shown = 0;
        buf[0] = 0;
        for (int k = 0; k < kStkW && shown < 10; ++k) {
            const uintptr_t v = r->stk[k];
            if (v < 0x1000) continue;
            char mm[192] = {0};
            LvResolve(v, mm, sizeof(mm));
            if (strstr(mm, "libc.so") || strstr(mm, "[anon]") || strstr(mm, "no mapping")) continue;
            const int w = snprintf(buf + off, sizeof(buf) - (size_t)off, " [+%d]%s", k * 8, mm);
            if (w <= 0 || off + w >= (int)sizeof(buf) - 8) break;
            off += w; shown++;
        }
        if (shown) SE_LOGW("  stack candidates:%s", buf);
        else       SE_LOGW("  stack: 16 words 全在 libc/[anon]");
    }
}

static void LvDumpMaps(void) {
    FILE* src = fopen("/proc/self/maps", "re");
    if (!src) return;
    FILE* dst = fopen(kMapsPath, "we");
    if (!dst) { fclose(src); return; }
    char buf[4096];
    size_t r;
    while ((r = fread(buf, 1, sizeof(buf), src)) > 0) fwrite(buf, 1, r, dst);
    fclose(dst);
    fclose(src);
}

static void* LvPollWorker(void*) {
    int tick = 0;
    for (int i = 0; i < 12000; i++) {          /* 12000 x 50 ms = 600 s */
        usleep(50 * 1000);
        if (g_lvSeccompMode >= 2 && (++tick % 10) == 0) {
            if (g_lvSeccompMode != 8) {         /* mode 8 是 RET_LOG：不装自定义 SIGSYS 处理器 */
                struct sigaction cur;
                struct sigaction* want = (g_lvSeccompMode == 9) ? &g_sa9 : &g_saSigsys;
                if (sigaction(SIGSYS, nullptr, &cur) == 0 &&
                    cur.sa_sigaction != want->sa_sigaction) {
                    __atomic_fetch_add(&g_rearmed, 1, __ATOMIC_RELAXED);
                    sigaction(SIGSYS, want, nullptr);
                    SE_LOGW("tgkill 闸：SIGSYS 处理器被覆盖(现=%p) -> 已抢回(第 %d 次)",
                            (void*)cur.sa_sigaction, g_rearmed);
                }
            }
            LvDumpMaps();                       /* 500 ms 一次 maps 快照，供死后归一 */
        }
        if (g_trapMode) { LvArmSegv(); LvTrapTick(); }
        if (g_perfMode) LvPerfTick();
        if (g_v2Mode) LvV2Tick();
        if (g_r9Mode) LvR9Flush();
        LvFlush("poll");
    }
    return nullptr;
}

static void LvStatus(const char* when) {
    FILE* f = fopen("/proc/self/status", "re");
    if (!f) return;
    char line[256], seccomp[64] = {0}, filters[64] = {0};
    while (fgets(line, sizeof(line), f)) {
        if (!strncmp(line, "Seccomp:", 8)) {
            char* p = line + 8; while (*p == ' ' || *p == '\t') p++;
            snprintf(seccomp, sizeof(seccomp), "%s", p);
        } else if (!strncmp(line, "Seccomp_filters:", 16)) {
            char* p = line + 16; while (*p == ' ' || *p == '\t') p++;
            snprintf(filters, sizeof(filters), "%s", p);
        }
    }
    fclose(f);
    char* nl;
    if ((nl = strchr(seccomp, '\n'))) *nl = 0;
    if ((nl = strchr(filters, '\n'))) *nl = 0;
    SE_LOGI("tgkill 闸 · %s Seccomp=%s Seccomp_filters=%s",
            when, seccomp[0] ? seccomp : "?", filters[0] ? filters : "?");
}

static int LvInstallOnce(void) {
    const int mode = g_lvSeccompMode;
    const uint32_t action = (mode >= 2) ? SECCOMP_RET_TRAP : SECCOMP_RET_LOG;

    if (mode >= 2) {
        char oldc[64] = {0};
        LvReadComm((uintptr_t)syscall(__NR_gettid), oldc, sizeof(oldc));
        memset(&g_saSigsys, 0, sizeof(g_saSigsys));
        g_saSigsys.sa_sigaction = LvOnSigsys;
        g_saSigsys.sa_flags = SA_SIGINFO | SA_NODEFER;
        sigemptyset(&g_saSigsys.sa_mask);
        if (sigaction(SIGSYS, &g_saSigsys, nullptr) != 0)
            SE_LOGW("tgkill 闸：装 SIGSYS 处理器失败 errno=%d（mode=2 会退化为杀进程）", errno);
        else
            SE_LOGI("tgkill 闸：SIGSYS 处理器已装（mode=2 取 lr/fp/栈字，SIGILL 投递就地 EPERM）");
    }

    if (prctl(PR_SET_NO_NEW_PRIVS, 1, 0, 0, 0) != 0)
        SE_LOGW("tgkill 闸：PR_SET_NO_NEW_PRIVS 失败 errno=%d（继续）", errno);

    static const int kNr[5]   = { 131, 130, 240, 129, 424 };
    static const int kArg[5]  = { 2, 1, 2, 1, 1 };
    struct sock_filter f[32];
    memset(f, 0, sizeof(f));
    int n = 0;
    f[n++] = (struct sock_filter)BPF_STMT(BPF_LD | BPF_W | BPF_ABS,
                                          offsetof(struct seccomp_data, arch));
    f[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, AUDIT_ARCH_AARCH64, 1, 0);
    f[n++] = (struct sock_filter)BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW);
    f[n++] = (struct sock_filter)BPF_STMT(BPF_LD | BPF_W | BPF_ABS,
                                          offsetof(struct seccomp_data, nr));
    for (int i = 0; i < 5; i++) {
        if (mode >= 2) {
            f[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K,
                                                  (uint32_t)kNr[i], 0, 3);
            f[n++] = (struct sock_filter)BPF_STMT(BPF_LD | BPF_W | BPF_ABS,
                                                  (uint32_t)(offsetof(struct seccomp_data, args) + 8 * kArg[i]));
            f[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 4u, 0, 1);
            f[n++] = (struct sock_filter)BPF_STMT(BPF_RET | BPF_K, action);
        } else {
            f[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K,
                                                  (uint32_t)kNr[i], 0, 1);
            f[n++] = (struct sock_filter)BPF_STMT(BPF_RET | BPF_K, action);
        }
    }
    f[n++] = (struct sock_filter)BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW);

    struct sock_fprog prog;
    prog.len = (unsigned short)n;
    prog.filter = f;

    errno = 0;
    long rc = syscall(__NR_seccomp, SECCOMP_SET_MODE_FILTER,
                      (unsigned long)SECCOMP_FILTER_FLAG_TSYNC, &prog);
    const int e1 = errno;
    if (rc == 0) {
        g_lvTsyncOk = 1;
        SE_LOGI("tgkill 闸已装：mode=%d %s · TSYNC 成功（全线程）· insns=%d",
                mode, (mode >= 2) ? "RET_TRAP(sig=SIGILL only, 取 lr/栈字)" : "RET_LOG(全族记账)",
                n);
        return 0;
    }
    SE_LOGW("tgkill 闸：TSYNC 失败 rc=%ld errno=%d ⇒ 退回单线程", rc, e1);
    errno = 0;
    long rc2 = syscall(__NR_seccomp, SECCOMP_SET_MODE_FILTER, 0UL, &prog);
    if (rc2 != 0) {
        SE_LOGW("tgkill 闸：单线程安装也失败 rc=%ld errno=%d ⇒ 放弃", rc2, errno);
        return -1;
    }
    SE_LOGI("tgkill 闸已装（仅本线程）mode=%d", mode);
    return 1;
}

static void* LvRetryWorker(void*) {
    for (int i = 1; i <= 6; i++) {
        usleep(300 * 1000);
        if (g_lvTsyncOk) return nullptr;
        if (LvInstallOnce() == 0) { SE_LOGI("tgkill 闸：重试 %d 次后 TSYNC 成功", i); return nullptr; }
    }
    return nullptr;
}

/* ================= mode 3：只观测、不改行为（Dobby 钩 libc syscall()/tgkill()） =================
 * 为什么还要这一档（实测结论驱动）：
 *   mode 2（RET_TRAP 拦 sig=SIGILL）两次冷启都只抓到**一条** tgkill(SIGILL)，调用者是
 *   libCrashSight.so+0xe300（崩溃上报器的 "Forwarding signal 4" 重新投递），而本轮真正的
 *   致命事件是 **真非法指令**（signal 4 code 1 ILL_ILLOPC，pc=libil2cpp+0x363b118）
 *   ⇒ 拦信号会让 FairGuard 改走「直接执行非法指令」这条路，等于把要抓的那条 tgkill 挡掉。
 *   所以需要一档**零行为变化**的观测：用 Dobby 钩 libc 的 syscall()/tgkill()，
 *   __builtin_return_address(0) 就是调用者（FairGuard/il2cpp）的返回地址。
 *   与 mode 1（RET_LOG）可叠加：seccomp 不装，只有钩子。
 * ============================================================================= */
#include <stdarg.h>
#include <dlfcn.h>
#include "Dobby/dobby.h"

static int   g_spyMode = 0;
static int   g_spyHits = 0;
static long (*g_origSyscall)(long, ...) = nullptr;
static int  (*g_origTgkill)(pid_t, pid_t, int) = nullptr;

static int LvSpyIsSigSyscall(long nr) {
    return nr == 131 || nr == 130 || nr == 240 || nr == 129 || nr == 424;
}

static void LvSpyLog(const char* api, long nr, long a0, long a1, long a2, void* ra) {
    const int n = __atomic_fetch_add(&g_spyHits, 1, __ATOMIC_RELAXED) + 1;
    if (n > 60) return;                       /* 只留前 60 条，避免刷爆 */
    char m[192] = {0};
    LvResolve((uintptr_t)ra, m, sizeof(m));
    char tidbuf[64] = {0};
    LvReadComm((uintptr_t)syscall(__NR_gettid), tidbuf, sizeof(tidbuf));
    SE_LOGW("SPY#%d %s(nr=%ld) caller=%p -> %s [caller_tid=%ld comm=%s] tgid=%ld tid=%ld sig=%ld",
            n, api, nr, ra, m, (long)syscall(__NR_gettid), tidbuf[0] ? tidbuf : "?",
            a0, a1, a2);
}

static long LvSpySyscall(long nr, ...) {
    va_list ap;
    va_start(ap, nr);
    const long a0 = va_arg(ap, long);
    const long a1 = va_arg(ap, long);
    const long a2 = va_arg(ap, long);
    const long a3 = va_arg(ap, long);
    const long a4 = va_arg(ap, long);
    const long a5 = va_arg(ap, long);
    va_end(ap);
    if (g_spyMode && LvSpyIsSigSyscall(nr))
        LvSpyLog("syscall", nr, a0, a1, a2, __builtin_return_address(0));
    return g_origSyscall ? g_origSyscall(nr, a0, a1, a2, a3, a4, a5) : -1;
}

static int LvSpyTgkill(pid_t tgid, pid_t tid, int sig) {
    /* 只记 SIGILL(4)：ART 线程挂起用 sig=24/30 的 tgkill，量极大（实测 8 s 内 60 条）会把
     * 记录名额吃光，把真正的伪造崩溃挤掉。 */
    if (g_spyMode && sig == SIGILL)
        LvSpyLog("tgkill", 131, (long)tgid, (long)tid, (long)sig, __builtin_return_address(0));
    return g_origTgkill ? g_origTgkill(tgid, tid, sig) : -1;
}

static int LvSpyInstall(void) {
    g_spyMode = 1;
    void* s1 = (void*)dlsym(RTLD_DEFAULT, "syscall");
    void* s2 = (void*)dlsym(RTLD_DEFAULT, "tgkill");
    int rc1 = s1 ? DobbyHook(s1, (void*)LvSpySyscall, (void**)&g_origSyscall) : -1;
    int rc2 = s2 ? DobbyHook(s2, (void*)LvSpyTgkill, (void**)&g_origTgkill) : -1;
    SE_LOGI("tgkill SPY（mode=3，零行为变化）：syscall hook rc=%d orig=%p / tgkill hook rc=%d orig=%p",
            rc1, (void*)g_origSyscall, rc2, (void*)g_origTgkill);
    return 3;
}

/* ============ mode 5：页写陷阱（in-process，无 ptrace、无 TracerPid） ============
 * 为什么用这招：Phase 1 证明 il2cpp 影副本在 t≈12–17 s 有一轮「每 3256 字节改一个 4 字节字」
 * 的滚动变换；静态镜像搜不到 0xcb8、perf 采样也分不出（变换太稀疏）。
 * 该段是 rwxp ⇒ 写入不需要 mprotect ⇒ 只能在**页权限**上设陷阱：把被改字所在页先
 * mprotect(PROT_READ|PROT_EXEC)，谁写谁吃 SIGSEGV，在 SIGSEGV 里记录 pc（=执行写入的那条
 * 指令）+lr+tid+si_addr，然后立刻恢复 rwxp 让写入完成；轮询线程每 200 ms 重新布防。
 * 风险：① 短暂改代码页权限，FairGuard 若校验页权限可能察觉；② SIGSEGV 处理器必须把
 * 「非本陷阱」的故障转交原处理器，否则会吃掉真崩溃。 */
#include <sys/mman.h>

static uintptr_t g_trapPage[8];
static uintptr_t g_trapBase = 0;
static volatile int g_selfTestDone = 0;
static int   g_trapN = 0;
static volatile int g_trapArmed = 0;
static volatile int g_trapHits = 0;
static struct { uintptr_t pc, lr, fp, tid, addr; } g_trapRec[16];
static struct sigaction g_oldSegv;
static volatile int g_hasOldSegv = 0;

static int LvIsTrapAddr(uintptr_t a) {
    for (int i = 0; i < g_trapN; i++)
        if (a >= g_trapPage[i] && a < g_trapPage[i] + 0x1000) return 1;
    return 0;
}

static void LvRearmTrap(void) {
    static int logged = 0;
    int rc0 = 0, rc1 = 0, rc2 = 0;
    for (int i = 0; i < g_trapN; i++) {
        const int rc = mprotect((void*)g_trapPage[i], 0x1000, PROT_READ | PROT_EXEC);
        if (i == 0) rc0 = rc; else if (i == 1) rc1 = rc; else rc2 = rc;
        if (rc != 0 && logged < 8) {
            SE_LOGW("页写陷阱：mprotect(#%d %p) 失败 errno=%d", i, (void*)g_trapPage[i], errno);
            logged++;
        }
    }
    if (logged == 0 && g_trapArmed == 0) {
        SE_LOGI("页写陷阱：布防生效 mprotect rc=%d/%d/%d（PROT_READ|PROT_EXEC）", rc0, rc1, rc2);
        logged = 1;
    }
    g_trapArmed = 1;
}

static void LvOnSegv(int sig, siginfo_t* si, void* uc_) {
    ucontext_t* uc = (ucontext_t*)uc_;
    const uintptr_t addr = (uintptr_t)(si ? si->si_addr : 0);
    if (!LvIsTrapAddr(addr)) {
        if (g_hasOldSegv && g_oldSegv.sa_sigaction && g_oldSegv.sa_sigaction != (void*)LvOnSegv)
            g_oldSegv.sa_sigaction(sig, si, uc_);
        return;
    }
    const uint32_t i = __atomic_fetch_add(&g_trapHits, 1, __ATOMIC_RELAXED);
    if (i < 16) {
        g_trapRec[i].pc   = (uintptr_t)uc->uc_mcontext.pc;
        g_trapRec[i].lr   = (uintptr_t)uc->uc_mcontext.regs[30];
        g_trapRec[i].fp   = (uintptr_t)uc->uc_mcontext.regs[29];
        g_trapRec[i].tid  = (uintptr_t)syscall(__NR_gettid);
        g_trapRec[i].addr = addr;
    }
    for (int k = 0; k < g_trapN; k++)
        mprotect((void*)g_trapPage[k], 0x1000, PROT_READ | PROT_WRITE | PROT_EXEC);
    g_trapArmed = 0;
}

/* 每次布防前重装 SIGSEGV：CrashSight 在 ~12 s 会覆盖我们的处理器（实测同一套 sa 顶掉 SIGSYS），
 * 不重装的话 12–17 s 变换窗口里陷阱实际失效。 */
static void LvArmSegv(void) {
    struct sigaction sa;
    memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = LvOnSegv;
    sa.sa_flags = SA_SIGINFO | SA_NODEFER | SA_RESTART;
    sigemptyset(&sa.sa_mask);
    sigaction(SIGSEGV, &sa, nullptr);
}

static int LvTrapInstall(void) {
    g_trapMode = 1;
    struct sigaction sa;
    memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = LvOnSegv;
    sa.sa_flags = SA_SIGINFO | SA_NODEFER | SA_RESTART;
    sigemptyset(&sa.sa_mask);
    if (sigaction(SIGSEGV, &sa, &g_oldSegv) == 0) g_hasOldSegv = 1;
    SE_LOGI("页写陷阱（mode=5）：SIGSEGV 处理器已装（原=%p），等 il2cpp 影副本基址",
            (void*)(g_hasOldSegv ? g_oldSegv.sa_sigaction : nullptr));
    return 5;
}

static void LvTrapTick(void) {
    if (g_trapN == 0) {
        FILE* f = fopen("/proc/self/maps", "re");
        if (!f) return;
        char line[512];
        uintptr_t base = 0;
        while (fgets(line, sizeof(line), f)) {
            if (!strstr(line, "libil2cpp.so") || !strstr(line, "rwxp")) continue;
            unsigned long long s = 0;
            if (sscanf(line, "%llx-", &s) == 1) base = (uintptr_t)s;
            break;
        }
        fclose(f);
        if (!base) return;
        /* ★位置纠正（2026-10-20）：Phase 1 的格点 0xa78/0x1730/0x23e8 是相对
         * 「窗口基址 base+0x3630000」的，真实地址要加回去 ⇒ 布防 base+0x3630000 起的三页。 */
        for (int i = 0; i < 8; i++)            /* 8 页：覆盖格点 0xa78…0x6fd0 的全部相位 */
            g_trapPage[i] = base + 0x3630000 + (uintptr_t)i * 0x1000;
        g_trapN = 8;
        g_trapBase = base;
        LvRearmTrap();
        SE_LOGI("页写陷阱：base=%p 布防 %p/%p/%p", (void*)base,
                (void*)g_trapPage[0], (void*)g_trapPage[1], (void*)g_trapPage[2]);
    }
    static int printed = 0;
    const int hits = g_trapHits;
    while (printed < hits && printed < 16) {
        const int i = printed++;
        char mpc[192] = {0}, mlr[192] = {0}, mfp[192] = {0}, comm[64] = {0};
        LvResolve(g_trapRec[i].pc, mpc, sizeof(mpc));
        LvResolve(g_trapRec[i].lr, mlr, sizeof(mlr));
        LvResolve(g_trapRec[i].fp, mfp, sizeof(mfp));
        LvReadComm(g_trapRec[i].tid, comm, sizeof(comm));
        SE_LOGW("==== 页写陷阱命中 #%d：tid=%lu comm=%s fault_addr=%p ====", i + 1,
                (unsigned long)g_trapRec[i].tid, comm[0] ? comm : "?", (void*)g_trapRec[i].addr);
        SE_LOGW("  WRITER pc=%p -> %s", (void*)g_trapRec[i].pc, mpc);
        SE_LOGW("         lr=%p -> %s / fp=%p -> %s", (void*)g_trapRec[i].lr, mlr,
                (void*)g_trapRec[i].fp, mfp);
    }
    if (!g_trapArmed && g_trapN > 0) LvRearmTrap();
    /* 自检：向第一个格点写一次同值（会 fault 一次），确认「机制 + 位置」都对。
     * 写之前再确认一次处理器是我们自己的，避免 fault 落到 CrashSight 手里变成真崩溃。 */
    if (g_trapN > 0 && !g_selfTestDone && g_trapBase) {
        struct sigaction cur;
        if (sigaction(SIGSEGV, nullptr, &cur) == 0 && cur.sa_sigaction != (void*)LvOnSegv)
            LvArmSegv();
        g_selfTestDone = 1;
        volatile uint32_t* t = (volatile uint32_t*)(g_trapBase + 0x3630000 + 0xa78);
        const uint32_t v = *t;
        *t = v;                                  /* 期望：SIGSEGV → 记录 → 恢复 RWX → 重试成功 */
        SE_LOGI("页写陷阱：自检写入 %p（同值 %08x）完成；若上面有 HIT 即为自检命中", (void*)t, v);
    }
}

/* ============ mode 6：in-process perf 硬件写断点（不碰页权限 ⇒ 不会逼停变换） ============
 * 背景：mode 5 用 mprotect(READ) 把写入者逼停了（8 页 3 轮零命中，而 prop=0 时 Phase 1 能观测到变换）。
 * 本档改用 perf_event_open(PERF_TYPE_BREAKPOINT, HW_BREAKPOINT_W, sigtrap=1)：内核在**写入发生后**
 * 投 SIGTRAP，页权限不变、变换照常跑；处理器记 pc/lr/tid。
 * 限制（写明白）：aarch64 只有 4 个硬件槽，perf 事件是 per-thread ⇒ 只能给有限线程布点，
 * 槽位会轮转（漏采）。所以先只打 1 个地址（base+0x3630a78）＋最多 32 个线程。 */
#include <linux/perf_event.h>
#include <linux/hw_breakpoint.h>
#include <dirent.h>

static volatile int g_perfArmed = 0;
static volatile int g_perfHits = 0;
static uintptr_t g_perfAddr = 0;
static struct { uintptr_t pc, lr, tid; } g_perfRec[16];
static int g_perfFds[64]; static pid_t g_perfTids[64]; static int g_perfN = 0;
static struct sigaction g_oldTrap;
static volatile int g_hasOldTrap = 0;

static void LvOnTrapPerf(int sig, siginfo_t* si, void* uc_) {
    ucontext_t* uc = (ucontext_t*)uc_;
    const uint32_t i = __atomic_fetch_add(&g_perfHits, 1, __ATOMIC_RELAXED);
    if (i < 16) {
        g_perfRec[i].pc  = (uintptr_t)uc->uc_mcontext.pc;   /* 写入之后的那条指令 */
        g_perfRec[i].lr  = (uintptr_t)uc->uc_mcontext.regs[30];
        g_perfRec[i].tid = (uintptr_t)syscall(__NR_gettid);
    }
    (void)sig; (void)si;
}

static long LvPerfOpen(uintptr_t addr, pid_t tid) {
    struct perf_event_attr a;
    memset(&a, 0, sizeof(a));
    a.type = PERF_TYPE_BREAKPOINT;
    a.size = sizeof(a);
    a.bp_type = HW_BREAKPOINT_W;
    a.bp_addr = addr;
    a.bp_len = HW_BREAKPOINT_LEN_4;
    a.sample_period = 1;
    a.sigtrap = 1;
    a.sig_data = (uint64_t)tid;
    a.disabled = 0;
    a.exclude_kernel = 1;
    a.exclude_hv = 1;
    return syscall(__NR_perf_event_open, &a, tid, -1, -1, 0UL);
}

static int LvPerfInstall(void) {
    g_perfMode = 1;
    struct sigaction sa;
    memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = LvOnTrapPerf;
    sa.sa_flags = SA_SIGINFO | SA_NODEFER | SA_RESTART;
    sigemptyset(&sa.sa_mask);
    if (sigaction(SIGTRAP, &sa, &g_oldTrap) == 0) g_hasOldTrap = 1;
    SE_LOGI("perf 写断点（mode=6）：SIGTRAP 处理器已装（原=%p），等 il2cpp 基址",
            (void*)(g_hasOldTrap ? g_oldTrap.sa_sigaction : nullptr));
    return 6;
}

/* 轮询线程调用：拿基址 → 对现有线程逐个开事件（限 32 个线程）→ 打印命中 */
static void LvPerfTick(void) {
    if (!g_perfArmed) {
        uintptr_t base = 0;
        FILE* f = fopen("/proc/self/maps", "re");
        if (!f) return;
        char line[512];
        while (fgets(line, sizeof(line), f)) {
            if (!strstr(line, "libil2cpp.so") || !strstr(line, "rwxp")) continue;
            unsigned long long s = 0;
            if (sscanf(line, "%llx-", &s) == 1) base = (uintptr_t)s;
            break;
        }
        fclose(f);
        if (!base) return;
        g_perfAddr = base + 0x3630a78;
        g_perfArmed = 1;
        int ok = 0, fail = 0;
        DIR* d = opendir("/proc/self/task");
        if (d) {
            struct dirent* e;
            while ((e = readdir(d)) && g_perfN < 32) {
                if (e->d_name[0] < '0' || e->d_name[0] > '9') continue;
                const pid_t tid = (pid_t)atoi(e->d_name);
                const long fd = LvPerfOpen(g_perfAddr, tid);
                if (fd >= 0) { g_perfFds[g_perfN] = (int)fd; g_perfTids[g_perfN] = tid; g_perfN++; ok++; }
                else fail++;
            }
            closedir(d);
        }
        SE_LOGI("perf 写断点：addr=%p 布点成功=%d 失败=%d（线程数=%d），槽位=4 会轮转",
                (void*)g_perfAddr, ok, fail, g_perfN);
    }
    static int printed = 0;
    const int hits = g_perfHits;
    while (printed < hits && printed < 16) {
        const int i = printed++;
        char mpc[192] = {0}, mlr[192] = {0}, comm[64] = {0};
        LvResolve(g_perfRec[i].pc, mpc, sizeof(mpc));
        LvResolve(g_perfRec[i].lr, mlr, sizeof(mlr));
        LvReadComm(g_perfRec[i].tid, comm, sizeof(comm));
        SE_LOGW("==== perf 写断点命中 #%d tid=%lu comm=%s ====", i + 1,
                (unsigned long)g_perfRec[i].tid, comm[0] ? comm : "?");
        SE_LOGW("  WRITER(after-store) pc=%p -> %s", (void*)g_perfRec[i].pc, mpc);
        SE_LOGW("                     lr=%p -> %s", (void*)g_perfRec[i].lr, mlr);
    }
}

/* ============ mode 7：v2 签名块页 PROT_NONE —— 抓 cert 比较的「读取者」 ============
 * 依据 §13.2：v2 签名块被直接映射在进程内（base.apk 的 r--s 映射，偏移 0xAB3C000 一带），
 * FairGuard 的 cert 比较直接读这段已映射内存（无 read syscall）⇒ 只能靠页权限设陷阱抓读取者。
 * 做法：对 v2 块所在页 mprotect(PROT_NONE) → 读取即 SIGSEGV → 记 pc（读指令）/lr/fp/tid/si_addr
 * → 立刻恢复 PROT_READ（r--s）让读取者重试成功 → 轮询线程重布防。非本页 fault 转交原处理器。 */
static uintptr_t g_v2Page = 0;
static volatile int g_v2Armed = 0;
static volatile int g_v2Hits = 0;
static struct { uintptr_t pc, lr, fp, tid, addr; } g_v2Rec[8];
static struct sigaction g_oldSegv7;
static volatile int g_hasOld7 = 0;
static volatile int g_v2SelfTest = 0;

static void LvOnSegvV2(int sig, siginfo_t* si, void* uc_) {
    ucontext_t* uc = (ucontext_t*)uc_;
    const uintptr_t a = (uintptr_t)(si ? si->si_addr : 0);
    if (!g_v2Page || a < g_v2Page || a >= g_v2Page + 0x1000) {
        if (g_hasOld7 && g_oldSegv7.sa_sigaction && g_oldSegv7.sa_sigaction != (void*)LvOnSegvV2)
            g_oldSegv7.sa_sigaction(sig, si, uc_);
        return;
    }
    const uint32_t i = __atomic_fetch_add(&g_v2Hits, 1, __ATOMIC_RELAXED);
    if (i < 8) {
        g_v2Rec[i].pc   = (uintptr_t)uc->uc_mcontext.pc;
        g_v2Rec[i].lr   = (uintptr_t)uc->uc_mcontext.regs[30];
        g_v2Rec[i].fp   = (uintptr_t)uc->uc_mcontext.regs[29];
        g_v2Rec[i].tid  = (uintptr_t)syscall(__NR_gettid);
        g_v2Rec[i].addr = a;
    }
    mprotect((void*)g_v2Page, 0x1000, PROT_READ);    /* v2 块原权限 = r--s */
    g_v2Armed = 0;
}

static void LvArmSegvV2(void) {
    struct sigaction sa;
    memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = LvOnSegvV2;
    sa.sa_flags = SA_SIGINFO | SA_NODEFER | SA_RESTART;
    sigemptyset(&sa.sa_mask);
    sigaction(SIGSEGV, &sa, nullptr);
}

static void LvV2Tick(void) {
    if (!g_v2Page) {
        FILE* f = fopen("/proc/self/maps", "re");
        if (!f) return;
        char line[512];
        uintptr_t base = 0;
        while (fgets(line, sizeof(line), f)) {
            if (!strstr(line, "base.apk")) continue;
            if (!strstr(line, "r--s")) continue;
            unsigned long long s = 0, off = 0;
            if (sscanf(line, "%llx-%*llx %*s %llx", &s, &off) == 2 && off == 0) { base = (uintptr_t)s; break; }
        }
        fclose(f);
        if (!base) return;
        g_v2Page = base + 0xab3c000;                 /* v2 块页（4K 对齐） */
        LvArmSegvV2();
        mprotect((void*)g_v2Page, 0x1000, PROT_NONE);
        g_v2Armed = 1;
        SE_LOGI("v2 块陷阱：apk_map_base=%p v2 页=%p 已 PROT_NONE", (void*)base, (void*)g_v2Page);
    }
    if (!g_v2SelfTest) {                              /* 自检：自己读一次该页 */
        LvArmSegvV2();
        g_v2SelfTest = 1;
        volatile const uint32_t* q = (volatile const uint32_t*)g_v2Page;
        const uint32_t v = *q;
        SE_LOGI("v2 块陷阱：自检读取 %p（值 %08x）完成", (void*)g_v2Page, v);
    }
    static int printed = 0;
    const int hits = g_v2Hits;
    while (printed < hits && printed < 8) {
        const int i = printed++;
        char mpc[192] = {0}, mlr[192] = {0}, comm[64] = {0};
        LvResolve(g_v2Rec[i].pc, mpc, sizeof(mpc));
        LvResolve(g_v2Rec[i].lr, mlr, sizeof(mlr));
        LvReadComm(g_v2Rec[i].tid, comm, sizeof(comm));
        SE_LOGW("==== v2 块读取命中 #%d tid=%lu comm=%s addr=%p ====", i + 1,
                (unsigned long)g_v2Rec[i].tid, comm[0] ? comm : "?", (void*)g_v2Rec[i].addr);
        SE_LOGW("  READER pc=%p -> %s", (void*)g_v2Rec[i].pc, mpc);
        SE_LOGW("         lr=%p -> %s / fp=%p", (void*)g_v2Rec[i].lr, mlr, (void*)g_v2Rec[i].fp);
    }
    if (!g_v2Armed && g_v2Page) { LvArmSegvV2(); mprotect((void*)g_v2Page, 0x1000, PROT_NONE); g_v2Armed = 1; }
}

/* ============ mode 8：RET_LOG 记账「APK 尾段读取」（零行为变化，只让内核 audit 记 ip） ============
 * 为什么 mode 7 在本包立不住（2026-10-20 05:02 实测，见 cert_reader_mode7_20261020.md）：
 *   安装包 371,367,646 B 的 v2 签名块在文件偏移 0x16214000、META-INF/STARRY.RSA 数据在
 *   0x1620B1EC，而进程里 base.apk 只有三段映射（0x07337000+0x21E8000 / 0x16215000+0x15000 /
 *   0x00ADE000 一页）⇒ 签名块与 v1 证书所在区间**根本没被映射**，PROT_NONE 页永远没有读者可抓。
 *   叠加 Stage 6/7 的结论「换 fd 后 FairGuard 源 libc read/pread 零日志」⇒ 读只能走自有 svc 桩，
 *   libc 钩子看不见，**只有 seccomp 这一层能看见它**。
 * 做法：对 pread64(67)/preadv(69)/preadv2(286)/lseek(62)/mmap(222) 做 RET_LOG，
 *   BPF 里先按「offset 参数 >= 256 MiB」过滤（本包条目区最大 0x0955F000 ⇒ 噪声≈0），
 *   命中只写内核 audit（type=1326），**不改任何寄存器/返回值** ⇒ 行为零变化；
 *   audit 里的 ip= 就是那条 svc 的地址 ⇒ 直接归一「模块+RVA」= 读签名块的代码点。
 * 判读：adb logcat -b events | grep type=1326，看 syscall=67/69/286 与崩溃时刻对齐的那条。
 * ========================================================================== */
#define LV_R8_TAIL_LO 0x10000000u          /* 256 MiB：本包条目区最大 0x0955F000，远小于它 */

static int LvInstallTailLog(void) {
    if (prctl(PR_SET_NO_NEW_PRIVS, 1, 0, 0, 0) != 0)
        SE_LOGW("尾段读取闸：PR_SET_NO_NEW_PRIVS 失败 errno=%d（继续）", errno);

    struct sock_filter f[96];
    memset(f, 0, sizeof(f));
    int n = 0;
    f[n++] = (struct sock_filter)BPF_STMT(BPF_LD | BPF_W | BPF_ABS,
                                          offsetof(struct seccomp_data, arch));
    f[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, AUDIT_ARCH_AARCH64, 1, 0);
    f[n++] = (struct sock_filter)BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW);
    f[n++] = (struct sock_filter)BPF_STMT(BPF_LD | BPF_W | BPF_ABS,
                                          offsetof(struct seccomp_data, nr));

    static const int kNr8[5]  = { 67, 69, 286, 62, 222 };   /* pread64/preadv/preadv2/lseek/mmap */
    static const int kArg8[5] = { 3,  3,  3,   1,   5 };    /* 64 位 offset 所在参数下标 */
    int patchIdx[8], np = 0;

    for (int i = 0; i < 5; i++) {
        const unsigned lo = (unsigned)(offsetof(struct seccomp_data, args) + 8 * (unsigned)kArg8[i]);
        f[n++] = (struct sock_filter)BPF_STMT(BPF_LD | BPF_W | BPF_ABS,
                                              offsetof(struct seccomp_data, nr));  /* 每组重载 A=nr */
        f[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, (uint32_t)kNr8[i], 0, 5);
        f[n++] = (struct sock_filter)BPF_STMT(BPF_LD | BPF_W | BPF_ABS, lo + 4);   /* offset 高 32 */
        patchIdx[np++] = n;                                                       /* jf -> ALLOW */
        f[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 0u, 0, 0);
        f[n++] = (struct sock_filter)BPF_STMT(BPF_LD | BPF_W | BPF_ABS, lo);      /* offset 低 32 */
        f[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP | BPF_JGE | BPF_K, LV_R8_TAIL_LO, 0, 1);
        f[n++] = (struct sock_filter)BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_LOG);
    }
    const int allowIdx = n;
    f[n++] = (struct sock_filter)BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW);
    for (int k = 0; k < np; k++) {
        f[patchIdx[k]].jf = (unsigned char)(allowIdx - (patchIdx[k] + 1));   /* hi!=0 ⇒ 放行 */
    }

    struct sock_fprog prog;
    prog.len = (unsigned short)n;
    prog.filter = f;

    errno = 0;
    long rc = syscall(__NR_seccomp, SECCOMP_SET_MODE_FILTER,
                      (unsigned long)SECCOMP_FILTER_FLAG_TSYNC, &prog);
    const int e1 = errno;
    if (rc == 0) { g_lvTsyncOk = 1; SE_LOGI("尾段读取闸：TSYNC 成功，insns=%d", n); return 0; }
    SE_LOGW("尾段读取闸：TSYNC 失败 rc=%ld errno=%d ⇒ 退回单线程", rc, e1);
    errno = 0;
    long rc2 = syscall(__NR_seccomp, SECCOMP_SET_MODE_FILTER, 0UL, &prog);
    if (rc2 != 0) { SE_LOGW("尾段读取闸：单线程也失败 errno=%d ⇒ 放弃", errno); return -1; }
    SE_LOGI("尾段读取闸：仅本线程安装成功 insns=%d", n);
    return 1;
}

/* ============ mode 9：透明 TRAP「APK 尾段 lseek/pread」→ 取调用者 lr/栈 ============
 * mode 8（RET_LOG）实测（05:07:27.607）：libFairGuard 的 ip（其自有 svc 桩 +0x35C608，
 * 桩体 = \"mov x8,#0x3e; svc #0\"）用 **offset>=256 MiB** 调了 3 次 lseek（comm=Thread-36），
 * 5 ms 后就是 FG open(base.apk)+0x11b5c4 ⇒ **FairGuard 确实在读 APK 尾段（签名块区）**，
 * 但 audit 只给 ip（=桩内 svc 的下一条），拿不到「谁调的桩」。
 * 本档把这几族改成 RET_TRAP：SIGSYS 处理器①先把 pc/lr/fp/sp/tid/16 个栈字记进内存环 +
 * 预开 fd 手写落盘；②再用 **safe 页内的裸 svc** 把该系统调用原样重做一次、把结果写回 x0
 * ⇒ 对被测进程**完全透明**（否则 RET_TRAP 会让 syscall 不执行、返回垃圾）。
 * safe 页：LvSafeSvc 按 4096 对齐，BPF 里对 instruction_pointer 做页内白名单，
 * 只放行这一页发出的 svc（否则处理器内的 svc 会被自己的过滤器再次拦下 → 递归）。
 * 判据：日志里 lr/stack 归一后落在 libFairGuard 的那条 = 读签名块的代码点。
 */
/* mode 9 的窗口收紧到 APK 尾段的最后 180 KB（本包 = 0x16200000..0x16229ede）：
 * 实心那 8 条 lseek(off=0x1422xxxx/0x161407e1) 全是 Unity 读 assets 的噪声，与签名无关。 */
/* 2026-10-20 06:1x：窗口下限必须同时覆盖**原厂包**（179 MB ⇒ 0x0AB3C000 一带）和现装包
 * （371 MB ⇒ 0x16200000 一带），否则「改写成原厂 fd」后读偏移落到 0x0AB3xxxx 会被窗口**滤掉**，
 * 表现为「0 条 FG 尾段读」——那是假阴性（06:09 组合轮实测）。 */
#define LV_R9_TAIL_LO 0x0AB00000u

static long __attribute__((noinline, aligned(4096), used))
LvSafeSvc(long nr, long a0, long a1, long a2, long a3, long a4, long a5) {
    register long x8 __asm__("x8") = nr;
    register long x0 __asm__("x0") = a0;
    register long x1 __asm__("x1") = a1;
    register long x2 __asm__("x2") = a2;
    register long x3 __asm__("x3") = a3;
    register long x4 __asm__("x4") = a4;
    register long x5 __asm__("x5") = a5;
    __asm__ volatile("svc #0"
                     : "+r"(x0)
                     : "r"(x8), "r"(x1), "r"(x2), "r"(x3), "r"(x4), "r"(x5)
                     : "memory");
    return x0;
}

/* 2026-10-20 06:0x（cert 根因路）：FairGuard 的 openat 走它自己的 svc 桩（0x35B100），
 * libc 钩子看不见 ⇒ 在 SIGSYS 处理器里**把路径参数改写成原厂包**，让它自己去按那份 179 MB 文件
 * 解析 EOCD/CD/签名块（偏移自然落到 0xAB3C000 一带），根本不用改 pread/lseek 的 offset。
 * 只改「lr 落在 libFairGuard 段内 + path 含 base.apk」的 openat；其余一律原样放行。 */
#define LV_R9_SPOOF_PATH "/data/data/com.zhuziplay.lvmengnew.op/files/fg_orig.apk"
static volatile uintptr_t g_r9FgLo = 0, g_r9FgHi = 0;
static volatile int g_r9SpoofReady = 0;
/* 2026-10-20 06:2x：溯源证明漏网的 32 条读，其 fd 来自 **libc 侧 open(base.apk)**
 * （open_lr=libc、含 base=1、未改写），不是 FG 段 ⇒ 增加「不限 lr」开关，prop 控制，
 * 默认关；开时对所有含 base.apk 的 openat 都改写（需实证不误伤 Unity/ART 的资源读）。 */
static volatile int g_r9ForceAll = 0;
static volatile int g_r9LogOpenat = 0;   /* prop debug.starry.lvmeng_openatlog=1 ⇒ 打 openat 重做结果 */
static volatile int g_r9OpenatRewrites = 0, g_r9OpenatSeen = 0;

/* fd 溯源环：记录「我们拦到的 openat」返回的 fd + 调用者 lr + 是否含 base.apk + 是否被改写，
 * 用于反查「32 条现装块读」的 fd 到底来自哪次 open、为什么没被改写。 */
/* 直接按 fd 号索引（原来用 128 条环存，被 8745 次 openat 挤掉 ⇒ 「不在表里」是假象，
 * 06:15 实测 1534 条读全部「不在拦截表」，其实是环被覆盖）。fd 号 < 1024 足够。 */
#define LV_FDMAX 1024
struct LvFdInfo { uintptr_t lr; int hasBase; int rewritten; int valid; };
static LvFdInfo g_fdInfo[LV_FDMAX];

static void LvRecordFd(int fd, uintptr_t lr, int hasBase, int rewritten) {
    if (fd < 0 || fd >= LV_FDMAX) return;
    g_fdInfo[fd].lr = lr; g_fdInfo[fd].hasBase = hasBase;
    g_fdInfo[fd].rewritten = rewritten; g_fdInfo[fd].valid = 1;
}

static int LvLookupFd(int fd, uintptr_t* lr, int* hasBase, int* rewritten) {
    if (fd < 0 || fd >= LV_FDMAX || !g_fdInfo[fd].valid) return 0;
    *lr = g_fdInfo[fd].lr; *hasBase = g_fdInfo[fd].hasBase;
    *rewritten = g_fdInfo[fd].rewritten;
    return 1;
}

/* 2026-10-21：libc 侧 openat 的改写判据从「无条件」改成「**调用栈里有 libFairGuard 帧**」。
 * 起因：全量改写把 Unity 的资源安装读也改到 179 MB 原厂包 ⇒ Unity 报
 * "Not enough storage space to install required resources"（设备实有 108 G，纯误报）。
 * 只认 FG 自己的调用栈，游戏/Unity 的 open 原样放行。 */
/* fp 链在 Unity/il2cpp 里常被省略（实测第一版 walking 全 0 ⇒ 一次都没改写、cert 直接不过），
 * 改成**扫栈字**：从 sp 起 256 个字里出现任何 libFairGuard 段内地址 ⇒ 认为调用栈含 FG 帧。
 * 栈一定已映射，不会像 fp 链那样踩到非法地址。 */
static int LvStackHasFgFrame(uintptr_t sp) {
    if (!g_r9FgLo || !sp || (sp & 0x7)) return 0;
    const uintptr_t* p = (const uintptr_t*)sp;
    /* 收紧到 64 字（512 B）：256 字 = 2 KB，Unity 初始化早期线程栈可能靠近栈顶/保护页，
     * 越界读会 SIGSEGV 秒崩（无 tombstone）。先小步，不够再放开。 */
    for (int i = 0; i < 64; ++i) {
        const uintptr_t v = p[i];
        if (v >= g_r9FgLo && v < g_r9FgHi) return 1;
    }
    return 0;
}

/* 受限长度子串查找（处理器内不能用 strstr 的不确定开销/越界风险） */
static int lv_has_sub(const char* s, const char* sub) {
    if (!s || !sub) return 0;
    const int n = (int)strlen(sub);
    for (int i = 0; i < 512 && s[i]; ++i) {
        int k = 0;
        while (k < n && s[i + k] && s[i + k] == sub[k]) ++k;
        if (k == n) return 1;
    }
    return 0;
}

static const int kR9Max = 2048;
struct LvR9Rec { uintptr_t kind, pc, lr, fp, sp, tid, nr, a0, a1, a2, a3; uintptr_t stk[16]; };
static LvR9Rec g_r9[kR9Max];
static volatile uint32_t g_r9seq[kR9Max];
static volatile uint32_t g_r9n = 0;    /* 命中总数（含不记录的 openat） */
static volatile uint32_t g_r9s = 0;    /* 实际入环的条数（无空洞，flush 用这个） */
static volatile uint32_t g_r9f = 0;
static volatile int g_r9FileLines = 0;
static volatile int g_r9Recursed = 0;
static __thread int t_r9In = 0;      /* 处理器重入哨：safe 页判定失手时退化成 -ENOSYS 而不是死循环 */

static void LvR9UpdateFg(void) {
    FILE* f = fopen("/proc/self/maps", "re");
    if (!f) return;
    char line[512];
    uintptr_t lo = 0, hi = 0;
    while (fgets(line, sizeof(line), f)) {
        if (!strstr(line, "libFairGuard.so")) continue;
        unsigned long long s = 0, e = 0;
        if (sscanf(line, "%llx-%llx", &s, &e) != 2) continue;
        if (!lo || (uintptr_t)s < lo) lo = (uintptr_t)s;
        if ((uintptr_t)e > hi) hi = (uintptr_t)e;
    }
    fclose(f);
    if (lo) { g_r9FgLo = lo; g_r9FgHi = hi; }
}

static void LvOnSigsys9(int sig, siginfo_t* si, void* uc_) {
    (void)sig; (void)si;
    ucontext_t* uc = (ucontext_t*)uc_;
    const uintptr_t pc  = (uintptr_t)uc->uc_mcontext.pc;
    const uintptr_t lr  = (uintptr_t)uc->uc_mcontext.regs[30];
    const uintptr_t fp  = (uintptr_t)uc->uc_mcontext.regs[29];
    const uintptr_t sp  = (uintptr_t)uc->uc_mcontext.sp;
    const uintptr_t nr  = (uintptr_t)uc->uc_mcontext.regs[8];
    const uintptr_t tid = (uintptr_t)syscall(__NR_gettid);

    const int isSig = (nr == 131 || nr == 130 || nr == 240 ||
                       nr == 129 || nr == 138 || nr == 424);   /* 信号族全谱 */
    /* 极简档（openatall=8）：只把 openat 原样重做，不读路径、不用 TLS 哨兵、不写日志
     * —— 用来分清「TRAP+重做机制」还是「改写/扫描逻辑」。 */
    if (g_r9ForceAll == 8 && nr == 56) {
        const long rc0 = LvSafeSvc((long)nr,
                                   (long)uc->uc_mcontext.regs[0], (long)uc->uc_mcontext.regs[1],
                                   (long)uc->uc_mcontext.regs[2], (long)uc->uc_mcontext.regs[3],
                                   (long)uc->uc_mcontext.regs[4], (long)uc->uc_mcontext.regs[5]);
        uc->uc_mcontext.regs[0] = (uint64_t)rc0;
        return;
    }
    /* kind：0=其它(不打印) 1=尾段读 2=信号族 3=openat 改写 */
    uintptr_t kind = 0;
    int openatBase = 0, openatRewrote = 0;
    if (nr == 56) {                            /* openat：**唯一被拦的系统调用** */
        const char* path = (const char*)uc->uc_mcontext.regs[1];
        __atomic_fetch_add(&g_r9OpenatSeen, 1, __ATOMIC_RELAXED);
        openatBase = lv_has_sub(path, "base.apk");
        const int lrInFg = (g_r9FgLo && lr >= g_r9FgLo && lr < g_r9FgHi);
        /* g_r9ForceAll 新含义：**允许用调用栈判定** libc 侧 openat（默认开）；
         * 设 0 ⇒ 只认直接 lr∈FG（会漏掉 libc 侧那条，cert 可能不过）。 */
        const int stackInFg = (g_r9ForceAll == 1 && !lrInFg) ? LvStackHasFgFrame(sp) : 0;
        const int rewriteAll = (g_r9ForceAll == 2);   /* 回退：无条件全部改写（会误伤 Unity） */
        const int noRewrite  = (g_r9ForceAll == 9);   /* 诊断档：只 TRAP 绝不改写 */
        if (!noRewrite && g_r9SpoofReady && path && openatBase &&
            (lrInFg || stackInFg || rewriteAll)) {
            uc->uc_mcontext.regs[1] = (uint64_t)(uintptr_t)LV_R9_SPOOF_PATH;
            __atomic_fetch_add(&g_r9OpenatRewrites, 1, __ATOMIC_RELAXED);
            kind = 3;
            openatRewrote = 1;
        }
    }
    __atomic_fetch_add(&g_r9n, 1, __ATOMIC_RELAXED);
    /* 只把「关心的」入环：否则 9602 条 openat 会把环灌满、flush 直接卡在空洞上（本轮实测） */
    const uint32_t i = kind ? __atomic_fetch_add(&g_r9s, 1, __ATOMIC_RELAXED) : 0;
    if (kind && i < (uint32_t)kR9Max) {
        LvR9Rec* r = &g_r9[i];
        r->kind = kind;
        r->pc = pc; r->lr = lr; r->fp = fp; r->sp = sp; r->tid = tid; r->nr = nr;
        r->a0 = (uintptr_t)uc->uc_mcontext.regs[0];
        r->a1 = (uintptr_t)uc->uc_mcontext.regs[1];
        r->a2 = (uintptr_t)uc->uc_mcontext.regs[2];
        r->a3 = (uintptr_t)uc->uc_mcontext.regs[3];
        const uintptr_t* stk = (const uintptr_t*)sp;
        for (int k = 0; k < 16; ++k) r->stk[k] = stk[k];
        __atomic_store_n(&g_r9seq[i], i + 1, __ATOMIC_RELEASE);
    }
    const int fd = g_fd;
    if (kind && fd >= 0 && g_r9FileLines < 256) {
        g_r9FileLines++;
        char b[512];
        char* p = b;
        p = lv_str(p, "T9 #");
        p = lv_hex(p, (uintptr_t)(i + 1));
        p = lv_str(p, " tid="); p = lv_hex(p, tid);
        p = lv_str(p, " nr=");  p = lv_hex(p, nr);
        p = lv_str(p, " lr=");  p = lv_hex(p, lr);
        p = lv_str(p, " pc=");  p = lv_hex(p, pc);
        p = lv_str(p, " fp=");  p = lv_hex(p, fp);
        p = lv_str(p, " a0=");  p = lv_hex(p, (uintptr_t)uc->uc_mcontext.regs[0]);
        p = lv_str(p, " a1=");  p = lv_hex(p, (uintptr_t)uc->uc_mcontext.regs[1]);
        p = lv_str(p, " a2=");  p = lv_hex(p, (uintptr_t)uc->uc_mcontext.regs[2]);
        p = lv_str(p, " a3=");  p = lv_hex(p, (uintptr_t)uc->uc_mcontext.regs[3]);
        *p++ = 0x0a;
        ssize_t wr = write(fd, b, (size_t)(p - b)); (void)wr;
    }
    /* 透明化：用 safe 页的裸 svc 原样重做该系统调用（带重入哨） */
    if (t_r9In) {
        __atomic_fetch_add(&g_r9Recursed, 1, __ATOMIC_RELAXED);
        uc->uc_mcontext.regs[0] = (uint64_t)(-ENOSYS);
        return;
    }
    t_r9In = 1;
    const long rc = LvSafeSvc((long)nr,
                              (long)uc->uc_mcontext.regs[0], (long)uc->uc_mcontext.regs[1],
                              (long)uc->uc_mcontext.regs[2], (long)uc->uc_mcontext.regs[3],
                              (long)uc->uc_mcontext.regs[4], (long)uc->uc_mcontext.regs[5]);
    t_r9In = 0;
    uc->uc_mcontext.regs[0] = (uint64_t)rc;
    if (nr == 56 && rc >= 0) LvRecordFd((int)rc, lr, openatBase, openatRewrote);
    if (nr == 56 && g_r9LogOpenat) {
        static volatile int sLogN = 0;
        if (rc < 0 || sLogN < 400) {
            __atomic_fetch_add(&sLogN, 1, __ATOMIC_RELAXED);
            SE_LOGW("openat rc=%ld lr=%#lx 改写=%d base=%d tid=%lu",
                    rc, (unsigned long)lr, openatRewrote, openatBase,
                    (unsigned long)syscall(__NR_gettid));
        }
    }
}

static int LvInstallTailTrap(void) {
    memset(&g_sa9, 0, sizeof(g_sa9));
    g_sa9.sa_sigaction = LvOnSigsys9;
    g_sa9.sa_flags = SA_SIGINFO | SA_NODEFER | SA_RESTART;
    sigemptyset(&g_sa9.sa_mask);
    /* 每 50 ms 抢回一次（CrashSight 会覆盖 SIGSYS） */
    sigaction(SIGSYS, &g_sa9, nullptr);

    const uintptr_t safeLo = (uintptr_t)&LvSafeSvc;
    const uintptr_t safeHi = safeLo + 0xFFFu;
    if ((safeLo >> 32) != (safeHi >> 32)) {
        SE_LOGW("尾段 TRAP：safe 页跨 4 GiB 边界（%p）⇒ 放弃本档以免递归", (void*)safeLo);
        return -3;
    }
    /* openat 改写的前提：原厂包就位 */
    {
        char v[PROP_VALUE_MAX] = {0};
        /* 交付化：未设 prop ⇒ 默认**全部改写**（含 libc 侧 open），显式设 0 才退回「仅 FG 段」 */
        /* 0=仅 lr∈FG；1（默认）=lr∈FG 或**调用栈含 FG 帧**（收窄，不碰 Unity）；2=无条件全部改写 */
        const int vn = __system_property_get("debug.starry.lvmeng_openatall", v);
        g_r9ForceAll = (vn > 0) ? atoi(v) : 1;
        char v2[PROP_VALUE_MAX] = {0};
        if (__system_property_get("debug.starry.lvmeng_openatlog", v2) > 0 && v2[0] == '1')
            g_r9LogOpenat = 1;
        SE_LOGI("openat 改写范围：%s", g_r9ForceAll ? "全部 base.apk（含 libc 侧 open）"
                                                     : "仅 lr∈libFairGuard");
    }
    if (access(LV_R9_SPOOF_PATH, R_OK) == 0) {
        g_r9SpoofReady = 1;
        SE_LOGI("openat 改写：原厂包就位 %s（只改 lr∈libFairGuard 的 base.apk open）",
                LV_R9_SPOOF_PATH);
    } else {
        SE_LOGW("openat 改写：缺原厂包 %s ⇒ 只观测不改写", LV_R9_SPOOF_PATH);
    }
    /* 装过滤器前先自检 safe svc：能正常发 syscall 才继续 */
    {
        const long st = LvSafeSvc(172 /*getpid*/, 0, 0, 0, 0, 0, 0);
        SE_LOGI("尾段 TRAP：safe svc 自检 getpid=%ld（本进程 %ld）safe 页=%p",
                st, (long)getpid(), (void*)safeLo);
    }
    if (prctl(PR_SET_NO_NEW_PRIVS, 1, 0, 0, 0) != 0)
        SE_LOGW("尾段 TRAP：PR_SET_NO_NEW_PRIVS 失败 errno=%d（继续）", errno);

    struct sock_filter f[96];
    memset(f, 0, sizeof(f));
    int n = 0;
    int patchIdx[16]; int np = 0;
    f[n++] = (struct sock_filter)BPF_STMT(BPF_LD | BPF_W | BPF_ABS,
                                          offsetof(struct seccomp_data, arch));
    f[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, AUDIT_ARCH_AARCH64, 1, 0);
    f[n++] = (struct sock_filter)BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW);
    f[n++] = (struct sock_filter)BPF_STMT(BPF_LD | BPF_W | BPF_ABS,
                                          offsetof(struct seccomp_data, nr));
    /* --- safe 页白名单：instruction_pointer ∈ [safeLo, safeHi] → ALLOW ---
     * 注意跳转方向：ip 不在本页 ⇒ **继续走 nr 判定**（不是 ALLOW！第一版写反了 ⇒ 全部放行、零命中） */
    f[n++] = (struct sock_filter)BPF_STMT(BPF_LD | BPF_W | BPF_ABS,
                                          (uint32_t)(offsetof(struct seccomp_data, instruction_pointer) + 4));
    f[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, (uint32_t)(safeLo >> 32), 0, 4);
    f[n++] = (struct sock_filter)BPF_STMT(BPF_LD | BPF_W | BPF_ABS,
                                          (uint32_t)offsetof(struct seccomp_data, instruction_pointer));
    f[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP | BPF_JGE | BPF_K, (uint32_t)safeLo, 0, 2);
    f[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP | BPF_JGT | BPF_K, (uint32_t)safeHi, 1, 0);
    f[n++] = (struct sock_filter)BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW);

    /* 2026-10-21：**只拦 openat(56)**。
     * 摘掉的理由（委托方实测）：mode 9 还在 TRAP 网络线程（comm=NetworkKit_GRS_）的 pread64(67)，
     * 被当成「尾段读取命中」并且要经过 safe 页重做 ⇒ 登录/闪屏走网络时读数据被搞坏 ⇒ 闪退。
     * 尾段读捕获只为「验证 FG 读原厂块」，已由 2038 条证据确认，不再需要实时捕获；
     * 信号族（129/130/131/138/240/424）也一并摘掉（同理：TRAP+safe 重做对线上流程是风险）。
     * ⇒ 现在过滤器只做一件事：拦 openat 并（按需）改写路径。 */
    f[n++] = (struct sock_filter)BPF_STMT(BPF_LD | BPF_W | BPF_ABS,
                                          offsetof(struct seccomp_data, nr));
    f[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 56u /* openat */, 0, 1);
    f[n++] = (struct sock_filter)BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_TRAP);
    const int allowIdx = n;
    f[n++] = (struct sock_filter)BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW);
    /* 修补「offset 高 32 != 0 → ALLOW」的 jf 跳转（ip 白名单已自带 ALLOW，无跳转需补） */
    {
        static const int kIsJt[16] = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        for (int k = 0; k < np; k++) {
            const int off = allowIdx - (patchIdx[k] + 1);
            if (kIsJt[k]) f[patchIdx[k]].jt = (unsigned char)off;
            else          f[patchIdx[k]].jf = (unsigned char)off;
        }
    }

    struct sock_fprog prog;
    prog.len = (unsigned short)n;
    prog.filter = f;
    errno = 0;
    long rc = syscall(__NR_seccomp, SECCOMP_SET_MODE_FILTER,
                      (unsigned long)SECCOMP_FILTER_FLAG_TSYNC, &prog);
    const int e1 = errno;
    if (rc == 0) { g_lvTsyncOk = 1; SE_LOGI("openat 闸：TSYNC 成功 insns=%d safe=%p..%p（只拦 openat）", n, (void*)safeLo, (void*)safeHi); return 0; }
    SE_LOGW("尾段 TRAP：TSYNC 失败 rc=%ld errno=%d ⇒ 退回单线程", rc, e1);
    errno = 0;
    long rc2 = syscall(__NR_seccomp, SECCOMP_SET_MODE_FILTER, 0UL, &prog);
    if (rc2 != 0) { SE_LOGW("尾段 TRAP：单线程也失败 errno=%d ⇒ 放弃", errno); return -1; }
    SE_LOGI("尾段 TRAP：仅本线程安装 insns=%d", n);
    return 1;
}

static void LvR9Flush(void) {
    static int fgTick = 0;
    if (!g_r9FgLo && (++fgTick % 10) == 0) LvR9UpdateFg();   /* 500 ms 找一次 FG 段 */
    const uint32_t n = __atomic_load_n(&g_r9s, __ATOMIC_RELAXED);
    while (g_r9f < n && g_r9f < (uint32_t)kR9Max) {
        const uint32_t i = g_r9f;
        if (__atomic_load_n(&g_r9seq[i], __ATOMIC_ACQUIRE) != i + 1) break;
        g_r9f = i + 1;
        const LvR9Rec* r = &g_r9[i];
        if (r->kind == 0) continue;            /* 不关心的 syscall：不打印 */
        char mpc[192] = {0}, mlr[192] = {0}, mfp[192] = {0}, comm[48] = {0};
        LvResolve(r->pc, mpc, sizeof(mpc));
        LvResolve(r->lr, mlr, sizeof(mlr));
        LvResolve(r->fp, mfp, sizeof(mfp));
        LvReadComm(r->tid, comm, sizeof(comm));
        const int fg = strstr(mpc, "FairGuard") || strstr(mlr, "FairGuard") ||
                       strstr(mfp, "FairGuard");
        const unsigned long long offArg = (r->nr == 62)
            ? (unsigned long long)r->a1      /* lseek(fd, off, whence) */
            : (unsigned long long)r->a3;     /* pread64/preadv/preadv2(…, off, …) */
        /* 信号号：tgkill(tgid,tid,sig)→x2 / tkill(tid,sig)→x1 / kill(pid,sig)→x1
         * rt_sigqueueinfo(pid,sig,uinfo)→x1 / pidfd_send_signal(fd,sig,…)→x1 */
        const unsigned long long sigArg = (r->nr == 131 || r->nr == 130)
            ? (unsigned long long)r->a2 : (unsigned long long)r->a1;
        const int isIll = (r->nr >= 128 && r->nr <= 131) || r->nr == 138 || r->nr == 240 ||
                          r->nr == 424;
        const char* kindTag = (r->kind == 3) ? "openat 改写" :
                              (isIll ? "信号命中" : "尾段读取命中");
        if (r->kind == 1) {
            uintptr_t olr = 0; int ob = -1, orw = -1;
            const int fdFound = LvLookupFd((int)r->a0, &olr, &ob, &orw);
            SE_LOGW("   └ fd=%d 溯源=%s open_lr=%#lx 含base=%d 已改写=%d",
                    (int)r->a0, fdFound ? "命中" : "不在拦截表(dup/继承/libc侧)",
                    (unsigned long)olr, ob, orw);
        }
        SE_LOGW("==== %s#%u%s tid=%lu comm=%s nr=%lu sig=%llu off=%#llx cnt=%lu ====",
                kindTag, (unsigned)(i + 1), fg ? "[FG]" : "",
                (unsigned long)r->tid, comm[0] ? comm : "?", (unsigned long)r->nr,
                isIll ? sigArg : 0ULL, offArg, (unsigned long)r->a2);
        SE_LOGW("  CALLER lr(x30)=%p -> %s", (void*)r->lr, mlr);
        SE_LOGW("  stub pc=%p -> %s / fp=%p -> %s / fd=%lu", (void*)r->pc, mpc, (void*)r->fp, mfp,
                (unsigned long)r->a0);
        char buf[768]; int off = 0, shown = 0; buf[0] = 0;
        for (int k = 0; k < 16 && shown < 10; ++k) {
            const uintptr_t v = r->stk[k];
            if (v < 0x1000) continue;
            char mm[192] = {0};
            LvResolve(v, mm, sizeof(mm));
            if (strstr(mm, "libc.so") || strstr(mm, "no mapping")) continue;
            const int w = snprintf(buf + off, sizeof(buf) - (size_t)off, " [+%d]%s", k * 8, mm);
            if (w <= 0 || off + w >= (int)sizeof(buf) - 8) break;
            off += w; shown++;
        }
        if (shown) SE_LOGW("  stack candidates:%s", buf);
    }
    if (g_r9Recursed) SE_LOGW("尾段 TRAP：重入哨触发 %d 次（safe 页判定失手）", g_r9Recursed);
    if (g_r9OpenatRewrites)
        SE_LOGW("openat 改写累计：%d 次（看到的 openat 总数 %d）",
                g_r9OpenatRewrites, g_r9OpenatSeen);
}

int Lvmeng_InstallTgkillGuard(void) {
    if (g_lvSeccompMode >= 1) return g_lvSeccompMode;
    if (g_lvSeccompTried) return g_lvSeccompMode;
    g_lvSeccompTried = 1;

    char buf[PROP_VALUE_MAX] = {0};
    const int pn = __system_property_get("debug.starry.lvmeng_tgkillguard", buf);
    /* 2026-10-20 06:3x 交付化：**未设 prop ⇒ 默认 mode 9**（openat 改写要常开才能保护玩家），
     * 显式设 0 才关闭（调试/回滚用）。 */
    /* 2026-10-21 05:0x 实测回归：装上 seccomp 过滤器后进程 1–2 s 内被杀（新旧两版同表现，
     * 且 fgspoof=0/1 都一样；关掉过滤器则活 40 s+）⇒ FairGuard 现在会检测「多出的 seccomp filter」。
     * ⇒ **默认不装 seccomp**（mode=0），cert 伪装退回 libc 层（fgspoof）。要复现旧行为设 prop=9。 */
    int mode = (pn > 0) ? atoi(buf) : 0;
    if (mode <= 0) { SE_LOGI("tgkill 闸：显式关闭（prop=%s）", buf); return 0; }
    if (mode > 9) { SE_LOGW("tgkill 闸：prop=%d 越界 ⇒ 按 9 执行", mode); mode = 9; }
    if (mode == 9) {                     /* 透明 TRAP 尾段读取：取调用者 lr/栈 */
        g_lvSeccompMode = 9;
        g_r9Mode = 1;
        g_fd = open(kSeclogPath, O_WRONLY | O_CREAT | O_APPEND | O_CLOEXEC, 0600);
        LvStatus("装前");
        const int r9 = LvInstallTailTrap();
        LvStatus("装后");
        pthread_t th9;
        if (pthread_create(&th9, nullptr, LvPollWorker, nullptr) == 0) pthread_detach(th9);
        SE_LOGI("尾段 TRAP（mode=9）：透明重做 + 取 lr/16 栈字（rc=%d）· 命中落盘 %s",
                r9, kSeclogPath);
        return 9;
    }
    if (mode == 8) {                     /* RET_LOG 记账 APK 尾段读取（零行为变化） */
        g_lvSeccompMode = 8;
        LvStatus("装前");
        const int r8 = LvInstallTailLog();
        LvStatus("装后");
        pthread_t th8;
        if (pthread_create(&th8, nullptr, LvPollWorker, nullptr) == 0) pthread_detach(th8);
        SE_LOGI("尾段读取闸（mode=8）：RET_LOG 只记 offset>=0x%x 的 pread64/preadv/preadv2/lseek/mmap（rc=%d，0=TSYNC）· maps 快照 -> %s",
                LV_R8_TAIL_LO, r8, kMapsPath);
        return 8;
    }
    if (mode == 7) {                     /* v2 签名块页 PROT_NONE：抓 cert 比较读取者 */
        g_lvSeccompMode = 7;
        g_v2Mode = 1;
        pid_t unused = 0; (void)unused;
        pthread_t th7;
        if (pthread_create(&th7, nullptr, LvPollWorker, nullptr) == 0) pthread_detach(th7);
        SE_LOGI("v2 块陷阱（mode=7）：已起轮询线程，等 base.apk r--s 映射");
        return 7;
    }
    if (mode == 6) {                     /* perf 硬件写断点：抓滚动变换的写入者 */
        g_lvSeccompMode = 6;
        g_perfMode = 1;
        pthread_t th6;
        LvPerfInstall();
        if (pthread_create(&th6, nullptr, LvPollWorker, nullptr) == 0) pthread_detach(th6);
        return 6;
    }
    if (mode == 3) {                     /* 只观测：不装 seccomp，只挂 Dobby 探针 */
        g_lvSeccompMode = 3;
        return LvSpyInstall();
    }
    if (mode == 5) {                     /* 页写陷阱：抓滚动变换的执行者 */
        g_lvSeccompMode = 5;
        LvTrapInstall();
        pthread_t th5;
        if (pthread_create(&th5, nullptr, LvPollWorker, nullptr) == 0) pthread_detach(th5);
        return 5;
    }
    g_lvSeccompMode = mode;

    if (mode >= 2) {
        g_fd = open(kSeclogPath, O_WRONLY | O_CREAT | O_APPEND | O_CLOEXEC, 0600);
        if (g_fd < 0) SE_LOGW("tgkill 闸：命中日志打不开 errno=%d（只剩 logcat 一路）", errno);
        else           SE_LOGI("tgkill 闸：命中日志 fd=%d -> %s", g_fd, kSeclogPath);
    }

    LvStatus("装前");
    const int r = LvInstallOnce();
    LvStatus("装后");

    pthread_t th;
    if (pthread_create(&th, nullptr, LvPollWorker, nullptr) == 0) pthread_detach(th);
    if (r > 0) {
        pthread_t th2;
        if (pthread_create(&th2, nullptr, LvRetryWorker, nullptr) == 0) pthread_detach(th2);
    }
    SE_LOGI("tgkill 闸：prop=%s mode=%d 结果=%d", pn > 0 ? buf : "(unset)", mode, r);
    return g_lvSeccompMode;
}

__attribute__((constructor)) static void Lvmeng_SeccompCtor() {
    Lvmeng_InstallTgkillGuard();
}
