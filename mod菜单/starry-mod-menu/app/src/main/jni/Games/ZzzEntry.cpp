/* ================================================================
 *  ZzzEntry — 绝区零 注册表入口（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  游戏：绝区零 com.miHoYo.Nap 3.2.0 (vc429)，arm64-v8a，Unity IL2CPP
 *  菜单五项（硬约束 #9，禁扩张）：
 *    0 Toggle_无敌          池鸳同款站点 file 0x1151d138
 *    1 Toggle_无限技能      v1 站点未定稿（只记开关）
 *    2 Slider_移动速度:1:20:1
 *    3 Slider_伤害倍率:1:100:1
 *    4 Toggle_定怪          BehaviorManager.Tick file 0x19ce2164
 *
 *  铁律：改菜单/调初始化节奏只改本文件；featureList 下标 = featNum（与 ZzzMod.h 对齐）；
 *        滑块 1 = 关闭，上限必须与 ZzzMod.h 的 *_MUL_MAX 一致。
 * ================================================================ */
#include "ModEntry.h"
#include "ZzzMod.h"

static const char* kZzz_Features[] = {
    "Toggle_无敌",              // 0  ZZZ_FEAT_INV
    "Toggle_无限技能",          // 1  ZZZ_FEAT_SKILL
    "Slider_移动速度:1:20:1",   // 2  ZZZ_FEAT_MOVE   上限 = ZZZ_MOVE_MUL_MAX
    "Slider_伤害倍率:1:100:1",  // 3  ZZZ_FEAT_DMG    上限 = ZZZ_DMG_MUL_MAX
    "Toggle_定怪",              // 4  ZZZ_FEAT_FREEZE
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "jql",
        .authGameId = "jql",            // 后台 games.game_id（绝区零拼音首字母），需运营侧注册
        .featureList = kZzz_Features,
        .featureCount = (int)(sizeof(kZzz_Features) / sizeof(kZzz_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 2000,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000,
        .tickIntervalMs = 5000,

        .onLoad = nullptr,              // 无过检补丁/无 early dex 注入
        .tryInit = Zzz_TryInit,
        .isReady = Zzz_IsReady,
        .tick = Zzz_Tick,
        .onFeatureChanged = Zzz_OnFeatureChanged,
        .onValueChanged = Zzz_OnValueChanged,
        .onInputChanged = nullptr,      // 无 InputText
        .forceClear = Zzz_ForceClear,
        .onApplyPlayPatch = nullptr,    // 无 OTA 偏移热更
    };
    return &mod;
}
