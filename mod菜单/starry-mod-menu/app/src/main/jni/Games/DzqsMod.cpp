/* ================================================================
 *  独奏骑士 1.1.760 — HybridCLR 公共跳板 + 方法名过滤
 *
 *  挂点：libil2cpp RVA 0x20B840C（斌哥 memdiff）
 *  偏移来源：06_dumped/独奏骑士/replacement_disasm.txt（斌哥壳实锤，非猜）
 *
 *  isCdOK（disasm 0x61c84..0x61cd8）：
 *    role = *(skill+0x1a8); camp = *(*role+0x80)+0xa8
 *    baseData = *(skill+0x88); 若 [baseData+0x18]>=0x52 → [baseData+0x2ac]=0
 *  1.1.768 校正：zCd 是 Game_RoleSkill_Property[81] 的 _val，偏移为 baseData+0x2AC。
 *  索引 82 是 damage_ex_p；误写 +0x2B4 会命中错误字段。
 *  isManaOk：skill+0x5c ← 1（strb）
 *  calDamageBuffer：target=*(args+0x10); camp 同；kill 写 args+0；inv 写 0
 * ================================================================ */

#include "DzqsMod.h"

#include <android/log.h>
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

#define DZQS_TRACE(...) \
    __android_log_print(ANDROID_LOG_INFO, "StarryTrace", "[Dzqs] " __VA_ARGS__)

/* 正式包：关 Trace 热路径；伤害写入开 */
#ifndef DZQS_TRACE_BUILD
#define DZQS_TRACE_BUILD 0
#endif
#ifndef DZQS_DMG_WRITE
#define DZQS_DMG_WRITE 1
#endif

#if !DZQS_TRACE_BUILD
#undef DZQS_TRACE
#define DZQS_TRACE(...) ((void)0)
#endif

enum : uintptr_t {
    // 1.1.768 EnterFrameFromInterpreter, VA 0x20B9A40.
    FILE_ENTER_INTERP = 0x20B5A40,
};

/* —— 全部来自 replacement_disasm.txt —— */
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
        if (!strchr(perm, 'x') || end <= start) continue;
        if (fileOff >= off && fileOff < off + (end - start)) {
            found = reinterpret_cast<void*>(start + (fileOff - off));
            break;
        }
    }
    fclose(fp);
    return found;
}

enum : size_t {
    kOffArgsTarget   = 0x10,  // calDamageBuffer: ldr x24,[x19,#0x10]
    kOffArgsTargetTd = 0x08,  // calTotalDamage:  ldr x24,[x19,#8]
    kOffArgsExDmg    = 0x18,  // calTotalDamage:  str s0,[x19,#0x18]
    kOffCampPtr      = 0x80,
    kOffCampId       = 0xA8,
    kOffSkillRole    = 0x1B0, // 1.1.768 runtime field offset
    kOffSkillBaseData= 0x88,  // Game_RoleSkill._baseData
    kOffArrLen       = 0x18,  // array length
    kOffArrZCd       = 0x20 + 81 * 8 + 4,  // zCd property _val = baseData+0x2AC
    kOffSkillManaOk  = 0x5C,  // isManaOk: strb 1,[skill,#0x5c]
    kOffRoleFriendly = 0x90,  // Game_Role._isFriendly
    kOffRoleRtSkills = 0x120, // Game_Role._rtSkills
    kOffListItems    = 0x10,  // List<T>._items
    kOffListSize     = 0x18,  // List<T>._size
    kOffArrayData    = 0x20,  // T[] object data
    kOffHealArg8     = 0x08,
};

enum : int32_t { kCampFriend = 1 };

static constexpr float kKillDmgMild = 20000.f;      // 0x469C3E00
static constexpr float kKillDmgHard = 1.0e8f;       // 0x4CBEBC20
static constexpr float kHealAmt     = 1000000.f;

enum MethKind : uint8_t {
    MK_NONE = 0,
    MK_TOTAL_DMG,
    MK_DMG_BUF,
    MK_ADD_HP,
    MK_CD_OK,
    MK_MANA_OK,
};

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_hooked{false};
static std::atomic<bool> g_kill{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_nocd{false};
static std::atomic<uint32_t> g_hitsKill{0};
static std::atomic<uint32_t> g_hitsInv{0};
static std::atomic<uint32_t> g_hitsNocd{0};
static std::atomic<uint32_t> g_hitsMana{0};
static std::atomic<uint32_t> g_clearCalls{0};

typedef void* (*Trampoline_t)(void* a0, void* a1, void* a2, void* a3);
typedef const char* (*method_get_name_t)(void* method);
typedef void* (*class_get_field_from_name_t)(void* klass, const char* name);
typedef size_t (*field_get_offset_t)(void* field);

static Trampoline_t g_origTramp = nullptr;
static method_get_name_t g_methodGetName = nullptr;
static class_get_field_from_name_t g_classGetField = nullptr;
static field_get_offset_t g_fieldGetOffset = nullptr;
static std::atomic<bool> g_skillFieldsResolved{false};
static std::atomic<bool> g_roleFieldsResolved{false};
static size_t g_offSkillRole = kOffSkillRole;
static size_t g_offSkillBaseData = kOffSkillBaseData;
static size_t g_offRoleFriendly = kOffRoleFriendly;
static size_t g_offRoleRtSkills = kOffRoleRtSkills;

static constexpr int kCacheCap = 256;
struct MethCacheSlot {
    void* mi = nullptr;
    MethKind kind = MK_NONE;
};
static MethCacheSlot g_methCache[kCacheCap]{};

static void ClearMethCache() {
    for (int i = 0; i < kCacheCap; i++) {
        g_methCache[i].mi = nullptr;
        g_methCache[i].kind = MK_NONE;
    }
}

static bool SoftPtr(void* p) {
    if (!p) return false;
    const uintptr_t u = starptr::Untag((uintptr_t)p);
    if (u & 7) return false;
    // ★设备无关修正：上界原为 0x8000000000（512GiB）——真机 Scudo 堆会落到 0x76…/0x78…
    //   （0xb4000076c1646d58 清标签后 = 0x76c1646d58 ≈ 509.6GiB，逼近上界；0x78… 直接越界）
    //   ⇒ 越界即整片判否，而模拟器地址小所以只在真机暴露。统一改用共享区间 4GiB~128TiB。
    return u >= starptr::kUserVaMin && u < starptr::kUserVaMax;
}

static bool SoftHeapPtr(void* p) {
    if (!SoftPtr(p)) return false;
    uintptr_t u = starptr::Untag((uintptr_t)p);
    const uintptr_t tag = u >> 40;
    return tag != 0x71 && tag != 0x2e;
}

static bool SoftArgPtr(void* p) { return SoftHeapPtr(p); }

static int32_t ReadI32(void* obj, size_t off) {
    if (!SoftArgPtr(obj)) return -1;
    return *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(obj) + off);
}

static float ReadF32(void* obj, size_t off) {
    if (!SoftArgPtr(obj)) return 0.f;
    return *reinterpret_cast<float*>(reinterpret_cast<uintptr_t>(obj) + off);
}

static void* ReadPtr(void* obj, size_t off) {
    if (!SoftArgPtr(obj)) return nullptr;
    return *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(obj) + off);
}

static void WriteF32(void* obj, size_t off, float v) {
    if (!SoftArgPtr(obj)) return;
    *reinterpret_cast<float*>(reinterpret_cast<uintptr_t>(obj) + off) = v;
}

static void WriteI32(void* obj, size_t off, int32_t v) {
    if (!SoftArgPtr(obj)) return;
    *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(obj) + off) = v;
}

static void WriteU8(void* obj, size_t off, uint8_t v) {
    if (!SoftArgPtr(obj)) return;
    *reinterpret_cast<uint8_t*>(reinterpret_cast<uintptr_t>(obj) + off) = v;
}

static uint8_t ReadU8(void* obj, size_t off) {
    if (!SoftArgPtr(obj)) return 0;
    return *reinterpret_cast<uint8_t*>(reinterpret_cast<uintptr_t>(obj) + off);
}

/** role+0x80 → +0xa8；失败 -1（disasm 失败时 mov w4,#-1） */
static int32_t ReadCampId(void* role) {
    if (!SoftArgPtr(role)) return -1;
    void* st = ReadPtr(role, kOffCampPtr);
    if (!SoftArgPtr(st)) return -1;
    return ReadI32(st, kOffCampId);
}

/** 斌哥 calDamageBuffer 秒杀：camp∈[2,8]∪[9,14] */
static bool CampKillRange(int32_t camp) {
    if (camp >= 2 && camp <= 8) return true;
    if (camp >= 9 && camp <= 14) return true;
    return false;
}

static MethKind ClassifyName(const char* name) {
    if (!name) return MK_NONE;
    if (strcmp(name, "calTotalDamage") == 0) return MK_TOTAL_DMG;
    if (strcmp(name, "calDamageBuffer") == 0) return MK_DMG_BUF;
    if (strcmp(name, "AddRtHPTo") == 0) return MK_ADD_HP;
    if (strcmp(name, "isCdOK") == 0) return MK_CD_OK;
    if (strcmp(name, "isManaOk") == 0) return MK_MANA_OK;
    return MK_NONE;
}

static MethKind LookupKind(void* mi) {
    if (!mi || !SoftPtr(mi)) return MK_NONE;
    uintptr_t h = ((uintptr_t)mi >> 4) ^ ((uintptr_t)mi >> 12);
    MethCacheSlot& slot = g_methCache[h % kCacheCap];
    if (slot.mi == mi) return slot.kind;
    const char* name = g_methodGetName ? g_methodGetName(mi) : nullptr;
    MethKind k = ClassifyName(name);
    slot.mi = mi;
    slot.kind = k;
    return k;
}

static void* ArgPtr(void* args, size_t byteOff) {
    if (!SoftArgPtr(args)) return nullptr;
    return ReadPtr(args, byteOff);
}

static void* InstSelf(void* a2) {
    void* self = ArgPtr(a2, 0);
    return SoftArgPtr(self) ? self : nullptr;
}

static void* ObjectKlass(void* obj) {
    return SoftArgPtr(obj) ? ReadPtr(obj, 0) : nullptr;
}

static bool ResolveSkillFields(void* skill) {
    if (g_skillFieldsResolved.load()) return true;
    if (!g_classGetField || !g_fieldGetOffset) return false;
    void* klass = ObjectKlass(skill);
    if (!SoftArgPtr(klass)) return false;

    void* fRole = g_classGetField(klass, "_role");
    void* fBase = g_classGetField(klass, "_baseData");
    const size_t offRole = fRole ? g_fieldGetOffset(fRole) : 0;
    const size_t offBase = fBase ? g_fieldGetOffset(fBase) : 0;
    if (!offRole || !offBase) {
        LOGI("[Dzqs] field resolve skill failed role=%p base=%p offRole=%zu offBase=%zu",
             fRole, fBase, offRole, offBase);
        return false;
    }

    g_offSkillRole = offRole;
    g_offSkillBaseData = offBase;
    g_skillFieldsResolved.store(true);
    LOGI("[Dzqs] field resolve Game_RoleSkill klass=%p _role=0x%zX _baseData=0x%zX",
         klass, offRole, offBase);
    return true;
}

static bool ResolveRoleFields(void* role) {
    if (g_roleFieldsResolved.load()) return true;
    if (!g_classGetField || !g_fieldGetOffset) return false;
    void* klass = ObjectKlass(role);
    if (!SoftArgPtr(klass)) return false;

    void* fFriendly = g_classGetField(klass, "_isFriendly");
    void* fSkills = g_classGetField(klass, "_rtSkills");
    const size_t offFriendly = fFriendly ? g_fieldGetOffset(fFriendly) : 0;
    const size_t offSkills = fSkills ? g_fieldGetOffset(fSkills) : 0;
    if (!offFriendly || !offSkills) {
        LOGI("[Dzqs] field resolve role failed friendly=%p skills=%p offFriendly=%zu offSkills=%zu",
             fFriendly, fSkills, offFriendly, offSkills);
        return false;
    }

    g_offRoleFriendly = offFriendly;
    g_offRoleRtSkills = offSkills;
    g_roleFieldsResolved.store(true);
    LOGI("[Dzqs] field resolve Game_Role klass=%p _isFriendly=0x%zX _rtSkills=0x%zX",
         klass, offFriendly, offSkills);
    return true;
}

static bool IsFriendlyRole(void* role) {
    if (!SoftArgPtr(role)) return false;
    ResolveRoleFields(role);
    return ReadU8(role, g_offRoleFriendly) != 0 || ReadCampId(role) == kCampFriend;
}

/** Clear live cooldowns for a friendly role's compiled skill list. */
static void ClearRoleSkillCooldowns(void* role, const char* source) {
    if (!g_nocd.load()) return;

    uint32_t n = g_clearCalls.fetch_add(1) + 1;
    const bool shouldLog = n <= 8 || (n % 200) == 0;
    if (!SoftArgPtr(role)) {
        if (shouldLog) LOGI("[Dzqs] nocd probe %s #%u invalid-role=%p", source, n, role);
        return;
    }

    ResolveRoleFields(role);
    const uint8_t friendlyFlag = ReadU8(role, g_offRoleFriendly);
    const int32_t camp = ReadCampId(role);
    if (friendlyFlag == 0 && camp != kCampFriend) {
        if (shouldLog) {
            LOGI("[Dzqs] nocd probe %s #%u not-friendly role=%p flag=%u camp=%d",
                 source, n, role, friendlyFlag, camp);
        }
        return;
    }

    void* list = ReadPtr(role, g_offRoleRtSkills);
    if (!SoftArgPtr(list)) {
        if (shouldLog) {
            LOGI("[Dzqs] nocd probe %s #%u missing-list role=%p flag=%u camp=%d list=%p",
                 source, n, role, friendlyFlag, camp, list);
        }
        return;
    }

    void* arr = ReadPtr(list, kOffListItems);
    const int32_t count = ReadI32(list, kOffListSize);
    if (!SoftArgPtr(arr) || count <= 0 || count > 256) {
        if (shouldLog) {
            LOGI("[Dzqs] nocd probe %s #%u bad-array role=%p list=%p arr=%p count=%d",
                 source, n, role, list, arr, count);
        }
        return;
    }

    uint32_t hit = 0;
    for (int32_t i = 0; i < count; i++) {
        void* skill = ReadPtr(arr, kOffArrayData + static_cast<size_t>(i) * 8);
        if (!SoftArgPtr(skill)) continue;
        void* baseData = ReadPtr(skill, g_offSkillBaseData);
        if (!SoftArgPtr(baseData) || ReadI32(baseData, kOffArrLen) < 0x52) continue;
        WriteI32(baseData, kOffArrZCd, 0);
        WriteU8(skill, kOffSkillManaOk, 1);
        hit++;
    }

    if (shouldLog) {
        LOGI("[Dzqs] nocd clear %s #%u role=%p list=%p count=%d hit=%u",
             source, n, role, list, count, hit);
    }
}

/**
 * isCdOK: role=*(skill+0x1b0), baseData=*(skill+0x88)。
 * 1.1.768 的 Game_Role._isFriendly 位于 +0x90；旧版 camp 链偏移已漂移。
 */
static bool PatchIsCdOK(void* skill) {
    if (!g_nocd.load() || !SoftArgPtr(skill)) return false;
    ResolveSkillFields(skill);
    void* role = ReadPtr(skill, g_offSkillRole);
    if (!IsFriendlyRole(role)) return false;
    void* baseData = ReadPtr(skill, g_offSkillBaseData);
    const int32_t len = SoftArgPtr(baseData) ? ReadI32(baseData, kOffArrLen) : -1;
    if (!SoftArgPtr(baseData) || len < 0x52) return false;
    const int32_t typeAtZCd = ReadI32(baseData, kOffArrZCd - 4);
    const float cdBefore = ReadF32(baseData, kOffArrZCd);
    WriteI32(baseData, kOffArrZCd, 0);
    WriteU8(skill, kOffSkillManaOk, 1);
    uint32_t n = g_hitsNocd.fetch_add(1) + 1;
    if (n <= 20 || (n % 1000) == 0) {
        LOGI("[Dzqs] isCdOK patch #%u skill=%p offRole=0x%zX offBase=0x%zX base=%p len=%d zType=%d cdBefore=%.3f",
             n, skill, g_offSkillRole, g_offSkillBaseData, baseData, len,
             typeAtZCd, cdBefore);
    }
    return true;
}

/** 斌哥 isManaOk：strb 1 @ skill+0x5c；友方同 isCdOK */
static bool PatchIsManaOk(void* skill) {
    if (!g_nocd.load() || !SoftArgPtr(skill)) return false;
    ResolveSkillFields(skill);
    void* role = ReadPtr(skill, g_offSkillRole);
    if (!IsFriendlyRole(role)) return false;
    WriteU8(skill, kOffSkillManaOk, 1);
    uint32_t n = g_hitsMana.fetch_add(1) + 1;
    if (n <= 5 || (n % 200) == 0) LOGI("[Dzqs] isManaOk patch #%u", n);
    return true;
}

static bool AnyFeatOn() { return g_kill.load() || g_inv.load() || g_nocd.load(); }

static void ObserveAndPatch(MethKind kind, void* a2) {
    switch (kind) {
    case MK_TOTAL_DMG: {
        void* target = ArgPtr(a2, kOffArgsTargetTd);
        int32_t camp = ReadCampId(target);
        DZQS_TRACE("calTotalDamage camp=%d kill=%d", camp, (int)g_kill.load());
#if DZQS_DMG_WRITE
        ClearRoleSkillCooldowns(ArgPtr(a2, 0), "calTotalDamage");
        /* 斌哥：camp==1 跳过；可读且非友才写 args+0x18 */
        if (g_kill.load() && SoftArgPtr(a2) && SoftArgPtr(target) && camp != kCampFriend &&
            camp != -1) {
            WriteF32(a2, kOffArgsExDmg, kKillDmgMild);
            uint32_t n = g_hitsKill.fetch_add(1) + 1;
            if (n <= 6 || (n % 50) == 0) LOGI("[Dzqs] calTotalDamage kill #%u camp=%d", n, camp);
        }
#endif
        break;
    }
    case MK_DMG_BUF: {
        void* target = ArgPtr(a2, kOffArgsTarget);
        int32_t camp = ReadCampId(target);
        DZQS_TRACE("calDamageBuffer camp=%d dmg=%.1f kill=%d inv=%d", camp,
                   SoftArgPtr(a2) ? ReadF32(a2, 0) : 0.f, (int)g_kill.load(),
                   (int)g_inv.load());
#if DZQS_DMG_WRITE
        ClearRoleSkillCooldowns(target, "calDamageBuffer");
        if (g_kill.load() && CampKillRange(camp) && SoftArgPtr(a2)) {
            WriteF32(a2, 0, kKillDmgHard);
            uint32_t n = g_hitsKill.fetch_add(1) + 1;
            if (n <= 6 || (n % 50) == 0) LOGI("[Dzqs] calDamageBuffer kill #%u camp=%d", n, camp);
        }
        if (g_inv.load() && camp == kCampFriend && SoftArgPtr(a2)) {
            WriteF32(a2, 0, 0.f);
            uint32_t n = g_hitsInv.fetch_add(1) + 1;
            if (n <= 6 || (n % 50) == 0) LOGI("[Dzqs] calDamageBuffer inv #%u", n);
        }
#endif
        break;
    }
    case MK_ADD_HP: {
#if DZQS_DMG_WRITE
        void* self = InstSelf(a2);
        ClearRoleSkillCooldowns(self, "AddRtHPTo");
        if (g_inv.load() && SoftArgPtr(self) && ReadCampId(self) == kCampFriend && SoftArgPtr(a2)) {
            WriteF32(a2, kOffHealArg8, kHealAmt);
            uint32_t n = g_hitsInv.fetch_add(1) + 1;
            if (n <= 6 || (n % 50) == 0) LOGI("[Dzqs] AddRtHPTo #%u", n);
        }
#endif
        break;
    }
    case MK_CD_OK:
        PatchIsCdOK(InstSelf(a2));
        break;
    case MK_MANA_OK:
        PatchIsManaOk(InstSelf(a2));
        break;
    default:
        break;
    }
}

/**
 * 共享跳板会跑 float 返回方法（calTotalDamage/calDamageBuffer）。
 * 必须 tail-call orig，避免 C 返回 void* 时打烂 s0 → 表现为物理伤害恒 0、DoT 仍可能掉血。
 */
static MethKind ClassifyForCall(void* mi) {
#if DZQS_TRACE_BUILD
    if (mi && g_methodGetName) return LookupKind(mi);
#else
    if (AnyFeatOn() && mi && g_methodGetName) return LookupKind(mi);
#endif
    return MK_NONE;
}

static void* Hooked_Trampoline(void* a0, void* a1, void* a2, void* a3) {
    if (!g_origTramp) return nullptr;
    MethKind kind = ClassifyForCall(a1);
    if (kind != MK_NONE) ObserveAndPatch(kind, a2);
#if defined(__clang__)
    [[clang::musttail]]
#endif
    return g_origTramp(a0, a1, a2, a3);
}

static bool EnsureSyms() {
    if (g_methodGetName) return true;
    void* h = dlopen(GameUnity::TARGET_LIB, RTLD_NOW);
    if (!h) return false;
    g_methodGetName = (method_get_name_t)dlsym(h, "il2cpp_method_get_name");
    g_classGetField = (class_get_field_from_name_t)dlsym(h, "il2cpp_class_get_field_from_name");
    g_fieldGetOffset = (field_get_offset_t)dlsym(h, "il2cpp_field_get_offset");
    return g_methodGetName != nullptr;
}

static bool InstallTrampoline() {
    if (g_hooked.load()) return true;
    if (!EnsureSyms()) {
        LOGE("[Dzqs] il2cpp_method_get_name missing");
        return false;
    }
    void* addr = FileOffToVa(GameUnity::TARGET_LIB, FILE_ENTER_INTERP);
    if (!addr) {
        LOGE("[Dzqs] resolve enter-interp failed file=0x%lX",
             (unsigned long)FILE_ENTER_INTERP);
        return false;
    }
    if (DobbyHook(addr, (void*)Hooked_Trampoline, (void**)&g_origTramp) != 0) {
        LOGE("[Dzqs] DobbyHook failed @ %p", addr);
        g_origTramp = nullptr;
        return false;
    }
    g_hooked.store(true);
    LOGI("[Dzqs] hooked enter-interp @ %p TRACE=%d DMG_WRITE=%d", addr,
         DZQS_TRACE_BUILD, DZQS_DMG_WRITE);
    DZQS_TRACE("hooked TRACE=%d DMG_WRITE=%d", DZQS_TRACE_BUILD, DZQS_DMG_WRITE);
    return true;
}

bool Dzqs_IsReady() { return g_ready.load(); }

bool Dzqs_TryInit() {
    if (g_ready.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;
#if DZQS_TRACE_BUILD
    if (!InstallTrampoline()) return false;
#else
    if (!AnyFeatOn()) {
        g_ready.store(true);
        LOGI("[Dzqs] ready (lazy trampoline)");
        return true;
    }
    if (!InstallTrampoline()) return false;
#endif
    g_ready.store(true);
    return true;
}

void Dzqs_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == DZQS_FEAT_KILL) g_kill.store(enabled);
    if (featNum == DZQS_FEAT_INV) g_inv.store(enabled);
    if (featNum == DZQS_FEAT_NO_CD) g_nocd.store(enabled);
    DZQS_TRACE("feat %d -> %d | kill=%d inv=%d nocd=%d", featNum, (int)enabled,
               (int)g_kill.load(), (int)g_inv.load(), (int)g_nocd.load());
    ClearMethCache();
    if ((AnyFeatOn() || DZQS_TRACE_BUILD) && isLibraryLoaded(GameUnity::TARGET_LIB)) {
        InstallTrampoline();
    }
}

void Dzqs_OnValueChanged(int /*featNum*/, int /*value*/) {}

void Dzqs_Tick(const bool* features) {
    if (!features) return;
    g_kill.store(features[DZQS_FEAT_KILL]);
    g_inv.store(features[DZQS_FEAT_INV]);
    g_nocd.store(features[DZQS_FEAT_NO_CD]);
    if (!g_ready.load()) Dzqs_TryInit();
    if ((AnyFeatOn() || DZQS_TRACE_BUILD) && !g_hooked.load() &&
        isLibraryLoaded(GameUnity::TARGET_LIB)) {
        InstallTrampoline();
    }
}

void Dzqs_ForceClear() {
    g_kill.store(false);
    g_inv.store(false);
    g_nocd.store(false);
    ClearMethCache();
}
