// Games/CrashTrap.cpp
// 目的：在没有 root、拿不到 /data/tombstones 的前提下，把「谁杀了我们」变成 logcat 里可读的证据。
//   1) 装 SIGABRT/SIGSEGV/SIGBUS/SIGILL/SIGFPE/SIGTRAP/SIGSYS 处理器（带独立信号栈，栈溢出也能记录）
//   2) 记录 siginfo：信号号、si_code、si_pid（谁发的）、si_addr（硬件故障地址）
//   3) 记录回溯：_Unwind_Backtrace 原始 PC + dladdr 解析出的「模块+偏移」
//   4) 启动时把 /proc/self/maps 里的 .so 段打进日志 → 离线用 addr2line 精确还原
//   5) 看门线程反复重装处理器（抢在 libtprt.so 之后注册，确保我们赢）
//   行为中性：先记录（含 ucontext 里的真实 fault-pc），再把信号转交先前处理器（ART FaultManager
//   能处理的可恢复故障放行），链满/无处理器时才按默认方式重发信号。
#include <jni.h>
#include <android/log.h>
#include <signal.h>
#include <unistd.h>
#include <fcntl.h>
#include <dirent.h>
#include <dlfcn.h>
#include <unwind.h>
#include <pthread.h>
#include <cstdio>
#include <cstring>
#include <cstdlib>
#include <sys/syscall.h>
#include <sys/types.h>
#include <ucontext.h>

#define CT_TAG "Mod_Menu"
#define CT_STKSZ (64 * 1024)

static char g_altstack[CT_STKSZ] __attribute__((aligned(16)));
static volatile sig_atomic_t g_in_handler = 0;
static volatile sig_atomic_t g_chainCount = 0;

static const int kCtSigs[] = {SIGABRT, SIGSEGV, SIGBUS, SIGILL, SIGFPE, SIGTRAP, SIGSYS};
static const int kCtSigCount = (int)(sizeof(kCtSigs) / sizeof(kCtSigs[0]));

// 「先前注册的处理器」：Android 上 sigaction 会被 libsigchain 接管，
// oldact 拿到的是信号链里的上一个处理器（通常是 ART 的 FaultManager）。
// 只记录第一次安装时的值，避免看门重装把自己记成 previous。
static struct sigaction g_prev[kCtSigCount];
static volatile sig_atomic_t g_prevSaved[kCtSigCount] = {0};

static int CtSigIndex(int s) {
  for (int i = 0; i < kCtSigCount; i++) if (kCtSigs[i] == s) return i;
  return -1;
}

static const char* CtSigName(int s) {
  switch (s) {
    case SIGABRT: return "SIGABRT";
    case SIGSEGV: return "SIGSEGV";
    case SIGBUS:  return "SIGBUS";
    case SIGILL:  return "SIGILL";
    case SIGFPE:  return "SIGFPE";
    case SIGTRAP: return "SIGTRAP";
    case SIGSYS:  return "SIGSYS";
    default:      return "?";
  }
}

static const char* CtCodeName(int sig, int code) {
  if (sig == SIGSEGV) {
    switch (code) { case SEGV_MAPERR: return "SEGV_MAPERR"; case SEGV_ACCERR: return "SEGV_ACCERR";
                    case SEGV_BNDERR: return "SEGV_BNDERR"; default: return "SEGV_?"; }
  }
  if (sig == SIGBUS) {
    switch (code) { case BUS_ADRALN: return "BUS_ADRALN"; case BUS_ADRERR: return "BUS_ADRERR";
                    case BUS_OBJERR: return "BUS_OBJERR"; default: return "BUS_?"; }
  }
  switch (code) {
    case SI_USER: return "SI_USER"; case SI_QUEUE: return "SI_QUEUE"; case SI_TIMER: return "SI_TIMER";
    case SI_TKILL: return "SI_TKILL"; case SI_KERNEL: return "SI_KERNEL";
    case SI_ASYNCNL: return "SI_ASYNCNL"; default: return "code_?";
  }
}

struct CtBt { int n; uintptr_t pc[48]; };

static _Unwind_Reason_Code CtBtCb(struct _Unwind_Context* ctx, void* arg) {
  CtBt* b = (CtBt*)arg;
  uintptr_t pc = (uintptr_t)_Unwind_GetIP(ctx);
  if (pc && b->n < (int)(sizeof(b->pc) / sizeof(b->pc[0]))) b->pc[b->n++] = pc;
  return _URC_NO_REASON;
}

static void CtHandler(int sig, siginfo_t* si, void* uc) {
  if (g_in_handler) {
    signal(sig, SIG_DFL);
    syscall(SYS_tgkill, getpid(), (pid_t)syscall(SYS_gettid), sig);
    return;
  }
  g_in_handler = 1;

  int code = si ? si->si_code : -1;
  int spid = si ? si->si_pid : -1;
  int suid = si ? si->si_uid : -1;
  void* addr = si ? si->si_addr : nullptr;
  int tid = (int)syscall(SYS_gettid);

  __android_log_print(ANDROID_LOG_ERROR, CT_TAG,
                      "[crash] sig=%d(%s) code=%d(%s) sender_pid=%d sender_uid=%d fault=%p pid=%d tid=%d",
                      sig, CtSigName(sig), code, CtCodeName(sig, code), spid, suid, addr, (int)getpid(), tid);

  // ---- 真正的故障现场：回溯会被信号帧截断，只有 ucontext 里的 pc 才是「谁崩的」----
  uintptr_t fpc = 0, flr = 0, fsp = 0, ffp = 0, faddr = 0;
  if (uc) {
    ucontext_t* u = (ucontext_t*)uc;
    fpc = (uintptr_t)u->uc_mcontext.pc;
    fsp = (uintptr_t)u->uc_mcontext.sp;
    flr = (uintptr_t)u->uc_mcontext.regs[30];
    ffp = (uintptr_t)u->uc_mcontext.regs[29];
    faddr = (uintptr_t)u->uc_mcontext.fault_address;
  }
  if (fpc) {
    Dl_info dp;
    memset(&dp, 0, sizeof(dp));
    if (dladdr((void*)fpc, &dp) && dp.dli_fname) {
      __android_log_print(ANDROID_LOG_ERROR, CT_TAG,
                          "[crash] fault-pc=%p %s+0x%lx base=%p lr=%p sp=%p fp=%p fault_addr=%p",
                          (void*)fpc, dp.dli_fname,
                          (unsigned long)(fpc - (uintptr_t)dp.dli_fbase), dp.dli_fbase,
                          (void*)flr, (void*)fsp, (void*)ffp, (void*)faddr);
    } else {
      __android_log_print(ANDROID_LOG_ERROR, CT_TAG,
                          "[crash] fault-pc=%p <unmapped> lr=%p sp=%p fp=%p fault_addr=%p",
                          (void*)fpc, (void*)flr, (void*)fsp, (void*)ffp, (void*)faddr);
    }
  }

  CtBt b;
  b.n = 0;
  _Unwind_Backtrace(CtBtCb, &b);
  __android_log_print(ANDROID_LOG_ERROR, CT_TAG, "[crash] frames=%d", b.n);
  for (int i = 0; i < b.n; i++) {
    Dl_info di;
    memset(&di, 0, sizeof(di));
    if (dladdr((void*)b.pc[i], &di) && di.dli_fname) {
      __android_log_print(ANDROID_LOG_ERROR, CT_TAG, "[crash] #%02d %p %s+0x%lx base=%p",
                          i, (void*)b.pc[i], di.dli_fname,
                          (unsigned long)(b.pc[i] - (uintptr_t)di.dli_fbase), di.dli_fbase);
    } else {
      __android_log_print(ANDROID_LOG_ERROR, CT_TAG, "[crash] #%02d %p <unmapped>", i, (void*)b.pc[i]);
    }
  }

  // 把当前线程名单也打一遍：ACE 的看门线程往往是死前新出现的
  DIR* d = opendir("/proc/self/task");
  if (d) {
    struct dirent* e;
    char buf[256];
    while ((e = readdir(d)) != nullptr) {
      if (e->d_name[0] == '.') continue;
      char p[128];
      snprintf(p, sizeof(p), "/proc/self/task/%s/comm", e->d_name);
      int fd = open(p, O_RDONLY);
      if (fd < 0) continue;
      ssize_t r = read(fd, buf, sizeof(buf) - 1);
      close(fd);
      if (r <= 0) continue;
      buf[r] = 0;
      for (ssize_t i = 0; i < r; i++) if (buf[i] == '\n') buf[i] = 0;
      __android_log_print(ANDROID_LOG_ERROR, CT_TAG, "[crash] thread tid=%s comm=%s", e->d_name, buf);
    }
    closedir(d);
  }

  // ---- 让信号链继续：不要把「游戏自己的可恢复故障」升级成进程死亡 ----
  // 真机实测（境界刀鸣 1.39.211507，3 次复现）：启动 ~13s 有一次
  //   sig=11(SIGSEGV) code=1(SEGV_MAPERR) fault=0x18a44
  // 栈链 = [vdso 信号跳板] → libbytehook.so+0x93a8 → libsigchain.so+0x60b8 → 本处理器。
  // 反汇编 libbytehook.so 0x9374-0x93a4 可见它正是在「主动调用先前保存的处理器」
  // （cmp handler,#2 → blr / SA_SIGINFO 分支），返回后继续自己的逻辑（该库导入
  // sigsetjmp/siglongjmp → 自恢复）。官方包同样走这条链但能活，因为我们不在。
  // 我们无条件重发信号 = 把可恢复故障变成必死（且 seccomp 拦了 tgkill，连 tombstone 都没有）。
  // 因此：记录完就 return，让 sigchain 继续分发、让 bytehook 自恢复。
  // 上限 6 次：若确实没人处理，故障指令会被反复重放 → 到上限才走默认处理，避免死循环。
  if (g_chainCount < 6) {
    g_chainCount++;
    __android_log_print(ANDROID_LOG_ERROR, CT_TAG,
                        "[crash] 让信号链继续（第 %d/6 次）sig=%d tid=%d",
                        (int)g_chainCount, sig, tid);
    g_in_handler = 0;
    return;
  }
  __android_log_print(ANDROID_LOG_ERROR, CT_TAG, "[crash] 已达让行上限，交给默认处理（重发信号 %d）", sig);
  signal(sig, SIG_DFL);
  syscall(SYS_tgkill, getpid(), (pid_t)tid, sig);
  _exit(134);
}

extern "C" void CrashTrap_Install(void) {
  static bool stk = false;
  if (!stk) {
    stk = true;
    stack_t ss;
    memset(&ss, 0, sizeof(ss));
    ss.ss_sp = g_altstack;
    ss.ss_size = sizeof(g_altstack);
    ss.ss_flags = 0;
    if (sigaltstack(&ss, nullptr) != 0) {
      __android_log_print(ANDROID_LOG_WARN, CT_TAG, "[crash] sigaltstack 失败");
    }
  }
  struct sigaction sa;
  memset(&sa, 0, sizeof(sa));
  sa.sa_sigaction = CtHandler;
  sa.sa_flags = SA_SIGINFO | SA_ONSTACK | SA_NODEFER;
  sigemptyset(&sa.sa_mask);
  for (int i = 0; i < kCtSigCount; i++) {
    struct sigaction old;
    memset(&old, 0, sizeof(old));
    if (sigaction(kCtSigs[i], &sa, &old) != 0) {
      __android_log_print(ANDROID_LOG_WARN, CT_TAG, "[crash] sigaction(%s) 失败", CtSigName(kCtSigs[i]));
      continue;
    }
    if (!g_prevSaved[i]) {
      g_prev[i] = old;
      g_prevSaved[i] = 1;
      if (old.sa_flags & SA_SIGINFO) {
        __android_log_print(ANDROID_LOG_ERROR, CT_TAG,
                            "[crash] 记录先前处理器 sig=%s siginfo=%p flags=0x%x",
                            CtSigName(kCtSigs[i]), (void*)old.sa_sigaction, old.sa_flags);
      } else {
        __android_log_print(ANDROID_LOG_ERROR, CT_TAG,
                            "[crash] 记录先前处理器 sig=%s handler=%p flags=0x%x",
                            CtSigName(kCtSigs[i]), (void*)old.sa_handler, old.sa_flags);
      }
    }
  }
}

extern "C" void CrashTrap_LogMaps(void) {
  int fd = open("/proc/self/maps", O_RDONLY);
  if (fd < 0) { __android_log_print(ANDROID_LOG_WARN, CT_TAG, "[crash] 打不开 /proc/self/maps"); return; }
  static char buf[256 * 1024];
  ssize_t r = read(fd, buf, sizeof(buf) - 1);
  close(fd);
  if (r <= 0) return;
  buf[r] = 0;
  int n = 0;
  char* p = buf;
  while (p && *p && n < 400) {
    char* nl = strchr(p, '\n');
    if (nl) *nl = 0;
    if (strstr(p, ".so") || strstr(p, "base.apk")) {
      __android_log_print(ANDROID_LOG_ERROR, CT_TAG, "[maps] %s", p);
      n++;
    }
    if (!nl) break;
    p = nl + 1;
  }
  __android_log_print(ANDROID_LOG_ERROR, CT_TAG, "[maps] 共 %d 行（已过滤 .so/base.apk）", n);
}

extern "C" void CrashTrap_LogThreads(void) {
  DIR* d = opendir("/proc/self/task");
  if (!d) return;
  struct dirent* e;
  char buf[256];
  int n = 0;
  while ((e = readdir(d)) != nullptr) {
    if (e->d_name[0] == '.') continue;
    char p[128];
    snprintf(p, sizeof(p), "/proc/self/task/%s/comm", e->d_name);
    int fd = open(p, O_RDONLY);
    if (fd < 0) continue;
    ssize_t r = read(fd, buf, sizeof(buf) - 1);
    close(fd);
    if (r <= 0) continue;
    buf[r] = 0;
    for (ssize_t i = 0; i < r; i++) if (buf[i] == '\n') buf[i] = 0;
    __android_log_print(ANDROID_LOG_ERROR, CT_TAG, "[threads] tid=%s comm=%s", e->d_name, buf);
    n++;
  }
  closedir(d);
  __android_log_print(ANDROID_LOG_ERROR, CT_TAG, "[threads] 共 %d 个线程", n);
}

static void* CtKeeper(void*) {
  // libtprt.so 在我们的 JNI_OnLoad 之后才装载，它可能覆盖信号处理器；反复重装保证我们赢
  for (int i = 0; i < 160; i++) {
    CrashTrap_Install();
    if (i == 10 || i == 25 || i == 50) CrashTrap_LogThreads();
    usleep(300 * 1000);
  }
  return nullptr;
}

extern "C" void CrashTrap_Start(void) {
  CrashTrap_Install();
  CrashTrap_LogMaps();
  pthread_t t;
  if (pthread_create(&t, nullptr, CtKeeper, nullptr) == 0) pthread_detach(t);
  __android_log_print(ANDROID_LOG_ERROR, CT_TAG, "[crash] 崩溃捕获已就绪（7 个信号 + 看门重装）");
}
