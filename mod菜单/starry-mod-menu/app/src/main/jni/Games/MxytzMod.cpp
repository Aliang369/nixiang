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
#include "MxytzBattle.h"   // 战斗三功能（v60 全新实现，独立文件）

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
/* ★v50 交付档：诊断闸关回 0（v49 诊断轮的普查结论已固化在文档里：
 *   两条入口 `ProtocolManager::sendEvent` 与 `DS_Advertising::OnGetADInfoRsp` **都经过**我们的挂点；
 *   结构发现在 `EventItemInt+0x20` 拿到 `Proto_CSRsp`。此后只需看 AD50 自检行）。 */
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

static std::atomic<bool> g_hurtStatic{true};
static std::atomic<bool> g_doAttackStatic{false};
static std::atomic<bool> g_playAdStatic{false};
static std::atomic<bool> g_onResultStatic{false};  // DS_Advertising::OnADVideoResult 是否静态

static size_t g_offAcceptIgo = 0;  // IAccept.interactiveGameObject
static void* g_acceptKlass = nullptr;


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

static bool g_installedDmg = false;
static bool g_installedAd = false;

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
    /* ★v48：**#4 免广告的「不出画」落点** —— `DS_Advertising::OnGetADInfoRsp(Proto_CSRsp)`
     *  （token `0x06001253`，argc=1，实例方法）。铁证见 `ApplyNoAdRspV48` 的整条 IL 注释：
     *  游戏在这一帧里判断 `start_watch_ad_rsp.result`，**等于 0 才去调 `PlayADVideo` 拉 SDK**；
     *  非 0（尤其 `==2`「已使用免广告特权跳过观看」）则改走它自己的 `DispatchResult(ad_id,true)`。
     *  ⇒ 在建帧时刻把 `result` 写成 2，广告就**根本不会被拉起**（v47 只是「代回调发奖」，
     *    所以用户真机看到「奖励到手了、广告照样跳出来」）。 */
    K_GETADRSP = 1u << 11,
    /* ★v49：**委托跳不可见，但它的调用者可见** —— 真机 C# 栈证据：
     *   `Engine:FixedUpdate → ProtocolManager:update → ProtocolManager:sendEvent(EventItemInt)
     *    → [委托跳：不可见] → DS_Advertising:OnGetADInfoRsp(Proto_CSRsp)`
     *  `sendEvent` 是热更→热更调用（pushframe，实测可见），且**先于** `OnGetADInfoRsp` 执行
     *  ⇒ 在这一帧用**结构发现**取出 `Proto_CSRsp`，就能在 `get_result()` 被读之前写 `result=2`。 */
    K_SENDEVENT = 1u << 12,
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
    /* ★v48：#4 免广告「不出画」的落点（见 `K_GETADRSP` 与 `ApplyNoAdRspV48`）。
     *  类名必须一起比 —— `OnGetADInfoRsp` 是网络回调，别的方法名混不进来，但仍按 §⑩a 的
     *  「名字 + 类」双键登记（LearnSlot::klass），与其它目标一致。 */
    {"DS_Advertising", "OnGetADInfoRsp", K_GETADRSP, "GetAdRsp"},
    /* ★v49：走得到的落点（见 `K_SENDEVENT`）。类名一起比；argc 用 1（`sendEvent(EventItemInt)`）。 */
    {"ProtocolManager", "sendEvent", K_SENDEVENT, "SendEvent"},
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
/* ★v50：**预登记**标记（`PreRegisterKindV43` 成功登记过的目标）。
 *  为什么需要：预登记只往快表写槽、**不置 `g_kindDone`**，于是 v47 的「补学」检查会把它们
 *  永远当成"未登记" ⇒ 每 30 秒重开一次 60 秒学习窗（真机 v47 日志可见 `slow=` 持续增长）。
 *  补学只该管「*没*预登记、又一直没被慢路径学到」的目标。 */
static uint8_t g_kindPre[kKindN] = {};
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
/* ★v48：`OnGetADInfoRsp` 路径（见 `ApplyNoAdRspV48`）的独立计数 —— 用来在真机日志里
 *  一眼分辨「广告是被这条新路径拦下的」还是「回落到 v47 的 PlayADVideo 路径」。 */
static std::atomic<uint32_t> g_adV48Seen{0};        // 入口①（OnGetADInfoRsp）把 rsp 送进核心的次数
static std::atomic<uint32_t> g_adV48Wrote{0};       // 成功把 result 写成 2 的次数（= 不拉 SDK）
/* ★v49：入口②（`ProtocolManager::sendEvent`）的独立计数 —— 与入口①分开，日志里一眼看出是哪条路生效。 */
static std::atomic<uint32_t> g_adV49Event{0};       // sendEvent 帧被分发到的次数
static std::atomic<uint32_t> g_adV49Found{0};       // 结构发现在该帧里找到 rsp 的次数
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
/* ★v48：登记闸的前置声明（定义在下方 `ApplyNoAdV43` 之后，两条登记路径共用；
 *  这里只是为了「先声明后使用」—— `ApplyNoAdV43` 也要用它）。 */
static bool NoAdEnqueueGatedV48(int32_t adId, uint64_t uuid, const char* src);

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
    /* ★v48：闸①熔断窗口 / 闸②累计配额 / mi 就绪 —— 抽成 `NoAdEnqueueGatedV48` 与
     *  `OnGetADInfoRsp` 路径共用（判据只留一份，避免两处漂移）。 */
    NoAdEnqueueGatedV48(adId, uuid, "interp");
}

/** ★v48：登记前统一过闸（熔断窗口 / 累计配额 / 回调 MethodInfo 是否就绪）。
 *  两条登记路径（`ApplyNoAdV43` 的 PlayADVideo 帧、`ApplyNoAdRspV48` 的 OnGetADInfoRsp 帧）
 *  共用，避免两处判据漂移。`src` 只进日志。 */
static bool NoAdEnqueueGatedV48(int32_t adId, uint64_t uuid, const char* src) {
    if (g_adV43FuseUntilMs.load(std::memory_order_relaxed) > NowMs()) return false;
    if (g_adV43Done.load(std::memory_order_relaxed) >= kAdV43MaxDone) return false;
    if (!g_miOnAdResultV43) return false;
    EnqueueNoAdV43(adId, uuid, src);
    return true;
}

/* ============================================================================
 * ★★v48 #4 免广告「广告还是会跳出来」的**机制级修复**（用户 2026-10-16 实测：
 *   「免广告可以获得奖励，但是广告还是会跳出来」）
 *
 * 先纠正一条被推翻的旧结论：v46b 记录「本机厂商 SDK 根本不出画」⇒ 当时以为
 * `PlayADVideo` 不会被真正执行、于是只做了「代回调发奖」。**真机实测证明它会出画**
 * ⇒ 只代回调不够，必须让游戏**根本不调** `PlayADVideo`。
 *
 * 铁证（官方 `Assembly-CSharp` 元数据 + IL，本机 dump `06_dumped/冒险与征途/aot/_tgt_il_ad2.txt`，
 * 与 `_钩子目标与语义.md` §11 的调用链一致）——`DS_Advertising::OnGetADInfoRsp 0x06001253`：
 *   ```
 *   IL_0043: ldloc.0 / callvirt Proto_SCStartWatchAdRsp::get_result()
 *   IL_0049: brtrue.s 31                      // result != 0 → 跳 IL_006A
 *   IL_004B: ldsfld SDKProxy::Instance
 *   IL_0050..005F: ad_id, uuid.ToString()
 *   IL_0064: callvirt BaseSDKAdapter::PlayADVideo(int,string) -> void   ← ★广告从这里出画
 *   IL_0069: ret
 *   IL_006A: DispatchResult(ad_id, true)      // ← 官方「不需要看视频」分支（本地事件）
 *   IL_0077: get_result() / ldc.i4.2 / bne.un ← result==2 时补一句提示
 *   IL_0080: ldstr "已使用免广告特权跳过观看"
 *   ```
 *  ⇒ **`start_watch_ad_rsp.result != 0` 就是游戏自己的「免广告特权」开关**：
 *     它走 `DispatchResult(ad_id,true)`（= `EventManager.send("Event_WatchADResult",[ad_id])`），
 *     并且**永远到不了 `PlayADVideo`** ⇒ 厂商 SDK 不被拉起 ⇒ **不播广告**。
 *
 * 本函数在 `OnGetADInfoRsp` 的**建帧时刻**（方法体还没跑、`get_result()` 还没读）把
 * `result` 写成 **2**（= 官方「已使用免广告特权跳过观看」那个值），于是：
 *   ① 广告视频不再出现（IL 走 else 分支，`PlayADVideo` 一次都不会被调用）；
 *   ② 游戏自己发本地事件 + 提示「已使用免广告特权跳过观看」（正是我们要的语义）；
 *   ③ **发奖链路一字不改**：奖励仍由本 mod 的代回调走官方 `OnADVideoResult(true, ad_id, uuid)`
 *      → `ReqADVideoResult` → cmd 2762（结果与 v46b/v47 完全一致，用户实测已通过的那条）。
 *
 * 为什么必须用「写服务端响应里的字段」而不是「不调 orig」：
 *   `OnGetADInfoRsp` 与它要调的 `PlayADVideo` 都是**热更解释方法**，`callvirt` 走的是
 *   解释器体内的**建帧桩** `0x207DCCC`（`Hooked_PushFrame`）——该桩的返回值 x0 被调用方
 *   接着用于 `str x23,[x0,#0x18]`（frame->obj），**跳过 orig 会让调用方拿着野指针写内存**
 *   ⇒ 在建帧桩上「不调 orig」物理上不成立。写数据字段则完全安全（`allowWrite=true`）。
 *
 * 自证三道（缺一不写，fail-safe；任何一步失败都退回「广告照播但奖励照发」的 v47 行为）：
 *   ① 参数对象类名必须含 `Proto_CSRsp`；
 *   ② 按名解析 `start_watch_ad_rsp` 拿到的子对象类名必须含 `SCStartWatchAdRsp`；
 *   ③ 子对象上 `result` 字段按名解析成功且偏移 ≤0x400 ⇒ 此刻读到的值==0 才动（非 0 说明
 *      服务端自己就不要求看，让它走原分支，我们只登记代回调）。 */
/** ★v49：把 v48 的「写 result=2」逻辑抽成**核心**（入参 = 已经确认是 `Proto_CSRsp` 的对象）。
 *  两个入口共用它：① `ApplyNoAdRspV48`（`DS_Advertising::OnGetADInfoRsp` 帧）；
 *  ② `ApplyNoAdRspFromEventV49`（`ProtocolManager::sendEvent` 帧，结构发现拿到 rsp）。
 *  **每一次 bail 都打日志**（前若干次）—— v48 全线静默，导致真机上无法判断「没进函数」
 *  还是「进了但自证没过」，这一版把可观测性补齐（这是本轮唯一的诊断性改动）。 */
/* ============================================================================
 * ★v50 —— **不再猜字段名**：改成调用**游戏自己的 getter/setter**。
 *
 * v49 的真机证据（`AD50 结构发现：容器 cls=EventItemInt 的 +0x20 就是 Proto_CSRsp`，
 * 紧接 `AD49 rsp 字段 cls=Proto_CSRsp start_watch_ad_rsp=+0x0`）：**两条入口都可见**
 * （`INTERP SendEvent kinds=0x1000 … hit=0..11`、`INTERP GetAdRsp kinds=0x0800 … hit=0`），
 * 结构发现也成功，唯独 `FieldOffset(Proto_CSRsp,"start_watch_ad_rsp")` 返回 **0** ——
 * 说明 `Proto_CSRsp` 上那个字段**不叫这个名字**（该族类用的是自动属性 `<X>k__BackingField`
 * 命名，本作已有实证：`ActionGameLibrary.DamageFactory::<IsPVPBattleMode>k__BackingField`、
 * `InteractiveGameObject::<ZAxis>k__BackingField`，见 `06_dumped/…/aot/_tgt_il_df.txt:2273`、
 * `_tgt_il_igo.txt:185`）。继续猜字段名是错的路子。
 *
 * 现在改为**逐字使用 IL 里出现的公开成员名**（`_tgt_il_ad2.txt` 的 `OnGetADInfoRsp 0x06001253`）：
 *   `Proto_CSRsp::get_start_watch_ad_rsp()`   → 取子消息（不必知道它存在哪个字段）
 *   `Proto_SCStartWatchAdRsp::get_result() / get_ad_id() / get_uuid()` → 读三个值
 *   `Proto_SCStartWatchAdRsp::set_result(int)` → **写 2**（走游戏自己的 setter，连 protobuf 的
 *                                               变更跟踪一起走，比裸写字段更"正"）
 * 全部经 `runtime_invoke` 调用（本作已验证可用：免广告代回调 `DrainPendingAdV43`、
 * `CdInvoke` 都走它；invoke 期间打 `t_modInvoke` 标 ⇒ 被调方内部的解释器入口跳过派发，
 * 不构成帧内递归）。这些名字长且非混淆（`hook-rules.md` §24 指纹铁律）⇒ 可安全写死。
 *
 * 三级 fail-safe 与 v49 相同：不是 `Proto_CSRsp` / 取不到子消息 / 取不到 mi → 一个字节不写；
 * `result != 0` → 不干预；**先登记代回调成功才写**（奖励优先）。写完再用 `get_result()` 复核。 */
static void* NoAdMiGetV50(void* klass, const char* name, int argc) {
    auto& api = Il2Cpp::GetApi();
    void* mi = api.class_method ? api.class_method(klass, name, argc) : nullptr;
    /* 降级：`Api::GetMethod` 内部先试 `class_method`，失败再走 `GetMethodScan`
     *（class_methods 迭代 + 20000 次防御上限）。**只在按类缓存未命中时调用**，不在热路径。 */
    if (!mi) mi = api.GetMethod(klass, name, argc);
    return mi;
}

/* ============================================================================
 * ★v51 —— **`runtime_invoke` 返回值约定的双读自证**（v50 真机血证驱动的修正）
 *
 * v50 真机日志（`AD50[sendEvent]/AD50[rsp] 子对象 cls=0WŐt 不是 SCStartWatchAdRsp`）证明：
 * 我按「`r` = 指向 retval 槽的指针」去读**引用类型**返回值是错的 —— 实际 `r` **就是对象本身**
 * （`*(void**)r` 读到的是对象的 klass，于是 `class_name(klass)` 打出 `0WŐt` 这种垃圾名字）。
 * il2cpp 的 `il2cpp_runtime_invoke` 对**引用类型直接返回对象指针**、对**值类型**（bool/int/ulong）
 * 另有约定（值类型 = **装箱对象、真值在 `+0x10`** —— v50→v52 已在本作定案，规则见
 * `skill/re-implement/il2cpp-abi-pitfalls.md` 坑 9；`CdInvoke` 的旧写法 `*(uint8_t*)r` 属读错偏移，
 * 见其函数头 ★v53 注释与 `10_docs/冒险与征途/技能先例.md` §2）。
 *
 * 所以这里不再赌约定，而是**双读 + 自证**（全部 fail-safe：任何一条不成立就返回失败，
 * 让广告照播、奖励照发）：
 *   · 引用类型：候选① `r` 本身是对象、候选② `*(void**)r` 是对象 —— **用期望的类名子串自证**
 *     （`get_start_watch_ad_rsp` 只可能是 `SCStartWatchAdRsp`）；
 *   · 值类型：候选① `*(T*)r`、候选② `*(T*)((char*)r + 0x10)`（装箱对象的数据区）——
 *     用 **`ad_id` 必须在 [1, 100000)** 这条业务约束自证（真机实测 adid=7/13/17），
 *     并把选中的约定**缓存 + 打一行日志**（`AD51 retval 约定=A|B`），之后不再双读。
 * ============================================================================ */
static int g_noAdRetvalDirectV51 = -1;  // -1 未定案 / 1 = 候选A（读 r） / 0 = 候选B（读 r+0x10）

/** 调 0 参 getter，返回 `runtime_invoke` 的原始返回值指针（失败返回 nullptr）。 */
static void* NoAdInvokeGetRawV52(void* mi, void* obj) {
    if (!mi || !LivePtr(obj)) return nullptr;
    auto& api = Il2Cpp::GetApi();
    if (!api.runtime_invoke) return nullptr;
    void* exc = nullptr;
    t_modInvoke++;
    void* r = api.runtime_invoke(mi, obj, nullptr, &exc);
    t_modInvoke--;
    if (!r || exc) return nullptr;
    return r;
}

/** 按候选约定从原始返回值里取出 64 位：`direct==1` 读 `r`（值类型直接给值）、
 *  `direct==0` 读 `r+0x10`（装箱对象的数据区）。 */
static void NoAdReadRawV52(void* r, int direct, uint64_t* out) {
    *out = 0;
    if (!r) return;
    memcpy(out, (direct == 1) ? r : (const char*)r + 0x10, 8);
}

/** ★v52：三个标量一次性**双约定读取 + 业务约束定案**（比 v51 只用 ad_id 更稳）。
 *  判据（全部来自真机实测值）：`uuid ≈ 1.31e17`（19 位）⇒ `> 1e15`；`ad_id = 7/13/17` ⇒ `(0,1e5)`；
 *  `result ∈ {0,1,2}` ⇒ `[0,16]`。**两种约定都打印一次**（`AD52 raw` 行），便于日后核对。
 *  两种都不满足 ⇒ 不定案、不写任何东西（fail-safe：广告照播、奖励走 AD43 老路）。 */
struct NoAdScalarsV52 {
    int32_t adId;
    int32_t res;
    uint64_t uuid;
    bool ok;
};
static NoAdScalarsV52 NoAdReadScalarsV52(void* miAdId, void* miRes, void* miUuid, void* sub,
                                        const char* src) {
    NoAdScalarsV52 o = {0, 0, 0, false};
    void* rA = NoAdInvokeGetRawV52(miAdId, sub);
    void* rR = NoAdInvokeGetRawV52(miRes, sub);
    void* rU = NoAdInvokeGetRawV52(miUuid, sub);
    if (!rA || !rR || !rU) {
        static std::atomic<uint32_t> s_e{0};
        if (s_e.fetch_add(1, std::memory_order_relaxed) < 6)
            LOGW(TPL_LOG " AD52[%s] getter 调用失败（exc）⇒ 不干预", src);
        return o;
    }
    uint64_t a1 = 0, r1 = 0, u1 = 0, a0 = 0, r0 = 0, u0 = 0;
    NoAdReadRawV52(rA, 1, &a1); NoAdReadRawV52(rR, 1, &r1); NoAdReadRawV52(rU, 1, &u1);
    NoAdReadRawV52(rA, 0, &a0); NoAdReadRawV52(rR, 0, &r0); NoAdReadRawV52(rU, 0, &u0);
    static std::atomic<uint32_t> s_raw{0};
    if (s_raw.fetch_add(1, std::memory_order_relaxed) < 4)
        LOGI(TPL_LOG
             " AD52[%s] raw 候选A(读r): adid=%d result=%d uuid=%llu | 候选B(r+0x10): adid=%d result=%d uuid=%llu",
             src, (int32_t)(a1 & 0xFFFFFFFFu), (int32_t)(r1 & 0xFFFFFFFFu), (unsigned long long)u1,
             (int32_t)(a0 & 0xFFFFFFFFu), (int32_t)(r0 & 0xFFFFFFFFu), (unsigned long long)u0);
    const int32_t ad1 = (int32_t)(a1 & 0xFFFFFFFFu), rs1 = (int32_t)(r1 & 0xFFFFFFFFu);
    const int32_t ad0 = (int32_t)(a0 & 0xFFFFFFFFu), rs0 = (int32_t)(r0 & 0xFFFFFFFFu);
    const bool ok1 = (u1 > 1000000000000000ull) && ad1 > 0 && ad1 < 100000 && rs1 >= 0 && rs1 <= 16;
    const bool ok0 = (u0 > 1000000000000000ull) && ad0 > 0 && ad0 < 100000 && rs0 >= 0 && rs0 <= 16;
    int conv = g_noAdRetvalDirectV51;
    if (conv < 0) {
        if (ok1) conv = 1;
        else if (ok0) conv = 0;
        else {
            static std::atomic<uint32_t> s_nd{0};
            if (s_nd.fetch_add(1, std::memory_order_relaxed) < 6)
                LOGW(TPL_LOG
                     " AD52[%s] 两种返回值约定都不满足业务约束（A: adid=%d res=%d uuid=%llu | "
                     "B: adid=%d res=%d uuid=%llu）⇒ 不定案、不干预",
                     src, ad1, rs1, (unsigned long long)u1, ad0, rs0, (unsigned long long)u0);
            return o;
        }
        g_noAdRetvalDirectV51 = conv;
        LOGI(TPL_LOG " AD52 retval 约定定案=%s（adid=%d result=%d uuid=%llu）", conv ? "A(读 r)" : "B(读 r+0x10)",
             conv ? ad1 : ad0, conv ? rs1 : rs0, (unsigned long long)(conv ? u1 : u0));
    }
    o.adId = (conv == 1) ? ad1 : ad0;
    o.res = (conv == 1) ? rs1 : rs0;
    o.uuid = (conv == 1) ? u1 : u0;
    o.ok = true;
    return o;
}

/** 调 0 参 getter 取对象引用：**双读 + 类名自证**（见上方说明）。 */
static void* NoAdCallGetObjV51(void* mi, void* obj, const char* expect) {
    if (!mi || !LivePtr(obj)) return nullptr;
    auto& api = Il2Cpp::GetApi();
    if (!api.runtime_invoke || !api.object_class || !api.class_name) return nullptr;
    void* exc = nullptr;
    t_modInvoke++;
    void* r = api.runtime_invoke(mi, obj, nullptr, &exc);
    t_modInvoke--;
    if (!r || exc) return nullptr;
    /* 候选①：`r` 本身就是对象（v50 实测的真约定）；候选②：`r` 指向 retval 槽。 */
    void* cand[2] = {r, nullptr};
    memcpy(&cand[1], r, 8);
    for (int i = 0; i < 2; i++) {
        void* p = cand[i];
        if (!LivePtr(p) || !Il2Cpp::IsReadableRange((uintptr_t)p, 0x10)) continue;
        void* k = api.object_class(p);
        if (!LivePtr(k)) continue;
        const char* n = api.class_name(k);
        if (n && strstr(n, expect)) {
            static std::atomic<uint32_t> s_conv{0};
            if (s_conv.fetch_add(1, std::memory_order_relaxed) < 3)
                LOGI(TPL_LOG " AD51 引用返回约定=候选%d（cls=%s，期望含 %s）", i + 1, n, expect);
            return p;
        }
    }
    static std::atomic<uint32_t> s_bad{0};
    if (s_bad.fetch_add(1, std::memory_order_relaxed) < 6) {
        void* k0 = api.object_class(cand[0]);
        const char* n0 = LivePtr(k0) ? api.class_name(k0) : "?";
        void* k1 = LivePtr(cand[1]) ? api.object_class(cand[1]) : nullptr;
        const char* n1 = LivePtr(k1) ? api.class_name(k1) : "?";
        LOGW(TPL_LOG " AD51 引用返回的两种读法都不像 %s（r=%p 候选1 cls=%s / 候选2 cls=%s）", expect, r,
             n0 ? n0 : "?", n1 ? n1 : "?");
    }
    return nullptr;
}

/** 调 1 参 setter 写 int32（走游戏自己的 setter）。 */
static bool NoAdCallSetI32V51(void* mi, void* obj, int32_t val) {
    if (!mi || !LivePtr(obj)) return false;
    auto& api = Il2Cpp::GetApi();
    if (!api.runtime_invoke) return false;
    void* exc = nullptr;
    int32_t v = val;
    void* args[1] = {&v};
    t_modInvoke++;
    void* r = api.runtime_invoke(mi, obj, args, &exc);
    t_modInvoke--;
    (void)r;
    return exc == nullptr;
}

static void NoAdRspCoreV50(void* rsp, const char* src) {
    if (!g_noAd.load(std::memory_order_relaxed)) return;
    static std::atomic<uint32_t> s_bailCls{0}, s_bailSub{0}, s_bailMi{0}, s_nosub{0}, s_noq{0}, s_skip{0};
    auto& api = Il2Cpp::GetApi();
    if (!api.object_class || !api.class_name) return;
    if (!LivePtr(rsp) || !Il2Cpp::IsReadableRange((uintptr_t)rsp, 0x10)) return;
    /* ── 自证①：必须是 Proto_CSRsp ── */
    void* rk = api.object_class(rsp);
    if (!LivePtr(rk)) return;
    const char* rn = api.class_name(rk);
    if (!rn || !strstr(rn, "Proto_CSRsp")) {
        if (s_bailCls.fetch_add(1, std::memory_order_relaxed) < 6)
            LOGW(TPL_LOG " AD50[%s] rsp=%p cls=%s 不是 Proto_CSRsp → 放弃", src, rsp, rn ? rn : "?");
        return;
    }
    /* ── MI 解析（按 rsp 类缓存）：get_start_watch_ad_rsp() ── */
    static const void* s_rk = nullptr;
    static void* s_getSub = nullptr;
    if (s_rk != rk) {
        s_rk = rk;
        s_getSub = NoAdMiGetV50(rk, "get_start_watch_ad_rsp", 0);
        LOGI(TPL_LOG " AD50 rsp 类 %s :: get_start_watch_ad_rsp=%p（走游戏 getter，不碰字段）", rn,
             s_getSub);
    }
    if (!s_getSub) {
        if (s_bailMi.fetch_add(1, std::memory_order_relaxed) < 6)
            LOGW(TPL_LOG " AD50[%s] 解析不到 get_start_watch_ad_rsp → 放弃", src);
        return;
    }
    void* sub = NoAdCallGetObjV51(s_getSub, rsp, "SCStartWatchAdRsp");
    if (!sub) {
        if (s_nosub.fetch_add(1, std::memory_order_relaxed) < 3)
            DBGLOG(TPL_LOG " AD50[%s] 本条 rsp 没有 start_watch_ad_rsp 子消息 → 忽略", src);
        return;
    }
    /* ── 自证②：子对象必须是 SCStartWatchAdRsp ── */
    void* sk = api.object_class(sub);
    const char* sn = LivePtr(sk) ? api.class_name(sk) : nullptr;
    if (!sn || !strstr(sn, "SCStartWatchAdRsp")) {
        if (s_bailSub.fetch_add(1, std::memory_order_relaxed) < 6)
            LOGW(TPL_LOG " AD50[%s] 子对象 cls=%s 不是 SCStartWatchAdRsp → 放弃", src, sn ? sn : "?");
        return;
    }
    /* ── MI 解析（按子类缓存）：get_result / get_ad_id / get_uuid / set_result ── */
    static const void* s_sk = nullptr;
    static void* s_getRes = nullptr;
    static void* s_getAdId = nullptr;
    static void* s_getUuid = nullptr;
    static void* s_setRes = nullptr;
    if (s_sk != sk) {
        s_sk = sk;
        s_getRes = NoAdMiGetV50(sk, "get_result", 0);
        s_getAdId = NoAdMiGetV50(sk, "get_ad_id", 0);
        s_getUuid = NoAdMiGetV50(sk, "get_uuid", 0);
        s_setRes = NoAdMiGetV50(sk, "set_result", 1);
        LOGI(TPL_LOG " AD50 子类 %s :: get_result=%p get_ad_id=%p get_uuid=%p set_result=%p", sn, s_getRes,
             s_getAdId, s_getUuid, s_setRes);
    }
    if (!s_getRes || !s_getAdId || !s_getUuid || !s_setRes) {
        if (s_bailMi.fetch_add(1, std::memory_order_relaxed) < 6)
            LOGW(TPL_LOG " AD50[%s] 子类成员解析不全（res=%p adid=%p uuid=%p set=%p）→ 本次不干预", src,
                 s_getRes, s_getAdId, s_getUuid, s_setRes);
        return;
    }
    /* ★v52：三个标量一次性双约定读取 + 业务约束定案（见 `NoAdReadScalarsV52` 注释） */
    const NoAdScalarsV52 sc = NoAdReadScalarsV52(s_getAdId, s_getRes, s_getUuid, sub, src);
    if (!sc.ok) return;
    const int32_t adId = sc.adId;
    const int32_t res = sc.res;
    const uint64_t uuid = sc.uuid;
    const uint32_t n = g_adV48Seen.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 12 || (n % 20) == 0)
        LOGI(TPL_LOG " AD50[%s] 读到 adid=%d uuid=%llu result=%d（第%u次）", src, adId,
             (unsigned long long)uuid, res, n);
    /* ★v51 二次自证：**uuid 必须像服务端真 uuid**（真机实测值 ≈1.31e17 的 19 位十进制）。
     *  为什么必须挡：这条路的 uuid 是我们 invoke 读出来的，一旦读错就会把**错的 uuid** 交给
     *  代回调 ⇒ cmd 2762 被服务端拒 ⇒ 奖励拿不到（而这条路的 AD43 兜底又因为我们成功拦下了
     *  `PlayADVideo` 而不会启动）。所以「不像真 uuid」时**一律不登记、不写**，退回「广告照播 +
     *  AD43 路径发奖」的既有行为（奖励优先）。 */
    if (uuid < 1000000000000000ull) {
        static std::atomic<uint32_t> s_badu{0};
        if (s_badu.fetch_add(1, std::memory_order_relaxed) < 6)
            LOGW(TPL_LOG " AD50[%s] uuid=%llu 不像服务端 uuid → 不拦截（奖励优先）", src,
                 (unsigned long long)uuid);
        return;
    }
    /* 服务端自己就不要求看（`result != 0`，含 `==2` 官方「免广告特权跳过」）⇒ **一律不干预**：
     *  这一支游戏本来就不拉 SDK，而且它**不需要**客户端再发 cmd 2762（奖励由服务端在 start
     *  阶段就发了）⇒ 我们若多补一发 finish 包就是重复上报，风险大于收益。 */
    if (res != 0) {
        if (s_skip.fetch_add(1, std::memory_order_relaxed) < 3)
            LOGI(TPL_LOG " AD50[%s] result=%d（服务端已跳过观看）⇒ 不干预", src, res);
        return;
    }
    /* 奖励优先：代回调没登记成功就一个字节都不写，让广告照播（= v47 行为）。 */
    if (!NoAdEnqueueGatedV48(adId, uuid, src)) {
        if (s_noq.fetch_add(1, std::memory_order_relaxed) < 5)
            LOGW(TPL_LOG " AD50[%s] 代回调未登记（熔断/配额/mi）⇒ 本次不拦截（奖励优先）", src);
        return;
    }
    if (!NoAdCallSetI32V51(s_setRes, sub, 2)) {
        if (s_bailMi.fetch_add(1, std::memory_order_relaxed) < 6)
            LOGW(TPL_LOG " AD50[%s] set_result(2) 抛异常 → 本次不拦截", src);
        return;
    }
    /* 写后复核：再用游戏自己的 getter 读一次。`复核=2` = 「广告不会再出画」已实测成立。 */
    void* rBack = NoAdInvokeGetRawV52(s_getRes, sub);
    uint64_t backRaw = 0;
    bool ok2 = (rBack != nullptr);
    if (ok2) NoAdReadRawV52(rBack, (g_noAdRetvalDirectV51 == 0) ? 0 : 1, &backRaw);
    const int32_t back = (int32_t)(backRaw & 0xFFFFFFFFu);
    const uint32_t w = g_adV48Wrote.fetch_add(1, std::memory_order_relaxed) + 1;
    if (w <= 12 || (w % 20) == 0)
        LOGI(TPL_LOG
             " AD50[%s] ★set_result(2)：result %d→%d（getter 复核 ok=%d）⇒ 游戏走自带"
             "「已使用免广告特权跳过观看」分支、不拉 SDK★ adid=%d uuid=%llu 第%u次",
             src, res, back, ok2 ? 1 : 0, adId, (unsigned long long)uuid, w);
}

/** ★v49：**结构发现** —— 在 `EventItemInt`（或任何容器对象）里找出 `Proto_CSRsp`。
 *
 * 为什么需要它：`OnGetADInfoRsp` 是被 `ProtocolManager.callback` **委托**调进来的
 * （真机栈证据：`Engine:FixedUpdate → ProtocolManager:update → ProtocolManager:sendEvent(EventItemInt)
 *  → DS_Advertising:OnGetADInfoRsp(Proto_CSRsp)`，见 `Log::infoPhone` 附带的 C# 调用栈），
 * 而委托那一跳**不经过我们的两个挂点**（v48 全无 AD48 日志，而同一帧里它调用的 `PlayADVideo`
 * 我们看得见 ⇒ 热更→热更的 pushframe 可见、委托跳不可见）。
 * `sendEvent` 本身是 `update()` 里的**热更→热更**调用 ⇒ **可见**，且它在 `OnGetADInfoRsp`
 * **之前**执行 ⇒ 在这里拿到 rsp 就能在 `get_result()` 被读之前把 `result` 写好。
 *
 * 布局未知怎么办（不猜字段名）：**按结构找** —— 在容器对象前 0x40 字节的 8 字节对齐字里找
 * 「klass 名字含 `Proto_CSRsp`」的活指针；两级（容器 → 其字段对象）就够，因为 `EventItemInt`
 * 是队列项、proto 直接挂在它（或其内部一个小结构）身上。**结果按类缓存**（含「没找到」），
 * 所以只有第一次遇到该类才付出几十次元数据查询的代价，之后是「1 次解引用 + 1 次类比较」。
 * 全部走 `LivePtr` / 可读性闸，失败一律返回 nullptr（= 什么都不做，广告照播）。 */
static void* ProtoCsRspInObjV49(void* o) {
    if (!LivePtr(o) || !Il2Cpp::IsReadableRange((uintptr_t)o, 0x48)) return nullptr;
    auto& api = Il2Cpp::GetApi();
    if (!api.object_class || !api.class_name) return nullptr;
    void* ok = api.object_class(o);
    if (!LivePtr(ok)) return nullptr;
    /* 缓存：{容器类, 命中偏移}（off==0 表示「这个类里没有」） */
    static const void* s_ck[8] = {};
    static size_t s_coff[8] = {};
    static std::atomic<uint32_t> s_cn{0};
    const uint32_t cn = s_cn.load(std::memory_order_relaxed);
    for (uint32_t i = 0; i < cn && i < 8; i++)
        if (s_ck[i] == ok) {
            if (!s_coff[i]) return nullptr;
            /* 命中缓存：**不再复核类名**（该偏移对该容器类是稳定的，且核心 `NoAdRspCoreV50`
             *  仍会拿类名自证一次）⇒ 每帧只付「1 次解引用 + 1 次 LivePtr」的代价。 */
            void* p = (void*)RawWord(o, s_coff[i]);
            return LivePtr(p) ? p : nullptr;
        }
    /* 一级：容器自身的 8 字节对齐字段（跳过 +0 = klass） */
    for (size_t off = 8; off < 0x40; off += 8) {
        void* p = (void*)RawWord(o, off);
        if (!LivePtr(p) || !Il2Cpp::IsReadableRange((uintptr_t)p, 0x10)) continue;
        void* pk = api.object_class(p);
        if (!LivePtr(pk)) continue;
        const char* pn = api.class_name(pk);
        if (pn && strstr(pn, "Proto_CSRsp")) {
            if (cn < 8) { s_ck[cn] = ok; s_coff[cn] = off; s_cn.store(cn + 1, std::memory_order_relaxed); }
            LOGI(TPL_LOG " AD50 结构发现：容器 cls=%s 的 +0x%zx 就是 Proto_CSRsp", api.class_name(ok),
                 off);
            return p;
        }
        /* 二级：该字段对象自己的前 0x20 字节里再找一层（只在没命中缓存时做一次） */
        if (!Il2Cpp::IsReadableRange((uintptr_t)p, 0x28)) continue;
        for (size_t o2 = 8; o2 < 0x20; o2 += 8) {
            void* q = (void*)RawWord(p, o2);
            if (!LivePtr(q) || !Il2Cpp::IsReadableRange((uintptr_t)q, 0x10)) continue;
            void* qk = api.object_class(q);
            if (!LivePtr(qk)) continue;
            const char* qn = api.class_name(qk);
            if (qn && strstr(qn, "Proto_CSRsp")) {
                LOGI(TPL_LOG
                     " AD50 结构发现：容器 cls=%s 的 +0x%zx → 其字段 +0x%zx 是 Proto_CSRsp（两级）",
                     api.class_name(ok), off, o2);
                /* 两级命中时**不缓存**（缓存语义是「容器字段直接就是 rsp」）——
                 * 只当次使用；代价是该类每次都要再搜一遍两级，但那只有几十次元数据查询，
                 * 且只在真的有广告响应时才发生（低频）。 */
                return q;
            }
        }
    }
    if (cn < 8) { s_ck[cn] = ok; s_coff[cn] = 0; s_cn.store(cn + 1, std::memory_order_relaxed); }
    static std::atomic<uint32_t> s_nf{0};
    if (s_nf.fetch_add(1, std::memory_order_relaxed) < 6)
        LOGW(TPL_LOG " AD50 结构发现：容器 cls=%s 里没找到 Proto_CSRsp → 该类以后不再搜",
             api.class_name(ok));
    return nullptr;
}

/** ★v48 入口①：`DS_Advertising::OnGetADInfoRsp(Proto_CSRsp)` 帧（若该帧可见时生效）。 */
static void ApplyNoAdRspV48(void* a1, uint8_t argBase) {
    if (!g_noAd.load(std::memory_order_relaxed)) return;
    if (!a1) return;
    if (!Il2Cpp::IsReadableRange((uintptr_t)a1 + (uintptr_t)argBase * 8u, 8)) return;
    void* rsp = (void*)RawWord(a1, (size_t)argBase * 8);  // 实例方法 ⇒ 第 1 个 IL 实参在 argBase+0
    NoAdRspCoreV50(rsp, "rsp");
}

/** ★v49 入口②：`ProtocolManager::sendEvent(EventItemInt)` 帧 —— 实测**可见**的那条路。
 *  实参可能是「对象指针」也可能是「按值内联的结构体」，两种都试：
 *   ① `RawWord(a1, argBase*8)` 若是活对象 → 直接当容器做结构发现；
 *   ② 否则把 `a1 + argBase*8` 当作内联结构体起点做结构发现。 */
static void ApplyNoAdRspFromEventV49(void* a1, uint8_t argBase) {
    if (!g_noAd.load(std::memory_order_relaxed)) return;
    if (!a1) return;
    const size_t so = (size_t)argBase * 8u;
    if (!Il2Cpp::IsReadableRange((uintptr_t)a1 + so, 8)) return;
    const uint32_t n = g_adV49Event.fetch_add(1, std::memory_order_relaxed) + 1;
    void* cand = (void*)RawWord(a1, so);
    void* rsp = LivePtr(cand) ? ProtoCsRspInObjV49(cand) : nullptr;
    if (!rsp) {
        /* 按值内联的结构体：直接扫那块内存（同一个函数复用，代价只在本类第一次出现时付） */
        rsp = ProtoCsRspInObjV49(reinterpret_cast<void*>((uintptr_t)a1 + so));
    }
    if (!rsp) {
        if (n <= 3) DBGLOG(TPL_LOG " AD50[sendEvent] 第%u次：本条消息里没有 start_watch_ad_rsp", n);
        return;
    }
    g_adV49Found.fetch_add(1, std::memory_order_relaxed);
    NoAdRspCoreV50(rsp, "sendEvent");
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

static void InterpFire(uint16_t kinds, uint8_t argBase, void* a0, void* a1, void* a2,
                       bool allowWrite) {
    if (!a1) return;
    if (!allowWrite) {
        InterpEvidence(kinds, a1, a2);  // 二级挂点（建帧桩）：只留证，不改行为
        return;
    }
    /* ★★v60：战斗三项功能**全部**交给 `Games/MxytzBattle.cpp`（独立文件、独立状态）。
     *  这里只做「位 → 帧」的转发，不含任何功能逻辑；帧语义见 MxytzBattle.h。 */
    if (kinds & K_HURT) MxytzBattle::OnFrame(MxytzBattle::FRAME_HURT, a1, argBase);
    if (kinds & K_DOATTACK) MxytzBattle::OnFrame(MxytzBattle::FRAME_DOATTACK, a1, argBase);
    if (kinds & K_PROJ) MxytzBattle::OnFrame(MxytzBattle::FRAME_PROJADD, a1, argBase);
    if (kinds & K_PROJEXEC) MxytzBattle::OnFrame(MxytzBattle::FRAME_PROJEXEC, a1, argBase);
    /* 免广告（#4）—— 逐字保留 v48~v52 已真机定案的三条落点 */
    if (kinds & K_ADRESULT) ForceTrueArg(a1, argBase, 0, "OnResult");
    if (kinds & K_ADREWARD) ForceTrueArg(a1, argBase, 1, "Dispatch");
    if (kinds & K_PLAYAD) ApplyNoAdV43(a1, argBase);
    if (kinds & K_GETADRSP) ApplyNoAdRspV48(a1, argBase);
    if (kinds & K_SENDEVENT) ApplyNoAdRspFromEventV49(a1, argBase);
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
    for (size_t i = 0; i < kKindN; i++)      // ★v50：标记为「已预登记」，补学不再重复开窗
        if (kKinds[i].kind == kind) __atomic_store_n(&g_kindPre[i], 1u, __ATOMIC_RELAXED);
    LOGI(TPL_LOG " ↑v43 预登记 %s kind=0x%04x base=%u mi=%p %s", tag, kind, ab, mi,
         isNew ? "新建" : "已存在");
}

/** ★v43：广告四目标预登记（可重试；四个都成功即锁闸）。
 *  由 `SyncHooks` 每拍调用 —— 锁闸后只剩「1 次原子读」，冷路径开销可忽略。
 *  为什么不用 `InstallAdHooks` 一次搞定：那里解析 mi 时类可能还没被加载（懒加载 AOT 镜像），
 *  一次失败就再也补不上（v26x 血证：`g_installedDmg` 无条件锁闸 ⇒ 一次失败永久失效）。 */
static void EnsureAdTargetsV43() {
    if (!g_noAd.load(std::memory_order_relaxed)) return;
    /* ★v48：目标从 4 个变 5 个（多出 `OnGetADInfoRsp`）⇒ 完成掩码 0x0F → 0x1F。
     *  这里**必须**同步改，否则 `mask==0x0F` 永远不会成立、会一直重试到 `g_adRegTryV43` 上限。 */
    static const uint32_t kAdAllMask = 0x3Fu;   /* ★v49：4→5→6 个目标（+OnGetADInfoRsp +sendEvent） */
    uint32_t mask = g_adRegMaskV43.load(std::memory_order_relaxed);
    if (mask == kAdAllMask) return;
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
        /* ★v48：**广告出画的判定帧**。预登记它是为了「不等它自己被调用一次」——
         *  它只在服务端下发 `start_watch_ad_rsp` 时才跑，而我们要在**第一次**点击看广告时
         *  就拦住（学习期能不能赶上完全取决于玩家点得快不快，不能赌）。 */
        {"DS_Advertising", "OnGetADInfoRsp", 1, K_GETADRSP, "GetAdRsp", 16u},
        /* ★v49：**实测可见的落点**（热更→热更 pushframe）—— 预登记它，避免赌学习期。 */
        {"ProtocolManager", "sendEvent", 1, K_SENDEVENT, "SendEvent", 32u},
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
    if (mask == kAdAllMask)
        LOGI(TPL_LOG " ↑v49 广告六目标预登记**全部完成**（A1 + A2 + result 改写：rsp/sendEvent 两条入口）");
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
    /* ★v60：本游戏的**唯一**挂载点 = HybridCLR 解释器入口（+ 建帧桩）。
     *  v60 起战斗三项功能改由 `MxyztBattle` 承接；Dobby 伤害挂点（原 `kDmgSlots`）整块删除 ——
     *  那些目标全是解释方法（methodPointer 指向解释器共享桩，物理上挂不上），
     *  交付版一直 `kDmgHooks=false`，留着只有「周期重试 + 失败日志」的成本。 */
    if (!g_interpHooked.load(std::memory_order_relaxed)) {
        InstallInterpHook();
        if (g_interpHooked.load(std::memory_order_relaxed))
            LOGI(TPL_LOG " interp hooks armed (四项功能主挂载点)");
        else
            LOGW(TPL_LOG " interp 入口挂载失败 —— 功能不可用");
    }
    if (!g_installedAd && g_noAd.load()) {
        InstallAdHooks();
        g_installedAd = true;
        LOGI(TPL_LOG " ad hooks installed");
    }
    EnsureAdTargetsV43();  // 免广告四目标预登记（可重试；锁闸后仅 1 次原子读）
    if (g_installedAd && !g_hooked.load()) {
        g_installMs.store(NowMs(), std::memory_order_relaxed);
        g_hooked.store(true);
        LOGI(TPL_LOG " hooks armed");
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
    /* 运行时就绪闸（v26t 血证）：`domain_get_assemblies` 能取到 image 才说明 il2cpp 完成了
     * domain/image 初始化。在此之前做全镜像类遍历会把 Boehm GC 的静态根扫描撞崩
     * （`GC_exclude_static_roots_inner` → `abort("Exclusion ranges overlap")` 嫌疑人）。 */
    if (!RuntimeReadyForWalk()) {
        static int s_wait = 0;
        if (s_wait < 12 && (s_wait == 0 || s_wait % 4 == 0))
            LOGI(TPL_LOG " il2cpp 运行时未就绪，暂缓（等 domain/assembly）wait=%d", s_wait);
        s_wait++;
        return false;
    }
    g_processStartMs.store(NowMs(), std::memory_order_relaxed);
    /* v60：解释器入口**懒装**（等功能打开再装），启动期一个字节都不改写游戏代码页。
     *  历史：v26x 的 `kStartupArm` 在启动窗口给正在执行的入口打 inline patch，机型相关闪退
     *  嫌疑人（v47 已定案关闭），其唯一收益（抓 `set_damage*`）随 v34 那条路一起废掉了。 */
    DBGLOG(TPL_LOG " step1 api ok（v60：不装任何启动期 hook）");
    SyncHooks();
    g_libReady.store(true);
    LOGI(TPL_LOG " lib ready (il2cpp+HybridCLR, lazy interp hook)");
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
    /* 战斗三功能的状态由 MxytzBattle 自己持有；关 #3 时它立刻还原投射物字段（不留脏数据）。 */
    MxytzBattle::SetSwitches(g_killMob.load(), g_invincible.load(), g_skillDmg.load());
    MxytzBattle::OnFeatureChanged(featNum, enabled);
    if (featNum == MXYTZ_FEAT_NO_AD) {
        g_pendAdReady.store(0, std::memory_order_relaxed);
        g_adV43FuseUntilMs.store(0, std::memory_order_relaxed);
        AdForceGateReset();
        g_adV43Done.store(0, std::memory_order_relaxed);  // 配额按激活期重新上膛
        LOGI(TPL_LOG " AD43 待办/熔断窗口/配额已重置（免广告=%d）", enabled ? 1 : 0);
    }
    /* 功能一开就重开学习期：目标方法是按需调用的（尤其广告链路只在玩家点广告时才出现）。 */
    if (enabled) {
        for (size_t i = 0; i < kKindN; i++)
            if (__atomic_load_n(&g_kindDone[i], __ATOMIC_RELAXED) == 2)
                __atomic_store_n(&g_kindDone[i], 0u, __ATOMIC_RELAXED);
        g_interpCalls.store(0, std::memory_order_relaxed);
        g_learnMs.store(0, std::memory_order_relaxed);
        g_learnActive.store(1, std::memory_order_relaxed);
    }
}

void Mxytz_OnValueChanged(int featNum, int value) {
    (void)featNum;
    (void)value;
}

void Mxytz_Tick(const bool* features, const int* /*values*/) {
    if (!features) return;
    g_killMob.store(features[MXYTZ_FEAT_KILL_MOB]);
    g_invincible.store(features[MXYTZ_FEAT_INVINCIBLE]);
    g_skillDmg.store(features[MXYTZ_FEAT_SKILL_DMG]);
    g_noAd.store(features[MXYTZ_FEAT_NO_AD]);
    if (!g_libReady.load() && isLibraryLoaded(MXYTZ_TARGET_LIB)) Mxytz_TryInit(nullptr);

    /* 战斗三功能：同步开关 + 400ms 心跳（刷新玩家身份 / 维持免疫位）。 */
    MxytzBattle::SetSwitches(g_killMob.load(), g_invincible.load(), g_skillDmg.load());
    if (g_libReady.load()) MxytzBattle::OnTick();

    SyncHooks();

    /* 学习期补学（v47 的失效修复，保留）：每 30s 检查「已打开的功能里是否还有目标没登记」，
     * 有就重开 60s 窗口并把缺口写进日志。零成本时的开销 = 几个原子读。 */
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
                if (k & K_HURT) wanted = wantKill;
                if (k & K_DOATTACK) wanted = wantInv;
                if (k & (K_PROJ | K_PROJEXEC)) wanted = wantSkill;
                if (k & (K_PLAYAD | K_ADRESULT | K_ADREWARD | K_CANCLAIM | K_GETADRSP | K_SENDEVENT))
                    wanted = wantAd;
                if (!wanted) continue;  // K_DAMAGE / K_LOCALDMG 只取证，缺了不影响功能
                if (__atomic_load_n(&g_kindPre[i], __ATOMIC_RELAXED) == 1) continue;
                if (__atomic_load_n(&g_kindDone[i], __ATOMIC_RELAXED) == 1) continue;
                need++;
                const int r = snprintf(miss + w, sizeof(miss) - w, "%s(%s) ", kKinds[i].tag,
                                       kKinds[i].cls);
                if (r > 0 && (size_t)r < sizeof(miss) - w) w += (size_t)r;
            }
            if (need) {
                g_interpCalls.store(0, std::memory_order_relaxed);
                g_learnMs.store(0, std::memory_order_relaxed);
                g_learnActive.store(1, std::memory_order_relaxed);
                LOGI(TPL_LOG " interp learn ★补学窗口重开★（已开功能仍有 %u 个目标未登记：%s）",
                     (unsigned)need, miss);
            }
        }
    }

    /* 学习期到期关闸 + 状态行 */
    if (g_learnActive.load(std::memory_order_relaxed)) {
        const uint64_t st = g_learnMs.load(std::memory_order_relaxed);
        const uint64_t nowLg = NowMs();
        if (st == 0) {
            g_learnMs.store(nowLg, std::memory_order_relaxed);
            LOGI(TPL_LOG " interp learn 窗口开启（%u 秒，%u 个目标）",
                 (unsigned)(kLearnWindowMs / 1000u), (unsigned)kKindN);
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
        }
    }

    /* 30s 一条汇总（装配自检：一眼看出「功能到底有没有在干活」） */
    {
        static uint64_t s_statMs = 0;
        const uint64_t nowSt = NowMs();
        if (!s_statMs) s_statMs = nowSt;
        if (nowSt - s_statMs > 30000ull) {
            s_statMs = nowSt;
            char battle[320] = {0};
            MxytzBattle::StatLine(battle, sizeof(battle));
            LOGI(TPL_LOG " STATS 开关 kill=%d inv=%d skill=%d ad=%d | interp 慢路径=%u | %s",
                 g_killMob.load() ? 1 : 0, g_invincible.load() ? 1 : 0,
                 g_skillDmg.load() ? 1 : 0, g_noAd.load() ? 1 : 0,
                 g_interpCalls.load(std::memory_order_relaxed), battle);
        }
    }
}

void Mxytz_ForceClear() {
    g_killMob.store(false);
    g_invincible.store(false);
    g_skillDmg.store(false);
    g_noAd.store(false);
    g_pendAdReady.store(0, std::memory_order_relaxed);
    g_adV43FuseUntilMs.store(0, std::memory_order_relaxed);
    AdForceGateReset();
    MxytzBattle::ForceClear();  // 还原一切写入（投射物 count / 免疫位）
    LOGI(TPL_LOG " ForceClear：开关清零 + 战斗写入已还原");
}
