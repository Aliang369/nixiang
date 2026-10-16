/* ================================================================
 *  ZmxyEntry — 造梦西游·黎尤浩劫 注册表入口（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  游戏：com.xm4399.zmxy6hj  1.1.0 (vc3)  4399 渠道
 *  引擎：Cocos Creator / libcocos2djs.so
 *  native 只 hook se::ScriptEngine::evalString；功能状态由
 *  com.starry.magic.cocos.ZmxyHooks 读取（app 内 classes7.dex）。
 *
 *  铁律：featureList 下标 = featNum，必须与 ZmxyMod.h 的 enum 对齐。
 * ================================================================ */
#include "ModEntry.h"
#include "ZmxyMod.h"

static void Zmxy_OnLoad(JavaVM* vm) { Zmxy_StartEarlyHookThread(vm); }

static const char* kZmxy_Features[] = {
    "Category_战斗",            // 0
    "Slider_伤害倍率:1:100:1",  // 1  ZMXY_FEAT_DMG_MUL（1 = 原版）
    "Toggle_无敌",              // 2  ZMXY_FEAT_INVINCIBLE
    "Toggle_无限技能",          // 3  ZMXY_FEAT_ULTRA_SKILL
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "zmxy",
        .authGameId = "zmxy",
        .featureList = kZmxy_Features,
        .featureCount = (int)(sizeof(kZmxy_Features) / sizeof(kZmxy_Features[0])),

        .waitLib = "libcocos2djs.so",
        .initDelayMs = 0,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = Zmxy_OnLoad,
        .tryInit = Zmxy_TryInit,
        .isReady = Zmxy_IsReady,
        .tick = Zmxy_Tick,
        .onFeatureChanged = Zmxy_OnFeatureChanged,
        .onValueChanged = Zmxy_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Zmxy_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
