/* ================================================================
 *  我本布衣世家录Mod.h — 我本布衣：世家录 (com.JAGames.TalesoftheClan / TapTap 1.1.46 / vc55)
 *
 *  入口：MainModern.cpp（注册表模式；Main.cpp/GameConfig.h/公共 Android.mk 零修改）
 *  实现：我本布衣世家录Mod.cpp
 * ================================================================ */
#ifndef STARRY_WBJY_MOD_H
#define STARRY_WBJY_MOD_H

#include <jni.h>

// 功能位号：必须与我本布衣世家录Entry.cpp 的 featureList 数组下标一一对应
enum WbjyFeat : int {
    WBJY_FEAT_AD = 1,
    WBJY_FEAT_COPPER = 2,
    WBJY_FEAT_GOLD = 3,
    WBJY_FEAT_AP = 4,
    WBJY_FEAT_RESEARCH = 5,
    WBJY_FEAT_LAND = 6,
};

bool Wbjy_TryInit(JavaVM* vm);
bool Wbjy_IsReady();
void Wbjy_OnFeatureChanged(int featNum, bool enabled);
void Wbjy_OnValueChanged(int featNum, int value);
void Wbjy_Tick(const bool* features, const int* values);
void Wbjy_ForceClear();

#endif  // STARRY_WBJY_MOD_H
