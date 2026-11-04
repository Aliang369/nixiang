/* ================================================================
 *  WxsjVc1637Entry — 无限升级 11.266018 / vc1637 注册表入口
 *
 *  独立于 12.x：菜单、鉴权 id、挂点、回调全部在这里声明。
 *  MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 均不改。
 * ================================================================ */
#include "ModEntry.h"
#include "WxsjVc1637Mod.h"

static const char* kWxsjVc1637_Features[] = {
    "Category_战斗",                 // 0
    "Toggle_无敌",                   // 1  WXSJVC_FEAT_GOD
    "Toggle_秒杀",                   // 2  WXSJVC_FEAT_ONESHOT
    "Category_功能",                 // 3
    "Toggle_免广告",                 // 4  WXSJVC_FEAT_FREEADS
    "Toggle_物品不减",               // 5  WXSJVC_FEAT_ITEMKEEP
    "Toggle_技能免费升级",           // 6  WXSJVC_FEAT_SKILLFREE
    // ===== 三个倍率：开关在**输入框上面**（2026-11-04 用户定：输入框只存倍数，开关控制生效）=====
    "Toggle_广告奖励倍数生效",       // 7  WXSJVC_FEAT_ADSREWARD_ON -> ads_reward_on
    "InputValue_1:广告奖励倍数",     // 8  WXSJVC_FEAT_ADSREWARD    -> ads_reward_x
    "Toggle_物品倍数生效",           // 9  WXSJVC_FEAT_ITEMDOUBLE_ON -> item_double_on
    "InputValue_1:物品倍数",         // 10 WXSJVC_FEAT_ITEMDOUBLE    -> item_double_x
    "Toggle_无限挑战秘境",           // 11 WXSJVC_FEAT_MIJINGFREE
    "Toggle_皮肤全解锁",             // 12 WXSJVC_FEAT_SKINUNLOCK
    "Toggle_功能全解锁",             // 13 WXSJVC_FEAT_FUNCUNLOCK
    "Toggle_经验倍数生效",           // 14 WXSJVC_FEAT_EXPMULTI_ON   -> exp_multi_on
    "InputValue_1:经验倍数",         // 15 WXSJVC_FEAT_EXPMULTI      -> exp_multi
    "Toggle_商店广告无限",           // 16 WXSJVC_FEAT_SHOPADS
    "Toggle_物品使用反增",           // 17 WXSJVC_FEAT_ITEMREVERSE（使用 N 个 → +N×倍率）
    "InputValue_1000:物品反增倍率",  // 18 WXSJVC_FEAT_ITEMREVERSEX
#ifdef WXSJ_ANTIBAN_MENU
    // 正常版：防封留在菜单（由用户开关控制 block_report）
    "Toggle_防封",                   // 19 WXSJVC_FEAT_ANTIBAN（默认关；开=上报/执法屏蔽生效）
    "Toggle_转生时间无限制",         // 20 WXSJVC_FEAT_ZHUANSHENG（默认关；开=取消转生 1 小时冷却）
#else
    // 屏蔽版：防封**不进菜单**（2026-10-26 用户定）：随 so 加载即生效
    "Toggle_转生时间无限制",         // 19 WXSJVC_FEAT_ZHUANSHENG（默认关；开=取消转生 1 小时冷却）
#endif
    // ===== 2026-11-04 新增三项（11 线口径与 12 线 wxsjdz2 逐字一致）=====
    "Slider_技能投掷物倍数:1:100",   // WXSJVC_FEAT_PROJECTILE（1=官方原样；进阶技能加成同样放大）
    "Slider_技能伤害倍数:1:1000",    // WXSJVC_FEAT_SKILLDMG（1=官方原样；反作弊基准同步乘）
    "Toggle_转生无条件升级",         // WXSJVC_FEAT_LEGACYFREE（开=轮回属性升级不扣轮回点）
};

static bool WxsjVc1637_TryInitEntry(JavaVM* /*vm*/) {
    return WxsjVc1637_TryInit();
}

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "wxsj_vc1637",
        .authGameId = "wxsjdz1",   // 2026-10-26 独立开号：11 线加固包与 12.x(wxsj) 分游戏，载荷 sha 互不影响
        .featureList = kWxsjVc1637_Features,
        .featureCount = (int)(sizeof(kWxsjVc1637_Features) / sizeof(kWxsjVc1637_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 0,
        .initRetryMax = 600,
        .initRetryIntervalMs = 100000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,
        .tryInit = WxsjVc1637_TryInitEntry,
        .isReady = WxsjVc1637_IsReady,
        .tick = nullptr,
        .onFeatureChanged = WxsjVc1637_OnFeatureChanged,
        .onValueChanged = WxsjVc1637_OnValueChanged,
        .onInputChanged = WxsjVc1637_OnInputChanged,
        .forceClear = WxsjVc1637_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
