// 勇士之心 1.0.4：退出探针。
//
// 背景：星空包在真机启动后，引擎/SDK 均正常（cocos_android_app_init、onInitSuccess、菜单就绪），
// 但约 5~13s 后进程 **干净退出**（logcat 只有 `Zygote: Process N exited cleanly (0)`，
// 无 FATAL / tombstone / SIGSEGV）。需要判定调用者是谁：
//   - 壳 libsecret.so 的二次自检（DWDLM 同壳经验：PatchSecretExitNop 0x1ED8C/0x20000/0x202DC/0x3D688）
//   - 游戏自身（libcocos2djs.so 里的 gdkjsb.exitProgram；热更 JS assets/main/index.js:10968）
//   - 保护组件（libsecsdk.so / libthemis.so / Bugly_Native 等）
//
// 钩子写法沿用 jni/Games/WgdzzMod.cpp:259-345 已验证过的同类实现（libc exit/_exit +
// JNI System.exit/Runtime.exit/Process.killProcess），并补上 native 栈（_Unwind_Backtrace +
// dladdr，逐帧打印 "库名 + 0x偏移"，直接指出调用来自哪个 so）。
#include "Games/YsxzExitTrace.h"

#include <dlfcn.h>
#include <errno.h>
#include <stddef.h>
#include <linux/audit.h>
#include <linux/filter.h>
#include <linux/seccomp.h>
#include <pthread.h>
#include <fcntl.h>
#include <signal.h>
#include <stdio.h>
#include <sys/prctl.h>
#include <sys/syscall.h>
#include <ucontext.h>
#include <unwind.h>
#include <unistd.h>

#include <atomic>
#include <cstdarg>
#include <cstdint>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"

#define YEXIT_LOG "[YsxzExit] "

namespace {

JavaVM* g_vm = nullptr;
std::atomic<bool> g_started{false};
uint32_t g_hits = 0;

void (*o_exit)(int) = nullptr;
void (*o_uexit)(int) = nullptr;
void (*o_Exit)(int) = nullptr;
void (*o_abort)(void) = nullptr;
int (*o_kill)(int, int) = nullptr;
void (*o_pthread_exit)(void*) = nullptr;
void (*o_jniSystemExit)(JNIEnv*, jclass, jint) = nullptr;
void (*o_jniRuntimeExit)(JNIEnv*, jobject, jint) = nullptr;
void (*o_jniKillProcess)(JNIEnv*, jclass, jint) = nullptr;

const char* ShortLib(const char* path) {
    if (!path) return "?";
    const char* slash = strrchr(path, '/');
    return slash ? slash + 1 : path;
}

_Unwind_Reason_Code BtCb(struct _Unwind_Context* ctx, void* arg) {
    int* n = static_cast<int*>(arg);
    uintptr_t pc = _Unwind_GetIP(ctx);
    if (!pc) return _URC_NO_REASON;
    Dl_info info;
    memset(&info, 0, sizeof(info));
    if (dladdr(reinterpret_cast<void*>(pc), &info) && info.dli_fname) {
        unsigned long off = static_cast<unsigned long>(
            pc - reinterpret_cast<uintptr_t>(info.dli_fbase));
        LOGW(YEXIT_LOG "    #%d %s+0x%lx (pc=%p)", *n, ShortLib(info.dli_fname), off,
             reinterpret_cast<void*>(pc));
    } else {
        LOGW(YEXIT_LOG "    #%d pc=%p (未映射)", *n, reinterpret_cast<void*>(pc));
    }
    (*n)++;
    return (*n >= 14) ? _URC_END_OF_STACK : _URC_NO_REASON;
}

void LogNativeBt(const char* who) {
    int n = 0;
    LOGW(YEXIT_LOG "%s native 调用栈:", who);
    _Unwind_Backtrace(BtCb, &n);
}

// 打印 Java 栈（若退出由 Java 触发）。退出路径上调用 Java 有风险，故只在前几次命中时做。
void LogJavaStack(const char* who) {
    if (!g_vm) return;
    JNIEnv* env = nullptr;
    bool detach = false;
    if (g_vm->GetEnv(reinterpret_cast<void**>(&env), JNI_VERSION_1_6) == JNI_EDETACHED) {
        if (g_vm->AttachCurrentThread(&env, nullptr) != 0) return;
        detach = true;
    }
    if (!env) return;
    jclass logCls = env->FindClass("android/util/Log");
    jclass exCls = env->FindClass("java/lang/Exception");
    if (logCls && exCls) {
        jmethodID getStack = env->GetStaticMethodID(
            logCls, "getStackTraceString", "(Ljava/lang/Throwable;)Ljava/lang/String;");
        jmethodID ctor = env->GetMethodID(exCls, "<init>", "()V");
        if (getStack && ctor) {
            jobject thr = env->NewObject(exCls, ctor);
            if (thr) {
                jstring s = static_cast<jstring>(
                    env->CallStaticObjectMethod(logCls, getStack, thr));
                if (s) {
                    const char* utf = env->GetStringUTFChars(s, nullptr);
                    if (utf) {
                        LOGW(YEXIT_LOG "%s Java 栈:\n%s", who, utf);
                        env->ReleaseStringUTFChars(s, utf);
                    }
                    env->DeleteLocalRef(s);
                }
                env->DeleteLocalRef(thr);
            }
        }
    }
    if (detach) g_vm->DetachCurrentThread();
}

// ============================ 内核级退出拦截（seccomp + SIGSYS） ============================
// 第一轮实验：libc exit/_exit/abort/kill/pthread_exit 全钩上后仍出现 `exited cleanly (0)` 且一次
// 都没命中 ⇒ 调用者没走 libc 导出。可能是 (a) 内联 `svc #0` 直发 exit_group，(b) 走 bionic 的
// `syscall()` 包装。二者都逃不过 seccomp：给本进程叠加一层过滤器，把 exit(93)/exit_group(94)
// 变成 SIGSYS，处理器里打印调用点（PC/LR → 库名+偏移）并**吞掉**该 syscall（进程继续活着）。
std::atomic<uint32_t> g_syscamHits{0};

void LogAddr(const char* tag, uintptr_t addr);
void ThreadName(char* out, size_t n);

void LogAddr(const char* tag, uintptr_t addr) {
    if (!addr) {
        LOGW(YEXIT_LOG "  %s=0", tag);
        return;
    }
    Dl_info info;
    memset(&info, 0, sizeof(info));
    if (dladdr(reinterpret_cast<void*>(addr), &info) && info.dli_fname) {
        unsigned long off =
            static_cast<unsigned long>(addr - reinterpret_cast<uintptr_t>(info.dli_fbase));
        LOGW(YEXIT_LOG "  %s=%s+0x%lx (%p)", tag, ShortLib(info.dli_fname), off,
             reinterpret_cast<void*>(addr));
    } else {
        LOGW(YEXIT_LOG "  %s=%p (未映射)", tag, reinterpret_cast<void*>(addr));
    }
}

void OnSysExitTrap(int, siginfo_t*, void* uc_) {
    ucontext_t* uc = static_cast<ucontext_t*>(uc_);
    const uintptr_t pc = uc->uc_mcontext.pc;          // svc 之后的下一条指令
    const uintptr_t lr = uc->uc_mcontext.regs[30];    // 调用者的返回地址
    const unsigned long nr = uc->uc_mcontext.regs[8];  // x8 = syscall 号
    const unsigned long code = uc->uc_mcontext.regs[0];
    const uint32_t n = ++g_syscamHits;
    if (n <= 6 || n % 50 == 0) {
        LOGW(YEXIT_LOG "内核拦截退出 syscall #%u nr=%lu(%s) code=%lu", n, nr,
             nr == __NR_exit_group ? "exit_group" : (nr == __NR_exit ? "exit" : "?"), code);
        LogAddr("pc", pc);
        LogAddr("lr", lr);
    }
    // 不能返回：svc 之后是 unreachable 代码（实测 SIGILL）。用 exit(93)（未被过滤）只结束当前线程，
    // 进程继续跑；这样既拦下了“静默退出”，又不会把调用者丢进非法指令。
    char name[32];
    ThreadName(name, sizeof(name));
    LOGW(YEXIT_LOG "该线程=%s tid=%d 已被单独结束（进程存活）", name, gettid());
    syscall(__NR_exit, 0);
    while (true) usleep(1000000);
}

bool InstallSyscallTrap() {
    struct sock_filter f[] = {
        BPF_STMT(BPF_LD | BPF_W | BPF_ABS, offsetof(struct seccomp_data, arch)),
        BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, AUDIT_ARCH_AARCH64, 1, 0),
        BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW),  // 非 arm64（不可能）放行
        BPF_STMT(BPF_LD | BPF_W | BPF_ABS, offsetof(struct seccomp_data, nr)),
        BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, __NR_exit_group, 0, 1),
        BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_TRAP),
        BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW),
    };
    struct sock_fprog prog;
    prog.len = static_cast<unsigned short>(sizeof(f) / sizeof(f[0]));
    prog.filter = f;

    struct sigaction sa;
    memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = OnSysExitTrap;
    sa.sa_flags = SA_SIGINFO | SA_NODEFER | SA_RESTART;
    sigemptyset(&sa.sa_mask);
    if (sigaction(SIGSYS, &sa, nullptr) != 0) {
        LOGW(YEXIT_LOG "sigaction(SIGSYS) 失败 errno=%d", errno);
        return false;
    }
    if (prctl(PR_SET_NO_NEW_PRIVS, 1, 0, 0, 0) != 0) {
        LOGW(YEXIT_LOG "PR_SET_NO_NEW_PRIVS 失败 errno=%d", errno);
    }
    if (prctl(PR_SET_SECCOMP, SECCOMP_MODE_FILTER, &prog) != 0) {
        LOGW(YEXIT_LOG "PR_SET_SECCOMP 失败 errno=%d", errno);
        return false;
    }
    LOGI(YEXIT_LOG "内核级退出陷阱已安装（exit_group(94) → SIGSYS；命中打印调用点后仅结束该线程）");
    return true;
}

void ThreadName(char* out, size_t n) {
    out[0] = '\0';
    char path[64];
    snprintf(path, sizeof(path), "/proc/self/task/%d/comm", gettid());
    int fd = open(path, O_RDONLY | O_CLOEXEC);
    if (fd < 0) return;
    ssize_t r = read(fd, out, n - 1);
    close(fd);
    if (r > 0) {
        out[r] = '\0';
        char* nl = strchr(out, '\n');
        if (nl) *nl = '\0';
    }
}

std::atomic<uint32_t> g_faults{0};

void OnFatalSignal(int sig, siginfo_t* si, void* uc_) {
    ucontext_t* uc = static_cast<ucontext_t*>(uc_);
    const uint32_t n = ++g_faults;
    if (n <= 5) {
        char name[32];
        ThreadName(name, sizeof(name));
        LOGW(YEXIT_LOG "致命信号 %d(%s) tid=%d 线程=%s", sig,
             sig == SIGILL ? "SIGILL" : (sig == SIGSEGV ? "SIGSEGV" : (sig == SIGABRT ? "SIGABRT" : (sig == SIGBUS ? "SIGBUS" : "?"))),
             gettid(), name);
        LogAddr("fault pc", uc->uc_mcontext.pc);
        LogAddr("fault lr", uc->uc_mcontext.regs[30]);
    }
    // 只杀当前线程：syscall(exit=93) 未被过滤，线程干净退出，进程继续跑
    syscall(__NR_exit, 0);
}

void InstallFaultHandler() {
    struct sigaction sa;
    memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = OnFatalSignal;
    sa.sa_flags = SA_SIGINFO | SA_NODEFER;
    sigemptyset(&sa.sa_mask);
    const int sigs[] = {SIGILL, SIGSEGV, SIGBUS, SIGABRT, SIGFPE};
    for (int s : sigs) sigaction(s, &sa, nullptr);
    LOGI(YEXIT_LOG "致命信号现场探针已安装（SIGILL/SEGV/BUS/ABRT/FPE）");
}

void Report(const char* who, int code) {
    const uint32_t n = ++g_hits;
    LOGW(YEXIT_LOG "%s 被调用 code=%d (#%u)", who, code, n);
    if (n <= 6) {
        LogJavaStack(who);
        LogNativeBt(who);
    }
}

void H_exit(int code) {
    Report("exit", code);
    if (o_exit) o_exit(code);
}

void H_uexit(int code) {
    Report("_exit", code);
    if (o_uexit) o_uexit(code);
}

void H_Exit(int code) {
    Report("_Exit", code);
    if (o_Exit) o_Exit(code);
}

std::atomic<uint32_t> g_aborts{0};
void H_abort(void) {
    const uint32_t n = ++g_aborts;
    if (n <= 5) {
        char name[32];
        ThreadName(name, sizeof(name));
        LOGW(YEXIT_LOG "abort() (#%u) tid=%d 线程=%s", n, gettid(), name);
        LogNativeBt("abort");
    }
    if (o_abort) o_abort();
}

int H_kill(int pid, int sig) {
    if (sig == 9 || sig == 6 || sig == 15) Report("kill", sig);
    if (pid == getpid() || pid == gettid()) {
        LOGW(YEXIT_LOG "自杀式 kill(pid=%d, sig=%d)", pid, sig);
        LogNativeBt("kill");
    }
    return o_kill ? o_kill(pid, sig) : -1;
}

// bionic 的 syscall() 包装：保护组件常用 syscall(SYS_exit_group,0) 绕过 libc 导出。
long (*o_syscall)(long, ...) = nullptr;
long H_syscall(long nr, ...) {
    va_list ap;
    va_start(ap, nr);
    long a1 = va_arg(ap, long), a2 = va_arg(ap, long), a3 = va_arg(ap, long);
    long a4 = va_arg(ap, long), a5 = va_arg(ap, long), a6 = va_arg(ap, long);
    va_end(ap);
    if (nr == 93 || nr == 94) {
        const uint32_t n = ++g_hits;
        LOGW(YEXIT_LOG "syscall(nr=%ld code=%ld) 直调退出 (#%u)", nr, a1, n);
        if (n <= 3) LogNativeBt("syscall-exit");
    }
    return o_syscall ? o_syscall(nr, a1, a2, a3, a4, a5, a6) : -1;
}

void H_pthread_exit(void* ret) {
    const uint32_t n = ++g_hits;
    char name[32];
    ThreadName(name, sizeof(name));
    LOGW(YEXIT_LOG "pthread_exit(%p) (#%u) tid=%d 线程=%s", ret, n, gettid(), name);
    if (n <= 3) LogNativeBt("pthread_exit");
    if (o_pthread_exit) o_pthread_exit(ret);
}

void H_jniSystemExit(JNIEnv* env, jclass cls, jint code) {
    Report("System.exit", code);
    if (o_jniSystemExit) o_jniSystemExit(env, cls, code);
}

void H_jniRuntimeExit(JNIEnv* env, jobject self, jint code) {
    Report("Runtime.exit", code);
    if (o_jniRuntimeExit) o_jniRuntimeExit(env, self, code);
}

void H_jniKillProcess(JNIEnv* env, jclass cls, jint pid) {
    Report("Process.killProcess", pid);
    if (o_jniKillProcess) o_jniKillProcess(env, cls, pid);
}

template <typename Fn>
bool Hook(const char* lib, const char* sym, void* fn, Fn* orig, const char* label) {
    void* p = DobbySymbolResolver(lib, sym);
    if (!p) {
        LOGW(YEXIT_LOG "符号未找到 %s!%s", lib, sym);
        return false;
    }
    if (DobbyHook(p, fn, reinterpret_cast<void**>(orig)) != 0) {
        LOGW(YEXIT_LOG "hook 失败 %s", label);
        return false;
    }
    LOGI(YEXIT_LOG "hook ok %s (%s:%s @%p)", label, lib, sym, p);
    return true;
}

void InstallHooks() {
    Hook("libc.so", "exit", reinterpret_cast<void*>(H_exit), &o_exit, "libc.exit");
    Hook("libc.so", "_exit", reinterpret_cast<void*>(H_uexit), &o_uexit, "libc._exit");
    Hook("libc.so", "_Exit", reinterpret_cast<void*>(H_Exit), &o_Exit, "libc._Exit");
    Hook("libc.so", "abort", reinterpret_cast<void*>(H_abort), &o_abort, "libc.abort");
    Hook("libc.so", "kill", reinterpret_cast<void*>(H_kill), &o_kill, "libc.kill");
    Hook("libc.so", "pthread_exit", reinterpret_cast<void*>(H_pthread_exit), &o_pthread_exit,
         "libc.pthread_exit");
    Hook("libc.so", "syscall", reinterpret_cast<void*>(H_syscall), &o_syscall, "libc.syscall");
    Hook("libandroid_runtime.so", "Java_java_lang_System_exit__I",
         reinterpret_cast<void*>(H_jniSystemExit), &o_jniSystemExit, "JNI System.exit");
    if (!Hook("libandroid_runtime.so", "Java_java_lang_Runtime_exit__I",
              reinterpret_cast<void*>(H_jniRuntimeExit), &o_jniRuntimeExit, "JNI Runtime.exit")) {
        Hook("libandroid_runtime.so", "Java_java_lang_Runtime_exitInternal__I",
             reinterpret_cast<void*>(H_jniRuntimeExit), &o_jniRuntimeExit, "JNI Runtime.exitInternal");
    }
    Hook("libandroid_runtime.so", "Java_android_os_Process_killProcess__I",
         reinterpret_cast<void*>(H_jniKillProcess), &o_jniKillProcess, "JNI Process.killProcess");
}

}  // namespace

void YsxzExitTrace_Start(JavaVM* vm) {
    g_vm = vm;
    if (g_started.exchange(true)) return;
    InstallHooks();
    InstallFaultHandler();
    InstallSyscallTrap();
    LOGI(YEXIT_LOG "退出探针已安装");
}
