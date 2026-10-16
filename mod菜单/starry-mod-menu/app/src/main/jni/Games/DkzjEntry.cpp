/* 蛋壳战机 — 星光注册表入口。 */
#include "ModEntry.h"
#include "DkzjMod.h"

static const char* kDkzj_Features[] = {
    "Category_功能",              // 0
    "Slider_攻击倍率:1:100:1",    // 1  DKZJ_FEAT_ATTACK_MULT
    "Toggle_上帝模式",            // 2  DKZJ_FEAT_GOD
    "Toggle_偷盗技能",            // 3  DKZJ_FEAT_STEAL_SKILL
    "Toggle_免广告",              // 4  DKZJ_FEAT_NO_ADS
    // 「立即胜利」已于 2026-10-14（用户 m00790）整体删除
    // 拦截上报：已按要求（m00392 第 2 条）取消菜单开关，改为常开注入，不占菜单项
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "dkzj",
        .authGameId = "dkzj",
        .featureList = kDkzj_Features,
        .featureCount = (int)(sizeof(kDkzj_Features) / sizeof(kDkzj_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 1500000,
        .initRetryMax = 120,
        .initRetryIntervalMs = 1000000,
        .tickIntervalMs = 15000,

        .onLoad = nullptr,
        .tryInit = Dkzj_TryInit,
        .isReady = Dkzj_IsReady,
        .tick = Dkzj_Tick,
        .onFeatureChanged = Dkzj_OnFeatureChanged,
        .onValueChanged = Dkzj_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Dkzj_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
