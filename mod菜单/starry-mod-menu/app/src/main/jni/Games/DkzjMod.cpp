/* ================================================================
 *  蛋壳战机 1.1.8 / 私服 1.2.1 (vc35) — 星光 Mod
 *  私服 1.2.1 通过 Android_dkzjsf.mk 的 DKZJ_TARGET_1_2_1 选择新锚点；
 *  官方/旧私服 1.1.8 仍走原锚点（同一份源码，两个版本表）。
 *
 *  固定 file offset 只适用于 1.1.8。RVA = file offset + 0x4000。
 *
 *  【2026-10-14 用户 m00790】「立即胜利」功能已**整体删除**：
 *    卡密包的同名功能被其作者停用（三个 nativeTriggerWin 都是 0x2C 日志桩），
 *    星光版自建的波次驱动 + SetWinPlayerId 路线也一并撤掉，不留死代码。
 *    当前菜单 = 攻击倍率 / 上帝模式 / 偷盗技能 / 免广告 四项，
 *    外加**常开注入**的拦截上报（BattleRecordManager.IsNeedUploadSaveData）。
 *
 *  锚点按版本分组：1.1.8 与私服 1.2.1 都是 Il2CppDumper dump.cs 实测，delta 0x4000；
 *  1.2.1 的 12 个 hook 序言与 1.1.8 相同，但 file offset 全部平移。
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
constexpr int kMaxDiagnosticLogs = 32;
// 每候选诊断日志单独放宽（升级链排查需要看到每一次选择的每个候选，32 条会被前几局吃光）
constexpr uint32_t kMaxCandLogs = 200;

// 蛋壳战机 libil2cpp file offsets. RVA = file offset + 0x4000.
// 1.2.1 与 1.1.8 的 code layout 不同；由 Android_dkzjsf.mk 选择 1.2.1 表。
#if defined(DKZJ_TARGET_1_2_1)
// 私服 1.2.1 (vc35): libil2cpp sha256 ff308eea… / metadata def5f032…
constexpr uintptr_t kFileEntityCharacterChangeHp = 0x6D1B118;   // RVA 0x6D1F118
constexpr uintptr_t kFileEntityHeroChangeHp = 0x6E13D30;        // RVA 0x6E17D30
constexpr uintptr_t kFileCheckAndPlayVideo = 0x78E1148;         // RVA 0x78E5148
constexpr uintptr_t kFilePostProcessLearnedSkill = 0x6FD7A08;   // RVA 0x6FDBA08
constexpr uintptr_t kFileAddSkillToCharacter = 0x6FC7BDC;       // RVA 0x6FCBBDC
constexpr uintptr_t kFileGetSkillMain = 0x69C231C;              // RVA 0x69C631C
constexpr uintptr_t kFileGameGetTable = 0x76F9B58;              // RVA 0x76FDB58
constexpr uintptr_t kFileSetShowSkillsRandom = 0x6DF7784;      // RVA 0x6DFB784
constexpr uintptr_t kFileSetShowSkillsAngle = 0x7A51BD8;       // RVA 0x7A55BD8
constexpr uintptr_t kFileSetShowSkillsContract = 0x7B3168C;    // RVA 0x7B3568C
constexpr uintptr_t kFileSetShowSkillsPunchboard = 0x7FB7B1C;  // RVA 0x7FBBB1C
constexpr uintptr_t kFileIsNeedUploadSaveData = 0x7623018;     // RVA 0x7627018
constexpr uintptr_t kFileHurtBaseFinalizeAttackResult = 0x6E2E404;  // RVA 0x6E32404
constexpr uintptr_t kFileEntityHeroSetCurrentHp = 0x6E16390;  // RVA 0x6E1A390
constexpr uintptr_t kFileAdsMgrCheckAndPlayVideo = 0x4457058;  // RVA 0x445B058
constexpr size_t kOffCharCurrentHp = 0x1A0;    // EntityCharacter._currentHp (1.2.1)
#else
// 官方/私服 1.1.8 (vc35): 旧锚点表（保留用于旧包回退）
constexpr uintptr_t kFileEntityCharacterChangeHp = 0x6C2A314;   // RVA 0x6C2E314
constexpr uintptr_t kFileEntityHeroChangeHp = 0x6D20180;        // RVA 0x6D24180
constexpr uintptr_t kFileCheckAndPlayVideo = 0x77E7A98;         // RVA 0x77EBA98
constexpr uintptr_t kFilePostProcessLearnedSkill = 0x6EE4654;   // RVA 0x6EE8654
constexpr uintptr_t kFileAddSkillToCharacter = 0x6ED4CA8;       // RVA 0x6ED8CA8
constexpr uintptr_t kFileGetSkillMain = 0x68DBBD0;              // RVA 0x68DFBD0
constexpr uintptr_t kFileGameGetTable = 0x75B9F30;              // RVA 0x75BDF30
constexpr uintptr_t kFileSetShowSkillsRandom = 0x6D03FE4;       // RVA 0x6D07FE4
constexpr uintptr_t kFileSetShowSkillsAngle = 0x790F48C;        // RVA 0x791348C
constexpr uintptr_t kFileSetShowSkillsContract = 0x79D4C2C;     // RVA 0x79D8C2C
constexpr uintptr_t kFileSetShowSkillsPunchboard = 0x7E19A10;   // RVA 0x7E1DA10
constexpr uintptr_t kFileIsNeedUploadSaveData = 0x74E1900;      // RVA 0x74E5900
constexpr uintptr_t kFileHurtBaseFinalizeAttackResult = 0x6D39C44;  // RVA 0x6D3DC44
constexpr uintptr_t kFileEntityHeroSetCurrentHp = 0x6D227E0;  // RVA 0x6D267E0
constexpr uintptr_t kFileAdsMgrCheckAndPlayVideo = 0x43C9720;  // RVA 0x43CD720
constexpr size_t kOffCharCurrentHp = 0x198;    // EntityCharacter._currentHp (1.1.8)
#endif
constexpr uint32_t kInsnIsNeedUploadSaveData = 0xA9BE57FE;
constexpr uint32_t kInsnHurtBaseFinalizeAttackResult = 0xF81D0FFE;
constexpr size_t kOffHurtEntity = 0x60;        // HurtBase.Entity（被击中方）
constexpr int64_t kFpRawOne = 65536;           // FP.RAW_ONE
constexpr uint32_t kInsnEntityHeroSetCurrentHp = 0xF81D0FFE;
constexpr uint32_t kInsnAdsMgrCheckAndPlayVideo = 0xD10183FF;

constexpr uint32_t kInsnEntityCharacterChangeHp = 0xA9BB6FFE;
constexpr uint32_t kInsnEntityHeroChangeHp = 0xF81D0FFE;
constexpr uint32_t kInsnPostProcessLearnedSkill = 0xD10303FF;
constexpr uint32_t kInsnCheckAndPlayVideo = 0xA9BC67FE;
constexpr uint32_t kInsnSetShowSkills = 0xF81D0FFE;

// HeroComponentRandomSkill layout (1.1.8 dump.cs 实测与 1.1.7 全等)
// _allSkills 0x58 / _randomSkillResult 0x88 / RandomSkillResult.ShowSkills 0x60 /
// List.items 0x10 / List.size 0x18 / 数组 length 0x18 / 数组 data 0x20 /
// Skill_Main.UpgradeSkillId 0x30（LocalModels.Bean，另有 UpgradeSkillId_B 0x150 未用）
constexpr size_t kOffHrsAllSkills = 0x58;
constexpr size_t kOffHrsRandomSkillResult = 0x88;
constexpr size_t kOffRandomSkillShowSkills = 0x60;
constexpr size_t kOffListItems = 0x10;
constexpr size_t kOffListSize = 0x18;
constexpr size_t kOffArrayLength = 0x18;
constexpr size_t kOffArrayData = 0x20;
constexpr size_t kOffSkillUpgradeIds = 0x30;

// Skill_Main (HotFix.BattleLogic) 升级链相关字段（1.1.8 dump.cs:309778）
constexpr size_t kOffSkillIsUpgrade = 0x28;
constexpr size_t kOffSkillStar = 0x3C;
constexpr size_t kOffSkillMaxStar = 0x40;
constexpr size_t kOffSkillNextSkill = 0x84;
constexpr size_t kOffSkillCoverSkill = 0x80;
constexpr size_t kOffSkillIsUpgradeB = 0x14C;
constexpr size_t kOffSkillUpgradeSkillIdB = 0x150;   // 第二条升级链（m00790 补）
constexpr size_t kOffSkillUnlockUpgradeId = 0x158;
constexpr size_t kOffSkillRepeatSkills = 0x168;

// RandomSkillResult (HotFix.BattleLogic) 其余字段：诊断用
constexpr size_t kOffRandomSkillType = 0x28;
constexpr size_t kOffRandomSkillAllSkills = 0x30;
constexpr size_t kOffRandomSkillOwnSkills = 0x58;
constexpr size_t kOffRandomSkillLastSel = 0x70;
constexpr size_t kOffRandomSkillNeedSelect = 0x74;

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
std::atomic<uint32_t> g_candLogs{0};
std::atomic<uint32_t> g_showSkillsSets{0};
std::atomic<uint32_t> g_snapshotHits{0};
std::atomic<uint32_t> g_readPathLogs{0};
std::atomic<uint32_t> g_grantFails{0};
std::atomic<uint32_t> g_dedupeSkips{0};

// 拦截上报状态 + 计数器（常开：不是开关，见 g_origIsNeedUploadSaveData 注释）
std::atomic<bool> g_blockReport{true};
std::atomic<uint32_t> g_reportBlocks{0};

thread_local int t_heroDepth = 0;
thread_local int t_stealDepth = 0;
thread_local int t_hurtDepth = 0;
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
typedef void (*SetShowSkillsFn)(void* self, void* list, void* method);
typedef void* (*GetSkillMainFn)(void* self, int32_t key, void* method);
typedef void* (*GameGetTableFn)();

ChangeHpFn g_origEntityCharacterChangeHp = nullptr;
ChangeHpFn g_origEntityHeroChangeHp = nullptr;
PostProcessLearnedSkillFn g_origPostProcessLearnedSkill = nullptr;
CheckAndPlayVideoFn g_origCheckAndPlayVideo = nullptr;
SetShowSkillsFn g_origSetShowSkillsRandom = nullptr;
SetShowSkillsFn g_origSetShowSkillsAngle = nullptr;
SetShowSkillsFn g_origSetShowSkillsContract = nullptr;
SetShowSkillsFn g_origSetShowSkillsPunchboard = nullptr;
std::atomic<AddSkillToCharacterFn> g_fnAddSkillToCharacterAtomic{nullptr};
std::atomic<GetSkillMainFn> g_fnGetSkillMainAtomic{nullptr};
std::atomic<GameGetTableFn> g_fnGameGetTableAtomic{nullptr};
std::atomic<void*> g_miAddSkillToCharacter{nullptr};
std::atomic<void*> g_miGetSkillMain{nullptr};
std::atomic<void*> g_miGameGetTable{nullptr};

std::atomic<void*> g_actionBoolClass{nullptr};
std::atomic<void*> g_miActionBoolInvoke{nullptr};

// 拦截上报：hook 函数签名（实例私有静态方法 argc 0 ⇒ (MethodInfo*)）
typedef bool (*IsNeedUploadSaveDataFn)(void* method);

// 拦截上报：hook 原始指针。**常开注入**：用户要求不做菜单开关，默认即生效
//（见 m00392 第 2 条），因此这里是 true 且运行期只由 Dkzj_ForceClear/重载时维持。
IsNeedUploadSaveDataFn g_origIsNeedUploadSaveData = nullptr;

// ── 卡密包挂点对齐：新 hook 的原始指针与状态 ──
typedef int64_t (*FinalizeAttackResultFn)(void* self, int64_t rawResult, void* method);
typedef void (*SetCurrentHpFn)(void* self, int64_t value, void* method);
typedef bool (*AdsMgrCheckAndPlayVideoFn)(void* self, void* onClose, void* onShow,
                                          void* onPlayFail, void* getSource,
                                          void* method);

FinalizeAttackResultFn g_origFinalizeAttackResult = nullptr;
SetCurrentHpFn g_origEntityHeroSetCurrentHp = nullptr;
AdsMgrCheckAndPlayVideoFn g_origAdsMgrCheckAndPlayVideo = nullptr;

// 攻击结算挂点是否上线 —— 上线后 EntityCharacter.ChangeHp 不再自己乘倍率（避免平方倍）
std::atomic<bool> g_dmgAtkHookLive{false};
std::atomic<bool> g_adsMgrHookLive{false};
std::atomic<uint32_t> g_dmgAtkHits{0};
std::atomic<uint32_t> g_godSetHits{0};
std::atomic<uint32_t> g_noAdsMgrHits{0};

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

// 通用饱和乘（攻击结算用；FP.RawValue 可正可负，不能只处理负数）
int64_t SaturatingScaleInt64(int64_t value, int multiplier) {
    if (multiplier <= 1 || value == 0) return value;
    const int64_t minValue = std::numeric_limits<int64_t>::min();
    const int64_t maxValue = std::numeric_limits<int64_t>::max();
    if (value > 0) {
        if (value > maxValue / multiplier) return maxValue;
    } else {
        if (value < minValue / multiplier) return minValue;
    }
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
    // 1.2.1(vc44) dump.cs：LocalModels.LocalModelManager.GetSkill_Main(int)
    if (!ResolveMethodPtr("LocalModels", "LocalModelManager", "GetSkill_Main", 1, &methodInfo)
        || !methodInfo) {
        ResolveMethodPtr("", "LocalModelManager", "GetSkill_Main", 1, &methodInfo);
    }
    if (methodInfo) {
        g_miGetSkillMain.store(methodInfo, std::memory_order_release);
    }
    return methodInfo;
}

void* EnsureGameGetTableMethodInfo() {
    void* cached = g_miGameGetTable.load(std::memory_order_acquire);
    if (cached) return cached;
    if (!AttachCurrentThreadToIl2Cpp()) return nullptr;
    void* methodInfo = nullptr;
    // 1.2.1(vc44) dump.cs：HotFix.Game.get_Table()
    if (!ResolveMethodPtr("HotFix", "Game", "get_Table", 0, &methodInfo) || !methodInfo) {
        ResolveMethodPtr("", "Game", "get_Table", 0, &methodInfo);
    }
    if (methodInfo) {
        g_miGameGetTable.store(methodInfo, std::memory_order_release);
    }
    return methodInfo;
}

// ---------------------------------------------------------------------------
// aux 三点的「无 RX 段」兜底（2026-10-28，雷电模拟器实测）：
//   LDPlayer/MuMu 这类 x86_64 + libhoudini 环境里 libil2cpp.so **没有可执行段**
//   （/proc/<pid>/maps 实测 r-xp=0）⇒ FileOffToVa() 恒 nullptr ⇒ AddSkillToCharacter /
//   GetSkill_Main / get_Table 永不解析 ⇒ 偷盗技能报 `steal abort: AddSkillToCharacter
//   fn unresolved`。主 hook 已能 by-name（il2cpp 反射解析）装成功，故 aux 同样走 by-name 兜底：
//   先按 file offset（真机快路径，行为不变），失败再按名解析并回填缓存（每次调用都重试）。
// ---------------------------------------------------------------------------
void* MethodInfoToFn(void* methodInfo) {
    return methodInfo ? *reinterpret_cast<void**>(methodInfo) : nullptr;
}

AddSkillToCharacterFn ResolveAddSkillFn() {
    AddSkillToCharacterFn fn =
        g_fnAddSkillToCharacterAtomic.load(std::memory_order_acquire);
    if (fn) return fn;
    void* p = MethodInfoToFn(EnsureAddSkillMethodInfo());
    if (!OkPtr(p)) return nullptr;
    fn = reinterpret_cast<AddSkillToCharacterFn>(p);
    g_fnAddSkillToCharacterAtomic.store(fn, std::memory_order_release);
    LOGI(DKZJ_LOG " AddSkillToCharacter resolved by-name @%p", p);
    return fn;
}

GetSkillMainFn ResolveGetSkillMainFn() {
    GetSkillMainFn fn = g_fnGetSkillMainAtomic.load(std::memory_order_acquire);
    if (fn) return fn;
    void* p = MethodInfoToFn(EnsureGetSkillMainMethodInfo());
    if (!OkPtr(p)) return nullptr;
    fn = reinterpret_cast<GetSkillMainFn>(p);
    g_fnGetSkillMainAtomic.store(fn, std::memory_order_release);
    LOGI(DKZJ_LOG " GetSkill_Main resolved by-name @%p", p);
    return fn;
}

GameGetTableFn ResolveGameGetTableFn() {
    GameGetTableFn fn = g_fnGameGetTableAtomic.load(std::memory_order_acquire);
    if (fn) return fn;
    void* p = MethodInfoToFn(EnsureGameGetTableMethodInfo());
    if (!OkPtr(p)) return nullptr;
    fn = reinterpret_cast<GameGetTableFn>(p);
    g_fnGameGetTableAtomic.store(fn, std::memory_order_release);
    LOGI(DKZJ_LOG " Game.get_Table resolved by-name @%p", p);
    return fn;
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
    } else if (hp < 0 && !g_dmgAtkHookLive.load(std::memory_order_relaxed)) {
        // 兜底路径：攻击结算挂点（HurtBase.FinalizeAttackResult）没装上时才在扣血处乘倍率。
        // 两处都乘会变成平方倍，所以挂点上线后这里自动让位。
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

// 去重窗口（m00790 由 3000ms 收紧到 800ms）。
// 原意只是压掉「同一次三选一」在 pre/post/snap 多路回调里的重复授予，
// 3000ms 太宽：技能池连续两次抽到相同候选集合时会被误判为重复而整轮漏给。
// 800ms 足够覆盖同一次选择的连锁回调（实测同帧~数十 ms），又不会跨过两次选择。
constexpr int64_t kStealDedupeMs = 800;

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
    if (sig == lastSig && lastMs > 0 && now - lastMs < kStealDedupeMs) return false;
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

// ---------------------------------------------------------------
// 候选快照：1.1.8 实测 PostProcessLearnedSkill 触发时
// RandomSkillResult.ShowSkills 已为空（count=0），因此把每个候选来源
// 的 set_ShowSkills 赋值都按「宿主对象指针 + 来源」缓存下来，授予链
// 先用实时读，读不到再用「最近一次包含本次所选 id 的快照」兜底。
// 1.1.8 有多个 ISkillSelectData 实现各自持 ShowSkills：
//   RandomSkillResult 0x60（普通升级三选一）/ AngleViewData 0x30 /
//   ContractDevilData 0x50 / PunchboardViewData 0x48。
// 宝箱（BoxSkillResultData）**故意不接**：1.1.7 已按用户要求撤掉「宝箱全拿」。
// ---------------------------------------------------------------
constexpr int kMaxSnapshots = 16;
// 快照有效期（m00790 由 30000ms 放宽到 120000ms）：
// 一局内两次三选一之间常超过 30s，30s 会让后面的选择拿不到候选快照而漏给。
constexpr int64_t kSnapshotMaxAgeMs = 120000;

enum SnapshotSource {
    kSrcRandom = 0,
    kSrcAngle = 1,
    kSrcContract = 2,
    kSrcPunchboard = 3,
};

const char* SnapshotSourceName(int src) {
    switch (src) {
        case kSrcRandom: return "random";
        case kSrcAngle: return "angle";
        case kSrcContract: return "contract";
        case kSrcPunchboard: return "punch";
        default: return "?";
    }
}

struct SkillSnapshot {
    void* key;
    int src;
    int32_t ids[kMaxCandidates];
    int count;
    int64_t ms;
};

SkillSnapshot g_snaps[kMaxSnapshots] = {};
std::atomic<uint32_t> g_snapNext{0};
std::mutex g_snapMutex;

int ReadIdList(void* list, int32_t* out, int capacity) {
    if (!OkPtr(list) || !out || capacity <= 0) return 0;
    const int count = Il2Cpp::ReadI32(list, kOffListSize);
    if (count < 0 || count > 4096) return 0;
    void* items = Il2Cpp::ReadPtr(list, kOffListItems);
    if (!OkPtr(items)) return 0;
    const int arrayLength = Il2Cpp::ReadI32(items, kOffArrayLength);
    if (arrayLength < count || arrayLength > 4096) return 0;

    const int readable = count < capacity ? count : capacity;
    const auto* data = reinterpret_cast<const int32_t*>(
        reinterpret_cast<const uint8_t*>(items) + kOffArrayData);
    int written = 0;
    for (int i = 0; i < readable; i++) {
        if (data[i] > 0) out[written++] = data[i];
    }
    return written;
}

void StoreSnapshot(void* key, int src, const int32_t* ids, int count) {
    if (!key || !ids || count < 0 || count > kMaxCandidates) return;
    std::lock_guard<std::mutex> lock(g_snapMutex);
    const uint32_t slot =
        g_snapNext.fetch_add(1, std::memory_order_relaxed) % kMaxSnapshots;
    SkillSnapshot& snap = g_snaps[slot];
    snap.key = key;
    snap.src = src;
    snap.count = count;
    snap.ms = NowMs();
    for (int i = 0; i < count; i++) snap.ids[i] = ids[i];
}

// 取「优先同对象、其次最新」的近期快照，必须包含本次所选 id。
int LoadSnapshot(void* key, int32_t mustContain, int32_t* out, int capacity,
                 int* srcOut, void** keyOut) {
    if (!out || capacity <= 0) return 0;
    std::lock_guard<std::mutex> lock(g_snapMutex);
    const int64_t now = NowMs();
    int best = -1;
    int bestScore = -1;
    int64_t bestMs = 0;
    for (int i = 0; i < kMaxSnapshots; i++) {
        const SkillSnapshot& snap = g_snaps[i];
        if (snap.count < 2 || snap.ms <= 0) continue;
        if (now - snap.ms > kSnapshotMaxAgeMs) continue;
        if (mustContain > 0 && !ContainsSkillId(snap.ids, snap.count, mustContain)) {
            continue;
        }
        const int score = (OkPtr(key) && snap.key == key) ? 1 : 0;
        if (score > bestScore || (score == bestScore && snap.ms >= bestMs)) {
            bestScore = score;
            bestMs = snap.ms;
            best = i;
        }
    }
    if (best < 0) return 0;
    const SkillSnapshot& snap = g_snaps[best];
    const int n = snap.count < capacity ? snap.count : capacity;
    for (int j = 0; j < n; j++) out[j] = snap.ids[j];
    if (srcOut) *srcOut = snap.src;
    if (keyOut) *keyOut = snap.key;
    return n;
}

int LastSnapshotCount(void* key) {
    int count = -1;
    std::lock_guard<std::mutex> lock(g_snapMutex);
    for (int i = 0; i < kMaxSnapshots; i++) {
        if (OkPtr(key) && g_snaps[i].key == key && g_snaps[i].count > count) {
            count = g_snaps[i].count;
        }
    }
    return count;
}

void CaptureShowSkills(void* self, void* list, int src) {
    int32_t ids[kMaxCandidates] = {};
    const int count = ReadIdList(list, ids, kMaxCandidates);
    StoreSnapshot(self, src, ids, count);

    const uint32_t hit = g_showSkillsSets.fetch_add(1, std::memory_order_relaxed);
    if (hit < kMaxDiagnosticLogs) {
        LOGI(DKZJ_LOG " show-skills set src=%s self=%p list=%p count=%d head=%d",
             SnapshotSourceName(src), self, list, count,
             count > 0 ? ids[0] : 0);
    }
}

void LogStealMiss(int32_t skillId, void* heroSkill, void* randomResult,
                  int candidateCount, int candidateSource, const char* readTag) {
    const uint32_t miss = g_stealNoSelection.fetch_add(1, std::memory_order_relaxed);
    if (miss >= kMaxDiagnosticLogs) return;

    int showCount = -1;
    int ownCount = -1;
    int allCount = -1;
    int skillType = -1;
    int needSelect = -1;
    int lastSelect = -1;
    if (OkPtr(randomResult)) {
        void* showList = Il2Cpp::ReadPtr(randomResult, kOffRandomSkillShowSkills);
        if (OkPtr(showList)) showCount = Il2Cpp::ReadI32(showList, kOffListSize);
        void* ownArray = Il2Cpp::ReadPtr(randomResult, kOffRandomSkillOwnSkills);
        if (OkPtr(ownArray)) ownCount = Il2Cpp::ReadI32(ownArray, kOffArrayLength);
        void* allList = Il2Cpp::ReadPtr(randomResult, kOffRandomSkillAllSkills);
        if (OkPtr(allList)) allCount = Il2Cpp::ReadI32(allList, kOffListSize);
        skillType = Il2Cpp::ReadI32(randomResult, kOffRandomSkillType);
        needSelect = Il2Cpp::ReadI32(randomResult, kOffRandomSkillNeedSelect);
        lastSelect = Il2Cpp::ReadI32(randomResult, kOffRandomSkillLastSel);
    }
    LOGW(DKZJ_LOG
         " steal miss skill=%d hero=%p rrs=%p show=%d own=%d all=%d type=%d"
         " need=%d lastSel=%d snap=%d got=%d read=%s",
         skillId, heroSkill, randomResult, showCount, ownCount, allCount, skillType,
         needSelect, lastSelect, LastSnapshotCount(randomResult), candidateCount,
         readTag ? readTag : "none");
}

void Hooked_SetShowSkillsRandom(void* self, void* list, void* method) {
    g_il2cppRuntimeUp.store(true, std::memory_order_release);
    if (!g_origSetShowSkillsRandom) return;
    g_origSetShowSkillsRandom(self, list, method);
    CaptureShowSkills(self, list, kSrcRandom);
}

void Hooked_SetShowSkillsAngle(void* self, void* list, void* method) {
    g_il2cppRuntimeUp.store(true, std::memory_order_release);
    if (!g_origSetShowSkillsAngle) return;
    g_origSetShowSkillsAngle(self, list, method);
    CaptureShowSkills(self, list, kSrcAngle);
}

void Hooked_SetShowSkillsContract(void* self, void* list, void* method) {
    g_il2cppRuntimeUp.store(true, std::memory_order_release);
    if (!g_origSetShowSkillsContract) return;
    g_origSetShowSkillsContract(self, list, method);
    CaptureShowSkills(self, list, kSrcContract);
}

void Hooked_SetShowSkillsPunchboard(void* self, void* list, void* method) {
    g_il2cppRuntimeUp.store(true, std::memory_order_release);
    if (!g_origSetShowSkillsPunchboard) return;
    g_origSetShowSkillsPunchboard(self, list, method);
    CaptureShowSkills(self, list, kSrcPunchboard);
}

bool NameHas(const char* s, const char* sub) {
    if (!s || !sub || !*sub) return false;
    for (const char* p = s; *p; ++p) {
        const char* a = p;
        const char* b = sub;
        while (*a && *b && *a == *b) { ++a; ++b; }
        if (!*b) return true;
    }
    return false;
}

// ══ 卡密包挂点对齐（1.1.8）════════════════════════════════════════════
// 依据：10_docs/蛋壳战机/卡密包拆解/03_卡密包挂点还原与1.1.8映射_20261014.md
// 1.1.7 那个 mod 是 5 个硬编码 RVA + 函数指针表改写；1.1.8 已无 16 字节间接桩，
// 所以这里直接用星光版的 HookAtFileOffset（序言校验 + Dobby）落在同一批游戏函数上。

// ── ① 伤害倍率（英雄攻击）：HurtBase.FinalizeAttackResult(FP) → FP ──
// 卡密包把倍率乘在「攻击结算结果」上（不是扣血处）。FP = 单 long 的 16.16 定点
//（RAW_ONE=65536），AAPCS64 下该 8 字节结构走 x1 入参 / x0 返回。
// 1.1.8 file 0x6D39C44 只有 0x7C 字节（IL2CPP 转发桩：ldr method; br 真实体），
// 返回值就是最终攻击值 ⇒ 直接乘倍率。
// 被击中方是英雄/僚机时不动，否则敌人打我方的伤害也会翻倍（卡密包没做这个过滤，
// 因为它默认同时开锁血，玩家察觉不到；星光版要单独开上帝模式，所以必须自己过滤）。
bool VictimIsHero(void* hurtSelf) {
    if (!OkPtr(hurtSelf)) return false;
    auto& api = Il2Cpp::GetApi();
    if (!api.object_class || !api.class_name) return false;
    void* victim = Il2Cpp::ReadPtr(hurtSelf, kOffHurtEntity);
    if (!OkPtr(victim)) return false;
    void* klass = api.object_class(victim);
    if (!klass) return false;
    const char* name = api.class_name(klass);
    return NameHas(name, "Hero") || NameHas(name, "Follower");
}

int64_t Hooked_FinalizeAttackResult(void* self, int64_t rawResult, void* method) {
    g_il2cppRuntimeUp.store(true, std::memory_order_release);
    if (!g_origFinalizeAttackResult) return rawResult;
    if (t_hurtDepth > 0) return g_origFinalizeAttackResult(self, rawResult, method);

    struct HurtGuard {
        HurtGuard() { ++t_hurtDepth; }
        ~HurtGuard() { if (t_hurtDepth > 0) --t_hurtDepth; }
    } hurtGuard;

    const int64_t result = g_origFinalizeAttackResult(self, rawResult, method);

    const int multiplier =
        ClampAttackMultiplier(g_attackMultiplier.load(std::memory_order_relaxed));
    if (multiplier <= 1 || result == 0) return result;
    if (!OkPtr(self) || VictimIsHero(self)) return result;

    const int64_t scaled = SaturatingScaleInt64(result, multiplier);
    const uint32_t hit = g_dmgAtkHits.fetch_add(1, std::memory_order_relaxed);
    if (hit < kMaxDiagnosticLogs) {
        LOGI(DKZJ_LOG " attack-finalize x%d raw=%lld -> %lld",
             multiplier, static_cast<long long>(result),
             static_cast<long long>(scaled));
    }
    return scaled;
}

// ── ② 上帝模式补刀：EntityHero.set_CurrentHp(long) ──
// 卡密包同时锁 ChangeHp（增量）与 CurrentHp 属性 setter；星光版之前只有前者，
// 所以凡是通过属性赋值扣血/致死判定的路径会漏过去。这里直接拒绝「往下改」。
void Hooked_EntityHeroSetCurrentHp(void* self, int64_t value, void* method) {
    g_il2cppRuntimeUp.store(true, std::memory_order_release);
    if (!g_origEntityHeroSetCurrentHp) return;
    if (g_godMode.load(std::memory_order_relaxed) && OkPtr(self)) {
        const int64_t current =
            *reinterpret_cast<const int64_t*>(reinterpret_cast<const uint8_t*>(self) +
                                              kOffCharCurrentHp);
        if (value < current) {
            const uint32_t hit = g_godSetHits.fetch_add(1, std::memory_order_relaxed);
            if (hit < kMaxDiagnosticLogs) {
                LOGI(DKZJ_LOG " god blocked set_CurrentHp %lld -> %lld",
                     static_cast<long long>(value), static_cast<long long>(current));
            }
            return;   // 不落值 ⇒ 血量不变
        }
    }
    g_origEntityHeroSetCurrentHp(self, value, method);
}


// 拦截上报：BattleRecordManager.IsNeedUploadSaveData()（private static bool）。
// 返回 false ⇒ 游戏走自带合法分支 SkippedNotNeeded=2：跳过存档/录像上传，
// 不进对账请求，也不改任何战斗结果字段（不碰 PassWave/Rewards/存档哈希）。
bool Hooked_IsNeedUploadSaveData(void* method) {
    if (g_blockReport.load(std::memory_order_relaxed)) {
        const uint32_t n = g_reportBlocks.fetch_add(1, std::memory_order_relaxed) + 1;
        if (n <= kMaxDiagnosticLogs) {
            LOGI(DKZJ_LOG " block-report skip#%u -> IsNeedUploadSaveData=false", n);
        }
        return false;
    }
    if (g_origIsNeedUploadSaveData) return g_origIsNeedUploadSaveData(method);
    return true;
}

void* ResolveGameTable() {
    GameGetTableFn fn = ResolveGameGetTableFn();
    if (!fn) return nullptr;
    return fn();
}

void* ResolveSkillMain(int32_t skillId) {
    GetSkillMainFn fn = ResolveGetSkillMainFn();
    if (!fn || skillId <= 0) return nullptr;
    void* table = ResolveGameTable();
    if (!OkPtr(table)) return nullptr;
    void* methodInfo = EnsureGetSkillMainMethodInfo();
    void* skill = fn(table, skillId, methodInfo);
    return OkPtr(skill) ? skill : nullptr;
}

int ReadIntArrayField(void* obj, size_t off, int32_t* out, int capacity) {
    if (!OkPtr(obj) || !out || capacity <= 0) return 0;
    void* array = Il2Cpp::ReadPtr(obj, off);
    if (!OkPtr(array)) return 0;
    const int count = Il2Cpp::ReadI32(array, kOffArrayLength);
    if (count < 0 || count > kMaxUpgradeIds * 8) return 0;

    const int readable = count < capacity ? count : capacity;
    const auto* data = reinterpret_cast<const int32_t*>(
        reinterpret_cast<const uint8_t*>(array) + kOffArrayData);
    for (int i = 0; i < readable; i++) out[i] = data[i];
    return readable;
}

int ReadUpgradeIds(void* skillMain, int32_t* out, int capacity) {
    return ReadIntArrayField(skillMain, kOffSkillUpgradeIds, out, capacity);
}

// 已拥有候选 → 下一级 id 的**多来源**解析（1.1.8 实测 UpgradeSkillId 常为空）。
// 顺序：UpgradeSkillId(0x30) → UpgradeSkillId_B(0x150) → UnlockUpgradeId(0x158)
//       → NextSkill(0x84) → CoverSkill(0x80)。
// 每个来源都取“第一个未被拥有”的 id；全部落空返回 0。
// m00790 补 UpgradeSkillId_B：1.1.8 的 Skill_Main 有两条升级链
// （IsUpgrade 0x28 / UpgradeSkillId 0x30 与 IsUpgrade_B 0x14C / UpgradeSkillId_B 0x150），
// 双形态技能只填后者，之前会漏掉这类升级。**不**加入 UnlockUpgradeNeddSkill(0x160)：
// 那是「解锁所需前置技能」，当成升级目标授予会授错技能。
int32_t PickNextTier(void* skillMain, const int32_t* owned, int ownedCount,
                     const char** srcOut) {
    if (srcOut) *srcOut = "none";
    if (!OkPtr(skillMain)) return 0;

    struct Source {
        size_t off;
        const char* name;
        bool isArray;
    };
    const Source sources[] = {
        {kOffSkillUpgradeIds, "upgrade", true},
        {kOffSkillUpgradeSkillIdB, "upgradeB", true},
        {kOffSkillUnlockUpgradeId, "unlock", true},
        {kOffSkillRepeatSkills, "repeat", true},
        {kOffSkillNextSkill, "next", false},
        {kOffSkillCoverSkill, "cover", false},
    };

    int32_t buf[kMaxUpgradeIds] = {};
    for (const Source& s : sources) {
        if (s.isArray) {
            const int n = ReadIntArrayField(skillMain, s.off, buf, kMaxUpgradeIds);
            for (int i = 0; i < n; i++) {
                if (buf[i] > 0 && !ContainsSkillId(owned, ownedCount, buf[i])) {
                    if (srcOut) *srcOut = s.name;
                    return buf[i];
                }
            }
        } else {
            const int32_t id = Il2Cpp::ReadI32(skillMain, s.off);
            if (id > 0 && !ContainsSkillId(owned, ownedCount, id)) {
                if (srcOut) *srcOut = s.name;
                return id;
            }
        }
    }
    return 0;
}

// 原版升级池会跳过已拥有升级 id。这里显式把已拥有候选映射到第一个
// 未拥有的下一级；没有下一级时返回原 id，让完整授予链自行拒绝。
int32_t ResolveGrantId(void* skillMain, int32_t requestedId,
                       const int32_t* owned, int ownedCount, const char** srcOut) {
    if (srcOut) *srcOut = "none";
    if (!ContainsSkillId(owned, ownedCount, requestedId)) return requestedId;

    const char* src = "none";
    const int32_t next = PickNextTier(skillMain, owned, ownedCount, &src);
    if (next <= 0) return requestedId;

    const uint32_t n = g_upgradeRewrites.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= kMaxDiagnosticLogs) {
        LOGI(DKZJ_LOG " upgrade requested=%d -> granted=%d src=%s",
             requestedId, next, src);
    }
    if (srcOut) *srcOut = src;
    return next;
}

bool GrantOne(void* self, void* entity, void* hrs, int32_t requestedId,
              const int32_t* owned, int ownedCount, void* methodInfo,
              int32_t* grantedIdOut) {
    void* skillMain = ResolveSkillMain(requestedId);
    // 诊断：每候选打印一次决策依据（Skill_Main 取不到时升级链永远走不到）。
    const uint32_t d = g_candLogs.fetch_add(1, std::memory_order_relaxed);
    const bool ownedAlready = ContainsSkillId(owned, ownedCount, requestedId);
    if (d < kMaxCandLogs) {
        int32_t ups[kMaxUpgradeIds] = {};
        const int uc = ReadIntArrayField(skillMain, kOffSkillUpgradeIds, ups, kMaxUpgradeIds);
        int32_t unl[kMaxUpgradeIds] = {};
        const int un = ReadIntArrayField(skillMain, kOffSkillUnlockUpgradeId, unl, kMaxUpgradeIds);
        int32_t upb[kMaxUpgradeIds] = {};
        const int ub = ReadIntArrayField(skillMain, kOffSkillUpgradeSkillIdB, upb, kMaxUpgradeIds);
        LOGI(DKZJ_LOG
             " steal cand id=%d sm=%p owned=%d uc=%d[%d,%d] un=%d[%d,%d]"
             " uB=%d[%d,%d] star=%d/%d up=%d upB=%d next=%d cover=%d",
             requestedId, skillMain, ownedAlready ? 1 : 0,
             uc, uc > 0 ? ups[0] : 0, uc > 1 ? ups[1] : 0,
             un, un > 0 ? unl[0] : 0, un > 1 ? unl[1] : 0,
             ub, ub > 0 ? upb[0] : 0, ub > 1 ? upb[1] : 0,
             skillMain ? Il2Cpp::ReadI32(skillMain, kOffSkillStar) : -1,
             skillMain ? Il2Cpp::ReadI32(skillMain, kOffSkillMaxStar) : -1,
             skillMain ? Il2Cpp::ReadI32(skillMain, kOffSkillIsUpgrade) : -1,
             skillMain ? Il2Cpp::ReadI32(skillMain, kOffSkillIsUpgradeB) : -1,
             skillMain ? Il2Cpp::ReadI32(skillMain, kOffSkillNextSkill) : -1,
             skillMain ? Il2Cpp::ReadI32(skillMain, kOffSkillCoverSkill) : -1);
    }

    const char* src = "none";
    const int32_t grantId = (skillMain && ownedAlready)
        ? ResolveGrantId(skillMain, requestedId, owned, ownedCount, &src)
        : requestedId;
    AddSkillToCharacterFn fn = ResolveAddSkillFn();
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

    // 1.1.8 实测：原版返回后 heroSkill._randomSkillResult.ShowSkills 已被清空
    // （`steal skill=N … ShowSkills count=0`），故必须在原版执行前先抓一份候选。
    const bool argsOk = OkPtr(heroSkill) && skillId > 0;
    void* randomResult = argsOk ? Il2Cpp::ReadPtr(heroSkill, kOffHrsRandomSkillResult)
                                : nullptr;
    int32_t preCandidates[kMaxCandidates] = {};
    int preCount = argsOk ? ReadShowSkillIds(heroSkill, preCandidates, kMaxCandidates) : 0;

    g_origPostProcessLearnedSkill(self, entity, skillId, skillData, addToComponent,
                                  heroSkill, method);

    if (!OkPtr(self) || !OkPtr(entity) || !argsOk) return;
    if (t_stealDepth != 1) return;
    if (!g_stealSkill.load(std::memory_order_relaxed)) return;

    AddSkillToCharacterFn fn = ResolveAddSkillFn();
    if (!fn) {
        LOGW(DKZJ_LOG " steal abort: AddSkillToCharacter fn unresolved picked=%d",
             skillId);
        return;
    }

    int32_t candidates[kMaxCandidates] = {};
    int candidateCount = 0;
    int candidateSource = -1;
    void* candidateKey = randomResult;
    const char* readTag = "none";
    if (preCount >= 2 && ContainsSkillId(preCandidates, preCount, skillId)) {
        for (int i = 0; i < preCount; i++) candidates[i] = preCandidates[i];
        candidateCount = preCount;
        candidateSource = kSrcRandom;
        readTag = "pre";
        StoreSnapshot(randomResult, kSrcRandom, preCandidates, preCount);
    } else {
        candidateCount = ReadShowSkillIds(heroSkill, candidates, kMaxCandidates);
        readTag = "post";
    }
    const bool liveSource = candidateCount >= 2;
    if (!liveSource || !ContainsSkillId(candidates, candidateCount, skillId)) {
        int snapSource = -1;
        void* snapKey = nullptr;
        const int cached = LoadSnapshot(randomResult, skillId, candidates,
                                        kMaxCandidates, &snapSource, &snapKey);
        if (cached >= 2) {
            candidateCount = cached;
            candidateSource = snapSource;
            candidateKey = snapKey;
            readTag = "snap";
        }
    }
    if (candidateCount < 2 ||
        !ContainsSkillId(candidates, candidateCount, skillId)) {
        LogStealMiss(skillId, heroSkill, randomResult, candidateCount,
                     candidateSource, readTag);
        return;
    }

    if (readTag[0] == 's') {
        g_snapshotHits.fetch_add(1, std::memory_order_relaxed);
    }
    {
        const uint32_t hit = g_readPathLogs.fetch_add(1, std::memory_order_relaxed);
        if (hit < kMaxDiagnosticLogs) {
            LOGI(DKZJ_LOG " steal read=%s src=%s picked=%d count=%d",
                 readTag, SnapshotSourceName(candidateSource), skillId,
                 candidateCount);
        }
    }

    if (!ShouldInjectOnce(OkPtr(candidateKey) ? candidateKey : heroSkill, candidates,
                          candidateCount)) {
        const uint32_t d = g_dedupeSkips.fetch_add(1, std::memory_order_relaxed);
        if (d < kMaxDiagnosticLogs) {
            LOGW(DKZJ_LOG " steal dedupe skip picked=%d count=%d read=%s",
                 skillId, candidateCount, readTag);
        }
        return;
    }

    int32_t owned[kMaxOwnedSkills] = {};
    void* ownedItems = ReadOwnedSkills(heroSkill, owned, kMaxOwnedSkills);
    int ownedCount = 0;
    if (!OkPtr(ownedItems)) {
        LOGW(DKZJ_LOG " steal owned read fail hrs=%p picked=%d (continue owned=0)",
             heroSkill, skillId);
    } else {
        ownedCount = Il2Cpp::ReadI32(ownedItems, kOffArrayLength);
        if (ownedCount < 0 || ownedCount > kMaxOwnedSkills) {
            LOGW(DKZJ_LOG " steal owned count=%d out of range items=%p picked=%d"
                 " (continue owned=0)",
                 ownedCount, ownedItems, skillId);
            ownedCount = 0;
        }
    }

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
            const uint32_t f = g_grantFails.fetch_add(1, std::memory_order_relaxed);
            if (f < kMaxDiagnosticLogs) {
                void* sm = ResolveSkillMain(requestedId);
                LOGW(DKZJ_LOG " steal grant fail requested=%d skillMain=%p owned=%d"
                     " picked=%d (already owned or rejected)",
                     requestedId, sm, ownedCount, skillId);
            }
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
            // m00790：不再只打 first/last，把本轮实际授予的 id 全列出来，
            // 便于区分「本轮只补了 1 个」与「候选被去重/升级链解析成同一个 id」。
            char ids[192] = {};
            size_t used = 0;
            for (int i = 0; i < granted && used + 12 < sizeof(ids); i++) {
                const int n = snprintf(ids + used, sizeof(ids) - used,
                                       i ? ",%d" : "%d", grantedIds[i]);
                if (n <= 0) break;
                used += static_cast<size_t>(n);
            }
            LOGI(DKZJ_LOG
                 " steal picked=%d granted=%d first=%d last=%d ids=[%s] src=%s read=%s",
                 skillId, granted, grantedIds[0], grantedIds[granted - 1], ids,
                 SnapshotSourceName(candidateSource), readTag);
        }
    } else {
        const uint32_t miss = g_stealNoSelection.fetch_add(1, std::memory_order_relaxed);
        if (miss < kMaxDiagnosticLogs) {
            LOGW(DKZJ_LOG
                 " steal picked=%d candidates=%d no extra skills granted src=%s read=%s",
                 skillId, candidateCount, SnapshotSourceName(candidateSource), readTag);
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

// ── ④ 免广告（卡密包挂点）：Framework.GameAdsManager.CheckAndPlayVideo 4 参 ──
// 1.1.8 file 0x43C9720；签名 (Action<bool> onClose, Action onShow, Action<string> onPlayFail,
// Func<string> getSource())。这层比 ADModuleMgr 低，所有广告入口都过它。
// 卡密包的做法：不播广告，反射直接调 onShow() 与 onClose(true)，让上层以为「已展示且看完」。
bool InvokeDelegateNoArg(void* action) {
    if (!OkPtr(action) || !AttachCurrentThreadToIl2Cpp()) return false;
    auto& api = GetApi();
    if (!api.runtime_invoke || !api.object_class) return false;

    void* klass = api.object_class(action);
    if (!klass) return false;
    void* methodInfo = api.GetMethod(klass, "Invoke", 0);
    if (!methodInfo) {
        LOGW(DKZJ_LOG " ads-mgr Action.Invoke(0) resolve miss");
        return false;
    }
    void* exception = nullptr;
    try {
        api.runtime_invoke(methodInfo, action, nullptr, &exception);
    } catch (...) {
        LOGW(DKZJ_LOG " ads-mgr onShow invoke C++ exception");
        return false;
    }
    if (exception) {
        LOGW(DKZJ_LOG " ads-mgr onShow invoke exception");
        return false;
    }
    return true;
}

bool Hooked_AdsMgrCheckAndPlayVideo(void* self, void* onClose, void* onShow,
                                    void* onPlayFail, void* getSource, void* method) {
    g_il2cppRuntimeUp.store(true, std::memory_order_release);
    if (g_noAds.load(std::memory_order_relaxed) && !t_adReenter) {
        // 用「保存-恢复」而不是 RAII 清标志：InvokeActionBool 内部也会动 t_adReenter
        const bool prevReenter = t_adReenter;
        t_adReenter = true;
        const bool showed = InvokeDelegateNoArg(onShow);
        const bool closed = InvokeActionBool(onClose);
        t_adReenter = prevReenter;
        if (showed || closed) {
            const uint32_t hit = g_noAdsMgrHits.fetch_add(1, std::memory_order_relaxed) + 1;
            if (hit <= kMaxDiagnosticLogs) {
                LOGI(DKZJ_LOG " no-ads GameAdsManager: onShow=%d onClose(true)=%d -> 视为已观看",
                     showed ? 1 : 0, closed ? 1 : 0);
            }
            return true;
        }
        g_noAdsFallbacks.fetch_add(1, std::memory_order_relaxed);
    }
    return g_origAdsMgrCheckAndPlayVideo
        ? g_origAdsMgrCheckAndPlayVideo(self, onClose, onShow, onPlayFail, getSource, method)
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
    // ── 卡密包挂点对齐（1.1.8 新增；见 03_卡密包挂点还原与1.1.8映射_20261014.md）──
    {
        "HurtBase.FinalizeAttackResult",
        kFileHurtBaseFinalizeAttackResult,
        kInsnHurtBaseFinalizeAttackResult,
        "HotFix.BattleLogic",
        "HurtBase",
        "FinalizeAttackResult",
        1,
        reinterpret_cast<void*>(Hooked_FinalizeAttackResult),
        reinterpret_cast<void**>(&g_origFinalizeAttackResult),
    },
    {
        "EntityHero.set_CurrentHp",
        kFileEntityHeroSetCurrentHp,
        kInsnEntityHeroSetCurrentHp,
        "HotFix.BattleLogic",
        "EntityHero",
        "set_CurrentHp",
        1,
        reinterpret_cast<void*>(Hooked_EntityHeroSetCurrentHp),
        reinterpret_cast<void**>(&g_origEntityHeroSetCurrentHp),
    },
    {
        "GameAdsManager.CheckAndPlayVideo",
        kFileAdsMgrCheckAndPlayVideo,
        kInsnAdsMgrCheckAndPlayVideo,
        "Framework",
        "GameAdsManager",
        "CheckAndPlayVideo",
        4,
        reinterpret_cast<void*>(Hooked_AdsMgrCheckAndPlayVideo),
        reinterpret_cast<void**>(&g_origAdsMgrCheckAndPlayVideo),
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
    {
        "RandomSkillResult.set_ShowSkills",
        kFileSetShowSkillsRandom,
        kInsnSetShowSkills,
        "HotFix.BattleLogic",
        "RandomSkillResult",
        "set_ShowSkills",
        1,
        reinterpret_cast<void*>(Hooked_SetShowSkillsRandom),
        reinterpret_cast<void**>(&g_origSetShowSkillsRandom),
    },
    {
        "AngleViewData.set_ShowSkills",
        kFileSetShowSkillsAngle,
        kInsnSetShowSkills,
        "HotFix",
        "AngleViewData",
        "set_ShowSkills",
        1,
        reinterpret_cast<void*>(Hooked_SetShowSkillsAngle),
        reinterpret_cast<void**>(&g_origSetShowSkillsAngle),
    },
    {
        "ContractDevilData.set_ShowSkills",
        kFileSetShowSkillsContract,
        kInsnSetShowSkills,
        "HotFix",
        "ContractDevilData",
        "set_ShowSkills",
        1,
        reinterpret_cast<void*>(Hooked_SetShowSkillsContract),
        reinterpret_cast<void**>(&g_origSetShowSkillsContract),
    },
    {
        "PunchboardViewData.set_ShowSkills",
        kFileSetShowSkillsPunchboard,
        kInsnSetShowSkills,
        "HotFix",
        "PunchboardViewData",
        "set_ShowSkills",
        1,
        reinterpret_cast<void*>(Hooked_SetShowSkillsPunchboard),
        reinterpret_cast<void**>(&g_origSetShowSkillsPunchboard),
    },
    {
        "BattleRecordManager.IsNeedUploadSaveData",
        kFileIsNeedUploadSaveData,
        kInsnIsNeedUploadSaveData,
        "HotFix",
        "BattleRecordManager",
        "IsNeedUploadSaveData",
        0,
        reinterpret_cast<void*>(Hooked_IsNeedUploadSaveData),
        reinterpret_cast<void**>(&g_origIsNeedUploadSaveData),
    },
};

void SyncHooks() {
    if (!g_libReady.load(std::memory_order_relaxed)) return;
    std::lock_guard<std::mutex> lock(g_hookMutex);
    for (const auto& slot : kHookSlots) {
        InstallSlot(slot);
    }
    // 攻击结算挂点一旦上线，扣血处的兜底倍率就让位（两处都乘会变成平方倍）
    g_dmgAtkHookLive.store(g_origFinalizeAttackResult != nullptr,
                           std::memory_order_relaxed);
    g_adsMgrHookLive.store(g_origAdsMgrCheckAndPlayVideo != nullptr,
                           std::memory_order_relaxed);
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
        // 「立即胜利」已于 2026-10-14（用户 m00790）整体删除，enum 项与菜单项一并移除。
        // 拦截上报没有 case：按用户要求（m00392 第 2 条）它是**常开注入**，
        // 不进菜单、不做开关，g_blockReport 恒为 true。
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
        // 拦截上报常开：不受菜单影响，这里显式钉死
        g_blockReport.store(true, std::memory_order_relaxed);
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
    g_blockReport.store(true, std::memory_order_relaxed);   // 常开：清空也不关
    ResetHeroDepth();
    t_stealDepth = 0;
    g_lastStealSig.store(0, std::memory_order_relaxed);
    g_lastStealMs.store(0, std::memory_order_relaxed);
}
