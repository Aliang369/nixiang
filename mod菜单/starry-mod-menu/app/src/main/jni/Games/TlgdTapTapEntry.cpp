/* ================================================================
 *  TlgdTapTapEntry — 弹力果冻 TapTap 渠道（com.ginicat.sweet）注册表入口
 *
 *  功能位与 TlgdTapTapMod.h 的 enum 严格对齐（featNum = 本数组下标）：
 *    0 Category_战斗（占位，仅供 UI 分组）
 *    1 Toggle_秒杀
 *    2 Toggle_全屏秒杀
 *    3 Toggle_无敌
 *    4 Toggle_无限技能
 *
 *  接线要点：
 *   - authGameId="tlgdtt"（TapTap 独立 ID；4399 历史 tlgd 不复用）
 *   - waitLib="libil2cpp.so"（Unity 2020.3.22 IL2CPP + HybridCLR 热更）
 *   - initRetryMax × initRetryIntervalMs：热更程序集要等登录/进关，窗口给足
 *   - 秒杀/全屏/无敌按 4399 2026-08-24 路线挂 HpHandle 桥；无限技能保持原数据链
 * ================================================================ */
#include "ModEntry.h"
#include "TlgdTapTapMod.h"

static const char* kTlgdTapTap_Features[] = {
    "Category_战斗",           // 0（占位）
    "Toggle_秒杀",             // 1  TLGDTT_FEAT_KILL
    "Toggle_全屏秒杀",         // 2  TLGDTT_FEAT_AOE
    "Toggle_无敌",             // 3  TLGDTT_FEAT_INV
    "Toggle_无限技能",         // 4  TLGDTT_FEAT_NOCD
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Tlgd",
        .authGameId = "tlgdtt",
        .featureList = kTlgdTapTap_Features,
        .featureCount = (int)(sizeof(kTlgdTapTap_Features) / sizeof(kTlgdTapTap_Features[0])),

        .waitLib = "libil2cpp.so",
        // ⚠ 单位陷阱（2026-09-11 实测踩坑）：框架 main loop 里是 usleep(initDelayMs)，
        //   字段名带 Ms 但**值按微秒**。原先填 15000 只等于 15 毫秒 → libil2cpp 刚 JNI_OnLoad、
        //   Unity 运行时还在初始化（MemoryManager 日志晚 80ms）时就去 attach/读元数据 →
        //   libunity.so 段错误 → ART SignalChain 判定无人接管 → abort（进程猝死 uptime 0s）。
        //   参照已跑通的 HybridCLR 游戏魂坠深境：initDelayMs=3000000 = 3 秒。
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
