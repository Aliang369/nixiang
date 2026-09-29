/* ================================================================
 *  我在神界刷装备 (com.windforce.rg.offical) — Sjzz 菜单模块
 *
 *  引擎：Cocos Creator 3 / libcocos.so
 *  注入：Dobby hook se::ScriptEngine::evalString，对运行时 JS 文本插桩
 *  功能：1 伤害倍率 / 2 无敌 / 3 免广告 / 4 游戏加速
 * ================================================================ */
#ifndef STARRY_SJZZ_MOD_H
#define STARRY_SJZZ_MOD_H

#include <jni.h>

// 功能位号：必须与 SjzzEntry.cpp 的 featureList 数组下标一一对应
enum SjzzFeat : int {
    SJZZ_FEAT_DMG = 1,    // Slider_伤害倍率
    SJZZ_FEAT_INV = 2,    // Toggle_无敌
    SJZZ_FEAT_ADFREE = 3, // Toggle_免广告
    SJZZ_FEAT_SPEED = 4,  // Toggle_游戏加速
};

// 签名与 ModEntry.h 回调一致（Cocos 系需要 vm 挂 evalString）
bool Sjzz_TryInit(JavaVM* vm);
bool Sjzz_IsReady();
void Sjzz_OnFeatureChanged(int featNum, bool enabled);
void Sjzz_OnValueChanged(int featNum, int value);
void Sjzz_Tick(const bool* features, const int* values);
void Sjzz_ForceClear();
void Sjzz_StartEarlyHookThread(JavaVM* vm);

#endif  // STARRY_SJZZ_MOD_H
