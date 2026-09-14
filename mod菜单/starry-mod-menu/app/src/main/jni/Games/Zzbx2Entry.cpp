/* 正中靶心2 — modern 注册表入口 */
#include "ModEntry.h"
#include "Zzbx2Mod.h"

static void Zzbx2_OnLoad(JavaVM* vm) { Zzbx2_StartEarlyHookThread(vm); }

static const char* kZzbx2_Features[] = {
    "Category_功能",       // 0
    "Slider_攻击倍率:1:100:1", // 1 ZZBX2_FEAT_ATK（1=关，2..100=最终伤害倍率）
    "Toggle_无敌",         // 2 ZZBX2_FEAT_INV
    "Slider_战斗加速:1:5:1", // 3 ZZBX2_FEAT_SPD（1=关，2..5=倍率）
    "Toggle_免广告",       // 4 ZZBX2_FEAT_ADFREE
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "zzbx2",
        .authGameId = "zzbx2",
        .featureList = kZzbx2_Features,
        .featureCount = (int)(sizeof(kZzbx2_Features) / sizeof(kZzbx2_Features[0])),

        .waitLib = "libcocos2djs.so",
        .initDelayMs = 0,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000,
        .tickIntervalMs = 200000,

        .onLoad = Zzbx2_OnLoad,
        .tryInit = Zzbx2_TryInit,
        .isReady = Zzbx2_IsReady,
        .tick = Zzbx2_Tick,
        .onFeatureChanged = Zzbx2_OnFeatureChanged,
        .onValueChanged = Zzbx2_OnValueChanged,
        .onInputChanged = nullptr,      // 有 InputText/InputValue 才接
        .forceClear = Zzbx2_ForceClear,
        .onApplyPlayPatch = nullptr,    // 有 OTA 偏移热更才接
    };
    return &mod;
}
