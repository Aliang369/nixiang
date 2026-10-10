/* ================================================================
 *  无悔华夏 3.8.280 — 最小 hook
 *  九游 com.yhhy.whrhx.aligames (STARRY_GAME_WHRHX)      v23 零身份钩子
 *  快爆 com.syyx.whrhx          (STARRY_GAME_WHRHXKB)    8/20 构建，捕获式身份
 *  装钩双路径（谁成功用谁，禁止二选一来回改）：
 *    1) RVA：maps RX + 序言校验（有表则校验；禁全 RX 误扫）
 *    2) Il2Cpp 按名：API ready 时 methodPointer（不锁死旧 RVA）
 *  闪退约束：启动零挂点；禁 CanAddArmy；开关 ON 才 Dobby
 *  STARRY_TRACE=1 → adb logcat -s StarryTrace StarryMagic:V
 * ================================================================ */

#include "WhrhxMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <ctime>

#include <android/log.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Unity/Il2CppApi.h"

#include <sys/syscall.h>
#include <sys/uio.h>
#include <unistd.h>

using Il2Cpp::GetApi;

// 装钩 RVA：官方运行时 dump（原包dump实锤）+ 乐变热更 190MB 对照（斌哥/lebian 落盘 so 序言吻合）
enum : uintptr_t {
    // 经济类官版 RVA 已删（2026-08-30）：官版会话身份链不可用，经济功能本就惰性，
    // 挂钩无意义；战斗/加速官版路径保留（官版首会话真生效，v9/v10 验证）。
    // ★ 2026-09-23 热更重校：以下"官版"值即官方 so（APK 内置 221MB）在新构建下的真身，
    //   乐变 OTA 那份 201MB 与本簇 RVA 同值（两者同构建）；旧值整体作废。
    RVA_BATTLE_SET_END    = 0x33951C8,  // BattleControlMgr.SetEnd(bool,bool,int,int,int)
    RVA_BATTLE_START      = 0x339A9E4,  // BattleControlMgr.StartBattle
    RVA_BATTLE_UPDATE_TIME = 0x3394D5C, // BattleControlMgr.UpdateBattleTime
    RVA_GET_DELTA_TIME    = 0x8BB9860,  // UnityEngine.Time.get_deltaTime()
    // 乐变热更后常见 190MB libil2cpp——官方 RVA 序言 miss，用下列对照
    // 九游 3.8.280 乐变 201MB（sha c04afff4…，2026-09-23 OTA 热更落地）
    RVA_COST_SINGLE_LEB   = 0x325E210,  // CountryData.CostSingleRes(CostResData,bool)  ins D10283FF
    RVA_COST_RES_LEB      = 0x326D35C,  // CountryData.CostRes(List<CostResData>)      ins D10143FF
    RVA_CHECK_RES_LEB     = 0x326D548,  // CountryData.CheckRes(List<CostResData>,int) ins D10183FF
    RVA_GET_DELTA_LEB     = 0x8BB9860,  // 与官版同址（本次热更后不再分叉）
    RVA_ON_BATTLE_END_LEB = 0x3BDDBBC,  // BattleUtil.OnBattleEnd(bool)  ins F81D0FF5（官方 SetEnd 页为 0）
    // 资源发放 + 玩家身份
    RVA_ADD_RES_LEB       = 0x32691C8,  // CountryData.AddRes(resType, cnt, isTrading)  ins A9BC5FF8
    RVA_ADD_RES_CAMPAIGN  = 0x3908B40,  // CampaignData.AddRes(resType, cnt)            ins F81D0FF5
    RVA_COUNTRY_ID_OFF    = 0xE0,       // CountryData 国家 id 字段偏移
    RVA_MGR_COUNTRY_ID_OFF = 0x30,      // CountryDataMgr.m_countryId（IsPlayerCountry 反汇编比对源）
    // 2026-08-30 v23 零身份钩子（乐变库带导出表，IsPlayerCountry 的 Dobby 钩 4/4 崩溃 → 永久禁钩）：
    // 读它的取值链（+0x8C adrp→ldr→ldr 得 MethodInfo；bl get_Instance），Tick 直呼 getter 读 mgr+0x30
    RVA_IS_PLAYER_LEB      = 0x67DF754,  // CountryData.IsPlayerCountry（兼作乐变构建标记，序言 A9BE4FF4）
                                         // 2026-10-11 校订：旧值 0x32584F4 落在 protobuf Field.Types.Cardinality
                                         // 区间（0x64F89D0→ILFixDynamicMethodWrapper.__Gen_Wrap_1150），
                                         // 两值在本机 79adda77 so 上序言均 miss → 身份链恒失败。
    RVA_GET_INSTANCE_LEB   = 0x64F89D0,  // QF.Singleton<CountryDataMgr>$$get_Instance 泛型共享实例 ins F81C0FF7
                                         // 2026-10-11 已失效：该 RVA 实为 ILFixDynamicMethodWrapper.__Gen_Wrap_1150
                                         // → 身份链改走 ResolveCountryMgrByName()（按名 + runtime_invoke）
    RVA_IS_PLAYER_DECODE_OFF = 0x8C,
    // 战斗三件（乐变 dump RVA；官方 SetEnd/Update 空页即拒）
    RVA_UPDATE_BATTLE_LEB  = 0x3394D5C,
    RVA_SET_END_LEB        = 0x33951C8,
    RVA_START_BATTLE_LEB   = 0x339A9E4,
    RVA_ARMY_CORRECT_LEB   = 0x3A0264C,  // CountryDataMgr.CorrectArmyData()：军队校正总口（周期重算，钩后同步回钉）
    RVA_GET_COUNTRY_MGR_LEB = 0x3A0379C, // CountryDataMgr.GetCountryDataById(int) → 捕获 this
    RVA_GET_CITY_MGR_LEB    = 0x38659F0, // CityDataMgr.GetCityData(int) → 捕获 this

    // ── 好游快爆 com.syyx.whrhx · 乐变 223.7MB（sha 1646c8bb…，8/20 构建）──
    // 2026-08-30 活内存 dump：metadata v24.5 变体(32段/method 64B token@+52/type 92B/image 40B)
    // + gap 区 CodeGenModule methodPointers（idx=rid-1，get_deltaTime 序言 f81e0ff3 与九游互证）。
    // 8/20 构建重构：CountryData.CostRes/CanAddArmy 删除；BattleControlMgr.StartBattle/UpdateBattleTime
    // 改名（候选 StartTimer/OnCountDown/UpdateBattleCostTime，Trace 分辨）；QF.Singleton getter 泛型化。
    RVA_KB_COST_SINGLE     = 0x31EE844,  // CountryData.CostSingleRes
    RVA_KB_CHECK_RES       = 0x31DBF28,  // CountryData.CheckRes
    RVA_KB_ADD_RES         = 0x31EBCB4,  // CountryData.AddRes
    RVA_KB_ADD_RES_CAMP    = 0x3E84B58,  // CampaignData.AddRes
    RVA_KB_ARMY_CORRECT    = 0x3A9A258,  // CountryDataMgr.CorrectArmyData
    RVA_KB_GET_DELTA       = 0x8BD2134,  // Time.get_deltaTime（九游 0x8BD2100 + 0x34）
    RVA_KB_SET_END         = 0x33091F4,  // BattleControlMgr.SetEnd（同名沿用）
    RVA_KB_ON_COUNTDOWN    = 0x32FF1D8,  // BattleControlMgr.OnCountDown（tick 候选1）
    RVA_KB_UPDATE_COST     = 0x3300AF0,  // BattleControlMgr.UpdateBattleCostTime（tick 候选2）
    RVA_KB_START_TIMER     = 0x32FEEE8,  // BattleControlMgr.StartTimer（开战候选）
    RVA_KB_ON_BATTLE_END   = 0x3629344,  // LockStepMgr.OnBattleEnd（首指令 B 跳板，resolve-only）
    RVA_KB_GET_COUNTRY_MGR = 0x3A9AF68,  // CountryDataMgr.GetCountryDataById → 捕获 this
    RVA_KB_GET_CITY_MGR    = 0x3728BBC,  // CityDataMgr.GetCityData → 捕获 this

    // ── 2026-09-23 乐变 OTA 热更后重校（新 dump.cs + libil2cpp 首指令逐点实锤）──
    // 旧值全部作废：OTA 换掉了运行时 libil2cpp.so，RVA 整体漂移。
    RVA_RESEARCH_TECH          = 0x44AF604,  // MengHuaRecordMgr.ResearchTech(int)
    RVA_TECH_POLICY_RESEARCH   = 0x3C09BF0,  // MengHuaTechPolicyData.Research()
    RVA_POLICY_GET_DATA        = 0x3C506AC,  // PolicyDataMgr.GetPolicyData(int)
    RVA_POLICY_UPDATE_TIME     = 0x3C51A6C,  // PolicyDataMgr.UpdatePolicyTime(float,bool,bool)  ← 2→3 参
    RVA_POLICY_FINISH_ANYWAY   = 0x3C538B8,  // PolicyDataMgr.FinishPolicyTimeAnyway(int)
    RVA_POLICY_EXECUTE         = 0x3C56968,  // PolicyDataMgr.ExecutePolicy(int,float,bool,bool) ← 3→4 参
    RVA_POLICY_UI_EXECUTE      = 0x3854030,  // GameUI.PolicyUI.ExecutePolicy(int)
    // 名臣科研入口（上一轮漏做）：与 PolicyUI 并列的另外两个政策面板
    RVA_MINISTER_UI_EXECUTE       = 0x3C6C2B8,  // GameUI.MinisterPolicyUI.ExecutePolicy(int)
    RVA_EXTRA_MINISTER_UI_EXECUTE = 0x422A7E8,  // GameUI.ExtraMinisterPolicyUI.ExecutePolicy(int)
    // 国家政策面板的「资源不足」资源门（在内联于 ActivePolicy，不走 CountryData.CheckRes）：
    // PolicyUI.ActivePolicy 内 → CanActive 早退 → GetResCnt(21/22) 对比 GetSpecialResCostList → ShowPopUI
    RVA_POLICY_UI_ACTIVE      = 0x3852D18,  // GameUI.PolicyUI.ActivePolicy()  ins D10303FF
    RVA_POLICY_CAN_ACTIVE     = 0x3C50760,  // PolicyDataMgr.CanActive(int,bool,bool,bool) ins A9BC5FF8
    RVA_COUNTRY_GET_RES_CNT   = 0x326A8D0,  // CountryData.GetResCnt(eSpecialResType) → int ins A9BE4FF4
    // 名臣两侧：TryActivePolicy → PolicyDataMgr.CheckPolicyDisable(学习条件) → ShowPopUI(条件文案)
    // 实测：名臣面板默认也提示「资源不足」，执行完一项资源够的政策后因条件重算才放开；科技面板则始终不放
    RVA_MINISTER_TRY_ACTIVE        = 0x3C6BA68,  // GameUI.MinisterPolicyUI.TryActivePolicy() ins D101C3FF
    RVA_EXTRA_MINISTER_TRY_ACTIVE  = 0x4229014,  // GameUI.ExtraMinisterPolicyUI.TryActivePolicy() ins D101C3FF
    RVA_POLICY_CHECK_DISABLE       = 0x3C5959C,  // PolicyDataMgr.CheckPolicyDisable(int) → int ins 0xA9BB67FA
    // 「能不能点」的真门（2026-09-23 续修）：条目显示/可点状态由它返回
    // 调用者 PolicyItem.CheckShowState @0x389AB0C（+0x124 处 bl 本函数，返回值存 PolicyItem+0x104）
    // ← 列表构建 PolicyItem.InitPolicyItem +0x120、点击 PolicyUI.OnClickPolicyItem +0xF4
    // 函数内 0x3C5C8E0 cmp/0x3C5C8E4 b.ge → 0x3C5C8E8 orr w20,wzr,#7(ResNotEnough)
    //                                      / 0x3C5C8F0 orr w20,wzr,#2(CanExecute)
    RVA_POLICY_ITEM_SHOW_STATE     = 0x3C5B5A4,  // PolicyDataMgr.GetPolicyItemShowState(int,float) ins D104C3FF

    // ── 2026-10-11 本机运行时 so 重校（sha256 79adda77…、201,368,952B；dump=06_dumped/无悔华夏/current_20261011）──
    // 说明：上面 0923 那批锚点基准是 c04afff4…，本机再热更后已整体漂移（旧值首指令不符 →
    //       ResolveRva 直接 miss → 自动回落按名）。本轮只登记四个新功能用到的锚点，
    //       首指令值均按当前 so 逐点读出（见 kExpect[]）。
    RVA_ARMY_GET_MAX_CAN_SEND  = 0x7016B98,  // CountryData.CountryArmyData.GetMaxCanSendNum(int,bool,int,bool,long,bool) → long
    RVA_ARMY_GET_MAX_SEND      = 0x7017094,  // CountryData.CountryArmyData.GetMaxSendNum(eBattleType,int) → long
    RVA_MINISTER_GET_FAVOR     = 0x7CD6F58,  // MinisterPlayMgr.GetMinisterFavorite(int) → int
    RVA_MINISTER_ADD_FAVOR     = 0x7633D40,  // MinisterPlayMgr.MinisterPlayData.AddFavor(int)
};

static constexpr size_t kOffCountryMoney  = 0x118;
static constexpr size_t kOffCountryFood   = 0x120;
static constexpr size_t kOffCountryPop    = 0x188;
static constexpr size_t kOffCountryArmy   = 0x198;
static constexpr size_t kOffResDataNumFx  = 0x10;
static constexpr size_t kOffArmyPercentFx = 0x18;
static constexpr size_t kOffPolicyRemainTime = 0x14;  // PolicyData.m_remainTime (float)

// 2026-10-11 新增四项所需字段（dump current_20261011 逐点实锤）
static constexpr size_t kOffPolicyRepeatCd     = 0x18;   // PolicyData.m_repeatRemainCd (float) ←「政策重复CD」
static constexpr size_t kOffCountryPolicyMgr   = 0x1C8;  // CountryData.m_policyMgr (PolicyDataMgr)
static constexpr size_t kOffCountryMinisterMgr = 0x2A8;  // CountryData.m_ministerData (MinisterPlayMgr)
static constexpr size_t kOffMgrPolicyDic       = 0x10;   // PolicyDataMgr.m_policyDic
static constexpr size_t kOffMinisterPlayDic    = 0x18;   // MinisterPlayMgr.m_playDataDic
static constexpr size_t kOffMinisterFavorite   = 0x18;   // MinisterPlayMgr.MinisterPlayData.Favorite (int)
static constexpr size_t kOffArmyCountryId      = 0x98;   // CountryData.CountryArmyData.m_countryId (int)
static constexpr int32_t kFavorFull = 100;               // 好感度拉满值（游戏 90 档阈值之上）

static constexpr int64_t kFakeRes = 99999999LL;
static constexpr int64_t kFakeNum = 999999LL;

enum eEffectType : int32_t {
    kEffMoney         = 101,
    kEffStandardMoney = 102,
    kEffFood          = 151,
    kEffStandardFood  = 153,
    kEffArmy          = 201,
};

enum eSpecialResType : int32_t {
    kResFood  = 6,
    kResMoney = 7,
};

static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_hooksEco{false};
static std::atomic<bool> g_hooksBattle{false};
static std::atomic<bool> g_hooksSpeed{false};
static std::atomic<bool> g_hooksResearch{false};
static std::atomic<bool> g_hooksMinister{false};   // feat11/12 名臣钩
static std::atomic<bool> g_hooksArmyScale{false};  // feat13 军队规模钩

static std::atomic<bool> g_gold{false};
static std::atomic<bool> g_food{false};
static std::atomic<bool> g_army{false};
static std::atomic<bool> g_pop{false};
static std::atomic<bool> g_win{false};
static std::atomic<bool> g_researchDone{false};
static std::atomic<int>  g_speedLvl{1};
static std::atomic<int>  g_lastSpeedLvl{0};

// 2026-10-11 新增四项的开关（feat10..#13）
static std::atomic<bool> g_policyNoCd{false};         // feat10 政策技能无CD
static std::atomic<bool> g_ministerFavor{false};      // feat11 名臣好感度
static std::atomic<bool> g_ministerSkillNoCd{false};  // feat12 名臣技能无CD
static std::atomic<int>  g_armyScalePct{0};           // feat13 0/1=关；2..100 = 规模加成 +X%
// 名臣面板入口作用域（只在 MinisterPolicyUI/ExtraMinisterPolicyUI 调用栈内 >0）
static thread_local int t_ministerSkillDepth = 0;

static std::atomic<int32_t> g_playerCountryId{-1};
static std::atomic<void*> g_playerCountry{nullptr};
static std::atomic<void*> g_playerMoneyFx{nullptr};
static std::atomic<void*> g_playerFoodFx{nullptr};
static std::atomic<void*> g_playerArmyFx{nullptr};
// v23 身份链：getter 直呼（零钩子）+ 最近 Cost thiz（钩子线程只存裸指针）
static std::atomic<void*> g_countryMgr{nullptr};
static std::atomic<void*> g_lastCostCountry{nullptr};
static std::atomic<void*> g_policyMgr{nullptr};  // PolicyDataMgr 实例（Hooked_PolicyExecute 的 thiz）
static std::atomic<size_t> g_offCountryId{0xE0};
static std::atomic<size_t> g_offMgrCountryId{0x30};
static std::atomic<bool> g_identityApiResolved{false};
static void* g_countryIdMi{nullptr};       // CountryDataMgr.get_m_countryId MethodInfo*
static void* g_countryIdFn{nullptr};       // 同上 methodPointer（不挂 Dobby，仅只读调用/兜底）
// 钉值计数（功能逻辑使用，非仅 trace）
static std::atomic<uint32_t> g_pins{0};
static std::atomic<uint32_t> g_pinSkip{0};
static void* g_mi{nullptr};          // Singleton<CountryDataMgr>.get_Instance 的 MethodInfo*
static void* g_getInstFn{nullptr};   // 0x6558B64
static std::atomic<int64_t> g_lastMgrMs{0};
static std::atomic<void*> g_cityMgr{nullptr};  // CityDataMgr 实例（GetCityData 捕获）
// v24 城市驻军源头钉值：CityDataMgr 单例（mi 经类名校验）+ 字典遍历
static void* g_cityMi{nullptr};
static constexpr uintptr_t kCityMgrSlotCandidates[] = {0xb651da8, 0xb5f5e70, 0xb534d68};

// EffectData / 数据类字段偏移（dump.cs 1113413 起实锤）
static constexpr size_t kOffFxBaseValue = 0x14;   // EffectData.m_baseValue (float)
static constexpr size_t kOffTotalNum    = 0x10;   // CountryArmyData/CountryPopulationData.m_totalNum (long)

static std::atomic<bool> g_battleActive{false};
static std::atomic<bool> g_battleEnded{false};
static std::atomic<int32_t> g_battleFrames{0};

#if defined(STARRY_TRACE)
static std::atomic<uint32_t> g_hitsCostSingle{0};
static std::atomic<uint32_t> g_hitsCostRes{0};
static std::atomic<uint32_t> g_hitsCheckRes{0};
static std::atomic<uint32_t> g_hitsStartBattle{0};
static std::atomic<uint32_t> g_hitsUpdateBattle{0};
static std::atomic<uint32_t> g_hitsSetEnd{0};
static std::atomic<uint32_t> g_hitsAddRes{0};
static std::atomic<uint32_t> g_hitsAddResCamp{0};
static std::atomic<uint32_t> g_hitsArmyCorrect{0};
static std::atomic<uint32_t> g_traceTick{0};
#ifdef STARRY_GAME_WHRHXKB
static std::atomic<uint32_t> g_hitsCountDown{0};
static std::atomic<uint32_t> g_hitsUpdateCost{0};
#endif
#define WHRHX_TRACE(...) \
    __android_log_print(ANDROID_LOG_INFO, "StarryTrace", "[Whrhx] " __VA_ARGS__)
#else
#define WHRHX_TRACE(...) ((void)0)
#endif

typedef void (*CostSingleRes_t)(void*, void*, bool, void*);
typedef void (*CostRes_t)(void*, void*, void*);
typedef bool (*CheckRes_t)(void*, void*, int32_t, void*);
typedef void (*BattleSetEnd_t)(void*, bool, bool, int32_t, int32_t, int32_t, void*);
typedef void (*StartBattle_t)(void*, void*);
typedef void (*UpdateBattleTime_t)(void*, void*);
typedef float (*GetDelta_t)(void*);
// BattleUtil.OnBattleEnd — 乐变上 SetEnd 空页时的结算兜底（参数以 Trace 常见形态为准）
typedef void (*OnBattleEnd_t)(void*, bool, void*);
// 2026-08-29 Trace：资源发放（只观测，恒调 orig）
typedef void (*AddResTrade_t)(void*, int32_t, int64_t, bool, void*);
typedef void (*AddResCamp_t)(void*, int32_t, int64_t, void*);
// v23 身份：单例 getter（静态，直呼）
typedef void* (*GetMgrInst_t)(void*);
typedef void* (*GetCountryById_t)(void*, int32_t, void*);
typedef void* (*GetCityById_t)(void*, int32_t, void*);
typedef int32_t (*GetCountryId_t)(void*, void*);
static GetCountryById_t g_origGetCountryById = nullptr;
static GetCityById_t g_origGetCityById = nullptr;
// 军队重算校正（mgr 级 CorrectArmyData；orig 后同步回钉，消除互跳）
typedef void (*ArmyCorrect_t)(void*, void*);
static ArmyCorrect_t g_origArmyCorrect = nullptr;

static CostSingleRes_t g_origCostSingle = nullptr;
static CostRes_t g_origCostRes = nullptr;
static CheckRes_t g_origCheckRes = nullptr;
static BattleSetEnd_t g_origBattleSetEnd = nullptr;
static OnBattleEnd_t g_origOnBattleEnd = nullptr;
static StartBattle_t g_origStartBattle = nullptr;
static UpdateBattleTime_t g_origUpdateBattleTime = nullptr;
static GetDelta_t g_origGetDelta = nullptr;
static std::atomic<void*> g_battleMgr{nullptr};
static AddResTrade_t g_origAddResTrade = nullptr;
static AddResCamp_t g_origAddResCamp = nullptr;
#ifdef STARRY_GAME_WHRHXKB
// kb 捕获钩（身份链替代：getter 泛型化拿不到直呼地址）
static UpdateBattleTime_t g_origOnCountDown = nullptr;
static UpdateBattleTime_t g_origUpdateCost = nullptr;
#endif

// 2026-09-12 科研立即完成：只消费科技研发入口触发的 ExecutePolicy，避免全局放大政策时间。
// 2026-09-23 OTA 后签名重校：ExecutePolicy +1 参（isAutoSave），UpdatePolicyTime +1 参（isAutoSave）。
typedef void (*ResearchTech_t)(void*, int32_t, void*);
typedef void (*TechPolicyResearch_t)(void*, void*);
typedef void* (*PolicyGetData_t)(void*, int32_t, void*);
typedef void (*PolicyExecute_t)(void*, int32_t, float, bool, bool, void*);
typedef void (*PolicyFinishAnyway_t)(void*, int32_t, void*);
typedef void (*PolicyUpdateTime_t)(void*, float, bool, bool, void*);
typedef void (*PolicyUiExecute_t)(void*, int32_t, void*);
// 面板资源门（2026-09-23）：科技面板内联检查 + 名臣面板学习条件检查
typedef void (*PanelActive_t)(void*, void*);                                  // PolicyUI.ActivePolicy() 等 0 参实例方法
typedef int32_t (*CountryGetResCnt_t)(void*, int32_t, void*);                 // CountryData.GetResCnt(eSpecialResType)
typedef int32_t (*PolicyCheckDisable_t)(void*, int32_t, void*);               // PolicyDataMgr.CheckPolicyDisable(int)
typedef bool (*PolicyCanActive_t)(void*, int32_t, bool, bool, bool, void*);   // PolicyDataMgr.CanActive(int,bool,bool,bool)
typedef int32_t (*PolicyItemShowState_t)(void*, int32_t, float, void*);       // PolicyDataMgr.GetPolicyItemShowState(int,float) → ePolicyItemShowState

static ResearchTech_t g_origResearchTech = nullptr;
static TechPolicyResearch_t g_origTechPolicyResearch = nullptr;
static PolicyGetData_t g_policyGetData = nullptr;
static PolicyExecute_t g_origPolicyExecute = nullptr;
static PolicyFinishAnyway_t g_policyFinishAnyway = nullptr;
static PolicyUpdateTime_t g_policyUpdateTime = nullptr;
static PolicyUiExecute_t g_origPolicyUiExecute = nullptr;
// 名臣科研入口（MinisterPolicyUI / ExtraMinisterPolicyUI），签名与 PolicyUI.ExecutePolicy 同构
static PolicyUiExecute_t g_origMinisterUiExecute = nullptr;
static PolicyUiExecute_t g_origExtraMinisterUiExecute = nullptr;
static PanelActive_t g_origPolicyUiActive = nullptr;          // GameUI.PolicyUI.ActivePolicy()
static PanelActive_t g_origMinisterTryActive = nullptr;       // GameUI.MinisterPolicyUI.TryActivePolicy()
static PanelActive_t g_origExtraMinisterTryActive = nullptr;  // GameUI.ExtraMinisterPolicyUI.TryActivePolicy()
static CountryGetResCnt_t g_countryGetResCnt = nullptr;       // 解析后直呼（不需 hook 时）
static PolicyCheckDisable_t g_policyCheckDisable = nullptr;
static PolicyCanActive_t g_policyCanActive = nullptr;
static PolicyItemShowState_t g_policyItemShowState = nullptr;  // 条目状态门（决定可点/显示文案）
static thread_local int t_researchDepth = 0;
static thread_local bool t_researchHandled = false;
// 只在这三个面板的 ActivePolicy/TryActivePolicy 调用栈内放行资源门，避免全局污染 UI 数值
static thread_local int t_policyPanelDepth = 0;
static std::atomic<bool> g_hooksPanelGate{false};
static constexpr int32_t kResCntBypass = 999999999;

// 2026-10-11 新增四项的原生函数指针
// ④ 军队规模：CountryArmyData 实例方法（this=armyData）
typedef int64_t (*ArmyMaxCanSend_t)(void*, int32_t, bool, int32_t, bool, int64_t, bool, void*);
typedef int64_t (*ArmyMaxSend_t)(void*, int32_t, int32_t, void*);
// ② 名臣好感度：MinisterPlayMgr 实例 + MinisterPlayData 实例
typedef int32_t (*MinisterGetFavor_t)(void*, int32_t, void*);
typedef void    (*MinisterAddFavor_t)(void*, int32_t, void*);
static ArmyMaxCanSend_t g_origArmyMaxCanSend = nullptr;
static ArmyMaxSend_t g_origArmyMaxSend = nullptr;
static MinisterGetFavor_t g_origMinisterGetFavor = nullptr;
static MinisterAddFavor_t g_origMinisterAddFavor = nullptr;


// --- 双路径：RVA ↔ Il2Cpp 按名；禁止 base+RVA；禁止全 RX 序言误扫 ---

static bool IsIl2CppMapLine(const char* line, const char* lib) {
    return line && lib && strstr(line, lib) && strstr(line, ".so");
}

static void* VaInSegment(unsigned long start, unsigned long end, unsigned long mapOff,
                         uintptr_t fileOff) {
    if (fileOff < mapOff) return nullptr;
    const unsigned long span = end - start;
    if (fileOff - mapOff >= span) return nullptr;
    return (void*)(start + (fileOff - mapOff));
}

static void* FileOffToVaBest(const char* lib, uintptr_t fileOff) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return nullptr;
    char line[768];
    void* found = nullptr;
    size_t bestSpan = 0;
    int bestPrio = -1;  // 多份同名库同时映射时的优先级（乐变热更副本 = RVA 基准）
    while (fgets(line, sizeof(line), fp)) {
        if (!IsIl2CppMapLine(line, lib)) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x')) continue;
        if (end <= start) continue;
        void* va = VaInSegment(start, end, off, fileOff);
        if (!va) continue;
        const size_t span = static_cast<size_t>(end - start);
        // 2026-10-11：APK 内置 libil2cpp.so（221MB）与乐变热更副本（201MB）可能同时映射，
        // 仅按 span 取大会选错库（RVA 基准是乐变那份）→ 先比路径优先级，同优先级再比 span。
        const int prio = strstr(line, "/lebian/") ? 2
                       : (strstr(line, "/data/app/") ? 1 : 0);
        if (prio > bestPrio || (prio == bestPrio && span > bestSpan)) {
            bestPrio = prio;
            bestSpan = span;
            found = va;
        }
    }
    fclose(fp);
    return found;
}

// 数据段版：静态槽位（.bss/.data）RVA→VA。必须优先可写段——数据 RVA 常小于
// 巨大代码段的偏移范围，先匹配会错误命中 r-xp（0xB651DA8<0x31DF000 实锤踩过）。
static void* FileOffToVaAny(const char* lib, uintptr_t fileOff) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return nullptr;
    char line[768];
    void* writable = nullptr;
    void* readable = nullptr;
    while (fgets(line, sizeof(line), fp)) {
        if (!IsIl2CppMapLine(line, lib)) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'r')) continue;
        void* va = VaInSegment(start, end, off, fileOff);
        if (!va) continue;
        if (strchr(perm, 'w')) { writable = va; break; }
        if (!readable) readable = va;
    }
    fclose(fp);
    return writable ? writable : readable;
}

static bool ReadInsn(void* addr, uint32_t* out) {
    if (!addr || !out) return false;
    uint32_t v = 0;
    memcpy(&v, addr, sizeof(v));
    *out = v;
    return true;
}

struct PrologueExpect {
    uintptr_t rva;
    uint32_t firstInsn;
    const char* tag;
};

static const PrologueExpect kExpect[] = {
    {RVA_COST_SINGLE_LEB, 0xD10283FF, "CostSingleRes"},
    {RVA_COST_RES_LEB, 0xD10143FF, "CostRes"},
    {RVA_CHECK_RES_LEB, 0xD10183FF, "CheckRes"},
    {RVA_IS_PLAYER_LEB, 0xA9BE4FF4, "IsPlayerCountry"},
    {RVA_GET_DELTA_LEB, 0xF81E0FF3, "get_deltaTime"},
    {RVA_ON_BATTLE_END_LEB, 0xF81D0FF5, "OnBattleEnd"},
    {RVA_ADD_RES_LEB, 0xA9BC5FF8, "AddRes"},
    {RVA_ADD_RES_CAMPAIGN, 0xF81D0FF5, "AddResCampaign"},
    // 2026-08-29 官方库活体序言（乐变内存补丁会让官方库变化，pristine 态为准）
    {RVA_BATTLE_START, 0xD10143FF, "StartBattle"},
    {RVA_BATTLE_UPDATE_TIME, 0xD10143FF, "UpdateBattleTime"},
    {RVA_BATTLE_SET_END, 0xD10243FF, "BattleSetEnd"},
    {RVA_GET_DELTA_TIME, 0xF81E0FF3, "get_deltaTime"},
    // 战斗 LEB（dump 序言）
    {RVA_UPDATE_BATTLE_LEB, 0xD10143FF, "UpdateBattleTime"},
    {RVA_SET_END_LEB, 0xD10243FF, "BattleSetEnd"},
    {RVA_START_BATTLE_LEB, 0xD10143FF, "StartBattle"},
    {RVA_ARMY_CORRECT_LEB, 0xD10303FF, "ArmyCorrectMgr"},
    {RVA_GET_COUNTRY_MGR_LEB, 0xF81D0FF5, "GetCountryDataById"},
    {RVA_GET_CITY_MGR_LEB, 0xF81D0FF5, "GetCityData"},
    // 好游快爆（8/20 构建活体内存序言）
    {RVA_KB_COST_SINGLE, 0xA9BD57F6, "KB.CostSingleRes"},
    {RVA_KB_CHECK_RES, 0xD101C3FF, "KB.CheckRes"},
    {RVA_KB_ADD_RES, 0xA9BE4FF4, "KB.AddRes"},
    {RVA_KB_ADD_RES_CAMP, 0xD10143FF, "KB.AddResCampaign"},
    {RVA_KB_ARMY_CORRECT, 0xF81D0FF5, "KB.ArmyCorrectMgr"},
    {RVA_KB_GET_DELTA, 0xF81E0FF3, "KB.get_deltaTime"},
    {RVA_KB_SET_END, 0xF81E0FF3, "KB.BattleSetEnd"},
    {RVA_KB_ON_COUNTDOWN, 0xD10143FF, "KB.OnCountDown"},
    {RVA_KB_UPDATE_COST, 0xF81E0FF3, "KB.UpdateBattleCostTime"},
    {RVA_KB_START_TIMER, 0xA9BE4FF4, "KB.StartTimer"},
    {RVA_KB_GET_COUNTRY_MGR, 0xD101C3FF, "KB.GetCountryDataById"},
    {RVA_KB_GET_CITY_MGR, 0xA9BD57F6, "KB.GetCityData"},
    // 2026-09-23 乐变 OTA 热更后九游科研链重校（首指令逐点实锤）
    {RVA_RESEARCH_TECH, 0xF81D0FF5, "MengHuaRecordMgr.ResearchTech"},
    {RVA_TECH_POLICY_RESEARCH, 0xA9BE4FF4, "MengHuaTechPolicyData.Research"},
    {RVA_POLICY_GET_DATA, 0xF81D0FF5, "PolicyDataMgr.GetPolicyData"},
    {RVA_POLICY_UPDATE_TIME, 0xD10383FF, "PolicyDataMgr.UpdatePolicyTime"},
    {RVA_POLICY_FINISH_ANYWAY, 0xD10103FF, "PolicyDataMgr.FinishPolicyTimeAnyway"},
    {RVA_POLICY_EXECUTE, 0xFC1C0FE8, "PolicyDataMgr.ExecutePolicy"},
    {RVA_POLICY_UI_EXECUTE, 0xF81D0FF5, "PolicyUI.ExecutePolicy"},
    {RVA_MINISTER_UI_EXECUTE, 0xFC1D0FE8, "MinisterPolicyUI.ExecutePolicy"},
    {RVA_EXTRA_MINISTER_UI_EXECUTE, 0xFC1D0FE8, "ExtraMinisterPolicyUI.ExecutePolicy"},
    // 国家政策面板资源门三件（2026-09-23 重校，首指令逐点实锤）
    {RVA_POLICY_UI_ACTIVE, 0xD10303FF, "PolicyUI.ActivePolicy"},
    {RVA_POLICY_CAN_ACTIVE, 0xA9BC5FF8, "PolicyDataMgr.CanActive"},
    {RVA_COUNTRY_GET_RES_CNT, 0xA9BE4FF4, "CountryData.GetResCnt"},
    // 名臣两侧学习条件门（首指令逐点实锤）
    {RVA_MINISTER_TRY_ACTIVE, 0xD101C3FF, "MinisterPolicyUI.TryActivePolicy"},
    {RVA_EXTRA_MINISTER_TRY_ACTIVE, 0xD101C3FF, "ExtraMinisterPolicyUI.TryActivePolicy"},
    {RVA_POLICY_CHECK_DISABLE, 0xA9BB67FA, "PolicyDataMgr.CheckPolicyDisable"},
    // 条目状态门（2026-09-23 续修）：资源/科技/前置等拦截态 → CanExecute
    {RVA_POLICY_ITEM_SHOW_STATE, 0xD104C3FF, "PolicyDataMgr.GetPolicyItemShowState"},
    // 2026-10-11 本机运行时 so（sha 79adda77…）四锚点首指令逐点实锤
    {RVA_ARMY_GET_MAX_CAN_SEND, 0xD101C3FF, "CountryArmyData.GetMaxCanSendNum"},
    {RVA_ARMY_GET_MAX_SEND, 0xFC1C0FE8, "CountryArmyData.GetMaxSendNum"},
    {RVA_MINISTER_GET_FAVOR, 0xF81D0FF5, "MinisterPlayMgr.GetMinisterFavorite"},
    {RVA_MINISTER_ADD_FAVOR, 0xA9BE4FF4, "MinisterPlayData.AddFavor"},
};

static const PrologueExpect* ExpectForRva(uintptr_t rva) {
    for (const auto& e : kExpect) {
        if (e.rva == rva) return &e;
    }
    return nullptr;
}

static void* ResolveRva(uintptr_t rva) {
    void* best = FileOffToVaBest(GameUnity::TARGET_LIB, rva);
    if (!best) return nullptr;
    uint32_t got = 0;
    if (!ReadInsn(best, &got) || got == 0 || got == 0xFFFFFFFFu) {
        LOGW("[Whrhx] rva miss off=0x%lX zero/invalid insn — try alt/name", (unsigned long)rva);
        return nullptr;
    }
    const PrologueExpect* exp = ExpectForRva(rva);
    if (exp) {
        if (got != exp->firstInsn) {
            LOGW("[Whrhx] rva miss %s got=0x%08X want=0x%08X — try il2cpp-name", exp->tag, got,
                 exp->firstInsn);
            return nullptr;
        }
    }
    // 乐变上官方 get_deltaTime 常是无条件 B stub；拒掉以免加速挂空
    if (rva == RVA_GET_DELTA_TIME && (got & 0xFC000000u) == 0x14000000u) {
        LOGW("[Whrhx] rva miss get_deltaTime stub B=0x%08X — try LEB/name", got);
        return nullptr;
    }
    return best;
}

static bool FnInLibExec(void* fn, const char* lib, uintptr_t* outOff) {
    if (!fn || !lib) return false;
    const uintptr_t addr = reinterpret_cast<uintptr_t>(fn);
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return false;
    char line[768];
    bool found = false;
    while (fgets(line, sizeof(line), fp)) {
        if (!IsIl2CppMapLine(line, lib)) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x')) continue;
        if (addr < start || addr >= end) continue;
        if (outOff) *outOff = off + (addr - start);
        found = true;
        break;
    }
    fclose(fp);
    return found;
}

static bool EnsureIl2CppApi() {
    if (GetApi().ready) return true;
    return GetApi().Load(GameUnity::TARGET_LIB);
}

static void* MethodPointer(void* mi) {
    return mi ? *reinterpret_cast<void**>(mi) : nullptr;
}

static void* FindClassNs(const char* ns, const char* name) {
    auto& api = GetApi();
    size_t n = 0;
    void** asms = api.Assemblies(&n);
    if (!asms || n == 0) return nullptr;
    for (size_t i = 0; i < n; i++) {
        void* img = api.ImageOf(asms[i]);
        if (!img) continue;
        void* k = api.ClassFromName(img, ns, name);
        if (k) return k;
    }
    return nullptr;
}

static void* ResolveByName(const char* ns, const char* cls, const char* method, const int* argcs,
                           size_t nArgcs) {
    if (!EnsureIl2CppApi() || !method) return nullptr;
    void* k = FindClassNs(ns, cls);
    if (!k) k = GetApi().FindClassByShortName(cls);
    if (!k) return nullptr;
    auto& api = GetApi();
    for (size_t i = 0; i < nArgcs; i++) {
        void* mi = api.GetMethod(k, method, argcs[i]);
        if (!mi) continue;
        if (api.method_get_name) {
            const char* nm = api.method_get_name(mi);
            if (!nm || strcmp(nm, method) != 0) continue;
        }
        void* fn = MethodPointer(mi);
        if (!fn || !Il2Cpp::IsValidHeapPtr(fn)) continue;
        uintptr_t off = 0;
        if (!FnInLibExec(fn, GameUnity::TARGET_LIB, &off)) continue;
        uint32_t insn = 0;
        if (!ReadInsn(fn, &insn) || insn == 0 || insn == 0xFFFFFFFFu) continue;
        return fn;
    }
    return nullptr;
}

static bool HookAt(const char* tag, void* addr, void* hook, void** orig, const char* via) {
    if (*orig) return true;
    if (!addr) return false;
    uintptr_t off = 0;
    if (!FnInLibExec(addr, GameUnity::TARGET_LIB, &off)) {
        LOGW("[Whrhx] %s %p not in RX", tag, addr);
        return false;
    }
    if (DobbyHook(addr, hook, orig) != 0) {
        LOGE("[Whrhx] DobbyHook %s failed @ %p", tag, addr);
        return false;
    }
    LOGI("[Whrhx] hooked %s @ %p off=0x%lX (%s)", tag, addr, (unsigned long)off, via);
    return true;
}

static bool HookAtRva(const char* tag, uintptr_t rva, void* hook, void** orig) {
    if (*orig) return true;
    void* addr = ResolveRva(rva);
    if (!addr) return false;
    uintptr_t off = 0;
    if (!FnInLibExec(addr, GameUnity::TARGET_LIB, &off) || off != rva) return false;
    return HookAt(tag, addr, hook, orig, "rva");
}

static bool HookByName(const char* tag, const char* ns, const char* cls, const char* method,
                       const int* argcs, size_t nArgcs, void* hook, void** orig) {
    if (*orig) return true;
    void* fn = ResolveByName(ns, cls, method, argcs, nArgcs);
    if (!fn) return false;
    return HookAt(tag, fn, hook, orig, "il2cpp-name");
}

// 三路径：主 RVA → 乐变对照 RVA → Il2Cpp 按名；任一成功即停
static bool HookRvaOrIl2Cpp(const char* tag, const char* ns, const char* cls, const char* method,
                            const int* argcs, size_t nArgcs, uintptr_t rva, void* hook, void** orig,
                            bool /*requirePrologue*/, uintptr_t altRva = 0) {
    if (*orig) return true;
    if (rva && HookAtRva(tag, rva, hook, orig)) return true;
    if (altRva && altRva != rva && HookAtRva(tag, altRva, hook, orig)) return true;
    if (method && nArgcs > 0 && HookByName(tag, ns, cls, method, argcs, nArgcs, hook, orig))
        return true;
    LOGW("[Whrhx] %s paths failed (rva=0x%lX alt=0x%lX name=%s)", tag, (unsigned long)rva,
         (unsigned long)altRva, method ? method : "?");
    return false;
}

static bool ResolveOrigAny(const char* tag, const char* ns, const char* cls, const char* method,
                           const int* argcs, size_t nArgcs, uintptr_t expectRva, uintptr_t altRva,
                           void** fn) {
    if (*fn) return true;
    const uintptr_t tryRvas[2] = {expectRva, altRva};
    for (uintptr_t rva : tryRvas) {
        if (!rva) continue;
        void* addr = ResolveRva(rva);
        uintptr_t off = 0;
        if (addr && FnInLibExec(addr, GameUnity::TARGET_LIB, &off) && off == rva) {
            *fn = addr;
            LOGI("[Whrhx] resolved %s @ %p off=0x%lX (rva)", tag, addr, (unsigned long)off);
            return true;
        }
    }
    void* p = ResolveByName(ns, cls, method, argcs, nArgcs);
    if (!p) {
        LOGE("[Whrhx] %s both paths failed", tag);
        return false;
    }
    *fn = p;
    uintptr_t off = 0;
    FnInLibExec(p, GameUnity::TARGET_LIB, &off);
    LOGI("[Whrhx] resolved %s @ %p off=0x%lX (il2cpp-name)", tag, p, (unsigned long)off);
    return true;
}

// --- 玩家缓存 / 敌我 ---
// v22 身份链：CountryDataMgr 钩（只存 this）→ Tick KRead mgr+0x30 得权威 pid →
// Cost 钩存的 thiz（KRead +0xE0）与 pid 匹配 → g_playerCountry。全程不碰 IsPlayerCountry。

static void* EffectFromResData(void* resData) {
    if (!resData) return nullptr;
    return *reinterpret_cast<void**>(static_cast<char*>(resData) + kOffResDataNumFx);
}

static bool IsPlayerCountryObj(void* countryData) {
    if (!countryData) return false;
    void* cached = g_playerCountry.load();
    return cached && countryData == cached;
}



static int32_t CostType(void* cost) {
    if (!cost) return -1;
    int32_t t = *reinterpret_cast<int32_t*>(cost);
    if (t == kResMoney || t == kResFood) return t;
    t = *reinterpret_cast<int32_t*>(static_cast<char*>(cost) + 0x10);
    if (t == kResMoney || t == kResFood) return t;
    return *reinterpret_cast<int32_t*>(cost);
}

static bool ShouldBypassType(int32_t type) {
    if (type == kResMoney && g_gold.load()) return true;
    if (type == kResFood && g_food.load()) return true;
    return false;
}

static bool ListAllCostsBypass(void* list) {
    if (!list) return false;
    void* arr = *reinterpret_cast<void**>(static_cast<char*>(list) + 0x10);
    int32_t size = *reinterpret_cast<int32_t*>(static_cast<char*>(list) + 0x18);
    if (!arr || size <= 0 || size > 64) return false;
    char* base = static_cast<char*>(arr) + 0x20;
    for (int32_t i = 0; i < size; i++) {
        if (!ShouldBypassType(*reinterpret_cast<int32_t*>(base + static_cast<size_t>(i) * 0x18)))
            return false;
    }
    return true;
}

static float SpeedMul() {
    const int lvl = g_speedLvl.load();
    return (lvl <= 1) ? 1.f : static_cast<float>(lvl <= 5 ? lvl : 5);
}

#if defined(STARRY_TRACE)
static void TraceHit(const char* tag, uint32_t n) {
    if (n <= 20 || (n % 10) == 0) WHRHX_TRACE("%s hits=%u", tag, n);
}

static void LogTraceSummary(const char* tag) {
    WHRHX_TRACE(
        "summary(%s) eco=%d army=%d pop=%d win=%d spd=%d CostSingle=%u CostRes=%u CheckRes=%u "
        "Start=%u Update=%u SetEnd=%u AddRes=%u AddResCamp=%u "
        "Pins=%u Skip=%u pid=%d",
        tag, g_gold.load() ? 1 : 0, g_army.load() ? 1 : 0, g_pop.load() ? 1 : 0,
        g_win.load() ? 1 : 0, g_speedLvl.load(), g_hitsCostSingle.load(), g_hitsCostRes.load(),
        g_hitsCheckRes.load(), g_hitsStartBattle.load(),
        g_hitsUpdateBattle.load(), g_hitsSetEnd.load(), g_hitsAddRes.load(),
        g_hitsAddResCamp.load(), g_pins.load(), g_pinSkip.load(),
        g_playerCountryId.load());
}
#endif

// --- Hook 替换（仅 Trace 定稿路径）---
// 禁止 Dobby 钩 IsPlayerCountry：4/4 崩溃相关（00:30 现场实锤，adrp 静态槽重定位破坏）→ 永久禁钩禁调

static bool KRead(const void* addr, void* out, size_t len);
static bool KWrite(void* addr, const void* src, size_t len);
static void CaptureCountryMgr(void* mgr);
static void PinPlayerValues();
static void PinCityArmies();

static void Hooked_CostSingleRes(void* thiz, void* cost, bool isTrading, void* method) {
#if defined(STARRY_TRACE)
    TraceHit("CostSingleRes", g_hitsCostSingle.fetch_add(1) + 1);
#endif
    g_lastCostCountry.store(thiz);  // 只存指针，Tick 线程 KRead 验 cid
    if (IsPlayerCountryObj(thiz) && ShouldBypassType(CostType(cost))) {
        static std::atomic<int> s_bp{0};
        const int n = s_bp.fetch_add(1);
        if (n < 8) LOGI("[Whrhx] bypass CostSingle type=%d n=%d", CostType(cost), n + 1);
        return;
    }
    if (g_origCostSingle) g_origCostSingle(thiz, cost, isTrading, method);
}

static void Hooked_CostRes(void* thiz, void* costs, void* method) {
#if defined(STARRY_TRACE)
    TraceHit("CostRes", g_hitsCostRes.fetch_add(1) + 1);
#endif
    g_lastCostCountry.store(thiz);
    if (IsPlayerCountryObj(thiz) && ListAllCostsBypass(costs)) {
        static std::atomic<int> s_bp{0};
        const int n = s_bp.fetch_add(1);
        if (n < 8) LOGI("[Whrhx] bypass CostRes n=%d", n + 1);
        return;
    }
    if (g_origCostRes) g_origCostRes(thiz, costs, method);
}

static bool Hooked_CheckRes(void* thiz, void* costs, int32_t from, void* method) {
#if defined(STARRY_TRACE)
    TraceHit("CheckRes", g_hitsCheckRes.fetch_add(1) + 1);
#endif
    g_lastCostCountry.store(thiz);
    // 科研「无条件直接执行」：只要处在政策面板（国家/名臣/额外名臣）ExecutePolicy 的作用域内，
    // 且 #9 科研开关为开，则跳过资源校验 → TryExecutePolicy 不再走「资源不足,无法执行」分支，
    // 直接进入 CostRes/执行流程，再由科研链把它立刻完成。
    if (g_researchDone.load() && t_researchDepth > 0) return true;
    if (IsPlayerCountryObj(thiz) && ListAllCostsBypass(costs)) return true;
    if (g_origCheckRes) return g_origCheckRes(thiz, costs, from, method);
    return true;
}

// 军队校正总口（CountryDataMgr，周期调用）：orig 校正完 → 同一时机补回城驻军。
// 国家级 totalNum 不再钉（与城驻军是两套数据，钉它只会制造显示闪跳）。
static void Hooked_ArmyCorrect(void* thiz, void* method) {
#if defined(STARRY_TRACE)
    TraceHit("ArmyCorrectMgr", g_hitsArmyCorrect.fetch_add(1) + 1);
#endif
    CaptureCountryMgr(thiz);
    if (g_origArmyCorrect) g_origArmyCorrect(thiz, method);
    if (g_army.load()) PinCityArmies();  // 校正周期内同步补回，用时减少瞬时可见后即恢复
}

static void TryForceWin(void* mgr) {
    if (!g_win.load() || g_battleEnded.load()) return;
    if (g_origBattleSetEnd && mgr) {
        g_origBattleSetEnd(mgr, true, false, -1, -1, -1, nullptr);
        g_battleEnded.store(true);
        g_battleActive.store(false);
        LOGI("[Whrhx] ForceWin via SetEnd mgr=%p", mgr);
        return;
    }
    if (g_origOnBattleEnd) {
        g_origOnBattleEnd(mgr ? mgr : g_battleMgr.load(), true, nullptr);
        g_battleEnded.store(true);
        g_battleActive.store(false);
        LOGI("[Whrhx] ForceWin via OnBattleEnd mgr=%p", mgr);
    }
}

static void Hooked_StartBattle(void* thiz, void* method) {
#if defined(STARRY_TRACE)
    TraceHit("StartBattle", g_hitsStartBattle.fetch_add(1) + 1);
#endif
    g_battleMgr.store(thiz);
    g_battleActive.store(true);
    g_battleEnded.store(false);
    g_battleFrames.store(0);
    if (g_origStartBattle) g_origStartBattle(thiz, method);
}

// 共用 tick：战斗中计数 → tick8 主动强胜。无开战钩时自臂（连续 2 tick 视为战斗中）。
static void BattleTick(void* thiz) {
    const uint32_t n = g_battleFrames.fetch_add(1) + 1;
    if (!g_battleActive.load()) {
        if (n >= 2) {
            g_battleActive.store(true);
            g_battleFrames.store(0);
        }
        return;
    }
    if (g_win.load() && !g_battleEnded.load() && n == 8) TryForceWin(thiz);
}

static void Hooked_UpdateBattleTime(void* thiz, void* method) {
    if (g_origUpdateBattleTime) g_origUpdateBattleTime(thiz, method);
#if defined(STARRY_TRACE)
    TraceHit("UpdateBattleTime", g_hitsUpdateBattle.fetch_add(1) + 1);
#endif
    BattleTick(thiz);
}

#ifdef STARRY_GAME_WHRHXKB
// tick 双候选（8/20 构建改名，Trace 分辨后定稿删一）
static void Hooked_OnCountDown(void* thiz, void* method) {
#if defined(STARRY_TRACE)
    TraceHit("OnCountDown", g_hitsCountDown.fetch_add(1) + 1);
#endif
    if (g_origOnCountDown) g_origOnCountDown(thiz, method);
    BattleTick(thiz);
}
static void Hooked_UpdateCostTime(void* thiz, void* method) {
#if defined(STARRY_TRACE)
    TraceHit("UpdateCostTime", g_hitsUpdateCost.fetch_add(1) + 1);
#endif
    if (g_origUpdateCost) g_origUpdateCost(thiz, method);
    BattleTick(thiz);
}
#endif

// CountryDataMgr.GetCountryDataById 是热更后仍保留的稳定入口：拿实例、拿权威 cid、
// 并顺带捕获玩家 CountryData。这样资源钩不再依赖 IsPlayerCountry 的旧字节链。
static void* Hooked_GetCountryMgr(void* thiz, int32_t id, void* method) {
    if (thiz && (g_countryMgr.load() != thiz || g_playerCountryId.load() < 0))
        CaptureCountryMgr(thiz);
    void* result = g_origGetCountryById ? g_origGetCountryById(thiz, id, method) : nullptr;
    const int32_t pid = g_playerCountryId.load();
    if (result && pid > 0 && id == pid) g_playerCountry.store(result);
    return result;
}

// CityDataMgr.GetCityData 同样是热更稳定的实例入口，军队/人口不再依赖旧静态槽链。
static void* Hooked_GetCityMgr(void* thiz, int32_t id, void* method) {
    if (thiz) g_cityMgr.store(thiz);
    return g_origGetCityById ? g_origGetCityById(thiz, id, method) : nullptr;
}

static void Hooked_OnBattleEnd(void* thiz, bool win, void* method) {
    if (g_win.load()) win = true;
    if (g_origOnBattleEnd) g_origOnBattleEnd(thiz, win, method);
}

static float Hooked_GetDelta(void* method) {
    float d = g_origGetDelta ? g_origGetDelta(method) : 0.016f;
    const float m = SpeedMul();
    if (m > 1.f) d *= m;
    // UpdateBattleTime 在乐变空页时：用 delta 帧推进瞬间胜利
    if (g_win.load() && g_battleActive.load() && !g_battleEnded.load() && !g_origUpdateBattleTime) {
        if (g_battleFrames.fetch_add(1) + 1 == 45) TryForceWin(g_battleMgr.load());
    }
    return d;
}

// --- Trace 专用：资源发放（恒调 orig；thiz 为执行中的活对象，读单个 id 字段与游戏自身等价）---

static int32_t CountryIdOf(void* countryData) {
    if (!countryData) return -1;
    return *reinterpret_cast<int32_t*>(static_cast<char*>(countryData) +
                                              g_offCountryId.load());
}

static void Hooked_AddResTrade(void* thiz, int32_t resType, int64_t cnt, bool isTrading,
                               void* method) {
#if defined(STARRY_TRACE)
    const uint32_t n = g_hitsAddRes.fetch_add(1) + 1;
    // 大额不抽样（物资车/大额发放）；小额抽样防刷屏
    if (cnt >= 100 || n <= 40 || (n % 25) == 0)
        WHRHX_TRACE("CountryData.AddRes cid=%d type=%d cnt=%lld trade=%d hits=%u", CountryIdOf(thiz),
                    resType, (long long)cnt, isTrading ? 1 : 0, n);
#endif
    if (g_origAddResTrade) g_origAddResTrade(thiz, resType, cnt, isTrading, method);
}

static void Hooked_AddResCamp(void* thiz, int32_t resType, int64_t cnt, void* method) {
#if defined(STARRY_TRACE)
    const uint32_t n = g_hitsAddResCamp.fetch_add(1) + 1;
    if (cnt >= 100 || n <= 40 || (n % 25) == 0)
        WHRHX_TRACE("CampaignData.AddRes cid=%d type=%d cnt=%lld hits=%u", CountryIdOf(thiz),
                    resType, (long long)cnt, n);
#endif
    if (g_origAddResCamp) g_origAddResCamp(thiz, resType, cnt, method);
}

// --- 实体数值直写（真实数值钉大：所有花费/检查路径天然通过，不依赖物资车）---
// EffectData: m_type@0x10 / m_baseValue@0x14(float) / m_countryId@0x20；Army: m_totalNum@0x10(long) m_countryId@0x98；
// Pop: m_totalNum@0x10(long)。读写全走 process_vm 自进程系统调用（坏地址=失败返回，不 SIGSEGV——粮食闪退根因即裸写坏指针）。

static bool KRead(const void* addr, void* out, size_t len) {
    if (!addr || !out) return false;
    iovec local{out, len};
    iovec remote{const_cast<void*>(addr), len};
    return syscall(__NR_process_vm_readv, getpid(), &local, 1, &remote, 1, 0) == (ssize_t)len;
}

static bool KWrite(void* addr, const void* src, size_t len) {
    if (!addr || !src) return false;
    iovec local{const_cast<void*>(src), len};
    iovec remote{addr, len};
    return syscall(__NR_process_vm_writev, getpid(), &local, 1, &remote, 1, 0) == (ssize_t)len;
}

static bool FxMatches(void* fx, int32_t pid, const int32_t* types, size_t n, int32_t* gotType) {
    if (!fx) return false;
    int32_t type = 0, cid = -1;
    if (!KRead(static_cast<char*>(fx) + 0x10, &type, sizeof(type))) return false;
    if (!KRead(static_cast<char*>(fx) + 0x20, &cid, sizeof(cid))) return false;
    if (gotType) *gotType = type;
    if (cid != pid) return false;
    for (size_t i = 0; i < n; i++)
        if (type == types[i]) return true;
    return false;
}

static void PinResFx(void* resSlot, std::atomic<void*>& cachedFx, int32_t pid,
                     const int32_t* types, size_t n, const char* tag) {
    void* res = nullptr;
    if (!KRead(resSlot, &res, sizeof(res)) || !res) return;
    void* fx = nullptr;
    if (!KRead(static_cast<char*>(res) + kOffResDataNumFx, &fx, sizeof(fx)) || !fx) return;
    int32_t gotType = -1;
    if (!FxMatches(fx, pid, types, n, &gotType)) {
        void* alt = cachedFx.load();
        if (!FxMatches(alt, pid, types, n, &gotType)) {
            const uint32_t s = g_pinSkip.fetch_add(1) + 1;
            if (s <= 6) WHRHX_TRACE("pin %s skip: type=%d (pid=%d)", tag, gotType, pid);
            return;
        }
        fx = alt;
    }
    cachedFx.store(fx);  // 校验通过的指针回填缓存（Effect 显示钩 / 下轮 pin 兜底复用）
    float cur = 0.f;
    if (!KRead(static_cast<char*>(fx) + kOffFxBaseValue, &cur, sizeof(cur))) return;
    if (cur < 99999990.0f) {
        const float v = 99999999.0f;
        if (KWrite(static_cast<char*>(fx) + kOffFxBaseValue, &v, sizeof(v))) {
            const uint32_t k = g_pins.fetch_add(1) + 1;
            if (k <= 10) WHRHX_TRACE("pin %s old=%.1f → 99999999", tag, cur);
        }
    }
}

static int64_t NowMs() {
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return static_cast<int64_t>(ts.tv_sec) * 1000 + ts.tv_nsec / 1000000;
}

// v23 身份链（Tick 线程，零钩子）：复刻 IsPlayerCountry 的取值序列——
// 静态槽 → MethodInfo* → 直呼 Singleton<CountryDataMgr>.get_Instance(mi) → mgr → KRead +0x30 = 权威 cid。
// Cost 钩存的 thiz 经 KRead(+0xE0) 与 cid 匹配 → g_playerCountry。全程不碰 IsPlayerCountry 本体。
static bool ResolveIdentityChainLEB() {
    if (g_getInstFn) return true;
    void* ipc = ResolveRva(RVA_IS_PLAYER_LEB);  // 序言校验兼乐变构建标记
    if (!ipc) return false;
    uint32_t adrp = 0, ldr1 = 0, ldr2 = 0;
    if (!KRead(static_cast<char*>(ipc) + RVA_IS_PLAYER_DECODE_OFF, &adrp, 4)) return false;
    if (!KRead(static_cast<char*>(ipc) + RVA_IS_PLAYER_DECODE_OFF + 4, &ldr1, 4)) return false;
    if (!KRead(static_cast<char*>(ipc) + RVA_IS_PLAYER_DECODE_OFF + 8, &ldr2, 4)) return false;
    if ((adrp & 0x9F000000u) != 0x90000000u) return false;   // adrp x8
    if ((ldr1 & 0xFFC00000u) != 0xF9400000u) return false;   // ldr x8,[x8,#imm]
    if (ldr2 != 0xF9400100u) return false;                   // ldr x0,[x8]
    int64_t imm = ((adrp >> 5) & 0x7FFFFull) << 2 | ((adrp >> 29) & 3ull);
    if (imm & (1 << 20)) imm -= (1 << 21);                   // 21 位符号扩展
    const uintptr_t page = (reinterpret_cast<uintptr_t>(ipc) & ~uintptr_t(0xFFF)) + (imm << 12);
    const uintptr_t slot = page + (((ldr1 >> 10) & 0xFFF) * 8);
    void* holder = nullptr;
    if (!KRead(reinterpret_cast<void*>(slot), &holder, 8) || !holder) return false;
    void* mi = nullptr;
    if (!KRead(holder, &mi, 8) || !mi) return false;
    void* fn = FileOffToVaBest(GameUnity::TARGET_LIB, RVA_GET_INSTANCE_LEB);
    if (!fn) return false;
    uint32_t p0 = 0;
    if (!ReadInsn(fn, &p0) || p0 != 0xF81C0FF7) return false;  // get_Instance 序言
    g_mi = mi;
    g_getInstFn = fn;
    LOGI("[Whrhx] identity chain ready mi=%p fn=%p", mi, fn);
    return true;
}

// 热更后旧字节链可能漂移：优先用运行时 IL2CPP API 按类名/字段名取权威布局。
// 只读调用 getter，不 Dobby；拿不到 API 时由 CaptureCountryMgr 用旧偏移兜底。
static bool ResolveCountryIdentityApi() {
    if (g_identityApiResolved.load()) return true;
    if (!EnsureIl2CppApi()) return false;
    void* mgrClass = FindClassNs("", "CountryDataMgr");
    if (!mgrClass) return false;
    void* mi = GetApi().GetMethod(mgrClass, "get_m_countryId", 0);
    void* fn = MethodPointer(mi);
    if (!fn || !Il2Cpp::IsValidHeapPtr(fn) ||
        !FnInLibExec(fn, GameUnity::TARGET_LIB, nullptr))
        return false;
    if (GetApi().method_get_name) {
        const char* name = GetApi().method_get_name(mi);
        if (!name || strcmp(name, "get_m_countryId") != 0) return false;
    }
    size_t mgrOff = 0;
    if (GetApi().class_get_field && GetApi().field_offset) {
        void* field = GetApi().class_get_field(mgrClass, "<m_countryId>k__BackingField");
        if (field) mgrOff = GetApi().field_offset(field);
    }
    size_t countryOff = 0;
    if (void* countryClass = FindClassNs("", "CountryData");
        countryClass && GetApi().class_get_field && GetApi().field_offset) {
        void* field = GetApi().class_get_field(countryClass, "<m_id>k__BackingField");
        if (field) countryOff = GetApi().field_offset(field);
    }
    if (mgrOff > 0 && mgrOff < 0x10000) g_offMgrCountryId.store(mgrOff);
    if (countryOff > 0 && countryOff < 0x10000) g_offCountryId.store(countryOff);
    g_countryIdMi = mi;
    g_countryIdFn = fn;
    g_identityApiResolved.store(true);
    LOGI("[Whrhx] identity api ready mgrOff=0x%lX countryOff=0x%lX fn=%p",
         (unsigned long)g_offMgrCountryId.load(), (unsigned long)g_offCountryId.load(), fn);
    return true;
}

static void CaptureCountryMgr(void* mgr) {
    if (!mgr) return;
    g_countryMgr.store(mgr);
    int32_t cid = -1;
    if (!KRead(static_cast<char*>(mgr) + g_offMgrCountryId.load(), &cid, sizeof(cid)) ||
        cid <= 0) {
        if (g_countryIdFn && g_countryIdMi)
            cid = reinterpret_cast<GetCountryId_t>(g_countryIdFn)(mgr, g_countryIdMi);
    }
    if (cid > 0) {
        const int32_t oldCid = g_playerCountryId.load();
        if (cid != oldCid) {
            g_playerCountry.store(nullptr);
            g_playerMoneyFx.store(nullptr);
            g_playerFoodFx.store(nullptr);
            g_playerArmyFx.store(nullptr);
        }
        if (cid != oldCid)
            WHRHX_TRACE("player cid=%d (mgr capture)", cid);
        g_playerCountryId.store(cid);
    }
}

// 2026-10-11 新增：身份链按名兜底。
// RVA_IS_PLAYER_LEB / RVA_GET_INSTANCE_LEB 在热更后都已漂移（均不是函数入口），
// 且 F81C0FF7 序言在 201MB so 里有 12994 处碰撞，无法按序言定位 get_Instance。
// 故改用 il2cpp API 按名取 Singleton<CountryDataMgr>.get_Instance 的 MethodInfo，
// 再用 runtime_invoke 直调：泛型共享 thunk 的调用约定交给 runtime，避免裸函数指针误调。
static std::atomic<void*> g_countryMgrMi{nullptr};

static void* ResolveCountryMgrByName() {
    if (!EnsureIl2CppApi()) return nullptr;
    void* mi = g_countryMgrMi.load();
    if (!mi) {
        void* cls = FindClassNs("", "CountryDataMgr");
        if (!cls) return nullptr;
        void* m = GetApi().GetMethod(cls, "get_Instance", 0);
        if (!m) return nullptr;
        if (GetApi().method_get_name) {
            const char* n = GetApi().method_get_name(m);
            if (!n || strcmp(n, "get_Instance") != 0) return nullptr;
        }
        g_countryMgrMi.store(m);
        mi = m;
        LOGI("[Whrhx] country mgr mi by-name %p", mi);
    }
    if (!GetApi().runtime_invoke) return nullptr;
    void* exc = nullptr;
    void* obj = GetApi().runtime_invoke(mi, nullptr, nullptr, &exc);
    if (exc || !obj) return nullptr;
    return obj;
}

static void RefreshPlayerIdentityTick() {
    ResolveCountryIdentityApi();
    const int64_t now = NowMs();
    if (now - g_lastMgrMs.load() >= 2000) {
        g_lastMgrMs.store(now);
        void* mgr = g_countryMgr.load();
#ifndef STARRY_GAME_WHRHXKB
        if (!mgr && ResolveIdentityChainLEB())
            mgr = reinterpret_cast<GetMgrInst_t>(g_getInstFn)(g_mi);
#endif
        // 按名兜底（不依赖 RVA，热更漂移后仍可用；KB 包同样受益）
        if (!mgr) mgr = ResolveCountryMgrByName();
        if (mgr) CaptureCountryMgr(mgr);
        PinCityArmies();
    }
    void* thiz = g_lastCostCountry.load();
    if (thiz) {
        int32_t ccid = -1;
        if (KRead(static_cast<char*>(thiz) + g_offCountryId.load(), &ccid, sizeof(ccid)) &&
            ccid >= 0 &&
            ccid == g_playerCountryId.load())
            g_playerCountry.store(thiz);
    }
}

static void RefreshPlayerPtrsTick() {
    void* country = g_playerCountry.load();
    if (!country) return;
    const char* base = static_cast<const char*>(country);
    void* armyData = nullptr;
    if (KRead(base + kOffCountryArmy, &armyData, sizeof(armyData)) && armyData) {
        void* armyFx = nullptr;
        if (KRead(static_cast<char*>(armyData) + kOffArmyPercentFx, &armyFx, sizeof(armyFx)) &&
            armyFx)
            g_playerArmyFx.store(armyFx);
    }
    // 钱/粮 EffectData 由 PinResFx 校验 type+cid 后回填缓存（g_playerMoneyFx/FoodFx）
}

// il2cpp 对象 klass 名（对象+0x0=klass，klass+0x10=name；仅非泛型类可靠）
static bool ObjKlassNameIs(void* obj, const char* want) {
    if (!obj) return false;
    void* klass = nullptr;
    if (!KRead(obj, &klass, 8) || !klass) return false;
    void* namePtr = nullptr;
    if (!KRead(static_cast<char*>(klass) + 0x10, &namePtr, 8) || !namePtr) return false;
    char buf[24] = {0};
    if (!KRead(namePtr, buf, sizeof(buf) - 1)) return false;
    return strncmp(buf, want, sizeof(buf) - 1) == 0;
}

// 泛型共享实现的 mi 的 klass 是开放类 Singleton`1，无法按名区分 T；
// 改形状验证：直呼 getter，看管理器 +0x10 字典里是否装着 CityData 对象
static bool ResolveCityMgrChain() {
    if (g_cityMi) return true;
    if (!g_getInstFn) return false;
    static std::atomic<int> s_diag{0};
    const bool diag = s_diag.fetch_add(1) < 2;  // 只在前两轮打全量诊断
    for (uintptr_t slotRva : kCityMgrSlotCandidates) {
        void* slot = FileOffToVaAny(GameUnity::TARGET_LIB, slotRva);
        if (!slot) { if (diag) LOGW("[citydiag] slot 0x%lX va=null", (unsigned long)slotRva); continue; }
        void* holder = nullptr;
        if (!KRead(slot, &holder, 8) || !holder) { if (diag) LOGW("[citydiag] slot 0x%lX holder fail", (unsigned long)slotRva); continue; }
        void* mi = nullptr;
        if (!KRead(holder, &mi, 8) || !mi) { if (diag) LOGW("[citydiag] slot 0x%lX mi fail", (unsigned long)slotRva); continue; }
        void* mgr = reinterpret_cast<GetMgrInst_t>(g_getInstFn)(mi);
        if (diag) LOGW("[citydiag] slot 0x%lX mi=%p mgr=%p", (unsigned long)slotRva, mi, mgr);
        if (!mgr) continue;
        void* dict = nullptr;
        if (!KRead(static_cast<char*>(mgr) + 0x10, &dict, 8) || !dict) { if (diag) LOGW("[citydiag] mgr=%p +0x10 null", mgr); continue; }
        void* entries = nullptr;
        if (!KRead(static_cast<char*>(dict) + 0x18, &entries, 8) || !entries) { if (diag) LOGW("[citydiag] mgr=%p dict=%p entries null", mgr, dict); continue; }
        int32_t maxLen = 0;
        if (!KRead(static_cast<char*>(entries) + 0x18, &maxLen, 4) || maxLen <= 0 || maxLen > 65536) {
            if (diag) LOGW("[citydiag] mgr=%p maxLen=%d bad", mgr, maxLen);
            continue;
        }
        char* data = static_cast<char*>(entries) + 0x20;
        bool isCity = false;
        int32_t probed = 0, named = 0;
        char firstName[24] = {0};
        for (int32_t i = 0; i < maxLen && i < 512; i++) {
            void* city = nullptr;
            if (!KRead(data + static_cast<size_t>(i) * 0x18 + 0x10, &city, 8) || !city) continue;
            probed++;
            if (ObjKlassNameIs(city, "CityData")) { isCity = true; break; }
            if (!firstName[0] && city) {
                void* klass = nullptr;
                if (KRead(city, &klass, 8) && klass) {
                    void* np = nullptr;
                    if (KRead(static_cast<char*>(klass) + 0x10, &np, 8) && np)
                        KRead(np, firstName, sizeof(firstName) - 1);
                    named++;
                }
            }
        }
        if (diag)
            LOGW("[citydiag] mgr=%p maxLen=%d probed=%d firstName=%s", mgr, maxLen, probed,
                 firstName[0] ? firstName : "<none>");
        if (isCity) {
            g_cityMi = mi;
            LOGI("[Whrhx] city mgr chain ready (slot 0x%lX)", (unsigned long)slotRva);
            return true;
        }
    }
    if (diag) LOGW("[citydiag] all candidates failed this round");
    return false;
}

// 遍历 CityDataMgr.m_cityDataDic，钉玩家各城驻军（源头值大 → 汇总自然大，不回跳）
static void PinCityArmies() {
    if (!g_army.load() && !g_pop.load()) return;
    const int32_t pid = g_playerCountryId.load();
    if (pid < 0) return;
    void* mgr = g_cityMgr.load();
#ifndef STARRY_GAME_WHRHXKB
    if (!mgr && ResolveCityMgrChain())
        mgr = reinterpret_cast<GetMgrInst_t>(g_getInstFn)(g_cityMi);
#endif
    if (!mgr) return;
    void* dict = nullptr;
    if (!KRead(static_cast<char*>(mgr) + 0x10, &dict, 8) || !dict) return;
    void* entries = nullptr;
    if (!KRead(static_cast<char*>(dict) + 0x18, &entries, 8) || !entries) return;
    int32_t maxLen = 0;
    if (!KRead(static_cast<char*>(entries) + 0x18, &maxLen, 4) || maxLen <= 0 || maxLen > 8192)
        return;
    char* base = static_cast<char*>(entries) + 0x20;  // entries 数据区
    int pinned = 0;
    for (int32_t i = 0; i < maxLen; i++) {
        char* ent = base + static_cast<size_t>(i) * 0x18;
        void* city = nullptr;
        if (!KRead(ent + 0x10, &city, 8) || !city) continue;
        int32_t owner = -1, cityId = -1;
        if (!KRead(static_cast<char*>(city) + 0x14, &owner, 4) || owner != pid) continue;
        void* armyData = nullptr;
        if (!KRead(static_cast<char*>(city) + 0x38, &armyData, 8) || !armyData) continue;
        void* fx = nullptr;
        if (!KRead(static_cast<char*>(armyData) + 0x10, &fx, 8) || !fx) continue;
        int32_t fxOwner = -1, fxCity = -1;
        if (!KRead(static_cast<char*>(fx) + 0x20, &fxOwner, 4) || fxOwner != pid) continue;
        if (!KRead(static_cast<char*>(fx) + 0x28, &fxCity, 4)) continue;
        if (fxCity != -1 && cityId >= 0 && fxCity != cityId) {
            if (!KRead(static_cast<char*>(city) + 0x10, &cityId, 4) || fxCity != cityId) continue;
        }
        float cur = 0.f;
        if (!KRead(static_cast<char*>(fx) + kOffFxBaseValue, &cur, sizeof(cur))) continue;
        if (cur < 999990.0f) {
            const float v = 999999.0f;
            if (KWrite(static_cast<char*>(fx) + kOffFxBaseValue, &v, sizeof(v))) pinned++;
        }
        // 人口真实化：城人口 m_num@0x10 与上限 m_maxNum@0x18（CityData.PopulationData@city+0x30）。
        // KWrite 只防崩溃不防语义破坏——人口 fx 必须与军队同级别校验（countryId==pid），
        // 13:2x 闪退（ES3 存档→Type.GetType→strcmp）即无校验写穿别的对象所致。
        if (g_pop.load()) {
            void* popData = nullptr;
            if (KRead(static_cast<char*>(city) + 0x30, &popData, 8) && popData) {
                void* pfx = nullptr;
                if (KRead(static_cast<char*>(popData) + 0x10, &pfx, 8) && pfx) {  // m_num@0x10，漏偏移实锤过
                    int32_t pfxOwner = -1;
                    float pcur = 0.f;
#if defined(STARRY_TRACE)
                    static std::atomic<int> s_pd{0};
                    if (s_pd.fetch_add(1) < 3) {
                        int32_t pfxType = -1;
                        float pcurRaw = 0.f;
                        KRead(static_cast<char*>(pfx) + 0x10, &pfxType, 4);
                        KRead(static_cast<char*>(pfx) + kOffFxBaseValue, &pcurRaw, sizeof(pcurRaw));
                        WHRHX_TRACE("popdiag city=%p popData=%p pfx=%p type=%d owner=%d cur=%.1f",
                                    city, popData, pfx, pfxType, pfxOwner, pcurRaw);
                    }
#endif
                    if (KRead(static_cast<char*>(pfx) + 0x20, &pfxOwner, 4) && pfxOwner == pid &&
                        KRead(static_cast<char*>(pfx) + kOffFxBaseValue, &pcur, sizeof(pcur))) {
                        if (pcur < 999990.0f) {
                            const float pv = 999999.0f;
                            if (KWrite(static_cast<char*>(pfx) + kOffFxBaseValue, &pv, sizeof(pv)))
                                pinned++;
                        }
                        int64_t maxNum = 0;
                        if (KRead(static_cast<char*>(popData) + 0x18, &maxNum, 8) &&
                            maxNum > 0 && maxNum < 99999999) {
                            const int64_t mv = 99999999;
                            KWrite(static_cast<char*>(popData) + 0x18, &mv, sizeof(mv));
                        }
                    }
                }
            }
        }
    }
    if (pinned > 0) {
        const uint32_t k = g_pins.fetch_add(1) + 1;
        if (k <= 12) WHRHX_TRACE("pin cityArmies cities=%d (pid=%d)", pinned, pid);
    }
}

static void PinPlayerValues() {
    RefreshPlayerIdentityTick();
    if (g_playerCountryId.load() < 0) return;  // 无 mgr 钩（官方库）或未进战役 → 经济功能惰性
    RefreshPlayerPtrsTick();
    void* country = g_playerCountry.load();
    if (!country) return;
    const int32_t pid = g_playerCountryId.load();
    char* base = static_cast<char*>(country);
    static const int32_t kMoneyTypes[] = {kEffMoney, kEffStandardMoney};
    static const int32_t kFoodTypes[] = {kEffFood, kEffStandardFood};

    if (g_gold.load())
        PinResFx(base + kOffCountryMoney, g_playerMoneyFx, pid, kMoneyTypes, 2, "money");
    if (g_food.load())
        PinResFx(base + kOffCountryFood, g_playerFoodFx, pid, kFoodTypes, 2, "food");
}

// --- 按功能懒加载（启动零挂点；仅开关 ON 才 Dobby；不依赖 Il2Cpp API）---

static void InstallEcoHooks() {
    if (g_hooksEco.load() && g_origGetCountryById && g_origGetCityById) return;
    static std::atomic<uint32_t> s_attempts{0};
    const uint32_t attempt = s_attempts.fetch_add(1);
    if (attempt > 0 && (attempt % 10) != 0) return;
    int n = 0, primary = 0;
    // 身份捕获与资源钩分离：即使某个 Cost 方法漂移，管理器入口仍能把玩家链立起来。
    static const int kOneArgcs[] = {1};
    if (!g_origGetCountryById) {
#ifdef STARRY_GAME_WHRHXKB
        HookRvaOrIl2Cpp("GetCountryDataById", "", "CountryDataMgr", "GetCountryDataById",
                            kOneArgcs, 1, RVA_KB_GET_COUNTRY_MGR, (void*)Hooked_GetCountryMgr,
                            (void**)&g_origGetCountryById, false, 0);
#else
        HookRvaOrIl2Cpp("GetCountryDataById", "", "CountryDataMgr", "GetCountryDataById",
                            kOneArgcs, 1, RVA_GET_COUNTRY_MGR_LEB, (void*)Hooked_GetCountryMgr,
                            (void**)&g_origGetCountryById, false, 0);
#endif
    }
    if (!g_origGetCityById) {
#ifdef STARRY_GAME_WHRHXKB
        HookRvaOrIl2Cpp("GetCityData", "", "CityDataMgr", "GetCityData",
                            kOneArgcs, 1, RVA_KB_GET_CITY_MGR, (void*)Hooked_GetCityMgr,
                            (void**)&g_origGetCityById, false, 0);
#else
        HookRvaOrIl2Cpp("GetCityData", "", "CityDataMgr", "GetCityData",
                            kOneArgcs, 1, RVA_GET_CITY_MGR_LEB, (void*)Hooked_GetCityMgr,
                            (void**)&g_origGetCityById, false, 0);
#endif
    }
    static const int kCostSingleArgcs[] = {2};
    static const int kCostResArgcs[] = {1};
    static const int kCheckResArgcs[] = {2};
#ifdef STARRY_GAME_WHRHXKB
    // kb 8/20 构建：CountryData.CostRes / CanAddArmy 已删 → CostSingleRes + CheckRes 双旁路
    if (HookRvaOrIl2Cpp("CostSingleRes", "", "CountryData", "CostSingleRes", kCostSingleArgcs, 1,
                        RVA_KB_COST_SINGLE, (void*)Hooked_CostSingleRes,
                        (void**)&g_origCostSingle, true, 0))
        ++n, ++primary;
    if (HookRvaOrIl2Cpp("CheckRes", "", "CountryData", "CheckRes", kCheckResArgcs, 1,
                        RVA_KB_CHECK_RES, (void*)Hooked_CheckRes, (void**)&g_origCheckRes, true, 0))
        ++n, ++primary;
#else
    if (HookRvaOrIl2Cpp("CostSingleRes", "", "CountryData", "CostSingleRes", kCostSingleArgcs, 1,
                        RVA_COST_SINGLE_LEB, (void*)Hooked_CostSingleRes,
                        (void**)&g_origCostSingle, true, 0))
        ++n, ++primary;
    if (HookRvaOrIl2Cpp("CostRes", "", "CountryData", "CostRes", kCostResArgcs, 1,
                        RVA_COST_RES_LEB, (void*)Hooked_CostRes, (void**)&g_origCostRes, true, 0))
        ++n, ++primary;
    if (HookRvaOrIl2Cpp("CheckRes", "", "CountryData", "CheckRes", kCheckResArgcs, 1,
                        RVA_CHECK_RES_LEB, (void*)Hooked_CheckRes, (void**)&g_origCheckRes, true,
                        0))
        ++n, ++primary;
#endif
#if defined(STARRY_TRACE)
    // Trace 附加：AddRes×2（只观测恒调 orig）
    static const int kAddTradeArgcs[] = {3};
    static const int kAddCampArgcs[] = {2};
#ifdef STARRY_GAME_WHRHXKB
    if (HookRvaOrIl2Cpp("AddRes", "", "CountryData", "AddRes", kAddTradeArgcs, 1, RVA_KB_ADD_RES,
                        (void*)Hooked_AddResTrade, (void**)&g_origAddResTrade, true, 0))
        ++n;
    if (HookRvaOrIl2Cpp("AddResCampaign", "", "CampaignData", "AddRes", kAddCampArgcs, 1,
                        RVA_KB_ADD_RES_CAMP, (void*)Hooked_AddResCamp, (void**)&g_origAddResCamp,
                        true, 0))
        ++n;
#else
    if (HookRvaOrIl2Cpp("AddRes", "", "CountryData", "AddRes", kAddTradeArgcs, 1, RVA_ADD_RES_LEB,
                        (void*)Hooked_AddResTrade, (void**)&g_origAddResTrade, true, 0))
        ++n;
    if (HookRvaOrIl2Cpp("AddResCampaign", "", "CampaignData", "AddRes", kAddCampArgcs, 1,
                        RVA_ADD_RES_CAMPAIGN, (void*)Hooked_AddResCamp, (void**)&g_origAddResCamp,
                        true, 0))
        ++n;
#endif
#endif
    // 军队校正总口同步回钉
    static const int kCorrectArgcs[] = {0};
#ifdef STARRY_GAME_WHRHXKB
    if (HookRvaOrIl2Cpp("ArmyCorrectMgr", "", "CountryDataMgr", "CorrectArmyData", kCorrectArgcs, 1,
                        RVA_KB_ARMY_CORRECT, (void*)Hooked_ArmyCorrect,
                        (void**)&g_origArmyCorrect, true, 0))
        ++n, ++primary;
#else
    if (HookRvaOrIl2Cpp("ArmyCorrectMgr", "", "CountryDataMgr", "CorrectArmyData", kCorrectArgcs, 1,
                        RVA_ARMY_CORRECT_LEB, (void*)Hooked_ArmyCorrect,
                        (void**)&g_origArmyCorrect, true, 0))
        ++n, ++primary;
#endif
    if (n > 0) {
        LOGI("[Whrhx] eco hooks=%d primary=%d capture=%d", n, primary,
             g_origGetCountryById ? 1 : 0);
    }
#ifdef STARRY_GAME_WHRHXKB
    const int required = 3;
#else
    const int required = 4;
#endif
    if (primary >= required) {
        g_hooksEco.store(true);
        LOGI("[Whrhx] eco hooks ready primary=%d", primary);
    }
}

#ifdef STARRY_GAME_WHRHXKB
static void InstallBattleHooks() {
    if (g_hooksBattle.load()) return;
    int n = 0;
    // 结算：BattleControlMgr.SetEnd（同名沿用；签名 5/6 变体按名兜底）
    if (!g_origBattleSetEnd) {
        static const int kSetEndArgcs[] = {5, 6, 4, 3};
        if (HookRvaOrIl2Cpp("BattleSetEnd", "", "BattleControlMgr", "SetEnd", kSetEndArgcs, 4,
                            RVA_KB_SET_END, nullptr, (void**)&g_origBattleSetEnd, true, 0))
            ++n;
    }
    // 兜底：LockStepMgr.OnBattleEnd（首指令 B 跳板 → resolve-only 不挂）
    if (!g_origBattleSetEnd && !g_origOnBattleEnd) {
        void* addr = ResolveRva(RVA_KB_ON_BATTLE_END);
        if (addr) {
            g_origOnBattleEnd = reinterpret_cast<OnBattleEnd_t>(addr);
            LOGI("[Whrhx] resolved OnBattleEnd @ %p off=0x%lX (kb resolve-only)", addr,
                 (unsigned long)RVA_KB_ON_BATTLE_END);
        }
    }
    if (!g_origBattleSetEnd && !g_origOnBattleEnd) {
        static const int kSetEndArgcs[] = {5, 6, 4, 3};
        static const int kEndArgcs[] = {1, 2, 0};
        (void)ResolveOrigAny("BattleSetEnd", "", "BattleControlMgr", "SetEnd", kSetEndArgcs, 4, 0,
                             0, (void**)&g_origBattleSetEnd);
        if (!g_origBattleSetEnd && !g_origOnBattleEnd) {
            void* p = nullptr;
            if (ResolveOrigAny("OnBattleEnd", "BattleOnline.Battle", "LockStepMgr", "OnBattleEnd",
                               kEndArgcs, 3, 0, 0, &p))
                g_origOnBattleEnd = reinterpret_cast<OnBattleEnd_t>(p);
        }
    }
    // tick 双候选（Trace 分辨定稿）；StartTimer 开战标记（失败无碍 — BattleTick 自臂）
    static const int kZeroArgcs[] = {0};
    if (!g_origOnCountDown &&
        HookRvaOrIl2Cpp("OnCountDown", "", "BattleControlMgr", "OnCountDown", kZeroArgcs, 1,
                        RVA_KB_ON_COUNTDOWN, (void*)Hooked_OnCountDown,
                        (void**)&g_origOnCountDown, true, 0))
        ++n;
    if (!g_origUpdateCost &&
        HookRvaOrIl2Cpp("UpdateBattleCostTime", "", "BattleControlMgr", "UpdateBattleCostTime",
                        kZeroArgcs, 1, RVA_KB_UPDATE_COST, (void*)Hooked_UpdateCostTime,
                        (void**)&g_origUpdateCost, true, 0))
        ++n;
    if (!g_origStartBattle &&
        HookRvaOrIl2Cpp("StartTimer", "", "BattleControlMgr", "StartTimer", kZeroArgcs, 1,
                        RVA_KB_START_TIMER, (void*)Hooked_StartBattle,
                        (void**)&g_origStartBattle, false, 0))
        ++n;
    const bool hasEnd = g_origBattleSetEnd || g_origOnBattleEnd;
    if (n >= 1 && hasEnd) {
        g_hooksBattle.store(true);
        LOGI("[Whrhx] kb battle hooks=%d setEnd=%d onEnd=%d countDown=%d updateCost=%d", n,
             g_origBattleSetEnd ? 1 : 0, g_origOnBattleEnd ? 1 : 0, g_origOnCountDown ? 1 : 0,
             g_origUpdateCost ? 1 : 0);
    } else {
        g_hooksBattle.store(false);
        LOGW("[Whrhx] kb battle incomplete hooks=%d end=%d — will retry", n, hasEnd ? 1 : 0);
    }
}
#else
static void InstallBattleHooks() {
    const bool needEnd = !g_origBattleSetEnd && !g_origOnBattleEnd;
    if (g_hooksBattle.load() && !needEnd) return;

    // 路径 A：官方 SetEnd RVA（空页则拒）。禁止过早按名——乐变上易解析到错误符号，堵死兜底。
    if (!g_origBattleSetEnd) {
        void* addr = ResolveRva(RVA_BATTLE_SET_END);
        if (addr) {
            g_origBattleSetEnd = reinterpret_cast<BattleSetEnd_t>(addr);
            LOGI("[Whrhx] resolved BattleSetEnd @ %p off=0x%lX (official rva)", addr,
                 (unsigned long)RVA_BATTLE_SET_END);
        }
    }
    // 路径 A2：乐变 SetEnd（dump 实锤 0x32FF644）
    if (!g_origBattleSetEnd) {
        void* addr = ResolveRva(RVA_SET_END_LEB);
        if (addr) {
            g_origBattleSetEnd = reinterpret_cast<BattleSetEnd_t>(addr);
            LOGI("[Whrhx] resolved BattleSetEnd @ %p off=0x%lX (leb rva)", addr,
                 (unsigned long)RVA_SET_END_LEB);
        }
    }
    // 路径 B：乐变 OnBattleEnd resolve-only
    if (!g_origBattleSetEnd && !g_origOnBattleEnd) {
        void* addr = ResolveRva(RVA_ON_BATTLE_END_LEB);
        if (addr) {
            g_origOnBattleEnd = reinterpret_cast<OnBattleEnd_t>(addr);
            LOGI("[Whrhx] resolved OnBattleEnd @ %p off=0x%lX (leb resolve-only)", addr,
                 (unsigned long)RVA_ON_BATTLE_END_LEB);
        }
    }
    // 路径 C：仍无结算函数时才按名找 SetEnd / OnBattleEnd
    if (!g_origBattleSetEnd && !g_origOnBattleEnd) {
        static const int kSetEndArgcs[] = {5, 6, 4, 3};
        static const int kEndArgcs[] = {1, 2, 0};
        (void)ResolveOrigAny("BattleSetEnd", "", "BattleControlMgr", "SetEnd", kSetEndArgcs, 4, 0, 0,
                             (void**)&g_origBattleSetEnd);
        if (!g_origBattleSetEnd) {
            void* p = nullptr;
            if (ResolveOrigAny("OnBattleEnd", "", "BattleUtil", "OnBattleEnd", kEndArgcs, 3, 0, 0,
                               &p) ||
                ResolveOrigAny("OnBattleEnd", "BattleOnline.Battle", "LockStepMgr", "OnBattleEnd",
                               kEndArgcs, 3, 0, 0, &p))
                g_origOnBattleEnd = reinterpret_cast<OnBattleEnd_t>(p);
        }
    }

    int n = 0;
    static const int kStartArgcs[] = {0, 1};
    // Start：官方 RVA → 乐变 dump RVA → 按名
    if (HookRvaOrIl2Cpp("StartBattle", "", "BattleControlMgr", "StartBattle", kStartArgcs, 2,
                        RVA_BATTLE_START, (void*)Hooked_StartBattle, (void**)&g_origStartBattle,
                        false, RVA_START_BATTLE_LEB))
        ++n;
    // Update：官方 RVA（乐变上空页被拒）→ 乐变 dump RVA →（按名由 HookRvaOrIl2Cpp 内处理）
    if (!g_origUpdateBattleTime) {
        static const int kUpdateArgcs[] = {0};
        if (HookRvaOrIl2Cpp("UpdateBattleTime", "", "BattleControlMgr", "UpdateBattleTime",
                            kUpdateArgcs, 1, RVA_BATTLE_UPDATE_TIME,
                            (void*)Hooked_UpdateBattleTime, (void**)&g_origUpdateBattleTime, true,
                            RVA_UPDATE_BATTLE_LEB))
            ++n;
    } else {
        ++n;
    }

    const bool hasEnd = g_origBattleSetEnd || g_origOnBattleEnd;
    if (n >= 1 && hasEnd) {
        g_hooksBattle.store(true);
        LOGI("[Whrhx] battle hooks=%d setEnd=%d onEnd=%d update=%d", n, g_origBattleSetEnd ? 1 : 0,
             g_origOnBattleEnd ? 1 : 0, g_origUpdateBattleTime ? 1 : 0);
    } else {
        g_hooksBattle.store(false);
        LOGW("[Whrhx] battle incomplete start=%d end=%d — will retry", n, hasEnd ? 1 : 0);
    }
}
#endif  // STARRY_GAME_WHRHXKB

static void InstallSpeedHooks() {
    if (g_hooksSpeed.load()) return;
    static const int kDeltaArgcs[] = {0};
#ifdef STARRY_GAME_WHRHXKB
    if (HookAtRva("get_deltaTime", RVA_KB_GET_DELTA, (void*)Hooked_GetDelta,
                  (void**)&g_origGetDelta) ||
        HookRvaOrIl2Cpp("get_deltaTime", "UnityEngine", "Time", "get_deltaTime", kDeltaArgcs, 1,
                        RVA_KB_GET_DELTA, (void*)Hooked_GetDelta, (void**)&g_origGetDelta, false))
        g_hooksSpeed.store(true);
#else
    // 乐变：真身在 LEB（官方入口常是 B stub，先挂 stub 会导致加速无效）
    // 211MB：LEB 空页被拒 → 回退官方 RVA / 按名
    if (HookAtRva("get_deltaTime", RVA_GET_DELTA_LEB, (void*)Hooked_GetDelta, (void**)&g_origGetDelta) ||
        HookRvaOrIl2Cpp("get_deltaTime", "UnityEngine", "Time", "get_deltaTime", kDeltaArgcs, 1,
                        RVA_GET_DELTA_TIME, (void*)Hooked_GetDelta, (void**)&g_origGetDelta, false))
        g_hooksSpeed.store(true);
#endif
}

// --- 科研立即完成：线程内标记仅覆盖 ResearchTech / MengHuaTechPolicyData.Research 调用栈 ---
static void ResearchScopeEnter() {
    if (t_researchDepth++ == 0) t_researchHandled = false;
}

static void ResearchScopeLeave() {
    if (t_researchDepth > 0 && --t_researchDepth == 0) t_researchHandled = false;
}

// 2026-10-11 名臣技能无CD：名臣两侧面板入口的调用栈标记（与科研作用域并列、互不干扰）
static void MinisterSkillScopeEnter() { t_ministerSkillDepth++; }
static void MinisterSkillScopeLeave() {
    if (t_ministerSkillDepth > 0) t_ministerSkillDepth--;
}

static void Hooked_ResearchTech(void* thiz, int32_t techId, void* method) {
    const bool enabled = g_researchDone.load();
    if (enabled) ResearchScopeEnter();
    if (g_origResearchTech) g_origResearchTech(thiz, techId, method);
    if (enabled) ResearchScopeLeave();
}

static void Hooked_PolicyUiExecute(void* thiz, int32_t policyId, void* method) {
    const bool enabled = g_researchDone.load();
    if (enabled) {
        LOGI("[Whrhx] policy ui execute id=%d", policyId);
        ResearchScopeEnter();
    }
    if (g_origPolicyUiExecute) g_origPolicyUiExecute(thiz, policyId, method);
    if (enabled) ResearchScopeLeave();
}

// 名臣科研入口（2026-09-23 补做）：MinisterPolicyUI / ExtraMinisterPolicyUI 与 PolicyUI 并列，
// 内部同样 bl → PolicyDataMgr.ExecutePolicy，所以只要进 scope，「立即完成 + 无条件」就自动覆盖名臣。
static void Hooked_MinisterUiExecute(void* thiz, int32_t policyId, void* method) {
    const bool enabled = g_researchDone.load();
    const bool noCd = g_ministerSkillNoCd.load();
    if (enabled) {
        LOGI("[Whrhx] minister policy ui execute id=%d", policyId);
        ResearchScopeEnter();
    }
    if (noCd) {
        LOGI("[Whrhx] minister skill noCD execute id=%d", policyId);
        MinisterSkillScopeEnter();
    }
    if (g_origMinisterUiExecute) g_origMinisterUiExecute(thiz, policyId, method);
    if (noCd) MinisterSkillScopeLeave();
    if (enabled) ResearchScopeLeave();
}

static void Hooked_ExtraMinisterUiExecute(void* thiz, int32_t policyId, void* method) {
    const bool enabled = g_researchDone.load();
    const bool noCd = g_ministerSkillNoCd.load();
    if (enabled) {
        LOGI("[Whrhx] extra minister policy ui execute id=%d", policyId);
        ResearchScopeEnter();
    }
    if (noCd) {
        LOGI("[Whrhx] extra minister skill noCD execute id=%d", policyId);
        MinisterSkillScopeEnter();
    }
    if (g_origExtraMinisterUiExecute) g_origExtraMinisterUiExecute(thiz, policyId, method);
    if (noCd) MinisterSkillScopeLeave();
    if (enabled) ResearchScopeLeave();
}

static void Hooked_TechPolicyResearch(void* thiz, void* method) {
    const bool enabled = g_researchDone.load();
    if (enabled) ResearchScopeEnter();
    if (g_origTechPolicyResearch) g_origTechPolicyResearch(thiz, method);
    if (enabled) ResearchScopeLeave();
}

// ①/③ 共用：把某政策的重复CD（PolicyData.m_repeatRemainCd）清 0
static void ClearRepeatCdNow(void* mgr, int32_t policyId) {
    if (!mgr || policyId <= 0 || !g_policyGetData) return;
    void* data = g_policyGetData(mgr, policyId, nullptr);
    if (!data) return;
    const float zero = 0.f;
    if (KWrite(static_cast<char*>(data) + kOffPolicyRepeatCd, &zero, sizeof(zero)))
        LOGI("[Whrhx] policy repeat cd cleared id=%d", policyId);
}

// 立即完成（科研 / ① 政策无CD / ③ 名臣技能无CD 三处共用同一套动作）：
// GetPolicyData → FinishPolicyTimeAnyway → 读回校验（不降则兜底写 0.1f）→ UpdatePolicyTime(1.0f)
static void PolicyFinishNow(void* mgr, int32_t id, const char* tag) {
    if (!mgr || id <= 0 || !g_policyGetData || !g_policyFinishAnyway || !g_policyUpdateTime) return;
    void* data = g_policyGetData(mgr, id, nullptr);
    float remain = 0.f;
    if (!data || !KRead(static_cast<char*>(data) + kOffPolicyRemainTime, &remain, sizeof(remain)) ||
        !(remain > 0.f)) {
        LOGW("[Whrhx] %s finish id=%d no active remain=%.3f", tag, id, remain);
        return;
    }

    g_policyFinishAnyway(mgr, id, nullptr);
    float after = remain;
    const bool readBack =
        KRead(static_cast<char*>(data) + kOffPolicyRemainTime, &after, sizeof(after));
    bool fallback = false;
    if (!readBack || !(after > 0.f) || after >= remain) {
        const float nearDone = 0.1f;
        fallback = KWrite(static_cast<char*>(data) + kOffPolicyRemainTime, &nearDone,
                           sizeof(nearDone));
    }
    g_policyUpdateTime(mgr, 1.0f, true, true, nullptr);
    LOGI("[Whrhx] %s finish id=%d remain=%.3f fallback=%d", tag, id, remain, fallback ? 1 : 0);
}

static void Hooked_PolicyExecute(void* thiz, int32_t id, float discount, bool needLog,
                                 bool isAutoSave, void* method) {
    if (g_origPolicyExecute)
        g_origPolicyExecute(thiz, id, discount, needLog, isAutoSave, method);

    // 顺手缓存 PolicyDataMgr（做字典级清 CD 用；thiz 就是 PolicyDataMgr 实例）
    if (thiz && ObjKlassNameIs(thiz, "PolicyDataMgr")) g_policyMgr.store(thiz);

    // ① 政策技能无CD：所有政策执行即完成 + 该政策重复CD 清 0
    const bool noCdPolicy = g_policyNoCd.load();
    // ③ 名臣技能无CD：只在名臣面板入口的调用栈内生效（独立生效，不依赖 ① ）
    const bool noCdMinSkill = g_ministerSkillNoCd.load() && t_ministerSkillDepth > 0;
    if (noCdPolicy || noCdMinSkill) {
        ClearRepeatCdNow(thiz, id);
        PolicyFinishNow(thiz, id, noCdMinSkill ? "noCD-minister" : "noCD-policy");
        return;
    }

    // 既有科研立即完成路径（语义与 2026-09-23 版完全一致，不动）
    if (!g_researchDone.load() || t_researchDepth <= 0 || t_researchHandled) return;
    t_researchHandled = true;
    PolicyFinishNow(thiz, id, "research");
}

// ── 面板资源门放行（与 feat#9 同一开关）───────────────────────────────────────
// 科技面板：GameUI.PolicyUI.ActivePolicy() 内联 GetResCnt(特殊资源 21/22) 与
//   PolicyConfigMgr.GetSpecialResCostList 比较，不足即 Mathf.Clamp + ShowPopUI。
// 名臣面板：MinisterPolicyUI/ExtraMinisterPolicyUI.TryActivePolicy() → PolicyDataMgr.
//   CheckPolicyDisable(学习条件) → 非 0 即 ShowPopUI（实测文案同为「资源不足」）。
// 作用域只在三个面板入口的调用栈内，离开即恢复，避免污染其它 UI 的真实数值。
static void PolicyPanelScopeEnter() { t_policyPanelDepth++; }
static void PolicyPanelScopeLeave() {
    if (t_policyPanelDepth > 0) t_policyPanelDepth--;
}

static void Hooked_PolicyUiActive(void* thiz, void* method) {
    const bool enabled = g_researchDone.load();
    if (enabled) PolicyPanelScopeEnter();
    if (g_origPolicyUiActive) g_origPolicyUiActive(thiz, method);
    if (enabled) PolicyPanelScopeLeave();
}

static void Hooked_MinisterTryActive(void* thiz, void* method) {
    const bool enabled = g_researchDone.load();
    const bool noCd = g_ministerSkillNoCd.load();
    if (enabled) PolicyPanelScopeEnter();
    if (noCd) MinisterSkillScopeEnter();
    if (g_origMinisterTryActive) g_origMinisterTryActive(thiz, method);
    if (noCd) MinisterSkillScopeLeave();
    if (enabled) PolicyPanelScopeLeave();
}

static void Hooked_ExtraMinisterTryActive(void* thiz, void* method) {
    const bool enabled = g_researchDone.load();
    const bool noCd = g_ministerSkillNoCd.load();
    if (enabled) PolicyPanelScopeEnter();
    if (noCd) MinisterSkillScopeEnter();
    if (g_origExtraMinisterTryActive) g_origExtraMinisterTryActive(thiz, method);
    if (noCd) MinisterSkillScopeLeave();
    if (enabled) PolicyPanelScopeLeave();
}

// 资源数量直接给足：冲掉 GetResCnt 与消耗表的比较（以及随后的 Clamp）
static int32_t Hooked_CountryGetResCnt(void* thiz, int32_t resType, void* method) {
    if (g_researchDone.load() && t_policyPanelDepth > 0 && thiz) return kResCntBypass;
    if (g_countryGetResCnt) return g_countryGetResCnt(thiz, resType, method);
    return 0;
}

// 政策禁用原因 → 0 表示可执行（名臣面板的学习条件门）
static int32_t Hooked_PolicyCheckDisable(void* thiz, int32_t policyId, void* method) {
    if (g_researchDone.load() && t_policyPanelDepth > 0) return 0;
    if (g_policyCheckDisable) return g_policyCheckDisable(thiz, policyId, method);
    return 0;
}

// 面板前置判断（CanActive）在内被一并放行，避免另一条早退分支直接弹窗
static bool Hooked_PolicyCanActive(void* thiz, int32_t policyId, bool isMinister, bool considerSlot,
                                   bool includeOnlyForShow, void* method) {
    if (g_researchDone.load() && t_policyPanelDepth > 0) return true;
    if (g_policyCanActive)
        return g_policyCanActive(thiz, policyId, isMinister, considerSlot, includeOnlyForShow,
                                 method);
    return false;
}

// ★ 真正的「能不能点」门（2026-09-23 续修）
// 条目在**未点击之前**就由本函数定状态：ResNotEnough(7) → UI 显示「资源不足」且不可点，
// 于是 ActivePolicy/TryActivePolicy 永不被调用、上面的作用域钩子永不生效（鸡生蛋）。
// 本函数是纯查询（只读资源、返回枚举），改返回值不写存档/数值，风险最低。
// 用户选定范围「乙」：资源/科技/前置/文化/嫁接/其他不可执行 一并转为可执行。
static constexpr int32_t kPolicyStateCanExecute = 2;  // ePolicyItemShowState.CanExecute
static int32_t Hooked_PolicyItemShowState(void* thiz, int32_t policyId, float discount, void* method) {
    const int32_t st = g_policyItemShowState
                           ? g_policyItemShowState(thiz, policyId, discount, method)
                           : kPolicyStateCanExecute;
    if (!g_researchDone.load()) return st;
    switch (st) {
        case 4:   // CultureNotEnough
        case 5:   // TecNotEnough
        case 6:   // GraftNotEnough
        case 7:   // ResNotEnough ← 用户看到的「资源不足」
        case 11:  // PreNotComplete
        case 12:  // CanNotExecute
            return kPolicyStateCanExecute;
        default:  // 0 Executed / 1 Executing / 2 CanExecute / 3 CannotShow
                  // / 8 FollowPolicy / 9 NotMission / 10 NoRemainingTimes → 保持原样
            return st;
    }
}

static void InstallPolicyPanelHooks() {
    if (g_hooksPanelGate.load()) return;
    static std::atomic<uint32_t> s_panelAttempts{0};
    const uint32_t attempt = s_panelAttempts.fetch_add(1);
    if (attempt > 0 && (attempt % 10) != 0) return;
    static const int kZeroArgcsPanel[] = {0};
    static const int kOneArgcsPanel[] = {1};
    static const int kCanActiveArgcs[] = {4};
    static const int kTwoArgcsPanel[] = {2};   // GetPolicyItemShowState(int, float)
    if (!g_origPolicyUiActive)
        HookRvaOrIl2Cpp("PolicyUiActive", "GameUI", "PolicyUI", "ActivePolicy", kZeroArgcsPanel, 1,
                        RVA_POLICY_UI_ACTIVE, (void*)Hooked_PolicyUiActive,
                        (void**)&g_origPolicyUiActive, true);
    if (!g_origMinisterTryActive)
        HookRvaOrIl2Cpp("MinisterTryActive", "GameUI", "MinisterPolicyUI", "TryActivePolicy",
                        kZeroArgcsPanel, 1, RVA_MINISTER_TRY_ACTIVE,
                        (void*)Hooked_MinisterTryActive, (void**)&g_origMinisterTryActive, true);
    if (!g_origExtraMinisterTryActive)
        HookRvaOrIl2Cpp("ExtraMinisterTryActive", "GameUI", "ExtraMinisterPolicyUI",
                        "TryActivePolicy", kZeroArgcsPanel, 1, RVA_EXTRA_MINISTER_TRY_ACTIVE,
                        (void*)Hooked_ExtraMinisterTryActive,
                        (void**)&g_origExtraMinisterTryActive, true);
    if (!g_countryGetResCnt)
        HookAtRva("CountryGetResCnt", RVA_COUNTRY_GET_RES_CNT, (void*)Hooked_CountryGetResCnt,
                  (void**)&g_countryGetResCnt);
    if (!g_policyCheckDisable)
        HookAtRva("PolicyCheckDisable", RVA_POLICY_CHECK_DISABLE, (void*)Hooked_PolicyCheckDisable,
                  (void**)&g_policyCheckDisable);
    if (!g_policyCanActive)
        HookRvaOrIl2Cpp("PolicyCanActive", "", "PolicyDataMgr", "CanActive", kCanActiveArgcs, 1,
                        RVA_POLICY_CAN_ACTIVE, (void*)Hooked_PolicyCanActive,
                        (void**)&g_policyCanActive, true);
    // 条目状态门必须最先可用：它决定条目能不能被点，点击之后的面板作用域才谈得上
    if (!g_policyItemShowState)
        HookRvaOrIl2Cpp("PolicyItemShowState", "", "PolicyDataMgr", "GetPolicyItemShowState",
                        kTwoArgcsPanel, 1, RVA_POLICY_ITEM_SHOW_STATE,
                        (void*)Hooked_PolicyItemShowState, (void**)&g_policyItemShowState, true);

    const bool ready = g_origPolicyUiActive && g_origMinisterTryActive &&
                       g_origExtraMinisterTryActive && g_countryGetResCnt && g_policyCheckDisable &&
                       g_policyCanActive && g_policyItemShowState;
    g_hooksPanelGate.store(ready);
    if (ready)
        LOGI("[Whrhx] policy panel gate ready tech=%d min=%d extra=%d res=%d can=%d disable=%d "
             "state=%d",
             g_origPolicyUiActive ? 1 : 0, g_origMinisterTryActive ? 1 : 0,
             g_origExtraMinisterTryActive ? 1 : 0, g_countryGetResCnt ? 1 : 0,
             g_policyCanActive ? 1 : 0, g_policyCheckDisable ? 1 : 0,
             g_policyItemShowState ? 1 : 0);
    else
        LOGW("[Whrhx] policy panel gate incomplete tech=%d min=%d extra=%d res=%d can=%d "
             "disable=%d state=%d — will retry",
             g_origPolicyUiActive ? 1 : 0, g_origMinisterTryActive ? 1 : 0,
             g_origExtraMinisterTryActive ? 1 : 0, g_countryGetResCnt ? 1 : 0,
             g_policyCanActive ? 1 : 0, g_policyCheckDisable ? 1 : 0,
             g_policyItemShowState ? 1 : 0);
}

static void InstallResearchHooks() {
    // 面板资源门独立重试/独立就绪标志，必须放在 research 就绪早退之前
    InstallPolicyPanelHooks();
    if (g_hooksResearch.load()) return;
    static std::atomic<uint32_t> s_attempts{0};
    const uint32_t attempt = s_attempts.fetch_add(1);
    if (attempt > 0 && (attempt % 10) != 0) return;

    static const int kOneArgcs[] = {1};
    static const int kZeroArgcs[] = {0};
    static const int kExecuteArgcs[] = {4};   // 2026-09-23: int,float,bool,bool
    static const int kUpdateArgcs[] = {3};    // 2026-09-23: float,bool,bool
    if (!g_origPolicyUiExecute)
        HookRvaOrIl2Cpp("PolicyUiExecute", "GameUI", "PolicyUI", "ExecutePolicy", kOneArgcs, 1,
                        RVA_POLICY_UI_EXECUTE, (void*)Hooked_PolicyUiExecute,
                        (void**)&g_origPolicyUiExecute, true);
    if (!g_origMinisterUiExecute)
        HookRvaOrIl2Cpp("MinisterUiExecute", "GameUI", "MinisterPolicyUI", "ExecutePolicy",
                        kOneArgcs, 1, RVA_MINISTER_UI_EXECUTE,
                        (void*)Hooked_MinisterUiExecute, (void**)&g_origMinisterUiExecute, true);
    if (!g_origExtraMinisterUiExecute)
        HookRvaOrIl2Cpp("ExtraMinisterUiExecute", "GameUI", "ExtraMinisterPolicyUI",
                        "ExecutePolicy", kOneArgcs, 1, RVA_EXTRA_MINISTER_UI_EXECUTE,
                        (void*)Hooked_ExtraMinisterUiExecute,
                        (void**)&g_origExtraMinisterUiExecute, true);
    if (!g_origResearchTech)
        HookRvaOrIl2Cpp("ResearchTech", "", "MengHuaRecordMgr", "ResearchTech", kOneArgcs, 1,
                        RVA_RESEARCH_TECH, (void*)Hooked_ResearchTech,
                        (void**)&g_origResearchTech, true);
    if (!g_origTechPolicyResearch)
        HookRvaOrIl2Cpp("TechPolicyResearch", "", "MengHuaTechPolicyData", "Research",
                        kZeroArgcs, 1, RVA_TECH_POLICY_RESEARCH,
                        (void*)Hooked_TechPolicyResearch,
                        (void**)&g_origTechPolicyResearch, true);
    if (!g_origPolicyExecute)
        HookRvaOrIl2Cpp("PolicyExecute", "", "PolicyDataMgr", "ExecutePolicy", kExecuteArgcs,
                        1, RVA_POLICY_EXECUTE, (void*)Hooked_PolicyExecute,
                        (void**)&g_origPolicyExecute, true);
    if (!g_policyGetData)
        ResolveOrigAny("PolicyGetData", "", "PolicyDataMgr", "GetPolicyData", kOneArgcs, 1,
                       RVA_POLICY_GET_DATA, 0, (void**)&g_policyGetData);
    if (!g_policyFinishAnyway)
        ResolveOrigAny("PolicyFinishAnyway", "", "PolicyDataMgr", "FinishPolicyTimeAnyway",
                       kOneArgcs, 1, RVA_POLICY_FINISH_ANYWAY, 0,
                       (void**)&g_policyFinishAnyway);
    if (!g_policyUpdateTime)
        ResolveOrigAny("PolicyUpdateTime", "", "PolicyDataMgr", "UpdatePolicyTime",
                       kUpdateArgcs, 1, RVA_POLICY_UPDATE_TIME, 0,
                       (void**)&g_policyUpdateTime);

    const bool hasEntry = g_origResearchTech || g_origTechPolicyResearch || g_origPolicyUiExecute ||
                          g_origMinisterUiExecute || g_origExtraMinisterUiExecute;
    if (hasEntry && g_origPolicyExecute && g_policyGetData && g_policyFinishAnyway &&
        g_policyUpdateTime) {
        g_hooksResearch.store(true);
        LOGI("[Whrhx] research hooks ready wrapper=%d direct=%d ui=%d execute=%d min=%d extra=%d",
             hasEntry, g_origResearchTech ? 1 : 0, g_origPolicyUiExecute ? 1 : 0,
             g_origPolicyExecute ? 1 : 0, g_origMinisterUiExecute ? 1 : 0,
             g_origExtraMinisterUiExecute ? 1 : 0);
    } else {
        LOGW("[Whrhx] research hooks incomplete wrapper=%d direct=%d ui=%d execute=%d get=%d "
             "finish=%d update=%d min=%d extra=%d — will retry",
             g_origResearchTech ? 1 : 0, g_origTechPolicyResearch ? 1 : 0,
             g_origPolicyUiExecute ? 1 : 0, g_origPolicyExecute ? 1 : 0,
             g_policyGetData ? 1 : 0, g_policyFinishAnyway ? 1 : 0,
             g_policyUpdateTime ? 1 : 0, g_origMinisterUiExecute ? 1 : 0,
             g_origExtraMinisterUiExecute ? 1 : 0);
    }
}

#ifdef STARRY_GAME_WHRHXKB
// kb 身份捕获钩（lib ready 后即装，不依赖功能开关；前 60 tick 重试）
static void InstallCaptureHooks() {
    static std::atomic<uint32_t> s_att{0};
    if (g_origGetCountryById && g_origGetCityById) return;
    if (s_att.fetch_add(1) > 60) return;
    static const int kOneArgcs[] = {1};
    if (!g_origGetCountryById)
        HookRvaOrIl2Cpp("GetCountryDataById", "", "CountryDataMgr", "GetCountryDataById",
                        kOneArgcs, 1, RVA_KB_GET_COUNTRY_MGR, (void*)Hooked_GetCountryMgr,
                        (void**)&g_origGetCountryById, false, 0);
    if (!g_origGetCityById)
        HookRvaOrIl2Cpp("GetCityData", "", "CityDataMgr", "GetCityData", kOneArgcs, 1,
                        RVA_KB_GET_CITY_MGR, (void*)Hooked_GetCityMgr,
                        (void**)&g_origGetCityById, false, 0);
    if (s_att.load() == 1)
        LOGI("[Whrhx] kb capture hooks: country=%d city=%d", g_origGetCountryById ? 1 : 0,
             g_origGetCityById ? 1 : 0);
}
#endif

// ════════════════════════════════════════════════════════════════════════════
//  2026-10-11 新增四项（feat 10..13）
//    ① 10 政策技能无CD   ② 11 名臣好感度
//    ③ 12 名臣技能无CD   ④ 13 军队规模（滑块 1..100，语义=规模加成 +X%）
//  锚点取自 current_20261011 dump（运行时 so sha256 79adda77…）；RVA 首指令由
//  kExpect 校验，不符自动回落按名 ⇒ 热更漂移不会让功能失效，只会换解析路径。
// ════════════════════════════════════════════════════════════════════════════

// 身份：四项功能只需要「玩家 CountryData*」。这里**只走按名解析**、刻意不用
// 旧 LEB RVA（RVA_GET_COUNTRY_MGR_LEB=0x3A0379C 是 c04afff4… 基准，本机 so
// 79adda77… 已漂移且不在 kExpect 内 ⇒ 会挂到函数中段、把身份链挂错）。
// Hooked_GetCountryMgr 会把 id==pid 的结果存进 g_playerCountry。
// 另外 RefreshPlayerIdentityTick 还有一条不依赖钩子的路（Singleton.get_Instance
// → CaptureCountryMgr），两条路互为兜底。
static void InstallNewFeatureIdentity() {
    if (!g_origGetCountryById) {
        static const int kOneArgcs[] = {1};
        if (HookRvaOrIl2Cpp("GetCountryDataById", "", "CountryDataMgr", "GetCountryDataById",
                            kOneArgcs, 1, /*rva=*/0, (void*)Hooked_GetCountryMgr,
                            (void**)&g_origGetCountryById, false, 0)) {
            LOGI("[Whrhx] identity hooks: country(by-name)");
        }
    }
    static std::atomic<uint32_t> s_att{0};
    const uint32_t att = s_att.fetch_add(1);
    if (!g_origGetCountryById || (att % 20) != 0) return;
    void* mgr = g_countryMgr.load();
    const int32_t pid = g_playerCountryId.load();
    if (!mgr || pid <= 0 || pid > 64) return;
    void* c = g_origGetCountryById(mgr, pid, nullptr);
    if (!c || !ObjKlassNameIs(c, "CountryData")) return;
    int32_t cid = -1;
    if (KRead(static_cast<char*>(c) + g_offCountryId.load(), &cid, sizeof(cid)) && cid == pid)
        g_playerCountry.store(c);
}

// ① 政策重复CD：遍历 PolicyDataMgr.m_policyDic，把每条 PolicyData.m_repeatRemainCd 清 0。
//    （m_repeatRemainCd 即游戏「政策重复CD」——CountryReformMgr.GetPolicyRepeatRemainCd
//      的返回值就是 [PolicyData+0x18]，GM 面板也有官方「清除政策重复剩余CD」按钮。）
static int SweepPolicyCdInMgr(void* mgr, bool verbose) {
    if (!mgr) return 0;
    void* dict = nullptr;
    if (!KRead(static_cast<char*>(mgr) + kOffMgrPolicyDic, &dict, 8) || !dict) return 0;
    void* entries = nullptr;
    if (!KRead(static_cast<char*>(dict) + 0x18, &entries, 8) || !entries) return 0;
    int32_t maxLen = 0;
    if (!KRead(static_cast<char*>(entries) + 0x18, &maxLen, 4) || maxLen <= 0 || maxLen > 65536)
        return 0;
    char* base = static_cast<char*>(entries) + 0x20;
    const float zero = 0.f;
    int cleared = 0;
    for (int32_t i = 0; i < maxLen; i++) {
        char* ent = base + static_cast<size_t>(i) * 0x18;
        void* data = nullptr;
        if (!KRead(ent + 0x10, &data, 8) || !data) continue;
        if (!ObjKlassNameIs(data, "PolicyData")) continue;  // 布局假设兜底
        float cd = 0.f;
        if (!KRead(static_cast<char*>(data) + kOffPolicyRepeatCd, &cd, sizeof(cd))) continue;
        if (cd > 0.f && KWrite(static_cast<char*>(data) + kOffPolicyRepeatCd, &zero, sizeof(zero)))
            cleared++;
    }
    if (verbose && cleared > 0) LOGI("[Whrhx] policy noCD sweep cleared=%d", cleared);
    return cleared;
}

// ② 名臣好感度：遍历 MinisterPlayMgr.m_playDataDic，把每个 MinisterPlayData.Favorite 抬到 100。
static int PinMinisterFavorInMgr(void* mgr, bool verbose) {
    if (!mgr) return 0;
    void* dict = nullptr;
    if (!KRead(static_cast<char*>(mgr) + kOffMinisterPlayDic, &dict, 8) || !dict) return 0;
    void* entries = nullptr;
    if (!KRead(static_cast<char*>(dict) + 0x18, &entries, 8) || !entries) return 0;
    int32_t maxLen = 0;
    if (!KRead(static_cast<char*>(entries) + 0x18, &maxLen, 4) || maxLen <= 0 || maxLen > 65536)
        return 0;
    char* base = static_cast<char*>(entries) + 0x20;
    int n = 0;
    for (int32_t i = 0; i < maxLen; i++) {
        char* ent = base + static_cast<size_t>(i) * 0x18;
        void* play = nullptr;
        if (!KRead(ent + 0x10, &play, 8) || !play) continue;
        if (!ObjKlassNameIs(play, "MinisterPlayData")) continue;  // 身份校验，防错写
        int32_t fav = 0;
        if (!KRead(static_cast<char*>(play) + kOffMinisterFavorite, &fav, 4)) continue;
        if (fav >= kFavorFull) continue;
        if (KWrite(static_cast<char*>(play) + kOffMinisterFavorite, &kFavorFull, 4)) n++;
    }
    if (verbose && n > 0) LOGI("[Whrhx] minister favor pinned=%d", n);
    return n;
}

// ② 好感度读取口：面板/任务读取时直接给满值
static int32_t Hooked_MinisterGetFavor(void* thiz, int32_t ministerId, void* method) {
    int32_t v = g_origMinisterGetFavor ? g_origMinisterGetFavor(thiz, ministerId, method) : 0;
    if (!g_ministerFavor.load() || v >= kFavorFull) return v;
    LOGI("[Whrhx] minister favorite id=%d %d→%d", ministerId, v, kFavorFull);
    return kFavorFull;
}

// ② 好感度写入口：先抬到满值再走原生（原生是 += 语义，结果只会更大）
static void Hooked_MinisterAddFavor(void* thiz, int32_t favor, void* method) {
    if (g_ministerFavor.load() && thiz && ObjKlassNameIs(thiz, "MinisterPlayData")) {
        int32_t cur = 0;
        if (KRead(static_cast<char*>(thiz) + kOffMinisterFavorite, &cur, 4) && cur < kFavorFull) {
            if (KWrite(static_cast<char*>(thiz) + kOffMinisterFavorite, &kFavorFull, 4))
                LOGI("[Whrhx] minister add favor %d→%d (arg=%d)", cur, kFavorFull, favor);
        }
    }
    if (g_origMinisterAddFavor) g_origMinisterAddFavor(thiz, favor, method);
}

// ④ 规模加成：cap = 原生 × (100+pct)/100，只对玩家自己的 CountryArmyData 生效
//    （不做玩家判定会把 AI 一起加强）。pct<=1 视为关闭（滑块 1=关）。
static int64_t ScaleArmyCap(void* armyData, int64_t orig) {
    const int32_t pct = g_armyScalePct.load();
    if (pct <= 1 || orig <= 0 || !armyData) return orig;
    if (!ObjKlassNameIs(armyData, "CountryArmyData")) return orig;
    int32_t cid = -1;
    if (!KRead(static_cast<char*>(armyData) + kOffArmyCountryId, &cid, sizeof(cid))) return orig;
    const int32_t pid = g_playerCountryId.load();
    if (cid <= 0 || pid <= 0 || cid != pid) return orig;
    const int64_t scaled = orig * static_cast<int64_t>(100 + pct) / 100;
    return scaled > orig ? scaled : orig;
}

static int64_t Hooked_ArmyMaxCanSend(void* thiz, int32_t targetCity, bool isRobberBattle,
                                     int32_t roadPointId, bool isDefend, int64_t maxSendNum,
                                     bool canPassByLord, void* method) {
    int64_t r = g_origArmyMaxCanSend
                    ? g_origArmyMaxCanSend(thiz, targetCity, isRobberBattle, roadPointId, isDefend,
                                           maxSendNum, canPassByLord, method)
                    : 0;
    const int64_t s = ScaleArmyCap(thiz, r);
    if (s != r)
        LOGI("[Whrhx] army cap target=%d %lld→%lld (+%d%%)", targetCity, (long long)r,
             (long long)s, g_armyScalePct.load());
    return s;
}

static int64_t Hooked_ArmyMaxSend(void* thiz, int32_t battleType, int32_t intParam, void* method) {
    int64_t r = g_origArmyMaxSend ? g_origArmyMaxSend(thiz, battleType, intParam, method) : 0;
    const int64_t s = ScaleArmyCap(thiz, r);
    if (s != r)
        LOGI("[Whrhx] army maxSend type=%d %lld→%lld (+%d%%)", battleType, (long long)r,
             (long long)s, g_armyScalePct.load());
    return s;
}

// ② 名臣钩安装（GetMinisterFavorite 读口 + MinisterPlayData.AddFavor 写口）
static void InstallMinisterHooks() {
    if (g_hooksMinister.load()) return;
    static std::atomic<uint32_t> s_att{0};
    const uint32_t att = s_att.fetch_add(1);
    if (att > 0 && (att % 10) != 0) return;
    static const int kOneArgcs[] = {1};
    int n = 0;
    if (!g_origMinisterGetFavor) {
        if (HookRvaOrIl2Cpp("MinisterGetFavorite", "", "MinisterPlayMgr", "GetMinisterFavorite",
                            kOneArgcs, 1, RVA_MINISTER_GET_FAVOR,
                            (void*)Hooked_MinisterGetFavor, (void**)&g_origMinisterGetFavor, true,
                            0))
            ++n;
    }
    if (!g_origMinisterAddFavor) {
        // 嵌套类在 il2cpp 里的 klass 名是短名（MinisterPlayData），不是 dump 的 MinisterPlayMgr.MinisterPlayData
        if (HookRvaOrIl2Cpp("MinisterAddFavor", "", "MinisterPlayData", "AddFavor", kOneArgcs, 1,
                            RVA_MINISTER_ADD_FAVOR, (void*)Hooked_MinisterAddFavor,
                            (void**)&g_origMinisterAddFavor, true, 0))
            ++n;
    }
    if (n > 0)
        LOGI("[Whrhx] minister hooks=%d rva-read=%d rva-write=%d", n,
             g_origMinisterGetFavor ? 1 : 0, g_origMinisterAddFavor ? 1 : 0);
    if (g_origMinisterGetFavor && g_origMinisterAddFavor) {
        g_hooksMinister.store(true);
        LOGI("[Whrhx] minister hooks ready");
    }
}

// ④ 军队规模钩安装
static void InstallArmyScaleHooks() {
    if (g_hooksArmyScale.load()) return;
    static std::atomic<uint32_t> s_att{0};
    const uint32_t att = s_att.fetch_add(1);
    if (att > 0 && (att % 10) != 0) return;
    static const int kCanSendArgcs[] = {6};
    static const int kMaxSendArgcs[] = {2};
    int n = 0;
    if (!g_origArmyMaxCanSend) {
        if (HookRvaOrIl2Cpp("ArmyGetMaxCanSendNum", "", "CountryArmyData", "GetMaxCanSendNum",
                            kCanSendArgcs, 1, RVA_ARMY_GET_MAX_CAN_SEND,
                            (void*)Hooked_ArmyMaxCanSend, (void**)&g_origArmyMaxCanSend, true, 0))
            ++n;
    }
    if (!g_origArmyMaxSend) {
        if (HookRvaOrIl2Cpp("ArmyGetMaxSendNum", "", "CountryArmyData", "GetMaxSendNum",
                            kMaxSendArgcs, 1, RVA_ARMY_GET_MAX_SEND, (void*)Hooked_ArmyMaxSend,
                            (void**)&g_origArmyMaxSend, true, 0))
            ++n;
    }
    if (n > 0)
        LOGI("[Whrhx] army scale hooks=%d canSend=%d maxSend=%d", n,
             g_origArmyMaxCanSend ? 1 : 0, g_origArmyMaxSend ? 1 : 0);
    if (g_origArmyMaxCanSend && g_origArmyMaxSend) {
        g_hooksArmyScale.store(true);
        LOGI("[Whrhx] army scale hooks ready");
    }
}

// 新增四项的周期维护（1s 节流）：身份兜底 → ① 清政策重复CD → ② 抬名臣好感度
static void NewFeaturesTick() {
    const bool anyPolicy = g_policyNoCd.load();
    const bool anyFavor = g_ministerFavor.load();
    const bool anySkill = g_ministerSkillNoCd.load();
    const int32_t pct = g_armyScalePct.load();
    if (!anyPolicy && !anyFavor && !anySkill && pct <= 1) return;
    InstallNewFeatureIdentity();
    static std::atomic<int64_t> s_lastMs{0};
    const int64_t now = NowMs();
    if (now - s_lastMs.load() < 1000) return;
    s_lastMs.store(now);

    // ①（③ 的持续面靠执行口清 CD）：玩家 CountryData.m_policyMgr，拿不到就退回
    // Hooked_PolicyExecute 缓存的 PolicyDataMgr。
    if (anyPolicy) {
        void* mgr = nullptr;
        void* country = g_playerCountry.load();
        if (country)
            KRead(static_cast<char*>(country) + kOffCountryPolicyMgr, &mgr, sizeof(mgr));
        if (!mgr) mgr = g_policyMgr.load();
        if (mgr && ObjKlassNameIs(mgr, "PolicyDataMgr")) SweepPolicyCdInMgr(mgr, true);
    }
    // ② 好感度：玩家 CountryData.m_ministerData（MinisterPlayMgr）
    if (anyFavor) {
        void* country = g_playerCountry.load();
        void* mgr = nullptr;
        if (country)
            KRead(static_cast<char*>(country) + kOffCountryMinisterMgr, &mgr, sizeof(mgr));
        if (mgr && ObjKlassNameIs(mgr, "MinisterPlayMgr")) PinMinisterFavorInMgr(mgr, true);
    }
}

static void Whrhx_SyncHooks() {
    if (!g_libReady.load()) return;
    // ── 2026-10-11 新增四项（feat 10..13）──
    const bool newNoCd = g_policyNoCd.load() || g_ministerSkillNoCd.load();
    const bool anyNew = newNoCd || g_ministerFavor.load() || g_armyScalePct.load() > 1;
    // 身份先建：新增四项走「按名」建立身份链（旧 LEB RVA 在当前 so 已漂移，
    // 见 InstallNewFeatureIdentity 注释），先占位可顺带让生态路径跳过错误 RVA。
    if (anyNew) InstallNewFeatureIdentity();
#ifdef STARRY_GAME_WHRHXKB
    InstallCaptureHooks();
#endif
    // feat#9（科研立即完成）也要装 CheckRes/CostRes 钩：科研放行判定在 Hooked_CheckRes 内，
    // 之前只在 #1-#4 开启时才装，导致「只开 #9」时资源无条件放行不生效。
    if (g_gold.load() || g_food.load() || g_army.load() || g_pop.load() || g_researchDone.load())
        InstallEcoHooks();
    if (g_win.load()) {
        InstallBattleHooks();
        InstallSpeedHooks();  // Update 空页时用 delta 推帧强胜
    }
    if (g_speedLvl.load() > 1) InstallSpeedHooks();
    if (g_researchDone.load()) InstallResearchHooks();
    PinPlayerValues();

    // ① / ③ 复用科研那套 PolicyExecute 钩（GetPolicyData / FinishPolicyTimeAnyway / UpdatePolicyTime）
    if (newNoCd) InstallResearchHooks();
    if (g_ministerFavor.load()) InstallMinisterHooks();
    if (g_armyScalePct.load() > 1) InstallArmyScaleHooks();
    if (anyNew) NewFeaturesTick();
}

bool Whrhx_TryInit() {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;
    g_libReady.store(true);
#if defined(STARRY_GAME_WHRHX4399)
    LOGI("[Whrhx] lib ready target=m4399");
#endif
    // 导出已剥：GetApi 常失败，装钩不依赖它；仍尝试一次便于日后诊断
    if (GetApi().Load(GameUnity::TARGET_LIB))
        LOGI("[Whrhx] lib ready (dual: rva + il2cpp-name)");
    else
        LOGW("[Whrhx] lib ready (rva primary; il2cpp-name when api appears)");
    Whrhx_SyncHooks();
    return true;
}

bool Whrhx_IsReady() { return g_libReady.load(); }

static std::atomic<uint32_t> g_il2cppRetryTick{0};

void Whrhx_Tick(const bool* features) {
    if (features) {
        g_gold.store(features[WHRHX_FEAT_GOLD]);
        g_food.store(features[WHRHX_FEAT_FOOD]);
        g_army.store(features[WHRHX_FEAT_ARMY]);
        g_pop.store(features[WHRHX_FEAT_POP]);
        g_win.store(features[WHRHX_FEAT_WIN]);
        g_researchDone.store(features[WHRHX_FEAT_RESEARCH_DONE]);
        g_policyNoCd.store(features[WHRHX_FEAT_POLICY_NOCD]);
        g_ministerFavor.store(features[WHRHX_FEAT_MINISTER_FAVOR]);
        g_ministerSkillNoCd.store(features[WHRHX_FEAT_MINISTER_SKILL_NOCD]);
    }
    if (g_libReady.load() && !GetApi().ready && isLibraryLoaded(GameUnity::TARGET_LIB)) {
        const uint32_t t = g_il2cppRetryTick.fetch_add(1);
        if ((t % 120) == 0) GetApi().Load(GameUnity::TARGET_LIB);
    }
    Whrhx_SyncHooks();
#if defined(STARRY_TRACE)
    const uint32_t t = g_traceTick.fetch_add(1) + 1;
    if ((t % 360) == 0) LogTraceSummary("tick");
#endif
}

void Whrhx_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case WHRHX_FEAT_GOLD: g_gold.store(enabled); break;
        case WHRHX_FEAT_FOOD: g_food.store(enabled); break;
        case WHRHX_FEAT_ARMY: g_army.store(enabled); break;
        case WHRHX_FEAT_POP:  g_pop.store(enabled); break;
        case WHRHX_FEAT_WIN:  g_win.store(enabled); break;
        case WHRHX_FEAT_RESEARCH_DONE: g_researchDone.store(enabled); break;
        case WHRHX_FEAT_POLICY_NOCD: g_policyNoCd.store(enabled); break;
        case WHRHX_FEAT_MINISTER_FAVOR: g_ministerFavor.store(enabled); break;
        case WHRHX_FEAT_MINISTER_SKILL_NOCD: g_ministerSkillNoCd.store(enabled); break;
        case WHRHX_FEAT_SPEED:
            if (!enabled) {
                g_speedLvl.store(1);
                g_lastSpeedLvl.store(0);
            }
            break;
        default: break;
    }
    LOGI("[Whrhx] feature #%d=%s", featNum, enabled ? "ON" : "OFF");
    Whrhx_SyncHooks();
#if defined(STARRY_TRACE)
    LogTraceSummary("feat");
#endif
}

void Whrhx_OnValueChanged(int featNum, int value) {
    // ④ 军队规模：滑块 1..100，1=关；语义=规模加成 +X%
    if (featNum == WHRHX_FEAT_ARMY_SCALE) {
        if (value < 1) value = 1;
        if (value > 100) value = 100;
        if (value == g_armyScalePct.load()) return;
        g_armyScalePct.store(value);
        Whrhx_SyncHooks();
        LOGI("[Whrhx] armyScalePct=%d%% (cap=orig*%d/100)", value, 100 + value);
        return;
    }
    if (featNum != WHRHX_FEAT_SPEED) return;
    if (value < 1) value = 1;
    if (value > 5) value = 5;
    if (value == g_lastSpeedLvl.load()) return;
    g_lastSpeedLvl.store(value);
    g_speedLvl.store(value);
    Whrhx_SyncHooks();
    LOGI("[Whrhx] speedLvl=%d mul=%.1f", value, SpeedMul());
}

void Whrhx_ForceClear() {
    g_gold.store(false);
    g_food.store(false);
    g_army.store(false);
    g_pop.store(false);
    g_win.store(false);
    g_researchDone.store(false);
    g_policyNoCd.store(false);
    g_ministerFavor.store(false);
    g_ministerSkillNoCd.store(false);
    g_armyScalePct.store(0);
    t_ministerSkillDepth = 0;
    g_speedLvl.store(1);
    g_lastSpeedLvl.store(0);
    g_playerCountryId.store(-1);
    g_playerCountry.store(nullptr);
    g_playerMoneyFx.store(nullptr);
    g_playerFoodFx.store(nullptr);
    g_playerArmyFx.store(nullptr);
    g_countryMgr.store(nullptr);
    g_lastCostCountry.store(nullptr);
    g_policyMgr.store(nullptr);
#ifdef STARRY_GAME_WHRHXKB
    g_cityMgr.store(nullptr);
#endif
    g_battleActive.store(false);
    g_battleEnded.store(false);
    g_battleFrames.store(0);
}
