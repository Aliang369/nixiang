// mntbind: 进入目标进程的 mount namespace 并 bind mount（root）
// usage: mntbind <pid> <src> <dst>
#define _GNU_SOURCE
#include <stdio.h>
#include <stdlib.h>
#include <fcntl.h>
#include <unistd.h>
#include <sys/mount.h>
#include <sched.h>
#include <sys/stat.h>

int main(int argc, char** argv) {
    if (argc < 4) { fprintf(stderr, "usage: %s <pid> <src> <dst>\n", argv[0]); return 1; }
    char ns[64];
    snprintf(ns, sizeof ns, "/proc/%s/ns/mnt", argv[1]);
    int fd = open(ns, O_RDONLY);
    if (fd < 0) { perror("open ns"); return 2; }
    if (setns(fd, CLONE_NEWNS) != 0) { perror("setns"); return 3; }
    // 确保挂载传播不被带走：把根改成 private 再挂（可选，失败忽略）
    mount(NULL, "/", NULL, MS_REC | MS_PRIVATE, NULL);
    if (mount(argv[2], argv[3], NULL, MS_BIND, NULL) != 0) { perror("mount bind"); return 4; }
    printf("BOUND %s -> %s (ns of pid %s)\n", argv[2], argv[3], argv[1]);
    return 0;
}
