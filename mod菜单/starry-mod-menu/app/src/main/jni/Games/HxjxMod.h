/* ================================================================
 *  星光菜单游戏模块 — 标准骨架 v3.1（注册表版，复制后按 TODO 填空）
 *
 *  生成：bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh {Game}
 *  产出四件套：{Game}Mod.h/.cpp + {Game}Entry.cpp + Android_{tag}.mk
 *  入口：MainModern.cpp（注册表模式；Main.cpp/GameConfig.h/公共 Android.mk 均零修改）
 *
 *  ⚠️ 本作 0 个 hook：无敌 = 热更程序集 HotFix.dll 的内存 IL 字节补丁（见 .cpp §③）。
 *    内置防闪退设施（删任一条都可能复现历史闪退）：只扫 rw 段 / 逐命中 PE 校验 /
 *    sigsetjmp 保护区 + sigaltstack / 幂等写入 / 读回校验 / Tick 轻量。
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 * ================================================================ */
#ifndef STARRY_HXJX_MOD_H
#define STARRY_HXJX_MOD_H

#include <jni.h>

// 功能位号：必须与 HxjxEntry.cpp 的 featureList 数组下标一一对应
//   0 = Category_战斗（占位，无回调）
//   1 = Toggle_无敌  → HXJX_FEAT_INV（HotFix.dll 内存 IL 补丁，见 Mod.cpp §③）
//       补丁点：**A + A' 两处同时写**（真机验证：只留 A 仍掉血）
//         A  EnemyAttackHit 的 hp -= dmg ⇒ 0
//         A' UnitUpdate._changeHpMax 的 if (hp > newMax) hp = newMax ⇒ 无条件 hp = newMax
enum HxjxFeat : int {
    HXJX_FEAT_INV = 1,
};

// 签名与 ModEntry.h 回调一致（Cocos 系需要 vm 挂 evalString；Unity 系忽略）
bool Hxjx_TryInit(JavaVM* vm);
bool Hxjx_IsReady();
void Hxjx_OnFeatureChanged(int featNum, bool enabled);
void Hxjx_OnValueChanged(int featNum, int value);
void Hxjx_Tick(const bool* features, const int* values);
void Hxjx_ForceClear();

#endif  // STARRY_HXJX_MOD_H
