/* ================================================================
 *  霓虹深渊 (com.shangyoo.neon) — Unity IL2CPP 3.2.860 / vc104
 *  feat_num = featureList 数组下标（含 Category 占位）
 *
 *  两套菜单，由 -DNEON_FULL_MENU=1（NEON_VARIANT=dz 定制版）编译期选择：
 *    · 正式版（默认，8 项）：1 无敌 / 2 秒杀 / 3 技能无CD / 4 移速 /
 *      5 金币使用不减 / 6 钥匙使用不减 / 7 彩色钥匙使用不减 / 8 无限跳跃
 *    · 定制版（12 项）：1 无敌 / 2 秒杀 / 3 技能无CD / 4 移速 / 5 攻速 /
 *      6 金币使用不减 / 7 钥匙使用不减 / 8 彩色钥匙使用不减 / 9 无限跳跃 /
 *      10 伤害倍率 / 11 丝血无敌 / 12 全屏攻击
 *
 *  两套编号不同（攻速占 5、资源项整体后移），所以 enum 必须随变体切换；
 *  代码只按枚举名引用，切换由编译期完成。
 *
 *  签名与 ModEntry.h 回调一致；legacy Main.cpp 用默认参数仍可编译
 *  （Main.cpp 禁改，仅声明 Neon_IsReady/Neon_ForceClear，无签名冲突）
 * ================================================================ */
#ifndef STARRY_NEON_MOD_H
#define STARRY_NEON_MOD_H

#include <jni.h>

#ifdef NEON_FULL_MENU
enum NeonFeat : int {
    NEON_FEAT_INVINCIBLE = 1,  // Toggle_无敌
    NEON_FEAT_SECKILL    = 2,  // Toggle_秒杀
    NEON_FEAT_NO_CD      = 3,  // Toggle_技能无CD
    NEON_FEAT_MOVE_SPEED = 4,  // Slider_移速:1:5
    NEON_FEAT_ATTACK_SPEED = 5,// Slider_攻速:1:5
    NEON_FEAT_INF_COIN   = 6,  // Toggle_金币使用不减
    NEON_FEAT_INF_KEY    = 7,  // Toggle_钥匙使用不减
    NEON_FEAT_INF_GKEY   = 8,  // Toggle_彩色钥匙使用不减
    NEON_FEAT_INF_JUMP   = 9,  // Toggle_无限跳跃
    NEON_FEAT_DAMAGE_MUL = 10, // Slider_伤害倍率:1:50
    NEON_FEAT_LAST_HEART = 11, // Toggle_丝血无敌
    NEON_FEAT_FULLSCREEN_ATK = 12, // Toggle_全屏攻击
};
#else
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
#endif

bool Neon_TryInit(JavaVM* vm = nullptr);
bool Neon_IsReady();
void Neon_OnFeatureChanged(int featNum, bool enabled);
void Neon_OnValueChanged(int featNum, int value);
void Neon_Tick(const bool* features, const int* values = nullptr);
void Neon_ForceClear();

#endif // STARRY_NEON_MOD_H
