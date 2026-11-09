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

#include "旷野征途Mod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>

#include <mutex>      // §④ 装钩互斥
#include <setjmp.h>   // §④ sigsetjmp 保护区
#include <signal.h>   // §④ 致命信号兜底

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

// 生成器把 [KYZT] 替换为本游戏标签（如 [jfqs]），日志过滤用 adb logcat -s Mod_Menu
#define TPL_LOG "[KYZT]"

// 注册表模式不依赖 GameConfig.h（legacy 配置中心）。
// Cocos 系生成后把下面这处（含各 hook 内引用）统一换成目标库名。
#define TPL_TARGET_LIB sec_lib_il2cpp()

// ============================================================================
//  ① 开关与 orig 指针
//     g_libReady = 「库已映射」≠「已挂钩」；全开关关闭时应当保持 0 个 hook。
// ============================================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_dmg{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_spd{false};
static std::atomic<bool> g_reenter{false};

typedef void (*SetTimeScale_t)(float value, void* method);
static SetTimeScale_t g_origSetTimeScale = nullptr;
static std::atomic<void*> g_tsMethod{nullptr};   // 缓存 MethodInfo*，Tick 强制调用时要用
static std::atomic<int>   g_tsCalls{0};          // 诊断：set_timeScale 被游戏调用了几次

// ============================================================================
//  ② 字段偏移与 RVA
//     TODO：全部来自 dump.cs / IDA / Trace 实测，禁止猜；换包必须重 dump。
// ============================================================================
enum : size_t {
    kOffHurtValue = 0x30,   // 示例：伤害字段偏移（本游戏实测后改）
};

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
static void Hooked_SetTimeScale(float value, void* method) {
    g_tsMethod.store(method);
    const int n = g_tsCalls.fetch_add(1);
    if (n < 8) LOGI(TPL_LOG " set_timeScale call#%d arg=%.3f spd=%d", n + 1, value, (int)g_spd.load());
    const float v = g_spd.load() ? 1.8f : value;
    if (g_origSetTimeScale) g_origSetTimeScale(v, method);
}

// TODO：rva/expectInsn 由 dump.cs / IDA 实测后回填（0 = 仅按名）
static const HookSlot kSlots[] = {
    {"TimeScale", 0x0, 0x0, "UnityEngine", "Time", "set_timeScale",
     1, (void*)Hooked_SetTimeScale, (void**)&g_origSetTimeScale},
};

// ============================================================================
//  ⑦ 分组安装 + 懒装（全开关关 = 0 hook）
// ============================================================================
static const int kSlotsCount = (int)(sizeof(kSlots) / sizeof(kSlots[0]));

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

// ============================================================================
//  ⑤-a 资源型（Button：点一次 +9999）
//     抓点依据：斌哥走 ItemManager.AddItem(id, 9999)，id 按物品表 1=金币/2=钻石/3=积分
//     （type=3 另有 AccountInfo.points 写入，本版先只走 AddItem，points 待诊断确认）
// ============================================================================
static void* g_itemMgrCls = nullptr;
static std::atomic<int> g_grantSeq{0};
static std::atomic<bool> g_diagDone{false};
static std::atomic<int> g_diagTries{0};

static void DumpImagesOnce() {
    auto& api = GetApi();
    if (!api.ready) { if (!api.Load(TPL_TARGET_LIB)) { LOGW(TPL_LOG " Il2Cpp Load 失败"); return; } }
    typedef const char* (*ImgName_t)(const void*);
    static ImgName_t imgName = nullptr;
    if (!imgName) imgName = (ImgName_t)Il2Cpp::ResolveExport("il2cpp_image_get_name");
    size_t n = 0;
    void** asms = api.Assemblies(&n);
    LOGI(TPL_LOG " assemblies=%zu imgNameFn=%p", n, (void*)imgName);
    if (!asms) return;
    for (size_t i = 0; i < n && i < 80; ++i) {
        void* img = asms[i] ? api.ImageOf(asms[i]) : nullptr;
        LOGI(TPL_LOG "  asm[%zu] img=%p name=%s", i, img, (img && imgName) ? imgName(img) : "?");
    }
}

static void DumpClassShape(const char* name) {
    auto& api = GetApi();
    char buf[64];
    snprintf(buf, sizeof(buf), "%s", name);
    size_t L = strlen(buf);
    bool fieldsOnly = false;
    if (L > 10 && strcmp(buf + L - 10, "FieldsOnly") == 0) { buf[L - 10] = 0; fieldsOnly = true; }
    void* k = api.FindClassByShortName(buf);
    if (!k) { LOGW(TPL_LOG " %s 未找到", name); return; }
    LOGI(TPL_LOG " == %s @%p", name, k);
    if (api.class_fields && api.field_name) {
        void* iter = nullptr;
        void* f = nullptr;
        int n = 0;
        while ((f = api.class_fields(k, &iter)) != nullptr && n < 90) {
            ++n;
            const char* fn = api.field_name(f);
            const char* tn = (api.field_type && api.type_name) ? api.type_name(api.field_type(f)) : "?";
            LOGI(TPL_LOG "    F %-26s off=%-4zu ty=%s", fn ? fn : "?", api.field_offset ? api.field_offset(f) : 0, tn ? tn : "?");
        }
    }
    if (!fieldsOnly && api.class_methods && api.method_get_name) {
        void* iter = nullptr;
        void* m = nullptr;
        int n = 0;
        while ((m = api.class_methods(k, &iter)) != nullptr && n < 400) {
            ++n;
            const char* mn = api.method_get_name(m);
            if (!mn) continue;
            const bool want = (strstr(mn, "Add") || strstr(mn, "Change") || strstr(mn, "Gold")
                               || strstr(mn, "gold") || strstr(mn, "point") || strstr(mn, "Point")
                               || strstr(mn, "Hp") || strstr(mn, "atk") || strstr(mn, "Attr"));
            if (!want) continue;
            LOGI(TPL_LOG "    M %s/%d", mn, api.method_param_count ? api.method_param_count(m) : -1);
        }
    }
}

static void DiagConfigSet();

// 读 BattleRoleActor.combatPowerBaseAttributes（Dictionary<EAttributeEnum,float>）的 key/value
static void DumpAttrDict() {
    auto& api = GetApi();
    if (!api.ready) return;
    void* cls = api.FindClassByShortName("BattleRoleActor");
    if (!cls) { LOGW(TPL_LOG " BRA 类未找到"); return; }
    std::vector<void*> v;
    api.ScanInstances(cls, v, 512);
    LOGI(TPL_LOG " BRA 实例 %zu 个", v.size());
    for (size_t i = 0; i < v.size() && i < 2; ++i) {
        void* inst = v[i];
        if (!inst || !Il2Cpp::IsValidHeapPtr(inst)) continue;
        void* icls = api.object_class(inst);
        size_t off = icls ? api.FieldOffset(icls, "combatPowerBaseAttributes") : 0;
        if (!off) continue;
        void* dict = Il2Cpp::ReadPtr(inst, off);
        LOGI(TPL_LOG "  BRA[%zu]=%p attrDict=%p", i, inst, dict);
        if (!dict || !Il2Cpp::IsValidHeapPtr(dict)) continue;
        void* dcls = api.object_class(dict);
        size_t eOff = dcls ? api.FieldOffset(dcls, "_entries") : 0;
        size_t cOff = dcls ? api.FieldOffset(dcls, "_count") : 0;
        LOGI(TPL_LOG "   _entries=%zu _count=%zu", eOff, cOff);
        if (!eOff || !cOff) continue;
        const int32_t cnt = Il2Cpp::ReadI32(dict, cOff);
        void* arr = Il2Cpp::ReadPtr(dict, eOff);
        LOGI(TPL_LOG "   count=%d arr=%p", cnt, arr);
        if (!arr || cnt <= 0 || cnt > 200) continue;
        for (int k = 0; k < cnt && k < 14; ++k) {
            uint8_t* e = (uint8_t*)arr + 32 + (size_t)k * 16;
            const int32_t key = *(int32_t*)(e + 8);
            const float val = *(float*)(e + 12);
            LOGI(TPL_LOG "    attr[%d] key=%d val=%.2f", k, key, val);
        }
    }
}

static void DiagHotClasses() {
    auto& api = GetApi();
    if (!api.ready) { if (!api.Load(TPL_TARGET_LIB)) { LOGW(TPL_LOG " Il2Cpp Load 失败（diag）"); return; } }
    const char* names[] = {"ItemManager", "PlayerInfo", "AccountMgr", "BattleRoleActor"};
    int hit = 0;
    for (int i = 0; i < 4; ++i) {
        void* k = api.FindClassByShortName(names[i]);
        if (!k) { LOGW(TPL_LOG " resolve %s -> null", names[i]); continue; }
        ++hit;
        LOGI(TPL_LOG " resolve %s -> %p instance=%p", names[i], k, api.StaticRefField(k, "instance"));
    }
    if (hit > 0 && !g_diagDone.load()) {
        DumpClassShape("BattleRoleActor");
        {
            auto& a3 = GetApi();
            void* k3 = a3.FindClassByShortName("AccountMgr");
            if (k3 && a3.class_methods && a3.method_get_name) {
                void* it3 = nullptr; void* m3 = nullptr; int c3 = 0;
                while ((m3 = a3.class_methods(k3, &it3)) != nullptr && c3 < 60) {
                    ++c3;
                    const char* n3 = a3.method_get_name(m3);
                    if (!n3) continue;
                    LOGI(TPL_LOG "    AM %s/%d", n3, a3.method_param_count ? a3.method_param_count(m3) : -1);
                }
            }
        }
        {
            auto& a2 = GetApi();
            void* k2 = a2.FindClassByShortName("ItemManager");
            if (k2 && a2.class_methods && a2.method_get_name) {
                void* it = nullptr; void* mm = nullptr; int cnt = 0;
                while ((mm = a2.class_methods(k2, &it)) != nullptr && cnt < 80) {
                    ++cnt;
                    const char* mn = a2.method_get_name(mm);
                    const int pc = a2.method_param_count ? a2.method_param_count(mm) : -1;
                    char sig[160] = {0};
                    if (mn && (strncmp(mn, "Spawn", 5) == 0 || strncmp(mn, "Drop", 4) == 0
                               || strcmp(mn, "OnGetItem") == 0 || strcmp(mn, "GetNewItem") == 0
                               || strcmp(mn, "GetAll") == 0 || strcmp(mn, "GetItemByItemid") == 0)
                        && a2.method_param && a2.type_name) {
                        for (int pi = 0; pi < pc && pi < 4; ++pi) {
                            const char* tn = a2.type_name(a2.method_param(mm, pi));
                            snprintf(sig + strlen(sig), sizeof(sig) - strlen(sig), " p%d=%s", pi, tn ? tn : "?");
                        }
                        const char* rt = (a2.method_ret_type && a2.type_name) ? a2.type_name(a2.method_ret_type(mm)) : "?";
                        snprintf(sig + strlen(sig), sizeof(sig) - strlen(sig), " ret=%s", rt ? rt : "?");

                    }
                    LOGI(TPL_LOG "    IM %s/%d%s", mn ? mn : "?", pc, sig);
                }
            }
        }
    }
    if (hit > 0) g_diagDone.store(true);
}

// ===== 受保护的 il2cpp 调用（参数类型猜错时不崩进程）=====
struct InvokeCtx { void* mi; void* inst; void** args; void* exc; void* ret; bool done; };
static void InvokeThunk(void* p) {
    auto* c = static_cast<InvokeCtx*>(p);
    c->exc = nullptr;
    c->ret = GetApi().runtime_invoke(c->mi, c->inst, c->args, &c->exc);
    c->done = true;
}
static bool SafeInvoke(const char* tag, void* mi, void* inst, void** args, void** outRet, void** outExc) {
    if (!mi) return false;
    InvokeCtx c{mi, inst, args, nullptr, nullptr, false};
    if (!RunGuarded(tag, InvokeThunk, &c)) {
        LOGW(TPL_LOG " %s: 调用触发致命信号，已拦截（参数签名不符）", tag);
        return false;
    }
    if (outRet) *outRet = c.ret;
    if (outExc) *outExc = c.exc;
    return true;
}

// AccountMgr.instance -> 字段 AccountInfo(off=40) 才是账号数据（get_nowPlayerInfo 返回的是 PlayerInfo）
struct AcctRef { void* mgr; void* info; };

static AcctRef GetAccount() {
    auto& api = GetApi();
    if (!api.ready) { if (!api.Load(TPL_TARGET_LIB)) return {nullptr, nullptr}; }
    void* mgrCls = api.FindClassByShortName("AccountMgr");
    if (!mgrCls) return {nullptr, nullptr};
    void* mgr = api.StaticRefField(mgrCls, "instance");
    if (!mgr) return {nullptr, nullptr};
    size_t off = api.FieldOffset(mgrCls, "AccountInfo");
    if (!off) { LOGW(TPL_LOG " AccountMgr.AccountInfo 字段偏移未找到"); return {mgr, nullptr}; }
    void* info = Il2Cpp::ReadPtr(mgr, off);
    if (!info || !Il2Cpp::IsValidHeapPtr(info)) { LOGW(TPL_LOG " AccountInfo 为空（mgr=%p off=%zu）", mgr, off); return {mgr, nullptr}; }
    return {mgr, info};
}

// 写 AccountInfo 的整型字段，并把 AccountMgr.needSave 置真（否则只在内存里，读档即回滚）
static bool BumpAcctField(const char* field, int delta, const char* tag) {
    AcctRef a = GetAccount();
    if (!a.info) { LOGW(TPL_LOG " %s: AccountInfo 拿不到", tag); return false; }
    auto& api = GetApi();
    void* cls = api.object_class(a.info);
    size_t off = cls ? api.FieldOffset(cls, field) : 0;
    if (!off) {
        void* declCls = api.FindClassByShortName("AccountInfo");
        if (declCls) off = api.FieldOffset(declCls, field);
    }
    if (!off) { LOGW(TPL_LOG " %s: 字段 %s 未找到", tag, field); return false; }
    // points=Int64；sharedGold=Nullable<Int64>（bool@+0, long@+8）——一律按 64 位处理
    if (strcmp(field, "sharedGold") == 0) {
        uint8_t has = Il2Cpp::ReadU8(a.info, off);
        int64_t v = *(int64_t*)((uintptr_t)a.info + off + 8);
        *(int64_t*)((uintptr_t)a.info + off + 8) = v + (int64_t)delta;
        Il2Cpp::WriteU8(a.info, off, 1);
        LOGI(TPL_LOG " %s: sharedGold has=%u value %lld -> %lld (off=%zu)", tag, has, (long long)v, (long long)(v + delta), off);
    } else {
        int64_t v = *(int64_t*)((uintptr_t)a.info + off);
        *(int64_t*)((uintptr_t)a.info + off) = v + (int64_t)delta;
        LOGI(TPL_LOG " %s: %s(off=%zu) %lld -> %lld", tag, field, off, (long long)v, (long long)(v + delta));
    }
    if (a.mgr) {
        void* mgrCls = api.object_class(a.mgr);
        size_t nOff = mgrCls ? api.FieldOffset(mgrCls, "needSave") : 0;
        if (nOff) { Il2Cpp::WriteU8(a.mgr, nOff, 1); LOGI(TPL_LOG " %s: needSave=1（触发存档）", tag); }
    }
    return true;
}

// 直接写 AccountInfo 的整型字段（points=积分 / sharedGold=金币）
static bool BumpIntField(void* obj, const char* field, int delta, const char* tag) {
    if (!obj) { LOGW(TPL_LOG " %s: 对象为空", tag); return false; }
    auto& api = GetApi();
    void* cls = api.object_class(obj);
    const char* cname = cls ? api.ClassName(obj) : "?";
    size_t off = cls ? api.FieldOffset(cls, field) : 0;
    if (!off) {
        // get_nowPlayerInfo 返回的未定是 AccountInfo 本身；用声明类再试一次
        void* declCls = api.FindClassByShortName("AccountInfo");
        if (declCls) off = api.FieldOffset(declCls, field);
        LOGI(TPL_LOG " %s: obj=%p cls=%p(%s) 回退声明类 -> off=%zu", tag, obj, cls, cname, off);
    }
    if (!off) { LOGW(TPL_LOG " %s: 字段 %s 未找到（cls=%s）", tag, field, cname); return false; }
    const int32_t cur = Il2Cpp::ReadI32(obj, off);
    Il2Cpp::WriteI32(obj, off, cur + delta);
    LOGI(TPL_LOG " %s: %s(off=%zu) %d -> %d", tag, field, off, cur, cur + delta);
    return true;
}

static bool GrantPoints(int delta) {
    return BumpAcctField("points", delta, "积分");
}

static bool GrantGold(int delta) {
    if (BumpAcctField("sharedGold", delta, "金币")) return true;
    // 兜底：ItemManager.ChangeGold(delta)，走保护区（上一版在这里 SIGSEGV）
    auto& api = GetApi();
    if (!api.ready) api.Load(TPL_TARGET_LIB);
    void* cls = api.FindClassByShortName("ItemManager");
    if (!cls) return false;
    void* inst = api.StaticRefField(cls, "instance");
    void* mi = api.GetMethod(cls, "ChangeGold", 1);
    if (!inst || !mi) { LOGW(TPL_LOG " 金币: ChangeGold=%p instance=%p", mi, inst); return false; }
    void* args[1] = { api.BoxInt32(delta) };
    void* exc = nullptr;
    if (!SafeInvoke("ChangeGold", mi, inst, args, nullptr, &exc)) return false;
    LOGI(TPL_LOG " 金币: ChangeGold(%d) exc=%p", delta, exc);
    return exc == nullptr;
}

static std::atomic<int> g_itemNum{1};    // 数量

// 堆扫描找活实例（1.8 的 ItemManager/BattleRoleActor 静态 instance 都是空的）
static void* FindLiveInstance(const char* clsName, const char* tag) {
    auto& api = GetApi();
    if (!api.ready) { if (!api.Load(TPL_TARGET_LIB)) return nullptr; }
    void* cls = api.FindClassByShortName(clsName);
    if (!cls) { LOGW(TPL_LOG " %s: 类 %s 未找到", tag, clsName); return nullptr; }
    void* st = api.StaticRefField(cls, "instance");
    if (st && Il2Cpp::IsValidHeapPtr(st)) return st;
    std::vector<void*> v;
    api.ScanInstances(cls, v, 384);
    LOGI(TPL_LOG " %s: 堆扫描 %s 命中 %zu 个实例", tag, clsName, v.size());
    if (v.empty()) return nullptr;
    for (size_t i = 0; i < v.size() && i < 4; ++i) LOGI(TPL_LOG "   inst[%zu]=%p", i, v[i]);
    return v[0];
}

// 添加道具：ItemManager.OnGetItem(id)（1.8 方法表里最接近「直接获得」的入口）
static bool AddItemById(int id) {
    auto& api = GetApi();
    if (!api.ready) api.Load(TPL_TARGET_LIB);
    void* cls = api.FindClassByShortName("ItemManager");
    if (!cls) { LOGW(TPL_LOG " 添加道具: ItemManager 类未找到"); return false; }
    void* inst = FindLiveInstance("ItemManager", "添加道具");
    if (!inst) { LOGW(TPL_LOG " 添加道具: 无活实例"); return false; }
    void* mi = api.GetMethod(cls, "OnGetItem", 1);
    if (!mi) mi = api.GetMethod(cls, "GetNewItem", 1);
    if (!mi) { LOGW(TPL_LOG " 添加道具: OnGetItem/GetNewItem 均未找到"); return false; }
    const int n = g_itemNum.load() > 0 ? g_itemNum.load() : 1;
    void* args[1] = { api.BoxInt32(id) };
    void* exc = nullptr;
    if (!SafeInvoke("OnGetItem", mi, inst, args, nullptr, &exc)) return false;
    LOGI(TPL_LOG " 添加道具 id=%d n=%d mi=%p exc=%p", id, n, mi, exc);
    return exc == nullptr;
}

static bool GrantItem(int id, int count) {
    auto& api = GetApi();
    if (!api.ready) return false;
    if (!g_itemMgrCls) g_itemMgrCls = api.FindClassByShortName("ItemManager");
    if (!g_itemMgrCls) { LOGW(TPL_LOG " ItemManager 类未找到"); return false; }
    void* inst = api.StaticRefField(g_itemMgrCls, "instance");
    if (!inst) { LOGW(TPL_LOG " ItemManager.instance 为空"); return false; }
    void* mi = api.GetMethod(g_itemMgrCls, "AddItem", 2);
    if (!mi) { LOGW(TPL_LOG " AddItem(argc=2) 未找到（待确认签名）"); return false; }
    const int n = g_grantSeq.fetch_add(1);
    void* args[2] = { api.BoxInt32(id), api.BoxInt32(count) };
    void* exc = nullptr;
    api.runtime_invoke(mi, inst, args, &exc);
    LOGI(TPL_LOG " AddItem(id=%d n=%d) #%d exc=%p", id, count, n + 1, exc);
    return exc == nullptr;
}

static bool AnyFeatureOn() {
    return g_spd.load();
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
bool 旷野征途_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;

    InstallFaultGuard();          // §④ 先装兜底，再装钩
    g_libReady.store(true);
    SyncHooks();                  // 已 ON 的开关立即装；全关保持 0 hook
    LOGI(TPL_LOG " lib ready (dual-path, lazy hooks)");
    return true;
}

bool 旷野征途_IsReady() {
    return g_libReady.load();
}

void 旷野征途_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case 旷野征途_FEAT_GOLD:    if (enabled && !GrantGold(9999)) GrantItem(1, 9999); break;
        case 旷野征途_FEAT_DIAMOND: if (enabled) GrantItem(2, 9999); break;
        case 旷野征途_FEAT_POINT:   if (enabled && !GrantPoints(9999)) GrantItem(3, 9999); break;
        case 旷野征途_FEAT_SPD: g_spd.store(enabled); break;
        case 旷野征途_FEAT_DMG: g_dmg.store(enabled); break;
        case 旷野征途_FEAT_INV: g_inv.store(enabled); break;
        default: break;
    }
    SyncHooks();
}

void 旷野征途_OnValueChanged(int /*featNum*/, int /*value*/) {
}

void 旷野征途_Tick(const bool* features, const int* /*values*/) {
    if (!features) return;

    g_spd.store(features[旷野征途_FEAT_SPD]);
    g_dmg.store(features[旷野征途_FEAT_DMG]);
    g_inv.store(features[旷野征途_FEAT_INV]);

    if (g_libReady.load() && !g_diagDone.load()) {
        // il2cpp 要求线程 attach：Tick 跑在 Mod 后台线程，不 attach 则 domain 枚举返回 0
        {
            auto& api0 = GetApi();
            if (api0.thread_attach) {
                void* d0 = api0.Domain();
                if (d0) api0.thread_attach(d0);
            }
        }
        const int t = g_diagTries.fetch_add(1);
        if (t == 0) DumpImagesOnce();
        if ((t % 5) == 0) DiagHotClasses();     // 每 1s 重试，热更 dll 加载后才有类
        if (t > 120) g_diagDone.store(true);    // 24s 后放弃
    }

    // 兜底：库晚于首帧映射时，这里补一次
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) {
        InstallFaultGuard();
        g_libReady.store(true);
        LOGI(TPL_LOG " lib ready (via Tick fallback)");
    }
    SyncHooks();

    // 主动钉 timeScale：不依赖游戏主动调用 setter（本作可能在启动时只调一次）
    if (g_spd.load()) {
        void* m = g_tsMethod.load();
        if (m && g_origSetTimeScale) {
            g_origSetTimeScale(1.8f, m);
            static int tickLog = 0;
            if (tickLog < 3) { ++tickLog; LOGI(TPL_LOG " force timeScale=1.8 (direct orig call)"); }
        }
    }

    // ★ 禁止在此 SafeProbe / 扫堆 / 遍历对象 / 逐对象写（Tick 频率高，代价爆炸）
}

void 旷野征途_ForceClear() {
    g_spd.store(false);
    g_dmg.store(false);
    g_inv.store(false);
    g_reenter.store(false);
    // TODO：新增开关一并复位
}
