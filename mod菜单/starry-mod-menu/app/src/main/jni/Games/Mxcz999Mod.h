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
#ifndef STARRY_MXCZ999_MOD_H
#define STARRY_MXCZ999_MOD_H

#include <jni.h>

// 功能位号：必须与 Mxcz999Entry.cpp 的 kMxcz999_Features[] 下标逐一相等
//   下标 0 = Category_战斗分类（分类占 1 个下标，无回调），功能从 1 开始
enum Mxcz999Feat : int {
    MXCZ999_FEAT_INVINCIBLE     = 1,  // 无敌（Toggle）
    MXCZ999_FEAT_DMG_MUL        = 2,  // 伤害倍率（Slider 1..100）
    MXCZ999_FEAT_MOVE_SPEED     = 3,  // 移动速度（Slider 1..5）
    MXCZ999_FEAT_UNLOCK_SKIN    = 4,  // 解锁所有皮肤（Toggle）
    MXCZ999_FEAT_UNLOCK_COUPON  = 5,  // 无限解锁券（Toggle）
    MXCZ999_FEAT_NO_AD          = 6,  // 免广告（Toggle）
};

// 签名与 ModEntry.h 回调一致（Cocos 系需要 vm 挂 evalString；Unity 系忽略）
bool Mxcz999_TryInit(JavaVM* vm);
bool Mxcz999_IsReady();
void Mxcz999_OnFeatureChanged(int featNum, bool enabled);
void Mxcz999_OnValueChanged(int featNum, int value);
void Mxcz999_Tick(const bool* features, const int* values);
void Mxcz999_ForceClear();

#endif  // STARRY_MXCZ999_MOD_H
