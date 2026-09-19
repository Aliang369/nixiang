/* ================================================================
 *  Sf3Entry.cpp — 暗影格斗3 (com.wanda.sf3.wd / TapTap 1.45.3) 注册表入口
 *
 *  Unity IL2CPP / libil2cpp.so
 *
 *  菜单（数组下标 = featNum）：
 *    0  Category_功能
 *    1  Slider_伤害倍率:1:10:1   SF3_FEAT_DMG
 *    2  Toggle_上帝模式          SF3_FEAT_GOD
 *    3  Slider_战斗速度:1:10:1   SF3_FEAT_SPEED
 *    4  Toggle_无限体力          SF3_FEAT_ENERGY
 *  （原 #5 Toggle_无限货币已于 2026-09-19 删除：实测假 UI 无效）
 *
 *  铁律：改菜单/初始化节奏/回调只改本文件；
 *        MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 永不需要动。
 * ================================================================ */

#include "ModEntry.h"
#include "Sf3Mod.h"

static const char* kSf3_Features[] = {
    "Category_功能",               // 0（占位）
    "Slider_伤害倍率:1:10:1",       // 1  SF3_FEAT_DMG
    "Toggle_上帝模式",             // 2  SF3_FEAT_GOD
    "Slider_战斗速度:1:10:1",       // 3  SF3_FEAT_SPEED
    "Toggle_无限体力",             // 4  SF3_FEAT_ENERGY
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "sf3",
        .authGameId = "sf3",            // 暗影格斗3
        .featureList = kSf3_Features,
        .featureCount = (int)(sizeof(kSf3_Features) / sizeof(kSf3_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 5000000,         // 5s：等 il2cpp 类表就绪（值单位=微秒）
        .initRetryMax = 120,            // 120 × 2s 启动期重试；失败后 Tick 懒装兜底
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,              // 无需 EarlyHook：全部功能都是懒装 Dobby hook
        .tryInit = Sf3_TryInit,
        .isReady = Sf3_IsReady,
        .tick = Sf3_Tick,
        .onFeatureChanged = Sf3_OnFeatureChanged,
        .onValueChanged = Sf3_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Sf3_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
