/* ================================================================
 *  堡垒之战 (com.ultralisk.game304) — Cocos Creator 3.8 / libcocos.so
 * ================================================================ */
#ifndef STARRY_BLZZ_MOD_H
#define STARRY_BLZZ_MOD_H

#include <jni.h>

enum BlzzFeat : int {
    BLZZ_FEAT_INVINCIBLE = 1,  // Toggle_无敌
    BLZZ_FEAT_ATK10      = 2,  // Toggle_十倍攻击（高倍秒杀）
    BLZZ_FEAT_XULI_NOCD  = 3,  // Toggle_蓄力无CD
    BLZZ_FEAT_UNLOCK     = 4,  // Button_解锁所有功能
};

bool Blzz_TryInit(JavaVM* jvm);
bool Blzz_IsReady();
void Blzz_OnFeatureChanged(int featNum, bool enabled);
void Blzz_Tick(const bool* features);
void Blzz_ForceClear();

#endif // STARRY_BLZZ_MOD_H
