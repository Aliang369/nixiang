// ============================================================================
//  星光菜单游戏模块 — 黎明前20分钟（com.nanostudios.games.twenty.minutes 1.0.106）
//
//  抓点来源（本轮实测复核，2026-11-04）：
//    10_docs/_batch_binge6/三款新版菜单_抓点总表_20261103.md §四.3（斌哥 4 桩）
//    10_docs/_batch_binge6/三款原版基线_与斌哥包差异_20261103.md §三.3（il2cpp 零改动）
//    Il2CppDumper(v27.1) 解析 1.0.106 的 global-metadata + libil2cpp.so：
//      flanne.Health::HPChange(int, EnumDamage)              RVA 0x21E9CCC 序言 F81C0FF7
//      UnityEngine.Time::set_timeScale(float)                RVA 0x0DC0A2C 序言 FC1E0FE8
//      （get_time 0x0DC08C0 / get_deltaTime 0x0DC08F4 本轮不挂，理由见下）
//
//  ★ 与斌哥 4 桩的差异（本轮定案，路线锁定.md §3 有对照）：
//    斌哥：get_time + get_deltaTime + set_timeScale 三桩并联。三桩同时乘算会 **speed²**
//          叠加（timeScale 已让 native 侧 deltaTime 变快，再乘一次即双算）。
//    本轮：只挂 set_timeScale 一桩 —— 它同时作用于托管与 native（协程 WaitForSeconds、
//          物理），且天然保留游戏自己的暂停语义（value<=0 原样透传，不强制）。
//          游戏若整局只设一次 timeScale 后再不调用，Tick 兜底补一次（见 ApplySpeedIfIdle）。
//    另：get_time 乘算会破坏「绝对时间」语义（存档时间戳 / 埋点上报），不挂更稳。
//
//  防闪退设施沿用标准骨架 v3.1（懒装 / 双路径挂点 / RVA 漂移拒绝 / 幂等安装 /
//  orig 必调 / reenter 防递归 / 并发装钩互斥 / 致命信号兜底 / 槽位熔断）。
// ============================================================================

#include "Lmq20fzMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <ctime>

#include <mutex>      // 装钩互斥
#include <setjmp.h>   // sigsetjmp 保护区
#include <signal.h>   // 致命信号兜底

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define LMQ_LOG "[Lmq20fz]"

// 目标库：IL2CPP 运行时（框架内统一用 SecStrings 的混淆串，避免明文库名）
#define LMQ_TARGET_LIB sec_lib_il2cpp()

// ============================================================================
//  ① 开关与 orig 指针
// ============================================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_anyHooked{false};

static std::atomic<bool> g_inv{false};      // 无敌
static std::atomic<int>  g_dmgMul{1};       // 百倍伤害倍率（1 = 关）
static std::atomic<int>  g_speed{1};        // 战斗加速倍率（1 = 关）
static std::atomic<bool> g_noad{false};     // 免广告（激励视频不播、直发奖）

static std::atomic<bool> g_reenter{false};

// 统计（Tick 里按变化打日志，热路径不打印）
static std::atomic<uint32_t> g_blocked{0};
static std::atomic<uint32_t> g_scaled{0};
static std::atomic<uint32_t> g_spdApplied{0};
static std::atomic<uint32_t> g_adSkipped{0};
static std::atomic<uint32_t> g_adFallback{0};
static std::atomic<uint32_t> g_adReady{0};
static std::atomic<uint32_t> g_couponGet{0};
static std::atomic<uint32_t> g_couponSet{0};

// 最近一次 set_timeScale 的时间戳（Tick 兜底判据：游戏长期不设时我们自己补）
static std::atomic<uint64_t> g_lastTsSetMs{0};
static std::atomic<uint32_t> g_tsSeen{0};
static std::atomic<int>      g_appliedSpd{0};  // 已写入的倍率（0 = 尚未写过）

typedef void (*HPChange_t)(void* self, int32_t change, int32_t damageType, void* method);
static HPChange_t g_origHPChange = nullptr;

typedef void (*SetTimeScale_t)(float value, void* method);
static SetTimeScale_t g_origSetTimeScale = nullptr;

// AnyThinkAdsManager.ShowRewardAd(Action<bool> call, string triggerType)
typedef void (*ShowRewardAd_t)(void* self, void* call, void* triggerType, void* method);
static ShowRewardAd_t g_origShowRewardAd = nullptr;

// AnyThinkAdsManager.get_RewardAdIsReady()
typedef bool (*RewardAdIsReady_t)(void* self, void* method);
static RewardAdIsReady_t g_origRewardAdIsReady = nullptr;

// 广告卷（CouponsType.AdFree=2）计数：SaveSystem 静态属性
// 路子来源：用户 2026-11-04「游戏有一个叫广告卷的，他可以通过广告卷进行免广」
typedef int32_t (*GetAdFreeCoupons_t)(void* method);
typedef void    (*SetAdFreeCoupons_t)(int32_t value, void* method);
static GetAdFreeCoupons_t g_origGetAdFreeCoupons = nullptr;
static SetAdFreeCoupons_t g_origSetAdFreeCoupons = nullptr;

// ============================================================================
//  ② 字段偏移与常量（全部来自本轮 dump.cs 实测，换包必须重 dump）
// ============================================================================
enum : size_t {
    // flanne.Health 实例字段：CreatureType 在 0x18（EnumCreature：0=Player 1=普通怪 2=Boss）
    kOffCreatureType = 0x18,
};
enum : int32_t {
    kCreaturePlayer = 0,
    kDmgMulMax      = 100,
    kSpeedMax       = 5,
};
enum : uint32_t {
    kInsnHPChange      = 0xF81C0FF7u,   // str x23, [sp, #-0x40]!
    kInsnSetTimeScale  = 0xFC1E0FE8u,   // str d8,  [sp, #-0x20]!
    kInsnShowRewardAd  = 0xA9BD57F6u,   // stp x22, x21, [sp, #-0x30]!
    kInsnRewardReady   = 0xF9401C00u,   // ldr x0, [x0, #0x38]
    kInsnGetAdCoupon   = 0xF81E0FF3u,   // str x30, [sp, #-0x20]!
    kInsnSetAdCoupon   = 0xA9BE4FF4u,   // stp x20, x19, [sp, #-0x20]!
};
enum : uint64_t {
    kSpeedIdleMs = 3000,   // 3s 没见到 set_timeScale ⇒ 视为游戏不再设，Tick 兜底补一次
};

static uint64_t NowMs() {
    struct timespec ts;
    if (clock_gettime(CLOCK_MONOTONIC, &ts) != 0) return 0;
    return static_cast<uint64_t>(ts.tv_sec) * 1000ULL + static_cast<uint64_t>(ts.tv_nsec / 1000000L);
}

// ============================================================================
//  ③ 安全助手
// ============================================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

static int32_t SafeReadI32(void* obj, size_t off, int32_t defv) {
    if (!OkPtr(obj)) return defv;
    return Il2Cpp::ReadI32(obj, off);
}

static int ClampInt(int v, int lo, int hi) {
    if (v < lo) return lo;
    if (v > hi) return hi;
    return v;
}

// ============================================================================
//  ④ 并发 + 致命信号兜底 + 槽位熔断（标准骨架 v3.1，逐字沿用，勿删）
// ============================================================================
static std::mutex g_installMutex;
static std::mutex g_guardMutex;

static thread_local sigjmp_buf            t_faultJmp;
static thread_local volatile sig_atomic_t t_inGuard    = 0;
static thread_local volatile sig_atomic_t t_faultDepth = 0;
static thread_local volatile int          t_faultSig   = 0;
static thread_local volatile void*        t_faultAddr  = nullptr;

static thread_local char t_altStack[65536];
static thread_local bool t_altStackSet = false;

static struct sigaction g_oldSegv;
static struct sigaction g_oldBus;
static struct sigaction g_oldIll;
static std::atomic<bool> g_guardArmed{false};
static bool g_lastFatal = false;

static void LmqFaultHandler(int sig, siginfo_t* si, void* /*uc*/) {
    if (t_inGuard) {
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
    if (old->sa_sigaction && old->sa_sigaction != LmqFaultHandler) {
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
        LOGW(LMQ_LOG " sigaltstack failed — 兜底降级为无备用栈");
    }
}

static void InstallFaultGuard() {
    if (g_guardArmed.load()) return;
    std::lock_guard<std::mutex> lk(g_guardMutex);
    if (g_guardArmed.load()) return;

    struct sigaction sa;
    std::memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = LmqFaultHandler;
    sa.sa_flags     = SA_SIGINFO | SA_ONSTACK | SA_NODEFER;
    sigemptyset(&sa.sa_mask);

    sigaction(SIGSEGV, &sa, &g_oldSegv);
    sigaction(SIGBUS,  &sa, &g_oldBus);
    sigaction(SIGILL,  &sa, &g_oldIll);

    g_guardArmed.store(true);
    LOGI(LMQ_LOG " fault guard armed (SIGSEGV/SIGBUS/SIGILL, altstack=%zu)", sizeof(t_altStack));
}

static bool DobbyHookGuarded(const char* tag, void* target, void* hook, void** orig) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        int rc = DobbyHook(target, hook, orig);
        t_inGuard = 0;
        if (rc != 0) {
            LOGW(LMQ_LOG " %s dobby fail rc=%d", tag, rc);
            return false;
        }
        return true;
    }
    t_inGuard = 0;
    g_lastFatal = true;
    LOGW(LMQ_LOG " %s dobby SIG(%d) @%p — 槽位熔断，游戏继续", tag, (int)t_faultSig, (void*)t_faultAddr);
    if (orig) *orig = nullptr;
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
    LOGW(LMQ_LOG " %s prologue read SIG(%d) @%p — 跳过该槽位", tag, (int)t_faultSig, (void*)t_faultAddr);
    return false;
}

// 跑第三方/原生 setter：炸了吞掉，只记日志（Tick 兜底与 ForceClear 用）
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
    LOGW(LMQ_LOG " %s 回调内 SIG(%d) @%p — 已吞掉，游戏继续",
         tag, (int)t_faultSig, (void*)t_faultAddr);
    return false;
}

static const int kSlotsMax = 64;
static bool g_slotDead[kSlotsMax];
static std::atomic<int> g_installedCount{0};

// ============================================================================
//  ⑤ 双路径挂点机器（RVA 优先 + 按名兜底）
// ============================================================================
static bool IsLibMapLine(const char* line, const char* lib) {
    return std::strstr(line, lib) != nullptr && std::strstr(line, ".so") != nullptr;
}

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
        if (!std::strchr(perm, 'x')) continue;
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

static bool HookAtRva(const char* tag, uintptr_t rva, uint32_t expectInsn,
                      void* hook, void** orig) {
    void* va = FileOffToVa(LMQ_TARGET_LIB, rva);
    if (!va) {
        LOGW(LMQ_LOG " %s rva 0x%lX not in RX — try by-name", tag, (unsigned long)rva);
        return false;
    }
    if (expectInsn) {
        uint32_t got = 0;
        if (!ReadU32Guarded(tag, va, &got)) return false;
        if (got == 0 || got == 0xFFFFFFFFu || got != expectInsn) {
            LOGW(LMQ_LOG " %s rva drift got=0x%08X want=0x%08X — try by-name",
                 tag, got, expectInsn);
            return false;
        }
    }
    if (!DobbyHookGuarded(tag, va, hook, orig)) {
        LOGW(LMQ_LOG " %s dobby fail @rva 0x%lX", tag, (unsigned long)rva);
        return false;
    }
    LOGI(LMQ_LOG " %s hooked @rva 0x%lX", tag, (unsigned long)rva);
    return true;
}

static bool EnsureApi() {
    if (GetApi().ready) return true;
    return GetApi().Load(LMQ_TARGET_LIB);
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

static bool HookByName(const char* tag, const char* ns, const char* cls,
                       const char* method, int argc, void* hook, void** orig) {
    if (!EnsureApi()) {
        LOGW(LMQ_LOG " %s api not ready — will retry via Tick", tag);
        return false;
    }

    void* k = FindClassNs(ns, cls);
    if (!k) k = GetApi().FindClassByShortName(cls);
    if (!k) {
        LOGW(LMQ_LOG " %s class miss %s — degrade", tag, cls);
        return false;
    }

    void* mi = GetApi().GetMethod(k, method, argc);
    if (!mi) {
        LOGW(LMQ_LOG " %s method miss %s.%s(argc=%d) — degrade, install others",
             tag, cls, method, argc);
        return false;
    }

    void* fn = *reinterpret_cast<void**>(mi);
    if (!OkPtr(fn)) {
        LOGW(LMQ_LOG " %s bad method ptr %s.%s", tag, cls, method);
        return false;
    }

    if (!DobbyHookGuarded(tag, fn, hook, orig)) return false;
    LOGI(LMQ_LOG " %s hooked by-name %s.%s(argc=%d)", tag, cls, method, argc);
    return true;
}

struct HookSlot {
    const char* tag;
    uintptr_t   rva;
    uint32_t    expectInsn;
    const char* ns;
    const char* cls;
    const char* method;
    int         argc;
    void*       hookFn;
    void**      origPtr;
};

static bool HookSlotInstall(const HookSlot& s) {
    if (s.origPtr && *s.origPtr) return true;
    if (s.rva && HookAtRva(s.tag, s.rva, s.expectInsn, s.hookFn, s.origPtr)) return true;
    return HookByName(s.tag, s.ns, s.cls, s.method, s.argc, s.hookFn, s.origPtr);
}

// ============================================================================
//  ⑥ hook 函数
// ============================================================================

//  flanne.Health::HPChange(int change, EnumDamage damageType) —— 实例方法
//    · 无敌：目标是自己（CreatureType==Player）且 change<0 ⇒ 吞掉，不调 orig
//    · 百倍伤害：目标是怪（CreatureType>0）且 change<0 ⇒ change × 倍率
//  EnumDamage 只表达伤害来源/属性（Cure/OnPlayer/OnMonster/Fire…），不表目标，
//  故目标一律按实例字段 CreatureType 判定（dump.cs 实测 0x18）。
static void Hooked_HPChange(void* self, int32_t change, int32_t damageType, void* method) {
    if (!g_origHPChange) return;

    if (change < 0 && !g_reenter.load()) {
        g_reenter.store(true);
        const int32_t creature = SafeReadI32(self, kOffCreatureType, -1);
        if (creature == kCreaturePlayer) {
            if (g_inv.load()) {
                g_blocked.fetch_add(1);
                g_reenter.store(false);
                g_origHPChange(self, 0, damageType, method);   // 传 0：既不扣血也不触发死亡判定
                return;
            }
        } else if (creature > kCreaturePlayer) {
            const int mul = g_dmgMul.load();
            if (mul > 1) {
                int64_t v = static_cast<int64_t>(change) * mul;
                if (v < -2147483647LL) v = -2147483647LL;
                change = static_cast<int32_t>(v);
                g_scaled.fetch_add(1);
            }
        }
        g_reenter.store(false);
    }

    g_origHPChange(self, change, damageType, method);
}

//  UnityEngine.Time::set_timeScale(float) —— 静态方法（末参 MethodInfo*）
//  value<=0 = 游戏主动暂停/定格，原样透传（不改暂停语义）；value>0 乘算倍率。
static void Hooked_SetTimeScale(float value, void* method) {
    const int spd = g_speed.load();
    if (spd > 1 && value > 0.0f) {
        value = value * static_cast<float>(spd);
        g_spdApplied.fetch_add(1);
    }
    g_lastTsSetMs.store(NowMs());
    g_tsSeen.fetch_add(1);
    if (g_origSetTimeScale) g_origSetTimeScale(value, method);
}

//  ---------------------------------------------------------------
//  免广告：AnyThinkAdsManager.ShowRewardAd(Action<bool>, string)
//  拦截后直接以 true 调用回调 ⇒ 不播广告但按「看完」发奖；
//  AnyThink 不加载/不播，任何看广告解锁、看广告复活、看广告加倍都走这里。
//  ---------------------------------------------------------------
//  Il2CppDelegate 布局（metadata v27）：0x10 method_ptr / 0x18 invoke_impl /
//  0x20 target / 0x28 method(MethodInfo*)。**不能**用 runtime_invoke 调 Action<bool>.Invoke：
//  Invoke 是运行时分派桩，methodPointer 为空 ⇒ 调用即 SIGSEGV@0x0（2026-11-04 实测）。
//  正解：取 delegate 里绑定的真实方法（0x28）与其 target（0x20）再 runtime_invoke。
struct InvokeBoolArg { void* del; bool val; bool ok; };

static void InvokeBoolCall(void* p) {
    InvokeBoolArg* a = static_cast<InvokeBoolArg*>(p);
    if (!a || !a->del) return;
    // ★本作 libil2cpp 的 il2cpp_runtime_invoke 解析不到（实测 diag 打 runtime_invoke=0x0），
    //   ⇒ 完全不用托管 API：直接按 IL2CPP 原生 ABI 调 delegate 绑定的方法。
    //   Il2CppDelegate：0x10 method_ptr（代码） / 0x20 target / 0x28 method(MethodInfo*)。
    //   实例方法 ABI＝(this, args…, method)；静态目标（target==null）＝(args…, method)。
    char* del = static_cast<char*>(a->del);
    void* mptr   = *reinterpret_cast<void**>(del + 0x10);
    void* target = *reinterpret_cast<void**>(del + 0x20);
    void* mi     = *reinterpret_cast<void**>(del + 0x28);
    if (!mptr) return;
    if (target) {
        reinterpret_cast<void (*)(void*, bool, void*)>(mptr)(target, a->val, mi);
    } else {
        reinterpret_cast<void (*)(bool, void*)>(mptr)(a->val, mi);
    }
    a->ok = true;
}

static void Hooked_ShowRewardAd(void* self, void* call, void* triggerType, void* method) {
    if (g_noad.load() && call) {
        static bool diagLogged = false;
        if (!diagLogged) {
            diagLogged = true;
            Il2Cpp::Api& api = GetApi();
            char* d = static_cast<char*>(call);
            void* mptr = *reinterpret_cast<void**>(d + 0x10);
            void* tgt  = *reinterpret_cast<void**>(d + 0x20);
            void* rm   = *reinterpret_cast<void**>(d + 0x28);
            const char* mn = (api.method_get_name && rm) ? api.method_get_name(rm) : "?";
            LOGI(LMQ_LOG " ad-callback diag: runtime_invoke=%p mptr=%p target=%p method=%p name=%s",
                 (void*)api.runtime_invoke, mptr, tgt, rm, mn ? mn : "?");
        }
        g_adSkipped.fetch_add(1);
        InvokeBoolArg arg{call, true, false};
        // 托管回调必须走保护区；失败（含信号）则退回真实广告，绝不吞掉玩家奖励
        if (RunGuarded("ad-callback", InvokeBoolCall, &arg) && arg.ok) return;
        g_adFallback.fetch_add(1);
        LOGW(LMQ_LOG " ad-callback 未成功（ok=%d）→ 退回真实广告", (int)arg.ok);
    }
    if (g_origShowRewardAd) g_origShowRewardAd(self, call, triggerType, method);
}

static bool Hooked_RewardAdIsReady(void* self, void* method) {
    if (g_noad.load()) {
        g_adReady.fetch_add(1);
        return true;    // 让依赖「是否就绪」的按钮保持可点（广告根本没加载也点得动）
    }
    if (g_origRewardAdIsReady) return g_origRewardAdIsReady(self, method);
    return false;
}

// 免广告券（第二层）：让 AdFree 券恒有货、且永不减少 ⇒ 广告弹窗里的「用广告卷」永远可用，
// 点它由游戏自己的逻辑发奖（不播广告、也不依赖上面的 delegate 直调）。
static int32_t Hooked_GetAdFreeCoupons(void* method) {
    if (g_noad.load()) {
        g_couponGet.fetch_add(1);
        return 99;
    }
    if (g_origGetAdFreeCoupons) return g_origGetAdFreeCoupons(method);
    return 0;
}

static void Hooked_SetAdFreeCoupons(int32_t value, void* method) {
    if (g_noad.load()) {
        g_couponSet.fetch_add(1);
        return;   // 吞掉扣减：券子数量不会被真实消耗（也就不会写坏存档）
    }
    if (g_origSetAdFreeCoupons) g_origSetAdFreeCoupons(value, method);
}

// TODO 无：rva/expectInsn 已按 1.0.106 实测回填，失效自动转按名
static const HookSlot kSlots[] = {
    // HPChange：无敌 + 百倍伤害共用一个槽（同一结算口，按目标分流）
    {"HPChange",     0x21E9CCC, kInsnHPChange,     "flanne",     "Health", "HPChange",     2,
     (void*)Hooked_HPChange, (void**)&g_origHPChange},
    // 战斗加速：一桩覆盖托管 + native（见文件头「与斌哥 4 桩的差异」）
    {"set_timeScale", 0xDC0A2C, kInsnSetTimeScale, "UnityEngine", "Time",  "set_timeScale", 1,
     (void*)Hooked_SetTimeScale, (void**)&g_origSetTimeScale},
    // 免广告（激励视频）：拦截播放，直接回调成功
    {"ShowRewardAd",   0x1E9E008, kInsnShowRewardAd, "AnyThinkAdsManager", "AnyThinkAdsManager",
     "ShowRewardAd", 2, (void*)Hooked_ShowRewardAd, (void**)&g_origShowRewardAd},
    {"RewardAdIsReady", 0x1E9DFF4, kInsnRewardReady, "AnyThinkAdsManager", "AnyThinkAdsManager",
     "get_RewardAdIsReady", 0, (void*)Hooked_RewardAdIsReady, (void**)&g_origRewardAdIsReady},
    // 广告卷（SaveSystem.AdFreeCouponsCount）：恒 99 + 扣减吞掉
    {"GetAdFreeCoupons", 0x1E08B8C, kInsnGetAdCoupon, "", "SaveSystem",
     "get_AdFreeCouponsCount", 0, (void*)Hooked_GetAdFreeCoupons, (void**)&g_origGetAdFreeCoupons},
    {"SetAdFreeCoupons", 0x1E08BF4, kInsnSetAdCoupon, "", "SaveSystem",
     "set_AdFreeCouponsCount", 1, (void*)Hooked_SetAdFreeCoupons, (void**)&g_origSetAdFreeCoupons},
};

// ============================================================================
//  ⑦ 分组安装 + 懒装（全开关关 = 0 hook）
// ============================================================================
static const int kSlotsCount = (int)(sizeof(kSlots) / sizeof(kSlots[0]));

static int InstallHooks() {
    int ok = 0, dead = 0;
    for (int i = 0; i < kSlotsCount && i < kSlotsMax; ++i) {
        if (g_slotDead[i]) { ++dead; continue; }
        g_lastFatal = false;
        if (HookSlotInstall(kSlots[i])) {
            ++ok;
        } else if (g_lastFatal) {
            g_slotDead[i] = true;
            LOGW(LMQ_LOG " slot[%d] %s 已熔断（装钩期致命信号），本次会话不再重试",
                 i, kSlots[i].tag);
        }
    }
    LOGI(LMQ_LOG " install pass: ok=%d dead=%d/%d", ok, dead, kSlotsCount);
    if (ok > 0) g_anyHooked.store(true);
    return ok;
}

static bool AnyFeatureOn() {
    return g_inv.load() || g_noad.load() || g_dmgMul.load() > 1 || g_speed.load() > 1;
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    if (!AnyFeatureOn()) return;
    if (g_installedCount.load() >= kSlotsCount) return;

    std::lock_guard<std::mutex> lk(g_installMutex);
    if (g_installedCount.load() >= kSlotsCount) return;
    g_installedCount.store(InstallHooks());
}

static void ApplySpeedCall(void* arg) {
    const int spd = (int)(intptr_t)arg;
    if (g_origSetTimeScale) g_origSetTimeScale(static_cast<float>(spd), nullptr);
}

// 写入一次倍率（含还原成 1）。apply 会跑游戏侧代码 ⇒ 走保护区。
static void ApplySpeed(int spd) {
    g_lastTsSetMs.store(NowMs());
    RunGuarded("speed-apply", ApplySpeedCall, (void*)(intptr_t)spd);
    g_appliedSpd.store(spd);
    g_spdApplied.fetch_add(1);
    static int lastLogged = -1;
    if (lastLogged != spd) {   // 同倍率只打一次：3s 一次刷屏会把日志缓冲冲掉
        lastLogged = spd;
        LOGI(LMQ_LOG " timeScale applied = %d (via orig)", spd);
    }
}

// 游戏整局不调用 set_timeScale 时，由 Tick 兜底写一次；倍率变了才重写（含关掉时还原 1）。
static void ApplySpeedIfIdle() {
    if (!g_origSetTimeScale) return;
    const int spd = g_speed.load();
    if (g_appliedSpd.load() == spd) return;              // 已经是这个倍率，不重复写
    const uint64_t now = NowMs();
    const uint64_t last = g_lastTsSetMs.load();
    if (spd > 1 && last != 0 && now - last < kSpeedIdleMs) return;  // 游戏刚设过，让它先
    ApplySpeed(spd);
}

// ============================================================================
//  ⑧ 对外接口（ModEntry.h 注册表契约，签名不可改）
// ============================================================================
bool Lmq20fz_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(LMQ_TARGET_LIB)) return false;

    InstallFaultGuard();
    g_libReady.store(true);
    SyncHooks();
    LOGI(LMQ_LOG " lib ready (dual-path, lazy hooks)");
    return true;
}

bool Lmq20fz_IsReady() {
    return g_libReady.load();
}

void Lmq20fz_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case LMQ20FZ_FEAT_INV: g_inv.store(enabled); break;
        case LMQ20FZ_FEAT_NOAD: g_noad.store(enabled); break;
        default: break;
    }
    SyncHooks();
}

void Lmq20fz_OnValueChanged(int featNum, int value) {
    switch (featNum) {
        case LMQ20FZ_FEAT_DMG:
            g_dmgMul.store(ClampInt(value, 1, kDmgMulMax));
            LOGI(LMQ_LOG " dmgMul=%d", g_dmgMul.load());
            break;
        case LMQ20FZ_FEAT_SPD:
            g_speed.store(ClampInt(value, 1, kSpeedMax));
            g_lastTsSetMs.store(0);   // 立刻允许兜底补一次（含 spd=1 时还原）
            LOGI(LMQ_LOG " speed=%d", g_speed.load());
            break;
        default:
            break;
    }
    SyncHooks();
}

void Lmq20fz_Tick(const bool* features, const int* values) {
    if (!features || !values) return;

    g_inv.store(features[LMQ20FZ_FEAT_INV]);
    g_noad.store(features[LMQ20FZ_FEAT_NOAD]);
    const int dmg = ClampInt(values[LMQ20FZ_FEAT_DMG], 1, kDmgMulMax);
    const int spd = ClampInt(values[LMQ20FZ_FEAT_SPD], 1, kSpeedMax);
    g_dmgMul.store(dmg > 1 ? dmg : 1);
    g_speed.store(spd > 1 ? spd : 1);

    if (!g_libReady.load() && isLibraryLoaded(LMQ_TARGET_LIB)) {
        InstallFaultGuard();
        g_libReady.store(true);
        LOGI(LMQ_LOG " lib ready (via Tick fallback)");
    }
    SyncHooks();
    ApplySpeedIfIdle();

    // 统计只在变化时打（热路径零日志）
    static uint32_t lastBlocked = 0, lastScaled = 0, lastAd = 0, lastFb = 0;
    const uint32_t b = g_blocked.load(), s = g_scaled.load();
    const uint32_t a = g_adSkipped.load(), f = g_adFallback.load();
    if (b != lastBlocked || s != lastScaled || a != lastAd || f != lastFb) {
        lastBlocked = b; lastScaled = s; lastAd = a; lastFb = f;
        LOGI(LMQ_LOG " stats blocked=%u scaled=%u adSkipped=%u adFallback=%u couponGet=%u couponSet=%u",
             b, s, a, f, g_couponGet.load(), g_couponSet.load());
    }
}

void Lmq20fz_ForceClear() {
    g_inv.store(false);
    g_noad.store(false);
    g_dmgMul.store(1);
    g_speed.store(1);
    g_reenter.store(false);
    // 授权失效：把 timeScale 还原成 1（唯一会残留到游戏侧的副作用）
    if (g_origSetTimeScale) RunGuarded("force-clear", ApplySpeedCall, (void*)(intptr_t)1);
    g_appliedSpd.store(1);
}
