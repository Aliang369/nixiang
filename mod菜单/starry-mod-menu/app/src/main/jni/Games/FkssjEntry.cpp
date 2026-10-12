/* ================================================================
 *  FkssjEntry — 疯狂水世界 注册表入口（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  游戏：com.gx.sw.qa.fkssj004.esj  1.11.6 (vc480)  TapTap 渠道原版
 *        （真机 96f0d883 只读拉取，md5 f8ed1b954a3c5ce91dcecb4e721f3e9b）
 *  引擎：Cocos Creator 3.x / libcocos.so（JSB；无 IL2CPP、无整包加固壳）
 *
 *  阶段：① 过环境（菜单组装进游戏 + 去签名）
 *        本入口只交付菜单与生命周期；【无敌】【伤害倍率】【战斗加速】
 *        的实现待用户放行口令后填 FkssjMod.cpp 的 §TODO 区。
 *
 *  铁律：featureList 下标 = featNum（Category_ 占下标），必须与 FkssjMod.h 的 enum 对齐。
 *        MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 零修改。
 * ================================================================ */
#include "ModEntry.h"
#include "FkssjMod.h"

static const char* kFkssj_Features[] = {
    "Category_战斗",              // 0
    "Toggle_无敌",                // 1  FKSSJ_FEAT_INV
    "Slider_伤害倍率:1:100:1",    // 2  FKSSJ_FEAT_DMG（1 = 关闭，2..100 = 倍率）
    "Slider_战斗加速:1:10:1",     // 3  FKSSJ_FEAT_SPD（1 = 关闭，2..10 = 倍速）
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Fkssj",
        .authGameId = "fkssj",          // 疯狂水世界 -> fkssj（官方中文名拼音首字母，全小写）
        .featureList = kFkssj_Features,
        .featureCount = (int)(sizeof(kFkssj_Features) / sizeof(kFkssj_Features[0])),

        .waitLib = "libcocos.so",       // Cocos Creator 3.x 引擎库（包内实测存在，30,429,096 B）
        .initDelayMs = 0,               // 引擎库映射即可判定，无需额外延迟
        .initRetryMax = 120,            // 启动期重试上限
        .initRetryIntervalMs = 2000,
        .tickIntervalMs = 200000,       // 阶段① 零 hook；阶段② JS 桥需要 1s 级轮询再改小

        .onLoad = nullptr,              // 阶段① 无 JNI_OnLoad 期工作（无 {Game}Hooks 类缓存需求）
        .tryInit = Fkssj_TryInit,
        .isReady = Fkssj_IsReady,
        .tick = Fkssj_Tick,
        .onFeatureChanged = Fkssj_OnFeatureChanged,
        .onValueChanged = Fkssj_OnValueChanged,
        .onInputChanged = nullptr,      // 无 InputText/InputValue
        .forceClear = Fkssj_ForceClear,
        .onApplyPlayPatch = nullptr,    // 无 OTA 偏移热更
    };
    return &mod;
}
