/* ================================================================
 *  造梦无双 (com.xm4399.zmws.m4399) — Cocos2djs / libcocos2djs
 *  对齐斌哥：get_ret 1..4
 * ================================================================ */
#ifndef STARRY_ZMWS_MOD_H
#define STARRY_ZMWS_MOD_H

#include <jni.h>

enum ZmwsFeat : int {
    ZMWS_FEAT_ULTRA_SKILL = 1,  // 无限技能
    ZMWS_FEAT_FABAO       = 2,  // 法宝技能
    ZMWS_FEAT_SECKILL     = 3,  // 超级秒杀
    ZMWS_FEAT_INVINCIBLE  = 4,  // 超级无敌
};

bool Zmws_TryInit(JavaVM* jvm);
void Zmws_StartEarlyHookThread(JavaVM* jvm);
bool Zmws_IsReady();
void Zmws_OnFeatureChanged(int featNum, bool enabled);
void Zmws_Tick(const bool* features);
void Zmws_ForceClear();

#endif
