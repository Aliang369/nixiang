/* ================================================================
 *  星光菜单游戏模块 — 标准骨架 v3.1（注册表版，复制后按 TODO 填空）
 *
 *  生成：bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh {Game}
 *  产出四件套：{Game}Mod.h/.cpp + {Game}Entry.cpp + Android_{tag}.mk
 *  入口：MainModern.cpp（注册表模式；Main.cpp/GameConfig.h/公共 Android.mk 均零修改）
 *
 *  ⚠️ 防闪退设施已内置为代码，删任何一节都可能复现历史闪退：
 *    默认 0 hook（懒装）/ 双路径挂点 / RVA 漂移拒绝 / 幂等安装
 *    orig 必调 / reenter 防递归 / SafeWrite 校验写 / 类名缓存 / Tick 轻量
 *  v3.1 追加（见 .cpp §④，2026-10-02 疾风骑士 60.0.1 实战下沉）：
 *    并发装钩互斥（g_installMutex + double-check，防同址双 Dobby SIGSEGV）
 *    致命信号兜底（sigsetjmp 保护区 DobbyHookGuarded/ReadU32Guarded/RunGuarded
 *                  + sigaltstack 备用栈 + 链回旧处理器）
 *    槽位熔断（g_slotDead[]：炸过的槽位本次会话不再重试）
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 * ================================================================ */
#ifndef STARRY_SLCB2_MOD_H
#define STARRY_SLCB2_MOD_H

#include <jni.h>

// 功能位号：必须与 {Game}Entry.cpp 的 featureList 数组下标一一对应
enum Slcb2Feat : int {
    SLCB2_FEAT_DMG = 1,      // Toggle_百倍伤害（斌哥桩 RVA 0x110829a0）
    SLCB2_FEAT_INV = 2,      // Toggle_无敌（RVA 0x110649cc）
    SLCB2_FEAT_SKILL = 3,    // Toggle_无限技能（免CD 0x10ce8c24 / 不耗蓝 0x118dd824）
    SLCB2_FEAT_IRON = 5,     // Toggle_无限魔铁锭（LC2.BagSystem::get_IronPowder）
    SLCB2_FEAT_CRYSTAL = 6,  // Toggle_无限魂晶（LC2.BagSystem::get_Crystal）
    SLCB2_FEAT_SKIN = 7,     // Toggle_全装扮（AppearanceMgr.IsAppearanceItemUnlocked 0x11944af8）
    SLCB2_FEAT_NOAD = 8,     // Toggle_免广告（RechargeShopMgr.CheckRewardVideoAvailable 0x113a549c）     // Toggle_全装扮（AppearanceMgr.IsAppearanceItemUnlocked 0x11944af8）
};

// 签名与 ModEntry.h 回调一致（Cocos 系需要 vm 挂 evalString；Unity 系忽略）
bool Slcb2_TryInit(JavaVM* vm);
bool Slcb2_IsReady();
void Slcb2_OnFeatureChanged(int featNum, bool enabled);
void Slcb2_OnValueChanged(int featNum, int value);
void Slcb2_Tick(const bool* features, const int* values);
void Slcb2_ForceClear();
void Slcb2_StartEarlyHook();   // 抢在易盾校验基线之前装钩（由 onLoad 调用）

#endif  // STARRY_SLCB2_MOD_H
