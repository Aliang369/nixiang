/* ================================================================
 *  不一样的修仙宗门2 (com.gamebl.byydxxzm.xxt) — Cocos Creator 2.x JS 注入
 *  功能：无限仙玉 / 无限灵石 / 免广告
 *  机制：hook se::ScriptEngine::evalString，在主脚本末尾追加补丁 JS，
 *        菜单开关经 JNI -> Java Byydxxzm2Hooks -> CocosJsBridge 实时下发。
 * ================================================================ */
#ifndef STARRY_BYYDXXZM2_MOD_H
#define STARRY_BYYDXXZM2_MOD_H

#include <jni.h>

// 功能位号 = Entry featureList 下标
enum Byydxxzm2Feat : int {
    BYYDXXZM2_FEAT_YU   = 1,   // 无限仙玉
    BYYDXXZM2_FEAT_LING = 2,   // 无限灵石
    BYYDXXZM2_FEAT_NOAD = 3,   // 免广告
    BYYDXXZM2_FEAT_COUNT = 4,
};

void Byydxxzm2_StartEarlyHookThread(JavaVM* vm);
bool Byydxxzm2_TryInit(JavaVM* vm);
bool Byydxxzm2_IsReady();
void Byydxxzm2_OnFeatureChanged(int featNum, bool enabled);
void Byydxxzm2_OnValueChanged(int featNum, int value);
void Byydxxzm2_Tick(const bool* features, const int* values);
void Byydxxzm2_ForceClear();

#endif
