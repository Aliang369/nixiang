/* ================================================================
 *  WorEntry — 注册表入口模板（MainModern.cpp 经 GameMod_Get 读取）
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
#include "WorMod.h"

static const char* kWor_Features[] = {
    "Category_战斗",              // 0（占位）
    "Toggle_无敌",                // 1  WOR_FEAT_INV（我方单位承伤清零）
    "Slider_伤害倍率:1:10:1",     // 2  WOR_FEAT_DMG（1 = 关闭；上限必须与 WorMod.cpp 的 DMG_MUL_MAX 一致）
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Wor",
        .authGameId = "wor",            // 后台 games.game_id =「境界守望者」国际服
        .featureList = kWor_Features,
        .featureCount = (int)(sizeof(kWor_Features) / sizeof(kWor_Features[0])),

        .waitLib = "liblogic.so",       // 运行时解压出的真 IL2CPP 库（包内 libil2cpp.so 只是 7z 壳）
        .initDelayMs = 2000000,            // 等库后延迟（避开 il2cpp/HybridCLR 半初始化；实测再调）
        .initRetryMax = 120,            // 启动期重试上限
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,              // TODO: EarlyHook / 过检补丁 / 菜单 bootstrap 在此挂
        .tryInit = Wor_TryInit,
        .isReady = Wor_IsReady,
        .tick = Wor_Tick,
        .onFeatureChanged = Wor_OnFeatureChanged,
        .onValueChanged = Wor_OnValueChanged,
        .onInputChanged = nullptr,      // 有 InputText/InputValue 才接
        .forceClear = Wor_ForceClear,
        .onApplyPlayPatch = nullptr,    // 有 OTA 偏移热更才接
    };
    return &mod;
}