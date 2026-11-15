/* ================================================================
 *  星光菜单游戏模块 — 阿比斯少年冒险团（abssnmxt）2.36 / vc236 / TapTap
 *
 *  生成：new_game_mod.sh Abssnmxt（骨架 v3.1）→ 填 TODO
 *  引擎：Unity 2022.3.62f2 IL2CPP（lib/arm64-v8a/libil2cpp.so + armeabi-v7a）
 *
 *  ⚠️ 防闪退设施是代码不是约定，删任何一节都可能复现历史闪退：
 *    默认 0 hook（懒装）/ 双路径挂点 / RVA 漂移拒绝 / 幂等安装
 *    orig 必调 / reenter 防递归 / SafeWrite 校验写 / 类名缓存 / Tick 轻量
 *    + 并发装钩互斥 / 致命信号兜底（sigsetjmp+altstack）/ 槽位熔断
 *
 *  功能（用户点名四项，一项不多）：无敌 / 伤害倍率 1-100 / 无限技能 / 移动速度 1-10
 *  A 阶段：先出「空功能」so 打通构建链（挂点表为空，等 02_锚点与挂点方案.md）
 * ================================================================ */
#ifndef STARRY_ABSSNMXT_MOD_H
#define STARRY_ABSSNMXT_MOD_H

#include <jni.h>

// 功能位号：必须与 AbssnmxtEntry.cpp 的 featureList 数组下标一一对应
enum AbssnmxtFeat : int {
    ABSSNMXT_FEAT_GOD   = 1,  // Toggle_无敌
    ABSSNMXT_FEAT_DMGX  = 2,  // Slider_伤害倍率:1:100（1 = 关闭）
    ABSSNMXT_FEAT_SKILL = 3,  // Toggle_无限技能
    ABSSNMXT_FEAT_SPEED = 4,  // Slider_移动速度:1:10（1 = 关闭）
};

// 签名与 ModEntry.h 回调一致（Unity 系忽略 vm，保留形参以对齐契约）
bool Abssnmxt_TryInit(JavaVM* vm);
bool Abssnmxt_IsReady();
void Abssnmxt_OnFeatureChanged(int featNum, bool enabled);
void Abssnmxt_OnValueChanged(int featNum, int value);
void Abssnmxt_Tick(const bool* features, const int* values);
void Abssnmxt_ForceClear();

#endif  // STARRY_ABSSNMXT_MOD_H
