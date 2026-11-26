/* ================================================================
 *  MfqlMod.h — 魔法奇录 (com.shw.mfqlm1 1.0.1 vc2) 星光菜单游戏模块
 *
 *  本作是 Cocos Creator 2.x（libcocos2djs.so + assets/assets/Game/index.js 明文 JS），
 *  本轮唯一功能「免广告」全部在 Java(MfqlHooks) + JS(index.js splice) 两层完成，
 *  native 侧**刻意 0 hook**（全开关关闭即零改写，见 hook-rules §16 懒装）。
 *
 *  功能位号：必须与 MfqlEntry.cpp 的 featureList 下标一一对应。
 * ================================================================ */
#ifndef STARRY_MFQL_MOD_H
#define STARRY_MFQL_MOD_H

#include <jni.h>

enum MfqlFeat : int {
    MFQL_FEAT_FREE_AD = 1,  // Toggle_免广告
};

bool Mfql_TryInit(JavaVM* vm);
bool Mfql_IsReady();
void Mfql_OnFeatureChanged(int featNum, bool enabled);
void Mfql_OnValueChanged(int featNum, int value);
void Mfql_Tick(const bool* features, const int* values);
void Mfql_ForceClear();

#endif  // STARRY_MFQL_MOD_H
