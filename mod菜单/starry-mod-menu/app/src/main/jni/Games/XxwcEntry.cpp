/* ================================================================
 *  XxwcEntry — 修仙王朝模拟器 (com.sky.xxwc.xxt 1.0.4 / vc5)
 *  引擎：Cocos Creator 3.8.7 / libcocos.so
 *  抓点：10_docs/_batch_binge6/六款斌哥改包_抓点总表_20261102.md §二.2（免广告）
 *        + 1.0.4 静态解密补全（战斗结算 WWarQingjiao）
 *  形态：native 零 hook，JS 层 monkey-patch（见 Games/XxwcPatch.js）
 *
 *  铁律：featureList 下标 = featNum（Category_ 占 0），与 XxwcMod.h 的 enum 对齐。
 *        MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 零修改。
 * ================================================================ */
#include "ModEntry.h"
#include "XxwcMod.h"

static const char* kXxwc_Features[] = {
    "Category_功能",              // 0
    "Toggle_免广告",              // 1  XXWC_FEAT_NOAD
    "Toggle_超级无敌(战斗必胜)",   // 2  XXWC_FEAT_INV
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "xxwc",
        .authGameId = "xxwc",
        .featureList = kXxwc_Features,
        .featureCount = (int)(sizeof(kXxwc_Features) / sizeof(kXxwc_Features[0])),

        .waitLib = "libcocos.so",
        .initDelayMs = 0,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 1000000,

        .onLoad = Xxwc_OnLoad,
        .tryInit = Xxwc_TryInit,
        .isReady = Xxwc_IsReady,
        .tick = Xxwc_Tick,
        .onFeatureChanged = Xxwc_OnFeatureChanged,
        .onValueChanged = Xxwc_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Xxwc_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
