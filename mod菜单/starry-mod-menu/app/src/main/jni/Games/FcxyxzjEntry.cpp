/* ================================================================
 *  FcxyxzjEntry.cpp — 凡尘西游修真记 (com.qxjc.fcxyxzj / TapTap 1.1.0) 注册表入口
 *
 *  game_id 推导：官方全名「凡尘西游修真记」→ 拼音首字母 fcxyxzj（无分隔符、小写）
 *  功能位号与 FcxyxzjMod.h enum 对齐（下标 = featNum，Category_ 占下标 0）：
 *    0 Category_功能 / 1 免广告 / 2 无限资源 / 3 物品不减 / 4 掉落翻倍
 *    （5 掉落必掉 = 副本结算概率判定全通过）
 * ================================================================ */
#include "ModEntry.h"
#include "FcxyxzjMod.h"

static const char* kFcxyxzjFeatures[] = {
    "Category_功能",        // 0（占位）
    "Toggle_免广告",        // 1  FCXYXZJ_FEAT_AD
    "Toggle_无限资源",      // 2  FCXYXZJ_FEAT_CUR（修为/灵石 获取放大 + 消耗不减）
    "Toggle_物品不减",      // 3  FCXYXZJ_FEAT_ITEM
    "Toggle_掉落翻倍",      // 4  FCXYXZJ_FEAT_DROP
    "Toggle_掉落必掉",      // 5  FCXYXZJ_FEAT_RATE（副本掉落几率判定必过）
    "Toggle_灵玉不减",      // 6  FCXYXZJ_FEAT_JADE（灵玉兑换等读值恒为大值）
    "Toggle_积分不减",      // 7  FCXYXZJ_FEAT_POINT（商店/兑换积分读值恒为大值）
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "fcxyxzj",
        .authGameId = "fcxyxzj",
        .featureList = kFcxyxzjFeatures,
        .featureCount = (int)(sizeof(kFcxyxzjFeatures) / sizeof(kFcxyxzjFeatures[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 3000000,        // 3s：等 il2cpp 半初始化窗口过去（值单位=微秒）
        .initRetryMax = 120,           // 启动期 120 × 2s 重试；失败后由 Tick 懒装兜底
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,      // 200ms

        .onLoad = nullptr,
        .tryInit = Fcxyxzj_TryInit,
        .isReady = Fcxyxzj_IsReady,
        .tick = Fcxyxzj_Tick,
        .onFeatureChanged = Fcxyxzj_OnFeatureChanged,
        .onValueChanged = Fcxyxzj_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Fcxyxzj_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
