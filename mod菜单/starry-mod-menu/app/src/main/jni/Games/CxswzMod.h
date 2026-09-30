/* ================================================================
 *  星光菜单游戏模块 — 标准骨架 v3（注册表版，复制后按 TODO 填空）
 *
 *  本作：潮汐守望者（com.gf.cxswz / TapTap 官包 1.2.56.375.3）
 *  目标库：liblogic.so（不是 libil2cpp.so —— 包内那份是 7z 解压壳，见 .cpp 说明）
 *  生成：bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh Cxswz
 *  产出四件套：{Game}Mod.h/.cpp + {Game}Entry.cpp + Android_{tag}.mk
 *  入口：MainModern.cpp（注册表模式；Main.cpp/GameConfig.h/公共 Android.mk 均零修改）
 *
 *  ⚠️ 防闪退设施已内置为代码，删任何一节都可能复现历史闪退：
 *    默认 0 hook（懒装）/ 双路径挂点 / RVA 漂移拒绝 / 幂等安装
 *    orig 必调 / reenter 防递归 / SafeWrite 校验写 / 类名缓存 / Tick 轻量
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 * ================================================================ */
#ifndef STARRY_CXSWZ_MOD_H
#define STARRY_CXSWZ_MOD_H

#include <jni.h>

// 功能位号：必须与 CxswzEntry.cpp 的 featureList 数组下标一一对应
enum CxswzFeat : int {
    CXSWZ_FEAT_INV = 1,  // Toggle_无敌（环境阶段占位，未实现）
    CXSWZ_FEAT_DMG = 2,  // Slider_伤害倍率（环境阶段占位，未实现；1 = 关闭）
};

// 签名与 ModEntry.h 回调一致（Cocos 系需要 vm 挂 evalString；Unity 系忽略）
bool Cxswz_TryInit(JavaVM* vm);
bool Cxswz_IsReady();
void Cxswz_OnFeatureChanged(int featNum, bool enabled);
void Cxswz_OnValueChanged(int featNum, int value);
void Cxswz_Tick(const bool* features, const int* values);
void Cxswz_ForceClear();

#endif  // STARRY_CXSWZ_MOD_H
