/* ================================================================
 *  音舞光年 (com.uniugame.ywgn) — HybridCLR
 *  - Toggle「模拟真人」= BubbleConfig.isAutoPlay
 *  - 过检：Java SignSpoof（PackageInfo.CREATOR 回灌原版证书）
 * ================================================================ */
#ifndef STARRY_YWGN_MOD_H
#define STARRY_YWGN_MOD_H

enum YwgnFeat : int {
    YWGN_FEAT_SIM_HUMAN = 1,    // Toggle_模拟真人
    YWGN_FEAT_PCT_PPLUS = 3,    // Perfect+ → ScoreType 5
    YWGN_FEAT_PCT_PERFECT = 4,  // Perfect → ScoreType 4
    YWGN_FEAT_PCT_GREAT = 5,    // Great → ScoreType 3
    YWGN_FEAT_PCT_GOOD = 6,     // Good  → ScoreType 2
};

bool Ywgn_TryInit();
bool Ywgn_IsReady();
/** 过检脉冲：不依赖卡密会话，大厅也要跑 */
void Ywgn_SecurityTick();
void Ywgn_OnFeatureChanged(int featNum, bool enabled);
void Ywgn_OnValueChanged(int featNum, int value);
/** 从 Main.cpp g_values 同步四项权重（全 0 时用默认 85/13/1/1） */
void Ywgn_SyncPctSliders(const int* values);
void Ywgn_Tick(const bool* features);
void Ywgn_ForceClear();

#endif // STARRY_YWGN_MOD_H
