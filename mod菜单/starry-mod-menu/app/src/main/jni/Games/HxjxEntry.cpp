/* ================================================================
 *  HxjxEntry.cpp — 幻想九霄 (hxjx) 注册表入口
 *
 *  Unity IL2CPP + HybridCLR 热更（HotFix.dll 原始字节载入内存，无壳）
 *
 *  菜单（数组下标 = featNum，与 HxjxMod.h 的 HxjxFeat 对齐）：
 *    0  Category_战斗   （Category 占下标，无回调）
 *    1  Toggle_无敌     HXJX_FEAT_INV
 *
 *  实现形态：0 个 Dobby hook —— 无敌 = 后台线程扫 /proc/self/maps 的 rw 段，
 *  按 32 字节唯一签名命中 HotFix.dll 内存副本后改 2 处 IL（Mod.cpp §⑤）。
 *
 *  初始化节奏：initRetryMax=120 × 2s 启动期等 libil2cpp 映射（线程不碰 il2cpp API，
 *  不存在 gjc2 那种继续局死锁面）；库晚到还有 Tick 兜底，不阻塞启动。
 *
 *  铁律：改菜单/初始化节奏/回调只改本文件；
 *        MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 永不需要动。
 * ================================================================ */

#include "ModEntry.h"
#include "HxjxMod.h"

static const char* kHxjx_Features[] = {
    "Category_战斗",      // 0（占位，无回调）
    "Toggle_无敌",        // 1  HXJX_FEAT_INV
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Hxjx",
        .authGameId = "hxjx",           // 幻想九霄：huan xiang jiu xiao → hxjx
        .featureList = kHxjx_Features,
        .featureCount = (int)(sizeof(kHxjx_Features) / sizeof(kHxjx_Features[0])),

        .waitLib = "libil2cpp.so",      // Unity IL2CPP（本作无壳，静态 libil2cpp.so）
        .initDelayMs = 0,               // 不延迟：线程自己等库，越早起越早扫到热更 DLL
        .initRetryMax = 120,            // 120 × 2s 启动期等库；失败由 Tick 懒装兜底
        .initRetryIntervalMs = 2000000, // 2s
        .tickIntervalMs = 200000,       // 200ms

        .onLoad = nullptr,
        .tryInit = Hxjx_TryInit,
        .isReady = Hxjx_IsReady,
        .tick = Hxjx_Tick,
        .onFeatureChanged = Hxjx_OnFeatureChanged,
        .onValueChanged = Hxjx_OnValueChanged,
        .onInputChanged = nullptr,      // 无 InputText/InputValue
        .forceClear = Hxjx_ForceClear,
        .onApplyPlayPatch = nullptr,    // 无 OTA 偏移热更
    };
    return &mod;
}
