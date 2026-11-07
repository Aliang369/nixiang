/* ================================================================
 *  墨守孤城 (com.chillyroom.inkvasion.yw) 星光模块头
 *  功能位号与 MsgcEntry.cpp 的 featureList 下标一一对应（下标 = featNum）
 * ================================================================ */
#ifndef STARRY_MSGC_MOD_H
#define STARRY_MSGC_MOD_H

#include <jni.h>

enum MsgcFeat : int {
    MSGC_FEAT_INF_RES        = 1,  // Toggle_建造资源无限
    MSGC_FEAT_NO_BUILD_TIME  = 2,  // Toggle_无建造时间
    MSGC_FEAT_BUILD_GOD      = 3,  // Toggle_建筑无限生命
    MSGC_FEAT_TROOP_GOD      = 4,  // Toggle_部队无限生命
};

bool Msgc_TryInit(JavaVM* vm);
bool Msgc_IsReady();
void Msgc_OnFeatureChanged(int featNum, bool enabled);
void Msgc_OnValueChanged(int featNum, int value);
void Msgc_Tick(const bool* features, const int* values);
void Msgc_ForceClear();

#endif  // STARRY_MSGC_MOD_H
