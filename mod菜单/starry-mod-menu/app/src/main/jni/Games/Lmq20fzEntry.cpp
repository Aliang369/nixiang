/* ================================================================
 *  Lmq20fzEntry — 黎明前20分钟（com.nanostudios.games.twenty.minutes 1.0.106）
 *  菜单注册表（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  功能范围＝斌哥新版菜单全部 3 项（用户 2026-11-04「功能全要」）：
 *    无敌（toggle）/ 百倍伤害（1–100）/ 战斗加速（1–5）
 *  抓点：10_docs/_batch_binge6/三款新版菜单_抓点总表_20261103.md §四.3
 * ================================================================ */
#include "ModEntry.h"
#include "Lmq20fzMod.h"

static const char* kLmq20fz_Features[] = {
    "Category_战斗",          // 0
    "Toggle_无敌",            // 1  LMQ20FZ_FEAT_INV
    "Slider_百倍伤害:1:100",  // 2  LMQ20FZ_FEAT_DMG   1=关
    "Slider_战斗加速:1:5",    // 3  LMQ20FZ_FEAT_SPD   1=关
    "Toggle_免广告",          // 4  LMQ20FZ_FEAT_NOAD（用户 2026-11-04 追加）
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Lmq20fz",
        .authGameId = "lmq20fz",   // 黎明前20分钟 → lmq20fz（拼音首字母，数字保留；对齐 nhsy2 先例）
        .featureList = kLmq20fz_Features,
        .featureCount = (int)(sizeof(kLmq20fz_Features) / sizeof(kLmq20fz_Features[0])),

        .waitLib = "libil2cpp.so",   // Unity IL2CPP；等库后再挂点（RVA 优先、按名兜底）
        .initDelayMs = 3000000,      // 3s（单位＝微秒）：等 il2cpp 半初始化窗口过去
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,           // 本作无需 EarlyHook / 过检补丁
        .tryInit = Lmq20fz_TryInit,
        .isReady = Lmq20fz_IsReady,
        .tick = Lmq20fz_Tick,
        .onFeatureChanged = Lmq20fz_OnFeatureChanged,
        .onValueChanged = Lmq20fz_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Lmq20fz_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
