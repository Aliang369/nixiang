/* ================================================================
 *  HxwjMod.h — 幻想武将模拟器（com.qxjc.hxwjmlq / TapTap 1.0.8 / vc8）
 *
 *  入口：MainModern.cpp（注册表模式；Main.cpp/GameConfig.h/公共 Android.mk 零修改）
 *  实现：HxwjMod.cpp
 * ================================================================ */
#ifndef STARRY_HXWJ_MOD_H
#define STARRY_HXWJ_MOD_H

#include <jni.h>

// 功能位号：必须与 HxwjEntry.cpp 的 featureList 数组下标一一对应
enum HxwjFeat : int {
    HXWJ_FEAT_DMG = 1,   // Toggle_秒杀
    HXWJ_FEAT_DROP = 2,  // Toggle_百分百掉落
    HXWJ_FEAT_AD = 3,    // Toggle_免广告
};

bool Hxwj_TryInit(JavaVM* vm);
bool Hxwj_IsReady();
void Hxwj_OnFeatureChanged(int featNum, bool enabled);
void Hxwj_OnValueChanged(int featNum, int value);
void Hxwj_Tick(const bool* features, const int* values);
void Hxwj_ForceClear();

#endif  // STARRY_HXWJ_MOD_H
