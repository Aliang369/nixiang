/* ================================================================
 *  星光菜单游戏模块 — 圣翼传说（com.aiyue.wscq）
 *
 *  目标库：libil2cpp.so（Unity IL2CPP，7.6.06 / versionCode 7606）
 *  锚点来源：7.6.06 原版包 libil2cpp.so md5 53b3363b559d85d3e8975f384bfc009b
 *            当前只使用 4 个实际挂点；历史超集见 7.6.06 锚点 JSON
 *  抓点档案：10_docs/圣翼传说/斌哥改包抓点_20260924.md
 *           10_docs/圣翼传说/斌哥hook锚点_20260924.json
 *
 *  ⚠️ 防闪退设施已内置为代码，删任何一节都可能复现历史闪退：
 *    默认 0 hook（懒装）/ 双路径挂点 / RVA 漂移拒绝 / 幂等安装
 *    orig 必调 / reenter 防递归 / 类名缓存 / Tick 轻量
 * ================================================================ */
#ifndef STARRY_WSCQ_MOD_H
#define STARRY_WSCQ_MOD_H

#include <jni.h>

// 功能位号：必须与 WscqEntry.cpp 的 featureList 数组下标一一对应
enum WscqFeat : int {
    WSCQ_FEAT_DMG     = 1,  // SeekBar_伤害倍率_1_1000_1    （1 = 关）
    WSCQ_FEAT_INV     = 2,  // Toggle_玩家无敌
    WSCQ_FEAT_AOE     = 3,  // Toggle_全屏攻击
};

// 签名与 ModEntry.h 回调一致（Cocos 系需要 vm 挂 evalString；Unity 系忽略）
bool Wscq_TryInit(JavaVM* vm);
bool Wscq_IsReady();
void Wscq_OnFeatureChanged(int featNum, bool enabled);
void Wscq_OnValueChanged(int featNum, int value);
void Wscq_Tick(const bool* features, const int* values);
void Wscq_ForceClear();

#endif  // STARRY_WSCQ_MOD_H
