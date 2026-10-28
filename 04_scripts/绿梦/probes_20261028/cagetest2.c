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
#define ARCH_I386    0x40000003U
#define ARCH_ARM     0x40000028U
#define ARCH_AARCH64 0xC00000B7U

static struct sock_filter F[] = {
    BPF_STMT(BPF_LD | BPF_W | BPF_ABS, offsetof(struct seccomp_data, arch)),   /* 0 */
    BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, ARCH_X86_64_, 0, 3),                   /* 1 */
    BPF_STMT(BPF_LD | BPF_W | BPF_ABS, offsetof(struct seccomp_data, nr)),     /* 2 */
    BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 231, 15, 1),                           /* 3 */
    BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW),                              /* 4 */
    BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, ARCH_I386, 0, 3),                      /* 5 */
    BPF_STMT(BPF_LD | BPF_W | BPF_ABS, offsetof(struct seccomp_data, nr)),     /* 6 */
    BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 252, 11, 1),                           /* 7 */
    BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW),                              /* 8 */
    BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, ARCH_ARM, 0, 3),                       /* 9 */
    BPF_STMT(BPF_LD | BPF_W | BPF_ABS, offsetof(struct seccomp_data, nr)),     /* 10 */
    BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 248, 7, 1),                            /* 11 */
    BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW),                              /* 12 */
    BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, ARCH_AARCH64, 0, 3),                   /* 13 */
    BPF_STMT(BPF_LD | BPF_W | BPF_ABS, offsetof(struct seccomp_data, nr)),     /* 14 */
    BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 94, 3, 1),                             /* 15 */
    BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW),                              /* 16 */
    BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW),                              /* 17 */
    BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW),                              /* 18 */
    BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_TRAP),                               /* 19 */
};

static void h(int s, siginfo_t* si, void* uc) {
    (void)s;(void)uc;
    printf("TRAPPED si_code=%d -> surviving\n", si ? si->si_code : -1);
    fflush(stdout);
    for(;;) { sleep(3600); }
}

int main(void) {
    struct sigaction sa; memset(&sa,0,sizeof sa);
    sa.sa_sigaction = h; sa.sa_flags = SA_SIGINFO|SA_NODEFER|SA_ONSTACK;
    sigaction(SIGSYS, &sa, NULL);
    if (prctl(PR_SET_NO_NEW_PRIVS,1,0,0,0)!=0) { perror("no_new_privs"); return 2; }
    struct sock_fprog p = { (unsigned short)(sizeof(F)/sizeof(F[0])), F };
    long r = syscall(__NR_seccomp, SECCOMP_SET_MODE_FILTER, 1, &p);
    printf("seccomp(TSYNC) -> %ld errno=%d\n", r, errno);
    fflush(stdout);
    printf("now calling exit_group(0) ...\n"); fflush(stdout);
    syscall(SYS_exit_group, 0);
    printf("SURVIVED\n"); fflush(stdout);
    return 0;
}
