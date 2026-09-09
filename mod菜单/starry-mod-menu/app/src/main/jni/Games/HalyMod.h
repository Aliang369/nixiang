/* ================================================================
 *  黑暗领域 (com.haly.tap) — Cocos Creator 3.8 / libcocos.so
 * ================================================================ */
#ifndef STARRY_HALY_MOD_H
#define STARRY_HALY_MOD_H

#include <jni.h>

enum HalyFeat : int {
    HALY_FEAT_NO_AD      = 1,  // Toggle_免广告
    HALY_FEAT_NUHUO      = 2,  // Toggle_不羁怒火增强（长无敌/零CD）
    HALY_FEAT_XIANZU     = 3,  // Toggle_先祖之魂增强（高继承+长持续）
    HALY_FEAT_LEIFA      = 4,  // Toggle_雷罚打击增强
};

bool Haly_TryInit(JavaVM* jvm);
bool Haly_IsReady();
void Haly_OnFeatureChanged(int featNum, bool enabled);
void Haly_Tick(const bool* features);
void Haly_ForceClear();

#endif // STARRY_HALY_MOD_H
