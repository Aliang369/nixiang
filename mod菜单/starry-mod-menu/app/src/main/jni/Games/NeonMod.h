/* ================================================================
 *  霓虹深渊 (com.shangyoo.neon) — Unity IL2CPP 3.2.860 / vc104
 *  feat_num = featureList 数组下标（含 Category 占位）
 *
 *  2026-10-02 迁移 modern 注册表（NeonEntry.cpp + Android_neon.mk），
 *  并新增两项武器攻速、三项对局内资源（2026-10-02 第二轮）：
 *    ⑤ 近战武器攻速 → Gun.<MeleeCheck>d__85::MoveNext 的 <timer>（挥砍 CD）
 *    ⑥ 远程武器攻速 → Gun.<shootingInterval>d__96::MoveNext 的 WaitForSeconds
 *
 *  签名与 ModEntry.h 回调一致；legacy Main.cpp 用默认参数仍可编译
 *  （Main.cpp 禁改，仅声明 Neon_IsReady/Neon_ForceClear，无签名冲突）
 * ================================================================ */
#ifndef STARRY_NEON_MOD_H
#define STARRY_NEON_MOD_H

#include <jni.h>

enum NeonFeat : int {
    NEON_FEAT_INVINCIBLE   = 1,  // Toggle_无敌
    NEON_FEAT_SECKILL      = 2,  // Toggle_秒杀
    NEON_FEAT_NO_CD        = 3,  // Toggle_技能无CD
    NEON_FEAT_MOVE_SPEED   = 4,  // Slider_移速:1:5
    NEON_FEAT_MELEE_SPEED  = 5,  // Slider_近战攻速:1:5（刀/球棒/拳套/指虎）
    NEON_FEAT_RANGED_SPEED = 6,  // Slider_远程攻速:1:5（枪/弓）
    NEON_FEAT_INF_COIN     = 7,  // Toggle_无限金币（NEONPlayerState.UseCoin/HasCoin）
    NEON_FEAT_INF_KEY      = 8,  // Toggle_无限钥匙（NEONPlayerState.UseKey/HasKey）
    NEON_FEAT_INF_GKEY     = 9,  // Toggle_无限彩色钥匙（NEONPlayerState.Use/HasGameplayKey）
};

bool Neon_TryInit(JavaVM* vm = nullptr);
bool Neon_IsReady();
void Neon_OnFeatureChanged(int featNum, bool enabled);
void Neon_OnValueChanged(int featNum, int value);
void Neon_Tick(const bool* features, const int* values = nullptr);
void Neon_ForceClear();

#endif // STARRY_NEON_MOD_H
