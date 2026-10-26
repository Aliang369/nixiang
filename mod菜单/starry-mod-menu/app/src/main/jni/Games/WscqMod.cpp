/* ================================================================
 *  星光菜单游戏模块 — 圣翼传说（com.aiyue.wscq）功能实现
 *
 *  ⚠️ 防闪退设施已内置为代码，删任何一节都可能复现历史闪退：
 *    默认 0 hook（懒装）/ 双路径挂点 / RVA 漂移拒绝 / 幂等安装
 *    orig 必调 / reenter 防递归 / 类名缓存 / Tick 轻量
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 *
 *  功能清单（下标 = WscqMod.h 的 enum）：
 *    1 伤害倍率（滑块 1..1000，1=关）  → Enemy::GetHurt 入口乘倍率
 *    2 玩家无敌                        → PlayerCtr::GetHurt 负伤改 0
 *    3 全屏攻击                        → EnemyCtr2::FixedUpdate 登记活体
 *                                        + Enemy::GetHurt 扩散同一份伤害
 *
 *  锚点（file_off，原版 libil2cpp.so md5 563b40a81370833858ef2d4452f8dc3f，
 *        与斌哥改包同文件逐字节一致 → 15 处 RVA 可直接沿用）：
 *    10_docs/圣翼传说/斌哥hook锚点_20260924.json
 *    va = file_off + 0x4000（本 so 实测：vaddr = file + 0x4000）
 *
 *  20261026 用户实测后收窄：删除战斗加速、怪物/BOSS瞬刷及其反作弊组；
 *  只保留上述 3 项。锚点档案可以保留历史超集，本模块只安装实际使用槽。
 * ================================================================ */

#include "WscqMod.h"

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

#define TPL_LOG "[wscq]"  // 生成器会替换为本游戏标签

// 目标库：注册表模式不依赖 GameConfig.h（那是 legacy 配置中心）。
#define TPL_TARGET_LIB sec_lib_il2cpp()

// ============================================================
// ① 开关与 orig
// ============================================================
static std::atomic<bool> g_libReady{false};  // il2cpp 已映射（≠ 已 hook）
static std::atomic<bool> g_anyHooked{false};

// 功能开关（Tick 只同步这几个原子，禁重活）
static std::atomic<int>  g_dmgMul{1};    // 1 = 关；1000 = 百倍以上
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_aoe{false};

// 运行期辅助状态
static std::atomic<bool> g_aoeReenter{false};     // AOE 扩散防递归

// orig
typedef void (*EnemyGetHurt_t)(void* self, int64_t blood, uint8_t is吸血, uint8_t is真伤,
                               uint8_t is暴击, void* fx, void* mi);
typedef void (*PlayerGetHurt_t)(void* self, float hurt, void* color, void* mi);
typedef void (*VoidSelf_t)(void* self, void* mi);

static EnemyGetHurt_t  g_origEnemyGetHurt  = nullptr;
static PlayerGetHurt_t g_origPlayerGetHurt = nullptr;
static VoidSelf_t      g_origPlayerFixed   = nullptr;
static VoidSelf_t      g_origEnemyCtr2Fix  = nullptr;

// ============================================================
// ② 字段偏移与 file_off（实测，禁止猜；换包必须重新抓点）
// ============================================================
enum : uintptr_t {
    kFoEnemyGetHurt      = 0x13240B4,
    kFoPlayerGetHurt     = 0x13C82C8,
    kFoPlayerFixedUpdate = 0x13CA180,
    kFoEnemyCtr2Fixed    = 0x138A280,
};

// 序言首 4 字节（小端）= 原版磁盘 so 实测值，防版本漂移挂错地址
enum : uint32_t {
    kInsEnemyGetHurt  = 0xFC170FEC,
    kInsPlayerGetHurt = 0x6DB92BEB,
    kInsPlayerFixed   = 0x6DBB23E9,
    kInsEnemyCtr2Fix  = 0x6DBA33ED,
};

// ============================================================
// ③ 安全助手（防闪退核心，勿删；新代码禁止绕开它们裸读写内存）
// ============================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

static int ClampI(int v, int lo, int hi) { return v < lo ? lo : (v > hi ? hi : v); }

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
    uintptr_t   rva;         // = libil2cpp FILE OFFSET（0 = 只走按名）
    uint32_t    expectInsn;  // 0 = 不校验序言；实测后回填防漂移
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
// ⑤ 全屏攻击：活体登记表（自持，不依赖壳 BSS；主线程访问）
// ============================================================
enum : int { kAoeCap = 256, kAoeHitsMax = 0x3F };

struct AoeEntry {
    void*    ptr;
    uint32_t stamp;
};

static AoeEntry        g_aoeReg[kAoeCap];
static std::atomic<uint32_t> g_aoeCount{0};
static std::atomic<uint32_t> g_frameTick{0};  // 帧序（PlayerCtr::FixedUpdate 每帧 +1）

// 登记：命中刷新时间戳；未命中且未满则追加；满则 LRU 替换最旧
static void AoeTouch(void* self) {
    if (!self) return;
    const uint32_t n = g_aoeCount.load();
    const uint32_t t = g_frameTick.load();
    for (uint32_t i = 0; i < n; i++) {
        if (g_aoeReg[i].ptr == self) {
            g_aoeReg[i].stamp = t;
            return;
        }
    }
    if (n < static_cast<uint32_t>(kAoeCap)) {
        g_aoeReg[n].ptr = self;
        g_aoeReg[n].stamp = t;
        g_aoeCount.store(n + 1);
        return;
    }
    uint32_t oldest = 0;
    for (uint32_t i = 1; i < n; i++) {
        if (g_aoeReg[i].stamp < g_aoeReg[oldest].stamp) oldest = i;
    }
    g_aoeReg[oldest].ptr = self;
    g_aoeReg[oldest].stamp = t;
}

// 扩散：对最近 1 帧内活跃过的其它敌人补同一份伤害（上限 63 个 + 防递归）
static void AoeSpread(void* self, int64_t dmg, uint8_t a, uint8_t b, uint8_t c, void* fx, void* mi) {
    if (!g_origEnemyGetHurt) return;
    bool expect = false;
    if (!g_aoeReenter.compare_exchange_strong(expect, true)) return;  // 已在扩散中
    const uint32_t n = g_aoeCount.load();
    const uint32_t t = g_frameTick.load();
    int hits = 0;
    for (uint32_t i = 0; i < n && hits <= kAoeHitsMax; i++) {
        void* e = g_aoeReg[i].ptr;
        if (!e || e == self) continue;
        if (t - g_aoeReg[i].stamp > 1) continue;   // 2 帧未活跃 = 已死/离场
        if (!Il2Cpp::IsValidHeapPtr(e)) continue;
        g_origEnemyGetHurt(e, dmg, a, b, c, fx, mi);
        hits++;
    }
    g_aoeReenter.store(false);
}

// ============================================================
// ⑥ hook 函数与挂点表（一个具体方法一个槽；绝不 hook 共享 invoke 入口）
// ============================================================

// 伤害倍率 + 全屏攻击：#9 Enemy::GetHurt
static void Hooked_EnemyGetHurt(void* self, int64_t blood, uint8_t is吸血, uint8_t is真伤,
                                uint8_t is暴击, void* fx, void* mi) {
    if (!g_origEnemyGetHurt) return;
    const int mul = g_dmgMul.load();
    int64_t dmg = blood;
    if (mul > 1 && blood > 0) dmg = blood * static_cast<int64_t>(mul);  // 只放大正向伤害
    g_origEnemyGetHurt(self, dmg, is吸血, is真伤, is暴击, fx, mi);
    if (dmg >= 1 && g_aoe.load() && !g_aoeReenter.load()) {
        AoeSpread(self, dmg, is吸血, is真伤, is暴击, fx, mi);
    }
}

// 玩家无敌：#11 PlayerCtr::GetHurt（本作负伤害 = 扣血，直接归零）
static void Hooked_PlayerGetHurt(void* self, float hurt, void* color, void* mi) {
    if (!g_origPlayerGetHurt) return;
    if (g_inv.load() && hurt < 0.0f) hurt = 0.0f;
    g_origPlayerGetHurt(self, hurt, color, mi);
}

// 帧序：#12 PlayerCtr::FixedUpdate —— 仅供全屏攻击判断敌人是否仍在最近帧活跃
static void Hooked_PlayerFixedUpdate(void* self, void* mi) {
    g_frameTick.fetch_add(1);
    if (g_origPlayerFixed) g_origPlayerFixed(self, mi);
}

// 全屏攻击：#10 EnemyCtr2::FixedUpdate —— 无条件登记活体（桶 #10 同款）
static void Hooked_EnemyCtr2FixedUpdate(void* self, void* mi) {
    if (g_aoe.load() && OkPtr(self)) AoeTouch(self);
    if (g_origEnemyCtr2Fix) g_origEnemyCtr2Fix(self, mi);
}

static const HookSlot kSlots[] = {
    {"EnemyGetHurt",   kFoEnemyGetHurt,      kInsEnemyGetHurt,  "", "Enemy", "GetHurt", 5,
     (void*)Hooked_EnemyGetHurt, (void**)&g_origEnemyGetHurt},
    {"PlayerGetHurt",  kFoPlayerGetHurt,     kInsPlayerGetHurt, "", "PlayerCtr", "GetHurt", 2,
     (void*)Hooked_PlayerGetHurt, (void**)&g_origPlayerGetHurt},
    {"PlayerFixed",    kFoPlayerFixedUpdate, kInsPlayerFixed,   "", "PlayerCtr", "FixedUpdate", 0,
     (void*)Hooked_PlayerFixedUpdate, (void**)&g_origPlayerFixed},
    {"EnemyCtr2Fixed", kFoEnemyCtr2Fixed,    kInsEnemyCtr2Fix,  "", "EnemyCtr2", "FixedUpdate", 0,
     (void*)Hooked_EnemyCtr2FixedUpdate, (void**)&g_origEnemyCtr2Fix},
};

enum : int {
    kSlotEnemyHurt = 0, kSlotPlayerHurt, kSlotPlayerFixed, kSlotEnemyCtr2Fixed,
};

// ============================================================
// ⑦ 分组安装 + 懒装（开关全关 = 0 hook）
// ============================================================
static bool InstallSlot(int i) {
    const bool ok = HookSlotInstall(kSlots[i]);
    if (ok) g_anyHooked.store(true);
    return ok;
}

static void SyncHooks() {
    if (!g_libReady.load()) return;

    const bool inv = g_inv.load();
    const bool aoe = g_aoe.load();
    const bool dmg = g_dmgMul.load() > 1;

    if (dmg || aoe) InstallSlot(kSlotEnemyHurt);          // 倍率 / 全屏攻击共用入口
    if (inv)        InstallSlot(kSlotPlayerHurt);
    if (aoe) {
        InstallSlot(kSlotPlayerFixed);                     // AOE 活跃判据
        InstallSlot(kSlotEnemyCtr2Fixed);
    }
}

// ============================================================
// ⑧ 对外接口（ModEntry.h 注册表契约，勿改签名）
// ============================================================
static void SyncAllFromMenu(const bool* features, const int* values) {
    if (features) {
        g_inv.store(features[WSCQ_FEAT_INV]);
        g_aoe.store(features[WSCQ_FEAT_AOE]);
    }
    if (values) {
        const int m = values[WSCQ_FEAT_DMG];
        if (m > 0) g_dmgMul.store(ClampI(m, 1, 1000));
    }
}

bool Wscq_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;
#ifdef WSCQ_SMOKE_ALL_ON
    // ★ 仅本机冒烟测试用（Android_wscq.mk 的 ifdef WSCQ_SMOKE_ALL_ON 开，正式包永不含）：
    // 绕过菜单/卡密把 3 个功能强制 ON，用于验证实际 4 处 Dobby 挂点安装后不闪退。
    static bool smokeDone = false;
    if (!smokeDone) {
        smokeDone = true;
        g_dmgMul.store(101);
        g_inv.store(true);
        g_aoe.store(true);
        LOGW(TPL_LOG " SMOKE ALL-ON（冒烟测试构建，勿发布）");
    }
#endif
    g_libReady.store(true);
    SyncHooks();  // 已 ON 的开关立即装；全关则保持 0 hook
    LOGI(TPL_LOG " lib ready (dual-path, lazy hooks)");
    return true;
}

bool Wscq_IsReady() { return g_libReady.load(); }

void Wscq_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case WSCQ_FEAT_INV:     g_inv.store(enabled); break;
        case WSCQ_FEAT_AOE:     g_aoe.store(enabled); break;
        default: break;
    }
    SyncHooks();
}

// 滑块：菜单初始化会为每个 SeekBar 回灌持久化值，这里同步落状态
void Wscq_OnValueChanged(int featNum, int value) {
    switch (featNum) {
        case WSCQ_FEAT_DMG:   g_dmgMul.store(ClampI(value, 1, 1000)); break;
        default: break;
    }
    SyncHooks();
}

void Wscq_Tick(const bool* features, const int* values) {
    // 只做：同步开关/滑块 + 兜底安装。禁止在这里 SafeProbe/扫堆/遍历对象/逐对象写
    SyncAllFromMenu(features, values);
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) {
        g_libReady.store(true);
        LOGI(TPL_LOG " lib ready (tick)");
    }
    SyncHooks();
}

void Wscq_ForceClear() {
    g_dmgMul.store(1);
    g_inv.store(false);
    g_aoe.store(false);
    g_aoeCount.store(0);
}
