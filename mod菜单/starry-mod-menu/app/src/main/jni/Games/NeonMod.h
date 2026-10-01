/* ================================================================
 *  霓虹深渊 (com.shangyoo.neon) — Unity IL2CPP 3.2.860 / vc104
 *  feat_num = featureList 数组下标（含 Category 占位）
 *
 *  2026-10-02 迁移 modern 注册表（NeonEntry.cpp + Android_neon.mk），
 *  新增对局内资源 ⑦⑧⑨，并把原「近战攻速/远程攻速」合并为单一 ⑤ 攻速
 *  （第四轮，用户要求：不区分近远，一把滑条把所有玩家攻击节奏调快）：
 *    ⑤ 攻速 → ① GunOutput.get_FireDelay(60/RPM) ② 枪图协程 d__45/d__46 的 <timer>
 *             ③ DeputyWeaponSlot_MeleeMultiSeg.set_BusyTime（多段近战＝梅那把刀的挥砍 CD）
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
    NEON_FEAT_ATTACK_SPEED = 5,  // Slider_攻速:1:5（统一近战/远程，不分流）
    NEON_FEAT_INF_COIN     = 6,  // Toggle_金币使用不减（NEONPlayerState.UseCoin/HasCoin）
    NEON_FEAT_INF_KEY      = 7,  // Toggle_钥匙使用不减（NEONPlayerState.UseKey/HasKey）
    NEON_FEAT_INF_GKEY     = 8,  // Toggle_彩色钥匙使用不减（NEONPlayerState.Use/HasGameplayKey）
};

bool Neon_TryInit(JavaVM* vm = nullptr);
bool Neon_IsReady();
void Neon_OnFeatureChanged(int featNum, bool enabled);
void Neon_OnValueChanged(int featNum, int value);
void Neon_Tick(const bool* features, const int* values = nullptr);
void Neon_ForceClear();

#endif // STARRY_NEON_MOD_H
