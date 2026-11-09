/* ================================================================
 *  农场守护者 2.1.21 / vc21 / com.qld.ncsh.xiyou
 *  星光菜单游戏模块 — 注册表版
 * ================================================================ */
#ifndef STARRY_NCSH_MOD_H
#define STARRY_NCSH_MOD_H

#include <jni.h>

// 功能位号：必须与 NcshEntry.cpp 的 featureList 数组下标一一对应
enum NcshFeat : int {
    NCSH_FEAT_DMG100 = 1,   // Toggle_百倍伤害
    NCSH_FEAT_INVULN = 2,   // Toggle_无敌
    NCSH_FEAT_SKILL  = 3,   // Toggle_无限技能
};

bool Ncsh_TryInit(JavaVM* vm);
bool Ncsh_IsReady();
void Ncsh_OnFeatureChanged(int featNum, bool enabled);
void Ncsh_OnValueChanged(int featNum, int value);
void Ncsh_Tick(const bool* features, const int* values);
void Ncsh_ForceClear();

#endif  // STARRY_NCSH_MOD_H
