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

static void tracer(pid_t target) {
    usleep(400 * 1000);
    if (ptrace(PTRACE_ATTACH, target, 0, 0) < 0) { fprintf(stderr, "[c] ATTACH errno=%d\n", errno); _exit(3); }
    int st; pid_t w = waitpid(target, &st, __WALL);
    fprintf(stderr, "[c] attach ok, waitpid=%d status=%#x\n", (int)w, st);
    long so = ptrace(PTRACE_SETOPTIONS, target, 0, (void *)PTRACE_O_TRACESYSGOOD);
    fprintf(stderr, "[c] SETOPTIONS=%ld errno=%d\n", so, errno);
    int stops = 0;
    for (int i = 0; i < 4000; i++) {
        errno = 0;
        long r = ptrace(PTRACE_SYSCALL, target, 0, 0);
        if (r < 0) { fprintf(stderr, "[c] SYSCALL errno=%d at stop %d\n", errno, stops); break; }
        errno = 0;
        pid_t wp = waitpid(target, &st, __WALL);
        if (wp < 0) { fprintf(stderr, "[c] waitpid errno=%d at stop %d\n", errno, stops); break; }
        if (WIFEXITED(st) || WIFSIGNALED(st)) { fprintf(stderr, "[c] ended status=%#x after %d stops\n", st, stops); break; }
        if (WIFSTOPPED(st)) {
            int sig = WSTOPSIG(st);
            if (sig == (SIGTRAP | 0x80)) {
                stops++;
                unsigned long long regs[27];
                struct iovec iov = { regs, sizeof(regs) };
                errno = 0;
                long g = ptrace(PTRACE_GETREGSET, target, (void *)1, &iov);
                if (stops <= 6 || (g == 0 && (regs[15] == 231 || regs[15] == 60))) {
                    fprintf(stderr, "[c] stop#%d nr=%llu (getregset=%ld errno=%d)\n", stops, g == 0 ? regs[15] : 0ULL, g, errno);
                }
                if (g == 0 && (regs[15] == 231 || regs[15] == 60)) {
                    regs[15] = 34;
                    ptrace(PTRACE_SETREGSET, target, (void *)1, &iov);
                    fprintf(stderr, "[c] BLOCKED exit syscall\n");
                }
            } else {
                static int shown = 0;
                if (shown++ < 3) fprintf(stderr, "[c] other stop sig=%d\n", sig);
                ptrace(PTRACE_SYSCALL, target, 0, (void *)(long)(sig == SIGSTOP || sig == SIGTRAP ? 0 : sig));
                continue;
            }
        }
    }
    _exit(0);
}

int main(void) {
    pid_t c = fork();
    if (c == 0) { tracer(getppid()); _exit(0); }
    usleep(1200 * 1000);
    fprintf(stderr, "[p] exit_group(0) now\n");
    syscall(SYS_exit_group, 0);
    fprintf(stderr, "[p] SURVIVED\n");
    sleep(2);
    kill(c, SIGKILL);
    return 0;
}
