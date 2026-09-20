/* ================================================================
 *  弹壳战机 1.1.5 — 星光 Mod
 *
 *  输入同源已核对：原版与斌哥包 libil2cpp.so / global-metadata.dat
 *  SHA-256 完全一致，因此本文件固定 file offset 只适用于 1.1.5。
 *
 *  防闪退约束：
 *    - 每处 Hook 都先走序言校验；失败转按名，仍失败则完全不动该点
 *    - Hook 只在 orig 非空后进入；原函数始终在主逻辑中保留
 *    - 战斗热路径只做整数算术，不扫堆、不遍历对象、不做字符串解析
 *    - 技能补授最多处理 8 个候选，带 thread_local 重入保护
 *    - 免广告通过 il2cpp_runtime_invoke 调成功回调并检查异常，失败回退原流程
 * ================================================================ */

#include "DkzjMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <limits>
#include <mutex>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define DKZJ_LOG "[dkzj]"
#define DKZJ_TARGET_LIB sec_lib_il2cpp()

namespace {

constexpr int kAttackMin = 1;
constexpr int kAttackMax = 100;
constexpr int kMaxCandidates = 16;
constexpr int kMaxDiagnosticLogs = 8;

// 弹壳战机 1.1.5: libil2cpp file offsets. RVA = file offset + 0x4000.
constexpr uintptr_t kFileEntityCharacterChangeHp = 0x6B72618;
constexpr uintptr_t kFileEntityHeroChangeHp = 0x6C67610;
constexpr uintptr_t kFileOnAddSkill = 0x6C4C9A4;
constexpr uintptr_t kFileAddSkillSurvior = 0x6C4D85C;
constexpr uintptr_t kFileCheckAndPlayVideo = 0x7723920;

constexpr uint32_t kInsnEntityCharacterChangeHp = 0xA9BB6FFE;
constexpr uint32_t kInsnEntityHeroChangeHp = 0xF81D0FFE;
constexpr uint32_t kInsnOnAddSkill = 0xA9BB6FFE;
constexpr uint32_t kInsnCheckAndPlayVideo = 0xA9BC67FE;

// HeroComponentRandomSkill._randomSkillResult
constexpr size_t kOffHrsRandomSkillResult = 0x88;
// RandomSkillResult.<ShowSkills>k__BackingField
constexpr size_t kOffRandomSkillShowSkills = 0x60;
// List<int>._items / _size
constexpr size_t kOffListItems = 0x10;
constexpr size_t kOffListSize = 0x18;
// Il2CppArray.max_length / first element
constexpr size_t kOffArrayLength = 0x18;
constexpr size_t kOffArrayData = 0x20;

std::atomic<bool> g_libReady{false};
std::mutex g_hookMutex;
std::atomic<int> g_attackMultiplier{1};
std::atomic<bool> g_godMode{false};
std::atomic<bool> g_stealSkill{false};
std::atomic<bool> g_noAds{false};

std::atomic<uint32_t> g_damageHits{0};
std::atomic<uint32_t> g_godHits{0};
std::atomic<uint32_t> g_stealHits{0};
std::atomic<uint32_t> g_grantedSkills{0};
std::atomic<uint32_t> g_noAdsHits{0};
std::atomic<uint32_t> g_noAdsFallbacks{0};

thread_local int t_heroDepth = 0;
thread_local bool t_stealReenter = false;
thread_local bool t_adReenter = false;

typedef void (*ChangeHpFn)(void* self, int64_t hp, void* method);
typedef void (*OnAddSkillFn)(void* self, int32_t id, void* method);
typedef void (*AddSkillSurviorFn)(void* self, int32_t id, void* method);
typedef bool (*CheckAndPlayVideoFn)(void* self, void* source, void* onClose, void* method);

ChangeHpFn g_origEntityCharacterChangeHp = nullptr;
ChangeHpFn g_origEntityHeroChangeHp = nullptr;
OnAddSkillFn g_origOnAddSkill = nullptr;
CheckAndPlayVideoFn g_origCheckAndPlayVideo = nullptr;
std::atomic<AddSkillSurviorFn> g_fnAddSkillSurviorAtomic{nullptr};

std::atomic<void*> g_actionBoolClass{nullptr};
std::atomic<void*> g_miActionBoolInvoke{nullptr};

bool OkPtr(void* p) {
    return p && Il2Cpp::IsValidHeapPtr(p);
}

int ClampAttackMultiplier(int value) {
    if (value < kAttackMin) return kAttackMin;
    if (value > kAttackMax) return kAttackMax;
    return value;
}

int64_t SaturatingScaleNegative(int64_t value, int multiplier) {
    if (value >= 0 || multiplier <= 1) return value;
    const int64_t minValue = std::numeric_limits<int64_t>::min();
    if (value < minValue / multiplier) return minValue;
    return value * static_cast<int64_t>(multiplier);
}

bool IsLibMapLine(const char* line, const char* lib) {
    return line && lib && strstr(line, lib) && strstr(line, ".so");
}

// file offset -> RX VA. Do not use ELF base + offset on split mappings.
void* FileOffToVa(const char* lib, uintptr_t fileOff) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return nullptr;
    char line[768];
    void* found = nullptr;
    size_t bestSpan = 0;
    while (fgets(line, sizeof(line), fp)) {
        if (!IsLibMapLine(line, lib)) continue;
        unsigned long start = 0;
        unsigned long end = 0;
        unsigned long off = 0;
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

bool EnsureApi() {
    if (GetApi().ready) return true;
    return GetApi().Load(DKZJ_TARGET_LIB);
}

void* FindClassNs(const char* ns, const char* name) {
    auto& api = GetApi();
    size_t count = 0;
    void** assemblies = api.Assemblies(&count);
    if (!assemblies || count == 0) return nullptr;
    for (size_t i = 0; i < count; i++) {
        void* image = api.ImageOf(assemblies[i]);
        if (!image) continue;
        void* klass = api.ClassFromName(image, ns, name);
        if (klass) return klass;
    }
    return nullptr;
}

void* ResolveMethodPtr(const char* ns, const char* cls, const char* method, int argc,
                       void** methodInfoOut = nullptr) {
    if (!EnsureApi()) return nullptr;
    void* klass = FindClassNs(ns, cls);
    if (!klass) klass = GetApi().FindClassByShortName(cls);
    if (!klass) return nullptr;
    void* methodInfo = GetApi().GetMethod(klass, method, argc);
    if (methodInfoOut) *methodInfoOut = methodInfo;
    return methodInfo ? *reinterpret_cast<void**>(methodInfo) : nullptr;
}

bool HookAtFileOffset(const char* tag, uintptr_t fileOffset, uint32_t expectedInsn,
                      void* hook, void** orig) {
    void* target = FileOffToVa(DKZJ_TARGET_LIB, fileOffset);
    if (!target) {
        LOGW(DKZJ_LOG " %s file offset 0x%lX not in RX — try by-name",
             tag, static_cast<unsigned long>(fileOffset));
        return false;
    }
    if (expectedInsn) {
        uint32_t got = 0;
        memcpy(&got, target, sizeof(got));
        if (got != expectedInsn) {
            LOGW(DKZJ_LOG " %s prologue got=0x%08X want=0x%08X — reject",
                 tag, got, expectedInsn);
            return false;
        }
    }
    if (!DobbyHook(target, hook, orig)) {
        LOGI(DKZJ_LOG " %s hooked file=0x%lX", tag,
             static_cast<unsigned long>(fileOffset));
        return true;
    }
    LOGW(DKZJ_LOG " %s DobbyHook failed file=0x%lX", tag,
         static_cast<unsigned long>(fileOffset));
    return false;
}

bool HookResolvedMethod(const char* tag, const char* ns, const char* cls,
                        const char* method, int argc, void* hook, void** orig) {
    void* fn = ResolveMethodPtr(ns, cls, method, argc);
    if (!fn || !OkPtr(fn)) {
        LOGW(DKZJ_LOG " %s resolve miss %s.%s", tag, cls, method);
        return false;
    }
    if (DobbyHook(fn, hook, orig) == 0) {
        LOGI(DKZJ_LOG " %s hooked by-name %s.%s", tag, cls, method);
        return true;
    }
    LOGW(DKZJ_LOG " %s DobbyHook by-name failed %s.%s", tag, cls, method);
    return false;
}

struct HookSlot {
    const char* tag;
    uintptr_t fileOffset;
    uint32_t expectedInsn;
    const char* ns;
    const char* cls;
    const char* method;
    int argc;
    void* hook;
    void** orig;
};

bool InstallSlot(const HookSlot& slot) {
    if (slot.orig && *slot.orig) return true;
    if (HookAtFileOffset(slot.tag, slot.fileOffset, slot.expectedInsn,
                         slot.hook, slot.orig)) {
        return true;
    }
    return HookResolvedMethod(slot.tag, slot.ns, slot.cls, slot.method, slot.argc,
                              slot.hook, slot.orig);
}

void ResolveSkillAux() {
    if (!g_fnAddSkillSurviorAtomic.load(std::memory_order_relaxed)) {
        AddSkillSurviorFn fn = reinterpret_cast<AddSkillSurviorFn>(
            FileOffToVa(DKZJ_TARGET_LIB, kFileAddSkillSurvior));
        g_fnAddSkillSurviorAtomic.store(fn, std::memory_order_release);
    }
}

void ResetHeroDepth() {
    t_heroDepth = 0;
}

void Hooked_EntityHero_ChangeHp(void* self, int64_t hp, void* method) {
    if (!g_origEntityHeroChangeHp) return;
    struct DepthGuard {
        DepthGuard() { ++t_heroDepth; }
        ~DepthGuard() { if (t_heroDepth > 0) --t_heroDepth; }
    } guard;
    g_origEntityHeroChangeHp(self, hp, method);
}

void Hooked_EntityCharacter_ChangeHp(void* self, int64_t hp, void* method) {
    if (!g_origEntityCharacterChangeHp) return;

    int64_t adjustedHp = hp;
    if (t_heroDepth > 0) {
        if (g_godMode.load(std::memory_order_relaxed) && hp < 0) {
            adjustedHp = 0;
            const uint32_t hit = g_godHits.fetch_add(1, std::memory_order_relaxed);
            if (hit < kMaxDiagnosticLogs) {
                LOGI(DKZJ_LOG " god blocked hero hp delta=%lld",
                     static_cast<long long>(hp));
            }
        }
    } else if (hp < 0) {
        const int multiplier = ClampAttackMultiplier(
            g_attackMultiplier.load(std::memory_order_relaxed));
        if (multiplier > 1) {
            adjustedHp = SaturatingScaleNegative(hp, multiplier);
            const uint32_t hit = g_damageHits.fetch_add(1, std::memory_order_relaxed);
            if (hit < kMaxDiagnosticLogs) {
                LOGI(DKZJ_LOG " attack x%d hpdelta %lld -> %lld",
                     multiplier, static_cast<long long>(hp),
                     static_cast<long long>(adjustedHp));
            }
        }
    }
    g_origEntityCharacterChangeHp(self, adjustedHp, method);
}

int ReadShowSkillIds(void* hrs, int32_t* out, int capacity) {
    if (!OkPtr(hrs) || !out || capacity <= 0) return 0;
    void* randomResult = Il2Cpp::ReadPtr(hrs, kOffHrsRandomSkillResult);
    if (!OkPtr(randomResult)) return 0;
    void* list = Il2Cpp::ReadPtr(randomResult, kOffRandomSkillShowSkills);
    if (!OkPtr(list)) return 0;

    const int count = Il2Cpp::ReadI32(list, kOffListSize);
    if (count < 2 || count > capacity) return 0;
    void* items = Il2Cpp::ReadPtr(list, kOffListItems);
    if (!OkPtr(items)) return 0;
    const int arrayLength = Il2Cpp::ReadI32(items, kOffArrayLength);
    if (arrayLength < count || arrayLength > 4096) return 0;

    int written = 0;
    const auto* data = reinterpret_cast<const int32_t*>(
        reinterpret_cast<const uint8_t*>(items) + kOffArrayData);
    for (int i = 0; i < count && written < capacity; i++) {
        const int32_t id = data[i];
        if (id > 0) out[written++] = id;
    }
    return written;
}

bool InvokeAddSkillSurvior(void* hrs, int32_t id) {
    if (!OkPtr(hrs) || id <= 0) return false;
    AddSkillSurviorFn fn =
        g_fnAddSkillSurviorAtomic.load(std::memory_order_acquire);
    if (!fn) return false;
    // There are two AddSkillSurvior(int) / AddSkillSurvior(Skill_Main)
    // overloads with the same argc. Use the dump-confirmed int method pointer
    // instead of resolving by name and risking the wrong overload.
    fn(hrs, id, nullptr);
    return true;
}

void Hooked_OnAddSkill(void* self, int32_t selectedId, void* method) {
    if (!g_origOnAddSkill) return;

    const bool wantSteal =
        g_stealSkill.load(std::memory_order_relaxed) && !t_stealReenter;

    // Match the bingge hook order: let the original selection mutate/refresh
    // RandomSkillResult first, then read ShowSkills for the extra grants.
    g_origOnAddSkill(self, selectedId, method);

    int32_t candidates[kMaxCandidates] = {};
    const int candidateCount =
        wantSteal ? ReadShowSkillIds(self, candidates, kMaxCandidates) : 0;
    if (!wantSteal || candidateCount < 2) return;
    struct StealGuard {
        StealGuard() { t_stealReenter = true; }
        ~StealGuard() { t_stealReenter = false; }
    } stealGuard;
    int granted = 0;
    bool selectedSkipped = false;
    try {
        for (int i = 0; i < candidateCount; i++) {
            const int32_t id = candidates[i];
            if (id <= 0) continue;
            // The selected cast is already handled by the original OnAddSkill.
            // Duplicate candidates with the same id are extra upgrade picks, so
            // consume only one matching occurrence and grant the rest.
            if (!selectedSkipped && id == selectedId) {
                selectedSkipped = true;
                continue;
            }
            if (InvokeAddSkillSurvior(self, id)) granted++;
        }
    } catch (...) {
        LOGW(DKZJ_LOG " steal callback C++ exception; stop grant loop");
    }

    if (granted > 0) {
        g_grantedSkills.fetch_add(static_cast<uint32_t>(granted),
                                  std::memory_order_relaxed);
        const uint32_t hit = g_stealHits.fetch_add(1, std::memory_order_relaxed);
        if (hit < kMaxDiagnosticLogs) {
            LOGI(DKZJ_LOG " steal selected=%d granted=%d", selectedId, granted);
        }
    } else if (g_fnAddSkillSurviorAtomic.load(std::memory_order_relaxed)) {
        LOGW(DKZJ_LOG " steal selected=%d no skills granted", selectedId);
    }
}

bool InvokeActionBool(void* action) {
    if (!OkPtr(action) || !EnsureApi()) return false;
    auto& api = GetApi();
    if (!api.runtime_invoke || !api.object_class) return false;

    void* klass = api.object_class(action);
    if (!klass) return false;
    if (g_actionBoolClass.load(std::memory_order_relaxed) != klass) {
        void* methodInfo = api.GetMethod(klass, "Invoke", 1);
        if (!methodInfo) {
            // Do not cache a failed lookup: an early callback can otherwise make
            // every later ad bypass fall back to the real video flow.
            LOGW(DKZJ_LOG " ad Action<bool>.Invoke resolve miss; retry next time");
            return false;
        }
        g_miActionBoolInvoke.store(methodInfo, std::memory_order_relaxed);
        g_actionBoolClass.store(klass, std::memory_order_release);
    }
    void* methodInfo = g_miActionBoolInvoke.load(std::memory_order_acquire);
    if (!methodInfo) return false;

    uint8_t success = 1;
    void* args[1] = {&success};
    void* exception = nullptr;
    struct AdGuard {
        AdGuard() { t_adReenter = true; }
        ~AdGuard() { t_adReenter = false; }
    } adGuard;
    try {
        api.runtime_invoke(methodInfo, action, args, &exception);
    } catch (...) {
        LOGW(DKZJ_LOG " ad callback C++ exception; fallback original");
        return false;
    }
    if (exception) {
        LOGW(DKZJ_LOG " ad success callback exception; fallback original");
        return false;
    }
    return true;
}

bool Hooked_CheckAndPlayVideo(void* self, void* source, void* onClose, void* method) {
    if (g_noAds.load(std::memory_order_relaxed) && !t_adReenter &&
        InvokeActionBool(onClose)) {
        const uint32_t hit = g_noAdsHits.fetch_add(1, std::memory_order_relaxed);
        if (hit < kMaxDiagnosticLogs) {
            LOGI(DKZJ_LOG " no-ads success callback invoked");
        }
        return true;
    }
    if (g_noAds.load(std::memory_order_relaxed) && !t_adReenter) {
        g_noAdsFallbacks.fetch_add(1, std::memory_order_relaxed);
    }
    return g_origCheckAndPlayVideo ? g_origCheckAndPlayVideo(self, source, onClose, method)
                                   : false;
}

const HookSlot kHookSlots[] = {
    {
        "EntityCharacter.ChangeHp",
        kFileEntityCharacterChangeHp,
        kInsnEntityCharacterChangeHp,
        "HotFix.BattleLogic",
        "EntityCharacter",
        "ChangeHp",
        1,
        reinterpret_cast<void*>(Hooked_EntityCharacter_ChangeHp),
        reinterpret_cast<void**>(&g_origEntityCharacterChangeHp),
    },
    {
        "EntityHero.ChangeHp",
        kFileEntityHeroChangeHp,
        kInsnEntityHeroChangeHp,
        "HotFix.BattleLogic",
        "EntityHero",
        "ChangeHp",
        1,
        reinterpret_cast<void*>(Hooked_EntityHero_ChangeHp),
        reinterpret_cast<void**>(&g_origEntityHeroChangeHp),
    },
    {
        "HeroComponentRandomSkill.OnAddSkill",
        kFileOnAddSkill,
        kInsnOnAddSkill,
        "HotFix.BattleLogic",
        "HeroComponentRandomSkill",
        "OnAddSkill",
        1,
        reinterpret_cast<void*>(Hooked_OnAddSkill),
        reinterpret_cast<void**>(&g_origOnAddSkill),
    },
    {
        "ADModuleMgr.CheckAndPlayVideo",
        kFileCheckAndPlayVideo,
        kInsnCheckAndPlayVideo,
        "HotFix",
        "ADModuleMgr",
        "CheckAndPlayVideo",
        2,
        reinterpret_cast<void*>(Hooked_CheckAndPlayVideo),
        reinterpret_cast<void**>(&g_origCheckAndPlayVideo),
    },
};

void SyncHooks() {
    if (!g_libReady.load(std::memory_order_relaxed)) return;
    std::lock_guard<std::mutex> lock(g_hookMutex);
    for (const auto& slot : kHookSlots) {
        InstallSlot(slot);
    }
    ResolveSkillAux();
}

}  // namespace

bool Dkzj_TryInit(JavaVM* /*vm*/) {
    if (!isLibraryLoaded(DKZJ_TARGET_LIB)) return false;
    g_libReady.store(true, std::memory_order_relaxed);
    SyncHooks();
    LOGI(DKZJ_LOG " lib ready; hooks file-offset + by-name fallback");
    return true;
}

bool Dkzj_IsReady() {
    return g_libReady.load(std::memory_order_relaxed);
}

void Dkzj_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case DKZJ_FEAT_ATTACK_MULT:
            if (!enabled) g_attackMultiplier.store(1, std::memory_order_relaxed);
            break;
        case DKZJ_FEAT_GOD:
            g_godMode.store(enabled, std::memory_order_relaxed);
            break;
        case DKZJ_FEAT_STEAL_SKILL:
            g_stealSkill.store(enabled, std::memory_order_relaxed);
            break;
        case DKZJ_FEAT_NO_ADS:
            g_noAds.store(enabled, std::memory_order_relaxed);
            break;
        default:
            break;
    }
    SyncHooks();
}

void Dkzj_OnValueChanged(int featNum, int value) {
    if (featNum == DKZJ_FEAT_ATTACK_MULT) {
        g_attackMultiplier.store(ClampAttackMultiplier(value),
                                 std::memory_order_relaxed);
    }
}

void Dkzj_Tick(const bool* features, const int* values) {
    if (features) {
        g_godMode.store(features[DKZJ_FEAT_GOD], std::memory_order_relaxed);
        g_stealSkill.store(features[DKZJ_FEAT_STEAL_SKILL],
                           std::memory_order_relaxed);
        g_noAds.store(features[DKZJ_FEAT_NO_ADS], std::memory_order_relaxed);
    }
    if (values) {
        g_attackMultiplier.store(ClampAttackMultiplier(values[DKZJ_FEAT_ATTACK_MULT]),
                                 std::memory_order_relaxed);
    }
    if (!g_libReady.load(std::memory_order_relaxed) &&
        isLibraryLoaded(DKZJ_TARGET_LIB)) {
        g_libReady.store(true, std::memory_order_relaxed);
    }
    SyncHooks();
}

void Dkzj_ForceClear() {
    g_attackMultiplier.store(1, std::memory_order_relaxed);
    g_godMode.store(false, std::memory_order_relaxed);
    g_stealSkill.store(false, std::memory_order_relaxed);
    g_noAds.store(false, std::memory_order_relaxed);
    ResetHeroDepth();
}
