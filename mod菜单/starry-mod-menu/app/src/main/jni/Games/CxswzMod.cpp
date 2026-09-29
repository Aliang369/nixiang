/* ================================================================
 *  CxswzMod — 潮汐守望者（com.gf.cxswz / TapTap 官包 1.2.56.375.3）
 *
 *  ⚠️ 本作架构特殊（2026-09-30 实测，改动前必读）：
 *    APK 内 lib/arm64-v8a/libil2cpp.so 仅 157KB，是 7z 解压壳
 *    （com.moba.unityplugin.Z7Extractor）；真 IL2CPP 库首启解压到
 *    getExternalFilesDir()/dragon2019/assets/comlibs/arm64-v8a/liblogic.bytes，
 *    由 DynamicSOLoader 复制为 liblogic.so 后 System.load(path) 动态装进进程。
 *    ⇒ 目标库名 = liblogic.so；RVA→运行时地址必须走 /proc/self/maps 换算。
 *
 *  阶段状态（2026-09-30 功能阶段）：
 *    环境（菜单进游戏 / 去签名 / TapTap 强制网页登录）已过；只读探针实测确认挂点
 *    与数据布局后，在 HpChangeEntitySystem.ExcuteOnce 入口改 self+0x60(hData).fValue：
 *      · 无敌 = 我方单位（TowerSign / CardSoldier）承伤（fValue<0）清零
 *      · 伤害倍率 = 对敌（MonsterSign）伤害（fValue<0）乘倍率（1 = 关闭）
 *    敌我判定与正负号均为真机探针实测（P# 日志），不是猜测。
 *
 *  ⚠️ 防闪退设施已内置为代码，删任何一节都可能复现历史闪退：
 *    默认 0 hook（懒装）/ 双路径挂点 / RVA 漂移拒绝 / 幂等安装
 *    orig 必调 / reenter 防递归 / SafeWrite 校验写 / Tick 轻量
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 *
 * ================================================================ */

#include "CxswzMod.h"

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

#define TPL_LOG "[Cxswz]"  // 生成器会替换为本游戏标签

// 目标库：运行时解压出来的真 IL2CPP 库（包内 libil2cpp.so 是 7z 解压壳）
#define TPL_TARGET_LIB "liblogic.so"

// 构建身份（出包/装机/真机日志三处对账用；改功能时同步递增）
#define CXSWZ_BUILD_TAG "cxswz-f2-20260930"

// 伤害倍率上限（必须与 CxswzEntry.cpp 的 "Slider_伤害倍率:1:10:1" 一致）
#define DMG_MUL_MAX 10

// ============================================================
// ① 偏移与 RVA（绑定 liblogic.bytes 1.2.56.375.3；换包/热更必须重 dump 复核）
//    dump.cs: HpChangeEntitySystem(16410) / HPChangeComponent(16331) /
//             stHpChangeData(16702) / Entity(16387) / EComponentType(16296)
// ============================================================
#define RVA_EXCUTE_ONCE    0x4927B28u   // bool HpChangeEntitySystem.ExcuteOnce(Entity)
#define INSN_EXCUTE_ONCE   0xA9BB67FCu // 序言字（文件字节 fc 67 bb a9，小端读为 0xA9BB67FC）
#define RVA_HAS_COMPONENT  0x497368Cu   // bool Entity.HasComponent(EComponentType)

// HpChangeEntitySystem 字段（self 偏移）
#define SYS_OFF_COM_HP    0x50   // HPChangeComponent* com_hp
#define SYS_OFF_HDATA     0x60   // stHpChangeData hData（当前正在结算的一条）
#define SYS_OFF_MATK      0xD0   // Entity* mAtkEntity
#define SYS_OFF_FDAMAGE   0xD8   // Fix64 fDamage

// HPChangeComponent / List<T> / 托管数组
#define HPCOMP_OFF_LVALUES 0x20  // List<stHpChangeData> lValues
#define LIST_OFF_ITEMS     0x10  // T[] _items
#define LIST_OFF_SIZE      0x18  // int _size
#define ARRAY_OFF_DATA     0x20  // 首元素（数组头 0x20）

// stHpChangeData（stride 0x70）
#define HPD_STRIDE        0x70
#define HPD_OFF_FVALUE    0x00   // Fix64 fValue
#define HPD_OFF_SRC       0x08   // EDamageSourceType
#define HPD_OFF_SETTLE    0x0C   // EDamageSettleType
#define HPD_OFF_SHOW      0x10   // ETextShowType
#define HPD_OFF_ATK_EUID  0x1C   // eRelationData.eUid（攻击者 entity uid）
#define HPD_OFF_ATK_CUID  0x20   // eRelationData.cUid
#define HPD_OFF_ATK_UNIT  0x24   // eRelationData.iUnitID
#define HPD_OFF_SKILL     0x28   // iAttackSkillID
#define HPD_OFF_FORMULA   0x2C   // iFormularID

// EComponentType
#define ECOMP_CARDSOLDIER 43     // CardSoldier（我方英雄/卡牌单位）
#define ECOMP_MONSTERSIGN 60     // MonsterSign（敌方怪物）
#define ECOMP_TOWERSIGN   61     // TowerSign（我方塔/建筑）

// Fix64 = Q20.12（FRACTIONAL_PLACES=12 ⇒ ONE=4096）
#define FIX64_ONE 4096.0

// ============================================================
// ============================================================
static std::atomic<bool> g_libReady{false};  // liblogic.so 已映射（≠ 已 hook）
static std::atomic<bool> g_anyHooked{false};
static std::atomic<int>  g_dmgMul{1};      // 伤害倍率（1 = 关闭；菜单 Slider 1..DMG_MUL_MAX）
static std::atomic<bool> g_inv{false};      // 无敌开关
static std::atomic<bool> g_reenter{false};
static std::atomic<int>  g_probeCount{0};   // 探针阶段：已打印条数（有界）
static std::atomic<bool> g_probeReady{false}; // ExcuteOnce 已成功挂上
#define PROBE_MAX 400                     // 探针日志上限（防刷爆 logcat）
static std::atomic<int>  g_modCount{0};     // 功能生效日志计数（有界）
#define MOD_LOG_MAX 200                   // 功能生效日志上限


// ============================================================
// ② 字段偏移与 RVA（功能阶段填：dump.cs / IDA / Trace 实测，禁止猜；换包重 dump）
// ============================================================
// enum : size_t { kOffXxx = 0x0 };  // 示例占位已删：功能阶段按 dump 实测回填

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
// ⑤ 探针挂点（只读）：HpChangeEntitySystem.ExcuteOnce
//    ExcuteAll 循环把 com_hp.lValues[i] 拷到 self+0x60(hData) 后调 ExcuteOnce；
//    故 hook 入口处 self+0x60 = 当前这条 stHpChangeData（含 fValue/AtkRelationData）。
//    本阶段只 LOGI 读值，不改任何内存（hook-rules §22 步 2：只读计数验证）。
// ============================================================
typedef bool (*HasComponentFn)(void* entity, int type, void* methodInfo);
typedef bool (*ExcuteOnceFn)(void* self, void* entity);
static HasComponentFn g_hasComponent = nullptr;
static ExcuteOnceFn  orig_ExcuteOnce = nullptr;

// Entity.m_uID 后备字段（dump.cs: private uint <m_uID>k__BackingField @ 0x10）
#define ENTITY_OFF_MUID 0x10

// 读托管 List<T> 的 items 数据指针 + size（只做存活/边界检查，不遍历）
static bool ReadListData(void* list, void** outData, int32_t* outSize) {
    if (!OkPtr(list)) return false;
    void*    items = nullptr;
    int32_t  size  = 0;
    memcpy(&items, reinterpret_cast<uint8_t*>(list) + LIST_OFF_ITEMS, sizeof(items));
    memcpy(&size,  reinterpret_cast<uint8_t*>(list) + LIST_OFF_SIZE,  sizeof(size));
    if (!OkPtr(items) || size < 0) return false;
    *outData = reinterpret_cast<uint8_t*>(items) + ARRAY_OFF_DATA;
    *outSize = size;
    return true;
}

// Fix64 放大（Q20.12：乘整数倍不改变定点标度）；__int128 防溢出后夹取
static inline int64_t ScaleFix64(int64_t raw, int mul) {
    __int128 t = (__int128)raw * (__int128)mul;
    if (t < (__int128)INT64_MIN) return INT64_MIN;
    if (t > (__int128)INT64_MAX) return INT64_MAX;
    return (int64_t)t;
}

static bool Hook_ExcuteOnce(void* self, void* entity) {
    if (!orig_ExcuteOnce) return false;  // orig 未就绪，绝不伪造成功
    if (g_reenter.load()) return orig_ExcuteOnce(self, entity);
    g_reenter.store(true);

    const bool wantInv = g_inv.load();
    const int  mul     = g_dmgMul.load();
    const bool wantLog = g_probeReady.load() && g_probeCount.load() < PROBE_MAX;

    if ((wantInv || mul > 1 || wantLog) && OkPtr(self)) {
        const int n = wantLog ? (g_probeCount.fetch_add(1) + 1) : 0;
        uint8_t* sys = reinterpret_cast<uint8_t*>(self);
        uint8_t* hd  = sys + SYS_OFF_HDATA;

        int64_t  raw = 0;
        int32_t  src = 0, settle = 0, show = 0, skill = 0, formula = 0;
        uint32_t atkE = 0, atkC = 0;
        int32_t  atkU = 0;
        memcpy(&raw,     hd + HPD_OFF_FVALUE,   sizeof(raw));
        memcpy(&src,     hd + HPD_OFF_SRC,      4);
        memcpy(&settle,  hd + HPD_OFF_SETTLE,   4);
        memcpy(&show,    hd + HPD_OFF_SHOW,     4);
        memcpy(&skill,   hd + HPD_OFF_SKILL,    4);
        memcpy(&formula, hd + HPD_OFF_FORMULA,  4);
        memcpy(&atkE,    hd + HPD_OFF_ATK_EUID, 4);
        memcpy(&atkC,    hd + HPD_OFF_ATK_CUID, 4);
        memcpy(&atkU,    hd + HPD_OFF_ATK_UNIT, 4);
        const int64_t rawIn = raw;   // 改写前的原始值（日志用）

        void* comHp = nullptr;
        memcpy(&comHp, sys + SYS_OFF_COM_HP, sizeof(comHp));
        int32_t sz = -1;
        if (OkPtr(comHp)) {
            void* lv = nullptr;
            memcpy(&lv, reinterpret_cast<uint8_t*>(comHp) + HPCOMP_OFF_LVALUES, sizeof(lv));
            void* data = nullptr;
            if (ReadListData(lv, &data, &sz)) { /* sz 已取到 */ }
        }

        int32_t mon = -1, twr = -1, cs = -1;
        uint32_t euid = 0;
        if (OkPtr(entity)) {
            memcpy(&euid, reinterpret_cast<uint8_t*>(entity) + ENTITY_OFF_MUID, 4);
            if (g_hasComponent) {
                mon = g_hasComponent(entity, ECOMP_MONSTERSIGN, nullptr) ? 1 : 0;
                if (mon == 0) {
                    twr = g_hasComponent(entity, ECOMP_TOWERSIGN,   nullptr) ? 1 : 0;
                    if (twr == 0)
                        cs = g_hasComponent(entity, ECOMP_CARDSOLDIER, nullptr) ? 1 : 0;
                }
            }
        }
        const bool isFriendly = (twr == 1 || cs == 1);

        // ---- 无敌：我方单位（Tower/CardSoldier）承伤清零 ----
        if (wantInv && raw < 0 && isFriendly) {
            const int64_t zero = 0;
            memcpy(hd + HPD_OFF_FVALUE, &zero, sizeof(zero));
            const int m = g_modCount.fetch_add(1);
            if (m < MOD_LOG_MAX)
                LOGI(TPL_LOG " INV e=%p uid=%u raw=%lld->0", entity, euid, (long long)rawIn);
        }
        // ---- 伤害倍率：对敌（MonsterSign）伤害放大 ----
        else if (mul > 1 && raw < 0 && mon == 1) {
            const int64_t scaled = ScaleFix64(raw, mul);
            memcpy(hd + HPD_OFF_FVALUE, &scaled, sizeof(scaled));
            const int m = g_modCount.fetch_add(1);
            if (m < MOD_LOG_MAX)
                LOGI(TPL_LOG " DMG x%d e=%p uid=%u raw=%lld->%lld", mul, entity, euid,
                     (long long)rawIn, (long long)scaled);
        }

        LOGI(TPL_LOG " P#%d e=%p uid=%u mon=%d twr=%d cs=%d sz=%d raw=%lld v=%.3f "  // NOLINT
                    "src=%d set=%d show=%d atk=%u/%u u%d sk=%d fm=%d",
             n, entity, euid, mon, twr, cs, sz, (long long)rawIn, (double)rawIn / FIX64_ONE,
             src, settle, show, atkE, atkC, atkU, skill, formula);
    }

    g_reenter.store(false);
    return orig_ExcuteOnce(self, entity);
}

static const HookSlot kSlots[] = {
    {"ExcuteOnce", RVA_EXCUTE_ONCE, INSN_EXCUTE_ONCE,
     "CSharpBattle.Battle", "HpChangeEntitySystem", "ExcuteOnce", 1,
     (void*)Hook_ExcuteOnce, (void**)&orig_ExcuteOnce},
};
static const size_t kSlotsCount = sizeof(kSlots) / sizeof(kSlots[0]);

// 解析 HasComponent 的运行时地址（失败则 mon/twr/cs 全 -1，功能不生效但不闪退）
static void ResolveProbeHelpers() {
    if (!g_hasComponent)
        g_hasComponent = reinterpret_cast<HasComponentFn>(FileOffToVa(TPL_TARGET_LIB, RVA_HAS_COMPONENT));
    LOGI(TPL_LOG " probe helpers hasComponent=%p", (void*)g_hasComponent);
}

// ============================================================
// ⑥ 分组安装 + 懒装（功能阶段按功能组拆；开关全关 = 0 hook）
// ============================================================
static void InstallBattleHooks() {
    for (size_t i = 0; i < kSlotsCount; i++) {
        if (HookSlotInstall(kSlots[i])) g_anyHooked.store(true);
    }
    if (orig_ExcuteOnce) g_probeReady.store(true);
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    if (kSlotsCount == 0) return;   // 无挂点表 = 不安装
    if (g_inv.load() || g_dmgMul.load() > 1) InstallBattleHooks();
}

// ============================================================
// ⑦ 对外接口（ModEntry.h 注册表契约，勿改签名）
// ============================================================
bool Cxswz_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;
    g_libReady.store(true);
    ResolveProbeHelpers();
    InstallBattleHooks();  // 无条件挂 ExcuteOnce（开关全关时等同只读探针，零改写）
    LOGI(TPL_LOG " lib ready (liblogic.so, feature stage) hooked=%d tag=%s",
         g_anyHooked.load() ? 1 : 0, CXSWZ_BUILD_TAG);
    return true;
}

bool Cxswz_IsReady() { return g_libReady.load(); }

void Cxswz_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == CXSWZ_FEAT_INV) {
        g_inv.store(enabled);
        LOGI(TPL_LOG " 无敌 = %d", (int)enabled);
    }
    SyncHooks();
}

void Cxswz_OnValueChanged(int featNum, int value) {
    if (featNum == CXSWZ_FEAT_DMG) {
        const int v = value < 1 ? 1 : (value > DMG_MUL_MAX ? DMG_MUL_MAX : value);
        g_dmgMul.store(v);
        LOGI(TPL_LOG " 伤害倍率 = %d", v);
    }
}

void Cxswz_Tick(const bool* features, const int* values) {
    // 只做：同步开关 + 兜底安装。禁止在这里 SafeProbe/扫堆/遍历对象/逐对象写
    if (features) g_inv.store(features[CXSWZ_FEAT_INV]);
    if (values) {
        const int v = values[CXSWZ_FEAT_DMG];
        g_dmgMul.store(v < 1 ? 1 : (v > DMG_MUL_MAX ? DMG_MUL_MAX : v));
    }
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) g_libReady.store(true);
    SyncHooks();
}

void Cxswz_ForceClear() {
    g_dmgMul.store(1);
    g_inv.store(false);
}
