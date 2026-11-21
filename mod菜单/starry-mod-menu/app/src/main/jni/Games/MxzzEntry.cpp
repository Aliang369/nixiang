/* ================================================================
 *  MxzzEntry — 冒险战争 1.1.036 (vc7, com.mxzz.zbml) 注册表入口
 *
 *  独立于其它游戏：菜单、鉴权 id、初始化节奏、回调全部在这里声明。
 *  MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 均不改。
 *
 *  单位提醒（ModEntry.h）：initDelayMs / initRetryIntervalMs / tickIntervalMs
 *  语义是**微秒**（框架走 usleep）。故 200ms = 200000。
 *
 *  菜单边界（用户/HardConstraints）：**只有两项** —— 秒杀 / 无敌，默认关，可恢复。
 *  多一项即越界（收尾要用 mxzz_menu_template.json 逐项对照）。
 * ================================================================ */
#include "ModEntry.h"
#include "MxzzMod.h"

static const char* kMxzz_Features[] = {
    // ★b6：0 号位必须用 Category_* 占住 —— Java 侧 featNum 是**数组下标（0 基）**
    //   （NativeMenuController.java:699 FeatureListParser.parse(i, specs[i]) → Item(index,…)），
    //   MainModern.cpp:290 原样透传不 +1。缺占位时 秒杀=0/无敌=1，而常量是 KILL=1/GOD=2
    //   ⇒ 开「无敌」发 1 命中 KILL（反而秒杀）、开「秒杀」发 0 落到 default（无效果）。
    //   与 HkldEntry.cpp 同形（其首项 "Category_功能" // 0）。
    "Category_功能",  // 0  占位，仅供 UI 分组，不参与功能索引
    "Toggle_秒杀",    // 1  MXZZ_FEAT_KILL -> kill
    "Toggle_无敌",    // 2  MXZZ_FEAT_GOD  -> god
};

extern "C" const StarryGameMod* const GameMod_Get() {
    static const StarryGameMod mod = {
        .tag = "Mxzz",
        .authGameId = "mxzz",

        .featureList = kMxzz_Features,
        .featureCount = (int)(sizeof(kMxzz_Features) / sizeof(kMxzz_Features[0])),

        .waitLib = "libil2cpp.so",
        // b4：**立即起线程**——装 il2cpp_init 钩只是解析 native 符号 + DobbyHook，
        //     不调用任何 il2cpp API，早期执行天然安全；真正的 il2cpp 调用由「il2cpp_init 已返回」事件把关。
        //     （若这里加延迟，可能错过 il2cpp_init：verifier 实测 libil2cpp JNI_OnLoad 在 so 加载后 ~0.5s）
        .initDelayMs = 0,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000000, // 2s（微秒）：失败后由内层 500ms 探测线程兜住
        .tickIntervalMs = 200000,       // 200ms（微秒）

        .onLoad = nullptr,
        .tryInit = Mxzz_TryInit,
        .isReady = Mxzz_IsReady,
        .tick = Mxzz_Tick,
        .onFeatureChanged = Mxzz_OnFeatureChanged,
        .onValueChanged = Mxzz_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Mxzz_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
}
