// process_vm_readv 内存段转储（无 ptrace attach；复刻 KSU memdump64 思路）
// usage: memdump <pid> <out_file> <offset_hex> <length_hex>  —— 不可读页零填充并记录
#define _GNU_SOURCE
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/uio.h>

int main(int argc, char** argv) {
    if (argc < 5) { fprintf(stderr, "usage: %s pid out offset_hex len_hex\n", argv[0]); return 1; }
    int pid = atoi(argv[1]);
    unsigned long long off = strtoull(argv[3], NULL, 16);
    unsigned long long len = strtoull(argv[4], NULL, 16);
    FILE* f = fopen(argv[2], "wb");
    if (!f) { perror("fopen"); return 2; }
    static char buf[1 << 20];
    unsigned long long done = 0;
    while (done < len) {
        unsigned long long chunk = len - done;
        if (chunk > sizeof(buf)) chunk = sizeof(buf);
        struct iovec local = { buf, chunk };
        struct iovec remote = { (void*)(off + done), chunk };
        ssize_t n = process_vm_readv(pid, &local, 1, &remote, 1, 0);
        if (n <= 0) {
            fprintf(stderr, "GAP 0x%llx+%llx\n", off + done, chunk);
            memset(buf, 0, chunk);
            n = (ssize_t)chunk;
        }
        fwrite(buf, 1, (size_t)n, f);
        done += (unsigned long long)n;
    }
    fclose(f);
    printf("OK %llu bytes @0x%llx\n", done, off);
    return 0;
}
