/* ================================================================
 *  燃烧纪元 (com.rsjy.yf) — Cocos2d-x JS 模块
 *
 *  注入：se::ScriptEngine::evalString 明文 bundle 末尾 splice
 *  功能：
 *    1 Slider_攻击倍率 1-100   （1=普通，2..100=伤害倍率）
 *    2 Toggle_上帝模式
 *    3 Slider_战斗速度 1-10   （1=普通，2..10=倍率）
 *    4 Toggle_免广告
 * ================================================================ */
#ifndef STARRY_RSJY_MOD_H
#define STARRY_RSJY_MOD_H

#include <jni.h>

// 功能位号：必须与 RsjyEntry.cpp 的 featureList 数组下标一一对应
enum RsjyFeat : int {
    RSJY_FEAT_KILL = 1,
    RSJY_FEAT_GOD = 2,
    RSJY_FEAT_SPEED = 3,
    RSJY_FEAT_ADFREE = 4,
    RSJY_FEAT_COUNT = 5,

    RSJY_KILL_MIN = 1,
    RSJY_KILL_MAX = 100,
    RSJY_KILL_DEFAULT = 1,
    RSJY_SPEED_MIN = 1,
    RSJY_SPEED_MAX = 10,
    RSJY_SPEED_DEFAULT = 1,
};

bool Rsjy_TryInit(JavaVM* vm);
void Rsjy_StartEarlyHookThread(JavaVM* vm);
bool Rsjy_IsReady();
void Rsjy_OnFeatureChanged(int featNum, bool enabled);
void Rsjy_OnValueChanged(int featNum, int value);
void Rsjy_Tick(const bool* features, const int* values);
void Rsjy_ForceClear();

#endif  // STARRY_RSJY_MOD_H
