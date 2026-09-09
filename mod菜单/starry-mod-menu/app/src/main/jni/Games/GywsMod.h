/* ================================================================
 *  孤勇无双 (com.fattoy.gyws.android) 1.0.3
 *  对齐斌哥：百倍伤害(×100) / 超级无敌 / 全屏攻击
 * ================================================================ */
#ifndef STARRY_GYWS_MOD_H
#define STARRY_GYWS_MOD_H

enum GywsFeat : int {
    GYWS_FEAT_SECKILL    = 1,  // Toggle_百倍伤害
    GYWS_FEAT_INVINCIBLE = 2,  // Toggle_超级无敌
    GYWS_FEAT_AOE        = 3,  // Toggle_全屏攻击
};

bool Gyws_TryInit();
bool Gyws_IsReady();
void Gyws_OnFeatureChanged(int featNum, bool enabled);
void Gyws_Tick(const bool* features);
void Gyws_ForceClear();

#endif // STARRY_GYWS_MOD_H
