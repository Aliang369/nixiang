/* ================================================================
 *  动物大联盟 (com.fightingstars.games.animal) — Cocos2d-x JS 模块
 *  注入：非导出 se::ScriptEngine::evalString 入口 splice
 *  功能：免广告 / 攻击倍率 / 上帝模式 / 无限技能 / 自动战斗 / 移速
 * ================================================================ */
#ifndef STARRY_DWDLM_MOD_H
#define STARRY_DWDLM_MOD_H

#include <jni.h>

enum DwdlmFeat : int {
    DWDLM_FEAT_ATK = 1,
    DWDLM_FEAT_GOD = 2,
    DWDLM_FEAT_SKILL = 3,
    DWDLM_FEAT_AUTO = 4,
    DWDLM_FEAT_SPEED = 5,
    DWDLM_FEAT_ADFREE = 6,
    DWDLM_FEAT_COUNT = 7,
    DWDLM_ATK_MIN = 1,
    DWDLM_ATK_MAX = 100,
    DWDLM_ATK_DEFAULT = 1,
    DWDLM_SPEED_MIN = 1,
    DWDLM_SPEED_MAX = 5,
    DWDLM_SPEED_DEFAULT = 1,
};

bool Dwdlm_TryInit(JavaVM* vm);
void Dwdlm_StartEarlyHookThread(JavaVM* vm);
void Dwdlm_PrepareSecureShell();
void Dwdlm_InstallPathProbe();
bool Dwdlm_IsReady();
void Dwdlm_OnFeatureChanged(int featNum, bool enabled);
void Dwdlm_OnValueChanged(int featNum, int value);
void Dwdlm_Tick(const bool* features, const int* values);
void Dwdlm_ForceClear();

#endif  // STARRY_DWDLM_MOD_H
