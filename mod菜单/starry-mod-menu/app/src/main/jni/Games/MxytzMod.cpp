/* ================================================================
 *  冒险与征途 (com.mxyzt.gzmy) 1.5.9 — 星光菜单游戏模块
 *
 *  移植依据（结论；证据见 10_docs/冒险与征途/斌哥包抓点_20261002.md）：
 *    目标包 = Unity 2022.3.14f1 + IL2CPP + HybridCLR 热更（Assembly-CSharp 解释执行）
 *    游戏逻辑在热更程序集里，而且**这些方法一直保持解释执行**：`methodPointer` 恒等于解释器
 *    共享桩 `libil2cpp基址+0x207F04C`，永不物化成真本地码 ⇒ **「只 hook 具体方法」在本游戏
 *    物理上不成立**（v26x3 实测 `PREFLIGHT Hurt5 fn=0x40002527f04c code=0 static=1`）。
 *
 *  ★★v27 路线（= `10_docs/冒险与征途/斌哥包抓点_20261002.md` 的复刻要点）：
 *    **hook HybridCLR 解释器入口（RVA 0x207F798）+ 按「类::方法」名分发**。
 *    ⚠️ 这与 `mod菜单/starry-mod-menu/docs/GAME_MOD_GUIDE.md` §1「只 hook 具体方法，绝不 hook
 *    共享入口」**正面冲突**，是**有意为之**，三条理由（详见 `§⑩a` 上方注释，勿删）：
 *      ① 本游戏的具体方法 hook 物理不可能（见上），指南那条规则的前提在本包不成立；
 *      ② 指南点名该反例的危害是「每次调用都进 hook + 无法精确过滤 → 掉帧」。v27 已把入口
 *         热路径压到「1 次读 + 1~2 次指针比较」：**名字学习只发生一次**，之后纯指针比较，
 *         热路径上没有任何字符串比较 —— 正好消掉指南点名的那条反例；
 *      ③ 斌哥包（唯一成功参照）走的就是这条路，且他的名字在运行期才解密到匿名段。
 *    若判定该策略不可接受，回退办法：不挂 `InstallInterpHook`（`SyncHooks` 里那一段），
 *    代价是 #1 秒杀 / #3 技能伤害 / #4 免广告 全部回到「无实现」（只剩 #2 无敌的内存写路径）。
 *
 *  四项功能（下标与 MxytzEntry.cpp 的 featureList 一一对应）：
 *    #1 秒杀小怪           行为型：结算路径放大「打给非玩家目标」的伤害
 *    #2 无敌               行为型：目标是自己时作废本次受击（不调 orig）
 *    #3 全模式技能伤害增加 行为型：全模式伤害放大（技能/普攻同一结算路径）
 *    #4 免广告             行为型：广告观看/领奖入口放行 + 强制成功结果
 *
 *  防闪退设施（勿删任何一节）：
 *    默认 0 hook（懒装）/ 冷启宽限 / 幂等安装（防双 Dobby 同址 SIGSEGV）
 *    orig 必调（除无敌作废）/ reenter 防递归 / 只为已解析的槽装钩
 *    类名缓存（热路径禁 object_class+class_name）/ Tick 只同步开关
 *    MI 槽（*MethodInfo）**禁止写** / 懒桩漂移观察表（v6）
 *
 *  ⚠️ v6（2026-10-02 实测）：MI 槽写入必须永久禁用。证据：`MI-patch DoAttack7` 后 0.4s
 *    logcat `I/scudo(24364): Scudo ERROR: corrupted chunk header at address 0x7d435b429420`，
 *    随即 SIGABRT（栈 #01..#03 全在 libstarry_magic.so 的 free 路径）→ 游戏闪退。
 *    本版起改「方法指针观察表」：只读 methodPointer，看懒桩是否漂移成可执行真代码。
 *
 *  运行期取证：候选类字段/方法表写
 *    /data/data/com.mxyzt.gzmy/files/mxyzt_dump.txt（logcat 会被轮转冲掉）
 * ================================================================ */

#include "MxytzMod.h"

#include <atomic>
#include <cerrno>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <ctime>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/StarPtr.h"   // v28：设备无关指针判据（清 TBI/MTE 顶字节）
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define TPL_LOG "[mxyzt]"
#define MXYTZ_TARGET_LIB sec_lib_il2cpp()
#define MXYTZ_DUMP_FILE "/data/data/com.mxyzt.gzmy/files/mxyzt_dump.txt"

/* ★v44 交付版日志闸：**诊断日志一律走 DBGLOG**（默认编译期关闭，参数连求值都不会发生）。
 *  交付包只保留四类行：开机/挂载（`lib ready` / `* armed` / `* installed`）、菜单开关（`feat`）、
 *  免广告链路（`↑v43 预登记` / `AD43`）、以及每 30s 一条 `STATS` 汇总。
 *  排障时把下面的 0 改成 1 重编即可把全部诊断行放回来（代码没删，只是不编译进去）。 */
#define MXYTZ_VERBOSE 0
#if MXYTZ_VERBOSE
#define DBGLOG(...) LOGI(__VA_ARGS__)
#else
#define DBGLOG(...) ((void)0)
#endif

#ifndef STARRY_MXYTZ_PROBE
#define STARRY_MXYTZ_PROBE 0
#endif
#ifndef STARRY_MXYTZ_TRACE
#define STARRY_MXYTZ_TRACE 0
#endif

// ============================================================
// ① 状态 / 倍率 / 时间闸
// ============================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_hooked{false};
static std::atomic<bool> g_killMob{false};     // #1 秒杀小怪
static std::atomic<bool> g_invincible{false};  // #2 无敌
static std::atomic<bool> g_skillDmg{false};    // #3 全模式技能伤害增加
static std::atomic<bool> g_noAd{false};        // #4 免广告

static std::atomic<bool> g_dmgReenter{false};
static std::atomic<uint32_t> g_hitDmg{0};
static std::atomic<uint32_t> g_hitInv{0};
static std::atomic<uint32_t> g_hitSkill{0};
static std::atomic<uint32_t> g_hitAd{0};
static std::atomic<uint32_t> g_gateFail{0};
static std::atomic<uint32_t> g_dmgSeen{0};
static std::atomic<uint32_t> g_adSeen{0};

static std::atomic<uint64_t> g_processStartMs{0};
static std::atomic<uint64_t> g_featOnMs{0};
static std::atomic<uint64_t> g_installMs{0};

static void* g_hookAt[32] = {};        // 已装钩目标地址（幂等）
static uint8_t g_rebindUsed[32] = {};  // 懒桩漂移只重挂一次

/* 量级安全线（`skill/re-implement/anti-ban-analysis.md` §三）：伤害倍数 ×5~×100 是安全区，
 * ×999999+ 属高危；秒杀建议 ×100 CAP。
 * ★v27：1000 → 100。同时删掉 `ApplyHurtPolicy` 里「秒杀时把指数硬抬到 ≥12」那一行 ——
 *   它把这一击抬到 **1e12 量级**，直接踩进高危区；而 `m_llValue` 会被
 *   `CHitNotifier::AddOneHitHurt` 第 3 参写进命中通知包（飘字/落地血/死亡判定三者同源），
 *   也就是**这个数字会到服务端**。要「更狠」先看服务端回包/结算，不要直接加倍数。 */
static constexpr int64_t kKillMul = 100;   // 秒杀小怪（对非玩家目标）
static constexpr int64_t kSkillMul = 50;   // 全模式技能伤害增加
static constexpr int64_t kValCap = 2000000000LL;

/* ★v41：碰撞体级 `localDamage` 写入**停用**（用户 m02899 血证「我开了 3，为什么敌人打我，我直接死了…」）。
 *  原因：`localDamage = 1` 是**攻击方**的开关 —— `StarsCollider::DoAttack` 里 `this` 就是攻击方的碰撞体，
 *  敌人的 DoAttack 一样命中我们的写入 ⇒ 敌人对我们走的也是客户端权威伤害（还跳过 miss/dodge）。
 *  斌哥要的 #3 是「关卡模式里让**我们的**伤害客户端权威」，不是「全场景都客户端权威」。 */
static constexpr bool kColliderLocalDamageV41 = false;

static constexpr uint64_t kDeferAfterFeatMs = 1000ull;     // 开功能后 1s 装 Dobby
static constexpr uint64_t kDeferAfterProcessMs = 12000ull; // 冷启 12s（恢复窗口）

static uint64_t NowMs() {
    timespec ts{};
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return static_cast<uint64_t>(ts.tv_sec) * 1000ull + static_cast<uint64_t>(ts.tv_nsec) / 1000000ull;
}

static bool CanInstallHooks() {
    uint64_t ps = g_processStartMs.load(std::memory_order_relaxed);
    uint64_t fo = g_featOnMs.load(std::memory_order_relaxed);
    if (!ps || !fo) return false;
    uint64_t gate = ps + kDeferAfterProcessMs;
    if (fo + kDeferAfterFeatMs > gate) gate = fo + kDeferAfterFeatMs;
    return NowMs() >= gate;
}

static void SyncFeatOnGate(bool anyOn) {
    static bool s_prev = false;
    if (anyOn && !s_prev) g_featOnMs.store(NowMs(), std::memory_order_relaxed);
    s_prev = anyOn;
}

// ============================================================
// ② 安全助手
// ============================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

/** ★v27 廉价「像样指针」判据：**不做任何 maps 查询**。
 *  为什么需要它：`Il2Cpp::IsValidHeapPtr` 内部走 `PermCached`，对 8192 条 maps 快照做线性扫
 *  （`mod菜单/starry-mod-menu/app/src/main/jni/Unity/Il2CppApi.cpp:727`）——在
 *  「每次热更方法调用都过一遍」的解释器入口路径、以及「每次受击都跑」的伤害策略里调用它是致命的。
 *  ★v28 设备无关修正：原先这里直接对**原始指针**判 `0x100000000 ~ 0x800000000000`，
 *  没清顶字节。真机（arm64 + Android 12+）的 Scudo 堆在 `0xb4…` 段，指针形如
 *  `0xb4000076c1646d58` ⇒ `>= 0x800000000000` ⇒ 恒 false ⇒ 全部 45 处调用点早退 ⇒
 *  四项功能在真机上静默全灭（模拟器 x86_64+Houdini 地址不越界，所以那边正常）。
 *  现统一走 `Includes/StarPtr.h`：先清 TBI/MTE 顶字节，再判 4GiB ~ 128TiB 用户态区间。 */
static inline bool LivePtr(const void* p) { return starptr::LiveFastCounted(p); }

static int32_t SafeReadI32(void* obj, size_t off, int32_t defv) {
    if (!obj) return defv;
    int32_t v = defv;
    memcpy(&v, reinterpret_cast<uint8_t*>(obj) + off, sizeof(v));
    return v;
}

static float SafeReadF32(void* obj, size_t off, float defv) {
    if (!obj) return defv;
    float v = defv;
    memcpy(&v, reinterpret_cast<uint8_t*>(obj) + off, sizeof(v));
    return v;
}

static void SafeWriteF32(void* obj, size_t off, float v) {
    if (!obj) return;
    memcpy(reinterpret_cast<uint8_t*>(obj) + off, &v, sizeof(v));
}

struct ClassNameCache {
    void* klass = nullptr;
    char  name[64] = {};
    bool  valid = false;
};

static const char* CacheClassName(ClassNameCache& cache, void* obj) {
    if (!OkPtr(obj)) return nullptr;
    auto& api = GetApi();
    if (!api.object_class || !api.class_name) return nullptr;
    void* k = api.object_class(obj);
    if (!k) return nullptr;
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

static bool NameHas(const char* got, const char* needle) {
    if (!got || !needle || !*needle) return false;
    for (size_t i = 0; got[i]; i++) {
        size_t j = 0;
        while (needle[j] && got[i + j] == needle[j]) j++;
        if (!needle[j]) return true;
    }
    return false;
}

static bool NameHasAny(const char* got, const char* const* list, size_t n) {
    for (size_t i = 0; i < n; i++)
        if (NameHas(got, list[i])) return true;
    return false;
}

static bool TypeEndsWith(void* fi, const char* suffix) {
    auto& api = GetApi();
    if (!fi || !api.field_type || !api.type_name) return false;
    void* t = api.field_type(fi);
    if (!t) return false;
    char* full = api.type_name(t);
    if (!full) return false;
    size_t fl = strlen(full), sl = strlen(suffix);
    bool ok = fl >= sl && strcmp(full + (fl - sl), suffix) == 0;
    /* 注意：热更类型上 type_name 返回的是 HybridCLR 管理的字符串，free() 会 SIGABRT（实测）。 */
    return ok;
}

// ============================================================
// ③ il2cpp 解析（按名，热更程序集优先）
// ============================================================
static bool EnsureApi() {
    if (GetApi().ready) return true;
    return GetApi().Load(MXYTZ_TARGET_LIB);
}

static void* MethodPointer(void* mi) { return mi ? *reinterpret_cast<void**>(mi) : nullptr; }

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

static void* FindClassAnywhere(const char* name) {
    if (!name) return nullptr;
    // v26f：结果记忆化。`FindClassByShortName` 每次全量遍历所有 image 的全部 class（13755 个），
    //   实测单次 ~1 s；诊断路径一轮要问十几个类 ⇒ 十几秒卡顿。命中过的类名以后直接返回。
    static char s_ckName[64][40];
    static void* s_ckKlass[64];
    static std::atomic<uint32_t> s_ckN{0};
    const uint32_t cn = s_ckN.load(std::memory_order_relaxed);
    for (uint32_t i = 0; i < cn && i < 64; i++)
        if (strcmp(s_ckName[i], name) == 0 && s_ckKlass[i]) return s_ckKlass[i];
    auto& api = GetApi();
    void* k = api.FindClassByShortName(name);
    if (!k) {
        static const char* kNs[] = {"ActionGameLibrary", "ActionData", "GameConfigData",
                                    "",                  "HotFix",     "Game"};
        for (size_t i = 0; i < sizeof(kNs) / sizeof(kNs[0]) && !k; i++) k = FindClassNs(kNs[i], name);
    }
    if (k && cn < 64) {
        snprintf(s_ckName[cn], sizeof(s_ckName[0]), "%s", name);
        s_ckKlass[cn] = k;
        s_ckN.store(cn + 1, std::memory_order_relaxed);
    }
    return k;
}

/** 同名重载第 nth 个（HybridCLR 下 argc 与源码一致，先按 argc 取再退 nth）。
 *  ⚠️ v6 实测：HybridCLR 热更类的 `class_methods` 迭代器**不会终止**（Character 上连续返回
 *  6.9 亿个 null-name 项 → 死循环）。故此处必有迭代上限，绝不裸 while(true)。 */
static void* FindMethodNth(void* klass, const char* name, int nth) {
    auto& api = GetApi();
    if (!klass || !api.class_methods || !api.method_get_name) return nullptr;
    void* iter = nullptr;
    int seen = 0;
    int guard = 0;
    while (guard++ < 4096) {
        void* mi = api.class_methods(klass, &iter);
        if (!mi) break;
        const char* mn = api.method_get_name(mi);
        if (!mn || strcmp(mn, name) != 0) continue;
        if (seen == nth) return mi;
        seen++;
    }
    return nullptr;
}

/** 方法指针观察表（v6 诊断）：不写任何游戏内存，只每秒读一次 methodPointer，
 *  看 HybridCLR 懒桩会不会漂移成「可读+可执行」的真代码 —— 那才是能 Dobby 的合法目标。 */
struct SlotWatch {
    const char* tag;
    void* mi;
    void* last;
    uint32_t seen;
    uint32_t exec;
};
static SlotWatch g_watch[32];
static size_t g_watchN = 0;
static std::atomic<uint64_t> g_watchMs{0};

static void RegisterWatch(const char* tag, void* mi) {
    if (!tag || !mi || g_watchN >= 32) return;
    for (size_t i = 0; i < g_watchN; i++)
        if (g_watch[i].tag == tag) return;
    if (!Il2Cpp::IsReadableRange((uintptr_t)mi, 8)) {
        DBGLOG(TPL_LOG " WATCH- %s mi=%p 不可读，跳过", tag, mi);
        return;
    }
    SlotWatch& w = g_watch[g_watchN++];
    w.tag = tag;
    w.mi = mi;
    w.last = MethodPointer(mi);
    w.seen = 0;
    w.exec = 0;
    DBGLOG(TPL_LOG " WATCH+ %s mi=%p ptr=%p", tag, mi, w.last);
}


// ============================================================
// ④ 角色判别（玩家/非玩家）：优先类名，其次 camp 字段；都不确定就不改
// ============================================================
static ClassNameCache g_clsCache;
static void* g_playerKlass = nullptr;
static size_t g_offCamp = 0;
static void* g_isPlayerMP = nullptr;  // GameObjectManager::IsPlayer(object, bool) 的 native 桥
static void* g_roleKlass = nullptr;   // ulRoleId 偏移按目标类缓存
static size_t g_offRoleId = 0;

/** 用游戏自身的判定（只认静态方法；未解析/非静态 = 不参与，绝不瞎猜 ABI）。 */
static bool GameIsPlayer(void* obj) {
    if (!g_isPlayerMP || !OkPtr(obj)) return false;
    typedef void* (*IsPlayer_t)(void*, void*, void*, void*, void*);
    IsPlayer_t fn = reinterpret_cast<IsPlayer_t>(g_isPlayerMP);
    return reinterpret_cast<intptr_t>(fn(obj, nullptr, nullptr, nullptr, nullptr)) != 0;
}

static const char* const kPlayerWords[] = {"Player", "Hero", "MainChar", "SelfChar",
                                           "LocalPlayer", "MyChar"};
static const char* const kEnemyWords[] = {"Monster", "Enemy", "Mob", "Npc", "Creep", "Boss"};

static bool IsPlayerObj(void* obj) {
    if (!OkPtr(obj)) return false;
    auto& api = GetApi();
    void* k = api.object_class ? api.object_class(obj) : nullptr;
    if (g_playerKlass && k && k == g_playerKlass) return true;
    const char* cn = CacheClassName(g_clsCache, obj);
    if (NameHasAny(cn, kPlayerWords, sizeof(kPlayerWords) / sizeof(kPlayerWords[0]))) return true;
    if (NameHasAny(cn, kEnemyWords, sizeof(kEnemyWords) / sizeof(kEnemyWords[0]))) return false;
    return GameIsPlayer(obj);  // 只做「肯定判定」；不确定一律 false（宁可功能不生效，也不误判）
}

static bool IsEnemyObj(void* obj) {
    if (!OkPtr(obj)) return false;
    if (IsPlayerObj(obj)) return false;
    auto& api = GetApi();
    void* k = api.object_class ? api.object_class(obj) : nullptr;
    if (k) {
        if (k != g_roleKlass) {  // ulRoleId 偏移按类缓存（子类偏移可能与基类不同）
            g_offRoleId = api.FieldOffset(k, "ulRoleId", 0);
            g_roleKlass = k;
            DBGLOG(TPL_LOG " ulRoleId @+0x%zx cls=%s", g_offRoleId,
                 api.class_name ? api.class_name(k) : "?");
        }
        if (g_offRoleId && k == g_roleKlass && Il2Cpp::ReadI32(obj, g_offRoleId) == 0)
            return true;  // IL 证据 §2：ulRoleId==0 → 小怪/NPC
    }
    const char* cn = CacheClassName(g_clsCache, obj);
    if (NameHasAny(cn, kEnemyWords, sizeof(kEnemyWords) / sizeof(kEnemyWords[0]))) return true;
    if (g_offCamp) return true;  // 有阵营字段且已排除玩家 → 视为非玩家目标
    return false;
}

// ============================================================
// ⑤ 伤害形态解析（P0 是数值还是结构体，运行时判定，禁写死偏移）
// ============================================================
enum DmgP0Kind { P0_UNKNOWN = 0, P0_I64, P0_I32, P0_F32, P0_STRUCT };

static std::atomic<int> g_p0Kind{P0_UNKNOWN};
static size_t g_offMagF = 0;  // 结构体里的浮点伤害
static size_t g_offMagI = 0;  // 结构体里的整型伤害
static std::atomic<bool> g_structReady{false};

// ---------- v7 字段名探针（禁用迭代器：class_fields/class_methods 在热更类上不可靠） ----------
struct FieldHit {
    const char* cls;
    const char* name;
    size_t off;
};
static FieldHit g_fhits[256];
static size_t g_fhitN = 0;

static void NoteField(const char* cls, const char* name, size_t off) {
    for (size_t i = 0; i < g_fhitN; i++) {
        if (g_fhits[i].cls == cls && g_fhits[i].name == name) {
            if (g_fhits[i].off != off) {
                DBGLOG(TPL_LOG " FIELD! %s.%s +0x%zx -> +0x%zx", cls, name, g_fhits[i].off, off);
                g_fhits[i].off = off;
            }
            return;
        }
    }
    if (g_fhitN >= 256) return;
    g_fhits[g_fhitN].cls = cls;
    g_fhits[g_fhitN].name = name;
    g_fhits[g_fhitN].off = off;
    g_fhitN++;
    DBGLOG(TPL_LOG " FIELD %s.%s +0x%zx", cls, name, off);
}


/** 结构体形态：挑伤害字段（名字优先，其次第一个 float / int32 字段）。 */
static void ResolveStructLayout(void* klass) {
    auto& api = GetApi();
    if (!klass || !api.class_fields || !api.field_name) return;
    static const char* kPref[] = {"m", "value", "Value", "damage", "Damage", "hurt", "Hurt",
                                  "hp", "HP", "amount", "Amount", "num", "Num"};
    size_t firstF = 0, firstI = 0;
    void* iter = nullptr;
    int n = 0;
    while (n < 512) {
        void* fi = api.class_fields(klass, &iter);
        if (!fi) break;
        const char* fn = api.field_name(fi);
        size_t off = api.field_offset ? api.field_offset(fi) : 0;
        if (off) {
            if (TypeEndsWith(fi, "Single")) {
                if (!firstF) firstF = off;
                if (!g_offMagF && NameHasAny(fn, kPref, sizeof(kPref) / sizeof(kPref[0])))
                    g_offMagF = off;
            } else if (TypeEndsWith(fi, "Int32")) {
                if (!firstI) firstI = off;
                if (!g_offMagI && NameHasAny(fn, kPref, sizeof(kPref) / sizeof(kPref[0])))
                    g_offMagI = off;
            }
        }
        n++;
    }
    if (!g_offMagF && firstF) g_offMagF = firstF;
    if (!g_offMagI && firstI && !g_offMagF) g_offMagI = firstI;
    g_structReady.store(g_offMagF != 0 || g_offMagI != 0);
    DBGLOG(TPL_LOG " struct fields=%d magF=+0x%zx magI=+0x%zx", n, g_offMagF, g_offMagI);
}

/** v12 停用：诊断体（type_name 后 free 会 SIGABRT；ResolveStructLayout 迭代热更类字段也不安全）。
 *  功能实现不再依赖 p0 形态判定（改走解释器入口的参数帧）。 */
static void ResolveP0FromMethod(void* /*mi*/, const char* /*tag*/) {}

// ============================================================
// ⑥ 伤害策略（#1/#2/#3 共用；返回 true = 作废本次结算）
// ============================================================
static bool ApplyDamagePolicy(void* self, void*& a0) {
    const bool kill = g_killMob.load(std::memory_order_relaxed);
    const bool inv = g_invincible.load(std::memory_order_relaxed);
    const bool skill = g_skillDmg.load(std::memory_order_relaxed);
    if (!kill && !inv && !skill) return false;  // 开关全关 → 零改写零日志
    if (g_dmgReenter.exchange(true)) return false;

    const bool player = IsPlayerObj(self);
    if (player && inv) {
        g_hitInv.fetch_add(1, std::memory_order_relaxed);
        g_dmgReenter.store(false);
        DBGLOG(TPL_LOG " INV cancel hit self=%p", self);
        return true;  // 受击作废
    }

    int64_t mul = 0;
    if (!player && kill && IsEnemyObj(self)) mul = kKillMul;
    if (skill && kSkillMul > mul) mul = kSkillMul;  // 技能伤害增加对所有模式生效

    if (!mul) {
        g_dmgReenter.store(false);
        return false;
    }

#if STARRY_MXYTZ_PROBE || STARRY_MXYTZ_TRACE
    {
        const char* cn = CacheClassName(g_clsCache, self);
        uint32_t n = g_dmgSeen.fetch_add(1, std::memory_order_relaxed);
        if (n < 40 || (n % 60) == 0)
            DBGLOG(TPL_LOG " DMG self=%p cls=%s player=%d mul=%lld p0=%p kind=%d", self,
                 cn ? cn : "?", player ? 1 : 0, (long long)mul, a0, g_p0Kind.load());
    }
#endif

    const int kind = g_p0Kind.load(std::memory_order_relaxed);
    if (kind == P0_I64) {
        int64_t v = static_cast<int64_t>(reinterpret_cast<intptr_t>(a0));
        if (v > 0 && v < kValCap) {
            int64_t w = v * mul;
            if (w > kValCap) w = kValCap;
            a0 = reinterpret_cast<void*>(static_cast<intptr_t>(w));
            g_hitDmg.fetch_add(1, std::memory_order_relaxed);
            if (skill) g_hitSkill.fetch_add(1, std::memory_order_relaxed);
        }
    } else if (kind == P0_I32) {
        int32_t v = static_cast<int32_t>(reinterpret_cast<intptr_t>(a0));
        if (v > 0 && v < 1000000) {
            int64_t w = static_cast<int64_t>(v) * mul;
            if (w > kValCap) w = kValCap;
            a0 = reinterpret_cast<void*>(static_cast<intptr_t>(w));
            g_hitDmg.fetch_add(1, std::memory_order_relaxed);
            if (skill) g_hitSkill.fetch_add(1, std::memory_order_relaxed);
        }
    } else if (kind == P0_STRUCT) {
        // 值类型可能整块走寄存器（float 在向量寄存器）→ 只在拿到堆对象指针时才写
        if (OkPtr(a0) && g_structReady.load()) {
            if (g_offMagF) {
                float f = SafeReadF32(a0, g_offMagF, 0.0f);
                if (f > 0.0f && f < 1.0e9f) {
                    float nf = f * static_cast<float>(mul);
                    if (nf > 1.0e9f) nf = 1.0e9f;
                    SafeWriteF32(a0, g_offMagF, nf);
                    g_hitDmg.fetch_add(1, std::memory_order_relaxed);
                    if (skill) g_hitSkill.fetch_add(1, std::memory_order_relaxed);
                }
            } else if (g_offMagI) {
                int32_t v = SafeReadI32(a0, g_offMagI, 0);
                if (v > 0 && v < 1000000) {
                    int64_t w = static_cast<int64_t>(v) * mul;
                    if (w > kValCap) w = kValCap;
                    Il2Cpp::WriteI32(a0, g_offMagI, static_cast<int32_t>(w));
                    g_hitDmg.fetch_add(1, std::memory_order_relaxed);
                    if (skill) g_hitSkill.fetch_add(1, std::memory_order_relaxed);
                }
            }
        }
    } else {
        static std::atomic<uint32_t> s_unk{0};
        uint32_t n = s_unk.fetch_add(1, std::memory_order_relaxed);
        if (n < 5) DBGLOG(TPL_LOG " DMG p0 kind unknown — 仅记录不改");
    }

    g_dmgReenter.store(false);
    return false;
}

// ============================================================
// ⑥b 抓点主路径（IL 证据：10_docs/冒险与征途/_钩子目标与语义.md §2/§3）
//   #1 秒杀小怪 / #3 全模式技能伤害增加 → ActionGameLibrary.CBattleActorUtil::Hurt
//        (ref CtagHurtHPPara, ref bool, bool, ref BigInt, bool) argc=5
//        伤害值 = para->m_llValue（BigInt = double m_mantissa × 10^int m_exponent）
//        改此处是唯一「伤害数字 / 扣血 / 死亡判定三者一致」的点：ProcDmgHit 取的是
//        para->GetValue()（不是方法实参 arg.3），而 HurtHp 会把伤害钳到 hp-1、永不致死。
//   #2 无敌 → ActionGameLibrary.StarsCollider::DoAttack(...) argc=7：把 accepter（第 3 个
//        IL 实参）置 null 即官方「命中作废」分支（IL: arg.2==null ||
//        arg.2.interactiveGameObject==null → ret）；另挂 Character::Damage 兜底（服务器
//        同步效果包路径）。
//   #4 免广告 → MiaoYueSDKProxy::PlayADVideo(int,string) argc=2：不弹广告，直接回调
//        DS_Advertising::OnADVideoResult(true, adid, uuid) 让 finish_watch_ad_req 正常收尾
//        （start 包不可跳过：uuid 由 start_watch_ad_rsp 下发）。
// ============================================================
static const size_t kNoOff = static_cast<size_t>(-1);

static size_t g_offParaTarget = 0;   // CtagHurtHPPara.m_pTarget (object*)
static size_t g_offParaAttacker = 0; // CtagHurtHPPara.m_pAttacker (object*) —— v26x：判「这一击是谁打的」
static size_t g_offParaValue = 0;    // CtagHurtHPPara.m_llValue (BigInt)
static size_t g_offBiMant = 0;       // BigInt.m_mantissa (double)
static size_t g_offBiExp = kNoOff;   // BigInt.m_exponent (int32)
static std::atomic<bool> g_paraReady{false};

// bridge 首参是否为 this（静态方法无 self）；装钩时按 tag 回写，避免写死实参下标
static std::atomic<bool> g_hurtStatic{true};
static std::atomic<bool> g_doAttackStatic{false};
static std::atomic<bool> g_playAdStatic{false};
static std::atomic<bool> g_onResultStatic{false};  // DS_Advertising::OnADVideoResult 是否静态

static size_t g_offAcceptIgo = 0;  // IAccept.interactiveGameObject
static void* g_acceptKlass = nullptr;

static bool ResolveParaLayout() {
    auto& api = GetApi();
    DBGLOG(TPL_LOG " pl1 find CtagHurtHPPara");
    void* kp = FindClassAnywhere("CtagHurtHPPara");
    DBGLOG(TPL_LOG " pl2 para=%p find BigInt", kp);
    void* kb = FindClassAnywhere("BigInt");
    DBGLOG(TPL_LOG " pl3 bigint=%p", kb);
    if (kp) {
        g_offParaTarget = api.FieldOffset(kp, "m_pTarget", 0);
        g_offParaAttacker = api.FieldOffset(kp, "m_pAttacker", 0);
        g_offParaValue = api.FieldOffset(kp, "m_llValue", 0);
    }
    if (kb) {
        g_offBiMant = api.FieldOffset(kb, "m_mantissa", 0);
        size_t e = api.FieldOffset(kb, "m_exponent", 0);
        g_offBiExp = e ? e : (g_offBiMant ? g_offBiMant + 8 : kNoOff);
    }
    if (!g_offBiMant && g_offBiExp == kNoOff) {  // IL 布局兜底：double 在前，int 在后
        g_offBiMant = 0;
        g_offBiExp = 8;
    }
    g_paraReady.store(g_offParaTarget != 0 && g_offParaValue != 0);
    DBGLOG(TPL_LOG " para layout target=+0x%zx attacker=+0x%zx value=+0x%zx biMant=+0x%zx biExp=+0x%zx ready=%d",
         g_offParaTarget, g_offParaAttacker, g_offParaValue, g_offBiMant, g_offBiExp,
         g_paraReady.load() ? 1 : 0);
    return g_paraReady.load();
}

/** BigInt 解包（值 = mantissa × 10^exponent）；布局可疑时返回 false，宁可不改。 */
static bool BigIntGet(void* bi, double& mant, int32_t& exp) {
    // v27：`bi` 是 `para + offV` 这种**内部指针**，只用 LivePtr；OkPtr 的 8192 条 maps 线性扫
    //   在「每次受击」的热路径上太贵（原始代码就是这么写的）。
    if (!LivePtr(bi) || g_offBiExp == kNoOff) return false;
    const char* p = reinterpret_cast<const char*>(bi);
    memcpy(&mant, p + g_offBiMant, sizeof(double));
    memcpy(&exp, p + g_offBiExp, sizeof(int32_t));
    if (mant != mant) return false;  // NaN
    if (mant < 0.0 || mant > 1.0e15) return false;
    if (exp < -64 || exp > 64) exp = 0;
    return true;
}

static void BigIntSet(void* bi, double mant, int32_t exp) {
    if (!LivePtr(bi) || g_offBiExp == kNoOff) return;  // v27：同上，禁用 maps 查询
    char* p = reinterpret_cast<char*>(bi);
    memcpy(p + g_offBiMant, &mant, sizeof(double));
    memcpy(p + g_offBiExp, &exp, sizeof(int32_t));
}

struct StaticFlagEntry {
    const char* tag;
    std::atomic<bool>* slot;
};
static StaticFlagEntry kStaticFlags[] = {
    {"Hurt5", &g_hurtStatic},
    {"DoAttack7", &g_doAttackStatic},
    {"PlayAD2", &g_playAdStatic},
    {"OnResult7", &g_onResultStatic},
};

static void NoteStatic(const char* tag, void* mi) {
    auto& api = GetApi();
    if (!api.method_flags || !mi) return;
    const bool st = (api.method_flags(mi, nullptr) & 0x10u) != 0;
    for (size_t i = 0; i < sizeof(kStaticFlags) / sizeof(kStaticFlags[0]); i++) {
        if (strcmp(kStaticFlags[i].tag, tag) == 0) {
            kStaticFlags[i].slot->store(st);
            DBGLOG(TPL_LOG " %s static=%d", tag, st ? 1 : 0);
            return;
        }
    }
}

// 宽签名桩（AArch64 多余实参无害；P0=long 走 x 寄存器，与 void* 同宽）
typedef void (*DmgHook_t)(void*, void*, void*, void*, void*, void*, void*, void*, void*);
typedef void (*AdVoid_t)(void*, void*, void*, void*, void*, void*, void*, void*);
typedef void* (*AdBoolRet_t)(void*, void*, void*, void*, void*, void*, void*, void*);

static void* g_origDmg3 = nullptr;
static void* g_origDmg2 = nullptr;
static void* g_origTake = nullptr;
static void* g_origDmgRebind = nullptr;
static void* g_origAdVoid = nullptr;
static void* g_origAdVoid2 = nullptr;
static void* g_origAdVoid3 = nullptr;
static void* g_origAdBoolRet = nullptr;
static void* g_origAdArg0 = nullptr;
static void* g_origAdArg0b = nullptr;
static void* g_origHurt = nullptr;      // CBattleActorUtil::Hurt argc=5
static void* g_origDoAttack = nullptr;  // StarsCollider::DoAttack argc=7
static void* g_origLocalDmg = nullptr;  // BattleElementCenter::get_localDamage argc=0
static void* g_origPlayAd = nullptr;    // MiaoYueSDKProxy::PlayADVideo argc=2
static void* g_onAdResultMP = nullptr;  // DS_Advertising::OnADVideoResult 的 native 桥

// v26x：`g_playerPtr` 的定义**提前到伤害钩子之前**（原来在 §⑫ 演员区，但 `Hooked_Hurt5`
//   在文件更前面，需要用它判「这一击是不是玩家打的」）。全文件只有这一处定义。
static uintptr_t g_playerPtr = 0;  // GameObjectManager._player 的值（唯一可靠的玩家身份来源）

static void Hooked_Damage3(void* self, void* a0, void* a1, void* a2, void* a3, void* a4, void* a5,
                           void* a6, void* a7) {
    DmgHook_t orig = reinterpret_cast<DmgHook_t>(g_origDmg3);
    bool cancel = ApplyDamagePolicy(self, a0);
    if (!cancel && orig) orig(self, a0, a1, a2, a3, a4, a5, a6, a7);
}

static void Hooked_Damage2(void* self, void* a0, void* a1, void* a2, void* a3, void* a4, void* a5,
                           void* a6, void* a7) {
    DmgHook_t orig = reinterpret_cast<DmgHook_t>(g_origDmg2);
    bool cancel = ApplyDamagePolicy(self, a0);
    if (!cancel && orig) orig(self, a0, a1, a2, a3, a4, a5, a6, a7);
}

static void Hooked_TakeDamage(void* self, void* a0, void* a1, void* a2, void* a3, void* a4,
                              void* a5, void* a6, void* a7) {
    DmgHook_t orig = reinterpret_cast<DmgHook_t>(g_origTake);
    bool cancel = ApplyDamagePolicy(self, a0);
    if (!cancel && orig) orig(self, a0, a1, a2, a3, a4, a5, a6, a7);
}

// —— ⑥c 抓点主桩：CBattleActorUtil::Hurt（#1 秒杀小怪 / #3 全模式技能伤害增加）——
// ★v26x：这一枚就是**斌哥的同款机制**（`[冒险-打怪] 改前 m=%.4f e=%d → 改后 m=%.4f e=%d`）。
//   唯一改动点 = `CtagHurtHPPara::m_llValue`（BigInt）。
//   安全性（v26x 新加，修掉一个致命设计缺陷）：
//     · 放大**只在「这一击的 attacker 是玩家」时**做 —— 旧版只要 target 不是玩家就放大，
//       于是**小怪打玩家的伤害也被放大 1000 倍** ⇒ 秒杀小怪一开、无敌一关，玩家瞬间暴毙。
//       现在按 `m_pAttacker`（+0x18）判定，并且玩家身份只认 `GameObjectManager._player` 指针
//       （v26p 血证：战斗里所有 Character 的 roleId 都是 0，roleId 判据不可用）。
//     · `m_pAttacker` 偏移若解析不到（g_offParaAttacker==0）→ 退回旧行为（放大所有非玩家目标），
//       保证功能不至于因为一个字段名解析失败而整体失效。
//     · 玩家当 target 且开无敌 → 清零（第二层保险，主保险是 ActionState.immuneDamage@60）。
/* ★v26y：核心策略抽成**独立函数**，因为它有两个调用者：
 *   ① `Hooked_Hurt5`（Dobby 内联桩）—— 本环境**挂不上**（见 kDmgHooks 处说明）；
 *   ② `InterpFire`（解释器入口分发）—— **真正生效的那条**（`Hurt` 是解释方法）。
 *  入参 `refSlot` = 「指向 `CtagHurtHPPara` 引用」的槽（ref 参数槽里存的是对象引用，需解引用）。
 *
 *  ★v27 三处修改（原版有两条「功能不可能生效」+ 一条「超量级安全线」的硬伤）：
 *   ① 判据 `OkPtr` → `LivePtr`：`OkPtr` 内部对 8192 条 maps 快照线性扫
 *      （`Il2CppApi.cpp:727`），而这个函数**每次受击都跑**，不能用它。
 *   ② 不再要求「装钩前就拿到 para 布局」：`ResolveParaLayout()` 依赖
 *      `FindClassAnywhere("CtagHurtHPPara")` 全镜像遍历（375ms~1s，且开机期必然失败 →
 *      v26x 实测 `ready=0`）。改成**从活对象反查**：`Hurt` 真被调用的那一刻手上就有 `para`，
 *      直接用它自己的 klass 按名取字段偏移（`ResolveParaLayoutFromObj`）—— 零全镜像遍历。
 *   ③ 删掉「秒杀时把指数抬到 ≥12」= 1e12 量级（高危区，见 `kKillMul` 处说明）。 */

/** ⑫-v27 从**活对象**反查 `CtagHurtHPPara` 布局（替代 `ResolveParaLayout` 的全镜像遍历）。
 *  调用时机：解释器入口第一次真的分发到 `CBattleActorUtil::Hurt` 时（main 线程，一次）。
 *  代价：3 次 `class_get_field_from_name`（µs 级元数据查询），不碰 `domain_assemblies`。 */
static void ResolveParaLayoutFromObj(void* para) {
    auto& api = GetApi();
    if (!api.class_name) return;
    void* kp = Il2Cpp::ReadPtr(para, 0);  // Il2CppObject.klass = +0x00
    if (!LivePtr(kp)) return;
    const size_t offT = api.FieldOffset(kp, "m_pTarget", 0);
    const size_t offA = api.FieldOffset(kp, "m_pAttacker", 0);
    const size_t offV = api.FieldOffset(kp, "m_llValue", 0);
    if (!offT || !offV || offT > 0x400 || offV > 0x400) {
        DBGLOG(TPL_LOG " para layout(obj) 失败 cls=%s target=%zu value=%zu", api.class_name(kp), offT,
             offV);
        return;
    }
    g_offParaTarget = offT;
    g_offParaAttacker = (offA && offA <= 0x400) ? offA : 0;
    g_offParaValue = offV;
    /* BigInt 布局：`m_mantissa : double` 在前、`m_exponent : int` 在后（`_钩子目标与语义.md` §BigInt）。
     * 这里**不查 BigInt 类**（那又要一次全镜像遍历）：直接按「BigInt 起点 = mantissa、+8 = exponent」取。
     * `BigIntGet` 自带 NaN / 范围校验（mant∈[0,1e15]、exp∈[-64,64]），偏移猜错会自己拒改，不会乱写。 */
    g_offBiMant = 0;
    g_offBiExp = 8;
    g_paraReady.store(true, std::memory_order_relaxed);
    DBGLOG(TPL_LOG " para layout(obj) cls=%s target=+0x%zx attacker=+0x%zx value=+0x%zx biExp=+0x%zx",
         api.class_name(kp), g_offParaTarget, g_offParaAttacker, g_offParaValue, g_offBiExp);
}

/** ★v30：判「这个指针是不是 `CtagHurtHPPara` 对象」——用于在**实参约定二选一**（见 ApplyHurtPolicy）。
 *  只做有界只读：`obj+0` = klass、`klass+0x10` = 类名字符串，全走 `IsReadableRange` 前置闸，
 *  绝不把野指针交给 `api.class_name`。代价 = 一次元数据读 + 一次 strstr（只在布局定案前跑）。 */
static int LooksLikeHurtPara(void* p) {
    if (!LivePtr(p)) return 0;
    if (!Il2Cpp::IsReadableRange((uintptr_t)p, 0x40)) return 0;
    const uintptr_t k = *reinterpret_cast<const uintptr_t*>(p);  // 不用 RawWord（它定义在本文件更下方）
    if (!LivePtr((void*)k) || !Il2Cpp::IsReadableRange(k, 0x20)) return 0;
    auto& api = GetApi();
    if (!api.class_name) return 0;
    const char* n = api.class_name((void*)k);
    return (n && strstr(n, "HurtHPPara")) ? 1 : 0;
}

/* ==================== v31：照斌哥包的三条机制（IL 逐字定案，不再自创） ====================
 *  权威依据：本轮直读 `Assembly-CSharp.dll`（22,647,808 B / TypeDef 14,971 / MethodDef 173,184）的元数据，
 *  工具 `python _tgt_q.py il 0xTOKEN`（workdir `06_dumped/冒险与征途/aot`）。
 *  ① 击杀链（#1 秒杀小怪）：`StarsCollider::DoAttack 0x0601E8D9` → `DamageFactory::ProcDmgHit 0x0601E578`
 *     → `CBattleActorUtil::Hurt 0x0601E25A`（**只记录**）→ `CHitNotifier::AddOneHitHurt 0x0601E456`
 *     把 `CtagHurtHPPara::GetValue()`（= `m_llValue` FieldDef **0x040173EC**，**不是** `m_llCalculateDamage`
 *     0x040173ED）经 `ToolKit::ToMsgInt128` 写进 `Proto_CSEffectNtf.effects[n].hurt.value`
 *     → `CHitNotifier::EffectToCast 0x0601E461` → `Character::Damage(BigInt,(DamageType)1,true) 0x0601F132`
 *     （全仓**唯一**写 HP 的方法）→ `set_healthPointNow(0)` → `CheckDead` → `ActionState::ToDead() 0x0601F1CE`
 *     （isDead=1）→ `CHitNotifier::SyncDieStateToServer`。
 *     ★ 关键：`CBattleActorUtil::HurtHp 0x0601E25B` **一个属性都不写**（只算 delta + 发事件；
 *       `CBuffTriggerManager::OnAttrChange 0x0601E306` 是纯通知），所以「算出的 delta 被钳」与「能否击杀」
 *       **无关** —— 放大 `m_llValue` 就是真正能杀死小怪的那条路
 *       （斌哥壳日志 `[冒险-打怪#n] ctype=%d 合法=%d | 改前 m=%.4f e=%d → 改后 m=%.4f e=%d`）。
 *  ② 无敌（#2）：`CBattleActorUtil::IsDamageImmune 0x0601E262` 只读 `target.get_state().immuneDamage`
 *     （FieldDef **0x04017E2D**）⇒ 置该位后 `Hurt` 立刻 `SetValue(0)` + `SetNoDamageType(6)`，伤害归零。
 *     斌哥原法 = `StarsCollider::DoAttack` 的 accepter 槽置 null（`DoAttack` IL_0039：accepter 为 null → return）。
 *  ③ 全模式秒杀（#3，= 星空版「全模式技能伤害增加」）：
 *     `BattleElementCenter::get_localDamage 0x0601E7FB` = `collider.localDamage`；
 *     `BattleElementCenter::set_localDamage(bool) 0x0601E7FA` = `ldsfld collider; stfld StarsCollider::localDamage`
 *     ⇒ 直接写这个字段（1 字节，零 hook）。`StarsCollider::DoAttack` IL_03CC：
 *       `if (localDamage || IsEditorMode || BattleElementCenter::get_localDamage())`
 *         → `DamageFactory::ProcDamageByClient 0x0601E570`（客户端权威伤害，用本地公式
 *           `GameFormula::GetResult(attacker,target,fomulaID,paramA,paramB,dmgConst,factorA,factorB)`）
 *     ⇒ 配合 `DMGARG`（factorA/set_damageFactor_A 上乘 mul）即「所有技能持续增伤」。
 *     （斌哥壳日志 `[冒险-关卡模式#%d] localDamage=%d (1=客户端权威)`。） */

static std::atomic<uintptr_t> g_playerHitterV31{0};  // 玩家 IGO 的 `_hitter`（Hitter 实例）
static std::atomic<size_t> g_offHitterV31{0};        // InteractiveGameObject::_hitter（IL FieldDef 0x04017109）
/* v31：这两个**上移到此处**（原定义在文件下方的阵营区）—— 因为 `ApplyHurtPolicy` 的
 * 「同阵营跳过」闸门要用它们，而它在文件更上方。 */
static int32_t g_playerCamp = 0;      // 玩家阵营（`CampTool::ConvertCamp` 恒等 ⇒ 原始 camp）
static bool g_playerCampSet = false;  // 玩家阵营是否**已解析**（合法值就是 0，不能用 `!=0` 当闸）
static int32_t ActorCamp(uintptr_t actor);           // 定义在本文件更下方
static size_t StateOffV31(void* obj);                // 同上（另在本文件下方定义）

/* ★★v32 玩家身份判据 —— 修「四项全开打首领，玩家先死」的真凶（v31 血证驱动）。
 *  现场（01:38 首领关）：`GMPLAY _player=0x7d42b47f4700 camp=0 set=1`、`ACTSUM n=56 main=0 mob=10`
 *  ⇒ 场上 56 个演员**没有一个等于 `_player`**；而 `SECATTR` 里真正承伤的是
 *  `Character actor=0x7d426d1e7700 hpNow=90724 role=131202533413845712`，
 *  连玩家分身 `SimpleCharacter 0x7d42b47f4700` 的 roleId 也是同一个 **131202533413845712**。
 *  即：战斗里 `_player` 指向分身（本尊），承伤的是另一个 `Character`，两者 **`ulRoleId` 相同**。
 *  v31 的 `tPlayer = (target == _player) ? true : IsPlayerObj(target)` 两条都不成立
 *  （`IsPlayerObj` 的类名词表里没有 "Character"，`GameObjectManager::IsPlayer` 桥 `gm=0x0` 未解析）
 *  ⇒ `tPlayer=false` ⇒ `×100` 的 KILLHURT 倍率被加在**打向玩家的伤害**上 ⇒ 玩家先死、首领同死、判负。
 *  v32 的修法（不猜、直接用游戏自己的身份量）：
 *    ① 玩家 RoleId 从 `_player` 的 `cdata+112` 直接读（`PlayerRoleEnsureV32`，每拍刷新）；
 *    ② 演员循环里把「`== _player`」或「roleId == 玩家 RoleId」的演员记入历史表（`PlayerSideMarkV32`）；
 *    ③ `Hurt` 判定时**只认这两个判据**，命中即玩家侧（走无敌零伤 / 不清则完全不写）。
 *  ⇒ 任何判据都不可用的对象一律**不是**玩家侧（保持 秒杀/免广告 可用），但玩家侧判据一旦命中就**一字节不写**。 */
static std::atomic<uint64_t> g_playerRoleV32{0};  // 玩家 `ulRoleId`（`_player` 的 cdata+112）
static std::atomic<uint32_t> g_plHistN{0};        // 玩家侧演员历史表（本拍扫到的全部玩家侧对象）
static uintptr_t g_plHistV32[16];
static uint32_t g_plSideLogV32 = 0;               // 只为打日志用（主线程写、主线程读）

static size_t HitterFieldOff();                    // 定义在下方（v31）
static uintptr_t ActorHitterV32(uintptr_t actor);  // 定义在下方（v33）
static int32_t CtypeOfV36(void* obj);              // 定义在下方（v36，`cdata+148` 标定之后）
static uint64_t RoleIdOfV40(void* obj);            // 定义在下方（v40，按名读 `ulRoleId`）
static bool PlayerSideV41(void* obj);              // 定义在下方（v41，玩家侧判据合集）
static void* CasterOfV41(void* acd, void* ak, const char* an);  // 定义在下方（v41）
static void ProjMarkPlayerDataV41(void* data);     // 定义在下方（v41）
static bool ProjIsPlayerDataV41(void* data);       // 定义在下方（v41）

static void PlayerSideMarkV32(uintptr_t a) {
    if (!a) return;
    const uint32_t n = g_plHistN.load(std::memory_order_relaxed);
    for (uint32_t i = 0; i < n && i < 16; i++)
        if (g_plHistV32[i] == a) return;
    if (n < 16) {
        g_plHistV32[n] = a;
        g_plHistN.store(n + 1, std::memory_order_relaxed);
    }
}

/** 玩家身份判据（三条并列，任一成立即算玩家侧）：
 *  ① `_player` 指针本身；
 *  ② 本拍扫到的玩家侧历史表（`roleId` 与 `_player` 相同的演员）；
 *  ③ **同帧活体判据**：受击对象的 `InteractiveGameObject::_hitter`（IL FieldDef 0x04017109）
 *     就是当前 `_player._hitter`。设备日志已实测这条命中（`INV cancel DoAttack 槽 2
 *     accepter=0x7d4289b32d80 playerHitter=0x7d4289b32d80`）⇒ 它是「此刻谁是玩家」最可靠的读数，
 *     不受 `_player` 每 400ms 刷新延迟与跨场景指针残留的影响（v32 里 ① ② 全失效的补丁）。 */
static bool IsPlayerActorV32(void* obj) {
    const uintptr_t t = (uintptr_t)obj;
    if (!LivePtr(obj)) return false;
    if (g_playerPtr && t == g_playerPtr) return true;
    const uint32_t n = g_plHistN.load(std::memory_order_relaxed);
    for (uint32_t i = 0; i < n && i < 16; i++)
        if (g_plHistV32[i] == t) return true;
    const uintptr_t ph = g_playerHitterV31.load(std::memory_order_relaxed);
    if (ph) {
        const uintptr_t h = ActorHitterV32(t);
        if (h && h == ph) return true;
    }
    return false;
}

/** ★v47 防闪退/防「功能永久失效」兜底：受击策略的重入闸改 **RAII**。
 *
 *  旧写法是手工配对：开头 `if (g_dmgReenter.exchange(true)) return;`、函数**末尾**一行
 *  `g_dmgReenter.store(false)`。这在今天是对的（函数体里没有第二个 return），但它是
 *  「**只要有人在中间加一个 return，这条闸就永久上锁**」的结构 —— 一旦上锁，
 *  `Hurt` 这一整条路（#1 秒杀小怪 / #2 无敌的受击清零 / #3 的技能增伤统计）
 *  会在**本次进程剩余时间内全部静默失效**，而且没有任何日志。
 *  这正是用户报的「有时候功能会失效」那一类"不可自恢复"故障的形状，所以按结构消掉它。
 *  RAII 保证：无论从哪个分支离开、哪怕中途抛异常，计数一定归位。
 *  （`g_dmgReenter` 是全局而非 thread_local：跨线程撞上时后来者**只跳过本次策略**，
 *   不会写坏状态；保持原语义不变，只把「配对」这件事交给析构函数。） */
struct HurtReenterGuard {
    bool pass;
    HurtReenterGuard() : pass(!g_dmgReenter.exchange(true, std::memory_order_acq_rel)) {}
    ~HurtReenterGuard() {
        if (pass) g_dmgReenter.store(false, std::memory_order_release);
    }
    HurtReenterGuard(const HurtReenterGuard&) = delete;
    HurtReenterGuard& operator=(const HurtReenterGuard&) = delete;
};

static void ApplyHurtPolicy(void* refSlot) {
    const bool kill = g_killMob.load(std::memory_order_relaxed);
    const bool inv = g_invincible.load(std::memory_order_relaxed);
    const bool skill = g_skillDmg.load(std::memory_order_relaxed);
    if (!(kill || inv || skill)) return;
    if (!LivePtr(refSlot)) return;
    /* ★v34→**回退**（用户 m02501：「与最开始没任何变化，还把无敌改坏了…你快别改」）：
     *  这里恢复成 v33 你已验证「#1、#2 单独开都没问题」的**硬闸**，一行不多一行不少。
     *  ★v47：配对交给 `HurtReenterGuard` 的析构函数（结构上不再可能漏配对而永久锁死）。 */
    HurtReenterGuard reenter;
    if (!reenter.pass) return;
    /* ★★v30 `para` 定案（v29b 血证驱动，**不再靠猜**）：
     *  ① 已实测的实参布局：入口 `a1` 是「**实参值内联**的 8 字节槽区」（`DMGARG` 里 slot 直接是
     *     0.5f/0.4f/40.0f/1.0f 的位模式），不是「指向实参的指针数组」。
     *  ② `Hurt(ref CtagHurtHPPara para, ref bool, bool, ref BigInt, bool)` 是**静态**方法
     *     （`base=0`，v29b `LEARN ok ... Hurt base=0`）⇒ 槽 0 就是第一个参数 `para`。
     *     参数类型是 **`ref`** ⇒ 槽里放的是「指向调用方变量的指针」，不是对象引用本身。
     *  ③ 于是从槽 0 的原始值出发，**解一层**应当得到 para 对象（内联约定下）；
     *     若解一层拿到的不是 `CtagHurtHPPara`（例如引擎改用「arg 槽=指向变量、变量里才是指针」
     *     的双层形态），**再解一层**就对了。
     *  ⇒ 做法：**按深度 1/2 依次试，谁的对象 klass 名字含 `HurtHPPara` 就定案谁，并把深度缓存下来**
     *    （`g_hurtDepth`），之后热路径**只解固定层数**、不做任何类名查询。
     *  这是「自证后才写」：定不出深度 ⇒ `para=nullptr` ⇒ 一个字节都不写，只打一行日志。 */
    void* para = nullptr;
    static std::atomic<int> g_hurtDepth{-1};  // -1 未定案；1/2 = 从槽 0 起解的层数；0 = 判定失败（暂缓重试）
    static std::atomic<uint32_t> g_hurtMiss{0};
    int depth = g_hurtDepth.load(std::memory_order_relaxed);
    if (depth == 0 && g_hurtMiss.fetch_add(1, std::memory_order_relaxed) >= 5000) {
        g_hurtMiss.store(0, std::memory_order_relaxed);
        g_hurtDepth.store(-1, std::memory_order_relaxed);  // 每 5000 次失败后重试一次（慢但不会永久哑）
        depth = -1;
    }
    if (depth > 0) {
        void* p = nullptr;
        if (Il2Cpp::IsReadableRange((uintptr_t)refSlot, 8)) {
            p = *reinterpret_cast<void**>(refSlot);
            for (int k = 1; k < depth && LivePtr(p) && Il2Cpp::IsReadableRange((uintptr_t)p, 8); k++)
                p = *reinterpret_cast<void**>(p);
            if (!LivePtr(p)) p = nullptr;
        }
        para = p;
    } else if (depth == 0) {
        // 已判定过「两种深度都不像」，本轮不再花代价探测
    } else {
        void* cand = Il2Cpp::IsReadableRange((uintptr_t)refSlot, 8)
                         ? *reinterpret_cast<void**>(refSlot)
                         : nullptr;  // 深度 1（内联约定下的正确形态）
        void* cand2 = (LivePtr(cand) && Il2Cpp::IsReadableRange((uintptr_t)cand, 8))
                          ? *reinterpret_cast<void**>(cand)
                          : nullptr;  // 深度 2（旧「指针数组」约定下的正确形态）
        if (LooksLikeHurtPara(cand)) {
            para = cand;
            g_hurtDepth.store(1, std::memory_order_relaxed);
            DBGLOG(TPL_LOG " HURT para 定案 depth=1 refSlot=%p para=%p（内联约定）", refSlot, para);
        } else if (LooksLikeHurtPara(cand2)) {
            para = cand2;
            g_hurtDepth.store(2, std::memory_order_relaxed);
            DBGLOG(TPL_LOG " HURT para 定案 depth=2 refSlot=%p para=%p（指针数组约定）", refSlot, para);
        } else {
            g_hurtDepth.store(0, std::memory_order_relaxed);
            DBGLOG(TPL_LOG " HURT para 未识别 refSlot=%p w=%p w2=%p（两种约定都不像 HurtHPPara，本轮不写）",
                 refSlot, cand, cand2);
        }
    }
    if (LivePtr(para) && !g_paraReady.load(std::memory_order_relaxed))
        ResolveParaLayoutFromObj(para);
    if (LivePtr(para) && g_paraReady.load(std::memory_order_relaxed)) {
        void* target = Il2Cpp::ReadPtr(para, g_offParaTarget);
        void* attacker = g_offParaAttacker ? Il2Cpp::ReadPtr(para, g_offParaAttacker) : nullptr;
        void* bi = reinterpret_cast<char*>(para) + g_offParaValue;
        double mant = 0.0;
        int32_t exp = 0;
        if (LivePtr(target) && BigIntGet(bi, mant, exp)) {
            /* ★v36（**照斌哥包重新抓的修改点**，他壳串池 anon1b @0xb310e 原文）：
             *   `[冒险-打怪#%d] ctype=%d 合法=%d | 改前 m=%.4f e=%d → 改后 m=%.4f e=%d`
             *  ⇒ 日志按他的原格式输出；`合法` 定案为**唯一可靠的那条判据**：
             *    攻击者的 `_hitter` == 玩家的 `_hitter`（v31/v33 血证：
             *    `INV cancel DoAttack 槽 2 accepter=0x7d4289b32d80 playerHitter=0x7d4289b32d80` 两者相等、命中）。
             *    **只有合法的一击才放大** ⇒ 把「打向玩家的那一笔被 ×kKillMul」这条
             *    「人物先死、首领后死」的根因直接切断（这正是 #1+#2 同开时玩家先死的原因）。 */
            /* 他日志的第一个字段 `ctype` = 被打目标的 `CharacterData.characterType`（`cdata+148`）。 */
            const int32_t ctype = CtypeOfV36(target);
            const uintptr_t atkH = ActorHitterV32((uintptr_t)attacker);
            const uintptr_t plH = g_playerHitterV31.load(std::memory_order_relaxed);
            /* ★v37（m02615 实测血证，修正 v36 过窄的 `合法`）：
             *  v36 只认「攻击者 `_hitter` == 玩家 `_hitter`」，实测 33 条**全部 `合法=1` 却全部
             *  打在同一个 `target=0x7d426b08d700`**，首领/真正的输出那一笔全走「跳过」
             *  ⇒ 用户看到的「单开 #1 没任何效果」。
             *  机械原因（两条血证摆在一起就能看出）：
             *    · `V31 playerHitter _player=0x7d426b08de00 _hitter=0x7d426fe33b40 off=0x128`
             *      ⇒ 只有**分身**的 `_hitter` 等于 `_player._hitter`；
             *    · v33 `SECATTR` 血证：分身 `SimpleCharacter 0x7d42b47f4700` 与**真正承伤/打出
             *      伤害的 `Character 0x7d426d1e7700` 是两个对象**，但 **roleId 相同**
             *      （131202533413845712）。
             *  ⇒ 正确判据 = 指针/roleId 这条宽判据（`IsPlayerActorV32`）**或** `_hitter` 相等；
             *    同时补一道**目标侧反向闸**（目标是玩家侧一律不放大），双保险，
             *    既恢复 #1 的放大威力，又不让「怪打玩家」的那一笔被乘。 */
            /* ★v40（用户 m02805：「打完一个首领后，1 的效果就没了」）：
             *  v39 日志血证 —— 首领战后 `合法` 恒 0，因为两条旧判据都依赖**会被刷掉的缓存量**
             *  （`g_playerHitterV31` 被刷成 0；`IsPlayerActorV32` 的指针历史表过期），且不自恢复。
             *  现在补一条**每笔命中现读**的判据：`ulRoleId`（`ActionGameObjectData 0x04017D2F`）。
             *  血证：玩家侧（分身与真正承伤的战斗 `Character`）roleId 相同（131202533413845712），
             *  小怪/首领恒 0 ⇒ `atkRole != 0 && atkRole == plRole` 就是「玩家侧打出的那一笔」，
             *  与场景切换无关。旧判据全部保留做「或」，只会更宽、不会更窄。 */
            const uint64_t plRole = g_playerRoleV32.load(std::memory_order_relaxed);
            const uint64_t atkRole = RoleIdOfV40(attacker);
            const uint64_t tgtRole = RoleIdOfV40(target);
            const bool atkByRole = (atkRole != 0 && plRole != 0 && atkRole == plRole);
            const bool tgtByRole = (tgtRole != 0 && plRole != 0 && tgtRole == plRole);
            const bool atkIsPlayer =
                atkByRole || IsPlayerActorV32(attacker) || (atkH != 0 && plH != 0 && atkH == plH);
            const bool legal = atkIsPlayer;
            const uintptr_t tgtH = ActorHitterV32((uintptr_t)target);
            const bool tPlayer =
                tgtByRole || IsPlayerActorV32(target) || (tgtH != 0 && plH != 0 && tgtH == plH);
            const int32_t camp = ActorCamp((uintptr_t)target);
            const bool ally =
                (camp > 0 && g_playerCampSet && g_playerCamp > 0 && camp == g_playerCamp);
            g_dmgSeen.fetch_add(1, std::memory_order_relaxed);
            /* ★v36：**删掉** v32 自造的「打向玩家就归零」那一层 —— 斌哥的 #2 只有一条机制
             *  （`DoAttack` 的 accepter 置 null，串池 @0xb319c），他没有任何免疫位/归零日志；
             *  v33 实测这一支触发 **0 次**（`INV zero hurt` 恒 0）。功能留着只会互相干扰。 */
            static std::atomic<uint32_t> s_killWrites{0};
            static std::atomic<uint32_t> s_killSkipV36{0};
            /* ★v37：再加一道**目标侧**闸 —— 只放大打在**敌方阵营**（`camp == 1`）身上的那一笔。
             *  血证：v37 日志里全部被放大的 `打怪#` 行都是 `camp=1`（小怪/首领都是 1），
             *  而玩家自己 `GMPLAY camp=0` ⇒ 「怪打玩家」那一笔的 target `camp=0`，会被这里挡掉。
             *  （v36 只有攻击者侧一条判据，实测「#1+#2 同开 → 人物还掉血」，这道闸就是为它补的。） */
            const bool enemyTarget = (camp == 1);
            /* ★v39（用户 m02712 原话：「#1 首先名字叫秒杀小怪，首领为什么会有效果还会直接死？」）：
             *  菜单名就是**秒杀小怪** ⇒ 用斌哥日志里的第一个字段 `ctype` 把**首领**排除：
             *  `CharacterType`（0x020000F2）字段序 = PLAYER, APC, **MONSTER(2)**, **BOSS(3)**, …。
             *  判据取「**确认为首领**（ctype==3）才拦」，读不到（-1）时不拦 —— 日志会打出真实 ctype，
             *  下一轮实测就能把 MONSTER/BOSS 的真值钉死。 */
            const bool bossCtype = (ctype == 3);
            if (kill && legal && enemyTarget && !tPlayer && !ally && !bossCtype) {
                const double mul = static_cast<double>(kKillMul);
                double nm = mant * mul;
                int32_t ne = exp;
                while (nm > 1.0e9 && ne < 300) {  // 归一化，避免溢出/精度崩
                    nm /= 10.0;
                    ne += 1;
                }
                BigIntSet(bi, nm, ne);
                g_hitDmg.fetch_add(1, std::memory_order_relaxed);
                const uint32_t n = s_killWrites.fetch_add(1, std::memory_order_relaxed);
                if (n < 20 || (n % 60) == 0)
                    DBGLOG(TPL_LOG
                         " 打怪#%u ctype=%d 合法=%d | 改前 m=%.4f e=%d → 改后 m=%.4f e=%d "
                         "(target=%p camp=%d atkR=%llu)",
                         n, ctype, legal ? 1 : 0, mant, exp, nm, ne, target, camp,
                         (unsigned long long)atkRole);
            } else if (kill) {
                const uint32_t n = s_killSkipV36.fetch_add(1, std::memory_order_relaxed);
                if (n < 10 || (n % 80) == 0)
                    DBGLOG(TPL_LOG
                         " 打怪#%u ctype=%d 合法=%d | 跳过（%s）tPlayer=%d ally=%d camp=%d atkH=%p "
                         "plH=%p plR=%llu atkR=%llu tgtR=%llu",
                         n, ctype, legal ? 1 : 0,
                         bossCtype ? "首领(ctype==3)不放大" : "照斌哥:只改合法的一击", tPlayer ? 1 : 0,
                         ally ? 1 : 0, camp, (void*)atkH, (void*)plH, (unsigned long long)plRole,
                         (unsigned long long)atkRole, (unsigned long long)tgtRole);
            }
            /* #3 在这里只做统计、不写伤害 —— 斌哥的 #3 落在 `localDamage`（碰撞体）与投射物模块上。 */
            if (skill) g_hitSkill.fetch_add(1, std::memory_order_relaxed);
        } else {
            static std::atomic<uint32_t> s_bad{0};
            uint32_t n = s_bad.fetch_add(1, std::memory_order_relaxed);
            if (n < 5)
                DBGLOG(TPL_LOG " HURT para 不可读 refSlot=%p para=%p ready=%d", refSlot, para,
                     g_paraReady.load() ? 1 : 0);
        }
    }
    // ← 出作用域即由 `HurtReenterGuard` 释放重入闸（v47：不再手工配对）
}

/** Dobby 内联桩 —— **备用路径**：本环境 `Hurt` 是解释方法（methodPointer = 解释器共享桩），
 *  这一枚实际永远不会被调用；保留是为了「某天热更方法被 AOT 编译成真本地码」时不至于丢功能。 */
static void Hooked_Hurt5(void* a0, void* a1, void* a2, void* a3, void* a4, void* a5, void* a6,
                         void* a7) {
    DmgHook_t orig = reinterpret_cast<DmgHook_t>(g_origHurt);
    // 首个 IL 实参 = ref CtagHurtHPPara（参数槽里存的是对象引用，需解引用）
    void* refSlot = g_hurtStatic.load(std::memory_order_relaxed) ? a0 : a1;
    ApplyHurtPolicy(refSlot);
    if (orig) orig(a0, a1, a2, a3, a4, a5, a6, a7, nullptr);
}

// —— ⑥d 抓点主桩：StarsCollider::DoAttack（#2 无敌）——
// accepter 置 null = 官方「命中作废」分支；同时兜底 Character::Damage（见 Hooked_Damage3）。
static void Hooked_DoAttack7(void* a0, void* a1, void* a2, void* a3, void* a4, void* a5, void* a6,
                             void* a7) {
    DmgHook_t orig = reinterpret_cast<DmgHook_t>(g_origDoAttack);
    if (g_invincible.load(std::memory_order_relaxed)) {
        const int base = g_doAttackStatic.load(std::memory_order_relaxed) ? 2 : 3;  // IL arg index 2
        void** slot = (base == 2) ? &a2 : &a3;
        void* accepter = *slot;
        if (OkPtr(accepter)) {
            if (!g_offAcceptIgo || !g_acceptKlass) {
                auto& api = GetApi();
                void* k = api.object_class ? api.object_class(accepter) : nullptr;
                if (k) {
                    g_offAcceptIgo = api.FieldOffset(k, "interactiveGameObject", 0);
                    g_acceptKlass = k;
                    DBGLOG(TPL_LOG " accepter igo=+0x%zx", g_offAcceptIgo);
                }
            }
            auto& api = GetApi();
            const bool sameKlass =
                !g_acceptKlass || !api.object_class || api.object_class(accepter) == g_acceptKlass;
            void* igo =
                (g_offAcceptIgo && sameKlass) ? Il2Cpp::ReadPtr(accepter, g_offAcceptIgo) : nullptr;
            if (IsPlayerObj(igo) || IsPlayerObj(accepter)) {
                *slot = nullptr;
                uint32_t n = g_hitInv.fetch_add(1, std::memory_order_relaxed);
                if (n < 10 || (n % 50) == 0)
                    DBGLOG(TPL_LOG " INV cancel DoAttack accepter=%p igo=%p", accepter, igo);
            }
        }
    }
    if (orig) orig(a0, a1, a2, a3, a4, a5, a6, a7, nullptr);
}

// —— ⑥e 抓点副桩：BattleElementCenter::get_localDamage → 恒 true（客户端权威，配合 #3）——
static void* Hooked_LocalDamage(void* a0, void* a1, void* a2, void* a3, void* a4, void* a5,
                                void* a6, void* a7) {
    AdBoolRet_t orig = reinterpret_cast<AdBoolRet_t>(g_origLocalDmg);
    if (g_skillDmg.load(std::memory_order_relaxed)) return reinterpret_cast<void*>(1);
    return orig ? orig(a0, a1, a2, a3, a4, a5, a6, a7) : nullptr;
}

/* ============================================================================
 * ★v46 免广告代回调：**待办槽 + 帧外执行**（声明放这里的原因：路径A 的代码就在下面几行）
 *
 * 真机血证（`/data/tombstones/tombstone_11`，pid 31958 / tid 32007 UnityMain，560 帧）：
 *   旧版有**两条互不知情的代回调路径**，且都在「我们自己 hook 的帧内」当场调托管方法：
 *     路径A `Hooked_PlayADVideo2`（静态挂点 `kAdSlots` 的 `PlayAD2`，`required=true`）
 *             → `CallOnAdResult` 拿 `g_onAdResultMP` 方法指针**直调**；
 *     路径B `ApplyNoAdV43`（解释器入口分发 `kKinds` 的 `K_PLAYAD`）→ `api.runtime_invoke`。
 *   一次广告请求因此发**两次** `OnADVideoResult`，而回调里游戏再触发广告请求时，
 *   就与「仍在栈上的我们」互相递归 ⇒ UnityMain 栈 8MB 被涨破
 *   （`Fatal signal 11 SIGSEGV SEGV_ACCERR fault addr 0x743401ffc8`，sp 距栈底仅 0x70 字节）。
 *
 * 现在：两条路径**只往同一个槽登记**，由 `DrainPendingAdV43` 在下一次解释器入口
 * 取走一次、执行一次（覆盖式登记 ⇒ 天然的重复合并）。
 * ============================================================================ */
static std::atomic<uint32_t> g_pendAdReady{0};
static std::atomic<int32_t> g_pendAdId{0};
static std::atomic<uint64_t> g_pendAdUuid{0};
static std::atomic<uint32_t> g_pendAdQueued{0};
static std::atomic<uint64_t> g_adV43FuseUntilMs{0};
static constexpr uint64_t kAdV43FuseCooldownMs = 30000ull;
static thread_local int t_modInvoke = 0;  // >0 = 本线程正在执行 mod 主动发起的托管调用
static bool Il2CppU64OfStringV43(void* s, uint64_t* out);  // 定义见 §⑦c（免广告 A2 段）

/** 登记一次免广告代回调（覆盖式：同一时刻最多一个待办）。`src` = 哪条路径登记的。 */
static void EnqueueNoAdV43(int32_t adId, uint64_t uuid, const char* src) {
    g_pendAdId.store(adId, std::memory_order_relaxed);
    g_pendAdUuid.store(uuid, std::memory_order_relaxed);
    g_pendAdReady.store(1, std::memory_order_release);
    const uint32_t q = g_pendAdQueued.fetch_add(1, std::memory_order_relaxed) + 1;
    if (q <= 10)
        LOGI(TPL_LOG " AD43 登记待办#%u adid=%d uuid=%llu ← %s（改到帧外代回调）", q, adId,
             (unsigned long long)uuid, src);
}

// ============================================================
// ⑦ 广告路径（#4 免广告）：客户端放行 + 强制成功结果
//    说明：服务端 start_watch_ad_req / finish_watch_ad_req 校验风险见文档 §10
// ============================================================
/* ★v46b 每 tag 计数（诊断）：真机实测「开免广告后 canclaim/vresult 两条上游调用量从 180/s
 *  飙到 1087/s（6 倍）」，但 `分流 ad` 只有总数、无法区分是哪一个 tag 在自激 ⇒ 这里把 6 个
 *  HookSlot 的调用数分开累加，随 STATS 每 30 秒多打一行 `STATS-AD`。 */
enum AdTagIdx {
    kAdTagWatch = 0,
    kAdTagWatch0,
    kAdTagClick,
    kAdTagCanClaim,
    kAdTagResult,
    kAdTagVResult,
    kAdTagPlayAd,
    kAdTagN
};
static const char* const kAdTagNames[kAdTagN] = {"watch", "watch0",      "click",     "canclaim",
                                                 "result", "vresult",    "playad"};
static std::atomic<uint32_t> g_adTagCnt[kAdTagN]{};

static void AdLogOnce(int idx, uint32_t n) {
    g_adTagCnt[idx].fetch_add(1, std::memory_order_relaxed);
    if (n < 20 || (n % 40) == 0) DBGLOG(TPL_LOG " AD hit %s n=%u", kAdTagNames[idx], n);
}

/* ============================================================================
 * ★v46b 一次性事件门 —— 治「开启免广告后跟断网一样」
 *
 * 语义：**一次广告请求只该产生一次「成功」**。凡是我们「改写返回值」的地方
 * （canclaim / vresult / 解释器 ForceTrueArg）先过这道 2 秒门，关着就一律按原值透传 orig。
 *
 * 真机血证（v46 装机后 pid 5978）：
 *   · 开免广告 17 秒内 `命中 免广告` 从 0 冲到 **25267（≈1087/s，免广告关闭时基线仅 ≈180/s）**；
 *   · 而两条代回调路径**一次都没走到**（`AD43 见=0 排队=0 已回调=0` ⇒ `PlayADVideo` 根本没被调用）
 *     ⇒ 卡顿与代回调无关，是「我们一直返回成功」让游戏侧上游陷入**自激轮询**
 *        （面板永远认为可领奖、结果永远成功 ⇒ 无限重试 = 表现成「断网一样」）。
 * 上锁后上游拿到的「成功」是稀疏的，自激环自然断；一次真实事件路径完全不受影响。
 * ============================================================================ */
enum AdGateIdx { kAdGateCanClaim = 0, kAdGateVResult, kAdGateInterp, kAdGateN };
static std::atomic<uint64_t> g_adGateUntilMs[kAdGateN]{};
static std::atomic<uint32_t> g_adGatePass[kAdGateN]{};
static std::atomic<uint32_t> g_adGateSkip[kAdGateN]{};
static constexpr uint64_t kAdGateCooldownMs = 2000ull;

static bool AdForceGateOpen(int i) {
    const uint64_t now = NowMs();
    if (now < g_adGateUntilMs[i].load(std::memory_order_relaxed)) {
        g_adGateSkip[i].fetch_add(1, std::memory_order_relaxed);
        return false;
    }
    g_adGateUntilMs[i].store(now + kAdGateCooldownMs, std::memory_order_relaxed);
    g_adGatePass[i].fetch_add(1, std::memory_order_relaxed);
    return true;
}

/** 开/关免广告、ForceClear 时把门重新上膛（清掉冷却窗口）。 */
static void AdForceGateReset() {
    for (int i = 0; i < kAdGateN; i++) g_adGateUntilMs[i].store(0, std::memory_order_relaxed);
}

static void Hooked_AdVoid(void* a0, void* a1, void* a2, void* a3, void* a4, void* a5, void* a6,
                          void* a7) {
    AdVoid_t orig = reinterpret_cast<AdVoid_t>(g_origAdVoid);
    AdLogOnce(kAdTagWatch, g_adSeen.fetch_add(1, std::memory_order_relaxed));
    if (orig) orig(a0, a1, a2, a3, a4, a5, a6, a7);
}

static void Hooked_AdVoid2(void* a0, void* a1, void* a2, void* a3, void* a4, void* a5, void* a6,
                           void* a7) {
    AdVoid_t orig = reinterpret_cast<AdVoid_t>(g_origAdVoid2);
    AdLogOnce(kAdTagWatch0, g_adSeen.fetch_add(1, std::memory_order_relaxed));
    if (orig) orig(a0, a1, a2, a3, a4, a5, a6, a7);
}

static void Hooked_AdVoid3(void* a0, void* a1, void* a2, void* a3, void* a4, void* a5, void* a6,
                           void* a7) {
    AdVoid_t orig = reinterpret_cast<AdVoid_t>(g_origAdVoid3);
    AdLogOnce(kAdTagClick, g_adSeen.fetch_add(1, std::memory_order_relaxed));
    if (orig) orig(a0, a1, a2, a3, a4, a5, a6, a7);
}

static void* Hooked_AdBoolRet(void* a0, void* a1, void* a2, void* a3, void* a4, void* a5, void* a6,
                              void* a7) {
    AdBoolRet_t orig = reinterpret_cast<AdBoolRet_t>(g_origAdBoolRet);
    AdLogOnce(kAdTagCanClaim, g_adSeen.fetch_add(1, std::memory_order_relaxed));
    bool r = orig ? (reinterpret_cast<intptr_t>(orig(a0, a1, a2, a3, a4, a5, a6, a7)) != 0) : false;
    if (r) return reinterpret_cast<void*>(1);
    if (g_noAd.load(std::memory_order_relaxed)) {
        g_hitAd.fetch_add(1, std::memory_order_relaxed);  // 计数语义不变（仍按「命中」记，便于跨版本对比）
        /* ★v46b：改写返回值前过一次性门 —— 一次广告请求只该产生一次「可领奖」。
         *  真机实测：开免广告后本钩调用量涨到基线 6 倍且 g_hitAd≈1087/s ⇒ 恒返回 1
         *  让上游（奖池面板/红点）永远认为可领，陷入自激轮询 = 作者侧「跟断网一样」。 */
        return AdForceGateOpen(kAdGateCanClaim) ? reinterpret_cast<void*>(1) : nullptr;
    }
    return nullptr;
}

[[maybe_unused]] static void Hooked_AdArg0True(void* a0, void* a1, void* a2, void* a3, void* a4,
                                               void* a5, void* a6, void* a7) {
    AdVoid_t orig = reinterpret_cast<AdVoid_t>(g_origAdArg0);
    AdLogOnce(kAdTagResult, g_adSeen.fetch_add(1, std::memory_order_relaxed));
    if (g_noAd.load(std::memory_order_relaxed)) {
        g_hitAd.fetch_add(1, std::memory_order_relaxed);
        a0 = reinterpret_cast<void*>(1);  // 结果标志位置成功（bool/int 同宽）
    }
    if (orig) orig(a0, a1, a2, a3, a4, a5, a6, a7);
}

// OnADVideoResult(bool success, int ad_id, string uuid) argc=3：免广告开时把 success 强制为 true
// （第二层保险；首参可能是 self，按 method_flags 回写的静态标记选位置，绝不盲改 a0）。
static void Hooked_AdArg0TrueB(void* a0, void* a1, void* a2, void* a3, void* a4, void* a5, void* a6,
                               void* a7) {
    AdVoid_t orig = reinterpret_cast<AdVoid_t>(g_origAdArg0b);
    AdLogOnce(kAdTagVResult, g_adSeen.fetch_add(1, std::memory_order_relaxed));
    if (g_noAd.load(std::memory_order_relaxed)) {
        g_hitAd.fetch_add(1, std::memory_order_relaxed);
        /* ★v46b：同样过一次性门；门关着一律按原值透传 orig（不做任何改写）。 */
        if (AdForceGateOpen(kAdGateVResult)) {
            if (g_onResultStatic.load(std::memory_order_relaxed))
                a0 = reinterpret_cast<void*>(1);
            else
                a1 = reinterpret_cast<void*>(1);
        }
    }
    if (orig) orig(a0, a1, a2, a3, a4, a5, a6, a7);
}

// —— ⑦b 路径A：MiaoYueSDKProxy::PlayADVideo(int ad_id, string uuid) argc=2 ——
// ★v46 起**不再使用**：拿方法指针在 hook 帧内直调托管方法，是 tombstone_11 递归放大的来源之一。
// 保留仅为留证（真要复用必须 `t_modInvoke++/--` 包住）。
[[maybe_unused]] static void CallOnAdResult(void* adId, void* uuid) {
    if (!g_onAdResultMP) return;
    AdVoid_t fn = reinterpret_cast<AdVoid_t>(g_onAdResultMP);
    fn(reinterpret_cast<void*>(1), adId, uuid, nullptr, nullptr, nullptr, nullptr, nullptr);
}

static void Hooked_PlayADVideo2(void* a0, void* a1, void* a2, void* a3, void* a4, void* a5, void* a6,
                                void* a7) {
    AdVoid_t orig = reinterpret_cast<AdVoid_t>(g_origPlayAd);
    const bool st = g_playAdStatic.load(std::memory_order_relaxed);
    void* adId = st ? a0 : a1;
    void* uuid = st ? a1 : a2;
    AdLogOnce(kAdTagPlayAd, g_adSeen.fetch_add(1, std::memory_order_relaxed));
    if (g_noAd.load(std::memory_order_relaxed)) {
        g_hitAd.fetch_add(1, std::memory_order_relaxed);
        uint64_t u = 0;
        const bool parsed = Il2CppU64OfStringV43(uuid, &u);
        DBGLOG(TPL_LOG " AD bypass PlayADVideo adid=%p uuid=%p 解析=%d", adId, uuid, parsed ? 1 : 0);
        /* ★v46：只登记待办，帧外由 `DrainPendingAdV43` 统一发**一次**回调 —— 不再当场直调。 */
        if (parsed) {
            EnqueueNoAdV43((int32_t)(intptr_t)adId, u, "playad2");
            return;  // 不调 orig = 不弹真实广告
        }
        /* uuid 解析不出来 ⇒ 登记出去的 uuid=0 服务端不认（玩家会卡在「等广告结果」），
         *  宁可降级成「照常出真广告」也不要把玩法卡死。 */
        LOGW(TPL_LOG " AD43 playad2 uuid 解析失败 → 降级出处真广告 a0=%p a1=%p a2=%p", a0, a1, a2);
    }
    if (orig) orig(a0, a1, a2, a3, a4, a5, a6, a7);
}
// ============================================================
// ⑧ 挂点表（只列本包存在的方法名；解析不到 = 该槽静默降级）
// ============================================================
struct HookSlot {
    const char* tag;
    const char* cls;
    const char* method;
    int         argc;
    int         nth;
    uint8_t     index;
    bool        required;
    void*       hookFn;
    void**      origPtr;
};

// #1/#2/#3：伤害结算。前两枚为主抓点（IL 证据：10_docs/冒险与征途/_钩子目标与语义.md §2/§3）
static HookSlot kDmgSlots[] = {
    // 主：秒杀小怪 + 全模式技能伤害增加（改 para->m_llValue）
    {"Hurt5", "CBattleActorUtil", "Hurt", 5, 0, 0, true, (void*)Hooked_Hurt5, &g_origHurt},
    // 主：无敌（accepter 置 null = 官方「命中作废」分支）
    {"DoAttack7", "StarsCollider", "DoAttack", 7, 0, 1, true, (void*)Hooked_DoAttack7,
     &g_origDoAttack},
    // 无敌兜底：服务器同步效果包最终落到 Character::Damage(BigInt,DamageType,bool)
    {"Damage3", "Character", "Damage", 3, 0, 2, false, (void*)Hooked_Damage3, &g_origDmg3},
    // #3 配套：客户端权威（让技能伤害在全模式生效）
    {"LocalDmg", "BattleElementCenter", "get_localDamage", 0, 0, 3, false,
     (void*)Hooked_LocalDamage, &g_origLocalDmg},
    {"Damage2", "Character", "Damage", 2, 0, 4, false, (void*)Hooked_Damage2, &g_origDmg2},
    {"TakeDamage", "Character", "TakeDamage", 2, 0, 5, false, (void*)Hooked_TakeDamage,
     &g_origTake},
};

// #4：免广告（主抓点 PlayAD2；其余为兜底/观测，解析不到即静默降级）
static HookSlot kAdSlots[] = {
    {"PlayAD2", "MiaoYueSDKProxy", "PlayADVideo", 2, 0, 8, true, (void*)Hooked_PlayADVideo2,
     &g_origPlayAd},
    {"WatchAds1", "DS_Advertising", "WatchAdvertisements", 1, 0, 9, false, (void*)Hooked_AdVoid,
     &g_origAdVoid},
    {"WatchAds0", "DS_Advertising", "WatchAdvertisements", 0, 0, 10, false, (void*)Hooked_AdVoid2,
     &g_origAdVoid2},
    {"CanClaim", "AdMascotManager", "CanClaimReward", 0, 0, 11, false, (void*)Hooked_AdBoolRet,
     &g_origAdBoolRet},
    {"ClickAd", "AdMascotManager", "OnClickWatchAd", 0, 0, 12, false, (void*)Hooked_AdVoid3,
     &g_origAdVoid3},
    {"OnResult7", "DS_Advertising", "OnADVideoResult", 3, 0, 13, false, (void*)Hooked_AdArg0TrueB,
     &g_origAdArg0b},
};

static const size_t kDmgSlotN = sizeof(kDmgSlots) / sizeof(kDmgSlots[0]);
static const size_t kAdSlotN = sizeof(kAdSlots) / sizeof(kAdSlots[0]);

// ============================================================
// v23 总闸（取证结论修正：v22 日志证明**热更方法确实经过解释器入口**
//   —— mxyzt_lg20b.txt 的 60 条 INTEREST 里含 Monster::set_perHp / LHitBoxGroupInput::set_isHurtbox /
//      DataContainer::OnControllerColliderHit / StateData::set_isIgnoreBuffAdd / LHitDataInput::set_showDamage /
//      <>c::<IsMyRoleDamage>b__0 / <>c::<SyncDieStateToServer>b__46_0 等战斗期真实调用。
//   所以入口 hook 不是死路，缺的是「正确的目标名 + 参数槽解释」——斌哥的壳日志明说「本游戏第2参是
//   包装指针,已解一层取名字」「静态方法的参数槽约定由运行时试错定案」。
//   kDiag     = true ：重开入口 hook + 名册/兴趣取证（**纯只读，不写任何游戏内存**）
//   kDmgHooks = false：继续关闭 6 个伤害 Dobby inline hook（v21 实测崩在 libil2cpp 热更代码区
//                      pc=libil2cpp+0x205acd8, x21=Monster klass，7.7s 后死亡 → houdini 翻译缓存失效）
//   kDataWrites=false：关闭数据面写入（v22 实测：ScanInstances 只返回「某字==klass 的地址」，
//                      校验恒真 ⇒ 写入目标是假对象；CALIB 零命中证明 A/C 偏移根本没探到）。
//                      本轮只保留读日志（KILLCFG scan/MONROW/APPLY 计数），不落一个字节。
// ============================================================
// ★v26v 出包定案：**kDiag = false**。
//   理由（三条，全部有实测依据）：
//   ① 解释器入口 hook 是**每次热更方法调用**都要过一遍的 Dobby trampoline —— `interp calls=540000`
//      是活生生的证据；这就是用户反馈「游戏变的好掉帧」的**头号来源**。
//   ② `InterpFeature`（命中目标后的功能分发）自 v11 起就是**空桩**（`(void)t;(void)a1;(void)a2;`
//      "功能写入在下一步接入"从未接入）⇒ 入口 hook 对四项功能**零贡献**，纯诊断。
//   ③ 演员图已改由 `GmHarvest`（`BattleElementCenter.gameObjectManager` 静态字段 → `_unitList`）
//      提供，实测 `GMHARV _unitList size=6..10 acc=N` 稳定命中，不再依赖 `ActorOfferArgs` 那条路。
//   需要重新取证（名册/兴趣/参数帧）时改回 true 即可，功能不受影响。
static const bool kDiag = false;
/* ★v47（2026-10-16）：`kStartupArm` **true → false**（防闪退兜底，用户报「打开游戏闪退」）。
 *
 *  v29 当年开它的**唯一理由**是抓住「开机/加载期那一批 `set_damage*` setter」——
 *  即 `K_DMGMUL`（配置表 setter 实参乘倍率）这条路。那条路**已在本文件 1437 行永久停用**
 *  （`kDmgMulHook=false`，三条血证：实测无效 / 复用实参缓冲 / 连乘失控），
 *  目标项 `set_damageFactor_A` 等也不再有任何消费方（见 §⑩a `kKinds` 的 v47 注）。
 *  ⇒ 开机即挂**收益为零**，剩下的只有风险：
 *    ① `DobbyHook` 是**原地改写正在执行的代码页**，而 `Mxytz_TryInit` 跑在游戏刚
 *       `domain/assembly` 就绪的窗口里 —— 解释器入口此刻正被主线程高频调用，
 *       改写与执行并发 ⇒ icache/预取拿到半条指令的设备就会 SIGILL/SIGSEGV。
 *       这解释了「**部分手机**打开游戏闪退」（同一份 so 在不同机型 GC/调度时序下表现不同）。
 *    ② 该窗口里连带跑的大件（`ResolveParaLayout` 全镜像遍历）已被记为 Boehm GC
 *       `Exclusion ranges overlap` abort 的嫌疑人（见 v44 清理注释），本轮一并挪出启动路径。
 *  现在改成**懒装**：功能打开后 `SyncHooks` 走 `CanInstallHooks()`（冷启 12s + 开功能 1s 双闸）
 *  才装钩。功能可用性不受影响（三条独立理由）：
 *    · `Hurt`/`DoAttack` 在战斗里被**持续**调用 ⇒ 学习期随时能把它们学进来；
 *    · `#4` 的四个广告目标由 `EnsureAdTargetsV43()` 按 `MethodInfo` **预登记**，不等调用；
 *    · `Mxytz_OnFeatureChanged` 每次开功能都会**重开学习期**（见该函数注释）。 */
static const bool kStartupArm = false;
// ★v26x 出包定案：**kDmgHooks = true**，但**只装 `Hurt5` 这一枚**（见 kDmgOnlyHurt）。
//   理由（全部有实测/元数据依据）：
//   ① 用户实测「秒杀小怪怪物直接死了，但什么奖励装备没给」+「全模式技能伤害增加没效果」+
//      「按照斌哥的方式去改那是很好的案例」。斌哥的机制 = `10_docs/冒险与征途/斌哥包抓点_20261002.md`
//      L241 `[冒险-打怪#%d] ctype=%d 合法=%d | 改前 m=%.4f e=%d → 改后 m=%.4f e=%d`
//      —— 他改的是**伤害数值的 BigInt（mantissa/exponent）**，让击杀归属玩家的真实伤害，
//      所以掉落/结算正常。我们写 HP=0 绕过了官方伤害管线 ⇒ 怪死了但无掉落。
//   ② 官方元数据已实证：`CBattleActorUtil` 0x02001F55 / `CtagHurtHPPara` 0x02001FC9 /
//      `StarsCollider` 0x02002032 / `DamageFactory` 0x02001FCD **全部落在 AOT 镜像
//      `ActionGameLibrary`（Assembly-CSharp.dll）**，不是热更 DLL ⇒ 它们的 `methodPointer`
//      是**真实的本地代码**，Dobby 内联 hook 打到的是 AOT 代码段（不是热更解释器页）。
//   ③ 伤害杠杆已被元数据钉死：`CBattleActorUtil::Hurt(ref CtagHurtHPPara, ref bool, bool, ref BigInt, bool)`
//      0x0601E25A 的 IL 首行就是 `CtagHurtHPPara.GetValue() <= 0 → return -1`，而
//      `GetValue()` 0x0601E55B 读的正是 `m_llValue` ⇒ 在 orig 之前放大 `m_llValue` 就是
//      「改前 m/e → 改后 m/e」，与斌哥逐字同构。`Hooked_Hurt5` 早已实现这件事，只是从没开闸。
//   ④ 崩溃保险已就位：`SaSetHpNow`（唯一的 runtime_invoke 崩溃向量）已整段删除；
//      `ScanSegvHandler` 的熔断已改成「与地址无关（tid+sig，阈值 3）」⇒ 万一 Dobby 在本环境
//      仍不可用，最坏结果是**一次干净自退**，不再是 v26v2 那种 100% CPU + 日志洪水的死循环。
/* ★v26y 定案：**kDmgHooks = false**。`CBattleActorUtil::Hurt` 是**解释方法**（HybridCLR 热更），
 *  它的 `methodPointer` = `libil2cpp 基址 + 0x207F04C` = 解释器**共享桩**（v7 早就记录过
 *  「4 个不同方法共享 ptr=0x40002527f04c」），v26x3 实测逐字复现：
 *  `PREFLIGHT Hurt5 mi=0x7d4301f61d30 fn=0x40002527f04c code=0 static=1`。
 *  ⇒ Dobby 内联 hook **物理上挂不上**（不是闸门判定问题）。而且「挂不上」这件事本身还会
 *  每 5 秒触发一次 `FindClassAnywhere` 全镜像遍历（~375 ms）—— 那正是掉帧源之一。
 *  真正的挂载点只有一个：**解释器入口 hook**（`InstallInterpHook`）→ `InterpDispatch`
 *  →（v27：按名自学习的指针表分发）`InterpFire` → `ApplyHurtPolicy` / `ApplyDoAttackPolicy`。
 *  与斌哥的壳同构（`斌哥包抓点_20261002.md`
 *  §5/§6：挂解释器入口 + 按「类::方法」名分发）。 */
static const bool kDmgHooks = false;
// v26x：只装伤害表的第 0 枚（`Hurt5`）。`DoAttack7`（`StarsCollider::DoAttack` 7 参）是
//   每次攻击都走的最热路径，且我们的无敌已由 `ActionState.immuneDamage@60` 内存写实证生效
//   ⇒ 不值得为它多冒一次 Dobby 风险。其余 4 枚是兜底/观测，一律不装。
static const bool kDmgOnlyHurt = true;
/* ★v26y 定案：**秒杀小怪不再写血量**（`kKillByHp = false`）。
 *  用户 m06433 实测：「秒杀小怪进入关卡怪物直接死了，但是什么奖励装备没给」。
 *  根因：关卡一加载我们就把所有小怪压到 1 血并（兜底）调 `Character::CheckDead` ⇒ 怪物
 *  **死亡时不携带玩家伤害** ⇒ 掉落/结算为空（热更名册里就有 `<>c::<IsMyRoleDamage>b__0` /
 *  `<>c::<SyncDieStateToServer>b__46_0` —— 服务端按「这一击是不是我的伤害」给奖励）。
 *  斌哥**从不碰血量**，他只改 `CtagHurtHPPara::m_llValue`（伤害 BigInt）：
 *  `[冒险-打怪#%d] ctype=%d 合法=%d | 改前 m=%.4f e=%d → 改后 m=%.4f e=%d`
 *  ⇒ 玩家自己那一击就是**真·击杀**，掉落/结算走官方链路。
 *  ⇒ 秒杀小怪改为「玩家伤害 ×100（`kKillMul`）+ 不设指数下限」（见 `ApplyHurtPolicy`），血量路线整块关掉。
 *    ★v27 改量级：原 `kKillMul=1000` + `ne<12 兜底`（1e12）超过 `skill/re-implement/anti-ban-analysis.md`
 *    §三「秒杀 安全线 = ×100 CAP」，已按安全线收回到 ×100 并删掉指数下限。
 *  需要回退取证（例如确认掉落仍为空）时把这里改回 true 即可 —— 代码原样保留在下面。 */
static const bool kKillByHp = false;
static const bool kDataWrites = false;
/* ★v34 残留清理（全面自查结论之一）：`ApplyDmgMulArg`（v28~v33 的「#3 走配置表 setter 实参乘倍率」）
 *  **整段停用**。三条血证：
 *  ① 实测无效：`DMGARG=107` 全跑过、写入也成功，但四个伤害与技能表现毫无变化（用户实测 3 轮）；
 *  ② 写的是**复用实参缓冲**：v32 日志里 `DMGARG#1..6` 的 `slot` 全是同一个地址 `0x7d4413ff5198`，
 *     且 `#5` 读到的 `old=40.0` 正是 `#4` 自己写进去的值 ⇒ 这个缓冲会被下一次调用复用，
 *     我们的乘值既不会持久化、也**会污染同一缓冲里的其它实参**；
 *  ③ 危险残留：`#9/#10` 出现过 `old=100 → 10000 → 1000000` 这种**连乘**，
 *     一旦这些垃圾被游戏当作伤害系数/常量读走，就会出现用户报的「突然冒出 90 多万伤害」。
 *  ⇒ 保留函数体（便于日后取证）但**默认永不执行**。 */
static const bool kDmgMulHook = false;
/* v44 清理：删除两个永久关闭的「大件诊断」开关及其全部实现。
 *  - `kProbeDump`（旧名）：`InstallProbeDump()`→`ProbeWatchInit()` 里 12 个目标各走一次
 *    `ResolveClassPref`→`FindClassAnywhere`，后者每次全量遍历所有 image 的全部 class（13755 个）
 *    ⇒ 实测 step4→step5 卡 **15.5 s**，正是用户看到的「获取角色数据中卡一次」。
 *    且大范围 image/class 遍历会触发热更元数据懒加载，疑似 v26e 那次 Boehm GC
 *    `GC_exclude_static_roots_inner` → `abort("Exclusion ranges overlap")`。
 *  - `kDataPlane`（旧名）：v20 老数据面，实测恒为 `APPLY … ok=0 bad=N`、`CALIB=0`（从未命中），
 *    却每 2s 做一次 787MB 裸扫（`scan done regions=1041 bytes=787MB`）。
 *  两者职责都已由**解释器入口 hook + 演员图**（`ActorApplyTick`）取代，正式版一行不留。 */

/** Dobby 内联 Hook。返回 true = Dobby 已装上；false = 未装（调用方走 MI 槽兜底）。 */
static bool tryHook(void* target, void* stub, void** orig, void** at, const char* name) {
    if (!target) return false;
    if (*at == target && *orig) return true;
    errno = 0;
    int rc = DobbyHook(target, stub, orig);
    if (rc != 0) {
        if (*orig) return true;  // 半成功：orig 已填，原函数可调，视为可用
        DBGLOG(TPL_LOG " Dobby %s rc=%d errno=%d @%p", name, rc, errno, target);
        return false;
    }
    *at = target;
    DBGLOG(TPL_LOG " hooked %s @%p", name, target);
    return true;
}

/** Dobby 在本环境是否可用的判定实验：hook 我们自己 so 里的函数（不碰游戏代码）。 */
extern "C" int Mxyzt_SelfTest(int x) { return x + 1; }
static int (*g_selfOrig)(int) = nullptr;
static int Hooked_SelfTest(int x) { return g_selfOrig ? g_selfOrig(x) + 100 : -1; }

/** 抓点地址取证：同一地址被多个方法共用 = 共享入口（禁止 Dobby），必须走 MI 槽。
 *  v5 实测：HybridCLR 解释执行的方法其 methodPointer 可能是「标记值」（本轮 = 0x400000000000，
 *  不在任何映射内）⇒ 直接解引用会致命；模拟器上还会因信号链回退变成 SIGSEGV 风暴/ANR。
 *  故此处一切读取先过 maps 判定，并顺带把 mi 前 4 槽打出来（解释方法的真桥可能在别处）。 */
static uint32_t g_mpSeen[12];
static void* g_mpAddr[12];
static int g_mpN = 0;
static void LogTargetAddr(const char* tag, void* mi) {
    void* fn = MethodPointer(mi);
    int shared = -1;
    for (int i = 0; i < g_mpN; i++)
        if (g_mpAddr[i] == fn) shared = i;
    if (shared < 0 && g_mpN < 12) {
        g_mpAddr[g_mpN] = fn;
        g_mpSeen[g_mpN] = 0;
        shared = g_mpN++;
    }
    if (shared >= 0) g_mpSeen[shared]++;
    uint32_t w0 = 0, w1 = 0, w2 = 0, w3 = 0;
    const int rd = (fn && Il2Cpp::IsReadableRange((uintptr_t)fn, 16)) ? 1 : 0;
    const int ex = (fn && Il2Cpp::IsExecutableRange((uintptr_t)fn, 16)) ? 1 : 0;
    if (rd) {
        w0 = Il2Cpp::ReadI32((void*)fn, 0);
        w1 = Il2Cpp::ReadI32((void*)fn, 4);
        w2 = Il2Cpp::ReadI32((void*)fn, 8);
        w3 = Il2Cpp::ReadI32((void*)fn, 12);
    }
    void* c[4] = {nullptr, nullptr, nullptr, nullptr};
    char cp[4] = {'-', '-', '-', '-'};
    if (mi && Il2Cpp::IsReadableRange((uintptr_t)mi, 40)) {
        for (int i = 0; i < 4; i++) {
            c[i] = Il2Cpp::ReadPtr(mi, (size_t)i * 8);
            cp[i] = (c[i] && Il2Cpp::IsExecutableRange((uintptr_t)c[i], 16))
                        ? 'x'
                        : ((c[i] && Il2Cpp::IsReadableRange((uintptr_t)c[i], 8)) ? 'r' : '-');
        }
    }
    DBGLOG(TPL_LOG " MP %s=%p orig=%p same[%d] rd=%d ex=%d w=%08x %08x %08x %08x mi=[%p%c %p%c %p%c %p%c]",
         tag, fn, mi, shared, rd, ex, w0, w1, w2, w3, c[0], cp[0], c[1], cp[1], c[2], cp[2], c[3], cp[3]);
}

/** 优先在游戏命名空间里找类（同名短名可能撞车），再退回全局短名扫描。 */
static void* ResolveClassPref(const char* cls) {
    static const char* kNss[] = {"ActionGameLibrary", "ActionData", "GameConfigData"};
    for (size_t i = 0; i < sizeof(kNss) / sizeof(kNss[0]); i++) {
        void* k = FindClassNs(kNss[i], cls);
        if (k) return k;
    }
    return FindClassAnywhere(cls);
}

static void* ResolveMethod(const char* cls, const char* method, int argc, int nth) {
    void* k = ResolveClassPref(cls);
    if (!k) {
        DBGLOG(TPL_LOG " class miss %s", cls);
        return nullptr;
    }
    auto& api = GetApi();
    void* mi = nullptr;
    if (nth > 0) mi = FindMethodNth(k, method, nth);
    if (!mi) mi = api.GetMethod(k, method, argc);
    if (!mi) {
        DBGLOG(TPL_LOG " method miss %s.%s argc=%d", cls, method, argc);
        return nullptr;
    }
    return mi;
}

/** 解析游戏自身的玩家判定 GameObjectManager::IsPlayer(object, bool)（仅静态方法参与）。 */
static void ResolvePlayerJudge() {
    static bool s_done = false;
    if (s_done) return;
    s_done = true;
    auto& api = GetApi();
    void* mi = ResolveMethod("GameObjectManager", "IsPlayer", 2, 0);
    if (!mi) return;
    const bool st = api.method_flags ? ((api.method_flags(mi, nullptr) & 0x10u) != 0) : false;
    if (!st) {
        DBGLOG(TPL_LOG " IsPlayer is instance — 不做判定依据");
        return;
    }
    g_isPlayerMP = MethodPointer(mi);
    DBGLOG(TPL_LOG " IsPlayer bridge=%p", g_isPlayerMP);
}

static bool g_installedDmg = false;
static bool g_installedAd = false;
static std::atomic<uint32_t> g_dmgFail{0};

/** v26x2：**预检** `Hurt5` 的钩子目标 —— 只解析 + 打印（mi / methodPointer / 是否可执行 /
 *  静态标志），**不装钩**。用途：在不点菜单（功能全关）的情况下就能从日志确认
 *  「AOT 类已加载 + 目标有本地代码 + 首参是不是 this」，避免用户白花一次体力。
 *  日志：`PREFLIGHT Hurt5 mi=… fn=… code=… static=…`。 */
static void PreflightDmgTarget() {
    auto& api = GetApi();
    void* mi = ResolveMethod(kDmgSlots[0].cls, kDmgSlots[0].method, kDmgSlots[0].argc, kDmgSlots[0].nth);
    void* fn = mi ? MethodPointer(mi) : nullptr;
    const bool code = fn && Il2Cpp::IsReadableRange((uintptr_t)fn, 16) &&
                      Il2Cpp::IsExecutableRange((uintptr_t)fn, 16);
    const bool st = (api.method_flags && mi) ? ((api.method_flags(mi, nullptr) & 0x10u) != 0) : false;
    DBGLOG(TPL_LOG " PREFLIGHT Hurt5 mi=%p fn=%p code=%d static=%d", mi, fn, code ? 1 : 0, st ? 1 : 0);
}

static void InstallDmgHooks() {
    /* v12：装钩路径只做「解析方法 + 登记观察」，不再跑任何诊断
     *（Diagnose 里 type_name/free + 热更类字段迭代曾把进程打成 SIGABRT）。 */
    /* v26x：★节流。装钩失败时 `SyncHooks` 会每拍重试，而本函数内部
     *  `FindClassAnywhere` 是**全镜像遍历**（实测 ~375 ms/次）⇒ 不节流就又变成掉帧。
     *  最多 5 秒试一次。 */
    static uint64_t s_tryMs = 0;
    const uint64_t nowTry = NowMs();
    if (s_tryMs && nowTry - s_tryMs < 5000ull) return;
    s_tryMs = nowTry;
    ResolvePlayerJudge();
    /* v26x：★**必须在这里补一次** `ResolveParaLayout()`。开机期（`Mxytz_TryInit` 的 step3）
     *  调它时 AOT 镜像 `Assembly-CSharp`（含 `CtagHurtHPPara`/`BigInt`）**还没被加载**——
     *  v26x 实测那一轮 `domain_assemblies` 只枚举到 13 个框架程序集（BouncyCastle、mscorlib、
     *  UnityEngine 系列、Sentry 系列…），于是 `FindClassAnywhere("CtagHurtHPPara")` 返回 null、
     *  日志 `pl2 para=0x0 find BigInt` / `pl3 bigint=0x0` /
     *  `para layout target=+0x0 attacker=+0x0 value=+0x0 … ready=0` ⇒ `Hooked_Hurt5` 只能
     *  空转（`HURT para unreadable`）。本函数在「用户开功能 + 进程起 12s」之后才被调用，
     *  那时游戏早已 `MainInit:InitStep1()`，类一定在。 */
    if (!g_paraReady.load()) ResolveParaLayout();
    for (size_t i = 0; i < kDmgSlotN; i++) {
        if (kDmgOnlyHurt && i != 0) continue;  // v26x：只装 Hurt5（见 kDmgOnlyHurt 处说明）
        HookSlot& s = kDmgSlots[i];
        if (*s.origPtr) continue;
        void* mi = ResolveMethod(s.cls, s.method, s.argc, s.nth);
        if (!mi) continue;
        LogTargetAddr(s.tag, mi);
        /* v26x：装钩前**先定案「首参是不是 this」**（`method_flags & 0x10`）。
         *  `Hooked_Hurt5` 靠 `g_hurtStatic` 决定解引用 a0 还是 a1 —— 猜错就等于把
         *  某个整数当对象指针解引用（`OkPtr` 能挡掉大部分，但语义会整体错位）。
         *  以前只有另一条诊断路径会填这个标志，装钩路径从不填 ⇒ 一直是默认值。 */
        NoteStatic(s.tag, mi);
        void* fn = MethodPointer(mi);
        /* v5：只对「可读且可执行」的目标做 inline hook；解释方法的标记值一律跳过（否则
         * 故障会以信号形式打到主线程 → 风暴/ANR），改走 MI 槽。 */
        const bool code = fn && Il2Cpp::IsReadableRange((uintptr_t)fn, 16) &&
                          Il2Cpp::IsExecutableRange((uintptr_t)fn, 16);
        if (fn && !code)
            DBGLOG(TPL_LOG " %s methodPointer 非可执行码（解释方法标记值）→ 只登记观察，不写 MI 槽", s.tag);
        DBGLOG(TPL_LOG " dmgslot %s mi=%p fn=%p code=%d", s.tag, mi, fn, code ? 1 : 0);
        const bool db = code ? tryHook(fn, s.hookFn, s.origPtr, &g_hookAt[s.index], s.tag) : false;
        if (!db) {
            RegisterWatch(s.tag, mi);
            if (s.required) DBGLOG(TPL_LOG " %s 暂不可挂（无本地代码）→ 已登记观察", s.tag);
        }
    }
}

/** HybridCLR 懒桩漂移：方法真正编译后 methodPointer 换址 → 重挂一次（每槽限一次）。 */
static void RebindIfMpMoved() {
    for (size_t i = 0; i < kDmgSlotN; i++) {
        HookSlot& s = kDmgSlots[i];
        if (!*s.origPtr || g_rebindUsed[s.index]) continue;
        void* mi = ResolveMethod(s.cls, s.method, s.argc, s.nth);
        void* cur = MethodPointer(mi);
        if (!cur || cur == s.hookFn || cur == g_hookAt[s.index]) continue;
        /* v5：methodPointer 若换成了不可执行标记值，绝不把它丢给 Dobby（会读/写非法页） */
        if (!Il2Cpp::IsReadableRange((uintptr_t)cur, 16) ||
            !Il2Cpp::IsExecutableRange((uintptr_t)cur, 16)) {
            g_rebindUsed[s.index] = 1;
            DBGLOG(TPL_LOG " rebind %s 跳过：%p 非可执行码", s.tag, cur);
            continue;
        }
        if (DobbyHook(cur, s.hookFn, &g_origDmgRebind) == 0) {
            g_rebindUsed[s.index] = 1;
            g_hookAt[s.index] = cur;
            DBGLOG(TPL_LOG " rebind %s @%p", s.tag, cur);
        }
    }
}

// ============================================================
// ⑩ HybridCLR 解释器入口 hook（与斌哥包同构：挂入口 + 按「类::方法」名分发）
// ------------------------------------------------------------
// 依据 `10_docs/冒险与征途/斌哥包抓点_20261002.md` §5/§6：
//   热更 DLL 里的方法是**解释执行**的，AOT 侧只有「解释器桩」；实测这些方法的
//   methodPointer 全部指向解释器内部的**同一个地址**（v7 实测 4 个不同方法共享
//   ptr=0x40002527f04c = libil2cpp 基址 + RVA 0x207F04C）⇒ 逐方法 Dobby 既无意义
//   也不稳定。斌哥包实测可行的唯一路线：DobbyHook 解释器入口（RVA 0x207F798），
//   在其中按 MethodInfo 的 名字/类名 分发到我们的功能实现。
//   注意：本 hook 必须永远**透传原函数**（不漏调用），且入口是热路径 —— 内部只做
//   「读 mi 名字 + 与小型名字表比对」，未命中立即透传。
// ============================================================
static const uintptr_t kInterpEntryRva = 0x207F798;  // libil2cpp: HybridCLR 解释器入口
static void* g_interpOrig = nullptr;
static std::atomic<bool> g_interpHooked{false};
static std::atomic<uint32_t> g_interpCalls{0};  // v27：**慢路径**调用计数（不再是「每次调用」都自增）

/* ------------------------------------------------------------
 * libil2cpp.so 加载基址（v45 修复：真机上同名双映射必须选「真身」）
 * ------------------------------------------------------------
 * ★ 2026-10-04 真机 (OPD2404 / ColorOS / arm64, com.mxyzt.gzmy 1.5.9) 血证：
 *   `grep libil2cpp /proc/<pid>/maps` 出现**两份同 inode（fe:48 405147）映射**：
 *     影子 A: 0x73268f3000  r--p off 0x0 + 两处孤立 r-xp 单页；**没有任何 rw-p 段**
 *     真身 B: 0x7432c17000  r--p off 0x0 → r-xp off 0x1d73000 → r--p → **rw-p**
 *   旧实现取「off==0 且地址最小」⇒ 恒定命中影子 A。在那里 Dobby 挂点照样 rc=0，
 *   但 A 是**从不执行的副本**（游戏实际跑 B，B 尾部紧跟 [anon:.bss]）⇒ 解释器入口
 *   hook 20 分钟 0 命中：`[mxyzt] interp learn 进行中 slow=0 已登记=0/14` +
 *   `STATS ... 命中 打怪=0 无敌=0 技能=0 免广告=0` ⇒ 四项功能全体失效；免广告
 *   开开关后还「像断网一样卡住」。
 *   对照 Frida 17.16.4 权威读数（`Process.enumerateModules()`）：
 *     `MOD libil2cpp.so base=0x7432c17000 size=89321472` = **真身 B**，
 *   影子 A 根本不在 linker 模块表里。
 *   win/MuMu 上只有单份映射 ⇒ 「win 做完在模拟器测都有用」完全自洽。
 *
 * ★ v45 判据（为什么是「同一映像组里出现 rw-p」而不是别的）：
 *   真身必须由 linker 映射出**可写数据段**（全局变量/重定位/BSS，本包 rw-p ≈ 2780KB）；
 *   影子副本是给扫描/反调用用的整段只读映像，**一条 rw-p 都没有**（实测 A 只有
 *   `73288f3000-732dbef000 r--p 00000000` 一行、覆盖全文件）。
 *   注意 A 与 B 的 `dev:inode` **完全相同**（fe:48 594484），所以不能用 inode 区分；
 *   也不能取 rw-p 那段的**起点**当基址 —— rw-p 段是偏移 0x5045000 的第三个 LOAD，
 *   起点不是模块基址（v45 初版就栽在这：base=0x7437700000 导致 entry 落到模块外、
 *   rd=0、hook 写飞）。正确做法：按「同 dev:inode 且地址首尾相接」切出每份映像组，
 *   在**含 rw-p 的那一组里取 off==0 那行的起点**才是模块基址。
 *
 * ⚠️ 不要改成 dl_iterate_phdr / xdl_info / dlsym 拿 dli_fbase 来「顺便修好」：
 *   InstallInterpHook 跑在 mod 启动早期（此时游戏侧 libil2cpp 仍在热更/加载中），
 *   进 linker 会抢 g_dl_mutex —— 有死锁风险，而 /proc/self/maps 解析是无锁的。
 */
static uintptr_t LibBaseFromMaps(const char* needle) {
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) return 0;
    const size_t nlen = strlen(needle);
    char line[512];
    uintptr_t best = 0;       // 含 rw-p 的那份映像里 off==0 的起点 = 模块基址
    size_t bestWr = 0;        // 该组 rw-p 总字节数（多组时取最大者，更稳）
    uintptr_t firstOff0 = 0;  // 兜底：第一份 off==0 映射

    /* 当前「映像组」（同一份 so 的全部连续映射）累积状态 */
    char gDev[24] = {0}, gIno[24] = {0};
    uintptr_t gEnd = 0, gOff0 = 0, gFirst = 0;
    size_t gWr = 0;
    bool gHasWr = false, gOpen = false;

    auto flushGroup = [&]() {
        if (gOpen && gHasWr && gWr > bestWr) {
            best = gOff0 ? gOff0 : gFirst;
            bestWr = gWr;
        }
        gOpen = false; gHasWr = false; gWr = 0; gOff0 = 0; gFirst = 0; gEnd = 0;
        gDev[0] = '\0'; gIno[0] = '\0';
    };

    while (fgets(line, sizeof(line), f)) {
        char* p = strstr(line, needle);
        if (!p) continue;
        if (p[nlen] != '\n' && p[nlen] != '\0') continue;  // 后面还有字符 → 非同名 so
        unsigned long s = 0, e = 0, off = 0;
        char perms[8] = {0}, dev[24] = {0}, ino[24] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx %23s %23s", &s, &e, perms, &off, dev, ino) != 6) continue;
        if (off == 0 && firstOff0 == 0) firstOff0 = (uintptr_t)s;

        // 同 dev:inode 且地址首尾相接 ⇒ 仍属同一份映像；否则开新组
        const bool sameGroup = gOpen && strcmp(dev, gDev) == 0 && strcmp(ino, gIno) == 0 &&
                               (uintptr_t)s == gEnd;
        if (!sameGroup) {
            flushGroup();
            gOpen = true;
            gFirst = (uintptr_t)s;
            snprintf(gDev, sizeof(gDev), "%s", dev);
            snprintf(gIno, sizeof(gIno), "%s", ino);
        }
        gEnd = (uintptr_t)e;
        if (off == 0 && gOff0 == 0) gOff0 = (uintptr_t)s;
        if (perms[0] == 'r' && perms[1] == 'w') { gHasWr = true; gWr += (size_t)(e - s); }
    }
    flushGroup();
    fclose(f);
    if (best) return best;
    return firstOff0;  // 理论上到不了：真身必然有 rw-p
}

/** libil2cpp.so 基址（进程内缓存，只解析一次）。 */
static uintptr_t LibBaseResolve() {
    static std::atomic<uintptr_t> s_base{0};
    uintptr_t b = s_base.load(std::memory_order_relaxed);
    if (b) return b;
    b = LibBaseFromMaps("libil2cpp.so");
    if (b) s_base.store(b, std::memory_order_relaxed);
    return b;
}

/* v45 取证：把该 so 的**每一份**映射逐行打到 logcat，用来一眼分辨「真身/影子」。
 * 只在 InstallInterpHook 里打一次，不在热路径。判真身看该行有没有 rw-p。 */
static void LogLibMaps(const char* needle) {
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) return;
    const size_t nlen = strlen(needle);
    char line[512];
    int i = 0;
    while (fgets(line, sizeof(line), f)) {
        char* p = strstr(line, needle);
        if (!p) continue;
        if (p[nlen] != '\n' && p[nlen] != '\0') continue;
        line[strcspn(line, "\n")] = '\0';
        LOGI(TPL_LOG " maps[%d] %s", i++, line);
    }
    fclose(f);
}

typedef void* (*InterpFn)(void*, void*, void*);

// ============================================================
// ⑩a 解释器入口分发（★v27 定案：**按「类::方法」名自学习 → 之后纯指针比较**）
// ------------------------------------------------------------
//  推倒 v11~v26 那套的三条实证（勿重蹈）：
//   ① 旧版 `kInterpTargets[*].mi` 由 il2cpp API（`class_methods` / `GetMethod`）解析，再在入口
//      按 `mi == a0` 比较 —— 但**热更方法的 MethodInfo 对象与解释器实际传入的不是同一个**
//      （v13 注释已记录血证；v26y2 实测 `interp hooks armed` 后 `hits=0/0/0`、无任何 ITARGET 行）
//      ⇒ 按 mi 比较的分发**恒不命中**，四项功能等于从没挂上过。
//   ② 斌哥壳的做法就是**按「类::方法」名分发**（`10_docs/冒险与征途/斌哥包抓点_20261002.md`
//      §5/§6），而且他的名字在运行期才解密到匿名段（dex/so/metadata 全 0 命中）
//      ⇒ 他也不可能靠 API 反查。我们把唯一可行的键降级成了兜底，这就是根因。
//   ③ 入口 hook 是**每次热更方法调用都过一遍**的 Dobby trampoline（一场战斗 ~1.5e7 次）⇒
//      热路径只允许「1 次读 + 1~2 次指针比较」：禁止 maps 查询 / 遍历字符串 / 除法 / 日志 / 碰 a1。
//      （v26v 定案：原来热路径上的 36 次原子读 + `%20000` 除法 + 名字哈希，就是用户说的「好掉帧」。）
//
//  机制（名字指针自学习）：
//   · 热路径：读 `mi+0x18`（该方法的名字串指针；同一方法在运行期只有一份名字串，
//     **跨 MethodInfo 实例共享**），查 64 槽开放寻址表 → 命中即按功能位分发；空槽 = 未命中，透传。
//   · 慢路径（只在表未命中**且学习期开**时）：把名字串与 `kKinds` 的目标**逐个明文比对**，
//     命中后**核对类名**（`klass->name`，一次指针读，廉价）→ 登记进表 ⇒ 之后全部走热路径。
//   · 学习期 = 「还有目标没登记」；全部登记完 / 慢路径预算耗尽 / Tick 窗口到期 → 关，
//     此后退化为纯指针表查（热路径不再有任何字符串工作）。
// ============================================================
enum : uint16_t {
    K_HURT = 1u << 0,      // CBattleActorUtil::Hurt         → #1 秒杀 / #3 技能伤害 / #2 无敌(清零)
    K_DOATTACK = 1u << 1,  // StarsCollider::DoAttack        → #2 无敌（accepter 置 null = 官方作废分支）
    K_DAMAGE = 1u << 2,    // Character::Damage              → #2 无敌第二层（本版只取证，不写）
    K_LOCALDMG = 1u << 3,  // BattleElementCenter::get_localDamage → #3 客户端权威（本版只取证）
    K_PLAYAD = 1u << 4,    // MiaoYueSDKProxy::PlayADVideo   → #4（本版只取证）
    K_ADRESULT = 1u << 5,  // DS_Advertising::OnADVideoResult → #4 强制 success=true
    K_ADREWARD = 1u << 6,  // DS_Advertising::DispatchResult → #4 强制 ok=true（发奖唯一信号）
    K_CANCLAIM = 1u << 7,  // AdMascotManager::CanClaimReward → #4（本版只取证）
    /* ★v28 正路：**伤害公式参数 setter**。普查（653 对/218 类）实测它们**走 AOT 入口
     *  `0x207F798` 可达** —— AOT 侧配置表反序列化在往热更 Data 对象里塞字段。
     *  `GameFormula::GetResult(…, damageConst, factorA, factorB)` 的参数就来自这些字段
     *  （`06_dumped/冒险与征途/aot/_tgt_il_df4.txt:100-229`）⇒ 在 setter 的**实参槽**上乘倍率
     *  = 全模式伤害放大，且**完全不碰战斗逻辑方法**。 */
    K_DMGMUL = 1u << 8,
    /* ★v34：**#3「全模式技能伤害增加」的真落点** —— `ActionCommand_AddProjectile::Execute`。
     *  证据链：斌哥壳内存里解出的中文日志串含 `★count %d→%d★`、`activeLimit %d→%d`、
     *  `emitByTargetCount`（见 `06_dumped/冒险与征途/mem_sdk2/` 的 anon*.bin 串表），
     *  与 `ActionData.LAddProjectile` 的字段名一一对应（`_count 0x040169AB`、
     *  `_activeCountLimit 0x040169AC`、`_activeCountLimitWithTag 0x040169AD`、
     *  `_emitByTargetCount 0x040169AF`）；实例经 `ActionCommandData::data`
     *  （FieldDef `0x0401793D`）+ `isinst LAddProjectile` 取得（IL 见
     *  `ActionCommand_AddProjectile::Execute 0x0601EB99`）。
     *  ⇒ 提高投射物「数量/同时在场上限/每目标数量」= 同一技能打出更多段 =
     *    **各模式都变强**（这正是用户说的「适配其他模式」，而不是秒杀小怪那种一击秒）。 */
    K_PROJ = 1u << 9,
    /* ★v39：`_count` 的**真落点**。铁证（`_tgt_q.py callers "LAddProjectile" "get_count"`）：
     *  全工程**唯一**读 `_count`（FieldDef `0x040169AB`）的方法是
     *  `ActionCommand_AddProjectile::Execute(ActionCommandData) -> object 0x0601EB99`：
     *    `IL_011B callvirt get_count()` → `IL_0120 stloc.s 4`，loc4 就是 17 参
     *    `AddProjectile`（`0x0601EB9A`）的**循环上界**（循环体唯一调用点 `IL_02E1 call 0x0601EB9A`），
     *    即 `n = count + Action::projectileCount(0x04016F67) + 1` 发。
     *  而 17 参方法体内**没有任何 `ldfld 0x040169AB`** ⇒ 它不读 `_count`（v36/v37 写 `data._count`
     *  写了 14 次而游戏毫无变化，就是这么来的）。
     *  ⇒ 必须在 **Execute 帧、它读 count 之前**改 `data._count`（`ActionCommandData::data`
     *    FieldDef `0x0401793D` → `isinst LAddProjectile`）。
     *  另外：17 参方法的第 4 个实参经 `IL_0106 ldarg.s 4 / switch(get_direction())` 只喂给
     *  `ProjectileFactory::CreatProjectile(...) 0x0601EB3A` ⇒ 那是**方向值**，v37/v38 写它是错的（已撤）。 */
    K_PROJEXEC = 1u << 10,
};

struct KindDef {
    const char* cls;     // 明文类名（hook-rules §24：禁短混淆标识符）
    const char* method;  // 明文方法名
    uint16_t kind;
    const char* tag;
};

static const KindDef kKinds[] = {
    /* ★★v47：**删除 v28 的 4 个 `set_damage*` 目标**（`K_DMGMUL`）。
     *  它们不是「暂时不用」，而是**必须删**——留着会直接造成用户报的「功能有时候会失效」：
     *   ① 消费方早已永久停用：`kDmgMulHook=false`（1437 行，三条血证：实测无效 / 复用实参缓冲 /
     *      连乘失控 ⇒「突然 90 多万伤害」）。即这 4 项目标**登记了也没有任何行为**；
     *   ② 它们是 `cls==""`（按名匹配任意类）⇒ `InterpLearn` **永远不给它们置 `g_kindDone`**
     *      （v28 的原意是「别让第一个类把整族关掉」），于是 `LearnAllDone()` **恒为假**、
     *      学习期永远只有「窗口到期」这一条关闭路径；
     *   ③ 更要命的是它们会**持续占表**：每个 (名字, 类) 组合各登记一个槽，而 `kLearnSlots`
     *      当时只有 **64** —— MuMu 普查该族就有「653 对/218 类」⇒ 表会被这族**填满**，
     *      `LearnRegister` 的 8 次探测全部失败后 `LEARN 表满`，后面真正的战斗目标
     *      （`Hurt`/`DoAttack`/`ProjExec`）**再也登记不进去** ⇒ 功能静默失效。
     *      （`kLearnSlots` 本轮同时提到 256，双重消除这条饱和路径，见该常量处注释。）
     *  留档：若将来要重启 `K_DMGMUL`，先把 `kDmgMulHook` 改 true，再把这 4 行加回来。 */
    /* ============ 兜底：3 分 50 秒普查里**从未出现**（= 纯托管→托管，见 §⑩b 挂点说明）============
     *  留着不为命中，只为「万一别的场景会过入口」时能立刻看到 `LEARN ok`；
     *  真正的覆盖靠 §⑩b 的**帧构造二级挂点**（`0x207DCCC`，x1 = MethodInfo*）。 */
    {"CBattleActorUtil", "Hurt", K_HURT, "Hurt5"},
    {"StarsCollider", "DoAttack", K_DOATTACK, "DoAttack7"},
    {"Character", "Damage", K_DAMAGE, "Damage"},
    {"BattleElementCenter", "get_localDamage", K_LOCALDMG, "LocalDmg"},
    {"MiaoYueSDKProxy", "PlayADVideo", K_PLAYAD, "PlayAD2"},
    {"DS_Advertising", "OnADVideoResult", K_ADRESULT, "OnResult"},
    {"DS_Advertising", "DispatchResult", K_ADREWARD, "Dispatch"},
    {"AdMascotManager", "CanClaimReward", K_CANCLAIM, "CanClaim"},
    /* ★v34：#3 真落点（投射物模块）。**照斌哥**：他的名字池注册顺序是
     *  `Damage→Character→DoAttack→StarsCollider→AddProjectile→AddProjectile→ShowWin→MW_GameNotice`，
     *  其中第二个 `AddProjectile` = `ActionCommand_AddProjectile::AddProjectile(Character, ActionGameObject,
     *  ActionData.LAddProjectile, int, Vector3, Vector3, …)`（`0x0601EB9A`，17 参）——
     *  只有它拿得到 `LAddProjectile` 数据对象（要改的 `_count`/`_activeCountLimit` 就在里面）。
     *  文档 §6「菜单 id ↔ 壳模块」表也把「投射物（顶层/套娃层）」归到 **id 2 同组（增伤/放技能）**，
     *  与用户说的「适配其他模式」、斌哥菜单备注「需要配合所有技能／持续帮放技能增伤」完全一致。 */
    {"ActionCommand_AddProjectile", "AddProjectile", K_PROJ, "AddProj"},
    /* ★v39：`_count` 的真落点 = **同一个类的 `Execute`**（见 enum 处铁证）。
     *  类名必须一起比（旧教训：挂 `Execute` 会被 `EventDelegate::Execute` 这类同名方法混进来，
     *  学习期日志 `LEARN 拒 ActionCommand_AddProjectile::Execute ← 实际 EventDelegate::Execute`
     *  就是这条判据在起作用）。 */
    {"ActionCommand_AddProjectile", "Execute", K_PROJEXEC, "ProjExec"},
};
static const size_t kKindN = sizeof(kKinds) / sizeof(kKinds[0]);

struct LearnSlot {
    const char* name;  // MethodInfo+0x18（方法名字符串指针）
    void* klass;       // MethodInfo+0x20（**必须**：方法名字符串在 il2cpp 里是共享池，
                       //   只比名字指针会把「别的类里的同名方法」混成同一个目标）
    uint16_t kinds;    // 命中的功能位（同名重载合并）
    uint8_t argBase;   // 1 = 实例方法（a1[0] = this）；0 = 静态（由 method_flags & 0x10 定案）
    uint32_t hits;     // 该目标的命中次数（hook-rules §25：独立计数，装配自检用）
};
static const uint32_t kLearnSlots = 256;  // 2 的幂（★v47：64 → 256，见下方 kLearnShift 说明）
/* ★v47：哈希位移必须与槽数**联动**（旧代码把 `>> 26` 硬编码在 `LearnRegister` 与
 *  `InterpDispatch` **两处**，改槽数不改位移 = 一半以上槽永远用不到 + 探测窗口错位）。
 *  这里抽成一个常量 + 静态断言，改槽数时只会在这里改一次。
 *  `h = (name >> 4) * 2654435761u >> kLearnShift` ∈ [0, kLearnSlots)。 */
static const uint32_t kLearnShift = 24;  // 32 - log2(256)
static_assert((1u << (32 - kLearnShift)) == kLearnSlots, "kLearnShift 必须等于 32 - log2(kLearnSlots)");
static inline uint32_t LearnHash(uintptr_t namePtr) {
    return ((uint32_t)(namePtr >> 4) * 2654435761u) >> kLearnShift;
}
static const uint32_t kLearnProbeMax = 16;  // 开放寻址探测上限（★v47：8 → 16，抗聚集）
static LearnSlot g_lslot[kLearnSlots] = {};
static std::atomic<uint32_t> g_learnActive{0};  // 0 = 学习期已关（热路径只剩纯指针表查）
static std::atomic<uint32_t> g_learnMs{0};      // 学习窗口起点（Tick 写）
static uint8_t g_kindDone[kKindN] = {};         // 1 = 已登记 / 2 = 类名核对拒绝（只作跳过提示）
static const char* g_rejectNm[16] = {};         // 类名核对失败的名字指针（避免每次重复核对）
static uintptr_t g_rejectKls[16] = {};          // ★v40：**类指针也进键**（见 InterpLearn 的 v40 注释）
static uint32_t g_rejectN = 0;
static const uint32_t kLearnBudget = 64000000u;  // 慢路径总预算（累计开销上限 ~0.3s）
static const uint32_t kLearnWindowMs = 600000u;  // 学习窗口 10 分钟（Tick 到期即关）
/* ★v27：目标方法**首字节位图**（256 位）。慢路径进 `InterpLearn` 之前先过这一道 ——
 *  战斗里绝大多数热更方法的名字都不在目标表里，靠它把无关调用挡在慢路径之外。
 *  位图在**动态初始化期**从 `kKinds[]` 建好（同 TU 内 `kKinds` 是常量初始化，先于本变量），
 *  运行期只读、不加锁、无原子操作。 */
static uint8_t g_firstMask[32] = {};
static const bool g_firstMaskInit = []() {
    for (size_t i = 0; i < kKindN; i++) {
        const unsigned char c = (unsigned char)kKinds[i].method[0];
        if (c) g_firstMask[c >> 3] |= (uint8_t)(1u << (c & 7));
    }
    return true;
}();

// ---- v14/v15：入口全名册 + 关键词兴趣探针 + 嵌套检测 ----
//  名册目的：确认解释器入口到底服务哪些方法，尤其「战斗伤害类方法是否经过入口」。
//  每调用成本 = 1 次名字指针哈希 + ≤4 次指针比较；只有**新名字**才做字符串调用与落日志。
//  两条独立限量：NAME 名册 3000 条（参考）、INTEREST 200 条（关键词命中即取证参数帧，不受名册上限影响）。
//  嵌套检测：若热更方法内部调用另一个热更方法也走入口，则深 >0 会出现 —— 这决定了后续能不能只靠
//  入口 hook 分发战斗功能，还是必须改走数据面（堆对象字段读写）。
static const int kNameSlots = 32768;  // 2 的幂（v25：8192 → 32768，一次会话名字数可上千，别让哈希表饱和）
static void* g_nameSlot[kNameSlots] = {};
static std::atomic<uint32_t> g_nameLogged{0};
static std::atomic<uint32_t> g_nestLogged{0};
static std::atomic<uint32_t> g_interestLogged{0};
static thread_local int t_interpDepth = 0;

/* ============================================================================
 * ★v46 重入闸 —— 真机闪退（免广告开启后玩法进不去 + SIGSEGV）的直接对策。
 *
 * 血证：`/data/tombstones/tombstone_11`（pid 31958 / tid 32007 `UnityMain`）
 *   `Fatal signal 11 (SIGSEGV), code 2 (SEGV_ACCERR), fault addr 0x743401ffc8`
 *   `Cause: stack pointer is not in a rw map; likely due to stack overflow.`
 *   `pc 0x74364a77c0` − base `0x7434428000` = RVA `0x207F7C0` = 我们 DobbyHook 的
 *   解释器入口 `0x207F798` + 0x28；崩溃线程共 **560 帧**，`#02 = libstarry_magic.so+0x199a8`
 *   （我们的桩），其后 `libunity.so` 的 `0x74fa64` / `0x74f5a0` 严格交替 ~275 组；
 *   栈内存里能读到 ASCII `"eoResult"` ⇒ 崩的就是 `OnADVideoResult` 这条链路。
 *
 * 机理：`ApplyNoAdV43` 在解释器帧内 `runtime_invoke(OnADVideoResult)`，被调方又走到
 *   `PlayADVideo` ⇒ 再来一次代回调 ⇒ 两个方向互相递归，把 8MB 线程栈吃爆。
 *
 * 三道闸：
 *   ① `t_modInvoke` —— mod **主动发起**的托管调用执行期间，任何派发一律跳过（纯透传）。
 *      这是唯一真正必需的闸：它把「我们自己引发的再入」和「游戏正常的托管→托管嵌套」区分开。
 *      后者**绝不能拦** —— `Hurt`/`DoAttack` 本来就出现在嵌套深度 >0 处，拦了 #1/#2/#3 全哑
 *      （v30 已经踩过一次同型坑：`allowWrite=false` 让二级挂点什么都干不了）。
 *   ② `t_interpDepth` —— 只统计**我们自己派发期间**的嵌套（进 `InterpDispatch` 前 +1，
 *      调 `orig` **之前**就已 -1）⇒ 合法游戏嵌套永远看到 0，爬升只可能来自①漏网的再入，
 *      所以它只作硬上限兜底（`kInterpMaxDepth`），不是常规闸。
 *   ③ 免广告改**帧外执行**：`ApplyNoAdV43` 只登记待办，`DrainPendingAdV43` 在下一次解释器
 *      入口（上一次派发的帧已退干净）才 invoke ⇒ 从根上不再出现「帧内 invoke 帧」。
 * ============================================================================ */
/* 注：`t_modInvoke` 的声明已上移到 ⑦ 广告路径之前（路径A `Hooked_PlayADVideo2` 也要用）。 */
static const int kInterpMaxDepth = 24;    // 闸②的硬上限（正常路径永远到不了）
static std::atomic<uint32_t> g_reentrySkip{0};  // 被闸拦下的派发次数（STATS 可见）

struct InterpGateGuard {
    bool pass;
    InterpGateGuard() : pass(false) {
        if (t_interpDepth < kInterpMaxDepth) {
            t_interpDepth++;
            pass = true;
        }
    }
    ~InterpGateGuard() {
        if (pass) t_interpDepth--;
    }
    InterpGateGuard(const InterpGateGuard&) = delete;
    InterpGateGuard& operator=(const InterpGateGuard&) = delete;
};

static inline uintptr_t RawWord(const void* p, size_t off) {
    return *reinterpret_cast<const uintptr_t*>(reinterpret_cast<const char*>(p) + off);
}
// v28：同上，先清顶字节再判用户态区间（真机 0xb4… 指针原先直接越界被否）
static inline bool PlausiblePtr(uintptr_t u) { return u != 0 && starptr::InUserVa(u); }


/** v23：安全读「疑似对象」的类名。全程有界只读，绝不把野指针交给 il2cpp API（否则崩在 class_name 里）。
 *  Il2CppClass 布局（2022.3 arm64）：+0x00 image, +0x08 gc_desc, +0x10 name, +0x18 namespaze。 */
static const char* SafeObjType(uintptr_t obj, char* buf, size_t cap) {
    if (!obj || (obj & 7u)) return nullptr;
    // v25 修正：这里**不能**用 Il2Cpp::IsValidHeapPtr 做前置闸——它走 PermCached 的 1024 条
    //   maps 缓存表，本机映射数已超 1024（scan done regions=541 skipped=517 ⇒ 1058+），
    //   表溢出后它对所有对象返回 false ⇒ v24 的实参类型 dump 在 NAME[21] 之后全哑（只剩 6 行）。
    if (!Il2Cpp::IsReadableRange(obj, 8)) return nullptr;
    const uintptr_t k = RawWord((const void*)obj, 0);
    if (!k || (k & 7u)) return nullptr;
    if (!Il2Cpp::IsReadableRange(k, 0x20)) return nullptr;
    const uintptr_t np = RawWord((const void*)k, 0x10);
    if (!np || !Il2Cpp::IsReadableRange(np, 2)) return nullptr;
    const char* s = (const char*)np;
    size_t n = 0;
    while (n + 1 < cap && n < 72) {
        const unsigned char c = (unsigned char)s[n];
        if (!c) break;
        if (c < 0x20 || c > 0x7e) return nullptr;  // 非可打印 ASCII → 判定不是类名，放弃
        n++;
    }
    if (n == 0 || n >= 72) return nullptr;
    memcpy(buf, s, n);
    buf[n] = 0;
    return buf;
}

/** v24：只对「疑似伤害/命中/状态」类的实参对象 dump 字段表（类名 + 偏移 + 类型 + 前 3 个字）。
 *  目的：直接拿到活体对象里的真实字段布局（例：CtagHurtHPPara.m_llValue BigInt 的 mantissa/exponent）。 */
static std::atomic<uint32_t> g_fdPriN{0};
static std::atomic<uint32_t> g_fdOtherN{0};
static bool TypeInteresting(const char* t) {
    if (!t) return false;
    static const char* const kT[] = {"Hurt",  "Hit",      "Damage", "Hp",     "HP",     "Blood",
                                     "State", "Battle",   "Skill",  "Monster", "Character", "Buff",
                                     "Dead",  "Collider", "Attr",   "Role"};
    for (size_t i = 0; i < sizeof(kT) / sizeof(kT[0]); i++)
        if (strstr(t, kT[i])) return true;
    return false;
}
/** 高优先：名字里直接带伤害/血量语义（v25：独立配额，别被 Monster/SkillData 配置对象吃光）。 */
static bool TypePriority(const char* t) {
    if (!t) return false;
    static const char* const kT[] = {"Hurt", "Hit", "Damage", "Blood", "Hp", "HP", "Dead", "Collide"};
    for (size_t i = 0; i < sizeof(kT) / sizeof(kT[0]); i++)
        if (strstr(t, kT[i])) return true;
    return false;
}
static void DumpObjFields(void* obj, const char* tname) {
    if (!TypeInteresting(tname)) return;
    // v25：同类只 dump 一次（要的是**不同类的字段布局**，不是同一类的许多实例）
    static char g_fdDone[96][44];
    static std::atomic<uint32_t> g_fdDoneN{0};
    const uint32_t dn = g_fdDoneN.load(std::memory_order_relaxed);
    for (uint32_t i = 0; i < dn && i < 96; i++)
        if (strcmp(g_fdDone[i], tname) == 0) return;
    if (dn < 96) {
        snprintf(g_fdDone[dn], sizeof(g_fdDone[0]), "%s", tname);
        g_fdDoneN.store(dn + 1, std::memory_order_relaxed);
    }
    if (TypePriority(tname)) {
        if (g_fdPriN.fetch_add(1, std::memory_order_relaxed) >= 200) return;
    } else if (g_fdOtherN.fetch_add(1, std::memory_order_relaxed) >= 120) {
        return;
    }
    auto& api = Il2Cpp::GetApi();
    if (!api.object_class || !api.class_fields || !api.field_name || !api.field_offset) return;
    void* k = api.object_class(obj);
    if (!k) return;
    // v26e：字段区一次校验（原来逐字段 IsReadableRange = 逐字段 maps 解析）
    if (!Il2Cpp::IsReadableRange((uintptr_t)obj, 0x800)) return;
    void* it = nullptr;
    int n = 0;
    while (void* f = api.class_fields(k, &it)) {
        const char* fn = api.field_name(f);
        const size_t off = api.field_offset(f);
        const char* tn = "?";
        if (api.field_type && api.type_name) {
            void* ft = api.field_type(f);
            if (ft) {
                char* s = api.type_name(ft);
                if (s && Il2Cpp::IsReadableRange((uintptr_t)s, 2)) tn = s;
            }
        }
        uint64_t v0 = 0, v1 = 0;
        if (off < 0x7f0) {
            v0 = *(const uint64_t*)((const char*)obj + off);
            v1 = *(const uint64_t*)((const char*)obj + off + 8);
        }
        DBGLOG(TPL_LOG "  FD %s.%s off=%zu type=%s v0=%llx v1=%llx", tname, fn ? fn : "?", off, tn,
             (unsigned long long)v0, (unsigned long long)v1);
        if (++n >= 24) break;
    }
}

/** v26f 保命闸 2：**解释器入口 hook 的取证热路径总开关**。
 *  证据（`%TEMP%\mxyzt_lg22.txt`，pid 35571）：`feat 1 -> 1` @16:38:55.761 →
 *  `[mxyzt] proc /data/app/.../libstarry_magic.so`（mod 自带崩溃报告器）
 *  + `F/libc Fatal signal 11 (SIGSEGV) … in tid 35673 ([mxyzt] proc)` @16:38:56.3xx
 *  ⇒ 开「秒杀小怪」后 **0.5 s** 就死在自己的入口 hook 线程上；而同轮 `CALIB=0 / AS v=0`
 *  ⇒ 数据面写入路径一个字都没执行 ⇒ 死因只可能是入口 hook 里的只读取证（NAME/CALL/名册）。
 *  另：v21 一起崩在 `libil2cpp+0x205acd8`（x21=Monster klass、x07=0x7f7f…毒值）也源自这条路径。
 *  ⇒ 入口 hook 该看的都看完了（调用图 + 实参类型 + 演员对象），本轮**不再进热路径**：
 *    只保留 Dobby 装钩（证明装得上）+ NAME 级名册 + 「实参对象登记」这一条极轻动作。 */
static const bool kHookProbe = false;

/** v26i：战斗类开关是否有开（定义在 §⑫ 演员图附近，此处先用）。 */
static bool HookArmWanted();

/** v26：演员图入口（定义在 §⑫，本处只前向声明）。传进来的都是 hook 收到的**真实活体对象**。 */
static void ActorOffer(void* obj, const char* tname);
static void ActorRecord(uintptr_t a);

// ---- v26f：入口 hook 只「登记真实对象」，识别与写入一律搬到 mod 线程 ----
//  背景：v22 实测开「秒杀小怪」0.5s 后崩在入口 hook 线程（`Fatal signal 11 in tid 35673 ([mxyzt] proc)`），
//  而同轮数据面零执行 ⇒ 死因是 hook 热路径上的只读取证。这里把热路径压到「一次 IsReadableRange +
//  一次原子自增 + 存指针」，其余（类名判定 / 字段解析 / 写入）全在 mod 线程做。
static const uint32_t kSeenMax = 512;
static uintptr_t g_actorSeen[kSeenMax];
static std::atomic<uint32_t> g_actorSeenN{0};

/** 把实参里所有「可读 8 字节的真指针」登记进 g_actorSeen（热路径，绝不做类名/字段解析）。 */
static void ActorOfferArgs(void* a1, size_t wordOff) {
    if (!a1) return;
    const uintptr_t base = (uintptr_t)a1 + wordOff;
    if (!Il2Cpp::IsReadableRange(base, 48)) return;
    const uint64_t* w = (const uint64_t*)base;  // 此处不用 RawWord/PtrAt（它们在文件更后面才定义）
    for (int i = 0; i < 6; i++) {
        const uintptr_t v = (uintptr_t)w[i];
        if (!PlausiblePtr(v)) continue;
        if (!Il2Cpp::IsReadableRange(v, 16)) continue;
        /* v26l：首字必须是**像样的 klass 指针**。v26k 实测 `ACT?[2] obj=0x400026e87c90 cls=?`
         *   且 `+16=0xaa1f03e054fffe0b`（ARM64 `mov x0,xzr` 的机器码）—— 实参数组越界时会读到
         *   代码页，旧判据（非 0 + 可读）放行了一大堆这种垃圾，把 512 个名额占满。 */
        const uintptr_t k = *(const uint64_t*)v;
        // v28：klass 指针在真机上同样带 TBI 标签（0xb4…），必须先清标签再判区间，
        //   否则真机上这一整段 actor 扫描永远 continue ⇒ 身份链断 ⇒ 功能全部无效果。
        if (!starptr::InUserVa(k) || (k & 7ull)) continue;
        const uint32_t n = g_actorSeenN.load(std::memory_order_relaxed);
        if (n >= kSeenMax) return;
        uint32_t j = 0;
        for (; j < n; j++)
            if (g_actorSeen[j] == v) break;
        if (j < n) continue;
        g_actorSeen[n] = v;
        g_actorSeenN.store(n + 1, std::memory_order_relaxed);
    }
}


/** 新名字落一次日志（非热路径）；带伤害/血量关键词的额外取证。 */
static void InterpNameCensus(void* a0, void* a1, void* a2) {
    auto& api = Il2Cpp::GetApi();
    const uintptr_t nu = RawWord(a0, 0x18);
    const uintptr_t ku = RawWord(a0, 0x20);
    const char* nm = (PlausiblePtr(nu) && api.method_get_name) ? api.method_get_name(a0)
                                                             : (const char*)nu;
    const char* kn = (PlausiblePtr(ku) && api.class_name) ? api.class_name((void*)ku) : "?";
    const char* nmS = (nm && Il2Cpp::IsReadableRange((uintptr_t)nm, 2)) ? nm : "?";
    // v26f：实参遍历（CALL 行 + DumpObjFields + ActorOffer）就是 v22 那次 0.5s 内炸掉的现场。
    //   留下的只有这一条「登记真实对象」的极轻动作：把实参里的活体对象存进 g_actorSeen[]，
    //   由 mod 线程的 ActorApplyTick（400ms 一次、可验证、可封顶）负责后续识别与写入。
    if (HookArmWanted()) ActorOfferArgs(a1, 0);  // v26i：功能没开就不收（省掉启动期噪声）
    // v26u 降噪：12000 → 400。NAME 名册是纯诊断产物（整张解释器调用图已由 DLL 元数据+IL 取证
    //   完毕），而每次新名字都要写一行 logcat；加载期上千行是用户反馈「掉帧」的次要来源。
    //   需要完整名册时把 kHookProbe 打开即可（那时走 InterpDumpArgTypes 分支）。
    const uint32_t idx = g_nameLogged.fetch_add(1, std::memory_order_relaxed);
    if (idx >= (kHookProbe ? 12000u : 400u)) return;
    DBGLOG(TPL_LOG " NAME[%u] %s::%s mi=%p", idx, kn ? kn : "?", nmS, a0);
    // v24 关键升级：每个**新名字**都打一次实参类型 → 一次战斗就能覆盖整张解释器调用图
    //   （不再只靠关键词猜；哪条方法收到 CtagHurtHPPara / 玩家对象，日志里直接能看出来）
    // v26e 性能闸：整张调用图已取证完毕，而且每个实参的 SafeObjType 内部要 3 次 maps 解析
    //   （IsReadableRange 每次 fopen /proc/self/maps）⇒ 加载期上千个新名字会把主线程卡住。
    // v26f：整段搬到 kHookProbe 分支里（默认关）。
    if (nmS[0] == '?' && (!kn || kn[0] == '?')) return;
    static const char* const kInt[] = {"Damage", "Hurt",  "DoAttack", "Attack",  "Hp",    "HP",
                                       "Dead",   "Die",   "Kill",     "Invincible", "Immune", "Hit",
                                       "Wound",  "Heal",  "Recover",  "Monster", "Battle", "Skill",
                                       "Buff",   "Blood", "Block",    "Crit",    "Dodge",  "Shield"};
    // v23：类名也要匹配（本作战斗语义多在类名上：LHitDataInput / StateData / LHitBoxGroupInput …）
    static const char* const kClsInt[] = {"LHit",      "HitData",  "HitBox",   "Hitter",     "ActionState",
                                          "StateData",  "Character", "Monster",  "Battle",
                                          "Damage",     "Skill",    "Buff",     "Collider",   "Actor",
                                          "Attr",       "Role",     "Immune",   "Hurt",       "Blood",
                                          "Show",       "LData"};
    const char* kw = nullptr;
    for (size_t i = 0; i < sizeof(kInt) / sizeof(kInt[0]) && !kw; i++)
        if (nmS[0] != '?' && strstr(nmS, kInt[i])) kw = kInt[i];
    for (size_t i = 0; i < sizeof(kClsInt) / sizeof(kClsInt[0]) && !kw; i++)
        if (kn && kn[0] != '?' && strstr(kn, kClsInt[i])) kw = kClsInt[i];
    if (!kw) return;
    if (g_interestLogged.fetch_add(1, std::memory_order_relaxed) < 500) {
        DBGLOG(TPL_LOG " INTEREST %s::%s mi=%p kw=%s", kn ? kn : "?", nmS, a0, kw);
    }
}

// ---- v16：数据面发现（候选字段名探测 + 实例扫描）—— **v44 已整体删除** ----
//  旧实现：`kFieldProbeClasses` / `kFieldProbeNames` / `kInstProbeClasses` +
//  `FieldProbePass` / `DataProbePass` / `DumpInstancesFor` / `ProbeFieldList` / `ProbePass`
//  / `ProbeWatchInit` / `InstallProbeDump` / `WatchSlots` / `DataApplyTick`。
//  保留的 `DumpFieldList` / `FieldTryPass` / `DumpClassChain` 是**活路径**（偏移解析用）。

static void DumpFieldList(const char* cls);
static void FieldTryPass();
static void FieldTryPassFor(const char* cls);
static void DumpClassChain(const char* cls);


// ---- v17：字段清单 / 已知名回验 / 实例快照-比对（找 HP 等真实偏移）----
static void* g_snapObj[4] = {};
static uint64_t g_snapWord[4][32] = {};
static int g_snapN = 0;
static uint64_t g_snapMs = 0;
static bool g_snapDone = false;

/** 列出类的全部字段（名字/偏移/类型）；热更类靠这个拿真实字段名。 */
static void DumpFieldList(const char* cls) {
    void* k = ResolveClassPref(cls);
    if (!k) {
        DBGLOG(TPL_LOG " FLIST %s = class miss", cls);
        return;
    }
    auto& api = Il2Cpp::GetApi();
    if (!api.class_fields || !api.field_name) {
        DBGLOG(TPL_LOG " FLIST %s api miss", cls);
        return;
    }
    void* iter = nullptr;
    int n = 0;
    while (void* f = api.class_fields(k, &iter)) {
        const char* fn = api.field_name(f);
        const size_t off = api.field_offset ? api.field_offset(f) : 0;
        const char* tn = "?";
        if (api.field_type && api.type_name) {
            void* ft = api.field_type(f);
            if (ft) {
                char* s = api.type_name(ft);
                if (s && Il2Cpp::IsReadableRange((uintptr_t)s, 2)) tn = s;
            }
        }
        DBGLOG(TPL_LOG " FLIST %s::%s off=%zu type=%s", cls, fn ? fn : "?", off, tn);
        if (++n >= 120) break;
    }
    DBGLOG(TPL_LOG " FLIST %s n=%d klass=%p", cls, n, k);
}

/** 用「已知存在的方法名反推字段名」回验 class_get_field 到底能不能用。 */
static void FieldTryPassFor(const char* cls) {
    void* k = ResolveClassPref(cls);
    if (!k) {
        DBGLOG(TPL_LOG " FTRY %s = class miss", cls);
        return;
    }
    auto& api = Il2Cpp::GetApi();
    if (!api.class_get_field || !api.field_offset) return;
    static const char* const kNames[] = {
        // v19：按本作命名风格（下划线前缀驼峰）给出血量/攻击/免疫候选
        "_perHp",    "_hp",        "_Hp",       "_HP",        "hp",          "Hp",
        "HP",        "m_hp",       "curHp",     "_curHp",     "maxHp",       "_maxHp",
        "_hpMax",    "m_llValue",  "_llValue",  "_Invincible", "_InfiniteBlood",
        "_SilenceDead", "_dontBroken", "_oldSheildValue", "_dead", "isDead", "_isDead",
        "_attack",   "attack",     "_atk",      "atk",        "_damage",     "damage"};
    for (size_t i = 0; i < sizeof(kNames) / sizeof(kNames[0]); i++) {
        void* f = api.class_get_field(k, kNames[i]);
        DBGLOG(TPL_LOG " FTRY %s::%s = %s off=%zu", cls, kNames[i], f ? "HIT" : "null",
             f ? api.field_offset(f) : (size_t)0);
    }
}

static void FieldTryPass() { FieldTryPassFor("Monster"); }

/** v18：沿 class_parent 链列出「本类 + 全部父类」的字段（活体 HP/攻击/免疫字段一般在父类）。 */
static void DumpClassChain(const char* cls) {
    void* k = ResolveClassPref(cls);
    if (!k) {
        DBGLOG(TPL_LOG " CHAIN %s = class miss", cls);
        return;
    }
    auto& api = Il2Cpp::GetApi();
    if (!api.class_fields || !api.field_name) {
        DBGLOG(TPL_LOG " CHAIN %s api miss", cls);
        return;
    }
    for (int lvl = 0; k && lvl < 6; lvl++) {
        const char* kn = api.class_name ? api.class_name(k) : nullptr;
        void* iter = nullptr;
        int n = 0;
        while (void* f = api.class_fields(k, &iter)) {
            const char* fn = api.field_name(f);
            const size_t off = api.field_offset ? api.field_offset(f) : 0;
            const char* tn = "?";
            if (api.field_type && api.type_name) {
                void* ft = api.field_type(f);
                if (ft) {
                    char* s = api.type_name(ft);
                    if (s && Il2Cpp::IsReadableRange((uintptr_t)s, 2)) tn = s;
                }
            }
            DBGLOG(TPL_LOG " CFLIST %s[%d] %s.%s off=%zu type=%s", cls, lvl, kn ? kn : "?",
                 fn ? fn : "?", off, tn);
            if (++n >= 120) break;
        }
        DBGLOG(TPL_LOG " CHAIN %s lvl=%d cls=%s n=%d klass=%p", cls, lvl, kn ? kn : "?", n, k);
        k = api.class_parent ? api.class_parent(k) : nullptr;
    }
}

/** 快照前 4 个 Monster 实例的 256 字节（战斗开始时刻）。 */
static void SnapTake() {
    if (g_snapN > 0) return;
    void* k = ResolveClassPref("Monster");
    if (!k) return;
    std::vector<void*> v;
    Il2Cpp::GetApi().ScanInstances(k, v, 2048);
    int n = 0;
    for (size_t i = 0; i < v.size() && n < 4; i++) {
        const uintptr_t p = (uintptr_t)v[i] + 8;  // ScanInstances 给的是 klass 字前一格
        if (!Il2Cpp::IsReadableRange(p, 256) || RawWord((void*)p, 0) != (uintptr_t)k) continue;
        g_snapObj[n] = (void*)p;
        for (int w = 0; w < 32; w++) g_snapWord[n][w] = RawWord((void*)p, (size_t)w * 8);
        n++;
    }
    g_snapN = n;
    g_snapMs = NowMs();
    DBGLOG(TPL_LOG " SNAP taken n=%d", n);
}

/** 10s 后比对同一批实例，打印变化的字（HP 掉血 / 状态位）。 */
static void SnapDiff() {
    if (g_snapN <= 0 || g_snapDone || NowMs() - g_snapMs < 10000ull) return;
    g_snapDone = true;
    for (int i = 0; i < g_snapN; i++) {
        const uintptr_t p = (uintptr_t)g_snapObj[i];
        if (!Il2Cpp::IsReadableRange(p, 256)) continue;
        int changed = 0;
        for (int w = 0; w < 32; w++) {
            const uint64_t cur = RawWord((void*)p, (size_t)w * 8);
            if (cur == g_snapWord[i][w]) continue;
            changed++;
            if (changed <= 24)
                DBGLOG(TPL_LOG " SNAPDIFF i=%d +%02x old=%016llx new=%016llx", i, w * 8,
                     (unsigned long long)g_snapWord[i][w], (unsigned long long)cur);
        }
        DBGLOG(TPL_LOG " SNAPDIFF i=%d changed=%d", i, changed);
    }
}



/** 命中目标时的有界取证（只在每槽头 12 次命中走；这里才允许 maps 校验/解引用）。
 *  = hook-rules §25 要求的「探针自带装配自检行」：`INTERP <tag> kinds=… hit=N` 一旦出现，
 *  就证明「入口 hook 已装 + 名字学习已登记 + 该目标真的被调用过」三件事同时成立。 */
static const char* TagOf(uint16_t kinds) {
    const uint16_t k = (uint16_t)(kinds & (uint16_t)(0u - kinds));  // 最低置位的功能位
    for (size_t i = 0; i < kKindN; i++)
        if (kKinds[i].kind == k) return kKinds[i].tag;
    return "?";
}

static void InterpDumpTarget(uint16_t kinds, void* a0, void* a1, void* a2, uint32_t h) {
    const uintptr_t nmu = RawWord(a0, 0x18);
    const uintptr_t kpu = RawWord(a0, 0x20);
    auto& api = Il2Cpp::GetApi();
    const char* kn = (PlausiblePtr(kpu) && api.class_name) ? api.class_name((void*)kpu) : "?";
    DBGLOG(TPL_LOG " INTERP %s kinds=0x%04x (%s::%s) hit=%u mi=%p klass=%p a1=%p a2=%p", TagOf(kinds),
         kinds, kn, PlausiblePtr(nmu) ? (const char*)nmu : "?", h, a0, (void*)kpu, a1, a2);
}

/** #4 免广告：把布尔实参槽强制成 true —— **只写参数槽，不调用任何被解释的方法**。
 *  为什么不照斌哥那样「不调 orig、直接回调 `DS_Advertising::OnADVideoResult`」：
 *  那需要 `il2cpp_runtime_invoke` 或 HybridCLR 的 `AllocFrame` 才能构造解释器帧，而本项目
 *  唯一一次 runtime_invoke 尝试（`SaSetHpNow`）是崩溃向量、已整段删除 ⇒ 本版不做。
 *  anti-ban 口径（`skill/re-implement/anti-ban-analysis.md` §三）：免广告的安全做法 =
 *  「正常回调 / 走 successCallback」，不是「直接给奖励」——强制回调结果正好落在安全区。 */
static void ForceTrueArg(void* a1, uint8_t argBase, int argIdx, const char* tag) {
    if (!g_noAd.load(std::memory_order_relaxed)) return;
    /* ★v30 内联实参约定：bool 值就在 `(char*)a1 + 8*(argBase+argIdx)` 处（低字节）。
     *  v29 走的是 `slot=RawWord(...)` + `LivePtr(slot)` 的「指针数组」约定 ⇒ bool 值 0/1
     *  永远不是活指针 ⇒ 免广告这条路**从未写入过**（v29b `AD force` 计数为 0）。 */
    char* slot = reinterpret_cast<char*>(a1) + (size_t)(argBase + argIdx) * 8;
    if (!Il2Cpp::IsReadableRange((uintptr_t)slot, 8)) return;
    /* ★只写 **1 字节**：目标参数都是 `bool`（`OnADVideoResult(bool,…)` / `DispatchResult(int,bool)`）。
     *  解释器栈槽名义上是 8 字节，但万一把 bool 压成 1 字节，写 32 位就会踩到相邻参数 ——
     *  写 1 字节在任何实现下都安全，且 0→1 的语义完全等价（小端下整型同样得到 1）。 */
    const uint8_t old = *reinterpret_cast<volatile uint8_t*>(slot);
    if (old == 1u) return;
    /* ★v46b：解释器这条改写路径同样过一次性门（见 §⑦ 的 `AdForceGateOpen` 注释）。 */
    if (!AdForceGateOpen(kAdGateInterp)) return;
    *reinterpret_cast<volatile uint8_t*>(slot) = 1u;
    const uint32_t n = g_hitAd.fetch_add(1, std::memory_order_relaxed);
    if (n < 20 || (n % 40) == 0)
        DBGLOG(TPL_LOG " AD force %s slot=%p %u -> 1 (base=%u idx=%d)", tag, (void*)slot, (unsigned)old,
             argBase, argIdx);
}

/* ============================================================
 * ★v43 #4 免广告 = A1（强制成功结果）+ A2（代厂商 SDK 回调）
 *  IL 全链铁证（工具 `06_dumped/冒险与征途/aot/_tgt_q.py`）：
 *   · `DS_Advertising::OnGetADInfoRsp` 0x06001253 `IL_0043-0064`：
 *       `start_watch_ad_rsp.result==0` → `SDKProxy::Instance.PlayADVideo(ad_id, uuid.ToString())`
 *       （`SDKProxy::Instance` 静态字段 0x040024D5；uuid 是服务端刚下发的 ulong）
 *   · `MiaoYueSDKProxy::PlayADVideo(int,string)` 0x0600368D → `MiaoYueSDKGameObject::PlayADVideo(Hashtable)`
 *     0x0600363D → 厂商 SDK → 回调 `MiaoYueADCallBack::onRewardArrived` 0x0600362C
 *     → `MiaoYueSDKGameObject::OnADVideoResult(string)` 0x0600364B
 *     → `DataService::advertising`(静态字段 0x04000F1F) `.OnADVideoResult(bool ret,int adid,ulong uuid)`
 *       0x06001255 → `ReqADVideoResult` 0x06001256 = **cmd 2762 `finish_watch_ad_req{ad_id,uuid,result}`**
 *   · 奖励 100% 由服务端下发；客户端唯一的「领奖动作」就是那一发 cmd 2762，`result=0` 即「看完了」。
 *  ⇒ A1（`K_ADRESULT` 帧把 ret 槽写成 1，见 `ForceTrueArg`）**只在真实回调到来时**才有用；
 *    本机厂商 SDK 不出画、也永不回调 ⇒ A1 独自永远不触发 —— 这就是「点看广告没任何反应」。
 *  ⇒ A2 = 在 `PlayADVideo` 帧上**自己把成功的 `OnADVideoResult` 调起来**：uuid 取槽 2 的十进制字符串
 *    （由 `IL_005C stloc.1 / ldloca / UInt64::ToString()` 生成，就是服务端刚下发的那个）⇒ cmd 2762 必被服务端认。
 *  参数槽（实例方法 argBase=1）：槽1 = ad_id(int)、槽2 = uuid(String)
 * ============================================================ */
static void* g_miOnAdResultV43 = nullptr;  // DS_Advertising::OnADVideoResult(bool,int,ulong)
static void* g_dataSvcKlassV43 = nullptr;  // DataService（取静态字段 advertising）
static std::atomic<uint32_t> g_adRegMaskV43{0};  // 预登记进度位（1/2/4/8）
static std::atomic<uint32_t> g_adRegTryV43{0};
static std::atomic<uint32_t> g_adV43Seen{0};
static std::atomic<uint32_t> g_adV43Done{0};
static std::atomic<uint32_t> g_adV43Fuse{0};        // 熔断**次数**（只作 STATS 可见性，不再永久关死）
static constexpr uint32_t kAdV43MaxDone = 100;
/* ★v47：这个上限原本是**进程级永久** 30 次 —— 看满 30 个广告之后 `DrainPendingAdV43` 会
 *  在 `g_adV43Done >= kAdV43MaxDone` 处静默 `return`，功能**永久失效**且没有任何提示。
 *  这是用户报的「有时候功能会失效」的一条确定来源（尤其是长时间在线的玩家）。
 *  现在语义改为**按激活期**计：`Mxytz_OnFeatureChanged` / `Mxytz_ForceClear` 都会把它清零
 *  （关掉再打开 = 重新上膛），并把额度从 30 提到 100 —— 真正的失控保护仍然在，
 *  而且更强：`g_adV43FuseUntilMs` 的 30 秒熔断窗口是按「invoke 抛异常」触发的速率保护，
 *  与这个「累计成功次数」是两道独立的闸。 */
/* ★v46：熔断从「一次异常永久停用」改为「降级 30 秒自动重试」。
 *  旧语义（`g_adV43Fuse.store(1)` 之后 `return`）一旦触发就把整条广告链路永久关死，
 *  游戏侧再也等不到 `OnADVideoResult` ⇒ 玩家看到的是「点了广告玩法进不去」，
 *  与「栈溢出闪退」是同一功能的两张面孔。
 *  注：`g_adV43FuseUntilMs` / `kAdV43FuseCooldownMs` / 待办槽（`g_pendAd*`）与 `t_modInvoke`
 *  的声明已上移到 ⑦ 广告路径之前 —— 路径A `Hooked_PlayADVideo2` 也要用它们。 */

/** 读 Il2Cpp `String`（`klass|monitor|len@+0x10|utf16@+0x14`）转 uint64；非纯数字 → false。 */
static bool Il2CppU64OfStringV43(void* s, uint64_t* out) {
    if (!s || !out) return false;
    if (!Il2Cpp::IsReadableRange((uintptr_t)s, 0x18)) return false;
    const char* base = reinterpret_cast<const char*>(s);
    const int32_t len = *reinterpret_cast<const int32_t*>(base + 0x10);
    if (len <= 0 || len > 20) return false;
    const uint16_t* p = reinterpret_cast<const uint16_t*>(base + 0x14);
    if (!Il2Cpp::IsReadableRange((uintptr_t)p, (uintptr_t)len * 2u)) return false;
    uint64_t v = 0;
    for (int32_t i = 0; i < len; i++) {
        const uint16_t c = p[i];
        if (c < (uint16_t)'0' || c > (uint16_t)'9') return false;
        v = v * 10ull + (uint64_t)(c - (uint16_t)'0');
    }
    *out = v;
    return true;
}

/** A2 本体 —— ★v46 语义变更：这里**只登记待办，不再 invoke**。
 *  目标帧：`MiaoYueSDKProxy::PlayADVideo(int adid, string uuid)`；把「该叫什么回调」记进待办槽，
 *  真正的 `runtime_invoke` 由 `DrainPendingAdV43` 在**帧外**执行。
 *  真机血证（`/data/tombstones/tombstone_11`，pid 31958 / tid 32007 UnityMain）：
 *  旧版在这一帧内直接 `il2cpp_runtime_invoke(OnADVideoResult,…)`，被调方又走到 `PlayADVideo`
 *  ⇒ 两个方向互相递归，UnityMain 560 帧把栈涨破 8MB 保护页
 *  （`Fatal signal 11 SIGSEGV SEGV_ACCERR fault addr 0x743401ffc8`，sp 距栈顶仅 0x70 字节）。
 *  熔断：一次异常降级 30 秒（不再永久停用整条链路）；累计成功回调上限 `kAdV43MaxDone`。 */
static void ApplyNoAdV43(void* a1, uint8_t argBase) {
    if (!a1) return;
    /* 冷路径才允许做可读性检查（走 maps 快照，慢）：两个实参槽必须都在可读映射内。 */
    if (!Il2Cpp::IsReadableRange((uintptr_t)a1 + (uintptr_t)argBase * 8u, 16)) return;
    const uint32_t n = g_adV43Seen.fetch_add(1, std::memory_order_relaxed);
    const bool on = g_noAd.load(std::memory_order_relaxed);
    const int32_t adId = (int32_t)RawWord(a1, (size_t)argBase * 8);
    void* ustr = (void*)RawWord(a1, (size_t)(argBase + 1) * 8);
    uint64_t uuid = 0;
    const bool parsed = Il2CppU64OfStringV43(ustr, &uuid);
    if (n < 12 || (n % 20) == 0)
        LOGI(TPL_LOG " AD43 PlayADVideo 免广告=%d adid=%d uuidStr=%p uuid=%llu 解析=%d mi=%p",
             on ? 1 : 0, adId, ustr, (unsigned long long)uuid, parsed ? 1 : 0, g_miOnAdResultV43);
    if (!on || !parsed || !g_miOnAdResultV43) return;
    if (g_adV43FuseUntilMs.load(std::memory_order_relaxed) > NowMs()) return;
    if (g_adV43Done.load(std::memory_order_relaxed) >= kAdV43MaxDone) return;
    /* ★v46：登记待办（不 invoke）。同批多次 `PlayADVideo` 会被合并成一次回调 ——
     *  这既省掉重复 invoke，也直接砍掉「一次调用变两次」的递归放大。 */
    EnqueueNoAdV43(adId, uuid, "interp");
}

/** ★v46：免广告代回调的**帧外**执行体。
 *  调用点 = 解释器入口/压帧桩（见 `Hooked_Interp` / `Hooked_PushFrame`），且必须在
 *  `t_modInvoke == 0` 时执行 —— invoke 期间置位的 `t_modInvoke` 会让被调方内部的解释器入口
 *  一律跳过派发，递归链在此断掉。
 *  取待办用 `exchange(0, acq_rel)`：多线程同时进来也只有一个能拿到，且天然合并重复登记。 */
static void DrainPendingAdV43() {
    if (t_modInvoke != 0) return;  // mod 主动发起的托管调用里绝不再入
    if (g_pendAdReady.exchange(0, std::memory_order_acq_rel) == 0) return;
    if (!g_noAd.load(std::memory_order_relaxed)) return;
    if (g_adV43FuseUntilMs.load(std::memory_order_relaxed) > NowMs()) return;
    if (g_adV43Done.load(std::memory_order_relaxed) >= kAdV43MaxDone) return;
    auto& api = GetApi();
    if (!api.runtime_invoke || !g_miOnAdResultV43) return;
    /* 被调方是实例方法 ⇒ `this` = `DataService::advertising`（静态字段，与 `OnGetADInfoRsp` 同源）。 */
    if (!g_dataSvcKlassV43) g_dataSvcKlassV43 = FindClassAnywhere("DataService");
    void* adv = g_dataSvcKlassV43 ? api.StaticRefField(g_dataSvcKlassV43, "advertising") : nullptr;
    if (!adv) {
        LOGW(TPL_LOG " AD43 取不到 DataService::advertising klass=%p", g_dataSvcKlassV43);
        return;
    }
    const int32_t adId = g_pendAdId.load(std::memory_order_relaxed);
    const uint64_t uuid = g_pendAdUuid.load(std::memory_order_relaxed);
    uint8_t ret = 1;  // bool ret = true ⇒ cmd 2762 的 result = 0（服务端认定「看完了」）
    int32_t aid = adId;
    uint64_t uid = uuid;
    void* args[3] = {&ret, &aid, &uid};
    void* exc = nullptr;
    t_modInvoke++;
    api.runtime_invoke(g_miOnAdResultV43, adv, args, &exc);
    t_modInvoke--;
    if (exc) {
        g_adV43Fuse.fetch_add(1, std::memory_order_relaxed);
        g_adV43FuseUntilMs.store(NowMs() + kAdV43FuseCooldownMs, std::memory_order_relaxed);
        LOGW(TPL_LOG " AD43 invoke 抛异常 → 熔断降级 %llu ms（到期自动重试，不再永久关死）",
             (unsigned long long)kAdV43FuseCooldownMs);
        return;
    }
    const uint32_t k = g_adV43Done.fetch_add(1, std::memory_order_relaxed) + 1;
    if (k <= 10 || (k % 10) == 0)
        LOGI(TPL_LOG
             " AD43 ★已代回调 OnADVideoResult(true,%d,%llu)★ 第%u次（帧外，不出画也照发 cmd2762）",
             adId, (unsigned long long)uuid, k);
}

/** 「本版只取证」的目标（`Character::Damage` / `get_localDamage` / `PlayADVideo` /
 *  `CanClaimReward`）：一次点击或一场战斗就能定案「哪个方法真正决定发奖 / 扣血」，
 *  下一版再动行为。**绝不在未定案前动返回槽**（写错 a2 缓冲区会直接毁掉解释器帧）。 */
static void InterpEvidence(uint16_t kinds, void* a1, void* a2) {
    static std::atomic<uint32_t> s_n{0};
    const uint32_t n = s_n.fetch_add(1, std::memory_order_relaxed);
    if (n > 80) return;
    DBGLOG(TPL_LOG " IEVID#%u %s kinds=0x%04x a1=%p a2=%p", n, TagOf(kinds), kinds, a1, a2);
}

/** #2 无敌：把 `DoAttack` 的 accepter 置 null ⇒ 走官方「命中作废」分支
 *  （函数开头 `IL_0039-0044`：accepter 为 null 或 `accepter.interactiveGameObject` 为 null → `ret`，
 *  不结算、不飘字、不触发死亡）。**这就是斌哥的机制**（壳内存中文串：
 *  「打中玩家的命中已作废(accepter置null,原函数自行返回)」）。
 *  `StarsCollider::DoAttack(IAttack,IAccept,AttackData,AcceptData,Vector3,int,SynHitData)`
 *  是实例方法 ⇒ IL 的 arg.2(accepter) 落在实参数组下标 `argBase + 1`。 */
/* ============================================================================
 * ★v34：#3「全模式技能伤害增加」的**真落点** —— 投射物模块（斌哥 id2 的另一半）。
 *
 * 为什么不是 `localDamage`：v33 实测 `V33 localDamage=1` **写出 0 次** —— 每一颗
 * `StarsCollider` 的 `localDamage` 字节**本来就已经是 1** ⇒ 斌哥文档里 id2 的那一条
 * 「localDamage→1」在 v1.5.9 上是**空操作**，这就是「只开 #3 没有任何效果」的机械解释
 * （斌哥能有效，说明他改的还有别的；留下的日志串才是线索）。
 *
 * 斌哥壳内存里解出的中文日志串（`06_dumped/冒险与征途/mem_sdk2/` 的 anon*.bin 串表）：
 *   `★count %d→%d★`、`activeLimit %d→%d`、`emitByTargetCount`
 * 与 `ActionData.LAddProjectile` 的字段名**一一对应**（元数据 FieldDef）：
 *   `_count 0x040169AB`、`_activeCountLimit 0x040169AC`、`_activeCountLimitWithTag 0x040169AD`、
 *   `_emitByTargetCount 0x040169AF`（另有 `_emitByTarget 0x040169B7`、`_recursionTimes 0x040169AA`）。
 * 实例取得路径（**照斌哥的挂点**，文档 §6 名字池 + Token 表）：
 *   `ActionGameLibrary.ActionCommand_AddProjectile::AddProjectile(Character, ActionGameObject,
 *    ActionData.LAddProjectile, int, Vector3, Vector3, Action, int, int, IActionGameObject,
 *    int, bool, int, int, bool, int, int) -> ActionGameProjectile` `0x0601EB9A`
 *   ⇒ `LAddProjectile` 数据对象直接就是**第 3 个参数**（槽 `argBase+2`），不需要再走 `data` 字段。
 *   （`Execute(ActionCommandData) 0x0601EB99` 只是上游命令入口，斌哥名字池里挂的是上面这个
 *    `AddProjectile`；实测 `Execute` 还会被 `EventDelegate::Execute` 同名混淆命中，已弃用。）
 * ⇒ 把 `_count` / `_activeCountLimit` / `_activeCountLimitWithTag` / `_emitByTargetCount`
 *   **翻倍** = 同一技能打出更多段投射物 = **各模式都变强**（正是用户说的
 *   「为了适配其他模式做的」，而不是秒杀小怪那种一击秒）。
 * 只用 #3 的开关（`g_skillDmg`）管，与 #1/#2 完全不相干。
 * ========================================================================== */
static const int32_t kProjCountMax = 200;   /* 安全上限：别把场面投射物堆爆（掉帧/崩） */
static const float   kProjFanout   = 5.0f;  /* ★扇出：他代码里的这个常量从串池读不出来（他日志里是运行时
                                             *   值 `扇出×%d.%d`），先按 ×2.0 起步，用他这套日志调。 */

/* ============================================================================
 * ★v47 防闪退兜底 —— #3「全模式技能伤害增加」的**速率预算 + 看门狗**（用户报「玩久了掉帧闪退
 *   是技能伤害增加导致的」）。这一节是**纯新增的限流**，不改变任何判据与落点。
 *
 * 为什么需要（机理，标注为「推断」的证据链）：
 *   #3 的动作 = 把 `LAddProjectile._count` 乘 `kProjFanout`(5)、上限 `kProjCountMax`(200)，
 *   并且**写进游戏自己的技能配置对象**（不是在栈上临时改一下）。于是：
 *   ① 单次施放的投射物数最多 ×5（`_count` 小时）到 200（`_count` 大时）——这是**每个投射物
 *      一个 GameObject + 碰撞体 + 生命周期回调**的实打实的对象数；
 *   ② 它**不区分场景频率**：带周期性/持续施放（光环、连射、召唤）的技能会**每秒重复**这个放大，
 *      于是「每秒新增对象数」被放大 5 倍并**持续整个会话** —— 这正是「**玩久了**掉帧、最后闪退」
 *      的形状（内存与 GC 压力单调累积，而不是某一次瞬间爆掉）；
 *   ③ 唯一可能兜住它的那道引擎闸是 `_activeCountLimit`（`CheckProjectileActiveCountLimit`
 *      在 `activeCount > limit` 时 Dispose 最老的），而旧实现在 `limit > 0` 时把它**抬到 target**
 *      ⇒ **等于把回收闸门一起关掉**，与「防闪退」的目标正好相反。
 *
 * 因此三条兜底（都是 fail-safe：出问题只**减少**放大，绝不写坏内存）：
 *   ⓪ **不再写 `_activeCountLimit`** —— 保留引擎自带的回收上限（见 `ApplyProjectileExecBoost`
 *      里 v47 注）。`limit == 0`（= 游戏自己声明「不限制」）时旧实现本来也是不写。
 *   ① **每秒预算** `kProjBoostPerSecMax`：本秒「被放大的投射物**发数**」累计超过预算后，
 *      本秒**剩余施放一律不放大**（按游戏原值透传）。注意计的是 `Σtarget`，不是施放次数 ——
 *      一次 200 发的施放和 40 次 5 发的施放对内存的压力是同一量级。
 *   ② **看门狗**：连续 `kProjOverSecLimit` 秒都超预算 ⇒ **降级挂起** `kProjSuspendMs` 毫秒
 *      （期间不放大、并把已写进配置对象的 `_count` 还原），到期**自动恢复** ——
 *      既不永久废掉功能（用户会看到「功能突然没了」），又能在持续过载时把游戏从 OOM 边缘拉回来。
 *   ③ 诊断可见：`STATS-PROJ` 每 30s 一行（本秒峰值 / 预算拦下次数 / 挂起次数），下一轮可直接
 *      用真机日志把 `kProjBoostPerSecMax` 调到「正常游玩几乎不触发」的位置，而不是继续凭猜。
 *
 * 取值口径：`kProjBoostPerSecMax = 1500` 是**保守取大方**（正常战斗每秒 2000 个投射物已经是
 * 极端场面），目的是「基本只在失控时触发」；宁可偶尔多放过一点，也不要让正常技能被限流成废功能。 */
static const uint32_t kProjBoostPerSecMax = 1500;   /* 每秒钟被放大的投射物总发数上限 */
static const uint32_t kProjOverSecLimit   = 3;      /* 连续 N 秒超预算 ⇒ 降级挂起 */
static const uint64_t kProjSuspendMs      = 20000ull; /* 降级窗口（到期自动恢复） */
static const int      kProjSlotN          = 64;     /* ★v47：槽表 16 → 64（原 16 槽一旦被挤出，
                                                     * 丢了 base 就再也还原不回去 = 脏数据永驻） */

struct ProjFldCacheV34 {
    const void* k;
    size_t count, activeLimit, activeLimitTag, emitByTargetCount;
};
static ProjFldCacheV34 g_projFldV34 = {};
/* 他日志里的「槽%d/已用%d/挤掉%d次」= 按 `data` 指针开的一张槽表，存**游戏给的原始 count**；
 * 没有这张表就会在已经放大过的值上再乘一次（连乘失控 —— 正是「突然 90 多万伤害」那类残留）。
 * ★v47：容量 16 → 64 并**改为环形覆盖**（见 `ProjSlotTakeV47`）—— 16 槽在「一场战斗几十个不同
 *   技能配置对象」的真实场面下必然溢出，溢出即丢失 base ⇒ 功能关掉后那批配置仍是放大值。 */
struct ProjSlotV36 {
    const void* data;
    int32_t base;
};
static ProjSlotV36 g_projSlotV36[kProjSlotN] = {};
static uint32_t g_projSlotRR = 0;                  /* 环形游标（槽满时覆盖最老一项） */
static std::atomic<uint32_t> g_projTopSec{0};      /* 本秒顶层发射数 */
static std::atomic<uint32_t> g_projNestSec{0};     /* 本秒套娃层发射数 */
static std::atomic<uint32_t> g_projSetV36{0};      /* count 写入次数 */
static std::atomic<uint32_t> g_projLimitSetV36{0}; /* activeLimit 写入次数 */
static std::atomic<uint32_t> g_projSqueezedV36{0}; /* 「挤掉」次数（当时 count 比目标还大） */
static std::atomic<uint32_t> g_projNestLogV36{0};
static std::atomic<uint32_t> g_projSecLogV36{0};
static std::atomic<uint32_t> g_projMainLogV36{0};
static std::atomic<uint64_t> g_projSecStartV36{0};
/* ---- v47 限流/看门狗状态 ---- */
static std::atomic<uint32_t> g_projEmitSec{0};     /* 本秒已放大的投射物发数（Σtarget） */
static std::atomic<uint32_t> g_projBudgetSkip{0};  /* 被预算拦下的施放次数（累计） */
static std::atomic<uint32_t> g_projOverSec{0};     /* 连续超预算秒数 */
static std::atomic<uint32_t> g_projSuspendN{0};    /* 触发降级的次数（累计） */
static std::atomic<uint32_t> g_projEmitPeak{0};    /* 本窗口内「单秒放出量」峰值（诊断） */
static std::atomic<uint64_t> g_projSuspendUntilMs{0}; /* 降级挂起到期时刻（0 = 未挂起） */

/* 他原文（anon1b @0xb31f5）：
 *   `[冒险-投射物·★每秒总量#%d★] 顶层%d + 套娃%d = ★共%d个/秒★ | 扇出×%d.%d (目标N=%d)` */
static void ProjSecTickV36() {
    const uint64_t now = NowMs();
    const uint64_t st = g_projSecStartV36.load(std::memory_order_relaxed);
    if (!st) {
        g_projSecStartV36.store(now, std::memory_order_relaxed);
        return;
    }
    if (now - st < 1000ull) return;
    g_projSecStartV36.store(now, std::memory_order_relaxed);
    const uint32_t top = g_projTopSec.exchange(0, std::memory_order_relaxed);
    const uint32_t nest = g_projNestSec.exchange(0, std::memory_order_relaxed);
    if (!top && !nest) return;
    const uint32_t n = g_projSecLogV36.fetch_add(1, std::memory_order_relaxed);
    if (n < 30 || (n % 10) == 0) {
        const int fi = (int)kProjFanout;
        const int fd = (int)((kProjFanout - (float)fi) * 10.0f + 0.5f);
        DBGLOG(TPL_LOG " 投射物·★每秒总量#%u★ 顶层%u + 套娃%u = ★共%u个/秒★ | 扇出×%d.%d",
             n, top, nest, top + nest, fi, fd);
    }
}

/** ★v47：本秒「已放大的投射物发数」预算闸 + 看门狗。**只在冷路径（一次施放一次）调用**。
 *  返回 true = 允许本次放大（并已把 `cost` 计入本秒预算）；false = 本秒预算用尽 / 正在降级窗口。
 *  设计要点（fail-safe，全部方向都是「少放大」）：
 *   · 预算用尽 ⇒ 本次**按游戏原值透传**（不写 `_count`），不是把值改小；
 *   · 挂起窗口只影响**放大动作**，不影响 `Hurt`/`DoAttack`/广告等其它功能；
 *   · 挂起到期自动恢复（同 `g_adV43FuseUntilMs` 的口径），不永久废功能。 */
static bool ProjBudgetTakeV47(uint32_t cost) {
    const uint64_t now = NowMs();
    /* ① 降级窗口内：不放大（并顺手做一次还原，见 `ApplyProjectileExecBoost` 的调用点） */
    const uint64_t su = g_projSuspendUntilMs.load(std::memory_order_relaxed);
    if (su && now < su) return false;
    if (su) {
        g_projSuspendUntilMs.store(0, std::memory_order_relaxed);
        g_projOverSec.store(0, std::memory_order_relaxed);
        g_projEmitSec.store(0, std::memory_order_relaxed);
        LOGI(TPL_LOG " ★PROJ 降级窗口结束 → 自动恢复放大★（已拦 %u 次 / 累计降级 %u 次）",
             g_projBudgetSkip.load(std::memory_order_relaxed),
             g_projSuspendN.load(std::memory_order_relaxed));
    }
    /* ② 本秒预算（按秒滚动；`ProjSecTickV36` 只管日志，这里独立记账） */
    static std::atomic<uint64_t> s_secTick{0};
    if (now - s_secTick.load(std::memory_order_relaxed) >= 1000ull) {
        s_secTick.store(now, std::memory_order_relaxed);
        const uint32_t used = g_projEmitSec.exchange(0, std::memory_order_relaxed);
        const uint32_t pk = g_projEmitPeak.load(std::memory_order_relaxed);
        if (used > pk) g_projEmitPeak.store(used, std::memory_order_relaxed);
        /* 看门狗：连续 `kProjOverSecLimit` 秒超预算 ⇒ 降级挂起 */
        if (used > kProjBoostPerSecMax) {
            const uint32_t over = g_projOverSec.fetch_add(1, std::memory_order_relaxed) + 1;
            if (over >= kProjOverSecLimit) {
                g_projSuspendUntilMs.store(now + kProjSuspendMs, std::memory_order_relaxed);
                g_projSuspendN.fetch_add(1, std::memory_order_relaxed);
                g_projOverSec.store(0, std::memory_order_relaxed);
                LOGW(TPL_LOG
                     " ★PROJ 连续%u秒超预算(单秒%u发 > 上限%u) → 降级挂起 %llu ms 并还原脏数据★"
                     "（防闪退兜底，到期自动恢复）",
                     over, used, kProjBoostPerSecMax, (unsigned long long)kProjSuspendMs);
                return false;
            }
        } else {
            g_projOverSec.store(0, std::memory_order_relaxed);
        }
    }
    const uint32_t used = g_projEmitSec.fetch_add(cost, std::memory_order_relaxed);
    if (used + cost > kProjBoostPerSecMax) {
        const uint32_t n = g_projBudgetSkip.fetch_add(1, std::memory_order_relaxed);
        if (n < 10 || (n % 50) == 0)
            DBGLOG(TPL_LOG " PROJ 本秒预算用尽（已 %u + 本次 %u > %u）→ 本次按原值透传（第%u次）",
                 used, cost, kProjBoostPerSecMax, n + 1);
        return false;
    }
    return true;
}

/** ★v47：本秒累计量（给 `STATS-PROJ` 诊断行用）。 */
static uint32_t ProjEmitPeakTakeV47() {
    const uint32_t used = g_projEmitSec.load(std::memory_order_relaxed);
    const uint32_t pk = g_projEmitPeak.load(std::memory_order_relaxed);
    return used > pk ? used : pk;
}

/** ★v47：清空限流/看门狗状态（功能开关切换与 ForceClear 调用：重新上膛，不带着旧窗口）。 */
static void ProjBudgetResetV47(bool clearSuspend) {
    g_projEmitSec.store(0, std::memory_order_relaxed);
    g_projOverSec.store(0, std::memory_order_relaxed);
    g_projEmitPeak.store(0, std::memory_order_relaxed);
    if (clearSuspend) g_projSuspendUntilMs.store(0, std::memory_order_relaxed);
}

/** ★v47：槽表取槽 —— 先按 data 指针找；没有空槽就**环形覆盖最老一项**（而不是像 v36 那样
 *  固定挤掉 0 号槽）。为什么必须改：v36 在 16 槽满时恒取 `slot = 0`，一旦有第 17 个不同的
 *  技能配置对象进来，0 号槽的 base 就丢了 ⇒ 那个对象**永远还原不回去**（功能关掉之后它还是放大值）。
 *  环形覆盖至少保证「最近用过的 64 个对象」的 base 是准的，并且 `ProjRestoreV41` 会一次性扫全表。 */
static int ProjSlotTakeV47(const void* data) {
    for (int i = 0; i < kProjSlotN; i++)
        if (g_projSlotV36[i].data == data) return i;
    for (int i = 0; i < kProjSlotN; i++)
        if (!g_projSlotV36[i].data) return i;
    const int i = (int)(g_projSlotRR % (uint32_t)kProjSlotN);
    g_projSlotRR = (g_projSlotRR + 1u) % (uint32_t)kProjSlotN;
    return i;
}


/** ★v41：把之前写进 `LAddProjectile._count` 的扇出**还回原始值**（#3 关掉时调用）。
 *  为什么必须有：`_count` 是我们写进**游戏对象**里的持久状态，功能一关我们不再写，但游戏那边
 *  **还留着 count=5** ⇒ 技能继续发 5 发 ⇒ 用户 m03027 的「所有功能都关闭了，感觉伤害还是很高」。
 *  （这也是他说的「脏数据」—— 我们的写必须**可逆**。） */
static void ProjRestoreV41() {
    if (!g_projFldV34.count || g_projFldV34.count > 0x400) return;
    for (int i = 0; i < kProjSlotN; i++) {
        void* d = const_cast<void*>(g_projSlotV36[i].data);
        if (!d) continue;
        const uintptr_t fp = (uintptr_t)d + g_projFldV34.count;
        if (!Il2Cpp::IsReadableRange(fp, 4)) {          // 对象已回收 ⇒ 忘掉这槽
            g_projSlotV36[i].data = nullptr;
            continue;
        }
        int32_t* v = reinterpret_cast<int32_t*>(fp);
        const int32_t now = *v;
        if (now < 0 || now > kProjCountMax) {           // 异常值不动，只忘掉
            g_projSlotV36[i].data = nullptr;
            continue;
        }
        if (now == g_projSlotV36[i].base) {             // 已经是原始值
            /* ★v47：这种情况也把槽清掉 —— 否则 64 槽全占着时，每个 `Execute` 帧都要做
             *  最多 64 次 maps 查询（`IsReadableRange`）。清掉不影响正确性：功能再打开时
             *  base 会从「当前（= 原始）count」重新记一遍，仍是原始值。 */
            g_projSlotV36[i].data = nullptr;
            continue;
        }
        /* ★v47 安全闸：**只在「当前值比记录的原始值大」时还原**。
         * 旧实现只要 `now != base` 就写 base —— 于是当游戏自己把 count 改成更小的值时
         * （切场景/换技能/配置重载），我们会把**过期的大值**写回去，等于自己制造脏数据。
         * 我们的写永远是「调大」（`target = base × 扇出 ≥ base`），所以「变小」一定是游戏干的，
         * 那种情况**不是我们造成的，也不该由我们改**，只把槽忘掉即可。 */
        if (g_projSlotV36[i].base > now) {
            g_projSlotV36[i].data = nullptr;
            continue;
        }
        const int32_t bad = now;
        *v = g_projSlotV36[i].base;
        g_projSlotV36[i].data = nullptr;                // 还原过就退出槽表
        static std::atomic<uint32_t> s_log{0};
        const uint32_t n = s_log.fetch_add(1, std::memory_order_relaxed);
        if (n < 20 || (n % 20) == 0)
            DBGLOG(TPL_LOG " ★#3已关闭·还原投射物 count %d→%d★ data=%p（不留脏数据）", bad,
                 g_projSlotV36[i].base, d);
    }
}

/** ★v47：还原 + 清空全部槽（功能关闭 / ForceClear / 看门狗降级时调用）。
 *  比 `ProjRestoreV41` 多做一件事：把槽表**整表清干净**，避免下次开功能时用旧 base 去写。 */
static void ProjRestoreAllV47() {
    ProjRestoreV41();
    for (int i = 0; i < kProjSlotN; i++) g_projSlotV36[i].data = nullptr;
    g_projSlotRR = 0;
}

static void ApplyProjectileBoost(void* a1, uint8_t argBase) {
    if (!g_skillDmg.load(std::memory_order_relaxed)) return;
    if (!a1) return;
    ProjSecTickV36();
    auto& api = Il2Cpp::GetApi();
    if (!api.object_class || !api.class_name) return;
    /* 照斌哥的挂点：`AddProjectile(Character, ActionGameObject, LAddProjectile, …)` 是实例方法 ⇒
     *  槽 `argBase` = this，`argBase+0` = Character(chr)，`+1` = ActionGameObject(src)，
     *  `+2` = **`ActionData.LAddProjectile` 数据对象**（要改的字段就在它身上）。 */
    void* chr = (void*)RawWord(a1, (size_t)(argBase + 0) * 8);
    void* src = (void*)RawWord(a1, (size_t)(argBase + 1) * 8);
    void* data = (void*)RawWord(a1, (size_t)(argBase + 2) * 8);
    if (!LivePtr(data)) return;
    void* dk = api.object_class(data);
    if (!LivePtr(dk) || !Il2Cpp::IsReadableRange((uintptr_t)dk, 0x40)) return;
    const char* dn = api.class_name(dk);
    if (!dn || !strstr(dn, "LAddProjectile")) return;  // == IL 里的 `isinst LAddProjectile`
    /* ★v41：这一帧的 `chr` 是**权威施法者**（17 参方法的第 1 参），把「玩家侧用过的 data 对象」
     *  记进白名单 —— 只用于 `ApplyProjectileExecBoost` 里发送者解不出来时的**兜底**。
     *  （`LAddProjectile` 是技能配置对象、敌我可能共用实例 ⇒ 白名单**不能单独作为判据**。） */
    if (PlayerSideV41(chr) || PlayerSideV41(src)) ProjMarkPlayerDataV41(data);
    if (g_projFldV34.k != dk) {
        g_projFldV34.k = dk;
        g_projFldV34.count = api.FieldOffset(dk, "_count", 0);
        g_projFldV34.activeLimit = api.FieldOffset(dk, "_activeCountLimit", 0);
        g_projFldV34.activeLimitTag = api.FieldOffset(dk, "_activeCountLimitWithTag", 0);
        g_projFldV34.emitByTargetCount = api.FieldOffset(dk, "_emitByTargetCount", 0);
        DBGLOG(TPL_LOG " 投射物 字段 cls=%s count=+0x%zx activeLimit=+0x%zx tag=+0x%zx emit=+0x%zx", dn,
             g_projFldV34.count, g_projFldV34.activeLimit, g_projFldV34.activeLimitTag,
             g_projFldV34.emitByTargetCount);
    }
    /* ★v37 定案：count 的真落点是**第 4 个实参**，不是 `data._count`。
     *  IL 铁证（`_tgt_q.py il 0x0601EB9A` = 这个 17 参方法本体，5884 字节）：
     *    · 方法体内**没有任何** `ldfld 0x040169AB`（`LAddProjectile::_count`）⇒ 它根本不读 `_count`；
     *    · `IL_0106 ldarg.s 4 / IL_0108 stloc.s 10`，之后一路用 loc10 去发 prefab
     *      ⇒ **第 4 个实参就是要发射的数量**；
     *    · `_count` 是在 `ActionCommand_AddProjectile::Execute 0x0601EB99` 里被
     *      `IL_011B callvirt get_count()` 读出来、算好之后**当第 4 参传进来**的（`IL_02E1 call 0x0601EB9A`）。
     *  ⇒ v36 在这个方法里写 `data._count`，等于写了个**没人再读**的字段：实测
     *    「`投射物#0 ★count 1→5★` 写了 14 次、游戏毫无变化」就是这么来的。
     *  写第 4 参还顺带避开了「data 被复用当模板 ⇒ 每发再 ×扇出 连乘」的风险。 */
    /* ★v39：**本方法一个字节都不写**。
     * 铁证：17 参方法体内**没有任何 `ldfld 0x040169AB`（`_count`）** ⇒ 它根本不读 `_count`；
     * `_count` 的唯一读者在 `ActionCommand_AddProjectile::Execute 0x0601EB99`（见 enum 处铁证），
     * 它把 count 算成**循环上界**后逐发调用本方法 ⇒ 在本方法里写什么都已经晚了。
     * 第 4 个实参经 `IL_0106 ldarg.s 4 / switch(get_direction())` 只喂给
     * `ProjectileFactory::CreatProjectile(string, Vector3, Character, int, bool) 0x0601EB3A`
     * ⇒ 那是**方向值**，v37/v38 写它 = 把某些技能的朝向改掉（已撤）。
     * 这里只保留他串池 @0xb3431 那条「顶层实飞」观测日志：
     *   `[冒险-投射物·顶层实飞#%d] data=%p ✅实际%d个 = 当时count%d (目标%d)` */
    {
        const size_t cslot0 = (size_t)(argBase + 3) * 8;
        const int32_t acnt0 =
            *reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(a1) + cslot0);
        const uintptr_t cfp0 = (uintptr_t)data + g_projFldV34.count;
        const int32_t cnt0 = (g_projFldV34.count != 0 && g_projFldV34.count <= 0x400 &&
                              Il2Cpp::IsReadableRange(cfp0, 4))
                                 ? *reinterpret_cast<int32_t*>(cfp0)
                                 : -1;
        const uint32_t n0 = g_projMainLogV36.fetch_add(1, std::memory_order_relaxed);
        if (n0 < 40 || (n0 % 50) == 0)
            DBGLOG(TPL_LOG
                 " 投射物·顶层实飞#%u data=%p count=%d 第4参(方向)=%d chr=%p src=%p "
                 "(v39:17参方法不读_count、不写任何字节;真落点=Execute)",
                 n0, data, cnt0, acnt0, chr, src);
        return;
    }
    const size_t cslot = (size_t)(argBase + 3) * 8;
    int32_t* av = reinterpret_cast<int32_t*>(reinterpret_cast<uintptr_t>(a1) + cslot);
    const int32_t acnt = *av;
    const uintptr_t cfp = (uintptr_t)data + g_projFldV34.count;
    const bool cntReadable = (g_projFldV34.count != 0 && g_projFldV34.count <= 0x400 &&
                              Il2Cpp::IsReadableRange(cfp, 4));
    const int32_t cnt = cntReadable ? *reinterpret_cast<int32_t*>(cfp) : -1;  /* 只进日志 */
    /* ★顶层 / 套娃层（他原文 `★未放大,由游戏决定★`）：顶层 = 这一发不是由已有的投射物再发射出来的。
     *  v36 判据 `!LivePtr(src)` 被实测证伪：v36 日志里 **35 次调用全部被判成套娃层**（一个字节没写）
     *  ⇒ 「单开 #3 没任何效果」。实测那一帧是同一次技能施放：
     *    `投射物·套娃层#19 data=0x7d426c6cc540 … src=0x7d426b08de00 chr=0x7d426b08de00`
     *    且当拍 `GMPLAY _player=0x7d426b08de00`
     *  ⇒ **顶层施放时 `src` 与 `chr` 是同一个对象（就是玩家自己）**，`src` 不是空。
     *  ⇒ 顶层判据改为：`src` 为空 **或** `src == chr`（他自己那条「套娃层」日志里 src/chr 是不同的
     *    两个对象 —— 由已有投射物再发射时，发射者是投射物、角色是玩家 ⇒ 两者不等）。
     *  注意**不能**用「chr 的 _hitter == 玩家 _hitter」当判据：套娃层里 chr 同样还是玩家自己，
     *  那样会把套娃层也放大，违背他 `★未放大,由游戏决定★` 的原设计。 */
    const bool top = !LivePtr(src) || (LivePtr(src) && src == chr);
    if (!top) {
        g_projNestSec.fetch_add(1, std::memory_order_relaxed);
        const uint32_t n = g_projNestLogV36.fetch_add(1, std::memory_order_relaxed);
        if (n < 20 || (n % 20) == 0)
            DBGLOG(TPL_LOG
                 " 投射物·套娃层#%u data=%p 实飞%d个 count=%d (★未放大,由游戏决定★ src=%p chr=%p)",
                 n, data, cnt, cnt, src, chr);
        return;
    }
    g_projTopSec.fetch_add(1, std::memory_order_relaxed);
    if (acnt <= 0 || acnt > 10000) return;  // 安全闸：异常值一律不碰
    /* 槽表：记下**游戏给的原始 count**（目标N 的基数），目标 = 原始 × 扇出 */
    int slot = -1;
    bool isNew = false;
    for (int i = 0; i < 16; i++)
        if (g_projSlotV36[i].data == data) { slot = i; break; }
    if (slot < 0) {
        for (int i = 0; i < 16; i++)
            if (!g_projSlotV36[i].data) { slot = i; break; }
        if (slot < 0) slot = 0;
        g_projSlotV36[slot].data = data;
        g_projSlotV36[slot].base = (acnt < kProjCountMax) ? acnt : kProjCountMax;
        isNew = true;
    }
    int used = 0;
    for (int i = 0; i < 16; i++)
        if (g_projSlotV36[i].data) used++;
    int32_t target = (int32_t)((float)g_projSlotV36[slot].base * kProjFanout + 0.5f);
    if (target < 1) target = 1;
    if (target > kProjCountMax) target = kProjCountMax;
    /* count 往目标拉（他原文 `★count %d→%d★ (目标%d)`）—— 写的是**第 4 个实参**（见上） */
    int32_t cntAfter = acnt;
    bool wrote = false;
    if (*av != target) {
        if (*av > target) g_projSqueezedV36.fetch_add(1, std::memory_order_relaxed);
        *av = target;
        cntAfter = *av;
        wrote = true;
        g_projSetV36.fetch_add(1, std::memory_order_relaxed);
    }
    /* activeLimit：IL_14EC `get_activeCountLimit()` / IL_14F3 `ble.s` ⇒ 引擎**只有 >0 才做上限检查**
     *  （`CheckProjectileActiveCountLimit 0x0601E996` 里 `activeCount <= limit` 就返回，否则 Dispose 最老的）
     *  ⇒ `0` = **不限制**、**不该动** —— v36 日志里的 `activeLimit 0→0` 是正确行为，不是漏写。 */
    int32_t limBefore = 0, limAfter = 0;
    const size_t lofs[2] = {g_projFldV34.activeLimit, g_projFldV34.activeLimitTag};
    for (int i = 0; i < 2; i++) {
        const size_t o = lofs[i];
        if (!o || o > 0x400) continue;
        const uintptr_t fp = (uintptr_t)data + o;
        if (!Il2Cpp::IsReadableRange(fp, 4)) continue;
        int32_t* v = reinterpret_cast<int32_t*>(fp);
        if (*v <= 0 || *v > 10000) continue;
        if (i == 0) limBefore = *v;
        if (*v < target) {
            *v = target;
            if (i == 0) limAfter = *v;
            wrote = true;
            g_projLimitSetV36.fetch_add(1, std::memory_order_relaxed);
        } else if (i == 0) {
            limAfter = *v;
        }
    }
    if (!wrote) return;
    const uint32_t n = g_projMainLogV36.fetch_add(1, std::memory_order_relaxed);
    if (n < 40 || (n % 50) == 0)
        DBGLOG(TPL_LOG
             " 投射物#%u data=%p ★第4参count %d→%d★ (目标%d) 槽%d/已用%d/挤掉%u次%s activeLimit %d→%d "
             "chr=%p data._count=%d(不写:IL证明它不读)",
             n, data, acnt, cntAfter, target, slot, used,
             g_projSqueezedV36.load(std::memory_order_relaxed), isNew ? " ★新对象★" : "", limBefore,
             limAfter, chr, cnt);
}

/** ★v39 = **#3「全模式技能伤害增加」的真落点**（`ActionCommand_AddProjectile::Execute 0x0601EB99`）。
 *
 *  为什么必须换到这里（两条 IL 铁证）：
 *   ① `_tgt_q.py callers "LAddProjectile" "get_count"` ⇒ 全工程**唯一**读 `_count` 的方法就是
 *      `Execute`：`IL_011B callvirt get_count()` → `IL_0120 stloc.s 4`，loc4 之后当**循环上界**用；
 *   ② 17 参 `AddProjectile 0x0601EB9A`（5884 B）体内**没有任何 `ldfld 0x040169AB`**
 *      ⇒ 在它里面写 `_count` 或写第 4 参（方向）都没用（v36/v37 实测：写了 14 次、游戏毫无变化）。
 *
 *  取参路径（照 IL）：`Execute(ActionCommandData)` 是实例方法 ⇒ 槽 `argBase` = this、
 *  `argBase+1` = `ActionCommandData`；它的 `data` 字段（FieldDef `0x0401793D`，类型
 *  `ProtoBuf.IExtensible`）就是 Execute 里 `IL_0006 isinst LAddProjectile` 的那个对象。
 *  我们在**入口**（pushframe 钩子）就写 `_count` ⇒ 一定早于 `IL_011B` 的读取。
 *
 *  「顶层/套娃层」在 Execute 帧里天然只有顶层（套娃层由已有投射物自己再发射，不走命令帧），
 *  与斌哥 `★未放大,由游戏决定★` 一致 ⇒ 这里无条件放大，槽表仍记**原始 count** 防连乘。 */
static void ApplyProjectileExecBoost(void* a1, uint8_t argBase) {
    if (!g_skillDmg.load(std::memory_order_relaxed)) return;
    if (!a1) return;
    ProjSecTickV36();
    auto& api = Il2Cpp::GetApi();
    if (!api.object_class || !api.class_name) return;
    /* ★v40 修正槽号（v39 写的是 `argBase + 1`，那是**第 2 个**实参 —— `Execute` 只有 1 个实参，
     *  读到的必然不是 `ActionCommandData` ⇒ 即使登记成功也会在此早退）。
     *  槽约定：`argBase` 已经含实例方法的 `this`（`this` 占槽 0 ⇒ `argBase = 1`），
     *  故**第 1 个 IL 实参在槽 `argBase + 0`**（同一约定在 17 参 `AddProjectile` 上已验证：
     *  `chr = 槽 argBase+0`、`src = argBase+1`、`data = argBase+2`）。
     *  两个槽都试一遍，用**类名核对**决定，绝不靠猜。 */
    void* acd = nullptr;
    void* ak = nullptr;
    const char* an = nullptr;
    for (int si = 0; si < 2 && !acd; si++) {
        void* cand = reinterpret_cast<void*>(RawWord(a1, (size_t)(argBase + si) * 8));
        if (!LivePtr(cand)) continue;
        void* kk = api.object_class(cand);
        if (!LivePtr(kk)) continue;
        const char* cn = api.class_name(kk);
        if (!cn) continue;
        if (!strstr(cn, "ActionCommandData")) continue;  // 挡掉同名的 `EventDelegate::Execute` 之类
        acd = cand;
        ak = kk;
        an = cn;
        if (si)
            DBGLOG(TPL_LOG " 投射物·Exec 槽位 fd=argBase+%d（第 1 参在实际槽 argBase+0 之外）", si);
    }
    if (!acd) return;
    static const void* s_ak = nullptr;
    static size_t s_aoff = 0;
    if (s_ak != ak) {
        size_t o = api.FieldOffset(ak, "data", 0);
        if (!o) o = api.FieldOffset(ak, "_data", 0);
        s_ak = ak;
        s_aoff = o;
        DBGLOG(TPL_LOG " 投射物·Exec 字段 cls=%s ActionCommandData.data=+0x%zx", an, o);
    }
    if (!s_aoff || s_aoff > 0x400) return;
    void* data = reinterpret_cast<void*>(RawWord(acd, s_aoff));
    if (!LivePtr(data)) return;
    void* dk = api.object_class(data);
    if (!LivePtr(dk) || !Il2Cpp::IsReadableRange((uintptr_t)dk, 0x40)) return;
    const char* dn = api.class_name(dk);
    if (!dn || !strstr(dn, "LAddProjectile")) return;  // == IL 里的 `isinst LAddProjectile`
    /* ★★v41 发送者闸（用户 m02899 血证：「我开了 3，为什么敌人打我，我直接死了，打我的时候一堆伤害，
     *  是不是反了？」—— **就是反了**）。
     *  根因：`LAddProjectile` 是**技能配置对象**（`ActionData` 反序列化产物），**敌我共用同一个实例**；
     *  v40 之前 `ProjExec` 从来没登记成功（同名方法被毒化缓存挡住、且参槽号写错）⇒ 这个
     *  ×kProjFanout 的写入**第一次真正生效**，于是**敌人的技能弹幕也一起翻倍** ⇒ 被自己开的 #3 打死。
     *  修法：先解出「这次施放是谁发的」，**只有玩家侧才写**；解不出来就**不写（fail-closed）**。
     *  来源优先级：① `ActionCommandData.actionGameObject 0x0401793F`（IActionGameObject，实测就是
     *  施法者本体 —— v39 日志 `chr=0x7d427b532e00 src=0x7d427b532e00` 同指 `_player`）；
     *  ② `ActionCommandData.action 0x0401793E`（Action → 其 `character`/`gameObject` 字段）；
     *  ③ 白名单兜底（只在 ①②都解不出来时用，来源是 17 参帧上已验证为玩家侧的 `data`）。 */
    void* caster = CasterOfV41(acd, ak, an);
    if (!caster) {
        /* ★ 严格 fail-closed：**解不出施法者就不写**。
         *  绝不退回「白名单 data 指针」去授权 —— 那是敌我共用的技能配置实例，一旦按指针放行，
         *  敌人下一次技能就会重新吃到 ×5（m02899 的原样复现）。白名单只打日志，不做判据。 */
        static std::atomic<uint32_t> s_skip{0};
        const uint32_t sn = s_skip.fetch_add(1, std::memory_order_relaxed);
        if (sn < 10 || (sn % 200) == 0)
            DBGLOG(TPL_LOG " 投射物#%u 跳过（发送者解不出）data=%p acd=%p 白名单命中=%d", sn, data, acd,
                 ProjIsPlayerDataV41(data) ? 1 : 0);
        return;
    }
    {
        static std::atomic<uint32_t> s_okV41{0};
        const uint32_t o2 = s_okV41.fetch_add(1, std::memory_order_relaxed);
        if (o2 < 5 || (o2 % 200) == 0)
            DBGLOG(TPL_LOG " 投射物·发送者#%u 玩家侧=%p data=%p acd=%p", o2, caster, data, acd);
    }
    if (g_projFldV34.k != dk) {
        g_projFldV34.k = dk;
        g_projFldV34.count = api.FieldOffset(dk, "_count", 0);
        g_projFldV34.activeLimit = api.FieldOffset(dk, "_activeCountLimit", 0);
        g_projFldV34.activeLimitTag = api.FieldOffset(dk, "_activeCountLimitWithTag", 0);
        g_projFldV34.emitByTargetCount = api.FieldOffset(dk, "_emitByTargetCount", 0);
        DBGLOG(TPL_LOG " 投射物 字段 cls=%s count=+0x%zx activeLimit=+0x%zx tag=+0x%zx emit=+0x%zx", dn,
             g_projFldV34.count, g_projFldV34.activeLimit, g_projFldV34.activeLimitTag,
             g_projFldV34.emitByTargetCount);
    }
    if (!g_projFldV34.count || g_projFldV34.count > 0x400) return;
    const uintptr_t cfp = (uintptr_t)data + g_projFldV34.count;
    if (!Il2Cpp::IsReadableRange(cfp, 4)) return;
    int32_t* cv = reinterpret_cast<int32_t*>(cfp);
    const int32_t cnt0 = *cv;
    if (cnt0 < 0 || cnt0 > 10000) return;  // 安全闸：异常值一律不碰
    g_projTopSec.fetch_add(1, std::memory_order_relaxed);
    /* ★v47：槽表查找/登记改走 `ProjSlotTakeV47`（64 槽 + 环形覆盖，见其注释） */
    int slot = ProjSlotTakeV47(data);
    const bool isNew = (g_projSlotV36[slot].data != data);
    if (isNew) {
        g_projSlotV36[slot].data = data;
        g_projSlotV36[slot].base = (cnt0 < kProjCountMax) ? cnt0 : kProjCountMax;
    }
    int used = 0;
    for (int i = 0; i < kProjSlotN; i++)
        if (g_projSlotV36[i].data) used++;
    int32_t target = (int32_t)((float)g_projSlotV36[slot].base * kProjFanout + 0.5f);
    if (target < 1) target = 1;
    if (target > kProjCountMax) target = kProjCountMax;
    /* ★★v47 防闪退兜底（本节的核心）：**预算闸 + 看门狗**。
     *  顺序很重要：先算 target，再问预算 —— 预算按「本次将放出多少发」计费。 */
    if (!ProjBudgetTakeV47((uint32_t)target)) {
        /* 被限流：① 顺手把已经写进配置对象的放大值还原（降级窗口内不留脏数据）；
         *          ② **按游戏原值透传**（不写 `_count`）—— 只少放大，绝不改小。 */
        ProjRestoreAllV47();
        return;
    }
    bool wrote = false;
    int32_t cntAfter = cnt0;
    if (*cv != target) {
        if (*cv > target) g_projSqueezedV36.fetch_add(1, std::memory_order_relaxed);
        *cv = target;
        cntAfter = *cv;
        wrote = true;
        g_projSetV36.fetch_add(1, std::memory_order_relaxed);
    }
    /* ★★v47：**不再写 `_activeCountLimit` / `_activeCountLimitWithTag`**（防闪退兜底，本版关键改动）。
     *
     *  旧实现（v36~v46b）：`if (*v > 0 && *v < target) *v = target;`
     *  注释理由是「引擎只有 >0 才做上限检查，所以把上限抬到 target 才能让 5 倍弹幕都活着」。
     *  但这条恰好**把引擎唯一的回收闸门关掉了**：
     *    `GameObjectManager::CheckProjectileActiveCountLimit 0x0601E996` 的语义是
     *    「`activeCount > limit` 就 `Dispose` 最老那发」= 游戏自带的**同时在场投射物上限**。
     *    我们把它从配置值（例如 30）抬到 target（最多 200）⇒ 同时在场的投射物从 30 变 200，
     *    再叠加 `_count ×5` 的发射速率 ⇒ 「每秒新增对象数」与「常驻对象数」**同时**放大约 5~6 倍，
     *    而且**持续整个会话**（`_count` 写在游戏的技能配置对象上，不是一次性效果）。
     *    这与用户报的「**玩久了**掉帧、最后闪退」是同一形状：对象/内存压力单调累积到 OOM 或
     *    某次 GC/分配失败，而不是某一击瞬间爆掉。
     *  ⇒ 保留引擎自带的上限（它的回收策略是游戏作者调过的）。代价只是「同时在场数」不再被我们
     *    额外抬高；`_count` 的放大**完全保留** ⇒ 单次技能的段数/伤害增益不受影响。
     *  （`limit == 0` 时旧实现本来也是不写 —— 这里只是把「>0 也写」这条去掉。） */
    if (g_projFldV34.activeLimit || g_projFldV34.activeLimitTag) {
        static std::atomic<uint32_t> s_limLog{0};
        const uint32_t ln = s_limLog.fetch_add(1, std::memory_order_relaxed);
        if (ln < 3)
            DBGLOG(TPL_LOG
                 " PROJ v47：activeLimit 保持游戏原值不写（off=+0x%zx/+0x%zx）—— 保留引擎回收闸门",
                 g_projFldV34.activeLimit, g_projFldV34.activeLimitTag);
    }
    const uint32_t n = g_projMainLogV36.fetch_add(1, std::memory_order_relaxed);
    if (wrote && (n < 40 || (n % 50) == 0))
        DBGLOG(TPL_LOG
             " 投射物#%u data=%p ★Execute帧 count %d→%d★ (目标%d) 槽%d/已用%d/挤掉%u次%s "
             "acd=%p | 本秒预算已用=%u 上限=%u",
             n, data, cnt0, cntAfter, target, slot, used,
             g_projSqueezedV36.load(std::memory_order_relaxed), isNew ? " ★新对象★" : "", acd,
             g_projEmitSec.load(std::memory_order_relaxed), kProjBoostPerSecMax);
    else if (!wrote && (n < 8 || (n % 200) == 0))
        DBGLOG(TPL_LOG " 投射物#%u Execute帧 已是目标值 count=%d (目标%d) data=%p", n, cnt0, target, data);
}

/** ★v33 = 斌哥 id2「全模式秒杀 / 关卡模式」的**唯一落点**，与 #1 秒杀小怪、#2 无敌完全独立。
 *
 *  IL 铁证（`_tgt_il_damage.txt`，`StarsCollider::DoAttack 0x0601E8D9`）：
 *    `IL_03CC ldarg.0 / IL_03CD ldfld StarsCollider::localDamage(0x0401764D) / brtrue.s 17`
 *    `IL_03D4 ldsfld ActionGameLibraryConfig::IsEditorMode / brtrue.s 10`
 *    `IL_03DB call BattleElementCenter::get_localDamage() / brfalse IL_04B0`
 *  ⇒ 三条任一条为真，就落到 **客户端权威**分支 `IL_0422 DamageFactory::ProcDamageByClient(...)`；
 *    为假则落到 `IL_04B0` 的服务端路径。斌哥壳内原话就是
 *    「[冒险-关卡模式#%d] localDamage=%d (1=客户端权威)」。
 *
 *  所以这里在**每一颗即将结算的 `StarsCollider` 实例**（`this` = 帧槽 0）上把 `localDamage` 写 1 ——
 *  任何模式、任何技能/普攻的碰撞体都会走客户端权威伤害；不再只靠
 *  `BattleElementCenter.collider` 那个静态实例（`LocalDamageApplyV31` 仍然保留，两者互补）。 */
static void ApplyLocalDamageOnCollider(void* a1, uint8_t argBase) {
    (void)argBase;
    /* ★v41 停用（见 `kColliderLocalDamageV41` 声明处）：写的是**攻击方**的碰撞体 ⇒ 敌人打我们也生效。 */
    if (!kColliderLocalDamageV41) return;
    if (!g_skillDmg.load(std::memory_order_relaxed)) return;  // 只受 #3 开关管
    void* self = (void*)RawWord(a1, 0);                       // 实例方法 ⇒ 槽 0 = this
    if (!LivePtr(self)) return;
    auto& api = Il2Cpp::GetApi();
    if (!api.object_class || !api.class_name) return;
    void* k = api.object_class(self);
    if (!LivePtr(k) || !Il2Cpp::IsReadableRange((uintptr_t)k, 0x40)) return;
    static const void* s_k = nullptr;
    static size_t s_off = 0;
    if (k != s_k) {
        const size_t o = api.FieldOffset(k, "localDamage", 0);
        s_k = k;
        s_off = (o && o <= 0x400) ? o : 0;
        if (s_off) DBGLOG(TPL_LOG " V33 localDamage 字段 cls=%s +0x%zx", api.class_name(k), s_off);
    }
    if (!s_off) return;
    const size_t off = s_off;
    if (*reinterpret_cast<const uint8_t*>((uintptr_t)self + off) == 1) return;  // 已是客户端权威
    *reinterpret_cast<uint8_t*>((uintptr_t)self + off) = 1;
    const uint32_t n = g_hitSkill.fetch_add(1, std::memory_order_relaxed);
    if (n < 10 || (n % 200) == 0)
        DBGLOG(TPL_LOG " V33 localDamage=1 collider=%p cls=%s 第%u次（客户端权威）", self,
             api.class_name(k), n + 1);
}

static void ApplyDoAttackPolicy(void* a1, uint8_t argBase) {
    if (!g_invincible.load(std::memory_order_relaxed)) return;
    /* ★v30 内联实参约定（同 ApplyDmgMulArg 的血证分析）：`a1` = 逐个 8 字节**内联存放实参值**的槽区，
     *  实例方法的槽 0 = `this` ⇒ IL arg.2（accepter，引用类型，占一个槽）在槽 `argBase+1`。
     *  v29 沿用「指针数组」约定写成 `accepter = *(void**)aslot` —— 在内联约定下这一步解出来的是
     *  **accepter 的 klass 指针**（klass 本身是活指针，所以 `LivePtr` 闸放行、`INV cancel` 计数器
     *  却不是 0 就是错的），等于拿 klass 去当对象查 `interactiveGameObject` ⇒ 必然判错。 */
    char* aslot = reinterpret_cast<char*>(a1) + (size_t)(argBase + 1) * 8;
    if (!Il2Cpp::IsReadableRange((uintptr_t)aslot, 8)) return;
    void* accepter = (void*)RawWord(a1, (size_t)(argBase + 1) * 8);
    if (!LivePtr(accepter)) return;
    auto& api = Il2Cpp::GetApi();
    if (!api.object_class || !api.class_name) return;
    /* ★v31（照斌哥机制 + IL 定案）：
     *  `StarsCollider::DoAttack(IAttack, IAccept, AttackData, AcceptData, Vector3, int, SynHitData)`
     *  是实例方法 ⇒ 槽 `argBase+1` = 第 2 个 IL 参数 `IAccept accepter`，其真实类型就是 **`Hitter`**
     *  （v30 实测该槽对象 `cls=Hitter` ✓）。`DoAttack` IL_0039：
     *    `if (arg2 != null && arg2.get_interactiveGameObject() == null) return;`
     *  ⇒ 把「打在玩家身上的那一击」的 accepter 槽写 0 ⇒ 原函数自行 return，不结算、不飘字、不触发死亡。
     *  这正是斌哥壳内的原话：「打中玩家的命中已作废(accepter置null,原函数自行返回)」。
     *  玩家身份判据（唯一可靠）：`accepter == GameObjectManager._player._hitter`
     *  （`InteractiveGameObject::_hitter`，IL FieldDef 0x04017109）。 */
    const uintptr_t ph = g_playerHitterV31.load(std::memory_order_relaxed);
    bool hit = (ph != 0 && (uintptr_t)accepter == ph);
    if (!hit) {
        // 次判据：accepter 的 `interactiveGameObject`（属性 ⇒ backing 字段名按候选表试）== `_player`
        void* k = api.object_class(accepter);
        if (!LivePtr(k) || !Il2Cpp::IsReadableRange((uintptr_t)k, 0x40)) return;
        static const char* const kIgoNames[] = {"<interactiveGameObject>k__BackingField",
                                                "_interactiveGameObject", "interactiveGameObject"};
        size_t offIgo = 0;
        const char* hitName = nullptr;
        for (size_t i = 0; i < sizeof(kIgoNames) / sizeof(kIgoNames[0]) && !offIgo; i++) {
            const size_t o = api.FieldOffset(k, kIgoNames[i], 0);
            if (o && o <= 0x400) {
                offIgo = o;
                hitName = kIgoNames[i];
            }
        }
        static std::atomic<uint32_t> s_noIgo{0};
        if (!offIgo) {  // 认不出 IGO ⇒ 坚决不动（写错一个槽 = 毁解释器帧）
            const uint32_t n = s_noIgo.fetch_add(1, std::memory_order_relaxed);
            if (n < 5)
                DBGLOG(TPL_LOG " DOATTACK 槽 %d cls=%s accepter=%p 无 IGO 字段（playerHitter=%p）未动",
                     argBase + 1, api.class_name(k), accepter, (void*)ph);
            return;
        }
        if (k != g_acceptKlass || offIgo != g_offAcceptIgo) {  // IAccept.interactiveGameObject 按类缓存
            g_offAcceptIgo = offIgo;
            g_acceptKlass = k;
            DBGLOG(TPL_LOG " accepter igo=%s +0x%zx cls=%s", hitName, offIgo, api.class_name(k));
        }
        void* igo = Il2Cpp::ReadPtr(accepter, offIgo);
        /* ★v41 修为「按 roleId 判」：v31/v40 用的 `igo == g_playerPtr` / `IsPlayerObj(igo)` 都不可靠 ——
         *  logcat 血证（用户 m03027「2的无敌失效了」）：`V31 playerHitter _player=0x7d428a3aa700
         *  _hitter=0x7d448c05c900` → 03:45:09 变成 `_player=0x7d428718a000 _hitter=0x7d4288713900`
         *  → 03:48:05 / 03:48:52 又各换一套 ⇒ **`_player`/`_hitter` 每场战斗都换新对象**，
         *  缓存的 `g_playerHitterV31` / `g_playerPtr` 多半是上一场的野指针 ⇒ 一次都没判中
         *  （实测 `INV cancel` = **0** 次）。而 `ulRoleId` 整局不变（`131202533413845712`）⇒ 用 roleId。
         *  `PlayerSideV41` 里已包含 roleId/指针历史/`_hitter` 三条判据，任一条命中即算玩家侧。 */
        if (PlayerSideV41(igo)) hit = true;
        else {
            static std::atomic<uint32_t> s_dbgIgo{0};
            const uint32_t dn = s_dbgIgo.fetch_add(1, std::memory_order_relaxed);
            if (dn < 8)
                DBGLOG(TPL_LOG " DOATTACK 未判中 accepter=%p igo=%p igoRole=%llu plRole=%llu plH=%p ph=%p",
                     accepter, igo, (unsigned long long)RoleIdOfV40(igo),
                     (unsigned long long)g_playerRoleV32.load(std::memory_order_relaxed),
                     (void*)g_playerPtr, (void*)ph);
        }
    }
    if (hit) {
        *reinterpret_cast<void**>(aslot) = nullptr;  // 官方「命中作废」分支：函数自行 return
        const uint32_t n = g_hitInv.fetch_add(1, std::memory_order_relaxed);
        if (n < 10 || (n % 50) == 0)
            DBGLOG(TPL_LOG " INV cancel DoAttack 槽 %d accepter=%p playerHitter=%p", argBase + 1,
                 accepter, (void*)ph);
    }
}

/** ★v28 正路：#3 全模式技能伤害 / #1 秒杀小怪 —— 在**伤害公式参数 setter** 的实参槽上乘倍率。
 *
 *  为什么这条路（而不是 hook `Hurt`/`Damage`）：`CBattleActorUtil::Hurt` 这类战斗逻辑方法是
 *  **解释代码调解释代码**，走解释器体内的 `bl 0x207DCCC` 建帧后回到同一 dispatch 循环，
 *  **永不经过 AOT 入口 `0x207F798`**（全库 `bl 0x207F798` 的 5221 个调用点里 5219 个在 AOT 桩群
 *  `0x21C7960–0x220BA00`，解释器体内 0 个）⇒ 按名 hook 入口**结构上就够不到战斗核心**。
 *  而配置表反序列化是 **AOT 侧**调热更 setter ⇒ 实测可达（MuMu 普查：`ProjectileData::set_damageFactor_A`
 *  /`set_damageConst`/`set_damageFomulaID`、`BuffStateEffect::set_damageFactorA`/`set_damageConstPara`
 *  都在入口留下过 LP# 行）。伤害公式 `GameFormula::GetResult(attacker,target,damageFomulaID,
 *  damageParameter_A,damageParameter_B,damageConst,factorA,factorB)` 的参数**正是**这些字段
 *  （`06_dumped/冒险与征途/aot/_tgt_il_df4.txt:100-229`）⇒ 改参数即全局改伤害。
 *
 *  实参约定（il2cpp RuntimeInvoker 的 `void** args`）：`a1[i]` = **指向第 i 个实参值的指针**，
 *  实例方法的 `a1[0]` = this ⇒ 第一个真实实参在 `a1[argBase]`（`argBase` 由 `method_flags & 0x10`
 *  定：静态=0、实例=1）。写之前**必须**先确认落在可读映射内且原值是有界有限 float
 *  （`IsReadableRange` 走 maps 快照，慢但这里只在 setter 命中时跑，且**失败方向是「不改」**，安全）。 */
static void ApplyDmgMulArg(void* a1, uint8_t argBase, const char* tag) {
    const bool kill = g_killMob.load(std::memory_order_relaxed);
    const bool skill = g_skillDmg.load(std::memory_order_relaxed);
    if (!kill && !skill) return;
    float mul = skill ? (float)kSkillMul : 1.0f;
    if (kill && (float)kKillMul > mul) mul = (float)kKillMul;  // 两个功能共用一个伤害旋钮，取大者
    if (!(mul > 1.0f)) return;
    /* ★v29 装配自检（hook-rules §25）：**每一次进入都计数**，前 60 次打完整明细
     *  （实参槽指针 / 可读性 / 原值 / 写后值 / argBase / 倍率），之后每 200 次打一行汇总。
     *  这样「钩子到底有没有跑到、槽位对不对、值有没有真被改」全都能从日志直接看出来，
     *  不再需要用户截图或靠推理（上一版只对 40 次打日志，且 `DMGARG=0` 无法区分三种失败）。 */
    static std::atomic<uint32_t> s_n{0};
    const uint32_t en = s_n.fetch_add(1, std::memory_order_relaxed) + 1;
    const bool loud = (en <= 60) || ((en % 200) == 0);
    /* ★★v30 实参约定**实测修正**（v29b 血证，这是「全模式技能伤害增加无效」的唯一根因）：
     *  v29 按 il2cpp `RuntimeInvoker` 的 `void** args` 约定写成「`a1[i]` 是指向实参值的指针」，
     *  于是先 `slot = RawWord(a1, argBase*8)` 再解一层引用去读写 float。v29b 实测日志却是
     *      `DMGARG#1 FacA slot=0x3f000000 / #4 0x3ecccccd / #5 0x42200000 / #6..8 0x3f800000`
     *  —— 这些**正是 float 的位模式**（0.5f / 0.4f / 40.0f / 1.0f），合起来就是一组
     *  `damageFactor` 的合理取值；而它们被 `LivePtr` 判成「非活指针」而整段早退（`DMGARG=107` 全早退）。
     *  ⇒ **HybridCLR 的解释器入口 `0x207F798`/`0x207DCCC` 传的 `args` 是「实参值按序内联存放的
     *  连续 8 字节槽区」，不是「指向实参的指针数组」**（与 AOT 桩 `add x1,sp,#0x18` 完全吻合：
     *  它传的就是承载实参值的栈区地址）。所以：**值就在 `(char*)a1 + 8*argBase` 处，原地写。**
     *  实例方法的 `this` 占槽 0 ⇒ 第一个真实实参在槽 `argBase`（静态 0 / 实例 1）。 */
    char* slot = reinterpret_cast<char*>(a1) + (size_t)argBase * 8;
    if (!Il2Cpp::IsReadableRange((uintptr_t)slot, 8)) {
        if (loud) DBGLOG(TPL_LOG " DMGARG#%u %s slot=%p 不可读（未改）base=%u", en, tag, (void*)slot, argBase);
        return;
    }
    volatile float* p = reinterpret_cast<volatile float*>(slot);
    const float old = *p;
    if (!(old == old) || old < 0.0f || old > 1.0e7f) {
        if (loud) DBGLOG(TPL_LOG " DMGARG#%u %s slot=%p old=%.5f 非法（NaN/负/越界，未改）", en, tag, (void*)slot, (double)old);
        return;
    }
    float nv = old * mul;
    if (nv > 1.0e7f) nv = 1.0e7f;
    if (!(nv > old)) {
        if (loud) DBGLOG(TPL_LOG " DMGARG#%u %s slot=%p old=%.5f 已是放大值（未改）mul=%g", en, tag, (void*)slot, (double)old, (double)mul);
        return;
    }
    *p = nv;
    if (loud)
        DBGLOG(TPL_LOG " DMGARG#%u %s slot=%p 内联写 old=%.5f -> %.5f base=%u mul=%g", en, tag, (void*)slot, (double)old,
             (double)nv, argBase, (double)mul);
}

/** 确认是目标方法之后的一次调用分发。**只有这里允许碰 a1**：v26g 血证「入口热路径碰 a1 崩」
 *  的成因是**对任意调用**解 a1；现在身份已确认，才碰实参。
 *  `allowWrite=false`（★v28：来自**帧构造二级挂点** `0x207DCCC` 的调用）表示「只登记/只取证、
 *  不改行为」—— 该挂点的实参约定尚未实测定案，先只做覆盖性与参数布局取证，避免写错内存。 */
static void InterpFire(uint16_t kinds, uint8_t argBase, void* a0, void* a1, void* a2,
                       bool allowWrite) {
    if (!a1) return;
    if (kinds & K_DMGMUL) {  // ★v34：**整段停用**（残留清理，见 kDmgMulHook 处血证）
        if (kDmgMulHook && allowWrite) ApplyDmgMulArg(a1, argBase, TagOf(kinds));
    }
    if (!allowWrite) {
        InterpEvidence(kinds, a1, a2);  // 二级挂点：只留证（含参数帧解引用取证）
        return;
    }
    if (kinds & K_HURT) {
        /* `Hurt(ref CtagHurtHPPara para, ref bool, bool, ref BigInt, bool)` —— **静态**
         * ⇒ a1[0] 就是「指向 para 引用」的槽（ref 语义），`ApplyHurtPolicy` 内部再解一层。 */
        void* refSlot = (void*)RawWord(a1, 0);
        static std::atomic<uint32_t> s_ih{0};
        const uint32_t n = s_ih.fetch_add(1, std::memory_order_relaxed);
        if (n < 20 || (n % 100) == 0)
            DBGLOG(TPL_LOG " IHURT#%u refSlot=%p para=%p ready=%d", n, refSlot,
                 LivePtr(refSlot) ? *reinterpret_cast<void**>(refSlot) : nullptr,
                 g_paraReady.load() ? 1 : 0);
        ApplyHurtPolicy(refSlot);
    }
    if (kinds & K_DOATTACK) {
        ApplyLocalDamageOnCollider(a1, argBase);  // #3 全模式技能伤害增加（独立，与 #1/#2 无关）
        ApplyDoAttackPolicy(a1, argBase);         // #2 无敌（accepter 置 null，照斌哥）
    }
    if (kinds & K_PROJ) ApplyProjectileBoost(a1, argBase);  // #3 顶层实飞观测（照斌哥 @0xb3431，只读不写）
    /* #3 真落点：改 `Execute` 帧的 `_count`。★v41：关掉时**反向还原**，不留脏数据。 */
    if (kinds & K_PROJEXEC) {
        if (g_skillDmg.load(std::memory_order_relaxed))
            ApplyProjectileExecBoost(a1, argBase);
        else
            ProjRestoreV41();
    }
    if (kinds & K_ADRESULT) ForceTrueArg(a1, argBase, 0, "OnResult");  // OnADVideoResult(bool,…)
    if (kinds & K_ADREWARD) ForceTrueArg(a1, argBase, 1, "Dispatch");  // DispatchResult(int,bool)
    /* ★v43 A2：在 `PlayADVideo` 帧上代厂商 SDK 回调「成功」（见 ApplyNoAdV43 的整条 IL 链注释）。 */
    if (kinds & K_PLAYAD) ApplyNoAdV43(a1, argBase);
    if (kinds & (K_DAMAGE | K_LOCALDMG | K_PLAYAD | K_CANCLAIM)) InterpEvidence(kinds, a1, a2);
}

// ---------- 学习期：按**明文「类::方法」名**登记（只在「表未命中 + 学习期开」时跑）----------
/* 登记互斥（hook-rules §14：跨线程共享状态不许用「两次独立原子读写」当互斥）。
 *  只在**冷路径**（学习期里每个目标登记一次，全局总共 ≤ 目标数）取锁，热路径完全不碰。
 *  作用：避免两个线程同时往同一个空槽写，出现「A 的名字 + B 的类」这种不存在的组合键。 */
static std::atomic<uint32_t> g_learnLock{0};
struct LearnLockGuard {
    LearnLockGuard() {
        while (g_learnLock.exchange(1u, std::memory_order_acquire)) {
        }
    }
    ~LearnLockGuard() { g_learnLock.store(0u, std::memory_order_release); }
    LearnLockGuard(const LearnLockGuard&) = delete;
    LearnLockGuard& operator=(const LearnLockGuard&) = delete;
};

/** 登记一个 (名字指针, 类指针) → 功能位。返回 true = **本次新建了槽**（调用方据此打一次日志，
 *  ★v28：因为「按名匹配任意类」的目标（`cls==""`）会反复进入慢路径，不能靠 `g_kindDone` 抑制重复日志）。 */
static bool LearnRegister(const char* nm, void* klass, uint16_t kind, uint8_t argBase) {
    LearnLockGuard guard;
    const uint32_t mask = kLearnSlots - 1;
    const uint32_t h = LearnHash((uintptr_t)nm);  // ★v47：与 InterpDispatch 共用同一套哈希/位移
    for (uint32_t p = 0; p < kLearnProbeMax; p++) {
        LearnSlot& s = g_lslot[(h + p) & mask];
        const char* cur = __atomic_load_n(&s.name, __ATOMIC_ACQUIRE);
        if (cur == nm) {
            /* 同名同类的重载（如 `Character::Damage` 的 2/3 参）并入同一个槽；
             *  同名**不同类**必须各占一个槽（见 `LearnSlot::klass` 注释）。 */
            if (__atomic_load_n(&s.klass, __ATOMIC_ACQUIRE) == klass) {
                __atomic_fetch_or(&s.kinds, kind, __ATOMIC_RELAXED);
                return false;
            }
            continue;  // 同名不同类 → 往后找空槽
        }
        if (!cur) {
            s.kinds = kind;
            s.argBase = argBase;
            __atomic_store_n(&s.klass, klass, __ATOMIC_RELAXED);
            __atomic_store_n(&s.name, nm, __ATOMIC_RELEASE);  // 发布：名字最后写
            return true;
        }
    }
    DBGLOG(TPL_LOG " LEARN 表满（%u 槽）", kLearnSlots);
    return false;
}

/** ★v43：把广告目标**预先登记**进功能表（不等它自己被调用一次）。
 *  为什么必须预登记：登记原本只在「该方法真的流过解释器入口一次」时发生（学习期捕获），而
 *  `OnADVideoResult` / `DispatchResult` **只在广告回调时才被调用** —— 本机厂商 SDK 根本不出画，
 *  于是永远等不到那次调用 ⇒ A1（强制 ret=1）从未生效（= 「点看广告没反应」的另一半原因）。
 *  这里直接用已解析的 `MethodInfo*` 取 `(名字指针@+0x18, 类@+0x20)` 当键，与热路径 `InterpDispatch`
 *  的键完全一致 ⇒ 预登记后每次调用都命中快表。 */
static void PreRegisterKindV43(void* mi, uint16_t kind, const char* tag) {
    if (!mi) {
        LOGW(TPL_LOG " ↑v43 预登记 %s 失败（mi=0）", tag);
        return;
    }
    auto& api = GetApi();
    const uintptr_t nu = RawWord(mi, 0x18);
    const uintptr_t ku = RawWord(mi, 0x20);
    if (!nu || !ku) return;
    const bool isStatic = (api.method_flags && ((api.method_flags(mi, nullptr) & 0x10u) != 0));
    const uint8_t ab = isStatic ? 0 : 1;
    const bool isNew = LearnRegister((const char*)nu, (void*)ku, kind, ab);
    LOGI(TPL_LOG " ↑v43 预登记 %s kind=0x%04x base=%u mi=%p %s", tag, kind, ab, mi,
         isNew ? "新建" : "已存在");
}

/** ★v43：广告四目标预登记（可重试；四个都成功即锁闸）。
 *  由 `SyncHooks` 每拍调用 —— 锁闸后只剩「1 次原子读」，冷路径开销可忽略。
 *  为什么不用 `InstallAdHooks` 一次搞定：那里解析 mi 时类可能还没被加载（懒加载 AOT 镜像），
 *  一次失败就再也补不上（v26x 血证：`g_installedDmg` 无条件锁闸 ⇒ 一次失败永久失效）。 */
static void EnsureAdTargetsV43() {
    if (!g_noAd.load(std::memory_order_relaxed)) return;
    uint32_t mask = g_adRegMaskV43.load(std::memory_order_relaxed);
    if (mask == 0x0Fu) return;
    if (g_adRegTryV43.fetch_add(1, std::memory_order_relaxed) > 12) return;
    static const struct {
        const char* cls;
        const char* m;
        int argc;
        uint16_t kind;
        const char* tag;
        uint32_t bit;
    } kT[] = {
        {"MiaoYueSDKProxy", "PlayADVideo", 2, K_PLAYAD, "PlayAD2", 1u},
        {"DS_Advertising", "OnADVideoResult", 3, K_ADRESULT, "OnResult7", 2u},
        {"DS_Advertising", "DispatchResult", 2, K_ADREWARD, "Dispatch2", 4u},
        {"AdMascotManager", "CanClaimReward", 0, K_CANCLAIM, "CanClaim", 8u},
    };
    for (size_t i = 0; i < sizeof(kT) / sizeof(kT[0]); i++) {
        if (mask & kT[i].bit) continue;
        void* mi = ResolveMethod(kT[i].cls, kT[i].m, kT[i].argc, 0);
        if (!mi) continue;
        if (kT[i].bit == 2u) {
            g_miOnAdResultV43 = mi;        // ★A2 要 invoke 的就是它
            g_onAdResultMP = MethodPointer(mi);  // 旧 Dobby 路径的 native 桥
        }
        PreRegisterKindV43(mi, kT[i].kind, kT[i].tag);
        mask |= kT[i].bit;
    }
    g_adRegMaskV43.store(mask, std::memory_order_relaxed);
    if (mask == 0x0Fu) LOGI(TPL_LOG " ↑v43 广告四目标预登记**全部完成**（免广告 A1+A2 已就绪）");
}

static bool LearnAllDone() {
    for (size_t i = 0; i < kKindN; i++)
        if (__atomic_load_n(&g_kindDone[i], __ATOMIC_RELAXED) == 0) return false;
    return true;
}

/** 慢路径：名字比对 + **类名核对**（hook-rules §8：方法名可能重名，必须核对类身份）。
 *  ★v29：多带 `a1/a2/allowWrite` —— **登记成功的那一次调用本身也要执行策略**。
 *  血证：配置表 setter（`set_damageConstPara` 等）**每次加载只被调用一次**，
 *  而原来「命中表 → InterpFire / 慢路径 → 只登记」⇒ 唯一的那一次调用被白白放过、
 *  乘系数永远落不到任何一次调用上（实测 `LEARN ok …set_damageConst` 有、`DMGARG=0` 无）。
 *  ⇒ 登记后**立即对本次调用**执行一次策略。 */
static void InterpLearn(const char* nm, void* a0, void* a1, void* a2, bool allowWrite) {
    const uint32_t slow = g_interpCalls.fetch_add(1, std::memory_order_relaxed) + 1;
    if (slow > kLearnBudget) {
        g_learnActive.store(0, std::memory_order_relaxed);
        DBGLOG(TPL_LOG " LEARN 慢路径预算用尽（%u）→ 关闭学习期", slow);
        return;
    }
    /* ★v40 —— **修毒化 bug**（v39 血证：全日志只有一行
     *   `LEARN 拒 ActionCommand_AddProjectile::Execute ← 实际 EventDelegate::Execute`，
     *   而 `LEARN ok` 里**没有** `ProjExec`、`ApplyProjectileExecBoost` 全程 0 次调用 ⇒ #3 无效）。
     *   原因：这里原来**只用方法名指针 `nm` 做键**，而 IL2CPP 的方法名字符串是**驻留共享**的 ——
     *   `EventDelegate::Execute` 与 `ActionCommand_AddProjectile::Execute` 的名字是**同一个指针**
     *   ⇒ 第一次拒绝就把同名不同类的**真实目标永久挡在门外**（而且是静默早退，连日志都没有）。
     *   现在把**类指针**一起入键：只有「这个类确实不是目标类」才跳过。
     *   `RawWord(a0, 0x20)` 是纯内存读（与 `loadMeta` 里取类指针同一处），不增加任何外部调用。 */
    const uintptr_t rku = RawWord(a0, 0x20);
    const uint32_t rn = __atomic_load_n(&g_rejectN, __ATOMIC_RELAXED);
    for (uint32_t j = 0; j < rn && j < 16; j++)
        if (__atomic_load_n(&g_rejectNm[j], __ATOMIC_RELAXED) == nm &&
            __atomic_load_n(&g_rejectKls[j], __ATOMIC_RELAXED) == rku)
            return;  // 已核对：这个类确实与目标类不同
    auto& api = Il2Cpp::GetApi();
    /* ★v27：`api.class_name()` / `api.method_flags()` 是**跨模块外部调用**，
     *  **绝不能在名字对上之前调用** —— 慢路径每次进来都取一次类名/标志，等于给战斗里
     *  每一条「名字不在表里」的调用都加一次外部调用，是实打实的掉帧源。
     *  改成「名字先对上，再取元数据」（`loadMeta` 只执行一次，多目标命中时复用）。 */
    const char* kn = nullptr;
    void* clsk = nullptr;
    uint8_t argBase = 1;
    bool metaDone = false;
    auto loadMeta = [&]() {
        if (metaDone) return;
        metaDone = true;
        const uintptr_t ku = RawWord(a0, 0x20);
        clsk = (void*)ku;
        kn = (PlausiblePtr(ku) && api.class_name) ? api.class_name((void*)ku) : nullptr;
        /* ★2026-10-03 血证修正：`il2cpp_method_get_flags(method, uint32_t* iflags)` 是**双参** API。
         *  这里原来只传 1 个参数 ⇒ x1 残留（本次实测 = libil2cpp 代码段只读地址
         *  `0x400020807ac6`）→ callee 末尾 `str w0,[x20]` 写只读页 → SIGSEGV_ACCERR，
         *  崩在 `il2cpp_method_get_flags+0x1c`。第二参传 `nullptr` 时 callee 开头 `cbz x1` 直接跳过
         *  写入（反汇编 `20b16b8`），所以既正确又安全。 */
        argBase = (api.method_flags && (api.method_flags(a0, nullptr) & 0x10u)) ? 0 : 1;
    };
    for (size_t i = 0; i < kKindN; i++) {
        const KindDef& d = kKinds[i];
        if (__atomic_load_n(&g_kindDone[i], __ATOMIC_RELAXED) != 0) continue;
        if (d.method[0] != nm[0]) continue;  // 廉价首字节闸
        if (strcmp(nm, d.method) != 0) continue;
        loadMeta();
        if (kn && d.cls[0] && strcmp(kn, d.cls) != 0) {
            const uint32_t r = __atomic_fetch_add(&g_rejectN, 1u, __ATOMIC_RELAXED);
            if (r < 16) {
                __atomic_store_n(&g_rejectKls[r], rku, __ATOMIC_RELAXED);  // ★v40：类指针先写
                __atomic_store_n(&g_rejectNm[r], nm, __ATOMIC_RELAXED);    // 名字最后写（发布）
            }
            DBGLOG(TPL_LOG " LEARN 拒 %s::%s ← 实际 %s::%s (kls=%p)", d.cls, d.method, kn, nm,
                 (void*)rku);
            continue;
        }
        const bool added = LearnRegister(nm, clsk, d.kind, argBase);
        /* ★v29：**本次调用也要生效**（详见函数头注释）。注意 `InterpFire` 内部所有写动作
         *  都由功能开关把关 ⇒ 功能没开时这里只是走一遍只读取证，零副作用。 */
        InterpFire(d.kind, argBase, a0, a1, a2, allowWrite);
        /* ★v28：`cls==""`（按名匹配任意类）的目标**不置 kindDone** —— 否则第一个类登记后
         *  其它类（ProjectileData / SkillData / BuffStateEffect …）就再也进不来，「全模式」退化成单类。 */
        if (d.cls[0]) __atomic_store_n(&g_kindDone[i], 1u, __ATOMIC_RELAXED);
        if (added)
            DBGLOG(TPL_LOG " LEARN ok %s::%s kind=0x%04x base=%u nm=%p mi=%p", d.cls[0] ? d.cls : "*",
                 d.method, d.kind, argBase, nm, a0);
    }
    if (LearnAllDone()) {
        g_learnActive.store(0, std::memory_order_relaxed);
        LOGI(TPL_LOG " LEARN 全部登记完成 → 学习期关（热路径退化为纯指针表查）");
    }
}

/* ★v27 诊断：解释器**名字普查**（kLearnProbe，交付前必须置 false）。
 *  目的（2026-10-02 MuMu 首测驱动）：入口 hook 已 `armed`、90s 内慢路径 43 万次，但 8 个目标
 *  **一个都没登记**（`已登记=0/8`），而 `LEARN 拒` 一行都没有 ⇒ 8 个目标名一个都没出现过。
 *  到底是「目标方法根本不走这条入口」还是「名字/类指针位置猜错了」，必须**看实际流过入口的
 *  (类名::方法名) 对**，不能靠推理。
 *  ★设计要点：普查表**每 45 秒由 `Mxytz_Tick` 清一次（滚动窗口）**。若只做「首次出现才打」，
 *    开局加载期的几百个不同的对会在 1 秒内把上限吃光，随后真正打怪时才出现的 `Hurt`
 *    就永远打不出来了 —— 这正是首测拿不到结论的原因。滚动窗口保证**每个时间段的热点集**
 *    都会被重新打出来（战斗窗口里必然含 `Hurt`/`DoAttack`，如果它们真的走这条入口）。
 *  放在首字节位图**之前**，所以能看到全部名字（不只 H/D/g/P/O/C）。 */
static const bool kLearnProbe = false;  // ★v44 交付：关（要普查时改 true 重编）
static const uint32_t kLpCap = 400;  // 每个窗口最多打多少行
static std::atomic<uint32_t> g_lpN{0};
static uintptr_t g_lpKlass[kLpCap] = {};
static uintptr_t g_lpName[kLpCap] = {};

static void LearnProbe(uintptr_t ku, uintptr_t nu, void* a0) {
    const uint32_t used = g_lpN.load(std::memory_order_relaxed);
    if (used >= kLpCap) return;
    for (uint32_t i = 0; i < used; i++) {
        if (__atomic_load_n(&g_lpKlass[i], __ATOMIC_RELAXED) == ku &&
            __atomic_load_n(&g_lpName[i], __ATOMIC_RELAXED) == nu)
            return;  // 本窗口已打过
    }
    /* 多线程竞争在这里是良性的（诊断代码）：最坏情况是重复一行或丢一行，不会写坏内存。 */
    __atomic_store_n(&g_lpKlass[used], ku, __ATOMIC_RELAXED);
    __atomic_store_n(&g_lpName[used], nu, __ATOMIC_RELAXED);
    g_lpN.store(used + 1, std::memory_order_relaxed);
    auto& api = Il2Cpp::GetApi();
    const char* kn = (PlausiblePtr(ku) && api.class_name) ? api.class_name((void*)ku) : "?";
    DBGLOG(TPL_LOG " LP#%u %s::%s klass=%p nm=%p mi=%p", used, kn ? kn : "?", (const char*)nu, (void*)ku,
         (void*)nu, a0);
}

/** 普查窗口滚动（由 `Mxytz_Tick` 每 45s 调一次；**不在热路径上**）。 */
static void LearnProbeRollWindow() {
    const uint32_t n = g_lpN.exchange(0, std::memory_order_relaxed);
    DBGLOG(TPL_LOG " LP==== 窗口滚动（上窗 %u 行）====", n);
}

/** ★热路径（每次热更方法调用都过一遍）：1 次读 + 1~2 次指针比较。
 *  **禁止** maps 查询 / 字符串遍历 / 除法 / 日志 / 碰 a1 —— 这里每多 10 个周期，一场战斗就多 ~1.5e8 周期。 */
static void InterpDispatch(void* a0, void* a1, void* a2, bool allowWrite) {
    /* ★v47 防闪退兜底：**碰 a0 之前先做一次零 I/O 的廉价自证**（对齐 + 用户态区间），
     *  不合规立刻透传。理由（同型血证的直接教训）：v26g 那次崩就是「入口热路径上直接解引用
     *  一个没验过的指针」→ `SIGSEGV in libstarry_magic.so+0x1a338`，从此 a1 只在身份确认后才碰；
     *  但 a0 一直是无条件读 `+0x18`/`+0x20` 的 —— 两个挂点各有一条来源：
     *    · `Hooked_Interp` 的 a0 来自 `0x207F798` 的第 1 参（约定是 MethodInfo*）；
     *    · `Hooked_PushFrame` 的 mi 来自 `0x207DCCC` 的 **x1** —— 那 16 个调用点里
     *      混进一个「x1 不是 MethodInfo」的调用（例如控制流旁路/被优化的分支），
     *      或者模块尚在重定位中，我们对 `mi+0x18` 的读就是一次野读。
     *  代价：两条整数指令（无 maps 查询、无原子、无分支预测代价大的路径）——
     *  与「一场战斗 1.5e7 次调用」（文件头 §⑩a 注释里的量级）相比可忽略。
     *  注意**不要**在这里用 `starptr::LiveFastCounted`（它每次判都做 3 次原子 RMW）。 */
    if (!starptr::IsAligned(a0) || !starptr::InUserVa(a0)) return;
    const uintptr_t nu = RawWord(a0, 0x18);  // MethodInfo+0x18 = 方法名字符串指针
    if (!nu) return;
    const uint32_t mask = kLearnSlots - 1;
    const uint32_t h = LearnHash(nu);  // ★v47：与 LearnRegister 共用同一套哈希/位移
    const uintptr_t ku = RawWord(a0, 0x20);  // MethodInfo+0x20 = 类（同名不同类必须区分）
    for (uint32_t p = 0; p < kLearnProbeMax; p++) {
        LearnSlot& s = g_lslot[(h + p) & mask];
        const char* cur = __atomic_load_n(&s.name, __ATOMIC_ACQUIRE);
        if (cur == (const char*)nu) {
            /* ★名字指针相同还不够：il2cpp 的方法名字符串来自**共享池**，别的类里同名的方法
             *  很可能就是同一个 `const char*`。必须再核对**类指针**，否则会把「别的类的 Hurt /
             *  DoAttack」当成目标去动它的实参槽 —— 那是内存改写级别的错误。
             *  这一行是热路径新增的**唯一**开销：从已经在访存的 a0 上再读一个字 + 一次比较。 */
            if (__atomic_load_n(&s.klass, __ATOMIC_ACQUIRE) != (void*)ku) continue;
            const uint32_t hn = __atomic_fetch_add(&s.hits, 1u, __ATOMIC_RELAXED);
            if (hn < 12) InterpDumpTarget(s.kinds, a0, a1, a2, hn);
            InterpFire(s.kinds, s.argBase, a0, a1, a2, allowWrite);
            return;
        }
        if (!cur) break;  // 开放寻址：空槽 = 未命中
    }
    if (g_learnActive.load(std::memory_order_relaxed) == 0) return;
    /* 慢路径要读 `nu` 指向的字符串。安全前提（有实证，不是猜的）：
     *   v22 名册实测 `RawWord(a0,0x18)` 连续给出真实方法名（`Monster::set_perHp` 等）；
     *   v26y2 已确认 `a0` 就是解释器传进来的 MethodInfo。这里再加一道「首字节必须是
     *   标识符字符」的廉价闸，避免把非名字指针当字符串解引用（这是本函数唯一的裸读）。 */
    const uint8_t c0 = *reinterpret_cast<const uint8_t*>(nu);
    if (!((c0 >= 'A' && c0 <= 'Z') || (c0 >= 'a' && c0 <= 'z') || c0 == '_')) return;
    if (kLearnProbe) LearnProbe(ku, nu, a0);  // ★诊断：看实际流过入口的 (类::方法) 对
    /* ★v27：首字节不在目标名单（Hurt/DoAttack/Damage/get_localDamage/PlayADVideo/
     *  OnADVideoResult/DispatchResult/CanClaimReward ⇒ H D g P O C）就直接返回，
     *  连 `InterpLearn` 都不进（一次 load + 一次位测试）。
     *  ★v47：**删掉 v28 追加的 `set_damage*` 放行**（那 4 项目标已从 `kKinds` 移除，
     *  见其处注释）—— 放它们进慢路径只会做一整轮 strcmp 后什么都匹配不到，
     *  而且在负载高的战斗里还占着慢路径预算（`kLearnBudget`）。 */
    if (!(g_firstMask[c0 >> 3] & (uint8_t)(1u << (c0 & 7)))) return;
    InterpLearn((const char*)nu, a0, a1, a2, allowWrite);
}

/** ★v27 解释器入口桩：**只做一件事** —— 查功能表 + 无条件透传。
 *  这里曾经有 4 项开销（嵌套深度日志 / 每次调用的共享原子自增 / `%20000` 除法 / 名字哈希名册），
 *  v26v 定案它们就是用户说的「游戏变得好掉帧」的头号来源（一场战斗 ~1.5e7 次调用全过一遍）。
 *  现在热路径在 `InterpDispatch` 里只剩「1 次读 + 1~2 次指针比较」，这里不再加任何东西。
 *  ⚠️ 不要在这里碰 `a1`（v26g 血证：入口热路径解 a1 → SIGSEGV 崩在 libstarry_magic.so+0x1a338）；
 *     实参只在 `InterpFire`（身份已确认为目标方法）里碰。 */
static void* Hooked_Interp(void* a0, void* a1, void* a2) {
    /* ★v46：闸①`t_modInvoke`（mod 自己发起的托管调用里绝不再派发）+ 闸②`t_interpDepth` 硬上限。
     *  注意 `InterpGateGuard` 的作用域**只包住 `InterpDispatch`** —— 调 `orig` 之前计数就已归零，
     *  所以游戏正常的托管→托管嵌套（`Hurt`/`DoAttack` 就在那种深度上）看到的永远是 0，不受影响。 */
    if (t_modInvoke != 0) {
        g_reentrySkip.fetch_add(1, std::memory_order_relaxed);
    } else {
        InterpGateGuard gate;
        if (!gate.pass) {
            g_reentrySkip.fetch_add(1, std::memory_order_relaxed);
        } else if (a0) {
            InterpDispatch(a0, a1, a2, /*allowWrite=*/true);
        }
    }
    void* r = ((InterpFn)g_interpOrig)(a0, a1, a2);
    /* ★v46：免广告代回调**移到帧外**执行 —— `orig` 已返回，本线程上一次派发的解释器帧已经
     *  退干净，不再有「帧内 invoke 帧」；被调方内部的解释器入口会被闸①挡住。
     *  热路径成本：一次 relaxed load（无 RMW、不弄脏 cacheline），有活才进函数体。 */
    if (t_modInvoke == 0 && g_pendAdReady.load(std::memory_order_relaxed) != 0) DrainPendingAdV43();
    return r;
#if 0  // —— v10 诊断体（每调用扫 maps + 15 路子串匹配，实测把游戏拖死，停用保命）——
    if ((n % 500u) == 0)
        DBGLOG(TPL_LOG " interp calls=%u census=%u", n,
             g_censusLogged.load(std::memory_order_relaxed));
    if (n <= 30) {  // 前 30 次无条件取证：入口是否真被调用 + 真实类名/方法名字串
        const char* an0 = Il2Cpp::GetApi().method_get_name
                              ? Il2Cpp::GetApi().method_get_name(a0)
                              : nullptr;
        const char* nm0 = nullptr;
        const char* kn0 = nullptr;
        if (a0 && Il2Cpp::IsReadableRange((uintptr_t)a0, 0x28)) {
            void* np0 = Il2Cpp::ReadPtr(a0, 0x18);
            if (np0 && Il2Cpp::IsReadableRange((uintptr_t)np0, 1)) nm0 = (const char*)np0;
            void* kp0 = Il2Cpp::ReadPtr(a0, 0x20);
            kn0 = (kp0 && Il2Cpp::GetApi().class_name) ? Il2Cpp::GetApi().class_name(kp0)
                                                       : nullptr;
        }
        DBGLOG(TPL_LOG " INTERP#%u a0=%p nm=%s an=%s kn=%s a1=%p a2=%p", n, a0,
             nm0 ? nm0 : "?", an0 ? an0 : "?", kn0 ? kn0 : "?", a1, a2);
    }
    if (a0 && Il2Cpp::IsReadableRange((uintptr_t)a0, 0x28)) {
        void* np = Il2Cpp::ReadPtr(a0, 0x18);
        if (np && Il2Cpp::IsReadableRange((uintptr_t)np, 1)) {
            const char* nm = (const char*)np;
            // ① 全量名册（按 MethodInfo* 直映射哈希去重，最多 400 条）
            if (g_censusLogged.load(std::memory_order_relaxed) < 400) {
                const size_t h = (((uintptr_t)a0) >> 4) & (kCensusSlots - 1);
                if (g_censusSeen[h] != a0) {
                    g_censusSeen[h] = a0;
                    if (g_censusLogged.fetch_add(1) < 400) {
                        void* kpc = Il2Cpp::ReadPtr(a0, 0x20);
                        const char* knc = (kpc && Il2Cpp::GetApi().class_name)
                                              ? Il2Cpp::GetApi().class_name(kpc)
                                              : nullptr;
                        DBGLOG(TPL_LOG " CENSUS[%u] %s::%s mi=%p a1=%p a2=%p",
                             g_censusLogged.load(std::memory_order_relaxed) - 1,
                             knc ? knc : "?", nm, a0, a1, a2);
                    }
                }
            }
            // ② 子串名单（打怪/无敌/免广告链路）
            for (size_t i = 0; i < kInterpSubN; i++) {
                if (!strstr(nm, kInterpSub[i])) continue;
                if (g_interpSubSeen[i].fetch_add(1) < 4) {
                    void* kps = Il2Cpp::ReadPtr(a0, 0x20);
                    const char* kns = (kps && Il2Cpp::GetApi().class_name)
                                          ? Il2Cpp::GetApi().class_name(kps)
                                          : nullptr;
                    DBGLOG(TPL_LOG " SUB %s::%s mi=%p a1=%p a2=%p", kns ? kns : "?", nm, a0, a1,
                         a2);
                    if (a1 && Il2Cpp::IsReadableRange((uintptr_t)a1, 64)) {
                        DBGLOG(TPL_LOG "  S.arg a1[0..7]=%p %p %p %p %p %p %p %p",
                             Il2Cpp::ReadPtr(a1, 0), Il2Cpp::ReadPtr(a1, 8),
                             Il2Cpp::ReadPtr(a1, 16), Il2Cpp::ReadPtr(a1, 24),
                             Il2Cpp::ReadPtr(a1, 32), Il2Cpp::ReadPtr(a1, 40),
                             Il2Cpp::ReadPtr(a1, 48), Il2Cpp::ReadPtr(a1, 56));
                    }
                }
                break;
            }
            for (size_t i = 0; i < kInterpWatchN; i++) {
                if (nm[0] != kInterpWatch[i][0]) continue;
                if (strcmp(nm, kInterpWatch[i]) != 0) continue;
                if (g_interpSeen[i].fetch_add(1) < 8) {
                    void* kp = Il2Cpp::ReadPtr(a0, 0x20);
                    const char* kn = (kp && Il2Cpp::GetApi().class_name)
                                         ? Il2Cpp::GetApi().class_name(kp)
                                         : nullptr;
                    const char* an = Il2Cpp::GetApi().method_get_name
                                         ? Il2Cpp::GetApi().method_get_name(a0)
                                         : nullptr;
                    DBGLOG(TPL_LOG " INTERP %s::%s mi=%p klass=%p an=%s a1=%p a2=%p",
                         kn ? kn : "?", nm, a0, kp, an ? an : "?", a1, a2);
                    if (a1 && Il2Cpp::IsReadableRange((uintptr_t)a1, 64)) {
                        DBGLOG(TPL_LOG "  I.arg a1[0..7]=%p %p %p %p %p %p %p %p",
                             Il2Cpp::ReadPtr(a1, 0), Il2Cpp::ReadPtr(a1, 8),
                             Il2Cpp::ReadPtr(a1, 16), Il2Cpp::ReadPtr(a1, 24),
                             Il2Cpp::ReadPtr(a1, 32), Il2Cpp::ReadPtr(a1, 40),
                             Il2Cpp::ReadPtr(a1, 48), Il2Cpp::ReadPtr(a1, 56));
                        void* w = Il2Cpp::ReadPtr(a1, 0);
                        if (w && Il2Cpp::IsReadableRange((uintptr_t)w, 32)) {
                            DBGLOG(TPL_LOG "  I.wrap *a1=%p [0..3]=%p %p %p %p", w,
                                 Il2Cpp::ReadPtr(w, 0), Il2Cpp::ReadPtr(w, 8),
                                 Il2Cpp::ReadPtr(w, 16), Il2Cpp::ReadPtr(w, 24));
                        }
                    }
                    if (a2 && Il2Cpp::IsReadableRange((uintptr_t)a2, 16)) {
                        DBGLOG(TPL_LOG "  I.ret a2=%p [0..1]=%p %p", a2, Il2Cpp::ReadPtr(a2, 0),
                             Il2Cpp::ReadPtr(a2, 8));
                    }
                }
                break;
            }
        }
    }
    return ((InterpFn)g_interpOrig)(a0, a1, a2);
#endif  // v10 诊断体结束
}

/** ★v28 二级挂点：解释器**压帧/帧构造**函数 `0x207DCCC`（约定 `x0=ctx, x1=MethodInfo*, x2=实参`）。
 *
 *  这是「托管→托管」调用的唯一建帧通道 —— 也是**战斗核心唯一可能被看到的地方**：
 *  只读反汇编（`.eh_frame_hdr` 权威边界）实测 `bl 0x207DCCC` 全库 **16** 个调用点，**16/16 落在
 *  解释器体内**（AOT 桩群 `0x21C7960–0x220BA00` 0 个）；每个「CALL/CALLVIRT 建帧」处置块都是
 *  `mov x1,x27(=被调 MethodInfo*) … bl 0x207DCCC` → `str x23,[x0,#0x18]`(frame->obj=this) →
 *  `b` 回同一 dispatch 循环。⇒ `CBattleActorUtil::Hurt` / `Character::Damage` / `StarsCollider::DoAttack`
 *  这类**解释调解释**的方法只走这条路，AOT 入口 `0x207F798`（x0=MethodInfo*）永远看不到它们。
 *  （孪生结论：`0x20B3CD8 → 0x20CADE4` 才是「所有被解释方法进入」的单一咽喉，x0=MethodInfo*，
 *    但那是 4 字节 `b` 跳板 + 引擎自带 enter/exit 回调表 —— 官方扩展点，留作后续无补丁方案；
 *    本版先用**寄存器里带实参**的压帧函数，这样策略能直接改实参。）
 *
 *  ★v30：本桩改为 `allowWrite=true`。v29 这里写 `false` 是**「秒杀/无敌全失效」的第二根因**：
 *  v29b 实测 `IEVID` 81 行的 tag 分布 = `CanClaim 48 / DoAttack7 7 / Hurt5 7 / Damage 6 / LocalDmg 13`
 *  —— 即 `Hurt`/`DoAttack` **只在二级挂点上出现**（一级 AOT 入口一次都没看到它们），而 `allowWrite=false`
 *  会让 `InterpFire` 在 `K_DMGMUL` 之后直接 `InterpEvidence` + `return`（`IHURT=0`、`INV cancel=0`
 *  双证）⇒ **策略一次都没执行**。所以战斗核心的三条功能全部只差这一个布尔值。
 *  放开的底气 = 写前自证已做足：`ApplyHurtPolicy` 要求「对象 klass 名字含 `HurtHPPara`」，
 *  `ApplyDoAttackPolicy` 要求「klass 真有 `interactiveGameObject` 字段且偏移 ≤0x400」，
 *  两者任一不成立就只打日志、**一个字节都不写**（v26g 血证是「无自证乱碰 a1」，与此不同）。 */
static const uintptr_t kPushFrameRva = 0x207DCCC;
static void* g_pushFrameOrig = nullptr;
static std::atomic<bool> g_pushFrameHooked{false};
typedef void* (*PushFrameFn)(void*, void*, void*);

static void* Hooked_PushFrame(void* ctx, void* mi, void* args) {
    /* ★v46：与 `Hooked_Interp` 同一套闸 —— 二级挂点同样是解释器帧内的派发点，
     *  免广告那条 `runtime_invoke` 从这里进来时也必须被 `t_modInvoke` 挡住。 */
    if (t_modInvoke != 0) {
        g_reentrySkip.fetch_add(1, std::memory_order_relaxed);
    } else {
        InterpGateGuard gate;
        if (!gate.pass) {
            g_reentrySkip.fetch_add(1, std::memory_order_relaxed);
        } else if (mi) {
            InterpDispatch(mi, args, nullptr, /*allowWrite=*/true);
        }
    }
    void* r = ((PushFrameFn)g_pushFrameOrig)(ctx, mi, args);
    if (t_modInvoke == 0 && g_pendAdReady.load(std::memory_order_relaxed) != 0) DrainPendingAdV43();
    return r;
}

static void InstallPushFrameHook(const uintptr_t base) {
    if (g_pushFrameHooked.load(std::memory_order_relaxed)) return;
    void* fn = base ? (void*)(base + kPushFrameRva) : nullptr;
    const int rd = (fn && Il2Cpp::IsReadableRange((uintptr_t)fn, 16)) ? 1 : 0;
    LOGI(TPL_LOG " pushframe base=%p fn=%p rd=%d rva=0x%zx", (void*)base, fn, rd,
         (size_t)kPushFrameRva);
    if (!fn || !rd) {
        LOGW(TPL_LOG " pushframe 不可用，跳过二级挂点");
        return;
    }
    const int rc = DobbyHook(fn, (void*)&Hooked_PushFrame, &g_pushFrameOrig);
    LOGI(TPL_LOG " pushframe DobbyHook rc=%d errno=%d orig=%p", rc, errno, g_pushFrameOrig);
    const bool ok = (rc == 0 && g_pushFrameOrig != nullptr);
    g_pushFrameHooked.store(ok);
    if (ok) {
        g_interpCalls.store(0, std::memory_order_relaxed);
        g_learnMs.store(0, std::memory_order_relaxed);
        g_learnActive.store(1, std::memory_order_relaxed);
        LOGI(TPL_LOG " pushframe armed（托管→托管全覆盖：Hurt/Damage/DoAttack 从这条路进来）");
    }
}

/** 装解释器入口 hook（只装一次；失败不影响其它功能）。 */
static void InstallInterpHook() {
    static std::atomic<bool> s_done{false};
    if (s_done.exchange(true)) return;
    /* ★v45：改走 LibBaseResolve（「最大 rw-p」判据）。旧版是 LibBaseFromMaps 的
     * 「off==0 且地址最小」，在真机上恒定命中只读影子副本 ⇒ 四项功能全灭。
     * LogLibMaps 把每一份映射摊开打日志，便于对照 Frida 的模块表验收。 */
    LogLibMaps("libil2cpp.so");
    const uintptr_t base = LibBaseResolve();
    void* entry = base ? (void*)(base + kInterpEntryRva) : nullptr;
    const int rd = (entry && Il2Cpp::IsReadableRange((uintptr_t)entry, 16)) ? 1 : 0;
    LOGI(TPL_LOG " interp base=%p entry=%p rd=%d rva=0x%zx", (void*)base, entry, rd,
         (size_t)kInterpEntryRva);
    if (!entry || !rd) {
        LOGW(TPL_LOG " interp 入口不可用，跳过解释器 hook");
        return;
    }
    const int rc = DobbyHook(entry, (void*)&Hooked_Interp, &g_interpOrig);
    LOGI(TPL_LOG " interp DobbyHook rc=%d errno=%d orig=%p", rc, errno, g_interpOrig);
    g_interpHooked.store(rc == 0 && g_interpOrig != nullptr);
    if (g_interpHooked.load(std::memory_order_relaxed)) {
        /* ★v27：装成功后**打开学习期**并重置计数（原来这里是 `ResolveInterpTargets` 的 mi 表，
         *  已删除）。学习期一开，入口每次未命中都会按明文「类::方法」名扫一遍目标表；
         *  全部登记完 / 预算耗尽 / Tick 窗口到期即关。 */
        g_interpCalls.store(0, std::memory_order_relaxed);
        g_learnMs.store(0, std::memory_order_relaxed);
        g_learnActive.store(1, std::memory_order_relaxed);
    }
    InstallPushFrameHook(base);  // ★v28：二级挂点（托管→托管）
}

static void InstallAdHooks() {
    for (size_t i = 0; i < kAdSlotN; i++) {
        HookSlot& s = kAdSlots[i];
        if (*s.origPtr) continue;
        void* mi = ResolveMethod(s.cls, s.method, s.argc, s.nth);
        if (!mi) continue;
        NoteStatic(s.tag, mi);
        if (strcmp(s.tag, "PlayAD2") == 0) {
            // 拿到「真发奖回调」的 native 桥，供免广告时直接以成功结果收尾
            void* rmi = ResolveMethod("DS_Advertising", "OnADVideoResult", 3, 0);
            g_onAdResultMP = MethodPointer(rmi);
            g_miOnAdResultV43 = rmi;  // ★v43 A2 用 runtime_invoke 直调它
            DBGLOG(TPL_LOG " ad result bridge=%p mi=%p", g_onAdResultMP, g_miOnAdResultV43);
        }
        LogTargetAddr(s.tag, mi);
        void* fn = MethodPointer(mi);
        const bool code = fn && Il2Cpp::IsReadableRange((uintptr_t)fn, 16) &&
                          Il2Cpp::IsExecutableRange((uintptr_t)fn, 16);
        /* ⚠️ MI 槽（*MethodInfo）**禁止写**（v6 实测：`MI-patch DoAttack7` 后 0.4s
         *   scudo corrupted chunk → SIGABRT）。因此这里只有两条合法出路：
         *   ① methodPointer 是「可读+可执行」的真 AOT 码 → 用 Dobby inline hook；
         *   ② 否则（热更解释方法标记值）→ 只登记观察，不写任何游戏内存。 */
        if (fn && !code) {
            DBGLOG(TPL_LOG " %s methodPointer 非可执行码（解释方法标记值）→ 只登记观察，不写 MI 槽", s.tag);
            RegisterWatch(s.tag, mi);
            continue;
        }
        const bool db = code ? tryHook(fn, s.hookFn, s.origPtr, &g_hookAt[s.index], s.tag) : false;
        if (!db) RegisterWatch(s.tag, mi);
    }
    /* ★v43：免广告的四个目标预登记（含**只在广告回调时才被调用**的那两个）。 */
    EnsureAdTargetsV43();
}


/* v44 清理：删除 v7 的候选名册（`kProbeFields` 血量/伤害/状态位/阵营/广告字段名表、
 *  `kProbeClsNames` 28 个类名表、`g_probeKlass` 槽位）与其计数 `g_probePass*`/`g_probeMs`。
 *  偏移与身份判据已全部由 roleId / `_characterData` / 元数据固化，正式版不再做按名探针。
 *  注意：`kProbeFields` 里的名字表**不要**在将来「想省事随便探字段」时复活 ——
 *  HybridCLR 热更类的迭代器实测不终止（v6 事故：23.4 GB 死循环文件）。 */



static void SyncHooks() {
    if (!g_libReady.load()) return;
    const bool anyOn =
        g_killMob.load() || g_invincible.load() || g_skillDmg.load() || g_noAd.load();
    SyncFeatOnGate(anyOn);
    if (!anyOn) return;
    if (!CanInstallHooks()) {
        uint32_t n = g_gateFail.fetch_add(1, std::memory_order_relaxed);
        if (n < 5 || (n % 20) == 0) LOGI(TPL_LOG " hooks deferred (gate) n=%u", n);
        return;
    }
    /* ★v26y：**解释器入口 hook 才是四项功能的真正挂载点**，改为「任一功能打开就懒装」。
     *  依据：`CBattleActorUtil::Hurt`（#1 秒杀小怪 / #3 技能伤害）与广告链路
     *  （`MiaoYueSDKProxy::PlayADVideo` 等）都是 HybridCLR 热更**解释方法**，它们的
     *  `methodPointer` 全部指向解释器共享桩（`libil2cpp+0x207F04C`）⇒ Dobby 内联 hook
     *  物理上挂不上；只有入口 hook + 按「类::方法」名分发这一条路（= 斌哥壳的做法）。
     *  以前它挂在 `kDiag` 下面（v26v 因掉帧被关）⇒ 四项功能等于**从未真正挂载过**。
     *  现在挂在 `anyOn` 下面：全关时零开销，开任一功能即装（`InstallInterpHook` 内部自带 once 闸）。 */
    if (!g_interpHooked.load(std::memory_order_relaxed)) {
        InstallInterpHook();
        if (g_interpHooked.load(std::memory_order_relaxed))
            LOGI(TPL_LOG " interp hooks armed (四项功能主挂载点)");
        else
            LOGW(TPL_LOG " interp 入口挂载失败 —— 功能只能靠 Dobby/内存写");
    }
    if (kDmgHooks && !g_installedDmg && (g_killMob.load() || g_invincible.load() || g_skillDmg.load())) {
        InstallDmgHooks();
        /* v26x：★**只有 `Hurt5` 真挂上了才锁闸**。`InstallDmgHooks` 内部要
         *  `FindClassAnywhere("CBattleActorUtil")` 取 mi，而 AOT 镜像 `Assembly-CSharp`
         *  是懒加载的（v26x 实测：开机 11s 时枚举到的 13 个程序集里没有它）⇒ 若此刻
         *  取不到 mi 就装不上。旧代码无条件 `g_installedDmg = true` ⇒ **一次失败就永久
         *  不再重试**，功能静默失效。现在装不上就不锁闸，下一拍继续试。 */
        if (*kDmgSlots[0].origPtr) {
            g_installedDmg = true;
            LOGI(TPL_LOG " dmg hooks installed");
        } else {
            const uint32_t n = g_dmgFail.fetch_add(1, std::memory_order_relaxed) + 1;
            if (n < 5 || (n % 20) == 0) LOGI(TPL_LOG " dmg hooks 未装上（类/代码未就绪）n=%u", n);
        }
    }
    if (!g_installedAd && g_noAd.load()) {
        InstallAdHooks();
        g_installedAd = true;
        LOGI(TPL_LOG " ad hooks installed");
    }
    EnsureAdTargetsV43();  // ★v43：免广告四目标预登记（可重试，锁闸后仅 1 次原子读）
    if (g_installedDmg || g_installedAd) {
        if (kDmgHooks) RebindIfMpMoved();
        if (!g_hooked.load()) {
            g_installMs.store(NowMs(), std::memory_order_relaxed);
            g_hooked.store(true);
            LOGI(TPL_LOG " hooks armed");
        }
    }
}

// ============================================================
// ⑩ 对外接口（ModEntry.h 注册表契约，签名勿改）
// ============================================================
// ============================================================
// ⑪ v20 数据面落地（战斗三项功能；依据 v18/v19 实战字段表）
//   ActionState(+16 = _actionGameObject): isDying+38 isDead+39 immuneDamage+60
//        immunePhysicalDamage+61 immuneMagicDamage+62 invincible+70
//   ActionGameObject: <isMain>+20(Bool) _cdata+40(CharacterData*) <DmgProp>+240(tagDmgProp*)
//   CharacterData(ActionGameObjectData): obj_id+96 npcId+132 charType+148 ulRoleId+112
//   tagDmgProp.lfDamageOwnerRate+144(Single)
//   Monster(ProtoBuf 配置行): _perHp+56 _Invincible+84 _InfiniteBlood+236
// ============================================================
static void* g_asKlass = nullptr;
static std::atomic<bool> g_killCfgDone{false};
static uint64_t g_applyLastMs = 0;
static uint32_t g_applyRounds = 0;
static uint64_t g_myRoleId = 0;
static float g_skillDmgRate = 50.0f;  // 技能伤害倍率（可调）

// v21：实例校验 + 偏移自校准（v20 崩溃根因 = ScanInstances 命中里的假指针被当成实例写坏堆）
//   —— 只用「指针字段的目标 klass 必须匹配」来确认对象与字段偏移，绝不盲信元数据偏移
static void* g_agoKlass = nullptr;     // ActionGameObject
static void* g_cdKlass = nullptr;      // CharacterData
static void* g_dmgPropKlass = nullptr; // tagDmgProp
static long g_deltaA = 0;              // ActionState 实例字段偏移修正
static long g_deltaC = 0;              // CharacterData 实例字段偏移修正
static size_t g_offAgo = 0, g_offCdata = 0, g_offDmgProp = 0;
static int g_shiftStat[3] = {0, 0, 0};  // 扫描命中相对对象首地址的位移分布
static bool g_calibLogged = false;

struct MonSaved {
    void* p;
    int32_t perHp, inv, inf;
};
static MonSaved g_monSaved[512];
static int g_monSavedN = 0;

static inline uintptr_t PtrAt(uintptr_t p) {
    uintptr_t v = 0;
    memcpy(&v, (const void*)p, sizeof(v));
    return v;
}
static inline uint8_t ByteAt(uintptr_t p) {
    uint8_t v = 0;
    memcpy(&v, (const void*)p, 1);
    return v;
}
static inline void SetByte(uintptr_t p, uint8_t v) { memcpy((void*)p, &v, 1); }
static inline int32_t I32At(uintptr_t p) {
    int32_t v = 0;
    memcpy(&v, (const void*)p, 4);
    return v;
}
static inline void SetI32(uintptr_t p, int32_t v) { memcpy((void*)p, &v, 4); }
static inline uint32_t U32At(uintptr_t p) {
    uint32_t v = 0;
    memcpy(&v, (const void*)p, 4);
    return v;
}
static inline uint64_t U64At(uintptr_t p) {
    uint64_t v = 0;
    memcpy(&v, (const void*)p, 8);
    return v;
}
static inline float F32At(uintptr_t p) {
    float v = 0;
    memcpy(&v, (const void*)p, 4);
    return v;
}
static inline void SetF32(uintptr_t p, float v) { memcpy((void*)p, &v, 4); }

/** 在 [start,end) 里找「指向 expectKlass 实例」的指针字段（带 maps 校验）。0 = 未找到。 */
static size_t DiscoverPtrField(uintptr_t obj, void* expectKlass, size_t start, size_t end) {
    if (!expectKlass || end <= start || !Il2Cpp::IsReadableRange(obj + start, end - start)) return 0;
    for (size_t off = start; off + 8 <= end; off += 8) {
        const uintptr_t p = PtrAt(obj + off);
        if (!Il2Cpp::IsReadableRange(p, 16)) continue;
        if (*(void**)p == expectKlass) return off;
    }
    return 0;
}

/** 扫描命中 → 真对象首地址（优先 +8 约定，其次命中点本身；要求首字 == klass）。 */
static uintptr_t InstanceFrom(void* hit, void* klass) {
    const uintptr_t h = (uintptr_t)hit;
    if (Il2Cpp::IsReadableRange(h + 8, 16) && *(void**)(h + 8) == klass) {
        g_shiftStat[0]++;
        return h + 8;
    }
    if (Il2Cpp::IsReadableRange(h, 16) && *(void**)h == klass) {
        g_shiftStat[1]++;
        return h;
    }
    g_shiftStat[2]++;
    return 0;
}

/** 小怪配置行改写：开 = 全表 _perHp=1/_Invincible=0/_InfiniteBlood=0；关 = 原值还原。 */
static void* g_strKlass = nullptr;
static uintptr_t g_monFirst = 0;   // 首个通过三要素校验的行（用于后续轮次快速复用）

/** v22 三要素校验：只认「首字==Monster klass + _id 合法 + _heroName 为空或真 String」的行。
 *  v21 仅「首字==klass」不够：任何**存放 klass 指针的字段**（例：System.Type.type）都会命中同一
 *  个字节模式，按 +56/+84/+236 写入就是打穿别的对象 → 堆损坏 → 由 il2cpp 内部以 klass 为上下文崩。 */
static bool LooksLikeMonsterRow(uintptr_t o, void* k) {
    if (PtrAt(o) != (uintptr_t)k) return false;
    const int32_t id = I32At(o + 16);     // Monster._id
    if (id < 1 || id > 10000000) return false;
    const int32_t perHp = I32At(o + 56);  // Monster._perHp
    if (perHp < 0 || perHp > 2000000000) return false;
    const uintptr_t nm = PtrAt(o + 24);   // Monster._heroName (System.String)
    if (nm) {
        if (!Il2Cpp::IsReadableRange(nm, 32)) return false;
        void* nk = *(void**)nm;
        if (g_strKlass) {
            if (nk != g_strKlass) return false;
        } else if (nk && Il2Cpp::IsValidHeapPtr(nk) && Il2Cpp::GetApi().class_name) {
            const char* cn = Il2Cpp::GetApi().class_name(nk);
            if (cn && strcmp(cn, "String") == 0) return true;
            return false;
        }
    }
    return true;
}

static void KillMobConfigPass(bool on) {
    void* k = ResolveClassPref("Monster");
    if (!k) {
        DBGLOG(TPL_LOG " KILLCFG Monster class miss");
        return;
    }
    if (!g_strKlass) {
        auto& api = GetApi();
        void* corlib = api.GetCorlib();
        if (corlib) g_strKlass = api.ClassFromName(corlib, "System", "String");
        DBGLOG(TPL_LOG " KILLCFG strKlass=%p", g_strKlass);
    }
    std::vector<void*> v;
    Il2Cpp::GetApi().ScanInstances(k, v, 512);
    DBGLOG(TPL_LOG " KILLCFG scan hits=%zu", v.size());
    int n = 0, saved = 0, restored = 0, bad = 0;
    for (size_t i = 0; i < v.size() && n < 4096; i++) {
        const uintptr_t o = InstanceFrom(v[i], k);
        if (!o || !Il2Cpp::IsReadableRange(o, 264)) { bad++; continue; }
        if (!LooksLikeMonsterRow(o, k)) { bad++; continue; }
        if (i < 6 || n < 6) DBGLOG(TPL_LOG " MONROW o=%p id=%d perHp=%d", (void*)o, I32At(o + 16), I32At(o + 56));
        if (on) {
            if (g_monSavedN < 512) {
                g_monSaved[g_monSavedN].p = (void*)o;
                g_monSaved[g_monSavedN].perHp = I32At(o + 56);
                g_monSaved[g_monSavedN].inv = I32At(o + 84);
                g_monSaved[g_monSavedN].inf = I32At(o + 236);
                saved++;
                g_monSavedN++;
            }
            if (kDataWrites) {     // v23：只读观测轮不落字节
                SetI32(o + 56, 1);   // _perHp
                SetI32(o + 84, 0);   // _Invincible
                SetI32(o + 236, 0);  // _InfiniteBlood
            }
        }
        if (!g_monFirst) g_monFirst = o;
        n++;
    }
    if (!on && kDataWrites) {
        for (int i = 0; i < g_monSavedN; i++) {
            uintptr_t o = (uintptr_t)g_monSaved[i].p;
            if (!Il2Cpp::IsReadableRange(o, 256)) continue;
            SetI32(o + 56, g_monSaved[i].perHp);
            SetI32(o + 84, g_monSaved[i].inv);
            SetI32(o + 236, g_monSaved[i].inf);
            restored++;
        }
        g_monSavedN = 0;
        DBGLOG(TPL_LOG " KILLCFG restore rows=%d", restored);
        return;
    }
    DBGLOG(TPL_LOG " KILLCFG patch rows=%d scanned=%zu saved=%d", n, v.size(), saved);
}


// ============================================================================
// §⑫ v26 演员图（Actor Graph）：只用 hook 收到的**真实活体对象**走到演员/血条/DmgProp
//   —— 不扫堆、不走裸 ScanInstances（v20 崩因），也不装任何 Dobby inline hook。
//   依据 v25 FD 取证（全部来自活体对象实测）：
//     Buff.source@96 / Buff.target@104 = ActionGameLibrary.InteractiveGameObject（演员）
//     actor(InteractiveGameObject/ActionGameObject 系)：<isMain>@20 Bool、_data@32、
//                    _cdata@40(CharacterData)、_state@64(ActionState)、<DmgProp>@240(tagDmgProp)
//     ActionState: isDying@38、isDead@39、immuneDamage@60、immunePhysicalDamage@61、
//                  immuneMagicDamage@62、invincible@70
//     tagDmgProp:  lfDamageOwnerRate@144 Single
//     CharacterData：继承链字段已到 320+；属性字典 Dictionary<AttrType,Int64> 候选 @416
//   写入策略：一切目标先 IsReadableRange；每演员只在登记时做一次偏移自发现，热路径只裸读缓存。
// ============================================================================
static const bool kActorOn = true;
/* ★v29（2026-10-03 实测定案）：**演员内存写全部关闭**。
 *  用户症状（m00938）：「秒杀小怪会让小怪和首领直接血管清空，实际是假死……技能伤害增加打 boss 毫无变化」。
 *  取证结论：真凶不是 `Hurt` 钩子（`IHURT=0`，一次都没进），而是下面这块 v26 时代的「接收侧落地」——
 *   `if (kActorWrites && wantSkill)` 里 `SKILLHP` 分支把**非玩家目标**的 `_attributes[23]` 与 BigInt
 *   写成 `realMax / g_skillDmgRate`（rate=50）⇒ 怪物血量被直接压到 2%（「血管清空」）却没人杀它
 *   （「假死」），同时把 BigInt 的指数位写成 0 ⇒ 伤害判定读到畸形值 ⇒「打不中 / 选不到」。
 *  ⇒ 所有「写别人血量/属性」的动作一律关死；功能只走**挂号发现的方法**（`Hurt`/`DoAttack`/set_damage*）。
 *  保留 `kActorWritesInv`：玩家自身「免疫位」（`ActionState.immuneDamage` 家族）——这是本工程
 *  **唯一被实测证明有效**的写，且只写玩家自己的标志位，不碰任何非玩家对象。 */
static const bool kActorWrites = false;
static const bool kActorWritesInv = true;
/* 玩家属性表/血量内存写（`_attributes[23]=[22]` + BigInt 同步）：**★v30 重新打开（恢复 true）**。
 *  依据（v29b 血证 m01105）：`kActorWritesInv` 只写了玩家的免疫位字节（`INVULN` 日志 12 行、三个场景
 *  各 4 次、actor 就是当时的 `_player`），**写确实落地了**，但用户实测「无敌没效果了」
 *  ⇒ 免疫位字节**本身不足以无敌**；v26 时代真正让无敌生效的正是这条**玩家 HP 顶满**
 *  （`_attributes[23]=_attributes[22]` + `Character::CheckDead` 读的 BigInt 同步）。
 *  安全性：整块位于 `if (kActorWritesInv && wantInv && isPlayer)` 之内，而 `isPlayer` 已是严格
 *  `(g_playerPtr != 0) && (e.actor == g_playerPtr)`（v29 已删掉会把怪物判成玩家的 `roleId!=0` 回退）
 *  ⇒ **只会写玩家自己**，绝不会再出现「小怪 0 血不死」。 */
static const bool kActorWritesPlayerHp = true;
// v26e 性能闸：字段表已经取证完毕（ACT/CDATA/ASTATE/DMGPROP 全部入库），
//   而每登记一个演员就要打 ~200 行字段、并逐字段做 maps 校验（IsReadableRange 每次 fopen /proc/self/maps）
//   ⇒ 这就是用户看到的「每次进入游戏、获取角色数据中卡一下」的主因。默认关闭。
static const bool kDumpFields = false;

// 血量落点类型
enum { SK_I64 = 1, SK_I32 = 2, SK_F32 = 3, SK_F64 = 4, SK_BIG = 5 };

struct ActorSink {
    uintptr_t addr;  // SK_BIG 时为 BigInt 的 m(double) 地址
    uint8_t kind;
    char name[32];
};

struct ActorEnt {
    uintptr_t actor, kw, cdata, state, dmg, dict;
    char cls[40], ccls[40];
    int32_t isMain;
    size_t dataOff;      // 数据指针字段偏移（v26b：+40 可能为 null，实测 +368=_characterData）
    uint64_t roleId;     // cdata+112 ulRoleId（0 = 小怪/NPC，见 _钩子目标与语义.md）
    uint64_t teamId;     // cdata+552 CharacterData.teamId
    uint64_t ownerRole;  // cdata+120 ownerRoleId
    int32_t ctype;       // cdata+148 characterType（-1 = 读不到）
    int32_t hpKey;   // 属性字典里被观测为「血量」的 key（-1 未定）
    uintptr_t hpAddr;    // 血量值槽地址（缓存；每次写前仍 AddrOk 校验，防字典搬移）
    uint32_t dictReadMs; // 上次全量读字典的时刻（v26b：热路径 2s 一次，别每拍扫 100 条 entry）
    uint32_t sinkMs;     // 上次对落点做 maps 校验的时刻（v26e：AddrOk 每 2s 一次，其余拍子裸写）
    int64_t hpVal;
    int64_t hpMax;
    int32_t decConfirm;
    int32_t killTicks;
    uint8_t skillApplied;  // 全模式技能伤害增加：本轮已对本对象落地过一次
    uint8_t dmgBaseSet;    // v26j：dmgprop 基线只取一次（v26i 实测每拍都乘 → 50/2500/125000 越滚越大）
    float dmgBase;         // v26j：玩家 tagDmgProp.lfDamageOwnerRate 的原始值
    int64_t atkBase;       // v26m：玩家 _attributes[EN_ATTR_PHY_DMG] 的原始值（只取一次，避免每拍再乘）
    uint8_t atkBaseSet;    // v26m
    uint8_t killInvoked;   // v26q：已对该演员调过一次 Character::CheckDead（一次性，绝不复调）
    int32_t camp;          // v26s：GetServerCamp() 的原始值（== Hitter._camp，ConvertCamp 是恒等函数）
    uint8_t campSet;       // v26s：camp 已解析
    uintptr_t sa[4];       // v26t：本演员所有 CharacterData 候选的 _attributes 数据区（游戏读哪个我们就写哪个）
    uintptr_t saB[4];      // v26t：对应的 _attributesBigInt 数据区（Character::CheckDead 读 BigInt）
    int32_t saLen[4];      // v26t
    uint8_t saN;           // v26t：有效候选条数
    uint32_t saMs;         // v26t：上次解析时刻（解析失败时 5s 重试一次）
    int64_t ampBase[2];    // v26u：玩家 _attributes[21 DMG_AMPLIFY_PERCENT] / [24 DMG_AMPLIFY] 原始值
    uint8_t ampSet;        // v26u
    uint32_t writes;
    uint32_t lastVerifyMs;
    int32_t skN;
    ActorSink sk[8];
    bool dead;
    // 属性字典值缓存（用于观测「哪个 key 在下降」）
    int32_t ck[16];
    int64_t cv[16];
    int32_t cn;
};
static const uint32_t kActMax = 64;  // v26q：32 不够 —— v26p 实测 25/32 槽被特效/投射物占掉（ActorAccept 已收紧）
static ActorEnt g_act[kActMax];
static std::atomic<uint32_t> g_actN{0};
static bool g_roleAsPlayer = false;  // v26b：若整场没见过 isMain=1 的演员，改用 roleId≠0 判玩家
static bool g_sawMain = false;
static uint32_t g_noMainRounds = 0;
static char s_ab0[96];
static char s_ab1[96];
static char s_ab2[96];
static char s_ab3[96];

static bool AddrOk(uintptr_t p, size_t n) {
    return p && (p & 7u) == 0 && Il2Cpp::IsReadableRange(p, n);
}

/** 结构判定：+64→*State* 且（+40→*Data* 或 +240→*DmgProp*）→ 判为演员对象。
 *  两处独立校验，避免把「恰好有个指向别处的指针」的对象当成演员（v20 的假指针教训）。 */
static bool ActorAccept(uintptr_t o) {
    if (!o || (o & 7u) || !Il2Cpp::IsReadableRange(o, 0x100)) return false;
    /* v26q：对象自身的类名必须含 "Character"。
     *  v26p 实测血证：ACT+ 32 槽里 `ActionGameEffect` 占 21、`ActionGameProjectile` 占 4，
     *  真 `Character` 只有 5 个 ⇒ kActMax 饱和，真怪进不来；而特效/投射物的 `state` 也被我们
     *  写了 isDying/isDead（危险）。它们都不是 Character，类名一刀切掉最干净。 */
    const char* self = SafeObjType(o, s_ab3, sizeof(s_ab3));
    if (!self || !strstr(self, "Character")) return false;
    int score = 0;
    const uintptr_t st = PtrAt(o + 64);
    if (AddrOk(st, 0x60)) {
        const char* t = SafeObjType(st, s_ab0, sizeof(s_ab0));
        if (t && strcmp(t, "ActionState") == 0) return true;  // 最强特征：精确类名，直接定案
        if (t && strstr(t, "State")) score += 2;
    }
    const uintptr_t cd = PtrAt(o + 40);
    if (AddrOk(cd, 0x40)) {
        const char* t = SafeObjType(cd, s_ab1, sizeof(s_ab1));
        if (t && (strstr(t, "Data") || strstr(t, "Character"))) score += 1;
    }
    const uintptr_t dp = PtrAt(o + 240);
    if (AddrOk(dp, 0x98)) {
        const char* t = SafeObjType(dp, s_ab2, sizeof(s_ab2));
        if (t && strstr(t, "DmgProp")) score += 1;
    }
    return score >= 3;  // 必须「State」+ 至少一条旁证（避免 +64 恰好指向别的 *State* UI 对象）
}

// ============================ v26m：真·血量数据面（SecondaryAttribute） ============================
// 依据（全部来自官方热更 DLL Assembly-CSharp.dll 的元数据/IL，不是猜测）：
//   · CFLIST 实证：`InteractiveGameObjectData.secondaryAttribute off=360 type=ActionGameLibrary.SecondaryAttribute`
//     —— CharacterData 继承 InteractiveGameObjectData，所以 cdata+360 就是 SecondaryAttribute 引用。
//   · `callvirt get_healthPointNowBigInt()` 证明 SecondaryAttribute 是**引用类型**（class）⇒ +360 存指针。
//   · SecondaryAttribute 实例字段：`_attributes : long[]`、`_attributesBigInt : BigInt[]`、`_delegates`、
//     `_attributesChkMax : int[]`、`_shield_A/B`、`mainData`、`_hitSeed`、`tempDamageCumulativeNow/Max`。
//   · get_healthPointNow() (0x0601DF48) 的 IL 全文：
//         ldfld _attributes ; ldc.i4.s 23 ; ldelem.i8 ; ldc.i4 10000 ; conv.i8 ; div ; ret
//     ⇒ **当前 HP = _attributes[23] / 10000**；get_healthPoint() 用 22 ⇒ **最大 HP = _attributes[22] / 10000**。
//   · proto_ff.AttrType 枚举声明序（0 基）实证：22 = EN_ATTR_MAX_HP、23 = EN_ATTR_HP、
//     8 = EN_ATTR_PHY_DMG、9 = EN_ATTR_MGC_DMG（旁证：EN_ATTR_BONUS_ATK_NUMB 的 getter 用 53、
//     BONUS_NUMB_DEF 用 54，与枚举声明序完全对上）。
//   ⇒ 这条链是「官方自己的 CheckDead 判据」，比任何 HP 名字扫描都硬。
static const size_t kSecAttrOff = 360;  // InteractiveGameObjectData.secondaryAttribute（CFLIST 实证）
static const int kIdxMaxHp = 22;        // EN_ATTR_MAX_HP
static const int kIdxHp = 23;           // EN_ATTR_HP（CheckDead 判它 <= 0）
static const int kIdxPhyDmg = 8;        // EN_ATTR_PHY_DMG
static const int kIdxMgcDmg = 9;        // EN_ATTR_MGC_DMG
static const int64_t kAttrScale = 10000;  // _attributes 里存的是「真实值 × 10000」

static size_t g_saAttrOff = 0;    // SecondaryAttribute._attributes 字段偏移（按字段名解析一次并缓存）
static size_t g_saBigOff = 0;     // SecondaryAttribute._attributesBigInt 字段偏移
static int g_saAttrOffLogged = 0;

static inline void SetI64(uintptr_t p, int64_t v) { memcpy((void*)p, &v, 8); }

/** 从 CharacterData 取 `SecondaryAttribute._attributes`（long[]）的数据首址；失败返回 0。
 *  返回的是**数组数据区**（arr+0x20），因此 `data[i*8]` 就是 AttrType i 的原始值。
 *  outBig（可空）同时给出 `_attributesBigInt`（BigInt[]）的数据区 —— 元素步长 16，
 *  `BigInt{m_mantissa:double @0, m_exponent:int @8}`（见官方 BigInt [0x020000CB]）。
 *  ⚠️ `Character::CheckDead` 读的是 `get_healthPointNowBigInt()`（**BigInt 数组**），
 *  而 `set_healthPointNow()` 写的是 long 数组 ⇒ 两个都要写，游戏自己的判死才会立刻生效。 */
static uintptr_t SecAttrArr(uintptr_t cdata, int* outLen, uintptr_t* outBig) {
    if (outLen) *outLen = 0;
    if (outBig) *outBig = 0;
    if (!cdata || !AddrOk(cdata + kSecAttrOff, 8)) return 0;
    const uintptr_t sa = PtrAt(cdata + kSecAttrOff);
    if (!AddrOk(sa, 0x60)) return 0;
    char b[96];
    const char* san = SafeObjType(sa, b, sizeof(b));
    if (!san || !strstr(san, "SecondaryAttribute")) return 0;
    if (!g_saAttrOff) {
        auto& api = GetApi();
        void* k = api.object_class ? api.object_class((void*)sa) : nullptr;
        if (k && api.class_fields && api.field_name && api.field_offset) {
            void* it = nullptr;
            int guard = 0;
            while (void* f = api.class_fields(k, &it)) {
                const char* fn = api.field_name(f);
                if (fn && strcmp(fn, "_attributes") == 0) {
                    g_saAttrOff = api.field_offset(f);
                    break;
                }
                if (++guard >= 256) break;
            }
        }
        if (!g_saAttrOff) g_saAttrOff = 0x10;  // 声明序回退：_attributes 是第一个实例字段
        if (!g_saAttrOffLogged) {
            g_saAttrOffLogged = 1;
            DBGLOG(TPL_LOG " SECATTR _attributes off=0x%zx", g_saAttrOff);
        }
    }
    if (!AddrOk(sa + g_saAttrOff, 8)) return 0;
    const uintptr_t arr = PtrAt(sa + g_saAttrOff);
    if (!AddrOk(arr, 0x30)) return 0;
    const int32_t len = I32At(arr + 0x18);
    if (len < 24 || len > 4096) return 0;
    if (outLen) *outLen = len;
    if (outBig) {
        if (!g_saBigOff) {
            auto& api = GetApi();
            void* kk = api.object_class ? api.object_class((void*)sa) : nullptr;
            if (kk && api.class_fields && api.field_name && api.field_offset) {
                void* it = nullptr;
                int guard = 0;
                while (void* f = api.class_fields(kk, &it)) {
                    const char* fn = api.field_name(f);
                    if (fn && strcmp(fn, "_attributesBigInt") == 0) {
                        g_saBigOff = api.field_offset(f);
                        break;
                    }
                    if (++guard >= 256) break;
                }
            }
            if (!g_saBigOff) g_saBigOff = g_saAttrOff + 8;  // 声明序回退
        }
        const uintptr_t barr = AddrOk(sa + g_saBigOff, 8) ? PtrAt(sa + g_saBigOff) : 0;
        if (AddrOk(barr, 0x30)) {
            const int32_t blen = I32At(barr + 0x18);
            if (blen >= 24 && blen <= 4096) *outBig = barr + 0x20;
        }
    }
    return arr + 0x20;
}

/* v26t：把一个演员身上**所有** CharacterData 候选的 SecondaryAttribute 都解析出来。
 *  为什么必须多候选：`Character::CheckDead` 的 IL 第一行是
 *      `ldarg.0; ldfld ActionGameObject::_data(0x040170D8); isinst CharacterData`（**用 `_data`，不是 `_cdata`**），
 *  而 `SimpleCharacter` 另有一个 `_characterData@368`。v26s3 实测：只写 +368 那个对象时，
 *  `_attributes[8](phyDmg) ×50` 对伤害**毫无影响**，而 `GameFormula::GetResult` 明确用
 *  `phyDmg/armor/mgcDmg/resist` 算伤害 ⇒ 游戏读的是另一个对象。
 *  所以这里把 actor 上偏移 32..376 里所有「类名含 CharacterData」的指针字段全收进来，一律写。
 *  返回条数；outSa/outBig/outLen 与返回条数一一对应。 */
static int SecAttrAll(uintptr_t actor, uintptr_t cdata, uintptr_t* outSa, uintptr_t* outBig, int* outLen, int cap) {
    if (!kActorOn || !outSa || cap <= 0) return 0;
    uintptr_t cand[12];
    int cn = 0;
    if (cdata) cand[cn++] = cdata;
    if (AddrOk(actor, 0x40)) {
        for (size_t off = 32; off + 8 <= 384 && cn < 12; off += 8) {
            const uintptr_t p = PtrAt(actor + off);
            if (p < 0x10000) continue;
            bool dup = false;
            for (int i = 0; i < cn; i++)
                if (cand[i] == p) {
                    dup = true;
                    break;
                }
            if (dup || !AddrOk(p, 0x40)) continue;
            char b[96];
            const char* t = SafeObjType(p, b, sizeof(b));
            if (t && strstr(t, "CharacterData")) cand[cn++] = p;
        }
    }
    int n = 0;
    for (int i = 0; i < cn && n < cap; i++) {
        int len = 0;
        uintptr_t big = 0;
        const uintptr_t sa = SecAttrArr(cand[i], &len, &big);
        if (!sa) continue;
        bool dup = false;
        for (int j = 0; j < n; j++)
            if (outSa[j] == sa) {
                dup = true;
                break;
            }
        if (dup) continue;
        outSa[n] = sa;
        outBig[n] = big;
        if (outLen) outLen[n] = len;
        n++;
    }
    return n;
}

// ---- v26m：runtime_invoke 直调官方 setter（忠实复刻游戏自己的 Act_KillEnemy::KillEnenmy()）----
//  依据：`Act_KillEnemy::KillEnenmy()` 的 IL（0x060026AE）末尾就是
//      `characterData.secondaryAttribute.set_healthPointNow(0); CheckDead(true,false,false,false);`
//  自检实测（20:11:48 INVOKE 行，只读 getter，零副作用）：
//      mi1(get_healthPointNow) 解析成功、exc=0x0、boxed 返回值在 ret+0x10 → v=100
//      mi2(get_healthPointNowBigInt) 解析成功、exc2=0x0、boxed BigInt 在 ret+0x10 → mant=1 e=2 ⇒ 100
//      rawHp=1000000（= _attributes[23] = 100×10000）—— 三条读数完全一致
//  ⇒ **本游戏热更方法可以用 runtime_invoke 直调**（这条是 v26m 最关键的一次实证）。
//  ⚠️ 刻意**不** invoke `Character::CheckDead`：它的死亡序列会调 Unity 对象
//     （ModelUnity3DLegacy::AddSpeedLineEffect 等），而我们的 tick 线程不是 Unity 主线程。
//     改由「官方 setter + 原始数组归零 + isDying/isDead 状态位」三管齐下，
//     让游戏自己的伤害管线在主线程上完成收尾。
static std::atomic<int> g_attached{0};

static void EnsureIl2CppAttach() {
    if (g_attached.load(std::memory_order_relaxed)) return;
    auto& api = GetApi();
    if (api.thread_attach && api.domain_get) {
        void* d = api.domain_get();
        if (d) api.thread_attach(d);
    }
    g_attached.store(1, std::memory_order_relaxed);
}

/** v26w：★**彻底删除** `SaSetHpNow`（`runtime_invoke` 直调 `set_healthPointNow`）。
 *  v26v2 实测铁证（`%TEMP%\mxyzt_test_battle.txt`，2086680 行）：
 *    22:22:23.666  `SASET 异常 exc=0x… v=1743`（连续两次）
 *    22:22:23.680  最后一条 `Mod_Menu` 日志
 *    22:22:23.697  `CRASH signal 11 (SIGSEGV) code 1 (SEGV_MAPERR)`，tid 64740（= mod 线程）
 *        backtrace: #00 libil2cpp.so+0x2085ad8 → #01 +0x208e018 → #02 +0x22183b4 → #03 +0x22182e8
 *                   → #04 libstarry_magic.so+0x1a5c4（≈ 本函数，源码序倒排）→ #05 +0x16a58
 *                   → #06 +0x14e84 → #07 +0x12f58（MainModern mod_thread）
 *    之后 **`Forwarding signal 11` × 131233** + houdini 寄存器转储 ×375387
 *    ⇒ 该线程从此永远卡在「重执故障指令」循环：**一个核 100% 跑满 + logcat 洪水 = 用户看到的「掉帧很卡」**，
 *      并且**四项功能从此全部失效**（线程死了，tick 再也不跑）。
 *  根因：SKILLHP 分支把它写成 `SaSetHpNow(e.sa[ci] - 0x20, nv)` ——
 *    `e.sa[ci]` 是 `SecAttrArr()` 返回的 **`long[]` 数组数据区（arr+0x20）**，
 *    减 0x20 得到的是 **`long[]` 数组对象本身，而不是 `SecondaryAttribute` 对象**！
 *    ⇒ `object_class()` 拿到 `System.Int64[]` 的 klass ⇒ `GetMethodBySig` 拿到一个
 *      与 `Int64[]` 无关的 mi ⇒ `runtime_invoke(错 mi, 错 this, …)` ⇒ il2cpp 解释器内 SIGSEGV。
 *  为什么干脆全删而不是改对参数：官方 setter 做的事就是写 `_attributes[23]`，
 *  而我们已经**直接写** `_attributes[23]` + `_attributesBigInt[23]`（判死读的正是 BigInt 那个），
 *  效果完全等价（v26s3/v26t 实测：`SKILLHP … saHp=871710000 -> 1743` 落地、`KILLDEAD ok=1` 真死）。
 *  ⇒ 纯内存写足够，`runtime_invoke` 是**唯一的崩溃向量**，全部从热路径移除。
 *  目前只剩 `CdInvoke`（CheckDead）一处 invoke，且带熔断（见 SaCheckDead）。
 *
 *  历史教训（保留）：v26n 曾把 getter 返回的**装箱对象本身**当 `args[0]` 传进 `runtime_invoke`
 *  ⇒ il2cpp 把对象头第一个 8 字节（Int64 的 klass 指针）当成 long ⇒ `_attributes[23] = klass×10000 ≈ 1.377e18`
 *  （铁证 `KILLMOB … saHp=1377267289703200000`）。il2cpp 约定：**返回值是装箱对象（数据在 ret+0x10）；
 *  参数必须是指向裸值的指针**。 */

/** v26b：演员 → 数据对象的指针。
 *  v26 现场：`DataContainer._actionGameObject@32` 拿到的 SimpleCharacter，`+40`(ActionGameObject._cdata)
 *  为 **null**，而自身字段 `_characterData@368` = CharacterData（非 null）⇒ 单靠 +40 会漏掉整张演员表。
 *  做法：先试已知候选偏移，再在 +16..0x180 里按「类名含 CharacterData」自发现（每演员只做一次）。 */
static uintptr_t ActorDataPtr(uintptr_t a, size_t* offOut, char* cclsOut, size_t cap) {
    static const size_t kTry[] = {40, 368, 32, 48, 56};
    for (size_t i = 0; i < sizeof(kTry) / sizeof(kTry[0]); i++) {
        const uintptr_t p = PtrAt(a + kTry[i]);
        if (!AddrOk(p, 0x60)) continue;
        const char* t = SafeObjType(p, s_ab0, sizeof(s_ab0));
        if (t && (strstr(t, "CharacterData") || strstr(t, "GameObjectData"))) {
            if (offOut) *offOut = kTry[i];
            if (cclsOut && cap) snprintf(cclsOut, cap, "%s", t);
            return p;
        }
    }
    for (size_t off = 16; off + 8 <= 0x180; off += 8) {
        const uintptr_t p = PtrAt(a + off);
        if (!AddrOk(p, 0x60)) continue;
        const char* t = SafeObjType(p, s_ab0, sizeof(s_ab0));
        if (t && strstr(t, "CharacterData")) {
            if (offOut) *offOut = off;
            if (cclsOut && cap) snprintf(cclsOut, cap, "%s", t);
            DBGLOG(TPL_LOG " DATAOFF actor=%p 自发现数据指针 off=0x%zx cls=%s obj=%p", (void*)a, off, t,
                 (void*)p);
            return p;
        }
    }
    return 0;
}

/** 每个 label 只 dump 一次：把对象**全部**字段（含 >24）打到日志，供离线定案。
 *  这是 v25「每类 ≤24 字段」的补充：CharacterData/UI_HpBar 这类宽对象必须看全。 */
static void DumpAllFields(void* obj, const char* label, int cap) {
    if (!obj || !label) return;
    if (!kDumpFields) return;  // v26e：字段表已取证完毕 → 默认关（逐字段 maps 解析是卡顿主因之一）
    static char g_faDone[64][56];
    static std::atomic<uint32_t> g_faN{0};
    const uint32_t dn = g_faN.load(std::memory_order_relaxed);
    for (uint32_t i = 0; i < dn && i < 64; i++)
        if (strcmp(g_faDone[i], label) == 0) return;
    if (dn >= 64) return;
    snprintf(g_faDone[dn], sizeof(g_faDone[0]), "%s", label);
    g_faN.store(dn + 1, std::memory_order_relaxed);

    auto& api = Il2Cpp::GetApi();
    if (!api.object_class || !api.class_fields || !api.field_name || !api.field_offset) return;
    void* k = api.object_class(obj);
    if (!k) return;  // v26b：obj 可读但 klass 字为 0 时 class_fields(nullptr) 会崩
    const char* kn = (k && api.class_name) ? api.class_name(k) : "?";
    DBGLOG(TPL_LOG " ==== FIELDS %s class=%s obj=%p ====", label, kn ? kn : "?", obj);
    void* it = nullptr;
    int n = 0;
    while (void* f = api.class_fields(k, &it)) {
        const char* fn = api.field_name(f);
        const size_t off = api.field_offset(f);
        const char* tn = "?";
        if (api.field_type && api.type_name) {
            void* ft = api.field_type(f);
            if (ft) {
                char* s = api.type_name(ft);
                if (s && Il2Cpp::IsReadableRange((uintptr_t)s, 2)) tn = s;
            }
        }
        uint64_t v0 = 0, v1 = 0;
        if (off < 0x800 && Il2Cpp::IsReadableRange((uintptr_t)obj + off, 16)) {
            v0 = *(const uint64_t*)((const char*)obj + off);
            v1 = *(const uint64_t*)((const char*)obj + off + 8);
        }
        DBGLOG(TPL_LOG "  FL %s.%s off=%zu type=%s v0=%llx v1=%llx", label, fn ? fn : "?", off, tn,
             (unsigned long long)v0, (unsigned long long)v1);
        if (++n >= cap) break;
    }
    DBGLOG(TPL_LOG " ==== FIELDS %s end n=%d ====", label, n);
}

/** 找「血量」命名落点：名字含 hp/HP/Hp 且不是 max/totle 那类上限字段。 */
static void ScanHpFields(uintptr_t obj, const char* label, ActorEnt* e) {
    if (!obj || !e) return;  // ★v26b 崩溃修复：obj==0 时 il2cpp_object_get_class(nullptr) → SIGSEGV
    auto& api = Il2Cpp::GetApi();
    if (!api.object_class || !api.class_fields || !api.field_name || !api.field_offset) return;
    // v26e：字段区一次校验覆盖（原来逐字段 IsReadableRange = 逐字段 fopen /proc/self/maps）
    if (!Il2Cpp::IsReadableRange(obj, 0x800)) return;
    void* k = api.object_class((void*)obj);
    if (!k) return;
    void* it = nullptr;
    int n = 0;
    while (void* f = api.class_fields(k, &it)) {
        const char* fn = api.field_name(f);
        const size_t off = api.field_offset(f);
        if (fn && off < 0x800 && (strstr(fn, "hp") || strstr(fn, "HP") || strstr(fn, "Hp")) &&
            !strstr(fn, "max") && !strstr(fn, "Max") && !strstr(fn, "totle") && !strstr(fn, "Totle")) {
            const char* tn = "?";
            if (api.field_type && api.type_name) {
                void* ft = api.field_type(f);
                if (ft) {
                    char* s = api.type_name(ft);
                    if (s && Il2Cpp::IsReadableRange((uintptr_t)s, 2)) tn = s;
                }
            }
            int kind = 0;
            size_t need = 8;
            if (strstr(tn, "Int64") || strstr(tn, "UInt64")) { kind = SK_I64; need = 8; }
            else if (strstr(tn, "Int32") || strstr(tn, "UInt32")) { kind = SK_I32; need = 4; }
            else if (strstr(tn, "Single")) { kind = SK_F32; need = 4; }
            else if (strstr(tn, "Double")) { kind = SK_F64; need = 8; }
            else if (strstr(tn, "BigInt") || strstr(tn, "MsgInt128") || strstr(tn, "BigInteger")) { kind = SK_BIG; need = 16; }
            if (kind && e->skN < 8 && off + need <= 0x800) {  // v26e：已一次校验过 [obj,obj+0x800)
                ActorSink& s = e->sk[e->skN++];
                s.addr = obj + off;
                s.kind = (uint8_t)kind;
                snprintf(s.name, sizeof(s.name), "%s.%s", label, fn);
                DBGLOG(TPL_LOG " HPFIELD %s off=%zu type=%s kind=%d v=%llx", s.name, off, tn, kind,
                     (unsigned long long)U64At(obj + off));
            }
        }
        if (++n >= 96) break;
    }
}

/** 读 Dictionary<AttrType,Int64> 的 (key,value) 对。返回对数；-1=布局不符。
 *  IL2CPP Dictionary 字段：buckets@0x10 entries@0x18 count@0x20；Entry 步长 24
 *  （hashCode@0 next@4 key@8 value@16；hashCode<0 = 空闲槽）。 */
static int AttrRead(uintptr_t dict, int32_t* keys, int64_t* vals, int cap) {
    if (!AddrOk(dict, 0x28)) return -1;
    const uintptr_t arr = PtrAt(dict + 0x18);
    const int32_t cnt = I32At(dict + 0x20);
    if (!AddrOk(arr, 0x28) || cnt < 0 || cnt > 4096) return -1;
    const int32_t alen = I32At(arr + 0x18);
    if (alen < cnt || alen > 65536) return -1;
    const uintptr_t data = arr + 0x20;
    // ★v26b 成本：整段一次校验（原来每条 entry 一次 IsReadableRange = 每次 maps 解析，热路径杀手）
    if (!Il2Cpp::IsReadableRange(data, (size_t)cnt * 24)) return -1;
    int m = 0;
    for (int i = 0; i < cnt && m < cap; i++) {
        const uintptr_t ep = data + (size_t)i * 24;
        if (I32At(ep) < 0) continue;
        const int32_t key = I32At(ep + 8);
        if (key < 0 || key > 4096) continue;
        keys[m] = key;
        vals[m] = (int64_t)U64At(ep + 16);
        m++;
    }
    return m;
}

/** 属性字典里某 key 的 value 槽地址（写血量用）。 */
static uintptr_t AttrValueAddr(uintptr_t dict, int32_t key) {
    if (!AddrOk(dict, 0x28)) return 0;
    const uintptr_t arr = PtrAt(dict + 0x18);
    const int32_t cnt = I32At(dict + 0x20);
    if (!AddrOk(arr, 0x28) || cnt < 0 || cnt > 4096) return 0;
    const uintptr_t data = arr + 0x20;
    if (!Il2Cpp::IsReadableRange(data, (size_t)cnt * 24)) return 0;  // ★v26b 整段一次校验
    for (int i = 0; i < cnt; i++) {
        const uintptr_t ep = data + (size_t)i * 24;
        if (I32At(ep) < 0) continue;
        if (I32At(ep + 8) == key) return ep + 16;
    }
    return 0;
}

/** v26t：**已删除** `SetDictAll`（v26j 的「属性字典全量写 1」）。
 *  v26s3 血证：它在 246 次调用里毫无可见收益（`sinks=0` 恒成立、怪不会因此死），
 *  却是那记 SIGSEGV 的头号嫌疑 —— 21:31:53 `x0 = 0101010100000000`（字节全 0x01）
 *  正是「往某个错位置写 int64 1」的指纹，`x19` 又恰是玩家 state 对象基址。
 *  真正的杀怪链路已经由 `_attributes[23]=0` + BigInt 归零 + 官方 `CheckDead` 三件套覆盖，删掉零风险。 */

static void AttrDumpLog(ActorEnt* e) {
    if (!e->dict) return;
    int32_t k[48];
    int64_t v[48];
    const int m = AttrRead(e->dict, k, v, 48);
    if (m <= 0) {
        DBGLOG(TPL_LOG " ATTR %s dict=%p 读取失败 m=%d（布局不符，需离线校正）", e->cls, (void*)e->dict, m);
        return;
    }
    char line[700];
    int p = snprintf(line, sizeof(line), " ATTR %s dict=%p n=%d:", e->cls, (void*)e->dict, m);
    for (int i = 0; i < m; i++) {
        p += snprintf(line + p, sizeof(line) - (size_t)p, " %d=%lld", k[i], (long long)v[i]);
        if (p >= (int)sizeof(line) - 40) break;
    }
    DBGLOG(TPL_LOG "%s", line);
}

/** v26l：字段名像「集合/目标/角色列表」时，才值得多做一次对象类名解析去判断它是不是 List。
 *  纯 strstr，零 il2cpp API、零 maps 解析。 */
static bool FieldLooksLikeList(const char* fn) {
    if (!fn) return false;
    static const char* const kW[] = {"target", "Target", "List", "list", "enemy", "Enemy",
                                     "actor",  "Actor",  "player", "Player", "character", "Character",
                                     "units",  "Units",  "members", "Members"};
    for (size_t i = 0; i < sizeof(kW) / sizeof(kW[0]); i++)
        if (strstr(fn, kW[i])) return true;
    return false;
}

/** 字段蜘蛛：把「战场容器」类对象（DataContainer/BattleElementCenter/GameObjectManager/
 *  血条 UI…）的指针字段逐个送 ActorAccept，命中即登记（每个类名只做一次）。 */
static void ActorRecord(uintptr_t a);  // 前向（SpiderFields/ActorOffer 互调）

static void SpiderFields(uintptr_t obj, const char* tname) {
    if (!obj || !tname) return;
    static char s_sp[32][40];
    static std::atomic<uint32_t> s_spN{0};
    const uint32_t dn = s_spN.load(std::memory_order_relaxed);
    for (uint32_t i = 0; i < dn && i < 32; i++)
        if (strcmp(s_sp[i], tname) == 0) return;
    if (dn >= 32) return;
    snprintf(s_sp[dn], sizeof(s_sp[0]), "%s", tname);
    s_spN.store(dn + 1, std::memory_order_relaxed);
    auto& api = Il2Cpp::GetApi();
    if (!api.object_class || !api.class_fields || !api.field_offset) return;
    void* k = api.object_class((void*)obj);
    if (!k) return;
    void* it = nullptr;
    int n = 0, found = 0;
    static char s_spl[64];
    while (void* f = api.class_fields(k, &it)) {
        const size_t off = api.field_offset(f);
        if (off < 0x400) {
            const uintptr_t p = PtrAt(obj + off);
            const char* fn = api.field_name ? api.field_name(f) : "?";
            if (!fn) fn = "?";
            if (AddrOk(p, 0x100) && ActorAccept(p)) {
                DBGLOG(TPL_LOG " SPIDER %s.%s off=%zu -> actor %p", tname, fn, off, (void*)p);
                found++;
                ActorRecord(p);
            } else if (AddrOk(p, 0x100) && FieldLooksLikeList(fn)) {
                /* v26l：容器类对象（GameObjectManager/BattleElementCenter/DataContainer）里的
                 *   演员列表不是裸指针而是 `List<IActionGameObject>`，ActorAccept 不可能通过。
                 *   字段名像集合时才多做一次类名解析（SafeObjType 要读 maps，不做无谓开销），
                 *   是 List 就按 `_items@0x10 / _size@0x18 / 数组长度@+0x18 / 数据@+0x20` 展开。 */
                const char* pt = SafeObjType(p, s_spl, sizeof(s_spl));
                if (pt && strstr(pt, "List")) {
                    const uintptr_t items = PtrAt(p + 0x10);
                    const int sz = (int)PtrAt(p + 0x18);
                    if (items && sz > 0 && sz <= 64) {
                        const int alen = (int)PtrAt(items + 0x18);
                        if (alen >= sz && alen <= 1024) {
                            int lf = 0;
                            for (int i = 0; i < sz; i++) {
                                const uintptr_t el = PtrAt(items + 0x20 + 8 * (uintptr_t)i);
                                if (AddrOk(el, 0x100) && ActorAccept(el)) {
                                    DBGLOG(TPL_LOG " SPIDER %s.%s[%d] off=%zu -> actor %p", tname, fn, i, off,
                                         (void*)el);
                                    found++;
                                    lf++;
                                    ActorRecord(el);
                                }
                            }
                            if (lf) DBGLOG(TPL_LOG " SPIDER %s.%s list size=%d 命中=%d", tname, fn, sz, lf);
                        }
                    }
                }
            }
        }
        if (++n >= 96) break;
    }
    if (found) DBGLOG(TPL_LOG " SPIDER %s done found=%d", tname, found);
    else s_spN.store(dn, std::memory_order_relaxed);  // v26l：没捞到就撤销去重，允许以后再试
}

// ActorRecord 定义见下（已在上方前向声明）

/** v26 成本闸：ActorOffer 每次都要 maps 解析（SafeObjType 内部 3 次），
 *  而解释器名册一次会话能给出 2000+ 个新名字 × 最多 6 个实参 ⇒ 必须限流，
 *  否则重演 v10「探针把游戏拖死」。做法：指针去重环 + 总预算。 */
static uintptr_t g_offerSeen[256];
static uint32_t g_offerSeenIdx = 0;
static std::atomic<uint32_t> g_offerN{0};

static bool OfferSeen(uintptr_t p) {
    for (uint32_t i = 0; i < 256; i++)
        if (g_offerSeen[i] == p) return true;
    g_offerSeen[g_offerSeenIdx++ & 255u] = p;
    return false;
}

/** v26b 成本闸：先按类名粗筛（纯 strstr，零 il2cpp API / 零 maps 解析）。
 *  解释器名册一次会话给出 2000+ 新名字 × ≤6 实参，若每个都做 ActorAccept（3 次 maps 解析）
 *  就是重演 v10「探针把游戏拖死」；只放行「演员类 / 战场容器类 / Buff」。 */
static bool OfferInteresting(const char* t) {
    if (!t) return false;
    static const char* const kW[] = {"Buff",           "Character",     "Monster",
                                     "DataContainer",  "BattleElementCenter", "GameObjectManager",
                                     "InteractiveGameObject", "ActionGameObject", "AvatarBody",
                                     "BattleStyle",    "Hitter",        "HpBar",
                                     "BattleJudge",    "DataModel"};
    for (size_t i = 0; i < sizeof(kW) / sizeof(kW[0]); i++)
        if (strstr(t, kW[i])) return true;
    return false;
}

/** 演员图入口：所有输入都来自 hook 收到的真实对象。 */
static void ActorOffer(void* obj, const char* tname) {
    if (!kActorOn || !obj) return;
    const uintptr_t o = (uintptr_t)obj;
    if (!o || (o & 7u)) return;
    if (!OfferInteresting(tname)) return;
    const bool isBuff = (tname && strcmp(tname, "Buff") == 0);
    if (OfferSeen(o) && !isBuff) return;
    // 1) Buff → 直接给出 source/target（v25 已实测：target@104 就是演员）
    if (isBuff) {
        const uintptr_t src = PtrAt(o + 96), dst = PtrAt(o + 104);
        if (AddrOk(src, 0x100) && ActorAccept(src)) ActorRecord(src);
        if (AddrOk(dst, 0x100) && ActorAccept(dst)) ActorRecord(dst);
        return;
    }
    // 预算闸（Buff 不受限，它是最可靠的入口）
    if (g_offerN.fetch_add(1, std::memory_order_relaxed) > 2500) return;
    if (!AddrOk(o, 8)) return;
    // 2) 它本身就是演员
    if (ActorAccept(o)) {
        ActorRecord(o);
        return;
    }
    // 3) 战场容器/血条 → 字段蜘蛛（把玩家对象也捞出来）
    if (!tname) return;
    if (strstr(tname, "DataContainer") || strstr(tname, "BattleElementCenter") ||
        strstr(tname, "GameObjectManager") || strstr(tname, "DataModel") ||
        strstr(tname, "BattleJudge") || strstr(tname, "Hitter") || strstr(tname, "HpBar") ||
        strstr(tname, "AvatarBody") || strstr(tname, "BattleStyle")) {
        DumpAllFields((void*)o, tname, 64);
        SpiderFields(o, tname);
    }
}

// ---- v26s：阵营（官方 GetServerCamp 的原值）—— 秒杀小怪的安全闸，防止误伤自己/队友/宠物 ----
//  依据（DLL IL 实证）：
//    `InteractiveGameObject::GetServerCamp()` 0x0601E0B2 = `CampTool::ConvertCamp(get_camp())`，
//    而 `CampTool::ConvertCamp(int)` 0x06000B90 的 IL 只有 `ldarg.0; ret`（**恒等函数**）
//    ⇒ 直接比原始 camp 与官方比较**完全等价**，不需要 invoke。
//    `get_camp()` 0x0601E0BB = `_hitter == null ? 0 : _hitter._camp`；
//    `Hitter._camp` 是 Hitter 第 2 个实例字段（`_interactiveGameObject`@0x10、`_camp`@0x18）。
static size_t g_hitterOff = 0;
/* v31：`g_playerCamp` / `g_playerCampSet` 已上移到文件上方（`ApplyHurtPolicy` 之前的 v31 块）。
 *  v26t：玩家阵营是否**已解析**的独立标志。
 *  v26s3 血证：玩家阵营的合法值就是 **0**，而老闸门写成 `g_playerCamp != 0` ⇒ 直接被关掉、
 *  形同虚设，结果把自己人也秒了（「首领被秒 + 不结算 + 转圈圈」的真凶）。 */

static size_t HitterFieldOff() {
    if (g_hitterOff) return g_hitterOff;
    auto& api = GetApi();
    void* k = ResolveClassPref("InteractiveGameObject");
    if (!k || !api.class_get_field || !api.field_offset) return 0;
    void* f = api.class_get_field(k, "_hitter");
    if (!f) return 0;
    g_hitterOff = api.field_offset(f);
    DBGLOG(TPL_LOG " CAMP _hitter off=0x%zx", g_hitterOff);
    return g_hitterOff;
}

/* ===================== v31 三个 helper（照斌哥机制 + IL 定案）=====================
 * IL 依据见本文件上方 `ApplyHurtPolicy` 前的 v31 说明块，要点：
 *  ① `ActionGameObject::_state`（FieldDef 0x040170DC）—— 替掉硬编码 +64（v26s 实测值，保留为回退）。
 *  ② `InteractiveGameObject::_hitter`（FieldDef 0x04017109）—— 无敌「accepter 置 null」的精确判据：
 *     `StarsCollider::DoAttack` 的第 2 个 IL 参数 `IAccept accepter` 的真实对象就是
 *     `_player._hitter`。
 *  ③ `StarsCollider::localDamage`（FieldDef 0x0401764D）—— #3「全模式」：
 *     `aot/_tgt_ld.txt:17-26`（`StarsCollider::DoAttack 0x0601E8D9`）逐字：
 *       IL_03CC: ldarg.0
 *       IL_03CD: ldfld  0x0401764D StarsCollider::localDamage : bool
 *       IL_03D2: brtrue.s 17                       // localDamage → 客户端权威路径
 *       IL_03D4: ldsfld 0x04017507 ActionGameLibraryConfig::IsEditorMode
 *       IL_03D9: brtrue.s 10
 *       IL_03DB: call   0x0601E7FB BattleElementCenter::get_localDamage() -> bool
 *     而 `get_localDamage`（0x0601E7FB）本体 = `ldsfld collider; ldfld localDamage` ⇒ **同一个字段**。
 *     ⇒ 直接把这个字段写 1 比挂 getter 更彻底（`DoAttack` 读的是字段本身，根本不走 getter），
 *       等价于斌哥壳的 `[冒险-关卡模式#%d] localDamage=%d (1=客户端权威)`。
 *     实例来源：`BattleElementCenter::collider`（**静态** FieldDef 0x04017537）。
 * ============================================================================= */

static size_t g_stateOffCacheV31[8][2] = {};  // {klass, off} 按类缓存

/** ★v34：按**字段名**解析 `ActionState` 实例字段的偏移（klass+id 缓存 8 项）。
 *  残留 bug（全面自查结论之三）：`ActorApplyTick` 的无敌块写的是硬编码
 *  `e.state + 60/61/62/63/70/76`，**漏了 `g_deltaA` 修正** —— 而读同一批字段的日志用的是
 *  `st + 38 + g_deltaA`。即那几个 1 一直写进了**别的状态字节**，玩家免疫位从未真正置上
 *  ⇒ 这就是「开无敌还会掉血」的成因之一。改为按名字解析（失败再退回「旧偏移 + g_deltaA」）。 */
struct StateFldCacheV34 {
    const void* k;
    size_t off;
    int id;
};
static StateFldCacheV34 g_stateFldV34[8] = {};
/* ★v47：本函数**重新启用**（v34 之后一直挂着 `__attribute__((unused))`）—— 见下方
 *  `InvStateWriteV47`：无敌的免疫位改成「**按字段名优先**、按名失败才退回旧硬偏移」。 */
static size_t StateFieldOffV34(void* state, const char* name, int id) {
    if (!state || !name) return 0;
    auto& api = Il2Cpp::GetApi();
    if (!api.object_class) return 0;
    void* k = api.object_class(state);
    if (!LivePtr(k)) return 0;
    for (int i = 0; i < 8; i++)
        if (g_stateFldV34[i].k == k && g_stateFldV34[i].id == id) return g_stateFldV34[i].off;
    const size_t off = api.FieldOffset(k, name, 0);
    const bool ok = (off > 0 && off <= 0x400);
    for (int i = 0; i < 8; i++)
        if (!g_stateFldV34[i].k) {
            g_stateFldV34[i].k = k;
            g_stateFldV34[i].off = ok ? off : 0;
            g_stateFldV34[i].id = id;
            break;
        }
    if (ok) {
        static std::atomic<uint32_t> s_log{0};
        if (s_log.fetch_add(1, std::memory_order_relaxed) < 12)
            DBGLOG(TPL_LOG " V34 state 字段 %s +0x%zx cls=%s", name, off, api.class_name(k));
    }
    return ok ? off : 0;
}

/** ★v47 —— #2 无敌的免疫位写入：**按字段名优先，按名失败才退回旧硬偏移**。
 *
 *  为什么必须做这一步（把仓库里已有的两条自相矛盾的记录摆在一起就看出来了）：
 *   · 本文件 §v31 的 IL 定案注释（**元数据级证据**）写明：
 *     「`CBattleActorUtil::IsDamageImmune 0x0601E262` 只读 `target.get_state().immuneDamage`
 *       （FieldDef **0x04017E2D**）⇒ 置该位后 `Hurt` 立刻 `SetValue(0)`」；
 *     —— 即**游戏真正读的**是 `state` 对象上名为 `immuneDamage` 的那个字段。
 *   · 而本文件 v34 的「全面自查结论之三」写明：`ActorApplyTick` 的无敌块写的是**硬编码**
 *     `e.state + 60/61/62/63/70/76`，并且那几个 1「一直写进了**别的状态字节**，
 *     玩家免疫位从未真正置上 ⇒ 这就是『开无敌还会掉血』的成因之一」。
 *  ⇒ 结论：硬偏移**不可信**（写错位置 = 既没生效、又污染了无关状态字节——后者本身就是闪退/失控
 *     的一类种子），而按名字段解析是**有元数据依据**的那条路。
 *
 *  行为（三级，全部 fail-safe）：
 *   ① 按名解析 `immuneDamage` 成功 ⇒ 只写**解析出来的**那几个字段（同族的名字各自解析，
 *      解析不到的就不写），**完全不碰**旧硬偏移；
 *   ② 按名解析失败 ⇒ 退回旧硬偏移写法（保留 v46b「已验证可用」的行为，不做无证据的削弱）；
 *   ③ `e.state` 本身不可读/未解析 ⇒ 一个字节都不写。
 *  无论哪一级，写之前都要求**目标地址可读**（`recheck` 为假时用零 I/O 的区间判据兜一道）。 */
static void InvStateWriteV47(void* state, bool recheck) {
    if (!state) return;
    auto& api = Il2Cpp::GetApi();
    const uintptr_t st = (uintptr_t)state;
    if (!starptr::IsAligned(st) || !starptr::InUserVa(st)) return;
    if (recheck && !AddrOk(st, 0x60)) return;
    /* ① 按名解析（结果按类缓存；`StateFieldOffV34` 内部自带 8 项缓存与范围闸） */
    static const char* const kNames[5] = {"immuneDamage", "immunePhysicalDamage", "immuneMagicDamage",
                                          "immuneDebuff", "invincible"};
    static const int kIds[5] = {101, 102, 103, 104, 105};
    size_t offs[5] = {0, 0, 0, 0, 0};
    int have = 0;
    for (int i = 0; i < 5; i++) {
        offs[i] = StateFieldOffV34(state, kNames[i], kIds[i]);
        if (offs[i]) have++;
    }
    if (offs[0]) {  // `immuneDamage` 是 IL 里被读的那一个 ⇒ 它解析成功才认「按名这条线成立」
        static std::atomic<uint32_t> s_byName{0};
        for (int i = 0; i < 5; i++) {
            if (!offs[i]) continue;
            if (recheck && !AddrOk(st + offs[i], 1)) continue;
            SetByte(st + offs[i], 1);
        }
        const uint32_t n = s_byName.fetch_add(1, std::memory_order_relaxed);
        if (n < 8)
            DBGLOG(TPL_LOG
                 " V47 INV 按名写免疫位 state=%p immuneDamage=+0x%zx(+%d个同族) 旧硬偏移未使用",
                 state, offs[0], have);
        return;
    }
    /* ② 按名失败 ⇒ 退回 v46b 的硬偏移（并在日志里说清走的是哪条路） */
    static std::atomic<uint32_t> s_byOff{0};
    const uint32_t m = s_byOff.fetch_add(1, std::memory_order_relaxed);
    if (m < 8)
        LOGW(TPL_LOG
             " V47 INV 按名解析不到 immuneDamage → 退回旧硬偏移 60/61/62/63/70/76（v46b 行为）state=%p",
             state);
    if (recheck ? AddrOk(st + 60, 4) : true) {
        SetByte(st + 60, 1);  // immuneDamage
        SetByte(st + 61, 1);  // immunePhysicalDamage
        SetByte(st + 62, 1);  // immuneMagicDamage
        SetByte(st + 63, 1);  // immuneDebuff
    }
    if (recheck ? AddrOk(st + 70, 1) : true) {
        SetByte(st + 70, 1);  // invincible
        SetByte(st + 76, 1);  // v33 原样的第二枚
    }
}

/** `ActionGameObject::_state` 偏移；解析失败回退 v26s 实测的 64。 */
static size_t StateOffV31(void* obj) {
    auto& api = Il2Cpp::GetApi();
    if (!LivePtr(obj) || !api.object_class) return 64;
    void* k = api.object_class(obj);
    if (!LivePtr(k)) return 64;
    for (int i = 0; i < 8; i++)
        if (g_stateOffCacheV31[i][0] == (size_t)k) return g_stateOffCacheV31[i][1];
    const size_t off = api.FieldOffset(k, "_state", 0);
    if (off && off <= 0x400) {
        for (int i = 0; i < 8; i++)
            if (!g_stateOffCacheV31[i][0]) {
                g_stateOffCacheV31[i][0] = (size_t)k;
                g_stateOffCacheV31[i][1] = off;
                break;
            }
        DBGLOG(TPL_LOG " V31 _state +0x%zx cls=%s（v26s 硬编码 64）", off,
             api.class_name ? api.class_name(k) : "?");
        return off;
    }
    return 64;
}

/** 玩家 `_hitter`：无敌「accepter 置 null」只认这一个指针；认不到就一个字节都不写。 */
static void PlayerHitterEnsureV31() {
    static std::atomic<uint32_t> s_log{0};
    const uintptr_t pl = g_playerPtr;
    if (!pl) {
        g_playerHitterV31.store(0, std::memory_order_relaxed);
        return;
    }
    size_t ho = g_offHitterV31.load(std::memory_order_relaxed);
    if (!ho) {
        ho = HitterFieldOff();
        if (ho) g_offHitterV31.store(ho, std::memory_order_relaxed);
    }
    if (!ho) return;
    const uintptr_t h = PtrAt(pl + ho);
    if (!AddrOk(h, 0x20)) {
        g_playerHitterV31.store(0, std::memory_order_relaxed);
        return;
    }
    const uintptr_t old = g_playerHitterV31.exchange(h, std::memory_order_relaxed);
    if (old != h && s_log.fetch_add(1, std::memory_order_relaxed) < 12)
        DBGLOG(TPL_LOG " V31 playerHitter _player=%p _hitter=%p off=0x%zx", (void*)pl, (void*)h, ho);
}

/** ★v33：读某个演员的 `InteractiveGameObject::_hitter`（= 它的 `IAccept` 实现 `Hitter` 对象）。
 *  玩家侧判定的第 ③ 号判据靠它：`受击者._hitter == _player._hitter`。 */
static uintptr_t ActorHitterV32(uintptr_t actor) {
    if (!actor) return 0;
    size_t ho = g_offHitterV31.load(std::memory_order_relaxed);
    if (!ho) {
        ho = HitterFieldOff();
        if (ho) g_offHitterV31.store(ho, std::memory_order_relaxed);
    }
    if (!ho) return 0;
    const uintptr_t h = PtrAt(actor + ho);
    return AddrOk(h, 0x20) ? h : 0;
}

/** ★v36/v39：读任意 actor 的 `CharacterData.characterType` —— 就是斌哥串池 @0xb310e
 *  `[冒险-打怪#%d] ctype=%d …` 里的那个 `ctype`（菜单「秒杀小怪」就是靠它把首领排除）。
 *  v36 用 `g_offCdata`（旧标定量）读，实测**恒返回 -1**（v36/v37/v38 日志里 `ctype=-1`）⇒ 改成按字段名读：
 *    · 元数据铁证：`SimpleCharacter::get_characterData() 0x0601E0CF` 的 IL 是
 *      `IL_0001 ldfld 0x0401710F field SimpleCharacter::_characterData : CharacterData`
 *      ⇒ 字段名 **`_characterData`**（`api.FieldOffset` 走运行时 API，给的偏移直接可用）；
 *    · 枚举铁证：`CharacterType 0x020000F2`（field 序即枚举序）= PLAYER, APC, **MONSTER**, **BOSS**, …
 *      ⇒ 推测 MONSTER=2、BOSS=3（C# 默认从 0 递增）；真值由日志里的 `ctype=` 现场核对。
 *  读不到返回 -1。 */
static int32_t CtypeOfV36(void* obj) {
    if (!obj) return -1;
    auto& api = Il2Cpp::GetApi();
    if (!api.object_class || !api.class_name) return -1;
    void* k = api.object_class(obj);
    if (!LivePtr(k)) return -1;
    static const void* s_k = nullptr;
    static size_t s_off = 0;
    if (s_k != k) {
        size_t o = api.FieldOffset(k, "_characterData", 0);
        if (!o) o = api.FieldOffset(k, "characterData", 0);
        if (!o && g_offCdata) o = (size_t)g_offCdata;
        s_k = k;
        s_off = o;
        DBGLOG(TPL_LOG " V39 ctype: cls=%s _characterData=+0x%zx", api.class_name(k), o);
    }
    if (!s_off || s_off > 0x800) return -1;
    const uintptr_t cd = PtrAt((uintptr_t)obj + s_off);
    if (!AddrOk(cd, 0x40)) return -1;
    void* cdk = api.object_class(reinterpret_cast<void*>(cd));
    static const void* s_ck = nullptr;
    static size_t s_coff = 0;
    if (LivePtr(cdk) && s_ck != cdk) {
        size_t o = api.FieldOffset(cdk, "characterType", 0);
        if (!o) o = 148 + (size_t)g_deltaC;
        s_ck = cdk;
        s_coff = o;
        DBGLOG(TPL_LOG " V39 ctype: dataCls=%s characterType=+0x%zx", api.class_name(cdk), o);
    }
    if (!s_coff || s_coff > 0x800) return -1;
    const uintptr_t fp = cd + s_coff;
    if (!Il2Cpp::IsReadableRange(fp, 4)) return -1;
    return I32At(fp);
}

/** ★v40 —— 现场读任意 actor 的 `ActionGameObjectData.ulRoleId`（**#1 的身份判据换成它**）。
 *  用户 m02805：「打完一个首领后，1 的效果就没了」。v39 日志血证：首领战后
 *  `打怪#8/#9/#80/…/#800` 全是 `合法=0 … plH=0x0`（`atkH` 非 0）⇒ `g_playerHitterV31`
 *  被刷成 0，`IsPlayerActorV32` 的指针历史表也过期 ⇒ **两条旧判据同时失效且不自恢复**。
 *
 *  元数据铁证：`ActionGameLibrary.ActionGameObjectData :: ulRoleId : ulong 0x04017D2F`
 *  （`CharacterData` 继承它；`characterType 0x04017D35` 同属该类，而它按名解析成功 = +0x94 ⇒
 *  `api.FieldOffset` 确实会向上搜继承链，`ulRoleId` 同样能解析）。
 *  血证：玩家侧（分身 `SimpleCharacter` 与真正承伤的战斗 `Character`）roleId 都是
 *  131202533413845712，而小怪/首领恒 0 ⇒ **非 0 且相等 = 玩家侧**，每笔命中现读，
 *  不依赖任何会被场景刷新清掉的缓存。读不到返回 0。 */
static uint64_t RoleIdOfV40(void* obj) {
    if (!obj) return 0;
    auto& api = Il2Cpp::GetApi();
    if (!api.object_class || !api.class_name) return 0;
    void* k = api.object_class(obj);
    if (!LivePtr(k)) return 0;
    static const void* s_k = nullptr;
    static size_t s_off = 0;
    if (s_k != k) {
        size_t o = api.FieldOffset(k, "_characterData", 0);
        if (!o) o = api.FieldOffset(k, "characterData", 0);
        if (!o && g_offCdata) o = (size_t)g_offCdata;
        s_k = k;
        s_off = o;
        DBGLOG(TPL_LOG " V40 role: cls=%s _characterData=+0x%zx", api.class_name(k), o);
    }
    if (!s_off || s_off > 0x800) return 0;
    const uintptr_t cd = PtrAt((uintptr_t)obj + s_off);
    if (!AddrOk(cd, 0x40)) return 0;
    void* cdk = api.object_class(reinterpret_cast<void*>(cd));
    static const void* s_ck = nullptr;
    static size_t s_roff = 0;
    if (LivePtr(cdk) && s_ck != cdk) {
        size_t o = api.FieldOffset(cdk, "ulRoleId", 0);
        if (!o) o = api.FieldOffset(cdk, "roleId", 0);
        if (!o) o = 112 + (size_t)g_deltaC;  // 旧标定量兜底（元数据里 `cdata+112` 就是 ulRoleId）
        s_ck = cdk;
        s_roff = o;
        DBGLOG(TPL_LOG " V40 role: dataCls=%s ulRoleId=+0x%zx", api.class_name(cdk), o);
    }
    if (!s_roff || s_roff > 0x800) return 0;
    const uintptr_t fp = cd + s_roff;
    if (!Il2Cpp::IsReadableRange(fp, 8)) return 0;
    return U64At(fp);
}

/* ============================ v41：玩家侧判据 / 施法者解算 / 投射物 data 白名单 ============
 * 用户 m02899 血证：「我开了 3，为什么敌人打我，我直接死了，打我的时候一堆伤害，是不是反了？」
 * 根因见 `ApplyProjectileExecBoost` 的 v41 注释：`LAddProjectile` 是**敌我共用的技能配置对象**，
 * 而 #3 的 ×kProjFanout 写在它身上 ⇒ 敌人的弹幕一起翻倍。
 * 下面这一组就是把「这次施放/这笔伤害归谁」问清楚，**问不出来就不写**。 */

/** 对象是不是**玩家侧**（`_player` 本体 / roleId 与玩家相同 / 指针历史表 / `_hitter` 与玩家相同）。
 *  逐条试，任一条命中即算；全不命中返回 false。 */
static bool PlayerSideV41(void* obj) {
    if (!obj) return false;
    if ((uintptr_t)obj == g_playerPtr) return true;
    const uint64_t pl = g_playerRoleV32.load(std::memory_order_relaxed);
    const uint64_t r = RoleIdOfV40(obj);
    if (r != 0 && pl != 0 && r == pl) return true;
    if (IsPlayerActorV32(obj)) return true;
    const uintptr_t h = ActorHitterV32((uintptr_t)obj);
    const uintptr_t plh = g_playerHitterV31.load(std::memory_order_relaxed);
    if (h != 0 && plh != 0 && h == plh) return true;
    return false;
}

/** 「玩家侧用过的 `LAddProjectile` 对象」白名单 —— **只做兜底**（来源是 17 参帧上已验证的
 *  `chr`/`src`）。因为技能配置对象敌我可能共用实例，**绝不单独当判据用**。 */
static const uint32_t kProjPlDataV41 = 32;
static void* g_projPlDataV41[kProjPlDataV41] = {};
static std::atomic<uint32_t> g_projPlDataNV41{0};

static void ProjMarkPlayerDataV41(void* data) {
    if (!data) return;
    const uint32_t n = g_projPlDataNV41.load(std::memory_order_relaxed);
    for (uint32_t i = 0; i < n && i < kProjPlDataV41; i++)
        if (g_projPlDataV41[i] == data) return;
    if (n >= kProjPlDataV41) return;
    g_projPlDataV41[n] = data;
    g_projPlDataNV41.store(n + 1, std::memory_order_relaxed);
}

static bool ProjIsPlayerDataV41(void* data) {
    if (!data) return false;
    const uint32_t n = g_projPlDataNV41.load(std::memory_order_relaxed);
    for (uint32_t i = 0; i < n && i < kProjPlDataV41; i++)
        if (g_projPlDataV41[i] == data) return true;
    return false;
}

/** 解出「这次 `ActionCommand_AddProjectile::Execute` 是谁发的」。
 *  ① `ActionCommandData.actionGameObject 0x0401793F`（IActionGameObject，实测就是施法者本体：
 *     v39 日志 `chr=0x7d427b532e00 src=0x7d427b532e00` 同指 `_player`）；
 *  ② `ActionCommandData.action 0x0401793E`（Action → 其 `character`/`_character`/`gameObject`）。
 *  玩家侧 ⇒ 返回该对象；否则 / 解不出 ⇒ `nullptr`。 */
static void* CasterOfV41(void* acd, void* ak, const char* an) {
    if (!acd || !LivePtr(ak)) return nullptr;
    auto& api = Il2Cpp::GetApi();
    if (!api.object_class) return nullptr;
    static const void* s_k = nullptr;
    static size_t s_igo = 0, s_act = 0;
    if (s_k != ak) {
        s_k = ak;
        s_igo = api.FieldOffset(ak, "actionGameObject", 0);
        s_act = api.FieldOffset(ak, "action", 0);
        DBGLOG(TPL_LOG " V41 caster: cls=%s actionGameObject=+0x%zx action=+0x%zx", an ? an : "?", s_igo,
             s_act);
    }
    void* cand[2] = {nullptr, nullptr};
    if (s_igo && s_igo <= 0x400) cand[0] = (void*)RawWord(acd, s_igo);
    if (s_act && s_act <= 0x400) cand[1] = (void*)RawWord(acd, s_act);
    /* 诊断：把两个候选的类名打出来（前 6 次）—— 万一都没解成玩家侧，靠这行决定下一步按名解析哪个字段。 */
    static std::atomic<uint32_t> s_dbg{0};
    const uint32_t dbg = s_dbg.fetch_add(1, std::memory_order_relaxed);
    if (dbg < 6) {
        const char* c0 = (LivePtr(cand[0]) && api.object_class) ? api.class_name(api.object_class(cand[0])) : "-";
        const char* c1 = (LivePtr(cand[1]) && api.object_class) ? api.class_name(api.object_class(cand[1])) : "-";
        DBGLOG(TPL_LOG " V41 cand[0]igo=%p cls=%s side=%d | cand[1]act=%p cls=%s side=%d", cand[0],
             c0 ? c0 : "?", cand[0] ? (PlayerSideV41(cand[0]) ? 1 : 0) : -1, cand[1], c1 ? c1 : "?",
             cand[1] ? (PlayerSideV41(cand[1]) ? 1 : 0) : -1);
    }
    for (int i = 0; i < 2; i++) {
        void* o = cand[i];
        if (!LivePtr(o)) continue;
        if (PlayerSideV41(o)) return o;
        if (i != 1) continue;
        void* ok2 = api.object_class(o);  // Action → 找它的施法者
        if (!LivePtr(ok2)) continue;
        static const void* s_ak2 = nullptr;
        static size_t s_c2 = 0, s_g2 = 0;
        if (s_ak2 != ok2) {
            s_ak2 = ok2;
            s_c2 = api.FieldOffset(ok2, "character", 0);
            if (!s_c2) s_c2 = api.FieldOffset(ok2, "_character", 0);
            s_g2 = api.FieldOffset(ok2, "gameObject", 0);
            if (!s_g2) s_g2 = api.FieldOffset(ok2, "_gameObject", 0);
            DBGLOG(TPL_LOG " V41 caster(Action): cls=%s character=+0x%zx gameObject=+0x%zx",
                 api.class_name ? api.class_name(ok2) : "?", s_c2, s_g2);
        }
        const size_t offs[2] = {s_c2, s_g2};
        for (int j = 0; j < 2; j++) {
            if (!offs[j] || offs[j] > 0x400) continue;
            void* p = (void*)RawWord(o, offs[j]);
            if (LivePtr(p) && PlayerSideV41(p)) return p;
        }
    }
    return nullptr;
}

/** ★v32：玩家 `ulRoleId`（读 `_player` 的 `cdata+112`，与演员扫描用同一套校准量）。
 *  血证：战斗中 `_player` 是分身 `SimpleCharacter`（`ACTSUM main=0`），但它与真正承伤的战斗
 *  `Character` **roleId 相同**（`SECATTR` 两侧都是 131202533413845712）⇒ roleId 是最可靠的玩家身份量。 */
static void PlayerRoleEnsureV32() {
    static std::atomic<uint32_t> s_log{0};
    const uintptr_t pl = g_playerPtr;
    if (!pl) return;
    /* ★v40：改用**按字段名**读 `ulRoleId`（`RoleIdOfV40`）。
     *  v32 的 `cdata+112+g_deltaC` 这条硬偏移在本版本读不到（v33 实测 `V32 playerRole` **0 条**），
     *  而 v39 已证明「按名读 `_characterData`」可行 ⇒ 身份量必须走按名这条路。 */
    uint64_t r = RoleIdOfV40(reinterpret_cast<void*>(pl));
    if (!r && g_offCdata) {  // 兜底：旧标定量
        const uintptr_t cd = PtrAt(pl + g_offCdata);
        if (cd && Il2Cpp::IsReadableRange(cd, 210)) r = U64At(cd + 112 + g_deltaC);
    }
    if (!r) return;
    const uint64_t old = g_playerRoleV32.exchange(r, std::memory_order_relaxed);
    if (old != r && s_log.fetch_add(1, std::memory_order_relaxed) < 8)
        DBGLOG(TPL_LOG " V40 playerRole _player=%p role=%llu", (void*)pl, (unsigned long long)r);
}

/** #3「全模式」：`StarsCollider.localDamage = 1`（客户端权威伤害）。每拍维护，怕被战斗流程重置。
 *  ★v41 停用（见 `kColliderLocalDamageV41` 声明处）：这是 `BattleElementCenter` 那个**全局静态**
 *  碰撞体，谁的攻击都会读它 ⇒ 等于把敌我双方全变成客户端权威；而且 v39 实测冒险模式里它本来
 *  就是 1（写出 0 次）= 空操作。真正要的「只为玩家侧施放加速」走 v41 的 `ApplyProjectileExecBoost` 发送者闸。 */
static void LocalDamageApplyV31(bool on) {
    if (!kColliderLocalDamageV41) return;
    if (!on) return;
    static std::atomic<uint32_t> s_log{0};
    static void* s_be = nullptr;
    static size_t s_off = 0;
    auto& api = Il2Cpp::GetApi();
    if (!s_be) {
        s_be = FindClassAnywhere("BattleElementCenter");
        if (!s_be) return;
    }
    void* sc = api.StaticRefField(s_be, "collider");
    if (!LivePtr(sc)) return;
    if (!s_off) {
        void* ck = api.object_class ? api.object_class(sc) : nullptr;
        if (!LivePtr(ck)) return;
        const size_t off = api.FieldOffset(ck, "localDamage", 0);
        if (!off || off > 0x400) return;
        s_off = off;
        DBGLOG(TPL_LOG " V31 localDamage +0x%zx cls=%s collider=%p", off,
             api.class_name ? api.class_name(ck) : "?", sc);
    }
    if (ByteAt((uintptr_t)sc + s_off) != 1) {
        SetByte((uintptr_t)sc + s_off, 1);
        const uint32_t n = s_log.fetch_add(1, std::memory_order_relaxed);
        if (n < 10 || (n % 200) == 0)
            DBGLOG(TPL_LOG " V31 localDamage -> 1（客户端权威）collider=%p 第%u次", sc, n + 1);
    }
}

/** 演员的原始 camp；`_hitter` 为空时按官方语义返回 0；读不到返回 -1。 */
static int32_t ActorCamp(uintptr_t actor) {
    const size_t ho = HitterFieldOff();
    if (!ho || !actor) return -1;
    const uintptr_t h = PtrAt(actor + ho);
    if (!h || !AddrOk(h, 0x20)) return 0;
    return I32At(h + 0x18);  // Hitter._camp
}

static void ActorRecord(uintptr_t a) {
    if (!kActorOn || !a) return;
    const uint32_t n = g_actN.load(std::memory_order_relaxed);
    uint32_t slot = n;
    for (uint32_t i = 0; i < n && i < kActMax; i++) {
        if (g_act[i].actor != a) continue;
        if (!g_act[i].dead) return;  // 已在表里且有效
        slot = i;                    // v26q：已回收的槽位直接复用（主城 NPC 换场景后大量失效）
        break;
    }
    if (slot >= kActMax) {
        slot = kActMax;
        for (uint32_t i = 0; i < kActMax; i++)
            if (g_act[i].dead) {
                slot = i;  // v26q：表满时抢占最早的死槽（否则新怪永远进不来）
                break;
            }
        if (slot >= kActMax) return;
    }
    ActorEnt& e = g_act[slot];
    memset(&e, 0, sizeof(e));
    e.actor = a;
    e.kw = PtrAt(a);
    e.state = PtrAt(a + StateOffV31((void*)a));  // v31：`ActionGameObject::_state` 按类解析（回退 64）
    e.dmg = PtrAt(a + 240);
    e.hpKey = -1;
    e.ctype = -1;
    e.isMain = (int32_t)ByteAt(a + 20);
    SafeObjType(a, e.cls, sizeof(e.cls));
    e.camp = ActorCamp(a);  // v26s：阵营（-1 = 未知；秒杀小怪靠它挡同阵营）
    e.campSet = 1;
    // v26b：数据指针多路解析（+40 可能是 null；SimpleCharacter 在 +368=_characterData）
    e.cdata = ActorDataPtr(a, &e.dataOff, e.ccls, sizeof(e.ccls));
    if (e.cdata && AddrOk(e.cdata + 112, 8)) e.roleId = U64At(e.cdata + 112);
    if (e.cdata && AddrOk(e.cdata + 120, 8)) e.ownerRole = U64At(e.cdata + 120);
    if (e.cdata && AddrOk(e.cdata + 552, 8)) e.teamId = U64At(e.cdata + 552);
    if (e.cdata && AddrOk(e.cdata + 148, 4)) e.ctype = I32At(e.cdata + 148);
    // 属性字典：先试已知候选 @416，再在 cdata+16..+0x280 自发现（每演员只做一次）
    size_t dictOff = 0;
    if (AddrOk(e.cdata + 416, 8)) {
        const uintptr_t p = PtrAt(e.cdata + 416);
        if (AddrOk(p, 0x40)) {
            const char* t = SafeObjType(p, s_ab0, sizeof(s_ab0));
            if (t && strstr(t, "Dictionary")) {
                e.dict = p;
                dictOff = 416;
            }
        }
    }
    if (!e.dict && AddrOk(e.cdata, 0x100)) {
        for (size_t off = 16; off + 8 <= 0x280; off += 8) {
            const uintptr_t p = PtrAt(e.cdata + off);
            if (!AddrOk(p, 0x40)) continue;
            const char* t = SafeObjType(p, s_ab0, sizeof(s_ab0));
            if (t && strstr(t, "Dictionary")) {
                e.dict = p;
                dictOff = off;
                break;
            }
        }
    }
    if (slot == n) g_actN.store(n + 1, std::memory_order_relaxed);
    DBGLOG(TPL_LOG " ACT+ %s actor=%p cdata=%s:+0x%zx:%p role=%llu owner=%llu team=%llu ctype=%d state=%p dmg=%p isMain=%d camp=%d dict=+0x%zx:%p n=%u", e.cls,
         (void*)a, e.ccls, e.dataOff, (void*)e.cdata, (unsigned long long)e.roleId,
         (unsigned long long)e.ownerRole, (unsigned long long)e.teamId, e.ctype, (void*)e.state,
         (void*)e.dmg, e.isMain, e.camp, dictOff, (void*)e.dict, slot + 1);
    // v26e：字段表已取证完毕 → DumpAllFields 默认关（连 AddrOk 的 maps 解析也一并省掉）
    if (kDumpFields) {
        if (AddrOk(a, 0x100)) DumpAllFields((void*)a, "ACT", 96);
        if (e.cdata && AddrOk(e.cdata, 0x60)) DumpAllFields((void*)e.cdata, "CDATA", 96);
        if (e.state && AddrOk(e.state, 0x60)) DumpAllFields((void*)e.state, "ASTATE", 48);
        if (e.dmg && AddrOk(e.dmg, 0x60)) DumpAllFields((void*)e.dmg, "DMGPROP", 24);
    }
    /* v26t：**删掉** `ScanHpFields(e.cdata,"CDATA")` / `ScanHpFields(a,"ACT")` / `AttrDumpLog(&e)`。
     *  用户实测反馈第 1 条「游戏变的好掉帧」的主要来源：这三个函数每个新演员都要遍历整张字段表
     *  （每个字段一次 `AddrOk` = 一次 /proc/self/maps 解析，外加逐字段读值），一进场就是几十个演员。
     *  它们产出的 `sk[]`（命名血量落点）在 v26s3 全场比赛里恒为 `sinks=0`、`key=-1` ——
     *  **一次都没命中真实血量**，零收益。血量现在只走官方链路（见下方 SECATTR 多候选块）。 */
    (void)dictOff;
}

/** 写一个血量落点：kill=true → 1（秒杀）；kill=false → 极大值（无敌）。 */
static void WriteSink(ActorEnt* e, int i, bool kill) {
    ActorSink& s = e->sk[i];
    switch (s.kind) {
        case SK_I64:
            if (AddrOk(s.addr, 8)) {
                const int64_t v = kill ? 1 : (int64_t)900000000;
                memcpy((void*)s.addr, &v, 8);
            }
            break;
        case SK_I32:
            if (AddrOk(s.addr, 4)) SetI32(s.addr, kill ? 1 : 2000000000);
            break;
        case SK_F32:
            if (AddrOk(s.addr, 4)) SetF32(s.addr, kill ? 1.0f : 1.0e9f);
            break;
        case SK_F64:
            if (AddrOk(s.addr, 8)) {
                const double v = kill ? 1.0 : 1.0e9;
                memcpy((void*)s.addr, &v, 8);
            }
            break;
        case SK_BIG:
            if (AddrOk(s.addr, 16)) {
                const double m = kill ? 1.0 : 9.9;
                const int32_t ex = kill ? 0 : 9;
                memcpy((void*)s.addr, &m, 8);
                memcpy((void*)(s.addr + 8), &ex, 4);
            }
            break;
        default: break;
    }
}

/** v26b：把某个血量落点写成指定值（技能伤害下压 / 无敌顶满都用它）。
 *  v26e：recheck=false 时跳过 maps 校验（AddrOk 每次都要 fopen /proc/self/maps；
 *        落点地址只在每 2s 重验一次，其余拍子直接写）。 */
static void WriteSinkVal(ActorEnt* e, int i, int64_t val, bool recheck) {
    ActorSink& s = e->sk[i];
    const bool okA = recheck ? AddrOk(s.addr, 16) : (s.addr != 0);
    if (!okA) return;
    switch (s.kind) {
        case SK_I64:
            memcpy((void*)s.addr, &val, 8);
            break;
        case SK_I32:
            SetI32(s.addr, (int32_t)val);
            break;
        case SK_F32:
            SetF32(s.addr, (float)val);
            break;
        case SK_F64: {
            const double d = (double)val;
            memcpy((void*)s.addr, &d, 8);
            break;
        }
        case SK_BIG: {
            const double m = (double)val;
            const int32_t ex = 0;
            memcpy((void*)s.addr, &m, 8);
            memcpy((void*)(s.addr + 8), &ex, 4);
            break;
        }
        default: break;
    }
}

/** v26t：**已删除** `SetHpAll`（「属性字典血量槽 + 全部命名血量落点」一起写）。
 *  v26s3 血证：这些落点（`hpKey` 字典槽 / `sk[]` 命名 sink）恒为 `sinks=0`、`key=-1`，
 *  一次都没命中过真实血量；而它们会往一堆「看起来像指针」的堆地址写 int64，
 *  是 21:31:53 那记 SIGSEGV（`x0 = 0101010100000000`）的另一号嫌疑。
 *  现在血量只走官方链路：`cdata→SecondaryAttribute._attributes[23]` + `_attributesBigInt[23]` + `set_healthPointNow()`。 */

/** v26 主循环（Tick 内 400ms 节流）：观测血量 → 落地三功能。
 *  写入只发生在**通过身份校验**的演员对象上（klass 字比对 + 定期重验）。 */
/** v26i：功能开启时重置「演员登记」收集状态（见 Mxytz_OnFeatureChanged）。 */
static std::atomic<uint32_t> g_seenScan{0};
static std::atomic<uint32_t> g_actDumpN{0};
static void ActorResetScan() {
    g_seenScan.store(0, std::memory_order_relaxed);
    g_actDumpN.store(0, std::memory_order_relaxed);
}

/** v26i：只收「功能已开」之后登记的对象 —— 启动期噪声（协程状态机/代码页/字符串）
 *  把预算吃光是上一版战斗期零日志的原因。 */
// v26m：启动自检窗口（**只读**）—— 不改游戏状态，只为在本地验证
//   `cdata+360 → SecondaryAttribute._attributes` 这条官方血量链是否真的通。
//  窗口内 HookArmWanted() 恒真 ⇒ 演员图三路补员照跑、SECATTR 照打日志，但三个功能开关仍是关的，
//  所以任何 KILLMOB/INVULN/SKILLATK 写入分支都不会执行（不碰游戏状态，零副作用）。
static const uint64_t kSelfTestMs = 0;  // 0 = 关闭（本地验证时改成 150000；正式包不要开，
                                        // 因为窗口内会跑 1024MB 全堆扫描，启动期会卡一下）
static std::atomic<uint64_t> g_selfTestUntilMs{0};
static inline bool SelfTestActive() {
    const uint64_t u = g_selfTestUntilMs.load(std::memory_order_relaxed);
    return u != 0 && NowMs() < u;
}

static bool HookArmWanted() {
    return SelfTestActive() || g_killMob.load(std::memory_order_relaxed) ||
           g_invincible.load(std::memory_order_relaxed) || g_skillDmg.load(std::memory_order_relaxed);
}

/** v26f：把入口 hook 登记的真实对象（g_actorSeen）在 mod 线程上逐个识别成演员。
 *  400ms 一次、每次最多看 40 个、识别出来的对象走 ActorRecord（内部自带去重与上限）。
 *  认不出来的对象打一条只读「ACT? 这是什么」日志（上限 120 条）：给离线定案真正的演员布局。 */
static void ActorDumpSeenObj(uintptr_t o, const char* why) {
    const uint32_t dn = g_actDumpN.fetch_add(1, std::memory_order_relaxed);
    if (dn >= 120) return;
    char b[80];
    const char* t = SafeObjType(o, b, sizeof(b));
    char f[400];
    int p = 0;
    static const size_t kOffs[] = {16, 24, 32, 40, 48, 56, 64, 72, 80, 240};
    for (size_t i = 0; i < sizeof(kOffs) / sizeof(kOffs[0]); i++) {
        const uintptr_t v = Il2Cpp::IsReadableRange(o + kOffs[i], 8) ? *(const uintptr_t*)(o + kOffs[i]) : 0;
        const char* vt = (v && (v & 7u) == 0 && Il2Cpp::IsReadableRange(v, 0x18)) ? SafeObjType(v, s_ab1, sizeof(s_ab1)) : nullptr;
        p += snprintf(f + p, sizeof(f) - (size_t)p, " +%zu=%p:%s", kOffs[i], (void*)v, vt ? vt : "?");
        if (p >= (int)sizeof(f) - 64) break;
    }
    DBGLOG(TPL_LOG " ACT?[%u] %s obj=%p cls=%s%s", dn, why, (void*)o, t ? t : "?", f);
}

static void ActorScanSeen() {
    const uint32_t n = g_actorSeenN.load(std::memory_order_relaxed);
    uint32_t s_scan = g_seenScan.load(std::memory_order_relaxed);
    if (!n || s_scan >= n) return;
    uint32_t looked = 0;
    for (; s_scan < n && looked < 40; s_scan++, looked++) {
        const uintptr_t o = g_actorSeen[s_scan];
        if (!AddrOk(o, 0x100)) continue;
        if (ActorAccept(o)) ActorRecord(o);
        // Buff → source@96 / target@104 也是演员（v25 实测），顺手试一次
        else {
            const char* t = SafeObjType(o, s_ab0, sizeof(s_ab0));
            if (t && strcmp(t, "Buff") == 0) {
                const uintptr_t src = PtrAt(o + 96), dst = PtrAt(o + 104);
                if (AddrOk(src, 0x100) && ActorAccept(src)) ActorRecord(src);
                if (AddrOk(dst, 0x100) && ActorAccept(dst)) ActorRecord(dst);
            } else {
                ActorDumpSeenObj(o, "未识别");  // v26f：只读取证，帮助定案真演员布局
                /* v26l：v26b 里**唯一真正捞出演员**的路线就是字段蜘蛛（实测日志
                 *   `SPIDER DataContainer._actionGameObject off=32 -> actor 0x7d4488c77540`）。
                 *   v26f 把 InterpDumpArgTypes 一起关掉后它跟着断了（v26k 实测 `SPIDER=0`）。
                 *   这里在「入口 hook 已登记的真实对象」上把它重开：先 OfferInteresting 粗筛，
                 *   再按类名去重（SpiderFields 内部 ≤32 类、每类 ≤96 字段），成本可控。 */
                if (OfferInteresting(t)) SpiderFields(o, t);
            }
        }
    }
    g_seenScan.store(s_scan, std::memory_order_relaxed);
    if (s_scan >= n && s_scan > 0)
        DBGLOG(TPL_LOG " SEEN 扫描完成 n=%u 已建演员=%u", n, g_actN.load(std::memory_order_relaxed));
}

/** v26j：在「已知演员所在的内存段」里定点扫 ActionState 实例 → 补齐入口 hook 看不见的演员（小怪）。
 *
 *  为什么这条路线可行（有实测依据，不是猜）：
 *   v20 数据面每轮都能扫到 18–54 个 ActionState（` APPLY … states=18`…`states=54`），
 *   当时**只输在校验写错**：`*(void**)(st+16) == ActionGameObject 的 klass` 恒假 ——
 *   对象的 klass 字是**最派生类**（实测 `Character`），永远不会等于基类 `ActionGameObject`，
 *   所以 ok=0 bad=all、CALIB 从未命中。
 *  现在改成**往返校验**：`ActionState._actionGameObject@16` → 演员；演员的 ActionState 槽 @64
 *   又指回同一个 st（`PtrAt(ago+64)==st`）。这条在结构上不可能凑巧成立，再叠加 ActorAccept。
 *  成本：只扫「已知演员所在的那几段」（每段 ≤64MB、总计 ≤128MB、1.5s 一次），
 *   而不是 v20 那样每 2s 把 787MB 全堆扫一遍。 */
static int LocalScanStates(void* asKlass, uintptr_t* out, int cap) {
    if (!asKlass || !out || cap <= 0) return 0;
    const uint32_t an = g_actN.load(std::memory_order_relaxed);
    const uint32_t sn = g_actorSeenN.load(std::memory_order_relaxed);
    /* v26l：原来这里 `if (!an) return 0;` —— 没有演员就无从定位段 ⇒ 死锁（v26k 实测连 LSCAN
     *   日志都不打）。改成把「入口 hook 登记的真实堆对象」也当锚点：g_actorSeen[] 里的
     *   DataContainer/Buff/UI_HpBar 一定落在游戏对象堆里，那几段里必然有 ActionState。 */
    if (!an && !sn) return 0;
    uintptr_t cand[192];
    uint32_t cn = 0;
    for (uint32_t i = 0; i < an && i < kActMax && cn < 192; i++)
        if (g_act[i].actor) cand[cn++] = g_act[i].actor;
    for (uint32_t i = 0; i < sn && i < kSeenMax && cn < 192; i++)
        if (g_actorSeen[i]) cand[cn++] = g_actorSeen[i];
    if (!cn) return 0;
    uintptr_t lo[3] = {0, 0, 0}, hi[3] = {0, 0, 0};
    int nr = 0;
    FILE* f = fopen("/proc/self/maps", "r");
    if (!f) return 0;
    char line[512];
    while (nr < 3 && fgets(line, sizeof(line), f)) {
        unsigned long long s = 0, e = 0;
        if (sscanf(line, "%llx-%llx", &s, &e) != 2) continue;
        if (!strstr(line, "rw")) continue;
        for (uint32_t i = 0; i < cn; i++) {
            const uintptr_t a = cand[i];
            if (!a || a < s || a >= e) continue;
            bool dup = false;
            for (int j = 0; j < nr; j++)
                if (lo[j] == (uintptr_t)s && hi[j] == (uintptr_t)e) dup = true;
            if (!dup) {
                lo[nr] = (uintptr_t)s;
                hi[nr] = (uintptr_t)e;
                nr++;
            }
            break;
        }
    }
    fclose(f);
    if (!nr) return 0;
    const uintptr_t kw = (uintptr_t)asKlass;
    int n = 0;
    int probed = 0;
    size_t bytes = 0;
    for (int r = 0; r < nr && n < cap; r++) {
        const uintptr_t end = (hi[r] - lo[r] > (64u << 20)) ? lo[r] + (64u << 20) : hi[r];
        bytes += (size_t)(end - lo[r]);
        for (uintptr_t p = lo[r]; p + 8 <= end; p += 8) {
            if (*(volatile uintptr_t*)p != kw) continue;
            if (++probed > 600) break;  // 误命中（别的字段恰好存着 ActionState 的 klass）不无限试
            const uintptr_t st = p;     // klass 是对象的第一个字 ⇒ 命中地址就是对象首地址
            const uintptr_t ago = PtrAt(st + 16);
            if (!AddrOk(ago, 0x100)) continue;
            if (PtrAt(ago + 64) != st) continue;  // 往返校验
            if (!ActorAccept(ago)) continue;
            out[n++] = ago;
            if (n >= cap) break;
        }
    }
    static uint32_t s_lastLog = 0;
    const uint32_t now = (uint32_t)NowMs();
    if (now - s_lastLog > 3000) {
        s_lastLog = now;
        DBGLOG(TPL_LOG " LSCAN regions=%d bytes=%zuMB probed=%d hits=%d actors=%u", nr, bytes >> 20, probed, n,
             g_actN.load(std::memory_order_relaxed));
    }
    return n;
}

/** v26j：顺着演员的 `<targets>@168` 直接拿敌人列表。
 *  依据（_钩子目标与语义.md 字段表）：ActionGameObject 有 `<targets>`+168；
 *  若它是 List<T>，`_items@0x10` 是数组、`_size@0x18` 是长度 ⇒ O(1) 就能拿到这一波的目标/敌人，
 *  不需要任何堆扫描。拿到后逐个过 ActorAccept（结构校验）再登记。 */
static int TraverseTargets(uintptr_t a) {
    const uintptr_t lst = PtrAt(a + 168);
    if (!AddrOk(lst, 0x28)) return 0;
    const char* t = SafeObjType(lst, s_ab2, sizeof(s_ab2));
    if (!t || !strstr(t, "List")) return 0;
    const uintptr_t items = PtrAt(lst + 0x10);
    const int32_t size = I32At(lst + 0x18);
    if (!AddrOk(items, 0x20) || size < 0 || size > 64) return 0;
    const int32_t alen = I32At(items + 0x18);
    if (alen < size || alen > 1024) return 0;
    const uintptr_t data = items + 0x20;
    if (!Il2Cpp::IsReadableRange(data, (size_t)size * 8)) return 0;
    int n = 0;
    for (int i = 0; i < size; i++) {
        const uintptr_t el = *(const uintptr_t*)(data + (size_t)i * 8);
        if (!AddrOk(el, 0x100)) continue;
        if (ActorAccept(el)) {
            ActorRecord(el);
            n++;
        }
    }
    static uint32_t s_log = 0;
    const uint32_t now = (uint32_t)NowMs();
    if (size > 0 && now - s_log > 4000) {
        s_log = now;
        DBGLOG(TPL_LOG " TGT actor=%p list=%p size=%d acc=%d", (void*)a, (void*)lst, size, n);
    }
    return n;
}

// ==================== v26q：官方演员总表 `GameObjectManager`（本段最大收获） ====================
// 依据（官方热更 DLL Assembly-CSharp.dll 的元数据 + IL，全部实证）：
//   · `ActionGameLibrary.GameObjectManager` [0x02002055] fields=36 methods=98，实例字段：
//       `_role`:SimpleCharacter @0x0401770A、**`_player`:SimpleCharacter @0x0401770B**、
//       `_partner`@0x0401770C、`_focus`:InteractiveGameObject@0x0401770D、`_focusIndex`:int@0x0401770E、
//       **`_unitList`:List<IActionGameObject>@0x0401770F**、`_objId_Map`@0x04017710、
//       `_roleId_Map`:Dictionary<ulong,IActionGameObject>@0x04017711、…、`_allActionGameEft`@0x0401772B、
//       **`listCharacter`:List<Character>@0x0401772D**
//   · `IsPlayer(IActionGameObject a, bool deep)` IL（0x0601E977，len 81）：
//       deep=true  → `((SimpleCharacter)a).characterData.ulRoleId == _player.…ulRoleId && obj_id == _player.…obj_id`
//       deep=false → **`a == _player`（直接引用比较）**
//     ⇒ 玩家判据 = 与 `_player` 比指针（v26p 的 `roleId != 0` 在战斗里是**错的**：所有战斗
//       Character（含玩家）都是 role=0，只有主城玩家和一个投射物非零 ⇒ 无敌加到了投射物上）。
//   · `get_player()` 0x0601E971、`get_unitList()` 0x0601E9AF、`GetCharacterListByCamp(int)` 0x0601E9BC。
//   ⇒ 演员来源不再靠满堆扫：找到**一个** GameObjectManager 实例，直接读它手里的两张表。
static void* g_gmKlass = nullptr;
static void* g_becKlass = nullptr;  // v26r：BattleElementCenter（gameObjectManager 是它的静态字段）
static uintptr_t g_gmObj = 0;
static size_t g_gmUnitOff = 0;    // _unitList
static size_t g_gmListChOff = 0;  // listCharacter
static size_t g_gmPlayerOff = 0;  // _player
// v26x：`g_playerPtr` 的**定义已提前到文件前段的伤害钩子区**（`Hooked_Hurt5` 要用它），
//   此处不再重复定义（重复定义 = 编译错误）。
static uint32_t g_gmScans = 0;
static uint64_t g_gmLastScan = 0;
static uint64_t g_gmLastHarvest = 0;
static uint64_t g_gmBecMs = 0;   // v26r：静态字段取单例的节流
static uint64_t g_gmBecLog = 0;

/** 按字段名在 klass 上解析偏移（只做一次）。 */
static size_t FieldOffByName(void* k, const char* name) {
    if (!k || !name) return 0;
    auto& api = GetApi();
    if (!api.class_fields || !api.field_name || !api.field_offset) return 0;
    void* it = nullptr;
    int guard = 0;
    while (void* f = api.class_fields(k, &it)) {
        const char* fn = api.field_name(f);
        if (fn && strcmp(fn, name) == 0) return api.field_offset(f);
        if (++guard >= 256) break;
    }
    return 0;
}

/** 展开一个 List<T>（`_items@0x10`、`_size@0x18`、数组长度 `+0x18`、数据 `+0x20`），
 *  把元素里通过 ActorAccept 的登记进演员表；返回登记数。 */
static int HarvestList(uintptr_t lst, const char* tag, int cap) {
    if (!AddrOk(lst, 0x28)) return 0;
    const char* t = SafeObjType(lst, s_ab2, sizeof(s_ab2));
    if (!t || !strstr(t, "List")) return 0;
    const uintptr_t items = PtrAt(lst + 0x10);
    const int32_t size = I32At(lst + 0x18);
    if (!AddrOk(items, 0x20) || size < 0 || size > 512) return 0;
    const int32_t alen = I32At(items + 0x18);
    if (alen < size || alen > 4096) return 0;
    const uintptr_t data = items + 0x20;
    if (!Il2Cpp::IsReadableRange(data, (size_t)size * 8)) return 0;
    int acc = 0;
    for (int i = 0; i < size && i < cap; i++) {
        const uintptr_t el = *(const uintptr_t*)(data + (size_t)i * 8);
        if (!AddrOk(el, 0x100)) continue;
        if (ActorAccept(el)) {
            ActorRecord(el);
            acc++;
        }
    }
    static uint32_t s_log = 0;
    const uint32_t now = (uint32_t)NowMs();
    if (size > 0 && now - s_log > 5000) {
        s_log = now;
        DBGLOG(TPL_LOG " GMHARV %s list=%p size=%d acc=%d actors=%u", tag, (void*)lst, size, acc,
             g_actN.load(std::memory_order_relaxed));
    }
    return acc;
}

/** 找/刷新 GameObjectManager 实例与字段偏移。最多试 12 次（每次 5s），找到后只刷新指针。 */
static void GmEnsure(uint64_t now) {
    if (!g_gmKlass) g_gmKlass = ResolveClassPref("GameObjectManager");
    if (!g_gmKlass) return;
    if (!g_gmUnitOff || !g_gmPlayerOff) {
        g_gmUnitOff = FieldOffByName(g_gmKlass, "_unitList");
        g_gmListChOff = FieldOffByName(g_gmKlass, "listCharacter");
        g_gmPlayerOff = FieldOffByName(g_gmKlass, "_player");
        DBGLOG(TPL_LOG " GM off _unitList=0x%zx listCharacter=0x%zx _player=0x%zx", g_gmUnitOff, g_gmListChOff,
             g_gmPlayerOff);
    }
    if (g_gmObj && AddrOk(g_gmObj, 0x100) && PtrAt(g_gmObj) == (uintptr_t)g_gmKlass) return;  // 仍有效
    if (g_gmObj) {
        g_gmObj = 0;
        g_playerPtr = 0;
    }
    /* v26r：不再靠堆扫描找单例。
     *  官方 `Act_KillEnemy::KillEnenmy()`（IL 0x060026AE）用的是
     *    `ldsfld 0x04017534 BattleElementCenter::gameObjectManager`（**静态字段**，flags=0x0016 = Static|Private）
     *  ⇒ 直接用 `field_static_get` 把单例取出来，绕开 `ScanInstances`。
     *  v26q3 实测 `ScanInstances` 根本扫不到真实例（`GMSCAN#1..#9 hits=4 ok=0 rejField=2`），
     *  还给出过 0x4000… 的 houdini 代码页伪命中 ⇒ 这条路本来就不可靠。 */
    if (now - g_gmBecMs > 1000) {
        g_gmBecMs = now;
        if (!g_becKlass) g_becKlass = FindClassAnywhere("BattleElementCenter");
        if (g_becKlass) {
            auto& api = Il2Cpp::GetApi();
            void* fld = api.class_get_field ? api.class_get_field(g_becKlass, "gameObjectManager") : nullptr;
            const uintptr_t gm = (uintptr_t)api.StaticRefField(g_becKlass, "gameObjectManager");
            if (gm && AddrOk(gm, 0x100) && PtrAt(gm) == (uintptr_t)g_gmKlass) {
                g_gmObj = gm;
                g_gmScans = 12;  // 已拿到，不再做堆扫描
                DBGLOG(TPL_LOG " GMBEC 静态字段命中 BattleElementCenter.gameObjectManager=%p", (void*)gm);
                return;
            }
            if (now - g_gmBecLog > 10000) {
                g_gmBecLog = now;
                /* 诊断：把 BattleElementCenter 的其它静态字段一并读出来。
                 *  它们全是 static（flags=0x0016/0x0011），所以可以拿它们判断
                 *  「field_static_get 是否可用」以及「是只有 GM 为 null，还是整类都没初始化」。 */
                const char* kn = api.class_name ? api.class_name(g_becKlass) : "?";
                DBGLOG(TPL_LOG
                     " GMBEC 未就绪 cls=%s fld=%p fn=%p gm=%p | battleMode=%p attackDataLib=%p projectileLib=%p "
                     "battleJudge=%p collider=%p battleDataModel=%p",
                     kn ? kn : "?", fld, (void*)(uintptr_t)(api.field_static_get ? 1 : 0), (void*)gm,
                     (void*)(uintptr_t)api.StaticRefField(g_becKlass, "battleMode"),
                     (void*)(uintptr_t)api.StaticRefField(g_becKlass, "attackDataLibrary"),
                     (void*)(uintptr_t)api.StaticRefField(g_becKlass, "projectileLib"),
                     (void*)(uintptr_t)api.StaticRefField(g_becKlass, "battleJudge"),
                     (void*)(uintptr_t)api.StaticRefField(g_becKlass, "collider"),
                     (void*)(uintptr_t)api.StaticRefField(g_becKlass, "battleDataModel"));
            }
        }
    }
    /* v26s3：**堆扫描整段删除**。
     *  v26r 实测：`scan done regions=1029 skipped=2 bytes=654MB hits=43` —— 全部 rw 段都扫到了，
     *  43 个命中里一个真实例都没有（`GMDIAG` 证明命中全是 houdini 代码页与垃圾）。
     *  更糟的是 1024MB 扫描会把 tick 线程卡住好几分钟（新启动实测 `lib ready 21:14:41` 之后
     *  直到 `21:16:55` 全在打 `scan[...]`），连每秒一次的静态字段检查都跑不动 ⇒ 净负收益。
     *  单例只能靠 `field_static_get`（`BattleElementCenter.gameObjectManager` 静态字段）取。 */
    if (now - g_gmBecLog > 20000) {
        g_gmBecLog = now;
        DBGLOG(TPL_LOG " GMBEC 静态字段仍未就绪（等待游戏初始化 BattleElementCenter，扫描路线已废弃）");
    }
}

/** v26s：`_player` 每拍（400ms）刷新一次 —— 它是**唯一可靠的玩家身份**（官方 IsPlayer(a,false) 就是 `a==_player`）。
 *  秒杀小怪靠它排除自己：刷新越慢，换场景时「旧 _player 还在、新玩家对象已换」的窗口就越长，
 *  那个窗口里新玩家对象会被当成小怪 ⇒ 所以这里不能只靠 800ms 的 GmHarvest。 */
static void GmRefreshPlayer() {
    if (!g_gmObj || !g_gmPlayerOff || !AddrOk(g_gmObj + g_gmPlayerOff, 8)) return;
    const uintptr_t pl = PtrAt(g_gmObj + g_gmPlayerOff);
    if (AddrOk(pl, 0x100) && ActorAccept(pl)) {
        if (pl != g_playerPtr) {
            g_playerPtr = pl;
            g_playerCamp = ActorCamp(pl);  // v26s：玩家阵营（秒杀小怪靠它挡同阵营）
            g_playerCampSet = (g_playerCamp >= 0);  // v26t：0 是合法阵营值，必须独立标志
            ActorRecord(pl);
            DBGLOG(TPL_LOG " GMPLAY _player=%p camp=%d set=%d actors=%u", (void*)pl, g_playerCamp,
                 g_playerCampSet ? 1 : 0, g_actN.load(std::memory_order_relaxed));
        }
    } else if (g_playerPtr) {
        g_playerPtr = 0;
        g_playerCamp = 0;
        g_playerCampSet = false;
    }
}

/** 从 GameObjectManager 收演员：展开 `_unitList` 与 `listCharacter`。 */
static void GmHarvest(uint64_t now) {
    if (!g_gmObj || !AddrOk(g_gmObj, 0x100)) return;
    if (now - g_gmLastHarvest < 800) return;
    g_gmLastHarvest = now;
    GmRefreshPlayer();
    int a1 = 0, a2 = 0;
    int sz1 = -1, sz2 = -1;
    if (g_gmUnitOff) {
        const uintptr_t lst = PtrAt(g_gmObj + g_gmUnitOff);
        if (AddrOk(lst, 0x28)) sz1 = I32At(lst + 0x18);
        a1 = HarvestList(lst, "_unitList", 256);
    }
    if (g_gmListChOff) {
        const uintptr_t lst = PtrAt(g_gmObj + g_gmListChOff);
        if (AddrOk(lst, 0x28)) sz2 = I32At(lst + 0x18);
        a2 = HarvestList(lst, "listCharacter", 256);
    }
    static uint64_t s_sum = 0;
    if (now - s_sum > 5000) {
        s_sum = now;
        DBGLOG(TPL_LOG " GMSUM gm=%p _player=%p unitList size=%d acc=%d listCharacter size=%d acc=%d actors=%u",
             (void*)g_gmObj, (void*)g_playerPtr, sz1, a1, sz2, a2, g_actN.load(std::memory_order_relaxed));
    }
}

// ---- v26q：调官方 `Character::CheckDead(true,false,false,false)` —— 唯一的真死亡路径 ----
//  依据：`Character::CheckDead(bool,bool,bool,bool) -> bool` 0x0601F133（len 799）：
//    `if (characterData.secondaryAttribute.get_healthPointNowBigInt() <= 0) { set_spNow(0);
//       ResetProtectRate(); if (!_state.isDead) { …; _state.ToDead(); } }`
//  `ActionState::ToDead()` 0x0601F1CE（len 120）：`isDead=true`、`Dispath(Event("OnDead"))`、
//    `set_actionSummaryStatus(3)`、`QteManager.QteExitByObjDead(obj_id)`、
//    `Collider.set_enabled(false)`。
//  v26m 曾刻意回避它（ToDead 会碰 Unity Collider，而 tick 线程不是 Unity 主线程），
//  v26p 实测证明**不调它怪就永远不死**（同一个 Character 零血挂 4 分钟）⇒ 必须调，且**每个演员只调一次**。
static void* g_cdActorKlass = nullptr;  // v26s2：上次解析 CheckDead 用的类（按类缓存，避免跨类用错 mi）
static void* g_miCheckDead = nullptr;   // v26t：**已行为验证过的真实现**（空桩会被剔除）
static void* g_cdCand[4] = {nullptr, nullptr, nullptr, nullptr};
static int g_cdCandN = 0;
static int g_cdTry = 0;
static int g_cdBad = 0;
static int g_cdMiss = 0;
static int g_cdThisTick = 0;  // v26s2：每拍最多 invoke 4 次 CheckDead（避免一进场就爆发一串 Unity 调用）
// v26w 熔断：`runtime_invoke` 是本 mod **唯一剩下的崩溃向量**（`SaSetHpNow` 已整段删除）。
//   任何一次 invoke 抛异常、或累计调用数越界，就**永久停用**这条链路 ——
//   宁可少一个「补刀」动作，也绝不让 mod 线程卡进「重执故障指令」死循环
//   （v26v2 实测：一次错 invoke 换来 131233 次 `Forwarding signal 11` + 一个核 100% 跑满 + 四项功能全灭）。
static int g_cdDisabled = 0;
static int g_cdTotal = 0;
static const int kCdTotalMax = 400;

/* v26t：枚举本类所有 `CheckDead(bool,bool,bool,bool) -> bool` 的 MethodInfo。
 *  v26s3 血证：`GetMethodBySig` 命中了基类空桩 `InteractiveGameObject::CheckDead`
 *  （IL 只有 `ldc.i4.0; ret`，0x0601E0BA）—— 两者签名**完全一样**，靠签名永远分不开，
 *  结果怪零血假死、关卡不结算、角色识别不到一直转圈圈。改成**行为探测**：
 *  依次 invoke，返回 true 的那个才是真实现（它内部跑完 `_state.ToDead()` 才回 true）。 */
static int CdCollect(void* ak, void** out, int cap) {
    auto& api = GetApi();
    if (!ak || !api.class_methods || !api.method_get_name || !api.method_ret_type || !api.method_param ||
        !api.type_name)
        return 0;
    int n = 0;
    void* iter = nullptr;
    int guard = 0;
    while (n < cap && guard++ < 1024) {
        void* mi = api.class_methods(ak, &iter);
        if (!mi) break;
        const char* nm = api.method_get_name(mi);
        if (!nm || strcmp(nm, "CheckDead") != 0) continue;
        void* rt = api.method_ret_type(mi);
        const char* rtn = rt ? api.type_name(rt) : nullptr;
        if (!rtn || !strstr(rtn, "Boolean")) continue;
        int pc = 0;
        for (; pc < 8; pc++) {
            void* pt = api.method_param(mi, (uint32_t)pc);
            if (!pt) break;
            const char* ptn = api.type_name(pt);
            if (!ptn || !strstr(ptn, "Boolean")) break;
        }
        if (pc != 4) continue;
        out[n++] = mi;
    }
    return n;
}

static bool CdInvoke(void* mi, uintptr_t actor, bool* okOut) {
    auto& api = GetApi();
    if (!mi || !api.runtime_invoke) return false;
    EnsureIl2CppAttach();
    uint8_t b[4] = {1, 0, 0, 0};  // CheckDead(true, false, false, false) —— 官方 Act_KillEnemy 的实参
    void* args[4] = {&b[0], &b[1], &b[2], &b[3]};
    void* exc = nullptr;
    /* ★v46：本工程**所有** mod 主动发起的 `runtime_invoke` 都必须打这个标 ——
     *  被调方内部再走到解释器入口时（`Hooked_Interp` / `Hooked_PushFrame`）会被闸①跳过派发，
     *  否则就是「帧内 invoke 帧」的递归放大（免广告那条已用 tombstone_11 实证）。 */
    t_modInvoke++;
    void* r = api.runtime_invoke(mi, (void*)actor, args, &exc);
    t_modInvoke--;
    if (exc) return false;
    if (okOut) *okOut = (r && *(uint8_t*)r != 0);
    return true;
}

static bool SaCheckDead(uintptr_t actor) {
    auto& api = GetApi();
    if (!actor || !api.runtime_invoke) return false;
    if (g_cdDisabled) return false;  // v26w 熔断：一次异常即永久停用
    if (g_cdTotal >= kCdTotalMax) {
        if (g_cdDisabled < 2) {
            g_cdDisabled = 2;
            DBGLOG(TPL_LOG " CHECKDED 熔断 已达总调用上限 %d → 永久停用 invoke", kCdTotalMax);
        }
        return false;
    }
    void* ak = api.object_class ? api.object_class((void*)actor) : nullptr;
    if (!ak) return false;
    if (ak != g_cdActorKlass || (!g_miCheckDead && g_cdCandN == 0)) {
        g_cdActorKlass = ak;
        g_miCheckDead = nullptr;
        g_cdCandN = CdCollect(ak, g_cdCand, 4);
        g_cdTry = 0;
        if (g_cdCandN == 0) {
            if (g_cdMiss < 4) {
                g_cdMiss++;
                DBGLOG(TPL_LOG " CHECKDED 本类无 CheckDead(bool×4) cls=%s → 跳过",
                     api.class_name ? api.class_name(ak) : "?");
            }
            return false;
        }
        DBGLOG(TPL_LOG " CHECKDED 候选 cls=%s n=%d", api.class_name ? api.class_name(ak) : "?", g_cdCandN);
    }
    if (g_miCheckDead) {  // 已锁定真实现：之后一律用它
        bool ok = false;
        if (CdInvoke(g_miCheckDead, actor, &ok)) {
            g_cdTotal++;
            return true;
        }
        g_cdDisabled = 1;  // v26w：锁定过的 mi 都抛异常 ⇒ 整条链路不可信
        DBGLOG(TPL_LOG " CHECKDED 熔断 锁定实现 invoke 异常 → 永久停用 invoke actor=%p", (void*)actor);
        return false;
    }
    while (g_cdTry < g_cdCandN) {
        const int idx = g_cdTry++;
        bool ok = false;
        if (!CdInvoke(g_cdCand[idx], actor, &ok)) {
            g_cdDisabled = 1;  // v26w 熔断：**立即**停用，不再试下一个候选
            DBGLOG(TPL_LOG " CHECKDED 熔断 候选 idx=%d invoke 异常 → 永久停用 invoke actor=%p", idx,
                 (void*)actor);
            return false;
        }
        g_cdTotal++;
        if (ok) {
            g_miCheckDead = g_cdCand[idx];
            DBGLOG(TPL_LOG " CHECKDED 锁定真实现 idx=%d mi=%p actor=%p", idx, g_cdCand[idx], (void*)actor);
            return true;
        }
        DBGLOG(TPL_LOG " CHECKDED 候选 idx=%d 返回 false（空桩/未判死）actor=%p", idx, (void*)actor);
    }
    return false;
}

static void ActorApplyTick() {
    if (!kActorOn) return;
    static uint64_t s_last = 0;
    static uint32_t s_round = 0;
    const uint64_t now = NowMs();
    if (now - s_last < 400) return;
    s_last = now;
    s_round++;
    if (!kHookProbe) ActorScanSeen();  // v26f：hook 只登记，识别在 mod 线程
    // ⓪ v26q（主路线）：官方演员总表 GameObjectManager → `_player` + `_unitList` + `listCharacter`。
    //   v26p 实测「零血不死 + 无敌加到投射物上」两条根因，都靠这条官方链路解决。
    //   **刻意放在 `HookArmWanted()` 之外**：GM 只在启动后扫最多 12 次（每次 5s，找到后只刷新指针），
    //   代价可忽略；好处是「进关卡前在主城」就能免费验证这条链是否通（`GM off` / `GMPLAY` / `GMHARV`
    //   三行日志），不必再花一次用户体力。写操作仍然全部由功能开关把关（kActorWrites && wantX）。
    GmEnsure(now);
    GmRefreshPlayer();  // v26s：每拍刷新 `_player`（玩家身份的唯一可靠来源）
    GmHarvest(now);
    g_cdThisTick = 0;  // v26s2：每拍的 CheckDead 配额（见下方 KILLDEAD）
    /* v26t：**删掉三条重活补员路线**（`TraverseTargets` / `LocalScanStates` / 兜底 `ScanInstances` 全堆扫描）。
     *  用户实测反馈第 1 条就是「游戏变的好掉帧」——这三条以前每拍都在跑：
     *  ① 对每个已登记演员展开 `<targets>` 列表；② 每 1.5s 定点扫「已知演员所在内存段」（每段 ≤64MB）；
     *  ③ 每 15s 一次 256MB 限界全堆扫描（`ScanInstances` 自己解析 /proc/self/maps 上千个 region）。
     *  而 v26r/v26s3 已证明主路线 —— `BattleElementCenter.gameObjectManager`（**静态字段** 0x04017534）
     *  → `_unitList` / `listCharacter` —— 是官方、稳定、零扫描的演员来源
     *  （主城实测 `GMHARV _unitList size=9 acc=9`，战斗实测 106 次采集）。
     *  所以演员补员全部交给上面的 `GmHarvest`，这里不再做任何扫描。 */
    const uint32_t n = g_actN.load(std::memory_order_relaxed);
    const bool wantKill = g_killMob.load(std::memory_order_relaxed);
    const bool wantInv = g_invincible.load(std::memory_order_relaxed);
    const bool wantSkill = g_skillDmg.load(std::memory_order_relaxed);
    /* ★v31：两条「照斌哥」的每拍维护 —— 与演员表是否有货无关，故放在 `if (!n)` 之前。
     *  `g_playerPtr` 刚在上面 `GmRefreshPlayer()` 刷过，这里读到的就是本拍玩家。 */
    PlayerHitterEnsureV31();         // 无敌 id1：玩家 `_hitter`（`DoAttack` 槽 2 的精确判据）
    LocalDamageApplyV31(wantSkill);  // #3 全模式：`StarsCollider.localDamage = 1`（客户端权威）
    PlayerRoleEnsureV32();           // ★v32：玩家 `ulRoleId`（`_player` 的 cdata+112）
    if (!n) {
        if ((wantKill || wantInv) && s_round % 15 == 0)
            DBGLOG(TPL_LOG " ACT wait n=0（还没从 hook 实参收到演员对象）");
        return;
    }
    int nMain = 0, nMob = 0, nWrite = 0, nUnknown = 0;
    for (uint32_t i = 0; i < n && i < kActMax; i++) {
        ActorEnt& e = g_act[i];
        if (e.dead) continue;
        if (PtrAt(e.actor) != e.kw) {  // 对象被回收/复用 → 立即停手
            e.dead = true;
            DBGLOG(TPL_LOG " ACT- %s actor=%p 已回收（klass 字变了）", e.cls, (void*)e.actor);
            continue;
        }
        if (now - e.lastVerifyMs > 3000) {
            e.lastVerifyMs = (uint32_t)now;
            if (!ActorAccept(e.actor)) {
                e.dead = true;
                DBGLOG(TPL_LOG " ACT- %s actor=%p 结构重验失败", e.cls, (void*)e.actor);
                continue;
            }
        }
        // v26e：落点地址的 maps 校验每 2s 才做一次（AddrOk 每次 = 一次 /proc/self/maps 解析）
        const bool recheck = (e.sinkMs == 0 || now - e.sinkMs > 2000);
        if (recheck) e.sinkMs = (uint32_t)now;
        // ---- 身份判定（v26j：改用 roleId，不再信 isMain）----
        //  v26i 实测（ACTSUM 连续 147 条）：找到的两个 Character **都是 isMain=1**（main=2 mob=0），
        //  其中一个是 roleId=0 的小怪 → isMain 把小怪也判成玩家 ⇒ 秒杀小怪一次都没动手（KILLMOB=0）。
        //  定案依据（_钩子目标与语义.md）：ulRoleId==0 → 小怪/NPC；玩家 roleId=131202533413845712。
        //  cdata 未解析时身份未知 → 一律不写（宁可不生效，也绝不误杀玩家）。
        const bool idKnown = (e.cdata != 0);
        // v26q：玩家判据改成「与 GameObjectManager._player 比指针」——这正是官方
        //  `IsPlayer(IActionGameObject a, bool deep)`（0x0601E977）在 deep=false 时的实现（`a == _player`）。
        //  v26p 血证：战斗里所有 Character（**包括玩家自己**）都是 roleId=0，整场只有主城玩家和一个
        //  `ActionGameProjectile` 非零 ⇒ 旧判据「roleId≠0」把无敌加到了投射物上、玩家本体一次没写。
        //  ★v29 定案（m00938 症状「小怪 0 血不死、能动能打」）：**彻底删掉这个猜测式回退**。
        //   `_player` 没拿到时就不写（宁可不生效），否则怪物会被判成玩家、被写上 `immuneDamage=1`。
        const bool isPlayer = (g_playerPtr != 0) && (e.actor == g_playerPtr);
        /* ★v32：玩家侧历史表 —— 首领关 `main=0`（场上没有任何演员等于 `_player`），真正承伤的是
         * 另一个 `Character`；但两者的 `ulRoleId` 相同（v31 血证 131202533413845712）。
         * ⇒ 除 `== _player` 外，把 roleId 等于玩家 RoleId 的演员也记进玩家侧表（`ApplyHurtPolicy` 用）。 */
        {
            const uint64_t pr32 = g_playerRoleV32.load(std::memory_order_relaxed);
            if (isPlayer || (pr32 != 0 && e.roleId == pr32)) {
                const bool wasNew = (g_plHistN.load(std::memory_order_relaxed) == 0) ||
                                    !IsPlayerActorV32((void*)e.actor);
                PlayerSideMarkV32(e.actor);
                if (wasNew && !isPlayer && now - e.lastVerifyMs < 5000)
                    DBGLOG(TPL_LOG " V32 plSide %s actor=%p role=%llu _player=%p（roleId 判玩家侧）", e.cls,
                         (void*)e.actor, (unsigned long long)e.roleId, (void*)g_playerPtr);
            }
        }
        if (e.isMain == 1) g_sawMain = true;
        if (!idKnown) {
            nUnknown++;
            if (e.writes++ < 2)
                DBGLOG(TPL_LOG " ACTID %s actor=%p cdata=0 身份未知 → 跳过写入", e.cls, (void*)e.actor);
            continue;
        }
        if (isPlayer) nMain++;
        else nMob++;

        // ---- 1) 读属性字典：锁定「血量 key」并跟踪当前血量 ----
        int64_t hpNow = -1;
        if (e.dict && (e.hpKey < 0 || now - e.dictReadMs > 2000)) {
            e.dictReadMs = (uint32_t)now;
            int32_t k[16];
            int64_t v[16];
            const int m = AttrRead(e.dict, k, v, 16);
            if (m > 0) {
                // 1a) 命中已知 key
                for (int j = 0; j < m; j++)
                    if (k[j] == e.hpKey) hpNow = v[j];
                // 1b) 未锁定 → 用「上一轮缓存值下降」找 HP（HP 是全属性里唯一会掉的）
                if (e.hpKey < 0) {
                    for (int j = 0; j < m; j++) {
                        for (int c = 0; c < e.cn; c++) {
                            if (e.ck[c] == k[j] && e.cv[c] > 1000 && v[j] < e.cv[c]) {
                                if (++e.decConfirm >= 2 && e.hpKey < 0) {
                                    e.hpKey = k[j];
                                    e.hpVal = v[j];
                                    e.hpMax = e.cv[c];
                                    e.hpAddr = AttrValueAddr(e.dict, k[j]);
                                    DBGLOG(TPL_LOG " HPLOCK %s key=%d %lld -> %lld (max=%lld) slot=%p", e.cls,
                                         k[j], (long long)e.cv[c], (long long)v[j],
                                         (long long)e.hpMax, (void*)e.hpAddr);
                                }
                                break;
                            }
                        }
                    }
                }
                // 1c) 更新缓存（按值大小保留前 16）
                e.cn = 0;
                for (int j = 0; j < m && e.cn < 16; j++) {
                    e.ck[e.cn] = k[j];
                    e.cv[e.cn] = v[j];
                    e.cn++;
                }
            }
        }
        // 热路径：直接读缓存的血量槽（1 次 maps 校验，不再是整字典扫描）
        if (e.hpAddr && AddrOk(e.hpAddr, 8)) hpNow = (int64_t)U64At(e.hpAddr);
        if (hpNow > e.hpMax) e.hpMax = hpNow;
        if (hpNow >= 0) e.hpVal = hpNow;

        // ---- v26t：官方血量链 SecondaryAttribute._attributes —— **多候选**（见 SecAttrAll 说明）----
        //  v26s3 血证：只写 +368 那个对象时 `phyDmg×50` 对伤害毫无影响 ⇒ 游戏读的是另一个 CharacterData。
        if (!e.saN && (e.saMs == 0 || now - e.saMs > 5000)) {
            e.saMs = (uint32_t)now;
            e.saN = (uint8_t)SecAttrAll(e.actor, e.cdata, e.sa, e.saB, e.saLen, 4);
            if (e.saN)
                DBGLOG(TPL_LOG " SECALL %s actor=%p n=%d sa0=%p sa1=%p sa2=%p sa3=%p", e.cls, (void*)e.actor,
                     e.saN, (void*)e.sa[0], (void*)e.sa[1], (void*)e.sa[2], (void*)e.sa[3]);
            else if (s_round <= 2)
                DBGLOG(TPL_LOG " SECALL %s actor=%p cdata=%p 无可用 SecondaryAttribute", e.cls, (void*)e.actor,
                     (void*)e.cdata);
        }
        const uintptr_t saData = e.saN ? e.sa[0] : 0;
        const uintptr_t saBig = e.saN ? e.saB[0] : 0;
        int64_t saHp = -1, saMax = -1, saPhy = 0, saMgc = 0;
        if (saData) {
            saHp = (int64_t)U64At(saData + (size_t)kIdxHp * 8);
            saMax = (int64_t)U64At(saData + (size_t)kIdxMaxHp * 8);
            saPhy = (int64_t)U64At(saData + (size_t)kIdxPhyDmg * 8);
            saMgc = (int64_t)U64At(saData + (size_t)kIdxMgcDmg * 8);
            e.hpVal = saHp / kAttrScale;
            if (saMax / kAttrScale > e.hpMax) e.hpMax = saMax / kAttrScale;
            if (s_round <= 2 || s_round % 150 == 0)
                DBGLOG(TPL_LOG
                     " SECATTR %s actor=%p arr=%p len=%d n=%d hpNow=%lld hpMax=%lld phy=%lld mgc=%lld role=%llu",
                     e.cls, (void*)e.actor, (void*)(saData - 0x20), e.saLen[0], e.saN,
                     (long long)(saHp / kAttrScale), (long long)(saMax / kAttrScale),
                     (long long)(saPhy / kAttrScale), (long long)(saMgc / kAttrScale),
                     (unsigned long long)e.roleId);
        }

        // ---- v26m 自检②：验证 runtime_invoke 在本游戏热更方法上是否可用（只读 getter，无副作用）----
        //  依据：游戏自己的 Act_KillEnemy::KillEnenmy() IL 里就是
        //     `characterData.secondaryAttribute.set_healthPointNow(0); CheckDead(true,false,false,false);`
        //  ——要忠实复刻这条官方链路就得能 runtime_invoke 热更方法。先只读验证可行性再决定用不用。
        static int s_invokeProbe = 0;
        if (s_invokeProbe < 3 && saData) {
            s_invokeProbe++;
            auto& ia = GetApi();
            void* saObj = (void*)PtrAt(e.cdata + kSecAttrOff);
            if (ia.thread_attach && ia.domain_get) {
                void* d = ia.domain_get();
                if (d) ia.thread_attach(d);
            }
            void* sak = ia.object_class ? ia.object_class(saObj) : nullptr;
            char sn[96];
            const char* sakn = SafeObjType((uintptr_t)saObj, sn, sizeof(sn));
            void* m1 = sak ? ia.GetMethodBySig(sak, "get_healthPointNow", 0, "Int64", nullptr) : nullptr;
            void* x1 = nullptr;
            void* r1 = (m1 && ia.runtime_invoke) ? ia.runtime_invoke(m1, saObj, nullptr, &x1) : nullptr;
            int64_t v1a = r1 ? *(int64_t*)r1 : -1;
            int64_t v1b = r1 ? *(int64_t*)((char*)r1 + 0x10) : -1;
            void* m2 = sak ? ia.GetMethodBySig(sak, "get_healthPointNowBigInt", 0, "BigInt", nullptr) : nullptr;
            void* x2 = nullptr;
            void* r2 = (m2 && ia.runtime_invoke) ? ia.runtime_invoke(m2, saObj, nullptr, &x2) : nullptr;
            double mant = 0;
            int32_t expe = 0;
            if (r2) {
                memcpy(&mant, (char*)r2 + 0x10, 8);
                memcpy(&expe, (char*)r2 + 0x18, 4);
            }
            DBGLOG(TPL_LOG
                 " INVOKE sa=%p cls=%s rawHp=%lld | mi1=%p ret1=%p v=%lld/%lld exc=%p | mi2=%p ret2=%p mant=%.6g e=%d exc2=%p",
                 saObj, sakn ? sakn : "?", (long long)saHp, m1, r1, (long long)v1a, (long long)v1b, x1, m2, r2, mant,
                 expe, x2);
            // v26w：**不再回写** —— `SaSetHpNow` 已整段删除（见其原处注释）。
            //   只保留只读 getter 探针（零副作用、零崩溃风险）。
        }

        // ---- 2) 全模式技能伤害增加 ----
        //  (a) 真·倍率杠杆：玩家 tagDmgProp.lfDamageOwnerRate@144（实测默认 0 → 视为 1 倍基线上乘）
        //  (b) 接收侧落地：把非玩家目标「当前血量」按倍率下压一次 —— 效果 = 我这几十下伤害一次到位，
        //      首领战肉眼可见；与「秒杀小怪」的区别是它不直接置 1，仍要靠正常攻击收尾。
        if (kActorWrites && wantSkill) {
            if (isPlayer && e.dmg && (recheck ? AddrOk(e.dmg + 144, 4) : true)) {
                // v26j：基线只取一次。v26i 实测 `SKILLDMG rate 0.0000->50`、下一拍 `50->2500`、
                //   再下一拍 125000…（每拍都乘一次）⇒ 越滚越离谱。现在恒为 base×倍率。
                const float cur = F32At(e.dmg + 144);
                if (!e.dmgBaseSet) {
                    e.dmgBase = (cur > 0.0f && cur < 1000.0f) ? cur : 1.0f;
                    e.dmgBaseSet = 1;
                }
                const float nv = e.dmgBase * g_skillDmgRate;
                if (cur < nv) {
                    SetF32(e.dmg + 144, nv);
                    nWrite++;
                }
                if (e.writes++ < 4)
                    DBGLOG(TPL_LOG " SKILLDMG %s dmgprop=%p base=%.4f cur=%.4f -> %.4f", e.cls, (void*)e.dmg,
                         e.dmgBase, cur, nv);
            }
            if (!isPlayer && !e.skillApplied && e.saN) {
                // v26t：接收侧落地走官方链路 —— 目标「当前 HP」按倍率下压一次（效果 = 我这几下伤害一次到位）。
                //   对所有 CharacterData 候选都写（v26s3 血证：只写一个候选时游戏读的是另一个）。
                const int64_t realMax = saMax / kAttrScale;
                // v26w：★**读数合理性闸** —— 只有 `saMax` 换算出的真实血量落在 [1, 1e8] 才动手。
                //   v26v2 实测出现过 `saHp=-1780352865546754261 saMax=5514746683060523718` 这种
                //   垃圾读数（actor 已被回收、内存被复用），而那一拍正好是崩溃前的最后一次 SKILLHP。
                //   没有这道闸就会把天文数字写进 `_attributes[23]` / BigInt，把整条链路带进未知状态。
                if (realMax > 0 && realMax <= 100000000LL) {
                    const int64_t nvReal =
                        (int64_t)((double)realMax / (double)(g_skillDmgRate > 0.0f ? g_skillDmgRate : 1.0f));
                    int64_t nv = nvReal < 1 ? 1 : nvReal;
                    if (nv > 100000000LL) nv = 100000000LL;  // v26w：写入值同样封顶
                    for (int ci = 0; ci < e.saN; ci++) {
                        if (AddrOk(e.sa[ci] + (size_t)kIdxHp * 8, 8)) {
                            SetI64(e.sa[ci] + (size_t)kIdxHp * 8, nv * kAttrScale);
                            nWrite++;
                        }
                        // v26w：**删掉** `SaSetHpNow(e.sa[ci] - 0x20, nv)` ——
                        //   `e.sa[ci]` 是 `long[]` 的数据区，减 0x20 是**数组对象**而非
                        //   `SecondaryAttribute` 对象 ⇒ 传错 this ⇒ il2cpp 内 SIGSEGV ⇒
                        //   线程卡进「重执故障指令」死循环（100% CPU + 功能全灭）。
                        //   内存写已足够（判死读 BigInt 数组，另写见下）。
                        if (AddrOk(e.saB[ci] + (size_t)kIdxHp * 16, 16)) {
                            const double mant = (double)nv;
                            const int32_t expe = 0;
                            memcpy((void*)(e.saB[ci] + (size_t)kIdxHp * 16), &mant, 8);
                            memcpy((void*)(e.saB[ci] + (size_t)kIdxHp * 16 + 8), &expe, 4);
                            nWrite++;
                        }
                    }
                    e.skillApplied = 1;
                    DBGLOG(TPL_LOG " SKILLHP %s actor=%p role=%llu ctype=%d n=%d saHp=%lld saMax=%lld -> %lld", e.cls,
                         (void*)e.actor, (unsigned long long)e.roleId, e.ctype, e.saN, (long long)saHp,
                         (long long)saMax, (long long)nv);
                }
            }
            /* v26t：真·倍率杠杆走官方属性表 —— 玩家 `_attributes[EN_ATTR_PHY_DMG(8) / MGC_DMG(9)]` × 倍率。
             *  `GameFormula::GetResult`(0x06000D79) 公式 1001/1002 直接吃 `phyDmg/mgcDmg`
             *  （`get_phyDmg()` 0x0601DF75 = `_attributes[8]/10000`），所以这是货真价实的伤害杠杆。
             *  v26s3 只写了一个候选（+368 那个）⇒ 实测 ×50 毫无效果；现在**所有候选都写**。
             *  幂等：基线只取一次存 e.atkBase，只在「当前值 < 基线×倍率」时写。 */
            if (isPlayer && e.saN) {
                int64_t phy0 = (int64_t)U64At(e.sa[0] + (size_t)kIdxPhyDmg * 8);
                int64_t mgc0 = (int64_t)U64At(e.sa[0] + (size_t)kIdxMgcDmg * 8);
                if (!e.atkBaseSet && (phy0 > 0 || mgc0 > 0)) {
                    e.atkBase = phy0 > 0 ? phy0 : mgc0;
                    e.atkBaseSet = 1;
                }
                if (e.atkBase > 0) {
                    const int64_t tgt = e.atkBase * (int64_t)(g_skillDmgRate > 0.0f ? g_skillDmgRate : 1.0f);
                    for (int ci = 0; ci < e.saN; ci++) {
                        const int64_t ph = (int64_t)U64At(e.sa[ci] + (size_t)kIdxPhyDmg * 8);
                        const int64_t mg = (int64_t)U64At(e.sa[ci] + (size_t)kIdxMgcDmg * 8);
                        if (ph > 0 && ph < tgt) {
                            SetI64(e.sa[ci] + (size_t)kIdxPhyDmg * 8, tgt);
                            nWrite++;
                        }
                        if (mg > 0 && mg < tgt) {
                            SetI64(e.sa[ci] + (size_t)kIdxMgcDmg * 8, tgt);
                            nWrite++;
                        }
                    }
                    if (e.writes++ < 6)
                        DBGLOG(TPL_LOG " SKILLATK %s n=%d base=%lld phy0=%lld mgc0=%lld -> %lld rate=%.1f", e.cls,
                             e.saN, (long long)e.atkBase, (long long)phy0, (long long)mgc0, (long long)tgt,
                             (double)g_skillDmgRate);
                }
                /* v26u：再加两把「伤害加成」杠杆 —— `_attributes[21 DMG_AMPLIFY_PERCENT]` /
                 *  `[24 DMG_AMPLIFY]`。理由：v26s3 实测 `_attributes[8]` ×50 落地但看不到伤害变化，
                 *  说明本作的伤害链未必走 `GameFormula::GetResult` 的 1001/1002 分支；而这两条是
                 *  「伤害加成/加成百分比」的经典语义，最终伤害多为 base×(1+amplify/10000) 形式，
                 *  对它们的缩放能覆盖「不吃 phyDmg 的公式分支」。两者都只放大，不缩小。 */
                if (e.saN) {
                    static const int kAmpIdx[2] = {21, 24};  // DMG_AMPLIFY_PERCENT / DMG_AMPLIFY
                    if (!e.ampSet) {
                        int64_t b0 = (int64_t)U64At(e.sa[0] + (size_t)kAmpIdx[0] * 8);
                        int64_t b1 = (int64_t)U64At(e.sa[0] + (size_t)kAmpIdx[1] * 8);
                        if (b0 >= 0 && b1 >= 0) {  // 全 0 也算合法基线（很多角色没这两条）
                            e.ampBase[0] = b0;
                            e.ampBase[1] = b1;
                            e.ampSet = 1;
                        }
                    }
                    if (e.ampSet) {
                        const float rate = g_skillDmgRate > 0.0f ? g_skillDmgRate : 1.0f;
                        for (int ai = 0; ai < 2; ai++) {
                            const int64_t bt = e.ampBase[ai] * (int64_t)rate;
                            if (bt == e.ampBase[ai]) continue;  // 倍率 1.0 → 无需写
                            for (int ci = 0; ci < e.saN; ci++) {
                                uintptr_t cell = e.sa[ci] + (size_t)kAmpIdx[ai] * 8;
                                const int64_t cur = (int64_t)U64At(cell);
                                if (cur >= e.ampBase[ai] && cur < bt) {
                                    SetI64(cell, bt);
                                    nWrite++;
                                }
                            }
                        }
                        if (e.writes <= 6)
                            DBGLOG(TPL_LOG " SKILLAMP %s n=%d base=%lld/%lld -> %lld/%lld rate=%.1f", e.cls, e.saN,
                                 (long long)e.ampBase[0], (long long)e.ampBase[1],
                                 (long long)(e.ampBase[0] * (int64_t)rate), (long long)(e.ampBase[1] * (int64_t)rate),
                                 (double)rate);
                    }
                }
            }
        }

        // ---- 3) 无敌（玩家）：状态位 + 血量顶满 ----
        /* ★v29：改为 `kActorWritesInv`（仅玩家自身免疫位）+ `kActorWritesPlayerHp`（属性表/血量，默认关）。
         *  `isPlayer` 必须严格等于「这个对象就是 `GameObjectManager._player`」（见 ActorApplyTick 里的
         *  v29 修正：去掉 `roleId!=0` 猜测式回退）—— 那个回退曾把**怪物**判成玩家，
         *  于是怪物被写上 `immuneDamage=1` ⇒ 「小怪 0 血但不死、还能动」。 */
        if (kActorWritesInv && wantInv && isPlayer) {
            /* ★v34→**回退**（用户 m02501：「还把无敌改坏了…你快别改」）：逐字节恢复 v33 你已验证
             *  「#2 单独开没问题」的硬编码偏移写法（v34 的按名解析已移除）。
             *  ★v47：改走 `InvStateWriteV47` —— **按字段名优先、按名失败才退回上面这套硬偏移**。
             *  理由见该函数注释（IL 明写游戏读的是 `state.immuneDamage`，而 v34 自查发现硬偏移
             *  写的是别的字节并污染无关状态）；两条路都保留，所以不会比 v46b 更弱。 */
            InvStateWriteV47((void*)e.state, recheck);
            /* v26t：**所有** CharacterData 候选的属性表都顶满（`_attributes[23] = _attributes[22]`），
             *  BigInt 数组同步（`Character::CheckDead` 首行读的正是 get_healthPointNowBigInt()）。
             *  v26s3 血证：只写一个候选时玩家照样被打死、`phyDmg×50` 也毫无效果 ⇒ 游戏读的是另一个候选。 */
            for (int ci = 0; kActorWritesPlayerHp && ci < e.saN; ci++) {
                if (!AddrOk(e.sa[ci] + (size_t)kIdxMaxHp * 8, 16)) continue;
                const int64_t mx = (int64_t)U64At(e.sa[ci] + (size_t)kIdxMaxHp * 8);
                SetI64(e.sa[ci] + (size_t)kIdxHp * 8, mx > 0 ? mx : (int64_t)900000000 * kAttrScale);
                nWrite++;
                if (AddrOk(e.saB[ci] + (size_t)kIdxMaxHp * 16, 32)) {
                    memcpy((void*)(e.saB[ci] + (size_t)kIdxHp * 16),
                           (const void*)(e.saB[ci] + (size_t)kIdxMaxHp * 16), 16);
                    nWrite++;
                }
            }
            // v26w：官方 setter 顶满已删除（`SaSetHpNow` 整段移除，见其原处注释）。
            //   `_attributes[23]=[22]` + BigInt 同步这两笔就是它做的全部事情。
            if (e.writes++ < 4)
                DBGLOG(TPL_LOG " INVULN %s actor=%p state=%p hp=%lld max=%lld key=%d sinks=%d n=%d saHp=%lld",
                     e.cls, (void*)e.actor, (void*)e.state, (long long)hpNow, (long long)e.hpMax, e.hpKey, e.skN,
                     e.saN, (long long)saHp);
        }

        // ---- 4) 秒杀小怪（判定：非 `_player` 且非同一阵营，见下方安全闸）----
        /* ★v26y 定案（用户 m06433 驱动）：「秒杀小怪进入关卡怪物直接死了，但是什么奖励装备没给」
         *  —— 根因就是下面那一整块**替游戏杀怪**（写 1 血 + 兜底 invoke `Character::CheckDead`）。
         *  关卡一加载怪物就被判死，死亡**不携带玩家伤害** ⇒ 掉落/结算为空。
         *  斌哥从不碰血量，他只改 `CtagHurtHPPara::m_llValue`（伤害 BigInt）⇒ 玩家自己那一击
         *  就是真·击杀。所以这里改走「伤害放大」，血量路线整块关掉（`kKillByHp=false`），
         *  代码原样保留以便回退取证。 */
        if (kActorWrites && !kKillByHp && wantKill && !isPlayer) {
            if (e.writes++ < 3)
                DBGLOG(TPL_LOG " KILLHURT %s actor=%p role=%llu saN=%d（v26y：改走 Hurt 伤害放大，不再写血）",
                     e.cls, (void*)e.actor, (unsigned long long)e.roleId, e.saN);
        }
        if (kActorWrites && kKillByHp && wantKill && !isPlayer) {
            /* v26t 安全闸（宁可不生效，绝不误杀自己/队友）：
             *  ① `_player` 还没拿到 → 一个都不杀（v26p 血证：战斗里玩家的 ulRoleId 也是 0，
             *     所以「roleId≠0 判玩家」在战斗里恒假；唯一可靠的玩家身份就是 `_player` 指针）；
             *  ② actor 就是 `_player` 本人 → 跳过（**v26s3 血证：这就是「首领被秒」的真凶**——
             *     玩家阵营合法值就是 0，而老闸门写成 `g_playerCamp != 0` 被直接关掉，闸门形同虚设）；
             *  ③ 阵营已解析且与玩家同阵营 → 跳过（官方 `Act_KillEnemy::KillEnenmy()` 就是这么做的）。 */
            if (!g_playerPtr) {
                if (e.writes++ < 2)
                    DBGLOG(TPL_LOG " KILLWAIT %s actor=%p（_player 未就绪，不杀）", e.cls, (void*)e.actor);
                continue;
            }
            if (e.actor == g_playerPtr) {
                if (e.writes++ < 2)
                    DBGLOG(TPL_LOG " KILLSELF %s actor=%p（就是 _player 本人，跳过）", e.cls, (void*)e.actor);
                continue;
            }
            if (!e.campSet) {
                e.camp = ActorCamp(e.actor);
                e.campSet = 1;
            }
            if (e.camp >= 0 && g_playerCampSet && e.camp == g_playerCamp) {
                if (e.writes++ < 2)
                    DBGLOG(TPL_LOG " KILLALLY %s actor=%p camp=%d playerCamp=%d（同阵营，跳过）", e.cls,
                         (void*)e.actor, e.camp, g_playerCamp);
                continue;
            }
            if (!e.saN) {  // 属性表未解析 → 绝不动手
                if (e.writes++ < 3)
                    DBGLOG(TPL_LOG " KILLSKIP %s actor=%p role=%llu cdata=%p saN=0（无属性表）", e.cls,
                         (void*)e.actor, (unsigned long long)e.roleId, (void*)e.cdata);
                continue;
            }
            /* v26x：已经死掉的演员不再写 —— 否则会把 `ToDead()` 之后的尸体血量顶回去（复活/卡关）。
             *  `ActionState.isDead` @+39（CFLIST 权威偏移）。 */
            if (e.state && AddrOk(e.state + 39, 1) &&
                *reinterpret_cast<const uint8_t*>(reinterpret_cast<const char*>(e.state) + 39)) {
                continue;
            }
            int wrote = 0;
            /* ★v26x 定案：血量写 **1 点真实血量**（`_attributes[23] = kAttrScale`），**不再写 0**。
             *  用户实测 m06433：「秒杀小怪进入关卡怪物直接死了，但是什么奖励装备没给」。
             *  根因：写 0 让游戏自己判死，**绕过了官方伤害管线** ⇒ 击杀不归属玩家的伤害
             *  （热更名册里就有 `<>c::<IsMyRoleDamage>b__0` / `<>c::<SyncDieStateToServer>b__46_0`）
             *  ⇒ 怪死了但掉落/结算为空。
             *  写 1 血后：玩家自己的那一击就是**真·击杀**，掉落/结算走官方链路 —— 与斌哥一致
             *  （斌哥 `[冒险-打怪]` 改的是伤害数值，让击杀归属于玩家的真实伤害）。
             *  双保险：`Hooked_Hurt5` 同时把玩家造成的伤害放大到 1e12 量级（真·秒杀）。 */
            for (int ci = 0; ci < e.saN; ci++) {
                if (!AddrOk(e.sa[ci] + (size_t)kIdxHp * 8, 8)) continue;
                SetI64(e.sa[ci] + (size_t)kIdxHp * 8, (int64_t)kAttrScale);  // = 1 点真实血量
                wrote++;
                if (AddrOk(e.saB[ci] + (size_t)kIdxHp * 16, 16)) {
                    const double m1 = 1.0;
                    const int32_t e1 = 0;
                    memcpy((void*)(e.saB[ci] + (size_t)kIdxHp * 16), &m1, 8);
                    memcpy((void*)(e.saB[ci] + (size_t)kIdxHp * 16 + 8), &e1, 4);
                    wrote++;
                }
            }
            // v26w：官方 setter 归零已删除（`SaSetHpNow` 整段移除）。
            //   `_attributes[23]=0` + BigInt 数组双 8 字节归零就是它的全部作用，
            //   而 `Character::CheckDead` 首行读的正是 `get_healthPointNowBigInt()` ⇒ 足够。
            /* v26t：★**删掉** `isDying@38` / `isDead@39` 的字节写入。
             *  v26s3 血证：这两笔只是把演员「标成」死的假象，游戏自己的死亡簿记
             *  （`ToDead()` → `OnDead` 事件 / `set_actionSummaryStatus(3)` / `QteExitByObjDead`）从没跑过
             *  ⇒ 小怪假死、关卡不结算、角色识别不到一直转圈圈。死亡只交给游戏自己的 `ToDead()`。 */
            /* v26x：★**延迟兜底**。小怪被压到 1 血后，正常情况下玩家自己的攻击就是击杀
             *  （Hurt 里伤害被放大到 1e12 量级，必死）⇒ **不再需要每拍 runtime_invoke**。
             *  只有当它停在 1 血 **≥15 拍（约 6 秒）** 仍没被玩家打死时，才兜底调一次官方
             *  `Character::CheckDead(true,false,false,false)`（行为探测版，见 SaCheckDead）。
             *  这样把 `runtime_invoke` 从热路径彻底挪走（它曾是唯一的崩溃向量）。 */
            if (!e.killInvoked && ++e.killTicks >= 15 && g_cdThisTick < 4) {
                e.killInvoked = 1;
                g_cdThisTick++;
                const bool cdOk = SaCheckDead(e.actor);
                DBGLOG(TPL_LOG " KILLDEAD %s actor=%p ok=%d n=%d saHp=%lld", e.cls, (void*)e.actor, cdOk ? 1 : 0,
                     e.saN, (long long)saHp);
            }
            nWrite++;
            if (e.writes++ < 8)
                DBGLOG(TPL_LOG
                     " KILLMOB %s actor=%p state=%p ctype=%d hp=%lld key=%d sinks=%d wrote=%d n=%d saHp=%lld saMax=%lld",
                     e.cls, (void*)e.actor, (void*)e.state, e.ctype, (long long)hpNow, e.hpKey, e.skN, wrote,
                     e.saN, (long long)saHp, (long long)saMax);
        }
    }
    if (!g_sawMain && n && ++g_noMainRounds > 25 && !g_roleAsPlayer) {
        g_roleAsPlayer = true;
        DBGLOG(TPL_LOG " ACT role-fallback on（整场未见 isMain=1 → 改用 roleId≠0 判玩家）");
    }
    if ((wantKill || wantInv || wantSkill) && (s_round % 10 == 0 || nWrite))
        DBGLOG(TPL_LOG " ACTSUM n=%u main=%d mob=%d unk=%d wrote=%d kill=%d inv=%d skill=%d fallback=%d pCamp=%d round=%u", n,
             nMain, nMob, nUnknown, nWrite, wantKill ? 1 : 0, wantInv ? 1 : 0, wantSkill ? 1 : 0,
             g_roleAsPlayer ? 1 : 0, g_playerCamp, s_round);
}

/** v26t：il2cpp 运行时是否已经初始化到「可以安全遍历类表」的程度。
 *  见 `Mxytz_TryInit` 里的说明（早遍历会撞崩 Boehm GC 的静态根扫描）。
 *  只用最轻量的两步探测，绝不遍历类表：① `domain_get_assemblies` 拿到非空列表；
 *  ② 首个 assembly 能取到非空 image。两步都过才允许后续的 `FindClassAnywhere` 大遍历。 */
static bool RuntimeReadyForWalk() {
    auto& api = GetApi();
    if (!api.domain_get || !api.domain_assemblies || !api.assembly_image) return false;
    void* dom = api.domain_get();
    if (!dom) return false;
    size_t n = 0;
    void** asms = api.domain_assemblies(dom, &n);
    if (!asms || n == 0 || n > 4096) return false;
    for (size_t i = 0; i < n && i < 8; i++) {
        if (!asms[i]) continue;
        if (api.assembly_image(asms[i])) return true;
    }
    return false;
}

bool Mxytz_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(MXYTZ_TARGET_LIB)) return false;
    if (!EnsureApi()) {
        DBGLOG(TPL_LOG " il2cpp api load pending");
        return false;
    }
    /* v26t：**运行时就绪闸**。
     *  现象（21:47/21:48 连续两次启动崩）：`step1 api ok -> resolve classes` 之后 17–23ms 就
     *  `signal 11 (SIGSEGV), code 1 (SEGV_MAPERR), Cause: null pointer dereference`，
     *  崩溃点 `#00 libil2cpp.so+0x205a83c`，调用链里紧邻的 `#01 +0x2234508` / `#02 +0x20f0b30`
     *  正落在 Boehm GC / il2cpp 初始化代码区（已知那条 `+0x22580f8` 的 GC abort 也在同一段）。
     *  ⇒ 我们在 il2cpp **还没初始化完**的时候就去做全镜像类名遍历（`FindClassAnywhere` 每次
     *  13755 个类），把 GC 的静态根扫描撞崩了。修法：先确认运行时真的起来了再走大件。
     *  判据：`domain_get_assemblies` 返回非空且首个 assembly 能取到 image —— 这是「il2cpp 已
     *  完成 domain/image 初始化」的最早可靠信号；没就绪就 return false 让 Tick 下一拍再试。 */
    if (!RuntimeReadyForWalk()) {
        static int s_wait = 0;
        if (s_wait < 12 && (s_wait == 0 || s_wait % 4 == 0))
            LOGI(TPL_LOG " il2cpp 运行时未就绪，暂缓类遍历（等 domain/assembly）wait=%d", s_wait);
        s_wait++;
        return false;
    }
    g_processStartMs.store(NowMs(), std::memory_order_relaxed);
    // 解释器入口 hook 必须尽早装：晚装会错过首轮解释执行（且 houdini 可能已翻译该页）。
    // ★v29：`kStartupArm` ⇒ **开机就挂**（不再等 `anyOn`），否则会错过配置表反序列化那一批
    //   `set_damage*`（实测 arming 晚 5~6 秒 ⇒ 参数路线永远赶不上，见标志位注释）。
    if (kStartupArm || kDiag) InstallInterpHook();
    DBGLOG(TPL_LOG " step1 api ok -> resolve classes");  // 卡死定位锚点（末行即故障点）

    /* v44 清理：原来这里是 `if (kProbeDump) { … }` 的类名/camp 字段探测（g_offCamp / g_playerKlass）。
     *  身份判据早已换成 **roleId**（v40 `RoleIdOfV40` + `PlayerSideV41`），类名路线不再需要。 */

    /* ★v47：**启动期不再跑 `ResolveParaLayout()`**（防闪退兜底）。
     *  它是 `FindClassAnywhere("CtagHurtHPPara")` + `FindClassAnywhere("BigInt")`，
     *  每次都是一次**全镜像类遍历**（13755 个类，375ms~1s），而且是在
     *  「il2cpp 刚完成 domain/assembly 初始化」的窗口里跑 —— v44 清理注释里那条
     *  Boehm GC `GC_exclude_static_roots_inner` → `abort("Exclusion ranges overlap")`
     *  就是大范围 image/class 遍历的嫌疑人；这一步同时把启动时间顶长。
     *  偏移不再需要在启动期钉死，两条兜底都已存在且更可靠：
     *    ① `InstallDmgHooks()` 装钩时补一次（v26x 已写的注释：「必须在这里补一次」）；
     *    ② `ApplyHurtPolicy` 在**第一次真的分发到 `Hurt`** 时用活体对象反查
     *       （`ResolveParaLayoutFromObj`，3 次 `class_get_field_from_name`，µs 级、零遍历）。 */
    DBGLOG(TPL_LOG " step3 classes done（v47：para layout 延后到装钩/首次 Hurt，启动期不做全镜像遍历）");
    /* v44 清理：原来这里跑 `InstallProbeDump()`（字段/实例探测，省 15s 卡顿的「大件诊断」），
     *  已随 kProbeDump 一起删除 —— 偏移早已标定固化，无需再探。 */
    SyncHooks();
    /* v26t：`g_libReady` 挪到**最后**才置位。
     *  以前它在本函数第一行就置 true，于是「类遍历中途崩了/没跑完」也会被框架当成已就绪，
     *  后面 Tick 不再重试 —— 一旦崩在遍历里就是彻底不可用。现在只有全程走完才算就绪。 */
    g_libReady.store(true);
    LOGI(TPL_LOG " lib ready (il2cpp+HybridCLR, lazy hooks)");
    // v26m：开自检窗口（只读，见 SelfTestActive 说明）
    g_selfTestUntilMs.store(NowMs() + kSelfTestMs, std::memory_order_relaxed);
    DBGLOG(TPL_LOG " SELFTEST 只读自检窗口开启 %llu ms（演员图 + SECATTR 链路验证，不写游戏状态）",
         (unsigned long long)kSelfTestMs);
    return true;
}

bool Mxytz_IsReady() { return g_libReady.load(); }

void Mxytz_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case MXYTZ_FEAT_KILL_MOB:   g_killMob.store(enabled); break;
        case MXYTZ_FEAT_INVINCIBLE: g_invincible.store(enabled); break;
        case MXYTZ_FEAT_SKILL_DMG:  g_skillDmg.store(enabled); break;
        case MXYTZ_FEAT_NO_AD:      g_noAd.store(enabled); break;
        default: return;  // 分类占位项不参与
    }
    LOGI(TPL_LOG " feat %d -> %d", featNum, enabled ? 1 : 0);
    /* ★v46：免广告开关变化 ⇒ 清掉待办槽与熔断窗口。
     *  关掉时不能让一条已登记的待办在关闭后还发出去；重开时应当**立刻**可重试，
     *  而不是干等熔断窗口（那是「点了开关要等半天才生效」的一个来源）。 */
    if (featNum == MXYTZ_FEAT_NO_AD) {
        g_pendAdReady.store(0, std::memory_order_relaxed);
        g_adV43FuseUntilMs.store(0, std::memory_order_relaxed);
        AdForceGateReset();  // ★v46b：一次性门重新上膛
        /* ★v47：免广告的「累计成功回调上限」改为**按激活期**计（见 `kAdV43MaxDone` 处注释）。
         *  旧语义是进程级永久 30 次 —— 看满 30 个广告后这个功能会**静默永久失效**，
         *  这正是用户报的「有时候功能会失效」的一种。现在每次开关都重新上膛。 */
        g_adV43Done.store(0, std::memory_order_relaxed);
        LOGI(TPL_LOG " AD43 待办/熔断窗口/配额已重置（免广告=%d）", enabled ? 1 : 0);
    }
    /* ★v47：#3 的「脏数据还原 + 限流状态重置」——**开关切换时立刻做**，不再只等下一次
     *  `Execute` 帧路过才还原（旧实现只有 `InterpFire` 的 K_PROJEXEC 分支会还原，
     *  如果关掉之后玩家的技能暂时不再施放，配置对象上就一直是放大值 ⇒ 用户看到
     *  「功能都关了伤害还是很高」）。 */
    if (featNum == MXYTZ_FEAT_SKILL_DMG) {
        ProjRestoreAllV47();
        ProjBudgetResetV47(true);
        LOGI(TPL_LOG " PROJ #3开关=%d：已还原投射物 count 并重置限流/看门狗（槽%d）",
             enabled ? 1 : 0, kProjSlotN);
    }
    /* ★v27：功能一开就**重开学习期**。目标方法是「按需调用」的 —— 尤其广告链路
     *  （`PlayADVideo` / `OnADVideoResult` / `DispatchResult`）只在玩家点广告时才被调用。
     *  用户在点广告**之前**打开「免广告」，学习期就能把那个方法学进来；窗口到期后再开功能会重开。
     *  已登记(1) 的保持，只把「已拒绝(2)」复位（让新开的功能有机会重新核对类名）。 */
    if (enabled) {
        for (size_t i = 0; i < kKindN; i++)
            if (__atomic_load_n(&g_kindDone[i], __ATOMIC_RELAXED) == 2)
                __atomic_store_n(&g_kindDone[i], 0u, __ATOMIC_RELAXED);
        g_interpCalls.store(0, std::memory_order_relaxed);
        g_learnMs.store(0, std::memory_order_relaxed);
        g_learnActive.store(1, std::memory_order_relaxed);
    }
    // v26i：功能一开就把「演员登记」预算重置 —— 之前 60 条 ACT? 上限被**启动期噪声**
    //   （AotUpdateBootstrap/协程状态机/代码页）吃光了，战斗期一条都打不出来（实测 battle 日志
    //   ACT?=0）。现在开功能才开始收：清空已登记队列，让战斗期对象独占预算。
    if (enabled && (featNum == MXYTZ_FEAT_KILL_MOB || featNum == MXYTZ_FEAT_INVINCIBLE ||
                    featNum == MXYTZ_FEAT_SKILL_DMG)) {
        g_actorSeenN.store(0, std::memory_order_relaxed);
        ActorResetScan();
        DBGLOG(TPL_LOG " SEEN 预算重置（功能已开，只收战斗期对象）");
    }
    SyncHooks();
}

void Mxytz_OnValueChanged(int featNum, int value) {
    (void)featNum;
    (void)value;  // 四项均为开关，无滑条
}

void Mxytz_Tick(const bool* features, const int* /*values*/) {
    // 只做：同步开关 + 兜底安装 + 方法指针观察（v6 诊断）。
    // 禁止在这里扫堆/遍历对象/SafeProbe；WatchSlots 只读观察表里 <32 个指针，且仅在值变化时查 maps。
    if (features) {
        g_killMob.store(features[MXYTZ_FEAT_KILL_MOB]);
        g_invincible.store(features[MXYTZ_FEAT_INVINCIBLE]);
        g_skillDmg.store(features[MXYTZ_FEAT_SKILL_DMG]);
        g_noAd.store(features[MXYTZ_FEAT_NO_AD]);
    }
    if (!g_libReady.load() && isLibraryLoaded(MXYTZ_TARGET_LIB)) Mxytz_TryInit(nullptr);

    /* ★★临时自验开关 —— **v27 已关（交付状态）**。原来它用来「不点菜单也能让 install 跑一遍」
     *   以确认 Dobby 挂点。v27 改成解释器入口分发后，证据行改由 `LEARN ok` / `INTERP` 提供；
     *   而它原来的副作用是致命的：把三个功能**强制打开**，配上旧版「按 mi 的分发表恒空」
     *   ⇒ 「入口 hook 必装 + 三功能强制开 + 一个都不生效」= 用户看到的「又卡又没功能」。
     *   ⚠️ 交付前必须保持 false；要自验请直接用菜单开关。 */
    static const bool kForceFeatTest = false;
    if (kForceFeatTest && g_processStartMs && NowMs() > g_processStartMs + 20000ull) {
        /* ★必须**每拍**强制，不能只做一次：本函数开头刚用 `features[]`（菜单真实状态，全 0）
         *  覆盖过这四个原子量，所以「只强制一次」会在下一拍立刻被覆盖回 false ⇒
         *  `SyncHooks` 的 `anyOn` 变 false 直接 return，`InstallInterpHook` 永远不跑
         *  （v26y 实测：全日志只有一条 `hooks deferred (gate) n=0`，之后再无任何 hook 行）。 */
        static bool s_forcedLog = false;
        g_killMob.store(true);
        g_invincible.store(true);
        g_skillDmg.store(true);
        if (!s_forcedLog) {
            s_forcedLog = true;
            DBGLOG(TPL_LOG " FORCETEST 三功能强制开启（自验用，交付前删除）");
        }
    }

    /* ★★v47 修「**功能有时候会失效**」—— 学习期**周期性补学**（fail-open 方向：只多学，不夺权）。
     *
     *  旧的关闭语义是三选一：`LearnAllDone()` / 预算耗尽 / 窗口到期（10 分钟）。
     *  关闭之后**没有任何再打开的通路**（除非用户手动关-开功能触发 `Mxytz_OnFeatureChanged`）。
     *  而目标方法是**按需调用**的：某个模式/关卡/首领战里才第一次被调用到的类上，
     *  只要那一轮没赶上学习窗口，它的 (名字, 类) 就**永远进不了表** ⇒ 那条功能在该场景里永久静默失效。
     *  这正是用户说的「有时候功能会失效」（换模式/换关卡后某一项没反应，重启又好了）的形状。
     *
     *  兜底：每 30 秒检查一次「**已经打开**的功能里，是否还有目标没登记」——
     *    · 有 ⇒ 重开一个 60 秒的学习窗口，并把缺口写进日志（下一轮可据此判断是哪个目标没学到）；
     *    · 没有 ⇒ 什么也不做（零开销：只有几个原子读）。
     *  开销上限：每次重开最多 60s 的慢路径，而慢路径已由首字节位图挡掉绝大多数无关名字。 */
    if (g_learnActive.load(std::memory_order_relaxed) == 0) {
        static uint64_t s_reopenMs = 0;
        const uint64_t nowR = NowMs();
        if (!s_reopenMs) s_reopenMs = nowR;
        if (nowR - s_reopenMs > 30000ull) {
            s_reopenMs = nowR;
            const bool wantKill = g_killMob.load(std::memory_order_relaxed);
            const bool wantInv = g_invincible.load(std::memory_order_relaxed);
            const bool wantSkill = g_skillDmg.load(std::memory_order_relaxed);
            const bool wantAd = g_noAd.load(std::memory_order_relaxed);
            char miss[160] = {0};
            size_t w = 0;
            size_t need = 0;
            for (size_t i = 0; i < kKindN; i++) {
                const uint16_t k = kKinds[i].kind;
                bool wanted = false;
                if (k & K_HURT) wanted = wantKill || wantInv || wantSkill;
                if (k & K_DOATTACK) wanted = wantInv;
                if (k & (K_PROJ | K_PROJEXEC)) wanted = wantSkill;
                if (k & (K_PLAYAD | K_ADRESULT | K_ADREWARD | K_CANCLAIM)) wanted = wantAd;
                /* K_DAMAGE / K_LOCALDMG 是「只取证」目标，缺了不影响任何功能 ⇒ 不参与补学 */
                if (!wanted) continue;
                if (__atomic_load_n(&g_kindDone[i], __ATOMIC_RELAXED) == 1) continue;
                need++;
                const int r = snprintf(miss + w, sizeof(miss) - w, "%s(%s) ", kKinds[i].tag, kKinds[i].cls);
                if (r > 0 && (size_t)r < sizeof(miss) - w) w += (size_t)r;
            }
            if (need) {
                g_interpCalls.store(0, std::memory_order_relaxed);
                g_learnMs.store(0, std::memory_order_relaxed);
                g_learnActive.store(1, std::memory_order_relaxed);
                LOGI(TPL_LOG " interp learn ★补学窗口重开★（已开功能仍有 %u 个目标未登记：%s）", (unsigned)need,
                     miss);
            }
        }
    }

    /* ★v27：删掉原来的「每 5 秒 `ResolveParaLayout()` + `PreflightDmgTarget()`」重试。
     *  这两条内部都会走 `FindClassAnywhere`（`domain_assemblies` 全镜像遍历 13755 个类，
     *  实测 375ms~1s/次）⇒ 每 5 秒一次的周期性长卡顿，是用户说的「很卡」的来源之一。
     *  现在 `CtagHurtHPPara` 的字段偏移改成**从活对象反查**（`ResolveParaLayoutFromObj`）：
     *  解释器入口第一次真的分发到 `Hurt` 时按需解析，零全镜像遍历。 */

    /* ★v27：学习期管理（替代原来的 `ResolveInterpTargets` 全镜像重试）。
     *  学习期只在「还有目标没登记」时为真；全登记完 / 预算耗尽 / 窗口到期即关，
     *  关掉后热路径退化为纯指针表查。这里只做三件廉价事：读一个原子量、每 30s 打一行状态、
     *  到期关闸并把未登记项列出来（hook-rules §25：装配自检必须能从日志看出来）。 */
    if (g_learnActive.load(std::memory_order_relaxed)) {
        const uint64_t st = g_learnMs.load(std::memory_order_relaxed);
        const uint64_t nowLg = NowMs();
        if (st == 0) {
            g_learnMs.store(nowLg, std::memory_order_relaxed);
            LOGI(TPL_LOG " interp learn 窗口开启（%u 秒，%u 个目标）", (unsigned)(kLearnWindowMs / 1000u),
                 (unsigned)kKindN);
        } else if (nowLg > st + kLearnWindowMs) {
            g_learnActive.store(0, std::memory_order_relaxed);
            char miss[192] = {0};
            size_t w = 0;
            for (size_t i = 0; i < kKindN; i++) {
                if (__atomic_load_n(&g_kindDone[i], __ATOMIC_RELAXED) == 1) continue;
                const int r = snprintf(miss + w, sizeof(miss) - w, "%s(%s) ", kKinds[i].tag,
                                       kKinds[i].cls);
                if (r <= 0 || (size_t)r >= sizeof(miss) - w) break;
                w += (size_t)r;
            }
            LOGI(TPL_LOG " interp learn 窗口到期 → 关（未登记：%s）", w ? miss : "无");
        } else {
            static uint64_t s_lgMs = 0;
            if (!s_lgMs || nowLg - s_lgMs > 30000ull) {
                s_lgMs = nowLg;
                uint32_t okc = 0;
                for (size_t i = 0; i < kKindN; i++)
                    if (__atomic_load_n(&g_kindDone[i], __ATOMIC_RELAXED) == 1) okc++;
                LOGI(TPL_LOG " interp learn 进行中 slow=%u 已登记=%u/%u",
                     g_interpCalls.load(std::memory_order_relaxed), okc, (unsigned)kKindN);
            }
        }
    }

    /* ★v27：删掉原来的「按 5 秒节流调 `ResolveInterpTargets()` 补目标表 + 12 轮后定案」。
     *  那段是「按 MethodInfo* 分发」路线的配套设施，已证明恒不命中（见 §⑩a 三条实证）；
     *  而且每次失败重试都要走 `FindClassAnywhere`/`ResolveClassPref` 全镜像遍历（375ms~1s）
     *  ⇒ 本身就是周期性长卡顿。现在目标表由**名字自学习**填充，学习期管理见上面的块。 */

    /* ★v27 诊断：普查窗口滚动（`kLearnProbe=false` 时整块被优化掉）。
     *  每 45s 清一次表 —— 不这样做的话，开局加载那一秒的几百个不同的对会把 400 行上限吃光，
     *  之后打怪时才出现的 `Hurt`/`DoAttack` 就永远打不出来（= 本次首测拿不到结论的原因）。 */
    if (kLearnProbe) {
        static uint64_t s_lpMs = 0;
        const uint64_t nowLp = NowMs();
        if (!s_lpMs) s_lpMs = nowLp;
        if (nowLp - s_lpMs > 45000ull) {
            s_lpMs = nowLp;
            if (g_lpN.load(std::memory_order_relaxed)) LearnProbeRollWindow();
        }
    }

    /* ★v44 交付：每 30s 一条汇总行，替代原来满屏的逐次诊断 —— 一眼看出「功能到底有没有在干活」。 */
    {
        static uint64_t s_statMs = 0;
        const uint64_t nowSt = NowMs();
        if (!s_statMs) s_statMs = nowSt;
        if (nowSt - s_statMs > 30000ull) {
            s_statMs = nowSt;
            LOGI(TPL_LOG " STATS 开关 kill=%d inv=%d skill=%d ad=%d | 命中 打怪=%u 无敌=%u 技能=%u 免广告=%u | 分流 dmg=%u ad=%u 闸拒=%u | AD43 见=%u 排队=%u 已回调=%u 熔断=%u 重入闸=%u",
                 g_killMob.load() ? 1 : 0, g_invincible.load() ? 1 : 0, g_skillDmg.load() ? 1 : 0,
                 g_noAd.load() ? 1 : 0, g_hitDmg.load(), g_hitInv.load(), g_hitSkill.load(),
                 g_hitAd.load(), g_dmgSeen.load(), g_adSeen.load(), g_gateFail.load(),
                 g_adV43Seen.load(), g_pendAdQueued.load(), g_adV43Done.load(),
                 g_adV43Fuse.load(), g_reentrySkip.load());
            /* ★v46b 诊断行：区分到底是哪一个 tag 在自激（真机 v46 只能看到总数 1087/s）。
             *  判读：开免广告前后两次 STATS-AD 的差值 / 30s 即为该 tag 的调用速率；
             *  `门` 三列 = 上锁通过次数（应≈1/2s）、被门拦下次数（大就说明上游在空转）。 */
            LOGI(TPL_LOG " STATS-AD watch=%u watch0=%u click=%u canclaim=%u vresult=%u playad=%u result=%u | 门 canclaim=%u/%u vresult=%u/%u interp=%u/%u",
                 g_adTagCnt[kAdTagWatch].load(), g_adTagCnt[kAdTagWatch0].load(),
                 g_adTagCnt[kAdTagClick].load(), g_adTagCnt[kAdTagCanClaim].load(),
                 g_adTagCnt[kAdTagVResult].load(), g_adTagCnt[kAdTagPlayAd].load(),
                 g_adTagCnt[kAdTagResult].load(), g_adGatePass[kAdGateCanClaim].load(),
                 g_adGateSkip[kAdGateCanClaim].load(), g_adGatePass[kAdGateVResult].load(),
                 g_adGateSkip[kAdGateVResult].load(), g_adGatePass[kAdGateInterp].load(),
                 g_adGateSkip[kAdGateInterp].load());
            /* ★v47 诊断行：#3 的**限流/看门狗**可见性（用户报「玩久了掉帧闪退是技能伤害增加
             *  导致的」→ 下一轮用真机日志判断预算该调大还是调小，而不是继续凭猜）。
             *  判读：`单秒峰值` 一直贴着 `上限`（或 `拦下` 持续增长）= 场面确实被限流，
             *  说明预算取小了；`降级=0 拦下=0` 且峰值远低于上限 = 正常游玩不触发兜底。
             *  `槽` = 当前记录的「被放大过的技能配置对象」数量（还原表的占用），越大越接近槽容量。 */
            {
                uint32_t slotUsed = 0;
                for (int i = 0; i < kProjSlotN; i++)
                    if (g_projSlotV36[i].data) slotUsed++;
                const uint64_t su = g_projSuspendUntilMs.load(std::memory_order_relaxed);
                LOGI(TPL_LOG
                     " STATS-PROJ fanout=%.1f 上限%d/次 预算%d/秒 | 单秒峰值=%u 拦下=%u 降级=%u 挂起=%s | 写count=%u 槽=%u/%d",
                     (double)kProjFanout, (int)kProjCountMax, (int)kProjBoostPerSecMax,
                     ProjEmitPeakTakeV47(), g_projBudgetSkip.load(), g_projSuspendN.load(),
                     (su && NowMs() < su) ? "是" : "否", g_projSetV36.load(), slotUsed, kProjSlotN);
            }
        }
    }

    SyncHooks();
    /* v44 清理：删掉 `kProbeDump`（字段/名册诊断件，每 2s 一轮全量 class 遍历 + WatchSlots + ProbePass）
     *  与 `kDataPlane`（v20 老数据面，每 2s 一次 787MB 堆扫描，实测 `ok=0 bad=N`、`CALIB=0` 从未标定）
     *  两个**永久关闭**的死开关及其全部实现。二者都只在诊断期使用，正式版一行不留。 */
    ActorApplyTick();  // v26：演员图三功能（400ms 节流；只写 hook 收到的真实演员对象）
    /* ★v27：删掉原来的「解释器取证保命闸（装 hook 3 小时后自动静音）」。
     *  那个闸当初只为「诊断取证别开太久」，而现在入口分发**就是功能的挂载点** ——
     *  跟着一起静音会让玩满 3 小时后四项功能集体消失（v26y 已踩过一次：目标分发被放在该闸之后）。 */
}

void Mxytz_ForceClear() {
    g_killMob.store(false);
    g_invincible.store(false);
    g_skillDmg.store(false);
    g_noAd.store(false);
    g_pendAdReady.store(0, std::memory_order_relaxed);  // ★v46：清掉未执行的免广告待办
    g_adV43FuseUntilMs.store(0, std::memory_order_relaxed);
    g_adV43Done.store(0, std::memory_order_relaxed);     // ★v47：配额一起重新上膛
    AdForceGateReset();                                  // ★v46b：一次性门重新上膛
    /* ★v47：ForceClear 也要把 #3 写进游戏配置对象的 `_count` 还原（旧实现漏了这一步 ——
     *  它是「全部关闭后伤害还是很高」这类残留的另一条来源），并清空限流/看门狗状态。 */
    ProjRestoreAllV47();
    ProjBudgetResetV47(true);
    DBGLOG(TPL_LOG " ForceClear hits dmg=%u inv=%u skill=%u ad=%u", g_hitDmg.load(), g_hitInv.load(),
         g_hitSkill.load(), g_hitAd.load());
}
