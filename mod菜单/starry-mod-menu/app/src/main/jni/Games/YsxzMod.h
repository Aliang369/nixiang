#ifndef STARRY_YSXZ_MOD_H
#define STARRY_YSXZ_MOD_H

#include <jni.h>

enum YsxzFeat : int {
    YSXZ_FEAT_KILL = 1,    // Toggle_秒杀
    YSXZ_FEAT_INV = 2,     // Toggle_无敌
    YSXZ_FEAT_SKILL = 3,   // Toggle_无限技能
    YSXZ_FEAT_MOVE = 4,    // Toggle_移动速度
    YSXZ_FEAT_AUTO = 5,    // Toggle_自动战斗
    YSXZ_FEAT_COUNT = 6,
};

bool Ysxz_TryInit(JavaVM* vm);
bool Ysxz_IsReady();
void Ysxz_OnLoad(JavaVM* vm);
void Ysxz_OnFeatureChanged(int featNum, bool enabled);
void Ysxz_OnValueChanged(int featNum, int value);
void Ysxz_Tick(const bool* features, const int* values);
void Ysxz_ForceClear();

#endif  // STARRY_YSXZ_MOD_H
