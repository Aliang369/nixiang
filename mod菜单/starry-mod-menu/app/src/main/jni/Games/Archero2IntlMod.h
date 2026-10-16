/* ================================================================
 *  弓箭传说2国际版 (com.facai.archeroii) — 1.0.1（game_id=gjc2intl）
 *  与国服 gjc2 独立项目；HybridCLR HotFix.BattleLogic
 *
 *  2026-10-17 迁 modern 注册表路线（Gjc2IntlEntry.cpp + Android_gjc2intl.mk）：
 *    菜单 3 → 5 项（新增 秒杀 / 战斗加速）；
 *    同日按用户指定**移除「攻速倍率」**（战斗加速已涵盖攻速），
 *    共享 Main.cpp / GameConfig.h / 公共 Android.mk 零改动。
 * ================================================================ */
#ifndef STARRY_ARCHERO2_INTL_MOD_H
#define STARRY_ARCHERO2_INTL_MOD_H

// 功能位号：必须与 Gjc2IntlEntry.cpp 的 featureList 数组下标一一对应
enum Gjc2IntlFeat : int {
    GJC2INTL_FEAT_STEAL_SKILL  = 1,  // Toggle_国际_偷全部技能
    GJC2INTL_FEAT_INVIS        = 2,  // Toggle_国际_玩家隐身
    GJC2INTL_FEAT_FULLSCREEN   = 3,  // Toggle_国际_全屏攻击
    GJC2INTL_FEAT_ONESHOT      = 4,  // Toggle_国际_秒杀
    GJC2INTL_FEAT_BATTLE_SPEED = 5,  // Slider_国际_战斗加速:1:10（1 = 关）
    // 攻速倍率（原 5 号位）已于 2026-10-17 按用户指定移除：战斗加速已涵盖攻速
};

bool Gjc2Intl_TryInit();
bool Gjc2Intl_IsReady();
void Gjc2Intl_OnFeatureChanged(int featNum, bool enabled);
void Gjc2Intl_OnValueChanged(int featNum, int value);
void Gjc2Intl_Tick(const bool* features, const int* values);
void Gjc2Intl_ForceClear();

#endif  // STARRY_ARCHERO2_INTL_MOD_H
