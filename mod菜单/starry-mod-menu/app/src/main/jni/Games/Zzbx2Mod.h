/* ================================================================
 *  正中靶心2 (com.openew.monster) — Cocos2d-x JS 模块
 *  注入：se::ScriptEngine::evalString 主脚本末尾 splice
 *  功能：秒杀 / 无敌 / 战斗加速 / 免广告
 * ================================================================ */
#ifndef STARRY_ZZBX2_MOD_H
#define STARRY_ZZBX2_MOD_H

#include <jni.h>

// 功能位号：必须与 Zzbx2Entry.cpp 的 featureList 数组下标一一对应
enum Zzbx2Feat : int {
    ZZBX2_FEAT_ATK = 1,
    ZZBX2_FEAT_INV = 2,
    ZZBX2_FEAT_SPD = 3,
    ZZBX2_FEAT_ADFREE = 4,
    ZZBX2_FEAT_COUNT = 5,
    ZZBX2_ATK_MIN = 1,
    ZZBX2_ATK_MAX = 100,
    ZZBX2_ATK_DEFAULT = 1,
    ZZBX2_SPEED_MIN = 1,
    ZZBX2_SPEED_MAX = 5,
    ZZBX2_SPEED_DEFAULT = 1,
};

// 签名与 ModEntry.h 回调一致。
bool Zzbx2_TryInit(JavaVM* vm);
void Zzbx2_StartEarlyHookThread(JavaVM* vm);
bool Zzbx2_IsReady();
void Zzbx2_OnFeatureChanged(int featNum, bool enabled);
void Zzbx2_OnValueChanged(int featNum, int value);
void Zzbx2_Tick(const bool* features, const int* values);
void Zzbx2_ForceClear();

#endif  // STARRY_ZZBX2_MOD_H
