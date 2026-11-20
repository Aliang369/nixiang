/* ================================================================
 *  ywtMod.h — 鹦鹉团（com.jinsaina.auditionsgo.gw 1.0.7）Lua 注入模块
 *
 *  落地方式：hook libxlua.so 的 lua_pcall → 运行时 luaL_loadstring 注入
 *  （设计：10_docs/鹦鹉团/鹦鹉团_Lua注入落地设计_20261119.md）
 *  功能：① 自动跳舞（走游戏自带 DanceVM.SetCheatMode，官方 Auto 链路）
 *        ② 判定概率（六档权重，包装 GetJudgeResult + Ctrl.OnJudgeResult）
 * ================================================================ */
#ifndef STARRY_YWT_MOD_H
#define STARRY_YWT_MOD_H

#include <jni.h>

// 功能位号：必须与 ywtEntry.cpp 的 featureList 数组下标一一对应
enum ywtFeat : int {
    YWT_FEAT_AUTO_DANCE = 1,   // Toggle_自动跳舞
    YWT_FEAT_JUDGE_ON   = 3,   // Toggle_判定概率
    YWT_FEAT_W_SPP      = 4,   // SeekBar_超完美+ 0-100（默认 70）
    YWT_FEAT_W_SP       = 5,   // SeekBar_超完美（15）
    YWT_FEAT_W_P        = 6,   // SeekBar_完美（8）
    YWT_FEAT_W_G        = 7,   // SeekBar_优秀（4）
    YWT_FEAT_W_GRT      = 8,   // SeekBar_良好(Great)（2）
    YWT_FEAT_W_GOOD     = 9,   // SeekBar_一般(Good)（1）——原「失误」已删除，此位改绑 Good
};

bool ywt_TryInit(JavaVM* vm);
bool ywt_IsReady();
void ywt_OnFeatureChanged(int featNum, bool enabled);
void ywt_OnValueChanged(int featNum, int value);
void ywt_Tick(const bool* features, const int* values);
void ywt_ForceClear();

#endif  // STARRY_YWT_MOD_H
