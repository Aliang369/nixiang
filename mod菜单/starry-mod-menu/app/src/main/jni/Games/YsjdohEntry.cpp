/* ================================================================
 *  YsjdohEntry — 异世界当欧皇（com.cxgame.ysjdoh 1.0.104）菜单注册表
 *
 *  抓点来源：10_docs/_batch_binge6/六款斌哥改包_抓点总表_20261102.md §二.1
 *    - Enemy.prototype.Hurt  -> 伤害倍率（斌哥 ×100）
 *    - Hero.prototype.Hurt   -> 超级无敌（斌哥强制 Invincible 分支）
 *    - FightSimulator.SimulationSpeed -> 战斗加速（斌哥 5×）
 *  落地形态：assets/assets/scripts/index.jsc 最小补丁（见 04_scripts/异世界当欧皇/
 *  ysjdoh_patch_js.py），native 侧零 hook，只做开关通道。
 * ================================================================ */
#include "ModEntry.h"
#include "YsjdohMod.h"

static const char* kYsjdoh_Features[] = {
    "Category_战斗",           // 0
    "Slider_伤害倍率:1:100:1",  // 1  YSJDOH_FEAT_MULT（1=关闭，2..100=倍率）
    "Toggle_超级无敌",          // 2  YSJDOH_FEAT_INV
    "Toggle_战斗加速",          // 3  YSJDOH_FEAT_SPD
    "Toggle_免广告",            // 4  YSJDOH_FEAT_NOAD
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "ysjdoh",
        .authGameId = "ysjdhoh",
        .featureList = kYsjdoh_Features,
        .featureCount = (int)(sizeof(kYsjdoh_Features) / sizeof(kYsjdoh_Features[0])),

        .waitLib = "libcocos2djs.so",
        .initDelayMs = 0,
        .initRetryMax = 600,
        .initRetryIntervalMs = 100000,
        .tickIntervalMs = 500000,

        .onLoad = nullptr,
        .tryInit = Ysjdoh_TryInit,
        .isReady = Ysjdoh_IsReady,
        .tick = Ysjdoh_Tick,
        .onFeatureChanged = Ysjdoh_OnFeatureChanged,
        .onValueChanged = Ysjdoh_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Ysjdoh_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
