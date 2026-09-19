/* ================================================================
 *  Gjc2Entry.cpp — 弓箭传说2 国服 (com.xq.archeroii.ua / TapTap 1.1.9) 注册表入口
 *
 *  Unity IL2CPP + HybridCLR 热更（HotFix.BattleLogic）
 *
 *  菜单（数组下标 = featNum，与 Archero2Mod.h 的 Gjc2Feat 对齐）：
 *    0  Category_战斗
 *    1  Toggle_拿全部技能    GJC2_FEAT_STEAL_SKILL
 *    2  Toggle_玩家隐身      GJC2_FEAT_INVIS
 *    3  Toggle_全屏攻击      GJC2_FEAT_FULLSCREEN
 *
 *  初始化节奏（沿用 legacy Main.cpp 的 gjc2 专用策略）：
 *    initRetryMax = 0 → 跳过启动期轮询。8-30 实锤：继续关卡恢复窗口里
 *    后台扫 HotFix/Il2Cpp 会与主线程死锁卡死；改为用户打开功能后 Tick 懒初始化。
 *
 *  铁律：改菜单/初始化节奏/回调只改本文件；
 *        MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 永不需要动。
 * ================================================================ */

#include "ModEntry.h"
#include "Archero2Mod.h"

static const char* kGjc2_Features[] = {
    "Category_战斗",       // 0（占位）
    "Toggle_拿全部技能",   // 1  GJC2_FEAT_STEAL_SKILL
    "Toggle_玩家隐身",     // 2  GJC2_FEAT_INVIS
    "Toggle_全屏攻击",     // 3  GJC2_FEAT_FULLSCREEN
};

static bool Gjc2Entry_TryInit(JavaVM* /*vm*/) { return Gjc2_TryInit(); }

/** 框架 Tick 合同带 values；gjc2 三项均为开关，无滑块值。 */
static void Gjc2Entry_Tick(const bool* features, const int* /*values*/) {
    Gjc2_Tick(features);
}

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "gjc2",
        .authGameId = "gjc2",           // 弓箭传说2 国服（8-17 起沿用，未变）
        .featureList = kGjc2_Features,
        .featureCount = (int)(sizeof(kGjc2_Features) / sizeof(kGjc2_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 0,
        .initRetryMax = 0,              // 禁止启动期 init（继续局死锁案）
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,       // 200ms

        .onLoad = nullptr,
        .tryInit = Gjc2Entry_TryInit,
        .isReady = Gjc2_IsReady,
        .tick = Gjc2Entry_Tick,
        .onFeatureChanged = Gjc2_OnFeatureChanged,
        .onValueChanged = Gjc2_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Gjc2_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
