/* ================================================================
 *  HxwjEntry.cpp — 幻想武将模拟器 (com.qxjc.hxwjmlq / TapTap 1.0.8) 注册表入口
 *
 *  功能位号与 HxwjMod.h enum 对齐（下标 = featNum，Category_ 占下标 0）：
 *    0 Category_功能 / 1 秒杀 / 2 百分百掉落 / 3 免广告
 * ================================================================ */
#include "ModEntry.h"
#include "HxwjMod.h"

static const char* kHxwjFeatures[] = {
    "Category_功能",       // 0（占位）
    "Toggle_秒杀",         // 1  HXWJ_FEAT_DMG
    "Toggle_百分百掉落",   // 2  HXWJ_FEAT_DROP
    "Toggle_免广告",       // 3  HXWJ_FEAT_AD
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "hxwj",
        .authGameId = "hxwjmlq",
        .featureList = kHxwjFeatures,
        .featureCount = (int)(sizeof(kHxwjFeatures) / sizeof(kHxwjFeatures[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 3000000,        // 3s：避开 il2cpp 半初始化窗口
        .initRetryMax = 120,           // 启动期 120 × 2s；失败后 Tick 懒装兜底
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,      // 200ms

        .onLoad = nullptr,
        .tryInit = Hxwj_TryInit,
        .isReady = Hxwj_IsReady,
        .tick = Hxwj_Tick,
        .onFeatureChanged = Hxwj_OnFeatureChanged,
        .onValueChanged = Hxwj_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Hxwj_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
