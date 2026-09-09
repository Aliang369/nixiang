/* ================================================================
 *  青鸾繁华录 (com.hdg4.g4qlfh) — Cocos2djs / libcocos2djs
 *  v2：免广告三合一 + 战斗（客户端模拟器）百倍伤害/无敌 + 变速
 * ================================================================ */
#ifndef STARRY_QLFH_MOD_H
#define STARRY_QLFH_MOD_H

#include <jni.h>

enum QlfhFeat : int {
    QLFH_FEAT_NO_AD      = 1,  // Toggle_免广告（直领+本地广告+秒过三合一）→ get_ret(1)
    QLFH_FEAT_DMG100     = 2,  // Toggle_百倍伤害（客户端战斗模拟器）→ get_ret(2)
    QLFH_FEAT_INVINCIBLE = 3,  // Toggle_无敌（客户端战斗模拟器）→ get_ret(3)
    QLFH_FEAT_SPEED      = 4,  // Slider_游戏加速:1:10 → get_speed()
};

bool Qlfh_TryInit(JavaVM* jvm);
void Qlfh_StartEarlyHookThread(JavaVM* jvm);
bool Qlfh_IsReady();
void Qlfh_OnFeatureChanged(int featNum, bool enabled);
void Qlfh_OnValueChanged(int featNum, int value);
void Qlfh_Tick(const bool* features);
void Qlfh_ForceClear();

#endif // STARRY_QLFH_MOD_H
