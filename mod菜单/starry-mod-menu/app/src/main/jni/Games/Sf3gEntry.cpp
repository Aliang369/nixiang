/* ================================================================
 *  Sf3gEntry.cpp — 暗影格斗3 国际服（com.nekki.shadowfight3 / Google Play 1.46.1）
 *
 *  Unity IL2CPP / libil2cpp.so（arm64-v8a，split 内）
 *  菜单（数组下标 = featNum）：
 *    0  Category_功能
 *    1  Slider_伤害倍率:1:100:1  SF3G_FEAT_DMG
 *    2  Toggle_上帝模式          SF3G_FEAT_GOD
 *  战斗速度按用户 2026-11-06 要求不做。
 * ================================================================ */

#include "ModEntry.h"
#include "Sf3gMod.h"

static const char* kSf3g_Features[] = {
    "Category_功能",               // 0（占位）
    "Slider_伤害倍率:1:100:1",      // 1  SF3G_FEAT_DMG
    "Toggle_上帝模式",             // 2  SF3G_FEAT_GOD
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "sf3g",
        .authGameId = "aygd3",
        .featureList = kSf3g_Features,
        .featureCount = (int)(sizeof(kSf3g_Features) / sizeof(kSf3g_Features[0])),

        .waitLib = nullptr,   // 2026-11-06 探针：库名匹配不命中，直接进 init
        .initDelayMs = 1000000,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = Sf3g_OnLoad,
        .tryInit = Sf3g_TryInit,
        .isReady = Sf3g_IsReady,
        .tick = Sf3g_Tick,
        .onFeatureChanged = Sf3g_OnFeatureChanged,
        .onValueChanged = Sf3g_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Sf3g_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
