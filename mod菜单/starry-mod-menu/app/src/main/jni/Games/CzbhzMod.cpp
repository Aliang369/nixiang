/* ================================================================
 *  CzbhzMod — 《城主别慌张》星光模块实现（纯壳 / JS 补丁模式）
 *
 *  定案（2026-09-11）：本作是 Cocos Creator 3.8.8 web-mobile(H5) 构建，
 *  跑在腾讯 X5 WebView 里，业务代码 100% 来自 CDN（无 libil2cpp/libcocos，
 *  APK 内只有盒子 + SDK）。因此：
 *
 *    ✗ 不做任何原生 hook（没有可挂的原生游戏逻辑）
 *    ✓ 本文件只维护「菜单功能位 ↔ 开关状态」，作为补丁侧的唯一状态源
 *    ✓ 功能主体 = assets/starry/czbhz_patch.js（页面内 JS 补丁）
 *
 *  为什么还要在 so 里存状态：补丁跑在 WebView 里，通过 Java 桥
 *  (com.starry.magic.h5box.StarryH5) 读到的是菜单偏好；本文件的 atomics
 *  则是框架 onFeatureChanged/onValueChanged/Tick 的直接落点，便于 logcat
 *  对账「菜单点了什么」与「补丁做了什么」，也保证 ForceClear 能一键复位。
 *
 *  上报一致性提醒：战斗类功能改动仿真流程，会影响上报给服务端的
 *  begin_seed/last_seed 与 hp/damage 摘要，务必按
 *  10_docs/城主别慌张/上报与检测面_20260911.md 的口径处理。
 * ================================================================ */

#include "CzbhzMod.h"

#include <atomic>

#include "Includes/Logger.h"

#define CZBHZ_LOG "[czbhz] "

// ============================================================
// ① 开关状态（菜单 → so；补丁经 Java 桥读偏好，二者对账用）
// ============================================================
static std::atomic<bool> g_ad{false};     // 免广告
static std::atomic<bool> g_inv{false};    // 无敌
static std::atomic<int> g_dmg{0};         // 伤害倍率秒杀（0 = 关）
static std::atomic<int> g_spd{0};         // 移速倍率（0 = 关）
static std::atomic<bool> g_skill{false};  // 无限技能
static std::atomic<bool> g_ready{false};

// ============================================================
// ② 注册表契约回调（签名见 Games/ModEntry.h，勿改）
// ============================================================
bool Czbhz_TryInit(JavaVM* /*vm*/) {
    // 没有目标库要等；框架 waitLib=nullptr 时首次即时调用本函数。
    if (!g_ready.load()) {
        g_ready.store(true);
        LOGI(CZBHZ_LOG "ready（纯壳/JS 补丁模式：无原生 hook，功能在 WebView 内）");
    }
    return true;
}

bool Czbhz_IsReady() { return g_ready.load(); }

void Czbhz_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case CZBHZ_FEAT_AD:    g_ad.store(enabled); break;
        case CZBHZ_FEAT_INV:   g_inv.store(enabled); break;
        case CZBHZ_FEAT_DMG:   if (!enabled) g_dmg.store(0); break;
        case CZBHZ_FEAT_SPD:   if (!enabled) g_spd.store(0); break;
        case CZBHZ_FEAT_SKILL: g_skill.store(enabled); break;
        default: break;
    }
    LOGI(CZBHZ_LOG "toggle #%d = %d", featNum, enabled ? 1 : 0);
}

void Czbhz_OnValueChanged(int featNum, int value) {
    if (featNum == CZBHZ_FEAT_DMG) {
        g_dmg.store(value);
    } else if (featNum == CZBHZ_FEAT_SPD) {
        g_spd.store(value);
    }
    LOGI(CZBHZ_LOG "value #%d = %d", featNum, value);
}

void Czbhz_Tick(const bool* features, const int* values) {
    // 铁律：Tick 只做开关同步，禁止扫堆/逐对象写内存/重活。
    if (features) {
        g_ad.store(features[CZBHZ_FEAT_AD]);
        g_inv.store(features[CZBHZ_FEAT_INV]);
        g_skill.store(features[CZBHZ_FEAT_SKILL]);
    }
    if (values) {
        g_dmg.store(values[CZBHZ_FEAT_DMG]);
        g_spd.store(values[CZBHZ_FEAT_SPD]);
    }
    if (!g_ready.load()) {
        g_ready.store(true);
    }
}

void Czbhz_ForceClear() {
    g_ad.store(false);
    g_inv.store(false);
    g_dmg.store(0);
    g_spd.store(0);
    g_skill.store(false);
    LOGI(CZBHZ_LOG "forceClear：全部开关复位（补丁侧 live=0 后自然哑火）");
}
