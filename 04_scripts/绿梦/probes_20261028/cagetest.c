// cagetest.c ? ?? seccomp TRAP ???(houdini ?? / x86_64 ??)????? exit_group
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

static struct sock_filter F[] = {
    BPF_STMT(BPF_LD | BPF_W | BPF_ABS, offsetof(struct seccomp_data, arch)),
    BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, AUDIT_ARCH_AARCH64, 1, 0),
    BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, AUDIT_ARCH_X86_64, 2, 7),
    BPF_STMT(BPF_LD | BPF_W | BPF_ABS, offsetof(struct seccomp_data, nr)),
    BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 94, 6, 5),
    BPF_STMT(BPF_LD | BPF_W | BPF_ABS, offsetof(struct seccomp_data, nr)),
    BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 231, 4, 3),
    BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW),
    BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW),
    BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW),
    BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW),
    BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_TRAP),
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
    long r = syscall(__NR_seccomp, SECCOMP_SET_MODE_FILTER, 1 /*TSYNC*/, &p);
    printf("seccomp(TSYNC) -> %ld errno=%d\n", r, errno);
    if (r != 0) {
        errno = 0;
        r = syscall(__NR_seccomp, SECCOMP_SET_MODE_FILTER, 0, &p);
        printf("seccomp(0) -> %ld errno=%d\n", r, errno);
    }
    fflush(stdout);
    FILE* f = fopen("/proc/self/status","r"); char line[256];
    while (f && fgets(line,sizeof line,f)) if (!strncmp(line,"Seccomp",7)||!strncmp(line,"NoNewPrivs",10)) printf("%s", line);
    if (f) fclose(f);
    fflush(stdout);
    printf("now calling exit_group(0) ...\n"); fflush(stdout);
    syscall(SYS_exit_group, 0);
    printf("SURVIVED exit_group\n"); fflush(stdout);
    return 0;
}
