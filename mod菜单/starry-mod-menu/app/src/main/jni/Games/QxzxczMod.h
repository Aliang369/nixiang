/* ================================================================
 *  QxzxczMod.h — 群侠传：幸存者（Godot 4.6.1 / GDScript）模块
 *
 *  本作无 il2cpp / 无 native hook 面：游戏逻辑在 assets/scripts/**.gdc（GDScript 字节码），
 *  功能实现走「静态改 .gdc」路线，运行时开关经 Java StateMirror 落盘的
 *  {filesDir}/starry_state.txt（= Godot user://starry_state.txt）由 GDScript 读取。
 *
 *  ⇒ 本模块**不装任何 hook**，只承担：
 *      1) 让注册表有合法的 featNum 表（菜单三项：伤害倍率 / 无敌 / 无限银钱）
 *      2) 让 native 侧接受开关值（nativeSetFeature/nativeSetValue 写 g_features/g_values）
 *      3) 保持 isReady()=true，避免菜单显示「native 未就绪」
 *
 *  功能位号必须与 QxzxczEntry.cpp 的 featureList 下标一一对应。
 * ================================================================ */
#ifndef STARRY_QXZXCZ_MOD_H
#define STARRY_QXZXCZ_MOD_H

#include <jni.h>

enum QxzxczFeat : int {
    QXZXCZ_FEAT_DMG    = 1,  // Slider_伤害倍率:1:10:1  → starry_state[1]
    QXZXCZ_FEAT_INV    = 2,  // Toggle_无敌           → starry_state[2]
    QXZXCZ_FEAT_SILVER = 3,  // Toggle_无限银钱       → starry_state[3]
};

bool Qxzxcz_TryInit(JavaVM* vm);
bool Qxzxcz_IsReady();
void Qxzxcz_OnFeatureChanged(int featNum, bool enabled);
void Qxzxcz_OnValueChanged(int featNum, int value);
void Qxzxcz_Tick(const bool* features, const int* values);
void Qxzxcz_ForceClear();

#endif  // STARRY_QXZXCZ_MOD_H
