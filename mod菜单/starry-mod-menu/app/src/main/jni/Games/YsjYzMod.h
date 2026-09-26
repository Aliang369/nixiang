/* ================================================================
 *  异世界勇者 (com.mudamuda.idlewow 8.1.1) — Cocos Creator 2.4.2
 *  libcocos2djs.so + assets/main/index.jsc（XXTEA+gzip 解密后 2.99MB 明文）
 *
 *  注入方式：Dobby hook se::ScriptEngine::evalString（按名解析），
 *            命中主 bundle 后在明文末尾追加 JS 补丁块（同一次 evalString 调用内完成；
 *            补丁在全部模块执行完毕之后运行 → cc.Service / cc.UIHelper / BattleHero 均已就绪）。
 *
 *  功能位号 = YsjYzEntry.cpp featureList 下标（Category_ 占下标，不在此 enum 中）
 * ================================================================ */
#ifndef STARRY_YSJYZ_MOD_H
#define STARRY_YSJYZ_MOD_H

#include <jni.h>

enum YsjYzFeat : int {
    // 单一分类：0 = Category_基础功能（用户要求只保留一个分类），功能一律排在分类之后
    YSJYZ_FEAT_ADFREE = 1,   // Toggle_免广告
    YSJYZ_FEAT_SKILL  = 2,   // Toggle_无限技能（零CD + 不耗蓝，合并项）
    YSJYZ_FEAT_INV    = 3,   // Toggle_无敌
    YSJYZ_FEAT_DMG    = 4,   // SeekBar_伤害倍率（1-100，默认 1）
    YSJYZ_FEAT_GSPD   = 5,   // SeekBar_游戏倍速（1-5，默认 1）
    YSJYZ_FEAT_COUNT  = 6,   // = featureList 长度
};

bool YsjYz_TryInit(JavaVM* jvm);
void YsjYz_StartEarlyHookThread(JavaVM* jvm);
bool YsjYz_IsReady();
void YsjYz_OnFeatureChanged(int featNum, bool enabled);
void YsjYz_OnValueChanged(int featNum, int value);
void YsjYz_Tick(const bool* features, const int* values);
void YsjYz_ForceClear();

#endif  // STARRY_YSJYZ_MOD_H
