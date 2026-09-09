// flagzeroer：外部 root 清零检测标志（AB-7）
// 全量扫 rw 区找 0x79BACE2B / 0x4FAF2AEB → 清零；找到后进入快循环只盯已命中地址 + 慢速增量全扫
// usage: flagzeroer <pkg> <timeout_ms>
#define _GNU_SOURCE
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <dirent.h>
#include <sys/uio.h>

static const unsigned int MAGICS[2] = { 0x79BACE2Bu, 0x4FAF2AEBu };

struct site { unsigned long long addr; unsigned int magic; };

static struct site sites[256];
static int nsites = 0;

static int rwv(int pid, unsigned long long addr, void* buf, size_t len, int write) {
    struct iovec l = { buf, len };
    struct iovec r = { (void*)addr, len };
    ssize_t n = write ? process_vm_writev(pid, &l, 1, &r, 1, 0)
                      : process_vm_readv(pid, &l, 1, &r, 1, 0);
    return n == (ssize_t)len;
}

static int wait_main_pid(const char* pkg) {
    for (int i = 0; i < 600; i++) {
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

struct region { unsigned long long lo, hi; char path[128]; };

// 全扫一轮：读所有 rw 匿名/数据区，找魔数
static int full_scan(int pid, int zero_it) {
    char path[64]; snprintf(path, sizeof path, "/proc/%d/maps", pid);
    FILE* f = fopen(path, "r");
    if (!f) return -1;
    char line[600];
    static unsigned char buf[1 << 20];
    int hits = 0;
    unsigned long long base_mygame = 0;
    // 先取 libMyGame 基址（其 RWX 数据区魔数是代码常数，跳过）
    while (fgets(line, sizeof line, f))
        if (strstr(line, "libMyGame.so")) { base_mygame = strtoull(line, NULL, 16); break; }
    rewind(f);
    while (fgets(line, sizeof line, f)) {
        if (!strstr(line, "rw") || strstr(line, "r--")) continue;
        char* perms = strchr(line, ' ');
        if (!perms || perms[1] != 'r' || perms[2] != 'w') continue;
        if (strstr(line, "/dev/")) continue;
        unsigned long long lo = strtoull(line, NULL, 16);
        unsigned long long hi = strtoull(strchr(line, '-') + 1, NULL, 16);
        if (hi - lo > (512ULL << 20)) continue;               // 跳过超大区
        if (base_mygame && lo >= base_mygame && lo < base_mygame + 0x1f00000) continue; // 跳过 libMyGame 自身
        for (unsigned long long p = lo; p < hi; p += sizeof buf) {
            size_t chunk = hi - p; if (chunk > sizeof buf) chunk = sizeof buf;
            if (!rwv(pid, p, buf, chunk, 0)) continue;
            for (size_t i = 0; i + 4 <= chunk; i += 4) {
                unsigned v; memcpy(&v, buf + i, 4);
                if (v == MAGICS[0] || v == MAGICS[1]) {
                    unsigned long long addr = p + i;
                    if (zero_it) {
                        unsigned zero = 0;
                        rwv(pid, addr, &zero, 4, 1);
                        printf("ZERO 0x%llx (=%08x)\n", addr, v);
                    } else {
                        printf("HIT  0x%llx (=%08x)\n", addr, v);
                    }
                    if (nsites < 256) { sites[nsites].addr = addr; sites[nsites].magic = v; nsites++; }
                    hits++;
                    if (hits >= 64) goto out;
                }
            }
        }
    }
out:
    fclose(f);
    return hits;
}

int main(int argc, char** argv) {
    if (argc < 3) { fprintf(stderr, "usage: %s <pkg> <timeout_ms> [--probe]\n", argv[0]); return 1; }
    const char* pkg = argv[1];
    int ms = atoi(argv[2]);
    int probe = (argc > 3 && strcmp(argv[3], "--probe") == 0);
    int pid = wait_main_pid(pkg);
    if (!pid) { printf("NO_PID\n"); return 2; }
    printf("PID=%d\n", pid);

    if (probe) { full_scan(pid, 0); printf("PROBE_DONE nsites=%d\n", nsites); return 0; }

    nsites = 0;
    int first = full_scan(pid, 1);
    printf("FIRST_SCAN hits=%d sites=%d\n", first, nsites);
    long t = 1000;
    while (t < ms) {
        // 快循环：盯已知地址
        int rezeroed = 0;
        for (int i = 0; i < nsites; i++) {
            unsigned v;
            if (rwv(pid, sites[i].addr, &v, 4, 0) && (v == MAGICS[0] || v == MAGICS[1])) {
                unsigned zero = 0;
                if (rwv(pid, sites[i].addr, &zero, 4, 1)) rezeroed++;
            }
        }
        // 慢循环：每 2s 全扫一遍找新写点
        static int round = 0;
        if (++round % 2 == 0) full_scan(pid, 1);
        usleep(100000); t += 100;
    }
    printf("DONE nsites=%d\n", nsites);
    return 0;
}
