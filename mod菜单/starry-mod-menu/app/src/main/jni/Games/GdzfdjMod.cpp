/* ================================================================
 *  GdzfdjMod.cpp — 敢达争锋对决（com.kingnet.gundam.mg.normal）hook 实现
 *
 *  依据：Games/_TemplateMod.cpp v3.1（527 行官方骨架）+ Games/JfqsMod.cpp（同族量产范本）
 *
 *  ⚠️ 防闪退设施已内置为代码，删任何一节都可能复现历史闪退：
 *     默认 0 hook（懒装）/ 双路径挂点 / RVA 漂移拒绝 / 幂等安装 / orig 必调 /
 *     reenter 防递归 / SafeWrite 校验写 / 类名缓存 / Tick 轻量
 *     v3.1：并发装钩互斥 / 致命信号兜底 / 槽位熔断
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 *
 *  本游戏实测挂点（dump.cs）：见 kSlots[]。全部走 RVA 主路径 + 序言 4 字节校验，
 *  失败自动降级按名解析（xRole 的 ns 为空串，UnityEngine.Time 的 ns 为 "UnityEngine"）。
 * ================================================================ */
#include "GdzfdjMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <mutex>
#include <setjmp.h>
#include <signal.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

// 生成器把 [gdzfdj] 写成日志 tag；日志过滤：adb logcat -s Mod_Menu
#define GDZFDJ_LOG "[gdzfdj]"

// 注册表模式不依赖 GameConfig.h（legacy 配置中心），目标库直接取混淆串。
#define GDZFDJ_TARGET_LIB sec_lib_il2cpp()

// ============================================================================
//  ① 开关、滑块值与 orig 指针
//     g_libReady = 「库已映射」≠「已挂钩」；全开关关闭时应当保持 0 个 hook。
//     滑块语义铁律：1 = 关，默认必须 1（禁止默认开）。
// ============================================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_rootEnemy{false};      // feat 2：超级定怪
static std::atomic<int>  g_dmgMul{1};             // feat 1：伤害倍率（1 = 关）
static std::atomic<int>  g_speedMul{1};           // feat 3：全局加速（1 = 关）
static std::atomic<bool> g_reenter{false};        // 防递归（hook 体内再进 hook）

// --- 挂点 1：xRole.UpdateAI()  —— 实例方法，0 参 ⇒ 原生签名 (self, method)
typedef void (*UpdateAI_t)(void* self, void* method);
static UpdateAI_t g_origUpdateAI = nullptr;

// --- 挂点 2：xRole.CalcDamage(xRoleStateHit.Param, xRole, xRole, bool, bool, double) -> int
//     ★ 6 参重载（dump.cs @43036）；另一个 4 参重载 @32986 勿混。
//     原生签名 = (self, param, attacker, takeDamage, hasExecuted, absorbed, injuryReduction, method)
//     bool 按 il2cpp ABI 用 C++ bool 声明（与 dump.cs 生成的 native 签名一致）。
typedef int (*CalcDamage_t)(void* self, void* param, void* attacker, void* takeDamage,
                            bool hasExecuted, bool absorbed, double injuryReduction, void* method);
static CalcDamage_t g_origCalcDamage = nullptr;

// --- 挂点 3：UnityEngine.Time.get_deltaTime() -> float —— 静态，0 参 ⇒ (method)
typedef float (*GetDeltaTime_t)(void* method);
static GetDeltaTime_t g_origGetDeltaTime = nullptr;

// --- 挂点 4：UnityEngine.Time.set_timeScale(float) -> void —— 静态，1 参 ⇒ (value, method)
typedef void (*SetTimeScale_t)(float value, void* method);
static SetTimeScale_t g_origSetTimeScale = nullptr;

// ============================================================================
//  ② 常量与 RVA
//     全部来自 dump.cs / IDA 实测，禁止猜；换包必须重 dump。
// ============================================================================
// libil2cpp.so 本包 RX 段 vaddr 比 file_off 大 0x4000（maps 的 off 字段是文件偏移）：
//   RVA 0x3D9CF54 ⇔ file_off 0x3D98F54，差值恒为 0x4000（与 jfqs 同值）。
//   ★ FileOffToVa() 收的是 file_off，故 HookAtRva 内必须做 rva - kElfSegDelta 换算。
enum : uintptr_t { kElfSegDelta = 0x4000 };

// 伤害倍率返回值上限（防 int 溢出；游戏自身伤害可达 1e9 级）
enum : long long { kDamageReturnMax = 2000000000LL };

// ============================================================================
//  ③ 安全助手（防闪退核心；新代码禁止绕开它们裸读写内存）
//     注：本游戏当前 3 个功能都不写游戏字段（只改返回值/跳过调用），
//         下面写读助手保留给后续功能，勿删。
// ============================================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

static void SafeWriteI32(void* obj, size_t off, int32_t v) {
    if (!OkPtr(obj)) return;
    Il2Cpp::WriteI32(obj, off, v);
}

static void SafeWriteF32(void* obj, size_t off, float v) {
    if (!OkPtr(obj)) return;
    Il2Cpp::WriteF32(obj, off, v);
}

static int32_t SafeReadI32(void* obj, size_t off, int32_t defv) {
    if (!OkPtr(obj)) return defv;
    return Il2Cpp::ReadI32(obj, off);
}

// 类名缓存：热路径禁止每帧 object_class + class_name 解析（类指针不变就不重取）
struct ClassNameCache {
    void* klass = nullptr;
    char  name[40] = {};
    bool  valid = false;
};

static const char* CacheClassName(ClassNameCache& cache, void* obj) {
    if (!OkPtr(obj)) return "";
    void* k = GetApi().object_class(obj);
    if (!k) return "";
    if (cache.valid && cache.klass == k) return cache.name;
    const char* n = GetApi().class_name(k);
    cache.klass = k;
    cache.valid = true;
    std::snprintf(cache.name, sizeof(cache.name), "%s", n ? n : "");
    return cache.name;
}

static bool NameHas(const char* got, const char* needle) {
    if (!got || !needle || !*needle) return false;
    return std::strstr(got, needle) != nullptr;
}

// ============================================================================
//  ④ 并发 + 致命信号兜底 + 槽位熔断（v3.1，来自疾风骑士 60.0.1 实战）
//
//  为什么需要（三段真实事故，删任一段都会复现）：
//    (1) 并发装钩竞态：UI 线程与游戏线程同时进入 SyncHooks()，
//        HookSlotInstall 的「幂等」判断是 check-then-act 非原子
//        ⇒ 同一地址被 DobbyHook 两次 ⇒ DobbyCodePatch 越界 memcpy
//        ⇒ SIGSEGV(SEGV_ACCERR)，栈：
//           __memcpy_aarch64_simd ← DobbyCodePatch+96 ← DobbyHook+216
//        修法：g_installMutex 串行化 + double-check。
//    (2) DobbyHook 返回码接不住 Dobby 内部的进程级 SIGSEGV：
//        必须用 sigsetjmp 保护区把「装钩」和「读序言」包起来，
//        炸了就跳过该槽位、游戏继续跑。
//    (3) hook 体内会执行游戏/第三方代码（回调里可能读到尚未填充的字段 ⇒ null-deref）：
//        用 RunGuarded 包住回调，炸了吞掉 + 记日志，不整包崩。
//
//  sigaltstack 是 per-thread 的，备用栈必须 thread_local；
//  故障态同理必须 thread_local（hook 体会在 UI 线程 / 游戏线程 / SDK 回调线程并发执行）。
// ============================================================================

// ---- 4.1 互斥：装钩串行 ----
static std::mutex g_installMutex;   // 保护「装钩全过程」，只在 SyncHooks 里取
static std::mutex g_guardMutex;     // 只保护一次性「安装信号处理器」动作

// ---- 4.2 本线程故障态（thread_local：hook 体在多线程并发） ----
static thread_local sigjmp_buf              t_faultJmp;
static thread_local volatile sig_atomic_t   t_inGuard    = 0;   // 是否正在保护区内
static thread_local volatile sig_atomic_t   t_faultDepth = 0;   // 保护区嵌套/连炸计数
static thread_local volatile int            t_faultSig   = 0;   // 最近一次致命信号号
static thread_local volatile void*          t_faultAddr  = nullptr; // 最近一次故障地址

// 备用信号栈（SIGSEGV 时原栈可能已不可用）；64KB/线程，per-thread 必须 thread_local
static thread_local char t_altStack[65536];
static thread_local bool t_altStackSet = false;

// ---- 4.3 被链式接管的旧处理器 ----
static struct sigaction g_oldSegv;
static struct sigaction g_oldBus;
static struct sigaction g_oldIll;
static std::atomic<bool> g_guardArmed{false};
// 装钩期是否发生过致命信号：只在 g_installMutex 保护下读写
static bool g_lastFatal = false;

// ---- 4.4 故障处理器 ----
static void GdzfdjFaultHandler(int sig, siginfo_t* si, void* /*uc*/) {
    if (t_inGuard) {
        // 保护区内连炸：说明保护区本身不可靠，交回系统默认行为（宁可崩，不要死循环）
        if (++t_faultDepth > 4) {
            signal(sig, SIG_DFL);
            raise(sig);
            return;
        }
        t_faultSig  = sig;
        t_faultAddr = si ? si->si_addr : nullptr;
        siglongjmp(t_faultJmp, 1);   // 永不返回
    }

    // 保护区外：链回旧处理器，再走默认
    const struct sigaction* old = (sig == SIGSEGV) ? &g_oldSegv
                               : (sig == SIGBUS)  ? &g_oldBus
                                                  : &g_oldIll;
    if (old->sa_sigaction && old->sa_sigaction != GdzfdjFaultHandler) {
        old->sa_sigaction(sig, si, nullptr);
        return;
    }
    signal(sig, SIG_DFL);
    raise(sig);
}

// ---- 4.5 为本线程挂上备用信号栈（懒执行，失败降级为无备用栈） ----
static void ArmFaultGuardForThread() {
    if (t_altStackSet) return;
    t_altStackSet = true;
    stack_t ss;
    std::memset(&ss, 0, sizeof(ss));
    ss.ss_sp    = t_altStack;
    ss.ss_size  = sizeof(t_altStack);
    ss.ss_flags = 0;
    if (sigaltstack(&ss, nullptr) != 0) {
        LOGW(GDZFDJ_LOG " sigaltstack failed — 兜底降级为无备用栈");
    }
}

// ---- 4.6 一次性安装进程级处理器 ----
static void InstallFaultGuard() {
    if (g_guardArmed.load()) return;
    std::lock_guard<std::mutex> lk(g_guardMutex);
    if (g_guardArmed.load()) return;

    struct sigaction sa;
    std::memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = GdzfdjFaultHandler;
    sa.sa_flags     = SA_SIGINFO | SA_ONSTACK | SA_NODEFER;
    sigemptyset(&sa.sa_mask);

    sigaction(SIGSEGV, &sa, &g_oldSegv);
    sigaction(SIGBUS,  &sa, &g_oldBus);
    sigaction(SIGILL,  &sa, &g_oldIll);

    g_guardArmed.store(true);
    LOGI(GDZFDJ_LOG " fault guard armed (SIGSEGV/SIGBUS/SIGILL, altstack=%zu)",
         sizeof(t_altStack));
}

// ---- 4.7 保护区包装器 ----
// 装钩：Dobby 内部崩（返回码接不住的那种）⇒ 返回 false + 置 g_lastFatal
static bool DobbyHookGuarded(const char* tag, void* target, void* hook, void** orig) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        int rc = DobbyHook(target, hook, orig);
        t_inGuard = 0;
        if (rc != 0) {
            LOGW(GDZFDJ_LOG " %s dobby fail rc=%d", tag, rc);
            return false;
        }
        return true;
    }
    // longjmp 回来：保护区内的致命信号已被吞掉
    t_inGuard = 0;
    g_lastFatal = true;
    LOGW(GDZFDJ_LOG " %s dobby SIG(%d) @%p — 槽位熔断，游戏继续",
         tag, (int)t_faultSig, (void*)t_faultAddr);
    if (orig) *orig = nullptr;   // ★ 必须清掉可能被半写的 orig，否则后续调用跳到野指针
    return false;
}

// 读序言（RVA 漂移校验）：读坏页也会炸，同样走保护区
static bool ReadU32Guarded(const char* tag, const void* p, uint32_t* out) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        *out = *reinterpret_cast<const volatile uint32_t*>(p);
        t_inGuard = 0;
        return true;
    }
    t_inGuard = 0;
    LOGW(GDZFDJ_LOG " %s prologue read SIG(%d) @%p — 跳过该槽位",
         tag, (int)t_faultSig, (void*)t_faultAddr);
    return false;
}

// 跑第三方代码（游戏回调 / SDK 回调）：炸了吞掉，只记日志
static bool RunGuarded(const char* tag, void (*fn)(void*), void* arg) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        fn(arg);
        t_inGuard = 0;
        return true;
    }
    t_inGuard = 0;
    LOGW(GDZFDJ_LOG " %s 回调内 SIG(%d) @%p — 已吞掉，游戏继续",
         tag, (int)t_faultSig, (void*)t_faultAddr);
    return false;
}

// ---- 4.8 槽位熔断 ----
// 装钩期炸过的槽位，本次会话不再重试（否则 Tick 每轮再炸一次 ⇒ 每轮一次闪退）
static const int kSlotsMax = 64;
static bool g_slotDead[kSlotsMax];                    // 只在 g_installMutex 下读写
static std::atomic<int> g_installedCount{0};

// ============================================================================
//  ⑤ 双路径挂点机器（提炼自 WhrhxMod 实测；禁 base+RVA、禁全 RX 盲扫）
// ============================================================================
static bool IsLibMapLine(const char* line, const char* lib) {
    return std::strstr(line, lib) != nullptr && std::strstr(line, ".so") != nullptr;
}

// file_off → 运行时 VA：libil2cpp 是分段映射，base+RVA 会挂到错误地址，
// 必须扫 /proc/self/maps 只取可执行段，并在覆盖该 file_off 的段里取最大 span。
static void* FileOffToVa(const char* lib, uintptr_t fileOff) {
    FILE* fp = std::fopen("/proc/self/maps", "r");
    if (!fp) return nullptr;

    char line[768];
    void* best = nullptr;
    uintptr_t bestSpan = 0;

    while (std::fgets(line, sizeof(line), fp)) {
        if (!IsLibMapLine(line, lib)) continue;

        uintptr_t start = 0, end = 0, off = 0;
        char perm[8] = {};
        if (std::sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!std::strchr(perm, 'x')) continue;   // 只要可执行段
        if (end <= start) continue;

        uintptr_t span = end - start;
        if (fileOff >= off && fileOff < off + span) {
            if (span > bestSpan) {
                bestSpan = span;
                best = reinterpret_cast<void*>(start + (fileOff - off));
            }
        }
    }
    std::fclose(fp);
    return best;
}

// RVA 主路径：表里填的是 dump.cs 的 RVA，本包需 −0x4000 换成 file_off。
static bool HookAtRva(const char* tag, uintptr_t rva, uint32_t expectInsn,
                      void* hook, void** orig) {
    if (rva < kElfSegDelta) {
        LOGW(GDZFDJ_LOG " %s rva 0x%lX < seg delta — try by-name", tag, (unsigned long)rva);
        return false;
    }
    void* va = FileOffToVa(GDZFDJ_TARGET_LIB, rva - kElfSegDelta);
    if (!va) {
        LOGW(GDZFDJ_LOG " %s rva 0x%lX not in RX — try by-name", tag, (unsigned long)rva);
        return false;
    }
    if (expectInsn) {
        uint32_t got = 0;
        if (!ReadU32Guarded(tag, va, &got)) return false;   // 读序言也走保护区
        if (got == 0 || got == 0xFFFFFFFFu || got != expectInsn) {
            LOGW(GDZFDJ_LOG " %s rva drift got=0x%08X want=0x%08X — try by-name",
                 tag, got, expectInsn);
            return false;
        }
    }
    if (!DobbyHookGuarded(tag, va, hook, orig)) {
        LOGW(GDZFDJ_LOG " %s dobby fail @rva 0x%lX", tag, (unsigned long)rva);
        return false;
    }
    LOGI(GDZFDJ_LOG " %s hooked @rva 0x%lX", tag, (unsigned long)rva);
    return true;
}

static bool EnsureApi() {
    if (GetApi().ready) return true;
    return GetApi().Load(GDZFDJ_TARGET_LIB);
}

static void* FindClassNs(const char* ns, const char* name) {
    size_t n = 0;
    void** asms = GetApi().Assemblies(&n);
    for (size_t i = 0; asms && i < n; ++i) {
        void* img = GetApi().ImageOf(asms[i]);
        if (!img) continue;
        void* k = GetApi().ClassFromName(img, ns, name);
        if (k) return k;
    }
    return nullptr;
}

// 类/方法缺失只记日志降级，绝不让整包失败（hook-rules §17）
static bool HookByName(const char* tag, const char* ns, const char* cls,
                       const char* method, int argc, void* hook, void** orig) {
    if (!EnsureApi()) {
        LOGW(GDZFDJ_LOG " %s api not ready — will retry via Tick", tag);
        return false;
    }

    void* k = FindClassNs(ns, cls);
    if (!k) k = GetApi().FindClassByShortName(cls);
    if (!k) {
        LOGW(GDZFDJ_LOG " %s class miss %s — degrade", tag, cls);
        return false;
    }

    void* mi = GetApi().GetMethod(k, method, argc);
    if (!mi) {
        LOGW(GDZFDJ_LOG " %s method miss %s.%s(argc=%d) — degrade, install others",
             tag, cls, method, argc);
        return false;
    }

    void* fn = *reinterpret_cast<void**>(mi);
    if (!OkPtr(fn)) {
        LOGW(GDZFDJ_LOG " %s bad method ptr %s.%s", tag, cls, method);
        return false;
    }

    if (!DobbyHookGuarded(tag, fn, hook, orig)) return false;
    LOGI(GDZFDJ_LOG " %s hooked by-name %s.%s(argc=%d)", tag, cls, method, argc);
    return true;
}

struct HookSlot {
    const char* tag;
    uintptr_t   rva;          // 0 = 只走按名（dump.cs 的 RVA；HookAtRva 内 −0x4000）
    uint32_t    expectInsn;   // 0 = 不校验序言；实测后回填
    const char* ns;
    const char* cls;
    const char* method;
    int         argc;
    void*       hookFn;
    void**      origPtr;
};

// 幂等安装（防同址双 Dobby 叠钩 SIGSEGV）→ RVA 优先 → 失败转按名
static bool HookSlotInstall(const HookSlot& s) {
    if (s.origPtr && *s.origPtr) return true;
    if (s.rva && HookAtRva(s.tag, s.rva, s.expectInsn, s.hookFn, s.origPtr)) return true;
    return HookByName(s.tag, s.ns, s.cls, s.method, s.argc, s.hookFn, s.origPtr);
}

// ============================================================================
//  ⑥ hook 函数与挂点表
//     铁律：一个具体方法一个槽；绝不 hook 共享 invoke 入口。
// ============================================================================
// 挂点 2：伤害倍率（乘法倍率，不写回游戏字段）
static int Hooked_CalcDamage(void* self, void* param, void* attacker, void* takeDamage,
                             bool hasExecuted, bool absorbed, double injuryReduction,
                             void* method) {
    if (!g_origCalcDamage) return 0;   // 不可达：只有装钩成功（orig 非空）才会进本函数

    if (g_reenter.load()) {            // 递归重入：原样放行，绝不叠乘
        return g_origCalcDamage(self, param, attacker, takeDamage,
                                hasExecuted, absorbed, injuryReduction, method);
    }
    g_reenter.store(true);
    int r = g_origCalcDamage(self, param, attacker, takeDamage,
                             hasExecuted, absorbed, injuryReduction, method);
    g_reenter.store(false);

    const int mul = g_dmgMul.load();
    if (mul <= 1) return r;            // 1 = 关

    long long v = (long long)r * (long long)mul;
    if (v >  kDamageReturnMax) v =  kDamageReturnMax;   // 防 int 溢出
    if (v < -kDamageReturnMax) v = -kDamageReturnMax;
    return (int)v;
}

// 挂点 1：超级定怪（跳过 AI 更新 —— 状态门禁，hook-rules §28 允许的唯一例外形态：
//          开关为 Toggle 且默认 false，绝不恒真）
static void Hooked_UpdateAI(void* self, void* method) {
    if (g_reenter.load()) {            // 在别人的 hook 链里：原样放行
        if (g_origUpdateAI) g_origUpdateAI(self, method);
        return;
    }
    if (g_rootEnemy.load()) return;    // ★ 定怪：本次不推进 AI
    if (g_origUpdateAI) g_origUpdateAI(self, method);
}

// 挂点 3：全局加速（时间缩放直读路径）
static float Hooked_GetDeltaTime(void* method) {
    float d = g_origGetDeltaTime ? g_origGetDeltaTime(method) : 0.0f;
    const int mul = g_speedMul.load();
    if (mul <= 1) return d;            // 1 = 关
    return d * (float)mul;
}

// 挂点 4：全局加速（时间缩放写路径；value<=0 的暂停/倒放语义原样透传）
static void Hooked_SetTimeScale(float value, void* method) {
    const int mul = g_speedMul.load();
    if (mul <= 1 || value <= 0.0f) {
        if (g_origSetTimeScale) g_origSetTimeScale(value, method);
        return;
    }
    if (g_origSetTimeScale) g_origSetTimeScale(value * (float)mul, method);
}

// RVA / file_off / 序言（磁盘原始 4 字节，小端）：
//   1) UpdateAI      RVA 0x3D9CF54 / file_off 0x3D98F54 / FE 0F 1D F8 = str x30,[sp,#-0x30]!
//   2) CalcDamage(6) RVA 0x3DB99CC / file_off 0x3DB59CC / FF 83 03 D1 = sub sp,sp,#0xe0
//   3) get_deltaTime RVA 0x67BE180 / file_off 0x67BA180 / FE 4F BF A9
//   4) set_timeScale RVA 0x67BE458 / file_off 0x67BA458 / E8 0F 1E FC
static const HookSlot kSlots[] = {
    {"UpdateAI",     0x3D9CF54u, 0xF81D0FFEu, "",            "xRole", "UpdateAI",
     0, (void*)Hooked_UpdateAI,     (void**)&g_origUpdateAI},
    {"CalcDamage6",  0x3DB99CCu, 0xD10383FFu, "",            "xRole", "CalcDamage",
     6, (void*)Hooked_CalcDamage,   (void**)&g_origCalcDamage},
    {"get_deltaTime",0x67BE180u, 0xA9BF4FFEu, "UnityEngine", "Time",  "get_deltaTime",
     0, (void*)Hooked_GetDeltaTime, (void**)&g_origGetDeltaTime},
    {"set_timeScale",0x67BE458u, 0xFC1E0FE8u, "UnityEngine", "Time",  "set_timeScale",
     1, (void*)Hooked_SetTimeScale, (void**)&g_origSetTimeScale},
};

// ============================================================================
//  ⑦ 分组安装 + 懒装（全开关关 = 0 hook）
// ============================================================================
static const int kSlotsCount = (int)(sizeof(kSlots) / sizeof(kSlots[0]));

static int InstallBattleHooks() {
    int ok = 0, dead = 0;
    for (int i = 0; i < kSlotsCount && i < kSlotsMax; ++i) {
        if (g_slotDead[i]) { ++dead; continue; }        // 熔断，跳过
        g_lastFatal = false;
        if (HookSlotInstall(kSlots[i])) {
            ++ok;
        } else if (g_lastFatal) {
            g_slotDead[i] = true;                       // 永久拉黑（本次会话）
            LOGW(GDZFDJ_LOG " slot[%d] %s 已熔断（装钩期致命信号），本次会话不再重试",
                 i, kSlots[i].tag);
        }
    }
    LOGI(GDZFDJ_LOG " install pass: ok=%d dead=%d/%d", ok, dead, kSlotsCount);
    if (ok > 0) g_anyHooked.store(true);
    return ok;
}

// 任意功能开启（滑块 >1 视为开；1 = 关）
static bool AnyFeatureOn() {
    return g_rootEnemy.load() || g_dmgMul.load() > 1 || g_speedMul.load() > 1;
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    if (!AnyFeatureOn()) return;                        // 全关：不装、也不卸载
    if (g_installedCount.load() >= kSlotsCount) return; // 快路径：已装齐

    std::lock_guard<std::mutex> lk(g_installMutex);     // ★ 串行化，防并发双挂
    if (g_installedCount.load() >= kSlotsCount) return; // double-check
    g_installedCount.store(InstallBattleHooks());
}

// 滑块钳制（与菜单声明段 min/max 一致）
static int ClampInt(int v, int lo, int hi) {
    if (v < lo) return lo;
    if (v > hi) return hi;
    return v;
}

// ============================================================================
//  ⑧ 对外接口（ModEntry.h 注册表契约，签名不可改）
// ============================================================================
bool Gdzfdj_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(GDZFDJ_TARGET_LIB)) return false;

    // 装机自检锚点：下列字面量必须留在 so 里（grep 自检靠它，中文串不能用 strings 查）
    LOGI(GDZFDJ_LOG " probe: nav=Category_战斗 dmg=Slider_伤害倍率:1:100:1"
                    " root=Toggle_超级定怪 spd=Slider_全局加速:1:10:1");

    InstallFaultGuard();          // §④ 先装兜底，再装钩
    g_libReady.store(true);
    SyncHooks();                  // 已 ON 的开关立即装；全关保持 0 hook
    LOGI(GDZFDJ_LOG " lib ready (dual-path, lazy hooks)");
    return true;
}

bool Gdzfdj_IsReady() {
    return g_libReady.load();
}

void Gdzfdj_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case GDZFDJ_FEAT_ROOT:
            g_rootEnemy.store(enabled);
            break;
        case GDZFDJ_FEAT_DMG_MUL:
            // 滑块行开关关闭 ⇒ 值回落 1（1 = 关，与 forceFeatureOff 的 nativeSetValue(reset) 同义）
            if (!enabled) g_dmgMul.store(1);
            break;
        case GDZFDJ_FEAT_SPEED_MUL:
            if (!enabled) g_speedMul.store(1);
            break;
        default:
            break;
    }
    SyncHooks();
}

void Gdzfdj_OnValueChanged(int featNum, int value) {
    switch (featNum) {
        case GDZFDJ_FEAT_DMG_MUL:
            g_dmgMul.store(ClampInt(value, 1, GDZFDJ_DMG_MUL_MAX));
            break;
        case GDZFDJ_FEAT_SPEED_MUL:
            g_speedMul.store(ClampInt(value, 1, GDZFDJ_SPEED_MUL_MAX));
            break;
        default:
            break;
    }
    SyncHooks();
}

void Gdzfdj_Tick(const bool* features, const int* values) {
    if (!features) return;

    g_rootEnemy.store(features[GDZFDJ_FEAT_ROOT]);

    if (values) {
        g_dmgMul.store(ClampInt(values[GDZFDJ_FEAT_DMG_MUL], 1, GDZFDJ_DMG_MUL_MAX));
        g_speedMul.store(ClampInt(values[GDZFDJ_FEAT_SPEED_MUL], 1, GDZFDJ_SPEED_MUL_MAX));
    }

    // 兜底：库晚于首帧映射时，这里补一次
    if (!g_libReady.load() && isLibraryLoaded(GDZFDJ_TARGET_LIB)) {
        InstallFaultGuard();
        g_libReady.store(true);
        LOGI(GDZFDJ_LOG " lib ready (via Tick fallback)");
    }
    SyncHooks();

    // ★ 禁止在此 SafeProbe / 扫堆 / 遍历对象 / 逐对象写（Tick 频率高，代价爆炸）
}

// 授权失效 / 进程收尾：全部回到默认（滑块回 1 = 关，开关回 false）
void Gdzfdj_ForceClear() {
    g_rootEnemy.store(false);
    g_dmgMul.store(1);
    g_speedMul.store(1);
    g_reenter.store(false);
}
