/* ================================================================
 *  JjdmIntlEntry — 境界刀鸣 国际服 注册表入口（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  职责：只填一张 StarryGameMod 表（菜单 / 初始化参数 / 回调接线）。
 *  铁律：MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 零修改。
 *  featureList 下标 = featNum，与 JjdmMod.h 的 enum 对齐。
 * ================================================================ */
#include "ModEntry.h"
#include "JjdmMod.h"

static const char* kJjdmIntl_Features[] = {
    "Category_战斗",              // 0（占位）
    "Slider_伤害倍率:1:100:1",     // 1  JJDM_FEAT_DMG（1 = 关闭；默认必须为 1，禁止默认开）
    "Toggle_无敌",                // 2  JJDM_FEAT_INV
    "Toggle_无限技能",            // 3  JJDM_FEAT_NOCD
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Jjdm",
        .authGameId = "jjdmintl",      // 国际版独立 id（先例 gjc2 -> gjc2intl，勿与国服 jjdm 共用）
        .featureList = kJjdmIntl_Features,
        .featureCount = (int)(sizeof(kJjdmIntl_Features) / sizeof(kJjdmIntl_Features[0])),

        .waitLib = "libil2cpp.so",      // il2cpp 先起；libgame.so 由 TryInit 自己等
        .initDelayMs = 3000000,            // 避开 il2cpp/Lua 半初始化
        .initRetryMax = 0,              // 0 = Tick 懒加载（不死锁启动）
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,
        .tryInit = Jjdm_TryInit,
        .isReady = Jjdm_IsReady,
        .tick = Jjdm_Tick,
        .onFeatureChanged = Jjdm_OnFeatureChanged,
        .onValueChanged = Jjdm_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Jjdm_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
