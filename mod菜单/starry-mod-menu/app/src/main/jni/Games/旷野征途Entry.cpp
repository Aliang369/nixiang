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
#include "旷野征途Mod.h"

// menu 必须与 featNum 位置对齐（下标 = featNum，Category 占 0/4），
// 所以不能删项缩表 —— 未适配项保留位置并在标签上标注，native 侧仍失败关闭。
//
// 2.2 状态：资源 3 项（XOR 编码，等 Economy22.h）与战斗 2 项（解释器入口未定位）未适配；
// 加速 / 免广告 已实现（未上机验证）。Economy22 集成后把 KYZT_ECON_TAG 置空即可。
// 资源 3 项：v5 已实现（含钻石 AddItem 创建+回读），但**未上机验证** ⇒ 标「2.2待验证」。
// 战斗 2 项：解释器入口未定位 ⇒ 标「2.2未适配」。两类标签语义不同，不可混用。
#define KYZT_ECON_TAG ""
#define KYZT_BATTLE_TAG ""   // 2026-11-14 21:50：标签清空（用户要求菜单名干净）；验证状态改记文档

static const char* k旷野征途_Features[] = {
    "Category_资源",                              // 0
    "Button_金币+9999" KYZT_ECON_TAG,              // 1
    "Button_钻石+9999" KYZT_ECON_TAG,              // 2
    "Button_积分+9999" KYZT_ECON_TAG,              // 3
    "Category_战斗",                              // 4
    "Toggle_游戏加速",                             // 5
    "Toggle_玩家无敌" KYZT_BATTLE_TAG,             // 6
    "Toggle_百倍伤害" KYZT_BATTLE_TAG,             // 7
    "Toggle_免广告",                               // 8
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "KYZT",
        .authGameId = "kyzt",   // TODO: 后台 games.game_id（官方中文名拼音首字母，勿与存量冲突）
        .featureList = k旷野征途_Features,
        .featureCount = (int)(sizeof(k旷野征途_Features) / sizeof(k旷野征途_Features[0])),

        .waitLib = "libil2cpp.so",             // TODO: 目标库，如 "libil2cpp.so"；Cocos 系填 GameConfig 对应 TARGET_LIB
        .initDelayMs = 3000000,            // 等库后延迟（避开 il2cpp/HybridCLR 半初始化；实测再调）
        .initRetryMax = 120,            // 启动期重试上限；死锁案（如 gjc2）改 0 = Tick 懒加载
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,              // TODO: EarlyHook / 过检补丁 / 菜单 bootstrap 在此挂
        .tryInit = 旷野征途_TryInit,
        .isReady = 旷野征途_IsReady,
        .tick = 旷野征途_Tick,
        .onFeatureChanged = 旷野征途_OnFeatureChanged,
        .onValueChanged = 旷野征途_OnValueChanged,
        .onInputChanged = nullptr,      // 有 InputText/InputValue 才接
        .forceClear = 旷野征途_ForceClear,
        .onApplyPlayPatch = nullptr,    // 有 OTA 偏移热更才接
    };
    return &mod;
}
