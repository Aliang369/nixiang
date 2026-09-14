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
#ifndef STARRY_YNTT_MOD_H
#define STARRY_YNTT_MOD_H

#include <jni.h>

// 功能位号：必须与 {Game}Entry.cpp 的 featureList 数组下标一一对应
enum YnttFeat : int {
    YNTT_FEAT_KILL     = 1,  // Toggle_秒杀
    YNTT_FEAT_INV      = 2,  // Toggle_无敌
    YNTT_FEAT_SPEED    = 3,  // Toggle_战斗加速
    YNTT_FEAT_STAMINA  = 4,  // Toggle_无限体力
    YNTT_FEAT_CURRENCY = 5,  // Toggle_无限货币
    YNTT_FEAT_NOADS    = 6,  // Toggle_免广告
};

// 签名与 ModEntry.h 回调一致（Cocos 系需要 vm 挂 evalString；Unity 系忽略）
bool Yntt_TryInit(JavaVM* vm);
bool Yntt_IsReady();
void Yntt_OnFeatureChanged(int featNum, bool enabled);
void Yntt_OnValueChanged(int featNum, int value);
void Yntt_Tick(const bool* features, const int* values);
void Yntt_ForceClear();

#endif  // STARRY_YNTT_MOD_H
