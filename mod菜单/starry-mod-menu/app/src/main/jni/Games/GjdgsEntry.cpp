/* ================================================================
 *  GjdgsEntry — 挂机打怪兽（org.cocos2d.guajidaguaishou.tap 1.1.65）菜单注册表
 *
 *  抓点来源：10_docs/_batch_binge6/三款新版菜单_抓点总表_20261103.md §四.2
 *            + 三款原版基线_与斌哥包差异_20261103.md §三.1（斌哥 JS 层明文全文）
 *  落地形态：assets/main.js 追加 JS 功能层（无 native hook），本文件只做菜单与开关通道。
 * ================================================================ */
#include "ModEntry.h"
#include "GjdgsMod.h"

static const char* kGjdgs_Features[] = {
    "Category_资源",              // 0
    "Toggle_金币满值",            // 1  GJDGS_FEAT_GOLD
    "Toggle_宝石满值",            // 2  GJDGS_FEAT_GEM
    "Toggle_碎片满值",            // 3  GJDGS_FEAT_SHARD
    "Category_战斗",              // 4
    "Slider_战斗加速:1:10",        // 5  GJDGS_FEAT_SPD   1=关
    "Toggle_无敌",                // 6  GJDGS_FEAT_GOD
    "Slider_百倍伤害:1:100",       // 7  GJDGS_FEAT_DMG   1=关
    "Toggle_免广告",              // 8  GJDGS_FEAT_NOAD
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Gjdgs",
        .authGameId = "gjdgs",
        .featureList = kGjdgs_Features,
        .featureCount = (int)(sizeof(kGjdgs_Features) / sizeof(kGjdgs_Features[0])),

        .waitLib = "libcocos2djs.so",
        .initDelayMs = 0,
        .initRetryMax = 600,
        .initRetryIntervalMs = 100000,
        .tickIntervalMs = 500000,

        .onLoad = nullptr,
        .tryInit = Gjdgs_TryInit,
        .isReady = Gjdgs_IsReady,
        .tick = Gjdgs_Tick,
        .onFeatureChanged = Gjdgs_OnFeatureChanged,
        .onValueChanged = Gjdgs_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Gjdgs_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
