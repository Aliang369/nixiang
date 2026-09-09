/* ================================================================
 *  SelfEntry.cpp — SELF 自测壳注册表入口（modern 框架验收用）
 *
 *  覆盖原生菜单已实现的全部控件类型（无真实作弊逻辑）。
 *  行为对齐 legacy Main.cpp 的 STARRY_GAME_SELF 分支（game_id=starry_self）。
 *  验收通过后接真实新游戏时参照 _TemplateEntry.cpp。
 * ================================================================
 */
#include "ModEntry.h"

static const char* kSelfFeatures[] = {
    "Category_开关",                     // 0
    "Toggle_测试开关",                   // 1
    "CheckBox_测试勾选",                 // 2
    "Category_滑块",                     // 3
    "Slider_测试滑块:1:10",              // 4
    "SeekBar_测试滑轮_0_100",            // 5
    "Category_选择",                     // 6
    "Spinner_难度:简单,普通,困难",       // 7
    "Category_输入",                     // 8
    "InputText_备注",                    // 9
    "InputValue_100:数量",               // 10
    "Category_操作",                     // 11
    "Button_测试执行",                   // 12
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "self",
        .authGameId = "starry_self",
        .featureList = kSelfFeatures,
        .featureCount = (int)(sizeof(kSelfFeatures) / sizeof(kSelfFeatures[0])),
        .waitLib = nullptr,          // 纯壳：不等待游戏库
        .initDelayMs = 0,
        .initRetryMax = 0,           // 无游戏初始化
        .initRetryIntervalMs = 0,
        .tickIntervalMs = 500000,
        .onLoad = nullptr,
        .tryInit = nullptr,
        .isReady = nullptr,
        .tick = nullptr,
        .onFeatureChanged = nullptr,
        .onValueChanged = nullptr,
        .onInputChanged = nullptr,
        .forceClear = nullptr,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
