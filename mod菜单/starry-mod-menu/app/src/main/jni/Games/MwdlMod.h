/* ================================================================
 *  迷雾大陆 (com.hortor.mwdl) — Cocos2djs / libcocos2djs
 *  get_ret 2/4/5/6 + get_val(1) 伤害倍率 / get_val(3) 移速倍率
 * ================================================================ */
#ifndef STARRY_MWDL_MOD_H
#define STARRY_MWDL_MOD_H

#include <jni.h>

enum MwdlFeat : int {
    MWDL_FEAT_DMG_RATE    = 1,  // 伤害倍率 Slider 1~1000（1=原伤害）
    MWDL_FEAT_INVINCIBLE  = 2,  // 无敌 Toggle
    MWDL_FEAT_MOVESPEED   = 3,  // 移动速度 Slider 1=关 2~10=倍率
    MWDL_FEAT_ULTRA_SKILL = 4,  // 无限技能 Toggle
    MWDL_FEAT_AUTO_FIGHT  = 5,  // 自动挂机 Toggle（开游戏自带 autoPlay）
    MWDL_FEAT_PICK_ALL    = 6,  // 全图拾取 Toggle（借「有宠物=全图拾取」分支）
};

bool Mwdl_TryInit(JavaVM* jvm);
void Mwdl_StartEarlyHookThread(JavaVM* jvm);
bool Mwdl_IsReady();
void Mwdl_OnFeatureChanged(int featNum, bool enabled);
void Mwdl_OnValueChanged(int featNum, int value);
void Mwdl_Tick(const bool* features);
void Mwdl_ForceClear();

#endif
