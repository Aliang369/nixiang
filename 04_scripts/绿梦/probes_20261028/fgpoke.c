#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <fcntl.h>
#include <unistd.h>
#include <errno.h>
#include <time.h>

static unsigned int rd(int fd, unsigned long long a) {
    unsigned int v = 0;
    if (pread(fd, &v, 4, (off_t)a) != 4) return 0xFFFFFFFFu;
    return v;
}

int main(int argc, char **argv) {
    if (argc < 5) { fprintf(stderr, "usage: %s <pid> <addr> <value_hex> <seconds>\n", argv[0]); return 2; }
    pid_t pid = atoi(argv[1]);
    unsigned long long addr = strtoull(argv[2], NULL, 0);
    unsigned int val = (unsigned int)strtoul(argv[3], NULL, 0);
    int secs = atoi(argv[4]);
    char mp[64]; snprintf(mp, sizeof mp, "/proc/%d/mem", pid);
    int fd = open(mp, O_RDWR);
    if (fd < 0) { perror("open mem"); return 3; }
    unsigned int before = rd(fd, addr);
    printf("addr=%#llx before=%#010x -> write %#010x\n", addr, before, val); fflush(stdout);
    if (pwrite(fd, &val, 4, (off_t)addr) != 4) { perror("pwrite"); return 4; }
    unsigned int now = rd(fd, addr);
    printf("after write=%#010x %s\n", now, now == val ? "OK" : "NOT-WRITTEN"); fflush(stdout);
    time_t t0 = time(NULL);
    int lastsec = -1;
    while (time(NULL) - t0 < secs) {
        unsigned int v = rd(fd, addr);
        int el = (int)(time(NULL) - t0);
        if (v != val && el != lastsec) {
            printf("[t=%ds] RESTORED to %#010x\n", el, v);
            fflush(stdout);
            break;
        }
        if (el != lastsec) lastsec = el;
        usleep(200000);
    }
    printf("final=%#010x\n", rd(fd, addr));
    close(fd);
    return 0;
}
