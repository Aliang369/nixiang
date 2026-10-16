/* ================================================================
 *  GgbhEntry — 鬼谷八荒注册表入口（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  生成：bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh Ggbh Ggbh
 *  职责：只填一张 StarryGameMod 表（菜单 / 初始化参数 / 回调接线）。
 *
 *  铁律：
 *    - 改菜单、调初始化节奏、增删回调 → 只改本文件
 *    - MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 永不需要动
 *    - 不用的回调留 nullptr，不要造假实现
 *    - featureList 下标 = featNum（Category_ 占下标），与 GgbhMod.h 的 enum 对齐
 *
 *  功能集只有用户点名的两项（硬约束 #1 原话，禁扩张）：
 *    ① GM 菜单   ← UISwitchPlayerMenu.get_IsCheatModeEnabled（读侧恒真）
 *    ② 全局加速   ← Time.get_deltaTime 每帧钉 Time.timeScale（滑块倍率，1 = 关）
 * ================================================================ */
#include "ModEntry.h"
#include "GgbhMod.h"

static const char* kGgbh_Features[] = {
    "Category_鬼谷功能",          // 0
    "Toggle_GM菜单",              // 1  GGBH_FEAT_GMMENU
    "SeekBar_加速倍率_1_20_1",    // 2  GGBH_FEAT_SPEED（1 = 关，2..20 = 倍率）
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Ggbh",
        // 游戏官方中文全名汉语拼音首字母（鬼谷八荒 → ggbh）；
        // 四点同步：本行 = 后台 games.game_id = 菜单 template_id = 文档 JSON。
        // 注意：斌哥服务端的 game_id=746 与星光无关，禁止复用。
        .authGameId = "ggbh",
        .featureList = kGgbh_Features,
        .featureCount = (int)(sizeof(kGgbh_Features) / sizeof(kGgbh_Features[0])),

        .waitLib = "libil2cpp.so",      // Unity IL2CPP 瘦壳（官包 so md5 与斌哥包一致）
        .initDelayMs = 2000000,            // 等库后延迟（避开 il2cpp 半初始化）
        .initRetryMax = 120,            // 启动期重试上限；失败后 Tick 懒装兜底
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,       // 200ms：开关/滑块同步要跟手

        .onLoad = nullptr,              // 本游戏无需 EarlyHook / 过检补丁
        .tryInit = Ggbh_TryInit,
        .isReady = Ggbh_IsReady,
        .tick = Ggbh_Tick,
        .onFeatureChanged = Ggbh_OnFeatureChanged,
        .onValueChanged = Ggbh_OnValueChanged,
        .onInputChanged = nullptr,      // 无 InputText/InputNum
        .forceClear = Ggbh_ForceClear,
        .onApplyPlayPatch = nullptr,    // 无 OTA 偏移热更
    };
    return &mod;
}
