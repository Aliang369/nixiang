/* ================================================================
 *  CzbhzEntry — 《城主别慌张》注册表入口（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  只填一张 StarryGameMod 表（菜单 / 初始化参数 / 回调接线）。
 *  铁律：MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 零修改。
 *
 *  本作特殊性（2026-09-11 定案）：H5 盒子游戏，无原生游戏库。
 *    - waitLib = nullptr  → 框架「纯壳不等待」分支，即刻 tryInit
 *    - 无 onLoad 过检补丁   （客户端未发现完整性/反调试校验）
 *    - tickIntervalMs 1s    → Tick 只做开关同步，轻量
 *  功能主体在 assets/starry/czbhz_patch.js（页面内 JS 补丁）。
 * ================================================================ */
#include "ModEntry.h"
#include "CzbhzMod.h"

// featNum = 下标（Category_ 占位），与 CzbhzMod.h 的 enum 严格对齐。
// 滑块格式（FeatureListParser）：SeekBar_名_min_max[_default]，名不得含下划线。
static const char* kCzbhz_Features[] = {
    "Category_基础功能",              // 0
    "Toggle_免广告",                  // 1  CZBHZ_FEAT_AD
    "Category_战斗",                  // 2
    "Toggle_无敌",                    // 3  CZBHZ_FEAT_INV
    "SeekBar_伤害倍率秒杀_1_1000_50",  // 4  CZBHZ_FEAT_DMG
    "SeekBar_移速_1_10_1",            // 5  CZBHZ_FEAT_SPD
    "Toggle_无限技能",                // 6  CZBHZ_FEAT_SKILL
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "czbhz",
        .authGameId = "czbhz",          // 后台 games.game_id（需运营侧注册后发卡）
        .featureList = kCzbhz_Features,
        .featureCount = (int)(sizeof(kCzbhz_Features) / sizeof(kCzbhz_Features[0])),

        .waitLib = nullptr,             // 无原生游戏库：纯壳不等待
        .initDelayMs = 0,               // 无可等目标，立即 init
        .initRetryMax = 3,              // 纯壳恒成功；留 3 次兜底
        .initRetryIntervalMs = 500000,
        .tickIntervalMs = 1000000,         // Tick 只同步开关，1s 足够

        .onLoad = nullptr,              // 无过检补丁需求（客户端未发现完整性校验）
        .tryInit = Czbhz_TryInit,
        .isReady = Czbhz_IsReady,
        .tick = Czbhz_Tick,
        .onFeatureChanged = Czbhz_OnFeatureChanged,
        .onValueChanged = Czbhz_OnValueChanged,
        .onInputChanged = nullptr,      // 无文本输入项
        .forceClear = Czbhz_ForceClear,
        .onApplyPlayPatch = nullptr,    // 无 OTA 偏移热更
    };
    return &mod;
}
