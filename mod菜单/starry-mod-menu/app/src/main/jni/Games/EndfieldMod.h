/* ================================================================
 *  明日方舟：终末地 (com.hypergryph.endfield)
 *  feat_num = Main.cpp g_featureList 下标（含 Category）
 *  抓点：10_docs/终末地/抓点_四功能_20260902.md
 * ================================================================ */
#ifndef STARRY_ENDFIELD_MOD_H
#define STARRY_ENDFIELD_MOD_H

enum EndfieldFeat : int {
    ENDFIELD_FEAT_DMG100 = 1,  // Toggle_百倍伤害
    ENDFIELD_FEAT_INV    = 2,  // Toggle_无敌
    ENDFIELD_FEAT_JUMP   = 3,  // Toggle_无限跳跃
    ENDFIELD_FEAT_SKILL  = 4,  // Toggle_无限技能
};

bool Endfield_TryInit();
bool Endfield_IsReady();
void Endfield_OnFeatureChanged(int featNum, bool enabled);
void Endfield_OnValueChanged(int featNum, int value);
void Endfield_Tick(const bool* features);
void Endfield_ForceClear();

#endif  // STARRY_ENDFIELD_MOD_H
