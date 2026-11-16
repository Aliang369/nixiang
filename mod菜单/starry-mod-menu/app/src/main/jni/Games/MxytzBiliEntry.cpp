/* ================================================================
 *  MxytzBiliEntry.cpp — 冒险与征途 B站渠道 (com.my.mxyzt.bilibili 2.0.1 / vc201) 注册表入口
 *  底包：02_apks/冒险与征途/冒险与征途_bili_2.0.1_原版.apk
 *        （md5 56128ac55012c7a9d3cd881776777170，907,126,398 B，仅 arm64-v8a）
 *  Unity IL2CPP 2022.3.14f1 + HybridCLR 热更（解释执行）。
 *  ★与 TapTap 1.5.9 的唯一差异：tag / authGameId = mxyztbili（渠道独立 id，
 *    禁复用 mxyzt，见 AGENTS.md §4 与 skill/starry-mod/SKILL.md:304）；
 *    featureList 四项（顺序与文案）与 MxytzEntry.cpp **逐字一致**（featNum 是菜单硬契约）。
 *  ★libil2cpp 三个挂点 RVA 与 1.5.9 完全一致（Δ=0），故 MxytzMod.cpp / MxytzBattle.cpp
 *    按 RVA 常量直接沿用；证据见 10_docs/冒险与征途/bili_2.0.1_native挂点.md。
 *
 *  菜单（数组下标 = featNum，与 MxytzMod.h 的 MxytzFeat 一一对齐）：
 *    0  Category_战斗
 *    1  Toggle_秒杀小怪              MXYTZ_FEAT_KILL_MOB
 *    2  Toggle_无敌                  MXYTZ_FEAT_INVINCIBLE
 *    3  Toggle_全模式技能伤害增加     MXYTZ_FEAT_SKILL_DMG
 *    4  Toggle_免广告                MXYTZ_FEAT_NO_AD
 *
 *  初始化节奏：initRetryMax = 0 → 跳过启动期后台轮询。
 *    参照 gjc2（弓箭传说2 HybridCLR）2026-08-30 实锤：加载/恢复窗口里后台扫
 *    HotFix/Il2Cpp 会与主线程死锁卡死；改为用户打开菜单勾选功能后由 Tick 懒装。
 *
 *  铁律：改菜单、改初始化节奏、增删回调 → 只改本文件；
 *        MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 永不需要动。
 * ================================================================ */
#include "ModEntry.h"
#include "MxytzMod.h"

static const char* kMxytz_Features[] = {
    "Category_战斗",              // 0（占位）
    "Toggle_秒杀小怪",            // 1  MXYTZ_FEAT_KILL_MOB
    "Toggle_无敌",                // 2  MXYTZ_FEAT_INVINCIBLE
    "Toggle_全模式技能伤害增加",   // 3  MXYTZ_FEAT_SKILL_DMG
    "Toggle_免广告",              // 4  MXYTZ_FEAT_NO_AD
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "mxyztbili",
        .authGameId = "mxyztbili",      // 后台 games.game_id（冒险与征途 B站渠道 = mxyzt + bili）
        .featureList = kMxytz_Features,
        .featureCount = (int)(sizeof(kMxytz_Features) / sizeof(kMxytz_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 0,
        .initRetryMax = 0,              // 禁止启动期 init（启动/加载窗死锁案，见 gk 注释）
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,       // 200ms

        .onLoad = nullptr,
        .tryInit = Mxytz_TryInit,
        .isReady = Mxytz_IsReady,
        .tick = Mxytz_Tick,
        .onFeatureChanged = Mxytz_OnFeatureChanged,
        .onValueChanged = Mxytz_OnValueChanged,
        .onInputChanged = nullptr,      // 本作四项均为开关，无输入框
        .forceClear = Mxytz_ForceClear,
        .onApplyPlayPatch = nullptr,    // 本作无 OTA 偏移热更
    };
    return &mod;
}
