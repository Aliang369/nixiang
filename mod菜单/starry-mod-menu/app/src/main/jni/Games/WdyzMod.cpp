/* ================================================================
 *  星光菜单游戏模块 — 我的勇者 7.6.0（wdyz）
 *
 *  路线（2026-09-07 星光自注入验证，替代 frida）：
 *    仅挂 Battle 结算层两个函数（§10.1-4 证 monSettle hook 安全）：
 *      - 秒杀：MonsterCore.SettleDamage（0x45F8524）value<0 时 ×1000 放大
 *      - 无敌：HeroCore.SettleDamage（0x45EE170）  value<0 时归 0（免伤）
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
static std::atomic<bool> g_dmg{false};  // 秒杀
static std::atomic<bool> g_inv{false};  // 无敌
static std::atomic<bool> g_reenter{false};

// BattleObjectCore.SettleDamage(value, shield, toughness, energy, fromgroup) 的函数签名：
//   MonsterCore.SettleDamage override @0x45F8524（怪受伤结算，秒杀用）
//   HeroCore.SettleDamage   override @0x45EE170（玩家受伤结算，无敌用）
typedef void (*SettleDamage_t)(void* self, float value, float shield, float toughness,
                               float energy, int fromgroup);
static SettleDamage_t g_origMonsterSettle = nullptr;
static SettleDamage_t g_origHeroSettle = nullptr;
static SettleDamage_t g_origBaseSettle = nullptr;

// ============================================================
// ② 偏移与 RVA（dump.cs + IDA 反汇编实测；换版本必须重 dump）
//   RVA 序言首 4 字节（小端）：
//     MonsterSettle 0x45F8524  "6d bb 2b eb" → 0xEB2BBB6D（stp d11,d10,[sp,#-0x50]!）
//     HeroSettle    0x45EE170  "6d bc 2b eb" → 0xEB2BBC6D（stp d11,d10,[sp,#-0x40]!）
// ============================================================
enum : size_t {
    kRvaMonsterSettle = 0x45F8524,
    kRvaHeroSettle    = 0x45EE170,
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
// 采样日志（确认 value 符号/量级；挂点测试期间小采样，避免热路径刷屏）
static int g_monSamp = 0;
static int g_heroSamp = 0;

// 敌我/类别判定（热路径：object_class 读对象头 O(1) + 类名缓存）
static int ClassTagOf(void* self) {
    auto& api = GetApi();
    if (!api.object_class || !self) return 0;
    void* k = api.object_class(self);
    if (!k || !api.class_name) return 0;
    const char* n = api.class_name(k);
    if (!n) return 0;
    if (strstr(n, "Monster") || strstr(n, "Boss") || strstr(n, "mob")) return 1;  // 敌
    if (strstr(n, "Hero")) return 2;  // 己方
    return 0;
}

// 基类 BattleObjectCore.SettleDamage — 真正 HP 落地点（override 重算后再经此转发）
// 秒杀：敌 core value ×1000；无敌：己 core value 归零（双保险，HeroSettle 已灵）
static void Wdyz_HookedBaseSettle(void* self, float value, float shield, float toughness,
                                  float energy, int fromgroup) {
    if (!g_reenter.load()) {
        int tag = ClassTagOf(self);
        if (tag == 1 && g_dmg.load()) {
            // 用户定稿：保持"五倍攻击"语义（×5），勿放大过度（护盾/多段血表现已实测）
            if (++g_monSamp <= 6) {
                LOGI(WDYZ_LOG "BASE enemy v=%+.3f -> %+.3f g=%d self=%p", value, value * 5.0f, fromgroup, self);
            }
            value *= 5.0f;
        } else if (tag == 2 && g_inv.load()) {
            if (++g_heroSamp <= 6) {
                LOGI(WDYZ_LOG "BASE hero v=%+.3f -> 0 g=%d self=%p", value, fromgroup, self);
            }
            value = 0.0f;
        }
    }
    if (g_origBaseSettle) g_origBaseSettle(self, value, shield, toughness, energy, fromgroup);
}

// 秒杀（菜单"十倍攻击"）[保留旧槽作诊断对照，逻辑已迁基类]
static void Wdyz_HookedMonsterSettle(void* self, float value, float shield, float toughness,
                                     float energy, int fromgroup) {
    if (++g_monSamp <= 4) {
        LOGI(WDYZ_LOG "MON raw v=%+.3f sh=%+.3f tg=%+.3f g=%d self=%p",
             value, shield, toughness, fromgroup, self);
    }
    if (g_origMonsterSettle) g_origMonsterSettle(self, value, shield, toughness, energy, fromgroup);
}

// 无敌：HeroCore.SettleDamage — 玩家受击结算 value 无条件归零
static void Wdyz_HookedHeroSettle(void* self, float value, float shield, float toughness,
                                  float energy, int fromgroup) {
    if (g_inv.load()) {
        if (++g_heroSamp <= 4) {
            LOGI(WDYZ_LOG "HERO settle v=%+.3f sh=%+.3f tg=%+.3f g=%d self=%p",
                 value, shield, toughness, fromgroup, self);
        }
        value = 0.0f;
    }
    if (g_origHeroSettle) g_origHeroSettle(self, value, shield, toughness, energy, fromgroup);
}

static const HookSlot kSlots[] = {
    // 无敌：HeroCore.SettleDamage override（已实测有效）
    {"HeroSettle",    kRvaHeroSettle,    0xEB2BBC6D, "Battle", "HeroCore",     "SettleDamage", 5,
     (void*)Wdyz_HookedHeroSettle,    (void**)&g_origHeroSettle},
    // 秒杀：BattleObjectCore.SettleDamage 基类（override 重算后的落地转发，按类名分派敌我）
    {"BaseSettle",    0x0,               0x0,        "Battle", "BattleObjectCore", "SettleDamage", 5,
     (void*)Wdyz_HookedBaseSettle,    (void**)&g_origBaseSettle},
    // 诊断对照槽（仅观察原始入参，秒杀不在这里改）
    {"MonsterSettle", kRvaMonsterSettle, 0xEB2BBB6D, "Battle", "MonsterCore",  "SettleDamage", 5,
     (void*)Wdyz_HookedMonsterSettle, (void**)&g_origMonsterSettle},
};

// ============================================================
// ⑥ 分组安装 + 懒装（开关全关 = 0 hook）
// ============================================================
static void InstallBattleHooks() {
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