// ct8.c ? ??????????? ptrace ???????????arm64 ? GETREGSET?
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <pthread.h>
#include <errno.h>
#include <sys/ptrace.h>
#include <sys/wait.h>
#include <sys/types.h>
#include <sys/syscall.h>
#include <sys/uio.h>

static volatile int stop = 0;
static volatile int g_worker_tid = 0;

static void *worker(void *arg) {
    (void)arg;
    g_worker_tid = (int)syscall(SYS_gettid);
    for (int i = 0; i < 300 && !stop; i++) { syscall(SYS_getpid); usleep(1000); }
    return NULL;
}

int main(void) {
    pthread_t th;
    pthread_create(&th, NULL, worker, NULL);
    usleep(150000);
    int tid = g_worker_tid;
    printf("attach to worker tid=%d\n", tid); fflush(stdout);
    if (ptrace(PTRACE_ATTACH, tid, 0, 0) < 0) { printf("ATTACH failed errno=%d\n", errno); return 2; }
    int st; waitpid(tid, &st, __WALL);
    ptrace(PTRACE_SETOPTIONS, tid, 0, (void *)PTRACE_O_TRACESYSGOOD);
    int n = 0;
    for (int k = 0; k < 600; k++) {
        if (ptrace(PTRACE_SYSCALL, tid, 0, 0) < 0) { printf("SYSCALL failed errno=%d\n", errno); break; }
        if (waitpid(tid, &st, __WALL) < 0) { printf("waitpid errno=%d\n", errno); break; }
        if (WIFEXITED(st) || WIFSIGNALED(st)) { printf("worker ended\n"); break; }
        if (WIFSTOPPED(st) && WSTOPSIG(st) == (SIGTRAP | 0x80)) {
            unsigned long long regs[34];
            struct iovec iov = { regs, sizeof(regs) };
            if (ptrace(PTRACE_GETREGSET, tid, (void *)1, &iov) == 0) {
                if (regs[8] == 172UL) n++;   /* x8 = getpid */
            }
        }
    }
    ptrace(PTRACE_DETACH, tid, 0, 0);
    stop = 1;
    pthread_join(th, NULL);
    printf("traced getpid syscalls = %d\n", n);
    return 0;
}
