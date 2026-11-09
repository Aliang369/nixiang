/* ================================================================
 *  农场守护者 Entry — 注册表入口（MainModern.cpp 经 GameMod_Get 读取）
 * ================================================================ */
#include "ModEntry.h"
#include "NcshMod.h"

static const char* kNcsh_Features[] = {
    "Category_战斗",       // 0
    "Toggle_百倍伤害",      // 1
    "Toggle_无敌",          // 2
    "Toggle_无限技能",      // 3
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "NCSH",
        .authGameId = "ncsh",
        .featureList = kNcsh_Features,
        .featureCount = (int)(sizeof(kNcsh_Features) / sizeof(kNcsh_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 3000000,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,
        .tryInit = Ncsh_TryInit,
        .isReady = Ncsh_IsReady,
        .tick = Ncsh_Tick,
        .onFeatureChanged = Ncsh_OnFeatureChanged,
        .onValueChanged = Ncsh_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Ncsh_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
