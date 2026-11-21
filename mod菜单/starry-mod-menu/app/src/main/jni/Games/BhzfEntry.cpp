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
#include "BhzfMod.h"

static const char* kBhzf_Features[] = {
    "Category_战斗",      // 0
    "Toggle_秒杀",        // 1  BHZF_FEAT_DMG（仅敌方）
    "Toggle_无敌",        // 2  BHZF_FEAT_INV（仅己方）
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "bhzf",
        .authGameId = "bhzf",   // TODO: 后台 games.game_id（官方中文名拼音首字母，勿与存量冲突）
        .featureList = kBhzf_Features,
        .featureCount = (int)(sizeof(kBhzf_Features) / sizeof(kBhzf_Features[0])),

        .waitLib = "libil2cpp.so",             // TODO: 目标库，如 "libil2cpp.so"；Cocos 系填 GameConfig 对应 TARGET_LIB
        .initDelayMs = 0,            // 等库后延迟（避开 il2cpp/HybridCLR 半初始化；实测再调）
        .initRetryMax = 600,            // 启动期重试上限；死锁案（如 gjc2）改 0 = Tick 懒加载
        .initRetryIntervalMs = 200,
        .tickIntervalMs = 100,

        .onLoad = nullptr,              // TODO: EarlyHook / 过检补丁 / 菜单 bootstrap 在此挂
        .tryInit = Bhzf_TryInit,
        .isReady = Bhzf_IsReady,
        .tick = Bhzf_Tick,
        .onFeatureChanged = Bhzf_OnFeatureChanged,
        .onValueChanged = Bhzf_OnValueChanged,
        .onInputChanged = nullptr,      // 有 InputText/InputValue 才接
        .forceClear = Bhzf_ForceClear,
        .onApplyPlayPatch = nullptr,    // 有 OTA 偏移热更才接
    };
    return &mod;
}
