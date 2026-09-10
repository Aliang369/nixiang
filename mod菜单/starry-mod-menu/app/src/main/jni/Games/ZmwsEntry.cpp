/* ================================================================
 *  ZmwsEntry.cpp — 造梦无双 (com.xm4399.zmws.m4399) 注册表入口
 *
 *  Cocos2djs se::ScriptEngine::evalString 运行时拼接。
 *  功能位号与 ZmwsMod.h enum 对齐（下标 = featNum）：
 *    1 无限技能 / 2 法宝技能 / 3 超级秒杀 / 4 超级无敌
 * ================================================================
 */
#include "ModEntry.h"
#include "ZmwsMod.h"

static void Zmws_OnLoad(JavaVM* vm) { Zmws_StartEarlyHookThread(vm); }

static void Zmws_TickAdapter(const bool* features, const int* /*values*/) {
    Zmws_Tick(features);
}

static const char* kZmwsFeatures[] = {
    "Category_战斗",        // 0
    "Toggle_无限技能",      // 1  ZMWS_FEAT_ULTRA_SKILL
    "Toggle_法宝技能",      // 2  ZMWS_FEAT_FABAO
    "Toggle_超级秒杀",      // 3  ZMWS_FEAT_SECKILL
    "Toggle_超级无敌",      // 4  ZMWS_FEAT_INVINCIBLE
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "zmws",
        .authGameId = "zmws",
        .featureList = kZmwsFeatures,
        .featureCount = (int)(sizeof(kZmwsFeatures) / sizeof(kZmwsFeatures[0])),

        .waitLib = "libcocos2djs.so",
        .initDelayMs = 0,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000,
        .tickIntervalMs = 200000,

        .onLoad = Zmws_OnLoad,
        .tryInit = Zmws_TryInit,
        .isReady = Zmws_IsReady,
        .tick = Zmws_TickAdapter,
        .onFeatureChanged = Zmws_OnFeatureChanged,
        .onValueChanged = nullptr,
        .onInputChanged = nullptr,
        .forceClear = Zmws_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
