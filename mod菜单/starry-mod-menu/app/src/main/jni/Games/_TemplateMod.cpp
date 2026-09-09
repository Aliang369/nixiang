/* ================================================================
 *  星光菜单游戏模块 — 标准骨架 v3（注册表版，复制后按 TODO 填空）
 *
 *  ⚠️ 防闪退设施已内置为代码，删任何一节都可能复现历史闪退：
 *    默认 0 hook（懒装）/ 双路径挂点 / RVA 漂移拒绝 / 幂等安装
 *    orig 必调 / reenter 防递归 / SafeWrite 校验写 / 类名缓存 / Tick 轻量
 *  生成：bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh {Game}
 *  （产出四件套：Mod.h/.cpp + Entry.cpp + Android_{tag}.mk，走 MainModern.cpp）
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 *
 *  功能实现分层（定稿 2026-09-09，详见 GAME_MOD_GUIDE.md）：
 *    数据型（无敌=HP回写/属性增益/资源锁定）→ 偏移定位 + SafeWrite 直写，
 *      Tick 轮询维持（零 hook 痕迹，防封首选；开关全关=零改写）
 *    行为型（改判定/伤害结算/拦截上报）   → Dobby hook：RVA 优先（序言
 *      校验），RVA 失效自动转按名（本骨架 HookSlot 双路径）
 *    字节 patch 改函数 → 默认禁用（E15-E18 死路清单）
 * ================================================================ */

#include "_TemplateMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define TPL_LOG "[Tpl]"  // 生成器会替换为本游戏标签

// 目标库：注册表模式不依赖 GameConfig.h（那是 legacy 配置中心）。
// Cocos 系生成后把下面 4 处 sec_lib_il2cpp() 换成目标库名即可。
#define TPL_TARGET_LIB sec_lib_il2cpp()

// ============================================================
// ① 开关与 orig（TODO：按游戏增减；功能位号见 .h 的 enum）
// ============================================================
static std::atomic<bool> g_libReady{false};  // il2cpp 已映射（≠ 已 hook）
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_dmg{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_reenter{false};

typedef void (*TakeDamage_t)(void* self, void* damageData, void* method);
static TakeDamage_t g_origTakeDamage = nullptr;

// ============================================================
// ② 字段偏移与 RVA（TODO：dump.cs / IDA / Trace 实测，禁止猜；换包重 dump）
// ============================================================
enum : size_t {
    kOffHurtValue = 0x30,  // 示例：伤害字段偏移
};

// ============================================================
// ③ 安全助手（防闪退核心，勿删；新代码禁止绕开它们裸读写内存）
// ============================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

static void SafeWriteI32(void* obj, size_t off, int32_t v) {
    if (OkPtr(obj)) Il2Cpp::WriteI32(obj, off, v);
}

static void SafeWriteF32(void* obj, size_t off, float v) {
    if (OkPtr(obj)) memcpy(reinterpret_cast<uint8_t*>(obj) + off, &v, sizeof(v));
}

static int32_t SafeReadI32(void* obj, size_t off, int32_t defv) {
    if (!OkPtr(obj)) return defv;
    int32_t v = defv;
    memcpy(&v, reinterpret_cast<uint8_t*>(obj) + off, sizeof(v));
    return v;
}

// 类名缓存：热路径禁止每帧 object_class + class_name 解析
struct ClassNameCache {
    void* klass = nullptr;
    char  name[40] = {};
    bool  valid = false;
};

static const char* CacheClassName(ClassNameCache& cache, void* obj) {
    if (!OkPtr(obj)) return nullptr;
    auto& api = GetApi();
    void* k = api.object_class ? api.object_class(obj) : nullptr;
    if (!k || !api.class_name) return nullptr;
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
    if (!got || !needle) return false;
    for (size_t i = 0; got[i]; i++) {
        size_t j = 0;
        while (needle[j] && got[i + j] == needle[j]) j++;
        if (!needle[j]) return true;
    }
    return false;
}

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
    void* va = FileOffToVa(TPL_TARGET_LIB, rva);
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
    return GetApi().Load(TPL_TARGET_LIB);
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

// 按名路径：类/方法缺失只 log 降级，不整包失败（hook-rules §17）
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
    uint32_t    expectInsn;  // 0 = 不校验序言；建议实测后回填防漂移
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
// ⑤ hook 函数与挂点表（TODO：一个具体方法一个槽；绝不 hook 共享 invoke 入口）
// ============================================================
static void Hooked_TakeDamage(void* self, void* damageData, void* method) {
    if (g_origTakeDamage) g_origTakeDamage(self, damageData, method);  // 铁律：先 orig
    if (g_reenter.load()) return;                                      // 重入保护
    if (!g_dmg.load() && !g_inv.load()) return;                        // 开关全关不动数据
    if (!OkPtr(damageData)) return;                                    // 先校验再写

    g_reenter.store(true);
    SafeWriteI32(damageData, kOffHurtValue, g_dmg.load() ? 999999 : 0);
    g_reenter.store(false);
}

static const HookSlot kSlots[] = {
    // TODO: rva/expectInsn 从 dump.cs/IDA 实测后回填（0 = 只走按名路径）
    {"TakeDamage", 0x0, 0x0, "HotFix.BattleLogic", "BattleCharacter", "TakeDamage", 2,
     (void*)Hooked_TakeDamage, (void**)&g_origTakeDamage},
};

// ============================================================
// ⑥ 分组安装 + 懒装（TODO：按功能组拆；开关全关 = 0 hook）
// ============================================================
static void InstallBattleHooks() {
    for (const auto& s : kSlots) {
        if (HookSlotInstall(s)) g_anyHooked.store(true);
    }
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    if (g_dmg.load() || g_inv.load()) InstallBattleHooks();  // TODO: 更多功能组
}

// ============================================================
// ⑦ 对外接口（ModEntry.h 注册表契约，勿改签名）
// ============================================================
bool Template_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;
    g_libReady.store(true);
    SyncHooks();  // 已 ON 的开关立即装；全关则保持 0 hook
    LOGI(TPL_LOG " lib ready (dual-path, lazy hooks)");
    return true;
}

bool Template_IsReady() { return g_libReady.load(); }

void Template_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case TEMPLATE_FEAT_DMG: g_dmg.store(enabled); break;
        case TEMPLATE_FEAT_INV: g_inv.store(enabled); break;
        default: break;
    }
    SyncHooks();
}

void Template_OnValueChanged(int /*featNum*/, int /*value*/) {}

void Template_Tick(const bool* features, const int* /*values*/) {
    // 只做：同步开关 + 兜底安装。禁止在这里 SafeProbe/扫堆/遍历对象/逐对象写
    if (features) {
        g_dmg.store(features[TEMPLATE_FEAT_DMG]);
        g_inv.store(features[TEMPLATE_FEAT_INV]);
    }
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) g_libReady.store(true);
    SyncHooks();
}

void Template_ForceClear() {
    g_dmg.store(false);
    g_inv.store(false);
    // TODO: 其他功能开关一并复位
}
