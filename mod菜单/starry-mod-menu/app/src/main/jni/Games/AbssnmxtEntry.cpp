/* ================================================================
 *  AbssnmxtEntry — 阿比斯少年冒险团 2.36 (vc236, TapTap) 注册表入口
 *
 *  自主生成（new_game_mod.sh Abssnmxt）+ 手工填 TODO：
 *    authGameId = abssnmxt（全名首字母：阿a 比b 斯s 少s 年n 冒m 险x 团t）
 *    waitLib    = libil2cpp.so（Unity 2022.3.62f2 IL2CPP）
 *  MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 均不改。
 *
 *  单位提醒（ModEntry.h）：initDelayMs / initRetryIntervalMs / tickIntervalMs
 *  语义是**微秒**（框架走 usleep）。故 200ms = 200000。
 * ================================================================ */
#include "ModEntry.h"
#include "AbssnmxtMod.h"

// 菜单项 = 用户点名四项，一项不多（AGENTS §4 功能边界）
// 下标 = featNum，Category_ 占下标 0。
static const char* kAbssnmxt_Features[] = {
    "Category_战斗强化",              // 0（占下标，仅 UI 分组）
    "Toggle_无敌",                    // 1  ABSSNMXT_FEAT_GOD    -> god
    "Slider_伤害倍率:1:100",           // 2  ABSSNMXT_FEAT_DMGX   -> damage_x（1 = 关闭）
    "Toggle_无限技能",                // 3  ABSSNMXT_FEAT_SKILL  -> inf_skill
    "Slider_移动速度:1:10",            // 4  ABSSNMXT_FEAT_SPEED  -> speed_x（1 = 关闭）
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Abssnmxt",
        .authGameId = "abssnmxt",       // 阿比斯少年冒险团 -> 全名首字母
        .featureList = kAbssnmxt_Features,
        .featureCount = (int)(sizeof(kAbssnmxt_Features) / sizeof(kAbssnmxt_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 200000,          // 200ms（微秒）：避开 il2cpp 半初始化窗口
        .initRetryMax = 600,            // 600 x 100ms = 60s 预算（大包首启解析慢）
        .initRetryIntervalMs = 100000,  // 100ms（微秒）
        .tickIntervalMs = 200000,       // 200ms（微秒）

        .onLoad = nullptr,              // 无 EarlyHook / 过检补丁需求
        .tryInit = Abssnmxt_TryInit,
        .isReady = Abssnmxt_IsReady,
        .tick = Abssnmxt_Tick,
        .onFeatureChanged = Abssnmxt_OnFeatureChanged,
        .onValueChanged = Abssnmxt_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Abssnmxt_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
