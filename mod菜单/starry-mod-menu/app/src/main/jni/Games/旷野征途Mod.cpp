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

#include <pthread.h>  // 3.5 早期装钩线程（pthread_create，detached）
#include <unistd.h>   // 3.5 usleep(5ms) 轮询 libil2cpp
#include <mutex>      // §④ 装钩互斥
#include <setjmp.h>   // §④ sigsetjmp 保护区
#include <signal.h>   // §④ 致命信号兜底
#include <sys/stat.h> // 3.2 探针：stat(libil2cpp.so) 取文件大小（大小兜底判据）

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
static void ModHitFirst(const char* tag);   // 前向声明（MODHIT 首报；定义在 §⑤-c DbgLog 之后）
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
    ModHitFirst("GameUpdate");   // MODHIT：帧锚点真的被游戏调用
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
static std::atomic<bool> g_spdRestore{false};  // OFF 后待恢复 timeScale=1.0（UI 线程只置位，Tick 里执行）
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
static bool  LibGuestRange(const char* lib, uintptr_t* lo, uintptr_t* hi);          // 3.2 装钩自证
static bool  LibFileSize(const char* lib, unsigned long long* out);                 // 3.2 探针兜底
static bool  IsExecRange(uintptr_t p);                                             // MODHOOK：任意可执行段判定
static void  LogHookDiag(const char* tag, void* target, void* orig, int installed); // MODHOOK 单行

static uintptr_t kRvaShowInternal = 0x1C8A9AC;   // 1.8 实测（file）
static uintptr_t kRvaHandleNative = 0x1C8A724;   // 1.8 实测（file，只调不挂）
static uintptr_t kRvaEnterInterp  = 0x19876a8;   // 1.8 实测（file）
static std::atomic<int> g_libVer{0};             // 0=未知 / 18=1.8 / 22=2.2 / 24=2.4 / 32=3.2 / 33=3.3 / 35=3.5

// 3.2 指纹常量（2026-11-24 静态实测；全部 **file offset**，VA = file + 0x4000）
static const uintptr_t kOff32Show   = 0x1CFC6D0u;  // ShowInternal（VA 0x1D006D0，序言 0xF81E0FFE）
static const uintptr_t kOff32Interp = 0x19F6364u;  // EnterFrameFromInterpreter（VA 0x19FA364，序言 0xF81D0FFE）
static const uintptr_t kOff32Timescale = 0x37B27A8u;    // Time::set_timeScale（VA 0x37B67A8，序言 0xFC1E0FE8）
static const uint32_t  kInsnTimeScale  = 0xFC1E0FE8u;   // str d8,[sp,#-0x20]! —— 装钩前序言校验字
static const unsigned long long kSize32Lib = 66268112ull;  // libil2cpp.so 66,268,112 B（sha256 b249a131…332b）

// 3.3 指纹常量（2026-11-25 静态实测；全部 **file offset**，VA = file + 0x4000）
//   底包 libil2cpp.so 66,297,952 B / sha256 bbf9fc3017eddda6365510c2a8a1fba84e0b236ee411f9a9db3c04875097a004
static const uintptr_t kOff33Show   = 0x1CFF134u;  // ShowInternal（VA 0x1D03134，序言 0xF81E0FFE）
static const uintptr_t kOff33Interp = 0x19F8A30u;  // EnterFrameFromInterpreter（VA 0x19FCA30，序言 0xF81D0FFE）
static const uintptr_t kOff33Timescale = 0x37B9344u;    // Time::set_timeScale（VA 0x37BD344，序言 0xFC1E0FE8）
static const unsigned long long kSize33Lib = 66297952ull;  // libil2cpp.so 66,297,952 B（sha256 bbf9fc30…97a004）

// 3.5 指纹常量（2026-11-28 静态实测；全部 **file offset**，VA = file + 0x4000）
//   底包 libil2cpp.so 66,377,504 B / sha256 26d210e1e9e93a6d27c6cb56b78671fa47f724d5dfca907f9fad920601ce4f6c
//   ⚠ 3.5 三个首字与 3.3 **完全相同**（show 0xF81E0FFE / interp 0xF81D0FFE / timescale 0xFC1E0FE8）：
//     首字只证明「该地址上确实是指定函数」，不证明「这是 3.5」⇒ 判别只能靠「3.5 的地址表 + 文件大小」，
//     所以 3.3 分支必须留着（不能因为同段结构就合并），两者只有 size（66,297,952 vs 66,377,504）能分开。
//   反篡改（ET.AndroidPackageIntegrity，TypeDefIndex 10253）第三个方法名 3.5 = **VerifyPackageContents**
//     （file 0x2B7C680 / RVA 0x2B80680 / 体长 0xA24），**不是** 3.3 的 VerifyReleaseManifest，
//     也**不是** 3.2 的 VerifyNativeLibraries —— 两个旧名在 3.5 库按名 0 命中。
//     480 B 条目校验由硬编码改成 hashesLength(0x60)+SignatureLength(0x180)；白名单仍是
//     libmain/libunity/libil2cpp 的 sha256 ⇒ libil2cpp.so 必须字节不变，改包只能靠 killer dex 回灌官方 DER。
static const uintptr_t kOff35Show   = 0x1D04B30u;  // ShowInternal（VA 0x1D08B30，序言 0xF81E0FFE）
static const uintptr_t kOff35Interp = 0x19FE258u;  // EnterFrameFromInterpreter（VA 0x1A02258，序言 0xF81D0FFE）
static const uintptr_t kOff35Timescale = 0x37CA8F0u;    // Time::set_timeScale（VA 0x37CE8F0，序言 0xFC1E0FE8）
static const unsigned long long kSize35Lib = 66377504ull;  // libil2cpp.so 66,377,504 B（sha256 26d210e1…4f6c）

// 探针：用「首指令是否等于已知序言」判版本，不依赖包名/版本号字符串
static int DetectLibVersion() {
    struct Probe { int ver; uintptr_t off; };
    // 探针 = 各版本 ShowInternal(file) 首指令；旧版互不误判（2.4 侧由 delta24 静态核验）：
    //   2.4 独有探针 0x1CCE6CC 处 = 0xF81E0FFE（str x30,[sp,#-0x20]!）
    //   2.4 在 2.2 探针 0x1CA9130 处 = 0xB4000D96（≠0xF81E0FFE，不会误判成 2.2）
    //   2.4 在 1.8 探针 0x1C8A9AC 处 = 0x97F951DD（≠0xF81E0FFE）
    // 3.2 单点序言不足（0xF81E0FFE 在段内多处同构）⇒ 三点取二，见下。
    // 3.5 首插表首（2026-11-28）：3.5 与 3.3 的三个首字一模一样（同段结构），
    //   只能靠「3.5 的地址表 + 文件大小 66,377,504」区分 ⇒ 3.5 必须先试，3.3 分支原样保留。
    static const Probe probes[] = {{35, kOff35Show}, {33, kOff33Show}, {32, kOff32Show}, {24, 0x1CCE6CCu}, {22, 0x1CA9130u}, {18, 0x1C8A9ACu}};
    for (const auto& pr : probes) {
        void* va = FileOffToVa(TPL_TARGET_LIB, pr.off);
        if (!va) continue;
        uint32_t insn = 0;
        const bool prologueOk = ReadU32Guarded("verprobe", va, &insn) && insn == 0xF81E0FFEu;

        if (pr.ver == 35) {
            // 3.5 判据（三选二，2026-11-28 静态实测）：
            //   ① file 0x1D04B30 == 0xF81E0FFE（ShowInternal 序言）
            //   ② file 0x19FE258 == 0xF81D0FFE（解释器帧入口序言）
            //   ③ libil2cpp.so 文件大小 == 66,377,504（sha256 26d210e1…4f6c）
            // 接受条件：① && (② || ③)  或  ② && ③。
            // ① 与 ② 与 3.3 同字 ⇒ **单独一条没有区分力**，靠 ③ 定版本；只中 ①/②
            // （size 不符）⇒ 证据不足判未知，继续向后探 3.3/3.2，不硬套任何地址。
            const bool c1 = prologueOk;
            bool c2 = false, c3 = false;
            uint32_t insn35 = 0;
            void* va35 = FileOffToVa(TPL_TARGET_LIB, kOff35Interp);
            if (va35) c2 = ReadU32Guarded("verprobe", va35, &insn35) && insn35 == 0xF81D0FFEu;
            unsigned long long fsz35 = 0;
            c3 = LibFileSize(TPL_TARGET_LIB, &fsz35) && fsz35 == kSize35Lib;
            LOGI(TPL_LOG " verprobe 3.5 cond show=%d interp=%d size=%d/%llu",
                 (int)c1, (int)c2, (int)c3, fsz35);
            DbgLog("verprobe: 3.5 cond show=%d interp=%d size=%d/%llu", (int)c1, (int)c2, (int)c3, fsz35);
            if (!((c1 && (c2 || c3)) || (c2 && c3))) continue;
            g_libVer.store(35);
            kRvaShowInternal = kOff35Show;      // file；VA 0x1D08B30
            kRvaHandleNative = 0x1D048A8u;      // file；VA 0x1D088A8（只记录，不挂）
            kRvaEnterInterp  = kOff35Interp;    // file；VA 0x1A02258，序言 0xF81D0FFE
            LOGI(TPL_LOG " lib version probe: 3.5 (ShowInternal file=0x1D04B30, interp=0x19FE258)");
            DbgLog("verprobe: 3.5 showInternal=0x1D04B30 interp=0x19FE258");
            return 35;
        }
        if (pr.ver == 33) {
            // 3.3 判据（三选二，2026-11-25 静态实测）：
            //   ① file 0x1CFF134 == 0xF81E0FFE（ShowInternal 序言）
            //   ② file 0x19F8A30 == 0xF81D0FFE（解释器帧入口序言）
            //   ③ libil2cpp.so 文件大小 == 66,297,952（sha256 bbf9fc30…97a004）
            // 接受条件：① && (② || ③)  或  ② && ③。
            // 只中 ①（另两条都不符）⇒ 证据不足，判未知并继续向后探旧版本，不硬套 2.x/3.2 地址。
            const bool c1 = prologueOk;
            bool c2 = false, c3 = false;
            uint32_t insn33 = 0;
            void* va33 = FileOffToVa(TPL_TARGET_LIB, kOff33Interp);
            if (va33) c2 = ReadU32Guarded("verprobe", va33, &insn33) && insn33 == 0xF81D0FFEu;
            unsigned long long fsz33 = 0;
            c3 = LibFileSize(TPL_TARGET_LIB, &fsz33) && fsz33 == kSize33Lib;
            LOGI(TPL_LOG " verprobe 3.3 cond show=%d interp=%d size=%d/%llu",
                 (int)c1, (int)c2, (int)c3, fsz33);
            DbgLog("verprobe: 3.3 cond show=%d interp=%d size=%d/%llu", (int)c1, (int)c2, (int)c3, fsz33);
            if (!((c1 && (c2 || c3)) || (c2 && c3))) continue;
            g_libVer.store(33);
            kRvaShowInternal = kOff33Show;      // file；VA 0x1D03134
            kRvaHandleNative = 0x1CFEEACu;      // file；VA 0x1D02EAC（只记录，不挂）
            kRvaEnterInterp  = kOff33Interp;    // file；VA 0x19FCA30，序言 0xF81D0FFE
            LOGI(TPL_LOG " lib version probe: 3.3 (ShowInternal file=0x1CFF134, interp=0x19F8A30)");
            DbgLog("verprobe: 3.3 showInternal=0x1CFF134 interp=0x19F8A30");
            return 33;
        }
        if (pr.ver == 32) {
            // 3.2 判据（三选二，2026-11-24 静态实测）：
            //   ① file 0x1CFC6D0 == 0xF81E0FFE（ShowInternal 序言）
            //   ② file 0x19F6364 == 0xF81D0FFE（解释器帧入口序言）
            //   ③ libil2cpp.so 文件大小 == 66,268,112
            // 接受条件：① && (② || ③)  或  ② && ③。
            // 只中 ①（另两条都不符）⇒ 证据不足，判未知并继续向后探旧版本，不硬套 2.x 地址。
            const bool c1 = prologueOk;
            bool c2 = false, c3 = false;
            uint32_t insn2 = 0;
            void* va2 = FileOffToVa(TPL_TARGET_LIB, kOff32Interp);
            if (va2) c2 = ReadU32Guarded("verprobe", va2, &insn2) && insn2 == 0xF81D0FFEu;
            unsigned long long fsz = 0;
            c3 = LibFileSize(TPL_TARGET_LIB, &fsz) && fsz == kSize32Lib;
            LOGI(TPL_LOG " verprobe 3.2 cond show=%d interp=%d size=%d/%llu",
                 (int)c1, (int)c2, (int)c3, fsz);
            DbgLog("verprobe: 3.2 cond show=%d interp=%d size=%d/%llu", (int)c1, (int)c2, (int)c3, fsz);
            if (!((c1 && (c2 || c3)) || (c2 && c3))) continue;
            g_libVer.store(32);
            kRvaShowInternal = kOff32Show;      // file；VA 0x1D006D0
            kRvaHandleNative = 0x1CFC448u;      // file；VA 0x1D00448（只记录，不挂）
            kRvaEnterInterp  = kOff32Interp;    // file；VA 0x19FA364，序言 0xF81D0FFE
            LOGI(TPL_LOG " lib version probe: 3.2 (ShowInternal file=0x1CFC6D0, interp=0x19F6364)");
            DbgLog("verprobe: 3.2 showInternal=0x1CFC6D0 interp=0x19F6364");
            return 32;
        }

        if (!prologueOk) continue;
        g_libVer.store(pr.ver);
        if (pr.ver == 24) {
            kRvaShowInternal = 0x1CCE6CCu;  // file；VA 0x1CD26CC
            kRvaHandleNative = 0x1CCE444u;  // file；VA 0x1CD2444（仅记录，本轮不挂）
            kRvaEnterInterp  = 0x19C9A78u;  // file；VA 0x19cda78，序言 0xF81D0FFE
            LOGI(TPL_LOG " lib version probe: 2.4 (ShowInternal file=0x1CCE6CC, interp=0x19C9A78)");
            DbgLog("verprobe: 2.4 showInternal=0x1CCE6CC interp=0x19C9A78");
        } else if (pr.ver == 22) {
            kRvaShowInternal = 0x1CA9130u;
            kRvaHandleNative = 0x1CA8EA8u;
            kRvaEnterInterp  = 0x19a5004u;  // file；VA 0x19a9004（原来只写在 InstallBattleHooks 里）
            LOGI(TPL_LOG " lib version probe: 2.2 (ShowInternal file=0x1CA9130, interp=0x19a5004)");
            DbgLog("verprobe: 2.2 showInternal=0x1CA9130 interp=0x19a5004");
        } else {
            LOGI(TPL_LOG " lib version probe: 1.8 (ShowInternal file=0x1C8A9AC)");
            DbgLog("verprobe: 1.8");
        }
        return pr.ver;
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
            LogHookDiag(tag, target, nullptr, 0);   // 失败也落一行：便于对照「没装 / 装了不可信」
            return false;
        }
        // ★ 每次 DobbyHook 后必落 MODHOOK：target / 返回的 orig / orig 是否落在 libil2cpp guest 区间
        LogHookDiag(tag, target, orig ? *orig : nullptr, 1);
        return true;
    }
    // longjmp 回来：保护区内的致命信号已被吞掉
    t_inGuard = 0;
    g_lastFatal = true;
    LOGW(TPL_LOG " %s dobby SIG(%d) @%p — 槽位熔断，游戏继续",
         tag, (int)t_faultSig, (void*)t_faultAddr);
    if (orig) *orig = nullptr;   // ★ 必须清掉可能被半写的 orig，否则后续调用跳到野指针
    LogHookDiag(tag, target, nullptr, -1);          // 半写状态：orig 已清，记录这次熔断
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

// libil2cpp 在 /proc/self/maps 里的**整段 guest 映射区间**（含 r--p / r-xp 等所有段，取总跨度）。
// 真机 arm64 原生执行时这就是 .so 被 mmap 的全部范围；Dobby 返回的 orig 必然落在其中。
// 模拟器 Houdini 翻译层下 arm64 guest 段也会出现在 maps 里，但 Dobby 可能把 orig 返回到
// x86_64 用户空间（0x7xxx…）⇒ 区间外 ⇒ 该次装钩不可信。
static bool LibGuestRange(const char* lib, uintptr_t* lo, uintptr_t* hi) {
    FILE* fp = std::fopen("/proc/self/maps", "r");
    if (!fp) return false;
    char line[768];
    uintptr_t l = UINTPTR_MAX, h = 0;
    while (std::fgets(line, sizeof(line), fp)) {
        if (!IsLibMapLine(line, lib)) continue;
        uintptr_t start = 0, end = 0, off = 0;
        char perm[8] = {};
        if (std::sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (end <= start) continue;
        if (start < l) l = start;
        if (end > h) h = end;
    }
    std::fclose(fp);
    if (h <= l) return false;
    if (lo) *lo = l;
    if (hi) *hi = h;
    return true;
}

// 3.2 探针的「文件大小兜底」判据：从 maps 取 libil2cpp.so 的磁盘路径再 stat。
// 大小不受「段内偶然同构序言」影响，是最稳的一条；取不到就返回 false（只作加分项，不阻塞）。
static bool LibFileSize(const char* lib, unsigned long long* out) {
    FILE* fp = std::fopen("/proc/self/maps", "r");
    if (!fp) return false;
    char line[768];
    char path[512] = {0};
    while (std::fgets(line, sizeof(line), fp)) {
        if (!IsLibMapLine(line, lib)) continue;
        const char* sl = std::strchr(line, '/');
        if (!sl) continue;
        std::snprintf(path, sizeof(path), "%s", sl);
        char* nl = std::strchr(path, '\n');
        if (nl) *nl = '\0';
        break;
    }
    std::fclose(fp);
    if (!path[0]) return false;
    struct stat st;
    if (::stat(path, &st) != 0) return false;
    if (out) *out = (unsigned long long)st.st_size;
    return true;
}

// 任意含 'x' 权限的映射段判定（/proc/self/maps）。
// 用途：Dobby 的 orig 是它自己 mmap 出来的 **trampoline**（可执行页），天然不在 libil2cpp 区间内；
// 真机 arm64 用户态地址本来就在 0x7xxx… ⇒ 「不在 guest 区间」「0x7xxx ⇒ x86」都不是故障判据。
// 唯一有效的判据：orig 必须落在**某个可执行映射段**里；落在不可执行段/未映射区才是非法返回。
static bool IsExecRange(uintptr_t p) {
    if (!p) return false;
    FILE* fp = std::fopen("/proc/self/maps", "r");
    if (!fp) return false;
    char line[768];
    bool hit = false;
    while (std::fgets(line, sizeof(line), fp)) {
        uintptr_t start = 0, end = 0, off = 0;
        char perm[8] = {};
        if (std::sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (p < start || p >= end) continue;
        if (std::strchr(perm, 'x')) hit = true;   // 命中可执行段
        break;                                    // 命中任意段（含不可执行）即停：地址只属于一段
    }
    std::fclose(fp);
    return hit;
}

// ============================================================================
//  ⑥-a 装钩自证（MODHOOK / MODHIT）— 2026-11-24 新增，防「装上了却不可信」
//
//  目标设备：真机 Xiaomi M2102K1AC / arm64-v8a 原生 / 未 root
//    ⇒ 读不到 /data/data/com.xf.kyzt/files/（DbgLog 文件），**诊断必须同时进 logcat**。
//    历史风险设备：雷电模拟器（Houdini 翻译层，libil2cpp 代码段 r--p）曾有「Dobby 返回成功
//    但句柄落 x86 空间」的假阳性 ⇒ 下面这条判定就是那次风险的防线，必须保留。
//
//  判定口径（写死在代码里，日志一眼可判）：
//    · orig 落在 [libbase, libbase+libsize) 内 ⇒ inGuest=1 ⇒ 装钩可信；
//    · inGuest=0（典型 orig = 0x7xxx… 的 x86_64 用户空间，或区间外野地址）⇒ **装钩不可信**，
//      即便 Dobby 返回 0 也不能认为 hook 生效；此时也不会有对应的 MODHIT 行。
//      libbase/libsize 取自 /proc/self/maps 中 libil2cpp 全部段的并集跨度。
//
//  日志格式固定字段顺序（勿改，配合 adb logcat -d | grep MODHOOK）：
//    MODHOOK tag=.. target=.. orig=.. inGuest=.. libbase=.. libsize=.. tgtInGuest=.. installed=..
//    MODHIT  tag=.. n=1 va=..            （首次真正命中，每 tag 只落一次）
// ============================================================================
static const int kHookDiagMax = 8;                                 // 本文件 hook 标签数上限
static std::mutex g_hookDiagMutex;
static const char* g_hookDiagTag[kHookDiagMax] = {};
static uintptr_t   g_hookDiagVa[kHookDiagMax]  = {};

static void RememberHookVa(const char* tag, uintptr_t va) {
    if (!tag || !va) return;
    std::lock_guard<std::mutex> lk(g_hookDiagMutex);
    for (int i = 0; i < kHookDiagMax; ++i) {
        if (g_hookDiagTag[i] && std::strcmp(g_hookDiagTag[i], tag) == 0) { g_hookDiagVa[i] = va; return; }
    }
    for (int i = 0; i < kHookDiagMax; ++i) {
        if (!g_hookDiagTag[i]) { g_hookDiagTag[i] = tag; g_hookDiagVa[i] = va; return; }
    }
}

static uintptr_t HookVaOf(const char* tag) {
    if (!tag) return 0;
    std::lock_guard<std::mutex> lk(g_hookDiagMutex);
    for (int i = 0; i < kHookDiagMax; ++i)
        if (g_hookDiagTag[i] && std::strcmp(g_hookDiagTag[i], tag) == 0) return g_hookDiagVa[i];
    return 0;
}

// installed: 1=装钩成功 / 0=Dobby 返回失败 / -1=装钩期致命信号（保护区吞掉）
//
// 判定口径（2026-11-24 真机修订，首版判据是误报，勿回退）：
//   · Dobby 返回的 orig 默认是它自己分配的 **trampoline**（独立可执行页），
//     天然不在 libil2cpp 区间内 ⇒ inGuest=0 属**正常**，不能据此告警；
//   · 真机 arm64 用户态地址本来就在 0x7xxx…，**「0x7xxx = x86 空间」是错的**；
//   · 唯一有效判据：orig 必须落在**任意 r-x 可执行映射段**（origExec=1）；
//     orig=null 或 origExec=0（且 installed=1）⇒ 这才是「装钩不可信」。
//   · inGuest / tgtInGuest 保留：tgtInGuest 对 target 是否落在目标库仍有用，inGuest 仅供参考。
// 字段顺序固定（origExec 为尾部新增，前 8 段不变）：
//   MODHOOK tag= target= orig= inGuest= libbase= libsize= tgtInGuest= installed= origExec=
static void LogHookDiag(const char* tag, void* target, void* orig, int installed) {
    uintptr_t lo = 0, hi = 0;
    const bool have = LibGuestRange(TPL_TARGET_LIB, &lo, &hi);
    const uintptr_t t = (uintptr_t)target, o = (uintptr_t)orig;
    const int tgtIn    = (have && t >= lo && t < hi) ? 1 : 0;
    const int inGuest  = (have && o >= lo && o < hi) ? 1 : 0;
    const int origExec = IsExecRange(o) ? 1 : 0;
    const unsigned long long span = have ? (unsigned long long)(hi - lo) : 0ull;

    LOGI(TPL_LOG " MODHOOK tag=%s target=0x%lX orig=0x%lX inGuest=%d libbase=0x%lX libsize=0x%llX tgtInGuest=%d installed=%d origExec=%d",
         tag, (unsigned long)t, (unsigned long)o, inGuest,
         (unsigned long)lo, span, tgtIn, installed, origExec);
    DbgLog("MODHOOK tag=%s target=0x%lX orig=0x%lX inGuest=%d libbase=0x%lX libsize=0x%llX tgtInGuest=%d installed=%d origExec=%d",
           tag, (unsigned long)t, (unsigned long)o, inGuest,
           (unsigned long)lo, span, tgtIn, installed, origExec);

    if (installed == 1) RememberHookVa(tag, t);   // MODHIT 回填用
    if (installed != 1) return;                    // 失败/熔断已在别处告警，这里不重复判
    if (!o)
        LOGW(TPL_LOG " MODHOOK tag=%s orig=null ⇒ 装钩不可信", tag);
    else if (!origExec)
        LOGW(TPL_LOG " MODHOOK tag=%s orig=0x%lX 不在任何可执行映射段 ⇒ 装钩不可信（无 trampoline）",
             tag, (unsigned long)o);
    else if (!inGuest)
        LOGI(TPL_LOG " MODHOOK tag=%s orig 为 Dobby trampoline（不在 libil2cpp 区间属正常，勿按此判失败）", tag);
    if (!have)
        LOGI(TPL_LOG " MODHOOK tag=%s 未找到 libil2cpp 映射 ⇒ inGuest/tgtInGuest 无意义（不影响 orig 判定）", tag);
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
    if (s.rva && HookAtRva(s.tag, s.rva, s.expectInsn, s.hookFn, s.origPtr)) {
        DbgLog("hook %s: path=rva 0x%lX", s.tag, (unsigned long)s.rva);   // logcat 侧见 "hooked @rva"
        return true;
    }
    // 兜底：只有 rva 为 0 / 序言不符 / rva 不在 RX 时才走到这里。
    // 注意 AOT **直调**方法（如 Time.set_timeScale）按名装钩会「装上但零命中」，日志必须能区分。
    const bool ok = HookByName(s.tag, s.ns, s.cls, s.method, s.argc, s.hookFn, s.origPtr);
    DbgLog("hook %s: path=by-name ok=%d (rva=%d expect=0x%08X)", s.tag, (int)ok, (int)(s.rva ? 1 : 0),
           (unsigned)s.expectInsn);
    return ok;
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

// 「首次真正命中」首报：每 tag 只落一次（logcat + 文件双落）。
// 用途：真机未 root 时唯一能证明「钩子真的被游戏调用」的证据 ——
//   MODHOOK installed=1 只说明地址改成功；有 MODHIT 才说明该地址确实是热路径。
// 缺失 MODHIT + 有 MODHOOK installed=1 ⇒ 大概率挂在了同构序言的假地址上（假阳性）。
static std::mutex g_hitMutex;
static const char* g_hitTag[8]  = {};
static bool        g_hitDone[8] = {};

static void ModHitFirst(const char* tag) {
    bool report = false;
    {
        std::lock_guard<std::mutex> lk(g_hitMutex);
        int slot = -1;
        for (int i = 0; i < 8; ++i) {
            if (g_hitTag[i] && std::strcmp(g_hitTag[i], tag) == 0) { slot = i; break; }
        }
        if (slot < 0) {
            for (int i = 0; i < 8; ++i) {
                if (!g_hitTag[i]) { g_hitTag[i] = tag; slot = i; break; }
            }
        }
        if (slot >= 0 && !g_hitDone[slot]) { g_hitDone[slot] = true; report = true; }
    }
    if (!report) return;
    const uintptr_t va = HookVaOf(tag);
    LOGI(TPL_LOG " MODHIT tag=%s n=1 va=0x%lX", tag, (unsigned long)va);
    DbgLog("MODHIT tag=%s n=1 va=0x%lX", tag, (unsigned long)va);
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
    ModHitFirst("EnterInterp");  // MODHIT：1.8 路（kSlots 装）帧入口命中 ApplyHp

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
    ModHitFirst("NoAd");         // MODHIT：ShowInternal 真的被触发（免广告生效的前提）
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
    // 说明：3.2 走 inline hook 后，本函数由**真实调用点**进入（AOT 直调只传 float），
    // x1 里的 "method" 不是 MethodInfo，仅是调用点残留。它只被原样转发给 trampoline，
    // 而 set_timeScale(float) 不读第二个参数 ⇒ 无副作用；g_tsMethod 只当「已命中」标记用。
    g_tsMethod.store(method);
    if (g_unityTid.load() == 0) {
        g_unityTid.store((unsigned long)pthread_self());
        DbgLog("tid: unity=%lu (from set_timeScale)", (unsigned long)pthread_self());
    }
    const int n = g_tsCalls.fetch_add(1);
    ModHitFirst("TimeScale");    // MODHIT：按名挂的 set_timeScale 真的被调用
    if (n < 8) LOGI(TPL_LOG " set_timeScale call#%d arg=%.3f spd=%d", n + 1, value, (int)g_spd.load());
    const float v = g_spd.load() ? 1.8f : value;
    if (g_origSetTimeScale) g_origSetTimeScale(v, method);
}

// TODO：rva/expectInsn 由 dump.cs / IDA 实测后回填（0 = 仅按名）
static HookSlot kSlots[] = {
    // rva/expectInsn 由 InitSlotAddrs() 按版本填（3.2 = 0x37B27A8 / 0xFC1E0FE8）：
    // AOT 直调点只能靠 inline hook 改函数体；按名仅在 rva 缺失时兜底（真机实测按名零命中）。
    {"TimeScale", 0x0, 0x0, "UnityEngine", "Time", "set_timeScale",
     1, (void*)Hooked_SetTimeScale, (void**)&g_origSetTimeScale},
    // HybridCLR 解释器入口：无按名路径，纯 RVA + 序言校验（漂移即拒绝，不盲挂）
    // 帧锚点：ET.GameManagerMono.Update（AOT / 私有 / argc=0），按名安装，无 RVA
    {"GameUpdate", 0x0, 0x0u, "ET", "GameManagerMono", "Update", 0,
     (void*)Hooked_GameUpdate, (void**)&g_origGameUpdate},
    {"EnterInterp", 0x0, 0xF81D0FFEu, "", "", "", -1,   // 1.8 由本槽位装；2.2/2.4 由 InstallBattleHooks 单点装（此处恒 0）
     (void*)Hooked_EnterInterp, (void**)&g_origEnterInterp},
    // 免广告：DirichletRewardAdBase::ShowInternal（AOT，序言 str x30,[sp,#-0x20]!）
    // 2026-11-16：补**按名三要素**（Dirichlet.Ad / DirichletRewardAdBase / ShowInternal，argc=1）。
    // 依据：2.4 dynsym 仍导出 241 个 il2cpp_*（class_from_name / class_get_method_from_name 全在），
    // AOT 方法 methodPointer 是真实代码地址 ⇒ 按名可挂。缺三要素时一旦版本探测失配
    // （ver=0 ⇒ rva 全 0），免广告就静默失败关闭 —— 这是实现缺口，不是能力缺口。
    {"NoAd", 0x0, 0xF81E0FFEu, "Dirichlet.Ad", "DirichletRewardAdBase", "ShowInternal", 1,  // rva 由 InitSlotAddrs() 按版本填，失效即转按名
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
    // 全部是 **file offset**（HookAtRva/FileOffToVa 口径，运行时走 /proc/self/maps；禁 base+RVA）。
    const uintptr_t interp = (ver == 18) ? 0x19876a8u
                           : (ver == 22) ? 0x19a5004u          // VA 0x19a9004
                           : (ver == 24) ? 0x19C9A78u          // VA 0x19cda78
                           : (ver == 35) ? kOff35Interp        // 3.5 VA 0x1A02258（序言 0xF81D0FFE）
                           : (ver == 33) ? kOff33Interp        // 3.3 VA 0x19FCA30（序言 0xF81D0FFE）
                           : (ver == 32) ? kOff32Interp : 0u;  // 3.2 VA 0x19FA364（序言 0xF81D0FFE）
    const uintptr_t noad   = (ver == 18) ? 0x1C8A9ACu
                           : (ver == 22) ? 0x1CA9130u
                           : (ver == 24) ? 0x1CCE6CCu          // VA 0x1CD26CC
                           : (ver == 35) ? kOff35Show          // 3.5 VA 0x1D08B30
                           : (ver == 33) ? kOff33Show          // 3.3 VA 0x1D03134
                           : (ver == 32) ? kOff32Show : 0u;    // 3.2 VA 0x1D006D0
    kRvaEnterInterp = interp;
    kRvaShowInternal = noad;
    kRvaHandleNative = (ver == 18) ? 0x1C8A724u
                     : (ver == 22) ? 0x1CA8EA8u
                     : (ver == 24) ? 0x1CCE444u            // 2.4 只记录不挂
                     : (ver == 35) ? 0x1D048A8u            // 3.5 VA 0x1D088A8（只记录不挂）
                     : (ver == 33) ? 0x1CFEEACu            // 3.3 VA 0x1D02EAC（只记录不挂）
                     : (ver == 32) ? 0x1CFC448u : 0u;      // 3.2 VA 0x1D00448（只记录不挂）
    // TimeScale 必须走 inline hook（真机实测 2026-11-24）：
    //   UnityEngine.Time::set_timeScale 在游戏 C# 里是 **AOT 直调**（bl 到函数体），
    //   by-name 换 MethodInfo->methodPointer 影响不到直调点 ⇒ 装钩成功但零命中（MODHIT 缺失）。
    //   GameUpdate 之所以 by-name 能命中，是因为它由 Unity 引擎回调（delegate/虚表路径）。
    const uintptr_t timescale = (ver == 35) ? kOff35Timescale    // 3.5 VA 0x37CE8F0，序言 0xFC1E0FE8
                              : (ver == 33) ? kOff33Timescale    // 3.3 VA 0x37BD344，序言 0xFC1E0FE8
                              : (ver == 32) ? kOff32Timescale    // 3.2 VA 0x37B67A8，序言 0xFC1E0FE8
                              : (ver == 24) ? 0x37541A0u          // 2.4 VA 0x37581A0（同一序言，静态文档 §2c）
                              : 0u;                               // 其它版本/未知 ⇒ 0，退回按名（已知无效，仅兜底）
    for (auto& sl : kSlots) {
        // EnterInterp 只在 1.8 由 kSlots 装；2.2/2.4/3.2/3.3/3.5 由 InstallBattleHooks 单点显式装
        // （同址双 Dobby 叠钩会 SIGSEGV ⇒ 这里绝不能重复填 rva）
        if (std::strcmp(sl.tag, "EnterInterp") == 0) sl.rva = (ver == 18) ? interp : 0u;
        else if (std::strcmp(sl.tag, "NoAd") == 0)   sl.rva = noad;
        else if (std::strcmp(sl.tag, "TimeScale") == 0) {
            sl.rva = timescale;                                  // rva 优先（序言不符即拒绝，不硬装）
            sl.expectInsn = timescale ? kInsnTimeScale : 0u;      // 0xFC1E0FE8 = str d8,[sp,#-0x20]!
        }
    }
    DbgLog("slot addrs: interp=0x%lX noad=0x%lX timescale=0x%lX ver=%d",
           (unsigned long)kRvaEnterInterp, (unsigned long)kRvaShowInternal,
           (unsigned long)timescale, g_libVer.load());
    LOGI(TPL_LOG " slot addrs: interp=0x%lX noad=0x%lX timescale=0x%lX ver=%d",
         (unsigned long)kRvaEnterInterp, (unsigned long)kRvaShowInternal,
         (unsigned long)timescale, g_libVer.load());
}

// ============================================================================
//  ④-b 战斗：EnterFrame 入口按 MethodInfo 精确匹配 ApplyHp
//
//  【3.2 合同（2026-11-24 从 HotUpdate.decrypted.dll 反推，sha256 812e6563…d551b0）】
//    sig : void ApplyHp(BattleRoleActor atker, int atkSeatNum,
//                       BattleDamageResult dmgInfo, Action<BattleRoleActor> OnDie)
//          rva 0x0FF20C / body 1594 B / fat / metadata Param 行 = 4（**不含 this**）
//          ldarg 首用：0=this@IL_0003 1=atker@IL_0014 2=atkSeatNum@IL_004c
//                      3=dmgInfo@IL_0000 4=OnDie@IL_052b（ldarg.s 4）
//    argBase 槽序 +0 this / +8 atker / +16 atkSeatNum / +24 dmgInfo / +32 OnDie
//    IL 锚点：isGod 取用 @IL_021C（本文件**不调用**该属性）；首次 set_curHp @IL_0249；
//              ret @IL_000B/0036/0055/0091/0205/0230/0639
//    落血语义未变：ldloc.0 = dmgInfo.get_intDmg()（get_intDmg = ceil(dmg) + 下界钳位），
//                  set_curHp(curHp + intDmg) ⇒ dmg 为负即扣血 ⇒ 写 dmgInfo.dmg 就能左右结果
//    this = 挨打方（HP 主人）；atker 可能是召唤物（ldarg.1 后续会取 get_SummonOwner）
//  【3.3 合同（2026-11-25 从 HotUpdate.decrypted.dll 反推，sha256 3c21663c…34280）】
//    sig 未变：params = 4（不含 this）；argBase 五槽 +0/+8/+16/+24/+32 顺序与语义逐槽复现
//          rva 0x104200 / body 1610 B / fat / metadata Param 行 = 4（atker/atkSeatNum/dmgInfo/OnDie）
//    IL 锚点全体 +2：isGod 取用 @IL_021E（本文件仍**不调用**该属性）；首次 set_curHp @IL_024B
//              （第二处 IL_0265）；get_curHp @IL_0210/0244/0251/02AE；早退 ret @IL_000B/0036/0055/0091/0207/0232
//    落血语义未变：set_curHp(curHp + intDmg)，intDmg = ceil(dmg)（下界钳位 -2.1474835e9）
//    ⇒ 3.2 的过滤器 / 五槽 / 写 dmgInfo.dmg 逻辑对 3.3 原样成立，只换挂点与版本门（不进 ApplyHp 内部）
//  【3.5 合同（2026-11-28 静态实测；底包 libil2cpp 66,377,504 B，HotUpdate MVID d87df3ff…7855）】
//    sig 未变：argc 4（atker / int32 atkSeatNum / dmgInfo / Action<BattleRoleActor> OnDie；不含 this）
//          argBase 五槽 +0 this / +8 atker / +16 atkSeatNum / +24 dmgInfo / +32 OnDie —— 与 3.2/3.3 逐槽相同
//          rva 0x10B848 / body 1610 B（fat 头 12 B + codesize 0x64A）/ maxstack 5 / locals 8
//          RVA 全移、body 长度与 3.3 一字不差 ⇒ body 长度仍是最稳的版本指纹
//    IL 锚点（与 3.3 偏移全同）：isGod 取用 @IL_021E（本文件仍**不调用**该属性）；
//          首个 set_curHp @IL_024B；封顶第二处 set_curHp @IL_0265；
//          第三处 set_curHp @IL_042C（多血条循环 curHp += maxHp，get_hpCount>1 才走）；
//          OnDie.Invoke @IL_0542；早退 ret @IL_0207 / IL_0232
//          （另有 000B/0036/0055/0091 守卫 ret，末 ret @IL_0649）
//          唯一变的判据偏移：blt @IL_0282（3.3 记 0280）——按字节硬编码旧偏移会失配
//    ⚠ 3.5 的 ApplyHp 内有 **1 处 neg（IL_04BE）**，但只服务「训练假人」分支
//          （reallyDmg = clamp(-intDmg) → stfld reallyDmg @IL_04CA），**不在 curHp 路径上**；
//          curHp 仍是 IL_0244 get_curHp → IL_0249 ldloc.0(intDmg) → IL_024A add → IL_024B set_curHp
//          ⇒ 落血仍是加法、dmg<0 才掉血。**别把这处 neg 当成「3.5 伤害符号翻了」去改符号逻辑。**
//    稳定访问器 RVA（body 短）：get_curHp 0x10EA86/7、set_curHp 0x10EA8E/8、
//          get_isGod 0x10DE1D/24、get_maxHp 0x10E920/15（RVA 全移、codesize 全同）
//    调用链未变：AddHp(0x10B744/246) → 闭包 <>c__DisplayClass466_0.<AddHp>b__0(0x1C9E2B/36)
//          IL_001E call ApplyHp ⇒ ApplyHp 全汇编唯一调用点仍是该闭包
//    ⇒ 3.2/3.3 的过滤器 / 五槽 / 写 dmgInfo.dmg 逻辑对 3.5 原样成立，只换挂点与版本门
//  【2.4 历史合同（保留作对照，勿删）】
//    argc=4 / body=1265 B / isGod 取用 @IL_0173 / set_curHp @IL_0197→IL_01A0 / ret @IL_0187
//
//  行为：无敌=本机受击且 d<0 → d=0；百倍=本机输出且非本机受击且 d<0 → d*100（float 边界 clamp）
//  orig 无条件调用；不读 WasApplied；不用 isGod；不碰审计/特权/检测。
#ifndef KYZT_BATTLE_ENABLE
#define KYZT_BATTLE_ENABLE 1
#endif

#if KYZT_BATTLE_ENABLE
namespace kb {
  // 合同常量（与 IL 交叉表一一对应，改动必须同步 check_battle_contract.py）
  //   method_get_param 是 0 基且**不含 this**：0=atker 1=atkSeatNum 2=dmgInfo 3=OnDie
  //   argBase 槽位是另一套编号：+0=this +8=atker +16=atkSeatNum +24=dmgInfo +32=OnDie
  //   3.2 实测（metadata Param 行 = atker/atkSeatNum/dmgInfo/OnDie，共 4 个，不含 this）：
  //     argc(=method_param_count) = 4；把 this 也算进去才是 5 个 argBase 槽位 —— 两者别混。
  constexpr int kIdxAtker = 0;
  constexpr int kIdxSeat  = 1;
  constexpr int kIdxDmg   = 2;
  constexpr int kIdxOnDie = 3;
  constexpr int kSlotThis = 0;
  constexpr int kSlotAtker= 8;
  constexpr int kSlotSeat = 16;
  constexpr int kSlotDmg  = 24;
  constexpr int kSlotOnDie= 32;
  // ApplyHp 的 argc 接受面：4 = il2cpp method_param_count 口径（不含 this，3.2 metadata 实测）；
  // 5 = 把 this 计入槽位的口径（2026-11-24 转述值，防止口径差把功能静默关掉）。
  // 真正防误匹配靠「方法名 + 声明类 == BattleRoleActor + param0/param2 类型」三重校验。
  constexpr int kApplyHpArgc = 4;
  constexpr int kApplyHpArgcWithThis = 5;
  static_assert(kIdxAtker == 0 && kIdxSeat == 1 && kIdxDmg == 2 && kIdxOnDie == 3,
                "param 索引必须 0 基且不含 this（atker=0/seat=1/dmg=2/onDie=3）");
  static_assert(kSlotThis == 0 && kSlotAtker == 8 && kSlotSeat == 16
                && kSlotDmg == 24 && kSlotOnDie == 32,
                "argBase 槽位必须 +0 this / +8 atker / +16 seat / +24 dmgInfo / +32 OnDie（3.2 IL 反推）");
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
  static std::atomic<int>  g_argcLog{0};   // ApplyHp argc 口径只记一次（真机 logcat 可判 4/5）
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
          if (nm && std::strcmp(nm, "ApplyHp") == 0 && p_mgetClass && p_mgetClass(mi) == clsRole
              && g_argcLog.fetch_add(1) == 0) {
              // 3.2 口径自证（只落一次）：4 = method_param_count 不含 this（metadata Param 行=4）；
              // 若真机打出 5，说明本机 il2cpp 把 this 计入了参数表 ⇒ 两种都放行，功能不受影响。
              DbgLog("battle: ApplyHp argc=%u (param 行 atker/atkSeatNum/dmgInfo/OnDie=4)", (unsigned) argc);
              LOGI(TPL_LOG " battle: ApplyHp argc=%u", (unsigned) argc);
          }
          const bool argcOk = (argc == kApplyHpArgc || argc == kApplyHpArgcWithThis);
          bool match = (nm && std::strcmp(nm, "ApplyHp") == 0 && argcOk
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
          ModHitFirst("Battle");   // MODHIT：解释器帧入口命中 ApplyHp（战斗两项的前提）
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
    // 帧入口（HybridCLR 解释器，无按名路径）：22 / 24 / 32 / 33 / 35 都从这里单点装，1.8 走 kSlots。
    // 注意：HookAtRva 的参数是 **file offset**（内部走 FileOffToVa），不是 VA。
    //   · 2.2：VA 0x19a9004 ⇔ file 0x19a5004（该 VA 处为 0xB4052B20 ⇒ 传 VA 必失败）
    //   · 2.4：VA 0x19cda78 ⇔ file 0x19C9A78（序言 0xF81D0FFE = str x30,[sp,#-0x30]!）
    // 地址一律取 InitSlotAddrs 填好的 kRvaEnterInterp（不在此写版本字面量）。
    const int verHook = g_libVer.load();
    if (verHook == 22 || verHook == 24 || verHook == 32 || verHook == 33 || verHook == 35) {
        if (HookAtRva("Battle", kRvaEnterInterp, 0xF81D0FFEu,
                      (void*)kb::Hooked_EnterFrame, &kb::g_origEnter)) {
            ++ok;
        } else {
            DbgLog("battle: EnterFrame hook 未装上 ver=%d rva=0x%lX（失败关闭）",
                   verHook, (unsigned long)kRvaEnterInterp);
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

// ============================================================================
//  ⑥-b 反篡改绕行（3.5 新增面；**无条件安装**，不受懒装门控制）
//
//  为什么必须无条件装（2026-11-28 真机现象：启动 ~1.4s 后
//    `Process : Quit itself ... com.unity3d.player.UnityPlayer.nativeRender` + SIG:9，
//    无 Java FATAL / 无 tombstone / 无 crash buffer ⇒ 不是崩溃，是**进程自杀**）：
//    C# AOT 的 ET.AndroidPackageIntegrity.VerifyInstalledPackageAsync 由
//    GameManagerMono.<Start>d__41::MoveNext 在**冷启**路径上发起：先 VerifyCurrentSigner
//    （Java 侧 PackageInfo 签名读数），再 VerifyPackageContents（apk 条目 + 三个 so 的 sha256
//    + RSA-2048）；任一为 false ⇒ killProcess。这条路径在玩家**一个功能都没开**时也照跑
//    ⇒ 若跟 kSlots 一样挂在 AnyFeatureOn/SyncHooks 懒装门下，全关时 0 hook，冷启必被自杀。
//    所以本组钩子挂在 旷野征途_TryInit 的 `lib ready` 时机（先于 GameManagerMono.Start），
//    与开关无关、只与版本有关；且**不占 kSlots 名额**（kSlots 仍保持「全关 0 hook」语义）。
//
//  3.5 的面（静态实测）：VerifyPackageContents 取代 3.3 的 VerifyReleaseManifest
//    （旧名在本库按名 0 命中），480 B 条目校验由硬编码改成 hashesLength(0x60)+SignatureLength(0x180)；
//    VerifyInstalledPackageAsync（file 0x2B7AA7C）/ VerifyReleaseSignature（RSA-2048）本身未动。
//    我们只把两个 bool 判定点钉成 true，**不碰 Java 侧签名回灌**（killer dex 仍必需）。
//
//  语义：不调用 orig（签名/内容比对没有需要保留的副作用；Dobby trampoline 只占位不用）。
//    返回 true ⇒ 上层 Task<bool> 走「包完好」分支。若哪天出现「钩了仍自杀」，
//    先看 MODHIT tag=AntiTamper 是否两条都命中：只中 signer ⇒ 卡在 contents/后续比对；
//    一条都不中 ⇒ 还有第三个判定点（例如 ExitModifiedPackage）或装钩地址漂移。
// ============================================================================
static const uintptr_t kOff35VerifySigner   = 0x2B7BE58u;   // VerifyCurrentSigner（file；VA 0x2B7FE58）
static const uintptr_t kOff35VerifyContents = 0x2B7C680u;   // VerifyPackageContents（file；VA 0x2B80680）
static const uint32_t  kInsn35VerifySigner   = 0xD10183FFu;  // sub sp,sp,#0x60（后随 A9016FFE A90267FA A9035FF8）
static const uint32_t  kInsn35VerifyContents = 0xD101C3FFu;  // sub sp,sp,#0x70（后随 A9017BFD A9026FFC A90367FA）

static std::atomic<bool> g_atDone{false};          // 已尝试过安装（防重复 DobbyHook）
static std::atomic<int>  g_atSignerHits{0};
static std::atomic<int>  g_atContentsHits{0};
static uintptr_t g_atSignerVa   = 0;
static uintptr_t g_atContentsVa = 0;
static void* g_origVerifySigner   = nullptr;       // 只记录 trampoline，**本文件不调用**
static void* g_origVerifyContents = nullptr;

// 首次各一条：MODHIT tag=AntiTamper which=signer|contents n=1 va=..
// （不复用 ModHitFirst：它按 tag 去重，两条钩子同 tag 会把第二条吞掉）
static void AntiTamperHit(const char* which, uintptr_t va, std::atomic<int>& cnt) {
    if (cnt.fetch_add(1) != 0) return;
    LOGI(TPL_LOG " MODHIT tag=AntiTamper which=%s n=1 va=0x%lX", which, (unsigned long)va);
    DbgLog("MODHIT tag=AntiTamper which=%s n=1 va=0x%lX", which, (unsigned long)va);
}

// AArch64 传参：实例方法 = (this, arg0, arg1, MethodInfo*)，静态 = (arg0..arg2, MethodInfo*)；
// 两种都按 4 个指针槽接住即可，返回值 bool 在 w0 ⇒ C++ 的 bool 直接对上，不需要 orig。
static bool Hooked_VerifyCurrentSigner(void* a0, void* a1, void* a2, void* a3) {
    (void)a0; (void)a1; (void)a2; (void)a3;
    AntiTamperHit("signer", g_atSignerVa, g_atSignerHits);
    return true;
}

static bool Hooked_VerifyPackageContents(void* a0, void* a1, void* a2, void* a3) {
    (void)a0; (void)a1; (void)a2; (void)a3;
    AntiTamperHit("contents", g_atContentsVa, g_atContentsHits);
    return true;
}

// 无条件安装入口：只在 ver==35 生效（3.3/3.2 继续走原 killer 路径，行为一律不动）
static void InstallAntiTamperHooks() {
    if (g_atDone.load()) return;
    const int ver = (g_libVer.load() == 0) ? DetectLibVersion() : g_libVer.load();
    if (ver != 35) {
        LOGI(TPL_LOG " antitamper: skip ver=%d", ver);
        DbgLog("antitamper: skip ver=%d", ver);
        return;
    }
    g_atDone.store(true);
    g_atSignerVa   = (uintptr_t)FileOffToVa(TPL_TARGET_LIB, kOff35VerifySigner);
    g_atContentsVa = (uintptr_t)FileOffToVa(TPL_TARGET_LIB, kOff35VerifyContents);
    const int s = HookAtRva("AntiTamper", kOff35VerifySigner, kInsn35VerifySigner,
                            (void*)Hooked_VerifyCurrentSigner, &g_origVerifySigner) ? 1 : 0;
    const int c = HookAtRva("AntiTamper", kOff35VerifyContents, kInsn35VerifyContents,
                            (void*)Hooked_VerifyPackageContents, &g_origVerifyContents) ? 1 : 0;
    LOGI(TPL_LOG " antitamper: ver=35 signer=%d(slot 0x%lX) contents=%d(slot 0x%lX)",
         s, (unsigned long)kOff35VerifySigner, c, (unsigned long)kOff35VerifyContents);
    DbgLog("antitamper: ver=35 signer=%d contents=%d origSigner=%p origContents=%p",
           s, c, g_origVerifySigner, g_origVerifyContents);
}

// ============================================================================
//  ⑥-c 退出探针（3.5 专用诊断；**无条件安装**，仍在 lib ready 时机）
//
//  为什么加：§⑥-b 两个反篡改钩都 installed=1 但 **MODHIT=0**（一次都没被调用）
//    ⇒ 启动期自杀不是那两个 bool 判定点干的，得抓「谁调了退出」。
//  第二轮（同日）：RequestExit / RecordRevisionConfirmation 也 MODHIT=0 ⇒ 也不是它们。
//    现在的主嫌是 ExitModifiedPackage（体内 AndroidJavaClass("android.os.Process")
//    + CallStatic("killProcess", myPid())，与 logcat「Quit itself + SIG:9」完全对得上），
//    外加 ShowPackageIntegrityError / ShowBlockingError 两个「失败阶段」探针：
//    它们不打退出，但能把「哪一步判定失败」印出来。
//
//  RequestExit 静态实测（file 0x2B8E1DC / VA 0x2B921DC，capstone 全函数）：
//    +0x000 sub sp,sp,#0x60 / +0x004..+0x010 存 x30/x19..x25
//    +0x014/+0x018/+0x01C adrp 三处静态区，+0x020 ldrb w8,[x20,#0x747]（类 cctor 已初始化标志）
//    +0x02C mov x19,x0（**reason 全程留在 x19 兼作数组元素**）→ +0x030 tbnz w8,#0 跳过 9 次 class-init
//    +0x0CC DateTimeOffset::get_UtcNow → +0x13C Application::get_version → +0x17C get_buildGUID
//    +0x1C4 Time::get_frameCount → +0x24C Environment::get_StackTrace（6 元 object[] 逐个填充）
//    +0x2B4 String::Format(格式串@0x3CC7C10) → +0x2CC String::Concat → +0x2F0 Debug::LogWarning
//    +0x300 PlayerPrefs::SetString(键@0x3CC7C18) → +0x308 PlayerPrefs::Save
//    +0x31C/+0x320 把静态 bool（+0xB8 → +0x20）**置 1**（「exit requested」标记）
//    +0x338 UnityEngine.Application::Quit()   ← 全函数**唯一**退出调用
//    +0x350 ret；+0x354 起是异常落地垫（__cxa_begin_catch → end_catch → __cxa_throw → terminate）
//    ⇒ 没有 killProcess / exit(3) / _exit：退出走的是 Application.Quit（Unity 正常退出请求，
//      与 logcat 的 nativeRender 栈一致）。**先取 reason 拼诊断、再 Quit**，顺序如上。
//  ⇒ 拦 RequestExit 且不调 orig = 不置标志、不写 PlayerPrefs、不 Quit（本轮只观察）。
//     reason 在 x0（Il2CppString*），x1 是 MethodInfo*。
//
//  RecordRevisionConfirmation(stage, details) 是「版本修订确认」记录点：**照常调 orig**，
//    只旁路记录 stage/details + caller rva —— 它若在退出前被调用，能直接看到阶段名。
//
//  诊断口径：MODHIT 命中 ⇒ 退出确由 RequestExit 发起；仍自杀但 MODHIT=0 ⇒ 换目标
//    （GetRevisionExitDiagnostics 只读不退出，暂不挂）。
// ============================================================================
static const uintptr_t kOff35RequestExit  = 0x2B8E1DCu;   // RequestExit(file)；VA 0x2B921DC
static const uintptr_t kOff35RevConfirm   = 0x2B8E620u;   // RecordRevisionConfirmation(file)；VA 0x2B92620
static const uint32_t  kInsn35RequestExit = 0xD10183FFu;  // sub sp,sp,#0x60（后随 A90267FE A9035FF8 A90457F6）
static const uint32_t  kInsn35RevConfirm  = 0xD10143FFu;  // sub sp,sp,#0x50（后随 F9000BFE A9025FF8 A90357F6）

// —— 3.5 第二个退出候选：ExitModifiedPackage（native 早前认定体内就是
//    new AndroidJavaClass("android.os.Process") + CallStatic("killProcess", myPid())）
static const uintptr_t kOff35ExitModified  = 0x2B7BB58u;   // ExitModifiedPackage(file)；VA 0x2B7FB58
static const uint32_t  kInsn35ExitModified = 0xF81D0FFEu;  // str x30,[sp,#-0x30]!（后随 A9026FF6 A9035FF4 ...）
// —— 3.5 失败阶段探针：ShowPackageIntegrityError(Exception) / ShowBlockingError(string, Exception)
static const uintptr_t kOff35ShowPkgErr    = 0x2BB5020u;   // ShowPackageIntegrityError(file)；VA 0x2BB9020
static const uintptr_t kOff35ShowBlocking  = 0x2BBCD00u;   // ShowBlockingError(file)；VA 0x2BC0D00
static const uint32_t  kInsn35ShowPkgErr   = 0xF81C0FFEu;  // str x30,[sp,#-0x40]!
static const uint32_t  kInsn35ShowBlocking = 0xF81C0FFEu;  // str x30,[sp,#-0x40]!（两者首字相同，靠地址区分）
// —— 3.5 返回值探针：ReadInstalledIdentity 在 VerifyInstalledPackageAsync 里先跑，
//    它自己就会调 VerifyCurrentSigner（+0x580）⇒ 两钩都 0 命中说明它**提前 return false**。
//    签名实测（dump.cs:642742）= private **static** bool ReadInstalledIdentity(out string apkPath,
//    out long versionCode) ⇒ x0=&apkPath、x1=&versionCode、x2=MethodInfo*（**不是** this 在 x0）。
static const uintptr_t kOff35ReadIdentity = 0x2B7ABF8u;   // ReadInstalledIdentity(file)；VA 0x2B7EBF8
static const uint32_t  kInsn35ReadIdentity = 0xD10243FFu; // sub sp,sp,#0x90
// lambda <>c__DisplayClass5_0.<VerifyInstalledPackageAsync>b__0()：body 只有 4 条
//   ldp x8,x1,[x0,#0x10] / ldr x2,[x0,#0x20] / mov x0,x8 / b VerifyPackageContents
static const uintptr_t kOff35LambdaB0   = 0x2B7D9A8u;     // file；VA 0x2B819A8
static const uint32_t  kInsn35LambdaB0  = 0xA9410408u;    // ldp x8, x1, [x0, #0x10]

static void* g_origRequestExit = nullptr;                 // 只记录 trampoline，**本文件不调用**
typedef void (*RevConfirm_t)(void*, void*, void*);
static RevConfirm_t g_origRevConfirm = nullptr;           // 照常调用（不改行为）
static void* g_origExitModified = nullptr;                // 只记录，**不调用**（本轮要让进程活下来）
typedef void (*ShowPkgErr_t)(void*, void*);
static ShowPkgErr_t g_origShowPkgErr = nullptr;           // 照常调用
typedef void (*ShowBlocking_t)(void*, void*, void*);
static ShowBlocking_t g_origShowBlocking = nullptr;       // 照常调用
static std::atomic<bool> g_epDone{false};
static std::atomic<int>  g_epExitHits{0};
static std::atomic<int>  g_epRevHits{0};
static std::atomic<int>  g_epKillHits{0};
static std::atomic<int>  g_epPkgErrHits{0};
static std::atomic<int>  g_epBlockHits{0};
static uintptr_t g_epExitVa = 0;
static uintptr_t g_epRevVa  = 0;
static uintptr_t g_epKillVa = 0;
static uintptr_t g_epPkgErrVa = 0;
static uintptr_t g_epBlockVa  = 0;
// 返回值探针（只记录，不改语义）
typedef bool (*ReadIdentity_t)(void*, void*, void*);       // static bool(out string, out long)
static ReadIdentity_t g_origReadIdentity = nullptr;       // 照常调用
typedef bool (*LambdaB0_t)(void*, void*);                 // instance bool()
static LambdaB0_t g_origLambdaB0 = nullptr;               // 照常调用
static std::atomic<int> g_epReadIdHits{0};
static std::atomic<int> g_epLambdaHits{0};
static uintptr_t g_epReadIdVa = 0;
static uintptr_t g_epLambdaVa = 0;

// Il2CppString → UTF-8：对象头后 +0x10 = int32 length，+0x14 起 UTF-16LE。
// 指针可能已是野指针 ⇒ 整个读取跑在保护区内（炸了给 <fault@ptr>，不让游戏进程崩）。
struct EpStrCtx { const void* s; char* out; size_t cap; };
static void EpReadStrThunk(void* p) {
    auto* c = static_cast<EpStrCtx*>(p);
    c->out[0] = '\0';
    if (!c->s) return;
    const int32_t len = *(const int32_t*)((const uint8_t*)c->s + 0x10);
    if (len < 0 || len > 512) return;                      // 离谱长度：不读，交回 <unreadable>
    const uint16_t* ch = (const uint16_t*)((const uint8_t*)c->s + 0x14);
    size_t o = 0;
    for (int32_t i = 0; i < len && o + 4 < c->cap; ++i) {
        const uint16_t u = ch[i];
        if (u < 0x80) {
            c->out[o++] = (char)u;
        } else if (u < 0x800) {
            c->out[o++] = (char)(0xC0 | (u >> 6));
            c->out[o++] = (char)(0x80 | (u & 0x3F));
        } else {
            c->out[o++] = (char)(0xE0 | (u >> 12));
            c->out[o++] = (char)(0x80 | ((u >> 6) & 0x3F));
            c->out[o++] = (char)(0x80 | (u & 0x3F));
        }
    }
    c->out[o] = '\0';
}

static void EpCopyStr(const void* s, char* out, size_t cap) {
    out[0] = '\0';
    if (!s) { std::snprintf(out, cap, "<null>"); return; }
    EpStrCtx c{ s, out, cap };
    if (!RunGuarded("ExitProbe", EpReadStrThunk, &c)) { std::snprintf(out, cap, "<fault@%p>", s); return; }
    if (!out[0]) std::snprintf(out, cap, "<unreadable@%p>", s);
}

// caller 的 RVA（回 dump.cs 反查是谁调的）：RA − libil2cpp 映射起点（LOAD#1 VA=0 ⇒ 基址即 lo）
static unsigned long long EpCallerRva(void* ra) {
    if (!ra) return 0ull;
    uintptr_t lo = 0, hi = 0;
    if (!LibGuestRange(TPL_TARGET_LIB, &lo, &hi)) return 0ull;
    const uintptr_t a = (uintptr_t)ra;
    if (a < lo || a >= hi) return 0ull;
    return (unsigned long long)(a - lo);
}

static void EpHit(const char* which, uintptr_t va, std::atomic<int>& cnt) {
    if (cnt.fetch_add(1) != 0) return;                     // 首次各一条
    LOGI(TPL_LOG " MODHIT tag=ExitProbe which=%s n=1 va=0x%lX", which, (unsigned long)va);
    DbgLog("MODHIT tag=ExitProbe which=%s n=1 va=0x%lX", which, (unsigned long)va);
}

// RequestExit(string reason)：**拦下不执行**（不置标志 / 不写 PlayerPrefs / 不 Application.Quit）
static void Hooked_RequestExit(void* reason, void* mi) {
    (void)mi;
    EpHit("RequestExit", g_epExitVa, g_epExitHits);
    char buf[512];
    EpCopyStr(reason, buf, sizeof(buf));
    LOGI(TPL_LOG " exit-probe RequestExit reason=\"%s\"", buf);
    DbgLog("exit-probe RequestExit reason=\"%s\"", buf);
    const unsigned long long rva = EpCallerRva(__builtin_return_address(0));
    LOGI(TPL_LOG " exit-probe caller rva=0x%llX", rva);
    DbgLog("exit-probe caller rva=0x%llX", rva);
    // 直接返回：本轮只观察「拦下后游戏还跑不跑」
}

// RecordRevisionConfirmation(string stage, string details)：只记录，**照常调 orig**
static void Hooked_RecordRevisionConfirmation(void* stage, void* details, void* mi) {
    (void)mi;
    EpHit("RevConfirm", g_epRevVa, g_epRevHits);
    char s1[384], s2[384];
    EpCopyStr(stage, s1, sizeof(s1));
    EpCopyStr(details, s2, sizeof(s2));
    const unsigned long long rva = EpCallerRva(__builtin_return_address(0));
    LOGI(TPL_LOG " exit-probe RecordRevisionConfirmation stage=\"%s\" details=\"%s\" caller rva=0x%llX",
         s1, s2, rva);
    DbgLog("exit-probe RecordRevisionConfirmation stage=\"%s\" details=\"%s\" caller rva=0x%llX",
           s1, s2, rva);
    if (g_origRevConfirm) g_origRevConfirm(stage, details, mi);
}

// ExitModifiedPackage()：静态实测 body 0x22C / 556 B（file 0x2B7BB58 → ret@+0x22C）：
//   +0x01C ldrb w8,[x22,#0x6c9] +0x02C tbnz → 只是类 cctor 闸门，**没有任何前置 if**；
//   +0xC0 AndroidJavaClass::.ctor("android.os.Process"@0x3CC7058) → +0xCC new object[1]
//   → +0x14C 取 myPid（helper 0x1D88E58）→ +0x168 box(int) → +0x1BC AndroidJavaObject::CallStatic
//   （方法名 "killProcess"@0x3CC7068，参数数组）→ 清理 → ret。⇒ 体内**只有 killProcess**。
// 本轮：直接 return，不放行 killProcess（看进程活下来之后会发生什么）。
static void Hooked_ExitModifiedPackage(void* a0, void* a1) {
    (void)a0; (void)a1;
    EpHit("ExitModifiedPackage", g_epKillVa, g_epKillHits);
    const unsigned long long rva = EpCallerRva(__builtin_return_address(0));
    LOGI(TPL_LOG " exit-probe ExitModifiedPackage caller rva=0x%llX", rva);
    DbgLog("exit-probe ExitModifiedPackage caller rva=0x%llX", rva);
}

// ShowPackageIntegrityError(Exception)：失败阶段探针，只记首次 + 照常调用 orig
static void Hooked_ShowPackageIntegrityError(void* ex, void* mi) {
    EpHit("ShowPackageIntegrityError", g_epPkgErrVa, g_epPkgErrHits);
    if (g_origShowPkgErr) g_origShowPkgErr(ex, mi);
}

// ShowBlockingError(string stage, Exception ex)：x0=stage 字符串，照常调用 orig
static void Hooked_ShowBlockingError(void* stage, void* ex, void* mi) {
    EpHit("ShowBlockingError", g_epBlockVa, g_epBlockHits);
    char s1[512];
    EpCopyStr(stage, s1, sizeof(s1));
    const unsigned long long rva = EpCallerRva(__builtin_return_address(0));
    LOGI(TPL_LOG " exit-probe ShowBlockingError stage=\"%s\" caller rva=0x%llX", s1, rva);
    DbgLog("exit-probe ShowBlockingError stage=\"%s\" caller rva=0x%llX", s1, rva);
    if (g_origShowBlocking) g_origShowBlocking(stage, ex, mi);
}

// ReadInstalledIdentity(out string apkPath, out long versionCode)：**先调 orig**，再读 out 参数与返回值，
// 原样返回 w0（只观察）。x0=&apkPath（指向 Il2CppString*）、x1=&versionCode（指向 int64）、x2=MethodInfo*。
// il2cpp_string_new 包装（懒解析）：强制通过时要把一个合法 apkPath 写回 out 槽
static void* KyztNewIl2cppString(const char* utf8) {
    typedef void* (*StrNew_t)(const char*);
    static StrNew_t fn = nullptr;
    if (!fn) fn = (StrNew_t)Il2Cpp::ResolveExport("il2cpp_string_new");
    if (!fn || !utf8) return nullptr;
    return fn(utf8);
}

// 2026-11-28 第二轮：**强制通过**。链条实测（lead 反汇编）：
//   <Start>d__41.MoveNext +0x?? bl VerifyInstalledPackageAsync → 结果 w0 为 false 就 ExitModifiedPackage；
//   VerifyInstalledPackageAsync +0xA4 bl ReadInstalledIdentity → 它 false 就直接 Task.FromResult(false)。
// ⇒ 只要把这里变成 true，后面 VerifyCurrentSigner / lambda / VerifyPackageContents 才可能跑，
//   而那三个点我们已经分别钩成 true 或「先调 orig 再记录」。所以这里 false 就补齐 out 参数并返回 true。
static bool Hooked_ReadInstalledIdentity(void* pApk, void* pVc, void* mi) {
    EpHit("ReadInstalledIdentity", g_epReadIdVa, g_epReadIdHits);
    if (!g_origReadIdentity) return false;
    const bool ret = g_origReadIdentity(pApk, pVc, mi);     // 先跑真实实现
    char apk[512];
    EpCopyStr(pApk ? *(void**)pApk : nullptr, apk, sizeof(apk));   // out 是引用：解一层
    const long long vc = (pVc ? *(const long long*)pVc : 0ll);
    const int force = ret ? 0 : 1;
    LOGI(TPL_LOG " exit-probe ReadInstalledIdentity ret=%d apk=\"%s\" vc=%lld force=%d",
         (int)ret, apk, vc, force);
    DbgLog("exit-probe ReadInstalledIdentity ret=%d apk=\"%s\" vc=%lld force=%d",
           (int)ret, apk, vc, force);
    if (ret) return true;                                    // 本来就通过：原样
    // —— FORCE-PASS：优先沿用 orig 给的路径；空/不可读则用占位常量 ——
    const bool haveApk = (apk[0] && apk[0] != '<');
    const char* use = haveApk ? apk : "/data/app/com.xf.kyzt/base.apk";
    LOGI(TPL_LOG " exit-probe ReadInstalledIdentity FORCE-PASS (orig apk=\"%s\" vc=%lld)", apk, vc);
    DbgLog("exit-probe ReadInstalledIdentity FORCE-PASS (orig apk=\"%s\" vc=%lld)", apk, vc);
    void* ns = KyztNewIl2cppString(use);
    if (ns && pApk) {
        *(void**)pApk = ns;                                  // *out apkPath = 新托管字符串
    } else {
        LOGW(TPL_LOG " exit-probe ReadInstalledIdentity force: il2cpp_string_new 失败，apk 槽未改写");
        DbgLog("exit-probe ReadInstalledIdentity force: string_new null");
    }
    if (pVc) *(long long*)pVc = 35;                          // 与 3.5 vc 对齐（读出来是 35 时更自然）
    return true;
}

// lambda <>c__DisplayClass5_0.<VerifyInstalledPackageAsync>b__0()：先调 orig，只打返回值。
static bool Hooked_LambdaB0(void* self, void* mi) {
    EpHit("LambdaB0", g_epLambdaVa, g_epLambdaHits);
    if (!g_origLambdaB0) return false;
    const bool ret = g_origLambdaB0(self, mi);
    LOGI(TPL_LOG " exit-probe lambda b__0 ret=%d", (int)ret);
    DbgLog("exit-probe lambda b__0 ret=%d", (int)ret);
    return ret;
}

// 无条件安装入口：只在 ver==35 生效（3.3/3.2 行为不动）
static void InstallExitProbeHooks() {
    if (g_epDone.load()) return;
    const int ver = (g_libVer.load() == 0) ? DetectLibVersion() : g_libVer.load();
    if (ver != 35) {
        LOGI(TPL_LOG " exit-probe: skip ver=%d", ver);
        DbgLog("exit-probe: skip ver=%d", ver);
        return;
    }
    g_epDone.store(true);
    g_epExitVa   = (uintptr_t)FileOffToVa(TPL_TARGET_LIB, kOff35RequestExit);
    g_epRevVa    = (uintptr_t)FileOffToVa(TPL_TARGET_LIB, kOff35RevConfirm);
    g_epKillVa   = (uintptr_t)FileOffToVa(TPL_TARGET_LIB, kOff35ExitModified);
    g_epPkgErrVa = (uintptr_t)FileOffToVa(TPL_TARGET_LIB, kOff35ShowPkgErr);
    g_epBlockVa  = (uintptr_t)FileOffToVa(TPL_TARGET_LIB, kOff35ShowBlocking);
    g_epReadIdVa = (uintptr_t)FileOffToVa(TPL_TARGET_LIB, kOff35ReadIdentity);
    g_epLambdaVa = (uintptr_t)FileOffToVa(TPL_TARGET_LIB, kOff35LambdaB0);
    const int a = HookAtRva("ExitProbe", kOff35RequestExit, kInsn35RequestExit,
                            (void*)Hooked_RequestExit, &g_origRequestExit) ? 1 : 0;
    const int b = HookAtRva("ExitProbe", kOff35RevConfirm, kInsn35RevConfirm,
                            (void*)Hooked_RecordRevisionConfirmation,
                            (void**)&g_origRevConfirm) ? 1 : 0;
    const int c = HookAtRva("ExitProbe", kOff35ExitModified, kInsn35ExitModified,
                            (void*)Hooked_ExitModifiedPackage, &g_origExitModified) ? 1 : 0;
    const int d = HookAtRva("ExitProbe", kOff35ShowPkgErr, kInsn35ShowPkgErr,
                            (void*)Hooked_ShowPackageIntegrityError,
                            (void**)&g_origShowPkgErr) ? 1 : 0;
    const int e = HookAtRva("ExitProbe", kOff35ShowBlocking, kInsn35ShowBlocking,
                            (void*)Hooked_ShowBlockingError,
                            (void**)&g_origShowBlocking) ? 1 : 0;
    const int f = HookAtRva("ExitProbe", kOff35ReadIdentity, kInsn35ReadIdentity,
                            (void*)Hooked_ReadInstalledIdentity,
                            (void**)&g_origReadIdentity) ? 1 : 0;
    const int g = HookAtRva("ExitProbe", kOff35LambdaB0, kInsn35LambdaB0,
                            (void*)Hooked_LambdaB0,
                            (void**)&g_origLambdaB0) ? 1 : 0;
    LOGI(TPL_LOG " exit-probe: ver=35 requestExit=%d revConfirm=%d exitModified=%d pkgErr=%d blocking=%d readIdentity=%d lambda=%d",
         a, b, c, d, e, f, g);
    LOGI(TPL_LOG " exit-probe slots: reqExit=0x%lX revConfirm=0x%lX exitModified=0x%lX pkgErr=0x%lX blocking=0x%lX readIdentity=0x%lX lambda=0x%lX",
         (unsigned long)kOff35RequestExit, (unsigned long)kOff35RevConfirm,
         (unsigned long)kOff35ExitModified, (unsigned long)kOff35ShowPkgErr,
         (unsigned long)kOff35ShowBlocking, (unsigned long)kOff35ReadIdentity,
         (unsigned long)kOff35LambdaB0);
    DbgLog("exit-probe: ver=35 reqExit=%d rev=%d kill=%d pkgErr=%d block=%d readId=%d lambda=%d",
           a, b, c, d, e, f, g);
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
//  ⑦-c 早期装钩（3.5；抢在 <Start> 协程发起校验之前）
//
//  实测时序：校验发生在启动后 ~0.1s（<Start>d__41.MoveNext → VerifyInstalledPackageAsync），
//    而框架 mod_thread 的 initDelayMs=3s ⇒ 钩子 3s 后才装，必然全落空（历史 MODHIT=0 的真因）。
//  ⇒ 在 JNI_OnLoad（Entry 的 .onLoad）起 detached 线程，**每 5ms** 轮询 isLibraryLoaded，
//    一为真立刻 TryInit（装 §⑥-b/§⑥-c 的钩），最多轮 2000 次（=10s），然后退出。
//  并发：TryInit 内有 g_initStarted CAS，框架线程进来只会看到「已在初始化」而直接返回，
//    不会出现同址双 DobbyHook（§④ 事故 1 的 SIGSEGV）。
// ============================================================================
static void* KyztEarlyInitThread(void* arg) {
    JavaVM* vm = static_cast<JavaVM*>(arg);
    const auto t0 = std::chrono::steady_clock::now();
    for (int i = 0; i < 2000; ++i) {                 // 2000 × 5ms = 10s 上限
        if (isLibraryLoaded(TPL_TARGET_LIB)) {
            const long long ms = std::chrono::duration_cast<std::chrono::milliseconds>(
                std::chrono::steady_clock::now() - t0).count();
            LOGI(TPL_LOG " early-init: lib appeared after %lld ms", ms);
            DbgLog("early-init: lib appeared after %lld ms", ms);
            if (旷野征途_TryInit(vm)) {
                LOGI(TPL_LOG " early-init: TryInit ok（钩子先于 <Start> 就位）");
                DbgLog("early-init: TryInit ok");
            } else {
                LOGW(TPL_LOG " early-init: TryInit 返回 false（另一线程在装/库未就绪），交回框架线程");
                DbgLog("early-init: TryInit false");
            }
            return nullptr;
        }
        usleep(5000);                                // 5ms
    }
    LOGW(TPL_LOG " early-init: 10s 内未等到 libil2cpp.so，交回框架线程");
    DbgLog("early-init: timeout");
    return nullptr;
}

void 旷野征途_OnLoad(JavaVM* vm) {
    pthread_t tid;
    pthread_attr_t attr;
    pthread_attr_init(&attr);
    pthread_attr_setdetachstate(&attr, PTHREAD_CREATE_DETACHED);
    const int rc = pthread_create(&tid, &attr, KyztEarlyInitThread, (void*)vm);
    pthread_attr_destroy(&attr);
    if (rc == 0) {
        LOGI(TPL_LOG " early-init: 线程已起（5ms 轮询 libil2cpp，上限 10s）");
        DbgLog("early-init: thread started");
    } else {
        LOGW(TPL_LOG " early-init: pthread_create 失败 rc=%d（退回框架 initDelay 路径）", rc);
        DbgLog("early-init: pthread_create fail rc=%d", rc);
    }
}

// ============================================================================
//  ⑧ 对外接口（ModEntry.h 注册表契约，签名不可改）
// ============================================================================
// 初始化一次性闸门（3.5 加入）：框架线程（initDelayMs=3s 后）与 early-init 线程（5ms 轮询）
// 会并发进 TryInit；Dobby 同址双挂 = SIGSEGV（见 §④ 事故 1）⇒ 只允许一个线程真正执行。
static std::atomic<bool> g_initStarted{false};

bool 旷野征途_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    bool expected = false;
    if (!g_initStarted.compare_exchange_strong(expected, true, std::memory_order_acq_rel)) {
        // 另一线程正在（或已经）初始化：不重入，交回调用方；框架循环会看 isReady
        return g_libReady.load();
    }
    if (!isLibraryLoaded(TPL_TARGET_LIB)) {
        g_initStarted.store(false);   // 库还没到：放行后续重试
        return false;
    }

    InstallFaultGuard();          // §④ 先装兜底，再装钩
    g_libReady.store(true);
    // ★ 反篡改绕行必须在这里装（§⑥-b）：C# AOT 校验跑在 GameManagerMono.Start（冷启、功能全关也跑），
    //   与开关无关 ⇒ 不能放进 SyncHooks（那条路 AnyFeatureOn()==false 时直接 return）。
    InstallAntiTamperHooks();
    InstallExitProbeHooks();      // §⑥-c 退出探针：同样只需 ver==35，同样不依赖开关
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
        // 2.4 同口径：Economy22 的字段/属性/方法一律 il2cpp_field_get_offset 按名现取
        //（delta24 已核 2.4 HotUpdate.dll 名字齐备）⇒ 字段常量零改动，只放行版本门。
        if (g_libVer.load() == 22 || g_libVer.load() == 24 || g_libVer.load() == 32 ||
            g_libVer.load() == 33 || g_libVer.load() == 35) {
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
        case 旷野征途_FEAT_SPD: {
            // 只在「开→关」沿置恢复标志：关掉后必须主动写回 1.0，
            // 否则最后一笔 1.8 会一直留在 Time.timeScale 上（真机 2026-11-24：战斗内可能不再调 setter，
            // 用户看到「关了还有加速」，最长要等游戏自己下一次 set_timeScale 才恢复）。
            // UI 线程不直接调 orig（跨线程调 Unity API 风险），只置标志，由 Tick 落地。
            const bool was = g_spd.exchange(enabled);
            // 2026-11-25 诊断：每次切换打一行（频率极低），下次一眼判定 mi 是否为空。
            LOGI(TPL_LOG " spd toggle=%d was=%d mi=%p orig=%p",
                 (int)enabled, (int)was, (void*)g_tsMethod.load(), (void*)g_origSetTimeScale);
            if (!enabled && was) g_spdRestore.store(true);
            else if (enabled)    g_spdRestore.store(false);   // 重新开启 ⇒ 撤销待恢复，避免竞态下先写 1.0
            break;
        }
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

    // OFF 主动恢复：把 1.0 写回去，不等游戏自己下一次调用 setter。
    // 2026-11-25 修复「游戏加速无效」：旧守卫是 `if (m && g_origSetTimeScale)`，而 g_tsMethod 只在
    //   Hooked_SetTimeScale() 里赋值（= 游戏真的调过一次 setter 才有）。3.3 这局游戏在钩子装上
    //   （16:07:50）之后再没调 setter（MODHIT tag=TimeScale 0 次 / set_timeScale call# 0 次）
    //   ⇒ m == nullptr ⇒ 强制与恢复两条路径都被 `m &&` 挡死，加速静默失效（真机 96f0d883）。
    //   现在 **m 允许为 nullptr**，仍原样作为第二参数转发。
    //   「传 nullptr 安全」的依据：capstone 复跑 file 0x37B9344（VA 0x37BD344）
    //     037bd344 str  d8, [sp, #-0x20]!
    //     037bd348 stp  x30, x19, [sp, #0x10]
    //     037bd34c adrp x19, #0x3f52000
    //     037bd350 ldr  x0, [x19, #0xeb8]      ; 缓存的 icall 指针
    //     037bd354 mov  v8.16b, v0.16b          ; 只用 v0 = float value
    //     037bd358 cbnz x0, #0x37bd36c
    //     037bd35c adrp x0, #0xa6b000 / add x0, x0, #0xef / bl #0x1b13b90 / str x0, [x19, #0xeb8]
    //     037bd36c ldp  x30, x19, [sp, #0x10]
    //     037bd370 mov  v0.16b, v8.16b
    //     037bd374 ldr  d8, [sp], #0x20
    //     037bd378 br   x0                      ; 尾调 icall
    //   ⇒ **x1（第二参数 MethodInfo*）全程未被读取**（寄存器足迹只有 x0/x19/x30/v0/v8/sp）。
    // 另一条出口：g_origSetTimeScale 仍为空（inline hook 没装上）时不吃标志，保持 pending 下帧再试。
    if (g_spdRestore.load() && !g_spd.load()) {
        void* m = g_tsMethod.load();
        if (g_origSetTimeScale) {
            g_origSetTimeScale(1.0f, m);
            g_spdRestore.store(false);
            LOGI(TPL_LOG " restore timeScale=1.0 (toggle off) spd=%d mi=%d",
                 (int)g_spd.load(), (int)(m != nullptr));
        }
    }

    // 主动钉 timeScale：不依赖游戏主动调用 setter（本作可能在启动时只调一次）。
    // 2026-11-24：从「每帧」改成 **100 ms 一次**（每帧调既浪费又与开关竞态；1.8 时代的正确形态就是 100ms 重置）。
    if (g_spd.load()) {
        void* m = g_tsMethod.load();
        // 2026-11-25：同上去掉 `m &&`（m 可能为 nullptr —— capstone 已证 x1 不被读，见 OFF 恢复块注释）。
        if (g_origSetTimeScale) {
            const long long nowMs = std::chrono::duration_cast<std::chrono::milliseconds>(
                std::chrono::steady_clock::now().time_since_epoch()).count();
            static long long lastForceMs = 0;
            if (lastForceMs == 0 || (nowMs - lastForceMs) >= 100) {
                lastForceMs = nowMs;
                g_origSetTimeScale(1.8f, m);
                static int tickLog = 0;
                if (tickLog < 3) { ++tickLog; LOGI(TPL_LOG " force timeScale=1.8 (100ms tick) spd=%d mi=%d", (int)g_spd.load(), (int)(m != nullptr)); }
            }
        }
    }

    // ★ 禁止在此 SafeProbe / 扫堆 / 遍历对象 / 逐对象写（Tick 频率高，代价爆炸）
}

void 旷野征途_ForceClear() {
    // 同一类缺陷的另一条出口：强制全清时若加速还开着，也要让 Tick 把 1.0 写回去
    if (g_spd.exchange(false)) g_spdRestore.store(true);   // 等价于 g_spd=false，只是顺带置恢复标志
    g_dmg.store(false);
    g_inv.store(false);
    g_noad.store(false);
    g_reenter.store(false);
    // TODO：新增开关一并复位
}
