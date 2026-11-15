/* ================================================================
 *  Wxsjdz1Entry — 无限升级【断网版】12.10701 / vc1647 注册表入口
 *
 *  独立开号（用户 2026-11-15）：
 *    authGameId = "wxsjdz1"、tag = "wxsjdz1"
 *    ⚠ 不复用 wxsj（官方 12 线 vc1644）、不复用 wxsjdz2（官方 12 线定制 vc1644）。
 *    本包是别人改的第三方「断网/离线」包：包名 com.xingzuodaoshi.infinityOffline / vc1647，
 *    载荷 DLL、挂点 RVA、卡密链路与官方两套**全不相同**，产物/文档/脚本一律带「断网版」标识。
 *
 *  隔离方式与 wxsjdz2 / vc1637 一致：只走 MainModern.cpp 的 GameMod_Get() 注册表，
 *  不改 Main.cpp / MainModern.cpp / GameConfig.h / 公共 Android.mk。
 *
 *  菜单 20 个控件 = 18 项功能 + 2 个 Category_ 分组行（下标即 feat_num，feat 3 占位无功能）。
 *  功能集唯一口径：10_docs/无限升级/wxsjdz1_menu_template.json（收尾须逐项对账）。
 * ================================================================ */
#include "ModEntry.h"
#include "Wxsjdz1Mod.h"

static const char* kWxsjdz1_Features[] = {
    "Category_战斗",                      // 0
    "Toggle_无敌",                        // 1  WXSJ1_FEAT_GOD
    "Toggle_秒杀",                        // 2  WXSJ1_FEAT_ONESHOT
    "Category_功能",                      // 3  （占位，无功能体）
    "Toggle_免广告",                      // 4  WXSJ1_FEAT_FREEADS
    "Toggle_物品不减",                    // 5  WXSJ1_FEAT_ITEMKEEP
    "Toggle_技能免费升级",                // 6  WXSJ1_FEAT_SKILLFREE
    "InputValue_1:广告奖励倍数",          // 7  WXSJ1_FEAT_ADSREWARD -> ads_reward_x（1=原样）
    "InputValue_1:物品倍数",              // 8  WXSJ1_FEAT_ITEMDOUBLE -> item_double_x（1=原样）
    "Toggle_无限挑战秘境",                // 9  WXSJ1_FEAT_MIJINGFREE
    "Toggle_皮肤全解锁",                  // 10 WXSJ1_FEAT_SKINUNLOCK
    "Toggle_功能全解锁",                  // 11 WXSJ1_FEAT_FUNCUNLOCK
    "InputValue_1:经验倍数",              // 12 WXSJ1_FEAT_EXPMULTI -> exp_multi（1=原样）
    // ---- wxsjdz1 独有 4 项（12 线 wxsj / wxsjdz2 菜单均无）----
    "Toggle_商店广告无限",                // 13 WXSJ1_FEAT_SHOPADS -> shop_ads_unlimited
    "Toggle_物品使用反增",                // 14 WXSJ1_FEAT_ITEMREVERSE -> item_reverse
    "InputValue_1000:物品反增倍率",       // 15 WXSJ1_FEAT_ITEMREVX -> item_reverse_x
    "Toggle_转生时间无限制",              // 16 WXSJ1_FEAT_ZHUANSHENG -> zhuansheng_free
    "Slider_技能投掷物倍数:1:100",        // 17 WXSJ1_FEAT_PROJECTILE -> skill_projectile_x
    "Slider_技能伤害倍数:1:1000",         // 18 WXSJ1_FEAT_SKILLDMG   -> skill_damage_x
    "Toggle_转生无条件升级",              // 19 WXSJ1_FEAT_LEGACYFREE -> zhuan_sheng_free_point
};

static bool Wxsjdz1_TryInitEntry(JavaVM* /*vm*/) {
    return Wxsjdz1_TryInit();
}

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "wxsjdz1",
        .authGameId = "wxsjdz1",   // 断网版独立开号：与 wxsj / wxsjdz2 各自一份后台模板
        .featureList = kWxsjdz1_Features,
        .featureCount = (int)(sizeof(kWxsjdz1_Features) / sizeof(kWxsjdz1_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 0,
        .initRetryMax = 600,
        .initRetryIntervalMs = 100000,   // 微秒：600 × 100ms = 60s 等待预算
        .tickIntervalMs = 200000,        // 微秒 = 200ms

        .onLoad = nullptr,
        .tryInit = Wxsjdz1_TryInitEntry,
        .isReady = Wxsjdz1_IsReady,
        .tick = nullptr,
        .onFeatureChanged = Wxsjdz1_OnFeatureChanged,
        .onValueChanged = Wxsjdz1_OnValueChanged,  // feat 18/19 两个 Slider
        .onInputChanged = Wxsjdz1_OnInputChanged,  // feat 7/8/12/15 四个 InputValue
        .forceClear = Wxsjdz1_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
