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
#ifndef STARRY_GGBH_MOD_H
#define STARRY_GGBH_MOD_H

#include <jni.h>

/* 功能位号：必须与 GgbhEntry.cpp 的 featureList 数组下标一一对应
 * 功能集只有这两项（用户点名，硬约束 #1：禁止扩张）：
 *   GGBH_FEAT_GMMENU — GM 菜单：hook UISwitchPlayerMenu.get_IsCheatModeEnabled（读侧恒真）
 *   GGBH_FEAT_SPEED  — 全局加速：hook Time.get_deltaTime / Time.set_timeScale（每帧钉 timeScale）
 * 对照方（斌哥改包）机制全文见 10_docs/鬼谷八荒/路线锁定.md 与 斌哥包抓点_20261004.md §五。 */
enum GgbhFeat : int {
    GGBH_FEAT_GMMENU = 1,  // Toggle_GM菜单
    GGBH_FEAT_SPEED  = 2,  // SeekBar_加速倍率_1_20_1（1 = 关）
};

// 签名与 ModEntry.h 回调一致（Cocos 系需要 vm 挂 evalString；Unity 系忽略）
bool Ggbh_TryInit(JavaVM* vm);
bool Ggbh_IsReady();
void Ggbh_OnFeatureChanged(int featNum, bool enabled);
void Ggbh_OnValueChanged(int featNum, int value);
void Ggbh_Tick(const bool* features, const int* values);
void Ggbh_ForceClear();

#endif  // STARRY_GGBH_MOD_H
