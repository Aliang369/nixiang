/* ================================================================
 *  YimoEntry — 伊莫 注册表入口（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  职责：只填一张 StarryGameMod 表（菜单 / 初始化参数 / 回调接线）。
 *  铁律：MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 零修改。
 *  featureList 下标 = featNum，与 YimoMod.h 的 enum 对齐。
 *
 *  本作 = Unity IL2CPP 宿主 + xLua 玩法层（C# 侧只是壳，逻辑全在 Lua）。
 *  所以 waitLib 仍填 libil2cpp.so（它先起），libxlua.so 由 Yimo_TryInit 自己 dlopen。
 * ================================================================ */
#include "ModEntry.h"
#include "YimoMod.h"

static const char* kYimo_Features[] = {
    "Category_战斗",                 // 0（占位，不是 featNum）
    "Toggle_精灵攻击秒杀",            // 1  YIMO_FEAT_DMG（开 = 精灵命中上报重发 100 段；关 = 1 段）
    "Slider_移速:1:10:1",            // 2  YIMO_FEAT_SPD（1 = 关闭；过高会与服务器位置校验打架）
    "Toggle_玩家无敌",                // 3  YIMO_FEAT_INV
    "Toggle_无限跳跃",                // 4  YIMO_FEAT_JUMP
    "Toggle_无限体力",                // 5  YIMO_FEAT_STAM
    "Toggle_瞬移采集",                // 6  YIMO_FEAT_TP（跳跃键接管：有采集物就飞过去，没有就正常跳）
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Yimo",
        .authGameId = "ym",             // 后台 games.game_id（伊莫 → ym；已确认未与存量 35 个冲突）
        .featureList = kYimo_Features,
        .featureCount = (int)(sizeof(kYimo_Features) / sizeof(kYimo_Features[0])),

        .waitLib = "libil2cpp.so",      // il2cpp 先起；libxlua.so 由 Yimo_TryInit 自己 dlopen
        .initDelayMs = 3000000,            // 避开 il2cpp / xLua 半初始化（此时 Lua VM 还没建）
        .initRetryMax = 0,              // 0 = Tick 懒加载（不死锁启动；libxlua 起得比 il2cpp 晚）
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,       // 200ms：开关/滑块同步要跟手

        .onLoad = nullptr,
        .tryInit = Yimo_TryInit,
        .isReady = Yimo_IsReady,
        .tick = Yimo_Tick,
        .onFeatureChanged = Yimo_OnFeatureChanged,
        .onValueChanged = Yimo_OnValueChanged,
        .onInputChanged = nullptr,      // 无 InputText/InputValue 菜单项
        .forceClear = Yimo_ForceClear,
        .onApplyPlayPatch = nullptr,    // 无 OTA 偏移热更
    };
    return &mod;
}
