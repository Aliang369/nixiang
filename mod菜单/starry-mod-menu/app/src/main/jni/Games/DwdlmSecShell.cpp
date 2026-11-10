// Games/DwdlmSecShell.cpp —— 动物大联盟 1.0.25 SecShell(libsecret.so) 最小状态归一化看门狗
//
// 定位：独立环境接入模块。不并入 Games/DwdlmMod.cpp，不 hook 任何壳函数，不链接同族
// YsxzSecShell（只借鉴「序言反推状态字 + 双映像分别解析」的思路，不复制其版本表与符号）。
// 唯一调用方是私有入口 DwdlmAppComponentFactory 的 nativeEnvStart（以及 JNI_OnLoad →
// Dwdlm_OnLoad），两处都只调 DwdlmSecShell_Start()，靠 g_state 的 exchange 保证只起一个线程。
//
// ── 依据（1.0.25 原版输入 ELF 已核验，仅作注释记录，运行期不复用这些数值）──
//   libsecret.so 459344 B，sha256 e0a31698b9c14fb7ba8e677a7af136551479ce0f203941437042a4fa7fb709ae
//   （本轮复核样本：06_dumped/动物大联盟/env_1.0.25/code/raw/libsecret.so，哈希与大小一致）
//   ELF64 / aarch64 / ET_DYN / stripped
//   .dynsym 导出：SecShell_Require 0x5b0c4 / IsReady 0x5b15c / IsDirty 0x5b1a0 /
//                CheckedMask 0x5b1f4 / FailedMask 0x5b238 / ResultJson 0x5b27c
//   SecShell_IsDirty   序言 adrp x9,#0x78000 → ldr w8,[x9,#0x6bc]  ⇒ 状态字 RVA 0x786bc
//   SecShell_FailedMask 序言 adrp x9,#0x78000 → ldr w19,[x9,#0x6c8] ⇒ 状态字 RVA 0x786c8
//   PT_LOAD#3：vaddr 0x6dfb0 / filesz 0x9b78（文件 back 部分到 0x77b28）/ memsz 0xa9f0
//   （末 0x789a0）/ RW ⇒ 两个状态字都在 bss 区 [0x77b28,0x789a0) 内（本轮静态复算已核）。
//   上面这些值**只用于交叉核验**：运行期一律从当前镜像的导出符号 + 序言 ADRP/LDR 反推，
//   禁止「对旧版本偏移加差值」，也禁止沿用 1.0.23 的任何硬地址。
//
// ── 做什么 / 不做什么 ──
//   做：仅把 dirty / failedMask 两个运行态状态字归零（让壳按自检通过继续走它自己的 unshell）。
//   不做：不改 .text/.plt/.got、不改生命周期返回值、不碰 token / server-verify、不写
//         ready / checked / created / descriptor、不调用壳的任何导出函数、不遍历进程内存
//         （只按映像解析）、不读账号 / 存档 / 持久化 .sec_shell_state。
//
// ── 第二轮修复（2026-11-05 复核报告 D1/D2/D3/V3）──
//   D1 写入 TOCTOU（已确认）：原实现「每 1000ms 扫 maps 复核 + 每 1ms 直接 volatile 写」，
//      在复核通过之后、写入之前这段时间里映像被 unmap 或改成只读就 SIGSEGV；只把复核频率
//      从 1000ms 改成每 8 次一次只是缩小窗口，不消除 TOCTOU。现改为**唯一写入通道**：
//      syscall(__NR_process_vm_writev, getpid(), &local, 1, &remote, 1, 0)（aarch64=271；
//      bionic 的 process_vm_writev 包装是 API 23+，本作 minSdk 21 ⇒ 必须走 syscall 宏）。
//      内核按**当前页表权限**判定：页已 unmap / 只读 / 已回收时返回 -EFAULT/-ESRCH，本进程
//      不崩；返回字节数必须 == 4，短写不续写、不重试。系统调用不可用（ENOSYS / EPERM /
//      EACCES / EINVAL，如内核未开 CONFIG_CROSS_MEMORY_ATTACH 或被 seccomp 拦）⇒ 记明确
//      日志后 fail-closed 整体 ABORT，**不**降级为直接解引用写；不装 SIGSEGV handler、
//      不 mprotect、不写 /proc/self/mem（后者绕过页权限，正是要避免的那条路）。
//      读同一个字也走 process_vm_readv：读已 unmap 页同样会 SIGSEGV。
//      先例（真机已验证）：Games/WhrhxMod.cpp:954 在同一进程内用同一 syscall 做数值写，
//      注释记录「坏地址=失败返回，不 SIGSEGV」。本模块沿用该模式，并补 fail-closed 分支。
//   D2 失效 target 不再拖死全局（已确认）：单个 target 写返回 EFAULT/ESRCH 时压缩剔除并
//      继续其余 target；全部失效 ⇒ 明确进入 RESOLVING 有界重解析窗口（kReResolveWindowMs），
//      窗口内重新读 ELF 头、重解序言（不拿地址复用当同一镜像），窗口结束仍无 ⇒ ABORT 停止。
//   D3 目标必须是「当前可写 PT_LOAD 的 bss 段」：rva >= p_vaddr + p_filesz（文件 back 的
//      .data / .got 一律不采纳）、rva + 4 <= p_vaddr + p_memsz（不越界）、且不在
//      PT_GNU_RELRO（p_type 0x6474e552）覆盖区。只满足「落在 PF_W 段内」的候选不再采纳。
//      附带收益：bss 是匿名页，经内核写不会触发文件映射 COW，也不会把只读页偷偷变成可写。
//   V3 DT_HASH / DT_STRTAB / DT_SYMTAB / DT_GNU_HASH 的 d_val：先按 bias + d_val 试读，
//      失败时才在**原 d_val 本身落在本映像可读区间**时采纳（应对已绝对重定位的映像），
//      两者都不成立即判失败，不跨映像借用。
//
// ── 第三轮修复（2026-11-05）：通道探针必须真正 fork ──
//   问题（设计风险，已确认）：上一版把 TOCTOU 写换成 self process_vm_writev/readv 是对的，但
//   「通道自检」直接在 WatchThread 里对本进程发这两个 syscall。seccomp 若配 SECCOMP_RET_TRAP，
//   内核投递的 SIGSYS 默认动作是**终止整个进程**（不是单个线程），于是 fail-closed 的返回值
//   分支永远执行不到 —— 探针本身就把游戏打死了。返回值处理再严谨也拦不住。
//   修复：探针改为**真正独立的子进程**（bionic fork；不用另一个 pthread 冒充 —— 线程共享
//   seccomp 与信号处置，SIGSYS 照样打死整个进程）。子进程只用自身栈缓冲做 write+read 往返
//   并 _exit；父进程 waitpid(WNOHANG) 有界等待（1s），只有子进程退出码 0 才允许本线程随后
//   使用这两个 syscall。信号（含 SIGSYS）/ 非 0 退出 / 超时 / fork 失败 / waitpid 失败 ⇒ 明确
//   ABORT：父进程不发起任何 process_vm_*，不降级 volatile 直写 / pwrite / mprotect / 同进程
//   handler，不改系统 seccomp、不绕过系统限制。超时先 kill 再回收，不留僵尸。
//   探针只跑一次（CAS 起的唯一 WatchThread），ABORT 是终态，不重置、不重试。
//
// ── 安全前提（任一不满足就不写，只记日志）──
//   ① 写入通道自检：启动时对**自己栈上的 4 字节缓冲**跑一次写 + 读回，失败即 fail-closed；
//   ② 映像身份：该 base 处 ELF 头可读且 magic/class/data/type/machine 全对；
//   ③ 代码模式：导出函数序言能反推出「ADRP 页 + LDR 立即数」且页与字都在本映像可读段内；
//   ④ 段语义：状态字在 PF_W 的 PT_LOAD 的 bss 区（见 D3），不在 RELRO；
//   ⑤ 解析期 maps 预筛：状态字当前落在带 w 的映射内。**该预筛不作为写安全保证**，
//      写是否成立只看 syscall 返回值（maps 是快照，天然 TOCTOU）。

#include "DwdlmSecShell.h"

#include <dlfcn.h>
#include <link.h>
#include <jni.h>
#include <pthread.h>
#include <signal.h>
#include <stdint.h>
#include <stdio.h>
#include <string.h>
#include <sys/wait.h>
#include <time.h>
#include <unistd.h>
#include <sys/system_properties.h>

#include <atomic>
#include <errno.h>
#include <sys/syscall.h>  // __NR_process_vm_writev / __NR_process_vm_readv（aarch64: 271 / 270）
#include <sys/uio.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"

#define DWDLM_SEC_LOGI(...) LOGI("[DwdlmSec] " __VA_ARGS__)
#define DWDLM_SEC_LOGW(...) LOGW("[DwdlmSec] " __VA_ARGS__)
#define DWDLM_SEC_LOGE(...) LOGE("[DwdlmSec] " __VA_ARGS__)

#if defined(DWDLM_SEC_TEST_HOST)
// 宿主自测（macOS，无 process_vm_*）：内核中介原语由测试 TU 注入，契约与目标平台一致——
// 受页权限约束、坏地址返回错误码而不是让进程崩。声明放在匿名 namespace 之外，保证
// extern "C" 名字与测试 TU 的全局定义是同一个实体。
extern "C" long SecTestKernelAccess(uintptr_t addr, void *word, int is_write);
#endif

static JavaVM *g_dwdlm_vm = nullptr;  // nativeEnvStart 里保存，供反射线程 attach

namespace {

constexpr const char *kSecLib = "libsecret.so";

// ── 节拍（毫秒）与各类上限 ──────────────────────────────────
// 首段高频借同族实测（Ysxz 前 6s @1ms 能抢在壳首轮自检前），本作放宽到 8s 覆盖
// 「首轮 + 707ms/3.87s 级重检」；中段 20ms、尾段 200ms 降低常驻唤醒。
constexpr int64_t kFastUntilMs = 8000;
constexpr int64_t kMidUntilMs = 60000;
constexpr int64_t kTotalMs = 600000;        // 到点即 DONE（明确停止，不无限运行）
constexpr int kResolveTimeoutMs = 4000;     // 首次解析窗口：等不到可信映像就 ABORT 且不写
constexpr int kResolvePollMs = 20;
constexpr int kReResolveWindowMs = 3000;    // 全部 target 都失效后的有界重解析窗口
constexpr int kLateScanEveryMs = 2000;      // 补收新映像的节拍（只到 kMidUntilMs）
constexpr int kLateScanMax = 8;             // 补收次数硬上限
constexpr int kMaxImages = 8;
constexpr int kMaxTargets = 4;
constexpr int kMaxSegs = 12;
constexpr int kMaxAnnounceLogs = 8;         // 「已归一化」日志上限
constexpr int kMaxRejectLogs = 8;           // 解析失败日志上限
constexpr int kMaxDropLogs = 8;             // 淘汰 target 日志上限
constexpr int kWordBytes = 4;

enum {
    kStateIdle = 0,
    kStateResolving = 1,
    kStateActive = 2,
    kStateAbort = 3,
    kStateDone = 4,
};

std::atomic<int> g_state{kStateIdle};

int64_t NowMs() {
    struct timespec ts;
    if (clock_gettime(CLOCK_MONOTONIC, &ts) != 0) return 0;
    return static_cast<int64_t>(ts.tv_sec) * 1000 + ts.tv_nsec / 1000000;
}

const char *ErrName(int e) {
    switch (e) {
        case 0: return "ok";
        case EFAULT: return "EFAULT";
        case EPERM: return "EPERM";
        case EACCES: return "EACCES";
        case EINVAL: return "EINVAL";
        case ENOSYS: return "ENOSYS";
        case ESRCH: return "ESRCH";
        case ENOMEM: return "ENOMEM";
        default: return "other";
    }
}

// ── 内核中介的 4 字节读写（唯一访存通道，见 D1）──
// 返回 >=0：实际完成字节数；<0：-errno。
long RawWordAccess(uintptr_t addr, uint32_t *word, bool write) {
#if defined(DWDLM_SEC_TEST_HOST)
    return SecTestKernelAccess(addr, reinterpret_cast<void *>(word), write ? 1 : 0);
#elif defined(__linux__) && defined(__NR_process_vm_readv) && defined(__NR_process_vm_writev)
    if (addr == 0 || word == nullptr) return -EFAULT;
    struct iovec local;
    struct iovec remote;
    local.iov_base = reinterpret_cast<void *>(word);
    local.iov_len = sizeof(*word);
    remote.iov_base = reinterpret_cast<void *>(addr);
    remote.iov_len = sizeof(*word);
    errno = 0;
    const long n = write
        ? syscall(__NR_process_vm_writev, static_cast<pid_t>(getpid()), &local, 1UL, &remote,
                  1UL, 0UL)
        : syscall(__NR_process_vm_readv, static_cast<pid_t>(getpid()), &local, 1UL, &remote,
                  1UL, 0UL);
    if (n >= 0) return n;
    return -errno;
#else
    (void)addr;
    (void)word;
    (void)write;
    return -ENOSYS;  // 无该 syscall 的构建目标：走 fail-closed，绝不直接解引用
#endif
}

enum WordStatus {
    kWordOk = 0,
    kWordShort,   // 返回值 0..3：短写/短读 —— 不续写、不重试，该 target 判失效
    kWordDead,    // EFAULT / ESRCH / ENOMEM：目标页不可达（或已回收）⇒ 淘汰该 target
    kWordFatal,   // ENOSYS / EPERM / EACCES / EINVAL：机制不可用 ⇒ 整体 fail-closed
};

struct WordResult {
    WordStatus status;
    int err;
    long got;
};

WordResult AccessWord(uintptr_t addr, uint32_t *word, bool write) {
    WordResult r;
    r.status = kWordOk;
    r.err = 0;
    r.got = 0;
    if (addr == 0 || word == nullptr) {
        r.status = kWordDead;
        r.err = EFAULT;
        return r;
    }
    const long n = RawWordAccess(addr, word, write);
    r.got = n;
    if (n == kWordBytes) return r;
    if (n >= 0) {
        r.status = kWordShort;
        return r;
    }
    r.err = static_cast<int>(-n);
    switch (r.err) {
        case ENOSYS:
        case EPERM:
        case EACCES:
        case EINVAL:
            r.status = kWordFatal;
            break;
        default:
            r.status = kWordDead;
            break;
    }
    return r;
}

// ── 写入通道隔离探针（D1 补强 / 第三轮）：必须在**真正 fork 出来的子进程**里证明 ──
// 为什么不能同进程试：seccomp 用 SECCOMP_RET_TRAP 拦下 syscall 时，内核投递 SIGSYS，而 SIGSYS
// 的默认动作是**终止整个进程**（连同其它线程）；只有 SECCOMP_RET_ERRNO 那一支才让 syscall 返回
// EPERM。所以「先在本进程试一次、看返回值」在 TRAP 策略下走不到返回值，游戏当场死亡，fail-closed
// 分支形同不存在。子进程被杀只死子进程，父进程能拿到明确信号号，这才是能落地的判据。
//
// 子进程约束（fork 之后只能做 async-signal-safe 的事）：只用自身栈上的 4 字节缓冲，只调
// getpid / process_vm_writev / process_vm_readv（裸 syscall）/ _exit；不 malloc（malloc 锁可能
// 被 fork 瞬间已消失的线程持有）、不调 Java、不打日志、不开文件、不取任何锁。
// 父进程约束：waitpid(WNOHANG) 有界等待；只有「子进程正常退出码 0」才允许本线程随后使用这两个
// syscall。信号 / 非 0 退出 / 超时 / fork 失败 / waitpid 失败 ⇒ 一律 ABORT，父进程不发起
// process_vm_*，不降级任何写法。
//
// 有效性依据：seccomp 过滤器在 fork 时被子进程继承，且本线程与子进程面对同一份过滤器（若过滤器
// 只装在别的线程且未 TSYNC，则本线程与子进程同样不受它管），故子进程的结论对本线程成立。
// atfork 评估（按 bionic 的 fork 实现）：fork 由 clone(SIGCHLD) + atfork 钩子实现（父侧 prepare、
// 子侧 child），子进程是全新的单线程进程；本 child 不取 libc 锁、不分配，因此不新增死锁点。
// 最坏情况是别的库在 atfork prepare 里与第三个线程互锁，此时卡住的是本后台线程（WatchThread），
// 主线程与游戏启动路径不受影响 —— 这也正是不用「另一个 pthread」的理由：线程共享 seccomp 与
// 信号处置，SIGSYS 照样打死整个进程。
// 子进程的 getpid() 必须是子进程自身（POSIX 要求）；下面仍加了一道显式校验，不依赖对 bionic
// 内部 pid 缓存实现的信任。

enum {
    kProbeExitWrite = 1,  // 写 syscall 不可用 / 短写
    kProbeExitRead = 2,   // 读 syscall 不可用 / 短读
    kProbeExitValue = 3,  // 读回值不符
    kProbeExitPid = 4,    // 子进程 getpid() 仍等于父 pid ⇒ 目标进程不确定，不试写
};

enum ProbeFail {
    kProbePass = 0,
    kProbeForkFailed,   // fork() == -1
    kProbeChildExit,    // 子进程正常退出，但退出码 != 0
    kProbeChildSignal,  // 子进程被信号杀（SIGSYS = seccomp TRAP）
    kProbeTimeout,      // 有界超时未返回，已 kill + 回收
    kProbeWaitFailed,   // waitpid 出错，无法确认子进程状态
};

constexpr int kProbeTimeoutMs = 1000;
constexpr int kProbePollMs = 2;
// 第二道有界栅栏：即便时钟读数异常（NowMs 恒 0）也不会无限等 —— 轮询次数同样设上限。
constexpr int kProbeMaxPolls = kProbeTimeoutMs / kProbePollMs + 500;

const char *SignalName(int sig) {
    switch (sig) {
        case SIGSYS: return "SIGSYS(seccomp TRAP)";
        case SIGSEGV: return "SIGSEGV";
        case SIGBUS: return "SIGBUS";
        case SIGILL: return "SIGILL";
        case SIGABRT: return "SIGABRT";
        case SIGKILL: return "SIGKILL";
        case SIGTERM: return "SIGTERM";
        default: return "signal";
    }
}

// 子进程体：只做自身受控缓冲的往返，返回值直接当退出码。绝不返回值到父流程。
int ProbeChildBody(pid_t parent_pid) {
    // bionic 的 fork 在子侧刷新缓存 pid，getpid() 应返回子进程自身；万一没刷新，就会把
    // 「self」当成父进程去写（同一虚拟地址、不同的地址空间）⇒ 不试写，直接判失败。
    if (getpid() == parent_pid) return kProbeExitPid;
    uint32_t src = 0x5EC5EC01u;
    uint32_t dst = 0;
    const uintptr_t at = reinterpret_cast<uintptr_t>(&dst);
    if (RawWordAccess(at, &src, true) != kWordBytes) return kProbeExitWrite;
    if (RawWordAccess(at, &dst, false) != kWordBytes) return kProbeExitRead;
    if (dst != src) return kProbeExitValue;
    return 0;
}

// 返回 nullptr = 读写两个 syscall 都可用（唯一放行条件）；否则返回失败环节名，并把类别与细节
// （信号号 / 退出码 / errno）写回出参。
const char *ProbeAccess(ProbeFail *fail_out, int *detail_out) {
    *fail_out = kProbePass;
    *detail_out = 0;

    const pid_t parent_pid = getpid();
    const pid_t pid = fork();
    if (pid < 0) {
        *fail_out = kProbeForkFailed;
        *detail_out = errno;
        return "fork";
    }
    if (pid == 0) {
        _exit(ProbeChildBody(parent_pid));  // 子进程在这里结束，永不返回父流程
    }

    int status = 0;
    bool reaped = false;
    int polls = 0;
    const int64_t deadline = NowMs() + kProbeTimeoutMs;
    for (;;) {
        const pid_t got = waitpid(pid, &status, WNOHANG);
        if (got == pid) {
            reaped = true;
            break;
        }
        if (got < 0) {
            // EINTR 重试；其余（含 ECHILD：调用方若把 SIGCHLD 设成 SIG_IGN 则子进程被自动回收）
            // 一律无法确认子进程状态 ⇒ ABORT，不猜。
            if (errno == EINTR) continue;
            *fail_out = kProbeWaitFailed;
            *detail_out = errno;
            return "waitpid";
        }
        if (++polls >= kProbeMaxPolls || NowMs() >= deadline) break;
        usleep(static_cast<useconds_t>(kProbePollMs) * 1000);
    }

    if (!reaped) {
        // 超时必须 kill + 回收：既不留僵尸，也不让卡住的子进程继续占用通道。
        kill(pid, SIGKILL);
        int reap_polls = 0;
        const int64_t reap_deadline = NowMs() + kProbeTimeoutMs;
        while (!reaped && NowMs() < reap_deadline && reap_polls++ < kProbeMaxPolls) {
            const pid_t got = waitpid(pid, &status, WNOHANG);
            if (got == pid) {
                reaped = true;
                break;
            }
            if (got < 0 && errno != EINTR) break;
            usleep(static_cast<useconds_t>(kProbePollMs) * 1000);
        }
        if (!reaped) {
            // 兜底：SIGKILL 之后子进程一定会死，这里最多再阻塞回收一次（EINTR 重试一次）。
            for (int i = 0; i < 2 && !reaped; ++i) {
                if (waitpid(pid, &status, 0) == pid) reaped = true;
                else if (errno != EINTR) break;
            }
        }
        *fail_out = kProbeTimeout;
        *detail_out = 0;
        return "timeout";
    }

    if (WIFSIGNALED(status)) {
        *fail_out = kProbeChildSignal;
        *detail_out = WTERMSIG(status);
        return "child signal";
    }
    if (!WIFEXITED(status)) {
        *fail_out = kProbeChildExit;
        *detail_out = -1;
        return "child abnormal";
    }
    if (WEXITSTATUS(status) != 0) {
        *fail_out = kProbeChildExit;
        *detail_out = WEXITSTATUS(status);
        return "child exit";
    }
    return nullptr;
}

// ── 极简 ELF64 视图（不依赖 <elf.h>，便于 host / NDK 双向语法验证）──
struct Elf64Ehdr {
    unsigned char e_ident[16];
    uint16_t e_type;
    uint16_t e_machine;
    uint32_t e_version;
    uint64_t e_entry;
    uint64_t e_phoff;
    uint64_t e_shoff;
    uint32_t e_flags;
    uint16_t e_ehsize;
    uint16_t e_phentsize;
    uint16_t e_phnum;
    uint16_t e_shentsize;
    uint16_t e_shnum;
    uint16_t e_shstrndx;
};
static_assert(sizeof(Elf64Ehdr) == 64, "Elf64_Ehdr layout");

struct Elf64Phdr {
    uint32_t p_type;
    uint32_t p_flags;
    uint64_t p_offset;
    uint64_t p_vaddr;
    uint64_t p_paddr;
    uint64_t p_filesz;
    uint64_t p_memsz;
    uint64_t p_align;
};
static_assert(sizeof(Elf64Phdr) == 56, "Elf64_Phdr layout");

struct Elf64Dyn {
    int64_t d_tag;
    uint64_t d_val;
};
static_assert(sizeof(Elf64Dyn) == 16, "Elf64_Dyn layout");

struct Elf64Sym {
    uint32_t st_name;
    unsigned char st_info;
    unsigned char st_other;
    uint16_t st_shndx;
    uint64_t st_value;
    uint64_t st_size;
};
static_assert(sizeof(Elf64Sym) == 24, "Elf64_Sym layout");

constexpr uint32_t kPtLoad = 1;
constexpr uint32_t kPtDynamic = 2;
constexpr uint32_t kPtGnuRelro = 0x6474e552u;
constexpr uint32_t kPfWrite = 2;
constexpr uint16_t kEtDyn = 3;
constexpr uint16_t kEmAArch64 = 183;
constexpr int64_t kDtNull = 0;
constexpr int64_t kDtHash = 4;
constexpr int64_t kDtStrtab = 5;
constexpr int64_t kDtSymtab = 6;
constexpr int64_t kDtStrsz = 10;
constexpr int64_t kDtGnuHash = 0x6ffffef5;

struct SecSeg {
    uintptr_t start;
    uintptr_t end;
};

// 一份 libsecret 映像：base = offset==0 的映射起点，seg = 该映像的可读区间集合。
struct SecImage {
    uintptr_t base;
    int seg_count;
    SecSeg seg[kMaxSegs];
};

struct SecTarget {
    uintptr_t base;
    uintptr_t dirty;
    uintptr_t failed;
    const char *how;
    bool announced;
};

bool ImgReadable(const SecImage &img, uintptr_t addr, size_t len) {
    if (len == 0 || addr + len < addr) return false;
    for (int i = 0; i < img.seg_count; ++i) {
        if (addr >= img.seg[i].start && addr + len <= img.seg[i].end) return true;
    }
    return false;
}

template <typename T>
bool Peek(const SecImage &img, uintptr_t addr, T *out) {
    if (!ImgReadable(img, addr, sizeof(T))) return false;
    memcpy(out, reinterpret_cast<const void *>(addr), sizeof(T));
    return true;
}

// 数组实参传进 Peek 时会退化成 T*（sizeof(T) 只等于单元素），读表头必须按元素数显式取。
// 修复前 DT_HASH 的 nchain / DT_GNU_HASH 的 header 只读进第 1 个字段，sym_count 恒为 0，
// 「dlsym 看不到导出 ⇒ 退回本映像 dynsym」这条回退路径必然失败（本轮宿主测试实测）。
template <size_t N>
bool PeekWords(const SecImage &img, uintptr_t addr, uint32_t (&out)[N]) {
    const size_t bytes = N * sizeof(uint32_t);
    if (!ImgReadable(img, addr, bytes)) return false;
    memcpy(out, reinterpret_cast<const void *>(addr), bytes);
    return true;
}

// 路径末段必须就是 libsecret.so（排除 libsecret_x.so 这类前缀巧合）
bool PathIsSecLib(const char *line) {
    const char *p = strstr(line, kSecLib);
    if (!p || p == line || p[-1] != '/') return false;
    const char c = p[strlen(kSecLib)];
    return c == '\0' || c == '\n' || c == '\r' || c == ' ';
}

// 读该映像 ELF 头，取 PT_LOAD 覆盖的 vaddr 范围（只读；调用方保证 [base,readable_end) 可读）。
bool ElfVaddrSpan(uintptr_t base, uintptr_t readable_end, uintptr_t *out_lo, uintptr_t *out_hi) {
    if (readable_end <= base || readable_end - base < sizeof(Elf64Ehdr)) return false;
    Elf64Ehdr eh;
    memcpy(&eh, reinterpret_cast<const void *>(base), sizeof(eh));
    if (memcmp(eh.e_ident, "\x7f" "ELF", 4) != 0) return false;
    if (eh.e_ident[4] != 2 || eh.e_ident[5] != 1) return false;
    if (eh.e_type != kEtDyn || eh.e_machine != kEmAArch64) return false;
    if (eh.e_phentsize != sizeof(Elf64Phdr) || eh.e_phnum == 0 || eh.e_phnum > 64) return false;
    if (eh.e_phoff + static_cast<uint64_t>(eh.e_phnum) * sizeof(Elf64Phdr) >
        readable_end - base) {
        return false;
    }
    uintptr_t lo = ~static_cast<uintptr_t>(0);
    uintptr_t hi = 0;
    for (uint16_t i = 0; i < eh.e_phnum; ++i) {
        Elf64Phdr ph;
        memcpy(&ph,
               reinterpret_cast<const void *>(base + static_cast<uintptr_t>(eh.e_phoff) +
                                              static_cast<uintptr_t>(i) * sizeof(Elf64Phdr)),
               sizeof(ph));
        if (ph.p_type != kPtLoad) continue;
        if (ph.p_vaddr < lo) lo = static_cast<uintptr_t>(ph.p_vaddr);
        const uintptr_t e = static_cast<uintptr_t>(ph.p_vaddr + ph.p_memsz);
        if (e > hi) hi = e;
    }
    if (lo == ~static_cast<uintptr_t>(0) || hi <= lo) return false;
    *out_lo = base + lo;
    *out_hi = base + hi;
    return true;
}

// 把 maps 里与 [lo,hi) 相交、且带 r 的映射裁进该映像。
// **不要求映射带 so 名字**：1.0.25 真机实测（f9a116a2 / pid 27292，只读 maps）最后一个
// PT_LOAD 的 .bss 尾页 [base+0x78000, base+0x79000) 是一条无名匿名映射；按名字筛选会漏掉
// 它，而 dirty(0x786bc) / failedMask(0x786c8) 恰好落在这一页 ⇒ 会误判"不可写"而全线放弃。
int CollectSegs(uintptr_t lo, uintptr_t hi, SecSeg *segs, int max) {
    if (hi <= lo) return 0;
    FILE *fp = fopen("/proc/self/maps", "r");
    if (!fp) return 0;
    char line[512];
    int n = 0;
    while (fgets(line, sizeof(line), fp)) {
        unsigned long start = 0, end = 0;
        char perms[8] = {0};
        if (sscanf(line, "%lx-%lx %7s", &start, &end, perms) != 3) continue;
        if (end <= start) continue;
        uintptr_t s = static_cast<uintptr_t>(start);
        uintptr_t e = static_cast<uintptr_t>(end);
        if (e <= lo || s >= hi) continue;
        if (s < lo) s = lo;
        if (e > hi) e = hi;
        if (perms[0] != 'r' || n >= max) continue;
        segs[n].start = s;
        segs[n].end = e;
        ++n;
    }
    fclose(fp);
    return n;
}

// 收集 libsecret.so 的**全部**映像（APKSecure 下实测两份：粗粒度 JNI 映像 + 保护映像）。
// 每份映像的可读区间 = ELF 声明的 PT_LOAD 范围 ∩ maps 可读段（含匿名 bss 尾页）。
int CollectSecImages(SecImage *out, int max) {
    uintptr_t bases[kMaxImages];
    uintptr_t first_end[kMaxImages];
    int nb = 0;
    FILE *fp = fopen("/proc/self/maps", "r");
    if (!fp) return 0;
    char line[512];
    while (fgets(line, sizeof(line), fp)) {
        if (!PathIsSecLib(line)) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perms[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perms, &off) != 4) continue;
        if (off != 0 || end <= start || perms[0] != 'r') continue;
        bool dup = false;
        for (int i = 0; i < nb; ++i) {
            if (bases[i] == static_cast<uintptr_t>(start)) dup = true;
        }
        if (dup || nb >= max) continue;
        bases[nb] = static_cast<uintptr_t>(start);
        first_end[nb] = static_cast<uintptr_t>(end);
        ++nb;
    }
    fclose(fp);

    int n = 0;
    for (int i = 0; i < nb; ++i) {
        uintptr_t lo = 0;
        uintptr_t hi = 0;
        SecImage *img = &out[n];
        img->base = bases[i];
        img->seg_count = 0;
        ++n;
        if (!ElfVaddrSpan(bases[i], first_end[i], &lo, &hi)) {
            // ELF 身份不过：只留首段最小视图，让 ResolveImage 记下明确原因后放弃
            img->seg[0].start = bases[i];
            img->seg[0].end = first_end[i];
            img->seg_count = 1;
            continue;
        }
        img->seg_count = CollectSegs(lo, hi, img->seg, kMaxSegs);
    }
    for (int i = 1; i < n; ++i) {  // 升序：guest 地址段排在宿主地址段之前
        SecImage tmp = out[i];
        int j = i - 1;
        while (j >= 0 && out[j].base > tmp.base) {
            out[j + 1] = out[j];
            --j;
        }
        out[j + 1] = tmp;
    }
    return n;
}

// 地址当前是否落在带 w 的映射内。**只作解析期预筛**：maps 是快照，写前/写后都可能过期，
// 因此它不是写安全保证（D1 的 TOCTOU 正是这么来的）；写是否成立只看 syscall 返回值。
bool AddrWritable(uintptr_t addr) {
    FILE *fp = fopen("/proc/self/maps", "r");
    if (!fp) return false;
    char line[512];
    bool ok = false;
    while (fgets(line, sizeof(line), fp)) {
        unsigned long start = 0, end = 0;
        char perms[8] = {0};
        if (sscanf(line, "%lx-%lx %7s", &start, &end, perms) != 3) continue;
        if (addr >= start && addr < end) {
            ok = (perms[1] == 'w');
            break;
        }
    }
    fclose(fp);
    return ok;
}

// GNU_HASH 反推 dynsym 条目数（无 DT_HASH 时用）
uint32_t GnuHashSymCount(const SecImage &img, uintptr_t gh) {
    uint32_t hdr[4] = {0, 0, 0, 0};
    if (!PeekWords(img, gh, hdr)) return 0;
    const uint32_t nbuckets = hdr[0];
    const uint32_t symoffset = hdr[1];
    const uint32_t bloom_size = hdr[2];
    if (nbuckets == 0 || nbuckets > 65536u || bloom_size > 65536u) return 0;
    const uintptr_t buckets = gh + 16 + static_cast<uintptr_t>(bloom_size) * 8;
    uint32_t last = 0;
    for (uint32_t i = 0; i < nbuckets; ++i) {
        uint32_t b = 0;
        if (!Peek(img, buckets + static_cast<uintptr_t>(i) * 4, &b)) return 0;
        if (b > last) last = b;
    }
    if (last < symoffset) return symoffset;
    const uintptr_t chain = buckets + static_cast<uintptr_t>(nbuckets) * 4;
    uint32_t idx = last;
    for (uint32_t guard = 0; guard < (1u << 20); ++guard) {
        uint32_t v = 0;
        if (!Peek(img, chain + static_cast<uintptr_t>(idx - symoffset) * 4, &v)) return 0;
        if ((v & 1u) != 0) return idx + 1;
        ++idx;
    }
    return 0;
}

// DT_* 里的指针：先试 bias + d_val（常见：未重定位的 RVA），失败且**原 d_val 本身**落在
// 本映像可读区间时才采纳 d_val（已绝对重定位的映像）；两者都不成立 ⇒ 失败，不跨映像借用。
bool ResolveDynPtr(const SecImage &img, uintptr_t bias, uint64_t d_val, size_t need,
                   uintptr_t *out) {
    const uintptr_t max_addr = ~static_cast<uintptr_t>(0);
    if (bias <= max_addr && d_val <= static_cast<uint64_t>(max_addr - bias)) {
        const uintptr_t biased = bias + static_cast<uintptr_t>(d_val);
        if (ImgReadable(img, biased, need)) {
            *out = biased;
            return true;
        }
    }
    const uintptr_t raw = static_cast<uintptr_t>(d_val);
    if (raw != 0 && ImgReadable(img, raw, need)) {
        *out = raw;
        return true;
    }
    return false;
}

// 本映像自己的 dynsym 里找导出（dlsym 全局看不到时走这条路）
uintptr_t ElfFindExport(const SecImage &img, const char *want, const char **why) {
    Elf64Ehdr eh;
    if (!Peek(img, img.base, &eh)) {
        *why = "ehdr unreadable";
        return 0;
    }
    if (memcmp(eh.e_ident, "\x7f" "ELF", 4) != 0) {
        *why = "not ELF";
        return 0;
    }
    if (eh.e_ident[4] != 2 || eh.e_ident[5] != 1) {
        *why = "not ELF64/LSB";
        return 0;
    }
    if (eh.e_type != kEtDyn) {
        *why = "not ET_DYN";
        return 0;
    }
    if (eh.e_machine != kEmAArch64) {
        *why = "not AArch64";
        return 0;
    }
    if (eh.e_phentsize != sizeof(Elf64Phdr) || eh.e_phnum == 0 || eh.e_phnum > 64) {
        *why = "bad phdr table";
        return 0;
    }
    uintptr_t min_vaddr = ~static_cast<uintptr_t>(0);
    uintptr_t dyn_vaddr = 0;
    uint64_t dyn_size = 0;
    for (uint16_t i = 0; i < eh.e_phnum; ++i) {
        Elf64Phdr ph;
        if (!Peek(img, img.base + static_cast<uintptr_t>(eh.e_phoff) +
                           static_cast<uintptr_t>(i) * sizeof(Elf64Phdr), &ph)) {
            *why = "phdr unreadable";
            return 0;
        }
        if (ph.p_type == kPtLoad && ph.p_vaddr < min_vaddr) {
            min_vaddr = static_cast<uintptr_t>(ph.p_vaddr);
        }
        if (ph.p_type == kPtDynamic) {
            dyn_vaddr = static_cast<uintptr_t>(ph.p_vaddr);
            dyn_size = ph.p_memsz;
        }
    }
    if (min_vaddr == ~static_cast<uintptr_t>(0) || dyn_vaddr == 0) {
        *why = "no PT_LOAD/PT_DYNAMIC";
        return 0;
    }
    if (min_vaddr > img.base) {
        *why = "base below min vaddr";
        return 0;
    }
    const uintptr_t bias = img.base - min_vaddr;
    uintptr_t symtab = 0;
    uintptr_t strtab = 0;
    uintptr_t hash = 0;
    uintptr_t gnu = 0;
    uint64_t strsz = 0;
    uint64_t hash_val = 0;
    uint64_t gnu_val = 0;
    uint64_t sym_val = 0;
    uint64_t str_val = 0;
    const uintptr_t dyn = bias + dyn_vaddr;
    const uint64_t dyn_n = dyn_size / sizeof(Elf64Dyn);
    bool bad_ptr = false;
    for (uint64_t i = 0; i < dyn_n; ++i) {
        Elf64Dyn d;
        if (!Peek(img, dyn + static_cast<uintptr_t>(i) * sizeof(Elf64Dyn), &d)) break;
        if (d.d_tag == kDtNull) break;
        switch (d.d_tag) {
            case kDtHash: hash_val = d.d_val; break;
            case kDtStrtab: str_val = d.d_val; break;
            case kDtSymtab: sym_val = d.d_val; break;
            case kDtStrsz: strsz = d.d_val; break;
            case kDtGnuHash: gnu_val = d.d_val; break;
            default: break;
        }
    }
    if (strsz == 0) {
        *why = "no dynstr";
        return 0;
    }
    if (sym_val != 0 && !ResolveDynPtr(img, bias, sym_val, sizeof(Elf64Sym), &symtab)) {
        bad_ptr = true;
    }
    if (str_val != 0 && !ResolveDynPtr(img, bias, str_val, 8, &strtab)) bad_ptr = true;
    if (hash_val != 0 && !ResolveDynPtr(img, bias, hash_val, 8, &hash)) bad_ptr = true;
    if (gnu_val != 0 && !ResolveDynPtr(img, bias, gnu_val, 16, &gnu)) bad_ptr = true;
    if (bad_ptr || symtab == 0 || strtab == 0) {
        *why = bad_ptr ? "dyn ptr outside image" : "no dynsym/dynstr";
        return 0;
    }
    uint32_t sym_count = 0;
    if (hash != 0) {
        uint32_t h[2] = {0, 0};
        if (PeekWords(img, hash, h)) sym_count = h[1];
    }
    if (sym_count == 0 && gnu != 0) sym_count = GnuHashSymCount(img, gnu);
    if (sym_count == 0) {
        *why = "dynsym count unknown";
        return 0;
    }
    if (sym_count > 200000u) sym_count = 200000u;
    const size_t want_len = strlen(want);
    for (uint32_t i = 1; i < sym_count; ++i) {
        Elf64Sym sym;
        if (!Peek(img, symtab + static_cast<uintptr_t>(i) * sizeof(Elf64Sym), &sym)) break;
        if (sym.st_name == 0 || static_cast<uint64_t>(sym.st_name) >= strsz) continue;
        const uintptr_t nm = strtab + sym.st_name;
        if (!ImgReadable(img, nm, want_len + 1)) continue;
        if (memcmp(reinterpret_cast<const void *>(nm), want, want_len + 1) != 0) continue;
        if (sym.st_value == 0) continue;
        *why = "dynsym";
        return bias + static_cast<uintptr_t>(sym.st_value);
    }
    *why = "export not in dynsym";
    return 0;
}

// 从访问器序言反推它读的状态字：第一个 ADRP 定页，其后第一个以该页寄存器为基址的
// LDR Wt,[Xn,#imm12*4] 即目标。页与字都必须落在本映像可读段内，否则判失败。
bool DecodeStateWord(const SecImage &img, uintptr_t fn, uintptr_t *out) {
    if (!ImgReadable(img, fn, 16)) return false;
    uintptr_t page = 0;
    int rd = -1;
    for (int i = 0; i < 32; ++i) {
        const uintptr_t at = fn + static_cast<uintptr_t>(i) * 4;
        if (!ImgReadable(img, at, 4)) return false;
        uint32_t insn = 0;
        memcpy(&insn, reinterpret_cast<const void *>(at), 4);
        if (insn == 0xD65F03C0u) break;  // ret
        if ((insn & 0x9F000000u) == 0x90000000u) {  // ADRP Xd,#imm
            int64_t imm = static_cast<int64_t>((insn >> 5) & 0x7FFFFu) << 2;
            imm |= static_cast<int64_t>((insn >> 29) & 0x3u);
            if (imm & (1LL << 20)) imm -= (1LL << 21);
            const uintptr_t pc_page = at & ~static_cast<uintptr_t>(0xFFF);
            page = static_cast<uintptr_t>(static_cast<int64_t>(pc_page) + (imm << 12));
            rd = static_cast<int>(insn & 0x1Fu);
            continue;
        }
        if (rd >= 0 && (insn & 0xFFC00000u) == 0xB9400000u) {  // LDR Wt,[Xn,#imm12*4]
            const int rn = static_cast<int>((insn >> 5) & 0x1Fu);
            if (rn == rd) {
                const uintptr_t word =
                        page + static_cast<uintptr_t>(((insn >> 10) & 0xFFFu) * 4);
                if (!ImgReadable(img, page, 4) || !ImgReadable(img, word, 4)) return false;
                *out = word;
                return true;
            }
        }
    }
    return false;
}

enum BssCheck {
    kBssOk = 0,
    kBssBadElf,
    kBssNotWritableLoad,
    kBssFileBacked,
    kBssOutOfMemsz,
    kBssRelro,
};

const char *BssWhy(BssCheck c) {
    switch (c) {
        case kBssOk: return "ok";
        case kBssBadElf: return "bad ELF/bias";
        case kBssNotWritableLoad: return "outside PF_W PT_LOAD";
        case kBssFileBacked: return "inside file-backed data (.data/.got)";
        case kBssOutOfMemsz: return "crosses PT_LOAD memsz end";
        case kBssRelro: return "inside PT_GNU_RELRO";
        default: return "?";
    }
}

// D3：状态字必须是「本映像当前可写 PT_LOAD 的 bss 段」里的 4 字节区：
//   rva >= p_vaddr + p_filesz（文件 back 的 .data/.got 不接受）、
//   rva + 4 <= p_vaddr + p_memsz（不越界，含 64 位溢出检查）、
//   不在 PT_GNU_RELRO 覆盖区。
BssCheck CheckStateWord(const SecImage &img, uintptr_t addr) {
    Elf64Ehdr eh;
    if (!Peek(img, img.base, &eh)) return kBssBadElf;
    if (memcmp(eh.e_ident, "\x7f" "ELF", 4) != 0) return kBssBadElf;
    if (eh.e_phentsize != sizeof(Elf64Phdr) || eh.e_phnum == 0 || eh.e_phnum > 64) {
        return kBssBadElf;
    }
    uintptr_t min_vaddr = ~static_cast<uintptr_t>(0);
    for (uint16_t i = 0; i < eh.e_phnum; ++i) {
        Elf64Phdr ph;
        if (!Peek(img, img.base + static_cast<uintptr_t>(eh.e_phoff) +
                           static_cast<uintptr_t>(i) * sizeof(Elf64Phdr), &ph)) {
            return kBssBadElf;
        }
        if (ph.p_type == kPtLoad && ph.p_vaddr < min_vaddr) {
            min_vaddr = static_cast<uintptr_t>(ph.p_vaddr);
        }
    }
    if (min_vaddr == ~static_cast<uintptr_t>(0)) return kBssBadElf;
    if (addr < img.base || min_vaddr > img.base) return kBssBadElf;
    const uint64_t rva = static_cast<uint64_t>(min_vaddr) + (addr - img.base);

    bool in_writable_load = false;
    bool in_bss = false;
    bool has_relro = false;
    uint64_t relro_lo = 0;
    uint64_t relro_hi = 0;
    const uint64_t max64 = ~static_cast<uint64_t>(0);
    for (uint16_t i = 0; i < eh.e_phnum; ++i) {
        Elf64Phdr ph;
        if (!Peek(img, img.base + static_cast<uintptr_t>(eh.e_phoff) +
                           static_cast<uintptr_t>(i) * sizeof(Elf64Phdr), &ph)) {
            return kBssBadElf;
        }
        if (ph.p_type == kPtGnuRelro) {
            if (ph.p_vaddr <= max64 - ph.p_memsz) {
                relro_lo = ph.p_vaddr;
                relro_hi = ph.p_vaddr + ph.p_memsz;
                has_relro = true;
            }
            continue;
        }
        if (ph.p_type != kPtLoad || (ph.p_flags & kPfWrite) == 0) continue;
        if (ph.p_filesz > ph.p_memsz) continue;         // 段自相矛盾：不采纳
        if (ph.p_vaddr > max64 - ph.p_memsz) continue;  // 溢出：不采纳
        const uint64_t seg_lo = ph.p_vaddr;
        const uint64_t file_end = ph.p_vaddr + ph.p_filesz;
        const uint64_t mem_end = ph.p_vaddr + ph.p_memsz;
        if (rva < seg_lo || rva >= mem_end) continue;
        in_writable_load = true;
        if (rva < file_end) return kBssFileBacked;
        if (mem_end < static_cast<uint64_t>(kWordBytes) ||
            rva > mem_end - static_cast<uint64_t>(kWordBytes)) {
            return kBssOutOfMemsz;
        }
        in_bss = true;
    }
    if (!in_bss) return in_writable_load ? kBssOutOfMemsz : kBssNotWritableLoad;
    if (has_relro && rva >= relro_lo && rva < relro_hi) return kBssRelro;
    return kBssOk;
}

// 解析一份映像：导出 → 序言 → 状态字 → bss 段语义 → 解析期 maps 预筛。任一环失败即放弃本份。
bool ResolveImage(const SecImage &img, SecTarget *t, char *why, size_t why_sz) {
    t->base = img.base;
    t->dirty = 0;
    t->failed = 0;
    t->how = "?";
    t->announced = false;

    const uintptr_t dl_dirty =
            reinterpret_cast<uintptr_t>(dlsym(RTLD_DEFAULT, "SecShell_IsDirty"));
    const uintptr_t dl_failed =
            reinterpret_cast<uintptr_t>(dlsym(RTLD_DEFAULT, "SecShell_FailedMask"));

    bool dirty_via_dlsym = false;
    bool failed_via_dlsym = false;
    uintptr_t fn_dirty = 0;
    uintptr_t fn_failed = 0;

    // dlsym 只在本映像范围内才采纳（跨映像地址不能用来解本映像的序言）
    if (dl_dirty != 0 && ImgReadable(img, dl_dirty, 4)) {
        fn_dirty = dl_dirty;
        dirty_via_dlsym = true;
    }
    if (dl_failed != 0 && ImgReadable(img, dl_failed, 4)) {
        fn_failed = dl_failed;
        failed_via_dlsym = true;
    }
    if (fn_dirty == 0) {
        const char *w = "";
        fn_dirty = ElfFindExport(img, "SecShell_IsDirty", &w);
        if (fn_dirty == 0) {
            snprintf(why, why_sz, "IsDirty: %s", w);
            return false;
        }
    }
    if (fn_failed == 0) {
        const char *w = "";
        fn_failed = ElfFindExport(img, "SecShell_FailedMask", &w);
        if (fn_failed == 0) {
            snprintf(why, why_sz, "FailedMask: %s", w);
            return false;
        }
    }
    t->how = (dirty_via_dlsym && failed_via_dlsym)
                     ? "dlsym"
                     : ((!dirty_via_dlsym && !failed_via_dlsym) ? "dynsym" : "dlsym+dynsym");

    uintptr_t d = 0;
    uintptr_t f = 0;
    if (!DecodeStateWord(img, fn_dirty, &d)) {
        snprintf(why, why_sz, "IsDirty prologue decode failed");
        return false;
    }
    if (!DecodeStateWord(img, fn_failed, &f)) {
        snprintf(why, why_sz, "FailedMask prologue decode failed");
        return false;
    }
    if (d == f) {
        snprintf(why, why_sz, "decode collision (dirty==failed)");
        return false;
    }
    const BssCheck cd = CheckStateWord(img, d);
    if (cd != kBssOk) {
        snprintf(why, why_sz, "dirty word rejected: %s", BssWhy(cd));
        return false;
    }
    const BssCheck cf = CheckStateWord(img, f);
    if (cf != kBssOk) {
        snprintf(why, why_sz, "failed word rejected: %s", BssWhy(cf));
        return false;
    }
    if (!AddrWritable(d)) {
        snprintf(why, why_sz, "dirty word not writable in maps (prefilter)");
        return false;
    }
    if (!AddrWritable(f)) {
        snprintf(why, why_sz, "failed word not writable in maps (prefilter)");
        return false;
    }
    t->dirty = d;
    t->failed = f;
    return true;
}

// 把一批映像里能解析出来的都收进来（只对**仍存活**的 target 按 base 去重）。返回新增条数。
// 不缓存「base → 状态字」：每次都是重新读 ELF 头 + 重解序言，地址复用不会被当成同一镜像。
int AppendTargets(const SecImage *imgs, int n, SecTarget *targets, int &ntarget, int max,
                  int *reject_logs) {
    int added = 0;
    for (int i = 0; i < n && ntarget < max; ++i) {
        SecTarget t;
        char why[112] = {0};
        if (ResolveImage(imgs[i], &t, why, sizeof(why))) {
            bool dup = false;
            for (int k = 0; k < ntarget; ++k) {
                if (targets[k].base == t.base) dup = true;
            }
            if (dup) continue;
            targets[ntarget] = t;
            ++ntarget;
            ++added;
            DWDLM_SEC_LOGI("image[%d] base=%p dirty=%p failed=%p via %s", i,
                           reinterpret_cast<void *>(t.base),
                           reinterpret_cast<void *>(t.dirty),
                           reinterpret_cast<void *>(t.failed), t.how);
        } else if (*reject_logs < kMaxRejectLogs) {
            ++(*reject_logs);
            DWDLM_SEC_LOGW("image[%d] base=%p rejected: %s", i,
                           reinterpret_cast<void *>(imgs[i].base), why);
        }
    }
    return added;
}

// 淘汰一个失效 target 并压缩数组（D2：不影响其余映像继续归一化）。
void DropTarget(SecTarget *targets, int &ntarget, int idx, int *drop_logs, int64_t elapsed,
                const char *what, const WordResult &r) {
    if (*drop_logs < kMaxDropLogs) {
        ++(*drop_logs);
        DWDLM_SEC_LOGW("drop image base=%p at %lldms: %s failed errno=%s(%d) got=%ld "
                       "(page unmapped/read-only or image gone); targets left=%d",
                       reinterpret_cast<void *>(targets[idx].base),
                       static_cast<long long>(elapsed), what, ErrName(r.err), r.err, r.got,
                       ntarget - 1);
    }
    for (int k = idx; k + 1 < ntarget; ++k) targets[k] = targets[k + 1];
    --ntarget;
}

// ── 引擎侧壳接口表（照 1.0.23：改引擎的可写表项，不动 libsecret 的 .text）──
// 1.0.23 硬编码 base+0x221B800..0x221B838；1.0.25 偏移已变（官方运行期实测落在引擎可写段
// 内相对 +0x52920，绝对基址无法从离线 dump 反推）⇒ 改为**按函数指针模式定位**：
// 连续 5 个 8 字节槽位依次等于 libsecret 的 IsReady/IsDirty/CheckedMask/FailedMask/Require。
// 语义照抄 1.0.23：IsReady→1、IsDirty→0、CheckedMask→0x7EF、FailedMask→0、Require→0。
extern "C" int DwdlmSecTbl_IsReady() { return 1; }
extern "C" int DwdlmSecTbl_IsDirty() { return 0; }
extern "C" int DwdlmSecTbl_CheckedMask() { return 0x7EF; }
extern "C" int DwdlmSecTbl_FailedMask() { return 0; }
extern "C" int DwdlmSecTbl_Require(const char *, const char *, const char *) { return 0; }

struct CocosSegScan {
    uintptr_t want[5];
    uintptr_t slot;
    bool found;
};

int CocosPhdrCb(struct dl_phdr_info *info, size_t, void *data) {
    auto *s = static_cast<CocosSegScan *>(data);
    if (info->dlpi_name == nullptr ||
        strstr(info->dlpi_name, "libcocos2djs.so") == nullptr) {
        return 0;
    }
    for (int i = 0; i < info->dlpi_phnum; ++i) {
        const ElfW(Phdr) &ph = info->dlpi_phdr[i];
        if (ph.p_type != PT_LOAD) continue;
        if ((ph.p_flags & PF_W) == 0) continue;
        const uintptr_t start = info->dlpi_addr + ph.p_vaddr;
        const size_t len = ph.p_memsz;
        if (len < sizeof(s->want) || start == 0) continue;
        for (size_t off = 0; off + sizeof(s->want) <= len; off += 8) {
            const uintptr_t *q = reinterpret_cast<const uintptr_t *>(start + off);
            if (q[0] != s->want[0]) continue;
            if (memcmp(q, s->want, sizeof(s->want)) != 0) continue;
            s->slot = start + off;
            s->found = true;
            return 1;
        }
    }
    return 0;
}

std::atomic<int> g_tbl_state{0};  // 0=未处理 1=已换 2=未找到

// 诊断门控：debug.dwdlm.diagonly=1 时本 so 只读 —— 不扫壳表、不写状态字。
// 目的：把「我们的干预」从壳行为里摘出去做单变量对照（进程启动时只读一次属性）。
bool DiagOnly() {
    static const bool v = [] {
        char b[PROP_VALUE_MAX] = {0};
        return __system_property_get("debug.dwdlm.diagonly", b) > 0 && b[0] == '1';
    }();
    return v;
}

void PatchCocosShellTable() {
    if (DiagOnly()) { return; }
    if (g_tbl_state.load(std::memory_order_relaxed) != 0) return;
    SecImage imgs[kMaxImages];
    const int n = CollectSecImages(imgs, kMaxImages);
    if (n <= 0) return;
    static const char *kNames[5] = {"SecShell_IsReady", "SecShell_IsDirty",
                                    "SecShell_CheckedMask", "SecShell_FailedMask",
                                    "SecShell_Require"};
    CocosSegScan scan{};
    for (int i = 0; i < 5; ++i) {
        const char *why = nullptr;
        const uintptr_t fn = ElfFindExport(imgs[0], kNames[i], &why);
        if (fn == 0) {
            static std::atomic<int> wlog{0};
            if (++wlog <= 2) {
                DWDLM_SEC_LOGW("shell table: export %s unresolved (%s)", kNames[i],
                               why ? why : "?");
            }
            return;
        }
        scan.want[i] = fn;
    }
    dl_iterate_phdr(CocosPhdrCb, &scan);
    if (!scan.found) {
        static std::atomic<int> nlog{0};
        if (++nlog <= 40) {
            DWDLM_SEC_LOGI("shell table: pattern not found yet (retry)");
        }
        return;
    }
    auto *q = reinterpret_cast<uintptr_t *>(scan.slot);
    q[0] = reinterpret_cast<uintptr_t>(&DwdlmSecTbl_IsReady);
    q[1] = reinterpret_cast<uintptr_t>(&DwdlmSecTbl_IsDirty);
    q[2] = reinterpret_cast<uintptr_t>(&DwdlmSecTbl_CheckedMask);
    q[3] = reinterpret_cast<uintptr_t>(&DwdlmSecTbl_FailedMask);
    q[4] = reinterpret_cast<uintptr_t>(&DwdlmSecTbl_Require);
    g_tbl_state.store(1);
    DWDLM_SEC_LOGI("shell table patched at slot=%p (5 entries -> 1/0/0x7EF/0/0)",
                   reinterpret_cast<void *>(scan.slot));
}

void InstallBootTrace();
void DumpSecretsOnce();

void *WatchThread(void *) {
    g_state.store(kStateResolving);
    InstallBootTrace();

    // ① 先证明写入通道可用 —— 但必须在**真正 fork 出来的子进程**里证明（理由见 ProbeAccess）：
    //    seccomp 的 SECCOMP_RET_TRAP 以 SIGSYS 终止整个进程，同进程探针走不到返回值分支。
    //    子进程被杀 / 非 0 退出 / 超时 / fork 失败 / waitpid 失败 ⇒ fail-closed：本进程之后
    //    一个 process_vm_* 都不发，也不降级为任何直写路径。
    ProbeFail probe_kind = kProbePass;
    int probe_detail = 0;
    const char *probe_why = ProbeAccess(&probe_kind, &probe_detail);
    if (probe_why != nullptr) {
        switch (probe_kind) {
            case kProbeChildSignal:
                DWDLM_SEC_LOGE("isolated probe denied: child killed by signal %d = %s -> "
                               "process_vm_* blocked in this sandbox; fail-closed, no write, "
                               "no fallback, abort",
                               probe_detail, SignalName(probe_detail));
                break;
            case kProbeTimeout:
                DWDLM_SEC_LOGE("isolated probe timeout after %dms: child never returned; "
                               "killed+reaped -> fail-closed, no write, no fallback, abort",
                               kProbeTimeoutMs);
                break;
            case kProbeChildExit:
                DWDLM_SEC_LOGE("isolated probe denied: child exit=%d "
                               "(1=write 2=read 3=value 4=pid) -> fail-closed, no write, abort",
                               probe_detail);
                break;
            case kProbeForkFailed:
            case kProbeWaitFailed:
                DWDLM_SEC_LOGE("isolated probe could not run (%s errno=%s(%d)); cannot prove "
                               "channel availability -> fail-closed, no write, abort",
                               probe_why, ErrName(probe_detail), probe_detail);
                break;
            default:
                DWDLM_SEC_LOGE("isolated probe failed (%s); fail-closed, no write, abort",
                               probe_why);
                break;
        }
        g_state.store(kStateAbort);
        return nullptr;
    }
    DWDLM_SEC_LOGI("isolated 4B probe ok: forked child did write+read round-trip and exited 0; "
                   "kernel-mediated path usable");

    SecTarget targets[kMaxTargets];
    int ntarget = 0;
    int last_seen = -1;
    int reject_logs = 0;
    int drop_logs = 0;
    int announce_logs = 0;
    int late_scans = 0;

    const int64_t t0 = NowMs();
    int64_t elapsed = 0;

    // 阶段 1：等第一份可信映像。壳可能还没 dlopen libsecret（本入口早于壳的 Application），
    // 所以「边等边验」；到 kResolveTimeoutMs 仍无结果就 ABORT 且不写。
    while (ntarget == 0 && elapsed <= kResolveTimeoutMs) {
        SecImage imgs[kMaxImages];
        const int n = CollectSecImages(imgs, kMaxImages);
        if (n != last_seen) {
            last_seen = n;
            DWDLM_SEC_LOGI("libsecret images=%d (waited=%lldms)", n,
                           static_cast<long long>(elapsed));
        }
        AppendTargets(imgs, n, targets, ntarget, kMaxTargets, &reject_logs);
        if (ntarget == 0) {
            usleep(static_cast<useconds_t>(kResolvePollMs) * 1000);
            elapsed = NowMs() - t0;
        }
    }

    if (ntarget == 0) {
        DWDLM_SEC_LOGE("no trusted state word within %dms; no write performed, abort",
                       kResolveTimeoutMs);
        g_state.store(kStateAbort);
        return nullptr;
    }

    g_state.store(kStateActive);
    DWDLM_SEC_LOGI("watchdog active: targets=%d", ntarget);

    int64_t next_late_scan = kLateScanEveryMs;

    while (elapsed < kTotalMs) {
        bool fatal = false;
        int fatal_err = 0;

        PatchCocosShellTable();
        InstallBootTrace();
        DumpSecretsOnce();

        // ② 只写 dirty / failedMask 两个运行态状态字，每字 4 字节，经内核中介通道。
        //    写回 0 是幂等的；写失败按 errno 分流：目标页没了 ⇒ 淘汰该 target 并继续其余，
        //    机制不可用 ⇒ 整体 fail-closed。
        for (int i = 0; i < ntarget;) {
            uint32_t zero = 0;
            const WordResult wd = AccessWord(targets[i].dirty, &zero, !DiagOnly());
            if (wd.status == kWordFatal) {
                fatal = true;
                fatal_err = wd.err;
                break;
            }
            if (wd.status != kWordOk) {
                DropTarget(targets, ntarget, i, &drop_logs, elapsed, "dirty", wd);
                continue;
            }
            const WordResult wf = AccessWord(targets[i].failed, &zero, !DiagOnly());
            if (wf.status == kWordFatal) {
                fatal = true;
                fatal_err = wf.err;
                break;
            }
            if (wf.status != kWordOk) {
                DropTarget(targets, ntarget, i, &drop_logs, elapsed, "failedMask", wf);
                continue;
            }
            if (!targets[i].announced) {
                targets[i].announced = true;
                // 诊断（一次性）：读壳 .bss 的 flag / IV / KEY，用于 const32 修正。
                // 同进程读自己的映像，无需 root / ptrace；只读，不写。
                {
                    const uintptr_t b = targets[i].base + 0x786a0u;
                    const uint8_t *pp = reinterpret_cast<const uint8_t *>(b);
                    static const char kHex[] = "0123456789abcdef";
                    char hex[0x300 * 2 + 1];
                    for (size_t k = 0; k < 0x300; ++k) {
                        hex[k * 2] = kHex[(pp[k] >> 4) & 0xF];
                        hex[k * 2 + 1] = kHex[pp[k] & 0xF];
                    }
                    hex[0x300 * 2] = 0;
                    DWDLM_SEC_LOGI("secrets @%p: %s", reinterpret_cast<void *>(b), hex);
                }
                if (announce_logs < kMaxAnnounceLogs) {
                    ++announce_logs;
                    DWDLM_SEC_LOGI("wrote dirty=0 failed=0 to image base=%p at %lldms "
                                   "(targets=%d)",
                                   reinterpret_cast<void *>(targets[i].base),
                                   static_cast<long long>(elapsed), ntarget);
                }
            }
            ++i;
        }

        if (fatal) {
            DWDLM_SEC_LOGE("write path rejected (errno=%s(%d)) at %lldms; fail-closed: "
                           "stopping all writes, no direct dereference fallback, abort",
                           ErrName(fatal_err), fatal_err, static_cast<long long>(elapsed));
            g_state.store(kStateAbort);
            return nullptr;
        }

        // ③ 全部 target 失效 ⇒ 进入 RESOLVING 有界重解析窗口；窗口内重新解析（不是复用旧地址）。
        if (ntarget == 0) {
            g_state.store(kStateResolving);
            DWDLM_SEC_LOGW("all targets invalid at %lldms; re-resolving (window %dms)",
                           static_cast<long long>(elapsed), kReResolveWindowMs);
            const int64_t deadline = elapsed + kReResolveWindowMs;
            while (ntarget == 0 && elapsed < deadline) {
                SecImage imgs[kMaxImages];
                const int n = CollectSecImages(imgs, kMaxImages);
                if (n != last_seen) {
                    last_seen = n;
                    DWDLM_SEC_LOGI("re-resolve: libsecret images=%d", n);
                }
                AppendTargets(imgs, n, targets, ntarget, kMaxTargets, &reject_logs);
                if (ntarget == 0) {
                    usleep(static_cast<useconds_t>(kResolvePollMs) * 1000);
                    elapsed = NowMs() - t0;
                }
            }
            if (ntarget == 0) {
                DWDLM_SEC_LOGE("no target recovered within %dms; abort (no further write)",
                               kReResolveWindowMs);
                g_state.store(kStateAbort);
                return nullptr;
            }
            DWDLM_SEC_LOGI("re-resolved targets=%d at %lldms", ntarget,
                           static_cast<long long>(elapsed));
        }

        // ④ 兜底：壳可能在 watchdog 起来之后才 dlopen 出新的 libsecret 映像，周期性补扫一次。
        if (elapsed >= next_late_scan) {
            next_late_scan = elapsed + kLateScanEveryMs;
            SecImage imgs[kMaxImages];
            const int n = CollectSecImages(imgs, kMaxImages);
            const int before = ntarget;
            AppendTargets(imgs, n, targets, ntarget, kMaxTargets, &reject_logs);
            ++late_scans;
            if (ntarget > before) {
                DWDLM_SEC_LOGI("late image picked up: targets=%d (scan=%d/%d)", ntarget,
                               before, kMaxTargets);
            }
        }

        usleep(200 * 1000);
        elapsed = NowMs() - t0;
    }

    DWDLM_SEC_LOGI("watchdog finished at %lldms; state=DONE; targets=%d",
                   static_cast<long long>(elapsed), ntarget);
    g_state.store(kStateDone);
    return nullptr;
}

// ── 无壳包启动链追踪（2026-11-08）──
// 无壳包里引擎 runScript(0x1fde78) 从未被调用、jswrapper 0 条。
// 这里在最早时机（JNI_OnLoad → Start）hook nativeInit 与 runScript，确认启动链走到哪。
static int (*g_orig_nativeInit)(JNIEnv *, jclass, jint, jint) = nullptr;
static int (*g_orig_runScript)() = nullptr;

static jint TraceNativeInit(JNIEnv *env, jclass clazz, jint w, jint h) {
    DWDLM_SEC_LOGI("BOOTTRACE nativeInit enter w=%d h=%d", (int)w, (int)h);
    jint r = g_orig_nativeInit ? g_orig_nativeInit(env, clazz, w, h) : 0;
    DWDLM_SEC_LOGI("BOOTTRACE nativeInit return %d", (int)r);
    return r;
}

static int TraceRunScript() {
    DWDLM_SEC_LOGI("BOOTTRACE runScript ENTER");
    int r = g_orig_runScript ? g_orig_runScript() : 1;
    DWDLM_SEC_LOGI("BOOTTRACE runScript return %d", r);
    return r;
}

struct BootBaseFind { void *base; };
static int BootFindCb(struct dl_phdr_info *info, size_t, void *data) {
    if (info->dlpi_name != nullptr && strstr(info->dlpi_name, "libcocos2djs.so") != nullptr) {
        reinterpret_cast<BootBaseFind *>(data)->base = reinterpret_cast<void *>(info->dlpi_addr);
        return 1;
    }
    return 0;
}

// 注意：绝不能用 dlopen 触发引擎加载（会在错误时机初始化引擎，实测 2.6s 即 native crash）。
// 只查已加载的映像；没出现就下次再试。
// 用 /proc/self/maps 找引擎映像起点（dl_iterate_phdr 在本进程里 dlpi_name 不可靠）。
uintptr_t FindCocosBaseByMaps() {
    FILE *fp = fopen("/proc/self/maps", "r");
    if (fp == nullptr) return 0;
    char line[512];
    uintptr_t base = 0;
    while (fgets(line, sizeof(line), fp) != nullptr) {
        if (strstr(line, "libcocos2djs.so") == nullptr) continue;
        unsigned long start = 0, off = 0;
        char perms[8] = {0};
        if (sscanf(line, "%lx-%*lx %7s %lx", &start, perms, &off) != 3) continue;
        if (off != 0) continue;
        base = static_cast<uintptr_t>(start);
        break;
    }
    fclose(fp);
    return base;
}

// A 方案探测：cocos2d-x 2.4 的 ScriptingCore 是单例，若符号还在动态符号表里，
// 就能直接 getInstance()->start()，不必硬编码 0x1fde78 与 this 指针。
void ProbeScriptingCore() {
    void *gi = dlsym(RTLD_DEFAULT, "_ZN7cocos2d13ScriptingCore11getInstanceEv");
    void *st = dlsym(RTLD_DEFAULT, "_ZN7cocos2d13ScriptingCore5startEv");
    void *gi2 = dlsym(RTLD_DEFAULT, "_ZN7cocos2d6Script13ScriptingCore11getInstanceEv");
    void *st2 = dlsym(RTLD_DEFAULT, "_ZN7cocos2d6Script13ScriptingCore5startEv");
    DWDLM_SEC_LOGI("PROBE getInstance=%p start=%p alt=%p/%p", gi, st, gi2, st2);
    void *g = gi ? gi : gi2;
    if (g == nullptr) return;
    typedef void *(*GetInst)();
    void *inst = reinterpret_cast<GetInst>(g)();
    DWDLM_SEC_LOGI("PROBE instance=%p", inst);
    if (inst == nullptr) return;
    const uintptr_t *vt = *reinterpret_cast<const uintptr_t **>(inst);
    if (vt == nullptr) return;
    DWDLM_SEC_LOGI("PROBE vtable=%p [0]=%p [1]=%p [2]=%p", vt,
                   reinterpret_cast<void *>(vt[0]), reinterpret_cast<void *>(vt[1]),
                   reinterpret_cast<void *>(vt[2]));
}

// C 方案前置：进程内读壳的 .data 常量与 .bss 三元组。
// 读法 = 「进程内 + ImgReadable 保护」：不走跨进程 /proc/<pid>/mem（Enforcing 下不可靠），
// 也不 mprotect 绕过页权限（与本文 D1–D3 契约一致）。先按映像可读区间判定，不在区间内就
// 跳过并打印 addr/segs 事实 —— 诊断本身绝不允许把进程打死。
void DumpSecretsOnce() {
    static int64_t last_ms = 0;
    const int64_t now = NowMs();
    if (now - last_ms < 400) { return; }
    last_ms = now;

    SecImage imgs[kMaxTargets];
    const int n = CollectSecImages(imgs, kMaxTargets);
    if (n <= 0) {
        DWDLM_SEC_LOGW("SECRETS no libsecret image yet");
        return;
    }
    static const char kHex[] = "0123456789abcdef";
    const struct { uintptr_t off; int len; const char *name; } items[5] = {
        {0x6e0deu, 32, "const32"}, {0x6e0feu, 16, "const16"},
        {0x786b0u, 16, "flag"}, {0x787a0u, 32, "IV"}, {0x788a0u, 32, "KEY"}};
    static char last_hex[kMaxTargets][5][65];

    for (int i = 0; i < n; ++i) {
        const SecImage &img = imgs[i];
        for (int k = 0; k < 5; ++k) {
            const uintptr_t a = img.base + items[k].off;
            const size_t len = static_cast<size_t>(items[k].len);
            if (!ImgReadable(img, a, len)) {
                if (last_hex[i][k][0] != '!') {
                    last_hex[i][k][0] = '!'; last_hex[i][k][1] = 0;
                    DWDLM_SEC_LOGI("SECRETS img%d/%d base=0x%lx %s @0x%lx NOT in readable segs"
                                   " (segs=%d) -> skip", i + 1, n,
                                   static_cast<unsigned long>(img.base), items[k].name,
                                   static_cast<unsigned long>(a), img.seg_count);
                }
                continue;
            }
            char hex[65];
            const uint8_t *pp = reinterpret_cast<const uint8_t *>(a);
            for (size_t t = 0; t < len; ++t) {
                hex[t * 2] = kHex[(pp[t] >> 4) & 0xF];
                hex[t * 2 + 1] = kHex[pp[t] & 0xF];
            }
            hex[len * 2] = 0;
            if (strcmp(last_hex[i][k], hex) == 0) { continue; }
            memcpy(last_hex[i][k], hex, len * 2 + 1);
            DWDLM_SEC_LOGI("SECRETS img%d/%d base=0x%lx %s @0x%lx = %s", i + 1, n,
                           static_cast<unsigned long>(img.base), items[k].name,
                           static_cast<unsigned long>(a), hex);
        }
    }
}

void InstallBootTrace() {
    static std::atomic<int> st{0};
    if (st.load() == 2) return;
    const uintptr_t base = FindCocosBaseByMaps();
    if (base == 0) return;
    void *rs = reinterpret_cast<void *>(base + 0x1fde78u);
    if (DobbyHook(rs, reinterpret_cast<dobby_dummy_func_t>(TraceRunScript),
                  reinterpret_cast<dobby_dummy_func_t *>(&g_orig_runScript)) == 0) {
        DWDLM_SEC_LOGI("BOOTTRACE hooked runScript @%p base=0x%lx", rs, static_cast<unsigned long>(base));
    } else {
        DWDLM_SEC_LOGW("BOOTTRACE hook runScript failed @%p", rs);
    }
    void *ni = dlsym(RTLD_DEFAULT, "Java_org_cocos2dx_lib_Cocos2dxRenderer_nativeInit");
    if (ni != nullptr) {
        if (DobbyHook(ni, reinterpret_cast<dobby_dummy_func_t>(TraceNativeInit),
                      reinterpret_cast<dobby_dummy_func_t *>(&g_orig_nativeInit)) == 0)
            DWDLM_SEC_LOGI("BOOTTRACE hooked nativeInit @%p", ni);
        else DWDLM_SEC_LOGW("BOOTTRACE hook nativeInit failed");
    } else {
        DWDLM_SEC_LOGW("BOOTTRACE nativeInit symbol missing");
    }
    st.store(2);
    ProbeScriptingCore();
}

}  // namespace

// 独立轮询线程：引擎由 Java 侧 loadLibrary 加载，时机晚于 so 的 JNI_OnLoad，
// 而 WatchThread 会因找不到 libsecret 在 4s 内 abort ⇒ 必须单独起一个线程持续等引擎出现。
// 方案 A：1.0.25 壳不创建真实 Application ⇒ PluginHelper.app 恒为 null ⇒ nativeInit 里
// CCAppInfo.getBingoAppId → PluginHelper.getAppInfo() 抛 NPE，进程 ~8 s 后静默 exit(0)。
// 这里在 native 侧用 JNI 反射把当前 Application 补进去：类必须经 Application 的 ClassLoader 加载
// （attach 上来的 native 线程 FindClass 只看得到 boot classpath，看不到应用类）。
bool PatchPluginAppOnce(JNIEnv *env) {
    env->PushLocalFrame(16);
    bool handled = false;
    do {
        jclass clsAT = env->FindClass("android/app/ActivityThread");
        if (clsAT == nullptr) { env->ExceptionClear(); break; }
        jmethodID midCur = env->GetStaticMethodID(clsAT, "currentApplication",
                                                 "()Landroid/app/Application;");
        jobject app = midCur ? env->CallStaticObjectMethod(clsAT, midCur) : nullptr;
        if (env->ExceptionCheck()) { env->ExceptionClear(); break; }
        if (app == nullptr) break;

        jclass clsApp = env->GetObjectClass(app);
        jmethodID midCL = clsApp ? env->GetMethodID(clsApp, "getClassLoader",
                                                    "()Ljava/lang/ClassLoader;") : nullptr;
        jobject cl = midCL ? env->CallObjectMethod(app, midCL) : nullptr;
        if (env->ExceptionCheck() || cl == nullptr) { env->ExceptionClear(); break; }

        jclass clsCL = env->FindClass("java/lang/ClassLoader");
        jmethodID midLoad = clsCL ? env->GetMethodID(clsCL, "loadClass",
                                                     "(Ljava/lang/String;)Ljava/lang/Class;") : nullptr;
        jstring jname = env->NewStringUTF("com.bingo.sdklibs.utils.PluginHelper");
        jclass clsPH = (midLoad && jname)
                           ? static_cast<jclass>(env->CallObjectMethod(cl, midLoad, jname)) : nullptr;
        if (env->ExceptionCheck() || clsPH == nullptr) { env->ExceptionClear(); break; }

        jfieldID fid = env->GetStaticFieldID(clsPH, "app", "Landroid/app/Application;");
        if (fid == nullptr) { env->ExceptionClear(); break; }
        jobject cur = env->GetStaticObjectField(clsPH, fid);
        if (cur != nullptr) {
            DWDLM_SEC_LOGI("PLUGIN app already set, nothing to do");
            handled = true;
            break;
        }
        jmethodID midInit = env->GetStaticMethodID(clsPH, "initWithApplication",
                                                   "(Landroid/app/Application;)V");
        if (midInit == nullptr) { env->ExceptionClear(); break; }
        env->CallStaticVoidMethod(clsPH, midInit, app);
        if (env->ExceptionCheck()) {
            env->ExceptionClear();
            break;
        }
        DWDLM_SEC_LOGI("PLUGIN PluginHelper.app filled (native reflection)");
        handled = true;
    } while (false);
    env->PopLocalFrame(nullptr);
    return handled;
}

void *PatchAppThread(void *) {
    JavaVM *vm = g_dwdlm_vm;
    if (vm == nullptr) return nullptr;
    JNIEnv *env = nullptr;
    if (vm->AttachCurrentThread(&env, nullptr) != JNI_OK) return nullptr;
    for (int i = 0; i < 600; ++i) {
        if (PatchPluginAppOnce(env)) break;
        usleep(100 * 1000);
    }
    vm->DetachCurrentThread();
    return nullptr;
}

void *BootTraceThread(void *) {
    for (int i = 0; i < 900; ++i) {
        InstallBootTrace();
        if (g_orig_runScript != nullptr) break;
        if ((i % 20) == 0) DWDLM_SEC_LOGI("BOOTTRACE wait engine base=0x%lx i=%d",
                                     static_cast<unsigned long>(FindCocosBaseByMaps()), i);
        usleep(100 * 1000);
    }
    return nullptr;
}

void DwdlmSecShell_Start() {
    InstallBootTrace();
    pthread_t btid;
    const int bret = pthread_create(&btid, nullptr, BootTraceThread, nullptr);
    DWDLM_SEC_LOGI("BOOTTRACE spawn ret=%d base=0x%lx", bret,
                   static_cast<unsigned long>(FindCocosBaseByMaps()));
    if (bret == 0) pthread_detach(btid);
    static std::atomic<bool> started{false};
    if (started.exchange(true)) return;
    pthread_t ptid;
    if (pthread_create(&ptid, nullptr, PatchAppThread, nullptr) == 0) {
        pthread_detach(ptid);
        DWDLM_SEC_LOGI("PLUGIN patch thread started");
    }
    pthread_t tid;
    if (pthread_create(&tid, nullptr, WatchThread, nullptr) != 0) {
        g_state.store(kStateAbort);
        DWDLM_SEC_LOGE("pthread_create failed; watchdog not started");
        return;
    }
    pthread_detach(tid);
    DWDLM_SEC_LOGI("watchdog requested (state=RESOLVING)");
}

int DwdlmSecShell_State() {
    return g_state.load();
}

extern "C" JNIEXPORT void JNICALL
Java_com_starry_magic_dwdlm_DwdlmAppComponentFactory_nativeEnvStart(JNIEnv *env, jclass) {
    if (env != nullptr) env->GetJavaVM(&g_dwdlm_vm);
    DwdlmSecShell_Start();
}

extern "C" JNIEXPORT jint JNICALL
Java_com_starry_magic_dwdlm_DwdlmAppComponentFactory_nativeEnvState(JNIEnv *, jclass) {
    return DwdlmSecShell_State();
}
