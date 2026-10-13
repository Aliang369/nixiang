/* ================================================================
 *  蛋壳战机 1.1.8 (com.survivor.acecn) — 星光 Mod
 *
 *  功能（★ = 本轮按卡密包挂点重做；依据 03_卡密包挂点还原与1.1.8映射_20261014.md）：
 *    1 Slider_攻击倍率:1:100:1  ★ HurtBase.FinalizeAttackResult（FP 攻击结算 ×N；
 *                                挂点未上线时退回 EntityCharacter.ChangeHp 兜底）
 *    2 Toggle_上帝模式           EntityHero.ChangeHp + ★ EntityHero.set_CurrentHp
 *    3 Toggle_偷盗技能           BattleWorldContext.PostProcessLearnedSkill
 *    4 Toggle_免广告             ★ GameAdsManager.CheckAndPlayVideo（框架层 4 参委托直调）
 *                                + ADModuleMgr.CheckAndPlayVideo（旧层兜底）
 *    （拦截上报 BattleRecordManager.IsNeedUploadSaveData 改为**常开注入**，无菜单开关）
 *    【2026-10-14 用户 m00790】「立即胜利」功能已**整体删除**：
 *      卡密包侧该功能早被其作者停用（三个 nativeTriggerWin 都是 0x2C 日志桩；
 *      字符串「一键通关已停用（按钮保留，不做任何事）」），星光版自建的波次驱动
 *      + BattleWorldContext.SetWinPlayerId 路线也一并撤掉，不留死代码。
 * ================================================================ */
#ifndef STARRY_DKZJ_MOD_H
#define STARRY_DKZJ_MOD_H

#include <jni.h>

enum DkzjFeat : int {
    DKZJ_FEAT_ATTACK_MULT = 1,
    DKZJ_FEAT_GOD = 2,
    DKZJ_FEAT_STEAL_SKILL = 3,
    DKZJ_FEAT_NO_ADS = 4,
    // DKZJ_FEAT_INSTANT_WIN = 5 已移除（用户 m00790：立即胜利删除）
    // DKZJ_FEAT_BLOCK_REPORT 已移除：拦截上报是常开注入，不是菜单项
};

bool Dkzj_TryInit(JavaVM* vm);
bool Dkzj_IsReady();
void Dkzj_OnFeatureChanged(int featNum, bool enabled);
void Dkzj_OnValueChanged(int featNum, int value);
void Dkzj_Tick(const bool* features, const int* values);
void Dkzj_ForceClear();

#endif  // STARRY_DKZJ_MOD_H
