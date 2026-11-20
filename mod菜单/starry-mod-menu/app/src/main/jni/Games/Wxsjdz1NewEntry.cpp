/* ================================================================
 *  Wxsjdz1NewEntry — 无限升级 星光版 21 槽注册表入口（13.101 / TapTap 官方包）
 *
 *  身份：
 *    tag        = "wxsjdz1_new"（只做日志/调试标识）
 *    authGameId = "wxsjdz1"    （后台「定制1」模板 game_id，21 槽按它取）
 *    包名       = com.xingzuodaoshi.infinityUpgrade（TapTap 官方包；包名不写死在本文件，
 *                 由 dex 侧 ModTarget 决定，这里只提供鉴权号与菜单表）
 *
 *  隔离方式（与 Wxsjdz1Entry / vc1637Entry 一致）：只走 MainModern.cpp 的 GameMod_Get()
 *  注册表，不改 Main.cpp / MainModern.cpp / GameConfig.h / 公共 Android.mk；
 *  也不复用 wxsj（13.x legacy 11 项）、wxsjdz1（断网版 com.xingzuodaoshi.infinityOffline）、
 *  wxsjdz2（12 线定制）的载荷与挂点。
 *
 *  菜单 21 槽 = 19 项功能 + 2 个 Category_ 分组行（下标即 feat_num，0/3 为分组占位，无功能体）。
 *  功能集唯一口径：10_docs/无限升级/wxsjdz1_menu_template.json（收尾须逐项对账）。
 *  21 槽文本与 Games/Wxsjdz1Entry.cpp:20-41 逐字一致（那条是断网版的，只是文本可抄）。
 * ================================================================ */
#include "ModEntry.h"
#include "Wxsjdz1NewMod.h"

static const char* kWxsj1New_Features[] = {
    "Category_战斗",                      // 0  （分组行，无功能体）
    "Toggle_无敌",                        // 1  WXSJ1NEW_FEAT_GOD
    "Toggle_秒杀",                        // 2  WXSJ1NEW_FEAT_ONESHOT
    "Category_功能",                      // 3  （分组行，无功能体）
    "Toggle_免广告",                      // 4  WXSJ1NEW_FEAT_FREEADS
    "Toggle_物品不减",                    // 5  WXSJ1NEW_FEAT_ITEMKEEP
    "Toggle_技能免费升级",                // 6  WXSJ1NEW_FEAT_SKILLFREE
    "InputValue_1:广告奖励倍数",          // 7  WXSJ1NEW_FEAT_ADSREWARD  -> ads_reward_x（1=原样）
    "InputValue_1:物品倍数",              // 8  WXSJ1NEW_FEAT_ITEMDOUBLE -> item_double_x（1=原样）
    "Toggle_无限挑战秘境",                // 9  WXSJ1NEW_FEAT_MIJINGFREE
    "Toggle_皮肤全解锁",                  // 10 WXSJ1NEW_FEAT_SKINUNLOCK
    "Toggle_功能全解锁",                  // 11 WXSJ1NEW_FEAT_FUNCUNLOCK
    "InputValue_1:经验倍数",              // 12 WXSJ1NEW_FEAT_EXPMULTI   -> exp_multi（1=原样）
    "Toggle_商店广告无限",                // 13 WXSJ1NEW_FEAT_SHOPADS    -> shop_ads_unlimited
    "Toggle_物品使用反增",                // 14 WXSJ1NEW_FEAT_ITEMREVERSE -> item_reverse
    "InputValue_1000:物品反增倍率",       // 15 WXSJ1NEW_FEAT_ITEMREVX   -> item_reverse_x
    "Toggle_转生时间无限制",              // 16 WXSJ1NEW_FEAT_ZHUANSHENG -> zhuansheng_free
    "Slider_技能投掷物倍数:1:100",        // 17 WXSJ1NEW_FEAT_PROJECTILE -> skill_projectile_x
    "Slider_技能伤害倍数:1:1000",         // 18 WXSJ1NEW_FEAT_SKILLDMG   -> skill_damage_x
    "Toggle_转生无条件升级",              // 19 WXSJ1NEW_FEAT_LEGACYFREE -> zhuan_sheng_free_point
    "Slider_称号等级:1:20",               // 20 WXSJ1NEW_FEAT_CHENGHAO   -> chenghao_lv（1-20）
};

static bool Wxsjdz1New_TryInitEntry(JavaVM* /*vm*/) {
    return Wxsj1New_TryInit();
}

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "wxsjdz1_new",       // 日志标识（与断网版 wxsjdz1 区分）
        .authGameId = "wxsjdz1",    // 后台「定制1」21 槽模板
        .featureList = kWxsj1New_Features,
        .featureCount = (int)(sizeof(kWxsj1New_Features) / sizeof(kWxsj1New_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 0,
        .initRetryMax = 600,
        .initRetryIntervalMs = 100000,   // 微秒：600 × 100ms = 60s 等待预算
        .tickIntervalMs = 200000,        // 微秒 = 200ms

        .onLoad = nullptr,
        .tryInit = Wxsjdz1New_TryInitEntry,
        .isReady = Wxsj1New_IsReady,
        .tick = nullptr,
        .onFeatureChanged = Wxsj1New_OnFeatureChanged,
        .onValueChanged = Wxsj1New_OnValueChanged,  // feat 17/18/20 三个 Slider
        .onInputChanged = Wxsj1New_OnInputChanged,  // feat 7/8/12/15 四个 InputValue
        .forceClear = Wxsj1New_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
