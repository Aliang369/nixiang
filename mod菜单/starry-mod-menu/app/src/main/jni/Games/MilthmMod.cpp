/* ================================================================
 *  milthm — 自动跳舞（6.0.5）
 *
 *  MethodMap Address = 文件偏移；runtime RVA = Address + 0x4000。
 *
 *  AddJudge 评分用 ToStatisticJudgeLevel()→StatisticJudgeLevel（Exact=0x23），
 *  不是只看 JudgeLevel 字段。上游 AddJudgeStatus 在 Miss 时还会清 0x8C。
 *  因此：AddJudge 入口强写字段 + ToStatisticJudgeLevel 在 auto 时直接返回 0x23。
 * ================================================================ */

#include "MilthmMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"

// getAbsoluteAddress / isLibraryLoaded via Utils.hpp

enum : uintptr_t {
    ELF_RX_BIAS        = 0x4000,
    FILE_ADD_JUDGE     = 0x42E2474,
    FILE_TO_STAT       = 0x4182484,
    RVA_ADD_JUDGE      = FILE_ADD_JUDGE + ELF_RX_BIAS, // 0x42E6474
    RVA_TO_STAT        = FILE_TO_STAT + ELF_RX_BIAS,   // 0x4186484
};

enum : size_t {
    OFF_ALWAYS_PERFECT = 0x89,
    OFF_JUDGE_LEVEL    = 0x8C,
    OFF_JUDGE_BIAS     = 0x90,
    OFF_STATUS         = 0x94, // HitStatus: Completed=3, Missed=4
};

enum : int32_t {
    JUDGE_EXACT       = 4,
    HIT_STATUS_DONE   = 3,
    STAT_EXACT        = 0x23, // StatisticJudgeLevel.Exact
};

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_auto{false};
static std::atomic<uint32_t> g_hits{0};

typedef void (*AddJudge_t)(void* thiz, void* hitObj, void* method);
typedef int32_t (*ToStat_t)(void* hitObj, void* method);

static AddJudge_t g_origAddJudge = nullptr;
static ToStat_t g_origToStat = nullptr;

static void ForceExact(void* hitObj) {
    if (!hitObj) return;
    auto base = reinterpret_cast<uintptr_t>(hitObj);
    *reinterpret_cast<uint8_t*>(base + OFF_ALWAYS_PERFECT) = 1;
    *reinterpret_cast<int32_t*>(base + OFF_JUDGE_LEVEL) = JUDGE_EXACT;
    *reinterpret_cast<float*>(base + OFF_JUDGE_BIAS) = 0.f;
    *reinterpret_cast<int32_t*>(base + OFF_STATUS) = HIT_STATUS_DONE;
}

static int32_t Hooked_ToStat(void* hitObj, void* method) {
    if (g_auto.load()) {
        ForceExact(hitObj);
        return STAT_EXACT;
    }
    return g_origToStat ? g_origToStat(hitObj, method) : STAT_EXACT;
}

static void Hooked_AddJudge(void* thiz, void* hitObj, void* method) {
    if (g_auto.load()) {
        ForceExact(hitObj);
        uint32_t n = g_hits.fetch_add(1) + 1;
        if (n <= 3 || (n % 50) == 0) {
            LOGI("[Milthm] AddJudge auto hit #%u obj=%p", n, hitObj);
        }
    }
    if (g_origAddJudge) g_origAddJudge(thiz, hitObj, method);
}

static bool HookOne(const char* tag, uintptr_t rva, void* hook, void** orig) {
    char rvaBuf[32];
    snprintf(rvaBuf, sizeof(rvaBuf), "0x%lX", (unsigned long)rva);
    void* addr = getAbsoluteAddress(GameUnity::TARGET_LIB, rvaBuf);
    if (!addr) {
        LOGE("[Milthm] %s resolve failed (%s)", tag, rvaBuf);
        return false;
    }
    if (*orig) return true;
    if (DobbyHook(addr, hook, orig) != 0) {
        LOGE("[Milthm] DobbyHook %s failed @ %p", tag, addr);
        return false;
    }
    LOGI("[Milthm] hooked %s @ %p (rva %s)", tag, addr, rvaBuf);
    return true;
}

bool Milthm_TryInit() {
    if (g_ready.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;

    LOGI("[Milthm] init AddJudge+ToStatistic (file+0x4000)");
    if (!HookOne("AddJudge", RVA_ADD_JUDGE, (void*)Hooked_AddJudge, (void**)&g_origAddJudge))
        return false;
    // ToStat 失败不阻断：仍靠 AddJudge 入口写字段
    HookOne("ToStatisticJudgeLevel", RVA_TO_STAT, (void*)Hooked_ToStat, (void**)&g_origToStat);

    g_ready.store(true);
    LOGI("[Milthm] init ok auto=%d toStat=%d", g_auto.load() ? 1 : 0, g_origToStat ? 1 : 0);
    return true;
}

bool Milthm_IsReady() { return g_ready.load(); }

void Milthm_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == MILTHM_FEAT_AUTO) {
        g_auto.store(enabled);
        LOGI("[Milthm] 自动跳舞=%s", enabled ? "ON" : "OFF");
    }
}

void Milthm_ForceClear() { g_auto.store(false); }

void Milthm_Tick(const bool* features) {
    if (!features) return;
    g_auto.store(features[MILTHM_FEAT_AUTO]);
    if (!g_ready.load()) Milthm_TryInit();
}
