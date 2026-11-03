#ifndef STARRY_GJDGS_MOD_H
#define STARRY_GJDGS_MOD_H

#include <jni.h>

// 功能位号：与 GjdgsEntry.cpp 的 featureList 下标一一对应
enum GjdgsFeat : int {
    GJDGS_FEAT_GOLD  = 1,   // Toggle 金币满值
    GJDGS_FEAT_GEM   = 2,   // Toggle 宝石满值
    GJDGS_FEAT_SHARD = 3,   // Toggle 碎片满值
    GJDGS_FEAT_SPD   = 5,   // Slider 战斗加速 1-10（1=关）
    GJDGS_FEAT_GOD   = 6,   // Toggle 无敌
    GJDGS_FEAT_DMG   = 7,   // Slider 百倍伤害 1-100（1=关）
    GJDGS_FEAT_NOAD  = 8,   // Toggle 免广告
};

bool Gjdgs_TryInit(JavaVM* vm);
bool Gjdgs_IsReady();
void Gjdgs_OnFeatureChanged(int featNum, bool enabled);
void Gjdgs_OnValueChanged(int featNum, int value);
void Gjdgs_Tick(const bool* features, const int* values);
void Gjdgs_ForceClear();

#endif  // STARRY_GJDGS_MOD_H
