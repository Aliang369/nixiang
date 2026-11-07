/* ================================================================
 *  MsgcEntry.cpp — 墨守孤城 (com.chillyroom.inkvasion.yw 1.3.0/vc162) 注册表入口
 *  0 Category_局内 / 1 建造资源无限 / 2 无建造时间 / 3 建筑无限生命 / 4 部队无限生命
 * ================================================================ */
#include "ModEntry.h"
#include "MsgcMod.h"

static void Msgc_TickAdapter(const bool* features, const int* /*values*/) {
    Msgc_Tick(features, nullptr);
}

static bool Msgc_TryInitAdapter(JavaVM* /*vm*/) {
    return Msgc_TryInit(nullptr);
}

static const char* kMsgcFeatures[] = {
    "Category_局内",          // 0（占位）
    "Toggle_建造资源无限",    // 1  MSGC_FEAT_INF_RES
    "Toggle_无建造时间",      // 2  MSGC_FEAT_NO_BUILD_TIME
    "Toggle_建筑无限生命",    // 3  MSGC_FEAT_BUILD_GOD
    "Toggle_部队无限生命",    // 4  MSGC_FEAT_TROOP_GOD
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "msgc",
        .authGameId = "msgc",
        .featureList = kMsgcFeatures,
        .featureCount = (int)(sizeof(kMsgcFeatures) / sizeof(kMsgcFeatures[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 3000000,
        .initRetryMax = 180,
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,
        .tryInit = Msgc_TryInitAdapter,
        .isReady = Msgc_IsReady,
        .tick = Msgc_TickAdapter,
        .onFeatureChanged = Msgc_OnFeatureChanged,
        .onValueChanged = Msgc_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Msgc_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
