/* ================================================================
 *  无限升级 · 星光版 21 槽入口 —— 13.101 / TapTap 官方包
 *  game_id = wxsjdz1（后台「定制1」模板，21 槽）｜tag = wxsjdz1_new（只做日志标识）
 *
 *  ★ 三套既有 wxsj* 一律不动、互不复用 ★
 *    - Games/WxsjMod.cpp        = 13.x legacy 入口（Main.cpp 分支，11 项写死）
 *    - Games/Wxsjdz1Mod.cpp     = **断网版** com.xingzuodaoshi.infinityOffline / vc1647
 *    - Games/WxsjVc1637Mod.cpp  = 11 线 vc1637
 *    本文件 = 官方 TapTap 包（com.xingzuodaoshi.infinityUpgrade）走 **MainModern 注册表** 路线，
 *    载荷/挂点用 13.x 那条链（RVA 0x2b890d8 + selfarray），菜单用 wxsjdz1 的 21 槽文本。
 *    三者载荷、挂点、菜单口径各自独立。
 *
 *  菜单 21 槽 = 19 项功能 + 2 个 Category_ 分组行（下标即 feat_num，0/3 为分组占位）。
 *  唯一口径：10_docs/无限升级/wxsjdz1_menu_template.json（收尾须逐项对账）。
 *
 *  功能开关契约（三套一致）：native 只写 starry_config.ini，
 *  效果由补丁 DLL 内的 StarryConfig（GetBool/GetInt，2s 缓存）读取 —— native 侧禁碰 IL2CPP/UI。
 * ================================================================ */
#ifndef STARRY_WXSJDZ1NEW_MOD_H
#define STARRY_WXSJDZ1NEW_MOD_H

// feat 号 = 菜单 featureList 下标（Category_ 也占位），与 Wxsjdz1NewEntry.cpp 逐行对齐
enum Wxsj1NewFeat : int {
    // 0 = Category_战斗（占位，无功能体）
    WXSJ1NEW_FEAT_GOD         = 1,  // Toggle_无敌                -> god
    WXSJ1NEW_FEAT_ONESHOT     = 2,  // Toggle_秒杀                -> oneshot
    // 3 = Category_功能（占位，无功能体）
    WXSJ1NEW_FEAT_FREEADS     = 4,  // Toggle_免广告              -> free_ads
    WXSJ1NEW_FEAT_ITEMKEEP    = 5,  // Toggle_物品不减            -> item_keep
    WXSJ1NEW_FEAT_SKILLFREE   = 6,  // Toggle_技能免费升级        -> skill_free
    WXSJ1NEW_FEAT_ADSREWARD   = 7,  // InputValue_1:广告奖励倍数  -> ads_reward_x（1=官方原样）
    WXSJ1NEW_FEAT_ITEMDOUBLE  = 8,  // InputValue_1:物品倍数      -> item_double_x（1=官方原样）
    WXSJ1NEW_FEAT_MIJINGFREE  = 9,  // Toggle_无限挑战秘境        -> mijing_free
    WXSJ1NEW_FEAT_SKINUNLOCK  = 10, // Toggle_皮肤全解锁          -> skin_unlock
    WXSJ1NEW_FEAT_FUNCUNLOCK  = 11, // Toggle_功能全解锁          -> func_unlock
    WXSJ1NEW_FEAT_EXPMULTI    = 12, // InputValue_1:经验倍数      -> exp_multi（1=官方原样）
    // ---- 以下 8 项为 21 槽新增（13 项起；对应 IL 补丁由另一子代理产出）----
    WXSJ1NEW_FEAT_SHOPADS     = 13, // Toggle_商店广告无限        -> shop_ads_unlimited
    WXSJ1NEW_FEAT_ITEMREVERSE = 14, // Toggle_物品使用反增        -> item_reverse
    WXSJ1NEW_FEAT_ITEMREVX    = 15, // InputValue_1000:物品反增倍率 -> item_reverse_x
    WXSJ1NEW_FEAT_ZHUANSHENG  = 16, // Toggle_转生时间无限制      -> zhuansheng_free
    WXSJ1NEW_FEAT_PROJECTILE  = 17, // Slider_技能投掷物倍数:1:100  -> skill_projectile_x
    WXSJ1NEW_FEAT_SKILLDMG    = 18, // Slider_技能伤害倍数:1:1000   -> skill_damage_x
    WXSJ1NEW_FEAT_LEGACYFREE  = 19, // Toggle_转生无条件升级      -> zhuan_sheng_free_point
    WXSJ1NEW_FEAT_CHENGHAO    = 20, // Slider_称号等级:1:20        -> chenghao_lv（1..20）
};

bool Wxsj1New_TryInit();
bool Wxsj1New_IsReady();
void Wxsj1New_OnFeatureChanged(int featNum, bool enabled);
void Wxsj1New_OnValueChanged(int featNum, int value);   // Slider：feat 17/18/20
void Wxsj1New_OnInputChanged(int featNum, const char* text);  // InputValue：feat 7/8/12/15
void Wxsj1New_ForceClear();

#endif  // STARRY_WXSJDZ1NEW_MOD_H
