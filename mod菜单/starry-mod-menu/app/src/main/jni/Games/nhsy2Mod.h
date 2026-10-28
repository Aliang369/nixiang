/* ================================================================
 *  霓虹深渊2 (nhsy2 / com.veewo.neonabyss2 TapTap 渠道) — 星光菜单模块
 *
 *  引擎：Unity 6000.0.73f1 / IL2CPP metadata v31 / libil2cpp.so
 *  保护：心动 themis（反调试/反模拟器）+ global-metadata 磁盘加密
 *        （运行时才解密）+ 磁盘 so 的 dynsym strtab 亦被加密
 *        ⇒ 拿不到静态 RVA ⇒ 全部走【按名挂点】HookByName
 *        （运行时 dynsym 实测已解密，241 个 il2cpp_* 导出可用；
 *          证据见 10_docs/霓虹深渊2/路线锁定.md）
 *
 *  ⚠️ 本游戏与「霓虹深渊1」（com.shangyoo.neon，另一款游戏、另一套 metadata）
 *      没有任何可复用的挂点：nhsy1 的 IsSkillCD/SetSkillCD/GetRemainCD/
 *      UseCoin/HasCoin/HasKey0/HasKeyN/GetCoinNum 在本游戏 metadata 中
 *      命中数全为 0（已全量核对）。本文件所有挂点一律取自
 *      霓虹深渊2 自己的解密 metadata（/tmp/neonabyss2_dev/meta_v31.dat）。
 *      证据见 10_docs/霓虹深渊2/路线锁定.md
 *
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 * ================================================================ */
#ifndef STARRY_NHSY2_MOD_H
#define STARRY_NHSY2_MOD_H

#include <jni.h>

// 功能位号：必须与 nhsy2Entry.cpp 的 featureList 数组下标一一对应
//
//  2026-10-11 四次定稿（用户要求「彩色钥匙也要的补上」）：共 14 个槽位。
//    - **一功能一开关（1:1）**：手雷 / 金币 / 水晶 / 钥匙 / 彩色钥匙 各自一个开关，互不牵连；
//      资源闸门＝**五通道白名单**（CostType.Bomb / Coin / Crystal / Key + 游戏原生
//      InfiniteKey 通道），其余 CostType 一律走游戏原逻辑 —— 废弃早期「万能闸」。
//    - 「无限钥匙」(KEY)      = CostType.Key 免费 + 数量 9999（HUD 显示数字）。
//    - 「无限彩色钥匙」(CKEY) = 游戏自带 **InfiniteKey / KeySuit（钥匙套装）** 通道：
//      NEONPlayerState::get_HasInfiniteKey() -> true、get_InfiniteKey() -> >0
//      ⇒ HUD 钥匙图标切到 KeyImageSync.infiniteKeySprite（彩色/∞ 钥匙），
//        所有读 HasInfiniteKey 的消耗路径全免。
//      依据：NEON.UI.KeyImageSync{normalSprite, infiniteKeySprite, _infiniteKey}、
//            NEONPlayerState{_keySuitInfiniteKey, SetKeySuitInfiniteKey, ResolveInfiniteKeySources}、
//            NEON.Game.PowerUps.HackerMarkPowerUp.KeySuitPowerUp（钥匙套装）。
//      ⚠️ NHY2 metadata 中**不存在**独立的「彩色钥匙」货币字段/枚举项（全量核对：
//        GameplayKey / ColorKey / SpecialKey / 彩色 / 彩虹 = 0 命中；HUD 属性资源面板
//        attribute-resources 只有 resources1{bomb,coin,key,item} + resources2{crystal,egg,jump,pet}），
//        NHY1 的「彩色钥匙」= GameplayKey 属性机制，本作无对应物。
//    - 移速倍率 / 无限跳 属**战斗**（二次定稿结论）。
//    - 资源槽顺序：解锁标准版(7) → 手雷(8) → 金币(9) → 水晶(10) → 钥匙(11) → 彩色钥匙(12)。
//    - 伤害倍率滑块上限 20 → 100（覆盖斌哥改包「百倍伤害」）。
//    - 「全屏攻击」**已落地**（Attri 属性读点强制，见 nhsy2Mod.cpp §6.6z）。
//    - 「无限体力」用户明确不要 ⇒ 不设槽。
//    - ★ 2026-10-11 五轮定稿：**删除「技能无CD」**（用户原话：「技能无cd没用可以删除了」）
//      ⇒ 功能常量整体前移一位（全屏攻击 4→3，其后依次 -1），总数 14 → 13。
enum nhsy2Feat : int {
    // ---- Category_战斗 (0) ----
    NHSY2_FEAT_DMG  = 1,   // Slider_伤害倍率:1:100:1
    NHSY2_FEAT_INV  = 2,   // Toggle_无敌
    NHSY2_FEAT_FULL = 3,   // Toggle_全屏攻击（Attri 读点强制，§6.6z）
    NHSY2_FEAT_SPD  = 4,   // Slider_移速倍率:1:10:1
    NHSY2_FEAT_JUMP = 5,   // Toggle_无限跳
    NHSY2_FEAT_WLV  = 6,   // Button_武器升级 ★2026-10-29 r31：点一次＝升一级（调游戏自己的 LevelUpWeapon(1,0)）
    // ---- Category_资源 (7)：四项资源各自一个开关，1:1 白名单，互不牵连 ----
    //  ★ 2026-10-27：新增「武器满级」放在战斗分类末尾（下标 6），资源分类整体后移一位
    //    ⇒ 「解锁标准版」回到**槽 8**（硬约束 #3 的固定槽位口径重新对齐）。
    NHSY2_FEAT_STD     = 8,    // Toggle_解锁标准版
    NHSY2_FEAT_BOMB    = 9,    // Toggle_无限手雷    -> CostType.Bomb
    NHSY2_FEAT_COIN    = 10,   // Toggle_无限金币    -> CostType.Coin
    NHSY2_FEAT_CRYSTAL = 11,   // Toggle_无限水晶    -> CostType.Crystal
    NHSY2_FEAT_KEY     = 12,   // Toggle_无限钥匙        -> CostType.Key
    NHSY2_FEAT_CKEY    = 13,   // Toggle_无限彩色钥匙    -> 原生 InfiniteKey(KeySuit) 通道
};

// featureList 数组长度（必须与 nhsy2Entry.cpp 的数组长度一致）
#define NHSY2_FEAT_COUNT 14

// 伤害倍率滑块上限（必须与 Slider_伤害倍率:1:100:1 一致）
#define NHSY2_DMG_MUL_MAX 100

// 签名与 ModEntry.h 回调一致（Unity 系忽略 vm）
bool nhsy2_TryInit(JavaVM* vm);
bool nhsy2_IsReady();
void nhsy2_OnFeatureChanged(int featNum, bool enabled);
void nhsy2_OnValueChanged(int featNum, int value);
void nhsy2_Tick(const bool* features, const int* values);
void nhsy2_ForceClear();

#endif  // STARRY_NHSY2_MOD_H
