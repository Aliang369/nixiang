/* ================================================================
 *  造梦西游·黎尤浩劫 (com.xm4399.zmxy6hj) — Cocos2djs / libcocos2djs
 * ================================================================ */
#ifndef STARRY_ZMXY_MOD_H
#define STARRY_ZMXY_MOD_H

#include <jni.h>

enum ZmxyFeat : int {
    ZMXY_FEAT_DMG100      = 1,  // 百倍伤害
    ZMXY_FEAT_INVINCIBLE  = 2,  // 无敌
    ZMXY_FEAT_ULTRA_SKILL = 3,  // 无限技能（无CD+不耗蓝）
};

bool Zmxy_TryInit(JavaVM* jvm);
void Zmxy_StartEarlyHookThread(JavaVM* jvm);
bool Zmxy_IsReady();
void Zmxy_OnFeatureChanged(int featNum, bool enabled);
void Zmxy_Tick(const bool* features);
void Zmxy_ForceClear();

#endif
