/* ================================================================
 *  Gjc2IntlEntry.cpp — 弓箭传说2国际版 (com.facai.archeroii) 注册表入口
 *
 *  Unity IL2CPP + HybridCLR 热更（HotFix.BattleLogic）
 *
 *  菜单（数组下标 = featNum，与 Archero2IntlMod.h 的 Gjc2IntlFeat 对齐）：
 *    0  Category_国际版战斗
 *    1  Toggle_国际_偷全部技能    GJC2INTL_FEAT_STEAL_SKILL
 *    2  Toggle_国际_玩家隐身      GJC2INTL_FEAT_INVIS
 *    3  Toggle_国际_全屏攻击      GJC2INTL_FEAT_FULLSCREEN
 *    4  Toggle_国际_秒杀          GJC2INTL_FEAT_ONESHOT
 *    5  Slider_国际_战斗加速      GJC2INTL_FEAT_BATTLE_SPEED（1 = 关）
 *
 *  攻速倍率（原 5 号位滑块）已按用户 2026-10-17 指定移除：战斗加速已涵盖攻速。
 *  移除后各下标整体前移，本文件数组下标 = featNum，与 Archero2IntlMod.h 的枚举严格对齐。
 *
 *  初始化节奏（沿用 8-23 已验证的 gjc2 系列策略）：
 *    initRetryMax = 0 → 跳过启动期轮询，改由 Tick 懒装（继续局恢复窗口里
 *    后台扫 HotFix/Il2Cpp 会与主线程死锁卡死）。
 *
 *  铁律：改菜单/初始化节奏/回调只改本文件；
 *        MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 永不需要动。
 * ================================================================ */

#include "ModEntry.h"
#include "Archero2IntlMod.h"

static const char* kGjc2Intl_Features[] = {
    "Category_国际版战斗",           // 0（占位）
    "Toggle_国际_偷全部技能",        // 1  GJC2INTL_FEAT_STEAL_SKILL
    "Toggle_国际_玩家隐身",          // 2  GJC2INTL_FEAT_INVIS
    "Toggle_国际_全屏攻击",          // 3  GJC2INTL_FEAT_FULLSCREEN
    "Toggle_国际_秒杀",              // 4  GJC2INTL_FEAT_ONESHOT
    "Slider_国际_战斗加速:1:10",     // 5  GJC2INTL_FEAT_BATTLE_SPEED（1 = 关）
};

static bool Gjc2IntlEntry_TryInit(JavaVM* /*vm*/) { return Gjc2Intl_TryInit(); }

/** 框架 Tick 合同带 values：战斗加速为滑块，值经此下发。 */
static void Gjc2IntlEntry_Tick(const bool* features, const int* values) {
    Gjc2Intl_Tick(features, values);
}

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "gjc2intl",
        .authGameId = "gjc2intl",       // 国际版独立卡密，勿与国服 gjc2 共用
        .featureList = kGjc2Intl_Features,
        .featureCount = (int)(sizeof(kGjc2Intl_Features) / sizeof(kGjc2Intl_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 0,
        .initRetryMax = 0,              // 禁止启动期 init（继续局死锁案）
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,       // 200ms

        .onLoad = nullptr,
        .tryInit = Gjc2IntlEntry_TryInit,
        .isReady = Gjc2Intl_IsReady,
        .tick = Gjc2IntlEntry_Tick,
        .onFeatureChanged = Gjc2Intl_OnFeatureChanged,
        .onValueChanged = Gjc2Intl_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Gjc2Intl_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
