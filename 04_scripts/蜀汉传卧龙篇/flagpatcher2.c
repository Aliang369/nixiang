// flagpatcher v2：多态 dispatcher 语义扫描补丁（蜀汉传卧龙篇 v100.22.1）
// 每轮: 读保护区窗口 → 找 "MOVZ/MOVK 生成 0x79BACE2B" 指令对 → 其后第一条 b.cond → NOP
// 重臂循环: 原文再现(重新解密/重定位)则再补。计数分支(0x4FAF2AEB)不动。
// usage: flagpatcher2 <pkg> <timeout_ms>
#define _GNU_SOURCE
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <dirent.h>
#include <sys/uio.h>

#define WIN_LO 0x1e30000u
#define WIN_HI 0x1e50000u          // 调度区+handler 区窗口
#define KILL_LO 0xCE2Bu
#define KILL_HI 0x79BAu
#define MAXSITES 16

static const unsigned int NOP = 0xD503201Fu;

static int rw(int pid, unsigned long long addr, void* buf, size_t len, int write) {
    struct iovec l = { buf, len };
    struct iovec r = { (void*)addr, len };
    ssize_t n = write ? process_vm_writev(pid, &l, 1, &r, 1, 0)
                      : process_vm_readv(pid, &l, 1, &r, 1, 0);
    return n == (ssize_t)len;
}

static unsigned long long find_base(int pid) {
    char path[64]; snprintf(path, sizeof path, "/proc/%d/maps", pid);
    FILE* f = fopen(path, "r");
    if (!f) return 0;
    char line[512]; unsigned long long base = 0;
    while (fgets(line, sizeof line, f))
        if (strstr(line, "libMyGame.so")) { base = strtoull(line, NULL, 16); break; }
    fclose(f);
    return base;
}

static int wait_main_pid(const char* pkg) {
    for (int i = 0; i < 600; i++) {
        char path[64]; snprintf(path, sizeof path, "/proc");
        DIR* d = opendir("/proc");
        struct dirent* e;
        while ((e = readdir(d))) {
            if (e->d_name[0] < '0' || e->d_name[0] > '9') continue;
            char cp[96]; snprintf(cp, sizeof cp, "/proc/%s/cmdline", e->d_name);
            FILE* f = fopen(cp, "r");
            if (!f) continue;
            char cmd[128] = {0};
            size_t n = fread(cmd, 1, sizeof cmd - 1, f);
            fclose(f);
            (void)n;
            if (strcmp(cmd, pkg) == 0) { closedir(d); return atoi(e->d_name); }
        }
        closedir(d);
        usleep(200000);
    }
    return 0;
}

int main(int argc, char** argv) {
    if (argc < 3) { fprintf(stderr, "usage: %s <pkg> <timeout_ms>\n", argv[0]); return 1; }
    const char* pkg = argv[1];
    int ms = atoi(argv[2]);
    int pid = wait_main_pid(pkg);
    if (!pid) { printf("NO_PID\n"); return 2; }
    printf("PID=%d\n", pid);

    unsigned long long base = 0;
    for (int i = 0; i < 100 && !base; i++) { base = find_base(pid); if (!base) usleep(100000); }
    if (!base) { printf("NO_BASE\n"); return 3; }
    printf("BASE=0x%llx\n", base);

    static unsigned int win[(WIN_HI - WIN_LO) / 4];
    int npatch_total = 0;
    long t = 0;
    while (t < ms) {
        if (rw(pid, base + WIN_LO, win, sizeof win, 0)) {
            int n = (int)(sizeof win / 4);
            for (int i = 0; i + 1 < n && i + 18 < n; i++) {
                unsigned w1 = win[i], w2 = win[i + 1];
                unsigned d1 = w1 & 0x1F, d2 = w2 & 0x1F;
                if (d1 != d2) continue;
                if ((w1 & 0xFFFFFFE0) != (0x52800000u | (KILL_LO << 5))) continue;
                if ((w2 & 0xFFFFFFE0) != (0x72A00000u | (KILL_HI << 5))) continue;
                // 找其后 0x40 字节内第一条 b.cond（cmp 之后消费比较结果的分支）
                for (int j = i + 2; j < i + 18; j++) {
                    unsigned b = win[j];
                    if ((b & 0xFF000010u) == 0x54000000u) {   // b.cond
                        unsigned long long site = base + WIN_LO + (unsigned long long)j * 4;
                        if (rw(pid, site, (void*)&NOP, 4, 1)) {
                            printf("PATCH site=0x%llx (pair@+%lx, insn=%08x) t=%ldms\n",
                                   site, (unsigned long)(i * 4 + WIN_LO), b, t);
                            npatch_total++;
                        } else printf("WRITE_FAIL site=0x%llx t=%ldms\n", site, t);
                        break;
                    }
                }
            }
        } else {
            printf("READ_FAIL t=%ldms (进程可能已退出)\n", t);
            break;
        }
        usleep(500000); t += 500;
    }
    printf("DONE total_patches=%d\n", npatch_total);
    return npatch_total ? 0 : 4;
}
