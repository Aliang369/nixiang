#define _GNU_SOURCE
#include <errno.h>
#include <fcntl.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>

/* Tiny aarch64 helper: dump [start,end) from /proc/<pid>/mem via pread64.
 * Prefer this when toybox dd skip overflows; KernelSU root required. */

static int dump_pread(int fd, uint64_t start, uint64_t size, int out) {
  const size_t CHUNK = 1024 * 1024;
  uint8_t *buf = malloc(CHUNK);
  if (!buf) return -1;
  uint64_t off = 0;
  while (off < size) {
    size_t n = (size - off > CHUNK) ? CHUNK : (size_t)(size - off);
    ssize_t r = pread64(fd, buf, n, (off_t)(start + off));
    if (r < 0) { perror("pread64"); free(buf); return -1; }
    if (r == 0) break;
    if (write(out, buf, (size_t)r) != r) { perror("write"); free(buf); return -1; }
    off += (uint64_t)r;
  }
  free(buf);
  return 0;
}

int main(int argc, char **argv) {
  if (argc != 5) {
    fprintf(stderr, "usage: %s <pid> <start_hex> <end_hex> <out_path>\n", argv[0]);
    return 2;
  }
  int pid = atoi(argv[1]);
  uint64_t start = strtoull(argv[2], NULL, 0);
  uint64_t end = strtoull(argv[3], NULL, 0);
  if (end <= start) { fprintf(stderr, "bad range\n"); return 2; }
  uint64_t size = end - start;
  char path[64];
  snprintf(path, sizeof(path), "/proc/%d/mem", pid);
  int fd = open(path, O_RDONLY);
  if (fd < 0) { perror("open mem"); return 1; }
  int out = open(argv[4], O_CREAT | O_TRUNC | O_WRONLY, 0644);
  if (out < 0) { perror("open out"); close(fd); return 1; }
  int rc = dump_pread(fd, start, size, out);
  close(out); close(fd);
  if (rc == 0)
    fprintf(stderr, "dumped %llu bytes -> %s\n", (unsigned long long)size, argv[4]);
  return rc ? 1 : 0;
}
