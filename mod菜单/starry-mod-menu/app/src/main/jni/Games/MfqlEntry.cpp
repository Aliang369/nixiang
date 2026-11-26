/* ================================================================
 *  MfqlEntry.cpp — 魔法奇录 (com.shw.mfqlm1) 注册表入口
 *
 *  Cocos Creator 2.x；本轮唯一功能「免广告」（featureList 下标 1）。
 *  waitLib = libcocos2djs.so：Cocos 系只需要它已映射，不做任何 hook。
 * ================================================================ */
#include "ModEntry.h"
#include "MfqlMod.h"

static const char* kMfql_Features[] = {
    "Category_广告",      // 0
    "Toggle_免广告",      // 1  MFQL_FEAT_FREE_AD
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Mfql",
        .authGameId = "mfql",
        .featureList = kMfql_Features,
        .featureCount = (int)(sizeof(kMfql_Features) / sizeof(kMfql_Features[0])),

        .waitLib = "libcocos2djs.so",
        .initDelayMs = 0,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,
        .tryInit = Mfql_TryInit,
        .isReady = Mfql_IsReady,
        .tick = Mfql_Tick,
        .onFeatureChanged = Mfql_OnFeatureChanged,
        .onValueChanged = Mfql_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Mfql_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
