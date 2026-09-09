// 外部根进程补丁器：轮询等待目标分支指令出现 → 写 NOP → 周期性重臂
// usage: flagpatcher <pid> <rel_off_hex> <expect_hex8> <timeout_ms>
// rel_off 相对 libMyGame.so 首映射基址；expect 为 8 字节原文（补丁前校验）
#define _GNU_SOURCE
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/uio.h>
#include <time.h>

static unsigned char NOP4[4] = {0x1f, 0x20, 0x03, 0xd5}; // nop

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
    char line[512];
    unsigned long long base = 0;
    while (fgets(line, sizeof line, f)) {
        if (strstr(line, "libMyGame.so")) {
            base = strtoull(line, NULL, 16);
            break;
        }
    }
    fclose(f);
    return base;
}

static int hex2bin(const char* h, unsigned char* out, int max) {
    int n = 0;
    for (const char* p = h; p[0] && p[1] && n < max; p += 2) {
        unsigned b; if (sscanf(p, "%2x", &b) != 1) return -1;
        out[n++] = (unsigned char)b;
    }
    return n;
}

int main(int argc, char** argv) {
    if (argc < 5) { fprintf(stderr, "usage: %s pid rel_off_hex expect_hex8 timeout_ms\n", argv[0]); return 1; }
    int pid = atoi(argv[1]);
    unsigned long long rel = strtoull(argv[2], NULL, 16);
    unsigned char expect[8], patched[8];
    int el = hex2bin(argv[3], expect, 8);
    int ms = atoi(argv[4]);
    (void)el;
    memcpy(patched, expect, 8);
    // 首指令改 NOP（movz/movk 对 → nop+nop）
    memcpy(patched, NOP4, 4);
    memcpy(patched + 4, NOP4, 4);

    unsigned long long base = 0;
    unsigned char cur[8];
    long waited = 0;
    while (!base) {
        base = find_base(pid);
        if (base) break;
        usleep(100000); waited += 100;
        if (waited > ms) { printf("TIMEOUT wait base\n"); return 2; }
    }
    unsigned long long target = base + rel;
    printf("base=0x%llx target=0x%llx\n", base, target);
    int patched_ok = 0, rearms = 0;
    waited = 0;
    while (waited < ms) {
        if (rw(pid, target, cur, 8, 0)) {
            if (memcmp(cur, patched, 8) == 0) {
                // 已是补丁态
            } else if (memcmp(cur, expect, 8) == 0) {
                if (rw(pid, target, patched, 8, 1)) {
                    printf("PATCHED@%ldms rearms=%d\n", waited, rearms);
                    patched_ok = 1; rearms++;
                } else { printf("WRITE FAIL@%ldms\n", waited); }
            } else if (!patched_ok) {
                unsigned char e8[8]; // 未见预期也未补丁——打印一次现场
                printf("UNEXPECTED@%ldms: %02x%02x%02x%02x%02x%02x%02x%02x\n", waited,
                       cur[0],cur[1],cur[2],cur[3],cur[4],cur[5],cur[6],cur[7]);
            }
        }
        usleep(200000); waited += 200;
    }
    printf("DONE patched_ok=%d rearms=%d\n", patched_ok, rearms);
    return patched_ok ? 0 : 3;
}
