/* 动物大联盟 — modern 注册表入口 */
#include "ModEntry.h"
#include "DwdlmMod.h"

static void Dwdlm_OnLoad(JavaVM* vm) {
#ifndef DWDLM_SHELL_ONLY
    Dwdlm_InstallPathProbe();
#endif
    Dwdlm_PrepareSecureShell();
    Dwdlm_PrepareSecureShell();
#ifndef DWDLM_DATA_GUARD_ONLY
    Dwdlm_StartEarlyHookThread(vm);
#endif
}

static const char* kDwdlm_Features[] = {
    "Category_功能",
    "Slider_攻击倍率:1:100:1",
    "Toggle_上帝模式",
    "Toggle_无限技能",
    "Toggle_自动战斗",
    "Slider_移速:1:5:1",
    "Toggle_免广告",
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "dwdlm",
        .authGameId = "dwdlm",
        .featureList = kDwdlm_Features,
        .featureCount = (int)(sizeof(kDwdlm_Features) / sizeof(kDwdlm_Features[0])),

        .waitLib = "libcocos2djs.so",
        .initDelayMs = 0,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = Dwdlm_OnLoad,
        .tryInit = Dwdlm_TryInit,
        .isReady = Dwdlm_IsReady,
        .tick = Dwdlm_Tick,
        .onFeatureChanged = Dwdlm_OnFeatureChanged,
        .onValueChanged = Dwdlm_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Dwdlm_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
