/* ================================================================
 *  弹力果冻 (com.ginicat.sweet.m4399) 2.4.0 — HybridCLR hotfix
 *  从 _TemplateMod.h 复制；feat 与 Main.cpp g_featureList 下标对齐
 * ================================================================ */
#ifndef STARRY_TLGD_MOD_H
#define STARRY_TLGD_MOD_H

enum TlgdFeat : int {
    TLGD_FEAT_KILL     = 1,  // Toggle_秒杀
    TLGD_FEAT_INV      = 2,  // Toggle_无敌
    TLGD_FEAT_AOE_KILL = 3,  // Toggle_全屏秒杀
    TLGD_FEAT_NO_CD    = 4,  // Toggle_无限技能
};

bool Tlgd_TryInit();
bool Tlgd_IsReady();
void Tlgd_OnFeatureChanged(int featNum, bool enabled);
void Tlgd_OnValueChanged(int featNum, int value);
void Tlgd_Tick(const bool* features);
void Tlgd_ForceClear();

#endif
