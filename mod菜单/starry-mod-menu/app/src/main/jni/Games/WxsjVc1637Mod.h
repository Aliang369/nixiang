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
    WXSJVC_FEAT_GOD        = 1,  // Toggle_无敌          -> god
    WXSJVC_FEAT_ONESHOT    = 2,  // Toggle_秒杀          -> oneshot
    WXSJVC_FEAT_FREEADS    = 4,  // Toggle_免广告        -> free_ads
    WXSJVC_FEAT_ITEMKEEP   = 5,  // Toggle_物品不减      -> item_keep
    WXSJVC_FEAT_SKILLFREE  = 6,  // Toggle_技能免费升级  -> skill_free
    WXSJVC_FEAT_ADSREWARD  = 7,  // InputValue_1:广告奖励倍数  -> ads_reward_x（N 倍，缺省 1=不放大）
    WXSJVC_FEAT_ITEMDOUBLE = 8,  // InputValue_1:物品倍数      -> item_double_x（N 倍，缺省 1=不放大）
    WXSJVC_FEAT_MIJINGFREE = 9,  // Toggle_无限挑战秘境  -> mijing_free
    WXSJVC_FEAT_SKINUNLOCK = 10, // Toggle_皮肤全解锁    -> skin_unlock
    WXSJVC_FEAT_FUNCUNLOCK = 11, // Toggle_功能全解锁    -> func_unlock
    WXSJVC_FEAT_EXPMULTI   = 12, // InputValue_1:经验倍数      -> exp_multi（N 倍，缺省 1=原逻辑）
    WXSJVC_FEAT_SHOPADS    = 13, // Toggle_商店广告无限  -> shop_ads_unlimited
    WXSJVC_FEAT_ITEMREVERSE = 14, // Toggle_物品使用反增 -> item_reverse（使用 N 个反增 N×倍率）
    WXSJVC_FEAT_ITEMREVERSEX = 15, // InputValue_1000:物品反增倍率 -> item_reverse_x（缺省 1000，1..1000000）
    // 16 = 转生时间无限制（防封已撤出菜单，改为随 so 无条件生效的基线，见 .cpp）
    WXSJVC_FEAT_ZHUANSHENG = 16, // Toggle_转生时间无限制 -> zhuansheng_free（开=取消转生 1 小时客户端冷却）
};

bool WxsjVc1637_TryInit();
bool WxsjVc1637_IsReady();
void WxsjVc1637_OnFeatureChanged(int featNum, bool enabled);
void WxsjVc1637_OnValueChanged(int featNum, int value);
void WxsjVc1637_OnInputChanged(int featNum, const char* text);
void WxsjVc1637_ForceClear();

#endif  // STARRY_WXSJ_VC1637_MOD_H
