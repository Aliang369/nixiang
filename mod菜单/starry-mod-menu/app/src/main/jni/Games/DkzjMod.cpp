/* ================================================================
 *  蛋壳战机 1.1.6 (vc27) — 星光 Mod
 *
 *  固定 file offset 只适用于 1.1.6。RVA = file offset + 0x4000。
 *
 *  【1.1.6 偷盗技能重做 — 根因】
 *  旧实现只调 HeroComponentRandomSkill.AddSkillSurvior(int)，反汇编证明它
 *  等价于 `_allSkills.Add(id); OnAddSkill(id);` —— 只是把技能 id 登记进
 *  英雄技能表，所以 UI 会显示、但既没有行为组件也没有升级逻辑，即「纯 UI
 *  修改 / 不升级 / 无效果」。
 *
 *  真正的完整授予链（1.1.6 反汇编确认）：
 *    SelectSlotCommand.Exec
 *      -> BattleWorldContext.ApplySelectedSkillsToPlayer
 *        -> BattleWorldContext.AddSkillToCharacter(entity, skillId, addToComponent)
 *          -> AddSkillToCharacterInSTG   [checkIfReplaced 处理重复/升级]
 *            -> PostProcessLearnedSkill [GetSkill_Main + IsDankeSkill
 *                 + AddSkill/AddSkillSurvior(int 与 Skill_Main 两个重载)
 *                 + AddSkillFlags/AddSkillRejectFlags/AddNeedSkill/AddRejectSkills]
 *            -> DispatchAddSkill + entity.OnSkillLearned
 *
 *  因此改为 Hook PostProcessLearnedSkill（能同时拿到 entity 与 heroSkill），
 *  在原始逻辑跑完后，对同一份 RandomSkillResult.ShowSkills 里未被选中的候选
 *  逐个回灌 BattleWorldContext.AddSkillToCharacter，让游戏自己的完整管线
 *  （含 checkIfReplaced 升级分支）处理，而不是只做登记。
 *
 *  防闪退约束：
 *    - 每处 Hook 都先走序言校验；失败转按名，仍失败则完全不动该点
 *    - Hook 只在 orig 非空后进入；原函数始终在主逻辑中保留
 *    - 战斗热路径只做整数算术，不扫堆、不遍历对象、不做字符串解析
 *    - 技能补授最多处理 16 个候选，带 thread_local 深度保护 + 同一次选择去重
 *    - 免广告通过 il2cpp_runtime_invoke 调成功回调并检查异常，失败回退原流程
 *
 *  【1.1.6 启动即闪退 — 根因与修复（2026-09-23 真机取证）】
 *  症状：装上后能玩，打一局后闪退，之后每次启动 0.2~0.7s 就静默死亡。
 *        无 tombstone、无 crash buffer、无 am_crash，只有 exit-info 的
 *        `reason=2 (SIGNALED) status=11 (SIGSEGV)`。
 *  取证：应用日志每次都停在
 *        `[dkzj] AddSkillToCharacter resolved @...`
 *        `[Il2Cpp] api loaded from libil2cpp.so (xdl+dynsym)`
 *        然后 10~30ms 内整进程消失。
 *  根因：上一版 ResolveSkillAux() 在启动期无条件调 ResolveMethodPtr()
 *        → FindClassNs() → il2cpp_domain_get_assemblies()，而
 *        (a) il2cpp_init 由 libunity 在 nativeRender 里才调用，此时还没跑完；
 *        (b) MOD 后台线程是裸 std::thread，没有 attach 到 il2cpp。
 *        该 API 内部会做托管数组分配（Array::New），踩未就绪的运行时状态
 *        → 静默 SIGSEGV。1.1.5 从不做启动期 il2cpp 调用，所以不崩。
 *  修复：启动期只允许 /proc/self/maps 换算（FileOffToVa）。
 *        - ResolveSkillAux() 只解析函数地址，不解析 MethodInfo；
 *        - MethodInfo 改为惰性：EnsureAddSkillMethodInfo()，只在
 *          Hooked_PostProcessLearnedSkill 里调（游戏线程、il2cpp 必已就绪），
 *          解析不到就传 nullptr（AddSkillToCharacter 非泛型，不用 method 参数）；
 *        - 所有 il2cpp API 调用统一走 AttachCurrentThreadToIl2Cpp()；
 *        - by-name 回退（HookResolvedMethod）加 g_il2cppRuntimeUp 门控，
 *          只在 hook 已被游戏线程调到、或首次 Tick(≥15s) 之后才允许。
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
constexpr int kMaxDiagnosticLogs = 8;

// 蛋壳战机 1.1.6 (vc27): libil2cpp file offsets. RVA = file offset + 0x4000.
constexpr uintptr_t kFileEntityCharacterChangeHp = 0x6BB9D44;
constexpr uintptr_t kFileEntityHeroChangeHp = 0x6CAF970;
constexpr uintptr_t kFileCheckAndPlayVideo = 0x776EDBC;
constexpr uintptr_t kFilePostProcessLearnedSkill = 0x6E72E40;
// BattleWorldContext.AddSkillToCharacter(EntityCharacter, int, bool) — 只读调用，
// 不 Hook，仅作为「完整授予管线」的入口。
constexpr uintptr_t kFileAddSkillToCharacter = 0x6E63534;

constexpr uint32_t kInsnEntityCharacterChangeHp = 0xA9BB6FFE;
constexpr uint32_t kInsnEntityHeroChangeHp = 0xF81D0FFE;
constexpr uint32_t kInsnPostProcessLearnedSkill = 0xD10303FF;
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
std::atomic<uint32_t> g_stealNoSelection{0};

thread_local int t_heroDepth = 0;
thread_local int t_stealDepth = 0;
thread_local bool t_adReenter = false;

// 同一次选择只补授一次：ApplySelectedSkillsToPlayer 可能对多个已选技能
// 各调一次 AddSkillToCharacter，因此去重键只含「选择对象 + 候选集合」。
std::atomic<uint32_t> g_lastStealSig{0};
std::atomic<int64_t> g_lastStealMs{0};

// il2cpp 运行时是否已经真正跑起来（托管代码已在执行）。
// 1.1.6 崩溃根因：后台线程在 il2cpp_init 完成前调用 il2cpp_domain_get_assemblies，
// 内部 Array::New 踩未就绪的 il2cpp_defaults → 静默 SIGSEGV（无 tombstone）。
// 置位点只有两个：我们的 hook 被游戏线程调到（托管代码必然在跑），或首次 Tick（≥15s）。
std::atomic<bool> g_il2cppRuntimeUp{false};

typedef void (*ChangeHpFn)(void* self, int64_t hp, void* method);
typedef void (*PostProcessLearnedSkillFn)(void* self, void* entity, int32_t skillId,
                                          void* skillData, bool addToComponent,
                                          void* heroSkill, void* method);
typedef bool (*AddSkillToCharacterFn)(void* self, void* entity, int32_t skillId,
                                      bool addToComponent, void* method);
typedef bool (*CheckAndPlayVideoFn)(void* self, void* source, void* onClose, void* method);

ChangeHpFn g_origEntityCharacterChangeHp = nullptr;
ChangeHpFn g_origEntityHeroChangeHp = nullptr;
PostProcessLearnedSkillFn g_origPostProcessLearnedSkill = nullptr;
CheckAndPlayVideoFn g_origCheckAndPlayVideo = nullptr;
std::atomic<AddSkillToCharacterFn> g_fnAddSkillToCharacterAtomic{nullptr};
std::atomic<void*> g_miAddSkillToCharacter{nullptr};

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

// 调用任何 il2cpp 运行时 API 之前的统一入口：确认库已加载、域已存在，并把
// 当前线程 attach 到 il2cpp。裸 std::thread（MOD 后台线程）未 attach 时，
// il2cpp 的托管分配/线程状态查询会踩空指针，表现为整进程静默 SIGSEGV。
// il2cpp_domain_get 只是读全局 s_domain，未初始化时返回 null，本身安全。
bool AttachCurrentThreadToIl2Cpp() {
    if (!EnsureApi()) return false;
    auto& api = GetApi();
    void* domain = api.Domain();
    if (!domain) return false;  // il2cpp_init 尚未建域
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
    // by-name 解析要枚举 il2cpp 程序集（内部有托管分配）。后台线程若跑在
    // il2cpp_init 完成之前，这一句就是 1.1.6 的静默崩溃点，必须先确认运行时已起来。
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
    // 只做 /proc/self/maps 换算，绝不碰 il2cpp 运行时 API。
    // 1.1.6 的崩溃就是在这里多调了一次 ResolveMethodPtr（枚举程序集 → 静默 SIGSEGV）。
    if (!g_fnAddSkillToCharacterAtomic.load(std::memory_order_relaxed)) {
        AddSkillToCharacterFn fn = reinterpret_cast<AddSkillToCharacterFn>(
            FileOffToVa(DKZJ_TARGET_LIB, kFileAddSkillToCharacter));
        if (fn) {
            g_fnAddSkillToCharacterAtomic.store(fn, std::memory_order_release);
            LOGI(DKZJ_LOG " AddSkillToCharacter resolved @%p",
                 reinterpret_cast<void*>(fn));
        }
    }
}

// MethodInfo 惰性解析：只在 Hooked_PostProcessLearnedSkill 里调用（游戏线程，
// il2cpp 必然已初始化且当前线程已 attach），失败就传 nullptr —— AddSkillToCharacter
// 是非泛型实例方法，生成的代码不使用 method 参数。
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
        const int32_t id = data[i];
        if (id > 0) out[written++] = id;
    }
    return written;
}

int64_t NowMs() {
    return std::chrono::duration_cast<std::chrono::milliseconds>(
               std::chrono::steady_clock::now().time_since_epoch())
        .count();
}

// 同一次选择只补授一次。去重键 = 选择对象指针 + 候选 id 集合的 FNV 哈希，
// 3 秒内同签名视为同一次选择的后续授予（ApplySelectedSkillsToPlayer 会对
// 多个已选技能各调一次 AddSkillToCharacter）。
bool ShouldInjectOnce(void* randomResult, const int32_t* ids, int count) {
    uint32_t sig = 2166136261u;
    sig = (sig ^ static_cast<uint32_t>(
                     reinterpret_cast<uintptr_t>(randomResult))) *
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

// 1.1.6 真正的技能生效漏斗：AddSkillToCharacter -> AddSkillToCharacterInSTG
// -> PostProcessLearnedSkill。原版 AddSkillSurvior(int) 只做
// `_allSkills.Add(id); OnAddSkill(id);`，不实例化行为组件、不走升级判定，
// 这正是「纯 UI 修改 / 不能升级 / 技能无效果」的根因。
// 这里改为：放行原版授予，然后对本次三选一的其它候选逐个回灌
// AddSkillToCharacter，让它们走完整链路（含 checkIfReplaced 升级判定）。
void Hooked_PostProcessLearnedSkill(void* self, void* entity, int32_t skillId,
                                    void* skillData, bool addToComponent,
                                    void* heroSkill, void* method) {
    // 托管代码正在执行 ⇒ il2cpp 运行时已就绪，此后允许走 il2cpp API。
    g_il2cppRuntimeUp.store(true, std::memory_order_release);
    if (!g_origPostProcessLearnedSkill) return;

    struct StealGuard {
        StealGuard() { ++t_stealDepth; }
        ~StealGuard() {
            if (t_stealDepth > 0) --t_stealDepth;
        }
    } stealGuard;

    g_origPostProcessLearnedSkill(self, entity, skillId, skillData, addToComponent,
                                  heroSkill, method);

    if (!g_stealSkill.load(std::memory_order_relaxed)) return;
    // 只有最外层授予才补授；补授自身触发的嵌套授予直接放行。
    if (t_stealDepth != 1) return;
    if (!OkPtr(self) || !OkPtr(entity) || !OkPtr(heroSkill) || skillId <= 0) return;

    AddSkillToCharacterFn fn =
        g_fnAddSkillToCharacterAtomic.load(std::memory_order_acquire);
    if (!fn) return;

    int32_t candidates[kMaxCandidates] = {};
    const int candidateCount =
        ReadShowSkillIds(heroSkill, candidates, kMaxCandidates);
    if (candidateCount < 2) {
        const uint32_t miss = g_stealNoSelection.fetch_add(1, std::memory_order_relaxed);
        if (miss < kMaxDiagnosticLogs) {
            LOGW(DKZJ_LOG " steal skill=%d hero=%p but ShowSkills count=%d",
                 skillId, heroSkill, candidateCount);
        }
        return;
    }

    // 只在本次候选集合确实包含被选技能时才补授，避免 InitSkills / 读档等
    // 无关授予被误判成一次三选一。
    bool picked = false;
    for (int i = 0; i < candidateCount; i++) {
        if (candidates[i] == skillId) {
            picked = true;
            break;
        }
    }
    if (!picked) return;

    void* randomResult = Il2Cpp::ReadPtr(heroSkill, kOffHrsRandomSkillResult);
    if (!ShouldInjectOnce(randomResult, candidates, candidateCount)) return;

    void* methodInfo = EnsureAddSkillMethodInfo();
    int granted = 0;
    for (int i = 0; i < candidateCount; i++) {
        const int32_t id = candidates[i];
        // 被选技能已由原版链路授予，这里只补其余候选（含重复 id 的升级项）。
        if (id <= 0 || id == skillId) continue;
        if (fn(self, entity, id, true, methodInfo)) granted++;
    }

    if (granted > 0) {
        g_grantedSkills.fetch_add(static_cast<uint32_t>(granted),
                                  std::memory_order_relaxed);
        const uint32_t hit = g_stealHits.fetch_add(1, std::memory_order_relaxed);
        if (hit < kMaxDiagnosticLogs) {
            LOGI(DKZJ_LOG " steal picked=%d granted=%d (of %d candidates)",
                 skillId, granted, candidateCount);
        }
    } else {
        LOGW(DKZJ_LOG " steal picked=%d no skills granted", skillId);
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
    // Tick 间隔 15s，走到这里 Unity/il2cpp 必然已经在跑；据此放开 by-name 回退。
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
