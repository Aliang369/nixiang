/* ================================================================
 *  独奏骑士 1.1.760 — HybridCLR 公共跳板 + 方法名过滤
 *
 *  挂点：libil2cpp RVA 0x20B840C（斌哥 memdiff）
 *  偏移来源：06_dumped/独奏骑士/replacement_disasm.txt（斌哥壳实锤，非猜）
 *
 *  isCdOK（disasm 0x61c84..0x61cd8）：
 *    role = *(skill+0x1a8); camp = *(*role+0x80)+0xa8
 *    baseData = *(skill+0x88); 若 [baseData+0x18]>=0x52 → [baseData+0x2ac]=0
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
    FILE_TRAMPOLINE = 0x20B440C,
    ELF_RX_BIAS     = 0x4000,
    RVA_TRAMPOLINE  = FILE_TRAMPOLINE + ELF_RX_BIAS,
};

/* —— 全部来自 replacement_disasm.txt —— */
enum : size_t {
    kOffArgsTarget   = 0x10,  // calDamageBuffer: ldr x24,[x19,#0x10]
    kOffArgsTargetTd = 0x08,  // calTotalDamage:  ldr x24,[x19,#8]
    kOffArgsExDmg    = 0x18,  // calTotalDamage:  str s0,[x19,#0x18]
    kOffCampPtr      = 0x80,
    kOffCampId       = 0xA8,
    kOffSkillRole    = 0x1A8, // isCdOK: ldr x24,[x23,#0x1a8]
    kOffSkillBaseData= 0x88,  // isCdOK: ldr x23,[x23,#0x88] → 再写 +0x2ac
    kOffArrLen       = 0x18,  // max_length；斌哥 cmp >= 0x52
    kOffArrZCd       = 0x2AC, // str wzr,[baseData,#0x2ac]
    kOffSkillManaOk  = 0x5C,  // isManaOk: strb 1,[skill,#0x5c]
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

typedef void* (*Trampoline_t)(void* a0, void* a1, void* a2, void* a3);
typedef const char* (*method_get_name_t)(void* method);

static Trampoline_t g_origTramp = nullptr;
static method_get_name_t g_methodGetName = nullptr;

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
    uintptr_t u = (uintptr_t)p & 0x00FFFFFFFFFFFFFFULL;
    if (u & 7) return false;
    return u >= 0x1000000000ULL && u <= 0x8000000000ULL;
}

static bool SoftHeapPtr(void* p) {
    if (!SoftPtr(p)) return false;
    uintptr_t u = (uintptr_t)p & 0x00FFFFFFFFFFFFFFULL;
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

/**
 * 斌哥 isCdOK 实锤路径（replacement_disasm 0x61c84..0x61cd8）：
 *   role=*(skill+0x1a8) camp==1
 *   baseData=*(skill+0x88); len=*(i32*)(baseData+0x18)>=0x52
 *   *(i32*)(baseData+0x2ac)=0
 */
static bool PatchIsCdOK(void* skill) {
    if (!g_nocd.load() || !SoftArgPtr(skill)) return false;
    void* role = ReadPtr(skill, kOffSkillRole);
    if (ReadCampId(role) != kCampFriend) return false;
    void* baseData = ReadPtr(skill, kOffSkillBaseData);
    if (!SoftArgPtr(baseData)) return false;
    if (ReadI32(baseData, kOffArrLen) < 0x52) return false;
    WriteI32(baseData, kOffArrZCd, 0);
    uint32_t n = g_hitsNocd.fetch_add(1) + 1;
    if (n <= 5 || (n % 200) == 0) LOGI("[Dzqs] isCdOK patch #%u", n);
    return true;
}

/** 斌哥 isManaOk：strb 1 @ skill+0x5c；友方同 isCdOK */
static bool PatchIsManaOk(void* skill) {
    if (!g_nocd.load() || !SoftArgPtr(skill)) return false;
    void* role = ReadPtr(skill, kOffSkillRole);
    if (ReadCampId(role) != kCampFriend) return false;
    WriteU8(skill, kOffSkillManaOk, 1);
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
static void PrePatch(void* /*a0*/, void* a1, void* a2, void* /*a3*/) {
#if DZQS_TRACE_BUILD
    const bool run = a1 && g_methodGetName;
#else
    const bool run = AnyFeatOn() && a1 && g_methodGetName;
#endif
    if (!run) return;
    MethKind k = LookupKind(a1);
    if (k != MK_NONE) ObserveAndPatch(k, a2);
}

static void* Hooked_Trampoline(void* a0, void* a1, void* a2, void* a3) {
    PrePatch(a0, a1, a2, a3);
    if (!g_origTramp) return nullptr;
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
    return g_methodGetName != nullptr;
}

static bool InstallTrampoline() {
    if (g_hooked.load()) return true;
    if (!EnsureSyms()) {
        LOGE("[Dzqs] il2cpp_method_get_name missing");
        return false;
    }
    char buf[32];
    snprintf(buf, sizeof(buf), "0x%lX", (unsigned long)RVA_TRAMPOLINE);
    void* addr = getAbsoluteAddress(GameUnity::TARGET_LIB, buf);
    if (!addr) {
        LOGE("[Dzqs] resolve trampoline failed %s", buf);
        return false;
    }
    if (DobbyHook(addr, (void*)Hooked_Trampoline, (void**)&g_origTramp) != 0) {
        LOGE("[Dzqs] DobbyHook failed @ %p", addr);
        g_origTramp = nullptr;
        return false;
    }
    g_hooked.store(true);
    LOGI("[Dzqs] hooked trampoline @ %p TRACE=%d DMG_WRITE=%d", addr,
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
