/* ================================================================
 *  无限升级 11.266018 / vc1637 — 专用 native 模块（星空版）
 *
 *  与 12.x 的 WxsjMod 完全隔离：
 *    - 挂点 RVA = 0x2b3f3c4（vc1637 真机运行时映射实锤）
 *    - 内嵌 DLL = targetfuntional_dll_vc1637.bin
 *    - 菜单 featNum 由 WxsjVc1637Entry.cpp 独立注册
 * ================================================================ */
#ifndef STARRY_WXSJ_VC1637_MOD_H
#define STARRY_WXSJ_VC1637_MOD_H

enum WxsjVc1637Feat : int {
    // ★ feat 号 = 菜单 featureList 的**下标**（Category_ 也占位），
    //   任何插入/重排都必须与 WxsjVc1637Entry.cpp 的数组逐行对齐，
    //   并用 04_scripts/无限升级/check_feat_order.py 校验（构建脚本第 0 步会跑）。
    WXSJVC_FEAT_GOD        = 1,  // Toggle_无敌            -> god
    WXSJVC_FEAT_ONESHOT    = 2,  // Toggle_秒杀            -> oneshot
    WXSJVC_FEAT_FREEADS    = 4,  // Toggle_免广告          -> free_ads
    WXSJVC_FEAT_ITEMKEEP   = 5,  // Toggle_物品不减        -> item_keep
    WXSJVC_FEAT_SKILLFREE  = 6,  // Toggle_技能免费升级    -> skill_free
    // ---- 倍率区：开关在输入框上面（2026-11-04 用户定）----
    WXSJVC_FEAT_ADSREWARD_ON = 7,   // Toggle_广告奖励倍数生效 -> ads_reward_on
    WXSJVC_FEAT_ADSREWARD    = 8,   // InputValue_1:广告奖励倍数 -> ads_reward_x
    WXSJVC_FEAT_ITEMDOUBLE_ON = 9,  // Toggle_物品倍数生效     -> item_double_on
    WXSJVC_FEAT_ITEMDOUBLE    = 10, // InputValue_1:物品倍数   -> item_double_x
    WXSJVC_FEAT_MIJINGFREE = 11, // Toggle_无限挑战秘境    -> mijing_free
    WXSJVC_FEAT_SKINUNLOCK = 12, // Toggle_皮肤全解锁      -> skin_unlock
    WXSJVC_FEAT_FUNCUNLOCK = 13, // Toggle_功能全解锁      -> func_unlock
    WXSJVC_FEAT_EXPMULTI_ON = 14,// Toggle_经验倍数生效    -> exp_multi_on
    WXSJVC_FEAT_EXPMULTI    = 15,// InputValue_1:经验倍数  -> exp_multi
    WXSJVC_FEAT_SHOPADS    = 16, // Toggle_商店广告无限    -> shop_ads_unlimited
    WXSJVC_FEAT_ITEMREVERSE = 17,// Toggle_物品使用反增    -> item_reverse
    WXSJVC_FEAT_ITEMREVERSEX = 18,// InputValue_1000:物品反增倍率 -> item_reverse_x
#ifdef WXSJ_ANTIBAN_MENU
    // 正常版（菜单保留「防封」）：防封=19、转生时间=20、新三项 21/22/23
    WXSJVC_FEAT_ANTIBAN = 19,     // Toggle_防封 -> block_report（开=上报/执法屏蔽生效）
    WXSJVC_FEAT_ZHUANSHENG = 20,  // Toggle_转生时间无限制 -> zhuansheng_free
    WXSJVC_FEAT_PROJECTILE = 21,  // Slider_技能投掷物倍数:1:100 -> skill_projectile_x
    WXSJVC_FEAT_SKILLDMG   = 22,  // Slider_技能伤害倍数:1:1000 -> skill_damage_x
    WXSJVC_FEAT_LEGACYFREE = 23,  // Toggle_转生无条件升级 -> zhuan_sheng_free_point
#else
    // 屏蔽版（防封撤出菜单、随 so 常开）：转生时间=19、新三项 20/21/22
    WXSJVC_FEAT_ZHUANSHENG = 19, // Toggle_转生时间无限制 -> zhuansheng_free
    WXSJVC_FEAT_PROJECTILE = 20, // Slider_技能投掷物倍数:1:100 -> skill_projectile_x
    WXSJVC_FEAT_SKILLDMG   = 21, // Slider_技能伤害倍数:1:1000 -> skill_damage_x
    WXSJVC_FEAT_LEGACYFREE = 22, // Toggle_转生无条件升级 -> zhuan_sheng_free_point
#endif
};

bool WxsjVc1637_TryInit();
bool WxsjVc1637_IsReady();
void WxsjVc1637_OnFeatureChanged(int featNum, bool enabled);
void WxsjVc1637_OnValueChanged(int featNum, int value);
void WxsjVc1637_OnInputChanged(int featNum, const char* text);
void WxsjVc1637_ForceClear();

#endif  // STARRY_WXSJ_VC1637_MOD_H
