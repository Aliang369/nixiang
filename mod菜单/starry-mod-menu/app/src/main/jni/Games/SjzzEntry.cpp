/* ================================================================
 *  SjzzEntry — 我在神界刷装备 (com.windforce.rg.offical) 注册表入口
 *
 *  Cocos Creator 3 / libcocos.so；功能位与 SjzzMod.h enum 对齐。
 * ================================================================
 */
#include "ModEntry.h"
#include "SjzzMod.h"

static void Sjzz_OnLoad(JavaVM* vm) { Sjzz_StartEarlyHookThread(vm); }

static const char* kSjzz_Features[] = {
    "Category_功能",            // 0
    "Slider_伤害倍率:1:100:1",  // 1  SJZZ_FEAT_DMG
    "Toggle_无敌",              // 2  SJZZ_FEAT_INV
    "Toggle_免广告",            // 3  SJZZ_FEAT_ADFREE
    "Toggle_游戏加速",          // 4  SJZZ_FEAT_SPEED
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Sjzz",
        .authGameId = "wzsjszb",
        .featureList = kSjzz_Features,
        .featureCount = (int)(sizeof(kSjzz_Features) / sizeof(kSjzz_Features[0])),

        .waitLib = "libcocos.so",
        .initDelayMs = 0,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = Sjzz_OnLoad,
        .tryInit = Sjzz_TryInit,
        .isReady = Sjzz_IsReady,
        .tick = Sjzz_Tick,
        .onFeatureChanged = Sjzz_OnFeatureChanged,
        .onValueChanged = Sjzz_OnValueChanged,
        .onInputChanged = nullptr,      // 有 InputText/InputValue 才接
        .forceClear = Sjzz_ForceClear,
        .onApplyPlayPatch = nullptr,    // 有 OTA 偏移热更才接
    };
    return &mod;
}
