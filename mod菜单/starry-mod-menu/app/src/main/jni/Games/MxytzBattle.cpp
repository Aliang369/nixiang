/* ================================================================
 *  MxytzBattle.cpp — 冒险与征途 (com.mxyzt.gzmy 1.5.9) 战斗三功能 · 全新实现
 *
 *  为什么另起一个文件重写（而不是继续改旧代码）：
 *    旧 MxytzMod.cpp 里那套战斗实现是 v7→v52 五十多轮堆出来的：演员总表扫描、属性字典
 *    自发现、硬偏移 + 修正量、还原槽表、预算看门狗、CheckDead invoke、堆快照比对……
 *    其中**真正在交付版生效的只有三条**（其余全是关闭的死代码与互相干扰的兜底）。
 *    本轮按用户要求「所有功能单独重新写」，把三条生效机制抽出来重写，判据/写入全部按
 *    元数据名解析，不再有硬偏移、不再有扫描、不再有全局演员表。
 *
 *  三条生效机制（每条都是真机实测过的，证据见各节注释）：
 *    #1 秒杀小怪       = `CBattleActorUtil::Hurt` 帧里把 `para.m_llValue`（BigInt）× 100
 *                        —— 斌哥包唯一机制（他日志 `改前 m=%.4f e=%d → 改后 …` 就是这两个字段）
 *    #2 无敌           = ①`StarsCollider::DoAttack` 帧里把「打玩家的那一击」accepter 槽置 0
 *                           （IL_0044 官方作废分支：accepter 空 ⇒ 原函数直接 return）
 *                        ②玩家 `ActionState` 的免疫位（`immuneDamage` 等，按元数据名解析）
 *    #3 全模式技能增伤 = `ActionCommand_AddProjectile::Execute` 帧里把 `LAddProjectile._count`
 *                        放大（**唯一读者**就在这个方法的 IL_011B `callvirt get_count()`），
 *                        只对玩家侧施放生效，扇出默认 3、每秒发数有预算、关闭即还原。
 *
 *  纪律（每一条都是历史事故换来的）：
 *    · 热路径零 I/O：无 maps 查询、无日志（首次 N 次除外）、无字符串比较、无容器遍历。
 *    · 身份判据只读游戏自己的量：`_player` / `ulRoleId` / `_hitter`，且**解不出就不写**。
 *    · 写入前一律做「值合理性闸」（BigInt mantissa/指数、count 范围、bool 只写 0/1）。
 *    · 写入一律可逆：记录原值，功能关闭 / ForceClear 立刻还原（用户 m03027「全关后伤害还是高」）。
 * ================================================================ */

#include "MxytzBattle.h"

#include <atomic>
#include <cmath>
#include <cstdio>
#include <cstring>
#include <ctime>

#include "Includes/Logger.h"
#include "Includes/SecStrings.hpp"  // sec_lib_il2cpp（Il2CppApi.h 的默认参数）
#include "Includes/StarPtr.h"
#include "Unity/Il2CppApi.h"

#define BT_LOG "[mxyzt][battle]"

/* 诊断闸：0 = 交付档（只留「装配/还原/STATS」三类行），1 = 全量逐笔诊断。 */
#ifndef MXYTZ_BATTLE_VERBOSE
#define MXYTZ_BATTLE_VERBOSE 0
#endif
#if MXYTZ_BATTLE_VERBOSE
#define BLOG(...) LOGI(__VA_ARGS__)
#else
#define BLOG(...) ((void)0)
#endif

namespace MxytzBattle {
namespace {

using Il2Cpp::GetApi;

/* ============================ §1 基础读写 / 指针判据 ============================ */

static inline uint64_t NowMs() {
    timespec ts{};
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return static_cast<uint64_t>(ts.tv_sec) * 1000ull + static_cast<uint64_t>(ts.tv_nsec) / 1000000ull;
}

/** 「像样指针」：只做顶字节清零 + 用户态区间判定，**零 I/O**（Includes/StarPtr.h 的规约）。 */
static inline bool Plaus(const void* p) {
    return p && starptr::InUserVa(reinterpret_cast<uintptr_t>(p));
}
static inline uintptr_t Un(const void* p) { return starptr::Untag(reinterpret_cast<uintptr_t>(p)); }

/** 从槽区读一个字（槽区是 HybridCLR 入口的实参值内联区，调用前已确认 a1 合法）。 */
static inline uintptr_t RawWord(const void* base, size_t off) {
    return *reinterpret_cast<const uintptr_t*>(reinterpret_cast<uintptr_t>(base) + off);
}
/** 读对象里的引用字段；不像指针就返回 nullptr（**不解引用**，所以不会崩）。 */
static inline void* RefAt(uintptr_t obj, size_t off) {
    if (!obj || !off) return nullptr;
    const uintptr_t v = *reinterpret_cast<const uintptr_t*>(obj + off);
    return starptr::InUserVa(v) ? reinterpret_cast<void*>(starptr::Untag(v)) : nullptr;
}
static inline int32_t I32At(uintptr_t p, size_t off) { return *reinterpret_cast<const int32_t*>(p + off); }
static inline uint64_t U64At(uintptr_t p, size_t off) { return *reinterpret_cast<const uint64_t*>(p + off); }
static inline uint8_t U8At(uintptr_t p, size_t off) { return *reinterpret_cast<const uint8_t*>(p + off); }
static inline void SetI32(uintptr_t p, size_t off, int32_t v) { memcpy(reinterpret_cast<void*>(p + off), &v, 4); }
static inline void SetU8(uintptr_t p, size_t off, uint8_t v) { memcpy(reinterpret_cast<void*>(p + off), &v, 1); }
static inline void SetU64(uintptr_t p, size_t off, uint64_t v) { memcpy(reinterpret_cast<void*>(p + off), &v, 8); }

/* ============================ §2 类 / 字段解析（按元数据名，带缓存） ============================ */

struct ClsSlot {
    const char* name;
    void* klass;
};
static ClsSlot g_cls[16];
static std::atomic<uint32_t> g_clsN{0};

/** 按短类名找 klass（`FindClassByShortName` 是线性扫全镜像，慢；命中后永久缓存）。 */
static void* KC(const char* name) {
    if (!name) return nullptr;
    const uint32_t n = g_clsN.load(std::memory_order_relaxed);
    for (uint32_t i = 0; i < n && i < 16; i++)
        if (g_cls[i].name == name && g_cls[i].klass) return g_cls[i].klass;
    auto& api = GetApi();
    if (!api.ready) return nullptr;
    void* k = api.FindClassByShortName(name);
    if (!k) {
        /* 兜底：按已知命名空间逐个 il2cpp_class_from_name（比全镜像扫描便宜且更准） */
        static const char* kNss[] = {"ActionGameLibrary", "ActionData", "GameConfigData", ""};
        for (size_t i = 0; i < sizeof(kNss) / sizeof(kNss[0]) && !k; i++) {
            size_t an = 0;
            void** asms = api.Assemblies(&an);
            if (!asms) break;
            for (size_t j = 0; j < an && !k; j++) {
                void* img = api.ImageOf(asms[j]);
                if (img) k = api.ClassFromName(img, kNss[i], name);
            }
        }
    }
    if (k && n < 16) {
        g_cls[n].name = name;
        g_cls[n].klass = k;
        g_clsN.store(n + 1, std::memory_order_relaxed);
        LOGI(BT_LOG " 类解析 %s = %p", name, k);
    }
    return k;
}

struct FldSlot {
    const void* klass;
    const char* name;
    size_t off;  // 0 = 解析不到（负缓存，避免每次重试全类遍历）
};
static FldSlot g_fld[48];
static std::atomic<uint32_t> g_fldN{0};

/** 字段偏移（按名字，运行时解析；HybridCLR 热更类也能拿到）。0 = 不存在。 */
static size_t Off(void* klass, const char* name) {
    if (!klass || !name) return 0;
    const void* kk = reinterpret_cast<const void*>(Un(klass));
    /* ★2026-10-26 血证（真机 tombstone：pc libil2cpp+0x205aa5c ← 本文件的 FieldOffset 调用链，
     *  fault addr 0xffffff00000166）：把**已回收对象上读到的垃圾 klass** 递给
     *  `il2cpp_class_get_field_from_name` ⇒ il2cpp 内部 deref 野指针 ⇒ SIGSEGV。
     *  本函数是所有按名解析的唯一入口，故这里先做**零 il2cpp API** 的自证：
     *  ① klass 落在用户态；② klass->name（il2cpp Klass 布局 +0x10）也是用户态指针。
     *  两条不过就当作「解析不到」返回 0（fail-safe：功能少写一次，绝不崩）。 */
    if (!starptr::InUserVa(reinterpret_cast<uintptr_t>(kk))) return 0;
    const uintptr_t kname = *reinterpret_cast<const uintptr_t*>(reinterpret_cast<uintptr_t>(kk) + 0x10);
    if (!starptr::InUserVa(kname)) return 0;
    const uint32_t n = g_fldN.load(std::memory_order_relaxed);
    for (uint32_t i = 0; i < n && i < 48; i++)
        if (g_fld[i].klass == kk && g_fld[i].name == name) return g_fld[i].off;
    auto& api = GetApi();
    size_t o = api.FieldOffset(const_cast<void*>(kk), name, 0);
    if (o > 0x1000) o = 0;
    if (n < 48) {
        g_fld[n].klass = kk;
        g_fld[n].name = name;
        g_fld[n].off = o;
        g_fldN.store(n + 1, std::memory_order_relaxed);
        BLOG(BT_LOG " 字段 %s +0x%zx", name, o);
    }
    return o;
}

/** 依次尝试多个候选名（游戏里「属性 ⇒ backing 字段」的命名不统一）。 */
static size_t OffAny(void* klass, const char* const* names, size_t n) {
    for (size_t i = 0; i < n; i++) {
        const size_t o = Off(klass, names[i]);
        if (o) return o;
    }
    return 0;
}

/* ===== 沿父类链的字段解析（2026-11-16 v3，真机 ID ok=0 的根因修复）=====
 * 实测根因（lead 的 verify 反编译结论，逐字字段名）：
 *   il2cpp_class_get_field_from_name 只查**本类**字段，**不沿父类链**；而本作用到的关键字段
 *   多数声明在基类：
 *     _hitter        → InteractiveGameObject.cs:16（基类）
 *     _state         → ActionGameObject.cs:20（protected，基类；公开访问器是属性 state）
 *     _characterData → Character→SimpleCharacter→InteractiveGameObject→ActionGameObject 链上
 *     _player        → GameObjectManager.cs:38（本类，下划线名；属性名才是 player）
 *   v2 之前 Off 只查本类 ⇒ hitter/state/cdata 全解析不到 ⇒ 身份链恒空（真机一整局 ID ok=0）。
 * 逐级向上最多 16 级防环；命中回填 depth（0 = 本类，1 = 父类 …）。失败返回 nullptr（fail-closed）。 */
static void* FindFieldDeep(void* klass, const char* name, int* outDepth) {
    if (outDepth) *outDepth = -1;
    if (!klass || !name) return nullptr;
    auto& api = GetApi();
    if (!api.class_get_field) return nullptr;
    void* k = klass;
    for (int d = 0; d < 16 && k; d++) {
        if (!starptr::InUserVa(reinterpret_cast<uintptr_t>(k))) return nullptr;
        void* f = api.class_get_field(k, name);
        if (f) {
            if (outDepth) *outDepth = d;
            return f;
        }
        if (!api.class_parent) return nullptr;
        k = api.class_parent(k);
    }
    return nullptr;
}

struct DeepSlot {
    const void* klass;
    const char* name;
    size_t off;
    int depth;
};
static DeepSlot g_deep[32];
static std::atomic<uint32_t> g_deepN{0};

/** FindFieldDeep 的缓存版（缓存键 = klass + 名字；与 Off 同款「klass 自证」防野指针）。 */
static size_t OffDeep(void* klass, const char* name, int* outDepth) {
    if (outDepth) *outDepth = -1;
    if (!klass || !name) return 0;
    const void* kk = reinterpret_cast<const void*>(Un(klass));
    if (!starptr::InUserVa(reinterpret_cast<uintptr_t>(kk))) return 0;
    const uintptr_t kname = *reinterpret_cast<const uintptr_t*>(reinterpret_cast<uintptr_t>(kk) + 0x10);
    if (!starptr::InUserVa(kname)) return 0;
    const uint32_t n = g_deepN.load(std::memory_order_relaxed);
    for (uint32_t i = 0; i < n && i < 32; i++)
        if (g_deep[i].klass == kk && g_deep[i].name == name) {
            if (outDepth) *outDepth = g_deep[i].depth;
            return g_deep[i].off;
        }
    int d = -1;
    void* f = FindFieldDeep(const_cast<void*>(kk), name, &d);
    size_t o = 0;
    if (f) {
        auto& api = GetApi();
        if (api.field_offset) {
            o = api.field_offset(f);
            if (o > 0x1000) o = 0;
        }
    }
    if (n < 32) {
        g_deep[n].klass = kk;
        g_deep[n].name = name;
        g_deep[n].off = o;
        g_deep[n].depth = d;
        g_deepN.store(n + 1, std::memory_order_relaxed);
        if (o) BLOG(BT_LOG " 字段(深) %s +0x%zx @%d", name, o, d);
    }
    if (outDepth) *outDepth = d;
    return o;
}

/** 多候选名 + 沿父类链（属性 ⇒ backing 字段命名不统一时的兜底）。 */
static size_t OffDAny(void* klass, const char* const* names, size_t n, int* outDepth) {
    for (size_t i = 0; i < n; i++) {
        const size_t o = OffDeep(klass, names[i], outDepth);
        if (o) return o;
    }
    return 0;
}

/** 静态字段（沿父类链）取值：field_static_get(FieldInfo, void* out)。 */
static void* StaticFieldDeep(void* klass, const char* name, int* outDepth) {
    int d = -1;
    void* f = FindFieldDeep(klass, name, &d);
    if (outDepth) *outDepth = d;
    if (!f) return nullptr;
    auto& api = GetApi();
    if (!api.field_static_get) return nullptr;
    void* v = nullptr;
    api.field_static_get(f, &v);
    return v;
}

static inline void* KOf(const void* obj) {
    auto& api = GetApi();
    if (!obj || !api.object_class) return nullptr;
    return api.object_class(const_cast<void*>(obj));
}

/* ============================ §3 玩家身份（每拍刷新，不用演员表） ============================ */

static std::atomic<uintptr_t> g_player{0};        // GameObjectManager._player
static std::atomic<uintptr_t> g_playerHitter{0};  // 玩家 InteractiveGameObject._hitter
static std::atomic<uint64_t> g_playerRole{0};     // 玩家 CharacterData.ulRoleId

static const char* const kCdataNames[] = {"_characterData", "characterData", "_cdata", "cdata"};

/** 任意 actor 的 `ulRoleId`（元数据 `ActionGameObjectData::ulRoleId`；小怪/首领恒 0）。 */
static uint64_t RoleOf(void* obj) {
    if (!Plaus(obj)) return 0;
    void* k = KOf(obj);
    if (!k) return 0;
    const size_t cdOff = OffDAny(k, kCdataNames, sizeof(kCdataNames) / sizeof(kCdataNames[0]), nullptr);
    if (!cdOff) return 0;
    void* cd = RefAt(Un(obj), cdOff);
    if (!cd) return 0;
    void* ck = KOf(cd);
    if (!ck) return 0;
    size_t ro = Off(ck, "ulRoleId");
    if (!ro) ro = Off(ck, "roleId");
    if (!ro) return 0;
    return U64At(Un(cd), ro);
}

/** 任意 actor 的 `characterType`（`CharacterType`：0 PLAYER / 1 APC / 2 MONSTER / 3 BOSS）。 */
static int32_t CtypeOf(void* obj) {
    if (!Plaus(obj)) return -1;
    void* k = KOf(obj);
    if (!k) return -1;
    const size_t cdOff = OffDAny(k, kCdataNames, sizeof(kCdataNames) / sizeof(kCdataNames[0]), nullptr);
    if (!cdOff) return -1;
    void* cd = RefAt(Un(obj), cdOff);
    if (!cd) return -1;
    void* ck = KOf(cd);
    if (!ck) return -1;
    const size_t co = Off(ck, "characterType");
    if (!co) return -1;
    return I32At(Un(cd), co);
}

/** 任意 actor 的 `_hitter`（`StarsCollider::DoAttack` 的 accepter 就是它）。 */
static uintptr_t HitterOf(void* obj) {
    if (!Plaus(obj)) return 0;
    void* k = KOf(obj);
    if (!k) return 0;
    const size_t ho = OffDeep(k, "_hitter", nullptr);
    if (!ho) return 0;
    return Un(RefAt(Un(obj), ho));
}

/** 任意 actor 的阵营（`Hitter._camp`，0 = 玩家阵营、1 = 敌方）。读不到返回 -1。 */
static int32_t CampOf(void* obj) {
    const uintptr_t h = HitterOf(obj);
    if (!h) return -1;
    void* hk = KOf(reinterpret_cast<void*>(h));
    if (!hk) return -1;
    const size_t co = Off(hk, "_camp");
    if (!co) return -1;
    return I32At(h, co);
}

/* v3：管理器的**静态字段名**两个变体都试（属性 gameObjectManager 的 backing 可能是
 * _gameObjectManager）；_player 走沿父类链解析。命中结果缓存，命中名回显进 IDFIX 日志。 */
static const char* g_mgrFieldName = nullptr;
static int g_mgrFieldDepth = -1;
static size_t g_plOff = 0;
static int g_plDepth = -1;
static std::atomic<bool> g_idFixLogged{false};

/** 一次性解析快照（每进程一条，不在热路径）。 */
static void LogIdFixOnce(void* pl) {
    if (g_idFixLogged.exchange(true, std::memory_order_relaxed)) return;
    void* pk = pl ? KOf(pl) : nullptr;
    if (!pk) pk = KC("Character");
    int dH = -1, dS = -1, dC = -1;
    const size_t ho = pk ? OffDeep(pk, "_hitter", &dH) : 0;
    const size_t so = pk ? OffDeep(pk, "_state", &dS) : 0;
    const size_t co = pk ? OffDAny(pk, kCdataNames, sizeof(kCdataNames) / sizeof(kCdataNames[0]), &dC) : 0;
    LOGI(BT_LOG " IDFIX managerField=%s(@%d) playerOff=0x%zx(@%d) hitterOff=0x%zx(@%d) "
                "stateOff=0x%zx(@%d) cdataOff=0x%zx(@%d)",
         g_mgrFieldName ? g_mgrFieldName : "?", g_mgrFieldDepth, g_plOff, g_plDepth, ho, dH, so, dS,
         co, dC);
}

static void RefreshPlayer() {
    auto& api = GetApi();
    if (!api.ready) return;
    void* bec = KC("BattleElementCenter");
    if (!bec) return;
    /* 静态字段名两变体：命中一次后固定用它（两个都记日志，避免下次再猜） */
    void* gm = nullptr;
    if (g_mgrFieldName) {
        int d = -1;
        gm = StaticFieldDeep(bec, g_mgrFieldName, &d);
    } else {
        static const char* const kMgrNames[] = {"gameObjectManager", "_gameObjectManager"};
        for (size_t i = 0; i < 2 && !gm; i++) {
            int d = -1;
            void* v = StaticFieldDeep(bec, kMgrNames[i], &d);
            if (Plaus(v)) {
                gm = v;
                g_mgrFieldName = kMgrNames[i];
                g_mgrFieldDepth = d;
                LOGI(BT_LOG " 管理器字段命中 %s(@%d) = %p", kMgrNames[i], d, v);
            } else {
                BLOG(BT_LOG " 管理器字段 %s 解析不到(@%d)", kMgrNames[i], d);
            }
        }
    }
    if (!Plaus(gm)) {
        g_player.store(0, std::memory_order_relaxed);
        LogIdFixOnce(nullptr);
        return;
    }
    void* gk = KOf(gm);
    if (!gk) return;
    if (!g_plOff) {
        g_plOff = OffDeep(gk, "_player", &g_plDepth);
        if (!g_plOff) {
            LogIdFixOnce(nullptr);
            return;
        }
    }
    void* pl = RefAt(Un(gm), g_plOff);
    /* 解释器可能把槽里的引用打 tag（starptr::Untag 见 RefAt），这里再兜一次 */
    if (!Plaus(pl)) pl = reinterpret_cast<void*>(Un(pl));
    if (!Plaus(pl)) {
        g_player.store(0, std::memory_order_relaxed);
        LogIdFixOnce(nullptr);
        return;
    }
    const uintptr_t plU = Un(pl);
    const uintptr_t oldPl = g_player.exchange(plU, std::memory_order_relaxed);
    if (oldPl != plU) LOGI(BT_LOG " 玩家对象 _player=%p（刷新）", reinterpret_cast<void*>(plU));

    const uintptr_t h = HitterOf(pl);
    if (h) g_playerHitter.store(h, std::memory_order_relaxed);
    const uint64_t r = RoleOf(pl);
    if (r) {
        const uint64_t oldR = g_playerRole.exchange(r, std::memory_order_relaxed);
        if (oldR != r) LOGI(BT_LOG " 玩家 roleId=%llu（刷新）", static_cast<unsigned long long>(r));
    }
    LogIdFixOnce(pl);
}

/** 这个对象是不是「玩家侧」（本体 / 同 roleId 的分身与战斗 Character / 同 `_hitter`）。
 *  三条任一条命中即算；**全部读不到 = false**（fail-closed，宁可不生效也不误伤）。 */
static bool IsPlayerSide(void* obj) {
    if (!Plaus(obj)) return false;
    const uintptr_t u = Un(obj);
    if (u == g_player.load(std::memory_order_relaxed)) return true;
    const uint64_t pl = g_playerRole.load(std::memory_order_relaxed);
    const uint64_t r = RoleOf(obj);
    if (pl && r && r == pl) return true;
    const uintptr_t ph = g_playerHitter.load(std::memory_order_relaxed);
    const uintptr_t h = HitterOf(obj);
    if (ph && h && h == ph) return true;
    return false;
}

/* ============================ §4 功能 1：秒杀小怪 ============================ */

/* 斌哥包唯一机制（`10_docs/冒险与征途/斌哥包抓点_20261002.md` L241 原文）：
 *   `[冒险-打怪#%d] ctype=%d 合法=%d | 改前 m=%.4f e=%d → 改后 m=%.4f e=%d`
 * 他改的是 `CtagHurtHPPara.m_llValue`（`BigInt{mantissa double, exponent int}`），
 * 而该值经 `CHitNotifier::AddOneHitHurt(para.GetValue())` 写进命中通知包 ⇒ 飘字、落地血、
 * 死亡判定三者同源（`_钩子目标与语义.md` §2.1 的 IL 证据链）⇒ 掉落/结算正常。 */
static constexpr double kKillMul = 100.0;      // ×100：anti-ban 安全区（≥1e6 属高危）
static constexpr double kKillMantMax = 1.0e9;  // 归一化上限（超过就进位到指数）

static std::atomic<uint32_t> g_statKill{0};
static std::atomic<uint32_t> g_statKillSkip{0};
static std::atomic<uint32_t> g_statKillBoss{0};
static std::atomic<uint32_t> g_statHurtSeen{0};

/** `CtagHurtHPPara` 的布局（按元数据名解析一次，之后纯缓存）。 */
struct ParaLayout {
    size_t offTarget = 0;
    size_t offAttacker = 0;
    size_t offValue = 0;  // BigInt（值 = mantissa × 10^exponent）
    bool ready = false;
    std::atomic<uint32_t> fails{0};
};
static ParaLayout g_para;
static std::atomic<int> g_paraDepth{-1};  // -1 未定案 / 1 = 槽里就是 para / 2 = 槽里是「指向 para 的变量」

static bool ParaLooksLike(void* p) {
    auto& api = GetApi();
    if (!Plaus(p) || !api.object_class || !api.class_name) return false;
    void* k = api.object_class(p);
    if (!Plaus(k)) return false;
    const char* n = api.class_name(k);
    return n && strstr(n, "HurtHPPara") != nullptr;
}

/** 解析 Hurt 帧的 para 指针。深度（内联 / 指针数组两种约定）**自证后才缓存**。 */
static void* ParaOf(void* refSlot) {
    if (!Plaus(refSlot)) return nullptr;
    const uintptr_t w = *reinterpret_cast<const uintptr_t*>(refSlot);
    void* d1 = starptr::InUserVa(w) ? reinterpret_cast<void*>(starptr::Untag(w)) : nullptr;
    const int depth = g_paraDepth.load(std::memory_order_relaxed);
    if (depth == 1) return ParaLooksLike(d1) ? d1 : nullptr;
    if (depth == 2) {
        if (!Plaus(d1)) return nullptr;
        const uintptr_t w2 = *reinterpret_cast<const uintptr_t*>(Un(d1));
        void* d2 = starptr::InUserVa(w2) ? reinterpret_cast<void*>(starptr::Untag(w2)) : nullptr;
        return ParaLooksLike(d2) ? d2 : nullptr;
    }
    if (ParaLooksLike(d1)) {
        g_paraDepth.store(1, std::memory_order_relaxed);
        LOGI(BT_LOG " HURT para 约定=深度1（槽里直接是对象）");
        return d1;
    }
    if (Plaus(d1)) {
        const uintptr_t w2 = *reinterpret_cast<const uintptr_t*>(Un(d1));
        void* d2 = starptr::InUserVa(w2) ? reinterpret_cast<void*>(starptr::Untag(w2)) : nullptr;
        if (ParaLooksLike(d2)) {
            g_paraDepth.store(2, std::memory_order_relaxed);
            LOGI(BT_LOG " HURT para 约定=深度2（槽里是指向对象的变量）");
            return d2;
        }
    }
    if (g_para.fails.fetch_add(1, std::memory_order_relaxed) < 8)
        LOGI(BT_LOG " HURT para 未识别 refSlot=%p w=%p（本轮不写）", refSlot,
             reinterpret_cast<void*>(w));
    return nullptr;
}

static bool ParaLayoutReady(void* para) {
    if (g_para.ready) return true;
    auto& api = GetApi();
    void* k = api.object_class(para);
    if (!Plaus(k)) return false;
    const size_t t = api.FieldOffset(k, "m_pTarget", 0);
    const size_t a = api.FieldOffset(k, "m_pAttacker", 0);
    const size_t v = api.FieldOffset(k, "m_llValue", 0);
    if (!t || t > 0x400 || !v || v > 0x400) return false;
    g_para.offTarget = t;
    g_para.offAttacker = (a && a <= 0x400) ? a : 0;
    g_para.offValue = v;
    g_para.ready = true;
    LOGI(BT_LOG " para 布局 m_pTarget=+0x%zx m_pAttacker=+0x%zx m_llValue=+0x%zx", t, g_para.offAttacker, v);
    return true;
}

/* ================= BigInt 字段偏移：按名自证 + 回退（2026-11-16 B站 2.0.1 轮）=================
 * 证据：10_docs/冒险与征途/bili_2.0.1_热更程序集对照.md
 *   · 2.0.1 热更 dll 的 BigInt 字段名带 m_ 前缀（m_mantissa double / m_exponent int32）；
 *     1.5.9 时代是 mantissa / exponent。
 *   · 新版热更 dll 没有 FieldLayout（字段偏移全靠推断）⇒「+0 double / +8 int32」是布局假设。
 *   做法：运行期拿 BigInt klass 后按名取两次（先 m_ 前缀，再旧名），两值都拿到且
 *         mantOff<0x20 && expOff<0x20 && expOff>=mantOff+8 才采用，否则回退 +0/+8。
 *   自证口径：日志一行 "BigInt off=mant+0x.. exp+0x.. (src=field|fallback)" 即可判来源。 */
struct BigIntLayout {
    size_t mantOff;
    size_t expOff;
    bool tried;
    const char* src;
    int vt;          /* 1 = 已做「值类型成员字段」校正（-0x10），0 = 未校正 */
};
static BigIntLayout g_bi{0, 8, false, "default", 0};

static void EnsureBigIntLayout() {
    if (g_bi.tried) return;
    g_bi.tried = true;
    auto& api = GetApi();
    void* kb = KC("BigInt");   /* 无命名空间（_钩子目标与语义.md 类型表：BigInt 0x020000CB，struct） */
    if (!Plaus(kb) || !api.class_get_field || !api.field_offset) {
        LOGI(BT_LOG " BigInt off=mant+0x%zx exp+0x%zx (src=fallback:no-class)", g_bi.mantOff,
             g_bi.expOff);
        return;
    }
    /* 用 class_get_field + field_offset 而非 Api::FieldOffset：值类型 struct 的 mantissa
     * 可能正好在 +0，而 FieldOffset 的 0 既表示「不存在」也表示「偏移 0」，必须区分。 */
    auto foff = [&](const char* n) -> long {
        void* f = api.class_get_field(kb, n);
        if (!f) return -1;
        return static_cast<long>(api.field_offset(f));
    };
    static const char* const kMant[] = {"m_mantissa", "mantissa"};
    static const char* const kExp[] = {"m_exponent", "exponent"};
    for (size_t i = 0; i < 2; i++) {
        const long m = foff(kMant[i]);
        const long e = foff(kExp[i]);
        if (m < 0 || e < 0) continue;
        if (m < 0x20 && e < 0x20 && e >= m + 8) {
            g_bi.mantOff = static_cast<size_t>(m);
            g_bi.expOff = static_cast<size_t>(e);
            g_bi.src = "field";
            break;
        }
    }
    /* ★v2（2026-11-16 真机实测根因）：il2cpp_field_get_offset 对**值类型成员字段**返回的偏移
     *   含对象头 0x10 ⇒ 实测日志出现 mant+0x10 / exp+0x18。而 BigInt 是内联在 CtagHurtHPPara
     *   里的 struct，真实布局就是 mant+0 / exp+8（旧版硬编码 +0/+8 之所以有效的原因）。
     *   不校正的后果：写到紧随其后的 m_llCalculateDamage，而结算走 GetValue()→m_llValue
     *   ⇒ 改的是孤儿字段，功能静默无效（日志里「改前 m=2.8080」其实是 CalculateDamage 的值）。
     *   判据（lead 定）：mantOff>=0x10 且 expOff==mantOff+8 ⇒ 视为值类型，两偏移各减 0x10；
     *   减完必须正好是 0 / 8 才采信 src=field-vt，否则回退硬编码 0/8 并记 src=fallback。 */
    if (g_bi.src && strcmp(g_bi.src, "field") == 0 && g_bi.mantOff >= 0x10 &&
        g_bi.expOff == g_bi.mantOff + 8) {
        const size_t m2 = g_bi.mantOff - 0x10;
        const size_t e2 = g_bi.expOff - 0x10;
        if (m2 == 0 && e2 == 8) {
            g_bi.mantOff = m2;
            g_bi.expOff = e2;
            g_bi.src = "field-vt";
            g_bi.vt = 1;
        } else {
            g_bi.mantOff = 0;
            g_bi.expOff = 8;
            g_bi.src = "fallback";
        }
    }
    LOGI(BT_LOG " BigInt off=mant+0x%zx exp+0x%zx (src=%s vt=%d)", g_bi.mantOff, g_bi.expOff,
         g_bi.src, g_bi.vt);
}

static void HurtFrame(void* a1, uint8_t argBase) {
    if (!Plaus(a1)) return;
    /* 静态方法（`Hurt` 是静态，PROVEN）：第一个 IL 参数在槽 `argBase`（=0）。 */
    void* refSlot = reinterpret_cast<void*>(RawWord(a1, static_cast<size_t>(argBase) * 8));
    void* para = ParaOf(refSlot);
    if (!para) return;
    if (!ParaLayoutReady(para)) return;

    const uintptr_t pU = Un(para);
    void* target = RefAt(pU, g_para.offTarget);
    void* attacker = g_para.offAttacker ? RefAt(pU, g_para.offAttacker) : nullptr;
    if (!Plaus(target)) return;

    /* ① 身份闸：只放大「玩家侧打出、打在非玩家侧身上」的那一笔。
     *    —— 这是修「人物先死、boss 后死」的根本（用户 m02212/m02899 两次血证）。 */
    if (IsPlayerSide(target)) {
        g_statKillSkip.fetch_add(1, std::memory_order_relaxed);
        return;
    }
    /* 攻击者侧：三条身份判据（指针 / roleId / _hitter）之外再补一条**类型**判据
     *  `CharacterType.PLAYER == 0`（元数据枚举序）；四条任一命中即算玩家侧。
     *  全部不命中 = 不是玩家打的 ⇒ 不放大（用户 m02212/m02899 两次血证的根因就在这一行）。 */
    if (!IsPlayerSide(attacker) && CtypeOf(attacker) != 0) {
        g_statKillSkip.fetch_add(1, std::memory_order_relaxed);
        return;
    }
    /* ② 目标闸：`ctype==3` = BOSS ⇒ 菜单名是「秒杀**小怪**」，首领一律不动（用户 m02712 明令）。
     *    读不到 ctype（-1）时退回阵营判据 `_camp==1`（敌方），都读不到就不写。 */
    const int32_t ctype = CtypeOf(target);
    if (ctype == 3) {
        g_statKillBoss.fetch_add(1, std::memory_order_relaxed);
        return;
    }
    if (ctype != 2) {
        const int32_t camp = CampOf(target);
        if (camp != 1) {
            g_statKillBoss.fetch_add(1, std::memory_order_relaxed);
            return;
        }
    }

    /* ③ 改值：BigInt = mantissa(double) × 10^exponent。写前做值合理性闸（NaN/越界一律不写）。 */
    EnsureBigIntLayout();
    const size_t vOff = pU + g_para.offValue;
    double mant = 0.0;
    memcpy(&mant, reinterpret_cast<const void*>(vOff + g_bi.mantOff), 8);
    int32_t exp = I32At(vOff, g_bi.expOff);
    if (!(mant >= 0.0) || !std::isfinite(mant) || mant > 1.0e9 || exp < -40 || exp > 60) {
        g_statKillSkip.fetch_add(1, std::memory_order_relaxed);
        return;
    }
    const double oldMant = mant;
    const int32_t oldExp = exp;
    mant *= kKillMul;
    while (mant > kKillMantMax && exp < 300) {
        mant /= 10.0;
        exp += 1;
    }
    memcpy(reinterpret_cast<void*>(vOff + g_bi.mantOff), &mant, 8);
    SetI32(vOff, g_bi.expOff, exp);

    const uint32_t n = g_statKill.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 20 || (n % 200) == 0)
        LOGI(BT_LOG " 打怪#%u ctype=%d | 改前 m=%.4f e=%d → 改后 m=%.4f e=%d", n, ctype,
             oldMant, oldExp, mant, exp);
    /* v2 诊断（首 20 次）：写后**双向读回** —— 同一次 Hurt 里 m_llValue 与 m_llCalculateDamage
     * 各读一遍，用来确认我们到底改到了哪个字段（判 #1 是否真生效的唯一硬证据）。 */
    if (n <= 20) {
        double rdVal = 0.0;
        double rdCalc = 0.0;
        memcpy(&rdVal, reinterpret_cast<const void*>(vOff + g_bi.mantOff), 8);
        memcpy(&rdCalc, reinterpret_cast<const void*>(vOff + 0x10 + g_bi.mantOff), 8);
        LOGI(BT_LOG " RD#%u Value(+0x%zx) m=%.4f e=%d | Calc(+0x%zx) m=%.4f e=%d | offValue=0x%zx",
             n, g_bi.mantOff, rdVal, I32At(vOff, g_bi.expOff), 0x10 + g_bi.mantOff, rdCalc,
             I32At(vOff, 0x10 + g_bi.expOff), g_para.offValue);
    }
}

/* ============================ §5 功能 2：无敌 ============================ */

static std::atomic<uint32_t> g_statInv{0};
static std::atomic<uint32_t> g_statInvState{0};

/* 免疫位原名（元数据 `ActionState::immuneDamage` 0x04017E2D 等；`CBattleActorUtil::IsDamageImmune`
 * 只读这几条）。写过的位全部记录原值，关闭时逐位还原。 */
static const char* const kImmuneNames[] = {"immuneDamage", "immunePhysicalDamage", "immuneMagicDamage",
                                           "invincible"};

struct InvBit {
    uintptr_t addr;
    uint8_t orig;
    uint8_t wrote;
};
static InvBit g_invBits[8];
static std::atomic<uint32_t> g_invBitN{0};
static std::atomic<uint32_t> g_statInvWrite{0};   /* v2 诊断：免疫位写入次数 */

/** 维持玩家免疫位（每拍调）。只写 0/1，且只写「当前不是 1」的那些。 */
static void EnsureInvulnBits() {
    const uintptr_t pl = g_player.load(std::memory_order_relaxed);
    if (!pl) return;
    auto& api = GetApi();
    void* k = api.object_class(reinterpret_cast<void*>(pl));
    if (!Plaus(k)) return;
    const size_t so = OffDeep(k, "_state", nullptr);
    if (!so) return;
    void* st = RefAt(pl, so);
    if (!st) return;
    void* sk = KOf(st);
    if (!sk) return;
    for (size_t i = 0; i < sizeof(kImmuneNames) / sizeof(kImmuneNames[0]); i++) {
        const size_t o = Off(sk, kImmuneNames[i]);
        if (!o || o > 0x400) continue;
        const uintptr_t addr = Un(st) + o;
        const uint8_t cur = U8At(addr, 0);
        if (cur == 1) continue;
        if (cur != 0) continue;  // 非 0/1 不碰（可能是别的语义的字节）
        /* 登记原值（同一地址只登记一次） */
        bool known = false;
        const uint32_t bn = g_invBitN.load(std::memory_order_relaxed);
        for (uint32_t j = 0; j < bn && j < 8; j++)
            if (g_invBits[j].addr == addr) {
                known = true;
                break;
            }
        if (!known && bn < 8) {
            g_invBits[bn].addr = addr;
            g_invBits[bn].orig = cur;
            g_invBits[bn].wrote = 1;
            g_invBitN.store(bn + 1, std::memory_order_relaxed);
        }
        SetU8(addr, 0, 1);
        g_statInvState.fetch_add(1, std::memory_order_relaxed);
        if (g_statInvState.load(std::memory_order_relaxed) <= 6)
            LOGI(BT_LOG " 无敌 免疫位 %s +0x%zx → 1", kImmuneNames[i], o);
        /* v2 诊断：写入对象是谁 —— ActionState 地址 / 其 klass 名 / 字段名 / 偏移 / 原值。
         * 若这里的 cls 不是玩家自己的 state 类（或 state 地址每拍都变），就能立刻看出 #2 为什么无效。 */
        const uint32_t wn = g_statInvWrite.fetch_add(1, std::memory_order_relaxed) + 1;
        if (wn <= 20 || (wn % 200) == 0) {
            const char* cls = (api.class_name && Plaus(sk)) ? api.class_name(sk) : "?";
            LOGI(BT_LOG " IMMUNE state=%p cls=%s field=%s off=0x%zx old=%d",
                 reinterpret_cast<void*>(Un(st)), cls ? cls : "?", kImmuneNames[i], o,
                 static_cast<int>(cur));
        }
    }
}

/** 还原免疫位（关闭 / ForceClear）。 */
static void RestoreInvulnBits() {
    const uint32_t bn = g_invBitN.load(std::memory_order_relaxed);
    for (uint32_t i = 0; i < bn && i < 8; i++) {
        if (!g_invBits[i].addr) continue;
        if (Il2Cpp::IsReadableRange(g_invBits[i].addr, 1))  // 状态对象可能已随场景回收
            SetU8(g_invBits[i].addr, 0, g_invBits[i].orig);
        g_invBits[i].addr = 0;
    }
    g_invBitN.store(0, std::memory_order_relaxed);
}

/** `StarsCollider::DoAttack(IAttack, IAccept, …)` 帧：把「打玩家的那一击」accepter 槽置 0。
 *  官方现成路径（`_钩子目标与语义.md` §2.2，IL_0039-0044）：
 *    `if (accepter != null && accepter.interactiveGameObject == null) return;`
 *    ⇒ 置空等价于「本次命中作废」，不结算、不飘字、不触发死亡。 */
/* v2 诊断计数器（lead 要求：#2 本轮不猜、只加观测） */
static std::atomic<uint32_t> g_statDoAtkSeen{0};

/** #2 诊断：DoAttack 帧的结构快照（首 20 次 + 每 200 次一条，热路径不刷屏）。
 *  argc=7 是 StarsCollider::DoAttack(IAttack,IAccept,AttackData,AcceptData,Vector3,int,SynHitData)
 *  的固定参数个数（MxytzMod.cpp:397 g_origDoAttack 注释 / :958 注册表 DoAttack7）。 */
static void LogDoAttack(uintptr_t acc, uintptr_t ph, void* igo, bool hit) {
    const uint32_t n = g_statDoAtkSeen.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 20 || (n % 200) == 0)
        LOGI(BT_LOG " DOATTACK argc=7 accepter=%p vsHitter=%p igo=%p isPlayer=%d", (void*)acc,
             (void*)ph, igo, hit ? 1 : 0);
}

static void DoAttackFrame(void* a1, uint8_t argBase) {
    if (!Plaus(a1)) return;
    const size_t slot = static_cast<size_t>(argBase + 1) * 8;  // IL 第 2 参 = accepter
    void* accepter = reinterpret_cast<void*>(RawWord(a1, slot));
    if (!Plaus(accepter)) return;

    const uintptr_t acc = Un(accepter);
    const uintptr_t ph = g_playerHitter.load(std::memory_order_relaxed);
    bool hit = (ph && acc == ph);
    void* igo = nullptr;
    if (!hit) {
        auto& api = GetApi();
        void* k = api.object_class(accepter);
        if (!Plaus(k)) {
            LogDoAttack(acc, ph, nullptr, false);
            return;
        }
        static const char* const kIgoNames[] = {"_interactiveGameObject",
                                                "<interactiveGameObject>k__BackingField",
                                                "interactiveGameObject"};
        const size_t o = OffAny(k, kIgoNames, sizeof(kIgoNames) / sizeof(kIgoNames[0]));
        if (!o) {  // 认不出结构 ⇒ 一个字节都不动（写错槽 = 毁解释器帧）
            LogDoAttack(acc, ph, nullptr, false);
            return;
        }
        igo = RefAt(acc, o);
        if (Plaus(igo) && IsPlayerSide(igo)) hit = true;
    }
    LogDoAttack(acc, ph, igo, hit);
    if (!hit) return;
    *reinterpret_cast<uintptr_t*>(reinterpret_cast<uintptr_t>(a1) + slot) = 0;
    const uint32_t n = g_statInv.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 10 || (n % 100) == 0)
        LOGI(BT_LOG " 无敌 命中作废 accepter=%p（第%u次）", reinterpret_cast<void*>(acc), n);
}

/* ============================ §6 功能 3：全模式技能伤害增加（投射物） ============================ */

/* 真落点（`_钩子目标与语义.md` + v39 的 IL 铁证）：
 *   `ActionCommand_AddProjectile::Execute(ActionCommandData)` 的 IL_011B `callvirt get_count()`
 *   → loc4 = 17 参 `AddProjectile` 的**循环上界**（全工程唯一读 `_count` 的地方）。
 *   ⇒ 在 Execute 帧、它读 count 之前改 `ActionCommandData.data`（`LAddProjectile._count`）。
 *
 *  为什么「数量放大」而不是「伤害数值放大」：用户当轮明确定调 —— 数值放大在本作无效
 *   （v26s3/v26t/v28 属性与公式参数三条路都实测无效果，见 games 摘要与历史文件），
 *  斌哥包与旧版生效路径都是投射物扇出。
 *
 *  本轮相对旧实现的改动（专治「发烫/卡顿」）：
 *    ① 扇出 5 → **3**（对象数按扇出线性涨，5 倍弹幕是发热主因）；
 *    ② 删除 `_activeCountLimit` 写入（保留引擎自己的回收闸门，v47 已定案）；
 *    ③ 每秒「新增发数」硬预算，超了本次就**不放大**（不再靠 20s 挂起）；
 *    ④ 还原表按「我们写进去的值」精确还原（不再每帧做 maps 校验）。 */
static constexpr float kProjFanout = 3.0f;
static constexpr int32_t kProjCountCap = 120;
static constexpr uint32_t kProjPerSecCap = 600;  // 每秒新增投射物预算（超出本次不放大）

struct ProjSlot {
    void* data;
    const void* klass;  // 写入时记下的 `LAddProjectile` klass（还原时做裸比对，不调 il2cpp API）
    size_t countOff;    // 写入时记下的 `_count` 偏移（还原时不再按名解析）
    int32_t base;
    int32_t wrote;
};
static ProjSlot g_proj[32];
static std::atomic<uint32_t> g_projSlotN{0};
static std::atomic<uint32_t> g_statProj{0};
static std::atomic<uint32_t> g_statProjSkip{0};
static std::atomic<uint32_t> g_statProjBudget{0};
static std::atomic<uint32_t> g_statProjSec{0};      // 本秒已放大发数
static std::atomic<uint32_t> g_statProjPeak{0};     // 单秒峰值
static std::atomic<uint64_t> g_projSecMark{0};

static void ProjSecTick() {
    const uint64_t now = NowMs();
    const uint64_t mark = g_projSecMark.load(std::memory_order_relaxed);
    if (mark && now - mark < 1000ull) return;
    const uint32_t used = g_statProjSec.exchange(0, std::memory_order_relaxed);
    uint32_t peak = g_statProjPeak.load(std::memory_order_relaxed);
    while (used > peak && !g_statProjPeak.compare_exchange_weak(peak, used)) {}
    g_projSecMark.store(now, std::memory_order_relaxed);
}

/** 还原我们写过的 `_count`（只在「当前值仍是我们写的那个」时写回，避免覆盖游戏自己的修改）。 */
static void RestoreProj(const char* why) {
    /* ★2026-10-26 修复（真机 tombstone）：旧版还原时对**槽里记下的对象指针**再调
     *  `api.object_class()` + `Off(_count)` —— 而 `LAddProjectile` 是技能配置对象，
     *  换场景/换技能后会被回收，槽里的指针就成了野指针：`object_class` 从已释放内存读出
     *  垃圾 klass，再递给 `il2cpp_class_get_field_from_name` ⇒ il2cpp 内部 SIGSEGV
     *  （用户实测：关 #3 开关的瞬间闪退，特征 1 次，pc libil2cpp+0x205aa5c）。
     *  现在还原**只用裸内存**：klass 与 `_count` 偏移都在写入时记好，还原时
     *  ①先确认这段地址可读（maps 查询，还原是冷路径，允许）②再裸比对对象首字的 klass
     *  与记录值一致（不一致 = 已被回收/复用 ⇒ 直接放弃，不写）。全程不碰 il2cpp API。 */
    const uint32_t n = g_projSlotN.load(std::memory_order_relaxed);
    uint32_t restored = 0;
    uint32_t attempted = 0;
    for (uint32_t i = 0; i < n && i < 32; i++) {
        ProjSlot& sl = g_proj[i];
        if (sl.data && sl.klass && sl.countOff && Plaus(sl.data)) {
            attempted++;
            const uintptr_t base = Un(sl.data);
            const bool readable = Il2Cpp::IsReadableRange(base, sl.countOff + 4);
            const bool sameKlass =
                readable && *reinterpret_cast<const uintptr_t*>(base) == reinterpret_cast<uintptr_t>(sl.klass);
            if (readable && sameKlass) {
                const uintptr_t fp = base + sl.countOff;
                const int32_t cur = I32At(fp, 0);
                if (cur == sl.wrote && sl.wrote != sl.base) {
                    SetI32(fp, 0, sl.base);
                    restored++;
                }
            }
        }
        sl.data = nullptr;
        sl.klass = nullptr;
        sl.countOff = 0;
        sl.base = 0;
        sl.wrote = 0;
    }
    g_projSlotN.store(0, std::memory_order_relaxed);
    /* v3（lead 要求）：还原是否命中同一对象地址 —— n = 登记过的对象数，ok = 真写回成功的个数。
     * n>0 且 ok==0 ⇒ 地址已被回收/复用（_count 是**共享配置对象**，跨场景会换新实例）；
     * 这种情况下残留的放大值只能等游戏重建配置，报告里写明。 */
    LOGI(BT_LOG " PROJ restore n=%u ok=%u why=%s", attempted, restored, why ? why : "-");
    if (restored) LOGI(BT_LOG " 投射物已还原 count（%s，共 %u 个对象）", why ? why : "-", restored);
}

static ProjSlot* ProjSlotTake(void* data) {
    const uint32_t n = g_projSlotN.load(std::memory_order_relaxed);
    for (uint32_t i = 0; i < n && i < 32; i++)
        if (g_proj[i].data == data) return &g_proj[i];
    for (uint32_t i = 0; i < 32; i++)
        if (!g_proj[i].data) {
            if (i >= n) g_projSlotN.store(i + 1, std::memory_order_relaxed);
            g_proj[i].data = data;
            g_proj[i].klass = nullptr;
            g_proj[i].countOff = 0;
            g_proj[i].base = 0;
            g_proj[i].wrote = 0;
            return &g_proj[i];
        }
    g_proj[0].data = data;  // 满表：覆盖 0 号（正常战斗不会有 32 个不同技能配置）
    g_proj[0].klass = nullptr;
    g_proj[0].countOff = 0;
    g_proj[0].base = 0;
    g_proj[0].wrote = 0;
    return &g_proj[0];
}

static void ProjExecFrame(void* a1, uint8_t argBase) {
    if (!Plaus(a1)) return;
    auto& api = GetApi();
    if (!api.object_class || !api.class_name) return;

    /* ① 取 `ActionCommandData`（槽 argBase+0 / +1 里按类名核对；挡 `EventDelegate::Execute` 之类同名方法） */
    void* acd = nullptr;
    for (int i = 0; i < 2 && !acd; i++) {
        void* cand = reinterpret_cast<void*>(RawWord(a1, static_cast<size_t>(argBase + i) * 8));
        if (!Plaus(cand)) continue;
        void* ck = api.object_class(cand);
        if (!Plaus(ck)) continue;
        const char* cn = api.class_name(ck);
        if (cn && strstr(cn, "ActionCommandData")) acd = cand;
    }
    if (!acd) return;

    /* ② `acd.data` → `LAddProjectile`（类名核对 = IL 里的 isinst） */
    void* ak = api.object_class(acd);
    if (!Plaus(ak)) return;
    static const char* const kDataNames[] = {"data", "_data"};
    const size_t doff = OffAny(ak, kDataNames, sizeof(kDataNames) / sizeof(kDataNames[0]));
    if (!doff) return;
    void* data = RefAt(Un(acd), doff);
    if (!Plaus(data)) return;
    void* dk = api.object_class(data);
    if (!Plaus(dk)) return;
    const char* dn = api.class_name(dk);
    if (!dn || !strstr(dn, "LAddProjectile")) return;

    /* ③ 施法者闸：**只放大玩家侧施放**（用户 m02899：`LAddProjectile` 敌我共用实例 ⇒ 不判施法者
     *    会把敌人的弹幕一起翻倍，「我开着 3 被自己开的 3 打死」）。解不出施法者 ⇒ 不写（fail-closed）。 */
    static const char* const kCasterNames[] = {"actionGameObject", "action"};
    const size_t coff = OffAny(ak, kCasterNames, sizeof(kCasterNames) / sizeof(kCasterNames[0]));
    if (!coff) return;
    void* caster = RefAt(Un(acd), coff);
    if (!Plaus(caster)) return;
    bool playerCast = IsPlayerSide(caster);
    if (!playerCast) {
        /* `action` 兜底：Action → character / gameObject */
        void* ck2 = api.object_class(caster);
        if (Plaus(ck2)) {
            static const char* const kActNames[] = {"character", "_character", "gameObject", "_gameObject"};
            for (size_t i = 0; i < 4 && !playerCast; i++) {
                const size_t o = Off(ck2, kActNames[i]);
                if (!o) continue;
                void* p = RefAt(Un(caster), o);
                if (Plaus(p) && IsPlayerSide(p)) playerCast = true;
            }
        }
    }
    if (!playerCast) {
        g_statProjSkip.fetch_add(1, std::memory_order_relaxed);
        return;
    }

    /* ④ 改 `_count`（= 17 参 AddProjectile 的循环上界） */
    const size_t co = Off(dk, "_count");
    if (!co) return;
    const uintptr_t fp = Un(data) + co;
    const int32_t cur = I32At(fp, 0);
    if (cur <= 0 || cur > 10000) return;  // 安全闸：异常值一律不碰

    ProjSecTick();
    ProjSlot* s = ProjSlotTake(data);
    s->klass = reinterpret_cast<const void*>(Un(dk));
    s->countOff = co;
    if (s->wrote == 0) {
        /* 首次见到这个配置对象：记原始值（`base`），之后目标恒为 base × 扇出 */
        s->base = (cur < kProjCountCap) ? cur : kProjCountCap;
    }
    int32_t target = static_cast<int32_t>(static_cast<float>(s->base) * kProjFanout + 0.5f);
    if (target < 1) target = 1;
    if (target > kProjCountCap) target = kProjCountCap;
    if (target <= s->base) return;  // 原始 count 已经很大 ⇒ 不放大（只加不减）

    /* ⑤ 每秒预算：按「本次将放出的发数」计费；超预算 ⇒ 本次按游戏原值透传 */
    const uint32_t used = g_statProjSec.load(std::memory_order_relaxed);
    if (used + static_cast<uint32_t>(target) > kProjPerSecCap) {
        g_statProjBudget.fetch_add(1, std::memory_order_relaxed);
        return;
    }
    g_statProjSec.fetch_add(static_cast<uint32_t>(target), std::memory_order_relaxed);

    SetI32(fp, 0, target);
    s->wrote = target;
    const uint32_t n = g_statProj.fetch_add(1, std::memory_order_relaxed) + 1;
    /* v2 诊断：data 类名 / _count 偏移 / 原值 / 施法者指针 / 玩家侧判定 / 写入目标值。
     * 用来判 #3：写进去的 _count 到底是不是「17 参 AddProjectile 的循环上界」那个对象。 */
    if (n <= 20 || (n % 200) == 0)
        LOGI(BT_LOG " PROJ dataCls=%s cntOff=0x%zx cnt=%d caster=%p playerSide=%d wrote=%d",
             dn ? dn : "?", co, cur, reinterpret_cast<void*>(Un(caster)), playerCast ? 1 : 0,
             static_cast<int>(target));
}

/* ============ #2 第二层：Character::Damage 帧（2026-11-16 v3，lead 硬结论） ============
 * 为什么必须补：免疫位只挡「命中判定」两条读点（IsDamageImmune 读被打方 state.immuneDamage、
 * HitTest 读 accept.state.invincible 且在 DoAttack 之前 continue），而
 *   EffectToCast → CHitNotifier:485 → Character.Damage
 * 这条**服务端同步伤害**路径完全不检查免疫 ⇒ 免疫位全开也可能被打死（v2 真机无敌=0 的真相：
 * DoAttack 帧天然为 0，不是失效证据）。所以这里对 Damage 帧再挡一层。
 *
 * 落点：Character::Damage 两个重载都在 kKinds 里（K_DAMAGE，v3 起转发到本帧）：
 *   Damage(BigInt damage, DamageEnumerate.DamageType, bool)   ← 主路径（_钩子目标与语义.md）
 *   Damage(long   damage, DamageEnumerate.DamageType, bool)   ← 整数重载
 * 槽约定（HybridCLR 入口，见 MxytzBattle.h）：实例方法 this 在槽 argBase-1，第 k 个参数在
 * 槽 argBase+k。伤害值 = 第 0 参 ⇒ 槽 argBase。
 *
 * fail-closed 三条（任一不满足就不写）：
 *   ① this（被打方 Character）判不出玩家侧 ⇒ 不写（宁可不生效，也不误挡敌人）；
 *   ② 第 0 参槽值看起来像**指针**（BigInt 按值传参在解释器里可能是指针）⇒ 只零化其内容，
 *      读不到/不像 BigInt 就整帧放弃（绝不把指针槽写成 0，否则解引用空指针 = 崩）；
 *   ③ 槽值不像指针 ⇒ 当作内联 mantissa / long，直接写 0（BigInt 内联时首 8 字节 = mantissa）。 */
static std::atomic<uint32_t> g_statDmgSeen{0};
static std::atomic<uint32_t> g_statDmgBlock{0};

static void DamageFrame(void* a1, uint8_t argBase) {
    if (!Plaus(a1)) return;
    if (argBase < 1) return;  /* Damage 是实例方法；this 占 1 槽 */
    const uint32_t seen = g_statDmgSeen.fetch_add(1, std::memory_order_relaxed) + 1;
    if (seen <= 20 || (seen % 200) == 0)
        LOGI(BT_LOG " DAMAGE seen n=%u argBase=%u（帧观测）", seen, static_cast<unsigned>(argBase));

    void* self = reinterpret_cast<void*>(RawWord(a1, static_cast<size_t>(argBase - 1) * 8));
    if (!Plaus(self)) return;
    if (!IsPlayerSide(self)) return;  /* ① 只拦打在玩家侧的 */

    const size_t dmgSlot = static_cast<size_t>(argBase) * 8;
    const uintptr_t raw = RawWord(a1, dmgSlot);
    const uintptr_t slotAddr = reinterpret_cast<uintptr_t>(a1) + dmgSlot;
    int mode = 0;  /* 1 = 写槽为 0；2 = 零化 BigInt 内容 */
    if (starptr::InUserVa(raw)) {
        const uintptr_t bp = starptr::Untag(raw);
        if (!Il2Cpp::IsReadableRange(bp, 16)) return;  /* ② 判不出就不写 */
        double m = 0.0;
        memcpy(&m, reinterpret_cast<const void*>(bp), 8);
        const int32_t e = I32At(bp, 8);
        if (!std::isfinite(m) || m < 0.0 || m > 1.0e15 || e < -40 || e > 60) return;
        const double z = 0.0;
        const int32_t ze = 0;
        memcpy(reinterpret_cast<void*>(bp), &z, 8);
        SetI32(bp, 8, ze);
        mode = 2;
    } else {
        *reinterpret_cast<uintptr_t*>(slotAddr) = 0;
        mode = 1;
    }
    const uint32_t n = g_statDmgBlock.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 20 || (n % 200) == 0)
        LOGI(BT_LOG " DAMAGEBLOCK n=%u mode=%d slot=+0x%zx self=%p", n, mode, dmgSlot, self);
}

/* ============================ §7 对外接口 ============================ */

static std::atomic<bool> g_kill{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_skill{false};
static std::atomic<uint64_t> g_lastTickMs{0};

}  // namespace

void SetSwitches(bool killMob, bool invincible, bool skillDmg) {
    const bool skillWas = g_skill.exchange(skillDmg, std::memory_order_relaxed);
    const bool invWas = g_inv.exchange(invincible, std::memory_order_relaxed);
    g_kill.store(killMob, std::memory_order_relaxed);
    if (skillWas && !skillDmg) RestoreProj("功能关闭");
    if (invWas && !invincible) RestoreInvulnBits();
}

void OnFeatureChanged(int featNum, bool enabled) {
    /* 3 = Toggle_全模式技能伤害增加（与 MxytzMod.h 的 MxytzFeat 一致） */
    if (featNum == 3 && !enabled) RestoreProj("菜单关闭");
}

void OnTick() {
    const uint64_t now = NowMs();
    const uint64_t last = g_lastTickMs.load(std::memory_order_relaxed);
    if (last && now - last < 400ull) return;
    g_lastTickMs.store(now, std::memory_order_relaxed);
    RefreshPlayer();
    /* v2 诊断（lead 要求）：身份链心跳 —— 玩家指针 / hitter / roleId 三个身份锚点是否都取到。
     * 400ms 一拍（本函数已被 400ms 闸门限流）；#2 无效时先看这一行：ok=0 说明身份链断了。 */
    {
        const uintptr_t pl = g_player.load(std::memory_order_relaxed);
        const uintptr_t hh = g_playerHitter.load(std::memory_order_relaxed);
        const uint64_t rr = g_playerRole.load(std::memory_order_relaxed);
        const int ok = (pl && hh) ? 1 : 0;
        /* v3 限流（lead 要求）：每 400ms 一条太吵（约 1.5MB/分钟）⇒ 改成
         * 「ok 变化时立刻打一条」+「稳定后每 2s 一条」。 */
        static uint64_t s_lastIdMs = 0;
        static int s_lastOk = -1;
        if (ok != s_lastOk || s_lastIdMs == 0 || now - s_lastIdMs >= 2000ull) {
            LOGI(BT_LOG " ID player=%p hitter=%p roleId=%llu ok=%d", reinterpret_cast<void*>(pl),
                 reinterpret_cast<void*>(hh), static_cast<unsigned long long>(rr), ok);
            s_lastIdMs = now;
            s_lastOk = ok;
        }
    }
    if (g_inv.load(std::memory_order_relaxed)) EnsureInvulnBits();
}

void OnFrame(int frame, void* a1, uint8_t argBase) {
    if (!a1) return;
    switch (frame) {
        case FRAME_HURT:
            if (g_kill.load(std::memory_order_relaxed)) HurtFrame(a1, argBase);
            break;
        case FRAME_DOATTACK:
            if (g_inv.load(std::memory_order_relaxed)) DoAttackFrame(a1, argBase);
            break;
        case FRAME_PROJEXEC:
            if (g_skill.load(std::memory_order_relaxed)) ProjExecFrame(a1, argBase);
            break;
        case FRAME_DAMAGE:
            /* #2 第二层（v3）：服务端同步伤害路径不看免疫位 ⇒ 这里把伤害实参挡成 0 */
            if (g_inv.load(std::memory_order_relaxed)) DamageFrame(a1, argBase);
            break;
        case FRAME_PROJADD:
        default:
            break;  // 顶层 AddProjectile 帧只观测（旧版实测：它不读 `_count`，写它无效）
    }
}

void ForceClear() {
    g_kill.store(false, std::memory_order_relaxed);
    g_inv.store(false, std::memory_order_relaxed);
    g_skill.store(false, std::memory_order_relaxed);
    RestoreProj("ForceClear");
    RestoreInvulnBits();
    g_player.store(0, std::memory_order_relaxed);
    g_playerHitter.store(0, std::memory_order_relaxed);
    g_playerRole.store(0, std::memory_order_relaxed);
}

void StatLine(char* out, size_t cap) {
    if (!out || !cap) return;
    snprintf(out, cap,
             "BATTLE 开关 kill=%d inv=%d skill=%d | 命中 打怪=%u 无敌=%u 免疫位=%u 投射=%u "
             "| 跳过 非玩家侧=%u 首领/未知=%u 发射者非玩家=%u 预算=%u | 投射秒峰值=%u 槽=%u para深度=%d",
             g_kill.load() ? 1 : 0, g_inv.load() ? 1 : 0, g_skill.load() ? 1 : 0,
             g_statKill.load(), g_statInv.load(), g_statInvState.load(), g_statProj.load(),
             g_statKillSkip.load(), g_statKillBoss.load(), g_statProjSkip.load(),
             g_statProjBudget.load(), g_statProjPeak.load(), g_projSlotN.load(),
             g_paraDepth.load());
}

}  // namespace MxytzBattle
