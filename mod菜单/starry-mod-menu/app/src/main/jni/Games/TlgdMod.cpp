/* ================================================================
 *  弹力果冻 2.4.0 — HybridCLR 跳板 0xBAAB0C（斌哥 memdiff 实锤）
 *
 *  规范对齐（2026-08-20）：
 *    - 热路径禁 SafeProbe；SoftHeapPtr/SoftArgPtr + 直接读写
 *    - eid→阵营只查 RefreshBattleCache 列表，禁热路径 FindEntityByID
 *    - 跳板 lazy install（开战斗/nocd 功能才挂）
 *    - nocd：跳板 CanUse/ReCal 为主；Tick 仅低频 unit 链兜底，禁 ScanInstances
 *
 *  崩溃实锤（禁止再猜）：
 *    - method_get_name(MI*) 全空 / 毒指针 0x100000001
 *    - a2=0x710000000* 过 SoftPtr 但不可读 → ValidateDamageHandle SIGSEGV
 *  功能实锤：
 *    - 禁 MI 名/MP 过滤 HpHandle；用 a2 DamageHandle 形态
 *    - 无敌：beAttackerID→0 再 orig
 *    - 无限技能：跳板 CanUse（MI 或独占 MP）+ ReCal（仅 MI）
 *
 *  秒杀/全屏修复（2026-08-24）：
 *    - 形态识别只验 atk/vic 字段，禁 finalDamage>=1（0 伤预演会漏判）
 *    - 全屏秒杀独立生效：handle patch 与 sweep 不再依赖 g_kill
 *    - AlreadyAoeKilled 仅全屏 sweep 去重；秒杀 alone 禁止误标阻断
 *    - 未命中 eid 缓存时 hook 内单次 RefreshBattleCache 再判
 * ================================================================ */

#include "TlgdMod.h"

#include <android/log.h>
#include <atomic>
#include <cstdint>
#include <cstdio>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/StarPtr.h"   // 设备无关指针判据（清 TBI/MTE 顶字节）
#include "GameConfig.h"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define TLGD_TRACE(...) \
    __android_log_print(ANDROID_LOG_INFO, "StarryTrace", "[Tlgd] " __VA_ARGS__)

/* 正式包热路径关 Trace；调试时改 1 重编 so */
#if 1
#undef TLGD_TRACE
#define TLGD_TRACE(...) ((void)0)
#endif

enum : uintptr_t { FILE_TRAMPOLINE = 0xBAAB0C };

enum : size_t {
    kOffFinalDmg    = 0,
    kOffAttackerId  = 8,
    kOffBeAttacker  = 24,
    kOffDmgInfoAtk  = 16,
    kOffListItems   = 0x10,
    kOffListSize    = 0x18,
    kOffArrayData   = 0x20,
    kScCurSkillFb   = 0x18,
    kScSkillsDictFb = 0x20,
};
enum : int32_t { TEAM_NEUTRAL = 0, TEAM_PLAYER = 1, TEAM_ENEMY = 2 };
enum : int64_t { kKillDmg = 99999999LL };
static constexpr float kKillAtk = 99999999.f;
static constexpr int kMaxPlayerEid = 8;
static constexpr int kMaxEnemyEid = 64;

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_hooked{false};
static std::atomic<bool> g_kill{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_aoe{false};
static std::atomic<bool> g_nocd{false};
static std::atomic<uint32_t> g_hitsKill{0};
static std::atomic<uint32_t> g_hitsInv{0};
static std::atomic<uint32_t> g_hitsHandle{0};
static std::atomic<uint32_t> g_tick{0};

static size_t g_offTeam = 0;
static size_t g_offEntityId = 0;
static std::atomic<uint32_t> g_hitsDmgInfo{0};
static std::atomic<uint32_t> g_hitsCanUse{0};
static std::atomic<uint32_t> g_hitsReCal{0};
static std::atomic<uint32_t> g_hitsSkillPatch{0};

static size_t g_offCdCounter = 0;
static size_t g_offCanUseTimes = 0;
static size_t g_offSkillTimeCounter = 0;
static size_t g_offSkillUnit = 0;
static size_t g_offUnitSkillDriver = 0;
static size_t g_offSdRunner = 0;
static size_t g_offRunnerCurSkill = 0;
static size_t g_offRunnerSkills = 0;

static void* g_miCanUse = nullptr;
static void* g_miReCalSkillCD = nullptr;
static void* g_mpCanUse = nullptr;

static void* g_clsMgr = nullptr;
static void* g_clsSkill = nullptr;
static void* g_miGetMgrStuff = nullptr;
static void* g_miGetSrvStuff = nullptr;
static void* g_miFindTeam = nullptr;
static void* g_miHpHandle = nullptr;
static void* g_miGetAllEnt = nullptr;

typedef void* (*Trampoline_t)(void* a0, void* a1, void* a2, void* a3);
typedef void* (*GetStuff_t)(void* method);
typedef void* (*FindTeam_t)(void* self, int32_t team, bool dead, void* method);

static Trampoline_t g_origTramp = nullptr;
static GetStuff_t g_getMgrStuff = nullptr;
static GetStuff_t g_getSrvStuff = nullptr;
static FindTeam_t g_findTeam = nullptr;

static std::atomic<int64_t> g_playerEids[kMaxPlayerEid]{};
static std::atomic<int32_t> g_playerN{0};
static std::atomic<int64_t> g_enemyEids[kMaxEnemyEid]{};
static std::atomic<int32_t> g_enemyN{0};
static std::atomic<int64_t> g_aoeKilledEids[kMaxEnemyEid]{};
static std::atomic<int32_t> g_aoeKilledN{0};
static std::atomic<uint32_t> g_lastAoeTick{0};
static std::atomic<bool> g_inMgrCall{false};
static std::atomic<bool> g_aoeBusy{false};

static thread_local int t_trampDepth = 0;

struct DamageHandleBlob {
    int64_t finalDamage;
    int64_t attackerID;
    float raceValue;
    int32_t attachSkillType;
    int64_t beAttackerID;
    uint8_t flags[16];
};

static bool SoftPtr(void* p) {
    if (!p) return false;
    // ★设备无关修正：上界原为 0x8000000000（512GiB），真机 Scudo 堆清标签后可达 0x76…/0x78…，
    //   越界即整片判否、只在真机暴露 ⇒ 改用共享区间 4GiB~128TiB。
    const uintptr_t u = starptr::Untag((uintptr_t)p);
    if (u & 7) return false;
    return u >= starptr::kUserVaMin && u < starptr::kUserVaMax;
}

/** 拒 HybridCLR 标签指针（实锤 0x71… / 0x2e… 过 SoftPtr 但读字段 SIGSEGV） */
static bool SoftHeapPtr(void* p) {
    if (!SoftPtr(p)) return false;
    uintptr_t u = starptr::Untag((uintptr_t)p);
    const uintptr_t tag = u >> 40;
    return tag != 0x71 && tag != 0x2e;
}

static bool SoftArgPtr(void* p) { return SoftHeapPtr(p); }

static bool NeedDmgPatch() { return g_kill.load() || g_inv.load() || g_aoe.load(); }

static bool AnyGameplayOn() { return NeedDmgPatch() || g_nocd.load(); }

static bool NeedTrampHook() { return AnyGameplayOn(); }

static bool ValidTeam(int32_t t) { return t >= TEAM_NEUTRAL && t <= TEAM_ENEMY; }

static bool TeamsFight(int32_t ta, int32_t tv) {
    if (!ValidTeam(ta) || !ValidTeam(tv) || ta == tv) return false;
    if (ta == TEAM_NEUTRAL && tv == TEAM_NEUTRAL) return false;
    return true;
}

static int64_t ReadI64Arg(void* obj, size_t off) {
    if (!SoftArgPtr(obj)) return 0;
    return *reinterpret_cast<int64_t*>(reinterpret_cast<uintptr_t>(obj) + off);
}

static int64_t ReadI64Heap(void* obj, size_t off) {
    if (!SoftHeapPtr(obj)) return 0;
    return *reinterpret_cast<int64_t*>(reinterpret_cast<uintptr_t>(obj) + off);
}

static int32_t ReadI32Heap(void* obj, size_t off) {
    if (!SoftHeapPtr(obj)) return -1;
    return Il2Cpp::ReadI32(obj, off);
}

static void* ReadPtrHeap(void* obj, size_t off) {
    if (!SoftHeapPtr(obj)) return nullptr;
    return Il2Cpp::ReadPtr(obj, off);
}

static int32_t ReadI32Arg(void* obj, size_t off) {
    if (!SoftArgPtr(obj)) return -1;
    return Il2Cpp::ReadI32(obj, off);
}

static void* ReadPtrArg(void* obj, size_t off) {
    if (!SoftArgPtr(obj)) return nullptr;
    return Il2Cpp::ReadPtr(obj, off);
}

static bool IsEntityId(int64_t id) {
    // 实锤合法 EntityID 为个位数~几十（1/4/12…）；拒 1e9 级指针误读
    return id > 0 && id <= 4096;
}

static bool IsPlausibleDamage(int64_t dmg) { return dmg >= 1 && dmg <= kKillDmg; }

static void RefreshBattleCache();

static void WriteF32Heap(void* obj, size_t off, float v) {
    if (SoftHeapPtr(obj)) Il2Cpp::WriteF32(obj, off, v);
}

static void WriteI32Heap(void* obj, size_t off, int32_t v) {
    if (SoftHeapPtr(obj)) Il2Cpp::WriteI32(obj, off, v);
}

static void WriteI64Arg(void* obj, size_t off, int64_t v) {
    if (SoftArgPtr(obj))
        *reinterpret_cast<int64_t*>(reinterpret_cast<uintptr_t>(obj) + off) = v;
}

static void* GetBattleMgr() {
    if (!g_getMgrStuff || !g_miGetMgrStuff) return nullptr;
    void* mgr = g_getMgrStuff(g_miGetMgrStuff);
    if (SoftHeapPtr(mgr)) return mgr;
    if (g_clsMgr) {
        void* st = GetApi().StaticRefField(g_clsMgr, "_Stuff");
        if (SoftHeapPtr(st)) return st;
    }
    return nullptr;
}

static void* GetBattleServer() {
    if (!g_getSrvStuff || !g_miGetSrvStuff) return nullptr;
    void* srv = g_getSrvStuff(g_miGetSrvStuff);
    return SoftHeapPtr(srv) ? srv : nullptr;
}

/** 阵营只查 Tick 刷新的 eid 列表，禁止热路径 FindEntityByID */
static int32_t CachedTeamOf(int64_t eid) {
    if (!IsEntityId(eid)) return -1;
    int32_t n = g_playerN.load();
    for (int i = 0; i < n && i < kMaxPlayerEid; i++) {
        if (g_playerEids[i].load() == eid) return TEAM_PLAYER;
    }
    n = g_enemyN.load();
    for (int i = 0; i < n && i < kMaxEnemyEid; i++) {
        if (g_enemyEids[i].load() == eid) return TEAM_ENEMY;
    }
    return -1;
}

static bool IsPlayerTeamId(int64_t id) { return CachedTeamOf(id) == TEAM_PLAYER; }

static bool IsEnemyTeamId(int64_t id) { return CachedTeamOf(id) == TEAM_ENEMY; }

static bool IsCachedPlayerAtk(int64_t eid) {
    if (!IsEntityId(eid)) return false;
    int32_t n = g_playerN.load();
    for (int i = 0; i < n && i < kMaxPlayerEid; i++) {
        if (g_playerEids[i].load() == eid) return true;
    }
    return false;
}

static bool IsCachedEnemyVic(int64_t eid) {
    if (!IsEntityId(eid)) return false;
    int32_t n = g_enemyN.load();
    for (int i = 0; i < n && i < kMaxEnemyEid; i++) {
        if (g_enemyEids[i].load() == eid) return true;
    }
    return false;
}

/** 秒杀/全屏：必须 atk、vic 都在 Tick 刷新的 eid 表（拒进图内部杂散 id 误触） */
static bool IsConfirmedPlayerStrike(int64_t atk, int64_t vic) {
    return IsCachedPlayerAtk(atk) && IsCachedEnemyVic(vic);
}

static bool WantKillStrike() { return g_kill.load() || g_aoe.load(); }

/**
 * 无敌：vic 必须在玩家表；atk 只要不是玩家即可。
 * 实锤敌弹道/技能实体常不在 FindBattleUnitByTeam 的 enemy 表 → 禁要求 IsCachedEnemyVic(atk)。
 */
static bool IsInvIncomingHit(int64_t atk, int64_t vic) {
    if (!IsEntityId(atk) || !IsEntityId(vic) || atk == vic) return false;
    return IsCachedPlayerAtk(vic) && !IsCachedPlayerAtk(atk);
}

static bool IsValidDamagePair(int64_t atk, int64_t vic) {
    if (!IsEntityId(atk) || !IsEntityId(vic) || atk == vic) return false;
    if (IsConfirmedPlayerStrike(atk, vic)) return true;
    if (g_inv.load() && IsInvIncomingHit(atk, vic)) return true;
    return false;
}

static bool ValidateDamageHandle(void* h) {
    if (!SoftArgPtr(h)) return false;
    int64_t dmg = ReadI64Arg(h, kOffFinalDmg);
    int64_t atk = ReadI64Arg(h, kOffAttackerId);
    int64_t vic = ReadI64Arg(h, kOffBeAttacker);
    /* finalDamage 可为 0（预演/护盾）；禁 >=1 否则漏判整条伤害链 */
    if (dmg < 0 || dmg > kKillDmg) return false;
    return IsValidDamagePair(atk, vic);
}

static bool ValidateDamageInfo(void* p) {
    if (!SoftArgPtr(p)) return false;
    int64_t atk = ReadI64Arg(p, 0);
    int64_t vic = ReadI64Arg(p, 8);
    if (!IsValidDamagePair(atk, vic)) return false;
    float atkVal = Il2Cpp::ReadF32(p, kOffDmgInfoAtk);
    return atkVal >= 0.f && atkVal <= kKillAtk;
}

static void* FindDamageHandleInArgs(void* a2) {
    if (!SoftArgPtr(a2)) return nullptr;
    if (ValidateDamageHandle(a2)) return a2;
    void* p = reinterpret_cast<void*>(reinterpret_cast<uintptr_t>(a2) + 8);
    return ValidateDamageHandle(p) ? p : nullptr;
}

static void* FindDamageInfoInArgs(void* a2) {
    if (!SoftArgPtr(a2)) return nullptr;
    if (ValidateDamageInfo(a2)) return a2;
    void* p = reinterpret_cast<void*>(reinterpret_cast<uintptr_t>(a2) + 8);
    return ValidateDamageInfo(p) ? p : nullptr;
}

static bool IsPlayerSkillObj(void* skill) {
    if (!SoftHeapPtr(skill) || !g_offSkillUnit || !g_offTeam) return false;
    void* unit = ReadPtrHeap(skill, g_offSkillUnit);
    if (!SoftHeapPtr(unit)) return false;
    return ReadI32Heap(unit, g_offTeam) == TEAM_PLAYER;
}

static bool ValidateSkillShape(void* skill) {
    if (!SoftHeapPtr(skill) || !g_offCdCounter || !g_offCanUseTimes) return false;
    const float cd = Il2Cpp::ReadF32(skill, g_offCdCounter);
    const int32_t times = Il2Cpp::ReadI32(skill, g_offCanUseTimes);
    if (times < 0 || times > 99) return false;
    return cd >= -1.f && cd <= 999999.f;
}

static bool SkillIsRunning(void* skill) {
    if (!SoftHeapPtr(skill) || !g_offSkillTimeCounter) return false;
    return Il2Cpp::ReadF32(skill, g_offSkillTimeCounter) > 0.f;
}

static void NocdUndoCd(void* skill) {
    if (!SoftHeapPtr(skill) || !ValidateSkillShape(skill)) return;
    if (g_offCdCounter) WriteF32Heap(skill, g_offCdCounter, 0.f);
    if (g_offCanUseTimes) WriteI32Heap(skill, g_offCanUseTimes, 3);
    g_hitsSkillPatch.fetch_add(1);
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

static void* FindHotfix(const char* name) {
    void* k = FindClassNs("", name);
    if (k) return k;
    return FindClassNs("HotFix", name);
}

static size_t FieldOffHotfix(void* klass, const char* name) {
    return GetApi().FieldOffset(klass, name, 0);
}

static size_t FieldOff(void* klass, const char* name) {
    auto& api = GetApi();
    size_t bias = GameUnity::HYBRIDCLR_FIELD_BIAS;
    size_t off = api.FieldOffset(klass, name, bias);
    if (!off && bias != 8) off = api.FieldOffset(klass, name, 8);
    if (!off && bias != 0) off = api.FieldOffset(klass, name, 0);
    return off;
}

static void* MethodPointer(void* mi) {
    return mi ? *reinterpret_cast<void**>(mi) : nullptr;
}

static bool IsCombatHit(int64_t atk, int64_t vic) {
    return IsConfirmedPlayerStrike(atk, vic);
}

static void FillEidsFromList(void* list, int teamKind, int64_t* out, int cap, int32_t* outN) {
    *outN = 0;
    if (!SoftHeapPtr(list) || !g_offEntityId) return;
    int32_t size = ReadI32Heap(list, kOffListSize);
    if (size < 1 || size > cap * 2) return;
    void* items = ReadPtrHeap(list, kOffListItems);
    if (!SoftHeapPtr(items)) return;
    uintptr_t base = reinterpret_cast<uintptr_t>(items) + kOffArrayData;
    for (int i = 0; i < size && *outN < cap; i++) {
        void* u = *reinterpret_cast<void**>(base + static_cast<size_t>(i) * 8u);
        if (!SoftHeapPtr(u)) continue;
        if (g_offTeam && ReadI32Heap(u, g_offTeam) != teamKind) continue;
        int64_t eid = ReadI64Heap(u, g_offEntityId);
        if (!IsEntityId(eid)) continue;
        out[*outN] = eid;
        (*outN)++;
    }
}

static void PatchDictSkills(void* dict) {
    if (!SoftHeapPtr(dict)) return;
    int32_t count = ReadI32Heap(dict, 0x20);
    if (count < 1 || count > 64) return;
    void* entries = ReadPtrHeap(dict, 0x18);
    if (!SoftHeapPtr(entries)) return;
    int32_t len = ReadI32Heap(entries, 0x18);
    if (len < 1 || len > 128) return;
    uintptr_t base = reinterpret_cast<uintptr_t>(entries) + kOffArrayData;
    for (int i = 0; i < len; i++) {
        void* ent = reinterpret_cast<void*>(base + static_cast<size_t>(i) * 0x18u);
        if (ReadI32Arg(ent, 0) < 0) continue;
        void* skill = ReadPtrHeap(ent, 0x10);
        if (!SoftHeapPtr(skill)) continue;
        NocdUndoCd(skill);
    }
}

static void NocdPatchRunner(void* runner) {
    if (!SoftHeapPtr(runner)) return;
    const size_t offCur = g_offRunnerCurSkill ? g_offRunnerCurSkill : kScCurSkillFb;
    const size_t offDict = g_offRunnerSkills ? g_offRunnerSkills : kScSkillsDictFb;
    void* cur = ReadPtrHeap(runner, offCur);
    NocdUndoCd(cur);
    PatchDictSkills(ReadPtrHeap(runner, offDict));
}

static void NocdPatchUnit(void* unit) {
    if (!SoftHeapPtr(unit) || !g_offUnitSkillDriver || !g_offSdRunner) return;
    void* sd = ReadPtrHeap(unit, g_offUnitSkillDriver);
    if (!SoftHeapPtr(sd)) return;
    void* runner = ReadPtrHeap(sd, g_offSdRunner);
    if (!SoftHeapPtr(runner)) return;
    NocdPatchRunner(runner);
}

static void NocdPatchPlayerList(void* list) {
    if (!SoftHeapPtr(list)) return;
    int32_t size = ReadI32Heap(list, kOffListSize);
    if (size < 1 || size > 16) return;
    void* items = ReadPtrHeap(list, kOffListItems);
    if (!SoftHeapPtr(items)) return;
    uintptr_t base = reinterpret_cast<uintptr_t>(items) + kOffArrayData;
    for (int i = 0; i < size; i++) {
        void* unit = *reinterpret_cast<void**>(base + static_cast<size_t>(i) * 8u);
        NocdPatchUnit(unit);
    }
}

static void ClearAoeKilled() {
    g_aoeKilledN.store(0);
    for (int i = 0; i < kMaxEnemyEid; i++)
        g_aoeKilledEids[i].store(0);
}

static bool AlreadyAoeKilled(int64_t eid) {
    int32_t n = g_aoeKilledN.load();
    for (int i = 0; i < n && i < kMaxEnemyEid; i++) {
        if (g_aoeKilledEids[i].load() == eid) return true;
    }
    return false;
}

static void MarkAoeKilled(int64_t eid) {
    if (!IsEntityId(eid) || AlreadyAoeKilled(eid)) return;
    int32_t n = g_aoeKilledN.load();
    if (n >= kMaxEnemyEid) return;
    g_aoeKilledEids[n].store(eid);
    g_aoeKilledN.store(n + 1);
}

static void RefreshBattleCache() {
    if (!g_findTeam || !g_miFindTeam) return;
    g_inMgrCall.store(true);
    void* mgr = GetBattleMgr();
    if (!SoftHeapPtr(mgr)) {
        g_playerN.store(0);
        g_enemyN.store(0);
        g_inMgrCall.store(false);
        return;
    }
    int64_t players[kMaxPlayerEid];
    int64_t enemies[kMaxEnemyEid];
    int32_t pn = 0, en = 0;

    void* pl = g_findTeam(mgr, TEAM_PLAYER, false, g_miFindTeam);
    void* el = g_findTeam(mgr, TEAM_ENEMY, false, g_miFindTeam);
    FillEidsFromList(pl, TEAM_PLAYER, players, kMaxPlayerEid, &pn);
    FillEidsFromList(el, TEAM_ENEMY, enemies, kMaxEnemyEid, &en);

    g_playerN.store(pn);
    for (int i = 0; i < kMaxPlayerEid; i++)
        g_playerEids[i].store(i < pn ? players[i] : 0);
    g_enemyN.store(en);
    for (int i = 0; i < kMaxEnemyEid; i++)
        g_enemyEids[i].store(i < en ? enemies[i] : 0);

    // 仅战斗结束清空去重；禁止每 tick Clear（实锤重复 InvokeHpHandle → 结算 MonsterItem 异常）
    if (en == 0)
        ClearAoeKilled();

    static std::atomic<uint32_t> s_log{0};
    uint32_t n = s_log.fetch_add(1) + 1;
    if ((pn > 0 || en > 0) && (n <= 5 || (n % 60) == 0))
        TLGD_TRACE("cache player=%d enemy=%d p0=%lld mgr=%p", pn, en,
                   pn > 0 ? (long long)players[0] : 0LL, mgr);
    g_inMgrCall.store(false);
}

static void NocdTickBackup() {
    if (!g_nocd.load() || g_playerN.load() < 1) return;
    if (!g_offUnitSkillDriver || !g_offSdRunner || !g_findTeam || !g_miFindTeam) return;
    g_inMgrCall.store(true);
    void* mgr = GetBattleMgr();
    if (!SoftHeapPtr(mgr)) {
        g_inMgrCall.store(false);
        return;
    }
    const uint32_t before = g_hitsSkillPatch.load();
    void* pl = g_findTeam(mgr, TEAM_PLAYER, false, g_miFindTeam);
    NocdPatchPlayerList(pl);
    const uint32_t patched = g_hitsSkillPatch.load() - before;
    static std::atomic<uint32_t> s_cdLog{0};
    uint32_t ln = s_cdLog.fetch_add(1) + 1;
    if (ln <= 8 || (ln % 40) == 0)
        TLGD_TRACE("tick cd n=%u players=%d patch=%u total=%u", ln, g_playerN.load(), patched,
                   g_hitsSkillPatch.load());
    g_inMgrCall.store(false);
}

static void InvokeHpHandle(int64_t atkId, int64_t vicId) {
    if (!g_miHpHandle) return;
    void* srv = GetBattleServer();
    if (!SoftHeapPtr(srv)) return;
    auto& api = GetApi();
    if (!api.runtime_invoke) return;

    DamageHandleBlob blob{};
    blob.finalDamage = kKillDmg;
    blob.attackerID = atkId;
    blob.beAttackerID = vicId;
    void* args[1] = {&blob};
    void* exc = nullptr;
    api.runtime_invoke(g_miHpHandle, srv, args, &exc);
}

static void AoeSweep(int64_t atkId, int64_t skipVic) {
    if (!g_aoe.load() || g_aoeBusy.load()) return;
    if (!IsCachedPlayerAtk(atkId)) return;

    g_aoeBusy.store(true);
    int32_t n = g_enemyN.load();
    for (int i = 0; i < n && i < kMaxEnemyEid; i++) {
        int64_t eid = g_enemyEids[i].load();
        if (!IsEntityId(eid) || eid == skipVic || AlreadyAoeKilled(eid)) continue;
        InvokeHpHandle(atkId, eid);
        MarkAoeKilled(eid);
    }
    g_aoeBusy.store(false);
}

static bool PatchDamageInfo(void* info, bool* wantKillInvoke) {
    int64_t atk = ReadI64Arg(info, 0);
    int64_t vic = ReadI64Arg(info, 8);
    if (g_inv.load() && IsInvIncomingHit(atk, vic)) {
        WriteI64Arg(info, 8, 0);
        uint32_t n = g_hitsInv.fetch_add(1) + 1;
        if (n <= 8 || (n % 40) == 0)
            TLGD_TRACE("inv dmginfo n=%u atk=%lld vic=%lld", n, (long long)atk, (long long)vic);
        return true;
    }
    if (WantKillStrike() && IsConfirmedPlayerStrike(atk, vic)) {
        *wantKillInvoke = true;
        return true;
    }
    return false;
}

static bool PatchHpHandle(void* handle, int64_t* outAtk, int64_t* outVic, bool* didKill) {
    *didKill = false;
    int64_t atkId = ReadI64Arg(handle, kOffAttackerId);
    int64_t vicId = ReadI64Arg(handle, kOffBeAttacker);
    *outAtk = atkId;
    *outVic = vicId;

    if (g_inv.load() && IsInvIncomingHit(atkId, vicId)) {
        WriteI64Arg(handle, kOffBeAttacker, 0);
        uint32_t n = g_hitsInv.fetch_add(1) + 1;
        if (n <= 8 || (n % 40) == 0)
            TLGD_TRACE("inv n=%u atk=%lld vic=%lld", n, (long long)atkId, (long long)vicId);
        return true;
    }

    if (WantKillStrike() && IsConfirmedPlayerStrike(atkId, vicId)) {
        WriteI64Arg(handle, kOffFinalDmg, kKillDmg);
        *didKill = true;
        uint32_t n = g_hitsKill.fetch_add(1) + 1;
        if (n <= 8 || (n % 40) == 0)
            TLGD_TRACE("kill n=%u atk=%lld vic=%lld", n, (long long)atkId, (long long)vicId);
        return true;
    }
    return false;
}

static void* Hooked_Trampoline(void* a0, void* a1, void* a2, void* a3) {
    if (!g_origTramp) return nullptr;
    if (t_trampDepth > 0 || g_inMgrCall.load() || g_aoeBusy.load())
        return g_origTramp(a0, a1, a2, a3);

    const bool wantDmg = NeedDmgPatch();
    const bool wantNocd = g_nocd.load() && g_playerN.load() > 0;
    if (!wantDmg && !wantNocd) return g_origTramp(a0, a1, a2, a3);
    if (wantDmg && g_playerN.load() < 1 && g_enemyN.load() < 1 && !wantNocd)
        return g_origTramp(a0, a1, a2, a3);

    t_trampDepth++;
    void* handle = nullptr;
    void* dmgInfo = nullptr;
    int64_t atkId = 0, vicId = 0;
    bool didKill = false;
    bool killedViaHandle = false;
    bool wantDmgInfoInvoke = false;
    bool invokedPrimaryKill = false;

    if (wantDmg && SoftArgPtr(a2) && g_enemyN.load() > 0) {
        handle = FindDamageHandleInArgs(a2);
        if (!handle) dmgInfo = FindDamageInfoInArgs(a2);
        if (handle) {
            atkId = ReadI64Arg(handle, kOffAttackerId);
            vicId = ReadI64Arg(handle, kOffBeAttacker);
            uint32_t hn = g_hitsHandle.fetch_add(1) + 1;
            if (hn <= 8 || (hn % 40) == 0)
                TLGD_TRACE("shape n=%u atk=%lld vic=%lld dmg=%lld", hn,
                           (long long)atkId, (long long)vicId,
                           (long long)ReadI64Arg(handle, kOffFinalDmg));
            PatchHpHandle(handle, &atkId, &vicId, &didKill);
            killedViaHandle = didKill;
        } else if (dmgInfo) {
            atkId = ReadI64Arg(dmgInfo, 0);
            vicId = ReadI64Arg(dmgInfo, 8);
            uint32_t dn = g_hitsDmgInfo.fetch_add(1) + 1;
            if (dn <= 8 || (dn % 40) == 0)
                TLGD_TRACE("dmginfo n=%u atk=%lld vic=%lld", dn,
                           (long long)atkId, (long long)vicId);
            PatchDamageInfo(dmgInfo, &wantDmgInfoInvoke);
        }
    }

    void* ret = g_origTramp(a0, a1, a2, a3);

    if (wantNocd && SoftHeapPtr(a0) && SoftPtr(a1)) {
        const bool isReCal = (a1 == g_miReCalSkillCD);
        const bool isCanUse = (a1 == g_miCanUse) ||
            (g_mpCanUse && MethodPointer(a1) == g_mpCanUse);
        if ((isReCal || isCanUse) && ValidateSkillShape(a0) && IsPlayerSkillObj(a0)) {
            if (isReCal) {
                NocdUndoCd(a0);
                uint32_t n = g_hitsReCal.fetch_add(1) + 1;
                if (n <= 10 || (n % 40) == 0)
                    TLGD_TRACE("nocd recal n=%u", n);
            }
            if (isCanUse && !SkillIsRunning(a0)) {
                ret = reinterpret_cast<void*>(1);
                uint32_t n = g_hitsCanUse.fetch_add(1) + 1;
                if (n <= 10 || (n % 40) == 0)
                    TLGD_TRACE("nocd canuse n=%u", n);
            }
        }
    }

    if (wantDmg && IsConfirmedPlayerStrike(atkId, vicId)) {
        const bool aoeOn = g_aoe.load();
        /* handle 路径：patch+orig，禁止对主目标二次 invoke（结算 MonsterItem 实锤） */
        if (wantDmgInfoInvoke && WantKillStrike() && !killedViaHandle) {
            if (!aoeOn || !AlreadyAoeKilled(vicId)) {
                InvokeHpHandle(atkId, vicId);
                invokedPrimaryKill = true;
                if (aoeOn) MarkAoeKilled(vicId);
                uint32_t kn = g_hitsKill.fetch_add(1) + 1;
                if (kn <= 8 || (kn % 40) == 0)
                    TLGD_TRACE("invoke kill n=%u atk=%lld vic=%lld", kn,
                               (long long)atkId, (long long)vicId);
            }
        } else if (killedViaHandle && aoeOn) {
            MarkAoeKilled(vicId);
        }
        if (aoeOn && (killedViaHandle || invokedPrimaryKill)) {
            const uint32_t t = g_tick.load();
            const uint32_t last = g_lastAoeTick.load();
            if (t >= last + 2) {
                g_lastAoeTick.store(t);
                AoeSweep(atkId, vicId);
            }
        }
    }
    t_trampDepth--;
    return ret;
}

static bool ResolveTypes() {
    auto& api = GetApi();
    if (!api.ready && !api.Load(GameUnity::TARGET_LIB)) return false;

    void* clsSkill  = FindHotfix("Skill");
    void* clsUnit   = FindHotfix("BattleUnit");
    void* clsSd     = FindHotfix("SkillDriver");
    void* clsRunner = FindHotfix("SkillContainer");
    if (!clsRunner) clsRunner = api.FindClassByShortName("SkillContainer");
    void* clsEntity = FindHotfix("BattleEntity");
    void* clsMgr    = FindHotfix("BattleEntityManager");
    void* clsServer = FindHotfix("BattleServer");
    if (!clsSkill || !clsEntity || !clsMgr || !clsServer) {
        TLGD_TRACE("class missing skill=%p ent=%p mgr=%p srv=%p", clsSkill, clsEntity, clsMgr, clsServer);
        return false;
    }
    g_clsMgr = clsMgr;
    g_clsSkill = clsSkill;

    g_offTeam = FieldOff(clsEntity, "team");
    g_offEntityId = FieldOff(clsEntity, "EntityID");
    g_offCdCounter = FieldOffHotfix(clsSkill, "cdCounter");
    if (!g_offCdCounter) g_offCdCounter = FieldOff(clsSkill, "cdCounter");
    g_offCanUseTimes = FieldOffHotfix(clsSkill, "CanUseTimes");
    g_offSkillTimeCounter = FieldOffHotfix(clsSkill, "skillTimeCounter");
    g_offSkillUnit = FieldOffHotfix(clsSkill, "unit");
    if (!g_offSkillUnit) g_offSkillUnit = FieldOff(clsSkill, "unit");

    if (clsUnit) {
        g_offUnitSkillDriver = FieldOffHotfix(clsUnit, "_skillDriver");
        if (!g_offUnitSkillDriver) g_offUnitSkillDriver = FieldOff(clsUnit, "_skillDriver");
    }
    if (clsSd) {
        g_offSdRunner = FieldOffHotfix(clsSd, "runner");
        if (!g_offSdRunner) g_offSdRunner = FieldOff(clsSd, "runner");
    }
    if (clsRunner) {
        g_offRunnerCurSkill = FieldOffHotfix(clsRunner, "curSkill");
        if (!g_offRunnerCurSkill) g_offRunnerCurSkill = FieldOff(clsRunner, "curSkill");
        g_offRunnerSkills = FieldOffHotfix(clsRunner, "_skills");
        if (!g_offRunnerSkills) g_offRunnerSkills = FieldOff(clsRunner, "_skills");
    }

    if (!g_offRunnerCurSkill) g_offRunnerCurSkill = kScCurSkillFb;
    if (!g_offRunnerSkills) g_offRunnerSkills = kScSkillsDictFb;

    if (!g_offTeam || !g_offEntityId || !g_offCdCounter || !g_offCanUseTimes) {
        TLGD_TRACE("field off missing t=%zu id=%zu cd=%zu cut=%zu unit=%zu",
                   g_offTeam, g_offEntityId, g_offCdCounter, g_offCanUseTimes, g_offSkillUnit);
        return false;
    }

    g_miGetMgrStuff = api.GetMethod(clsMgr, "get_Stuff", 0);
    g_miGetSrvStuff = api.GetMethod(clsServer, "get_Stuff", 0);
    g_miFindTeam = api.GetMethod(clsMgr, "FindBattleUnitByTeam", 2);
    g_miGetAllEnt = api.GetMethod(clsMgr, "GetAllEntitys", 0);
    g_miHpHandle = api.GetMethod(clsServer, "HpHandle", 1);
    g_miCanUse = api.GetMethod(clsSkill, "CanUse", 0);
    g_miReCalSkillCD = api.GetMethod(clsSkill, "ReCalSkillCD", 0);
    g_mpCanUse = MethodPointer(g_miCanUse);

    void* fnMgrStuff = MethodPointer(g_miGetMgrStuff);
    void* fnSrvStuff = MethodPointer(g_miGetSrvStuff);
    void* fnTeam = MethodPointer(g_miFindTeam);
    if (!fnMgrStuff || !fnTeam || !g_miHpHandle || !g_miGetAllEnt ||
        !g_miCanUse || !g_miReCalSkillCD) {
        TLGD_TRACE("fn missing mgr=%p team=%p hp=%p all=%p can=%p recal=%p",
                   fnMgrStuff, fnTeam, g_miHpHandle, g_miGetAllEnt,
                   g_miCanUse, g_miReCalSkillCD);
        return false;
    }
    g_getMgrStuff = reinterpret_cast<GetStuff_t>(fnMgrStuff);
    g_getSrvStuff = reinterpret_cast<GetStuff_t>(fnSrvStuff);
    g_findTeam = reinterpret_cast<FindTeam_t>(fnTeam);
    g_ready.store(true);
    TLGD_TRACE("types ok cd=%zu cut=%zu unit=%zu sdRun=%zu cur=%zu dict=%zu usd=%zu",
               g_offCdCounter, g_offCanUseTimes, g_offSkillUnit, g_offSdRunner,
               g_offRunnerCurSkill, g_offRunnerSkills, g_offUnitSkillDriver);
    return true;
}

static bool InstallTrampolineOnce() {
    if (g_hooked.load()) return true;
    void* tramp = FileOffToVa(GameUnity::TARGET_LIB, FILE_TRAMPOLINE);
    if (!tramp) {
        TLGD_TRACE("FileOffToVa 0x%X failed", (unsigned)FILE_TRAMPOLINE);
        return false;
    }
    if (DobbyHook(tramp, (void*)Hooked_Trampoline, (void**)&g_origTramp) != 0) {
        TLGD_TRACE("Dobby tramp failed @ %p", tramp);
        return false;
    }
    g_hooked.store(true);
    LOGI("[Tlgd] hooked tramp=%p file=0x%X", tramp, (unsigned)FILE_TRAMPOLINE);
    TLGD_TRACE("hooked tramp=%p hpMi=%p", tramp, g_miHpHandle);
    return true;
}

static void EnsureTrampolineIfNeeded() {
    if (!NeedTrampHook()) return;
    if (g_hooked.load()) return;
    if (!g_ready.load()) return;
    if (!InstallTrampolineOnce())
        LOGI("[Tlgd] late tramp install failed");
}

bool Tlgd_TryInit() {
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;
    if (!g_ready.load() && !ResolveTypes()) return false;
    EnsureTrampolineIfNeeded();
    return g_ready.load();
}

bool Tlgd_IsReady() { return g_ready.load(); }

void Tlgd_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == TLGD_FEAT_KILL) g_kill.store(enabled);
    if (featNum == TLGD_FEAT_INV) g_inv.store(enabled);
    if (featNum == TLGD_FEAT_AOE_KILL) g_aoe.store(enabled);
    if (featNum == TLGD_FEAT_NO_CD) g_nocd.store(enabled);
    LOGI("[Tlgd] feat %d -> %d", featNum, enabled ? 1 : 0);
    if (enabled) EnsureTrampolineIfNeeded();
}

void Tlgd_OnValueChanged(int /*featNum*/, int /*value*/) {}

void Tlgd_Tick(const bool* features) {
    if (!features) return;
    g_kill.store(features[TLGD_FEAT_KILL]);
    g_inv.store(features[TLGD_FEAT_INV]);
    g_aoe.store(features[TLGD_FEAT_AOE_KILL]);
    g_nocd.store(features[TLGD_FEAT_NO_CD]);
    if (!g_ready.load()) Tlgd_TryInit();
    if (!g_ready.load()) return;
    EnsureTrampolineIfNeeded();
    if (!AnyGameplayOn()) return;

    uint32_t t = g_tick.fetch_add(1) + 1;
    if (NeedDmgPatch() || (t % 4) == 0)
        RefreshBattleCache();
    if (g_nocd.load() && (t % 8) == 0)
        NocdTickBackup();
}

void Tlgd_ForceClear() {
    g_kill.store(false);
    g_inv.store(false);
    g_aoe.store(false);
    g_nocd.store(false);
    g_playerN.store(0);
    g_enemyN.store(0);
    ClearAoeKilled();
}
