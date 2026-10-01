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
 *    - authGameId 保持 "neon"（存量 grandfather，后台 games.game_id 已是 neon）
 * ================================================================
 */
#include "ModEntry.h"
#include "NeonMod.h"

static const char* kNeon_Features[] = {
    "Category_战斗",           // 0
    "Toggle_无敌",             // 1  NEON_FEAT_INVINCIBLE（Hearts.DeductHP 直接返回 false）
    "Toggle_秒杀",             // 2  NEON_FEAT_SECKILL（伤害放大 + 目标 CurrentHp 归零）
    "Toggle_技能无CD",         // 3  NEON_FEAT_NO_CD（IsSkillCD/SetSkillCD/SecCool* 6 处）
    "Slider_移速:1:5",         // 4  NEON_FEAT_MOVE_SPEED（1 = 关，2/3/4/5 → 1.5/2/3/4 倍）
    "Slider_近战攻速:1:5",     // 5  NEON_FEAT_MELEE_SPEED（刀/球棒/拳套/指虎挥砍 CD 缩短）
    "Slider_远程攻速:1:5",     // 6  NEON_FEAT_RANGED_SPEED（枪/弓射击间隔缩短）
    "Toggle_无限金币",         // 7  NEON_FEAT_INF_COIN（对局内金币：UseCoin 拦截 + HasCoin 恒真）
    "Toggle_无限钥匙",         // 8  NEON_FEAT_INF_KEY（对局内钥匙：UseKey 拦截 + HasKey 恒真）
    "Toggle_无限彩色钥匙",     // 9  NEON_FEAT_INF_GKEY（GameplayKey＝彩色钥匙，SpecialKeyPowerUp 拾取）
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "neon",
        .authGameId = "neon",           // com.shangyoo.neon TapTap 渠道；后台 games.game_id = neon
        .featureList = kNeon_Features,
        .featureCount = (int)(sizeof(kNeon_Features) / sizeof(kNeon_Features[0])),

        .waitLib = "libil2cpp.so",      // Unity IL2CPP：等 libil2cpp 映射后再挂点
        .initDelayMs = 5000000,         // 5s：等 il2cpp 类表就绪（值单位=微秒）
        .initRetryMax = 120,            // 120 × 2s 启动期重试；失败后 Tick 懒装兜底
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,       // 200ms：开关/滑块同步要跟手

        .onLoad = nullptr,              // 无 EarlyHook：16 处挂点全懒装，过检为运行期改写
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
