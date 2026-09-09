/* ================================================================
 *  公主连结 (com.bilibili.priconne) 11.7.2
 *
 *  对齐 GAME_MOD_GUIDE / _TemplateMod：
 *    - 只 hook 具体方法（Time.get_deltaTime / UnitData.get_Total* / UnitCtrl.get_IsPlayerUnit）
 *    - 每个 hook 先调 orig
 *    - 热路径禁止 SafeProbe；一次性初始化才允许
 *    - 敌我：IsPlayerUnit 钩子写入 UnitData→side；Total* 只读表（未知不改）
 *    - 2026-08-29 终版：side 表信任条目 + mutex + 满表覆盖最旧（不整表清零）+
 *      30s 静默跨局清表。❌ il2cpp gchandle_* 在本游戏 hook 线程调用会 SIGSEGV
 *      （tombstone_01: gchandle_free 内崩），整层拆除，禁再引入。
 *    - 属性 2026-08-29：上限 999999；数值走 InputValue（nativeSetText→Pcr_OnInputChanged）
 * ================================================================ */

#include "PcrMod.h"

#include <atomic>
#include <cstdint>
#include <cstdlib>
#include <cstring>
#include <ctime>
#include <mutex>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

// ============================================================
// 全局开关
// ============================================================
static constexpr int kAttrValMax = 999999;  // 2026-08-29：9999 → 999999（输入框形式）
static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_attrOn{false};
static std::atomic<int> g_attrVal{9999};
static std::atomic<int> g_speedMul{1};
static std::atomic<bool> g_sideReenter{false};  // IsPlayerUnit ↔ Total* 防重入
static std::atomic<uint32_t> g_hitsDelta{0};
static std::atomic<uint32_t> g_hitsAttr{0};
static std::atomic<uint32_t> g_hitsBind{0};

// ============================================================
// 原函数指针
// ============================================================
typedef float (*GetDeltaTime_t)(void* method);
typedef int32_t (*I32Getter0_t)(void* self, void* method);
typedef int64_t (*I64Getter0_t)(void* self, void* method);
typedef uint8_t (*BoolGetter0_t)(void* self, void* method);

static GetDeltaTime_t g_origDelta = nullptr;
static BoolGetter0_t g_origIsPlayerUnit = nullptr;
static void* g_miIsPlayerUnit = nullptr;

static void* g_unitDataKlass = nullptr;
static void* g_unitCtrlKlass = nullptr;
static void* g_unitParamKlass = nullptr;
static size_t g_offUnitOnCtrl = 0;     // UnitCtrl → UnitData 或 UnitParameter
static size_t g_offUniqueOnParam = 0;  // UnitParameter → UniqueData(UnitData)
static bool g_offIsParam = false;      // true=先解 Parameter

// ============================================================
// UnitData → 敌我（由 get_IsPlayerUnit 热更新；Total* 只读）
// ============================================================
static constexpr int kMaxSide = 96;
struct SideBind {
    void* unitData;
    uint8_t isPlayer;  // 1=己方 0=敌方
    uint8_t valid;
};
static SideBind g_sides[kMaxSide];
static std::atomic<int> g_sideN{0};
static std::mutex g_sideMtx;  // IsPlayerUnit ↔ Total* 可能异线程；数组读写必须串行化

static void* MethodPointer(void* mi) {
    return mi ? *reinterpret_cast<void**>(mi) : nullptr;
}

static void* FindClassNs(const char* ns, const char* name) {
    auto& api = GetApi();
    size_t n = 0;
    void** asms = api.Assemblies(&n);
    if (!asms || n == 0) return nullptr;
    for (size_t i = 0; i < n; i++) {
        void* img = api.ImageOf(asms[i]);
        if (!img) continue;
        void* k = api.ClassFromName(img, ns, name);
        if (k) return k;
    }
    return nullptr;
}

static void ClearSides() {
    std::lock_guard<std::mutex> lk(g_sideMtx);
    for (int i = 0; i < g_sideN.load() && i < kMaxSide; i++) g_sides[i] = {};
    g_sideN.store(0);
}

// 跨局清表：距离上一次登记 >30s 视为新对局/新场景，清掉旧条目。
// 防跨局 GC 地址复用把新敌方套进旧己方条目（敌我瑕疵主场景）；局内登记密集不会触发。
static std::atomic<int64_t> g_lastBindMs{0};

static int64_t NowMs() {
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return (int64_t)ts.tv_sec * 1000 + ts.tv_nsec / 1000000;
}

static void MaybeClearSidesByGap() {
    int64_t now = NowMs();
    if (g_sideN.load() == 0) {
        g_lastBindMs.store(now);
        return;
    }
    int64_t last = g_lastBindMs.exchange(now);
    if (last > 0 && now - last > 30000) {
        ClearSides();
        LOGI("[Pcr] side session gap %llds → clear sides", (long long)((now - last) / 1000));
    }
}

static void UpsertSide(void* unitData, uint8_t isPlayer) {
    if (!unitData || !Il2Cpp::IsValidHeapPtr(unitData)) return;
    MaybeClearSidesByGap();
    std::lock_guard<std::mutex> lk(g_sideMtx);
    int n = g_sideN.load();
    for (int i = 0; i < n && i < kMaxSide; i++) {
        SideBind& e = g_sides[i];
        if (e.valid && e.unitData == unitData) {
            e.isPlayer = isPlayer;
            e.valid = 1;
            return;
        }
    }
    if (n >= kMaxSide) {
        // 满表：覆盖最旧（绝不整表清零）
        g_sides[0] = {unitData, isPlayer, 1};
        LOGW("[Pcr] side full → evict slot0");
        return;
    }
    g_sides[n] = {unitData, isPlayer, 1};
    g_sideN.store(n + 1);
    uint32_t lg = g_hitsBind.fetch_add(1) + 1;
    if (lg <= 12 || (lg % 80) == 0)
        LOGI("[Pcr] side UnitData=%p isPlayer=%u n=%d", unitData, (unsigned)isPlayer, n + 1);
}

/** 未知 / 无效 → false（失败关闭，禁止启发式放行）。
 *  条目信任策略（2026-08-29）：登记即信、同址重登自愈（敌方 IsPlayerUnit 会覆盖同址旧己方标记），
 *  跨局残留由 MaybeClearSidesByGap 清除。GC 弱句柄校验在本游戏误杀活条目，已撤（留诊断）。 */
static bool ShouldBoostUnit(void* unitData) {
    if (!unitData || !Il2Cpp::IsValidHeapPtr(unitData)) return false;
    if (g_sideReenter.load()) return false;
    std::lock_guard<std::mutex> lk(g_sideMtx);
    int n = g_sideN.load();
    for (int i = 0; i < n && i < kMaxSide; i++) {
        SideBind& e = g_sides[i];
        if (e.valid && e.unitData == unitData) return e.isPlayer != 0;
    }
    return false;
}

// ============================================================
// UnitCtrl → UnitData（FieldOffset 优先；失败则首次实例扫槽缓存偏移）
// 热路径只用 IsValidHeapPtr + object_class，禁止 SafeProbe
// ============================================================
static void* ResolveUniqueData(void* paramObj) {
    if (!paramObj || !Il2Cpp::IsValidHeapPtr(paramObj)) return nullptr;
    auto& api = GetApi();
    if (g_offUniqueOnParam == 0 && g_unitParamKlass) {
        size_t uOff = api.FieldOffset(g_unitParamKlass, "<UniqueData>k__BackingField", 0);
        if (uOff == 0) uOff = api.FieldOffset(g_unitParamKlass, "UniqueData", 0);
        if (uOff > 0 && uOff < 0x200) g_offUniqueOnParam = uOff;
    }
    if (g_offUniqueOnParam == 0) return nullptr;
    void* ud = Il2Cpp::ReadPtr(paramObj, g_offUniqueOnParam);
    if (!ud || !Il2Cpp::IsValidHeapPtr(ud)) return nullptr;
    if (g_unitDataKlass && api.object_class && api.object_class(ud) != g_unitDataKlass)
        return nullptr;
    return ud;
}

/** 首次拿到 UnitCtrl 实例时扫指针槽，钉死偏移（只成功一次；允许一次性 SafeProbe） */
static void DiscoverOffsetFromInstance(void* unitCtrl) {
    if (g_offUnitOnCtrl != 0 || !unitCtrl || !Il2Cpp::IsValidHeapPtr(unitCtrl)) return;
    auto& api = GetApi();
    if (!api.object_class) return;
    if (!g_unitParamKlass) g_unitParamKlass = FindClassNs("Elements", "UnitParameter");

    // 一次性探测：不在 Total* 热路径；仅 FieldOffset 失败后的首次 IsPlayerUnit
    Il2Cpp::SafeProbe([&]() -> bool {
        for (size_t off = 0x10; off <= 0x280; off += 8) {
            void* p = Il2Cpp::ReadPtr(unitCtrl, off);
            if (!p || !Il2Cpp::IsValidHeapPtr(p)) continue;
            void* k = api.object_class(p);
            if (!k) continue;
            if (g_unitDataKlass && k == g_unitDataKlass) {
                g_offUnitOnCtrl = off;
                g_offIsParam = false;
                LOGI("[Pcr] discover UnitCtrl→UnitData @ +0x%zx", off);
                return true;
            }
            if (g_unitParamKlass && k == g_unitParamKlass) {
                void* ud = ResolveUniqueData(p);
                if (!ud) continue;
                g_offUnitOnCtrl = off;
                g_offIsParam = true;
                LOGI("[Pcr] discover UnitCtrl→UnitParameter @ +0x%zx UniqueData=+0x%zx", off,
                     g_offUniqueOnParam);
                return true;
            }
        }
        LOGW("[Pcr] discover UnitCtrl slot miss");
        return true;
    });
}

static void* ExtractUnitDataFromCtrl(void* unitCtrl) {
    if (!unitCtrl || !Il2Cpp::IsValidHeapPtr(unitCtrl)) return nullptr;
    if (g_offUnitOnCtrl == 0) DiscoverOffsetFromInstance(unitCtrl);
    if (g_offUnitOnCtrl == 0) return nullptr;

    void* p = Il2Cpp::ReadPtr(unitCtrl, g_offUnitOnCtrl);
    if (!p || !Il2Cpp::IsValidHeapPtr(p)) return nullptr;

    auto& api = GetApi();
    if (!g_offIsParam) {
        if (g_unitDataKlass && api.object_class && api.object_class(p) != g_unitDataKlass)
            return nullptr;
        return p;
    }
    if (g_unitParamKlass && api.object_class && api.object_class(p) != g_unitParamKlass)
        return nullptr;
    return ResolveUniqueData(p);
}

static bool ResolveUnitCtrlOffsets(void* unitCtrlKlass) {
    auto& api = GetApi();
    static const char* kDirect[] = {
        "UnitData", "unitData", "_unitData", "<UnitData>k__BackingField", nullptr};
    static const char* kParam[] = {
        "UnitParameter", "unitParameter", "_unitParameter", "<UnitParameter>k__BackingField",
        nullptr};

    for (int i = 0; kDirect[i]; i++) {
        size_t off = api.FieldOffset(unitCtrlKlass, kDirect[i], 0);
        if (off > 0 && off < 0x400) {
            g_offUnitOnCtrl = off;
            g_offIsParam = false;
            LOGI("[Pcr] UnitCtrl.%s @ +0x%zx", kDirect[i], off);
            return true;
        }
    }
    g_unitParamKlass = FindClassNs("Elements", "UnitParameter");
    for (int i = 0; kParam[i]; i++) {
        size_t off = api.FieldOffset(unitCtrlKlass, kParam[i], 0);
        if (off > 0 && off < 0x400) {
            size_t uOff = 0;
            if (g_unitParamKlass) {
                uOff = api.FieldOffset(g_unitParamKlass, "<UniqueData>k__BackingField", 0);
                if (uOff == 0) uOff = api.FieldOffset(g_unitParamKlass, "UniqueData", 0);
            }
            if (uOff == 0) continue;
            g_offUnitOnCtrl = off;
            g_offUniqueOnParam = uOff;
            g_offIsParam = true;
            LOGI("[Pcr] UnitCtrl.%s @ +0x%zx UniqueData=+0x%zx", kParam[i], off, uOff);
            return true;
        }
    }
    LOGI("[Pcr] FieldOffset miss → defer discover on first IsPlayerUnit");
    return false;
}

// ============================================================
// Hooks
// ============================================================
static uint8_t Hooked_IsPlayerUnit(void* self, void* method) {
    uint8_t v = g_origIsPlayerUnit ? g_origIsPlayerUnit(self, method) : 0;
    if (g_sideReenter.load()) return v;
    if (!self || !Il2Cpp::IsValidHeapPtr(self)) return v;
    if (g_unitCtrlKlass && GetApi().object_class &&
        GetApi().object_class(self) != g_unitCtrlKlass)
        return v;

    g_sideReenter.store(true);
    void* ud = ExtractUnitDataFromCtrl(self);
    if (ud) UpsertSide(ud, v ? 1 : 0);
    g_sideReenter.store(false);
    return v;
}

static float Hooked_get_deltaTime(void* method) {
    float v = g_origDelta ? g_origDelta(method) : 0.016f;
    int mul = g_speedMul.load();
    if (mul > 1) {
        v *= static_cast<float>(mul);
        uint32_t n = g_hitsDelta.fetch_add(1) + 1;
        if (n <= 3 || (n % 400) == 0)
            LOGI("[Pcr] deltaTime×%d hit#%u", mul, n);
    }
    return v;
}

#define PCR_I32(N)                                                                             \
    static I32Getter0_t g_origI32_##N = nullptr;                                                \
    static int32_t Hooked_I32_##N(void* self, void* method) {                                   \
        int32_t v = g_origI32_##N ? g_origI32_##N(self, method) : 0;                           \
        if (!g_attrOn.load() || !ShouldBoostUnit(self)) return v;                              \
        int32_t out = g_attrVal.load();                                                        \
        uint32_t n = g_hitsAttr.fetch_add(1) + 1;                                              \
        if (n <= 6 || (n % 200) == 0)                                                          \
            LOGI("[Pcr] attr i32 hit#%u %d->%d sides=%d", n, v, out, g_sideN.load());         \
        return out;                                                                            \
    }

#define PCR_I64(N)                                                                             \
    static I64Getter0_t g_origI64_##N = nullptr;                                                \
    static int64_t Hooked_I64_##N(void* self, void* method) {                                   \
        int64_t v = g_origI64_##N ? g_origI64_##N(self, method) : 0;                           \
        if (!g_attrOn.load() || !ShouldBoostUnit(self)) return v;                              \
        int64_t out = (int64_t)g_attrVal.load();                                               \
        uint32_t n = g_hitsAttr.fetch_add(1) + 1;                                              \
        if (n <= 6 || (n % 200) == 0)                                                          \
            LOGI("[Pcr] attr i64 hit#%u %lld->%lld sides=%d", n, (long long)v,                \
                 (long long)out, g_sideN.load());                                              \
        return out;                                                                            \
    }

PCR_I32(0) PCR_I32(1) PCR_I32(2) PCR_I32(3) PCR_I32(4) PCR_I32(5)
PCR_I32(6) PCR_I32(7) PCR_I32(8) PCR_I32(9) PCR_I32(10) PCR_I32(11)
PCR_I32(12) PCR_I32(13) PCR_I32(14) PCR_I32(15)

PCR_I64(0) PCR_I64(1) PCR_I64(2) PCR_I64(3)

static I32Getter0_t* g_i32Origs[] = {
    &g_origI32_0,  &g_origI32_1,  &g_origI32_2,  &g_origI32_3,  &g_origI32_4,  &g_origI32_5,
    &g_origI32_6,  &g_origI32_7,  &g_origI32_8,  &g_origI32_9,  &g_origI32_10, &g_origI32_11,
    &g_origI32_12, &g_origI32_13, &g_origI32_14, &g_origI32_15,
};
static void* g_i32Hooks[] = {
    (void*)Hooked_I32_0,  (void*)Hooked_I32_1,  (void*)Hooked_I32_2,  (void*)Hooked_I32_3,
    (void*)Hooked_I32_4,  (void*)Hooked_I32_5,  (void*)Hooked_I32_6,  (void*)Hooked_I32_7,
    (void*)Hooked_I32_8,  (void*)Hooked_I32_9,  (void*)Hooked_I32_10, (void*)Hooked_I32_11,
    (void*)Hooked_I32_12, (void*)Hooked_I32_13, (void*)Hooked_I32_14, (void*)Hooked_I32_15,
};
static I64Getter0_t* g_i64Origs[] = {
    &g_origI64_0, &g_origI64_1, &g_origI64_2, &g_origI64_3,
};
static void* g_i64Hooks[] = {
    (void*)Hooked_I64_0, (void*)Hooked_I64_1, (void*)Hooked_I64_2, (void*)Hooked_I64_3,
};

static bool HookI32(void* klass, const char* name, int slot) {
    if (slot < 0 || slot >= (int)(sizeof(g_i32Hooks) / sizeof(g_i32Hooks[0]))) return false;
    auto& api = GetApi();
    void* mi = api.GetMethod(klass, name, 0);
    void* fn = MethodPointer(mi);
    if (!fn || !Il2Cpp::IsValidHeapPtr(fn)) return false;
    if (DobbyHook(fn, g_i32Hooks[slot], (void**)g_i32Origs[slot]) != 0) {
        *g_i32Origs[slot] = nullptr;
        return false;
    }
    LOGI("[Pcr] hooked i32 %s", name);
    return true;
}

static bool HookI64(void* klass, const char* name, int slot) {
    if (slot < 0 || slot >= (int)(sizeof(g_i64Hooks) / sizeof(g_i64Hooks[0]))) return false;
    auto& api = GetApi();
    void* mi = api.GetMethod(klass, name, 0);
    void* fn = MethodPointer(mi);
    if (!fn || !Il2Cpp::IsValidHeapPtr(fn)) return false;
    if (DobbyHook(fn, g_i64Hooks[slot], (void**)g_i64Origs[slot]) != 0) {
        *g_i64Origs[slot] = nullptr;
        return false;
    }
    LOGI("[Pcr] hooked i64 %s", name);
    return true;
}

// ============================================================
// 安装（一次性）
// ============================================================
static bool InstallHooks() {
    if (g_ready.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) {
        static std::atomic<uint32_t> s_wait{0};
        uint32_t w = s_wait.fetch_add(1) + 1;
        if (w <= 3 || (w % 25) == 0) LOGI("[Pcr] wait %s (%u)", GameUnity::TARGET_LIB, w);
        return false;
    }

    auto& api = GetApi();
    if (!api.Load(GameUnity::TARGET_LIB)) {
        LOGE("[Pcr] Il2Cpp Api::Load failed");
        return false;
    }

    // 1) 变速
    void* timeCls = FindClassNs("UnityEngine", "Time");
    if (!timeCls) {
        LOGE("[Pcr] UnityEngine.Time not found");
        return false;
    }
    void* miDelta = api.GetMethod(timeCls, "get_deltaTime", 0);
    void* fnDelta = MethodPointer(miDelta);
    if (!fnDelta || !Il2Cpp::IsValidHeapPtr(fnDelta)) {
        LOGE("[Pcr] get_deltaTime ptr missing");
        return false;
    }
    if (DobbyHook(fnDelta, (void*)Hooked_get_deltaTime, (void**)&g_origDelta) != 0) {
        LOGE("[Pcr] DobbyHook get_deltaTime failed");
        g_origDelta = nullptr;
        return false;
    }
    LOGI("[Pcr] hooked Time.get_deltaTime");

    // 2) 敌我
    g_unitCtrlKlass = FindClassNs("Elements", "UnitCtrl");
    g_unitDataKlass = FindClassNs("Elements", "UnitData");
    if (!g_unitCtrlKlass || !g_unitDataKlass) {
        LOGE("[Pcr] UnitCtrl/UnitData missing");
        return false;
    }
    ResolveUnitCtrlOffsets(g_unitCtrlKlass);

    void* miSide = api.GetMethod(g_unitCtrlKlass, "get_IsPlayerUnit", 0);
    void* fnSide = MethodPointer(miSide);
    if (!fnSide || !Il2Cpp::IsValidHeapPtr(fnSide)) {
        LOGE("[Pcr] get_IsPlayerUnit missing");
        return false;
    }
    if (DobbyHook(fnSide, (void*)Hooked_IsPlayerUnit, (void**)&g_origIsPlayerUnit) != 0) {
        LOGE("[Pcr] DobbyHook get_IsPlayerUnit failed");
        g_origIsPlayerUnit = nullptr;
        return false;
    }
    g_miIsPlayerUnit = miSide;
    LOGI("[Pcr] hooked UnitCtrl.get_IsPlayerUnit off=0x%zx param=%d", g_offUnitOnCtrl,
         g_offIsParam ? 1 : 0);

    // 3) 属性 Total*
    int ok = 0, s64 = 0, s32 = 0;
    static const char* kI64[] = {"get_TotalHp", "get_TotalHpWithoutExtra"};
    for (const char* n : kI64) {
        if (s64 >= (int)(sizeof(g_i64Hooks) / sizeof(g_i64Hooks[0]))) break;
        if (HookI64(g_unitDataKlass, n, s64)) {
            s64++;
            ok++;
        }
    }
    static const char* kI32[] = {
        "get_TotalAtk",
        "get_TotalDef",
        "get_TotalMagicAtk",
        "get_TotalMagicDef",
        "get_TotalCritical",
        "get_TotalMagicCritical",
        "get_TotalDodge",
        "get_TotalAccuracy",
        "get_TotalPhysicalPenetrate",
        "get_TotalMagicPenetrate",
        "get_TotalLifeSteal",
        "get_TotalHpRecoveryRate",
        "get_TotalEnergyRecoveryRate",
        "get_TotalEnergyReduceRate",
        "get_TotalWaveHpRecovery",
        "get_TotalWaveEnergyRecovery",
    };
    for (const char* n : kI32) {
        if (s32 >= (int)(sizeof(g_i32Hooks) / sizeof(g_i32Hooks[0]))) break;
        if (HookI32(g_unitDataKlass, n, s32)) {
            s32++;
            ok++;
        }
    }
    LOGI("[Pcr] attr Total* hooks=%d (i64=%d i32=%d)", ok, s64, s32);

    g_ready.store(true);
    LOGI("[Pcr] ready attrOn=%d attrVal=%d speed×%d", g_attrOn.load() ? 1 : 0, g_attrVal.load(),
         g_speedMul.load());
    return true;
}

// ============================================================
// 对外接口
// ============================================================
bool Pcr_IsReady() { return g_ready.load(); }

bool Pcr_TryInit() { return InstallHooks(); }

void Pcr_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == PCR_FEAT_ATTR_ON) {
        g_attrOn.store(enabled);
        if (!enabled) ClearSides();
        LOGI("[Pcr] 开启属性修改=%s", enabled ? "ON" : "OFF");
    }
}

void Pcr_OnValueChanged(int featNum, int value) {
    if (featNum == PCR_FEAT_ATTR_VAL) {
        if (value < 1) value = 1;
        if (value > kAttrValMax) value = kAttrValMax;
        int prev = g_attrVal.exchange(value);
        if (prev != value) LOGI("[Pcr] 修改属性=%d", value);
    } else if (featNum == PCR_FEAT_SPEED) {
        if (value < 1) value = 1;
        if (value > 10) value = 10;
        int prev = g_speedMul.exchange(value);
        if (prev != value) LOGI("[Pcr] 全局变速=×%d", value);
    }
}

void Pcr_OnInputChanged(int featNum, const char* text) {
    if (featNum != PCR_FEAT_ATTR_VAL || !text) return;
    // 空串 / 非数字：忽略（框架对输入框的「重置」就是下发空串，保持现值）
    const char* p = text;
    while (*p == ' ' || *p == '\t') p++;
    if (!*p) return;
    char* end = nullptr;
    long long v = strtoll(p, &end, 10);
    if (end == p) return;
    if (v < 1) v = 1;
    if (v > kAttrValMax) v = kAttrValMax;
    int prev = g_attrVal.exchange(static_cast<int>(v));
    if (prev != static_cast<int>(v)) LOGI("[Pcr] 修改属性(输入框)=%d", (int)v);
}

void Pcr_ForceClear() {
    g_attrOn.store(false);
    g_speedMul.store(1);
    ClearSides();
}

void Pcr_Tick(const bool* features) {
    if (!features) return;
    bool on = features[PCR_FEAT_ATTR_ON];
    if (!on && g_attrOn.load()) ClearSides();
    g_attrOn.store(on);
    if (!g_ready.load()) InstallHooks();
}
