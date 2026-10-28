// ct9.c ? ???app ?? fork ????? ptrace ????????? x86_64 ????
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <errno.h>
#include <signal.h>
#include <sys/prctl.h>
#include <sys/ptrace.h>
#include <sys/wait.h>
#include <sys/types.h>
#include <sys/syscall.h>
#include <sys/uio.h>

#ifndef PR_SET_PTRACER
#define PR_SET_PTRACER 0x59616d61
#endif

#define X86_64_NREGS 27
#define IDX_ORIG_RAX 15
#define IDX_RIP 16
#define IDX_RSP 19

static void tracer(pid_t target) {
    usleep(400 * 1000);
    if (ptrace(PTRACE_ATTACH, target, 0, 0) < 0) {
        fprintf(stderr, "[child] ATTACH failed errno=%d\n", errno);
        _exit(3);
    }
    int st;
    waitpid(target, &st, __WALL);
    ptrace(PTRACE_SETOPTIONS, target, 0, (void *)PTRACE_O_TRACESYSGOOD);
    fprintf(stderr, "[child] attached\n");
    int blocks = 0;
    for (;;) {
        if (ptrace(PTRACE_SYSCALL, target, 0, 0) < 0) break;
        if (waitpid(target, &st, __WALL) < 0) break;
        if (WIFEXITED(st) || WIFSIGNALED(st)) { fprintf(stderr, "[child] target ended\n"); break; }
        if (WIFSTOPPED(st) && WSTOPSIG(st) == (SIGTRAP | 0x80)) {
            unsigned long long regs[X86_64_NREGS];
            struct iovec iov = { regs, sizeof(regs) };
            if (ptrace(PTRACE_GETREGSET, target, (void *)1, &iov) == 0) {
                unsigned long long nr = regs[IDX_ORIG_RAX];
                if (nr == 231 || nr == 60) {
                    regs[IDX_ORIG_RAX] = 34;   /* pause */
                    ptrace(PTRACE_SETREGSET, target, (void *)1, &iov);
                    blocks++;
                    fprintf(stderr, "[child] BLOCKED nr=%llu rip=%#llx (n=%d)\n", nr, regs[IDX_RIP], blocks);
                }
            }
        }
    }
    _exit(0);
}

int main(void) {
    pid_t c = fork();
    if (c == 0) { tracer(getppid()); _exit(0); }
    long rc = prctl(PR_SET_PTRACER, (unsigned long)c, 0, 0, 0);
    printf("PR_SET_PTRACER -> %ld errno=%d child=%d\n", rc, errno, c); fflush(stdout);
    usleep(1200 * 1000);
    printf("parent: calling exit_group(0) ...\n"); fflush(stdout);
    syscall(SYS_exit_group, 0);
    printf("parent: SURVIVED exit_group -> watchdog works\n"); fflush(stdout);
    kill(c, SIGKILL);
    return 0;
}
