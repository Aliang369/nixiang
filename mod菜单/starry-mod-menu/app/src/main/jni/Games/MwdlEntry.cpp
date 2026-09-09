/* ================================================================
 *  MwdlEntry.cpp — 迷雾大陆 (com.hortor.mwdl) 注册表入口
 *
 *  Cocos2djs se::ScriptEngine::evalString 运行时拼接（方案 B）。
 *  功能位号与 MwdlMod.h enum 对齐（下标 = featNum）：
 *    1 百倍伤害 / 2 无敌 / 3 移动速度 / 4 无限技能
 * ================================================================
 */
#include "ModEntry.h"
#include "MwdlMod.h"

static void Mwdl_OnLoad(JavaVM* vm) { Mwdl_StartEarlyHookThread(vm); }

static void Mwdl_TickAdapter(const bool* features, const int* /*values*/) {
    Mwdl_Tick(features);
}

static const char* kMwdlFeatures[] = {
    "Category_战斗",              // 0
    "Toggle_百倍伤害",            // 1  MWDL_FEAT_DMG100
    "Toggle_无敌",                // 2  MWDL_FEAT_INVINCIBLE
    "Slider_移动速度:1:10",       // 3  MWDL_FEAT_MOVESPEED
    "Toggle_无限技能",            // 4  MWDL_FEAT_ULTRA_SKILL
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "mwdl",
        .authGameId = "mwdl",
        .featureList = kMwdlFeatures,
        .featureCount = (int)(sizeof(kMwdlFeatures) / sizeof(kMwdlFeatures[0])),

        .waitLib = "libcocos2djs.so",
        .initDelayMs = 0,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000,
        .tickIntervalMs = 200000,

        .onLoad = Mwdl_OnLoad,
        .tryInit = Mwdl_TryInit,
        .isReady = Mwdl_IsReady,
        .tick = Mwdl_TickAdapter,
        .onFeatureChanged = Mwdl_OnFeatureChanged,
        .onValueChanged = Mwdl_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Mwdl_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
