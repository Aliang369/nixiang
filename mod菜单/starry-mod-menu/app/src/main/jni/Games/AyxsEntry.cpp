/* ================================================================
 *  AyxsEntry — 暗影猎手:失落的世界（com.enigma.shadowhunter.m4399）
 *
 *  抓点来源：斌哥改包实测（3 处 il2cpp 桩址 + 壳旗标 feature1/2/3）
 *  官方版与斌哥包 libil2cpp.so / metadata / libunity 逐字节一致 → RVA 通用
 * ================================================================
 */
#include "ModEntry.h"
#include "AyxsMod.h"

static const char* kAyxs_Features[] = {
    "Category_战斗",      // 0
    "Toggle_超级定怪",     // 1  AYXS_FEAT_DINGGUAI
    "Toggle_超级恢复",     // 2  AYXS_FEAT_HUIFU
    "Toggle_超级秒杀",     // 3  AYXS_FEAT_MIAOSHA
    "Toggle_免广告",       // 4  AYXS_FEAT_NOAD
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Ayxs",
        .authGameId = "ayxssldsj",        // 全名「暗影猎手:失落的世界」拼音首字母
        .featureList = kAyxs_Features,
        .featureCount = (int)(sizeof(kAyxs_Features) / sizeof(kAyxs_Features[0])),

        .waitLib = "libil2cpp.so",   // Unity IL2CPP
        .initDelayMs = 3000000,           // 等库后延迟，避开 il2cpp 半初始化
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,
        .tryInit = Ayxs_TryInit,
        .isReady = Ayxs_IsReady,
        .tick = Ayxs_Tick,
        .onFeatureChanged = Ayxs_OnFeatureChanged,
        .onValueChanged = Ayxs_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Ayxs_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
