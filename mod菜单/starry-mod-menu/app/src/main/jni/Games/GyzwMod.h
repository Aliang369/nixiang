/* ================================================================
 *  光阴之外（com.shbq.gyzw.ppyx 1.0.3 TapTap 渠道）— 星光菜单游戏模块
 *
 *  引擎：Cocos Creator 3.x（libcocos2djs.so）
 *  注入机制：Dobby hook se::ScriptEngine::evalString，在 boot JS
 *            （assets/script/index.jsc 解密后的 1.87MB 文本）末尾追加
 *            星光前缀，并对 4 处锚点做原地替换。
 *
 *  抓点来源：斌哥改包运行时内存 dump
 *            （10_docs/光阴之外/斌哥hook锚点_20260926.json）
 *            —— 同锚点、同语义，重写为星空私有实现。
 *
 *  功能位号必须与 GyzwEntry.cpp 的 featureList 数组下标一一对应。
 * ================================================================ */
#ifndef STARRY_GYZW_MOD_H
#define STARRY_GYZW_MOD_H

#include <jni.h>

enum GyzwFeat : int {
    GYZW_FEAT_DMG   = 1,  // [1] Toggle_百倍伤害
    GYZW_FEAT_INV   = 2,  // [2] Toggle_超级无敌
    GYZW_FEAT_SPEED = 3,  // [3] Slider_战斗加速:1:10:1（1=关，2..10=倍率）
    GYZW_FEAT_NOAD  = 5,  // [5] Toggle_免广告   （[4] 是 Category_广告）
};

bool Gyzw_TryInit(JavaVM* vm);
bool Gyzw_IsReady();
void Gyzw_OnFeatureChanged(int featNum, bool enabled);
void Gyzw_OnValueChanged(int featNum, int value);
void Gyzw_Tick(const bool* features, const int* values);
void Gyzw_ForceClear();
void Gyzw_StartEarlyHookThread(JavaVM* vm);
void Gyzw_OnLoad(JavaVM* vm);

#endif  // STARRY_GYZW_MOD_H
