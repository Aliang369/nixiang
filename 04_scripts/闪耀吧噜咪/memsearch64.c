#define _GNU_SOURCE
#include <errno.h>
#include <fcntl.h>
#include <inttypes.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>

/*
 * Minimal read-only process-memory scanner for analysis.
 * Usage: memsearch64 <pid> <start_hex> <end_hex> <ascii-needle>
 */
int main(int argc, char **argv) {
  if (argc != 5) {
    fprintf(stderr, "usage: %s <pid> <start_hex> <end_hex> <needle>\n", argv[0]);
    return 2;
  }

  pid_t pid = (pid_t)atoi(argv[1]);
  uint64_t start = strtoull(argv[2], NULL, 0);
  uint64_t end = strtoull(argv[3], NULL, 0);
  const char *needle = argv[4];
  size_t needle_len = strlen(needle);
  if (end <= start || needle_len == 0 || needle_len > 255) {
    fprintf(stderr, "bad arguments\n");
    return 2;
  }

  char path[64];
  snprintf(path, sizeof(path), "/proc/%d/mem", pid);
  int fd = open(path, O_RDONLY);
  if (fd < 0) {
    perror("open mem");
    return 1;
  }

  const size_t chunk = 1024 * 1024;
  uint8_t *buf = malloc(chunk + needle_len);
  if (!buf) {
    perror("malloc");
    close(fd);
    return 1;
  }

  uint64_t hits = 0;
  uint64_t off = start;
  while (off < end) {
    size_t want = (end - off > chunk) ? chunk : (size_t)(end - off);
    ssize_t got = pread64(fd, buf, want, (off_t)off);
    if (got <= 0) {
      off += want;
      continue;
    }

    for (ssize_t i = 0; i <= got - (ssize_t)needle_len; ++i) {
      if (memcmp(buf + i, needle, needle_len) == 0) {
        uint64_t address = off + (uint64_t)i;
        printf("%016" PRIx64 "\n", address);
        hits++;
        if (hits >= 10000) {
          fprintf(stderr, "hit limit reached\n");
          free(buf);
          close(fd);
          return 0;
        }
      }
    }
    off += (uint64_t)got;
  }

  fprintf(stderr, "hits=%" PRIu64 "\n", hits);
  free(buf);
  close(fd);
  return 0;
}
