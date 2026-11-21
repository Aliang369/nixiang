/* ================================================================
 *  冒险战争 (com.mxzz.zbml) — 星空版 native 模块（骨架）
 *
 *  引擎：Unity IL2CPP + HybridCLR 热更。玩法在热更程序集里（解释执行），
 *  libil2cpp.so 中没有它们的机器码 ⇒ 不在玩法方法上 hook，改为：
 *    native 在**程序集加载边界**（System.Reflection.Assembly.Load(byte[])，AOT 有真地址、
 *    运行时按名解析）把官方程序集字节换成 Cecil 补丁版；
 *    补丁版 IL 从 starry_config.ini 读菜单开关键
 *    （口径同 汉克旅店 HkldMod.cpp；差异见下方「与 Hkld 的差别」）。
 *
 *  ★ 与 Hkld 的差别（Lead 2026-11-19 指示）：
 *    版本辨识用 **sha256 白名单**，而不是长度比对。热更新版本变化频繁，
 *    长度相同而内容不同的换版用长度比不出来（旧 b8 的 f44f75b8/18,655,744 B 已过期）。
 *    白名单为空 ⇒ 一律 PASSTHROUGH（fail-safe，不崩不闪退）。
 *
 *  ★ 静态结论门禁（用户 2026-11-19 硬要求）：
 *    本文件只承载「加载边界机制」；补丁 IL 语义与白名单取值来自
 *    10_docs/冒险战争/分析报告_复刻_*.md（静态分析 + 代码反推验证），
 *    由 04_scripts/冒险战争/gen_mxzz_payload_header.py 生成 Games/MxzzPayload.h。
 *    未拿到静态结论前，白名单为空、payload 长度 0 ⇒ 本模块只会 PASSTHROUGH。
 *
 *  配置键（与 04_scripts/冒险战争/ilpatch/Patch.cs 逐字一致）：god / kill
 * ================================================================ */
#ifndef STARRY_MXZZ_MOD_H
#define STARRY_MXZZ_MOD_H

#include <jni.h>

// 功能位号 = MxzzEntry.cpp 的 featureList 下标
enum MxzzFeat : int {
    MXZZ_FEAT_KILL = 1,   // Toggle_秒杀 -> kill
    MXZZ_FEAT_GOD  = 2,   // Toggle_无敌 -> god
};

bool Mxzz_TryInit(JavaVM* vm);
bool Mxzz_IsReady();
void Mxzz_Tick(const bool* features, const int* values);
void Mxzz_OnFeatureChanged(int featNum, bool enabled);
void Mxzz_OnValueChanged(int featNum, int value);
void Mxzz_ForceClear();

#endif  // STARRY_MXZZ_MOD_H
