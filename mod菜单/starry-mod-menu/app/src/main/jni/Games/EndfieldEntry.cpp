/* ================================================================
 *  明日方舟：终末地 — modern 注册表入口
 *
 *  移植来源：legacy `Main.cpp` 的 `#elif defined(STARRY_GAME_ENDFIELD)` 段
 *  （该段按铁律保留不动，只作为对照；本文件是等价的 modern 表达）。
 *  对照点（逐项照抄，避免行为漂移）：
 *    · 菜单 5 项 = Category_战斗 + 四功能（featNum = 数组下标，Category 占 0）
 *    · 启动：sleep(2) → 120 次 × 2s 重试 Endfield_TryInit()
 *    · Tick：300ms 节流，`if (!ready) TryInit(); if (ready) Tick(g_features);`
 *    · ForceClear：dead session / 授权失效时关全部 hook 效果
 *  游戏逻辑在 Games/EndfieldMod.cpp（v14.3 四功能，按名解析，本轮零改动）。
 *  抓点：10_docs/终末地/抓点_四功能_20260902.md
 * ================================================================ */
#include "ModEntry.h"
#include "EndfieldMod.h"
#include "Includes/SecStrings.hpp"

static const char* kEndfield_Features[] = {
    "Category_战斗",        // 0
    "Toggle_百倍伤害",      // 1  ENDFIELD_FEAT_DMG100
    "Toggle_无敌",          // 2  ENDFIELD_FEAT_INV
    "Toggle_无限跳跃",      // 3  ENDFIELD_FEAT_JUMP
    "Toggle_无限技能",      // 4  ENDFIELD_FEAT_SKILL
};

/* legacy 回调签名 → modern 合同（差异只有 vm / values 两个入参） */
static bool Entry_TryInit(JavaVM* vm) {
    (void)vm;                     // Unity 系不需要 JVM 句柄（与 legacy 调用一致）
    return Endfield_TryInit();
}

static void Entry_Tick(const bool* features, const int* values) {
    (void)values;                 // 本游戏无滑块项：legacy 也只传 g_features
    Endfield_Tick(features);
}

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "mrfzzmd",
        .authGameId = "mrfzzmd",   // = GameConfig.h STARRY_GAME_ENDFIELD 映射值
        .featureList = kEndfield_Features,
        .featureCount = (int)(sizeof(kEndfield_Features) / sizeof(kEndfield_Features[0])),

        .waitLib = sec_lib_il2cpp(),
        .initDelayMs = 2000,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000,
        .tickIntervalMs = 300,

        .onLoad = nullptr,
        .tryInit = Entry_TryInit,
        .isReady = Endfield_IsReady,
        .tick = Entry_Tick,
        .onFeatureChanged = Endfield_OnFeatureChanged,
        .onValueChanged = Endfield_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Endfield_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
