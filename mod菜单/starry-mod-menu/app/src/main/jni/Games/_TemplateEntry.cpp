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
#include "_TemplateMod.h"

static const char* kTemplate_Features[] = {
    "Category_战斗",      // 0
    "Toggle_秒杀",        // 1  TEMPLATE_FEAT_DMG
    "Toggle_无敌",        // 2  TEMPLATE_FEAT_INV
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Tpl",
        .authGameId = "TODO_game_id",   // TODO: 后台 games.game_id（官方中文名拼音首字母，勿与存量冲突）
        .featureList = kTemplate_Features,
        .featureCount = (int)(sizeof(kTemplate_Features) / sizeof(kTemplate_Features[0])),

        .waitLib = nullptr,             // TODO: 目标库，如 "libil2cpp.so"；Cocos 系填 GameConfig 对应 TARGET_LIB
        .initDelayMs = 2000,            // 等库后延迟（避开 il2cpp/HybridCLR 半初始化；实测再调）
        .initRetryMax = 120,            // 启动期重试上限；死锁案（如 gjc2）改 0 = Tick 懒加载
        .initRetryIntervalMs = 2000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,              // TODO: EarlyHook / 过检补丁 / 菜单 bootstrap 在此挂
        .tryInit = Template_TryInit,
        .isReady = Template_IsReady,
        .tick = Template_Tick,
        .onFeatureChanged = Template_OnFeatureChanged,
        .onValueChanged = Template_OnValueChanged,
        .onInputChanged = nullptr,      // 有 InputText/InputValue 才接
        .forceClear = Template_ForceClear,
        .onApplyPlayPatch = nullptr,    // 有 OTA 偏移热更才接
    };
    return &mod;
}
