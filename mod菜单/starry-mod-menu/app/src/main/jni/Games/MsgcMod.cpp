// ============================================================================
//  星光菜单游戏模块 — 标准骨架 v3.1（注册表版，复制后按 TODO 填空）
//
//  生成：bash mod菜单/starry-mod-menu/scripts/new_game_mod.sh {Game}
//        → 四件套 {Game}Mod.h/.cpp + {Game}Entry.cpp + Android_{tag}.mk
//        入口走 MainModern.cpp 注册表（不改 Main.cpp / GameConfig.h / 公共 Android.mk）
//
//  ★ 内置防闪退设施是「代码」不是「文档约定」，删任何一节都可能复现历史闪退：
//      默认 0 hook（懒装） / 双路径挂点 / RVA 漂移拒绝 / 幂等安装 /
//      orig 必调 / reenter 防递归 / SafeWrite 校验写 / 类名缓存 / Tick 轻量
//    v3.1（2026-10-02，疾风骑士 60.0.1 实战下沉）另加三件，见 §④：
//      并发装钩互斥 / 致命信号兜底（sigsetjmp 保护区 + sigaltstack） / 槽位熔断
//
//  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
//
//  功能实现分层（定稿 2026-09-09，详见 GAME_MOD_GUIDE.md）：
//    数据型（无敌=HP回写 / 属性增益 / 资源锁定）
//        → 偏移定位 + SafeWrite 直写，Tick 轮询维持（零 hook 痕迹，防封首选；
//          开关全关 = 零改写）
//    行为型（改判定 / 伤害结算 / 拦截上报）
//        → Dobby hook：RVA 优先（序言校验），RVA 失效自动转按名（本骨架 HookSlot 双路径）
//    字节 patch 改函数 → 默认禁用（E15–E18 死路清单）
// ============================================================================

#include "MsgcMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>

#include <mutex>      // §④ 装钩互斥
#include <setjmp.h>   // §④ sigsetjmp 保护区
#include <signal.h>   // §④ 致命信号兜底
#include <sys/stat.h>
#include <sys/uio.h>
#include <dlfcn.h>
#include <link.h>
#include <sys/syscall.h>
#include <unistd.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

// 生成器把 [msgc] 替换为本游戏标签（如 [jfqs]），日志过滤用 adb logcat -s Mod_Menu
#define TPL_LOG "[msgc]"

// 注册表模式不依赖 GameConfig.h（legacy 配置中心）。
// Cocos 系生成后把下面这处（含各 hook 内引用）统一换成目标库名。
#define TPL_TARGET_LIB sec_lib_il2cpp()

// ============================================================================
//  ① 开关与 orig 指针
//     g_libReady = 「库已映射」≠「已挂钩」；全开关关闭时应当保持 0 个 hook。
// ============================================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_infRes{false};       // 1 建造资源无限
static std::atomic<bool> g_noBuildTime{false};  // 2 无建造时间
static std::atomic<bool> g_buildGod{false};     // 3 建筑无限生命
static std::atomic<bool> g_troopGod{false};     // 4 部队无限生命
static std::atomic<bool> g_reenter{false};

// orig 指针在第二阶段按实际挂点逐个声明（orig 必调铁律见 hook-rules）

// ============================================================================
//  ② 字段偏移与 RVA
//     TODO：全部来自 dump.cs / IDA / Trace 实测，禁止猜；换包必须重 dump。
// ============================================================================
// TODO(第二阶段)：偏移/RVA/序言全部来自 dump.cs / IDA 实测，禁止猜；换包必须重 dump。
// enum : size_t { kOffXxx = 0x0 };

// ============================================================================
//  ③ 安全助手（防闪退核心；新代码禁止绕开它们裸读写内存）
// ============================================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

static void SafeWriteI32(void* obj, size_t off, int32_t v) {
    if (!OkPtr(obj)) return;
    Il2Cpp::WriteI32(obj, off, v);
}

static void SafeWriteF32(void* obj, size_t off, float v) {
    if (!OkPtr(obj)) return;
    Il2Cpp::WriteF32(obj, off, v);
}

static int32_t SafeReadI32(void* obj, size_t off, int32_t defv) {
    if (!OkPtr(obj)) return defv;
    return Il2Cpp::ReadI32(obj, off);
}

// 类名缓存：热路径禁止每帧 object_class + class_name 解析（类指针不变就不重取）
struct ClassNameCache {
    void* klass = nullptr;
    char  name[40] = {};
    bool  valid = false;
};

static const char* CacheClassName(ClassNameCache& cache, void* obj) {
    if (!OkPtr(obj)) return "";
    void* k = GetApi().object_class(obj);
    if (!k) return "";
    if (cache.valid && cache.klass == k) return cache.name;
    const char* n = GetApi().class_name(k);
    cache.klass = k;
    cache.valid = true;
    std::snprintf(cache.name, sizeof(cache.name), "%s", n ? n : "");
    return cache.name;
}

static bool NameHas(const char* got, const char* needle) {
    if (!got || !needle || !*needle) return false;
    return std::strstr(got, needle) != nullptr;
}

// ============================================================================
//  ④ 并发 + 致命信号兜底 + 槽位熔断（v3.1 新增，来自疾风骑士 60.0.1 实战）
//
//  为什么需要（三段真实事故，删任一段都会复现）：
//    (1) 并发装钩竞态：UI 线程与游戏线程同时进入 SyncHooks()，
//        HookSlotInstall 的「幂等」判断是 check-then-act 非原子
//        ⇒ 同一地址被 DobbyHook 两次 ⇒ DobbyCodePatch 越界 memcpy
//        ⇒ SIGSEGV(SEGV_ACCERR)，栈：
//           __memcpy_aarch64_simd ← DobbyCodePatch+96 ← DobbyHook+216
//        修法：g_installMutex 串行化 + double-check。
//    (2) DobbyHook 返回码接不住 Dobby 内部的进程级 SIGSEGV：
//        必须用 sigsetjmp 保护区把「装钩」和「读序言」包起来，
//        炸了就跳过该槽位、游戏继续跑。
//    (3) hook 体内会执行游戏/第三方代码（例如免广告直接 Invoke 游戏发奖回调，
//        回调里可能读到尚未填充的字段 ⇒ null-deref）：
//        用 RunGuarded 包住回调，炸了吞掉 + 记日志，不整包崩。
//
//  sigaltstack 是 per-thread 的，备用栈必须 thread_local；
//  故障态同理必须 thread_local（hook 体会在 UI 线程 / 游戏线程 / SDK 回调线程并发执行）。
// ============================================================================

// ---- 4.1 互斥：装钩串行 ----
static std::mutex g_installMutex;   // 保护「装钩全过程」，只在 SyncHooks 里取
static std::mutex g_guardMutex;     // 只保护一次性「安装信号处理器」动作

// ---- 4.2 本线程故障态（thread_local：hook 体在多线程并发） ----
static thread_local sigjmp_buf              t_faultJmp;
static thread_local volatile sig_atomic_t   t_inGuard    = 0;   // 是否正在保护区内
static thread_local volatile sig_atomic_t   t_faultDepth = 0;   // 保护区嵌套/连炸计数
static thread_local volatile int            t_faultSig   = 0;   // 最近一次致命信号号
static thread_local volatile void*          t_faultAddr  = nullptr; // 最近一次故障地址

// 备用信号栈（SIGSEGV 时原栈可能已不可用）；64KB/线程，per-thread 必须 thread_local
static thread_local char t_altStack[65536];
static thread_local bool t_altStackSet = false;

// ---- 4.3 被链式接管的旧处理器 ----
static struct sigaction g_oldSegv;
static struct sigaction g_oldBus;
static struct sigaction g_oldIll;
static std::atomic<bool> g_guardArmed{false};
// 装钩期是否发生过致命信号：只在 g_installMutex 保护下读写
static bool g_lastFatal = false;

// ---- 4.4 故障处理器 ----
static void TplFaultHandler(int sig, siginfo_t* si, void* /*uc*/) {
    if (t_inGuard) {
        // 保护区内连炸：说明保护区本身不可靠，交回系统默认行为（宁可崩，不要死循环）
        if (++t_faultDepth > 4) {
            signal(sig, SIG_DFL);
            raise(sig);
            return;
        }
        t_faultSig  = sig;
        t_faultAddr = si ? si->si_addr : nullptr;
        siglongjmp(t_faultJmp, 1);   // 永不返回
    }

    // 保护区外：链回旧处理器，再走默认
    const struct sigaction* old = (sig == SIGSEGV) ? &g_oldSegv
                               : (sig == SIGBUS)  ? &g_oldBus
                                                  : &g_oldIll;
    if (old->sa_sigaction && old->sa_sigaction != TplFaultHandler) {
        old->sa_sigaction(sig, si, nullptr);
        return;
    }
    signal(sig, SIG_DFL);
    raise(sig);
}

// ---- 4.5 为本线程挂上备用信号栈（懒执行，失败降级为无备用栈） ----
static void ArmFaultGuardForThread() {
    if (t_altStackSet) return;
    t_altStackSet = true;
    stack_t ss;
    std::memset(&ss, 0, sizeof(ss));
    ss.ss_sp    = t_altStack;
    ss.ss_size  = sizeof(t_altStack);
    ss.ss_flags = 0;
    if (sigaltstack(&ss, nullptr) != 0) {
        LOGW(TPL_LOG " sigaltstack failed — 兜底降级为无备用栈");
    }
}

// ---- 4.6 一次性安装进程级处理器 ----
static void InstallFaultGuard() {
    if (g_guardArmed.load()) return;
    std::lock_guard<std::mutex> lk(g_guardMutex);
    if (g_guardArmed.load()) return;

    struct sigaction sa;
    std::memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = TplFaultHandler;
    sa.sa_flags     = SA_SIGINFO | SA_ONSTACK | SA_NODEFER;
    sigemptyset(&sa.sa_mask);

    sigaction(SIGSEGV, &sa, &g_oldSegv);
    sigaction(SIGBUS,  &sa, &g_oldBus);
    sigaction(SIGILL,  &sa, &g_oldIll);

    g_guardArmed.store(true);
    LOGI(TPL_LOG " fault guard armed (SIGSEGV/SIGBUS/SIGILL, altstack=%zu)",
         sizeof(t_altStack));
}

// ---- 4.7 保护区包装器 ----
// 装钩：Dobby 内部崩（返回码接不住的那种）⇒ 返回 false + 置 g_lastFatal
static bool DobbyHookGuarded(const char* tag, void* target, void* hook, void** orig) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        int rc = DobbyHook(target, hook, orig);
        t_inGuard = 0;
        if (rc != 0) {
            LOGW(TPL_LOG " %s dobby fail rc=%d", tag, rc);
            return false;
        }
        return true;
    }
    // longjmp 回来：保护区内的致命信号已被吞掉
    t_inGuard = 0;
    g_lastFatal = true;
    LOGW(TPL_LOG " %s dobby SIG(%d) @%p — 槽位熔断，游戏继续",
         tag, (int)t_faultSig, (void*)t_faultAddr);
    if (orig) *orig = nullptr;   // ★ 必须清掉可能被半写的 orig，否则后续调用跳到野指针
    return false;
}

// 读序言（RVA 漂移校验）：读坏页也会炸，同样走保护区
static bool ReadU32Guarded(const char* tag, const void* p, uint32_t* out) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        *out = *reinterpret_cast<const volatile uint32_t*>(p);
        t_inGuard = 0;
        return true;
    }
    t_inGuard = 0;
    LOGW(TPL_LOG " %s prologue read SIG(%d) @%p — 跳过该槽位",
         tag, (int)t_faultSig, (void*)t_faultAddr);
    return false;
}

// 跑第三方代码（游戏回调 / SDK 回调）：炸了吞掉，只记日志
static bool RunGuarded(const char* tag, void (*fn)(void*), void* arg) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        fn(arg);
        t_inGuard = 0;
        return true;
    }
    t_inGuard = 0;
    LOGW(TPL_LOG " %s 回调内 SIG(%d) @%p — 已吞掉，游戏继续",
         tag, (int)t_faultSig, (void*)t_faultAddr);
    return false;
}

// ---- 4.8 槽位熔断 ----
// 装钩期炸过的槽位，本次会话不再重试（否则 Tick 每轮再炸一次 ⇒ 每轮一次闪退）
static const int kSlotsMax = 64;
static bool g_slotDead[kSlotsMax];                    // 只在 g_installMutex 下读写
static std::atomic<int> g_installedCount{0};

// ============================================================================
//  ⑤ 双路径挂点机器（提炼自 WhrhxMod 实测；禁 base+RVA、禁全 RX 盲扫）
// ============================================================================
static bool IsLibMapLine(const char* line, const char* lib) {
    return std::strstr(line, lib) != nullptr && std::strstr(line, ".so") != nullptr;
}

// file_off → 运行时 VA：libil2cpp 是分段映射，base+RVA 会挂到错误地址，
// 必须扫 /proc/self/maps 只取可执行段，并在覆盖该 file_off 的段里取最大 span。
static void* FileOffToVa(const char* lib, uintptr_t fileOff) {
    FILE* fp = std::fopen("/proc/self/maps", "r");
    if (!fp) return nullptr;

    char line[768];
    void* best = nullptr;
    uintptr_t bestSpan = 0;

    while (std::fgets(line, sizeof(line), fp)) {
        if (!IsLibMapLine(line, lib)) continue;

        uintptr_t start = 0, end = 0, off = 0;
        char perm[8] = {};
        if (std::sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!std::strchr(perm, 'x')) continue;   // 只要可执行段
        if (end <= start) continue;

        uintptr_t span = end - start;
        if (fileOff >= off && fileOff < off + span) {
            if (span > bestSpan) {
                bestSpan = span;
                best = reinterpret_cast<void*>(start + (fileOff - off));
            }
        }
    }
    std::fclose(fp);
    return best;
}

static bool HookAtRva(const char* tag, uintptr_t rva, uint32_t expectInsn,
                      void* hook, void** orig) {
    void* va = FileOffToVa(TPL_TARGET_LIB, rva);
    if (!va) {
        LOGW(TPL_LOG " %s rva 0x%lX not in RX — try by-name", tag, (unsigned long)rva);
        return false;
    }
    if (expectInsn) {
        uint32_t got = 0;
        if (!ReadU32Guarded(tag, va, &got)) return false;   // 读序言也走保护区
        if (got == 0 || got == 0xFFFFFFFFu || got != expectInsn) {
            LOGW(TPL_LOG " %s rva drift got=0x%08X want=0x%08X — try by-name",
                 tag, got, expectInsn);
            return false;
        }
    }
    if (!DobbyHookGuarded(tag, va, hook, orig)) {
        LOGW(TPL_LOG " %s dobby fail @rva 0x%lX", tag, (unsigned long)rva);
        return false;
    }
    LOGI(TPL_LOG " %s hooked @rva 0x%lX", tag, (unsigned long)rva);
    return true;
}

static bool EnsureApi() {
    if (GetApi().ready) return true;
    return GetApi().Load(TPL_TARGET_LIB);
}

static void* FindClassNs(const char* ns, const char* name) {
    size_t n = 0;
    void** asms = GetApi().Assemblies(&n);
    for (size_t i = 0; asms && i < n; ++i) {
        void* img = GetApi().ImageOf(asms[i]);
        if (!img) continue;
        void* k = GetApi().ClassFromName(img, ns, name);
        if (k) return k;
    }
    return nullptr;
}

// 类/方法缺失只记日志降级，绝不让整包失败（hook-rules §17）
static bool HookByName(const char* tag, const char* ns, const char* cls,
                       const char* method, int argc, void* hook, void** orig) {
    if (!EnsureApi()) {
        LOGW(TPL_LOG " %s api not ready — will retry via Tick", tag);
        return false;
    }

    void* k = FindClassNs(ns, cls);
    if (!k) k = GetApi().FindClassByShortName(cls);
    if (!k) {
        LOGW(TPL_LOG " %s class miss %s — degrade", tag, cls);
        return false;
    }

    void* mi = GetApi().GetMethod(k, method, argc);
    if (!mi) {
        LOGW(TPL_LOG " %s method miss %s.%s(argc=%d) — degrade, install others",
             tag, cls, method, argc);
        return false;
    }

    void* fn = *reinterpret_cast<void**>(mi);
    if (!OkPtr(fn)) {
        LOGW(TPL_LOG " %s bad method ptr %s.%s", tag, cls, method);
        return false;
    }

    if (!DobbyHookGuarded(tag, fn, hook, orig)) return false;
    LOGI(TPL_LOG " %s hooked by-name %s.%s(argc=%d)", tag, cls, method, argc);
    return true;
}

struct HookSlot {
    const char* tag;
    uintptr_t   rva;          // 0 = 只走按名
    uint32_t    expectInsn;   // 0 = 不校验序言；实测后回填
    const char* ns;
    const char* cls;
    const char* method;
    int         argc;
    void*       hookFn;
    void**      origPtr;
};

// 幂等安装（防同址双 Dobby 叠钩 SIGSEGV）→ RVA 优先 → 失败转按名
static bool HookSlotInstall(const HookSlot& s) {
    if (s.origPtr && *s.origPtr) return true;
    if (s.rva && HookAtRva(s.tag, s.rva, s.expectInsn, s.hookFn, s.origPtr)) return true;
    return HookByName(s.tag, s.ns, s.cls, s.method, s.argc, s.hookFn, s.origPtr);
}

// ============================================================================
//  ⑥ hook 函数与挂点表
//     铁律：一个具体方法一个槽；绝不 hook 共享 invoke 入口。
// ============================================================================
// ============================================================================
//  ⑥′ 托管路径探针（2026-11-07）：DOTS + [BurstCompile] 下托管方法可能只是
//  throw 桩。本探针只读调用 + 限量日志，不改数值；命中即证明该链托管可达。
// ============================================================================
static std::atomic<int> g_probeN{0};
static std::atomic<bool> g_dumpDone{false};
// 无限生命取值：足够大且不溢出（血量按 int 处理）
static const int kGodHp = 999999;

// il2cpp 基址：用已 hook 到的函数真实地址减去 dump.cs 里的 RVA 反推，
// 比扫 /proc/self/maps 可靠（雷电 houdini 下 maps 是 host 视角，guest 地址对不上）。
static uintptr_t g_il2cppBase = 0;
static const uintptr_t kRvaGetMaxHp = 0x418FE3C;   // GameBuffAspectRW.GetMaxHp(int,int,int)

static uintptr_t LibBasePhdr(const char* lib) {
    struct Ctx { const char* want; uintptr_t base; };
    Ctx ctx{lib, 0};
    dl_iterate_phdr([](struct dl_phdr_info* info, size_t, void* data) -> int {
        Ctx* c = (Ctx*)data;
        if (info->dlpi_name && std::strstr(info->dlpi_name, c->want)) { c->base = (uintptr_t)info->dlpi_addr; return 1; }
        return 0;
    }, &ctx);
    return ctx.base;
}

static uintptr_t LibBase(const char* lib) {
    uintptr_t b = LibBasePhdr(lib);
    if (b) return b;
    FILE* fp = std::fopen("/proc/self/maps", "r");
    if (!fp) return 0;
    char line[768];
    uintptr_t best = ~(uintptr_t)0;
    while (std::fgets(line, sizeof(line), fp)) {
        if (!std::strstr(line, lib)) continue;
        unsigned long st = 0, en = 0, off = 0; char perm[8] = {};
        if (std::sscanf(line, "%lx-%lx %7s %lx", &st, &en, perm, &off) != 4) continue;
        uintptr_t bias = (uintptr_t)st - (uintptr_t)off;
        if (bias < best) best = bias;
    }
    std::fclose(fp);
    return best == ~(uintptr_t)0 ? 0 : best;
}

static bool (*g_origSoldierIsDead)(void*) = nullptr;
static bool Hook_SoldierIsDead(void* self) {
    bool r = g_origSoldierIsDead ? g_origSoldierIsDead(self) : false;
    if (r && (g_troopGod.load() || g_buildGod.load())) {
        if (g_probeN.fetch_add(1) < 5) LOGI(TPL_LOG " god: is_dead suppressed (troop=%d build=%d)",
                                            (int)g_troopGod.load(), (int)g_buildGod.load());
        return false;   // 开关打开时不判死
    }
    return r;
}
static int (*g_origGetMaxHp)(void*, int, int, int) = nullptr;
static int Hook_GetMaxHp(void* self, int a, int b, int c) {
    int r = g_origGetMaxHp ? g_origGetMaxHp(self, a, b, c) : 0;
    if (g_probeN.fetch_add(1) < 12) {
        uintptr_t ra = (uintptr_t)__builtin_return_address(0);
        if (!g_il2cppBase) g_il2cppBase = LibBase("libil2cpp.so");
        LOGI(TPL_LOG " god: maxhp %d -> %d (base=%d tag=%d all=%d) callerRVA=0x%lX",
             r, (g_troopGod.load() || g_buildGod.load()) ? kGodHp : r, a, b, c,
             (unsigned long)(g_il2cppBase ? ra - g_il2cppBase : 0));
    }
    if (g_troopGod.load() || g_buildGod.load()) return kGodHp;
    return r;
}
static bool (*g_origBldIsValid)(void*) = nullptr;
static bool Hook_BldIsValid(void* self) {
    if (g_probeN.fetch_add(1) < 3) LOGI(TPL_LOG " probe building.is_valid HIT");
    return g_origBldIsValid ? g_origBldIsValid(self) : false;
}

// 类名全表（一次性）：跳过 System/Microsoft/Unity 等系统命名空间，
// 只打游戏自己的类（含资源/建造关键词优先），用于定位业务类型。
static void DumpInterestingClasses() {
    if (!EnsureApi()) { LOGW(TPL_LOG " cls: api not ready"); return; }
    static const char* kws[] = {"Res","Build","Cost","Ink","Food","Pop","Money","Coin",
                                "Wallet","Bag","Store","Price","Shop","Player","Game",
                                "Level","Tower","Unit","Data","State","Config","Manager"};
    size_t n = 0;
    void** asms = GetApi().Assemblies(&n);
    int hits = 0, total = 0;
    for (size_t i = 0; asms && i < n; ++i) {
        void* img = GetApi().ImageOf(asms[i]);
        if (!img) continue;
        uint32_t cnt = GetApi().image_class_count ? GetApi().image_class_count(img) : 0;
        for (uint32_t k = 0; k < cnt && hits < 260; ++k) {
            void* kl = GetApi().image_class ? GetApi().image_class(img, k) : nullptr;
            if (!kl) continue;
            const char* nm = GetApi().class_name ? GetApi().class_name(kl) : nullptr;
            if (!nm || !*nm || nm[0] == '<') continue;
            const char* ns = GetApi().class_namespace ? GetApi().class_namespace(kl) : "";
            if (ns && *ns && (std::strstr(ns, "System") || std::strstr(ns, "Microsoft") ||
                              std::strstr(ns, "Unity") || std::strstr(ns, "Newtonsoft") ||
                              std::strstr(ns, "Cysharp") || std::strstr(ns, "Mono"))) continue;
            ++total;
            bool hit = false;
            for (size_t w = 0; w < sizeof(kws) / sizeof(kws[0]); ++w)
                if (std::strstr(nm, kws[w])) { hit = true; break; }
            if (hit) { LOGI(TPL_LOG " GCLS %s", nm); ++hits; }
        }
    }
    LOGI(TPL_LOG " GCLS done total=%d hits=%d", total, hits);
}

// ============================================================================
//  解释器探针（2026-11-07）：HybridCLR Interpreter::Execute(imi, args, ret)
//  入口 RVA 0x3D70934（.eh_frame_hdr 反查 + 序言核对：sub sp,#0xc40 / x22=x0/x21=x1/x20=x2）
//  只采前若干次调用的 imi 字段，用于确定 InterpMethodInfo 布局，不改任何数据。
// ============================================================================
static void (*g_origInterp)(void*, void*, void*) = nullptr;
static std::atomic<int> g_interpN{0};

static uint64_t g_seen[2048];
static std::atomic<int> g_seenN{0};

static bool SeenOnce(uint64_t h) {
    if (!h) h = 1;
    size_t i = (size_t)(h * 1099511628211ull) & 2047u;
    for (int k = 0; k < 8; ++k) {
        size_t pos = (i + k) & 2047u;
        uint64_t cur = g_seen[pos];
        if (cur == h) return true;
        if (cur == 0) { g_seen[pos] = h; return false; }
    }
    return true;
}

// 方法名采样：imi+0x20 = MethodInfo*，MethodInfo+0x10 = char* name。
// 读内存走 process_vm_readv（失败只返回 -1，不会 SIGSEGV），且只在低频采样点调用。
static bool SafeRead(uintptr_t addr, void* buf, size_t len) {
    if (!addr || !len) return false;
    struct iovec local { buf, len };
    struct iovec remote { (void*)addr, len };
    long n = syscall(__NR_process_vm_readv, getpid(), &local, 1UL, &remote, 1UL, 0UL);
    return n == (long)len;
}

static void ReportInterpMethod(void* imi, void* args) {
    uint64_t mi = 0, np = 0, kp = 0;
    if (!SafeRead((uintptr_t)imi + 0x20, &mi, 8) || mi < 0x1000) return;
    if (!SafeRead((uintptr_t)mi + 0x10, &np, 8) || np < 0x1000) return;
    char nm[48] = {};
    if (!SafeRead((uintptr_t)np, nm, sizeof(nm) - 1) || !nm[0]) return;
    char kn[48] = {};
    if (SafeRead((uintptr_t)mi + 0x18, &kp, 8) && kp > 0x1000) {
        uint64_t knp = 0;
        if (SafeRead((uintptr_t)kp + 0x10, &knp, 8) && knp > 0x1000) SafeRead((uintptr_t)knp, kn, sizeof(kn) - 1);
    }
    bool hit = false;
    const char* kws[] = {"Resource","Building","Soldier","Enemy","Cost","Consume","Worker",
                         "Production","Hp","Health","Attack","Damage","Ink","Food","Pop"};
    for (size_t i = 0; i < sizeof(kws)/sizeof(kws[0]) && !hit; ++i) {
        if (std::strstr(kn, kws[i]) || std::strstr(nm, kws[i])) hit = true;
    }
    if (!hit) return;
    uint64_t h = 1469598103934665603ull;
    for (int i = 0; kn[i] && i < 45; ++i) { h ^= (unsigned char)kn[i]; h *= 1099511628211ull; }
    for (int i = 0; nm[i] && i < 45; ++i) { h ^= (unsigned char)nm[i]; h *= 1099511628211ull; }
    if (SeenOnce(h)) return;
    if (g_seenN.fetch_add(1) >= 600) return;
    uint64_t a[4] = {};
    if (args) SafeRead((uintptr_t)args, a, sizeof(a));
    LOGI(TPL_LOG " HIT %s::%s a0=%llx a1=%llx a2=%llx a3=%llx",
         kn, nm, (unsigned long long)a[0], (unsigned long long)a[1],
         (unsigned long long)a[2], (unsigned long long)a[3]);
}

static void Hook_Interp(void* imi, void* args, void* ret) {
    int c = g_interpN.fetch_add(1);
    if (c < 4000000 && (c % 60) == 0 && imi) ReportInterpMethod(imi, args);
    if (c > 0 && (c % 200000) == 0) LOGI(TPL_LOG " INTERP calls=%d seen=%d", c, g_seenN.load());
    if (g_origInterp) g_origInterp(imi, args, ret);
}

static void InstallInterpHook() {
    uintptr_t base = LibBase("libil2cpp.so");
    if (!base) { LOGW(TPL_LOG " INTERP no base"); return; }
    void* target = (void*)(base + 0x3D70928);   // 真正的函数入口 stp x29,x30,[sp,#-0x60]!
    if (DobbyHookGuarded("interp.exec", target, (void*)Hook_Interp, (void**)&g_origInterp))
        LOGI(TPL_LOG " INTERP hook ok @%p", target);
    else
        LOGW(TPL_LOG " INTERP hook failed");
}

// ============================================================================
//  ECS 组件指纹扫描（2026-11-07）
//  依据 dump.cs 字段偏移：SoldierComponentData{Health@0x0, LastHealth@0x4,
//  LastMaxHealth@0x18, DeadTime@0x38}。满血时 Health==LastHealth==LastMaxHealth，
//  即 int 序列满足 a[0]==a[1] && a[0]==a[6]，值域 1..5000，DeadTime(a[14])==0。
//  只读扫描：命中即打印地址与上下文，不改任何字节。
// ============================================================================
static std::atomic<uintptr_t> g_unitAddr{0};

static int ScanSoldierData(int round) {
    FILE* fp = std::fopen("/proc/self/maps", "r");
    if (!fp) return 0;
    char line[768];
    int hits = 0;
    while (std::fgets(line, sizeof(line), fp)) {
        unsigned long st = 0, en = 0; char perm[8] = {}; char path[512] = {};
        int n = std::sscanf(line, "%lx-%lx %7s %*s %*s %*s %511[^\n]", &st, &en, perm, path);
        if (n < 3 || perm[0] != 'r' || perm[1] != 'w') continue;
        // ECS chunk 走 Allocator（libc malloc 的匿名页）
        if (n >= 4 && path[0] && !std::strstr(path, "[anon")) continue;
        size_t len = (size_t)(en - st);
        if (len < 4096 || len > (size_t)256 * 1024 * 1024) continue;
        ArmFaultGuardForThread();
        const int* a = (const int*)st;
        size_t cnt = len / 4;
        for (size_t i = 6; i + 16 < cnt; ++i) {
            const int h = a[i], lh = a[i + 1], lmh = a[i + 6], dt = a[i + 14];
            if (h != lh || h != lmh) continue;
            if (h < 10 || h > 2000) continue;
            if (dt != 0) continue;
            // 相邻索引字段（ConfigIdx/EntityId 之类）应是小整数
            if (a[i+2] < -1 || a[i+2] > 4096 || a[i+3] < -1 || a[i+3] > 4096) continue;
            if (a[i+4] < -1 || a[i+4] > 4096 || a[i+5] < -1 || a[i+5] > 4096) continue;
            LOGI(TPL_LOG " UNIT r%d @%p h=%d ctx=%d,%d,%d,%d,%d | a7=%d a8=%d a9=%d",
                 round, (void*)(a + i), h, a[i+2], a[i+3], a[i+4], a[i+5], a[i+6], a[i+7], a[i+8], a[i+9]);
            if (g_unitAddr.load() == 0) g_unitAddr.store((uintptr_t)(a + i));
            if (++hits >= 20) { std::fclose(fp); LOGI(TPL_LOG " UNIT r%d capped", round); return hits; }
        }
    }
    std::fclose(fp);
    LOGI(TPL_LOG " UNIT r%d hits=%d", round, hits);
    return hits;
}

// ============================================================================
//  加密密钥探针（2026-11-07）：热更 dll 密文长度为 16 的整数倍 ⇒ 块加密。
//  .NET 侧一定经 SymmetricAlgorithm.CreateDecryptor(byte[] key, byte[] iv)
//  或 set_Key(byte[])/set_IV(byte[]) 传入密钥。命中即把 key/IV 打出来。
// ============================================================================
static std::atomic<int> g_cryptoN{0};

static void DumpByteArray(const char* tag, void* arr) {
    if (!arr) return;
    size_t len = 0; void* data = nullptr;
    std::memcpy(&len, (uint8_t*)arr + 0x18, 8);
    std::memcpy(&data, (uint8_t*)arr + 0x20, 8);
    if (!data || len == 0 || len > 512) {
        LOGI(TPL_LOG " CRYPTO %s len=%zu (no data)", tag, len);
        return;
    }
    char hex[1024]; size_t pos = 0;
    size_t n = len < 64 ? len : 64;
    for (size_t i = 0; i < n && pos + 3 < sizeof(hex); ++i)
        pos += std::snprintf(hex + pos, sizeof(hex) - pos, "%02x", ((const uint8_t*)data)[i]);
    LOGI(TPL_LOG " CRYPTO %s len=%zu hex=%s", tag, len, hex);
}

static void* (*g_origCreateDec)(void*, void*, void*) = nullptr;
static void* Hook_CreateDec(void* self, void* key, void* iv) {
    if (g_cryptoN.fetch_add(1) < 40) { DumpByteArray("key", key); DumpByteArray("iv", iv); }
    return g_origCreateDec ? g_origCreateDec(self, key, iv) : nullptr;
}

static void (*g_origSetKey)(void*, void*) = nullptr;
static void Hook_SetKey(void* self, void* key) {
    if (g_cryptoN.fetch_add(1) < 60) DumpByteArray("setKey", key);
    if (g_origSetKey) g_origSetKey(self, key);
}

static void InstallCryptoHook() {
    if (!EnsureApi()) return;
    static const char* kClasses[] = {"RijndaelManaged", "AesManaged", "AesCryptoServiceProvider",
                                     "SymmetricAlgorithm", "Rijndael", "Aes"};
    int ok = 0;
    for (size_t i = 0; i < sizeof(kClasses)/sizeof(kClasses[0]); ++i) {
        void* k = GetApi().FindClassByShortName(kClasses[i]);
        if (!k) continue;
        if (!g_origCreateDec) {
            void* m = GetApi().GetMethod(k, "CreateDecryptor", 2);
            if (m && DobbyHookGuarded("crypto.dec", *reinterpret_cast<void**>(m),
                                      (void*)Hook_CreateDec, (void**)&g_origCreateDec)) ++ok;
        }
        if (!g_origSetKey) {
            void* m = GetApi().GetMethod(k, "set_Key", 1);
            if (m && DobbyHookGuarded("crypto.key", *reinterpret_cast<void**>(m),
                                      (void*)Hook_SetKey, (void**)&g_origSetKey)) ++ok;
        }
    }
    LOGI(TPL_LOG " CRYPTO hooks=%d", ok);
}

// ============================================================================
//  热更 dll 明文抓取（2026-11-07 v2）
//  ① HybridCLR.RuntimeApi::LoadMetadataForAOTAssembly(byte[],mode) —— icall 名在
//     libil2cpp 里是明文字符串，用 il2cpp_resolve_icall 直接拿函数地址后 hook。
//  ② Assembly.Load 的重载消歧：按参数类型找 byte[] 版本（argc=1 会误取 string 版）。
// ============================================================================
static std::atomic<int> g_dllN{0};

static void SaveManagedBytes(const char* tag, void* arr) {
    if (!arr) return;
    size_t len = 0; void* data = nullptr;
    std::memcpy(&len, (uint8_t*)arr + 0x18, 8);
    std::memcpy(&data, (uint8_t*)arr + 0x20, 8);
    if (!data || len < 1024 || len > (size_t)64 * 1024 * 1024) return;
    const uint8_t* p = (const uint8_t*)data;
    bool pe = (p[0] == 'M' && p[1] == 'Z');
    bool bsjb = false;
    for (size_t i = 0; i + 4 < len && i < (size_t)4 * 1024 * 1024; ++i)
        if (p[i] == 'B' && p[i+1] == 'S' && p[i+2] == 'J' && p[i+3] == 'B') { bsjb = true; break; }
    LOGI(TPL_LOG " DLL %s len=%zu pe=%d bsjb=%d head=%02x%02x%02x%02x",
         tag, len, (int)pe, (int)bsjb, p[0], p[1], p[2], p[3]);
    if (!pe && !bsjb) return;
    char path[160];
    std::snprintf(path, sizeof(path), "/data/data/com.chillyroom.inkvasion.yw/files/msgc_dll/%s_%zu.dll", tag, len);
    FILE* out = std::fopen(path, "wb");
    if (!out) return;
    std::fwrite(p, 1, len, out);
    std::fclose(out);
    LOGI(TPL_LOG " DLL saved %s", path);
}

static void* (*g_origLoadMeta)(void*, int) = nullptr;
static void* Hook_LoadMeta(void* bytes, int mode) {
    if (g_dllN.fetch_add(1) < 30) SaveManagedBytes("aotmeta", bytes);
    return g_origLoadMeta ? g_origLoadMeta(bytes, mode) : nullptr;
}

static void* (*g_origAsmLoad2)(void*, void*) = nullptr;
static void* Hook_AsmLoad2(void* a, void* b) {
    if (g_dllN.fetch_add(1) < 40) { SaveManagedBytes("asm2a", a); SaveManagedBytes("asm2b", b); }
    return g_origAsmLoad2 ? g_origAsmLoad2(a, b) : nullptr;
}

static void InstallDllHooks() {
    // ① icall 解析
    void* h = dlopen("libil2cpp.so", RTLD_NOW | RTLD_NOLOAD);
    if (h) {
        typedef void* (*resolve_t)(const char*);
        resolve_t resolve = (resolve_t)dlsym(h, "il2cpp_resolve_icall");
        if (resolve) {
            void* fn = resolve("HybridCLR.RuntimeApi::LoadMetadataForAOTAssembly(System.Byte[],HybridCLR.HomologousImageMode)");
            if (fn && DobbyHookGuarded("clr.loadmeta", fn, (void*)Hook_LoadMeta, (void**)&g_origLoadMeta))
                LOGI(TPL_LOG " DLL loadmeta hook ok @%p", fn);
            else
                LOGW(TPL_LOG " DLL loadmeta resolve=%p hook=0", fn);
        } else {
            LOGW(TPL_LOG " DLL no il2cpp_resolve_icall");
        }
    }
    // ② Assembly.Load(byte[], byte[])
    if (EnsureApi()) {
        void* k = GetApi().FindClassByShortName("Assembly");
        if (k) {
            void* m = GetApi().GetMethod(k, "Load", 2);
            if (m && DobbyHookGuarded("asm.load2", *reinterpret_cast<void**>(m),
                                      (void*)Hook_AsmLoad2, (void**)&g_origAsmLoad2))
                LOGI(TPL_LOG " DLL asm.load(2) hook ok");
        }
    }
}

// 只读探针：解析 HybridCLR icall 的真实 RVA（不 hook，零风险）
static void ProbeHybridClrIcall() {
    void* h = dlopen("libil2cpp.so", RTLD_NOW | RTLD_NOLOAD);
    if (!h) { LOGW(TPL_LOG " CLR dlopen fail"); return; }
    typedef void* (*resolve_t)(const char*);
    resolve_t resolve = (resolve_t)dlsym(h, "il2cpp_resolve_icall");
    if (!resolve) { LOGW(TPL_LOG " CLR no resolve_icall"); return; }
    uintptr_t base = LibBase("libil2cpp.so");
    static const char* names[] = {
        "HybridCLR.RuntimeApi::LoadMetadataForAOTAssembly(System.Byte[],HybridCLR.HomologousImageMode)",
        "HybridCLR.RuntimeApi::PreJitMethod(System.Reflection.MethodInfo)",
        "HybridCLR.RuntimeApi::PreJitClass(System.Type)",
    };
    for (size_t i = 0; i < sizeof(names)/sizeof(names[0]); ++i) {
        void* fn = resolve(names[i]);
        if (fn && base)
            LOGI(TPL_LOG " CLR icall[%d] RVA=0x%lX", (int)i, (unsigned long)((uintptr_t)fn - base));
        else
            LOGW(TPL_LOG " CLR icall[%d] fn=%p", (int)i, fn);
    }
}

// 只读探针：打印 System.Reflection.Assembly 各 Load 重载的 RVA，
// 用于定位 HybridCLR 真正的 dll 加载/解密入口（不 hook）。
static void ProbeAssemblyLoad() {
    void* h = dlopen("libil2cpp.so", RTLD_NOW | RTLD_NOLOAD);
    if (!h) return;
    typedef const char* (*mname_t)(void*);
    mname_t mname = (mname_t)dlsym(h, "il2cpp_method_get_name");
    uintptr_t base = LibBase("libil2cpp.so");
    if (!EnsureApi() || !mname || !base) { LOGW(TPL_LOG " ASMLOAD probe prereq fail"); return; }
    void* k = GetApi().FindClassByShortName("Assembly");
    if (!k) { LOGW(TPL_LOG " ASMLOAD class not found"); return; }
    void* iter = nullptr;
    int shown = 0, total = 0;
    while (void* m = GetApi().class_methods(k, &iter)) {
        ++total;
        const char* nm = mname(m);
        if (!nm || std::strcmp(nm, "Load") != 0) continue;
        int argc = GetApi().method_param_count ? GetApi().method_param_count(m) : -1;
        void* ptr = *reinterpret_cast<void**>(m);
        LOGI(TPL_LOG " ASMLOAD argc=%d RVA=0x%lX", argc, (unsigned long)((uintptr_t)ptr - base));
        if (++shown >= 12) break;
    }
    LOGI(TPL_LOG " ASMLOAD done shown=%d total=%d", shown, total);
}

// Assembly.Load(byte[]) 真实实现（RVA 0x71DFF1C，由 il2cpp 的 Load 重载
// 0x70B8BD4 尾调用进入）：低频，只在加载热更 dll 时触发，dump 入参字节。
static void* (*g_origAsmLoadBytes)(void*, void*, void*) = nullptr;
static std::atomic<int> g_asmBytesN{0};

// 0x71DFA4C = Assembly.Load(byte[]) 实现（0x71DFF1C 读 [x1,#0x10] 是 string.length，
// 属 Load(string)）。三参数全透传，x1 才是 byte[] 数组。
static void* Hook_AsmLoadBytes(void* a, void* arr, void* c) {
    if (arr && g_asmBytesN.fetch_add(1) < 40) SaveManagedBytes("asmload", arr);
    return g_origAsmLoadBytes ? g_origAsmLoadBytes(a, arr, c) : nullptr;
}

static void InstallAsmLoadBytesHook() {
    static std::atomic<bool> done{false};
    if (done.exchange(true)) return;
    uintptr_t base = LibBase("libil2cpp.so");
    if (!base) { LOGW(TPL_LOG " ASMB no base"); return; }
    void* target = (void*)(base + 0x71DFA4C);
    if (DobbyHookGuarded("asm.loadbytes", target, (void*)Hook_AsmLoadBytes, (void**)&g_origAsmLoadBytes))
        LOGI(TPL_LOG " ASMB hook ok @%p", target);
    else
        LOGW(TPL_LOG " ASMB hook fail");
}

// 按 RVA 直接挂 LoadMetadataForAOTAssembly（0x3DEB044，签名 void(byte[], int)）
static void InstallLoadMetaByRva() {
    uintptr_t base = LibBase("libil2cpp.so");
    if (!base) { LOGW(TPL_LOG " CLRM no base"); return; }
    // HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly 的托管实现（实测 RVA）
    void* target = (void*)(base + 0x6EC6A80);
    if (DobbyHookGuarded("clr.loadmeta", target, (void*)Hook_LoadMeta, (void**)&g_origLoadMeta))
        LOGI(TPL_LOG " CLRM hook ok @%p", target);
    else
        LOGW(TPL_LOG " CLRM hook fail");
}

// ============================================================================
//  早期钩子安装（2026-11-07）：热更 dll 在 il2cpp_init 之后立刻加载，等 Tick 轮
//  询已经太晚。这里用独立线程等 libil2cpp 映射完成后立刻 hook il2cpp_init，
//  在它返回（引擎初始化完成）的瞬间再装 Assembly.Load 钩子。
// ============================================================================
static void InstallAsmLoadBytesHook();   // 前置声明
static void InstallLoadMetaByRva();      // 前置声明

typedef void* (*il2cpp_init_t)(const char*);
static il2cpp_init_t g_origIl2cppInit = nullptr;

static void* Hook_il2cppInit(const char* domain) {
    // HybridCLR 在 il2cpp_init 内部就预加载热更 dll ⇒ 必须先装钩子再进初始化
    InstallAsmLoadBytesHook();
    InstallLoadMetaByRva();
    LOGI(TPL_LOG " EARLY dll hooks installed BEFORE il2cpp_init");
    return g_origIl2cppInit ? g_origIl2cppInit(domain) : nullptr;
}

static void* MsgcEarlyInitThread(void*) {
    for (int i = 0; i < 3600; ++i) {          // 最多等 72s（20ms 一次）
        void* h = dlopen("libil2cpp.so", RTLD_NOW | RTLD_NOLOAD);
        if (h) {
            void* fn = dlsym(h, "il2cpp_init");
            if (fn) {
                if (DobbyHookGuarded("il2cpp.init", fn, (void*)Hook_il2cppInit, (void**)&g_origIl2cppInit))
                    LOGI(TPL_LOG " EARLY il2cpp_init hooked @%p", fn);
                else
                    LOGW(TPL_LOG " EARLY il2cpp_init hook fail");
                return nullptr;
            }
        }
        usleep(20000);
    }
    LOGW(TPL_LOG " EARLY timeout waiting libil2cpp");
    return nullptr;
}

__attribute__((constructor)) static void MsgcEarlyBootstrap() {
    // 2026-11-07 停用：早期钩子（il2cpp_init / dll 加载口）已验证会导致 SIGSEGV
    return;
}

// 只读：打印 HybridCLR.RuntimeApi 各方法的 methodPointer RVA
static void ProbeRuntimeApiRva() {
    if (!EnsureApi()) return;
    uintptr_t base = LibBase("libil2cpp.so");
    void* k = GetApi().FindClassByShortName("RuntimeApi");
    if (!k) { LOGW(TPL_LOG " RTAPI class not found"); return; }
    void* h = dlopen("libil2cpp.so", RTLD_NOW | RTLD_NOLOAD);
    typedef const char* (*mname_t)(void*);
    mname_t mname = h ? (mname_t)dlsym(h, "il2cpp_method_get_name") : nullptr;
    void* iter = nullptr;
    int n = 0;
    while (void* m = GetApi().class_methods(k, &iter)) {
        const char* nm = mname ? mname(m) : nullptr;
        if (!nm) continue;
        if (!std::strstr(nm, "Load") && !std::strstr(nm, "PreJit")) continue;
        int argc = GetApi().method_param_count ? GetApi().method_param_count(m) : -1;
        void* ptr = *reinterpret_cast<void**>(m);
        LOGI(TPL_LOG " RTAPI %s argc=%d RVA=0x%lX", nm, argc,
             (unsigned long)(base ? ((uintptr_t)ptr - base) : 0));
        if (++n >= 10) break;
    }
    LOGI(TPL_LOG " RTAPI done n=%d", n);
}

static void InstallProbes() {
    if (!EnsureApi()) { LOGW(TPL_LOG " probe: api not ready"); return; }
    int ok = 0;
    void* kSoldier = GetApi().FindClassByShortName("SoldierComponentData");
    if (kSoldier) {
        void* m = GetApi().GetMethod(kSoldier, "get_IsDead", 0);
        if (m && DobbyHookGuarded("probe.isDead", *reinterpret_cast<void**>(m),
                                  (void*)Hook_SoldierIsDead, (void**)&g_origSoldierIsDead)) ++ok;
    }
    void* kAspect = GetApi().FindClassByShortName("GameBuffAspectRW");
    if (kAspect) {
        void* m = GetApi().GetMethod(kAspect, "GetMaxHp", 3);
        if (m && DobbyHookGuarded("probe.maxhp", *reinterpret_cast<void**>(m),
                                  (void*)Hook_GetMaxHp, (void**)&g_origGetMaxHp)) ++ok;
    }
    void* kBld = GetApi().FindClassByShortName("BuildingSource");
    if (kBld) {
        void* m = GetApi().GetMethod(kBld, "IsValid", 0);
        if (m && DobbyHookGuarded("probe.bldValid", *reinterpret_cast<void**>(m),
                                  (void*)Hook_BldIsValid, (void**)&g_origBldIsValid)) ++ok;
    }
    LOGI(TPL_LOG " god hooks installed ok=%d/3 (开关决定是否生效)", ok);
}

// TODO(第二阶段)：四项功能的挂点在此逐个登记（一个具体方法一个槽，禁 hook 共享入口）：
//   1 建造资源无限 / 2 无建造时间 / 3 建筑无限生命 / 4 部队无限生命
//   当前 0 槽 = 环境阶段「菜单可见 + 能进游戏」，全开关关时不装任何 hook。
static const HookSlot* const kSlots = nullptr;

// ============================================================================
//  ⑦ 分组安装 + 懒装（全开关关 = 0 hook）
// ============================================================================
static const int kSlotsCount = 0;

static int InstallBattleHooks() {
    int ok = 0, dead = 0;
    for (int i = 0; i < kSlotsCount && i < kSlotsMax; ++i) {
        if (g_slotDead[i]) { ++dead; continue; }        // 熔断，跳过
        g_lastFatal = false;
        if (HookSlotInstall(kSlots[i])) {
            ++ok;
        } else if (g_lastFatal) {
            g_slotDead[i] = true;                       // 永久拉黑（本次会话）
            LOGW(TPL_LOG " slot[%d] %s 已熔断（装钩期致命信号），本次会话不再重试",
                 i, kSlots[i].tag);
        }
    }
    LOGI(TPL_LOG " install pass: ok=%d dead=%d/%d", ok, dead, kSlotsCount);
    if (ok > 0) g_anyHooked.store(true);
    return ok;
}

static bool AnyFeatureOn() {
    return g_infRes.load() || g_noBuildTime.load() || g_buildGod.load() || g_troopGod.load();
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    if (!AnyFeatureOn()) return;                        // 全关：不装、也不卸载
    if (g_installedCount.load() >= kSlotsCount) return; // 快路径：已装齐

    std::lock_guard<std::mutex> lk(g_installMutex);     // ★ 串行化，防并发双挂
    if (g_installedCount.load() >= kSlotsCount) return; // double-check
    g_installedCount.store(InstallBattleHooks());
}

// ============================================================================
//  ⑧ 对外接口（ModEntry.h 注册表契约，签名不可改）
// ============================================================================
bool Msgc_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;

    InstallFaultGuard();          // §④ 先装兜底，再装钩
    g_libReady.store(true);
    SyncHooks();                  // 已 ON 的开关立即装；全关保持 0 hook
    InstallProbes();              // 托管路径探针（只读、限量日志）
    ProbeHybridClrIcall();        // 只读：解析 HybridCLR icall 的 RVA
    ProbeAssemblyLoad();          // 只读：Assembly.Load 各重载 RVA
    // InstallInterpHook();       // 2026-11-07 采样完成，探针关闭（保留代码备查）
    DumpInterestingClasses();     // 类名转储（定位资源/建造/UI 类）
    LOGI(TPL_LOG " lib ready (dual-path, lazy hooks)");
    return true;
}

bool Msgc_IsReady() {
    return g_libReady.load();
}

void Msgc_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case MSGC_FEAT_INF_RES:       g_infRes.store(enabled); break;
        case MSGC_FEAT_NO_BUILD_TIME: g_noBuildTime.store(enabled); break;
        case MSGC_FEAT_BUILD_GOD:     g_buildGod.store(enabled); break;
        case MSGC_FEAT_TROOP_GOD:     g_troopGod.store(enabled); break;
        default: break;
    }
    SyncHooks();
}

void Msgc_OnValueChanged(int /*featNum*/, int /*value*/) {
    // 滑块类功能在此钳制并存入原子量（语义：1 = 关闭）
}

void Msgc_Tick(const bool* features, const int* /*values*/) {
    if (!features) return;

    g_infRes.store(features[MSGC_FEAT_INF_RES]);
    g_noBuildTime.store(features[MSGC_FEAT_NO_BUILD_TIME]);
    g_buildGod.store(features[MSGC_FEAT_BUILD_GOD]);
    g_troopGod.store(features[MSGC_FEAT_TROOP_GOD]);

    // 兜底：库晚于首帧映射时，这里补一次
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) {
        InstallFaultGuard();
        g_libReady.store(true);
        LOGI(TPL_LOG " lib ready (via Tick fallback)");
    }
    SyncHooks();

    // 延迟一次性内存 dump：HybridCLR 的 dll 要进局后才加载，启动期扫不到
    static int s_tickN = 0;
    ++s_tickN;
    // 组件指纹扫描（只读）：T+60s / T+120s 各一次，用于定位 ECS 士兵数据
    // 组件指纹扫描已停用（2026-11-07：ECS chunk 不在可扫描匿名堆内，命中均为填充值）

    // ★ 禁止在此 SafeProbe / 扫堆 / 遍历对象 / 逐对象写（Tick 频率高，代价爆炸）
}

void Msgc_ForceClear() {
    g_infRes.store(false);
    g_noBuildTime.store(false);
    g_buildGod.store(false);
    g_troopGod.store(false);
    g_reenter.store(false);
    // TODO：新增开关一并复位
}
