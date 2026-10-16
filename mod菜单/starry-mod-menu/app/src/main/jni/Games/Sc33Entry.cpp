/* ================================================================
 *  生存33天 (com.lgcb.scsst.sc33ttaptapwz37zgx3.dayssurvive) — 注册表入口
 *
 *  Cocos Creator JS / libcocos2djs.so。
 *  native 只 hook se::ScriptEngine::evalString，向运行时明文 JS 拼接
 *  JMYBTActivity.get_ret 门控；功能状态由 com.starry.magic.cocos.Sc33Hooks 读取。
 *
 *  迁移说明（2026-10-17）：本作原走 legacy 入口（公共 Android.mk 激活段 +
 *  Main.cpp 的 `#elif defined(STARRY_GAME_SC33)` 菜单表）。公共 Android.mk 头部
 *  告示与铁律：Main.cpp 用 __LINE__ 做 OBFUSCATE 混淆种子，行号漂移即产物漂移，
 *  严禁改动（含注释）—— 所以改菜单必须迁注册表。
 *  迁移 = 本文件 + Android_sc33.mk，Main.cpp / MainModern.cpp / GameConfig.h /
 *  公共 Android.mk 零接触；构建改走 APP_BUILD_SCRIPT=./Android_sc33.mk。
 * ================================================================ */
#include "ModEntry.h"
#include "Sc33Mod.h"

/* 菜单（下标 = featNum；Category_ 占下标，仅供 UI 分组）
 * 2026-10-17 用户要求「菜单分类保留一个」：
 *   原 spec = Category_战斗(0) / 百年伤害(1) / 无敌(2) / 全屏秒杀(3) / 攻速(4)
 *             / Category_其它(5) / 免广告(6)
 *   现 spec = 单分类「战斗」，免广告并入其后 → 免广告下标 6 → 5
 *   （Sc33Mod.h 的 SC33_FEAT_NO_AD 同步改 5；JS 侧 get_ret 编号不变） */
static const char* kSc33_Features[] = {
    "Category_战斗",        // 0
    "Toggle_百倍伤害",      // 1  SC33_FEAT_DMG100
    "Toggle_无敌",          // 2  SC33_FEAT_INVINCIBLE
    "Toggle_全屏秒杀",      // 3  SC33_FEAT_AOE_KILL
    "Slider_攻速:1:10",     // 4  SC33_FEAT_ATK_SPEED  1=关 2..10=倍率
    "Toggle_免广告",        // 5  SC33_FEAT_NO_AD
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "sc33",
        .authGameId = "sc33",   // 与 GameConfig.h STARRY_AUTH_GAME_ID 一致
        .featureList = kSc33_Features,
        .featureCount = (int)(sizeof(kSc33_Features) / sizeof(kSc33_Features[0])),

        .waitLib = "libcocos2djs.so",
        .initDelayMs = 0,                // 微秒；early hook 线程已在 JNI_OnLoad 起
        .initRetryMax = 90,              // legacy = 90 × 1s 轮询
        .initRetryIntervalMs = 1000000,  // 微秒 = 1s
        .tickIntervalMs = 500000,        // 微秒 = 500ms（legacy usleep(500000)）

        .onLoad = Sc33_OnLoad,           // JNI_OnLoad：起 early hook 线程挂 evalString
        .tryInit = Sc33_TryInit,
        .isReady = Sc33_IsReady,
        .tick = Sc33_TickModern,         // legacy tick 语义（Tick + 攻速续刷）
        .onFeatureChanged = Sc33_OnFeatureChanged,
        .onValueChanged = Sc33_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Sc33_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
