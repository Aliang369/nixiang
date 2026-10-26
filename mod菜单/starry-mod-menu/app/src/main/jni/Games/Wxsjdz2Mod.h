/* ================================================================
 *  无限升级 wxsjdz2 — 12.10701 / vc1644 定制号（game_id = wxsjdz2）
 *  方案：裸 Dobby hook 热更 dll 装载边界（免 il2cpp API，壳无导出）
 *    RuntimeAssemblyLoader::ExecuteRuntimeAssembly RVA=0x2b890d8（vc1644，
 *    2026-09-22 真机运行时映射重解，16B 序言 fe0f1cf8… 装机即验）
 *  功能：hook 内就地把 targetfuntional.dll 字节替换为内置 patched dll，
 *        功能开关由 dll 内 StarryConfig 读浮窗配置文件（2s 缓存）
 *  feat_num = GameMod_Get().featureList 下标（含 Category 占位）
 *
 *  2026-10-27 独立开号：12 线复刻（A 线 11 项）+ 用户点名新增 3 项。
 *  与 wxsj（12 线旧号）/ wxsjdz1（11 线）互不影响：载荷 sha、卡密、载荷 DLL 各自独立。
 * ================================================================ */
#ifndef STARRY_WXSJDZ2_MOD_H
#define STARRY_WXSJDZ2_MOD_H

// 12 线复刻（A 线既有 11 项，口径见 10_docs/_state/games/无限升级.md A 线段）
// 新增 3 项（2026-10-27 用户点名，落点见 ilpatch_wxsjdz2/Program.cs）
enum Wxsjdz2Feat : int {
    WXSJ2_FEAT_GOD        = 1,  // Toggle_无敌          -> god
    WXSJ2_FEAT_ONESHOT    = 2,  // Toggle_秒杀          -> oneshot
    WXSJ2_FEAT_FREEADS    = 4,  // Toggle_免广告        -> free_ads
    WXSJ2_FEAT_ITEMKEEP   = 5,  // Toggle_物品不减      -> item_keep
    WXSJ2_FEAT_SKILLFREE  = 6,  // Toggle_技能免费升级  -> skill_free
    WXSJ2_FEAT_ADSREWARD  = 7,  // Toggle_广告奖励翻倍  -> ads_reward_x（开=2，关=1）
    WXSJ2_FEAT_ITEMDOUBLE = 8,  // Toggle_物品翻倍      -> item_double
    WXSJ2_FEAT_MIJINGFREE = 9,  // Toggle_无限挑战秘境  -> mijing_free
    WXSJ2_FEAT_SKINUNLOCK = 10, // Toggle_皮肤全解锁    -> skin_unlock
    WXSJ2_FEAT_FUNCUNLOCK = 11, // Toggle_功能全解锁    -> func_unlock
    WXSJ2_FEAT_EXPMULTI   = 12, // Slider_经验倍率:1:10 -> exp_multi（1=原逻辑）
    // ---- 2026-10-27 新增三项 ----
    WXSJ2_FEAT_PROJECTILE = 13, // Slider_技能投掷物倍数:1:100 -> skill_projectile_x（1=原样）
    WXSJ2_FEAT_SKILLDMG   = 14, // Slider_技能伤害倍数:1:1000 -> skill_damage_x（1=原样）
    WXSJ2_FEAT_LEGACYFREE = 15, // Toggle_转生无条件升级      -> zhuan_sheng_free_point（神力属性升级免轮回点）
    // 静默防封（出站 RPC 白名单）无编号、无菜单开关：δ 层无条件生效
};

bool Wxsjdz2_TryInit();
bool Wxsjdz2_IsReady();
void Wxsjdz2_OnFeatureChanged(int featNum, bool enabled);
void Wxsjdz2_OnValueChanged(int featNum, int value);
void Wxsjdz2_ForceClear();

#endif  // STARRY_WXSJDZ2_MOD_H
