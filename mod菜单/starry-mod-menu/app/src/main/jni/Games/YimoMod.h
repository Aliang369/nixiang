/* ================================================================
 *  伊莫（com.x.yimoo.official）— 星光菜单游戏模块
 *
 *  引擎：Unity IL2CPP + xLua（玩法逻辑全在 Lua，不在 C#）
 *  注入路线（同 Jjdm / Match2 已验证的 xLua 路线）：
 *    ① dlopen("libxlua.so") + dlsym 取 Lua C API（不硬编码偏移）
 *    ② Dobby hook lua_pcall / lua_gettop → 直接拿到 lua_State*，
 *       且天然运行在调用 Lua 的线程（游戏逻辑 = 主线程）
 *    ③ 在该 hook 内用 luaL_loadstring + lua_pcall 注入 Lua 补丁，
 *       并用同一通道把菜单开关/滑块值写进 Lua 全局 YM
 *
 *  抓点依据：10_docs/伊莫/斌哥hook锚点_20260930.json
 *           10_docs/伊莫/抓点总结_斌哥mod_20260930.md
 * ================================================================ */
#ifndef STARRY_YIMO_MOD_H
#define STARRY_YIMO_MOD_H

#include <jni.h>

// 功能位号：必须与 YimoEntry.cpp 的 featureList 数组下标一一对应
enum YimoFeat : int {
    YIMO_FEAT_DMG  = 1,  // Slider_伤害倍率:1:100:1（1 = 关闭；原「百倍伤害」改名）
    YIMO_FEAT_SPD  = 2,  // Slider_移速:1:10:1（1 = 关闭；原「移速x3」改滑块）
    YIMO_FEAT_INV  = 3,  // Toggle_玩家无敌
    YIMO_FEAT_JUMP = 4,  // Toggle_无限跳跃
    YIMO_FEAT_STAM = 5,  // Toggle_无限体力
    YIMO_FEAT_TP   = 6,  // Toggle_瞬移开箱
};

// 签名与 ModEntry.h 回调一致
bool Yimo_TryInit(JavaVM* vm);
bool Yimo_IsReady();
void Yimo_OnFeatureChanged(int featNum, bool enabled);
void Yimo_OnValueChanged(int featNum, int value);
void Yimo_Tick(const bool* features, const int* values);
void Yimo_ForceClear();

#endif  // STARRY_YIMO_MOD_H
