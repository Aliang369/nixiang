/* ================================================================
 *  农场守护者 2.1.21 / vc21 / com.qld.ncsh.xiyou
 *
 *  挂点（真机取证两次）：
 *    HybridCLR 解释器「帧进入」变体 A
 *      VA 0x17E7750 / file 0x17E3750 / 磁盘序言 0xF81D0FFE
 *      签名 (x0 = frameGroup, x1 = MethodInfo*, x2 = argsPkt)
 *    ★2026-11-10 上一版挂 0x17E7888 = 只覆盖 UI/框架层（15 s / 19,645 次，
 *      90 个不同方法里无一个战斗类）；斌哥挂 0x17E7750 = 覆盖战斗侧
 *      （13 s / 1,609,338 次，含 GameFightPlaySystem.Update）。必须用 0x17E7750。
 *
 *  实现路线（放弃「拦方法改参数」）：
 *    实测 DoRemoveHp 桩 / set_CurHp 桩 / TickAfter 桩在 160 万次调用里命中 0
 *    ⇒ 扣血是纯 C#→C# 解释执行，既不经过 methodPointer 桩也不产生新 EnterFrame。
 *    ⇒ 改为挂 FightCore.Tick（每帧）直接改实体数据，伤害走游戏自身结算。
 *
 *  字段偏移（真机 il2cpp 反射枚举，13_scratch/ncsh18/ncsh2/enum_core2.txt）：
 *    FightCore: TowerList@240 AllHeroList@248 AllChildTowerList@256
 *               AllSummonList@280 _monsterEntityManager@296
 *    MonsterEntityManager: AllMonsterList@24 AliveMonsterList@40
 *    EntityBase: _curHp@88 _maxHp@92 IsDead@236 DmgAddRatio@264
 *    SkillComp: Hero@400 Tower@360 ChildTower@352 Summon@336 Monster@384
 *    SkillComponent.ActiveSkillUnitList@32 ; SkillUnit.CurCd@96 MaxCd@80
 *    List<T>: _items@0x10 _size@0x18 ; 数组元素 @0x20 + i*8
 *    TrueSync.FP = 32.32 定点（1.0 = 1<<32 = 4294967296）
 *
 *  安全：首次命中先做结构自检（this@argsPkt+0 的 klass 必须 == FightCore klass，
 *        各 List 的 items/size 合理），不过就禁用并落盘，绝不盲写。
 * ================================================================ */

#include "NcshMod.h"
#include "Includes/SecStrings.hpp"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <ctime>
#include <dlfcn.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Unity/Il2CppApi.h"

#define TPL_TARGET_LIB sec_lib_il2cpp()

enum : uintptr_t {
    FILE_ENTER_A = 0x17E3750,   // 帧进入变体 A（VA 0x17E7750）：战斗/逻辑层
    FILE_ENTER_B = 0x17E3888,   // 帧进入变体 B（VA 0x17E7888）：UI/框架层
};

enum : size_t {
    // FightCore
    FC_TOWER      = 240,
    FC_HERO       = 248,
    FC_CHILD      = 256,
    FC_SUMMON     = 280,
    FC_MGR        = 296,
    // MonsterEntityManager
    MGR_ALL_MON   = 24,
    MGR_ALIVE_MON = 40,
    // EntityBase
    E_CUR_HP      = 88,
    E_MAX_HP      = 92,
    E_IS_DEAD     = 236,
    E_DMG_RATIO   = 264,
    // SkillComp（按实体类不同）
    SK_HERO       = 400,
    SK_TOWER      = 360,
    SK_CHILD      = 352,
    SK_SUMMON     = 336,
    SK_MONSTER    = 384,
    // SkillComponent / SkillUnit
    SC_ACTIVE     = 32,
    SU_CUR_CD     = 96,
    SU_MAX_CD     = 80,
    // List<T>
    L_ITEMS       = 0x10,
    L_SIZE        = 0x18,
    ARR_DATA      = 0x20,
};

static const int64_t kFpOne  = 4294967296LL;    // TrueSync.FP 1.0
static const int64_t kFp100  = 100LL * kFpOne;  // 100 倍
static const int32_t kBigHp  = 100000000;       // 无敌血量上限 1e8

enum : uint8_t {
    KIND_NONE = 0,
    KIND_TICK = 1,      // FightCore.Tick —— 每帧，主路径
    KIND_CORE = 2,      // FightCore 的其它实例方法 —— 备路径（节流）
    KIND_FIGHTSYS = 3,  // FightSystem 任意方法（只观测，绝不写倍速）
    KIND_FIGHTFORM = 4, // UIA05FightForm 任意方法 —— 加速按钮解锁位
    KIND_PRIV = 5,      // A68PrivilegeCardSystem 任意方法 —— 注入/撤销「玩法倍速」特权
};

// 战斗倍速（FightSystem，Logic.dll）
enum : size_t {
    FS_FIGHT_TS = 148,   // FightLogicTimeScale : Single —— 游戏内倍速按钮控制的就是它
    FS_GM_TS    = 152,   // GmFightLogicTimeScale : Single —— GM 通道，Refresh 时优先采纳
    FF_TRIAL_ADDS = 768, // UIA05FightForm.IsTriggerTrialAddSpeed : Boolean —— 月卡试用加速解锁位
};
// A68PrivilegeCardSystem
enum : size_t { PRIV_EFFECT_LISTS = 32 };   // _curPrivilegeEffectLists : List<int>
// 月卡特权里控制「玩法倍速」的枚举项：EEffect.ActivatestheTrialsgameplaymultiplier
static const char* kSpeedEffectName = "ActivatestheTrialsgameplaymultiplier";

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_dmg{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_skill{false};
static std::atomic<bool> g_speed{false};

static std::atomic<uint32_t> g_hits{0};      // 挂点总命中
static std::atomic<uint32_t> g_coreHits{0};  // 命中 FightCore 方法
static std::atomic<uint32_t> g_patches{0};   // 执行 ApplyAll 次数
static std::atomic<uint32_t> g_cDmg{0};
static std::atomic<uint32_t> g_cInv{0};
static std::atomic<uint32_t> g_cSkill{0};

static void* g_miTick = nullptr;
static void* g_kCore  = nullptr;
static void* g_kFightSys  = nullptr;
static void* g_kFightForm = nullptr;
static void* g_kPrivSys   = nullptr;
static void* g_kEEffect   = nullptr;
static int32_t g_speedEffectVal = -1;   // -1 = 未解析
static std::atomic<bool> g_privLogDone{false};
static bool  g_layoutOk = false;
static std::atomic<int>   g_speedHold{0};     // >0 时继续把倍速压回 1.0（关闭后收尾）
static std::atomic<float> g_speedTarget{1.0f};

typedef void* (*EnterA_t)(void* frameGroup, void* methodInfo, void* argsPkt);
static EnterA_t g_origA = nullptr;
static EnterA_t g_origB = nullptr;

typedef void* (*method_get_class_t)(void* method);
typedef const char* (*method_get_name_t)(void* method);
typedef const char* (*class_get_name_t)(void* klass);
typedef void* (*class_get_parent_t)(void* klass);
static method_get_class_t g_mGetClass   = nullptr;
static method_get_name_t  g_mGetName    = nullptr;
static class_get_name_t   g_cGetName    = nullptr;
static class_get_parent_t g_cGetParent  = nullptr;

// ---------------------------------------------------------------- 落盘日志
static void DbgLog(const char* fmt, ...) {
    static std::atomic<uint32_t> cnt{0};
    if (cnt.fetch_add(1) > 3000) return;      // 熔断
    FILE* f = fopen("/data/data/com.qld.ncsh.xiyou/files/ncsh_dbg.log", "a");
    if (!f) return;
    va_list ap;
    va_start(ap, fmt);
    vfprintf(f, fmt, ap);
    va_end(ap);
    std::fputc(10, f);
    fclose(f);
}

static bool EnsureSyms() {
    if (g_mGetClass) return true;
    void* h = dlopen(TPL_TARGET_LIB, RTLD_NOW);
    if (!h) return false;
    g_mGetClass = (method_get_class_t)dlsym(h, "il2cpp_method_get_class");
    g_mGetName  = (method_get_name_t)dlsym(h, "il2cpp_method_get_name");
    g_cGetName  = (class_get_name_t)dlsym(h, "il2cpp_class_get_name");
    g_cGetParent = (class_get_parent_t)dlsym(h, "il2cpp_class_get_parent");
    return g_mGetClass != nullptr;
}

/** file_off → 运行时 VA（r-xp 及 HybridCLR rwxp 都认） */
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

// ---------------------------------------------------------------- mi 分类缓存
struct MiEnt { void* mi; uint8_t kind; };
static MiEnt g_miCache[8192];
static void* g_lastMi = nullptr;
static uint8_t g_lastKind = KIND_NONE;

static uint8_t ClassifyMi(void* mi) {
    if (!mi || !g_mGetClass) return KIND_NONE;
    void* k = g_mGetClass(mi);
    if (!k) return KIND_NONE;
    if (k == g_kCore) return (mi == g_miTick) ? KIND_TICK : KIND_CORE;
    if (g_kFightSys && k == g_kFightSys) return KIND_FIGHTSYS;
    if (g_kFightForm && k == g_kFightForm) return KIND_FIGHTFORM;
    if (g_kPrivSys && k == g_kPrivSys) return KIND_PRIV;
    return KIND_NONE;
}

static inline uint8_t KindOf(void* mi) {
    if (mi == g_lastMi) return g_lastKind;
    uintptr_t h = ((uintptr_t)mi >> 4) & 8191u;
    for (int p = 0; p < 8; p++) {
        MiEnt& e = g_miCache[(h + (uintptr_t)p) & 8191u];
        if (e.mi == mi) { g_lastMi = mi; g_lastKind = e.kind; return e.kind; }
        if (!e.mi) {
            uint8_t k = ClassifyMi(mi);
            e.mi = mi;
            e.kind = k;
            g_lastMi = mi;
            g_lastKind = k;
            return k;
        }
    }
    return KIND_NONE;
}

// ---------------------------------------------------------------- 列表遍历
static inline int ListSize(void* list) {
    if (!list) return -1;
    return Il2Cpp::ReadI32(list, L_SIZE);
}

static inline void* ListItems(void* list) {
    if (!list) return nullptr;
    return Il2Cpp::ReadPtr(list, L_ITEMS);
}

static inline void* ElemAt(void* items, int i) {
    return Il2Cpp::ReadPtr(items, ARR_DATA + (size_t)i * 8);
}

/** klass 的继承链上是否包含 base（TrunkGamePlay / TeamGamePlay 都是 FightCore 子类） */
static bool KlassIsA(void* k, void* base) {
    if (!k || !base) return false;
    for (int d = 0; d < 16 && k; d++) {
        if (k == base) return true;
        if (!g_cGetParent) return false;
        k = g_cGetParent(k);
    }
    return false;
}

/** 结构自检：this 的 klass 属于 FightCore 继承链，且四个我方列表形状合理 */
static bool SelfCheck(void* core) {
    if (!core || !g_kCore) return false;
    if (!Il2Cpp::IsReadableRange((uintptr_t)core, 0x200)) return false;
    void* k = Il2Cpp::ReadPtr(core, 0);
    if (!KlassIsA(k, g_kCore)) {
        static std::atomic<uint32_t> miss{0};
        if (miss.fetch_add(1) < 3)
            DbgLog("selfcheck: klass not FightCore core=%p got=%p(%s) want=%p",
                   core, k, (k && g_cGetName) ? g_cGetName(k) : "?", g_kCore);
        return false;
    }
    const size_t offs[4] = {FC_HERO, FC_TOWER, FC_SUMMON, FC_CHILD};
    for (int i = 0; i < 4; i++) {
        void* lp = Il2Cpp::ReadPtr(core, offs[i]);
        if (!lp) continue;
        if (!Il2Cpp::IsReadableRange((uintptr_t)lp, 0x20)) { DbgLog("selfcheck: list %zu unreadable %p", offs[i], lp); return false; }
        int n = ListSize(lp);
        if (n < 0 || n > 400) { DbgLog("selfcheck: list %zu size=%d", offs[i], n); return false; }
        void* items = ListItems(lp);
        if (n > 0 && !items) { DbgLog("selfcheck: list %zu n=%d items=null", offs[i], n); return false; }
        if (items && !Il2Cpp::IsReadableRange((uintptr_t)items, ARR_DATA + 8)) { DbgLog("selfcheck: items %zu unreadable", offs[i]); return false; }
    }
    DbgLog("selfcheck OK core=%p hero=%d tower=%d summon=%d child=%d",
           core,
           ListSize(Il2Cpp::ReadPtr(core, FC_HERO)),
           ListSize(Il2Cpp::ReadPtr(core, FC_TOWER)),
           ListSize(Il2Cpp::ReadPtr(core, FC_SUMMON)),
           ListSize(Il2Cpp::ReadPtr(core, FC_CHILD)));
    return true;
}

/* ---------------------------------------------------------------- 改写记录与还原
 *  DmgAddRatio / _maxHp 是「写进去就留着」的状态字段：游戏不会自己重算。
 *  只改不还原 ⇒ 关掉开关后效果仍在（2026-11-10 实测：关掉百倍伤害后怪继续秒死）。
 *  因此每实体首次改写时记录原值，关闭开关时逐条还原。 */
struct EntRec {
    void* e;
    int64_t dmgOrig;   // -1 = 未改过
    int32_t maxOrig;   //  0 = 未改过
};
static EntRec g_rec[1024];
static uint32_t g_recN = 0;

static void RecReset() {
    for (int i = 0; i < 1024; i++) { g_rec[i].e = nullptr; g_rec[i].dmgOrig = -1; g_rec[i].maxOrig = 0; }
    g_recN = 0;
}

static EntRec* FindOrAddRec(void* e) {
    uint32_t n = g_recN < 1024 ? g_recN : 1024;
    for (uint32_t i = 0; i < n; i++) if (g_rec[i].e == e) return &g_rec[i];
    uint32_t idx = (g_recN < 1024) ? g_recN : (g_recN % 1024);
    g_recN++;
    g_rec[idx].e = e;
    g_rec[idx].dmgOrig = -1;
    g_rec[idx].maxOrig = 0;
    return &g_rec[idx];
}

static void RestoreRecs() {
    if (g_recN == 0) return;
    int n = 0;
    for (int i = 0; i < 1024; i++) {
        EntRec& r = g_rec[i];
        if (!r.e) continue;
        int32_t mx = Il2Cpp::ReadI32(r.e, E_MAX_HP);
        int32_t hp = Il2Cpp::ReadI32(r.e, E_CUR_HP);
        if (mx <= 0 || mx > 4000000000LL) continue;
        if (r.maxOrig > 0 && mx >= kBigHp) {
            Il2Cpp::WriteI32(r.e, E_MAX_HP, r.maxOrig);
            Il2Cpp::WriteI32(r.e, E_CUR_HP, r.maxOrig);
            n++;
        }
        if (r.dmgOrig >= 0) {
            *(int64_t*)((char*)r.e + E_DMG_RATIO) = r.dmgOrig;
            n++;
        }
    }
    DbgLog("restore: entries=%d restored=%d", g_recN, n);
    LOGI("[NCSH]  restore entries=%d restored=%d", g_recN, n);
    RecReset();
}

/** 对一个我方列表内的实体做修正 */
static void PatchList(void* core, size_t listOff, size_t skillOff,
                      bool doDmg, bool doInv, bool doSkill,
                      uint32_t* nDmg, uint32_t* nInv, uint32_t* nSkill) {
    void* lp = Il2Cpp::ReadPtr(core, listOff);
    if (!lp) return;
    int n = ListSize(lp);
    if (n <= 0 || n > 400) return;
    void* items = ListItems(lp);
    if (!items) return;
    for (int i = 0; i < n; i++) {
        void* e = ElemAt(items, i);
        if (!e) continue;
        if (doDmg) {
            int64_t cur = *(int64_t*)((char*)e + E_DMG_RATIO);
            if (cur < kFp100) {
                EntRec* r = FindOrAddRec(e);
                if (r && r->dmgOrig < 0) r->dmgOrig = cur;
                *(int64_t*)((char*)e + E_DMG_RATIO) = kFp100;
                (*nDmg)++;
            }
        }
        if (doInv) {
            /* ★2026-11-11 修正：不再抬高 _maxHp。
             * 旧实现把 _maxHp/_curHp 都写成 1e8 —— maxHp 会被结算/评分/上报读到，
             * 1 亿是明显异常值，怀疑导致战斗判定/结算异常（用户实测「打完没正常结算奖励」）。
             * 现改为只把 _curHp 回填到 _maxHp（原值），上报数值全部保持游戏真实血量。 */
            int32_t mx = Il2Cpp::ReadI32(e, E_MAX_HP);
            int32_t hp = Il2Cpp::ReadI32(e, E_CUR_HP);
            if (mx > 0 && mx < kBigHp && hp < mx) {
                Il2Cpp::WriteI32(e, E_CUR_HP, mx);
                (*nInv)++;
            }
        }
        if (doSkill && skillOff) {
            void* sc = Il2Cpp::ReadPtr(e, skillOff);
            if (!sc) continue;
            void* al = Il2Cpp::ReadPtr(sc, SC_ACTIVE);
            if (!al) continue;
            int sn = ListSize(al);
            if (sn <= 0 || sn > 200) continue;
            void* si = ListItems(al);
            if (!si) continue;
            for (int j = 0; j < sn; j++) {
                void* u = ElemAt(si, j);
                if (!u) continue;
                if (*(int64_t*)((char*)u + SU_CUR_CD) != 0) {
                    *(int64_t*)((char*)u + SU_CUR_CD) = 0;
                    (*nSkill)++;
                }
            }
        }
    }
}

/* 战斗统计（每 5 秒一条）：塔/英雄数量、首塔血量、存活怪数、逃跑怪数。
 * 用途：区分「塔被打死」与「漏怪超限」两种失败原因。 */
static void MaybeStats(void* core) {
    if (!core) return;
    static timespec last{};
    timespec now{};
    clock_gettime(CLOCK_MONOTONIC, &now);
    long ms = (now.tv_sec - last.tv_sec) * 1000L + (now.tv_nsec - last.tv_nsec) / 1000000L;
    if (ms < 5000) return;
    last = now;
    int tw = ListSize(Il2Cpp::ReadPtr(core, FC_TOWER));
    int hw = ListSize(Il2Cpp::ReadPtr(core, FC_HERO));
    int cw = ListSize(Il2Cpp::ReadPtr(core, FC_CHILD));
    int sw = ListSize(Il2Cpp::ReadPtr(core, FC_SUMMON));
    void* mgr = Il2Cpp::ReadPtr(core, FC_MGR);
    int am = -1, tm = -1;
    if (mgr) { am = ListSize(Il2Cpp::ReadPtr(mgr, MGR_ALIVE_MON)); tm = ListSize(Il2Cpp::ReadPtr(mgr, MGR_ALL_MON)); }
    int runaway = Il2Cpp::ReadI32(core, 388);
    int thp = -1, tmax = -1;
    void* tl = Il2Cpp::ReadPtr(core, FC_TOWER);
    if (tl && tw > 0) {
        void* it = ListItems(tl);
        void* e0 = it ? ElemAt(it, 0) : nullptr;
        if (e0) { thp = Il2Cpp::ReadI32(e0, E_CUR_HP); tmax = Il2Cpp::ReadI32(e0, E_MAX_HP); }
    }
    DbgLog("stats tower=%d hero=%d child=%d summon=%d mon(all=%d,alive=%d) runaway=%d tower0=%d/%d",
           tw, hw, cw, sw, tm, am, runaway, thp, tmax);
}

static void ApplyAll(void* core) {
    const bool doDmg = g_dmg.load(std::memory_order_relaxed);
    const bool doInv = g_inv.load(std::memory_order_relaxed);
    const bool doSkill = g_skill.load(std::memory_order_relaxed);
    if (!doDmg && !doInv && !doSkill) return;

    uint32_t nDmg = 0, nInv = 0, nSkill = 0;
    PatchList(core, FC_HERO,   SK_HERO,   doDmg, doInv, doSkill, &nDmg, &nInv, &nSkill);
    PatchList(core, FC_TOWER,  SK_TOWER,  doDmg, doInv, doSkill, &nDmg, &nInv, &nSkill);
    PatchList(core, FC_SUMMON, SK_SUMMON, doDmg, doInv, doSkill, &nDmg, &nInv, &nSkill);
    PatchList(core, FC_CHILD,  SK_CHILD,  doDmg, doInv, doSkill, &nDmg, &nInv, &nSkill);

    if (nDmg)   g_cDmg.fetch_add(nDmg);
    if (nInv)   g_cInv.fetch_add(nInv);
    if (nSkill) g_cSkill.fetch_add(nSkill);
}

// ---------------------------------------------------------------- 战斗倍速（FightSystem）
/* 语义修正（2026-11-11 用户实测反馈）：
 *   旧实现写 UnityEngine.Time.timeScale ⇒ 整个游戏（含 UI 动画/加载）一起加速，
 *   那不是「游戏内 2 倍速按钮」的效果。
 *   游戏真正的倍速是 FightSystem.FightLogicTimeScale@148（float），由
 *   RefreshFightLogicTimeScale/0 依据月卡特权算出来；GmFightLogicTimeScale@152 是 GM 通道，
 *   Refresh 时优先采纳。UI 的解锁位是 UIA05FightForm.IsTriggerTrialAddSpeed@768。
 *   ⇒ 开：两个时间字段都压到 2.0，并把 UI 解锁位置 true
 *   ⇒ 关：写回 1.0 / false，并用 g_speedHold 兜住接下来约 2 秒（防游戏自己刷回未解锁态）*/
static void ObserveFightSpeed(void* fs) {
    /* 只观测、不写入：解锁由特权表完成，倍速必须由玩家在游戏内点按钮触发。 */
    if (!fs || g_privLogDone.load()) return;
    float cur = *(float*)((char*)fs + FS_FIGHT_TS);
    if (cur > -1.0f && cur < 100.0f) {
        g_privLogDone.store(true);
        DbgLog("fight-speed observed=%.3f (不写入)", cur);
    }
}

/* 解锁「玩法倍速」月卡特权（只解锁，不改速度）：
 *   EEffect.ActivatestheTrialsgameplaymultiplier = 3（真机静态字段实测）
 *   目标容器 A68PrivilegeCardSystem._curPrivilegeEffectLists@32 : List<int>
 *   实测为空 List（_size=0, items.cap=0）⇒ 直接写下标无处可写，
 *   必须调 BCL 的 List<int>.Add(int)（AOT 方法，methodPointer 可直调）让它自己扩容。
 *   关闭时调 Remove(int)。每次命中做一次幂等检查（含则不动）。 */
static void* g_fpListAdd = nullptr;
static void* g_fpListRemove = nullptr;
static void* g_miListAdd = nullptr;      // ★ il2cpp 方法有隐含的 MethodInfo* 尾参，必须传
static void* g_miListRemove = nullptr;

static bool EnsureListOps(void* list) {
    if (g_fpListAdd) return true;
    void* k = Il2Cpp::ReadPtr(list, 0);          // List<int> 的 klass
    if (!k) return false;
    auto& api = Il2Cpp::GetApi();
    void* miAdd = api.GetMethod(k, "Add", 1);
    void* miRem = api.GetMethod(k, "Remove", 1);
    void* mpAdd = miAdd ? Il2Cpp::ReadPtr(miAdd, 0) : nullptr;
    void* mpRem = miRem ? Il2Cpp::ReadPtr(miRem, 0) : nullptr;
    if (!mpAdd || !Il2Cpp::IsExecutableRange((uintptr_t)mpAdd, 4)) {
        DbgLog("list.Add 不可直调 mi=%p mp=%p", miAdd, mpAdd);
        return false;
    }
    g_fpListAdd = mpAdd;
    g_miListAdd = miAdd;
    g_miListRemove = miRem;
    g_fpListRemove = (mpRem && Il2Cpp::IsExecutableRange((uintptr_t)mpRem, 4)) ? mpRem : nullptr;
    DbgLog("list ops resolved klass=%p add=%p remove=%p mi=%p", k, mpAdd, mpRem, miAdd);
    return true;
}

static bool InjectPrivEffect(void* priv, bool on) {
    if (!priv || g_speedEffectVal < 0) return false;
    static uint32_t diag = 0;
    void* list = Il2Cpp::ReadPtr(priv, PRIV_EFFECT_LISTS);
    if (!list) {
        if (diag++ < 2) DbgLog("priv: list=null");
        return false;
    }
    void* items = Il2Cpp::ReadPtr(list, L_ITEMS);
    int n = Il2Cpp::ReadI32(list, L_SIZE);
    if (n < 0 || n > 500) return false;

    bool has = false;
    if (items && Il2Cpp::IsReadableRange((uintptr_t)items, ARR_DATA + 4)) {
        const int* data = (const int*)((char*)items + ARR_DATA);
        for (int i = 0; i < n; i++) if (data[i] == g_speedEffectVal) { has = true; break; }
    }
    if (has == on) return true;

    if (!EnsureListOps(list)) return false;
    if (on) {
        bool ok = false;
        for (int attempt = 0; attempt < 2; attempt++) {
            ok = Il2Cpp::SafeProbe([&]() {
                ((void (*)(void*, int32_t, void*))g_fpListAdd)(list, g_speedEffectVal, g_miListAdd);
                return true;
            });
            int after = Il2Cpp::ReadI32(list, L_SIZE);
            static uint32_t c0 = 0;
            if (c0++ < 4) DbgLog("priv ADD %d ok=%d n=%d->%d", g_speedEffectVal, ok ? 1 : 0, n, after);
            if (ok) break;
        }
    } else if (g_fpListRemove) {
        Il2Cpp::SafeProbe([&]() {
            ((bool (*)(void*, int32_t, void*))g_fpListRemove)(list, g_speedEffectVal, g_miListRemove);
            return true;
        });
        static uint32_t c1 = 0;
        if (c1++ < 4) DbgLog("priv REMOVE %d -> now n=%d", g_speedEffectVal, Il2Cpp::ReadI32(list, L_SIZE));
    }
    return true;
}

static void UnlockSpeedBtn(void* form) {
    if (!form) return;
    uint8_t b = Il2Cpp::ReadU8(form, FF_TRIAL_ADDS);
    if (!b) {
        Il2Cpp::WriteU8(form, FF_TRIAL_ADDS, 1);
        static uint32_t n1 = 0;
        if (n1 < 6) { n1++; DbgLog("speed-btn unlocked"); }
    }
}

// ---------------------------------------------------------------- 节流
static inline bool ThrottleOk(uint32_t minMs) {
    static timespec last{};
    timespec now{};
    clock_gettime(CLOCK_MONOTONIC, &now);
    long ms = (now.tv_sec - last.tv_sec) * 1000L + (now.tv_nsec - last.tv_nsec) / 1000000L;
    if (ms < (long)minMs) return false;
    last = now;
    return true;
}

// ---------------------------------------------------------------- hook
/* 双入口：战斗/逻辑层走 A（0x17E7750），UI/框架层走 B（0x17E7888）。
 * 2026-11-11 实测：只挂 A 时 UIA05FightForm / A68PrivilegeCardSystem 一次都不命中
 * （kPriv 定位成功但 KIND_PRIV 计数为 0）⇒ 解锁类功能必须同时挂 B。 */
static void HandleHit(void* frameGroup, void* methodInfo, void* argsPkt) {
    (void)frameGroup;
    const uint32_t hit = g_hits.fetch_add(1, std::memory_order_relaxed);
    uint8_t k = KindOf(methodInfo);
    if (k == KIND_NONE || !argsPkt) return;

    if (k == KIND_FIGHTSYS || k == KIND_FIGHTFORM || k == KIND_PRIV) {
        g_coreHits.fetch_add(1);
        void* self = *(void**)argsPkt;
        const bool on = g_speed.load(std::memory_order_relaxed);
        if (self) {
            if (k == KIND_FIGHTSYS) {
                ObserveFightSpeed(self);
            } else if (k == KIND_PRIV) {
                InjectPrivEffect(self, on);
            } else if (on) {
                UnlockSpeedBtn(self);
            }
        }
        return;
    }

    g_coreHits.fetch_add(1);
    void* core = *(void**)argsPkt;           // argsPkt+0 = this
    if (!core) return;
    if (!g_layoutOk) {
        g_layoutOk = SelfCheck(core);
        if (!g_layoutOk && (hit & 0xFFFFFu) == 0)
            DbgLog("selfcheck pending hit=%u core=%p", hit, core);
    }
    if (g_layoutOk) {
        MaybeStats(core);
        bool go = (k == KIND_TICK);
        if (!go && (g_dmg.load() || g_inv.load() || g_skill.load()))
            go = ThrottleOk(8);          // 备路径：最多 120 次/秒
        if (go) {
            ApplyAll(core);
            g_patches.fetch_add(1);
            if (g_patches.load() == 1)
                DbgLog("first apply: mi=%p core=%p", methodInfo, core);
        }
    }
}

static void* HookedA(void* frameGroup, void* methodInfo, void* argsPkt) {
    HandleHit(frameGroup, methodInfo, argsPkt);
    return g_origA ? g_origA(frameGroup, methodInfo, argsPkt) : nullptr;
}

static void* HookedB(void* frameGroup, void* methodInfo, void* argsPkt) {
    HandleHit(frameGroup, methodInfo, argsPkt);
    return g_origB ? g_origB(frameGroup, methodInfo, argsPkt) : nullptr;
}

static bool HookPtr(void* addr, void* repl, void** orig, const char* tag) {
    if (!addr) { LOGW("[NCSH]  %s addr null", tag); return false; }
    if (*orig) return true;
    if (DobbyHook(addr, repl, orig) != 0) {
        LOGE("[NCSH]  DobbyHook %s failed @ %p", tag, addr);
        return false;
    }
    LOGI("[NCSH]  hooked %s @ %p", tag, addr);
    DbgLog("hooked %s @ %p", tag, addr);
    return true;
}

// ---------------------------------------------------------------- 生命周期
bool Ncsh_TryInit(JavaVM* vm) {
    (void)vm;
    if (g_ready.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;
    if (!EnsureSyms()) return false;

    auto& api = Il2Cpp::GetApi();
    if (!api.ready) {
        if (!api.Load(TPL_TARGET_LIB)) { DbgLog("api.Load failed"); return false; }
    }
    if (!g_kCore) {
        g_kCore = api.FindClassByShortName("FightCore");
        if (!g_kCore) { DbgLog("FightCore klass not found"); LOGW("[NCSH]  FightCore not found"); return false; }
        g_miTick = api.GetMethod(g_kCore, "Tick", 1);
        g_kFightSys  = api.FindClassByShortName("FightSystem");
        g_kFightForm = api.FindClassByShortName("UIA05FightForm");
        g_kPrivSys   = api.FindClassByShortName("A68PrivilegeCardSystem");
        g_kEEffect   = api.FindClassByShortName("EEffect");
        if (g_kEEffect && api.class_get_field && api.field_static_get) {
            void* fld = api.class_get_field(g_kEEffect, kSpeedEffectName);
            if (fld) {
                int32_t v = 0;
                api.field_static_get(fld, &v);
                g_speedEffectVal = v;
            }
        }
        DbgLog("speed-effect %s -> val=%d", kSpeedEffectName, g_speedEffectVal);
        DbgLog("locate kCore=%p miTick=%p kFightSys=%p kFightForm=%p",
               g_kCore, g_miTick, g_kFightSys, g_kFightForm);
        LOGI("[NCSH]  kCore=%p miTick=%p kFightSys=%p kFightForm=%p kPriv=%p effectVal=%d",
             g_kCore, g_miTick, g_kFightSys, g_kFightForm, g_kPrivSys, g_speedEffectVal);
    }

    void* enterA = FileOffToVa(sec_lib_il2cpp(), FILE_ENTER_A);
    void* enterB = FileOffToVa(sec_lib_il2cpp(), FILE_ENTER_B);
    LOGI("[NCSH]  enter-A VA=%p enter-B VA=%p", enterA, enterB);
    DbgLog("enter-A VA=%p enter-B VA=%p", enterA, enterB);
    if (!enterA) return false;
    if (!HookPtr(enterA, (void*)HookedA, (void**)&g_origA, "enter-A")) return false;
    if (enterB && !HookPtr(enterB, (void*)HookedB, (void**)&g_origB, "enter-B"))
        DbgLog("enter-B hook failed (UI 解锁将不可用)");

    RecReset();
    g_ready.store(true);
    DbgLog("init ok");
    LOGI("[NCSH]  初始化成功");
    return true;
}

bool Ncsh_IsReady() { return g_ready.load(); }

void Ncsh_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case NCSH_FEAT_DMG100:
            g_dmg.store(enabled);
            if (!enabled) Il2Cpp::SafeProbe([]() { RestoreRecs(); return true; });
            break;
        case NCSH_FEAT_INVULN:
            g_inv.store(enabled);
            if (!enabled) Il2Cpp::SafeProbe([]() { RestoreRecs(); return true; });
            break;
        case NCSH_FEAT_SKILL:  g_skill.store(enabled); break;
        case NCSH_FEAT_SPEED:
            g_speed.store(enabled);
            g_speedHold.store(enabled ? 0 : 600);   // 关闭后再兜 ~2 秒写回 1.0
            DbgLog("speed feat -> %d", enabled ? 1 : 0);
            break;
        default: break;
    }
    LOGI("[NCSH]  feat %d -> %d", featNum, enabled ? 1 : 0);
    DbgLog("feat %d -> %d", featNum, enabled ? 1 : 0);
}

void Ncsh_OnValueChanged(int featNum, int value) {
    (void)featNum;
    (void)value;
}

void Ncsh_Tick(const bool* features, const int* values) {
    (void)values;
    if (!features) return;
    g_dmg.store(features[NCSH_FEAT_DMG100]);
    g_inv.store(features[NCSH_FEAT_INVULN]);
    g_skill.store(features[NCSH_FEAT_SKILL]);
    const bool sp = features[NCSH_FEAT_SPEED];
    if (sp != g_speed.load()) {
        g_speed.store(sp);
        g_speedHold.store(sp ? 0 : 600);
    }
}

void Ncsh_ForceClear() {
    if (g_dmg.load() || g_inv.load()) Il2Cpp::SafeProbe([]() { RestoreRecs(); return true; });
    g_dmg.store(false);
    g_inv.store(false);
    g_skill.store(false);
    g_speed.store(false);
    g_speedHold.store(600);
    LOGI("[NCSH]  ForceClear");
    DbgLog("ForceClear");
}
