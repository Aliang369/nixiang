/* ================================================================
 *  XxdlEntry — 修仙大陆模拟器（com.sky.xxdl.dlt 1.0.5）菜单注册表
 *
 *  抓点来源：10_docs/修仙大陆模拟器/抓点_20261104.md
 *            （原包 index.jsc 离线解密：XXTEA key 6aMz7p1tlGtr6MeR → 明文 System.register 模块）
 *  落地形态：assets/main.js 追加 JS 功能层（无 native hook），本文件只做菜单与开关通道。
 *  功能口径：资源 / 灵石 / 仙晶 / 体力（斌哥 4 项）＋ 免广告（用户 2026-11-04 追加）。
 *  ⚠️ 框架 featNum = featureList 数组下标（0-based）：必须留一个 Category_ 占下标 0，
 *     否则所有功能整体错位一位（2026-11-04 实测事故：点灵石加的是资源）。
 * ================================================================ */
#include "ModEntry.h"
#include "XxdlMod.h"

static const char* kXxdl_Features[] = {
    "Category_资源",        // 0  分类行占下标 0 ⇒ 功能项 featNum 才是 1..5（框架 featNum = 数组下标，0-based）
    "Button_资源+1000万",   // 1  XXDL_FEAT_RES       base.gold   （按钮：点一次加一次）
    "Button_灵石+1000万",   // 2  XXDL_FEAT_LINGSHI   base.diamond
    "Button_仙晶+100万",    // 3  XXDL_FEAT_XIANJING  base.gem
    "Button_体力回满",      // 4  XXDL_FEAT_TILI      assets.new = assets.max
    "Toggle_免广告",        // 5  XXDL_FEAT_NOAD      激励视频直发奖
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Xxdl",
        .authGameId = "xxdlmnq",   // 修仙大陆模拟器（全名拼音首字母）
        .featureList = kXxdl_Features,
        .featureCount = (int)(sizeof(kXxdl_Features) / sizeof(kXxdl_Features[0])),

        .waitLib = "libcocos.so",       // Cocos Creator 3.x
        .initDelayMs = 0,
        .initRetryMax = 600,
        .initRetryIntervalMs = 100000,
        .tickIntervalMs = 500000,

        .onLoad = nullptr,
        .tryInit = Xxdl_TryInit,
        .isReady = Xxdl_IsReady,
        .tick = Xxdl_Tick,
        .onFeatureChanged = Xxdl_OnFeatureChanged,
        .onValueChanged = Xxdl_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Xxdl_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
