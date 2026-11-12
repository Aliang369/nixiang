/* ================================================================
 *  QxzxczEntry — 群侠传：幸存者 注册表入口（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  游戏：群侠传：幸存者 0.2.3.9 / com.nenlystudio.qunxiasurvivor.cn / TapTap 中国渠道
 *  引擎：Godot 4.6.1（GDScript 字节码 .gdc，无 il2cpp）
 *  game_id：qxzxcz（全名首字母：群q 侠x 传z 幸x 存c 者z）
 *
 *  featureList 下标 = featNum = StateMirror 落盘数组下标（starry_state.txt）：
 *    [0] Category_功能（无回调，但占下标）
 *    [1] Slider_伤害倍率:1:100:1  → starry_state[1]（1..100，1=原样）
 *    [2] Toggle_无敌               → starry_state[2]（1/0）
 *    [3] Toggle_无限银钱           → starry_state[3]（1/0）
 * ================================================================
 */
#include "ModEntry.h"
#include "QxzxczMod.h"

static const char* kQxzxcz_Features[] = {
    "Category_功能",             // 0
    "Slider_伤害倍率:1:100:1",   // 1  QXZXCZ_FEAT_DMG
    "Toggle_无敌",               // 2  QXZXCZ_FEAT_INV
    "Toggle_无限银钱",           // 3  QXZXCZ_FEAT_SILVER
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Qxzxcz",
        .authGameId = "qxzxcz",
        .featureList = kQxzxcz_Features,
        .featureCount = (int)(sizeof(kQxzxcz_Features) / sizeof(kQxzxcz_Features[0])),

        // 无 hook 面：不等任何库（nullptr = 直接放行），initRetryMax=0 → 不跑 tryInit 重试循环
        .waitLib = nullptr,
        .initDelayMs = 0,
        .initRetryMax = 0,
        .initRetryIntervalMs = 0,
        .tickIntervalMs = 500000,   // 200.. 单位 us：本作 Tick 为空实现，500ms 足够轻

        .onLoad = nullptr,
        .tryInit = Qxzxcz_TryInit,
        .isReady = Qxzxcz_IsReady,
        .tick = Qxzxcz_Tick,
        .onFeatureChanged = Qxzxcz_OnFeatureChanged,
        .onValueChanged = Qxzxcz_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Qxzxcz_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
