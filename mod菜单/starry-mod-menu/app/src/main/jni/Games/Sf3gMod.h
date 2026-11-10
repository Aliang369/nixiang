/* ================================================================
 *  Sf3gMod.h — 暗影格斗3 国际服（com.nekki.shadowfight3 / Google Play 1.46.1）
 *
 *  功能位（与 Sf3gEntry.cpp featureList 下标一一对应）：
 *    1 Slider_伤害倍率:1:100:1
 *    2 Toggle_上帝模式
 *  （战斗速度按用户 2026-11-06 要求不做）
 * ================================================================ */
#ifndef STARRY_SF3G_MOD_H
#define STARRY_SF3G_MOD_H

#include <jni.h>

enum Sf3gFeat : int {
    SF3G_FEAT_DMG = 1,  // 伤害倍率（1=原伤害）
    SF3G_FEAT_GOD = 2,  // 上帝模式
};

void Sf3g_OnLoad(JavaVM* vm);
bool Sf3g_TryInit(JavaVM* vm);
bool Sf3g_IsReady();
void Sf3g_OnFeatureChanged(int featNum, bool enabled);
void Sf3g_OnValueChanged(int featNum, int value);
void Sf3g_Tick(const bool* features, const int* values);
void Sf3g_ForceClear();

#endif  // STARRY_SF3G_MOD_H
