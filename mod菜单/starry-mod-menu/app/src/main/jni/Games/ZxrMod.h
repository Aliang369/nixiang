/* ================================================================
 *  镇邪人 (com.zxr.dlxmz.zxrwz37zgx10.gg9b) — Cocos2djs / libcocos2djs
 * ================================================================ */
#ifndef STARRY_ZXR_MOD_H
#define STARRY_ZXR_MOD_H

#include <jni.h>

// g_featureList 数组下标（Category_ 占位）
enum ZxrFeat : int {
    ZXR_FEAT_SECKILL     = 1,  // Toggle_秒杀
    ZXR_FEAT_INVINCIBLE  = 2,  // Toggle_无敌
    ZXR_FEAT_ATK_SPEED   = 3,  // Toggle_攻速
    ZXR_FEAT_AOE_KILL    = 4,  // Toggle_全屏秒杀
    ZXR_FEAT_NO_AD       = 6,  // Toggle_免广告
    ZXR_FEAT_SPEEDUP     = 8,  // Slider_游戏加速:1:5（1=关）
};

bool Zxr_TryInit(JavaVM* jvm);
/** 尽早轮询挂 se::evalString（可在 JNI_OnLoad 起独立线程调用） */
void Zxr_StartEarlyHookThread(JavaVM* jvm);
bool Zxr_IsReady();
void Zxr_OnFeatureChanged(int featNum, bool enabled);
void Zxr_OnValueChanged(int featNum, int value);
void Zxr_Tick(const bool* features);
void Zxr_ForceClear();

#endif // STARRY_ZXR_MOD_H
