// fgdump.c ? ? /proc/<pid>/mem ?????? dump ????root ???
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <fcntl.h>
#include <unistd.h>
#include <errno.h>

int main(int argc, char **argv) {
    if (argc < 5) { fprintf(stderr, "usage: %s <pid> <hex_off> <hex_len> <out>\n", argv[0]); return 2; }
    pid_t pid = atoi(argv[1]);
    unsigned long long off = strtoull(argv[2], NULL, 0);
    unsigned long long len = strtoull(argv[3], NULL, 0);
    char mp[64]; snprintf(mp, sizeof mp, "/proc/%d/mem", pid);
    int fd = open(mp, O_RDONLY);
    if (fd < 0) { perror("open mem"); return 3; }
    int out = open(argv[4], O_WRONLY | O_CREAT | O_TRUNC, 0644);
    if (out < 0) { perror("open out"); return 3; }
    static char buf[1 << 20];
    unsigned long long done = 0;
    while (done < len) {
        size_t want = (len - done) > sizeof buf ? sizeof buf : (size_t)(len - done);
        ssize_t n = pread(fd, buf, want, (off_t)(off + done));
        if (n <= 0) { fprintf(stderr, "pread stop at +%llu errno=%d\n", done, errno); break; }
        if (write(out, buf, n) != n) { perror("write"); break; }
        done += n;
    }
    close(out); close(fd);
    printf("dumped %llu / %llu bytes\n", done, len);
    return 0;
}
