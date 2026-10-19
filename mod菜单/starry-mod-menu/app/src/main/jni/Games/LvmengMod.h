/* ================================================================
 *  绿梦：时空之声 — 星光 per-game 壳（无 il2cpp hook）
 *
 *  功能执行在热更 C#（Assembly-ACT.dll 的 IL 补丁）里；本壳只负责：
 *    1) 向菜单注册 5 个功能项（featNum 与数组下标一一对应）
 *    2) 把开关/数值按契约写进
 *       /data/data/com.zhuziplay.lvmengnew.op/files/starry_lvmeng_state.txt
 *       （独立文件名，避开框架 StateMirror 的 starry_state.txt 通道）
 *  热更 C# 每 30 固定帧读该文件 → 执行。so 与 C# 之间没有反向调用。
 */
#ifndef STARRY_LVMENG_MOD_H
#define STARRY_LVMENG_MOD_H

#include <jni.h>

enum LvmengFeat : int {
    LVMENG_FEAT_CAT   = 0,  // Category_战斗（占下标，不参与执行）
    LVMENG_FEAT_HP    = 1,  // InputValue_0:自定义血量（默认 0 不生效）
    LVMENG_FEAT_SPD   = 2,  // Slider_全局变速:1:10:1（UI 1x..10x，内部 ×100）
    LVMENG_FEAT_DMG   = 3,  // Slider_伤害倍率:1:1000:1（UI 1x..1000x，内部 ×100）
    LVMENG_FEAT_SKILL = 4,  // Toggle_无限技能
    LVMENG_FEAT_JUMP  = 5,  // Toggle_无限跳跃
    LVMENG_FEAT_COUNT = 6,
};

bool Lvmeng_TryInit(JavaVM* vm);
bool Lvmeng_IsReady();
void Lvmeng_Tick(const bool* features, const int* values);
void Lvmeng_OnFeatureChanged(int featNum, bool enabled);
void Lvmeng_OnValueChanged(int featNum, int value);
void Lvmeng_OnInputChanged(int featNum, const char* text);
void Lvmeng_ForceClear();

#endif  // STARRY_LVMENG_MOD_H
