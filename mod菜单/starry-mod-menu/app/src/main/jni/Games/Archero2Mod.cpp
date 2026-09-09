/* ================================================================
 *  弓箭传说2 (com.xq.archeroii.ua) 1.1.8 — HybridCLR HotFix.BattleLogic
 *
 *  现役触发：
 *    - 偷技能：HeroComponentRandomSkill.AddSkill → 直写 _skills Dictionary
 *    - 全屏：BattleWorldContext.PushHurtDataArray → 改写目标再 push
 *    - 隐身：SetTarget / TrySetTarget（AOT，Dobby）
 *
 *  关键：HotFix 方法冷启动时 MethodPointer 常指向同一 HybridCLR lazy stub。
 *  对共享地址 Dobby 会互相覆盖，且把 stub 上所有流量记成 AddSkill。
 *  正确做法：改写各 MethodInfo 自己的 methodPointer 槽（MI-patch），
 *  即使 orig 地址相同，AddSkill / PushHurt 也会进各自的桩。
 *
 *  文档：10_docs/弓箭传说2/诊断_失效根因_20260830.md
 * ================================================================ */

#include "Archero2Mod.h"
#include "Includes/SecStrings.hpp"

#include <atomic>
#include <cerrno>
#include <cstdarg>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <ctime>
#include <dlfcn.h>
#include <sys/syscall.h>
#include <sys/uio.h>
#include <unistd.h>

#include <android/log.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

enum : size_t {
    kOffListItems = 0x10,
    kOffListSize  = 0x18,
    kOffArrayLen  = 0x18,
    kOffArrayData = 0x20,  // Il2CppArray 内联数据（禁止把 +0x20 当指针解引用）
};

struct ClassNameCache {
    void* klass = nullptr;
    char name[40] = {};
    bool valid = false;
};

static ClassNameCache g_heroCache;
static ClassNameCache g_monsterCache;

static constexpr int kAoeMaxExtra = 40;
static constexpr int kRecentEnemyCap = 48;

static std::atomic<bool> g_ready{false};       // hooks 已安装
static std::atomic<bool> g_classesReady{false}; // HotFix 类已可解析（未挂钩）
static std::atomic<bool> g_hooked{false};
static std::atomic<bool> g_steal{false};
static std::atomic<bool> g_invis{false};
static std::atomic<bool> g_aoe{false};
static std::atomic<bool> g_pushProbe{false}; // 全屏开关驱动的 PushHurt 探针（不改变玩法）
static std::atomic<bool> g_stealReenter{false};
static std::atomic<bool> g_aoeReenter{false};
static std::atomic<uint32_t> g_hitsSteal{0};
static std::atomic<uint32_t> g_hitsAoe{0};
static std::atomic<uint32_t> g_hitsInvis{0};
static std::atomic<uint32_t> g_hitsAddSkill{0};
static std::atomic<uint32_t> g_hitsPushHurt{0};
static std::atomic<uint32_t> g_ssHits{0};
static std::atomic<uint32_t> g_anchorHero{0};
static std::atomic<uint32_t> g_anchorList{0};
static std::atomic<uint32_t> g_gateFail{0};

static std::atomic<void*> g_worldCtx{nullptr};
static void* g_wctxHero = nullptr; // wctx 缓存绑定的 hero 实例（跨对局失效保护）
static std::atomic<void*> g_lastHero{nullptr};
static std::atomic<void*> g_lastSkillComp{nullptr};

static int32_t g_ssIds[32];
static int g_ssIdN = 0;
static std::atomic<bool> g_ssReady{false};

static void* g_recentEnemies[kRecentEnemyCap]{};
static std::atomic<int> g_recentEnemyN{0};

static size_t g_offHrsEntity = 0;  // HeroComponentRandomSkill → Entity

// intl dump 字段（HeroComponentRandomSkill / RandomSkillResult）
enum : size_t {
    kOffRsShowSkills = 0x40,
    kOffHrsRandomResult = 0x78,
    kOffHrsLastSkills = 0xa0,
    kOffHrsSkills = 0x40,
};

typedef void* (*ShowSkills_t)(void* self);
typedef bool (*AddSkillToChar_t)(void* ctx, void* entity, int32_t skillId, bool addToComponent);
// 宽签名：与 diag 探针一致，避免 HybridCLR 多参/返回值把寄存器截断
typedef void (*AddSkillLive_t)(void* a, void* b, void* c, void* d, void* e, void* f, void* g,
                               void* h);
typedef void (*SetTarget_t)(void* self, void* target);
typedef void (*PushHurt_t)(void* a, void* b, void* c, void* d, void* e, void* f, void* g,
                           void* h);
typedef void* (*GetPlayer_t)(void* em, int32_t playerId);
typedef void (*FillSkillList_t)(void* self, void* list);

static ShowSkills_t g_origShowSkills = nullptr;
static AddSkillToChar_t g_origAddToChar = nullptr;
static AddSkillLive_t g_origAddSkillLive = nullptr;
static SetTarget_t g_origSetTarget = nullptr;
static SetTarget_t g_origTrySetTarget = nullptr;
static PushHurt_t g_origPushHurt = nullptr;
static GetPlayer_t g_getPlayer = nullptr;
static FillSkillList_t g_origFillSkillList = nullptr;
typedef bool (*Exec_t)(void* self);
typedef void (*Invoker_t)(void* methodPointer, const void* method, void* obj, void** params,
                          void* ret);
static Exec_t g_origExec = nullptr;
static Invoker_t g_origInvoker = nullptr;
static void* g_miAddToChar = nullptr;
static void* g_miAddSkillLive = nullptr;
static void* g_miPushHurt = nullptr;
static void* g_miShow = nullptr;
static void* g_miSet = nullptr;
static void* g_miTry = nullptr;
static void* g_miFill = nullptr;
static void* g_miExec = nullptr;
static void* g_hookAt[48] = {};
static std::atomic<bool> g_execHooked{false};
static void* g_coldSharedMp = nullptr;  // AddSkill/PushHurt 冷启动共用 stub
static std::atomic<uint64_t> g_hookInstallMs{0};
static std::atomic<uint64_t> g_processStartMs{0};
static std::atomic<uint64_t> g_featOnMs{0};

static constexpr uint64_t kGjc2DeferAfterFeatMs = 1000ull;     // 开功能后 1s 装 Dobby（原 8s 保险针对的崩溃真因已修：帧管线错钩/缺上下文）
static constexpr uint64_t kGjc2DeferAfterProcessMs = 12000ull; // 冷启至少 12s（继续恢复窗口）

static uint64_t NowMs() {
    timespec ts{};
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return static_cast<uint64_t>(ts.tv_sec) * 1000ull +
           static_cast<uint64_t>(ts.tv_nsec) / 1000000ull;
}

/** 挂钩即生效（原 2s 宽限针对的崩溃真因已修；效果延迟被用户体感为「生效慢」）。 */
static bool EffectsArmed() {
    return g_hookInstallMs.load(std::memory_order_relaxed) != 0;
}

/** log 实锤 12:40:33 开全屏后 186ms 即 Dobby → 继续局卡死；推迟到宽限期外。 */
static bool CanInstallHooks() {
    uint64_t ps = g_processStartMs.load(std::memory_order_relaxed);
    uint64_t fo = g_featOnMs.load(std::memory_order_relaxed);
    if (!ps || !fo) return false;
    uint64_t now = NowMs();
    uint64_t gate = ps + kGjc2DeferAfterProcessMs;
    if (fo + kGjc2DeferAfterFeatMs > gate) gate = fo + kGjc2DeferAfterFeatMs;
    return now >= gate;
}

static void SyncFeatOnGate(bool invis, bool aoe) {
    static bool s_prev = false;
    const bool on = invis || aoe;
    if (on && !s_prev) g_featOnMs.store(NowMs(), std::memory_order_relaxed);
    s_prev = on;
}

static void* MethodPointer(void* mi) {
    return mi ? *reinterpret_cast<void**>(mi) : nullptr;
}

/** 改写 MethodInfo.methodPointer — 仅作 fallback；HybridCLR 解释执行常不走此槽。 */
static bool PatchMiMp(void* mi, void* hookFn, void** origSlot, const char* name) {
    if (!mi || !hookFn || !origSlot) return false;
    void* cur = MethodPointer(mi);
    if (!cur) return false;
    if (cur == hookFn) return true;
    *origSlot = cur;
    *reinterpret_cast<void**>(mi) = hookFn;
    void** slot1 = reinterpret_cast<void**>(mi) + 1;
    if (*slot1 == cur) *slot1 = hookFn;
    LOGI("[Gjc2] MI-patch %s mi=%p orig=%p", name, mi, cur);
    return true;
}

static void* MiInvoker(void* /*mi*/) { return nullptr; }

static void DiscoverInvokerOff(void* /*mi*/) {
    // 已废弃：对 MethodInfo+N 盲 Dobby 会钩到错误热路径，继续关卡直接卡死（add/push 仍为 0）。
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

/** 全域精确名匹配：处理器类不在已知命名空间（8/31 四个 Processor 全 missing 的教训）。 */
static void* FindClassAnywhere(const char* name) {
    auto& api = GetApi();
    static size_t (*image_get_class_count)(void*) = nullptr;
    static void* (*image_get_class)(void*, size_t) = nullptr;
    if (!image_get_class) {
        void* h = dlopen(sec_lib_il2cpp(), RTLD_NOW);
        if (!h) return nullptr;
        image_get_class_count = reinterpret_cast<decltype(image_get_class_count)>(
            dlsym(h, "il2cpp_image_get_class_count"));
        image_get_class =
            reinterpret_cast<decltype(image_get_class)>(dlsym(h, "il2cpp_image_get_class"));
    }
    if (!image_get_class || !image_get_class_count || !api.class_name) return nullptr;
    size_t n = 0;
    void** asms = api.Assemblies(&n);
    if (!asms) return nullptr;
    for (size_t i = 0; i < n; i++) {
        void* img = api.ImageOf(asms[i]);
        if (!img) continue;
        size_t cnt = image_get_class_count(img);
        for (size_t k = 0; k < cnt; k++) {
            void* kls = image_get_class(img, k);
            if (!kls) continue;
            const char* cn = api.class_name(kls);
            if (cn && strcmp(cn, name) == 0) return kls;
        }
    }
    return nullptr;
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

/** 热路径对象门闩：对齐 + 堆校验，挡掉伤害对象里的打包 int 对 */
static bool SoftObjPtr(void* p) {
    if (!p) return false;
    if ((reinterpret_cast<uintptr_t>(p) & 7u) != 0) return false;
    // 用户空间常见指针高位；排除 tombstone 那种 aa1f03e2… 脏值
    uintptr_t u = reinterpret_cast<uintptr_t>(p);
    if (u < 0x100000000ULL || u > 0x800000000000ULL) return false;
    return Il2Cpp::IsValidHeapPtr(p);
}

static bool KRead(const void* addr, void* out, size_t len);

static void* ObjClass(void* obj) {
    if (!SoftObjPtr(obj)) return nullptr;
    // 加固：klass 槽先经 process_vm 试读——已释放/未映射内存在此失败返回，
    // 不再进入 object_class（Boehm GC 复用/还页的最后一类野读，8/31 风险评审）
    void* k = nullptr;
    if (!KRead(obj, &k, sizeof(k))) return nullptr;
    auto& api = GetApi();
    return api.object_class ? api.object_class(obj) : nullptr;
}

/** process_vm 自进程安全读：坏地址=系统调用失败返回 false，不 SIGSEGV（whhx 三铁律同款）。 */
static bool KRead(const void* addr, void* out, size_t len) {
    if (!addr || !out) return false;
    iovec local{out, len};
    iovec remote{const_cast<void*>(addr), len};
    return syscall(__NR_process_vm_readv, getpid(), &local, 1, &remote, 1, 0) == (ssize_t)len;
}

/** 打包 int 挡板 v2：{skillId,…} 打包对的高 32 位 = 第一个 int（≥10000 量级，如 0x32c9…），
 *  而本机 il2cpp 堆指针高 32 位实测 0x6c–0x78（0x764/0x774b/0x7866…）——
 *  高 32 位窗口 [0x60,0x80) 一刀切开，且热路径零系统调用。
 *  （process_vm 版快照在第 3 轮全哑，装机自检另行定性。） */
static bool LooksLikeEntity(void* p) {
    if (!SoftObjPtr(p)) return false;
    uintptr_t high = reinterpret_cast<uintptr_t>(p) >> 32;
    return high >= 0x60 && high < 0x80;
}

static const char* CacheClassName(ClassNameCache& cache, void* obj) {
    if (!SoftObjPtr(obj)) return nullptr;
    void* k = ObjClass(obj);
    if (!k || !SoftObjPtr(k)) return nullptr;
    auto& api = GetApi();
    if (!api.class_name) return nullptr;
    if (cache.klass != k) {
        const char* n = api.class_name(k);
        cache.klass = k;
        cache.valid = false;
        // 勿对 name 用 SoftObjPtr（只读串不在堆）；粗检首字节像 C# 标识符
        if (n) {
            unsigned char c0 = static_cast<unsigned char>(n[0]);
            if (c0 >= 'A' && (c0 <= 'Z' || (c0 >= 'a' && c0 <= 'z') || c0 == '_')) {
                size_t i = 0;
                for (; i + 1 < sizeof(cache.name) && n[i]; i++) {
                    unsigned char c = static_cast<unsigned char>(n[i]);
                    if (c < 32 || c > 126) break;
                    cache.name[i] = static_cast<char>(c);
                }
                cache.name[i] = 0;
                cache.valid = i > 0;
            }
        }
    }
    return cache.valid ? cache.name : nullptr;
}

static bool IsHeroObj(void* obj) {
    if (!SoftObjPtr(obj)) return false;
    const char* n = CacheClassName(g_heroCache, obj);
    return n && NameHas(n, "Hero");
}

static bool IsMonsterObj(void* obj) {
    if (!SoftObjPtr(obj)) return false;
    const char* n = CacheClassName(g_monsterCache, obj);
    return n && NameHas(n, "Monster");
}

static bool IsValidSkillId(int32_t id) {
    return id >= 10000 && id <= 99999;
}

static void WritePtr(void* p, size_t off, void* v) {
    if (SoftObjPtr(p)) *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(p) + off) = v;
}

static void RememberEnemy(void* ent) {
    // 只做指针收集（不解引用/不查类）——索敌者/受击者都是游戏刚用过的活对象；
    // 真伪过滤在扩散使用点做（LooksLikeEntity + IsMonsterObj）。
    if (!ent || !Il2Cpp::IsValidHeapPtr(ent)) return;
    int n = g_recentEnemyN.load();
    for (int i = 0; i < n && i < kRecentEnemyCap; i++) {
        if (g_recentEnemies[i] == ent) return;
    }
    if (n >= kRecentEnemyCap) {
        static std::atomic<int> sSlot{0};
        int slot = sSlot.fetch_add(1) % kRecentEnemyCap;
        g_recentEnemies[slot] = ent;
        return;
    }
    g_recentEnemies[n] = ent;
    g_recentEnemyN.store(n + 1);
}

static void SnapshotShowSkills(void* arr) {
    g_ssIdN = 0;
    g_ssReady.store(false);
    if (!arr || !Il2Cpp::IsValidHeapPtr(arr)) return;
    int32_t len = *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(arr) + kOffArrayLen);
    if (len < 2 || len > 32) return;
    // 内联数据：arr+0x20 起就是 int32[]，不要再解引用一次（旧 diag 读法会得到垃圾）
    uintptr_t data = reinterpret_cast<uintptr_t>(arr) + kOffArrayData;
    int n = 0;
    for (int i = 0; i < len && n < 32; i++) {
        int32_t id = *reinterpret_cast<int32_t*>(data + static_cast<size_t>(i) * 4u);
        if (!IsValidSkillId(id)) continue;
        g_ssIds[n++] = id;
    }
    g_ssIdN = n;
    if (n >= 2) g_ssReady.store(true);
}

/** List<int>：items@+0x10 size@+0x18，items 为 Int32[]。 */
static void SnapshotSkillList(void* list) {
    if (!SoftObjPtr(list)) return;
    void* items = Il2Cpp::ReadPtr(list, kOffListItems);
    int32_t size = Il2Cpp::ReadI32(list, kOffListSize);
    if (size < 2 || size > 32 || !SoftObjPtr(items)) return;
    uintptr_t data = reinterpret_cast<uintptr_t>(items) + kOffArrayData;
    int n = 0;
    g_ssIdN = 0;
    g_ssReady.store(false);
    for (int i = 0; i < size && n < 32; i++) {
        int32_t id = *reinterpret_cast<int32_t*>(data + static_cast<size_t>(i) * 4u);
        if (!IsValidSkillId(id)) continue;
        g_ssIds[n++] = id;
    }
    g_ssIdN = n;
    if (n >= 2) g_ssReady.store(true);
}

static void CaptureCandidatesFromComp(void* comp) {
    if (!SoftObjPtr(comp)) return;
    if (g_ssReady.load() && g_ssIdN >= 2) return;

    // RandomSkillResult._showSkills / get_ShowSkills
    void* result = Il2Cpp::ReadPtr(comp, kOffHrsRandomResult);
    if (SoftObjPtr(result)) {
        void* arr = Il2Cpp::ReadPtr(result, kOffRsShowSkills);
        if (SoftObjPtr(arr)) SnapshotShowSkills(arr);
        if ((!g_ssReady.load() || g_ssIdN < 2) && g_origShowSkills) {
            arr = g_origShowSkills(result);
            if (SoftObjPtr(arr)) SnapshotShowSkills(arr);
        }
        if (g_ssReady.load() && g_ssIdN >= 2) {
            LOGI("[Gjc2] candidates from RandomSkillResult n=%d", g_ssIdN);
            return;
        }
    }

    // _lastSkills
    void* last = Il2Cpp::ReadPtr(comp, kOffHrsLastSkills);
    if (SoftObjPtr(last)) {
        SnapshotShowSkills(last);
        if (!g_ssReady.load()) SnapshotSkillList(last);
        if (g_ssReady.load() && g_ssIdN >= 2) {
            LOGI("[Gjc2] candidates from _lastSkills n=%d", g_ssIdN);
            return;
        }
    }

    // 邻近指针槽扫 Int32[] / List
    for (size_t off = 0x50; off <= 0xC0; off += 8) {
        void* p = Il2Cpp::ReadPtr(comp, off);
        if (!SoftObjPtr(p)) continue;
        SnapshotShowSkills(p);
        if (!g_ssReady.load()) SnapshotSkillList(p);
        if (g_ssReady.load() && g_ssIdN >= 2) {
            LOGI("[Gjc2] candidates from comp+0x%zx n=%d", off, g_ssIdN);
            return;
        }
    }
}

static void* ResolveHeroFromComp(void* comp) {
    if (!comp || !Il2Cpp::IsValidHeapPtr(comp)) return g_lastHero.load();
    if (g_offHrsEntity) {
        void* e = Il2Cpp::ReadPtr(comp, g_offHrsEntity);
        if (IsHeroObj(e)) {
            g_lastHero.store(e);
            return e;
        }
    }
    for (size_t off = 0x10; off <= 0xB0; off += 8) {
        void* p = Il2Cpp::ReadPtr(comp, off);
        if (!IsHeroObj(p)) continue;
        g_offHrsEntity = off;
        g_lastHero.store(p);
        LOGI("[Gjc2] hero field @0x%zx", off);
        return p;
    }
    return g_lastHero.load();
}

static void* ResolveWorldCtx(void* hero) {
    void* cached = g_worldCtx.load();
    // 缓存绑定 hero 实例：跨对局英雄更换后强制重解析（tombstone_04 的 wctx 过期同源）
    if (cached && Il2Cpp::IsValidHeapPtr(cached) && g_wctxHero == hero) return cached;
    if (!hero || !Il2Cpp::IsValidHeapPtr(hero)) return nullptr;
    auto& api = GetApi();
    void* klass = ObjClass(hero);
    if (!klass) return nullptr;
    // Entity → 常见 World/Context 字段名
    static const char* kNames[] = {"<WorldContext>k__BackingField", "_worldContext", "WorldContext",
                                   "_world", "World", "_context", "Context", "_battleWorld",
                                   "BattleWorld", nullptr};
    for (int i = 0; kNames[i]; i++) {
        size_t off = api.FieldOffset(klass, kNames[i], 0);
        if (!off) continue;
        void* w = Il2Cpp::ReadPtr(hero, off);
        if (w && Il2Cpp::IsValidHeapPtr(w)) {
            g_worldCtx.store(w);
            g_wctxHero = hero;
            LOGI("[Gjc2] world via hero.%s @0x%zx", kNames[i], off);
            return w;
        }
    }
    // 扫指针槽：类名含 BattleWorld / WorldContext
    for (size_t off = 0x10; off <= 0x200; off += 8) {
        void* p = Il2Cpp::ReadPtr(hero, off);
        if (!p || !Il2Cpp::IsValidHeapPtr(p)) continue;
        const char* cn = api.ClassName(p);
        if (!cn) continue;
        if (NameHas(cn, "BattleWorld") || NameHas(cn, "WorldContext")) {
            g_worldCtx.store(p);
            g_wctxHero = hero;
            LOGI("[Gjc2] world scan hero+0x%zx (%s)", off, cn);
            return p;
        }
    }
    return nullptr;
}

// ---- Dictionary<int,SkillData> @ HeroComponentRandomSkill._skills (+0x40) ----
// Entry stride=0x18（diag22-23）：hash@0 next@4 key@8 value{i32,i32}@0xC
enum : size_t {
    kDictBuckets = 0x10,
    kDictEntries = 0x18,
    kDictCount   = 0x20,
    kEntryStride = 0x18,
    kEntryHash   = 0x0,
    kEntryNext   = 0x4,
    kEntryKey    = 0x8,
    kEntryVal0   = 0xC,
    kEntryVal1   = 0x10,
};

static bool DictHasKey(void* dic, int32_t key) {
    if (!SoftObjPtr(dic)) return false;
    void* entries = Il2Cpp::ReadPtr(dic, kDictEntries);
    if (!SoftObjPtr(entries)) return false;
    int32_t count = Il2Cpp::ReadI32(dic, kDictCount);
    if (count <= 0 || count > 256) return false;
    int32_t cap = Il2Cpp::ReadI32(entries, kOffArrayLen);
    if (cap <= 0 || cap > 512 || count > cap) return false;
    uintptr_t base = reinterpret_cast<uintptr_t>(entries) + kOffArrayData;
    for (int i = 0; i < count; i++) {
        int32_t k = *reinterpret_cast<int32_t*>(base + static_cast<size_t>(i) * kEntryStride + kEntryKey);
        if (k == key) return true;
    }
    return false;
}

/** 以已存在的 templateKey 条目为模板，插入新 key（.NET int hash = key & 0x7FFFFFFF）。 */
static bool DictInsertSkill(void* dic, int32_t newKey, int32_t templateKey) {
    if (!SoftObjPtr(dic) || !IsValidSkillId(newKey)) return false;
    void* buckets = Il2Cpp::ReadPtr(dic, kDictBuckets);
    void* entries = Il2Cpp::ReadPtr(dic, kDictEntries);
    if (!SoftObjPtr(buckets) || !SoftObjPtr(entries)) return false;
    int32_t count = Il2Cpp::ReadI32(dic, kDictCount);
    int32_t eLen = Il2Cpp::ReadI32(entries, kOffArrayLen);
    int32_t bLen = Il2Cpp::ReadI32(buckets, kOffArrayLen);
    if (count < 0 || eLen <= 0 || bLen <= 0 || count >= eLen || bLen > 512 || eLen > 512)
        return false;

    uintptr_t ebase = reinterpret_cast<uintptr_t>(entries) + kOffArrayData;
    uintptr_t bbase = reinterpret_cast<uintptr_t>(buckets) + kOffArrayData;

    int tmplIdx = -1;
    int32_t v0 = 1, v1 = 1;
    for (int i = 0; i < count; i++) {
        uintptr_t e = ebase + static_cast<size_t>(i) * kEntryStride;
        int32_t k = *reinterpret_cast<int32_t*>(e + kEntryKey);
        if (k == templateKey) {
            tmplIdx = i;
            v0 = *reinterpret_cast<int32_t*>(e + kEntryVal0);
            v1 = *reinterpret_cast<int32_t*>(e + kEntryVal1);
            break;
        }
    }
    if (tmplIdx < 0) {
        // 无模板：用常见默认值
        v0 = 1;
        v1 = 1;
    }

    int32_t hash = newKey & 0x7FFFFFFF;
    int32_t bucket = hash % bLen;
    int32_t oldHead = *reinterpret_cast<int32_t*>(bbase + static_cast<size_t>(bucket) * 4u);

    int32_t slot = count;
    uintptr_t ne = ebase + static_cast<size_t>(slot) * kEntryStride;
    *reinterpret_cast<int32_t*>(ne + kEntryHash) = hash;
    *reinterpret_cast<int32_t*>(ne + kEntryNext) = oldHead;
    *reinterpret_cast<int32_t*>(ne + kEntryKey) = newKey;
    *reinterpret_cast<int32_t*>(ne + kEntryVal0) = v0;
    *reinterpret_cast<int32_t*>(ne + kEntryVal1) = v1;

    *reinterpret_cast<int32_t*>(bbase + static_cast<size_t>(bucket) * 4u) = slot;
    Il2Cpp::WriteI32(dic, kDictCount, count + 1);
    return true;
}

static void* FindSkillCompFromHero(void* hero) {
    if (!SoftObjPtr(hero)) return nullptr;
    auto& api = GetApi();
    for (size_t off = 0x10; off <= 0x280; off += 8) {
        void* p = Il2Cpp::ReadPtr(hero, off);
        if (!SoftObjPtr(p)) continue;
        const char* cn = api.ClassName(p);
        if (cn && NameHas(cn, "RandomSkill")) return p;
    }
    return nullptr;
}

static std::atomic<void*> g_addSkillArgC{nullptr};  // diag：恒定静态指针
static std::atomic<void*> g_addSkillArgD{nullptr};

// ---- 偷技能取证轮：AddSkill 命中瞬间拍组件快照（orig 前拍=选前状态）----
struct SkillSnap {
    std::atomic<bool> used{false};
    void* comp = nullptr;
    void* ctxC = nullptr;
    void* argD = nullptr;
    int32_t skillId = 0;
    uint8_t bytes[0x100] = {};
};
static SkillSnap g_skillSnaps[4];
static std::atomic<uint32_t> g_skillSnapSeq{0};
static ClassNameCache g_hrsCache;

static void SnapshotSkillComp(void* comp, int32_t skillId, void* c, void* d) {
    if (!SoftObjPtr(comp)) return;
    SkillSnap& s = g_skillSnaps[g_skillSnapSeq.fetch_add(1) & 3u];
    memcpy(s.bytes, comp, sizeof(s.bytes)); // 活对象，本调用栈上直接读安全
    s.comp = comp;
    s.ctxC = c;
    s.argD = d;
    s.skillId = skillId;
    s.used.store(true, std::memory_order_release);
}

// ---- 偷技能（第 19 轮）：GM Exec 内部也走带门的 AddSkill（殊途同归被拒）。
//      改调 BWC.AddSkillToCharacter(EntityCharacter,Int32,Boolean)——8/17 斌哥的
//      无门直接授予原语，新热更只是不再调用它（hits=0 ≠ 方法死了）。 ----
static int32_t g_pendingCands[8];
static std::atomic<int> g_pendingCandN{0};
static std::atomic<uint32_t> g_stealHits{0};
static void* g_miAddToCharBwc = nullptr; // BWC.AddSkillToCharacter 的 MethodInfo
static void* g_fnAddToCharBwc = nullptr; // 其 methodPointer
typedef void* (*AddToCharWide_t)(void*, void*, void*, void*, void*, void*, void*, void*);
static size_t g_offHrsBoxResult = 0; // HRS._boxSkillResult（FieldOffset 解析）
static size_t g_offBoxId1 = 0;       // BoxSkillResult.SkillId1
static size_t g_offBoxId2 = 0;       // BoxSkillResult.SkillId2
static size_t g_offHrsRandomResult = 0; // HRS._randomSkillResult（FieldOffset 解析）
static size_t g_offRsShowSkills = 0;    // RandomSkillResult.<ShowSkills>k__BackingField

/** 直读 RSR.ShowSkills 数组（继续局选择不走 get_ShowSkills → 这是唯一可靠候选源）。 */
static int ReadShowSkillsDirect(void* hrsComp, int32_t* out, int maxN) {
    if (!hrsComp || !g_offHrsRandomResult || !g_offRsShowSkills) return 0;
    void* rsr = Il2Cpp::ReadPtr(hrsComp, g_offHrsRandomResult);
    if (!LooksLikeEntity(rsr)) return 0;
    void* arr = Il2Cpp::ReadPtr(rsr, g_offRsShowSkills);
    if (!SoftObjPtr(arr)) return 0;
    int32_t len = Il2Cpp::ReadI32(arr, kOffArrayLen);
    if (len < 2 || len > 8) return 0;
    int n = 0;
    uintptr_t data = reinterpret_cast<uintptr_t>(arr) + kOffArrayData;
    for (int i = 0; i < len && n < maxN; i++) {
        int32_t v = *reinterpret_cast<int32_t*>(data + static_cast<size_t>(i) * 4u);
        if (IsValidSkillId(v)) out[n++] = v;
    }
    return n;
}

/** 偷技能事件落盘（追加）：logcat 轮转快，诊断靠文件。 */
static void StealFileLog(const char* fmt, ...) {
    FILE* f = fopen("/data/data/com.xq.archeroii.ua/files/gjc2_steal_log.txt", "a");
    if (!f) return;
    time_t t = time(nullptr);
    struct tm tmv;
    localtime_r(&t, &tmv);
    fprintf(f, "[%02d:%02d:%02d] ", tmv.tm_hour, tmv.tm_min, tmv.tm_sec);
    va_list ap;
    va_start(ap, fmt);
    vfprintf(f, fmt, ap);
    va_end(ap);
    fputc('\n', f);
    fclose(f);
}

/** 从 HRS 组件反查当前英雄实体（AddSkill 的 a 永远是当前英雄的组件 → 首次三选一也有英雄）。 */
static void* ResolveHeroFromCompLive(void* comp) {
    if (!SoftObjPtr(comp)) return nullptr;
    for (size_t off = 0x10; off <= 0x60; off += 8) {
        void* p = Il2Cpp::ReadPtr(comp, off);
        if (IsHeroObj(p)) return p;
    }
    return nullptr;
}

static void CaptureShownCandidates(void* arr) {
    if (!SoftObjPtr(arr)) return;
    int32_t len = Il2Cpp::ReadI32(arr, kOffArrayLen);
    if (len < 2 || len > 8) return;
    int n = 0;
    uintptr_t data = reinterpret_cast<uintptr_t>(arr) + kOffArrayData;
    for (int i = 0; i < len && n < 8; i++) {
        int32_t v = *reinterpret_cast<int32_t*>(data + static_cast<size_t>(i) * 4u);
        if (IsValidSkillId(v)) g_pendingCands[n++] = v;
    }
    if (n >= 2) {
        g_pendingCandN.store(n);
        LOGI("[Gjc2] sshow n=%d ids=%d,%d,%d", n, g_pendingCands[0],
             n > 1 ? g_pendingCands[1] : 0, n > 2 ? g_pendingCands[2] : 0);
    }
}

typedef void* (*ShowSkillsLive_t)(void*, void*, void*, void*, void*, void*, void*, void*);
static ShowSkillsLive_t g_origShowSkillsLive = nullptr;

/** get_ShowSkills 返回 Int32[]（x0 透传），展示瞬间复制候选。 */
static void* Hooked_get_ShowSkillsLive(void* self, void* b, void* c, void* d, void* e, void* f,
                                       void* g, void* h) {
    void* arr = nullptr;
    if (g_origShowSkillsLive) arr = g_origShowSkillsLive(self, b, c, d, e, f, g, h);
    if (arr) CaptureShownCandidates(arr);
    return arr;
}

/** 偷技能补授暂禁用：再调 AddSkill(非选中) 会闪退（diag 已实锤）；字典直写会腐蚀继续存档。 */
static void TryStealViaAddSkill(void* /*comp*/, int32_t /*picked*/) {
    static std::atomic<uint32_t> s_once{0};
    if (s_once.fetch_add(1) < 3)
        LOGW("[Gjc2] steal grant disabled (AddSkill re-call crashes; dict write unsafe)");
}

static void TryStealDict(void* comp, int32_t skillId) {
    (void)comp;
    (void)skillId;
    TryStealViaAddSkill(nullptr, 0);
}

static void* Hooked_get_ShowSkills(void* self) {
    void* arr = g_origShowSkills ? g_origShowSkills(self) : nullptr;
    if (arr) {
        SnapshotShowSkills(arr);
        g_ssHits.fetch_add(1, std::memory_order_relaxed);
    }
    return arr;
}

static void Hooked_FillSkillList(void* self, void* list) {
    if (g_origFillSkillList) g_origFillSkillList(self, list);
    if (SoftObjPtr(self)) g_lastSkillComp.store(self);
}

/** Exec 只转发。偷技能补授未就绪前禁止在此扫字段/再调 AddSkill。 */
static bool Hooked_SelectSlotExec(void* self) {
    return g_origExec ? g_origExec(self) : false;
}

/** 现役授予：命中即拍快照（b=x1 原始寄存器值=int skillId）；偷技能=本职先行+其它候选同上下文重放。 */
static void Hooked_AddSkillLive(void* a, void* b, void* c, void* d, void* e, void* f, void* g,
                                void* h) {
    g_hitsAddSkill.fetch_add(1, std::memory_order_relaxed);
    if (SoftObjPtr(a)) g_lastSkillComp.store(a);
    if (c) g_addSkillArgC.store(c);
    g_addSkillArgD.store(d);
    int32_t picked = static_cast<int32_t>(reinterpret_cast<uintptr_t>(b));
    SnapshotSkillComp(a, picked, c, d);

    if (g_steal.load() && EffectsArmed() && !g_stealReenter.load() && IsValidSkillId(picked) &&
        g_origAddSkillLive) {
        // 保险门：picked 必须在展示期捕获的候选里（第 13 轮 extra=3 实锤错误源会戳坏恢复态）
        int n = g_pendingCandN.load(std::memory_order_relaxed);
        int32_t local[8] = {0};
        bool contains = false;
        for (int i = 0; i < n && i < 8; i++) {
            local[i] = g_pendingCands[i];
            if (local[i] == picked) contains = true;
        }
        // 继续局选择不走 get_ShowSkills → 直读 RSR.ShowSkills（第 23 轮）
        if (n < 2) {
            int n2 = ReadShowSkillsDirect(a, local, 8);
            if (n2 > n) {
                n = n2;
                for (int i = 0; i < n && i < 8; i++) {
                    if (local[i] == picked) contains = true;
                }
            }
        }
        g_origAddSkillLive(a, b, c, d, e, f, g, h); // 本职授予先行（玩家 picks 必中）
        int extra = 0;
        // 英雄优先从当前组件反查（首次三选一时 SetTarget 可能尚未发生）
        void* hero = ResolveHeroFromCompLive(a);
        if (!hero) {
            void* h2 = g_lastHero.load();
            if (h2 && IsHeroObj(h2)) hero = h2;
        }
        if (hero) hero = IsHeroObj(hero) ? hero : nullptr;
        void* wctx = hero ? ResolveWorldCtx(hero) : nullptr;
        if (wctx && LooksLikeEntity(wctx) && hero && g_fnAddToCharBwc && g_miAddToCharBwc) {
            g_stealReenter.store(true);
            if (contains) {
                // 普通三选一：pending 候选里除 picked 外全部直接授予
                for (int i = 0; i < n && i < 8; i++) {
                    if (local[i] == picked || !IsValidSkillId(local[i])) continue;
                    reinterpret_cast<AddToCharWide_t>(g_fnAddToCharBwc)(
                        wctx, hero,
                        reinterpret_cast<void*>(
                            static_cast<uintptr_t>(static_cast<uint32_t>(local[i]))),
                        (void*)1, g_miAddToCharBwc, 0, 0, 0);
                    extra++;
                }
            } else if (g_offHrsBoxResult && g_offBoxId1 && g_offBoxId2) {
                // 女武神/天使二选一（BoxSkillResult.SkillId1/2）：补上未被选的那个
                void* box = Il2Cpp::ReadPtr(a, g_offHrsBoxResult);
                if (LooksLikeEntity(box)) {
                    int32_t id1 = Il2Cpp::ReadI32(box, g_offBoxId1);
                    int32_t id2 = Il2Cpp::ReadI32(box, g_offBoxId2);
                    int32_t other = (id1 == picked) ? id2 : (id2 == picked) ? id1 : 0;
                    LOGI("[Gjc2] box picked=%d id1=%d id2=%d other=%d", picked, id1, id2, other);
                    StealFileLog("box picked=%d id1=%d id2=%d other=%d", picked, id1, id2, other);
                    if (IsValidSkillId(other) && other != picked) {
                        reinterpret_cast<AddToCharWide_t>(g_fnAddToCharBwc)(
                            wctx, hero,
                            reinterpret_cast<void*>(
                                static_cast<uintptr_t>(static_cast<uint32_t>(other))),
                            (void*)1, g_miAddToCharBwc, 0, 0, 0);
                        extra++;
                    }
                } else {
                    // 兜底1：组件相邻字段（天使实锤 comp+0x5c/0x60 存 picked+partner）
                    int32_t idA = Il2Cpp::ReadI32(a, 0x5c);
                    int32_t idB = Il2Cpp::ReadI32(a, 0x60);
                    int32_t other = (idA == picked) ? idB : (idB == picked) ? idA : 0;
                    StealFileLog("box fallback picked=%d a=%d b=%d other=%d", picked, idA, idB,
                                 other);
                    if (IsValidSkillId(other) && other != picked) {
                        reinterpret_cast<AddToCharWide_t>(g_fnAddToCharBwc)(
                            wctx, hero,
                            reinterpret_cast<void*>(
                                static_cast<uintptr_t>(static_cast<uint32_t>(other))),
                            (void*)1, g_miAddToCharBwc, 0, 0, 0);
                        extra++;
                    } else {
                        // 兜底2：全组件扫“像 id 的整数”（女武神 5xxxx 槽位侦察）
                        StealFileLog("box scan picked=%d:", picked);
                        for (size_t off = 0x10; off + 4 <= 0x100; off += 4) {
                            int32_t v = Il2Cpp::ReadI32(a, off);
                            if (IsValidSkillId(v) || (v >= 50000 && v <= 59999)) {
                                StealFileLog("  comp+0x%zx = %d", off, v);
                            }
                        }
                    }
                }
            }
            g_stealReenter.store(false);
        } else if (IsValidSkillId(picked) && n >= 0) {
            LOGI("[Gjc2] steal skip stale hero/wctx picked=%d", picked);
        }
        g_pendingCandN.store(0); // 一次选择消费一份候选；恢复局的 AddSkill 匹配不上→零重放
        uint32_t hn = g_stealHits.fetch_add(1) + 1;
        if (hn <= 6 || (hn % 20) == 0) {
            LOGI("[Gjc2] steal picked=%d pend=%d atc=%d", picked, n, extra);
            StealFileLog("steal picked=%d pend=%d atc=%d", picked, n, extra);
        }
        return;
    }
    if (IsValidSkillId(picked) && g_steal.load()) {
        StealFileLog("steal skip picked=%d (not armed/gated)", picked);
    }
    if (g_origAddSkillLive) g_origAddSkillLive(a, b, c, d, e, f, g, h);
}

static AddSkillLive_t g_origAddSkillLive2 = nullptr;
static std::atomic<uint32_t> g_hitsAddSkill2{0};
static void* g_skillMainSeen[3] = {0}; // AddSkill(Skill_Main) 真实授予时的对象实参
static std::atomic<uint32_t> g_skillMainN{0};

/** AddSkill 重载1（m[11] AddSkill(Skill_Main)：疑似无状态机门的底层插入）。 */
static void Hooked_AddSkillLive2(void* a, void* b, void* c, void* d, void* e, void* f, void* g,
                                 void* h) {
    g_hitsAddSkill2.fetch_add(1, std::memory_order_relaxed);
    if (SoftObjPtr(a)) g_lastSkillComp.store(a);
    uint32_t n = g_skillMainN.fetch_add(1);
    if (n < 3) g_skillMainSeen[n] = b;
    if (g_origAddSkillLive2) g_origAddSkillLive2(a, b, c, d, e, f, g, h);
}

static AddSkillLive_t g_origAddRandomSkill = nullptr;
static std::atomic<uint32_t> g_arHits{0};
static int32_t g_arArgs[6];
static std::atomic<uint32_t> g_arArgN{0};

/** AddRandomSkill(int,int) 计数 + 前三组参数捕获（权重轮询/自动授予路径）。 */
static void Hooked_AddRandomSkill(void* a, void* b, void* c, void* d, void* e, void* f, void* g,
                                  void* h) {
    g_arHits.fetch_add(1, std::memory_order_relaxed);
    uint32_t n = g_arArgN.fetch_add(1);
    if (n < 3) {
        g_arArgs[n * 2] = static_cast<int32_t>(reinterpret_cast<uintptr_t>(b));
        g_arArgs[n * 2 + 1] = static_cast<int32_t>(reinterpret_cast<uintptr_t>(c));
    }
    if (g_origAddRandomSkill) g_origAddRandomSkill(a, b, c, d, e, f, g, h);
}

static void Hooked_SetTarget(void* self, void* target) {
    if (SoftObjPtr(target) && IsHeroObj(target)) {
        // 英雄实例变化=新对局：旧敌表作废（GC 复用防护，8/31 风险评审）
        if (g_lastHero.exchange(target) != target) g_recentEnemyN.store(0);
        RememberEnemy(self); // 索敌者=怪：无论隐身开关都收集（只存指针，过滤在扩散点）
        if (g_invis.load() && EffectsArmed()) {
            uint32_t n = g_hitsInvis.fetch_add(1) + 1;
            if (n <= 8 || (n % 40) == 0) {
                LOGI("[Gjc2] invis block SetTarget hits=%u", n);
                __android_log_print(ANDROID_LOG_INFO, "StarryGjc2", "invis block hits=%u", n);
            }
            if (g_origSetTarget) g_origSetTarget(self, nullptr);
            return;
        }
    }
    if (g_origSetTarget) g_origSetTarget(self, target);
}

static void Hooked_TrySetTarget(void* self, void* target) {
    if (SoftObjPtr(target) && IsHeroObj(target)) {
        if (g_lastHero.exchange(target) != target) g_recentEnemyN.store(0);
        RememberEnemy(self);
        if (g_invis.load() && EffectsArmed()) {
            if (g_origTrySetTarget) g_origTrySetTarget(self, nullptr);
            return;
        }
    }
    if (g_origTrySetTarget) g_origTrySetTarget(self, target);
}

static bool RecentEnemiesContains(void* p) {
    if (!p) return false;
    int n = g_recentEnemyN.load(std::memory_order_relaxed);
    for (int i = 0; i < n && i < kRecentEnemyCap; i++) {
        if (g_recentEnemies[i] == p) return true;
    }
    return false;
}

// ---- 伤害对象取证快照：钩子线程只做 memcpy+存指针，Tick 线程倾倒 ----
struct HurtSnap {
    std::atomic<bool> used{false};
    void* hurt = nullptr;
    void* hero = nullptr;
    char cls[40] = {};
    uint8_t bytes[0x40] = {};
};
static HurtSnap g_hurtSnaps[4];
static std::atomic<uint32_t> g_snapSeq{0};
static ClassNameCache g_hurtClsCache;

static void SnapshotHurt(void* hurt) {
    if (!SoftObjPtr(hurt)) return;
    HurtSnap& s = g_hurtSnaps[g_snapSeq.fetch_add(1) & 3u];
    // hurt 是游戏刚传入、本调用栈上的活对象，直接读其字段安全（危险的是解引用其中读出的值）
    memcpy(s.bytes, hurt, sizeof(s.bytes));
    s.hurt = hurt;
    s.hero = g_lastHero.load();
    const char* n = CacheClassName(g_hurtClsCache, hurt);
    if (n) {
        size_t i = 0;
        for (; i + 1 < sizeof(s.cls) && n[i]; i++) s.cls[i] = n[i];
        s.cls[i] = 0;
    } else {
        s.cls[0] = 0;
    }
    s.used.store(true, std::memory_order_release);
}

static void TryAoeSpread(void* self, void* hurt, void* c, void* d, void* e, void* f, void* g,
                         void* h, PushHurt_t replay) {
    if (!g_aoe.load() || !EffectsArmed() || g_aoeReenter.load() || !replay) return;
    if (!SoftObjPtr(hurt)) return;

    // 定靶双锚（8/30 打包 int 野读教训：候选必须过 LooksLikeEntity 才准进 object_class）：
    //  ① 英雄锚：p18/p20 == 英雄指针（第 2 轮实测：记录里从未出现英雄指针，零命中）
    //  ② 成员锚：p18/p20 命中近期索敌怪列表 → 该字段就是受击目标（纯指针比较，零解引用）
    void* p18 = Il2Cpp::ReadPtr(hurt, 0x18); // hurt 本体是游戏刚传入的活对象，字段读安全
    void* p20 = Il2Cpp::ReadPtr(hurt, 0x20);

    void* victim = nullptr;
    size_t victimOff = 0;
    void* hero = g_lastHero.load();
    if (hero && p18 == hero && p20 != hero) {
        g_anchorHero.fetch_add(1, std::memory_order_relaxed);
        victim = p20;
        victimOff = 0x20;
    } else if (hero && p20 == hero && p18 != hero) {
        g_anchorHero.fetch_add(1, std::memory_order_relaxed);
        victim = p18;
        victimOff = 0x18;
    } else if (p18 != p20) {
        bool p18in = RecentEnemiesContains(p18);
        bool p20in = RecentEnemiesContains(p20);
        if (p18in && !p20in) {
            g_anchorList.fetch_add(1, std::memory_order_relaxed);
            victim = p18;
            victimOff = 0x18;
        } else if (p20in && !p18in) {
            g_anchorList.fetch_add(1, std::memory_order_relaxed);
            victim = p20;
            victimOff = 0x20;
        }
    }
    if (!victim) return;
    if (!LooksLikeEntity(victim) || !IsMonsterObj(victim)) {
        g_gateFail.fetch_add(1, std::memory_order_relaxed);
        return;
    }

    RememberEnemy(victim);

    int recentN = g_recentEnemyN.load();
    if (recentN < 1) return;

    g_aoeReenter.store(true);
    int spread = 0;
    void* saved = Il2Cpp::ReadPtr(hurt, victimOff);
    for (int i = 0; i < recentN && spread < kAoeMaxExtra; i++) {
        void* ent = g_recentEnemies[i];
        if (!ent || ent == victim || !LooksLikeEntity(ent) || !IsMonsterObj(ent)) continue;
        WritePtr(hurt, victimOff, ent);
        replay(self, hurt, c, d, e, f, g, h);
        spread++;
    }
    WritePtr(hurt, victimOff, saved);
    g_aoeReenter.store(false);

    if (spread > 0) {
        uint32_t n = g_hitsAoe.fetch_add(1) + 1;
        if (n <= 10 || (n % 40) == 0) {
            LOGI("[Gjc2] aoe spread=%d bursts=%u victimOff=0x%zx", spread, n, victimOff);
            __android_log_print(ANDROID_LOG_INFO, "StarryGjc2", "aoe spread=%d bursts=%u", spread,
                                n);
        }
    }
}

// ---- 全屏（第 9 轮）：英雄子弹命中怪 → 对近期索敌怪重放 OnHitByBullet（走完整伤害管线）----
static size_t g_offOnHitCom = 0; // EntityCharacter.OnHitCom 字段偏移（安装期解析）

static ClassNameCache g_onHitCompCache;
static bool IsOnHitComp(void* p) {
    if (!LooksLikeEntity(p)) return false;
    const char* n = CacheClassName(g_onHitCompCache, p);
    return n && NameHas(n, "OnHit");
}

typedef void (*OnHitByBullet_t)(void* selfComp, void* b, void* c, void* d, void* e, void* f,
                                void* g, void* h);
static OnHitByBullet_t g_origOnHitByBullet = nullptr;

static void Hooked_OnHitByBullet(void* selfComp, void* b, void* c, void* d, void* e, void* f,
                                 void* g, void* h) {
    if (g_origOnHitByBullet) g_origOnHitByBullet(selfComp, b, c, d, e, f, g, h);

    if (!g_aoe.load() || !EffectsArmed() || g_aoeReenter.load()) return;
    if (!IsOnHitComp(selfComp)) return;
    // 英雄自己被敌方子弹击中时不扩散（否则变成给全屏怪回血式误伤逻辑）
    void* hero = g_lastHero.load();
    if (hero) {
        void* heroComp = Il2Cpp::ReadPtr(hero, g_offOnHitCom);
        if (heroComp == selfComp) return;
    }

    int recentN = g_recentEnemyN.load(std::memory_order_relaxed);
    if (recentN < 1 || !g_offOnHitCom) return;

    g_aoeReenter.store(true);
    int spread = 0;
    for (int i = 0; i < recentN && spread < kAoeMaxExtra; i++) {
        void* ent = g_recentEnemies[i];
        if (!ent || !LooksLikeEntity(ent) || !IsMonsterObj(ent)) continue;
        void* comp = Il2Cpp::ReadPtr(ent, g_offOnHitCom);
        if (!comp || comp == selfComp || !IsOnHitComp(comp)) continue;
        // 关键：b..h 原样携带真实调用的全部寄存器（含解释器 MethodInfo* 上下文），
        // 只换 x0 —— tombstone_03（fault 0x70）实锤：合成调用缺上下文必崩
        if (g_origOnHitByBullet) g_origOnHitByBullet(comp, b, c, d, e, f, g, h);
        spread++;
    }
    g_aoeReenter.store(false);

    if (spread > 0) {
        uint32_t n = g_hitsAoe.fetch_add(1) + 1;
        if (n <= 10 || (n % 40) == 0) {
            LOGI("[Gjc2] aoe bullet spread=%d bursts=%u", spread, n);
            __android_log_print(ANDROID_LOG_INFO, "StarryGjc2", "aoe spread=%d bursts=%u", spread,
                                n);
        }
    }
}

/** 现役伤害通道。纯计数+转发，游戏线程桩零裸解引用/零日志；
 *  spread 在 TryAoeSpread 入口被 g_aoe 门控。 */
static void Hooked_PushHurt(void* self, void* hurt, void* c, void* d, void* e, void* f, void* g,
                            void* h) {
    g_hitsPushHurt.fetch_add(1, std::memory_order_relaxed);
    if (g_origPushHurt) g_origPushHurt(self, hurt, c, d, e, f, g, h);
    TryAoeSpread(self, hurt, c, d, e, f, g, h, g_origPushHurt);
    // 取证快照：每 16 次命中抓 1 份（KRead 安全读 + 类名缓存，均实锤可安全跑在游戏线程）
    if ((g_hitsPushHurt.load(std::memory_order_relaxed) & 0xFu) == 0) SnapshotHurt(hurt);
}

static void* g_aoeReplayMp = nullptr;

static void ReplayPushHurtInvoker(void* self, void* hurt, void* c, void* d, void* e, void* f,
                                  void* gArg, void* h) {
    (void)self;
    (void)hurt;
    (void)c;
    (void)d;
    (void)e;
    (void)f;
    (void)gArg;
    (void)h;
    // invoker 路径已废弃，保留符号防误链
}

/** 废弃：勿再 Dobby 共享 invoker。 */
static void Hooked_Invoker(void* mp, const void* method, void* obj, void** params, void* ret) {
    if (g_origInvoker) g_origInvoker(mp, method, obj, params, ret);
}

static void* FindMethodByNameNth(void* klass, const char* name, int nth) {
    auto& api = GetApi();
    static void* (*class_get_methods)(void*, void**) = nullptr;
    if (!class_get_methods) {
        void* h = dlopen(sec_lib_il2cpp(), RTLD_NOW);
        if (!h) return nullptr;
        class_get_methods =
            reinterpret_cast<decltype(class_get_methods)>(dlsym(h, "il2cpp_class_get_methods"));
    }
    if (!class_get_methods || !api.method_get_name) return nullptr;
    void* iter = nullptr;
    int seen = 0;
    while (void* m = class_get_methods(klass, &iter)) {
        const char* mn = api.method_get_name(m);
        if (!mn || strcmp(mn, name) != 0) continue;
        if (seen++ == nth) return m;
    }
    return nullptr;
}

static void* FindMethodNth(void* klass, const char* name, int argc, int nth) {
    auto& api = GetApi();
    if (nth == 0) {
        void* mi = api.GetMethod(klass, name, argc);
        if (mi) return mi;
    }
    static void* (*class_get_methods)(void*, void**) = nullptr;
    static uint32_t (*method_get_param_count)(void*) = nullptr;
    if (!class_get_methods) {
        void* h = dlopen(sec_lib_il2cpp(), RTLD_NOW);
        if (!h) return nullptr;
        class_get_methods =
            reinterpret_cast<decltype(class_get_methods)>(dlsym(h, "il2cpp_class_get_methods"));
        method_get_param_count = reinterpret_cast<decltype(method_get_param_count)>(
            dlsym(h, "il2cpp_method_get_param_count"));
    }
    if (!class_get_methods || !api.method_get_name) return nullptr;
    void* iter = nullptr;
    int seen = 0;
    while (void* m = class_get_methods(klass, &iter)) {
        const char* mn = api.method_get_name(m);
        if (!mn || strcmp(mn, name) != 0) continue;
        uint32_t pc = method_get_param_count ? method_get_param_count(m) : 99;
        if ((int)pc != argc) continue;
        if (seen++ == nth) return m;
    }
    return nullptr;
}

/** 方法名普查（一次性，安装线程安全）：新热更的方法名要在运行时枚举里找；带参数类型名。 */
static FILE* g_tblFile = nullptr; // 类表文件输出（logcat 会被轮转冲掉，文件不会）

static void DumpClassMethods(void* klass, const char* tag) {
    if (!klass) return;
    auto& api = GetApi();
    static void* (*class_get_methods)(void*, void**) = nullptr;
    static uint32_t (*method_get_param_count)(void*) = nullptr;
    static void* (*method_get_param)(void*, uint32_t) = nullptr;
    static void* (*class_from_type)(void*) = nullptr;
    if (!class_get_methods) {
        void* h = dlopen(sec_lib_il2cpp(), RTLD_NOW);
        if (!h) return;
        class_get_methods =
            reinterpret_cast<decltype(class_get_methods)>(dlsym(h, "il2cpp_class_get_methods"));
        method_get_param_count = reinterpret_cast<decltype(method_get_param_count)>(
            dlsym(h, "il2cpp_method_get_param_count"));
        method_get_param =
            reinterpret_cast<decltype(method_get_param)>(dlsym(h, "il2cpp_method_get_param"));
        class_from_type = reinterpret_cast<decltype(class_from_type)>(
            dlsym(h, "il2cpp_class_from_il2cpp_type"));
    }
    if (!class_get_methods || !api.method_get_name) return;
    void* iter = nullptr;
    int n = 0;
    while (void* m = class_get_methods(klass, &iter)) {
        const char* mn = api.method_get_name(m);
        if (!mn) continue;
        uint32_t pc = method_get_param_count ? method_get_param_count(m) : 0;
        char sig[96] = {0};
        size_t so = 0;
        if (method_get_param && class_from_type) {
            for (uint32_t pi = 0; pi < pc && pi < 4 && so + 2 < sizeof(sig); pi++) {
                void* pt = method_get_param(m, pi);
                void* pk = pt ? class_from_type(pt) : nullptr;
                const char* pn = pk ? api.class_name(pk) : nullptr;
                so += static_cast<size_t>(
                    snprintf(sig + so, sizeof(sig) - so, "%s%s", pi ? "," : "", pn ? pn : "?"));
            }
        }
        if (g_tblFile) fprintf(g_tblFile, "%s m[%d]=%s(%s) argc=%u\n", tag, n, mn, sig, pc);
        LOGI("[Gjc2] %s m[%d]=%s(%s) argc=%u", tag, n++, mn, sig, pc);
    }
    if (g_tblFile) fprintf(g_tblFile, "%s total=%d\n", tag, n);
    LOGI("[Gjc2] %s total=%d", tag, n);
}

/** 字段表（一次性）：找 HP 所在的组件字段。 */
static void DumpClassFields(void* klass, const char* tag) {
    if (!klass) return;
    auto& api = GetApi();
    static void* (*class_get_fields)(void*, void**) = nullptr;
    static const char* (*field_get_name)(void*) = nullptr;
    static void* (*field_get_type)(void*) = nullptr;
    static void* (*class_from_type)(void*) = nullptr;
    if (!class_get_fields) {
        void* h = dlopen(sec_lib_il2cpp(), RTLD_NOW);
        if (!h) return;
        class_get_fields =
            reinterpret_cast<decltype(class_get_fields)>(dlsym(h, "il2cpp_class_get_fields"));
        field_get_name =
            reinterpret_cast<decltype(field_get_name)>(dlsym(h, "il2cpp_field_get_name"));
        field_get_type =
            reinterpret_cast<decltype(field_get_type)>(dlsym(h, "il2cpp_field_get_type"));
        class_from_type = reinterpret_cast<decltype(class_from_type)>(
            dlsym(h, "il2cpp_class_from_il2cpp_type"));
    }
    if (!class_get_fields || !field_get_name || !field_get_type || !class_from_type) return;
    void* iter = nullptr;
    int n = 0;
    while (void* f = class_get_fields(klass, &iter)) {
        const char* fn = field_get_name(f);
        void* ft = field_get_type(f);
        void* fk = ft ? class_from_type(ft) : nullptr;
        const char* tn = fk ? api.class_name(fk) : "?";
        if (g_tblFile) fprintf(g_tblFile, "%s f[%d]=%s : %s\n", tag, n, fn ? fn : "?", tn ? tn : "?");
        LOGI("[Gjc2] %s f[%d]=%s : %s", tag, n++, fn ? fn : "?", tn ? tn : "?");
    }
    if (g_tblFile) fprintf(g_tblFile, "%s fields=%d\n", tag, n);
    LOGI("[Gjc2] %s fields=%d", tag, n);
}

/** 类表落盘（logcat 轮转的根治）：装机后一次，7 个关键类的方法/字段全量写入文件。 */
static void DumpTablesToFile() {
    FILE* f = fopen("/data/data/com.xq.archeroii.ua/files/gjc2_tables.txt", "w");
    if (!f) {
        LOGW("[Gjc2] table file open failed");
        return;
    }
    g_tblFile = f;
    const char* kTables[] = {"EntityCharacter",
                             "HeroComponentRandomSkill",
                             "RandomSkillResult",
                             "BattleWorldContext",
                             "SelectSlotCommand",
                             "HeroSkillCreator",
                             "BoxSkillResult",
                             "GMAddSkillCommand",
                             "Skill_Main"};
    for (const char* cn : kTables) {
        void* k = FindClassAnywhere(cn);
        fprintf(f, "==== %s ====\n", cn);
        if (!k) {
            fprintf(f, "(class missing)\n");
            continue;
        }
        DumpClassFields(k, cn);
        DumpClassMethods(k, cn);
    }
    g_tblFile = nullptr;
    fclose(f);
    LOGI("[Gjc2] tables written to gjc2_tables.txt");
    __android_log_print(ANDROID_LOG_INFO, "StarryGjc2", "tables written");
}

// ---- 第 6 轮伤害链探针：EC 候选方法纯计数（x0 透传兼容 void/bool/int 返回，零改写零日志）----
#define GJC2_PROBE_DEF(idx)                                                              \
    static std::atomic<uint32_t> g_pbC##idx{0};                                          \
    static void* g_pbOrig##idx = nullptr;                                                \
    static void* Hooked_Pb##idx(void* a, void* b, void* c, void* d, void* e, void* f,    \
                                void* g, void* h) {                                     \
        g_pbC##idx.fetch_add(1, std::memory_order_relaxed);                             \
        using Fn = void* (*)(void*, void*, void*, void*, void*, void*, void*, void*);   \
        return reinterpret_cast<Fn>(g_pbOrig##idx)(a, b, c, d, e, f, g, h);             \
    }
GJC2_PROBE_DEF(0)
GJC2_PROBE_DEF(1)
GJC2_PROBE_DEF(2)
GJC2_PROBE_DEF(3)
GJC2_PROBE_DEF(4)
GJC2_PROBE_DEF(5)
GJC2_PROBE_DEF(6)
GJC2_PROBE_DEF(7)

// ---- 动态探针池：伤害消息处理器逐方法计数（槽位 8..23）----
struct DynProbe {
    std::atomic<uint32_t> cnt{0};
    void* orig = nullptr;
    const char* name = nullptr; // il2cpp 方法名字符串，进程内有效
};
static DynProbe g_dynProbes[16];
static size_t g_dynProbeN = 0;
static void* g_hookAtDyn[16] = {};
#define GJC2_DYNPROBE_DEF(idx)                                                           \
    static void* Hooked_Dp##idx(void* a, void* b, void* c, void* d, void* e, void* f,    \
                                void* g, void* h) {                                     \
        g_dynProbes[(idx) - 8].cnt.fetch_add(1, std::memory_order_relaxed);             \
        using Fn = void* (*)(void*, void*, void*, void*, void*, void*, void*, void*);   \
        return reinterpret_cast<Fn>(g_dynProbes[(idx) - 8].orig)(a, b, c, d, e, f, g, h);\
    }
GJC2_DYNPROBE_DEF(8)
GJC2_DYNPROBE_DEF(9)
GJC2_DYNPROBE_DEF(10)
GJC2_DYNPROBE_DEF(11)
GJC2_DYNPROBE_DEF(12)
GJC2_DYNPROBE_DEF(13)
GJC2_DYNPROBE_DEF(14)
GJC2_DYNPROBE_DEF(15)
GJC2_DYNPROBE_DEF(16)
GJC2_DYNPROBE_DEF(17)
GJC2_DYNPROBE_DEF(18)
GJC2_DYNPROBE_DEF(19)
GJC2_DYNPROBE_DEF(20)
GJC2_DYNPROBE_DEF(21)
GJC2_DYNPROBE_DEF(22)
GJC2_DYNPROBE_DEF(23)
static void* g_dynStubs[16] = {
    (void*)Hooked_Dp8,  (void*)Hooked_Dp9,  (void*)Hooked_Dp10, (void*)Hooked_Dp11,
    (void*)Hooked_Dp12, (void*)Hooked_Dp13, (void*)Hooked_Dp14, (void*)Hooked_Dp15,
    (void*)Hooked_Dp16, (void*)Hooked_Dp17, (void*)Hooked_Dp18, (void*)Hooked_Dp19,
    (void*)Hooked_Dp20, (void*)Hooked_Dp21, (void*)Hooked_Dp22, (void*)Hooked_Dp23,
};

struct DamageProbe {
    const char* name;
    int argc;
    void** orig;
    void* stub;
    std::atomic<uint32_t>* cnt;
};
static DamageProbe g_damageProbes[] = {
    {"UpdateHp", 4, &g_pbOrig0, (void*)Hooked_Pb0, &g_pbC0},
    {"ChangeHp", 4, &g_pbOrig1, (void*)Hooked_Pb1, &g_pbC1},
    {"BeforeHurtTarget", 2, &g_pbOrig2, (void*)Hooked_Pb2, &g_pbC2},
    {"BeSecKill", 2, &g_pbOrig3, (void*)Hooked_Pb3, &g_pbC3},
    {"OnDeath", 1, &g_pbOrig4, (void*)Hooked_Pb4, &g_pbC4},
    {"SendHpUpdateEvent", 0, &g_pbOrig5, (void*)Hooked_Pb5, &g_pbC5},
    {"OnCharacterHpMaxUpdate", 1, &g_pbOrig6, (void*)Hooked_Pb6, &g_pbC6},
    {"TriggerHpChange", 0, &g_pbOrig7, (void*)Hooked_Pb7, &g_pbC7},
};

/** 热更程序集类名扫描：把名字带 Hp/Hurt/Damage/Life/SecKill 的类列出来（一次性）。 */
static void SweepHotfixClasses() {
    auto& api = GetApi();
    static size_t (*image_get_class_count)(void*) = nullptr;
    static void* (*image_get_class)(void*, size_t) = nullptr;
    if (!image_get_class) {
        void* h = dlopen(sec_lib_il2cpp(), RTLD_NOW);
        if (h) {
            image_get_class_count = reinterpret_cast<decltype(image_get_class_count)>(
                dlsym(h, "il2cpp_image_get_class_count"));
            image_get_class =
                reinterpret_cast<decltype(image_get_class)>(dlsym(h, "il2cpp_image_get_class"));
        }
    }
    if (!image_get_class || !image_get_class_count || !api.class_name) return;
    static const char* (*class_ns)(void*) = nullptr;
    if (!class_ns) {
        void* h2 = dlopen(sec_lib_il2cpp(), RTLD_NOLOAD);
        if (h2)
            class_ns =
                reinterpret_cast<decltype(class_ns)>(dlsym(h2, "il2cpp_class_get_namespace"));
    }
    size_t n = 0;
    void** asms = api.Assemblies(&n);
    if (!asms) return;
    int matched = 0;
    for (size_t i = 0; i < n && matched < 120; i++) {
        void* img = api.ImageOf(asms[i]);
        if (!img) continue;
        size_t cnt = image_get_class_count(img);
        for (size_t k = 0; k < cnt && matched < 120; k++) {
            void* kls = image_get_class(img, k);
            if (!kls) continue;
            const char* cn = api.class_name(kls);
            if (!cn) continue;
            if (NameHas(cn, "Hp") || NameHas(cn, "Hurt") || NameHas(cn, "Damage") ||
                NameHas(cn, "Life") || NameHas(cn, "SecKill")) {
                const char* ns = class_ns ? class_ns(kls) : nullptr;
                LOGI("[Gjc2] sweep[%d] %s::%s", matched++, ns ? ns : "", cn);
            }
        }
    }
    LOGI("[Gjc2] sweep total=%d", matched);
}

// 8/31：继续局卡死=8/30 错 hash 直写污染的战斗快照（用户放弃该局已闭环），战斗钩恢复。
// 全屏走三轮：第 1 轮=PushHurt 纯转发计数探针（kGjc2PushHurtProbeEnabled）；
// 第 2 轮才开 TryAoeSpread（kGjc2AoeEnabled）。偷技能仍禁。
static constexpr bool kGjc2BattleHooksEnabled = true;
static constexpr bool kGjc2AoeEnabled = true;            // 全屏：OnHitByBullet 重放（走完整伤害管线）
static constexpr bool kGjc2PushHurtProbeEnabled = false; // 帧管线钩子永久弃用（hs 快照实锤+卡死源）
static constexpr bool kGjc2DamageProbeEnabled = false;   // 24 个探针钩退役：热路径钩子群=继续局卡死元凶（8/31 两次实锤）
static constexpr bool kGjc2StealProbeEnabled = true;     // 偷技能取证轮：pending 状态机定位（冷路径钩+字段/方法普查）
static constexpr bool kGjc2StealEnabled = true;          // 偷技能：AddSkill(picked) 命中瞬间（orig 前，候选数组完整）对其它候选完整上下文重放

typedef void* (*thread_attach_t)(void*);
static thread_attach_t g_threadAttach = nullptr;

static bool EnsureIl2CppAttached() {
    auto& api = GetApi();
    if (!api.ready && !api.Load()) return false;
    if (!g_threadAttach) {
        void* h = dlopen(sec_lib_il2cpp(), RTLD_NOLOAD);
        if (!h) h = dlopen(sec_lib_il2cpp(), RTLD_NOW);
        if (h) g_threadAttach = reinterpret_cast<thread_attach_t>(dlsym(h, "il2cpp_thread_attach"));
    }
    void* domain = api.Domain();
    if (!domain) return false;
    if (g_threadAttach) g_threadAttach(domain);
    return true;
}

static void UninstallBattleHooks() {
    for (size_t i = 0; i < sizeof(g_hookAt) / sizeof(g_hookAt[0]); i++) {
        void* at = g_hookAt[i];
        if (at) DobbyDestroy(at);
        g_hookAt[i] = nullptr;
    }
    for (size_t i = 0; i < 16; i++) {
        if (g_hookAtDyn[i]) DobbyDestroy(g_hookAtDyn[i]);
        g_hookAtDyn[i] = nullptr;
    }
    g_dynProbeN = 0;
    g_origSetTarget = nullptr;
    g_origTrySetTarget = nullptr;
    g_origPushHurt = nullptr;
    g_origOnHitByBullet = nullptr;
    g_hooked.store(false);
    g_ready.store(false);
    g_classesReady.store(false);
    g_hookInstallMs.store(0);
    g_miSet = nullptr;
    g_miTry = nullptr;
    g_miPushHurt = nullptr;
    LOGI("[Gjc2] UninstallBattleHooks done");
    __android_log_print(ANDROID_LOG_INFO, "StarryGjc2", "hooks destroyed");
}

static bool InstallHooks() {
    if (g_hooked.load()) return true;
    if (!kGjc2BattleHooksEnabled) {
        g_hooked.store(true);
        g_ready.store(true);
        g_hookInstallMs.store(NowMs());
        LOGW("[Gjc2] InstallHooks DISABLED (continue A/B — no Dobby)");
        return true;
    }
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) {
        static std::atomic<uint32_t> s_wait{0};
        uint32_t w = s_wait.fetch_add(1) + 1;
        if (w == 1 || (w % 50) == 0) LOGI("[Gjc2] waiting %s (%u)", GameUnity::TARGET_LIB, w);
        return false;
    }
    if (!EnsureIl2CppAttached()) {
        LOGE("[Gjc2] Il2Cpp attach/Load failed");
        return false;
    }
    auto& api = GetApi();

    // 隐身只依赖 EntityCharacter（文档实锤热更未改此链）
    void* clsEc = FindBattle("EntityCharacter");
    if (!clsEc) {
        LOGE("[Gjc2] EntityCharacter missing");
        return false;
    }
    g_miSet = api.GetMethod(clsEc, "SetTarget", 1);
    g_miTry = api.GetMethod(clsEc, "TrySetTarget", 1);
    void* fnSet = MethodPointer(g_miSet);
    void* fnTry = MethodPointer(g_miTry);
    if (!fnSet || !fnTry) {
        LOGE("[Gjc2] method missing set=%p try=%p", fnSet, fnTry);
        return false;
    }

    auto tryHook = [](void* target, void* stub, void** orig, void** at, const char* name,
                      bool required) -> bool {
        if (!target) return !required;
        if (*at == target && *orig) return true;
        if (DobbyHook(target, stub, orig) != 0) {
            if (*orig) return true;
            if (required) LOGE("[Gjc2] Dobby %s failed", name);
            else LOGW("[Gjc2] Dobby %s failed (optional)", name);
            return !required;
        }
        *at = target;
        return true;
    };

    if (!tryHook(fnSet, (void*)Hooked_SetTarget, (void**)&g_origSetTarget, &g_hookAt[2], "SetTarget",
                 true))
        return false;
    if (!tryHook(fnTry, (void*)Hooked_TrySetTarget, (void**)&g_origTrySetTarget, &g_hookAt[3],
                 "TrySetTarget", true))
        return false;

    // 全屏第 1 轮：PushHurtDataArray 纯转发计数探针（可选钩，失败不阻断隐身）
    if (kGjc2PushHurtProbeEnabled) {
        void* clsBwc = FindBattle("BattleWorldContext");
        void* miPush = nullptr;
        if (clsBwc) {
            // diag 实锤现役调用 (a=GameProxy, b=伤害对象)；argc 按候选 2→3→1 探测
            miPush = FindMethodNth(clsBwc, "PushHurtDataArray", 2, 0);
            if (!miPush) miPush = FindMethodNth(clsBwc, "PushHurtDataArray", 3, 0);
            if (!miPush) miPush = FindMethodNth(clsBwc, "PushHurtDataArray", 1, 0);
        }
        g_miPushHurt = miPush;
        void* fnPush = MethodPointer(miPush);
        if (fnPush) {
            tryHook(fnPush, (void*)Hooked_PushHurt, (void**)&g_origPushHurt, &g_hookAt[4],
                    "PushHurt", false);
        } else {
            LOGW("[Gjc2] PushHurtDataArray MP missing (probe idle)");
            __android_log_print(ANDROID_LOG_WARN, "StarryGjc2", "PushHurt MP missing");
        }
    }

    g_hookInstallMs.store(NowMs());
    g_hooked.store(true);
    g_ready.store(true);
    // process_vm 自进程可用性定性（第 3 轮快照全哑的嫌疑点，一次性自检）
    {
        uint64_t tmp = 0;
        bool ok = KRead(fnSet, &tmp, sizeof(tmp)); // fnSet 为已映射代码地址
        LOGI("[Gjc2] kread selftest ok=%d errno=%d", ok ? 1 : 0, ok ? 0 : errno);
    }
    // 方法名普查：为全屏找新热更的 per-entity 伤害应用方法（UpdateHp 现役等价物）
    DumpClassMethods(clsEc, "EC");

    // 全屏（第 9 轮）：OnHitCom 偏移 + CharacterComponentOnHit.OnHitByBullet 钩
    if (kGjc2AoeEnabled) {
        g_offOnHitCom = api.FieldOffset(clsEc, "OnHitCom", 0);
        void* clsOnHit = FindClassAnywhere("CharacterComponentOnHit");
        void* miB = clsOnHit ? api.GetMethod(clsOnHit, "OnHitByBullet", 1) : nullptr;
        void* fnB = MethodPointer(miB);
        if (fnB && tryHook(fnB, (void*)Hooked_OnHitByBullet, (void**)&g_origOnHitByBullet,
                           &g_hookAt[4], "OnHitByBullet", false)) {
            LOGI("[Gjc2] aoe hook OnHitByBullet=%p onHitCom@0x%zx", fnB, g_offOnHitCom);
        } else {
            LOGW("[Gjc2] OnHitByBullet missing (aoe idle) cls=%p", clsOnHit);
        }
    }

    // 偷技能取证轮（8/31）：pending 状态机定位（冷路径钩 + 一次性字段/方法普查）
    if (kGjc2StealProbeEnabled) {
        void* clsHrs = FindClassAnywhere("HeroComponentRandomSkill");
        void* clsBwc = FindClassAnywhere("BattleWorldContext");
        void* clsSel = FindClassAnywhere("SelectSlotCommand");
        if (clsHrs) {
            DumpClassFields(clsHrs, "HRS");
            DumpClassMethods(clsHrs, "HRS");
            void* fnA = MethodPointer(FindMethodNth(clsHrs, "AddSkill", 1, 0));
            void* fnB = MethodPointer(FindMethodNth(clsHrs, "AddSkill", 1, 1));
            if (fnA) {
                tryHook(fnA, (void*)Hooked_AddSkillLive, (void**)&g_origAddSkillLive,
                        &g_hookAt[5], "AddSkill#0", false);
            }
            if (fnB && fnB != fnA) {
                tryHook(fnB, (void*)Hooked_AddSkillLive2, (void**)&g_origAddSkillLive2,
                        &g_hookAt[6], "AddSkill#1", false);
            }
            void* fnAR = MethodPointer(FindMethodNth(clsHrs, "AddRandomSkill", 2, 0));
            if (fnAR) {
                tryHook(fnAR, (void*)Hooked_AddRandomSkill, (void**)&g_origAddRandomSkill,
                        &g_hookAt[7], "AddRandomSkill", false);
            }
            // 候选源：RandomSkillResult.get_ShowSkills 展示瞬间捕获（第 13 轮 extra=3 实锤
            // _randomSkillResult+0x40 不是当前候选）
            void* clsRsr = FindClassAnywhere("RandomSkillResult");
            if (clsRsr) {
                DumpClassFields(clsRsr, "RSR");
                void* fnS = MethodPointer(api.GetMethod(clsRsr, "get_ShowSkills", 0));
                if (fnS) {
                    tryHook(fnS, (void*)Hooked_get_ShowSkillsLive, (void**)&g_origShowSkillsLive,
                            &g_hookAt[8], "get_ShowSkills", false);
                } else {
                    LOGW("[Gjc2] get_ShowSkills missing (steal idle)");
                }
            } else {
                LOGW("[Gjc2] RandomSkillResult missing (steal idle)");
            }
            // 第 19 轮：BWC.AddSkillToCharacter 直授原语（斌哥 8/17 路线复活）
            g_miAddToCharBwc = api.GetMethod(clsBwc, "AddSkillToCharacter", 3);
            g_fnAddToCharBwc = MethodPointer(g_miAddToCharBwc);
            g_offHrsBoxResult = api.FieldOffset(clsHrs, "_boxSkillResult", 0);
            g_offHrsRandomResult = api.FieldOffset(clsHrs, "_randomSkillResult", 0);
            void* clsBox = FindClassAnywhere("BoxSkillResult");
            if (clsBox && g_offHrsBoxResult) {
                g_offBoxId1 = api.FieldOffset(clsBox, "SkillId1", 0);
                g_offBoxId2 = api.FieldOffset(clsBox, "SkillId2", 0);
            }
            if (clsRsr) {
                g_offRsShowSkills =
                    api.FieldOffset(clsRsr, "<ShowSkills>k__BackingField", 0);
            }
            LOGI("[Gjc2] atc mi=%p fn=%p box@0x%zx id1@0x%zx id2@0x%zx rr@0x%zx ss@0x%zx",
                 g_miAddToCharBwc, g_fnAddToCharBwc, g_offHrsBoxResult, g_offBoxId1, g_offBoxId2,
                 g_offHrsRandomResult, g_offRsShowSkills);
            if (!g_fnAddToCharBwc) LOGW("[Gjc2] AddSkillToCharacter missing (steal idle)");
        } else {
            LOGW("[Gjc2] HeroComponentRandomSkill missing (steal probe idle)");
        }
        if (clsBwc) DumpClassFields(clsBwc, "BWC");
        if (clsSel) DumpClassMethods(clsSel, "SSC");
        void* clsHscDump = FindClassAnywhere("HeroSkillCreator");
        if (clsHscDump) DumpClassMethods(clsHscDump, "HSC");
    }
    // 类表落盘（logcat 轮转根治）：装机后一次写 7 个关键类的方法/字段
    DumpTablesToFile();
    // 第 6 轮：EC 伤害链候选纯计数探针（可选钩，缺方法不阻断）
    if (kGjc2DamageProbeEnabled) {
        for (size_t i = 0; i < sizeof(g_damageProbes) / sizeof(g_damageProbes[0]); i++) {
            DamageProbe& p = g_damageProbes[i];
            void* mi = FindMethodNth(clsEc, p.name, p.argc, 0);
            void* fn = MethodPointer(mi);
            if (fn) {
                tryHook(fn, p.stub, p.orig, &g_hookAt[8 + i], p.name, false);
            } else {
                LOGW("[Gjc2] probe %s missing", p.name);
            }
        }
        // 第 7 轮：伤害消息处理器逐方法计数探针（argc≤3 非 getter，池上限 16）
        const char* kProcs[] = {"CharacterOnDamageProcessor", "UpdateDamageMsgProcessor",
                                "HeroDamageUpdateProcessor", "UpdateHpUIProcessor",
                                "CharacterComponentOnHit"};
        for (const char* cn : kProcs) {
            void* k = FindClassAnywhere(cn);
            if (!k) {
                LOGW("[Gjc2] proc %s missing", cn);
                continue;
            }
            DumpClassMethods(k, cn);
            auto& api = GetApi();
            static void* (*class_get_methods_fn)(void*, void**) = nullptr;
            static uint32_t (*param_count_fn)(void*) = nullptr;
            if (!class_get_methods_fn) {
                void* h = dlopen(sec_lib_il2cpp(), RTLD_NOW);
                if (h) {
                    class_get_methods_fn =
                        reinterpret_cast<decltype(class_get_methods_fn)>(
                            dlsym(h, "il2cpp_class_get_methods"));
                    param_count_fn = reinterpret_cast<decltype(param_count_fn)>(
                        dlsym(h, "il2cpp_method_get_param_count"));
                }
            }
            if (!class_get_methods_fn || !api.method_get_name) continue;
            void* iter = nullptr;
            while (void* m = class_get_methods_fn(k, &iter)) {
                if (g_dynProbeN >= 16) break;
                const char* mn = api.method_get_name(m);
                if (!mn) continue;
                if ((mn[0] == 'g' || mn[0] == 's') && mn[1] == 'e' && mn[2] == 't' && mn[3] == '_')
                    continue;
                uint32_t pc = param_count_fn ? param_count_fn(m) : 99;
                if (pc > 3) continue;
                void* fn = MethodPointer(m);
                if (!fn) continue;
                size_t slot = g_dynProbeN;
                if (DobbyHook(fn, g_dynStubs[slot], &g_dynProbes[slot].orig) != 0) continue;
                g_dynProbes[slot].name = mn;
                g_hookAtDyn[slot] = fn;
                g_dynProbeN++;
            }
        }
        DumpClassFields(clsEc, "ECF");
        SweepHotfixClasses();
    }
    LOGI("[Gjc2] hooked invis-only set=%p try=%p grace=2s", fnSet, fnTry);
    __android_log_print(ANDROID_LOG_INFO, "StarryGjc2", "hooked invis-only set=%p try=%p", fnSet,
                        fnTry);
    return true;
}

static void RebindIfMpMoved() {
    if (!g_hooked.load() || !EnsureIl2CppAttached()) return;
    struct Item {
        void** mi;
        void* stub;
        void** orig;
        void** at;
        const char* name;
    } items[] = {
        {&g_miSet, (void*)Hooked_SetTarget, (void**)&g_origSetTarget, &g_hookAt[2], "Set"},
        {&g_miTry, (void*)Hooked_TrySetTarget, (void**)&g_origTrySetTarget, &g_hookAt[3], "Try"},
    };
    for (auto& it : items) {
        if (!*it.mi) continue;
        void* cur = MethodPointer(*it.mi);
        if (!cur || cur == *it.at) continue;
        if (DobbyHook(cur, it.stub, it.orig) == 0) {
            *it.at = cur;
            LOGI("[Gjc2] rebind %s -> %p", it.name, cur);
        }
    }
}

bool Gjc2_TryInit() {
    if (!kGjc2BattleHooksEnabled) {
        if (!g_classesReady.load()) {
            g_classesReady.store(true);
            LOGW("[Gjc2] TryInit stubbed (continue A/B — no Il2Cpp)");
        }
        return true;
    }
    if (g_classesReady.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;
    if (!EnsureIl2CppAttached()) return false;
    void* clsEc = FindBattle("EntityCharacter");
    if (!clsEc) return false;
    g_classesReady.store(true);
    LOGI("[Gjc2] EntityCharacter ready");
    __android_log_print(ANDROID_LOG_INFO, "StarryGjc2", "EntityCharacter ready");
    return true;
}

bool Gjc2_IsReady() { return g_classesReady.load(); }

static void EnsureHooksIfNeeded() {
    if (g_hooked.load()) return;
    if (!g_invis.load() && !g_pushProbe.load()) return;
    if (!CanInstallHooks()) return;
    if (!g_classesReady.load() && !Gjc2_TryInit()) return;
    InstallHooks();
}

static void SyncFeatureFlagsAndMaybeUnhook(bool invis, bool aoe, bool steal) {
    g_invis.store(invis);
    g_aoe.store((kGjc2AoeEnabled) && aoe);
    g_pushProbe.store((kGjc2PushHurtProbeEnabled || kGjc2DamageProbeEnabled || kGjc2AoeEnabled) &&
                      aoe);
    g_steal.store(steal);
    if (!invis && !aoe) UninstallBattleHooks();
}

void Gjc2_OnFeatureChanged(int featNum, bool enabled) {
    if (!g_processStartMs.load()) g_processStartMs.store(NowMs(), std::memory_order_relaxed);
    bool invis = g_invis.load();
    bool aoe = g_aoe.load();
    bool steal = g_steal.load();
    if (featNum == GJC2_FEAT_STEAL_SKILL) steal = enabled;
    if (featNum == GJC2_FEAT_INVIS) invis = enabled;
    if (featNum == GJC2_FEAT_FULLSCREEN) {
        aoe = enabled;
        if (enabled && !kGjc2AoeEnabled)
            __android_log_print(ANDROID_LOG_WARN, "StarryGjc2",
                                "AOE spread off; PushHurt probe %s",
                                kGjc2PushHurtProbeEnabled ? "armed" : "off");
    }
    if (enabled && (featNum == GJC2_FEAT_INVIS || featNum == GJC2_FEAT_FULLSCREEN))
        g_featOnMs.store(NowMs(), std::memory_order_relaxed);
    SyncFeatureFlagsAndMaybeUnhook(invis, aoe, steal);
    __android_log_print(ANDROID_LOG_INFO, "StarryGjc2", "feat %d -> %d hooked=%d", featNum,
                        enabled ? 1 : 0, g_hooked.load() ? 1 : 0);
}

void Gjc2_OnValueChanged(int /*featNum*/, int /*value*/) {}

void Gjc2_Tick(const bool* features) {
    if (!features) return;
    if (!g_processStartMs.load()) g_processStartMs.store(NowMs(), std::memory_order_relaxed);

    const bool invis = features[GJC2_FEAT_INVIS];
    const bool aoe = features[GJC2_FEAT_FULLSCREEN]; // 原始开关；spread/probe 门控在 Sync 内做
    SyncFeatOnGate(invis, aoe);
    SyncFeatureFlagsAndMaybeUnhook(invis, aoe, features[GJC2_FEAT_STEAL_SKILL]);
    if (!invis && !aoe) return;

    if (CanInstallHooks()) {
        if (!g_classesReady.load()) Gjc2_TryInit();
        EnsureHooksIfNeeded();
    }
    if (!g_hooked.load()) return;

    static uint32_t s_tick = 0;
    ++s_tick;
    if ((s_tick % 50) == 0) {
        RebindIfMpMoved();
        // 伤害对象快照倾倒（Tick 线程）：64B hex + 类名，定位 b 记录真实布局
        for (auto& s : g_hurtSnaps) {
            if (!s.used.load(std::memory_order_acquire)) continue;
            char hex[0x40 * 3 + 8] = {0};
            static const char* kHex = "0123456789abcdef";
            size_t o = 0;
            for (size_t i = 0; i + 1 < sizeof(s.bytes) && o + 3 < sizeof(hex); i++) {
                hex[o++] = kHex[s.bytes[i] >> 4];
                hex[o++] = kHex[s.bytes[i] & 0xF];
                hex[o++] = ' ';
            }
            hex[o] = 0;
            LOGI("[Gjc2] hs hurt=%p hero=%p cls=%s", s.hurt, s.hero, s.cls);
            LOGI("[Gjc2] hs bytes %s", hex);
            __android_log_print(ANDROID_LOG_INFO, "StarryGjc2", "hs hurt=%p hero=%p cls=%s",
                                s.hurt, s.hero, s.cls);
            s.used.store(false, std::memory_order_release);
        }
        LOGI("[Gjc2] stats invis=%u push=%u recent=%d ah=%u al=%u gf=%u aoe=%u armed=%d",
             g_hitsInvis.load(), g_hitsPushHurt.load(), g_recentEnemyN.load(),
             g_anchorHero.load(), g_anchorList.load(), g_gateFail.load(), g_hitsAoe.load(),
             EffectsArmed() ? 1 : 0);
        LOGI("[Gjc2] pb up=%u ch=%u bh=%u sk=%u de=%u ev=%u mu=%u tg=%u", g_pbC0.load(),
             g_pbC1.load(), g_pbC2.load(), g_pbC3.load(), g_pbC4.load(), g_pbC5.load(),
             g_pbC6.load(), g_pbC7.load());
        for (size_t i = 0; i < g_dynProbeN && i < 16; i++) {
            uint32_t c = g_dynProbes[i].cnt.load(std::memory_order_relaxed);
            if (c) LOGI("[Gjc2] dp %s=%u", g_dynProbes[i].name ? g_dynProbes[i].name : "?", c);
        }
        // 偷技能取证：授予组件快照 + AddRandomSkill 参数
        for (auto& s : g_skillSnaps) {
            if (!s.used.load(std::memory_order_acquire)) continue;
            const char* cn = CacheClassName(g_hrsCache, s.comp);
            LOGI("[Gjc2] ss comp=%p cls=%s id=%d c=%p d=%p", s.comp, cn ? cn : "?", s.skillId,
                 s.ctxC, s.argD);
            char hex[0x100 * 3 + 8] = {0};
            static const char* kHex = "0123456789abcdef";
            size_t o = 0;
            for (size_t i = 0; i + 1 < sizeof(s.bytes) && o + 3 < sizeof(hex); i++) {
                hex[o++] = kHex[s.bytes[i] >> 4];
                hex[o++] = kHex[s.bytes[i] & 0xF];
                hex[o++] = ' ';
            }
            hex[o] = 0;
            LOGI("[Gjc2] ss bytes %s", hex);
            s.used.store(false, std::memory_order_release);
        }
        if (g_arArgN.load(std::memory_order_relaxed)) {
            LOGI("[Gjc2] ar hits=%u a=%d,%d,%d,%d,%d,%d", g_arHits.load(), g_arArgs[0],
                 g_arArgs[1], g_arArgs[2], g_arArgs[3], g_arArgs[4], g_arArgs[5]);
        }
        LOGI("[Gjc2] as2 hits=%u obj0=%p", g_hitsAddSkill2.load(), g_skillMainSeen[0]);
        __android_log_print(ANDROID_LOG_INFO, "StarryGjc2",
                            "stats invis=%u push=%u recent=%d ah=%u al=%u gf=%u aoe=%u",
                            g_hitsInvis.load(), g_hitsPushHurt.load(), g_recentEnemyN.load(),
                            g_anchorHero.load(), g_anchorList.load(), g_gateFail.load(),
                            g_hitsAoe.load());
    }
}

void Gjc2_ForceClear() {
    g_ssReady.store(false);
    g_ssIdN = 0;
    g_recentEnemyN.store(0);
    g_lastHero.store(nullptr);
    g_lastSkillComp.store(nullptr);
    SyncFeatureFlagsAndMaybeUnhook(false, false, false);
}
