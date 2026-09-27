/* ================================================================
 *  我本布衣世家录Entry.cpp — 我本布衣：世家录 (com.JAGames.TalesoftheClan / TapTap 1.1.46) 注册表入口
 *
 *  功能位号与我本布衣世家录Mod.h enum 对齐（下标 = featNum，Category_ 占下标 0）：
 *    0 Category_功能 / 1 免广告 / 2 无限铜币 / 3 无限元宝 / 4 无限行动力 / 5 无限科研点 / 6 土地上限增加
 * ================================================================ */
#include "ModEntry.h"
#include "我本布衣世家录Mod.h"

static const char* kWbjyFeatures[] = {
    "Category_功能",          // 0（占位）
    "Toggle_免广告",          // 1  WBJY_FEAT_AD
    "Toggle_无限铜币",        // 2  WBJY_FEAT_COPPER
    "Toggle_无限元宝",        // 3  WBJY_FEAT_GOLD
    "Toggle_无限行动力",      // 4  WBJY_FEAT_AP
    "Toggle_无限科研点",      // 5  WBJY_FEAT_RESEARCH
    "Toggle_土地上限增加",    // 6  WBJY_FEAT_LAND
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "wbjy",
        .authGameId = "wbbysjl",
        .featureList = kWbjyFeatures,
        .featureCount = (int)(sizeof(kWbjyFeatures) / sizeof(kWbjyFeatures[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 3000000,        // 3s：避开 il2cpp 半初始化窗口
        .initRetryMax = 120,           // 启动期 120 × 2s；失败后 Tick 懒装兜底
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,      // 200ms

        .onLoad = nullptr,
        .tryInit = Wbjy_TryInit,
        .isReady = Wbjy_IsReady,
        .tick = Wbjy_Tick,
        .onFeatureChanged = Wbjy_OnFeatureChanged,
        .onValueChanged = Wbjy_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Wbjy_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
