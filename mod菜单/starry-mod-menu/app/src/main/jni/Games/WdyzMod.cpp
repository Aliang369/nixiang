/* ================================================================
 *  星光菜单游戏模块 — 我的勇者 7.6.3（wdyz，vc205）
 *
 *  路线（2026-09-07 星光自注入验证，替代 frida）：
 *    仅挂 Battle 结算层两个函数（§10.1-4 证 monSettle hook 安全）：
 *      - 五倍攻击：BattleObjectCore.SettleDamage 基类处理普通怪；类型 7 在 MonsterCore 覆盖入口处理
 *      - 无敌：HeroCore.SettleDamage value/shield 归 0；基类仅作己方兜底
 *  2026-09-28 7.6.3(vc205) 版本重定位（重 dump 实证）：
 *    - 类/方法名与参数个数保持 7.6.1 形态（HeroCore/BattleObjectCore/MonsterCore.SettleDamage 均 5 参）
 *    - il2cpp 12 个导出（domain_get_assemblies/class_from_name/…）全在 → 按名路径可用
 *    - RVA 全漂移，且修正历史 expectInsn 字节序错误：dump.cs 的 **Offset 字段才是文件偏移**
 *      （Offset = RVA - 0x4000；旧值用的是 RVA，故 7.6.0 时期实际一直走 @by-name 兜底）
 *    铁律：不挂 TryEnterCd/任何技能/CD 簇函数（§10.1-1 实锤保护会 SIGILL 自杀）；
 *          不直调任何 IL2CPP 方法（GoDeath 等在 frida 回调内必崩，§10.1-2/3）。
 *    防闪退设施保留：懒装 / 双路径挂点 / RVA 序言校验 / 幂等 / orig 必调 / OkPtr 校验。
 * ================================================================ */

#include "WdyzMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define WDYZ_LOG "[Wdyz]"

// ============================================================
// ① 开关与 orig
// ============================================================
static std::atomic<bool> g_libReady{false};  // il2cpp 已映射（≠ 已 hook）
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_dmg{false};  // 五倍攻击
static std::atomic<bool> g_inv{false};  // 无敌

// BattleObjectCore.SettleDamage(value, shield, toughness, energy, fromgroup) 的函数签名：
//   MonsterCore.SettleDamage override（7.6.3 Offset 0x47BF274，五倍攻击）
//   HeroCore.SettleDamage   override（7.6.3 Offset 0x4794970，无敌）
//   BattleObjectCore.SettleDamage 基类（7.6.3 Offset 0x47748A4，仅无敌兜底）
// 参数寄存器：s0=value s1=shield s2=toughness s3=energy w4=fromgroup（7.6.3 ABI 不变）
typedef void (*SettleDamage_t)(void* self, float value, float shield, float toughness,
                               float energy, int fromgroup);
static SettleDamage_t g_origMonsterSettle = nullptr;
static SettleDamage_t g_origHeroSettle = nullptr;
static SettleDamage_t g_origBaseSettle = nullptr;

// ============================================================
// ② 偏移与 RVA（7.6.3/vc205 重 dump：/tmp/wdyz763/il2cpp_dump/dump.cs）
//   地址口径 = dump.cs 的 Offset（文件偏移），供 FileOffToVa 在 /proc/self/maps 里换算 VA。
//   RVA 序言首 4 字节（小端 memcpy 值）：
//     MonsterCore.SettleDamage      Offset 0x47BF274  "eb 2b bb 6d" → 0x6DBB2BEB（stp d11,d10,[sp,#-0x50]!）
//     HeroCore.SettleDamage         Offset 0x4794970  "eb 2b bc 6d" → 0x6DBC2BEB（stp d11,d10,[sp,#-0x40]!）
//     BattleObjectCore.SettleDamage Offset 0x47748A4  "ff 03 05 d1" → 0xD10503FF（sub sp,sp,#0x140）
//   任一序言不符（热更/漂移）→ 自动转按名，不挂错地址。
// ============================================================
enum : size_t {
    kRvaMonsterSettle = 0x47BF274,
    kRvaHeroSettle    = 0x4794970,
    kRvaBaseSettle    = 0x47748A4,
};

// ============================================================
// ③ 安全助手（防闪退核心，勿删）
// ============================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

// ============================================================
// ④ 双路径挂点机器（提炼自 WhrhxMod 实测；禁 base+RVA、禁全 RX 误扫）
// ============================================================
static bool IsLibMapLine(const char* line, const char* lib) {
    return line && lib && strstr(line, lib) && strstr(line, ".so");
}

// file_off → 运行时 VA：只在 RX 段找（libil2cpp 分段映射，base+RVA 会挂错）
static void* FileOffToVa(const char* lib, uintptr_t fileOff) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return nullptr;
    char line[768];
    void* found = nullptr;
    size_t bestSpan = 0;
    while (fgets(line, sizeof(line), fp)) {
        if (!IsLibMapLine(line, lib)) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x') || end <= start) continue;
        if (fileOff < off || fileOff - off >= static_cast<uintptr_t>(end - start)) continue;
        const size_t span = static_cast<size_t>(end - start);
        if (span > bestSpan) {
            bestSpan = span;
            found = reinterpret_cast<void*>(start + (fileOff - off));
        }
    }
    fclose(fp);
    return found;
}

// RVA 路径：序言不匹配 = 版本漂移/空页，拒绝挂（防挂错地址闪退），转按名
static bool HookAtRva(const char* tag, uintptr_t rva, uint32_t expectInsn, void* hook, void** orig) {
    void* va = FileOffToVa(GameUnity::TARGET_LIB, rva);
    if (!va) {
        LOGW("%s rva 0x%lX not in RX — try by-name", tag, (unsigned long)rva);
        return false;
    }
    if (expectInsn) {
        uint32_t got = 0;
        memcpy(&got, va, sizeof(got));
        if (got == 0 || got == 0xFFFFFFFFu || got != expectInsn) {
            LOGW("%s rva drift got=0x%08X want=0x%08X — try by-name", tag, got, expectInsn);
            return false;
        }
    }
    if (DobbyHook(va, hook, orig) != 0) {
        LOGW("%s dobby fail @rva", tag);
        return false;
    }
    LOGI("%s hooked @rva 0x%lX", tag, (unsigned long)rva);
    return true;
}

static bool EnsureApi() {
    if (GetApi().ready) return true;
    return GetApi().Load(GameUnity::TARGET_LIB);
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

// 按名路径：类/方法缺失只 log 降级，不整包失败
static bool HookByName(const char* tag, const char* ns, const char* cls, const char* method,
                       int argc, void* hook, void** orig) {
    if (!EnsureApi()) {
        LOGW("%s api not ready — %s will retry via Tick", tag, method);
        return false;
    }
    auto& api = GetApi();
    void* k = FindClassNs(ns, cls);
    if (!k) k = api.FindClassByShortName(cls);
    if (!k) {
        LOGW("%s class miss %s — degrade", tag, cls);
        return false;
    }
    void* mi = api.GetMethod(k, method, argc);
    if (!mi) {
        LOGW("%s method miss %s.%s — degrade, install others", tag, cls, method);
        return false;
    }
    void* fn = *reinterpret_cast<void**>(mi);
    if (!OkPtr(fn)) {
        LOGW("%s bad method ptr %s.%s", tag, cls, method);
        return false;
    }
    if (DobbyHook(fn, hook, orig) != 0) {
        LOGW("%s dobby fail @name %s", tag, method);
        return false;
    }
    LOGI("%s hooked %s.%s", tag, cls, method);
    return true;
}

struct HookSlot {
    const char* tag;
    uintptr_t   rva;         // 0 = 只走按名
    uint32_t    expectInsn;  // 0 = 不校验序言
    const char* ns;
    const char* cls;
    const char* method;
    int         argc;
    void*       hookFn;
    void**      origPtr;
};

// 幂等 + 双路径：*orig 非空 = 已挂过（防双 Dobby 叠钩 SIGSEGV）；RVA 失败转按名
static bool HookSlotInstall(const HookSlot& s) {
    if (s.origPtr && *s.origPtr) return true;
    if (s.rva && HookAtRva(s.tag, s.rva, s.expectInsn, s.hookFn, s.origPtr)) return true;
    return HookByName(s.tag, s.ns, s.cls, s.method, s.argc, s.hookFn, s.origPtr);
}

// ============================================================
// ⑤ hook 函数与挂点表（仅 Battle 结算层；绝不挂技能/CD 簇函数）
// ============================================================
// 采样日志：raw* 在过滤前记录，确保能区分“Hook 未进入”和“类别过滤未命中”。
static std::atomic<int> g_monSamp{0};
static std::atomic<int> g_heroSamp{0};
static std::atomic<int> g_rawBaseSamp{0};
static std::atomic<int> g_rawHeroSamp{0};
static std::atomic<int> g_rawMonSamp{0};

static void* g_monsterCoreClass = nullptr;
static void* g_heroCoreClass = nullptr;
static std::atomic<bool> g_classCacheReady{false};

static bool IsSameOrSubclass(void* klass, void* base) {
    if (!klass || !base) return false;
    if (klass == base) return true;
    auto& api = GetApi();
    if (!api.class_parent) return false;
    for (int i = 0; i < 32 && klass; i++) {
        klass = api.class_parent(klass);
        if (klass == base) return true;
    }
    return false;
}

static void ResolveBattleClasses() {
    if (g_classCacheReady.load() || !EnsureApi()) return;
    void* monster = FindClassNs("Battle", "MonsterCore");
    void* hero = FindClassNs("Battle", "HeroCore");
    if (monster) g_monsterCoreClass = monster;
    if (hero) g_heroCoreClass = hero;
    if (monster && hero) {
        g_classCacheReady.store(true);
        LOGI(WDYZ_LOG " class cache MonsterCore=%p HeroCore=%p", monster, hero);
    }
}

static const char* ClassNameOf(void* self) {
    auto& api = GetApi();
    if (!self || !api.object_class || !api.class_name) return "?";
    void* k = api.object_class(self);
    const char* n = k ? api.class_name(k) : nullptr;
    return n ? n : "?";
}

// 敌我/类别判定：优先用安装时缓存的精确 IL2CPP 类与父类链，类名仅作兜底。
static int ClassTagOf(void* self) {
    auto& api = GetApi();
    if (!api.object_class || !self) return 0;
    void* k = api.object_class(self);
    if (!k) return 0;
    if (IsSameOrSubclass(k, g_monsterCoreClass)) return 1;
    if (IsSameOrSubclass(k, g_heroCoreClass)) return 2;
    if (!api.class_name) return 0;
    const char* n = api.class_name(k);
    if (!n) return 0;
    if (strstr(n, "Monster") || strstr(n, "Boss") || strstr(n, "mob") ||
        strstr(n, "Mob")) return 1;
    if (strstr(n, "Hero")) return 2;
    return 0;
}

// 敌我/类别判定（热路径：object_class 读对象头 O(1) + 类名缓存）
// MonsterCore.monster @0xD8 -> Monster.<monsterType>k__BackingField @0x60。
// 只读字段，不调 IL2CPP 方法；普通怪仍走基类落地点，类型 7 才需在覆盖入口处理。
static int MonsterTypeOf(void* self) {
    if (!self) return -1;
    auto* core = static_cast<uint8_t*>(self);
    void* monster = *reinterpret_cast<void**>(core + 0xD8);
    if (!monster) return -1;
    auto* obj = static_cast<uint8_t*>(monster);
    return *reinterpret_cast<int*>(obj + 0x60);
}

// 基类 BattleObjectCore.SettleDamage — 真正 HP 落地点（override 重算后再经此转发）
// 普通怪的五倍攻击必须在这里改：MonsterCore 覆盖入口会重算 value，改入口参数无效。
// 同时兼任无敌兜底：HeroCore 覆盖钩若失败，仍按精确类指针识别己方 core 并清零两个分量。
static void Wdyz_HookedBaseSettle(void* self, float value, float shield, float toughness,
                                  float energy, int fromgroup) {
    if (g_rawBaseSamp.fetch_add(1) < 12) {
        LOGI(WDYZ_LOG "BASE raw v=%+.3f sh=%+.3f tg=%+.3f g=%d self=%p class=%p tag=%d name=%s",
             value, shield, toughness, fromgroup, self,
             GetApi().object_class ? GetApi().object_class(self) : nullptr,
             ClassTagOf(self), ClassNameOf(self));
    }
    const int tag = ClassTagOf(self);
    if (tag == 1 && g_dmg.load()) {
        const float inValue = value;
        if (value != 0.0f) value *= 5.0f;
        if (g_monSamp.fetch_add(1) < 12) {
            LOGI(WDYZ_LOG "BASE enemy x5 v=%+.3f->%+.3f sh=%+.3f g=%d self=%p",
                 inValue, value, shield, fromgroup, self);
        }
    } else if (tag == 2 && g_inv.load()) {
        if (g_heroSamp.fetch_add(1) < 12) {
            LOGI(WDYZ_LOG "BASE hero inv v=%+.3f->0 sh=%+.3f->0 g=%d self=%p",
                 value, shield, fromgroup, self);
        }
        value = 0.0f;
        shield = 0.0f;
    }
    if (g_origBaseSettle) g_origBaseSettle(self, value, shield, toughness, energy, fromgroup);
}

// 五倍攻击：仅处理 DamageCollectionBoss=7。它在覆盖入口内提前返回，不走基类。
static void Wdyz_HookedMonsterSettle(void* self, float value, float shield, float toughness,
                                     float energy, int fromgroup) {
    const int monsterType = MonsterTypeOf(self);
    if (g_rawMonSamp.fetch_add(1) < 12) {
        LOGI(WDYZ_LOG "MON raw v=%+.3f sh=%+.3f tg=%+.3f g=%d type=%d self=%p class=%p name=%s",
             value, shield, toughness, fromgroup, monsterType, self,
             GetApi().object_class ? GetApi().object_class(self) : nullptr,
             ClassNameOf(self));
    }
    if (g_dmg.load() && monsterType == 7) {
        const float inValue = value;
        if (value != 0.0f) value *= 5.0f;
        if (g_monSamp.fetch_add(1) < 12) {
            LOGI(WDYZ_LOG "MON type7 x5 v=%+.3f->%+.3f sh=%+.3f tg=%+.3f g=%d self=%p",
                 inValue, value, shield, toughness, fromgroup, self);
        }
    }
    if (g_origMonsterSettle) g_origMonsterSettle(self, value, shield, toughness, energy, fromgroup);
}

// 无敌：HeroCore.SettleDamage — value 与 shield 都是承伤分量，必须同时归零。
static void Wdyz_HookedHeroSettle(void* self, float value, float shield, float toughness,
                                  float energy, int fromgroup) {
    if (g_rawHeroSamp.fetch_add(1) < 12) {
        LOGI(WDYZ_LOG "HERO raw v=%+.3f sh=%+.3f tg=%+.3f g=%d self=%p class=%p name=%s",
             value, shield, toughness, fromgroup, self,
             GetApi().object_class ? GetApi().object_class(self) : nullptr,
             ClassNameOf(self));
    }
    if (g_inv.load()) {
        if (value != 0.0f || shield != 0.0f) {
            if (g_heroSamp.fetch_add(1) < 12) {
                LOGI(WDYZ_LOG "HERO inv v=%+.3f->0 sh=%+.3f->0 tg=%+.3f g=%d self=%p",
                     value, shield, toughness, fromgroup, self);
            }
        }
        value = 0.0f;
        shield = 0.0f;
    }
    if (g_origHeroSettle) g_origHeroSettle(self, value, shield, toughness, energy, fromgroup);
}

static const HookSlot kSlots[] = {
    // 无敌：HeroCore.SettleDamage override（主路径）
    {"HeroSettle",    kRvaHeroSettle,    0x6DBC2BEB, "Battle", "HeroCore",     "SettleDamage", 5,
     (void*)Wdyz_HookedHeroSettle,    (void**)&g_origHeroSettle},
    // 五倍攻击：MonsterCore.SettleDamage override（仅 DamageCollectionBoss 类型 7）
    {"MonsterSettle", kRvaMonsterSettle, 0x6DBB2BEB, "Battle", "MonsterCore",  "SettleDamage", 5,
     (void*)Wdyz_HookedMonsterSettle, (void**)&g_origMonsterSettle},
    // 五倍攻击主路径 + 无敌兜底：BattleObjectCore.SettleDamage 基类
    {"BaseSettle",    kRvaBaseSettle,    0xD10503FF, "Battle", "BattleObjectCore", "SettleDamage", 5,
     (void*)Wdyz_HookedBaseSettle,    (void**)&g_origBaseSettle},
};

// ============================================================
// ⑥ 分组安装 + 懒装（开关全关 = 0 hook）
// ============================================================
static void InstallBattleHooks() {
    ResolveBattleClasses();
    for (const auto& s : kSlots) {
        if (HookSlotInstall(s)) g_anyHooked.store(true);
    }
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    if (g_dmg.load() || g_inv.load()) InstallBattleHooks();
}

// ============================================================
// ⑦ 对外接口（Main.cpp 契约，勿改签名）
// ============================================================
bool Wdyz_TryInit() {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;
    g_libReady.store(true);
    SyncHooks();  // 已 ON 的开关立即装；全关则保持 0 hook
    LOGI(WDYZ_LOG " lib ready (dual-path, lazy hooks)");
    return true;
}

bool Wdyz_IsReady() { return g_libReady.load(); }

void Wdyz_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case WDYZ_FEAT_DMG: g_dmg.store(enabled); break;
        case WDYZ_FEAT_INV: g_inv.store(enabled); break;
        default: break;
    }
    SyncHooks();
}

void Wdyz_OnValueChanged(int /*featNum*/, int /*value*/) {}

void Wdyz_Tick(const bool* features) {
    // 只做：同步开关 + 兜底安装。禁止在这里 SafeProbe/扫堆/遍历对象/逐对象写
    if (features) {
        g_dmg.store(features[WDYZ_FEAT_DMG]);
        g_inv.store(features[WDYZ_FEAT_INV]);
    }
    if (!g_libReady.load() && isLibraryLoaded(GameUnity::TARGET_LIB)) g_libReady.store(true);
    SyncHooks();
}

void Wdyz_ForceClear() {
    g_dmg.store(false);
    g_inv.store(false);
}
