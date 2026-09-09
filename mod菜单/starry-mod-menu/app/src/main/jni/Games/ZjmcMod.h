/* ================================================================
 *  追迹迷城 (com.TeaMelo.DungeonChasers) — 1.0.20260904 / vc97703
 *  feat_num = Main.cpp g_featureList 下标（含 Category 占位）
 * ================================================================ */
#ifndef STARRY_ZJMC_MOD_H
#define STARRY_ZJMC_MOD_H

enum ZjmcFeat : int {
    ZJMC_FEAT_SECKILL   = 1,  // Toggle_秒杀
    ZJMC_FEAT_INVINCIBLE = 2, // Toggle_无敌
    ZJMC_FEAT_AOE_KILL  = 3,  // Toggle_全屏秒杀
    ZJMC_FEAT_NO_CD     = 4,  // Toggle_无限技能
};

bool Zjmc_TryInit();
bool Zjmc_IsReady();
void Zjmc_OnFeatureChanged(int featNum, bool enabled);
void Zjmc_OnValueChanged(int featNum, int value);
void Zjmc_Tick(const bool* features);
void Zjmc_ForceClear();

#endif // STARRY_ZJMC_MOD_H
