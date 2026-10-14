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
#include "BwxrkMod.h"

static const char* kBwxrk_Features[] = {
    "Category_功能",          // 0
    "Toggle_拿全部技能",      // 1  BWXRK_FEAT_ALLSKILL
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Bwxrk",
        .authGameId = "bwxrk",          // 保卫向日葵 → b(保)w(卫)x(向)r(日)k(葵)
        .featureList = kBwxrk_Features,
        .featureCount = (int)(sizeof(kBwxrk_Features) / sizeof(kBwxrk_Features[0])),

        .waitLib = "libil2cpp.so",      // 运行时实际加载的是 dodjoy 热更落地的 files/libil2cpp.so（同名）
        .initDelayMs = 2000,            // 等库后延迟（避开 il2cpp/HybridCLR 半初始化；实测再调）
        .initRetryMax = 120,            // 启动期重试上限；死锁案（如 gjc2）改 0 = Tick 懒加载
        .initRetryIntervalMs = 2000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,              // TODO: EarlyHook / 过检补丁 / 菜单 bootstrap 在此挂
        .tryInit = Bwxrk_TryInit,
        .isReady = Bwxrk_IsReady,
        .tick = Bwxrk_Tick,
        .onFeatureChanged = Bwxrk_OnFeatureChanged,
        .onValueChanged = Bwxrk_OnValueChanged,
        .onInputChanged = nullptr,      // 有 InputText/InputValue 才接
        .forceClear = Bwxrk_ForceClear,
        .onApplyPlayPatch = nullptr,    // 有 OTA 偏移热更才接
    };
    return &mod;
}
