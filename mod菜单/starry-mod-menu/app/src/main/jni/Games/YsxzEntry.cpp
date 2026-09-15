/* ================================================================
 *  勇士之心 (com.fightingstars.games.warrior)
 *
 *  Cocos Creator 2.4.15 / libcocos2djs.so。
 *  native 只 hook se::ScriptEngine::evalString，向游戏主 bundle 尾部拼接
 *  自愈式 JS；功能状态由 com.starry.magic.cocos.YsxzHooks 读取。
 * ================================================================ */

#include "ModEntry.h"
#include "YsxzMod.h"

static const char* kYsxz_Features[] = {
    "Category_基础功能",       // 0
    "Toggle_秒杀",             // 1  YSXZ_FEAT_KILL
    "Toggle_无敌",             // 2  YSXZ_FEAT_INV
    "Toggle_无限技能",         // 3  YSXZ_FEAT_SKILL
    "Slider_移动速度:1:5:1",     // 4  YSXZ_FEAT_MOVE（1=关，2..5=倍率）
    "Toggle_自动战斗",         // 5  YSXZ_FEAT_AUTO（金卡特权解锁）
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "ysxz",
        .authGameId = "yszzx",   // 勇士之心 -> yszzx
        .featureList = kYsxz_Features,
        .featureCount = (int)(sizeof(kYsxz_Features) / sizeof(kYsxz_Features[0])),

        .waitLib = "libcocos2djs.so",
        .initDelayMs = 0,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = Ysxz_OnLoad,
        .tryInit = Ysxz_TryInit,
        .isReady = Ysxz_IsReady,
        .tick = Ysxz_Tick,
        .onFeatureChanged = Ysxz_OnFeatureChanged,
        .onValueChanged = Ysxz_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Ysxz_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
