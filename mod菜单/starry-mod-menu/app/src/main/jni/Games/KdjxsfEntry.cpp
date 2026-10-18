/* ================================================================
 *  KdjxsfEntry — 口袋觉醒 私服 注册表入口（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  职责：只填一张 StarryGameMod 表（菜单 / 初始化参数 / 回调接线）。
 *  铁律：MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 零修改。
 *  featureList 下标 = featNum，与 KdjxsfMod.h 的 enum 对齐。
 *
 *  game_id = kdjxsf（全名「口袋觉醒」→ kdjx；私服渠道 +sf，独立于任何国服 id）
 * ================================================================ */
#include "ModEntry.h"
#include "KdjxsfMod.h"

static const char* kKdjxsf_Features[] = {
    "Category_战斗",              // 0（占位）
    "Toggle_无敌",                // 1  KDJXSF_FEAT_INV
    "Slider_伤害倍率:1:100:1",     // 2  KDJXSF_FEAT_DMG（1 = 关；默认必须为 1）
    "Slider_战斗加速:1:5:1",       // 3  KDJXSF_FEAT_SPD（1 = 关）
    "Toggle_无限技能",             // 4  KDJXSF_FEAT_SKILL
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Kdjx",
        .authGameId = "kdjxsf",
        .featureList = kKdjxsf_Features,
        .featureCount = (int)(sizeof(kKdjxsf_Features) / sizeof(kKdjxsf_Features[0])),

        .waitLib = "libcocos2dlua.so",
        .initDelayMs = 2000000,          // 微秒：库命中后等 2s（避开 Lua VM 半初始化）
        .initRetryMax = 120,             // 120 × 2s ≈ 4min 预算
        .initRetryIntervalMs = 2000000,  // 微秒：2s
        .tickIntervalMs = 200000,        // 微秒：200ms

        .onLoad = nullptr,
        .tryInit = Kdjxsf_TryInit,
        .isReady = Kdjxsf_IsReady,
        .tick = Kdjxsf_Tick,
        .onFeatureChanged = Kdjxsf_OnFeatureChanged,
        .onValueChanged = Kdjxsf_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Kdjxsf_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
