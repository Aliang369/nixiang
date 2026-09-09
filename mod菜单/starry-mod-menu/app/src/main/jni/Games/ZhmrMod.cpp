/* ================================================================
 *  战魂铭人 MOD — 游戏逻辑（3.5.0 重构，骨架 Games/_TemplateMod v2）
 *
 *  3.5.0 实测证据（10_docs/战魂铭人/战魂铭人_3.5.0_复刻记录_20260829.md）：
 *    1) HybridCLR 游戏内调用走解释器分发，不经过 MethodInfo.methodPointer
 *       （Trace 实锤：4 个 hook 装在真函数序言上，战斗全程命中=0）→ 0 Dobby hook
 *    2) runtime_invoke 调热更方法从 starry 线程走解释器 SIGSEGV（tombstone 实锤：
 *       #00-#09 libil2cpp 解释器，#10-#12 libstarry_magic Tick 线程）
 *       → 经济/道具/皮肤也全部直写，0 invoke
 *    3) Body 字段装的是派生类实例（Hero._body=QuanhuyingBody 等）→ 类名判断
 *       必须用 il2cpp_class_is_assignable_from（本地 dlsym，不动共享 Il2CppApi）
 *    4) 未解锁 HeroArchive 的 skill/style 字典为 null → 解锁校验不得要求字典
 *    5) 旧文档「运行偏移」全部比 il2cpp_field_get_offset 原值大 8（旧工程扫描
 *       基址偏差）→ 偏移一律用原值；EncryptValue 用不变式 _showValue==mValue^key 自校验
 *
 *  铁律落点：0 hook / 0 invoke / Tick 轻量（无 SafeProbe）/ 跨对象读全走
 *  process_vm_readv 安全读（坏指针返回失败不崩）/ 失败路径节流日志
 * ================================================================ */

#include "ZhmrMod.h"
#include "Includes/SecStrings.hpp"

#include <atomic>
#include <cstddef>
#include <cstdint>
#include <cstring>
#include <dlfcn.h>
#include <pthread.h>
#include <sys/syscall.h>
#include <sys/uio.h>
#include <unistd.h>

#include "Dobby/dobby.h"

#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;
using Il2Cpp::IsValidHeapPtr;
using Il2Cpp::WriteF32;
using Il2Cpp::WriteU8;
using Il2Cpp::SafeProbe;

#define ZHMR_LOG "[Zhmr]"

// ============================================================
// ① 开关（功能位号见 ZhmrMod.h，与 Main.cpp g_featureList 对齐）
// ============================================================
static std::atomic<bool> g_god{false};
static std::atomic<bool> g_onehit{false};
static std::atomic<bool> g_currency{false};
static std::atomic<bool> g_items{false};
static std::atomic<bool> g_unlock{false};
static std::atomic<bool> g_skin{false};
static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_heroReset{false};  // 无敌关闭后下一 Tick 摘除免伤标志
// 3.6.0 relaunch SIGABRT 根因（tombstone_04 + crash 缓冲 14:08:30，三次同型，
// 反汇编 abort 调用点 0x20d4998 → 断言串 "Collecting from unknown thread"）：
// 我们的 mod_thread 在 libil2cpp 加载前创建，Boehm GC 线程表里没有它；
// il2cpp_init 中途 Tick 首探触碰 il2cpp API 后，UnityMain 一旦触发 GC 收集
// 就会扫到这个「未知线程」→ abort。首启会话 Tick 首探常落在 init 结束后
// （attach 成功 → GC 已知）故幸存；重开会话 init 变慢 → 必撞。
// 修复：Dobby 挂引擎函数 il2cpp_init（非游戏逻辑函数），原函数返回后才
// 允许本线程 attach+探测（attach 会把线程登记进 GC，之后永久安全）。
static std::atomic<bool> g_il2cppInitDone{false};
static std::atomic<bool> g_initHookTried{false};
static bool g_initHookFailed = false;
typedef int (*il2cpp_init_fn)(const char*);
static il2cpp_init_fn g_origIl2cppInit = nullptr;

static int Hooked_Il2cppInit(const char* domainName) {
    int r = g_origIl2cppInit ? g_origIl2cppInit(domainName) : 0;
    LOGI(ZHMR_LOG " il2cpp_init 返回 %d，探测门打开", r);
    if (r == 1) g_il2cppInitDone.store(true);
    return r;
}

// 热更类「可查到」≠「已完成 layout」：类齐后先缓存，隔一轮（≥2s）再读偏移，
// 指针漂移则复位重等（热更重注册防御，保留）。
static std::atomic<bool> g_classesSettled{false};

// 一次性待办（开关 ON 时置位，Tick 里执行一次）
static std::atomic<bool> g_currencyPending{false};
static std::atomic<bool> g_itemsPending{false};
static std::atomic<bool> g_skinPending{false};
static std::atomic<bool> g_skillsPending{false};

// ============================================================
// ② 类 / 字段偏移（全部运行时按名解析，禁猜；一律原值 + 两组兜底）
// ============================================================
struct CombatOff {  // 战斗：Hero→Body→Character 链 + LevelProcess.AllMonster
    size_t hero_body, body_char, char_block, char_hp, char_shield, lp_allmonster;
};
struct ArchiveOff {  // 解锁：HeroArchive
    size_t ha_name, ha_unlock, ha_exp, ha_skillDict, ha_styleDict;
};
struct EvOff {  // EncryptValue：三字段不变式 _showValue == mValue ^ encryptKey
    size_t ev_show, ev_m, ev_key;
};
static CombatOff g_combat[2] = {};
static ArchiveOff g_arch[2] = {};
static EvOff g_ev[2] = {};
static std::atomic<int> g_combatScheme{-1};
static std::atomic<int> g_archScheme{-1};
static std::atomic<int> g_evScheme{-1};

static void* g_heroClass      = nullptr;
static void* g_levelProcess   = nullptr;
static void* g_playerArchive  = nullptr;
static void* g_archiveClass   = nullptr;
static void* g_haClass        = nullptr;
static void* g_bodyClass      = nullptr;
static void* g_charClass      = nullptr;
static void* g_evClass        = nullptr;
static void* g_dataManager    = nullptr;
static void* g_skinEnumClass  = nullptr;  // HeroSkinConfig.HeroSkinName

typedef uint8_t (*class_is_assignable_t)(void* klass, void* objklass);
static class_is_assignable_t g_isAssignable = nullptr;

typedef void* (*object_new_t)(void* klass);
static object_new_t g_objectNew = nullptr;

typedef void* (*class_get_fields_t)(void* klass, void** iter);
typedef const char* (*field_get_name_t)(void* field);
typedef void* (*field_get_value_object_t)(void* field, void* obj);
static class_get_fields_t       g_classGetFields = nullptr;
static field_get_name_t         g_fieldGetName   = nullptr;
static field_get_value_object_t g_fieldGetValue  = nullptr;

typedef void* (*thread_current_t)();
typedef void* (*thread_attach_t)(void*);
static thread_current_t g_threadCurrent = nullptr;
static thread_attach_t  g_threadAttach  = nullptr;
static std::atomic<bool> g_threadAttached{false};

// 无限道具 dic 条目（tag 来自 3.5.0 ResourceTag 枚举，值 999）
struct DicItem {
    int32_t     tag;
    const char* id;
};
static const DicItem kDicItems[] = {
    {14, "SkinPiecesCount"},      // exchangeablePieces 皮肤碎片
    {14, "HP_HeroCommonPiece"},   // exchangeablePieces 通用英雄碎片
    {16, "BattlePassLevelKey"},   // battlePass 历练之钥
    {17, "PP_FoodCount"},         // petParadise 宠物食品
    {17, "PP_ToyCount"},          // petParadise 宠物玩具
    {47, "OrbRefineTicketCount"}, // SixCycleOrb 净化之水
};

static const int kCurrencyValue = 9999;  // anti-ban 安全线（3.4.0 终态值）
static const int kItemsValue    = 999;
static const int kSkinValue     = 1;

// ============================================================
// ③ 安全助手（Tick 路径禁 SafeProbe：跨对象读走内核安全读）
// ============================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

static bool SafeReadMem(const void* addr, void* out, size_t len) {
    if (!OkPtr(const_cast<void*>(addr))) return false;
    iovec local{out, len};
    iovec remote{const_cast<void*>(addr), len};
    return syscall(__NR_process_vm_readv, getpid(), &local, 1, &remote, 1, 0) ==
           static_cast<ssize_t>(len);
}

static bool SafeReadPtr(const void* addr, void** out) { return SafeReadMem(addr, out, 8); }

static bool SafeReadI32(const void* addr, int32_t* out) { return SafeReadMem(addr, out, 4); }

static void SafeWriteF32(void* obj, size_t off, float v) {
    if (OkPtr(obj) && off) memcpy(reinterpret_cast<uint8_t*>(obj) + off, &v, sizeof(v));
}

// 类名缓存：校验只在低频 Tick 路径调用，禁止每帧解析
struct ClassNameCache {
    void* klass = nullptr;
    char  name[48] = {};
    bool  valid = false;
};

static const char* CacheClassName(ClassNameCache& cache, void* obj) {
    if (!OkPtr(obj)) return nullptr;
    auto& api = GetApi();
    void* k = nullptr;
    if (!SafeReadPtr(obj, &k) || !k || !api.class_name) return nullptr;
    uint64_t probe = 0;
    if (!SafeReadMem(k, &probe, 8)) return nullptr;
    if (!SafeReadMem(reinterpret_cast<uint8_t*>(k) + 8, &probe, 8)) return nullptr;
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

// 派生类判断：klass 指针白名单表。新 klass 首次出现才走一次 il2cpp 登记
// （attach 后低频调用），稳态 Tick 对已登记类零 il2cpp 调用。
struct KlassSlot {
    void* klass;
    void* base;
    bool  ok;
};
static KlassSlot g_klassTable[32];
static std::atomic<int> g_klassCount{0};

static bool IsKindOfCached(void* obj, void* baseKlass) {
    if (!OkPtr(obj) || !baseKlass) return false;
    void* k = nullptr;
    if (!SafeReadPtr(obj, &k) || !k) return false;
    uint64_t probe = 0;
    if (!SafeReadMem(k, &probe, 8)) return false;
    if (!SafeReadMem(reinterpret_cast<uint8_t*>(k) + 8, &probe, 8)) return false;
    int n = g_klassCount.load();
    for (int i = 0; i < n; i++) {
        if (g_klassTable[i].klass == k && g_klassTable[i].base == baseKlass)
            return g_klassTable[i].ok;
    }
    bool ok = false;
    if (g_isAssignable) {
        ok = g_isAssignable(baseKlass, k) != 0;
    } else {
        static ClassNameCache fbCache;
        const char* nm = CacheClassName(fbCache, obj);
        ok = nm && strstr(nm, "Body") != nullptr;
    }
    if (n < static_cast<int>(sizeof(g_klassTable) / sizeof(g_klassTable[0]))) {
        g_klassTable[n].klass = k;
        g_klassTable[n].base = baseKlass;
        g_klassTable[n].ok = ok;
        g_klassCount.store(n + 1);
    }
    static int s_reg = 0;
    if (s_reg < 5) {
        s_reg++;
        LOGI(ZHMR_LOG " [TRACE] klass 登记 %s (base=%s)", ok ? "✓" : "✗",
             baseKlass == g_bodyClass ? "Body" : (baseKlass == g_charClass ? "Character" : "?"));
    }
    return ok;
}

static const CombatOff* ActiveCombat() {
    int s = g_combatScheme.load();
    if (s < 0) return nullptr;
    return &g_combat[s ? 1 : 0];
}

static const EvOff* ActiveEv() {
    int s = g_evScheme.load();
    if (s < 0) return nullptr;
    return &g_ev[s ? 1 : 0];
}

// 注意：不缓存 LevelProcess/Hero 单例实例——换角色/新开一局会换实例，
// klass 校验无法发现（同类新实例），写旧对象=无效+内存腐蚀。
// 每秒经 StaticRefField 重新解析（attach 后多会话 5s 频率实证稳定）。

// 战斗组 bias 实证：Hero._body 链路逐跳校验（Body→Character）
static bool ValidateChain(const CombatOff& o, void** outCh) {
    auto& api = GetApi();
    void* hero = api.StaticRefField(g_heroClass, "singleton");
    if (!OkPtr(hero)) {
        static int s_a = 0;
        if (s_a < 3) {
            s_a++;
            LOGI(ZHMR_LOG " [链路] singleton=null（主菜单？等待进关卡）");
        }
        return false;
    }
    (void)0;
    void* body = nullptr;
    if (!SafeReadPtr(reinterpret_cast<uint8_t*>(hero) + o.hero_body, &body) ||
        !IsKindOfCached(body, g_bodyClass)) {
        static int s_b = 0;
        if (++s_b % 10 == 1)
            LOGI(ZHMR_LOG " [链路] body@%zu 读失败或非 Body 派生 body=%p", o.hero_body, body);
        return false;
    }
    void* ch = nullptr;
    if (!SafeReadPtr(reinterpret_cast<uint8_t*>(body) + o.body_char, &ch) ||
        !IsKindOfCached(ch, g_charClass)) {
        static int s_c = 0;
        if (++s_c % 10 == 1)
            LOGI(ZHMR_LOG " [链路] char@%zu 读失败或非 Character 派生 ch=%p", o.body_char, ch);
        return false;
    }
    *outCh = ch;
    return true;
}

static void DecideCombatSchemeIfNeeded() {
    if (g_combatScheme.load() >= 0) return;
    void* ch = nullptr;
    for (int s = 0; s < 2; s++) {
        if (!g_combat[s].hero_body) continue;
        if (ValidateChain(g_combat[s], &ch)) {
            g_combatScheme.store(s);
            LOGI(ZHMR_LOG " 战斗 bias 方案实证: %s (hero_body@%zu char@%zu)",
                 s ? "raw+8" : "raw", g_combat[s].hero_body, g_combat[s].body_char);
            return;
        }
    }
}

// ============================================================
// ⑤ 战斗：单例缓存 + 字段直写（0 hook 0 invoke；稳态 Tick 零 il2cpp 调用）
// ============================================================
static void ApplyGod(bool on) {
    const CombatOff* o = ActiveCombat();
    if (!o) return;
    // 每秒重新解析：换角色/新局会换 Hero 实例，缓存会写旧对象
    void* ch = nullptr;
    if (!ValidateChain(*o, &ch)) return;
    if (o->char_block) WriteU8(ch, o->char_block, on ? 1 : 0);
    if (on && o->char_hp) SafeWriteF32(ch, o->char_hp, 999999.0f);
    static int s_log = 0;
    if (s_log < 3) {
        s_log++;
        LOGI(ZHMR_LOG " ApplyGod(%d) ch=%p", on ? 1 : 0, ch);
    }
}

static void ApplyOneHit() {
    const CombatOff* o = ActiveCombat();
    if (!o || !o->lp_allmonster) return;
    // AllMonster 是按房间重建的（进房换新 List），所以不能缓存 List 本身——
    // 缓存 LevelProcess 实例（整关不变），每秒重读实例上的字段（纯安全读）。
    auto& api = GetApi();
    void* lp = api.StaticRefField(g_levelProcess, "singleton");
    if (!OkPtr(lp)) {
        static int s_a = 0;
        if (s_a < 3) LOGI(ZHMR_LOG " [链路] LevelProcess.singleton=null（不在关卡内？）");
        s_a++;
        return;
    }
    void* list = nullptr;
    if (!SafeReadPtr(reinterpret_cast<uint8_t*>(lp) + o->lp_allmonster, &list) || !OkPtr(list)) {
        static int s_b = 0;
        if (s_b < 3) LOGI(ZHMR_LOG " [链路] AllMonster@%zu 读失败 list=%p", o->lp_allmonster, list);
        s_b++;
        return;
    }
    // List<T> 布局：klass@0 monitor@8 _items@0x10 _size@0x18；数组数据 @+0x20
    void* items = nullptr;
    int32_t size = 0;
    if (!SafeReadPtr(reinterpret_cast<uint8_t*>(list) + 0x10, &items)) return;
    if (!SafeReadI32(reinterpret_cast<uint8_t*>(list) + 0x18, &size)) return;
    if (!OkPtr(items) || size <= 0 || size > 500) {
        static int s_c = 0;
        if (++s_c % 10 == 1) LOGI(ZHMR_LOG " [链路] AllMonster items=%p size=%d", items, size);
        return;
    }
    int hit = 0;
    for (int32_t i = 0; i < size; i++) {
        void* body = nullptr;
        if (!SafeReadPtr(reinterpret_cast<uint8_t*>(items) + 0x20 + static_cast<size_t>(i) * 8,
                         &body))
            continue;
        if (!IsKindOfCached(body, g_bodyClass)) continue;
        void* ch = nullptr;
        if (!SafeReadPtr(reinterpret_cast<uint8_t*>(body) + o->body_char, &ch) ||
            !IsKindOfCached(ch, g_charClass))
            continue;
        if (o->char_hp) WriteF32(ch, o->char_hp, 0.1f);
        if (o->char_shield) WriteF32(ch, o->char_shield, 0.0f);
        hit++;
    }
    static int s_log = 0;
    if (s_log < 3 || s_log % 20 == 0) {
        s_log++;
        LOGI(ZHMR_LOG " ApplyOneHit size=%d hit=%d", (int)size, hit);
    } else {
        s_log++;
    }
}

// ============================================================
// ⑥ EncryptValue 直写（三字段一致；不变式自校验防写错偏移）
// ============================================================
static bool EvWrite(void* ev, int32_t value, const EvOff& e) {
    if (!OkPtr(ev) || !e.ev_show || !e.ev_m || !e.ev_key) return false;
    int32_t show = 0, m = 0, key = 0;
    if (!SafeReadMem(reinterpret_cast<uint8_t*>(ev) + e.ev_show, &show, 4)) return false;
    if (!SafeReadMem(reinterpret_cast<uint8_t*>(ev) + e.ev_m, &m, 4)) return false;
    if (!SafeReadMem(reinterpret_cast<uint8_t*>(ev) + e.ev_key, &key, 4)) return false;
    if (show != (m ^ key)) return false;  // 不变式不成立 = 偏移错/对象错，拒绝写
    memcpy(reinterpret_cast<uint8_t*>(ev) + e.ev_key, &key, 4);  // key 不变
    int32_t nv = value ^ key;
    memcpy(reinterpret_cast<uint8_t*>(ev) + e.ev_m, &nv, 4);
    memcpy(reinterpret_cast<uint8_t*>(ev) + e.ev_show, &value, 4);
    return true;
}

// 读 PlayerArchive 静态 EncryptValue 字段并直写（字段名直接给 StaticRefField）
static void ApplyStaticEv(const char* tag, int32_t value) {
    auto& api = GetApi();
    void* ev = api.StaticRefField(g_playerArchive, tag);
    if (!OkPtr(ev)) return;
    const EvOff* e = ActiveEv();
    if (!e) return;
    if (EvWrite(ev, value, *e)) {
        static int s_log = 0;
        if (s_log < 6) {
            s_log++;
            LOGI(ZHMR_LOG " [TRACE] %s = %d (直写)", tag, value);
        }
    }
}

// Il2CppString：length@0x10，UTF-16 字符 @0x14
static bool Il2cppStrEquals(void* str, const char* ascii) {
    if (!OkPtr(str)) return false;
    int32_t len = 0;
    if (!SafeReadI32(reinterpret_cast<uint8_t*>(str) + 0x10, &len)) return false;
    if (len < 0 || len > 256) return false;
    size_t want = strlen(ascii);
    if (static_cast<size_t>(len) != want) return false;
    uint16_t buf[256];
    if (!SafeReadMem(reinterpret_cast<uint8_t*>(str) + 0x14, buf, want * 2)) return false;
    for (size_t i = 0; i < want; i++) {
        if (buf[i] != static_cast<uint8_t>(ascii[i])) return false;
    }
    return true;
}

// ============================================================
// ⑥b Dictionary 插入工具（corlib AOT set_Item 原生指针直调，不走解释器——
//     runtime_invoke 走解释器在我们线程必炸，tombstone 实锤）
// ============================================================
static bool DictSetItem(void* dict, void* keyArg, void* valArg) {
    if (!OkPtr(dict)) return false;
    void* k = nullptr;
    if (!SafeReadPtr(dict, &k) || !k) return false;
    static void* s_cachedKlass = nullptr;
    static void* s_cachedFn = nullptr;
    static void* s_cachedMi = nullptr;
    if (k != s_cachedKlass) {
        auto& api = GetApi();
        void* mi = api.GetMethod(k, "set_Item", 2);
        if (!mi) return false;
        void* fn = *reinterpret_cast<void**>(mi);
        if (!OkPtr(fn)) return false;
        s_cachedKlass = k;
        s_cachedFn = fn;
        s_cachedMi = mi;
    }
    typedef void (*SetItem_t)(void*, void*, void*, void*);
    reinterpret_cast<SetItem_t>(s_cachedFn)(dict, keyArg, valArg, s_cachedMi);
    return true;
}

// 手动构造 EncryptValue（ctor 只填三字段，object_new 后手填等价；
// key 模拟游戏 Random.Range(200,500)，保持 _showValue==mValue^key 不变式）
static void* EvCreate(int32_t value) {
    const EvOff* e = ActiveEv();
    if (!e || !g_objectNew || !g_evClass) return nullptr;
    void* ev = g_objectNew(g_evClass);
    if (!OkPtr(ev)) return nullptr;
    int32_t key = 200 + (value % 300);
    int32_t m = value ^ key;
    memcpy(reinterpret_cast<uint8_t*>(ev) + e->ev_show, &value, 4);
    memcpy(reinterpret_cast<uint8_t*>(ev) + e->ev_m, &m, 4);
    memcpy(reinterpret_cast<uint8_t*>(ev) + e->ev_key, &key, 4);
    return ev;
}

// _resourceDic 遍历：tag(int32)→内层 Dictionary<string,EncryptValue>→id 字符串→直写；
// 条目/字典缺失时用 set_Item 创建（AOT 直调）
static int ApplyDicItems(int32_t tag, const char* id, int32_t value) {
    auto& api = GetApi();
    const EvOff* e = ActiveEv();
    if (!e) return -1;
    void* dic = api.StaticRefField(g_playerArchive, GameUnity::Fields::PA_RESOURCE_DIC);
    if (!OkPtr(dic)) return -1;
    void* entries = nullptr;
    int32_t count = 0;
    if (!SafeReadPtr(reinterpret_cast<uint8_t*>(dic) + 0x18, &entries)) return -1;
    if (!SafeReadI32(reinterpret_cast<uint8_t*>(dic) + 0x20, &count)) return -1;
    if (!OkPtr(entries) || count <= 0 || count > 512) return -1;
    void* inner = nullptr;
    void* innerTmpl = nullptr;
    for (int32_t i = 0; i < count; i++) {
        const uint8_t* entry =
            reinterpret_cast<uint8_t*>(entries) + 0x20 + static_cast<size_t>(i) * 0x18;
        int32_t key = -1;
        void* val = nullptr;
        if (!SafeReadI32(entry + 0x8, &key)) continue;
        if (!SafeReadPtr(entry + 0x10, &val)) continue;
        if (key == tag && OkPtr(val)) {
            inner = val;
            break;
        }
        if (!innerTmpl && OkPtr(val)) innerTmpl = val;  // 内层字典类型模板
    }
    if (!inner) {
        // tag 字典不存在：按已有内层字典类型创建并挂到外层
        if (!innerTmpl || !g_objectNew) return 0;
        void* tmplK = nullptr;
        if (!SafeReadPtr(innerTmpl, &tmplK) || !OkPtr(tmplK)) return 0;
        void* newInner = g_objectNew(tmplK);
        if (!OkPtr(newInner)) return 0;
        int32_t tagKey = tag;
        if (!DictSetItem(dic, &tagKey, newInner)) return -1;
        inner = newInner;
        LOGI(ZHMR_LOG " [TRACE] 新建 tag=%d 字典", tag);
    }
    void* iEntries = nullptr;
    int32_t iCount = 0;
    if (!SafeReadPtr(reinterpret_cast<uint8_t*>(inner) + 0x18, &iEntries)) return -1;
    if (!SafeReadI32(reinterpret_cast<uint8_t*>(inner) + 0x20, &iCount)) return -1;
    if (OkPtr(iEntries) && iCount > 0 && iCount <= 1024) {
        for (int32_t i = 0; i < iCount; i++) {
            const uint8_t* entry =
                reinterpret_cast<uint8_t*>(iEntries) + 0x20 + static_cast<size_t>(i) * 0x18;
            void* keyStr = nullptr;
            void* ev = nullptr;
            if (!SafeReadPtr(entry + 0x8, &keyStr)) continue;
            if (!Il2cppStrEquals(keyStr, id)) continue;
            if (!SafeReadPtr(entry + 0x10, &ev) || !OkPtr(ev)) continue;
            return EvWrite(ev, value, *e) ? 1 : -1;
        }
    }
    // 条目不存在：创建 EncryptValue 并插入
    void* ev = EvCreate(value);
    if (!ev) return -1;
    void* sid = api.string_new ? api.string_new(id) : nullptr;
    if (!sid) return -1;
    if (!DictSetItem(inner, sid, ev)) return -1;
    return 1;
}

static void ApplyCurrency() {
    ApplyStaticEv("_exSoulStoneCount", kCurrencyValue);
    ApplyStaticEv("_exBlueStoneCount", kCurrencyValue);
    ApplyStaticEv("_exRedStoneCount", kCurrencyValue);
    LOGI(ZHMR_LOG " 货币重放(直写) 值=%d", kCurrencyValue);
}

static void ApplyItems() {
    ApplyStaticEv("_exWeaponUpgradeTicket", kItemsValue);
    ApplyStaticEv("_exRebornCard", kItemsValue);
    ApplyStaticEv("_exProbationTicket", kItemsValue);
    int ok = 0, miss = 0;
    for (const DicItem& it : kDicItems) {
        int r = ApplyDicItems(it.tag, it.id, kItemsValue);
        if (r == 1) ok++;
        else if (r == 0) miss++;
    }
    LOGI(ZHMR_LOG " 道具重放(直写+建条目) ok=%d miss=%d/6 值=%d", ok, miss, kItemsValue);
}

static void ApplySkin() {
    auto& api = GetApi();
    if (!g_dataManager || !g_skinEnumClass || !g_classGetFields || !g_fieldGetName ||
        !g_fieldGetValue) {
        LOGW(ZHMR_LOG " 皮肤配置枚举 API 不全");
        return;
    }
    // HeroSkinName 枚举值→名字映射（一次构建；名字即 _resourceDic 的 id）
    static int32_t s_vals[512];
    static char s_names[512][40];
    static int s_mapN = -1;
    if (s_mapN < 0) {
        s_mapN = 0;
        void* iter = nullptr;
        void* f = nullptr;
        while (s_mapN < 512 && (f = g_classGetFields(g_skinEnumClass, &iter)) != nullptr) {
            const char* n = g_fieldGetName(f);
            if (!n || strcmp(n, "value__") == 0) continue;
            void* boxed = g_fieldGetValue(f, nullptr);
            int32_t v = -1;
            if (!OkPtr(boxed) || !SafeReadI32(reinterpret_cast<uint8_t*>(boxed) + 0x10, &v))
                continue;
            strncpy(s_names[s_mapN], n, sizeof(s_names[s_mapN]) - 1);
            s_vals[s_mapN] = v;
            s_mapN++;
        }
        LOGI(ZHMR_LOG " [TRACE] 皮肤枚举映射 %d 项", s_mapN);
        for (int k = 0; k < s_mapN && k < 12; k++)
            LOGI(ZHMR_LOG " [映射采样] %d = %s", s_vals[k], s_names[k]);
    }
    void* cfg = api.StaticRefField(g_dataManager, GameUnity::Fields::DM_SKIN_CONFIG);
    if (!OkPtr(cfg)) return;
    void* entries = nullptr;
    int32_t count = 0;
    if (!SafeReadPtr(reinterpret_cast<uint8_t*>(cfg) + 0x18, &entries)) return;
    if (!SafeReadI32(reinterpret_cast<uint8_t*>(cfg) + 0x20, &count)) return;
    if (!OkPtr(entries) || count <= 0 || count > 1024) return;
    // HeroSkinConfig 是 struct（值类型）→ Entry stride 不是 0x18（实测 0x60）。
    // 步长自检：key0=0 在 +0x8，找 key1 定 stride，再按步长遍历。
    int64_t stride = 0;
    {
        // 字典枚举顺序≠插入顺序，首 key 未必是 0。候选步长上做「key 全落在
        // [0,1024) 且互不相同」的集合校验，首个通过的即真实步长。
        const uint8_t* base = reinterpret_cast<uint8_t*>(entries) + 0x20;
        for (int64_t s = 0x18; s <= 0x400; s += 4) {
            bool okS = true;
            static int32_t seen[512];
            int seenN = 0;
            for (int32_t i = 0; i < count && okS; i++) {
                int32_t kv = -1;
                if (!SafeReadI32(base + 0x8 + static_cast<int64_t>(i) * s, &kv) || kv < 0 ||
                    kv >= 1024) {
                    okS = false;
                    break;
                }
                for (int q = 0; q < seenN; q++) {
                    if (seen[q] == kv) {
                        okS = false;
                        break;
                    }
                }
                if (seenN < 512) seen[seenN++] = kv;
            }
            if (okS && seenN == count) {
                stride = s;
                break;
            }
        }
        if (stride <= 0x10) {
            LOGI(ZHMR_LOG " 配置 stride 自检失败");
            return;
        }
        LOGI(ZHMR_LOG " [TRACE] 配置 Entry stride=0x%llx count=%d", (long long)stride, count);
    }
    int ok = 0, nameMiss = 0, writeFail = 0;
    char failNames[4][40] = {{0}};
    int failN = 0;
    for (int32_t i = 0; i < count; i++) {
        const uint8_t* entry =
            reinterpret_cast<uint8_t*>(entries) + 0x20 + static_cast<size_t>(i) * stride;
        int32_t skinVal = -1;
        if (!SafeReadI32(entry + 0x8, &skinVal)) continue;
        const char* name = nullptr;
        for (int j = 0; j < s_mapN; j++) {
            if (s_vals[j] == skinVal) {
                name = s_names[j];
                break;
            }
        }
        if (!name) {
            nameMiss++;
            continue;
        }
        if (ApplyDicItems(1 /*ResourceTag.HeroSkin*/, name, kSkinValue) == 1) {
            ok++;
        } else {
            writeFail++;
            if (failN < 4) {
                strncpy(failNames[failN], name, sizeof(failNames[failN]) - 1);
                failN++;
            }
        }
    }
    LOGI(ZHMR_LOG " 皮肤重放 ok=%d 名字缺失=%d 写入失败=%d 例:%s/%s/%s",
         ok, nameMiss, writeFail, failNames[0], failNames[1], failNames[2]);
}

// ============================================================
// ⑥c 技能/风格解锁：填充 HeroArchive.skill/style 字典
//     IsSkillUnlock/IsStyleUnlock = 字典 ContainsKey（反编译实锤）；
//     配置源 Archive.HeroBodySkillNameLibrary / HeroSkillStyleNameLibrary
//     （静态字段，游戏懒加载——先开一次选人界面再开开关）
// ============================================================
static bool IntArrayGet(void* arr, int32_t idx, int32_t* out) {
    if (!OkPtr(arr)) return false;
    int32_t len = 0;
    if (!SafeReadI32(reinterpret_cast<uint8_t*>(arr) + 0x18, &len)) return false;
    if (idx < 0 || idx >= len) return false;
    return SafeReadI32(reinterpret_cast<uint8_t*>(arr) + 0x20 + static_cast<size_t>(idx) * 4, out);
}

// Dictionary<int32key, ...>：按 key 找 value（key@+0x8 value@+0x10）
static void* DicLookupInt(void* dic, int32_t key) {
    if (!OkPtr(dic)) return nullptr;
    void* entries = nullptr;
    int32_t count = 0;
    if (!SafeReadPtr(reinterpret_cast<uint8_t*>(dic) + 0x18, &entries)) return nullptr;
    if (!SafeReadI32(reinterpret_cast<uint8_t*>(dic) + 0x20, &count)) return nullptr;
    if (!OkPtr(entries) || count <= 0 || count > 2048) return nullptr;
    for (int32_t i = 0; i < count; i++) {
        const uint8_t* entry =
            reinterpret_cast<uint8_t*>(entries) + 0x20 + static_cast<size_t>(i) * 0x18;
        int32_t k = -1;
        void* v = nullptr;
        if (!SafeReadI32(entry + 0x8, &k)) continue;
        if (k != key) continue;
        if (!SafeReadPtr(entry + 0x10, &v)) return nullptr;
        return v;
    }
    return nullptr;
}

static void WritePtrSafe(void* obj, size_t off, void* v) {
    if (OkPtr(obj) && off) *reinterpret_cast<void**>(reinterpret_cast<uint8_t*>(obj) + off) = v;
}

static bool ApplyHeroSkills() {
    int scheme = g_archScheme.load();
    if (scheme < 0) return false;
    const ArchiveOff& o = g_arch[scheme ? 1 : 0];
    auto& api = GetApi();
    void* dic = api.StaticRefField(g_archiveClass, GameUnity::Fields::ARCHIVE_HERO_DIC);
    if (!OkPtr(dic)) return false;
    void* skillLib = api.StaticRefField(g_archiveClass, GameUnity::Fields::ARCHIVE_SKILL_LIB);
    void* styleLib = api.StaticRefField(g_archiveClass, GameUnity::Fields::ARCHIVE_STYLE_LIB);
    if (!OkPtr(skillLib) || !OkPtr(styleLib)) return false;
    int32_t libN = 0;
    if (!SafeReadI32(reinterpret_cast<uint8_t*>(skillLib) + 0x20, &libN) || libN <= 0) {
        LOGI(ZHMR_LOG " 技能库未懒加载（先开一次选人界面）");
        return false;
    }
    void* entries = nullptr;
    int32_t count = 0;
    if (!SafeReadPtr(reinterpret_cast<uint8_t*>(dic) + 0x18, &entries)) return false;
    if (!SafeReadI32(reinterpret_cast<uint8_t*>(dic) + 0x20, &count)) return false;
    if (!OkPtr(entries) || count <= 0 || count > 512) return false;

    // 字典 klass 模板：任一已持有非空字典的英雄
    void* skillTmpl = nullptr;
    void* styleTmpl = nullptr;
    for (int32_t i = 0; i < count && (!skillTmpl || !styleTmpl); i++) {
        const uint8_t* entry =
            reinterpret_cast<uint8_t*>(entries) + 0x20 + static_cast<size_t>(i) * 0x18;
        void* ha = nullptr;
        if (!SafeReadPtr(entry + 0x10, &ha) || !OkPtr(ha)) continue;
        void* sd = nullptr;
        void* td = nullptr;
        if (!skillTmpl && SafeReadPtr(reinterpret_cast<uint8_t*>(ha) + o.ha_skillDict, &sd))
            skillTmpl = sd;
        if (!styleTmpl && SafeReadPtr(reinterpret_cast<uint8_t*>(ha) + o.ha_styleDict, &td))
            styleTmpl = td;
    }
    int heroes = 0, inserts = 0, styleInserts = 0, styleLibMiss = 0, styleDictMiss = 0,
        skillLibMiss = 0;
    for (int32_t i = 0; i < count; i++) {
        const uint8_t* entry =
            reinterpret_cast<uint8_t*>(entries) + 0x20 + static_cast<size_t>(i) * 0x18;
        void* ha = nullptr;
        if (!SafeReadPtr(entry + 0x10, &ha) || !OkPtr(ha)) continue;
        uint8_t isU = 0;
        if (!SafeReadMem(reinterpret_cast<uint8_t*>(ha) + o.ha_unlock, &isU, 1) || isU != 1)
            continue;
        int32_t bodyName = -1;
        if (!SafeReadMem(reinterpret_cast<uint8_t*>(ha) + o.ha_name, &bodyName, 4)) continue;
        void* skills = DicLookupInt(skillLib, bodyName);          // SkillName[]
        void* styleOf = DicLookupInt(styleLib, bodyName);         // Dict<SkillName, Style[]>
        if (!skills) {
            skillLibMiss++;
            continue;
        }
        if (!OkPtr(styleOf)) styleLibMiss++;
        void* sd = nullptr;
        void* td = nullptr;
        if (!SafeReadPtr(reinterpret_cast<uint8_t*>(ha) + o.ha_skillDict, &sd) || !OkPtr(sd)) {
            if (!skillTmpl || !g_objectNew) continue;
            sd = g_objectNew(skillTmpl);
            if (!OkPtr(sd)) continue;
            WritePtrSafe(ha, o.ha_skillDict, sd);
        }
        if (SafeReadPtr(reinterpret_cast<uint8_t*>(ha) + o.ha_styleDict, &td) && !OkPtr(td) &&
            styleTmpl && g_objectNew) {
            td = g_objectNew(styleTmpl);
            if (OkPtr(td)) WritePtrSafe(ha, o.ha_styleDict, td);
        }
        if (!OkPtr(td)) styleDictMiss++;
        heroes++;
        // 游戏创建英雄时已把全部技能/风格以 false 预填进字典（DUMP 实锤）。
        // Dictionary<枚举,bool> 的 Entry stride=0x10（hash@0 next@4 key@8 value@12），
        // 原地翻转 value 字节即可——禁用 set_Item（值类型闭包泛型是解释器桩，直调无效）。
        int32_t want[192];
        int wantN = 0;
        int32_t n = 0;
        if (!SafeReadI32(reinterpret_cast<uint8_t*>(skills) + 0x18, &n)) continue;
        for (int32_t j = 0; j < n && wantN < 192; j++) {
            int32_t skillId = -1;
            if (IntArrayGet(skills, j, &skillId) && wantN < 192) want[wantN++] = skillId;
            if (OkPtr(styleOf)) {
                void* styles = DicLookupInt(styleOf, skillId);
                int32_t sn = 0;
                if (OkPtr(styles) &&
                    SafeReadI32(reinterpret_cast<uint8_t*>(styles) + 0x18, &sn)) {
                    for (int32_t q = 0; q < sn && q < 64 && wantN < 192; q++) {
                        int32_t styleId = -1;
                        if (IntArrayGet(styles, q, &styleId) && wantN < 192)
                            want[wantN++] = styleId;
                    }
                }
            }
        }
        // 翻转技能字典
        {
            void* e2 = nullptr;
            int32_t c2 = 0;
            if (SafeReadPtr(reinterpret_cast<uint8_t*>(sd) + 0x18, &e2) &&
                SafeReadI32(reinterpret_cast<uint8_t*>(sd) + 0x20, &c2) && OkPtr(e2) &&
                c2 > 0 && c2 <= 2048) {
                for (int32_t q = 0; q < c2; q++) {
                    const uint8_t* ee =
                        reinterpret_cast<uint8_t*>(e2) + 0x20 + static_cast<size_t>(q) * 0x10;
                    int32_t kk = -1;
                    if (!SafeReadI32(ee + 0x8, &kk)) continue;
                    for (int w = 0; w < wantN; w++) {
                        if (want[w] == kk) {
                            WriteU8(reinterpret_cast<void*>(const_cast<uint8_t*>(ee)), 0xC, 1);
                            inserts++;
                            break;
                        }
                    }
                }
            }
        }
        // 翻转风格字典
        if (OkPtr(td)) {
            void* e2 = nullptr;
            int32_t c2 = 0;
            if (SafeReadPtr(reinterpret_cast<uint8_t*>(td) + 0x18, &e2) &&
                SafeReadI32(reinterpret_cast<uint8_t*>(td) + 0x20, &c2) && OkPtr(e2) &&
                c2 > 0 && c2 <= 2048) {
                for (int32_t q = 0; q < c2; q++) {
                    const uint8_t* ee =
                        reinterpret_cast<uint8_t*>(e2) + 0x20 + static_cast<size_t>(q) * 0x10;
                    int32_t kk = -1;
                    if (!SafeReadI32(ee + 0x8, &kk)) continue;
                    for (int w = 0; w < wantN; w++) {
                        if (want[w] == kk) {
                            WriteU8(reinterpret_cast<void*>(const_cast<uint8_t*>(ee)), 0xC, 1);
                            styleInserts++;
                            break;
                        }
                    }
                }
            } else {
                styleDictMiss++;
            }
        }
    }
    static int s_fillLog = 0;
    if (s_fillLog < 2) {
        s_fillLog++;
        LOGI(ZHMR_LOG " 技能/风格填充(翻转): heroes=%d 技能=%d 风格=%d 字典缺=%d",
             heroes, inserts - styleInserts, styleInserts, styleDictMiss);
    }
    return heroes > 0;
}

// ============================================================
// ⑦ 解锁全角色：heroArchiveDic 遍历直写 _isUnlock（未解锁条目字典可能为 null，
//    不得用字典类名做过滤——实锤过滤掉 32 个未解锁英雄）
// ============================================================
static int CountUnlockCandidates(const ArchiveOff& o, bool doWrite) {
    auto& api = GetApi();
    void* dic = api.StaticRefField(g_archiveClass, GameUnity::Fields::ARCHIVE_HERO_DIC);
    if (!OkPtr(dic)) return -1;
    void* entries = nullptr;
    int32_t count = 0;
    if (!SafeReadPtr(reinterpret_cast<uint8_t*>(dic) + 0x18, &entries)) return -1;
    if (!SafeReadI32(reinterpret_cast<uint8_t*>(dic) + 0x20, &count)) return -1;
    if (!OkPtr(entries) || count <= 0 || count > 512) return -1;
    int valid = 0, written = 0;
    for (int32_t i = 0; i < count; i++) {
        const uint8_t* entry =
            reinterpret_cast<uint8_t*>(entries) + 0x20 + static_cast<size_t>(i) * 0x18;
        void* ha = nullptr;
        if (!SafeReadPtr(entry + 0x10, &ha) || !OkPtr(ha)) continue;
        uint8_t isU = 2;
        // 条目来自 heroArchiveDic 本体，天生是 HeroArchive；只校验解锁布尔
        // （name/exp 哨兵值会把未解锁档案拒掉——valid=11/43 实锤）
        if (o.ha_unlock && !SafeReadMem(reinterpret_cast<uint8_t*>(ha) + o.ha_unlock, &isU, 1))
            continue;
        if (isU > 1) continue;
        valid++;
        if (doWrite && isU == 0) {
            WriteU8(ha, o.ha_unlock, 1);
            written++;
        }
    }
    if (doWrite) LOGI(ZHMR_LOG " 解锁重放: entries=%d valid=%d written=%d", (int)count, valid, written);
    return valid;
}

static void ApplyUnlockHero() {
    int scheme = g_archScheme.load();
    if (scheme >= 0) {
        CountUnlockCandidates(g_arch[scheme ? 1 : 0], true);
        return;
    }
    int v0 = CountUnlockCandidates(g_arch[0], false);
    int v1 = (g_arch[1].ha_unlock) ? CountUnlockCandidates(g_arch[1], false) : -1;
    if (v0 <= 0 && v1 <= 0) return;
    int s = (v1 > v0) ? 1 : 0;
    g_archScheme.store(s);
    LOGI(ZHMR_LOG " 存档 bias 方案实证(dic): %s valid=%d/%d", s ? "raw+8" : "raw",
         s ? v1 : v0, s ? v0 : v1);
    CountUnlockCandidates(g_arch[s], true);
}

// ============================================================
// ⑧ init（一次性解析；SafeProbe 仅此一处）
// ============================================================
static bool ResolveAll() {
    auto& api = GetApi();
    if (!api.Load(GameUnity::TARGET_LIB)) return false;

    // 派生类判断 + 对象创建 API（本地 dlsym，不改共享 Il2CppApi）
    void* h = dlopen(sec_lib_il2cpp(), RTLD_LAZY | RTLD_NOLOAD);
    if (h) {
        g_isAssignable = reinterpret_cast<class_is_assignable_t>(
            dlsym(h, "il2cpp_class_is_assignable_from"));
        g_objectNew = reinterpret_cast<object_new_t>(dlsym(h, "il2cpp_object_new"));
        // il2cpp 契约：外部线程调 il2cpp API/解释器前必须 attach（未 attach 的
        // 线程 field_static_get_value / 解释器 invoke 随机 SIGSEGV——tombstone 实锤）
        g_threadCurrent = reinterpret_cast<thread_current_t>(dlsym(h, "il2cpp_thread_current"));
        g_threadAttach  = reinterpret_cast<thread_attach_t>(dlsym(h, "il2cpp_thread_attach"));
        g_classGetFields = reinterpret_cast<class_get_fields_t>(dlsym(h, "il2cpp_class_get_fields"));
        g_fieldGetName   = reinterpret_cast<field_get_name_t>(dlsym(h, "il2cpp_field_get_name"));
        g_fieldGetValue  = reinterpret_cast<field_get_value_object_t>(dlsym(h, "il2cpp_field_get_value_object"));
    }

    g_heroClass     = api.FindClassByShortName(GameUnity::CLASS_HERO);
    g_levelProcess  = api.FindClassByShortName(GameUnity::CLASS_LEVEL_PROCESS);
    g_playerArchive = api.FindClassByShortName(GameUnity::CLASS_PLAYER_ARCHIVE);
    g_archiveClass  = api.FindClassByShortName(GameUnity::CLASS_ARCHIVE);
    g_haClass       = api.FindClassByShortName(GameUnity::CLASS_HERO_ARCHIVE);
    g_bodyClass     = api.FindClassByShortName(GameUnity::CLASS_BODY);
    g_charClass     = api.FindClassByShortName(GameUnity::CLASS_CHARACTER);
    g_evClass       = api.FindClassByShortName(GameUnity::CLASS_ENCRYPT_VALUE);
    g_dataManager   = api.FindClassByShortName("DataManager");
    g_skinEnumClass = api.FindClassByShortName("HeroSkinName");
    if (!g_heroClass || !g_levelProcess || !g_playerArchive || !g_archiveClass) {
        LOGI(ZHMR_LOG " 热更类未就绪（HybridCLR 未加载?）");
        return false;
    }
    if (!g_classesSettled.exchange(true)) {
        // 本轮类首次查齐：只缓存不碰 FieldOffset，下一轮（2s 后）再读偏移
        LOGI(ZHMR_LOG " classes 齐了（缓存），隔 2s 再读 offsets（3.6.0 abort 防御）");
        return false;
    }
    if (api.FindClassByShortName(GameUnity::CLASS_HERO) != g_heroClass) {
        // settle 轮指针漂移 = 热更仍在重新注册，复位重等
        g_classesSettled.store(false);
        LOGI(ZHMR_LOG " 类指针漂移（热更重注册中），继续等待");
        return false;
    }
    LOGI(ZHMR_LOG " classes: hero=%p lp=%p pa=%p arc=%p ha=%p body=%p char=%p ev=%p assignable=%d",
         g_heroClass, g_levelProcess, g_playerArchive, g_archiveClass, g_haClass,
         g_bodyClass, g_charClass, g_evClass, g_isAssignable ? 1 : 0);

    using namespace GameUnity::Fields;
    for (int s = 0; s < 2; s++) {
        size_t bias = s ? 8 : 0;
        g_combat[s].hero_body     = api.FieldOffset(g_heroClass, HERO_BODY, bias);
        g_combat[s].body_char     = g_bodyClass ? api.FieldOffset(g_bodyClass, BODY_CHARACTER, bias) : 0;
        g_combat[s].char_block    = g_charClass ? api.FieldOffset(g_charClass, CHAR_BLOCK, bias) : 0;
        g_combat[s].char_hp       = g_charClass ? api.FieldOffset(g_charClass, CHAR_HP, bias) : 0;
        g_combat[s].char_shield   = g_charClass ? api.FieldOffset(g_charClass, CHAR_SHIELD, bias) : 0;
        g_combat[s].lp_allmonster = api.FieldOffset(g_levelProcess, LP_ALL_MONSTER, bias);
        if (g_haClass) {
            g_arch[s].ha_name      = api.FieldOffset(g_haClass, HA_NAME, bias);
            g_arch[s].ha_unlock    = api.FieldOffset(g_haClass, HA_UNLOCK, bias);
            g_arch[s].ha_exp       = api.FieldOffset(g_haClass, HA_EXP, bias);
            g_arch[s].ha_skillDict = api.FieldOffset(g_haClass, HA_SKILL_DICT, bias);
            g_arch[s].ha_styleDict = api.FieldOffset(g_haClass, HA_STYLE_DICT, bias);
        }
        if (g_evClass) {
            g_ev[s].ev_show = api.FieldOffset(g_evClass, EV_SHOW, bias);
            g_ev[s].ev_m    = api.FieldOffset(g_evClass, EV_M, bias);
            g_ev[s].ev_key  = api.FieldOffset(g_evClass, EV_KEY, bias);
        }
    }
    LOGI(ZHMR_LOG " offsets raw: hero_body=%zu body_char=%zu block=%zu hp=%zu monsters=%zu "
         "unlock=%zu ev(show,m,key)=%zu,%zu,%zu",
         g_combat[0].hero_body, g_combat[0].body_char, g_combat[0].char_block,
         g_combat[0].char_hp, g_combat[0].lp_allmonster, g_arch[0].ha_unlock,
         g_ev[0].ev_show, g_ev[0].ev_m, g_ev[0].ev_key);
    return true;
}

// ============================================================
// ⑨ 对外接口（Main.cpp 契约）
// ============================================================
bool Zhmr_TryInit() {
    if (g_ready.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;

    // ① 首次：挂 il2cpp_init 返回门（引擎函数，load 检测后即刻装，赶在 UnityMain
    //    调 il2cpp_init 之前；装失败则退化为 10s 盲等兜底）
    if (!g_initHookTried.exchange(true)) {
        void* h = dlopen(sec_lib_il2cpp(), RTLD_LAZY | RTLD_NOLOAD);
        void* fn = h ? dlsym(h, "il2cpp_init") : nullptr;
        if (fn && DobbyHook(fn, (void*)Hooked_Il2cppInit, (void**)&g_origIl2cppInit) == 0) {
            LOGI(ZHMR_LOG " il2cpp_init 探测门已挂 @%p", fn);
        } else {
            LOGE(ZHMR_LOG " il2cpp_init 挂钩失败，改用 10s 盲等兜底");
            g_initHookFailed = true;
        }
    }

    // ② 等引擎 init 完成：此前本线程对 il2cpp 的任何触碰都会把线程带进
    //    GC「未知线程」毒态（relaunch 三连崩实锤）。兜底：挂钩失败盲等 5 轮
    //    （5×2s）；挂钩成功但 30 轮仍无回调（理论不应发生）也放行，免功能死锁。
    if (!g_il2cppInitDone.load()) {
        static std::atomic<int> s_waitN{0};
        int n = s_waitN.fetch_add(1) + 1;
        if (g_initHookFailed) {
            if (n >= 5) g_il2cppInitDone.store(true);
        } else if (n >= 30) {
            LOGE(ZHMR_LOG " 未收到 il2cpp_init 返回（疑似错过调用点），60s 后强制放行");
            g_il2cppInitDone.store(true);
        }
        if (!g_il2cppInitDone.load()) return false;
    }

    Il2Cpp::SetScanThread(pthread_self());
    bool ok = SafeProbe([&]() -> bool { return ResolveAll(); });
    if (!ok) return false;
    // 本线程 attach 到 il2cpp domain（一次性；外部线程未 attach 直接调 il2cpp
    // API/解释器会随机 SIGSEGV——tombstone 实锤）。Api 已缓存 domain_get。
    if (!g_threadAttached.load() && g_threadAttach) {
        void* t = g_threadCurrent ? g_threadCurrent() : nullptr;
        if (!t) t = g_threadAttach(GetApi().domain_get());
        g_threadAttached.store(t != nullptr);
        LOGI(ZHMR_LOG " il2cpp 线程 attach: thread=%p ok=%d", t, g_threadAttached.load() ? 1 : 0);
    }
    g_ready.store(true);
    LOGI(ZHMR_LOG " 初始化成功（0 hook 0 invoke，单例/dic 直写 + 安全读）");
    return true;
}

bool Zhmr_IsReady() { return g_ready.load(); }

void Zhmr_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case ZHMR_FEAT_GOD:
            if (g_god.load() && !enabled) g_heroReset.store(true);
            g_god.store(enabled);
            break;
        case ZHMR_FEAT_ONEHIT:   g_onehit.store(enabled); break;
        case ZHMR_FEAT_CURRENCY: g_currency.store(enabled); if (enabled) g_currencyPending.store(true); break;
        case ZHMR_FEAT_ITEMS:    g_items.store(enabled); if (enabled) g_itemsPending.store(true); break;
        case ZHMR_FEAT_UNLOCK:   g_unlock.store(enabled); if (enabled) g_skillsPending.store(true); break;
        case ZHMR_FEAT_SKIN:     g_skin.store(enabled); if (enabled) g_skinPending.store(true); break;
        default: break;
    }
}

void Zhmr_OnValueChanged(int /*featNum*/, int /*value*/) {}

void Zhmr_Tick(const bool* features) {
    if (!features) return;
    g_god.store(features[ZHMR_FEAT_GOD]);
    g_onehit.store(features[ZHMR_FEAT_ONEHIT]);
    g_currency.store(features[ZHMR_FEAT_CURRENCY]);
    g_items.store(features[ZHMR_FEAT_ITEMS]);
    g_unlock.store(features[ZHMR_FEAT_UNLOCK]);
    g_skin.store(features[ZHMR_FEAT_SKIN]);
    if (!g_ready.load()) return;

    static int tick = 0;
    tick++;

    // EncryptValue 不变式自校验（读 _exSoulStoneCount 现值即可验证 ev 偏移组）
    if (g_evScheme.load() < 0) {
        auto& api = GetApi();
        void* ev = api.StaticRefField(g_playerArchive, "_exSoulStoneCount");
        if (OkPtr(ev)) {
            for (int s = 0; s < 2; s++) {
                int32_t show = 0, m = 0, key = 0;
                if (!SafeReadMem(reinterpret_cast<uint8_t*>(ev) + g_ev[s].ev_show, &show, 4))
                    continue;
                if (!SafeReadMem(reinterpret_cast<uint8_t*>(ev) + g_ev[s].ev_m, &m, 4)) continue;
                if (!SafeReadMem(reinterpret_cast<uint8_t*>(ev) + g_ev[s].ev_key, &key, 4))
                    continue;
                if (show == (m ^ key)) {
                    g_evScheme.store(s);
                    LOGI(ZHMR_LOG " EV bias 方案实证: %s show=%d key=%d", s ? "raw+8" : "raw",
                         show, key);
                    break;
                }
            }
        }
    }

    DecideCombatSchemeIfNeeded();
    if (g_god.load() && tick % 5 == 0) ApplyGod(true);
    if (g_heroReset.load()) {
        g_heroReset.store(false);
        ApplyGod(false);
    }
    if (g_onehit.load() && tick % 5 == 0) ApplyOneHit();

    if (g_currency.load() && (g_currencyPending.load() || tick % 150 == 0)) {
        g_currencyPending.store(false);
        ApplyCurrency();
    }
    if (g_items.load() && (g_itemsPending.load() || tick % 300 == 0)) {
        g_itemsPending.store(false);
        ApplyItems();
    }
    if (g_unlock.load() && tick % 25 == 0) {
        ApplyUnlockHero();
        // 持续重放：游戏打开选人/存档时可能从 CryptoPrefs 重载英雄字典，冲掉插入
        ApplyHeroSkills();
    }
    if (g_skin.load() && (g_skinPending.load() || tick % 300 == 0)) {
        g_skinPending.store(false);
        ApplySkin();
    }
}

void Zhmr_ForceClear() {
    g_god.store(false);
    g_onehit.store(false);
    g_currency.store(false);
    g_items.store(false);
    g_unlock.store(false);
    g_skin.store(false);
    g_currencyPending.store(false);
    g_itemsPending.store(false);
    g_skinPending.store(false);
    g_heroReset.store(true);
    LOGI(ZHMR_LOG " ForceClear：全部开关复位");
}
