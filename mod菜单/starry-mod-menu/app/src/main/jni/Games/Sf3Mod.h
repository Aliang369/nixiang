/* ================================================================
 *  星光菜单游戏模块 — 暗影格斗3（com.wanda.sf3.wd / TapTap 1.45.3）
 *
 *  四项功能（featureList 下标 = featNum，见 Sf3Entry.cpp）：
 *    1 Slider_伤害倍率:1:10:1  IKHOBFONGIE.OGFLOMPOMCI 伤害容器 ×倍率（斌哥同款入口）
 *    2 Toggle_上帝模式         同入口，玩家受击伤害清零（斌哥同款入口）
 *    3 Slider_战斗速度:1:10:1  UnityEngine.Time.set_timeScale
 *    4 Toggle_无限体力         IGCAMALALAJ 体力 API
 *
 *  2026-09-19：无限货币已按用户实测（假 UI 无效）删除，不再实现。
 *
 *  防闪退设施（骨架强制，勿删）：
 *    默认 0 hook（懒装）/ 双路径挂点 / RVA 漂移拒绝 / 幂等安装
 *    orig 必调 / reenter 防递归 / 类指针运行时校验 / Tick 轻量
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 * ================================================================ */
#ifndef STARRY_SF3_MOD_H
#define STARRY_SF3_MOD_H

#include <jni.h>

// 功能位号：必须与 Sf3Entry.cpp 的 featureList 数组下标一一对应
enum Sf3Feat : int {
    SF3_FEAT_DMG   = 1,  // Slider_伤害倍率:1:10:1（1=原伤害，2..10=倍率）
    SF3_FEAT_GOD   = 2,  // Toggle_上帝模式
    SF3_FEAT_SPEED = 3,  // Slider_战斗速度:1:10:1（1=原速，2..10=倍率）
    SF3_FEAT_ENERGY = 4, // Toggle_无限体力
};

// 签名与 ModEntry.h 回调一致（Unity 系忽略 vm）
bool Sf3_TryInit(JavaVM* vm);
bool Sf3_IsReady();
void Sf3_OnFeatureChanged(int featNum, bool enabled);
void Sf3_OnValueChanged(int featNum, int value);
void Sf3_Tick(const bool* features, const int* values);
void Sf3_ForceClear();

#endif  // STARRY_SF3_MOD_H
