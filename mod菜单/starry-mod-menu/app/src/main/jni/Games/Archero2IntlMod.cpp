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
    // 注意：**不要**追加 il2cpp_field_get_type / il2cpp_type_get_name —— 2026-10-17 实测
    // 在 install 期调用会让 il2cpp 自己 abort（本作 `04:59:06.388 Fatal signal 6 (SIGABRT)`
    // 发生在 mod 线程），与仓库已记录的「半初始化窗口调 il2cpp API 会 abort」同源。
    // 字段类型判定改用「读原始字节 + 多解释打印」的纯内存方式，勿再引入新 API。
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

// ---- 2026-10-17 新增：秒杀 / 战斗加速 ----
// （原「攻速倍率」滑块按用户 2026-10-17 指定**整项移除**：战斗加速已涵盖攻速。
//   该功能的挂点作废记录见下方「攻速倍率 —— 功能已移除」小节，**不得再以 Dobby 直挂热更属性取值口**）
static std::atomic<bool> g_oneshot{false};
static std::atomic<uint32_t> g_hitsOneshot{0};
static std::atomic<int> g_battleSpeed{1};  // 1 = 关；2..10 = 倍率

// 懒装节流：≥1s 间隔、最多 60 次（防热更未就绪窗口反复全 image 扫描 → SIGSEGV）
static std::atomic<int> g_initTick{0};
static std::atomic<int> g_initAttempts{0};

// 秒杀：致死伤害量级。**必须是负数** —— 2026-10-17 探针实锤本作 `TakeDamageData.HurtValue`
// 是 int64 且**伤害以负值存放**（秒杀关闭时原始样本 `0xffffffffffffff9c` = i64 -100 /
// `0xffffffffffffff6a` = i64 -150，f32 解释为 -nan ⇒ 不是 float），游戏按 `Hp += HurtValue`
// 结算。写成正值等于**给怪物回血**（用户实测现象「打怪有伤害但会自动回血」即此因）。
static constexpr int64_t kOneshotDamage = -99999999;
// 战斗活跃窗口（帧）。战斗钩子每命中一次就盖章，超窗即判「已离开战斗」⇒ 还原 timeScale。
static constexpr uint64_t kBattleIdleFrames = 600;   // ≈10s @60fps

// 战斗加速：钉 UnityEngine.Time.set_timeScale + get_deltaTime 每帧心跳。
// 只钉「战斗内」（用户 2026-10-17 指定仅战斗内加速）：战斗活跃度由本作已有战斗钩子盖章。
static std::atomic<uint64_t> g_frameTick{0};        // get_deltaTime 帧计数
static std::atomic<uint64_t> g_lastBattleFrame{0};  // 最近一次战斗活动帧
static std::atomic<bool> g_battleSeen{false};       // 是否已真进过战斗（门控热身保护）
static std::atomic<int> g_pinnedScale{0};           // 已钉住的倍率（0 = 未钉）
static void* g_clsTime = nullptr;
static void* g_miSetTimeScale = nullptr;

static inline void StampBattleActivity() {
    g_battleSeen.store(true, std::memory_order_relaxed);
    g_lastBattleFrame.store(g_frameTick.load(std::memory_order_relaxed), std::memory_order_relaxed);
}

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
// Unity 静态方法：编译后签名带尾参 methodInfo（ABI 契约，见 hook-rules）
typedef void (*SetTimeScale_t)(float value, void* method);
typedef float (*GetDeltaTime_t)(void* method);

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
static SetTimeScale_t g_origSetTimeScale = nullptr;    // UnityEngine.Time.set_timeScale
static GetDeltaTime_t g_origGetDeltaTime = nullptr;    // UnityEngine.Time.get_deltaTime
static SetTimeScale_t g_setTimeScaleDirect = nullptr;  // 兜底直呼入口（set 槽装钩失败时用）
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
    // 战斗加速门控主力信号：该 getter 只在对局内被战斗代码取用
    StampBattleActivity();
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
    StampBattleActivity();
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
    StampBattleActivity();
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
    StampBattleActivity();
#ifdef GJC2INTL_PROBE
    // 伤害通道取证（纯内存只读；**读在任何写入之前**）。
    // 注意：damageData 可能对象池复用，上一次写入会残留 ⇒ 取「游戏原始值」必须让**秒杀=关**。
    // 目的：① 定 HurtValue@0x30 真实类型；② 定敌人当前血量字段（按名解析，不信硬编码 0xa0）
    if (damageData && Il2Cpp::IsValidHeapPtr(damageData) && IsEnemyChar(self) &&
        LooksLikePlayerHit(damageData)) {
        static std::atomic<uint32_t> sRaw{0};
        uint32_t rn = sRaw.fetch_add(1);
        if (rn < 12) {
            const uint8_t* p = reinterpret_cast<const uint8_t*>(damageData) + kOffDdHurtValue;
            uint64_t raw = 0;
            memcpy(&raw, p, 8);
            int32_t lo32 = 0;
            memcpy(&lo32, p, 4);
            int32_t hi32 = 0;
            memcpy(&hi32, p + 4, 4);
            float hf = 0.f;
            memcpy(&hf, p, 4);
            int64_t h64 = 0;
            memcpy(&h64, p, 8);
            double hd = 0.0;
            memcpy(&hd, p, 8);
            static size_t s_offHp = 0;
            static bool s_hpTried = false;
            if (!s_hpTried) {
                s_hpTried = true;
                auto& apiH = GetApi();
                void* kSelf = apiH.object_class ? apiH.object_class(self) : nullptr;
                if (kSelf) s_offHp = apiH.FieldOffset(kSelf, "CurrentHp", 0);
            }
            const size_t offHp = s_offHp ? s_offHp : 0xa0;
            int32_t hpI = 0;
            float hpF = 0.f;
            if (Il2Cpp::IsValidHeapPtr(self)) {
                memcpy(&hpI, reinterpret_cast<const uint8_t*>(self) + offHp, 4);
                memcpy(&hpF, reinterpret_cast<const uint8_t*>(self) + offHp, 4);
            }
            const char* ecn = CacheClassName(g_monsterCache, self);
            GJC2INTL_TRACE("RAW#%u hurtval=0x%016llx lo32=%d hi32=%d f32=%.6g i64=%lld f64=%.6g "
                           "| enemy=%s hp@0x%zx i32=%d f32=%.6g | oneshot=%d",
                           rn, (unsigned long long)raw, lo32, hi32, hf, (long long)h64, hd,
                           ecn ? ecn : "?", offHp, hpI, hpF, g_oneshot.load() ? 1 : 0);
        }
    }
#endif
    // 隐身兜底：英雄 HurtValue 归零仍走 orig
    if (g_invis.load() && IsHeroObj(self) && damageData && Il2Cpp::IsValidHeapPtr(damageData)) {
        SafeWriteI64(damageData, kOffDdHurtValue, 0);
    }

    // 秒杀：玩家命中敌人 ⇒ 把 HurtValue 抬到致死量级（与本函数既有敌我判定同源，
    // 不新增挂点；全屏扩散复用同一份 damageData，因此对扩散目标同样致死）。
    if (g_oneshot.load() && damageData && Il2Cpp::IsValidHeapPtr(damageData) &&
        IsEnemyChar(self) && LooksLikePlayerHit(damageData)) {
        SafeWriteI64(damageData, kOffDdHurtValue, kOneshotDamage);
        uint32_t n = g_hitsOneshot.fetch_add(1) + 1;
        if (n <= 10 || (n % 40) == 0) {
            LOGI("[Gjc2Intl] oneshot dmg=%lld hits=%u", (long long)kOneshotDamage, n);
        }
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

// ============================================================
//  战斗加速（仅战斗内）：UnityEngine.Time.set_timeScale 钉住 + get_deltaTime 心跳
// ============================================================

/**
 * 战斗活跃判定：战斗钩子盖章后 kBattleIdleFrames 帧内视为在对局中。
 * 帧计数只在 get_deltaTime 心跳里走，因此**心跳未装上时一律判「不在战斗」**
 * （宁可不加速，也不把菜单/主城一起加速）。
 * 另加 g_battleSeen 热身保护：**没真进过战斗就绝不加速** —— 否则开机前
 * kBattleIdleFrames（≈10s）窗口会被误判成战斗中，把主界面一起加速
 * （2026-10-17 实测 `bspeed pin x4 frame=1` 即此 bug）。
 */
static inline bool InBattleWindow() {
    if (!g_origGetDeltaTime) return false;
    if (!g_battleSeen.load(std::memory_order_relaxed)) return false;
    return (g_frameTick.load(std::memory_order_relaxed) -
            g_lastBattleFrame.load(std::memory_order_relaxed)) < kBattleIdleFrames;
}

/** 游戏调用 set_timeScale 时：战斗内把倍率换成我们的，其余一律透传。 */
static void Hooked_SetTimeScale(float value, void* method) {
    if (method && !g_miSetTimeScale) g_miSetTimeScale = method;  // 心跳复用真实 MethodInfo
    const int sp = g_battleSpeed.load();
    if (sp > 1 && value > 0.0f && InBattleWindow()) {
        // 只改正值；游戏自己写 <=0（暂停/冻结）时透传，不跟它抢暂停
        g_pinnedScale.store(sp);
        if (g_origSetTimeScale) g_origSetTimeScale((float)sp, method);
        return;
    }
    if (g_pinnedScale.exchange(0) > 0) {
        LOGI("[Gjc2Intl] bspeed restore (game set %.2f)", value);
    }
    if (g_origSetTimeScale) g_origSetTimeScale(value, method);
}

/**
 * 每帧心跳（get_deltaTime 每帧必被调用且在主线程）：
 *   ① 帧计数 ② 战斗活跃判定 ③ 钉住/还原 timeScale。
 * 借它做到「拖滑块立刻生效」「离开战斗立刻还原」，不依赖游戏再调一次 setter。
 */
static float Hooked_GetDeltaTime(void* method) {
    const uint64_t f = g_frameTick.fetch_add(1, std::memory_order_relaxed) + 1;
    if (f == 1) LOGI("[Gjc2Intl] bspeed heartbeat armed (get_deltaTime firing)");
    // 与 InBattleWindow() 同判据：未真进过战斗绝不加速（防开机热身窗口误加速）
    const bool inBattle =
        g_battleSeen.load(std::memory_order_relaxed) &&
        (f - g_lastBattleFrame.load(std::memory_order_relaxed)) < kBattleIdleFrames;
    const int sp = g_battleSpeed.load();
    SetTimeScale_t setter = g_origSetTimeScale ? g_origSetTimeScale : g_setTimeScaleDirect;
    if (sp > 1 && inBattle) {
        if (g_pinnedScale.load() != sp && setter) {
            g_pinnedScale.store(sp);
            setter((float)sp, g_miSetTimeScale);
            LOGI("[Gjc2Intl] bspeed pin x%d frame=%llu", sp, (unsigned long long)f);
        }
    } else if (g_pinnedScale.exchange(0) > 0) {
        if (setter) setter(1.0f, g_miSetTimeScale);
        LOGI("[Gjc2Intl] bspeed unpin -> 1.0 frame=%llu", (unsigned long long)f);
    }
    return g_origGetDeltaTime ? g_origGetDeltaTime(method) : 0.0166667f;
}

// ============================================================
//  攻速倍率 —— 【功能已整项移除，2026-10-17 用户指定】
//
//  移除依据（用户原话）：「不用改攻速倍率了，把这个功能也从菜单删除掉吧，战斗加速后也包括了攻速」
//  ⇒ 菜单项 `Slider_国际_攻速倍率:1:10` 与全部相关代码（g_atkSpeed / ApplyAtkSpeed /
//     菜单回调与 Tick 同步分支）一并删除：菜单数组 7 → 6 项，功能项 6 → 5 项。
//
//  历史事故（保留以防复发；全文见 `10_docs/弓箭传说2国际版/功能落地_20261017.md` §3 事故一）：
//  Hook `HotFix.Common.AttributeData.GetAttackSpeed` 后取值口**恒返回 0.000**
//  （日志 `atkspd hero x5 0.000 -> 0.001`）⇒ 英雄攻速=0 ⇒ **玩家完全不攻击（站着不动）**，
//  并连带把「秒杀 / 全屏攻击」一起打成 0 命中（玩家不攻击 ⇒ UpdateHp 不被调用）。
//  ⇒ **禁止再以 Dobby 直挂热更属性取值口**（`10_docs/弓箭传说2国际版/硬约束.md` E1）。
//
//  若将来重启本功能：走数据型（英雄 AttributeData 实例 + AttributeOneElement.AttrKey@0x10 /
//  Value@0x18），零代码 hook。实例指针获取路径待定（候选：AttributeOut.attributeData@0x30，
//  AttributeOut 另有 entityId@0x2c / skills@0x38）。
// ============================================================

// ============================================================
//  【探查脚手架 · 默认不编译】一次性类型探查
//
//  构建加 -DGJC2INTL_PROBE=1 才进 so；**release 产物内零探查代码**
//  （与 STARRY_DIAG 同思路，配套 check_release_clean.sh 门禁）。
//  2026-10-17 靠它定过热更类型清单：热更 image 2940 类 / 1349 个 HotFix 命名空间类；
//  HotFix.Common.AttributeData 有 60 个方法含 GetAttackSpeed/GetAttack/GetMoveSpeed，
//  唯一实例字段 AttributesArray@0x20；元素 AttributeOneElement：AttrKey@0x10/Value@0x18。
//  —— 只读只打印，绝不写内存 ——
// ============================================================
#ifdef GJC2INTL_PROBE

/** System.String → ASCII（非可见字符转 '?'）：length@0x10，utf16 chars@0x14。 */
static void RdUtf16Ascii(void* s, char* out, size_t cap) {
    if (!out || cap < 2) return;
    out[0] = '\0';
    if (!s || !Il2Cpp::IsValidHeapPtr(s)) return;
    int32_t len = 0;
    memcpy(&len, reinterpret_cast<const char*>(s) + 0x10, sizeof(len));
    if (len <= 0 || len > 256) return;
    const char* ch = reinterpret_cast<const char*>(s) + 0x14;
    size_t n = 0;
    for (int32_t i = 0; i < len && n + 1 < cap; ++i) {
        uint16_t c = 0;
        memcpy(&c, ch + i * 2, sizeof(c));
        out[n++] = (c >= 32 && c < 127) ? static_cast<char>(c) : '?';
    }
    out[n] = '\0';
}

/** Dictionary<,> 键清单：`_entries`@0x18 / `_count`@0x20 / entry 0x18B（key@+0x08）。 */
static void TraceDumpDictKeys(void* dict, const char* label, int maxKeys) {
    if (!dict || !Il2Cpp::IsValidHeapPtr(dict)) {
        GJC2INTL_TRACE("ATTR %s dict=%p (null)", label, dict);
        return;
    }
    void* entries = Il2Cpp::ReadPtr(dict, kOffDictEntries);
    int32_t count = ReadI32(dict, kOffDictCount);
    int32_t alen = ReadInt32ArrayLen(entries);
    GJC2INTL_TRACE("ATTR %s dict=%p entries=%p count=%d alen=%d", label, dict, entries, count, alen);
    if (!entries || count <= 0 || count > 512 || alen <= 0) return;
    uintptr_t data = reinterpret_cast<uintptr_t>(entries) + kOffArrayData;
    int shown = 0;
    for (int32_t i = 0; i < count && i < alen && shown < maxKeys; ++i) {
        void* keyObj = *reinterpret_cast<void**>(data + static_cast<size_t>(i) * kDictEntrySize + 0x08);
        char buf[48];
        RdUtf16Ascii(keyObj, buf, sizeof(buf));
        if (buf[0]) {
            GJC2INTL_TRACE("ATTR   %s[%d]=%s", label, i, buf);
            shown++;
        }
    }
    GJC2INTL_TRACE("ATTR %s done shown=%d", label, shown);
}

/** 枚举各 image 的类名清单。
 *  先按「命名空间是否 HotFix*」把热更侧挑出来（AOT/引擎侧同名关键词会把清单冲爆，
 *  上一版 300 条上限就是这样被 UnityEngine 类吃光的），并打印每 image 的归类统计。 */
static void TraceEnumerateTypes() {
    auto& api = GetApi();
    if (!api.image_class_count || !api.image_class || !api.class_name) {
        GJC2INTL_TRACE("TYPLIST skip (api missing)");
        return;
    }
    static const char* const kKws[] = {
        "Attribute", "Attr", "Speed", "Attack", "Weapon", "Property", "Component", nullptr,
    };
    size_t nasm = 0;
    void** asms = api.Assemblies(&nasm);
    if (!asms) return;
    int shown = 0, dumped = 0, hotTotal = 0, chosen = 0;
    for (size_t a = 0; a < nasm; ++a) {
        void* img = api.ImageOf(asms[a]);
        if (!img) continue;
        uint32_t n = api.image_class_count(img);
        if (n == 0 || n > 300000) continue;  // 异常 image 直接跳过（防御）
        int hot = 0, printed = 0;
        for (uint32_t i = 0; i < n; ++i) {
            void* k = api.image_class(img, i);
            if (!k) continue;
            const char* cn = api.class_name(k);
            if (!cn) continue;
            const char* kns = api.class_namespace ? api.class_namespace(k) : nullptr;
            if (!kns || strncmp(kns, "HotFix", 6) != 0) continue;
            hot++;
            bool hit = false;
            for (const char* const* p = kKws; *p && !hit; ++p) hit = NameHas(cn, *p);
            if (!hit || shown >= 400) continue;
            GJC2INTL_TRACE("TYPLIST idx=%zu ns=%s name=%s", a, kns, cn);
            shown++;
            printed++;
            if (dumped < 8 && (NameHas(cn, "Attribute") || NameHas(cn, "Weapon") ||
                               NameHas(cn, "Attack") || NameHas(cn, "Speed"))) {
                TraceDumpClass(k, cn);
                dumped++;
            }
        }
        if (hot > 0) {
            hotTotal += hot;
            chosen++;
            GJC2INTL_TRACE("TYPLIST IMG idx=%zu classes=%u hotfix_ns=%d printed=%d", a, n, hot,
                           printed);
        }
    }
    GJC2INTL_TRACE("TYPLIST done imgs=%d hotfix_ns=%d shown=%d dumped=%d", chosen, hotTotal, shown,
                   dumped);
}

/** 跨命名空间找类：HotFix.BattleLogic → HotFix.Common → 全局。 */
static void* FindAny(const char* name) {
    void* k = FindClassNs("HotFix.BattleLogic", name);
    if (!k) k = FindClassNs("HotFix.Common", name);
    if (!k) k = FindClassNs("", name);
    return k;
}

/** 定向 dump：属性/组件候选类的完整成员（只读只打印）。 */
static void TraceDumpNamedClasses() {
    static const char* const kNames[] = {
        "EntityCharacter",     "CharacterAttributeData", "EntityMonster",
        "EntityBase",          "AttributeOut",           "CharacterComponentBase",
        nullptr,
    };
    for (const char* const* p = kNames; *p; ++p) {
        void* k = FindAny(*p);
        if (k) {
            TraceDumpClass(k, *p);
        } else {
            GJC2INTL_TRACE("DUMPNAMED miss %s", *p);
        }
    }
}

/** 属性名表键清单：等 BattleWorldContext 实例出现后打一次（只读）。 */
static std::atomic<bool> g_attrKeysDumped{false};

static void TraceAttriKeysOnce() {
    if (g_attrKeysDumped.load()) return;
    void* ctx = g_worldCtx.load();
    if (!ctx || !Il2Cpp::IsValidHeapPtr(ctx)) return;  // 还没进对局，下拍再来
    if (g_attrKeysDumped.exchange(true)) return;
    auto& api = GetApi();
    void* klass = api.object_class ? api.object_class(ctx) : nullptr;
    if (!klass) return;
    static const char* const kFields[] = {
        "_hurtAttributeNameTable", "_reCoverAttributeNameTable",
        "_bodyAttackAttributeNameTable", "_weaponAttrCache",
    };
    for (const char* f : kFields) {
        size_t off = api.FieldOffset(klass, f, 0);
        if (!off) {
            GJC2INTL_TRACE("ATTR %s off=0 (miss)", f);
            continue;
        }
        void* dict = Il2Cpp::ReadPtr(ctx, off);
        GJC2INTL_TRACE("ATTR %s off=0x%zx dict=%p", f, off, dict);
        TraceDumpDictKeys(dict, f, 40);
    }
}
#endif  // GJC2INTL_PROBE

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
#ifdef GJC2INTL_PROBE
    // 一次性类名清单 + 定向成员 dump（热更成员定位用；只读只打印）
    TraceEnumerateTypes();
    TraceDumpNamedClasses();
#endif

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
    // ---- 攻速倍率已移除（用户 2026-10-17 指定）：本作**不装任何属性钩子**（见上方防复发记录）----
    // ---- 战斗加速：UnityEngine.Time（AOT，按名解析；不依赖热更类）----
    g_clsTime = FindClassNs("UnityEngine", "Time");
    if (g_clsTime) {
        auto& apiT = GetApi();
        void* miSetTs = apiT.GetMethod(g_clsTime, "set_timeScale", 1);
        void* miGetDt = apiT.GetMethod(g_clsTime, "get_deltaTime", 0);
        void* fnSetTs = MethodPointer(miSetTs);
        void* fnGetDt = MethodPointer(miGetDt);
        g_miSetTimeScale = miSetTs;
        g_setTimeScaleDirect = reinterpret_cast<SetTimeScale_t>(fnSetTs);
        // 先装心跳：战斗门控 InBattleWindow() 依赖它，顺序反了会短暂全局加速
        if (fnGetDt) {
            if (DobbyHook(fnGetDt, (void*)Hooked_GetDeltaTime, (void**)&g_origGetDeltaTime) != 0) {
                g_origGetDeltaTime = nullptr;
                LOGW("[Gjc2Intl] Dobby get_deltaTime failed — 战斗加速不可用（门控依赖它）");
            }
        } else {
            LOGW("[Gjc2Intl] Time.get_deltaTime missing — 战斗加速不可用");
        }
        if (fnSetTs) {
            if (DobbyHook(fnSetTs, (void*)Hooked_SetTimeScale, (void**)&g_origSetTimeScale) != 0) {
                g_origSetTimeScale = nullptr;
                LOGW("[Gjc2Intl] Dobby set_timeScale failed — 用直呼兜底");
            }
        } else {
            LOGW("[Gjc2Intl] Time.set_timeScale missing — 战斗加速不可用");
        }
        LOGI("[Gjc2Intl] bspeed cls=%p set=%p get=%p dst_ok=%d", g_clsTime, fnSetTs, fnGetDt,
             g_origGetDeltaTime ? 1 : 0);
    } else {
        LOGW("[Gjc2Intl] UnityEngine.Time 未解析到 — 战斗加速不可用");
    }

    g_hooked.store(true);
    g_ready.store(true);
    LOGI("[Gjc2Intl] hooked ShowSkills/AddSkill/Exec/Rsr/Fill/TmpList/SetTarget/UpdateHp/Time "
         "tmpList=%p exec=%p",
         fnTmpList, fnExec);
    return true;
}

bool Gjc2Intl_TryInit() { return InstallHooks(); }

bool Gjc2Intl_IsReady() { return g_ready.load(); }

/** 滑块档位收敛：菜单约定 `Slider_...:1:10`，1 = 关。 */
static int ClampSpeed(int v) {
    if (v < 1) return 1;
    if (v > 10) return 10;
    return v;
}

void Gjc2Intl_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == GJC2INTL_FEAT_STEAL_SKILL) g_steal.store(enabled);
    if (featNum == GJC2INTL_FEAT_INVIS) g_invis.store(enabled);
    if (featNum == GJC2INTL_FEAT_FULLSCREEN) g_aoe.store(enabled);
    if (featNum == GJC2INTL_FEAT_ONESHOT) {
        g_oneshot.store(enabled);
        g_hitsOneshot.store(0);
    }
    LOGI("[Gjc2Intl] feat %d -> %d", featNum, enabled ? 1 : 0);
}

void Gjc2Intl_OnValueChanged(int featNum, int value) {
    const int v = ClampSpeed(value);
    if (featNum == GJC2INTL_FEAT_BATTLE_SPEED) {
        g_battleSpeed.store(v);
        LOGI("[Gjc2Intl] battle speed -> %d", v);
    }
}

void Gjc2Intl_Tick(const bool* features, const int* values) {
    if (features) {
        g_steal.store(features[GJC2INTL_FEAT_STEAL_SKILL]);
        g_invis.store(features[GJC2INTL_FEAT_INVIS]);
        g_aoe.store(features[GJC2INTL_FEAT_FULLSCREEN]);
        g_oneshot.store(features[GJC2INTL_FEAT_ONESHOT]);
    }
    if (values) {
        // 滑块每次心跳都同步（防菜单侧改值后 onValueChanged 未达），变化才动手
        const int bsp = ClampSpeed(values[GJC2INTL_FEAT_BATTLE_SPEED]);
        if (bsp != g_battleSpeed.load()) {
            g_battleSpeed.store(bsp);
            LOGI("[Gjc2Intl] battle speed -> %d", bsp);
        }
    } else {
        // 无 values 的旧调用面：滑块一律按「关」，避免残留加速
        g_battleSpeed.store(1);
    }
    // ---- 懒初始化（2026-10-17 实测事故后改为与国服同策略）----
    // 事故：热更未就绪时若每 200ms 反复装钩，`FindBattle` 会走 ClassFromNameScan
    // 降级路径逐个 image 建表（日志 `built map for image=... classes=406`），该
    // 半初始化窗口内会 SIGSEGV（`Fatal signal 11 fault addr 0x4088`，栈在 libil2cpp）。
    // ⇒ **只有用户开了功能才尝试装钩**，且 ≥1s 间隔、最多 60 次（与国服 gjc2 同款
    //   「禁止启动期 init，改 Tick 懒装」；继续局死锁案亦同此源）。
    if (!g_ready.load()) {
#ifdef GJC2INTL_PROBE
        const bool anyOn = true;  // 探针构建：无条件懒装（仍受 20s 静默期与节流约束）
#else
        const bool anyOn = g_steal.load() || g_invis.load() || g_aoe.load() || g_oneshot.load() ||
                           g_battleSpeed.load() > 1;
#endif
        if (anyOn) {
            const int tick = g_initTick.fetch_add(1) + 1;
            // 启动 20s（100 tick）内**一律不装钩**：热更/il2cpp 半初始化窗口内做全 image
            // 扫描会 SIGSEGV（2026-10-17 05:03 实测 fault addr 0x4088）。之后 ≥1s 间隔、
            // 最多 60 次，失败即留待下一次用户操作触发。
            if (tick > 100 && (tick % 5) == 0 && g_initAttempts.load() < 60) {
                g_initAttempts.fetch_add(1);
                InstallHooks();
            }
        }
    }
#ifdef GJC2INTL_PROBE
    // 属性名表键探查：context 一出现就打一次（只读）
    TraceAttriKeysOnce();
#endif
}

void Gjc2Intl_ForceClear() {
    g_steal.store(false);
    g_invis.store(false);
    g_aoe.store(false);
    g_oneshot.store(false);
    g_battleSpeed.store(1);
    // 注意：**不要**清 g_pinnedScale —— 还原 timeScale 靠心跳里
    // `g_pinnedScale.exchange(0) > 0` 这条分支，清零会让加速残留。
}
