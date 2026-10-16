/* ================================================================
 *  RsjyEntry.cpp — 燃烧纪元 (com.rsjy.yf) 注册表入口
 *
 *  Cocos2d-x JS / libcocos2djs.so
 *  se::ScriptEngine::evalString 运行时追加星光 JS 补丁。
 *
 *  菜单（数组下标 = featNum）：
 *    0  Category_战斗
 *    1  Slider_攻击倍率:1:100:1
 *    2  Toggle_上帝模式
 *    3  Slider_战斗速度:1:10:1
 *    4  Toggle_免广告
 * ================================================================ */

#include "ModEntry.h"
#include "RsjyMod.h"

static void Rsjy_OnLoad(JavaVM* vm) { Rsjy_StartEarlyHookThread(vm); }

static const char* kRsjyFeatures[] = {
    "Category_战斗",                   // 0
    "Slider_攻击倍率:1:100:1",          // 1  RSJY_FEAT_KILL
    "Toggle_上帝模式",                 // 2  RSJY_FEAT_GOD
    "Slider_战斗速度:1:10:1",           // 3  RSJY_FEAT_SPEED
    "Toggle_免广告",                   // 4  RSJY_FEAT_ADFREE
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "rsjy",
        .authGameId = "rsjy",
        .featureList = kRsjyFeatures,
        .featureCount = (int)(sizeof(kRsjyFeatures) / sizeof(kRsjyFeatures[0])),

        .waitLib = "libcocos2djs.so",
        .initDelayMs = 0,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = Rsjy_OnLoad,
        .tryInit = Rsjy_TryInit,
        .isReady = Rsjy_IsReady,
        .tick = Rsjy_Tick,
        .onFeatureChanged = Rsjy_OnFeatureChanged,
        .onValueChanged = Rsjy_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Rsjy_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
