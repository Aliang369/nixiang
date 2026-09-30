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
#ifndef STARRY_SLCB_MOD_H
#define STARRY_SLCB_MOD_H

#include <jni.h>

// 功能位号：必须与 SlcbEntry.cpp 的 featureList 数组下标一一对应
enum SlcbFeat : int {
    SLCB_FEAT_INV   = 1,  // Toggle_无敌
    SLCB_FEAT_DMG   = 2,  // Slider_伤害倍率:1:100:1（1 = 原版）
    SLCB_FEAT_SKILL = 3,  // Toggle_无限技能
    SLCB_FEAT_GOLD  = 5,  // Toggle_无限金币（局内金币）
    SLCB_FEAT_SOUL  = 6,  // Toggle_无限魂石（第二货币 = Tera_OB.BagSystem.m_soul）

    SLCB_FEAT_NOAD  = 7,  // Toggle_免广告（拦截 OpenAdSDK.ShowAd → 直接触发 OnXxxAdFinished）
    // 注：Java 侧按 features[featNum] 取规格（ModBridge.resolveSpec），
    //     所以「免广告」挪到「资源」组后位号必须跟着由 8 变成 7。
};

// 签名与 ModEntry.h 回调一致（Cocos 系需要 vm 挂 evalString；Unity 系忽略）
bool Slcb_TryInit(JavaVM* vm);
bool Slcb_IsReady();
void Slcb_OnFeatureChanged(int featNum, bool enabled);
void Slcb_OnValueChanged(int featNum, int value);
void Slcb_Tick(const bool* features, const int* values);
void Slcb_ForceClear();

#endif  // STARRY_SLCB_MOD_H
