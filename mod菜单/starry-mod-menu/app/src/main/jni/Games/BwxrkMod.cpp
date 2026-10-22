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

#include "BwxrkMod.h"

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

// 生成器把 [Bwxrk] 替换为本游戏标签（如 [jfqs]），日志过滤用 adb logcat -s Mod_Menu
#define TPL_LOG "[Bwxrk]"

// 注册表模式不依赖 GameConfig.h（legacy 配置中心）。
// Cocos 系生成后把下面这处（含各 hook 内引用）统一换成目标库名。
#define TPL_TARGET_LIB sec_lib_il2cpp()

// ============================================================================
//  ① 开关与 orig 指针
//     g_libReady = 「库已映射」≠「已挂钩」；全开关关闭时应当保持 0 个 hook。
// ============================================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_allSkill{false};   // ① 拿全部技能
static std::atomic<bool> g_reenter{false};

// ---- ① 拿全部技能 -----------------------------------------------------------
// 机制（照斌哥 1:1）：玩家点技能 → 游戏调 BattleCoreSys::SelectRandomSkill(gid, idx)
//   我们拦下，改投【游戏自己的】帧指令 FrameCmdThreeStarSelectSkill{gid, idxList=[0,1,2]}。
//   本作自带公开 API BattleCoreSys::SendThreeStarSelectSkill(uint uid, List<sbyte> idxList)
//   就是这条帧指令的唯一发送口（与斌哥手搓 BuildFrameCmdData+AddInputCmd 产出同一条帧指令）。
static std::atomic<int>      g_quota{0};         // 每局剩余次数
static std::atomic<void*>    g_lastBattle{nullptr};  // 换局检测锚（BattleCoreSys+0x20）
static std::atomic<uint32_t> g_allHits{0};
static std::atomic<uint32_t> g_quotaResets{0};

typedef void (*OnUpdate_t)(void* self, void* method);
typedef void (*SelectRandomSkill_t)(void* self, uint32_t gid, uint32_t skillIdx, void* method);
typedef bool (*SendThreeStar_t)(void* self, uint32_t uid, void* idxList, void* method);

static OnUpdate_t            g_origOnUpdate = nullptr;
static SelectRandomSkill_t   g_origSelectRandomSkill = nullptr;
static SendThreeStar_t       g_fnSendThreeStar = nullptr;   // 不挂钩，只调用

// ============================================================================
//  ② 字段偏移与 RVA
//     来源（同轮实测，见 10_docs/保卫向日葵/斌哥包抓点_20261015.md）：
//       · 字段偏移 = 斌哥壳自愈自检串（[BW自愈] %s.%s = 0x%X (与建档一致)）
//       · RVA     = Il2CppDumper(hot files/libil2cpp.so + hot metadata) 精确命中
//     ★本作运行时加载的是 dodjoy 热更落地的 files/libil2cpp.so（≠APK 内嵌那份），
//       两构建 RVA 不同 ⇒ 下面用【热更构建】RVA + 序言校验；校验不过自动退按名解析。
//     ★2026-10-23 热更换构建（63,343,272 B / md5 8a66e68f16675d59d9d6bd1b900daca2；
//       上一版 62,235,240 B）⇒ 已按 10_docs/保卫向日葵/热更RVA重定位_20261023.md 重定位：
//       该 so 的 PT_LOAD 实测 p_vaddr − p_offset = 0（RVA == file offset）。
//       旧热更构建 62,235,240 B 对应值：OnUpdate 0x250307C / SelectRandomSkill 0x2508CF0。
// ============================================================================
enum : size_t {
    kOffCoreBattle = 0x20,   // BattleCoreSys.<Battle>（换局检测锚；与 +0x10 m_runtimeInput 同链）
};
enum : int {
    kQuotaPerBattle = 30,    // 每局配额（照斌哥「每局30次」）
};
enum : uint32_t {
    kIdxListCount = 3,       // idxList = [0,1,2]
};

// 热更构建（files/libil2cpp.so，20261023 新构建 63,343,272 B / md5 8a66e68f…）RVA + 序言首指令（小端 u32）
// 序言字节已实测复核（0x25D288C: ff 03 01 d1；0x25D8510: f5 53 be a9）⇒ 两序言值未变，仅 RVA 漂移
static const uintptr_t kRvaOnUpdate           = 0x25D288C;  // 旧热更构建 0x250307C
static const uint32_t  kInsnOnUpdate          = 0xD10103FF; // sub sp, sp, #0x40
static const uintptr_t kRvaSelectRandomSkill  = 0x25D8510;  // 旧热更构建 0x2508CF0
static const uint32_t  kInsnSelectRandomSkill = 0xA9BE53F5; // stp x21, x20, [sp, #-0x20]!

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

// 写托管对象指针字段（目标必须是本进程堆内的合法托管对象）
static void SafeWritePtr(void* obj, size_t off, void* v) {
    if (!OkPtr(obj) || !OkPtr(v)) return;
    *reinterpret_cast<void**>(reinterpret_cast<uintptr_t>(obj) + off) = v;
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
//  ⑤.5 调用游戏自带 API（只调用，不挂钩）
//       BattleCoreSys::SendThreeStarSelectSkill(uint uid, List<sbyte> idxList)
//       就是 FrameCmdThreeStarSelectSkill{gid, idxList} 这条帧指令的唯一发送口。
//       List<sbyte> 在 metadata 里没有静态引用可查 ⇒ 从该方法第 2 个形参的
//       Il2CppType 反推 inflate 类（class_from_il2cpp_type）。
// ============================================================================
typedef void* (*class_from_il2cpp_type_t)(void*);
typedef void* (*object_new_t)(void*);
typedef void* (*array_new_t)(void*, size_t);

static class_from_il2cpp_type_t g_fnClassFromType = nullptr;
static object_new_t             g_fnObjectNew = nullptr;
static array_new_t              g_fnArrayNew = nullptr;
static void*                    g_listSbyteKlass = nullptr;
static std::atomic<int>         g_gameApiState{0};   // 0=未解析 1=就绪 2=解析失败(可重试)
// ★审计发现 1（2026-10-15）修复：下面 4 个指针是**非原子**全局，首次解析必须串行化，
//   否则两线程并发首调会读到半写指针（实测单线程未触发，属加固）。
static std::mutex               g_apiMutex;

enum : size_t {
    kOffListItems = 0x10,   // List<T>._items
    kOffListSize  = 0x18,   // List<T>._size
    kOffArrayData = 0x20,   // il2cpp 数组元素数据区
};

static void* ResolveGameFn(const char* ns, const char* cls, const char* method, int argc,
                           void** outMi) {
    if (outMi) *outMi = nullptr;
    if (!EnsureApi()) return nullptr;
    void* k = FindClassNs(ns, cls);
    if (!k) k = GetApi().FindClassByShortName(cls);
    if (!k) { LOGW(TPL_LOG " 类未找到 %s.%s", ns, cls); return nullptr; }
    void* mi = GetApi().GetMethod(k, method, argc);
    if (!mi) { LOGW(TPL_LOG " 方法未找到 %s.%s/%d", cls, method, argc); return nullptr; }
    if (outMi) *outMi = mi;
    return *reinterpret_cast<void**>(mi);
}

// 解析一次游戏 API；失败留状态 2 供下次重试（只在玩家点技能时调用，频率极低）
// ★并发：首次解析写 4 个非原子全局 ⇒ 必须持锁 + double-check（审计发现 1）。
//   注意不能改用 std::call_once —— 那会「失败也只跑一次」，丢掉状态 2 的重试语义。
static bool EnsureGameApi() {
    if (g_gameApiState.load() == 1) return true;

    std::lock_guard<std::mutex> lk(g_apiMutex);
    if (g_gameApiState.load() == 1) return true;      // double-check：等锁期间别人已装好

    void* mi = nullptr;
    g_fnSendThreeStar = reinterpret_cast<SendThreeStar_t>(
        ResolveGameFn("C6Game", "BattleCoreSys", "SendThreeStarSelectSkill", 2, &mi));
    g_fnClassFromType = reinterpret_cast<class_from_il2cpp_type_t>(
        Il2Cpp::ResolveExport("il2cpp_class_from_il2cpp_type"));
    g_fnObjectNew = reinterpret_cast<object_new_t>(Il2Cpp::ResolveExport("il2cpp_object_new"));
    g_fnArrayNew  = reinterpret_cast<array_new_t>(Il2Cpp::ResolveExport("il2cpp_array_new"));

    if (mi && g_fnClassFromType && GetApi().method_param) {
        void* ty = GetApi().method_param(mi, 1);          // List<sbyte> 的 Il2CppType*
        if (ty) g_listSbyteKlass = g_fnClassFromType(ty);
    }

    const bool ok = g_fnSendThreeStar && g_fnClassFromType && g_fnObjectNew &&
                    g_fnArrayNew && g_listSbyteKlass;
    if (ok) {
        g_gameApiState.store(1);
        LOGI(TPL_LOG " 游戏API 就绪 SendThreeStar=%p List<sbyte>=%p",
             reinterpret_cast<void*>(g_fnSendThreeStar), g_listSbyteKlass);
    } else {
        g_gameApiState.store(2);
        LOGW(TPL_LOG " 游戏API 未就绪(Send=%p List=%p cfType=%p objNew=%p arrNew=%p) — 功能降级",
             reinterpret_cast<void*>(g_fnSendThreeStar), g_listSbyteKlass,
             reinterpret_cast<void*>(g_fnClassFromType), reinterpret_cast<void*>(g_fnObjectNew),
             reinterpret_cast<void*>(g_fnArrayNew));
    }
    return ok;
}

/** 造 List<sbyte>{0,1,2}（全托管对象，走 il2cpp 分配器，不碰第三方实例）。 */
static void* BuildIdxList012() {
    if (!EnsureGameApi()) return nullptr;
    if (!GetApi().get_corlib) return nullptr;
    void* corlibImage = GetApi().get_corlib();
    if (!corlibImage) return nullptr;
    void* sbyteKlass = GetApi().ClassFromName(corlibImage, "System", "SByte");
    if (!sbyteKlass) { LOGW(TPL_LOG " 未找到 System.SByte"); return nullptr; }

    void* arr = g_fnArrayNew(sbyteKlass, kIdxListCount);
    if (!arr) { LOGW(TPL_LOG " sbyte[] 分配失败"); return nullptr; }
    int8_t* vec = reinterpret_cast<int8_t*>(reinterpret_cast<uintptr_t>(arr) + kOffArrayData);
    vec[0] = 0; vec[1] = 1; vec[2] = 2;

    void* list = g_fnObjectNew(g_listSbyteKlass);
    if (!list) { LOGW(TPL_LOG " List<sbyte> 分配失败"); return nullptr; }
    SafeWritePtr(list, kOffListItems, arr);
    SafeWriteI32(list, kOffListSize, static_cast<int32_t>(kIdxListCount));
    return list;
}

// ============================================================================
//  ⑥ hook 函数与挂点表
//     铁律：一个具体方法一个槽；绝不 hook 共享 invoke 入口。
// ============================================================================

// ---- ① 换局检测 / 配额重置（照斌哥：钩 OnUpdate）----------------------------
static void Hooked_OnUpdate(void* self, void* method) {
    if (g_origOnUpdate) g_origOnUpdate(self, method);   // 铁律：先调 orig

    if (!g_allSkill.load()) return;
    if (!OkPtr(self)) return;

    void* battle = Il2Cpp::ReadPtr(self, kOffCoreBattle);
    void* last = g_lastBattle.load();
    if (battle == last) return;

    g_lastBattle.store(battle);
    g_quota.store(kQuotaPerBattle);
    g_quotaResets.fetch_add(1);
    LOGI(TPL_LOG " 换局检测: Battle %p -> %p，拿全部技能配额重置为 %d",
         last, battle, kQuotaPerBattle);
}

// ---- ① 拿全部技能（照斌哥：拦单点，改投三选帧指令）--------------------------
struct SendCtx {
    void*    self;
    uint32_t gid;
    void*    list;
    bool     ok;
};
static void SendThunk(void* p) {
    SendCtx* c = static_cast<SendCtx*>(p);
    c->ok = g_fnSendThreeStar(c->self, c->gid, c->list, nullptr);
}

static void Hooked_SelectRandomSkill(void* self, uint32_t gid, uint32_t skillIdx, void* method) {
    // ★本槽【故意不先调 orig】：orig 会再发一条「单选」帧指令，两次投递=一次点击拿 4 个
    //  （超出游戏自检上限 GetIntParam(545)=4 的余量）。照斌哥语义＝用三选帧指令【替换】单选。
    //  SelectRandomSkill 本体只是守卫 + 尾调 SendSelectRandomSkillCmd，无额外簿记（已反汇编核实）。
    if (!g_allSkill.load() || !OkPtr(self) || !EnsureGameApi() || g_quota.load() <= 0) {
        if (g_origSelectRandomSkill) g_origSelectRandomSkill(self, gid, skillIdx, method);
        return;
    }

    int q = g_quota.load();
    void* list = BuildIdxList012();
    if (!list) {
        if (g_origSelectRandomSkill) g_origSelectRandomSkill(self, gid, skillIdx, method);
        return;
    }

    SendCtx ctx{self, gid, list, false};
    const bool ran = RunGuarded("Send3Star", SendThunk, &ctx);
    if (!ran || !ctx.ok) {
        // 游戏侧守卫没过或调用异常 → 退回单选，不消耗配额
        if (g_origSelectRandomSkill) g_origSelectRandomSkill(self, gid, skillIdx, method);
        LOGW(TPL_LOG " 三选帧指令未生效(gid=%u ran=%d ok=%d) — 退回单选",
             gid, (int)ran, (int)ctx.ok);
        return;
    }

    g_quota.store(q - 1);
    g_allHits.fetch_add(1);
    LOGI(TPL_LOG " 拿全部技能 hit gid=%u idx=%u -> idxList=[0,1,2] 剩余配额=%d",
         gid, skillIdx, q - 1);
}

// ---- ② 战斗加速：已按用户 m00005 终审【放弃并摘除】------------------------
//  实机（2026-10-15 04:57）钩子装上了（`GetFinalTimeScale hooked @rva 0x2785F48`），
//  但用户实测「感觉不到 / 没变化」⇒ 按硬约束 #6「不能做就算了」直接移除，不做迭代。
//  未验证假设（留给后人）：该函数可能只在【进战斗/改档位】时被调一次，之后
//  timeScale 已固化；玩家在战斗中途拖滑块不会重新触发。斌哥自己也**从未实机验证过**
//  这条（其日志 `★战斗加速 就绪=0`，偏移守卫直接拒装）⇒ 本功能在对照包里同样未证实。

// rva/expectInsn 来自同轮 Il2CppDumper（热更构建）+ 序言字节实测；0 = 仅按名
static const HookSlot kSlots[] = {
    {"OnUpdate", kRvaOnUpdate, kInsnOnUpdate, "C6Game", "BattleCoreSys", "OnUpdate",
     0, (void*)Hooked_OnUpdate, (void**)&g_origOnUpdate},
    {"SelectRandomSkill", kRvaSelectRandomSkill, kInsnSelectRandomSkill, "C6Game", "BattleCoreSys",
     "SelectRandomSkill", 2, (void*)Hooked_SelectRandomSkill, (void**)&g_origSelectRandomSkill},
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

static bool AnyFeatureOn() {
    return g_allSkill.load();
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
bool Bwxrk_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;

    InstallFaultGuard();          // §④ 先装兜底，再装钩
    g_libReady.store(true);
    SyncHooks();                  // 已 ON 的开关立即装；全关保持 0 hook
    LOGI(TPL_LOG " lib ready (dual-path, lazy hooks)");
    return true;
}

bool Bwxrk_IsReady() {
    return g_libReady.load();
}

void Bwxrk_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case BWXRK_FEAT_ALLSKILL:
            g_allSkill.store(enabled);
            if (enabled) {
                // 刚打开时立刻给一局配额（不必等下一次换局）；锚不同即视为新局
                g_lastBattle.store(nullptr);
                g_quota.store(kQuotaPerBattle);
                LOGI(TPL_LOG " 拿全部技能 ON（每局配额 %d）", kQuotaPerBattle);
            }
            break;
        default: break;
    }
    SyncHooks();
}

void Bwxrk_OnValueChanged(int /*featNum*/, int /*value*/) {
    // 本作已无滑块类功能（战斗加速按用户 m00005 终审摘除）
}

void Bwxrk_Tick(const bool* features, const int* /*values*/) {
    if (!features) return;

    g_allSkill.store(features[BWXRK_FEAT_ALLSKILL]);

    // 兜底：库晚于首帧映射时，这里补一次
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) {
        InstallFaultGuard();
        g_libReady.store(true);
        LOGI(TPL_LOG " lib ready (via Tick fallback)");
    }
    SyncHooks();

    // ★ 禁止在此 SafeProbe / 扫堆 / 遍历对象 / 逐对象写（Tick 频率高，代价爆炸）
}

void Bwxrk_ForceClear() {
    g_allSkill.store(false);
    g_quota.store(0);
    g_lastBattle.store(nullptr);
    g_reenter.store(false);
}
