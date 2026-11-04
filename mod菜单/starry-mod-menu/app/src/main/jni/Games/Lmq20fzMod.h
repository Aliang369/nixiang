/* ================================================================
 *  Lmq20fzMod.h — 黎明前20分钟（com.nanostudios.games.twenty.minutes 1.0.106）
 *
 *  抓点来源：10_docs/_batch_binge6/三款新版菜单_抓点总表_20261103.md §四.3
 *            + 10_docs/黎明前20分钟/路线锁定.md（本轮 1.0.106 元数据复核）
 * ================================================================ */
#ifndef STARRY_LMQ20FZ_MOD_H
#define STARRY_LMQ20FZ_MOD_H

#include <jni.h>

// 功能位号：必须与 Lmq20fzEntry.cpp 的 featureList 数组下标一一对应
enum Lmq20fzFeat : int {
    LMQ20FZ_FEAT_INV = 1,  // Toggle_无敌      → flanne.Health::HPChange 玩家侧扣血吞掉
    LMQ20FZ_FEAT_DMG = 2,  // Slider_百倍伤害  → flanne.Health::HPChange 敌方侧伤害倍率
    LMQ20FZ_FEAT_SPD = 3,  // Slider_战斗加速  → UnityEngine.Time::set_timeScale 乘算
    LMQ20FZ_FEAT_NOAD = 4, // Toggle_免广告    → AnyThinkAdsManager::ShowRewardAd 拦截 + 直发奖
};

// 签名与 ModEntry.h 回调一致（Unity 系忽略 vm）
bool Lmq20fz_TryInit(JavaVM* vm);
bool Lmq20fz_IsReady();
void Lmq20fz_OnFeatureChanged(int featNum, bool enabled);
void Lmq20fz_OnValueChanged(int featNum, int value);
void Lmq20fz_Tick(const bool* features, const int* values);
void Lmq20fz_ForceClear();

#endif  // STARRY_LMQ20FZ_MOD_H
