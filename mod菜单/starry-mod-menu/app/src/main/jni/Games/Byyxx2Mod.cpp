// ============================================================================
//  不一样修仙2（com.wingjoy.immortal2 1.4.135 / vc1007）— 星光菜单游戏模块
//
//  机制定案（2026-11-02，证据：libil2cpp.so sha256 87565310741d8c49 与官方逐字节相同，
//  global-metadata 明文；DumpRva 反查方法名 + capstone 反汇编访问器取字段偏移）：
//    · 对照方（斌哥改包）唯一一处 12 字节 Dobby 跳桩 = GameMgr::Update @ RVA 0xF6F788；
//      回调里每帧按硬编码偏移写游戏内存。本模块 1:1 沿用同一挂点（常驻一帧钩），
//      区别只是「不直接写字段，而是调游戏自己的 setter/Add*（走 IL2CPP 反射）」，
//      以便拿到 set_Money 内部的 UI 刷新 + 变更事件派发（对照方是裸写字段）。
//    · 实例获取：GameMgr::get_Player = `ldr x0,[x0,#0x68]; ret` → GameMgr 实例 + 0x68。
//    · 数值全是 double（访问器 `ldr d0,[x0,#imm]`），setter 统一 fmax(v,0) 后落字段：
//         Money 0x190 / Renown 0x220 / SectContribution 0x100 / Exp 0x170 /
//         LifeTime 0x180 / MoneyIncSpeed 0x250 / ExpIncSpeed 0x1B8 / LifeTimeIncSpeed 0x218
//      ↑ 这些偏移**不写死使用**：运行期从 getter 首条 LDR 立即数里反推（见 DeriveImmOff），
//        写死值只当指纹校验；游戏换版 ⇒ 反推值变了会打 W 日志，不静默错写。
//    · 修为走 AddExp（PlayerLogic 只是 `mov x1,xzr; b CharacterLogic::AddExp` 的薄转发），
//      寿元走 AddLifeTime（同型薄转发），金币/声望/宗门贡献走 set_X(当前 + 加成)。
//
//  防闪退设施沿用骨架 v3.1（懒装 / 双路径挂点 / RVA 漂移拒绝 / 幂等安装 /
//  致命信号保护区 / 槽位熔断），删任何一节都可能复现历史闪退。
//  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
// ============================================================================

#include "Byyxx2Mod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
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

#define TPL_LOG "[Byyxx2]"
#define TPL_TARGET_LIB sec_lib_il2cpp()

// ============================================================================
//  ① 开关 / 实例 / 待执行动作
// ============================================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_updHooked{false};
static std::atomic<bool> g_inAction{false};     // 防重入（我们调游戏代码期间不进 RunPending）

static std::atomic<void*>     g_gameMgr{nullptr};
static std::atomic<void*>     g_player{nullptr};
static std::atomic<uint64_t>  g_updHits{0};
static std::atomic<uint32_t>  g_pending{0};     // 位掩码：待执行的一次性动作
static std::atomic<int64_t>   g_amount{1000000};// 单次加成
static std::atomic<int64_t>   g_incMult{1000};  // 自增倍率

static std::atomic<bool> g_incMoneyOn{false};
static std::atomic<bool> g_incExpOn{false};
static std::atomic<bool> g_incLifeOn{false};
static std::atomic<bool> g_noAd{false};          // 免广告：钩 GeneralInfoLogic::JedgeCanBlockAd
static std::atomic<bool> g_adHooked{false};
static std::atomic<uint64_t> g_adHits{0};

enum : uint32_t {
    kActMoney = 1u << 0,
    kActRenown = 1u << 1,
    kActSect = 1u << 2,
    kActExp = 1u << 3,
    kActLife = 1u << 4,
};

// ============================================================================
//  ② 字段偏移与 RVA（换包必须重 dump；运行期用 getter 首指令反推校验）
// ============================================================================
enum : size_t {
    kOffGameMgrPlayer    = 0x68,   // GameMgr::get_Player = ldr x0,[x0,#0x68]
    kOffMoney            = 0x190,
    kOffRenown           = 0x220,
    kOffSectContribution = 0x100,
    kOffExp              = 0x170,
    kOffLifeTime         = 0x180,
    kOffMoneyIncSpeed    = 0x250,
    kOffExpIncSpeed      = 0x1B8,
    kOffLifeTimeIncSpeed = 0x218,
};

static const uintptr_t kRvaGameMgrUpdate   = 0xF6F788;   // Immortal.GameMgr::Update
static const uint32_t  kInsnGameMgrUpdate  = 0xD10383FFu; // sub sp, sp, #0xe0

// 访问器首指令形态：ldr dN,[x0,#imm] / ldr xN,[x0,#imm]（掩码后）
static const uint32_t kOpLdrD0 = 0xFD400000u;  // 掩码 0xFFC00000
static const uint32_t kOpLdrX0 = 0xF9400000u;  // 掩码 0xFFC00000
static const uint32_t kInsnRet = 0xD65F03C0u;  // ret

// ============================================================================
//  ③ 安全助手（防闪退核心；新代码禁止绕开它们裸读写内存）
// ============================================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

static size_t g_offPlayer = kOffGameMgrPlayer;   // 运行期可被 get_Player 反推值覆盖
static size_t g_offMoney = kOffMoney, g_offRenown = kOffRenown,
              g_offSect = kOffSectContribution, g_offExp = kOffExp,
              g_offLife = kOffLifeTime, g_offMoneyInc = kOffMoneyIncSpeed,
              g_offExpInc = kOffExpIncSpeed, g_offLifeInc = kOffLifeTimeIncSpeed;

static double RdF64(const void* obj, size_t off) {
    return *reinterpret_cast<const double*>(static_cast<const char*>(obj) + off);
}

// ============================================================================
//  ④ 并发 + 致命信号兜底 + 槽位熔断（骨架 v3.1，原样保留）
// ============================================================================
static std::mutex g_installMutex;
static std::mutex g_guardMutex;

static thread_local sigjmp_buf            t_faultJmp;
static thread_local volatile sig_atomic_t t_inGuard    = 0;
static thread_local volatile sig_atomic_t t_faultDepth = 0;
static thread_local volatile int          t_faultSig   = 0;
static thread_local volatile void*        t_faultAddr  = nullptr;

static thread_local char t_altStack[65536];
static thread_local bool t_altStackSet = false;

static struct sigaction g_oldSegv;
static struct sigaction g_oldBus;
static struct sigaction g_oldIll;
static std::atomic<bool> g_guardArmed{false};
static bool g_lastFatal = false;

static void Byyxx2FaultHandler(int sig, siginfo_t* si, void* /*uc*/) {
    if (t_inGuard) {
        if (++t_faultDepth > 4) {
            signal(sig, SIG_DFL);
            raise(sig);
            return;
        }
        t_faultSig  = sig;
        t_faultAddr = si ? si->si_addr : nullptr;
        siglongjmp(t_faultJmp, 1);
    }
    const struct sigaction* old = (sig == SIGSEGV) ? &g_oldSegv
                               : (sig == SIGBUS)  ? &g_oldBus
                                                  : &g_oldIll;
    if (old->sa_sigaction && old->sa_sigaction != Byyxx2FaultHandler) {
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
    ss.ss_sp    = t_altStack;
    ss.ss_size  = sizeof(t_altStack);
    ss.ss_flags = 0;
    if (sigaltstack(&ss, nullptr) != 0) {
        LOGW(TPL_LOG " sigaltstack failed — 兜底降级为无备用栈");
    }
}

static void InstallFaultGuard() {
    if (g_guardArmed.load()) return;
    std::lock_guard<std::mutex> lk(g_guardMutex);
    if (g_guardArmed.load()) return;

    struct sigaction sa;
    std::memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = Byyxx2FaultHandler;
    sa.sa_flags     = SA_SIGINFO | SA_ONSTACK | SA_NODEFER;
    sigemptyset(&sa.sa_mask);

    sigaction(SIGSEGV, &sa, &g_oldSegv);
    sigaction(SIGBUS,  &sa, &g_oldBus);
    sigaction(SIGILL,  &sa, &g_oldIll);

    g_guardArmed.store(true);
    LOGI(TPL_LOG " fault guard armed (SIGSEGV/SIGBUS/SIGILL, altstack=%zu)",
         sizeof(t_altStack));
}

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
    t_inGuard = 0;
    g_lastFatal = true;
    LOGW(TPL_LOG " %s dobby SIG(%d) @%p — 槽位熔断，游戏继续",
         tag, (int)t_faultSig, (void*)t_faultAddr);
    if (orig) *orig = nullptr;
    return false;
}

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

static const int kSlotsMax = 8;
static bool g_slotDead[kSlotsMax];
static std::atomic<int> g_installedCount{0};

// ============================================================================
//  ⑤ 双路径挂点机器（骨架 v3.1，原样保留）
// ============================================================================
static bool IsLibMapLine(const char* line, const char* lib) {
    return std::strstr(line, lib) != nullptr && std::strstr(line, ".so") != nullptr;
}

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
        if (!std::strchr(perm, 'x')) continue;
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
        if (!ReadU32Guarded(tag, va, &got)) return false;
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

static void* MethodPtr(void* mi) {
    if (!mi) return nullptr;
    void* fn = *reinterpret_cast<void**>(mi);
    if (!Il2Cpp::IsExecutableRange(reinterpret_cast<uintptr_t>(fn), 8)) return nullptr;
    return fn;
}

// 从 getter 首指令 `ldr dN/xN,[x0,#imm]` 反推字段偏移；形态不符返回 fallback
static size_t DeriveImmOff(void* mi, uint32_t expectOp, const char* name, size_t fallback) {
    void* fn = MethodPtr(mi);
    if (!fn) {
        LOGW(TPL_LOG " derive %s: 无方法指针，沿用静态偏移 0x%zX", name, fallback);
        return fallback;
    }
    uint32_t i0 = 0, i1 = 0;
    if (!ReadU32Guarded(name, fn, &i0)) return fallback;
    if (!ReadU32Guarded(name, static_cast<const char*>(fn) + 4, &i1)) return fallback;
    if (i1 != kInsnRet || (i0 & 0xFFC00000u) != expectOp || ((i0 >> 5) & 31u) != 0) {
        LOGW(TPL_LOG " derive %s: 形态非 ldr/ret (0x%08X 0x%08X)，沿用静态偏移 0x%zX",
             name, i0, i1, fallback);
        return fallback;
    }
    size_t off = static_cast<size_t>((i0 >> 10) & 0xFFFu) * 8u;
    if (off != fallback) {
        LOGW(TPL_LOG " derive %s: 反推 0x%zX ≠ 静态 0x%zX（版本漂移？取反推值）",
             name, off, fallback);
    }
    return off;
}

// ============================================================================
//  ⑥ IL2CPP 方法表（按名解析；runtime_invoke 调用，不裸跳 methodPointer）
// ============================================================================
struct MethodSet {
    void* kChar = nullptr;
    void* kGameMgr = nullptr;
    void* getPlayer = nullptr;

    void* getMoney = nullptr;   void* setMoney = nullptr;
    void* getRenown = nullptr;  void* setRenown = nullptr;
    void* getSect = nullptr;    void* setSect = nullptr;
    void* getExp = nullptr;     void* addExp = nullptr;
    void* getLife = nullptr;    void* addLife = nullptr;
    void* getMoneyInc = nullptr; void* setMoneyInc = nullptr;
    void* getExpInc = nullptr;   void* setExpInc = nullptr;
    void* getLifeInc = nullptr;  void* setLifeInc = nullptr;
    bool ok = false;
};

static MethodSet g_m;
static std::atomic<int> g_resolveState{0};   // 0=未解析 1=成功 2=失败待重试
static std::mutex g_resolveMutex;

static void LogSig(const char* name, void* mi) {
    auto& api = GetApi();
    if (!api.method_param_count || !api.method_param || !api.type_name) return;
    uint32_t argc = api.method_param_count(mi);
    char buf[256];
    int n = std::snprintf(buf, sizeof(buf), "%s argc=%u:", name, argc);
    for (uint32_t i = 0; i < argc && n > 0 && n < (int)sizeof(buf) - 1; ++i) {
        void* t = api.method_param(mi, i);
        char* tn = t ? api.type_name(t) : nullptr;
        n += std::snprintf(buf + n, sizeof(buf) - n, " %s", tn ? tn : "?");
        if (tn) std::free(tn);
    }
    LOGI(TPL_LOG " sig %s", buf);
}

static void* GetM(void* klass, const char* name, int argc) {
    if (!klass) return nullptr;
    void* mi = GetApi().GetMethod(klass, name, argc);
    if (!mi) LOGW(TPL_LOG " method miss %s(argc=%d) — degrade", name, argc);
    return mi;
}

static bool ResolveMethods() {
    if (g_resolveState.load() == 1) return true;
    std::lock_guard<std::mutex> lk(g_resolveMutex);
    if (g_resolveState.load() == 1) return true;
    if (!EnsureApi()) return false;

    MethodSet m;
    m.kChar = FindClassNs("Immortal", "CharacterLogic");
    if (!m.kChar) m.kChar = GetApi().FindClassByShortName("CharacterLogic");
    m.kGameMgr = FindClassNs("Immortal", "GameMgr");
    if (!m.kGameMgr) m.kGameMgr = GetApi().FindClassByShortName("GameMgr");

    if (!m.kChar || !m.kGameMgr) {
        LOGW(TPL_LOG " resolve: class miss char=%p gameMgr=%p — retry later",
             m.kChar, m.kGameMgr);
        g_resolveState.store(2);
        return false;
    }

    m.getPlayer = GetM(m.kGameMgr, "get_Player", 0);

    m.getMoney  = GetM(m.kChar, "get_Money", 0);
    m.setMoney  = GetM(m.kChar, "set_Money", 1);
    m.getRenown = GetM(m.kChar, "get_Renown", 0);
    m.setRenown = GetM(m.kChar, "set_Renown", 1);
    m.getSect   = GetM(m.kChar, "get_SectContribution", 0);
    m.setSect   = GetM(m.kChar, "set_SectContribution", 1);
    m.getExp    = GetM(m.kChar, "get_Exp", 0);
    m.addExp    = GetM(m.kChar, "AddExp", 1);
    m.getLife   = GetM(m.kChar, "get_LifeTime", 0);
    m.addLife   = GetM(m.kChar, "AddLifeTime", 1);
    m.getMoneyInc = GetM(m.kChar, "get_MoneyIncSpeed", 0);
    m.setMoneyInc = GetM(m.kChar, "set_MoneyIncSpeed", 1);
    m.getExpInc   = GetM(m.kChar, "get_ExpIncSpeed", 0);
    m.setExpInc   = GetM(m.kChar, "set_ExpIncSpeed", 1);
    m.getLifeInc  = GetM(m.kChar, "get_LifeTimeIncSpeed", 0);
    m.setLifeInc  = GetM(m.kChar, "set_LifeTimeIncSpeed", 1);

    // 指纹自检：字段偏移从访问器反推（形态不符或漂移都会打日志）
    if (m.getPlayer)  g_offPlayer  = DeriveImmOff(m.getPlayer,  kOpLdrX0, "GameMgr.Player", kOffGameMgrPlayer);
    if (m.getMoney)   g_offMoney   = DeriveImmOff(m.getMoney,   kOpLdrD0, "Money",   kOffMoney);
    if (m.getRenown)  g_offRenown  = DeriveImmOff(m.getRenown,  kOpLdrD0, "Renown",  kOffRenown);
    if (m.getSect)    g_offSect    = DeriveImmOff(m.getSect,    kOpLdrD0, "SectContribution", kOffSectContribution);
    if (m.getExp)     g_offExp     = DeriveImmOff(m.getExp,     kOpLdrD0, "Exp",     kOffExp);
    if (m.getLife)    g_offLife    = DeriveImmOff(m.getLife,    kOpLdrD0, "LifeTime", kOffLifeTime);
    if (m.getMoneyInc) g_offMoneyInc = DeriveImmOff(m.getMoneyInc, kOpLdrD0, "MoneyIncSpeed", kOffMoneyIncSpeed);
    if (m.getExpInc)  g_offExpInc  = DeriveImmOff(m.getExpInc,  kOpLdrD0, "ExpIncSpeed", kOffExpIncSpeed);
    if (m.getLifeInc) g_offLifeInc = DeriveImmOff(m.getLifeInc, kOpLdrD0, "LifeTimeIncSpeed", kOffLifeTimeIncSpeed);

    LogSig("set_Money", m.setMoney);
    LogSig("AddExp", m.addExp);
    LogSig("AddLifeTime", m.addLife);
    LogSig("set_MoneyIncSpeed", m.setMoneyInc);

    m.ok = (m.setMoney && m.addExp && m.addLife);
    g_m = m;
    g_resolveState.store(m.ok ? 1 : 2);
    LOGI(TPL_LOG " resolve %s: char=%p gameMgr=%p player=%p 偏移[Money=0x%zX Renown=0x%zX Sect=0x%zX Exp=0x%zX Life=0x%zX MonInc=0x%zX ExpInc=0x%zX LifeInc=0x%zX]",
         m.ok ? "ok" : "partial", m.kChar, m.kGameMgr, m.getPlayer,
         g_offMoney, g_offRenown, g_offSect, g_offExp, g_offLife,
         g_offMoneyInc, g_offExpInc, g_offLifeInc);
    return m.ok;
}

// 调 void 方法（0 或 1 个 double 参数）；返回是否无异常
static bool Invoke0(void* mi, void* obj) {
    auto& api = GetApi();
    if (!mi || !api.runtime_invoke) return false;
    void* exc = nullptr;
    api.runtime_invoke(mi, obj, nullptr, &exc);
    if (exc) { LOGW(TPL_LOG " invoke exc mi=%p obj=%p", mi, obj); return false; }
    return true;
}

static bool Invoke1D(void* mi, void* obj, double v) {
    auto& api = GetApi();
    if (!mi || !api.runtime_invoke) return false;
    void* exc = nullptr;
    double arg = v;
    void* argv[1] = { &arg };
    api.runtime_invoke(mi, obj, argv, &exc);
    if (exc) { LOGW(TPL_LOG " invoke exc mi=%p obj=%p v=%.2f", mi, obj, v); return false; }
    return true;
}

// ============================================================================
//  ⑦ 动作执行（全部在 GameMgr::Update 的游戏线程内跑）
// ============================================================================
static void DoAdd(const char* name, void* obj, void* miSet, size_t off, double delta) {
    double before = RdF64(obj, off);
    bool ok = Invoke1D(miSet, obj, before + delta);
    double after = RdF64(obj, off);
    LOGI(TPL_LOG " %s +%.0f before=%.0f after=%.0f ok=%d", name, delta, before, after, ok ? 1 : 0);
}

static void DoAddRaw(const char* name, void* obj, void* miAdd, size_t off, double delta) {
    double before = RdF64(obj, off);
    bool ok = Invoke1D(miAdd, obj, delta);
    double after = RdF64(obj, off);
    LOGI(TPL_LOG " %s add=%.0f before=%.0f after=%.0f ok=%d", name, delta, before, after, ok ? 1 : 0);
}

struct IncSlot {
    std::atomic<bool>* on;
    const char* name;
    void* miGet;
    void* miSet;
    size_t off;
    bool applied;
    double orig;
    bool forcePositive;   // ★寿元：该字段是「寿元自增速度」，原值 -1 = 每秒扣 1（流逝）。
                          //   直接 ×倍率 会变成 -1000（扣得更快 ⇒ 当场寿元见底暴死，
                          //   2026-11-02 真机实证）。所以本槽一律取绝对值放大 ⇒ 正数 = 寿元增长。
};

static IncSlot g_incSlots[3] = {
    { &g_incMoneyOn, "MoneyIncSpeed",    nullptr, nullptr, kOffMoneyIncSpeed,    false, 0.0, false },
    { &g_incExpOn,   "ExpIncSpeed",      nullptr, nullptr, kOffExpIncSpeed,      false, 0.0, false },
    { &g_incLifeOn,  "LifeTimeIncSpeed", nullptr, nullptr, kOffLifeTimeIncSpeed, false, 0.0, true  },
};

static void SyncIncSlot(IncSlot& s, void* player) {
    bool want = s.on->load();
    if (want && !s.applied) {
        if (!s.miSet) return;
        double save = RdF64(player, s.off);
        double mult = static_cast<double>(g_incMult.load());
        if (mult < 1.0) mult = 1.0;
        double base = save;
        if (s.forcePositive && base < 0.0) base = -base;   // -1 ⇒ +1 ⇒ +倍率
        double target = base * mult;
        if (target == 0.0) target = mult;
        if (target > 1000000.0) target = 1000000.0;
        if (target < -1000000.0) target = -1000000.0;
        if (Invoke1D(s.miSet, player, target)) {
            s.orig = save;
            s.applied = true;
            LOGI(TPL_LOG " %s ON orig=%.2f -> x%.0f = %.2f (now %.2f)%s",
                 s.name, save, mult, target, RdF64(player, s.off),
                 s.forcePositive ? " [取正:本字段为流逝速度]" : "");
        }
    } else if (!want && s.applied) {
        if (!s.miSet) return;
        if (Invoke1D(s.miSet, player, s.orig)) {
            s.applied = false;
            LOGI(TPL_LOG " %s OFF restore=%.2f now=%.2f",
                 s.name, s.orig, RdF64(player, s.off));
        }
    }
}

static void RunPendingOnGameThread() {
    void* player = g_player.load();
    if (!OkPtr(player)) return;
    if (!ResolveMethods()) return;

    uint32_t pend = g_pending.exchange(0, std::memory_order_acq_rel);
    if (pend) {
        g_inAction.store(true);
        double amt = static_cast<double>(g_amount.load());
        if (pend & kActMoney)  DoAdd("Money", player, g_m.setMoney, g_offMoney, amt);
        if (pend & kActRenown) DoAdd("Renown", player, g_m.setRenown, g_offRenown, amt);
        if (pend & kActSect)   DoAdd("SectContribution", player, g_m.setSect, g_offSect, amt);
        if (pend & kActExp)    DoAddRaw("Exp", player, g_m.addExp, g_offExp, amt);
        if (pend & kActLife)   DoAddRaw("LifeTime", player, g_m.addLife, g_offLife, amt);
        g_inAction.store(false);
    }

    if (g_incSlots[0].miSet == nullptr) {
        // 方法表刚就绪时补一次句柄
        g_incSlots[0].miSet = g_m.setMoneyInc;
        g_incSlots[1].miSet = g_m.setExpInc;
        g_incSlots[2].miSet = g_m.setLifeInc;
        g_incSlots[0].off = g_offMoneyInc;
        g_incSlots[1].off = g_offExpInc;
        g_incSlots[2].off = g_offLifeInc;
    }

    if (g_inAction.load()) return;
    g_inAction.store(true);
    for (int i = 0; i < 3; ++i) SyncIncSlot(g_incSlots[i], player);
    g_inAction.store(false);
}

// ============================================================================
//  ⑧ hook 函数与挂点表
//    唯一槽：Immortal.GameMgr::Update(0)——对照方同址（RVA 0xF6F788）。
//    返回签名：void (this, const MethodInfo*)。
// ============================================================================
typedef void (*GameMgrUpdate_t)(void* self, void* method);
static GameMgrUpdate_t g_origUpdate = nullptr;

static void Hooked_GameMgrUpdate(void* self, void* method) {
    if (g_origUpdate) g_origUpdate(self, method);

    if (!OkPtr(self)) return;
    g_gameMgr.store(self);
    void* p = Il2Cpp::ReadPtr(self, g_offPlayer);
    g_player.store(OkPtr(p) ? p : nullptr);

    uint64_t n = g_updHits.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n == 1 || n == 600 || (n % 3600) == 0) {
        LOGI(TPL_LOG " update hit=%llu mgr=%p player=%p",
             (unsigned long long)n, self, g_player.load());
    }
    if (g_inAction.load()) return;
    RunPendingOnGameThread();
}

struct HookSpec {
    const char* tag;
    uintptr_t   rva;
    uint32_t    expectInsn;
    const char* ns;
    const char* cls;
    const char* method;
    int         argc;
};

static const HookSpec kUpdSpec = {
    "GameMgr::Update", kRvaGameMgrUpdate, kInsnGameMgrUpdate,
    "Immortal", "GameMgr", "Update", 0,
};

static bool HookByName(const HookSpec& s, void* hook, void** orig) {
    if (!EnsureApi()) return false;
    void* k = FindClassNs(s.ns, s.cls);
    if (!k) k = GetApi().FindClassByShortName(s.cls);
    if (!k) { LOGW(TPL_LOG " %s class miss", s.tag); return false; }
    void* mi = GetApi().GetMethod(k, s.method, s.argc);
    if (!mi) { LOGW(TPL_LOG " %s method miss", s.tag); return false; }
    void* fn = MethodPtr(mi);
    if (!fn) { LOGW(TPL_LOG " %s bad method ptr", s.tag); return false; }
    if (!DobbyHookGuarded(s.tag, fn, hook, orig)) return false;
    LOGI(TPL_LOG " %s hooked by-name %s.%s.%s(argc=%d)",
         s.tag, s.ns, s.cls, s.method, s.argc);
    return true;
}

static int InstallUpdateHook() {
    if (g_updHooked.load()) return 1;
    std::lock_guard<std::mutex> lk(g_installMutex);
    if (g_updHooked.load()) return 1;
    if (g_origUpdate) { g_updHooked.store(true); return 1; }

    g_lastFatal = false;
    bool ok = HookAtRva(kUpdSpec.tag, kUpdSpec.rva, kUpdSpec.expectInsn,
                        (void*)Hooked_GameMgrUpdate, (void**)&g_origUpdate);
    if (!ok && !g_lastFatal) ok = HookByName(kUpdSpec, (void*)Hooked_GameMgrUpdate,
                                            (void**)&g_origUpdate);
    if (ok) {
        g_updHooked.store(true);
        g_installedCount.store(1);
    }
    LOGI(TPL_LOG " install update-hook ok=%d (rva 0x%lX)",
         ok ? 1 : 0, (unsigned long)kUpdSpec.rva);
    return ok ? 1 : 0;
}

// ---- ⑧.2 免广告：Immortal.GeneralInfoLogic::JedgeCanBlockAd @0xF7A5C0（序言 0xA9BE4FF4）----
//  证据（capstone 反汇编 ADMgr::ShowRewardVedio）：
//    0xE7B5C8  bl GeneralInfoLogic::Instance   → 0xE7B5D0  instance
//    0xE7B5D4  bl JedgeCanBlockAd              → 返回值 w0 只看 bit0
//    0xE7B5D8  tbz w0,#0, 0xE7B608            → false 才走去播广告的建监听分支
//    true 分支 0xE7B5DC→b 0x15b46c8           → 直接派发奖励（跳过播放）
//  ⇒ 这就是游戏自带的「去广告（JedgeCanBlock = 判断能否屏蔽广告）」特权开关：
//     让它恒真 = 游戏自己认为已有去广告特权，奖励视频/广告宝箱/广告装备全部直接发奖。
//  另：ShowInterAd @0xE7B734 本身就是 `ret`（空函数），插屏无需处理。
typedef bool (*BoolGetter_t)(void* self, void* method);
static BoolGetter_t g_origCanBlockAd = nullptr;

static bool Hooked_JedgeCanBlockAd(void* self, void* method) {
    uint64_t n = g_adHits.fetch_add(1, std::memory_order_relaxed) + 1;
    if (g_noAd.load(std::memory_order_relaxed)) {
        if (n <= 3 || (n % 200) == 0) {
            LOGI(TPL_LOG " JedgeCanBlockAd -> true (免广告, hits=%llu)", (unsigned long long)n);
        }
        return true;
    }
    if (n <= 3) LOGI(TPL_LOG " JedgeCanBlockAd -> orig (hits=%llu)", (unsigned long long)n);
    return g_origCanBlockAd ? g_origCanBlockAd(self, method) : false;
}

static const uintptr_t kRvaJedgeCanBlockAd  = 0xF7A5C0;
static const uint32_t  kInsnJedgeCanBlockAd = 0xA9BE4FF4u;  // stp x20, x19, [sp, #-0x20]!

static int InstallAdHook() {
    if (g_adHooked.load()) return 1;
    std::lock_guard<std::mutex> lk(g_installMutex);
    if (g_adHooked.load()) return 1;
    if (g_origCanBlockAd) { g_adHooked.store(true); return 1; }

    g_lastFatal = false;
    bool ok = HookAtRva("JedgeCanBlockAd", kRvaJedgeCanBlockAd, kInsnJedgeCanBlockAd,
                        (void*)Hooked_JedgeCanBlockAd, (void**)&g_origCanBlockAd);
    if (!ok && !g_lastFatal) {
        HookSpec spec = {"JedgeCanBlockAd", 0, 0, "Immortal", "GeneralInfoLogic",
                         "JedgeCanBlockAd", 0};
        ok = HookByName(spec, (void*)Hooked_JedgeCanBlockAd, (void**)&g_origCanBlockAd);
    }
    if (ok) g_adHooked.store(true);
    LOGI(TPL_LOG " install ad-hook ok=%d (rva 0x%lX)", ok ? 1 : 0,
         (unsigned long)kRvaJedgeCanBlockAd);
    return ok ? 1 : 0;
}

// ============================================================================
//  ⑨ 对外接口（ModEntry.h 注册表契约，签名不可改）
// ============================================================================
bool Byyxx2_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;

    InstallFaultGuard();
    g_libReady.store(true);
    LOGI(TPL_LOG " lib ready (%s)", TPL_TARGET_LIB);

    InstallUpdateHook();   // 与对照方一致：常驻一帧钩，用来取实例
    InstallAdHook();       // 免广告：JedgeCanBlockAd 恒真的门闩（关着时行为与原版一致）
    ResolveMethods();      // 运行时可能尚未就绪，失败留给 Tick 重试
    return true;
}

bool Byyxx2_IsReady() {
    return g_libReady.load() && g_updHooked.load();
}

static void RequestAction(uint32_t bit) {
    // 钩子没装好（库还没起）也要尽量装上，否则按钮永远不生效
    if (!g_updHooked.load()) InstallUpdateHook();
    g_pending.fetch_or(bit, std::memory_order_acq_rel);
    LOGI(TPL_LOG " request act=0x%X amount=%lld", bit, (long long)g_amount.load());
}

void Byyxx2_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case BYYXX2_FEAT_MONEY:    if (enabled) RequestAction(kActMoney);  break;
        case BYYXX2_FEAT_RENOWN:   if (enabled) RequestAction(kActRenown); break;
        case BYYXX2_FEAT_SECT:     if (enabled) RequestAction(kActSect);   break;
        case BYYXX2_FEAT_EXP:      if (enabled) RequestAction(kActExp);    break;
        case BYYXX2_FEAT_LIFETIME: if (enabled) RequestAction(kActLife);   break;
        case BYYXX2_FEAT_ALL:
            if (enabled) RequestAction(kActMoney | kActRenown | kActSect | kActExp | kActLife);
            break;
        case BYYXX2_FEAT_INC_MONEY: g_incMoneyOn.store(enabled); break;
        case BYYXX2_FEAT_INC_EXP:   g_incExpOn.store(enabled);   break;
        case BYYXX2_FEAT_INC_LIFE:  g_incLifeOn.store(enabled);  break;
        case BYYXX2_FEAT_NOAD:
            g_noAd.store(enabled);
            if (enabled && !g_adHooked.load()) InstallAdHook();
            LOGI(TPL_LOG " 免广告 = %d (hooked=%d)", enabled ? 1 : 0, g_adHooked.load() ? 1 : 0);
            break;
        default: break;
    }
}

void Byyxx2_OnValueChanged(int featNum, int value) {
    if (featNum == BYYXX2_FEAT_AMOUNT && value > 0) g_amount.store(value);
    if (featNum == BYYXX2_FEAT_INC_MULT && value > 0) g_incMult.store(value);
}

void Byyxx2_OnInputChanged(int featNum, const char* text) {
    if (!text || !*text) return;
    long long v = std::strtoll(text, nullptr, 10);
    if (v <= 0) return;
    if (featNum == BYYXX2_FEAT_AMOUNT) {
        if (v > 100000000000000LL) v = 100000000000000LL;
        g_amount.store(v);
        LOGI(TPL_LOG " amount=%lld", v);
    } else if (featNum == BYYXX2_FEAT_INC_MULT) {
        if (v > 1000000LL) v = 1000000LL;
        g_incMult.store(v);
        LOGI(TPL_LOG " incMult=%lld", v);
    }
}

void Byyxx2_Tick(const bool* features, const int* /*values*/) {
    if (features) {
        g_incMoneyOn.store(features[BYYXX2_FEAT_INC_MONEY]);
        g_incExpOn.store(features[BYYXX2_FEAT_INC_EXP]);
        g_incLifeOn.store(features[BYYXX2_FEAT_INC_LIFE]);
        g_noAd.store(features[BYYXX2_FEAT_NOAD]);
    }

    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) {
        InstallFaultGuard();
        g_libReady.store(true);
        LOGI(TPL_LOG " lib ready (via Tick fallback)");
    }
    if (!g_updHooked.load() && g_libReady.load()) InstallUpdateHook();
    if (!g_adHooked.load() && g_libReady.load()) InstallAdHook();
    if (g_resolveState.load() != 1) ResolveMethods();

    // 禁在此扫堆/遍历对象（Tick 频率高）
}

void Byyxx2_ForceClear() {
    g_pending.store(0);
    g_incMoneyOn.store(false);
    g_incExpOn.store(false);
    g_incLifeOn.store(false);
    g_noAd.store(false);
    for (int i = 0; i < 3; ++i) g_incSlots[i].applied = false;
    LOGI(TPL_LOG " force clear");
}
