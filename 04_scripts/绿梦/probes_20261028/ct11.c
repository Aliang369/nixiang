// ct11.c ? ?? arch ????? nr ? {231,60,94,93,252,1} ?? RET_TRAP
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <errno.h>
#include <sys/prctl.h>
#include <sys/syscall.h>
#include <linux/filter.h>
#include <linux/seccomp.h>

static struct sock_filter F[] = {
  BPF_STMT(BPF_LD | BPF_W | BPF_ABS, offsetof(struct seccomp_data, nr)),
  BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 231, 4, 0),
  BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 60, 3, 0),
  BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 94, 2, 0),
  BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 93, 1, 0),
  BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 252, 0, 1),
  BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_TRAP),
  BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW)
};

int main(void) {
    if (prctl(PR_SET_NO_NEW_PRIVS, 1, 0, 0, 0) != 0) { perror("nnp"); return 2; }
    struct sock_fprog p; p.len = sizeof(F)/sizeof(F[0]); p.filter = F;
    errno = 0;
    long r = syscall(__NR_seccomp, SECCOMP_SET_MODE_FILTER, 1, &p);
    printf("seccomp install -> %ld errno=%d\n", r, errno); fflush(stdout);
    printf("calling exit_group(0) ...\n"); fflush(stdout);
    syscall(SYS_exit_group, 0);
    printf("SURVIVED (seccomp saw it)\n"); fflush(stdout);
    sleep(1);
    printf("still alive\n"); fflush(stdout);
    return 0;
}
