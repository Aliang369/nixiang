/* ================================================================
 *  nhsy2Entry — 霓虹深渊2 (com.veewo.neonabyss2 TapTap 渠道) 注册表入口
 *
 *  职责：只填一张 StarryGameMod 表（菜单 / 初始化参数 / 回调接线）。
 *
 *  铁律：
 *    - 改菜单、调初始化节奏、增删回调 → 只改本文件
 *    - MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 永不需要动
 *    - 不用的回调留 nullptr，不要造假实现
 *    - featureList 下标 = featNum（Category_ 占下标），与 nhsy2Mod.h 的 enum 对齐
 *
 *  挂点说明：本游戏（心动 themis 保护）磁盘 libil2cpp.so 的 dynsym strtab
 *  与 global-metadata 均被加密，静态 RVA 不可得 ⇒ nhsy2Mod.cpp 全部走
 *  按名挂点（HookByName），HookSlot.rva 恒为 0。
 * ================================================================
 */
#include "ModEntry.h"
#include "nhsy2Mod.h"

// 2026-10-11 五轮定稿：只留 2 个分类（战斗 / 资源），共 13 槽。
//   战斗：伤害倍率(1-100) / 无敌 / 全屏攻击 / 移速倍率(1-10) / 无限跳
//   资源：解锁标准版 / 无限手雷 / 无限金币 / 无限水晶 / 无限钥匙 / 无限彩色钥匙
//        （一功能一开关，1:1）
//   ★ 本轮删除「技能无CD」（用户原话：「技能无cd没用可以删除了」）⇒ 下标 3 起整体前移。
//   注：NHY2 无独立「彩色钥匙」货币字段 ⇒ CKEY 走游戏原生 InfiniteKey(KeySuit) 通道
//       （HUD 钥匙图标变彩色/∞，消耗全免），详见 nhsy2Mod.h 头注释。
//   ★ 2026-10-29 r31（用户 m00002）：「武器满级」开关**不要了**，槽 6 换成「武器升级」按钮：
//       点一次「执行」＝游戏自己的 LevelUpWeapon(1,0) ⇒ 升一级（属性由游戏应用）；
//       槽位总数不变（14），「解锁标准版」仍固定槽 8（硬约束 #3）。
static const char* knhsy2_Features[] = {
    // ---- 下标 0..5：战斗 ----
    "Category_战斗",              // 0
    "Slider_伤害倍率:1:100:1",    // 1  NHSY2_FEAT_DMG
    "Toggle_无敌",                // 2  NHSY2_FEAT_INV
    "Toggle_全屏攻击",            // 3  NHSY2_FEAT_FULL（Attri 读点强制，§6.6z）
    "Slider_移速倍率:1:10:1",     // 4  NHSY2_FEAT_SPD
    "Toggle_无限跳",              // 5  NHSY2_FEAT_JUMP
    "Button_武器升级",            // 6  NHSY2_FEAT_WLV  ★2026-10-29 r31：满级开关改为「执行」按钮（点一次升一级）
    // ---- 下标 7..13：资源（一功能一开关，1:1，互不牵连）----
    "Category_资源",              // 7
    "Toggle_解锁标准版",          // 8  NHSY2_FEAT_STD      ← 硬约束 #3 固定槽 8
    "Toggle_无限手雷",            // 9  NHSY2_FEAT_BOMB    CostType.Bomb
    "Toggle_无限金币",            // 10 NHSY2_FEAT_COIN    CostType.Coin
    "Toggle_无限水晶",            // 11 NHSY2_FEAT_CRYSTAL CostType.Crystal
    "Toggle_无限钥匙",            // 12 NHSY2_FEAT_KEY     CostType.Key
    "Toggle_无限彩色钥匙",        // 13 NHSY2_FEAT_CKEY    原生 InfiniteKey(KeySuit)
};
static_assert((int)(sizeof(knhsy2_Features) / sizeof(knhsy2_Features[0])) == NHSY2_FEAT_COUNT,
              "featureList 长度必须等于 NHSY2_FEAT_COUNT");

// ★r31：槽 6 是「按钮」而不是开关 ⇒ 「有开着的功能才装钩」这条懒装策略对它不成立：
//   按钮点击那一刻才装钩，就永远拿不到"点击之前"的武器实例/UI 实例。
//   所以本模块声明「需要开机即装钩」（nhsy2Mod.cpp 的 AnyFeatureOn 会认这个返回值）。
extern "C" int nhsy2_EagerHooks() { return 1; }

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "nhsy2",
        .authGameId = "nhsy2",          // 官方中文全名「霓虹深渊2」拼音首字母；"neon" 已被霓虹深渊1 占用
        .featureList = knhsy2_Features,
        .featureCount = (int)(sizeof(knhsy2_Features) / sizeof(knhsy2_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 5000000,         // 单位=微秒（5s）：等 themis + il2cpp 完成初始化
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000000, // 2s
        .tickIntervalMs = 200000,

        .onLoad = nullptr,
        .tryInit = nhsy2_TryInit,
        .isReady = nhsy2_IsReady,
        .tick = nhsy2_Tick,
        .onFeatureChanged = nhsy2_OnFeatureChanged,
        .onValueChanged = nhsy2_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = nhsy2_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
