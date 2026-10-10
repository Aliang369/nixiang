/* ================================================================
 *  明日方舟 2.7.71 星光实现（arm64-v8a / Unity IL2CPP）
 *
 *  设计边界：
 *    - 类和字段只按精确命名空间/类名解析，Deck.Card/TokenCard 走嵌套类型校验；
 *    - 功能按需懒装，功能全关时不改游戏函数；
 *    - 所有 Hook 保留 orig 调用，关闭时返回原行为；
 *    - Hook/热路径不做 SafeProbe、扫堆和每帧字符串解析；
 *    - 可选依赖缺失只降级当前功能，不阻断独立挂点。
 *
 *  运行时字段/方法来自官方 2.7.71，与斌哥包核心 so/metadata 同哈希。
 * ================================================================ */

#include "ArknightsMod.h"

#include <atomic>
#include <chrono>
#include <climits>
#include <cstdint>
#include <cstring>
#include <dlfcn.h>
#include <mutex>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/SecStrings.hpp"
#include "Includes/StarPtr.h"
#include "Includes/Utils.hpp"
#include "Unity/Il2CppApi.h"
#include "xDL/xdl.h"

using Il2Cpp::GetApi;
using Il2Cpp::IsValidHeapPtr;

#define MRFZZ_LOG "[mrfzz]"

namespace {

constexpr int32_t  kSideAlly       = 1;
constexpr int32_t  kBuildableAll   = 3;
constexpr int64_t  kFpOne          = 0x100000000LL;
constexpr int64_t  kFp99999        = 99999LL * kFpOne;

std::atomic<bool> g_libReady{false};
std::atomic<bool> g_anyHooked{false};

std::atomic<bool> g_attackSpeed{false};
std::atomic<bool> g_invincible{false};
std::atomic<bool> g_baseLock{false};
std::atomic<bool> g_damage10{false};
std::atomic<bool> g_deployAnywhere{false};
std::atomic<bool> g_infiniteSkill{false};
std::atomic<bool> g_zeroCost{false};

std::mutex g_installMutex;
thread_local int g_hookDepth = 0;

struct HookGuard {
    HookGuard() { ++g_hookDepth; }
    ~HookGuard() { --g_hookDepth; }
    HookGuard(const HookGuard&) = delete;
    HookGuard& operator=(const HookGuard&) = delete;
};

static bool InHook() { return g_hookDepth > 0; }

template <typename Fn>
static void NoThrow(const char* what, Fn&& fn) noexcept {
    try {
        fn();
    } catch (...) {
        LOGW(MRFZZ_LOG " optional path failed: %s", what ? what : "unknown");
    }
}

static bool OkPtr(const void* p) {
    return p && IsValidHeapPtr(const_cast<void*>(p));
}

static void MarkFirst(const char* name, std::atomic<bool>& seen) {
    if (!seen.exchange(true, std::memory_order_relaxed))
        LOGI(MRFZZ_LOG " hit %s", name);
}

static void* MethodPtr(void* methodInfo) {
    if (!methodInfo) return nullptr;
    return *reinterpret_cast<void**>(methodInfo);
}

typedef uint8_t (*class_is_assignable_t)(void* base, void* derived);
typedef void* (*class_get_nested_types_t)(void* klass, void** iter);

struct ExtApi {
    class_is_assignable_t is_assignable = nullptr;
    class_get_nested_types_t nested_types = nullptr;
    bool loaded = false;
};

static ExtApi g_ext;
static std::once_flag g_extOnce;

static void* ResolveLocalSymbol(const char* name) {
    if (!name) return nullptr;
    void* p = nullptr;
    void* h = dlopen(sec_lib_il2cpp(), RTLD_NOW | RTLD_NOLOAD);
    if (h) {
        p = dlsym(h, name);
        dlclose(h);
    }
    if (p) return p;

    void* xh = xdl_open(sec_lib_il2cpp(), XDL_DEFAULT);
    if (!xh) return nullptr;
    size_t size = 0;
    p = xdl_sym(xh, name, &size);
    xdl_close(xh);
    return p;
}

static void LoadExtApi() {
    std::call_once(g_extOnce, [] {
        g_ext.is_assignable = reinterpret_cast<class_is_assignable_t>(
            ResolveLocalSymbol("il2cpp_class_is_assignable_from"));
        g_ext.nested_types = reinterpret_cast<class_get_nested_types_t>(
            ResolveLocalSymbol("il2cpp_class_get_nested_types"));
        g_ext.loaded = true;
        LOGI(MRFZZ_LOG " ext api assignable=%d nested=%d",
             g_ext.is_assignable != nullptr, g_ext.nested_types != nullptr);
    });
}

static bool EnsureApi() {
    LoadExtApi();
    if (GetApi().ready) return true;
    return GetApi().Load(sec_lib_il2cpp());
}

/* ================================================================
 *  程序集解析加固（2026-10-10，针对 2.7.81 实测「七项全灭」的排查产物）
 *
 *  现象（09_test/_recorder/mrfzz_20261010/logcat.txt，第二轮）：
 *      [mrfzz] status hooked=0/7 assemblies=88 entity=0x0 apiReady=1
 *    —— 枚举到 88 个程序集，但 Api::ImageOf() 对它们**全部**静默返回 nullptr：
 *    so 里确实含 "[Il2Cpp] ImageOf asm=" 串，整份 logcat 却 0 次出现，
 *    ⇒ 死在 ImageOf 自己日志之前的静默守卫（IsValidHeapPtr / IsReadableRange）。
 *    entity=0x0 ⇒ FindClassNs 的 for 循环全程 continue ⇒ 七项全灭。
 *
 *  对策（只在本游戏模块内做，不动共享层 Il2CppApi.cpp）：
 *    ① api.ImageOf()            —— 原路径，能用就用；
 *    ② 自己读 Il2CppAssembly 首字段 —— 读前用 Il2Cpp::IsReadableRange() 精确守卫，
 *       并同时试探「原指针」与 starptr::Untag() 后的地址（挡带标签/MTR 指针）；
 *    ③ 按名兜底 domain_assembly_open("Assembly-CSharp" 等) —— 绕开「已加载枚举」；
 *       该程序集由 runtime metadata 反查确认：Torappu.Battle.Entity 的 type idx 10202
 *       落在 Assembly-CSharp.dll（typeStart=0, typeCount=38093）区间内。
 *    ④ 三级都失败时打诊断行（含前 4 个程序集的原始指针与各守卫判定结果），
 *       让下一轮日志能直接定位是哪一级/哪条守卫的问题。
 * ================================================================ */
std::atomic<int> g_imgViaApi{0};
std::atomic<int> g_imgViaRead{0};
std::atomic<int> g_imgViaApiImg{0};
std::atomic<int> g_imgFail{0};

struct ImageList {
    void* items[8];
    size_t n;
};

static ImageList& ByNameImages();
static void* ImageOfRobust(void* asmPtr);

static void* ImageOfRobust(void* asmPtr) {
    if (!asmPtr) return nullptr;
    auto& api = GetApi();

    if (void* img = api.ImageOf(asmPtr)) {
        g_imgViaApi.fetch_add(1, std::memory_order_relaxed);
        return img;
    }

    /* 路径②：自己读首字段（Il2CppAssembly 首字段 = Il2CppImage*）。
     * 先判可读再解引用，坏指针只返回 nullptr，不带崩进程。 */
    const uintptr_t raw = (uintptr_t)asmPtr;
    const uintptr_t cand[2] = {starptr::Untag(raw), raw};
    for (int i = 0; i < 2; ++i) {
        if (i == 1 && cand[1] == cand[0]) break;
        const uintptr_t a = cand[i];
        if (!Il2Cpp::IsReadableRange(a, sizeof(void*))) continue;
        void* first = nullptr;
        memcpy(&first, (const void*)a, sizeof(first));
        if (!first) continue;
        const uintptr_t fi = starptr::Untag((uintptr_t)first);
        if (!Il2Cpp::IsReadableRange(fi, sizeof(void*))) continue;
        g_imgViaRead.fetch_add(1, std::memory_order_relaxed);
        return (void*)fi;
    }

    g_imgFail.fetch_add(1, std::memory_order_relaxed);
    return nullptr;
}

/* 按名取程序集镜像（只做一次；仅当枚举路径全部失效时才有实际价值）。
 * 指针来自 il2cpp_domain_assembly_open，可信，故允许叠加 il2cpp_assembly_get_image。 */
static ImageList& ByNameImages() {
    static std::once_flag once;
    static ImageList list = {{nullptr}, 0};
    std::call_once(once, [] {
        auto& api = GetApi();
        if (!api.domain_assembly_open) {
            LOGW(MRFZZ_LOG " by-name: no domain_assembly_open export");
            return;
        }
        void* domain = api.domain_get ? api.domain_get() : nullptr;
        if (!domain) {
            LOGW(MRFZZ_LOG " by-name: domain null");
            return;
        }
        static const char* kNames[] = {
            "Assembly-CSharp", "Assembly-CSharp.dll", "Torappu.Common",
            "Torappu.Battle", "ThirdPartyAssembly", "mscorlib",
            "UnityEngine", "UnityEngine.CoreModule"};
        for (const char* n : kNames) {
            void* a = api.domain_assembly_open(domain, n);
            if (!a) {
                LOGI(MRFZZ_LOG " by-name miss: %s", n);
                continue;
            }
            void* img = ImageOfRobust(a);
            if (!img && api.assembly_image) {
                img = const_cast<void*>(api.assembly_image((const void*)a));
                if (img) g_imgViaApiImg.fetch_add(1, std::memory_order_relaxed);
            }
            LOGI(MRFZZ_LOG " by-name ok: %s asm=%p img=%p", n, a, img);
            if (img && list.n < (sizeof(list.items) / sizeof(list.items[0]))) {
                list.items[list.n++] = img;
            }
        }
        LOGI(MRFZZ_LOG " by-name images=%zu", list.n);
    });
    return list;
}

static void* FindClassNs(const char* ns, const char* name) {
    if (!ns || !name) return nullptr;
    auto& api = GetApi();
    size_t count = 0;
    void** assemblies = api.Assemblies(&count);
    for (size_t i = 0; assemblies && i < count; ++i) {
        void* image = ImageOfRobust(assemblies[i]);
        if (!image) continue;
        void* klass = api.ClassFromName(image, ns, name);
        if (klass) return klass;
    }
    /* 按名兜底：枚举路径全灭（本版实测）时仍能拿到 Assembly-CSharp 的镜像。 */
    ImageList& byName = ByNameImages();
    for (size_t i = 0; i < byName.n; ++i) {
        if (!byName.items[i]) continue;
        if (void* klass = api.ClassFromName(byName.items[i], ns, name)) return klass;
    }
    return nullptr;
}

/* 一次性解析诊断：把「哪一级成功/哪个守卫判否」写进日志，供下一轮直接判读。 */
static void LogResolveDiagOnce() {
    static std::once_flag once;
    std::call_once(once, [] {
        auto& api = GetApi();
        size_t count = 0;
        void** assemblies = api.Assemblies(&count);
        LOGI(MRFZZ_LOG " diag: ready=%d assemblies=%zu ptr=%p",
             api.ready ? 1 : 0, count, (void*)assemblies);
        for (size_t i = 0; assemblies && i < count && i < 4; ++i) {
            void* a = assemblies[i];
            LOGI(MRFZZ_LOG " diag: asm[%zu]=%p valid=%d readRaw=%d readUntag=%d sz=%d",
                 i, a, Il2Cpp::IsValidHeapPtr(a) ? 1 : 0,
                 Il2Cpp::IsReadableRange((uintptr_t)a, 8) ? 1 : 0,
                 Il2Cpp::IsReadableRange(starptr::Untag((uintptr_t)a), 8) ? 1 : 0,
                 (int)sizeof(void*));
        }
        void* clsObj = FindClassNs("System", "Object");
        void* clsGo  = FindClassNs("UnityEngine", "GameObject");
        void* clsEnt = FindClassNs("Torappu.Battle", "Entity");
        LOGI(MRFZZ_LOG " diag: System.Object=%p UnityEngine.GameObject=%p "
                       "Torappu.Battle.Entity=%p", clsObj, clsGo, clsEnt);
        LOGI(MRFZZ_LOG " diag: imgVia api=%d selfread=%d apiImg=%d fail=%d byName=%zu",
             g_imgViaApi.load(), g_imgViaRead.load(), g_imgViaApiImg.load(),
             g_imgFail.load(), ByNameImages().n);
    });
}

static void* ResolveNestedClass(const char* ns, const char* outerName,
                                const char* nestedName) {
    if (!nestedName) return nullptr;
    void* outer = FindClassNs(ns, outerName);
    if (!outer) return nullptr;
    if (!g_ext.nested_types) return nullptr;

    auto& api = GetApi();
    void* iter = nullptr;
    for (;;) {
        void* nested = g_ext.nested_types(outer, &iter);
        if (!nested) break;
        const char* name = api.class_name ? api.class_name(nested) : nullptr;
        if (name && strcmp(name, nestedName) == 0) return nested;
    }
    LOGW(MRFZZ_LOG " nested miss %s.%s/%s", ns ? ns : "?", outerName, nestedName);
    return nullptr;
}

static void* FindFieldInHierarchy(void* klass, const char* name) {
    if (!klass || !name) return nullptr;
    auto& api = GetApi();
    for (void* cur = klass; cur; cur = api.class_parent ? api.class_parent(cur) : nullptr) {
        void* field = api.class_get_field ? api.class_get_field(cur, name) : nullptr;
        if (field) return field;
    }
    return nullptr;
}

static void* GetMethodInHierarchy(void* klass, const char* name, int argc) {
    if (!klass || !name) return nullptr;
    auto& api = GetApi();
    for (void* cur = klass; cur; cur = api.class_parent ? api.class_parent(cur) : nullptr) {
        void* method = api.GetMethod(cur, name, argc);
        if (method && MethodPtr(method)) return method;
    }
    return nullptr;
}

static size_t FieldOffset(void* klass, const char* name) {
    void* field = FindFieldInHierarchy(klass, name);
    if (!field || !GetApi().field_offset) return 0;
    return GetApi().field_offset(field);
}

static void* ObjectClass(void* obj) {
    auto& api = GetApi();
    if (!OkPtr(obj) || !api.object_class) return nullptr;
    return api.object_class(obj);
}

static bool IsKindOf(void* base, void* derived) {
    if (!base || !derived) return false;
    if (g_ext.is_assignable) return g_ext.is_assignable(base, derived) != 0;
    auto& api = GetApi();
    for (void* cur = derived; cur; cur = api.class_parent ? api.class_parent(cur) : nullptr) {
        if (cur == base) return true;
    }
    return false;
}

static int32_t GetSideRaw(void* entity);
static bool IsAlly(void* entity) {
    return entity && GetSideRaw(entity) == kSideAlly;
}

// ============================================================
// Hook signatures
// ============================================================
typedef int64_t (*calc_attack_interval_t)(int64_t baseAttackTime, int64_t attackSpeed,
                                          void* method);
typedef bool    (*get_bool_t)(void* self, void* method);
typedef int32_t (*get_i32_t)(void* self, void* method);
typedef int64_t (*get_fp_t)(void* self, void* method);
typedef int32_t (*get_side_t)(void* self, void* method);

static calc_attack_interval_t g_origCalcAttackInterval = nullptr;
static get_bool_t g_origIsInvincible = nullptr;
static get_i32_t g_origLifePointReduce = nullptr;
static get_fp_t g_origAtk = nullptr;
static get_i32_t g_origBuildableType = nullptr;
static get_i32_t g_origCardCost = nullptr;
static get_i32_t g_origTokenCardCost = nullptr;
static get_fp_t g_origSp = nullptr;
static get_i32_t g_origAvailableCnt = nullptr;
static get_bool_t g_origIsUsedUp = nullptr;

static get_side_t g_getSide = nullptr;
static void* g_miGetSide = nullptr;

static std::atomic<bool> g_loggedAtkSpeed{false};
static std::atomic<bool> g_loggedInvincible{false};
static std::atomic<bool> g_loggedBaseLock{false};
static std::atomic<bool> g_loggedDamage10{false};
static std::atomic<bool> g_loggedDeployAnywhere{false};
static std::atomic<bool> g_loggedInfiniteSkill{false};
static std::atomic<bool> g_loggedZeroCost{false};

static int32_t GetSideRaw(void* entity) {
    if (!OkPtr(entity) || !g_getSide || !g_miGetSide) return -1;
    return g_getSide(entity, g_miGetSide);
}

// ============================================================
// Replacement functions
// ============================================================
static int64_t Hooked_CalcAttackInterval(int64_t baseAttackTime, int64_t attackSpeed,
                                         void* method) {
    if (!g_origCalcAttackInterval) return 0;
    if (InHook()) return g_origCalcAttackInterval(baseAttackTime, attackSpeed, method);
    HookGuard guard;
    int64_t result = g_origCalcAttackInterval(baseAttackTime, attackSpeed, method);
    if (!g_attackSpeed.load(std::memory_order_relaxed)) return result;
    MarkFirst("attack_speed", g_loggedAtkSpeed);
    return 0;
}

static bool Hooked_IsInvincible(void* self, void* method) {
    if (!g_origIsInvincible) return false;
    if (InHook()) return g_origIsInvincible(self, method);
    HookGuard guard;
    bool result = g_origIsInvincible(self, method);
    if (result || !g_invincible.load(std::memory_order_relaxed)) return result;
    if (!IsAlly(self)) return result;
    MarkFirst("invincible", g_loggedInvincible);
    return true;
}

static int32_t Hooked_LifePointReduce(void* self, void* method) {
    if (!g_origLifePointReduce) return 0;
    if (InHook()) return g_origLifePointReduce(self, method);
    HookGuard guard;
    int32_t result = g_origLifePointReduce(self, method);
    if (!g_baseLock.load(std::memory_order_relaxed)) return result;
    MarkFirst("base_lock", g_loggedBaseLock);
    return 0;
}

static int64_t Hooked_GetAtk(void* self, void* method) {
    if (!g_origAtk) return 0;
    if (InHook()) return g_origAtk(self, method);
    HookGuard guard;
    int64_t value = g_origAtk(self, method);
    if (!g_damage10.load(std::memory_order_relaxed)) return value;
    if (value > LLONG_MAX / 10) return LLONG_MAX;
    if (value < LLONG_MIN / 10) return LLONG_MIN;
    MarkFirst("damage10", g_loggedDamage10);
    return value * 10;
}

static int32_t Hooked_BuildableType(void* self, void* method) {
    if (!g_origBuildableType) return 0;
    if (InHook()) return g_origBuildableType(self, method);
    HookGuard guard;
    int32_t result = g_origBuildableType(self, method);
    if (!g_deployAnywhere.load(std::memory_order_relaxed)) return result;
    MarkFirst("deploy_anywhere", g_loggedDeployAnywhere);
    return kBuildableAll;
}

static int32_t Hooked_CardCost(void* self, void* method) {
    if (!g_origCardCost) return 0;
    if (InHook()) return g_origCardCost(self, method);
    HookGuard guard;
    int32_t result = g_origCardCost(self, method);
    if (!g_zeroCost.load(std::memory_order_relaxed)) return result;
    MarkFirst("zero_cost_card", g_loggedZeroCost);
    return 0;
}

static int32_t Hooked_TokenCardCost(void* self, void* method) {
    if (!g_origTokenCardCost) return 0;
    if (InHook()) return g_origTokenCardCost(self, method);
    HookGuard guard;
    int32_t result = g_origTokenCardCost(self, method);
    if (!g_zeroCost.load(std::memory_order_relaxed)) return result;
    MarkFirst("zero_cost_token", g_loggedZeroCost);
    return 0;
}

static int64_t Hooked_GetSp(void* self, void* method) {
    if (!g_origSp) return 0;
    if (InHook()) return g_origSp(self, method);
    HookGuard guard;
    int64_t result = g_origSp(self, method);
    if (!g_infiniteSkill.load(std::memory_order_relaxed) || !IsAlly(self)) return result;
    MarkFirst("infinite_sp", g_loggedInfiniteSkill);
    return kFp99999;
}

static int32_t Hooked_AvailableCnt(void* self, void* method) {
    if (!g_origAvailableCnt) return 0;
    if (InHook()) return g_origAvailableCnt(self, method);
    HookGuard guard;
    int32_t result = g_origAvailableCnt(self, method);
    if (!g_infiniteSkill.load(std::memory_order_relaxed)) return result;
    MarkFirst("infinite_count", g_loggedInfiniteSkill);
    return 9;
}

static bool Hooked_IsUsedUp(void* self, void* method) {
    if (!g_origIsUsedUp) return true;
    if (InHook()) return g_origIsUsedUp(self, method);
    HookGuard guard;
    bool result = g_origIsUsedUp(self, method);
    if (!g_infiniteSkill.load(std::memory_order_relaxed)) return result;
    MarkFirst("infinite_usedup", g_loggedInfiniteSkill);
    return false;
}

// ============================================================
// Install helpers
// ============================================================
static bool HookMethod(const char* tag, void* klass, const char* method, int argc,
                       void* replacement, void** orig) {
    if (!klass || !method || !replacement || !orig) return false;
    if (*orig) return true;
    void* methodInfo = GetApi().GetMethod(klass, method, argc);
    void* target = MethodPtr(methodInfo);
    if (!target) {
        LOGW(MRFZZ_LOG " method miss %s argc=%d", method, argc);
        return false;
    }
    if (DobbyHook(target, replacement, orig) != 0) {
        *orig = nullptr;
        LOGW(MRFZZ_LOG " dobby fail %s", method);
        return false;
    }
    g_anyHooked.store(true);
    LOGI(MRFZZ_LOG " hooked %s argc=%d target=%p", method, argc, target);
    return true;
}

static void EnsureSideGetter() {
    if (g_getSide) return;
    void* entity = FindClassNs("Torappu.Battle", "Entity");
    if (!entity) return;
    g_miGetSide = GetApi().GetMethod(entity, "get_side", 0);
    g_getSide = reinterpret_cast<get_side_t>(MethodPtr(g_miGetSide));
}

static int g_attackSpeedAttempts = 0;
static int g_invincibleAttempts = 0;
static int g_baseLockAttempts = 0;
static int g_damage10Attempts = 0;
static int g_deployAttempts = 0;
static int g_infiniteSkillAttempts = 0;
static int g_zeroCostAttempts = 0;

static std::atomic<bool> g_attackSpeedDone{false};
static std::atomic<bool> g_invincibleDone{false};
static std::atomic<bool> g_baseLockDone{false};
static std::atomic<bool> g_damage10Done{false};
static std::atomic<bool> g_deployDone{false};
static std::atomic<bool> g_infiniteSkillDone{false};
static std::atomic<bool> g_zeroCostDone{false};

static void InstallAttackSpeedHooks() {
    if (g_attackSpeedDone.load()) return;
    ++g_attackSpeedAttempts;
    void* klass = FindClassNs("Torappu.Battle", "BattleFormula");
    if (!klass) {
        if (g_attackSpeedAttempts == 1)
            LOGE(MRFZZ_LOG " attack speed: class BattleFormula not resolvable yet (will retry)");
        else if (g_attackSpeedAttempts % 40 == 0)
            LOGW(MRFZZ_LOG " attack speed: still no class after %d tries", g_attackSpeedAttempts);
        return;
    }
    if (HookMethod("attack_speed", klass, "CalculateAttackInterval", 2,
                   reinterpret_cast<void*>(Hooked_CalcAttackInterval),
                   reinterpret_cast<void**>(&g_origCalcAttackInterval)))
        g_attackSpeedDone.store(true);
    else if (g_attackSpeedAttempts % 40 == 0)
        LOGW(MRFZZ_LOG " attack speed: hook still failing after %d tries", g_attackSpeedAttempts);
}

static void InstallInvincibleHooks() {
    if (g_invincibleDone.load()) return;
    ++g_invincibleAttempts;
    EnsureSideGetter();
    if (!g_getSide) {
        if (g_invincibleAttempts == 1)
            LOGE(MRFZZ_LOG " invincible: Entity.get_side not resolvable yet (will retry)");
        else if (g_invincibleAttempts % 40 == 0)
            LOGW(MRFZZ_LOG " invincible: still no Entity.get_side after %d tries", g_invincibleAttempts);
        return;
    }
    void* klass = FindClassNs("Torappu.Battle", "Entity");
    if (!klass) return;
    if (HookMethod("invincible", klass, "get_isInvincible", 0,
                   reinterpret_cast<void*>(Hooked_IsInvincible),
                   reinterpret_cast<void**>(&g_origIsInvincible)))
        g_invincibleDone.store(true);
}

static void InstallBaseLockHooks() {
    if (g_baseLockDone.load()) return;
    ++g_baseLockAttempts;
    void* klass = FindClassNs("Torappu.Battle", "Enemy");
    if (!klass) {
        if (g_baseLockAttempts == 1)
            LOGE(MRFZZ_LOG " base lock: class Enemy not resolvable yet (will retry)");
        else if (g_baseLockAttempts % 40 == 0)
            LOGW(MRFZZ_LOG " base lock: still no class after %d tries", g_baseLockAttempts);
        return;
    }
    if (HookMethod("base_lock", klass, "get_lifePointReduce", 0,
                   reinterpret_cast<void*>(Hooked_LifePointReduce),
                   reinterpret_cast<void**>(&g_origLifePointReduce)))
        g_baseLockDone.store(true);
    else if (g_baseLockAttempts % 40 == 0)
        LOGW(MRFZZ_LOG " base lock: hook still failing after %d tries", g_baseLockAttempts);
}

static void InstallDamage10Hooks() {
    if (g_damage10Done.load()) return;
    ++g_damage10Attempts;
    void* klass = FindClassNs("Torappu.Battle", "Entity");
    if (!klass) {
        if (g_damage10Attempts == 1)
            LOGE(MRFZZ_LOG " damage10: class Entity not resolvable yet (will retry)");
        else if (g_damage10Attempts % 40 == 0)
            LOGW(MRFZZ_LOG " damage10: still no class after %d tries", g_damage10Attempts);
        return;
    }
    if (HookMethod("damage10", klass, "get_atk", 0,
                   reinterpret_cast<void*>(Hooked_GetAtk),
                   reinterpret_cast<void**>(&g_origAtk)))
        g_damage10Done.store(true);
    else if (g_damage10Attempts % 40 == 0)
        LOGW(MRFZZ_LOG " damage10: hook still failing after %d tries", g_damage10Attempts);
}

static void InstallDeployAnywhereHooks() {
    if (g_deployDone.load()) return;
    ++g_deployAttempts;
    void* klass = FindClassNs("Torappu.Battle", "Tile");
    if (!klass) {
        if (g_deployAttempts == 1)
            LOGE(MRFZZ_LOG " deploy anywhere: class Tile not resolvable yet (will retry)");
        else if (g_deployAttempts % 40 == 0)
            LOGW(MRFZZ_LOG " deploy anywhere: still no class after %d tries", g_deployAttempts);
        return;
    }
    if (HookMethod("deploy_anywhere", klass, "get_buildableType", 0,
                   reinterpret_cast<void*>(Hooked_BuildableType),
                   reinterpret_cast<void**>(&g_origBuildableType)))
        g_deployDone.store(true);
    else if (g_deployAttempts % 40 == 0)
        LOGW(MRFZZ_LOG " deploy anywhere: hook still failing after %d tries", g_deployAttempts);
}

static void InstallZeroCostHooks() {
    if (g_zeroCostDone.load()) return;
    ++g_zeroCostAttempts;
    void* card = ResolveNestedClass("Torappu.Battle", "Deck", "Card");
    void* token = ResolveNestedClass("Torappu.Battle", "Deck", "TokenCard");
    if (!card && !token) {
        if (g_zeroCostAttempts == 1)
            LOGE(MRFZZ_LOG " zero cost: nested Deck/Card+TokenCard not resolvable yet (will retry)");
        else if (g_zeroCostAttempts % 40 == 0)
            LOGW(MRFZZ_LOG " zero cost: still no nested class after %d tries", g_zeroCostAttempts);
        return;
    }
    bool cardOk = HookMethod("zero_cost_card", card, "get_cost", 0,
                             reinterpret_cast<void*>(Hooked_CardCost),
                             reinterpret_cast<void**>(&g_origCardCost));
    bool tokenOk = HookMethod("zero_cost_token", token, "get_cost", 0,
                              reinterpret_cast<void*>(Hooked_TokenCardCost),
                              reinterpret_cast<void**>(&g_origTokenCardCost));
    if (cardOk && tokenOk) g_zeroCostDone.store(true);
}

static void InstallInfiniteSkillHooks() {
    if (g_infiniteSkillDone.load()) return;
    ++g_infiniteSkillAttempts;
    EnsureSideGetter();
    if (!g_getSide) {
        if (g_infiniteSkillAttempts == 1)
            LOGE(MRFZZ_LOG " infinite skill: Entity.get_side not resolvable yet (will retry)");
        else if (g_infiniteSkillAttempts % 40 == 0)
            LOGW(MRFZZ_LOG " infinite skill: still no Entity.get_side after %d tries", g_infiniteSkillAttempts);
        return;
    }
    void* entity = FindClassNs("Torappu.Battle", "Entity");
    void* basic = FindClassNs("Torappu.Battle", "BasicSkill");
    bool spOk = HookMethod("infinite_sp", entity, "get_sp", 0,
                           reinterpret_cast<void*>(Hooked_GetSp),
                           reinterpret_cast<void**>(&g_origSp));
    bool cntOk = HookMethod("infinite_cnt", basic, "get_availableCnt", 0,
                            reinterpret_cast<void*>(Hooked_AvailableCnt),
                            reinterpret_cast<void**>(&g_origAvailableCnt));
    bool usedOk = HookMethod("infinite_usedup", basic, "get_isUsedUp", 0,
                             reinterpret_cast<void*>(Hooked_IsUsedUp),
                             reinterpret_cast<void**>(&g_origIsUsedUp));
    if (spOk && cntOk && usedOk) g_infiniteSkillDone.store(true);
}

// ---- 诊断状态行：每 10 秒至多一条，仅当有待装功能时打印（供实测判读，非热路径）----
static int64_t NowMs() {
    using namespace std::chrono;
    return duration_cast<milliseconds>(steady_clock::now().time_since_epoch()).count();
}

static int DoneCount() {
    int n = 0;
    if (g_attackSpeedDone.load()) ++n;
    if (g_invincibleDone.load()) ++n;
    if (g_baseLockDone.load()) ++n;
    if (g_damage10Done.load()) ++n;
    if (g_deployDone.load()) ++n;
    if (g_infiniteSkillDone.load()) ++n;
    if (g_zeroCostDone.load()) ++n;
    return n;
}

static bool AnyEnabled() {
    return g_attackSpeed.load() || g_invincible.load() || g_baseLock.load() ||
           g_damage10.load() || g_deployAnywhere.load() || g_infiniteSkill.load() ||
           g_zeroCost.load();
}

static void LogStatusIfDue() {
    if (!AnyEnabled()) return;
    if (DoneCount() >= 7) return;
    static std::atomic<int64_t> s_nextMs{0};
    int64_t now = NowMs();
    int64_t next = s_nextMs.load(std::memory_order_relaxed);
    if (now < next) return;
    if (!s_nextMs.compare_exchange_strong(next, now + 10000)) return;
    if (!GetApi().ready) {
        LOGW(MRFZZ_LOG " status api not ready yet (hooked=%d/7)", DoneCount());
        return;
    }
    size_t count = 0;
    (void)GetApi().Assemblies(&count);
    LogResolveDiagOnce();
    void* entity = FindClassNs("Torappu.Battle", "Entity");
    LOGI(MRFZZ_LOG " status hooked=%d/7 assemblies=%zu entity=%p apiReady=%d",
         DoneCount(), count, entity, GetApi().ready ? 1 : 0);
}

static void SyncHooks() {
    if (!g_libReady.load(std::memory_order_relaxed)) return;
    std::lock_guard<std::mutex> lock(g_installMutex);
    if (!EnsureApi()) return;
    if (g_attackSpeed.load(std::memory_order_relaxed)) InstallAttackSpeedHooks();
    if (g_invincible.load(std::memory_order_relaxed)) InstallInvincibleHooks();
    if (g_baseLock.load(std::memory_order_relaxed)) InstallBaseLockHooks();
    if (g_damage10.load(std::memory_order_relaxed)) InstallDamage10Hooks();
    if (g_deployAnywhere.load(std::memory_order_relaxed)) InstallDeployAnywhereHooks();
    if (g_infiniteSkill.load(std::memory_order_relaxed)) InstallInfiniteSkillHooks();
    if (g_zeroCost.load(std::memory_order_relaxed)) InstallZeroCostHooks();
}

}  // namespace

// ============================================================
// Registry contract
// ============================================================
bool Arknights_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(sec_lib_il2cpp())) return false;
    if (!EnsureApi()) return false;
    g_libReady.store(true);
    // 不在此处装 hook：TryInit 可能由主线程（Provider/JNI 首次调用）触发，
    // 安装交 worker tick 做（见 Arknights_Tick）。
    LOGI(MRFZZ_LOG " lib ready (runtime resolve, hooks installed by worker)");
    return true;
}

bool Arknights_IsReady() { return g_libReady.load(); }

void Arknights_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case ARKNIGHTS_FEAT_ATTACK_SPEED:    g_attackSpeed.store(enabled); break;
        case ARKNIGHTS_FEAT_INVINCIBLE:      g_invincible.store(enabled); break;
        case ARKNIGHTS_FEAT_BASE_LOCK:       g_baseLock.store(enabled); break;
        case ARKNIGHTS_FEAT_DAMAGE10:        g_damage10.store(enabled); break;
        case ARKNIGHTS_FEAT_DEPLOY_ANYWHERE: g_deployAnywhere.store(enabled); break;
        case ARKNIGHTS_FEAT_INFINITE_SKILL:  g_infiniteSkill.store(enabled); break;
        case ARKNIGHTS_FEAT_ZERO_COST:       g_zeroCost.store(enabled); break;
        default: return;
    }
    // 只置位后立刻返回：安装由 worker tick 完成。
    // 早期版本在这里同步调 SyncHooks()，UI 线程被 Dobby 安装 + g_installMutex
    // 阻塞 1~6 秒 ⇒ 菜单卡顿，最后 Input dispatching timed out（ANR）被杀。
    LOGI(MRFZZ_LOG " feature #%d -> %d (install deferred to worker)", featNum, enabled ? 1 : 0);
}

void Arknights_OnValueChanged(int /*featNum*/, int /*value*/) {}

void Arknights_Tick(const bool* features, const int* /*values*/) {
    if (features) {
        g_attackSpeed.store(features[ARKNIGHTS_FEAT_ATTACK_SPEED]);
        g_invincible.store(features[ARKNIGHTS_FEAT_INVINCIBLE]);
        g_baseLock.store(features[ARKNIGHTS_FEAT_BASE_LOCK]);
        g_damage10.store(features[ARKNIGHTS_FEAT_DAMAGE10]);
        g_deployAnywhere.store(features[ARKNIGHTS_FEAT_DEPLOY_ANYWHERE]);
        g_infiniteSkill.store(features[ARKNIGHTS_FEAT_INFINITE_SKILL]);
        g_zeroCost.store(features[ARKNIGHTS_FEAT_ZERO_COST]);
    }
    if (!g_libReady.load() && isLibraryLoaded(sec_lib_il2cpp()))
        g_libReady.store(true);
    SyncHooks();
    LogStatusIfDue();
}

void Arknights_ForceClear() {
    g_attackSpeed.store(false);
    g_invincible.store(false);
    g_baseLock.store(false);
    g_damage10.store(false);
    g_deployAnywhere.store(false);
    g_infiniteSkill.store(false);
    g_zeroCost.store(false);
}
