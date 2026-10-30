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

#include "YnttMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <ctime>
#include <mutex>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define TPL_LOG "[yntt]"  // 生成器会替换为本游戏标签

// 目标库：注册表模式不依赖 GameConfig.h（那是 legacy 配置中心）。
// Cocos 系生成后把下面 4 处 sec_lib_il2cpp() 换成目标库名即可。
#define TPL_TARGET_LIB sec_lib_il2cpp()

// ============================================================
// ① 开关与 orig（TODO：按游戏增减；功能位号见 .h 的 enum）
// ============================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_kill{false};
static std::atomic<bool> g_inv{false};
static std::atomic<int> g_speed{1};  // Slider: 1=off, 2..10=multiplier
static std::atomic<bool> g_stamina{false};
static std::atomic<bool> g_currency{false};
static std::atomic<bool> g_noads{false};
static std::atomic<int64_t> g_lastInstallMs{0};
static std::mutex g_installMutex;

static thread_local bool t_inCombatHook = false;
static thread_local bool t_inShowAd = false;

// 739: monsterStatus.ReduceHP 在 728 的 bool isAreaDamge 后新增 bool zhanlijue；
// NpcStatus.ReduceHP 在 728 的两参后新增 bool zhanlijue。两者仍不能共用函数指针类型。
typedef void (*MonsterReduceHp_t)(void* self, void* skillCfg, double damage, bool isAreaDamage, bool zhanlijue, void* method);
typedef void (*NpcReduceHp_t)(void* self, void* skillCfg, double damage, bool zhanlijue, void* method);
typedef void (*OnDamageBuff_t)(void* self, int32_t buffId, int32_t overLayer, void* method);
typedef void (*PlayerOnDamage_t)(void* self, int64_t hate, void* skillOwner, void* method);
typedef void (*SetTimeScale_t)(float value, void* method);
typedef void (*GameUpdate_t)(void* self, void* method);
typedef void (*ShowAd_t)(void* self, void* request, void* method);
typedef void (*WbarrierSetField_t)(void* obj, void** field, void* value);

static MonsterReduceHp_t g_origMonsterReduce = nullptr;
static NpcReduceHp_t     g_origNpcReduce = nullptr;
static OnDamageBuff_t    g_origMonsterDot = nullptr;
static OnDamageBuff_t    g_origNpcDot = nullptr;
static PlayerOnDamage_t  g_origPlayerDamage = nullptr;
static SetTimeScale_t    g_origSetTimeScale = nullptr;
static GameUpdate_t      g_origGameUpdate = nullptr;
static ShowAd_t          g_origShowAd = nullptr;

static void* g_gameRuningKlass = nullptr;
static void* g_onADSuccessMethod = nullptr;
static WbarrierSetField_t g_wbarrierSetField = nullptr;

// ============================================================
// ② 字段偏移与 RVA（768 dump.cs / 序言实测；换包必须重 dump）
// 重定位工具：python3 04_scripts/一念通天/yntt_rva_extract.py（--prev 对照打印漂移）
// 768 复核结论（2026-10-30）：9 个挂点 RVA 相对 759 全部漂移；**序言首指令有 6 条变化**（759 只需换 RVA，
//   768 必须同步改 kIns*，否则 HookAtRva 序言校验不过会静默回退按名）：
//   MonsterReduce 0xD10303FF→0xD102C3FF、NpcReduce 0xD102C3FF→0xD10283FF、
//   MonsterDot 0xD103C3FF→0xD10443FF、PlayerDamage 0xD10343FF→0xD102C3FF、
//   GameUpdate 0xF81D0FF6→0xD100C3FF；NpcDot / SetTimeScale / ShowAD 三条与 759 相同。
//   字段 7 项变化：gas 0xA8→0xB0、tili 0xB0→0xB8、lingshi 0xB8→0xC0、money 0xC0→0xC8、
//   NPCData 0x2E8→0x2F0、fumodata 0x2F0→0x2F8、adRequest 0x458→0x470；**NpcStatus.type 0x18→0x20**（ENEMY 仍 =2）。
//   ABI 未变：Monster `mov w21,w3`(zhanlijue)+`mov w22,w2`(isAreaDamge)、Npc `mov w21,w2`、
//   OnDamageBuff `mov w22,w2`/`mov w26,w1`、onDamage `x1→x22,x2→x23` ⇒ 函数指针类型不动。
//   ⚠ 768 起 `.text` 段 p_offset≠p_vaddr（差 0x4000）：kRva* 是 **file offset**（HookAtRva 走 FileOffToVa），
//     必须取 dump.cs 的 Offset 列，不是 RVA/VA 列。
// ============================================================
enum : size_t {
    kOffNpcType       = 0x20,    // 768 变更：759 为 0x18
    kOffGas           = 0xB0,    // 768 变更：759 为 0xA8
    kOffTili          = 0xB8,    // 768 变更：759 为 0xB0
    kOffLingShi       = 0xC0,    // 768 变更：759 为 0xB8
    kOffMoney         = 0xC8,    // 768 变更：759 为 0xC0
    kOffNpcData       = 0x2F0,   // 768 变更：759 为 0x2E8
    kOffFuMoData      = 0x2F8,   // 768 变更：759 为 0x2F0
    kOffAdRequest     = 0x470,   // 768 变更：759 为 0x458
    kOffFuMoTili      = 0x28,
    kOffFuMoTiliZq    = 0x48,
    kOffFuMoTiliGq    = 0x80,
    kOffZldgTili      = 0x20,
};

enum : uintptr_t {
    kRvaMonsterReduce = 0x24F61D4,
    kRvaNpcReduce     = 0x223FFF4,
    kRvaMonsterDot    = 0x24E6C30,
    kRvaNpcDot        = 0x223567C,
    kRvaPlayerDamage  = 0x23709A8,
    kRvaSetTimeScale  = 0x462C89C,
    kRvaGameUpdate    = 0x248494C,
    kRvaShowAD        = 0x2482EAC,
};

enum : uint32_t {
    kInsMonsterReduce = 0xD102C3FF,   // 768 变更：759 为 0xD10303FF
    kInsNpcReduce     = 0xD10283FF,   // 768 变更：759 为 0xD102C3FF
    kInsMonsterDot    = 0xD10443FF,   // 768 变更：759 为 0xD103C3FF
    kInsNpcDot        = 0xD10383FF,
    kInsPlayerDamage  = 0xD102C3FF,   // 768 变更：759 为 0xD10343FF
    kInsSetTimeScale  = 0xFC1E0FE8,
    kInsGameUpdate    = 0xD100C3FF,   // 768 变更：759 为 0xF81D0FF6
    kInsShowAD        = 0xD10183FF,
};

enum : int32_t {
    kKillMultiplier = 100,
    kStaminaValue   = 9999,
    kLingShiValue   = 66666666,
    kMoneyValue     = 99999999,
};
enum : int64_t {
    kGasValue = 66666666,
};
static constexpr double kDamageCap = 1000000000.0;
enum : uint64_t {
    kInstallRetryMs = 1000,
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

static int64_t SafeReadI64(void* obj, size_t off, int64_t defv) {
    if (!OkPtr(obj)) return defv;
    int64_t v = defv;
    memcpy(&v, reinterpret_cast<uint8_t*>(obj) + off, sizeof(v));
    return v;
}

static void SafeWriteI64(void* obj, size_t off, int64_t v) {
    if (!OkPtr(obj)) return;
    memcpy(reinterpret_cast<uint8_t*>(obj) + off, &v, sizeof(v));
}

static void RaiseI32(void* obj, size_t off, int32_t target) {
    if (!OkPtr(obj)) return;
    if (SafeReadI32(obj, off, 0) < target) SafeWriteI32(obj, off, target);
}

static void RaiseI64(void* obj, size_t off, int64_t target) {
    if (!OkPtr(obj)) return;
    if (SafeReadI64(obj, off, 0) < target) SafeWriteI64(obj, off, target);
}

static int64_t NowMs() {
    struct timespec ts {};
    if (clock_gettime(CLOCK_MONOTONIC, &ts) != 0) return 0;
    return static_cast<int64_t>(ts.tv_sec) * 1000 + ts.tv_nsec / 1000000;
}

static double ScaleDamage(double damage) {
    if (!(damage > 0.0) || damage != damage) return damage;
    if (damage >= kDamageCap / static_cast<double>(kKillMultiplier)) return kDamageCap;
    return damage * static_cast<double>(kKillMultiplier);
}

static int32_t ScaleLayers(int32_t layers) {
    if (layers <= 0) return layers;
    if (layers > 0x7FFFFFFF / kKillMultiplier) return 0x7FFFFFFF;
    return layers * kKillMultiplier;
}

static bool IsEnemyNpc(void* self) {
    return SafeReadI32(self, kOffNpcType, -1) == 2;  // NPCTYPE.ENEMY
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

static void InstallTable(const HookSlot* slots, size_t count) {
    for (size_t i = 0; i < count; i++) HookSlotInstall(slots[i]);
}

// ============================================================
// ⑤ hook 函数与挂点表（TODO：一个具体方法一个槽；绝不 hook 共享 invoke 入口）
// ============================================================
static void Hooked_MonsterReduce(void* self, void* cfg, double damage, bool isAreaDamage, bool zhanlijue, void* method) {
    if (g_kill.load() && !t_inCombatHook) {
        t_inCombatHook = true;
        if (g_origMonsterReduce) g_origMonsterReduce(self, cfg, ScaleDamage(damage), isAreaDamage, zhanlijue, method);
        t_inCombatHook = false;
        return;
    }
    if (g_origMonsterReduce) g_origMonsterReduce(self, cfg, damage, isAreaDamage, zhanlijue, method);
}

static void Hooked_NpcReduce(void* self, void* cfg, double damage, bool zhanlijue, void* method) {
    const bool scale = g_kill.load() && IsEnemyNpc(self) && !t_inCombatHook;
    if (g_origNpcReduce) g_origNpcReduce(self, cfg, scale ? ScaleDamage(damage) : damage, zhanlijue, method);
}

static void Hooked_MonsterDot(void* self, int32_t buffId, int32_t overLayer, void* method) {
    const bool scale = g_kill.load() && !t_inCombatHook;
    if (g_origMonsterDot) g_origMonsterDot(self, buffId, scale ? ScaleLayers(overLayer) : overLayer, method);
}

static void Hooked_NpcDot(void* self, int32_t buffId, int32_t overLayer, void* method) {
    const bool scale = g_kill.load() && IsEnemyNpc(self) && !t_inCombatHook;
    if (g_origNpcDot) g_origNpcDot(self, buffId, scale ? ScaleLayers(overLayer) : overLayer, method);
}

static void Hooked_PlayerDamage(void* self, int64_t hate, void* skillOwner, void* method) {
    if (g_inv.load() && hate > 0) hate = 0;
    if (g_origPlayerDamage) g_origPlayerDamage(self, hate, skillOwner, method);
}

static void Hooked_SetTimeScale(float value, void* method) {
    const int speed = g_speed.load();
    if (speed > 1 && value > 0.0f && value < 1000.0f) {
        value *= static_cast<float>(speed);
        if (value > 100.0f) value = 100.0f;
    }
    if (g_origSetTimeScale) g_origSetTimeScale(value, method);
}

static void Hooked_GameUpdate(void* self, void* method) {
    if (g_origGameUpdate) g_origGameUpdate(self, method);
    if (!OkPtr(self)) return;

    if (g_stamina.load()) {
        RaiseI32(self, kOffTili, kStaminaValue);
        void* fumo = Il2Cpp::ReadPtr(self, kOffFuMoData);
        if (OkPtr(fumo)) {
            RaiseI32(fumo, kOffFuMoTili, kStaminaValue);
            RaiseI32(fumo, kOffFuMoTiliZq, kStaminaValue);
            RaiseI32(fumo, kOffFuMoTiliGq, kStaminaValue);
        }
        void* npcData = Il2Cpp::ReadPtr(self, kOffNpcData);
        if (OkPtr(npcData)) RaiseI32(npcData, kOffZldgTili, kStaminaValue);
    }

    if (g_currency.load()) {
        RaiseI32(self, kOffMoney, kMoneyValue);
        RaiseI32(self, kOffLingShi, kLingShiValue);
        RaiseI64(self, kOffGas, kGasValue);
    }
}

static void Hooked_ShowAd(void* self, void* request, void* method) {
    const bool bypass = g_noads.load() && OkPtr(self) && OkPtr(request) &&
                        g_wbarrierSetField && g_onADSuccessMethod &&
                        GetApi().runtime_invoke && !t_inShowAd;
    if (!bypass) {
        if (g_origShowAd) g_origShowAd(self, request, method);
        return;
    }

    t_inShowAd = true;
    void** field = reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(self) + kOffAdRequest);
    g_wbarrierSetField(self, field, request);

    void* exception = nullptr;
    auto& api = GetApi();
    if (api.runtime_invoke) api.runtime_invoke(g_onADSuccessMethod, self, nullptr, &exception);
    t_inShowAd = false;

    if (exception) {
        LOGW(TPL_LOG " noads onADSuccess exception, fallback original");
        if (g_origShowAd) g_origShowAd(self, request, method);
    } else {
        LOGI(TPL_LOG " noads bypass: onADSuccess invoked");
    }
}

static const HookSlot kKillSlots[] = {
    {"MonsterReduceHP", kRvaMonsterReduce, kInsMonsterReduce, "", "monsterStatus", "ReduceHP", 4,
     (void*)Hooked_MonsterReduce, (void**)&g_origMonsterReduce},
    {"NpcReduceHP", kRvaNpcReduce, kInsNpcReduce, "", "NpcStatus", "ReduceHP", 3,
     (void*)Hooked_NpcReduce, (void**)&g_origNpcReduce},
    {"MonsterOnDamageBuff", kRvaMonsterDot, kInsMonsterDot, "", "monsterStatus", "OnDamageBuff", 2,
     (void*)Hooked_MonsterDot, (void**)&g_origMonsterDot},
    {"NpcOnDamageBuff", kRvaNpcDot, kInsNpcDot, "", "NpcStatus", "OnDamageBuff", 2,
     (void*)Hooked_NpcDot, (void**)&g_origNpcDot},
};

static const HookSlot kInvSlots[] = {
    {"charStatus.onDamage", kRvaPlayerDamage, kInsPlayerDamage, "", "charStatus", "onDamage", 2,
     (void*)Hooked_PlayerDamage, (void**)&g_origPlayerDamage},
};

static const HookSlot kSpeedSlots[] = {
    {"Time.set_timeScale", kRvaSetTimeScale, kInsSetTimeScale, "UnityEngine", "Time", "set_timeScale", 1,
     (void*)Hooked_SetTimeScale, (void**)&g_origSetTimeScale},
};

static const HookSlot kResourceSlots[] = {
    {"GameRuning.Update", kRvaGameUpdate, kInsGameUpdate, "", "GameRuning", "Update", 0,
     (void*)Hooked_GameUpdate, (void**)&g_origGameUpdate},
};

static const HookSlot kAdSlots[] = {
    {"GameRuning.ShowAD", kRvaShowAD, kInsShowAD, "", "GameRuning", "ShowAD", 1,
     (void*)Hooked_ShowAd, (void**)&g_origShowAd},
};

// ============================================================
// ⑥ 分组安装 + 懒装（TODO：按功能组拆；开关全关 = 0 hook）
// ============================================================
static bool ResolveRuntimeBits() {
    if (!EnsureApi()) return false;
    auto& api = GetApi();
    if (!g_gameRuningKlass) g_gameRuningKlass = api.FindClassByShortName("GameRuning");
    if (!g_gameRuningKlass) {
        LOGW(TPL_LOG " GameRuning class not ready yet — combat hooks continue");
        return true;
    }
    if (!g_onADSuccessMethod) {
        g_onADSuccessMethod = api.GetMethod(g_gameRuningKlass, "onADSuccess", 0);
        if (!g_onADSuccessMethod) LOGW(TPL_LOG " onADSuccess not found — noads degraded");
    }
    if (!g_wbarrierSetField) {
        void* h = dlopen(TPL_TARGET_LIB, RTLD_NOW | RTLD_NOLOAD);
        if (h) {
            g_wbarrierSetField = reinterpret_cast<WbarrierSetField_t>(
                dlsym(h, "il2cpp_gc_wbarrier_set_field"));
            dlclose(h);
        }
        if (!g_wbarrierSetField) {
            g_wbarrierSetField = reinterpret_cast<WbarrierSetField_t>(
                dlsym(RTLD_DEFAULT, "il2cpp_gc_wbarrier_set_field"));
        }
        if (!g_wbarrierSetField) LOGW(TPL_LOG " wbarrier missing — noads degraded");
    }
    return true;
}

static void SyncHooks(bool force) {
    if (!g_libReady.load()) return;
    std::lock_guard<std::mutex> lock(g_installMutex);
    const int64_t now = NowMs();
    if (!force) {
        const int64_t last = g_lastInstallMs.load(std::memory_order_relaxed);
        if (last && now > last && now - last < static_cast<int64_t>(kInstallRetryMs)) return;
    }
    g_lastInstallMs.store(now, std::memory_order_relaxed);

    if (g_kill.load()) InstallTable(kKillSlots, sizeof(kKillSlots) / sizeof(kKillSlots[0]));
    if (g_inv.load()) InstallTable(kInvSlots, sizeof(kInvSlots) / sizeof(kInvSlots[0]));
    if (g_speed.load()) InstallTable(kSpeedSlots, sizeof(kSpeedSlots) / sizeof(kSpeedSlots[0]));
    if (g_stamina.load() || g_currency.load()) {
        InstallTable(kResourceSlots, sizeof(kResourceSlots) / sizeof(kResourceSlots[0]));
    }
    if (g_noads.load() && (!g_wbarrierSetField || !g_onADSuccessMethod)) {
        ResolveRuntimeBits();
    }
    if (g_noads.load() && g_wbarrierSetField && g_onADSuccessMethod) {
        InstallTable(kAdSlots, sizeof(kAdSlots) / sizeof(kAdSlots[0]));
    }
}

static void SyncHooks() { SyncHooks(true); }

// ============================================================
// ⑦ 对外接口（ModEntry.h 注册表契约，勿改签名）
// ============================================================
bool Yntt_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;
    if (!ResolveRuntimeBits()) return false;
    g_libReady.store(true);
    SyncHooks();  // 已 ON 的开关立即装；全关则保持 0 hook
    LOGI(TPL_LOG " lib ready (dual-path, lazy hooks)");
    return true;
}

bool Yntt_IsReady() { return g_libReady.load(); }

void Yntt_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case YNTT_FEAT_KILL:     g_kill.store(enabled); break;
        case YNTT_FEAT_INV:      g_inv.store(enabled); break;
        case YNTT_FEAT_SPEED:    g_speed.store(enabled ? 5 : 1); break;
        case YNTT_FEAT_STAMINA:  g_stamina.store(enabled); break;
        case YNTT_FEAT_CURRENCY: g_currency.store(enabled); break;
        case YNTT_FEAT_NOADS:    g_noads.store(enabled); break;
        default: break;
    }
    SyncHooks();
}

void Yntt_OnValueChanged(int featNum, int value) {
    if (featNum != YNTT_FEAT_SPEED) return;
    if (value < 1) value = 1;
    if (value > 10) value = 10;
    g_speed.store(value);
    LOGI(TPL_LOG " speed slider=%d", value);
}

void Yntt_Tick(const bool* features, const int* values) {
    // 只做：同步开关 + 兜底安装。禁止在这里 SafeProbe/扫堆/遍历对象/逐对象写
    if (features) {
        g_kill.store(features[YNTT_FEAT_KILL]);
        g_inv.store(features[YNTT_FEAT_INV]);
        if (values && values[YNTT_FEAT_SPEED] > 0) {
            int speed = values[YNTT_FEAT_SPEED];
            if (speed < 1) speed = 1;
            if (speed > 10) speed = 10;
            g_speed.store(speed);
        }
        g_stamina.store(features[YNTT_FEAT_STAMINA]);
        g_currency.store(features[YNTT_FEAT_CURRENCY]);
        g_noads.store(features[YNTT_FEAT_NOADS]);
    }
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) g_libReady.store(true);
    SyncHooks();
}

void Yntt_ForceClear() {
    g_kill.store(false);
    g_inv.store(false);
    g_speed.store(1);
    g_stamina.store(false);
    g_currency.store(false);
    g_noads.store(false);
    // TODO: 其他功能开关一并复位
}
