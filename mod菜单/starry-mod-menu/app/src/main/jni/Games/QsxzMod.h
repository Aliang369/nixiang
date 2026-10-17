/* ================================================================
 *  QsxzMod — 《炁术行者》星光模块（纯壳 / JS 补丁模式）
 *
 *  本作没有原生游戏逻辑可 hook：包体是 1.4MB 的纯 WebView 壳，
 *  游戏本体是 Cocos Creator 3.8.2 web-mobile(H5)，玩法 JS 由服务端
 *  http://43.248.140.201:8888 明文下发。因此：
 *
 *    ✗ 不做任何原生 hook（无 libil2cpp / libcocos，无可挂目标）
 *    ✓ 本模块只维护「菜单功能位 ↔ 开关状态」
 *    ✓ 功能主体 = assets/starry/qsxz_patch.js（页面内 JS 补丁，
 *      经 window.StarryH5.getState() 读菜单状态，live=0 时哑火）
 *
 *  数值口径（2026-10-18 用户定制放宽，勿再擅改）：
 *    · 伤害倍率 1~100：默认 1 = 原版（上限 100 = 用户定制）
 *    · 减伤 1~100%：默认 1 = 最低档；0 仅作「未授权原版」哨兵（上限 100 = 用户定制）
 *    · 战斗加速 1~10：1 = 原版完全不干预；上限 10 = 用户定制（>3 服务端 9998 拒判风险已知）
 *    · 真无敌不做：hp≈0 仍存活会被服务端 9988 拒判（减伤 100% 等同真无敌，慎用）
 *    · 上报战力一律不动：放大战力会被 9987 拒判，且用户明令禁止
 * ================================================================ */
#ifndef STARRY_QSXZ_MOD_H
#define STARRY_QSXZ_MOD_H

#include <jni.h>

// 功能位号：必须与 QsxzEntry.cpp 的 featureList 数组下标一一对应
enum QsxzFeat : int {
    QSXZ_FEAT_DMG = 1,    // SeekBar_伤害倍率_1_100_1
    QSXZ_FEAT_DR  = 2,    // SeekBar_减伤_1_100_1
    QSXZ_FEAT_SPEED = 3,  // SeekBar_战斗加速_1_10_1（1 = 原版；上限 10）
};

// 签名与 ModEntry.h 回调一致（本作无原生 hook，vm 不用）
bool Qsxz_TryInit(JavaVM* vm);
bool Qsxz_IsReady();
void Qsxz_OnFeatureChanged(int featNum, bool enabled);
void Qsxz_OnValueChanged(int featNum, int value);
void Qsxz_Tick(const bool* features, const int* values);
void Qsxz_ForceClear();

#endif  // STARRY_QSXZ_MOD_H
