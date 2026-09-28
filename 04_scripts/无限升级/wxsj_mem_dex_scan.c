/*
 * wxsj_mem_dex_scan - read-only DEX scanner for a live Android process.
 *
 * Reads /proc/<pid>/mem region by region (never writes to the target, never
 * ptraces it) and extracts every well-formed DEX image it can find. Written
 * for 无限升级 because the NIS shell keeps the real DEX only in memory and the
 * game locks ptrace once nesec finishes initialising, so Frida cannot attach.
 *
 * Build:
 *   aarch64-linux-android21-clang -O2 -o wxsj_mem_dex_scan wxsj_mem_dex_scan.c
 * Run (as root on device):
 *   ./wxsj_mem_dex_scan <pid> <outdir>
 */

#define _GNU_SOURCE
#include <ctype.h>
#include <errno.h>
#include <fcntl.h>
#include <inttypes.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/stat.h>
#include <unistd.h>

#define CHUNK (1u << 20)
#define OVERLAP 16

static uint32_t rd32(const unsigned char *p) {
    return (uint32_t)p[0] | ((uint32_t)p[1] << 8) | ((uint32_t)p[2] << 16) | ((uint32_t)p[3] << 24);
}

static uint32_t adler32_of(const unsigned char *data, size_t len) {
    uint32_t a = 1, b = 0;
    size_t i = 0;
    while (i < len) {
        size_t n = len - i;
        if (n > 5552) n = 5552;
        for (size_t j = 0; j < n; j++) {
            a += data[i + j];
            b += a;
        }
        a %= 65521;
        b %= 65521;
        i += n;
    }
    return (b << 16) | a;
}

struct region {
    uint64_t start;
    uint64_t end;
    char perms[8];
    char path[512];
};

static int parse_maps(pid_t pid, struct region **out, size_t *count) {
    char map_path[64];
    snprintf(map_path, sizeof(map_path), "/proc/%d/maps", (int)pid);
    FILE *f = fopen(map_path, "r");
    if (!f) {
        fprintf(stderr, "open %s: %s\n", map_path, strerror(errno));
        return -1;
    }
    size_t cap = 1024, n = 0;
    struct region *regs = calloc(cap, sizeof(*regs));
    if (!regs) { fclose(f); return -1; }

    char line[1024];
    while (fgets(line, sizeof(line), f)) {
        unsigned long long s, e;
        char perms[8] = {0};
        char rest[768] = {0};
        /* start-end perms offset dev inode path */
        if (sscanf(line, "%llx-%llx %7s %*s %*s %*s %767[^\n]", &s, &e, perms, rest) < 3) {
            continue;
        }
        if (n == cap) {
            cap *= 2;
            struct region *tmp = realloc(regs, cap * sizeof(*regs));
            if (!tmp) { free(regs); fclose(f); return -1; }
            regs = tmp;
        }
        regs[n].start = s;
        regs[n].end = e;
        snprintf(regs[n].perms, sizeof(regs[n].perms), "%s", perms);
        char *p = rest;
        while (*p == ' ') p++;
        snprintf(regs[n].path, sizeof(regs[n].path), "%s", p);
        n++;
    }
    fclose(f);
    *out = regs;
    *count = n;
    return 0;
}

int main(int argc, char **argv) {
    if (argc < 3) {
        fprintf(stderr, "usage: %s <pid> <outdir>\n", argv[0]);
        return 2;
    }
    pid_t pid = (pid_t)atoi(argv[1]);
    const char *outdir = argv[2];
    mkdir(outdir, 0755);

    struct region *regs = NULL;
    size_t count = 0;
    if (parse_maps(pid, &regs, &count) != 0) {
        return 1;
    }
    printf("[scan] pid=%d regions=%zu\n", (int)pid, count);

    char mem_path[64];
    snprintf(mem_path, sizeof(mem_path), "/proc/%d/mem", (int)pid);
    int memfd = open(mem_path, O_RDONLY);
    if (memfd < 0) {
        fprintf(stderr, "open %s: %s\n", mem_path, strerror(errno));
        free(regs);
        return 1;
    }

    unsigned char *buf = malloc(CHUNK + OVERLAP);
    if (!buf) { close(memfd); free(regs); return 1; }

    int found = 0;
    for (size_t i = 0; i < count; i++) {
        if (regs[i].perms[0] != 'r') continue;
        if (strcmp(regs[i].perms, "---p") == 0) continue;
        uint64_t pos = regs[i].start;
        size_t carry = 0;
        while (pos < regs[i].end) {
            size_t want = CHUNK;
            if (regs[i].end - pos < want) want = (size_t)(regs[i].end - pos);
            ssize_t got = pread(memfd, buf + carry, want, (off_t)pos);
            if (got <= 0) {
                pos += want;
                carry = 0;
                continue;
            }
            size_t total = carry + (size_t)got;
            for (size_t off = 0; off + 0x70 <= total; off++) {
                if (memcmp(buf + off, "dex\n0", 5) != 0) continue;
                uint64_t addr = pos - carry + off;
                unsigned char hdr[0x40];
                memcpy(hdr, buf + off, sizeof(hdr));
                uint32_t file_size = rd32(hdr + 0x20);
                uint32_t header_size = rd32(hdr + 0x24);
                uint32_t endian_tag = rd32(hdr + 0x28);
                if (endian_tag != 0x12345678 || header_size != 0x70) continue;
                if (file_size < 0x70 || file_size > 512u * 1024u * 1024u) continue;

                /* verify adler32 over the whole image */
                unsigned char *img = malloc(file_size);
                if (!img) continue;
                ssize_t r = pread(memfd, img, file_size, (off_t)addr);
                if (r != (ssize_t)file_size) {
                    free(img);
                    continue;
                }
                uint32_t expect = rd32(img + 0x08);
                uint32_t actual = adler32_of(img + 12, file_size - 12);
                if (expect != actual) {
                    char lp[512];
                    snprintf(lp, sizeof(lp), "%s/loose_%" PRIx64 "_%u.dex", outdir, addr, file_size);
                    FILE *lf = fopen(lp, "wb");
                    if (lf) { fwrite(img, 1, file_size, lf); fclose(lf); }
                    printf("[scan] checksum-mismatch @0x%" PRIx64 " size=%u -> %s\n", addr, file_size, lp);
                    free(img);
                    continue;
                }
                char out_path[512];
                snprintf(out_path, sizeof(out_path), "%s/dex_%" PRIx64 "_%u.dex", outdir, addr, file_size);
                FILE *of = fopen(out_path, "wb");
                if (of) {
                    fwrite(img, 1, file_size, of);
                    fclose(of);
                    printf("[scan] FOUND @0x%" PRIx64 " size=%u -> %s\n", addr, file_size, out_path);
                    found++;
                }
                free(img);
            }
            /* loose pass: report every "dex\n" magic with its version bytes */
            for (size_t off = 0; off + 8 <= total; off++) {
                if (memcmp(buf + off, "dex\n0", 5) != 0) continue;
                uint64_t addr = pos - carry + off;
                char ver[5] = {0};
                memcpy(ver, buf + off + 4, 4);
                uint32_t fsz = 0, hsz = 0, etag = 0;
                if (off + 0x30 <= total) {
                    fsz = rd32(buf + off + 0x20);
                    hsz = rd32(buf + off + 0x24);
                    etag = rd32(buf + off + 0x28);
                }
                printf("[loose] @0x%" PRIx64 " ver=%02x%02x%02x%02x size=%u hdr=%u etag=%08x\n",
                       addr, (unsigned char)ver[0], (unsigned char)ver[1], (unsigned char)ver[2], (unsigned char)ver[3],
                       fsz, hsz, etag);
            }
            carry = total < OVERLAP ? total : OVERLAP;
            memmove(buf, buf + total - carry, carry);
            pos += (uint64_t)got;
        }
    }

    printf("[scan] done found=%d\n", found);
    free(buf);
    close(memfd);
    free(regs);
    return 0;
}
