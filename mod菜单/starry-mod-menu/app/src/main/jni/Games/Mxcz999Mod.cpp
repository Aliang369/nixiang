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

#include "Mxcz999Mod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
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

// 生成器把 [Mxcz999] 替换为本游戏标签（如 [jfqs]），日志过滤用 adb logcat -s Mod_Menu
#define TPL_LOG "[Mxcz999]"

// 注册表模式不依赖 GameConfig.h（legacy 配置中心）。
// Cocos 系生成后把下面这处（含各 hook 内引用）统一换成目标库名。
#define TPL_TARGET_LIB sec_lib_il2cpp()

// ============================================================================
//  ① 开关与 orig 指针
//     g_libReady = 「库已映射」≠「已挂钩」；全开关关闭时应当保持 0 个 hook。
// ============================================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_reenter{false};

// 六项开关：默认全关（铁律：没有任何功能默认开启）。1.0 = 该滑杆项等效关闭。
static std::atomic<bool>  g_inv{false};        // ① 无敌
static std::atomic<float> g_dmgMul{1.0f};      // ② 伤害倍率（1.0 = 关）
static std::atomic<float> g_moveSpeed{1.0f};   // ③ 移动速度（1.0 = 关）
static std::atomic<bool>  g_skin{false};       // ④ 解锁所有皮肤
static std::atomic<bool>  g_coupon{false};     // ⑤ 无限解锁券
static std::atomic<bool>  g_noad{false};       // ⑥ 免广告

// UnityEngine.Vector2 在 AAPCS64 里是 HFA(2×float) ⇒ 走 s0/s1，不能用 void* 占位，
// 否则钩子里的形参位置与真实寄存器错位，回传 orig 时整串参数被移位（实测级坑）。
struct UI2 { float x, y; };

// 原始实现指针（§⑥ hook 体统一经它们转调；nullptr = 安全退化，见各 hook）
typedef bool (*HpCheck_t)      (void* self, void* mth);
typedef void (*PlayerHurt_t)   (void* self, void* st, void* est, void* bufflist, void* pst,
                                int eid, void* e, void* b, float demmul, void* mth);
typedef void (*EnemyHurt_t)    (void* self, void* st, double dem, void* tf, UI2 add,
                                void* e, void* mth);
typedef void (*EnemyHurtNN_t)  (void* self, void* st, float dem, void* tf, UI2 add,
                                void* e, void* mth);
typedef void (*Move_t)         (void* self, void* mth);
typedef bool (*HaveAc_t)       (void* self, void* mth);
typedef void (*Coupon_t)       (void* self, void* mth);
typedef void (*Ad1_t)          (void* self, void* mth);            // getadrevive
typedef bool (*AdCheck_t)       (void* self, void* mth);            // getadcheck（返回 bool）
typedef void (*Ad2_t)          (void* self, void* arg, void* mth);  // getadfreebuy / getadexpmul
typedef void (*Reward_t)       (void* self, void* mth);             // admanager.reward

typedef void (*NmHurte_t)(void*, void*, void*, int32_t, void*, float, float, float, float, void*);
typedef void (*NoArg_t)(void*, void*);
typedef void (*AdExp_t)(void*, float, void*);

static HpCheck_t     g_origHpCheck      = nullptr;
static PlayerHurt_t  g_origPlayerHurt   = nullptr;
static EnemyHurt_t   g_origEnemyHurt    = nullptr;
static EnemyHurtNN_t g_origEnemyHurtNN  = nullptr;
static Move_t        g_origMove         = nullptr;
static HaveAc_t      g_origHaveAc       = nullptr;
static Coupon_t      g_origCoupon       = nullptr;
static Ad1_t         g_origAdLoadReward = nullptr;   // admanager.LoadRewardAd（免广告单点）
static AdCheck_t     g_origAdCheck      = nullptr;   // admanager.getadcheck
static NmHurte_t     g_origNmHurte      = nullptr;   // basicbase.nmhurte（玩家输出侧倍率）
static NoArg_t       g_origCheckButton  = nullptr;   // checkpanel.checkbutton（真实扣券路径）
static NoArg_t       g_origAdBuy        = nullptr;   // perchaseui0.getfreebuyad
static NoArg_t       g_origAdExp        = nullptr;   // settlepanel.adexpmul
static NoArg_t       g_origAdRevive     = nullptr;   // uicontrol0.getrevivead
static NoArg_t       g_origProbeCheckBtn   = nullptr;   // checkpanel.checkbutton（签到领取，探针）
static NoArg_t       g_origProbeCheckAdBtn = nullptr;   // checkpanel.checkadbutton（额外领取按钮，探针）

// ============================================================================
//  ② 字段偏移与 RVA
//     TODO：全部来自 dump.cs / IDA / Trace 实测，禁止猜；换包必须重 dump。
// ============================================================================
// player.stat 指针在 player+0x130；stat 内 hp=maxhp 之后的 double/float/int
enum : size_t {
    kOffPlayerStat     = 0x130,  // player -> basicbase.stat*（obj+0x130）
    kOffStatHp         = 0x20,   // stat.hp double
    kOffStatHpMax      = 0x18,   // stat.maxhp double（玩家血量上限）   // stat.hp       double（当前血量）
    kOffStatMaxHp      = 0x18,   // stat.maxhp    double（备用：回写掩盖掉血）
    kOffStatAdie       = 0x154,  // stat.adie（死亡标记，hpcheck 读它）
    kOffStatSp         = 0x9C,   // stat.sp       float（移速，move() 读取）
    kOffSdField        = 0x4F0,  // data.sd -> database.Savedata*（券余额所在存档）
    kOffUnlockCoupon   = 0xBC,   // Savedata.unlockcoupon int（唯一扣券点 0x1B213A4 写它）
    kOffAdAt           = 0x50,   // admanager.at int（adtype，reward() 按它 switch）
    kOffAdNative       = 0x48,   // admanager.adNative（DirichletAdNative，SDK 拉起处）
};

// ★ RVA 基准换算（2026-11-15 实测，勿删）：
//   dump.cs / IDA 给的 RVA 是 **ELF vaddr**；本骨架 HookAtRva()->FileOffToVa() 要的是
//   **文件偏移**。本包 libil2cpp.so 可执行段 p_offset=0x1631B84 / p_vaddr=0x1635B84
//   ⇒ file_off = vaddr - 0x4000。直接填 vaddr 会 hook 到函数后 0x4000 处 = 错函数（静默）。
//   下方 kSlots 一律写成 「实测 vaddr - kRvaVaddrBias」，expectInsn 用段内首条指令字做序言校验。
enum : uintptr_t {
    kRvaVaddrBias = 0x4000,
};

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

static void SafeWriteF64(void* obj, size_t off, double v) {
    if (!OkPtr(obj)) return;
    std::memcpy(reinterpret_cast<char*>(obj) + off, &v, sizeof(double));
}

static double SafeReadF64(void* obj, size_t off, double defv) {
    if (!OkPtr(obj)) return defv;
    double v = defv;
    std::memcpy(&v, reinterpret_cast<char*>(obj) + off, sizeof(double));
    return v;
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
//  ⑥ hook 函数与挂点表
//     铁律：一个具体方法一个槽；绝不 hook 共享 invoke 入口。
// ============================================================================
// ---- 1) 无敌：死亡判定恒「活着」----
// 语义：hpcheck() 返回「玩家是否存活」；返回 true 即不进入 die 分支。
// 关：原样转调 orig；orig 缺失：返回 true（宁可当作活着，也不再触发死亡流程）。
static bool Hk_HpCheck(void* self, void* mth) {
    if (!g_inv.load()) return g_origHpCheck ? g_origHpCheck(self, mth) : false;
    // 无敌：hpcheck(self=player) 每帧被调用 ⇒ 把 hp(+0x20 double) 拉回 maxhp(+0x18 double)。
    // 只作用于玩家实例（player 类方法），不会影响怪物 —— 这是 2026-11-15 首测「人怪都无敌」的修法。
    void* stat = OkPtr(self) ? *reinterpret_cast<void**>(reinterpret_cast<char*>(self) + kOffPlayerStat) : nullptr;
    if (OkPtr(stat)) {
        const double maxhp = SafeReadF64(stat, kOffStatHpMax, 0.0);
        if (maxhp > 0.0) SafeWriteF64(stat, kOffStatHp, maxhp);
    }
    return true;
}

// ---- 2) 无敌：玩家扣血总入口整段跳过 ----
// playerhurt(stat st, equipstat est, List<buffstat> bufflist, stat pst, int eid,
//            enemy e, bullet b, float demmul)  ⇒ 9 机器参数 + trailing methodInfo = 10 个形参。
// 无敌开：直接 return，不转调 orig（玩家完全不掉血；受击表现一并跳过，见实现文档已知副作用）。
static void Hk_PlayerHurt(void* self, void* st, void* est, void* bufflist, void* pst,
                          int eid, void* e, void* b, float demmul, void* mth) {
    if (!g_inv.load()) {
        if (g_origPlayerHurt) g_origPlayerHurt(self, st, est, bufflist, pst, eid, e, b, demmul, mth);
        return;
    }
    // 无敌：整段跳过（orig 不调）
}

// ---- 3) 伤害倍率：enemyhurt 的 dem 是 double（d0） ----
static double MulDem(double dem) {
    const float k = g_dmgMul.load();
    if (k <= 1.0001f) return dem;      // 1.0 = 关闭，不动数值
    const double out = dem * (double)k;
    return (out > 1.0e12) ? 1.0e12 : out;   // 防溢出成 inf 引发 hp 计算异常
}

// 2026-11-15 实测修正：enemyhurt = **敌人施加伤害（怪打人）**，不是敌人受伤。
// ⇒ 无敌挂这里：开关开时整段跳过（玩家不掉血，怪物输出不受影响）。
static void Hk_EnemyHurt(void* self, void* st, double dem, void* tf, UI2 add,
                         void* e, void* mth) {
    if (!g_inv.load()) {
        if (g_origEnemyHurt) g_origEnemyHurt(self, st, dem, tf, add, e, mth);
        return;
    }
    // 无敌：跳过
}

// enemyhurtnotnac 的 dem 是 float（s0）：同为「敌人施加伤害」，无敌一并拦。
static void Hk_EnemyHurtNN(void* self, void* st, float dem, void* tf, UI2 add,
                           void* e, void* mth) {
    if (!g_inv.load()) {
        if (g_origEnemyHurtNN) g_origEnemyHurtNN(self, st, dem, tf, add, e, mth);
        return;
    }
    // 无敌：跳过
}

// ---- 3b) 伤害倍率（玩家输出侧）：basicbase.nmhurte 0x1A3983C ----
// ⚠ 实测 ABI 与 dump 声明顺序不同（analyst 2026-11-15）：x1=st, x2=est, w3=eid, x4=额外指针，
//    s0=addx, s1=addy, s2=d, s3=dimmul。改 s3(dimmul)×k（0x1A398C8: fmul s0,s11,s10 = d×dimmul）。
//    它是 playerhurt 的唯一下游 ⇒ 只放大「玩家打出去的伤害」，不影响怪打人。
static void Hk_NmHurte(void* self, void* x1, void* x2, int32_t x3, void* x4,
                       float addx, float addy, float d, float dimmul, void* mth) {
    const float k = g_dmgMul.load();
    float dm = dimmul;
    if (k > 1.0001f) {
        dm = dimmul * k;
        if (dm > 1.0e6f) dm = 1.0e6f;
        if (dm < -1.0e6f) dm = -1.0e6f;
    }
    if (g_origNmHurte) g_origNmHurte(self, x1, x2, x3, x4, addx, addy, d, dm, mth);
}

// ---- 4) 移动速度：每次 move() 重设 stat.sp = base * k ----
// stat = *(void**)(player + 0x130)（player.stat），sp 在 stat + 0x9C（float）。
// base 在首次进入（或玩家实例变化）时抓取并缓存；开关每次打开时清缓存重新取基准。
static void* g_speedBaseStat = nullptr;
static float g_speedBase = 0.0f;

static void ResetSpeedBase() {
    g_speedBaseStat = nullptr;
    g_speedBase = 0.0f;
}

static void Hk_Move(void* self, void* mth) {
    if (g_moveSpeed.load() <= 1.0001f) {
        if (g_origMove) g_origMove(self, mth);
        return;
    }
    void* stat = OkPtr(self) ? *reinterpret_cast<void**>(reinterpret_cast<char*>(self) + kOffPlayerStat)
                             : nullptr;
    if (OkPtr(stat)) {
        if (g_speedBaseStat != stat || g_speedBase <= 0.0f) {
            const float cur = Il2Cpp::ReadF32(stat, kOffStatSp);
            if (cur > 0.0f) { g_speedBase = cur; g_speedBaseStat = stat; }
        }
        if (g_speedBase > 0.0f) {
            SafeWriteF32(stat, kOffStatSp, g_speedBase * g_moveSpeed.load());
        }
    }
    if (g_origMove) g_origMove(self, mth);
}

// ---- 5) 解锁所有皮肤：onelock.haveac() 恒 true ----
// 关：原样转调。orig 缺失：返回 false（保守，不假装解锁）。
static bool Hk_HaveAc(void* self, void* mth) {
    if (!g_skin.load()) return g_origHaveAc ? g_origHaveAc(self, mth) : false;
    return true;
}

// ---- 6) 无限解锁券：调 orig 后把余额回写成调用前的值（净扣减 0）----
// data = *(void**)(self + 0xB8)（实测 ldr x8,[x0,#0xB8]）；sd = *(void**)(data + 0x4F0)；
// unlockcoupon = *(int*)(sd + 0xBC)（唯一扣券点 0x1B213A4 写它）。
static void Hk_Coupon(void* self, void* mth) {
    if (!g_coupon.load()) {
        if (g_origCoupon) g_origCoupon(self, mth);
        return;
    }
    void* data = OkPtr(self) ? *reinterpret_cast<void**>(reinterpret_cast<char*>(self) + 0xB8) : nullptr;
    void* sd   = OkPtr(data) ? *reinterpret_cast<void**>(reinterpret_cast<char*>(data) + kOffSdField) : nullptr;
    const bool ok = OkPtr(sd);
    const int32_t before = ok ? SafeReadI32(sd, kOffUnlockCoupon, 0) : 0;

    // 2026-11-15 二测：余额为 0 时「还原原值」等于仍解锁失败（日志实测 restored -> 0）。
    // ⇒ 开关开时把余额**维持在 9999**（无限券），扣完再补，保证校验与扣减都过。
    if (ok) SafeWriteI32(sd, kOffUnlockCoupon, 9999);

    if (g_origCoupon) g_origCoupon(self, mth);

    if (ok && OkPtr(sd)) {
        SafeWriteI32(sd, kOffUnlockCoupon, 9999);
        LOGI(TPL_LOG " coupon kept at 9999 (was %d)", (int)before);
    }
}

// ---- 7) 免广告：**单点**拦 admanager.LoadRewardAd（2026-11-15 analyst 第四轮实测改口径）----
// 实测链路：getadfreebuy / getadexpmul / getadrevive 各自先写好 at(+0x50) 与 nowp/nows(+0x38/+0x40)，
//          然后**尾跳 b LoadRewardAd**（0x1ADED34）。⇒ 拦在 LoadRewardAd 时上下文已就绪，一处覆盖三项。
//          旧口径「hook 三个业务入口」作废：那三处 at 值也不同（freebuy=1 / xpmul=2 / revive=3，原假设 0/1/2 是错的）。
// 实现：开关开 → 不调 orig（跳过 SDK 加载），用同一个 this 直接调 reward(0x1ADF120)；开关关 → 原样调 orig。
// ★ 残余风险（analyst 明确未验证，写在这里防遗忘）：
//   ① reward 分支内部是否校验 canad / 每日次数 / nowp ——  未验证；
//   ② **revive 不写 nowp** ⇒ 复活分支可能拿不到上下文；
//   ③ 结论：首测只开「免广告购买」场景，复活/经验加成待实测再放开。
static const uintptr_t kRvaAdReward = 0x1ADF120 - kRvaVaddrBias;   // reward：vaddr - bias = 文件偏移
static void* g_adReward = nullptr;
static bool  g_adRewardTried = false;

static void CallReward(void* self, void* mth) {
    if (!g_adReward && !g_adRewardTried) {
        g_adRewardTried = true;
        g_adReward = FileOffToVa(TPL_TARGET_LIB, kRvaAdReward);
        if (g_adReward) {
            const uint32_t got = *reinterpret_cast<uint32_t*>(g_adReward);
            if (got != 0xF81E0FFE) {                       // reward 序言：str x30,[sp,#-0x20]!
                LOGW(TPL_LOG " reward 序言漂移 got=0x%08X — 放弃直调", got);
                g_adReward = nullptr;
            }
        }
    }
    if (g_adReward) {
        reinterpret_cast<Reward_t>(g_adReward)(self, mth);
        LOGI(TPL_LOG " 免广告：跳过 LoadRewardAd，直调 reward（at 由业务入口已写好）");
    } else {
        LOGW(TPL_LOG " reward 地址不可用 — 仅跳过 SDK 加载（不发奖）");
    }
}

static void Hk_AdLoadReward(void* self, void* mth) {
    if (!g_noad.load()) {
        if (g_origAdLoadReward) g_origAdLoadReward(self, mth);
        return;
    }
    const int32_t at = (OkPtr(self) ? SafeReadI32(self, kOffAdAt, -1) : -1);
    LOGI(TPL_LOG " AdLoad intercepted at=%d reward=%p", (int)at, g_adReward);
    CallReward(self, mth);      // 不调 orig：跳过 SDK；at/nowp 由调用方业务入口写好
    LOGI(TPL_LOG " AdLoad reward returned (at=%d)", (int)at);
}

// ---- 7b) 免广告：直接挂 UI 入口（2026-11-15 实测 LoadRewardAd 从未被调用）----
// 链路（实测）：UI 按钮 → 业务入口(尾跳) → LoadRewardAd → SDK 回调 → reward。
// 拦在 UI 入口最不易漏：开关开时跳过整段，直接调对应发奖函数。
typedef void (*NoArg_t2)(void*, void*);
static void Hk_AdBuy(void* self, void* mth) {
    if (!g_noad.load()) { if (g_origAdBuy) g_origAdBuy(self, mth); return; }
    static NoArg_t2 fn = nullptr;
    if (!fn) fn = reinterpret_cast<NoArg_t2>(FileOffToVa(TPL_TARGET_LIB, 0x1A66CF0 - kRvaVaddrBias));
    if (fn) { fn(self, mth); LOGI(TPL_LOG " ad buy -> freeperchase (skip SDK)"); }
    else { LOGW(TPL_LOG " freeperchase 地址不可用 — 转调 orig"); if (g_origAdBuy) g_origAdBuy(self, mth); }
}

static void Hk_AdExp(void* self, void* mth) {
    if (!g_noad.load()) { if (g_origAdExp) g_origAdExp(self, mth); return; }
    static AdExp_t fn = nullptr;
    if (!fn) fn = reinterpret_cast<AdExp_t>(FileOffToVa(TPL_TARGET_LIB, 0x1A6FDE8 - kRvaVaddrBias));
    if (fn) { fn(self, 2.0f, mth); LOGI(TPL_LOG " ad exp -> addexpmul(2.0) (skip SDK)"); }
    else { LOGW(TPL_LOG " addexpmul 地址不可用 — 转调 orig"); if (g_origAdExp) g_origAdExp(self, mth); }
}

// 复活：反汇编确认 reward 的 at=3 分支没有显式发奖函数（未确认落点）⇒ 本版保持原样，不假装支持。
static void Hk_AdRevive(void* self, void* mth) {
    if (g_noad.load()) LOGW(TPL_LOG " ad revive 未实现（发奖落点未确认），转调 orig");
    if (g_origAdRevive) g_origAdRevive(self, mth);
}

// ---- 8) 免广告：可用性检查恒 true（按钮常亮）----
// 2026-11-15 定案（analyst 静态穷举）：getadcheck 尾部是**无条件** b LoadRewardAd，
//   全库通往 Dirichlet SDK 的唯一入口就是这条链。
//   ⇒ 之前「开关开就直接 return true、不调 orig」等于把链切断（LoadRewardAd 永不触发）。
//   正确做法：照常调 orig（链继续走到 LoadRewardAd），由 Hk_AdLoadReward 在汇合点拦下并直接发奖。
static bool Hk_AdCheck(void* self, void* mth) {
    const bool r = g_origAdCheck ? g_origAdCheck(self, mth) : true;
    if (g_noad.load()) {
        LOGI(TPL_LOG " getadcheck passed-through (NO_AD on), chain→LoadRewardAd");
        return true;                       // 按钮保持可用，但不改变上游链路
    }
    return r;
}

// ---- 9) 只读探针（不改行为，只为确定「额外领取」点击后走哪条路）----
static void Hk_ProbeCheckBtn(void* self, void* mth) {
    LOGI(TPL_LOG " PROBE checkpanel.checkbutton fired");
    if (g_origProbeCheckBtn) g_origProbeCheckBtn(self, mth);
}
static void Hk_ProbeCheckAdBtn(void* self, void* mth) {
    LOGI(TPL_LOG " PROBE checkpanel.checkadbutton fired");
    if (g_origProbeCheckAdBtn) g_origProbeCheckAdBtn(self, mth);
}

// 六项功能 → 11 个槽（一个具体方法一个槽）。
//   rva      = 实测 vaddr - kRvaVaddrBias（文件偏移，见 §② 换算说明）
//   expectInsn = 该函数首条指令字（aarch64 小端 u32，取自已安装底包 libil2cpp.so）
//   ns/cls/method/argc = 按名回落（dump 里类命名空间为空，回退不保证成功；主路径是 RVA）
static const HookSlot kSlots[] = {
    // ① 无敌：player.hpcheck 保底 + 「敌人施加伤害」两个入口整段跳过
    {"InvChk",  0x1B06D38 - kRvaVaddrBias, 0xFC1D0FE8, "", "player",    "hpcheck",     1,
     (void*)Hk_HpCheck,   (void**)&g_origHpCheck},
    {"InvE",    0x1A3D37C - kRvaVaddrBias, 0xFC1B0FEA, "", "basicbase", "enemyhurt",   5,
     (void*)Hk_EnemyHurt, (void**)&g_origEnemyHurt},
    {"InvEN",   0x1A3D664 - kRvaVaddrBias, 0xFC1C0FEA, "", "basicbase", "enemyhurtnotnac", 5,
     (void*)Hk_EnemyHurtNN, (void**)&g_origEnemyHurtNN},
    // ② 伤害倍率：玩家输出侧 nmhurte 的 dimmul(s3) × k
    {"DmgMul",  0x1A3983C - kRvaVaddrBias, 0x0, "", "basicbase", "nmhurte",     8,
     (void*)Hk_NmHurte, (void**)&g_origNmHurte},
    // ③ 移速
    {"Speed",   0x1B07020 - kRvaVaddrBias, 0xFC180FEC, "", "player",    "move",        1,
     (void*)Hk_Move,    (void**)&g_origMove},
    // ④ 皮肤
    {"SkinUnl", 0x1ABBE1C - kRvaVaddrBias, 0xA9BE57FE, "", "onelock",   "haveac",      1,
     (void*)Hk_HaveAc,  (void**)&g_origHaveAc},
    // ⑤ 券：**不再 hook couponunlockachieve**（2026-11-15 实测：UnityEvent 以空 self 调用 ⇒
    //    我们调 orig 时游戏内部 ldr [x0,#0xb8] 读空指针，SIGSEGV fault addr 0xb8 闪退一次）。
    //    改为 Tick 数据型：直接维持 sd.unlockcoupon = 9999（见 Mxcz999_Tick）。
    // ⑥ 免广告：三个 UI 入口（LoadRewardAd 实测从未被调用，已弃）
    {"AdBuy",   0x1A66E2C - kRvaVaddrBias, 0x0, "", "perchaseui0", "getfreebuyad", 0,
     (void*)Hk_AdBuy, (void**)&g_origAdBuy},
    {"AdExpMul",0x1A6FD8C - kRvaVaddrBias, 0x0, "", "settlepanel", "adexpmul",     0,
     (void*)Hk_AdExp, (void**)&g_origAdExp},
    {"AdRevive",0x1A7937C - kRvaVaddrBias, 0x0, "", "uicontrol0",  "getrevivead",  0,
     (void*)Hk_AdRevive, (void**)&g_origAdRevive},
    {"AdCheck", 0x1ADF094 - kRvaVaddrBias, 0xA9BE57FE, "", "admanager", "getadcheck",  1,
     (void*)Hk_AdCheck, (void**)&g_origAdCheck},
    // 只读探针（onClick 绑定在预制体里，静态看不到 ⇒ 用一次日志确定）
    {"ProbeBtn",  0x1B3419C - kRvaVaddrBias, 0x0, "", "checkpanel", "checkbutton",   0,
     (void*)Hk_ProbeCheckBtn, (void**)&g_origProbeCheckBtn},
    {"ProbeAdBtn",0x1B3453C - kRvaVaddrBias, 0x0, "", "checkpanel", "checkadbutton", 0,
     (void*)Hk_ProbeCheckAdBtn, (void**)&g_origProbeCheckAdBtn},
    // 兜底：LoadRewardAd（若某条广告链不经三个 UI 入口，仍能在这里拦下并直接发奖）
    {"AdLoad",  0x1ADED34 - kRvaVaddrBias, 0xF81D0FFE, "", "admanager", "LoadRewardAd", 0,
     (void*)Hk_AdLoadReward, (void**)&g_origAdLoadReward},
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
            LOGW(TPL_LOG " slot[%d] %s 已熔断（装钩期致命信号），本次会话不再重试",
                 i, kSlots[i].tag);
        }
    }
    LOGI(TPL_LOG " install pass: ok=%d dead=%d/%d", ok, dead, kSlotsCount);
    if (ok > 0) g_anyHooked.store(true);
    return ok;
}

static bool AnyFeatureOn() {
    return g_inv.load() || g_skin.load() || g_coupon.load() || g_noad.load()
        || g_dmgMul.load() > 1.0001f || g_moveSpeed.load() > 1.0001f;
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
//  ⑧ 对外接口（ModEntry.h 注册表契约，签名不可改）
// ============================================================================
bool Mxcz999_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;

    InstallFaultGuard();          // §④ 先装兜底，再装钩
    g_libReady.store(true);
    SyncHooks();                  // 已 ON 的开关立即装；全关保持 0 hook
    LOGI(TPL_LOG " lib ready (dual-path, lazy hooks)");
    return true;
}

bool Mxcz999_IsReady() {
    return g_libReady.load();
}

void Mxcz999_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case MXCZ999_FEAT_INVINCIBLE:    g_inv.store(enabled); break;
        case MXCZ999_FEAT_UNLOCK_SKIN:   g_skin.store(enabled); break;
        case MXCZ999_FEAT_UNLOCK_COUPON: g_coupon.store(enabled); break;
        case MXCZ999_FEAT_NO_AD:         g_noad.store(enabled); break;
        case MXCZ999_FEAT_MOVE_SPEED:     // 滑杆开关态：打开时清移速基准缓存
            if (enabled) ResetSpeedBase();
            break;
        default: break;
    }
    SyncHooks();
}

void Mxcz999_OnValueChanged(int featNum, int value) {
    // 滑杆（getVal 路径）：框架 values[] 是 int，这里转 float 并钳制；1 = 等效关闭
    switch (featNum) {
        case MXCZ999_FEAT_DMG_MUL: {          // 1..100
            float k = (value < 1) ? 1.0f : (float)value;
            if (k > 100.0f) k = 100.0f;
            g_dmgMul.store(k);
            LOGI(TPL_LOG " dmg mul = %.2f", (double)k);
            break;
        }
        case MXCZ999_FEAT_MOVE_SPEED: {        // 1..5
            float k = (value < 1) ? 1.0f : (float)value;
            if (k > 5.0f) k = 5.0f;
            g_moveSpeed.store(k);
            ResetSpeedBase();                  // 倍率变化后基准重取，避免叠加写坏
            LOGI(TPL_LOG " move speed = %.2f", (double)k);
            break;
        }
        default: break;
    }
    SyncHooks();
}

// ★ 2026-11-15 静态定案（dump.cs 原文）：
//    data : MonoBehaviour
//      public static data d;             // 0x0   单例（字段名是 d，不是 i）
//      public static data.Savedata sd;   // 0x4F0 同样是**静态**字段（不是实例偏移！）
//    data.Savedata
//      public int unlockcoupon;          // 0xBC  券余额（实例字段）
//   ⇒ 必须全部走 il2cpp_field_static_get，绝不能 data+0x4F0 这种实例偏移读法。
static void* GetDataClass() {
    static void* k = nullptr;
    if (k) return k;
    if (!EnsureApi()) return nullptr;
    k = FindClassNs("", "data");
    if (!k) k = GetApi().FindClassByShortName("data");
    return k;
}

static void* GetStaticFieldObj(const char* fieldName) {
    void* klass = GetDataClass();
    if (!klass) return nullptr;
    if (!GetApi().class_get_field || !GetApi().field_static_get) return nullptr;
    void* fld = GetApi().class_get_field(klass, fieldName);
    if (!fld) return nullptr;
    void* val = nullptr;
    GetApi().field_static_get(fld, &val);
    return OkPtr(val) ? val : nullptr;
}

static void* GetDataSingleton() { return GetStaticFieldObj("d"); }

static void* GetSavedata() { return GetStaticFieldObj("sd"); }

// 无限券（数据型）：sd = data+0x4F0，unlockcoupon = sd+0xBC，维持 9999。
static void KeepCouponFull() {
    static bool probed = false;
    void* sd = GetSavedata();
    if (!probed) {
        probed = true;
        LOGI(TPL_LOG " coupon probe: data=%p sd=%p coupon=%d", GetDataSingleton(), sd,
             sd ? (int)SafeReadI32(sd, kOffUnlockCoupon, -1) : -1);
    }
    if (!OkPtr(sd)) return;
    const int32_t cur = SafeReadI32(sd, kOffUnlockCoupon, -1);
    if (cur != 9999) {
        SafeWriteI32(sd, kOffUnlockCoupon, 9999);
        LOGI(TPL_LOG " coupon kept at 9999 (was %d) [tick]", (int)cur);
    }
}

void Mxcz999_Tick(const bool* features, const int* values) {
    if (!features) return;

    // 稳态同步（权威来源是 Java 侧开关/数值；Tick 只做兜底对账，不做游戏内存读写）
    g_inv.store(features[MXCZ999_FEAT_INVINCIBLE]);
    g_skin.store(features[MXCZ999_FEAT_UNLOCK_SKIN]);
    g_coupon.store(features[MXCZ999_FEAT_UNLOCK_COUPON]);
    g_noad.store(features[MXCZ999_FEAT_NO_AD]);
    if (values) {
        const int dm = values[MXCZ999_FEAT_DMG_MUL];
        const int ms = values[MXCZ999_FEAT_MOVE_SPEED];
        g_dmgMul.store(dm < 1 ? 1.0f : (dm > 100 ? 100.0f : (float)dm));
        const float nms = ms < 1 ? 1.0f : (ms > 5 ? 5.0f : (float)ms);
        if (nms != g_moveSpeed.load()) { g_moveSpeed.store(nms); ResetSpeedBase(); }
    }

    // 兜底：库晚于首帧映射时，这里补一次
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) {
        InstallFaultGuard();
        g_libReady.store(true);
        LOGI(TPL_LOG " lib ready (via Tick fallback)");
    }
    SyncHooks();

    // 无限券：数据型维持（每 tick 一次 int 写，代价可忽略；不再是 hook）
    if (g_coupon.load()) KeepCouponFull();

    // ★ 禁止在此 SafeProbe / 扫堆 / 遍历对象 / 逐对象写（Tick 频率高，代价爆炸）
}

void Mxcz999_ForceClear() {
    g_inv.store(false);
    g_skin.store(false);
    g_coupon.store(false);
    g_noad.store(false);
    g_dmgMul.store(1.0f);
    g_moveSpeed.store(1.0f);
    g_reenter.store(false);
    ResetSpeedBase();
    LOGI(TPL_LOG " forceClear: 六项全部复位（含已写入的 stat.sp 基准缓存）");
}
