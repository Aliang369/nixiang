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
    "Toggle_广告奖励翻倍",           // 7  WXSJVC_FEAT_ADSREWARD
    "Toggle_物品翻倍",               // 8  WXSJVC_FEAT_ITEMDOUBLE
    "Toggle_无限挑战秘境",           // 9  WXSJVC_FEAT_MIJINGFREE
    "Toggle_皮肤全解锁",             // 10 WXSJVC_FEAT_SKINUNLOCK
    "Toggle_功能全解锁",             // 11 WXSJVC_FEAT_FUNCUNLOCK
    "Slider_经验倍率:1:10",          // 12 WXSJVC_FEAT_EXPMULTI
    "Toggle_商店广告无限",           // 13 WXSJVC_FEAT_SHOPADS
};

static bool WxsjVc1637_TryInitEntry(JavaVM* /*vm*/) {
    return WxsjVc1637_TryInit();
}

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "wxsj_vc1637",
        .authGameId = "wxsj",
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
        .onInputChanged = nullptr,
        .forceClear = WxsjVc1637_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
