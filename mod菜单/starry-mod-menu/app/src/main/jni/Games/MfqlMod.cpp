// ============================================================================
//  MfqlMod.cpp — 魔法奇录 星光菜单游戏模块（Cocos Creator 2.x）
//
//  设计（2026-11-26）：**0 hook 模块**。
//    免广告的实现链是「菜单 Java 侧读 prefs → JS 侧 jsb.reflection 查询 → index.js
//    在 SdkManger.showAd 里跳过原生播放、保留 c_start_see_asd / c_end_see_asd 上报」，
//    完全不需要 native 改写，因此本文件不装任何 Dobby 钩子、不碰内存。
//    好处：全关 = 零改写；没有 inline hook 就没有 ABI/反调试面。
//    需要 native 兜底（如 JS 被热更替换）时再按 GAME_MOD_GUIDE.md 增补挂点表。
// ============================================================================

#include "MfqlMod.h"

#include <atomic>

#include "Includes/Logger.h"

#define MFQL_TAG "[Mfql]"

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_freeAd{false};

bool Mfql_TryInit(JavaVM* vm) {
    (void)vm;
    if (g_ready.exchange(true)) return true;
    LOGI("%s init ok (0 hook; 免广告走 Java/JS 两层)", MFQL_TAG);
    return true;
}

bool Mfql_IsReady() { return g_ready.load(); }

void Mfql_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == MFQL_FEAT_FREE_AD) {
        g_freeAd.store(enabled);
        LOGI("%s feature #%d=free_ad %s", MFQL_TAG, featNum, enabled ? "ON" : "OFF");
    }
}

void Mfql_OnValueChanged(int featNum, int value) {
    (void)featNum;
    (void)value;
}

void Mfql_Tick(const bool* features, const int* values) {
    // 0 hook 模块：Tick 只做状态镜像，不触碰游戏内存。
    (void)values;
    if (!features) return;
    g_freeAd.store(features[MFQL_FEAT_FREE_AD]);
}

void Mfql_ForceClear() {
    g_freeAd.store(false);
    LOGI("%s force clear", MFQL_TAG);
}
