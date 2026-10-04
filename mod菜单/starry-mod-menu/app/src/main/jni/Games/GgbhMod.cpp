// ============================================================================
//  Games/GgbhMod.cpp — 鬼谷八荒（com.guigugame.guigubahuang 1.1.329）星空实现
//
//  对照方：斌哥改包（libSDKloading.so 壳，Dobby 4 字节 b inline hook）
//  抓点证据：10_docs/鬼谷八荒/斌哥包抓点_20261004.md §五 / 路线锁定.md
//
//  移植范围（用户点名，硬约束 #1，禁止扩张）：只有两项
//    ① GM 菜单   ← UISwitchPlayerMenu.get_IsCheatModeEnabled 读侧恒真
//    ② 全局加速   ← Time.get_deltaTime 每帧强写 Time.timeScale + set_timeScale 透传
//  第 3 个壳 hook（libunity 符号解析拦截）是壳私有基础设施，不移植（路线锁定.md 第 3 行）
//
//  对照方 1:1 核对要点（路线锁定.md 表内已逐行确认）：
//    · 三处挂点 RVA 与斌哥逐字节相同（官包 so md5 与斌哥包一致 = 瘦壳）
//    · GM 槽：斌哥 detour 读旗标字节 !=0 → return true；我们读菜单功能位 g_gmMenu
//    · deltaTime 槽：斌哥每帧 blr 原 setter 写倍率再尾调原 getter；我们同序
//    · set_timeScale 槽：斌哥纯透传（br 原函数）；我们同（调 orig 后返回）
//    · 唯一偏离：门控/倍率的存储位置（壳内存字节 → 菜单功能位/原子量），已记入偏离栏
//
//  本包特殊性（设备画像实测，10_docs/_state/设备画像/f9a116a2.md）：
//    libil2cpp 是「只读影子 + 执行真身」多映射（组数=2），rw 段与 rx 段之间隔 0x10000 空隙，
//    框架通用 LibBaseFromMaps() 会选到 rw 段起点（PICK base=0x6fedb32000）⇒ 三处 RVA 全不命中。
//    ⇒ 本模块**只走 FileOffToVa（maps 可执行段扫描）**，禁止 base+RVA、禁止全 RX 盲扫。
// ============================================================================

#include "GgbhMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <unistd.h>   // sysconf（SELFCHECK 的 page=）

#include <mutex>      // §④ 装钩互斥
#include <setjmp.h>   // §④ sigsetjmp 保护区
#include <signal.h>   // §④ 致命信号兜底

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "ModDiag.h"          // DLOG/DLOGN：不带 STARRY_DIAG 时宏展开为 no-op
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

// 日志过滤：adb logcat -s Mod_Menu
#define TPL_LOG "[Ggbh]"
#define TPL_TARGET_LIB sec_lib_il2cpp()   // 返回 "libil2cpp.so"（OBFUSCATE）

// ============================================================================
//  ① 开关与 orig 指针
//     g_libReady = 「库已映射」≠「已挂钩」；两项功能全关时保持 0 个 hook。
// ============================================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_gmMenu{false};     // 功能位 1：GM 菜单
static std::atomic<int>  g_speedMult{1};      // 功能位 2：加速倍率（1 = 关）
// 上一次写进 Time.timeScale 的倍率（1 = 未改写）。用途：把滑块调回 1（或关功能）时
// 还原 timeScale=1，避免残留加速（斌哥包同位置只会一直写当前值，关掉后 timeScale 残留）。
static std::atomic<int>  g_pinnedMult{1};
static std::atomic<bool> g_reenter{false};
static std::atomic<bool> g_rdOk{false};       // 序言读回校验通过过（SELFCHECK rd=）
static std::atomic<bool> g_selfCheckDone{false};

// 三处挂点的签名（与 il2cpp AOT wrapper 一致：第一参 = MethodInfo*）
typedef bool  (*CheatGetter_t)(void* method);
typedef float (*DeltaTime_t)(void* method);
typedef void  (*SetTimeScale_t)(float value, void* method);

static CheatGetter_t  g_origCheatGetter  = nullptr;
static DeltaTime_t    g_origDeltaTime    = nullptr;
static SetTimeScale_t g_origSetTimeScale = nullptr;

// setter 兜底直呼入口（仅当 set_timeScale 槽装钩失败时用；范式见 BlfxMod.cpp ApplySpeed）
static SetTimeScale_t g_setTimeScaleDirect = nullptr;
static void*          g_miSetTimeScale     = nullptr;
static std::atomic<bool> g_triedDirect{false};

// hit 计数（hook-rules §22 第 2/4 步：只读计数验证 + 首次装机日志必须含 hit 计数）
static std::atomic<uint32_t> g_hitGm{0};
static std::atomic<uint32_t> g_hitDelta{0};
static std::atomic<uint32_t> g_hitSetTs{0};

// ============================================================================
//  ② RVA 与序言期望值（pristine 官包实测；换包必须重 dump —— hook-rules §23）
//     libil2cpp.so md5 188c796e6b53b6e520a327f259ca565f（官包 == 斌哥包）
// ============================================================================
enum : uintptr_t {
    kRvaCheatGetter  = 0x40A4D28,  // UISwitchPlayerMenu.get_IsCheatModeEnabled（namespace 空）
    kRvaDeltaTime    = 0x54553E4,  // UnityEngine.Time.get_deltaTime
    kRvaSetTimeScale = 0x5455560,  // UnityEngine.Time.set_timeScale(float)
};

enum : uint32_t {
    kInsnStrX19Sp20 = 0xF81E0FF3u, // str x19,[sp,#-0x20]!  （get_IsCheatModeEnabled / get_deltaTime）
    kInsnStrD8Sp20  = 0xFC1E0FE8u, // str d8,[sp,#-0x20]!   （set_timeScale）
};

enum : int { kSpeedMin = 1, kSpeedMax = 20 };   // 滑块 1=关，2..20=倍率

static inline bool SpeedOn() {
    return g_speedMult.load(std::memory_order_relaxed) > kSpeedMin;
}

// ============================================================================
//  ③ 安全助手（新代码禁止绕开它们裸读写内存）
// ============================================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

// ============================================================================
//  ④ 并发 + 致命信号兜底 + 槽位熔断（v3.1，模板原样保留，勿删）
//     三段事故来源：并发双挂 SIGSEGV / DobbyHook 返回码接不住的进程级 SIGSEGV /
//     hook 体内执行游戏回调崩（详见模板头注释）
// ============================================================================
static std::mutex g_installMutex;
static std::mutex g_guardMutex;

static thread_local sigjmp_buf              t_faultJmp;
static thread_local volatile sig_atomic_t   t_inGuard    = 0;
static thread_local volatile sig_atomic_t   t_faultDepth = 0;
static thread_local volatile int            t_faultSig   = 0;
static thread_local volatile void*          t_faultAddr  = nullptr;

static thread_local char t_altStack[65536];
static thread_local bool t_altStackSet = false;

static struct sigaction g_oldSegv;
static struct sigaction g_oldBus;
static struct sigaction g_oldIll;
static std::atomic<bool> g_guardArmed{false};
static bool g_lastFatal = false;   // 只在 g_installMutex 下读写

static void TplFaultHandler(int sig, siginfo_t* si, void* /*uc*/) {
    if (t_inGuard) {
        // 保护区内连炸：保护区本身不可靠，交回系统默认（宁可崩，不要死循环）
        if (++t_faultDepth > 4) {
            signal(sig, SIG_DFL);
            raise(sig);
            return;
        }
        t_faultSig  = sig;
        t_faultAddr = si ? si->si_addr : nullptr;
        siglongjmp(t_faultJmp, 1);
    }
    const struct sigaction* old = (sig == SIGSEGV) ? &g_oldSegv
                               : (sig == SIGBUS)  ? &g_oldBus
                                                  : &g_oldIll;
    if (old->sa_sigaction && old->sa_sigaction != TplFaultHandler) {
        old->sa_sigaction(sig, si, nullptr);
        return;
    }
    signal(sig, SIG_DFL);
    raise(sig);
}

static void ArmFaultGuardForThread() {
    if (t_altStackSet) return;
    t_altStackSet = true;
    stack_t ss;
    std::memset(&ss, 0, sizeof(ss));
    ss.ss_sp    = t_altStack;
    ss.ss_size  = sizeof(t_altStack);
    ss.ss_flags = 0;
    if (sigaltstack(&ss, nullptr) != 0) {
        LOGW(TPL_LOG " sigaltstack failed — 兜底降级为无备用栈");
    }
}

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
    LOGI(TPL_LOG " fault guard armed (SIGSEGV/SIGBUS/SIGILL, altstack=%zu)", sizeof(t_altStack));
}

static bool DobbyHookGuarded(const char* tag, void* target, void* hook, void** orig) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        int rc = DobbyHook(target, hook, orig);
        t_inGuard = 0;
        if (rc != 0) {
            LOGW(TPL_LOG " %s dobby fail rc=%d", tag, rc);
            return false;
        }
        return true;
    }
    t_inGuard = 0;
    g_lastFatal = true;
    LOGW(TPL_LOG " %s dobby SIG(%d) @%p — 槽位熔断，游戏继续",
         tag, (int)t_faultSig, (void*)t_faultAddr);
    if (orig) *orig = nullptr;   // ★ 清掉半写的 orig，否则后续调用跳野指针
    return false;
}

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
    LOGW(TPL_LOG " %s prologue read SIG(%d) @%p — 跳过该槽位",
         tag, (int)t_faultSig, (void*)t_faultAddr);
    return false;
}

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
    LOGW(TPL_LOG " %s 回调内 SIG(%d) @%p — 已吞掉，游戏继续",
         tag, (int)t_faultSig, (void*)t_faultAddr);
    return false;
}

static const int kSlotsMax = 64;
static bool g_slotDead[kSlotsMax];                    // 只在 g_installMutex 下读写
static std::atomic<int> g_installedCount{0};

// ============================================================================
//  ⑤ 双路径挂点机器（提炼自 WhrhxMod 实测；禁 base+RVA、禁全 RX 盲扫）
// ============================================================================
static bool IsLibMapLine(const char* line, const char* lib) {
    return std::strstr(line, lib) != nullptr && std::strstr(line, ".so") != nullptr;
}

// file_off → 运行时 VA：libil2cpp 是分段映射（本游戏尤其：rw 段与 rx 段隔 0x10000 空隙），
// base+RVA 会挂到错误地址，必须扫 /proc/self/maps 只取可执行段，并在覆盖该 file_off 的段里取最大 span。
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

// SELFCHECK 用（非热路径）：本游戏组数=2（只读影子 + 执行真身），base 取 off==0 的可执行段起点
static void MapsProbe(int* groups, uintptr_t* base, int* page) {
    if (groups) *groups = 0;
    if (base)   *base   = 0;
    if (page)   *page   = (int)sysconf(_SC_PAGESIZE);

    FILE* fp = std::fopen("/proc/self/maps", "r");
    if (!fp) return;
    char line[768];
    int g = 0;
    uintptr_t execOff0 = 0;
    while (std::fgets(line, sizeof(line), fp)) {
        if (!IsLibMapLine(line, TPL_TARGET_LIB)) continue;
        uintptr_t start = 0, end = 0, off = 0;
        char perm[8] = {};
        if (std::sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (off == 0) ++g;
        if (!execOff0 && off == 0 && std::strchr(perm, 'x')) execOff0 = start;
    }
    std::fclose(fp);
    if (groups) *groups = g;
    if (base)   *base   = execOff0;
}

static bool HookAtRva(const char* tag, uintptr_t rva, uint32_t expectInsn,
                      void* hook, void** orig) {
    void* va = FileOffToVa(TPL_TARGET_LIB, rva);
    if (!va) {
        LOGW(TPL_LOG " %s rva 0x%lX not in RX — try by-name", tag, (unsigned long)rva);
        return false;
    }
    if (expectInsn) {
        uint32_t got = 0;
        if (!ReadU32Guarded(tag, va, &got)) return false;   // 读序言也走保护区
        if (got == 0 || got == 0xFFFFFFFFu || got != expectInsn) {
            LOGW(TPL_LOG " %s rva drift got=0x%08X want=0x%08X — try by-name",
                 tag, got, expectInsn);
            return false;
        }
        g_rdOk.store(true);   // 序言读回校验通过（SELFCHECK rd=1）
    }
    if (!DobbyHookGuarded(tag, va, hook, orig)) {
        LOGW(TPL_LOG " %s dobby fail @rva 0x%lX", tag, (unsigned long)rva);
        return false;
    }
    LOGI(TPL_LOG " %s hooked @rva 0x%lX va=%p", tag, (unsigned long)rva, va);
    return true;
}

static bool EnsureApi() {
    if (GetApi().ready) return true;
    return GetApi().Load(TPL_TARGET_LIB);
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
        LOGW(TPL_LOG " %s api not ready — will retry via Tick", tag);
        return false;
    }

    void* k = FindClassNs(ns, cls);
    if (!k) k = GetApi().FindClassByShortName(cls);
    if (!k) {
        LOGW(TPL_LOG " %s class miss %s — degrade", tag, cls);
        return false;
    }

    void* mi = GetApi().GetMethod(k, method, argc);
    if (!mi) {
        LOGW(TPL_LOG " %s method miss %s.%s(argc=%d) — degrade, install others",
             tag, cls, method, argc);
        return false;
    }

    void* fn = *reinterpret_cast<void**>(mi);
    if (!OkPtr(fn)) {
        LOGW(TPL_LOG " %s bad method ptr %s.%s", tag, cls, method);
        return false;
    }

    if (!DobbyHookGuarded(tag, fn, hook, orig)) return false;
    LOGI(TPL_LOG " %s hooked by-name %s.%s(argc=%d) fn=%p", tag, cls, method, argc, fn);
    return true;
}

struct HookSlot {
    const char* tag;
    uintptr_t   rva;          // 0 = 只走按名
    uint32_t    expectInsn;   // 0 = 不校验序言
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

// ---- 功能①：GM 菜单 --------------------------------------------------------
// 对照方（斌哥 detour @0x7438AAC9B8）：
//   adrp/ldrb 旗标; cbz w8,→原函数; mov w0,#1; ret; 原路径: br [原函数桩]
// ⇒ 旗标 ON 时恒真，OFF 时透传。我们 1:1（旗标换成菜单功能位 g_gmMenu）。
// 备注：set_IsCheatModeEnabled（RVA 0x40A4DC4）斌哥未挂，我们也不挂（1:1）。
static bool Hooked_get_IsCheatModeEnabled(void* method) {
    const uint32_t hit = g_hitGm.fetch_add(1, std::memory_order_relaxed) + 1;
    if (hit <= 3) {
        LOGI(TPL_LOG " PROBE gmCheat hit=%u on=%d", hit, (int)g_gmMenu.load());
    }
    if (g_gmMenu.load(std::memory_order_relaxed)) return true;   // ★ 读侧恒真
    if (g_origCheatGetter) return g_origCheatGetter(method);     // OFF：零改写透传
    return false;
}

// ---- 功能②：全局加速（施力点） ---------------------------------------------
// 对照方（斌哥 detour @0x7438AACA14）：
//   读 int32 倍率 → scvtf → blr 原 set_timeScale → br 原 get_deltaTime
// ⇒ 借每帧必被调用的 get_deltaTime 钉 Time.timeScale = 倍率，再走原 getter。
// 我们同序，但倍率来源 = 菜单滑块（g_speedMult），倍率==1 时不写（零改写契约），
// 并把倍率调回 1 / 关功能时**还原 timeScale=1**（消残留，优于对照方）。
static float Hooked_get_deltaTime(void* method) {
    const uint32_t hit = g_hitDelta.fetch_add(1, std::memory_order_relaxed) + 1;
    const int mult = g_speedMult.load(std::memory_order_relaxed);
    const bool needWrite = (mult > kSpeedMin) ||
                           (g_pinnedMult.load(std::memory_order_relaxed) > kSpeedMin);

    SetTimeScale_t setter = g_origSetTimeScale;
    if (!setter && needWrite && !g_triedDirect.exchange(true)) {
        // 槽 2 装钩失败时的兜底：按名取 MethodInfo 直呼 AOT 本体
        if (EnsureApi()) {
            void* k = FindClassNs("UnityEngine", "Time");
            if (!k) k = GetApi().FindClassByShortName("Time");
            void* mi = k ? GetApi().GetMethod(k, "set_timeScale", 1) : nullptr;
            if (mi) {
                void* fn = *reinterpret_cast<void**>(mi);
                if (OkPtr(fn)) {
                    g_miSetTimeScale = mi;
                    g_setTimeScaleDirect = (SetTimeScale_t)fn;
                }
            }
            LOGI(TPL_LOG " setter fallback mi=%p fn=%p",
                 g_miSetTimeScale, (void*)g_setTimeScaleDirect);
        }
    }
    if (!setter) setter = g_setTimeScaleDirect;

    if (setter) {
        if (mult > kSpeedMin) {
            setter((float)mult, method);                   // ★ 1:1 斌哥：每帧强写 timeScale
            g_pinnedMult.store(mult, std::memory_order_relaxed);
        } else {
            const int last = g_pinnedMult.exchange(kSpeedMin, std::memory_order_relaxed);
            if (last > kSpeedMin) {
                setter((float)kSpeedMin, method);          // 关（或调回 1）：还原 timeScale=1
                if (hit <= 3) LOGI(TPL_LOG " PROBE deltaTime restore timeScale=1 (was %d)", last);
            }
        }
    }

    if (hit <= 3) {
        LOGI(TPL_LOG " PROBE deltaTime hit=%u mult=%d pin=%d", hit, mult,
             (g_origSetTimeScale || g_setTimeScaleDirect) ? 1 : 0);
    }

    if (g_origDeltaTime) return g_origDeltaTime(method);
    return 0.0166667f;
}

// ---- 功能②：全局加速（辅助挂点） -------------------------------------------
// 对照方（斌哥 detour @0x7438AACA54）：ldr x0,[原函数桩]; br x0 —— 纯透传，不改进参。
// 我们同。挂它的意义：①与斌哥挂点集 1:1 ②为 deltaTime 槽提供 orig setter。
static void Hooked_set_timeScale(float value, void* method) {
    const uint32_t hit = g_hitSetTs.fetch_add(1, std::memory_order_relaxed) + 1;
    if (hit <= 3) LOGI(TPL_LOG " PROBE setTimeScale hit=%u value=%.3f", hit, value);
    if (g_origSetTimeScale) g_origSetTimeScale(value, method);
}

static const HookSlot kSlots[] = {
    // 0) GM 菜单 —— 仅当 GM 菜单 ON 才装
    {"gmCheat", kRvaCheatGetter, kInsnStrX19Sp20, "", "UISwitchPlayerMenu",
     "get_IsCheatModeEnabled", 0, (void*)Hooked_get_IsCheatModeEnabled,
     (void**)&g_origCheatGetter},
    // 1) 全局加速（施力点）
    {"deltaTime", kRvaDeltaTime, kInsnStrX19Sp20, "UnityEngine", "Time",
     "get_deltaTime", 0, (void*)Hooked_get_deltaTime, (void**)&g_origDeltaTime},
    // 2) 全局加速（辅助）
    {"setTimeScale", kRvaSetTimeScale, kInsnStrD8Sp20, "UnityEngine", "Time",
     "set_timeScale", 1, (void*)Hooked_set_timeScale, (void**)&g_origSetTimeScale},
};

// ============================================================================
//  ⑦ 分组安装 + 懒装（对应功能全关 = 0 hook，全关也不卸载）
// ============================================================================
static const int kSlotsCount = (int)(sizeof(kSlots) / sizeof(kSlots[0]));

static bool SlotNeeded(int i) {
    switch (i) {
        case 0:          return g_gmMenu.load(std::memory_order_relaxed);
        case 1: case 2:  return SpeedOn();
        default:         return false;
    }
}

static int NeededCount() {
    int n = 0;
    for (int i = 0; i < kSlotsCount; ++i) {
        if (SlotNeeded(i)) ++n;
    }
    return n;
}

static int InstallBattleHooks() {
    int ok = 0, dead = 0, skip = 0;
    for (int i = 0; i < kSlotsCount && i < kSlotsMax; ++i) {
        if (!SlotNeeded(i)) { ++skip; continue; }
        if (g_slotDead[i]) { ++dead; continue; }        // 熔断，跳过
        g_lastFatal = false;
        if (HookSlotInstall(kSlots[i])) {
            ++ok;
        } else if (g_lastFatal) {
            g_slotDead[i] = true;                       // 永久拉黑（本次会话）
            LOGW(TPL_LOG " slot[%d] %s 已熔断（装钩期致命信号），本次会话不再重试",
                 i, kSlots[i].tag);
        }
    }
    LOGI(TPL_LOG " install pass: ok=%d dead=%d skip=%d/%d (gm=%d mult=%d)",
         ok, dead, skip, kSlotsCount, (int)g_gmMenu.load(), g_speedMult.load());
    if (ok > 0) g_anyHooked.store(true);
    return ok;
}

static bool AnyFeatureOn() {
    return g_gmMenu.load() || SpeedOn();
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    if (!AnyFeatureOn()) return;                        // 全关：不装、也不卸载
    const int need = NeededCount();
    if (need == 0) return;
    if (g_installedCount.load() >= need) return;        // 快路径

    std::lock_guard<std::mutex> lk(g_installMutex);     // ★ 串行化，防并发双挂
    if (g_installedCount.load() >= need) return;        // double-check
    g_installedCount.store(InstallBattleHooks());
}

// hook-rules §31.3：功能开启后打印一次，一行判定本机形态
static void EmitSelfCheckOnce() {
    if (g_selfCheckDone.load()) return;
    if (!AnyFeatureOn()) return;
    if (g_selfCheckDone.exchange(true)) return;

    int groups = 0, page = 0;
    uintptr_t base = 0;
    MapsProbe(&groups, &base, &page);
    LOGI(TPL_LOG " SELFCHECK base=0x%lX rd=%d hook=%d interp=%d path=%s maps=%d page=%d verity=%s",
         (unsigned long)base, g_rdOk.load() ? 1 : 0, g_anyHooked.load() ? 1 : 0,
         0, "A", groups, page, "?");
}

// ============================================================================
//  ⑧ 对外接口（ModEntry.h 注册表契约，签名不可改）
// ============================================================================
bool Ggbh_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;

    InstallFaultGuard();          // §④ 先装兜底，再装钩
    g_libReady.store(true);
    SyncHooks();                  // 已 ON 的功能立即装；全关保持 0 hook
    LOGI(TPL_LOG " lib ready (FileOffToVa 双路径, lazy hooks, %d slots)", kSlotsCount);
    return true;
}

bool Ggbh_IsReady() {
    return g_libReady.load();
}

void Ggbh_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case GGBH_FEAT_GMMENU:
            g_gmMenu.store(enabled);
            break;
        case GGBH_FEAT_SPEED:
            // 滑块位语义：1 = 关（零改写）；打开时若无倍率则默认 2 倍
            if (enabled) {
                const int cur = g_speedMult.load();
                g_speedMult.store(cur > kSpeedMin ? cur : 2);
            } else {
                g_speedMult.store(kSpeedMin);
            }
            break;
        default: break;
    }
    LOGI(TPL_LOG " feature %d -> %d (gm=%d mult=%d)", featNum, (int)enabled,
         (int)g_gmMenu.load(), g_speedMult.load());
    SyncHooks();
    EmitSelfCheckOnce();
}

void Ggbh_OnValueChanged(int featNum, int value) {
    if (featNum != GGBH_FEAT_SPEED) return;
    int v = value;
    if (v < kSpeedMin) v = kSpeedMin;
    if (v > kSpeedMax) v = kSpeedMax;
    g_speedMult.store(v);
    LOGI(TPL_LOG " speed mult -> %d (on=%d)", v, (int)SpeedOn());
    SyncHooks();
    EmitSelfCheckOnce();
}

void Ggbh_Tick(const bool* features, const int* values) {
    if (!features) return;

    g_gmMenu.store(features[GGBH_FEAT_GMMENU]);
    if (values) {
        int v = values[GGBH_FEAT_SPEED];
        if (v < kSpeedMin) v = kSpeedMin;
        if (v > kSpeedMax) v = kSpeedMax;
        g_speedMult.store(v);
    }

    // 兜底：库晚于首帧映射时，这里补一次
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) {
        InstallFaultGuard();
        g_libReady.store(true);
        LOGI(TPL_LOG " lib ready (via Tick fallback)");
    }
    SyncHooks();
    EmitSelfCheckOnce();

    // 每 5 秒（25 × 200ms）在有 hit 增量时打一行探针摘要，供 L3 实测期间读 logcat
    static uint32_t lastGm = 0, lastDelta = 0, lastSetTs = 0;
    static int tickCnt = 0;
    if (++tickCnt >= 25) {
        tickCnt = 0;
        const uint32_t a = g_hitGm.load();
        const uint32_t b = g_hitDelta.load();
        const uint32_t c = g_hitSetTs.load();
        if (a != lastGm || b != lastDelta || c != lastSetTs) {
            lastGm = a; lastDelta = b; lastSetTs = c;
            LOGI(TPL_LOG " HITS gm=%u deltaTime=%u setTimeScale=%u (gm=%d mult=%d)",
                 a, b, c, (int)g_gmMenu.load(), g_speedMult.load());
        }
    }

    // ★ 禁止在此 SafeProbe / 扫堆 / 遍历对象 / 逐对象写（Tick 频率高，代价爆炸）
}

void Ggbh_ForceClear() {
    g_gmMenu.store(false);
    g_speedMult.store(kSpeedMin);
    g_reenter.store(false);
    // 注意：timeScale 的还原交给 deltaTime 槽（它永不卸载，每帧自检 g_pinnedMult > 1 即写回 1）。
    // 这里不直呼 setter：ForceClear 可能不在游戏线程上，跨线程调 il2cpp 方法风险大于收益。
}
