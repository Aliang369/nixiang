/* ================================================================
 *  NeonEntry — 霓虹深渊（com.shangyoo.neon）注册表入口
 *
 *  职责：只填一张 StarryGameMod 表（菜单 / 初始化参数 / 回调接线）。
 *
 *  铁律：
 *    - 改菜单、调初始化节奏、增删回调 → 只改本文件
 *    - MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 永不需要动
 *    - 不用的回调留 nullptr，不要造假实现
 *    - featureList 下标 = featNum（Category_ 占下标），与 NeonMod.h 的 enum 对齐
 *    - game_id：正式版 "nhsy"（默认）；定制版 "nhsy-dz"（NEON_VARIANT=dz 编译期切换）
 * ================================================================
 */
#include "ModEntry.h"
#include "NeonMod.h"

// game_id：正式版默认 "nhsy"；编译期 -DNEON_GAME_ID_DZ=1（NEON_VARIANT=dz）出定制版 "nhsy-dz"
#ifdef NEON_GAME_ID_DZ
#define NEON_AUTH_GAME_ID "nhsy-dz"
#else
#define NEON_AUTH_GAME_ID "nhsy"
#endif

static const char* kNeon_Features[] = {
    "Category_战斗",           // 0
    "Toggle_无敌",             // 1  NEON_FEAT_INVINCIBLE
    "Toggle_秒杀",             // 2  NEON_FEAT_SECKILL
    "Toggle_技能无CD",         // 3  NEON_FEAT_NO_CD
    "Slider_移速:1:5",         // 4  NEON_FEAT_MOVE_SPEED（1 = 关）
    "Toggle_金币使用不减",     // 5  NEON_FEAT_INF_COIN
    "Toggle_钥匙使用不减",     // 6  NEON_FEAT_INF_KEY
    "Toggle_彩色钥匙使用不减", // 7  NEON_FEAT_INF_GKEY
    "Toggle_无限跳跃",         // 8  NEON_FEAT_INF_JUMP
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = NEON_AUTH_GAME_ID,
        .authGameId = NEON_AUTH_GAME_ID,  // com.shangyoo.neon -> nhsy
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
