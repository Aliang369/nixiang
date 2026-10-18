/* 蛋壳战机私服 — 星光注册表入口（复用 DkzjMod，仅 game_id/标签独立为 dkzjsf）。
 * 私服包名 com.wxdmxga.dkzj（国服 com.survivor.acecn = dkzj）。
 * libil2cpp 代码布局与国服 1.1.8 完全一致（见 10_docs/蛋壳战机/私服拆解/02_锚点一致性核验_20261018.md），
 * DkzjMod.cpp/h 原样复用，零偏移改动。 */
#include "ModEntry.h"
#include "DkzjMod.h"

static const char* kDkzjSf_Features[] = {
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
        .tag = "dkzjsf",
        .authGameId = "dkzjsf",
        .featureList = kDkzjSf_Features,
        .featureCount = (int)(sizeof(kDkzjSf_Features) / sizeof(kDkzjSf_Features[0])),

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
