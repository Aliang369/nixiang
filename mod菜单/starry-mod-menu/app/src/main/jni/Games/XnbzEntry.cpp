/* ================================================================
 *  XnbzEntry — 仙逆：本尊 注册表入口（MainModern.cpp 经 GameMod_Get 读取）
 *
 *  游戏：com.qqsy.xnbz  0.0.14 (vc14)  TapTap 渠道原版（真机 f9a116a2 拉取）
 *  引擎：Cocos Creator 3.x / libcocos.so（JSB；无 IL2CPP、无加固壳）
 *
 *  阶段：①过环境（菜单组装进游戏 + 去签名 + TapTap 强制网页登录）
 *        本入口只交付菜单与生命周期；【无敌】【伤害倍率】的实现
 *        待用户放行后填 XnbzMod.cpp 的 §TODO 区。
 *
 *  铁律：featureList 下标 = featNum（Category_ 占下标），必须与 XnbzMod.h 的 enum 对齐。
 * ================================================================ */
#include "ModEntry.h"
#include "XnbzMod.h"

static const char* kXnbz_Features[] = {
    "Category_战斗",              // 0
    "Toggle_无敌",                // 1  XNBZ_FEAT_INV
    "Slider_伤害倍率:1:100:1",    // 2  XNBZ_FEAT_DMG（1 = 关闭，2..100 = 倍率）
    "Toggle_重试判胜",            // 3  XNBZ_FEAT_RETRY（服务端判负自动重开，博服务端判胜→真发奖）
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Xnbz",
        .authGameId = "xnbz",           // 仙逆 -> xnbz（官方中文名拼音首字母，全小写）
        .featureList = kXnbz_Features,
        .featureCount = (int)(sizeof(kXnbz_Features) / sizeof(kXnbz_Features[0])),

        .waitLib = "libcocos.so",       // Cocos Creator 3.x 引擎库（包内实测存在）
        .initDelayMs = 0,               // 引擎库映射即可判定，无需额外延迟
        .initRetryMax = 120,            // 启动期重试上限
        .initRetryIntervalMs = 2000,
        .tickIntervalMs = 200000,         // 阶段②：JS 桥需要 1s 级轮询（此前零 hook 用 200000）

        .onLoad = Xnbz_OnLoad,         // JNI_OnLoad 内缓存 XnbzHooks（classloader 可用）
        .tryInit = Xnbz_TryInit,
        .isReady = Xnbz_IsReady,
        .tick = Xnbz_Tick,
        .onFeatureChanged = Xnbz_OnFeatureChanged,
        .onValueChanged = Xnbz_OnValueChanged,
        .onInputChanged = nullptr,      // 无 InputText/InputValue
        .forceClear = Xnbz_ForceClear,
        .onApplyPlayPatch = nullptr,    // 无 OTA 偏移热更
    };
    return &mod;
}
