/* ================================================================
 *  {Game}Entry — 注册表入口模板（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  生成：new_game_mod.sh 自动复制为 {Game}Entry.cpp 并替换标识。
 *  职责：只填一张 StarryGameMod 表（菜单 / 初始化参数 / 回调接线）。
 *
 *  铁律：
 *    - 改菜单、调初始化节奏、增删回调 → 只改本文件
 *    - MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 永不需要动
 *    - 不用的回调留 nullptr，不要造假实现
 *    - featureList 下标 = featNum（Category_ 占下标），与 Mod 的 enum 对齐
 * ================================================================
 */
#include "ModEntry.h"
#include "WdjnwcfMod.h"

static const char* kWdjnwcf_Features[] = {
    "Category_功能",            // 0 WDJNWCF_FEAT_CAT
    "Toggle_无敌",              // 1 WDJNWCF_FEAT_INVINCIBLE
    "InputValue_100:伤害倍率",   // 2 WDJNWCF_FEAT_DMG_MULT（1 = 关）
    "InputValue_10:加速",        // 3 WDJNWCF_FEAT_SPEED（1 = 关）
    "Button_金币",              // 4 WDJNWCF_FEAT_GOLD
    "Button_钻石",              // 5 WDJNWCF_FEAT_CRYSTAL
    "Button_体力",              // 6 WDJNWCF_FEAT_POWER
    "Toggle_免广告",            // 7 WDJNWCF_FEAT_NOAD（NativeData.SetPlayerIgnoreAds）
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Wdjnwcf",
        .authGameId = "wdjnwcf",   // 我的技能无限触发 → w-d-j-n-w-c-f（后台 games.game_id 同名）
        .featureList = kWdjnwcf_Features,
        .featureCount = (int)(sizeof(kWdjnwcf_Features) / sizeof(kWdjnwcf_Features[0])),

        .waitLib = "libil2cpp.so",       // Unity IL2CPP
        .initDelayMs = 2000000,        // 微秒 = 2s（等 il2cpp 初始化完成）
        .initRetryMax = 120,
        .initRetryIntervalMs = 1000000,// 微秒 = 1s
        .tickIntervalMs = 200000,      // 微秒 = 200ms

        .onLoad = nullptr,              // TODO: EarlyHook / 过检补丁 / 菜单 bootstrap 在此挂
        .tryInit = Wdjnwcf_TryInit,
        .isReady = Wdjnwcf_IsReady,
        .tick = Wdjnwcf_Tick,
        .onFeatureChanged = Wdjnwcf_OnFeatureChanged,
        .onValueChanged = Wdjnwcf_OnValueChanged,
        .onInputChanged = Wdjnwcf_OnInputChanged,  // InputValue 走文本通道时接住
        .forceClear = Wdjnwcf_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
