// ============================================================================
//  星光菜单游戏模块 — 霓虹深渊2 (nhsy2 / com.veewo.neonabyss2 TapTap 渠道)
//
//  引擎：Unity 6000.0.73f1 / IL2CPP metadata v31 / libil2cpp.so
//  保护：心动 themis + global-metadata 磁盘加密 + so dynsym strtab 加密
//        ⇒ 静态 RVA 不可得 ⇒ 本模块**全部按名挂点**（HookByName）
//        证据：活进程 /proc/PID/mem 中 base+0x3410d0 的 strtab 已解密，
//              含 241 个 il2cpp_* 符号（il2cpp_class_from_name 等），
//              且游戏自身 libunity.so 亦走 dlsym 取 il2cpp API。
//
//  ★ 内置防闪退设施是「代码」不是「文档约定」，删任何一节都可能复现历史闪退：
//      默认 0 hook（懒装） / 双路径挂点 / RVA 漂移拒绝 / 幂等安装 /
//      orig 必调 / reenter 防递归 / SafeWrite 校验写 / 类名缓存 / Tick 轻量
//    v3.1 另加三件，见 §④：并发装钩互斥 / 致命信号兜底 / 槽位熔断
//
//  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
//  路线锁定：10_docs/nhsy2/路线锁定.md（§22 第 5 步，对照方 1:1 核对表）
// ============================================================================

#include "nhsy2Mod.h"

#include <atomic>
#include <cerrno>
#include <cstdint>
#include <cstdio>
#include <cstdlib>    // §⑤.1 maps 快照缓冲（malloc；实测本机 6334 条映射，定长数组会截断）
#include <cstring>
#include <chrono>     // §6.5 登录保命：单调毫秒时钟（steady_clock）

#include <mutex>      // §④ 装钩互斥
#include <sys/system_properties.h>   // §r20 诊断分级：setprop debug.starry.il2cpp_diag
#include <setjmp.h>   // §④ sigsetjmp 保护区
#include <signal.h>   // §④ 致命信号兜底
#include <ucontext.h> // §④.4 故障现场取证：uc_mcontext.pc / regs[30] / sp
#include <sys/mman.h> // §⑤.1 极小函数 4 字节挂钩（邻近跳板 mmap/mprotect）
#include <unistd.h>   // §⑤.1 sysconf(_SC_PAGESIZE)
#include <fcntl.h>    // ★r20 /proc/self/clear_refs（soft-dirty 清除）
#include <cstdarg>    // §⑦.6 async_safe 日志护盾：va_list 转发
#include <dlfcn.h>    // §⑦.6 护盾诊断：dladdr 定位「谁喂了坏格式串」
#include <unwind.h>   // §⑦.6 护盾诊断：_Unwind_Backtrace
#include <sys/prctl.h>       // ★r20c §⑦.6h seccomp：PR_SET_NO_NEW_PRIVS
#include <sys/syscall.h>     // ★r20c §⑦.6h seccomp：__NR_seccomp / __NR_exit_group
#include <linux/seccomp.h>   // ★r20c §⑦.6h SECCOMP_SET_MODE_FILTER / RET_LOG / RET_TRAP / TSYNC
#include <linux/filter.h>    // ★r20c §⑦.6h BPF_STMT / BPF_JUMP / sock_filter / sock_fprog
#include <linux/audit.h>     // ★r20c §⑦.6h AUDIT_ARCH_AARCH64
#include <stddef.h>          // ★r20c §⑦.6h offsetof(struct seccomp_data, …)
#ifndef PR_SET_NO_NEW_PRIVS
#define PR_SET_NO_NEW_PRIVS 38   // ★r20c：bionic 老头文件可能没带，补固定值（内核 ABI 不变）
#endif

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

// 生成器把 [nhsy2] 替换为本游戏标签，日志过滤用 adb logcat -s Mod_Menu
#define TPL_LOG "[nhsy2]"

// 注册表模式不依赖 GameConfig.h（legacy 配置中心）
#define TPL_TARGET_LIB sec_lib_il2cpp()

// ============================================================================
//  ① 开关与 orig 指针
//     g_libReady = 「库已映射」≠「已挂钩」；全开关关闭时应当保持 0 个 hook。
// ============================================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_reenter{false};

static std::atomic<bool> g_inv{false};      // 无敌
static std::atomic<bool> g_dmg{false};      // 伤害倍率（开关）
static std::atomic<int>  g_dmgMul{1};       // 伤害倍率（倍数值 1..100，1=关）
// （g_nocd「技能无CD」已按用户要求删除 —— ★ 2026-10-11 五轮定稿）
static std::atomic<bool> g_full{false};     // 全屏攻击（Attri 属性读点强制，见 §6.6z）
// 2026-10-10 三次定稿（用户拍板）：资源闸**收窄为四个白名单通道**，一功能一开关（1:1），
//  废弃此前的「万能闸」——其余 33 个 CostType 一律走游戏原逻辑，绝不连带修改。
//  白名单 = CostType.Bomb / Coin / Crystal / Key（手雷 / 金币 / 水晶 / 钥匙）。
//  「彩色钥匙」在 metadata 里没有独立字段/枚举项（见 10_docs/霓虹深渊2/路线锁定.md
//  §彩色钥匙考古：静态搜索 0 命中），**不占用任何 CostType 通道**；2026-10-11 五次定稿
//  （用户：「彩色钥匙也要的补上」）后改走**原生通道**：NEONPlayerState 的
//  get_HasInfiniteKey / get_InfiniteKey（键位 13 Toggle_无限彩色钥匙，见 §6.6b）。
static std::atomic<bool> g_bomb{false};     // 无限手雷（CostType.Bomb）
static std::atomic<bool> g_coin{false};     // 无限金币（CostType.Coin）
static std::atomic<bool> g_crystal{false};  // 无限水晶（CostType.Crystal）
static std::atomic<bool> g_key{false};      // 无限钥匙（CostType.Key）
static std::atomic<bool> g_ckey{false};     // 无限彩色钥匙（原生 InfiniteKey / KeySuit 通道）
static std::atomic<bool> g_std{false};      // 解锁标准版
static std::atomic<bool> g_spd{false};      // 移速倍率（UI 只发 nativeSetValue ⇒ 本标志恒假）
static std::atomic<int>  g_spdMul{1};       // 移速倍率（1..10，1=关）
static std::atomic<bool> g_jump{false};     // 无限跳

//  ---- ③.1「滑块项即开关」（★ 2026-10-11 真机实测修正） ----
//  事故：真机把「移速倍率」从 1 拉到 10，日志只有
//        `[数值] #5 Slider_移速倍率:1:10:1 = 10` + `[nhsy2] speed multiplier = 10`，
//        游戏内毫无变化；整场日志**没有一行 `install pass`**。
//  根因：菜单侧对 slider 项只调 `nativeSetValue` → `mb_nativeSetValue`（MainModern.cpp:397），
//        它**只写 g_values[]，从不写 g_features[]**（那是 `nativeSetFeature` 的事）。
//        而 MOD 侧原来把「开」判定写成 `g_spd.load() && g_spdMul > 1` / `g_dmg.load() && g_dmgMul > 1`
//        ⇒ `features[1]/[5]` 恒假 ⇒ AnyFeatureOn() 恒假 ⇒ SyncHooks() 早退 ⇒ **一个 hook 都没装**，
//        所以伤害倍率、移速倍率两个滑块从头到尾都是死的（toggles 项不受影响）。
//  定稿：slider 的**倍数值本身就是开关**（1 = 关，这是滑块的默认位），
//        不再要求额外的 features[] 位；两个 slider 项因此天然满足「一功能一开关」。
static inline bool DmgMulActive() { return g_dmgMul.load(std::memory_order_relaxed) > 1; }
static inline bool SpdMulActive() { return g_spdMul.load(std::memory_order_relaxed) > 1; }

// 命中计数（§29 探针：装机日志里必须能看出挂点是否被真正调用）
static std::atomic<uint32_t> g_hitInv{0};
static std::atomic<uint32_t> g_hitDmg{0};
static std::atomic<uint32_t> g_hitCoin{0};
static std::atomic<uint32_t> g_hitKey{0};
static std::atomic<uint32_t> g_hitCrystal{0};
static std::atomic<uint32_t> g_hitStd{0};
static std::atomic<uint32_t> g_hitSpd{0};
static std::atomic<uint32_t> g_hitJump{0};
static std::atomic<uint32_t> g_hitRes{0};    // 资源通道命中总数（GetAmountRaw/HasEnough/…）
static std::atomic<uint32_t> g_hitBomb{0};   // 手雷通道命中
static std::atomic<uint32_t> g_hitCKey{0};   // 彩色钥匙（InfiniteKey）通道命中

// ★ 2026-10-11 五轮定稿：**无偏 CostType 探针**（用户实测 #2「水晶无效」/#3
//   「金币没开却能买」的取证盲区）。旧版只在 ResGateOn 命中时才打印 ⇒ 只能证明
//   「白名单通道被调用过」，无法回答「商店的判定到底走没走 CostType 接口」。
//   现在改为「进函数即打」，但每个 costType 数值只打前 4 次（上限自带，不会被
//   每帧调用的通道刷屏）—— 见 ResProbeCost。
static std::atomic<int>      g_ctSeen[64];
static std::atomic<uint32_t> g_hitResOdd{0};  // ct 越界（<0 或 >=64）探针

// 商店交易闸（NEON.UI.StoreItem.CanBuy/IsFree）：只认「该道具 _tradingType == Coin」
//  ⇒ 天然 1:1（以挂心脏/诅咒付款的道具不受金币开关影响）。
static std::atomic<uint32_t> g_hitShop{0};
static std::atomic<int32_t>  g_ttCoin{-1};             // NEON.Game.TradingType.Coin（运行期解析）
static std::atomic<size_t>   g_offStoreTradingType{0}; // StoreItem._tradingType
static std::atomic<size_t>   g_offStoreCanPurChase{0}; // StoreItem.CanPurChase

// ---- 战斗：orig ----
typedef bool (*DeductHP_t)(void* thiz, void* damage, int32_t count, void* method);
static DeductHP_t g_origHeartsDeduct   = nullptr;   // NEON.Game.Hearts.DeductHP
static DeductHP_t g_origHitPointsDeduct = nullptr;  // NEON.Game.HitPoints.DeductHP

// 伤害计算入口：HitPoints::CalculationDamage(DamageInfo, Int32) -> Single
typedef float (*CalcDamage_t)(void* thiz, void* damage, int32_t count, void* method);
static CalcDamage_t g_origCalcDamage = nullptr;

// 伤害倍率的唯一改写点：HitPoints::ReallyDeductHP(DamageInfo, Single) -> Void
//   —— 伤害链最后一跳（DeductHP → GetFinalDamage → ReallyDeductHP），
//      只乘最终值实参 ⇒ 不依赖任何字段布局、结构上不可能平方。
typedef void (*ReallyDeduct_t)(void* thiz, void* damage, float finalDamage, void* method);
static ReallyDeduct_t g_origReallyDeduct = nullptr;

// ================= ★ r14 全屏攻击：挂点迁到 Killable 族（历史：r11-r13 的伤害广播） =================
//  用户 m04997「两个功能仍然没有效果 …… 不行就给我换方法」→ 试过「在 ReallyDeductHP 这一跳
//  把同一份伤害按 klass 扫内存广播给同类 HP 实例」；真机取证（记录仪 20261012_025029）：
//  `fullscreen bcast scan #1 found=41` 之后 1 ms 即 `[THEMIS]: get signal 11` —— 内存扫描
//  分不清活体与已释放/复用的尸体，一调原函数就踩错内存 ⇒ 该机制**整段删除**。
//  r14 改为：Killable.TakeDamage 记录这一下 + Killable.Update 对在场活体逐个施放（见 :1290 段）。
static thread_local bool t_inBcast = false;        // 施放重入闸（防级联）

// ================= ★ r11 无限水晶：直接挂「水晶计数」的共享实现 =================
//  r9/r10 两条路都无效的根因推断：`SpriteManager.GetCrystalSpriteCount`（水晶计数
//  getter，ret=Int32）是 8 字节 veneer `mov w1,#imm; b <共享实现>` —— 我们的入口形态
//  闸门**主动跳过**它（避免 Dobby 16 字节补丁吃邻居），所以从来没挂上过；
//  于是「水晶够不够」的判定一直读的是真实值 ⇒ 用户看到「需要更多水晶」。
//  修法：不解 veneer，改为**按地址挂它的目标（共享实现）**，在钩子里只拦
//  「选择子 == veneer 里那个立即数」的调用 ⇒ 无限水晶开闸时直接返回 9999。
//  寄存器保持：处理函数与跳板声明同样的 6 参（x0..x5）原样转发，避免未知参数被截断。
typedef int32_t (*Shared6_t)(void*, void*, void*, void*, void*, void*);
// ★ r12b：**不再挂共享实现本身**（它的 16 字节窗口里有邻居方法入口 ⇒ Dobby 会吃掉邻居）。
//   改为只把 veneer 的第一条指令（`mov w1,#imm`）换成 `b <hook>`，钩子内**直接调用真实现**。
//   ⇒ 这个指针是「真实现地址」（不是 trampoline），调用时必须自己补上 w1=选择子。
static Shared6_t g_implCrystalCount = nullptr;
static bool g_crystalVeneerHooked = false;
static uintptr_t g_crystalCountSel = 0;            // veneer 里的立即数（=水晶计数的选择子/偏移）
static std::atomic<uint32_t> g_hitCrystalImpl{0};  // 命中次数
static std::atomic<uint32_t> g_crystalImplShown{0};
static std::atomic<uint32_t> g_crystalImplOtherShown{0};  // ★ r12 诊断：非目标选择子的调用（≤8 条）

// ---- 移动 / 跳跃：orig ----
//  PlayerActor.get_MovementSpeed() -> Single  （Mover.Init(deltaAcc,maxSpeed) 的速度源）
//  Mover.get_FinalMaxSpeed()      -> Single  （夹取上限，仅做探针对照）
//  PlayerActor.get_FinalMultipleJumpCountLimit() -> Int32
typedef float   (*FloatGetter_t)(void* thiz, void* method);
static FloatGetter_t g_origGetMovementSpeed  = nullptr;
static FloatGetter_t g_origGetFinalMaxSpeed  = nullptr;
// 与下方 IntGetter_t 同型（此处独立声明，避免依赖声明顺序）
typedef int32_t (*IntGetterI_t)(void* thiz, void* method);
static IntGetterI_t  g_origGetFinalJumpLimit = nullptr;
static IntGetterI_t  g_origGetJumpLimit      = nullptr;

// ---- 资源：orig ----
typedef int32_t (*IntGetter_t)(void* thiz, void* method);
typedef bool    (*BoolGetter_t)(void* thiz, void* method);
static IntGetter_t  g_origGetCoin    = nullptr;
static IntGetter_t  g_origGetMaxCoin = nullptr;
static IntGetter_t  g_origGetKey     = nullptr;
static IntGetter_t  g_origGetCrystal = nullptr;
static IntGetter_t  g_origGetGitter  = nullptr;
static BoolGetter_t g_origHasInfKey  = nullptr;
static IntGetter_t  g_origGetInfKey  = nullptr;   // get_InfiniteKey（KeySuit 通道计数）
// 次要读取面（商店/门禁走的是 CoinValue/RealKey/KeyValue 这几个）
static IntGetter_t  g_origGetCoinValue = nullptr;
static IntGetter_t  g_origGetRealKey   = nullptr;
static IntGetter_t  g_origGetKeyValue  = nullptr;

// ============================================================================
//  ★ 2026-10-12 r8：用户实测 m04183「全屏攻击无效果、无限水晶没效果」的两处新挂点
// ============================================================================
//  ① 全屏攻击 —— 真正生效的射程读点：`NEONPlayerState.get_ProjectileRange()`（实例、argc=0）。
//     旧路线（§⑥.7 kFullRules 按 Attri 键名）实测整场 58 个属性键里**没有任何
//     range/scope 类键**（只命中 finalbulletlifespanmult）⇒ 对射程无能为力。
//     ★ r8 首轮真机已证：这个 getter 的返回类型**不是数值**。装钩诊断打印的真名是
//     `Veewo.Stats.Stat`（Veewo.Stat.dll，TYPE index=25553，40 方法 / 9 字段）——
//     它是游戏自己的 stat 包装对象（字段 `baseValue` / `_cachedValue`，属性 `get_Value`）。
//     所以 r8 那套「伪造一个 double 当返回值」＝把垃圾当**对象指针**交给游戏 ⇒ 必然崩。
//     r8b 改法（本轮落地）：本层只**透传**并记下这个 Stat 实例指针；
//     真正的强制点挪到 `Veewo.Stats.Stat.get_Value`（argc=0 -> System.Single），
//     且只在 `thiz == g_projRangeStat`（同一个实例）时强制 ⇒ 1:1 只改「射程」这一条 stat，
//     不波及伤害 / 移速 / 其它任何 stat；也不写任何字段与缓存（最坏只是不生效）。
static std::atomic<uint32_t> g_hitRange{0};
static std::atomic<uint32_t> g_hitRangeForce{0};
static std::atomic<uint32_t> g_hitDamageSize{0};
typedef void* (*ObjGetter_t)(void* thiz, void* method);
static ObjGetter_t   g_origProjRange  = nullptr;  // NEONPlayerState.get_ProjectileRange -> Veewo.Stats.Stat*
static ObjGetter_t   g_origDamageSize = nullptr;  // NEONPlayerState.get_DamageSize（同类对象，只读探针）
static FloatGetter_t g_origStatValue  = nullptr;  // Veewo.Stats.Stat.get_Value -> System.Single
static std::atomic<void*> g_projRangeStat{nullptr};
static const float kFullRangeForce = 99999.0f;   // 射程强制值（真类型 Single，不再赌调用约定）

//  ② 无限水晶 —— 真正的水晶通道在 `NEON.Game.SpriteManager`。
//     依据：`NEONPlayerState` 的 275 个字段里**没有任何 crystal 字段**（离线字段表实证），
//     `get_CurrentSpriteCrystalCount` 只是 SpriteManager 用 `SetPlayerStateSpriteCount`
//     推过去的展示值 ⇒ 旧挂点（Res_Crystal）实测整场 0 次调用＝死路径。
//     SpriteManager 三件套（实例方法）：GetCrystalSpriteCount(argc=0)->Int32、
//     UseCrystalSprite(argc=1)->Int32、AddCrystalSprite(argc=0)->Void；
//     另有 NEONPlayerState.ConsumeSpriteAtkRaw(argc=2)->Bool（精灵攻击消耗，开闸恒 true）。
static std::atomic<uint32_t> g_hitCrystalCount{0};
static std::atomic<uint32_t> g_hitUseCrystal{0};
static std::atomic<uint32_t> g_hitSpriteAtk{0};
static IntGetter_t  g_origGetCrystalSpriteCount = nullptr;
typedef int32_t (*SpriteArg1_t)(void* thiz, intptr_t a, void* method);
static SpriteArg1_t g_origUseCrystalSprite = nullptr;
typedef bool (*ConsumeSpriteAtk_t)(void* thiz, intptr_t a, intptr_t b, void* method);
static ConsumeSpriteAtk_t g_origConsumeSpriteAtk = nullptr;
static void* g_miAddCrystalSprite = nullptr;      // SpriteManager.AddCrystalSprite()（退款/补发用）
static std::atomic<bool> g_grantCrystalTried{false};
// 扣减面：恒不扣（参数一律 intptr_t 透传，且**不调用 orig** ⇒ 参数类型无关紧要）
typedef bool (*ConsumeRaw_t)(void* thiz, intptr_t a, intptr_t b, void* method);
static ConsumeRaw_t g_origConsumeCoin = nullptr;
static ConsumeRaw_t g_origConsumeKey  = nullptr;

// ---- 通用资源通道（★ 2026-10-08 实测「无限金币/钥匙/水晶无效」后的真挂点）----
//  用户实测：「购买提示需要更多金币 / 使用提示需要更多水晶」⇒ 商店走的不是
//  get_Coin/get_Key 这几个属性，而是 NEONPlayerState 的通用 CostType 接口：
//    GetAmount(CostType,T#24771)            -> Int32   「我有多少」（带转换器版）
//    GetAmountRaw(CostType,T#24771)         -> Int32   「我有多少」
//    HasEnough(CostType,T#34288,T#24771)    -> Boolean 「买得起吗」
//    HasEnoughRaw(CostType,T#34288,T#24771) -> Boolean
//    HasEnoughRaw(CostType,Single)          -> Boolean 「买得起吗（向上取整）」
//    ConsumeRaw(CostType,Int32,Int32,Boolean)-> Boolean 「消费」
//    SetResource(CostType,Int32)            -> Void    「写入」（§21.1 数据优先）
//  ABI 全部经 metadata 签名核对：含浮点实参者一律显式声明 float（s 寄存器），
//  其余用 intptr_t 原样透传；argc 有歧义的 ConsumeRaw(3) 两个重载**故意不挂**
//  （无法区分 Single/int 形参，关开关时的 orig 透传会错寄存器）。
typedef int32_t (*ResAmount_t)(void* thiz, int32_t costType, intptr_t p2, void* method);
static ResAmount_t g_origGetAmountRaw = nullptr;
static ResAmount_t g_origGetAmount    = nullptr;

typedef bool (*ResEnough3_t)(void* thiz, int32_t costType, intptr_t amount, intptr_t flag, void* method);
static ResEnough3_t g_origHasEnough    = nullptr;
static ResEnough3_t g_origHasEnoughRaw = nullptr;

typedef bool (*ResEnough2F_t)(void* thiz, int32_t costType, float amountRoundUp, void* method);
static ResEnough2F_t g_origHasEnoughRawF = nullptr;

typedef bool (*ResConsume4_t)(void* thiz, int32_t costType, int32_t amount, int32_t reported,
                              bool silent, void* method);
static ResConsume4_t g_origConsumeRaw4 = nullptr;

typedef void (*ResSet_t)(void* thiz, int32_t costType, int32_t amount, void* method);
static ResSet_t g_origSetResource = nullptr;

typedef void (*ResUseCoin_t)(void* thiz, int32_t amount, void* method);
static ResUseCoin_t g_origUseCoinByCovert = nullptr;

// ============================================================================
//  ② 字段偏移（全部运行时用 il2cpp_field_get_offset 取，静态 dump.cs 不可得）
//     FieldOffset 返回 0 = 未找到（这些字段都在对象头之后，真偏移不会是 0）
// ============================================================================
static std::atomic<size_t> g_offHeartsInv{0};       // Hearts.isInvincible  (bool)
static std::atomic<size_t> g_offHeartsUnkill{0};    // Hearts.UnKillable    (bool)
static std::atomic<size_t> g_offHeartsDamageCD{0};  // Hearts.isDamageCD    (bool, slot=18)
static std::atomic<size_t> g_offDmgDamage{0};       // DamageInfo.damage    (float)
static std::atomic<size_t> g_offDmgMult{0};         // DamageInfo.damageMult(float)
static std::atomic<size_t> g_offSecSkillTimer{0};   // PlayerActor.SecSkillTimer (float)
static std::atomic<size_t> g_offPassiveSkillTimer{0};// PlayerActor.PassiveSkillTimer (float)
static std::atomic<size_t> g_offFullGameUnlocked{0};// LoginService._isFullGameUnlocked (bool)
static std::atomic<size_t> g_offLegacyStdOwned{0};  // LoginService._legacyStandardOwned (bool)
static std::atomic<size_t> g_offStdDlcOwned{0};     // LoginService._standardDlcOwned (bool)

// 类指针缓存（装钩时解析一次）
static void* g_clsHearts      = nullptr;
static void* g_clsDamageInfo  = nullptr;
static void* g_clsPlayerActor = nullptr;
static void* g_clsLoginSvc    = nullptr;

// 枚举字面量 StandardEditionAccessResult.Allowed（运行时读，避免硬编码错值）
static std::atomic<int> g_allowedEnum{1};

// DamageInfo.IsFromLocalPLayer 的方法指针（只在本地玩家造成的伤害上放大）
static void* g_fnIsFromLocalPlayer = nullptr;

// 资源给量：足够大但不溢出 int32
static const int kInfiniteAmount = 9999;

// ============================================================================
//  ③ 安全助手（防闪退核心；新代码禁止绕开它们裸读写内存）
// ============================================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

static void SafeWriteI32(void* obj, size_t off, int32_t v) {
    if (!OkPtr(obj) || !off) return;
    Il2Cpp::WriteI32(obj, off, v);
}

static void SafeWriteF32(void* obj, size_t off, float v) {
    if (!OkPtr(obj) || !off) return;
    Il2Cpp::WriteF32(obj, off, v);
}

static void SafeWriteU8(void* obj, size_t off, uint8_t v) {
    if (!OkPtr(obj) || !off) return;
    Il2Cpp::WriteU8(obj, off, v);
}

static int32_t SafeReadI32(void* obj, size_t off, int32_t defv) {
    if (!OkPtr(obj) || !off) return defv;
    return Il2Cpp::ReadI32(obj, off);
}

static float SafeReadF32(void* obj, size_t off, float defv) {
    if (!OkPtr(obj) || !off) return defv;
    return Il2Cpp::ReadF32(obj, off);
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
//     三段真实事故：并发装钩竞态 / Dobby 内部 SIGSEGV 接不住 / 反复炸槽位
// ============================================================================

// ---- 4.1 互斥：装钩串行 ----
static std::mutex g_installMutex;   // 保护「装钩全过程」，只在 SyncHooks 里取
static std::mutex g_guardMutex;     // 只保护一次性「安装信号处理器」动作

// ---- 4.1b r27 最小化二分开关（定义见 §④ 前部；此处只放读取，任何地方都能问） ----
static int SafeMask() {
    static int cached = -1;
    if (cached >= 0) return cached;
    char b[PROP_VALUE_MAX] = {0};
    const int n = __system_property_get("debug.starry.nhsy2_safemask", b);
    /* ★r27 定案（真机二分，vc646）：**默认 4 = 关掉 ReProtectExecPages**。
     *  bisect 结果：mask=28（守护线程开、reprotect 关）稳；mask=24（开 reprotect）⇒ 必进
     *  themis 崩溃处理器无限递归风暴。即「每 500 ms 对 rwx 代码页做 mprotect(R-X)」这个小动作
     *  在本版被判定为篡改。它是 r20 的辅助手段（防 themis 回改页权限），本版代价远大于收益 ⇒ 默认关。 */
    int v = (n > 0) ? atoi(b) : 4;
    if (v < 0) v = 0;
    if (v > 31) v = 31;
    cached = v;
    return cached;
}

// ---- 4.2 本线程故障态（thread_local：hook 体在多线程并发） ----
static thread_local volatile sig_atomic_t   t_inGuard    = 0;
static thread_local volatile sig_atomic_t   t_faultDepth = 0;
static thread_local volatile int            t_faultSig   = 0;
static thread_local volatile void*          t_faultAddr  = nullptr;
//  ★ r16：故障现场（定位用）。旧版只存了 si_addr，拿不到 PC/LR ⇒ 真机上只能看到
//  THEMIS 转发后那张「二次故障」tombstone（其 memcpy 用的还是垃圾上下文，272 字节）。
static thread_local volatile uintptr_t      t_faultPc    = 0;
static thread_local volatile uintptr_t      t_faultLr    = 0;
static thread_local volatile uintptr_t      t_faultSp    = 0;
//  ★ r16：保护区**栈顶** jmp_buf。旧实现只有一个全局 t_faultJmp，嵌套调用互相覆盖
//  （§⑦ 注释里记过：内层 RunGuarded 已返回后再来信号，siglongjmp 会跳到失效帧 ⇒ 栈毁）。
//  现在每次进入保护区都在**调用方自己**的栈上开一个 buf，用 t_jmpTop 串成栈：信号永远
//  longjmp 到最内层仍然有效的那一个。
static thread_local sigjmp_buf*             t_jmpTop     = nullptr;

// 备用信号栈（SIGSEGV 时原栈可能已不可用）；64KB/线程
static thread_local char t_altStack[65536];
static thread_local bool t_altStackSet = false;

// ---- 4.3 被链式接管的旧处理器 ----
static struct sigaction g_oldSegv;
static struct sigaction g_oldBus;
static struct sigaction g_oldIll;
static std::atomic<bool> g_guardArmed{false};
static bool g_lastFatal = false;   // 只在 g_installMutex 保护下读写

// ---- 4.4a 未捕获崩溃现场采集（★r24 防闪退：兜底之外的「取证兜底」）----
//  保护区内的故障会被就地吞掉（GuardedOr/FS_GUARDED）；但保护区**外**的故障（我们没预料到的
//  野读、线程栈溢出、Dobby 迁移指令异常…）必须链给系统 ⇒ 进程仍会死。死了也要留下可归因的证据：
//  处理器里只做「写定长环 + 一次 __android_log_print（纯整数，不 snprintf 任何字符串）」，绝不
//  fopen/malloc/递归加锁 —— 那些在信号上下文里会二次崩。
struct UncaughtFaultRec {
    int      sig;
    int      tid;
    uintptr_t addr, pc, lr, sp;
};
static UncaughtFaultRec g_ucFaults[8];
static std::atomic<uint32_t> g_ucFaultW{0};   // 写序号（>1 轮时环形覆盖）
static std::atomic<uint32_t> g_ucFaultFlushed{0};

static void RecordUncaughtFault(int sig, siginfo_t* si, void* uc) {
    const uint32_t idx = g_ucFaultW.fetch_add(1, std::memory_order_acq_rel);
    UncaughtFaultRec* r = &g_ucFaults[idx & 7u];
    r->sig  = sig;
    r->tid  = (int)syscall(__NR_gettid);
    r->addr = si ? (uintptr_t)si->si_addr : 0;
    if (uc) {
        const ucontext_t* u = static_cast<const ucontext_t*>(uc);
        r->pc = (uintptr_t)u->uc_mcontext.pc;
        r->lr = (uintptr_t)u->uc_mcontext.regs[30];
        r->sp = (uintptr_t)u->uc_mcontext.sp;
    } else {
        r->pc = r->lr = r->sp = 0;
    }
    // 唯一一次异步上下文日志：只打整数（走 Logger.h 的 LOGE 宏，底层同为 __android_log_print，
    // 内部只写 socket、不做堆分配；用宏而不是裸调，是为了过 check_release_clean 的「裸日志」门禁）
    LOGE(TPL_LOG " ★未捕获崩溃现场 sig=%d tid=%d addr=%p pc=%p lr=%p sp=%p"
                 "（保护区外故障：已落环，随后按系统默认链继续）",
         r->sig, r->tid, (void*)r->addr, (void*)r->pc, (void*)r->lr, (void*)r->sp);
}

// ---- 4.4 故障处理器 ----
static void TplFaultHandler(int sig, siginfo_t* si, void* uc) {
    if (t_inGuard && t_jmpTop) {
        if (++t_faultDepth > 4) {
            signal(sig, SIG_DFL);
            raise(sig);
            return;
        }
        t_faultSig  = sig;
        t_faultAddr = si ? si->si_addr : nullptr;
        //  ★ r16：把故障现场一起带走。真机上 THEMIS 会把原始崩溃吞掉（只留一张「转发后
        //  二次故障」的系统 tombstone，里面 memcpy 用的还是垃圾上下文），第一现场永远查不到，
        //  所以必须在处理器里自己落 PC/LR/SP —— 这是唯一能定位「游戏内部哪条指令崩」的机会。
        if (uc) {
            const ucontext_t* u = static_cast<const ucontext_t*>(uc);
            t_faultPc = (uintptr_t)u->uc_mcontext.pc;
            t_faultLr = (uintptr_t)u->uc_mcontext.regs[30];
            t_faultSp = (uintptr_t)u->uc_mcontext.sp;
        } else {
            t_faultPc = t_faultLr = t_faultSp = 0;
        }
        siglongjmp(*t_jmpTop, 1);   // 永不返回（跳到最内层仍然有效的保护区）
    }

    RecordUncaughtFault(sig, si, uc);      // ★r24：保护区外 ⇒ 先取证，再照原样链给系统

    const struct sigaction* old = (sig == SIGSEGV) ? &g_oldSegv
                               : (sig == SIGBUS)  ? &g_oldBus
                                                  : &g_oldIll;
    if (old->sa_sigaction && old->sa_sigaction != TplFaultHandler) {
        old->sa_sigaction(sig, si, uc);   // ★ r16：透传真上下文（旧版传 nullptr，下游只能拿到垃圾）
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

// ---- 4.6b 周期性抢回信号链头位（★r24 关键）----
//  问题（真机 vc646 实测）：Android 的 sigaction 是**链式**的，后注册者排在链头。
//  libthemis-v8 在我们之后才装载并注册它自己的 SIGSEGV 处理器 ⇒ 我们所有「故意造一次
//  可恢复故障」的探针（SafeProbe/SelfProbe）都会先落到 themis 的处理器里，而它本版的
//  处理器自己要 pthread_mutex_lock 一个失效的锁 ⇒ SEGV_ACCERR ⇒ 再进 sigchain ⇒
//  **无限递归风暴**（实测 20 万~166 万帧，进程必死）。
//  对策：守护线程每拍重装一次我们的处理器（只换 handler，不覆盖 g_old*），保持我们在链头；
//  探针故障仍由我们 longjmp 收回，themis 根本看不到。
static void RearmFaultGuard() {
    if (!g_guardArmed.load()) return;
    struct sigaction cur;
    memset(&cur, 0, sizeof(cur));
    if (sigaction(SIGSEGV, nullptr, &cur) != 0) return;
    if (cur.sa_sigaction == TplFaultHandler) return;      // 已在链头，什么都不做
    struct sigaction sa;
    memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = TplFaultHandler;
    sa.sa_flags     = SA_SIGINFO | SA_ONSTACK | SA_NODEFER;
    sigemptyset(&sa.sa_mask);
    struct sigaction prev;
    memset(&prev, 0, sizeof(prev));
    if (sigaction(SIGSEGV, &sa, &prev) != 0) return;
    sigaction(SIGBUS, &sa, nullptr);
    sigaction(SIGILL, &sa, nullptr);
    /* 注意：**不覆盖 g_oldSegv/g_oldBus/g_oldIll** —— 那三个是「首次安装时排在我们前面的
     * 处理器」，是我们的链式回退目标。这里只是把自己挪回链头，捕获到的 prev 仅用于日志。 */
    static std::atomic<int> rn{0};
    const int k = rn.fetch_add(1) + 1;
    if (k <= 8)
        LOGW(TPL_LOG " fault guard 抢回链头 #%d：被顶掉的是 %p（探针故障不再落到它们手里）",
             k, (void*)prev.sa_sigaction);
}

// ---- 4.7 保护区包装器 ----
static int DiagLevel();   // ★r20 前置声明（定义见 §④ 段 EnsureApi 上方）
/* ★r20 诊断补丁闸：按 tag 逐类放行，用于判定「哪一类代码补丁触发 ~3.8 s 后的干净退出」。
 *   L≤4 全禁；L5 只放 libc 日志护盾；L6 再加登录保命钩；L7（默认）全放；
 *   L8 = 只放 libc 护盾 + `Inv_DeductHP`（Hearts.DeductHP，**与启动路径完全无关**的单个玩法方法）
 *        —— 用于区分「全局定时完整性校验」与「钩坏启动路径」：只挂它仍退 ⇒ 定时校验。 */
static bool DiagAllowPatch(const char* tag) {
    const int L = DiagLevel();
    if (L == 9) return false;               // L9：全禁补丁，只做「同值写」判决实验
    if (L >= 7 && L != 8) return true;
    if (L <= 4) return false;
    if (!tag) return false;
    if (strcmp(tag, "asf.shield") == 0) return true;
    if (L == 5) return false;
    if (L == 8) return strcmp(tag, "Inv_DeductHP") == 0;
    return strncmp(tag, "Login_", 6) == 0;
}
static bool DobbyHookGuarded(const char* tag, void* target, void* hook, void** orig) {
    if (!DiagAllowPatch(tag)) {
        LOGW(TPL_LOG " diag L%d：跳过 Dobby 装钩 %s", DiagLevel(), tag);
        return false;
    }
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    sigjmp_buf  buf;                        // ★ r16：本函数**自己的**帧，穿成保护区栈
    sigjmp_buf* prev = t_jmpTop;
    t_jmpTop = &buf;
    if (sigsetjmp(buf, 1) == 0) {
        t_inGuard = 1;
        int rc = DobbyHook(target, hook, orig);
        t_inGuard = 0;
        t_jmpTop  = prev;                   // ★ 返回前必须还栈，否则信号会跳到失效帧
        if (rc != 0) {
            LOGW(TPL_LOG " %s dobby fail rc=%d", tag, rc);
            return false;
        }
        return true;
    }
    t_inGuard = 0;
    t_jmpTop  = prev;
    g_lastFatal = true;
    LOGW(TPL_LOG " %s dobby SIG(%d) @%p — 槽位熔断，游戏继续",
         tag, (int)t_faultSig, (void*)t_faultAddr);
    if (orig) *orig = nullptr;   // ★ 必须清掉可能被半写的 orig
    return false;
}

static bool ReadU32Guarded(const char* tag, const void* p, uint32_t* out) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    sigjmp_buf  buf;
    sigjmp_buf* prev = t_jmpTop;
    t_jmpTop = &buf;
    if (sigsetjmp(buf, 1) == 0) {
        t_inGuard = 1;
        *out = *reinterpret_cast<const volatile uint32_t*>(p);
        t_inGuard = 0;
        t_jmpTop  = prev;
        return true;
    }
    t_inGuard = 0;
    t_jmpTop  = prev;
    LOGW(TPL_LOG " %s prologue read SIG(%d) @%p — 跳过该槽位",
         tag, (int)t_faultSig, (void*)t_faultAddr);
    return false;
}

static bool RunGuarded(const char* tag, void (*fn)(void*), void* arg) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    sigjmp_buf  buf;
    sigjmp_buf* prev = t_jmpTop;
    t_jmpTop = &buf;
    if (sigsetjmp(buf, 1) == 0) {
        t_inGuard = 1;
        fn(arg);
        t_inGuard = 0;
        t_jmpTop  = prev;
        return true;
    }
    t_inGuard = 0;
    t_jmpTop  = prev;
    LOGW(TPL_LOG " %s 回调内 SIG(%d) @%p — 已吞掉，游戏继续",
         tag, (int)t_faultSig, (void*)t_faultAddr);
    return false;
}

// ---- 4.8 故障定位：把绝对地址写成「库名+偏移」（★ r16 新增）----
//  只读 /proc/self/maps，普通上下文调用（不在信号处理器里），安全。
//  偏移基准取该模块**最低**的一段映射起始（＝装载基址），所以 `库名+0x…` 里的数字
//  可以直接和静态反汇编（capstone 的 vaddr / RVA）对齐比对。
static void ModOff(uintptr_t addr, char* out, size_t cap) {
    if (!out || !cap) return;
    out[0] = '\0';
    if (!addr) { std::snprintf(out, cap, "-"); return; }
    FILE* f = std::fopen("/proc/self/maps", "re");
    if (!f) { std::snprintf(out, cap, "%p", (void*)addr); return; }

    char      line[320];
    char      hitPath[192] = {0};
    uintptr_t hitLo = 0;
    bool      hit    = false;
    uintptr_t minLo  = 0;
    bool      minSet = false;
    while (std::fgets(line, sizeof(line), f)) {
        unsigned long lo = 0, hi = 0;
        char perms[8] = {0}, path[200] = {0};
        if (std::sscanf(line, "%lx-%lx %7s %*s %*s %*s %199s", &lo, &hi, perms, path) < 3) continue;
        if (!hit && addr >= (uintptr_t)lo && addr < (uintptr_t)hi) {
            hit = true; hitLo = (uintptr_t)lo;
            std::snprintf(hitPath, sizeof(hitPath), "%s", path);
        }
        // 同一模块的最低映射（用于算装载基址）：无 path 的匿名段不参与
        if (path[0] == '/' && !minSet) { /* 占位：下面按需覆盖 */ }
        if (path[0] == '/' && hitPath[0] == '/' && std::strcmp(path, hitPath) == 0) {
            if (!minSet || (uintptr_t)lo < minLo) { minLo = (uintptr_t)lo; minSet = true; }
        }
    }
    std::fclose(f);

    if (!hit)            { std::snprintf(out, cap, "%p", (void*)addr); return; }
    if (hitPath[0] != '/') { std::snprintf(out, cap, "[anon]+0x%zx", (size_t)(addr - hitLo)); return; }
    const char* base = std::strrchr(hitPath, '/');
    base = base ? base + 1 : hitPath;
    const uintptr_t ref = (minSet && minLo && minLo <= addr) ? minLo : hitLo;
    std::snprintf(out, cap, "%s+0x%zx", base, (size_t)(addr - ref));
}

//  故障已就地拦下 ⇒ 落一行带 PC/LR 的定位日志（★ r16：r11–r15 的 0xb8 崩溃之所以
//  始终查不出「游戏内部哪条指令崩」，就是因为原始现场被 THEMIS 吞掉、我们也没自己取证）。
static void LogGuardFault(const char* tag) {
    char pc[128], lr[128];
    ModOff((uintptr_t)t_faultPc, pc, sizeof(pc));
    ModOff((uintptr_t)t_faultLr, lr, sizeof(lr));
    LOGW(TPL_LOG " ★原函数崩溃已拦下 %s SIG(%d) pc=%s lr=%s sp=%p addr=%p"
                 " —— 就地吞掉：游戏不闪退，本次按「未生效」处理",
         tag, (int)t_faultSig, pc, lr, (void*)t_faultSp, (void*)t_faultAddr);
}

// ---- 4.9 调原函数保护区（★ r16）----
//  用法：  bool ok = false; T r = 安全默认值;
//          FS_GUARDED("orig.X", ok, { r = g_origX(a, b); });
//          if (!ok) { 用 r 的默认值 }
//  语义：正常 ⇒ ok=true；原函数内部炸了 ⇒ ok=false（调用方给安全返回值），
//        现场 PC/LR/SP 落盘。这样「游戏自己在我们调进去的链上崩」既不会带走进程，
//        又能一次拿到定位（不用再靠 THEMIS 那张二次故障 tombstone）。
#define FS_GUARDED(tag, okvar, ...)                                          \
    do {                                                                     \
        ArmFaultGuardForThread();                                            \
        t_faultDepth = 0;                                                    \
        sigjmp_buf _fsbuf;                                                   \
        sigjmp_buf* _fsprev = t_jmpTop;                                      \
        t_jmpTop = &_fsbuf;                                                  \
        if (sigsetjmp(_fsbuf, 1) == 0) {                                     \
            t_inGuard = 1;                                                   \
            __VA_ARGS__;                                                     \
            t_inGuard = 0;                                                   \
            t_jmpTop  = _fsprev;                                             \
            (okvar)   = true;                                                \
        } else {                                                             \
            t_inGuard = 0;                                                   \
            t_jmpTop  = _fsprev;                                             \
            LogGuardFault(tag);                                              \
            (okvar)   = false;                                               \
        }                                                                    \
    } while (0)

// ---- 4.9b 调 orig 的「带回退值」包装（★r24 防闪退兜底）----
//  §④ 骨架的 FS_GUARDED 只覆盖了 r11–r16 出过事的几处（伤害/Killable 链）；
//  其余 40+ 个 hook 体仍是裸调 orig：一旦游戏原函数在我们这一跳里崩（我们改过实参、
//  或该机型对象形态不同），t_inGuard==0 ⇒ 信号直接链给 ART ⇒ **进程闪退**。
//  这里把「调 orig」统一收口成 6 行模板：正常返回原值，崩了则记一行定位日志并回退到
//  调用方给的**安全默认值**（比例键 0/false/nullptr），进程继续跑。
//  代价：每次多一次 sigsetjmp（arm64 ~10–20 ns），只在被挂钩的方法上付出。
template <typename R, typename F>
static R GuardedOr(const char* tag, R defv, F&& body) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    sigjmp_buf  buf;
    sigjmp_buf* prev = t_jmpTop;
    t_jmpTop = &buf;
    if (sigsetjmp(buf, 1) == 0) {
        t_inGuard = 1;
        R v = body();
        t_inGuard = 0;
        t_jmpTop  = prev;
        return v;
    }
    t_inGuard = 0;
    t_jmpTop  = prev;
    LogGuardFault(tag);
    return defv;
}

template <typename F>
static bool GuardedVoid(const char* tag, F&& body) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    sigjmp_buf  buf;
    sigjmp_buf* prev = t_jmpTop;
    t_jmpTop = &buf;
    if (sigsetjmp(buf, 1) == 0) {
        t_inGuard = 1;
        body();
        t_inGuard = 0;
        t_jmpTop  = prev;
        return true;
    }
    t_inGuard = 0;
    t_jmpTop  = prev;
    LogGuardFault(tag);
    return false;
}


// ---- 4.8 槽位熔断 ----
//  ★ 2026-10-12（r8c 根因）：kSlotsMax 必须 ≥ kSlotsCount。原来写死 64，而
//   表已扩到 66 ⇒ 循环 `i < kSlotsCount && i < kSlotsMax` 在 i=64 处静默截断，
//   最后两槽（Res_UseCrystal / Res_SpriteAtk）**从未被尝试安装**，日志里既没有
//   probe 也没有 skip、也没进 dead ⇒ 「无限水晶没效果」而查不出原因（真机证据：
//   install pass: ok=61 dead=0/66、probe 行 64 条主表 + 6 条登录槽 = 70 条，
//   最后的 Res_CrystalCount 之后直接跳到 install pass）。教训：上限类常量必须
//   配一条 static_assert 把「表比上限大」变成编译期错误，而不是运行期静默丢槽。
static const int kSlotsMax = 96;
static bool g_slotDead[kSlotsMax];
// （编译期断言写在 `kSlotsCount` 定义之后 —— 常量必须先声明，见本文件槽位表尾部）
static std::atomic<int> g_installedCount{0};
// 是否已跑过一次完整装钩。结构性失败的槽位（本机 ±124MB 内无跳板空洞、
// 入口是 `br xN` veneer 等）再重试一万次也还是失败，而每轮都要全量重扫
// /proc/self/maps + 重解析偏移 —— 实测由此产生了 5 次/秒、共 117 轮的空转。
static std::atomic<bool> g_installPassDone{false};

// ============================================================================
//  ⑤ 双路径挂点机器（本游戏 RVA 不可得 ⇒ 实际全部走按名路径）
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
        if (!ReadU32Guarded(tag, va, &got)) return false;
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

/* ★r20（2026-10-15 底包 vc603）首次触碰延迟 + 节流 —— 治「游戏在启动流里干净退出 exit(0)」
 *
 * 现场（2026-10-17 真机四轮，`/tmp/nhsy2_r20_*.txt` 已留证）：只要我方碰过 il2cpp API，
 * 游戏就在 `Unity: Neon Abyss v…` 之后约 3.7 s **干净退出**（`Zygote: Process … exited
 * cleanly (0)`，全程 0 行 `Fatal signal` / `FATAL EXCEPTION`）；完全不碰（10.14 期 r19 的同款
 * 首启、以及本轮 rt 层解析失败时的对照轮）则正常走到 `TapTapSDK.Init done` 并持续存活。
 * 判据差异在**游戏相位**而不是绝对时间：
 *   · r19（10.14 底包，通过）：我方首次触碰 = Unity `MemoryManager` 后 +4.99 s，
 *     而 Unity 版本行 +5.12 s ⇒ 触碰只提前 0.13 s（恰在 `il2cpp_init` 完成之后）。
 *   · 本轮（10.15 底包，干净退出）：同样 +5.01 s 触碰，但版本行推迟到 +5.71 s
 *     ⇒ 提前 0.70 s，落在 `il2cpp_init` 完成**之前** ⇒ 运行时半初始化时被触碰/装钩。
 * 机制同 `Includes/Il2cppInitGate.h`（FW-20）：init 完成前从外部线程触碰 il2cpp 会让该线程
 * 进 Boehm GC「未知线程」毒态，之后必崩/被游戏带进退出。该头依赖 `il2cpp_init` 可解析，
 * 而本底包 `.dynsym` 里**没有** `il2cpp_init`（实测 23 个 il2cpp_* 名里无它）⇒ 不接线，
 * 改用「首次触碰延迟 + 最多 500 ms 一次的节流」这条不依赖符号的保守门：
 *   ① 从首次被问 API 起先静默 DELAY ms（默认 3000；环境变量 `NHSY2_FIRST_TOUCH_DELAY_MS` 可调）；
 *   ② 之后每 500 ms 才允许真正 `Load()` 一次，避免「未就绪 + 每拍重试」把 il2cpp 打爆
 *      （2026-10-17 实测未节流时 4 秒内重试 190 次）。
 * 延迟到点后若 `Load()` 仍失败，只是晚一点装钩，不会更差。 */
static long long NowMonoMs();   // 前置声明（定义在 §6.5）；本门要用

/* ★r20 诊断分级（`setprop debug.starry.il2cpp_diag <0..7>`，默认 7 = 正常）
 *   用途：定位「游戏在 `Unity: Neon Abyss v…` 之后约 3.8 s 干净退出 exit(0)」的**触发点**。
 *   逐级只增一个动作，一次重编 + 多次覆盖安装冷启即可收敛：
 *     0 = 完全不碰 il2cpp（对照）
 *     3 = 解析符号 + 台账恢复 + 自检 API 调用（不 attach、不扫类、不装钩）
 *     4 = 3 + `thread_attach` + 按图找类（FindClassInImage，只读）
 *     5 = 4 + **只装 libc 日志护盾那一个补丁**（asf.shield）
 *     6 = 5 + 登录保命钩（Login_*）
 *     7 = 6 + 功能钩（SyncHooks 全表）＝ 正常（默认）
 *   2026-10-17 首轮二分结论：0 / 3 / 4 全部**存活**（都走到 TapTapSDK.Init done / Client identity
 *   resolved），而当时 L5（全装）**退出** ⇒ 触发者是**代码补丁**，不是 il2cpp API 读写。
 *   本表继续细分是哪一类补丁。缓存于静态量（同一进程内不变；换级需 force-stop 重开）。 */
static int DiagLevel() {
    static std::atomic<int> cached{-1};
    int v = cached.load();
    if (v >= 0) return v;
    char buf[PROP_VALUE_MAX] = {0};
    const int n = __system_property_get("debug.starry.il2cpp_diag", buf);
    v = (n > 0) ? atoi(buf) : 7;
    if (v < 0) v = 0;
    if (v > 9) v = 9;   // 9 = L9 判决级（同值写）
    cached.store(v);
    return v;
}

static std::atomic<long long> g_firstAskMs{0};
static std::atomic<long long> g_lastLoadMs{0};

/* ★r20：登录保命钩开关（`setprop debug.starry.nhsy2_login_hooks 1` 才开；**默认关**）。
 *   2026-10-17 分级二分实测：这 3 个 Dobby 补丁会在本底包上让游戏启动约 4s 后干净退出，
 *   关掉即恢复正常启动（详见 InstallLoginAssistHooks 里的定案注释）。 */
static bool LoginHooksEnabled() {
    static std::atomic<int> cached{-1};
    int v = cached.load();
    if (v >= 0) return v == 1;
    char buf[PROP_VALUE_MAX] = {0};
    const int n = __system_property_get("debug.starry.nhsy2_login_hooks", buf);
    /* ★r20c 收官定案：**默认 1**（登录预算延长是必需功能；r20 曾因它会触发退出而默认关，
     *   现在退出已被 exit_group 闸就地停住，用户 2026-10-17 L3 实测（含本钩）登录与功能均正常）。 */
    v = (n > 0) ? atoi(buf) : 1;
    cached.store(v);
    return v == 1;
}

static int FirstTouchDelayMs() {
    /* ★r20（2026-10-17 定案）：默认 **0**（不延迟）。
     *   曾经默认 3000 是基于「触碰太早」的假设；分级二分已否证该假设（L3/L4 在同样时刻触碰并
     *   调用 API、甚至 attach+扫类，进程都存活；真凶只是登录链那 3 个补丁）。而延迟会让
     *   「开关下发 → SyncHooks」跑在 API 未就绪时 ⇒ `install pass: ok=0`（功能钩一个都装不上）。
     *   保留该机制仅供日后诊断：环境变量 `NHSY2_FIRST_TOUCH_DELAY_MS=<ms>`。 */
    static std::atomic<int> cached{-1};
    int v = cached.load();
    if (v >= 0) return v;
    v = 0;
    if (const char* e = getenv("NHSY2_FIRST_TOUCH_DELAY_MS")) { int t = atoi(e); if (t >= 0) v = t; }
    cached.store(v);
    return v;
}

static bool EnsureApi() {
    if (GetApi().ready) return true;
    if (DiagLevel() == 0) return false;      // 诊断 L0：完全不碰 il2cpp（对照组）
    const long long now = NowMonoMs();
    long long t0 = g_firstAskMs.load();
    if (t0 == 0) { g_firstAskMs.store(now); t0 = now; }
    if (now - t0 < static_cast<long long>(FirstTouchDelayMs())) {
        static std::atomic<bool> logged{false};
        if (!logged.exchange(true)) {
            LOGI(TPL_LOG " api: 首次触碰延迟 %d ms（等 il2cpp_init 完成，FW-20 同族：本底包实测在版本行前 0.70s 触碰会被游戏带进 exit(0)，r19 只提前 0.13s 才侥幸安全）",
                 FirstTouchDelayMs());
        }
        return false;                       // 门未到：本拍一次 il2cpp 调用都不做
    }
    if (now - g_lastLoadMs.load() < 500) return false;   // 节流：最多 500 ms 一次
    g_lastLoadMs.store(now);
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

// ============================================================================
//  §⑤.1 极小函数（入口 16 字节内出现 RET）专用「4 字节 B 补丁」挂钩
//
//  ★ 为什么 Dobby 不能用（2026-10-08 真机实测真根因）：
//    Dobby 的 arm64 内联补丁**固定改写入口 16 字节**，而 IL2CPP 会把 8 字节 /
//    12 字节的 getter（`ldr/mov/cset` + `ret`）**紧挨着**排布。实测原码：
//      get_CoolingTime  @0x7356f64c4c : BD42A800 D65F03C0 | BD02A800 D65F03C0
//                        = ldr s0,[x0,#0x2a8]; ret | str s0,[x0,#0x2a8]; ret
//                          └ get_CoolingTime(8B) ┘  └ set_CoolingTime(8B) ┘
//      get_AppliesToCurrentPlatform @0x7357627268 : 52800020 D65F03C0 | 2A1F03E0 D65F03C0
//                        = mov w0,#1; ret | mov w0,wzr; ret
//                          └ AppliesToCurrentPlatform(8B) ┘ └ HasContentOverride(8B) ┘
//    ⇒ 挂钩前者时 Dobby 的 16 字节补丁**整个吃掉邻居函数体**，把邻居入口变成
//      补丁的中间指令（二次探针实测读到邻居入口 = `D61F0220` = `br x17`，正是
//      Dobby 补丁的第 2 条指令）⇒ 游戏调用邻居时乱跳，栈上出现
//      `Hooked_Std_HasContentOverride` 自递归 506 层后 SIGSEGV(fault addr 0xb8)。
//
//  做法：只改写入口**第 1 条指令（4 字节）**为 `B <dest>`，邻居完全不受影响：
//    · hookFn 与 target 同在 ±128MB 内 ⇒ dest = hookFn，零额外映射
//    · 否则在 target 附近 mmap 一页写
//        [+0x00] ldr x17, #8   (0x58000051)
//        [+0x04] br  x17       (0xD61F0220)
//        [+0x08] .quad hookFn
//      再 `B <该页>`（该页由 MmapPageNear 保证落在 ±124MB 内）
//    orig 是普通函数指针，位置不受 ±128MB 限制 ⇒ 单独一页放「原样副本 + 回跳」：
//        [+0x00] target 前 16 字节原样副本（调 orig ≡ 调原函数）
//        [+0x10] B target+16
//      **要求前 16 字节内出现 RET**（极小函数恒满足）⇒ 副本必然在走到回跳前
//      就已 ret；16 字节内无 RET 则本路径根本不启用。
//
//  对其它游戏 / 其它槽位零影响：只有「入口 16 字节内有 RET」的槽位走本路径，
//  其余仍走 Dobby，回退语义完全不变。
// ============================================================================
#ifndef MAP_FIXED_NOREPLACE
#define MAP_FIXED_NOREPLACE 0x100000
#endif

struct MapRange { uintptr_t s; uintptr_t e; };

// 指针 → 有符号 64 位（便于算 B 指令的字节距离；指针不能直接 static_cast 到整型）
static inline int64_t PtrI64(const void* p) {
    return static_cast<int64_t>(reinterpret_cast<uintptr_t>(p));
}

// 读 /proc/self/maps 全量快照（按地址升序）。
//
// ★ 必须动态分配、不能定长：实测本机 6334 条映射，定长 4096 会**截断到低地址段**，
//   而目标地址（0x735a2af8d0 这类 il2cpp 方法）在高地址段 ⇒ 截断后「空闲判定」会把
//   已映射的游戏代码页误判成空洞，再 MAP_FIXED 落页就会**覆盖游戏代码**。
static MapRange* MapsSnapshot(int* outN) {
    const int kCap = 32768;
    static MapRange* buf = nullptr;
    static std::mutex mtx;
    *outN = 0;
    std::lock_guard<std::mutex> lk(mtx);
    if (!buf) buf = static_cast<MapRange*>(malloc(sizeof(MapRange) * static_cast<size_t>(kCap)));
    if (!buf) return nullptr;
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) return nullptr;
    char line[512];
    int n = 0;
    while (n < kCap && fgets(line, sizeof(line), f)) {
        unsigned long long s = 0, e = 0;
        if (sscanf(line, "%llx-%llx", &s, &e) != 2) continue;
        buf[n].s = static_cast<uintptr_t>(s);
        buf[n].e = static_cast<uintptr_t>(e);
        ++n;
    }
    fclose(f);
    // /proc/self/maps 本身有序 ⇒ 插入排序在有序输入上是 O(n)，顺带兜住乱序
    for (int i = 1; i < n; ++i) {
        const MapRange k = buf[i];
        int j = i - 1;
        while (j >= 0 && buf[j].s > k.s) { buf[j + 1] = buf[j]; --j; }
        buf[j + 1] = k;
    }
    *outN = n;
    return buf;
}

// 把 4 字节写进代码页（临时放开 RWX，写完立刻收回 RX）
static int SvcMprotect(void* addr, size_t len, int prot);   // ★r20 前置声明（定义见 §⑦.6e）
/* ★r24：绕开 mprotect 的代码页写入通道（`/proc/self/mem` + FOLL_FORCE）。
 *  动机（真机实测 2026-10-25）：vc646 上「任何 mprotect(RWX)+写代码页」都会在 ~10 s 内
 *  触发 themis 的反篡改（exit_group / 崩溃风暴），而 r20 已证「同值写也触发 ⇒ 不是内容校验」。
 *  内核给 /proc/self/mem 的写走 FOLL_FORCE，**不改页表权限、不调用 mprotect** ⇒ 是一个
 *  与 r20 实验过的动作面（libc mprotect / 裸 svc mprotect / soft-dirty 清理）都不重叠的新通道。
 *  prop `debug.starry.nhsy2_patchmem`：1（默认）= 优先走它；0 = 老路径（mprotect）。 */
static bool MemWriteU32ViaProcSelf(const char* tag, void* target, uint32_t value) {
    static std::atomic<int> g_fd{-2};
    int fd = g_fd.load(std::memory_order_relaxed);
    if (fd == -2) {
        fd = open("/proc/self/mem", O_RDWR | O_CLOEXEC);
        g_fd.store(fd, std::memory_order_relaxed);
    }
    if (fd < 0) return false;
    const ssize_t n = pwrite(fd, &value, sizeof(value),
                             static_cast<off_t>(reinterpret_cast<uintptr_t>(target)));
    if (n != static_cast<ssize_t>(sizeof(value))) {
        static std::atomic<int> once{0};
        if (once.fetch_add(1) < 3)
            LOGW(TPL_LOG " %s /proc/self/mem 写入失败 n=%zd errno=%d @%p（退回 mprotect 路径）",
                 tag, n, errno, target);
        return false;
    }
    __builtin___clear_cache(reinterpret_cast<char*>(target),
                            reinterpret_cast<char*>(target) + sizeof(value));
    return true;
}

static int PatchMemMode() {
    static int cached = -1;
    if (cached >= 0) return cached;
    char b[PROP_VALUE_MAX] = {0};
    const int n = __system_property_get("debug.starry.nhsy2_patchmem", b);
    cached = (n > 0) ? atoi(b) : 1;
    return cached;
}

static bool CodePatchU32(const char* tag, void* target, uint32_t value) {
    if (PatchMemMode() == 1 && MemWriteU32ViaProcSelf(tag, target, value)) {
        static std::atomic<int> once{0};
        if (once.fetch_add(1) < 4)
            LOGI(TPL_LOG " %s 代码页写入走 /proc/self/mem（未调用 mprotect）@%p", tag, target);
        return true;
    }
    long ps = sysconf(_SC_PAGESIZE);
    if (ps <= 0) ps = 4096;
    const uintptr_t page = reinterpret_cast<uintptr_t>(target) & ~static_cast<uintptr_t>(ps - 1);
    if (SvcMprotect(reinterpret_cast<void*>(page), static_cast<size_t>(ps),
                    PROT_READ | PROT_WRITE | PROT_EXEC) != 0) {
        LOGW(TPL_LOG " %s mprotect RWX 失败 errno=%d @%p", tag, errno, target);
        return false;
    }
    *reinterpret_cast<volatile uint32_t*>(target) = value;
    __builtin___clear_cache(reinterpret_cast<char*>(target),
                            reinterpret_cast<char*>(target) + 4);
    SvcMprotect(reinterpret_cast<void*>(page), static_cast<size_t>(ps), PROT_READ | PROT_EXEC);
    return true;
}

struct MapCand { uintptr_t a; uintptr_t d; };   // 候选页首地址 + 与 target 的字节距离

// 在 target 附近（|addr-target| ≤ 124MB ⇒ 稳落在 arm64 `B` 的 ±128MB 内）的
// **真实未映射空洞**里落一页（RW；调用方写完自行 mprotect 成 RX）。
//
// 为什么不再「按对齐步长盲扫」：libil2cpp 的 26 个 PT_LOAD 段散布在 8GB 地址空间里，
// 段间空洞普遍只有 4KB~60KB 且**不按 64KB 对齐**。实测 target=0x735a2af8d0 的
// ±120MB 窗口内有 69 条映射、10 个 ≥4KB 空洞，最大 60KB
// （0x73531d1000-0x73531e0000，ΔT=-112.8MB）—— 按 64KB 步进扫时，窗口内唯一的
// 64KB 对齐点 0x73531e0000 恰是空洞的**结束**地址（不含）⇒ 一个都命中不了，
// 于是 40 槽里 3 个极小函数槽位全部报「找不到 ±120MB 内的跳板空洞」。
// 故改为「枚举 maps 缺口 → 按离 target 由近到远依次尝试」。
static void* MmapPageNear(void* target, size_t size) {
    const uintptr_t t = reinterpret_cast<uintptr_t>(target);
    const uintptr_t maxDist = 0x7C00000ULL;      // 124MB（B 是 ±128MB，留 4MB 余量）
    const uintptr_t lo = (t > maxDist) ? (t - maxDist) : 0x10000ULL;
    const uintptr_t hi = t + maxDist;
    const uintptr_t need = (static_cast<uintptr_t>(size) + 0xFFFULL) & ~static_cast<uintptr_t>(0xFFFULL);

    int n = 0;
    const MapRange* m = MapsSnapshot(&n);
    if (!m || n <= 0) {
        LOGW(TPL_LOG " MmapPageNear: /proc/self/maps 读取失败 @%p", target);
        return nullptr;
    }

    static MapCand cands[256];
    int cn = 0;
    uintptr_t cur = lo;
    for (int i = 0; i < n && cn < 256; ++i) {
        if (m[i].e <= cur) continue;
        if (m[i].s > cur) {                      // 空洞 [cur, m[i].s)
            const uintptr_t a = (cur + 0xFFFULL) & ~static_cast<uintptr_t>(0xFFFULL);
            if (a >= 0x10000ULL && a + need <= m[i].s && a + need <= hi) {
                cands[cn].a = a;
                cands[cn].d = (a > t) ? (a - t) : (t - a);
                ++cn;
            }
        }
        if (m[i].e > cur) cur = m[i].e;
        if (cur >= hi) break;
    }
    if (cur < hi && cn < 256) {                  // 尾部空洞 [cur, hi)
        const uintptr_t a = (cur + 0xFFFULL) & ~static_cast<uintptr_t>(0xFFFULL);
        if (a + need <= hi) {
            cands[cn].a = a;
            cands[cn].d = (a > t) ? (a - t) : (t - a);
            ++cn;
        }
    }
    if (cn <= 0) {
        LOGW(TPL_LOG " MmapPageNear: ±124MB 内无 %lu 字节空洞（maps=%d）@%p",
             static_cast<unsigned long>(need), n, target);
        return nullptr;
    }
    for (int i = 1; i < cn; ++i) {               // 按距离升序
        const MapCand k = cands[i];
        int j = i - 1;
        while (j >= 0 && cands[j].d > k.d) { cands[j + 1] = cands[j]; --j; }
        cands[j + 1] = k;
    }

    // 只用 MAP_FIXED_NOREPLACE：候选刚由 maps 快照确认空闲，但快照与本调用之间
    // 别的线程可能已占位 —— NOREPLACE 让内核替我们兜住（EEXIST ⇒ 试下一个候选），
    // 绝不覆盖既有映射。仅当内核对 NOREPLACE 返回 EINVAL（< 4.17）才退回 MAP_FIXED。
    bool sawEINVAL = false;
    for (int i = 0; i < cn; ++i) {
        errno = 0;
        void* p = mmap(reinterpret_cast<void*>(cands[i].a), need, PROT_READ | PROT_WRITE,
                       MAP_PRIVATE | MAP_ANONYMOUS | MAP_FIXED_NOREPLACE, -1, 0);
        if (p == reinterpret_cast<void*>(cands[i].a)) {
            LOGI(TPL_LOG " MmapPageNear: 跳板页 %p ΔT=%.1fMB（%d 个候选中第 %d 近）",
                 p, static_cast<double>(cands[i].d) / 1048576.0, cn, i + 1);
            return p;
        }
        if (p != MAP_FAILED) munmap(p, need);    // 理论上不会发生；保险收回
        else if (errno == EINVAL) sawEINVAL = true;
    }
    if (sawEINVAL) {
        errno = 0;
        void* p = mmap(reinterpret_cast<void*>(cands[0].a), need, PROT_READ | PROT_WRITE,
                       MAP_PRIVATE | MAP_ANONYMOUS | MAP_FIXED, -1, 0);
        if (p == reinterpret_cast<void*>(cands[0].a)) {
            LOGI(TPL_LOG " MmapPageNear: 跳板页 %p ΔT=%.1fMB（MAP_FIXED 回退）",
                 p, static_cast<double>(cands[0].d) / 1048576.0);
            return p;
        }
    }
    LOGW(TPL_LOG " MmapPageNear: %d 个候选全部 mmap 失败 errno=%d @%p", cn, errno, target);
    return nullptr;
}

// ================= ★ r17：跳板页池（一页共享给同窗所有槽位） =================
//  背景（2026-10-12 r16 真机 04:04 装机自检）：`MmapPageNear` 每次为**一个**槽位找**一整页**
//  空洞，而本机 libil2cpp 窗口内的空洞极小（实测整窗只剩 8KB：0x754d3fe000-0x754d400000）。
//  前两个槽位各吃掉一页之后，同窗口后续槽位全部报
//    `MmapPageNear: ±124MB 内无 4096 字节空洞（maps=418X）@0x754xxxxxxx`
//  ⇒ 静默丢掉 4 个槽位：`Res_CrystalCountImpl`（无限水晶主挂点，veneer 4 字节补丁）、
//    `Res_GitterCoin`（金币通道）、`Std_IsFreeBoss`/`Std_IsFreeLevel`（解锁标准版），
//    装机行从 r15 的 `install pass: ok=69 dead=0/72` 退化成 `ok=66 dead=0/72`。
//  对策：一页只 mmap 一次，之后按 16 字节粒度切给**所有** |ΔT| ≤ 124MB 的槽位复用。
//    每个桩只占 16-20 字节 ⇒ 4KB 可服务上百个桩，窗内消耗从「每槽一页」降到「每窗一页」。
//    这修的是**确定性**：同一份代码在不同本机布局下不再随机丢槽（r15 命中、r16 不命中）。
//  权限：池页落地即 RWX（同一页会被多个槽位先后写入，不能装完一个就收成 RX）；
//    只作用在我们自己的匿名页上，不改游戏代码页的权限模型。
struct TrampPool { uintptr_t base; uint32_t size; uint32_t used; };
static TrampPool g_trampPools[16];
static int g_trampPoolCount = 0;
static std::mutex g_trampPoolMtx;

// 在 target 附近切一块可执行暂存（优先复用窗外 124MB 内已有的池页）
static void* TrampAllocNear(void* target, uint32_t size) {
    const uintptr_t t = reinterpret_cast<uintptr_t>(target);
    const uintptr_t maxDist = 0x7C00000ULL;                     // 同 MmapPageNear：124MB
    const uint32_t need = (size + 15u) & ~15u;                  // 16 字节对齐（B/字面量都够用）
    std::lock_guard<std::mutex> lk(g_trampPoolMtx);
    for (int i = 0; i < g_trampPoolCount; ++i) {
        TrampPool& p = g_trampPools[i];
        const uintptr_t d = (p.base > t) ? (p.base - t) : (t - p.base);
        if (d > maxDist || p.size - p.used < need) continue;
        void* r = reinterpret_cast<void*>(p.base + p.used);
        p.used += need;
        return r;
    }
    if (g_trampPoolCount >= static_cast<int>(sizeof(g_trampPools) / sizeof(g_trampPools[0]))) {
        LOGW(TPL_LOG " 跳板池已满（%d 页）@%p", g_trampPoolCount, target);
        return nullptr;
    }
    void* page = MmapPageNear(target, 0x1000);
    if (!page) return nullptr;                                  // 该窗确实没有空洞（无解）
    if (mprotect(page, 0x1000, PROT_READ | PROT_WRITE | PROT_EXEC) != 0) {
        LOGW(TPL_LOG " 跳板池页 %p 置 RWX 失败 errno=%d", page, errno);
        munmap(page, 0x1000);
        return nullptr;
    }
    TrampPool& p = g_trampPools[g_trampPoolCount++];
    p.base = reinterpret_cast<uintptr_t>(page);
    p.size = 0x1000;
    p.used = need;
    const uintptr_t d = (p.base > t) ? (p.base - t) : (t - p.base);
    LOGI(TPL_LOG " 跳板池 #%d %p ΔT=%.1fMB（池内每桩 16 字节，窗内槽位共用这一页）",
         g_trampPoolCount, page, static_cast<double>(d) / 1048576.0);
    return page;
}

// 池页落地时已是 RWX；从池里切出来的地址可能不在页首 ⇒ 先对齐再 mprotect。
// 非池页（HookTiny4 里「附近无空洞」的退路 `mmap(nullptr,0x1000,RW)`）必须升到 RX 才能执行。
static bool TrampMakeExec(void* page) {
    if (!page) return false;
    const uintptr_t pg = reinterpret_cast<uintptr_t>(page) & ~static_cast<uintptr_t>(0xFFFULL);
    if (mprotect(reinterpret_cast<void*>(pg), 0x1000, PROT_READ | PROT_WRITE | PROT_EXEC) != 0) {
        LOGW(TPL_LOG " TrampMakeExec: mprotect RWX 失败 errno=%d @%p", errno, page);
        return false;
    }
    __builtin___clear_cache(reinterpret_cast<char*>(pg), reinterpret_cast<char*>(pg) + 0x1000);
    return true;
}

// 入口 16 字节是否「极小且可安全 4 字节挂钩」
static bool TinyHookable(const uint32_t w[4], int retAt) {
    if (retAt < 0 || retAt > 3) return false;                 // 无 RET ⇒ 走 Dobby
    // ★ 2026-10-11：原样副本只执行到 RET 为止，因此必须对**执行到的每条指令**（w[0..retAt]）
    //   检查 PC 相对形式 —— 原来只查 w[0]，像 `ldr x0,[x0,#0x10]; b.cond …; ret` 这种会把
    //   PC 相对分支原样搬进副本 ⇒ 在副本地址上跳错。只做收紧：判不安全即跳过该槽位。
    for (int i = 0; i <= retAt; ++i) {
        const uint32_t x = w[i];
        if ((x & 0xFC000000u) == 0x14000000u) return false;      // B
        if ((x & 0xFC000000u) == 0x94000000u) return false;      // BL
        if ((x & 0xFFFFFC1Fu) == 0xD61F0000u) return false;      // BR xN（veneer，静态不可解）
        if ((x & 0xFFFFFC1Fu) == 0xD63F0000u) return false;      // BLR xN
        if ((x & 0x9F000000u) == 0x90000000u) return false;      // ADRP（PC 相对，副本失效）
        if ((x & 0x9F000000u) == 0x10000000u) return false;      // ADR
        if ((x & 0xFF000010u) == 0x54000000u) return false;      // B.cond（PC 相对）
        if ((x & 0x7E000000u) == 0x34000000u) return false;      // CBZ / CBNZ
        if ((x & 0x7E000000u) == 0x36000000u) return false;      // TBZ / TBNZ
        if ((x & 0x3B000000u) == 0x18000000u) return false;      // LDR (literal)：PC 相对取常量
    }
    return true;
}

static bool HookTiny4(const char* tag, const char* label, void* target, void* hook, void** orig) {
    // ---- A) orig 跳板：原样副本 + 回跳（orig 是普通函数指针，位置不受 ±128MB 限制）----
    void* pageA = TrampAllocNear(target, 32);   // 5 word（4 条原样副本 + 回跳）⇒ 32 字节足够
    const bool aNear = (pageA != nullptr);
    if (!pageA) {   // 该窗真的没有空洞（极罕见）：退回任意页，副本尾部改用 ret 收尾
        pageA = mmap(nullptr, 0x1000, PROT_READ | PROT_WRITE,
                     MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
    }
    if (pageA == MAP_FAILED) {
        LOGW(TPL_LOG " %s %s orig 跳板页 mmap 失败", tag, label);
        return false;
    }
    uint32_t* a = reinterpret_cast<uint32_t*>(pageA);
    for (int i = 0; i < 4; ++i) {
        if (!ReadU32Guarded(tag, reinterpret_cast<const uint32_t*>(target) + i, &a[i])) {
            return false;                       // ★ 池页不许 munmap（同页还有别的槽位的桩）
        }
    }
    if (aNear) {
        // 副本执行完（retAt ≤ 3 ⇒ 必在 16 字节内 ret）后本应回到 target+16
        const int64_t d = (PtrI64(reinterpret_cast<char*>(target) + 16) -
                           PtrI64(reinterpret_cast<char*>(pageA) + 16)) / 4;
        a[4] = 0x14000000u | (static_cast<uint32_t>(d) & 0x03FFFFFFu);
    } else {
        a[4] = 0xD65F03C0u;   // ret（安全终止；正常路径永不可达，因为副本里含 RET）
    }
    if (!TrampMakeExec(pageA)) {                 // 池页已 RWX；退路页 RW→RWX
        return false;
    }

    // ---- B) 入口 4 字节 B 补丁 ----
    void* dest = hook;
    const int64_t dHook = (PtrI64(hook) - PtrI64(target)) / 4;
    if (dHook > 0x1FFFFFFLL || dHook < -0x2000000LL) {
        void* pageN = TrampAllocNear(target, 32);   // ldr x17,#8 + br x17 + 8B 字面量 = 16B
        if (!pageN) {
            LOGW(TPL_LOG " %s %s 找不到 ±124MB 内的跳板空洞 — 放弃该槽位", tag, label);
            return false;
        }
        uint32_t* n = reinterpret_cast<uint32_t*>(pageN);
        n[0] = 0x58000051u;   // ldr x17, #8
        n[1] = 0xD61F0220u;   // br  x17
        *reinterpret_cast<uint64_t*>(n + 2) = reinterpret_cast<uint64_t>(hook);
        if (!TrampMakeExec(pageN)) {
            return false;
        }
        dest = pageN;
        LOGI(TPL_LOG " %s %s hook 距目标 %.1fMB > 128MB ⇒ 邻近跳板 %p（原样副本 %p）",
             tag, label, static_cast<double>(dHook) * 4.0 / 1048576.0, pageN, pageA);
    }
    const int64_t dB = (PtrI64(dest) - PtrI64(target)) / 4;
    if (dB > 0x1FFFFFFLL || dB < -0x2000000LL) {
        LOGW(TPL_LOG " %s %s B 补丁超出 ±128MB — 放弃该槽位", tag, label);
        return false;
    }
    if (!CodePatchU32(tag, target,
                      0x14000000u | (static_cast<uint32_t>(dB) & 0x03FFFFFFu))) {
        return false;
    }
    if (orig) *orig = pageA;
    return true;
}

// ================= ★ r12b：只改入口 4 字节的挂钩（不碰 16 字节窗口） =================
//  背景（2026-10-12 真机，用户 m05208「进游戏直接闪退了」）：`SpriteManager` 那条
//  `mov w1,#imm ; b <共享实现>` 的 veneer 目标，其 16 字节窗口内**另有**一个方法入口
//  （真机取证：`GetCustomSpriteTypeCount`）⇒ 只要用 Dobby（固定改写入口 16 字节）就会把
//  邻居的方法入口改成野指令，一进关对方代码首次运行即 SIGSEGV（r7 事故同机理）。
//  做法：**完全不碰目标函数**，只把 veneer 入口的第一条指令（MOVZ/MOVN）换成 `b <hook>`；
//  4 字节补丁只覆盖 veneer 自己的 +0..+3（veneer 只有 8 字节，+8 起的邻居入口不动）。
//  钩子内部直接调用**真实现**（不需要任何 orig 跳板，因为我们不修改目标函数）。
static bool PatchEntryBranch4(const char* tag, const char* label, void* entry, void* hook) {
    if (!DiagAllowPatch(tag)) {   // ★r20 诊断：4 字节入口补丁同受补丁闸约束
        LOGW(TPL_LOG " diag L%d：跳过入口补丁 %s %s", DiagLevel(), tag, label);
        return false;
    }
    if (!entry || !hook) return false;
    if (!Il2Cpp::IsExecutableRange(reinterpret_cast<uintptr_t>(entry), 4)) {
        LOGW(TPL_LOG " %s %s 入口 %p 不可执行 — 放弃该挂点", tag, label, entry);
        return false;
    }
    void* dest = hook;
    void* pageN = nullptr;
    const int64_t dHook = (PtrI64(hook) - PtrI64(entry)) / 4;
    if (dHook > 0x1FFFFFFLL || dHook < -0x2000000LL) {
        pageN = TrampAllocNear(entry, 32);   // ldr x17,#8 + br x17 + 8B 字面量 = 16B
        if (!pageN) {
            LOGW(TPL_LOG " %s %s 找不到 ±124MB 内的跳板空洞 — 放弃该挂点", tag, label);
            return false;
        }
        uint32_t* n = reinterpret_cast<uint32_t*>(pageN);
        n[0] = 0x58000051u;   // ldr x17, #8
        n[1] = 0xD61F0220u;   // br  x17
        *reinterpret_cast<uint64_t*>(n + 2) = reinterpret_cast<uint64_t>(hook);
        if (!TrampMakeExec(pageN)) {
            return false;
        }
        dest = pageN;
        LOGI(TPL_LOG " %s %s hook 距入口 %.1fMB > 128MB ⇒ 邻近跳板 %p",
             tag, label, static_cast<double>(dHook) * 4.0 / 1048576.0, pageN);
    }
    const int64_t dB = (PtrI64(dest) - PtrI64(entry)) / 4;
    if (dB > 0x1FFFFFFLL || dB < -0x2000000LL) {
        LOGW(TPL_LOG " %s %s 4 字节 B 补丁超出 ±128MB — 放弃该挂点", tag, label);
        return false;
    }
    if (!CodePatchU32(tag, entry, 0x14000000u | (static_cast<uint32_t>(dB) & 0x03FFFFFFu))) {
        return false;
    }
    return true;
}

// ================= ★ r12b：16 字节窗口内的「外来方法入口」检测 =================
//  Dobby 的入口补丁固定改写 16 字节 ⇒ 只要窗口 [target,target+16) 里存在**别的方法入口**，
//  那个方法一被调用就跳到野指令（r7/r12 两次真机 SIGSEGV 的同一根因）。
//  判据来源：r12 真机日志 `该实现 16 字节窗口内另有 1 个方法入口：GetCustomSpriteTypeCount`。
//  返回窗口内外来入口个数（≤4 条名字写进 namesOut，可为空）。
static int ForeignEntryInWindow(void* klass, void* target, const char** namesOut, int namesCap) {
    if (!klass || !target || !GetApi().class_methods) return 0;
    void* iter = nullptr;
    int guard = 0, n = 0;
    char* end = reinterpret_cast<char*>(target) + 16;
    while (true) {
        void* mi = GetApi().class_methods(klass, &iter);
        if (!mi) break;
        if (++guard > 4096) break;
        void* f = *reinterpret_cast<void**>(mi);
        if (!OkPtr(f) || f == target) continue;
        if (f >= target && f < end) {
            if (namesOut && n < namesCap) {
                namesOut[n] = GetApi().method_get_name ? GetApi().method_get_name(mi) : nullptr;
            }
            ++n;
        }
    }
    return n;
}

// ★ 2026-10-11：已成功挂钩的入口表。同一入口被两个槽位命中时，第二次必须跳过——
//   再挂一次会把我们自己的补丁字节当成「原序言」重定位，等于自毁；转发器解析后
//   也要用同一张表判冗余（例：Attri.get_Item 转发到 Attri.Get，后者已被前面的槽位挂钩）。
static void* g_hookEntries[128];
static int   g_hookEntryN = 0;

// ★ 2026-10-11：因「入口形态不安全」被主动跳过的槽位数（与「方法找不到」的 dead 区分开，
//   装机汇总会单独打一行；跳过的原因逐条打在 WARN/INFO 里）
static std::atomic<int> g_installSkip{0};

static bool HookEntryAlready(void* p) {
    for (int i = 0; i < g_hookEntryN; ++i) if (g_hookEntries[i] == p) return true;
    return false;
}
static void HookEntryNote(void* p) {
    if (g_hookEntryN < 128) g_hookEntries[g_hookEntryN++] = p;
}

// 解开「入口即无条件 B」的转发器链，返回真实实现；>4 跳或越界返回 nullptr
static void* ResolveBranchForwarders(const char* tag, void* fn, int* hopsOut) {
    void* cur = fn;
    int   hops = 0;
    while (hops < 4) {
        uint32_t w0 = 0;
        if (!ReadU32Guarded(tag, reinterpret_cast<const uint32_t*>(cur), &w0)) break;
        if ((w0 & 0xFC000000u) != 0x14000000u) break;            // 不是无条件 B ⇒ 已达真实实现
        int64_t imm = static_cast<int64_t>(w0 & 0x03FFFFFFu);
        if (imm & 0x02000000LL) imm -= 0x04000000LL;             // B: 26 位立即数符号扩展
        void* dest = reinterpret_cast<char*>(cur) + (imm << 2);
        if (!OkPtr(dest)) { cur = nullptr; break; }
        cur = dest;
        ++hops;
    }
    if (hopsOut) *hopsOut = hops;
    return cur;
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

    // ---- 探针（★ 2026-10-08）：记录 methodPointer 与函数体前 16 字节 ----
    //  用途：① 判「两个不同方法是否共用同一个 methodPointer」；
    //        ② 判「函数体是否短到 Dobby 无法安全重定位」（16 字节内出现 RET / 分支）
    //           —— 见 §⑤.1：Dobby 固定改写入口 16 字节，会吃掉紧邻的邻居方法。
    uint32_t w[4] = {0, 0, 0, 0};
    {
        const uint32_t* code = reinterpret_cast<const uint32_t*>(fn);
        for (int i = 0; i < 4; ++i) ReadU32Guarded(tag, code + i, &w[i]);
    }
    int retAt = -1;
    for (int i = 0; i < 4; ++i) {
        if ((w[i] & 0xFFFFFC1Fu) == 0xD65F0000u) { retAt = i; break; }  // RET
    }
    bool tiny = TinyHookable(w, retAt);
    LOGI(TPL_LOG " %s probe %s.%s ptr=%p code=%08X %08X %08X %08X%s",
         tag, cls, method, fn, w[0], w[1], w[2], w[3],
         tiny ? "  <-TINY/RET -> 4B 补丁" : ((retAt >= 0) ? "  <-RET" : ""));

    // ---- ★ 真机根因闸门 A（2026-10-11）：入口是「转发器」。----
    //   IL2CPP 会把只有一条 `b <真实实现>` 的 4 字节方法紧挨着排布，且真实实现常就在同一
    //   16 字节窗口内。本游戏实例：Attri.get_Item @0x3cf9d78 = `b Attri.Get`，+0x4 =
    //   `mov w2,#1; b <共享实现>`（8 字节方法），+0x8 = `b <共享实现>`（4 字节方法），
    //   +0xc = 共享实现本体。Dobby 的 16 字节补丁会把这 4 个入口一起改写：
    //   +0x4 变成补丁中段的 `br x17`（野跳 ⇒ 落进别的 hook 自递归 ⇒ SIGSEGV fault addr 0xb8，
    //   与 2026-10-08 记录的事故 fault 偏移一致），+0x8/+0xc 变成钩子地址的高低半字
    //   （未定义指令 ⇒ SIGILL）。做法：顺链解析到真实实现再挂钩；已被前面槽位挂钩则判冗余跳过。
    {
        int hops = 0;
        void* real = ResolveBranchForwarders(tag, fn, &hops);
        if (!real) {
            LOGW(TPL_LOG " %s %s.%s 入口转发链 %d 跳后越界 — 跳过该槽位",
                 tag, cls, method, hops);
            g_installSkip.fetch_add(1);
            return false;
        }
        if (real != fn) {
            if (HookEntryAlready(real)) {
                LOGI(TPL_LOG " %s %s.%s 入口为转发器（%d 跳）→ 真实实现 %p 已被前面槽位挂钩"
                             " — 跳过冗余槽位（功能已覆盖；用 16 字节补丁会吃掉邻居入口）",
                     tag, cls, method, hops, real);
                g_installSkip.fetch_add(1);
                return false;
            }
            LOGI(TPL_LOG " %s %s.%s 入口为转发器（%d 跳，原入口 %p）→ 改挂真实实现 %p"
                         "（避免 Dobby 16 字节补丁改写紧随其后的邻居方法入口）",
                 tag, cls, method, hops, fn, real);
            fn = real;
            for (int i = 0; i < 4; ++i) {
                ReadU32Guarded(tag, reinterpret_cast<const uint32_t*>(fn) + i, &w[i]);
            }
            retAt = -1;
            for (int i = 0; i < 4; ++i) {
                if ((w[i] & 0xFFFFFC1Fu) == 0xD65F0000u) { retAt = i; break; }
            }
            tiny = TinyHookable(w, retAt);
        }
        if (HookEntryAlready(fn)) {
            LOGW(TPL_LOG " %s %s.%s 入口 %p 已被前面的槽位挂钩 — 跳过重复槽位"
                         "（避免二次补丁把补丁字节当原序言重定位）", tag, cls, method, fn);
            g_installSkip.fetch_add(1);
            return false;
        }
    }

    // ---- ① 入口是 `br xN` veneer：静态不可解开，Dobby 同样无法安全挂钩 ⇒ 跳过 ----
    if ((w[0] & 0xFFFFFC1Fu) == 0xD61F0000u) {
        LOGW(TPL_LOG " %s %s.%s 入口为 `br xN` veneer（需寄存器上下文，不可静态解开）"
                     " — 跳过该槽位", tag, cls, method);
        g_installSkip.fetch_add(1);
        return false;
    }

    // ---- ② 极小函数（16 字节内出现 RET）：只补 4 字节，绝不碰邻居 ----
    if (tiny) {
        if (!HookTiny4(tag, method, fn, hook, orig)) return false;
        HookEntryNote(fn);
        LOGI(TPL_LOG " %s hooked tiny4 %s.%s(argc=%d) orig=%p",
             tag, cls, method, argc, orig ? *orig : nullptr);
        return true;
    }

    // ---- ★ 真机根因闸门 B（2026-10-11）：短函数（16 字节窗口的 +0/+4/+8 即终止指令）----
    //   函数体不足 16 字节 ⇒ Dobby 的 16 字节补丁必然改写紧随其后的邻居方法入口；
    //   而既有 4 字节路径只对「以 RET 结束」的极小函数安全（原样副本靠 RET 提前返回，
    //   且 PC 相对指令已被 TinyHookable 拒绝），对以 B/BR/BLR 结束的短函数无法重定位。
    //   ⇒ 一律跳过该槽位（fail-soft：只丢这一个槽，其它槽位照常装）。
    {
        int termAt = -1; uint32_t termW = 0;
        for (int i = 0; i <= 2; ++i) {          // +0xc 收尾 ⇒ 恰好 16 字节，Dobby 安全
            const uint32_t x = w[i];
            if ((x & 0xFFFFFC1Fu) == 0xD65F0000u || (x & 0xFC000000u) == 0x14000000u ||
                (x & 0xFFFFFC1Fu) == 0xD61F0000u || (x & 0xFFFFFC1Fu) == 0xD63F0000u) {
                termAt = i; termW = x; break;
            }
        }
        if (termAt >= 0) {
            const char* kind = ((termW & 0xFC000000u) == 0x14000000u) ? "b"
                             : ((termW & 0xFFFFFC1Fu) == 0xD61F0000u) ? "br"
                             : ((termW & 0xFFFFFC1Fu) == 0xD63F0000u) ? "blr" : "ret";
            LOGW(TPL_LOG " %s %s.%s 入口 +%d 处即 `%s`（函数体不足 16 字节）— 跳过该槽位："
                         "Dobby 16 字节补丁会改写紧随其后的邻居方法入口"
                         "（历史事故 SIGSEGV fault addr 0xb8）",
                 tag, cls, method, termAt * 4, kind);
            g_installSkip.fetch_add(1);
            return false;
        }
    }

    // ---- ③ 常规函数（入口 16 字节内无 RET/分支）：Dobby 的 16 字节补丁完全落在函数体内 ----
    //  ★ r12b（2026-10-12）：但「函数体够长」不代表窗口干净——真机取证证明 IL2CPP 会把**另一个
    //  方法的入口**放在某方法体的前 16 字节内（`SpriteManager` 那条 veneer 目标窗口里就有
    //  `GetCustomSpriteTypeCount`）。那种情况下 Dobby 一样会吃掉邻居入口 ⇒ SIGSEGV（r7/r12 同源）。
    //  所以这里再加一道：窗口内出现外来方法入口就禁用 Dobby，跳过该槽位（fail-soft，只丢这一个槽）。
    {
        const int foreign = ForeignEntryInWindow(k, fn, nullptr, 0);
        if (foreign > 0) {
            LOGW(TPL_LOG " %s %s.%s 入口 16 字节窗口内有 %d 个**外来方法入口** — 跳过该槽位："
                         "Dobby 16 字节补丁会改写邻居方法入口（r7/r12 事故同源，SIGSEGV）",
                 tag, cls, method, foreign);
            g_installSkip.fetch_add(1);
            return false;
        }
    }
    if (!DobbyHookGuarded(tag, fn, hook, orig)) return false;
    HookEntryNote(fn);
    //  ★ r16：补印**目标入口**的库名+偏移（`trampoline` 是 Dobby 的跳板，不是目标入口，
    //  离线对崩溃帧时用不上）。有了这一行，任何 tombstone / 护栏日志里的 `libil2cpp+0x…`
    //  都能立刻对上「是哪个被挂钩的方法在哪一跳崩的」。
    char offbuf[128];
    ModOff(reinterpret_cast<uintptr_t>(fn), offbuf, sizeof(offbuf));
    LOGI(TPL_LOG " %s hooked by-name %s.%s(argc=%d) trampoline=%p target=%s",
         tag, cls, method, argc, orig ? *orig : nullptr, offbuf);
    return true;
}

struct HookSlot {
    const char* tag;
    uintptr_t   rva;          // 0 = 只走按名（本游戏恒为 0）
    uint32_t    expectInsn;   // 0 = 不校验序言
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
//  ⑥ hook 函数与挂点表
//     铁律：一个具体方法一个槽；绝不 hook 共享 invoke 入口。
// ============================================================================

// ---- 6.1 无敌：NEON.Game.Hearts.DeductHP(DamageInfo, Int32) -> Boolean ----
//  行为型（改判定）⇒ Dobby hook。
//  实现：置 isInvincible / UnKillable 为真（让游戏自身无敌逻辑也生效，防后续
//        代码路径绕过 DeductHP），再直接返回 false（本帧不扣血）。
//  为什么不是恒真谓词：DeductHP 是「执行扣血的行为方法」，返回 false = 不扣血，
//  与 §28 禁止的「输入门禁恒真」（会表现为自动连发）不是一回事。
static bool Hooked_Hearts_DeductHP(void* thiz, void* damage, int32_t count, void* method) {
    if (g_inv.load()) {
        if (OkPtr(thiz)) {
            SafeWriteU8(thiz, g_offHeartsInv.load(), 1);
            SafeWriteU8(thiz, g_offHeartsUnkill.load(), 1);
        }
        g_hitInv.fetch_add(1, std::memory_order_relaxed);
        return false;   // 不扣血
    }
    if (!g_origHeartsDeduct) return false;
    //  ★ r16：原函数自己崩 ⇒ 就地拦下（不闪退），落 PC/LR 定位行；本次按「未扣血」返回。
    bool ok = false, r = false;
    FS_GUARDED("orig.Hearts.DeductHP", ok,
               { r = g_origHeartsDeduct(thiz, damage, count, method); });
    return ok ? r : false;
}

// ---- 6.2 伤害倍率：NEON.Game.HitPoints.DeductHP(DamageInfo, Int32) -> Boolean ----
//  行为型 ⇒ Dobby hook。
//  只放大 DamageInfo.damage（基值），**不动 damageMult**（避免与
//  GetFinalDamage 里的乘法叠成平方）。
//  §28 安全阀：只在「伤害来自本地玩家」时放大（DamageInfo.IsFromLocalPLayer），
//  否则（本地玩家挨打）原样放行。
struct LocalPlayerProbe { void* dmg; bool result; };

static void ProbeLocalPlayerThunk(void* arg) {
    LocalPlayerProbe* p = reinterpret_cast<LocalPlayerProbe*>(arg);
    typedef bool (*Fn)(void* thiz, void* method);
    p->result = ((Fn)g_fnIsFromLocalPlayer)(p->dmg, nullptr);
}

static bool DamageFromLocalPlayer(void* dmg) {
    if (!dmg || !g_fnIsFromLocalPlayer) return true;   // 判不了 ⇒ 放行（HitPoints 目标是敌人）
    LocalPlayerProbe probe{dmg, true};
    if (!RunGuarded("dmg.IsFromLocalPLayer", ProbeLocalPlayerThunk, &probe)) return true;
    return probe.result;
}

//  ★ 与对照方 nhsy1 NeonMod.cpp:591-608 的差异（见 10_docs/霓虹深渊2/路线锁定.md）：
//  nhsy1 在 HitPoints::DeductHP 里乘 DamageInfo 的 3 个浮点字段（+0x08/+0x10/+0x18），
//  并留下硬教训「DoDeductHP 实际读的是 +0x18(FINAL)，只在这一处缩放，否则平方」。
//  nhsy2 的 DamageInfo 字段布局不同（damage/damageMult 之后还有 20 个字段），
//  且 HitPoints 多了 CalculationDamage(DamageInfo,int)->Single 这个**计算入口**。
//  ⇒ 改乘 CalculationDamage 的返回值：不猜字段、不依赖布局、结构上不可能平方。
static bool Hooked_HitPoints_DeductHP(void* thiz, void* damage, int32_t count, void* method) {
    // 只做探针：确认 DeductHP 这条入口真的被调用（§29 默认动作）
    const uint32_t n = g_hitDmg.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 20 && OkPtr(damage)) {
        LOGI(TPL_LOG " dmg probe #%u DeductHP base=%.3f mult=%.3f (off=0x%zX/0x%zX)",
             n, SafeReadF32(damage, g_offDmgDamage.load(), 0.f),
             SafeReadF32(damage, g_offDmgMult.load(), 0.f),
             g_offDmgDamage.load(), g_offDmgMult.load());
    }
    if (!g_origHitPointsDeduct) return false;
    //  ★ r16：原函数自己崩 ⇒ 就地拦下（不闪退），落 PC/LR 定位行；本次按「未扣血」返回。
    bool ok = false, r = false;
    FS_GUARDED("orig.HitPoints.DeductHP", ok,
               { r = g_origHitPointsDeduct(thiz, damage, count, method); });
    return ok ? r : false;
}

// 伤害倍率的唯一改写点：HitPoints::ReallyDeductHP(DamageInfo, Single) -> Void
//  ★ 挂点迁移（真机实测「伤害倍率无效」）：
//    旧挂点 = CalculationDamage 的返回值。实测该入口全程 0 命中 —— 敌人受击
//    走的是 Mirror 网络同步链，根本不经过它：
//      DeductHP(damage,count) → CmdDeductHP(damage,netId,lastDamage) [客户端→主机]
//      → RpcDeductHP → AddDeductQueue(damage,lastDamage) → GetFinalDamage(...)
//      → ReallyDeductHP(damage, finalDamage:System.Single)
//    ReallyDeductHP 是伤害链**最后一跳**（slot=34 虚方法，token 0x06008D25）：
//    只乘 finalDamage 实参 ⇒ 不读任何字段偏移、不可能与下游重复相乘（不存在平方）。
//    门禁：DamageInfo.IsFromLocalPLayer()（本地玩家造成的伤害才放大；本机受伤原样放行）。
// ================= ★ r14 全屏攻击：挂点已迁到 Killable 族（见下方 FullScreenArm 段）=================
//  r11-r13 的「伤害广播」已删除：真机实测「扫同类实例再转发」1 ms 内 SIGSEGV。
static long long NowMonoMs();   // 定义在 §6.5（登录保命）段，此处前置声明

static void Hooked_ReallyDeductHP(void* thiz, void* damage, float finalDamage, void* method) {
    float out = finalDamage;
    const int mul = g_dmgMul.load(std::memory_order_relaxed);
    const bool local = OkPtr(damage) && DamageFromLocalPlayer(damage);
    if (mul > 1 && local) {   // 滑块值即开关（§③.1）
        out = finalDamage * static_cast<float>(mul);
        const uint32_t n = g_hitDmg.fetch_add(1, std::memory_order_relaxed) + 1;
        if (n <= 20) LOGI(TPL_LOG " dmg probe #%u ReallyDeductHP %.3f -> %.3f (x%d)",
                          n, finalDamage, out, mul);
    }
    // ★ r14：全屏攻击的广播点已从本层**移除**（真机 20261012_025029 实证：
    //   本层扫内存转发同类实例后 1 ms SIGSEGV —— 扫描分不清活体与已释放的尸体）。
    //   新机制挂在 `Killable.TakeDamage`（记录）＋ `Killable.Update`（逐个施放）。
    if (!g_origReallyDeduct) return;
    //  ★ r16：伤害倍率这一跳也是「我们调原函数」，同一层事故（0xb8）就在这条链上 ⇒ 纳入护栏。
    //  拦下时本次伤害按「未生效」处理（原函数没跑完，不能再补一次，否则可能扣两次血）。
    bool ok = false;
    FS_GUARDED("orig.HitPoints.ReallyDeductHP", ok,
               { g_origReallyDeduct(thiz, damage, out, method); });
}

// ================= ★ r14 全屏攻击（用户 m05869 指定思路）=================
//  用户原话：「先获取到在场的所有怪物，然后攻击一个怪物的时候，所有怪物收到相同的伤害
//  或者动作，能明白吗？这个思路能做吗？」⇒ 按这个思路重做，不再裸扫内存。
//  为什么换掉 r11-r13：「按同类 HP 类扫内存再转发」在真机上 1 ms 内 SIGSEGV
//  （记录仪 09_test/_recorder/20261012_025029：`fullscreen bcast scan #1 found=41`
//   紧跟 `[THEMIS]: get signal 11`，崩前连 `targets=` 那行都没打出来）——扫描分不清
//  「活着的怪」与「已释放/被复用的尸体」，转发时一调原函数就踩该死内存。
//  新机制（只用活体的必经入口，绝不枚举内存）：
//    ① 记录：`NEON.Game.Killable.TakeDamage(DamageInfo, Int32)`（斌哥原版 4 个描述符之一，
//       实测在玩家打怪链上）拿到「主动目标 + 那一份 DamageInfo + 那一个 Int32 实参」；
//    ② 施放：`NEON.Game.Killable.Update()`（MonoBehaviour，每只活体每帧必经）在 200 ms
//       窗口内对**同阵营**的其它活体原样调用 TakeDamage ⇒ 只有活着的对象会被施放。
//  1:1 保真：判据只有「活体自己调 Update」「faction 相同」「类名不含 player/pet/follower/
//  npc/hearts」「本窗口未施放过」「上限 256 只」；不写任何字段、不猜伤害公式
//  （转发的是游戏自己那一份参数 ⇒ 与玩家打那一下完全等价）。
static const uint32_t kFsMaxTargets = 256;
static const uint64_t kFsWindowMs   = 200;
//  ★★★ r15（修 r14「打怪就闪退」）：DamageInfo 的**常驻深拷贝槽**
//   两份真机 tombstone（r13 pid 8103 tid 8183 / r14 pid 13302 tid 13384）逐帧同址：
//   `#00 libc __memcpy_aarch64_simd+232` ← … ← 最外层都有我方 `libstarry_magic.so` 帧
//   （r13 `+0x262a0`、r14 `+0x26c50`），且都是
//   `signal 11 (SIGSEGV) code 1 (SEGV_MAPERR) fault addr 0xb8`、`x1=0xb8`（memcpy 源）、
//   `x2=0x110`（长度 272 字节）⇒ 游戏侧正从「空基址 + 0xb8」处拷 272 字节结构体。
//   为什么是我们那一下：`Killable.TakeDamage(DamageInfo, Int32)` /
//   `HitPoints.ReallyDeductHP(DamageInfo, Single)` 的 DamageInfo 实参是**调用方栈上的
//   临时副本**（值传递结构），我们 r11-r14 一直把那个指针**留到调用返回之后**再用
//   ⇒ 栈帧一撤，指针指向的就是被复用/被清零的栈内存（r14 实测 hit #1 成功、hit #2 崩）。
//   ⇒ 记录时**当场**深拷贝进本 so 的常驻缓冲（地址永不失效、不被栈复用），
//     每次施放再从模板复制一个**独立槽位**（7 个轮转）发给目标，绝不共用一块。
static const size_t   kFsDmgCopyBytes = 272;      // = 游戏自己那一次 memcpy 的 0x110
static const uint32_t kFsDmgSlots     = 8;        // 槽 0 = 模板，槽 1..7 轮转发给各目标
alignas(16) static uint8_t g_fsDmgRing[kFsDmgSlots][kFsDmgCopyBytes + 16];
static std::atomic<uint32_t> g_fsDmgSlot{1};
static bool    g_fsDmgCopied   = false;           // 深拷贝成功（失败则本窗口不施放）
static std::atomic<uint64_t> g_fsUntilMs{0};      // >0 且未过期 ⇒ 令牌生效（热路径只读这一个）
static void*   g_fsDamage      = nullptr;         // ★ r15：恒指向 g_fsDmgRing[0]（常驻模板）
//  ★ r16：第 3 个实参改成 64 位透传。反汇编 `Killable.TakeDamage`(RVA 0x3f24730) 的序言是
//  `0x3f24760 mov x21, x2` —— 游戏自己按 **64 位**保存它（我们原来写 int32_t，编译成
//  `mov w2, w24`，高 32 位被清零）。虽然扫过的函数体里 x21 在 0x3f24b28 被覆盖前没被用过，
//  但「原样透传」在两种可能签名（Int32 / 指针）下都安全，且零成本 ⇒ 按 64 位走。
static intptr_t g_fsArg2       = 0;               // TakeDamage 的第 2 实参（64 位透传）
static void*   g_fsMethod      = nullptr;
static void*   g_fsSelf        = nullptr;         // 主动目标（本窗口不再重复打）
static int32_t g_fsFaction     = 0;
static bool    g_fsHasFaction  = false;
static uint32_t g_fsSeenCount  = 0;
static void*   g_fsSeen[kFsMaxTargets];
static std::atomic<uint32_t> g_hitFsArm{0};
static std::atomic<uint32_t> g_hitFsHit{0};
static std::atomic<size_t>   g_offKillableFaction{0};
typedef bool (*KillableTakeDamage_t)(void* thiz, void* damage, intptr_t arg2, void* method);
typedef void (*KillableUpdate_t)(void* thiz, void* method);
static KillableTakeDamage_t g_origKillableTakeDamage = nullptr;
static KillableUpdate_t     g_origKillableUpdate     = nullptr;

// 类名闸：只挡「玩家自己 / 宠物 / 随从 / NPC / 玩家 HP 类」，其余（含各类怪物）一律放行。
static inline bool FsNameAllowed(void* obj) {
    const char* cn = GetApi().ClassName(obj);
    if (!cn) return true;
    char low[96]; size_t j = 0;
    for (const char* p = cn; *p && j + 1 < sizeof(low); ++p) {
        char c = *p; if (c >= 'A' && c <= 'Z') c = (char)(c - 'A' + 'a');
        low[j++] = c;
    }
    low[j] = 0;
    return !(strstr(low, "player") || strstr(low, "pet") || strstr(low, "follower") ||
             strstr(low, "npc") || strstr(low, "hearts"));
}

// ★ r15：把调用方那份 DamageInfo 拷进常驻槽。必须在**调用原函数之前**做：
//   调用一返回，那个指针就可能已经指向被复用/被覆盖的栈内存。
static bool FsCopyDamage(void* src, uint32_t slot) {
    if (!OkPtr(src) || slot >= kFsDmgSlots) return false;
    if (!Il2Cpp::IsReadableRange(reinterpret_cast<uintptr_t>(src), kFsDmgCopyBytes)) return false;
    memcpy(g_fsDmgRing[slot], src, kFsDmgCopyBytes);
    return true;
}

// 记下这一下（damage 必须是**已深拷贝**的常驻地址）
static void FullScreenArm(void* src, void* damage, intptr_t arg2, void* method, bool copied) {
    g_fsDamage = damage; g_fsArg2 = arg2; g_fsMethod = method; g_fsSelf = src;
    g_fsDmgCopied = copied;
    g_fsFaction = 0; g_fsHasFaction = false;
    const size_t off = g_offKillableFaction.load(std::memory_order_relaxed);
    if (off && OkPtr(src)) {
        int32_t v = 0; memcpy(&v, reinterpret_cast<const char*>(src) + off, sizeof(v));
        g_fsFaction = v; g_fsHasFaction = true;
    }
    g_fsDmgSlot.store(1, std::memory_order_relaxed);
    g_fsSeenCount = 0;                                  // 先写齐负载，最后开门（顺序保证）
    g_fsUntilMs.store((uint64_t)NowMonoMs() + kFsWindowMs, std::memory_order_release);
}

// Killable.TakeDamage(DamageInfo, Int32) -> Boolean（argc=2）：只**记录**这一下
static bool Hooked_KillableTakeDamage(void* thiz, void* damage, intptr_t arg2, void* method) {
    // ★ r15：先深拷贝（此刻指针一定有效），再调原函数；此后一律只用这份拷贝。
    const bool wantArm = g_full.load(std::memory_order_relaxed) && OkPtr(thiz) && OkPtr(damage);
    bool copied = false;
    if (wantArm) copied = FsCopyDamage(damage, 0);
    void* stable = copied ? (void*)g_fsDmgRing[0] : damage;
    // ★ r16：这一跳是**我们调原函数**，r11–r15 的 `fault addr 0xb8` 崩溃就发生在它内部
    //   （tombstone 帧 #13 就是我们这条 `blr x8` 的返回址）。纳入护栏 ⇒ 原函数自己崩就地
    //   拦下、落 PC/LR/SP 定位行、游戏继续；本次伤害按「未生效」返回 false。
    bool ok = false, r = false;
    FS_GUARDED("orig.Killable.TakeDamage", ok, {
        r = g_origKillableTakeDamage ? g_origKillableTakeDamage(thiz, damage, arg2, method) : false;
    });
    if (!ok) return false;
    // ★ r16：触发闸换判据。r15 的闸是「必须 DamageFromLocalPlayer」——记录仪全程（pid 19880 /
    //   22685 两次崩溃会话）**一行 `fullscreen token arm` 都没有**，说明这道闸恒假，全屏攻击
    //   等于根本没接上（且该分支连日志都不打，所以此前无法察觉）。
    //   新闸改为**受害者侧判据**：被记的对象不是玩家/宠物/随从/NPC/玩家HP 类就记这一下；
    //   `DamageFromLocalPlayer` 降级成日志字段，不再单独决定成败。
    const bool victimOk = FsNameAllowed(thiz);
    const bool localHit = copied ? DamageFromLocalPlayer(stable) : false;
    if (wantArm && copied && victimOk) {
        FullScreenArm(thiz, stable, arg2, method, true);
        const uint32_t n = g_hitFsArm.fetch_add(1, std::memory_order_relaxed) + 1;
        if (n <= 10) {
            LOGI(TPL_LOG " fullscreen token arm #%u self=%p faction=%d has=%d dmg=%.3f copy=%d local=%d"
                         "（全屏攻击：记下这一下，交给在场活体逐个施放）",
                 n, thiz, g_fsFaction, (int)g_fsHasFaction,
                 (double)SafeReadF32(stable, g_offDmgDamage.load(), 0.f), (int)copied, (int)localHit);
        }
    } else if (wantArm && !copied) {
        const uint32_t n = g_hitFsArm.fetch_add(1, std::memory_order_relaxed) + 1;
        if (n <= 4) LOGW(TPL_LOG " fullscreen token arm 跳过 #%u：damage=%p 无法深拷贝 %zu 字节",
                         n, damage, kFsDmgCopyBytes);
    } else if (wantArm && copied && !victimOk) {
        const uint32_t n = g_hitFsArm.fetch_add(1, std::memory_order_relaxed) + 1;
        if (n <= 4) LOGW(TPL_LOG " fullscreen token arm 跳过 #%u：受害者 %p 被类名闸挡（玩家/宠物/随从/NPC/HP）",
                         n, thiz);
    }
    return r;
}

// Killable.Update() -> Void（argc=0，每只活体每帧必经）：施放点
static void FullScreenTryApply(void* thiz) {
    if (t_inBcast) return;
    if (!g_origKillableTakeDamage || !OkPtr(thiz) || thiz == g_fsSelf || !g_fsDamage) return;
    if (!Il2Cpp::IsManagedObjectPtr(thiz)) return;      // ★ r15：只对**真托管对象**施放
    const size_t foff = g_offKillableFaction.load(std::memory_order_relaxed);
    if (g_fsHasFaction && foff) {                       // 同阵营判据（拿不到偏移时退化为不判）
        int32_t v = 0; memcpy(&v, reinterpret_cast<const char*>(thiz) + foff, sizeof(v));
        if (v != g_fsFaction) return;
    }
    if (!FsNameAllowed(thiz)) return;
    for (uint32_t i = 0; i < g_fsSeenCount; ++i) {
        if (g_fsSeen[i] == thiz) return;                // 本窗口已施放过 ⇒ 不重复
    }
    if (g_fsSeenCount >= kFsMaxTargets) { g_fsUntilMs.store(0, std::memory_order_release); return; }
    // ★ r15：每个目标发一份**独立**的拷贝（同一块缓冲被多个目标共用＝互相踩）
    void* send = g_fsDamage;
    if (g_fsDmgCopied) {
        const uint32_t slot = 1 + (g_fsDmgSlot.fetch_add(1, std::memory_order_relaxed) % (kFsDmgSlots - 1));
        memcpy(g_fsDmgRing[slot], g_fsDmgRing[0], kFsDmgCopyBytes);
        send = (void*)g_fsDmgRing[slot];
    }
    t_inBcast = true;
    // ★ r16：广播这一下也是「我们调原函数」（只是换了个受害者）⇒ 同样纳入护栏。
    //  某个目标自己崩了，只丢这一个目标，其余目标继续，进程不受影响。
    bool ok = false;
    FS_GUARDED("bcast.Killable.TakeDamage", ok, {
        g_origKillableTakeDamage(thiz, send, g_fsArg2, g_fsMethod);
    });
    t_inBcast = false;
    g_fsSeen[g_fsSeenCount++] = thiz;
    const uint32_t t = g_hitFsHit.fetch_add(1, std::memory_order_relaxed) + 1;
    if (t <= 8 || (t % 200) == 0) {
        LOGI(TPL_LOG " fullscreen token hit #%u target=%p seen=%u send=%p ok=%d"
                     "（全屏攻击：在场活体逐个施放同一份伤害）",
             t, thiz, g_fsSeenCount, send, (int)ok);
    }
}

static void Hooked_KillableUpdate(void* thiz, void* method) {
    const uint64_t until = g_fsUntilMs.load(std::memory_order_acquire);
    if (until) {                                        // 令牌未开时零额外开销
        if ((uint64_t)NowMonoMs() > until) g_fsUntilMs.store(0, std::memory_order_release);
        else FullScreenTryApply(thiz);
    }
    if (!g_origKillableUpdate) return;
    // ★ r16：原函数纳入护栏（拦下 ⇒ 这一帧的 Update 没跑完，不能补调用，直接返回）。
    bool ok = false;
    FS_GUARDED("orig.Killable.Update", ok, { g_origKillableUpdate(thiz, method); });
}
// 伤害计算入口：HitPoints::CalculationDamage(DamageInfo, Int32) -> Single
//  已降级为**纯探针**（不再缩放：与 ReallyDeductHP 同时缩放会产生平方）
static float Hooked_CalculationDamage(void* thiz, void* damage, int32_t count, void* method) {
    if (!g_origCalcDamage) return 0.f;
    float r = 0.f;
    bool ok = false;
    FS_GUARDED("orig.HitPoints.CalculationDamage", ok,
               { r = g_origCalcDamage(thiz, damage, count, method); });
    if (!ok) return 0.f;
    const uint32_t n = g_hitDmg.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 8) LOGI(TPL_LOG " dmg probe #%u CalculationDamage -> %.3f (乘算已迁至 ReallyDeductHP)", n, r);
    return r;
}

// ---- 6.3 技能无CD —— **已删除**（★ 2026-10-11 五轮定稿，用户原话：
//   「技能无cd没用可以删除了」）。
//   删除范围：菜单项 Toggle_技能无CD、开关 g_nocd、三个挂点
//   （PlayerActor.SecSkillFinishCooling/1、SecSkillPowerUp.get_CoolingTime/0、
//     set_CoolingTime/1）。功能常量已重排（全屏攻击 4→3，其后依次前移一位）。
//   注意：删除 ≠ 关闭 —— 三个逻辑槽位也一并摘除，避免「留着不用」的钩子
//   在本游戏后续版本里变成不可预期的行为源。

// ---- 6.4 资源：NEON.Game.Managers.NEONPlayerState 的属性 getter ----
//  数据型但无同名后备字段（_coin/_key/_crystal 在 metadata 中不存在）
//  ⇒ 按对照方 nhsy1 斌哥菜单的路线 hook getter 返回大值。
//  §28 说明：这是「数量上限」类，不是输入门禁 ⇒ 不会自动连发。
//
//  ★ 2026-10-08 实测「无限金币/钥匙/水晶全部无效」补充：这些属性 getter 并
//    不在商店结算路径上（商店走 6.4d 的通用 CostType 通道）。保留它们是
//    因为 HUD / 存档仍读它们。
//  ★ 2026-10-10 三次定稿：回到「一功能一开关」——每个资源各回自己的原子，
//    互不牵连（金币 ≠ 钥匙 ≠ 水晶 ≠ 手雷）。
static inline bool GateCoin()    { return g_coin.load(); }
static inline bool GateKey()     { return g_key.load(); }
static inline bool GateCrystal() { return g_crystal.load(); }

// get_Coin / get_MaxCoin / get_Key / get_CurrentSpriteCrystalCount / get_GitterCoin
static int32_t Hooked_GetCoin(void* thiz, void* method) {
    const int32_t r = GuardedOr<int32_t>("orig.get_Coin", 0, [&]{ return g_origGetCoin ? g_origGetCoin(thiz, method) : 0; });
    if (!GateCoin()) return r;
    const uint32_t n = g_hitCoin.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 5) LOGI(TPL_LOG " coin probe #%u real=%d -> %d", n, r, kInfiniteAmount);
    return kInfiniteAmount;
}

static int32_t Hooked_GetMaxCoin(void* thiz, void* method) {
    const int32_t r = GuardedOr<int32_t>("orig.get_MaxCoin", 0, [&]{ return g_origGetMaxCoin ? g_origGetMaxCoin(thiz, method) : 0; });
    if (!GateCoin()) return r;
    return r > kInfiniteAmount ? r : kInfiniteAmount;
}

static int32_t Hooked_GetKey(void* thiz, void* method) {
    const int32_t r = GuardedOr<int32_t>("orig.get_Key", 0, [&]{ return g_origGetKey ? g_origGetKey(thiz, method) : 0; });
    if (!GateKey()) return r;
    const uint32_t n = g_hitKey.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 5) LOGI(TPL_LOG " key probe #%u real=%d -> %d", n, r, kInfiniteAmount);
    return kInfiniteAmount;
}

static int32_t Hooked_GetCrystal(void* thiz, void* method) {
    const int32_t r = GuardedOr<int32_t>("orig.get_Crystal", 0, [&]{ return g_origGetCrystal ? g_origGetCrystal(thiz, method) : 0; });
    if (!GateCrystal()) return r;
    const uint32_t n = g_hitCrystal.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 5) LOGI(TPL_LOG " crystal probe #%u real=%d -> %d（展示值路径：实测 0 次调用＝死路径）",
                     n, r, kInfiniteAmount);
    return kInfiniteAmount;
}

// ── r8b 全屏攻击：射程读点＝透传 + 记实例（不在这一层伪造返回值，见上方注释）──
static void* Hooked_GetProjectileRange(void* thiz, void* method) {
    void* stat = GuardedOr<void*>("orig.GetProjectileRange", nullptr, [&]{ return g_origProjRange ? g_origProjRange(thiz, method) : nullptr; });
    const uint32_t n = g_hitRange.fetch_add(1, std::memory_order_relaxed) + 1;
    if (!OkPtr(stat)) {
        if (n <= 12) LOGW(TPL_LOG " fullscreen range probe #%u stat=NULL（未取到射程 stat 实例）", n);
        return stat;
    }
    void* prev = g_projRangeStat.exchange(stat, std::memory_order_relaxed);
    if (n <= 12 || prev != stat) {
        LOGI(TPL_LOG " fullscreen range probe #%u stat=%p prev=%p on=%d（射程 stat 实例；强制点在 Stat.get_Value）",
             n, stat, prev, g_full.load(std::memory_order_relaxed) ? 1 : 0);
    }
    return stat;
}

// 只读探针：DamageSize（与射程同类的 stat 对象，本轮只留证据，不动它）
static void* Hooked_GetDamageSize(void* thiz, void* method) {
    void* stat = GuardedOr<void*>("orig.GetDamageSize", nullptr, [&]{ return g_origDamageSize ? g_origDamageSize(thiz, method) : nullptr; });
    const uint32_t n = g_hitDamageSize.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 12) {
        LOGI(TPL_LOG " fullscreen damagesize probe #%u stat=%p（只读探针）", n, stat);
    }
    return stat;
}

// ── r8b 全屏攻击真正的强制点：Veewo.Stats.Stat.get_Value（实例闸，1:1）──
//  只有 `thiz` 是射程那一个 Stat 实例时才抬到 99999；不写字段、不碰缓存 ⇒
//  游戏自身的 Refresh / Dirty / 事件逻辑完全不受影响，最坏情况只是「没命中、不生效」。
static float Hooked_StatValue(void* thiz, void* method) {
    const float r = GuardedOr<float>("orig.Stat.get_Value", 0.0f, [&]{ return g_origStatValue ? g_origStatValue(thiz, method) : 0.0f; });
    if (!g_full.load(std::memory_order_relaxed)) return r;
    if (!thiz || thiz != g_projRangeStat.load(std::memory_order_relaxed)) return r;
    const uint32_t n = g_hitRangeForce.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 12) {
        LOGI(TPL_LOG " fullscreen stat force #%u get_Value %.3f -> %.1f（射程 stat 实例闸命中）",
             n, static_cast<double>(r), static_cast<double>(kFullRangeForce));
    }
    return kFullRangeForce;
}

// ── r8 无限水晶：SpriteManager 真通道 ──
//  补发/退款都走游戏自带的 AddCrystalSprite()（作者意图内的入口，不写任何字段内存）。
//  补发只做一次（g_grantCrystalTried），目的是「先给一个」而不是「凭空造满」：
//  为 0 时把 count 硬抬到 9999 会让按 count 索引的 UI/逻辑越界，所以 0 保持 0。
static void CrystalSpriteRefund(void* spriteMgr, const char* why, uint32_t n) {
    auto& api = GetApi();
    if (!g_miAddCrystalSprite || !api.runtime_invoke || !OkPtr(spriteMgr)) {
        if (n <= 10) LOGW(TPL_LOG " crystal: 拒绝调用 AddCrystalSprite（句柄=%p 实例=%p why=%s）",
                          g_miAddCrystalSprite, spriteMgr, why ? why : "?");
        return;
    }
    void* exc = nullptr;
    api.runtime_invoke(g_miAddCrystalSprite, spriteMgr, nullptr, &exc);
    if (n <= 10) {
        LOGI(TPL_LOG " crystal: 已调用 SpriteManager.AddCrystalSprite() 补回 1 个（%s exc=%p）",
             why ? why : "?", exc);
    }
}

static int32_t Hooked_GetCrystalSpriteCount(void* thiz, void* method) {
    const int32_t r = GuardedOr<int32_t>("orig.GetCrystalSpriteCount", 0, [&]{ return g_origGetCrystalSpriteCount ? g_origGetCrystalSpriteCount(thiz, method) : 0; });
    const bool on = GateCrystal();
    const uint32_t n = g_hitCrystalCount.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 10) {
        LOGI(TPL_LOG " crystal probe #%u SpriteManager.GetCrystalSpriteCount real=%d on=%d（本层只探针：不抬 count，避免按数量索引的 UI/逻辑越界）",
             n, r, on ? 1 : 0);
    }
    if (!on) return r;
    if (r <= 0) {                     // 一个都没有时先给一个（只补一次）
        if (!g_grantCrystalTried.load(std::memory_order_relaxed)) {
            g_grantCrystalTried.store(true, std::memory_order_relaxed);
            CrystalSpriteRefund(thiz, "无限水晶：起手补发 1 个", n);
        }
        return r;
    }
    // ★ r8b：不再把 count 抬到 9999（真机未必安全：按数量索引的 UI/逻辑会越界）；
    //   「无限水晶」的实效改由「用完退款 + 精灵攻击免费」承担（数量只增不减）。
    return r;
}

// ── r9（用户 m04709「无限水晶改成无条件使用就可以了」）──
//  语义：开闸后水晶的「使用」不再受余额限制 —— 先按游戏自己的入口补 1 个
//  （AddCrystalSprite，作者意图内的 API，不写任何字段内存），再重试一次原调用；
//  成功后退款 ⇒ 数量只增不减。关闸时完全原样转发。
static int32_t Hooked_UseCrystalSprite(void* thiz, intptr_t idx, void* method) {
    const int32_t r = GuardedOr<int32_t>("orig.UseCrystalSprite", 0, [&]{ return g_origUseCrystalSprite ? g_origUseCrystalSprite(thiz, idx, method) : 0; });
    const bool on = GateCrystal();
    const uint32_t n = g_hitUseCrystal.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 10) {
        LOGI(TPL_LOG " crystal probe #%u SpriteManager.UseCrystalSprite(idx=%d) real=%d on=%d",
             n, static_cast<int>(idx), r, on ? 1 : 0);
    }
    if (!on) return r;
    if (r <= 0 && g_origUseCrystalSprite) {      // 余额不足/失败 ⇒ 无条件使用：补 1 再重试
        CrystalSpriteRefund(thiz, "无限水晶：余额不足，先补 1 再执行", n);
        const int32_t r2 = GuardedOr<int32_t>("orig.UseCrystalSprite.retry", 0, [&]{ return g_origUseCrystalSprite ? g_origUseCrystalSprite(thiz, idx, method) : 0; });
        CrystalSpriteRefund(thiz, "无限水晶：重试成功后退款", n);
        if (n <= 10) {
            LOGI(TPL_LOG " crystal: 无条件使用 real=%d -> 重试=%d（不足即补）", r, r2);
        }
        return r2 > 0 ? r2 : 1;
    }
    CrystalSpriteRefund(thiz, "无限水晶：用完退款", n);
    return r;
}

// 只读探针：水晶数量是否真由 AddCrystalSprite() 增长（判定水晶体系是否活着，
//   为下一轮「消耗点到底在哪」留证据；不改任何值）。
typedef void (*VoidGetter_t)(void* thiz, void* method);
static VoidGetter_t           g_origAddCrystalSprite = nullptr;
static std::atomic<uint32_t>  g_hitCrystalAdd{0};

static void Hooked_AddCrystalSprite(void* thiz, void* method) {
    const uint32_t n = g_hitCrystalAdd.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 10) {
        LOGI(TPL_LOG " crystal probe #%u SpriteManager.AddCrystalSprite（水晶被增加：通道活着）", n);
    }
    GuardedVoid("orig.AddCrystalSprite", [&]{ if (g_origAddCrystalSprite) g_origAddCrystalSprite(thiz, method); });
}

static bool Hooked_ConsumeSpriteAtk(void* thiz, intptr_t a, intptr_t b, void* method) {
    const bool on = GateCrystal();
    const bool r = on ? true
                      : (g_origConsumeSpriteAtk ? g_origConsumeSpriteAtk(thiz, a, b, method) : false);
    const uint32_t n = g_hitSpriteAtk.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 10) {
        LOGI(TPL_LOG " crystal probe #%u ConsumeSpriteAtkRaw(%d,%d) on=%d -> %d（无限水晶：精灵攻击免费）",
             n, static_cast<int>(a), static_cast<int>(b), on ? 1 : 0, r ? 1 : 0);
    }
    return r;
}

// ================= ★ r11 无限水晶：共享实现钩子（veneer 目标） =================
//  `SpriteManager.GetCrystalSpriteCount` 的入口是 8 字节 veneer
//    `mov w1,#imm ; b <共享实现>`
//  我们的入口形态闸门（避免 Dobby 16 字节补丁吃邻居）会跳过它 ⇒ 这个 getter
//  从来没被挂上过。这里改为挂 **veneer 的目标**（共享实现），并用「6 参原样转发」
//  保证未知参数不被截断；只拦「选择子 == veneer 里那个立即数」的调用。
// ★ r12b：veneer 入口钩子。我们只替换了 veneer 的 `mov w1,#imm`，所以 w1 要自己补上；
//  其余参数（x0..x5）与返回值原样透传。开闸时直接返回 kInfiniteAmount（水晶计数读点被抬）。
static int32_t Hooked_CrystalCountVeneer(void* a0, void* a1, void* a2,
                                         void* a3, void* a4, void* a5) {
    (void)a1;                          // 真值由 veneer 的 `mov w1,#imm` 提供，这里被我们替换掉了
    const uint32_t n = g_hitCrystalImpl.fetch_add(1, std::memory_order_relaxed) + 1;
    int32_t real = 0;
    if (g_implCrystalCount) {
        real = g_implCrystalCount(a0, reinterpret_cast<void*>(g_crystalCountSel), a2, a3, a4, a5);
    }
    if (GateCrystal()) {
        const uint32_t s = g_crystalImplShown.fetch_add(1, std::memory_order_relaxed) + 1;
        if (s <= 10) {
            LOGI(TPL_LOG " crystal veneer #%u sel=0x%zX real=%d -> %d（无限水晶：veneer 入口 4 字节补丁）",
                 n, g_crystalCountSel, real, kInfiniteAmount);
        }
        return kInfiniteAmount;
    }
    if (n <= 10) {
        LOGI(TPL_LOG " crystal veneer #%u sel=0x%zX real=%d on=0（无限水晶关闸，原值放行）",
             n, g_crystalCountSel, real);
    }
    return real;
}

//  ★ r12b：解 veneer（`mov w1,#imm ; b target`），**只在 veneer 入口改 4 字节**（不碰 target）。
static bool InstallCrystalCountImplHook() {
    if (g_crystalVeneerHooked) return true;
    void* cls = FindClassNs("NEON.Game", "SpriteManager");
    if (!cls) cls = GetApi().FindClassByShortName("SpriteManager");
    if (!cls) { LOGW(TPL_LOG " Res_CrystalCountImpl 未找到 SpriteManager 类 — 本局无限水晶仅走读点抬值"); return false; }
    void* mi = GetApi().GetMethod(cls, "GetCrystalSpriteCount", 0);
    if (!mi) { LOGW(TPL_LOG " Res_CrystalCountImpl 未找到 GetCrystalSpriteCount — 跳过"); return false; }
    void* fn = *reinterpret_cast<void**>(mi);
    if (!fn || !Il2Cpp::IsExecutableRange((uintptr_t)fn, 8)) {
        LOGW(TPL_LOG " Res_CrystalCountImpl 方法指针不可执行 ptr=%p — 跳过", fn);
        return false;
    }
    const uint32_t w0 = reinterpret_cast<const uint32_t*>(fn)[0];
    const uint32_t w1 = reinterpret_cast<const uint32_t*>(fn)[1];
    // 只接受 `b <impl>` 作第二条指令（0x14000000 掩码）+ 第一条是 MOVZ/MOVN 到 x1 的立即数
    if ((w1 & 0xFC000000u) != 0x14000000u) {
        LOGW(TPL_LOG " Res_CrystalCountImpl 入口不是 `b` 尾调用（w1=0x%08X）— 跳过（留给常规槽位）", w1);
        return false;
    }
    // ★ 编码校验（r11b 修）：MOVZ/MOVN 的 Rd 是 bits[4:0]（早期误写成 bits[9:5]，
    //   把 rd 判成 0 ⇒ 真机 `0x52800401`(mov w1,#0x20) 被误拒、veneer 目标从未挂上）
    const uint32_t rd = w0 & 0x1Fu;
    const bool movz = ((w0 & 0x7F800000u) == 0x52800000u) && (rd == 1);
    const bool movn = ((w0 & 0x7F800000u) == 0x12800000u) && (rd == 1);
    if (!movz && !movn) {
        LOGW(TPL_LOG " Res_CrystalCountImpl 入口首条不是 `mov w1,#imm`（w0=0x%08X）— 跳过", w0);
        return false;
    }
    int64_t imm26 = (int64_t)(w1 & 0x03FFFFFFu);
    if (imm26 & 0x02000000) imm26 -= 0x04000000;          // 符号扩展
    void* target = reinterpret_cast<uint8_t*>(fn) + (imm26 << 2);
    uint32_t imm16 = (w0 >> 5) & 0xFFFFu;
    if (w0 & 0x00200000u) imm16 <<= 16;                    // hw 位
    uintptr_t sel = movn ? (~(uintptr_t)imm16 & 0xFFFFFFFFu) : (uintptr_t)imm16;
    if (!Il2Cpp::IsExecutableRange((uintptr_t)target, 16)) {
        LOGW(TPL_LOG " Res_CrystalCountImpl veneer 目标 %p 不可执行 — 跳过", target);
        return false;
    }

    // ---- ★ r12 取证：这个 veneer 目标是不是「共享实现」？它的 16 字节窗口里还有谁？----
    //  判据意义：若窗口内存在**其它方法入口**，则 Dobby 的 16 字节入口补丁必然把它们改成野指令
    //  （r7 事故机理）；若同一实现被多个方法的 veneer 指向，则该实现是「常量偏移共享访问器」，
    //  必然是极短叶函数 ⇒ 必须走 4 字节补丁。两件事都打成一行日志，进关崩不崩都能自证。
    {
        void* iter = nullptr;
        int guard = 0, shared = 0, inWindow = 0;
        void* winEntry[4] = {nullptr, nullptr, nullptr, nullptr};
        const char* winName[4] = {nullptr, nullptr, nullptr, nullptr};
        while (GetApi().class_methods) {
            void* mi2 = GetApi().class_methods(cls, &iter);
            if (!mi2) break;
            if (++guard > 4096) break;
            void* f2 = *reinterpret_cast<void**>(mi2);
            if (!OkPtr(f2)) continue;
            if (f2 >= target && f2 < reinterpret_cast<char*>(target) + 16) {
                if (inWindow < 4) {
                    winEntry[inWindow] = f2;
                    winName[inWindow] = GetApi().method_get_name ? GetApi().method_get_name(mi2) : nullptr;
                }
                ++inWindow;
            }
            uint32_t y1 = 0;
            if (!ReadU32Guarded("Res_CrystalCountImpl", reinterpret_cast<const uint32_t*>(f2) + 1, &y1)) continue;
            if ((y1 & 0xFC000000u) != 0x14000000u) continue;      // 第二条指令不是无条件 B
            int64_t im = static_cast<int64_t>(y1 & 0x03FFFFFFu);
            if (im & 0x02000000LL) im -= 0x04000000LL;
            if (reinterpret_cast<char*>(f2) + (im << 2) == target) ++shared;
        }
        LOGI(TPL_LOG " Res_CrystalCountImpl 共享分析：SpriteManager 内 %d 个方法经 veneer 指向同一实现 %p；"
                     "该实现 16 字节窗口内另有 %d 个方法入口%s%s%s%s（判定共享/邻居是否被吃掉）",
             shared, target, inWindow,
             inWindow > 0 ? "：" : "",
             inWindow > 0 ? (winName[0] ? winName[0] : "(?)") : "",
             inWindow > 1 ? "," : "", inWindow > 1 ? (winName[1] ? winName[1] : "(?)") : "");
    }

    // ---- ★ r12：veneer 目标也必须过「入口形态闸门」 ----
    //  背景（2026-10-12 真机，用户 m05208「进游戏直接闪退了」）：r11 直接在**目标地址**上
    //  用 Dobby（固定改写入口 16 字节）装钩，**绕过了本文件 §⑤.1 的两道形态闸门**。而这条
    //  veneer（`mov w1,#imm ; b <共享实现>`）的目标正是 Unity 那种「常量偏移共享访问器」——
    //  典型形态 `ldr w0,[x0,x1] ; ret`（8 字节）⇒ 16 字节补丁必然吃掉紧随其后的邻居方法入口
    //  （就是 r7 那次 SIGSEGV fault addr 0xb8 的同一机理）。现场佐证：装钩后主菜单存活 20 分钟
    //  （那些方法没被调用），一进关（精灵/水晶代码首次运行）3 秒即 SIGSEGV 且无任何 tombstone
    //  （THEMIS 自处理），我方钩子一行日志都没出 ⇒ 崩在**邻居方法**上，不是崩在钩子里。
    //  做法：与常规槽位同款三步 —— 读入口 4 字 → 极小函数走 4 字节补丁（HookTiny4，不碰邻居）、
    //  短函数（+0/+4/+8 即终止指令）跳过、其余才用 Dobby。
    uint32_t tw[4] = {0, 0, 0, 0};
    {
        const uint32_t* code = reinterpret_cast<const uint32_t*>(target);
        for (int i = 0; i < 4; ++i) {
            if (!ReadU32Guarded("Res_CrystalCountImpl", code + i, &tw[i])) break;
        }
    }
    int tRetAt = -1;
    for (int i = 0; i < 4; ++i) {
        if ((tw[i] & 0xFFFFFC1Fu) == 0xD65F0000u) { tRetAt = i; break; }   // RET
    }
    const bool tTiny = TinyHookable(tw, tRetAt);
    LOGI(TPL_LOG " Res_CrystalCountImpl 目标形态 impl=%p code=%08X %08X %08X %08X%s",
         target, tw[0], tw[1], tw[2], tw[3],
         tTiny ? "  <-TINY/RET -> 4B 补丁（不碰邻居）" : ((tRetAt >= 0) ? "  <-RET" : ""));
    if ((tw[0] & 0xFFFFFC1Fu) == 0xD61F0000u) {           // target 本身是 `br xN`：只提示（本版不再改 target）
        LOGI(TPL_LOG " Res_CrystalCountImpl veneer 目标 %p 本身是 `br xN` — 无妨（r12b 只改 veneer 入口）", target);
    }
    if (HookEntryAlready(fn)) {
        LOGW(TPL_LOG " Res_CrystalCountImpl veneer 入口 %p 已被前面槽位挂钩 — 跳过重复挂点", fn);
        return false;
    }
    // ★ r12b（2026-10-12 真机：用户 m05208「进游戏直接闪退了」）：
    //  这里**绝不能再在 target 上用 Dobby**。Dobby 的入口补丁固定改写 16 字节，而真机取证
    //  （`共享分析` 那行）证明该实现的 16 字节窗口内**另有**一个方法入口
    //  （`GetCustomSpriteTypeCount`）⇒ 补丁会把邻居的方法入口改成野指令，一进关邻居首次运行
    //  即 SIGSEGV（与 r7 事故同机理，且我方钩子一行日志都不出）。
    //  改法：只把 **veneer 自己的第一条指令**（`mov w1,#imm`）换成 `b <hook>` —— 只覆盖 4 字节，
    //  veneer 只有 8 字节、+8 起的邻居入口一个字节都不动；钩子内直接调用真实现。
    const int foreign = ForeignEntryInWindow(cls, target, nullptr, 0);
    if (foreign > 0) {
        LOGW(TPL_LOG " Res_CrystalCountImpl 目标窗口内另有 %d 个方法入口 ⇒ 禁用 Dobby，"
                     "改走 veneer 入口 4 字节补丁（不碰共享实现）", foreign);
    }
    g_crystalCountSel = sel;
    g_implCrystalCount = reinterpret_cast<Shared6_t>(target);
    if (!PatchEntryBranch4("Res_CrystalCountImpl", "veneer", fn,
                           reinterpret_cast<void*>(Hooked_CrystalCountVeneer))) {
        g_crystalCountSel = 0;
        g_implCrystalCount = nullptr;
        return false;
    }
    g_crystalVeneerHooked = true;
    HookEntryNote(fn);
    LOGI(TPL_LOG " Res_CrystalCountImpl hooked veneer=%p（只改入口 4 字节）impl=%p sel=0x%zX "
                 "共享实现未被修改（无限水晶：不再用 Dobby，避免吃邻居方法入口）",
         fn, target, sel);
    return true;
}

static int32_t Hooked_GetGitterCoin(void* thiz, void* method) {
    const int32_t r = GuardedOr<int32_t>("orig.get_GitterCoin", 0, [&]{ return g_origGetGitter ? g_origGetGitter(thiz, method) : 0; });
    // ★ 2026-10-11 五轮定稿（用户实测 #3「无限金币没开、金币不够也能买」）：
    //   闪光币 GitterCoin(GlitterCoin) 是**金币的变体**，旧版却挂在 GateCrystal 上
    //   ⇒ 只要「无限水晶」开着，闪光币余额就恒 9999 ⇒ 商店不花真金币也能买单。
    //   归位到金币通道（一功能一开关：闪光币归金币，不归水晶）。
    if (!GateCoin()) return r;
    const uint32_t n = g_hitRes.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 6) LOGI(TPL_LOG " gitter probe #%u real=%d -> %d (闪光币归金币通道)", n, r, kInfiniteAmount);
    return kInfiniteAmount;
}

// get_HasInfiniteKey -> true（游戏自带的「钥匙无限」通道）
//   ★ 2026-10-11 四次定稿：本挂点归「无限彩色钥匙」(CKEY)，**不再**归「无限钥匙」。
//     理由：HasInfiniteKey 是游戏自己的「钥匙套装(KeySuit)/彩色钥匙」状态，
//     置真后 HUD 的钥匙图标走 KeyImageSync.infiniteKeySprite（彩色/∞），
//     且所有读该状态的消耗路径全免 —— 与「无限钥匙」(数量 9999 + CostType.Key 免费)
//     是两条不同的通道 ⇒ 满足「一功能一开关 1:1」。
static bool Hooked_HasInfiniteKey(void* thiz, void* method) {
    const bool r = GuardedOr<bool>("orig.get_HasInfiniteKey", false, [&]{ return g_origHasInfKey ? g_origHasInfKey(thiz, method) : false; });
    if (!g_ckey.load()) return r;
    const uint32_t n = g_hitCKey.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 10) LOGI(TPL_LOG " colored-key probe #%u HasInfiniteKey real=%d -> 1", n, (int)r);
    return true;
}

// get_InfiniteKey -> >0（同一个 KeySuit 通道的计数面；HUD/门禁也可能读它）
static int32_t Hooked_GetInfiniteKey(void* thiz, void* method) {
    const int32_t r = GuardedOr<int32_t>("orig.get_InfiniteKey", 0, [&]{ return g_origGetInfKey ? g_origGetInfKey(thiz, method) : 0; });
    if (!g_ckey.load()) return r;
    const uint32_t n = g_hitCKey.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 10) LOGI(TPL_LOG " colored-key probe #%u InfiniteKey real=%d -> %d", n, r, kInfiniteAmount);
    return r > kInfiniteAmount ? r : kInfiniteAmount;
}

// ---- 6.4b 次要读取面：商店实际读 CoinValue / RealKey / KeyValue ----
//  只 hook get_Coin 不够 —— HasEnough 走的是这几个「真实值」getter。
static int32_t Hooked_GetCoinValue(void* thiz, void* method) {
    const int32_t r = GuardedOr<int32_t>("orig.get_CoinValue", 0, [&]{ return g_origGetCoinValue ? g_origGetCoinValue(thiz, method) : 0; });
    if (!GateCoin()) return r;
    const uint32_t n = g_hitCoin.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 5) LOGI(TPL_LOG " coinValue probe #%u real=%d -> %d", n, r, kInfiniteAmount);
    return kInfiniteAmount;
}

static int32_t Hooked_GetRealKey(void* thiz, void* method) {
    const int32_t r = GuardedOr<int32_t>("orig.get_RealKey", 0, [&]{ return g_origGetRealKey ? g_origGetRealKey(thiz, method) : 0; });
    if (!GateKey()) return r;
    const uint32_t n = g_hitKey.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 5) LOGI(TPL_LOG " realKey probe #%u real=%d -> %d", n, r, kInfiniteAmount);
    return kInfiniteAmount;
}

static int32_t Hooked_GetKeyValue(void* thiz, void* method) {
    const int32_t r = GuardedOr<int32_t>("orig.get_KeyValue", 0, [&]{ return g_origGetKeyValue ? g_origGetKeyValue(thiz, method) : 0; });
    if (!GateKey()) return r;
    return kInfiniteAmount;
}

// ---- 6.4c 扣减面：恒不扣（从源头保证底层字典余额不下降）----
//  §28 复核：这不是「输入门禁」，而是「消费执行体」⇒ 恒成功不会引发自动连发；
//  返回 true = 扣款成功，是唯一能让「买得起」成立的语义。
//  故意**不调用 orig**，因此形参类型无关（用 intptr_t 原样透传寄存器）。
static bool Hooked_ConsumeCoinRaw(void* thiz, intptr_t a, intptr_t b, void* method) {
    (void)thiz; (void)b; (void)method;
    if (!GateCoin()) return GuardedOr<bool>("orig.ConsumeCoin", false,
        [&]{ return g_origConsumeCoin ? g_origConsumeCoin(thiz, a, b, method) : false; });
    const uint32_t n = g_hitCoin.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 5) LOGI(TPL_LOG " consumeCoinRaw probe #%u amount=%ld -> no-op", n, (long)a);
    return true;
}

static bool Hooked_ConsumeKeyRaw(void* thiz, intptr_t a, intptr_t b, void* method) {
    (void)thiz; (void)b; (void)method;
    if (!GateKey()) return GuardedOr<bool>("orig.ConsumeKey", false,
        [&]{ return g_origConsumeKey ? g_origConsumeKey(thiz, a, b, method) : false; });
    const uint32_t n = g_hitKey.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 5) LOGI(TPL_LOG " consumeKeyRaw probe #%u amount=%ld -> no-op", n, (long)a);
    return true;
}

// ============================================================================
//  6.4d 通用资源通道：NEONPlayerState 的 CostType 接口
//       ★ 真机实测「无限金币/钥匙/水晶无效」后的真挂点
// ============================================================================
//  实测证据（用户口述）：开「无限金币」后商店仍提示「需要更多金币」；
//  开「无限水晶」后使用仍提示「需要更多水晶」⇒ 商店/使用判定根本不走
//  get_Coin/get_Key/get_CurrentSpriteCrystalCount 这些属性，而走这里的通用接口。
//
//  三段式覆盖（hook-rules §21.1 数据优先）：
//    ①「买得起吗」HasEnough / HasEnoughRaw           → 恒 true
//    ②「我有多少」GetAmount / GetAmountRaw           → 返回大值
//    ③「消费执行体」ConsumeRaw(4) 恒成功 + SetResource(2) 直写大值（底层余额不下降）
//
//  ABI：含浮点形参的重载显式声明 float（落到 s0，不是 x 寄存器）；
//       其余一律 intptr_t 原样透传。
//  ⚠ 故意不挂的两个重载：ConsumeRaw(CostType,T#34288,T#24771) 与
//    ConsumeRaw(CostType,Single,T#24771) 都是 argc=3，无法从 metadata 分辨
//    第 2 个形参是 int 还是 float ⇒ 关开关时 orig 透传会错寄存器。
static std::atomic<int32_t> g_ctBomb{-1};
static std::atomic<int32_t> g_ctCoin{-1};
static std::atomic<int32_t> g_ctKey{-1};
static std::atomic<int32_t> g_ctCrystal{-1};
//  ★★★ r15（修 m06085「水晶还是没效果，仍然提示需要更多水晶」）：水晶通道必须**并入 Mana**
//   真机证据（跨全部 11 个记录仪目录统计）：
//     · `costType=2`(CostType.Crystal) 在**任何会话里都是 0 次**；
//     · 玩家实测那一次「要水晶」的交互，可负担性判定走的是
//       `NEONPlayerState.HasEnough / HasEnoughRaw / GetAmountRaw`，`costType=22`(CostType.Mana)
//       （r14 会话 `03:11:20.961 … res probe #1 HasEnough costType=22 gate=0 (Bomb=0 Coin=1 Crystal=2 Key=5)`，
//        早于全屏崩溃 10 秒，tid 与崩溃同一线程 13384；20261011_034826 同一模式）；
//     · 离线元数据里全库唯一一条「需要更多 X」的提示是
//       `NEON.Game.Portal.promptMsgNeedMoreMana`（token=0x04006BA4），X = **Mana**；
//     · `NEON.Game.PowerUps.AlternativeResourcePu.UnifyManaCrystal`（token=0x0600F75F）
//       ⇒ 这游戏的 Mana 就是玩家界面上看到的「水晶 / 灵力水晶」。
//  语义：Mana 只由「无限水晶」这一个开关管（不与金币/钥匙/手雷串联，保持一功能一开关）。
static std::atomic<int32_t> g_ctMana{-1};

// 资源闸（r15：**五通道白名单 + 一功能一开关**）。
//  命中白名单 = CostType.Bomb / Coin / Crystal / Key / Mana，且只有该项自己的开关为开时才生效：
//    开「无限手雷」⇒ 只有 CostType.Bomb 走闸；金币/水晶/钥匙完全不受影响（反之亦然）。
//  Mana 归「无限水晶」（开门消耗的是 Mana，见上面 g_ctMana 的取证）。
//  其余 32 个 CostType（Egg / Heart / Curse / Cooling / Unlocked / RewardDepleted …）
//  一律返回 false ⇒ 完整走游戏原逻辑（上版万能闸的副作用来源）。
static bool ResGateOn(int32_t ct) {
    if (ct == g_ctBomb.load())    return g_bomb.load();
    if (ct == g_ctCoin.load())    return g_coin.load();
    if (ct == g_ctCrystal.load()) return g_crystal.load();
    if (ct == g_ctKey.load())     return g_key.load();
    if (ct == g_ctMana.load())    return g_crystal.load();
    return false;
}

// 探针（★ 2026-10-11 **无偏化**：不过闸也打）
//  旧版只在 ResGateOn 命中时才打印 ⇒ 只能证明「白名单通道被调用过」，无法回答
//  「商店/水晶机/钥匙机的判定到底走没走 CostType 接口」（用户实测 #2/#3 的盲区）。
//  现在：进函数即打，但**每个 costType 数值只打前 4 次**（上限自带，不会被每帧
//  调用的通道刷屏），并附带四项白名单数值 + 本次是否过闸。
static void ResProbeCost(const char* who, int32_t ct) {
    if (ResGateOn(ct)) g_hitRes.fetch_add(1, std::memory_order_relaxed);
    if (ct < 0 || ct >= (int32_t)(sizeof(g_ctSeen) / sizeof(g_ctSeen[0]))) {
        const uint32_t n = g_hitResOdd.fetch_add(1, std::memory_order_relaxed) + 1;
        if (n <= 4) {
            LOGI(TPL_LOG " res probe(越界) #%u %s costType=%d (Bomb=%d Coin=%d Crystal=%d Key=%d Mana=%d)",
                 n, who, ct, g_ctBomb.load(), g_ctCoin.load(), g_ctCrystal.load(), g_ctKey.load(),
                 g_ctMana.load());
        }
        return;
    }
    const int n = g_ctSeen[ct].fetch_add(1, std::memory_order_relaxed) + 1;
    if (n > 4) return;
    LOGI(TPL_LOG " res probe #%d %s costType=%d gate=%d (Bomb=%d Coin=%d Crystal=%d Key=%d Mana=%d)",
         n, who, ct, (int)ResGateOn(ct),
         g_ctBomb.load(), g_ctCoin.load(), g_ctCrystal.load(), g_ctKey.load(), g_ctMana.load());
}

// ①-a 「我有多少」
static int32_t Hooked_GetAmountRaw(void* thiz, int32_t ct, intptr_t p2, void* method) {
    const int32_t r = GuardedOr<int32_t>("orig.GetAmountRaw", 0, [&]{ return g_origGetAmountRaw ? g_origGetAmountRaw(thiz, ct, p2, method) : 0; });
    ResProbeCost("GetAmountRaw", ct);
    if (!ResGateOn(ct)) return r;
    return r > kInfiniteAmount ? r : kInfiniteAmount;
}

static int32_t Hooked_GetAmount(void* thiz, int32_t ct, intptr_t p2, void* method) {
    const int32_t r = GuardedOr<int32_t>("orig.GetAmount", 0, [&]{ return g_origGetAmount ? g_origGetAmount(thiz, ct, p2, method) : 0; });
    ResProbeCost("GetAmount", ct);
    if (!ResGateOn(ct)) return r;
    return r > kInfiniteAmount ? r : kInfiniteAmount;
}

// ①-b 「买得起吗」（恒真；不调 orig ⇒ 形参语义无关紧要）
static bool Hooked_HasEnough(void* thiz, int32_t ct, intptr_t amount, intptr_t flag, void* method) {
    ResProbeCost("HasEnough", ct);
    if (ResGateOn(ct)) return true;
    return GuardedOr<bool>("orig.HasEnough", false,
        [&]{ return g_origHasEnough ? g_origHasEnough(thiz, ct, amount, flag, method) : false; });
}

static bool Hooked_HasEnoughRaw(void* thiz, int32_t ct, intptr_t amount, intptr_t flag, void* method) {
    ResProbeCost("HasEnoughRaw", ct);
    if (ResGateOn(ct)) return true;
    return GuardedOr<bool>("orig.HasEnoughRaw", false,
        [&]{ return g_origHasEnoughRaw ? g_origHasEnoughRaw(thiz, ct, amount, flag, method) : false; });
}

// 重载二：HasEnoughRaw(CostType, Single) —— 浮点实参在 s0
static bool Hooked_HasEnoughRawFloat(void* thiz, int32_t ct, float amountRoundUp, void* method) {
    ResProbeCost("HasEnoughRaw(f)", ct);
    if (ResGateOn(ct)) return true;
    return GuardedOr<bool>("orig.HasEnoughRawFloat", false,
        [&]{ return g_origHasEnoughRawF ? g_origHasEnoughRawF(thiz, ct, amountRoundUp, method) : false; });
}

// ③-a 「消费执行体」：恒成功、不实扣（关闸时原样转发）
static bool Hooked_ConsumeRaw4(void* thiz, int32_t ct, int32_t amount, int32_t reported,
                               bool silent, void* method) {
    ResProbeCost("ConsumeRaw", ct);
    if (ResGateOn(ct)) return true;
    return GuardedOr<bool>("orig.ConsumeRaw4", false,
        [&]{ return g_origConsumeRaw4 ? g_origConsumeRaw4(thiz, ct, amount, reported, silent, method) : false; });
}

// ③-b §21.1 数据优先：SetResource(costType, amount) 是资源的唯一写入点。
//   开闸时把**写入值**替换成大值 ⇒ 底层字典余额本身就是大值，
//   不依赖任何谓词（即使商店绕过 HasEnough 直接读余额也成立）。
static void Hooked_SetResource(void* thiz, int32_t ct, int32_t amount, void* method) {
    ResProbeCost("SetResource", ct);
    if (!g_origSetResource) return;
    if (ResGateOn(ct)) {
        g_origSetResource(thiz, ct, kInfiniteAmount, method);
        return;
    }
    g_origSetResource(thiz, ct, amount, method);
}

// 兑换币消费路径（UseCoinByCovert(Int32) -> Void）：**归金币通道**（1:1）⇒ 只有
//  「无限金币」为开时才整段跳过（原来跟万能闸一起动，现已解耦）
static void Hooked_UseCoinByCovert(void* thiz, int32_t amount, void* method) {
    ResProbeCost("UseCoinByCovert", g_ctCoin.load());
    if (g_coin.load()) return;
    GuardedVoid("orig.UseCoinByCovert", [&]{ if (g_origUseCoinByCovert) g_origUseCoinByCovert(thiz, amount, method); });
}

// ============================================================================
//  6.4e 消耗漏斗（ConsumeRawInternal）+ 商店交易闸（StoreItem）—— 2026-10-11 五轮
// ============================================================================
//  为什么在 6.4 之外还要这两处（用户实测 #2「无限水晶没效果」/#3「金币没开也能买」）：
//  ① 6.4 那套（HasEnough/GetAmount/ConsumeRaw4/SetResource）只覆盖**走 CostType
//     接口**的调用点。若某个商店 / 水晶机 / 钥匙机的判定既不是 HasEnough 也不是
//     GetAmount（例如「直接消费一次看返回值」或读别的口径），那套钩子全不在路径上
//     ⇒ 表现就是「开关开了没效果」。
//  ② 而**所有**扣减最终都落到 `NEONPlayerState.ConsumeRawInternal(String key, Int32)`
//     （metadata：argc=2 token=0x060112BF，第 1 形参是资源名字符串；**无重载歧义**）
//     ⇒ 这是唯一漏斗。在漏斗上「按资源名跳过扣减」可覆盖所有消费路径，且不依赖
//     任何余额谓词（余额不足那一层由 6.4 的 HasEnough/GetAmount 钩子放行）。
//  ③ `StoreItem.CanBuy/IsFree` 是商店自己的结算门（不是成本类型门）。只在
//     「该道具 _tradingType == TradingType.Coin」且「无限金币」开时改写 ⇒ 天然 1:1；
//     挂心脏(DeductHeart)/加诅咒(AddCurse)付款的道具完全不动。
static std::atomic<uint32_t> g_hitFunnel{0};

// Il2CppString → 小写 ascii（定义在 §6.6z 全屏攻击小节；此处前向声明）
static void AttriKeyLower(void* keyObj, char* out, size_t cap);

// 资源名 → 通道号（0=手雷 1=金币 2=水晶 3=钥匙，-1=与本模块无关）
//  资源名在 NHY2 里既可能是 ascii（"Coin"/"Crystal"/"Key"/"Bomb"）也可能是中文
//  （"金币"/"水晶"/"钥匙"/"手雷"）⇒ 两种都认（中文按 UTF-16 码元判）。
static int ResChannelOfKey(void* keyObj) {
    if (!OkPtr(keyObj)) return -1;
    int32_t len = 0;
    memcpy(&len, reinterpret_cast<const char*>(keyObj) + 0x10, sizeof(len));
    if (len <= 0 || len > 256) return -1;
    const char* chars = reinterpret_cast<const char*>(keyObj) + 0x14;
    char lower[64];
    size_t n = 0;
    for (int32_t i = 0; i < len && n + 1 < sizeof(lower); ++i) {
        uint16_t c = 0;
        memcpy(&c, chars + i * 2, sizeof(c));
        lower[n++] = (c >= 'A' && c <= 'Z') ? static_cast<char>(c + 32)
                   : (c < 128 ? static_cast<char>(c) : '?');
    }
    lower[n] = '\0';
    if (strstr(lower, "crystal")) return 2;
    if (strstr(lower, "mana"))    return 2;   // ★ r15：Mana 就是界面上那个「水晶/灵力水晶」
    if (strstr(lower, "coin"))    return 1;
    if (strstr(lower, "key"))     return 3;
    if (strstr(lower, "bomb"))    return 0;
    for (int32_t i = 0; i < len; ++i) {          // 中文名（按码元单字匹配）
        uint16_t c = 0;
        memcpy(&c, chars + i * 2, sizeof(c));
        if (c == 0x6C34 /* 水 */) return 2;      // 水晶
        if (c == 0x7075 /* 灵 */) return 2;      // 灵力水晶（Mana）
        if (c == 0x91D1 /* 金 */) return 1;      // 金币
        if (c == 0x94A5 /* 钥 */) return 3;      // 钥匙
        if (c == 0x624B /* 手 */) return 0;      // 手雷
    }
    return -1;
}

static inline bool ResChannelOn(int ch) {
    switch (ch) {
        case 0: return g_bomb.load();
        case 1: return g_coin.load();
        case 2: return g_crystal.load();
        case 3: return g_key.load();
        default: return false;
    }
}

// ConsumeRawInternal(String key, Int32 amount) -> Void
typedef void (*ConsumeRawInternal_t)(void* thiz, void* key, int32_t amount, void* method);
static ConsumeRawInternal_t g_origConsumeRawInternal = nullptr;

static void Hooked_ConsumeRawInternal(void* thiz, void* key, int32_t amount, void* method) {
    const int ch = ResChannelOfKey(key);
    const uint32_t n = g_hitFunnel.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 24) {
        char lower[64];
        AttriKeyLower(key, lower, sizeof(lower));   // 非 ASCII 显示为 '?'
        LOGI(TPL_LOG " funnel probe #%u key=%s amount=%d ch=%d on=%d (消耗漏斗)",
             n, lower, amount, ch, (int)ResChannelOn(ch));
    }
    if (ResChannelOn(ch)) return;                   // 该资源开关开 ⇒ 只记账不扣
    GuardedVoid("orig.ConsumeRawInternal", [&]{ if (g_origConsumeRawInternal) g_origConsumeRawInternal(thiz, key, amount, method); });
}

// static IsCostTypeSupported(CostType) -> Boolean（「该成本类型在本局是否可用」）
typedef bool (*IsCostTypeSupported_t)(int32_t ct, void* method);
static IsCostTypeSupported_t g_origIsCostTypeSupported = nullptr;

static bool Hooked_IsCostTypeSupported(int32_t ct, void* method) {
    const bool r = GuardedOr<bool>("orig.IsCostTypeSupported", false, [&]{ return g_origIsCostTypeSupported ? g_origIsCostTypeSupported(ct, method) : false; });
    ResProbeCost("IsCostTypeSupported", ct);
    if (ResGateOn(ct)) return true;
    return r;
}

// ================= ★ r14 无限水晶＝开门（用户 m05869：「水晶现在的作用就是开门，
//  你没找到这个吗？有一个门需要水晶开」）=================
//  离线元数据实证：`NEON.Game.Managers.NEONPlayerState`（TYPE index=10255）里有
//  「解锁成本」三连方法（dump 行 133236-133238，紧邻 RevokeAttrs/RefreshTopLeftUi）：
//    CanUnlock(CostType, Int32) -> Boolean        token=0x06011390
//    CanFreeUnlock(CostType)    -> Boolean        token=0x06011391
//    UnlockCosts(CostType, Int32, Boolean) -> Void token=0x06011392
//  它们**不属于** HasEnough / GetAmount / ConsumeRaw / IsCostTypeSupported 任何一族
//  ⇒ r11-r13 的那套资源闸门天然漏掉「开门付水晶」这条路径（这也解释了真机整局
//  costType=2(Crystal) 一次都没出现：门走的不是那三族）。
//  1:1 保真：只有当第一实参**看起来是合法 CostType**（按 w 寄存器语义取低 32 位，0..63）
//  且该通道开关为开时才放行/抑制；指针/对象地址绝不可能落在这个区间 ⇒ 不会顺手放行
//  别的成本类型（钥匙/金币/手雷各自管各自的开关）。
static std::atomic<uint32_t> g_hitUnlock{0};
typedef bool (*CanUnlock_t)(void* thiz, intptr_t p1, intptr_t p2, void* method);
typedef bool (*CanFreeUnlock_t)(void* thiz, intptr_t p1, void* method);
typedef void (*UnlockCosts_t)(void* thiz, intptr_t p1, intptr_t p2, intptr_t p3, void* method);
static CanUnlock_t     g_origCanUnlock     = nullptr;
static CanFreeUnlock_t g_origCanFreeUnlock = nullptr;
static UnlockCosts_t   g_origUnlockCosts   = nullptr;

// 形参是否「像 CostType」：枚举按 int32 传参（w 寄存器），故只看低 32 位。
static inline bool UnlockCostTypeLike(intptr_t p1, int32_t* outCt) {
    const uint32_t raw = (uint32_t)(uintptr_t)p1;
    if (raw > 63u) return false;                        // CostType 全表 0..36
    *outCt = (int32_t)raw;
    return true;
}

// CanUnlock(CostType, Int32) -> Boolean：开门判定
static bool Hooked_CanUnlock(void* thiz, intptr_t p1, intptr_t p2, void* method) {
    const bool r = GuardedOr<bool>("orig.CanUnlock", false, [&]{ return g_origCanUnlock ? g_origCanUnlock(thiz, p1, p2, method) : false; });
    int32_t ct = -1;
    const bool like = UnlockCostTypeLike(p1, &ct);
    if (like) ResProbeCost("CanUnlock", ct);
    const uint32_t n = g_hitUnlock.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 16) {
        LOGI(TPL_LOG " unlock probe #%u CanUnlock p1=%u p2=%u 像CostType=%d orig=%d"
                     "（开门付资源：0=手雷 1=金币 2=水晶 5=钥匙）",
             n, (unsigned)(uint32_t)(uintptr_t)p1, (unsigned)(uint32_t)(uintptr_t)p2,
             (int)like, (int)r);
    }
    if (like && ResGateOn(ct)) {
        if (n <= 16) LOGI(TPL_LOG " unlock gate #%u CanUnlock ct=%d -> true（无限资源：开门判定放行）", n, ct);
        return true;
    }
    return r;
}

// CanFreeUnlock(CostType) -> Boolean：免费开门判定
static bool Hooked_CanFreeUnlock(void* thiz, intptr_t p1, void* method) {
    const bool r = GuardedOr<bool>("orig.CanFreeUnlock", false, [&]{ return g_origCanFreeUnlock ? g_origCanFreeUnlock(thiz, p1, method) : false; });
    int32_t ct = -1;
    const bool like = UnlockCostTypeLike(p1, &ct);
    if (like) ResProbeCost("CanFreeUnlock", ct);
    const uint32_t n = g_hitUnlock.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 16) {
        LOGI(TPL_LOG " unlock probe #%u CanFreeUnlock p1=%u 像CostType=%d orig=%d",
             n, (unsigned)(uint32_t)(uintptr_t)p1, (int)like, (int)r);
    }
    if (like && ResGateOn(ct)) return true;
    return r;
}

// UnlockCosts(CostType, Int32, Boolean) -> Void：真正扣资源的那一下
static void Hooked_UnlockCosts(void* thiz, intptr_t p1, intptr_t p2, intptr_t p3, void* method) {
    int32_t ct = -1;
    const bool like = UnlockCostTypeLike(p1, &ct);
    if (like) ResProbeCost("UnlockCosts", ct);
    const uint32_t n = g_hitUnlock.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 16) {
        LOGI(TPL_LOG " unlock probe #%u UnlockCosts p1=%u p2=%u p3=%u 像CostType=%d"
                     "（开门扣资源：0=手雷 1=金币 2=水晶 5=钥匙）",
             n, (unsigned)(uint32_t)(uintptr_t)p1, (unsigned)(uint32_t)(uintptr_t)p2,
             (unsigned)(uint32_t)(uintptr_t)p3, (int)like);
    }
    if (like && ResGateOn(ct)) {
        if (n <= 16) LOGI(TPL_LOG " unlock gate #%u UnlockCosts ct=%d 已抑制（无限资源：不扣）", n, ct);
        return;                                          // 不调原函数 ⇒ 不扣资源
    }
    GuardedVoid("orig.UnlockCosts", [&]{ if (g_origUnlockCosts) g_origUnlockCosts(thiz, p1, p2, p3, method); });
}

// ---- 商店交易闸 ----
static inline int32_t StoreItemTradingType(void* thiz) {
    const size_t off = g_offStoreTradingType.load();
    if (!off || !OkPtr(thiz)) return -1;
    int32_t v = -1;
    memcpy(&v, reinterpret_cast<const char*>(thiz) + off, sizeof(v));
    return v;
}

static inline int StoreItemFlag(void* thiz, size_t off) {
    if (!off || !OkPtr(thiz)) return -1;
    uint8_t v = 0;
    memcpy(&v, reinterpret_cast<const char*>(thiz) + off, 1);
    return v;
}

// 只有「金币付款的道具 + 无限金币开」才改写（1:1）
static inline bool ShopCoinGate(void* thiz) {
    const int32_t coin = g_ttCoin.load();
    if (coin < 0 || !g_coin.load()) return false;
    return StoreItemTradingType(thiz) == coin;
}

static bool (*g_origStoreCanBuy)(void* thiz, void* method) = nullptr;
static bool (*g_origStoreIsFree)(void* thiz, void* method) = nullptr;

static bool Hooked_StoreItemCanBuy(void* thiz, void* method) {
    const bool r = GuardedOr<bool>("orig.StoreItem.CanBuy", false, [&]{ return g_origStoreCanBuy ? g_origStoreCanBuy(thiz, method) : false; });
    if (!ShopCoinGate(thiz)) return r;
    const uint32_t n = g_hitShop.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 12) {
        LOGI(TPL_LOG " shop probe #%u CanBuy real=%d tt=%d canPur=%d -> 1 (无限金币：商店结算放行)",
             n, (int)r, StoreItemTradingType(thiz), StoreItemFlag(thiz, g_offStoreCanPurChase.load()));
    }
    return true;
}

static bool Hooked_StoreItemIsFree(void* thiz, void* method) {
    const bool r = GuardedOr<bool>("orig.StoreItem.IsFree", false, [&]{ return g_origStoreIsFree ? g_origStoreIsFree(thiz, method) : false; });
    if (!ShopCoinGate(thiz)) return r;
    const uint32_t n = g_hitShop.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 12) {
        LOGI(TPL_LOG " shop probe #%u IsFree real=%d tt=%d -> 1 (无限金币：该道具按免费结算)",
             n, (int)r, StoreItemTradingType(thiz));
    }
    return true;
}

// ============================================================================
//  6.6 移动 / 跳跃（菜单归入 Category_战斗，2026-10-10 二次定稿）
// ============================================================================
//  移速：PlayerActor.get_MovementSpeed() -> Single 是 Mover.Init(deltaAcc,maxSpeed)
//  的速度来源 —— 在这里单点乘算。Mover.get_FinalMaxSpeed() **只做探针不缩放**
//  （两处同时缩放 ⇒ 速度平方，与伤害倍率同一个坑）。
static float Hooked_GetMovementSpeed(void* thiz, void* method) {
    const float r = GuardedOr<float>("orig.get_MovementSpeed", 0.f, [&]{ return g_origGetMovementSpeed ? g_origGetMovementSpeed(thiz, method) : 0.f; });
    const int mul = g_spdMul.load(std::memory_order_relaxed);
    if (mul <= 1) return r;   // 滑块值即开关（§③.1）：1 = 关
    const uint32_t n = g_hitSpd.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 10) LOGI(TPL_LOG " spd probe #%u MovementSpeed %.3f -> %.3f (x%d)",
                      n, r, r * static_cast<float>(mul), mul);
    return r * static_cast<float>(mul);
}

static float Hooked_GetFinalMaxSpeed(void* thiz, void* method) {
    const float r = GuardedOr<float>("orig.get_FinalMaxSpeed", 0.f, [&]{ return g_origGetFinalMaxSpeed ? g_origGetFinalMaxSpeed(thiz, method) : 0.f; });
    const uint32_t n = g_hitSpd.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 10) LOGI(TPL_LOG " spd probe #%u FinalMaxSpeed=%.3f (仅探针，不缩放)", n, r);
    return r;
}

//  无限跳：跳数上限。Final 与基础两个 getter 都取「不小于 99」⇒ 整数上限不会成幂。
static int32_t Hooked_GetFinalJumpLimit(void* thiz, void* method) {
    const int32_t r = GuardedOr<int32_t>("orig.get_FinalMultipleJumpCountLimit", 0, [&]{ return g_origGetFinalJumpLimit ? g_origGetFinalJumpLimit(thiz, method) : 0; });
    if (!g_jump.load()) return r;
    const uint32_t n = g_hitJump.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 5) LOGI(TPL_LOG " jump probe #%u FinalMultipleJumpCountLimit %d -> 99", n, r);
    return r > 99 ? r : 99;
}

static int32_t Hooked_GetJumpLimit(void* thiz, void* method) {
    const int32_t r = GuardedOr<int32_t>("orig.get_MultipleJumpCountLimit", 0, [&]{ return g_origGetJumpLimit ? g_origGetJumpLimit(thiz, method) : 0; });
    if (!g_jump.load()) return r;
    const uint32_t n = g_hitJump.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 5) LOGI(TPL_LOG " jump probe #%u MultipleJumpCountLimit %d -> 99", n, r);
    return r > 99 ? r : 99;
}

// ---- 6.3b 冷却计时字段的写入点 / 读取点 —— **随「技能无CD」一并删除**
//  （★ 2026-10-11 五轮定稿；原挂点：SecSkillPowerUp.get_CoolingTime/0 与
//    set_CoolingTime/1。它们只服务于「技能无CD」这一个功能，无其它功能复用。）

// ---- 6.5 标准版（18 元内购）闸门：全部是 static 谓词 ⇒ 恒真/Allowed ----
//  调用点：NEON.Framework.Login.StandardEdition*Access（全 static）。
//  参数一律用 intptr_t 透传（不关心具体值，只在关闭时原样转发给 orig）。
//  §28 例外说明：这里的谓词就是「付费门禁」本身，恒真是本功能唯一正确语义
//  （与「输入门禁恒真 ⇒ 自动连发」不同，付费门禁恒真不会产生连发副作用）。
//
//  ---- 6.5.0 hook 自递归熔断（★ 2026-10-08 真机实测崩溃对策） ----
//  现象：tombstone `512 total frames`，`#06..#511 = libstarry_magic.so+0x175d4`
//        重复 506 次，符号化后 = Hooked_Std_HasContentOverride
//        @ Games/nhsy2Mod.cpp:747；#05 = Hooked_Std_AppliesToCurrentPlatform。
//        即 Dobby 对「函数体极短 / 首指令为分支」的 getter 生成的 trampoline
//        回跳落回了函数入口（入口已被 Dobby 改写成跳向 hook）⇒
//        hook → orig(trampoline) → hook 无限循环，栈耗尽后 SIGSEGV。
//  对策：每个 hook 一个本线程深度计数。深度超限时记一次警告并直接返回本槽的
//        「功能开启值」，把致命崩溃降级为一次可观测事件（健康路径深度恒为 1
//        ⇒ 对正常游戏与正常槽位零语义影响）。
static const int kHookDepthMax = 8;
static thread_local int t_stdDepth = 0;

//  返回 true = 允许进入 hook 本体；false = 已处于病态深递归，调用方立即返回默认值
static bool StdDepthEnter(const char* tag) {
    if (t_stdDepth < kHookDepthMax) {
        ++t_stdDepth;
        return true;
    }
    static std::atomic<uint32_t> once{0};
    if (once.fetch_add(1, std::memory_order_relaxed) < 6U) {
        LOGW(TPL_LOG " %s SELF-RECURSION depth>%d — Dobby trampoline 回跳异常，"
                     "本轮不再调用 orig，直接返回功能开启值",
             tag, kHookDepthMax);
    }
    return false;
}

static void StdDepthLeave() {
    if (t_stdDepth > 0) --t_stdDepth;
}

#define STD_GATE0(NAME)                                                      \
    static bool (*g_orig_##NAME)(void*) = nullptr;                           \
    static bool Hooked_##NAME(void* method) {                                \
        if (!StdDepthEnter(#NAME)) return true;                              \
        const bool r = GuardedOr<bool>(#NAME, false, [&]{ return g_orig_##NAME ? g_orig_##NAME(method) : false; }); \
        StdDepthLeave();                                                     \
        if (!g_std.load()) return r;                                         \
        g_hitStd.fetch_add(1, std::memory_order_relaxed);                    \
        return true;                                                         \
    }
#define STD_GATE1(NAME)                                                      \
    static bool (*g_orig_##NAME)(intptr_t, void*) = nullptr;                 \
    static bool Hooked_##NAME(intptr_t a0, void* method) {                   \
        if (!StdDepthEnter(#NAME)) return true;                              \
        const bool r = GuardedOr<bool>(#NAME, false, [&]{ return g_orig_##NAME ? g_orig_##NAME(a0, method) : false; }); \
        StdDepthLeave();                                                     \
        if (!g_std.load()) return r;                                         \
        g_hitStd.fetch_add(1, std::memory_order_relaxed);                    \
        return true;                                                         \
    }
#define STD_GATE2(NAME)                                                      \
    static bool (*g_orig_##NAME)(intptr_t, intptr_t, void*) = nullptr;       \
    static bool Hooked_##NAME(intptr_t a0, intptr_t a1, void* method) {      \
        if (!StdDepthEnter(#NAME)) return true;                              \
        const bool r = GuardedOr<bool>(#NAME, false, [&]{ return g_orig_##NAME ? g_orig_##NAME(a0, a1, method) : false; }); \
        StdDepthLeave();                                                     \
        if (!g_std.load()) return r;                                         \
        g_hitStd.fetch_add(1, std::memory_order_relaxed);                    \
        return true;                                                         \
    }
#define STD_GATE3(NAME)                                                      \
    static bool (*g_orig_##NAME)(intptr_t, intptr_t, intptr_t, void*) = nullptr; \
    static bool Hooked_##NAME(intptr_t a0, intptr_t a1, intptr_t a2, void* method) { \
        if (!StdDepthEnter(#NAME)) return true;                              \
        const bool r = GuardedOr<bool>(#NAME, false, [&]{ return g_orig_##NAME ? g_orig_##NAME(a0, a1, a2, method) : false; }); \
        StdDepthLeave();                                                     \
        if (!g_std.load()) return r;                                         \
        g_hitStd.fetch_add(1, std::memory_order_relaxed);                    \
        return true;                                                         \
    }
#define STD_EVAL0(NAME)                                                      \
    static int32_t (*g_orig_##NAME)(void*) = nullptr;                        \
    static int32_t Hooked_##NAME(void* method) {                             \
        if (!StdDepthEnter(#NAME)) return g_allowedEnum.load();              \
        const int32_t r = GuardedOr<int32_t>(#NAME, 0, [&]{ return g_orig_##NAME ? g_orig_##NAME(method) : 0; }); \
        StdDepthLeave();                                                     \
        if (!g_std.load()) return r;                                         \
        g_hitStd.fetch_add(1, std::memory_order_relaxed);                    \
        return g_allowedEnum.load();                                         \
    }
#define STD_EVAL1(NAME)                                                      \
    static int32_t (*g_orig_##NAME)(intptr_t, void*) = nullptr;              \
    static int32_t Hooked_##NAME(intptr_t a0, void* method) {                \
        if (!StdDepthEnter(#NAME)) return g_allowedEnum.load();              \
        const int32_t r = GuardedOr<int32_t>(#NAME, 0, [&]{ return g_orig_##NAME ? g_orig_##NAME(a0, method) : 0; }); \
        StdDepthLeave();                                                     \
        if (!g_std.load()) return r;                                         \
        g_hitStd.fetch_add(1, std::memory_order_relaxed);                    \
        return g_allowedEnum.load();                                         \
    }
#define STD_EVAL2(NAME)                                                      \
    static int32_t (*g_orig_##NAME)(intptr_t, intptr_t, void*) = nullptr;    \
    static int32_t Hooked_##NAME(intptr_t a0, intptr_t a1, void* method) {   \
        if (!StdDepthEnter(#NAME)) return g_allowedEnum.load();              \
        const int32_t r = GuardedOr<int32_t>(#NAME, 0, [&]{ return g_orig_##NAME ? g_orig_##NAME(a0, a1, method) : 0; }); \
        StdDepthLeave();                                                     \
        if (!g_std.load()) return r;                                         \
        g_hitStd.fetch_add(1, std::memory_order_relaxed);                    \
        return g_allowedEnum.load();                                         \
    }
#define STD_EVAL4(NAME)                                                      \
    static int32_t (*g_orig_##NAME)(intptr_t, intptr_t, intptr_t, intptr_t, void*) = nullptr; \
    static int32_t Hooked_##NAME(intptr_t a0, intptr_t a1, intptr_t a2, intptr_t a3, void* method) { \
        if (!StdDepthEnter(#NAME)) return g_allowedEnum.load();              \
        const int32_t r = GuardedOr<int32_t>(#NAME, 0, [&]{ return g_orig_##NAME ? g_orig_##NAME(a0, a1, a2, a3, method) : 0; }); \
        StdDepthLeave();                                                     \
        if (!g_std.load()) return r;                                         \
        g_hitStd.fetch_add(1, std::memory_order_relaxed);                    \
        return g_allowedEnum.load();                                         \
    }

// StandardEditionClientAccess（3560）— 总闸
STD_GATE1(Std_CanUse)                       // CanUse(feature) -> bool      ★总闸
STD_EVAL2(Std_ClientEvaluate)               // Evaluate(feature, original) -> AccessResult
STD_EVAL0(Std_ClientEvalDiffEditing)        // EvaluateDifficultyEditing() -> AccessResult
STD_GATE0(Std_CanAdvanceBossProgress)       // get_CanAdvanceBossProgress() -> bool
STD_GATE1(Std_IsFreeBoss)                   // IsFreeBoss(boss) -> bool
STD_GATE1(Std_CanViewBoss)                  // CanViewBoss(boss) -> bool
STD_GATE3(Std_ClientResolveRunAccess)       // ResolveRunAccess(continuing, saved, current) -> bool

// StandardEditionAccessPolicy（3557）
STD_GATE0(Std_AppliesToCurrentPlatform)     // get_AppliesToCurrentPlatform() -> bool
STD_GATE0(Std_HasContentOverride)           // get_HasContentOverride() -> bool
STD_GATE3(Std_IsRestrictedEvolution)        // IsRestrictedEvolution(page, contentId, isExtraction) -> bool
STD_EVAL4(Std_PolicyEvaluatePurchase)       // EvaluatePurchase(purchase, orig, snapshot, applies) -> AccessResult

// StandardEditionAssaultAccess（3558）
STD_GATE1(Std_IsFreeLevel)                  // IsFreeLevel(levelKey) -> bool
STD_GATE2(Std_CanUseRunLevel)               // CanUseRunLevel(levelKey, paidAccessAllowed) -> bool
STD_EVAL2(Std_AssaultEvaluate)              // Evaluate(levelKey, original) -> AccessResult

// StandardEditionBanAccess（3559）
STD_GATE0(Std_BanCanUseSessionRules)        // get_CanUseSessionRules() -> bool
STD_GATE0(Std_BanCanUseCurrentRules)        // get_CanUseCurrentRules() -> bool
STD_GATE0(Std_BanHasOriginalAccess)         // get_HasOriginalAccess() -> bool
STD_GATE0(Std_BanIsPersonalOwner)           // get_IsPersonalOwner() -> bool
STD_EVAL1(Std_BanEvaluate)                  // Evaluate(original) -> AccessResult

// StandardEditionPatchAccess（3563）
STD_GATE0(Std_PatchCanProvisionEnergy)      // get_CanProvisionEnergy() -> bool
STD_GATE0(Std_PatchCanUseCurrentRun)        // get_CanUseCurrentRun() -> bool
STD_GATE0(Std_PatchCanProgressCurrentRun)   // get_CanProgressCurrentRun() -> bool
STD_GATE0(Std_PatchHasOriginalProgress)     // get_HasOriginalProgress() -> bool
STD_EVAL1(Std_PatchEvaluate)                // Evaluate(original) -> AccessResult

// LoginService（3551）
STD_EVAL1(Std_LoginEvaluateCurrentPurchase) // EvaluateCurrentPurchase(purchase) -> AccessResult

// ============================================================================
//  6.5 登录保命（login assist）：交互登录超时不再一刀切掉整次登录
// ============================================================================
//  ★ 真机取证（2026-10-11，记录仪 09_test/_recorder/20261011_014328/logcat.txt）：
//    · dex 补丁把 TapTap 登录强制走**内嵌网页**（改包后客户端 SSO 走不通），
//      于是登录必须由人**在网页里手动输入**（实证：登录页 01:44:48 打开，
//      01:45:00–01:45:20 输入法进程 11936（搜狗）持续收触摸＝用户在打字，
//      01:45:25.206 网页回调 onSuccess 才回来）。
//    · ⇒ ~35 s 预算变成「和人手速抢时间」：轮次1 打字 28.3 s → 成功；
//      轮次2 打字 37.3 s → 游戏先判超时（只差 0.21 s：onSuccess 25.206 vs 超时 25.419）。
//    · 超时链路：`[TapSDK] LoginWithScopes timed out …`（metadata 字面量 S 24060）
//      → `LoginService: Login Failed - code=5040` → `HandleLoginFailure(5040)` → 弹框
//      → 用户点确定 → `CloudBaseService: Logged out and tokens cleared`（登录态被拆）。
//  治本思路：把「超时」从**致命**降级为**可忽略**——5040 不再走原流程，等 SDK 把网页
//      授权码换成 token 后自己回调 HandleLoginSuccess（轮次2 的授权码在超时前 0.21 s
//      就已到，token 交换当时已在对飞，等它落地即可进游戏）。
//  护栏（防呆 + 防真故障）：① 只有「最近 180 s 内确实起过交互登录」才兜（起点由
//      StartInteractiveLoginInternal 置位）⇒ 与交互登录无关、网络真挂导致的 5040 照旧弹框；
//      ② 每次兜底间隔 ≥60 s、一个进程最多 3 次 ⇒ 不会无限吞掉失败；
//      ③ 一旦 HandleLoginSuccess 进来立即永久收工，此后完全透明。
//  ★ 这三个 hook **与菜单开关无关**（登录发生在用户打开菜单之前）⇒ 走独立安装路径
//    InstallLoginAssistHooks()，不受 AnyFeatureOn / g_installPassDone 门禁影响。
//    不改 dex、不改 Java、不动包内其它文件。
static std::atomic<bool>      g_loginAssist{true};         // 收到成功即永久关闭
static std::atomic<int>       g_loginGraceUsed{0};         // 已兜底次数
static std::atomic<int>       g_loginHitSuccess{0};
static std::atomic<long long> g_loginInteractiveAtMs{0};   // 交互登录起点（0=本会话还没起过）
static std::atomic<long long> g_loginLastGraceMs{0};       // 上次兜底时刻
static std::atomic<bool>      g_loginThreadAttached{false};
static long long              g_loginGraceStampLogged = 0;  // 观察窗去重（仅 Tick 线程写）

enum { NHSY2_LOGIN_ERR_TIMEOUT    = 5040,
       NHSY2_LOGIN_ARM_WINDOW_MS  = 180000,
       NHSY2_LOGIN_GRACE_GAP_MS   =  60000,
       NHSY2_LOGIN_GRACE_MAX      =      3 };

static long long NowMonoMs() {
    return static_cast<long long>(
        std::chrono::duration_cast<std::chrono::milliseconds>(
            std::chrono::steady_clock::now().time_since_epoch()).count());
}

// 6.5.1 LoginService::StartInteractiveLoginInternal() -> Void（静态 argc=0）
//   token 0x0600736B。只做「交互登录起点」标记 + 证据日志，不拦不改。
static void (*g_origLoginStartInteractive)(void*) = nullptr;
static void Hooked_LoginStartInteractive(void* method) {
    g_loginInteractiveAtMs.store(NowMonoMs());
    LOGI(TPL_LOG " login: 交互登录开始（内嵌网页页，需用户手动输入，游戏侧预算约 35s）");
    GuardedVoid("orig.Login.StartInteractive", [&]{ if (g_origLoginStartInteractive) g_origLoginStartInteractive(method); });
}

// 6.5.2 LoginService::HandleLoginFailure(Int32 errorCode, String message) -> Void（实例 argc=2）
//   token 0x06007370。只在 5040 + 护栏内抑制（不调用 orig）；其余一律原样放行。
static void (*g_origLoginFailure)(void*, int32_t, void*, void*) = nullptr;
static void Hooked_LoginFailure(void* thiz, int32_t code, void* message, void* method) {
    const long long now   = NowMonoMs();
    const long long armed = g_loginInteractiveAtMs.load();
    const bool recent = (armed != 0) &&
                        (now - armed) < static_cast<long long>(NHSY2_LOGIN_ARM_WINDOW_MS);
    const bool gapOk  = (now - g_loginLastGraceMs.load()) >=
                        static_cast<long long>(NHSY2_LOGIN_GRACE_GAP_MS);
    const int  used   = g_loginGraceUsed.load();

    if (code == NHSY2_LOGIN_ERR_TIMEOUT && g_loginAssist.load() &&
        recent && gapOk && used < NHSY2_LOGIN_GRACE_MAX) {
        g_loginLastGraceMs.store(now);
        const int n = g_loginGraceUsed.fetch_add(1) + 1;
        LOGW(TPL_LOG " login assist: 兜住交互登录超时 code=%d（第 %d/%d 次，距交互起点 %lld ms）"
                     " —— 抑制失败回调：不弹框、不拆登录态，等网页授权码换 token 后自己回调成功",
             code, n, static_cast<int>(NHSY2_LOGIN_GRACE_MAX), now - armed);
        return;   // ★ 故意不调用 orig
    }
    LOGW(TPL_LOG " login failure 原样放行 code=%d (assist=%d 交互起点=%lld 距今=%lld recent=%d gapOk=%d used=%d/%d)",
         code, static_cast<int>(g_loginAssist.load()), armed,
         armed ? (now - armed) : -1LL, static_cast<int>(recent), static_cast<int>(gapOk),
         used, static_cast<int>(NHSY2_LOGIN_GRACE_MAX));
    GuardedVoid("orig.Login.OnFailure", [&]{ if (g_origLoginFailure) g_origLoginFailure(thiz, code, message, method); });
}

// 6.5.3 LoginService::HandleLoginSuccess(LoginResult) -> Void（实例 argc=1）
//   token 0x0600736D。成功即收工：此后完全透明。
static void (*g_origLoginSuccess)(void*, void*, void*) = nullptr;
static void Hooked_LoginSuccess(void* thiz, void* result, void* method) {
    g_loginHitSuccess.fetch_add(1);
    if (g_loginAssist.exchange(false)) {
        LOGI(TPL_LOG " login assist: 登录成功 → 保命收工（累计兜住 %d 次超时，本次会话不再干预）",
             static_cast<int>(g_loginGraceUsed.load()));
    }
    GuardedVoid("orig.Login.OnSuccess", [&]{ if (g_origLoginSuccess) g_origLoginSuccess(thiz, result, method); });
}

// 6.5.3b 登录预算延长：窗口内的 Task.Delay(~30s) → 300s（治本，治的是「手速」而不是「弹框」）
//  实测依据（09_test/_recorder/20261011_014328/logcat.txt + 02:11 轮次日志）：
//    · 轮次3：02:11:32.029 点击 → 用户打字（23 次 dispatchTouchView / IME 显示）→
//      02:12:01.575 **网页授权成功**（TapTapLoginActivity$fragmentProvider$1.onSuccess）
//      → 02:12:02.056（+30.026s）游戏侧仍判超时。赢了 0.48s 的差，输在预算上。
//    · 轮次1：37.5s 超时；轮次2：用户没打字，登录页干等 3m20s（无超时）⇒ 那条等待
//      只在「交互登录协程在跑」时存在，长度 30~37s ⇒ 就是交互登录的等待预算。
//  ⇒ 把窗口内的这个等待拉长到 300s：用户从容打完，回调一到即成功（无需再点第二次）。
//  双编码识别（两种重载的实参都只占 1 个寄存器，量级可区分）：
//    Int32 毫秒       29000..31000        → 300000
//    TimeSpan ticks   290000000..310000000（=29..31s，1 tick = 100ns）→ 3000000000
//  作用域三重锁：① g_loginAssist（成功即永久关）② 交互起点后 180s 窗口内
//                ③ 仅命中 29~31s 这一个量级，其它任何 Delay 原样放行。
//  ★装机实测（02:22 冷启，本轮同版 .so）：`login assist: 3 个登录挂点全部就位`、
//    `预算延长槽 2/2（Task.Delay 重载）`、`LoginService.PostLoginTimeoutSeconds=40.000`
//    ⇒ Task.Delay(argc=1) 与 (argc=2) **都挂上了**；而字段值 40 s 与观测到的 ~30 s/37.5 s
//    不一致 ⇒ 判定改写区间不能只押 29~31 s，放宽到 **15~120 s**，并加「长等待观测」日志
//    （未改写也打印原值）以便一次真机轮就能定位真正的重载与数值。
enum { NHSY2_LOGIN_DELAY_EXTEND_MS = 300000 };
static std::atomic<int>  g_loginDelayExtended{0};
static std::atomic<int>  g_loginDelaySeen{0};
static std::atomic<bool> g_loginDelaySlotTried{false};
static void* (*g_origTaskDelay1)(intptr_t, void*) = nullptr;           // Task.Delay(?)
static void* (*g_origTaskDelay2)(intptr_t, void*, void*) = nullptr;    // Task.Delay(?, CancellationToken)
// r20：`g_origCtsCancelAfter` / `Hooked_CtsCancelAfter` 已随槽位一并摘除（理由见 kLoginOptionalSlots 注释）

static bool LoginAssistWindowOpen() {
    const long long armed = g_loginInteractiveAtMs.load();
    if (armed == 0) return false;
    return (NowMonoMs() - armed) < static_cast<long long>(NHSY2_LOGIN_ARM_WINDOW_MS);
}

// 只记日志：窗口内出现的「长等待」原值（≥3 s、≤10 min），最多 10 行。
//   用途 = 万一改写没命中，日志能直接告诉我们真正的等待值/重载，不必再猜。
static void NoteDelaySeen(const char* which, long long in, bool ticks) {
    const long long ms = ticks ? (in / 10000LL) : in;
    if (ms < 3000LL || ms > 600000LL) return;
    const int n = g_loginDelaySeen.fetch_add(1) + 1;
    if (n <= 10) {
        LOGI(TPL_LOG " login assist: 观测到长等待 %s = %lld%s（≈%lld ms，未改写）",
             which, in, ticks ? " (ticks)" : " (ms)", ms);
    }
}

static intptr_t ExtendLoginDelay(intptr_t v, const char* which) {
    const long long in = static_cast<long long>(v);
    long long out = in;
    bool ticks = false;
    // 毫秒重载：15 s ~ 120 s（覆盖 30 s / 37.5 s / 40 s 等观测值）
    if (in >= 15000LL && in <= 120000LL) {
        out = NHSY2_LOGIN_DELAY_EXTEND_MS;
    // TimeSpan(tick=100ns) 重载：15 s ~ 120 s ⇒ 150,000,000 ~ 1,200,000,000
    } else if (in >= 150000000LL && in <= 1200000000LL) {
        out = static_cast<long long>(NHSY2_LOGIN_DELAY_EXTEND_MS) * 10000LL;
        ticks = true;
    } else if (in >= 10000000LL && in <= 1200000000LL) {
        NoteDelaySeen(which, in, true);      // 1 s ~ 120 s 的 ticks 值（诊断）
        return v;
    } else {
        NoteDelaySeen(which, in, false);     // 毫秒（诊断）
        return v;
    }
    if (out == in) return v;
    const int n = g_loginDelayExtended.fetch_add(1) + 1;
    if (n <= 6) {
        LOGW(TPL_LOG " login assist: %s 入参 %lld%s -> %lld（延长交互登录等待，仅窗口内生效）",
             which, in, ticks ? " (ticks)" : " (ms)", out);
    }
    return static_cast<intptr_t>(out);
}

static void* Hooked_TaskDelay1(intptr_t v, void* method) {
    if (g_loginAssist.load() && LoginAssistWindowOpen()) v = ExtendLoginDelay(v, "Task.Delay(argc=1)");
    return GuardedOr<void*>("orig.Task.Delay1", nullptr, [&]{ return g_origTaskDelay1 ? g_origTaskDelay1(v, method) : nullptr; });
}
static void* Hooked_TaskDelay2(intptr_t v, void* ct, void* method) {
    if (g_loginAssist.load() && LoginAssistWindowOpen()) v = ExtendLoginDelay(v, "Task.Delay(argc=2)");
    return GuardedOr<void*>("orig.Task.Delay2", nullptr, [&]{ return g_origTaskDelay2 ? g_origTaskDelay2(v, ct, method) : nullptr; });
}
// r20（2026-10-15 底包 vc603）：原 `Hooked_CtsCancelAfter` 已摘除。它把实例方法的
// `this` 当毫秒数交给 `ExtendLoginDelay`（量级闸必然判否 ⇒ 从来没延长过任何等待），
// 却给 BCL 的 `System.Threading.CancellationTokenSource.CancelAfter` 打了一个 Dobby
// 16 字节入口补丁；本轮实测游戏恰好在登录等待窗口内干净退出 exit(0)，故按「零收益高
// 风险」摘掉。若日后确认超时确实走 TokenSource，应改 x1（实参）而不是 x0（this）。

// 6.5.4 独立槽表 + 独立安装（幂等，未就位就每 tick 重试）
static const HookSlot kLoginSlots[] = {
    {"Login_StartInteract", 0, 0, "NEON.Framework.Login", "LoginService",
     "StartInteractiveLoginInternal", 0,
     (void*)Hooked_LoginStartInteractive, (void**)&g_origLoginStartInteractive},
    {"Login_FailAssist",    0, 0, "NEON.Framework.Login", "LoginService",
     "HandleLoginFailure", 2,
     (void*)Hooked_LoginFailure, (void**)&g_origLoginFailure},
    {"Login_SuccessDisarm", 0, 0, "NEON.Framework.Login", "LoginService",
     "HandleLoginSuccess", 1,
     (void*)Hooked_LoginSuccess, (void**)&g_origLoginSuccess},
};
static const int kLoginSlotsCount = (int)(sizeof(kLoginSlots) / sizeof(kLoginSlots[0]));

// 可选的预算延长槽（系统类，两个重载最多中一个；缺失不影响核心三钩）
// ★r20（2026-10-15 底包 vc603）摘除 `Login_CancelExt`，理由两条：
//   ① **零收益**：`CancellationTokenSource.CancelAfter` 是**实例方法** ⇒ 钩子拿到的 x0 是
//      `this` 指针（非毫秒数），`ExtendLoginDelay` 的量级闸（15s~120s 毫秒 / 1.5e8~1.2e9 ticks）
//      必然判否并原样放行 ⇒ 该槽从来没有真正延长过任何等待（只多打一行安装日志）。
//   ② **纯风险**：它给 BCL（System.Threading）方法打了一个 Dobby 16 字节入口补丁，而本轮
//      实测「游戏在整个登录等待窗口里干净退出 exit(0)、窗口一过就没了」—— boot1（无任何钩子）
//      同一窗口正常走到 +7.5s 的 `TapTapSDK.Init done`。摘掉它是唯一零代价的排除项。
//   要真正延长 TokenSource 预算应改 `CancelAfter` 的 **x1**（毫秒/TimeSpan 实参），且需
//   真机先确认游戏确实用该路径超时（r17 实测走的是 Task.Delay 路径），故本轮不重实现。
static const HookSlot kLoginOptionalSlots[] = {
    {"Login_DelayExt1", 0, 0, "System.Threading.Tasks", "Task", "Delay", 1,
     (void*)Hooked_TaskDelay1, (void**)&g_origTaskDelay1},
    {"Login_DelayExt2", 0, 0, "System.Threading.Tasks", "Task", "Delay", 2,
     (void*)Hooked_TaskDelay2, (void**)&g_origTaskDelay2},
};
static const int kLoginOptionalCount = (int)(sizeof(kLoginOptionalSlots) / sizeof(kLoginOptionalSlots[0]));
static std::atomic<bool> g_loginHooksDone{false};

static void InstallLoginAssistHooks() {
    if (g_loginHooksDone.load()) return;                 // 快路径：已就位
    if (!g_libReady.load()) return;                      // 库未就绪：下一 tick 再试
    if (!EnsureApi()) return;

    // ★r20 诊断 L≤3：只到「API 可用（含自检调用）」为止，不 attach、不扫类、不装钩
    if (DiagLevel() <= 3) {
        static std::atomic<bool> once3{false};
        if (!once3.exchange(true)) LOGI(TPL_LOG " diag L%d：跳过 attach/扫类/装钩（二分定位）", DiagLevel());
        return;
    }

    // 本函数可能跑在 mod 线程（TryInit/Tick 都是 pthread）⇒ 先 attach 再解析元数据
    // （Games/HzsjMod.cpp:369 同款；thread_attach 幂等，重复调用无害）
    auto& api = GetApi();
    if (api.thread_attach && !g_loginThreadAttached.load()) {
        void* domain = api.domain_get ? api.domain_get() : nullptr;
        if (domain) {
            g_loginThreadAttached.store(true);
            api.thread_attach(domain);
            LOGI(TPL_LOG " login assist: mod 线程已 attach 到 il2cpp");
        }
    }

    /* ★r20（2026-10-17 二分定案）：登录保命钩**默认关闭**。
     *   真机分级实测（同一 .so，仅 `setprop debug.starry.il2cpp_diag` 不同；日志 /tmp/nhsy2_L*.txt）：
     *     L4 = attach + 按图找类、**零代码补丁**          → 进程存活，走到 TapTapSDK.Init done
     *     L5 = L4 + libc 日志护盾(asf.shield) 一个补丁     → 存活
     *     L6 = L5 + 下面这 3 个 Login_* Dobby 补丁         → **启动后约 4s 干净退出 exit(0)**
     *     L7 = 全装（+ SyncHooks 69 槽）                   → 退出（与 L6 同因）
     *   ⇒ 触发者只是这 3 个补丁：`LoginService.StartInteractiveLoginInternal` /
     *     `HandleLoginFailure` / `HandleLoginSuccess`；**69 个功能钩本身无罪**（L4 证明零补丁可活，
     *     功能钩不在启动路径上，10.15 底包新增/加严的保护面只覆盖了登录链这三个方法）。
     *   代价评估（为什么敢默认关）：登录保命钩是 r17 为「用户网页登录手速 vs 游戏 ~30–37s 预算」
     *   加的兜底——**关了不会闪退，只会在登录拖太久时照常弹「登录失败」框、可重试**；
     *   而开着会让游戏根本进不去。两害相权取重。
     *   需要复现研究时：`setprop debug.starry.nhsy2_login_hooks 1` 再冷启（会退出，仅供取证）。 */
    if (!LoginHooksEnabled()) {
        static std::atomic<bool> onceLh{false};
        if (!onceLh.exchange(true)) {
            LOGW(TPL_LOG " login assist: 登录保命钩默认关闭（本底包实测这 3 个补丁会让游戏启动约 4s 后干净退出 exit(0)）；"
                         "需要取证时 setprop debug.starry.nhsy2_login_hooks 1");
        }
        g_loginHooksDone.store(true);   // 不再重试，避免每 tick 刷屏
        return;
    }

    int ok = 0;
    for (int i = 0; i < kLoginSlotsCount; ++i) {
        g_lastFatal = false;
        if (HookSlotInstall(kLoginSlots[i])) ++ok;
    }

    // 诊断（只读，不改游戏状态）：LoginService.PostLoginTimeoutSeconds 是否为 30 量级。
    //   类型由 LoginService 字段表反查：typeIndex=43356 == System.Single（602 处字段同型）。
    //   若它读出 30f，说明游戏侧预算就是它；若 Task.Delay 那条路没命中，下一轮改打它。
    if (api.class_get_field && api.field_static_get) {
        void* kSvc = FindClassNs("NEON.Framework.Login", "LoginService");
        if (kSvc) {
            void* f = api.class_get_field(kSvc, "PostLoginTimeoutSeconds");
            if (f) {
                float fv = -1.f;
                api.field_static_get(f, &fv);
                LOGI(TPL_LOG " login assist: LoginService.PostLoginTimeoutSeconds=%.3f（float，只读诊断）",
                     static_cast<double>(fv));
            }
        }
    }

    // 可选槽：只试一轮（系统类重载可能不存在，避免每 tick 刷失败日志）
    if (!g_loginDelaySlotTried.exchange(true)) {
        int okOpt = 0;
        for (int i = 0; i < kLoginOptionalCount; ++i) {
            g_lastFatal = false;
            if (HookSlotInstall(kLoginOptionalSlots[i])) ++okOpt;
        }
        LOGI(TPL_LOG " login assist: 预算延长槽 %d/%d（Task.Delay 重载，缺失不影响登录保命）",
             okOpt, kLoginOptionalCount);
    }

    if (ok == kLoginSlotsCount) {
        g_loginHooksDone.store(true);
        LOGI(TPL_LOG " login assist: %d 个登录挂点全部就位（与菜单开关无关）", ok);
    } else {
        LOGW(TPL_LOG " login assist: 登录挂点仅 %d/%d，下一 tick 重试", ok, kLoginSlotsCount);
    }
}

// ============================================================================
//  ⑥.7 全屏攻击 —— `NEON.Framework.Attri` 读点强制（对齐斌哥机制，2026-10-11 落地）
// ============================================================================
//  取证链（详见 10_docs/霓虹深渊2/路线锁定.md §全屏攻击）：
//    ① 斌哥壳的 4 个 Dobby 钩点里 desc2 = `NEON.Framework.Attri::TryGetValue`
//       （壳 .data.rel.ro 描述符 + 40 字节签名）；壳字段表条目名与游戏属性名
//       **逐字同名**（MaxBomb/FinalBombCount/multipleJumpCountLimit…），消费者
//       0x2cf948 语义 =「按属性名把 double 抬到目标值（fcmp/str，只抬不降）」。
//    ② 壳的「全屏」组条目在静态初始化里被清零（stp q1,q1,[x8,#0x190]）⇒ 键名与
//       目标值由服务端「菜鸟云验证」下发，离线抓不到；但**游戏属性表已定位**：
//       `NEON.Game.Managers.GameConstVariables`（index 10208，380 字段）里
//       `ProjectileRange`（token 0x0400CA41）＝子弹射程，与 ProjectileCount /
//       BulletSizeMult / MeleeCheckDistance 同族 ⇒ **全屏攻击 = 射程抬满**。
//    ③ 属性系统 `NEON.Framework.Attri`（index 2966，26 方法）读接口：
//       `Get(String)->Double`、`get_Item(String)->Double`、
//       `GetOrDefault(String,Single)->Double`、`TryGetValue(String, out ?)->Bool`、
//       `f(String)->Single`；其名字典 `AttriDictDelegate`（index 2964）的
//       `get_Item -> Double` / `set_Item(String,Double)` 佐证**属性存储值就是
//       double** ⇒ `TryGetValue` 第 2 形参是 8 字节 double 出参，钩子里直接改写
//       它不会破坏任何东西（这也是 desc2 被斌哥选作「键名嗅探点」的原因）。
//  实现：**只在读点强制**（不写任何对象内存、不碰 SetClamp/变更事件、不调 Set）：
//    键名小写后命中规则表（§kFullRules：bulletsize / lifespan，均为真机实测键名）⇒
//    把这次读到的值抬到规则目标（只抬不降），其余键一律原样返回；同时按
//    「首次见到该键」去重打印 `attri probe #N key=… value=…`（上限 120 条），
//    用于真机直接看到游戏到底查询了哪些属性名
//    —— 若游戏用的键名不在候选里，这份名单就是下一步的定位依据。
//  安全边界：① 只在 g_full 开时改写；② 只改返回值/出参，绝不写对象内存；
//            ③ 探针窗口满 120 后零扫描（AttriProbeKey 首行短路）。
//  键名规则表（小写子串匹配）：命中即把这次读到的值抬到 rule.value（只抬不降）。
//  为什么是「一组」而不是单独 ProjectileRange（m02534 复盘）：
//    · 斌哥的全屏组条目是**服务端推送**（静态初始化被清零），离线拿不到键名；
//    · 游戏 AutoDict 的 104 个键里**没有** ProjectileRange，但跳跃组证明字典键
//      可以超出这 104 个（MultipleJumpCountLimit 等变体就不在表里）⇒ 键名只能
//      靠真机 `attri probe` 名单定；先用一组候选把「射程/存活/倍率」覆盖住，
//      并逐次打印命中的是哪个 needle，下一轮按名单收窄。
//  ★ r9（用户 m04709「全屏攻击抄不明白」）：needle **全部换成真机实测键名**
//    （名单 `/tmp/nhsy2/keys_observed.txt`，81 键，来自 `attri probe`）——
//    旧 needle `projectilerange` / `range` 在 81 个键里**一次都没命中**（整个名单
//    没有任何键含 "range"），所以旧版全场只抬了 `finalbulletlifespanmult` 一条。
//    斌哥机制＝「按名字把某个值抬上去、只抬不降」⇒ 这里就是它的执行面（Attri::TryGetValue）。
//  ★★★ r10（用户 m04844「攻击一个怪物，只有一个怪物受到伤害」）——键名名单已离线坐实：
//    `NEON.Game.Managers.GameConstVariables`（index 10208）的 **380 个字段名小写后
//    就是 AutoDict 的键名**（实测 81 键全部能在其中逐字对上，含 `basemeleeorbitalradius`
//    / `currentspriteattackcount` 这类只有玩到才被请求的键）。⇒ 键名不用再猜，直接把
//    「攻击判定几何」那一族字段名拿来当 needle（都只在与攻击相关的时机被读，抬它们
//    就是斌哥的「按名抬值、只抬不降」）：
//      MeleeCheckDistance / ExtraMeleeCheckDistance / FinalMeleeCheckDistance（近战命中判定距离）
//      DamageSize（NEONPlayerState.get_DamageSize 的底值键）、ProjectileRange（弹道射程）
//      BaseMeleeOrbitalRadius（环绕近战半径）
static struct FullForceRule { const char* needle; double value; } kFullRules[] = {
    {"checkdistance",   99999.0},  // MeleeCheckDistance 族：命中判定距离 ⇒ 判定覆盖整屏
    {"damagesize",      99999.0},  // DamageSize：玩家伤害判定大小
    {"projectilerange", 99999.0},  // ProjectileRange：弹道射程
    {"orbitalradius",   99999.0},  // BaseMeleeOrbitalRadius：环绕近战半径
    {"bulletcount",        24.0},  // finalbulletcount（实测键）：一次 24 发铺开 ⇒ 一枪扫全场
    {"bulletsize",        100.0},  // finalbulletsizemult（实测键）：子弹体积 ×100
    {"lifespan",           60.0},  // finalbulletlifespanmult（实测键）：子弹存活久
};

//  ★ r10 无限水晶（用户 m04844「使用水晶，提示需要更多水晶」）：水晶计数同样是
//    AutoDict 的键（GameConstVariables 的 `CurrentSpriteCrystalCount`(0x0400CAED)，
//    与**实测已被请求的** `currentspriteattackcount` / `currentspriteattackandshadowcount`
//    同一族）⇒ 用同一个读点强制机制抬计数。
//    只抬**计数键** `*spritecrystalcount`：`LeastOneSpriteCrystal`(保底值) 故意不抬——
//    保底若被抬到 9999，而内部真实计数走的是不走 Attri 的循环，就可能触发「补到保底」
//    的死循环/爆量生成；计数键本身只被读来判定「够不够」，抬它没有这个风险。
static struct GateForceRule { const char* needle; double value; } kCrystalRules[] = {
    {"spritecrystalcount", 9999.0},   // currentspritecrystalcount（水晶计数）
    {"crystalcount",       9999.0},   // 兜底：任何 *CrystalCount* 键（不含 leastonespritecrystal）
};
static const uint32_t    kAttriProbeMax  = 120;

static std::atomic<uint32_t> g_hitFull{0};     // 全屏读点命中（键匹配）次数
static std::atomic<uint32_t> g_fullForced{0};  // 实际抬值次数

typedef bool   (*AttriTryGetValue_t)(void* thiz, void* key, double* outValue, void* method);
typedef double (*AttriGetDouble_t)(void* thiz, void* key, void* method);
typedef double (*AttriGetOrDefSingle_t)(void* thiz, void* key, float defValue, void* method);
typedef float  (*AttriGetSingle_t)(void* thiz, void* key, void* method);

static AttriTryGetValue_t    g_origAttriTryGet   = nullptr;
static AttriGetDouble_t      g_origAttriGet      = nullptr;
static AttriGetDouble_t      g_origAttriGetItem  = nullptr;
static AttriGetOrDefSingle_t g_origAttriGetOrDef = nullptr;
static AttriGetSingle_t      g_origAttriF        = nullptr;

// Il2Cpp String → 小写 ASCII（属性名全 ASCII；非 ASCII 归一为 '?'）
//  布局：klass@0x0 / monitor@0x8 / length@0x10(int32) / utf16 chars@0x14
static void AttriKeyLower(void* keyObj, char* out, size_t cap) {
    if (cap == 0) return;
    out[0] = '\0';
    if (cap < 2 || !OkPtr(keyObj)) return;
    int32_t len = 0;
    memcpy(&len, reinterpret_cast<const char*>(keyObj) + 0x10, sizeof(len));
    if (len <= 0 || len > 256) return;
    const char* chars = reinterpret_cast<const char*>(keyObj) + 0x14;
    size_t n = 0;
    for (int32_t i = 0; i < len && n + 1 < cap; ++i) {
        uint16_t c = 0;
        memcpy(&c, chars + i * 2, sizeof(c));
        out[n++] = (c >= 'A' && c <= 'Z') ? static_cast<char>(c + 32)
                 : (c < 128 ? static_cast<char>(c) : '?');
    }
    out[n] = '\0';
}

static const FullForceRule* AttriMatchFullRule(const char* lower) {
    if (!lower || lower[0] == '\0') return nullptr;
    for (const FullForceRule& r : kFullRules)
        if (strstr(lower, r.needle) != nullptr) return &r;
    return nullptr;
}

// 去重探针：最多打印 120 个不同的属性键（窗口满后零成本短路）
static std::mutex g_attriProbeMu;
static char     g_attriKeys[kAttriProbeMax][40];
static uint32_t g_attriKeyCount = 0;

static void AttriProbeKey(void* keyObj, double value) {
    if (g_attriKeyCount >= kAttriProbeMax) return;      // 窗口已满：不再转换/扫描
    if (!OkPtr(keyObj)) return;
    char lower[40];
    AttriKeyLower(keyObj, lower, sizeof(lower));
    if (lower[0] == '\0') return;
    std::lock_guard<std::mutex> lk(g_attriProbeMu);
    if (g_attriKeyCount >= kAttriProbeMax) return;
    for (uint32_t i = 0; i < g_attriKeyCount; ++i)
        if (strcmp(g_attriKeys[i], lower) == 0) return;  // 已见过
    strncpy(g_attriKeys[g_attriKeyCount], lower, sizeof(g_attriKeys[0]) - 1);
    g_attriKeys[g_attriKeyCount][sizeof(g_attriKeys[0]) - 1] = '\0';
    ++g_attriKeyCount;
    LOGI(TPL_LOG " attri probe #%u key=%s value=%.4f", g_attriKeyCount, lower, value);
}

// 读点强制：键名命中规则表 ⇒ 抬到 rule.value（只抬不降，与壳同语义）
//  ★★★ r16：**本条已停用**（kFullRules 不再参与全屏攻击）。
//   停用理由（真机证据链）：① 记录仪 20261012_032803 里 pid 22685 崩溃**发生在
//   全屏攻击已经关掉（03:35:30 feature[3]=0）之后**，而崩前最后几行我方日志正是这条路的
//   `attri probe #56..#60`（basemeleeorbitalradius / extrabulletspeedmult / …）；
//   ② 反汇编解出的崩溃语义是「`[x19+0x30]` 里装着小整数 0xa8，被当对象读 +0x10」——
//   典型的**对象被释放/字段被写坏**；③ 这条规则表会把 `bulletsize/projectilerange/
//   orbitalradius/checkdistance/damagesize` 抬到 99999、`bulletcount` 抬到 24，直接改
//   已在飞行的投射物参数，是可疑的写坏源（斌哥壳虽然也改 Attri，但他们改的是**他们自己
//   那份**参数表，我们无法复刻其具体键与值）。
//   ⇒ r16 全屏攻击只保留「记录 + 逐个施放」这条纯转发机制（不写任何游戏字段），
//     属性强制整条下掉；函数保留空壳，避免 5 个读点调用点全部改动（单变量原则）。
static void AttriForceIfFull(void* keyObj, double* value) {
    (void)keyObj; (void)value;
    return;
}

// 无限水晶的读点强制（与全屏同一机制，闸不同）：开闸且键命中水晶计数族 ⇒ 抬到 9999
static const GateForceRule* AttriMatchCrystalRule(const char* lower) {
    if (!lower || lower[0] == '\0') return nullptr;
    for (const GateForceRule& r : kCrystalRules)
        if (strstr(lower, r.needle) != nullptr) return &r;
    return nullptr;
}

static std::atomic<uint32_t> g_hitCrystalKey{0};   // 水晶键请求次数（含未开闸）
static std::atomic<uint32_t> g_crystalForced{0};   // 实际抬值次数

// 水晶键专属探针：键名含 crystal 就打印（不受 120 条 attri 窗口限制），
// 用于下一轮直接看到游戏在哪个时机请求水晶计数、键名是什么。
static void AttriProbeCrystalKey(void* keyObj, double value) {
    static std::atomic<uint32_t> logged{0};
    if (logged.load(std::memory_order_relaxed) >= 60) return;
    if (!OkPtr(keyObj)) return;
    char lower[40];
    AttriKeyLower(keyObj, lower, sizeof(lower));
    // ★ r15：水晶＝Mana ⇒ 含 mana 的键也要能看到（门/提示走的就是这条）
    if (strstr(lower, "crystal") == nullptr && strstr(lower, "mana") == nullptr) return;
    const uint32_t n = ++g_hitCrystalKey;
    if (logged.fetch_add(1, std::memory_order_relaxed) < 60)
        LOGI(TPL_LOG " crystal/mana attri probe #%u key=%s value=%.4f on=%d（水晶计数键被请求）",
             n, lower, value, GateCrystal() ? 1 : 0);
}

static void AttriForceIfCrystal(void* keyObj, double* value) {
    if (!value || !OkPtr(keyObj)) return;
    char lower[40];
    AttriKeyLower(keyObj, lower, sizeof(lower));
    AttriProbeCrystalKey(keyObj, *value);
    if (!GateCrystal()) return;
    const GateForceRule* rule = AttriMatchCrystalRule(lower);
    if (!rule) return;
    if (*value >= rule->value) return;
    const double old = *value;
    *value = rule->value;
    const uint32_t n = ++g_crystalForced;
    if (n <= 12)
        LOGI(TPL_LOG " crystal attri: %s(needle=%s) %.3f -> %.3f（无限水晶：只抬不降）",
             lower, rule->needle, old, rule->value);
}

static bool Hooked_AttriTryGetValue(void* thiz, void* key, double* outValue, void* method) {
    const bool ok = GuardedOr<bool>("orig.Attri.TryGetValue", false, [&]{ return g_origAttriTryGet ? g_origAttriTryGet(thiz, key, outValue, method) : false; });
    if (ok && outValue) {
        AttriProbeKey(key, *outValue);
        AttriForceIfFull(key, outValue);
        AttriForceIfCrystal(key, outValue);
    }
    return ok;
}

static double Hooked_AttriGet(void* thiz, void* key, void* method) {
    double v = GuardedOr<double>("orig.Attri.Get", 0.0, [&]{ return g_origAttriGet ? g_origAttriGet(thiz, key, method) : 0.0; });
    AttriProbeKey(key, v);
    AttriForceIfFull(key, &v);
    AttriForceIfCrystal(key, &v);
    return v;
}

static double Hooked_AttriGetItem(void* thiz, void* key, void* method) {
    double v = GuardedOr<double>("orig.Attri.GetItem", 0.0, [&]{ return g_origAttriGetItem ? g_origAttriGetItem(thiz, key, method) : 0.0; });
    AttriProbeKey(key, v);
    AttriForceIfFull(key, &v);
    AttriForceIfCrystal(key, &v);
    return v;
}

static double Hooked_AttriGetOrDefault(void* thiz, void* key, float defValue, void* method) {
    double v = GuardedOr<double>("orig.Attri.GetOrDefault", static_cast<double>(defValue),
        [&]{ return g_origAttriGetOrDef ? g_origAttriGetOrDef(thiz, key, defValue, method)
                                        : static_cast<double>(defValue); });
    AttriProbeKey(key, v);
    AttriForceIfFull(key, &v);
    AttriForceIfCrystal(key, &v);
    return v;
}

static float Hooked_AttriF(void* thiz, void* key, void* method) {
    const float v = GuardedOr<float>("orig.Attri.GetFloat", 0.0f, [&]{ return g_origAttriF ? g_origAttriF(thiz, key, method) : 0.0f; });
    AttriProbeKey(key, static_cast<double>(v));
    double d = static_cast<double>(v);
    AttriForceIfFull(key, &d);
    AttriForceIfCrystal(key, &d);
    return static_cast<float>(d);
}

// ---- 挂点表 ----
// rva 一律 0：本游戏磁盘 so 的 dynsym 被加密，静态 RVA 不可得（见路线锁定.md）
static const HookSlot kSlots[] = {
    // 战斗
    {"Inv_DeductHP",   0, 0, "NEON.Game",         "Hearts",      "DeductHP", 2,
     (void*)Hooked_Hearts_DeductHP,    (void**)&g_origHeartsDeduct},
    {"Dmg_DeductHP",   0, 0, "NEON.Game",         "HitPoints",   "DeductHP", 2,
     (void*)Hooked_HitPoints_DeductHP, (void**)&g_origHitPointsDeduct},
    {"Dmg_CalcDamage", 0, 0, "NEON.Game",         "HitPoints",   "CalculationDamage", 2,
     (void*)Hooked_CalculationDamage,  (void**)&g_origCalcDamage},
    // （「技能无CD」的三个挂点已按用户要求删除，见 §6.3 注释）
    // 资源
    {"Res_Coin",       0, 0, "NEON.Game.Managers", "NEONPlayerState", "get_Coin", 0,
     (void*)Hooked_GetCoin,     (void**)&g_origGetCoin},
    {"Res_MaxCoin",    0, 0, "NEON.Game.Managers", "NEONPlayerState", "get_MaxCoin", 0,
     (void*)Hooked_GetMaxCoin,  (void**)&g_origGetMaxCoin},
    {"Res_Key",        0, 0, "NEON.Game.Managers", "NEONPlayerState", "get_Key", 0,
     (void*)Hooked_GetKey,      (void**)&g_origGetKey},
    {"Res_Crystal",    0, 0, "NEON.Game.Managers", "NEONPlayerState", "get_CurrentSpriteCrystalCount", 0,
     (void*)Hooked_GetCrystal,  (void**)&g_origGetCrystal},
    {"Res_GitterCoin", 0, 0, "NEON.Game.Managers", "NEONPlayerState", "get_GitterCoin", 0,
     (void*)Hooked_GetGitterCoin, (void**)&g_origGetGitter},
    {"Res_HasInfKey",  0, 0, "NEON.Game.Managers", "NEONPlayerState", "get_HasInfiniteKey", 0,
     (void*)Hooked_HasInfiniteKey, (void**)&g_origHasInfKey},
    {"Res_InfKey",     0, 0, "NEON.Game.Managers", "NEONPlayerState", "get_InfiniteKey", 0,
     (void*)Hooked_GetInfiniteKey, (void**)&g_origGetInfKey},
    {"Res_CoinValue",  0, 0, "NEON.Game.Managers", "NEONPlayerState", "get_CoinValue", 0,
     (void*)Hooked_GetCoinValue, (void**)&g_origGetCoinValue},
    {"Res_RealKey",    0, 0, "NEON.Game.Managers", "NEONPlayerState", "get_RealKey", 0,
     (void*)Hooked_GetRealKey,   (void**)&g_origGetRealKey},
    {"Res_KeyValue",   0, 0, "NEON.Game.Managers", "NEONPlayerState", "get_KeyValue", 0,
     (void*)Hooked_GetKeyValue,  (void**)&g_origGetKeyValue},
    {"Res_UseCoin",    0, 0, "NEON.Game.Managers", "NEONPlayerState", "ConsumeCoinRaw", 2,
     (void*)Hooked_ConsumeCoinRaw, (void**)&g_origConsumeCoin},
    {"Res_UseKey",     0, 0, "NEON.Game.Managers", "NEONPlayerState", "ConsumeKeyRaw", 2,
     (void*)Hooked_ConsumeKeyRaw,  (void**)&g_origConsumeKey},
    // 资源 —— 通用 CostType 通道（★「无限金币/钥匙/水晶」真挂点；万能闸同源）
    {"Res_AmountRaw",  0, 0, "NEON.Game.Managers", "NEONPlayerState", "GetAmountRaw", 2,
     (void*)Hooked_GetAmountRaw,   (void**)&g_origGetAmountRaw},
    {"Res_Amount",     0, 0, "NEON.Game.Managers", "NEONPlayerState", "GetAmount", 2,
     (void*)Hooked_GetAmount,      (void**)&g_origGetAmount},
    {"Res_Enough",     0, 0, "NEON.Game.Managers", "NEONPlayerState", "HasEnough", 3,
     (void*)Hooked_HasEnough,      (void**)&g_origHasEnough},
    {"Res_EnoughRaw",  0, 0, "NEON.Game.Managers", "NEONPlayerState", "HasEnoughRaw", 3,
     (void*)Hooked_HasEnoughRaw,   (void**)&g_origHasEnoughRaw},
    {"Res_EnoughRawF", 0, 0, "NEON.Game.Managers", "NEONPlayerState", "HasEnoughRaw", 2,
     (void*)Hooked_HasEnoughRawFloat, (void**)&g_origHasEnoughRawF},
    {"Res_ConsumeRaw", 0, 0, "NEON.Game.Managers", "NEONPlayerState", "ConsumeRaw", 4,
     (void*)Hooked_ConsumeRaw4,    (void**)&g_origConsumeRaw4},
    {"Res_SetResource",0, 0, "NEON.Game.Managers", "NEONPlayerState", "SetResource", 2,
     (void*)Hooked_SetResource,    (void**)&g_origSetResource},
    {"Res_UseCoinCvt", 0, 0, "NEON.Game.Managers", "NEONPlayerState", "UseCoinByCovert", 1,
     (void*)Hooked_UseCoinByCovert, (void**)&g_origUseCoinByCovert},
    // 资源 —— 6.4e：消耗漏斗 + 可用性谓词（★ 2026-10-11 五轮定稿新增）
    {"Res_Funnel",     0, 0, "NEON.Game.Managers", "NEONPlayerState", "ConsumeRawInternal", 2,
     (void*)Hooked_ConsumeRawInternal, (void**)&g_origConsumeRawInternal},
    {"Res_CostSupport",0, 0, "NEON.Game.Managers", "NEONPlayerState", "IsCostTypeSupported", 1,
     (void*)Hooked_IsCostTypeSupported, (void**)&g_origIsCostTypeSupported},
    // 商店交易闸（NEON.UI.StoreItem）—— 只认「金币付款的道具 + 无限金币开」
    {"Shop_CanBuy",    0, 0, "NEON.UI",            "StoreItem", "CanBuy", 0,
     (void*)Hooked_StoreItemCanBuy, (void**)&g_origStoreCanBuy},
    {"Shop_IsFree",    0, 0, "NEON.UI",            "StoreItem", "IsFree", 0,
     (void*)Hooked_StoreItemIsFree, (void**)&g_origStoreIsFree},
    // 战斗 —— 伤害倍率真改写点（伤害链最后一跳）
    {"Dmg_ReallyDeduct",0, 0, "NEON.Game",         "HitPoints",   "ReallyDeductHP", 2,
     (void*)Hooked_ReallyDeductHP,  (void**)&g_origReallyDeduct},
    // ★ r14 全屏攻击：记录点 + 施放点（只用活体的必经入口，绝不枚举内存）
    {"Full_KillTakeDmg",0, 0, "NEON.Game",         "Killable",    "TakeDamage", 2,
     (void*)Hooked_KillableTakeDamage, (void**)&g_origKillableTakeDamage},
    {"Full_KillUpdate", 0, 0, "NEON.Game",         "Killable",    "Update", 0,
     (void*)Hooked_KillableUpdate,     (void**)&g_origKillableUpdate},
    // ★ r14 无限水晶＝开门：解锁成本三连（HasEnough/GetAmount/ConsumeRaw 之外的单一路径）
    {"Res_CanUnlock",   0, 0, "NEON.Game.Managers", "NEONPlayerState", "CanUnlock", 2,
     (void*)Hooked_CanUnlock,    (void**)&g_origCanUnlock},
    {"Res_CanFreeUnlock",0,0, "NEON.Game.Managers", "NEONPlayerState", "CanFreeUnlock", 1,
     (void*)Hooked_CanFreeUnlock,(void**)&g_origCanFreeUnlock},
    {"Res_UnlockCosts", 0, 0, "NEON.Game.Managers", "NEONPlayerState", "UnlockCosts", 3,
     (void*)Hooked_UnlockCosts,  (void**)&g_origUnlockCosts},
    // 移动 / 跳跃
    {"Spd_MoveSpeed",  0, 0, "NEON.Game.Actors",  "PlayerActor", "get_MovementSpeed", 0,
     (void*)Hooked_GetMovementSpeed, (void**)&g_origGetMovementSpeed},
    {"Spd_FinalMax",   0, 0, "NEON.Game",         "Mover",       "get_FinalMaxSpeed", 0,
     (void*)Hooked_GetFinalMaxSpeed, (void**)&g_origGetFinalMaxSpeed},
    {"Jump_FinalLimit",0, 0, "NEON.Game.Actors",  "PlayerActor", "get_FinalMultipleJumpCountLimit", 0,
     (void*)Hooked_GetFinalJumpLimit, (void**)&g_origGetFinalJumpLimit},
    {"Jump_Limit",     0, 0, "NEON.Game.Actors",  "PlayerActor", "get_MultipleJumpCountLimit", 0,
     (void*)Hooked_GetJumpLimit,    (void**)&g_origGetJumpLimit},
    // 标准版 —— StandardEditionClientAccess
    {"Std_CanUse",      0, 0, "NEON.Framework.Login", "StandardEditionClientAccess", "CanUse", 1,
     (void*)Hooked_Std_CanUse, (void**)&g_orig_Std_CanUse},
    {"Std_ClientEval",  0, 0, "NEON.Framework.Login", "StandardEditionClientAccess", "Evaluate", 2,
     (void*)Hooked_Std_ClientEvaluate, (void**)&g_orig_Std_ClientEvaluate},
    {"Std_EvalDiffEdit",0, 0, "NEON.Framework.Login", "StandardEditionClientAccess", "EvaluateDifficultyEditing", 0,
     (void*)Hooked_Std_ClientEvalDiffEditing, (void**)&g_orig_Std_ClientEvalDiffEditing},
    {"Std_AdvBossProg", 0, 0, "NEON.Framework.Login", "StandardEditionClientAccess", "get_CanAdvanceBossProgress", 0,
     (void*)Hooked_Std_CanAdvanceBossProgress, (void**)&g_orig_Std_CanAdvanceBossProgress},
    {"Std_IsFreeBoss",  0, 0, "NEON.Framework.Login", "StandardEditionClientAccess", "IsFreeBoss", 1,
     (void*)Hooked_Std_IsFreeBoss, (void**)&g_orig_Std_IsFreeBoss},
    {"Std_CanViewBoss", 0, 0, "NEON.Framework.Login", "StandardEditionClientAccess", "CanViewBoss", 1,
     (void*)Hooked_Std_CanViewBoss, (void**)&g_orig_Std_CanViewBoss},
    {"Std_ResolveRun",  0, 0, "NEON.Framework.Login", "StandardEditionClientAccess", "ResolveRunAccess", 3,
     (void*)Hooked_Std_ClientResolveRunAccess, (void**)&g_orig_Std_ClientResolveRunAccess},
    // 标准版 —— StandardEditionAccessPolicy
    //  ★ 2026-10-08 移除两个槽位（原文如下，留作记录）：
    //    {"Std_AppliesPlat", ... "get_AppliesToCurrentPlatform", ...}
    //    {"Std_ContentOvr",  ... "get_HasContentOverride", ...}
    //  移除理由（两条，缺一不可）：
    //   ① 语义上是**常量 getter**，反汇编实测 get_AppliesToCurrentPlatform =
    //      `mov w0,#1; ret`（恒 true）、get_HasContentOverride = `mov w0,wzr; ret`
    //      （恒 false）⇒ 挂钩它们对本游戏没有任何功能增益，却把「恒真谓词」的风险
    //      （hook-rules §28.1）加进来。
    //   ② 两者都是 8 字节函数，与邻居紧贴；旧版走 Dobby（16 字节补丁）时**把邻居
    //      整个吃掉**，正是导致 `Hooked_Std_HasContentOverride` 自递归 506 层
    //      SIGSEGV 的真凶（详见 §⑤.1 注释）。
    //  标准版解锁不依赖它们：总闸 Std_CanUse + 各功能闸（ClientEval / Assault /
    //  Ban / Patch）+ 数据层兜底 PushStandardEditionEntitlement 三路覆盖。
    {"Std_RestrictEvo", 0, 0, "NEON.Framework.Login", "StandardEditionAccessPolicy", "IsRestrictedEvolution", 3,
     (void*)Hooked_Std_IsRestrictedEvolution, (void**)&g_orig_Std_IsRestrictedEvolution},
    {"Std_EvalPurchase",0, 0, "NEON.Framework.Login", "StandardEditionAccessPolicy", "EvaluatePurchase", 4,
     (void*)Hooked_Std_PolicyEvaluatePurchase, (void**)&g_orig_Std_PolicyEvaluatePurchase},
    // 标准版 —— StandardEditionAssaultAccess
    {"Std_IsFreeLevel", 0, 0, "NEON.Framework.Login", "StandardEditionAssaultAccess", "IsFreeLevel", 1,
     (void*)Hooked_Std_IsFreeLevel, (void**)&g_orig_Std_IsFreeLevel},
    {"Std_CanUseLevel", 0, 0, "NEON.Framework.Login", "StandardEditionAssaultAccess", "CanUseRunLevel", 2,
     (void*)Hooked_Std_CanUseRunLevel, (void**)&g_orig_Std_CanUseRunLevel},
    {"Std_AssaultEval", 0, 0, "NEON.Framework.Login", "StandardEditionAssaultAccess", "Evaluate", 2,
     (void*)Hooked_Std_AssaultEvaluate, (void**)&g_orig_Std_AssaultEvaluate},
    // 标准版 —— StandardEditionBanAccess
    {"Std_BanSession",  0, 0, "NEON.Framework.Login", "StandardEditionBanAccess", "get_CanUseSessionRules", 0,
     (void*)Hooked_Std_BanCanUseSessionRules, (void**)&g_orig_Std_BanCanUseSessionRules},
    {"Std_BanCurrent",  0, 0, "NEON.Framework.Login", "StandardEditionBanAccess", "get_CanUseCurrentRules", 0,
     (void*)Hooked_Std_BanCanUseCurrentRules, (void**)&g_orig_Std_BanCanUseCurrentRules},
    {"Std_BanOrigAcc",  0, 0, "NEON.Framework.Login", "StandardEditionBanAccess", "get_HasOriginalAccess", 0,
     (void*)Hooked_Std_BanHasOriginalAccess, (void**)&g_orig_Std_BanHasOriginalAccess},
    {"Std_BanOwner",    0, 0, "NEON.Framework.Login", "StandardEditionBanAccess", "get_IsPersonalOwner", 0,
     (void*)Hooked_Std_BanIsPersonalOwner, (void**)&g_orig_Std_BanIsPersonalOwner},
    {"Std_BanEval",     0, 0, "NEON.Framework.Login", "StandardEditionBanAccess", "Evaluate", 1,
     (void*)Hooked_Std_BanEvaluate, (void**)&g_orig_Std_BanEvaluate},
    // 标准版 —— StandardEditionPatchAccess
    {"Std_PatchEnergy", 0, 0, "NEON.Framework.Login", "StandardEditionPatchAccess", "get_CanProvisionEnergy", 0,
     (void*)Hooked_Std_PatchCanProvisionEnergy, (void**)&g_orig_Std_PatchCanProvisionEnergy},
    {"Std_PatchCurRun", 0, 0, "NEON.Framework.Login", "StandardEditionPatchAccess", "get_CanUseCurrentRun", 0,
     (void*)Hooked_Std_PatchCanUseCurrentRun, (void**)&g_orig_Std_PatchCanUseCurrentRun},
    {"Std_PatchProg",   0, 0, "NEON.Framework.Login", "StandardEditionPatchAccess", "get_CanProgressCurrentRun", 0,
     (void*)Hooked_Std_PatchCanProgressCurrentRun, (void**)&g_orig_Std_PatchCanProgressCurrentRun},
    {"Std_PatchOrig",   0, 0, "NEON.Framework.Login", "StandardEditionPatchAccess", "get_HasOriginalProgress", 0,
     (void*)Hooked_Std_PatchHasOriginalProgress, (void**)&g_orig_Std_PatchHasOriginalProgress},
    {"Std_PatchEval",   0, 0, "NEON.Framework.Login", "StandardEditionPatchAccess", "Evaluate", 1,
     (void*)Hooked_Std_PatchEvaluate, (void**)&g_orig_Std_PatchEvaluate},
    // 标准版 —— LoginService
    {"Std_LoginEvalPur",0, 0, "NEON.Framework.Login", "LoginService", "EvaluateCurrentPurchase", 1,
     (void*)Hooked_Std_LoginEvaluateCurrentPurchase, (void**)&g_orig_Std_LoginEvaluateCurrentPurchase},
    // 全屏攻击 —— NEON.Framework.Attri 读点强制（键名含 projectilerange ⇒ 抬到 9999）
    {"Full_AttriTryGet",0, 0, "NEON.Framework", "Attri", "TryGetValue", 2,
     (void*)Hooked_AttriTryGetValue,  (void**)&g_origAttriTryGet},
    {"Full_AttriGet",   0, 0, "NEON.Framework", "Attri", "Get", 1,
     (void*)Hooked_AttriGet,          (void**)&g_origAttriGet},
    {"Full_AttriItem",  0, 0, "NEON.Framework", "Attri", "get_Item", 1,
     (void*)Hooked_AttriGetItem,      (void**)&g_origAttriGetItem},
    {"Full_AttriOrDef", 0, 0, "NEON.Framework", "Attri", "GetOrDefault", 2,
     (void*)Hooked_AttriGetOrDefault, (void**)&g_origAttriGetOrDef},
    {"Full_AttriF",     0, 0, "NEON.Framework", "Attri", "f", 1,
     (void*)Hooked_AttriF,            (void**)&g_origAttriF},

    // ────────────────────────────────────────────────────────────────────────
    //  ★ r8（用户 m04183「全屏攻击没效果、无限水晶没效果」）新增 5 个槽位
    // ────────────────────────────────────────────────────────────────────────
    //  全屏攻击：射程真正生效的读点（旧 Attri 键名路线实测无 range 类键）；
    //            get_DamageSize 只读探针（留证据，不改值）。
    //  ★ r8b：get_ProjectileRange 返回的是 `Veewo.Stats.Stat` 对象（r8 真机打印真名），
    //          本层只透传 + 记实例；真正的强制点在 Full_StatValue（同实例闸 1:1）。
    {"Full_ProjRange",  0, 0, "NEON.Game.Managers", "NEONPlayerState", "get_ProjectileRange", 0,
     (void*)Hooked_GetProjectileRange, (void**)&g_origProjRange},
    {"Full_DamageSize", 0, 0, "NEON.Game.Managers", "NEONPlayerState", "get_DamageSize", 0,
     (void*)Hooked_GetDamageSize,      (void**)&g_origDamageSize},
    {"Full_StatValue",  0, 0, "Veewo.Stats", "Stat", "get_Value", 0,
     (void*)Hooked_StatValue,          (void**)&g_origStatValue},
    //  无限水晶：SpriteManager 真通道（旧 Res_Crystal 挂在 NEONPlayerState 展示值＝死路径）
    {"Res_CrystalCount", 0, 0, "NEON.Game", "SpriteManager", "GetCrystalSpriteCount", 0,
     (void*)Hooked_GetCrystalSpriteCount, (void**)&g_origGetCrystalSpriteCount},
    {"Res_UseCrystal",  0, 0, "NEON.Game", "SpriteManager", "UseCrystalSprite", 1,
     (void*)Hooked_UseCrystalSprite,      (void**)&g_origUseCrystalSprite},
    {"Res_SpriteAtk",   0, 0, "NEON.Game.Managers", "NEONPlayerState", "ConsumeSpriteAtkRaw", 2,
     (void*)Hooked_ConsumeSpriteAtk,      (void**)&g_origConsumeSpriteAtk},
    //  无限水晶 r9（m04709）：AddCrystalSprite 只读探针 —— 它的父类实现（水晶通道是否活着）；
    //  真正的「无条件使用」在 Res_UseCrystal（不足即补 1 再重试）。
    {"Res_CrystalAdd",  0, 0, "NEON.Game", "SpriteManager", "AddCrystalSprite", 0,
     (void*)Hooked_AddCrystalSprite,      (void**)&g_origAddCrystalSprite},
};

// ★ 2026-10-12（r8c 根因）：`InstallAllHooks` 的循环是
//   `for (i = 0; i < kSlotsCount && i < kSlotsMax; ++i)` —— 表比 kSlotsMax 长时
//   末尾槽位**静默丢弃**（无 probe / 无 skip / 不进 dead），只会表现为「功能没效果」。
//   真机实证：r8b 表 66 槽、kSlotsMax=64 ⇒ Res_UseCrystal / Res_SpriteAtk 两槽
//   从未被尝试安装（`install pass: ok=61 dead=0/66`，probe 行数 64+6）。

static const int kSlotsCount = (int)(sizeof(kSlots) / sizeof(kSlots[0]));

// 这条断言把上面那类事故变成**编译期**错误（必须紧跟 kSlotsCount 定义）。
static_assert(kSlotsCount <= kSlotsMax,
              "kSlotsMax 小于槽位表长度 ⇒ 末尾槽位会被静默丢弃，请同步抬高 kSlotsMax");

// ============================================================================
//  ⑥.6 运行时字段偏移 / 枚举字面量解析（装钩时做一次；失败只降级）
// ============================================================================

//  ---- 6.6.0 静态字段读取的**保护区**包装（★ 2026-10-11 真机闪退根因） ----
//  事故：2000 包真机实测，点「无限跳」开关 → `feature[6] = 1` → SyncHooks →
//        ResolveOffsetsAndEnums，日志停在 `StandardEditionAccessResult.Allowed = 1`
//        之后，紧接着 `[THEMIS]: get signal 11` / tombstone。
//        addr2line（Android_nhsy2dbg.mk 出的带符号 so，代码布局与 release 逐字节一致）：
//          #07 mb_nativeSetFeature           MainModern.cpp:305
//          #06 ResolveOffsetsAndEnums        Games/nhsy2Mod.cpp:1630  ← 内联的 field_static_get
//          #05 SyncHooks                     Games/nhsy2Mod.cpp:1730
//          #04 libil2cpp.so+0x304d0e8        (il2cpp_field_static_get_value)
//        tombstone：`Cause: null pointer dereference`，`fault addr 0xb8`，
//        `memcpy(dst=0x7421e97fa0, src=0xb8, n=0x110)`。
//  根因：CostType 枚举表的第一项是 `value__` —— 它是**枚举的实例背衬字段**，
//        `il2cpp_class_get_field_from_name` 一样能取到，但它没有静态存储，
//        `il2cpp_field_static_get_value` 于是拿 `static_fields(NULL)+0xb8` 当源地址 ⇒ SIGSEGV。
//  铁律：**交给 `field_static_get` 的字段必须先确认是静态字段**；本模块的做法是
//        ①调用点白名单只列静态枚举字面量（见下方 kCTNames，已删 `value__`）；
//        ②每次读取都过 RunGuarded ⇒ 万一还有漏网的实例字段，退化成「跳过该项 + 一行日志」，
//          不会再让整个进程陪葬（本次事故时该调用不在任何保护区里，信号直接交回 themis）。
struct StaticGetJob { void* field; int32_t value; };

static void StaticGetThunk(void* arg) {
    StaticGetJob* j = reinterpret_cast<StaticGetJob*>(arg);
    GetApi().field_static_get(j->field, &j->value);
}

static bool StaticGetI32(const char* tag, void* field, int32_t* out) {
    if (!field || !out) return false;
    StaticGetJob job{field, 0};
    if (!RunGuarded(tag, StaticGetThunk, &job)) return false;
    *out = job.value;
    return true;
}

//  ★ 注意：保护区**不可嵌套**。`t_faultJmp` 是单层 thread_local jmpbuf，每个
//    RunGuarded/DobbyHookGuarded 进来都 `sigsetjmp` 覆盖它；若在内层 RunGuarded
//    返回之后再发生信号，siglongjmp 会跳到**已经返回的**内层栈帧 ⇒ 栈被毁。
//    所以加固只能逐调用点做（像本节这样），绝不能在已有保护区的函数外面再套一层。

static void ResolveOffsetsAndEnums() {
    if (!EnsureApi()) return;

    g_clsHearts = FindClassNs("NEON.Game", "Hearts");
    if (!g_clsHearts) g_clsHearts = GetApi().FindClassByShortName("Hearts");
    if (g_clsHearts) {
        g_offHeartsInv.store(GetApi().FieldOffset(g_clsHearts, "isInvincible"));
        g_offHeartsUnkill.store(GetApi().FieldOffset(g_clsHearts, "UnKillable"));
        g_offHeartsDamageCD.store(GetApi().FieldOffset(g_clsHearts, "isDamageCD"));
        LOGI(TPL_LOG " Hearts offsets: inv=0x%zX unkill=0x%zX dmgCD=0x%zX",
             g_offHeartsInv.load(), g_offHeartsUnkill.load(), g_offHeartsDamageCD.load());
    } else {
        LOGW(TPL_LOG " class miss NEON.Game.Hearts — 无敌字段回写降级（仍返回 false）");
    }

    g_clsDamageInfo = FindClassNs("NEON.Game", "DamageInfo");
    if (g_clsDamageInfo) {
        g_offDmgDamage.store(GetApi().FieldOffset(g_clsDamageInfo, "damage"));
        g_offDmgMult.store(GetApi().FieldOffset(g_clsDamageInfo, "damageMult"));
        void* mi = GetApi().GetMethod(g_clsDamageInfo, "IsFromLocalPLayer", 0);
        if (mi) g_fnIsFromLocalPlayer = *reinterpret_cast<void**>(mi);
        LOGI(TPL_LOG " DamageInfo offsets: damage=0x%zX mult=0x%zX isFromLocal=%p",
             g_offDmgDamage.load(), g_offDmgMult.load(), g_fnIsFromLocalPlayer);
    } else {
        LOGW(TPL_LOG " class miss NEON.Game.DamageInfo — 伤害倍率降级为原样放行");
    }

    // ★ r14 全屏攻击：Killable.faction（同阵营判据；拿不到就走类名闸，不猜偏移）
    {
        void* clsK = FindClassNs("NEON.Game", "Killable");
        if (!clsK) clsK = GetApi().FindClassByShortName("Killable");
        if (clsK) {
            const size_t fo = GetApi().FieldOffset(clsK, "faction");
            g_offKillableFaction.store(fo, std::memory_order_relaxed);
            LOGI(TPL_LOG " fullscreen: Killable faction off=0x%zX（全屏攻击：同阵营判据；0=退化为类名闸）", fo);
        } else {
            LOGW(TPL_LOG " class miss NEON.Game.Killable — 全屏攻击改用类名闸（仍只打活体）");
        }
    }

    g_clsPlayerActor = FindClassNs("NEON.Game.Actors", "PlayerActor");
    if (!g_clsPlayerActor) g_clsPlayerActor = GetApi().FindClassByShortName("PlayerActor");
    if (g_clsPlayerActor) {
        g_offSecSkillTimer.store(GetApi().FieldOffset(g_clsPlayerActor, "SecSkillTimer"));
        g_offPassiveSkillTimer.store(GetApi().FieldOffset(g_clsPlayerActor, "PassiveSkillTimer"));
        LOGI(TPL_LOG " PlayerActor offsets: secTimer=0x%zX pasTimer=0x%zX",
             g_offSecSkillTimer.load(), g_offPassiveSkillTimer.load());
    }

    // 枚举字面量：StandardEditionAccessResult.Allowed
    void* kEnum = FindClassNs("NEON.Framework.Login", "StandardEditionAccessResult");
    if (!kEnum) kEnum = GetApi().FindClassByShortName("StandardEditionAccessResult");
    if (kEnum) {
        void* f = GetApi().class_get_field(kEnum, "Allowed");
        int32_t v = 0;
        if (StaticGetI32("StdAccessResult.Allowed", f, &v)) {
            if (v >= 0 && v < 16) {
                g_allowedEnum.store(v);
                LOGI(TPL_LOG " StandardEditionAccessResult.Allowed = %d", v);
            }
        }
    }

    // 通用资源通道的枚举字面量：NEON.Game.PowerUps.Skills.CostType
    //  运行时用 class_get_field + field_static_get 逐项取值并打全表日志 ——
    //  MetadataSearch 不输出枚举数值，而「彩色钥匙究竟是哪个 CostType」只能靠
    //  运行期实测反推（见 路线锁定.md §彩色钥匙考古：静态搜索 0 命中）。
    void* kCT = FindClassNs("NEON.Game.PowerUps.Skills", "CostType");
    if (!kCT) kCT = GetApi().FindClassByShortName("CostType");
    if (kCT) {
        // ★ 只列**静态枚举字面量**：`value__` 是枚举的实例背衬字段，喂给
        //   il2cpp_field_static_get_value 会在 NULL+0xb8 上 memcpy 而闪退
        //   （2026-10-11 真机事故根因，见 §6.6.0）。本表刻意不含 `value__`。
        static const char* kCTNames[] = {
            "Bomb", "Coin", "Crystal", "Egg", "HeartContainer", "Key", "Pet",
            "Shield", "Heart", "Portal", "BossCoin", "PrayActor", "SecSkill", "Elevator",
            "OtherBuy", "OtherDraw", "OtherInteract", "OtherOpenChest", "OtherPickUp",
            "PickUp", "Curse", "CurseLevel", "Mana", "SpriteAttack", "Cooling",
            "AutoAimOn", "AutoAimOff", "None", "Token", "Hurt", "NoChest",
            "SafeHouseNoBelief", "CanNotUseSecSkill", "TempHeart", "TempShield",
            "RewardDepleted", "Unlocked"
        };
        char buf[640];
        int off = 0;
        int misses = 0;
        for (size_t i = 0; i < sizeof(kCTNames) / sizeof(kCTNames[0]); ++i) {
            void* f = GetApi().class_get_field(kCT, kCTNames[i]);
            int32_t v = 0;
            if (!f || !StaticGetI32("CostType.static", f, &v)) {
                ++misses;
                continue;   // 未找到 / 非静态 / 读取越界：跳过该项，其它项照常
            }
            if (!std::strcmp(kCTNames[i], "Bomb"))    g_ctBomb.store(v);
            if (!std::strcmp(kCTNames[i], "Coin"))    g_ctCoin.store(v);
            if (!std::strcmp(kCTNames[i], "Key"))     g_ctKey.store(v);
            if (!std::strcmp(kCTNames[i], "Crystal")) g_ctCrystal.store(v);
            if (!std::strcmp(kCTNames[i], "Mana"))    g_ctMana.store(v);
            if (off < (int)sizeof(buf) - 24) {
                off += std::snprintf(buf + off, sizeof(buf) - off, "%s=%d ", kCTNames[i], v);
            }
        }
        LOGI(TPL_LOG " CostType 全表: %s", buf);
        if (misses) LOGW(TPL_LOG " CostType 跳过 %d 项（未找到或非静态字段）", misses);
        // 三次定稿后闸门是「白名单 + 一功能一开关」：某一项没解析出来 ⇒ 那句话对应的
        // 开关在本局是死的（ct == -1 永不命中），**不会**退化成影响其它通道。
        if (g_ctBomb.load() < 0)    LOGW(TPL_LOG " CostType.Bomb 未解析 ⇒ 「无限手雷」本局无效");
        if (g_ctCoin.load() < 0)    LOGW(TPL_LOG " CostType.Coin 未解析 ⇒ 「无限金币」本局无效");
        if (g_ctCrystal.load() < 0) LOGW(TPL_LOG " CostType.Crystal 未解析 ⇒ 「无限水晶」本局无效");
        if (g_ctKey.load() < 0)     LOGW(TPL_LOG " CostType.Key 未解析 ⇒ 「无限钥匙」本局无效");
        // ★ r15：水晶通道 = Crystal(2) + Mana(22)（开门消耗 Mana，取证见 g_ctMana 处注释）
        if (g_ctMana.load() < 0)    LOGW(TPL_LOG " CostType.Mana 未解析 ⇒ 「无限水晶（开门）」本局无效");
        else                        LOGI(TPL_LOG " 无限水晶通道 = Crystal(%d) + Mana(%d)（开门消耗的是 Mana）",
                                          g_ctCrystal.load(), g_ctMana.load());
    } else {
        LOGW(TPL_LOG " class miss CostType — 四项资源开关本局全部无效（不会牵连其它通道）");
    }

    // ---- 6.6z r8b：射程 stat 通道的句柄 + 无限水晶的 AddCrystalSprite 方法句柄 ----
    //  ★★ 坑（r8 首轮真机已证，绝对不要再犯）：**`api.type_name()` 的返回值不能 `free()`**。
    //      r8 用它打一行返回类型名诊断后 `free(tn)`，真机当场：
    //        F/libc: Pointer tag for 0x7441496f90 was truncated … tagged-pointers
    //        F/libc: Fatal signal 6 (SIGABRT), code -1 (SI_QUEUE)   → Abort message 同上
    //      （scudo 分配器 abort；该串不是可 free 的 malloc 内存 / 带 pointer tag）。
    //      r8 因此拿到了唯一想要的结论——`get_ProjectileRange` 返回 `Veewo.Stats.Stat`
    //      对象（不是数值）⇒ 强制点挪到 `Veewo.Stats.Stat.get_Value`（Full_StatValue，实例闸）。
    //      诊断目的已达成，本模块**不再调用 type_name / free**。
    {
        auto& apiR = GetApi();
        void* kPS = FindClassNs("NEON.Game.Managers", "NEONPlayerState");
        if (!kPS) kPS = apiR.FindClassByShortName("NEONPlayerState");
        void* miRange = kPS ? apiR.GetMethod(kPS, "get_ProjectileRange", 0) : nullptr;
        void* kStat = FindClassNs("Veewo.Stats", "Stat");
        if (!kStat) kStat = apiR.FindClassByShortName("Stat");
        void* miVal = (kStat && kStat != kPS) ? apiR.GetMethod(kStat, "get_Value", 0) : nullptr;
        void* miDmg = kPS ? apiR.GetMethod(kPS, "get_DamageSize", 0) : nullptr;
        LOGI(TPL_LOG " 全屏攻击: 射程 stat 通道 get_ProjectileRange=%p DamageSize=%p Veewo.Stats.Stat.get_Value=%p（实例闸强制点）",
             miRange, miDmg, miVal);
        if (!miRange)  LOGW(TPL_LOG " 全屏攻击: get_ProjectileRange 未解析 ⇒ 射程实例拿不到，本局无效");
        if (!miVal)    LOGW(TPL_LOG " 全屏攻击: Veewo.Stats.Stat.get_Value 未解析 ⇒ 射程强制本局无效");
        //  无限水晶：补发 / 退款要用的 AddCrystalSprite 方法句柄
        void* kSM = FindClassNs("NEON.Game", "SpriteManager");
        if (!kSM) kSM = apiR.FindClassByShortName("SpriteManager");
        if (kSM) {
            g_miAddCrystalSprite = apiR.GetMethod(kSM, "AddCrystalSprite", 0);
            LOGI(TPL_LOG " 无限水晶: SpriteManager.AddCrystalSprite 句柄=%p（用完退款 / 起手补发用）",
                 g_miAddCrystalSprite);
            if (!g_miAddCrystalSprite) {
                LOGW(TPL_LOG " 无限水晶: AddCrystalSprite 未解析 ⇒ 只剩「精灵攻击免费」一条通道");
            }
        } else {
            LOGW(TPL_LOG " 无限水晶: 未找到 NEON.Game.SpriteManager ⇒ 水晶通道降级");
        }
    }

    // ---- 6.4e 商店交易闸所需：TradingType.Coin 字面量 + StoreItem 字段偏移 ----
    //  依据（metadata 权威）：StoreItem.get_CurrentTradingType() 的返回类型是
    //  NEON.Game.TradingType，其枚举成员只有 Coin / DeductHeart / AddCurse / CurseLevel
    //  ⇒ 「金币付款」只是其中一项，只有它允许被「无限金币」改写（1:1；
    //    挂心脏 / 加诅咒付款的道具一律不动）。
    void* kTT = FindClassNs("NEON.Game", "TradingType");
    if (!kTT) kTT = GetApi().FindClassByShortName("TradingType");
    if (kTT) {
        void* fTT = GetApi().class_get_field(kTT, "Coin");   // ★ 不含 value__（见 §6.6.0）
        int32_t vTT = 0;
        if (StaticGetI32("TradingType.Coin", fTT, &vTT)) {
            g_ttCoin.store(vTT);
            LOGI(TPL_LOG " TradingType.Coin = %d", vTT);
        } else {
            LOGW(TPL_LOG " TradingType.Coin 未解析 ⇒ 商店交易闸本局失效（四项资源通道不受影响）");
        }
    } else {
        LOGW(TPL_LOG " class miss TradingType — 商店交易闸本局失效");
    }

    void* kStore = FindClassNs("NEON.UI", "StoreItem");
    if (!kStore) kStore = GetApi().FindClassByShortName("StoreItem");
    if (kStore) {
        g_offStoreTradingType.store(GetApi().FieldOffset(kStore, "_tradingType"));
        g_offStoreCanPurChase.store(GetApi().FieldOffset(kStore, "CanPurChase"));
        LOGI(TPL_LOG " StoreItem offsets: tradingType=0x%zX canPurChase=0x%zX",
             g_offStoreTradingType.load(), g_offStoreCanPurChase.load());
    } else {
        LOGW(TPL_LOG " class miss NEON.UI.StoreItem — 商店交易闸本局失效");
    }

    // LoginService 单例字段（数据层兜底：直接置「已拥有标准版」）
    g_clsLoginSvc = FindClassNs("NEON.Framework.Login", "LoginService");
    if (g_clsLoginSvc) {
        g_offFullGameUnlocked.store(GetApi().FieldOffset(g_clsLoginSvc, "_isFullGameUnlocked"));
        g_offLegacyStdOwned.store(GetApi().FieldOffset(g_clsLoginSvc, "_legacyStandardOwned"));
        g_offStdDlcOwned.store(GetApi().FieldOffset(g_clsLoginSvc, "_standardDlcOwned"));
        LOGI(TPL_LOG " LoginService offsets: fullGame=0x%zX legacy=0x%zX dlc=0x%zX",
             g_offFullGameUnlocked.load(), g_offLegacyStdOwned.load(), g_offStdDlcOwned.load());
    }
}

// 标准版数据层兜底：把 LoginService 单例上的三个权益布尔直写为真
static void PushStandardEditionEntitlement() {
    if (!g_std.load()) return;
    if (!g_clsLoginSvc) return;
    void* inst = GetApi().StaticRefField(g_clsLoginSvc, "Instance");
    if (!OkPtr(inst)) return;
    SafeWriteU8(inst, g_offFullGameUnlocked.load(), 1);
    SafeWriteU8(inst, g_offLegacyStdOwned.load(), 1);
    SafeWriteU8(inst, g_offStdDlcOwned.load(), 1);
}

// ============================================================================
//  ⑥.6z 全屏攻击（**已落地** —— 实现见上方「⑥.6z 全屏攻击」读点强制小节）
// ============================================================================
//  这是本项目里唯一一个「不是靠已知 Dobby 钩点、而是靠属性名强制」的功能，
//  因此把判定逻辑单独记在这里，避免下轮重复考古：
//    ① 斌哥「全屏」组的键名/目标值来自服务端，静态 .so 里被清零 ⇒ 不可复制；
//    ② 但机制可复制：**按属性名把读到的 double 抬大**（只抬不降）；
//    ③ 本游戏射程属性 = `ProjectileRange`（GameConstVariables index 10208，
//       token 0x0400CA41），与 ProjectileCount/BulletSizeMult 同族；
//    ④ 读点 = Attri 的 5 个读接口（TryGetValue/Get/get_Item/GetOrDefault/f），
//       key 小写含 "projectilerange" ⇒ 抬到 9999；`attri probe` 打印键名名单。
//  ⇒ g_full **参与** InstallAllHooks / AnyFeatureOn（不再只是保存状态）。
static_assert(NHSY2_FEAT_FULL == 3, "全屏攻击槽位变更后请同步本节说明");

// ============================================================================
//  ⑦ 分组安装 + 懒装（全开关关 = 0 hook）
// ============================================================================
static int InstallAllHooks() {
    int ok = 0, dead = 0;
    for (int i = 0; i < kSlotsCount && i < kSlotsMax; ++i) {
        if (g_slotDead[i]) { ++dead; continue; }
        g_lastFatal = false;
        if (HookSlotInstall(kSlots[i])) {
            ++ok;
        } else if (g_lastFatal) {
            g_slotDead[i] = true;
            LOGW(TPL_LOG " slot[%d] %s 已熔断（装钩期致命信号），本次会话不再重试",
                 i, kSlots[i].tag);
        }
    }
    LOGI(TPL_LOG " install pass: ok=%d dead=%d/%d", ok, dead, kSlotsCount);
    if (g_installSkip.load() > 0) {
        LOGI(TPL_LOG " install skip: %d 个槽位因「入口形态不安全」主动跳过"
                     "（转发器/短函数；避免 Dobby 16 字节补丁改写邻居入口）",
             g_installSkip.load());
    }
    // ★ r11 无限水晶：`SpriteManager.GetCrystalSpriteCount` 是 8 字节 veneer（上面会被
    //   主动跳过）⇒ 改为挂它的目标（共享实现），只拦 veneer 里那个选择子的调用。
    //  ★ 必须**无条件**挂载：装钩只在「首次有任一功能开启」时跑一次，而用户可能
    //    进游戏后才打开无限水晶（g_installPassDone 已置位 ⇒ 永远不会再装）。
    //    行为闸在钩子内部按 g_crystal 实时判断。
    if (InstallCrystalCountImplHook()) ++ok;
    if (ok > 0) g_anyHooked.store(true);
    return ok;
}

static bool AnyFeatureOn() {
    return g_inv.load() || DmgMulActive()                           // 滑块项用 DmgMulActive（§③.1）
        || g_bomb.load() || g_coin.load() || g_crystal.load() || g_key.load() || g_ckey.load()
        || g_std.load() || g_full.load()                            // 全屏攻击：Attri 读点强制（§6.6z）
        || SpdMulActive() || g_jump.load();
}

/* ★r20：把「首次装钩」推迟到启动后 N ms（`setprop debug.starry.nhsy2_hook_delay_ms`，默认 10000）。
 *   动机：本底包上任何 il2cpp 方法代码补丁都会让游戏在 `Neon Abyss v…` 之后约 3.8 s **干净退出**
 *   （分级实测：L4/L5 零或单个 libc 补丁存活；L6 只加 3 个登录补丁即退出；65 个功能钩同样退出；
 *   而钩子集合与 r19 逐项相同 ⇒ 新加的是**代码完整性校验**）。
 *   若该校验是**一次性**的，把装钩推到它之后再落即可既保住功能又不触发。Tick 每拍都会重调
 *   SyncHooks，所以只是晚装、不会漏装（判据：`install pass: ok=69` 且进程存活）。 */
static long long HookDelayMs() {
    static std::atomic<int> cached{-1};
    int v = cached.load();
    if (v >= 0) return v;
    char buf[PROP_VALUE_MAX] = {0};
    const int n = __system_property_get("debug.starry.nhsy2_hook_delay_ms", buf);
    /* ★r20c 收官定案：**默认 0**（立即装钩）。实测 2026-10-17：默认 10000 会把 69 钩推到 themis 装载之后
     *   （+10.5s），那时装钩必 SEGV_ACCERR（ok=67 少 2 槽 + 进程死）；而 +5s（il2cpp 刚就绪、themis 未载）
     *   装钩 69/72 全中且无崩溃 —— 用户 L3 实测即此配置。 */
    v = (n > 0) ? atoi(buf) : 0;
    if (v < 0) v = 0;
    cached.store(v);
    return v;
}
static bool HookInstallTimeReached() {
    if (HookDelayMs() <= 0) return true;
    static std::atomic<long long> t0{0};
    const long long now = NowMonoMs();
    long long s = t0.load();
    if (s == 0) { t0.store(now); s = now; }
    return (now - s) >= HookDelayMs();
}

static void ClearSoftDirty(const char* why);   // ★r20 前置声明（定义见 §⑦.6e）
static void ReProtectExecPages(const char* why);   // ★r20 前置声明（定义见 §⑦.6e）
static bool FreezeThemisWatchdog();                 // ★r20 前置声明（定义见 §⑦.6f）
static std::atomic<bool> g_watchdogFrozen{false};    // ★r20 前置定义（SyncHooks 闸门要用）
static bool ThemisWatchdogNeutralized();             // ★r20b 前置声明（定义见 §⑦.6g）
static void FlushThreadRecords(const char* why);     // ★r20b 前置声明（定义见 §⑦.6g）
static std::atomic<int> g_ntMode{0};                 // ★r20b pthread_create 闸模式（0=只记账/1=拒全部/2=拒一个偏移）
static std::atomic<int> g_tcRefused{0};              // ★r20b 已被拒绝出生的 themis 线程数
static void FlushExitGuardRecords(const char* why);  // ★r20c 前置声明（定义见 §⑦.6h）
static std::atomic<int> g_exitGuardMode{0};          // ★r20c exit_group 闸模式（0=不装/1=RET_LOG/2=RET_TRAP）

/* ★r28：SyncHooks 的**节流单行摘要**（每 5 s 最多一行）。
 *  为什么需要：本版日志量大（themis/Unity 刷屏 + logd 限流），关键节点行经常被挤掉，
 *  只能靠「gate=… anyOn=… neutral=… api=…」这一行判定它卡在哪一步。 */
static long long NowMonoMs();          // 定义在 §6.5
static long long NowMonoMsFwd() { return NowMonoMs(); }
static void SyncTrace(const char* why) {
    static std::atomic<long long> lastMs{0};
    const long long now = NowMonoMsFwd();
    long long prev = lastMs.load(std::memory_order_relaxed);
    if (now - prev < 5000) return;
    if (!lastMs.compare_exchange_strong(prev, now, std::memory_order_relaxed)) return;
    LOGI(TPL_LOG " sync@%s anyOn=%d passDone=%d installed=%d neutral=%d exitGuard=%d libReady=%d",
         why, (int)AnyFeatureOn(), (int)g_installPassDone.load(), g_installedCount.load(),
         (int)(g_watchdogFrozen.load() || g_exitGuardMode.load() >= 3),
         g_exitGuardMode.load(), (int)g_libReady.load());
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    if (!AnyFeatureOn()) { SyncTrace("anyOff"); return; }   // 全关：不装、也不卸载
    if (g_installPassDone.load()) return;               // 快路径：一次装钩已完成，不再重跑
    if (!HookInstallTimeReached()) { SyncTrace("delay"); return; }   // ★r20：推迟到启动后 N ms

    std::lock_guard<std::mutex> lk(g_installMutex);     // ★ 串行化，防并发双挂
    if (g_installPassDone.load()) return;               // double-check
    if (!HookInstallTimeReached()) return;

    // ★r20 闸门：看门狗没中和就**不许装钩**（带着活看门狗装钩 = 必然被 exit）。下一 tick 再试。
    // ★r20b 换序：中和优先走「**拒绝出生**」（§⑦.6g，纯 libc 侧、**不写 themis 任何代码页**）；
    //   只有在拒绝没生效（闸未装 / themis 线程早于闸诞生）时才退回 r20 的「冻结」（往 themis .text 写）。
    //   依据：① 分级实测 L5「只改 libc 不触发校验」⇒ libc 是我们唯一验证过的安全面；② L9 已证触发源是
    //   「写代码页这一事件本身」⇒ 冻结动作与触发动作**同类**，这正是它「改写成功却仍退出」的最自洽解释。
    //   ★r20b 关键补充：**只要操作者选了拒绝路径（`nhsy2_nothread≠0`），就绝不退回「冻结」**
    //   —— 否则 SyncHooks 会在 themis 还没装载时就先往它 .text 写（把要验证的变量污染掉），
    //   整个「拒绝出生」实验就永远测不出结论。宁可不装钩（每 tick 重试、日志会喊），也不写 themis 代码页。
    if (!ThemisWatchdogNeutralized()) {
        if (g_ntMode.load() == 0 && FreezeThemisWatchdog()) {
            /* 冻结路径成功 ⇒ 放行（保持 r20 行为） */
        } else {
            static std::atomic<int> n{0};
            if (n.fetch_add(1) < 12) {
                LOGW(TPL_LOG " 看门狗尚未中和（mode=%d 已拒绝=%d）⇒ 本轮暂缓装钩，稍后重试",
                     g_ntMode.load(), g_tcRefused.load());
            }
            SyncTrace("neutralGate");
            return;
        }
    }
    // ★r24 防闪退：解析与装钩两步在**同一个函数帧内**纳入保护区。
    //   之所以不把整个 SyncHooks 包起来：它持有 std::lock_guard，siglongjmp 会跳过 C++ 析构
    //   ⇒ g_installMutex 永久上锁、Tick 线程死锁。包在这两步之内，锁的生命周期不受影响。
    //   解析失败/装钩中崩 ⇒ **不置 pass-done**，下一拍重试（宁可不装，也不让进程陪葬）。
    const bool okResolve = GuardedVoid("resolve.offsets", [&]{ ResolveOffsetsAndEnums(); });
    if (!okResolve) {
        static std::atomic<int> nr{0};
        if (nr.fetch_add(1) < 10) LOGW(TPL_LOG " 偏移/枚举解析崩溃已拦下 ⇒ 本轮不装钩，稍后重试");
        return;
    }
    int installed = 0;
    const bool okInstall = GuardedOr<bool>("install.hooks", false, [&]{ installed = InstallAllHooks(); return true; });
    g_installedCount.store(installed);
    ClearSoftDirty("after-install-hooks");               // ★r20 装钩后立刻清脏位
    if (!(SafeMask() & 4)) ReProtectExecPages("after-install-hooks");   // ★r27：mask=4 时禁用（风暴根因）
    if (!okInstall) {
        static std::atomic<int> ni{0};
        if (ni.fetch_add(1) < 10) LOGW(TPL_LOG " 装钩过程崩溃已拦下（已装 %d 槽）⇒ 不置 pass-done，稍后重试", installed);
        return;
    }
    g_installPassDone.store(true);
}

// ============================================================================
//  ⑦.6 async_safe 日志护盾（进程级；2026-10-11 真机「打着打着闪退」修复）
//
//  ★ 真机铁证（tombstone，10-11 03:51:15 pid 31119 / tid 31203 UnityMain）：
//      #00 libc.so abort+160
//      #01 libc.so __assert+36
//      #02 libc.so void out_vformat<BufferOutputStream>(…)+3248   (= 内联在 async_safe_format_buffer 里)
//      #03 <anonymous:7480000000>（假帧：0x7480000000 段是 rw- IL2CPP 堆，非可执行）
//      Abort message: 'bionic/libc/async_safe/async_safe_log.cpp:424:
//                      assertion "conversion specifier unsupported" failed'
//    ⇒ libc 的 async_safe 格式化器一旦收到**它不认识的 `%` 转换**就直接 assert→abort，
//      也就是「谁传了坏格式串，谁就能把整个游戏进程带走」。崩溃前 1.2 s 是进房间
//      （`Unity: Entering room P012_AGE(Clone)0`），同时用户在点悬浮窗。
//
//  ★ 为什么不能「挂 __assert/__assert2 后 return」：反汇编真机 libc（BuildId
//      f0468d2d3a31c553a7103b6114f35dc6）证实 out_vformat 的 default 分支
//      `0x8532c bl __assert@plt` 之后**紧邻的 0x85330 就是 async_safe_format_buffer 的函数入口**
//      （paciasp; sub sp,sp,#0x130）——编译器把 __assert 当 noreturn，其后没有任何 fallthrough
//      指令；从 __assert 返回 = 踩进下一个函数的序言 ⇒ 必崩。所以只能从「源头」下手。
//
//  ★ 做法：把 libc 内部、**不导出**的 `async_safe_format_buffer(char* buf, size_t, const char* fmt, ...)`
//      （libc.so +0x85330）换成我们自己的实现：
//        · 安装前双指纹：`__assert2`（导出）必须 == base+0x8a128，且目标首两条指令必须是
//          paciasp / sub sp,sp,#0x130；任一不符只 WARN 跳过（fail-soft，跨 libc 版本自动失效而不是挂错）。
//        · 我们的实现**从不调用原函数**（原函数遇坏格式串就 abort），一律 `vsnprintf(buf,size,fmt,ap)`
//          ——libc 公共符号、支持全套转换符、永不 assert；遇到 async_safe 不认/危险的转换
//          （`%n` 会写内存、`%m` 是 glibc 专有、末位孤立 `%`、未知字母）先把该 `%` 转义成 `%%`，
//          于是「坏格式串」最多变成一行丑日志，进程照常活着。
//        · 前 8 次命中打诊断：caller 地址 + dladdr 模块&偏移 + `_Unwind_Backtrace` 5 帧 + 格式串前 160 字符
//          ⇒ 这一行日志直接指出「谁在喂坏格式串」（本轮 tombstone 拿不到的 frame #03）。
//        · thread_local 重入计数：我们自己打日志时若又走到这里，只做最朴素的 vsnprintf、不再打日志。
//
//  ★ 为什么不删：这是**进程级**闸门，与开关无关（和 §④ fatal guard 同级）。删掉它，
//      任何一处上游坏格式串都会立刻恢复「打着打着闪退」。
// ============================================================================
typedef int (*AsfFormatFn)(char* buf, size_t size, const char* fmt, ...);

static AsfFormatFn       g_asfOrig = nullptr;          // 保留仅用于日志/取证
static std::atomic<bool> g_asfShieldArmed{false};
static std::atomic<int>  g_asfBadHits{0};
static thread_local int  t_asfDepth = 0;

// bionic async_safe 支持的转换字母（其余会 assert；`n`/`m` 另有内存写风险）
static inline bool AsfConvKnown(char c) {
    return c == '%' || std::strchr("diouxXeEfFgGaAcsp", c) != nullptr;
}

// 解析一个 `%`（p 指向 '%' 之后首字符）⇒ 返回 specifier 结束位置；*bad 标记「不支持/危险」
static const char* AsfScanSpec(const char* p, bool* bad) {
    while (*p == '-' || *p == '+' || *p == ' ' || *p == '#' || *p == '0' || *p == '\'') ++p;
    if (*p == '*') { ++p; } else { while (*p >= '0' && *p <= '9') ++p; }
    if (*p == '.') {
        ++p;
        if (*p == '*') { ++p; } else { while (*p >= '0' && *p <= '9') ++p; }
    }
    if (p[0] == 'h' && p[1] == 'h') { p += 2; }
    else if (p[0] == 'l' && p[1] == 'l') { p += 2; }
    else if (*p == 'h' || *p == 'l' || *p == 'j' || *p == 'z' || *p == 't' || *p == 'L' || *p == 'q') { ++p; }

    if (*p == '\0') { *bad = true; return p; }      // 末尾孤立 % 同样会 abort
    *bad = !AsfConvKnown(*p);
    return p + 1;
}

// 坏转换 ⇒ 转义成 `%%` + 原样 specifier 正文（`%X` → 输出字面 `%X`）
static bool AsfSanitize(const char* fmt, char* out, size_t outSize) {
    size_t o = 0;
    bool changed = false;
    const char* p = fmt;
    while (*p != '\0' && o + 2 < outSize) {
        if (*p != '%') { out[o++] = *p++; continue; }
        bool bad = false;
        const char* end = AsfScanSpec(p + 1, &bad);
        if (!bad) {
            while (p < end && o + 2 < outSize) out[o++] = *p++;
            continue;
        }
        changed = true;
        out[o++] = '%';
        out[o++] = '%';
        const char* q = p + 1;
        while (q < end && o + 1 < outSize) out[o++] = *q++;
        p = end;
    }
    out[o] = '\0';
    return changed;
}

struct AsfBacktrace {
    char text[300];
    int  len;
};

static _Unwind_Reason_Code AsfBtStep(struct _Unwind_Context* ctx, void* arg) {
    AsfBacktrace* bt = reinterpret_cast<AsfBacktrace*>(arg);
    uintptr_t pc = (uintptr_t)_Unwind_GetIP(ctx);
    if (pc != 0) {
        Dl_info di;
        std::memset(&di, 0, sizeof(di));
        const char* name = "?";
        size_t off = 0;
        if (dladdr(reinterpret_cast<void*>(pc), &di) && di.dli_fname) {
            name = di.dli_fname;
            off  = pc - (uintptr_t)di.dli_fbase;
        }
        const char* slash = std::strrchr(name, '/');
        if (slash) name = slash + 1;
        char one[72];
        int k = std::snprintf(one, sizeof(one), " %s+0x%zx", name, off);
        if (k > 0 && bt->len + k < (int)sizeof(bt->text) - 1) {
            std::memcpy(bt->text + bt->len, one, (size_t)k);
            bt->len += k;
            bt->text[bt->len] = '\0';
        }
    }
    return bt->len >= 240 ? _URC_END_OF_STACK : _URC_NO_REASON;
}

// 只用字面量格式串打日志（铁律：绝不给 logcat 喂动态格式串）
static void AsfReport(int hits, void* caller, const char* fmt) {
    char safe[192];
    size_t i = 0;
    for (const char* p = fmt; *p != '\0' && i + 1 < sizeof(safe); ++p) {
        char c = *p;
        safe[i++] = (c == '\n' || c == '\r' || c == '\t') ? ' ' : c;
    }
    safe[i] = '\0';

    Dl_info di;
    std::memset(&di, 0, sizeof(di));
    const char* mod = "?";
    size_t off = 0;
    if (caller != nullptr && dladdr(caller, &di) && di.dli_fname) {
        mod = di.dli_fname;
        off = (uintptr_t)caller - (uintptr_t)di.dli_fbase;
    }
    const char* slash = std::strrchr(mod, '/');
    if (slash) mod = slash + 1;

    AsfBacktrace bt;
    bt.len    = 0;
    bt.text[0] = '\0';
    _Unwind_Backtrace(AsfBtStep, &bt);

    LOGW(TPL_LOG " async_safe 护盾 #%d：日志格式串含不支持的转换 ⇒ 已转义、不 abort；caller=%p %s+0x%zx fmt=\"%s\" bt:%s",
         hits, caller, mod, off, safe, bt.text);
}

static int Hooked_AsfFormatBuffer(char* buf, size_t size, const char* fmt, ...) {
    if (t_asfDepth > 0) {                        // 递归/信号上下文：朴素路径，不打任何日志
        if (buf == nullptr || size == 0 || fmt == nullptr) { if (buf && size) buf[0] = '\0'; return 0; }
        va_list ap;
        va_start(ap, fmt);
        int r = vsnprintf(buf, size, fmt, ap);
        va_end(ap);
        return r;
    }
    if (buf == nullptr || size == 0 || fmt == nullptr) { if (buf && size) buf[0] = '\0'; return 0; }

    t_asfDepth = 1;
    char local[1024];
    const char* use = fmt;
    bool changed = false;
    if (std::strchr(fmt, '%') != nullptr && std::strlen(fmt) < sizeof(local)) {
        changed = AsfSanitize(fmt, local, sizeof(local));
        use = local;
    }
    if (changed) {
        int n = g_asfBadHits.fetch_add(1) + 1;
        if (n <= 8) AsfReport(n, __builtin_return_address(0), fmt);
    }
    va_list ap;
    va_start(ap, fmt);
    int r = vsnprintf(buf, size, use, ap);
    va_end(ap);
    t_asfDepth = 0;
    return r;
}

// ---- 装配：双指纹校验后替换 libc 内部函数（fail-soft）----
static void InstallLogAbortShield() {
    if (g_asfShieldArmed.load()) return;
    std::lock_guard<std::mutex> lk(g_installMutex);        // ★ 与装钩共用互斥，防并发改写 libc
    if (g_asfShieldArmed.load()) return;

    uintptr_t base = getLibraryAddress("libc.so");
    if (base == 0) {
        LOGW(TPL_LOG " async_safe 护盾：libc 基址不可得 ⇒ 跳过（fail-soft）");
        return;
    }
    const uintptr_t kAssert2Off = 0x8a128;   // 指纹 1：同一 build 的导出符号 __assert2
    const uintptr_t kAsfOff     = 0x85330;   // 指纹 2：async_safe_format_buffer（非 dynsym）
    void* assert2 = getSymAddress("libc.so", "__assert2", false);
    if (assert2 == nullptr || reinterpret_cast<uintptr_t>(assert2) != base + kAssert2Off) {
        LOGW(TPL_LOG " async_safe 护盾：libc 布局指纹不符（__assert2=%p 期望=%p）⇒ 跳过",
             assert2, reinterpret_cast<void*>(base + kAssert2Off));
        return;
    }
    void* target = reinterpret_cast<void*>(base + kAsfOff);
    uint32_t w0 = 0, w1 = 0;
    if (!ReadU32Guarded("asf.pro0", target, &w0) ||
        !ReadU32Guarded("asf.pro4", reinterpret_cast<const char*>(target) + 4, &w1)) {
        return;
    }
    if (w0 != 0xd503233Fu || w1 != 0xd104c3FFu) {          // paciasp ; sub sp,sp,#0x130
        LOGW(TPL_LOG " async_safe 护盾：目标序言 %08X %08X 非预期 ⇒ 跳过（fail-soft）", w0, w1);
        return;
    }
    if (!DobbyHookGuarded("asf.shield", target, reinterpret_cast<void*>(Hooked_AsfFormatBuffer),
                          reinterpret_cast<void**>(&g_asfOrig))) {
        return;
    }
    g_asfShieldArmed.store(true);
    LOGI(TPL_LOG " async_safe 日志护盾已装：libc+0x%zX orig=%p（conversion specifier unsupported 不再 abort 进程）",
         (size_t)kAsfOff, reinterpret_cast<void*>(g_asfOrig));

    // ★ 自检（每次启动一次，日志即证据）：故意喂一个坏格式串给**已装护盾**的入口，
    //   若护盾生效 ⇒ 只打一条诊断、返回，不 abort；若没生效 ⇒ 这里会直接闪退（= 护盾没装上）。
    //   注意：只能调 target（已改写成我们实现的入口），绝不能调 g_asfOrig（那是原函数，必 abort）。
    char probe[80];
    probe[0] = '\0';
    int pr = reinterpret_cast<AsfFormatFn>(target)(probe, sizeof(probe), "shield selftest %q end");
    LOGI(TPL_LOG " async_safe 护盾自检：坏格式串已拦下 rc=%d out=\"%s\"（本行之后进程应照常运行）", pr, probe);
}

// ============================================================================
/* ---- ⑦.6c ★r20：exit / _exit 拦截（libc 代码补丁安全：分级 L5 实测只改 libc 不触发 il2cpp 校验）----
 *  背景：2026-10-15 底包（vc603）新增 **libil2cpp 代码完整性校验** —— 改任何一个 il2cpp 方法
 *  的代码，进程约 4 s 后**干净退出** exit(0)（无 tombstone、无 FATAL EXCEPTION）。分级实测：
 *    L0/L3/L4（零补丁）存活 · L5（只改 libc 的 async_safe）存活 · L6（+3 个登录补丁）退出 ·
 *    L7（+65 个功能钩）退出 · **L8（只挂 1 个与启动无关的 Hearts.DeductHP）也退出**
 *  ⇒ 是**全局定时校验**，不是「钩坏了启动路径」。
 *  处置：校验命中的外部表现就是 `exit(0)`/`_exit(0)`。libc 补丁不触发它 ⇒ 把退出吞掉、让进程
 *  继续跑；每次调用都留日志（前 5 次），`setprop debug.starry.nhsy2_block_exit 0` 可放行。 */
static void (*g_exitOrig)(int)  = nullptr;
static void (*g_uexitOrig)(int) = nullptr;
static std::atomic<int>  g_exitSeen{0};
static std::atomic<bool> g_exitShieldArmed{false};

static bool BlockExitEnabled() {
    static std::atomic<int> cached{-1};
    int v = cached.load();
    if (v >= 0) return v == 1;
    char buf[PROP_VALUE_MAX] = {0};
    const int n = __system_property_get("debug.starry.nhsy2_block_exit", buf);
    v = (n > 0) ? atoi(buf) : 1;
    cached.store(v);
    return v == 1;
}
static void Hooked_Exit(int code) {
    const int n = g_exitSeen.fetch_add(1) + 1;
    if (BlockExitEnabled() && code == 0) {
        if (n <= 5) LOGW(TPL_LOG " exit(0) 已拦截（第 %d 次）—— 疑似 libil2cpp 完整性校验命中的退出；放行用 setprop debug.starry.nhsy2_block_exit 0", n);
        return;                                   // ★ 吞掉：让进程继续运行
    }
    if (n <= 5) LOGW(TPL_LOG " exit(%d) 放行（第 %d 次）", code, n);
    if (g_exitOrig) g_exitOrig(code);
}
static void Hooked_UExit(int code) {
    const int n = g_exitSeen.fetch_add(1) + 1;
    if (BlockExitEnabled() && code == 0) {
        if (n <= 5) LOGW(TPL_LOG " _exit(0) 已拦截（第 %d 次）—— 同上", n);
        return;
    }
    if (n <= 5) LOGW(TPL_LOG " _exit(%d) 放行（第 %d 次）", code, n);
    if (g_uexitOrig) g_uexitOrig(code);
}
static void InstallExitShield() {
    if (g_exitShieldArmed.load()) return;
    std::lock_guard<std::mutex> lk(g_installMutex);
    if (g_exitShieldArmed.load()) return;
    g_exitShieldArmed.store(true);
    void* pe = getSymAddress("libc.so", "exit", false);
    void* pu = getSymAddress("libc.so", "_exit", false);
    if (pe && DobbyHookGuarded("exit.shield", pe, reinterpret_cast<void*>(Hooked_Exit),
                               reinterpret_cast<void**>(&g_exitOrig))) {
        LOGI(TPL_LOG " exit 护盾已装：orig=%p（code==0 时拦截）", reinterpret_cast<void*>(g_exitOrig));
    }
    if (pu && DobbyHookGuarded("uexit.shield", pu, reinterpret_cast<void*>(Hooked_UExit),
                               reinterpret_cast<void**>(&g_uexitOrig))) {
        LOGI(TPL_LOG " _exit 护盾已装：orig=%p（code==0 时拦截）", reinterpret_cast<void*>(g_uexitOrig));
    }
}

/* ---- ⑦.6d ★r20：libil2cpp 完整性校验的**钓取与中和**（走 libc 补丁，安全）----
 *  线索：`libthemis.so`（TapTap themis lite 的 JNI 库）导入表含 `memcmp / open / __open_2 /
 *  read / lseek / mprotect / pthread_create / pthread_cond_timedwait` —— 正是「起一个定时线程、
 *  按段读取原始 libil2cpp.so、与内存里的映射逐段 memcmp」的配方；而分级实测已证明
 *  **改 libc 代码不触发该校验**（L5 存活）⇒ 用 libc 补丁去钓它最划算。
 *  做法：挂 `memcmp`；只对「单次比较 ≥ 4 KB」（文件 vs 内存的块比较特征）记账，把**调用者返回地址**
 *  打出来 —— 若落在 libthemis 的映射里，就拿到了校验函数的位置。并可选（`debug.starry.nhsy2_memcmp_spoof=1`）
 *  对「调用者属于 themis」的大块比较**直接返回 0（视为一致）**，从而让校验永远通过、不必改 themis 代码。 */
static int (*g_memcmpOrig)(const void*, const void*, size_t) = nullptr;
static std::atomic<int>      g_mcSeen{0};
static std::atomic<uintptr_t> g_themisBase{0};
static std::atomic<bool>     g_mcProbeArmed{false};

static bool MemcmpSpoofEnabled() {
    static std::atomic<int> cached{-1};
    int v = cached.load();
    if (v >= 0) return v == 1;
    char buf[PROP_VALUE_MAX] = {0};
    const int n = __system_property_get("debug.starry.nhsy2_memcmp_spoof", buf);
    v = (n > 0) ? atoi(buf) : 0;
    cached.store(v);
    return v == 1;
}
static int Hooked_Memcmp(const void* a, const void* b, size_t n) {
    if (n >= 256) {
        const uintptr_t lr = reinterpret_cast<uintptr_t>(__builtin_return_address(0));
        /* 记账去重：同一 (n,a,b,lr) 只记一次，避免周期性校验把日志刷满。
         * 热路径 ⇒ try_lock：抢不到锁就直接放行（只影响记账，不影响功能）。 */
        struct Rec { size_t n; uintptr_t a, b, lr; };
        static Rec recs[48];
        static std::atomic<int> nrec{0};
        static std::mutex m;
        std::unique_lock<std::mutex> lk(m, std::try_to_lock);
        if (lk.owns_lock()) {
            const int cnt = nrec.load();
            bool dup = false;
            for (int i = 0; i < cnt && i < 48; i++) {
                if (recs[i].n == n && recs[i].a == (uintptr_t)a && recs[i].b == (uintptr_t)b &&
                    recs[i].lr == lr) { dup = true; break; }
            }
            if (!dup && cnt < 48) {
                recs[cnt] = Rec{n, (uintptr_t)a, (uintptr_t)b, lr};
                nrec.store(cnt + 1);
                LOGW(TPL_LOG " memcmp u#%d n=%zu a=%p b=%p lr=%p", cnt + 1, n, a, b,
                     reinterpret_cast<void*>(lr));
            }
        }
    }
    return g_memcmpOrig ? g_memcmpOrig(a, b, n) : 0;
}

/* 进程内打印**全量** maps（一次性）：消除跨进程 ASLR 歧义，让 a/b/lr 都能落到具体映射上。 */
static void LogKeyModuleBases() {
    FILE* f = fopen("/proc/self/maps", "re");
    if (!f) return;
    char line[512];
    int n = 0;
    while (fgets(line, sizeof(line), f) && n < 40) {
        if (!strstr(line, "libthemis") && !strstr(line, "libil2cpp.so") &&
            !strstr(line, "libunity.so") && !strstr(line, "libc.so") &&
            !strstr(line, "libstarry")) continue;
        char* nl = strchr(line, '\n'); if (nl) *nl = '\0';
        LOGW(TPL_LOG " MAP %s", line);
        n++;
    }
    fclose(f);
    LOGW(TPL_LOG " MAP end(%d)", n);
}

/* 文件侧探针：若校验是「重读原 libil2cpp.so 再比对」，这两条会直接给出**调用者地址**。
 *  themis 导入的是 `__open_2` / `__openat_2`（非变参版本），正好可以安全挂钩。 */
static int  (*g_open2Orig)(const char*, int) = nullptr;
static int  (*g_openat2Orig)(int, const char*, int) = nullptr;
static std::atomic<int> g_openSeen{0};
static int Hooked_Open2(const char* path, int flags) {
    const int k = g_openSeen.fetch_add(1) + 1;
    if (k <= 60) {
        LOGW(TPL_LOG " open#%d path=%s lr=%p", k, path ? path : "(null)",
             __builtin_return_address(0));
    }
    return g_open2Orig ? g_open2Orig(path, flags) : -1;
}
static int Hooked_Openat2(int dirfd, const char* path, int flags) {
    const int k = g_openSeen.fetch_add(1) + 1;
    if (k <= 60) {
        LOGW(TPL_LOG " openat#%d dirfd=%d path=%s lr=%p", k, dirfd, path ? path : "(null)",
             __builtin_return_address(0));
    }
    return g_openat2Orig ? g_openat2Orig(dirfd, path, flags) : -1;
}

/* ★r20 soft-dirty 清除器（决定性假设）：
 *  L9 判决实验里 `CodePatchU32` 已换成**裸 svc** 完全绕开 libc，仍被判退 ⇒ 观测点在**内核/页级**。
 *  最自洽的解释是 themis 用 **soft-dirty 位**做「写检测」：定期 `echo 4 >/proc/self/clear_refs`
 *  复位全进程脏位，再读 `/proc/self/pagemap` 看自己模块的页面有没有被写过 —— 这解释了
 *  ①写回同值也退（只看"写过"不看内容）②约 4 s 的固定延迟（扫描周期）③改 libc 不触发（只扫自己的模块）。
 *  对策：**我们写完立刻把全进程 soft-dirty 清掉**，并起一个轻量线程周期性清（400 ms），
 *  让它的扫描看不到「我们的页面被写过」。 */
static void ClearSoftDirty(const char* why) {
    const int fd = open("/proc/self/clear_refs", O_WRONLY);
    if (fd < 0) { LOGW(TPL_LOG " clear_refs 打不开 errno=%d（why=%s）", errno, why); return; }
    const ssize_t w = write(fd, "4", 1);
    close(fd);
    static std::atomic<int> n{0};
    const int k = n.fetch_add(1) + 1;
    if (k <= 6) LOGW(TPL_LOG " clear_refs(4) #%d why=%s w=%zd", k, why, w);
}

/* ★r20 关键实验：绕开 libc 的 `mprotect`（裸 `svc #0`），并把自己发起的 mprotect 从 themis 眼前藏掉。
 *  依据（2026-10-17 L9 判决实验）：唯一一次代码页改动是「按原值写回」，且 `CodePatchU32` 写完**已把
 *  页权限还原成 R-X、内容零变化**，游戏**照样干净退出** ⇒ 校验盯的不是内容（不是校验和），而是
 *  **mprotect/写页这个事件本身**；而只改 libc（L5）不触发 ⇒ themis 极可能是在 libc 层拦 `mprotect`
 *  来观察「谁把它保护的代码页改成可写」。
 *  做法：① `SvcMprotect()` 用 arm64 系统调用号 226 直接发 svc；② 钩住 libc `mprotect`，凡请求里带
 *  PROT_WRITE 的**都由我们自己发裸 syscall 并直接返回、不向下转发** ⇒ themis 的观察层看不到这次
 *  「代码页变可写」。行为上与真实 mprotect 等价（同一个系统调用），只是不让它记账。 */
static int SvcMprotect(void* addr, size_t len, int prot) {
#if defined(__aarch64__)
    register long x0 __asm__("x0") = reinterpret_cast<long>(addr);
    register long x1 __asm__("x1") = static_cast<long>(len);
    register long x2 __asm__("x2") = static_cast<long>(prot);
    register long x8 __asm__("x8") = 226;   // __NR_mprotect (arm64)
    __asm__ volatile("svc #0" : "+r"(x0) : "r"(x1), "r"(x2), "r"(x8) : "memory", "cc");
    if (x0 < 0) { errno = static_cast<int>(-x0); return -1; }
    return static_cast<int>(x0);
#else
    return mprotect(addr, len, prot);
#endif
}
static int (*g_mprotectLibcOrig)(void*, size_t, int) = nullptr;
static std::atomic<int> g_mphSeen{0};
static std::atomic<bool> g_mprotectHideArmed{false};
static int Hooked_MprotectHide(void* addr, size_t len, int prot) {
    const int k = g_mphSeen.fetch_add(1) + 1;
    if (k <= 40) {
        LOGW(TPL_LOG " mprotect(hide)#%d addr=%p len=%zu prot=%d lr=%p -> 裸 svc", k, addr, len, prot,
             __builtin_return_address(0));
    }
    if (prot & PROT_WRITE) return SvcMprotect(addr, len, prot);   // ★ 不转发：themis 看不到
    if (g_mprotectLibcOrig) return g_mprotectLibcOrig(addr, len, prot);
    return SvcMprotect(addr, len, prot);
}
static void InstallMprotectHider() {
    if (g_mprotectHideArmed.load()) return;
    std::lock_guard<std::mutex> lk(g_installMutex);
    if (g_mprotectHideArmed.load()) return;
    g_mprotectHideArmed.store(true);
    void* pm = getSymAddress("libc.so", "mprotect", false);
    if (!pm) { LOGW(TPL_LOG " mprotect 隐藏器：符号不可得 ⇒ 跳过"); return; }
    if (DobbyHookGuarded("mprotect.hide", pm, reinterpret_cast<void*>(Hooked_MprotectHide),
                         reinterpret_cast<void**>(&g_mprotectLibcOrig))) {
        LOGI(TPL_LOG " mprotect 隐藏器已装：orig=%p（PROT_WRITE 请求走裸 svc #226，不转发）",
             reinterpret_cast<void*>(g_mprotectLibcOrig));
    }
}

/* ★r20 关键修复尝试：把「可写的代码页」强制改回 RX。
 *  线索：themis 在 `_INIT_0` 里 `pthread_mutex_init` + `pthread_cond_init` 之后起了一个**常驻看门狗线程**
 *  （入口 FUN_001bf3f8，libthemis 0x1bf3f8；代码做了地址基址混淆，静态追到底困难），周期约 4 s。
 *  已排除「内容比对」（同值写也退）。最省事、最自洽的余下解释：它**周期性读 /proc/self/maps**，看
 *  自己模块里有没有出现「可写又可执行」的页 —— Dobby 装钩后常把代码页留在 RWX。
 *  对策：装钩后（并每 500 ms 复查一次）把 libil2cpp/libunity 里所有 rwx 页用裸 svc 强制改回 R-X。 */
static std::atomic<int> g_reprotectRuns{0};
static void ReProtectExecPages(const char* why) {
    FILE* f = fopen("/proc/self/maps", "re");
    if (!f) return;
    char line[512];
    int fixed = 0, seen = 0;
    while (fgets(line, sizeof(line), f)) {
        /* ★r20 扩展：不只模块内，**匿名 RWX 页**（Dobby 跳板页）同样归一化 —— 若看门狗统计的是
         * 「进程里可写可执行的页」，那它一样会看见 Dobby 的匿名跳板。 */
        const bool inModule = strstr(line, "libil2cpp.so") || strstr(line, "libunity.so") ||
                              strstr(line, "libthemis.so");
        const bool anon = (strchr(line, '/') == nullptr);
        if (!inModule && !anon) continue;
        unsigned long s = 0, e = 0; char perms[8] = {0};
        if (sscanf(line, "%lx-%lx %7s", &s, &e, perms) != 3) continue;
        if (!strchr(perms, 'w') || !strchr(perms, 'x')) continue;
        seen++;
        if (SvcMprotect(reinterpret_cast<void*>(s), static_cast<size_t>(e - s),
                        PROT_READ | PROT_EXEC) == 0) {
            fixed++;
            LOGW(TPL_LOG " reprotect(%s): %lx-%lx %s -> R-X", why, s, e, perms);
        } else {
            LOGW(TPL_LOG " reprotect(%s): %lx-%lx %s 改 RX 失败 errno=%d", why, s, e, perms, errno);
        }
    }
    fclose(f);
    const int k = g_reprotectRuns.fetch_add(1) + 1;
    if (k <= 8 || fixed) LOGW(TPL_LOG " reprotect #%d why=%s 发现 rwx=%d 已改=%d", k, why, seen, fixed);
}
static void FlushUncaughtFaultRecords();   // ★r24 前置声明（定义在 GuardThread 之后）
static void* GuardThread(void*) {
    for (;;) {
        usleep(500 * 1000);
        if (!(SafeMask() & 4)) ReProtectExecPages("tick");   // ★r27 二分：bit2=4 关闭
        FlushThreadRecords("tick");     // ★r20b：线程记账的**打印**放在这里（钩子体内禁止打日志）
        FlushExitGuardRecords("tick");  // ★r20c：exit_group 命中的模块解析与打印也在这里（信号处理器里禁止打日志）
        FlushUncaughtFaultRecords();    // ★r24：保护区外崩溃现场的「模块+偏移」解析（信号处理器里不能 fopen maps）
        RearmFaultGuard();              // ★r24：抢回 SIGSEGV/SIGBUS/SIGILL 链头（themis 后装载会顶掉我们）
    }
    return nullptr;
}
// ★r24：把未捕获崩溃现场解析成 `模块+0x偏移`（和静态反汇编/IG5 的 vaddr 可直接对齐）
static void FlushUncaughtFaultRecords() {
    uint32_t w = g_ucFaultW.load(std::memory_order_acquire);
    uint32_t done = g_ucFaultFlushed.load(std::memory_order_relaxed);
    if (w == done) return;
    if (w - done > 8u) done = w - 8u;          // 环形覆盖：只保留最后 8 条
    while (done != w) {
        const UncaughtFaultRec* r = &g_ucFaults[done & 7u];
        char pc[128], lr[128], sp[128];
        ModOff(r->pc, pc, sizeof(pc));
        ModOff(r->lr, lr, sizeof(lr));
        ModOff(r->sp, sp, sizeof(sp));
        LOGW(TPL_LOG " ★未捕获崩溃定位 #%u sig=%d tid=%d addr=%p pc=%s lr=%s sp=%s",
             done, r->sig, r->tid, (void*)r->addr, pc, lr, sp);
        ++done;
    }
    g_ucFaultFlushed.store(w, std::memory_order_release);
}

static void StartSoftDirtyCleaner() {   // 复用原入口：把「清脏位线程」升级成「页权限守护线程」
    static std::atomic<bool> once{false};
    if (once.exchange(true)) return;
    pthread_t t;
    if (pthread_create(&t, nullptr, GuardThread, nullptr) == 0) {
        pthread_detach(t);
        LOGI(TPL_LOG " 页权限守护线程已起（每 500 ms 把 libil2cpp/libunity 的 rwx 页改回 R-X）");
    } else {
        LOGW(TPL_LOG " 页权限守护线程创建失败");
    }
}

/* ★r20 关键修复：**冻结 themis 看门狗线程**（本轮最终解法）。
 *  定位过程（可复核）：① IG5 静态确认 themis 在 `_INIT_0`（构造函数）里 `pthread_mutex_init` +
 *  `pthread_cond_init` 后起了一个常驻线程（入口 libthemis+0x1bf3f8），是它唯一的周期线程；
 *  ② root 采样 `/proc/<pid>/task/<tid>/syscall` 的 PC，唯一长期停在 **libthemis+0xc16e0** 的线程就是它；
 *  ③ 本地反汇编 0xc16a0-0xc1800 显示 0xc16d8/0xc16dc 正是 `mov w8,#101(nanosleep); svc #0`，
 *  其后 0xc16e0 起是返回点 —— 「睡 N 秒 → 巡检 → 再睡」的循环。
 *  做法：把 0xc16c4（原 `sxtw x8,w8`，机器码 0x93407d08）改成 `movz x8,#0xffff,lsl #16`
 *  （0xD2BFFFE8 ≈ 4294901760 秒 ≈ 136 年）⇒ 看门狗第一次睡下去就再也不醒，之后既不巡检也不会
 *  `exit_group`。补丁前按机器码**指纹校验**，不符就不动（换版本时自动失效而不是改坏）。 */
/* themis 基址取法：**不能走 /proc/self/maps** —— 实测应用侧（同 uid）在自己的 maps 里
 *  **看不到 libthemis.so 的任何行**（themis 隐藏了自身映射；root 从外部 cat 才看得到），
 *  框架的 xdl_open 也因此报 `xdl_open failed for libthemis.so`。
 *  改用系统链接器自己认识已加载库这一点：`dlopen("libthemis.so", RTLD_NOLOAD)` 拿句柄 →
 *  `dlsym(handle,"JNI_OnLoad")` 拿到**模块内地址** → 从该页起**回扫找 ELF 头**（\x7fELF）得基址。 */
static uintptr_t FindThemisBase() {
    void* h = dlopen("libthemis.so", RTLD_NOW | RTLD_NOLOAD);
    if (!h) return 0;
    void* sym = dlsym(h, "JNI_OnLoad");
    dlclose(h);
    if (!sym) return 0;
    uintptr_t a = reinterpret_cast<uintptr_t>(sym) & ~static_cast<uintptr_t>(0xFFF);
    for (int i = 0; i < 0x400; i++, a -= 0x1000) {          // 回扫 ≤4 MB（模块本身 ~850 KB）
        if (memcmp(reinterpret_cast<const void*>(a), "\x7f" "ELF", 4) == 0) return a;
    }
    return 0;
}
static bool FreezeThemisWatchdog() {
    const uintptr_t base = FindThemisBase();
    if (!base) { LOGW(TPL_LOG " 冻结看门狗：找不到 libthemis 基址 ⇒ 稍后重试"); return false; }
    const uint32_t* p = reinterpret_cast<const uint32_t*>(base + 0xc16c4);
    const uint32_t cur = *p;
    LOGW(TPL_LOG " 冻结看门狗：themis 基址=%p 目标机器码=%08x（期望 93407d08）",
         reinterpret_cast<void*>(base), cur);
    if (cur != 0x93407d08u) {
        LOGW(TPL_LOG " 冻结看门狗：指纹不符 ⇒ 不动（该版本可能已改）");
        return false;
    }
    const bool ok = CodePatchU32("watchdog.freeze",
                                 const_cast<uint32_t*>(p), 0xD2BFFFE8u);
    LOGW(TPL_LOG " 冻结看门狗：改写 %s（睡眠改 ≈136 年）", ok ? "成功" : "失败");
    if (ok) g_watchdogFrozen.store(true);
    return ok;
}

/* ==== ⑦.6g ★r20b：看门狗线程「拒绝出生」闸（纯 libc 侧，本轮换的方法）====
 *  为什么不再用 r20 的「冻结」：冻结＝把 themis+0xc16c4 改成 `movz x8,#0xffff,lsl#16`（睡 ≈136 年），
 *    · 它只掐**一个 sleep 点**，而循环里还可能有别的检查点/别的线程（r20 「下一步 1」已怀疑）；
 *    · 更要紧：它**往 themis 自己的 .text 写**，而 L9 判决实验已证触发源是「写代码页这一事件本身」
 *      ⇒ 解决动作与触发动作**同类**。这是「改写成功却仍退出」的最自洽解释，也是本闸要绕开的东西。
 *  本闸做的是「在源头不让巡检线程诞生」：挂 libc `pthread_create`，当 `start_routine` 落在 themis 内
 *    （或等于指定偏移）时**直接 `return 0`（假装成功）而不建线程** ⇒ 一次掐掉该线程的**全部** sleep /
 *    检查点，且**完全不碰 themis 任何代码页**（libc 是分级实测 L5 唯一验证过的安全面）。
 *  仓外同机理依据（已落 `10_docs/_state/外部方案台账.md` §1 X-13）：看雪 thread-289359（libmsaoaidsec.so
 *    型检测线程）＝「Hook pthread_create，识别到来自该 SO 的创建请求就 return 0，反调试线程永不创建」；
 *    看雪 thread-284838 补充：检测方会**核对 `start_routine` 参数地址**，非预期地址会反复重建 ⇒ 拒绝闸
 *    必须能按**偏移**精确打击（故提供 mode 2），不要只会按「来自哪个 so」一刀切。
 *  分两轮，**同一份 so**，第二轮只改 setprop、不重新出包：
 *    round-1（默认 `debug.starry.nhsy2_nothread=0`）⇒ **只记账不拒绝**：先拿到 themis 起了几个线程、
 *      entry 偏移各是多少（日志 `pc#N fn=… arg=… lr=…`、落在 themis 的打成 `**themis+0x…**`）。
 *    round-2 `=1` 拒绝 themis 全部线程；`=0x<偏移>`（如 `0x1bf3f8`）只拒绝该 entry。逃生门 `=-1` 不装闸。
 *  ★ 钩子体纪律（针对 r20「装 libc 探针 ⇒ mod 线程在装钩前死锁」事故，务必保持）：
 *    钩子内**零锁、零日志、零系统调用、零内存分配、零 dladdr/FindThemisBase**（后者会碰链接器锁）。
 *    记账只写定长数组（写完再以 release 发布序号），**打印一律留给我方守护线程** FlushThreadRecords()。 */
static constexpr int kTcRecMax = 1024;      // ★r20b：64 太小 —— 实测 themis 装载后 10 s 就爆表（第 65 次创建）
static constexpr int kTcUniqMax = 128;      // 只打印「首次出现的 (fn,lr)」，避免 1024 条刷屏
static constexpr uintptr_t kVaMask = 0x0000FFFFFFFFFFFFull;   // 去掉 PAC 签名（高 16 位）
struct TcRec { uintptr_t fn, arg, lr; };
static TcRec g_tcRec[kTcRecMax];
static std::atomic<uint32_t> g_tcSeq[kTcRecMax];      // 0=未就绪；n=第 n 条已写完（release 发布）
static std::atomic<uint32_t> g_tcN{0};                // 已观测到的线程创建总次数（可 > kTcRecMax）
static std::atomic<uint32_t> g_tcFlushed{0};
static std::atomic<int>      g_ntOff{0};              // mode 2 的目标 entry 偏移
static std::atomic<bool>     g_pcGateArmed{false};
static std::atomic<uintptr_t> g_themisHi{0};          // themis 上界（ELF phdr 实算）
static int (*g_pcGateOrig)(void*, const void*, void* (*)(void*), void*) = nullptr;

static int Hooked_PthreadCreateGate(void* a, const void* attr, void* (*fn)(void*), void* arg) {
    const uint32_t i = g_tcN.fetch_add(1, std::memory_order_relaxed);
    if (i < kTcRecMax) {
        g_tcRec[i].fn  = reinterpret_cast<uintptr_t>(fn);
        g_tcRec[i].arg = reinterpret_cast<uintptr_t>(arg);
        g_tcRec[i].lr  = reinterpret_cast<uintptr_t>(__builtin_return_address(0));
        g_tcSeq[i].store(i + 1, std::memory_order_release);
    }
    const int mode = g_ntMode.load(std::memory_order_relaxed);
    if (mode) {
        const uintptr_t lo = g_themisBase.load(std::memory_order_relaxed);
        const uintptr_t hi = g_themisHi.load(std::memory_order_relaxed);
        /* ★ 必须先去 PAC：实测本机 fn 可能是签名指针（形如 0xab0000xxxxxxxxxx），
         *   原样比较必然落在区间外 ⇒ 拒绝闸静默失效。 */
        const uintptr_t p  = reinterpret_cast<uintptr_t>(fn) & kVaMask;
        if (lo && hi > lo && p >= lo && p < hi) {
            if (mode == 1 || static_cast<int>(p - lo) == g_ntOff.load(std::memory_order_relaxed)) {
                g_tcRefused.fetch_add(1, std::memory_order_relaxed);
                return 0;                            // ★ 假装成功：巡检线程永不诞生
            }
        }
    }
    return g_pcGateOrig ? g_pcGateOrig(a, attr, fn, arg) : -1;
}

/* ELF phdr 实算模块跨度：**只允许在我方线程调用**。拍脑袋的固定窗口（曾用 4 MB）会把邻居模块
 *  算进 themis 区间 ⇒ 拒绝闸可能误伤别的库的线程。此模块实测仅 ~0xD8000，必须按实际来。 */
static uintptr_t ElfSpanEnd(uintptr_t base) {
    const uint8_t* p = reinterpret_cast<const uint8_t*>(base);
    if (memcmp(p, "\x7f" "ELF", 4) != 0 || p[4] != 2) return 0;      // 64-bit only
    uint64_t phoff = 0; uint16_t phentsize = 0, phnum = 0;
    memcpy(&phoff, p + 32, 8);
    memcpy(&phentsize, p + 54, 2);
    memcpy(&phnum, p + 56, 2);
    if (!phoff || phentsize < 56 || phnum == 0 || phnum > 64) return 0;
    uintptr_t maxEnd = 0;
    for (uint16_t i = 0; i < phnum; i++) {
        const uint8_t* ph = p + phoff + static_cast<uint64_t>(i) * phentsize;
        uint32_t type = 0; memcpy(&type, ph, 4);
        if (type != 1) continue;                                     // PT_LOAD
        uint64_t vaddr = 0, filesz = 0, memsz = 0;
        memcpy(&vaddr, ph + 16, 8);
        memcpy(&filesz, ph + 32, 8);
        memcpy(&memsz,  ph + 40, 8);
        const uint64_t end = vaddr + (memsz > filesz ? memsz : filesz);
        if (base + end > maxEnd) maxEnd = base + end;
    }
    return maxEnd;
}

/* themis 区间刷新：**只允许在我方线程调用**（含 dlopen/dlsym，会碰链接器锁）。 */
static void RefreshThemisRange() {
    if (g_themisHi.load() > g_themisBase.load()) return;
    const uintptr_t b = FindThemisBase();
    if (!b) return;
    uintptr_t end = ElfSpanEnd(b);
    if (end <= b) end = b + 0x200000;    // 兜底 2 MB（phdr 读失败时）
    g_themisBase.store(b);
    g_themisHi.store(end);
    LOGW(TPL_LOG " themis 区间已定：base=%p hi=%p（跨度 %lu KB，源自 ELF phdr）",
         reinterpret_cast<void*>(b), reinterpret_cast<void*>(end),
         static_cast<unsigned long>((end - b) / 1024));
}

static void FlushThreadRecords(const char* why) {
    const uint32_t n = g_tcN.load(std::memory_order_relaxed);
    const uint32_t f = g_tcFlushed.load(std::memory_order_relaxed);
    static std::atomic<int>  lastRefused{-1};
    static std::atomic<bool> baseLogged{false};
    static std::atomic<bool> ovLogged{false};
    static uintptr_t uniq[kTcUniqMax][2];
    static std::atomic<int> nUniq{0};
    const int rf = g_tcRefused.load(std::memory_order_relaxed);
    const bool newRec = (n > f);
    const bool refChanged = (rf != lastRefused.load());
    if (!newRec && !refChanged) return;
    RefreshThemisRange();
    const uintptr_t lo = g_themisBase.load(std::memory_order_relaxed);
    const uintptr_t hi = g_themisHi.load(std::memory_order_relaxed);
    const uint32_t cap = (n < static_cast<uint32_t>(kTcRecMax)) ? n : static_cast<uint32_t>(kTcRecMax);
    for (uint32_t i = f; i < cap; i++) {
        if (g_tcSeq[i].load(std::memory_order_acquire) != i + 1) break;   // 该条还没写完
        const TcRec& r = g_tcRec[i];
        g_tcFlushed.store(i + 1, std::memory_order_relaxed);              // 先推进游标，防重复打印
        const uintptr_t ufn = r.fn & kVaMask, ulr = r.lr & kVaMask;
        /* 去重：1024 条里绝大多数是同几个 (fn,lr) 的重复（Unity/音频/GC 线程池），只打印首次。 */
        const int nu = nUniq.load();
        bool dup = false;
        for (int k = 0; k < nu && k < kTcUniqMax; k++) {
            if (uniq[k][0] == ufn && uniq[k][1] == ulr) { dup = true; break; }
        }
        if (dup) continue;
        if (nu < kTcUniqMax) { uniq[nu][0] = ufn; uniq[nu][1] = ulr; nUniq.store(nu + 1); }
        if (lo && hi > lo && ufn >= lo && ufn < hi) {
            LOGW(TPL_LOG " %s pc#%u **themis+0x%lx**（疑巡检线程）fn=%p arg=%p lr=%p", why, i + 1,
                 static_cast<unsigned long>(ufn - lo), reinterpret_cast<void*>(ufn),
                 reinterpret_cast<void*>(r.arg), reinterpret_cast<void*>(ulr));
        } else {
            LOGW(TPL_LOG " %s pc#%u fn=%p arg=%p lr=%p（lr 是否在 themis：%s）", why, i + 1,
                 reinterpret_cast<void*>(ufn), reinterpret_cast<void*>(r.arg),
                 reinterpret_cast<void*>(ulr),
                 (lo && hi > lo && ulr >= lo && ulr < hi) ? "是 ★" : "否");
        }
    }
    if (!baseLogged.load() && lo) {
        baseLogged.store(true);
        LOGW(TPL_LOG " %s THEMIS base=%p hi=%p（mode=%d off=+0x%x）", why, reinterpret_cast<void*>(lo),
             reinterpret_cast<void*>(hi), g_ntMode.load(), g_ntOff.load());
    }
    if (refChanged) {
        lastRefused.store(rf);
        LOGW(TPL_LOG " %s 已拒绝出生 themis 线程=%d（mode=%d off=+0x%x 观测创建=%u）",
             why, rf, g_ntMode.load(), g_ntOff.load(), n);
    }
    if (n > static_cast<uint32_t>(kTcRecMax) && !ovLogged.exchange(true)) {
        LOGW(TPL_LOG " %s 线程创建次数 %u 超记账上限 %d ⇒ 只记前 %d 条", why, n, kTcRecMax, kTcRecMax);
    }
}

/* 中和判据：冻结成功，或「拒绝出生」已确实发生过一次（mode≠0 且 refused>0），
 *  ★r20c 或 **exit_group 闸已在「就地停住」档（≥3）** —— 那一档能让任何检查器的裸 svc 退出失效，
 *  所以**不必再往 themis 代码页写**（r20 的冻结法本身就是"写代码页"这件事，是我们要避开的动作面）。
 *  实测依据：mode=3 下 3 个登录补丁 + 真凶线程被停住，进程一路走到 `native menu shown` / `LoginService: SDK Initialized`。 */
static bool ThemisWatchdogNeutralized() {
    if (g_watchdogFrozen.load()) return true;
    if (g_exitGuardMode.load() >= 3) return true;
    return g_ntMode.load() != 0 && g_tcRefused.load() > 0;
}

static void InstallPthreadCreateGate() {
    if (g_pcGateArmed.load()) return;
    std::lock_guard<std::mutex> lk(g_installMutex);
    if (g_pcGateArmed.load()) return;
    g_pcGateArmed.store(true);
    char buf[PROP_VALUE_MAX] = {0};
    const int n = __system_property_get("debug.starry.nhsy2_nothread", buf);
    /* ★r20c 收官定案：**默认 0xC1240**（只拒 themis 看门狗这一个 entry，不碰别的 themis 线程/业务线程）。 */
    const long v = (n > 0) ? strtol(buf, nullptr, 0) : 0xC1240;
    if (v < 0) { LOGI(TPL_LOG " pthread_create 闸：属性=%ld ⇒ 逃生门生效，不装闸（回到 r20 冻结路径）", v); return; }
    if (v == 1)      g_ntMode.store(1);
    else if (v > 1) { g_ntOff.store(static_cast<int>(v)); g_ntMode.store(2); }
    else             g_ntMode.store(0);
    void* pp = getSymAddress("libc.so", "pthread_create", false);
    if (!pp) { g_ntMode.store(0); LOGW(TPL_LOG " pthread_create 闸：符号不可得 ⇒ 跳过（退化为只靠冻结）"); return; }
    if (DobbyHookGuarded("pthread_create.gate", pp, reinterpret_cast<void*>(Hooked_PthreadCreateGate),
                         reinterpret_cast<void**>(&g_pcGateOrig))) {
        LOGI(TPL_LOG " pthread_create 闸已装：orig=%p mode=%d(0=只记账/1=拒 themis 全部/2=只拒一个) off=+0x%x",
             reinterpret_cast<void*>(g_pcGateOrig), g_ntMode.load(), g_ntOff.load());
    } else {
        g_ntMode.store(0);
        LOGW(TPL_LOG " pthread_create 闸装失败（DobbyHookGuarded=false）⇒ 退化为只记账");
    }
}

/* ==== ⑦.6h ★r20c：`exit_group` 的 seccomp 闸（指名真凶 + 可选就地中和）====
 *  现场（r20b 实测）：任一 libil2cpp 代码页补丁之后 **固定 4.427/4.441/4.440 s** 干净退出；退出**不经 libc**
 *    （`exit`/`_exit` 护盾从未命中）⇒ 凶手用**裸 `svc exit_group`**；且退出时 `libthemis.so` 尚未 dlopen
 *    ⇒ 真凶不是 themis 看门狗（r20 的核心归属已否证）。
 *  裸 svc 在用户态**钩不到**（没有函数可挂），但在**内核边界**可以拦：seccomp 过滤器按 syscall 号命中，
 *    并用 `SECCOMP_FILTER_FLAG_TSYNC` **一次同步到全部现有线程**（此后新建线程自动继承）
 *    ⇒ 不需要知道凶手是哪个线程、也不需要它由谁创建，这正好补上 r20b「按线程找巡检者」失败的那一环。
 *  五档 prop `debug.starry.nhsy2_exitguard`（★r24 起默认 4 = RET_ERRNO(EPERM)，见函数内长注释）：
 *    1 = `SECCOMP_RET_LOG`：只记录。内核把 `syscall=94 … comm=` 写进 audit，而本机 **audit 确实进 logcat**
 *        （实测 `logcat -b events | grep -c audit` = 318，**记录里带 `ip=`**）⇒ 直接**点名凶手线程**（最安全，先跑这一档）。
 *    2 = `SECCOMP_RET_TRAP`：**就地中和**。内核给该线程发 SIGSYS，我在处理器里只做「记 PC + 把 x0 置 -EPERM」
 *        （异步信号安全：只写定长数组，不打日志、不分配），返回后 **exit_group 不执行 ⇒ 进程不退**；
 *        模块+RVA 由守护线程下一拍解析打印。
 *    3 = `SECCOMP_RET_TRAP` + **把该线程就地停住**（pc 改指向 `ParkTrappedThread`）：实测该 syscall 桩页
 *        `svc` 之后是**全零**（AArch64 解成 `udf #0`）⇒ 第 2 档放它继续执行会 SIGILL 崩死；
 *        第 3 档不让它执行桩页后续字节 ⇒ 进程可望**存活**（代价：若该线程持锁则可能拖住别人）。
 *  记录内容（第 2/3 档）：pc（桩页内）+ **lr=x30（调用桩页的返回地址＝真凶本人所在模块+RVA）** + fp + sp + tid + 栈字。
 *  边界：① 只拦 `exit_group`（不碰其它 syscall）；② 第 2/3 档会连**合法退出**一起拦（实验档，默认关）；
 *    ③ 凶手若换成 `tgkill`/`_exit` 等别的路，本闸失效（届时看 audit 里换成了哪个 syscall）。 */

/* 地址 → 「模块名+文件相对偏移（含段权限）」。只在我方线程调用（要 fopen/malloc）。 */
static void ResolvePcToModule(uintptr_t pc, char* out, size_t cap) {
    out[0] = '\0';
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) { snprintf(out, cap, "%p(打不开 maps)", reinterpret_cast<void*>(pc)); return; }
    char line[512];
    while (fgets(line, sizeof(line), f)) {
        unsigned long long s = 0, e = 0, off = 0; char perms[8] = {0};
        if (sscanf(line, "%llx-%llx %7s %llx", &s, &e, perms, &off) != 4) continue;
        if (pc < s || pc >= e) continue;
        const char* slash = strrchr(line, '/');
        if (slash) {
            char name[128];
            snprintf(name, sizeof(name), "%s", slash + 1);
            char* nl = strchr(name, '\n'); if (nl) *nl = '\0';
            snprintf(out, cap, "%s+%#llx [段 %s 起 %#llx]", name,
                     static_cast<unsigned long long>(off + (pc - s)), perms, s);
        } else {
            snprintf(out, cap, "[anon]+%#llx [段 %s 起 %#llx]",
                     static_cast<unsigned long long>(pc - s), perms, s);
        }
        break;
    }
    fclose(f);
    if (!out[0]) snprintf(out, cap, "%p(不在任何映射内)", reinterpret_cast<void*>(pc));
}

static constexpr int kEgRecMax = 8;
static constexpr int kEgStackWords = 16;
struct EgRec {
    uintptr_t pc, lr, fp, sp, tid;
    uintptr_t stk[kEgStackWords];
};
static EgRec g_egRec[kEgRecMax];
static std::atomic<uint32_t> g_egSeq[kEgRecMax];
static std::atomic<uint32_t> g_egN{0};
static std::atomic<uint32_t> g_egFlushed{0};
static std::atomic<int> g_egBlocked{0};
static std::atomic<bool> g_exitGuardArmed{false};

/* ★r20c 第 3 档用：被拦下的线程就地处「永久等待」，**绝不执行桩页 svc 之后的字节**
 *  （实测那里的内容是 0 ⇒ AArch64 解成 `udf #0` ⇒ 直接 SIGILL 崩死，这就是第 2 档仍"死"的原因）。
 *  永不返回；故意不加任何同步/日志（在信号处理器里被跳转过来）。 */
static void ParkTrappedThread() {
    for (;;) { __asm__ __volatile__("wfe" ::: "memory"); }
}

/* ★ SIGSYS 处理器：**必须异步信号安全** —— 只写定长数组 + 原子 + 读本线程栈，禁止日志/分配/fopen。
 *  记录 pc（桩页内 svc 之后）、**lr=x30（调用桩页的返回地址＝真凶本人）**、fp=x29、sp、tid（裸 syscall 取）、
 *  以及 sp 上方 16 个栈字（供继续回溯）。sp 之上是本帧已用栈，必定已映射 ⇒ 读取安全。 */
static void OnSigsys(int, siginfo_t*, void* uc_) {
    ucontext_t* uc = reinterpret_cast<ucontext_t*>(uc_);
    const uint32_t i = g_egN.fetch_add(1, std::memory_order_relaxed);
    if (i < kEgRecMax) {
        EgRec& r = g_egRec[i];
        r.pc  = static_cast<uintptr_t>(uc->uc_mcontext.pc);
        r.lr  = static_cast<uintptr_t>(uc->uc_mcontext.regs[30]);
        r.fp  = static_cast<uintptr_t>(uc->uc_mcontext.regs[29]);
        r.sp  = static_cast<uintptr_t>(uc->uc_mcontext.sp);
        r.tid = static_cast<uintptr_t>(syscall(__NR_gettid));
        const uintptr_t* p = reinterpret_cast<const uintptr_t*>(r.sp);
        for (int k = 0; k < kEgStackWords; ++k) r.stk[k] = p[k];
        g_egSeq[i].store(i + 1, std::memory_order_release);
    }
    g_egBlocked.fetch_add(1, std::memory_order_relaxed);
    if (g_exitGuardMode.load(std::memory_order_relaxed) >= 3) {
        uc->uc_mcontext.pc = static_cast<uint64_t>(reinterpret_cast<uintptr_t>(&ParkTrappedThread));
        return;                                // 永不到达（ParkTrappedThread 不返回）—— 线程就地停住
    }
    uc->uc_mcontext.regs[0] = static_cast<uint64_t>(-EPERM);   // 第 2 档：让该次 syscall「失败」
}

static void FlushExitGuardRecords(const char* why) {
    const uint32_t n = g_egN.load(std::memory_order_relaxed);
    const uint32_t f = g_egFlushed.load(std::memory_order_relaxed);
    static std::atomic<bool> armedLogged{false};
    if (n == f) {
        if (!armedLogged.load() && g_exitGuardMode.load() != 0 && g_exitGuardArmed.load()) {
            armedLogged.store(true);
            LOGW(TPL_LOG " %s exit_group 闸：mode=%d 已就位，尚未命中", why, g_exitGuardMode.load());
        }
        return;
    }
    const uint32_t cap = (n < static_cast<uint32_t>(kEgRecMax)) ? n : static_cast<uint32_t>(kEgRecMax);
    for (uint32_t i = f; i < cap; i++) {
        if (g_egSeq[i].load(std::memory_order_acquire) != i + 1) break;
        g_egFlushed.store(i + 1, std::memory_order_relaxed);
        const EgRec& r = g_egRec[i];
        char mpc[192] = {0}, mlr[192] = {0}, mfp[192] = {0};
        ResolvePcToModule(r.pc, mpc, sizeof(mpc));
        ResolvePcToModule(r.lr, mlr, sizeof(mlr));
        ResolvePcToModule(r.fp, mfp, sizeof(mfp));
        char comm[64] = {0}, cpath[96];
        snprintf(cpath, sizeof(cpath), "/proc/self/task/%lu/comm", static_cast<unsigned long>(r.tid));
        if (FILE* cf = fopen(cpath, "re")) {
            if (fgets(comm, sizeof(comm), cf)) { char* nl = strchr(comm, '\n'); if (nl) *nl = '\0'; }
            fclose(cf);
        }
        LOGW(TPL_LOG " %s ★exit_group 命中 #%u：tid=%lu comm=\"%s\"（累计拦下=%d mode=%d）", why, i + 1,
             static_cast<unsigned long>(r.tid), comm, g_egBlocked.load(), g_exitGuardMode.load());
        LOGW(TPL_LOG " %s    ★调用者 lr=%p → %s", why, reinterpret_cast<void*>(r.lr), mlr);
        LOGW(TPL_LOG " %s    桩页 pc=%p → %s", why, reinterpret_cast<void*>(r.pc), mpc);
        LOGW(TPL_LOG " %s    fp=%p → %s / sp=%p", why, reinterpret_cast<void*>(r.fp), mfp,
             reinterpret_cast<void*>(r.sp));
        char buf[640]; int off = 0; int shown = 0;
        buf[0] = '\0';
        for (int k = 0; k < kEgStackWords && shown < 6; ++k) {
            const uintptr_t v = r.stk[k];
            if (v < 0x1000) continue;
            char mm[192] = {0};
            ResolvePcToModule(v, mm, sizeof(mm));
            if (strstr(mm, "libc.so") || strstr(mm, "[anon]") || strstr(mm, "不在任何映射")) continue;
            const int w = snprintf(buf + off, sizeof(buf) - static_cast<size_t>(off), " [%d]%s", k, mm);
            if (w <= 0 || off + w >= static_cast<int>(sizeof(buf)) - 8) break;
            off += w; shown++;
        }
        if (shown) LOGW(TPL_LOG " %s    栈上候选返回地址：%s", why, buf);
    }
    if (n > static_cast<uint32_t>(kEgRecMax)) {
        static std::atomic<bool> ov{false};
        if (!ov.exchange(true)) LOGW(TPL_LOG " %s exit_group 命中 %u 次，超记录上限 %d", why, n, kEgRecMax);
    }
}

static void InstallExitGroupGuard() {
    if (g_exitGuardArmed.load()) return;
    std::lock_guard<std::mutex> lk(g_installMutex);
    if (g_exitGuardArmed.load()) return;
    g_exitGuardArmed.store(true);
    char buf[PROP_VALUE_MAX] = {0};
    const int pn = __system_property_get("debug.starry.nhsy2_exitguard", buf);
    /* ★r20c 收官定案：**默认 3**（不再依赖 setprop）—— 10.15 底包反篡改靠裸 svc `exit_group` 退出，
     *   必须就地停住那个线程功能钩才装得上；用户 2026-10-17 L3 实测（69 钩 + 本档）功能全部有效。prop 仍可覆盖。 */
    /* ★r24（vc646 真机实测，2026-10-25）：**默认改成 4 = RET_ERRNO(EPERM)**。
     *  为什么不再用 3（RET_TRAP + 停住线程）：本版的反篡改 `exit_group` 是**多线程同时**发的
     *  （一次事故里 debuggerd 连开 6+ 个 dumper），RET_TRAP 每命中一次就发一次 SIGSYS，
     *  而我们只在一个线程上把 pc 改到 ParkTrappedThread ⇒ 其余命中仍按致命 SIGSYS 处理，
     *  进程照样死（真机日志：多次 `F DEBUG: signal 31 (SIGSYS), code 1 (SYS_SECCOMP)` 后 `has died`）。
     *  RET_ERRNO 不产生任何信号：内核直接把这次 exit_group 判成 `-EPERM` 返回给调用者，
     *  线程继续跑、不需要信号处理器、也不需要停住谁 ⇒ 多线程同时命中也不会死人。
     *  第 1/2/3 档保留作取证（1=只记账 / 2=TRAP 放行 / 3=TRAP+停住）。 */
    int mode = (pn > 0) ? atoi(buf) : 4;
    if (mode <= 0) { LOGI(TPL_LOG " exit_group 闸：未启用（debug.starry.nhsy2_exitguard=0）"); return; }
    if (mode > 4) mode = 4;
    g_exitGuardMode.store(mode);
    if (mode == 2 || mode == 3) {
        struct sigaction sa;
        memset(&sa, 0, sizeof(sa));
        sa.sa_sigaction = OnSigsys;
        sa.sa_flags = SA_SIGINFO | SA_NODEFER;
        sigemptyset(&sa.sa_mask);
        if (sigaction(SIGSYS, &sa, nullptr) != 0) {
            LOGW(TPL_LOG " exit_group 闸：装 SIGSYS 处理器失败 errno=%d（mode=2 将退化为进程被杀）", errno);
        }
    }
    if (prctl(PR_SET_NO_NEW_PRIVS, 1, 0, 0, 0) != 0) {
        LOGW(TPL_LOG " exit_group 闸：PR_SET_NO_NEW_PRIVS 失败 errno=%d（继续尝试）", errno);
    }
    const uint32_t action = (mode == 4) ? (SECCOMP_RET_ERRNO | (EPERM & SECCOMP_RET_DATA))
                            : (mode >= 2) ? SECCOMP_RET_TRAP
                                          : SECCOMP_RET_LOG;
    struct sock_filter f[7];
    f[0] = BPF_STMT(BPF_LD | BPF_W | BPF_ABS, offsetof(struct seccomp_data, arch));
    f[1] = BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, AUDIT_ARCH_AARCH64, 1, 0);
    f[2] = BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW);
    f[3] = BPF_STMT(BPF_LD | BPF_W | BPF_ABS, offsetof(struct seccomp_data, nr));
    f[4] = BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, __NR_exit_group, 0, 1);
    f[5] = BPF_STMT(BPF_RET | BPF_K, action);
    f[6] = BPF_STMT(BPF_RET | BPF_K, SECCOMP_RET_ALLOW);
    struct sock_fprog prog;
    prog.len = static_cast<unsigned short>(sizeof(f) / sizeof(f[0]));
    prog.filter = f;
    errno = 0;
    const long r = syscall(__NR_seccomp, SECCOMP_SET_MODE_FILTER, SECCOMP_FILTER_FLAG_TSYNC, &prog);
    if (r != 0) {
        const int e1 = errno;
        errno = 0;
        const long r2 = syscall(__NR_seccomp, SECCOMP_SET_MODE_FILTER, 0, &prog);
        const int e2 = errno;
        LOGW(TPL_LOG " exit_group 闸：TSYNC rc=%ld errno=%d ⇒ 退回单线程安装 rc=%ld errno=%d（只覆盖本线程）",
             r, e1, r2, e2);
        if (r2 != 0) { g_exitGuardMode.store(0); LOGW(TPL_LOG " exit_group 闸：安装失败 ⇒ 放弃"); return; }
    }
    LOGI(TPL_LOG " exit_group 闸已装：mode=%d（1=RET_LOG 只记账 / 2=RET_TRAP 放行 / 3=RET_TRAP+停住线程 / 4=RET_ERRNO(EPERM) 无信号中和）",
         mode);
}

/* mprotect 探针：Dobby 改码必须先 mprotect 放开写权限；若 themis 靠「页权限/写入」发现篡改，
 * 这里的 lr 会指出是谁在看。同时也能看到 themis 是否自己回改权限。 */
static int (*g_mprotectOrig)(void*, size_t, int) = nullptr;
static std::atomic<int> g_mpSeen{0};
static int Hooked_Mprotect(void* addr, size_t len, int prot) {
    const int k = g_mpSeen.fetch_add(1) + 1;
    if (k <= 40) {
        LOGW(TPL_LOG " mprotect#%d addr=%p len=%zu prot=%d lr=%p", k, addr, len, prot,
             __builtin_return_address(0));
    }
    return g_mprotectOrig ? g_mprotectOrig(addr, len, prot) : -1;
}

static void InstallMemcmpProbe() {
    if (g_mcProbeArmed.load()) return;
    std::lock_guard<std::mutex> lk(g_installMutex);
    if (g_mcProbeArmed.load()) return;
    g_mcProbeArmed.store(true);
    LogKeyModuleBases();          // 先在进程内留下各模块基址（离线解析 LR 用）
    /* themis 的映射范围：`getLibraryAddress("libthemis.so")` 实测返回 0（拿不到模块表），
     * 改用它自己的导出 `JNI_OnLoad` 作为「落在 themis 里」的地标。 */
    uintptr_t tb = getLibraryAddress("libthemis.so");
    if (tb == 0) {
        void* jl = getSymAddress("libthemis.so", "JNI_OnLoad", false);
        if (jl) tb = reinterpret_cast<uintptr_t>(jl) & ~static_cast<uintptr_t>(0xFFFFF);
    }
    g_themisBase.store(tb);
    void* pm = getSymAddress("libc.so", "memcmp", false);
    if (pm && DobbyHookGuarded("memcmp.probe", pm, reinterpret_cast<void*>(Hooked_Memcmp),
                               reinterpret_cast<void**>(&g_memcmpOrig))) {
        LOGI(TPL_LOG " memcmp 探针已装：orig=%p themisBase=%p（记账阈值 256B；spoof=%d）",
             reinterpret_cast<void*>(g_memcmpOrig), reinterpret_cast<void*>(tb),
             static_cast<int>(MemcmpSpoofEnabled()));
    }
    void* po = getSymAddress("libc.so", "__open_2", false);
    if (po && DobbyHookGuarded("open2.probe", po, reinterpret_cast<void*>(Hooked_Open2),
                               reinterpret_cast<void**>(&g_open2Orig))) {
        LOGI(TPL_LOG " __open_2 探针已装：orig=%p", reinterpret_cast<void*>(g_open2Orig));
    }
    void* pa = getSymAddress("libc.so", "__openat_2", false);
    if (pa && DobbyHookGuarded("openat2.probe", pa, reinterpret_cast<void*>(Hooked_Openat2),
                               reinterpret_cast<void**>(&g_openat2Orig))) {
        LOGI(TPL_LOG " __openat_2 探针已装：orig=%p", reinterpret_cast<void*>(g_openat2Orig));
    }
    /* ★r20b：`pthread_create` 探针已升级为 §⑦.6g 的**闸**（单独挂钩、带「拒绝出生」能力）。
     *  此处不再重复挂钩 —— 同一函数挂两次＝互踩，且 r20 已证四个探针同装必然死锁。 */
    void* pm2 = getSymAddress("libc.so", "mprotect", false);
    if (pm2 && DobbyHookGuarded("mprotect.probe", pm2, reinterpret_cast<void*>(Hooked_Mprotect),
                                reinterpret_cast<void**>(&g_mprotectOrig))) {
        LOGI(TPL_LOG " mprotect 探针已装：orig=%p", reinterpret_cast<void*>(g_mprotectOrig));
    }
}

/* ★r20 L9 判决实验：把 libil2cpp .text 里的 4 字节**按原值写回**（内容零变化，但确实执行了
 *  mprotect + 写代码页）。用途：区分校验的性质 ——
 *    · 仍然退出 ⇒ 检测的是「代码页被写 / 页属性被改」这一**事件**（与内容无关）；
 *    · 正常存活 ⇒ 检测的是**内容**（校验和/逐字节比对），那我们改的字节才是触发源。
 *  目标地址取已解析到的真 API 桩（get_corlib，必在 libil2cpp 可执行段内）。 */
static void InstallSameValueWriteTest() {
    if (!EnsureApi()) { LOGW(TPL_LOG " L9: api 未就绪，跳过"); return; }
    void* t = reinterpret_cast<void*>(GetApi().get_corlib);
    if (!t) { LOGW(TPL_LOG " L9: get_corlib 为空，跳过"); return; }
    uint32_t w = 0;
    memcpy(&w, t, sizeof(w));
    LOGW(TPL_LOG " L9 同值写测试：target=%p 现值=%08x（写回同值，内容不变）", t, w);
    const bool ok = CodePatchU32("L9.same", t, w);
    LOGW(TPL_LOG " L9 同值写结果=%d（1=已执行 mprotect+写页）", static_cast<int>(ok));
    ClearSoftDirty("L9-after-write");
    {   // 打印目标所在页的权限（验证 CodePatchU32 的 RX 还原是否真的生效）
        FILE* mf = fopen("/proc/self/maps", "re");
        if (mf) {
            char ln[512];
            const unsigned long tgt = reinterpret_cast<unsigned long>(t);
            while (fgets(ln, sizeof(ln), mf)) {
                unsigned long s = 0, e = 0; char pm[8] = {0};
                if (sscanf(ln, "%lx-%lx %7s", &s, &e, pm) != 3) continue;
                if (tgt >= s && tgt < e) { LOGW(TPL_LOG " L9 目标页权限=%s (%lx-%lx)", pm, s, e); break; }
            }
            fclose(mf);
        }
    }
}

//  ⑧ 对外接口（ModEntry.h 注册表契约，签名不可改）
// ============================================================================
bool nhsy2_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;

    /* ★r27 最小化二分开关 `debug.starry.nhsy2_safemask`（默认 0 = 正常行为）：
     *   1  = 不装 SIGSEGV/SIGBUS/SIGILL 处理器（含备用信号栈）
     *   2  = 不起守护线程（页权限/rearm/取证 flush 全不跑）
     *   4  = 守护线程里**不跑** ReProtectExecPages（mprotect 扫 rwx 页），其余照跑
     *   8  = 不解析符号、不装任何功能钩（SyncHooks / 登录保命钩全跳过）
     *   16 = 不装 libc 侧护盾（日志护盾 / exit 护盾 / exit_group seccomp 闸 / pthread_create 闸）
     *   31 = SAFE_MODE：只加载 so + 菜单可用，其余全关。
     * 用途：本版（vc646）themis 的崩溃处理器是坏的（任何真实 SIGSEGV 都会引爆无限递归），
     *   要靠这个把「谁在造故障」二分出来。日志 `safe mask=N` 便于对齐每一档。 */
    const int safemask = SafeMask();
    LOGI(TPL_LOG " safe mask=%d（0=正常）", safemask);
    if (!(safemask & 1)) InstallFaultGuard();     // §④ 先装兜底，再装钩
    // ★r20 注：此刻 libthemis 往往还没被 TapTap SDK dlopen ⇒ 冻结须在 tick 里重试（见 SyncHooks）。
    if (!(safemask & 2)) StartSoftDirtyCleaner(); // ★r20 页权限守护线程
    // ★r20 注意：`InstallMprotectHider()`（给 libc mprotect 打钩）实测**会把 Dobby 后续装钩一起带down**
    //   （日志停在它那一步，之后的 `探针已装` / `install pass` 全无）⇒ 已停用，只保留页权限守护。
    // InstallMprotectHider();
    if (!(safemask & 16)) {
        InstallLogAbortShield();  // §⑦.6 进程级日志护盾：坏格式串不再让 libc assert→abort
        InstallExitShield();      // §⑦.6c r20：拦截 exit(0)/_exit(0)（libil2cpp 完整性校验命中的干净退出）
    }
    // ★r20c：`exit_group` 的 seccomp 闸（§⑦.6h）。它拦的是**内核边界**上的裸 svc —— r20b 已证真凶不走 libc、
    //   也不在 themis 里，用户态没有可挂的函数，只有 seccomp 能按 syscall 号拦；TSYNC 一次覆盖全部现有线程。
    //   默认 prop=0 ⇒ 不装（行为与之前完全一致），要取证/中和必须显式 setprop。
    if (!(safemask & 16)) InstallExitGroupGuard();
    // ★r20：`InstallMemcmpProbe()`（memcmp/__open_2/__openat_2/mprotect 四个 libc 探针）实测
    //   会让紧接着的 `SyncHooks()` 卡死（日志正好停在最后一个「探针已装」之后，之后再无输出，
    //   而游戏本体照常运行 ⇒ mod 线程死锁）。且其 pthread_create 部分已由 §⑦.6g 的闸取代 ⇒ 保持停用。
    // InstallMemcmpProbe();
    if (DiagLevel() == 9) InstallSameValueWriteTest();   // ★L9 判决实验（同值写）
    g_libReady.store(true);
    if (!(safemask & 8)) SyncHooks();   // 已 ON 的开关立即装；全关保持 0 hook
    // ★r20b：只挂**一个** libc 函数（pthread_create，§⑦.6g 的「拒绝出生」闸）。放在这里是为了最大
    //   提前量 —— themis 是 TapTap SDK 稍后才 dlopen 的，此刻闸已就位就能拦住它的巡检线程诞生；
    //   同时它排在 SyncHooks 之后（SyncHooks 才是历史上会卡死的那个），爆炸半径最小。
    if (!(safemask & 16)) InstallPthreadCreateGate();
    if (!(safemask & 8)) InstallLoginAssistHooks();   // §6.5 登录保命：与开关无关，必须在登录页出现前就位
    LOGI(TPL_LOG " lib ready (by-name hooks only, lazy install)");
    return true;
}

bool nhsy2_IsReady() {
    return g_libReady.load();
}

void nhsy2_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case NHSY2_FEAT_DMG:  g_dmg.store(enabled);  break;
        case NHSY2_FEAT_INV:  g_inv.store(enabled);  break;
        case NHSY2_FEAT_FULL: g_full.store(enabled); break;   // Attri 读点强制（§6.6z）
        case NHSY2_FEAT_STD:  g_std.store(enabled);  break;
        case NHSY2_FEAT_BOMB:    g_bomb.store(enabled);    break;
        case NHSY2_FEAT_COIN:    g_coin.store(enabled);    break;
        case NHSY2_FEAT_CRYSTAL: g_crystal.store(enabled); break;
        case NHSY2_FEAT_KEY:     g_key.store(enabled);     break;
        case NHSY2_FEAT_CKEY:    g_ckey.store(enabled);    break;
        case NHSY2_FEAT_SPD:  g_spd.store(enabled);  break;
        case NHSY2_FEAT_JUMP: g_jump.store(enabled); break;
        default: break;
    }
    LOGI(TPL_LOG " feature[%d] = %d", featNum, (int)enabled);
    SyncHooks();
}

void nhsy2_OnValueChanged(int featNum, int value) {
    if (featNum == NHSY2_FEAT_DMG) {
        if (value < 1) value = 1;
        if (value > NHSY2_DMG_MUL_MAX) value = NHSY2_DMG_MUL_MAX;   // 1..100
        g_dmgMul.store(value);
        LOGI(TPL_LOG " damage multiplier = %d", value);
    } else if (featNum == NHSY2_FEAT_SPD) {
        if (value < 1)  value = 1;
        if (value > 10) value = 10;
        g_spdMul.store(value);
        LOGI(TPL_LOG " speed multiplier = %d", value);
    }
    // 滑块本身就是开关（§③.1）：拉到 >1 立即补一次装钩，不必等下一次 Tick。
    // SyncHooks 幂等（g_installPassDone 快路径 + AnyFeatureOn 门禁），重复调用零成本。
    SyncHooks();
}

void nhsy2_Tick(const bool* features, const int* values) {
    if (!features) return;

    g_dmg.store(features[NHSY2_FEAT_DMG]);
    g_inv.store(features[NHSY2_FEAT_INV]);
    g_full.store(features[NHSY2_FEAT_FULL]);
    g_std.store(features[NHSY2_FEAT_STD]);
    g_bomb.store(features[NHSY2_FEAT_BOMB]);
    g_coin.store(features[NHSY2_FEAT_COIN]);
    g_crystal.store(features[NHSY2_FEAT_CRYSTAL]);
    g_key.store(features[NHSY2_FEAT_KEY]);
    g_ckey.store(features[NHSY2_FEAT_CKEY]);
    g_spd.store(features[NHSY2_FEAT_SPD]);
    g_jump.store(features[NHSY2_FEAT_JUMP]);

    if (values) {
        int v = values[NHSY2_FEAT_DMG];
        if (v < 1) v = 1;
        if (v > NHSY2_DMG_MUL_MAX) v = NHSY2_DMG_MUL_MAX;   // 1..100
        g_dmgMul.store(v);

        int sv = values[NHSY2_FEAT_SPD];
        if (sv < 1) sv = 1;
        if (sv > 10) sv = 10;
        g_spdMul.store(sv);
    }

    // 兜底：库晚于首帧映射时，这里补一次
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) {
        InstallFaultGuard();
        g_libReady.store(true);
        LOGI(TPL_LOG " lib ready (via Tick fallback)");
    }
    if (!(SafeMask() & 8)) GuardedVoid("tick.syncHooks", [&]{ SyncHooks(); });
    // ★r24：登录保命钩的补装也在 mod 线程里跑；解析/装钩崩了同样会带走整个进程 ⇒ 纳入保护区
    if (!(SafeMask() & 8))
        GuardedVoid("tick.loginAssist", [&]{ InstallLoginAssistHooks(); });   // §6.5 幂等重试（每 200ms）

    // §6.5 观察窗（只记日志，不碰游戏状态）：兜住 5040 之后 60 s 还没等到成功回调 ⇒ 提示重试
    {
        const long long stamp = g_loginLastGraceMs.load();
        if (stamp != 0 && g_loginAssist.load() && stamp != g_loginGraceStampLogged) {
            const long long now2 = NowMonoMs();
            if (now2 - stamp >= static_cast<long long>(NHSY2_LOGIN_GRACE_GAP_MS)) {
                g_loginGraceStampLogged = stamp;
                LOGW(TPL_LOG " login assist 观察：兜住 5040 已 %lld ms 仍未收到成功回调"
                             " ⇒ 若界面停在登录页/无响应，请再点一次登录（网页此时已有会话 cookie，通常几秒完成）",
                     now2 - stamp);
            }
        }
    }

    // 标准版：单例权益字段的轻量维持（每 200ms 一次，非热路径；★r24 同纳入保护区）
    if (g_std.load()) GuardedVoid("tick.stdEntitlement", [&]{ PushStandardEditionEntitlement(); });

    // ★ 禁止在此 SafeProbe / 扫堆 / 遍历对象 / 逐对象写
}

void nhsy2_ForceClear() {
    g_inv.store(false);
    g_dmg.store(false);
    g_dmgMul.store(1);
    g_full.store(false);
    g_std.store(false);
    g_bomb.store(false);
    g_coin.store(false);
    g_crystal.store(false);
    g_key.store(false);
    g_ckey.store(false);
    g_spd.store(false);
    g_spdMul.store(1);
    g_jump.store(false);
    g_reenter.store(false);
}
