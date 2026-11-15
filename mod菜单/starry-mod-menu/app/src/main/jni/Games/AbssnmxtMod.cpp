// ============================================================================
//  阿比斯少年冒险团（abssnmxt）— 星光菜单游戏模块（数据面，零 inline hook）
//
//  ★ v1.1.1 解析层（2026-11-15 三份 tombstone 归因后重写）
//  归因：pc 0x2f91348 在 libil2cpp 内部，紧贴两处 bl <abort@plt>，x1 指向
//        "IL2CPP needs to know about the array type at compile time..." ⇒
//        **我们触发了 il2cpp 内部 fatal abort**（不是我们 SEGV，也不是 Dobby）。
//        最可疑的是遍历 assemblies 数组 / 任何让运行时构造托管数组或字符串的调用。
//
//  因此本版解析层只 dlsym 8 个符号，一个不多：
//    il2cpp_domain_get / il2cpp_thread_attach / il2cpp_domain_assembly_open /
//    il2cpp_assembly_get_image / il2cpp_class_from_name /
//    il2cpp_class_get_field_from_name / il2cpp_field_get_offset /
//    il2cpp_field_static_get_value
//  硬禁令：不用 il2cpp_domain_get_assemblies（它在造数组）、不用 il2cpp_runtime_invoke、
//          不读 assemblies 数组、不调任何可能构造托管对象的 API。
//
//  句柄：等 /proc/self/maps 出现 libil2cpp.so → 再等 3 秒（避开半初始化）→
//        domain_get 连续两次返回同一非空值 → dlopen(RTLD_NOW|RTLD_NOLOAD) 取已有句柄，
//        失败才 dlopen(RTLD_NOW) 并打 handle=newload 警告。
//  每个符号校验落点 ∈ libil2cpp 合并区间 + 4 字节对齐（**不要求可执行段**：本机 houdini 转译，\n//  libil2cpp 只有 r--p/rw-p，PF_X 判据恒假；首字合理性仅作辅助）；
//        越界即 sym_reject <name> 并整体 abort（安静降级，进程必须活着）。
//  每步先打日志再调用：api step=1 dlsym → 2 domain_get → 3 thread_attach →
//        4 assembly_open → 5 image → 6 class Unit → 7 fields → unit_table → unit_count。
//
//  功能（四项，各自独立）：
//    无敌     我方 faction==1 的 Unit：curHp(+0x190) ← maxHp(+0x1A0)
//    伤害倍率 敌方 faction==2 补扣：Δ>0 时 cur -= (mult-1)*Δ
//    无限技能 v1.1 只打 pending（需要 runtime_invoke，本版禁用）
//    移动速度 v1.1 只探针（pending-anchor）
//
//  编译期：-DSTARRY_PROBE=1 ⇒ 探针版（只解析+日志，绝不写内存）
// ============================================================================

#include "AbssnmxtMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <ctime>

#include <dlfcn.h>
#include <mutex>
#include <setjmp.h>
#include <signal.h>

#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"

#define ABS_LOG "[Abssnmxt]"

#if defined(STARRY_PROBE) || defined(STARRY_HOOK_PROBE)
static const bool kProbeMode = true;      // 纯验证版：绝不写内存
#else
static const bool kProbeMode = false;
#endif


// ============================================================================
//  ★ 可配置区（改地址/数值只改这里，别动下面的实现）
//    一局验证用：给定 (tag, RVA, kind) 就挂上并按 kind 打日志
//      kind 0 = int 探针（调原函数、打 ret、返回原值）
//      kind 1 = float 探针
//      kind 2 = 强制返回（不调原函数，恒返回 forceRet）
//      kind 3 = 施放标记（void，打第一个 int 参数）
// ============================================================================
struct HookCfg { const char* tag; uintptr_t rva; int kind; int forceRet; };

// 现有探针（v1.9.4 起）：KCPGOACDHOB 技能/动作驱动器
static const HookCfg kHookCfg[] = {
    // v2.0 正式版：只留真正生效的两组
    {"energygate", 0x3006D98, 4, 0xE8},   // kind4：ActionButton+0xE8=1（能量门禁恒过）
    {"skidA",      0x35546B0, 2, 1},      // IAGGDOCLNND::OIKKANJOLNC（static bool）
    {"skidC",      0x3608534, 2, 1},      // IDNHEGOKGNA::OIKKANJOLNC
    {"skidD",      0x349EB1C, 2, 1},      // PFNDCCBPLGI::OIKKANJOLNC
    {"skidE",      0x359516C, 2, 1},      // POBKCDEHPNK::OIKKANJOLNC
    {"skidF",      0x35FD1E8, 2, 1},      // IDNHEGOKGNA::KFFALNFHHJL（含 float 参数）
    {"skidB",      0x35547E4, 8, 0},      // FEJHANBJNCK（void：直接返回，不写冷却）
};

// 技能属性写入（v1.9.5 已证明写入成功且不被覆盖，留作对照，不再用于解释技能效果）
static const int kEnergyAdd  = 100000;   // values[48] 绝对加值
static const int kEnergyRate = 50000;    // values[49] 万分比（50000 ⇒ ×6）
static const int kCdrVal     = 100;      // values[51]/[52]/[57]
static const int kDiscountV  = 100;      // values[17]（无读取点，对照）
static const int kSkillGrpIdsCfg[6] = {51, 52, 57, 48, 49, 17};

// 数据面 CD 占位（0 = 未定，只打 skip）
static const uint32_t kOffSkillCd   = 0;
static const bool     kForceClearCd = true;

// 当前解析步骤（0=未开始）：fault guard 用它指认崩溃点
static std::atomic<int> g_step{0};
#define STEP_LOG(n, what) do { g_step.store(n); \
    LOGI(ABS_LOG " api step=" #n " " what); } while (0)

// ============================================================================
//  ① 开关与数值
// ============================================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_god{false};
static std::atomic<int>  g_dmgX{1};
static std::atomic<bool> g_skill{false};
static std::atomic<int>  g_speedX{1};

static const int kDmgXMin = 1,  kDmgXMax = 100;
static const int kSpeedXMin = 1, kSpeedXMax = 10;
static int ClampInt(int v, int lo, int hi) { return v < lo ? lo : (v > hi ? hi : v); }

namespace {

// ============================================================================
//  ② 锚点（dump.cs 实测；运行期 by-name 重取并交叉核对）
// ============================================================================
constexpr size_t kDumpOffState   = 0x120;   // LAGPMHKOAKN
constexpr size_t kDumpOffCC      = 0xA0;    // m_CharacterController
constexpr size_t kDumpOffFaction = 0x150;   // GPJLPFOKIDE (NONE=0 PLAYER=1 ENEMY=2)
constexpr size_t kDumpOffHpCur   = 0x190;   // DEPKEGHAHMB (long)
constexpr size_t kDumpOffHpMax   = 0x1A0;   // LAMEKJABEAP (long)
constexpr size_t kDumpOffCount   = 0x198;   // APPACLOOHNK (int) —— v1.5 血量候选
constexpr size_t kDumpOffHp198   = 0x198;   // 同上（语义名，v1.5 专用）
constexpr size_t kDumpOffMask    = 0x20;    // KDOFIAPPOGI
constexpr size_t kDumpOffDriver  = 0x38;    // GAMJINGNDHM : KPBKFHOKGHJ（移速驱动器）
constexpr int    kFactionPlayer  = 1;
constexpr int    kFactionEnemy   = 2;

// Dictionary<int,Unit> 固定布局（本版无 object_class，不改写、只读）
constexpr size_t kDictOffEntries = 0x18;    // entries (Il2CppArray*)
constexpr size_t kDictOffCount   = 0x20;    // count (int)
constexpr size_t kEntryStride    = 24;      // {int hashCode; int next; int key; Unit* value}
constexpr size_t kEntryOffValue  = 16;
constexpr size_t kArrayDataOff   = 0x20;

// ============================================================================
//  ③ 时间与 maps 工具
// ============================================================================
static uint64_t NowMs() {
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return (uint64_t)ts.tv_sec * 1000ull + (uint64_t)ts.tv_nsec / 1000000ull;
}

struct MapRange { uintptr_t s, e; unsigned char perm; };
static MapRange g_maps[4096];
static int      g_mapsN = 0;
static uint64_t g_mapsAt = 0;

static void RefreshMaps(bool force) {
    uint64_t now = NowMs();
    if (!force && g_mapsN > 0 && now - g_mapsAt < 1000) return;
    FILE* fp = std::fopen("/proc/self/maps", "r");
    if (!fp) return;
    char line[512];
    int n = 0;
    while (n < (int)(sizeof(g_maps) / sizeof(g_maps[0])) && std::fgets(line, sizeof(line), fp)) {
        uintptr_t s = 0, e = 0;
        char perm[8] = {};
        if (std::sscanf(line, "%lx-%lx %7s", &s, &e, perm) != 3) continue;
        unsigned char pm = 0;
        if (perm[0] == 'r') pm |= 1;
        if (perm[1] == 'w') pm |= 2;
        if (perm[2] == 'x') pm |= 4;
        g_maps[n].s = s; g_maps[n].e = e; g_maps[n].perm = pm;
        ++n;
    }
    std::fclose(fp);
    g_mapsN = n;
    g_mapsAt = now;
}

static bool RangePerm(uintptr_t addr, size_t len, unsigned char wantPerm) {
    if (!addr) return false;
    for (int pass = 0; pass < 2; ++pass) {
        RefreshMaps(pass == 1);
        for (int i = 0; i < g_mapsN; ++i) {
            if (addr >= g_maps[i].s && addr + len <= g_maps[i].e) {
                return wantPerm == 0 || (g_maps[i].perm & wantPerm) == wantPerm;
            }
        }
    }
    return false;
}

static bool LibRange(uintptr_t* base, uintptr_t* end) {
    FILE* fp = std::fopen("/proc/self/maps", "r");
    if (!fp) return false;
    char line[512];
    uintptr_t lo = 0, hi = 0;
    while (std::fgets(line, sizeof(line), fp)) {
        if (!std::strstr(line, "libil2cpp.so")) continue;
        uintptr_t s = 0, e = 0;
        if (std::sscanf(line, "%lx-%lx", &s, &e) != 2) continue;
        if (!lo || s < lo) lo = s;
        if (e > hi) hi = e;
    }
    std::fclose(fp);
    if (!lo || hi <= lo) return false;
    *base = lo; *end = hi;
    return true;
}

static bool OkPtr(void* p) {
    uintptr_t a = (uintptr_t)p;
    if (!a || (a & 7)) return false;
    return RangePerm(a, 8, 1);
}

static int64_t  ReadI64(void* o, size_t off) { return *reinterpret_cast<int64_t*>((uintptr_t)o + off); }
static void     WriteI64(void* o, size_t off, int64_t v) { *reinterpret_cast<int64_t*>((uintptr_t)o + off) = v; }
static int32_t  ReadI32(void* o, size_t off) { return *reinterpret_cast<int32_t*>((uintptr_t)o + off); }
static void     WriteI32(void* o, size_t off, int32_t v) { *reinterpret_cast<int32_t*>((uintptr_t)o + off) = v; }
static float    AbsF(float v) { return v < 0 ? -v : v; }
static float    ReadF32(void* o, size_t off) { return *reinterpret_cast<float*>((uintptr_t)o + off); }
static void     WriteF32(void* o, size_t off, float v) { *reinterpret_cast<float*>((uintptr_t)o + off) = v; }
static void*    ReadPtrAt(void* o, size_t off) { return *reinterpret_cast<void**>((uintptr_t)o + off); }

// ============================================================================
//  ④ 致命信号兜底：只吞「保护区内」的；保护区外链回旧处理器；先打 fault at step
// ============================================================================
static thread_local sigjmp_buf            t_faultJmp;
static thread_local volatile sig_atomic_t t_inGuard = 0;
static thread_local volatile sig_atomic_t t_faultDepth = 0;
static thread_local volatile int          t_faultSig = 0;
static thread_local char                  t_altStack[65536];
static thread_local bool                  t_altStackSet = false;

static struct sigaction g_oldSegv, g_oldBus, g_oldIll;
static std::atomic<bool> g_guardArmed{false};
static std::mutex g_guardMutex;

static void AbsFaultHandler(int sig, siginfo_t* si, void* /*uc*/) {
    if (t_inGuard) {
        if (++t_faultDepth > 2) { signal(sig, SIG_DFL); raise(sig); return; }
        t_faultSig = sig;
        siglongjmp(t_faultJmp, 1);
    }
    int step = g_step.load();
    if (step > 0) fail_step_log:;
    LOGW(ABS_LOG " fault at step=%d sig=%d addr=%p（保护区外，交回旧处理器）",
         step, sig, si ? si->si_addr : nullptr);
    const struct sigaction* old = (sig == SIGSEGV) ? &g_oldSegv
                               : (sig == SIGBUS)  ? &g_oldBus : &g_oldIll;
    if (old->sa_sigaction && old->sa_sigaction != AbsFaultHandler) {
        old->sa_sigaction(sig, si, nullptr);
        return;
    }
    signal(sig, SIG_DFL);
    raise(sig);
}

static void ArmFaultGuardForThread() {
    if (t_altStackSet) return;
    t_altStackSet = true;
    stack_t ss;
    std::memset(&ss, 0, sizeof(ss));
    ss.ss_sp = t_altStack;
    ss.ss_size = sizeof(t_altStack);
    ss.ss_flags = 0;
    if (sigaltstack(&ss, nullptr) != 0) LOGW(ABS_LOG " sigaltstack failed");
}

static void InstallFaultGuard() {
    if (g_guardArmed.load()) return;
    std::lock_guard<std::mutex> lk(g_guardMutex);
    if (g_guardArmed.load()) return;
    struct sigaction sa;
    std::memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = AbsFaultHandler;
    sa.sa_flags = SA_SIGINFO | SA_ONSTACK | SA_NODEFER;
    sigemptyset(&sa.sa_mask);
    sigaction(SIGSEGV, &sa, &g_oldSegv);
    sigaction(SIGBUS,  &sa, &g_oldBus);
    sigaction(SIGILL,  &sa, &g_oldIll);
    g_guardArmed.store(true);
    LOGI(ABS_LOG " fault guard armed（只接管保护区内）");
}

template <typename F>
static bool Guarded(const char* what, F fn) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        fn();
        t_inGuard = 0;
        return true;
    }
    t_inGuard = 0;
    LOGW(ABS_LOG " fault at step=%d in=%s（已吞掉，降级）", g_step.load(), what);
    return false;
}

// ============================================================================
//  ⑤ 最小解析器：8 个符号，一个不多
// ============================================================================
typedef void*  (*fn_domain_get_t)();
typedef void*  (*fn_thread_attach_t)(void*);
typedef void*  (*fn_assembly_open_t)(void*, const char*);
typedef void*  (*fn_assembly_image_t)(void*);
typedef void*  (*fn_class_from_name_t)(void*, const char*, const char*);
typedef void*  (*fn_class_get_field_t)(void*, const char*);
typedef size_t (*fn_field_offset_t)(void*);
typedef void   (*fn_field_static_get_t)(void*, void*);
// 技能项（可选符号；缺任一 ⇒ 该项 pending，不影响其它三项）
typedef void*  (*fn_object_class_t)(void*);
typedef void*  (*fn_class_method_t)(void*, const char*, int);
typedef const char* (*fn_class_name_t)(void*);
typedef void*  (*fn_runtime_invoke_t)(void*, void*, void**, void**);

struct MiniApi {
    void* handle = nullptr;
    const char* handleHow = "none";
    uintptr_t base = 0, end = 0;
    bool ready = false;
    bool aborted = false;
    char why[96] = {};

    fn_domain_get_t      domain_get = nullptr;
    fn_thread_attach_t   thread_attach = nullptr;
    fn_assembly_open_t   assembly_open = nullptr;
    fn_assembly_image_t  assembly_image = nullptr;
    fn_class_from_name_t class_from_name = nullptr;
    fn_class_get_field_t class_get_field = nullptr;
    fn_field_offset_t    field_offset = nullptr;
    fn_field_static_get_t field_static_get = nullptr;

    // 可选（技能用）：object_class / class_method / runtime_invoke
    fn_object_class_t  object_class = nullptr;
    fn_class_method_t  class_method = nullptr;
    fn_class_name_t    class_name = nullptr;   // v1.9.1：实现类判别用
    fn_runtime_invoke_t runtime_invoke = nullptr;

    bool attached = false;

    void Abort(int step, const char* reason) {
        if (aborted) return;
        aborted = true;
        std::snprintf(why, sizeof(why), "%s", reason);
        LOGW(ABS_LOG " api abort step=%d reason=%s（安静降级：菜单全灰，进程必须活着）",
             step, reason);
    }

    // ★v1.1.1（houdini 实测）：本机 libil2cpp.so 只有 r--p / rw-p，**没有 r-xp**，
    //   执行由 /system/vendor/lib64/libhoudini.so 的 JIT 承担（我们自己的 so 同样如此，
    //   却能正常执行 JNI 入口）⇒ 「地址在可执行段」在这个环境下恒假，不能当判据。
    //   改为：addr ∈ libil2cpp 合并区间 [base,end) + addr≠0 + 4 字节对齐；
    //   首字合理性只是辅助（**只在可读时**读，读不到不因此拒绝）。
    bool SymOk(const char* name, void* addr) {
        uintptr_t a = (uintptr_t)addr;
        if (!a || (a & 3) || a < base || a >= end) {
            LOGW(ABS_LOG " sym_reject %s addr=%p 不在 libil2cpp [%p,%p) 或未对齐", name, addr,
                 (void*)base, (void*)end);
            return false;
        }
        if (RangePerm(a, 4, 1)) {               // 可读就顺手看一眼首字（辅助判据）
            uint32_t w = *(volatile uint32_t*)a;
            if (w == 0u || w == 0xFFFFFFFFu) {
                LOGW(ABS_LOG " sym_reject %s addr=%p 首字=%08X", name, addr, w);
                return false;
            }
        }
        return true;
    }

    bool BindOpt(const char* name, void** out) {
        dlerror();
        void* q = dlsym(handle, name);
        if (!q) { LOGW(ABS_LOG " sym_miss %s（可选，相关项降级）", name); return false; }
        if (!SymOk(name, q)) { LOGW(ABS_LOG " sym_reject %s（可选，相关项降级）", name); return false; }
        *out = q;
        LOGI(ABS_LOG " sym ok(opt) %s = %p", name, q);
        return true;
    }

    bool Bind(const char* name, void** out, int step) {
        dlerror();
        void* q = dlsym(handle, name);
        if (!q) { Abort(step, name); return false; }
        if (!SymOk(name, q)) { Abort(step, name); return false; }
        *out = q;
        LOGI(ABS_LOG " sym ok %s = %p", name, q);
        return true;
    }
};

static MiniApi g_api;
static uint64_t g_libSeenAt = 0;

// 取 image：只用 il2cpp_domain_assembly_open("Assembly-CSharp")
static void* OpenImage(const char* name) {
    void* asmObj = nullptr;
    if (!Guarded("assembly_open", [&] { asmObj = g_api.assembly_open(g_api.domain_get(), name); })
        || !asmObj) {
        LOGW(ABS_LOG " assembly_open(%s) = null", name);
        return nullptr;
    }
    void* img = nullptr;
    if (!Guarded("assembly_image", [&] { img = g_api.assembly_image(asmObj); }) || !img) {
        LOGW(ABS_LOG " assembly_image(%s) = null", name);
        return nullptr;
    }
    return img;
}

static bool ApiLoad() {
    if (g_api.ready) return true;
    if (g_api.aborted) return false;

    uintptr_t b = 0, e = 0;
    if (!LibRange(&b, &e)) return false;
    if (!g_libSeenAt) {
        g_libSeenAt = NowMs();
        LOGI(ABS_LOG " libil2cpp mapped [%p,%p) %.1f MB ⇒ 等 3s 再解析（避开半初始化）",
             (void*)b, (void*)e, (double)(e - b) / 1048576.0);
        return false;
    }
    if (NowMs() - g_libSeenAt < 3000) return false;   // 硬要求：出现后再等 3 秒

    g_api.base = b; g_api.end = e;

    STEP_LOG(1, "dlsym");
    g_api.handle = dlopen("libil2cpp.so", RTLD_NOW | RTLD_NOLOAD);
    if (g_api.handle) {
        g_api.handleHow = "noload";
    } else {
        g_api.handle = dlopen("libil2cpp.so", RTLD_NOW);
        g_api.handleHow = g_api.handle ? "newload" : "none";
        if (g_api.handle) LOGW(ABS_LOG " handle=newload（NOLOAD 失败，退回常规 dlopen）");
    }
    if (!g_api.handle) { g_api.Abort(1, "dlopen failed"); return false; }
    LOGI(ABS_LOG " handle ok（%s）", g_api.handleHow);

    if (!g_api.Bind("il2cpp_domain_get", (void**)&g_api.domain_get, 1)) return false;
    if (!g_api.Bind("il2cpp_thread_attach", (void**)&g_api.thread_attach, 1)) return false;
    if (!g_api.Bind("il2cpp_domain_assembly_open", (void**)&g_api.assembly_open, 1)) return false;
    if (!g_api.Bind("il2cpp_assembly_get_image", (void**)&g_api.assembly_image, 1)) return false;
    if (!g_api.Bind("il2cpp_class_from_name", (void**)&g_api.class_from_name, 1)) return false;
    if (!g_api.Bind("il2cpp_class_get_field_from_name", (void**)&g_api.class_get_field, 1)) return false;
    if (!g_api.Bind("il2cpp_field_get_offset", (void**)&g_api.field_offset, 1)) return false;
    if (!g_api.Bind("il2cpp_field_static_get_value", (void**)&g_api.field_static_get, 1)) return false;
    // 技能项所需的可选符号（不参与 required 判定）
    g_api.BindOpt("il2cpp_object_get_class", (void**)&g_api.object_class);
    g_api.BindOpt("il2cpp_class_get_method_from_name", (void**)&g_api.class_method);
    g_api.BindOpt("il2cpp_runtime_invoke", (void**)&g_api.runtime_invoke);
    g_api.BindOpt("il2cpp_class_get_name", (void**)&g_api.class_name);
    LOGI(ABS_LOG " api step=1 ok（8 个符号全过落点校验，handle=%s）", g_api.handleHow);

    // step=2：domain_get 连续两次同值非空（避免 runtime 半初始化）
    STEP_LOG(2, "domain_get");
    void* d1 = nullptr;
    void* d2 = nullptr;
    if (!Guarded("domain_get#1", [&] { d1 = g_api.domain_get(); }) || !d1) {
        g_api.Abort(2, "domain_get null"); return false;
    }
    if (!Guarded("domain_get#2", [&] { d2 = g_api.domain_get(); }) || d2 != d1) {
        LOGI(ABS_LOG " domain 抖动（%p vs %p），稍后重试", d1, d2);
        return false;
    }
    LOGI(ABS_LOG " api step=2 ok（domain=%p）", d1);

    // step=3：thread_attach 一次并缓存
    STEP_LOG(3, "thread_attach");
    if (!Guarded("thread_attach", [&] { g_api.thread_attach(d1); })) {
        g_api.Abort(3, "thread_attach fault"); return false;
    }
    g_api.attached = true;
    LOGI(ABS_LOG " api step=3 ok（ModThread 已附着 domain）");

    g_api.ready = true;
    return true;
}

// ============================================================================
//  ⑥ 类/字段解析（by-name + 与 dump 偏移交叉核对）
// ============================================================================
struct FieldRef { void* field = nullptr; size_t off = 0; bool ok = false; };

struct Anchors {
    bool  resolved = false;
    bool  aborted  = false;
    char  abortWhy[96] = {};
    void* unitKlass = nullptr;
    FieldRef state, faction, hpCur, hpMax, count, cc, mask, driver;
    void*  staticDictField = nullptr;
    bool   dictOk = false;
    FieldRef oknd;
    bool     okndOk = false;
};
static Anchors g_anchors;
static void* g_image = nullptr;   // Assembly-CSharp image（oknd 兜底用）

static bool ResolveFieldChecked(void* klass, const char* name, size_t dumpOff,
                               FieldRef& out, const char* feat) {
    void* f = nullptr;
    if (!Guarded("class_get_field", [&] { f = g_api.class_get_field(klass, name); }) || !f) {
        LOGW(ABS_LOG " field %s miss（%s 降级）", name, feat);
        return false;
    }
    size_t off = 0;
    Guarded("field_offset", [&] { off = g_api.field_offset(f); });
    if (off != dumpOff) {
        g_anchors.aborted = true;
        std::snprintf(g_anchors.abortWhy, sizeof(g_anchors.abortWhy),
                      "offset mismatch %s got=0x%zX want=0x%zX", name, off, dumpOff);
        LOGW(ABS_LOG " abort: offset mismatch %s got=0x%zX want=0x%zX（整项跳过）",
             name, off, dumpOff);
        return false;
    }
    out.field = f; out.off = off; out.ok = true;
    LOGI(ABS_LOG " field %s off=0x%zX ok", name, off);
    return true;
}

static bool ResolveAll() {
    if (g_anchors.resolved) return true;
    if (!ApiLoad()) return false;

    STEP_LOG(4, "assembly_open Assembly-CSharp");
    void* img = OpenImage("Assembly-CSharp.dll");
    if (!img) img = OpenImage("Assembly-CSharp");
    if (!img) { g_api.Abort(4, "Assembly-CSharp image null"); return false; }
    LOGI(ABS_LOG " api step=4 ok（image=%p）", img);

    STEP_LOG(5, "image ok");
    LOGI(ABS_LOG " api step=5 ok");

    STEP_LOG(6, "class Unit");
    void* unit = nullptr;
    if (!Guarded("class_from_name Unit", [&] { unit = g_api.class_from_name(img, "", "Unit"); })
        || !unit) {
        g_api.Abort(6, "class Unit null"); return false;
    }
    g_anchors.unitKlass = unit;
    g_image = img;
    LOGI(ABS_LOG " api step=6 ok（class Unit=%p）", unit);

    STEP_LOG(7, "fields");
    ResolveFieldChecked(unit, "LAGPMHKOAKN",           kDumpOffState,   g_anchors.state,   "状态");
    ResolveFieldChecked(unit, "GPJLPFOKIDE",           kDumpOffFaction, g_anchors.faction, "阵营");
    ResolveFieldChecked(unit, "DEPKEGHAHMB",           kDumpOffHpCur,   g_anchors.hpCur,   "无敌");
    ResolveFieldChecked(unit, "LAMEKJABEAP",           kDumpOffHpMax,   g_anchors.hpMax,   "无敌");
    ResolveFieldChecked(unit, "m_CharacterController", kDumpOffCC,      g_anchors.cc,      "移速");
    ResolveFieldChecked(unit, "KDOFIAPPOGI",           kDumpOffMask,    g_anchors.mask,    "掩码");
    ResolveFieldChecked(unit, "APPACLOOHNK",           kDumpOffCount,   g_anchors.count,   "计数");
    ResolveFieldChecked(unit, "GAMJINGNDHM",           kDumpOffDriver,  g_anchors.driver,  "移速驱动器");
    Guarded("dict_field", [&] { g_anchors.staticDictField = g_api.class_get_field(unit, "LOJFLKLPODK"); });
    if (g_anchors.staticDictField) LOGI(ABS_LOG " field LOJFLKLPODK（静态主表）ok");
    else LOGW(ABS_LOG " field LOJFLKLPODK miss（单位枚举降级）");

    if (!Guarded("oknd_field", [&] { g_anchors.oknd.field = g_api.class_get_field(unit, "OKNDCLIKECF"); })
        || !g_anchors.oknd.field) {
        LOGI(ABS_LOG " probe: OKNDCLIKECF miss");
    } else {
        Guarded("oknd_off", [&] { g_anchors.oknd.off = g_api.field_offset(g_anchors.oknd.field); });
        g_anchors.okndOk = true;
        LOGI(ABS_LOG " probe: OKNDCLIKECF off=0x%zX ok", g_anchors.oknd.off);
    }
    LOGI(ABS_LOG " api step=7 ok（字段解析完成，aborted=%d）", (int)g_anchors.aborted);

    g_anchors.resolved = true;
    return true;
}

// ============================================================================
//  ⑦ 单位枚举：静态表 Dictionary<int,Unit>（固定布局，只读，不建托管对象）
// ============================================================================
struct UnitSlot { void* unit; int32_t faction; };

static void* StaticDict() {
    if (!g_anchors.staticDictField || !g_api.field_static_get) return nullptr;
    void* out = nullptr;
    if (!Guarded("static_dict", [&] { g_api.field_static_get(g_anchors.staticDictField, &out); }))
        return nullptr;
    return OkPtr(out) ? out : nullptr;
}

static int CollectUnits(UnitSlot* out, int cap) {
    if (!g_anchors.faction.ok) return 0;
    void* dict = StaticDict();
    if (!dict) return 0;
    int32_t count = ReadI32(dict, kDictOffCount);
    void* entries = ReadPtrAt(dict, kDictOffEntries);
    if (count <= 0 || count > 4096) { LOGI(ABS_LOG " unit_table: count=%d 不可信，跳过", count); return 0; }
    if (!OkPtr(entries)) { LOGI(ABS_LOG " unit_table: entries 指针不可用，跳过"); return 0; }
    if (!RangePerm((uintptr_t)entries + kArrayDataOff, (size_t)count * kEntryStride, 1)) {
        LOGI(ABS_LOG " unit_table: entries 数据区不可读，跳过");
        return 0;
    }
    if (!g_anchors.dictOk) {
        g_anchors.dictOk = true;
        LOGI(ABS_LOG " unit_table ok: fixed-layout count=%d entries=%p stride=%zu valueOff=%zu",
             count, entries, kEntryStride, kEntryOffValue);
    }
    int n = 0;
    for (int32_t i = 0; i < count && n < cap; ++i) {
        uintptr_t e = (uintptr_t)entries + kArrayDataOff + (uintptr_t)i * kEntryStride;
        void* u = *reinterpret_cast<void**>(e + kEntryOffValue);
        if (!OkPtr(u)) continue;
        int32_t f = ReadI32(u, g_anchors.faction.off);
        if (f != 0 && f != kFactionPlayer && f != kFactionEnemy) continue;   // 脏数据丢弃
        out[n].unit = u;
        out[n].faction = f;
        ++n;
    }
    return n;
}

static UnitSlot g_units[256];
static uint64_t g_lastCountLog = 0;
static bool g_loggedSemantics = false;

// ============================================================================
//  ⑧ 功能（数据面）
// ============================================================================

// 状态量提到 guard 外：Tick/ForceClear 两版都要用
static uint64_t g_lastSkillLog = 0;
static uint64_t g_lastSpeedLog = 0;
static uint64_t g_lastDmgxLog = 0;
struct HpCache { void* unit; int64_t hp; uint64_t seen; };
static HpCache g_hpCache[128];

#if !defined(STARRY_PROBE) && !defined(STARRY_SPEED_ONLY) && !defined(STARRY_HOOK_PROBE)
// ============================================================================
//  v1.5：血量字段 Unit+0x198（APPACLOOHNK，int）——「先证后写」三态机
//
//  背景：v1.2 实测 +0x190/+0x1A0 恒为 0（选点作废）；v1.3 hp_cand 里
//        off=0x198 出现 100→99→98→97→96（每次 delta=1），是唯一干净读数。
//
//  三态：UNKNOWN →(进战斗) LEARNING(观察 2000ms 取窗口最大值) → LEARNED
//        →(观察到我方受击下降 + 敌方数值合理) CONFIRMED（才允许写）
//        任一条被证伪 → REJECTED（本进程只打日志，一个字节都不写）
//
//  无敌不做还原：关闭开关只停止写，血量保持当前值，不退回旧值（写回旧值等于扣血）。
// ============================================================================
static void* ResolveOknd(void* state, const char** whyOut);   // 前置（属性探针要用）

struct AttrView { void* arr = nullptr; int len = 0; };

static AttrView AttrArray(void* oknd) {
    AttrView v;
    if (!OkPtr(oknd)) return v;
    void* holder = nullptr;
    if (!Guarded("attr_holder", [&] { holder = ReadPtrAt(oknd, 0x50); })) return v;
    if (!OkPtr(holder)) return v;
    if (!Guarded("attr_arr", [&] { v.arr = ReadPtrAt(holder, 0x10); })) return v;
    if (!OkPtr(v.arr)) { v.arr = nullptr; return v; }
    Guarded("attr_len", [&] { v.len = ReadI32(v.arr, 0x18); });
    if (v.len <= 0 || v.len > 4096) { v.arr = nullptr; v.len = 0; }
    return v;
}

static bool AttrGetV(const AttrView& v, int id, int* out) {
    if (!v.arr || id < 0 || id >= v.len) return false;
    if (!RangePerm((uintptr_t)v.arr + 0x20 + (size_t)id * 4, 4, 1)) return false;
    int val = 0;
    if (!Guarded("attr_get", [&] { val = ReadI32(v.arr, 0x20 + (size_t)id * 4); })) return false;
    *out = val;
    return true;
}

static bool AttrSetV(const AttrView& v, int id, int val) {
    if (!v.arr || id < 0 || id >= v.len) return false;
    if (!RangePerm((uintptr_t)v.arr + 0x20 + (size_t)id * 4, 4, 2)) return false;
    return Guarded("attr_set", [&] { WriteI32(v.arr, 0x20 + (size_t)id * 4, val); });
}

// ============================================================================
//  v1.9.1：属性容器实现类判别 + 正确读写链（recon §15）
//    oknd+0x50 的静态类型是接口 KEMEJCCDJKJ，三个实现布局完全不同：
//      GGLILJGGPIK  +0x10 = int[]（真实现）
//      AGNKCIDBODI  +0x10 = int，泛型容器在 +0x18
//      EHKMOKMNKKI  无实例字段，读恒 0
//    读：OKNDCLIKECF::FJMHJMEPFMC(int)，this = **容器 oknd**
//    写：仅 GGLILJGGPIK::JCHJIBOHDEL(int idx, int val)，this = attrset（oknd+0x50）
// ============================================================================
struct AttrImpl {
    void*       oknd = nullptr;
    void*       attrset = nullptr;
    void*       klass = nullptr;      // attrset 的运行时类
    const char* name = "?";
    bool        isGGLI = false;
    bool        isAGNK = false;
    void*       miRead = nullptr;     // FJMHJMEPFMC(int)  —— this=oknd（容器级读）
    void*       miWrite = nullptr;    // GGLI: JCHJIBOHDEL(int,int) / AGNK: NBOPCNCOKLD(enum,int)
    void*       miReadAttr = nullptr; // AGNK: KHPIJPHCFGP(enum) —— this=attrset
    bool        resolved = false;
};
static AttrImpl g_impl;
static uint64_t g_lastImplLog = 0;

static void AttrImplResolve(void* oknd) {
    if (!oknd || g_impl.oknd == oknd) return;
    g_impl = AttrImpl();
    g_impl.oknd = oknd;
    Guarded("impl_attrset", [&] { g_impl.attrset = ReadPtrAt(oknd, 0x50); });
    if (OkPtr(g_impl.attrset) && g_api.object_class) {
        Guarded("impl_klass", [&] { g_impl.klass = g_api.object_class(g_impl.attrset); });
    }
    if (g_impl.klass && g_api.class_name) {
        const char* nm = nullptr;
        Guarded("impl_name", [&] { nm = g_api.class_name(g_impl.klass); });
        if (nm) g_impl.name = nm;
    }
    g_impl.isGGLI = (std::strcmp(g_impl.name, "GGLILJGGPIK") == 0);
    g_impl.isAGNK = (std::strcmp(g_impl.name, "AGNKCIDBODI") == 0);
    if (g_impl.klass && g_api.class_method) {
        Guarded("impl_methods", [&] {
            if (g_impl.isGGLI) {
                g_impl.miWrite = g_api.class_method(g_impl.klass, "JCHJIBOHDEL", 2);
            } else if (g_impl.isAGNK) {
                g_impl.miWrite = g_api.class_method(g_impl.klass, "NBOPCNCOKLD", 2);
                g_impl.miReadAttr = g_api.class_method(g_impl.klass, "KHPIJPHCFGP", 1);
            }
        });
    }
    // 读方法在容器类上（this=oknd）
    if (g_api.object_class && g_api.class_method) {
        void* kc = nullptr;
        Guarded("impl_kc", [&] { kc = g_api.object_class(oknd); });
        if (kc) Guarded("impl_read_mi", [&] { g_impl.miRead = g_api.class_method(kc, "FJMHJMEPFMC", 1); });
    }
    g_impl.resolved = true;
    LOGI(ABS_LOG " attr impl=%s oknd=%p attrset=%p klass=%p read=%p write=%p",
         g_impl.name, oknd, g_impl.attrset, g_impl.klass, g_impl.miRead, g_impl.miWrite);
}

// 读：优先 FJMHJMEPFMC（this=oknd，unbox int），失败退 GGLI 的 int[] 路径
static bool AttrRwRead(void* oknd, int id, int* out) {
    AttrImplResolve(oknd);
    if (!g_impl.resolved) return false;
    if (g_impl.miRead && g_api.runtime_invoke) {
        bool ok = false;
        Guarded("attr_rw_read", [&] {
            int32_t a0 = id;
            void* args[1] = {&a0};
            void* exc = nullptr;
            void* ret = g_api.runtime_invoke(g_impl.miRead, oknd, args, &exc);
            if (!ret) return;
            if (RangePerm((uintptr_t)ret + 0x10, 4, 1)) {          // 装箱对象
                *out = *reinterpret_cast<int*>((uintptr_t)ret + 0x10);
                ok = true;
            } else {                                               // 直接返回 int
                int64_t v = (int64_t)(intptr_t)ret;
                if (v > -1000000000LL && v < 1000000000LL) { *out = (int)v; ok = true; }
            }
        });
        if (ok) return true;
    }
    // AGNK：KHPIJPHCFGP(KMLFBAENBMH) this=attrset
    if (g_impl.isAGNK && g_impl.miReadAttr && g_api.runtime_invoke) {
        bool ok2 = false;
        Guarded("attr_agnk_read", [&] {
            int32_t a0 = id;
            void* args[1] = {&a0};
            void* ret = g_api.runtime_invoke(g_impl.miReadAttr, g_impl.attrset, args, nullptr);
            if (!ret) return;
            if (RangePerm((uintptr_t)ret + 0x10, 4, 1)) {
                *out = *reinterpret_cast<int*>((uintptr_t)ret + 0x10);
                ok2 = true;
            } else {
                int64_t v = (int64_t)(intptr_t)ret;
                if (v > -1000000000LL && v < 1000000000LL) { *out = (int)v; ok2 = true; }
            }
        });
        if (ok2) return true;
    }
    // 退路：GGLI 数组路径（attrset+0x10 是 int[134]，len@+0x18、data@+0x20）
    AttrView v = AttrArray(oknd);
    return AttrGetV(v, id, out);
}

// 写：仅 GGLI 支持；其他实现打 unsupported 并跳过
static bool AttrRwWrite(void* oknd, int id, int val, const char* what) {
    AttrImplResolve(oknd);
    if (!g_impl.resolved) return false;
    if (!(g_impl.isGGLI || g_impl.isAGNK) || !g_impl.miWrite || !g_api.runtime_invoke) {
        if (std::strcmp(what, "attr62") == 0)
            LOGW(ABS_LOG " attr62 unsupported impl=%s（不写、不崩）", g_impl.name);
        else if (std::strcmp(what, "attr36") == 0)
            LOGW(ABS_LOG " attr36 unsupported impl=%s（不写、不崩）", g_impl.name);
        else
            LOGW(ABS_LOG " attr unsupported impl=%s what=%s", g_impl.name, what);
        return false;
    }
    bool ok = false;
    Guarded("attr_rw_write", [&] {
        int32_t a0 = id, a1 = val;
        void* args[2] = {&a0, &a1};
        g_api.runtime_invoke(g_impl.miWrite, g_impl.attrset, args, nullptr);   // this=attrset
        ok = true;
    });
    return ok;
}

enum Hp198State { HP198_UNKNOWN = 0, HP198_LEARNING, HP198_LEARNED, HP198_CONFIRMED, HP198_REJECTED };

struct Hp198Ctx {
    int      state = HP198_UNKNOWN;
    uint64_t winStart = 0;
    int      samples = 0;          // 窗口内合理样本数（1..1e6 且 >0）
    int      maxCand = 0;
    int      lastPlayer = 0;
    bool     sawDrop = false;
    bool     wrote = false;
    uint64_t lastBeat = 0;
    uint64_t lastHit = 0;
    // v1.6：越界样本不再一票否决
    int      invalidStreak = 0;
    uint64_t invalidSince = 0;
    uint64_t lastInvalidLog = 0;
    // v1.6：REJECTED 自愈
    int      recoverCnt = 0;
    uint64_t lastRecoverProbe = 0;
    // v1.6：CONFIRMED 后换战斗的短暂重学期（只重置 max/lastHp，不改状态机）
    uint64_t relearnUntil = 0;
};
static Hp198Ctx g_hp198;
static int       g_lastUnitCount = 0;

struct EnemyHp { void* unit; int hp; uint64_t seen; };
static EnemyHp  g_enemyHp[128];
static uint64_t g_lastDmgx198Log = 0;
static uint64_t g_lastGod198Log = 0;

static const int kHp198MaxSane = 1000000;   // 值域上限
static const int kHp198MaxDelta = 65535;    // 单次扣血量级上限

static int ReadHp198(void* u) {              // int 专用，禁止混用 long 版
    int v = 0;
    if (!RangePerm((uintptr_t)u + kDumpOffHp198, 4, 1)) return -1;
    if (!Guarded("hp198_read", [&] { v = ReadI32(u, kDumpOffHp198); })) return -1;
    return v;
}
static bool WriteHp198(void* u, int v) {
    if (!RangePerm((uintptr_t)u + kDumpOffHp198, 4, 1)) return false;
    return Guarded("hp198_write", [&] { WriteI32(u, kDumpOffHp198, v); });
}

static EnemyHp* EnemySlot(void* unit, uint64_t now) {
    EnemyHp* freeSlot = nullptr;
    EnemyHp* lru = &g_enemyHp[0];
    for (int i = 0; i < 128; ++i) {
        if (g_enemyHp[i].unit == unit) return &g_enemyHp[i];
        if (!g_enemyHp[i].unit && !freeSlot) freeSlot = &g_enemyHp[i];
        if (g_enemyHp[i].seen < lru->seen) lru = &g_enemyHp[i];
    }
    EnemyHp* s = freeSlot ? freeSlot : lru;
    s->unit = unit; s->hp = 0; s->seen = now;
    return s;
}

// Stage A：采样 + 状态机（只读）。返回是不是 CONFIRMED。
static bool Hp198Tick(const UnitSlot* us, int n, uint64_t now) {
    // ---- 进/出战斗检测 ----
    if (g_lastUnitCount == 0 && n > 0) {
        if (g_hp198.state == HP198_CONFIRMED) {
            // v1.6：已确认过就不重学状态机，只重置 max/lastHp，并给 2s 重学期（期间不写）
            g_hp198.maxCand = 0;
            g_hp198.lastPlayer = 0;
            g_hp198.relearnUntil = now + 2000;
            LOGI(ABS_LOG " hp198 new-battle（保持 CONFIRMED，仅重置 max/lastHp，重学 2000ms）");
        } else if (g_hp198.state != HP198_REJECTED) {
            g_hp198.state = HP198_LEARNING;
            g_hp198.winStart = now;
            g_hp198.samples = 0;
            g_hp198.maxCand = 0;
            g_hp198.lastPlayer = 0;
            g_hp198.sawDrop = false;
            g_hp198.wrote = false;
            g_hp198.invalidStreak = 0;
            LOGI(ABS_LOG " hp198 enter-battle ⇒ 观察窗口 2000ms（窗口期不写任何东西）");
        } else {
            // REJECTED：保持只读，但允许自愈采样
            g_hp198.lastPlayer = 0;
            LOGI(ABS_LOG " hp198 enter-battle（仍 REJECTED，低频采样等自愈）");
        }
    }
    g_lastUnitCount = n;
    if (n <= 0 || !g_anchors.count.ok) return false;

    // ---- 首个我方 ----
    void* pu = nullptr;
    for (int i = 0; i < n; ++i) {
        if (us[i].faction == kFactionPlayer) { pu = us[i].unit; break; }
    }
    if (!pu) return false;
    int cur = ReadHp198(pu);
    const bool valid = (cur >= 1 && cur <= kHp198MaxSane);

    // ---- v1.6-1：越界样本只记无效，不当场 reject ----
    if (!valid) {
        if (g_hp198.invalidStreak++ == 0) g_hp198.invalidSince = now;
        if (now - g_hp198.lastInvalidLog > 1000) {
            g_hp198.lastInvalidLog = now;
            LOGI(ABS_LOG " hp198 sample-invalid cur=%d n=%d state=%d（不因此判 REJECTED）",
                 cur, g_hp198.invalidStreak, g_hp198.state);
        }
        const bool persistent = (g_hp198.invalidStreak >= 3) && (now - g_hp198.invalidSince >= 1000);
        const bool windowEmpty = (g_hp198.state == HP198_LEARNING) &&
                                 (now - g_hp198.winStart >= 2000) && (g_hp198.samples < 2);
        if ((persistent || windowEmpty) && g_hp198.state != HP198_REJECTED) {
            g_hp198.state = HP198_REJECTED;
            g_hp198.recoverCnt = 0;
            LOGW(ABS_LOG " hp198 reject reason=%s cur=%d samples=%d streak=%d",
                 windowEmpty ? "窗口内合理样本不足 2 个" : "连续越界 ≥3 次且持续 ≥1s",
                 cur, g_hp198.samples, g_hp198.invalidStreak);
        }
        return false;
    }
    g_hp198.invalidStreak = 0;

    // ---- v1.6-2：REJECTED 自愈（1s 低频探测，连续 2 次合理 ⇒ 回 LEARNING）----
    if (g_hp198.state == HP198_REJECTED) {
        if (now - g_hp198.lastRecoverProbe < 1000) return false;
        g_hp198.lastRecoverProbe = now;
        g_hp198.maxCand = cur;
        if (++g_hp198.recoverCnt >= 2) {
            g_hp198.state = HP198_LEARNING;
            g_hp198.winStart = now;
            g_hp198.samples = 0;
            g_hp198.lastPlayer = cur;
            g_hp198.sawDrop = false;
            g_hp198.maxCand = cur;
            g_hp198.recoverCnt = 0;
            LOGI(ABS_LOG " hp198 recover（连续 2 次合理值 cur=%d ⇒ 重跑观察窗口）", cur);
        } else {
            LOGI(ABS_LOG " hp198 recover-probe cur=%d n=%d", cur, g_hp198.recoverCnt);
        }
        return false;
    }

    // ---- 敌方合理性 + 采样串（地址:faction:hp，最多 12 个）----
    bool enemiesSane = true;
    char elist[384];
    int eo = 0;
    int ecount = 0;
    elist[0] = 0;
    for (int i = 0; i < n && ecount < 12; ++i) {
        if (us[i].faction != kFactionEnemy) continue;
        int hp = ReadHp198(us[i].unit);
        if (hp < 1 || hp > kHp198MaxSane) enemiesSane = false;
        eo += std::snprintf(elist + eo, sizeof(elist) - (size_t)eo, "%s%p:%d:%d",
                            ecount ? "," : "", us[i].unit, (int)us[i].faction, hp);
        ++ecount;
    }

    // ---- 变化检测（先比后更新 lastPlayer）----
    if (g_hp198.lastPlayer > 0) {
        if (cur < g_hp198.lastPlayer) {
            int delta = g_hp198.lastPlayer - cur;
            if (delta > 0 && delta <= kHp198MaxDelta) {
                g_hp198.sawDrop = true;
                if (now - g_hp198.lastHit > 1000) {
                    g_hp198.lastHit = now;
                    LOGI(ABS_LOG " hp198 hit prev=%d now=%d delta=%d unit=%p",
                         g_hp198.lastPlayer, cur, delta, pu);
                    // v1.8 受击对照（只读）
                    void* hst = g_anchors.state.ok ? ReadPtrAt(pu, g_anchors.state.off) : nullptr;
                    const char* hwhy = "-";
                    void* hoknd = ResolveOknd(hst, &hwhy);
                    AttrView hv = AttrArray(hoknd);
                    int a3 = -1, a15 = -1, a62 = -1;
                    AttrImplResolve(hoknd);
                    AttrRwRead(hoknd, 3, &a3);
                    AttrRwRead(hoknd, 15, &a15);
                    bool guard = AttrRwRead(hoknd, 62, &a62) && a62 > 0;
                    LOGI(ABS_LOG " hit-view 190=%lld 1A0=%lld attr62=%d impl=%s guard=%d",
                         (long long)ReadI64(pu, kDumpOffHpCur), (long long)ReadI64(pu, kDumpOffHpMax),
                         a62, g_impl.name, (int)guard);
                    (void)a3; (void)a15;
                }
            }
        } else if (cur > g_hp198.lastPlayer) {
            LOGI(ABS_LOG " hp198 up prev=%d now=%d（可能是治疗或新战斗）", g_hp198.lastPlayer, cur);
        }
    }

    if (g_hp198.state == HP198_LEARNING || g_hp198.state == HP198_LEARNED) {
        if (cur > g_hp198.maxCand) g_hp198.maxCand = cur;
    }

    if (g_hp198.state == HP198_LEARNING) {
        ++g_hp198.samples;
        if (now - g_hp198.winStart >= 2000) {
            LOGI(ABS_LOG " hp198 max-learning max=%d samples=%d sawDrop=%d enemiesSane=%d",
                 g_hp198.maxCand, g_hp198.samples, (int)g_hp198.sawDrop, (int)enemiesSane);
            if (g_hp198.samples < 2 || g_hp198.maxCand <= 0) {
                g_hp198.state = HP198_REJECTED;
                g_hp198.recoverCnt = 0;
                LOGW(ABS_LOG " hp198 reject reason=窗口内合理样本不足 2 个 samples=%d", g_hp198.samples);
            } else if (!enemiesSane) {
                g_hp198.state = HP198_REJECTED;
                g_hp198.recoverCnt = 0;
                LOGW(ABS_LOG " hp198 reject reason=enemy value out of range");
            } else if (g_hp198.sawDrop) {
                g_hp198.state = HP198_CONFIRMED;
                LOGI(ABS_LOG " hp198 CONFIRMED（受击下降 + 敌方合理）max=%d", g_hp198.maxCand);
            } else {
                g_hp198.state = HP198_LEARNED;
                LOGI(ABS_LOG " hp198 LEARNED（max=%d，等一次受击下降再确认）", g_hp198.maxCand);
            }
        }
    } else if (g_hp198.state == HP198_LEARNED) {
        if (!enemiesSane) {
            g_hp198.state = HP198_REJECTED;
            g_hp198.recoverCnt = 0;
            LOGW(ABS_LOG " hp198 reject reason=enemy value out of range（LEARNED 阶段）");
        } else if (g_hp198.sawDrop) {
            g_hp198.state = HP198_CONFIRMED;
            LOGI(ABS_LOG " hp198 CONFIRMED（受击下降 + 敌方合理）max=%d", g_hp198.maxCand);
        }
    } else if (g_hp198.state == HP198_CONFIRMED) {
        if (cur > g_hp198.maxCand) {
            if (g_hp198.maxCand > 0) LOGI(ABS_LOG " hp198 max raised %d → %d（新战斗或增益）", g_hp198.maxCand, cur);
            g_hp198.maxCand = cur;
        }
        if (!enemiesSane) {
            g_hp198.state = HP198_REJECTED;
            g_hp198.recoverCnt = 0;
            LOGW(ABS_LOG " hp198 reject reason=enemy value out of range（CONFIRMED 后推翻）");
        }
    }

    g_hp198.lastPlayer = cur;

    if (now - g_hp198.lastBeat > 5000) {
        g_hp198.lastBeat = now;
        LOGI(ABS_LOG " hp198 player=%d enemies=[%s] state=%d max=%d",
             cur, ecount ? elist : "-", g_hp198.state, g_hp198.maxCand);
    }
    return g_hp198.state == HP198_CONFIRMED;
}


// ============================================================================
//  v1.7.1 诊断：镜像候选扫描 + 写探针
//    0x198 疑为受击同步的镜像值：真血量可能在 unit / state / **属性容器 oknd** 上。
//    扫描关键字：① == cur（当前显示血量）② == max（上限，可能单独存一份）
//    扫描类型：int32 与 float32（属性值很可能是 float）
//    写探针：写 cur+1（或 +1.0f）→ 立刻读回 → 立刻写回 cur，全程保护区，<1ms
// ============================================================================
static const size_t kScanUnitEnd  = 0x300;
static const size_t kScanStateEnd = 0x200;
static const size_t kScanOkndEnd  = 0x100;
static const int    kMirrorCap    = 12;     // 每个对象最多 12 条

enum MirrorWhere { MW_UNIT = 0, MW_STATE = 1, MW_OKND = 2 };

struct MirrorCand { size_t off; int where; int type; uint64_t lastProbe; };   // type 0=int 1=float
static MirrorCand g_mirror[40];
static int        g_mirrorN = 0;
static uint64_t   g_lastMirrorLog = 0;

static const char* WhereName(int w) {
    return w == MW_UNIT ? "unit" : (w == MW_STATE ? "state" : "oknd");
}

static void AddCand(size_t off, int where, int type, uint64_t now) {
    if (g_mirrorN >= 40) return;
    for (int i = 0; i < g_mirrorN; ++i) {
        if (g_mirror[i].off == off && g_mirror[i].where == where && g_mirror[i].type == type) return;
    }
    g_mirror[g_mirrorN].off = off;
    g_mirror[g_mirrorN].where = where;
    g_mirror[g_mirrorN].type = type;
    g_mirror[g_mirrorN].lastProbe = 0;
    ++g_mirrorN;
    (void)now;
}

// 扫一个对象：关键字 cur 与 max，类型 int32 与 float32
static void ScanRegion(void* base, size_t end, int where, int cur, int max, uint64_t now) {
    if (!OkPtr(base) || !RangePerm((uintptr_t)base, end, 1)) return;
    int hits = 0;
    for (size_t off = 0; off + 4 <= end && hits < kMirrorCap; off += 4) {
        int vi = 0;
        if (!Guarded("mirror_scan_i", [&] { vi = *reinterpret_cast<int*>((uintptr_t)base + off); })) continue;
        float vf = 0.0f;
        if (!Guarded("mirror_scan_f", [&] { vf = *reinterpret_cast<float*>((uintptr_t)base + off); })) continue;

        const char* tag = nullptr;
        if (cur > 0 && vi == cur) tag = "cur";
        else if (max > 0 && vi == max) tag = "max";
        if (tag) {
            LOGI(ABS_LOG " hp_mirror off=0x%zX val=%d type=int where=%s tag=%s",
                 off, vi, WhereName(where), tag);
            AddCand(off, where, 0, now);
            ++hits;
            continue;
        }
        if (cur > 0 && AbsF(vf - (float)cur) <= 1e-3f) {
            LOGI(ABS_LOG " hp_mirror off=0x%zX val=%.3f type=float where=%s tag=cur",
                 off, (double)vf, WhereName(where));
            AddCand(off, where, 1, now);
            ++hits;
        } else if (max > 0 && AbsF(vf - (float)max) <= 1e-3f) {
            LOGI(ABS_LOG " hp_mirror off=0x%zX val=%.3f type=float where=%s tag=max",
                 off, (double)vf, WhereName(where));
            AddCand(off, where, 1, now);
            ++hits;
        }
    }
}

static void MirrorScanOnce(void* u, void* st, void* oknd, int cur, int max, uint64_t now) {
    g_mirrorN = 0;
    (void)u;   // v1.7.2：unit 段不扫（省日志）
    if (OkPtr(st)) ScanRegion(st, kScanStateEnd, MW_STATE, cur, max, now);
    if (OkPtr(oknd)) ScanRegion(oknd, kScanOkndEnd, MW_OKND, cur, max, now);   // v1.7.1：属性容器
}

// 写探针：cur+1（float 为 +1.0f）→ 读回 → 立刻写回 cur
static void MirrorWriteProbe(void* u, void* st, void* oknd, int cur, uint64_t now) {
    for (int i = 0; i < g_mirrorN; ++i) {
        if (now - g_mirror[i].lastProbe < 10000) continue;      // 每候选 10s 一次
        void* base = (g_mirror[i].where == MW_UNIT) ? u
                   : (g_mirror[i].where == MW_STATE ? st : oknd);
        if (!OkPtr(base)) continue;
        const size_t off = g_mirror[i].off;
        if (!RangePerm((uintptr_t)base + off, 4, 2)) continue;
        g_mirror[i].lastProbe = now;

        const bool isFloat = (g_mirror[i].type == 1);
        float back = 0.0f, restored = 0.0f;
        bool ok = Guarded("hp_probe", [&] {
            uintptr_t a = (uintptr_t)base + off;
            if (isFloat) {
                *reinterpret_cast<volatile float*>(a) = (float)cur + 1.0f;
                back = *reinterpret_cast<float*>(a);
                *reinterpret_cast<volatile float*>(a) = (float)cur;      // 立刻写回
            } else {
                *reinterpret_cast<volatile int*>(a) = cur + 1;
                back = (float)*reinterpret_cast<int*>(a);
                *reinterpret_cast<volatile int*>(a) = cur;               // 立刻写回
            }
        });
        Guarded("hp_probe_restore_read", [&] {
            uintptr_t a = (uintptr_t)base + off;
            restored = isFloat ? *reinterpret_cast<float*>(a)
                               : (float)*reinterpret_cast<int*>(a);
        });
        const float want = (float)cur + 1.0f;
        const int stick = (ok && AbsF(back - want) <= 1e-3f) ? 1 : 0;
        if (isFloat) {
            LOGI(ABS_LOG " hp_probe off=0x%zX where=%s type=float cur=%d wrote=%.3f back=%.3f stick=%d restored=%.3f",
                 off, WhereName(g_mirror[i].where), cur, (double)want, (double)back, stick, (double)restored);
        } else {
            LOGI(ABS_LOG " hp_probe off=0x%zX where=%s type=int cur=%d wrote=%d back=%d stick=%d restored=%d",
                 off, WhereName(g_mirror[i].where), cur, cur + 1, (int)back, stick, (int)restored);
        }
    }
}



// ============================================================================
//  v1.7.1 追加：阵营真值 + 属性容器按 id 读 + setter 候选探测
//    recon：Unit+0x198 是属性缓存（FEDPMGDPLGD 里 bl PHBGLMHPMMC::IFEHBJOMMMB →
//    按属性 id 0x3A 取值 → 同值写 0x194 与 0x198）⇒ 真值应在属性层（id 0x3A）。
// ============================================================================
static uint64_t g_lastFactionLog = 0;
static uint64_t g_lastAttrLog = 0;
static uint64_t g_lastSetterLog = 0;

struct AttrCtx {
    void* oknd = nullptr;
    void* miRead2 = nullptr;    // CGIBCFFNFGL(int,int)
    void* miRead1 = nullptr;    // CGIBCFFNFGL(int)
    void* miSetFloat = nullptr; // GPHAMODMGHC(float)
    void* miSetA = nullptr;     // ONCOLFNNMBB(...)
    void* miSetB = nullptr;     // HCFPHOCMANE(...)
    bool  resolved = false;
    bool  failed = false;
};
static AttrCtx g_attr;

static void AttrResolve(void* oknd) {
    if (!oknd || g_attr.oknd == oknd) return;
    g_attr = AttrCtx();
    g_attr.oknd = oknd;
    if (!g_api.object_class || !g_api.class_method) { g_attr.failed = true; return; }
    void* k = nullptr;
    Guarded("attr_class", [&] { k = g_api.object_class(oknd); });
    if (!k) { g_attr.failed = true; return; }
    Guarded("attr_methods", [&] {
        g_attr.miRead1 = g_api.class_method(k, "CGIBCFFNFGL", 1);          // lead 定稿：唯一读接口，1 参
        g_attr.miRead2 = g_attr.miRead1 ? nullptr : g_api.class_method(k, "CGIBCFFNFGL", 2);
        g_attr.miSetFloat = g_api.class_method(k, "GPHAMODMGHC", 1);
        g_attr.miSetA = g_api.class_method(k, "ONCOLFNNMBB", 1);
        g_attr.miSetB = g_api.class_method(k, "HCFPHOCMANE", 1);
    });
    g_attr.resolved = (g_attr.miRead2 || g_attr.miRead1);
    LOGI(ABS_LOG " attr resolve read2=%p read1=%p setFloat=%p setA=%p setB=%p",
         g_attr.miRead2, g_attr.miRead1, g_attr.miSetFloat, g_attr.miSetA, g_attr.miSetB);
}

// 按属性 id 读一个值（返回是否成功）
static bool AttrRead(void* oknd, int id, int* out) {
    if (!oknd || !g_api.runtime_invoke) return false;
    if (!g_attr.miRead2 && !g_attr.miRead1) return false;
    bool ok = false;
    Guarded("attr_read", [&] {
        if (g_attr.miRead2) {
            int32_t a0 = id, a1 = 0;
            void* args[2] = {&a0, &a1};
            void* ret = g_api.runtime_invoke(g_attr.miRead2, oknd, args, nullptr);
            // 返回可能是装箱 int 或直接 int（AArch64 小整数也可能是指针）——两种都试
            if (ret) {
                int64_t asInt = (int64_t)(intptr_t)ret;
                if (asInt > -1000000 && asInt < 1000000) { *out = (int)asInt; ok = true; return; }
                int boxed = 0;
                if (RangePerm((uintptr_t)ret + 0x10, 4, 1)) {
                    Guarded("attr_unbox", [&] { boxed = *reinterpret_cast<int*>((uintptr_t)ret + 0x10); });
                    *out = boxed; ok = true;
                }
            }
        } else {
            int32_t a0 = id;
            void* args[1] = {&a0};
            void* ret = g_api.runtime_invoke(g_attr.miRead1, oknd, args, nullptr);
            if (ret) {
                int64_t asInt = (int64_t)(intptr_t)ret;
                if (asInt > -1000000 && asInt < 1000000) { *out = (int)asInt; ok = true; }
            }
        }
    });
    return ok;
}


// ============================================================================
//  v1.8：属性 = 每单位一份的 int[]（recon 静态闭环，实测指令）
//    GGLILJGGPIK::ENBOJIGBIGL 0x36B266C：
//      ldr x8,[x20,#0x10]      ; this.values = int[]
//      ldr w9,[x8,#0x18]       ; len
//      cmp w9,w19 / b.le →0
//      ldr w0,[x8,#0x20+idx*4] ; return values[id]
//    setter 0x36B26F0 = str w20,[x8,#0x20+idx*4]
//  取值链：unit+0x120(state) → IFEHBJOMMMB() → oknd → *(oknd+0x50) → +0x10 是 int[]
//          数组：len @+0x18，数据 @+0x20，下标 *4
// ============================================================================

// 三个写入点的 before 缓存（关开关时还原，属性是数组，还原安全）
struct AttrSaved { int before62 = 0, before36 = 0;
                   bool has62 = false, has36 = false; };

// v1.9.5：无限技能 = 属性组（recon 实测有真实读取点的 5 个 id + 1 个对照）
//   48 ENERGY_RECOVER_ADD / 49 ENERGY_RECOVER_RATE / 51 RELIC_SKILL_CDR /
//   52 DASH_CDR / 57 PASSIVE_A_SKILL_CDR / 17 能耗折扣（无读取点，对照）
//   量纲（recon 反汇编 IHHHPKIGIBN 0x3641CD0 定死）：
//     id48 = 绝对加值；id49 = 万分比（10000 = +100%，50000 = ×6）
//     id51/52/57 CDR 量纲未知，先用 100；id17 无读取点，作对照
static int  g_grpBefore[6] = {0, 0, 0, 0, 0, 0};
static bool g_grpHad[6]    = {false, false, false, false, false, false};
static bool g_grpLogged   = false;
static AttrSaved g_attrSaved;
static uint64_t g_lastAttrViewLog = 0;
static uint64_t g_lastHitViewLog = 0;

// 一行对照视图（3s）
static void AttrViewLog(void* u, void* oknd, uint64_t now) {
    if (now - g_lastAttrViewLog < 3000) return;
    g_lastAttrViewLog = now;
    AttrImplResolve(oknd);
    int id3 = 0, id9 = 0, id15 = 0, id28 = 0, id36 = 0, id37 = 0, id40 = 0, id62 = 0;
    AttrRwRead(oknd, 3, &id3);   AttrRwRead(oknd, 9, &id9);   AttrRwRead(oknd, 15, &id15);
    AttrRwRead(oknd, 28, &id28); AttrRwRead(oknd, 36, &id36); AttrRwRead(oknd, 37, &id37);
    AttrRwRead(oknd, 40, &id40); AttrRwRead(oknd, 62, &id62);
    if (g_impl.isAGNK) {
        void* dict = OkPtr(g_impl.attrset) ? ReadPtrAt(g_impl.attrset, 0x18) : nullptr;
        LOGI(ABS_LOG " attr-view uid=%p impl=%s id3=%d id9=%d id15=%d id28=%d id36=%d id37=%d id40=%d id62=%d dict=%p",
             u, g_impl.name, id3, id9, id15, id28, id36, id37, id40, id62, dict);
    } else {
        AttrView v = AttrArray(oknd);
        LOGI(ABS_LOG " attr-view uid=%p impl=%s id3=%d id9=%d id15=%d id28=%d id36=%d id37=%d id40=%d id62=%d len=%d arr=%p",
             u, g_impl.name, id3, id9, id15, id28, id36, id37, id40, id62, v.len, v.arr);
    }
}


static uint64_t g_lastEnergyObs = 0;

static void EnergyObsLog(void* oknd, void* u, uint64_t now) {
    if (now - g_lastEnergyObs < 3000) return;
    g_lastEnergyObs = now;
    int v11 = -1, v16 = -1, v48 = -1, v49 = -1, v17 = -1, v51 = -1, v52 = -1, v57 = -1;
    AttrRwRead(oknd, 11, &v11);
    AttrRwRead(oknd, 16, &v16);
    AttrRwRead(oknd, 48, &v48);
    AttrRwRead(oknd, 49, &v49);
    AttrRwRead(oknd, 17, &v17);
    AttrRwRead(oknd, 51, &v51);
    AttrRwRead(oknd, 52, &v52);
    AttrRwRead(oknd, 57, &v57);
    LOGI(ABS_LOG " energy-obs uid=%p impl=%s id11=%d id16=%d id48=%d id49=%d id17=%d id51=%d id52=%d id57=%d",
         u, g_impl.name, v11, v16, v48, v49, v17, v51, v52, v57);
}

// 写入执行：无敌(37→退40) / 伤害倍率(28) / 技能(36)；关闭即还原 before
static void AttrApplyPass(const UnitSlot* us, int n, bool godOn, int dmgMult, bool skillOn, uint64_t now) {
    for (int i = 0; i < n; ++i) {
        if (us[i].faction != kFactionPlayer) continue;
        void* u = us[i].unit;
        void* st = g_anchors.state.ok ? ReadPtrAt(u, g_anchors.state.off) : nullptr;
        const char* why = "-";
        void* oknd = ResolveOknd(st, &why);
        if (!OkPtr(oknd)) {
            if (now - g_lastAttrViewLog > 3000) {
                g_lastAttrViewLog = now;
                LOGI(ABS_LOG " attr skip reason=oknd null why=%s", why);
            }
            return;
        }
        AttrImplResolve(oknd);   // v1.9.1：实现类判别 + 方法解析

        // ---- 无敌：values[62] = 1（DAMAGE_GUARD，G6 静态已证是真正开关）----
        if (godOn) {
            if (!g_attrSaved.has62) { int b0 = 0; if (AttrRwRead(oknd, 62, &b0)) { g_attrSaved.before62 = b0; g_attrSaved.has62 = true; } }
            int before = 0;
            if (!AttrRwRead(oknd, 62, &before)) {
                LOGW(ABS_LOG " attr skip reason=id62 unreadable impl=%s", g_impl.name);
            } else if (before != 1) {
                AttrRwWrite(oknd, 62, 1, "attr62");
                int after = -999;
                AttrRwRead(oknd, 62, &after);
                if (after == 1) LOGI(ABS_LOG " attr62 set=%d before=%d after=%d", 1, before, after);
                else LOGW(ABS_LOG " attr write-fail id=62 before=%d target=1 after=%d", before, after);
            }
        } else if (g_attrSaved.has62) {
            AttrRwWrite(oknd, 62, g_attrSaved.before62, "attr62");
            LOGI(ABS_LOG " attr62 restore=%d", g_attrSaved.before62);
            g_attrSaved.has62 = false;
        }

        // ---- 无限技能：属性组（51/52/57/48/49/17），关开关逐条还原 ----
        if (skillOn) {
            for (int gi = 0; gi < 6; ++gi) {
                const int id = kSkillGrpIdsCfg[gi];
                const int targetV = (id == 48) ? kEnergyAdd
                                  : (id == 49) ? kEnergyRate
                                  : (id == 17) ? kDiscountV : kCdrVal;
                if (!g_grpHad[gi]) {
                    int b0 = 0;
                    if (AttrRwRead(oknd, id, &b0)) { g_grpBefore[gi] = b0; g_grpHad[gi] = true; }
                }
                int before = 0;
                if (!AttrRwRead(oknd, id, &before)) {
                    LOGW(ABS_LOG " attr skip reason=id unreadable id=%d impl=%s", id, g_impl.name);
                    continue;
                }
                if (before == targetV) continue;
                AttrRwWrite(oknd, id, targetV, "attrgrp");
                int after = -999999;
                AttrRwRead(oknd, id, &after);
                if (after != targetV) {
                    LOGW(ABS_LOG " attr write-fail id=%d before=%d target=%d got=%d", id, before, targetV, after);
                }
            }
            if (!g_grpLogged) {
                g_grpLogged = true;
                LOGI(ABS_LOG " attrgrp ok ids=51,52,57,48,49,17（写入成功，后续静默）");
            }
        } else {
            for (int gi = 0; gi < 6; ++gi) {
                if (!g_grpHad[gi]) continue;
                AttrRwWrite(oknd, kSkillGrpIdsCfg[gi], g_grpBefore[gi], "attrgrp");
                LOGI(ABS_LOG " attrgrp restore id=%d v=%d", kSkillGrpIdsCfg[gi], g_grpBefore[gi]);
                g_grpHad[gi] = false;
            }
        }

        // v1.9.2：受击旁证——id3/id15/id62 任一变化即打 hit-view（读失败也打，字段给 -1）
        {
            static int lastA3 = -1, lastA15 = -1, lastA62 = -1;
            int c3 = -1, c15 = -1, c62 = -1;
            AttrRwRead(oknd, 3, &c3);
            AttrRwRead(oknd, 15, &c15);
            bool ok62 = AttrRwRead(oknd, 62, &c62);
            if (c3 != lastA3 || c15 != lastA15 || c62 != lastA62) {
                if (now - g_lastHitViewLog > 1500) {
                    g_lastHitViewLog = now;
                    LOGI(ABS_LOG " hit-view 190=%lld 1A0=%lld attr62=%d impl=%s guard=%d",
                         (long long)ReadI64(u, kDumpOffHpCur), (long long)ReadI64(u, kDumpOffHpMax),
                         c62, g_impl.name, (ok62 && c62 > 0) ? 1 : 0);
                }
                lastA3 = c3; lastA15 = c15; lastA62 = c62;
            }
        }
        AttrViewLog(u, oknd, now);
        return;
    }
}

// Stage C：伤害倍率 —— v1.7 起**停止写入**（0x198 疑为镜像，写不进真值）
//   仅保留只读观测：谁掉了多少血，供下一轮判定镜像/真值关系。
static uint64_t g_lastDmgxObsLog = 0;

static void Dmgx198Pass(const UnitSlot* us, int n, int mult, uint64_t now) {
    if (g_hp198.state != HP198_CONFIRMED) return;
    for (int i = 0; i < n; ++i) {
        if (us[i].faction != kFactionEnemy) continue;
        void* u = us[i].unit;
        int cur = ReadHp198(u);
        if (cur < 0) continue;
        EnemyHp* c = EnemySlot(u, now);
        int prev = c->hp;
        c->hp = cur;
        c->seen = now;
        if (prev <= 0 || cur >= prev) continue;
        int delta = prev - cur;
        if (delta <= 0 || delta > kHp198MaxDelta) continue;
        if (now - g_lastDmgxObsLog > 1000) {
            g_lastDmgxObsLog = now;
            LOGI(ABS_LOG " dmgx198 observed faction=2 delta=%d mult=%d before=%d after=%d killed=%d unit=%p（v1.7 不写）",
                 delta, mult, prev, cur, (cur <= 0 ? 1 : 0), u);
        }
    }
}

// v1.5：技能保持关闭（写代码不编进 so）
static void SkillPass(const UnitSlot*, int, bool) {}

#else
// ===== 探针版 / 移速实验版：以上功能全部空实现 ⇒ so 内不含这些功能的 write 字符串 =====
static void SkillPass(const UnitSlot*, int, bool) {}
#endif

// oknd 取值链（recon §12.2）：state → IFEHBJOMMMB() → FPIALCOELGG（OKNDCLIKECF 子类）
//   兜底：运行时类取不到就退到 PHBGLMHPMMC 上的同名方法。
static void* ResolveOknd(void* state, const char** whyOut) {
    if (!OkPtr(state)) { if (whyOut) *whyOut = "state ptr null"; return nullptr; }
    if (!g_api.object_class || !g_api.class_method || !g_api.runtime_invoke) {
        if (whyOut) *whyOut = "missing symbol (object_class/class_method/runtime_invoke)";
        return nullptr;
    }
    void* out = nullptr;
    const char* why = "IFEHBJOMMMB miss";
    Guarded("oknd_chain", [&] {
        void* k = g_api.object_class(state);
        void* m = k ? g_api.class_method(k, "IFEHBJOMMMB", 0) : nullptr;
        if (!m && g_image && g_api.class_from_name) {
            void* base = g_api.class_from_name(g_image, "", "PHBGLMHPMMC");
            if (base) m = g_api.class_method(base, "IFEHBJOMMMB", 0);
            if (m) why = "used PHBGLMHPMMC fallback";
        }
        if (!m) return;
        void* ret = g_api.runtime_invoke(m, state, nullptr, nullptr);   // 引用类型返回，直接可用
        if (!OkPtr(ret)) { why = "invoke returned null"; return; }
        out = ret;
        why = "ok";
    });
    if (whyOut) *whyOut = why;
    return out;
}

static void SpeedPass(const UnitSlot* us, int n) {
    uint64_t now = NowMs();
    if (now - g_lastSpeedLog < 5000) return;      // 5s 节流；runtime_invoke 每个窗口最多一次
    g_lastSpeedLog = now;
    for (int i = 0; i < n; ++i) {
        if (us[i].faction != kFactionPlayer) continue;
        void* u = us[i].unit;
        void* st = g_anchors.state.ok ? ReadPtrAt(u, g_anchors.state.off) : nullptr;
        const char* why = "-";
        void* oknd = ResolveOknd(st, &why);

        long long s18 = 0, s20 = 0, s28 = 0, s30 = 0, s38 = 0, s40 = 0;
        float f48 = 0, f4c = 0;
        if (OkPtr(oknd)) {
            s18 = (long long)ReadI64(oknd, 0x18);
            s20 = (long long)ReadI64(oknd, 0x20);
            s28 = (long long)ReadI64(oknd, 0x28);
            s30 = (long long)ReadI64(oknd, 0x30);
            s38 = (long long)ReadI64(oknd, 0x38);
            s40 = (long long)ReadI64(oknd, 0x40);
            f48 = ReadF32(oknd, 0x48);
            f4c = ReadF32(oknd, 0x4C);
        }
        // 驱动器：Unit+0x38（GAMJINGNDHM : KPBKFHOKGHJ）
        void* drv = g_anchors.driver.ok ? ReadPtrAt(u, g_anchors.driver.off) : nullptr;
        void* drvUnit = OkPtr(drv) ? ReadPtrAt(drv, 0x20) : nullptr;   // +0x20 AIMKJECPDPI
        float drv28 = OkPtr(drv) ? ReadF32(drv, 0x28) : 0.0f;          // +0x28 JBNOMHPKBMP
        float drv30 = OkPtr(drv) ? ReadF32(drv, 0x30) : 0.0f;          // +0x30 BJLAKIJPDFI
        bool drvBack = (drvUnit == u);

        long long hpCur = g_anchors.hpCur.ok ? (long long)ReadI64(u, g_anchors.hpCur.off) : 0;
        long long hpMax = g_anchors.hpMax.ok ? (long long)ReadI64(u, g_anchors.hpMax.off) : 0;
        void* cc = g_anchors.cc.ok ? ReadPtrAt(u, g_anchors.cc.off) : nullptr;

        LOGI(ABS_LOG " speed_probe unit=%p state=%p oknd=%p why=%s", u, st, oknd, why);
        LOGI(ABS_LOG " speed_probe slots6 long [+0x18=%lld] [+0x20=%lld] [+0x28=%lld] [+0x30=%lld] [+0x38=%lld] [+0x40=%lld]",
             s18, s20, s28, s30, s38, s40);
        LOGI(ABS_LOG " speed_probe floats +0x48=%.2f +0x4C=%.2f hp cur=%lld max=%lld cc=%p",
             f48, f4c, hpCur, hpMax, cc);
        LOGI(ABS_LOG " speed_probe drv=%p back=%d +0x28=%.3f +0x30=%.3f speed: pending-anchor",
             drv, (int)drvBack, drv28, drv30);
        return;
    }
}


// ============================================================================
//  v1.4-A 移速写入（唯一写点 oknd+0x48）——修「不受滑块控制 / 滑块=1 仍很快」
//
//  三处根因针对性修复（lead 2026-11-15）：
//   ① base 捕获时机：只在**写之前**、且连续 2 次读到同一值时才锁定 base；
//      锁定后 base 不再变，除非外部把值改成一个「既不是 base、也不在 base×mult 集合内」的数。
//   ② 滑块<=1：必须还原，且**写后读回确认** cur==base，不相等最多重试 3 次。
//   ③ 派生值覆盖：每 tick 读回比对，|cur-target|>0.01 就重写，并单独打 speed drift（5s 节流）。
//
//  倍率：slider 1 不写；2..10 → mult = 1.0 + (slider-1)×0.25（2→1.25× … 10→3.25×）
//  写入钳制：[base×0.5, base×5.0]，越界打 clamp。
// ============================================================================
#if !defined(STARRY_PROBE) && !defined(STARRY_HOOK_PROBE)
static float    g_speedBase = 0.0f;
static bool     g_speedBaseLocked = false;
static void*    g_speedOknd = nullptr;
static bool     g_speedWrote = false;
static uint64_t g_lastSpeedWriteLog = 0;
static uint64_t g_lastDriftLog = 0;

// 未锁定前的稳定性跟踪（首次进战斗不许写）
static void*    g_stableOknd = nullptr;
static float    g_stableVal = 0.0f;
static int      g_stableCnt = 0;

// 还原 + 读回确认（最多 3 次）
static bool SpeedRestoreVerify(bool verbose) {
    if (!g_speedOknd || g_speedBase <= 0.0f || !OkPtr(g_speedOknd)) {
        g_speedOknd = nullptr; g_speedBase = 0.0f; g_speedWrote = false;
        g_speedBaseLocked = false; g_stableCnt = 0; g_stableOknd = nullptr;
        return true;
    }
    bool ok = false;
    for (int attempt = 1; attempt <= 3; ++attempt) {
        if (!RangePerm((uintptr_t)g_speedOknd + 0x48, 4, 1)) break;
        Guarded("speed_restore", [&] { WriteF32(g_speedOknd, 0x48, g_speedBase); });
        float back = 0.0f;
        if (!Guarded("speed_restore_readback", [&] { back = ReadF32(g_speedOknd, 0x48); })) break;
        if (AbsF(back - g_speedBase) <= 1e-3f) { ok = true; break; }
        LOGW(ABS_LOG " speed restore retry=%d back=%.3f base=%.3f", attempt, back, g_speedBase);
    }
    if (verbose) {
        if (ok) LOGI(ABS_LOG " speed restore ok base=%.2f", g_speedBase);
        else    LOGW(ABS_LOG " speed restore fail（读回不一致，字段疑为派生值）");
    }
    g_speedWrote = false;
    g_speedOknd = nullptr;
    g_speedBase = 0.0f;
    g_speedBaseLocked = false;
    g_stableCnt = 0;
    g_stableOknd = nullptr;
    return ok;
}

static void SpeedWritePass(const UnitSlot* us, int n) {
    int slider = g_speedX.load();                          // 1..10
    uint64_t now = NowMs();

    // ② 滑块=1：不写 + 还原 + 读回确认
    if (slider <= 1) {
        if (g_speedWrote || g_speedOknd) SpeedRestoreVerify(true);
        return;
    }
    const float mult = 1.0f + (float)(slider - 1) * 0.25f;  // 2→1.25x … 10→3.25x

    for (int i = 0; i < n; ++i) {
        if (us[i].faction != kFactionPlayer) continue;
        void* u = us[i].unit;
        void* st = g_anchors.state.ok ? ReadPtrAt(u, g_anchors.state.off) : nullptr;
        const char* why = "-";
        void* oknd = ResolveOknd(st, &why);
        if (!OkPtr(oknd) || !RangePerm((uintptr_t)oknd + 0x48, 4, 1)) {
            if (now - g_lastSpeedWriteLog > 5000) {
                g_lastSpeedWriteLog = now;
                LOGI(ABS_LOG " speed skip reason=oknd unusable why=%s oknd=%p", why, oknd);
            }
            return;
        }
        float cur = 0.0f;
        if (!Guarded("speed_readback", [&] { cur = ReadF32(oknd, 0x48); })) return;
        if (!(cur >= 0.1f && cur <= 50.0f)) {
            if (now - g_lastSpeedWriteLog > 5000) {
                g_lastSpeedWriteLog = now;
                LOGI(ABS_LOG " speed skip reason=range cur=%.3f（期望 0.1..50）", cur);
            }
            return;
        }

        // ① base 只捕获一次：写之前、连续 2 次同值（等战斗加载稳定）
        if (!g_speedBaseLocked) {
            if (g_stableOknd == oknd && AbsF(cur - g_stableVal) <= 1e-4f) {
                ++g_stableCnt;
            } else {
                g_stableOknd = oknd; g_stableVal = cur; g_stableCnt = 1;
            }
            if (g_stableCnt < 2) {
                if (now - g_lastSpeedWriteLog > 5000) {
                    g_lastSpeedWriteLog = now;
                    LOGI(ABS_LOG " speed wait-stable cur=%.3f n=%d（未锁定 base，不写）", cur, g_stableCnt);
                }
                return;
            }
            g_speedOknd = oknd;
            g_speedBase = cur;
            g_speedBaseLocked = true;
            LOGI(ABS_LOG " speed base captured base=%.3f at=%llu（写前锁定，之后不再变）",
                 g_speedBase, (unsigned long long)now);
        } else if (g_speedOknd != oknd) {
            if (g_speedWrote) SpeedRestoreVerify(false);
            g_speedOknd = oknd;
            g_speedBase = cur;
            g_stableOknd = oknd; g_stableVal = cur; g_stableCnt = 2;
            LOGI(ABS_LOG " speed base re-captured（oknd 变化）base=%.3f oknd=%p", g_speedBase, oknd);
        } else {
            // 外部把值改成「既非 base、也不在 base×mult 集合内」⇒ 重捕
            bool known = AbsF(cur - g_speedBase) <= 1e-3f;
            if (!known) {
                for (int s = 2; s <= 10; ++s) {
                    float m = 1.0f + (float)(s - 1) * 0.25f;
                    if (AbsF(cur - g_speedBase * m) <= 1e-3f) { known = true; break; }
                }
            }
            if (!known) {
                g_speedBase = cur;
                LOGI(ABS_LOG " speed base re-captured（外部改动）base=%.3f", g_speedBase);
            }
        }

        float target = g_speedBase * mult;
        const float lo = g_speedBase * 0.5f, hi = g_speedBase * 5.0f;
        if (target < lo) {
            target = lo;
            LOGI(ABS_LOG " speed clamp target→%.3f（下限 base×0.5）", target);
        } else if (target > hi) {
            target = hi;
            LOGI(ABS_LOG " speed clamp target→%.3f（上限 base×5.0）", target);
        }

        // ③ 漂移检测：与目标差 >0.01 就(重)写
        if (AbsF(cur - target) > 0.01f) {
            if (ReadI32(u, g_anchors.faction.off) != kFactionPlayer) return;   // 写前重读
            Guarded("speed_write", [&] { WriteF32(oknd, 0x48, target); });
            float back = 0.0f;
            Guarded("speed_write_readback", [&] { back = ReadF32(oknd, 0x48); });
            if (g_speedWrote && now - g_lastDriftLog > 5000) {
                g_lastDriftLog = now;
                LOGI(ABS_LOG " speed drift cur=%.2f target=%.2f back=%.2f（值被覆盖 ⇒ 疑派生值）",
                     cur, target, back);
            }
            g_speedWrote = true;
            if (now - g_lastSpeedWriteLog > 5000) {
                g_lastSpeedWriteLog = now;
                LOGI(ABS_LOG " write ok speed oknd+0x48 base=%.2f mult=%.2f target=%.2f cur=%.2f back=%.2f",
                     g_speedBase, mult, target, cur, back);
            }
        } else if (now - g_lastSpeedWriteLog > 5000) {
            g_lastSpeedWriteLog = now;
            LOGI(ABS_LOG " speed steady base=%.2f mult=%.2f target=%.2f cur=%.2f",
                 g_speedBase, mult, target, cur);
        }
        return;
    }
}
#endif  // !STARRY_PROBE && !STARRY_HOOK_PROBE

// ============================================================================
//  v1.3-B 血量字段自动定位（**只读**，仅移速实验版编译）
//    对首个 faction==1 的单位，逐 8 字节采样 Unit+0x00..0x2C0 与 state+0x00..0x200：
//      prev > 0 && now < prev && prev-now <= 65535              ⇒ hp_cand（整数位点）
//      低位按 float 解：prev ∈ [1,1e5] && now < prev          ⇒ hp_cand_f
//    节流：每偏移 10s 一条；整包 200 条/分钟上限，超了丢弃。
// ============================================================================
#ifdef STARRY_SPEED_ONLY
constexpr size_t kUnitScanEnd = 0x2C0;
constexpr size_t kStateScanEnd = 0x200;
constexpr size_t kScanStep = 8;
constexpr int kUnitSlots = (int)(kUnitScanEnd / kScanStep);    // 88
constexpr int kStateSlots = (int)(kStateScanEnd / kScanStep);  // 64

struct ScanState {
    bool     init = false;
    uint64_t u64[kUnitSlots];
    uint64_t s64[kStateSlots];
    float    uf[kUnitSlots];
    float    sf[kStateSlots];
    uint64_t lastLogU[kUnitSlots];
    uint64_t lastLogS[kStateSlots];
};
static ScanState g_scan;
static int      g_scanBudget = 0;
static uint64_t g_scanBudgetAt = 0;

static bool ScanBudgetOk(uint64_t now) {
    if (now - g_scanBudgetAt > 60000) { g_scanBudgetAt = now; g_scanBudget = 0; }
    if (g_scanBudget >= 200) return false;
    ++g_scanBudget;
    return true;
}

static void HpScanPass(const UnitSlot* us, int n) {
    if (!g_anchors.state.ok) return;
    for (int i = 0; i < n; ++i) {
        if (us[i].faction != kFactionPlayer) continue;
        void* u = us[i].unit;
        void* st = ReadPtrAt(u, g_anchors.state.off);
        if (!OkPtr(u)) return;
        bool stOk = OkPtr(st);
        if (!RangePerm((uintptr_t)u, kUnitScanEnd, 1)) return;
        if (stOk && !RangePerm((uintptr_t)st, kStateScanEnd, 1)) stOk = false;

        uint64_t now = NowMs();
        for (int k = 0; k < kUnitSlots; ++k) {
            size_t off = (size_t)k * kScanStep;
            uint64_t cur = *reinterpret_cast<uint64_t*>((uintptr_t)u + off);
            int64_t  curI = (int64_t)cur;
            float    curF = *reinterpret_cast<float*>((uintptr_t)u + off);
            if (g_scan.init) {
                int64_t prevI = (int64_t)g_scan.u64[k];
                float   prevF = g_scan.uf[k];
                if (prevI > 0 && curI < prevI && (prevI - curI) <= 65535 &&
                    now - g_scan.lastLogU[k] > 10000 && ScanBudgetOk(now)) {
                    g_scan.lastLogU[k] = now;
                    LOGI(ABS_LOG " hp_cand where=unit off=0x%zX prev=%lld now=%lld delta=%lld",
                         off, (long long)prevI, (long long)curI, (long long)(prevI - curI));
                } else if (prevF >= 1.0f && prevF <= 100000.0f && curF < prevF && curF > 0.0f &&
                           now - g_scan.lastLogU[k] > 10000 && ScanBudgetOk(now)) {
                    g_scan.lastLogU[k] = now;
                    LOGI(ABS_LOG " hp_cand_f where=unit off=0x%zX prev=%.3f now=%.3f",
                         off, prevF, curF);
                }
            }
            g_scan.u64[k] = cur;
            g_scan.uf[k] = curF;
        }
        if (stOk) {
            for (int k = 0; k < kStateSlots; ++k) {
                size_t off = (size_t)k * kScanStep;
                uint64_t cur = *reinterpret_cast<uint64_t*>((uintptr_t)st + off);
                int64_t  curI = (int64_t)cur;
                float    curF = *reinterpret_cast<float*>((uintptr_t)st + off);
                if (g_scan.init) {
                    int64_t prevI = (int64_t)g_scan.s64[k];
                    float   prevF = g_scan.sf[k];
                    if (prevI > 0 && curI < prevI && (prevI - curI) <= 65535 &&
                        now - g_scan.lastLogS[k] > 10000 && ScanBudgetOk(now)) {
                        g_scan.lastLogS[k] = now;
                        LOGI(ABS_LOG " hp_cand where=state off=0x%zX prev=%lld now=%lld delta=%lld",
                             off, (long long)prevI, (long long)curI, (long long)(prevI - curI));
                    } else if (prevF >= 1.0f && prevF <= 100000.0f && curF < prevF && curF > 0.0f &&
                               now - g_scan.lastLogS[k] > 10000 && ScanBudgetOk(now)) {
                        g_scan.lastLogS[k] = now;
                        LOGI(ABS_LOG " hp_cand_f where=state off=0x%zX prev=%.3f now=%.3f",
                             off, prevF, curF);
                    }
                }
                g_scan.s64[k] = cur;
                g_scan.sf[k] = curF;
            }
        }
        g_scan.init = true;
        return;
    }
}
#endif  // STARRY_SPEED_ONLY

} // namespace

// ============================================================================
//  ⑨ 对外接口（ModEntry.h 注册表契约）
// ============================================================================
// ============================================================================
//  v1.8a：Dobby inline hook 存在性验证（只读，不写内存，不改玩法）
//
//  背景：我们一直假设 houdini 转译层上 inline hook 不可靠，但从未单独验证。
//  做法：对两个「启动到主界面必然被调用」的方法各挂一个 hook：
//    · OKNDCLIKECF::HKLDOFIAKDD（血条取值链末端，RVA 0x36A0C28）
//    · Unit::KKGNKHFDCBG（每帧状态刷新，RVA 0x354A5D4）
//  桩函数只计数 + 每 50 次打一条日志，然后转调原函数。
//  失败（DobbyHook 非 0 / 方法指针无效）⇒ hook_skip，不崩、不影响解析链。
// ============================================================================
#if defined(STARRY_HOOK_PROBE)
#include "Dobby/dobby.h"

static std::atomic<int> g_hklN{0};
static std::atomic<int> g_kkgN{0};

typedef int  (*fn_int_self)(void*);
typedef void (*fn_void_self)(void*);
static fn_int_self  g_hklOrig = nullptr;
static fn_void_self g_kkgOrig = nullptr;

static int Stub_Hkl(void* self) {
    int ret = 0;
    if (g_hklOrig) ret = g_hklOrig(self);
    int n = ++g_hklN;
    if (n == 1 || (n % 50) == 0) LOGI(ABS_LOG " hook_ok hkl n=%d ret=%d", n, ret);
    return ret;
}
static void Stub_Kkg(void* self) {
    int n = ++g_kkgN;
    if (n == 1 || (n % 50) == 0) LOGI(ABS_LOG " hook_ok kkg n=%d", n);
    if (g_kkgOrig) g_kkgOrig(self);
}

// 取方法指针：优先 methodPointer（MethodInfo 首字段），失败退回 module_base + RVA
static void* MethodPtr(void* image, const char* klass, const char* method, uintptr_t rva, const char* tag) {
    void* mi = nullptr;
    if (image) {
        void* k = nullptr;
        Guarded("hook_class", [&] { k = g_api.class_from_name(image, "", klass); });
        if (k) {
            Guarded("hook_method", [&] { mi = g_api.class_method ? g_api.class_method(k, method, 0) : nullptr; });
        }
    }
    if (mi) {
        void* mp = nullptr;
        Guarded("hook_mi", [&] { mp = ReadPtrAt(mi, 0); });   // MethodInfo::methodPointer 在首字段
        if (mp && (uintptr_t)mp >= g_api.base && (uintptr_t)mp < g_api.end) {
            LOGI(ABS_LOG " hook ptr %s by-name=%p (mi=%p)", tag, mp, mi);
            return mp;
        }
        LOGW(ABS_LOG " hook ptr %s by-name 无效 mp=%p，退回 RVA", tag, mp);
    }
    void* fallback = (void*)(g_api.base + rva);
    if (g_api.base && (uintptr_t)fallback < g_api.end) {
        LOGW(ABS_LOG " hook ptr %s fallback=base+0x%zX=%p", tag, (size_t)rva, fallback);
        return fallback;
    }
    return nullptr;
}

static void HookProbeRun() {
    static std::atomic<bool> ran{false};
    if (ran.exchange(true)) return;
    if (!g_api.ready) return;
    LOGI(ABS_LOG " hook_probe start base=%p end=%p", (void*)g_api.base, (void*)g_api.end);

    void* p1 = MethodPtr(g_image, "OKNDCLIKECF", "HKLDOFIAKDD", 0x36A0C28, "hkl");
    if (!p1) {
        LOGW(ABS_LOG " hook_skip name=hkl rc=-1 reason=ptr null");
    } else {
        int rc = DobbyHook(p1, (dobby_dummy_func_t)&Stub_Hkl, (dobby_dummy_func_t*)&g_hklOrig);
        if (rc != 0) LOGW(ABS_LOG " hook_skip name=hkl rc=%d addr=%p", rc, p1);
        else LOGI(ABS_LOG " hook installed hkl addr=%p orig=%p", p1, (void*)g_hklOrig);
    }

    void* p2 = MethodPtr(g_image, "Unit", "KKGNKHFDCBG", 0x354A5D4, "kkg");
    if (!p2) {
        LOGW(ABS_LOG " hook_skip name=kkg rc=-1 reason=ptr null");
    } else {
        int rc = DobbyHook(p2, (dobby_dummy_func_t)&Stub_Kkg, (dobby_dummy_func_t*)&g_kkgOrig);
        if (rc != 0) LOGW(ABS_LOG " hook_skip name=kkg rc=%d addr=%p", rc, p2);
        else LOGI(ABS_LOG " hook installed kkg addr=%p orig=%p", p2, (void*)g_kkgOrig);
    }
    LOGI(ABS_LOG " hook_probe done（n_hkl=%d n_kkg=%d）", g_hklN.load(), g_kkgN.load());
}
#endif  // STARRY_HOOK_PROBE


// ============================================================================
//  v1.9：伤害倍率 = hook Unit::BLKEJBDMGAB（RVA 0x354B388）
//    载荷是 struct，同帧可能复用 ⇒ **禁止原地改**：memcpy 到栈上副本，
//    只把副本 +0x28 乘倍率（+0x38 不动），再把副本指针传给原函数。
//    DobbyHook 失败只降级（hook_skip），顺带回答「houdini 下 Dobby 能不能用」。
// ============================================================================
#if !defined(STARRY_PROBE) && !defined(STARRY_SPEED_ONLY)
#include "Dobby/dobby.h"

static std::atomic<int>  g_dmgHookN{0};
static uint64_t          g_lastDmgPayloadLog = 0;
static const uint64_t     kDmgLogInterval = 5000;   // v2.0：载荷日志 5s 节流
typedef void (*fn_dmg_t)(void*, void*, void*, void*);
static fn_dmg_t g_dmgOrig = nullptr;

static void* ResolveMethodPtr(void* image, const char* klass, const char* method,
                              uintptr_t rva, const char* tag) {
    void* mi = nullptr;
    if (image) {
        void* k = nullptr;
        Guarded("hook_class", [&] { k = g_api.class_from_name(image, "", klass); });
        if (k) Guarded("hook_method", [&] { mi = g_api.class_method ? g_api.class_method(k, method, 0) : nullptr; });
    }
    if (mi) {
        void* mp = nullptr;
        Guarded("hook_mi", [&] { mp = ReadPtrAt(mi, 0); });
        if (mp && (uintptr_t)mp >= g_api.base && (uintptr_t)mp < g_api.end) {
            LOGI(ABS_LOG " hook ptr %s by-name=%p mi=%p", tag, mp, mi);
            return mp;
        }
        LOGW(ABS_LOG " hook ptr %s by-name 无效 mp=%p，退回 RVA", tag, mp);
    }
    void* fb = (void*)(g_api.base + rva);
    if (g_api.base && (uintptr_t)fb < g_api.end) {
        LOGW(ABS_LOG " hook ptr %s fallback=base+0x%zX=%p", tag, (size_t)rva, fb);
        return fb;
    }
    return nullptr;
}

static bool LooksLikeUnit(void* p) {
    if (!OkPtr(p) || !g_anchors.faction.ok) return false;
    if (!RangePerm((uintptr_t)p + g_anchors.faction.off, 4, 1)) return false;
    int f = ReadI32(p, g_anchors.faction.off);
    return f >= 0 && f <= 2;
}

static void Stub_Dmg(void* a0, void* a1, void* a2, void* a3) {
    int n = ++g_dmgHookN;
    int mult = g_dmgX.load();
    // 载荷候选：成员函数时 a1 是载荷；非成员时 a0 是载荷。用「像不像 Unit」判定
    void* payload = nullptr;
    if (LooksLikeUnit(a0) && OkPtr(a1) && RangePerm((uintptr_t)a1 + 0x28, 8, 1)) payload = a1;
    if (payload && mult > 1) {
        if (mult > 100) mult = 100;                       // 上限钳制
        unsigned char copy[0x60];
        std::memcpy(copy, payload, sizeof(copy));
        // 三分量：+0x24(int) / +0x28(long) / +0x38(long)；delta = +0x28 + +0x38
        int32_t p24 = 0;
        int64_t p28 = 0, p38 = 0;
        std::memcpy(&p24, copy + 0x24, 4);
        std::memcpy(&p28, copy + 0x28, 8);
        std::memcpy(&p38, copy + 0x38, 8);
        const int32_t n24 = p24 * mult;
        const int64_t n28 = p28 * (int64_t)mult;
        const int64_t n38 = p38 * (int64_t)mult;
        uint64_t now = NowMs();
        if (p24 == 0 && p28 == 0 && p38 == 0) {
            if (now - g_lastDmgPayloadLog > kDmgLogInterval) {
                g_lastDmgPayloadLog = now;
                LOGI(ABS_LOG " dmgx zero-payload skip=1 mult=%d n=%d（三分量全 0，不改）", mult, n);
            }
            if (g_dmgOrig) g_dmgOrig(a0, payload, a2, a3);
            return;
        }
        std::memcpy(copy + 0x24, &n24, 4);
        std::memcpy(copy + 0x28, &n28, 8);
        std::memcpy(copy + 0x38, &n38, 8);   // +0x40(float) 不动（只用于表现）
        if (now - g_lastDmgPayloadLog > kDmgLogInterval) {
            g_lastDmgPayloadLog = now;
            LOGI(ABS_LOG " dmgx payload-copy mult=%d p24=%d->%d p28=%lld->%lld p38=%lld->%lld copy=0x60 n=%d",
                 mult, p24, n24, (long long)p28, (long long)n28, (long long)p38, (long long)n38, n);
        }
        if (g_dmgOrig) g_dmgOrig(a0, copy, a2, a3);
        return;
    }
    if (n == 1 || (n % 50) == 0) LOGI(ABS_LOG " hook_ok dmg n=%d mult=%d payload=%p", n, mult, payload);
    if (g_dmgOrig) g_dmgOrig(a0, a1, a2, a3);
}

static void InstallDmgHook() {
    static std::atomic<bool> ran{false};
    if (ran.exchange(true)) return;
    void* addr = ResolveMethodPtr(g_image, "Unit", "BLKEJBDMGAB", 0x354B388, "dmg");
    if (!addr) { LOGW(ABS_LOG " hook_skip name=BLKEJBDMGAB rc=-1 reason=ptr null"); return; }
    int rc = DobbyHook(addr, (dobby_dummy_func_t)&Stub_Dmg, (dobby_dummy_func_t*)&g_dmgOrig);
    if (rc != 0) LOGW(ABS_LOG " hook_skip name=BLKEJBDMGAB rc=%d addr=%p", rc, addr);
    else LOGI(ABS_LOG " hook installed dmg addr=%p orig=%p", addr, (void*)g_dmgOrig);
}
#endif  // !STARRY_PROBE && !STARRY_SPEED_ONLY



// ============================================================================
//  v1.9.6 预备：表驱动 hook（探针 / 强制返回 / 施放标记，全部由顶部 kHookCfg 决定）
//    Dobby 每个挂点需要独立入口 ⇒ 用模板按槽位实例化（最多 kMaxHook 个）。
// ============================================================================
#if !defined(STARRY_PROBE) && !defined(STARRY_SPEED_ONLY)
typedef int   (*fn_i4_t)(void*, void*, void*, void*);
typedef float (*fn_f4_t)(void*, void*, void*, void*);

static const int kMaxHook = 24;
struct HookSlot {
    const char* tag = "";
    int         kind = -1;
    int         forceRet = 0;
    void*       addr = nullptr;
    void*       orig = nullptr;
    std::atomic<int> n{0};
    uint64_t    lastLog = 0;
};
static HookSlot g_hook[kMaxHook];

static int ForceRun(int i, void* a0, void* a1, void* a2, void* a3) {
    HookSlot& s = g_hook[i];
    if (!g_skill.load()) {                        // 开关关：调原函数，完全透传
        return s.orig ? ((fn_i4_t)s.orig)(a0, a1, a2, a3) : 0;
    }
    int n = ++s.n;
    if (n == 1) LOGI(ABS_LOG " hook installed tag=%s ret=%d（首次）", s.tag, s.forceRet);
    return s.forceRet;
}

// kind8：void 函数 —— 什么都不做直接返回（不调原函数）；日志 hookvoid
static void VoidRun(int i) {
    HookSlot& s = g_hook[i];
    int n = ++s.n;
    if (n == 1) LOGI(ABS_LOG " hook installed tag=%s（void 直返，首次）", s.tag);
}

static int SetByteRun(int i, void* a0) {
    HookSlot& s = g_hook[i];
    int n = ++s.n;
    if (a0 && s.forceRet > 0) {
        uint8_t* p = reinterpret_cast<uint8_t*>((uintptr_t)a0 + (uintptr_t)s.forceRet);
        if (RangePerm((uintptr_t)p, 1, 2)) *p = 1;      // 写前校验可写
    }
    if (n == 1 || (n % 500) == 0) {
        if (s.tag && std::strcmp(s.tag, "energygate") == 0) {
            LOGI(ABS_LOG " hookset tag=energygate off=0x%X n=%d btn=%p", (unsigned)s.forceRet, n, a0);
        } else {
            LOGI(ABS_LOG " hookset tag=%s off=0x%X n=%d", s.tag, (unsigned)s.forceRet, n);
        }
    }
    return 1;                                           // 不调原函数
}

// kind5：passthrough + ActionButton 字段变化观测（每对象 500ms 节流，最多 64 个对象）
struct UiObs { void* btn; uint64_t lastLog; int last[7]; bool used; };
static UiObs g_ui[64];


// kind7：条件强制返回 —— 仅「无限技能」开关打开时返回 forceRet，关闭时调原函数透传
//   用于 IAGGDOCLNND 的两个技能槽判定入口（都返回 bool，入参 (Unit, 枚举)，不读参数）

// kind6：CD 门禁只观测（v1.9.11 起不再改内存 —— 已证明它不是技能 CD）


#define HOOK_STUB_R(k) static int  HookR##k(void* a, void* b, void* c, void* d) { return ForceRun(k, a, b, c, d); }
#define HOOK_STUB_S(k) static int  HookS##k(void* a, void* b, void* c, void* d) { (void)b; (void)c; (void)d; return SetByteRun(k, a); }
#define HOOK_STUB_V(k) static void HookV##k(void* a, void* b, void* c, void* d) { (void)a; (void)b; (void)c; (void)d; VoidRun(k); }

// v2.0：只保留正式版用到的三种桩（force/setbyte/void），每槽独立入口
HOOK_STUB_R(0) HOOK_STUB_S(0) HOOK_STUB_V(0)
HOOK_STUB_R(1) HOOK_STUB_S(1) HOOK_STUB_V(1)
HOOK_STUB_R(2) HOOK_STUB_S(2) HOOK_STUB_V(2)
HOOK_STUB_R(3) HOOK_STUB_S(3) HOOK_STUB_V(3)
HOOK_STUB_R(4) HOOK_STUB_S(4) HOOK_STUB_V(4)
HOOK_STUB_R(5) HOOK_STUB_S(5) HOOK_STUB_V(5)
HOOK_STUB_R(6) HOOK_STUB_S(6) HOOK_STUB_V(6)
HOOK_STUB_R(7) HOOK_STUB_S(7) HOOK_STUB_V(7)
HOOK_STUB_R(8) HOOK_STUB_S(8) HOOK_STUB_V(8)
HOOK_STUB_R(9) HOOK_STUB_S(9) HOOK_STUB_V(9)
HOOK_STUB_R(10) HOOK_STUB_S(10) HOOK_STUB_V(10)
HOOK_STUB_R(11) HOOK_STUB_S(11) HOOK_STUB_V(11)
HOOK_STUB_R(12) HOOK_STUB_S(12) HOOK_STUB_V(12)
HOOK_STUB_R(13) HOOK_STUB_S(13) HOOK_STUB_V(13)
HOOK_STUB_R(14) HOOK_STUB_S(14) HOOK_STUB_V(14)
HOOK_STUB_R(15) HOOK_STUB_S(15) HOOK_STUB_V(15)
HOOK_STUB_R(16) HOOK_STUB_S(16) HOOK_STUB_V(16)
HOOK_STUB_R(17) HOOK_STUB_S(17) HOOK_STUB_V(17)
HOOK_STUB_R(18) HOOK_STUB_S(18) HOOK_STUB_V(18)
HOOK_STUB_R(19) HOOK_STUB_S(19) HOOK_STUB_V(19)
HOOK_STUB_R(20) HOOK_STUB_S(20) HOOK_STUB_V(20)
HOOK_STUB_R(21) HOOK_STUB_S(21) HOOK_STUB_V(21)
HOOK_STUB_R(22) HOOK_STUB_S(22) HOOK_STUB_V(22)
HOOK_STUB_R(23) HOOK_STUB_S(23) HOOK_STUB_V(23)

static void* StubFor(int i, int kind) {
    (void)kind;
    switch (i) {
        case 0:  return kind == 8 ? (void*)&HookV0 : kind == 4 ? (void*)&HookS0 : (void*)&HookR0;
        case 1:  return kind == 8 ? (void*)&HookV1 : kind == 4 ? (void*)&HookS1 : (void*)&HookR1;
        case 2:  return kind == 8 ? (void*)&HookV2 : kind == 4 ? (void*)&HookS2 : (void*)&HookR2;
        case 3:  return kind == 8 ? (void*)&HookV3 : kind == 4 ? (void*)&HookS3 : (void*)&HookR3;
        case 4:  return kind == 8 ? (void*)&HookV4 : kind == 4 ? (void*)&HookS4 : (void*)&HookR4;
        case 5:  return kind == 8 ? (void*)&HookV5 : kind == 4 ? (void*)&HookS5 : (void*)&HookR5;
        case 6:  return kind == 8 ? (void*)&HookV6 : kind == 4 ? (void*)&HookS6 : (void*)&HookR6;
        case 7:  return kind == 8 ? (void*)&HookV7 : kind == 4 ? (void*)&HookS7 : (void*)&HookR7;
        case 8:  return kind == 8 ? (void*)&HookV8 : kind == 4 ? (void*)&HookS8 : (void*)&HookR8;
        case 9:  return kind == 8 ? (void*)&HookV9 : kind == 4 ? (void*)&HookS9 : (void*)&HookR9;
        case 10:  return kind == 8 ? (void*)&HookV10 : kind == 4 ? (void*)&HookS10 : (void*)&HookR10;
        case 11:  return kind == 8 ? (void*)&HookV11 : kind == 4 ? (void*)&HookS11 : (void*)&HookR11;
        case 12:  return kind == 8 ? (void*)&HookV12 : kind == 4 ? (void*)&HookS12 : (void*)&HookR12;
        case 13:  return kind == 8 ? (void*)&HookV13 : kind == 4 ? (void*)&HookS13 : (void*)&HookR13;
        case 14:  return kind == 8 ? (void*)&HookV14 : kind == 4 ? (void*)&HookS14 : (void*)&HookR14;
        case 15:  return kind == 8 ? (void*)&HookV15 : kind == 4 ? (void*)&HookS15 : (void*)&HookR15;
        case 16:  return kind == 8 ? (void*)&HookV16 : kind == 4 ? (void*)&HookS16 : (void*)&HookR16;
        case 17:  return kind == 8 ? (void*)&HookV17 : kind == 4 ? (void*)&HookS17 : (void*)&HookR17;
        case 18:  return kind == 8 ? (void*)&HookV18 : kind == 4 ? (void*)&HookS18 : (void*)&HookR18;
        case 19:  return kind == 8 ? (void*)&HookV19 : kind == 4 ? (void*)&HookS19 : (void*)&HookR19;
        case 20:  return kind == 8 ? (void*)&HookV20 : kind == 4 ? (void*)&HookS20 : (void*)&HookR20;
        case 21:  return kind == 8 ? (void*)&HookV21 : kind == 4 ? (void*)&HookS21 : (void*)&HookR21;
        case 22:  return kind == 8 ? (void*)&HookV22 : kind == 4 ? (void*)&HookS22 : (void*)&HookR22;
        case 23:  return kind == 8 ? (void*)&HookV23 : kind == 4 ? (void*)&HookS23 : (void*)&HookR23;
        default: return nullptr;
    }
}

static void InstallHookTable() {
    static std::atomic<bool> ran{false};
    if (ran.exchange(true)) return;
    if (!g_api.base || !g_api.end) { LOGW(ABS_LOG " hooktable skip reason=no-module-base"); return; }
    const int total = (int)(sizeof(kHookCfg) / sizeof(kHookCfg[0]));
    int slot = 0;
    int okCount = 0;
    int kindsMask = 0;
    for (int i = 0; i < total && slot < kMaxHook; ++i, ++slot) {
        const HookCfg& c = kHookCfg[i];
        volatile uintptr_t rv = c.rva;      // volatile：防常量折叠把 0 分支（含日志串）整段优化掉
        if (rv == 0) { LOGW(ABS_LOG " hooktable skip tag=%s reason=no-rva", c.tag); continue; }
        void* addr = (void*)(g_api.base + rv);
        if ((uintptr_t)addr >= g_api.end) {
            LOGW(ABS_LOG " hooktable fail tag=%s rc=-3 reason=rva out of range", c.tag);
            continue;
        }
        // 同一地址不能挂两次（Dobby 单点单 hook）：后面同址项直接跳过并说明
        bool dup = false;
        for (int k = 0; k < slot; ++k) {
            if (g_hook[k].addr == addr && g_hook[k].kind >= 0) { dup = true; break; }
        }
        if (dup) {
            LOGW(ABS_LOG " hooktable skip tag=%s reason=addr-already-hooked addr=%p", c.tag, addr);
            --slot;
            continue;
        }

        void* stub = StubFor(slot, c.kind);
        if (!stub) { LOGW(ABS_LOG " hooktable fail tag=%s rc=-4 reason=no-stub", c.tag); continue; }
        g_hook[slot].tag = c.tag;
        g_hook[slot].kind = c.kind;
        g_hook[slot].forceRet = c.forceRet;
        g_hook[slot].addr = addr;
        int rc = DobbyHook(addr, (dobby_dummy_func_t)stub, (dobby_dummy_func_t*)&g_hook[slot].orig);
        if (rc != 0) {
            LOGW(ABS_LOG " hooktable fail tag=%s rc=%d addr=%p", c.tag, rc, addr);
            g_hook[slot].kind = -1;
            continue;
        }
        ++okCount;
        kindsMask |= (1 << c.kind);
        (void)addr;
    }
    LOGI(ABS_LOG " hooktable ok n=%d kinds=0x%X（装上的数量与 kind 位掩码）", okCount, kindsMask);
}
#endif  // 非探针版


// ============================================================================
//  v1.9.8：无限蓝 + 无 CD（**每 tick 重新解析引用**，禁止缓存指针）
//
//  v1.9.7 实测教训：nocd-obs 里 driver 从活的 0x70a12a7ed6c0 换成 0x70a124178a00
//  （last=0、cd=0），我们仍在写旧对象 ⇒ 「引用换人」。故：
//    · 每 tick 用 CollectUnits 的结果重新挑玩家 Unit（活着的那个）
//    · driver 两个都写：unit+0x38 与 unit+0x40
//    · state 每次从当前 unit 重新取
//    · 指针一变就打 ref-change 并重新记录 before
// ============================================================================
#if !defined(STARRY_PROBE) && !defined(STARRY_SPEED_ONLY)
static uint64_t g_lastEnergyFill = 0;
static uint64_t g_lastNoCd       = 0;
static uint64_t g_lastEnergyObs2 = 0;
static uint64_t g_lastNoCdObs    = 0;
static int      g_energyFillN    = 0;
static int      g_noCdNa         = 0;
static int      g_noCdNb         = 0;

struct DrvRef { void* drv = nullptr; int before = 0; bool saved = false; };
static DrvRef g_drvA, g_drvB;
static void*  g_lastUnit = nullptr;
static void*  g_lastState = nullptr;

static void* PickPlayerUnit(const UnitSlot* us, int n, int* idxOut) {
    void* fallback = nullptr;
    int fbIdx = -1;
    for (int i = 0; i < n; ++i) {
        if (us[i].faction != kFactionPlayer) continue;
        void* u = us[i].unit;
        if (!OkPtr(u)) continue;
        const bool readable = RangePerm((uintptr_t)u + kDumpOffHpCur, 8, 1) &&
                              RangePerm((uintptr_t)u + kDumpOffHpMax, 8, 1);
        long long maxv = readable ? (long long)ReadI64(u, kDumpOffHpMax) : 0;
        if (readable && maxv >= 1) { if (idxOut) *idxOut = i; return u; }   // 活着的优先
        if (!fallback) { fallback = u; fbIdx = i; }
    }
    if (idxOut) *idxOut = fbIdx;
    return fallback;
}

static void* PickUnitLogged(const UnitSlot* us, int n, const char* who) {
    int cnt = 0;
    for (int i = 0; i < n; ++i) if (us[i].faction == kFactionPlayer) ++cnt;
    int idx = -1;
    void* u = PickPlayerUnit(us, n, &idx);
    static void* lastLogged = nullptr;
    if (u && u != lastLogged) {
        lastLogged = u;
        LOGI(ABS_LOG " unit-pick uid=%p n=%d who=%s", u, cnt, who);
    }
    return u;
}

static void RefChangeLog(const char* kind, void* oldp, void* newp) {
    if (oldp == newp) return;
    LOGI(ABS_LOG " ref-change kind=%s old=%p new=%p", kind, oldp, newp);
}

// 一参 int 的托管方法调用
static void* InvokeInt1(void* obj, const char* mname, int arg) {
    if (!OkPtr(obj) || !g_api.object_class || !g_api.class_method || !g_api.runtime_invoke) return nullptr;
    void* k = nullptr;
    Guarded("inv_class", [&] { k = g_api.object_class(obj); });
    if (!k) return nullptr;
    void* m = nullptr;
    Guarded("inv_m", [&] { m = g_api.class_method(k, mname, 1); });
    if (!m) return nullptr;
    void* ret = nullptr;
    int32_t a = arg;
    void* args[1] = {&a};
    Guarded("inv_call", [&] { ret = g_api.runtime_invoke(m, obj, args, nullptr); });
    return ret;
}
static bool InvokeInt0(void* obj, const char* mname, int* out) {
    if (!OkPtr(obj) || !g_api.object_class || !g_api.class_method || !g_api.runtime_invoke) return false;
    void* k = nullptr;
    Guarded("inv0_class", [&] { k = g_api.object_class(obj); });
    if (!k) return false;
    void* m = nullptr;
    Guarded("inv0_m", [&] { m = g_api.class_method(k, mname, 0); });
    if (!m) return false;
    void* ret = nullptr;
    Guarded("inv0_call", [&] { ret = g_api.runtime_invoke(m, obj, nullptr, nullptr); });
    if (!ret) return false;
    if (RangePerm((uintptr_t)ret + 0x10, 4, 1)) { *out = ReadI32(ret, 0x10); return true; }
    int64_t v = (int64_t)(intptr_t)ret;
    if (v > -1000000000LL && v < 1000000000LL) { *out = (int)v; return true; }
    return false;
}

// 无限蓝：每 tick 从当前 unit 取 state
static void EnergyFillPass(const UnitSlot* us, int n, bool enabled, uint64_t now) {
    if (!enabled) return;
    if (g_energyFillN > 1000000) return;                       // 熔断
    void* u = PickUnitLogged(us, n, "energy");
    if (!u) return;
    if (u != g_lastUnit) { RefChangeLog("unit", g_lastUnit, u); g_lastUnit = u; }
    if (now - g_lastEnergyFill < 500) return;
    g_lastEnergyFill = now;
    if (!g_anchors.state.ok) { LOGI(ABS_LOG " energy-fill skip reason=no-state-field"); return; }
    void* state = nullptr;
    Guarded("ef_state", [&] { state = ReadPtrAt(u, g_anchors.state.off); });
    if (!OkPtr(state)) { LOGI(ABS_LOG " energy-fill skip reason=state null uid=%p", u); return; }
    if (state != g_lastState) { RefChangeLog("state", g_lastState, state); g_lastState = state; }
    void* k = nullptr;
    Guarded("ef_class", [&] { k = g_api.object_class ? g_api.object_class(state) : nullptr; });
    void* m = nullptr;
    if (k && g_api.class_method) Guarded("ef_m", [&] { m = g_api.class_method(k, "FJDABAEBLJG", 1); });
    if (!m) { LOGI(ABS_LOG " energy-fill skip reason=no-method state=%p", state); return; }
    void* ret = nullptr;
    Guarded("ef_invoke", [&] {
        int32_t d = 10000;
        void* args[1] = {&d};
        ret = g_api.runtime_invoke(m, state, args, nullptr);
    });
    int cnt = g_energyFillN++;
    if (cnt == 0) {
        LOGI(ABS_LOG " energy-fill uid=%p state=%p ret=%p（首次，之后静默）", u, state, ret);
    }
}

// v1.9.10：驱动写入已删除（写的是「单位主驱动」，不是技能用的那批对象）
//   真正的清零在 kind6 门禁 hook 里做；这里只保留低频观测（10s）。
static void NoCdPass(const UnitSlot* us, int n, bool enabled, uint64_t now) {
    if (now - g_lastNoCd < 10000) return;
    g_lastNoCd = now;
    void* u = PickUnitLogged(us, n, "nocd");
    if (!u) return;
    void* da = nullptr;
    void* db = nullptr;
    Guarded("nc_a", [&] { da = ReadPtrAt(u, 0x38); });
    Guarded("nc_b", [&] { db = ReadPtrAt(u, 0x40); });
    int cda = (OkPtr(da) && RangePerm((uintptr_t)da + 0xBC, 4, 1)) ? ReadI32(da, 0xBC) : -1;
    int cdb = (OkPtr(db) && RangePerm((uintptr_t)db + 0xBC, 4, 1)) ? ReadI32(db, 0xBC) : -1;
    (void)enabled;
    LOGI(ABS_LOG " nocd a=%p cd=%d b=%p cd=%d（只观测，v1.9.10 起不写）", da, cda, db, cdb);
}

// 观测
static void EnergyObs2Pass(const UnitSlot* us, int n, uint64_t now) {
    if (now - g_lastEnergyObs2 < 3000) return;
    g_lastEnergyObs2 = now;
    void* u = PickUnitLogged(us, n, "obs2");
    if (!u) return;
    void* state = nullptr;
    Guarded("eo_state", [&] { state = ReadPtrAt(u, g_anchors.state.off); });
    if (!OkPtr(state)) return;
    const char* why = "-";
    void* container = ResolveOknd(state, &why);
    int cur = 0, maxv = 0;
    bool okCur = InvokeInt0(state, "CAODLBENOAM", &cur);
    bool okMax = OkPtr(container) ? InvokeInt0(container, "PMKAGMHPAHB", &maxv) : false;
    LOGI(ABS_LOG " energy-obs2 uid=%p cur=%d max=%d filled=%d（ok=%d/%d）",
         u, cur, maxv, (maxv > 0 && cur >= maxv) ? 1 : 0, (int)okCur, (int)okMax);
}

static void NoCdObsPass(const UnitSlot* us, int n, uint64_t now) {
    if (now - g_lastNoCdObs < 10000) return;
    g_lastNoCdObs = now;
    void* u = PickUnitLogged(us, n, "obs");
    if (!u) return;
    void* da = nullptr;
    void* db = nullptr;
    Guarded("nco_a", [&] { da = ReadPtrAt(u, 0x38); });
    Guarded("nco_b", [&] { db = ReadPtrAt(u, 0x40); });
    int cda = (OkPtr(da) && RangePerm((uintptr_t)da + 0xBC, 4, 1)) ? ReadI32(da, 0xBC) : -1;
    int lsta = (OkPtr(da) && RangePerm((uintptr_t)da + 0x98, 4, 1)) ? ReadI32(da, 0x98) : -1;
    int cdb = (OkPtr(db) && RangePerm((uintptr_t)db + 0xBC, 4, 1)) ? ReadI32(db, 0xBC) : -1;
    int lstb = (OkPtr(db) && RangePerm((uintptr_t)db + 0x98, 4, 1)) ? ReadI32(db, 0x98) : -1;
    LOGI(ABS_LOG " nocd-obs a=%p cd=%d last=%d b=%p cd=%d last=%d", da, cda, lsta, db, cdb, lstb);
}
#endif  // 非探针版


// ============================================================================
//  v1.9.9：驱动字段采样器（只读，变化才打）
//    unit+0x38 上每 100ms 采 +0x70..+0x74（bool 区）、+0x94(float)、+0x98、+0x9C(float)、
//    +0xA8(float)、+0xAC(float)、+0xB8、+0xBC；变化才打印（每对象 200ms 节流）。
//    目的：按一次技能后看哪个字段开始单调变化/归零 ⇒ 那是真正的计时器。
// ============================================================================
#if !defined(STARRY_PROBE) && !defined(STARRY_SPEED_ONLY)
static uint64_t g_lastDrvSample = 0;

struct DrvObs {
    void* drv = nullptr;
    uint64_t lastLog = 0;
    int prev[12] = {0};
    bool used = false;
};
static DrvObs g_drvObs[16];

static void DrvLogPass(const UnitSlot* us, int n, uint64_t now) {
    if (now - g_lastDrvSample < 1000) return;
    g_lastDrvSample = now;
    void* u = PickUnitLogged(us, n, "drv");
    if (!u) return;
    void* drv = nullptr;
    Guarded("dl_drv", [&] { drv = ReadPtrAt(u, 0x38); });
    if (!OkPtr(drv) || !RangePerm((uintptr_t)drv + 0x70, 0x50, 1)) return;

    int cur[12];
    cur[0]  = *reinterpret_cast<uint8_t*>((uintptr_t)drv + 0x70);
    cur[1]  = *reinterpret_cast<uint8_t*>((uintptr_t)drv + 0x71);
    cur[2]  = *reinterpret_cast<uint8_t*>((uintptr_t)drv + 0x72);
    cur[3]  = *reinterpret_cast<uint8_t*>((uintptr_t)drv + 0x73);
    cur[4]  = *reinterpret_cast<uint8_t*>((uintptr_t)drv + 0x74);
    cur[5]  = (int)ReadF32(drv, 0x94);
    cur[6]  = ReadI32(drv, 0x98);
    cur[7]  = (int)ReadF32(drv, 0x9C);
    cur[8]  = (int)ReadF32(drv, 0xA8);
    cur[9]  = (int)ReadF32(drv, 0xAC);
    cur[10] = ReadI32(drv, 0xB8);
    cur[11] = ReadI32(drv, 0xBC);

    DrvObs* slot = nullptr;
    DrvObs* freeSlot = nullptr;
    for (int k = 0; k < 16; ++k) {
        if (g_drvObs[k].used && g_drvObs[k].drv == drv) { slot = &g_drvObs[k]; break; }
        if (!g_drvObs[k].used && !freeSlot) freeSlot = &g_drvObs[k];
    }
    if (!slot) {
        slot = freeSlot ? freeSlot : &g_drvObs[0];
        slot->used = true;
        slot->drv = drv;
        for (int k = 0; k < 12; ++k) slot->prev[k] = cur[k];
        slot->lastLog = 0;
    }
    bool changed = false;
    for (int k = 0; k < 12; ++k) if (slot->prev[k] != cur[k]) { changed = true; break; }
    if (changed && now - slot->lastLog > 200) {
        slot->lastLog = now;
        LOGI(ABS_LOG " drvlog drv=%p b70=%d b71=%d b72=%d b73=%d b74=%d f94=%.3f i98=%d f9c=%.3f fA8=%.3f fAC=%.3f iB8=%d iBC=%d changed=1",
             drv, cur[0], cur[1], cur[2], cur[3], cur[4],
             (double)ReadF32(drv, 0x94), cur[6], (double)ReadF32(drv, 0x9C),
             (double)ReadF32(drv, 0xA8), (double)ReadF32(drv, 0xAC), cur[10], cur[11]);
        for (int k = 0; k < 12; ++k) slot->prev[k] = cur[k];
    }
}
#endif  // 非探针版

bool Abssnmxt_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(sec_lib_il2cpp())) return false;
    InstallFaultGuard();
    if (!ResolveAll()) return false;
    g_libReady.store(true);
#ifdef STARRY_HOOK_PROBE
    HookProbeRun();
#endif
#if !defined(STARRY_PROBE) && !defined(STARRY_SPEED_ONLY)
    InstallDmgHook();          // v1.9：伤害倍率走载荷 hook（三分量）
    InstallHookTable();        // v1.9.6：表驱动（探针/强制返回/施放标记，全部由 kHookCfg 决定）
#endif
    LOGI(ABS_LOG " lib ready (data-plane, no hook, probe=%d)", (int)kProbeMode);
#ifdef STARRY_PROBE
    LOGI(ABS_LOG " PROBE_BUILD=1 只解析不写内存");
#endif
    return true;
}

bool Abssnmxt_IsReady() { return g_libReady.load(); }

void Abssnmxt_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case ABSSNMXT_FEAT_GOD:   g_god.store(enabled);   break;
        case ABSSNMXT_FEAT_SKILL: g_skill.store(enabled); break;
        default: break;
    }
    LOGI(ABS_LOG " feature %d = %d", featNum, (int)enabled);
}

void Abssnmxt_OnValueChanged(int featNum, int value) {
    switch (featNum) {
        case ABSSNMXT_FEAT_DMGX:  g_dmgX.store(ClampInt(value, kDmgXMin, kDmgXMax));
            LOGI(ABS_LOG " damage_x=%d", g_dmgX.load()); break;
        case ABSSNMXT_FEAT_SPEED: g_speedX.store(ClampInt(value, kSpeedXMin, kSpeedXMax));
            LOGI(ABS_LOG " speed_x=%d", g_speedX.load()); break;
        default: break;
    }
}

void Abssnmxt_Tick(const bool* features, const int* values) {
    if (!features) return;

    g_god.store(features[ABSSNMXT_FEAT_GOD]);
    g_skill.store(features[ABSSNMXT_FEAT_SKILL]);
    if (values) {
        g_dmgX.store(ClampInt(values[ABSSNMXT_FEAT_DMGX], kDmgXMin, kDmgXMax));
        g_speedX.store(ClampInt(values[ABSSNMXT_FEAT_SPEED], kSpeedXMin, kSpeedXMax));
    }

    if (!g_libReady.load()) {
        if (!isLibraryLoaded(sec_lib_il2cpp())) return;
        InstallFaultGuard();
        if (!ResolveAll()) return;
        g_libReady.store(true);
        LOGI(ABS_LOG " lib ready (via Tick fallback)");
    }
    if (g_api.aborted || g_anchors.aborted) return;

    const int n = CollectUnits(g_units, (int)(sizeof(g_units) / sizeof(g_units[0])));
    uint64_t now = NowMs();
    if (now - g_lastCountLog > 5000) {
        int pl = 0, en = 0;
        for (int i = 0; i < n; ++i) {
            if (g_units[i].faction == kFactionPlayer) ++pl;
            else if (g_units[i].faction == kFactionEnemy) ++en;
        }
        g_lastCountLog = now;
        LOGI(ABS_LOG " unit_count=%d player=%d enemy=%d", n, pl, en);
    }
    if (n <= 0) return;

    // v1.5：Stage A 采样/状态机（只读，先证后写）
#if !defined(STARRY_PROBE) && !defined(STARRY_SPEED_ONLY) && !defined(STARRY_HOOK_PROBE)
    bool confirmed = Hp198Tick(g_units, n, now);
    Dmgx198Pass(g_units, n, g_dmgX.load(), now);    // 只读观测（v1.7 不写）
    if (now - g_lastFactionLog > 5000) {
        g_lastFactionLog = now;
        int shown = 0;
        for (int i = 0; i < n && shown < 4; ++i) {
            int fv = 0;
            Guarded("faction_read", [&] { fv = ReadI32(g_units[i].unit, g_anchors.faction.off); });
            if (shown == 0 || (fv == kFactionEnemy && shown <= 3)) {
                LOGI(ABS_LOG " faction player_unit=%p val=%d", g_units[i].unit, fv);
            }
            ++shown;
        }
    }
    AttrApplyPass(g_units, n, g_god.load(), g_dmgX.load(), g_skill.load(), now);   // v1.8：属性 int[] 写入
    if (confirmed && now - g_lastMirrorLog > 5000) {
        g_lastMirrorLog = now;
        for (int i = 0; i < n; ++i) {
            if (g_units[i].faction != kFactionPlayer) continue;
            void* u = g_units[i].unit;
            void* st = g_anchors.state.ok ? ReadPtrAt(u, g_anchors.state.off) : nullptr;
            int cur = ReadHp198(u);
            if (cur < 1 || cur > kHp198MaxSane) break;
            const char* why = "-";
            void* oknd = ResolveOknd(st, &why);
            MirrorScanOnce(u, st, oknd, cur, g_hp198.maxCand, now);
            if (g_god.load()) MirrorWriteProbe(u, st, oknd, cur, now);
            break;
        }
    }
#endif
    SkillPass(g_units, n, g_skill.load());   // v1.5：空实现（技能关闭）
#if !defined(STARRY_PROBE) && !defined(STARRY_SPEED_ONLY)
    EnergyFillPass(g_units, n, g_skill.load(), now);   // v2.0 保留：无限蓝（500ms，首次一条日志）
#endif
    SpeedPass(g_units, n);                   // 移速观测行
#if !defined(STARRY_PROBE) && !defined(STARRY_HOOK_PROBE)
    SpeedWritePass(g_units, n);            // v1.3：唯一允许的写（oknd+0x48）
#endif
#ifdef STARRY_SPEED_ONLY
    HpScanPass(g_units, n);                // v1.3：血量字段只读定位
#endif
}

void Abssnmxt_ForceClear() {
    g_god.store(false);
    g_dmgX.store(1);
    g_skill.store(false);
    g_speedX.store(1);
#if !defined(STARRY_PROBE) && !defined(STARRY_HOOK_PROBE)
    SpeedRestoreVerify(true);
#endif
    std::memset(g_hpCache, 0, sizeof(g_hpCache));
#if !defined(STARRY_PROBE) && !defined(STARRY_SPEED_ONLY) && !defined(STARRY_HOOK_PROBE)
    // 血量类不还原：只停止写，当前血量保持不变（写回旧值等于扣血）
    g_hp198.wrote = false;
    std::memset(g_enemyHp, 0, sizeof(g_enemyHp));
    LOGI(ABS_LOG " force clear：功能全关；血量保持当前值，不退回旧值");
#else
    LOGI(ABS_LOG " force clear：功能全关、缓存清空");
#endif
}