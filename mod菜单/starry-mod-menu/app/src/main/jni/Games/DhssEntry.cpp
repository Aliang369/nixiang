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
#include "DhssMod.h"

static const char* kDhss_Features[] = {
    "Category_战斗",      // 0
    "Toggle_秒杀",        // 1  DHSS_FEAT_KILL（斌哥「百倍伤害」→ 怪物目标伤害 ×100）
    "Toggle_无敌",        // 2  DHSS_FEAT_INV（斌哥「玩家无敌」→ 玩家侧受伤置 0）
    "Toggle_免广告",      // 3  DHSS_FEAT_AD（跳过广告播放，走官方 avoidAd 分支）
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Dhss",
        .authGameId = "dhss",        // 大话蜀山 → dhss（官方中文全名拼音首字母）
        .featureList = kDhss_Features,
        .featureCount = (int)(sizeof(kDhss_Features) / sizeof(kDhss_Features[0])),

        .waitLib = "libil2cpp.so",   // Unity IL2CPP；等库后再按名解析挂点
        .initDelayMs = 3000000,      // 3s（单位=微秒）：等 il2cpp 半初始化窗口过去
        .initRetryMax = 120,            // 启动期重试上限；死锁案（如 gjc2）改 0 = Tick 懒加载
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,              // 本作无需 EarlyHook / 过检补丁
        .tryInit = Dhss_TryInit,
        .isReady = Dhss_IsReady,
        .tick = Dhss_Tick,
        .onFeatureChanged = Dhss_OnFeatureChanged,
        .onValueChanged = Dhss_OnValueChanged,
        .onInputChanged = nullptr,      // 有 InputText/InputValue 才接
        .forceClear = Dhss_ForceClear,
        .onApplyPlayPatch = nullptr,    // 有 OTA 偏移热更才接
    };
    return &mod;
}
