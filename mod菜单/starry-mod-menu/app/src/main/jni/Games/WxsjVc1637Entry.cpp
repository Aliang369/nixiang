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
    "InputValue_1:广告奖励倍数",     // 7  WXSJVC_FEAT_ADSREWARD（输入框，写 ads_reward_x，默认 1=不放大）
    "InputValue_1:物品倍数",         // 8  WXSJVC_FEAT_ITEMDOUBLE（输入框，写 item_double_x，默认 1=不放大）
    "Toggle_无限挑战秘境",           // 9  WXSJVC_FEAT_MIJINGFREE
    "Toggle_皮肤全解锁",             // 10 WXSJVC_FEAT_SKINUNLOCK
    "Toggle_功能全解锁",             // 11 WXSJVC_FEAT_FUNCUNLOCK
    "InputValue_1:经验倍数",         // 12 WXSJVC_FEAT_EXPMULTI（输入框，写 exp_multi，默认 1=不放大）
    "Toggle_商店广告无限",           // 13 WXSJVC_FEAT_SHOPADS
    "Toggle_物品使用反增",           // 14 WXSJVC_FEAT_ITEMREVERSE（使用 N 个 → +N×倍率）
    "InputValue_1000:物品反增倍率",  // 15 WXSJVC_FEAT_ITEMREVERSEX（输入框，写 item_reverse_x）
    "Toggle_转生时间无限制",         // 16 WXSJVC_FEAT_ZHUANSHENG（默认关；开=取消转生 1 小时冷却）
    // 注：防封**不进菜单**（2026-10-26 用户定）：随 so 一起加载、无条件生效，见 WxsjVc1637Mod.cpp 防封基线段。
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
