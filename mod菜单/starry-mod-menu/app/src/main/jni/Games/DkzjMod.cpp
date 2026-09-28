/* ================================================================
 *  蛋壳战机 1.1.7 (vc32) — 星光 Mod
 *
 *  固定 file offset 只适用于 1.1.7。RVA = file offset + 0x4000。
 *
 *  授予链：
 *    SelectSlotCommand.Exec
 *      -> BattleWorldContext.AddSkillToCharacter(entity, id, true)
 *        -> PostProcessLearnedSkill
 *
 *  原版升级池会跳过已拥有的 UpgradeSkillId。补授其余候选时读取
 *  HeroComponentRandomSkill._allSkills 与 Skill_Main.UpgradeSkillId[]，
 *  把已拥有的同名候选显式改授为下一个未拥有的升级 id，再走完整
 *  AddSkillToCharacter 链，确保技能行为和升级状态同时生效。
 *
 *  防闪退约束：
 *    - 启动期只做 /proc/self/maps 换算，不枚举程序集、不调 MethodInfo
 *    - LocalModelManager 等运行时对象全部在 Hook 内惰性解析
 *    - Hook 先序言校验，失败走 by-name，仍失败则不动该点
 *    - 授予路径仅做小数组读取与直接函数调用，不做堆扫描
 * ================================================================ */

#include "DkzjMod.h"

#include <atomic>
#include <chrono>
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
constexpr int kMaxUpgradeIds = 16;
constexpr int kMaxOwnedSkills = 256;
constexpr int kMaxDiagnosticLogs = 8;

// 蛋壳战机 1.1.7 (vc32): libil2cpp file offsets. RVA = file offset + 0x4000.
constexpr uintptr_t kFileEntityCharacterChangeHp = 0x6C31EC0;
constexpr uintptr_t kFileEntityHeroChangeHp = 0x6D1C3E4;
constexpr uintptr_t kFileCheckAndPlayVideo = 0x77E2EF8;
constexpr uintptr_t kFilePostProcessLearnedSkill = 0x6EDE058;
constexpr uintptr_t kFileAddSkillToCharacter = 0x6ECE6AC;
constexpr uintptr_t kFileGetSkillMain = 0x68D88A0;
constexpr uintptr_t kFileGameGetTable = 0x75B5390;

constexpr uint32_t kInsnEntityCharacterChangeHp = 0xA9BB6FFE;
constexpr uint32_t kInsnEntityHeroChangeHp = 0xF81D0FFE;
constexpr uint32_t kInsnPostProcessLearnedSkill = 0xD10303FF;
constexpr uint32_t kInsnCheckAndPlayVideo = 0xA9BC67FE;

// HeroComponentRandomSkill layout (1.1.7 dump.cs)
constexpr size_t kOffHrsAllSkills = 0x58;
constexpr size_t kOffHrsRandomSkillResult = 0x88;
constexpr size_t kOffRandomSkillShowSkills = 0x60;
constexpr size_t kOffListItems = 0x10;
constexpr size_t kOffListSize = 0x18;
constexpr size_t kOffArrayLength = 0x18;
constexpr size_t kOffArrayData = 0x20;
constexpr size_t kOffSkillUpgradeIds = 0x30;

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
std::atomic<uint32_t> g_stealNoSelection{0};
std::atomic<uint32_t> g_upgradeRewrites{0};

thread_local int t_heroDepth = 0;
thread_local int t_stealDepth = 0;
thread_local bool t_adReenter = false;

std::atomic<uint32_t> g_lastStealSig{0};
std::atomic<int64_t> g_lastStealMs{0};

std::atomic<bool> g_il2cppRuntimeUp{false};

typedef void (*ChangeHpFn)(void* self, int64_t hp, void* method);
typedef void (*PostProcessLearnedSkillFn)(void* self, void* entity, int32_t skillId,
                                          void* skillData, bool addToComponent,
                                          void* heroSkill, void* method);
typedef bool (*AddSkillToCharacterFn)(void* self, void* entity, int32_t skillId,
                                      bool addToComponent, void* method);
typedef bool (*CheckAndPlayVideoFn)(void* self, void* source, void* onClose, void* method);
typedef void* (*GetSkillMainFn)(void* self, int32_t key, void* method);
typedef void* (*GameGetTableFn)();

ChangeHpFn g_origEntityCharacterChangeHp = nullptr;
ChangeHpFn g_origEntityHeroChangeHp = nullptr;
PostProcessLearnedSkillFn g_origPostProcessLearnedSkill = nullptr;
CheckAndPlayVideoFn g_origCheckAndPlayVideo = nullptr;
std::atomic<AddSkillToCharacterFn> g_fnAddSkillToCharacterAtomic{nullptr};
std::atomic<GetSkillMainFn> g_fnGetSkillMainAtomic{nullptr};
std::atomic<GameGetTableFn> g_fnGameGetTableAtomic{nullptr};
std::atomic<void*> g_miAddSkillToCharacter{nullptr};
std::atomic<void*> g_miGetSkillMain{nullptr};

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

bool AttachCurrentThreadToIl2Cpp() {
    if (!EnsureApi()) return false;
    auto& api = GetApi();
    void* domain = api.Domain();
    if (!domain) return false;
    if (api.thread_attach) api.thread_attach(domain);
    return true;
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
    if (DobbyHook(target, hook, orig) == 0) {
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
    if (!g_il2cppRuntimeUp.load(std::memory_order_acquire)) {
        LOGW(DKZJ_LOG " %s by-name deferred (il2cpp runtime not up)", tag);
        return false;
    }
    if (!AttachCurrentThreadToIl2Cpp()) {
        LOGW(DKZJ_LOG " %s by-name deferred (il2cpp domain not ready)", tag);
        return false;
    }
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
    if (!g_fnAddSkillToCharacterAtomic.load(std::memory_order_relaxed)) {
        AddSkillToCharacterFn fn = reinterpret_cast<AddSkillToCharacterFn>(
            FileOffToVa(DKZJ_TARGET_LIB, kFileAddSkillToCharacter));
        if (fn) {
            g_fnAddSkillToCharacterAtomic.store(fn, std::memory_order_release);
            LOGI(DKZJ_LOG " AddSkillToCharacter resolved @%p", reinterpret_cast<void*>(fn));
        }
    }
    if (!g_fnGetSkillMainAtomic.load(std::memory_order_relaxed)) {
        GetSkillMainFn fn = reinterpret_cast<GetSkillMainFn>(
            FileOffToVa(DKZJ_TARGET_LIB, kFileGetSkillMain));
        if (fn) {
            g_fnGetSkillMainAtomic.store(fn, std::memory_order_release);
            LOGI(DKZJ_LOG " GetSkill_Main resolved @%p", reinterpret_cast<void*>(fn));
        }
    }
    if (!g_fnGameGetTableAtomic.load(std::memory_order_relaxed)) {
        GameGetTableFn fn = reinterpret_cast<GameGetTableFn>(
            FileOffToVa(DKZJ_TARGET_LIB, kFileGameGetTable));
        if (fn) {
            g_fnGameGetTableAtomic.store(fn, std::memory_order_release);
            LOGI(DKZJ_LOG " Game.get_Table resolved @%p", reinterpret_cast<void*>(fn));
        }
    }
}

void* EnsureAddSkillMethodInfo() {
    void* cached = g_miAddSkillToCharacter.load(std::memory_order_acquire);
    if (cached) return cached;
    if (!AttachCurrentThreadToIl2Cpp()) return nullptr;
    void* methodInfo = nullptr;
    ResolveMethodPtr("HotFix.BattleLogic", "BattleWorldContext",
                     "AddSkillToCharacter", 3, &methodInfo);
    if (methodInfo) {
        g_miAddSkillToCharacter.store(methodInfo, std::memory_order_release);
    }
    return methodInfo;
}

void* EnsureGetSkillMainMethodInfo() {
    void* cached = g_miGetSkillMain.load(std::memory_order_acquire);
    if (cached) return cached;
    if (!AttachCurrentThreadToIl2Cpp()) return nullptr;
    void* methodInfo = nullptr;
    ResolveMethodPtr("", "LocalModelManager", "GetSkill_Main", 1, &methodInfo);
    if (methodInfo) {
        g_miGetSkillMain.store(methodInfo, std::memory_order_release);
    }
    return methodInfo;
}

void ResetHeroDepth() {
    t_heroDepth = 0;
}

void Hooked_EntityHero_ChangeHp(void* self, int64_t hp, void* method) {
    g_il2cppRuntimeUp.store(true, std::memory_order_release);
    if (!g_origEntityHeroChangeHp) return;
    struct DepthGuard {
        DepthGuard() { ++t_heroDepth; }
        ~DepthGuard() { if (t_heroDepth > 0) --t_heroDepth; }
    } guard;
    g_origEntityHeroChangeHp(self, hp, method);
}

void Hooked_EntityCharacter_ChangeHp(void* self, int64_t hp, void* method) {
    g_il2cppRuntimeUp.store(true, std::memory_order_release);
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
        if (data[i] > 0) out[written++] = data[i];
    }
    return written;
}

int64_t NowMs() {
    return std::chrono::duration_cast<std::chrono::milliseconds>(
               std::chrono::steady_clock::now().time_since_epoch())
        .count();
}

bool ShouldInjectOnce(void* keyObject, const int32_t* ids, int count) {
    uint32_t sig = 2166136261u;
    sig = (sig ^ static_cast<uint32_t>(
                     reinterpret_cast<uintptr_t>(keyObject))) *
          16777619u;
    for (int i = 0; i < count; i++) {
        sig = (sig ^ static_cast<uint32_t>(ids[i])) * 16777619u;
    }
    const int64_t now = NowMs();
    const uint32_t lastSig = g_lastStealSig.load(std::memory_order_relaxed);
    const int64_t lastMs = g_lastStealMs.load(std::memory_order_relaxed);
    if (sig == lastSig && lastMs > 0 && now - lastMs < 3000) return false;
    g_lastStealSig.store(sig, std::memory_order_relaxed);
    g_lastStealMs.store(now, std::memory_order_relaxed);
    return true;
}

void* ReadOwnedSkills(void* hrs, int32_t* out, int capacity) {
    if (!OkPtr(hrs) || !out || capacity <= 0) return nullptr;
    void* list = Il2Cpp::ReadPtr(hrs, kOffHrsAllSkills);
    if (!OkPtr(list)) return nullptr;
    const int count = Il2Cpp::ReadI32(list, kOffListSize);
    if (count < 0 || count > 4096) return nullptr;
    void* items = Il2Cpp::ReadPtr(list, kOffListItems);
    if (!OkPtr(items)) return nullptr;
    const int arrayLength = Il2Cpp::ReadI32(items, kOffArrayLength);
    if (arrayLength < count || arrayLength > 4096) return nullptr;

    const auto* data = reinterpret_cast<const int32_t*>(
        reinterpret_cast<const uint8_t*>(items) + kOffArrayData);
    const int readable = count < capacity ? count : capacity;
    for (int i = 0; i < readable; i++) out[i] = data[i];
    return items;
}

bool ContainsSkillId(const int32_t* ids, int count, int32_t id) {
    for (int i = 0; i < count; i++) {
        if (ids[i] == id) return true;
    }
    return false;
}

void* ResolveGameTable() {
    GameGetTableFn fn = g_fnGameGetTableAtomic.load(std::memory_order_acquire);
    if (!fn) return nullptr;
    return fn();
}

void* ResolveSkillMain(int32_t skillId) {
    GetSkillMainFn fn = g_fnGetSkillMainAtomic.load(std::memory_order_acquire);
    if (!fn || skillId <= 0) return nullptr;
    void* table = ResolveGameTable();
    if (!OkPtr(table)) return nullptr;
    void* methodInfo = EnsureGetSkillMainMethodInfo();
    void* skill = fn(table, skillId, methodInfo);
    return OkPtr(skill) ? skill : nullptr;
}

int ReadUpgradeIds(void* skillMain, int32_t* out, int capacity) {
    if (!OkPtr(skillMain) || !out || capacity <= 0) return 0;
    void* array = Il2Cpp::ReadPtr(skillMain, kOffSkillUpgradeIds);
    if (!OkPtr(array)) return 0;
    const int count = Il2Cpp::ReadI32(array, kOffArrayLength);
    if (count < 0 || count > kMaxUpgradeIds * 8) return 0;

    const int readable = count < capacity ? count : capacity;
    const auto* data = reinterpret_cast<const int32_t*>(
        reinterpret_cast<const uint8_t*>(array) + kOffArrayData);
    for (int i = 0; i < readable; i++) out[i] = data[i];
    return readable;
}

// 原版升级池会跳过已拥有升级 id。这里显式把已拥有候选映射到第一个
// 未拥有的下一级；没有下一级时返回原 id，让完整授予链自行拒绝。
int32_t ResolveGrantId(void* hrs, void* skillMain, int32_t requestedId,
                       const int32_t* owned, int ownedCount) {
    int32_t upgrades[kMaxUpgradeIds] = {};
    const int upgradeCount = ReadUpgradeIds(skillMain, upgrades, kMaxUpgradeIds);
    if (upgradeCount <= 0 || !ContainsSkillId(owned, ownedCount, requestedId)) {
        return requestedId;
    }
    for (int i = 0; i < upgradeCount; i++) {
        if (upgrades[i] > 0 && !ContainsSkillId(owned, ownedCount, upgrades[i])) {
            g_upgradeRewrites.fetch_add(1, std::memory_order_relaxed);
            if (g_upgradeRewrites.load(std::memory_order_relaxed) <= kMaxDiagnosticLogs) {
                LOGI(DKZJ_LOG " upgrade requested=%d -> granted=%d",
                     requestedId, upgrades[i]);
            }
            return upgrades[i];
        }
    }
    return requestedId;
}

bool GrantOne(void* self, void* entity, void* hrs, int32_t requestedId,
              const int32_t* owned, int ownedCount, void* methodInfo,
              int32_t* grantedIdOut) {
    void* skillMain = ResolveSkillMain(requestedId);
    const int32_t grantId = skillMain
        ? ResolveGrantId(hrs, skillMain, requestedId, owned, ownedCount)
        : requestedId;
    AddSkillToCharacterFn fn =
        g_fnAddSkillToCharacterAtomic.load(std::memory_order_acquire);
    if (!fn || grantId <= 0) return false;
    const bool granted = fn(self, entity, grantId, true, methodInfo);
    if (granted && grantedIdOut) *grantedIdOut = grantId;
    return granted;
}

void Hooked_PostProcessLearnedSkill(void* self, void* entity, int32_t skillId,
                                    void* skillData, bool addToComponent,
                                    void* heroSkill, void* method) {
    g_il2cppRuntimeUp.store(true, std::memory_order_release);
    if (!g_origPostProcessLearnedSkill) return;

    struct GrantGuard {
        GrantGuard() { ++t_stealDepth; }
        ~GrantGuard() { if (t_stealDepth > 0) --t_stealDepth; }
    } grantGuard;

    g_origPostProcessLearnedSkill(self, entity, skillId, skillData, addToComponent,
                                  heroSkill, method);

    if (!OkPtr(self) || !OkPtr(entity) || !OkPtr(heroSkill) || skillId <= 0) return;
    if (t_stealDepth != 1) return;
    if (!g_stealSkill.load(std::memory_order_relaxed)) return;

    AddSkillToCharacterFn fn =
        g_fnAddSkillToCharacterAtomic.load(std::memory_order_acquire);
    if (!fn) return;

    int32_t candidates[kMaxCandidates] = {};
    const int candidateCount =
        ReadShowSkillIds(heroSkill, candidates, kMaxCandidates);
    if (candidateCount < 2 ||
        !ContainsSkillId(candidates, candidateCount, skillId)) {
        if (candidateCount < 2) {
            const uint32_t miss =
                g_stealNoSelection.fetch_add(1, std::memory_order_relaxed);
            if (miss < kMaxDiagnosticLogs) {
                LOGW(DKZJ_LOG " steal skill=%d hero=%p but ShowSkills count=%d",
                     skillId, heroSkill, candidateCount);
            }
        }
        return;
    }

    void* keyObject = Il2Cpp::ReadPtr(heroSkill, kOffHrsRandomSkillResult);
    if (!ShouldInjectOnce(keyObject ? keyObject : heroSkill, candidates,
                          candidateCount)) {
        return;
    }

    int32_t owned[kMaxOwnedSkills] = {};
    void* ownedItems = ReadOwnedSkills(heroSkill, owned, kMaxOwnedSkills);
    if (!OkPtr(ownedItems)) return;
    int ownedCount = Il2Cpp::ReadI32(ownedItems, kOffArrayLength);
    if (ownedCount < 0 || ownedCount > kMaxOwnedSkills) return;

    void* methodInfo = EnsureAddSkillMethodInfo();
    int granted = 0;
    int32_t grantedIds[kMaxCandidates] = {};
    for (int i = 0; i < candidateCount; i++) {
        const int32_t requestedId = candidates[i];
        if (requestedId <= 0 || requestedId == skillId) continue;
        if (i > 0 && ContainsSkillId(candidates, i, requestedId)) continue;

        int32_t grantedId = 0;
        if (!GrantOne(self, entity, heroSkill, requestedId, owned, ownedCount,
                      methodInfo, &grantedId)) {
            continue;
        }
        grantedIds[granted] = grantedId;
        granted++;
        if (grantedId > 0 && !ContainsSkillId(owned, ownedCount, grantedId) &&
            ownedCount < kMaxOwnedSkills) {
            owned[ownedCount++] = grantedId;
        }
    }

    if (granted > 0) {
        g_grantedSkills.fetch_add(static_cast<uint32_t>(granted),
                                  std::memory_order_relaxed);
        const uint32_t hit = g_stealHits.fetch_add(1, std::memory_order_relaxed);
        if (hit < kMaxDiagnosticLogs) {
            LOGI(DKZJ_LOG " steal picked=%d granted=%d first=%d last=%d",
                 skillId, granted, grantedIds[0], grantedIds[granted - 1]);
        }
    } else {
        const uint32_t miss = g_stealNoSelection.fetch_add(1, std::memory_order_relaxed);
        if (miss < kMaxDiagnosticLogs) {
            LOGW(DKZJ_LOG " steal picked=%d candidates=%d no extra skills granted",
                 skillId, candidateCount);
        }
    }
}

bool InvokeActionBool(void* action) {
    if (!OkPtr(action) || !AttachCurrentThreadToIl2Cpp()) return false;
    auto& api = GetApi();
    if (!api.runtime_invoke || !api.object_class) return false;

    void* klass = api.object_class(action);
    if (!klass) return false;
    if (g_actionBoolClass.load(std::memory_order_relaxed) != klass) {
        void* methodInfo = api.GetMethod(klass, "Invoke", 1);
        if (!methodInfo) {
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
    g_il2cppRuntimeUp.store(true, std::memory_order_release);
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
        "BattleWorldContext.PostProcessLearnedSkill",
        kFilePostProcessLearnedSkill,
        kInsnPostProcessLearnedSkill,
        "HotFix.BattleLogic",
        "BattleWorldContext",
        "PostProcessLearnedSkill",
        5,
        reinterpret_cast<void*>(Hooked_PostProcessLearnedSkill),
        reinterpret_cast<void**>(&g_origPostProcessLearnedSkill),
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
    g_il2cppRuntimeUp.store(true, std::memory_order_release);
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
    t_stealDepth = 0;
    g_lastStealSig.store(0, std::memory_order_relaxed);
    g_lastStealMs.store(0, std::memory_order_relaxed);
}
