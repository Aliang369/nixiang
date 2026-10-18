/* ================================================================
 *  KdjxsfMod.h — 口袋觉醒（私服 com.zcdsg.xmgm 3.10）星光菜单模块
 *
 *  引擎：Cocos2d-x 3.x + LuaJIT 2.1(GC64)，主 Activity org.cocos2dx.lua.AppActivity
 *  玩法逻辑 100% 在 Lua（libcocos2dlua.so 内嵌 LuaJIT，lua_* 未导出）
 *  ⇒ 注入路线 = hook JNI 导出的 nativeRender（GL 线程）→ 用 RVA 取 lua C API
 *     → loadstring + pcall 注入 Lua monkey-patch（详见 10_docs/口袋觉醒/）
 * ================================================================ */
#ifndef STARRY_KDJXSF_MOD_H
#define STARRY_KDJXSF_MOD_H

#include <jni.h>

// 功能位号：必须与 KdjxsfEntry.cpp 的 featureList 下标一一对应
enum KdjxsfFeat : int {
    KDJXSF_FEAT_INV = 1,    // Toggle_无敌
    KDJXSF_FEAT_DMG = 2,    // Slider_伤害倍率:1:100:1（1 = 关）
    KDJXSF_FEAT_SPD = 3,    // Slider_战斗加速:1:5:1（1 = 关）
    KDJXSF_FEAT_SKILL = 4,  // Toggle_无限技能
};

bool Kdjxsf_TryInit(JavaVM* vm);
bool Kdjxsf_IsReady();
void Kdjxsf_OnFeatureChanged(int featNum, bool enabled);
void Kdjxsf_OnValueChanged(int featNum, int value);
void Kdjxsf_Tick(const bool* features, const int* values);
void Kdjxsf_ForceClear();

#endif  // STARRY_KDJXSF_MOD_H
