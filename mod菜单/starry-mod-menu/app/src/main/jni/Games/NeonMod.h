/* ================================================================
 *  霓虹深渊 (com.shangyoo.neon) — Unity IL2CPP 3.2.860 / vc104
 *  feat_num = featureList 数组下标（含 Category 占位）
 *
 *  2026-11-26 正式版收敛（第八轮）：菜单从 12 项砍到 8 项，删除
 *  【攻速】【伤害倍率】【丝血无敌】【全屏攻击】（用户当轮指定）。
 *  删除方式＝摘掉开关通道与挂载点（InstallHooks 不再安装对应钩子），
 *  保留编号连续：1 无敌 / 2 秒杀 / 3 技能无CD / 4 移速 /
 *  5 金币使用不减 / 6 钥匙使用不减 / 7 彩色钥匙使用不减 / 8 无限跳跃
 *
 *  签名与 ModEntry.h 回调一致；legacy Main.cpp 用默认参数仍可编译
 *  （Main.cpp 禁改，仅声明 Neon_IsReady/Neon_ForceClear，无签名冲突）
 * ================================================================ */
#ifndef STARRY_NEON_MOD_H
#define STARRY_NEON_MOD_H

#include <jni.h>

enum NeonFeat : int {
    NEON_FEAT_INVINCIBLE = 1,  // Toggle_无敌（Hearts.DeductHP 直接返回 false）
    NEON_FEAT_SECKILL    = 2,  // Toggle_秒杀（伤害放大 + 目标 CurrentHp 归零）
    NEON_FEAT_NO_CD      = 3,  // Toggle_技能无CD（35 处 CanUseSkill 全挂）
    NEON_FEAT_MOVE_SPEED = 4,  // Slider_移速:1:5（1 = 关）
    NEON_FEAT_INF_COIN   = 5,  // Toggle_金币使用不减（UseCoin 拦截 + HasCoin 恒真）
    NEON_FEAT_INF_KEY    = 6,  // Toggle_钥匙使用不减（UseKey 拦截 + HasKey 恒真）
    NEON_FEAT_INF_GKEY   = 7,  // Toggle_彩色钥匙使用不减（GameplayKey）
    NEON_FEAT_INF_JUMP   = 8,  // Toggle_无限跳跃（get_multipleJumpCount 返回 -1000）
};

bool Neon_TryInit(JavaVM* vm = nullptr);
bool Neon_IsReady();
void Neon_OnFeatureChanged(int featNum, bool enabled);
void Neon_OnValueChanged(int featNum, int value);
void Neon_Tick(const bool* features, const int* values = nullptr);
void Neon_ForceClear();

#endif // STARRY_NEON_MOD_H
