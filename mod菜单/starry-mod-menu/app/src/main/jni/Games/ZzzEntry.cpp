/* ================================================================
 *  ZzzEntry — 绝区零 注册表入口（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  游戏：绝区零 com.miHoYo.Nap 3.2.0 (vc429)，arm64-v8a，Unity IL2CPP
 *  菜单四项（2026-10-07 用户「移动速度功能删除不需要改了」后定稿，禁扩张）：
 *    0 Toggle_无敌          池鸳同款站点 file 0x1151d138
 *    1 Toggle_无限技能      v1 站点未定稿（只记开关）
 *    2 Slider_伤害倍率:1:100:1
 *    3 Toggle_定怪          BehaviorManager.Tick file 0x19ce2164
 *
 *  铁律：改菜单/调初始化节奏只改本文件；featureList 下标 = featNum（与 ZzzMod.h 对齐）；
 *        滑块 1 = 关闭，上限必须与 ZzzMod.h 的 *_MUL_MAX 一致。
 * ================================================================ */
#include "ModEntry.h"
#include "ZzzMod.h"

static const char* kZzz_Features[] = {
    "Toggle_无敌",              // 0  ZZZ_FEAT_INV
    "Toggle_无限技能",          // 1  ZZZ_FEAT_SKILL
    "Slider_伤害倍率:1:100:1",  // 2  ZZZ_FEAT_DMG    上限 = ZZZ_DMG_MUL_MAX
    "Toggle_定怪",              // 3  ZZZ_FEAT_FREEZE
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "jql",
        .authGameId = "jql",            // 后台 games.game_id（绝区零拼音首字母），需运营侧注册
        .featureList = kZzz_Features,
        .featureCount = (int)(sizeof(kZzz_Features) / sizeof(kZzz_Features[0])),

        .waitLib = "libil2cpp.so",
        // ⚠️ MainModern 直接 usleep()，本组字段实际是【微秒】（名字带 Ms 是历史误称）：
        //   旧值 initRetry=2000＝2ms（120 次 0.24s 烧完，形同虚设）、tick=5000＝5ms＝200Hz。
        //   R20 全关态冻死现场：WatchHooks 按 Tick 频率碰 dl_iterate_phdr（loader 锁）⇒ 降频。
        .initDelayMs = 2000000,            // 2ms：历史值，仅首轮让路，本轮不动
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000000, // 2s/次（意图值；旧 2000＝2ms）
        .tickIntervalMs = 200000,       // 200ms（旧 5000＝5ms＝200Hz，高频碰 loader 锁）

        .onLoad = nullptr,              // 无过检补丁/无 early dex 注入
        .tryInit = Zzz_TryInit,
        .isReady = Zzz_IsReady,
        .tick = Zzz_Tick,
        .onFeatureChanged = Zzz_OnFeatureChanged,
        .onValueChanged = Zzz_OnValueChanged,
        .onInputChanged = nullptr,      // 无 InputText
        .forceClear = Zzz_ForceClear,
        .onApplyPlayPatch = nullptr,    // 无 OTA 偏移热更
    };
    return &mod;
}
