/* ================================================================
 *  宝石战争 — 星光注册表入口
 * ================================================================ */
#include "BszzMod.h"
#include "ModEntry.h"

static const char* kBszz_Features[] = {
    "Category_战斗",            // 0
    "Slider_攻击倍率:1:100:1",  // 1  BSZZ_FEAT_ATTACK_MULT
    "Toggle_上帝模式",          // 2  BSZZ_FEAT_GOD_MODE
    "Toggle_无限技能",          // 3  BSZZ_FEAT_UNLIMITED_SKILL
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Bszz",
        .authGameId = "bszz",
        .featureList = kBszz_Features,
        .featureCount = (int)(sizeof(kBszz_Features) / sizeof(kBszz_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 0,
        .initRetryMax = 0,
        .initRetryIntervalMs = 1000000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,
        .tryInit = Bszz_TryInit,
        .isReady = Bszz_IsReady,
        .tick = Bszz_Tick,
        .onFeatureChanged = Bszz_OnFeatureChanged,
        .onValueChanged = Bszz_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Bszz_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
