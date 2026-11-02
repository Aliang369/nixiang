// ============================================================================
//  星光菜单游戏模块 — 标准骨架 v3.1（注册表版，复制后按 TODO 填空）
//
//  生成：bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh {Game}
//        → 四件套 {Game}Mod.h/.cpp + {Game}Entry.cpp + Android_{tag}.mk
//        入口走 MainModern.cpp 注册表（不改 Main.cpp / GameConfig.h / 公共 Android.mk）
//
//  ★ 内置防闪退设施是「代码」不是「文档约定」，删任何一节都可能复现历史闪退：
//      默认 0 hook（懒装） / 双路径挂点 / RVA 漂移拒绝 / 幂等安装 /
//      orig 必调 / reenter 防递归 / SafeWrite 校验写 / 类名缓存 / Tick 轻量
//    v3.1（2026-10-02，疾风骑士 60.0.1 实战下沉）另加三件，见 §④：
//      并发装钩互斥 / 致命信号兜底（sigsetjmp 保护区 + sigaltstack） / 槽位熔断
//
//  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
//
//  功能实现分层（定稿 2026-09-09，详见 GAME_MOD_GUIDE.md）：
//    数据型（无敌=HP回写 / 属性增益 / 资源锁定）
//        → 偏移定位 + SafeWrite 直写，Tick 轮询维持（零 hook 痕迹，防封首选；
//          开关全关 = 零改写）
//    行为型（改判定 / 伤害结算 / 拦截上报）
//        → Dobby hook：RVA 优先（序言校验），RVA 失效自动转按名（本骨架 HookSlot 双路径）
//    字节 patch 改函数 → 默认禁用（E15–E18 死路清单）
// ============================================================================

#include "WdjnwcfMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <cstring>

#include <mutex>      // §④ 装钩互斥
#include <setjmp.h>   // §④ sigsetjmp 保护区
#include <signal.h>   // §④ 致命信号兜底

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

// 生成器把 [wdjnwcf] 替换为本游戏标签（如 [jfqs]），日志过滤用 adb logcat -s Mod_Menu
#define TPL_LOG "[wdjnwcf]"

// 注册表模式不依赖 GameConfig.h（legacy 配置中心）。
// Cocos 系生成后把下面这处（含各 hook 内引用）统一换成目标库名。
#define TPL_TARGET_LIB sec_lib_il2cpp()

// ============================================================================
//  ① 开关与 orig 指针
//     1:1 对照斌哥菜单（10_docs/_batch_binge6/六款斌哥改包_抓点总表_20261102.md §二.6）
//     全开关关闭 ⇒ 0 个 hook（AnyFeatureOn 为 false 时不装钩）
// ============================================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_reenter{false};

static std::atomic<int> g_invincible{0};    // Toggle_无敌            0/1
static std::atomic<int> g_dmgMult{1};       // InputValue_伤害倍率     1 = 关
static std::atomic<int> g_speed{1};         // InputValue_加速         1 = 关

// 按钮动作：UI 线程只置位，游戏线程（Tick）执行，避免跨线程碰 il2cpp
enum : int { kActNone = 0, kActGold = 1, kActCrystal = 2, kActPower = 3, kActIgnoreAds = 4 };
static std::atomic<int> g_pendingAction{kActNone};
static std::atomic<int> g_ignoreAds{0};   // 免广告目标状态（1 = 免广告）
static const int32_t kResourceAmount = 999999;   // 单击加成（斌哥菜单无数量项，固定值）

typedef float (*ChangeHp_t)(void* self, int32_t num, void* method);
static ChangeHp_t g_origChangeHp = nullptr;
typedef void (*SetTimeScale_t)(float value, void* method);
static SetTimeScale_t g_origSetTimeScale = nullptr;

// Unity 主线程泵：资源按钮必须在 Unity 主线程执行（非主线程直调会 SIGSEGV，
// fault addr 0xb8 —— 2026-11-02 23:35 三次按钮崩溃实证）
typedef void (*VoidSelf_t)(void* self, void* method);
static VoidSelf_t g_origRootUpdate = nullptr;

// ============================================================================
//  ② 字段偏移与 RVA
//     全部来自 06_dumped/我的技能无限触发/dump.cs（Offset == RVA），
//     绑定 libil2cpp.so md5 4406aaaf1545bde437375dcf4ad76d69（与斌哥包逐字节一致）
// ============================================================================
enum : size_t {
    kOffNowHp      = 0x40,   // BattleData.nowHp (float)
    kOffMonster    = 0x48,   // BattleData.tMonsterData
    kOffHero       = 0x58,   // BattleData.tHeroData
    kOffDataNative = 0x10,   // DataMgr.nativeData
};

static const uintptr_t kRvaChangeHp     = 0xDEB660;   // BattleData::changeHp(int) -> float
static const uint32_t  kInsnChangeHp    = 0xFC1D0FE8; // 序言首 4 字节
static const uintptr_t kRvaSetTimeScale = 0x1EC796C;  // UnityEngine.Time::set_timeScale(float)
static const uint32_t  kInsnSetTimeScale = 0xFC1E0FE8;
static const uintptr_t kRvaRootUpdate    = 0x115D364; // Root::Update()（MonoBehaviour 每帧，Unity 主线程）
static const uint32_t  kInsnRootUpdate   = 0xFC1D0FE8;

static const char* kNsUnity       = "UnityEngine";
static const char* kClsGame       = "Game";
static const char* kClsBattleData = "BattleData";
static const char* kClsTime       = "Time";
static const char* kFieldDataMgr  = "dataMgr";
static const char* kFieldEventMgr = "evnetMgr";   // ⚠ 游戏自带拼写错误（dump.cs 原文），不是笔误
static const char* kClsEventMgr   = "EventMgr";
static const int   kEvtPlayerResChange = 37;      // EEvent.playerResChange：资源变化 → 顶栏刷新

// ============================================================================
//  ③ 安全助手（防闪退核心；新代码禁止绕开它们裸读写内存）
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
//  ④ 并发 + 致命信号兜底 + 槽位熔断（v3.1 新增，来自疾风骑士 60.0.1 实战）
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
//    (3) hook 体内会执行游戏/第三方代码（例如免广告直接 Invoke 游戏发奖回调，
//        回调里可能读到尚未填充的字段 ⇒ null-deref）：
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
static void TplFaultHandler(int sig, siginfo_t* si, void* /*uc*/) {
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
    if (old->sa_sigaction && old->sa_sigaction != TplFaultHandler) {
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
        LOGW(TPL_LOG " sigaltstack failed — 兜底降级为无备用栈");
    }
}

// ---- 4.6 一次性安装进程级处理器 ----
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
    LOGI(TPL_LOG " fault guard armed (SIGSEGV/SIGBUS/SIGILL, altstack=%zu)",
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
            LOGW(TPL_LOG " %s dobby fail rc=%d", tag, rc);
            return false;
        }
        return true;
    }
    // longjmp 回来：保护区内的致命信号已被吞掉
    t_inGuard = 0;
    g_lastFatal = true;
    LOGW(TPL_LOG " %s dobby SIG(%d) @%p — 槽位熔断，游戏继续",
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
    LOGW(TPL_LOG " %s prologue read SIG(%d) @%p — 跳过该槽位",
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
    LOGW(TPL_LOG " %s 回调内 SIG(%d) @%p — 已吞掉，游戏继续",
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
    }
    if (!DobbyHookGuarded(tag, va, hook, orig)) {
        LOGW(TPL_LOG " %s dobby fail @rva 0x%lX", tag, (unsigned long)rva);
        return false;
    }
    LOGI(TPL_LOG " %s hooked @rva 0x%lX", tag, (unsigned long)rva);
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
    LOGI(TPL_LOG " %s hooked by-name %s.%s(argc=%d)", tag, cls, method, argc);
    return true;
}

struct HookSlot {
    const char* tag;
    uintptr_t   rva;          // 0 = 只走按名
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
//  ⑥ hook 函数（行为型：改判定，不是改字节）
// ============================================================================
static std::atomic<int> g_hpLogBudget{6};   // 无敌命中日志上限，防刷屏

static float Hook_ChangeHp(void* self, int32_t num, void* mi) {
    if (!g_origChangeHp) return 0.0f;
    if (g_reenter.load()) return g_origChangeHp(self, num, mi);   // 防递归
    g_reenter.store(true);

    int32_t applied = num;
    if (OkPtr(self)) {
        void* hero = *reinterpret_cast<void**>(reinterpret_cast<char*>(self) + kOffHero);
        void* mon  = *reinterpret_cast<void**>(reinterpret_cast<char*>(self) + kOffMonster);

        if (g_invincible.load() && OkPtr(hero) && applied < 0) {
            float hp = Il2Cpp::ReadF32(self, kOffNowHp);
            if (g_hpLogBudget.fetch_sub(1) > 0) {
                LOGI(TPL_LOG " 无敌 @rva 生效：hero hp=%.1f 拦截 %d", hp, applied);
            }
            g_reenter.store(false);
            return hp;   // 不改血量直接返回当前值
        }

        int mult = g_dmgMult.load();
        if (mult > 1 && OkPtr(mon) && applied < 0) {
            long long v = static_cast<long long>(applied) * mult;
            if (v < -2000000000LL) v = -2000000000LL;
            applied = static_cast<int32_t>(v);
        }
    }

    float r = g_origChangeHp(self, applied, mi);
    g_reenter.store(false);
    return r;
}

static void Hook_SetTimeScale(float value, void* mi) {
    if (!g_origSetTimeScale) return;
    int sp = g_speed.load();
    if (sp > 1 && value > 0.01f) value *= static_cast<float>(sp);   // 暂停(0)不放大
    g_origSetTimeScale(value, mi);
}

static void ServicePendingAction();   // 定义见 §⑧；资源按钮必须在主线程跑

// Unity 主线程泵：Root::Update 每帧调用 ⇒ 资源按钮到这里才算安全
static void Hook_RootUpdate(void* self, void* mi) {
    ServicePendingAction();
    if (g_origRootUpdate) g_origRootUpdate(self, mi);   // orig 必调
}

// ============================================================================
//  ⑦ 挂点表与分组安装（RVA 优先 → 序言漂移自动转按名）
// ============================================================================
static const int kSlotsCount = 2;

static int InstallBattleHooks() {
    int n = 0;

    if (HookAtRva("changeHp", kRvaChangeHp, kInsnChangeHp,
                  reinterpret_cast<void*>(Hook_ChangeHp),
                  reinterpret_cast<void**>(&g_origChangeHp)) ||
        HookByName("changeHp", "", kClsBattleData, "changeHp", 1,
                   reinterpret_cast<void*>(Hook_ChangeHp),
                   reinterpret_cast<void**>(&g_origChangeHp))) {
        ++n;
    }

    if (HookAtRva("setTimeScale", kRvaSetTimeScale, kInsnSetTimeScale,
                  reinterpret_cast<void*>(Hook_SetTimeScale),
                  reinterpret_cast<void**>(&g_origSetTimeScale)) ||
        HookByName("setTimeScale", kNsUnity, kClsTime, "set_timeScale", 1,
                   reinterpret_cast<void*>(Hook_SetTimeScale),
                   reinterpret_cast<void**>(&g_origSetTimeScale))) {
        ++n;
    }

    LOGI(TPL_LOG " install pass: %d/%d hooks", n, kSlotsCount);
    return n;
}

// 主线程泵常驻（与开关无关）：资源按钮队列要有人排空
static std::atomic<bool> g_pumpTried{false};

static void EnsurePump() {
    if (g_pumpTried.load() || !g_libReady.load()) return;
    std::lock_guard<std::mutex> lk(g_installMutex);
    if (g_pumpTried.load()) return;
    g_pumpTried.store(true);   // 只试一次，失败降级（按钮日志会提示未装）
    if (HookAtRva("rootUpdate", kRvaRootUpdate, kInsnRootUpdate,
                  reinterpret_cast<void*>(Hook_RootUpdate),
                  reinterpret_cast<void**>(&g_origRootUpdate)) ||
        HookByName("rootUpdate", "", "Root", "Update", 0,
                   reinterpret_cast<void*>(Hook_RootUpdate),
                   reinterpret_cast<void**>(&g_origRootUpdate))) {
        LOGI(TPL_LOG " 主线程泵已装（Root::Update）");
    } else {
        LOGW(TPL_LOG " 主线程泵未装 ⇒ 资源按钮不会执行");
    }
}

static bool AnyFeatureOn() {
    return g_invincible.load() != 0 || g_dmgMult.load() > 1 || g_speed.load() > 1;
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    if (!AnyFeatureOn()) return;                        // 全关：不装、也不卸载
    if (g_installedCount.load() >= kSlotsCount) return; // 快路径：已装齐

    std::lock_guard<std::mutex> lk(g_installMutex);     // ★ 串行化，防并发双挂
    if (g_installedCount.load() >= kSlotsCount) return; // double-check
    g_installedCount.store(InstallBattleHooks());
}

// ============================================================================
//  ⑧ 资源按钮：Game.dataMgr(静态) → DataMgr.nativeData(0x10) → changePlayer*(ObscuredInt 内部处理)
// ============================================================================
static void* GameDataMgrPtr() {
    if (!EnsureApi()) return nullptr;
    void* k = FindClassNs("", kClsGame);
    if (!k) k = GetApi().FindClassByShortName(kClsGame);
    if (!k) return nullptr;
    void* dm = GetApi().StaticRefField(k, kFieldDataMgr);
    return OkPtr(dm) ? dm : nullptr;
}

// 资源变化后刷新 UI（否则要等下一局结束才刷新，用户 2026-11-02 反馈
// 「一次不刷新数据我以为没效果，打了一局才发现数值变了」）
static void RefreshResUI() {
    void* ek = FindClassNs("", kClsEventMgr);
    if (!ek) ek = GetApi().FindClassByShortName(kClsEventMgr);
    if (!ek) return;
    void* em = GetApi().StaticRefField(ek, kFieldEventMgr);
    if (!OkPtr(em)) return;
    void* mi = GetApi().GetMethod(ek, "sendEvent", 2);
    if (!mi) return;
    void* fn = *reinterpret_cast<void**>(mi);
    if (!OkPtr(fn)) return;
    reinterpret_cast<void (*)(void*, int, void*, void*)>(fn)(em, kEvtPlayerResChange, nullptr, mi);
}

static bool CallNativeMethod(void* nd, const char* name, int argc, int32_t amount) {
    void* k = GetApi().object_class(nd);
    if (!k) return false;
    void* mi = GetApi().GetMethod(k, name, argc);
    if (!mi) {
        LOGW(TPL_LOG " %s(argc=%d) 未找到 — 资源按钮降级", name, argc);
        return false;
    }
    void* fn = *reinterpret_cast<void**>(mi);
    if (!OkPtr(fn)) return false;

    if (argc == 2) {
        reinterpret_cast<void (*)(void*, int32_t, bool, void*)>(fn)(nd, amount, true, mi);
    } else {
        reinterpret_cast<void (*)(void*, int32_t, void*)>(fn)(nd, amount, mi);
    }
    return true;
}

static void ServicePendingAction() {
    int act = g_pendingAction.exchange(kActNone);
    if (act == kActNone) return;

    void* dm = GameDataMgrPtr();
    if (!dm) {
        LOGW(TPL_LOG " 资源按钮：Game.dataMgr 未就绪（仍在登录/存档界面？）— 忽略本次");
        return;
    }
    void* nd = *reinterpret_cast<void**>(reinterpret_cast<char*>(dm) + kOffDataNative);
    if (!OkPtr(nd)) {
        LOGW(TPL_LOG " 资源按钮：DataMgr.nativeData 为空 — 忽略本次");
        return;
    }

    if (act == kActIgnoreAds) {
        void* k = GetApi().object_class(nd);
        void* mi = k ? GetApi().GetMethod(k, "SetPlayerIgnoreAds", 1) : nullptr;
        void* fn = mi ? *reinterpret_cast<void**>(mi) : nullptr;
        if (!OkPtr(fn)) {
            LOGW(TPL_LOG " SetPlayerIgnoreAds 未找到 — 免广告降级");
            return;
        }
        const bool want = g_ignoreAds.load() != 0;
        reinterpret_cast<void (*)(void*, bool, void*)>(fn)(nd, want, mi);
        LOGI(TPL_LOG " 免广告 SetPlayerIgnoreAds(%d) ok", want ? 1 : 0);
        return;   // 不涉及资源刷新
    }

    const char* m = (act == kActGold)    ? "changePlayerGold"
                  : (act == kActCrystal) ? "changePlayerCrystal"
                                         : "changePlayerPower";
    const int argc = (act == kActPower) ? 2 : 1;
    const bool ok = CallNativeMethod(nd, m, argc, kResourceAmount);
    if (ok) RefreshResUI();
    LOGI(TPL_LOG " 资源按钮 %s(+%d) %s（已发 playerResChange 刷新）", m, kResourceAmount, ok ? "ok" : "fail");
}

// ============================================================================
//  ⑨ 对外接口（ModEntry.h 注册表契约，签名不可改）
// ============================================================================
bool Wdjnwcf_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;

    InstallFaultGuard();          // §④ 先装兜底，再装钩
    g_libReady.store(true);
    EnsurePump();                 // 主线程泵常驻
    SyncHooks();                  // 已 ON 的开关立即装；全关保持 0 hook
    LOGI(TPL_LOG " lib ready (dual-path, lazy hooks)");
    return true;
}

bool Wdjnwcf_IsReady() {
    return g_libReady.load();
}

void Wdjnwcf_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case WDJNWCF_FEAT_INVINCIBLE: g_invincible.store(enabled ? 1 : 0); break;
        case WDJNWCF_FEAT_GOLD:       if (enabled) g_pendingAction.store(kActGold);    break;
        case WDJNWCF_FEAT_CRYSTAL:    if (enabled) g_pendingAction.store(kActCrystal); break;
        case WDJNWCF_FEAT_POWER:      if (enabled) g_pendingAction.store(kActPower);   break;
        case WDJNWCF_FEAT_NOAD:
            g_ignoreAds.store(enabled ? 1 : 0);
            g_pendingAction.store(kActIgnoreAds);   // 开/关都立即同步到游戏
            break;
        default: break;
    }
    SyncHooks();
}

void Wdjnwcf_OnValueChanged(int featNum, int value) {
    if (featNum == WDJNWCF_FEAT_DMG_MULT) {
        if (value < 1) value = 1;
        if (value > 100) value = 100;
        g_dmgMult.store(value);
    } else if (featNum == WDJNWCF_FEAT_SPEED) {
        if (value < 1) value = 1;
        if (value > 10) value = 10;
        g_speed.store(value);
    }
    SyncHooks();
}

void Wdjnwcf_OnInputChanged(int featNum, const char* text) {
    if (!text || !*text) return;
    long long v = std::strtoll(text, nullptr, 10);
    if (v < 1) return;
    if (v > 100) v = 100;
    Wdjnwcf_OnValueChanged(featNum, static_cast<int>(v));
}

void Wdjnwcf_Tick(const bool* features, const int* /*values*/) {
    // 兜底：库晚于首帧映射时，这里补一次
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) {
        InstallFaultGuard();
        g_libReady.store(true);
        EnsurePump();
        LOGI(TPL_LOG " lib ready (via Tick fallback)");
    }

    // ★ 资源按钮不在这里执行：Tick 跑在菜单线程，直调 il2cpp 会 SIGSEGV。
    //   队列由 Hook_RootUpdate（Unity 主线程每帧）排空。
    SyncHooks();

    (void)features;
    // ★ 禁止在此 SafeProbe / 扫堆 / 遍历对象 / 逐对象写（Tick 频率高，代价爆炸）
}

void Wdjnwcf_ForceClear() {
    g_reenter.store(false);
    g_invincible.store(0);
    g_dmgMult.store(1);
    g_speed.store(1);
    g_pendingAction.store(kActNone);
    g_ignoreAds.store(0);
}
