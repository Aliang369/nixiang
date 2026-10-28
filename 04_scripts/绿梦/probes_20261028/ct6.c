// ct6.c ? ? houdini ????????????**?? libc**?x86_64?? exit_group / exit ?
// syscall ???B8 xx 00 00 00 0F 05??? 0F 05 ?? EB FE?jmp $??????
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <errno.h>
#include <sys/mman.h>
#include <sys/syscall.h>

static unsigned char *patched[8];
static int npatched = 0;

static int patch_range(unsigned long lo, unsigned long hi, const char *host) {
    unsigned char *p = (unsigned char *)lo;
    int hits = 0;
    for (unsigned long i = 0; i + 7 < (hi - lo); i++) {
        if (p[i] == 0xB8 && p[i+5] == 0x0F && p[i+6] == 0x05) {
            unsigned nr = p[i+1] | (p[i+2] << 8) | (p[i+3] << 16);
            if (nr == 231 || nr == 60) {
                long pg = (long)(p + i + 5) & ~0xFFFL;
                if (mprotect((void *)pg, 0x1000, PROT_READ | PROT_WRITE | PROT_EXEC) == 0) {
                    printf("patch %s syscall(nr=%u) @%p\n", host, nr, (void *)(p + i + 5));
                    p[i+5] = 0xEB; p[i+6] = 0xFE;
                    if (npatched < 8) patched[npatched++] = p + i + 5;
                    hits++;
                }
            }
        }
    }
    return hits;
}

int main(void) {
    FILE *f = fopen("/proc/self/maps", "r");
    char line[512]; int total = 0;
    while (f && fgets(line, sizeof line, f)) {
        if (!strstr(line, "libc.so") && !strstr(line, "libhoudini")) continue;
        unsigned long lo, hi; char perms[8] = {0};
        if (sscanf(line, "%lx-%lx %7s", &lo, &hi, perms) != 3) continue;
        if (perms[2] != 'x') continue;
        const char *nm = strrchr(line, '/'); nm = nm ? nm + 1 : "?";
        printf("scan %s %#lx-%#lx\n", nm, lo, hi); fflush(stdout);
        total += patch_range(lo, hi, nm);
    }
    if (f) fclose(f);
    printf("total patched=%d\n", total); fflush(stdout);
    printf("calling exit_group(0) ...\n"); fflush(stdout);
    syscall(SYS_exit_group, 0);
    printf("SURVIVED exit_group\n"); fflush(stdout);
    for (int i = 0; i < npatched; i++) { patched[i][0] = 0x0F; patched[i][1] = 0x05; }
    printf("restored, exiting normally\n"); fflush(stdout);
    return 0;
}
