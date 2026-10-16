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
        // v14.6（FW-20）：本字段在 MainModern.cpp:230 走 `usleep(initDelayMs)` ⇒ **单位是微秒**。
        // 原值 2000 = 2ms（意图 2s）⇒ 首次触碰 il2cpp 几乎与库加载同拍，必然早于
        // `il2cpp_init` 完成 ⇒ mod 线程进 GC「未知线程」毒态 ⇒ UnityMain 触发 GC 即 abort。
        // 现改为 0：延迟交给 `EndfieldMod.cpp` 的 il2cpp_init 就绪门（真信号，非盲等），
        // 越早挂门越能抢在 UnityMain 调 il2cpp_init 之前。
        .initDelayMs = 0,
        // v14.6（FW-20）：本字段同样走 `MainModern.cpp:248/262` 的 `usleep(tickInterval)` ⇒
        // **单位是微秒**（框架默认值 200000 即 200ms，可对照）。原值 300 = 0.3ms ⇒ 实测
        // **~2300 次 tick/秒**（precheck_20261016_212826：479 条门日志 / 4.1s）：
        //   ① `SyncHooks` 的退避节拍按 **tick 计数**设计（`tickNo + (1<<shift)*7`，本意 2.1s）
        //      在 0.3ms 节拍下退化为 ~67ms ⇒ v14.4 的指数退避**实际失效**（FW-18 的高频重解析风险复发）；
        //   ② 每 20 拍一条门日志 ⇒ ~130 条/秒的日志噪声。
        // 改为 300000（=300ms，与文件头「Tick：300ms 节流」的原意一致）。
        .tickIntervalMs = 300000,
        .initRetryMax = 120,
        // v14.6：同走 `usleep(initRetryIntervalMs)` ⇒ 单位是微秒。原值 2000 = 2ms（意图 2s）。
        // 本作 `TryInit` 库加载即返回 true（重试循环首轮即 break）⇒ 现无实际影响，
        // 但保持单位与框架一致，避免日后 TryInit 改成延迟返回时静默退化。
        .initRetryIntervalMs = 2000000,

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
