/* ================================================================
 *  无限升级 · 星光版 **22 槽「带防封按钮」变体**（Full）—— 13.101 / TapTap 官方包
 *  game_id = wxsjdz1（后台「定制1」模板）｜tag = wxsjdz1_full（只做日志标识）
 *
 *  ★ 与 Wxsjdz1New（fix1，21 槽，防封随 so 常开）的关系 ★
 *    唯一差异：菜单末尾多一个 `Toggle_防封`（feat 21 → 键 `block_report`），
 *    且**初始化不再无条件写 block_report=1**；其余 19 项键、挂点、载荷、三元组完全一致。
 *    两个包**并存**，用户按需安装（New＝防封恒开、无按钮；Full＝用户自己开关）。
 *
 *  默认值口径：`block_report` 缺键 = **0（关）**，与 11 线 vc1637 正常版一致
 *  （vc1637 注释：「正常版：防封由菜单开关控制（缺键=false=关，与原定制版一致），这里不强制写」）。
 *  要让默认开：在 Wxsjdz1FullMod.cpp 的 TryHookOnce 里那一行
 *  `// [default-off] 不写 block_report` 后加 `WriteConfigKey("block_report", "1");`（日志已打印 exists 便于判定）。
 *
 *  ★ 三套既有 wxsj* 一律不动、互不复用 ★
 *    - Games/WxsjMod.cpp        = 13.x legacy 入口（Main.cpp 分支，11 项写死）
 *    - Games/Wxsjdz1Mod.cpp     = **断网版** com.xingzuodaoshi.infinityOffline / vc1647
 *    - Games/WxsjVc1637Mod.cpp  = 11 线 vc1637
 *    本文件 = 官方 TapTap 包（com.xingzuodaoshi.infinityUpgrade）走 **MainModern 注册表** 路线，
 *    载荷/挂点用 13.x 那条链（RVA 0x2b890d8 + selfarray），菜单用 wxsjdz1 的 21 槽文本。
 *    三者载荷、挂点、菜单口径各自独立。
 *
 *  菜单 22 槽 = 20 项功能 + 2 个 Category_ 分组行（下标即 feat_num，0/3 为分组占位）。
 *  feat 0–20 与 Wxsjdz1NewEntry.cpp **逐项逐字一致**；feat 21 = `Toggle_防封`（本变体追加）。
 *  对账口径：New 的 wxsjdz1_menu_template.json + 末尾多 `Toggle_防封`。
 *
 *  功能开关契约（三套一致）：native 只写 starry_config.ini，
 *  效果由补丁 DLL 内的 StarryConfig（GetBool/GetInt，2s 缓存）读取 —— native 侧禁碰 IL2CPP/UI。
 * ================================================================ */
#ifndef STARRY_WXSJDZ1FULL_MOD_H
#define STARRY_WXSJDZ1FULL_MOD_H

// feat 号 = 菜单 featureList 下标（Category_ 也占位），与 Wxsjdz1FullEntry.cpp 逐行对齐
enum Wxsj1FullFeat : int {
    // 0 = Category_战斗（占位，无功能体）
    WXSJ1FULL_FEAT_GOD         = 1,  // Toggle_无敌                -> god
    WXSJ1FULL_FEAT_ONESHOT     = 2,  // Toggle_秒杀                -> oneshot
    // 3 = Category_功能（占位，无功能体）
    WXSJ1FULL_FEAT_FREEADS     = 4,  // Toggle_免广告              -> free_ads
    WXSJ1FULL_FEAT_ITEMKEEP    = 5,  // Toggle_物品不减            -> item_keep
    WXSJ1FULL_FEAT_SKILLFREE   = 6,  // Toggle_技能免费升级        -> skill_free
    WXSJ1FULL_FEAT_ADSREWARD   = 7,  // InputValue_1:广告奖励倍数  -> ads_reward_x（1=官方原样）
    WXSJ1FULL_FEAT_ITEMDOUBLE  = 8,  // InputValue_1:物品倍数      -> item_double_x（1=官方原样）
    WXSJ1FULL_FEAT_MIJINGFREE  = 9,  // Toggle_无限挑战秘境        -> mijing_free
    WXSJ1FULL_FEAT_SKINUNLOCK  = 10, // Toggle_皮肤全解锁          -> skin_unlock
    WXSJ1FULL_FEAT_FUNCUNLOCK  = 11, // Toggle_功能全解锁          -> func_unlock
    WXSJ1FULL_FEAT_EXPMULTI    = 12, // InputValue_1:经验倍数      -> exp_multi（1=官方原样）
    // ---- 以下 8 项为 21 槽新增（13 项起；对应 IL 补丁由另一子代理产出）----
    WXSJ1FULL_FEAT_SHOPADS     = 13, // Toggle_商店广告无限        -> shop_ads_unlimited
    WXSJ1FULL_FEAT_ITEMREVERSE = 14, // Toggle_物品使用反增        -> item_reverse
    WXSJ1FULL_FEAT_ITEMREVX    = 15, // InputValue_1000:物品反增倍率 -> item_reverse_x
    WXSJ1FULL_FEAT_ZHUANSHENG  = 16, // Toggle_转生时间无限制      -> zhuansheng_free
    WXSJ1FULL_FEAT_PROJECTILE  = 17, // Slider_技能投掷物倍数:1:100  -> skill_projectile_x
    WXSJ1FULL_FEAT_SKILLDMG    = 18, // Slider_技能伤害倍数:1:1000   -> skill_damage_x
    WXSJ1FULL_FEAT_LEGACYFREE  = 19, // Toggle_转生无条件升级      -> zhuan_sheng_free_point
    WXSJ1FULL_FEAT_CHENGHAO    = 20, // Slider_称号等级:1:20        -> chenghao_lv（1..20）
    // ---- 22 槽「带防封按钮」变体独有（New 版没有这一项）----
    WXSJ1FULL_FEAT_ANTIBAN     = 21, // Toggle_防封 -> block_report（开=上报/执法屏蔽生效；缺键=关）
};

bool Wxsj1Full_TryInit();
bool Wxsj1Full_IsReady();
void Wxsj1Full_OnFeatureChanged(int featNum, bool enabled);
void Wxsj1Full_OnValueChanged(int featNum, int value);   // Slider：feat 17/18/20
void Wxsj1Full_OnInputChanged(int featNum, const char* text);  // InputValue：feat 7/8/12/15
void Wxsj1Full_ForceClear();

#endif  // STARRY_WXSJDZ1FULL_MOD_H
