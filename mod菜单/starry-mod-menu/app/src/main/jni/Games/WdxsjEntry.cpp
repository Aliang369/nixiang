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
#include "WdxsjMod.h"

static const char* kWdxsj_Features[] = {
    "Category_功能",          // 0（占位）
    "Toggle_百倍伤害",        // 1  WDXSJ_FEAT_DMG
    "Toggle_无敌",            // 2  WDXSJ_FEAT_INV
    "Toggle_无限货币道具",    // 3  WDXSJ_FEAT_CUR
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "wdxsj",
        .authGameId = "wdxsj",           // 我的修仙世界
        .featureList = kWdxsj_Features,
        .featureCount = (int)(sizeof(kWdxsj_Features) / sizeof(kWdxsj_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 5000000,         // 5s：等 il2cpp 与 Assembly-CSharp 类表就绪（值单位=微秒）
        .initRetryMax = 120,            // 120 × 2s 启动期重试；失败后 Tick 懒装兜底
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,              // TODO: EarlyHook / 过检补丁 / 菜单 bootstrap 在此挂
        .tryInit = Wdxsj_TryInit,
        .isReady = Wdxsj_IsReady,
        .tick = Wdxsj_Tick,
        .onFeatureChanged = Wdxsj_OnFeatureChanged,
        .onValueChanged = Wdxsj_OnValueChanged,
        .onInputChanged = nullptr,      // 有 InputText/InputValue 才接
        .forceClear = Wdxsj_ForceClear,
        .onApplyPlayPatch = nullptr,    // 有 OTA 偏移热更才接
    };
    return &mod;
}
