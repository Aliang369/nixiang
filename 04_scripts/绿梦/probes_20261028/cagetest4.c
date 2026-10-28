#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <signal.h>
#include <errno.h>
#include <sys/prctl.h>
#include <sys/syscall.h>
#include <linux/audit.h>
#include <linux/filter.h>
#include <linux/seccomp.h>

#define ARCH_X86_64_ 0xC000003EU
#define ARCH_AARCH64 0xC00000B7U

/* 7-insn filter: trap `g1` and `g2` for arch `A`, allow everything else. */
#define MKFILTER(A, G1, G2) { \
  BPF_STMT(BPF_LD | BPF_W | BPF_ABS, offsetof(struct seccomp_data, arch)), \
  BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, (A), 0, 4), \
  BPF_STMT(BPF_LD | BPF_W | BPF_ABS, offsetof(struct seccomp_data, nr)), \
  BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, (G1), 1, 0), \
  BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, (G2), 0, 1), \
  BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_TRAP), \
  BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW) }

static struct sock_filter FX[] = MKFILTER(ARCH_X86_64_, 231, 60);
static struct sock_filter FA[] = MKFILTER(ARCH_AARCH64, 94, 93);

static void h(int s, siginfo_t* si, void* uc) {
    (void)s;(void)uc;
    printf("TRAPPED si_code=%d -> surviving\n", si ? si->si_code : -1);
    fflush(stdout);
    for(;;) sleep(3600);
}

static int inst(struct sock_filter* f, int n, const char* tag) {
    struct sock_fprog p; p.len = (unsigned short)n; p.filter = f;
    errno = 0;
    long r = syscall(__NR_seccomp, SECCOMP_SET_MODE_FILTER, 1, &p);
    printf("seccomp %s -> %ld errno=%d\n", tag, r, errno); fflush(stdout);
    return (int)r;
}

int main(void) {
    struct sigaction sa; memset(&sa,0,sizeof sa);
    sa.sa_sigaction = h; sa.sa_flags = SA_SIGINFO|SA_NODEFER|SA_ONSTACK;
    sigaction(SIGSYS, &sa, NULL);
    if (prctl(PR_SET_NO_NEW_PRIVS,1,0,0,0)!=0) { perror("nnp"); return 2; }
    inst(FX, sizeof(FX)/sizeof(FX[0]), "x86_64");
    inst(FA, sizeof(FA)/sizeof(FA[0]), "aarch64");
    printf("now exit_group(0) ...\n"); fflush(stdout);
    syscall(SYS_exit_group, 0);
    printf("SURVIVED\n"); fflush(stdout);
    return 0;
}
