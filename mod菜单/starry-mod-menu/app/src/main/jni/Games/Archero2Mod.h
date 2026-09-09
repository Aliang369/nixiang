/* ================================================================
 *  弓箭传说2 (com.xq.archeroii.ua) — 1.1.8
 *  HybridCLR HotFix.BattleLogic：偷技能 / 玩家隐身 / 全屏攻击
 * ================================================================ */
#ifndef STARRY_ARCHERO2_MOD_H
#define STARRY_ARCHERO2_MOD_H

enum Gjc2Feat : int {
    GJC2_FEAT_STEAL_SKILL = 1,  // Toggle_拿全部技能
    GJC2_FEAT_INVIS       = 2,  // Toggle_玩家隐身
    GJC2_FEAT_FULLSCREEN  = 3,  // Toggle_全屏攻击
};

bool Gjc2_TryInit();
bool Gjc2_IsReady();
void Gjc2_OnFeatureChanged(int featNum, bool enabled);
void Gjc2_OnValueChanged(int featNum, int value);
void Gjc2_Tick(const bool* features);
void Gjc2_ForceClear();

#endif
