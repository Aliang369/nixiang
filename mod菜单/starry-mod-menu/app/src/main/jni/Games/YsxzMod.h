#ifndef STARRY_YSXZ_MOD_H
#define STARRY_YSXZ_MOD_H

#include <jni.h>

enum YsxzFeat : int {
    YSXZ_FEAT_DAMAGE = 1,  // Slider_伤害倍率 1..50（1=原倍率）—— 序号=菜单位置，勿与 JS 脱钩
    YSXZ_FEAT_KILL = 2,    // Toggle_秒杀
    YSXZ_FEAT_INV = 3,     // Toggle_无敌
    YSXZ_FEAT_SKILL = 4,   // Toggle_无限技能
    YSXZ_FEAT_MOVE = 5,    // Slider_移动速度 1..10
    YSXZ_FEAT_AUTO = 6,    // Toggle_自动战斗
    YSXZ_FEAT_COUNT = 7,
};

bool Ysxz_TryInit(JavaVM* vm);
bool Ysxz_IsReady();
void Ysxz_OnLoad(JavaVM* vm);
void Ysxz_OnFeatureChanged(int featNum, bool enabled);
void Ysxz_OnValueChanged(int featNum, int value);
void Ysxz_Tick(const bool* features, const int* values);
void Ysxz_ForceClear();

#endif  // STARRY_YSXZ_MOD_H
