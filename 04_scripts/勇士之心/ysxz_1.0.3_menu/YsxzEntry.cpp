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
    "Slider_伤害倍率:1:50:1",    // 1  YSXZ_FEAT_DAMAGE（1=原倍率，2..50=我方对敌伤害乘数）
    "Toggle_秒杀",             // 2  YSXZ_FEAT_KILL（与伤害倍率换位：滑块置顶）
    "Toggle_无敌",             // 3  YSXZ_FEAT_INV
    "Toggle_无限技能",         // 4  YSXZ_FEAT_SKILL
    "Slider_移动速度:1:10:1",    // 5  YSXZ_FEAT_MOVE（1=关，2..10=倍率）
    "Toggle_自动战斗",         // 6  YSXZ_FEAT_AUTO（金卡特权解锁）
    "Category_新增功能",       // 7  分组标题（占位）
    "Toggle_解锁金卡",         // 8  YSXZ_FEAT_GOLD（$eZ.monCard.fDn 原型 fDc 放行 gold）
    "Category_自动化",         // 9  分组标题（占位）
    "Toggle_自动重复挑战",     // 10 YSXZ_FEAT_REPEAT
    "Toggle_自动出售",         // 11 YSXZ_FEAT_SELL
    "Toggle_自动分解",         // 12 YSXZ_FEAT_DECO
    "Toggle_自动穿戴",         // 13 YSXZ_FEAT_WEAR
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
