// fg_cage.c — 绿梦 FairGuard「反自杀笼」验证工具（x86_64 Android，root 外部运行）
//
// 目的：验证假设「FairGuard 判死后是靠进程自身 syscall 自杀（exit_group/tgkill/kill）」。
//   mode=trace : 只记录所有线程的 syscall（重点记录 exit/exit_group/kill/tgkill/tkill/prctl/ptrace/ptrace 等），不干预
//   mode=cage  : 把 exit/exit_group/kill/tgkill/tkill 全部改写成一个无害 syscall（getpid），进程就杀不死自己
//
// 注意：雷电 14 里游戏进程本体是 x86_64（arm64 代码走 houdini 翻译），故本工具编 x86_64。
//
// 用法: fg_cage <pid> <seconds> <trace|cage> [日志路径]
// 退出码: 0 = 正常结束；2 = 用法错误；3 = attach 失败
#define _GNU_SOURCE
#include <stdio.h>
#include <stdarg.h>
#include <stdlib.h>
#include <signal.h>
#include <string.h>
#include <errno.h>
#include <unistd.h>
#include <dirent.h>
#include <time.h>
#include <sys/ptrace.h>
#include <sys/wait.h>
#include <sys/types.h>
#include <sys/user.h>
#include <sys/uio.h>

#define MAXT 512

static pid_t tids[MAXT];
static int ntids = 0;
static FILE *logf = NULL;
static int cage = 0;
static int load_maps_once = 1;
static int logall = 0;
static pid_t pid0 = 0;
static unsigned long n_syscall = 0, n_blocked = 0;

static void logline(const char *fmt, ...) {
    va_list ap;
    va_start(ap, fmt);
    vfprintf(logf, fmt, ap);
    va_end(ap);
    fflush(logf);
}

static int is_interesting(long nr) {
    switch (nr) {
    case 60:  /* exit */
    case 231: /* exit_group */
    case 62:  /* kill */
    case 234: /* tgkill */
    case 200: /* tkill */
    case 129: /* rt_sigqueueinfo */
    case 157: /* prctl */
    case 101: /* ptrace */
    case 310: /* process_vm_readv */
    case 311: /* process_vm_writev */
    case 39:  /* getpid */
    case 2:   /* open */
    case 4:   /* stat */
    case 6:   /* lstat */
    case 21:  /* access */
    case 89:  /* readlink */
    case 257: /* openat */
    case 262: /* newfstatat */
    case 267: /* readlinkat */
    case 332: /* statx */
        return 1;
    default: return 0;
    }
}

static long set_orig_rax(pid_t tid, long val) {
    struct user_regs_struct r;
    struct iovec iov = { &r, sizeof(r) };
    if (ptrace(PTRACE_GETREGSET, tid, (void *)1 /* NT_PRSTATUS */, &iov) < 0) return -1;
    r.orig_rax = (unsigned long)val;
    if (ptrace(PTRACE_SETREGSET, tid, (void *)1, &iov) < 0) return -1;
    return 0;
}

static int attach_one(pid_t tid) {
    if (ptrace(PTRACE_ATTACH, tid, 0, 0) < 0) return -1;
    int st;
    if (waitpid(tid, &st, __WALL) < 0) return -1;
    ptrace(PTRACE_SETOPTIONS, tid, 0, (void *)(PTRACE_O_TRACECLONE | PTRACE_O_TRACEFORK |
                                               PTRACE_O_TRACEVFORK | PTRACE_O_TRACESYSGOOD |
                                               PTRACE_O_EXITKILL));
    return 0;
}


/* 找出地址落在哪个 so（读 /proc/<pid>/maps），用于判断是 libc 包装还是内联 svc */
static void mod_of(pid_t pid, unsigned long addr, char *out, size_t n) {
    snprintf(out, n, "?");
    char mp[64]; snprintf(mp, sizeof(mp), "/proc/%d/maps", pid);
    FILE *f = fopen(mp, "r");
    if (!f) return;
    char line[512];
    while (fgets(line, sizeof(line), f)) {
        unsigned long a1, a2;
        if (sscanf(line, "%lx-%lx", &a1, &a2) != 2) continue;
        if (addr >= a1 && addr < a2) {
            char *slash = strrchr(line, '/');
            if (slash) { slash[strcspn(slash, "\n")] = 0; snprintf(out, n, "%s", slash); }
            else snprintf(out, n, "[anon]");
            break;
        }
    }
    fclose(f);
}


/* ---- 栈扫描：把栈上落在 guest 映射区间里的指针挑出来，用于定位退出的发起库 ---- */
typedef struct { unsigned long lo, hi; char nm[96]; } MapEnt;
static MapEnt g_maps[256];
static int g_nmaps = 0;

static void load_maps(pid_t pid) {
    g_nmaps = 0;
    char mp[64]; snprintf(mp, sizeof(mp), "/proc/%d/maps", pid);
    FILE *f = fopen(mp, "r");
    if (!f) return;
    char line[512];
    while (fgets(line, sizeof(line), f) && g_nmaps < 256) {
        unsigned long lo, hi; char perms[8] = {0};
        if (sscanf(line, "%lx-%lx %7s", &lo, &hi, perms) != 3) continue;
        char *slash = strrchr(line, '/');
        if (!slash) continue;
        slash[strcspn(slash, "\n")] = 0;
        const char *base = strrchr(slash, '/'); base = base ? base + 1 : slash;
        if (!strstr(base, "lib")) continue;
        g_maps[g_nmaps].lo = lo; g_maps[g_nmaps].hi = hi;
        snprintf(g_maps[g_nmaps].nm, sizeof(g_maps[g_nmaps].nm), "%s", base);
        g_nmaps++;
    }
    fclose(f);
}

static void dump_stack_hits(pid_t tid, unsigned long rsp, unsigned long lr) {
    if (!g_nmaps) return;
    for (int pass = 0; pass < 1; pass++) {
        /* lr 本身 */
        for (int i = 0; i < g_nmaps; i++)
            if (lr >= g_maps[i].lo && lr < g_maps[i].hi)
                logline("      lr=%#lx -> %s+%#lx\n", lr, g_maps[i].nm, lr - g_maps[i].lo);
        /* 栈上 512 个字 */
        for (int w = 0; w < 512; w++) {
            errno = 0;
            long v = ptrace(PTRACE_PEEKDATA, tid, (void *)(rsp + w * 8), 0);
            if (v == -1 && errno) break;
            unsigned long u = (unsigned long)v;
            for (int i = 0; i < g_nmaps; i++)
                if (u >= g_maps[i].lo && u < g_maps[i].hi) {
                    logline("      stk[%d]=%#lx -> %s+%#lx\n", w, u, g_maps[i].nm, u - g_maps[i].lo);
                    break;
                }
        }
    }
}


/* ---- 路径解析：从 tracee 内存读 C 字符串，用于给 openat/stat 打上路径 ---- */

/* ---- readlinkat 结果回读：entry 时记住 (buf,size)，exit 时把内容打出来 ---- */
typedef struct { pid_t tid; int kind; unsigned long ptr; unsigned long size; } Pending;
static Pending g_pend[64];
static int g_npend = 0;

static void pend_set(pid_t tid, int kind, unsigned long ptr, unsigned long size) {
    for (int i = 0; i < g_npend; i++) if (g_pend[i].tid == tid) { g_pend[i].kind = kind; g_pend[i].ptr = ptr; g_pend[i].size = size; return; }
    if (g_npend < 64) { g_pend[g_npend].tid = tid; g_pend[g_npend].kind = kind; g_pend[g_npend].ptr = ptr; g_pend[g_npend].size = size; g_npend++; }
}
static Pending* pend_get(pid_t tid) {
    for (int i = 0; i < g_npend; i++) if (g_pend[i].tid == tid) return &g_pend[i];
    return NULL;
}

static void read_cstr(pid_t pid, unsigned long addr, char *out, int n) {
    out[0] = 0;
    if (!addr) return;
    int k = 0;
    while (k < n - 1) {
        errno = 0;
        long v = ptrace(PTRACE_PEEKDATA, pid, (void *)(addr + k), 0);
        if (v == -1 && errno) break;
        for (int b = 0; b < 8 && k < n - 1; b++) {
            char c = (char)((v >> (8 * b)) & 0xFF);
            if (!c) { out[k] = 0; return; }
            out[k++] = c;
        }
    }
    out[k] = 0;
}

static void log_paths(pid_t tid, long nr, unsigned long a0, unsigned long a1) {
    char path[300];
    /* x86_64: open=2, stat=4, lstat=6, access=21, openat=257, newfstatat=262, statx=332, readlink=89, readlinkat=267 */
    if (nr == 2 || nr == 4 || nr == 6 || nr == 21 || nr == 89) {
        read_cstr(tid, a0, path, sizeof path);
        logline("      path[%ld] %s\n", nr, path);
    } else if (nr == 257 || nr == 262 || nr == 267 || nr == 332) {
        read_cstr(tid, a1, path, sizeof path);
        logline("      path[%ld] dirfd=%ld %s\n", nr, (long)a0, path);
    }
}

static void add_tid(pid_t tid) {    for (int i = 0; i < ntids; i++) if (tids[i] == tid) return;
    if (ntids < MAXT) tids[ntids++] = tid;
}

static void attach_all(pid_t pid) {
    char path[128];
    snprintf(path, sizeof(path), "/proc/%d/task", pid);
    DIR *d = opendir(path);
    if (!d) return;
    struct dirent *e;
    while ((e = readdir(d))) {
        if (e->d_name[0] == '.') continue;
        pid_t tid = atoi(e->d_name);
        if (tid <= 0) continue;
        if (attach_one(tid) == 0) {
            add_tid(tid);
            logline("[attached] tid=%d\n", tid);
            ptrace(PTRACE_SYSCALL, tid, 0, 0);
        }
    }
    closedir(d);
}

int main(int argc, char **argv) {
    if (argc < 4) { fprintf(stderr, "usage: %s <pid> <seconds> <trace|cage> [logfile]\n", argv[0]); return 2; }
    pid_t pid = atoi(argv[1]);
    pid0 = pid;
    int secs = atoi(argv[2]);
    cage = (strcmp(argv[3], "cage") == 0 || strcmp(argv[3], "cageall") == 0);
    logall = (strcmp(argv[3], "traceall") == 0 || strcmp(argv[3], "cageall") == 0);
    const char *lp = (argc > 4) ? argv[4] : "/data/local/tmp/fg_cage.log";
    logf = fopen(lp, "w");
    if (!logf) { perror("fopen"); return 2; }
    logline("# pid=%d secs=%d mode=%s\n", pid, secs, cage ? "cage" : "trace");

    attach_all(pid);
    if (ntids == 0) { logline("attach failed: %s\n", strerror(errno)); return 3; }

    time_t t0 = time(NULL);
    while (time(NULL) - t0 < secs) {
        int st;
        pid_t tid = waitpid(-1, &st, __WALL | WNOHANG);
        if (tid <= 0) { usleep(200); continue; }
        if (WIFEXITED(st) || WIFSIGNALED(st)) {
            logline("[gone] tid=%d exited=%d status=%d sig=%d\n", tid,
                    WIFEXITED(st), WIFEXITED(st) ? WEXITSTATUS(st) : -1,
                    WIFSIGNALED(st) ? WTERMSIG(st) : -1);
            continue;
        }
        if (!WIFSTOPPED(st)) continue;
        int sig = WSTOPSIG(st);
        if (sig == (SIGTRAP | 0x80)) {
            struct user_regs_struct r;
            struct iovec iov = { &r, sizeof(r) };
            if (ptrace(PTRACE_GETREGSET, tid, (void *)1, &iov) == 0) {
                if ((long)r.rax != -38 /* syscall EXIT stop */) {
                    Pending *pd = pend_get(tid);
                    if (pd && pd->kind != 0 && (long)r.rax > 0) {
                        char rbuf[300];
                        read_cstr(tid, pd->ptr, rbuf, sizeof rbuf);
                        logline("      ==> readlink RESULT[%d] len=%ld \"%s\"\n", pd->kind, (long)r.rax, rbuf);
                        pd->kind = 0;
                        pd->ptr = 0;
                    }
                }
                if ((long)r.rax == -38 /* -ENOSYS: syscall entry */) {
                    long nr = (long)r.orig_rax;
                    n_syscall++;
                    if (logall || is_interesting(nr)) {
                        logline("[%ld] tid=%d nr=%ld a0=%#llx a1=%#llx a2=%#llx\n",
                                (long)(time(NULL) - t0), tid, nr,
                                (unsigned long long)r.rdi, (unsigned long long)r.rsi,
                                (unsigned long long)r.rdx);
                        log_paths(tid, nr, (unsigned long)r.rdi, (unsigned long)r.rsi);
                        if (nr == 267) pend_set(tid, 267, (unsigned long)r.rdx, (unsigned long)r.r10);
                        else if (nr == 89) pend_set(tid, 89, (unsigned long)r.rsi, (unsigned long)r.rdx);
                    } else if (nr == 267) {
                        pend_set(tid, 267, (unsigned long)r.rdx, (unsigned long)r.r10);
                    } else if (nr == 89) {
                        pend_set(tid, 89, (unsigned long)r.rsi, (unsigned long)r.rdx);
                    }
                    if (cage && nr == 231 && tid != pid0) { /* 只拦非主线程的 exit_group：主线程正常退出放行 */
                        n_blocked++;
                        char cpath[64], comm[64] = "?";
                        snprintf(cpath, sizeof(cpath), "/proc/%d/comm", tid);
                        FILE *cf = fopen(cpath, "r");
                        if (cf) { if (fgets(comm, sizeof(comm), cf)) { char *nl = strchr(comm, '\n'); if (nl) *nl = 0; } fclose(cf); }
                        char mod[128]; mod_of(pid, (unsigned long)r.rip, mod, sizeof(mod));
                        if (load_maps_once) { load_maps(pid); load_maps_once = 0; }
                        dump_stack_hits(tid, (unsigned long)r.rsp, (unsigned long)r.rax /* placeholder */);
                        logline("[BLOCK] t=%ld tid=%d comm=%s nr=%ld a0=%lld rip=%#llx mod=%s\n",
                                (long)(time(NULL) - t0), tid, comm, nr, (long long)r.rdi, (unsigned long long)r.rip, mod);
                        set_orig_rax(tid, 34 /* pause：把自杀线程永久挂住，且不落回调用点后面的垃圾指令 */);
                    }
                }
            }
            ptrace(PTRACE_SYSCALL, tid, 0, 0);
        } else if (sig == SIGTRAP) {
            /* clone/fork event: 新线程继承 trace，把它也纳入 */
            unsigned long ev = (unsigned long)st >> 16;
            if (ev == PTRACE_EVENT_CLONE || ev == PTRACE_EVENT_FORK || ev == PTRACE_EVENT_VFORK) {
                unsigned long newtid = 0;
                if (ptrace(PTRACE_GETEVENTMSG, tid, 0, &newtid) == 0 && newtid)
                    add_tid((pid_t)newtid);
            }
            ptrace(PTRACE_SYSCALL, tid, 0, 0);
        } else {
            /* 把信号原样放回去（不能吞，否则改变行为） */
            ptrace(PTRACE_SYSCALL, tid, 0, (void *)(long)sig);
        }
    }
    logline("# done syscalls=%lu blocked=%lu\n", n_syscall, n_blocked);

    /* detach：先全部脱离，让游戏自由跑 */
    char path[128];
    snprintf(path, sizeof(path), "/proc/%d/task", pid);
    DIR *d = opendir(path);
    if (d) {
        struct dirent *e;
        while ((e = readdir(d))) {
            if (e->d_name[0] == '.') continue;
            pid_t tid = atoi(e->d_name);
            if (tid > 0) ptrace(PTRACE_DETACH, tid, 0, 0);
        }
        closedir(d);
    }
    for (int i = 0; i < ntids; i++) ptrace(PTRACE_DETACH, tids[i], 0, 0);

    int alive = (kill(pid, 0) == 0);
    logline("# alive_after=%d\n", alive);
    fclose(logf);
    printf("done syscalls=%lu blocked=%lu alive=%d\n", n_syscall, n_blocked, alive);
    return 0;
}
