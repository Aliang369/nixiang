/* ================================================================
 *  HzsjEntry.cpp — 魂坠深境 (com.chillyroom.containerofsoul.yw) 注册表入口
 *
 *  2026-09-10 legacy→modern 迁移（新增功能需扩菜单，Main.cpp 禁改）。
 *  功能位号与 HzsjMod.h enum 对齐（下标 = featNum，1-5 与 legacy 完全一致，
 *  Preferences/后台菜单 JSON 零漂移；6 = Category_功能 占位）：
 *    0 Category_战斗 / 1 秒杀 / 2 无敌 / 3 无限卡牌 / 4 无限金币 / 5 无限魂晶
 *    （20260910 晚：自动胜利/全角色体验/全天赋解锁三实验项回退，见适配文档 §8.2-§8.14）
 * ================================================================
 */
#include "ModEntry.h"
#include "HzsjMod.h"

static void Hzsj_TickAdapter(const bool* features, const int* /*values*/) {
    Hzsj_Tick(features);
}

// ModEntry 合同：tryInit 带 JavaVM*（legacy Hzsj_TryInit 无参，包一层）
static bool Hzsj_TryInitAdapter(JavaVM* /*vm*/) {
    return Hzsj_TryInit();
}

static const char* kHzsjFeatures[] = {
    "Category_战斗",          // 0（占位：1-5 归战斗）
    "Toggle_秒杀",            // 1  HZSJ_FEAT_SECKILL
    "Toggle_无敌",            // 2  HZSJ_FEAT_INVINCIBLE
    "Toggle_无限卡牌",        // 3  HZSJ_FEAT_INF_CARD
    "Toggle_无限金币",        // 4  HZSJ_FEAT_INF_COIN
    "Toggle_无限魂晶",        // 5  HZSJ_FEAT_INF_CRYSTAL
    "Category_功能",          // 6（占位）
    "Toggle_自动胜利",        // 7  HZSJ_FEAT_AUTO_WIN（进战斗写敌血 0 走自然死亡链，实测通过）
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "hzsj",
        .authGameId = "hzsj",
        .featureList = kHzsjFeatures,
        .featureCount = (int)(sizeof(kHzsjFeatures) / sizeof(kHzsjFeatures[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 3000000,   // legacy sleep(3)：等 il2cpp/HybridCLR 半初始化窗口过去
        .initRetryMax = 180,      // legacy 轮询 180 × 2s
        .initRetryIntervalMs = 2000,
        .tickIntervalMs = 200000, // 200ms，与 legacy 节拍一致

        .onLoad = nullptr,
        .tryInit = Hzsj_TryInitAdapter,
        .isReady = Hzsj_IsReady,
        .tick = Hzsj_TickAdapter,
        .onFeatureChanged = Hzsj_OnFeatureChanged,
        .onValueChanged = Hzsj_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Hzsj_ForceClear,
        .onApplyPlayPatch = Hzsj_ApplyPlayPatch,
    };
    return &mod;
}
