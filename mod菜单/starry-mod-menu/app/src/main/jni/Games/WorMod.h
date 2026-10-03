/* ================================================================
 *  星光菜单游戏模块 — 标准骨架 v3（注册表版）
 *
 *  本作：境界守望者（Watcher of Realms / 国际服 com.ap.watcherofrealms 1.6.38.829.1）
 *  目标库：liblogic.so（不是 libil2cpp.so —— 包内那份是 7z 解压壳，见 .cpp 说明）
 *  母本：Games/CxswzMod.h（国服潮汐守望者 com.gf.cxswz，同产品线同架构，逐段对照改编）
 *  阶段：e1 环境阶段（菜单能弹出 + SignSpoof 回灌官证 + 能正常进游戏；0 hook）
 *  产出四件套：{Game}Mod.h/.cpp + {Game}Entry.cpp + Android_{tag}.mk
 *  入口：MainModern.cpp（注册表模式；Main.cpp/GameConfig.h/公共 Android.mk 均零修改）
 *
 *  ⚠️ 防闪退设施已内置为代码，删任何一节都可能复现历史闪退：
 *    默认 0 hook（懒装）/ 双路径挂点 / RVA 漂移拒绝 / 幂等安装
 *    orig 必调 / reenter 防递归 / SafeWrite 校验写 / 类名缓存 / Tick 轻量
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 * ================================================================ */
#ifndef STARRY_WOR_MOD_H
#define STARRY_WOR_MOD_H

#include <jni.h>

// 功能位号：必须与 WorEntry.cpp 的 featureList 数组下标一一对应
enum WorFeat : int {
    WOR_FEAT_INV = 1,  // Toggle_无敌（环境阶段占位，未实现）
    WOR_FEAT_DMG = 2,  // Slider_伤害倍率（环境阶段占位，未实现；1 = 关闭）
};

// 签名与 ModEntry.h 回调一致（Cocos 系需要 vm 挂 evalString；Unity 系忽略）
bool Wor_TryInit(JavaVM* vm);
bool Wor_IsReady();
void Wor_OnFeatureChanged(int featNum, bool enabled);
void Wor_OnValueChanged(int featNum, int value);
void Wor_Tick(const bool* features, const int* values);
void Wor_ForceClear();

#endif  // STARRY_WOR_MOD_H
