/* ================================================================
 *  XxwcMod — 修仙王朝模拟器 (com.sky.xxwc.xxt 1.0.4 / vc5)
 *  引擎：Cocos Creator 3.8.7 / libcocos.so / JSB
 *
 *  形态：native 零 hook。菜单开关经 Java CocosJsBridge 投 JS 到游戏线程，
 *        在 JS 层 monkey-patch 已注册模块（Haly 同款路线）。
 *  落点（1.0.4 静态解密实测）：
 *    · 免广告  模块 zghbhh45g4jh5d4fh5dfh45h1f 静态入口
 *              fgdfhgdfh45dfh45df4hr8hht(g,d,e)：临时置 window.g_config.checkPlayVideo=false
 *              → 游戏自带 else 分支 n(!0) 直接发奖
 *    · 超级无敌 模块 WWarQingjiao 原型 dealqjTanziResult / dealShenhaiResult：
 *              首参（胜率）抬到 100 → e<=t 恒成立，掉落判定 e>0 不受影响
 * ================================================================ */
#ifndef STARRY_XXWC_MOD_H
#define STARRY_XXWC_MOD_H

#include <jni.h>

// 功能位号：与 XxwcEntry.cpp 的 featureList 下标一一对应（Category_ 占 0）
enum XxwcFeat : int {
    XXWC_FEAT_NOAD = 1,  // Toggle_免广告
    XXWC_FEAT_INV  = 2,  // Toggle_超级无敌（战斗必胜）
};

void Xxwc_OnLoad(JavaVM* vm);
bool Xxwc_TryInit(JavaVM* vm);
bool Xxwc_IsReady();
void Xxwc_OnFeatureChanged(int featNum, bool enabled);
void Xxwc_OnValueChanged(int featNum, int value);
void Xxwc_Tick(const bool* features, const int* values);
void Xxwc_ForceClear();

#endif  // STARRY_XXWC_MOD_H
