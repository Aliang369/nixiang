/* ================================================================
 *  弓箭传说2国际版 (com.facai.archeroii) 1.0.1 — HybridCLR HotFix.BattleLogic
 *
 *  独立 game_id=gjc2intl；1.0.1 EntityManager 列表方法名与国服 1.1.8 不同
 *  文档：10_docs/弓箭传说2国际版/
 * ================================================================ */

#include "Archero2IntlMod.h"

#include <android/log.h>
#include <atomic>
#include <cstdint>
#include <cstring>
#include <dlfcn.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define GJC2INTL_TRACE(...) \
    __android_log_print(ANDROID_LOG_INFO, "StarryTrace", "[Gjc2Intl] " __VA_ARGS__)

typedef void* (*class_get_methods_t)(void* klass, void** iter);
typedef const char* (*method_get_name_t)(void* method);
typedef uint32_t (*method_get_param_count_t)(void* method);
typedef void* (*class_get_fields_t)(void* klass, void** iter);
typedef const char* (*field_get_name_t)(void* field);
typedef size_t (*field_get_offset_t)(void* field);

struct Il2CppIterApi {
    class_get_methods_t class_get_methods = nullptr;
    method_get_name_t method_get_name = nullptr;
    method_get_param_count_t method_get_param_count = nullptr;
    class_get_fields_t class_get_fields = nullptr;
    field_get_name_t field_get_name = nullptr;
    field_get_offset_t field_get_offset = nullptr;
    bool tried = false;
    bool ok = false;
};

static Il2CppIterApi g_iter;
static std::atomic<uint32_t> g_traceShow{0};
static std::atomic<uint32_t> g_traceAdd{0};
static std::atomic<uint32_t> g_traceExec{0};

static void* MethodPointer(void* mi);

static bool LoadIterApi() {
    if (g_iter.tried) return g_iter.ok;
    g_iter.tried = true;
    void* h = dlopen(GameUnity::TARGET_LIB, RTLD_NOLOAD);
    if (!h) h = dlopen(GameUnity::TARGET_LIB, RTLD_NOW);
    if (!h) return false;
    g_iter.class_get_methods = (class_get_methods_t)dlsym(h, "il2cpp_class_get_methods");
    g_iter.method_get_name = (method_get_name_t)dlsym(h, "il2cpp_method_get_name");
    g_iter.method_get_param_count =
        (method_get_param_count_t)dlsym(h, "il2cpp_method_get_param_count");
    g_iter.class_get_fields = (class_get_fields_t)dlsym(h, "il2cpp_class_get_fields");
    g_iter.field_get_name = (field_get_name_t)dlsym(h, "il2cpp_field_get_name");
    g_iter.field_get_offset = (field_get_offset_t)dlsym(h, "il2cpp_field_get_offset");
    g_iter.ok = g_iter.class_get_methods && g_iter.method_get_name && g_iter.class_get_fields &&
                g_iter.field_get_name && g_iter.field_get_offset;
    return g_iter.ok;
}

static void TraceDumpClass(void* klass, const char* tag) {
    if (!klass || !LoadIterApi()) {
        GJC2INTL_TRACE("DUMP %s skip (klass=%p iter=%d)", tag, klass, LoadIterApi() ? 1 : 0);
        return;
    }
    auto& api = GetApi();
    const char* cn = api.class_name ? api.class_name(klass) : "?";
    GJC2INTL_TRACE("DUMP %s class=%s", tag, cn ? cn : "?");

    void* iter = nullptr;
    int mc = 0;
    while (void* m = g_iter.class_get_methods(klass, &iter)) {
        const char* mn = g_iter.method_get_name(m);
        uint32_t pc = g_iter.method_get_param_count ? g_iter.method_get_param_count(m) : 99;
        if (mc < 100)
            GJC2INTL_TRACE("  M %s(%u) mi=%p mp=%p", mn ? mn : "?", pc, m, MethodPointer(m));
        mc++;
    }
    GJC2INTL_TRACE("  methods_total=%d", mc);

    iter = nullptr;
    int fc = 0;
    while (void* f = g_iter.class_get_fields(klass, &iter)) {
        const char* fn = g_iter.field_get_name(f);
        size_t off = g_iter.field_get_offset(f);
        if (fc < 100) GJC2INTL_TRACE("  F %s off=0x%zx", fn ? fn : "?", off);
        fc++;
    }
    GJC2INTL_TRACE("  fields_total=%d", fc);
}

static void* FindMethodByName(void* klass, const char* name, int maxArgc) {
    auto& api = GetApi();
    for (int argc = 0; argc <= maxArgc; argc++) {
        void* mi = api.GetMethod(klass, name, argc);
        if (mi) return mi;
    }
    return nullptr;
}

enum : size_t {
    kOffListItems = 0x10,
    kOffListSize  = 0x18,
    kOffArrayLen  = 0x18,
    kOffArrayData = 0x20,
    // TakeDamageData（dump.cs：含对象头）
    kOffDdAttackerType = 0x20,
    kOffDdHurtValue    = 0x30,
    kOffDdAttacker     = 0x38,  // EntityRef {Index, Version}
    // 1.0.1 国际版 StarryTrace 实锤（2026-08-23）
    kOffSlotSkillId    = 0x20,  // SelectSlotCommand.SkillId
    kOffSlotPlayerId   = 0x24,  // SelectSlotCommand.PlayerId
    kOffRsShowSkills   = 0x40,  // RandomSkillResult.<ShowSkills>
    kOffCtxTmpChars    = 0x10,  // BattleWorldContext._tmpCharacters
    kOffCtxEntity      = 0x188, // BattleWorldContext.<Entity>
    kOffHrsRandomResult = 0x78, // HeroComponentRandomSkill._randomSkillResult
    kOffHrsLastSkills  = 0xa0,  // HeroComponentRandomSkill._lastSkills
    // EntityManager（Trace 实锤）
    kOffEmEntityDic    = 0x10,  // _entityDic
    kOffEmWorldCtx     = 0x30,  // <WorldContext>k__BackingField
    kOffDictEntries    = 0x18,
    kOffDictCount      = 0x20,
    kDictEntrySize     = 0x18,
};

static size_t g_offDdAttackerType = 0x20;  // TakeDamageData，Install 时 Trace 校正

// 类名校验单槽缓存：热路径避免每帧走 il2cpp object_class+class_name。
// 只在 obj 类和上一次不同时才重新解析。
struct ClassNameCache {
    void* klass = nullptr;
    char  name[40] = {};
    bool  valid = false;
};

static ClassNameCache g_heroCache;
static ClassNameCache g_monsterCache;

static void* ObjClass(void* obj) {
    if (!obj || !Il2Cpp::IsValidHeapPtr(obj)) return nullptr;
    auto& api = GetApi();
    return api.object_class ? api.object_class(obj) : nullptr;
}

static const char* CacheClassName(ClassNameCache& cache, void* obj) {
    void* k = ObjClass(obj);
    if (!k) return nullptr;
    auto& api = GetApi();
    if (!api.class_name) return nullptr;
    if (cache.klass != k) {
        const char* n = api.class_name(k);
        cache.klass = k;
        cache.valid = false;
        if (n) {
            size_t i = 0;
            for (; i + 1 < sizeof(cache.name) && n[i]; i++) cache.name[i] = n[i];
            cache.name[i] = 0;
            cache.valid = i > 0;
        }
    }
    return cache.valid ? cache.name : nullptr;
}

static constexpr int kAoeMaxExtra = 40;

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_hooked{false};
static std::atomic<bool> g_steal{false};
static std::atomic<bool> g_invis{false};
static std::atomic<bool> g_aoe{false};
static std::atomic<bool> g_stealReenter{false};
static std::atomic<bool> g_aoeReenter{false};
static std::atomic<uint32_t> g_hitsSteal{0};
static std::atomic<uint32_t> g_hitsAoe{0};
static std::atomic<uint32_t> g_hitsInvis{0};

static std::atomic<void*> g_lastShowSkills{nullptr};
static std::atomic<void*> g_worldCtx{nullptr};
static std::atomic<void*> g_lastHero{nullptr};
static std::atomic<void*> g_lastHeroSkillComp{nullptr};
static std::atomic<void*> g_lastEm{nullptr};
static std::atomic<void*> g_cachedTmpList{nullptr};
static void* g_clsSlot = nullptr;

// 全屏：只扩散「已经走过 UpdateHp 且类名校验通过」的敌人，禁止猜 Dictionary/GetEntityValues
static constexpr int kRecentEnemyCap = 48;
static void* g_recentEnemies[kRecentEnemyCap]{};
static std::atomic<int> g_recentEnemyN{0};

static int32_t ReadI32(void* obj, size_t off) {
    if (!obj || !Il2Cpp::IsValidHeapPtr(obj)) return 0;
    return *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(obj) + off);
}

typedef void* (*ShowSkills_t)(void* self);
typedef bool (*AddSkill_t)(void* self, void* entity, int32_t skillId, int32_t addToComponent);
typedef bool (*Exec_t)(void* self);
typedef void (*SetTarget_t)(void* self, void* target);
typedef void (*UpdateHp_t)(void* self, void* damageData, bool noCheck, bool noCount, bool ignoreHpEvent);
typedef void* (*GetTmpCharList_t)(void* self);
typedef void* (*GetPlayer_t)(void* self, int32_t playerId);
typedef void* (*GetRandomSkillResult_t)(void* self);
typedef void (*FillSkillList_t)(void* self, void* list);

static ShowSkills_t g_origShowSkills = nullptr;
static AddSkill_t g_origAddSkill = nullptr;
static SetTarget_t g_origSetTarget = nullptr;
static SetTarget_t g_origTrySetTarget = nullptr;
static UpdateHp_t g_origUpdateHp = nullptr;
static GetTmpCharList_t g_getTmpCharList = nullptr;
static GetTmpCharList_t g_origGetTmpCharList = nullptr;
static GetTmpCharList_t g_getTmpCharList2 = nullptr;
static GetPlayer_t g_getPlayer = nullptr;
static GetRandomSkillResult_t g_origGetRandomSkillResult = nullptr;
static FillSkillList_t g_origFillSkillList = nullptr;
static Exec_t g_origExec = nullptr;
static void* g_clsEm = nullptr;
static std::atomic<uint32_t> g_traceHp{0};

static void* MethodPointer(void* mi) {
    return mi ? *reinterpret_cast<void**>(mi) : nullptr;
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

static void* FindBattle(const char* name) {
    void* k = FindClassNs("HotFix.BattleLogic", name);
    if (k) return k;
    return FindClassNs("", name);
}

static bool NameHas(const char* got, const char* needle) {
    if (!got || !needle) return false;
    for (size_t i = 0; got[i]; i++) {
        size_t j = 0;
        while (needle[j] && got[i + j] == needle[j]) j++;
        if (!needle[j]) return true;
    }
    return false;
}

static bool IsHeroObj(void* obj) {
    const char* n = CacheClassName(g_heroCache, obj);
    return n && NameHas(n, "Hero");
}

static bool IsEnemyChar(void* obj) {
    if (!obj || IsHeroObj(obj)) return false;
    const char* n = CacheClassName(g_monsterCache, obj);
    if (!n) return false;
    return NameHas(n, "Monster") || NameHas(n, "Enemy") || NameHas(n, "Summon");
}

static bool IsMonsterObj(void* obj) { return IsEnemyChar(obj); }

static bool IsValidSkillId(int32_t id) { return id >= 100 && id < 500000; }

// LGL 同款：写字段前校验对象指针，写死偏移时不做裸解引用
static void SafeWriteI32(void* obj, size_t off, int32_t v) {
    if (obj && Il2Cpp::IsValidHeapPtr(obj)) Il2Cpp::WriteI32(obj, off, v);
}
static void SafeWriteI64(void* obj, size_t off, int64_t v) {
    if (obj && Il2Cpp::IsValidHeapPtr(obj)) *(int64_t*)((uintptr_t)obj + off) = v;
}

static void* ReadWorldEntityMgr(void* ctx) {
    if (!ctx || !Il2Cpp::IsValidHeapPtr(ctx)) return nullptr;
    auto& api = GetApi();
    void* klass = api.object_class ? api.object_class(ctx) : nullptr;
    size_t off = 0;
    if (klass) {
        off = api.FieldOffset(klass, "<Entity>k__BackingField", 0);
        if (!off) off = api.FieldOffset(klass, "_Entity_k__BackingField", 0);
    }
    if (!off) off = kOffCtxEntity;
    return Il2Cpp::ReadPtr(ctx, off);
}

static void* ResolveWorldCtx() {
    void* ctx = g_worldCtx.load();
    if (ctx && Il2Cpp::IsValidHeapPtr(ctx)) return ctx;
    void* em = g_lastEm.load();
    if (!em) return nullptr;
    void* wc = Il2Cpp::ReadPtr(em, kOffEmWorldCtx);
    if (wc && Il2Cpp::IsValidHeapPtr(wc)) return wc;
    return nullptr;
}

static void* ResolveEntityMgr() {
    void* em = g_lastEm.load();
    if (em && Il2Cpp::IsValidHeapPtr(em)) return em;
    void* ctx = ResolveWorldCtx();
    return ctx ? ReadWorldEntityMgr(ctx) : nullptr;
}

static void CacheShowSkillsFromResult(void* result) {
    if (!result || !Il2Cpp::IsValidHeapPtr(result)) return;
    void* arr = Il2Cpp::ReadPtr(result, kOffRsShowSkills);
    if (!arr && g_origShowSkills) arr = g_origShowSkills(result);
    if (arr && Il2Cpp::IsValidHeapPtr(arr)) g_lastShowSkills.store(arr);
}

static void* ResolveHeroFromSlot(void* cmd) {
    if (!cmd) return g_lastHero.load();
    void* hero = g_lastHero.load();
    void* ctx = g_worldCtx.load();
    if (!ctx) return hero;
    void* em = ReadWorldEntityMgr(ctx);
    if (em && g_getPlayer) {
        int32_t pid = ReadI32(cmd, kOffSlotPlayerId);
        void* p = g_getPlayer(em, pid);
        if (p && IsHeroObj(p)) {
            g_lastHero.store(p);
            return p;
        }
    }
    return hero;
}

static bool LooksLikePlayerHit(void* damageData) {
    if (!damageData || !Il2Cpp::IsValidHeapPtr(damageData)) return false;
    int32_t at = *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(damageData) +
                                               g_offDdAttackerType);
    return at == 1 || at == 3 || at == 4;
}

static void* ReadFieldPtr(void* obj, void* klass, const char* const* names, size_t fallbackOff) {
    if (!obj || !Il2Cpp::IsValidHeapPtr(obj)) return nullptr;
    auto& api = GetApi();
    if (klass) {
        for (const char* const* p = names; *p; ++p) {
            size_t off = api.FieldOffset(klass, *p, GameUnity::HYBRIDCLR_FIELD_BIAS);
            if (off) {
                void* v = Il2Cpp::ReadPtr(obj, off);
                if (v && Il2Cpp::IsValidHeapPtr(v)) return v;
            }
        }
    }
    if (fallbackOff) {
        void* v = Il2Cpp::ReadPtr(obj, fallbackOff);
        if (v && Il2Cpp::IsValidHeapPtr(v)) return v;
    }
    return nullptr;
}

static int32_t ReadInt32ArrayLen(void* arr) {
    if (!arr || !Il2Cpp::IsValidHeapPtr(arr)) return 0;
    return *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(arr) + kOffArrayLen);
}

static int32_t ReadInt32ArrayAt(void* arr, int i) {
    if (!arr || !Il2Cpp::IsValidHeapPtr(arr) || i < 0) return 0;
    uintptr_t data = reinterpret_cast<uintptr_t>(arr) + kOffArrayData;
    return *reinterpret_cast<int32_t*>(data + static_cast<size_t>(i) * 4u);
}

static bool LooksLikeSkillIdArray(void* arr) {
    if (!arr || !Il2Cpp::IsValidHeapPtr(arr)) return false;
    int32_t len = ReadInt32ArrayLen(arr);
    if (len < 2 || len > 4) return false;
    for (int i = 0; i < len; i++) {
        if (!IsValidSkillId(ReadInt32ArrayAt(arr, i))) return false;
    }
    return true;
}

static void TryCacheShowSkillsArray(void* arr) {
    if (LooksLikeSkillIdArray(arr)) g_lastShowSkills.store(arr);
}

static void CacheShowSkillsFromHeroComp(void* comp) {
    if (!comp || !Il2Cpp::IsValidHeapPtr(comp)) return;
    g_lastHeroSkillComp.store(comp);
    void* result = Il2Cpp::ReadPtr(comp, kOffHrsRandomResult);
    if (result) CacheShowSkillsFromResult(result);
    if (!g_lastShowSkills.load()) {
        void* last = Il2Cpp::ReadPtr(comp, kOffHrsLastSkills);
        TryCacheShowSkillsArray(last);
    }
}

static void RefreshShowSkillsBeforeExec(void* cmd) {
    (void)cmd;
    void* comp = g_lastHeroSkillComp.load();
    if (comp) CacheShowSkillsFromHeroComp(comp);
}

static void StealExtraSkillsInAdd(void* ctx, void* entity, int32_t pickedId, int32_t addToComponent) {
    if (!g_steal.load() || g_stealReenter.load() || !g_origAddSkill || !ctx || !entity) return;

    void* arr = g_lastShowSkills.load();
    if (!LooksLikeSkillIdArray(arr)) return;

    int32_t len = ReadInt32ArrayLen(arr);
    g_stealReenter.store(true);
    int added = 0;
    for (int i = 0; i < len; i++) {
        int32_t id = ReadInt32ArrayAt(arr, i);
        if (!IsValidSkillId(id) || id == pickedId) continue;
        g_origAddSkill(ctx, entity, id, addToComponent);
        added++;
    }
    g_stealReenter.store(false);
    if (added > 0) {
        uint32_t n = g_hitsSteal.fetch_add(1) + 1;
        if (n <= 12 || (n % 20) == 0)
            LOGI("[Gjc2Intl] steal skills +%d (picked=%d hits=%u)", added, pickedId, n);
    }
}

static bool ValidateEntityList(void* list) {
    if (!list || !Il2Cpp::IsValidHeapPtr(list)) return false;
    int32_t size = *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(list) + kOffListSize);
    if (size < 1 || size > 500) return false;
    void* items = Il2Cpp::ReadPtr(list, kOffListItems);
    return items && Il2Cpp::IsValidHeapPtr(items);
}

static void RememberEnemy(void* ent) {
    if (!ent || !Il2Cpp::IsValidHeapPtr(ent) || !IsEnemyChar(ent)) return;
    int n = g_recentEnemyN.load();
    for (int i = 0; i < n && i < kRecentEnemyCap; i++) {
        if (g_recentEnemies[i] == ent) return;
    }
    if (n >= kRecentEnemyCap) {
        // 环形覆盖最旧
        static std::atomic<int> sSlot{0};
        int slot = sSlot.fetch_add(1) % kRecentEnemyCap;
        g_recentEnemies[slot] = ent;
        return;
    }
    g_recentEnemies[n] = ent;
    g_recentEnemyN.store(n + 1);
}

static void* GetAoeCharacterList(void* ctx) {
    void* cached = g_cachedTmpList.load();
    if (ValidateEntityList(cached)) return cached;

    if (ctx && Il2Cpp::IsValidHeapPtr(ctx)) {
        // 只读字段，不调可能有副作用的 Fill
        void* list = Il2Cpp::ReadPtr(ctx, kOffCtxTmpChars);
        if (ValidateEntityList(list)) {
            g_cachedTmpList.store(list);
            return list;
        }
        list = Il2Cpp::ReadPtr(ctx, kOffCtxTmpChars + 8);  // _tmpCharacters2@0x18
        if (ValidateEntityList(list)) {
            g_cachedTmpList.store(list);
            return list;
        }
    }
    return nullptr;
}

static int SpreadAoeViaList(void* list, void* self, void* damageData, bool noCheck, bool noCount,
                            bool ignoreHpEvent) {
    if (!list || !ValidateEntityList(list) || !g_origUpdateHp) return 0;
    int32_t size = *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(list) + kOffListSize);
    void* items = Il2Cpp::ReadPtr(list, kOffListItems);
    if (!items || !Il2Cpp::IsValidHeapPtr(items)) return 0;

    int spread = 0;
    uintptr_t base = reinterpret_cast<uintptr_t>(items) + kOffArrayData;
    for (int i = 0; i < size && spread < kAoeMaxExtra; i++) {
        void* ent = *reinterpret_cast<void**>(base + static_cast<size_t>(i) * 8u);
        if (!ent || ent == self || !Il2Cpp::IsValidHeapPtr(ent)) continue;
        if (!IsEnemyChar(ent)) continue;
        RememberEnemy(ent);
        g_origUpdateHp(ent, damageData, noCheck, noCount, ignoreHpEvent);
        spread++;
    }
    return spread;
}

static int SpreadAoeViaRecent(void* self, void* damageData, bool noCheck, bool noCount,
                              bool ignoreHpEvent) {
    if (!g_origUpdateHp) return 0;
    int n = g_recentEnemyN.load();
    if (n < 1) return 0;
    if (n > kRecentEnemyCap) n = kRecentEnemyCap;

    int spread = 0;
    for (int i = 0; i < n && spread < kAoeMaxExtra; i++) {
        void* ent = g_recentEnemies[i];
        if (!ent || ent == self || !Il2Cpp::IsValidHeapPtr(ent)) continue;
        if (!IsEnemyChar(ent)) continue;
        g_origUpdateHp(ent, damageData, noCheck, noCount, ignoreHpEvent);
        spread++;
    }
    return spread;
}

static int SpreadAoeDamage(void* self, void* damageData, bool noCheck, bool noCount,
                           bool ignoreHpEvent) {
    void* ctx = ResolveWorldCtx();
    void* list = GetAoeCharacterList(ctx);
    int spread = SpreadAoeViaList(list, self, damageData, noCheck, noCount, ignoreHpEvent);
    if (spread > 0) return spread;
    return SpreadAoeViaRecent(self, damageData, noCheck, noCount, ignoreHpEvent);
}

static void* Hooked_get_TmpEntityCharacterList(void* self) {
    void* list = g_origGetTmpCharList ? g_origGetTmpCharList(self) : nullptr;
    if (ValidateEntityList(list)) g_cachedTmpList.store(list);
    return list;
}

static void* Hooked_get_ShowSkills(void* self) {
    void* arr = g_origShowSkills ? g_origShowSkills(self) : nullptr;
    if (arr) g_lastShowSkills.store(arr);
    uint32_t n = g_traceShow.fetch_add(1);
    if (n < 12) {
        int32_t len = arr ? ReadInt32ArrayLen(arr) : -1;
        GJC2INTL_TRACE("ShowSkills#%u self=%p arr=%p len=%d", n, self, arr, len);
        if (arr && len > 0 && len <= 8) {
            for (int i = 0; i < len; i++)
                GJC2INTL_TRACE("  skill[%d]=%d", i, ReadInt32ArrayAt(arr, i));
        }
    }
    return arr;
}

static bool Hooked_AddSkillToCharacter(void* self, void* entity, int32_t skillId,
                                       int32_t addToComponent) {
    if (self) g_worldCtx.store(self);
    void* em = self ? ReadWorldEntityMgr(self) : nullptr;
    if (em) g_lastEm.store(em);
    if (entity && IsHeroObj(entity)) g_lastHero.store(entity);
    uint32_t n = g_traceAdd.fetch_add(1);
    if (n < 12) {
        const char* en = CacheClassName(g_heroCache, entity);
        void* arr = g_lastShowSkills.load();
        int32_t alen = arr ? ReadInt32ArrayLen(arr) : -1;
        GJC2INTL_TRACE("AddSkill#%u ctx=%p ent=%p(%s) id=%d add=%d arrLen=%d steal=%d", n, self,
                       entity, en ? en : "?", skillId, addToComponent, alen,
                       g_steal.load() ? 1 : 0);
    }
    return g_origAddSkill ? g_origAddSkill(self, entity, skillId, addToComponent) : false;
}

static void* Hooked_get_RandomSkillResult(void* self) {
    void* result = g_origGetRandomSkillResult ? g_origGetRandomSkillResult(self) : nullptr;
    if (self) g_lastHeroSkillComp.store(self);
    CacheShowSkillsFromResult(result);
    return result;
}

static void Hooked_FillSkillList(void* self, void* list) {
    if (g_origFillSkillList) g_origFillSkillList(self, list);
    if (self) CacheShowSkillsFromHeroComp(self);
}

static bool Hooked_SelectSlotExec(void* self) {
    uint32_t n = g_traceExec.fetch_add(1);
    if (g_steal.load() && self) RefreshShowSkillsBeforeExec(self);
    if (n < 12) {
        void* arr = g_lastShowSkills.load();
        int32_t alen = arr ? ReadInt32ArrayLen(arr) : -1;
        GJC2INTL_TRACE("Exec#%u self=%p skillId=%d arrLen=%d steal=%d", n, self,
                       self ? ReadI32(self, kOffSlotSkillId) : 0, alen, g_steal.load() ? 1 : 0);
    }
    bool ok = g_origExec ? g_origExec(self) : false;
    if (g_steal.load() && self) {
        int32_t picked = ReadI32(self, kOffSlotSkillId);
        void* ctx = g_worldCtx.load();
        void* hero = ResolveHeroFromSlot(self);
        if (ctx && hero) StealExtraSkillsInAdd(ctx, hero, picked, 1);
    }
    return ok;
}

static void Hooked_SetTarget(void* self, void* target) {
    if (self && IsEnemyChar(self)) RememberEnemy(self);
    if (target && IsEnemyChar(target)) RememberEnemy(target);
    if (g_invis.load() && IsHeroObj(target)) {
        uint32_t n = g_hitsInvis.fetch_add(1) + 1;
        if (n <= 8 || (n % 40) == 0) LOGI("[Gjc2Intl] invis block SetTarget hits=%u", n);
        if (g_origSetTarget) g_origSetTarget(self, nullptr);
        return;
    }
    if (g_origSetTarget) g_origSetTarget(self, target);
}

static void Hooked_TrySetTarget(void* self, void* target) {
    if (self && IsEnemyChar(self)) RememberEnemy(self);
    if (target && IsEnemyChar(target)) RememberEnemy(target);
    if (g_invis.load() && IsHeroObj(target)) {
        if (g_origTrySetTarget) g_origTrySetTarget(self, nullptr);
        return;
    }
    if (g_origTrySetTarget) g_origTrySetTarget(self, target);
}

static void Hooked_UpdateHp(void* self, void* damageData, bool noCheck, bool noCount,
                            bool ignoreHpEvent) {
    // 隐身兜底：英雄 HurtValue 归零仍走 orig
    if (g_invis.load() && IsHeroObj(self) && damageData && Il2Cpp::IsValidHeapPtr(damageData)) {
        SafeWriteI64(damageData, kOffDdHurtValue, 0);
    }

    // 任何敌人受击都记入近期列表（全屏扩散用，不猜字典）
    if (!g_aoeReenter.load() && IsEnemyChar(self)) RememberEnemy(self);

    const bool aoeOn = g_aoe.load();
    const bool enemy = IsEnemyChar(self);
    const bool playerHit = LooksLikePlayerHit(damageData);
    if (aoeOn && !g_aoeReenter.load()) {
        uint32_t hn = g_traceHp.fetch_add(1);
        if (hn < 12) {
            int32_t at = damageData ? ReadI32(damageData, g_offDdAttackerType) : -999;
            const char* cn = CacheClassName(g_monsterCache, self);
            if (!cn) cn = CacheClassName(g_heroCache, self);
            GJC2INTL_TRACE("Hp#%u self=%s enemy=%d at=%d playerHit=%d recent=%d", hn,
                           cn ? cn : "?", enemy ? 1 : 0, at, playerHit ? 1 : 0,
                           g_recentEnemyN.load());
        }
    }

    if (!aoeOn || g_aoeReenter.load() || !g_origUpdateHp || !enemy || !playerHit) {
        if (g_origUpdateHp) g_origUpdateHp(self, damageData, noCheck, noCount, ignoreHpEvent);
        return;
    }

    g_aoeReenter.store(true);
    g_origUpdateHp(self, damageData, noCheck, noCount, ignoreHpEvent);

    void* ctx0 = ResolveWorldCtx();
    void* list0 = GetAoeCharacterList(ctx0);
    int32_t listSz = list0 ? *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(list0) +
                                                          kOffListSize)
                           : -1;
    int spread = SpreadAoeDamage(self, damageData, noCheck, noCount, ignoreHpEvent);
    g_aoeReenter.store(false);

    static std::atomic<uint32_t> sAoeLog{0};
    uint32_t nlog = sAoeLog.fetch_add(1);
    if (nlog < 8 || spread > 0) {
        const char* cn = CacheClassName(g_monsterCache, self);
        LOGI("[Gjc2Intl] aoe listSz=%d recent=%d spread=%d self=%s", listSz, g_recentEnemyN.load(),
             spread, cn ? cn : "?");
    }
    if (spread > 0) {
        uint32_t n = g_hitsAoe.fetch_add(1) + 1;
        if (n <= 10 || (n % 40) == 0) LOGI("[Gjc2Intl] aoe spread=%d bursts=%u", spread, n);
    }
}

static bool InstallHooks() {
    if (g_hooked.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;
    auto& apiLoad = GetApi();
    if (!apiLoad.Load(GameUnity::TARGET_LIB)) {
        LOGE("[Gjc2Intl] Il2Cpp Api::Load failed");
        return false;
    }

    void* clsShow = FindBattle("RandomSkillResult");
    void* clsCtx = FindBattle("BattleWorldContext");
    void* clsEc = FindBattle("EntityCharacter");
    void* clsEm = FindBattle("EntityManager");
    void* clsSlot = FindBattle("SelectSlotCommand");
    void* clsHeroSkill = FindBattle("HeroComponentRandomSkill");
    g_clsSlot = clsSlot;
    g_clsEm = clsEm;
    if (!clsShow || !clsCtx || !clsEc || !clsEm) {
        LOGE("[Gjc2Intl] class missing show=%p ctx=%p ec=%p em=%p slot=%p heroSkill=%p (HotFix 未就绪)",
             clsShow, clsCtx, clsEc, clsEm, clsSlot, clsHeroSkill);
        return false;
    }

    TraceDumpClass(clsEm, "EntityManager");
    TraceDumpClass(clsCtx, "BattleWorldContext");
    TraceDumpClass(clsShow, "RandomSkillResult");
    if (clsSlot) TraceDumpClass(clsSlot, "SelectSlotCommand");
    if (clsHeroSkill) TraceDumpClass(clsHeroSkill, "HeroComponentRandomSkill");

    auto& api = GetApi();
    void* miShow = FindMethodByName(clsShow, "get_ShowSkills", 0);
    void* miAdd = FindMethodByName(clsCtx, "AddSkillToCharacter", 4);
    void* miSet = api.GetMethod(clsEc, "SetTarget", 1);
    void* miTry = api.GetMethod(clsEc, "TrySetTarget", 1);
    void* miHp = api.GetMethod(clsEc, "UpdateHp", 4);
    void* miExec = clsSlot ? FindMethodByName(clsSlot, "Exec", 0) : nullptr;
    void* miTmpList = api.GetMethod(clsCtx, "get_TmpEntityCharacterList", 0);
    void* miTmpList2 = api.GetMethod(clsCtx, "get_TmpEntityCharacterList2", 0);
    void* miGetPlayer = api.GetMethod(clsEm, "GetPlayer", 1);
    void* miRsr = clsHeroSkill ? FindMethodByName(clsHeroSkill, "get_RandomSkillResult", 0) : nullptr;
    void* miFill = clsHeroSkill ? FindMethodByName(clsHeroSkill, "FillSkillList", 1) : nullptr;
    void* clsDd = FindBattle("TakeDamageData");

    void* fnShow = MethodPointer(miShow);
    void* fnAdd = MethodPointer(miAdd);
    void* fnSet = MethodPointer(miSet);
    void* fnTry = MethodPointer(miTry);
    void* fnHp = MethodPointer(miHp);
    void* fnExec = MethodPointer(miExec);
    void* fnTmpList = MethodPointer(miTmpList);
    void* fnTmpList2 = MethodPointer(miTmpList2);
    void* fnGetPlayer = MethodPointer(miGetPlayer);
    void* fnRsr = MethodPointer(miRsr);
    void* fnFill = MethodPointer(miFill);
    if (clsDd) TraceDumpClass(clsDd, "TakeDamageData");
    if (LoadIterApi() && clsDd) {
        void* fiter = nullptr;
        while (void* f = g_iter.class_get_fields(clsDd, &fiter)) {
            const char* fn = g_iter.field_get_name(f);
            if (!fn) continue;
            if (NameHas(fn, "AttackerType") || NameHas(fn, "HurtAttackerType")) {
                g_offDdAttackerType = g_iter.field_get_offset(f);
                LOGI("[Gjc2Intl] TakeDamageData %s off=0x%zx", fn, g_offDdAttackerType);
                break;
            }
        }
    }
    if (!fnShow || !fnAdd || !fnSet || !fnTry || !fnHp) {
        LOGE("[Gjc2Intl] method missing show=%p add=%p set=%p try=%p hp=%p exec=%p tmpList=%p",
             fnShow, fnAdd, fnSet, fnTry, fnHp, fnExec, fnTmpList);
        return false;
    }
    if (fnTmpList) {
        g_getTmpCharList = reinterpret_cast<GetTmpCharList_t>(fnTmpList);
        if (DobbyHook(fnTmpList, (void*)Hooked_get_TmpEntityCharacterList,
                      (void**)&g_origGetTmpCharList) != 0) {
            g_origGetTmpCharList = g_getTmpCharList;
            LOGW("[Gjc2Intl] Dobby get_TmpEntityCharacterList cache hook failed");
        }
        LOGI("[Gjc2Intl] aoe list get_TmpEntityCharacterList -> %p", fnTmpList);
    } else {
        LOGW("[Gjc2Intl] get_TmpEntityCharacterList missing — 全屏用 _entityDic");
    }
    if (fnTmpList2) {
        g_getTmpCharList2 = reinterpret_cast<GetTmpCharList_t>(fnTmpList2);
        LOGI("[Gjc2Intl] aoe list get_TmpEntityCharacterList2 -> %p", fnTmpList2);
    }
    if (fnGetPlayer) {
        g_getPlayer = reinterpret_cast<GetPlayer_t>(fnGetPlayer);
    } else {
        LOGW("[Gjc2Intl] EntityManager.GetPlayer missing — 偷技能用 g_lastHero");
    }
    if (!fnExec) LOGW("[Gjc2Intl] SelectSlotCommand.Exec missing");

    if (DobbyHook(fnShow, (void*)Hooked_get_ShowSkills, (void**)&g_origShowSkills) != 0) {
        LOGE("[Gjc2Intl] Dobby get_ShowSkills failed");
        return false;
    }
    if (DobbyHook(fnAdd, (void*)Hooked_AddSkillToCharacter, (void**)&g_origAddSkill) != 0) {
        LOGE("[Gjc2Intl] Dobby AddSkillToCharacter failed");
        return false;
    }
    if (DobbyHook(fnSet, (void*)Hooked_SetTarget, (void**)&g_origSetTarget) != 0) {
        LOGE("[Gjc2Intl] Dobby SetTarget failed");
        return false;
    }
    if (DobbyHook(fnTry, (void*)Hooked_TrySetTarget, (void**)&g_origTrySetTarget) != 0) {
        LOGE("[Gjc2Intl] Dobby TrySetTarget failed");
        return false;
    }
    if (DobbyHook(fnHp, (void*)Hooked_UpdateHp, (void**)&g_origUpdateHp) != 0) {
        LOGE("[Gjc2Intl] Dobby UpdateHp failed");
        return false;
    }
    if (fnExec &&
        DobbyHook(fnExec, (void*)Hooked_SelectSlotExec, (void**)&g_origExec) != 0) {
        LOGE("[Gjc2Intl] Dobby SelectSlotCommand.Exec failed");
        return false;
    }
    if (fnRsr &&
        DobbyHook(fnRsr, (void*)Hooked_get_RandomSkillResult, (void**)&g_origGetRandomSkillResult) !=
            0) {
        LOGE("[Gjc2Intl] Dobby get_RandomSkillResult failed");
        return false;
    }
    if (fnFill &&
        DobbyHook(fnFill, (void*)Hooked_FillSkillList, (void**)&g_origFillSkillList) != 0) {
        LOGE("[Gjc2Intl] Dobby FillSkillList failed");
        return false;
    }
    g_hooked.store(true);
    g_ready.store(true);
    LOGI("[Gjc2Intl] hooked ShowSkills/AddSkill/Exec/Rsr/Fill/TmpList/SetTarget/UpdateHp tmpList=%p exec=%p",
         fnTmpList, fnExec);
    return true;
}

bool Gjc2Intl_TryInit() { return InstallHooks(); }

bool Gjc2Intl_IsReady() { return g_ready.load(); }

void Gjc2Intl_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == GJC2INTL_FEAT_STEAL_SKILL) g_steal.store(enabled);
    if (featNum == GJC2INTL_FEAT_INVIS) g_invis.store(enabled);
    if (featNum == GJC2INTL_FEAT_FULLSCREEN) g_aoe.store(enabled);
    LOGI("[Gjc2Intl] feat %d -> %d", featNum, enabled ? 1 : 0);
}

void Gjc2Intl_OnValueChanged(int /*featNum*/, int /*value*/) {}

void Gjc2Intl_Tick(const bool* features) {
    if (!features) return;
    g_steal.store(features[GJC2INTL_FEAT_STEAL_SKILL]);
    g_invis.store(features[GJC2INTL_FEAT_INVIS]);
    g_aoe.store(features[GJC2INTL_FEAT_FULLSCREEN]);
    if (!g_ready.load()) InstallHooks();
}

void Gjc2Intl_ForceClear() {
    g_steal.store(false);
    g_invis.store(false);
    g_aoe.store(false);
}
