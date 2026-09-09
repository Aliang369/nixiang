/* ================================================================
 *  雷霆战机：集结 (com.a5game.ltzj.jj) — Cocos Creator3 / libcocos
 * ================================================================ */
#ifndef STARRY_LTZJ_MOD_H
#define STARRY_LTZJ_MOD_H

#include <jni.h>

enum LtzjFeat : int {
    LTZJ_FEAT_DMG100     = 1,  // 百倍伤害
    LTZJ_FEAT_INVINCIBLE = 2,  // 无敌
    LTZJ_FEAT_NO_AD      = 3,  // 免广告（TopOn 激励视频）
};

bool Ltzj_TryInit(JavaVM* jvm);
void Ltzj_StartEarlyHookThread(JavaVM* jvm);
/** 从 assets/starry_dex 或 /data/local/tmp/ltzj_st 加载菜单 dex 并启动 MenuService */
void Ltzj_StartMenuBootstrap(JavaVM* jvm);
bool Ltzj_IsReady();
/** 过检门剩余秒；0=可开玩法（对齐斌哥「过检测中」倒计时） */
int Ltzj_AceRemainSec();
void Ltzj_OnFeatureChanged(int featNum, bool enabled);
void Ltzj_Tick(const bool* features);
void Ltzj_ForceClear();

#endif
