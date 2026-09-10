/* ================================================================
 *  无尽探险队 (com.wjtxdtt.zy) — Cocos Creator 3.8.75 / libUnityV114514.so
 *  对齐斌哥注入 JS（BSS 抓取）：get_ret 1..2
 *    1 超级无敌 — get invulnerable() getter → return true
 *    2 超级移速 — getFrameSpeed() 返回值 ×5
 * ================================================================ */
#ifndef STARRY_WJTXD_MOD_H
#define STARRY_WJTXD_MOD_H

#include <jni.h>

enum WjtxdFeat : int {
    WJTXD_FEAT_INV   = 1,  // 超级无敌
    WJTXD_FEAT_SPD   = 2,  // 超级移速
    WJTXD_FEAT_ADFREE = 3, // 免广告
};

bool Wjtxd_TryInit(JavaVM* jvm);
void Wjtxd_StartEarlyHookThread(JavaVM* jvm);
bool Wjtxd_IsReady();
void Wjtxd_OnFeatureChanged(int featNum, bool enabled);
void Wjtxd_Tick(const bool* features, const int* values);
void Wjtxd_ForceClear();

#endif  // STARRY_WJTXD_MOD_H