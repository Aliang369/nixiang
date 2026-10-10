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
    WHRHX_FEAT_RESEARCH_DONE = 9,
    // 2026-10-11 新增四项（用户点名）
    WHRHX_FEAT_POLICY_NOCD         = 10,  // 政策技能无CD：执行即完成 + 重复CD 清零
    WHRHX_FEAT_MINISTER_FAVOR      = 11,  // 名臣好感度：拉到 100
    WHRHX_FEAT_MINISTER_SKILL_NOCD = 12,  // 名臣技能无CD：名臣面板政策执行即完成
    WHRHX_FEAT_ARMY_SCALE          = 13,  // 军队规模：滑块 1..100（1=关，语义=规模加成 +X%）
};

bool Whrhx_TryInit();
bool Whrhx_IsReady();
void Whrhx_Tick(const bool* features);
void Whrhx_OnFeatureChanged(int featNum, bool enabled);
void Whrhx_OnValueChanged(int featNum, int value);
void Whrhx_ForceClear();

#endif // STARRY_WHRHX_MOD_H
