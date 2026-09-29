/* ================================================================
 *  造梦西游·黎尤浩劫 (com.xm4399.zmxy6hj) — Cocos2djs / libcocos2djs
 *  现代注册表版：伤害倍率滑块 / 无敌 / 无限技能
 * ================================================================ */
#ifndef STARRY_ZMXY_MOD_H
#define STARRY_ZMXY_MOD_H

#include <jni.h>

enum ZmxyFeat : int {
    ZMXY_FEAT_DMG_MUL    = 1,  // Slider_伤害倍率:1:100:1
    ZMXY_FEAT_INVINCIBLE = 2,  // Toggle_无敌
    ZMXY_FEAT_ULTRA_SKILL = 3, // Toggle_无限技能（无CD+不耗蓝）
};

bool Zmxy_TryInit(JavaVM* jvm);
void Zmxy_StartEarlyHookThread(JavaVM* jvm);
bool Zmxy_IsReady();
void Zmxy_OnFeatureChanged(int featNum, bool enabled);
void Zmxy_OnValueChanged(int featNum, int value);
void Zmxy_Tick(const bool* features, const int* values);
void Zmxy_ForceClear();

#endif
