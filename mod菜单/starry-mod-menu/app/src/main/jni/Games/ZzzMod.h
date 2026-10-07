/* ================================================================
 *  星光菜单游戏模块 — 绝区零 (com.miHoYo.Nap) 3.2.0 (vc429) arm64-v8a
 *
 *  目标库：libil2cpp.so（Unity IL2CPP；metadata 为 MHY 加密头，Il2CppDumper 不可用）
 *  站点来源：10_docs/绝区零/星空版站点定位_20261005.md
 *            （站点一律用【文件偏移】空间；RVA = 文件偏移 + 0x4000）
 *    无敌        file 0x1151d138  JNCLILAONEF.HLAOJKPGHIO（池鸳同款，反汇编语义已确认）
 *    伤害倍率    file 0xc0269f0   入口 fmov s8,s0 ⇒ s0 即倍率入参（池鸳同款）
 *    移动速度    file 0xfb2700c   函数内 3 条写指令（str s8,[x0,#0xf8]/[+0x100]/strb w8,[x0,#0x104]）
 *    定怪        file 0x19ce2164  BehaviorManager.Tick（唯一调用者 0x1a7f0710）
 *    无限技能    v1 未定稿（候选 0x1391de04 三簇调用者，未确认扣费点）→ 仅保留菜单开关
 *
 *  ⚠️ 防闪退设施已内置为代码，删任何一节都可能复现历史闪退：
 *    默认 0 hook（懒装）/ 序言校验 / 幂等安装 / orig 必调 / reenter 防递归 /
 *    SafeWrite 校验写 / 类名缓存 / Tick 轻量 / 致命信号兜底 + 槽位熔断
 * ================================================================ */
#ifndef STARRY_ZZZ_MOD_H
#define STARRY_ZZZ_MOD_H

#include <jni.h>

// 功能位号：必须与 ZzzEntry.cpp 的 kZzz_Features 数组下标一一对应
enum ZzzFeat : int {
    ZZZ_FEAT_INV    = 0,   // Toggle_无敌
    ZZZ_FEAT_SKILL  = 1,   // Toggle_无限技能（v1 站点未定稿，仅记录开关）
    ZZZ_FEAT_DMG    = 2,   // Slider_伤害倍率:1:100:1  (1 = 关闭) —— 2026-10-07 移速删除后前移
    ZZZ_FEAT_FREEZE = 3,   // Toggle_定怪
};

// Slider 上限（必须与 ZzzEntry.cpp 的 Slider_ 声明一致）
#define ZZZ_DMG_MUL_MAX  100

// 签名与 ModEntry.h 回调一致
bool Zzz_TryInit(JavaVM* vm);
bool Zzz_IsReady();
void Zzz_OnFeatureChanged(int featNum, bool enabled);
void Zzz_OnValueChanged(int featNum, int value);
void Zzz_Tick(const bool* features, const int* values);
void Zzz_ForceClear();

#endif  // STARRY_ZZZ_MOD_H
