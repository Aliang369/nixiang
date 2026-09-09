/* ================================================================
 *  独奏骑士 (com.Shimmer.SoloKnight.cn) 1.1.760 — HybridCLR
 *  从 _TemplateMod.h 复制；feat 与 Main.cpp g_featureList 下标对齐
 * ================================================================ */
#ifndef STARRY_DZQS_MOD_H
#define STARRY_DZQS_MOD_H

enum DzqsFeat : int {
    DZQS_FEAT_KILL  = 1,  // Toggle_百倍伤害
    DZQS_FEAT_INV   = 2,  // Toggle_无敌
    DZQS_FEAT_NO_CD = 3,  // Toggle_无限技能
};

bool Dzqs_TryInit();
bool Dzqs_IsReady();
void Dzqs_OnFeatureChanged(int featNum, bool enabled);
void Dzqs_OnValueChanged(int featNum, int value);
void Dzqs_Tick(const bool* features);
void Dzqs_ForceClear();

#endif
