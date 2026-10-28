#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <errno.h>
#include <sys/prctl.h>
#include <sys/syscall.h>
#include <linux/audit.h>
#include <linux/filter.h>
#include <linux/seccomp.h>

#define AX 0xC000003EU
#define AA 0xC00000B7U
#define MK(A,G1,G2,ACT) { \
  BPF_STMT(BPF_LD|BPF_W|BPF_ABS, offsetof(struct seccomp_data, arch)), \
  BPF_JUMP(BPF_JMP|BPF_JEQ|BPF_K, (A), 0, 4), \
  BPF_STMT(BPF_LD|BPF_W|BPF_ABS, offsetof(struct seccomp_data, nr)), \
  BPF_JUMP(BPF_JMP|BPF_JEQ|BPF_K, (G1), 1, 0), \
  BPF_JUMP(BPF_JMP|BPF_JEQ|BPF_K, (G2), 0, 1), \
  BPF_STMT(BPF_RET|BPF_K, (ACT)), \
  BPF_STMT(BPF_RET|BPF_K, SECCOMP_RET_ALLOW) }

static struct sock_filter FX[] = MK(AX, 231, 60, SECCOMP_RET_ERRNO | 1);
static struct sock_filter FA[] = MK(AA, 94, 93, SECCOMP_RET_ERRNO | 1);

static void inst(struct sock_filter* f, int n, const char* t) {
    struct sock_fprog p; p.len=(unsigned short)n; p.filter=f;
    errno=0; long r = syscall(__NR_seccomp, SECCOMP_SET_MODE_FILTER, 1, &p);
    printf("%s -> %ld errno=%d\n", t, r, errno); fflush(stdout);
}

int main(void) {
    if (prctl(PR_SET_NO_NEW_PRIVS,1,0,0,0)!=0){perror("nnp");return 2;}
    inst(FX, sizeof(FX)/sizeof(FX[0]), "x86_64");
    inst(FA, sizeof(FA)/sizeof(FA[0]), "aarch64");
    printf("calling exit_group(0) ...\n"); fflush(stdout);
    long r = syscall(SYS_exit_group, 0);
    printf("SURVIVED exit_group ret=%ld errno=%d\n", r, errno); fflush(stdout);
    sleep(1);
    printf("still here\n"); fflush(stdout);
    return 0;
}
