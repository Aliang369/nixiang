/* ================================================================
 *  无悔华夏 3.8.280 — Unity IL2CPP
 *  九游 com.yhhy.whrhx.aligames (STARRY_GAME_WHRHX,  game_id=whhx)
 *  快爆 com.syyx.whrhx          (STARRY_GAME_WHRHXKB, game_id=whhxkb)
 * ================================================================ */
#ifndef STARRY_WHRHX_MOD_H
#define STARRY_WHRHX_MOD_H

enum WhrhxFeat : int {
    WHRHX_FEAT_GOLD  = 1,
    WHRHX_FEAT_FOOD  = 2,
    WHRHX_FEAT_ARMY  = 3,
    WHRHX_FEAT_POP   = 4,
    WHRHX_FEAT_WIN   = 6,
    WHRHX_FEAT_SPEED = 8,
};

bool Whrhx_TryInit();
bool Whrhx_IsReady();
void Whrhx_Tick(const bool* features);
void Whrhx_OnFeatureChanged(int featNum, bool enabled);
void Whrhx_OnValueChanged(int featNum, int value);
void Whrhx_ForceClear();

#endif // STARRY_WHRHX_MOD_H
