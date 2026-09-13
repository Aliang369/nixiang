/* ================================================================
 * DcslMod - LayaAir/Conch JS bridge state for 地城狩猎.
 *
 * This game has no native gameplay library to hook. The functional
 * patch runs inside the Conch JavaScript runtime through StarryConch.
 * This file only keeps the native menu feature state and provides
 * lightweight logging/state synchronization.
 * ================================================================ */

#include "DcslMod.h"

#include <atomic>

#include "Includes/Logger.h"

#define DCSL_LOG "[dcsl] "

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_skill{false};

bool Dcsl_TryInit(JavaVM* /*vm*/) {
    if (!g_ready.load()) {
        g_ready.store(true);
        LOGI(DCSL_LOG "ready (Conch JS bridge mode)");
    }
    return true;
}

bool Dcsl_IsReady() { return g_ready.load(); }

void Dcsl_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == DCSL_FEAT_SKILL) {
        g_skill.store(enabled);
    }
    LOGI(DCSL_LOG "toggle #%d = %d", featNum, enabled ? 1 : 0);
}

void Dcsl_OnValueChanged(int featNum, int value) {
    LOGI(DCSL_LOG "value #%d = %d", featNum, value);
}

void Dcsl_Tick(const bool* features, const int* /*values*/) {
    if (features) {
        g_skill.store(features[DCSL_FEAT_SKILL]);
    }
    if (!g_ready.load()) {
        g_ready.store(true);
    }
}

void Dcsl_ForceClear() {
    g_skill.store(false);
    LOGI(DCSL_LOG "forceClear: skill disabled");
}
