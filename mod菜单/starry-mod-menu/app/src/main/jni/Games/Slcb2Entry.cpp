/* ================================================================
 *  Slcb2Entry.cpp — 失落城堡2 (com.leiting.slcb2) 注册表入口
 *
 *  底包：官方原版 1.0.3 / vc10（CN=lt），libil2cpp.so md5 9aede69e…，
 *        metadata md5 6e94e96b…（与斌哥包字节相同 ⇒ 抓点可直接复用）。
 *  引擎：Unity IL2CPP metadata v31 + HybridCLR；易盾 4.23 + 乐变 LBApplication。
 *
 *  功能位号（下标 = featNum，与 Slcb2Mod.h 的 enum 一一对齐）：
 *    0 Category_战斗 / 1 百倍伤害 / 2 无敌 / 3 无限技能
 *    4 Category_资源 / 5 无限魔铁锭（IronPowder）/ 6 无限魂晶（Crystal）
 *
 *  ⚠️ 本版为**环境阶段**：菜单先出，hook 一律未实现（0 hook，绝不改游戏行为）。
 *     功能实现见 10_docs/失落城堡2/移植方案_两项货币与反作弊_20261029.md。
 * ================================================================
 */
#include "ModEntry.h"
#include "Slcb2Mod.h"

static void Slcb2_TickAdapter(const bool* features, const int* values) {
    Slcb2_Tick(features, values);
}

static bool Slcb2_TryInitAdapter(JavaVM* /*vm*/) {
    return Slcb2_TryInit(nullptr);
}

static const char* kSlcb2Features[] = {
    "Category_战斗",        // 0（占位）
    "Toggle_百倍伤害",      // 1  SLCB2_FEAT_DMG
    "Toggle_无敌",          // 2  SLCB2_FEAT_INV
    "Toggle_无限技能",      // 3  SLCB2_FEAT_SKILL
    "Category_资源",        // 4（占位）
    "Toggle_无限魔铁锭",    // 5  SLCB2_FEAT_IRON
    "Toggle_无限魂晶",      // 6  SLCB2_FEAT_CRYSTAL
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Slcb2",
        .authGameId = "slcb2",
        .featureList = kSlcb2Features,
        .featureCount = (int)(sizeof(kSlcb2Features) / sizeof(kSlcb2Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 3000000,    // 等 il2cpp/HybridCLR 半初始化窗口过去（同 hzsj 先例）
        .initRetryMax = 180,       // 180 × 2s
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,
        .tryInit = Slcb2_TryInitAdapter,
        .isReady = Slcb2_IsReady,
        .tick = Slcb2_TickAdapter,
        .onFeatureChanged = Slcb2_OnFeatureChanged,
        .onValueChanged = Slcb2_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Slcb2_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
