/* ================================================================
 *  我嘎嘎乱杀 (com.sixag.gaga) v1.3.6 — Unity IL2CPP
 *  feat_num = Main.cpp g_featureList 下标（含 Category）
 *  定稿：10_docs/我嘎嘎乱杀/抓点定稿_20260825.md（1.3.6 重定位 20260902）
 * ================================================================ */
#ifndef STARRY_WGGLS_MOD_H
#define STARRY_WGGLS_MOD_H

enum WgglsFeat : int {
    WGGLS_FEAT_DMG100 = 1,  // Toggle_百倍伤害
    WGGLS_FEAT_INV    = 2,  // Toggle_超级无敌
    WGGLS_FEAT_SPEED  = 3,  // Slider_战斗加速:1:5  1=关 2..=倍速档
    WGGLS_FEAT_NOAD   = 4,  // Toggle_免广告（CanSkip / PurityMode）
};

bool Wggls_TryInit();
bool Wggls_IsReady();
void Wggls_OnFeatureChanged(int featNum, bool enabled);
void Wggls_OnValueChanged(int featNum, int value);
void Wggls_Tick(const bool* features);
void Wggls_ForceClear();

#endif // STARRY_WGGLS_MOD_H
