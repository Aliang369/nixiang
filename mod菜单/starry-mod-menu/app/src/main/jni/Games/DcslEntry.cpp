/* ================================================================
 * DcslEntry - modern registry entry for 地城狩猎.
 *
 * No native target library: gameplay is LayaAir/Conch JavaScript.
 * StarryConch loads the runtime patch and this module only owns the
 * menu feature state.
 * ================================================================ */

#include "DcslMod.h"
#include "ModEntry.h"

static const char* kDcsl_Features[] = {
    "Category_基础功能",  // 0
    "Toggle_无限技能",    // 1  DCSL_FEAT_SKILL
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "dcsl",
        .authGameId = "dcsl",
        .featureList = kDcsl_Features,
        .featureCount = (int)(sizeof(kDcsl_Features) / sizeof(kDcsl_Features[0])),

        .waitLib = nullptr,
        .initDelayMs = 0,
        .initRetryMax = 3,
        .initRetryIntervalMs = 500000,
        .tickIntervalMs = 1000000,

        .onLoad = nullptr,
        .tryInit = Dcsl_TryInit,
        .isReady = Dcsl_IsReady,
        .tick = Dcsl_Tick,
        .onFeatureChanged = Dcsl_OnFeatureChanged,
        .onValueChanged = Dcsl_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Dcsl_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
