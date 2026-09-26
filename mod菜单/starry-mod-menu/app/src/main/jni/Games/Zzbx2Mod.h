/* ================================================================
 *  正中靶心2 (com.openew.monster) — Cocos2d-x JS 模块
 *  注入：se::ScriptEngine::evalString 主脚本末尾 splice
 *  功能：攻击倍率 / 解锁月卡特权（含战斗加速 + 广告跳过）
 * ================================================================ */
#ifndef STARRY_ZZBX2_MOD_H
#define STARRY_ZZBX2_MOD_H

#include <jni.h>

// 功能位号：必须与 Zzbx2Entry.cpp 的 featureList 数组下标一一对应。
// 2026-09-26 精简：原 2 号「无敌」、原 4 号「免广告」已删除，
// 月卡特权由 3 号前移至 2 号（菜单现为 2 项：1 攻击倍率 / 2 解锁月卡特权）。
// VIP 位已改为 Toggle（走 RET），SPEED_MIN/MAX 常量与 Java 侧 setVal 同步调用
// 仅为兼容旧链路保留，不再有实际滑块。
// Java 侧 Zzbx2Hooks 的 RET[8]/VAL[8] 仍留旧槽位，不再被查询。
enum Zzbx2Feat : int {
    ZZBX2_FEAT_ATK = 1,
    ZZBX2_FEAT_VIP = 2,
    ZZBX2_FEAT_COUNT = 3,
    ZZBX2_ATK_MIN = 1,
    ZZBX2_ATK_MAX = 10,
    ZZBX2_ATK_DEFAULT = 1,
    ZZBX2_SPEED_MIN = 1,
    ZZBX2_SPEED_MAX = 5,
    ZZBX2_SPEED_DEFAULT = 1,
};

// 签名与 ModEntry.h 回调一致。
bool Zzbx2_TryInit(JavaVM* vm);
void Zzbx2_StartEarlyHookThread(JavaVM* vm);
bool Zzbx2_IsReady();
void Zzbx2_OnFeatureChanged(int featNum, bool enabled);
void Zzbx2_OnValueChanged(int featNum, int value);
void Zzbx2_Tick(const bool* features, const int* values);
void Zzbx2_ForceClear();

#endif  // STARRY_ZZBX2_MOD_H
