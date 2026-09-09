/* ================================================================
 *  公主连结 (com.bilibili.priconne) 11.7.2
 *  对齐斌哥 11.4.0 抓点：变速 = Time.get_deltaTime × N
 *  属性修改：UnitData.get_Total*；敌我由 IsPlayerUnit 写入 side 表，未知不改
 *  敌我 2026-08-29：side 表条目带 GC 弱句柄，查表先验存活（防 GC 地址复用误判）
 *  属性 2026-08-29：InputValue_9999:修改属性（1–999999），经 nativeSetText 进 Pcr_OnInputChanged
 *  规范：docs/GAME_MOD_GUIDE.md（热路径无 SafeProbe）
 * ================================================================ */
#ifndef STARRY_PCR_MOD_H
#define STARRY_PCR_MOD_H

enum PcrFeat : int {
    PCR_FEAT_ATTR_ON  = 1,  // Toggle_开启属性修改
    PCR_FEAT_ATTR_VAL = 2,  // InputValue_9999:修改属性（1–999999）
    PCR_FEAT_SPEED    = 4,  // SeekBar_全局变速_1_10（1=原速）
};

bool Pcr_TryInit();
bool Pcr_IsReady();
void Pcr_OnFeatureChanged(int featNum, bool enabled);
void Pcr_OnValueChanged(int featNum, int value);
void Pcr_OnInputChanged(int featNum, const char* text);
void Pcr_Tick(const bool* features);
void Pcr_ForceClear();

#endif // STARRY_PCR_MOD_H
