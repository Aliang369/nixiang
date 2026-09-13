/* ================================================================
 *  星光菜单游戏模块 — 大话蜀山 (com.cylj.rogue.tp479 / TapTap 2.5)
 *
 *  功能（复刻斌哥改包，见 10_docs/大话蜀山/斌哥包抓点_2.5_20260913.md）：
 *    featNum=1「秒杀」 ← 斌哥「百倍伤害」：怪物目标伤害 ×100
 *    featNum=2「无敌」 ← 斌哥「玩家无敌」：玩家侧目标受伤置 0
 *
 *  ------------------------------------------------------------------
 *  双副本事实（2026-09-14 真机 /proc/pid/maps 实测）：
 *    /data/data/<pkg>/files/libil2cpp.so        118MB ← 壳解密后的真身，游戏跑这份
 *    /data/app/~~…/lib/arm64/libil2cpp.so       117MB ← APK 里那份（宿主/诱饵）
 *  后果（v1/v2 两次踩坑，均已实证）：
 *    v1: dlsym("libil2cpp.so") 命中 APK 诱饵副本 → 调 il2cpp API 即 Class::Init(NULL) 崩
 *    v2: 静态 RVA 是 APK 副本坐标系 → 在 files/ 副本上序言不符（拒绝安装，功能不生效）
 *  v3 对策：**按路径精确 dlopen 真身副本**，从该 handle 解析符号；只用真身副本的
 *  il2cpp API 取 MethodInfo→methodPointer 挂点（不再需要 RVA）。
 *
 *  安全设计（异常兜底防闪退）：
 *    1. 只挂真身：路径含 "/files/libil2cpp.so" 优先；找不到则不挂（宁可不生效）
 *    2. 运行时就绪探测只用 SafeProbe 包住的两次只读调用；失败即退避重试，不硬闯
 *    3. lib 就绪后先等 kArmDelayMs 才尝试；重试有上限，不无限循环
 *    4. 挂点前校验目标落在可执行映射内；orig 必调；同线程重入保护
 *    5. 伤害钩子只在「ServerStep 已跑过 && isSingle==1」的单机战斗里安装
 * ================================================================ */

#include "DhssMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <ctime>
#include <dlfcn.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"  // IsValidHeapPtr / SafeProbe

#define DHSS_LOG "[Dhss]"
#define DHSS_TARGET_LIB sec_lib_il2cpp()

// ============================================================
// ① 开关 / orig / 状态
// ============================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_kill{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_noads{false};      // feat 3 免广告
static thread_local bool g_inHook = false;

static std::atomic<bool> g_stepHooked{false};
static std::atomic<bool> g_stepFailed{false};
static std::atomic<bool> g_dmgHooked{false};
static std::atomic<bool> g_adHooked{false};
static std::atomic<bool> g_adFailed{false};
static std::atomic<bool> g_dmgFailed{false};

static std::atomic<bool> g_battleSeen{false};
static std::atomic<int32_t> g_isSingle{-1};
static std::atomic<uint64_t> g_lastStepMs{0};
static std::atomic<uint32_t> g_stepCount{0};
static std::atomic<uint32_t> g_applySeen{0};
static std::atomic<uint32_t> g_scaled{0};
static std::atomic<uint32_t> g_blocked{0};
static std::atomic<uint64_t> g_armAtMs{0};
static std::atomic<int32_t> g_retry{0};

typedef void (*AdNotify_t)(void* cmd, void* method);   // 静态方法无 self
static AdNotify_t g_origAdNotify = nullptr;

typedef bool (*ServerStep_t)(void* self, int64_t deltaTimeFp, void* method);
static ServerStep_t g_origServerStep = nullptr;

typedef void (*ApplyDamage_t)(void* self, int32_t damage, void* other, bool isCrit,
                              void* skillCode, int32_t magicType, void* method);
static ApplyDamage_t g_origApplyDamage = nullptr;

// ============================================================
// ② 字段偏移与门控常量（dump.cs 实测）
// ============================================================
enum : size_t {
    kOffIsSingle = 0x10,   // BattleRoom.isSingle
    kOffCamp     = 0x670,  // AnimalEntity.Camp（0=Monster，1..20=玩家侧）
};
enum : int32_t {
    kCampMonster    = 0,
    kKillMultiplier = 100,
};
enum : uint64_t {
    kBattleTimeoutMs = 2000,   // ServerStep 2 秒没动 = 不在战斗
    kArmDelayMs      = 10000,  // lib 就绪后先等 10s 再尝试挂点（避开半初始化）
    kRetryBackoffMs  = 5000,   // 失败退避
};
enum : int32_t {
    kRetryMax = 12,            // 最多重试 12 次（约 1 分钟）
};

// ============================================================
// ③ 安全助手
// ============================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

static uint64_t NowMs() {
    struct timespec ts;
    if (clock_gettime(CLOCK_MONOTONIC, &ts) != 0) return 0;
    return static_cast<uint64_t>(ts.tv_sec) * 1000ULL + static_cast<uint64_t>(ts.tv_nsec / 1000000L);
}

static int32_t ReadI32Safe(void* obj, size_t off, int32_t defv) {
    if (!OkPtr(obj)) return defv;
    int32_t v = defv;
    memcpy(&v, reinterpret_cast<uint8_t*>(obj) + off, sizeof(v));
    return v;
}

static uint8_t ReadU8Safe(void* obj, size_t off, uint8_t defv) {
    if (!OkPtr(obj)) return defv;
    uint8_t v = defv;
    memcpy(&v, reinterpret_cast<uint8_t*>(obj) + off, sizeof(v));
    return v;
}

static int32_t ScaleDamage(int32_t damage) {
    if (damage <= 0) return damage;
    if (damage > INT32_MAX / kKillMultiplier) return INT32_MAX;
    return damage * kKillMultiplier;
}

static bool BattleGateOpen() {
    if (g_isSingle.load(std::memory_order_relaxed) != 1) return false;
    const uint64_t last = g_lastStepMs.load(std::memory_order_relaxed);
    if (last == 0) return false;
    const uint64_t now = NowMs();
    if (now == 0 || now < last) return false;
    return (now - last) <= kBattleTimeoutMs;
}

// ============================================================
// ④ 真身副本定位（按路径精确 dlopen；不用 dlsym("libil2cpp.so")）
// ============================================================
static bool PathIsLiveLib(const char* p) {
    return p && strstr(p, "libil2cpp.so") && strstr(p, "/files/");
}

// 从 /proc/self/maps 收集 libil2cpp 的唯一路径；优先 files/ 真身
static int CollectLibPaths(char out[][512], int maxPaths) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return 0;
    char line[768];
    int n = 0;
    char fallback[512] = {0};
    while (fgets(line, sizeof(line), fp)) {
        char* p = strstr(line, "/");
        if (!p || !strstr(line, "libil2cpp.so")) continue;
        char path[512] = {0};
        size_t i = 0;
        for (; p[i] && p[i] != '\n' && i + 1 < sizeof(path); i++) path[i] = p[i];
        path[i] = 0;
        if (PathIsLiveLib(path)) {
            bool dup = false;
            for (int k = 0; k < n; k++) if (strcmp(out[k], path) == 0) dup = true;
            if (!dup && n < maxPaths) {
                snprintf(out[n], 512, "%s", path);
                n++;
            }
        } else if (!fallback[0]) {
            snprintf(fallback, sizeof(fallback), "%s", path);
        }
    }
    fclose(fp);
    if (n == 0 && fallback[0] && maxPaths > 0) {
        snprintf(out[0], 512, "%s", fallback);
        n = 1;
    }
    return n;
}

// 地址是否落在可执行映射内；顺带回报所在映射路径与 RVA（诊断用）
static bool AddrInExecMap(void* addr, char* pathOut, size_t pathLen, uintptr_t* rvaOut) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return false;
    char line[768];
    uintptr_t a = reinterpret_cast<uintptr_t>(addr);
    bool found = false;
    while (fgets(line, sizeof(line), fp)) {
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x')) continue;
        if (a < start || a >= end) continue;
        found = true;
        if (pathOut && pathLen) {
            const char* p = strstr(line, "/");
            snprintf(pathOut, pathLen, "%s", p ? p : "?");
            char* nl = strchr(pathOut, '\n');
            if (nl) *nl = 0;
        }
        if (rvaOut) *rvaOut = static_cast<uintptr_t>(a - start + off);
        break;
    }
    fclose(fp);
    return found;
}

// ============================================================
// ⑤ 真身副本的 il2cpp API（只用到 5 个只读函数）
// ============================================================
typedef void* (*domain_get_t)();
typedef void** (*domain_get_assemblies_t)(void*, size_t*);
typedef void* (*assembly_get_image_t)(const void*);
typedef void* (*class_from_name_t)(void*, const char*, const char*);
typedef void* (*class_get_method_from_name_t)(void*, const char*, int);
typedef void* (*class_get_field_from_name_t)(void*, const char*);
typedef void (*field_static_set_value_t)(void*, void*);

struct RtApi {
    void* handle = nullptr;
    domain_get_t domain_get = nullptr;
    domain_get_assemblies_t domain_assemblies = nullptr;
    assembly_get_image_t assembly_image = nullptr;
    class_from_name_t class_from_name = nullptr;
    class_get_method_from_name_t class_get_method = nullptr;
    class_get_field_from_name_t class_get_field = nullptr;
    field_static_set_value_t field_static_set = nullptr;
    void* domain = nullptr;
    bool ready = false;
};
static RtApi g_api;

static bool LoadRtApi() {
    if (g_api.ready) return true;
    char paths[4][512];
    const int n = CollectLibPaths(paths, 4);
    if (n <= 0) return false;
    void* h = nullptr;
    for (int i = 0; i < n && !h; i++) {
        h = dlopen(paths[i], RTLD_NOW | RTLD_NOLOAD);
        if (!h) h = dlopen(paths[i], RTLD_NOW);
        if (h) LOGI(DHSS_LOG " dlopen 真身副本: %s", paths[i]);
    }
    if (!h) { LOGW(DHSS_LOG " 未能 dlopen 任何 libil2cpp 副本"); return false; }

    g_api.handle = h;
    g_api.domain_get = (domain_get_t)dlsym(h, "il2cpp_domain_get");
    g_api.domain_assemblies = (domain_get_assemblies_t)dlsym(h, "il2cpp_domain_get_assemblies");
    g_api.assembly_image = (assembly_get_image_t)dlsym(h, "il2cpp_assembly_get_image");
    g_api.class_from_name = (class_from_name_t)dlsym(h, "il2cpp_class_from_name");
    g_api.class_get_method = (class_get_method_from_name_t)dlsym(h, "il2cpp_class_get_method_from_name");
    // 免广告用（缺失则只降级该功能，不影响战斗两功能）
    g_api.class_get_field = (class_get_field_from_name_t)dlsym(h, "il2cpp_class_get_field_from_name");
    g_api.field_static_set = (field_static_set_value_t)dlsym(h, "il2cpp_field_static_set_value");
    if (!g_api.domain_get || !g_api.domain_assemblies || !g_api.assembly_image ||
        !g_api.class_from_name || !g_api.class_get_method) {
        LOGW(DHSS_LOG " 真身副本缺 il2cpp 导出符号，放弃挂点");
        return false;
    }
    // 运行时就绪探测：只读两次调用，SafeProbe 兜住极端情况
    void* domain = nullptr;
    bool ok = Il2Cpp::SafeProbe([&]() -> bool {
        domain = g_api.domain_get();
        return domain != nullptr;
    });
    if (!ok || !domain) {
        LOGW(DHSS_LOG " 真身副本 il2cpp domain 未就绪（跳过本轮）");
        return false;
    }
    size_t cnt = 0;
    void** asms = nullptr;
    ok = Il2Cpp::SafeProbe([&]() -> bool {
        asms = g_api.domain_assemblies(domain, &cnt);
        return asms != nullptr && cnt > 0;
    });
    if (!ok || !asms || cnt == 0) {
        LOGW(DHSS_LOG " 真身副本 assemblies 为空（跳过本轮）");
        return false;
    }
    g_api.domain = domain;
    g_api.ready = true;
    LOGI(DHSS_LOG " il2cpp API 就绪（真身副本）：assemblies=%zu", cnt);
    return true;
}

static void* FindClassInDomain(const char* ns, const char* name) {
    if (!g_api.ready) return nullptr;
    size_t cnt = 0;
    void** asms = g_api.domain_assemblies(g_api.domain, &cnt);
    if (!asms) return nullptr;
    for (size_t i = 0; i < cnt; i++) {
        void* img = g_api.assembly_image(asms[i]);
        if (!img) continue;
        void* k = g_api.class_from_name(img, ns, name);
        if (k) return k;
    }
    return nullptr;
}

static void* ResolveMethodPtr(const char* ns, const char* cls, const char* method, int argc) {
    void* k = FindClassInDomain(ns, cls);
    if (!k) {
        LOGW(DHSS_LOG " 类未找到 %s.%s（真身副本）", ns, cls);
        return nullptr;
    }
    void* mi = g_api.class_get_method(k, method, argc);
    if (!mi) {
        LOGW(DHSS_LOG " 方法未找到 %s.%s/%d", cls, method, argc);
        return nullptr;
    }
    void* fn = *reinterpret_cast<void**>(mi);
    if (!fn) {
        LOGW(DHSS_LOG " %s.%s methodPointer 为空", cls, method);
        return nullptr;
    }
    return fn;
}

static bool HookResolved(const char* tag, void* fn, void* hook, void** orig) {
    char path[512] = {0};
    uintptr_t rva = 0;
    if (!AddrInExecMap(fn, path, sizeof(path), &rva)) {
        LOGW(DHSS_LOG " %s 目标 %p 不在可执行映射 — 拒绝安装", tag, fn);
        return false;
    }
    if (DobbyHook(fn, hook, orig) != 0) {
        LOGW(DHSS_LOG " %s DobbyHook 失败 @%p", tag, fn);
        return false;
    }
    LOGI(DHSS_LOG " %s 已挂 @%p（副本=%s rva=0x%lx）", tag, fn, path, (unsigned long)rva);
    return true;
}

// ============================================================
// ⑥ hook 实现
// ============================================================
// ---- 免广告：钩 GamePlatformManager.OnGameAdNotify（服务端广告响应缓存点）----
static std::atomic<void*> g_adAvoidField{nullptr};
static std::atomic<uint32_t> g_adCount{0};

// 官方机制：GameAdRsp.avoidAd=true 时客户端跳过 SDK 播放直接完成。
// 这里在响应被缓存后把静态字段 requestADAvoid 置真（用 il2cpp 反射设字段，不赌偏移）。
static void ForceAdAvoid() {
    void* field = g_adAvoidField.load(std::memory_order_relaxed);
    if (!field) {
        if (!g_api.class_get_field || !g_api.ready) return;
        void* k = FindClassInDomain("Cylj.Game", "GamePlatformManager");
        if (!k) return;
        field = g_api.class_get_field(k, "requestADAvoid");
        if (!field) {
            LOGW(DHSS_LOG " noads: 未找到字段 requestADAvoid（降级）");
            return;
        }
        g_adAvoidField.store(field, std::memory_order_relaxed);
    }
    if (!g_api.field_static_set) return;
    bool v = true;
    Il2Cpp::SafeProbe([&]() -> bool { g_api.field_static_set(field, &v); return true; });
}

static void Hooked_OnGameAdNotify(void* cmd, void* method) {
    if (g_origAdNotify) g_origAdNotify(cmd, method);  // 先让原逻辑缓存 adId/args/avoidAd
    if (!g_noads.load(std::memory_order_relaxed)) return;
    if (!OkPtr(cmd)) return;
    const bool serverAvoid = ReadU8Safe(cmd, 0x30, 0) != 0;   // GameAdRsp.avoidAd
    ForceAdAvoid();
    const uint32_t n = g_adCount.fetch_add(1, std::memory_order_relaxed);
    if (n < 5) {
        LOGI(DHSS_LOG " noads: 已强制 avoidAd=true（服务端原值=%d，第%u次）",
             serverAvoid ? 1 : 0, n + 1);
    }
}

static bool Hooked_ServerStep(void* self, int64_t deltaTimeFp, void* method) {
    if (!g_origServerStep) return false;
    const uint32_t idx = g_stepCount.fetch_add(1, std::memory_order_relaxed);
    if (OkPtr(self)) {
        const uint8_t flag = ReadU8Safe(self, kOffIsSingle, 0);
        const int32_t now = flag ? 1 : 0;
        const int32_t prev = g_isSingle.exchange(now, std::memory_order_relaxed);
        g_lastStepMs.store(NowMs(), std::memory_order_relaxed);
        if (idx == 0 || prev != now) {
            g_battleSeen.store(true, std::memory_order_relaxed);
            LOGI(DHSS_LOG " gate ServerStep %s isSingle=%d (%s)",
                 idx == 0 ? "first-call" : "changed", now,
                 now == 1 ? "single-player, features active" : "online mode, features stay off");
        }
    }
    return g_origServerStep(self, deltaTimeFp, method);
}

static void Hooked_ApplyDamage(void* self, int32_t damage, void* other, bool isCrit,
                               void* skillCode, int32_t magicType, void* method) {
    if (!g_origApplyDamage) return;

    const bool kill = g_kill.load(std::memory_order_relaxed);
    const bool inv = g_inv.load(std::memory_order_relaxed);
    if ((!kill && !inv) || g_inHook) {
        g_origApplyDamage(self, damage, other, isCrit, skillCode, magicType, method);
        return;
    }

    const uint32_t idx = g_applySeen.fetch_add(1, std::memory_order_relaxed);
    const bool gate = BattleGateOpen();
    int32_t camp = -1;
    int32_t dmg = damage;

    if (gate && OkPtr(self)) {
        camp = ReadI32Safe(self, kOffCamp, -1);
        if (camp == kCampMonster) {
            if (kill) {
                dmg = ScaleDamage(damage);
                if (dmg != damage) g_scaled.fetch_add(1, std::memory_order_relaxed);
            }
        } else if (camp > kCampMonster) {
            if (inv) {
                if (damage != 0) g_blocked.fetch_add(1, std::memory_order_relaxed);
                dmg = 0;
            }
        }
    }

    if (idx < 3) {
        LOGI(DHSS_LOG " apply#%u victim-camp=%d dmg=%d->%d gate=%d(single=%d) kill=%d inv=%d",
             idx + 1, camp, damage, dmg, gate ? 1 : 0,
             g_isSingle.load(std::memory_order_relaxed), kill ? 1 : 0, inv ? 1 : 0);
    }

    g_inHook = true;
    g_origApplyDamage(self, dmg, other, isCrit, skillCode, magicType, method);
    g_inHook = false;
}

// ============================================================
// ⑦ 懒装（后台 Tick 线程；带就绪等待与有界退避）
// ============================================================
static bool ArmDue() {
    const uint64_t armed = g_armAtMs.load(std::memory_order_relaxed);
    if (armed == 0) return false;
    const uint64_t now = NowMs();
    if (now == 0 || now < armed) return false;
    return (now - armed) >= kArmDelayMs;
}

static void EnsureHooks() {
    if (!g_libReady.load()) return;
    const bool wantBattle = g_kill.load() || g_inv.load();
    const bool wantAd = g_noads.load();
    if (!wantBattle && !wantAd) return;
    if (!ArmDue()) return;
    const bool battleSettled = (!wantBattle) || g_stepFailed.load() ||
                               (g_stepHooked.load() && (g_dmgHooked.load() || g_dmgFailed.load()));
    const bool adSettled = (!wantAd) || g_adHooked.load() || g_adFailed.load();
    if (battleSettled && adSettled) return;

    if (g_retry.load() >= kRetryMax) return;
    if (!g_api.ready && !LoadRtApi()) {
        g_retry.fetch_add(1, std::memory_order_relaxed);
        g_armAtMs.store(NowMs() + kRetryBackoffMs, std::memory_order_relaxed);
        return;
    }

    // 免广告：广告在 UI 阶段就发生，不依赖战斗门控
    if (wantAd && !g_adHooked.load() && !g_adFailed.load()) {
        void* fn = ResolveMethodPtr("Cylj.Game", "GamePlatformManager", "OnGameAdNotify", 1);
        if (fn) {
            if (HookResolved("OnGameAdNotify", fn, (void*)Hooked_OnGameAdNotify, (void**)&g_origAdNotify))
                g_adHooked.store(true);
            else
                g_adFailed.store(true);
        } else {
            g_adFailed.store(true);
        }
    }

    // 步进钩子：只读 isSingle 与时间戳，不改业务数据
    if (wantBattle && !g_stepHooked.load() && !g_stepFailed.load()) {
        void* fn = ResolveMethodPtr("Cylj.Game", "BattleRoom", "ServerStep", 1);
        if (fn) {
            if (HookResolved("ServerStep", fn, (void*)Hooked_ServerStep, (void**)&g_origServerStep))
                g_stepHooked.store(true);
            else
                g_stepFailed.store(true);
        } else {
            g_stepFailed.store(true);
        }
    }

    // 伤害钩子：只在「ServerStep 跑过 && 当前 isSingle==1」的单机战斗里挂
    if (g_stepHooked.load() && !g_dmgHooked.load() && !g_dmgFailed.load() &&
        g_battleSeen.load() && g_isSingle.load() == 1) {
        void* fn = ResolveMethodPtr("Cylj.LockStep", "AnimalEntity", "ApplyDamage", 5);
        if (fn) {
            if (HookResolved("ApplyDamage", fn, (void*)Hooked_ApplyDamage, (void**)&g_origApplyDamage))
                g_dmgHooked.store(true);
            else
                g_dmgFailed.store(true);
        } else {
            g_dmgFailed.store(true);
        }
    }
}

// ============================================================
// ⑧ 对外接口（ModEntry.h 注册表契约）
// ============================================================
bool Dhss_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(DHSS_TARGET_LIB)) return false;
    g_libReady.store(true);
    g_armAtMs.store(NowMs() + kArmDelayMs, std::memory_order_relaxed);
    LOGI(DHSS_LOG " libil2cpp 已映射（真身副本定位 / 后台懒装 / isSingle 门控）");
    return true;
}

bool Dhss_IsReady() { return g_libReady.load(); }

void Dhss_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case DHSS_FEAT_KILL: g_kill.store(enabled); break;
        case DHSS_FEAT_INV:  g_inv.store(enabled);  break;
        case DHSS_FEAT_AD:   g_noads.store(enabled); break;
        default: return;
    }
    const char* name = featNum == DHSS_FEAT_KILL ? "kill"
                     : featNum == DHSS_FEAT_INV ? "invincible" : "no-ads";
    LOGI(DHSS_LOG " feat=%d %s -> %d (step=%d dmg=%d ad=%d scaled=%u blocked=%u)",
         featNum, name, enabled ? 1 : 0,
         g_stepHooked.load() ? 1 : 0, g_dmgHooked.load() ? 1 : 0,
         g_adHooked.load() ? 1 : 0,
         g_scaled.load(std::memory_order_relaxed), g_blocked.load(std::memory_order_relaxed));
}

void Dhss_OnValueChanged(int /*featNum*/, int /*value*/) {}

void Dhss_Tick(const bool* features, const int* /*values*/) {
    // 只做：同步开关 + 懒装 + 观测；禁止扫堆/遍历对象/逐对象写
    if (features) {
        g_kill.store(features[DHSS_FEAT_KILL]);
        g_inv.store(features[DHSS_FEAT_INV]);
        g_noads.store(features[DHSS_FEAT_AD]);
    }
    if (!g_libReady.load() && isLibraryLoaded(DHSS_TARGET_LIB)) {
        g_libReady.store(true);
        g_armAtMs.store(NowMs() + kArmDelayMs, std::memory_order_relaxed);
    }
    EnsureHooks();
}

void Dhss_ForceClear() {
    g_kill.store(false);
    g_inv.store(false);
    g_noads.store(false);
    g_isSingle.store(-1);
    g_lastStepMs.store(0);
}
