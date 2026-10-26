/* ================================================================
 *  Wxsjdz2Entry — 无限升级 12.10701 / vc1644 定制号 wxsjdz2 注册表入口
 *
 *  独立开号（用户 2026-10-27）：
 *    authGameId = wxsjdz2（不复用 12 线旧号 wxsj、也不复用 11 线 wxsjdz1）
 *    ⇒ 各自一份载荷 sha / 卡密 / 后台游戏模板，三线互不影响。
 *
 *  隔离方式与 vc1637 一致：只走 MainModern.cpp 的 GameMod_Get() 注册表，
 *  不改 Main.cpp / MainModern.cpp / GameConfig.h / 公共 Android.mk。
 *
 *  菜单 16 个控件 = 14 项功能 + 2 个 Category_ 分组行（下标即 feat_num）。
 *  功能集口径见 10_docs/无限升级/wxsjdz2_menu_template.json（收尾须逐项对账）。
 * ================================================================ */
#include "ModEntry.h"
#include "Wxsjdz2Mod.h"

static const char* kWxsjdz2_Features[] = {
    "Category_战斗",                   // 0
    "Toggle_无敌",                     // 1  WXSJ2_FEAT_GOD
    "Toggle_秒杀",                     // 2  WXSJ2_FEAT_ONESHOT
    "Category_功能",                   // 3
    "Toggle_免广告",                   // 4  WXSJ2_FEAT_FREEADS
    "Toggle_物品不减",                 // 5  WXSJ2_FEAT_ITEMKEEP
    "Toggle_技能免费升级",             // 6  WXSJ2_FEAT_SKILLFREE
    "Toggle_广告奖励翻倍",             // 7  WXSJ2_FEAT_ADSREWARD（开=ads_reward_x 2，关=1）
    "Toggle_物品翻倍",                 // 8  WXSJ2_FEAT_ITEMDOUBLE
    "Toggle_无限挑战秘境",             // 9  WXSJ2_FEAT_MIJINGFREE
    "Toggle_皮肤全解锁",               // 10 WXSJ2_FEAT_SKINUNLOCK
    "Toggle_功能全解锁",               // 11 WXSJ2_FEAT_FUNCUNLOCK
    "Slider_经验倍率:1:10",            // 12 WXSJ2_FEAT_EXPMULTI（1=官方原样）
    // ---- 2026-10-27 用户点名新增三项 ----
    "Slider_技能投掷物倍数:1:100",     // 13 WXSJ2_FEAT_PROJECTILE（1=官方原样）
    "Slider_技能伤害倍数:1:1000",      // 14 WXSJ2_FEAT_SKILLDMG（1=官方原样）
    "Toggle_转生无条件升级",           // 15 WXSJ2_FEAT_LEGACYFREE（神力属性升级免轮回点）
};

static bool Wxsjdz2_TryInitEntry(JavaVM* /*vm*/) {
    return Wxsjdz2_TryInit();
}

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "wxsjdz2",
        .authGameId = "wxsjdz2",   // 2026-10-27 独立开号：12 线定制版单独一份载荷 sha
        .featureList = kWxsjdz2_Features,
        .featureCount = (int)(sizeof(kWxsjdz2_Features) / sizeof(kWxsjdz2_Features[0])),

        .waitLib = "libil2cpp.so",
        .initDelayMs = 0,
        .initRetryMax = 600,
        .initRetryIntervalMs = 100000,
        .tickIntervalMs = 200000,

        .onLoad = nullptr,
        .tryInit = Wxsjdz2_TryInitEntry,
        .isReady = Wxsjdz2_IsReady,
        .tick = nullptr,
        .onFeatureChanged = Wxsjdz2_OnFeatureChanged,
        .onValueChanged = Wxsjdz2_OnValueChanged,
        .onInputChanged = nullptr,          // 本号三项滑块走 onValueChanged，不用输入框通道
        .forceClear = Wxsjdz2_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
