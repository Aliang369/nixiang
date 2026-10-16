/* ================================================================
 *  {Game}Entry — 注册表入口模板（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  生成：new_game_mod.sh 自动复制为 {Game}Entry.cpp 并替换标识。
 *  职责：只填一张 StarryGameMod 表（菜单 / 初始化参数 / 回调接线）。
 *
 *  铁律：
 *    - 改菜单、调初始化节奏、增删回调 → 只改本文件
 *    - MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 永不需要动
 *    - 不用的回调留 nullptr，不要造假实现
 *    - featureList 下标 = featNum（Category_ 占下标），与 Mod 的 enum 对齐
 * ================================================================
 */
#include "ModEntry.h"
#include "JjssjdMod.h"

static const char* kJjssjd_Features[] = {
    "Category_战斗",           // 0（占下标，仅供分组）
    "Toggle_无敌",             // 1  JJSSJD_FEAT_INVULN
    "Slider_伤害倍率:1:100:1", // 2  JJSSJD_FEAT_DMG_MUL（默认 1 = 原版）
    "Toggle_无限技能",         // 3  JJSSJD_FEAT_INF_SKILL
    "Toggle_秒杀",             // 4  JJSSJD_FEAT_KILL
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Jjssjd",
        .authGameId = "jjssjd",        // 境界-死神激斗 → 全名拼音首字母（后台 games.game_id 需同步）
        .featureList = kJjssjd_Features,
        .featureCount = (int)(sizeof(kJjssjd_Features) / sizeof(kJjssjd_Features[0])),

        .waitLib = "libil2cpp.so",     // Unity IL2CPP（游戏逻辑另含 libxlua.so，由 Mod 侧自行 dlopen）
        .initDelayMs = 0,               // 必须 0：Assembly.Load(byte[]) 实测在进程 ~6.6s，越早装 hook 越好
        .initRetryMax = 120,            // 启动期重试上限；死锁案（如 gjc2）改 0 = Tick 懒加载
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = Jjssjd_OnLoad,       // 早装热更 DLL 装载 hook（框架等库是 sleep(1) 粒度，太粗）
        .tryInit = Jjssjd_TryInit,
        .isReady = Jjssjd_IsReady,
        .tick = Jjssjd_Tick,
        .onFeatureChanged = Jjssjd_OnFeatureChanged,
        .onValueChanged = Jjssjd_OnValueChanged,
        .onInputChanged = nullptr,      // 有 InputText/InputValue 才接
        .forceClear = Jjssjd_ForceClear,
        .onApplyPlayPatch = nullptr,    // 有 OTA 偏移热更才接
    };
    return &mod;
}
