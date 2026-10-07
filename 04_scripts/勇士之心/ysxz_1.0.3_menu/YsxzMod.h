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
    // ---- 2026-10-07 新增（卡密包同名功能复刻，逻辑全在 JS 侧 auto_layer.js）----
    // 7 是 Category_新增功能 分组标题，占位不参与逻辑
    YSXZ_FEAT_GOLD = 8,    // Toggle_解锁金卡       JS: MOD.feature6
    // 9 是 Category_自动化 分组标题，占位不参与逻辑
    YSXZ_FEAT_REPEAT = 10, // Toggle_自动重复挑战   JS: MOD.feature7
    YSXZ_FEAT_SELL = 11,   // Toggle_自动出售       JS: MOD.feature8
    YSXZ_FEAT_DECO = 12,   // Toggle_自动分解       JS: MOD.feature9
    YSXZ_FEAT_WEAR = 13,   // Toggle_自动穿戴       JS: MOD.feature10
    YSXZ_FEAT_COUNT = 14,
};

bool Ysxz_TryInit(JavaVM* vm);
bool Ysxz_IsReady();
void Ysxz_OnLoad(JavaVM* vm);
void Ysxz_OnFeatureChanged(int featNum, bool enabled);
void Ysxz_OnValueChanged(int featNum, int value);
void Ysxz_Tick(const bool* features, const int* values);
void Ysxz_ForceClear();

#endif  // STARRY_YSXZ_MOD_H
