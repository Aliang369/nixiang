/* ================================================================
 *  境界刀鸣（com.blade.bd）— 星光菜单游戏模块
 *
 *  生成：bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh Jjdm Jjdm
 *  入口：MainModern.cpp（注册表模式；Main.cpp/GameConfig.h/公共 Android.mk 均零修改）
 *  实现：JjdmMod.cpp（il2cpp 按名找 LuaManager → 取 lua_State → 注入 Lua 补丁）
 * ================================================================ */
#ifndef STARRY_JJDM_MOD_H
#define STARRY_JJDM_MOD_H

#include <jni.h>

// 功能位号：必须与 JjdmEntry.cpp 的 featureList 数组下标一一对应
enum JjdmFeat : int {
    JJDM_FEAT_DMG = 1,   // Slider_伤害倍率:1:100:10（1 = 关闭）
    JJDM_FEAT_INV = 2,   // Toggle_无敌
    JJDM_FEAT_NOCD = 3,  // Toggle_无限技能
};

// 签名与 ModEntry.h 回调一致
bool Jjdm_TryInit(JavaVM* vm);
bool Jjdm_IsReady();
void Jjdm_OnFeatureChanged(int featNum, bool enabled);
void Jjdm_OnValueChanged(int featNum, int value);
void Jjdm_Tick(const bool* features, const int* values);
void Jjdm_ForceClear();

#endif  // STARRY_JJDM_MOD_H
