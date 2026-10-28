// fgexit.c <pid> [all]  ? ??? guest arm64 ??? exit/kill syscall ?????? b .
//   ?????? exit_group(94)/exit(93)/exit(60)/exit_group(231)?? all ?? kill/tgkill
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <fcntl.h>
#include <unistd.h>
#include <errno.h>
#include <stdint.h>

typedef struct { unsigned long lo, hi; } Rng;

int main(int argc, char **argv) {
    if (argc < 2) { fprintf(stderr, "usage: %s <pid> [all]\n", argv[0]); return 2; }
    pid_t pid = atoi(argv[1]);
    int all = (argc > 2 && strcmp(argv[2], "all") == 0);
    char mp[64]; snprintf(mp, sizeof mp, "/proc/%d/maps", pid);
    FILE *f = fopen(mp, "r");
    if (!f) { perror("maps"); return 3; }
    Rng r[256]; char nm[256][80]; int n = 0;
    char line[512];
    while (fgets(line, sizeof line, f) && n < 256) {
        unsigned long lo, hi; char perms[8] = {0};
        if (sscanf(line, "%lx-%lx %7s", &lo, &hi, perms) != 3) continue;
        if (perms[0] != 'r') continue;
        if (!strstr(line, "/lib64/arm64/") && !strstr(line, "/lib/arm64/")) continue;
        if (strstr(line, "libstarry_magic")) continue;
        char *sl = strrchr(line, '/'); if (!sl) continue;
        sl[strcspn(sl, "\n")] = 0;
        r[n].lo = lo; r[n].hi = hi;
        snprintf(nm[n], sizeof nm[n], "%s", sl + 1);
        n++;
    }
    fclose(f);
    char memp[64]; snprintf(memp, sizeof memp, "/proc/%d/mem", pid);
    int fd = open(memp, O_RDWR);
    if (fd < 0) { perror("mem"); return 3; }
    int total = 0;
    for (int i = 0; i < n; i++) {
        unsigned long len = r[i].hi - r[i].lo;
        unsigned char *buf = malloc(len);
        if (!buf) continue;
        if (pread(fd, buf, len, (off_t)r[i].lo) != (ssize_t)len) { free(buf); continue; }
        int hits = 0;
        for (unsigned long k = 0; k + 4 * 18 < len; k += 4) {
            uint32_t w; memcpy(&w, buf + k, 4);
            int nr = -1;
            if (w == 0xD2800BC8u) nr = 94;
            else if (w == 0xD2800BA8u) nr = 93;
            else if (w == 0xD2800788u) nr = 60;
            else if (w == 0xD2801CE8u) nr = 231;
            else if (all && (w == 0xD2801028u || w == 0xD2801068u)) nr = (w == 0xD2801028u) ? 129 : 131;
            if (nr < 0) continue;
            for (unsigned long j = k + 4; j <= k + 4 * 17 && j + 4 <= len; j += 4) {
                uint32_t w2; memcpy(&w2, buf + j, 4);
                if (w2 == 0xD4000001u) {
                    unsigned long long a = (unsigned long long)r[i].lo + j;
                    uint32_t pat = 0x14000000u;
                    if (pwrite(fd, &pat, 4, (off_t)a) == 4) {
                        printf("PATCH %s+%#lx svc(nr=%d) @ %#llx\n", nm[i], (unsigned long)j, nr, a);
                        hits++; total++;
                    } else {
                        printf("FAIL  %s+%#lx errno=%d\n", nm[i], (unsigned long)j, errno);
                    }
                    break;
                }
            }
        }
        if (hits) printf("-- %s: %d patched\n", nm[i], hits);
        free(buf);
    }
    printf("total patched = %d (ranges scanned = %d)\n", total, n);
    close(fd);
    return 0;
}
