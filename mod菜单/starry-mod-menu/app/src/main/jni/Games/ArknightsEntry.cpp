/* ================================================================
 *  明日方舟 2.7.71 — modern 注册表入口
 *
 *  菜单 subscript = featNum。
 *  waitLib 固定 libil2cpp.so；目标版本 arm64-v8a / Unity IL2CPP。
 * ================================================================ */
#include "ModEntry.h"
#include "ArknightsMod.h"
#include "Includes/SecStrings.hpp"

static const char* kArknights_Features[] = {
    "Category_战斗",          // 0
    "Toggle_超级攻速",        // 1  ARKNIGHTS_FEAT_ATTACK_SPEED
    "Toggle_超级无敌",        // 2  ARKNIGHTS_FEAT_INVINCIBLE
    "Toggle_基地锁血",        // 3  ARKNIGHTS_FEAT_BASE_LOCK
    "Toggle_十倍伤害",        // 4  ARKNIGHTS_FEAT_DAMAGE10
    "Toggle_全图放置",        // 5  ARKNIGHTS_FEAT_DEPLOY_ANYWHERE
    "Toggle_无限技能",        // 6  ARKNIGHTS_FEAT_INFINITE_SKILL
    "Toggle_零部署费用",      // 7  ARKNIGHTS_FEAT_ZERO_COST
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "mrfzz",
        .authGameId = "mrfzz",
        .featureList = kArknights_Features,
        .featureCount = (int)(sizeof(kArknights_Features) / sizeof(kArknights_Features[0])),

        .waitLib = sec_lib_il2cpp(),
        .initDelayMs = 3000,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000,
        .tickIntervalMs = 200,

        .onLoad = nullptr,
        .tryInit = Arknights_TryInit,
        .isReady = Arknights_IsReady,
        .tick = Arknights_Tick,
        .onFeatureChanged = Arknights_OnFeatureChanged,
        .onValueChanged = Arknights_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Arknights_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
