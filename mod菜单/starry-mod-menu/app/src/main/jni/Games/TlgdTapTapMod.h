/* ================================================================
 *  TlgdTapTapMod.h — 弹力果冻 TapTap 渠道（com.ginicat.sweet）2.4.0
 *  4 功能：秒杀 / 全屏秒杀 / 无敌 / 无限技能
 *
 *  feat 值 = MainModern 注册表 featureList 下标（Category_ 占位也占下标）
 * ================================================================ */
#ifndef STARRY_TLGD_TAPTAP_MOD_H
#define STARRY_TLGD_TAPTAP_MOD_H

#include <jni.h>

enum TlgdTapTapFeat : int {
    TLGDTT_FEAT_CAT_BATTLE = 0,  // Category_战斗（占位，仅供 UI 分组）
    TLGDTT_FEAT_KILL       = 1,  // Toggle_秒杀       HpHandle.finalDamage → 99999999
    TLGDTT_FEAT_AOE        = 2,  // Toggle_全屏秒杀   主目标击杀后补发敌方 eid 伤害
    TLGDTT_FEAT_INV        = 3,  // Toggle_无敌       玩家受击时 beAttackerID → 0
    TLGDTT_FEAT_NOCD       = 4,  // Toggle_无限技能   保留已验证 CanUseTimes/cdCounter 数据链
};

bool TlgdTapTap_TryInit(JavaVM* vm);
bool TlgdTapTap_IsReady();
void TlgdTapTap_Tick(const bool* features, const int* values);
void TlgdTapTap_OnFeatureChanged(int featNum, bool enabled);
void TlgdTapTap_OnValueChanged(int featNum, int value);
void TlgdTapTap_ForceClear();

#endif  // STARRY_TLGD_TAPTAP_MOD_H
