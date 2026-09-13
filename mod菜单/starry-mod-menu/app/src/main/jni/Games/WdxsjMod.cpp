/* ================================================================
 *  WdxsjMod.cpp — 我的修仙世界 (com.lingsi.xiuxiandanji / TapTap 4.5 / vc45)
 *
 *  只复刻斌哥改包的三项功能：
 *    1 百倍伤害       BattleMgr.HurtShow                     RVA 0x1437D24
 *    2 无敌          CreatureControl.ReduceCreatureSpNum      RVA 0x1287E9C
 *    3 无限货币道具   BagDataMgr.{GetMoneyId,GetItemNum}      RVA 0x1238808/0x1239104
 *                    BagDataMgr.CheckConsume(List<ItemInfo>)  RVA 0x123B858
 *                    LongRand.SetValue(long)                  RVA 0x144BBA4
 *
 *  反封号兜底（斌哥同款，常驻）：
 *    BaseDataMgr.GgSet()                     RVA 0x132BED0  空实现
 *    EncryptPlayerDataMgr.CheckZuoBi()        RVA 0x122AF80  恒 false
 *    UnityEngine.Application.Quit()           RVA 0x26E0120  只记录并放行
 *
 *  设计铁律：
 *    - RVA 优先 + 首指令校验；校验失败才按类名/方法名/参数个数回退
 *    - 开关全关 = 功能 hook 不安装；反封号 hook 常驻
 *    - 所有功能 hook 必须调用 orig（Void 空实现和恒 false 除外）
 *    - 类未就绪时不挂战斗 hook，等待 Tick 重试；不扫描堆、不盲写字段
 *    - 不修改 MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk
 * ================================================================ */

#include "WdxsjMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <ctime>
#include <link.h>
#include <pthread.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define WDXSJ_LOG "[wdxsj]"
#define WDXSJ_TARGET_LIB sec_lib_il2cpp()

// ============================================================
// ① 状态与开关
// ============================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_classesResolved{false};
static std::atomic<bool> g_safetyHooked{false};
static std::atomic<bool> g_combatHooked{false};
static std::atomic<bool> g_currencyHooked{false};
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_dmg{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_cur{false};

static std::atomic<bool> g_logGgSet{false};
static std::atomic<bool> g_logZuoBi{false};
static std::atomic<bool> g_logQuit{false};

// 类指针：只在初始化线程写，hook 里只读
static void* g_playerClass = nullptr;
static void* g_worldBossClass = nullptr;
static void* g_battleMgrClass = nullptr;
static void* g_creatureClass = nullptr;
static void* g_bagDataMgrClass = nullptr;
static void* g_baseDataMgrClass = nullptr;
static void* g_encryptPlayerDataMgrClass = nullptr;

// ============================================================
// ② 原函数指针
// ============================================================
typedef void    (*HurtShow_t)(void* self, void* atk_target, void* be_atk_target,
                              int64_t hurt, int32_t hurt_type, int32_t buffid, int32_t type);
typedef void    (*ReduceCreatureSpNum_t)(void* self, void* atk_target,
                                         int32_t reduce_type, int64_t value);
typedef void*   (*GetMoneyId_t)(int32_t sub_type);
typedef void*   (*GetItemNum_t)(int32_t itemid, int32_t favornpcid);
typedef void*   (*CheckConsumeList_t)(void* consume_items);
typedef void    (*LongRandSetValue_t)(void* self, int64_t value);
typedef void    (*GgSet_t)(void* self);
typedef bool    (*CheckZuoBi_t)(void* self);
typedef void    (*ApplicationQuit_t)();

static HurtShow_t              g_origHurtShow = nullptr;
static ReduceCreatureSpNum_t   g_origReduceSp = nullptr;
static GetMoneyId_t            g_origGetMoneyId = nullptr;
static GetItemNum_t            g_origGetItemNum = nullptr;
static CheckConsumeList_t      g_origCheckConsume = nullptr;
static LongRandSetValue_t      g_setLongValue = nullptr;
static GgSet_t                 g_origGgSet = nullptr;
static CheckZuoBi_t            g_origCheckZuoBi = nullptr;
static ApplicationQuit_t       g_origApplicationQuit = nullptr;

// ============================================================
// ③ 基础安全助手
// ============================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

static int32_t SafeReadI32(void* obj, size_t off, int32_t defv) {
    if (!OkPtr(obj)) return defv;
    int32_t v = defv;
    memcpy(&v, reinterpret_cast<uint8_t*>(obj) + off, sizeof(v));
    return v;
}

static int64_t SafeReadI64(void* obj, size_t off, int64_t defv) {
    if (!OkPtr(obj)) return defv;
    int64_t v = defv;
    memcpy(&v, reinterpret_cast<uint8_t*>(obj) + off, sizeof(v));
    return v;
}

static void SafeWriteI32(void* obj, size_t off, int32_t v) {
    if (!OkPtr(obj)) return;
    memcpy(reinterpret_cast<uint8_t*>(obj) + off, &v, sizeof(v));
}

static int64_t NowMs() {
    struct timespec ts {};
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return static_cast<int64_t>(ts.tv_sec) * 1000 + ts.tv_nsec / 1000000;
}

// ============================================================
// ④ libil2cpp 基址与 RVA 校验
// ============================================================
struct MapEntry {
    uintptr_t start;
    uintptr_t end;
    unsigned long off;
    unsigned long inode;
    char perm[8];
};

static int PhdrCallback(struct dl_phdr_info* info, size_t, void* data) {
    if (info->dlpi_name && strstr(info->dlpi_name, WDXSJ_TARGET_LIB) && info->dlpi_addr) {
        *reinterpret_cast<uintptr_t*>(data) = static_cast<uintptr_t>(info->dlpi_addr);
        return 1;
    }
    return 0;
}

static uintptr_t BaseFromMaps() {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return 0;
    MapEntry entries[256] = {};
    int count = 0;
    char line[768];
    while (count < 256 && fgets(line, sizeof(line), fp)) {
        if (!strstr(line, WDXSJ_TARGET_LIB)) continue;
        unsigned long start = 0, end = 0, off = 0, devMaj = 0, devMin = 0, inode = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx %lx:%lx %lu",
                   &start, &end, perm, &off, &devMaj, &devMin, &inode) != 7) {
            continue;
        }
        MapEntry& e = entries[count++];
        e.start = static_cast<uintptr_t>(start);
        e.end = static_cast<uintptr_t>(end);
        e.off = off;
        e.inode = inode;
        memcpy(e.perm, perm, sizeof(e.perm));
    }
    fclose(fp);

    uintptr_t best = 0;
    for (int i = 0; i < count; i++) {
        const MapEntry& exec = entries[i];
        if (!strchr(exec.perm, 'x')) continue;
        for (int j = 0; j < count; j++) {
            const MapEntry& cand = entries[j];
            if (cand.off != 0 || cand.inode != exec.inode) continue;
            if (cand.start <= exec.start && exec.start - cand.start < 0x10000000UL) {
                if (!best || cand.start > best) best = cand.start;
            }
        }
    }
    return best;
}

static uintptr_t LibBaseAddr() {
    static uintptr_t base = 0;
    if (base) return base;

    uintptr_t found = 0;
    dl_iterate_phdr(PhdrCallback, &found);
    if (found) {
        base = found;
        LOGI(WDXSJ_LOG " libil2cpp base=0x%lX (dl_iterate_phdr)", (unsigned long)base);
        return base;
    }
    base = BaseFromMaps();
    if (base) LOGI(WDXSJ_LOG " libil2cpp base=0x%lX (maps fallback)", (unsigned long)base);
    return base;
}

static bool VaInExecMap(uintptr_t va) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return false;
    char line[768];
    bool ok = false;
    while (fgets(line, sizeof(line), fp)) {
        if (!strstr(line, WDXSJ_TARGET_LIB)) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x')) continue;
        if (va >= start && va < end) { ok = true; break; }
    }
    fclose(fp);
    return ok;
}

static void* VaFromRva(const char* tag, uintptr_t rva, uint32_t expect) {
    uintptr_t base = LibBaseAddr();
    if (!base) {
        LOGW(WDXSJ_LOG " %s base miss", tag);
        return nullptr;
    }
    uintptr_t va = base + rva;
    if (!VaInExecMap(va)) {
        LOGW(WDXSJ_LOG " %s rva=0x%lX not in exec map", tag, (unsigned long)rva);
        return nullptr;
    }
    if (expect) {
        uint32_t got = 0;
        memcpy(&got, reinterpret_cast<void*>(va), sizeof(got));
        if (got != expect) {
            LOGW(WDXSJ_LOG " %s rva drift got=0x%08X want=0x%08X", tag, got, expect);
            return nullptr;
        }
    }
    return reinterpret_cast<void*>(va);
}

static bool EnsureApi() {
    if (GetApi().ready) return true;
    return GetApi().Load(WDXSJ_TARGET_LIB);
}

// ============================================================
// ⑤ 类解析：一次枚举所有 image，避免重复全类扫描
// ============================================================
static bool ResolveClasses() {
    if (g_classesResolved.load()) return true;

    static std::atomic<int64_t> lastAttemptMs{0};
    const int64_t now = NowMs();
    int64_t prev = lastAttemptMs.load(std::memory_order_relaxed);
    if (now - prev < 2000) return false;
    if (!lastAttemptMs.compare_exchange_strong(prev, now, std::memory_order_relaxed)) return false;

    auto& api = GetApi();
    if (!api.ready) return false;
    Il2Cpp::SetScanThread(pthread_self());
    const bool ok = Il2Cpp::SafeProbe([&]() -> bool {
        size_t n = 0;
        void** asms = api.Assemblies(&n);
        if (!asms || n == 0) return false;
        for (size_t i = 0; i < n; i++) {
            void* img = api.ImageOf(asms[i]);
            if (!img) continue;
            uint32_t cnt = api.image_class_count(img);
            if (cnt == 0 || cnt > 200000) continue;
            for (uint32_t j = 0; j < cnt; j++) {
                void* k = api.image_class(img, j);
                if (!k) continue;
                const char* nm = api.class_name(k);
                if (!nm) continue;
                if (!g_playerClass && strcmp(nm, "PlayerControl") == 0) g_playerClass = k;
                else if (!g_worldBossClass && strcmp(nm, "WorldBossControl") == 0) g_worldBossClass = k;
                else if (!g_battleMgrClass && strcmp(nm, "BattleMgr") == 0) g_battleMgrClass = k;
                else if (!g_creatureClass && strcmp(nm, "CreatureControl") == 0) g_creatureClass = k;
                else if (!g_bagDataMgrClass && strcmp(nm, "BagDataMgr") == 0) g_bagDataMgrClass = k;
                else if (!g_baseDataMgrClass && strcmp(nm, "BaseDataMgr") == 0) g_baseDataMgrClass = k;
                else if (!g_encryptPlayerDataMgrClass && strcmp(nm, "EncryptPlayerDataMgr") == 0) g_encryptPlayerDataMgrClass = k;
            }
        }
        return true;
    });

    if (!ok) {
        LOGW(WDXSJ_LOG " class scan probe failed (will retry)");
        return false;
    }
    if (!g_playerClass) {
        LOGW(WDXSJ_LOG " PlayerControl not found yet (will retry)");
        return false;
    }

    g_classesResolved.store(true);
    LOGI(WDXSJ_LOG " classes: Player=%p WorldBoss=%p BattleMgr=%p Creature=%p Bag=%p Base=%p Encrypt=%p",
         g_playerClass, g_worldBossClass, g_battleMgrClass, g_creatureClass,
         g_bagDataMgrClass, g_baseDataMgrClass, g_encryptPlayerDataMgrClass);
    return true;
}

// ============================================================
// ⑥ 运行类判定：精确类或类名匹配；不用堆扫描
// ============================================================
static bool IsClassLike(void* obj, void* klass, const char* name) {
    if (!obj) return false;
    auto& api = GetApi();
    if (!api.object_class) return false;
    void* k = api.object_class(obj);
    if (!k) return false;
    if (klass && k == klass) return true;
    if (api.class_name && name) {
        const char* n = api.class_name(k);
        if (n && strcmp(n, name) == 0) return true;
    }
    return false;
}

static bool IsPlayer(void* obj) {
    return IsClassLike(obj, g_playerClass, "PlayerControl");
}

static bool IsWorldBoss(void* obj) {
    return IsClassLike(obj, g_worldBossClass, "WorldBossControl");
}

// ============================================================
// ⑦ 货币 ID 白名单（斌哥 GetMoneyId hook 的等价实现）
// ============================================================
static constexpr int kMaxMoneyIds = 48;
static std::atomic<int> g_moneyCount{0};
static std::atomic<int> g_moneyIds[kMaxMoneyIds];
static std::atomic_flag g_moneyLock = ATOMIC_FLAG_INIT;

static void MoneyLock() {
    while (g_moneyLock.test_and_set(std::memory_order_acquire)) {}
}

static void MoneyUnlock() {
    g_moneyLock.clear(std::memory_order_release);
}

static void AddMoneyId(int32_t id) {
    if (id < 1) return;
    MoneyLock();
    const int n = g_moneyCount.load(std::memory_order_relaxed);
    bool found = false;
    for (int i = 0; i < n; i++) {
        if (g_moneyIds[i].load(std::memory_order_relaxed) == id) { found = true; break; }
    }
    if (!found && n < kMaxMoneyIds) {
        g_moneyIds[n].store(id, std::memory_order_relaxed);
        g_moneyCount.store(n + 1, std::memory_order_release);
    }
    MoneyUnlock();
}

static bool IsMoneyId(int32_t id) {
    MoneyLock();
    const int n = g_moneyCount.load(std::memory_order_acquire);
    bool found = false;
    for (int i = 0; i < n; i++) {
        if (g_moneyIds[i].load(std::memory_order_relaxed) == id) { found = true; break; }
    }
    MoneyUnlock();
    return found;
}

// ============================================================
// ⑧ Hook 实现
// ============================================================
static int64_t ScaleDamage100(int64_t hurt) {
    if (hurt <= 0) return hurt;
    if (hurt > 10000000000LL) return 1000000000000LL;
    int64_t out = hurt * 100;
    return out <= 1 ? 1 : out;
}

static void Hooked_HurtShow(void* self, void* atk, void* be, int64_t hurt,
                            int32_t hurt_type, int32_t buffid, int32_t type) {
    if (!g_origHurtShow) return;
    if (g_dmg.load() && be && hurt >= 1 && !IsPlayer(be) && !IsWorldBoss(be)) {
        hurt = ScaleDamage100(hurt);
    }
    g_origHurtShow(self, atk, be, hurt, hurt_type, buffid, type);
}

static void Hooked_ReduceCreatureSpNum(void* self, void* atk, int32_t reduce_type, int64_t value) {
    if (g_inv.load() && reduce_type == 3 && IsPlayer(self)) {
        // E_CreatureSpNum.NowHp = 3；吞掉这次玩家本体的当前 HP 扣减。
        return;
    }
    if (g_origReduceSp) g_origReduceSp(self, atk, reduce_type, value);
}

static void* Hooked_GetMoneyId(int32_t sub_type) {
    void* result = g_origGetMoneyId ? g_origGetMoneyId(sub_type) : nullptr;
    if (g_cur.load() && result && OkPtr(result)) {
        int32_t id = SafeReadI32(result, 0x18, 0);
        if (id >= 1) AddMoneyId(id);
    }
    return result;
}

static void* Hooked_GetItemNum(int32_t itemid, int32_t favornpcid) {
    void* result = g_origGetItemNum ? g_origGetItemNum(itemid, favornpcid) : nullptr;
    if (g_cur.load() && result && OkPtr(result)) {
        const bool relevant = (itemid > 999999) || IsMoneyId(itemid);
        if (relevant) {
            int64_t real = SafeReadI64(result, 0x20, 0);
            if (real < 0 && g_setLongValue) {
                // 与斌哥一致：负数 realNum 视为异常值，调用 LongRand(long) 归零修复。
                g_setLongValue(result, 0);
            }
        }
    }
    return result;
}

static void* Hooked_CheckConsumeList(void* consume_items) {
    void* result = g_origCheckConsume ? g_origCheckConsume(consume_items) : nullptr;
    if (g_cur.load() && result && OkPtr(result)) {
        // IntRand: numA@0x10, numB@0x14, realNum@0x18。
        // 斌哥把“不足”改成“够”：numA=1234, numB=-1234, realNum=0。
        if (SafeReadI32(result, 0x18, 0) != 0) {
            SafeWriteI32(result, 0x10, 0x4D2);
            SafeWriteI32(result, 0x14, static_cast<int32_t>(0xFFFFFB2E));
            SafeWriteI32(result, 0x18, 0);
        }
    }
    return result;
}

static void Hooked_GgSet(void* self) {
    (void)self;
    if (!g_logGgSet.exchange(true)) {
        LOGW(WDXSJ_LOG " GgSet blocked (permanent-ban flag path)");
    }
    // 不调用 orig：阻止写永久封号标记。
}

static bool Hooked_CheckZuoBi(void* self) {
    (void)self;
    if (!g_logZuoBi.exchange(true)) {
        LOGW(WDXSJ_LOG " CheckZuoBi forced false");
    }
    return false;  // 不调用 orig。
}

static void Hooked_ApplicationQuit() {
    if (!g_logQuit.exchange(true)) {
        LOGW(WDXSJ_LOG " Application.Quit observed; forwarding to game");
    }
    if (g_origApplicationQuit) g_origApplicationQuit();
}

// ============================================================
// ⑨ 挂点表 / 双路径安装
// ============================================================
struct HookSlot {
    const char* tag;
    uintptr_t   rva;
    uint32_t    expect;
    const char* cls;
    const char* method;
    int         argc;
    void*       hook;
    void**      orig;
};

static bool HookByName(const HookSlot& s) {
    if (!s.cls || !s.method) return false;
    auto& api = GetApi();
    if (!api.ready || !api.class_method) return false;
    void* klass = nullptr;
    if (strcmp(s.cls, "BattleMgr") == 0) klass = g_battleMgrClass;
    else if (strcmp(s.cls, "CreatureControl") == 0) klass = g_creatureClass;
    else if (strcmp(s.cls, "BagDataMgr") == 0) klass = g_bagDataMgrClass;
    else if (strcmp(s.cls, "BaseDataMgr") == 0) klass = g_baseDataMgrClass;
    else if (strcmp(s.cls, "EncryptPlayerDataMgr") == 0) klass = g_encryptPlayerDataMgrClass;
    if (!klass) klass = api.FindClassByShortName(s.cls);
    if (!klass) {
        LOGW(WDXSJ_LOG " %s class miss %s", s.tag, s.cls);
        return false;
    }
    void* mi = api.GetMethod(klass, s.method, s.argc);
    if (!mi) {
        LOGW(WDXSJ_LOG " %s method miss %s.%s argc=%d", s.tag, s.cls, s.method, s.argc);
        return false;
    }
    void* fn = *reinterpret_cast<void**>(mi);
    if (!fn || !OkPtr(fn)) {
        LOGW(WDXSJ_LOG " %s bad method pointer", s.tag);
        return false;
    }
    if (DobbyHook(fn, s.hook, s.orig) != 0) {
        LOGW(WDXSJ_LOG " %s DobbyHook by-name failed", s.tag);
        return false;
    }
    LOGI(WDXSJ_LOG " %s hooked by-name %s.%s", s.tag, s.cls, s.method);
    return true;
}

static bool InstallSlot(const HookSlot& s) {
    if (s.orig && *s.orig) return true;  // 幂等
    if (s.rva) {
        void* va = VaFromRva(s.tag, s.rva, s.expect);
        if (va && DobbyHook(va, s.hook, s.orig) == 0) {
            LOGI(WDXSJ_LOG " %s hooked @rva 0x%lX", s.tag, (unsigned long)s.rva);
            return true;
        }
    }
    return HookByName(s);
}

static int InstallSlots(const HookSlot* slots, size_t count) {
    int ok = 0;
    for (size_t i = 0; i < count; i++) {
        if (InstallSlot(slots[i])) {
            ok++;
            g_anyHooked.store(true);
        } else {
            LOGW(WDXSJ_LOG " %s hook not installed (will retry)", slots[i].tag);
        }
    }
    return ok;
}

static const HookSlot kSafetySlots[] = {
    {"GgSet", 0x132BED0, 0xF81E0FFE, "BaseDataMgr", "GgSet", 0,
     reinterpret_cast<void*>(Hooked_GgSet), reinterpret_cast<void**>(&g_origGgSet)},
    {"CheckZuoBi", 0x122AF80, 0xA9BE57FE, "EncryptPlayerDataMgr", "CheckZuoBi", 0,
     reinterpret_cast<void*>(Hooked_CheckZuoBi), reinterpret_cast<void**>(&g_origCheckZuoBi)},
    {"ApplicationQuit", 0x26E0120, 0xF81E0FFE, nullptr, nullptr, 0,
     reinterpret_cast<void*>(Hooked_ApplicationQuit), reinterpret_cast<void**>(&g_origApplicationQuit)},
};

static const HookSlot kCombatSlots[] = {
    {"HurtShow", 0x1437D24, 0xFC190FEA, "BattleMgr", "HurtShow", 6,
     reinterpret_cast<void*>(Hooked_HurtShow), reinterpret_cast<void**>(&g_origHurtShow)},
    {"ReduceCreatureSpNum", 0x1287E9C, 0xA9BA7BFD, "CreatureControl", "ReduceCreatureSpNum", 3,
     reinterpret_cast<void*>(Hooked_ReduceCreatureSpNum), reinterpret_cast<void**>(&g_origReduceSp)},
};

static const HookSlot kCurrencySlots[] = {
    {"GetMoneyId", 0x1238808, 0xF81E0FFE, "BagDataMgr", "GetMoneyId", 1,
     reinterpret_cast<void*>(Hooked_GetMoneyId), reinterpret_cast<void**>(&g_origGetMoneyId)},
    {"GetItemNum", 0x1239104, 0xD10103FF, "BagDataMgr", "GetItemNum", 2,
     reinterpret_cast<void*>(Hooked_GetItemNum), reinterpret_cast<void**>(&g_origGetItemNum)},
    // CheckConsume 有三个重载：只按 RVA 取 List<ItemInfo> 版本，禁止按名误选。
    {"CheckConsume(List)", 0x123B858, 0xF81C0FFE, nullptr, nullptr, 0,
     reinterpret_cast<void*>(Hooked_CheckConsumeList), reinterpret_cast<void**>(&g_origCheckConsume)},
};

// ============================================================
// ⑩ 分组安装 + Tick 懒装
// ============================================================
static bool EnsureSetLongValue() {
    if (g_setLongValue) return true;
    void* va = VaFromRva("LongRand.SetValue(long)", 0x144BBA4, 0xF81E0FFE);
    if (!va) return false;
    g_setLongValue = reinterpret_cast<LongRandSetValue_t>(va);
    LOGI(WDXSJ_LOG " LongRand.SetValue(long) @rva 0x144BBA4");
    return true;
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    if (!ResolveClasses()) return;

    if (!g_safetyHooked.load()) {
        const int ok = InstallSlots(kSafetySlots, sizeof(kSafetySlots) / sizeof(kSafetySlots[0]));
        if (ok == static_cast<int>(sizeof(kSafetySlots) / sizeof(kSafetySlots[0]))) {
            g_safetyHooked.store(true);
            LOGI(WDXSJ_LOG " safety hooks ready");
        }
    }

    if ((g_dmg.load() || g_inv.load()) && !g_combatHooked.load()) {
        const int ok = InstallSlots(kCombatSlots, sizeof(kCombatSlots) / sizeof(kCombatSlots[0]));
        if (ok == static_cast<int>(sizeof(kCombatSlots) / sizeof(kCombatSlots[0]))) {
            g_combatHooked.store(true);
            LOGI(WDXSJ_LOG " combat hooks ready");
        }
    }

    if (g_cur.load() && !g_currencyHooked.load()) {
        if (!EnsureSetLongValue()) return;
        const int ok = InstallSlots(kCurrencySlots, sizeof(kCurrencySlots) / sizeof(kCurrencySlots[0]));
        if (ok == static_cast<int>(sizeof(kCurrencySlots) / sizeof(kCurrencySlots[0]))) {
            g_currencyHooked.store(true);
            LOGI(WDXSJ_LOG " currency hooks ready");
        }
    }
}

// ============================================================
// ⑪ 对外接口（ModEntry.h 注册表契约）
// ============================================================
bool Wdxsj_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) {
        SyncHooks();
        return true;
    }
    if (!isLibraryLoaded(WDXSJ_TARGET_LIB)) return false;
    if (!LibBaseAddr()) return false;
    if (!EnsureApi()) return false;

    g_libReady.store(true);
    LOGI(WDXSJ_LOG " libil2cpp ready (lazy hooks)");
    SyncHooks();
    return true;
}

bool Wdxsj_IsReady() { return g_libReady.load(); }

void Wdxsj_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case WDXSJ_FEAT_DMG: g_dmg.store(enabled); break;
        case WDXSJ_FEAT_INV: g_inv.store(enabled); break;
        case WDXSJ_FEAT_CUR: g_cur.store(enabled); break;
        default: break;
    }
    SyncHooks();
}

void Wdxsj_OnValueChanged(int /*featNum*/, int /*value*/) {}

void Wdxsj_Tick(const bool* features, const int* /*values*/) {
    if (features) {
        g_dmg.store(features[WDXSJ_FEAT_DMG]);
        g_inv.store(features[WDXSJ_FEAT_INV]);
        g_cur.store(features[WDXSJ_FEAT_CUR]);
    }
    if (!g_libReady.load()) {
        if (isLibraryLoaded(WDXSJ_TARGET_LIB) && LibBaseAddr() && EnsureApi()) {
            g_libReady.store(true);
            LOGI(WDXSJ_LOG " libil2cpp ready (tick)");
        }
    }
    SyncHooks();
}

void Wdxsj_ForceClear() {
    g_dmg.store(false);
    g_inv.store(false);
    g_cur.store(false);
}
