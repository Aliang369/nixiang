/* ================================================================
 *  milthm (game.taptap.morizero.milthm) — Unity IL2CPP 6.0.5
 *  对齐斌哥：单 Toggle「自动跳舞」= prefs "0" + 4× Dobby
 * ================================================================ */
#ifndef STARRY_MILTHM_MOD_H
#define STARRY_MILTHM_MOD_H

enum MilthmFeat : int {
    MILTHM_FEAT_AUTO = 1,  // Toggle_自动跳舞
};

bool Milthm_TryInit();
bool Milthm_IsReady();
void Milthm_OnFeatureChanged(int featNum, bool enabled);
void Milthm_Tick(const bool* features);
void Milthm_ForceClear();

#endif // STARRY_MILTHM_MOD_H
