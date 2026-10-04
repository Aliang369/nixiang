/* ================================================================
 *  音舞光年 0.300.42 — 「模拟真人」
 *
 *  file=0x2BAC188：41 invoke(0x2660BDC) 同型搬迁；全局 HybridCLR invoke（高热）
 *  闪退根因：扫描 a0/a2/a3 盲调 get_name + 无校验写分 → SIGSEGV@+0x18
 *
 *  修复：仅认 a1=MethodInfo；MI 缓存；SafePtr 后才 patch；禁热路径日志
 * ================================================================ */

#include "YwgnMod.h"
#include "Includes/SecStrings.hpp"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <dlfcn.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/StarPtr.h"   // 设备无关指针判据（清 TBI/MTE 顶字节）
#include "GameConfig.h"
#include "Unity/Il2CppApi.h"

using Il2Cpp::IsValidHeapPtr;

enum : int32_t {
    SCORE_GOOD = 2,
    SCORE_GREAT = 3,
    SCORE_PERFECT = 4,
    SCORE_PERFECT_PLUS = 5,
};

enum : uintptr_t {
    FILE_INVOKE_42 = 0x2BAC188,
    FILE_INVOKE_41 = 0x2660BDC,
    ELF_BIAS = 0x4000,
};

enum : uint8_t {
    KIND_EMPTY = 0,
    KIND_SKIP = 1,
    /** SendScore / AddScoreToSendList：调用前改 a2 */
    KIND_SCORE_SEND = 2,
    /** CalculateTheScore / CalcNoteScore：调用前改参会破坏 timing → 仅调用后改分 */
    KIND_SCORE_CALC = 3,
};

static std::atomic<bool> g_inited{false};
static std::atomic<bool> g_hooked{false};
static std::atomic<bool> g_sim{false};
static std::atomic<uint32_t> g_simTicks{0};
static std::atomic<uint32_t> g_hits{0};
static std::atomic<int> g_pctPPlus{85};
static std::atomic<int> g_pctPerfect{13};
static std::atomic<int> g_pctGreat{1};
static std::atomic<int> g_pctGood{1};

typedef const char* (*method_get_name_t)(void* method);
typedef void* (*method_get_class_t)(void* method);
typedef const char* (*class_get_name_t)(void* klass);
typedef void* (*Invoke4_t)(void* a0, void* a1, void* a2, void* a3);

static method_get_name_t g_methodGetName = nullptr;
static method_get_class_t g_methodGetClass = nullptr;
static class_get_name_t g_classGetName = nullptr;
static Invoke4_t g_origInvoke = nullptr;

struct MiEnt {
    void* mi;
    uint8_t kind;
};
static MiEnt g_miCache[4096];

static bool SoftPtr(void* p) {
    if (!p) return false;
    // ★设备无关修正：上界原为 0x8000000000（512GiB），真机 Scudo 堆清标签后可达 0x76…/0x78…，
    //   越界即整片判否、只在真机暴露 ⇒ 改用共享区间 4GiB~128TiB。
    const uintptr_t u = starptr::Untag((uintptr_t)p);
    if (u & 7) return false;
    return u >= starptr::kUserVaMin && u < starptr::kUserVaMax;
}

static void* FileOffToVa(const char* lib, uintptr_t fileOff) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return nullptr;
    char line[768];
    void* found = nullptr;
    while (fgets(line, sizeof(line), fp)) {
        if (!strstr(line, lib)) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x')) continue;
        if (end <= start) continue;
        if (fileOff >= off && fileOff < off + (end - start)) {
            found = (void*)(start + (fileOff - off));
            break;
        }
    }
    fclose(fp);
    return found;
}

static bool EnsureSyms() {
    if (g_methodGetName) return true;
    void* h = dlopen(GameUnity::TARGET_LIB, RTLD_NOW);
    if (!h) return false;
    g_methodGetName = (method_get_name_t)dlsym(h, "il2cpp_method_get_name");
    g_methodGetClass = (method_get_class_t)dlsym(h, "il2cpp_method_get_class");
    g_classGetName = (class_get_name_t)dlsym(h, "il2cpp_class_get_name");
    return g_methodGetName && g_methodGetClass && g_classGetName;
}

static bool NameIsScoreSend(const char* n) {
    if (!n || !n[0]) return false;
    return strcmp(n, "SendScore") == 0 || strcmp(n, "AddScoreToSendList") == 0;
}

static bool NameIsScoreCalc(const char* n) {
    if (!n || !n[0]) return false;
    return strcmp(n, "CalcNoteScore") == 0 || strcmp(n, "CalculateTheScore") == 0;
}

static bool ClassOk(const char* c) {
    if (!c || !c[0]) return false;
    return strstr(c, "KeyJudgment") || strstr(c, "BubbleNodeController");
}

static uint8_t ClassifyMi(void* mi) {
    if (!mi || !SoftPtr(mi)) return KIND_SKIP;
    uintptr_t h = (reinterpret_cast<uintptr_t>(mi) >> 4) & 4095u;
    for (int p = 0; p < 12; p++) {
        MiEnt& e = g_miCache[(h + static_cast<uintptr_t>(p)) & 4095u];
        if (e.mi == mi) return e.kind;
        if (!e.mi) {
            const char* n = g_methodGetName(mi);
            if (!n || !n[0]) {
                e.mi = mi;
                e.kind = KIND_SKIP;
                return KIND_SKIP;
            }
            const char* cname = "";
            uint8_t k = KIND_SKIP;
            if (NameIsScoreSend(n) || NameIsScoreCalc(n)) {
                void* kls = g_methodGetClass(mi);
                if (kls && SoftPtr(kls)) {
                    const char* cn = g_classGetName(kls);
                    if (cn) cname = cn;
                }
                if (ClassOk(cname)) {
                    k = NameIsScoreSend(n) ? KIND_SCORE_SEND : KIND_SCORE_CALC;
                }
            }
            e.mi = mi;
            e.kind = k;
            if (k == KIND_SCORE_SEND || k == KIND_SCORE_CALC)
                LOGI("[Ywgn] MI score %s.%s (%s)", cname, n,
                     k == KIND_SCORE_SEND ? "pre" : "post");
            return k;
        }
    }
    return KIND_SKIP;
}

static uint32_t g_rng = 0xA5C3E917u;

static uint32_t NextRandU32() {
    uint32_t x = g_rng;
    x ^= x << 13;
    x ^= x >> 17;
    x ^= x << 5;
    g_rng = x ? x : 0xA5C3E917u;
    return g_rng;
}

/** 游戏 ScoreType：Good=2 Great=3 Perfect=4 Perfect+=5 */
static int32_t PickHumanScore() {
    int pp = g_pctPPlus.load();
    int p = g_pctPerfect.load();
    int gr = g_pctGreat.load();
    int go = g_pctGood.load();
    if (pp < 0) pp = 0;
    if (p < 0) p = 0;
    if (gr < 0) gr = 0;
    if (go < 0) go = 0;
    if (pp > 100) pp = 100;
    if (p > 100) p = 100;
    if (gr > 100) gr = 100;
    if (go > 100) go = 100;
    int total = pp + p + gr + go;
    if (total <= 0) {
        pp = 85;
        p = 13;
        gr = 1;
        go = 1;
        total = 100;
    }
    const uint32_t wGo = static_cast<uint32_t>(go) * 10000u / static_cast<uint32_t>(total);
    const uint32_t wGr = wGo + static_cast<uint32_t>(gr) * 10000u / static_cast<uint32_t>(total);
    const uint32_t wP = wGr + static_cast<uint32_t>(p) * 10000u / static_cast<uint32_t>(total);
    const uint32_t r = NextRandU32() % 10000u;
    if (r < wGo) return SCORE_GOOD;
    if (r < wGr) return SCORE_GREAT;
    if (r < wP) return SCORE_PERFECT;
    return SCORE_PERFECT_PLUS;
}

static const char* ScoreLabel(int32_t s) {
    switch (s) {
        case SCORE_GOOD: return "Good";
        case SCORE_GREAT: return "Great";
        case SCORE_PERFECT: return "Perfect";
        case SCORE_PERFECT_PLUS: return "Perfect+";
        default: return "?";
    }
}

static void WriteScoreSlot(int32_t* slot, int32_t score) {
    if (!slot || !IsValidHeapPtr(slot)) return;
    int32_t v = *slot;
    if (v >= 0 && v <= 6) *slot = score;
}

static int32_t PatchScorePkt(void* pkt) {
    if (!pkt || !IsValidHeapPtr(pkt)) return -1;
    const int32_t score = PickHumanScore();
    int32_t* at8 = reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(pkt) + 8);
    WriteScoreSlot(at8, score);
    void** slots = reinterpret_cast<void**>(pkt);
    if (slots[0] && IsValidHeapPtr(slots[0])) {
        int32_t* p0 = reinterpret_cast<int32_t*>(slots[0]);
        WriteScoreSlot(p0, score);
    }
    return score;
}

static void LogScoreHit(uint8_t kind, int32_t score) {
    uint32_t n = g_hits.fetch_add(1) + 1;
    if (n <= 8 || (n % 50) == 0) {
        LOGI("[Ywgn] %s→%s(%d) #%u",
             kind == KIND_SCORE_SEND ? "SendScore" : "CalcScore",
             ScoreLabel(score), score, n);
    }
}

static void* Hooked_Invoke(void* a0, void* a1, void* a2, void* a3) {
    uint8_t kind = KIND_SKIP;
    if (g_sim.load() && a1 && SoftPtr(a1)) {
        kind = ClassifyMi(a1);
        if (kind == KIND_SCORE_SEND) {
            int32_t score = PatchScorePkt(a2);
            if (score >= 0) LogScoreHit(kind, score);
        }
    }
    void* ret = g_origInvoke ? g_origInvoke(a0, a1, a2, a3) : nullptr;
    if (g_sim.load() && kind == KIND_SCORE_CALC) {
        int32_t score = PatchScorePkt(a2);
        if (score >= 0) LogScoreHit(kind, score);
    }
    return ret;
}

static bool TryHookAt(uintptr_t fileOff) {
    void* addr = FileOffToVa(sec_lib_il2cpp(), fileOff);
    if (!addr) return false;
    Invoke4_t orig = nullptr;
    if (DobbyHook(addr, (void*)Hooked_Invoke, (void**)&orig) != 0) return false;
    g_origInvoke = orig;
    g_hooked.store(true);
    LOGI("[Ywgn] hooked invoke @ %p file=0x%lX (a1-only filter)", addr, (unsigned long)fileOff);
    return true;
}

static void MaybeInstall() {
    if (!g_sim.load() || g_hooked.load() || g_simTicks.load() < 25) return;
    if (!EnsureSyms()) return;
    if (!TryHookAt(FILE_INVOKE_42)) TryHookAt(FILE_INVOKE_41);
}

bool Ywgn_IsReady() { return g_hooked.load(); }

bool Ywgn_TryInit() {
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;
    if (!g_inited.exchange(true)) LOGI("[Ywgn] init invoke file=0x%lX", (unsigned long)FILE_INVOKE_42);
    return true;
}

void Ywgn_SecurityTick() {}

void Ywgn_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum != YWGN_FEAT_SIM_HUMAN) return;
    g_sim.store(enabled);
    if (enabled) g_simTicks.store(0);
    LOGI("[Ywgn] 模拟真人=%s hits=%u hooked=%d", enabled ? "ON" : "OFF", g_hits.load(),
         g_hooked.load() ? 1 : 0);
}

void Ywgn_SyncPctSliders(const int* values) {
    int pp = 85, p = 13, gr = 1, go = 1;
    if (values) {
        pp = values[YWGN_FEAT_PCT_PPLUS];
        p = values[YWGN_FEAT_PCT_PERFECT];
        gr = values[YWGN_FEAT_PCT_GREAT];
        go = values[YWGN_FEAT_PCT_GOOD];
        if (pp <= 0 && p <= 0 && gr <= 0 && go <= 0) {
            pp = 85;
            p = 13;
            gr = 1;
            go = 1;
        } else {
            if (pp < 0) pp = 0;
            if (p < 0) p = 0;
            if (gr < 0) gr = 0;
            if (go < 0) go = 0;
        }
    }
    static int s_lastPp = -1, s_lastP = -1, s_lastGr = -1, s_lastGo = -1;
    if (pp == s_lastPp && p == s_lastP && gr == s_lastGr && go == s_lastGo) return;
    s_lastPp = pp;
    s_lastP = p;
    s_lastGr = gr;
    s_lastGo = go;
    g_pctPPlus.store(pp);
    g_pctPerfect.store(p);
    g_pctGreat.store(gr);
    g_pctGood.store(go);
    LOGI("[Ywgn] 概率 P+=%d P=%d Gt=%d Gd=%d", pp, p, gr, go);
}

void Ywgn_OnValueChanged(int featNum, int value) {
    if (value < 0) value = 0;
    if (value > 100) value = 100;
    switch (featNum) {
        case YWGN_FEAT_PCT_PPLUS:
            g_pctPPlus.store(value);
            break;
        case YWGN_FEAT_PCT_PERFECT:
            g_pctPerfect.store(value);
            break;
        case YWGN_FEAT_PCT_GREAT:
            g_pctGreat.store(value);
            break;
        case YWGN_FEAT_PCT_GOOD:
            g_pctGood.store(value);
            break;
        default:
            return;
    }
}

void Ywgn_ForceClear() {
    g_sim.store(false);
    g_pctPPlus.store(85);
    g_pctPerfect.store(13);
    g_pctGreat.store(1);
    g_pctGood.store(1);
}

void Ywgn_Tick(const bool* features) {
    if (!features) return;
    g_sim.store(features[YWGN_FEAT_SIM_HUMAN]);
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return;
    if (!g_sim.load()) {
        g_simTicks.store(0);
        return;
    }
    uint32_t st = g_simTicks.fetch_add(1) + 1;
    if ((st % 5) == 0) MaybeInstall();
    if ((st % 60) == 0)
        LOGI("[Ywgn] hb hits=%u hooked=%d", g_hits.load(), g_hooked.load() ? 1 : 0);
}
