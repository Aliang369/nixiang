/* ================================================================
 *  消消英雄2 (com.blina.match2.tt) — xLua 战斗补丁
 *  抓点来源：改包 AN_1/AN_2/AN_3 → CFightSystem:DoAfterTaxDamage / FightBegan
 * ================================================================ */
#ifndef STARRY_MATCH2_MOD_H
#define STARRY_MATCH2_MOD_H

#include <cstddef>

// g_featureList 数组下标（Category_ 占位，逻辑勿用）
enum Match2Feat : int {
    MATCH2_FEAT_INVINCIBLE = 1,  // Toggle_无敌  → AN_2
    MATCH2_FEAT_ONESHOT    = 2,  // Toggle_十倍伤害 → AN_1 (*10)
    MATCH2_FEAT_FASTEND    = 3,  // Toggle_瞬间结束战斗 → AN_3
};

bool Match2_TryInit();
bool Match2_TryInitTraceEarly();
bool Match2_IsReady();
void Match2_OnFeatureChanged(int featNum, bool enabled);
void Match2_Tick(const bool* features);
void Match2_ForceClear();

#endif // STARRY_MATCH2_MOD_H
