/* ================================================================
 *  星光菜单游戏模块 — 疯狂水世界（com.gx.sw.qa.fkssj004.esj 1.11.6 / vc480）
 *
 *  引擎：Cocos Creator 3.x + JSB（libcocos.so），无 IL2CPP、无整包加固壳
 *  阶段：① 过环境（用户 m00001「先不要直接做功能，先过环境」）
 *        native = **零 hook 环境版**：只保证菜单组装进游戏、游戏能正常玩。
 *
 *  ⚠️ 防闪退设施已内置为代码，删任何一节都可能复现历史闪退：
 *    默认 0 hook（懒装）/ §④ 致命信号兜底（sigsetjmp 保护区 + sigaltstack
 *    + 装钩互斥 + 槽位熔断）/ Tick 轻量
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 * ================================================================ */
#ifndef STARRY_FKSSJ_MOD_H
#define STARRY_FKSSJ_MOD_H

#include <jni.h>

// 功能位号：必须与 FkssjEntry.cpp 的 featureList 数组下标一一对应
// （Category_ 占下标 0，功能从 1 起）
enum FkssjFeat : int {
    FKSSJ_FEAT_INV = 1,  // Toggle_无敌
    FKSSJ_FEAT_DMG = 2,  // Slider_伤害倍率:1:100:1（1 = 关闭）
    FKSSJ_FEAT_SPD = 3,  // Slider_战斗加速:1:10:1 （1 = 关闭）
};

// 签名与 ModEntry.h 回调一致（Cocos 系可选传 vm 供 JS 桥用；本阶段忽略）
bool Fkssj_TryInit(JavaVM* vm);
bool Fkssj_IsReady();
void Fkssj_OnFeatureChanged(int featNum, bool enabled);
void Fkssj_OnValueChanged(int featNum, int value);
void Fkssj_Tick(const bool* features, const int* values);
void Fkssj_ForceClear();

#endif  // STARRY_FKSSJ_MOD_H
