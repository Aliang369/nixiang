/* ================================================================
 *  不一样修仙2 — 注册表入口
 *
 *  game_id 推导：全名「不一样修仙2」→ 不(b)一(y)样(y)修(x)仙(x)2 → byyxx2
 *  （数字原样保留；虚词计入。与后台 games.game_id / 菜单 template_id 必须同名）
 * ================================================================ */
#include "ModEntry.h"
#include "Byyxx2Mod.h"

static const char* kByyxx2_Features[] = {
    "Category_资源",              // 0  BYYXX2_FEAT_CAT_RES
    "InputValue_1000000:单次加成", // 1  BYYXX2_FEAT_AMOUNT
    "Button_金币",                // 2  BYYXX2_FEAT_MONEY
    "Button_声望",                // 3  BYYXX2_FEAT_RENOWN
    "Button_宗门贡献",            // 4  BYYXX2_FEAT_SECT
    "Button_修为",                // 5  BYYXX2_FEAT_EXP
    "Button_寿元",                // 6  BYYXX2_FEAT_LIFETIME
    "Button_一键全资源",          // 7  BYYXX2_FEAT_ALL
    "Category_自增",              // 8  BYYXX2_FEAT_CAT_INC
    "InputValue_1000:自增倍率",   // 9  BYYXX2_FEAT_INC_MULT
    "Toggle_金币自增",            // 10 BYYXX2_FEAT_INC_MONEY
    "Toggle_修为自增",            // 11 BYYXX2_FEAT_INC_EXP
    "Toggle_寿元自增",            // 12 BYYXX2_FEAT_INC_LIFE
    "Toggle_免广告",              // 13 BYYXX2_FEAT_NOAD
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Byyxx2",
        .authGameId = "byyxx2",
        .featureList = kByyxx2_Features,
        .featureCount = (int)(sizeof(kByyxx2_Features) / sizeof(kByyxx2_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 2000000,        // 微秒 = 2s（等 il2cpp 初始化完成）
        .initRetryMax = 120,
        .initRetryIntervalMs = 1000000,// 微秒 = 1s
        .tickIntervalMs = 200000,      // 微秒 = 200ms

        .onLoad = nullptr,
        .tryInit = Byyxx2_TryInit,
        .isReady = Byyxx2_IsReady,
        .tick = Byyxx2_Tick,
        .onFeatureChanged = Byyxx2_OnFeatureChanged,
        .onValueChanged = Byyxx2_OnValueChanged,
        .onInputChanged = Byyxx2_OnInputChanged,
        .forceClear = Byyxx2_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
