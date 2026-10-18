/* ================================================================
 *  WdyzEntry.cpp — 我的勇者 7.6.3（com.rsg.wdyz / 热更 build 357）注册表入口
 *
 *  Unity IL2CPP（libil2cpp.so，运行时加载 files/update/current 热更 so）
 *
 *  菜单（数组下标 = featNum，与 WdyzMod.h 的 WdyzFeat 对齐）：
 *    0  Category_战斗
 *    1  Toggle_无敌        WDYZ_FEAT_INV
 *    2  Toggle_五倍攻击    WDYZ_FEAT_DMG
 *    3  Toggle_无限技能    WDYZ_FEAT_SKILL
 *    4  Slider_伤害倍率:1:50  WDYZ_FEAT_DMGX（1=关闭，真实伤害穿盾）
 *
 *  初始化节奏（沿用 legacy Main.cpp 的 wdyz 策略）：
 *    initRetryMax=120 × initRetryIntervalMs=2000000μs(2s) —— 等 libil2cpp 热更 so 就位。
 *
 *  铁律：改菜单/初始化节奏/回调只改本文件；
 *        MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 永不需要动。
 * ================================================================ */

#include "ModEntry.h"
#include "WdyzMod.h"

static const char* kWdyz_Features[] = {
    "Category_战斗",        // 0（占位）
    "Toggle_无敌",          // 1  WDYZ_FEAT_INV
    "Toggle_五倍攻击",      // 2  WDYZ_FEAT_DMG
    "Toggle_无限技能",      // 3  WDYZ_FEAT_SKILL
    "Slider_伤害倍率:1:50", // 4  WDYZ_FEAT_DMGX
};

static bool WdyzEntry_TryInit(JavaVM* /*vm*/) { return Wdyz_TryInit(); }

/** 框架 Tick 合同带 values；wdyz 前四项为开关，第 5 项为伤害倍率 Slider。 */
static void WdyzEntry_Tick(const bool* features, const int* values) {
    Wdyz_Tick(features, values);
}

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "wdyz",
        .authGameId = "wdyz",           // 我的勇者（com.rsg.wdyz，沿用）
        .featureList = kWdyz_Features,
        .featureCount = (int)(sizeof(kWdyz_Features) / sizeof(kWdyz_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 0,
        .initRetryMax = 120,            // 等热更 libil2cpp.so 就位
        .initRetryIntervalMs = 2000000, // 2s（微秒）
        .tickIntervalMs = 200000,       // 200ms（微秒）

        .onLoad = nullptr,
        .tryInit = WdyzEntry_TryInit,
        .isReady = Wdyz_IsReady,
        .tick = WdyzEntry_Tick,
        .onFeatureChanged = Wdyz_OnFeatureChanged,
        .onValueChanged = Wdyz_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Wdyz_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
