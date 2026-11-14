/* ================================================================
 *  汉克旅店 1.1.0 (vc337, com.chillyroom.hanksbar.yw) — 星空版 native 模块
 *
 *  引擎：Unity IL2CPP + HybridCLR 热更。玩法 100% 在热更程序集 Assembly-CSharp.dll，
 *  libil2cpp.so 里没有它们的机器码 ⇒ 不在玩法方法上 hook，改为：
 *    native 在**程序集加载边界**（System.Reflection.Assembly.Load(byte[])，AOT 有真地址、
 *    运行时按名解析）把官方 Assembly-CSharp.dll 字节换成 Cecil 补丁版；
 *    补丁版 IL 从 starry_config.ini 读菜单开关键（HybridCLR 解释器调 native 不可靠，
 *    文件镜像是本仓 HybridCLR 游戏既有口径：无限升级 / 境界死神激斗）。
 *
 *  配置键（与 04_scripts/汉克旅店/ilpatch/Patch.cs 逐字一致）：改一处必须改两处
 *    god / damage_x / inf_skill / unlock_char / inf_money / inf_key / move_speed_x / inf_mp
 * ================================================================ */
#ifndef STARRY_HKLD_MOD_H
#define STARRY_HKLD_MOD_H

#include <jni.h>

// 功能位号 = HkldEntry.cpp 的 featureList 下标（Category_ 占下标）
enum HkldFeat : int {
    HKLD_FEAT_GOD     = 1,  // Toggle_无敌      -> god
    HKLD_FEAT_DMGX    = 2,  // Slider_伤害倍率:1:100 -> damage_x
    HKLD_FEAT_SKILL   = 3,  // Toggle_无限技能  -> inf_skill
    HKLD_FEAT_UNLOCK  = 4,  // Toggle_解锁角色  -> unlock_char
    HKLD_FEAT_MONEY   = 5,  // Toggle_无限金币  -> inf_money
    HKLD_FEAT_KEY     = 6,  // Toggle_无限钥匙  -> inf_key
    HKLD_FEAT_MOVESPD = 7,  // Slider_移动速度:100:300 -> move_speed_x（整数百分比，100 = 原速）
    HKLD_FEAT_INFMP   = 8,  // Toggle_无限蓝    -> inf_mp（1 = 蓝条不掉）
};

bool Hkld_TryInit(JavaVM* vm);
bool Hkld_IsReady();
void Hkld_Tick(const bool* features, const int* values);
void Hkld_OnFeatureChanged(int featNum, bool enabled);
void Hkld_OnValueChanged(int featNum, int value);
void Hkld_ForceClear();

#endif  // STARRY_HKLD_MOD_H
