/* 动物大联盟 — modern 注册表入口 */
#include "ModEntry.h"
#include "DwdlmMod.h"
#include "DwdlmSecShell.h"

// ＝＝ 启动链：两种构建由 DWDLM_ENV_ONLY 在**编译期**二选一（不是运行期 if）＝＝
//   ENV  ：只起 Games/DwdlmSecShell.cpp 的独立看门狗（1.0.25 从 libsecret 导出符号
//          序言反推状态字）。InstallPathProbe / PrepareSecureShell / StartEarlyHookThread
//          这三条 1.0.23 硬地址链在 ENV 编译里**连调用点都不存在**。
//   FULL ：1.0.23 完整壳链，保留给功能阶段按 1.0.25 偏移重新适配后再用。
static void Dwdlm_OnLoad(JavaVM* vm) {
#if defined(DWDLM_ENV_ONLY)
    (void)vm;                        // 看门狗不需要 JavaVM，也不注册任何 JNI 回调
    Dwdlm_LogEnvOnlyBanner("onLoad");
    DwdlmSecShell_Start();           // 幂等；只 pthread_create 后台线程后立刻返回（主线程零等待）
#else
#ifndef DWDLM_SHELL_ONLY
    Dwdlm_InstallPathProbe();
#endif
    // 幂等，只调一次：Dwdlm_PrepareSecureShell 以 g_origSecShellRequire 作一次性门闩
    // （Java 侧 DwdlmApplication.attachBaseContext 也会调一次，第二次直接早退）。
    // 2026-11-05：原文件此处连写两遍，确认是纯重复（两次调用之间无时序/状态差异），去一。
    Dwdlm_PrepareSecureShell();
#ifndef DWDLM_DATA_GUARD_ONLY
    Dwdlm_StartEarlyHookThread(vm);
#endif
#endif  // DWDLM_ENV_ONLY
}

static const char* kDwdlm_Features[] = {
    "Category_功能",
    "Slider_攻击倍率:1:100:1",
    "Toggle_上帝模式",
    "Toggle_无限技能",
    "Toggle_自动战斗",
    "Slider_移速:1:5:1",
    "Toggle_免广告",
};

extern "C" const StarryGameMod* const GameMod_Get() {
#ifdef DWDLM_ENV_ONLY
    // ＝＝ 环境阶段构建（本轮）＝＝
    // 菜单与正式包同六项，只有回调面收窄：tryInit / tick / onFeatureChanged /
    // onValueChanged / forceClear 全部 nullptr。框架对 nullptr 回调直接跳过
    // （Games/ModEntry.h 铁律：回调全部可选），因此本包不能经菜单触达任何玩法逻辑；
    // 玩法 JS 补丁也已在 DwdlmMod.cpp 以 DWDLM_ENV_ONLY 编译期剔除。
    // 明确禁止：不要用 g_feat=false 充当"功能未启用"——旧 hook 照样会被安装执行。
    // 环境接入面只剩 onLoad 里的 DwdlmSecShell_Start()（见文件头），不注册玩法 JNI。
    static const StarryGameMod mod = {
        .tag = "dwdlm",
        .authGameId = "dwdlm",
        .featureList = kDwdlm_Features,
        .featureCount = (int)(sizeof(kDwdlm_Features) / sizeof(kDwdlm_Features[0])),

        .waitLib = "libcocos2djs.so",
        .initDelayMs = 0,
        .initRetryMax = 0,          // 0＝不做启动期玩法初始化（tryInit 亦为 nullptr）
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = Dwdlm_OnLoad,
        .tryInit = nullptr,
        .isReady = Dwdlm_IsReady,
        .tick = nullptr,
        .onFeatureChanged = nullptr,
        .onValueChanged = nullptr,
        .onInputChanged = nullptr,
        .forceClear = nullptr,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
#else
    static const StarryGameMod mod = {
        .tag = "dwdlm",
        .authGameId = "dwdlm",
        .featureList = kDwdlm_Features,
        .featureCount = (int)(sizeof(kDwdlm_Features) / sizeof(kDwdlm_Features[0])),

        .waitLib = "libcocos2djs.so",
        .initDelayMs = 0,
        .initRetryMax = 120,
        .initRetryIntervalMs = 2000000,
        .tickIntervalMs = 200000,

        .onLoad = Dwdlm_OnLoad,
        .tryInit = Dwdlm_TryInit,
        .isReady = Dwdlm_IsReady,
        .tick = Dwdlm_Tick,
        .onFeatureChanged = Dwdlm_OnFeatureChanged,
        .onValueChanged = Dwdlm_OnValueChanged,
        .onInputChanged = nullptr,
        .forceClear = Dwdlm_ForceClear,
        .onApplyPlayPatch = nullptr,
    };
    return &mod;
#endif  // DWDLM_ENV_ONLY
}
