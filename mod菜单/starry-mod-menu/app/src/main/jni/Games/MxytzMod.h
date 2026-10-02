/* ================================================================
 *  星光菜单游戏模块 — 标准骨架 v3（注册表版，复制后按 TODO 填空）
 *
 *  生成：bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh {Game}
 *  产出四件套：{Game}Mod.h/.cpp + {Game}Entry.cpp + Android_{tag}.mk
 *  入口：MainModern.cpp（注册表模式；Main.cpp/GameConfig.h/公共 Android.mk 均零修改）
 *
 *  ⚠️ 防闪退设施已内置为代码，删任何一节都可能复现历史闪退：
 *    默认 0 hook（懒装）/ 双路径挂点 / RVA 漂移拒绝 / 幂等安装
 *    orig 必调 / reenter 防递归 / SafeWrite 校验写 / 类名缓存 / Tick 轻量
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 * ================================================================ */
#ifndef STARRY_MXYTZ_MOD_H
#define STARRY_MXYTZ_MOD_H

#include <jni.h>

// 功能位号：必须与 MxytzEntry.cpp 的 featureList 数组下标一一对应
enum MxytzFeat : int {
    MXYTZ_FEAT_CAT_BATTLE  = 0,  // Category_战斗（占位）
    MXYTZ_FEAT_KILL_MOB    = 1,  // Toggle_秒杀小怪（玩家侧伤害放大 → 小怪一击必杀）
    MXYTZ_FEAT_INVINCIBLE  = 2,  // Toggle_无敌（玩家受击判定作废）
    MXYTZ_FEAT_SKILL_DMG   = 3,  // Toggle_全模式技能伤害增加（全模式伤害/技能伤害放大）
    MXYTZ_FEAT_NO_AD       = 4,  // Toggle_免广告（广告入口 → 直接走发奖，不播视频）
    MXYTZ_FEAT_COUNT       = 5,
};

// 签名与 ModEntry.h 回调一致（Cocos 系需要 vm 挂 evalString；Unity 系忽略）
bool Mxytz_TryInit(JavaVM* vm);
bool Mxytz_IsReady();
void Mxytz_OnFeatureChanged(int featNum, bool enabled);
void Mxytz_OnValueChanged(int featNum, int value);
void Mxytz_Tick(const bool* features, const int* values);
void Mxytz_ForceClear();

#endif  // STARRY_MXYTZ_MOD_H
