/* ================================================================
 *  BlfxMod.h — 爆裂防线（com.muyegame.blfx）1.01.06
 *
 *  斌哥改包复刻 → 星空版。功能位号 = BlfxEntry.cpp 的 featureList 下标。
 *
 *  目标功能（用户 2026-09-26 指令）：
 *    1 伤害倍率（SeekBar 1..1000，1 = 关）
 *    2 无敌（Toggle）
 *    3 战斗加速（SeekBar 1..10，1 = 关）
 *    免广告：不做（游戏无任何可达的看广告得奖励入口，已实证）
 *
 *  ⚠️ 本作 HP 是 zbSafeLong 双影子防作弊结构（_obfValue^_key 必须 ==
 *     _shadowObf^_shadowKey，否则 zbCheatContext.ReportCheat 上报），
 *     禁止裸写 curHp/maxHp 单个副本。
 * ================================================================ */
#ifndef STARRY_BLFX_MOD_H
#define STARRY_BLFX_MOD_H

#include <jni.h>

enum BlfxFeat : int {
    BLFX_FEAT_CAT   = 0,  // Category_战斗（占位，仅供 UI 分组）
    BLFX_FEAT_DMG   = 1,  // SeekBar_伤害倍率_1_1000_1
    BLFX_FEAT_INV   = 2,  // Toggle_无敌
    BLFX_FEAT_SPEED = 3,  // SeekBar_战斗加速_1_10_1
};

bool Blfx_TryInit(JavaVM* vm);
bool Blfx_IsReady();
void Blfx_OnFeatureChanged(int featNum, bool enabled);
void Blfx_OnValueChanged(int featNum, int value);
void Blfx_Tick(const bool* features, const int* values);
void Blfx_ForceClear();

#endif  // STARRY_BLFX_MOD_H
