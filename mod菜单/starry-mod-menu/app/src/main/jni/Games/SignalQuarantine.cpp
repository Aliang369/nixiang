// Games/SignalQuarantine.cpp
// 信号隔离（AB-30 实验结论的产品化实现）。
//
// 背景：把本 so 伪装成 libtersafe2.so 后，ACE 的 libtprt.so 原生代码会在 ~10.8s 后用
// “伪装崩溃”的方式静默击杀进程（reason=2 SIGNALED, status=7 SIGBUS / 11 SIGSEGV）。
// 实测排除的路径：
//   * 装 sigaction 处理器 → 处理器从不被调用（ACE 先把 disposition 改成 SIG_DFL，且不留 tombstone）；
//   * 只屏蔽主线程 → 无效（信号被投给别的未屏蔽线程）；
//   * 挂钩 libc kill/tgkill/pthread_kill/raise/syscall → 致命投递一次都没走 libc 导出
//     （∴ 走裸 svc 内联系统调用，或由另一个进程/ ptrace 投递）。
// 唯一与 disposition 无关、能同时覆盖“裸 svc / 跨进程 / 先改 disposition”的防线是
// **按线程屏蔽（blocking）**：异步投递的信号若在目标线程被屏蔽就只会 pending，进程不死。
// 做法：JNI_OnLoad 最早处先屏蔽本线程，再起“清扫线程”每 300ms 遍历 /proc/self/task/*/status
// 的 SigBlk，对没屏蔽的线程投一个无害的 SIGWINCH，由处理器在该线程内补屏蔽
// （新线程继承创建者的屏蔽集，因此很快收敛）。
//
// 注意：本模块只在 JNI_OnLoad 最早期做“装处理器 → 屏蔽主线程 → 起清扫线程”，
// 不挂钩 libc（挂 syscall/kill 会与 il2cpp 的 GC 暂停信号 SIGPWR 相冲，见 AB-29b）。
#include <dirent.h>
#include <errno.h>
#include <fcntl.h>
#include <linux/audit.h>
#include <linux/filter.h>
#include <linux/seccomp.h>
#include <pthread.h>
#include <signal.h>
#include <stdarg.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/prctl.h>
#include <sys/syscall.h>
#include <time.h>
#include <unistd.h>

#include "SignalQuarantine.h"

#ifndef SQ_LOGFILE
#define SQ_LOGFILE "/storage/emulated/0/Android/data/com.bilibili.fatego/files/ts2_quarantine.log"
#endif

static void SQ_FileLog(const char* fmt, ...) __attribute__((format(printf, 1, 2)));
static void SQ_FileLog(const char* fmt, ...) {
  FILE* fp = fopen(SQ_LOGFILE, "a");
  if (!fp) {
    fp = fopen("/data/user/0/com.bilibili.fatego/files/ts2_quarantine.log", "a");
    if (!fp) return;
  }
  va_list ap;
  va_start(ap, fmt);
  vfprintf(fp, fmt, ap);
  va_end(ap);
  fputc('\n', fp);
  fflush(fp);
  fclose(fp);
}

#define SQ_LOG(...)                                              \
  do {                                                           \
    __android_log_print(ANDROID_LOG_ERROR, SQ_TAG, __VA_ARGS__);  \
    SQ_FileLog(__VA_ARGS__);                                     \
  } while (0)

// 致命集：ACE 用的是这一族里的某一个（实测 7=SIGBUS、11=SIGSEGV）
static const int kFatalSigs[] = {SIGBUS, SIGSEGV, SIGABRT, SIGILL, SIGFPE, SIGSYS, SIGTRAP};
#define kFatalCount ((int)(sizeof(kFatalSigs) / sizeof(kFatalSigs[0])))
static sigset_t g_fatal;
static int g_sweepSig = SIGWINCH;  // 无害且默认忽略
static volatile int g_sqRunning = 0;

static int SQ_IsFatal(int sig) {
  for (int i = 0; i < kFatalCount; i++) {
    if (kFatalSigs[i] == sig) return 1;
  }
  return 0;
}

static void SQ_LogPending(const char* why) {
  FILE* fp = fopen("/proc/self/status", "r");
  if (!fp) return;
  char line[512];
  unsigned long long pnd = 0, shd = 0;
  while (fgets(line, sizeof(line), fp)) {
    if (strncmp(line, "SigPnd:", 7) == 0) pnd = strtoull(line + 7, NULL, 16);
    else if (strncmp(line, "ShdPnd:", 7) == 0) shd = strtoull(line + 7, NULL, 16);
  }
  fclose(fp);
  int hit = 0;
  for (int i = 0; i < kFatalCount; i++) {
    if (((pnd >> (kFatalSigs[i] - 1)) & 1ull) || ((shd >> (kFatalSigs[i] - 1)) & 1ull)) hit = 1;
  }
  if (hit) {
    SQ_LOG("[quarantine] ⚠ %s：检测到待决致命信号 SigPnd=0x%llx ShdPnd=0x%llx（投递已被屏蔽拦下）",
           why, pnd, shd);
  }
}

static int SQ_ThreadHasFatalBlocked(int tid) {
  char path[64];
  snprintf(path, sizeof(path), "/proc/self/task/%d/status", tid);
  FILE* fp = fopen(path, "r");
  if (!fp) return -1;
  char line[512];
  unsigned long long m = 0;
  int found = 0;
  while (fgets(line, sizeof(line), fp)) {
    if (strncmp(line, "SigBlk:", 7) == 0) {
      m = strtoull(line + 7, NULL, 16);
      found = 1;
      break;
    }
  }
  fclose(fp);
  if (!found) return -1;
  for (int i = 0; i < kFatalCount; i++) {
    if (!((m >> (kFatalSigs[i] - 1)) & 1ull)) return 0;
  }
  return 1;
}

static void SQ_SweepHandler(int sig, siginfo_t* si, void* uc) {  (void)sig; (void)si; (void)uc;
  pthread_sigmask(SIG_BLOCK, &g_fatal, NULL);
}

static void SQ_FatalHandler(int sig, siginfo_t* si, void* uc) {
  (void)uc;
  int code = si ? si->si_code : -999;
  int sender = si ? (int)si->si_pid : 0;
  SQ_LOG("[quarantine] ！！！致命信号 sig=%d code=%d sender_pid=%d addr=%p tid=%ld",
         sig, code, sender, si ? si->si_addr : NULL, (long)syscall(SYS_gettid));
  if (code == SI_USER || code == SI_TKILL || code == SI_QUEUE) {
    SQ_LOG("[quarantine] → 判定为伪造击杀（异步投递），直接返回续跑");
    return;
  }
  struct sigaction dfl;
  memset(&dfl, 0, sizeof(dfl));
  dfl.sa_handler = SIG_DFL;
  sigaction(sig, &dfl, NULL);
  syscall(SYS_tgkill, getpid(), (pid_t)syscall(SYS_gettid), sig);
  _exit(139);  // seccomp 会拒绝上面的 tgkill，直接退出以免陷入“故障指令-死循环”
}

// ===== seccomp：按信号号拒绝“致命信号”的投递 syscall =====
// 为什么必须有它：ACE 的看门狗（libtprt 原生代码）投递致命信号时
//   * 不经过 libc 导出（AB-29b 挂钩 kill/tgkill/pthread_kill/raise/syscall 零命中）⇒ 裸 svc；
//   * 且先把 disposition 改成 SIG_DFL（所以 AB-24 装的处理器从不触发、也不留 tombstone）。
// seccomp 在 **syscall 层**生效：裸 svc 也绕不过，且与线程的 disposition/屏蔽集无关。
// 过滤器被“之后创建的线程”继承——ACE 比我们晚装载（Java 先 loadLibrary("tersafe2") 再 "tprt"），
// 所以它的看门狗线程一定继承到过滤器。非致命信号（如 Unity 的 SIGPWR=30 GC 暂停）原样放行。
static int SQ_InstallDenyFilter(int nr, int sigArgIndex, const char* name) {
  struct sock_filter f[8 + 2 * kFatalCount];
  int n = 0;
  f[n++] = (struct sock_filter)BPF_STMT(BPF_LD | BPF_W | BPF_ABS,
                                        offsetof(struct seccomp_data, arch));
  f[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, AUDIT_ARCH_AARCH64, 1, 0);
  f[n++] = (struct sock_filter)BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW);
  f[n++] = (struct sock_filter)BPF_STMT(BPF_LD | BPF_W | BPF_ABS,
                                        offsetof(struct seccomp_data, nr));
  f[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, (uint32_t)nr, 1, 0);
  f[n++] = (struct sock_filter)BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW);
  f[n++] = (struct sock_filter)BPF_STMT(
      BPF_LD | BPF_W | BPF_ABS,
      (uint32_t)(offsetof(struct seccomp_data, args) + 8 * sigArgIndex));
  int cmpStart = n;
  for (int i = 0; i < kFatalCount; i++) {
    f[n++] = (struct sock_filter)BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K,
                                          (uint32_t)kFatalSigs[i], 0, 0);
  }
  f[n++] = (struct sock_filter)BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW);
  int denyIdx = n;
  f[n++] = (struct sock_filter)BPF_STMT(BPF_RET | BPF_K,
                                        SECCOMP_RET_ERRNO | (EPERM & SECCOMP_RET_DATA));
  for (int i = 0; i < kFatalCount; i++) {
    f[cmpStart + i].jt = (uint8_t)(denyIdx - (cmpStart + i) - 1);
  }
  struct sock_fprog prog;
  prog.len = (unsigned short)n;
  prog.filter = f;
  int rc = prctl(PR_SET_SECCOMP, SECCOMP_MODE_FILTER, &prog, 0, 0);
  int err = errno;
  SQ_LOG("[quarantine] seccomp 过滤 %s(nr=%d arg%d) rc=%d errno=%d%s", name, nr, sigArgIndex,
         rc, err, rc == 0 ? "" : "（ROM 可能禁用了 PR_SET_SECCOMP）");
  return rc;
}

static void SQ_InstallSeccomp(void) {
  if (prctl(PR_SET_NO_NEW_PRIVS, 1, 0, 0, 0) != 0) {
    SQ_LOG("[quarantine] PR_SET_NO_NEW_PRIVS 失败 errno=%d（seccomp 装不上）", errno);
    return;
  }
  // 参数下标：tgkill(tgid,tid,sig)→2；tkill(tid,sig)→1；kill(pid,sig)→1；
  //          rt_sigqueueinfo(pid,sig,info)→1；rt_tgsigqueueinfo(tgid,tid,sig,info)→2
  SQ_InstallDenyFilter(__NR_tgkill, 2, "tgkill");
  SQ_InstallDenyFilter(__NR_tkill, 1, "tkill");
  SQ_InstallDenyFilter(__NR_kill, 1, "kill");
#ifdef __NR_rt_sigqueueinfo
  SQ_InstallDenyFilter(__NR_rt_sigqueueinfo, 1, "rt_sigqueueinfo");
#endif
#ifdef __NR_rt_tgsigqueueinfo
  SQ_InstallDenyFilter(__NR_rt_tgsigqueueinfo, 2, "rt_tgsigqueueinfo");
#endif
}

static void* SQ_SweepThread(void* arg) {
  (void)arg;
  int fixedTotal = 0;
  for (int round = 0; g_sqRunning; round++) {
    DIR* d = opendir("/proc/self/task");
    int n = 0, need = 0;
    pid_t self = (pid_t)syscall(SYS_gettid);
    if (d) {
      struct dirent* e;
      while ((e = readdir(d)) != NULL) {
        if (e->d_name[0] == '.') continue;
        int tid = atoi(e->d_name);
        if (tid <= 0) continue;
        n++;
        if (tid == (int)self) continue;
        if (SQ_ThreadHasFatalBlocked(tid) == 0) {
          need++;
          syscall(SYS_tgkill, getpid(), tid, g_sweepSig);
          fixedTotal++;
        }
      }
      closedir(d);
    }
    int milestone = (round == 0 || round == 2 || round == 5 || round == 10 || round == 20 ||
                     round == 33 || round == 50 || round == 100 || round == 200 || round == 400 ||
                     round == 800 || round == 1600 || round == 3200);
    if (need > 0 || milestone) {
      SQ_LOG("[quarantine] 第%d轮 t=%.1fs 线程数=%d 需补屏蔽=%d 累计补投=%d",
             round, round * 0.3, n, need, fixedTotal);
    }
    if (round % 10 == 0) SQ_LogPending("周期巡检");
    usleep(300 * 1000);
  }
  return NULL;
}

extern "C" void SignalQuarantine_Start(void) {
  if (g_sqRunning) return;
  g_sqRunning = 1;

  sigemptyset(&g_fatal);
  for (int i = 0; i < kFatalCount; i++) sigaddset(&g_fatal, kFatalSigs[i]);

  SQ_LOG("[quarantine] 信号隔离启动 pid=%d tid=%ld", (int)getpid(), (long)syscall(SYS_gettid));

  struct sigaction sa;
  memset(&sa, 0, sizeof(sa));
  sa.sa_flags = SA_SIGINFO | SA_RESTART;
  sigemptyset(&sa.sa_mask);
  sa.sa_sigaction = SQ_SweepHandler;
  sigaction(g_sweepSig, &sa, NULL);

  struct sigaction fa;
  memset(&fa, 0, sizeof(fa));
  fa.sa_flags = SA_SIGINFO | SA_ONSTACK;
  sigemptyset(&fa.sa_mask);
  fa.sa_sigaction = SQ_FatalHandler;
  for (int i = 0; i < kFatalCount; i++) sigaction(kFatalSigs[i], &fa, NULL);
  SQ_LOG("[quarantine] 诊断处理器已安装（%d 个致命信号 + 清扫信号 %d）", kFatalCount, g_sweepSig);

  pthread_sigmask(SIG_BLOCK, &g_fatal, NULL);
  SQ_LOG("[quarantine] 本线程已屏蔽致命信号（SigBlk 复核=%d）", SQ_ThreadHasFatalBlocked((int)syscall(SYS_gettid)));

  // 关键一步：seccomp（ACE 的看门狗线程在之后创建 → 继承过滤器 → 裸 svc 投递被 syscall 层拒绝）
  SQ_InstallSeccomp();

  pthread_t th;
  if (pthread_create(&th, NULL, SQ_SweepThread, NULL) == 0) {
    SQ_LOG("[quarantine] 清扫线程已启动");
  } else {
    SQ_LOG("[quarantine] 清扫线程创建失败 errno=%d", errno);
  }
}
