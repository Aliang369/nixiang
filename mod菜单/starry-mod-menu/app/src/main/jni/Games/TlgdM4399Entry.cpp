/* ================================================================
 *  TlgdM4399Entry — 弹力果冻 4399 渠道（com.ginicat.sweet.m4399）注册表入口
 *
 *  功能位与 TlgdTapTapMod.h 的 enum 严格对齐（featNum = 本数组下标）：
 *    0 Category_战斗（占位，仅供 UI 分组）
 *    1 Toggle_秒杀
 *    2 Toggle_全屏秒杀
 *    3 Toggle_无敌
 *    4 Toggle_无限技能
 *
 *  接线要点（2026-10-07 4399 稳定性整改）：
 *   - authGameId="tlgd"（4399 独立 ID；沿用历史 game_id，不复用 TapTap 的 tlgdtt）
 *   - 复用 Games/TlgdTapTapMod.cpp 同一份实现（TapTap 2.4.0 已实测稳定的架构）：
 *       后台线程只置 refreshWanted、战场缓存在解释器 Hook 的游戏线程内刷新、
 *       主目标只改 DamageInfo + 一次原 CreateDamage、全屏补发走 HpHandle 原生桥、
 *       跨桥调用全部异常兜底、敌我 EID 序号快照防撕裂。
 *     唯一渠道差异 = HybridCLR 公共解释器命中点文件偏移，
 *     由 Android_tlgdm4399.mk 的 -DSTARRY_TLGD_M4399 切到 4399 的 0xBAAB0C。
 *   - waitLib="libil2cpp.so"（Unity 2020.3.22 IL2CPP + HybridCLR 热更）
 *   - initRetryMax × initRetryIntervalMs：热更程序集要等登录/进关，窗口给足
 * ================================================================ */
#include "ModEntry.h"
#include "TlgdTapTapMod.h"

// ★编译期护栏（2026-10-07）：本入口只允许被 Android_tlgdm4399.mk 编译。
// 若那个 mk 里的 -DSTARRY_TLGD_M4399 丢了，TlgdTapTapMod.cpp 会退回 TapTap 的
// 0xBABB0C 偏移 → 挂到 4399 包的错误地址 → 进图即崩。宁可编不过，不可错挂。
#ifndef STARRY_TLGD_M4399
#error "TlgdM4399Entry.cpp 必须由 Android_tlgdm4399.mk 编译（缺 -DSTARRY_TLGD_M4399）"
#endif

static const char* kTlgdM4399_Features[] = {
    "Category_战斗",           // 0（占位）
    "Toggle_秒杀",             // 1  TLGDTT_FEAT_KILL
    "Toggle_全屏秒杀",         // 2  TLGDTT_FEAT_AOE
    "Toggle_无敌",             // 3  TLGDTT_FEAT_INV
    "Toggle_无限技能",         // 4  TLGDTT_FEAT_NOCD
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Tlgd",
        .authGameId = "tlgd",
        .featureList = kTlgdM4399_Features,
        .featureCount = (int)(sizeof(kTlgdM4399_Features) / sizeof(kTlgdM4399_Features[0])),

        .waitLib = "libil2cpp.so",
        // ⚠ 单位陷阱（2026-09-11 实测踩坑）：框架 main loop 里是 usleep(initDelayMs)，
        //   字段名带 Ms 但**值按微秒**。填 15000 只等于 15 毫秒 → ART SignalChain abort。
        .initDelayMs = 15000000,         // = 15 秒（微秒！）
        .initRetryMax = 120,             // 120 × 5s = 10 分钟窗口（覆盖登录/进关后热更加载）
        .initRetryIntervalMs = 5000000,  // = 5 秒（微秒）
        .tickIntervalMs = 200000,        // = 200 毫秒（微秒）；数据型 Tick 只做字段读写

        .onLoad = nullptr,
        .tryInit = TlgdTapTap_TryInit,
        .isReady = TlgdTapTap_IsReady,
        .tick = TlgdTapTap_Tick,
        .onFeatureChanged = TlgdTapTap_OnFeatureChanged,
        .onValueChanged = TlgdTapTap_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = TlgdTapTap_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
