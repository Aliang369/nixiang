// whrhx_mem_dex.c — 从运行中进程的 /proc/<pid>/mem 精确导出 InMemoryDexFile 的真实 DEX。
//
// 只读：不 ptrace attach、不注入、不改目标进程。以 root 读取 /proc/<pid>/mem，
// 扫所有可读区域里的 "dex\n0xx\0" magic，按 DEX 头校验后按 header.file_size 整段导出。
//
// 用法： whrhx_mem_dex <pid> <outdir>
// 输出： <outdir>/mem_<addr>_<size>.dex + <outdir>/mem_dex_index.txt
#define _GNU_SOURCE
#include <errno.h>
#include <fcntl.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/stat.h>
#include <unistd.h>

#define CHUNK (8u * 1024u * 1024u)
#define MIN_REGION (64u * 1024u)
#define MAX_REGION (3072ull * 1024ull * 1024ull)
#define MAX_DEX (256u * 1024u * 1024u)

static unsigned char *buf;
// bionic 要求 static 可执行文件 PT_TLS 对齐 >= 64；glibc 静态库只给 8，这里用一个对齐的
// TLS 变量把段对齐顶上去（否则 exec 报 "TLS segment is underaligned"）。
__thread char tls_align_pad[64] __attribute__((aligned(64), used)) = {1};

static int dump_dex(int memfd, uint64_t addr, uint32_t size, const char *outdir, FILE *index) {
    char name[512];
    snprintf(name, sizeof(name), "%s/mem_%llx_%u.dex", outdir, (unsigned long long)addr, size);
    int fd = open(name, O_CREAT | O_TRUNC | O_WRONLY, 0644);
    if (fd < 0) return -1;
    uint32_t done = 0;
    while (done < size) {
        uint32_t want = size - done;
        if (want > CHUNK) want = CHUNK;
        ssize_t n = pread(memfd, buf, want, (off_t)(addr + done));
        if (n <= 0) { close(fd); unlink(name); return -1; }
        if (write(fd, buf, (size_t)n) != n) { close(fd); unlink(name); return -1; }
        done += (uint32_t)n;
    }
    close(fd);
    fprintf(index, "0x%llx\t%u\t%s\n", (unsigned long long)addr, size, name);
    return 0;
}

int main(int argc, char **argv) {
    if (argc < 3) { fprintf(stderr, "usage: %s <pid> <outdir> [needle]\n", argv[0]); return 2; }
    int pid = atoi(argv[1]);
    const char *outdir = argv[2];
    mkdir(outdir, 0755);

    char path[64];
    snprintf(path, sizeof(path), "/proc/%d/maps", pid);
    FILE *maps = fopen(path, "r");
    if (!maps) { perror("maps"); return 1; }
    snprintf(path, sizeof(path), "/proc/%d/mem", pid);
    int memfd = open(path, O_RDONLY);
    if (memfd < 0) { perror("mem"); fclose(maps); return 1; }

    snprintf(path, sizeof(path), "%s/mem_dex_index.txt", outdir);
    FILE *index = fopen(path, "w");
    if (!index) { perror("index"); return 1; }

    tls_align_pad[0] = 1;  // 保活 TLS 变量，确保 PT_TLS 对齐生效
    const char *needle = (argc >= 4 && argv[3][0]) ? argv[3] : NULL;
    buf = malloc(CHUNK);
    if (!buf) return 1;

    char line[1024];
    int found = 0, scanned = 0;
    while (fgets(line, sizeof(line), maps)) {
        unsigned long long start, end;
        char perms[8], rest[900];
        rest[0] = 0;
        if (sscanf(line, "%llx-%llx %7s %*s %*s %*s %899[^\n]", &start, &end, perms, rest) < 3) continue;
        if (perms[0] != 'r') continue;
        unsigned long long size = end - start;
        if (size < MIN_REGION || size > MAX_REGION) continue;
        if (perms[0] != 'r') continue;
        if (strstr(rest, "---p")) continue;
        scanned++;
        // 边界 dex：magic 落在本 region 起点的前 4 字节（实测 B 站易盾 dex mmap 就这样）
        if (start >= 4) {
            unsigned char pre[4];
            if (pread(memfd, pre, 4, (off_t)(start - 4)) == 4 && memcmp(pre, "dex\n", 4) == 0) {
                unsigned char hdr[0x70];
                if (pread(memfd, hdr, sizeof(hdr), (off_t)(start - 4)) == (ssize_t)sizeof(hdr)) {
                    uint32_t fsize = *(uint32_t *)(hdr + 0x20);
                    uint32_t hsize = *(uint32_t *)(hdr + 0x24);
                    uint32_t endian = *(uint32_t *)(hdr + 0x28);
                    if (hsize == 0x70 && endian == 0x12345678 && fsize >= 0x70 && fsize <= MAX_DEX) {
                        if (dump_dex(memfd, start - 4, fsize, outdir, index) == 0) {
                            printf("dex 0x%llx size=%u ver=%c%c%c (edge)\n",
                                   (unsigned long long)(start - 4), fsize, hdr[4], hdr[5], hdr[6]);
                            found++;
                        }
                    }
                }
            }
        }
        for (unsigned long long off = 0; off < size; off += CHUNK) {
            size_t want = (size - off) < CHUNK ? (size_t)(size - off) : CHUNK;
            ssize_t n = pread(memfd, buf, want, (off_t)(start + off));
            if (n <= 0) break;
            if (needle) {
                ssize_t nl = (ssize_t)strlen(needle);
                for (ssize_t i = 0; i + nl <= n; i++) {
                    if (memcmp(buf + i, needle, (size_t)nl) == 0) {
                        unsigned long long hit = start + off + (uint64_t)i;
                        printf("needle @ 0x%llx (region 0x%llx-0x%llx %s)\n",
                               hit, start, end, perms);
                        // 回溯附近 64MB 找最近 dex magic
                        unsigned long long back = hit > (64ull << 20) ? hit - (64ull << 20) : start;
                        if (back < start) back = start;
                        unsigned char tmp[8];
                        for (unsigned long long a = hit; a > back; a -= 4096) {
                            if (pread(memfd, tmp, 8, (off_t)a) == 8 && memcmp(tmp, "dex\n", 4) == 0) {
                                printf("  nearest dex magic below @ 0x%llx\n", a);
                                break;
                            }
                        }
                    }
                }
            }
            for (ssize_t i = 0; i + 8 <= n; i++) {
                if (buf[i] != 'd' || buf[i + 1] != 'e' || buf[i + 2] != 'x' || buf[i + 3] != '\n') continue;
                if (buf[i + 4] != '0' || buf[i + 5] < '3' || buf[i + 5] > '9') continue;
                uint64_t vaddr = start + off + (uint64_t)i;
                // DEX 头校验：file_size@0x20、header_size@0x24、endian_tag@0x28
                unsigned char hdr[0x70];
                if (pread(memfd, hdr, sizeof(hdr), (off_t)vaddr) != (ssize_t)sizeof(hdr)) continue;
                uint32_t fsize = *(uint32_t *)(hdr + 0x20);
                uint32_t hsize = *(uint32_t *)(hdr + 0x24);
                uint32_t endian = *(uint32_t *)(hdr + 0x28);
                if (hsize != 0x70 || endian != 0x12345678) continue;
                if (fsize < 0x70 || fsize > MAX_DEX) continue;
                if (dump_dex(memfd, vaddr, fsize, outdir, index) == 0) {
                    printf("dex 0x%llx size=%u ver=%c%c%c\n", (unsigned long long)vaddr, fsize,
                           hdr[4], hdr[5], hdr[6]);
                    found++;
                }
                i += 0x6f;  // 跳到头部之后，避免同段重复命中
            }
        }
    }
    printf("regions_scanned=%d dex_found=%d\n", scanned, found);
    fclose(index);
    fclose(maps);
    close(memfd);
    free(buf);
    return 0;
}
