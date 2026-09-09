/* ================================================================
 *  霓虹深渊 (com.shangyoo.neon) — Unity IL2CPP 3.2.860 / vc104
 *  feat_num = Main.cpp g_featureList 下标（含 Category 占位）
 * ================================================================ */
#ifndef STARRY_NEON_MOD_H
#define STARRY_NEON_MOD_H

enum NeonFeat : int {
    NEON_FEAT_INVINCIBLE = 1,  // Toggle_无敌
    NEON_FEAT_SECKILL    = 2,  // Toggle_秒杀
    NEON_FEAT_NO_CD      = 3,  // Toggle_技能无CD
    NEON_FEAT_MOVE_SPEED = 4,  // Slider_移速:1:5
};

bool Neon_TryInit();
bool Neon_IsReady();
void Neon_OnFeatureChanged(int featNum, bool enabled);
void Neon_OnValueChanged(int featNum, int value);
void Neon_Tick(const bool* features);
void Neon_ForceClear();

#endif // STARRY_NEON_MOD_H
