// ============================================================================
//  疯狂水世界（com.gx.sw.qa.fkssj004.esj 1.11.6 / vc480，TapTap 渠道原版）
//  —— 星光菜单游戏模块（modern 注册表，tag=Fkssj / game_id=fkssj）
//
//  引擎：Cocos Creator 3.x + JSB（libcocos.so），**无 IL2CPP、无整包加固壳**
//  底包：真机 96f0d883 只读拉取（md5 f8ed1b95…），已登记 _state/底包再获取台账.md
//
//  ★阶段：① 过环境（用户 m00001 明令「先不要直接做功能，先过环境」）
//    本轮产物 = **零 hook 环境版**：只保证菜单能组装进游戏、游戏能正常玩。
//    【无敌】【伤害倍率】【战斗加速】三功能留 §⑤/⑥/⑦ TODO，待用户放行口令。
//
//  ⚠️ 防闪退设施已内置为代码，删任何一节都可能复现历史闪退：
//    默认 0 hook（懒装）/ 幂等安装 / orig 必调 / reenter 防递归
//    §④ 致命信号兜底（sigsetjmp 保护区 + sigaltstack 备用栈 + 装钩互斥 + 槽位熔断）
//
//  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
// ============================================================================

#include "FkssjMod.h"

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
// 生成器把 [Fkssj] 替换为本游戏标签（如 [jfqs]），日志过滤用 adb logcat -s Mod_Menu
#define TPL_LOG "[Fkssj]"

// 注册表模式不依赖 GameConfig.h（legacy 配置中心）。
// Cocos 系生成后把下面这处（含各 hook 内引用）统一换成目标库名。
#define TPL_TARGET_LIB "libcocos.so"

// ============================================================================
//  ① 开关与 orig 指针
//     g_libReady = 「库已映射」≠「已挂钩」；全开关关闭时应当保持 0 个 hook。
// ============================================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_inv{false};    // Toggle_无敌
static std::atomic<int>  g_dmgMul{1};     // Slider_伤害倍率（1 = 关闭）
static std::atomic<int>  g_spdMul{1};     // Slider_战斗加速（1 = 关闭）
static std::atomic<bool> g_reenter{false};

typedef void (*TakeDamage_t)(void* self, void* damageData, void* method);
static TakeDamage_t g_origTakeDamage = nullptr;

// ============================================================================
//  ② 字段偏移与 RVA
//     TODO：全部来自 dump.cs / IDA / Trace 实测，禁止猜；换包必须重 dump。
// ============================================================================
enum : size_t {
    kOffHurtValue = 0x30,   // 示例：伤害字段偏移（本游戏实测后改）
};

// ============================================================================
//  ③ 安全助手（防闪退核心；新代码禁止绕开它们裸读写内存）
// ============================================================================
static bool OkPtr(void* p) { return p != nullptr; }

static void SafeWriteI32(void* obj, size_t off, int32_t v) {
    if (!OkPtr(obj)) return;
    std::memcpy(reinterpret_cast<uint8_t*>(obj) + off, &v, sizeof(v));
}

static void SafeWriteF32(void* obj, size_t off, float v) {
    if (!OkPtr(obj)) return;
    std::memcpy(reinterpret_cast<uint8_t*>(obj) + off, &v, sizeof(v));
}

static int32_t SafeReadI32(void* obj, size_t off, int32_t defv) {
    if (!OkPtr(obj)) return defv;
    int32_t r = defv;
    std::memcpy(&r, reinterpret_cast<const uint8_t*>(obj) + off, sizeof(r));
    return r;
}

// 注：模板的 ClassNameCache / CacheClassName（依赖 il2cpp object_class+class_name）
// 在 Cocos 系不可用，已删除；阶段② 若需类名判断改走 JNI 侧 ModBridge。

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
//  ⑤/⑥/⑦ 挂点机器与功能实现 —— 阶段①（过环境）留空
//
//  用户 m00001 明令「先不要直接做功能，先过环境」⇒ 产物是**零 hook 环境版**：
//  InstallBattleHooks() 恒装 0 个钩子，开关置 ON 只写日志 + 状态镜像。
//
//  阶段②（用户明确放行后）在此恢复三件：
//    §⑤ 挂点机器：Cocos 系**无 il2cpp 类/方法 API** ⇒ 按名路径不可用，只走
//        RVA 路径（HookAtRva + 序言校验；file_off → VA 用 FileOffToVa 扫
//        /proc/self/maps 只取可执行段并取最大 span，禁 base+RVA 盲算）。
//    §⑥ hook 函数与挂点表 kSlots[]：【无敌】【伤害倍率】【战斗加速】。
//    §⑦ 分组安装 + 懒装（全开关关 = 0 hook，已在此保留不动）。
//
//  ★锚点来源优先级：本作 JS 层**明文**（assets/src/ 394 个 .js，无 .jsc）⇒
//    优先走 Cocos JavascriptJavaBridge / CocosJsBridge 运行期 JS 补丁，
//    而非 native RVA 挂钩（同构先例：仙逆 0.0.14，见 10_docs/仙逆/*）。
// ============================================================================
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

// 阶段①：挂点机器整体未启用 ⇒ 任何槽位都直接判「不装」（不触碰目标内存）。
static bool HookSlotInstall(const HookSlot& s) {
    LOGI(TPL_LOG " slot %s skipped (zero-hook env build)", s.tag);
    return false;
}

// TODO(阶段②)：实测后回填三功能锚点；现在留空 ⇒ kSlotsCount = 0，恒装 0 个钩。
static const HookSlot kSlots[] = {};

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
    // 任一项非默认即视为开启（阶段① 无实现，只影响是否尝试装钩 ⇒ kSlots 空 ⇒ 恒 0）
    return g_inv.load() || g_dmgMul.load() > 1 || g_spdMul.load() > 1;
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
bool Fkssj_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;

    InstallFaultGuard();          // §④ 先装兜底（阶段① 唯一的 native 副作用）
    g_libReady.store(true);
    SyncHooks();                  // kSlots 为空 ⇒ 恒装 0 个钩
    LOGI(TPL_LOG " lib ready (cocos jsb, zero-hook env build)");
    return true;
}

bool Fkssj_IsReady() {
    return g_libReady.load();
}

void Fkssj_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case FKSSJ_FEAT_INV:
            g_inv.store(enabled);
            LOGI(TPL_LOG " 无敌 %s（阶段① 仅记录状态，无实现）", enabled ? "ON" : "OFF");
            break;
        case FKSSJ_FEAT_DMG:
            g_dmgMul.store(enabled ? 2 : 1);
            LOGI(TPL_LOG " 伤害倍率 %s", enabled ? "ON" : "OFF");
            break;
        case FKSSJ_FEAT_SPD:
            g_spdMul.store(enabled ? 2 : 1);
            LOGI(TPL_LOG " 战斗加速 %s", enabled ? "ON" : "OFF");
            break;
        default: break;
    }
    SyncHooks();
}

void Fkssj_OnValueChanged(int featNum, int value) {
    // 滑块类功能在此钳制并存入原子量（语义：1 = 关闭）
    if (value < 1) value = 1;
    switch (featNum) {
        case FKSSJ_FEAT_DMG:
            g_dmgMul.store(value);
            LOGI(TPL_LOG " 伤害倍率 值=%d", value);
            break;
        case FKSSJ_FEAT_SPD:
            g_spdMul.store(value);
            LOGI(TPL_LOG " 战斗加速 值=%d", value);
            break;
        default: break;
    }
}

void Fkssj_Tick(const bool* features, const int* /*values*/) {
    if (!features) return;

    g_inv.store(features[FKSSJ_FEAT_INV]);
    if (!features[FKSSJ_FEAT_DMG]) g_dmgMul.store(1);   // 滑块关 ⇒ 复位
    if (!features[FKSSJ_FEAT_SPD]) g_spdMul.store(1);

    // 兜底：库晚于首帧映射时，这里补一次
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) {
        InstallFaultGuard();
        g_libReady.store(true);
        LOGI(TPL_LOG " lib ready (via Tick fallback)");
    }
    SyncHooks();

    // ★ 禁止在此 SafeProbe / 扫堆 / 遍历对象 / 逐对象写（Tick 频率高，代价爆炸）
}

void Fkssj_ForceClear() {
    g_inv.store(false);
    g_dmgMul.store(1);
    g_spdMul.store(1);
    g_reenter.store(false);
}
