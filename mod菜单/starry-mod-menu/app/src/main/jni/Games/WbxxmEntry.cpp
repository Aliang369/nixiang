/* ================================================================
 *  WbxxmEntry — 吾辈小熊猫 注册表入口（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  游戏：吾辈小熊猫 5.0 / com.wbxxm.wbxxm / TapTap 渠道 / minSdk24 targetSdk30
 *  引擎：Cocos Creator 3.7.1（libcocos.so + SystemJS + XXTEA jsc，无 il2cpp）
 *  game_id：wbxxm（全名首字母：吾w 辈b 小x 熊x 猫m）
 *
 *  featureList 下标 = featNum = starry_state.txt 数组下标：
 *    [0] Category_功能（无回调，但占下标）
 *    [1] Toggle_免广告           → starry_state[1]（JS 短路 VideoAdMgr.showVideoAd）
 *
 *  2026-11-13 用户定：只保留免广告一个功能。
 *    [2] 内购(GM)  已删除（删 main.js 分流 + jsc 还原原版 IsGmLogin=false + 本 feat 项）
 *    [3] 无限体力  已删除（此前已删：服务端字段，本地抬高被回滚）
 *  ⇒ 菜单共 1 项（免广告），feat#1 编号不变，starry_state 其余下标保持 0 不用。
 * ================================================================
 */
#include "ModEntry.h"
#include "WbxxmMod.h"

static const char* kWbxxm_Features[] = {
    "Category_功能",              // 0
    "Toggle_免广告",              // 1  WBXXM_FEAT_AD
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Wbxxm",
        .authGameId = "wbxxm",
        .featureList = kWbxxm_Features,
        .featureCount = (int)(sizeof(kWbxxm_Features) / sizeof(kWbxxm_Features[0])),

        // 无 hook 面：不等任何库（nullptr = 直接放行），initRetryMax=0 → 不跑 tryInit 重试循环
        .waitLib = nullptr,
        .initDelayMs = 0,
        .initRetryMax = 0,
        .initRetryIntervalMs = 0,
        .tickIntervalMs = 500000,   // 单位微秒：本作 Tick 为空实现，500ms 足够轻

        .onLoad = nullptr,
        .tryInit = Wbxxm_TryInit,
        .isReady = Wbxxm_IsReady,
        .tick = Wbxxm_Tick,
        .onFeatureChanged = Wbxxm_OnFeatureChanged,
        .onValueChanged = Wbxxm_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Wbxxm_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
