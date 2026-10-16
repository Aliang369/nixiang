/* ================================================================
 *  GdzfdjEntry.cpp — 敢达争锋对决 现代注册表入口（MainModern.cpp 只认本表）
 *
 *  铁律：MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 永不需要动；
 *        不用的回调留 nullptr，不要造假实现。
 *  构建：Android_gdzfdj.mk（APP_BUILD_SCRIPT=./Android_gdzfdj.mk）
 * ================================================================ */
#include "ModEntry.h"
#include "GdzfdjMod.h"

/* 菜单项（顺序即 featNum；Category_ 占下标，仅供 UI 分组）：
 *   [0] Category_战斗        → 分组标题
 *   [1] Slider_伤害倍率:1:100:1  → 乘法倍率，1 = 关
 *   [2] Toggle_超级定怪       → 跳过 xRole.UpdateAI
 *   [3] Slider_全局加速:1:10:1   → UnityEngine.Time 倍速，1 = 关
 */
static const char* const kGdzfdj_Features[] = {
    "Category_战斗",
    "Slider_伤害倍率:1:100:1",
    "Toggle_超级定怪",
    "Slider_全局加速:1:10:1",
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod kMod = {
        .tag                    = "gdzfdj",
        .authGameId             = "gdzfdj",        /* 后台 games.game_id 须同名（见交付回执） */
        .featureList            = kGdzfdj_Features,
        .featureCount           = (int)(sizeof(kGdzfdj_Features) / sizeof(kGdzfdj_Features[0])),
        .waitLib                = "libil2cpp.so",
        .initDelayMs            = 2000000,
        .initRetryMax           = 120,
        .initRetryIntervalMs    = 2000000,
        .tickIntervalMs         = 5000000,
        .onLoad                 = nullptr,
        .tryInit                = Gdzfdj_TryInit,
        .isReady                = Gdzfdj_IsReady,
        .tick                   = Gdzfdj_Tick,
        .onFeatureChanged       = Gdzfdj_OnFeatureChanged,
        .onValueChanged         = Gdzfdj_OnValueChanged,
        .onInputChanged         = nullptr,
        .forceClear             = Gdzfdj_ForceClear,
        .onApplyPlayPatch       = nullptr,
    };
    return &kMod;
}
