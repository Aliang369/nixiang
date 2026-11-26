/* ================================================================
 *  NeonEntry — 霓虹深渊（com.shangyoo.neon）注册表入口
 *
 *  只填一张 StarryGameMod 表（菜单 / 初始化参数 / 回调接线）。
 *
 *  铁律：
 *    - 改菜单、调初始化节奏、增删回调 → 只改本文件
 *    - MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 永不需要动
 *    - 不用的回调留 nullptr，不要造假实现
 *    - featureList 下标 = featNum（Category_ 占下标），与 NeonMod.h 的 enum 同变体对齐
 *
 *  两套菜单（编译期二选一，NEON_VARIANT=dz 即 -DNEON_FULL_MENU=1）：
 *    - 正式版 nhsy    ：8 项（无敌/秒杀/技能无CD/移速/金币/钥匙/彩色钥匙/无限跳跃）
 *    - 定制版 nhsy-dz ：12 项（正式版 8 项 ＋ 攻速/伤害倍率/丝血无敌/全屏攻击）
 * ================================================================
 */
#include "ModEntry.h"
#include "NeonMod.h"

#ifdef NEON_GAME_ID_DZ
#define NEON_AUTH_GAME_ID "nhsy-dz"   // 定制版
#else
#define NEON_AUTH_GAME_ID "nhsy"      // 正式版
#endif

#ifdef NEON_FULL_MENU
static const char* kNeon_Features[] = {
    "Category_战斗",           // 0
    "Toggle_无敌",             // 1
    "Toggle_秒杀",             // 2
    "Toggle_技能无CD",         // 3
    "Slider_移速:1:5",         // 4
    "Slider_攻速:1:5",         // 5
    "Toggle_金币使用不减",     // 6
    "Toggle_钥匙使用不减",     // 7
    "Toggle_彩色钥匙使用不减", // 8
    "Toggle_无限跳跃",         // 9
    "Slider_伤害倍率:1:50",    // 10
    "Toggle_丝血无敌",         // 11
    "Toggle_全屏攻击",         // 12
};
#else
static const char* kNeon_Features[] = {
    "Category_战斗",           // 0
    "Toggle_无敌",             // 1
    "Toggle_秒杀",             // 2
    "Toggle_技能无CD",         // 3
    "Slider_移速:1:5",         // 4
    "Toggle_金币使用不减",     // 5
    "Toggle_钥匙使用不减",     // 6
    "Toggle_彩色钥匙使用不减", // 7
    "Toggle_无限跳跃",         // 8
};
#endif

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = NEON_AUTH_GAME_ID,
        .authGameId = NEON_AUTH_GAME_ID,
        .featureList = kNeon_Features,
        .featureCount = (int)(sizeof(kNeon_Features) / sizeof(kNeon_Features[0])),

        .waitLib = "libil2cpp.so",      // Unity IL2CPP：等 libil2cpp 映射后再挂点
        .initDelayMs = 5000000,         // 5s：等 il2cpp 类表就绪（值单位=微秒）
        .initRetryMax = 120,            // 120 × 2s 启动期重试；失败后 Tick 懒装兜底
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,       // 200ms：开关/滑块同步要跟手

        .onLoad = nullptr,              // 无 EarlyHook：挂点全懒装，过检为运行期改写
        .tryInit = Neon_TryInit,
        .isReady = Neon_IsReady,
        .tick = Neon_Tick,
        .onFeatureChanged = Neon_OnFeatureChanged,
        .onValueChanged = Neon_OnValueChanged,
        .onInputChanged = nullptr,      // 无 InputText/InputValue
        .forceClear = Neon_ForceClear,
        .onApplyPlayPatch = nullptr,    // 无 OTA 偏移热更
    };
    return &mod;
}
