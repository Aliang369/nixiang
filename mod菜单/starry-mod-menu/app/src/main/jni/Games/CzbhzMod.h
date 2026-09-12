/* ================================================================
 *  CzbhzMod — 《城主别慌张》星光模块（纯壳 / JS 补丁模式）
 *
 *  本作没有原生游戏逻辑可 hook：它是 Cocos Creator 3.8.8 的 web-mobile(H5)
 *  构建，跑在腾讯 X5 WebView 里，业务代码全部来自 CDN
 *  （assets/script/index.2946f.js，14MB / 2655 模块）。
 *
 *  所以本模块**不做任何原生 hook**，只承担「菜单功能位 + 开关状态」，
 *  真正的功能由页面内 JS 补丁实现：
 *    载体  assets/starry/czbhz_patch.js   （外科注入塞进游戏 APK）
 *    注入  com.starry.magic.h5box.StarryH5（addJavascriptInterface + 周期灌入）
 *    状态  补丁经 window.StarryH5.getState() 读菜单开关（本文件维护的位号）
 *
 *  详见 10_docs/城主别慌张/分析_总览_20260911.md
 * ================================================================ */
#ifndef STARRY_CZBHZ_MOD_H
#define STARRY_CZBHZ_MOD_H

#include <jni.h>

// 功能位号：必须与 CzbhzEntry.cpp 的 featureList 数组下标一一对应
enum CzbhzFeat : int {
    CZBHZ_FEAT_AD = 1,     // Toggle_免广告
    CZBHZ_FEAT_INV = 3,    // Toggle_无敌
    CZBHZ_FEAT_DMG = 4,    // SeekBar_伤害倍率秒杀_1_1000_50
    CZBHZ_FEAT_SPD = 5,    // SeekBar_移速_1_10_1
    CZBHZ_FEAT_SKILL = 6,  // Toggle_无限技能
};

// 签名与 ModEntry.h 回调一致（本作无原生 hook，vm 不用）
bool Czbhz_TryInit(JavaVM* vm);
bool Czbhz_IsReady();
void Czbhz_OnFeatureChanged(int featNum, bool enabled);
void Czbhz_OnValueChanged(int featNum, int value);
void Czbhz_Tick(const bool* features, const int* values);
void Czbhz_ForceClear();

#endif  // STARRY_CZBHZ_MOD_H
