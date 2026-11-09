/* ================================================================
 *  农场守护者 2.1.21 / vc21 / com.qld.ncsh.xiyou
 *  功能：百倍伤害 / 无敌 / 无限技能 / 解锁加速
 *
 *  ★核心原则（照 10_docs/农场守护者/路线锁定.md §7.4「斌哥路线」）：
 *    只改「瞬时量」（当前血量 / 当前 CD），**绝不改会被结算与上报读到的属性**
 *    （maxHp、DmgAddRatio、FightLogicTimeScale …）。改了属性 ⇒ 游戏自己算出异常
 *    数值并上报 ⇒ 怪清空仍判负（2026-11-11 实测两次踩坑）。
 *
 *    百倍伤害：遍历怪物 AliveMonsterList，按「上一拍血量 − 本拍血量 = 游戏自己扣了多少 Δ」
 *              再额外扣 99×Δ（合计 100 倍）。我方属性一个都不动。
 *    无敌    ：把我方实体 _curHp@88 回填到游戏原 _maxHp@92，不改 _maxHp。
 *    无限技能：把技能单元的 CurCd@96 清零（瞬时量）。
 *    解锁加速：只把月卡特权 EEffect.ActivatestheTrialsgameplaymultiplier 注入
 *              A68PrivilegeCardSystem 的特权表，让游戏自己的加速按钮变可用；
 *              **不写任何倍速字段**，加速与否由玩家在游戏内点按钮决定。
 *
 *  挂点：HybridCLR 帧进入变体 A，VA 0x17E7750 / file 0x17E3750 / 序言 0xF81D0FFE
 *        签名 x0=frameGroup, x1=MethodInfo*, x2=argsPkt, argsPkt+0 = this
 *        （运行时实例是 TrunkGamePlay/TeamGamePlay，FightCore 子类 ⇒ 自检走上溯）
 *
 *  字段偏移见 §8.3；TrueSync.FP = 32.32 定点。
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
    FILE_ENTER_A = 0x17E3750,
};

enum : size_t {
    // FightCore
    FC_TOWER      = 240,
    FC_HERO       = 248,
    FC_CHILD      = 256,
    FC_SUMMON     = 280,
    FC_MGR        = 296,
    FC_RUNAWAY    = 388,
    // MonsterEntityManager
    MGR_ALL_MON   = 24,
    MGR_ALIVE_MON = 40,
    // EntityBase
    E_CUR_HP      = 88,
    E_MAX_HP      = 92,
    // 我方实体的技能组件
    SK_SUMMON     = 336,
    SK_CHILD      = 352,
    SK_TOWER      = 360,
    SK_HERO       = 400,
    // SkillComponent / SkillUnit
    SC_ACTIVE     = 32,
    SU_CUR_CD     = 96,
    // List<T>
    L_ITEMS       = 0x10,
    L_SIZE        = 0x18,
    ARR_DATA      = 0x20,
    // A68PrivilegeCardSystem
    PRIV_EFFECT_LISTS = 32,
};

static const char* kSpeedEffectName = "ActivatestheTrialsgameplaymultiplier";

enum : uint8_t {
    KIND_NONE  = 0,
    KIND_TICK  = 1,
    KIND_CORE  = 2,
    KIND_PRIV  = 3,   // A68PrivilegeCardSystem —— 解锁加速
};

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_dmg{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_skill{false};
static std::atomic<bool> g_speed{false};

// 解锁加速诊断计数（声明前移：InjectPrivEffect 在前面要用）
static std::atomic<uint32_t> g_privAdd{0};
static std::atomic<uint32_t> g_privSeen{0};

static std::atomic<uint32_t> g_hits{0};
static std::atomic<uint32_t> g_patches{0};

static void* g_miTick   = nullptr;
static void* g_kCore    = nullptr;
static void* g_kPrivSys = nullptr;
static void* g_kEEffect = nullptr;
static int32_t g_speedEffectVal = -1;
static bool  g_layoutOk = false;

typedef void* (*EnterA_t)(void* frameGroup, void* methodInfo, void* argsPkt);
static EnterA_t g_origA = nullptr;

typedef void* (*method_get_class_t)(void* method);
typedef const char* (*class_get_name_t)(void* klass);
typedef void* (*class_get_parent_t)(void* klass);
static method_get_class_t g_mGetClass  = nullptr;
static class_get_name_t   g_cGetName   = nullptr;
static class_get_parent_t g_cGetParent = nullptr;

// ---------------------------------------------------------------- 落盘日志
static void DbgLog(const char* fmt, ...) {
    static std::atomic<uint32_t> cnt{0};
    if (cnt.fetch_add(1) > 3000) return;
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
    g_mGetClass  = (method_get_class_t)dlsym(h, "il2cpp_method_get_class");
    g_cGetName   = (class_get_name_t)dlsym(h, "il2cpp_class_get_name");
    g_cGetParent = (class_get_parent_t)dlsym(h, "il2cpp_class_get_parent");
    return g_mGetClass != nullptr;
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

// ---------------------------------------------------------------- mi 分类缓存
struct MiEnt { void* mi; uint8_t kind; };
static MiEnt g_miCache[8192];
static void* g_lastMi = nullptr;
static uint8_t g_lastKind = KIND_NONE;

static uint8_t ClassifyMi(void* mi) {
    if (!mi || !g_mGetClass) return KIND_NONE;
    void* k = g_mGetClass(mi);
    if (!k) return KIND_NONE;
    if (g_kCore && k == g_kCore) return (mi == g_miTick) ? KIND_TICK : KIND_CORE;
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

// ---------------------------------------------------------------- 列表工具
static inline int ListSize(void* list) { return list ? Il2Cpp::ReadI32(list, L_SIZE) : -1; }
static inline void* ListItems(void* list) { return list ? Il2Cpp::ReadPtr(list, L_ITEMS) : nullptr; }
static inline void* ElemAt(void* items, int i) { return Il2Cpp::ReadPtr(items, ARR_DATA + (size_t)i * 8); }

static bool KlassIsA(void* k, void* base) {
    if (!k || !base) return false;
    for (int d = 0; d < 16 && k; d++) {
        if (k == base) return true;
        if (!g_cGetParent) return false;
        k = g_cGetParent(k);
    }
    return false;
}

static bool SelfCheck(void* core) {
    if (!core || !g_kCore) return false;
    if (!Il2Cpp::IsReadableRange((uintptr_t)core, 0x200)) return false;
    void* k = Il2Cpp::ReadPtr(core, 0);
    if (!KlassIsA(k, g_kCore)) {
        static std::atomic<uint32_t> miss{0};
        if (miss.fetch_add(1) < 3)
            DbgLog("selfcheck: not FightCore core=%p got=%p(%s)", core, k,
                   (k && g_cGetName) ? g_cGetName(k) : "?");
        return false;
    }
    const size_t offs[4] = {FC_HERO, FC_TOWER, FC_SUMMON, FC_CHILD};
    for (int i = 0; i < 4; i++) {
        void* lp = Il2Cpp::ReadPtr(core, offs[i]);
        if (!lp) continue;
        if (!Il2Cpp::IsReadableRange((uintptr_t)lp, 0x20)) return false;
        int n = ListSize(lp);
        if (n < 0 || n > 400) return false;
    }
    DbgLog("selfcheck OK core=%p hero=%d tower=%d summon=%d child=%d", core,
           ListSize(Il2Cpp::ReadPtr(core, FC_HERO)), ListSize(Il2Cpp::ReadPtr(core, FC_TOWER)),
           ListSize(Il2Cpp::ReadPtr(core, FC_SUMMON)), ListSize(Il2Cpp::ReadPtr(core, FC_CHILD)));
    return true;
}

// ---------------------------------------------------------------- 百倍伤害：给怪额外扣血
struct MonRec { void* e; int32_t lastHp; };
static MonRec g_mon[1200];

static void MonReset() {
    for (int i = 0; i < 1200; i++) { g_mon[i].e = nullptr; g_mon[i].lastHp = -1; }
}

static MonRec* FindMonRec(void* e) {
    uintptr_t h = ((uintptr_t)e >> 4) % 1200u;
    for (int p = 0; p < 12; p++) {
        MonRec& r = g_mon[(h + (uintptr_t)p) % 1200u];
        if (r.e == e) return &r;
        if (!r.e) { r.e = e; r.lastHp = -1; return &r; }
    }
    return nullptr;
}

static uint32_t ExtraDamageMonsters(void* core) {
    void* mgr = Il2Cpp::ReadPtr(core, FC_MGR);
    if (!mgr) return 0;
    void* lp = Il2Cpp::ReadPtr(mgr, MGR_ALIVE_MON);
    if (!lp) return 0;
    int n = ListSize(lp);
    if (n <= 0 || n > 400) return 0;
    void* items = ListItems(lp);
    if (!items) return 0;
    uint32_t hit = 0;
    if (!Il2Cpp::IsReadableRange((uintptr_t)items, ARR_DATA + (size_t)n * 8)) return 0;
    for (int i = 0; i < n; i++) {
        void* e = ElemAt(items, i);
        if (!e || !Il2Cpp::IsReadableRange((uintptr_t)e, E_MAX_HP + 8)) continue;
        int32_t hp = Il2Cpp::ReadI32(e, E_CUR_HP);
        if (hp < 0 || hp > 100000000) continue;
        MonRec* r = FindMonRec(e);
        if (!r) continue;
        if (r->lastHp < 0) { r->lastHp = hp; continue; }
        int32_t delta = r->lastHp - hp;
        if (delta <= 0) { r->lastHp = hp; continue; }
        int64_t nhp = (int64_t)hp - (int64_t)delta * 99;
        if (nhp < 0) nhp = 0;
        if (nhp < hp) { Il2Cpp::WriteI32(e, E_CUR_HP, (int32_t)nhp); hit++; }
        r->lastHp = (int32_t)nhp;
    }
    return hit;
}

// ---------------------------------------------------------------- 无敌 / 无限技能
static uint32_t HealSide(void* core) {
    static const size_t offs[4] = {FC_HERO, FC_TOWER, FC_SUMMON, FC_CHILD};
    uint32_t n = 0;
    for (int o = 0; o < 4; o++) {
        void* lp = Il2Cpp::ReadPtr(core, offs[o]);
        if (!lp) continue;
        int sz = ListSize(lp);
        if (sz <= 0 || sz > 400) continue;
        void* items = ListItems(lp);
        if (!items || !Il2Cpp::IsReadableRange((uintptr_t)items, ARR_DATA + (size_t)sz * 8)) continue;
        for (int i = 0; i < sz; i++) {
            void* e = ElemAt(items, i);
            if (!e || !Il2Cpp::IsReadableRange((uintptr_t)e, E_MAX_HP + 8)) continue;
            int32_t mx = Il2Cpp::ReadI32(e, E_MAX_HP);
            int32_t hp = Il2Cpp::ReadI32(e, E_CUR_HP);
            if (mx > 0 && mx < 2000000000 && hp < mx) {
                Il2Cpp::WriteI32(e, E_CUR_HP, mx);
                n++;
            }
        }
    }
    return n;
}

/* ★SK_HERO/SK_TOWER/SK_SUMMON/SK_CHILD 是「各实体类自己的技能组件字段偏移」，
 *  不是 FightCore 的字段（FightCore 最大字段才 388）。
 *  第一版写成 ReadPtr(core, SK_HERO) ⇒ 读到野指针 ⇒ 解引用闪退（2026-11-11 实测）。
 *  正确：按列表取到实体，再从实体读对应偏移。 */
static uint32_t ResetSkillCd(void* core) {
    struct { size_t listOff; size_t skOff; } map[4] = {
        {FC_HERO,   SK_HERO},
        {FC_TOWER,  SK_TOWER},
        {FC_SUMMON, SK_SUMMON},
        {FC_CHILD,  SK_CHILD},
    };
    uint32_t n = 0;
    for (int o = 0; o < 4; o++) {
        void* lp = Il2Cpp::ReadPtr(core, map[o].listOff);
        if (!lp || !Il2Cpp::IsReadableRange((uintptr_t)lp, L_SIZE + 4)) continue;
        int sz = ListSize(lp);
        if (sz <= 0 || sz > 400) continue;
        void* items = ListItems(lp);
        if (!items || !Il2Cpp::IsReadableRange((uintptr_t)items, ARR_DATA + (size_t)sz * 8)) continue;
        for (int i = 0; i < sz; i++) {
            void* e = ElemAt(items, i);
            if (!e || !Il2Cpp::IsReadableRange((uintptr_t)e, map[o].skOff + 8)) continue;
            void* sc = Il2Cpp::ReadPtr(e, map[o].skOff);
            if (!sc || !Il2Cpp::IsReadableRange((uintptr_t)sc, SC_ACTIVE + 8)) continue;
            void* sl = Il2Cpp::ReadPtr(sc, SC_ACTIVE);
            if (!sl || !Il2Cpp::IsReadableRange((uintptr_t)sl, L_SIZE + 4)) continue;
            int sn = ListSize(sl);
            if (sn <= 0 || sn > 64) continue;
            void* sitems = ListItems(sl);
            if (!sitems || !Il2Cpp::IsReadableRange((uintptr_t)sitems, ARR_DATA + (size_t)sn * 8)) continue;
            for (int j = 0; j < sn; j++) {
                void* u = ElemAt(sitems, j);
                if (!u || !Il2Cpp::IsReadableRange((uintptr_t)u, SU_CUR_CD + 4)) continue;
                int32_t cd = Il2Cpp::ReadI32(u, SU_CUR_CD);
                if (cd > 0 && cd < 1000000000) {
                    Il2Cpp::WriteI32(u, SU_CUR_CD, 0);
                    n++;
                }
            }
        }
    }
    return n;
}

// ---------------------------------------------------------------- 解锁加速：注入月卡「玩法倍速」特权
/* 解锁加速真正要的东西：月卡记录 Dictionary<int, sPrivateCard>（_privateDic@24）。
 * 实测（2026-11-11）：玩家只有 key=1/cid=1 一张永不过期卡，而 monthId=3 ⇒ 加速归属 Cid=3 的月卡；
 * 只往 _curPrivilegeEffectLists 塞 EEffect 值游戏不认（点击仍弹「开月卡」）。
 * sPrivateCard: Cid@24 / Over@32(Int64，-1=不过期) / Times@40 / State@44(=2 与现有卡一致) */
static void* g_kCard  = nullptr;
static void* g_fpObjNew = nullptr;
static void* g_fpDictAdd = nullptr;
static void* g_miDictAdd = nullptr;
static void* g_fpDictRemove = nullptr;
static void* g_miDictRemove = nullptr;
static int32_t kMonthCardCid = 3;          // 由 monthId 推得；改这里可换卡
static std::atomic<uint32_t> g_cardAdd{0};

static void* g_fpListAdd = nullptr;
static void* g_fpListRemove = nullptr;
static void* g_miListAdd = nullptr;
static void* g_miListRemove = nullptr;

static bool EnsureListOps(void* list) {
    if (g_fpListAdd) return true;
    void* k = Il2Cpp::ReadPtr(list, 0);
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
    DbgLog("list ops resolved klass=%p add=%p mi=%p", k, mpAdd, miAdd);
    return true;
}

/* 在 _privateDic 里查/加/删一张月卡。Dictionary 的 entries 元素 24 B：
 * { hashCode@0, next@4, key@8, pad@12, value@16 }，数组数据 @0x20。 */
static bool DictHasKey(void* dic, int key) {
    if (!dic || !Il2Cpp::IsReadableRange((uintptr_t)dic, 0x30)) return false;
    int cnt = Il2Cpp::ReadI32(dic, 0x20);
    if (cnt <= 0 || cnt > 60) return false;
    void* ent = Il2Cpp::ReadPtr(dic, 0x18);
    if (!ent || !Il2Cpp::IsReadableRange((uintptr_t)ent, ARR_DATA + (size_t)cnt * 24)) return false;
    for (int i = 0; i < cnt; i++) {
        char* base = (char*)ent + ARR_DATA + (size_t)i * 24;
        if (*(int*)base < 0) continue;
        if (*(int*)(base + 8) == key) return true;
    }
    return false;
}

static bool EnsureCardOps(void* dic) {
    if (g_fpDictAdd && g_fpObjNew && g_kCard) return true;
    auto& api = Il2Cpp::GetApi();
    if (!g_kCard) g_kCard = api.FindClassByShortName("sPrivateCard");
    if (!g_fpObjNew) {
        void* h = dlopen(TPL_TARGET_LIB, RTLD_NOW);
        if (h) g_fpObjNew = dlsym(h, "il2cpp_object_new");
    }
    if (!g_fpDictAdd) {
        void* dk = Il2Cpp::ReadPtr(dic, 0);
        if (dk) {
            void* miA = api.GetMethod(dk, "Add", 2);
            void* miR = api.GetMethod(dk, "Remove", 1);
            void* fpA = miA ? Il2Cpp::ReadPtr(miA, 0) : nullptr;
            void* fpR = miR ? Il2Cpp::ReadPtr(miR, 0) : nullptr;
            if (fpA && Il2Cpp::IsExecutableRange((uintptr_t)fpA, 4)) {
                g_fpDictAdd = fpA; g_miDictAdd = miA;
                if (fpR && Il2Cpp::IsExecutableRange((uintptr_t)fpR, 4)) { g_fpDictRemove = fpR; g_miDictRemove = miR; }
                DbgLog("card ops: klass=%p new=%p add=%p mi=%p", g_kCard, g_fpObjNew, fpA, miA);
            }
        }
    }
    return g_fpDictAdd && g_fpObjNew && g_kCard;
}

static void EnsureMonthCard(void* priv, bool on) {
    if (!priv) return;
    void* dic = Il2Cpp::ReadPtr(priv, 24);
    if (!dic || !Il2Cpp::IsReadableRange((uintptr_t)dic, 0x30)) return;
    bool has = DictHasKey(dic, kMonthCardCid);
    if (on == has) return;
    if (!EnsureCardOps(dic)) return;

    if (on) {
        void* card = ((void* (*)(void*))g_fpObjNew)(g_kCard);
        if (!card) { DbgLog("card: object_new failed"); return; }
        Il2Cpp::WriteI32(card, 24, kMonthCardCid);
        *(int64_t*)((char*)card + 32) = -1;     // Over = -1：与现有永不过期卡一致
        Il2Cpp::WriteI32(card, 40, 0);          // Times
        Il2Cpp::WriteI32(card, 44, 2);          // State（照抄现有卡）
        bool ok = Il2Cpp::SafeProbe([&]() {
            ((void (*)(void*, int32_t, void*, void*))g_fpDictAdd)(dic, kMonthCardCid, card, g_miDictAdd);
            return true;
        });
        g_cardAdd.fetch_add(1);
        static uint32_t c0 = 0;
        if (c0++ < 4) DbgLog("card ADD cid=%d ok=%d nowCnt=%d addCnt=%u", kMonthCardCid, ok ? 1 : 0,
                             Il2Cpp::ReadI32(dic, 0x20), g_cardAdd.load());
    } else if (g_fpDictRemove) {
        Il2Cpp::SafeProbe([&]() {
            ((bool (*)(void*, int32_t, void*))g_fpDictRemove)(dic, kMonthCardCid, g_miDictRemove);
            return true;
        });
        static uint32_t c1 = 0;
        if (c1++ < 4) DbgLog("card REMOVE cid=%d nowCnt=%d", kMonthCardCid, Il2Cpp::ReadI32(dic, 0x20));
    }
}

/* 注意：il2cpp 方法有隐含的 MethodInfo* 尾参，漏传会读到野指针直接崩进程（踩过）。 */
static bool InjectPrivEffect(void* priv, bool on) {
    if (!priv || g_speedEffectVal < 0) return false;
    static uint32_t diag = 0;
    void* list = Il2Cpp::ReadPtr(priv, PRIV_EFFECT_LISTS);
    if (!list) { if (diag++ < 2) DbgLog("priv: list=null"); return false; }
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
        bool ok = Il2Cpp::SafeProbe([&]() {
            ((void (*)(void*, int32_t, void*))g_fpListAdd)(list, g_speedEffectVal, g_miListAdd);
            return true;
        });
        g_privAdd.fetch_add(1);
        static uint32_t c0 = 0;
        if (c0++ < 4) DbgLog("priv ADD %d ok=%d n=%d->%d", g_speedEffectVal, ok ? 1 : 0, n,
                             Il2Cpp::ReadI32(list, L_SIZE));
    } else if (g_fpListRemove) {
        Il2Cpp::SafeProbe([&]() {
            ((bool (*)(void*, int32_t, void*))g_fpListRemove)(list, g_speedEffectVal, g_miListRemove);
            return true;
        });
        static uint32_t c1 = 0;
        if (c1++ < 4) DbgLog("priv REMOVE %d -> n=%d", g_speedEffectVal, Il2Cpp::ReadI32(list, L_SIZE));
    }
    return true;
}

/* 解锁加速诊断：打印特权效果表当前内容 + 月卡字典条数 + ADD 累计次数。
 * 用途：判断「注入是否被游戏覆盖」「游戏到底查列表还是查字典」。
 *   _privateDic@24 : Dictionary<int, sPrivateCard>（buckets@0x10 entries@0x18 count@0x20）
 *   _curPrivilegeEffectLists@32 : List<int>（_items@0x10 _size@0x18） */
static void MaybePrivStats(void* priv) {
    if (!priv) return;
    static timespec last{};
    timespec now{};
    clock_gettime(CLOCK_MONOTONIC, &now);
    long ms = (now.tv_sec - last.tv_sec) * 1000L + (now.tv_nsec - last.tv_nsec) / 1000000L;
    if (ms < 6000) return;
    last = now;
    void* dic = Il2Cpp::ReadPtr(priv, 24);
    int dcount = -1;
    if (dic && Il2Cpp::IsReadableRange((uintptr_t)dic, 0x30)) dcount = Il2Cpp::ReadI32(dic, 0x20);
    void* lst = Il2Cpp::ReadPtr(priv, PRIV_EFFECT_LISTS);
    int lsize = -1;
    char vals[160] = {0};
    if (lst && Il2Cpp::IsReadableRange((uintptr_t)lst, 0x20)) {
        lsize = Il2Cpp::ReadI32(lst, L_SIZE);
        void* items = Il2Cpp::ReadPtr(lst, L_ITEMS);
        if (items && lsize > 0 && lsize < 40 && Il2Cpp::IsReadableRange((uintptr_t)items, ARR_DATA + (size_t)lsize * 4)) {
            int off = 0;
            for (int i = 0; i < lsize && off < 150; i++) {
                int v = *(int*)((char*)items + ARR_DATA + (size_t)i * 4);
                off += snprintf(vals + off, sizeof(vals) - off, "%d,", v);
            }
        }
    }
    /* Dictionary<int, sPrivateCard> entries 布局（.NET）：
     *   数组元素 = { hashCode@0, next@4, key@8, (pad@12), value@16 } 共 24 B；数组数据 @0x20
     * sPrivateCard: Cid@24, Over@32(Int64), Times@40, State@44 */
    if (dic && Il2Cpp::IsReadableRange((uintptr_t)dic, 0x30) && dcount > 0 && dcount < 30) {
        void* ent = Il2Cpp::ReadPtr(dic, 0x18);
        if (ent && Il2Cpp::IsReadableRange((uintptr_t)ent, ARR_DATA + (size_t)dcount * 24)) {
            for (int i = 0; i < dcount; i++) {
                char* base = (char*)ent + ARR_DATA + (size_t)i * 24;
                int hc = *(int*)base;
                if (hc < 0) continue;
                int key = *(int*)(base + 8);
                void* val = *(void**)(base + 16);
                if (!val || !Il2Cpp::IsReadableRange((uintptr_t)val, 48)) continue;
                int cid = Il2Cpp::ReadI32(val, 24);
                int64_t over = *(int64_t*)((char*)val + 32);
                int times = Il2Cpp::ReadI32(val, 40);
                int state = Il2Cpp::ReadI32(val, 44);
                DbgLog("privcard key=%d cid=%d over=%lld times=%d state=%d", key, cid,
                       (long long)over, times, state);
            }
        }
    }
    int mid = Il2Cpp::ReadI32(priv, 56);      // MonthId
    int did = Il2Cpp::ReadI32(priv, 48);      // DailyId
    int wid = Il2Cpp::ReadI32(priv, 52);      // WeekId
    DbgLog("privstats dictCount=%d listSize=%d vals=[%s] monthId=%d dailyId=%d weekId=%d addCnt=%u seen=%u",
           dcount, lsize, vals, mid, did, wid,
           g_privAdd.load(std::memory_order_relaxed), g_privSeen.load(std::memory_order_relaxed));
}

// ---------------------------------------------------------------- 统计（诊断）
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
    void* mgr = Il2Cpp::ReadPtr(core, FC_MGR);
    int am = -1, tm = -1;
    if (mgr) { am = ListSize(Il2Cpp::ReadPtr(mgr, MGR_ALIVE_MON)); tm = ListSize(Il2Cpp::ReadPtr(mgr, MGR_ALL_MON)); }
    int runaway = Il2Cpp::ReadI32(core, FC_RUNAWAY);
    int thp = -1, tmax = -1;
    void* tl = Il2Cpp::ReadPtr(core, FC_TOWER);
    if (tl && tw > 0) {
        void* it = ListItems(tl);
        void* e0 = it ? ElemAt(it, 0) : nullptr;
        if (e0) { thp = Il2Cpp::ReadI32(e0, E_CUR_HP); tmax = Il2Cpp::ReadI32(e0, E_MAX_HP); }
    }
    DbgLog("stats tower=%d hero=%d mon(all=%d,alive=%d) runaway=%d tower0=%d/%d",
           tw, hw, tm, am, runaway, thp, tmax);
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

static void ApplyAll(void* core) {
    const bool doDmg = g_dmg.load(std::memory_order_relaxed);
    const bool doInv = g_inv.load(std::memory_order_relaxed);
    const bool doSkill = g_skill.load(std::memory_order_relaxed);
    if (!doDmg && !doInv && !doSkill) return;
    if (doDmg) ExtraDamageMonsters(core);
    if (doInv) HealSide(core);
    if (doSkill) ResetSkillCd(core);
}

// ---------------------------------------------------------------- hook
static void HandleHit(void* frameGroup, void* methodInfo, void* argsPkt) {
    (void)frameGroup;
    if (!argsPkt) return;
    g_hits.fetch_add(1, std::memory_order_relaxed);
    uint8_t k = KindOf(methodInfo);
    if (k == KIND_NONE) return;
    void* self = *(void**)argsPkt;
    if (!self) return;

    if (k == KIND_PRIV) {
        g_privSeen.fetch_add(1);
        if (g_speed.load(std::memory_order_relaxed)) {
            EnsureMonthCard(self, true);
            InjectPrivEffect(self, true);
        }
        MaybePrivStats(self);
        return;
    }

    if (!g_layoutOk) {
        g_layoutOk = SelfCheck(self);
        if (!g_layoutOk) return;
    }
    MaybeStats(self);
    bool go = (k == KIND_TICK);
    if (!go && (g_dmg.load() || g_inv.load() || g_skill.load())) go = ThrottleOk(8);
    if (go) {
        ApplyAll(self);
        if (g_patches.fetch_add(1) == 0)
            DbgLog("first apply: mi=%p core=%p", methodInfo, self);
    }
}

static void* HookedA(void* frameGroup, void* methodInfo, void* argsPkt) {
    HandleHit(frameGroup, methodInfo, argsPkt);
    return g_origA ? g_origA(frameGroup, methodInfo, argsPkt) : nullptr;
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
        if (!g_kCore) { DbgLog("FightCore klass not found"); return false; }
        g_miTick = api.GetMethod(g_kCore, "Tick", 1);
        g_kPrivSys = api.FindClassByShortName("A68PrivilegeCardSystem");
        g_kEEffect = api.FindClassByShortName("EEffect");
        if (g_kEEffect && api.class_get_field && api.field_static_get) {
            void* fld = api.class_get_field(g_kEEffect, kSpeedEffectName);
            if (fld) {
                int32_t v = 0;
                api.field_static_get(fld, &v);
                g_speedEffectVal = v;
            }
        }
        DbgLog("locate kCore=%p miTick=%p kPriv=%p effectVal=%d", g_kCore, g_miTick, g_kPrivSys, g_speedEffectVal);
        LOGI("[NCSH]  kCore=%p miTick=%p kPriv=%p effectVal=%d", g_kCore, g_miTick, g_kPrivSys, g_speedEffectVal);
    }

    void* enterA = FileOffToVa(sec_lib_il2cpp(), FILE_ENTER_A);
    LOGI("[NCSH]  enter-A VA=%p", enterA);
    DbgLog("enter-A VA=%p file=0x%lX", enterA, (unsigned long)FILE_ENTER_A);
    if (!enterA) return false;
    if (!HookPtr(enterA, (void*)HookedA, (void**)&g_origA, "enter-A")) return false;

    MonReset();
    g_ready.store(true);
    DbgLog("init ok (features=4)");
    LOGI("[NCSH]  初始化成功（百倍伤害/无敌/无限技能/解锁加速）");
    return true;
}

bool Ncsh_IsReady() { return g_ready.load(); }

void Ncsh_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case NCSH_FEAT_DMG100: g_dmg.store(enabled); if (enabled) MonReset(); break;
        case NCSH_FEAT_INVULN: g_inv.store(enabled); break;
        case NCSH_FEAT_SKILL:  g_skill.store(enabled); break;
        case NCSH_FEAT_SPEED:  g_speed.store(enabled); break;
        default: break;
    }
    LOGI("[NCSH]  feat %d -> %d", featNum, enabled ? 1 : 0);
    DbgLog("feat %d -> %d", featNum, enabled ? 1 : 0);
}

void Ncsb_OnValueChangedPlaceholder() {}

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
    g_speed.store(features[NCSH_FEAT_SPEED]);
}

void Ncsh_ForceClear() {
    g_dmg.store(false);
    g_inv.store(false);
    g_skill.store(false);
    g_speed.store(false);
    LOGI("[NCSH]  ForceClear");
    DbgLog("ForceClear");
}
