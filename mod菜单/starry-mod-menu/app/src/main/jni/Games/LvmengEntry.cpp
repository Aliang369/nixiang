/* ================================================================
 *  绿梦：时空之声 — 星光注册表入口（modern）
 *  纯壳：不等待 il2cpp、不装 hook；只注册 5 项并落盘状态文件。
 * ================================================================ */
#include "LvmengMod.h"
#include "ModEntry.h"

static const char* kLvmeng_Features[] = {
    "Category_战斗",                 // 0 分组，占下标
    "InputValue_0:自定义血量",       // 1 默认 0 不生效，输入数字才生效
    "Slider_全局变速:1:10:1",        // 2 1x..10x（UI 显示原始倍率，native 内部 ×100）
    "Slider_伤害倍率:1:1000:1",      // 3 1x..1000x（UI 显示原始倍率，native 内部 ×100）
    "Toggle_无限技能",               // 4
    "Toggle_无限跳跃",               // 5
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Lvmeng",
        .authGameId = "lmskzs",
        .featureList = kLvmeng_Features,
        .featureCount = (int)(sizeof(kLvmeng_Features) / sizeof(kLvmeng_Features[0])),

        .waitLib = nullptr,          // 纯壳：不等游戏库
        .initDelayMs = 0,
        .initRetryMax = 1,
        .initRetryIntervalMs = 200000,
        .tickIntervalMs = 200000,    // 200ms

        .onLoad = nullptr,
        .tryInit = Lvmeng_TryInit,
        .isReady = Lvmeng_IsReady,
        .tick = Lvmeng_Tick,
        .onFeatureChanged = Lvmeng_OnFeatureChanged,
        .onValueChanged = Lvmeng_OnValueChanged,
        .onInputChanged = Lvmeng_OnInputChanged,
        .forceClear = Lvmeng_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
