/* ================================================================
 *  HkldEntry — 汉克旅店 1.1.0 (vc337) 注册表入口
 *
 *  独立于其它游戏：菜单、鉴权 id、初始化节奏、回调全部在这里声明。
 *  MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 均不改。
 *
 *  单位提醒（ModEntry.h）：initDelayMs / initRetryIntervalMs / tickIntervalMs
 *  语义是**微秒**（框架走 usleep）。故 200ms = 200000。
 * ================================================================ */
#include "ModEntry.h"
#include "HkldMod.h"

static const char* kHkld_Features[] = {
    "Category_功能",              // 0  （占下标，仅供 UI 分组）
    "Toggle_无敌",                // 1  HKLD_FEAT_GOD    -> god
    "Slider_伤害倍率:1:100",       // 2  HKLD_FEAT_DMGX   -> damage_x
    "Toggle_无限技能",            // 3  HKLD_FEAT_SKILL  -> inf_skill（免蓝 + 无 CD）
    "Toggle_解锁角色",            // 4  HKLD_FEAT_UNLOCK -> unlock_char
    "Toggle_无限金币",            // 5  HKLD_FEAT_MONEY  -> inf_money
    "Toggle_无限钥匙",            // 6  HKLD_FEAT_KEY    -> inf_key
    "Slider_移动速度:100:300",    // 7  HKLD_FEAT_MOVESPD -> move_speed_x（100 = 原速/默认，300 = 3x）
    "Toggle_无限蓝",              // 8  HKLD_FEAT_INFMP  -> inf_mp
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Hkld",
        .authGameId = "hkld",           // 汉克旅店 -> 汉h 克k 旅l 店d
        .featureList = kHkld_Features,
        .featureCount = (int)(sizeof(kHkld_Features) / sizeof(kHkld_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 0,               // 等库命中后立即尝试（内部自带 100ms 快重试线程）
        .initRetryMax = 600,
        .initRetryIntervalMs = 100000,  // 100ms（微秒）
        .tickIntervalMs = 200000,       // 200ms（微秒）

        .onLoad = nullptr,
        .tryInit = Hkld_TryInit,
        .isReady = Hkld_IsReady,
        .tick = Hkld_Tick,
        .onFeatureChanged = Hkld_OnFeatureChanged,
        .onValueChanged = Hkld_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Hkld_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
