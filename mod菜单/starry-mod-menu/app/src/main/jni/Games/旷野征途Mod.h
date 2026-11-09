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
#ifndef STARRY_旷野征途_MOD_H
#define STARRY_旷野征途_MOD_H

#include <jni.h>

// 功能位号：必须与 {Game}Entry.cpp 的 featureList 数组下标一一对应
enum 旷野征途Feat : int {
    旷野征途_FEAT_GOLD = 1,     // Button_金币+9999
    旷野征途_FEAT_DIAMOND = 2,  // Button_钻石+9999
    旷野征途_FEAT_POINT = 3,    // Button_积分+9999
    旷野征途_FEAT_SPD = 5,      // Toggle_游戏加速
    旷野征途_FEAT_INV = 6,      // Toggle_玩家无敌
    旷野征途_FEAT_DMG = 7,      // Toggle_百倍伤害
    旷野征途_FEAT_NOAD = 8,     // Toggle_免广告
};

// 签名与 ModEntry.h 回调一致（Cocos 系需要 vm 挂 evalString；Unity 系忽略）
bool 旷野征途_TryInit(JavaVM* vm);
bool 旷野征途_IsReady();
void 旷野征途_OnFeatureChanged(int featNum, bool enabled);
void 旷野征途_OnValueChanged(int featNum, int value);
void 旷野征途_Tick(const bool* features, const int* values);
void 旷野征途_ForceClear();

#endif  // STARRY_旷野征途_MOD_H
