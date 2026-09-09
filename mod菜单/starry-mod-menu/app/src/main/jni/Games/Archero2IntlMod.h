/* ================================================================
 *  弓箭传说2国际版 (com.facai.archeroii) — 1.0.1
 *  与国服 gjc2 独立 game_id；HybridCLR HotFix.BattleLogic
 * ================================================================ */
#ifndef STARRY_ARCHERO2_INTL_MOD_H
#define STARRY_ARCHERO2_INTL_MOD_H

enum Gjc2IntlFeat : int {
    GJC2INTL_FEAT_STEAL_SKILL = 1,
    GJC2INTL_FEAT_INVIS       = 2,
    GJC2INTL_FEAT_FULLSCREEN  = 3,
};

bool Gjc2Intl_TryInit();
bool Gjc2Intl_IsReady();
void Gjc2Intl_OnFeatureChanged(int featNum, bool enabled);
void Gjc2Intl_OnValueChanged(int featNum, int value);
void Gjc2Intl_Tick(const bool* features);
void Gjc2Intl_ForceClear();

#endif
