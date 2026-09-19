/* ================================================================
 *  明日方舟 2.7.71 星光模块（modern 注册表版）
 *
 *  功能位号必须与 ArknightsEntry.cpp 的 featureList 下标一致。
 *  运行态全部按命名空间/类名/方法名/参数数解析，不硬编码斌哥壳地址。
 * ================================================================ */
#ifndef STARRY_ARKNIGHTS_MOD_H
#define STARRY_ARKNIGHTS_MOD_H

#include <jni.h>

enum ArknightsFeat : int {
    ARKNIGHTS_FEAT_CATEGORY          = 0,  // Category_战斗
    ARKNIGHTS_FEAT_ATTACK_SPEED      = 1,  // Toggle_超级攻速
    ARKNIGHTS_FEAT_INVINCIBLE        = 2,  // Toggle_超级无敌
    ARKNIGHTS_FEAT_BASE_LOCK         = 3,  // Toggle_基地锁血
    ARKNIGHTS_FEAT_DAMAGE10          = 4,  // Toggle_十倍伤害
    ARKNIGHTS_FEAT_DEPLOY_ANYWHERE   = 5,  // Toggle_全图放置
    ARKNIGHTS_FEAT_INFINITE_SKILL    = 6,  // Toggle_无限技能
    ARKNIGHTS_FEAT_ZERO_COST         = 7,  // Toggle_零部署费用
};

bool Arknights_TryInit(JavaVM* vm);
bool Arknights_IsReady();
void Arknights_OnFeatureChanged(int featNum, bool enabled);
void Arknights_OnValueChanged(int featNum, int value);
void Arknights_Tick(const bool* features, const int* values);
void Arknights_ForceClear();

#endif  // STARRY_ARKNIGHTS_MOD_H
