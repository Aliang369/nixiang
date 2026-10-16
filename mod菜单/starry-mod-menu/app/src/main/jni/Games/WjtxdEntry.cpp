/* ================================================================
 *  WjtxdEntry.cpp — 无尽探险队 (com.wjtxdtt.zy) 注册表入口
 *
 *  Cocos Creator 3.8.75 / libUnityV114514.so，JNI evalString splice 注入。
 *  功能位号与 WjtxdMod.h enum 对齐（下标 = featNum）：
 *    1 超级无敌 / 2 超级移速（对齐斌哥 get_ret 1..2）
 * ================================================================
 */
#include "ModEntry.h"
#include "WjtxdMod.h"

static void Wjtxd_OnLoad(JavaVM* vm) { Wjtxd_StartEarlyHookThread(vm); }

static void Wjtxd_TickAdapter(const bool* features, const int* values) {
    Wjtxd_Tick(features, values);
}

static const char* kWjtxdFeatures[] = {
    "Category_功能",        // 0
    "Toggle_超级无敌",      // 1  WJTXD_FEAT_INV
    "Toggle_超级移速",      // 2  WJTXD_FEAT_SPD
    "Toggle_免广告",        // 3  WJTXD_FEAT_ADFREE
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "wjtxd",
        .authGameId = "wjtxd",
        .featureList = kWjtxdFeatures,
        .featureCount = (int)(sizeof(kWjtxdFeatures) / sizeof(kWjtxdFeatures[0])),

        .waitLib = "libUnityV114514.so",
        .initDelayMs = 0,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = Wjtxd_OnLoad,
        .tryInit = Wjtxd_TryInit,
        .isReady = Wjtxd_IsReady,
        .tick = Wjtxd_TickAdapter,
        .onFeatureChanged = Wjtxd_OnFeatureChanged,
        .onValueChanged = nullptr,
        .onInputChanged = nullptr,
        .forceClear = Wjtxd_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}