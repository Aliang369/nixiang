/* ================================================================
 *  遗弃之地 (com.hortor.yqzd) — Cocos Creator 3.8.5 / hortor 框架
 *  CDN bundle game_index.js（SystemJS 2207 chunks，明文）
 *  零 native hook（ACE/TSS 环境）：EvalOnGLThread + System 模块原型 wrap
 * ================================================================ */
#ifndef STARRY_YQZD_MOD_H
#define STARRY_YQZD_MOD_H

#include <jni.h>

// 菜单 featureNum（对齐 Main.cpp g_features 列表下标，Category 占下标）
enum YqzdFeat : int {
    YQZD_FEAT_CAT_BATTLE = 0,  // Category_战斗
    YQZD_FEAT_INV        = 1,  // Toggle_无敌
    YQZD_FEAT_ATK_ON     = 2,  // Toggle_倍攻
    YQZD_FEAT_ATK_VAL    = 3,  // Slider_倍攻:10:100
    YQZD_FEAT_CAT_FUNC   = 4,  // Category_功能
    YQZD_FEAT_SPD_ON     = 5,  // Toggle_加速
    YQZD_FEAT_SPD_VAL    = 6,  // Slider_加速:1:5
    YQZD_FEAT_ADFREE     = 7   // Toggle_免广告
};

bool Yqzd_TryInit(JavaVM* jvm);
bool Yqzd_IsReady();
/** 菜单主循环每 ~500ms 调一次：推旗标 + 重装 wrap（热更后自愈） */
void Yqzd_Tick();
void Yqzd_OnFeatureChanged(int featNum, bool enabled);
void Yqzd_OnValueChanged(int featNum, int value);

#endif // STARRY_YQZD_MOD_H
