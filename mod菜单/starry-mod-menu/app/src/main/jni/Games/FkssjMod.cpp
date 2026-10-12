// ============================================================================
//  疯狂水世界（com.gx.sw.qa.fkssj004.esj 1.11.6 / vc480，TapTap 渠道原版）
//  —— 星光菜单游戏模块（modern 注册表，tag=Fkssj / game_id=fkssj）
//
//  引擎：Cocos Creator 3.x + JSB（libcocos.so），**无 IL2CPP、无整包加固壳**
//  底包：真机 96f0d883 只读拉取（md5 f8ed1b95…），已登记 _state/底包再获取台账.md
//
//  ★阶段：① 过环境**已达成**（用户上手验收「菜单正常，游戏能正常玩」）
//         ② 功能实现（用户口令「开始做功能」已下）——本文件 §⑨ JS 桥：
//    【无敌】【伤害倍率】【战斗加速】三功能全部落在 **运行时 JS 补丁**（native 仍 0 hook）。
//    ⚠️ 为什么必须走 JS：包内 assets/** 全是 NHPK 密文（不能静态改、也不能往包里塞 JS），
//       且 libcocos.so 的 dynsym 全为混淆垃圾名（无 evalString 可挂）⇒ 只能：
//       native 线程 evaluateString 投补丁 → 补丁在 SystemJS 模块层打锚点。
//    锚点（全部经解密后 JS 实证，见 10_docs/疯狂水世界/阶段②_锚点与JS补丁.md）：
//       伤害管线唯一汇聚点 = BattleUtils.applyDamage(battle,target,dmg,flag)（返回实际掉血）
//       死亡判定 = AIUnit.checkDeath()（camp===0 我方）
//       推帧/播放 = BattleInterfaceBase.update(dt)（帧推进 _delayTime >= FRAME_TIME/playSpeed）
//       阵营常量：ATTACK_CAMP=MY_CAMP=0（我方） / DEFEND_CAMP=ENEMY_CAMP=1（敌方）
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

#include <pthread.h>  // §⑨ 抓解密脚本线程
#include <unistd.h>   // §⑨ sleep
#include <mutex>      // §④ 装钩互斥
#include <setjmp.h>   // §④ sigsetjmp 保护区
#include <signal.h>   // §④ 致命信号兜底

#include <jni.h>      // §⑨ evalString 反射调用

#include "Dobby/dobby.h"
#include "Includes/obfuscate.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
// 生成器把 [Fkssj] 替换为本游戏标签（如 [jfqs]），日志过滤用 adb logcat -s Mod_Menu
#define TPL_LOG "[Fkssj]"

// 注册表模式不依赖 GameConfig.h（legacy 配置中心）。
// Cocos 系生成后把下面这处（含各 hook 内引用）统一换成目标库名。
#define TPL_TARGET_LIB "libcocos.so"

// 阶段② 抓解密脚本：0 = 关（2026-10-12 已抓齐 1912 模块 / 1.5MB bundle 明文，存
//   09_test/疯狂水世界/jsdump/）。游戏更新后锚点漂移、需重新取证时改 1 重编即可。
#define FKSSJ_JS_DUMP 0

// JS 桥节奏：补丁重挂周期（秒）。cfg 仅在数值变化时推送（见 FkssjBridgeThread）。
#define FKSSJ_BRIDGE_INTERVAL_SEC 2

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
//  ⑨ 运行期取「已解密游戏脚本」（阶段② 第一步：解锁 JS 锚点）
//
//  背景：本作 assets/ 下 394 个 .js 全是 NHPK 魔数密文（含 assets/src/chunks/bundle.js、
//  assets/src/cocos-js/cc.js、settings.json），libcocos.so 的 dynsym 3720 项全是
//  混淆垃圾名（无可按名挂载符号），NHPK 解密器只在那 28MB 的 .note.gnu.proc 里
//  ⇒ 静态解不了、native 挂不上。
//
//  实测通道（**无 root、无 frida**）：应用自己把明文落到
//  /sdcard/Android/data/com.gx.sw.qa.fkssj004.esj/files/，adb 直接 pull。
//    · 该目录对 shell 可读写（实测 echo/cat 均通），属主 u0_a265 ext_data_rw；
//    · phantom-frida 需要 root（本机无），故不采用。
//
//  做法照抄同构先例 Games/XnbzMod.cpp（仙逆 0.0.14）：
//    a) 直读：jsb.fileUtils.getStringFromFile("src/chunks/bundle.js") 等候选路径；
//       若引擎的 NHPK 解密发生在 getStringFromFile 内，这就是**完整原文**。
//    b) 兜底：走 SystemJS 模块注册表（System.registerRegistry / S.get(name)），
//       用 Function.prototype.toString 取各模块 declare 源码（模块名形如
//       chunks:///_virtual/*.ts），再整包落盘。
//
//  ⚠️ 目录顺序必须「外部存储优先」：jsb.fileUtils.getWritablePath() 返回的是
//     /data/user/0/<pkg>/files/（无 root 的 adb **读不到**），只有外部目录才是取证通道。
//  ⚠️ 催抓必须独立线程：框架 Tick 只在 session_live() 授权通过后才跑。
// ============================================================================
static JavaVM*   g_jvm       = nullptr;
static jclass    g_hooksCls  = nullptr;
static jmethodID g_hooksPump = nullptr;

// 只在 JNI_OnLoad 的 Java 栈帧内解析（native 线程 FindClass 解析不到应用类）
void Fkssj_OnLoad(JavaVM* vm) {
    if (vm) g_jvm = vm;
    if (g_hooksCls) return;
    if (!vm) { LOGW(TPL_LOG " onLoad: vm null"); return; }
    JNIEnv* env = nullptr;
    if (vm->GetEnv((void**)&env, JNI_VERSION_1_6) != JNI_OK || !env) {
        LOGW(TPL_LOG " onLoad: no env（非 Java 栈帧）");
        return;
    }
    jclass c = env->FindClass(OBFUSCATE("com/starry/magic/cocos/FkssjHooks"));
    if (!c) {
        env->ExceptionClear();
        LOGW(TPL_LOG " onLoad: FindClass FkssjHooks 失败");
        return;
    }
    jmethodID m = env->GetStaticMethodID(c, "pump", "(Ljava/lang/String;)V");
    if (!m) {
        env->ExceptionClear();
        env->DeleteLocalRef(c);
        LOGW(TPL_LOG " onLoad: pump 方法 ID 获取失败");
        return;
    }
    g_hooksCls  = (jclass)env->NewGlobalRef(c);
    g_hooksPump = m;
    env->DeleteLocalRef(c);
    LOGI(TPL_LOG " onLoad: FkssjHooks 已缓存（classloader ok）");
}

// 同步互斥 + 异常安全地执行一段 JS（走框架 CocosJsBridge，内部投递到游戏线程）。
// 任何失败只打日志，绝不抛出到调用方（Tick/UI 线程都不能被拖崩）。
static void EvalJs(const char* js) {
    if (!g_jvm || !js) { LOGW(TPL_LOG " eval skip: jvm=%p js=%p", (void*)g_jvm, (const void*)js); return; }
    bool detach = false;
    JNIEnv* env = nullptr;
    int st = g_jvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_jvm->AttachCurrentThread(&env, nullptr) != 0) { LOGW(TPL_LOG " eval skip: attach failed"); return; }
        detach = true;
    } else if (st != JNI_OK || !env) {
        LOGW(TPL_LOG " eval skip: GetEnv st=%d", st);
        return;
    }
    static std::atomic<int> s_callSeq{0};
    int seq = s_callSeq.fetch_add(1);

    jclass cls = g_hooksCls;
    jmethodID mid = g_hooksPump;
    bool localCls = false;
    if (!cls || !mid) {
        jclass lc = env->FindClass(OBFUSCATE("com/starry/magic/cocos/FkssjHooks"));
        if (!lc) {
            env->ExceptionClear();
            if (seq < 3) LOGW(TPL_LOG " eval#%d FindClass FkssjHooks FAIL（classloader）", seq);
            if (detach) g_jvm->DetachCurrentThread();
            return;
        }
        cls = lc;
        localCls = true;
        mid = env->GetStaticMethodID(cls, "pump", "(Ljava/lang/String;)V");
        if (!mid) {
            env->ExceptionClear();
            if (seq < 3) LOGW(TPL_LOG " eval#%d GetStaticMethodID pump FAIL", seq);
            env->DeleteLocalRef(lc);
            if (detach) g_jvm->DetachCurrentThread();
            return;
        }
    }
    jstring jjs = env->NewStringUTF(js);
    if (jjs) {
        if (seq < 3) LOGI(TPL_LOG " eval#%d call pump (len=%d)", seq, (int)strlen(js));
        env->CallStaticVoidMethod(cls, mid, jjs);
        if (env->ExceptionCheck()) {
            env->ExceptionClear();
            LOGW(TPL_LOG " eval#%d pump threw", seq);
        }
        env->DeleteLocalRef(jjs);
    }
    if (localCls) env->DeleteLocalRef(cls);
    if (detach) g_jvm->DetachCurrentThread();
}

// 抓取已解密游戏脚本（幂等；可重复 eval，内部有版本守卫）
// 仅在 FKSSJ_JS_DUMP=1 时编入，避免日常包带无用载荷 / 触发 unused 告警。
#if FKSSJ_JS_DUMP
static const char* kDumpJs = R"JS(
(function(){
  var g=(typeof globalThis!=="undefined")?globalThis:((typeof window!=="undefined")?window:this);
  var S=(typeof System!=="undefined")?System:null;
  var PKG="com.gx.sw.qa.fkssj004.esj";
  var EXT="/sdcard/Android/data/"+PKG+"/files/";
  function back(m){
    try{ jsb.reflection.callStaticMethod("com/starry/magic/cocos/FkssjHooks","logStatus","(Ljava/lang/String;)V","fkssj "+m); }catch(e){}
  }
  var dirs=[EXT];                       // ★ 外部目录优先（adb 取证通道）
  try{ var wp=jsb.fileUtils.getWritablePath(); if(wp&&dirs.indexOf(wp)<0) dirs.push(wp); }catch(e){}
  function wf(name,txt){
    for(var i=0;i<dirs.length;i++){
      try{ if(jsb.fileUtils.writeStringToFile(txt, dirs[i]+name)) return dirs[i]+name; }catch(e){}
    }
    return "";
  }
  var st=g.__fkssjStatus||(g.__fkssjStatus=[]);
  function say(m){
    try{ st.push(m); if(st.length>400) st.shift(); }catch(e){}
    try{ console.log("[FkssjDump] "+m); }catch(e){}
    try{ wf("fkssj_status.txt", st.join("\n")+"\n"); }catch(e){}
  }
  if(!g.__fkssjSaid){
    g.__fkssjSaid=1;
    say("inject dirs="+dirs.join(",")+" hasSystem="+(!!S)+" hasJsb="+(typeof jsb!=="undefined"));
    if(typeof jsb==="undefined"){ return; }
  }
  // ---- A) 直读已解密文件 ----
  if(!g.__fkssjRaw){
    g.__fkssjRaw=1;
    var cands=["src/chunks/bundle.js","src/cocos-js/cc.js","src/settings.json",
               "src/import-map.json","src/system.bundle.js","jsb-adapter/web-adapter.js",
               "jsb-adapter/engine-adapter.js","application.js","main.js"];
    for(var i=0;i<cands.length;i++){
      var p=cands[i], s=null;
      try{ s=jsb.fileUtils.getStringFromFile(p); }catch(e){ s=null; }
      if(s&&s.length>0){
        var head=s.substring(0,12).replace(/[^\x20-\x7e]/g,".");
        var fn="fkssj_raw_"+p.replace(/\//g,"_");
        var wrote=wf(fn,s);
        var rb="";
        try{ var rr=jsb.fileUtils.getStringFromFile(wrote); rb=" readback="+(rr?rr.length:-1); }catch(e){}
        say("RAW "+p+" len="+s.length+" head="+head+" -> "+wrote+rb);
        back("RAW "+p+" len="+s.length+" head="+head);
      } else { say("RAW "+p+" miss"); }
    }
    try{ say("writable="+jsb.fileUtils.getWritablePath()); }catch(e){}
    try{ say("fullpath bundle="+jsb.fileUtils.fullPathForFilename("src/chunks/bundle.js")); }catch(e){}
  }
  if(!S) return;
  if(g.__fkssjDumpVer>=3) return;
  g.__fkssjDumpVer=3;
  var store=g.__fkssjMods||(g.__fkssjMods={});
  function safeStr(f){ try{ return Function.prototype.toString.call(f); }catch(e){ return ""; } }
  function put(name,src){
    if(!name) return;
    if(typeof src==="string"&&src.length>32) store[name]=src;
    else if(!(name in store)) store[name]="";
  }
  var names=[];
  try{ var it=S.entries(),c; while(!(c=it.next()).done) names.push(c[0]); }catch(e){ say("entries err "+e); }
  if(names.length>0) g.__fkssjNames=names;
  else if(g.__fkssjNames) names=g.__fkssjNames;
  say("modules="+names.length);
  var nreg=0;
  try{
    var R=S.registerRegistry;
    if(R){ for(var k in R){ var v=R[k]; if(v&&typeof v[1]==="function"){ put(k,safeStr(v[1])); nreg++; } } }
  }catch(e){ say("reg err "+e); }
  var walked=0;
  for(var m=0;m<names.length;m++){
    try{
      var ns=S.get(names[m]); if(!ns) continue;
      var keys=Object.keys(ns), parts=[], got=0;
      for(var j=0;j<keys.length;j++){
        var vv; try{ vv=ns[keys[j]]; }catch(e2){ continue; }
        if(typeof vv==="function"){ var sv=safeStr(vv); if(sv){ parts.push("// ns."+keys[j]+"\n"+sv); got++; } }
      }
      if(got>0&&!store[names[m]]){ store[names[m]]="///// NAMESPACE "+names[m]+" /////\n"+parts.join("\n"); walked++; }
    }catch(e3){}
  }
  if(!g.__fkssjRegHooked&&typeof S.register==="function"){
    var orig=S.register;
    S.register=function(name,deps,declare){
      try{ var d=(typeof deps==="function")?deps:declare; if(typeof d==="function") put(name,safeStr(d)); }catch(e){}
      return orig.apply(this,arguments);
    };
    g.__fkssjRegHooked=1;
  }
  say("reg="+nreg+" walked="+walked+" hooked="+!!g.__fkssjRegHooked);
  back("reg="+nreg+" walked="+walked+" mods="+names.length);
  var rounds=0;
  function flush(){
    rounds++;
    try{
      var out=["///// MODULE NAMES ("+names.length+") /////\n"+names.join("\n")+"\n"];
      var cnt=0,bytes=0;
      for(var k2 in store){ var v2=store[k2]; if(typeof v2!=="string"||!v2) continue; out.push("///// MOD "+k2+" /////\n"+v2); cnt++; bytes+=v2.length; }
      var p2=wf("fkssj_dump.js", out.join("\n"));
      try{ wf("fkssj_modlist.txt", "MODULE NAMES ("+names.length+")\n"+names.join("\n")+"\n"); }catch(e){}
      say("flush#"+rounds+" mods="+cnt+" bytes="+bytes+" path="+p2);
      back("flush#"+rounds+" mods="+cnt+" bytes="+bytes+" path="+p2);
    }catch(e){ say("flush err "+e); }
  }
  flush();
  var t=setInterval(function(){ flush(); if(rounds>=12) clearInterval(t); },3000);
})();
)JS";

// 独立催抓线程：不依赖框架 Tick 循环（Tick 只在 session_live() 授权通过后才跑）
static std::atomic<bool> g_dumpThreadStarted{false};

static void* FkssjDumpThread(void*) {
    LOGI(TPL_LOG " dump thread start (1s x 600)");
    for (int i = 0; i < 600; i++) {
        EvalJs(kDumpJs);
        if ((i % 60) == 0) LOGI(TPL_LOG " dump pump i=%d", i);
        sleep(1);
    }
    LOGI(TPL_LOG " dump thread end");
    return nullptr;
}

static void StartDumpThread() {
    if (g_dumpThreadStarted.exchange(true)) return;
    pthread_t tid;
    if (pthread_create(&tid, nullptr, FkssjDumpThread, nullptr) == 0) {
        pthread_detach(tid);
    } else {
        LOGW(TPL_LOG " dump thread create failed");
    }
}
#endif  // FKSSJ_JS_DUMP

// ============================================================================
//  §⑨-b 三功能补丁（幂等，可无限重复 eval）
//
//  锚点全部来自对**解密后游戏 JS**的静态确认（09_test/疯狂水世界/jsdump/）：
//   ① 伤害汇聚点 BattleUtils.applyDamage(e=battle, t=target, r=dmg, s=flag)
//        let n=t.data.getValue(Hp).asInt(), i=n-r; … t.data.setValue(Hp,i); return n;
//      ⇒ r 是普通数字、返回值是**实际掉血量**；r=0 时 HP 不变且 return 0（连受击事件也不派发）。
//      ⇒ 我方阵营 c===0（ATTACK_CAMP=MY_CAMP=0）：dmg=0            → 【无敌】
//        敌方阵营 c===1（DEFEND_CAMP=ENEMY_CAMP=1）：dmg*=倍率      → 【伤害倍率】
//      ⚠️ DamageEffect 里的「单次伤害上限」PerLimitDmg 是在调用**之前**钳的
//      （i=Math.min(MaxHp*PerLimitDmg, i)）⇒ 在 applyDamage 里乘倍率可以绕过该上限。
//   ② 死亡判定 AIUnit.checkDeath() = statusCheck(DEATH) || Hp<=0
//      ⇒ 我阵营（this.camp===0）恒 false ⇒ 【无敌】双保险（血量被别的路径写 0 也不死）。
//      ⚠️ GameBattle.checkWin() 只看**敌方**单位；checkFail() 只看我方 ⇒ 我方恒不死 =
//         只会赢不会输，同时敌方仍会被我们正常打死（伤害链未受影响）。
//   ③ 推帧 BattleInterfaceBase.update(dt)：
//        _delayTime += t; while(_delayTime >= FRAME_TIME/playSpeed){ …; battle.update(); … }
//      ⇒ 传入 dt 乘倍率即可整体加速（推进与播放同一循环，动画/结算同步加速）→【战斗加速】
//      ⚠️ 该类的 playSpeed 在构造时被 a.clamp(defaultBattleSpeed(),1,1.5) 钳死成 ≤1.5，
//         所以不改 playSpeed，只放大 dt。
//
//  另记：游戏自带 EffectType.INVINCIBLE 状态（DamageInmunityEffect），但走效果表配置成本高，
//        且要求目标身上真的挂上该 buff；直接在汇聚点置 0 更稳。
// ============================================================================
static const char* kPatchJs = R"JS(
(function(){
 try{
  var g=(typeof globalThis!=="undefined")?globalThis:this;
  var F=g.__fkssj||(g.__fkssj={});
  if(!F.cfg) F.cfg={inv:0,dmg:1,spd:1};
  if(!F.stat) F.stat={};
  var S=(typeof System!=="undefined")?System:null;
  if(!S||typeof S.get!=="function") return;
  function mod(n){ try{ return S.get("chunks:///_virtual/"+n+".ts"); }catch(e){ return null; } }
  function back(m){
    try{ jsb.reflection.callStaticMethod("com/starry/magic/cocos/FkssjHooks","logStatus","(Ljava/lang/String;)V","fkssj-js "+m); }catch(e){}
  }

  // ① + ② 伤害汇聚点
  var bu=mod("BattleUtils");
  if(bu&&bu.BattleUtils&&typeof bu.BattleUtils.applyDamage==="function"&&F.hookBU!==bu.BattleUtils.applyDamage){
    var o1=bu.BattleUtils.applyDamage;
    bu.BattleUtils.applyDamage=function(battle,target,dmg,flag){
      try{
        var c=(target&&target.camp);
        if(F.cfg.inv&&c===0){ dmg=0; F.stat.inv=(F.stat.inv||0)+1; }
        else if(c===1&&F.cfg.dmg>1){ dmg=Math.floor(dmg*F.cfg.dmg); F.stat.dmg=(F.stat.dmg||0)+1; }
      }catch(e){}
      return o1.call(this,battle,target,dmg,flag);
    };
    F.hookBU=bu.BattleUtils.applyDamage; F.stat.bu=(F.stat.bu||0)+1;
    back("anchor#1 applyDamage hooked");
  }
  // ② 死亡判定双保险
  var ai=mod("AIUnit");
  if(ai&&ai.AIUnit&&ai.AIUnit.prototype&&typeof ai.AIUnit.prototype.checkDeath==="function"&&F.hookAI!==ai.AIUnit.prototype.checkDeath){
    var o2=ai.AIUnit.prototype.checkDeath;
    ai.AIUnit.prototype.checkDeath=function(){
      try{ if(F.cfg.inv&&this.camp===0) return false; }catch(e){}
      return o2.call(this);
    };
    F.hookAI=ai.AIUnit.prototype.checkDeath; F.stat.ai=(F.stat.ai||0)+1;
    back("anchor#2 checkDeath hooked");
  }
  // ③ 战斗加速
  var bib=mod("BattleInterfaceBase");
  if(bib&&bib.BattleInterfaceBase&&bib.BattleInterfaceBase.prototype&&typeof bib.BattleInterfaceBase.prototype.update==="function"&&F.hookBIB!==bib.BattleInterfaceBase.prototype.update){
    var o3=bib.BattleInterfaceBase.prototype.update;
    bib.BattleInterfaceBase.prototype.update=function(dt){
      var s=(F.cfg.spd>1?F.cfg.spd:1);
      return o3.call(this,(typeof dt==="number")?dt*s:dt);
    };
    F.hookBIB=bib.BattleInterfaceBase.prototype.update; F.stat.spd=(F.stat.spd||0)+1;
    back("anchor#3 battle speed hooked");
  }
  // ④ 战斗结束取证（**只读**，不改任何行为）
  //   目的：把「客户端模拟结果 vs 服务端结果 vs 实际弹窗依据」逐场钉进 logcat。
  //   ★分模式事实（对解密 JS 静态确认）：
  //     · 主线(BT_MAINLINE) 的弹窗读 **服务端** serverData.stats.isLeftWin
  //       （StageMainLineBattleInterface._showBattleResult），关卡推进也在响应回调里先做完。
  //     · 其余模式（英雄副本/试炼/PvE/世界BOSS/竞技场/联赛/锦标赛/城战/远征/推荐/Ultra…）
  //       的弹窗读 **客户端** battle.victorCamp == MY_CAMP(0)。
  //   ⇒ 这一行日志是判定「我们的补丁在哪个模式真的改到了结算」的唯一机器证据。
  var btName={1:"MAINLINE",2:"ARENA",3:"TOWER",4:"LADDER",5:"WORLD_BOSS",6:"WORLD_RES",7:"RECOMMEND",9:"HERO_RAID",10:"TRIAL",11:"TRIAL_RAID",12:"PIRATE",13:"CITY_WAR",14:"RES_FIELD",15:"TREASURE_LOOT",16:"RADAR",17:"LEAGUE",18:"EXPEDITION",19:"IMPEACH",20:"CHIP_WAR",21:"LIMITED_BOSS",22:"TOURNAMENT",23:"ULTRA"};
  var bibE=mod("BattleInterfaceBase");
  if(bibE&&bibE.BattleInterfaceBase&&bibE.BattleInterfaceBase.prototype&&typeof bibE.BattleInterfaceBase.prototype._onBattleEnd==="function"&&F.hookBIBE!==bibE.BattleInterfaceBase.prototype._onBattleEnd){
    var o4=bibE.BattleInterfaceBase.prototype._onBattleEnd;
    bibE.BattleInterfaceBase.prototype._onBattleEnd=function(){
      try{
        var b=(arguments&&arguments[0])||this.battle||null;
        var cw=(b&&b.victorCamp===0)?1:0;   // MY_CAMP=ATTACK_CAMP=0
        var sd=this.serverData||{}, sw=-1;
        if(sd.stats&&typeof sd.stats.isLeftWin==="boolean") sw=sd.stats.isLeftWin?1:0;
        else if(typeof sd.isWin==="boolean") sw=sd.isWin?1:0;
        var pop=(this.battleType===1)
          ?((sd.stats&&typeof sd.stats.isLeftWin==="boolean")?(sd.stats.isLeftWin?"WIN":"FAIL"):"?")
          :(cw?"WIN":"FAIL");
        back("battleEnd type="+(btName[this.battleType]||this.battleType)+" clientWin="+cw+" serverWin="+sw+" popup="+pop+" frame="+(b?b.frame:"-")+" fightTime="+(b?b.fightTime:"-")+"/"+(b?b.maxFightTime:"-")+" inv="+(F.stat.inv||0)+" dmg="+(F.stat.dmg||0));
      }catch(e){}
      return o4.apply(this,arguments);
    };
    F.hookBIBE=bibE.BattleInterfaceBase.prototype._onBattleEnd; F.stat.be=(F.stat.be||0)+1;
    back("anchor#4 battle-end forensics hooked");
  }
  if(F.hookBU&&!F.rep1){ F.rep1=1; back("damage pipeline armed (applyDamage)"); }
  if(F.hookBIB&&!F.rep2){ F.rep2=1; back("battle speed armed (update dt x N)"); }
  if(F.hookBIBE&&!F.rep4){ F.rep4=1; back("battle-end forensics armed"); }
  if(F.hookBU&&F.hookAI&&F.hookBIB&&!F.rep3){ F.rep3=1; back("all 3 anchors armed"); }
  // 命中计数回传（机器侧证据：战斗确实走过锚点）
  var st=F.stat, sum=(st.inv||0)+(st.dmg||0)+(st.spd||0);
  if(F.lastSum!==sum){
    F.lastSum=sum; F.repN=(F.repN||0)+1;
    if(F.repN<=60) back("stat#"+F.repN+" inv="+(st.inv||0)+" dmg="+(st.dmg||0)+" spd="+(st.spd||0));
  }
 }catch(e){
   try{ jsb.reflection.callStaticMethod("com/starry/magic/cocos/FkssjHooks","logStatus","(Ljava/lang/String;)V","fkssj-js patch ERR "+e); }catch(e2){}
 }
})();
)JS";

// cfg 推送（只有数值变化才 eval；%d 依次 = inv, dmg, spd）
static const char* kCfgJsFmt =
    "(function(){var g=(typeof globalThis!==\"undefined\")?globalThis:this;"
    "var F=g.__fkssj||(g.__fkssj={});if(!F.cfg)F.cfg={inv:0,dmg:1,spd:1};"
    "F.cfg.inv=%d;F.cfg.dmg=%d;F.cfg.spd=%d;return 1;})();";

// 常驻 JS 桥线程：不依赖框架 Tick（Tick 只在 session_live() 授权通过后才跑）
//   ① cfg 变化即推送（菜单拨动 ≤2s 生效）
//   ② 每轮重跑补丁：场景/System 重载后可自动重挂（补丁自带幂等守卫 F.hookBU 等）
static std::atomic<bool> g_bridgeStarted{false};

static void* FkssjBridgeThread(void*) {
    LOGI(TPL_LOG " js bridge thread start (interval=%ds)", FKSSJ_BRIDGE_INTERVAL_SEC);
    int lastInv = -1, lastDmg = -1, lastSpd = -1;
    int rounds = 0;
    for (;;) {
        const int inv = g_inv.load() ? 1 : 0;
        const int dmg = g_dmgMul.load();
        const int spd = g_spdMul.load();
        if (inv != lastInv || dmg != lastDmg || spd != lastSpd) {
            char buf[320];
            snprintf(buf, sizeof(buf), kCfgJsFmt, inv, dmg, spd);
            EvalJs(buf);
            lastInv = inv; lastDmg = dmg; lastSpd = spd;
            LOGI(TPL_LOG " js cfg push inv=%d dmg=%d spd=%d", inv, dmg, spd);
        }
        EvalJs(kPatchJs);
        if (++rounds == 1 || (rounds % 30) == 0) {
            LOGI(TPL_LOG " js bridge round=%d (inv=%d dmg=%d spd=%d)", rounds, inv, dmg, spd);
        }
        sleep(FKSSJ_BRIDGE_INTERVAL_SEC);
    }
    return nullptr;
}

static void StartBridgeThread() {
    if (g_bridgeStarted.exchange(true)) return;
    pthread_t tid;
    if (pthread_create(&tid, nullptr, FkssjBridgeThread, nullptr) == 0) {
        pthread_detach(tid);
    } else {
        LOGW(TPL_LOG " js bridge thread create failed");
    }
}

// ============================================================================
//  ⑧ 对外接口（ModEntry.h 注册表契约，签名不可改）
// ============================================================================
bool Fkssj_TryInit(JavaVM* vm) {
    if (vm) g_jvm = vm;
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;

    InstallFaultGuard();          // §④ 先装兜底（唯一 native 副作用）
    g_libReady.store(true);
    SyncHooks();                  // kSlots 为空 ⇒ 恒装 0 个钩
    LOGI(TPL_LOG " lib ready (cocos jsb, js-patch armed)");
#if FKSSJ_JS_DUMP
    // 取证用：抓已解密脚本（不依赖 auth-gated 的 Tick）
    StartDumpThread();
#endif
    // §⑨-b 阶段② 三功能：常驻 JS 桥（推送 cfg + 幂等重挂锚点）
    StartBridgeThread();
    return true;
}

bool Fkssj_IsReady() {
    return g_libReady.load();
}

void Fkssj_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case FKSSJ_FEAT_INV:
            g_inv.store(enabled);
            LOGI(TPL_LOG " 无敌 %s（我方免伤 + 我方死亡判定恒假）", enabled ? "ON" : "OFF");
            break;
        case FKSSJ_FEAT_DMG:
            if (!enabled) g_dmgMul.store(1);
            LOGI(TPL_LOG " 伤害倍率 开关 %s（值由滑块下发）", enabled ? "ON" : "OFF");
            break;
        case FKSSJ_FEAT_SPD:
            if (!enabled) g_spdMul.store(1);
            LOGI(TPL_LOG " 战斗加速 开关 %s（值由滑块下发）", enabled ? "ON" : "OFF");
            break;
        default: break;
    }
    SyncHooks();
}

void Fkssj_OnValueChanged(int featNum, int value) {
    // 滑块类功能在此钳制并存入原子量（语义：1 = 关闭）
    if (value < 1) value = 1;
    if (featNum == FKSSJ_FEAT_DMG && value > 100) value = 100;   // 与 UI 上限一致：1..100
    if (featNum == FKSSJ_FEAT_SPD && value > 10) value = 10;     // 与 UI 上限一致：1..10
    switch (featNum) {
        case FKSSJ_FEAT_DMG:
            g_dmgMul.store(value);
            LOGI(TPL_LOG " 伤害倍率 值=%d（敌方实际掉血 ×%d）", value, value);
            break;
        case FKSSJ_FEAT_SPD:
            g_spdMul.store(value);
            LOGI(TPL_LOG " 战斗加速 值=%d（推帧 dt ×%d）", value, value);
            break;
        default: break;
    }
}

void Fkssj_Tick(const bool* features, const int* values) {
    if (!features) return;

    g_inv.store(features[FKSSJ_FEAT_INV]);
    // 滑块：值数组为准（与 BlfxMod/CzbhzMod 同惯例：values[featureIndex]）
    if (values) {
        int d = values[FKSSJ_FEAT_DMG];
        int s = values[FKSSJ_FEAT_SPD];
        g_dmgMul.store(d < 1 ? 1 : (d > 100 ? 100 : d));
        g_spdMul.store(s < 1 ? 1 : (s > 10 ? 10 : s));
    } else {
        if (!features[FKSSJ_FEAT_DMG]) g_dmgMul.store(1);   // 滑块关 ⇒ 复位
        if (!features[FKSSJ_FEAT_SPD]) g_spdMul.store(1);
    }

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
