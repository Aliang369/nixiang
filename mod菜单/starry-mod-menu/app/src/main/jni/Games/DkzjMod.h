/* ================================================================
 *  蛋壳战机 1.1.7 (com.survivor.acecn) — 星光 Mod
 *
 *  功能：
 *    1 Slider_攻击倍率:1:100:1  EntityCharacter.ChangeHp
 *    2 Toggle_上帝模式           EntityHero.ChangeHp 标记 + 基类拦截
 *    3 Toggle_偷盗技能           BattleWorldContext.PostProcessLearnedSkill
 *    4 Toggle_免广告             ADModuleMgr.CheckAndPlayVideo
 * ================================================================ */
#ifndef STARRY_DKZJ_MOD_H
#define STARRY_DKZJ_MOD_H

#include <jni.h>

enum DkzjFeat : int {
    DKZJ_FEAT_ATTACK_MULT = 1,
    DKZJ_FEAT_GOD = 2,
    DKZJ_FEAT_STEAL_SKILL = 3,
    DKZJ_FEAT_NO_ADS = 4,
};

bool Dkzj_TryInit(JavaVM* vm);
bool Dkzj_IsReady();
void Dkzj_OnFeatureChanged(int featNum, bool enabled);
void Dkzj_OnValueChanged(int featNum, int value);
void Dkzj_Tick(const bool* features, const int* values);
void Dkzj_ForceClear();

#endif  // STARRY_DKZJ_MOD_H
