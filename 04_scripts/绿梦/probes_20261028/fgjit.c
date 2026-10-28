// fgjit.c <pid> <seconds> ? ???? JIT/???????? houdini ? guest svc ????
//   mov eax,<nr>; syscall  ?nr=231 exit_group / 60 exit??? syscall ?? jmp $
//   ??"???????"??????root??
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <fcntl.h>
#include <unistd.h>
#include <errno.h>
#include <stdint.h>
#include <time.h>

typedef struct { unsigned long lo, hi; char path[128]; } Rng;

static int scan_once(int fd, pid_t pid, int *patched, int verbose) {
    char mp[64]; snprintf(mp, sizeof mp, "/proc/%d/maps", pid);
    FILE *f = fopen(mp, "r");
    if (!f) return -1;
    char line[512];
    int regions = 0;
    while (fgets(line, sizeof line, f)) {
        unsigned long lo, hi; char perms[8] = {0};
        if (sscanf(line, "%lx-%lx %7s", &lo, &hi, perms) != 3) continue;
        if (perms[2] != 'x') continue;
        unsigned long len = hi - lo;
        if (len > 64UL * 1024 * 1024) continue;      /* skip giant libs */
        char *p = strchr(line, '/');
        /* ???? / memfd / ashmem / vdex ?? JIT ? */
        if (p && !strstr(p, "memfd") && !strstr(p, "ashmem") && !strstr(p, "jit") &&
            !strstr(p, "vdex") && !strstr(p, "dalvik")) continue;
        unsigned char *buf = malloc(len);
        if (!buf) continue;
        ssize_t got = pread(fd, buf, len, (off_t)lo);
        if (got <= 0) { free(buf); continue; }
        regions++;
        for (ssize_t i = 0; i + 7 <= got; i++) {
            if (buf[i] == 0xB8 && buf[i + 5] == 0x0F && buf[i + 6] == 0x05) {
                unsigned nr = buf[i + 1] | (buf[i + 2] << 8) | (buf[i + 3] << 16);
                if (nr == 231 || nr == 60 || nr == 94 || nr == 93 ||
                    (buf[i + 2] == 0 && buf[i + 3] == 0 && buf[i + 4] == 0 &&
                     (nr == 231 || nr == 60))) {
                    unsigned long long a = (unsigned long long)lo + i + 5;
                    unsigned char patch[2] = { 0xEB, 0xFE };
                    if (pwrite(fd, patch, 2, (off_t)a) == 2) {
                        printf("PATCH %#llx nr=%u region=%#lx-%#lx %s\n", a, nr, lo, hi, p ? p : "(anon)");
                        (*patched)++;
                        buf[i + 5] = 0xEB; buf[i + 6] = 0xFE;
                    }
                }
            }
        }
        free(buf);
    }
    fclose(f);
    if (verbose) printf("  regions scanned=%d patched=%d\n", regions, *patched);
    return regions;
}

int main(int argc, char **argv) {
    if (argc < 3) { fprintf(stderr, "usage: %s <pid> <seconds>\n", argv[0]); return 2; }
    pid_t pid = atoi(argv[1]);
    int secs = atoi(argv[2]);
    char memp[64]; snprintf(memp, sizeof memp, "/proc/%d/mem", pid);
    int fd = open(memp, O_RDWR);
    if (fd < 0) { perror("mem"); return 3; }
    int patched = 0;
    time_t t0 = time(NULL);
    while (time(NULL) - t0 < secs) {
        scan_once(fd, pid, &patched, time(NULL) - t0 < 1);
        usleep(100 * 1000);
    }
    printf("total patched=%d\n", patched);
    close(fd);
    return 0;
}
