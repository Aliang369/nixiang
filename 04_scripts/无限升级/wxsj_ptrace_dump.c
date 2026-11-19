#define _GNU_SOURCE
/* wxsj_ptrace_dump.c — 无限升级 13.101 崩溃瞬间栈帧取证探针（arm64 静态）
 *
 * 2026-11-19 v2：改用 PTRACE_SEIZE + PTRACE_INTERRUPT（不再 ATTACH+CONT+SIGSTOP）。
 *   原因（实测事故）：ATTACH 后释放必须先 SIGSTOP 才能在 ptrace-stop 上 DETACH，而 SIGSTOP 触发
 *   的是 group-stop，DETACH 携带的 SIGCONT 在 group-stop 下会被内核丢弃 ⇒ 260 个线程停在 T，
 *   游戏被冻住。SEIZE 不改变运行态，INTERRUPT 产生的是干净 ptrace-stop（PTRACE_EVENT_STOP），
 *   DETACH 直接放行，全程不制造 SIGSTOP，也不会凭空产生信号。
 *
 * 模式：
 *   -p <tid>            单线程
 *   -p <pid> --all      遍历 /proc/<pid>/task/* 逐线程 SEIZE（崩溃线程不一定是主线程时必用）
 *
 * 用法：wxsj_ptrace_dump -p <pid|tid> [--all] [--addr <hex>] [--timeout <sec>] [--out <path>]
 *
 * 安全约定：
 *   - 只转发**真实**信号；我们自己 INTERRUPT 产生的 stop 绝不回投；
 *   - 释放后逐线程核 /proc/<pid>/task/<tid>/stat 第 3 字段；仍有 T/t 则补 SIGCONT 并复核（最多 3 轮）；
 *   - 释放前后各打一行 threads state 统计（S/R/T/D/Z 计数），便于一眼判断有没有把别人挂住。
 */
#include <dirent.h>
#include <errno.h>
#include <fcntl.h>
#include <signal.h>
#include <stdarg.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <unistd.h>
#include <sys/ptrace.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <sys/uio.h>
#include <sys/wait.h>
#include <linux/elf.h>

#ifndef NT_PRSTATUS
#define NT_PRSTATUS 1
#endif
#ifndef PTRACE_SEIZE
#define PTRACE_SEIZE 0x4206
#endif
#ifndef PTRACE_INTERRUPT
#define PTRACE_INTERRUPT 0x4207
#endif
#ifndef PTRACE_EVENT_STOP
#define PTRACE_EVENT_STOP 128
#endif

struct ptregs { uint64_t regs[31]; uint64_t sp; uint64_t pc; uint64_t pstate; };

#define LIBIL2CPP_RANGES 8
static struct { uint64_t lo, hi; char perm[8]; } g_il2cpp[LIBIL2CPP_RANGES];
static int g_il2cpp_n = 0;
static FILE *g_out = NULL;
static int g_memfd = -1;

#define MAX_TIDS 1024
static pid_t g_tids[MAX_TIDS];
static int g_ntids = 0;
static pid_t g_pid = 0;

/* ---- 2026-11-19 v3：崩溃候选过滤 + 保持值守 ----
 * 事故：挂在游戏上 6 秒内抓到 tid=26974 comm=anythink_schedu 的 SIGSEGV，
 *   pc=0xaad4e34c 不在 libil2cpp 映射内，进程根本没死（广告 SDK 自己处理了该信号）
 *   ⇒ 假阳性，且探针抓完就退出，真正的秘境崩溃没人盯。
 * 对策：① pc 必须落在 libil2cpp 的 r-xp 段（默认自动算，可 --pc-range / --any-pc）；
 *       ② comm 命中广告 SDK 名单直接跳过（默认内置，可 --skip-comm 追加 / --no-skip-comm 清空）；
 *       ③ --keep：dump 后继续盯（同一 tid 只 dump 一次）。 */
/* 2026-11-19 v5：pc 过滤改多段（v4 只取第一段 r-xp，实测那段只有 0x3000，漏掉真代码段） */
#define MAX_PCX 16
static struct { uint64_t lo, hi; } g_pcx[MAX_PCX];
static int g_pcx_n = 0;
static uint64_t g_cfg_lo = 0, g_cfg_hi = 0;   /* --pc-range 显式区间 */
static int g_cfg_set = 0;
static uint64_t g_pc_lo = 0, g_pc_hi = 0;     /* 兼容保留：指向第一段，仅用于打印 */
static int g_pc_filter = 1;

/* ---- v5：入口断点（BRK #0）---- */
static uint64_t g_brk_addr = 0;      /* 目标绝对地址（用户给的） */
static uint64_t g_brk_waddr = 0;     /* 对齐后的 8 字节字地址 */
static uint64_t g_brk_orig = 0;      /* 原始 8 字节字 */
static int g_brk_armed = 0;
static int g_brk_continue = 0;
static int g_brk_hits = 0;
static int brk_arm(pid_t tid);
static int brk_restore(pid_t tid);
static void brk_hit(pid_t tid);
static int g_keep = 0;
static const char *g_skip_comm[32];
static int g_skip_comm_n = 0;
static pid_t g_dumped[MAX_TIDS];
static int g_dumped_n = 0;
static int g_dump_total = 0;
static int g_no_skip_defaults = 0;
static int g_pc_unknown_warned = 0;
/* ---- 2026-11-19 v4：崩溃瞬间把「代码页」也落盘 ----
 * 背景：libil2cpp 代码页按需解密，未执行过的页读出来是全 0；调用者那页只有崩溃瞬间才可读。
 * 落两类：① pc-0x2000..pc+0x2000（16KB）；② sp..sp+0x1000 里落在 libil2cpp 的候选返回地址，
 * 每个 ret-0x40..ret+0x40（128B），最多 32 个。文本进同一个 txt，二进制各落一个 .bin 便于 capstone。 */
#define MAX_CODE_CANDS 32
static char g_codedir[256];
static char g_brkcode[256];      /* v6：BRK 命中时 x30/page 代码块专用目录 */
static const char *g_dumpdir = NULL;   /* code_dump 用哪个目录 */

static void out(const char *fmt, ...) {
    va_list ap;
    va_start(ap, fmt); vfprintf(stdout, fmt, ap); va_end(ap);
    if (g_out) { va_start(ap, fmt); vfprintf(g_out, fmt, ap); va_end(ap); fflush(g_out); }
    fflush(stdout);
}

/* ---------- 线程状态统计（S/R/T/D/Z 计数 + 逐线程明细） ---------- */
static void print_states(const char *tag) {
    int nS = 0, nR = 0, nT = 0, nD = 0, nZ = 0, nOther = 0, nTot = 0;
    char tline[2048]; tline[0] = 0;
    char path[64];
    snprintf(path, sizeof path, "/proc/%d/task", (int)g_pid);
    DIR *d = opendir(path);
    if (!d) { out("[states] %s: 打不开 %s (%s)\n", tag, path, strerror(errno)); return; }
    struct dirent *e;
    while ((e = readdir(d)) != NULL) {
        if (e->d_name[0] < '0' || e->d_name[0] > '9') continue;
        char sp[128];
        snprintf(sp, sizeof sp, "/proc/%d/task/%s/stat", (int)g_pid, e->d_name);
        FILE *f = fopen(sp, "r");
        if (!f) continue;
        char buf[512];
        char st = 0;
        if (fgets(buf, sizeof buf, f)) {
            char *p = strrchr(buf, ')');
            if (p && p[1] == ' ') st = p[2];
        }
        fclose(f);
        nTot++;
        switch (st) {
            case 'S': nS++; break;
            case 'R': nR++; break;
            case 'T': nT++; break;
            case 't': nT++; break;
            case 'D': nD++; break;
            case 'Z': nZ++; break;
            default: nOther++; break;
        }
        if (st == 'T' || st == 't') {
            size_t l = strlen(tline);
            snprintf(tline + l, sizeof(tline) - l, " %s", e->d_name);
        }
    }
    closedir(d);
    out("[states] %s: total=%d S=%d R=%d T=%d D=%d Z=%d other=%d\n",
        tag, nTot, nS, nR, nT, nD, nZ, nOther);
    if (nT) out("[states] %s: 处于 T 的 tid:%s\n", tag, tline);
}

/* ---------- 内存读 ---------- */
static ssize_t vm_read_once(pid_t pid, uint64_t addr, void *buf, size_t len) {
    struct iovec l = { buf, len };
    struct iovec r = { (void *)(uintptr_t)addr, len };
    errno = 0;
    ssize_t n = process_vm_readv(pid, &l, 1, &r, 1, 0);
    if (n == (ssize_t)len) return n;
    if (g_memfd < 0) {
        char p[64];
        snprintf(p, sizeof p, "/proc/%d/mem", (int)pid);
        g_memfd = open(p, O_RDONLY);
        if (g_memfd < 0) out("[mem] open %s 失败: %s\n", p, strerror(errno));
    }
    if (g_memfd >= 0) {
        ssize_t m = pread(g_memfd, buf, len, (off_t)addr);
        if (m == (ssize_t)len) return m;
    }
    return -1;
}

static size_t read_mem(pid_t pid, uint64_t addr, void *buf, size_t len, int *bad, const char *what) {
    size_t done = 0; int nbad = 0;
    uint8_t *p = (uint8_t *)buf;
    while (done < len) {
        uint64_t a = addr + done;
        size_t page_left = 4096 - (size_t)(a & 0xFFF);
        size_t chunk = (len - done < page_left) ? (len - done) : page_left;
        ssize_t n = vm_read_once(pid, a, p + done, chunk);
        if (n == (ssize_t)chunk) { done += chunk; continue; }
        if (nbad == 0) out("[mem] 读 %s @0x%llx 失败: %s\n", what, (unsigned long long)a,
                           errno ? strerror(errno) : "short read");
        nbad++;
        memset(p + done, 0xCC, chunk);
        done += chunk;
    }
    if (bad) *bad = nbad;
    return done;
}

static int read_u64(pid_t pid, uint64_t addr, uint64_t *v) {
    return read_mem(pid, addr, v, 8, NULL, "u64") == 8 ? 0 : -1;
}

static void read_comm(pid_t pid, pid_t tid, char *buf, size_t n) {
    char p[128];
    snprintf(p, sizeof p, "/proc/%d/task/%d/comm", (int)pid, (int)tid);
    buf[0] = 0;
    FILE *f = fopen(p, "r");
    if (!f) { snprintf(buf, n, "<comm 读失败:%s>", strerror(errno)); return; }
    if (fgets(buf, (int)n, f)) {
        size_t l = strlen(buf);
        while (l && (buf[l - 1] == '\n' || buf[l - 1] == '\r')) buf[--l] = 0;
    }
    fclose(f);
}

static void load_maps(pid_t pid) {
    char path[64]; snprintf(path, sizeof path, "/proc/%d/maps", (int)pid);
    FILE *f = fopen(path, "r");
    if (!f) { out("[maps] 打不开 %s: %s\n", path, strerror(errno)); return; }
    char line[512];
    while (fgets(line, sizeof line, f)) {
        if (!strstr(line, "libil2cpp.so")) continue;
        unsigned long long lo, hi; char perm[8];
        if (sscanf(line, "%llx-%llx %7s", &lo, &hi, perm) == 3) {
            out("[maps] libil2cpp %s 0x%llx-0x%llx  %s", perm, lo, hi, line);
            if (g_il2cpp_n < LIBIL2CPP_RANGES) {
                g_il2cpp[g_il2cpp_n].lo = lo; g_il2cpp[g_il2cpp_n].hi = hi;
                snprintf(g_il2cpp[g_il2cpp_n].perm, sizeof g_il2cpp[g_il2cpp_n].perm, "%s", perm);
                g_il2cpp_n++;
            }
            /* 收集**全部**可执行段（v5 修：v4 只取第一段，实测第一段仅 0x3000，真代码段被漏） */
            if (strchr(perm, 'x') && g_pcx_n < MAX_PCX) {
                g_pcx[g_pcx_n].lo = lo; g_pcx[g_pcx_n].hi = hi; g_pcx_n++;
            }
        }
    }
    fclose(f);
    if (!g_il2cpp_n) out("[maps] 未找到 libil2cpp.so 映射（候选返回地址扫描将为空）\n");
}

/* 关注区间 = libil2cpp 全部 r-xp 段 ∪ --pc-range 显式区间 */
static uint64_t in_range(uint64_t v) {   /* 返回段基址，0 = 不在范围内 */
    for (int i = 0; i < g_pcx_n; i++)
        if (v >= g_pcx[i].lo && v < g_pcx[i].hi) return g_pcx[i].lo;
    if (g_cfg_set && v >= g_cfg_lo && v < g_cfg_hi) return g_cfg_lo;
    return 0;
}

static int in_il2cpp(uint64_t v, uint64_t *off, const char **perm) {
    for (int i = 0; i < g_il2cpp_n; i++) {
        if (v >= g_il2cpp[i].lo && v < g_il2cpp[i].hi) {
            if (off) *off = v - g_il2cpp[i].lo;
            if (perm) *perm = g_il2cpp[i].perm;
            return 1;
        }
    }
    uint64_t base = in_range(v);
    if (base) {
        if (off) *off = v - base;
        if (perm) *perm = (g_cfg_set && base == g_cfg_lo) ? "r-x(cfg)" : "r-x";
        return 1;
    }
    return 0;
}

static void hexdump(pid_t pid, const char *label, uint64_t addr, size_t len) {
    out("\n---- %s ---- 0x%llx (+0x%zx)\n", label, (unsigned long long)addr, len);
    if (!addr) { out("(基址为 0，跳过)\n"); return; }
    uint8_t buf[2048];
    if (len > sizeof buf) len = sizeof buf;
    int bad = 0;
    read_mem(pid, addr, buf, len, &bad, label);
    if (bad) out("(本段有 %d 页不可读，已填 0xCC)\n", bad);
    for (size_t o = 0; o < len; o += 32) {
        out("  +0x%03zx 0x%llx |", o, (unsigned long long)(addr + o));
        for (size_t j = 0; j < 32 && o + j < len; j++) {
            if (j == 16) out(" ");
            out(" %02x", buf[o + j]);
        }
        out(" | ");
        for (size_t j = 0; j < 32 && o + j < len; j++) {
            uint8_t c = buf[o + j];
            out("%c", (c >= 0x20 && c < 0x7f) ? c : '.');
        }
        out("\n");
    }
}

static void show_u64(pid_t pid, const char *name, uint64_t addr) {
    uint64_t v = 0;
    if (read_u64(pid, addr, &v) == 0) {
        uint64_t off = 0; const char *perm = NULL;
        if (in_il2cpp(v, &off, &perm))
            out("  %-22s [0x%llx] = 0x%016llx   <- libil2cpp+0x%llx (%s)\n", name,
                (unsigned long long)addr, (unsigned long long)v, (unsigned long long)off, perm);
        else
            out("  %-22s [0x%llx] = 0x%016llx\n", name, (unsigned long long)addr,
                (unsigned long long)v);
    } else {
        out("  %-22s [0x%llx] = <读失败: %s>\n", name, (unsigned long long)addr, strerror(errno));
    }
}

/* 把一段代码同时写进 txt（hexdump）和 .bin（给 capstone 用）。
 * 用 read_mem（内部 process_vm_readv，失败才退 /proc/pid/mem），不做 PEEKTEXT 逐字读。 */
static void code_dump(pid_t tid, const char *label, const char *fname,
                      uint64_t addr, size_t len) {
    if (!len) return;
    if (len > 65536) len = 65536;
    uint8_t *buf = (uint8_t *)malloc(len);
    if (!buf) { out("[code] malloc(%zu) 失败: %s\n", len, strerror(errno)); return; }
    int bad = 0;
    read_mem(tid, addr, buf, len, &bad, label);
    /* 全 0 提示：说明这页没被解密过（未执行过），别当有效指令 */
    int allzero = 1, allcc = 1;
    for (size_t i = 0; i < len; i++) {
        if (buf[i] != 0x00) allzero = 0;
        if (buf[i] != 0xCC) allcc = 0;
        if (!allzero && !allcc) break;
    }
    out("\n---- %s ---- 0x%llx (+0x%zx)%s%s\n", label, (unsigned long long)addr, len,
        bad ? "  [部分页不可读]" : "",
        allzero ? "  [警告: 全 0 ⇒ 该页未解密/未执行过]" : "");
    for (size_t o = 0; o < len; o += 32) {
        out("  +0x%04zx 0x%llx |", o, (unsigned long long)(addr + o));
        for (size_t j = 0; j < 32 && o + j < len; j++) out(" %02x", buf[o + j]);
        out("\n");
    }
    const char *dir = g_dumpdir ? g_dumpdir : g_codedir;
    if (dir[0]) {
        char path[512];
        snprintf(path, sizeof path, "%s/%s", dir, fname);
        FILE *f = fopen(path, "wb");
        if (!f) out("[code] 写 %s 失败: %s\n", path, strerror(errno));
        else {
            size_t w = fwrite(buf, 1, len, f);
            fclose(f);
            out("[code] bin -> %s (%zu B%s)\n", path, w, w == len ? "" : " 写入不完整!");
        }
    }
    free(buf);
}

static int dump_crash(pid_t tid, uint64_t extra_addr) {
    struct ptregs R; memset(&R, 0, sizeof R);
    struct iovec iov = { &R, sizeof R };
    errno = 0;
    if (ptrace(PTRACE_GETREGSET, tid, (void *)(long)NT_PRSTATUS, &iov) != 0) {
        out("[regs] PTRACE_GETREGSET(tid %d) 失败: %s\n", (int)tid, strerror(errno));
        return -1;
    }
    char comm[64]; read_comm(g_pid ? g_pid : tid, tid, comm, sizeof comm);
    out("\n================ CRASH (pid %d, tid %d, comm=%s) ================\n",
        (int)(g_pid ? g_pid : tid), (int)tid, comm);
    for (int i = 0; i < 31; i++)
        out("  x%-2d = 0x%016llx%s\n", i, (unsigned long long)R.regs[i], (i % 2) ? "" : "    ");
    out("  sp  = 0x%016llx\n  pc  = 0x%016llx\n  pstate = 0x%016llx\n",
        (unsigned long long)R.sp, (unsigned long long)R.pc, (unsigned long long)R.pstate);
    uint64_t off = 0; const char *perm = NULL;
    if (in_il2cpp(R.pc, &off, &perm))
        out("  pc 落在 libil2cpp+0x%llx (%s)\n", (unsigned long long)off, perm);
    else out("  pc 不在 libil2cpp 映射内（可能已切到别的 so/系统库）\n");

    uint64_t x19 = R.regs[19], x24 = R.regs[24];
    out("\n-- 关键寄存器 --\n  x19 = 0x%016llx (调用方栈帧候选)\n  x24 = 0x%016llx\n  x19 - sp = 0x%llx\n",
        (unsigned long long)x19, (unsigned long long)x24, (unsigned long long)(x19 - R.sp));

    hexdump(tid, "a) sp .. sp+0x800", R.sp, 0x800);
    hexdump(tid, "b) x19-0x40 .. x19+0x200", x19 - 0x40, 0x240);
    out("\n-- x19 结构体关键字段 --\n");
    show_u64(tid, "x19+0x28", x19 + 0x28);
    show_u64(tid, "x19+0x30", x19 + 0x30);
    show_u64(tid, "x19+0x40", x19 + 0x40);
    show_u64(tid, "x19+0x1c8", x19 + 0x1c8);
    show_u64(tid, "x19+0x1d0", x19 + 0x1d0);
    show_u64(tid, "x19+0x1d8", x19 + 0x1d8);

    uint64_t p40 = 0;
    if (read_u64(tid, x19 + 0x40, &p40) == 0 && p40)
        hexdump(tid, "c) [x19+0x40] .. +0x40", p40, 0x40);
    else out("\n---- c) [x19+0x40] ---- 读取失败或为 0，跳过\n");
    if (x24) {
        hexdump(tid, "c) x24 .. +0x40", x24, 0x40);
        uint32_t cnt = 0;
        if (read_mem(tid, x24 + 0x1c, &cnt, 4, NULL, "count") == 4)
            out("  x24+0x1c (count?) = %u (0x%x)\n", cnt, cnt);
    }
    if (extra_addr) hexdump(tid, "e) --addr", extra_addr, 0x100);

    out("\n---- d) sp .. sp+0x1000 内 libil2cpp 候选返回地址 ----\n");
    uint8_t win[0x1000]; int bad = 0;
    read_mem(tid, R.sp, win, sizeof win, &bad, "scan");
    if (bad) out("(扫描窗口有 %d 页不可读，已填 0xCC)\n", bad);
    int hits = 0;
    uint64_t cand[MAX_CODE_CANDS];
    int ncand = 0;
    for (size_t o = 0; o + 8 <= sizeof win; o += 8) {
        uint64_t v; memcpy(&v, win + o, 8);
        uint64_t o2 = 0; const char *pm2 = NULL;
        if (in_il2cpp(v, &o2, &pm2)) {
            out("  [sp+0x%03zx] = 0x%016llx  -> libil2cpp+0x%llx (%s)\n",
                o, (unsigned long long)v, (unsigned long long)o2, pm2);
            /* 候选返回地址进代码 dump 队列（去重，最多 32 个） */
            int dup = 0;
            for (int k = 0; k < ncand; k++) if (cand[k] == v) { dup = 1; break; }
            if (!dup && ncand < MAX_CODE_CANDS) cand[ncand++] = v;
            if (++hits > 64) { out("  ... (超过 64 条，截断)\n"); break; }
        }
    }
    if (!hits) out("  (无命中)\n");

    /* ---- e) code：崩溃瞬间的代码页（libil2cpp 按需解密，事后读会是全 0）---- */
    g_dumpdir = g_codedir;
    out("\n\n---- e) code ----\n");
    if (g_codedir[0]) out("[code] 二进制落盘目录: %s\n", g_codedir);
    else out("[code] 无落盘目录（只写 txt）\n");
    if (R.pc >= 0x2000) {
        char fn[64];
        snprintf(fn, sizeof fn, "pc_%llx.bin", (unsigned long long)(R.pc - 0x2000));
        code_dump(tid, "e) pc-0x2000 .. pc+0x2000", fn, R.pc - 0x2000, 0x4000);
    } else {
        out("[code] pc=0x%llx < 0x2000，无法回退 16KB\n", (unsigned long long)R.pc);
    }
    for (int k = 0; k < ncand; k++) {
        char label[96], fn[64];
        snprintf(label, sizeof label, "e) ret#%d 0x%llx -0x40..+0x40", k,
                 (unsigned long long)cand[k]);
        snprintf(fn, sizeof fn, "ret%02d_%llx.bin", k, (unsigned long long)(cand[k] - 0x40));
        code_dump(tid, label, fn, cand[k] - 0x40, 0x80);
    }
    out("\n[code] 共落 %d 个候选返回地址代码块（上限 %d）\n", ncand, MAX_CODE_CANDS);
    out("\n================ END (tid %d) ================\n", (int)tid);
    return 0;
}

/* ---------- SEIZE / INTERRUPT / DETACH ---------- */
static int seize_one(pid_t tid) {
    errno = 0;
    if (ptrace(PTRACE_SEIZE, tid, NULL, (void *)(long)PTRACE_O_TRACESYSGOOD) != 0) {
        out("[seize] tid %d PTRACE_SEIZE 失败: %s (errno=%d)\n", (int)tid, strerror(errno), errno);
        return -1;
    }
    return 0;
}

/* 请求某个 tid 进入 ptrace-stop，并返回它是因为什么停的：
 *   返回 0 = 由我们的 INTERRUPT 造成（内部 stop，不得回投信号）
 *   返回 >0 = 真实信号号（DETACH 时要回投，避免吞掉真实信号）
 *   返回 -1 = 拿不到 stop（线程已退出等） */
static int interrupt_and_wait(pid_t tid) {
    for (int attempt = 0; attempt < 50; attempt++) {
        int st = 0;
        errno = 0;
        pid_t w = waitpid(tid, &st, __WALL | WNOHANG);
        if (w == tid) {
            if (WIFEXITED(st) || WIFSIGNALED(st)) return -1;
            if (WIFSTOPPED(st)) {
                int ev = st >> 16;
                if (ev == PTRACE_EVENT_STOP) return 0;
                return WSTOPSIG(st);
            }
        }
        errno = 0;
        if (ptrace(PTRACE_INTERRUPT, tid, NULL, NULL) != 0 && errno != ESRCH)
            out("[interrupt] tid %d 失败: %s\n", (int)tid, strerror(errno));
        struct timespec ts = { 0, 5 * 1000 * 1000 };
        nanosleep(&ts, NULL);
        errno = 0;
        w = waitpid(tid, &st, __WALL | WNOHANG);
        if (w == tid) {
            if (WIFEXITED(st) || WIFSIGNALED(st)) return -1;
            if (WIFSTOPPED(st)) {
                int ev = st >> 16;
                if (ev == PTRACE_EVENT_STOP) return 0;
                return WSTOPSIG(st);
            }
        }
    }
    return -1;
}

static void detach_all(pid_t crash_tid, int crash_sig) {
    int done = 0, gone = 0;
    /* ★ 2026-11-19 血案：释放前**必须先撤掉 BRK 断点**，否则原指令被永久替换成 0xD4200000，
     * 目标一恢复执行就吃 SIGTRAP（默认动作=终止）⇒ 进程莫名其妙 DEAD。实测就是这么死的。 */
    if (g_brk_armed && g_ntids > 0) {
        pid_t t0 = g_tids[0];
        int s0 = interrupt_and_wait(t0);
        if (s0 >= 0) {
            out("[brk] 释放前撤销断点（在 tid %d 上恢复原指令）\n", (int)t0);
            brk_restore(t0);
        } else {
            out("[brk] 释放前无法停下 tid %d 撤点（errno 见上）\n", (int)t0);
        }
    }
    for (int i = 0; i < g_ntids; i++) {
        pid_t t = g_tids[i];
        if (t == crash_tid) {
            /* 崩溃线程正处在信号投递 stop：按原信号回投，让它照原样死 */
            errno = 0;
            if (ptrace(PTRACE_DETACH, t, NULL, (void *)(long)crash_sig) != 0 && errno != ESRCH)
                out("[detach] crash tid %d 失败: %s\n", (int)t, strerror(errno));
            done++;
            continue;
        }
        int s = interrupt_and_wait(t);
        if (s < 0) { gone++; continue; }
        /* SIGTRAP 只可能来自我们自己的 BRK 或单步，一律不回投（回投 = 杀进程） */
        if (s == SIGTRAP) s = 0;
        errno = 0;
        if (ptrace(PTRACE_DETACH, t, NULL, (void *)(long)s) != 0 && errno != ESRCH) {
            out("[detach] tid %d 失败: %s\n", (int)t, strerror(errno));
        } else done++;
    }
    out("[detach] %d 个线程已 DETACH（其中 crash_tid=%d 按信号 %d 回投），%d 个已自然退出\n",
        done, (int)crash_tid, crash_sig, gone);

    /* 复核 + 兜底：仍有 T 就补 SIGCONT，最多 3 轮 */
    for (int round = 0; round < 3; round++) {
        print_states(round == 0 ? "detach 后" : "detach 后(复核)");
        /* 若已无 T 就结束；只剩「崩溃线程」在 T 属预期（它正在走信号投递/死亡路径） */
        int stillT = 0, onlyCrashT = 1;
        pid_t tlist[64]; int tn = 0;
        char path[64];
        snprintf(path, sizeof path, "/proc/%d/task", (int)g_pid);
        DIR *d = opendir(path);
        if (!d) break;
        struct dirent *e;
        while ((e = readdir(d)) != NULL) {
            if (e->d_name[0] < '0' || e->d_name[0] > '9') continue;
            char sp[128];
            snprintf(sp, sizeof sp, "/proc/%d/task/%s/stat", (int)g_pid, e->d_name);
            FILE *f = fopen(sp, "r");
            if (!f) continue;
            char buf[512];
            if (fgets(buf, sizeof buf, f)) {
                char *p = strrchr(buf, ')');
                if (p && (p[2] == 'T' || p[2] == 't')) {
                    stillT++;
                    pid_t tt = (pid_t)atoi(e->d_name);
                    if (tn < 64) tlist[tn++] = tt;
                    if (crash_tid == 0 || tt != crash_tid) onlyCrashT = 0;
                }
            }
            fclose(f);
        }
        closedir(d);
        if (!stillT) break;
        if (onlyCrashT && crash_tid) {
            out("[recover] 仅剩崩溃线程 tid=%d 停在 T —— 它正在走信号投递/死亡路径，",
                (int)crash_tid);
            out("此时处于 group-stop 属**不可恢复且预期**，不是失败，也不再补 SIGCONT。\n");
            break;
        }
        out("[recover] 仍有 %d 个线程在 T（tids:", stillT);
        for (int i = 0; i < tn; i++) out(" %d", (int)tlist[i]);
        out("）⇒ 补 SIGCONT（第 %d 轮）\n", round + 1);
        errno = 0;
        if (kill(g_pid, SIGCONT) != 0 && errno != ESRCH)
            out("[recover] kill(pid,SIGCONT) 失败: %s\n", strerror(errno));
        struct timespec ts = { 0, 100 * 1000 * 1000 };
        nanosleep(&ts, NULL);
    }
}

static int list_threads(pid_t pid) {
    char path[64];
    snprintf(path, sizeof path, "/proc/%d/task", (int)pid);
    DIR *d = opendir(path);
    if (!d) { out("[threads] 打不开 %s: %s\n", path, strerror(errno)); return -1; }
    struct dirent *e;
    g_ntids = 0;
    while ((e = readdir(d)) != NULL) {
        if (e->d_name[0] < '0' || e->d_name[0] > '9') continue;
        if (g_ntids < MAX_TIDS) g_tids[g_ntids++] = (pid_t)atoi(e->d_name);
    }
    closedir(d);
    return g_ntids;
}

/* ---------- v5：入口断点 BRK #0 ---------- */
static int mem_write(pid_t pid, uint64_t addr, const void *buf, size_t len) {
    char p[64];
    snprintf(p, sizeof p, "/proc/%d/mem", (int)pid);
    errno = 0;
    int fd = open(p, O_WRONLY);
    if (fd < 0) { out("[brk] open %s 失败: %s\n", p, strerror(errno)); return -1; }
    errno = 0;
    ssize_t n = pwrite(fd, buf, len, (off_t)addr);
    int err = errno;
    close(fd);
    if (n == (ssize_t)len) return 0;
    out("[brk] /proc/pid/mem 写 0x%llx 失败: %s\n", (unsigned long long)addr, strerror(err));
    return -1;
}

/* 在已处于 ptrace-stop 的线程上布点：POKEDATA 写 BRK #0（0xD4200000）；失败退 /proc/pid/mem */
static int brk_arm(pid_t tid) {
    uint64_t wa = g_brk_addr & ~7ULL;
    unsigned off = (unsigned)(g_brk_addr & 7) * 8;
    errno = 0;
    long word = ptrace(PTRACE_PEEKDATA, tid, (void *)(uintptr_t)wa, NULL);
    if (word == -1 && errno) { out("[brk] PEEKDATA 0x%llx 失败: %s\n", (unsigned long long)wa, strerror(errno)); return -1; }
    g_brk_waddr = wa;
    g_brk_orig = (uint64_t)word;
    uint64_t patched = (g_brk_orig & ~(0xFFFFFFFFULL << off)) | (0xD4200000ULL << off);
    errno = 0;
    if (ptrace(PTRACE_POKEDATA, tid, (void *)(uintptr_t)wa, (void *)(uintptr_t)patched) != 0) {
        out("[brk] POKEDATA 失败: %s ⇒ 退 /proc/pid/mem 写\n", strerror(errno));
        uint64_t w = patched;
        if (mem_write(g_pid, wa, &w, 8) != 0) return -1;
    }
    g_brk_armed = 1;
    out("[brk] 布点 OK @0x%llx：字@0x%llx 原=%016llx 新=%016llx（BRK #0）\n",
        (unsigned long long)g_brk_addr, (unsigned long long)wa,
        (unsigned long long)g_brk_orig, (unsigned long long)patched);
    return 0;
}

static int brk_restore(pid_t tid) {
    if (!g_brk_armed) return 0;
    errno = 0;
    if (ptrace(PTRACE_POKEDATA, tid, (void *)(uintptr_t)g_brk_waddr, (void *)(uintptr_t)g_brk_orig) != 0) {
        out("[brk] 恢复 POKEDATA 失败: %s ⇒ 退 /proc/pid/mem 写\n", strerror(errno));
        uint64_t w = g_brk_orig;
        if (mem_write(g_pid, g_brk_waddr, &w, 8) != 0) return -1;
    }
    errno = 0;
    long v = ptrace(PTRACE_PEEKDATA, tid, (void *)(uintptr_t)g_brk_waddr, NULL);
    g_brk_armed = 0;
    int ok = ((uint64_t)v == g_brk_orig);
    out("[brk] 恢复原指令：回读=%016llx 期望=%016llx %s\n",
        (unsigned long long)(uint64_t)v, (unsigned long long)g_brk_orig, ok ? "✓" : "✗(不一致!)");
    return ok ? 0 : -1;
}

/* 命中处理：全寄存器（x30 = 调用者返回地址）+ [x0] 附近 0x100 B + 栈顶 */
static void brk_hit(pid_t tid) {
    struct ptregs R; memset(&R, 0, sizeof R);
    struct iovec iov = { &R, sizeof R };
    errno = 0;
    if (ptrace(PTRACE_GETREGSET, tid, (void *)(long)NT_PRSTATUS, &iov) != 0) {
        out("[brk] GETREGSET 失败: %s\n", strerror(errno));
        return;
    }
    char comm[64]; read_comm(g_pid, tid, comm, sizeof comm);
    g_brk_hits++;
    out("\n============ BRK HIT #%d (pid %d, tid %d, comm=%s) ============\n",
        g_brk_hits, (int)g_pid, (int)tid, comm);
    out("  pc    = 0x%016llx   （断点 0x%llx）\n", (unsigned long long)R.pc, (unsigned long long)g_brk_addr);
    out("  **x30 = 0x%016llx   <- 调用者返回地址（caller LR）**\n", (unsigned long long)R.regs[30]);
    uint64_t off = 0; const char *pm = NULL;
    if (in_il2cpp(R.regs[30], &off, &pm))
        out("      x30 落在 libil2cpp+0x%llx (%s)\n", (unsigned long long)off, pm);
    for (int i = 0; i < 30; i++)
        out("  x%-2d  = 0x%016llx%s\n", i, (unsigned long long)R.regs[i], (i % 2) ? "" : "    ");
    out("  sp    = 0x%016llx\n  pstate = 0x%016llx\n",
        (unsigned long long)R.sp, (unsigned long long)R.pstate);
    out("  x0    = 0x%016llx （结构体指针候选）\n", (unsigned long long)R.regs[0]);
    if (R.regs[0]) {
        out("\n-- [x0]-0x40 .. [x0]+0xc0（0x100 B）--\n");
        hexdump(tid, "x0 struct", R.regs[0] - 0x40, 0x100);
        show_u64(tid, "x0+0x1c", R.regs[0] + 0x1c);
        show_u64(tid, "x0+0x28", R.regs[0] + 0x28);
        show_u64(tid, "x0+0x30", R.regs[0] + 0x30);
    } else out("  x0 = 0，跳过结构体 dump\n");
    out("\n-- 栈顶 sp-0x40 .. sp+0x100 --\n");
    hexdump(tid, "stack top", R.sp - 0x40, 0x140);

    /* v6：调用者（x30）那几行指令 —— 崩溃现场限定，事后代码页会被回收/全 0 */
    g_dumpdir = g_brkcode[0] ? g_brkcode : g_codedir;
    uint64_t lr = R.regs[30];
    if (lr) {
        uint64_t page = lr & ~0xFFFULL;
        char fn[64];
        out("\n[cfg] x30 = 0x%llx（页 0x%llx，页内偏移 0x%llx）\n",
            (unsigned long long)lr, (unsigned long long)page, (unsigned long long)(lr - page));
        snprintf(fn, sizeof fn, "x30_%llx.bin", (unsigned long long)(lr - 0x40));
        code_dump(tid, "x30-0x40 .. x30+0x40（调用者指令）", fn, lr - 0x40, 0x80);
        snprintf(fn, sizeof fn, "page_%llx.bin", (unsigned long long)page);
        code_dump(tid, "x30 所在页（4KB）", fn, page, 0x1000);
    } else {
        out("\n[cfg] x30 = 0，无法 dump 调用者指令页\n");
    }
    g_dumpdir = g_codedir;
    out("\n============ BRK HIT END (tid %d) ============\n", (int)tid);
}

static void usage(const char *a0) {
    fprintf(stderr,
        "用法: %s -p <pid|tid> [--all] [--addr <hex>] [--timeout <sec>] [--out <path>]\n"
        "  -p <pid>        目标 pid（单线程模式时填 tid）\n"
        "  --all           对 /proc/<pid>/task/* 全部线程逐个 PTRACE_SEIZE\n"
        "  --keep          dump 后继续值守（同一 tid 只 dump 一次）；默认 dump 一次即退出\n"
        "  --brk <hex>     入口断点：在绝对地址写 BRK #0，命中即打全寄存器（x30=调用者返回地址）\n"
        "                  + [x0]±0x100 结构体 + 栈顶，然后恢复原指令并释放\n"
        "  --brk-continue  命中后不释放：恢复原指令→单步→重新布点，继续值守（可连续抓）\n"
        "  --pc-range <hex>-<hex>  只 dump pc 落在该区间的崩溃（默认自动取 libil2cpp 的 r-xp 段）\n"
        "  --any-pc        关闭 pc 过滤（任何 pc 都 dump，慎用）\n"
        "  --skip-comm <名>        追加要跳过的线程 comm 子串（大小写不敏感，可重复）\n"
        "  --no-skip-comm  清空内置广告 SDK 名单（anythink/pangle/unityads/...）\n"
        "  --addr <hex>    额外 dump 一段地址（0x100 字节）\n"
        "  --timeout <sec> 无致命信号多久释放（默认 120，0=不超时）\n"
        "  --out <path>    输出文件（默认 /data/local/tmp/wxsj_ptrace_<pid>.txt）\n", a0);
}

static double now_s(void) {
    struct timespec ts; clock_gettime(CLOCK_MONOTONIC, &ts);
    return (double)ts.tv_sec + (double)ts.tv_nsec / 1e9;
}

static void init_default_skip_comm(void) {
    static const char *defs[] = {
        "anythink", "pangle", "unityads", "applovin", "mintegral", "topon",
        "bytedance", "toutiao", "sigmob", "klevin", "ksad", "gdt", "csj",
    };
    for (size_t i = 0; i < sizeof(defs) / sizeof(defs[0]) && g_skip_comm_n < 32; i++)
        g_skip_comm[g_skip_comm_n++] = defs[i];
}

/* comm 子串匹配，大小写不敏感 */
static int comm_skipped(const char *comm) {
    char lc[64];
    size_t n = strlen(comm);
    if (n >= sizeof lc) n = sizeof lc - 1;
    for (size_t i = 0; i < n; i++) {
        char c = comm[i];
        lc[i] = (c >= 'A' && c <= 'Z') ? (char)(c - 'A' + 'a') : c;
    }
    lc[n] = 0;
    for (int i = 0; i < g_skip_comm_n; i++) {
        char pat[64];
        size_t m = strlen(g_skip_comm[i]);
        if (m >= sizeof pat) m = sizeof pat - 1;
        for (size_t j = 0; j < m; j++) {
            char c = g_skip_comm[i][j];
            pat[j] = (c >= 'A' && c <= 'Z') ? (char)(c - 'A' + 'a') : c;
        }
        pat[m] = 0;
        if (strstr(lc, pat)) return 1;
    }
    return 0;
}

static int tid_already_dumped(pid_t tid) {
    for (int i = 0; i < g_dumped_n; i++) if (g_dumped[i] == tid) return 1;
    return 0;
}

static void mark_dumped(pid_t tid) {
    if (g_dumped_n < MAX_TIDS) g_dumped[g_dumped_n++] = tid;
}

static int is_fatal(int sig) {
    return sig == SIGSEGV || sig == SIGBUS || sig == SIGABRT || sig == SIGILL || sig == SIGFPE;
}

int main(int argc, char **argv) {
    pid_t pid = 0;
    uint64_t extra = 0;
    int timeout = 120, all = 0;
    const char *outpath = NULL;
    for (int i = 1; i < argc; i++) {
        if (!strcmp(argv[i], "-p") && i + 1 < argc) pid = (pid_t)atoi(argv[++i]);
        else if (!strcmp(argv[i], "--all")) all = 1;
        else if (!strcmp(argv[i], "--addr") && i + 1 < argc) extra = strtoull(argv[++i], NULL, 16);
        else if (!strcmp(argv[i], "--timeout") && i + 1 < argc) timeout = atoi(argv[++i]);
        else if (!strcmp(argv[i], "--out") && i + 1 < argc) outpath = argv[++i];
        else if (!strcmp(argv[i], "--keep")) g_keep = 1;
        else if (!strcmp(argv[i], "--brk") && i + 1 < argc) g_brk_addr = strtoull(argv[++i], NULL, 16);
        else if (!strcmp(argv[i], "--brk-continue")) g_brk_continue = 1;
        else if (!strcmp(argv[i], "--any-pc")) g_pc_filter = 0;
        else if (!strcmp(argv[i], "--no-skip-comm")) g_no_skip_defaults = 1;
        else if (!strcmp(argv[i], "--skip-comm") && i + 1 < argc) {
            if (g_skip_comm_n < 32) g_skip_comm[g_skip_comm_n++] = argv[++i]; else i++;
        }
        else if (!strcmp(argv[i], "--pc-range") && i + 1 < argc) {
            const char *s = argv[++i];
            unsigned long long a = 0, b = 0;
            if (sscanf(s, "%llx-%llx", &a, &b) != 2) {
                fprintf(stderr, "--pc-range 格式应为 <start>-<end>（hex），收到: %s\n", s);
                return 2;
            }
            g_pc_lo = a; g_pc_hi = b; g_pc_filter = 1;
        }
        else if (!strcmp(argv[i], "-h") || !strcmp(argv[i], "--help")) { usage(argv[0]); return 0; }
        else { fprintf(stderr, "未知参数: %s\n", argv[i]); usage(argv[0]); return 2; }
    }
    if (pid <= 0) { usage(argv[0]); return 2; }
    if (!g_no_skip_defaults) init_default_skip_comm();
    g_pid = pid;

    char defpath[128];
    if (!outpath) {
        snprintf(defpath, sizeof defpath, "/data/local/tmp/wxsj_ptrace_%d.txt", (int)pid);
        outpath = defpath;
    }
    g_out = fopen(outpath, "w");
    if (!g_out) fprintf(stderr, "[out] 打开 %s 失败: %s（继续只打印到 stdout）\n", outpath, strerror(errno));
    /* 代码二进制目录：<out 去掉 .txt>_code/ */
    {
        char base[256];
        snprintf(base, sizeof base, "%s", outpath);
        size_t l = strlen(base);
        if (l > 4 && !strcmp(base + l - 4, ".txt")) base[l - 4] = 0;
        snprintf(g_codedir, sizeof g_codedir, "%s_code", base);
        if (mkdir(g_codedir, 0755) != 0 && errno != EEXIST)
            out("[code] mkdir %s 失败: %s（代码只进 txt）\n", g_codedir, strerror(errno));
        snprintf(g_brkcode, sizeof g_brkcode, "%s_brkcode", base);
        if (mkdir(g_brkcode, 0755) != 0 && errno != EEXIST)
            out("[code] mkdir %s 失败: %s（BRK 代码块只进 txt）\n", g_brkcode, strerror(errno));
    }

    out("# wxsj_ptrace_dump v2(SEIZE)  pid=%d  mode=%s  out=%s\n",
        (int)pid, all ? "all-threads" : "single-tid", outpath);
    print_states("attach 前");
    load_maps(pid);
    if (g_pc_filter) {
        out("[cfg] pc 过滤: %d 段 libil2cpp r-xp", g_pcx_n);
        for (int i = 0; i < g_pcx_n; i++)
            out(" [0x%llx,0x%llx)", (unsigned long long)g_pcx[i].lo, (unsigned long long)g_pcx[i].hi);
        if (g_cfg_set)
            out(" + --pc-range [0x%llx,0x%llx)", (unsigned long long)g_cfg_lo, (unsigned long long)g_cfg_hi);
        out("\n");
        if (!g_pcx_n && !g_cfg_set)
            out("[cfg] 警告：既无 libil2cpp r-xp 段也无 --pc-range ⇒ 崩溃事件将全部 skip（--any-pc 可关闭过滤）\n");
    } else {
        out("[cfg] pc 过滤: 关闭（--any-pc）\n");
    }
    if (g_brk_addr)
        out("[cfg] 入口断点: 0x%llx (BRK #0)，命中后 %s\n", (unsigned long long)g_brk_addr,
            g_brk_continue ? "重新布点继续值守（--brk-continue）" : "恢复原指令并释放");
    out("[cfg] keep=%d  skip_comm(%d):", g_keep, g_skip_comm_n);
    for (int i = 0; i < g_skip_comm_n; i++) out(" %s", g_skip_comm[i]);
    out("\n");

    int seized = 0;
    if (all) {
        if (list_threads(pid) <= 0) { out("[threads] 没枚举到线程，退出\n"); if (g_out) fclose(g_out); return 1; }
        out("[threads] 共 %d 个线程：\n", g_ntids);
        for (int i = 0; i < g_ntids; i++) {
            char comm[64]; read_comm(pid, g_tids[i], comm, sizeof comm);
            out("  tid %-8d comm=%s\n", (int)g_tids[i], comm);
        }
        for (int i = 0; i < g_ntids; i++) if (seize_one(g_tids[i]) == 0) seized++;
        out("[seize] 成功 SEIZE %d/%d 个线程（SEIZE 不改变运行态，无需 CONT）\n", seized, g_ntids);
    } else {
        if (seize_one(pid) != 0) { if (g_out) fclose(g_out); return 1; }
        g_tids[0] = pid; g_ntids = 1; seized = 1;
        out("[seize] ok（单线程模式）\n");
    }
    if (!seized) { if (g_out) fclose(g_out); return 1; }
    print_states("seize 后");

    /* v5：入口断点布点（在任意一个已 SEIZE 的线程上做，POKEDATA 需目标处于 ptrace-stop） */
    if (g_brk_addr) {
        pid_t t = g_tids[0];
        int s = interrupt_and_wait(t);
        if (s < 0) {
            out("[brk] tid %d 无法进入 ptrace-stop，布点失败\n", (int)t);
        } else {
            if (brk_arm(t) == 0)
                out("[brk] 目标地址 0x%llx（4 字节对齐: %s）\n", (unsigned long long)g_brk_addr,
                    (g_brk_addr & 3) ? "否 ⚠" : "是");
            errno = 0;
            if (ptrace(PTRACE_CONT, t, NULL, (void *)(long)(s > 0 ? s : 0)) != 0)
                out("[brk] CONT tid %d 失败: %s\n", (int)t, strerror(errno));
        }
        if (!g_brk_armed) { out("[brk] 布点未生效，退出\n"); detach_all(0, 0); if (g_out) fclose(g_out); return 1; }
    }

    double t0 = now_s();
    int st = 0;
    for (;;) {
        errno = 0;
        pid_t w = waitpid(-1, &st, __WALL | WNOHANG);
        if (w > 0) {
            if (WIFEXITED(st)) { out("[exit] tid %d 正常退出 code=%d\n", (int)w, WEXITSTATUS(st)); }
            else if (WIFSIGNALED(st)) { out("[exit] tid %d 被信号 %d 终止\n", (int)w, WTERMSIG(st)); }
            else if (WIFSTOPPED(st)) {
                int sig = WSTOPSIG(st), ev = st >> 16;
                if (ev == PTRACE_EVENT_STOP) {
                    out("[stop] tid=%d 内部 INTERRUPT stop（不转发）\n", (int)w);
                    continue;
                }
                /* v5：入口断点命中（BRK #0 触发 SIGTRAP，pc 落在断点处或其 +4） */
                if (g_brk_armed && sig == SIGTRAP && ev == 0) {
                    uint64_t bpc = 0;
                    struct ptregs rb; memset(&rb, 0, sizeof rb);
                    struct iovec ib = { &rb, sizeof rb };
                    if (ptrace(PTRACE_GETREGSET, w, (void *)(long)NT_PRSTATUS, &ib) == 0) bpc = rb.pc;
                    if (bpc == g_brk_addr || bpc == g_brk_addr + 4) {
                        /* ★顺序不能反（2026-11-19 lead 要求）：先 dump 完，再恢复原指令。
                         * 若进程在 dump 期间就要崩，先恢复会白恢复；而且恢复失败（ESRCH）也不影响
                         * 已经拿到的寄存器/代码页。 */
                        brk_hit(w);
                        if (brk_restore(w) != 0)
                            out("[brk] 恢复原指令失败/未确认（可能进程已在死亡路径）——已拿到的数据不受影响\n");
                        if (g_brk_continue) {
                            /* 恢复原指令 → 单步执行它 → 重新布点 → 继续 */
                            errno = 0;
                            if (ptrace(PTRACE_SINGLESTEP, w, NULL, NULL) != 0) {
                                out("[brk] SINGLESTEP 失败: %s ⇒ 不再布点\n", strerror(errno));
                                if (ptrace(PTRACE_CONT, w, NULL, NULL) != 0)
                                    out("[brk] CONT 失败: %s\n", strerror(errno));
                                continue;
                            }
                            int st3 = 0;
                            if (waitpid(w, &st3, __WALL) == w) {
                                if (!WIFSTOPPED(st3))
                                    out("[brk] 单步后非 stop（status=0x%x）\n", st3);
                            }
                            if (brk_arm(w) != 0) out("[brk] 重新布点失败，转普通值守\n");
                            if (ptrace(PTRACE_CONT, w, NULL, NULL) != 0)
                                out("[brk] CONT 失败: %s\n", strerror(errno));
                            continue;
                        }
                        out("[brk] 命中一次即释放（非 --brk-continue）\n");
                        detach_all(0, 0);
                        if (g_out) { fclose(g_out); g_out = NULL; }
                        return 0;
                    }
                    out("[skip] tid=%d SIGTRAP 但 pc=0x%llx 非断点 0x%llx ⇒ 原样转发\n",
                        (int)w, (unsigned long long)bpc, (unsigned long long)g_brk_addr);
                    if (ptrace(PTRACE_CONT, w, NULL, (void *)(long)sig) != 0)
                        out("[cont] tid %d CONT 失败: %s\n", (int)w, strerror(errno));
                    continue;
                }
                if (is_fatal(sig)) {
                    char comm[64]; read_comm(g_pid, w, comm, sizeof comm);
                    /* 先取 pc 判断是否值得 dump（不满足就原样转发、继续值守） */
                    uint64_t fpc = 0;
                    struct ptregs rq; memset(&rq, 0, sizeof rq);
                    struct iovec iq = { &rq, sizeof rq };
                    if (ptrace(PTRACE_GETREGSET, w, (void *)(long)NT_PRSTATUS, &iq) == 0) fpc = rq.pc;
                    if (comm_skipped(comm)) {
                        out("[skip] tid=%d comm=%s 命中 --skip-comm 名单（sig=%d pc=0x%llx）⇒ 原样转发继续盯\n",
                            (int)w, comm, sig, (unsigned long long)fpc);
                        if (ptrace(PTRACE_CONT, w, NULL, (void *)(long)sig) != 0)
                            out("[cont] tid %d CONT 失败: %s\n", (int)w, strerror(errno));
                        continue;
                    }
                    if (g_pc_filter && !g_pcx_n && !g_cfg_set) {
                        /* 拿不到 libil2cpp 可执行段时**不猜**：一律跳过（fail-safe），
                         * 否则会退化成「什么 pc 都 dump」，又踩 2026-11-19 广告 SDK 假阳性。 */
                        if (!g_pc_unknown_warned) {
                            out("[warn] 未取到 libil2cpp 可执行段 ⇒ pc 过滤无法生效，本次一律跳过崩溃事件"
                                "（如确要无过滤 dump，请显式加 --any-pc）\n");
                            g_pc_unknown_warned = 1;
                        }
                        out("[skip] tid=%d pc=0x%llx 无可用范围（comm=%s sig=%d）⇒ 原样转发继续盯\n",
                            (int)w, (unsigned long long)fpc, comm, sig);
                        if (ptrace(PTRACE_CONT, w, NULL, (void *)(long)sig) != 0)
                            out("[cont] tid %d CONT 失败: %s\n", (int)w, strerror(errno));
                        continue;
                    }
                    if (g_pc_filter && !in_range(fpc)) {
                        out("[skip] tid=%d pc=0x%llx 不在关注区间（comm=%s sig=%d）⇒ 原样转发继续盯\n",
                            (int)w, (unsigned long long)fpc, comm, sig);
                        if (ptrace(PTRACE_CONT, w, NULL, (void *)(long)sig) != 0)
                            out("[cont] tid %d CONT 失败: %s\n", (int)w, strerror(errno));
                        continue;
                    }
                    if (tid_already_dumped(w)) {
                        out("[skip] tid=%d 已 dump 过（--keep 模式只 dump 一次）sig=%d ⇒ 原样转发\n", (int)w, sig);
                        if (ptrace(PTRACE_CONT, w, NULL, (void *)(long)sig) != 0)
                            out("[cont] tid %d CONT 失败: %s\n", (int)w, strerror(errno));
                        continue;
                    }
                    out("[hit] 捕获致命信号 %d @ tid=%d comm=%s pc=0x%llx（在 libil2cpp 范围）⇒ 取证\n",
                        sig, (int)w, comm, (unsigned long long)fpc);
                    if (dump_crash(w, extra) != 0) out("[dump] 取证失败（见上）\n");
                    mark_dumped(w);
                    g_dump_total++;
                    if (g_keep) {
                        out("[keep] dump 完成，继续值守（本次 dump 第 %d 次）\n", g_dump_total);
                        if (ptrace(PTRACE_CONT, w, NULL, (void *)(long)sig) != 0)
                            out("[cont] tid %d CONT 失败: %s\n", (int)w, strerror(errno));
                        continue;
                    }
                    detach_all(w, sig);
                    if (g_out) { fclose(g_out); g_out = NULL; }
                    return 0;
                }
                /* 真实信号：原样转发（绝不吞） */
                out("[sig] tid=%d 真实信号 %d ⇒ 原样转发\n", (int)w, sig);
                errno = 0;
                if (ptrace(PTRACE_CONT, w, NULL, (void *)(long)sig) != 0)
                    out("[cont] tid %d CONT 失败: %s\n", (int)w, strerror(errno));
                continue;
            }
        } else if (w < 0 && errno != EINTR && errno != ECHILD) {
            out("[wait] waitpid 失败: %s\n", strerror(errno));
            break;
        }
        if (timeout > 0 && (now_s() - t0) > (double)timeout) {
            out("[timeout] %ds 内无致命信号 ⇒ 释放全部线程（INTERRUPT→DETACH，不制造 SIGSTOP）\n", timeout);
            detach_all(0, 0);
            break;
        }
        struct timespec ts = { 0, 20 * 1000 * 1000 };
        nanosleep(&ts, NULL);
    }
    if (g_out) { fclose(g_out); g_out = NULL; }
    return 0;
}
