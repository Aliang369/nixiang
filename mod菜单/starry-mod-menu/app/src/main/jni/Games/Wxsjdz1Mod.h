/* ================================================================
 *  无限升级 · 断网版 wxsjdz1 — 12.10701 / **vc1647**（第三方断网定制包）
 *  game_id = wxsjdz1｜tag = wxsjdz1｜菜单 16 项（feat 1–16，3 为 Category_ 占位）
 *
 *  ★ 与另外两套严格分开，禁止混用 ★
 *    - wxsj    = 官方 12 线（vc1644）
 *    - wxsjdz2 = 官方 12 线定制号（vc1644，14 项）
 *    - **本文件 = 第三方「断网版」vc1647**（包名 com.xingzuodaoshi.infinityOffline，
 *      versionCode 1647，别的实现改过的包；卡密已由 lead 去掉）
 *    三者的载荷 DLL / 挂点 RVA / 菜单口径 / 后台游戏模板各自独立，互不覆盖。
 *
 *  菜单口径唯一来源：10_docs/无限升级/wxsjdz1_menu_template.json
 *    feat 1/2/4–12 = 12 线既有 11 项；feat 13–16 = 11 线为 wxsjdz1 独有的 4 项
 *    （商店广告无限 / 物品使用反增 / 物品反增倍率 / 转生时间无限制）。
 *  feat_num = GameMod_Get().featureList 下标（含 Category_ 占位），与 Wxsjdz1Entry.cpp 逐行对齐。
 *
 *  ⚠ 本阶段（2026-11-15）= 对账 + 骨架：功能体为 stub + TODO，未挂任何 hook、未内嵌任何载荷。
 *    原因：vc1647 的 libil2cpp.so（md5 917c706a…／115,579,184 B）与 vc1644 不同，
 *    global-metadata.dat 是易盾加密（头 HTPX，非 AF1BB1FA）⇒ 不能静态 dump，
 *    ExecuteRuntimeAssembly 的 RVA 只能运行时重解；官方 vc1644 的 0x2b890d8 **禁止硬套**。
 *    对账结论与缺口清单见 10_docs/无限升级/wxsjdz1适配_断网版_对账_20261115.md。
 * ================================================================ */
#ifndef STARRY_WXSJDZ1_MOD_H
#define STARRY_WXSJDZ1_MOD_H

// feat 号 = 菜单 featureList 下标（Category_ 也占位）
enum Wxsjdz1Feat : int {
    WXSJ1_FEAT_GOD         = 1,  // Toggle_无敌              -> god
    WXSJ1_FEAT_ONESHOT     = 2,  // Toggle_秒杀              -> oneshot
    // 3 = Category_功能（占位，无功能体）
    WXSJ1_FEAT_FREEADS     = 4,  // Toggle_免广告            -> free_ads
    WXSJ1_FEAT_ITEMKEEP    = 5,  // Toggle_物品不减          -> item_keep
    WXSJ1_FEAT_SKILLFREE   = 6,  // Toggle_技能免费升级      -> skill_free
    WXSJ1_FEAT_ADSREWARD   = 7,  // InputValue_1:广告奖励倍数 -> ads_reward_x（1=官方原样）
    WXSJ1_FEAT_ITEMDOUBLE  = 8,  // InputValue_1:物品倍数     -> item_double_x（1=官方原样）
    WXSJ1_FEAT_MIJINGFREE  = 9,  // Toggle_无限挑战秘境      -> mijing_free
    WXSJ1_FEAT_SKINUNLOCK  = 10, // Toggle_皮肤全解锁        -> skin_unlock
    WXSJ1_FEAT_FUNCUNLOCK  = 11, // Toggle_功能全解锁        -> func_unlock
    WXSJ1_FEAT_EXPMULTI    = 12, // InputValue_1:经验倍数     -> exp_multi（1=官方原样）
    // ---- 以下 4 项为 wxsjdz1 独有（12 线菜单无此 4 项）----
    WXSJ1_FEAT_SHOPADS     = 13, // Toggle_商店广告无限      -> shop_ads_unlimited
    WXSJ1_FEAT_ITEMREVERSE = 14, // Toggle_物品使用反增      -> item_reverse
    WXSJ1_FEAT_ITEMREVX    = 15, // InputValue_1000:物品反增倍率 -> item_reverse_x
    WXSJ1_FEAT_ZHUANSHENG  = 16, // Toggle_转生时间无限制    -> zhuansheng_free
};

bool Wxsjdz1_TryInit();
bool Wxsjdz1_IsReady();
void Wxsjdz1_OnFeatureChanged(int featNum, bool enabled);
void Wxsjdz1_OnInputChanged(int featNum, const char* text);
void Wxsjdz1_ForceClear();

#endif  // STARRY_WXSJDZ1_MOD_H
