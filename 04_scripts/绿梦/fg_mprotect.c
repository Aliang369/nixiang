// fg_mprotect.c — aarch64 静态 ptrace 注入器：把目标进程某段映射 mprotect(PROT_NONE)
// 用法: fg_mprotect <pid> [map_offset_hex]   (默认 0xab36000 的 base.apk r--s 段)
// 原理: PTRACE_ATTACH → 读/写 regs → 在目标栈上临时写 svc #0 → SINGLESTEP 执行
//       mprotect syscall → 读返回值 → 还原 regs/指令 → DETACH
#define _GNU_SOURCE
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <errno.h>
#include <stdint.h>
#include <sys/ptrace.h>
#include <sys/wait.h>
#include <sys/uio.h>
#include <elf.h>

#ifndef __NR_mprotect
#define __NR_mprotect 226
#endif

struct uregs { uint64_t x[31]; uint64_t sp; uint64_t pc; uint64_t pstate; };

static int getregs(pid_t pid, struct uregs *r) {
  struct iovec io = { r, sizeof(*r) };
  return ptrace(PTRACE_GETREGSET, pid, (void *)NT_PRSTATUS, &io);
}
static int setregs(pid_t pid, struct uregs *r) {
  struct iovec io = { r, sizeof(*r) };
  return ptrace(PTRACE_SETREGSET, pid, (void *)NT_PRSTATUS, &io);
}

static int find_range(pid_t pid, unsigned long want_off,
                      unsigned long *start, unsigned long *end) {
  char p[64], line[512];
  snprintf(p, sizeof p, "/proc/%d/maps", pid);
  FILE *f = fopen(p, "r");
  if (!f) return -1;
  while (fgets(line, sizeof line, f)) {
    if (!strstr(line, "base.apk")) continue;
    unsigned long s, e, off;
    char perms[8];
    if (sscanf(line, "%lx-%lx %7s %lx", &s, &e, perms, &off) != 4) continue;
    if (off != want_off) continue;
    *start = s; *end = e;
    fclose(f);
    return 0;
  }
  fclose(f);
  return -1;
}

int main(int argc, char **argv) {
  if (argc < 2) { fprintf(stderr, "usage: %s <pid> [map_off_hex]\n", argv[0]); return 2; }
  pid_t pid = (pid_t)atoi(argv[1]);
  unsigned long want = (argc > 2) ? strtoul(argv[2], NULL, 16) : 0xab36000UL;
  unsigned long s = 0, e = 0;
  if (find_range(pid, want, &s, &e) != 0) {
    fprintf(stderr, "ERR: 没找到 offset=%#lx 的 base.apk 映射\n", want);
    return 3;
  }
  unsigned long len = e - s;
  fprintf(stderr, "map: %#lx-%#lx len=%#lx\n", s, e, len);

  if (ptrace(PTRACE_ATTACH, pid, NULL, NULL) != 0) {
    fprintf(stderr, "ERR: attach: %s\n", strerror(errno));
    return 4;
  }
  int st = 0;
  waitpid(pid, &st, 0);

  struct uregs r;
  if (getregs(pid, &r) != 0) { fprintf(stderr, "ERR: getregs: %s\n", strerror(errno)); ptrace(PTRACE_DETACH, pid, NULL, NULL); return 5; }
  struct uregs save = r;

  unsigned long slot = (r.sp - 16) & ~7UL;
  long orig = ptrace(PTRACE_PEEKTEXT, pid, (void *)slot, NULL);
  if (orig == -1 && errno) { fprintf(stderr, "ERR: peek: %s\n", strerror(errno)); ptrace(PTRACE_DETACH, pid, NULL, NULL); return 6; }
  // svc #0  = 0xd4000001
  if (ptrace(PTRACE_POKETEXT, pid, (void *)slot, (void *)0xd4000001UL) != 0) {
    fprintf(stderr, "ERR: poke: %s\n", strerror(errno)); ptrace(PTRACE_DETACH, pid, NULL, NULL); return 7;
  }
  r.pc = slot;
  r.x[8] = __NR_mprotect;
  r.x[0] = s;
  r.x[1] = len;
  r.x[2] = 0;            // PROT_NONE
  if (setregs(pid, &r) != 0) { fprintf(stderr, "ERR: setregs: %s\n", strerror(errno)); ptrace(PTRACE_DETACH, pid, NULL, NULL); return 8; }

  // 用 PTRACE_SYSCALL 走「入口→出口」两次停顿（SINGLESTEP 跨 svc 在 arm64 上拿不到返回值）
  if (ptrace(PTRACE_SYSCALL, pid, NULL, NULL) != 0) {
    fprintf(stderr, "ERR: syscall-enter: %s\n", strerror(errno)); ptrace(PTRACE_DETACH, pid, NULL, NULL); return 9;
  }
  waitpid(pid, &st, 0);
  if (ptrace(PTRACE_SYSCALL, pid, NULL, NULL) != 0) {
    fprintf(stderr, "ERR: syscall-exit: %s\n", strerror(errno)); ptrace(PTRACE_DETACH, pid, NULL, NULL); return 9;
  }
  waitpid(pid, &st, 0);
  if (getregs(pid, &r) != 0) { fprintf(stderr, "ERR: getregs2: %s\n", strerror(errno)); }
  long ret = (long)r.x[0];
  fprintf(stderr, "mprotect(%#lx, %#lx, PROT_NONE) -> %ld %s\n", s, len, ret, ret ? strerror((int)-ret) : "OK");

  // 还原
  struct uregs back = save;
  back.x[0] = save.x[0];
  setregs(pid, &back);
  ptrace(PTRACE_POKETEXT, pid, (void *)slot, (void *)orig);
  ptrace(PTRACE_DETACH, pid, NULL, NULL);
  return ret == 0 ? 0 : 10;
}
