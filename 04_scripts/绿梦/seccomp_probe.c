
/* seccomp_probe.c — 绿梦/FairGuard 评估用 seccomp 机制真机验证器（只跑在独立进程，不碰游戏）
 * 模式：
 *   1  RET_LOG（看 audit/logcat 是否落 type=1326、字段有哪些）
 *   2  RET_TRAP + SIGSYS（看 pc/lr/x8，验证 pc+=4 后可继续）
 *   3  N 线程 + SECCOMP_FILTER_FLAG_TSYNC（验证跨线程覆盖与 rc 语义）
 *   4  tgkill 参数过滤（验证 args[1]/args[2] 匹配 + 信号未送达目标线程）
 * 编译（NDK 27）：
 *   aarch64-linux-android26-clang -O1 -fPIE -pie -o seccomp_probe seccomp_probe.c
 */
#define _GNU_SOURCE
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <errno.h>
#include <signal.h>
#include <stdint.h>
#include <pthread.h>
#include <sys/prctl.h>
#include <sys/syscall.h>
#include <linux/seccomp.h>
#include <linux/filter.h>
#include <linux/audit.h>
#include <stddef.h>

#define NR_TEST 173            /* getppid：无害，专用于观测 */

static volatile unsigned long g_hits;
static unsigned long g_pc, g_lr, g_sp, g_x0, g_x2, g_x8, g_tid;
static volatile int g_target_got_sig;
static volatile pid_t g_target_tid;
static unsigned long g_hit_tids[16]; static int g_nhit;

static unsigned long libc_base(void) {
    FILE* f = fopen("/proc/self/maps", "r"); char line[512]; unsigned long base = 0;
    while (f && fgets(line, sizeof line, f)) {
        if (strstr(line, "libc.so") && strstr(line, "r-xp")) {
            base = strtoul(line, NULL, 16); break;
        }
    }
    if (f) fclose(f); return base;
}
static void dump_status(const char* tag) {
    FILE* f = fopen("/proc/self/status", "r"); char line[256];
    while (f && fgets(line, sizeof line, f)) if (!strncmp(line, "Seccomp", 7)) printf("%s %s", tag, line);
    if (f) fclose(f);
}
static void onsigsys(int s, siginfo_t* si, void* uc_) {
    (void)s; (void)si;
    ucontext_t* uc = (ucontext_t*)uc_;
    unsigned long t = (unsigned long)syscall(SYS_gettid);
    if (g_nhit < 16) g_hit_tids[g_nhit++] = t;
    g_pc = uc->uc_mcontext.pc; g_lr = uc->uc_mcontext.regs[30];
    g_sp = uc->uc_mcontext.sp; g_x0 = uc->uc_mcontext.regs[0];
    g_x2 = uc->uc_mcontext.regs[2]; g_x8 = uc->uc_mcontext.regs[8];
    g_tid = t; g_hits++;
    uc->uc_mcontext.regs[0] = (uint64_t)-EPERM;   /* 拦下：系统调用不执行 */
    uc->uc_mcontext.pc += 4;                      /* 越过 svc，避免桩页后续字节 */
}
static void h_usr1(int s) { (void)s; g_target_got_sig = 1; }
static void* victim(void* a) {
    (void)a;
    g_target_tid = (pid_t)syscall(SYS_gettid);
    struct sigaction sa; memset(&sa, 0, sizeof sa); sa.sa_handler = h_usr1;
    sigaction(SIGUSR1, &sa, NULL);
    for (int i = 0; i < 50 && !g_target_got_sig; i++) usleep(10000);
    return NULL;
}
static void* worker(void* a) {
    (void)a; volatile long x = 0;
    for (int i = 0; i < 30; i++) { x += syscall(NR_TEST); usleep(5000); }
    (void)x; return NULL;
}
static long do_getppid(void) { return syscall(NR_TEST); }

int main(int argc, char** argv) {
    int mode = (argc > 1) ? atoi(argv[1]) : 1;
    int nthreads = (argc > 2) ? atoi(argv[2]) : 4;
    printf("mode=%d pid=%d tid=%lu libc_base=%#lx\n", mode, getpid(),
           (unsigned long)syscall(SYS_gettid), libc_base());
    dump_status("BEFORE");

    pthread_t th[16]; int nt = 0;
    if (mode == 3) { for (int i = 0; i < nthreads; i++) pthread_create(&th[nt++], NULL, worker, NULL); usleep(20000); }
    if (mode == 4) { pthread_create(&th[nt++], NULL, victim, NULL); while (!g_target_tid) usleep(1000); }

    struct sigaction sa; memset(&sa, 0, sizeof sa);
    sa.sa_sigaction = onsigsys; sa.sa_flags = SA_SIGINFO | SA_NODEFER; sigemptyset(&sa.sa_mask);
    if (mode != 1) sigaction(SIGSYS, &sa, NULL);

    struct sock_filter fl[12]; int n = 0;
    fl[n++] = (struct sock_filter)BPF_STMT(BPF_LD|BPF_W|BPF_ABS, offsetof(struct seccomp_data, arch));
    fl[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP|BPF_JEQ|BPF_K, AUDIT_ARCH_AARCH64, 1, 0);
    fl[n++] = (struct sock_filter)BPF_STMT(BPF_RET|BPF_K, SECCOMP_RET_ALLOW);
    fl[n++] = (struct sock_filter)BPF_STMT(BPF_LD|BPF_W|BPF_ABS, offsetof(struct seccomp_data, nr));
    int nr_off = n;
    fl[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP|BPF_JEQ|BPF_K, (mode == 4) ? 131 : NR_TEST, 0, 0);
    if (mode == 4) {
        fl[n++] = (struct sock_filter)BPF_STMT(BPF_LD|BPF_W|BPF_ABS, offsetof(struct seccomp_data, args[1]));
        fl[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP|BPF_JEQ|BPF_K, (uint32_t)g_target_tid, 0, 0);
        fl[n++] = (struct sock_filter)BPF_STMT(BPF_LD|BPF_W|BPF_ABS, offsetof(struct seccomp_data, args[2]));
        fl[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP|BPF_JEQ|BPF_K, SIGUSR1, 0, 0);
    }
    int act = n;
    fl[n++] = (struct sock_filter)BPF_STMT(BPF_RET|BPF_K, (mode == 1) ? SECCOMP_RET_LOG : SECCOMP_RET_TRAP);
    int allow = n;
    fl[n++] = (struct sock_filter)BPF_STMT(BPF_RET|BPF_K, SECCOMP_RET_ALLOW);
    fl[nr_off].jf = (unsigned char)(allow - nr_off - 1);
    if (mode == 4) { fl[nr_off+2].jf = (unsigned char)(allow - (nr_off+2) - 1);
                     fl[nr_off+4].jf = (unsigned char)(allow - (nr_off+4) - 1); }

    struct sock_fprog prog; prog.len = (unsigned short)n; prog.filter = fl;
    prctl(PR_SET_NO_NEW_PRIVS, 1, 0, 0, 0);
    errno = 0;
    long rc = syscall(__NR_seccomp, SECCOMP_SET_MODE_FILTER,
                      (mode == 3) ? SECCOMP_FILTER_FLAG_TSYNC : 0, &prog);
    printf("install rc=%ld errno=%d%s\n", rc, errno,
           (mode == 3) ? "  (TSYNC: rc>0 = 首个无法同步的 tid)" : "");
    dump_status("AFTER");

    if (mode == 4) {
        printf("tgkill(victim,0) -> %ld errno=%d\n", syscall(SYS_tgkill, getpid(), g_target_tid, 0), errno);
        printf("tgkill(victim,SIGUSR1) -> %ld errno=%d\n", syscall(SYS_tgkill, getpid(), g_target_tid, SIGUSR1), errno);
        pthread_join(th[0], NULL);
        printf("victim_got_sig=%d hits=%lu tid=%lu x0=%#lx x2=%#lx x8=%#lx pc=%#lx pc_off=%#lx lr=%#lx\n",
               g_target_got_sig, g_hits, g_tid, g_x0, g_x2, g_x8, g_pc, g_pc - libc_base(), g_lr);
    } else if (mode == 3) {
        for (int i = 0; i < nt; i++) pthread_join(th[i], NULL);
    } else {
        for (int i = 0; i < 3; i++) { errno = 0; long (*fp)(void) = do_getppid; printf("call%d -> %ld errno=%d\n", i, fp(), errno); }
        if (mode == 2) printf("hits=%lu pc=%#lx pc_off=%#lx lr=%#lx sp=%#lx x8=%#lx\n", g_hits, g_pc, g_pc - libc_base(), g_lr, g_sp, g_x8);
    }
    printf("hits=%lu tids:", g_hits);
    for (int i = 0; i < g_nhit && i < 8; i++) printf(" %lu", g_hit_tids[i]);
    printf("\nDONE\n");
    return 0;
}
