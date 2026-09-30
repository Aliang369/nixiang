/* ================================================================
 *  SlcbEntry — 失落城堡（com.leiting.lostcastle.leiting / 1.26.0.331 / vc231）
 *
 *  引擎：Unity IL2CPP（libil2cpp.so 73,840,888 B）+ xLua
 *  壳  ：网易易盾 NIS 4.22.117（application = com.netease.nis.wrapper.MyApplication）
 *        真实 dex 只在运行时由壳解密为 InMemoryDexFile，APK 内无明文 ⇒ 保留壳注入。
 *
 *  注册表入口：MainModern.cpp 经 GameMod_Get() 读取本表。
 *  MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 零改动。
 * ================================================================ */
#include "ModEntry.h"
#include "SlcbMod.h"

// featureList 下标 = featNum（Category_ 占下标），必须与 SlcbMod.h 的 enum 对齐
static const char* kSlcb_Features[] = {
    "Category_战斗",                 // 0
    "Toggle_无敌",                   // 1  SLCB_FEAT_INV
    "Slider_伤害倍率:1:100:1",       // 2  SLCB_FEAT_DMG（1 = 原版，2..100 = 我方伤害乘数）
    "Toggle_无限技能",               // 3  SLCB_FEAT_SKILL（技能无 CD / 无消耗）

    "Category_资源",                 // 4
    "Toggle_无限金币",               // 5  SLCB_FEAT_GOLD（局内金币 🪙，锁定值 999）
    "Toggle_无限魂石",               // 6  SLCB_FEAT_SOUL（第二货币 = m_soul，紫色旋涡图标，锁定值 999）
    "Toggle_免广告",                 // 7  SLCB_FEAT_NOAD（看广告的奖励直接给，不播广告）
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Slcb",
        .authGameId = "slcb",           // 官方中文名拼音首字母（失落城堡）
        .featureList = kSlcb_Features,
        .featureCount = (int)(sizeof(kSlcb_Features) / sizeof(kSlcb_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 3000,            // 易盾壳解壳 + il2cpp 初始化较慢，给足余量
        .initRetryMax = 600,            // 60s 内每 100ms 探一次
        .initRetryIntervalMs = 100,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,
        .tryInit = Slcb_TryInit,
        .isReady = Slcb_IsReady,
        .tick = Slcb_Tick,
        .onFeatureChanged = Slcb_OnFeatureChanged,
        .onValueChanged = Slcb_OnValueChanged,
        .onInputChanged = nullptr,      // 有 InputText/InputValue 才接
        .forceClear = Slcb_ForceClear,
        .onApplyPlayPatch = nullptr,    // 无 OTA 偏移热更
    };
    return &mod;
}
