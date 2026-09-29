/* ================================================================
 *  长生：天机降世 (com.netkuu.changsheng.tjjs) — 1.0.2 / vc1020
 *  feat_num = Main.cpp g_featureList 下标（含 Category 占位）
 * ================================================================ */
#ifndef STARRY_CSTJJS_MOD_H
#define STARRY_CSTJJS_MOD_H

enum CstjjsFeat : int {
    CSTJJS_FEAT_DMG10   = 1,  // Toggle_十倍伤害
    CSTJJS_FEAT_INVULN  = 2,  // Toggle_无敌
};

bool Cstjjs_TryInit();
bool Cstjjs_IsReady();
void Cstjjs_OnFeatureChanged(int featNum, bool enabled);
void Cstjjs_OnValueChanged(int featNum, int value);
void Cstjjs_Tick(const bool* features);
void Cstjjs_ForceClear();

#endif // STARRY_CSTJJS_MOD_H
