/* ================================================================
 *  迷雾大陆 (com.hortor.mwdl) — Cocos2djs / libcocos2djs
 *  get_ret 1/2/4 + get_val(3) 移速倍率
 * ================================================================ */
#ifndef STARRY_MWDL_MOD_H
#define STARRY_MWDL_MOD_H

#include <jni.h>

enum MwdlFeat : int {
    MWDL_FEAT_DMG100      = 1,  // 百倍伤害 Toggle
    MWDL_FEAT_INVINCIBLE  = 2,  // 无敌 Toggle
    MWDL_FEAT_MOVESPEED   = 3,  // 移动速度 Slider 1=关 2~10=倍率
    MWDL_FEAT_ULTRA_SKILL = 4,  // 无限技能 Toggle
};

bool Mwdl_TryInit(JavaVM* jvm);
void Mwdl_StartEarlyHookThread(JavaVM* jvm);
bool Mwdl_IsReady();
void Mwdl_OnFeatureChanged(int featNum, bool enabled);
void Mwdl_OnValueChanged(int featNum, int value);
void Mwdl_Tick(const bool* features);
void Mwdl_ForceClear();

#endif
