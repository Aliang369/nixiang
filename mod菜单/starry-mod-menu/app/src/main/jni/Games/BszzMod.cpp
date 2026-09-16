/* ================================================================
 *  宝石战争 (com.acing.gow.adv) 6.0.012 — HybridCLR 薄桥接模块
 *
 *  本模块不 hook 游戏逻辑，只维护菜单开关状态，并导出 C ABI 供注入后的
 *  System.Network.dll 通过 P/Invoke 读取：
 *    - StarryGw_AdjustDamage        玩家伤害倍率 / AI 侧上帝模式
 *    - StarryGw_AdjustHealthValue   直写血量时的无敌兜底
 *    - StarryGw_ShouldKeepMana      无限技能：拦截 SetMana(0)
 * ================================================================ */

#include "BszzMod.h"

#include <atomic>
#include <cstdint>
#include <jni.h>

#include "Includes/Logger.h"

#define BSZZ_LOG "[Bszz]"

namespace {
constexpr int32_t kMaxScaledDamage = 100000000;
constexpr int32_t kMinScaledDamage = -kMaxScaledDamage;

std::atomic<int32_t> g_attackMultiplier{1};
std::atomic<bool> g_godMode{false};
std::atomic<bool> g_unlimitedSkill{false};
std::atomic<bool> g_ready{true};

// 诊断用：每个入口最多打印 8 次调用，确认 IL 侧 P/Invoke 是否真的打进来。
constexpr int32_t kMaxBridgeLogs = 8;
std::atomic<int32_t> g_logDamage{0};
std::atomic<int32_t> g_logHealth{0};
std::atomic<int32_t> g_logMana{0};
std::atomic<int32_t> g_logRunAway{0};

int32_t ClampMultiplier(int32_t value) {
    if (value < 1) return 1;
    if (value > 100) return 100;
    return value;
}
}  // namespace

extern "C" __attribute__((visibility("default"))) int32_t
StarryGw_GetAttackMultiplier() {
    return g_attackMultiplier.load(std::memory_order_relaxed);
}

extern "C" __attribute__((visibility("default"))) int32_t
StarryGw_AdjustDamage(int32_t adjust, int32_t targetPlayerIndex) {
    if (adjust >= 0) return adjust;

    if (targetPlayerIndex == 0 && g_godMode.load(std::memory_order_relaxed)) {
        if (g_logDamage.fetch_add(1) < kMaxBridgeLogs) {
            LOGI(BSZZ_LOG " AdjustDamage in=%d target=%d -> 0 (god=1)",
                 adjust, targetPlayerIndex);
        }
        return 0;
    }

    if (targetPlayerIndex == 1) {
        const int32_t multiplier =
            g_attackMultiplier.load(std::memory_order_relaxed);
        if (multiplier > 1) {
            int64_t scaled = static_cast<int64_t>(adjust) * multiplier;
            if (scaled < kMinScaledDamage) scaled = kMinScaledDamage;
            if (scaled > kMaxScaledDamage) scaled = kMaxScaledDamage;
            if (g_logDamage.fetch_add(1) < kMaxBridgeLogs) {
                LOGI(BSZZ_LOG " AdjustDamage in=%d target=%d -> %d (mult=%d)",
                     adjust, targetPlayerIndex, static_cast<int32_t>(scaled),
                     multiplier);
            }
            return static_cast<int32_t>(scaled);
        }
    }
    if (g_logDamage.fetch_add(1) < kMaxBridgeLogs) {
        LOGI(BSZZ_LOG " AdjustDamage in=%d target=%d -> pass (god=%d mult=%d)",
             adjust, targetPlayerIndex,
             g_godMode.load(std::memory_order_relaxed) ? 1 : 0,
             g_attackMultiplier.load(std::memory_order_relaxed));
    }
    return adjust;
}

extern "C" __attribute__((visibility("default"))) int32_t
StarryGw_AdjustHealthValue(int32_t value, int32_t playerIndex) {
    if (playerIndex == 0 && value <= 0 &&
        g_godMode.load(std::memory_order_relaxed)) {
        if (g_logHealth.fetch_add(1) < kMaxBridgeLogs) {
            LOGI(BSZZ_LOG " AdjustHealthValue value=%d player=%d -> 1 (god=1)",
                 value, playerIndex);
        }
        return 1;
    }
    if (g_logHealth.fetch_add(1) < kMaxBridgeLogs) {
        LOGI(BSZZ_LOG " AdjustHealthValue value=%d player=%d -> pass (god=%d)",
             value, playerIndex,
             g_godMode.load(std::memory_order_relaxed) ? 1 : 0);
    }
    return value;
}

extern "C" __attribute__((visibility("default"))) int32_t
StarryGw_ShouldKeepMana(int32_t playerIndex) {
    const bool keep = playerIndex == 0 &&
                      g_unlimitedSkill.load(std::memory_order_relaxed);
    if (g_logMana.fetch_add(1) < kMaxBridgeLogs) {
        LOGI(BSZZ_LOG " ShouldKeepMana player=%d -> %d (unlimited=%d)",
             playerIndex, keep ? 1 : 0,
             g_unlimitedSkill.load(std::memory_order_relaxed) ? 1 : 0);
    }
    return keep ? 1 : 0;
}

extern "C" __attribute__((visibility("default"))) int32_t
StarryGw_ShouldBlockRunAway(int32_t playerIndex) {
    const bool block = playerIndex == 0 &&
                       g_godMode.load(std::memory_order_relaxed);
    if (g_logRunAway.fetch_add(1) < kMaxBridgeLogs) {
        LOGI(BSZZ_LOG " ShouldBlockRunAway player=%d -> %d (god=%d)",
             playerIndex, block ? 1 : 0,
             g_godMode.load(std::memory_order_relaxed) ? 1 : 0);
    }
    return block ? 1 : 0;
}

bool Bszz_TryInit(JavaVM* /*vm*/) {
    g_ready.store(true, std::memory_order_relaxed);
    LOGI(BSZZ_LOG " bridge ready");
    return true;
}

bool Bszz_IsReady() {
    return g_ready.load(std::memory_order_relaxed);
}

void Bszz_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case BSZZ_FEAT_GOD_MODE:
            g_godMode.store(enabled, std::memory_order_relaxed);
            break;
        case BSZZ_FEAT_UNLIMITED_SKILL:
            g_unlimitedSkill.store(enabled, std::memory_order_relaxed);
            break;
        default:
            break;
    }
    LOGI(BSZZ_LOG " feature %d=%d", featNum, enabled ? 1 : 0);
}

void Bszz_OnValueChanged(int featNum, int value) {
    if (featNum == BSZZ_FEAT_ATTACK_MULT) {
        g_attackMultiplier.store(ClampMultiplier(value),
                                 std::memory_order_relaxed);
        LOGI(BSZZ_LOG " attack multiplier=%d",
             g_attackMultiplier.load(std::memory_order_relaxed));
    }
}

void Bszz_Tick(const bool* features, const int* values) {
    if (values) {
        g_attackMultiplier.store(ClampMultiplier(values[BSZZ_FEAT_ATTACK_MULT]),
                                 std::memory_order_relaxed);
    }
    if (features) {
        g_godMode.store(features[BSZZ_FEAT_GOD_MODE],
                        std::memory_order_relaxed);
        g_unlimitedSkill.store(features[BSZZ_FEAT_UNLIMITED_SKILL],
                               std::memory_order_relaxed);
    }
}

void Bszz_ForceClear() {
    g_attackMultiplier.store(1, std::memory_order_relaxed);
    g_godMode.store(false, std::memory_order_relaxed);
    g_unlimitedSkill.store(false, std::memory_order_relaxed);
}
