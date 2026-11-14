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

#include "旷野征途Mod.h"
#include "旷野征途BattlePure.h"
#include "旷野征途BattleFlow.h"

#include <atomic>
#include <chrono>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <cstdarg>    // DbgLog 变参
#include <cstdio>     // DbgLog 文件落盘

#include <mutex>      // §④ 装钩互斥
#include <setjmp.h>   // §④ sigsetjmp 保护区
#include <signal.h>   // §④ 致命信号兜底

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"

// ---------------------------------------------------------------------------
//  资源三项 2.2 适配层（实现由 login-review 独占：Games/旷野征途Economy22.h）
//
//  设计（2026-11-14 第2轮）：
//   · 头文件采用**硬断言**，不做「__has_include 悄悄漏编还宣称完成」：
//     一旦构建声明要含该能力（KYZT_ECONOMY22_EXPECTED=1），缺头文件或版本标记即**编译失败**。
//   · 未声明时保持现有失败关闭（2.2 禁 raw 旧偏移）。
//   · 调用线程：只允许从菜单开关回调（Java UI 线程）进入；mod_thread 的 Tick 里禁止调
//     （thread_local 标记 t_inModTick），避免从后台线程碰 Unity/il2cpp 对象。
// ---------------------------------------------------------------------------
#if defined(KYZT_ECONOMY22_EXPECTED) && KYZT_ECONOMY22_EXPECTED
#  if !defined(__has_include)
#    error "需要 __has_include 支持"
#  endif
#  if !__has_include("旷野征途Economy22.h")
#    error "声明了 KYZT_ECONOMY22_EXPECTED 但缺 Games/旷野征途Economy22.h"
#  endif
   // Il2CppApi.h 的默认参数用 sec_lib_il2cpp()，必须先引入 SecStrings
#  include "Includes/SecStrings.hpp"
#  include "旷野征途Economy22.h"
#  ifndef KYZT_ECONOMY22_API_VERSION
#    error "旷野征途Economy22.h 缺版本标记 KYZT_ECONOMY22_API_VERSION"
#  endif
static_assert(KYZT_ECONOMY22_API_VERSION == 6, "Economy22 API 版本不匹配（期望 6 / KYZT22ECO-BUILD-6，2026-11-14 v6：before/after 读回 + bool=false→10 不重试）");
#  define KYZT_HAS_ECONOMY22 1
#else
#  define KYZT_HAS_ECONOMY22 0
#endif
static thread_local bool t_inModTick = false;   // Tick 线程标记（禁止在 Tick 内调经济接口）

// ---------------------------------------------------------------------------
//  资源调用线程模型（2026-11-14 第3轮，按 lead 要求重做）
//
//  事实：框架内没有 Unity 线程派发器（grep UnityMain/runOnGameThread 命中 0）；
//        Android 主线程 ≠ Unity 主线程（Unity 游戏循环通常独占 UnityMain）。
//  结论：菜单回调只做「入队」，真正调用只发生在**游戏自己的帧回调**里：
//        锚点 = ET.GameManagerMono.Update（AOT，dump.cs RVA 0x2AF53DC / file 0x2AF13DC，
//        与相邻方法 RVA 各不相同、函数体 712 B ⇒ 非空实现、非解释器共用桩；全仓唯一）。
//  线程实证：Hooked_SetTimeScale 一定跑在 Unity 线程（游戏自己调 Time.set_timeScale），
//        在那里记 pthread_self()，与菜单回调线程比对后落盘，供 login-review 决定是否
//        允许直调（KYZT22_THREAD_PROVEN）。
// ---------------------------------------------------------------------------
static void DbgLog(const char* fmt, ...);   // 前向声明（定义在本文件后段）
static std::atomic<unsigned long> g_unityTid{0};
static std::atomic<int>  g_tidProbe{0};        // 0=未测 1=相同 2=不同 3=无参照

// 队列与线程判定全部由 Games/旷野征途Economy22.h（login-review 独占）实现：
//   GrantX() 只在 UI 侧入队并立即返回 0（不碰托管对象）；
//   OnUnityFrame() 由本模块在 AOT 帧回调里每帧调用，内部记录 pthread_self 并排空。
// 本文件不再持有第二条队列，避免双缓冲/双 drain 造成重复发奖。

// 帧锚点 hook：ET.GameManagerMono.Update（AOT，按名安装；orig 必调）
typedef void (*GameUpdate_t)(void* self, void* method);
static GameUpdate_t g_origGameUpdate = nullptr;
static std::atomic<int> g_updCalls{0};

static void Hooked_GameUpdate(void* self, void* method) {
    const int n = g_updCalls.fetch_add(1) + 1;
#if KYZT_HAS_ECONOMY22
    // 隔离：h 内部若抛 C++ 异常，不得吞掉游戏自己的 Update。
    // 注意：本隔离只兜 C++ 异常；托管异常由 runtime_invoke 的 exc 出参处理，
    // SIGSEGV 由已装的保护区/信号兜底，**不能**靠 catch 捕。
    try {
        Kyzt22::log_fn() = DbgLog;    // 诊断钩子（2026-11-14 21:50）
        Kyzt22::OnUnityFrame();      // h 内部：记录 Unity 线程 + 有界排空（每帧上限 4）
    } catch (...) {
        DbgLog("eco: OnUnityFrame threw (C++), orig 继续");
    }
    if ((n % 300) == 1) {
        const unsigned pend = Kyzt22::PendingCount();
        if (n == 1) DbgLog("eco: h=%s", Kyzt22::BuildMarker());
        if (pend || n == 1)
            DbgLog("eco: frame#%d tid=%lu pending=%u drained=%u last=%d",
                   n, (unsigned long)pthread_self(), pend, Kyzt22::DrainedCount(), Kyzt22::LastRc());
    }
#else
    if ((n % 600) == 1) DbgLog("update: frame hook #%d tid=%lu (Economy22.h 未编译)", n, (unsigned long)pthread_self());
#endif
    if (g_origGameUpdate) g_origGameUpdate(self, method);
}
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

// 生成器把 [KYZT] 替换为本游戏标签（如 [jfqs]），日志过滤用 adb logcat -s Mod_Menu
#define TPL_LOG "[KYZT]"

// 注册表模式不依赖 GameConfig.h（legacy 配置中心）。
// Cocos 系生成后把下面这处（含各 hook 内引用）统一换成目标库名。
#define TPL_TARGET_LIB sec_lib_il2cpp()

// ============================================================================
//  ① 开关与 orig 指针
//     g_libReady = 「库已映射」≠「已挂钩」；全开关关闭时应当保持 0 个 hook。
// ============================================================================
// 文件调试日志（定义在 §⑤-c）：logcat 环形缓冲仅 256KB，关键状态必须落盘
static void DbgLog(const char* fmt, ...);

static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_dmg{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_spd{false};
static std::atomic<bool> g_noad{false};   // 免广告
static std::atomic<bool> g_reenter{false};

typedef void (*SetTimeScale_t)(float value, void* method);
static SetTimeScale_t g_origSetTimeScale = nullptr;
static std::atomic<void*> g_tsMethod{nullptr};   // 缓存 MethodInfo*，Tick 强制调用时要用
static std::atomic<int>   g_tsCalls{0};          // 诊断：set_timeScale 被游戏调用了几次

// ============================================================================
//  ② 字段偏移与 RVA
//     TODO：全部来自 dump.cs / IDA / Trace 实测，禁止猜；换包必须重 dump。
// ============================================================================
enum : size_t {
    kOffHurtValue = 0x30,   // 示例：伤害字段偏移（本游戏实测后改）

    // —— 战斗功能实测偏移（1.8 / HotUpdate.dll，il2cpp_field_get_offset）
    //    取证：10_docs/旷野征途/移植记录_星空版_1.8_20261109.md §9，禁止猜改
    kOffRoleGroup    = 964,    // <group>k__BackingField Int32：玩家=0，怪=1
    kOffRoleCurHp    = 1124,   // _curHp Int32
    kOffRoleDead     = 968,    // _isDead Boolean
    kOffDmgShield    = 48,     // BattleDamageResult.absorbedShield Single
    kOffDmgIncoming  = 52,     // .incomingDamage Single
    kOffDmgDmg       = 56,     // .dmg Single
    kOffDmgReal      = 60,     // .reallyDmg Int32
};

// ============================================================================
//  ②-b 版本自适应挂点地址（2026-11-14 起支持 1.8 / 2.2 双版本同一 so）
//
//  背景：2.2/vc22 底包（sha256 8cb087ee…e023）把 Dirichlet 广告类整体位移，
//  ShowInternal file 由 0x1C8A9AC → 0x1CA9130（实测该处首指令仍为 0xF81E0FFE
//  = `str x30,[sp,#-0x20]!`，与 1.8 序言一致，故同一 expectInsn 可校验）。
//
//  铁律（路线锁定 §7）：
//   · 只认「实测 + 序言校验」的地址；查不到就置 0 走失败关闭，**不许填伪地址**。
//   · HybridCLR 解释器入口（EnterFrameFromInterpreter）在 2.2 **尚未定位**
//     （1.8 的 0x19876a8 已随 so 变化失效，65,520,080 B，签名扫描 0 命中）
//     ⇒ 2.2 下 rva=0 且无按名路径 ⇒ 战斗两项自动失败关闭并落盘告警。
// ============================================================================
// 前向声明：②-b 段在 ⑤ 挂点机器之前，需先声明这两个底层助手
static void* FileOffToVa(const char* lib, uintptr_t fileOff);
static bool  ReadU32Guarded(const char* tag, const void* p, uint32_t* out);

static uintptr_t kRvaShowInternal = 0x1C8A9AC;   // 1.8 实测（file）
static uintptr_t kRvaHandleNative = 0x1C8A724;   // 1.8 实测（file，只调不挂）
static uintptr_t kRvaEnterInterp  = 0x19876a8;   // 1.8 实测（file）
static std::atomic<int> g_libVer{0};             // 0=未知 / 18=1.8 / 22=2.2

// 探针：用「首指令是否等于已知序言」判版本，不依赖包名/版本号字符串
static int DetectLibVersion() {
    struct Probe { int ver; uintptr_t off; };
    static const Probe probes[] = {{22, 0x1CA9130u}, {18, 0x1C8A9ACu}};
    for (const auto& pr : probes) {
        void* va = FileOffToVa(TPL_TARGET_LIB, pr.off);
        if (!va) continue;
        uint32_t insn = 0;
        if (ReadU32Guarded("verprobe", va, &insn) && insn == 0xF81E0FFEu) {
            g_libVer.store(pr.ver);
            if (pr.ver == 22) {
                kRvaShowInternal = 0x1CA9130u;
                kRvaHandleNative = 0x1CA8EA8u;
                kRvaEnterInterp  = 0u;         // 2.2 未定位 ⇒ 失败关闭，禁伪地址
                LOGI(TPL_LOG " lib version probe: 2.2 (ShowInternal file=0x1CA9130, interp=未定位)");
                DbgLog("verprobe: 2.2 showInternal=0x1CA9130 interp=UNRESOLVED");
            } else {
                LOGI(TPL_LOG " lib version probe: 1.8 (ShowInternal file=0x1C8A9AC)");
                DbgLog("verprobe: 1.8");
            }
            return pr.ver;
        }
    }
    LOGW(TPL_LOG " lib version probe: 未命中已知序言（未知版本）⇒ 仅按名路径");
    DbgLog("verprobe: UNKNOWN patch-ready=no");
    return 0;
}

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
        DbgLog("hook %s: FileOffToVa(0x%lX) -> null", tag, (unsigned long)rva);
        return false;
    }
    if (expectInsn) {
        uint32_t got = 0;
        if (!ReadU32Guarded(tag, va, &got)) return false;   // 读序言也走保护区
        if (got == 0 || got == 0xFFFFFFFFu || got != expectInsn) {
            LOGW(TPL_LOG " %s rva drift got=0x%08X want=0x%08X — try by-name",
                 tag, got, expectInsn);
            DbgLog("hook %s: rva drift at va=%p got=0x%08X want=0x%08X", tag, va, got, expectInsn);
            return false;
        }
    }
    if (!DobbyHookGuarded(tag, va, hook, orig)) {
        LOGW(TPL_LOG " %s dobby fail @rva 0x%lX", tag, (unsigned long)rva);
        DbgLog("hook %s: DobbyHook fail va=%p", tag, va);
        return false;
    }
    LOGI(TPL_LOG " %s hooked @rva 0x%lX", tag, (unsigned long)rva);
    DbgLog("hook %s: OK va=%p orig=%p", tag, va, orig ? *orig : nullptr);
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
    // 2026-11-14：MethodInfo::methodPointer(offset 0) 对**解释器方法**可能是
    // 0x400000000000 这类标记值（先例 WorMod.cpp:25-26 / Il2CppApi.h:190），
    // 直接 DobbyHook 即致命。判据只有一条：IsExecutableRange 为 **false** 时拒绝；
    // 合法可执行指针（含 AOT 方法）一律放行，不做任何「命中即拒绝」的额外拦阻。
    if (!Il2Cpp::IsExecutableRange(reinterpret_cast<uintptr_t>(fn), 16)) {
        LOGW(TPL_LOG " %s methodPointer 不在可执行段（解释器/标记值?）%s.%s → 拒绝 hook",
             tag, cls, method);
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
//  ⑤-c 战斗功能：HybridCLR 解释器入口 hook（无敌 / 百倍伤害）
//     关键结论（取证见移植记录 §9）：
//       ApplyHp/AddHp 属于 HotUpdate.dll 的解释方法，methodPointer 全部指向同一
//       解释器桩 ⇒ libil2cpp.so 里没有它们的函数体，Dobby 挂不到方法本身。
//       唯一可挂的 native 点是 InterpFrameGroup::EnterFrameFromInterpreter：
//         VA 0x198b6a8 / file 0x19876a8 / 序言 str x30,[sp,#-0x30]! = 0xF81D0FFE
//       签名 (frameGroup, methodInfo, argsPkt)；argsPkt 槽位实测：
//         +0 = this(受害者)  +8 = arg0(BattleRoleActor)  +16 = arg1(Int32 标志)
//         +24 = arg2(BattleDamageResult)  +32 = arg3(Action 回调)
//     实测：ApplyHp 是唯一落血点（AddHp 前后 _curHp 不变），玩家 group=0 / 怪 group=1。
// ============================================================================
typedef void* (*EnterInterp_t)(void* self, void* methodInfo, void* argsPkt);
static EnterInterp_t g_origEnterInterp = nullptr;
static std::atomic<void*> g_miApplyHp{nullptr};
static std::atomic<int>   g_hpHits{0};

// 文件调试日志：logcat main 环形缓冲仅 256KB，Unity 洪水会把 [KYZT] 整段冲掉，
// 关键状态一律落盘，收尾用 adb pull 取，不再依赖 logcat 存活时间。
static void DbgLog(const char* fmt, ...) {
    FILE* f = std::fopen("/data/data/com.xf.kyzt/files/kyzt_dbg.log", "a");
    if (!f) return;
    va_list ap; va_start(ap, fmt);
    std::vfprintf(f, fmt, ap);
    va_end(ap);
    std::fputc(10, f);
    std::fclose(f);
}

static std::atomic<int> g_miTries{0};
static std::atomic<int> g_tickSeq{0};

// 解析 BattleRoleActor.ApplyHp：热更 dll 加载 + HybridCLR 注册后才可见，Tick 里节流重试
// ⚠ FindClassByShortName 对热更类会退化到全 image 扫描（贵），绝不能每 Tick 无节流跑
static void ResolveCombatMi() {
    if (g_miApplyHp.load()) return;
    if (g_miTries.load() > 40) return;
    auto& api = GetApi();
    if (!api.ready) { if (!api.Load(TPL_TARGET_LIB)) return; }
    void* k = api.FindClassByShortName("BattleRoleActor");
    if (!k) { DbgLog("resolve BRA -> null (try %d)", g_miTries.load()); return; }
    void* mi = api.GetMethod(k, "ApplyHp", 4);
    if (!mi) { DbgLog("resolve ApplyHp -> null (klass=%p)", k); return; }
    g_miApplyHp.store(mi);
    LOGI(TPL_LOG " combat: BattleRoleActor.ApplyHp mi=%p klass=%p", mi, k);
    DbgLog("combat: ApplyHp mi=%p klass=%p", mi, k);
}

// ⚠ 关键：绝不能用全局 g_reenter 做去重。ApplyHp 绝大多数是被别的解释方法
//   （技能/AI/伤害流程）内部调用的，那时外层方法已进过本 hook，全局标志会把内层的
//   ApplyHp 整条挡掉 ⇒ 钩装上了却不生效（2026-11-09 实测踩坑）。
//   只挡 ApplyHp 自身递归，用线程局部标记。
static thread_local bool t_inApplyHp = false;

static void* Hooked_EnterInterp(void* self, void* methodInfo, void* argsPkt) {
    void* hpMi = g_miApplyHp.load();
    if (!hpMi || methodInfo != hpMi || !argsPkt || t_inApplyHp) {
        return g_origEnterInterp ? g_origEnterInterp(self, methodInfo, argsPkt) : nullptr;
    }
    t_inApplyHp = true;

    void* victim = *(void**)argsPkt;
    void* res = (void*)(*(uintptr_t*)((uintptr_t)argsPkt + 24));
    const bool ok = OkPtr(victim);
    const int32_t grp = ok ? SafeReadI32(victim, kOffRoleGroup, -1) : -1;
    const bool isPlayer = (grp == 0);          // 实测：玩家 group=0，怪 group=1
    const int32_t before = ok ? SafeReadI32(victim, kOffRoleCurHp, 0) : 0;
    const bool inv = g_inv.load(), dmg = g_dmg.load();
    if (g_hpHits.load() < 12) DbgLog("HIT applyhp g=%d hp=%d inv=%d dmg=%d res=%p", grp, before, (int)inv, (int)dmg, res);

    // 主路径：入口改写 BattleDamageResult，让游戏自己结算（死亡判定/UI 都正常）
    if (ok && OkPtr(res)) {
        if (isPlayer && inv) {
            SafeWriteI32(res, kOffDmgReal, 0);
            SafeWriteF32(res, kOffDmgIncoming, 0.0f);
            SafeWriteF32(res, kOffDmgDmg, 0.0f);
            SafeWriteF32(res, kOffDmgShield, 0.0f);
        } else if (!isPlayer && dmg) {
            int32_t real = SafeReadI32(res, kOffDmgReal, 0);
            SafeWriteI32(res, kOffDmgReal, real * 100);
            SafeWriteF32(res, kOffDmgDmg, Il2Cpp::ReadF32(res, kOffDmgDmg) * 100.0f);
            SafeWriteF32(res, kOffDmgIncoming, Il2Cpp::ReadF32(res, kOffDmgIncoming) * 100.0f);
        }
    }

    void* r = g_origEnterInterp ? g_origEnterInterp(self, methodInfo, argsPkt) : nullptr;

    // 兜底：出口校正 _curHp（若游戏内部没采用 result 里的值，靠这一步照样生效）
    if (ok) {
        const int32_t after = SafeReadI32(victim, kOffRoleCurHp, before);
        const int n = g_hpHits.fetch_add(1);
        if (isPlayer && inv && after < before) {
            SafeWriteI32(victim, kOffRoleCurHp, before);
            if (n < 20) LOGI(TPL_LOG " inv: hp %d -> %d 回滚(%d) v=%p", before, after, before, victim);
        } else if (!isPlayer && dmg && after < before) {
            const int32_t d = before - after;
            int32_t want = before - d * 100;
            if (want < 1) want = 1;            // 避免卡 0 血不死
            SafeWriteI32(victim, kOffRoleCurHp, want);
            if (n < 20) LOGI(TPL_LOG " dmg100: hp %d -> %d 放大(%d) v=%p", before, after, want, victim);
        } else if (n < 8) {
            LOGI(TPL_LOG " hp-pass: g=%d hp %d -> %d v=%p", grp, before, after, victim);
        }
    }
    t_inApplyHp = false;
    return r;
}

// ============================================================================
//  ⑤-d 免广告：Dirichlet Ad 聚合 SDK（AOT 程序集，可 Dobby 直挂）
//     链路取证（06_dumped/旷野征途/dump.cs → Dirichlet.Ad.Runtime.dll）：
//       ET.TapTapAds.ShowRewardAsync → DirichletRewardVideoAd → DirichletRewardAdBase
//       事件入口 DirichletRewardAdBase::HandleNativeEvent(DirichletNativeEvent)
//         struct { string EventName@0; string AdType@8; DirichletNativeEventData* Data@0x10 }
//         Data: RewardVerify@0x10 / RewardAmount@0x14 / RewardName@0x18 / Code@0x20 / Message@0x28
//         事件名常量：show / click / close / reward / skip
//     做法：拦 ShowInternal（真播广告的唯一出口）→ 不播，直接伪造 reward + close 事件，
//           让游戏自己的奖励链路照常跑完（死亡判定/UI/发奖逻辑都不用重写）。
//     RVA：ShowInternal 0x1C8E9AC(file 0x1C8A9AC) / HandleNativeEvent 0x1C8E724(file 0x1C8A724)
// ============================================================================
typedef bool (*ShowInternal_t)(void* self, void* options);
typedef void* (*StrNew_t)(const char*);
typedef void* (*ObjNew_t)(void*);

static ShowInternal_t g_origShowInternal = nullptr;
static void* g_pfnHandleNativeEvent = nullptr;
static void* g_adEventDataKlass = nullptr;
static StrNew_t g_strNew = nullptr;
static ObjNew_t g_objNew = nullptr;
static std::atomic<int> g_noAdHits{0};

// 取 image（按程序集名），热更/ AOT 都适用
static void* FindImageByName(const char* want) {
    auto& api = GetApi();
    size_t n = 0;
    void** asms = api.Assemblies(&n);
    if (!asms || !want) return nullptr;
    typedef const char* (*ImgName_t)(const void*);
    static ImgName_t fn = nullptr;
    if (!fn) fn = (ImgName_t)Il2Cpp::ResolveExport("il2cpp_image_get_name");
    if (!fn) return nullptr;
    for (size_t i = 0; i < n; ++i) {
        void* img = asms[i] ? api.ImageOf(asms[i]) : nullptr;
        if (!img) continue;
        const char* nm = fn(img);
        if (nm && std::strcmp(nm, want) == 0) return img;
    }
    return nullptr;
}

static void ResolveAdGlobals() {
    auto& api = GetApi();
    if (!api.ready) { if (!api.Load(TPL_TARGET_LIB)) return; }
    if (!g_strNew) g_strNew = (StrNew_t)Il2Cpp::ResolveExport("il2cpp_string_new");
    if (!g_objNew) g_objNew = (ObjNew_t)Il2Cpp::ResolveExport("il2cpp_object_new");
    if (!g_pfnHandleNativeEvent) {
        g_pfnHandleNativeEvent = FileOffToVa(TPL_TARGET_LIB, kRvaHandleNative);
        DbgLog("noAd: HandleNativeEvent -> %p", g_pfnHandleNativeEvent);
    }
    if (!g_adEventDataKlass) {
        void* img = FindImageByName("Dirichlet.Ad.Runtime.dll");
        if (img) g_adEventDataKlass = api.ClassFromName(img, "Dirichlet.Ad", "DirichletNativeEventData");
        DbgLog("noAd: image=%p klass=%p", img, g_adEventDataKlass);
    }
}

static void EmitDirichletEvent(void* adObj, const char* evName, bool verified, int amount) {
    if (!g_pfnHandleNativeEvent || !adObj) return;
    void* sName = g_strNew ? g_strNew(evName) : nullptr;
    void* sType = g_strNew ? g_strNew("rewardVideo") : nullptr;
    void* data = nullptr;
    if (g_adEventDataKlass && g_objNew) {
        data = g_objNew(g_adEventDataKlass);
        if (data) {
            *(uint8_t*)((uintptr_t)data + 0x10) = verified ? 1 : 0;
            *(int32_t*)((uintptr_t)data + 0x14) = amount;
            *(void**)((uintptr_t)data + 0x18) = nullptr;
            *(int32_t*)((uintptr_t)data + 0x20) = 0;
            *(void**)((uintptr_t)data + 0x28) = nullptr;
        }
    }
    void* evt[3] = { sName, sType, data };
    ((void(*)(void*, void*))g_pfnHandleNativeEvent)(adObj, evt);
}

static bool Hooked_ShowInternal(void* self, void* options) {
    if (!g_noad.load() || !self)
        return g_origShowInternal ? g_origShowInternal(self, options) : false;
    const int n = g_noAdHits.fetch_add(1);
    EmitDirichletEvent(self, "reward", true, 1);
    EmitDirichletEvent(self, "close", true, 0);
    if (n < 10) DbgLog("noAd: fake reward+close #%d ad=%p hn=%p", n + 1, self, g_pfnHandleNativeEvent);
    return true;   // 假装展示成功，不播真广告
}

// ============================================================================
//  ⑥ hook 函数与挂点表
//     铁律：一个具体方法一个槽；绝不 hook 共享 invoke 入口。
// ============================================================================
static void Hooked_SetTimeScale(float value, void* method) {
    g_tsMethod.store(method);
    if (g_unityTid.load() == 0) {
        g_unityTid.store((unsigned long)pthread_self());
        DbgLog("tid: unity=%lu (from set_timeScale)", (unsigned long)pthread_self());
    }
    const int n = g_tsCalls.fetch_add(1);
    if (n < 8) LOGI(TPL_LOG " set_timeScale call#%d arg=%.3f spd=%d", n + 1, value, (int)g_spd.load());
    const float v = g_spd.load() ? 1.8f : value;
    if (g_origSetTimeScale) g_origSetTimeScale(v, method);
}

// TODO：rva/expectInsn 由 dump.cs / IDA 实测后回填（0 = 仅按名）
static HookSlot kSlots[] = {
    {"TimeScale", 0x0, 0x0, "UnityEngine", "Time", "set_timeScale",
     1, (void*)Hooked_SetTimeScale, (void**)&g_origSetTimeScale},
    // HybridCLR 解释器入口：无按名路径，纯 RVA + 序言校验（漂移即拒绝，不盲挂）
    // 帧锚点：ET.GameManagerMono.Update（AOT / 私有 / argc=0），按名安装，无 RVA
    {"GameUpdate", 0x0, 0x0u, "ET", "GameManagerMono", "Update", 0,
     (void*)Hooked_GameUpdate, (void**)&g_origGameUpdate},
    {"EnterInterp", 0x0, 0xF81D0FFEu, "", "", "", -1,   // rva 由 InitSlotAddrs() 按版本填；2.2 未定位＝0 ⇒ 失败关闭
     (void*)Hooked_EnterInterp, (void**)&g_origEnterInterp},
    // 免广告：DirichletRewardAdBase::ShowInternal（AOT，序言 str x30,[sp,#-0x20]!）
    {"NoAd", 0x0, 0xF81E0FFEu, "", "", "", -1,  // rva 由 InitSlotAddrs() 按版本填
     (void*)Hooked_ShowInternal, (void**)&g_origShowInternal},
};

// ============================================================================
//  ⑦ 分组安装 + 懒装（全开关关 = 0 hook）
// ============================================================================
static const int kSlotsCount = (int)(sizeof(kSlots) / sizeof(kSlots[0]));

// 把版本探测结果写进挂点表（幂等；探测失败则全部保持 0 ⇒ 仅按名或失败关闭）
static void InitSlotAddrs() {
    const int ver = (g_libVer.load() == 0) ? DetectLibVersion() : g_libVer.load();
    // 未知版本（ver==0）没有指纹依据 ⇒ 所有 RVA 归零，只保留按名路径；禁止默认套 1.8 地址。
    const uintptr_t interp = (ver == 18) ? 0x19876a8u : ((ver == 22) ? 0u : 0u);
    const uintptr_t noad   = (ver == 18) ? 0x1C8A9ACu : ((ver == 22) ? 0x1CA9130u : 0u);
    kRvaEnterInterp = interp;
    kRvaShowInternal = noad;
    kRvaHandleNative = (ver == 18) ? 0x1C8A724u : ((ver == 22) ? 0x1CA8EA8u : 0u);
    for (auto& sl : kSlots) {
        if (std::strcmp(sl.tag, "EnterInterp") == 0) sl.rva = interp;
        else if (std::strcmp(sl.tag, "NoAd") == 0)   sl.rva = noad;
    }
    DbgLog("slot addrs: interp=0x%lX noad=0x%lX ver=%d",
           (unsigned long)kRvaEnterInterp, (unsigned long)kRvaShowInternal, g_libVer.load());
}

// ============================================================================
//  ④-b 战斗（2.2）：EnterFrame 入口按 MethodInfo 精确匹配 ApplyHp(argc=4)
//  已证：EnterFrame=0x19a9004 (InterpFrameGroup*, MethodInfo*, StackObject*)->InterpFrame*
//        ApplyHp 槽序 +0 this / +8 atker / +16 atkSeatNum / +24 dmgInfo / +32 OnDie
//        伤害为负（IL add→set_curHp）；Mathf.Max 只用于 reallyDmg 上报
//  行为：无敌=本机目标且 d<0 → d=0；百倍=本机输出且非本机目标且 d<0 → d*100（float 边界 clamp）
//  orig 无条件调用；不读 WasApplied；不用 isGod；不碰审计/特权/检测。
#ifndef KYZT_BATTLE_ENABLE
#define KYZT_BATTLE_ENABLE 1
#endif

#if KYZT_BATTLE_ENABLE
namespace kb {
  // 合同常量（与 IL 交叉表一一对应，改动必须同步 check_battle_contract.py）
  //   method_get_param 是 0 基且**不含 this**：0=atker 1=atkSeatNum 2=dmgInfo 3=OnDie
  //   argBase 槽位是另一套编号：+0=this +8=atker +16=atkSeatNum +24=dmgInfo +32=OnDie
  constexpr int kIdxAtker = 0;
  constexpr int kIdxSeat  = 1;
  constexpr int kIdxDmg   = 2;
  constexpr int kIdxOnDie = 3;
  constexpr int kSlotThis = 0;
  constexpr int kSlotAtker= 8;
  constexpr int kSlotDmg  = 24;
  static_assert(kIdxAtker == 0 && kIdxSeat == 1 && kIdxDmg == 2 && kIdxOnDie == 3,
                "param 索引必须 0 基且不含 this（atker=0/seat=1/dmg=2/onDie=3）");
  static_assert(kSlotThis == 0 && kSlotAtker == 8 && kSlotDmg == 24,
                "argBase 槽位必须 +0 this / +8 atker / +24 dmgInfo");
  static_assert(kSlotDmg != kIdxDmg, "槽位编号与 param 索引是两套编号，禁止混用");
  typedef void  (*fget_t)(void*, void*, void*);
  typedef void  (*fset_t)(void*, void*, void*);
  static fget_t p_fget = nullptr;
  static fset_t p_fset = nullptr;
  static void*  f_dmg = nullptr;
  static void*  f_player = nullptr;
  static void*  f_inst = nullptr;
  static void*  clsDmg = nullptr;
  static void*  clsRole = nullptr;
  static void*  g_origEnter = nullptr;
  typedef void* (*mget_class_t)(void*);
  typedef void* (*mget_param_t)(void*, uint32_t);
  typedef void* (*class_from_type_t)(void*);
  static mget_class_t      p_mgetClass = nullptr;
  static mget_param_t      p_mgetParam = nullptr;
  static class_from_type_t p_classFromType = nullptr;
  static thread_local bool t_inHook = false;
  // RAII 再入 guard：覆盖 Init 与所有 il2cpp 查询；**不覆盖 orig 执行**（orig 在 scope 外唯一调用）
  struct ReentryGuard {
      bool prev;
      ReentryGuard() : prev(t_inHook) { t_inHook = true; }
      ~ReentryGuard() { t_inHook = prev; }
  };
  // 菜单开关（6=无敌 / 7=百倍），由 OnFeatureChanged 写入；未开启时 hook 命中也不改值
  static std::atomic<bool> g_bGod{false};
  static std::atomic<bool> g_bX100{false};
  static std::atomic<bool> g_ready{false};
  static std::mutex        g_initMtx;
  static long long g_lastInitMs = 0;


  static bool Init() {
      if (g_ready.load(std::memory_order_acquire)) return true;
      std::lock_guard<std::mutex> lk(g_initMtx);
      // 失败可重试：首个 EnterFrame 可能早于 GetApi().Load() 完成，不能一次失败就永久关闭。
      // 用单调时钟节流（500ms），不做次数上限 —— 启动慢也能恢复，且不会每帧解析导出。
      const long long nowMs = std::chrono::duration_cast<std::chrono::milliseconds>(
          std::chrono::steady_clock::now().time_since_epoch()).count();
      if (g_lastInitMs != 0 && (nowMs - g_lastInitMs) < 500) return false;
      g_lastInitMs = nowMs;
      auto& api = GetApi();
      if (!api.ready) return false;
      p_mgetClass    = (mget_class_t)Il2Cpp::ResolveExport("il2cpp_method_get_class");
      p_mgetParam    = (mget_param_t)Il2Cpp::ResolveExport("il2cpp_method_get_param");
      p_classFromType= (class_from_type_t)Il2Cpp::ResolveExport("il2cpp_class_from_type");
      p_fget = (fget_t)Il2Cpp::ResolveExport("il2cpp_field_get_value");
      p_fset = (fset_t)Il2Cpp::ResolveExport("il2cpp_field_set_value");
      if (!p_fget || !p_fset || !api.class_get_field || !api.field_static_get || !api.object_class
          || !p_mgetClass || !p_mgetParam || !p_classFromType) {
          DbgLog("battle: init fail (api)"); return false;
      }
      clsDmg  = api.FindClassByShortName("BattleDamageResult");
      clsRole = api.FindClassByShortName("BattleRoleActor");
      void* mgr = api.FindClassByShortName("BattleRoleMgr");
      if (!clsDmg || !clsRole || !mgr) { DbgLog("battle: init fail (class)"); return false; }
      f_dmg  = api.class_get_field(clsDmg, "dmg");
      // BattleRoleMgr.player 是自动属性，真实字段为私有 <player>k__BackingField（il.txt:95407，
      // get_player(il.txt:95447) 就是 ldfld 它）；字面 "player" 属于另一个类 BattleMgr。
      f_player = api.class_get_field(mgr, "<player>k__BackingField");
      if (!f_player) { f_player = api.class_get_field(mgr, "player"); }
      f_inst = api.class_get_field(mgr, "Instance");
      if (!f_dmg || !f_player || !f_inst) { DbgLog("battle: init fail (fields)"); return false; }
      g_ready.store(true, std::memory_order_release);
      DbgLog("battle: init ok");
#if KYZT_HAS_ECONOMY22
      // 2026-11-14 22:05：探针从帧回调迁到 init（帧回调只在进战斗后才跑，拿不到存档结构）
      Kyzt22::log_fn() = DbgLog;
      Kyzt22::probe_once(api);
#endif
      return true;
  }

  static void* LocalPlayer() {
      void* mgr = nullptr;
      GetApi().field_static_get(f_inst, &mgr);
      if (!mgr) return nullptr;
      void* pl = nullptr;
      p_fget(mgr, f_player, &pl);
      return pl;
  }
}
namespace kb { static void* Hooked_EnterFrame(void* self, void* mi, void* argBase); }
namespace kb {
  static void* Hooked_EnterFrame(void* self, void* mi, void* argBase) {
      typedef void* (*enter_t)(void*, void*, void*);
      enter_t orig = (enter_t)g_origEnter;
      auto callOrig = [&]() -> void* { return orig ? orig(self, mi, argBase) : nullptr; };
      auto work = [&]() {
          if (!mi || !argBase) return;
          auto& api = GetApi();
          const char* nm = api.method_get_name ? api.method_get_name(mi) : nullptr;
          uint8_t argc = api.method_param_count ? api.method_param_count(mi) : 0;
          bool match = (nm && std::strcmp(nm, "ApplyHp") == 0 && argc == 4
                        && p_mgetClass && p_mgetClass(mi) == clsRole);
          if (match && p_mgetParam && p_classFromType) {
              void* tA = p_mgetParam(mi, kIdxAtker);
              void* tD = p_mgetParam(mi, kIdxDmg);
              void* cA = tA ? p_classFromType(tA) : nullptr;
              void* cD = tD ? p_classFromType(tD) : nullptr;
              match = (cA == clsRole && cD == clsDmg);
          } else {
              match = false;
          }
          if (!match) return;
          void* pThis = *(void**)((char*)argBase + kSlotThis);
          void* pAtk  = *(void**)((char*)argBase + kSlotAtker);
          void* pDmg  = *(void**)((char*)argBase + kSlotDmg);
          if (!pThis || !pDmg) return;
          if (api.object_class(pThis) != clsRole || api.object_class(pDmg) != clsDmg) return;
          float d = 0.f;
          p_fget(pDmg, f_dmg, &d);
          void* lp = LocalPlayer();
          BattleDecision dec = Decide(g_bGod.load(), g_bX100.load(), d,
                                      (lp && pThis == lp), (lp && pAtk == lp));
          if (dec.action == kBAGod) {
              p_fset(pDmg, f_dmg, &dec.value);
              DbgLog("battle: god self d=%.1f -> 0", (double)d);
          } else if (dec.action == kBAX100) {
              p_fset(pDmg, f_dmg, &dec.value);
              DbgLog("battle: x100 d=%.1f -> %.1f", (double)d, (double)dec.value);
          }
      };
      // 控制流统一走 BattleFlow.h 的 RunFrameFlow（与 battle_flow_test.cpp 共用同一模板）：
      //   A 双开关关→orig；B 再入→orig；C guard 内 Init+查询+改写（scope 内不调 orig）；D scope 外唯一 orig。
      return RunFrameFlow(g_bGod.load(std::memory_order_relaxed),
                          g_bX100.load(std::memory_order_relaxed), t_inHook,
                          [&]() { return g_ready.load(std::memory_order_acquire); },
                          [&]() { return Init(); },
                          work, callOrig);
  }
  }   // namespace kb
#endif

static int InstallBattleHooks() {
    InitSlotAddrs();
    int ok = 0, dead = 0;
    for (int i = 0; i < kSlotsCount && i < kSlotsMax; ++i) {
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
#if KYZT_BATTLE_ENABLE
    // 2.2 帧入口（唯一：0x19a9004），序言 str x30,[sp,#-0x30]! = 0xF81D0FFE
    if (g_libVer.load() == 22) {
        // 注意：HookAtRva 的参数是 **file offset**（内部走 FileOffToVa），不是 VA。
        // EnterFrameFromInterpreter VA=0x19a9004 ⇔ file=0x19a5004（va−file=0x4000）；
        // 实测 file 0x19a5004 首指令 0xF81D0FFE（str x30,[sp,#-0x30]!），
        // 而 0x19a9004 处为 0xB4052B20 ⇒ 传 VA 会序言校验失败（失败关闭，不会错挂）。
        if (HookAtRva("Battle", 0x19a5004u, 0xF81D0FFEu,
                      (void*)kb::Hooked_EnterFrame, &kb::g_origEnter)) {
            ++ok;
        } else {
            DbgLog("battle: EnterFrame hook 未装上（失败关闭）");
        }
    }
#endif
    LOGI(TPL_LOG " install pass: ok=%d dead=%d/%d", ok, dead, kSlotsCount);
    if (ok > 0) g_anyHooked.store(true);
    return ok;
}

// ============================================================================
//  ⑤-a 资源型（Button：点一次 +9999）
//     抓点依据：斌哥走 ItemManager.AddItem(id, 9999)，id 按物品表 1=金币/2=钻石/3=积分
//     （type=3 另有 AccountInfo.points 写入，本版先只走 AddItem，points 待诊断确认）
// ============================================================================
static void* g_itemMgrCls = nullptr;
static std::atomic<int> g_grantSeq{0};
static std::atomic<bool> g_diagDone{false};
static std::atomic<int> g_diagTries{0};

static void DumpImagesOnce() {
    auto& api = GetApi();
    if (!api.ready) { if (!api.Load(TPL_TARGET_LIB)) { LOGW(TPL_LOG " Il2Cpp Load 失败"); return; } }
    typedef const char* (*ImgName_t)(const void*);
    static ImgName_t imgName = nullptr;
    if (!imgName) imgName = (ImgName_t)Il2Cpp::ResolveExport("il2cpp_image_get_name");
    size_t n = 0;
    void** asms = api.Assemblies(&n);
    LOGI(TPL_LOG " assemblies=%zu imgNameFn=%p", n, (void*)imgName);
    if (!asms) return;
    for (size_t i = 0; i < n && i < 80; ++i) {
        void* img = asms[i] ? api.ImageOf(asms[i]) : nullptr;
        LOGI(TPL_LOG "  asm[%zu] img=%p name=%s", i, img, (img && imgName) ? imgName(img) : "?");
    }
}

static void DumpClassShape(const char* name) {
    auto& api = GetApi();
    char buf[64];
    snprintf(buf, sizeof(buf), "%s", name);
    size_t L = strlen(buf);
    bool fieldsOnly = false;
    if (L > 10 && strcmp(buf + L - 10, "FieldsOnly") == 0) { buf[L - 10] = 0; fieldsOnly = true; }
    void* k = api.FindClassByShortName(buf);
    if (!k) { LOGW(TPL_LOG " %s 未找到", name); return; }
    LOGI(TPL_LOG " == %s @%p", name, k);
    if (api.class_fields && api.field_name) {
        void* iter = nullptr;
        void* f = nullptr;
        int n = 0;
        while ((f = api.class_fields(k, &iter)) != nullptr && n < 90) {
            ++n;
            const char* fn = api.field_name(f);
            const char* tn = (api.field_type && api.type_name) ? api.type_name(api.field_type(f)) : "?";
            LOGI(TPL_LOG "    F %-26s off=%-4zu ty=%s", fn ? fn : "?", api.field_offset ? api.field_offset(f) : 0, tn ? tn : "?");
        }
    }
    if (!fieldsOnly && api.class_methods && api.method_get_name) {
        void* iter = nullptr;
        void* m = nullptr;
        int n = 0;
        while ((m = api.class_methods(k, &iter)) != nullptr && n < 400) {
            ++n;
            const char* mn = api.method_get_name(m);
            if (!mn) continue;
            const bool want = (strstr(mn, "Add") || strstr(mn, "Change") || strstr(mn, "Gold")
                               || strstr(mn, "gold") || strstr(mn, "point") || strstr(mn, "Point")
                               || strstr(mn, "Hp") || strstr(mn, "atk") || strstr(mn, "Attr"));
            if (!want) continue;
            LOGI(TPL_LOG "    M %s/%d", mn, api.method_param_count ? api.method_param_count(m) : -1);
        }
    }
}

static void DiagConfigSet();

// 读 BattleRoleActor.combatPowerBaseAttributes（Dictionary<EAttributeEnum,float>）的 key/value
static void DumpAttrDict() {
    auto& api = GetApi();
    if (!api.ready) return;
    void* cls = api.FindClassByShortName("BattleRoleActor");
    if (!cls) { LOGW(TPL_LOG " BRA 类未找到"); return; }
    std::vector<void*> v;
    api.ScanInstances(cls, v, 512);
    LOGI(TPL_LOG " BRA 实例 %zu 个", v.size());
    for (size_t i = 0; i < v.size() && i < 2; ++i) {
        void* inst = v[i];
        if (!inst || !Il2Cpp::IsValidHeapPtr(inst)) continue;
        void* icls = api.object_class(inst);
        size_t off = icls ? api.FieldOffset(icls, "combatPowerBaseAttributes") : 0;
        if (!off) continue;
        void* dict = Il2Cpp::ReadPtr(inst, off);
        LOGI(TPL_LOG "  BRA[%zu]=%p attrDict=%p", i, inst, dict);
        if (!dict || !Il2Cpp::IsValidHeapPtr(dict)) continue;
        void* dcls = api.object_class(dict);
        size_t eOff = dcls ? api.FieldOffset(dcls, "_entries") : 0;
        size_t cOff = dcls ? api.FieldOffset(dcls, "_count") : 0;
        LOGI(TPL_LOG "   _entries=%zu _count=%zu", eOff, cOff);
        if (!eOff || !cOff) continue;
        const int32_t cnt = Il2Cpp::ReadI32(dict, cOff);
        void* arr = Il2Cpp::ReadPtr(dict, eOff);
        LOGI(TPL_LOG "   count=%d arr=%p", cnt, arr);
        if (!arr || cnt <= 0 || cnt > 200) continue;
        for (int k = 0; k < cnt && k < 14; ++k) {
            uint8_t* e = (uint8_t*)arr + 32 + (size_t)k * 16;
            const int32_t key = *(int32_t*)(e + 8);
            const float val = *(float*)(e + 12);
            LOGI(TPL_LOG "    attr[%d] key=%d val=%.2f", k, key, val);
        }
    }
}

static void DiagHotClasses() {
    auto& api = GetApi();
    if (!api.ready) { if (!api.Load(TPL_TARGET_LIB)) { LOGW(TPL_LOG " Il2Cpp Load 失败（diag）"); return; } }
    const char* names[] = {"ItemManager", "PlayerInfo", "AccountMgr", "BattleRoleActor"};
    int hit = 0;
    for (int i = 0; i < 4; ++i) {
        void* k = api.FindClassByShortName(names[i]);
        if (!k) { LOGW(TPL_LOG " resolve %s -> null", names[i]); continue; }
        ++hit;
        LOGI(TPL_LOG " resolve %s -> %p instance=%p", names[i], k, api.StaticRefField(k, "instance"));
    }
    if (hit > 0 && !g_diagDone.load()) {
        DumpClassShape("BattleRoleActor");
        {
            auto& a3 = GetApi();
            void* k3 = a3.FindClassByShortName("AccountMgr");
            if (k3 && a3.class_methods && a3.method_get_name) {
                void* it3 = nullptr; void* m3 = nullptr; int c3 = 0;
                while ((m3 = a3.class_methods(k3, &it3)) != nullptr && c3 < 60) {
                    ++c3;
                    const char* n3 = a3.method_get_name(m3);
                    if (!n3) continue;
                    LOGI(TPL_LOG "    AM %s/%d", n3, a3.method_param_count ? a3.method_param_count(m3) : -1);
                }
            }
        }
        {
            auto& a2 = GetApi();
            void* k2 = a2.FindClassByShortName("ItemManager");
            if (k2 && a2.class_methods && a2.method_get_name) {
                void* it = nullptr; void* mm = nullptr; int cnt = 0;
                while ((mm = a2.class_methods(k2, &it)) != nullptr && cnt < 80) {
                    ++cnt;
                    const char* mn = a2.method_get_name(mm);
                    const int pc = a2.method_param_count ? a2.method_param_count(mm) : -1;
                    char sig[160] = {0};
                    if (mn && (strncmp(mn, "Spawn", 5) == 0 || strncmp(mn, "Drop", 4) == 0
                               || strcmp(mn, "OnGetItem") == 0 || strcmp(mn, "GetNewItem") == 0
                               || strcmp(mn, "GetAll") == 0 || strcmp(mn, "GetItemByItemid") == 0)
                        && a2.method_param && a2.type_name) {
                        for (int pi = 0; pi < pc && pi < 4; ++pi) {
                            const char* tn = a2.type_name(a2.method_param(mm, pi));
                            snprintf(sig + strlen(sig), sizeof(sig) - strlen(sig), " p%d=%s", pi, tn ? tn : "?");
                        }
                        const char* rt = (a2.method_ret_type && a2.type_name) ? a2.type_name(a2.method_ret_type(mm)) : "?";
                        snprintf(sig + strlen(sig), sizeof(sig) - strlen(sig), " ret=%s", rt ? rt : "?");

                    }
                    LOGI(TPL_LOG "    IM %s/%d%s", mn ? mn : "?", pc, sig);
                }
            }
        }
    }
    if (hit > 0) g_diagDone.store(true);
}

// ===== 受保护的 il2cpp 调用（参数类型猜错时不崩进程）=====
struct InvokeCtx { void* mi; void* inst; void** args; void* exc; void* ret; bool done; };
static void InvokeThunk(void* p) {
    auto* c = static_cast<InvokeCtx*>(p);
    c->exc = nullptr;
    c->ret = GetApi().runtime_invoke(c->mi, c->inst, c->args, &c->exc);
    c->done = true;
}
static bool SafeInvoke(const char* tag, void* mi, void* inst, void** args, void** outRet, void** outExc) {
    if (!mi) return false;
    InvokeCtx c{mi, inst, args, nullptr, nullptr, false};
    if (!RunGuarded(tag, InvokeThunk, &c)) {
        LOGW(TPL_LOG " %s: 调用触发致命信号，已拦截（参数签名不符）", tag);
        return false;
    }
    if (outRet) *outRet = c.ret;
    if (outExc) *outExc = c.exc;
    return true;
}

// AccountMgr.instance -> 字段 AccountInfo(off=40) 才是账号数据（get_nowPlayerInfo 返回的是 PlayerInfo）
struct AcctRef { void* mgr; void* info; };

static AcctRef GetAccount() {
    auto& api = GetApi();
    if (!api.ready) { if (!api.Load(TPL_TARGET_LIB)) return {nullptr, nullptr}; }
    void* mgrCls = api.FindClassByShortName("AccountMgr");
    if (!mgrCls) return {nullptr, nullptr};
    void* mgr = api.StaticRefField(mgrCls, "instance");
    if (!mgr) return {nullptr, nullptr};
    size_t off = api.FieldOffset(mgrCls, "AccountInfo");
    if (!off) { LOGW(TPL_LOG " AccountMgr.AccountInfo 字段偏移未找到"); return {mgr, nullptr}; }
    void* info = Il2Cpp::ReadPtr(mgr, off);
    if (!info || !Il2Cpp::IsValidHeapPtr(info)) { LOGW(TPL_LOG " AccountInfo 为空（mgr=%p off=%zu）", mgr, off); return {mgr, nullptr}; }
    return {mgr, info};
}

// 写 AccountInfo 的整型字段，并把 AccountMgr.needSave 置真（否则只在内存里，读档即回滚）
static bool BumpAcctField(const char* field, int delta, const char* tag) {
    AcctRef a = GetAccount();
    if (!a.info) { LOGW(TPL_LOG " %s: AccountInfo 拿不到", tag); return false; }
    auto& api = GetApi();
    void* cls = api.object_class(a.info);
    size_t off = cls ? api.FieldOffset(cls, field) : 0;
    if (!off) {
        void* declCls = api.FindClassByShortName("AccountInfo");
        if (declCls) off = api.FieldOffset(declCls, field);
    }
    if (!off) { LOGW(TPL_LOG " %s: 字段 %s 未找到", tag, field); return false; }
    // points=Int64；sharedGold=Nullable<Int64>（bool@+0, long@+8）——一律按 64 位处理
    if (strcmp(field, "sharedGold") == 0) {
        uint8_t has = Il2Cpp::ReadU8(a.info, off);
        int64_t v = *(int64_t*)((uintptr_t)a.info + off + 8);
        *(int64_t*)((uintptr_t)a.info + off + 8) = v + (int64_t)delta;
        Il2Cpp::WriteU8(a.info, off, 1);
        LOGI(TPL_LOG " %s: sharedGold has=%u value %lld -> %lld (off=%zu)", tag, has, (long long)v, (long long)(v + delta), off);
    } else {
        int64_t v = *(int64_t*)((uintptr_t)a.info + off);
        *(int64_t*)((uintptr_t)a.info + off) = v + (int64_t)delta;
        LOGI(TPL_LOG " %s: %s(off=%zu) %lld -> %lld", tag, field, off, (long long)v, (long long)(v + delta));
    }
    if (a.mgr) {
        void* mgrCls = api.object_class(a.mgr);
        size_t nOff = mgrCls ? api.FieldOffset(mgrCls, "needSave") : 0;
        if (nOff) { Il2Cpp::WriteU8(a.mgr, nOff, 1); LOGI(TPL_LOG " %s: needSave=1（触发存档）", tag); }
    }
    return true;
}

// 直接写 AccountInfo 的整型字段（points=积分 / sharedGold=金币）
static bool BumpIntField(void* obj, const char* field, int delta, const char* tag) {
    if (!obj) { LOGW(TPL_LOG " %s: 对象为空", tag); return false; }
    auto& api = GetApi();
    void* cls = api.object_class(obj);
    const char* cname = cls ? api.ClassName(obj) : "?";
    size_t off = cls ? api.FieldOffset(cls, field) : 0;
    if (!off) {
        // get_nowPlayerInfo 返回的未定是 AccountInfo 本身；用声明类再试一次
        void* declCls = api.FindClassByShortName("AccountInfo");
        if (declCls) off = api.FieldOffset(declCls, field);
        LOGI(TPL_LOG " %s: obj=%p cls=%p(%s) 回退声明类 -> off=%zu", tag, obj, cls, cname, off);
    }
    if (!off) { LOGW(TPL_LOG " %s: 字段 %s 未找到（cls=%s）", tag, field, cname); return false; }
    const int32_t cur = Il2Cpp::ReadI32(obj, off);
    Il2Cpp::WriteI32(obj, off, cur + delta);
    LOGI(TPL_LOG " %s: %s(off=%zu) %d -> %d", tag, field, off, cur, cur + delta);
    return true;
}

// 钻石：1.8 里钻石不是 AccountInfo 独立字段，而是背包物品（itemid = 2）。
// 实测（2026-11-09，玩家背包 bag[17] itemid=2 count=19998）：
//   AccountMgr.instance → +40 AccountInfo → +96 players(List<PlayerInfo>) → [0]
//   → +272 saveBags(List<ItemSaveData>) → 遍历 baseItemid@164 == 2 → count@16 自增
// List<T> 内存布局：+0x10 元素数组指针 / +0x18 size；数组 +0x20 起为元素指针。
// 只在「已有条目」上自增，不造新物品（配置表接口 GetNewItem/SpawnItem 在注入环境一律 SIG）。
static bool GrantDiamond(int delta) {
    auto& api = GetApi();
    if (!api.ready) { if (!api.Load(TPL_TARGET_LIB)) return false; }
    void* mgrCls = api.FindClassByShortName("AccountMgr");
    if (!mgrCls) { LOGW(TPL_LOG " 钻石: AccountMgr 未找到"); return false; }
    void* mgr = api.StaticRefField(mgrCls, "instance");
    if (!OkPtr(mgr)) { LOGW(TPL_LOG " 钻石: AccountMgr.instance 为空"); return false; }

    void* info = *(void**)((uintptr_t)mgr + 40);
    if (!OkPtr(info)) { LOGW(TPL_LOG " 钻石: AccountInfo 为空"); return false; }
    void* players = *(void**)((uintptr_t)info + 96);
    if (!OkPtr(players)) { LOGW(TPL_LOG " 钻石: players 为空"); return false; }
    void* pArr = *(void**)((uintptr_t)players + 0x10);
    const int32_t pCnt = *(int32_t*)((uintptr_t)players + 0x18);
    if (!OkPtr(pArr) || pCnt <= 0) { LOGW(TPL_LOG " 钻石: players 空表(cnt=%d)", pCnt); return false; }
    void* p0 = *(void**)((uintptr_t)pArr + 0x20);
    if (!OkPtr(p0)) { LOGW(TPL_LOG " 钻石: player0 为空"); return false; }

    void* saveBags = *(void**)((uintptr_t)p0 + 272);
    if (!OkPtr(saveBags)) { LOGW(TPL_LOG " 钻石: saveBags 为空"); return false; }
    void* items = *(void**)((uintptr_t)saveBags + 0x10);
    const int32_t n = *(int32_t*)((uintptr_t)saveBags + 0x18);
    if (!OkPtr(items) || n <= 0) { LOGW(TPL_LOG " 钻石: saveBags 空表(cnt=%d)", n); return false; }

    int32_t hit = 0;
    for (int32_t i = 0; i < n; ++i) {
        void* it = *(void**)((uintptr_t)items + 0x20 + (uintptr_t)i * 8);
        if (!OkPtr(it)) continue;
        if (*(int32_t*)((uintptr_t)it + 164) != 2) continue;    // baseItemid == 钻石
        const int32_t before = *(int32_t*)((uintptr_t)it + 16);
        *(int32_t*)((uintptr_t)it + 16) = before + delta;
        ++hit;
        LOGI(TPL_LOG " 钻石: bag[%d] count %d -> %d", i, before, before + delta);
        DbgLog("diamond: bag[%d] count %d -> %d", i, before, before + delta);
    }
    if (!hit) { LOGW(TPL_LOG " 钻石: 背包内无 itemid=2 条目（不凭空创建）"); return false; }

    // 触发存档，否则读档回滚
    size_t nOff = api.FieldOffset(mgrCls, "needSave");
    if (nOff) { Il2Cpp::WriteU8(mgr, nOff, 1); LOGI(TPL_LOG " 钻石: needSave=1"); }
    return true;
}

static bool GrantPoints(int delta) {
    return BumpAcctField("points", delta, "积分");
}

static bool GrantGold(int delta) {
    if (BumpAcctField("sharedGold", delta, "金币")) return true;
    // 兜底：ItemManager.ChangeGold(delta)，走保护区（上一版在这里 SIGSEGV）
    auto& api = GetApi();
    if (!api.ready) api.Load(TPL_TARGET_LIB);
    void* cls = api.FindClassByShortName("ItemManager");
    if (!cls) return false;
    void* inst = api.StaticRefField(cls, "instance");
    void* mi = api.GetMethod(cls, "ChangeGold", 1);
    if (!inst || !mi) { LOGW(TPL_LOG " 金币: ChangeGold=%p instance=%p", mi, inst); return false; }
    void* args[1] = { api.BoxInt32(delta) };
    void* exc = nullptr;
    if (!SafeInvoke("ChangeGold", mi, inst, args, nullptr, &exc)) return false;
    LOGI(TPL_LOG " 金币: ChangeGold(%d) exc=%p", delta, exc);
    return exc == nullptr;
}

static std::atomic<int> g_itemNum{1};    // 数量

// 堆扫描找活实例（1.8 的 ItemManager/BattleRoleActor 静态 instance 都是空的）
static void* FindLiveInstance(const char* clsName, const char* tag) {
    auto& api = GetApi();
    if (!api.ready) { if (!api.Load(TPL_TARGET_LIB)) return nullptr; }
    void* cls = api.FindClassByShortName(clsName);
    if (!cls) { LOGW(TPL_LOG " %s: 类 %s 未找到", tag, clsName); return nullptr; }
    void* st = api.StaticRefField(cls, "instance");
    if (st && Il2Cpp::IsValidHeapPtr(st)) return st;
    std::vector<void*> v;
    api.ScanInstances(cls, v, 384);
    LOGI(TPL_LOG " %s: 堆扫描 %s 命中 %zu 个实例", tag, clsName, v.size());
    if (v.empty()) return nullptr;
    for (size_t i = 0; i < v.size() && i < 4; ++i) LOGI(TPL_LOG "   inst[%zu]=%p", i, v[i]);
    return v[0];
}

// 添加道具：ItemManager.OnGetItem(id)（1.8 方法表里最接近「直接获得」的入口）
static bool AddItemById(int id) {
    auto& api = GetApi();
    if (!api.ready) api.Load(TPL_TARGET_LIB);
    void* cls = api.FindClassByShortName("ItemManager");
    if (!cls) { LOGW(TPL_LOG " 添加道具: ItemManager 类未找到"); return false; }
    void* inst = FindLiveInstance("ItemManager", "添加道具");
    if (!inst) { LOGW(TPL_LOG " 添加道具: 无活实例"); return false; }
    void* mi = api.GetMethod(cls, "OnGetItem", 1);
    if (!mi) mi = api.GetMethod(cls, "GetNewItem", 1);
    if (!mi) { LOGW(TPL_LOG " 添加道具: OnGetItem/GetNewItem 均未找到"); return false; }
    const int n = g_itemNum.load() > 0 ? g_itemNum.load() : 1;
    void* args[1] = { api.BoxInt32(id) };
    void* exc = nullptr;
    if (!SafeInvoke("OnGetItem", mi, inst, args, nullptr, &exc)) return false;
    LOGI(TPL_LOG " 添加道具 id=%d n=%d mi=%p exc=%p", id, n, mi, exc);
    return exc == nullptr;
}

static bool GrantItem(int id, int count) {
    auto& api = GetApi();
    if (!api.ready) return false;
    if (!g_itemMgrCls) g_itemMgrCls = api.FindClassByShortName("ItemManager");
    if (!g_itemMgrCls) { LOGW(TPL_LOG " ItemManager 类未找到"); return false; }
    void* inst = api.StaticRefField(g_itemMgrCls, "instance");
    if (!inst) { LOGW(TPL_LOG " ItemManager.instance 为空"); return false; }
    void* mi = api.GetMethod(g_itemMgrCls, "AddItem", 2);
    if (!mi) { LOGW(TPL_LOG " AddItem(argc=2) 未找到（待确认签名）"); return false; }
    const int n = g_grantSeq.fetch_add(1);
    void* args[2] = { api.BoxInt32(id), api.BoxInt32(count) };
    void* exc = nullptr;
    api.runtime_invoke(mi, inst, args, &exc);
    LOGI(TPL_LOG " AddItem(id=%d n=%d) #%d exc=%p", id, count, n + 1, exc);
    return exc == nullptr;
}

static bool AnyFeatureOn() {
    bool q = false;
#if KYZT_HAS_ECONOMY22
    q = Kyzt22::PendingCount() > 0;   // 资源按钮按一下也要装帧锚点，否则队列永不排空
#endif
    return g_spd.load() || g_dmg.load() || g_inv.load() || g_noad.load() || q;
}

static std::atomic<int> g_syncTries{0};

static void SyncHooks() {
    if (!g_libReady.load()) return;
    if (!AnyFeatureOn()) return;                        // 全关：不装、也不卸载
    if (g_installedCount.load() >= kSlotsCount) return; // 快路径：已装齐
    if (g_syncTries.load() > 8) return;                 // 装不上就停：禁止每 Tick 反复 DobbyHook

    std::lock_guard<std::mutex> lk(g_installMutex);     // ★ 串行化，防并发双挂
    if (g_installedCount.load() >= kSlotsCount) return; // double-check
    g_syncTries.fetch_add(1);
    const int n = InstallBattleHooks();
    g_installedCount.store(n);
    DbgLog("install pass: ok=%d/%d (try %d) mi=%p", n, kSlotsCount, g_syncTries.load(), g_miApplyHp.load());
}

// ============================================================================
//  ⑧ 对外接口（ModEntry.h 注册表契约，签名不可改）
// ============================================================================
bool 旷野征途_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;

    InstallFaultGuard();          // §④ 先装兜底，再装钩
    g_libReady.store(true);
    SyncHooks();                  // 已 ON 的开关立即装；全关保持 0 hook
    LOGI(TPL_LOG " lib ready (dual-path, lazy hooks)");
    return true;
}

bool 旷野征途_IsReady() {
    return g_libReady.load();
}

// 2.2 资源三项失败关闭：ItemSaveData.count / AccountInfo.points、gold 均为 XOR 编码
// （MemoryValueEncoding.RotateKey，密钥每进程随机；取证见 login-review 2026-11-14 报告）。
// ⇒ 1.8 的「直接写偏移」在 2.2 必然写坏存档/无效，故一律拒绝，不回落旧偏移。
//   未实现的 setter 路径（il2cpp_runtime_invoke 属性 setter）另行适配，本轮不做。
static bool ResRawWriteAllowed() {
    if (g_libVer.load() == 0) DetectLibVersion();
    // 只允许 1.8（18）：2.2 是 XOR 编码（写偏移必坏档），未知版本没有指纹依据 ⇒ 一律拒绝。
    // 审查结论：旧实现「未知版本 fail-open」是错的，改 fail-closed（2026-11-14）。
    if (g_libVer.load() != 18) {
        DbgLog("res: DISABLED ver=%d (需要 ver==18 且旧指纹证据；2.2 为 xor 编码)", g_libVer.load());
        return false;
    }
    return true;
}

// 资源三项 2.2 路径：只在「菜单开关回调（Java UI 线程）」上调用，Tick 线程一律拒绝。
// 返回值：true=调用成功；false=未启用/线程不对/未就绪 ⇒ 调用方不得回退旧偏移。
static bool Eco22Grant(int featNum) {
#if KYZT_HAS_ECONOMY22
    if (t_inModTick) {
        DbgLog("eco: feat=%d refused (mod tick thread)", featNum);
        return false;
    }
    int rc = -1;
    switch (featNum) {
        case 旷野征途_FEAT_GOLD:    rc = Kyzt22::GrantGold(9999);    break;
        case 旷野征途_FEAT_DIAMOND: rc = Kyzt22::GrantDiamond(9999); break;
        case 旷野征途_FEAT_POINT:   rc = Kyzt22::GrantPoints(9999);  break;
        default: return false;
    }
    DbgLog("eco: feat=%d rc=%d", featNum, rc);
    return rc == 0;
#else
    (void)featNum;
    return false;
#endif
}

void 旷野征途_OnFeatureChanged(int featNum, bool enabled) {
#if KYZT_BATTLE_ENABLE
    // 2.2 战斗开关：只写原子标志，真正生效在帧入口过滤 ApplyHp 时判断
    if (featNum == 旷野征途_FEAT_INV) kb::g_bGod.store(enabled);
    if (featNum == 旷野征途_FEAT_DMG) kb::g_bX100.store(enabled);
#endif
    if (enabled && (featNum == 旷野征途_FEAT_GOLD || featNum == 旷野征途_FEAT_DIAMOND ||
                    featNum == 旷野征途_FEAT_POINT)) {
        if (g_libVer.load() == 0) DetectLibVersion();
        if (g_libVer.load() == 22) {
            // 2.2：菜单回调（Java UI 线程）**只入队**，真正调用发生在 ET.GameManagerMono.Update
            // （Unity 线程）里 drain —— 不在 UI 线程直接碰 il2cpp 对象。
            const unsigned long ut = g_unityTid.load();
            const unsigned long me = (unsigned long)pthread_self();
            const int probe = (ut == 0) ? 3 : ((ut == me) ? 1 : 2);
            if (g_tidProbe.load() != probe) {
                g_tidProbe.store(probe);
                DbgLog("tid: menu=%lu unity=%lu equal=%d (0=未知参照)", me, ut, probe == 1 ? 1 : 0);
            }
#if KYZT_HAS_ECONOMY22
            int rc = -1;
            // 2026-11-14 22:30：改「当场执行」，不再依赖 Unity 帧回调（主界面没有 Update 钩子）
            {
                Kyzt22::log_fn() = DbgLog;
                const int kind = (featNum == 旷野征途_FEAT_GOLD) ? 1
                               : (featNum == 旷野征途_FEAT_DIAMOND) ? 2 : 3;
                rc = Kyzt22::RunNow(kind, 9999);
            }
            // 返回码分类（h v3）：100=已入队（≠成功）；0=成功；7=钻石id未证；8=保存失败；9=溢出；1/2/3/4/5/6=失败
            const char* cls = (rc == 100) ? "QUEUED" : (rc == 0 ? "OK" : "FAIL");
            DbgLog("eco: enqueue feat=%d rc=%d(%s) pending=%u", featNum, rc, cls, Kyzt22::PendingCount());
#else
            DbgLog("eco: feat=%d SKIPPED (Economy22.h 未编译进本构建)", featNum);
#endif
            SyncHooks();
            return;
        }
        if (!ResRawWriteAllowed()) return;   // 1.8 以外（未知版本）仍失败关闭
    }
    switch (featNum) {
        case 旷野征途_FEAT_GOLD:    if (enabled && !GrantGold(9999)) GrantItem(1, 9999); break;
        case 旷野征途_FEAT_DIAMOND: if (enabled && !GrantDiamond(9999)) GrantItem(2, 9999); break;
        case 旷野征途_FEAT_POINT:   if (enabled && !GrantPoints(9999)) GrantItem(3, 9999); break;
        case 旷野征途_FEAT_SPD: g_spd.store(enabled); break;
        case 旷野征途_FEAT_DMG: g_dmg.store(enabled); break;
        case 旷野征途_FEAT_INV: g_inv.store(enabled); break;
        case 旷野征途_FEAT_NOAD: g_noad.store(enabled); break;
        default: break;
    }
    SyncHooks();
}

void 旷野征途_OnValueChanged(int /*featNum*/, int /*value*/) {
}

void 旷野征途_Tick(const bool* features, const int* /*values*/) {
    if (!features) return;
    struct TickScope { TickScope() { t_inModTick = true; } ~TickScope() { t_inModTick = false; } } _ts;

    g_spd.store(features[旷野征途_FEAT_SPD]);
    g_dmg.store(features[旷野征途_FEAT_DMG]);
    g_inv.store(features[旷野征途_FEAT_INV]);
    g_noad.store(features[旷野征途_FEAT_NOAD]);

    if (g_libReady.load() && !g_diagDone.load()) {
        // il2cpp 要求线程 attach：Tick 跑在 Mod 后台线程，不 attach 则 domain 枚举返回 0
        {
            auto& api0 = GetApi();
            if (api0.thread_attach) {
                void* d0 = api0.Domain();
                if (d0) api0.thread_attach(d0);
            }
        }
        const int t = g_diagTries.fetch_add(1);
        if (t == 0) DumpImagesOnce();
        if ((t % 5) == 0) DiagHotClasses();     // 每 1s 重试，热更 dll 加载后才有类
        if (t > 120) g_diagDone.store(true);    // 24s 后放弃
    }

#if KYZT_HAS_ECONOMY22
    // 2026-11-14 22:32：探针改从这里跑（Tick 常驻；battle init / 帧钩子都只在进战斗后才有）
    if (g_libReady.load()) {
        static int s_probeTry = 0;
        if (s_probeTry < 600) {
            ++s_probeTry;
            {
                auto& apiP = GetApi();
                if (apiP.thread_attach) { void* dP = apiP.Domain(); if (dP) apiP.thread_attach(dP); }
                Kyzt22::log_fn() = DbgLog;
                Kyzt22::probe_once(apiP);
            }
        }
    }
#endif

    // 兜底：库晚于首帧映射时，这里补一次
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) {
        InstallFaultGuard();
        g_libReady.store(true);
        LOGI(TPL_LOG " lib ready (via Tick fallback)");
    }
    if (g_libReady.load() && !g_miApplyHp.load()) {
        g_miTries.fetch_add(1);
        if ((g_miTries.load() % 5) == 0) ResolveAdGlobals();
        if ((g_tickSeq.fetch_add(1) % 25) == 0) ResolveCombatMi();   // 5s 一次，避免拖死 Tick
    }
    SyncHooks();

    // 主动钉 timeScale：不依赖游戏主动调用 setter（本作可能在启动时只调一次）
    if (g_spd.load()) {
        void* m = g_tsMethod.load();
        if (m && g_origSetTimeScale) {
            g_origSetTimeScale(1.8f, m);
            static int tickLog = 0;
            if (tickLog < 3) { ++tickLog; LOGI(TPL_LOG " force timeScale=1.8 (direct orig call)"); }
        }
    }

    // ★ 禁止在此 SafeProbe / 扫堆 / 遍历对象 / 逐对象写（Tick 频率高，代价爆炸）
}

void 旷野征途_ForceClear() {
    g_spd.store(false);
    g_dmg.store(false);
    g_inv.store(false);
    g_noad.store(false);
    g_reenter.store(false);
    // TODO：新增开关一并复位
}
