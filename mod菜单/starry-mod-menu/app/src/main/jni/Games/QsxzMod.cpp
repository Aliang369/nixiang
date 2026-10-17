/* ================================================================
 *  QsxzMod — 《炁术行者》星光模块实现（纯壳 / JS 补丁模式）
 *
 *  定案（2026-10-07）：本作是 Cocos Creator 3.8.2 web-mobile(H5)，
 *  跑在宿主自己的 WebView 里，APK 内零游戏业务逻辑（1.4MB 壳 +
 *  DEX 只有一个 classes.dex，无 lib/、无 assets/）。因此：
 *
 *    ✗ 不做任何原生 hook（没有可挂的原生游戏逻辑）
 *    ✓ 本文件只维护「菜单功能位 ↔ 开关状态」，作为补丁侧的唯一状态源
 *    ✓ 功能主体 = assets/starry/qsxz_patch.js（页面内 JS 补丁）
 *
 *  数据流：菜单 UI(MenuService) → 框架 onValueChanged/Tick → 本文件 atomics
 *          → Preferences（Java 侧）→ window.StarryH5.getState() → 补丁读值
 *          → 补丁调用页面内 window.__QSW_APPLY(k, dr, spd) → 伤害/减伤/战斗加速生效
 * ================================================================ */

#include "QsxzMod.h"

#include <atomic>

#include "Includes/Logger.h"

#define QSXZ_LOG "[qsxz] "

// ============================================================
// ① 开关状态（菜单 → so；补丁经 Java 桥读偏好，二者对账用）
// ============================================================
static std::atomic<int> g_dmg{1};   // 伤害倍率（1 = 关，等于原版）
static std::atomic<int> g_dr{0};    // 减伤百分比（0 = 关）
static std::atomic<int> g_spd{1};   // 战斗加速倍速（1 = 关，等于原版节奏）
static std::atomic<bool> g_ready{false};

// ============================================================
// ② 注册表契约回调（签名见 Games/ModEntry.h，勿改）
// ============================================================
bool Qsxz_TryInit(JavaVM* /*vm*/) {
    // 没有目标库要等；框架 waitLib=nullptr 时首次即时调用本函数。
    if (!g_ready.load()) {
        g_ready.store(true);
        LOGI(QSXZ_LOG "ready（纯壳/JS 补丁模式：无原生 hook，功能在 WebView 内）");
    }
    return true;
}

bool Qsxz_IsReady() { return g_ready.load(); }

void Qsxz_OnFeatureChanged(int featNum, bool enabled) {
    // 本作三个功能都是滑块，无 Toggle；关掉时回落到原版口径。
    switch (featNum) {
        case QSXZ_FEAT_DMG:   if (!enabled) g_dmg.store(1); break;
        case QSXZ_FEAT_DR:    if (!enabled) g_dr.store(0);  break;
        case QSXZ_FEAT_SPEED: if (!enabled) g_spd.store(1); break;
        default: break;
    }
    LOGI(QSXZ_LOG "toggle #%d = %d", featNum, enabled ? 1 : 0);
}

void Qsxz_OnValueChanged(int featNum, int value) {
    if (featNum == QSXZ_FEAT_DMG) {
        if (value < 1) value = 1;
        if (value > 100) value = 100;   // 2026-10-18：上限 5→100（用户定制）
        g_dmg.store(value);
    } else if (featNum == QSXZ_FEAT_DR) {
        if (value < 0) value = 0;        // 0 = 原版哨兵（未授权回落 0）；菜单 min=1 保证玩家最低 1%
        if (value > 100) value = 100;    // 2026-10-18：上限 50→100（用户定制）
        g_dr.store(value);
    } else if (featNum == QSXZ_FEAT_SPEED) {
        if (value < 1) value = 1;
        if (value > 10) value = 10;      // 2026-10-18：上限 3→10（用户定制 1-10）；1 = 原版完全不干预
        g_spd.store(value);
    }
    LOGI(QSXZ_LOG "value #%d = %d", featNum, value);
}

void Qsxz_Tick(const bool* features, const int* values) {
    // 铁律：Tick 只做开关同步，禁止扫堆/逐对象写内存/重活。
    (void)features;
    if (values) {
        int k = values[QSXZ_FEAT_DMG];
        int d = values[QSXZ_FEAT_DR];
        int s = values[QSXZ_FEAT_SPEED];
        if (k < 1) k = 1; else if (k > 100) k = 100;  // 2026-10-18：上限 5 → 100
        if (d < 0) d = 0; else if (d > 100) d = 100;  // 0 = 原版哨兵；上限 50 → 100
        if (s < 1) s = 1; else if (s > 10) s = 10;    // 2026-10-18：上限 3→10；1 = 原版
        g_dmg.store(k);
        g_dr.store(d);
        g_spd.store(s);
    }
    if (!g_ready.load()) {
        g_ready.store(true);
    }
}

void Qsxz_ForceClear() {
    g_dmg.store(1);
    g_dr.store(0);
    g_spd.store(1);
    LOGI(QSXZ_LOG "forceClear：全部功能回落原版（补丁侧 live=0 后自然哑火）");
}
