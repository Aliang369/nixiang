/* ================================================================
 *  我的技能无限触发（com.SmallMill.EnMaster 1.6.3）星光模块
 *
 *  1:1 对照：斌哥改包菜单（10_docs/_batch_binge6/六款斌哥改包_抓点总表_20261102.md §二.6）
 *    无敌 / 伤害倍率 / 加速 / 金币 / 钻石 / 体力
 *  官方底包与斌哥包 libil2cpp.so、global-metadata.dat **逐字节一致**
 *  （md5 4406aaaf… / 9289669c…）⇒ 抓点 RVA 直接可用。
 *
 *  实现分层：
 *    行为型 → Dobby hook：BattleData::changeHp（无敌 + 伤害倍率）、UnityEngine.Time::set_timeScale（加速）
 *    数据型 → 直调：Game.dataMgr(静态字段) → DataMgr.nativeData(0x10) → changePlayerGold/Crystal/Power
 *            （三者内部是 CodeStage ObscuredInt，必须走 changePlayer*，禁止直写 int）
 *
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 * ================================================================ */
#ifndef STARRY_WDJNWCF_MOD_H
#define STARRY_WDJNWCF_MOD_H

#include <jni.h>

// 功能位号：必须与 WdjnwcfEntry.cpp 的 featureList 数组下标一一对应
enum WdjnwcfFeat : int {
    WDJNWCF_FEAT_CAT        = 0,   // Category_功能
    WDJNWCF_FEAT_INVINCIBLE = 1,   // Toggle_无敌
    WDJNWCF_FEAT_DMG_MULT   = 2,   // InputValue_100:伤害倍率（1 = 关）
    WDJNWCF_FEAT_SPEED      = 3,   // InputValue_10:加速（1 = 关）
    WDJNWCF_FEAT_GOLD       = 4,   // Button_金币
    WDJNWCF_FEAT_CRYSTAL    = 5,   // Button_钻石
    WDJNWCF_FEAT_POWER      = 6,   // Button_体力
    WDJNWCF_FEAT_NOAD       = 7,   // Toggle_免广告（用户 2026-11-02 点名；斌哥未做）
};

bool Wdjnwcf_TryInit(JavaVM* vm);
bool Wdjnwcf_IsReady();
void Wdjnwcf_OnFeatureChanged(int featNum, bool enabled);
void Wdjnwcf_OnValueChanged(int featNum, int value);
void Wdjnwcf_OnInputChanged(int featNum, const char* text);
void Wdjnwcf_Tick(const bool* features, const int* values);
void Wdjnwcf_ForceClear();

#endif  // STARRY_WDJNWCF_MOD_H
