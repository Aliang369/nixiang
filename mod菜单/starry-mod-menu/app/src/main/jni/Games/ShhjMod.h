/* ================================================================
 *  山海幻境 (com.chx.shhj) — Cocos2djs / libcocos2djs
 *  对齐斌哥：GXR/GXD + tHurt 无敌 / curHp·createNum 倍率
 * ================================================================ */
#ifndef STARRY_SHHJ_MOD_H
#define STARRY_SHHJ_MOD_H

#include <jni.h>

enum ShhjFeat : int {
    SHHJ_FEAT_INVINCIBLE = 1,  // 无敌     → get_ret(2) / GXR(2)
    SHHJ_FEAT_DMG100     = 2,  // 百倍伤害 → get_ret(3) / GXR(3)
};

bool Shhj_TryInit(JavaVM* jvm);
void Shhj_StartEarlyHookThread(JavaVM* jvm);
bool Shhj_IsReady();
void Shhj_OnFeatureChanged(int featNum, bool enabled);
void Shhj_Tick(const bool* features);
void Shhj_ForceClear();

#endif
