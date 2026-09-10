/* ================================================================
 *  AwlzwEntry — 阿瓦隆之王 (King of Avalon 24.7.21) 注册表入口
 *
 *  2026-09-11 首次接入（斌哥包抓点：秒杀 + 无限技能，见
 *  10_docs/阿瓦隆之王/斌哥包抓点_20260911.md）。
 *  原版与斌哥包 libil2cpp md5 一致 → RVA 零重定位：
 *    0x8045BD0 DamageAttack(float) / 0x80927D8+0x8FEA408 EnergyRecovery(long)
 *  功能位号与 AwlzwMod.h enum 对齐：
 *    0 Category_战斗 / 1 秒杀 / 2 无限技能
 * ================================================================
 */
#include "ModEntry.h"
#include "AwlzwMod.h"

static const char* kAwlzwFeatures[] = {
    "Category_战斗",          // 0（占位）
    "Toggle_秒杀",            // 1  AWLZW_FEAT_SECKILL
    "Toggle_无限技能",        // 2  AWLZW_FEAT_INFSKILL
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "awlzw",
        .authGameId = "awlzw",      // 后台 games.game_id（阿瓦隆之王 → 拼音首字母）
        .featureList = kAwlzwFeatures,
        .featureCount = (int)(sizeof(kAwlzwFeatures) / sizeof(kAwlzwFeatures[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 2000,        // 等库后延迟，避开 il2cpp 半初始化窗口
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,
        .tryInit = Awlzw_TryInit,
        .isReady = Awlzw_IsReady,
        .tick = Awlzw_Tick,
        .onFeatureChanged = Awlzw_OnFeatureChanged,
        .onValueChanged = Awlzw_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Awlzw_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}