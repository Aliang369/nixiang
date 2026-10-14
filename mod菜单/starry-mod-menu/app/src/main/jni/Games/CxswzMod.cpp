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
#include <unordered_map>   // ★e7：游戏程序集短名索引（std::unordered_map<std::string, void*>）
#include <string>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <cstring>
#include <cerrno>
#include <ctime>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Includes/StarPtr.h"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define TPL_LOG "[Cxswz]"  // 生成器会替换为本游戏标签

// 目标库：运行时解压出来的真 IL2CPP 库（包内 libil2cpp.so 是 7z 解压壳）
#define TPL_TARGET_LIB "liblogic.so"

// 构建身份（出包/装机/真机日志三处对账用；改功能时同步递增）
// f1/f2 = 1.2.56.375.3（静态 RVA 时代）；e1+ = 1.2.65.385.1（运行时按名解析时代）
#define CXSWZ_BUILD_TAG "cxswz-e10-20261014"

// 伤害倍率上限（必须与 CxswzEntry.cpp 的 "Slider_伤害倍率:1:10:1" 一致）
#define DMG_MUL_MAX 10

// ============================================================
// ① 布局兜底值（初值 = 1.2.56.375.3 dump.cs 实测表；1.2.65.385.1 起
//    全部偏移在运行时由 ResolveRuntimeLayout() 复核覆盖，此处仅兜底）
//    旧版静态 dump 类型下标：HpChangeEntitySystem(16410) / HPChangeComponent(16331) /
//                          stHpChangeData(16702) / Entity(16387) / EComponentType(16296)
//
// ⚠️ 换包后静态 dump 不可得：1.2.65.385.1 的 liblogic.bytes 让 Il2CppDumper 在
//    AutoPlusInit 把 CodeRegistration 定位成假阳性 ⇒
//      System.IO.EndOfStreamException @ Il2Cpp.Init(UInt64,UInt64) Il2Cpp.cs:line 165
//    ⇒ RVA 路径**故意全部置 0**：HookSlotInstall 的 `if (s.rva && HookAtRva(...))`
//      短路，强制走 HookByName（MethodInfo.methodPointer）。旧值仅作文档留档。
// ============================================================
#define RVA_EXCUTE_ONCE_DOC_ONLY    0x4927B28u   // 仅供 1.2.56.375.3 对账，代码不引用
#define INSN_EXCUTE_ONCE_DOC_ONLY   0xA9BB67FCu  // 同上（序言字）
#define RVA_HAS_COMPONENT_DOC_ONLY  0x497368Cu   // 同上

// —— 功能关键偏移（热路径引用，运行时按名复核覆盖）——
static int32_t g_off_SYS_COM_HP     = 0x50;  // com_hp: HPChangeComponent*
static int32_t g_off_SYS_HDATA      = 0x60;  // hData: stHpChangeData（当前结算条目）
static int32_t g_off_HPCOMP_LVALUES = 0x20;  // HPChangeComponent.lValues: List<stHpChangeData>
static int32_t g_off_HPD_FVALUE     = 0x00;  // stHpChangeData.fValue: Fix64
// —— 仅探针日志/对账用（解析失败不影响功能）——
static int32_t g_off_SYS_MATK     = 0xD0;   // mAtkEntity: Entity*
static int32_t g_off_SYS_FDAMAGE  = 0xD8;   // fDamage: Fix64
static int32_t g_off_HPD_SRC      = 0x08;   // EDamageSourceType
static int32_t g_off_HPD_SETTLE   = 0x0C;   // EDamageSettleType
static int32_t g_off_HPD_SHOW     = 0x10;   // ETextShowType
static int32_t g_off_HPD_ATK_EUID = 0x1C;   // eRelationData.eUid
static int32_t g_off_HPD_ATK_CUID = 0x20;   // eRelationData.cUid
static int32_t g_off_HPD_ATK_UNIT = 0x24;   // eRelationData.iUnitID
static int32_t g_off_HPD_SKILL    = 0x28;   // iAttackSkillID
static int32_t g_off_HPD_FORMULA  = 0x2C;   // iFormularID
static int32_t g_off_ENTITY_MUID  = 0x10;   // <m_uID>k__BackingField（uint）

// HPChangeComponent / List<T> / 托管数组（IL2CPP 运行时内部布局，跨版本稳定）
#define LIST_OFF_ITEMS     0x10  // T[] _items
#define LIST_OFF_SIZE      0x18  // int _size
#define ARRAY_OFF_DATA     0x20  // 首元素（数组头 0x20）

// stHpChangeData 元素跨度（仅文档/对账用，代码不引用）
#define HPD_STRIDE        0x70

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

// 目标库的**真实绝对路径**（/proc/self/maps 最后一段，须以 '/' 开头）。
// 用途：裸名 dlopen/xdl_open 失败时的第二级兜底（Wor 侧裸名必失败；本作实测成功，留作加固）。
// 无 root 环境下也能拿到：liblogic.so 是应用私有目录，maps 里是绝对路径。
static const char* RealPathOfLib(const char* lib) {
    static char s_path[512];
    if (s_path[0]) return s_path;
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return nullptr;
    char line[768];
    while (fgets(line, sizeof(line), fp)) {
        if (!IsLibMapLine(line, lib)) continue;
        char* nl = strchr(line, '\n');
        if (nl) *nl = 0;
        char* sp = strrchr(line, ' ');
        if (!sp) continue;
        const char* p = sp + 1;
        if (p[0] != '/') continue;
        size_t n = strlen(p);
        if (n == 0 || n >= sizeof(s_path)) continue;
        memcpy(s_path, p, n + 1);
        break;
    }
    fclose(fp);
    if (s_path[0]) LOGI("%s maps: %s real path = %s", TPL_LOG, lib, s_path);
    else           LOGW("%s maps: %s real path not found", TPL_LOG, lib);
    return s_path[0] ? s_path : nullptr;
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
    // ★2026-10-14 修复「功能没效果」：本作包内 libil2cpp.so 只是 7z 解压壳（157KB），
    //   真 IL2CPP 运行时在首启自解压出来的 liblogic.so（177MB，.dynsym 有 229 个 il2cpp_*）。
    //   若沿用默认可信范围（libil2cpp.so），xdl_sym 解析到的正确地址会被 InIl2CppRange()
    //   当 themis 诱饵丢弃 ⇒ 93/93 miss ⇒ core api missing ⇒ hooked=0。
    //   必须在 Load() 之前把可信模块换成 liblogic.so。
    Il2Cpp::SetTrustLib(TPL_TARGET_LIB);
    if (GetApi().Load(TPL_TARGET_LIB)) return true;
    // 兜底：裸名 dlopen 失败时改用 /proc/self/maps 里的真实绝对路径再试一次
    //   （本作实测裸名 xdl_open 成功，此路仅为加固；WorMod 侧裸名 dlopen 必失败）
    const char* rp = RealPathOfLib(TPL_TARGET_LIB);
    if (rp && *rp) {
        LOGW("%s api: bare-name load failed, retry via maps path %s", TPL_LOG, rp);
        return GetApi().Load(rp);
    }
    return false;
}

/* ============================================================================
 * ★e6 启动崩溃第二修：hook `il2cpp_init` —— 唯一可靠的「运行时真正就绪」判据
 *
 * 证据（e5 版实测 / 小米 96f0d883 / 1.2.65.385.1）：
 *   14:23:10.44x 进程在 liblogic.so dlopen 后 ~0.6s 就做 67 图按名扫描
 *                （ImageOf 已修好、image 全部取到；随后 65 次 ClassFromNameScan 建表）
 *   14:23:10.463 SIGABRT，崩溃线程 = 我们的 mod_thread，x15 = liblogic+0x288AEF4
 *                （与 e2 崩溃**同一偏移** ⇒ abort 由 il2cpp 自己发起）
 *   ⇒ `domain_assemblies` 非空**不足以**代表 init 完成：程序集 vector 在 il2cpp_init
 *     内部就被填好（实测 67 条）。必须等 il2cpp_init 真正返回后才能碰元数据 API。
 * ========================================================================== */
static int64_t MsNow();

typedef void* (*il2cpp_init_t)(const char*);
static il2cpp_init_t        orig_il2cpp_init = nullptr;
static std::atomic<bool>    g_il2cppInited{false};
static std::atomic<int64_t> g_initDoneMs{0};
static std::atomic<int64_t> g_firstAsmMs{0};
#define CXSWZ_POST_INIT_MS  800     // init 返回后再缓一拍（等 Unity 自己把脚本层拉起来）
#define CXSWZ_ASM_SETTLE_MS 6000    // hook 没赶上（竞态）时的兜底静默期，自首次看到程序集起算

static void* Hook_il2cpp_init(const char* domainName) {
    const int64_t t0 = MsNow();
    void* ret = orig_il2cpp_init ? orig_il2cpp_init(domainName) : nullptr;
    const int64_t t1 = MsNow();
    g_initDoneMs.store(t1);
    g_il2cppInited.store(true);
    LOGI(TPL_LOG " il2cpp_init 已返回 ret=%p domain=%s 耗时=%lldms — 运行时就绪，%dms 后解析布局",
         ret, domainName ? domainName : "(null)", (long long)(t1 - t0), CXSWZ_POST_INIT_MS);
    return ret;
}

static void InstallInitHook() {
    static std::atomic<bool> hooked{false};
    static std::atomic<int>  tries{0};
    if (hooked.load()) return;
    if (tries.fetch_add(1) > 20) return;      // 有界重试（lib 未就绪时先失败，Tick 里再补）
    void* fn = Il2Cpp::ResolveExport("il2cpp_init");
    if (!fn || !Il2Cpp::IsExecutableRange((uintptr_t)fn, 16)) {
        LOGW(TPL_LOG " il2cpp_init hook: 导出不可用 fn=%p — 走时间兜底 %dms", fn, CXSWZ_ASM_SETTLE_MS);
        return;
    }
    if (DobbyHook(fn, (void*)Hook_il2cpp_init, (void**)&orig_il2cpp_init) != 0) {
        LOGW(TPL_LOG " il2cpp_init hook: dobby 失败 — 走时间兜底 %dms", CXSWZ_ASM_SETTLE_MS);
        return;
    }
    hooked.store(true);
    LOGI(TPL_LOG " il2cpp_init hooked %p — 以 init 返回作为运行时就绪判据", fn);
}

/* ============================================================================
 * ★2026-10-14 启动崩溃修复：il2cpp 运行时就绪闸门（只读探针，零副作用）
 *
 * 崩溃证据（小米 96f0d883 / 1.2.65.385.1 / e2 版）：
 *   13:54:10.402 游戏 dlopen liblogic.so（il2cpp 尚未 init）
 *   13:54:10.556 我们的 mod 调 il2cpp API（ResolveRuntimeLayout → Api::Assemblies）
 *   13:54:10.693 Unity 才打出第一行 init 日志
 *   13:54:10.986 SIGABRT（backtrace 只有 libc abort+168，无 caller 帧），随后每 ~3s 重启一次
 *   e1 版（API 解析失败、从未调用任何 il2cpp 函数）在同一台机跑满 7 分钟不崩 ⇒ 差异即本调用。
 *
 * 离线反汇编 liblogic.bytes 查实：
 *   il2cpp_domain_get = +0x44afca8 → +0x44faf9c：
 *       adrp x19,0xa92e000 ; ldr x0,[x19,#0x570] ; cbnz x0,ret ; mov w0,#0x30 ; bl <calloc-like> ; str x0,[x19,#0x570]
 *     ⇒ **有副作用**：lazy 创建 Domain 单例并写回全局 0xa92e570。在 il2cpp_init 之前调用它，
 *       会让 Unity 自己的初始化走岔路 ⇒ abort。★禁止使用会经过它的接口：
 *       Api::Domain() 与 Api::Assemblies()（后者 L1004 首行就是 `if (!domain) return nullptr;`）。
 *   il2cpp_domain_get_assemblies = +0x44afcd4：
 *       adrp/add x0,0xa92e198 ; ldp x9,x8,[x0] ; sub/asr → *size=(end-beg)/8 ; ldr x0,[x0]
 *     ⇒ 忽略 domain 参数、只读静态 std::vector ⇒ 传 nullptr 安全；程序集未加载时 (null,0)，
 *       加载完成后 (非空, n>0)。这就是本文件唯一允许的「就绪探针」。
 * ========================================================================== */
static bool Il2CppRuntimeUp() {
    auto& api = GetApi();
    if (!api.ready || !api.domain_assemblies) return false;
    size_t n = 0;
    void** a = api.domain_assemblies(nullptr, &n);
    if (!a || n == 0) return false;   // 程序集未加载 = init 还没走到填充那一步
    const int64_t now = MsNow();
    if (g_firstAsmMs.load() == 0) {
        g_firstAsmMs.store(now);
        LOGI(TPL_LOG " 首次看到程序集 n=%zu t0=%lldms — 兜底静默期 %dms 起算",
             n, (long long)now, CXSWZ_ASM_SETTLE_MS);
    }
    /* ★e6：优先用 il2cpp_init 返回时刻（精确）；hook 没赶上 init 的竞态场景退化为时间兜底。 */
    if (g_il2cppInited.load()) return now - g_initDoneMs.load() >= CXSWZ_POST_INIT_MS;
    return now - g_firstAsmMs.load() >= CXSWZ_ASM_SETTLE_MS;
}

/* 安全的程序集枚举（替代 Api::Assemblies：后者会调 Il2Cpp::Domain → il2cpp_domain_get） */
static size_t SafeAssemblies(void*** out) {
    if (out) *out = nullptr;
    auto& api = GetApi();
    if (!api.domain_assemblies) return 0;
    size_t n = 0;
    void** a = api.domain_assemblies(nullptr, &n);
    if (!a || n == 0) return 0;
    if (out) *out = a;
    return n;
}

static void* SweepClassByExport(const char* ns, const char* name);   // ★e6（定义见 §④.5）

static void* FindClassNs(const char* ns, const char* name) {
    if (!Il2CppRuntimeUp()) return nullptr;   // ★运行时就绪闸门（见上）
    /* ★e6：改走裸导出扫荡。不再用 Api::ClassFromName —— 它未命中时会降级到
     *   ClassFromNameScan（逐类建 std::string 表），67 个程序集 = 67 次建表，
     *   实测在 il2cpp_init 之前把进程带崩（SIGABRT）。 */
    return SweepClassByExport(ns, name);
}

// 按名路径：类/方法缺失只 log 降级，不整包失败（hook-rules §17）
// ★e4：类查找加本地兜底（FindClassShortLocal，定义在 §④.5）——共享层的
//   FindClassByShortName 内部只走 Api::ImageOf，主路径一失效它就必然全 miss。
static void* FindClassShortLocal(const char* shortName);
static bool HookByName(const char* tag, const char* ns, const char* cls, const char* method,
                       int argc, void* hook, void** orig) {
    if (!EnsureApi()) {
        LOGW("%s api not ready — %s will retry via Tick", tag, method);
        return false;
    }
    // ★2026-10-14 崩溃修复：运行时就绪闸门（详见 Il2CppRuntimeUp 注释）。
    //   TryInit 时刻 liblogic.so 刚 dlopen、il2cpp 尚未 init ⇒ 此处必须直接返回，
    //   由 Tick/OnFeatureChanged 在程序集加载完成后续装。
    if (!Il2CppRuntimeUp()) {
        static std::atomic<int> s_gateLogs{0};
        const int k = s_gateLogs.fetch_add(1);
        if (k < 6) LOGI("%s runtime not up — %s 延后安装（Tick 续装）", tag, method);
        return false;
    }
    auto& api = GetApi();
    void* k = FindClassNs(ns, cls);
    if (!k) k = FindClassShortLocal(cls);          // ★e4
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
    // HybridCLR/解释器方法的 methodPointer 可能是标记值（0x400000000000 这类），
    // 直接 DobbyHook 解引用即致命故障 ⇒ 必须先过 maps 级可执行段校验（WorMod 同款闸门）
    if (!Il2Cpp::IsExecutableRange(reinterpret_cast<uintptr_t>(fn), 16)) {
        LOGW("%s fn=%p %s.%s not in exec range — skip", tag, fn, cls, method);
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
// ④.5 运行时布局解析（按名找类 + 运行时字段偏移；全流程只读，绝不写游戏内存）
//   设计依据：同族（境界守望者 / liblogic.so + CSharpBattle.Battle.HpChangeEntitySystem）
//   已验收实现，见 10_docs/境界守望者/境界守望者_星光改造_功能阶段_20261003.md §3~§7。
//   · 类结构只依赖一个名字 CSharpBattle.Battle.HpChangeEntitySystem；
//     其余类按「短名 + 必需字段」双闸门确认（防 Entity 这类烂大街短名撞错类）。
//   · 字段偏移以 il2cpp_field_get_offset 的运行时真值为准（rt），
//     失败才保留兜底初值（cn）；DIFF 一律大声 log 并进对账表。
//   · 热路径（Hook_ExcuteOnce）只读 g_off_* 变量，绝不在此调 API。
//   · 解析失败 ⇒ 功能宁可不生效也不许乱写内存（写前另有 raw<0 闸门）。
//   产出对账表 /sdcard/Android/data/com.gf.cxswz/files/cxswz_tables.txt（adb pull 回看）；
//   另支持 /sdcard/Android/data/com.gf.cxswz/files/cxswz_cfg.txt 覆盖偏移（免重打包排障）。
// ============================================================
#define CXSWZ_FILES_DIR     "/sdcard/Android/data/com.gf.cxswz/files/"
#define CXSWZ_FILES_DIR_ALT "/storage/emulated/0/Android/data/com.gf.cxswz/files/"
#define CXSWZ_TBL_NAME      "cxswz_tables.txt"
#define CXSWZ_CFG_NAME      "cxswz_cfg.txt"

struct CxswzCfgOff { const char* key; int32_t* dst; };
static const CxswzCfgOff kCfgOffs[] = {
    {"off_com_hp",      &g_off_SYS_COM_HP},
    {"off_hdata",       &g_off_SYS_HDATA},
    {"off_lvalues",     &g_off_HPCOMP_LVALUES},
    {"off_hpd_fvalue",  &g_off_HPD_FVALUE},
    {"off_matk",        &g_off_SYS_MATK},
    {"off_fdamage",     &g_off_SYS_FDAMAGE},
    {"off_hpd_src",     &g_off_HPD_SRC},
    {"off_hpd_settle",  &g_off_HPD_SETTLE},
    {"off_hpd_show",    &g_off_HPD_SHOW},
    {"off_hpd_euid",    &g_off_HPD_ATK_EUID},
    {"off_hpd_cuid",    &g_off_HPD_ATK_CUID},
    {"off_hpd_unit",    &g_off_HPD_ATK_UNIT},
    {"off_hpd_skill",   &g_off_HPD_SKILL},
    {"off_hpd_formula", &g_off_HPD_FORMULA},
    {"off_entity_muid", &g_off_ENTITY_MUID},
};
static const int kCfgOffsN = static_cast<int>(sizeof(kCfgOffs) / sizeof(kCfgOffs[0]));

static void* g_kSys = nullptr;   // HpChangeEntitySystem
static void* g_kEnt = nullptr;   // Entity
static void* g_kComp = nullptr;  // ★e8：HPChangeComponent（S5 类布局 dump 用）
static void* g_kHpd = nullptr;   // ★e8：stHpChangeData（S5 类布局 dump 用）
static bool  g_layoutOk    = false;
static int   g_layoutRetry = 0;
static std::atomic<bool> g_layoutStop{false};  // ★e4：真实尝试用尽后永久停试（消卡顿）
static int64_t g_firstTryMs = 0;               // ★e4：首次真实尝试时刻（诊断耗时）

// —— 对账记录（WriteTables 用）——
struct OffRec { const char* what; int32_t cn; int32_t rt; bool found; };
static OffRec g_offRec[24];
static int    g_offRecN = 0;

static const char* ShortNameOf(const char* n) {
    if (!n) return "";
    const char* s = n;
    for (const char* p = n; *p; ++p)
        if (*p == '.' || *p == '/' || *p == '+') s = p + 1;
    return s;
}

static bool NameHasCI(const char* got, const char* needle) {
    if (!got || !needle) return false;
    const size_t nl = strlen(needle);
    if (!nl) return false;
    for (size_t i = 0; got[i]; i++) {
        size_t j = 0;
        while (j < nl) {
            char a = got[i + j], b = needle[j];
            if (!a) break;
            if (a >= 'A' && a <= 'Z') a = static_cast<char>(a - 'A' + 'a');
            if (b >= 'A' && b <= 'Z') b = static_cast<char>(b - 'A' + 'a');
            if (a != b) break;
            j++;
        }
        if (j == nl) return true;
    }
    return false;
}

// —— 时间戳（CLOCK_MONOTONIC, ms）：诊断节拍用 ——
static int64_t MsNow() {
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return (int64_t)ts.tv_sec * 1000 + ts.tv_nsec / 1000000;
}

/* assembly → image，带兜底。
 * 主路径 = 共享层 Api::ImageOf（直接读 Il2CppAssembly 首字段，避开必崩的导出）。
 * ★e4：主路径失败时兜底走官方导出 il2cpp_assembly_get_image。
 *   该导出在境界刀鸣会崩（那是壳的锅）；本作先过 IsValidHeapPtr + maps 可读闸门再调，
 *   仍是「读前守卫」语义，不会把进程带崩。how 回填 0=主路径 1=兜底。 */
static void* ImageOfAny(void* asmPtr, int* how) {
    if (how) *how = 0;
    if (!asmPtr) return nullptr;
    auto& api = GetApi();
    void* img = api.ImageOf(asmPtr);          // 内含 IsValidHeapPtr + IsReadableRange 守卫
    if (img) return img;
    if (!api.assembly_image) return nullptr;
    if (!Il2Cpp::IsValidHeapPtr(asmPtr) || !Il2Cpp::IsReadableRange((uintptr_t)asmPtr, 8))
        return nullptr;
    if (how) *how = 1;
    return const_cast<void*>(api.assembly_image(asmPtr));  // 导出返回 const void*
}

/* ★e4 本地短名扫描：共享层 FindClassByShortName 内部只走 Api::ImageOf，
 *   主路径一失效它就必然全 miss ⇒ 这里用 ImageOfAny 版本的等价扫描。
 *   只在「带命名空间的全名查找失败」时才走，命中即返回。 */
static void* FindClassShortLocal(const char* shortName) {
    if (!shortName) return nullptr;
    auto& api = GetApi();
    if (!api.image_class_count || !api.image_class || !api.class_name) return nullptr;
    if (!Il2CppRuntimeUp()) return nullptr;
    size_t an = 0;
    void** asms = nullptr;
    an = SafeAssemblies(&asms);
    if (!asms || an == 0) return nullptr;
    static std::atomic<int> s_logN{0};
    for (size_t a = 0; a < an; a++) {
        void* img = ImageOfAny(asms[a], nullptr);
        if (!img) continue;
        const uint32_t nc = api.image_class_count(img);
        if (nc == 0 || nc > 200000u) continue;
        const int li = s_logN.fetch_add(1);
        if (li < 12)
            LOGI(TPL_LOG " shortscan img#%zu=%p classes=%u want=%s", a, img, nc, shortName);
        for (uint32_t c = 0; c < nc; c++) {
            void* k = api.image_class(img, c);
            if (!k) continue;
            const char* nm = api.class_name(k);
            if (nm && NameHasCI(nm, shortName)) return k;
        }
    }
    return nullptr;
}

/* ★e6：image 名（Il2CppImage 首字段 = const char* name，纯读，已过 maps 闸门） */
static const char* ImageNameOf(void* img, char* buf, size_t cap) {
    if (!img || !buf || cap < 2) return nullptr;
    if (!Il2Cpp::IsReadableRange((uintptr_t)img, 8)) return nullptr;
    const char* nm = *reinterpret_cast<const char**>(img);
    if (!nm || !Il2Cpp::IsReadableRange((uintptr_t)nm, 1)) return nullptr;
    const size_t len = strnlen(nm, cap - 1);
    if (len == 0) return nullptr;
    memcpy(buf, nm, len);
    buf[len] = 0;
    return buf;
}

// 引擎/框架程序集（不含游戏类 ⇒ 扫荡时放第二遍，少调几十次）
static bool IsEngineImage(const char* nm) {
    if (!nm) return false;
    return NameHasCI(nm, "unityengine") || NameHasCI(nm, "system") || NameHasCI(nm, "mscorlib") ||
           NameHasCI(nm, "mono.") || NameHasCI(nm, "netstandard") || NameHasCI(nm, "nunit") ||
           NameHasCI(nm, "newtonsoft") || NameHasCI(nm, "unity.");
}

/* ★e7：游戏程序集「短名 → 类」索引（命名空间无关的按名查找）。
 *
 * 为什么必需 —— run6 实测（1.2.65.385.1，小米 96f0d883）：
 *   off SYS_COM_HP  cn=0x50 rt=0x50 match   / off SYS_HDATA cn=0x60 rt=0x60 match   ← kSys 命中
 *   off HPCOMP_LVALUES : no class/api — keep cn=0x20
 *   off HPD_FVALUE     : no class/api — keep cn=0x0    ← kComp / kHpd / kEnt 全 null
 * `ResolveRuntimeLayout` 对 kComp/kHpd/kEnt 传 ns=nullptr，而 SweepClassByExport 把 null
 * 归一成 ""（全局命名空间）⇒ 只有「定义在全局命名空间」的类才可能命中；而
 * HPChangeComponent / stHpChangeData / Entity 的真实命名空间未知（旧版走静态 RVA，
 * 从未按名解析过这三个类）⇒ 命名空间不对就必然 miss。
 * 索引只扫**非引擎**程序集（Assembly-CSharp / HybridCLR / ZGame.AOT …），一次建表，
 * 之后按短名（大小写不敏感）查，与 mustHaveField 双闸门配合即可定类。 */
static std::unordered_map<std::string, void*> g_shortIdx;
static std::atomic<bool> g_shortIdxDone{false};

static void EnsureGameShortIndex() {
    bool expected = false;
    if (!g_shortIdxDone.compare_exchange_strong(expected, true)) return;   // 只建一次
    auto& api = GetApi();
    if (!api.image_class_count || !api.image_class || !api.class_name) return;
    if (!Il2CppRuntimeUp()) { g_shortIdxDone.store(false); return; }        // 未就绪 → 下次再试
    size_t an = 0;
    void** asms = nullptr;
    an = SafeAssemblies(&asms);
    if (!asms || an == 0) { g_shortIdxDone.store(false); return; }
    size_t imgs = 0, cls = 0;
    for (size_t a = 0; a < an; a++) {
        void* img = ImageOfAny(asms[a], nullptr);
        if (!img) continue;
        char nb[64];
        const char* inm = ImageNameOf(img, nb, sizeof(nb));
        if (IsEngineImage(inm)) continue;          // 引擎程序集里不会有游戏战斗类
        const uint32_t nc = api.image_class_count(img);
        if (nc == 0 || nc > 200000u) continue;
        imgs++;
        for (uint32_t c = 0; c < nc; c++) {
            void* k = api.image_class(img, c);
            if (!k) continue;
            const char* nm = api.class_name(k);
            if (!nm) continue;
            if (!Il2Cpp::IsReadableRange((uintptr_t)nm, 1)) continue;
            const size_t len = strnlen(nm, 96);    // 有界读：野名字指针不许裸 strlen
            if (len == 0 || len >= 96) continue;
            char low[96];
            for (size_t i = 0; i < len; i++) {
                const char ch = nm[i];
                low[i] = (ch >= 'A' && ch <= 'Z') ? (char)(ch - 'A' + 'a') : ch;
            }
            std::string key(low, len);
            if (g_shortIdx.find(key) == g_shortIdx.end()) g_shortIdx.emplace(std::move(key), k);
            cls++;
        }
    }
    LOGI(TPL_LOG " short index: images=%zu classes=%zu keys=%zu", imgs, cls, g_shortIdx.size());
}

static void* FindClassByShortNameCI(const char* name) {
    if (!name || !*name) return nullptr;
    EnsureGameShortIndex();
    if (g_shortIdx.empty()) return nullptr;
    char low[96];
    size_t n = 0;
    for (; name[n] && n < sizeof(low) - 1; n++) {
        const char ch = name[n];
        low[n] = (ch >= 'A' && ch <= 'Z') ? (char)(ch - 'A' + 'a') : ch;
    }
    low[n] = 0;
    auto it = g_shortIdx.find(std::string(low, n));
    return it == g_shortIdx.end() ? nullptr : it->second;
}

/* ★e6：按名扫荡 —— 只调**裸导出** `il2cpp_class_from_name`（il2cpp 内部哈希查找，微秒级）。
 *  刻意不走 Api::ClassFromName：它未命中即降级到 ClassFromNameScan（逐类建 std::string 表），
 *  67 个程序集 = 67 次建表，实测在 il2cpp 就绪前把进程带崩（SIGABRT）。
 *  两遍：先游戏程序集，再引擎程序集；前两轮打逐图进度用于离线定位。 */
static std::atomic<int> s_sweepRounds{0};
static void* SweepClassByExport(const char* ns, const char* name) {
    if (!name) return nullptr;
    if (!ns) ns = "";                      // il2cpp 内部对 NULL 做 strcmp 会崩，必须给空串
    auto& api = GetApi();
    if (!api.class_from_name) return nullptr;
    if (!Il2CppRuntimeUp()) return nullptr;
    size_t an = 0;
    void** asms = nullptr;
    an = SafeAssemblies(&asms);
    if (!asms || an == 0) return nullptr;
    const bool verbose = s_sweepRounds.fetch_add(1) < 2;
    for (int pass = 0; pass < 2; pass++) {
        for (size_t a = 0; a < an; a++) {
            void* img = ImageOfAny(asms[a], nullptr);
            if (!img) continue;
            char nb[64];
            const char* inm = ImageNameOf(img, nb, sizeof(nb));
            const bool engine = IsEngineImage(inm);
            if ((pass == 0) == engine) continue;   // pass 0 = 先扫非引擎程序集
            if (verbose)
                LOGI(TPL_LOG " sweep #%zu img=%p %s ns='%s' name=%s",
                     a, img, inm ? inm : "?", ns, name);
            void* k = api.class_from_name(img, ns, name);
            if (k) {
                if (verbose) LOGI(TPL_LOG " sweep HIT #%zu %s -> %p", a, inm ? inm : "?", k);
                return k;
            }
        }
    }
    return nullptr;
}

// 按名找类；mustHaveField 非空时要求该类确实有该字段（双闸门，防短名撞错类）
static void* ClassByNames(const char* ns, const char* const* names, int n,
                          const char* mustHaveField) {
    auto& api = GetApi();
    if (!Il2CppRuntimeUp()) return nullptr;   // ★运行时就绪闸门（见上）
    for (int i = 0; i < n; i++) {
        void* k = nullptr;
        if (ns && *ns) {
            size_t an = 0;
            void** asms = nullptr;
            an = SafeAssemblies(&asms);
            /* ★e4 一次性诊断（全进程只打 3 次）：回答「assembly→image 为什么全 null」
             *   —— 这是「按名解析全 miss」的唯一断点：ImageOf 在打自己日志之前就 return 了。 */
            static std::atomic<int> s_diag{0};
            const int d = s_diag.fetch_add(1);
            if (d < 3) {
                void* a0 = (asms && an) ? asms[0] : nullptr;
                int how = 0;
                void* i0 = a0 ? ImageOfAny(a0, &how) : nullptr;
                const uintptr_t a0u = a0 ? starptr::Untag((uintptr_t)a0) : 0;
                const bool rd = a0 && Il2Cpp::IsReadableRange((uintptr_t)a0, 16);
                const bool rdu = a0u && Il2Cpp::IsReadableRange(a0u, 16);
                const uint64_t raw = rd ? *(const uint64_t*)a0 : 0;
                /* ★e5 版诊断：共享层 IsReadableRange 已内部 Untag（修 MTE 标签指针查 maps 恒 miss
                 *   ⇒ ImageOf 恒 null ⇒ 按名找类全 miss + 每拍 67 次 fopen 全量扫 maps 的 8~12s 卡顿）。
                 *   rd16u = 用去标签地址查（a0u 与 a0 只差 top-byte）应当恒为 1；a0u 直接暴露标签是否被剥掉。 */
                LOGI(TPL_LOG " diag asm(v5): n=%zu a0=%p a0u=%p rd16=%d rd16u=%d heap=%d raw0=%016llx "
                             "img=%p(how=%d) sym_asm_image=%p",
                     an, a0, (void*)a0u,
                     a0 ? (int)rd : -1, a0 ? (int)rdu : -1,
                     a0 ? (int)Il2Cpp::IsValidHeapPtr(a0) : -1,
                     (unsigned long long)raw, i0, how, (void*)api.assembly_image);
            }
            /* ★e6：扫荡改走裸导出（不再走 Api::ClassFromName 的降级建表）。
             *   命名空间不符时再按裸名扫一遍（廉价）；本作 class_from_name 未被污染。 */
            k = SweepClassByExport(ns, names[i]);
            if (!k) k = SweepClassByExport("", names[i]);
            /* ★e7：本作战斗类的真实命名空间（kSys 就是用 "CSharpBattle.Battle" 命中的）
             *   作为第三个候选；纯哈希查找，成本可忽略。 */
            if (!k) k = SweepClassByExport("CSharpBattle.Battle", names[i]);
        } else {
            k = SweepClassByExport("", names[i]);
            if (!k) k = SweepClassByExport("CSharpBattle.Battle", names[i]);
        }
        /* ★e7：命名空间未知/不符时的最终定类手段 —— 游戏程序集短名索引（命名空间无关）。
         *   直接解决 run6 的「HPCOMP_LVALUES / HPD_FVALUE / ENTITY_MUID 全 no class/api」。 */
        if (!k) k = FindClassByShortNameCI(names[i]);
        /* 全类名扫描是 O(类数) 重活 ⇒ 只在裸导出不可用（被壳污染）时才动用 */
        if (!k && !api.class_from_name) k = FindClassShortLocal(names[i]);
        if (!k && !api.class_from_name) k = api.FindClassByShortName(names[i]);
        if (!k) continue;
        if (mustHaveField && api.class_get_field && !api.class_get_field(k, mustHaveField)) {
            LOGW(TPL_LOG " class %s found but no field %s — skip", names[i], mustHaveField);
            continue;
        }
        LOGI(TPL_LOG " class %s -> %p", names[i], k);
        return k;
    }
    return nullptr;
}

// 按名取字段偏移；rt 不合理（>=0x800）或字段缺失 ⇒ 保留兜底 cn
static int32_t OffOf(void* k, const char* const* names, int n, int32_t cn,
                     const char* what, bool* found) {
    auto& api = GetApi();
    if (found) *found = false;
    if (!k || !api.class_get_field || !api.field_offset) {
        LOGW(TPL_LOG " off %-18s: no class/api — keep cn=0x%X", what, cn);
        return cn;
    }
    for (int i = 0; i < n; i++) {
        void* f = api.class_get_field(k, names[i]);
        if (!f) continue;
        const size_t raw = api.field_offset(f);
        if (raw >= 0x800) {
            LOGW(TPL_LOG " off %-18s: rt=0x%zX bogus (field %s) — keep cn=0x%X",
                 what, raw, names[i], cn);
            return cn;
        }
        const int32_t rt = static_cast<int32_t>(raw);
        if (found) *found = true;
        if (rt != cn) LOGI(TPL_LOG " off %-18s cn=0x%-3X rt=0x%-3X DIFF -> use rt", what, cn, rt);
        else          LOGI(TPL_LOG " off %-18s cn=0x%-3X rt=0x%-3X match", what, cn, rt);
        return rt;
    }
    LOGW(TPL_LOG " off %-18s: field miss — keep cn=0x%X", what, cn);
    return cn;
}

// —— 类名 / 字段名候选（含大小写与旧写法变体）——
static const char* const kSysNames[]  = {"HpChangeEntitySystem", "HPChangeEntitySystem",
                                         "HpChangeEntitySys", "HpChangeSystem"};
static const char* const kCompNames[] = {"HPChangeComponent", "HpChangeComponent",
                                         "HpComponent", "HPComponent"};
static const char* const kHpdNames[]  = {"stHpChangeData", "StHpChangeData",
                                         "HpChangeData", "stHpChangeInfo"};
static const char* const kEntNames[]  = {"Entity", "stEntity", "BattleEntity"};

static const char* const kFldComHp[]   = {"com_hp", "comHp", "ComHp", "comHP", "m_comHp"};
static const char* const kFldHData[]   = {"hData", "HData", "m_hData", "hdata"};
static const char* const kFldLValues[] = {"lValues", "LValues", "lvalues"};
static const char* const kFldFValue[]  = {"fValue", "FValue", "fvalue"};
static const char* const kFldMAtk[]    = {"mAtkEntity", "atkEntity", "m_atkEntity"};
static const char* const kFldFDamage[] = {"fDamage", "FDamage"};
static const char* const kFldSrc[]     = {"eDamageSourceType", "eSourceType", "srcType"};
static const char* const kFldSettle[]  = {"eDamageSettleType", "eSettleType", "settleType"};
static const char* const kFldShow[]    = {"eTextShowType", "textShowType", "eShowType"};
static const char* const kFldSkill[]   = {"iAttackSkillID", "iSkillID", "skillID"};
static const char* const kFldFormula[] = {"iFormularID", "iFormulaID", "formulaID"};
static const char* const kFldMuid[]    = {"<m_uID>k__BackingField", "m_uID", "uID"};

#define CXSWZ_NARR(a) (static_cast<int>(sizeof(a) / sizeof(a[0])))

// 解析完成度：com_hp / hData / lValues / fValue 四项全部按名命中才算 layoutOk
static bool ResolveRuntimeLayout() {
    if (g_layoutOk) return true;
    if (!EnsureApi()) { LOGW(TPL_LOG " layout: api not ready"); return false; }
    g_offRecN = 0;

    void* kSys = ClassByNames("CSharpBattle.Battle", kSysNames, CXSWZ_NARR(kSysNames), "hData");
    if (!kSys) kSys = ClassByNames(nullptr, kSysNames, CXSWZ_NARR(kSysNames), "hData");
    const char* how = kSys ? "by-name" : "miss";
    LOGI(TPL_LOG " layout: sys=%p how=%s", kSys, how);

    void* kComp = nullptr, *kHpd = nullptr, *kEnt = nullptr;
    if (kSys) {
        kComp = ClassByNames(nullptr, kCompNames, CXSWZ_NARR(kCompNames), "lValues");
        kHpd  = ClassByNames(nullptr, kHpdNames,  CXSWZ_NARR(kHpdNames),  "fValue");
        kEnt  = ClassByNames(nullptr, kEntNames,  CXSWZ_NARR(kEntNames),
                             "<m_uID>k__BackingField");
        // kEnt 只用探针日志的 uid；缺了不影响功能（ENTITY_MUID 保留兜底值）
    }

    bool fComHp = false, fHData = false, fLv = false, fFv = false;
    const int32_t cnComHp = g_off_SYS_COM_HP, cnHData = g_off_SYS_HDATA,
                  cnLv = g_off_HPCOMP_LVALUES, cnFv = g_off_HPD_FVALUE;
    // ★e10：stHpChangeData 全字段的 cn 侧（真实内联布局）锚点，供内联修正判据逐字段比对
    const int32_t cnSrc0 = g_off_HPD_SRC,  cnSettle0 = g_off_HPD_SETTLE,
                  cnShow0 = g_off_HPD_SHOW, cnSkill0 = g_off_HPD_SKILL,
                  cnFormula0 = g_off_HPD_FORMULA;

    g_off_SYS_COM_HP     = OffOf(kSys,  kFldComHp,   CXSWZ_NARR(kFldComHp),   cnComHp, "SYS_COM_HP",     &fComHp);
    g_off_SYS_HDATA      = OffOf(kSys,  kFldHData,   CXSWZ_NARR(kFldHData),   cnHData, "SYS_HDATA",      &fHData);
    g_off_SYS_MATK       = OffOf(kSys,  kFldMAtk,    CXSWZ_NARR(kFldMAtk),    g_off_SYS_MATK,   "SYS_MATK",   nullptr);
    g_off_SYS_FDAMAGE    = OffOf(kSys,  kFldFDamage, CXSWZ_NARR(kFldFDamage), g_off_SYS_FDAMAGE,"SYS_FDAMAGE",nullptr);
    g_off_HPCOMP_LVALUES = OffOf(kComp, kFldLValues, CXSWZ_NARR(kFldLValues), cnLv, "HPCOMP_LVALUES", &fLv);
    g_off_HPD_FVALUE     = OffOf(kHpd,  kFldFValue,  CXSWZ_NARR(kFldFValue),  cnFv, "HPD_FVALUE",     &fFv);
    g_off_HPD_SRC        = OffOf(kHpd,  kFldSrc,     CXSWZ_NARR(kFldSrc),     g_off_HPD_SRC,    "HPD_SRC",    nullptr);
    g_off_HPD_SETTLE     = OffOf(kHpd,  kFldSettle,  CXSWZ_NARR(kFldSettle),  g_off_HPD_SETTLE, "HPD_SETTLE", nullptr);
    g_off_HPD_SHOW       = OffOf(kHpd,  kFldShow,    CXSWZ_NARR(kFldShow),    g_off_HPD_SHOW,   "HPD_SHOW",   nullptr);
    g_off_HPD_SKILL      = OffOf(kHpd,  kFldSkill,   CXSWZ_NARR(kFldSkill),   g_off_HPD_SKILL,  "HPD_SKILL",  nullptr);
    g_off_HPD_FORMULA    = OffOf(kHpd,  kFldFormula, CXSWZ_NARR(kFldFormula), g_off_HPD_FORMULA,"HPD_FORMULA",nullptr);
    g_off_ENTITY_MUID    = OffOf(kEnt,  kFldMuid,    CXSWZ_NARR(kFldMuid),    g_off_ENTITY_MUID,"ENTITY_MUID",nullptr);

    // ============================================================
    // ★e10 内联布局修正 —— 1.2.65.385.1 真机内存窗口实证（run9 / D#0）
    //   现场：`hData`(self+0x60) 起的字节 0x00 = 0xfffffffffff62000 = -647168 = **-158.0**
    //        （Q20.12，与旧版可用的伤害编码完全一致）
    //   ⇒ 记录的 **fValue 在 +0x00**；而运行时报告的 stHpChangeData 字段偏移整体 +0x10
    //     （HybridCLR 热更程序集里的值类型按「装箱对象头」计偏移），AOT 侧内联用的却是真实布局。
    //   两条判据同时成立才修正，任一不成立则**安全失败**（不改落点，只打警告）：
    //     ① 六个字段 ct−cn 是同一个常量 shift 且 shift>0（整体平移，不是插字段）；
    //     ② 内联槽长 slotGap = SYS_MATK − SYS_HDATA 等于 cn 侧记录长度（0x78）。
    //   反证：按 rt 偏移记录长 0x88 > 槽 0x78 ⇒ 必越槽，不可能被内联 ⇒ rt 侧必偏。
    // ============================================================
    {
        const int32_t kHpdCnSpan = 0x78;   // cn 侧记录长度（fValue..iCreateBulletSkillID，对齐后）
        const int32_t rtFv = g_off_HPD_FVALUE;
        const int32_t sh   = rtFv - cnFv;
        const int32_t slotGap = g_off_SYS_MATK - g_off_SYS_HDATA;
        const bool uniform =
            (sh > 0) &&
            (g_off_HPD_SRC     - cnSrc0     == sh) &&
            (g_off_HPD_SETTLE  - cnSettle0  == sh) &&
            (g_off_HPD_SHOW    - cnShow0    == sh) &&
            (g_off_HPD_SKILL   - cnSkill0   == sh) &&
            (g_off_HPD_FORMULA - cnFormula0 == sh);
        if (uniform && slotGap == kHpdCnSpan) {
            g_off_HPD_FVALUE -= sh;
            g_off_HPD_SRC    -= sh;
            g_off_HPD_SETTLE -= sh;
            g_off_HPD_SHOW   -= sh;
            g_off_HPD_SKILL  -= sh;
            g_off_HPD_FORMULA-= sh;
            // 关联数据三件套：eRelationData 是内嵌 struct（12B，含 3 个 int32），从不按名解析，
            // 只用 cn 记录内相对位置（0x1C/0x20/0x24 已被 run9 窗口复核：4 / 1 / 2001 合理）
            g_off_HPD_ATK_EUID = g_off_HPD_FVALUE + 0x1C;
            g_off_HPD_ATK_CUID = g_off_HPD_FVALUE + 0x20;
            g_off_HPD_ATK_UNIT = g_off_HPD_FVALUE + 0x24;
            LOGI(TPL_LOG " hpd inline fix: 运行时偏移整体 +0x%X(值类型装箱头) slotGap=0x%X == cn 长度 0x%X"
                 " ⇒ 采用真实内联布局 fValue=0x%X src=0x%X settle=0x%X show=0x%X skill=0x%X formula=0x%X"
                 " atk=%X/%X/%X",
                 (unsigned)sh, (unsigned)slotGap, (unsigned)kHpdCnSpan,
                 (unsigned)g_off_HPD_FVALUE, (unsigned)g_off_HPD_SRC, (unsigned)g_off_HPD_SETTLE,
                 (unsigned)g_off_HPD_SHOW, (unsigned)g_off_HPD_SKILL, (unsigned)g_off_HPD_FORMULA,
                 (unsigned)g_off_HPD_ATK_EUID, (unsigned)g_off_HPD_ATK_CUID, (unsigned)g_off_HPD_ATK_UNIT);
        } else {
            LOGW(TPL_LOG " hpd inline fix: 未修正（uniform=%d shift=0x%X slotGap=0x%X cnSpan=0x%X）"
                 " — 记录偏移按运行时报告值使用",
                 (int)uniform, (unsigned)sh, (unsigned)slotGap, (unsigned)kHpdCnSpan);
        }
    }

    g_offRec[g_offRecN++] = {"SYS_COM_HP",     cnComHp, g_off_SYS_COM_HP,     fComHp};
    g_offRec[g_offRecN++] = {"SYS_HDATA",      cnHData, g_off_SYS_HDATA,      fHData};
    g_offRec[g_offRecN++] = {"HPCOMP_LVALUES", cnLv,    g_off_HPCOMP_LVALUES, fLv};
    g_offRec[g_offRecN++] = {"HPD_FVALUE",     cnFv,    g_off_HPD_FVALUE,     fFv};

    g_kSys = kSys;
    g_kEnt = kEnt;
    g_kComp = kComp;   // ★e8
    g_kHpd = kHpd;     // ★e8
    g_layoutOk = (kSys != nullptr) && fComHp && fHData && fLv && fFv;
    LOGI(TPL_LOG " layout: ok=%d how=%s com_hp=0x%X hData=0x%X lValues=0x%X fValue=0x%X",
         (int)g_layoutOk, how, g_off_SYS_COM_HP, g_off_SYS_HDATA,
         g_off_HPCOMP_LVALUES, g_off_HPD_FVALUE);
    if (!g_layoutOk) LOGW(TPL_LOG " layout: INCOMPLETE — 关键偏移退回兜底值，功能可能不生效");
    return g_layoutOk;
}

// cfg 覆盖（存在即生效，优先级最高；仅在解析之后应用 ⇒ 对账表仍显示 cn/rt 双方）
static void LoadCfgOverrides() {
    const char* cands[2] = {CXSWZ_FILES_DIR CXSWZ_CFG_NAME, CXSWZ_FILES_DIR_ALT CXSWZ_CFG_NAME};
    FILE* fp = nullptr;
    for (int i = 0; i < 2 && !fp; i++) fp = fopen(cands[i], "r");
    if (!fp) return;
    char line[256];
    int applied = 0;
    while (fgets(line, sizeof(line), fp)) {
        char* h = strchr(line, '#');
        if (h) *h = 0;
        char* eq = strchr(line, '=');
        if (!eq) continue;
        *eq = 0;
        char* k = line;
        while (*k == ' ' || *k == '\t') k++;
        char* ke = k + strlen(k);
        while (ke > k && (ke[-1] == ' ' || ke[-1] == '\t' || ke[-1] == '\r' || ke[-1] == '\n')) *--ke = 0;
        char* v = eq + 1;
        while (*v == ' ' || *v == '\t') v++;
        char* ve = nullptr;
        const long val = strtol(v, &ve, 0);
        if (ve == v) continue;
        for (int i = 0; i < kCfgOffsN; i++) {
            if (strcmp(k, kCfgOffs[i].key) == 0) {
                *kCfgOffs[i].dst = static_cast<int32_t>(val);
                LOGI(TPL_LOG " cfg %s=0x%lX", k, val);
                applied++;
                break;
            }
        }
    }
    fclose(fp);
    LOGI(TPL_LOG " cfg applied=%d", applied);
}

// 对账表落盘（诊断用；任何一次启动都写，adb pull 回来看）
/* ★e8：诊断工具（只读，仅用于离线判定结构漂移；不参与任何改写） */
static void HexWin(char* out, size_t cap, const void* p, int dwords) {
    if (!out || cap < 8) return;
    if (!p) { snprintf(out, cap, "(null)"); return; }
    size_t o = 0; out[0] = 0;
    for (int i = 0; i < dwords; i++) {
        if (o + 10 >= cap) break;
        const uint32_t v = reinterpret_cast<const uint32_t*>(p)[i];
        const int w = snprintf(out + o, cap - o, "%08x ", v);
        if (w <= 0) break;
        o += static_cast<size_t>(w);
    }
}

static const char* SafeStr(const char* p, char* buf, size_t cap) {
    if (!p || !buf || cap < 2) return "?";
    if (!Il2Cpp::IsReadableRange(reinterpret_cast<uintptr_t>(p), 1)) return "?";
    const size_t n = strnlen(p, cap - 1);
    if (n == 0 || n >= cap - 1) return "?";
    memcpy(buf, p, n); buf[n] = 0;
    return buf;
}

// ★e8：把一个类的全部字段（名/偏移/类型）写进 tables（离线判结构漂移的决定性依据）
static void DumpClassLayout(FILE* fp, const char* tag, void* k) {
    auto& api = GetApi();
    fprintf(fp, "--- %s = %p ---\n", tag, k);
    if (!k || !api.class_fields || !api.field_name || !api.field_offset) {
        fprintf(fp, "  (no class 或 class_fields/field_name/field_offset 不可用)\n");
        return;
    }
    void* iter = nullptr;
    int n = 0;
    for (;;) {
        void* f = api.class_fields(k, &iter);
        if (!f) break;
        char nb[96], tb[96];
        const char* fn = SafeStr(api.field_name(f), nb, sizeof(nb));
        const size_t off = api.field_offset(f);
        const char* tn = "?";
        if (api.field_type && api.type_name) {
            void* ft = api.field_type(f);
            if (ft) tn = SafeStr(api.type_name(ft), tb, sizeof(tb));
        }
        fprintf(fp, "  +0x%03zX  %-28s %s\n", off, fn, tn);
        if (++n >= 400) { fprintf(fp, "  (截断，>400 字段)\n"); break; }
    }
    fprintf(fp, "  (fields=%d)\n", n);
}

static void WriteTables() {
    char path[256] = {0};
    const char* cands[2] = {CXSWZ_FILES_DIR CXSWZ_TBL_NAME, CXSWZ_FILES_DIR_ALT CXSWZ_TBL_NAME};
    for (int i = 0; i < 2 && !path[0]; i++) {
        FILE* p = fopen(cands[i], "a");
        if (p) { fclose(p); snprintf(path, sizeof(path), "%s", cands[i]); }
    }
    if (!path[0]) { LOGW(TPL_LOG " tables: no writable path errno=%d", errno); return; }
    FILE* fp = fopen(path, "w");
    if (!fp) { LOGW(TPL_LOG " tables: fopen %s fail errno=%d", path, errno); return; }

    auto& api = GetApi();
    fprintf(fp, "build=%s  lib=%s  layoutOk=%d\n", CXSWZ_BUILD_TAG, TPL_TARGET_LIB, (int)g_layoutOk);
    fprintf(fp, "kSys=%p  kEnt=%p\n", g_kSys, g_kEnt);
    fprintf(fp, "api class_from_name=%p class_get_field=%p field_offset=%p class_fields=%p field_name=%p\n",
            (void*)api.class_from_name, (void*)api.class_get_field, (void*)api.field_offset,
            (void*)api.class_fields, (void*)api.field_name);

    fprintf(fp, "===== S3 reconcile (cn = 1.2.56.375.3 兜底, rt = 运行时真值) =====\n");
    for (int i = 0; i < g_offRecN; i++) {
        const OffRec& r = g_offRec[i];
        fprintf(fp, "off %-16s cn=0x%-3X rt=0x%-3X %s\n", r.what, r.cn, r.rt,
                !r.found ? "field-miss" : (r.rt == r.cn ? "match" : "DIFF"));
    }
    fprintf(fp, "===== S4 non-critical offsets (final used) =====\n");
    fprintf(fp, "off SYS_MATK=0x%X SYS_FDAMAGE=0x%X MUID=0x%X SRC=0x%X SETTLE=0x%X SHOW=0x%X SKILL=0x%X FORMULA=0x%X\n",
            g_off_SYS_MATK, g_off_SYS_FDAMAGE, g_off_ENTITY_MUID,
            g_off_HPD_SRC, g_off_HPD_SETTLE, g_off_HPD_SHOW,
            g_off_HPD_SKILL, g_off_HPD_FORMULA);

    /* ★e8：S5 类布局 dump —— 运行 1.2.65.385.1 后 stHpChangeData 全字段整体 +0x10
     * （旧 cn 表：fValue 0x00/src 0x08/settle 0x0C/show 0x10/skill 0x28/formula 0x2C），
     * 但 fValue 实际值不是旧的「负 Fix64 伤害」⇒ 必须拿到字段名/顺序/类型才能定性。 */
    fprintf(fp, "===== S5 class layout =====\n");
    DumpClassLayout(fp, "kSys/HpChangeEntitySystem", g_kSys);
    DumpClassLayout(fp, "kComp/HPChangeComponent",   g_kComp);
    DumpClassLayout(fp, "kHpd/stHpChangeData",       g_kHpd);
    DumpClassLayout(fp, "kEnt/Entity",               g_kEnt);

    fprintf(fp, "===== S1 images =====\n");
    size_t an = 0;
    void** asms = nullptr;
    an = SafeAssemblies(&asms);
    if (!asms) {
        fprintf(fp, "assemblies=null\n");
    } else {
        for (size_t i = 0; i < an; i++) fprintf(fp, "img#%zu %p\n", i, api.ImageOf(asms[i]));
        fprintf(fp, "assemblies=%zu\n", an);
    }

    // S2：只在主类没找到时才跑（有界扫描，命中即报；用于离线定位改名）
    if (!g_kSys) {
        fprintf(fp, "===== S2 HyChange-class scan (sys class miss) =====\n");
        int hits = 0, scanned = 0;
        for (size_t i = 0; asms && i < an && scanned < 60000; i++) {
            void* img = api.ImageOf(asms[i]);
            if (!img || !api.image_class_count || !api.image_class || !api.class_name) continue;
            const uint32_t n = api.image_class_count(img);
            if (n > 200000) continue;
            for (uint32_t c = 0; c < n && scanned < 60000; c++, scanned++) {
                void* k = api.image_class(img, c);
                if (!k) continue;
                const char* cn = api.class_name(k);
                if (!cn) continue;
                const char* sn = ShortNameOf(cn);
                if (NameHasCI(sn, "hpchange") || NameHasCI(sn, "changehp") ||
                    NameHasCI(sn, "hpcomponent")) {
                    fprintf(fp, "img#%zu cls=%s (%p)\n", i, sn, k);
                    hits++;
                }
            }
        }
        fprintf(fp, "scan classes=%d hits=%d\n", scanned, hits);
    }
    fflush(fp);
    fclose(fp);
    LOGI(TPL_LOG " tables written %s", path);
}

// ============================================================
// ⑤ 探针挂点（只读）：HpChangeEntitySystem.ExcuteOnce
//    ExcuteAll 循环把 com_hp.lValues[i] 拷到 hData 后调 ExcuteOnce；
//    故 hook 入口处 self+g_off_SYS_HDATA = 当前这条 stHpChangeData（含 fValue/AtkRelationData）。
//    本阶段只 LOGI 读值，不改任何内存（hook-rules §22 步 2：只读计数验证）。
// ============================================================
typedef bool (*HasComponentFn)(void* entity, int type, void* methodInfo);
typedef bool (*ExcuteOnceFn)(void* self, void* entity);
static HasComponentFn g_hasComponent = nullptr;
static ExcuteOnceFn  orig_ExcuteOnce = nullptr;

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

    // ★2026-10-14 安全闸门：运行时布局未解析成功时**绝不用兜底偏移改写内存**。
    //   兜底偏移来自 1.2.56.375.3 实测表；若目标版本结构漂移，按错偏移写 = 花屏/崩坏。
    //   宁可不生效也不许写坏（用户可见的是「没效果」，不是「闪退」）。
    if (!g_layoutOk) {
        static std::atomic<int> s_warned{0};
        if (s_warned.fetch_add(1) < 3) {
            LOGW(TPL_LOG " hotpath: layout not ok — 跳过改写（功能暂不生效）");
        }
        g_reenter.store(false);
        return orig_ExcuteOnce(self, entity);
    }

    if ((wantInv || mul > 1 || wantLog) && OkPtr(self)) {
        const int n = wantLog ? (g_probeCount.fetch_add(1) + 1) : 0;
        uint8_t* sys = reinterpret_cast<uint8_t*>(self);
        uint8_t* hd  = sys + g_off_SYS_HDATA;   // 热路径只读变量，绝不调 API

        int64_t  raw = 0;
        int32_t  src = 0, settle = 0, show = 0, skill = 0, formula = 0;
        uint32_t atkE = 0, atkC = 0;
        int32_t  atkU = 0;
        memcpy(&raw,     hd + g_off_HPD_FVALUE,     sizeof(raw));
        memcpy(&src,     hd + g_off_HPD_SRC,        4);
        memcpy(&settle,  hd + g_off_HPD_SETTLE,     4);
        memcpy(&show,    hd + g_off_HPD_SHOW,       4);
        memcpy(&skill,   hd + g_off_HPD_SKILL,      4);
        memcpy(&formula, hd + g_off_HPD_FORMULA,    4);
        memcpy(&atkE,    hd + g_off_HPD_ATK_EUID,   4);
        memcpy(&atkC,    hd + g_off_HPD_ATK_CUID,   4);
        memcpy(&atkU,    hd + g_off_HPD_ATK_UNIT,   4);
        const int64_t rawIn = raw;   // 改写前的原始值（日志用）

        void* comHp = nullptr;
        memcpy(&comHp, sys + g_off_SYS_COM_HP, sizeof(comHp));
        int32_t sz = -1;
        void* listData = nullptr;   // e8：外提，供结构 dump 用（只读）
        if (OkPtr(comHp)) {
            void* lv = nullptr;
            memcpy(&lv, reinterpret_cast<uint8_t*>(comHp) + g_off_HPCOMP_LVALUES, sizeof(lv));
            void* data = nullptr;
            if (ReadListData(lv, &data, &sz)) { listData = data; }
        }

        int32_t mon = -1, twr = -1, cs = -1;
        uint32_t euid = 0;
        if (OkPtr(entity)) {
            memcpy(&euid, reinterpret_cast<uint8_t*>(entity) + g_off_ENTITY_MUID, 4);
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

        // ============================================================
        // e10 收官验证（只读）：内联修正后 fValue 必须读成「负的 Q20.12 伤害」。
        //   e9 实证：hd+0x00 = 0xfffffffffff62000 = -158.0 ⇒ 修正后 raw 应为负值。
        //   若这里又出现 0x100000001 这类「两枚举拼成的正数」，说明修正被跳过（看上面的
        //   `hpd inline fix: 未修正` 警告）。仅前 3 次调用打印，之后静默。
        // ============================================================
        static std::atomic<int> s_dumpN{0};
        const int dn = s_dumpN.fetch_add(1);
        if (dn < 3) {
            char hdHex[240];
            HexWin(hdHex, sizeof(hdHex), hd, 16);
            const int64_t V0   = *reinterpret_cast<const int64_t*>(hd);
            const int64_t Vinl = *reinterpret_cast<const int64_t*>(hd + g_off_HPD_FVALUE);
            LOGI(TPL_LOG " D#%d hd=%p raw(@0x%X)=0x%llx V0=0x%llx Vinl=0x%llx sz=%d",
                 dn, hd, (unsigned)g_off_HPD_FVALUE, (unsigned long long)rawIn,
                 (unsigned long long)V0, (unsigned long long)Vinl, sz);
            LOGI(TPL_LOG " D#%d hd[%s]", dn, hdHex);
            LOGI(TPL_LOG " D#%d list=%p sz=%d", dn, listData, sz);
            // ★e9：此处原有 compMask(40..70) 枚举扫描，已删除 ——
            //   `Entity.HasComponent(entity, v)` 走 `_componentPools[type]` 按枚举下标索引的数组，
            //   越界枚举值 = 越界读 ⇒ SIGSEGV（run8 实测：pid 5596 打怪瞬间 signal 11 闪退，
            //   而 hook 日志一行未出，证明崩在探针里）。枚举值已由 mon/twr/cs 分类结果反证无需扫描。
        }

        // ---- 无敌：我方单位（Tower/CardSoldier）承伤清零 ----
        if (wantInv && raw < 0 && isFriendly) {
            const int64_t zero = 0;
            memcpy(hd + g_off_HPD_FVALUE, &zero, sizeof(zero));
            const int m = g_modCount.fetch_add(1);
            if (m < MOD_LOG_MAX)
                LOGI(TPL_LOG " INV e=%p uid=%u raw=%lld->0", entity, euid, (long long)rawIn);
        }
        // ---- 伤害倍率：对敌（MonsterSign）伤害放大 ----
        else if (mul > 1 && raw < 0 && mon == 1) {
            const int64_t scaled = ScaleFix64(raw, mul);
            memcpy(hd + g_off_HPD_FVALUE, &scaled, sizeof(scaled));
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

// ⚠️ rva 置 0：静态 RVA 只对 1.2.56.375.3 有效，换包后由按名路径接管
static const HookSlot kSlots[] = {
    {"ExcuteOnce", 0u, 0u,
     "CSharpBattle.Battle", "HpChangeEntitySystem", "ExcuteOnce", 1,
     (void*)Hook_ExcuteOnce, (void**)&orig_ExcuteOnce},
};
static const size_t kSlotsCount = sizeof(kSlots) / sizeof(kSlots[0]);

// 解析 HasComponent 的运行时地址：按名取 Entity.HasComponent(EComponentType) 的
// MethodInfo.methodPointer（失败则 mon/twr/cs 全 -1，功能不生效但不闪退）
static void ResolveProbeHelpers() {
    if (g_hasComponent) { LOGI(TPL_LOG " probe helpers hasComponent=%p", (void*)g_hasComponent); return; }
    if (!EnsureApi()) return;
    if (!g_kEnt) {
        g_kEnt = ClassByNames(nullptr, kEntNames, CXSWZ_NARR(kEntNames),
                              "<m_uID>k__BackingField");
    }
    if (g_kEnt) {
        auto& api = GetApi();
        // GetMethod 是成员函数（非可空指针），EnsureApi 已在上面把关
        void* mi = api.GetMethod(g_kEnt, "HasComponent", 1);
        if (mi) {
            void* fn = *reinterpret_cast<void**>(mi);
            // HybridCLR/解释器方法的 methodPointer 可能是标记值，必须过可执行段校验
            if (OkPtr(fn) && Il2Cpp::IsExecutableRange(reinterpret_cast<uintptr_t>(fn), 16))
                g_hasComponent = reinterpret_cast<HasComponentFn>(fn);
            else
                LOGW(TPL_LOG " HasComponent fn=%p not in exec range — skip", fn);
        } else {
            LOGW(TPL_LOG " HasComponent miss — mon/twr/cs 恒 -1（无敌/倍率将不生效）");
        }
    }
    LOGI(TPL_LOG " probe helpers hasComponent=%p", (void*)g_hasComponent);
}

// ============================================================
// ⑥ 分组安装 + 懒装（功能阶段按功能组拆；开关全关 = 0 hook）
// ============================================================
static void InstallBattleHooks() {
    const int64_t t0 = MsNow();
    for (size_t i = 0; i < kSlotsCount; i++) {
        if (HookSlotInstall(kSlots[i])) g_anyHooked.store(true);
    }
    if (orig_ExcuteOnce) g_probeReady.store(true);
    LOGI(TPL_LOG " timing hooks: install=%lldms hooked=%d", (long long)(MsNow() - t0),
         g_anyHooked.load() ? 1 : 0);
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    if (kSlotsCount == 0) return;   // 无挂点表 = 不安装
    // 已装上且开关全关 = 无需再做事（原逻辑每拍都重进 InstallBattleHooks）
    if (g_anyHooked.load() && !g_inv.load() && g_dmgMul.load() <= 1) return;
    /* ★e4 卡死修复：HookByName 内部同样要调 il2cpp 导出（实测单次以秒计）⇒
     *   ① 按名路径已被判死（g_layoutStop）后不再周期性锤 il2cpp；
     *   ② 未判死时也至少 2000ms 才重试一次（原为每 200ms 一次，是菜单卡顿的主因之一）。 */
    if (g_layoutStop.load()) return;
    static std::atomic<int64_t> s_lastMs{0};
    const int64_t now = MsNow();
    if (now - s_lastMs.load() < 2000) return;
    s_lastMs.store(now);
    InstallBattleHooks();
}

// 懒装：库就绪后解析运行时布局（失败则按 Tick 节奏有限重试，绝不阻塞主线程）
// 解析成功才发现一次对账表 ⇒ adb pull cxswz_tables.txt 即可离线复核偏移
static void EnsureLayout() {
    if (!g_libReady.load()) return;
    if (g_layoutOk) return;
    /* ★e4 卡死修复：本函数会调 il2cpp 导出，实测**单次耗时以秒计**（见下 timing 日志），
     *   且 OnFeatureChanged 会在 Java 主线程同步调它 ⇒ 曾造成菜单 3.8s / 点击 13.4s 延迟。
     *   三重约束：① 只在后台 Tick 调用（主线程回调已改为只置标志）；
     *            ② 节流放宽到 2000ms（原 500ms 相当于每拍都烧几秒）；
     *            ③ 真实尝试上限 12 次（原 120 次 = 十分钟的周期性卡顿），
     *               到顶后永久停止重试并大声 log 一次。 */
    if (g_layoutStop.load()) return;
    static std::atomic<int64_t> s_lastMs{0};
    const int64_t nowMs = MsNow();
    const int64_t lastMs = s_lastMs.load();
    if (lastMs && nowMs - lastMs < 2000) return;
    s_lastMs.store(nowMs);
    const int64_t t0 = nowMs;
    if (!EnsureApi()) {
        g_layoutRetry++;
        if (g_layoutRetry <= 3 || (g_layoutRetry % 50) == 0)
            LOGW(TPL_LOG " layout: api load fail (try %d)", g_layoutRetry);
        return;
    }
    const int64_t t1 = MsNow();
    InstallInitHook();     // ★e6：幂等补装（api 就绪后若还没 hook 上）
    // ★2026-10-14 崩溃修复：程序集尚未加载 ⇒ 立刻返回，绝不触碰任何 il2cpp 导出。
    //   本闸门**不消耗** g_layoutRetry 配额（探针只有几条指令，可无限续期），
    //   以保证运行时就绪后仍有完整尝试机会。
    if (!Il2CppRuntimeUp()) {
        static std::atomic<int> s_gateLogs{0};
        const int k = s_gateLogs.fetch_add(1);
        if (k < 8)
            LOGI(TPL_LOG " layout: waiting il2cpp runtime (probe %d) — 程序集未加载，"
                          "init 之前绝不调 il2cpp API", k + 1);
        return;
    }
    const int64_t t2 = MsNow();
    if (g_layoutRetry >= 12) {
        g_layoutStop.store(true);
        LOGE(TPL_LOG " layout: 放弃按名解析（%d 次真实尝试全部失败，累计约 %llds）— "
                      "停止重试以消除周期性卡顿；功能将不生效，需离线定位 ImageOf 断点",
             g_layoutRetry, (long long)((nowMs - g_firstTryMs) / 1000));
        return;
    }
    if (!g_firstTryMs) g_firstTryMs = nowMs;
    g_layoutRetry++;
    const bool ok = ResolveRuntimeLayout();
    const int64_t t3 = MsNow();
    LOGI(TPL_LOG " timing layout: api=%lldms gate=%lldms resolve=%lldms total=%lldms ok=%d try=%d",
         (long long)(t1 - t0), (long long)(t2 - t1), (long long)(t3 - t2),
         (long long)(t3 - t0), (int)ok, g_layoutRetry);
    if (!ok) return;
    LoadCfgOverrides();        // 优先级最高：解析之后应用（免重打包排障）
    ResolveProbeHelpers();
    WriteTables();
    LOGI(TPL_LOG " timing layout: post=%lldms", (long long)(MsNow() - t3));
}

// ============================================================
// ⑦ 对外接口（ModEntry.h 注册表契约，勿改签名）
// ============================================================
bool Cxswz_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;
    g_libReady.store(true);
    /* ★e6：库一映射就抢先 hook il2cpp_init（此刻 Unity 还没起引擎，正常都在 init 之前）。
     *   这是「运行时真正就绪」的判据；晚一步也不会坏事（Il2CppRuntimeUp 有时间兜底）。 */
    InstallInitHook();
    EnsureLayout();        // ① 运行时布局解析（失败不致命，Tick 会继续重试）
    InstallBattleHooks();  // ② 无条件挂 ExcuteOnce（开关全关时等同只读探针，零改写）
    LOGI(TPL_LOG " lib ready (liblogic.so, feature stage) hooked=%d layoutOk=%d tag=%s",
         g_anyHooked.load() ? 1 : 0, (int)g_layoutOk, CXSWZ_BUILD_TAG);
    return true;
}

bool Cxswz_IsReady() { return g_libReady.load(); }

void Cxswz_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == CXSWZ_FEAT_INV) {
        g_inv.store(enabled);
        LOGI(TPL_LOG " 无敌 = %d", (int)enabled);
    }
    /* ★e4 卡死修复（关键）：本回调由 Java **主线程**经 JNI 调进来。
     *   原实现同步调 EnsureLayout/SyncHooks，而这两个函数会调 il2cpp 导出、
     *   实测单次耗时以秒计 ⇒ 主线程被卡 3.8s（菜单消息）/ 13.4s（点击）。
     *   现改为只置开关标志，全部重活交后台 Tick（200ms 一拍，节流 2000ms）异步做。 */
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
    /* ★e4：本拍耗时诊断（这是「菜单卡死」的根因量化点）。
     *   门限 100ms 才打，且限流 40 条，不污染日志。 */
    const int64_t t0 = MsNow();
    EnsureLayout();
    SyncHooks();
    const int64_t cost = MsNow() - t0;
    if (cost >= 100) {
        static std::atomic<int> s_slowLogs{0};
        const int n = s_slowLogs.fetch_add(1);
        if (n < 40)
            LOGW(TPL_LOG " timing tick: 本拍阻塞 %lldms（layout/hook 走 il2cpp 导出，"
                          "已尽量移出主线程）", (long long)cost);
    }
}

void Cxswz_ForceClear() {
    g_dmgMul.store(1);
    g_inv.store(false);
}
