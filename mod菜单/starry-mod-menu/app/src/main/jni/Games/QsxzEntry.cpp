/* ================================================================
 *  QsxzEntry — 《炁术行者》注册表入口（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  只填一张 StarryGameMod 表（菜单 / 初始化参数 / 回调接线）。
 *  铁律：MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 零修改。
 *
 *  本作特殊性（2026-10-07 定案）：纯 WebView 壳 + H5(Cocos web-mobile) 游戏，
 *  APK 内无原生游戏库。
 *    - waitLib = nullptr  → 框架「纯壳不等待」分支，即刻 tryInit
 *    - 无 onLoad 过检补丁 （客户端未发现完整性/反调试校验）
 *    - tickIntervalMs 1s  → Tick 只做开关同步，轻量
 *  功能主体在 assets/starry/qsxz_patch.js（页面内 JS 补丁），
 *  经 window.StarryH5.getState() 读菜单状态，live=0 时全功能哑火。
 * ================================================================ */
#include "ModEntry.h"
#include "QsxzMod.h"

// featNum = 下标（Category_ 占位），与 QsxzMod.h 的 enum 严格对齐。
// 滑块格式（FeatureListParser）：SeekBar_名_min_max[_default]，名不得含下划线。
// 默认值一律取「原版口径」（倍率 1、减伤 0）：菜单首次打开不能是拉满状态，
// 否则玩家一进游戏就在用极限值，与「未授权=原版」的设计相悖。
static const char* kQsxz_Features[] = {
    "Category_基础功能",        // 0
    "SeekBar_伤害倍率_1_5_1",   // 1  QSXZ_FEAT_DMG（默认 1 = 原版；2026-10-09 上限 20→5）
    "SeekBar_减伤_0_50_0",      // 2  QSXZ_FEAT_DR（默认 0 = 原版）
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "qsxz",
        .authGameId = "qsxz",           // 后台 games.game_id（运营侧注册后发卡）
        .featureList = kQsxz_Features,
        .featureCount = (int)(sizeof(kQsxz_Features) / sizeof(kQsxz_Features[0])),

        .waitLib = nullptr,             // 无原生游戏库：纯壳不等待
        .initDelayMs = 0,               // 无可等目标，立即 init
        .initRetryMax = 3,              // 纯壳恒成功；留 3 次兜底
        .initRetryIntervalMs = 500,
        .tickIntervalMs = 1000,         // Tick 只同步开关，1s 足够

        .onLoad = nullptr,              // 无过检补丁需求（客户端未发现完整性校验）
        .tryInit = Qsxz_TryInit,
        .isReady = Qsxz_IsReady,
        .tick = Qsxz_Tick,
        .onFeatureChanged = Qsxz_OnFeatureChanged,
        .onValueChanged = Qsxz_OnValueChanged,
        .onInputChanged = nullptr,      // 无文本输入项
        .forceClear = Qsxz_ForceClear,
        .onApplyPlayPatch = nullptr,    // 无 OTA 偏移热更
    };
    return &mod;
}
