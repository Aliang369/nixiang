/* ================================================================
 *  BlfxEntry — 爆裂防线（com.muyegame.blfx）1.01.06 注册表入口
 *
 *  职责：只填一张 StarryGameMod 表（菜单 / 初始化参数 / 回调接线）。
 *
 *  铁律：
 *    - 改菜单、调初始化节奏、增删回调 → 只改本文件
 *    - MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 永不需要动
 *    - 不用的回调留 nullptr，不要造假实现
 *    - featureList 下标 = featNum（Category_ 占下标），与 BlfxMod.h 的 enum 对齐
 * ================================================================
 */
#include "ModEntry.h"
#include "BlfxMod.h"

static const char* kBlfx_Features[] = {
    "Category_战斗",              // 0
    "SeekBar_伤害倍率_1_100_1",   // 1  BLFX_FEAT_DMG（1 = 关，100 = 百倍）
    "Toggle_无敌",                // 2  BLFX_FEAT_INV
    "SeekBar_战斗加速_1_10_1",    // 3  BLFX_FEAT_SPEED（1 = 关）
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "blfx",
        .authGameId = "blfx",           // 爆裂防线（全名拼音首字母）；需后台 games.game_id 同步
        .featureList = kBlfx_Features,
        .featureCount = (int)(sizeof(kBlfx_Features) / sizeof(kBlfx_Features[0])),

        .waitLib = "libil2cpp.so",      // Unity IL2CPP（团结引擎）；等 libil2cpp 映射后再解析
        .initDelayMs = 5000000,         // 5s：等 il2cpp 类表 / HybridCLR 热更程序集就绪（单位=微秒）
        .initRetryMax = 120,            // 120 × 2s 启动期重试；失败后 Tick 懒装兜底
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,       // 200ms：开关/滑块同步要跟手

        .onLoad = nullptr,              // 全部懒装，无过检补丁
        .tryInit = Blfx_TryInit,
        .isReady = Blfx_IsReady,
        .tick = Blfx_Tick,
        .onFeatureChanged = Blfx_OnFeatureChanged,
        .onValueChanged = Blfx_OnValueChanged,
        .onInputChanged = nullptr,      // 无 InputText/InputValue
        .forceClear = Blfx_ForceClear,
        .onApplyPlayPatch = nullptr,    // 无 OTA 偏移热更
    };
    return &mod;
}
