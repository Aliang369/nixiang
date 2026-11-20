/* ================================================================
 *  ywtEntry — 鹦鹉团（com.jinsaina.auditionsgo.gw 1.0.7）注册表入口
 *
 *  由 new_game_mod.sh 生成（2026-11-19）。菜单只两项：
 *    ① 自动跳舞 Toggle
 *    ② 判定概率：Switch + 6 档权重 SeekBar + 保留真实 Miss
 *  内部参数（per_frame / skip）不进 UI，固定默认 2 / 0。
 *
 *  落地方式：hook libxlua.so 的 lua_pcall → 运行时 luaL_loadstring 注入
 *  （设计见 10_docs/鹦鹉团/鹦鹉团_Lua注入落地设计_20261119.md §9）
 *  ⇒ 不需要 IL2CPP 挂点，waitLib 仍是 libil2cpp.so（保证 il2cpp 起来后再装钩）。
 * ================================================================
 */
#include "ModEntry.h"
#include "ywtMod.h"

// 2026-11-19 用户要求删掉「忽略真实Miss」：本就没有必要。删掉后下标整体前移，
// 下面的 enum 与 native 侧 featNum 判断必须与这里逐项对齐（改一处就要改三处）。
static const char* kYwt_Features[] = {
    "Category_自动跳舞",          // 0  分组
    "Toggle_自动跳舞",            // 1  YWT_FEAT_AUTO_DANCE（默认关）
    "Category_判定概率",          // 2  分组
    "Toggle_判定概率",            // 3  YWT_FEAT_JUDGE_ON（默认关）
    // 2026-11-20 用户要求：每条滑条带英文枚举名（自己核对中文↔英文）；删「失误」；补 Good 共 6 档
    //   Good(2) < Great(3) < Perfect(4) < SuperPerfect(5) < SuperPerfectPlus(6) < SuperPerfectPlusPlusAdd(7)
    "SeekBar_超完美+(SPPPlusAdd)_0_100_70",  // 4  w1 → SuperPerfectPlusPlusAdd
    "SeekBar_超完美(SPerfectPlus)_0_100_15", // 5  w2 → SuperPerfectPlus
    "SeekBar_完美(SPerfect)_0_100_8",        // 6  w3 → SuperPerfect
    "SeekBar_优秀(Perfect)_0_100_4",         // 7  w4 → Perfect
    "SeekBar_良好(Great)_0_100_2",           // 8  w5 → Great
    "SeekBar_一般(Good)_0_100_1",            // 9  w6 → Good
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "ywt",
        .authGameId = "ywt",
        .featureList = kYwt_Features,
        .featureCount = (int)(sizeof(kYwt_Features) / sizeof(kYwt_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 2000000,         // 微秒：2s（避开 il2cpp 半初始化）
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,
        .tryInit = ywt_TryInit,
        .isReady = ywt_IsReady,
        .tick = ywt_Tick,
        .onFeatureChanged = ywt_OnFeatureChanged,
        .onValueChanged = ywt_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = ywt_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
