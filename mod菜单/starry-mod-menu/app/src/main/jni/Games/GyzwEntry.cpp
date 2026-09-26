/* ================================================================
 *  GyzwEntry — 光阴之外 注册表入口（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  游戏：com.shbq.gyzw.ppyx  1.0.3 (vc103)  TapTap 渠道
 *  引擎：Cocos Creator 3.x / libcocos2djs.so
 *  native 只 hook se::ScriptEngine::evalString；功能状态由
 *  com.starry.magic.cocos.GyzwHooks 读取。
 *
 *  铁律：featureList 下标 = featNum，必须与 GyzwMod.h 的 enum 对齐。
 * ================================================================ */
#include "ModEntry.h"
#include "GyzwMod.h"

static const char* kGyzw_Features[] = {
    "Category_战斗",              // 0
    "Toggle_百倍伤害",            // 1  GYZW_FEAT_DMG
    "Toggle_超级无敌",            // 2  GYZW_FEAT_INV
    "Slider_战斗加速:1:10:1",     // 3  GYZW_FEAT_SPEED（1=关，2..10=倍率）
    "Category_广告",              // 4
    "Toggle_免广告",              // 5  GYZW_FEAT_NOAD
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Gyzw",
        .authGameId = "gyzw",           // 光阴之外 -> gyzw
        .featureList = kGyzw_Features,
        .featureCount = (int)(sizeof(kGyzw_Features) / sizeof(kGyzw_Features[0])),

        .waitLib = "libcocos2djs.so",
        .initDelayMs = 0,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = Gyzw_OnLoad,          // 早期 hook 线程（抢在 boot JS eval 之前）
        .tryInit = Gyzw_TryInit,
        .isReady = Gyzw_IsReady,
        .tick = Gyzw_Tick,
        .onFeatureChanged = Gyzw_OnFeatureChanged,
        .onValueChanged = Gyzw_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Gyzw_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
