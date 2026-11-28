// ============================================================================
//  幻想九霄（hxjx）游戏模块 — 无敌 = HotFix.dll 运行期内存 IL 字节补丁
//  唯一被真机验证有效的实现（勿再引入任何 native hook）。
//
//  · Unity IL2CPP + HybridCLR 热更：HotFix.dll 以原始字节载入 rw 匿名堆段，运行期
//    不回读文件 ⇒ 改内存里的 IL 即生效。改 2 处（A + A' 同时写）才得无敌：
//      A  0x48995 「11 0D」→「16 00」  AttackSystemLogic.EnemyAttackHit：hp -= dmg ⇒ 0
//      A' 0x1971A0「31 09」→「00 00」  UnitUpdate._changeHpMax：if (hp > newMax) hp = newMax
//    真机事实链：A + A' 有效；只留 A 仍掉血 ⇒ 真正生效的是 A'，A 一并保留；A' 即无条件 hp = newMax。
//  · 不用固定地址：32 字节唯一签名扫，命中后只改「签名窗口 +16」起的 2 字节；
//    签名窗口本身含被改字节 ⇒ 已打过的拷贝不会二次命中。
//
//  安全边界（删任一条都会复现历史闪退）：只扫 rw / 非执行 / 6MB~512MB 段、跳过自身 so；
//    每命中先 PE 校验（MZ + e_lfanew + PE 头魔数 + A' 锚点同 base），不过的**原样跳过、
//    绝不写**（盲写会把同签名副本写坏 ⇒ 进关卡崩溃）；扫描/读取/写入全在 sigsetjmp
//    保护区内（坏页只作废本轮）；mprotect 改 RWX 写、写后恢复 RW + 逐点读回校验；
//    单轮有字节预算，超了下轮续扫；开关 OFF ⇒ 回填原始字节（零改写）；线程/签名失败只降级。
//  日志（adb logcat | grep -i hxjx）：dll_sig / dll patch / dll revert / feat inv
// ============================================================================

#include "HxjxMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>

#include <mutex>      // 保护区安装互斥
#include <setjmp.h>   // sigsetjmp 保护区
#include <signal.h>   // 致命信号兜底

#include <pthread.h>     // 补丁线程
#include <string.h>      // memmem
#include <sys/mman.h>    // mprotect
#include <unistd.h>      // usleep
#include <dlfcn.h>       // dlopen/dlsym/dladdr（il2cpp API 解析）
#include <time.h>        // clock_gettime
#include "Dobby/dobby.h" // 抓点补全：解释器入口 + 2 处 AOT hook

#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"

// 生成器把 [Hxjx] 替换为本游戏标签（如 [jfqs]），日志过滤用 adb logcat -s Mod_Menu
#define TPL_LOG     "[Hxjx]"
#define HXJX_DLL_LOG "[hxjx]"

// 注册表模式不依赖 GameConfig.h（legacy 配置中心）。
#define TPL_TARGET_LIB sec_lib_il2cpp()

// ============================================================================
//  ① 开关
//     g_libReady = 「库已映射」≠「已打补丁」；全开关关闭时应保持零改写。
// ============================================================================
static std::atomic<bool> g_libReady{false};   // 库已映射
static std::atomic<bool> g_inv{false};        // 菜单【无敌】开关
static std::atomic<int>  g_invLogged{-1};     // 上次已打印的开关值（-1 = 未打印）

// 判读锚点：开关状态只在「变化」时打一行（Tick 频率高，绝不能每帧刷屏）
static void LogInvIfChanged(bool inv) {
    const int v = inv ? 1 : 0;
    if (g_invLogged.load() == v) return;
    g_invLogged.store(v);
    LOGI(TPL_LOG " feat inv=%d", v);
}

// ============================================================================
//  ② 致命信号兜底（sigsetjmp 保护区 + sigaltstack 备用栈）
//  扫描/写入会碰进程内其它段，坏页在 Android 上是 SIGSEGV/SIGBUS：用保护区把整轮
//  扫描包起来，炸了只作废本轮（失败降级不崩）。备用栈与故障态必须 thread_local。
// ============================================================================
static std::mutex g_guardMutex;     // 只保护一次性「安装信号处理器」动作

static thread_local sigjmp_buf              t_faultJmp;
static thread_local volatile sig_atomic_t   t_inGuard    = 0;   // 是否正在保护区内
static thread_local volatile sig_atomic_t   t_faultDepth = 0;   // 保护区连炸计数

// 备用信号栈（SIGSEGV 时原栈可能已不可用）；64KB/线程，per-thread 必须 thread_local
static thread_local char t_altStack[65536];
static thread_local bool t_altStackSet = false;

// 被链式接管的旧处理器
static struct sigaction g_oldSegv;
static struct sigaction g_oldBus;
static struct sigaction g_oldIll;
static std::atomic<bool> g_guardArmed{false};

static void TplFaultHandler(int sig, siginfo_t* /*si*/, void* /*uc*/) {
    if (t_inGuard) {
        // 保护区内连炸：说明保护区本身不可靠，交回系统默认行为（宁可崩，不要死循环）
        if (++t_faultDepth > 4) {
            signal(sig, SIG_DFL);
            raise(sig);
            return;
        }
        siglongjmp(t_faultJmp, 1);   // 永不返回
    }

    // 保护区外：链回旧处理器，再走默认
    const struct sigaction* old = (sig == SIGSEGV) ? &g_oldSegv
                               : (sig == SIGBUS)  ? &g_oldBus
                                                  : &g_oldIll;
    if (old->sa_sigaction && old->sa_sigaction != TplFaultHandler) {
        old->sa_sigaction(sig, nullptr, nullptr);
        return;
    }
    signal(sig, SIG_DFL);
    raise(sig);
}

// 为本线程挂上备用信号栈（懒执行，失败降级为无备用栈）
static void ArmFaultGuardForThread() {
    if (t_altStackSet) return;
    t_altStackSet = true;
    stack_t ss;
    std::memset(&ss, 0, sizeof(ss));
    ss.ss_sp    = t_altStack;
    ss.ss_size  = sizeof(t_altStack);
    ss.ss_flags = 0;
    sigaltstack(&ss, nullptr);   // 失败也不影响主路径：只是没有备用栈
}

// 一次性安装进程级处理器
static void InstallFaultGuard() {
    if (g_guardArmed.load()) return;
    std::lock_guard<std::mutex> lk(g_guardMutex);
    if (g_guardArmed.load()) return;

    struct sigaction sa;
    std::memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = TplFaultHandler;
    sa.sa_flags     = SA_SIGINFO | SA_ONSTACK | SA_NODEFER;
    sigemptyset(&sa.sa_mask);

    sigaction(SIGSEGV, &sa, &g_oldSegv);
    sigaction(SIGBUS,  &sa, &g_oldBus);
    sigaction(SIGILL,  &sa, &g_oldIll);

    g_guardArmed.store(true);
}

// 保护区包装器：整轮扫描在区内跑，炸了返回 false（本轮作废），不崩
static bool RunGuarded(void (*fn)(void*), void* arg) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        fn(arg);
        t_inGuard = 0;
        return true;
    }
    t_inGuard = 0;
    return false;
}

// ============================================================================
//  ③ HotFix.dll 内存 IL 补丁（本作唯一功能，0 个 Dobby hook）
// ============================================================================

// 签名原文（hex 单一事实源；运行期解码，避免手抄 32 字节出错）。字符串混淆规则同 SecStrings。
static const char* HexSigA() {
    static const std::string s = OBFUSCATE(
        "0e0816287f470006087ccb080004254c110d6a5955110d16fe0311136039cb00");
    return s.c_str();
}
// A' 签名：既作写入点，也作 PE 校验锚点（base+0x197190 命中 A' = 真镜像证据）。
static constexpr bool kEnableSigAp = true;    // A' 写入总开关
static const char* HexSigAp() {
    static const std::string s = OBFUSCATE(
        "066a7dcc08000411047bcb080004066a31091104066a7dcb0800047ecb260004");
    return s.c_str();
}

enum : int {
    kSigLen        = 32,                 // 签名长度
    kSigPatchOff   = 16,                 // 补丁点 = 签名窗口起点 + 16
    kMaxPatchRefs  = 16,                 // 回填记录上限（真镜像通常 1 份，留 16 冗余）
    kMinSegBytes   = 6000000,            // < 6,000,000 的段不扫（HotFix.dll 6,694,400 B 按
                                         // 4KB 取整 = 6,697,984；门槛设 6.7MB 会整段漏掉）
    kMaxSegBytes   = 512 * 1024 * 1024,  // 真机见过两个 384MB rw 段，门槛放 512MB
    kScanBudgetBytes = 512 * 1024 * 1024, // 单轮累计扫描上限：超了就停本轮（每轮 >=1.5s），
                                         // 下轮从被停下的段续扫（游标 g_scanStart）
};

// HotFix.dll 内文件偏移（PE 校验唯一基准）：A 签名窗口起点 = kPatchOffA - kSigPatchOff = 0x48985
static constexpr uintptr_t kPatchOffA     = 0x48995;   // A 补丁点（11 0D → 16 00）
static constexpr uintptr_t kPatchOffApSig = 0x197190;  // A' 签名窗口起点（同 base 判定用）
static constexpr uintptr_t kPatchOffAp    = 0x1971A0;  // A' 补丁点 = 窗口起点 + kSigPatchOff

static unsigned char g_sigA[kSigLen];
static unsigned char g_sigAp[kSigLen];
// A' 已写入后的签名形态（补丁点 2 字节变 00 00）：用于识别「这个镜像我早前已经打过 A'」，
// 否则写完 A' 后签名本身失效，重扫会把整份镜像判成不匹配（连 A 也不写）。
static unsigned char g_sigApPatched[kSigLen];
static std::atomic<bool> g_sigReady{false};

// 原始/目标字节：A = 11 0D → 16 00；A' = 31 09 → 00 00
static const unsigned char kOrigA[2]   = {0x11, 0x0d};
static const unsigned char kPatchA[2]  = {0x16, 0x00};
static const unsigned char kOrigAp[2]  = {0x31, 0x09};
static const unsigned char kPatchAp[2] = {0x00, 0x00};

static bool HexToBytes(const char* hex, unsigned char* out, int n) {
    if (!hex) return false;
    if ((int)std::strlen(hex) != n * 2) return false;
    for (int i = 0; i < n; ++i) {
        auto nib = [](char c) -> int {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            if (c >= 'A' && c <= 'F') return c - 'A' + 10;
            return -1;
        };
        const int hi = nib(hex[i * 2]);
        const int lo = nib(hex[i * 2 + 1]);
        if (hi < 0 || lo < 0) return false;
        out[i] = static_cast<unsigned char>((hi << 4) | lo);
    }
    return true;
}

// ---- 3.0 保护区内的纯读取（PE 校验用；读到坏页只作废本次校验，不崩） ----
static bool ReadBytesGuarded(const void* src, void* dst, size_t n) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        std::memcpy(dst, src, n);
        t_inGuard = 0;
        return true;
    }
    t_inGuard = 0;
    return false;
}

// ---- 3.0b PE 镜像校验（不过校验的命中**一律不写**）：① base 处 MZ ② base+0x3c 的
// e_lfanew ∈ [0x40,0x400] ③ base+e_lfanew 处 PE 头魔数 0x00004550 ④ base+0x197190 处
// 命中 A' 签名（或「已打 A' 的形态」）。四项全过才算真 HotFix.dll 镜像。
static bool PeValidate(uintptr_t patchAddr, uintptr_t segStart, uintptr_t segEnd,
                       uintptr_t* baseOut) {
    *baseOut = 0;
    if (patchAddr < kPatchOffA) return false;
    const uintptr_t base = patchAddr - kPatchOffA;

    if (base < segStart)                          return false;
    if (base + kPatchOffApSig + kSigLen > segEnd)  return false;

    unsigned char mz[2] = {0, 0};
    if (!ReadBytesGuarded(reinterpret_cast<const void*>(base), mz, 2)) return false;
    if (mz[0] != 'M' || mz[1] != 'Z') return false;

    uint32_t lfanew = 0;
    if (!ReadBytesGuarded(reinterpret_cast<const void*>(base + 0x3c), &lfanew, 4)) return false;
    if (lfanew < 0x40 || lfanew > 0x400) return false;

    uint32_t pesig = 0;
    if (!ReadBytesGuarded(reinterpret_cast<const void*>(base + lfanew), &pesig, 4)) return false;
    if (pesig != 0x00004550u) return false;

    unsigned char ap[kSigLen];
    if (!ReadBytesGuarded(reinterpret_cast<const void*>(base + kPatchOffApSig), ap, kSigLen))
        return false;
    if (std::memcmp(ap, g_sigAp, kSigLen) != 0 &&
        std::memcmp(ap, g_sigApPatched, kSigLen) != 0) return false;

    *baseOut = base;
    return true;
}

// ---- 3.1 段描述 ----
struct SegDesc {
    uintptr_t start;
    uintptr_t end;
    uintptr_t fileOff;   // maps 第 4 列：该段起始对应的文件偏移
    char      perm[8];
    char      path[200];
};

// 解析 /proc/self/maps 一行；路径取该行剩余字段（含空格），没有则 <anon>
static bool ParseMapsLine(const char* line, SegDesc* d) {
    char dev[16]  = {0};
    char ino[24]  = {0};
    char rest[200] = {0};
    unsigned long s = 0, e = 0, fo = 0;
    char perm[8] = {0};
    const int n = std::sscanf(line, "%lx-%lx %7s %lx %15s %23s %199[^\n]",
                              &s, &e, perm, &fo, dev, ino, rest);
    if (n < 4) return false;
    d->start   = static_cast<uintptr_t>(s);
    d->end     = static_cast<uintptr_t>(e);
    d->fileOff = static_cast<uintptr_t>(fo);
    std::snprintf(d->perm, sizeof(d->perm), "%s", perm);
    const char* q = rest;
    while (*q == ' ' || *q == '\t') ++q;
    std::snprintf(d->path, sizeof(d->path), "%s", *q ? q : "<anon>");
    return true;
}

// ---- 3.2 补丁点记录（开关关闭时回填用；只有 rw 段，无需权限标记） ----
struct PatchRef {
    uintptr_t addr;   // 补丁点地址（A = 命中 + 16；A' = base + 0x1971A0）
    int       kind;   // 0 = A（11 0D→16 00），1 = A'（31 09→00 00）
};
static std::mutex g_patchMutex;
static PatchRef   g_refs[kMaxPatchRefs];
static std::atomic<int> g_refCount{0};

static void AddPatchRef(uintptr_t addr, int kind) {
    std::lock_guard<std::mutex> lk(g_patchMutex);
    const int n = g_refCount.load();
    for (int i = 0; i < n; ++i) {
        if (g_refs[i].addr == addr) return;   // 幂等
    }
    if (n < kMaxPatchRefs) {
        g_refs[n].addr = addr;
        g_refs[n].kind = kind;
        g_refCount.store(n + 1);
    }
}

// ---- 3.3 页级写入：rw 匿名堆段 mprotect RWX → 写 → icache flush → 恢复 RW ----
static bool WritePageBytes(uintptr_t addr, const unsigned char* bytes, size_t n) {
    const uintptr_t page0 = addr & ~static_cast<uintptr_t>(0xFFF);
    const uintptr_t page1 = (addr + n + 0xFFF) & ~static_cast<uintptr_t>(0xFFF);
    const size_t    len   = static_cast<size_t>(page1 - page0);

    if (mprotect(reinterpret_cast<void*>(page0), len,
                 PROT_READ | PROT_WRITE | PROT_EXEC) != 0) {
        return false;
    }
    std::memcpy(reinterpret_cast<void*>(addr), bytes, n);
    __builtin___clear_cache(reinterpret_cast<char*>(addr),
                            reinterpret_cast<char*>(addr + n));
    mprotect(reinterpret_cast<void*>(page0), len, PROT_READ | PROT_WRITE);   // 恢复
    return true;
}

// ---- 3.4 段内扫描：A 命中 → PE 校验 → A' 同 base 判定 → 双写 + 读回验证 ----
struct PatchStat {
    int accepted;    // A 过 PE 校验的真镜像副本数（= 汇总日志里的 A）
    int written;     // A 真写入且读回一致
    int rejected;    // PE 校验 / A' 同 base 判定不过（原样跳过，绝不写）
    int apAccepted;  // A' 同 base 命中数（= 汇总日志里的 Ap）
    int apWritten;   // A' 真写入且读回一致
};

// A' 窗口是否已是「打过之后」的形态（写 A' 会让 A' 签名本身失效，必须单独认出来）
static bool ApAlreadyPatched(uintptr_t baseA) {
    unsigned char buf[kSigLen];
    if (!ReadBytesGuarded(reinterpret_cast<const void*>(baseA + kPatchOffApSig), buf, kSigLen)) {
        return false;
    }
    return std::memcmp(buf, g_sigApPatched, kSigLen) == 0;
}

static void ScanSeg(const SegDesc& seg, PatchStat* st) {
    const unsigned char* cur = reinterpret_cast<const unsigned char*>(seg.start);
    const unsigned char* end = cur + static_cast<size_t>(seg.end - seg.start);
    const unsigned char* curAp = cur;   // A' 独立扫描游标（与 A 交叉验证同一 base）

    while (cur + kSigLen <= end) {
        const unsigned char* p = static_cast<const unsigned char*>(
            memmem(cur, static_cast<size_t>(end - cur), g_sigA, kSigLen));
        if (!p) break;

        const uintptr_t at = reinterpret_cast<uintptr_t>(p) + kSigPatchOff;

        uintptr_t base = 0;
        if (!PeValidate(at, seg.start, seg.end, &base)) {
            ++st->rejected;
            cur = p + kSigLen;         // 原样跳过，绝不写
            continue;
        }

        ++st->accepted;

        // ---- A' 同 base 判定：要求 baseAp(=addr_ap-0x197190) == baseA(=addr_a-0x48995)，
        //   PE 头已校验过；不满足 ⇒ rejected，A 也不写。 ----
        uintptr_t atAp = 0;
        if (kEnableSigAp) {
            bool matched = false;
            while (curAp + kSigLen <= end) {
                const unsigned char* q = static_cast<const unsigned char*>(
                    memmem(curAp, static_cast<size_t>(end - curAp), g_sigAp, kSigLen));
                if (!q) break;
                const uintptr_t baseAp = reinterpret_cast<uintptr_t>(q) - kPatchOffApSig;
                if (baseAp == base) {                 // 同一 base：真镜像的 A' 点
                    atAp = base + kPatchOffAp;
                    matched = true;
                    curAp = q + kSigLen;
                    break;
                }
                if (baseAp < base) { curAp = q + kSigLen; continue; }   // 更早的副本，继续找
                break;                                                  // 越过目标 base ⇒ 无
            }
            if (matched) {
                ++st->apAccepted;
            } else if (ApAlreadyPatched(base)) {
                atAp = base + kPatchOffAp;            // 早前已写过 A'，签名已变形态
            } else {
                ++st->rejected;
                cur = p + kSigLen;                    // 不满足同 base ⇒ 整份镜像不写
                continue;
            }
        }

        const unsigned char before0 = p[kSigPatchOff];
        const unsigned char before1 = p[kSigPatchOff + 1];

        if (before0 == kPatchA[0] && before1 == kPatchA[1]) {
            AddPatchRef(at, 0);                // 早前已打过
        } else if (WritePageBytes(at, kPatchA, 2)) {
            const volatile unsigned char* q =
                reinterpret_cast<const volatile unsigned char*>(at);
            const int ok = (q[0] == kPatchA[0] && q[1] == kPatchA[1]) ? 1 : 0;
            // 判读锚点：写前/写后字节，ok=1 才算真写上
            LOGI(TPL_LOG " dll patch addr=%p base=%p before=%02x%02x after=%02x%02x ok=%d",
                 reinterpret_cast<void*>(at), reinterpret_cast<void*>(base),
                 static_cast<unsigned>(before0), static_cast<unsigned>(before1),
                 static_cast<unsigned>(q[0]), static_cast<unsigned>(q[1]), ok);
            AddPatchRef(at, 0);
            if (ok) ++st->written;
        }

        // ---- A' 写入（base 同上；只认 31 09 → 00 00，字节不符不盲写） ----
        if (kEnableSigAp && atAp) {
            const volatile unsigned char* rp =
                reinterpret_cast<const volatile unsigned char*>(atAp);
            const unsigned char b0 = rp[0];
            const unsigned char b1 = rp[1];
            if (b0 == kPatchAp[0] && b1 == kPatchAp[1]) {
                AddPatchRef(atAp, 1);          // 已是目标字节
            } else if (b0 == kOrigAp[0] && b1 == kOrigAp[1] &&
                       WritePageBytes(atAp, kPatchAp, 2)) {
                const int ok = (rp[0] == kPatchAp[0] && rp[1] == kPatchAp[1]) ? 1 : 0;
                LOGI(TPL_LOG " dll patch ap addr=%p base=%p before=%02x%02x after=%02x%02x ok=%d",
                     reinterpret_cast<void*>(atAp), reinterpret_cast<void*>(base),
                     static_cast<unsigned>(b0), static_cast<unsigned>(b1),
                     static_cast<unsigned>(rp[0]), static_cast<unsigned>(rp[1]), ok);
                AddPatchRef(atAp, 1);
                if (ok) ++st->apWritten;
            }
            // 字节既非原值也非目标值 ⇒ 不写（不盲改）
        }
        cur = p + kSigLen;   // 继续找同一段的其它拷贝
    }
}

// ---- 3.5 /proc/self/maps 枚举（**只挑 rw 段**；r-- 与 r-xp 一律不动） ----
struct ScanJob {
    int  accepted;
    int  written;
    int  rejected;
    int  apAccepted;
    int  apWritten;
    bool budgetHit;     // 本轮被 512MB 预算提前结束（调用方据此把轮间隔抬到 >=1.5s）
};

// 只 rw、DLL 装得下、跳过自己；这条过滤直接决定 SIGBUS 概率
static bool SegSkipped(const SegDesc& d, size_t len) {
    if (!std::strchr(d.perm, 'r')) return true;   // 不可读
    if (!std::strchr(d.perm, 'w')) return true;   // ★ 只扫 rw
    if (std::strchr(d.perm, 'x'))  return true;   // 代码段禁改
    if (len < static_cast<size_t>(kMinSegBytes)) return true;   // 尺寸门槛
    if (len > static_cast<size_t>(kMaxSegBytes)) return true;
    if (std::strstr(d.path, "libstarry_magic.so")) return true;  // 不扫自己
    return false;
}

// 预算游标：上轮被预算打断时停在第几个可扫段（纯序号，不记地址），否则尾部段永远轮不到。
static std::atomic<uint32_t> g_scanStart{0};

static void ScanMaps(ScanJob* job) {
    FILE* fp = std::fopen("/proc/self/maps", "r");
    if (!fp) return;

    char line[512];
    int lines = 0;
    size_t   budgetBytes = 0;      // 本轮累计扫描字节（含段长）达到 kScanBudgetBytes 即停
    uint32_t eligible    = 0;      // 本行是第几个通过过滤的段
    bool     budgetHit   = false;
    const uint32_t startAt = g_scanStart.load(std::memory_order_relaxed);
    while (std::fgets(line, sizeof(line), fp) && lines < 8192) {
        ++lines;
        SegDesc d;
        std::memset(&d, 0, sizeof(d));
        if (!ParseMapsLine(line, &d)) continue;
        if (d.end <= d.start) continue;
        const size_t len = static_cast<size_t>(d.end - d.start);
        if (SegSkipped(d, len)) continue;

        const uint32_t myIdx = eligible++;
        if (myIdx < startAt) continue;                       // 上轮预算用尽点之前：本轮先跳过
        if (budgetBytes >= static_cast<size_t>(kScanBudgetBytes)) {
            if (!budgetHit) {
                budgetHit = true;
                g_scanStart.store(myIdx, std::memory_order_relaxed);   // 下轮从这里续
            }
            continue;
        }
        budgetBytes += len;

        PatchStat st = {};
        ScanSeg(d, &st);
        job->accepted   += st.accepted;
        job->written    += st.written + st.apWritten;   // patched = A + A' 真写入点数
        job->rejected   += st.rejected;
        job->apAccepted += st.apAccepted;
    }
    std::fclose(fp);
    // 本轮没被预算打断 ⇒ 可扫范围全过一遍，下轮从头再扫（不记地址）
    if (!budgetHit) g_scanStart.store(0, std::memory_order_relaxed);
    job->budgetHit = budgetHit;
}

static void ScanJobRun(void* arg) { ScanMaps(static_cast<ScanJob*>(arg)); }

// ---- 3.6 回填（开关关闭 = 恢复零改写） ----
// A（kind=0）回填 0x11,0x0d；A'（kind=1）回填 0x31,0x09。
// 每点写完**逐点读回**确认字节等于原始值，verified 即确认数。
static int RevertAll(int* verifiedOut) {
    std::lock_guard<std::mutex> lk(g_patchMutex);
    const int n = g_refCount.load();
    int ok = 0, verified = 0;
    for (int i = 0; i < n; ++i) {
        const bool isAp = (g_refs[i].kind == 1);
        const uintptr_t addr = g_refs[i].addr;
        const unsigned char* orig = isAp ? kOrigAp : kOrigA;
        if (!WritePageBytes(addr, orig, 2)) continue;
        ++ok;
        const volatile unsigned char* p =
            reinterpret_cast<const volatile unsigned char*>(addr);
        if (p[0] == orig[0] && p[1] == orig[1]) ++verified;
    }
    g_refCount.store(0);
    if (verifiedOut) *verifiedOut = verified;
    return ok;
}

// ============================================================================
//  ③b 抓点补全：无敌四点 —— 解释器入口（① ②）+ AOT 表现层（③ ④）
//
//  抓点依据（斌哥包 §5/§6/§7，语义逐条对应，不增不减）：
//    ① AttackSystemLogic.EnemyAttackHit    实参块 +8（attack）= 0
//    ② UnitUpdate.AttribChange*（≥3 参）   实参块 +0x10 为负 → 0
//    ③ HealthViewComponent.get_ratio       hp := maxHp 后再走原逻辑（血条恒满）
//    ④ HUDBloodSystem.Execute              对已登记玩家血量巡检回填（防回退）
//  配套（抓点第 2 组）：HealthComponent.Create 在 CreatePlayer 窗口内、
//    healthUnit==0 时登记玩家血量指针（+0=currentHealth，+8=maxHealth）。
//
//  入口说明：斌哥的「扫 opcode 找解释器入口 + 8 字节前缀 strcmp」在本包必然失效
//  （文档 §11：扫到栈帧簿记函数 / 前缀必不等 / AOT 全 RVA 非 0）。这里保留抓点语义，
//  入口改用**解释器真实派发函数**（0x3E30454，逐方法 stub 都汇入它），过滤用
//  MethodInfo 的完整类指针 + 方法名，不做前缀猜测。
// ============================================================================

typedef void*       (*FnDomainGet)();
typedef void**      (*FnDomainGetAssemblies)(void*, size_t*);
typedef void*       (*FnAssemblyGetImage)(void*);
typedef const char* (*FnImageGetName)(void*);
typedef void*       (*FnClassFromName)(void*, const char*, const char*);
typedef uint32_t    (*FnMethodParamCount)(void*);
typedef void*       (*FnThreadAttach)(void*);
typedef int         (*FnIsVmThread)();

typedef void  (*FnInterpExec)(void* method, void* args, void* ret);
typedef float (*FnGetRatio)(void* self);
struct HxjxJobHandle { uint64_t a; uint64_t b; };
typedef HxjxJobHandle (*FnHudExec)(void* self, uint64_t a, uint64_t b);

// 版本内固定 RVA（1.0.212；与 AOT 两处抓点 RVA 同属「每版重标」项，装前逐条校验首指令）
static constexpr uintptr_t kRvaInterpExec = 0x3E30454;   // HybridCLR 解释器派发入口
static constexpr uintptr_t kRvaGetRatio   = 0x6C9FE10;   // HealthViewComponent.get_ratio
static constexpr uintptr_t kRvaHudExec    = 0x6CA8900;   // HUDBloodSystem.Execute
// 首指令指纹：不符即视为版本不符，拒绝 hook（不盲挂）
static constexpr uint32_t kInsInterpExec = 0xa9ba7bfd;   // stp x29,x30,[sp,#-0x60]!
static constexpr uint32_t kInsGetRatio   = 0xf9400408;   // ldr x8,[x0,#0x8]
static constexpr uint32_t kInsHudExec    = 0xd10103ff;   // sub sp,sp,#0x40

// MethodInfo / Il2CppClass 字段偏移（IL2CPP v31 实测）
static constexpr size_t kMiNameOff    = 0x18;
static constexpr size_t kMiKlassOff   = 0x20;
static constexpr size_t kKlassNameOff = 0x10;

static FnInterpExec       g_origInterpExec = nullptr;
static FnGetRatio         g_origGetRatio   = nullptr;
static FnHudExec          g_origHudExec    = nullptr;
static FnMethodParamCount g_methodParamCnt = nullptr;

static void* g_klassAttack       = nullptr;   // FECS.Game.AttackSystemLogic
static void* g_klassHealth       = nullptr;   // FECS.Game.HealthComponent
static void* g_klassUnitUpdate   = nullptr;   // HotFix.Game.UnitUpdate
static void* g_klassPlayerSystem = nullptr;   // FECS.Game.PlayerSystemLogic

static std::atomic<uint64_t> g_statAtkZero{0};   // ① 命中次数
static std::atomic<uint64_t> g_statAttrClamp{0}; // ② 命中次数
static std::atomic<uint64_t> g_statReg{0};       // 血量登记次数
static std::atomic<uint64_t> g_statRatioFix{0};  // ③ 命中次数
static std::atomic<uint64_t> g_statHudFix{0};    // ④ 回填次数

// 玩家血量组件指针表（Create 时登记；CreatePlayer 时清空重登）
static constexpr int kMaxPlayerHp = 8;
static std::atomic<uintptr_t> g_playerHp[kMaxPlayerHp];
static std::atomic<int> g_playerHpCount{0};

static void ClearPlayerHp() {
    for (int i = 0; i < kMaxPlayerHp; ++i) g_playerHp[i].store(0);
    g_playerHpCount.store(0);
}

static void RegisterPlayerHp(void* ptr) {
    const uintptr_t v = reinterpret_cast<uintptr_t>(ptr);
    if (!v || (v & 7)) return;
    const int n = g_playerHpCount.load(std::memory_order_relaxed);
    for (int i = 0; i < n; ++i) {
        if (g_playerHp[i].load(std::memory_order_relaxed) == v) return;
    }
    if (n < kMaxPlayerHp) {
        g_playerHp[n].store(v);
        g_playerHpCount.store(n + 1);
        g_statReg.fetch_add(1, std::memory_order_relaxed);
    }
}

// CreatePlayer 窗口：该窗口内发生的 HealthComponent.Create(unit==0) 视为玩家血量。
static thread_local uint64_t t_execSeq  = 0;
static thread_local uint64_t t_ctxUntil = 0;

static void HookInterpExec(void* method, void* args, void* ret) {
    if (g_inv.load(std::memory_order_relaxed) && method && args) {
        void* klass = *reinterpret_cast<void**>(reinterpret_cast<char*>(method) + kMiKlassOff);
        const char* name = *reinterpret_cast<const char**>(reinterpret_cast<char*>(method) + kMiNameOff);
        if (klass && name) {
            ++t_execSeq;
            if (klass == g_klassPlayerSystem && std::strncmp(name, "CreatePl", 8) == 0) {
                ClearPlayerHp();                     // 新一局/新角色：旧指针作废
                t_ctxUntil = t_execSeq + 40000;      // 覆盖 CreatePlayer 内部调用深度
            } else if (klass == g_klassAttack && std::strcmp(name, "EnemyAttackHit") == 0) {
                // ① 伤害实参清零（抓点 +8=arg1）
                *reinterpret_cast<int*>(reinterpret_cast<char*>(args) + 8) = 0;
                g_statAtkZero.fetch_add(1, std::memory_order_relaxed);
            } else if (klass == g_klassHealth && std::strcmp(name, "Create") == 0) {
                // 第 2 组：登记玩家血量（+0 = this<HealthComponent*>，+0x10 = healthUnit）
                if (t_execSeq <= t_ctxUntil) {
                    const int unit = *reinterpret_cast<int*>(reinterpret_cast<char*>(args) + 0x10);
                    if (unit == 0) RegisterPlayerHp(*reinterpret_cast<void**>(args));
                }
            } else if (klass == g_klassUnitUpdate && std::strncmp(name, "AttribCh", 8) == 0) {
                // ② 负属性变更归零（仅当该方法确有第 2 个数值参数）
                if (g_methodParamCnt && g_methodParamCnt(method) >= 3) {
                    int* v = reinterpret_cast<int*>(reinterpret_cast<char*>(args) + 0x10);
                    if (*v < 0) {
                        *v = 0;
                        g_statAttrClamp.fetch_add(1, std::memory_order_relaxed);
                    }
                }
            }
        }
    }
    if (g_origInterpExec) g_origInterpExec(method, args, ret);
}

// ③ 血条恒满：把视图组件的 currentHealth 写成 maxHealth 后再走原 getter
static float HookGetRatio(void* self) {
    if (g_inv.load(std::memory_order_relaxed) && self) {
        const int64_t maxHp = *reinterpret_cast<int64_t*>(reinterpret_cast<char*>(self) + 8);
        if (maxHp > 0 && maxHp < static_cast<int64_t>(1e15)) {
            *reinterpret_cast<int64_t*>(self) = maxHp;
            g_statRatioFix.fetch_add(1, std::memory_order_relaxed);
        }
    }
    return g_origGetRatio ? g_origGetRatio(self) : 0.0f;
}

// ④ 已登记玩家血量巡检回填（每帧一次；只在「活着且未满」时回填，不复活）
static HxjxJobHandle HookHudExec(void* self, uint64_t a, uint64_t b) {
    if (g_inv.load(std::memory_order_relaxed)) {
        const int n = g_playerHpCount.load(std::memory_order_relaxed);
        for (int i = 0; i < n; ++i) {
            const uintptr_t p = g_playerHp[i].load(std::memory_order_relaxed);
            if (!p) continue;
            volatile int64_t* cur = reinterpret_cast<volatile int64_t*>(p);
            volatile int64_t* max = reinterpret_cast<volatile int64_t*>(p + 8);
            const int64_t maxHp = *max;
            const int64_t curHp = *cur;
            if (maxHp > 0 && maxHp < static_cast<int64_t>(1e15) && curHp > 0 && curHp < maxHp) {
                *cur = maxHp;
                g_statHudFix.fetch_add(1, std::memory_order_relaxed);
            }
        }
    }
    return g_origHudExec ? g_origHudExec(self, a, b) : HxjxJobHandle{0, 0};
}

// ---- 一次性安装（HotFix 载入后；失败 1s 后重试，绝不阻塞 Tick） ----
static std::atomic<bool>  g_hooksDone{false};
static std::atomic<bool>  g_hookBusy{false};
static std::atomic<int64_t> g_nextHookTryMs{0};

static int64_t NowMsMonotonic() {
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return static_cast<int64_t>(ts.tv_sec) * 1000 + ts.tv_nsec / 1000000;
}

static bool TryInstallIl2CppHooks() {
    void* h = dlopen("libil2cpp.so", RTLD_NOW);
    if (!h) return false;

    auto sym = [h](const char* n) -> void* { return dlsym(h, n); };
    FnDomainGet            domainGet   = reinterpret_cast<FnDomainGet>(sym("il2cpp_domain_get"));
    FnDomainGetAssemblies  getAssems   = reinterpret_cast<FnDomainGetAssemblies>(sym("il2cpp_domain_get_assemblies"));
    FnAssemblyGetImage     asmImage    = reinterpret_cast<FnAssemblyGetImage>(sym("il2cpp_assembly_get_image"));
    FnImageGetName         imgName     = reinterpret_cast<FnImageGetName>(sym("il2cpp_image_get_name"));
    FnClassFromName        classFrom  = reinterpret_cast<FnClassFromName>(sym("il2cpp_class_from_name"));
    FnThreadAttach         threadAtt  = reinterpret_cast<FnThreadAttach>(sym("il2cpp_thread_attach"));
    FnIsVmThread           isVmThread  = reinterpret_cast<FnIsVmThread>(sym("il2cpp_is_vm_thread"));

    if (!domainGet || !getAssems || !asmImage || !imgName || !classFrom) return false;

    Dl_info di;
    std::memset(&di, 0, sizeof(di));
    if (!dladdr(reinterpret_cast<void*>(domainGet), &di) || !di.dli_fbase) return false;
    const uintptr_t base = reinterpret_cast<uintptr_t>(di.dli_fbase);

    void* domain = domainGet();
    if (!domain) return false;
    if (isVmThread && threadAtt && !isVmThread()) threadAtt(domain);

    size_t n = 0;
    void** asms = getAssems(domain, &n);
    void* img = nullptr;
    for (size_t i = 0; asms && i < n; ++i) {
        void* im = asmImage(asms[i]);
        const char* nm = im ? imgName(im) : nullptr;
        if (nm && std::strcmp(nm, "HotFix.dll") == 0) { img = im; break; }
    }
    if (!img) return false;   // 热更还没载入：下轮再来

    g_klassAttack       = classFrom(img, "FECS.Game", "AttackSystemLogic");
    g_klassHealth       = classFrom(img, "FECS.Game", "HealthComponent");
    g_klassUnitUpdate   = classFrom(img, "HotFix.Game", "UnitUpdate");
    g_klassPlayerSystem = classFrom(img, "FECS.Game", "PlayerSystemLogic");
    g_methodParamCnt    = reinterpret_cast<FnMethodParamCount>(sym("il2cpp_method_get_param_count"));
    if (!g_klassAttack || !g_klassHealth || !g_klassUnitUpdate || !g_klassPlayerSystem) {
        LOGW(HXJX_DLL_LOG " hook klass missing (Attack=%p Health=%p UnitUpdate=%p PlayerSys=%p) -> retry",
             g_klassAttack, g_klassHealth, g_klassUnitUpdate, g_klassPlayerSystem);
        return false;
    }

    // ① ② 解释器入口：先校验首指令，版本不符不挂
    void* pExec = reinterpret_cast<void*>(base + kRvaInterpExec);
    if (*reinterpret_cast<uint32_t*>(pExec) != kInsInterpExec) {
        LOGE(HXJX_DLL_LOG " interp entry mismatch: %08x (expect %08x) -> abort hook",
             *reinterpret_cast<uint32_t*>(pExec), kInsInterpExec);
        return false;
    }
    if (DobbyHook(pExec, reinterpret_cast<void*>(&HookInterpExec),
                  reinterpret_cast<void**>(&g_origInterpExec)) != 0) {
        LOGE(HXJX_DLL_LOG " DobbyHook interp failed");
        return false;
    }

    // ③ get_ratio
    void* pRatio = reinterpret_cast<void*>(base + kRvaGetRatio);
    if (*reinterpret_cast<uint32_t*>(pRatio) == kInsGetRatio) {
        if (DobbyHook(pRatio, reinterpret_cast<void*>(&HookGetRatio),
                      reinterpret_cast<void**>(&g_origGetRatio)) != 0) {
            LOGW(HXJX_DLL_LOG " DobbyHook get_ratio failed");
        }
    } else {
        LOGW(HXJX_DLL_LOG " get_ratio mismatch: %08x", *reinterpret_cast<uint32_t*>(pRatio));
    }

    // ④ HUDBloodSystem.Execute
    void* pHud = reinterpret_cast<void*>(base + kRvaHudExec);
    if (*reinterpret_cast<uint32_t*>(pHud) == kInsHudExec) {
        if (DobbyHook(pHud, reinterpret_cast<void*>(&HookHudExec),
                      reinterpret_cast<void**>(&g_origHudExec)) != 0) {
            LOGW(HXJX_DLL_LOG " DobbyHook hud_exec failed");
        }
    } else {
        LOGW(HXJX_DLL_LOG " hud_exec mismatch: %08x", *reinterpret_cast<uint32_t*>(pHud));
    }

    LOGI(HXJX_DLL_LOG " hooks installed base=%p exec=%p ratio=%p hud=%p", reinterpret_cast<void*>(base), pExec, pRatio, pHud);
    return true;
}

static void EnsureIl2CppHooks() {
    if (g_hooksDone.load(std::memory_order_relaxed) || g_hookBusy.load(std::memory_order_relaxed)) return;
    const int64_t now = NowMsMonotonic();
    if (now < g_nextHookTryMs.load(std::memory_order_relaxed)) return;
    bool expect = false;
    if (!g_hookBusy.compare_exchange_strong(expect, true)) return;
    const bool ok = TryInstallIl2CppHooks();
    g_hookBusy.store(false);
    if (ok) g_hooksDone.store(true);
    else    g_nextHookTryMs.store(now + 1000);
}

// ============================================================================
//  ④ 后台补丁线程（懒起 / 常驻；线程内不调任何 il2cpp API ⇒ 不取引擎锁）
//  库未映射 ⇒ 100ms 快轮询；开关 ON ⇒ 扫 rw 段 + PE 校验后写入（上升沿立即扫）；
//  开关 OFF ⇒ 有记录就回填；打齐后 5s 一轮补漏，久未命中降到 1.5s 省电。
// ============================================================================
static std::atomic<bool> g_threadStarted{false};
static std::atomic<bool> g_rescanReq{false};

static void* PatchThreadMain(void* /*arg*/) {
    InstallFaultGuard();   // 保护区依赖进程级 handler，先装

    if (!g_sigReady.load()) {
        // A 与 A' 都用于扫描/写入；A' 签名同时兼 PE 校验锚点（base+0x197190）
        const bool okA  = HexToBytes(HexSigA(),  g_sigA,  kSigLen);
        const bool okAp = HexToBytes(HexSigAp(), g_sigAp, kSigLen);
        if (okA && okAp) {
            std::memcpy(g_sigApPatched, g_sigAp, kSigLen);
            g_sigApPatched[kSigPatchOff]     = kPatchAp[0];   // A' 已写入后的形态
            g_sigApPatched[kSigPatchOff + 1] = kPatchAp[1];
        }
        g_sigReady.store(okA && okAp);
        if (!okA || !okAp) return nullptr;   // 签名初始化失败 ⇒ 停用功能，游戏照跑
    }

    int retry = 0;          // 「命中 0 次」累计轮数（诊断 + 退避）
    int missStreak = 0;
    bool doneOnce = false;  // 开关 ON 期间至少写过一次

    for (;;) {
        if (!isLibraryLoaded(TPL_TARGET_LIB)) {   // 库还没映射：不阻塞，短睡重试
            usleep(100 * 1000);
            continue;
        }

        const bool inv    = g_inv.load();
        const bool forced = g_rescanReq.exchange(false);

        if (!inv) {
            if (g_refCount.load() > 0) {                 // 开关关：回填，恢复零改写
                int verified = 0;
                const int r = RevertAll(&verified);
                // 判读锚点：回填处数 + 逐点读回确认数（verified == n 才算真恢复）
                LOGI(TPL_LOG " dll revert n=%d verified=%d", r, verified);
                doneOnce = false;
                retry = 0;
                missStreak = 0;
            }
            usleep(200 * 1000);
            continue;
        }

        // 开关 ON：扫 rw 段 → 逐命中 PE 校验 → 只写真镜像
        ScanJob job = {};
        const bool scanOk = RunGuarded(ScanJobRun, &job);
        if (!scanOk) {                       // 保护区命中坏页：本轮作废，下轮重试
            ++retry;
            usleep(300 * 1000);
            continue;
        }

        if (job.accepted > 0 || job.rejected > 0) {
            // 判读锚点（一行定判）：真镜像数 / A' 命中数 / 真写入点数 / 重试轮 / 被拒
            LOGI(HXJX_DLL_LOG " dll_sig A=%d Ap=%d patched=%d retry=%d rejected=%d",
                 job.accepted, job.apAccepted, job.written + job.apWritten, retry, job.rejected);
            missStreak = 0;
            doneOnce = true;
        } else {
            ++retry;
            ++missStreak;
            // 未命中同样吐同一格式（retry 递增），便于判定「热更还没完成」而不是「签名错」
            if (missStreak == 1 || (missStreak % 10) == 0) {
                LOGI(HXJX_DLL_LOG " dll_sig A=0 Ap=0 patched=0 retry=%d rejected=0", retry);
            }
        }

        // 节奏：打齐后 5s 补漏；早期 150ms 快轮询（抢在方法首次执行前写入）；
        // 久未命中降到 1.5s 省电；预算被打断 / 开关上升沿立即重扫
        int waitMs = 150;
        if (doneOnce)             waitMs = 5000;
        else if (missStreak > 40) waitMs = 1500;
        if (job.budgetHit && waitMs < 1500) waitMs = 1500;
        if (forced)               waitMs = 0;
        if (waitMs > 0) usleep(static_cast<useconds_t>(waitMs) * 1000);
    }
    return nullptr;
}

// ---- 4.1 懒起线程（幂等；失败只降级） ----
static void EnsurePatchThread() {
    bool expected = false;
    if (!g_threadStarted.compare_exchange_strong(expected, true)) return;

    pthread_attr_t attr;
    pthread_attr_init(&attr);
    pthread_attr_setdetachstate(&attr, PTHREAD_CREATE_DETACHED);
    pthread_t th;
    const int rc = pthread_create(&th, &attr, PatchThreadMain, nullptr);
    pthread_attr_destroy(&attr);

    if (rc != 0) {
        g_threadStarted.store(false);   // 起不来就允许下次再试；功能不可用但游戏照跑
    }
}

// ============================================================================
//  ⑤ 对外接口（ModEntry.h 注册表契约，签名不可改）
// ============================================================================

// 装机自检锚点：so 里必须能字节命中「本游戏 game_id + 值」（tag 对 ≠ id 对）。
// 导出符号 + default 可见性 ⇒ 是 --gc-sections 的根，strip 后仍留在 .rodata 里。
extern "C" __attribute__((used, visibility("default"))) const char kHxjxAuthAnchor[] = "hxjx=1";

bool Hxjx_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;

    InstallFaultGuard();      // 先装兜底
    g_libReady.store(true);
    EnsurePatchThread();      // 线程只等签名，不动 il2cpp API
    return true;
}

bool Hxjx_IsReady() {
    const char* volatile anchor = kHxjxAuthAnchor;   // 引用一次，配合 used 钉住 .rodata
    return g_libReady.load() && (anchor[0] == 'h');
}

void Hxjx_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == HXJX_FEAT_INV) {
        g_inv.exchange(enabled);
        LogInvIfChanged(enabled);
        EnsurePatchThread();                    // 库就绪即常驻，不等开关
        if (enabled) g_rescanReq.store(true);   // 上升沿：立即补漏扫（抢在首次执行前）
    }
}

void Hxjx_OnValueChanged(int /*featNum*/, int /*value*/) {
    // 本作只有开关型功能，无滑块值
}

void Hxjx_Tick(const bool* features, const int* /*values*/) {
    if (!features) return;

    const bool inv = features[HXJX_FEAT_INV];
    const bool prev = g_inv.exchange(inv);
    LogInvIfChanged(inv);
    if (inv && !prev) g_rescanReq.store(true);   // 上升沿：立即重扫补漏
    EnsurePatchThread();                         // 常驻线程（不看开关；开关只决定写还是回填）

    // 兜底：库晚于首帧映射时这里补一次（只置状态 + 起线程，不做任何耗时操作）
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) {
        InstallFaultGuard();
        g_libReady.store(true);
        EnsurePatchThread();
    }

    // 抓点补全：热更载入后装一次解释器/AOT hook（失败 1s 重试；成功即原子置位）
    EnsureIl2CppHooks();

    // 判读锚点：四点各自命中计数（5s 一行，便于真机确认「哪点在跑」）
    static int64_t s_nextStatLog = 0;
    const int64_t nowMs = NowMsMonotonic();
    if (g_hooksDone.load(std::memory_order_relaxed) && nowMs >= s_nextStatLog) {
        s_nextStatLog = nowMs + 5000;
        LOGI(HXJX_DLL_LOG " pts inv=%d atk0=%llu attr=%llu reg=%d/%llu ratio=%llu hud=%llu",
             g_inv.load() ? 1 : 0,
             (unsigned long long)g_statAtkZero.load(),
             (unsigned long long)g_statAttrClamp.load(),
             g_playerHpCount.load(),
             (unsigned long long)g_statReg.load(),
             (unsigned long long)g_statRatioFix.load(),
             (unsigned long long)g_statHudFix.load());
    }

    // ★ 禁止在此 SafeProbe / 扫堆 / 遍历对象（Tick 频率高，扫描全部在后台线程里做）
}

void Hxjx_ForceClear() {
    g_inv.store(false);
    LogInvIfChanged(false);
    ClearPlayerHp();
}
