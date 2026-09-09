/* ================================================================
 *  偶像梦幻祭2 (es2) 4.4.8684 — 「自动跳舞」正式版
 *
 *  复刻斌哥 AllAmazing/AllPerfect（用户体感等效）：hook 3 个判定字段
 *  getter（RVA 实锤见 10_docs/偶像梦幻2/分析_斌哥包抓点_20260908.md §3.5），
 *  开关 ON 时返回最高判定枚举值 6（20260909 真机打歌实锤：全最高评分）。
 *
 *  RVA→file = RVA-0x4000（LOAD2）；FileOffToVa 按 maps RX 段匹配。
 *  序言校验防漂移：版本更新需重做 memdiff。
 * ================================================================ */

#include "Es2Mod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"

#define TPL_LOG "[Es2]"

// 最高判定枚举值（20260909 root 真机打歌实锤：ON 后全最高评分；正常值域 6主/4偶发，7 无效）
static constexpr int32_t ES2_JUDGE_TOP = 6;

// ============================================================
// ① 开关
// ============================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_hooked{false};
static std::atomic<bool> g_dance{false};      // 自动跳舞（对外唯一功能）
static std::atomic<uint32_t> g_ticks{0};

typedef int32_t (*Getter0_t)(void* self);
static Getter0_t g_origGet0 = nullptr;
static Getter0_t g_origGet1 = nullptr;
static Getter0_t g_origGet2 = nullptr;

// ============================================================
// ② file_off → 运行时 VA（RX 段匹配；il2cpp 分段映射禁 base+RVA）
// ============================================================
static void* FileOffToVa(const char* lib, uintptr_t fileOff) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return nullptr;
    char line[768];
    void* found = nullptr;
    size_t bestSpan = 0;
    while (fgets(line, sizeof(line), fp)) {
        if (!strstr(line, lib) || !strstr(line, ".so")) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x') || end <= start) continue;
        if (fileOff < off || fileOff - off >= static_cast<uintptr_t>(end - start)) continue;
        const size_t span = static_cast<size_t>(end - start);
        if (span > bestSpan) {
            bestSpan = span;
            found = reinterpret_cast<void*>(start + (fileOff - off));
        }
    }
    fclose(fp);
    return found;
}

// ============================================================
// ③ hook：ON 恒返最高判定，OFF 走原函数（判定正常）
// ============================================================
static int32_t Hooked_Get0(void* self) {
    if (g_dance.load()) return ES2_JUDGE_TOP;
    return g_origGet0 ? g_origGet0(self) : ES2_JUDGE_TOP;
}
static int32_t Hooked_Get1(void* self) {
    if (g_dance.load()) return ES2_JUDGE_TOP;
    return g_origGet1 ? g_origGet1(self) : ES2_JUDGE_TOP;
}
static int32_t Hooked_Get2(void* self) {
    if (g_dance.load()) return ES2_JUDGE_TOP;
    return g_origGet2 ? g_origGet2(self) : ES2_JUDGE_TOP;
}

struct GetterSlot {
    const char* tag;
    uintptr_t   fileOff;
    uint32_t    expectLo;   // 原始序言低字
    uint32_t    expectHi;   // 原始序言高字
    void*       hookFn;
    void**      origPtr;
};

// 序言 = 20260909 memdiff 实锤（0x70FB578/588/5A8）
static const GetterSlot kSlots[] = {
    {"get+0x24",  0x70FB578, 0xB9402400u, 0xD65F03C0u, (void*)Hooked_Get0, (void**)&g_origGet0},
    {"get+0x28",  0x70FB588, 0xB9402800u, 0xD65F03C0u, (void*)Hooked_Get1, (void**)&g_origGet1},
    {"get2+0x28", 0x70FB5A8, 0xAA0003E8u, 0xB9402800u, (void*)Hooked_Get2, (void**)&g_origGet2},
};

// 幂等 + 序言校验：漂移拒绝挂（防挂错地址闪退），下轮 Tick 重试
static void EnsureHooks() {
    if (g_hooked.load()) return;
    int ok = 0;
    for (const auto& s : kSlots) {
        if (*s.origPtr) { ok++; continue; }
        void* va = FileOffToVa(GameUnity::TARGET_LIB, s.fileOff);
        if (!va) continue;
        uint32_t lo, hi;
        memcpy(&lo, va, 4);
        memcpy(&hi, (char*)va + 4, 4);
        if (lo != s.expectLo || hi != s.expectHi) {
            LOGW("%s 序言漂移 got=%08X_%08X want=%08X_%08X（版本更新需重做 memdiff）",
                 s.tag, hi, lo, s.expectHi, s.expectLo);
            continue;
        }
        if (DobbyHook(va, s.hookFn, s.origPtr) == 0) {
            LOGI("%s hooked @va=%p (file 0x%lX)", s.tag, va, (unsigned long)s.fileOff);
            ok++;
        }
    }
    if (ok == 3) g_hooked.store(true);
}

// ============================================================
// ④ 对外接口（Main.cpp 契约）
// ============================================================
bool Es2_TryInit() {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;
    g_libReady.store(true);
    LOGI(TPL_LOG " lib ready (autoplay final: judge=%d)", ES2_JUDGE_TOP);
    return true;
}

bool Es2_IsReady() { return g_libReady.load(); }

void Es2_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == ES2_FEAT_AUTODANCE) {
        g_dance.store(enabled);
        LOGI(TPL_LOG " 自动跳舞=%s", enabled ? "ON" : "OFF");
    }
}

void Es2_OnValueChanged(int /*featNum*/, int /*value*/) {}

void Es2_ForceClear() { g_dance.store(false); }

void Es2_Tick(const bool* features) {
    if (features) g_dance.store(features[ES2_FEAT_AUTODANCE]);
    if (!g_libReady.load()) {
        if (isLibraryLoaded(GameUnity::TARGET_LIB)) g_libReady.store(true);
        else return;
    }
    // 功能开着才装 hook（默认 0 hook）；每 5 tick 重试直到 3/3
    if (!g_dance.load() && !g_hooked.load()) return;
    if (!g_hooked.load() && (g_ticks.fetch_add(1) + 1) % 5 == 0) EnsureHooks();
}
