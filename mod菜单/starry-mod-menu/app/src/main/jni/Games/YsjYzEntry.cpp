/* ================================================================
 *  YsjYzEntry.cpp — 异世界勇者 (com.mudamuda.idlewow) 注册表入口
 *
 *  Cocos Creator 2.4.2 / libcocos2djs.so + assets/main/index.jsc，
 *  se::ScriptEngine::evalString 运行时拼接 JS 补丁块。
 *
 *  菜单（数组下标 = featNum）—— 用户要求只保留**一个**分类，5 项功能平铺：
 *    0  Category_基础功能
 *    1  Toggle_免广告          挂 cc.UIHelper.ShowAD → 直接回调（含"快速挂机免费"）
 *    2  Toggle_无限技能        合并项：GetSkillCDDuration=0 + UpdateSkillCD 强制充满
 *                              + GetSkillCost=0（原本的"零CD"与"不耗蓝"两项）
 *    3  Toggle_无敌            AddHp 我方受击 → 不结算
 *    4  SeekBar_伤害倍率       1-100，默认 1（=关）
 *    5  SeekBar_游戏倍速       1-5，默认 1（=关）
 *
 *  版本沿革：v7 删除「吸血拉满」（用户要求）；伤害倍率上限 1000→100、默认 50→1；
 *            游戏倍速默认 2→1。因 featNum 重编号，Java 侧偏好由 ModBridge 每次
 *            初始化 clearFeaturePrefs() 清空，无旧值继承问题。
 *
 *  ⚠️ 上报一致性设计：
 *    本包**没有任何"改面板"的功能**，所有战斗增益都落在 AddHp 结算层，因此
 *    result 的 Hp/Atk/Def/Free/Php/Patk/Pdef 天然与官包一致，无需 CheckResult /
 *    Patk / Pdef 伪装。唯一照看的是 Dam：
 *      - 伤害倍率在 AddHp 放大，StatDam 按同系数 1:1 折回（同步调用链，精确）；
 *      - 另加 Mhp×100 的**宽松兜底限幅**（Dam 是整章累计、Mhp 只是当前怪，
 *        故比值≈本章怪数，用 10 倍会压低正常战报）。
 * ================================================================
 */

#include "ModEntry.h"
#include "YsjYzMod.h"

static void YsjYz_OnLoad(JavaVM* vm) { YsjYz_StartEarlyHookThread(vm); }

static const char* kYsjYzFeatures[] = {
    "Category_基础功能",          // 0（唯一分类）
    "Toggle_免广告",              // 1 YSJYZ_FEAT_ADFREE
    "Toggle_无限技能",            // 2 YSJYZ_FEAT_SKILL（零CD + 不耗蓝，合并项）
    "Toggle_无敌",                // 3 YSJYZ_FEAT_INV
    "SeekBar_伤害倍率_1_100_1",   // 4 YSJYZ_FEAT_DMG（v7：1-100，默认 1）
    "SeekBar_游戏倍速_1_5_1",     // 5 YSJYZ_FEAT_GSPD（v7：默认 1）
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "ysjyz",
        .authGameId = "ysjyz",
        .featureList = kYsjYzFeatures,
        .featureCount = (int)(sizeof(kYsjYzFeatures) / sizeof(kYsjYzFeatures[0])),

        .waitLib = "libcocos2djs.so",
        .initDelayMs = 0,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = YsjYz_OnLoad,
        .tryInit = YsjYz_TryInit,
        .isReady = YsjYz_IsReady,
        .tick = YsjYz_Tick,
        .onFeatureChanged = YsjYz_OnFeatureChanged,
        .onValueChanged = YsjYz_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = YsjYz_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
