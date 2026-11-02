/* 不一样的修仙宗门2 — modern 注册表入口（Cocos Creator 2.x） */
#include "ModEntry.h"
#include "Byydxxzm2Mod.h"

static const char* kByydxxzm2_Features[] = {
    "Category_功能",        // 0
    "Toggle_无限仙玉",      // 1 BYYDXXZM2_FEAT_YU
    "Toggle_无限灵石",      // 2 BYYDXXZM2_FEAT_LING
    "Toggle_免广告",        // 3 BYYDXXZM2_FEAT_NOAD
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "byydxxzm2",
        .authGameId = "byydxxzm2",
        .featureList = kByydxxzm2_Features,
        .featureCount = (int)(sizeof(kByydxxzm2_Features) / sizeof(kByydxxzm2_Features[0])),

        .waitLib = "libcocos2djs.so",
        .initDelayMs = 0,
        .initRetryMax = 600,
        .initRetryIntervalMs = 100,
        .tickIntervalMs = 500,

        .onLoad = nullptr,          // 不装 native hook（houdini 转译下 inline hook 必崩）
        .tryInit = Byydxxzm2_TryInit,
        .isReady = Byydxxzm2_IsReady,
        .tick = Byydxxzm2_Tick,
        .onFeatureChanged = Byydxxzm2_OnFeatureChanged,
        .onValueChanged = Byydxxzm2_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Byydxxzm2_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
