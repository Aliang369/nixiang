/* ================================================================
 *  WscqEntry — 圣翼传说（com.aiyue.wscq）注册表入口
 *
 *  职责：只填一张 StarryGameMod 表（菜单 / 初始化参数 / 回调接线）。
 *
 *  铁律：
 *    - 改菜单、调初始化节奏、增删回调 → 只改本文件
 *    - MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 永不需要动
 *    - 不用的回调留 nullptr，不要造假实现
 *    - featureList 下标 = featNum（Category_ 占下标），与 WscqMod.h 的 enum 对齐
 * ================================================================
 */
#include "ModEntry.h"
#include "WscqMod.h"

static const char* kWscq_Features[] = {
    "Category_战斗",              // 0
    "SeekBar_伤害倍率_1_1000_1",  // 1  WSCQ_FEAT_DMG（1 = 关，1000 = 千倍）
    "Toggle_玩家无敌",            // 2  WSCQ_FEAT_INV
    "Toggle_全屏攻击",            // 3  WSCQ_FEAT_AOE
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "wscq",
        .authGameId = "sycs",           // 圣翼传说（全名拼音首字母）；需后台 games.game_id 同步
        .featureList = kWscq_Features,
        .featureCount = (int)(sizeof(kWscq_Features) / sizeof(kWscq_Features[0])),

        .waitLib = "libil2cpp.so",      // Unity IL2CPP：等 libil2cpp 映射后再挂点
        .initDelayMs = 5000000,         // 5s：等 il2cpp 类表就绪（值单位=微秒）
        .initRetryMax = 120,            // 120 × 2s 启动期重试；失败后 Tick 懒装兜底
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,       // 200ms：开关/滑块同步要跟手

        .onLoad = nullptr,              // 不需要 EarlyHook：全部懒装，无过检补丁
        .tryInit = Wscq_TryInit,
        .isReady = Wscq_IsReady,
        .tick = Wscq_Tick,
        .onFeatureChanged = Wscq_OnFeatureChanged,
        .onValueChanged = Wscq_OnValueChanged,
        .onInputChanged = nullptr,      // 无 InputText/InputValue
        .forceClear = Wscq_ForceClear,
        .onApplyPlayPatch = nullptr,    // 无 OTA 偏移热更
    };
    return &mod;
}
