/* ================================================================
 *  超维进化 (com.grinning.beta.yszl) — Cocos Creator 2.x / libcocos2djs
 *  game.js 明文（v2.2 内置 + 0.2.89+ 热更双形态），se::evalString splice
 *  20260909 功能重排：移除秒杀/无敌/无限技能（封号风险），改属性增益系
 * ================================================================ */
#ifndef STARRY_CWJH_MOD_H
#define STARRY_CWJH_MOD_H

#include <jni.h>

// 菜单 featureNum（对齐 Main.cpp g_features；菜单 featNum=0 基列表下标，无 Category 占位）
enum CwjhFeat : int {
    CWJH_FEAT_REG_HP = 0,  // Toggle_超级回血（回复 tick ×2，己方）
    CWJH_FEAT_REG_SP = 1,  // Toggle_超级回蓝（回复 tick ×2，己方）
    CWJH_FEAT_CHC    = 2,  // Toggle_高暴击率（暴击率 +40pp，己方攻击方）
    CWJH_FEAT_CHD    = 3,  // Toggle_高暴击伤害（原暴伤倍率再 ×2，己方攻击方）
    CWJH_FEAT_DEF    = 4,  // Toggle_高防御（护甲减伤 AMR +15pp，己方受击方）
    CWJH_FEAT_RES    = 5,  // Toggle_高抗性（抗性净值 ×2，己方受击方）
    CWJH_FEAT_DODGE  = 6,  // Toggle_高闪避（闪避 +15pp，己方受击方）
    CWJH_FEAT_NO_AD  = 7,  // Toggle_免广告
    CWJH_FEAT_SECKILL = 8, // Toggle_秒杀（封号）——20260909 用户要求加回，菜单尾部署
    CWJH_FEAT_INV     = 9, // Toggle_无敌（封号）——doDmg 伤害=HP+1 / 受击归零
};
#define CWJH_FEAT_COUNT 10

bool Cwjh_TryInit(JavaVM* jvm);
/** 尽早轮询挂 se::evalString（JNI_OnLoad 起独立线程调用） */
void Cwjh_StartEarlyHookThread(JavaVM* jvm);
bool Cwjh_IsReady();
void Cwjh_OnFeatureChanged(int featNum, bool enabled);
void Cwjh_Tick(const bool* features);
void Cwjh_ForceClear();

#endif // STARRY_CWJH_MOD_H
