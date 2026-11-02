/* ================================================================
 *  不一样西游 — 注册表入口
 *
 *  game_id 推导：全名「不一样西游」→ 不(b)一(y)样(y)西(x)游(y) → byyxy
 *  （与后台 games.game_id / 菜单 template_id 必须同名）
 *
 *  菜单 = 对照方（斌哥改包）1:1：愿 / 体 / 缘 / 毅 四项各加一次，
 *  另加「单次加成」数值框（对照方写死 99999）。
 * ================================================================ */
#include "ModEntry.h"
#include "BysxyMod.h"

static const char* kBysxy_Features[] = {
    "Category_属性",             // 0 BYSXY_FEAT_CAT_ATTR
    "InputValue_99999:单次加成",   // 1 BYSXY_FEAT_AMOUNT
    "Button_一键四维",           // 2 BYSXY_FEAT_ONEKEY  （对照方「斌哥」同款：四项各加一次）
    "Toggle_免广告",             // 3 BYSXY_FEAT_NOAD
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Bysxy",
        .authGameId = "byyxy",
        .featureList = kBysxy_Features,
        .featureCount = (int)(sizeof(kBysxy_Features) / sizeof(kBysxy_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 2000000,         // 微秒 = 2s
        .initRetryMax = 120,
        .initRetryIntervalMs = 1000000, // 微秒 = 1s
        .tickIntervalMs = 200000,       // 微秒 = 200ms

        .onLoad = nullptr,
        .tryInit = Bysxy_TryInit,
        .isReady = Bysxy_IsReady,
        .tick = Bysxy_Tick,
        .onFeatureChanged = Bysxy_OnFeatureChanged,
        .onValueChanged = Bysxy_OnValueChanged,
        .onInputChanged = Bysxy_OnInputChanged,
        .forceClear = Bysxy_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
