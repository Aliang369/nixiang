/* ================================================================
 *  星光菜单游戏模块 — 宝石战争
 *
 *  功能实现走 System.Network.dll IL 薄桥接，不 hook HybridCLR 共享入口。
 *  mod 仅维护开关状态并导出 C ABI。
 * ================================================================ */
#ifndef STARRY_BSZZ_MOD_H
#define STARRY_BSZZ_MOD_H

#include <jni.h>

// 功能位号：必须与 BszzEntry.cpp featureList 数组下标一一对应
enum BszzFeat : int {
    BSZZ_FEAT_ATTACK_MULT = 1,      // Slider_攻击倍率
    BSZZ_FEAT_GOD_MODE = 2,         // Toggle_上帝模式
    BSZZ_FEAT_UNLIMITED_SKILL = 3,  // Toggle_无限技能
};

bool Bszz_TryInit(JavaVM* vm);
bool Bszz_IsReady();
void Bszz_OnFeatureChanged(int featNum, bool enabled);
void Bszz_OnValueChanged(int featNum, int value);
void Bszz_Tick(const bool* features, const int* values);
void Bszz_ForceClear();

#endif  // STARRY_BSZZ_MOD_H
