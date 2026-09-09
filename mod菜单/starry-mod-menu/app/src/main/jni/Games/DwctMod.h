/* ================================================================
 *  动物餐厅 (inland.twgame.restaurant) — Cocos Creator JSB
 *  功能：免广告 / 免内购（对照分析笔记 §6.2 / §8）
 * ================================================================ */
#ifndef STARRY_DWCT_MOD_H
#define STARRY_DWCT_MOD_H

#include <jni.h>

// g_featureList 数组下标（Category_ 占位，逻辑勿用 Category 下标）
enum DwctFeat : int {
    DWCT_FEAT_NO_AD  = 1,  // Toggle_免广告
    DWCT_FEAT_NO_IAP = 2,  // Toggle_免内购
};

/** 等 libcocos2djs + AppActivity 就绪；可重复调用 */
bool Dwct_TryInit(JavaVM* jvm);

bool Dwct_IsReady();

/** 开关变化时立即同步 JS 标志位并尝试安装补丁 */
void Dwct_OnFeatureChanged(int featNum, bool enabled);

/**
 * 主循环调用（建议 500ms~1s）。
 * features: starry g_features[64]
 * 未就绪时会重试安装 JS 补丁。
 */
void Dwct_Tick(const bool* features);

#endif // STARRY_DWCT_MOD_H
