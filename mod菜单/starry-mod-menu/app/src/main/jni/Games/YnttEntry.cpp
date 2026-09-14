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
#include "YnttMod.h"

static const char* kYntt_Features[] = {
    "Category_战斗",      // 0（占位）
    "Toggle_秒杀",        // 1  YNTT_FEAT_KILL
    "Toggle_无敌",        // 2  YNTT_FEAT_INV
    "Slider_战斗加速:1:10:1", // 3  YNTT_FEAT_SPEED（1=关，2..10=倍率）
    "Toggle_无限体力",    // 4  YNTT_FEAT_STAMINA
    "Toggle_无限货币",    // 5  YNTT_FEAT_CURRENCY
    "Toggle_免广告",      // 6  YNTT_FEAT_NOADS
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "yntt",
        .authGameId = "yntt",          // 一念通天 → yntt
        .featureList = kYntt_Features,
        .featureCount = (int)(sizeof(kYntt_Features) / sizeof(kYntt_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 3000000,         // 3s（值单位=微秒）
        .initRetryMax = 120,            // 启动期重试上限；死锁案（如 gjc2）改 0 = Tick 懒加载
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,              // TODO: EarlyHook / 过检补丁 / 菜单 bootstrap 在此挂
        .tryInit = Yntt_TryInit,
        .isReady = Yntt_IsReady,
        .tick = Yntt_Tick,
        .onFeatureChanged = Yntt_OnFeatureChanged,
        .onValueChanged = Yntt_OnValueChanged,
        .onInputChanged = nullptr,      // 有 InputText/InputValue 才接
        .forceClear = Yntt_ForceClear,
        .onApplyPlayPatch = nullptr,    // 有 OTA 偏移热更才接
    };
    return &mod;
}
