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
#include "XhzmMod.h"

static const char* kXhzm_Features[] = {
    "Category_战斗",              // 0
    "SeekBar_伤害倍率_1_100_1",   // 1  XHZM_FEAT_DMG
    "Toggle_无敌",                // 2  XHZM_FEAT_GOD
    "Toggle_无限技能",            // 3  XHZM_FEAT_NOCD
    "Toggle_免广告",              // 4  XHZM_FEAT_NOAD（状态位走状态文件 idx4，native 无 hook）
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "邂逅在迷宫",
        .authGameId = "xhzm",           // 邂逅在迷宫 → xie hou zai mi gong
        .featureList = kXhzm_Features,
        .featureCount = (int)(sizeof(kXhzm_Features) / sizeof(kXhzm_Features[0])),

        .waitLib = "libil2cpp.so",      // Unity/IL2CPP（团结引擎 + HybridCLR 热更）
        .initDelayMs = 2000000,            // 等库后延迟（避开 il2cpp/HybridCLR 半初始化；实测再调）
        .initRetryMax = 120,            // 启动期重试上限；死锁案（如 gjc2）改 0 = Tick 懒加载
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,              // TODO: EarlyHook / 过检补丁 / 菜单 bootstrap 在此挂
        .tryInit = Xhzm_TryInit,
        .isReady = Xhzm_IsReady,
        .tick = Xhzm_Tick,
        .onFeatureChanged = Xhzm_OnFeatureChanged,
        .onValueChanged = Xhzm_OnValueChanged,
        .onInputChanged = nullptr,      // 有 InputText/InputValue 才接
        .forceClear = Xhzm_ForceClear,
        .onApplyPlayPatch = nullptr,    // 有 OTA 偏移热更才接
    };
    return &mod;
}
