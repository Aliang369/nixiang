/* ================================================================
 *  HxwjMod.cpp — 幻想武将模拟器（com.qxjc.hxwjmlq / TapTap 1.0.8 / vc8）
 *
 *  锚点全部来自 1.0.8 libil2cpp.so 静态反汇编 + 真机序言核对：
 *    秒杀       FightGwItem::AttackDamage / AttackDamageOnce / SkillDamage
 *    百分百掉落 FightTimeMgr::GetSuccessWuPing 内 UnityEngine.Random.Range
 *    免广告     ToponVideo::isReady / showAd / onReward
 *
 *  关键 ABI/语义：
 *    - FightGwItem 方法签名 (self=victim, xxOtherGw=attacker)。
 *      self+0x30 = useZbGwInfo，+0x64 = fightType；
 *      XxGwInfo+0x80 = curHp，+0x84 = hp；FightType: player=1 / gw=2。
 *    - Random.Range(int,int) 是静态方法，第三个参数是 MethodInfo*。
 *      只在 GetSuccessWuPing 的 5 个返回地址白名单内返回下界 0。
 *    - ToponVideo.onReward 只读 arg2+0x10 的广告位字符串；showAd 命中后
 *      用栈上假 event 直接走官方发奖链，不拉起真实 SDK。
 *
 *  地址坐标系：ELF LOAD 段实测 VirtAddr = FileOffset + 0x4000，dump.cs 的
 *  RVA 是相对 load bias 的虚拟地址，因此运行时地址 = dl_iterate_phdr 的
 *  dlpi_addr + RVA；不能用 /proc/self/maps 的文件偏移直接当 RVA。
 *
 *  防闪退：懒装（开关全关 = 0 hook）/ RVA 优先 + 序言校验 / 失败转按名 /
 *          幂等（*orig 非空即已挂）/ orig 必调 / Tick 只翻开关不做重活。
 * ================================================================ */

#include "HxwjMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <link.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define HXWJ_LOG "[hxwj]"
#define HXWJ_TARGET_LIB sec_lib_il2cpp()

// ============================================================
// ① 开关与状态
// ============================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_dmg{false};
static std::atomic<bool> g_drop{false};
static std::atomic<bool> g_ad{false};

// ============================================================
// ② 锚点常量（1.0.8；换包必须重新 dump + 序言核对）
// ============================================================
enum : uintptr_t {
    kRvaAttackDamage     = 0x1A733C8,
    kRvaAttackDamageOnce = 0x1A756B8,
    kRvaSkillDamage      = 0x1A7695C,
    kRvaRandomRangeInt   = 0x35BD284,  // UnityEngine.Random::Range(int,int)
    kRvaShowAd           = 0x1A91F08,
    kRvaIsReady          = 0x1A91FA0,
    kRvaOnReward         = 0x1A923EC,

    // GetSuccessWuPing 内 5 个 Random.Range 返回地址（相对 load bias）
    kRetDropMainPool     = 0x1A7C3A4,  // Range(0,100000) 主权重池
    kRetDropMgr16        = 0x1A7D634,  // Range(0,10000)，结果当前反汇编未见使用
    kRetDropOnePercent   = 0x1A7D714,  // Range(0,10000)，rand > 99 跳过
    kRetDropDifficulty   = 0x1A7DDB0,  // Range(0,1000)，难度相关
    kRetDropSecondPool   = 0x1A7E94C,  // Range(0,100000) 第二权重池
};

enum : uint32_t {
    kInsAttackDamage     = 0xD10143FF,  // sub sp, sp, #0x50
    kInsAttackDamageOnce = 0xF81D0FFE,  // str x30, [sp, #-0x30]!
    kInsSkillDamage      = 0xD10183FF,  // sub sp, sp, #0x60
    kInsRandomRangeLike  = 0xA9BE57FE,  // stp x30, x21, [sp, #-0x20]!
    kInsOnReward         = 0xF81C0FFE,  // str x30, [sp, #-0x40]!
};

enum : size_t {
    kOffFightType    = 0x64,  // FightGwItem::fightType
    kOffUseZbGwInfo  = 0x30,  // FightGwItem::useZbGwInfo
    kOffCurHp        = 0x80,  // XxGwInfo::curHp
    kOffTvIdCsj      = 0x20,  // ToponVideo::topon_video_id_CSJ
    kOffTvIdTx       = 0x28,  // ToponVideo::topon_video_id_TX
};

enum : int32_t {
    kFightTypePlayer = 1,
    kFightTypeGw     = 2,
};

// ============================================================
// ③ 安全助手（热路径只做轻量校验，禁止 SafeProbe）
// ============================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

static int32_t SafeReadI32(void* obj, size_t off, int32_t defv) {
    if (!OkPtr(obj)) return defv;
    int32_t v = defv;
    memcpy(&v, reinterpret_cast<uint8_t*>(obj) + off, sizeof(v));
    return v;
}

static void* SafeReadPtr(void* obj, size_t off) {
    if (!OkPtr(obj)) return nullptr;
    void* v = nullptr;
    memcpy(&v, reinterpret_cast<uint8_t*>(obj) + off, sizeof(v));
    return v;
}

static void SafeWriteI32(void* obj, size_t off, int32_t v) {
    if (OkPtr(obj)) Il2Cpp::WriteI32(obj, off, v);
}

// ============================================================
// ④ 地址助手：load bias + RVA；by-name 只作兜底
// ============================================================
static int HxwjPhdrCb(struct dl_phdr_info* info, size_t /*size*/, void* data) {
    if (info->dlpi_name && strstr(info->dlpi_name, HXWJ_TARGET_LIB) && info->dlpi_addr) {
        *reinterpret_cast<uintptr_t*>(data) = static_cast<uintptr_t>(info->dlpi_addr);
        return 1;
    }
    return 0;
}

static bool VaInExecLib(uintptr_t va) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return false;
    char line[768];
    bool ok = false;
    while (fgets(line, sizeof(line), fp)) {
        if (!strstr(line, HXWJ_TARGET_LIB)) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x')) continue;
        if (va >= start && va < end) {
            ok = true;
            break;
        }
    }
    fclose(fp);
    return ok;
}

static uintptr_t LibBaseAddr() {
    static uintptr_t base = 0;
    if (base) return base;
    uintptr_t found = 0;
    dl_iterate_phdr(HxwjPhdrCb, &found);
    if (found) {
        base = found;
        LOGI(HXWJ_LOG " libil2cpp base=0x%lX (dl_iterate_phdr)", (unsigned long)base);
    }
    return base;
}

static void* VaFromRva(const char* tag, uintptr_t rva, uint32_t expectInsn) {
    uintptr_t base = LibBaseAddr();
    if (!base) {
        LOGW(HXWJ_LOG " %s libil2cpp base not found", tag);
        return nullptr;
    }
    uintptr_t va = base + rva;
    if (!VaInExecLib(va)) {
        LOGW(HXWJ_LOG " %s rva 0x%lX outside executable libil2cpp maps", tag, (unsigned long)rva);
        return nullptr;
    }
    if (expectInsn) {
        uint32_t got = 0;
        memcpy(&got, reinterpret_cast<void*>(va), sizeof(got));
        if (got != expectInsn) {
            LOGW(HXWJ_LOG " %s rva drift got=0x%08X want=0x%08X - try by-name",
                 tag, got, expectInsn);
            return nullptr;
        }
    }
    return reinterpret_cast<void*>(va);
}

static bool HookAtRva(const char* tag, uintptr_t rva, uint32_t expectInsn,
                      void* hook, void** orig) {
    void* va = VaFromRva(tag, rva, expectInsn);
    if (!va) return false;
    if (DobbyHook(va, hook, orig) != 0) {
        LOGW(HXWJ_LOG " %s dobby fail @rva 0x%lX", tag, (unsigned long)rva);
        return false;
    }
    LOGI(HXWJ_LOG " %s hooked @rva 0x%lX", tag, (unsigned long)rva);
    return true;
}

static bool HookByNameLocal(const char* tag, const char* cls, const char* method,
                            int argc, void* hook, void** orig) {
    if (!cls || !method) return false;
    auto& api = GetApi();
    if (!api.ready && !api.Load(HXWJ_TARGET_LIB)) {
        LOGW(HXWJ_LOG " %s api not ready - %s degrade", tag, method);
        return false;
    }
    void* klass = api.FindClassByShortName(cls);
    if (!klass) {
        LOGW(HXWJ_LOG " %s class miss %s - degrade", tag, cls);
        return false;
    }
    void* mi = api.GetMethod(klass, method, argc);
    if (!mi) {
        LOGW(HXWJ_LOG " %s method miss %s.%s - degrade", tag, cls, method);
        return false;
    }
    void* fn = *reinterpret_cast<void**>(mi);
    if (!OkPtr(fn)) {
        LOGW(HXWJ_LOG " %s bad method ptr %s.%s", tag, cls, method);
        return false;
    }
    if (DobbyHook(fn, hook, orig) != 0) {
        LOGW(HXWJ_LOG " %s dobby fail @name %s.%s", tag, cls, method);
        return false;
    }
    LOGI(HXWJ_LOG " %s hooked %s.%s @name %p", tag, cls, method, fn);
    return true;
}

struct HookSlot {
    const char* tag;
    uintptr_t   rva;
    uint32_t    expectInsn;
    const char* cls;
    const char* method;
    int         argc;
    void*       hookFn;
    void**      origPtr;
};

static bool HookSlotInstall(const HookSlot& s) {
    if (s.origPtr && *s.origPtr) return true;  // 幂等：防双 Dobby 叠钩
    if (s.rva && HookAtRva(s.tag, s.rva, s.expectInsn, s.hookFn, s.origPtr)) return true;
    if (s.cls && s.method) {
        return HookByNameLocal(s.tag, s.cls, s.method, s.argc, s.hookFn, s.origPtr);
    }
    return false;
}

// ============================================================
// ⑤ Hook 实现
// ============================================================
typedef void (*AttackDamage_t)(void* self, void* xxOtherGw, void* method);
typedef void (*SkillDamage_t)(void* self, void* xxOtherGw, void* xxSkillInfo, void* method);
typedef int32_t (*RandomRange_t)(int32_t minInclusive, int32_t maxExclusive, void* method);
typedef bool (*IsReady_t)(void* self, void* method);
typedef void (*ShowAd_t)(void* self, void* method);
typedef void (*OnReward_t)(void* self, void* sender, void* erg, void* method);

static AttackDamage_t g_origAttackDamage = nullptr;
static AttackDamage_t g_origAttackDamageOnce = nullptr;
static SkillDamage_t  g_origSkillDamage = nullptr;
static RandomRange_t  g_origRandomRange = nullptr;
static IsReady_t      g_origIsReady = nullptr;
static ShowAd_t       g_origShowAd = nullptr;
static OnReward_t     g_origOnReward = nullptr;

// 秒杀：x0 = 被打者，x1 = 攻击者。仅命中 player -> gw，避免反向秒杀玩家。
static bool ApplyKill(void* victim, void* attacker) {
    if (!OkPtr(victim) || !OkPtr(attacker)) return false;
    const int32_t victimType = SafeReadI32(victim, kOffFightType, -1);
    const int32_t attackerType = SafeReadI32(attacker, kOffFightType, -1);
    if (attackerType != kFightTypePlayer || victimType != kFightTypeGw) return false;

    void* info = SafeReadPtr(victim, kOffUseZbGwInfo);
    if (!OkPtr(info)) return false;
    const int32_t curHp = SafeReadI32(info, kOffCurHp, 0);
    if (curHp <= 1) return false;  // 已死/残血不再写，避免复活或重复触发
    // 写 1 而不是 0：AttackDamageOnce 尾段是「curHp<=0 才走 showGwInfoHp(0)」。
    // 若先写 0，原函数在序言处直接早退，反而不会触发官方死亡/结算链；
    // 写 1 后原函数继续执行，任何 >=1 的伤害都会把 curHp 钳到 <=0 并走完整死亡路径。
    SafeWriteI32(info, kOffCurHp, 1);
    return true;
}

static void Hooked_AttackDamage(void* self, void* xxOtherGw, void* method) {
    if (g_dmg.load()) ApplyKill(self, xxOtherGw);
    if (g_origAttackDamage) g_origAttackDamage(self, xxOtherGw, method);
}

static void Hooked_AttackDamageOnce(void* self, void* xxOtherGw, void* method) {
    if (g_dmg.load()) ApplyKill(self, xxOtherGw);
    if (g_origAttackDamageOnce) g_origAttackDamageOnce(self, xxOtherGw, method);
}

static void Hooked_SkillDamage(void* self, void* xxOtherGw, void* xxSkillInfo, void* method) {
    if (g_dmg.load()) ApplyKill(self, xxOtherGw);
    if (g_origSkillDamage) g_origSkillDamage(self, xxOtherGw, xxSkillInfo, method);
}

// 百分百掉落：只拦 GetSuccessWuPing 内的 5 个随机点，其他 Random.Range 全放行。
// 主池/第二池的判定是「weight > rand」，返回 0 让首个正权重候选必中；
// 附加闸门返回 0 使「rand 超过阈值才跳过」的分支必过。
static bool IsDropGateReturn(uintptr_t off) {
    switch (off) {
        case kRetDropMainPool:
        case kRetDropMgr16:
        case kRetDropOnePercent:
        case kRetDropDifficulty:
        case kRetDropSecondPool:
            return true;
        default:
            return false;
    }
}

static int32_t Hooked_RandomRange(int32_t minInclusive, int32_t maxExclusive, void* method) {
    if (g_drop.load()) {
        const uintptr_t base = LibBaseAddr();
        if (base) {
            const uintptr_t ret = reinterpret_cast<uintptr_t>(__builtin_return_address(0));
            const uintptr_t off = ret - base;
            if (IsDropGateReturn(off)) {
                static std::atomic<int> s_hits{0};
                const int n = s_hits.fetch_add(1);
                if (n < 8) {
                    LOGI(HXWJ_LOG " drop gate forced ret=0x%lX range=[%d,%d)",
                         (unsigned long)off, minInclusive, maxExclusive);
                }
                return minInclusive;
            }
        }
    }
    if (g_origRandomRange) return g_origRandomRange(minInclusive, maxExclusive, method);
    return minInclusive;
}

// 免广告：showAd 命中后构造栈上假 event，直接走官方 onReward 发奖链。
// onReward 只读 erg+0x10（广告位 id 字符串）；klass 填真实 string 类以防
// 保守 GC 扫描时把 id 当作无主对象。
static void GrantAdReward(void* self) {
    if (!OkPtr(self)) return;
    void* placement = SafeReadPtr(self, kOffTvIdTx);
    if (!OkPtr(placement)) placement = SafeReadPtr(self, kOffTvIdCsj);
    if (!OkPtr(placement)) {
        LOGW(HXWJ_LOG " ad placement id null - skip reward");
        return;
    }

    OnReward_t fn = g_origOnReward;
    if (!fn) {
        void* va = VaFromRva("onReward", kRvaOnReward, kInsOnReward);
        if (!va) {
            LOGW(HXWJ_LOG " onReward anchor missing - skip reward");
            return;
        }
        fn = reinterpret_cast<OnReward_t>(va);
    }

    struct FakeAdEvent {
        void* klass;
        void* monitor;
        void* placement;
        void* pad;
    } fake = {};
    auto& api = GetApi();
    fake.klass = (api.ready && api.object_class) ? api.object_class(placement) : nullptr;
    fake.placement = placement;
    fn(self, nullptr, &fake, nullptr);
}

static bool Hooked_IsReady(void* self, void* method) {
    if (g_ad.load()) return true;
    return g_origIsReady ? g_origIsReady(self, method) : false;
}

static void Hooked_ShowAd(void* self, void* method) {
    if (g_ad.load() && OkPtr(self)) {
        LOGI(HXWJ_LOG " showAd intercepted - grant reward without video");
        GrantAdReward(self);
        return;  // 不调 orig，避免真实广告 SDK 被拉起
    }
    if (g_origShowAd) g_origShowAd(self, method);
}

static void Hooked_OnReward(void* self, void* sender, void* erg, void* method) {
    if (g_origOnReward) g_origOnReward(self, sender, erg, method);
}

// ============================================================
// ⑥ 挂点表与分组安装
// ============================================================
static const HookSlot kSlots[] = {
    // 秒杀
    {"AttackDamage",     kRvaAttackDamage,     kInsAttackDamage,     "FightGwItem", "AttackDamage",     1,
     (void*)Hooked_AttackDamage,     (void**)&g_origAttackDamage},
    {"AttackDamageOnce", kRvaAttackDamageOnce, kInsAttackDamageOnce, "FightGwItem", "AttackDamageOnce", 1,
     (void*)Hooked_AttackDamageOnce, (void**)&g_origAttackDamageOnce},
    {"SkillDamage",      kRvaSkillDamage,      kInsSkillDamage,      "FightGwItem", "SkillDamage",      2,
     (void*)Hooked_SkillDamage,      (void**)&g_origSkillDamage},
    // 百分百掉落（Random.Range(int,int) 有 float 重载，禁止按名）
    {"RandomRange",      kRvaRandomRangeInt,   kInsRandomRangeLike,  nullptr,       nullptr,            0,
     (void*)Hooked_RandomRange,      (void**)&g_origRandomRange},
    // 免广告（onReward 先装，保证 showAd 内可直接走原始发奖函数）
    {"onReward",         kRvaOnReward,         kInsOnReward,         "ToponVideo",  "onReward",         2,
     (void*)Hooked_OnReward,         (void**)&g_origOnReward},
    {"isReady",          kRvaIsReady,          kInsRandomRangeLike,  "ToponVideo",  "isReady",          0,
     (void*)Hooked_IsReady,          (void**)&g_origIsReady},
    {"showAd",           kRvaShowAd,           kInsRandomRangeLike,  "ToponVideo",  "showAd",           0,
     (void*)Hooked_ShowAd,           (void**)&g_origShowAd},
};

static void InstallSlotByTag(const char* tag) {
    for (const auto& s : kSlots) {
        if (strcmp(s.tag, tag) != 0) continue;
        if (HookSlotInstall(s)) g_anyHooked.store(true);
        return;
    }
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    if (g_dmg.load()) {
        InstallSlotByTag("AttackDamage");
        InstallSlotByTag("AttackDamageOnce");
        InstallSlotByTag("SkillDamage");
    }
    if (g_drop.load()) InstallSlotByTag("RandomRange");
    if (g_ad.load()) {
        InstallSlotByTag("onReward");
        InstallSlotByTag("isReady");
        InstallSlotByTag("showAd");
    }
}

// ============================================================
// ⑦ 对外接口（ModEntry.h 注册表契约）
// ============================================================
bool Hxwj_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(HXWJ_TARGET_LIB)) return false;
    g_libReady.store(true);
    SyncHooks();
    LOGI(HXWJ_LOG " lib ready (load-bias RVA, lazy hooks)");
    return true;
}

bool Hxwj_IsReady() { return g_libReady.load(); }

void Hxwj_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case HXWJ_FEAT_DMG:  g_dmg.store(enabled);  break;
        case HXWJ_FEAT_DROP: g_drop.store(enabled); break;
        case HXWJ_FEAT_AD:   g_ad.store(enabled);   break;
        default: break;
    }
    SyncHooks();
}

void Hxwj_OnValueChanged(int /*featNum*/, int /*value*/) {}

void Hxwj_Tick(const bool* features, const int* /*values*/) {
    if (features) {
        g_dmg.store(features[HXWJ_FEAT_DMG]);
        g_drop.store(features[HXWJ_FEAT_DROP]);
        g_ad.store(features[HXWJ_FEAT_AD]);
    }
    if (!g_libReady.load() && isLibraryLoaded(HXWJ_TARGET_LIB)) g_libReady.store(true);
    SyncHooks();
}

void Hxwj_ForceClear() {
    g_dmg.store(false);
    g_drop.store(false);
    g_ad.store(false);
}
