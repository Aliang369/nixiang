/* ================================================================
 *  TlgdTapTapMod.cpp - 弹力果冻 TapTap 2.4.0 (com.ginicat.sweet)
 *  4 功能：秒杀 / 全屏秒杀 / 无敌 / 无限技能
 *
 *  2026-09-12：
 *    - 伤害三功能按 4399 2026-08-24 定稿路线迁移到 TapTap 的 HybridCLR
 *      公共解释器命中点；内部解释执行不经过 MethodInfo.methodPointer。
 *    - Hook 内只按 a2 参数块形态识别 DamageHandle/DamageInfo，不做 MI/MP 过滤。
 *    - HpHandle 型只 patch 后调用 orig，不对主目标二次 invoke；
 *      DamageInfo 型在主调用后走一次 runtime_invoke(HpHandle)。
 *    - 全屏秒杀在主目标处理后，用官方 HpHandle 补发其它敌方 eid。
 *    - 无限技能保留已验证的 CanUseTimes/cdCounter 数据写入链，不改语义。
 *    - mod 放大/补发的伤害标 isFromNet，绕过 TotalDamageCounter/ToTleDamager；
 *      官方 HpHandle 的死亡链和 OnMonsterDead 击杀记账保持不变。
 *    - 移速不在功能范围内，菜单不注册，本文件也不保留移速代码。
 *
 *  证据基线：10_docs/弹力果冻/探针结论_TapTap2.4.0_20260911.md
 *  参考实现：Games/TlgdMod.cpp + 功能收尾定稿_20260820.md 的 2026-08-24 修复。
 * ================================================================ */
#include "TlgdTapTapMod.h"

#include <atomic>
#include <cstdint>
#include <cstring>
#include <cstdio>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

#define TLGDF     "[TLGDF]"
#define TLGD_TAG  "[Tlgd]"
#define TLGD_LIB  "libil2cpp.so"

// 4399 定稿函数入口为 0xBAAB0C；TapTap 2.4.0 对应函数前移 0x1000。
// 两包逐指令比对：26 条中 25 条完全一致，仅一条内部相对分支不同。
static constexpr uintptr_t kFileTrampoline = 0xBABB0C;
static constexpr int64_t kKillDmg = 99999999LL;
static constexpr float kKillAtk = 99999999.f;
static constexpr size_t kOffIsFromNet = 37;
static constexpr int32_t kTeamPlayer = 1;
static constexpr int32_t kTeamEnemy = 2;
static constexpr int32_t kMaxPlayerEid = 8;
static constexpr int32_t kMaxEnemyEid = 64;
static constexpr int32_t kMaxUnits = 512;
static constexpr int32_t kUnlimitedTimes = 9;
static constexpr float kCdReady = -0.01f;

// ---------------- feature state ----------------
static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_attached{false};
static std::atomic<bool> g_hooked{false};
static std::atomic<uint32_t> g_tick{0};
static std::atomic<bool> g_kill{false};
static std::atomic<bool> g_aoe{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_nocd{false};
static std::atomic<bool> g_aoeBusy{false};
static std::atomic<uint32_t> g_lastAoeTick{0};

// ---------------- evidence counters ----------------
static std::atomic<uint32_t> g_trampHits{0};
static std::atomic<uint32_t> g_handleShapes{0};
static std::atomic<uint32_t> g_infoShapes{0};
static std::atomic<uint32_t> g_killHits{0};
static std::atomic<uint32_t> g_invHits{0};
static std::atomic<uint32_t> g_aoeInvokes{0};
static std::atomic<uint32_t> g_aoeErrors{0};
static std::atomic<uint32_t> g_netMasked{0};
static std::atomic<uint32_t> g_skillWrites{0};
static std::atomic<uint32_t> g_seenUnits{0};
static std::atomic<uint32_t> g_seenPlayers{0};
static std::atomic<uint32_t> g_seenEnemies{0};

static std::atomic<int64_t> g_playerEids[kMaxPlayerEid]{};
static std::atomic<int32_t> g_playerN{0};
static std::atomic<int64_t> g_enemyEids[kMaxEnemyEid]{};
static std::atomic<int32_t> g_enemyN{0};
static std::atomic<int64_t> g_aoeKilledEids[kMaxEnemyEid]{};
static std::atomic<int32_t> g_aoeKilledN{0};

// ---------------- resolved types ----------------
static void* g_clsMgr = nullptr;
static void* g_clsEntity = nullptr;
static void* g_clsUnit = nullptr;
static void* g_clsSkill = nullptr;
static void* g_clsSkillDriver = nullptr;
static void* g_clsSkillContainer = nullptr;
static void* g_clsBattleServer = nullptr;
static void* g_miHpHandle = nullptr;
static void* g_mgr = nullptr;

static size_t offMgrUnits = 0;
static size_t offEntTeam = 0;
static size_t offEntEid = 0;
static size_t offUnitSkillDriver = 0;
static size_t offSkillCd = 0;
static size_t offSkillCanUseTimes = 0;
static size_t offSdRunner = 0;
static size_t offScCurSkill = 0;
static size_t offScSkills = 0;

// 4399 8-24 路线：HybridCLR 解释器公共命中点，ABI 不是标准函数契约。
// 不读 MethodInfo、不按方法名过滤；只从 a2 参数块扫描 DamageHandle/DamageInfo 形态。
using Trampoline_t = void* (*)(void*, void*, void*, void*);
static Trampoline_t g_origTramp = nullptr;
static thread_local int t_trampDepth = 0;

// DamageHandle layout from the TapTap 2.4.0 hotfix assembly:
// finalDamage@0, attackerID@8, raceValue@16, attachSkillType@20,
// beAttackerID@24, followed by bool flags.
struct DamageHandleBlob {
    int64_t finalDamage;
    int64_t attackerID;
    float raceValue;
    int32_t attachSkillType;
    int64_t beAttackerID;
    uint8_t isCritical;
    uint8_t isSkill;
    uint8_t isBuff;
    uint8_t isAntiDamage;
    uint8_t needForceNetProxyDamage;
    uint8_t isFromNet;
    uint8_t padding[2];
};
static_assert(sizeof(DamageHandleBlob) == 40, "DamageHandle blob layout mismatch");

static inline bool PtrOk(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

// 4399 实锤：0x71/0x2e 标签指针能过普通指针范围判断，但读字段会 SIGSEGV。
static inline bool SoftArgPtr(void* p) {
    if (!p) return false;
    uintptr_t u = (uintptr_t)p & 0x00FFFFFFFFFFFFFFULL;
    if (u & 7) return false;
    if (u < 0x1000000000ULL || u > 0x8000000000ULL) return false;
    const uintptr_t tag = u >> 40;
    return tag != 0x71 && tag != 0x2e;
}
static inline int32_t ReadI32(void* p, size_t o) { return Il2Cpp::ReadI32(p, o); }
static inline void WriteI32(void* p, size_t o, int32_t v) { Il2Cpp::WriteI32(p, o, v); }
static inline float ReadF32(void* p, size_t o) { return Il2Cpp::ReadF32(p, o); }
static inline void WriteF32(void* p, size_t o, float v) { Il2Cpp::WriteF32(p, o, v); }
static inline void* ReadPtr(void* p, size_t o) { return Il2Cpp::ReadPtr(p, o); }

static inline int64_t ReadI64(void* p, size_t o) {
    int64_t v = 0;
    memcpy(&v, (const void*)((uintptr_t)p + o), sizeof(v));
    return v;
}

static inline void WriteI64(void* p, size_t o, int64_t v) {
    memcpy((void*)((uintptr_t)p + o), &v, sizeof(v));
}

static inline void WriteU8(void* p, size_t o, uint8_t v) {
    *reinterpret_cast<uint8_t*>((uintptr_t)p + o) = v;
}

static inline int32_t ListSize(void* list) { return list ? ReadI32(list, 0x18) : 0; }
static inline void* ListItems(void* list) { return list ? ReadPtr(list, 0x10) : nullptr; }
static inline int32_t ArrayLength(void* array) { return array ? ReadI32(array, 0x18) : 0; }
static inline void* ArrayAt(void* array, int32_t i) {
    return ReadPtr(array, 0x20 + (size_t)i * 8);
}

static bool NeedDamagePatch() {
    return g_kill.load() || g_aoe.load() || g_inv.load();
}

static bool IsEntityId(int64_t id) {
    return id > 0 && id <= 4096;
}

static bool IsSameOrSubclass(void* klass, void* base) {
    if (!klass || !base) return false;
    if (klass == base) return true;
    auto& api = Il2Cpp::GetApi();
    if (!api.class_parent) return true;
    for (int i = 0; i < 32 && klass; i++) {
        klass = api.class_parent(klass);
        if (klass == base) return true;
    }
    return false;
}

static void* FindClassAnyImage(const char* shortName) {
    auto& api = Il2Cpp::GetApi();
    size_t count = 0;
    void** assemblies = api.Assemblies(&count);
    if (!assemblies || count == 0) return nullptr;
    for (size_t i = 0; i < count; i++) {
        void* image = api.ImageOf(assemblies[i]);
        if (!image) continue;
        void* klass = api.FindClassInImage(image, shortName);
        if (klass) return klass;
    }
    return nullptr;
}

static bool ValidateMgr(void* mgr) {
    if (!PtrOk(mgr) || !offMgrUnits) return false;
    void* list = ReadPtr(mgr, offMgrUnits);
    if (!PtrOk(list)) return false;

    const int32_t size = ListSize(list);
    void* items = ListItems(list);
    if (!PtrOk(items)) return false;

    const int32_t cap = ArrayLength(items);
    if (size < 0 || size > kMaxUnits || cap < size || cap > 4096) return false;
    if (size == 0) return true;

    const int32_t step = (size > 3) ? (size / 3) : 1;
    int checked = 0;
    for (int32_t i = 0; i < size && checked < 3; i += step, checked++) {
        void* unit = ArrayAt(items, i);
        if (!PtrOk(unit)) continue;
        const int32_t team = ReadI32(unit, offEntTeam);
        if (team >= 0 && team <= kTeamEnemy) return true;
    }
    return false;
}

static bool AcquireMgr() {
    auto& api = Il2Cpp::GetApi();
    if (!api.ready || !g_clsMgr) return false;
    void* mgr = api.StaticRefField(g_clsMgr, "_Stuff");
    if (!ValidateMgr(mgr)) return false;
    g_mgr = mgr;
    return true;
}

static bool IsCachedPlayerAtk(int64_t eid) {
    if (!IsEntityId(eid)) return false;
    const int32_t count = g_playerN.load();
    for (int32_t i = 0; i < count && i < kMaxPlayerEid; i++) {
        if (g_playerEids[i].load() == eid) return true;
    }
    return false;
}

static bool IsCachedEnemyVic(int64_t eid) {
    if (!IsEntityId(eid)) return false;
    const int32_t count = g_enemyN.load();
    for (int32_t i = 0; i < count && i < kMaxEnemyEid; i++) {
        if (g_enemyEids[i].load() == eid) return true;
    }
    return false;
}

static bool IsConfirmedPlayerStrike(int64_t atk, int64_t vic) {
    return atk != vic && IsCachedPlayerAtk(atk) && IsCachedEnemyVic(vic);
}

// 4399 8-24 定稿：敌弹道/技能实体常不在敌方单位表，所以无敌只要求
// vic 是玩家、atk 不是玩家；秒杀仍要求 atk/vic 都在缓存表。
static bool IsInvIncomingHit(int64_t atk, int64_t vic) {
    return IsEntityId(atk) && IsEntityId(vic) && atk != vic &&
           IsCachedPlayerAtk(vic) && !IsCachedPlayerAtk(atk);
}

static bool WantKillStrike() { return g_kill.load() || g_aoe.load(); }

static bool ValidateDamagePair(int64_t atk, int64_t vic) {
    if (!IsEntityId(atk) || !IsEntityId(vic) || atk == vic) return false;
    return IsConfirmedPlayerStrike(atk, vic) ||
           (g_inv.load() && IsInvIncomingHit(atk, vic));
}

// DamageHandle: finalDamage@0, attackerID@8, beAttackerID@24。
// finalDamage 允许 0，内部预演/护盾路径不能因 >=1 被漏掉。
static bool ValidateDamageHandleShape(void* h, int64_t* outDmg,
                                     int64_t* outAtk, int64_t* outVic) {
    if (!SoftArgPtr(h)) return false;
    const int64_t dmg = ReadI64(h, 0);
    const int64_t atk = ReadI64(h, 8);
    const int64_t vic = ReadI64(h, 24);
    if (dmg < 0 || dmg > kKillDmg) return false;
    if (!ValidateDamagePair(atk, vic)) return false;
    *outDmg = dmg;
    *outAtk = atk;
    *outVic = vic;
    return true;
}

// DamageInfo: attackerID@0, beAttackerID@8, atk(float)@16。
static bool ValidateDamageInfoShape(void* p, int64_t* outAtk, int64_t* outVic) {
    if (!SoftArgPtr(p)) return false;
    const int64_t atk = ReadI64(p, 0);
    const int64_t vic = ReadI64(p, 8);
    if (!ValidateDamagePair(atk, vic)) return false;
    const float atkVal = ReadF32(p, 16);
    if (atkVal < 0.f || atkVal > kKillAtk) return false;
    *outAtk = atk;
    *outVic = vic;
    return true;
}

static void* FindDamageHandleInArgs(void* a2, int64_t* outDmg,
                                    int64_t* outAtk, int64_t* outVic) {
    if (!SoftArgPtr(a2)) return nullptr;
    if (ValidateDamageHandleShape(a2, outDmg, outAtk, outVic)) return a2;
    void* p = reinterpret_cast<void*>(reinterpret_cast<uintptr_t>(a2) + 8);
    return ValidateDamageHandleShape(p, outDmg, outAtk, outVic) ? p : nullptr;
}

static void* FindDamageInfoInArgs(void* a2, int64_t* outAtk, int64_t* outVic) {
    if (!SoftArgPtr(a2)) return nullptr;
    if (ValidateDamageInfoShape(a2, outAtk, outVic)) return a2;
    void* p = reinterpret_cast<void*>(reinterpret_cast<uintptr_t>(a2) + 8);
    return ValidateDamageInfoShape(p, outAtk, outVic) ? p : nullptr;
}

static void ClearAoeKilled() {
    for (int32_t i = 0; i < kMaxEnemyEid; i++) g_aoeKilledEids[i].store(0);
    g_aoeKilledN.store(0);
}

static bool AlreadyAoeKilled(int64_t eid) {
    const int32_t count = g_aoeKilledN.load();
    for (int32_t i = 0; i < count && i < kMaxEnemyEid; i++) {
        if (g_aoeKilledEids[i].load() == eid) return true;
    }
    return false;
}

static void MarkAoeKilled(int64_t eid) {
    if (!IsEntityId(eid) || AlreadyAoeKilled(eid)) return;
    const int32_t count = g_aoeKilledN.load();
    if (count >= kMaxEnemyEid) return;
    g_aoeKilledEids[count].store(eid);
    g_aoeKilledN.store(count + 1);
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
        if (!strchr(perm, 'x') || end <= start) continue;
        if (fileOff >= off && fileOff < off + (end - start)) {
            found = reinterpret_cast<void*>(start + (fileOff - off));
            break;
        }
    }
    fclose(fp);
    return found;
}

static void* BattleServerStuff() {
    auto& api = Il2Cpp::GetApi();
    if (!g_clsBattleServer) return nullptr;
    void* server = api.StaticRefField(g_clsBattleServer, "_Stuff");
    return PtrOk(server) ? server : nullptr;
}

static bool InvokeHpHandle(long long atkId, long long vicId) {
    auto& api = Il2Cpp::GetApi();
    void* server = BattleServerStuff();
    if (!server || !g_miHpHandle || !api.runtime_invoke) {
        g_aoeErrors.fetch_add(1);
        return false;
    }

    DamageHandleBlob blob{};
    blob.finalDamage = kKillDmg;
    blob.attackerID = atkId;
    blob.beAttackerID = vicId;
    blob.isFromNet = 1;
    void* args[1] = {&blob};
    void* exception = nullptr;
    api.runtime_invoke(g_miHpHandle, server, args, &exception);
    if (exception) {
        g_aoeErrors.fetch_add(1);
        return false;
    }
    g_aoeInvokes.fetch_add(1);
    g_netMasked.fetch_add(1);
    return true;
}

static void AoeSweep(long long atkId, long long primaryVic) {
    if (!g_aoe.load() || g_aoeBusy.exchange(true)) return;
    if (!IsCachedPlayerAtk(atkId)) {
        g_aoeBusy.store(false);
        return;
    }

    const uint32_t tick = g_tick.load();
    const uint32_t last = g_lastAoeTick.load();
    if (tick < last + 2) {
        g_aoeBusy.store(false);
        return;
    }
    g_lastAoeTick.store(tick);

    const int32_t count = g_enemyN.load();
    for (int32_t i = 0; i < count && i < kMaxEnemyEid; i++) {
        const int64_t eid = g_enemyEids[i].load();
        if (!IsEntityId(eid) || eid == primaryVic || AlreadyAoeKilled(eid)) continue;
        if (InvokeHpHandle(atkId, eid)) MarkAoeKilled(eid);
    }
    g_aoeBusy.store(false);
}

static void* Hooked_Trampoline(void* a0, void* a1, void* a2, void* a3) {
    if (!g_origTramp) return nullptr;
    if (t_trampDepth > 0 || !NeedDamagePatch()) return g_origTramp(a0, a1, a2, a3);

    g_trampHits.fetch_add(1);
    int64_t dmg = 0, atkId = 0, vicId = 0;
    void* handle = FindDamageHandleInArgs(a2, &dmg, &atkId, &vicId);
    void* info = nullptr;
    if (handle) {
        g_handleShapes.fetch_add(1);
    } else {
        info = FindDamageInfoInArgs(a2, &atkId, &vicId);
        if (!info) return g_origTramp(a0, a1, a2, a3);
        g_infoShapes.fetch_add(1);
    }

    t_trampDepth++;
    if (g_inv.load() && IsInvIncomingHit(atkId, vicId)) {
        if (handle) WriteI64(handle, 24, 0);
        else WriteI64(info, 8, 0);
        g_invHits.fetch_add(1);
        void* ret = g_origTramp(a0, a1, a2, a3);
        t_trampDepth--;
        return ret;
    }

    const bool killStrike = WantKillStrike() && IsConfirmedPlayerStrike(atkId, vicId);
    if (killStrike && handle) {
        WriteI64(handle, 0, kKillDmg);
        WriteU8(handle, kOffIsFromNet, 1);
        g_netMasked.fetch_add(1);
    }

    void* ret = g_origTramp(a0, a1, a2, a3);

    if (killStrike) {
        // DamageInfo 只描述输入伤害，主目标必须在 orig 后补一次正式 HpHandle。
        if (!handle) InvokeHpHandle(atkId, vicId);
        g_killHits.fetch_add(1);
        if (g_aoe.load()) {
            MarkAoeKilled(vicId);
            AoeSweep(atkId, vicId);
        }
    }
    t_trampDepth--;
    return ret;
}

static bool InstallTrampolineHook() {
    if (g_hooked.load()) return true;
    void* tramp = FileOffToVa(TLGD_LIB, kFileTrampoline);
    if (!tramp) {
        LOGW(TLGD_TAG " 解释器公共点解析失败 file=0x%llx", (unsigned long long)kFileTrampoline);
        return false;
    }
    if (DobbyHook(tramp, (void*)Hooked_Trampoline, (void**)&g_origTramp) != 0) {
        LOGW(TLGD_TAG " DobbyHook 解释器公共点失败 @%p", tramp);
        return false;
    }
    g_hooked.store(true);
    LOGI(TLGD_TAG " 解释器公共点已挂：file=0x%llx va=%p orig=%p " TLGDF "",
         (unsigned long long)kFileTrampoline, tramp, g_origTramp);
    return true;
}

// 无限技能：保留已验收的数据写入链，只改 Skill.CanUseTimes/cdCounter。
static void ApplyUnlimitedSkill(void* unit) {
    if (!offUnitSkillDriver || !offSdRunner || !g_clsSkill) return;
    void* driver = ReadPtr(unit, offUnitSkillDriver);
    if (!PtrOk(driver)) return;
    void* container = ReadPtr(driver, offSdRunner);
    if (!PtrOk(container)) return;

    int touched = 0;
    if (offScSkills) {
        void* dict = ReadPtr(container, offScSkills);
        if (PtrOk(dict)) {
            void* entries = ReadPtr(dict, 0x18);
            const int32_t count = ReadI32(dict, 0x20);
            const int32_t cap = ArrayLength(entries);
            if (PtrOk(entries) && count > 0 && count <= 64 && cap >= count) {
                for (int32_t i = 0; i < count; i++) {
                    void* entry = (void*)((uintptr_t)entries + 0x20 + (size_t)i * 24);
                    void* skill = ReadPtr(entry, 0x10);
                    if (!PtrOk(skill) || !IsSameOrSubclass(ReadPtr(skill, 0), g_clsSkill)) continue;
                    WriteI32(skill, offSkillCanUseTimes, kUnlimitedTimes);
                    WriteF32(skill, offSkillCd, kCdReady);
                    touched++;
                }
            }
        }
    }

    if (offScCurSkill) {
        void* skill = ReadPtr(container, offScCurSkill);
        if (PtrOk(skill) && IsSameOrSubclass(ReadPtr(skill, 0), g_clsSkill)) {
            WriteI32(skill, offSkillCanUseTimes, kUnlimitedTimes);
            WriteF32(skill, offSkillCd, kCdReady);
            touched++;
        }
    }
    if (touched > 0) g_skillWrites.fetch_add((uint32_t)touched);
}

static void RefreshBattleCache() {
    // 4399 路线：每轮都跟当前 BattleEntityManager._Stuff。
    // 跨局/跨章节后游戏会换管理器实例；旧实例仍可能通过 ValidateMgr，
    // 但其 battleUnits 已回收成 team=0，若缓存旧指针会导致三功能永久停摆。
    if (!AcquireMgr()) {
        g_mgr = nullptr;
        g_playerN.store(0);
        g_enemyN.store(0);
        ClearAoeKilled();
        return;
    }

    void* list = ReadPtr(g_mgr, offMgrUnits);
    if (!PtrOk(list)) {
        g_mgr = nullptr;
        g_playerN.store(0);
        g_enemyN.store(0);
        ClearAoeKilled();
        return;
    }

    void* items = ListItems(list);
    int32_t size = ListSize(list);
    const int32_t cap = ArrayLength(items);
    if (!PtrOk(items) || size < 0 || cap < size) {
        g_playerN.store(0);
        g_enemyN.store(0);
        ClearAoeKilled();
        return;
    }
    if (size > kMaxUnits) size = kMaxUnits;

    int32_t playerCount = 0;
    int32_t enemyCount = 0;
    for (int32_t i = 0; i < size; i++) {
        void* unit = ArrayAt(items, i);
        if (!PtrOk(unit)) continue;
        const int32_t team = ReadI32(unit, offEntTeam);
        if (team == kTeamPlayer) {
            const int64_t eid = offEntEid ? ReadI64(unit, offEntEid) : 0;
            if (IsEntityId(eid) && playerCount < kMaxPlayerEid) {
                g_playerEids[playerCount++].store(eid);
            }
            if (g_nocd.load()) ApplyUnlimitedSkill(unit);
        } else if (team == kTeamEnemy) {
            const int64_t eid = offEntEid ? ReadI64(unit, offEntEid) : 0;
            if (IsEntityId(eid) && enemyCount < kMaxEnemyEid) {
                g_enemyEids[enemyCount++].store(eid);
            }
        }
    }

    g_playerN.store(playerCount);
    g_enemyN.store(enemyCount);
    g_seenUnits.store((uint32_t)size);
    g_seenPlayers.store((uint32_t)playerCount);
    g_seenEnemies.store((uint32_t)enemyCount);
    if (enemyCount == 0) ClearAoeKilled();

    static std::atomic<int32_t> s_lastPlayers{-1};
    static std::atomic<int32_t> s_lastEnemies{-1};
    if (playerCount != s_lastPlayers.load() || enemyCount != s_lastEnemies.load()) {
        s_lastPlayers.store(playerCount);
        s_lastEnemies.store(enemyCount);
        LOGI(TLGD_TAG " 战场缓存：units=%d 我方=%d 敌方=%d p0=%lld e0=%lld",
             size, playerCount, enemyCount,
             (long long)(playerCount > 0 ? g_playerEids[0].load() : 0),
             (long long)(enemyCount > 0 ? g_enemyEids[0].load() : 0));
    }
}

static bool Resolve() {
    auto& api = Il2Cpp::GetApi();
    if (!api.ready && !api.Load(TLGD_LIB)) return false;

    // domain != null 时 Unity/IL2CPP 尚未完全就绪；程序集列表非空才是安全 attach 信号。
    size_t assemblyCount = 0;
    void** assemblies = api.Assemblies(&assemblyCount);
    if (!assemblies || assemblyCount == 0) return false;

    if (!g_attached.load() && api.thread_attach && api.domain_get) {
        void* domain = api.domain_get();
        if (!domain) return false;
        api.thread_attach(domain);
        g_attached.store(true);
    }

    g_clsMgr            = FindClassAnyImage("BattleEntityManager");
    g_clsEntity         = FindClassAnyImage("BattleEntity");
    g_clsUnit           = FindClassAnyImage("BattleUnit");
    g_clsSkill          = FindClassAnyImage("Skill");
    g_clsSkillDriver    = FindClassAnyImage("SkillDriver");
    g_clsSkillContainer = FindClassAnyImage("SkillContainer");
    g_clsBattleServer   = FindClassAnyImage("BattleServer");

    if (!g_clsMgr || !g_clsEntity || !g_clsUnit ||
        !g_clsSkill || !g_clsSkillDriver || !g_clsSkillContainer || !g_clsBattleServer) {
        LOGW(TLGD_TAG " 关键热更类未就绪 mgr=%p entity=%p unit=%p skill=%p driver=%p container=%p server=%p",
             g_clsMgr, g_clsEntity, g_clsUnit, g_clsSkill,
             g_clsSkillDriver, g_clsSkillContainer, g_clsBattleServer);
        return false;
    }

    offMgrUnits        = api.FieldOffset(g_clsMgr, "battleUnits");
    offEntTeam         = api.FieldOffset(g_clsEntity, "team");
    offEntEid          = api.FieldOffset(g_clsEntity, "EntityID");
    offUnitSkillDriver = api.FieldOffset(g_clsUnit, "_skillDriver");
    offSkillCd         = api.FieldOffset(g_clsSkill, "cdCounter");
    offSkillCanUseTimes = api.FieldOffset(g_clsSkill, "CanUseTimes");
    offSdRunner        = api.FieldOffset(g_clsSkillDriver, "runner");
    offScCurSkill      = api.FieldOffset(g_clsSkillContainer, "curSkill");
    offScSkills        = api.FieldOffset(g_clsSkillContainer, "_skills");

    g_miHpHandle = api.GetMethod(g_clsBattleServer, "HpHandle", 1);

    if (!offMgrUnits || !offEntTeam || !offEntEid ||
        !offUnitSkillDriver || !offSkillCd || !offSkillCanUseTimes ||
        !offSdRunner || !offScCurSkill || !offScSkills || !g_miHpHandle) {
        LOGW(TLGD_TAG " 关键字段/方法缺失 units=0x%zx team=0x%zx eid=0x%zx "
                      "skills(unit=0x%zx driver=0x%zx cur=0x%zx dict=0x%zx cd=0x%zx use=%zu) mi=%p",
             offMgrUnits, offEntTeam, offEntEid,
             offUnitSkillDriver, offSdRunner, offScCurSkill, offScSkills,
             offSkillCd, offSkillCanUseTimes, g_miHpHandle);
        return false;
    }

    LOGI(TLGD_TAG " 解析完成：units=0x%zx team=0x%zx eid=0x%zx "
                      "skill(unit=0x%zx driver=0x%zx cur=0x%zx dict=0x%zx cd=0x%zx use=%zu) "
                      "HpHandle(mi=%p)",
             offMgrUnits, offEntTeam, offEntEid,
             offUnitSkillDriver, offSdRunner, offScCurSkill, offScSkills,
             offSkillCd, offSkillCanUseTimes, g_miHpHandle);
    return true;
}

static void LogStat() {
    LOGI(TLGD_TAG " 状态 秒杀=%d 全屏=%d 无敌=%d 无限技能=%d hook=%d | "
                  "tramp=%u handle=%u info=%u kill=%u inv=%u aoe=%u net=%u err=%u | "
                  "技能写=%u | 缓存 我方=%d 敌方=%d 去重=%d | units=%u",
         g_kill.load() ? 1 : 0, g_aoe.load() ? 1 : 0,
         g_inv.load() ? 1 : 0, g_nocd.load() ? 1 : 0, g_hooked.load() ? 1 : 0,
         g_trampHits.load(), g_handleShapes.load(), g_infoShapes.load(),
         g_killHits.load(), g_invHits.load(), g_aoeInvokes.load(),
         g_netMasked.load(), g_aoeErrors.load(),
         g_skillWrites.load(), g_playerN.load(), g_enemyN.load(),
         g_aoeKilledN.load(), g_seenUnits.load());
}

bool TlgdTapTap_TryInit(JavaVM* /*vm*/) {
    if (g_ready.load()) return true;
    if (!isLibraryLoaded(TLGD_LIB)) return false;
    if (!Resolve()) return false;
    g_ready.store(true);
    LOGI(TLGD_TAG " 就绪：解释器公共点（秒杀/全屏秒杀/无敌）+ 原无限技能数据链");
    return true;
}

bool TlgdTapTap_IsReady() {
    return g_ready.load();
}

void TlgdTapTap_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case TLGDTT_FEAT_KILL: g_kill.store(enabled); break;
        case TLGDTT_FEAT_AOE:  g_aoe.store(enabled); break;
        case TLGDTT_FEAT_INV:  g_inv.store(enabled); break;
        case TLGDTT_FEAT_NOCD: g_nocd.store(enabled); break;
        default: break;
    }
    LOGI(TLGD_TAG " 功能 %d -> %s", featNum, enabled ? "开" : "关");
}

void TlgdTapTap_OnValueChanged(int featNum, int value) {
    (void)featNum;
    (void)value;
}

void TlgdTapTap_Tick(const bool* features, const int* values) {
    if (!g_ready.load() && !TlgdTapTap_TryInit(nullptr)) return;

    if (features) {
        g_kill.store(features[TLGDTT_FEAT_KILL]);
        g_aoe.store(features[TLGDTT_FEAT_AOE]);
        g_inv.store(features[TLGDTT_FEAT_INV]);
        g_nocd.store(features[TLGDTT_FEAT_NOCD]);
    }
    (void)values;

    const uint32_t tick = g_tick.fetch_add(1) + 1;
    if (NeedDamagePatch()) InstallTrampolineHook();

    if (!g_kill.load() && !g_aoe.load() && !g_inv.load() && !g_nocd.load()) {
        g_mgr = nullptr;
        g_playerN.store(0);
        g_enemyN.store(0);
        ClearAoeKilled();
        if (tick <= 3 || (tick % 25) == 0) LogStat();
        return;
    }

    RefreshBattleCache();
    if (tick <= 3 || (tick % 25) == 0) LogStat();
}

void TlgdTapTap_ForceClear() {
    g_kill.store(false);
    g_aoe.store(false);
    g_inv.store(false);
    g_nocd.store(false);
    g_mgr = nullptr;
    g_playerN.store(0);
    g_enemyN.store(0);
    ClearAoeKilled();
    LOGW(TLGD_TAG " ForceClear：功能全关");
}
