/* ================================================================
 *  生存33天 (com.lgcb.scsst.sc33ttaptapwz37zgx3.dayssurvive) — Cocos2djs
 * ================================================================ */
#ifndef STARRY_SC33_MOD_H
#define STARRY_SC33_MOD_H

#include <jni.h>

enum Sc33Feat : int {
    SC33_FEAT_DMG100     = 1,  // Toggle_百倍伤害  → get_ret(1)
    SC33_FEAT_INVINCIBLE = 2,  // Toggle_无敌      → get_ret(2)
    SC33_FEAT_AOE_KILL   = 3,  // Toggle_全屏秒杀  → get_ret(3)
    SC33_FEAT_ATK_SPEED  = 4,  // Slider_攻速 菜单下标4 → get_ret(5) / getAtkMul
    SC33_FEAT_NO_AD      = 6,  // Toggle_免广告    → get_ret(4) Java
};

bool Sc33_TryInit(JavaVM* jvm);
void Sc33_StartEarlyHookThread(JavaVM* jvm);
bool Sc33_IsReady();
void Sc33_OnFeatureChanged(int featNum, bool enabled);
void Sc33_OnValueChanged(int featNum, int value);
void Sc33_Tick(const bool* features);
void Sc33_ForceClear();

#endif // STARRY_SC33_MOD_H
