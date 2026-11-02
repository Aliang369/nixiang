// ============================================================================
//  不一样西游（com.wingjoy.toWest 1.0.40 / vc622）— 星光菜单游戏模块
//
//  证据（2026-11-02，官方包 `libil2cpp.so` sha 前16 `44c037a9c0a91379` /
//  `global-metadata.dat` 前16 `8496c5ddc51a25fe`，与对照方「斌哥改包」**逐字节一致**；
//  DumpRva 反查 + capstone 反汇编）：
//    · ToWest.Logic.CharacterLogic::SetAttr(int type, int delta, bool isAdd)  @0xE0CC30
//        反汇编实证：`tbz w3,#0` 分支 —— isAdd=true 时 `w2 = 当前值 + delta`（累加），
//        false 时直接赋值；写完尾部两次虚调用刷新（vtable +0x1b8 / +0x1c8）。
//        ⇒ 与对照方日志 `SetAttr t=0 +99999`（before 199998 → after 299997）语义吻合。
//    · ToWest.Logic.CharacterLogic::GetAttr(int type) -> int                  @0xE0CF18
//        argc=1（对照方日志里的 `lock=1` 是它自己 hook GetAttr 的旁路开关，不是参数）。
//    · ToWest.Logic.PlayerLogic::SetAttr  @0x1066578 —— 派生 override，主体独立实现；
//        默认走基类（与对照方同路），基类调用无效时才回落调用派生版。
//    · 实例：ToWest.Mgr.GameMgr::get_Player = `ldr x0,[x0,#0x28]; ret` @0x10469EC
//        ⇒ 钩它取玩家（对照方也钩了这一处，日志 `xy hooks getPlayer`）。
//    · 属性槽：0..3 = 菜单四项（愿/体/缘/毅，顺序按对照方菜单为推定，见路线锁定）；
//        4 号槽恒为 100 未被对照方触碰 ⇒ 我们也不碰。
//
//  防闪退设施沿用骨架 v3.1（懒装 / 双路径挂点 / RVA 漂移拒绝 / 幂等安装 /
//  致命信号保护区 / 槽位熔断）。
// ============================================================================

#include "BysxyMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstdlib>
#include <cstring>

#include <mutex>
#include <setjmp.h>
#include <signal.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define TPL_LOG "[Bysxy]"
#define TPL_TARGET_LIB sec_lib_il2cpp()

// ============================================================================
//  ① 开关 / 实例 / 待执行动作
// ============================================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_updHooked{false};
static std::atomic<bool> g_inAction{false};

static std::atomic<void*>     g_gameMgr{nullptr};
static std::atomic<void*>     g_player{nullptr};
static std::atomic<uint64_t>  g_hookHits{0};
static std::atomic<uint32_t>  g_pending{0};
static std::atomic<int64_t>   g_amount{99999};

enum : uint32_t {
    kActOne = 1u << 0,   // 单槽（槽号由 g_pendingType 指定）
    kActAll = 1u << 1,   // 0..3 全加
};
static std::atomic<int> g_pendingType{0};

static const int kAttrTypes = 4;       // 0..3（4 号槽不动）
static const char* kAttrNames[kAttrTypes] = { "愿(t0)", "体(t1)", "缘(t2)", "毅(t3)" };

// ============================================================================
//  ② 字段偏移与 RVA（换包必须重 dump；运行期用访问器首指令反推校验）
// ============================================================================
enum : size_t {
    kOffGameMgrPlayer = 0x28,   // GameMgr::get_Player = ldr x0,[x0,#0x28]
};

// 实例捕获主锚点：ToWest.Mgr.GameMgr::OnApplicationFocus(bool) @0x1046BDC
//   实证：Unity 主线程日志里真实出现 `ToWest.Mgr.GameMgr:OnApplicationFocus(Boolean)`；
//   反汇编 `sub sp,sp,#0x110` + `mov w19,w1` ⇒ 实例方法、1 个托管实参(this, bool)。
//   ★ 弃用 CharacterLogic::GetAttr @0xE0CF18：真机实测钩子收到的 x0=0x1、x1=垃圾值
//     （`GetAttr first hit self=0x1 type=-251321024`），据此捕获必然失败。
// 属性上限：CharacterLogic::JudgeAttrHasLimit(int type) @0xE0CEF4
//   反汇编：`cmp w1,#2; cset w0,eq; ret` ⇒ **只有 type==2（体力槽）有上限**
//   实测：t2 写 +99999 后恒为 100，减 50 能到 50 ⇒ 值可写、只是被上限夹住。
static const uintptr_t kRvaJudgeLimit = 0xE0CEF4;
static const uint32_t  kInsnJudgeLimit = 0x7100083Fu; // cmp w1, #2
static const uintptr_t kRvaShowAds   = 0x2AD8664;  // WingjoyMultiAdRuntime::ShowAds —— 激励视频入口
static const uintptr_t kRvaMgrInit   = 0x1046A10;   // GameMgr::Init —— 启动必调，用来拿 GameMgr 实例
static const uint32_t  kInsnMgrInit  = 0xD103C3FFu; // sub sp, sp, #0xf0
static const uintptr_t kRvaMgrFocus  = 0x1046BDC;
static const uint32_t  kInsnMgrFocus = 0xD10443FFu; // sub sp, sp, #0x110
static const uintptr_t kRvaGetPlayer  = 0x10469EC;
static const uint32_t  kInsnGetPlayer = 0xF9401400u;  // ldr x0, [x0, #0x28]

static const uint32_t kOpLdrX0 = 0xF9400000u;   // 掩码 0xFFC00000
static const uint32_t kInsnRet = 0xD65F03C0u;

// ============================================================================
//  ③ 安全助手（防闪退核心；新代码禁止绕开它们裸读写内存）
// ============================================================================
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

static size_t g_offPlayer = kOffGameMgrPlayer;

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

static void BysxyFaultHandler(int sig, siginfo_t* si, void* /*uc*/) {
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
    if (old->sa_sigaction && old->sa_sigaction != BysxyFaultHandler) {
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
    sa.sa_sigaction = BysxyFaultHandler;
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

static const int kSlotsMax = 8;
static bool g_slotDead[kSlotsMax];

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
//  ⑥ IL2CPP 方法表（按名解析；runtime_invoke 调用）
// ============================================================================
struct MethodSet {
    void* kChar = nullptr;        // ToWest.Logic.CharacterLogic
    void* kPlayer = nullptr;      // ToWest.Logic.PlayerLogic（可选）
    void* kGameMgr = nullptr;
    void* getPlayer = nullptr;    // argc 0 -> CharacterLogic*
    void* getAttr = nullptr;      // argc 1 (int type) -> int
    void* getAttrLimit = nullptr; // argc 1 -> int
    void* setAttrBase = nullptr;  // argc 3 (int type, int delta, bool isAdd)
    void* setAttrDerived = nullptr;
    bool ok = false;
};
static MethodSet g_m;
static std::atomic<int> g_resolveState{0};
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
    m.kChar = FindClassNs("ToWest.Logic", "CharacterLogic");
    if (!m.kChar) m.kChar = GetApi().FindClassByShortName("CharacterLogic");
    m.kPlayer = FindClassNs("ToWest.Logic", "PlayerLogic");
    if (!m.kPlayer) m.kPlayer = GetApi().FindClassByShortName("PlayerLogic");
    m.kGameMgr = FindClassNs("ToWest.Mgr", "GameMgr");
    if (!m.kGameMgr) m.kGameMgr = GetApi().FindClassByShortName("GameMgr");

    if (!m.kChar || !m.kGameMgr) {
        LOGW(TPL_LOG " resolve: class miss char=%p gameMgr=%p — retry later",
             m.kChar, m.kGameMgr);
        g_resolveState.store(2);
        return false;
    }

    m.getPlayer    = GetM(m.kGameMgr, "get_Player", 0);
    m.getAttr      = GetM(m.kChar, "GetAttr", 1);
    m.getAttrLimit = GetM(m.kChar, "GetAttrLimitValue", 1);
    m.setAttrBase  = GetM(m.kChar, "SetAttr", 3);
    m.setAttrDerived = m.kPlayer ? GetM(m.kPlayer, "SetAttr", 3) : nullptr;

    if (m.getPlayer) g_offPlayer = DeriveImmOff(m.getPlayer, kOpLdrX0, "GameMgr.Player", kOffGameMgrPlayer);

    LogSig("SetAttr(base)", m.setAttrBase);
    LogSig("SetAttr(derived)", m.setAttrDerived);
    LogSig("GetAttr", m.getAttr);

    m.ok = (m.setAttrBase != nullptr && m.getPlayer != nullptr);
    g_m = m;
    g_resolveState.store(m.ok ? 1 : 2);
    LOGI(TPL_LOG " resolve %s: char=%p player=%p gameMgr=%p getPlayer=%p getAttr=%p setAttr=%p/%p offPlayer=0x%zX",
         m.ok ? "ok" : "partial", m.kChar, m.kPlayer, m.kGameMgr,
         m.getPlayer, m.getAttr, m.setAttrBase, m.setAttrDerived, g_offPlayer);
    return m.ok;
}

// ---- 调用助手 ----
static int ReadBoxedInt32(void* boxed, int defv) {
    // Il2CppObject { Il2CppClass* klass; void* monitor; } → 值在 +0x10
    if (!OkPtr(boxed)) return defv;
    return *reinterpret_cast<int*>(reinterpret_cast<char*>(boxed) + 0x10);
}

static bool InvokeGetAttr(void* obj, int type, int* out) {
    auto& api = GetApi();
    if (!g_m.getAttr || !api.runtime_invoke) return false;
    void* exc = nullptr;
    int t = type;
    void* argv[1] = { &t };
    void* ret = api.runtime_invoke(g_m.getAttr, obj, argv, &exc);
    if (exc || !ret) {
        if (exc) LOGW(TPL_LOG " GetAttr exc type=%d", type);
        return false;
    }
    *out = ReadBoxedInt32(ret, 0);
    return true;
}

static bool InvokeSetAttr(void* mi, void* obj, int type, int delta, bool isAdd) {
    auto& api = GetApi();
    if (!mi || !api.runtime_invoke) return false;
    void* exc = nullptr;
    int t = type, d = delta;
    bool a = isAdd;
    void* argv[3] = { &t, &d, &a };
    api.runtime_invoke(mi, obj, argv, &exc);
    if (exc) { LOGW(TPL_LOG " SetAttr exc type=%d", type); return false; }
    return true;
}

// 前向声明：⑦ 里要用 ⑧.2 的玩家解析
static void* EnsurePlayer();
static void ForceRefresh(void* player, int type);
static void ProbeWritable(void* player, int type);

// ============================================================================
//  ⑦ 动作执行（在游戏线程的钩子内跑：OnApplicationFocus / get_Player）
// ============================================================================
static void ApplyOne(void* player, int type) {
    if (type < 0 || type >= 16) return;
    const char* nm = (type < kAttrTypes) ? kAttrNames[type] : (type == 4 ? "体力上限(t4)" : "t?");
    int before = 0, after = 0, limit = -1;
    bool haveBefore = InvokeGetAttr(player, type, &before);
    if (g_m.getAttrLimit) { int lim = 0; if (InvokeGetAttr(player, type, &lim)) limit = lim; }

    int amount = (int)g_amount.load();
    if (amount <= 0) amount = 99999;
    bool ok = InvokeSetAttr(g_m.setAttrBase, player, type, amount, true);
    bool got = InvokeGetAttr(player, type, &after);

    if (ok && haveBefore && got && after == before) {
        // 基类调用未生效 → 回落派生 override（PlayerLogic::SetAttr 主体独立实现）
        if (g_m.setAttrDerived && g_m.setAttrDerived != g_m.setAttrBase) {
            LOGW(TPL_LOG " %s base 无效(before=%d after=%d) → 试 PlayerLogic::SetAttr", nm, before, after);
            ok = InvokeSetAttr(g_m.setAttrDerived, player, type, amount, true);
            got = InvokeGetAttr(player, type, &after);
        }
    }
    LOGI(TPL_LOG " %s +%d before=%d after=%d delta=%+d ok=%d",
         nm, amount, before, after, after - before, ok ? 1 : 0);
    if (ok && after != before) ForceRefresh(player, type);
    if (ok && after == before) {
        static bool probed[16] = {false};
        if (type >= 0 && type < 16 && !probed[type]) { probed[type] = true; ProbeWritable(player, type); }
    }
    // 加法被夹住时，试绝对赋值路径（isAdd=false）：SetAttr 反汇编里 true/false 是两条分支，
    // 上限夹取只出现在其中一条（2026-11-02 实测 t2 加法恒被夹到 100）。
    if (ok && after == before && haveBefore) {
        int target = before + amount;
        bool ok2 = InvokeSetAttr(g_m.setAttrBase, player, type, target, false);
        int a2v = 0;
        bool got2 = InvokeGetAttr(player, type, &a2v);
        LOGI(TPL_LOG " %s 绝对赋值 %d => after=%d ok=%d", nm, target, a2v, ok2 ? 1 : 0);
        if (ok2 && got2 && a2v != before) { after = a2v; ForceRefresh(player, type); }
    }
    (void)limit;
}

// 属性槽探针：把所有槽位的当前值打出来，便于和界面对齐（不改任何值）
static void ProbeAttrs(void* player) {
    char buf[320];
    int n = std::snprintf(buf, sizeof(buf), "attrs:");
    for (int i = 0; i <= 12 && n > 0 && n < (int)sizeof(buf) - 12; ++i) {
        int v = 0;
        if (InvokeGetAttr(player, i, &v)) n += std::snprintf(buf + n, sizeof(buf) - n, " t%d=%d", i, v);
    }
    LOGI(TPL_LOG " %s", buf);
}

// 写完再以 delta=0 调一次，促使游戏自己的刷新虚调用再跑一遍（不改数值）
static void ForceRefresh(void* player, int type) {
    InvokeSetAttr(g_m.setAttrBase, player, type, 0, true);
}

static void RunPendingOnGameThread() {
    if (!g_pending.load(std::memory_order_acquire)) return;
    void* player = EnsurePlayer();
    if (!OkPtr(player)) return;
    if (!ResolveMethods()) return;

    uint32_t pend = g_pending.exchange(0, std::memory_order_acq_rel);
    if (!pend) return;

    g_inAction.store(true);
    if (pend & kActOne) {
        ApplyOne(player, g_pendingType.load());
    }
    if (pend & kActAll) {
        for (int t = 0; t < kAttrTypes; ++t) ApplyOne(player, t);
    }
    g_inAction.store(false);
}

// ============================================================================
//  ⑧ 挂点：ToWest.Mgr.GameMgr::get_Player（对照方同址）
//     签名 void* (this, const MethodInfo*)
// ============================================================================
typedef void* (*GetPlayer_t)(void* self, void* method);
static GetPlayer_t g_origGetPlayer = nullptr;

static void* Hooked_GetPlayer(void* self, void* method) {
    void* p = g_origGetPlayer ? g_origGetPlayer(self, method) : nullptr;
    if (OkPtr(p)) {
        g_gameMgr.store(self);
        g_player.store(p);
    }
    uint64_t n = g_hookHits.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n == 1 || n == 100 || (n % 3600) == 0) {
        LOGI(TPL_LOG " get_Player hit=%llu mgr=%p player=%p",
             (unsigned long long)n, self, p);
    }
    if (!g_inAction.load()) RunPendingOnGameThread();
    return p;
}

// （已删除 HookSpec / HookByName：by-name 兜底需调 il2cpp API，
//   TryInit 阶段调用会 SIGABRT；本作只保留 RVA 挂点。）

static int InstallPlayerHook() {
    if (g_updHooked.load()) return 1;
    std::lock_guard<std::mutex> lk(g_installMutex);
    if (g_updHooked.load()) return 1;
    if (g_origGetPlayer) { g_updHooked.store(true); return 1; }

    g_lastFatal = false;
    // ★ 只走 RVA：本函数在 TryInit（il2cpp 尚未 init 完）就会被调用，
    //   而 by-name 兜底要调 il2cpp_class_from_name/Assemblies ⇒ **提前调必 SIGABRT**
    //   （2026-11-02 本作实测：装钩后 0.8s 进程 abort+160，backtrace 只有 libc abort；
    //    同型事故见 Games/CxswzMod.cpp 注释「在 il2cpp_init 之前把进程带崩」）。
    bool ok = HookAtRva("GameMgr::get_Player", kRvaGetPlayer, kInsnGetPlayer,
                        (void*)Hooked_GetPlayer, (void**)&g_origGetPlayer);
    if (!ok) {
        LOGW(TPL_LOG " player-hook RVA 0x%lX 未命中（版本漂移？）— 不在此处按名兜底（会提前碰 il2cpp）",
             (unsigned long)kRvaGetPlayer);
    }
    if (ok) g_updHooked.store(true);
    LOGI(TPL_LOG " install player-hook ok=%d (rva 0x%lX)", ok ? 1 : 0,
         (unsigned long)kRvaGetPlayer);
    return ok ? 1 : 0;
}

// ---- ⑧.2 实例捕获：ToWest.Mgr.GameMgr::OnApplicationFocus(bool) @0x1046BDC ----
//  GameMgr 是单例式实例类（get_Player = `ldr x0,[x0,#0x28]; ret`），但它没有任何静态
//  get_Instance；get_Player 本身实测在游戏中也不被调用。唯一稳定的「拿到 this」入口就是
//  Unity 回调 OnApplicationFocus：它在切前后台/收放菜单浮层时由主线程调用。
typedef void (*Focus_t)(void* self, bool focus, void* method);
static Focus_t g_origFocus = nullptr;
static std::atomic<uint64_t> g_focusHits{0};
static std::atomic<bool> g_focusHooked{false};

// ---- 静态单例探测：不依赖任何钩子时机 ----
//  实测教训（2026-11-02）：GameMgr::get_Player / OnApplicationFocus / GameMgr::Init 三个钩子
//  在真机上都拿不到实例（get_Player 十分钟 0 命中；focus 0 命中；Init 未触发），
//  CharacterLogic::GetAttr 钩子收到的 x0=0x1/x1=垃圾（不能用来捕获）。
//  ⇒ 改为从类静态字段直接取单例（Api::StaticRefField），并做「同类型校验」防误取。
static void* TryStaticRef(const char* ns, const char* cls, const char* field) {
    auto& api = GetApi();
    if (!api.object_class) return nullptr;
    void* k = FindClassNs(ns, cls);
    if (!k) k = api.FindClassByShortName(cls);
    if (!k) return nullptr;
    void* v = api.StaticRefField(k, field);
    if (!v || !Il2Cpp::IsValidHeapPtr(v)) return nullptr;
    if (api.object_class(v) != k) return nullptr;   // 必须是同类实例，排除读到别的字段
    return v;
}

static void* FindSingletonByStatics(const char* ns, const char* cls) {
    static const char* kNames[] = {"Instance", "instance", "Inst", "inst", "Cur", "cur",
                                   "Current", "current", "Player", "player", "Main", "main"};
    for (const char* n : kNames) {
        void* v = TryStaticRef(ns, cls, n);
        if (v) {
            LOGI(TPL_LOG " static singleton hit %s::%s = %p", cls, n, v);
            return v;
        }
    }
    return nullptr;
}

// 诊断：把某个类的静态字段名/类型/值打出来（只在直接候选全失败时跑一次）
static void DumpStaticFields(const char* ns, const char* cls) {
    auto& api = GetApi();
    if (!api.class_fields || !api.field_static_get || !api.field_name) return;
    void* k = FindClassNs(ns, cls);
    if (!k) k = api.FindClassByShortName(cls);
    if (!k) { LOGW(TPL_LOG " dump %s: class miss", cls); return; }
    void* iter = nullptr;
    void* f = nullptr;
    int n = 0;
    while ((f = api.class_fields(k, &iter)) && n < 16) {
        const char* fn = api.field_name(f);
        const char* tn = "?";
        if (api.field_type && api.type_name) {
            void* ft = api.field_type(f);
            const char* raw = ft ? api.type_name(ft) : nullptr;
            if (raw) tn = raw;
        }
        void* val = nullptr;
        api.field_static_get(f, &val);
        LOGI(TPL_LOG " dump %s.%s : %s = %p", cls, fn ? fn : "?", tn, val);
        ++n;
    }
}

// 用 GameMgr 实例调 get_Player 取玩家对象（runtime_invoke，引用类型直接返回对象指针）
static bool g_staticDumped = false;

static void* EnsurePlayer() {
    void* cached = g_player.load();
    if (OkPtr(cached)) return cached;
    if (!ResolveMethods()) return nullptr;

    // ① 直接找 PlayerLogic 静态单例（最理想：拿到就是玩家本体）
    void* direct = FindSingletonByStatics("ToWest.Logic", "PlayerLogic");
    if (!direct) direct = FindSingletonByStatics("ToWest.Logic", "CharacterLogic");
    if (direct) { g_player.store(direct); return direct; }

    // ② GameMgr 静态单例 → get_Player
    void* mgr = g_gameMgr.load();
    if (!OkPtr(mgr)) {
        mgr = FindSingletonByStatics("ToWest.Mgr", "GameMgr");
        if (OkPtr(mgr)) g_gameMgr.store(mgr);
    }
    if (!OkPtr(mgr)) {
        if (!g_staticDumped) { g_staticDumped = true; DumpStaticFields("ToWest.Mgr", "GameMgr"); DumpStaticFields("ToWest.Logic", "PlayerLogic"); }
        return nullptr;
    }
    if (!g_m.getPlayer) return nullptr;
    void* exc = nullptr;
    void* ret = GetApi().runtime_invoke(g_m.getPlayer, mgr, nullptr, &exc);
    if (exc || !OkPtr(ret)) {
        if (exc) LOGW(TPL_LOG " get_Player exc（mgr=%p）", mgr);
        return nullptr;
    }
    g_player.store(ret);
    LOGI(TPL_LOG " player resolved via GameMgr.get_Player → %p", ret);
    return ret;
}

static void Hooked_Focus(void* self, bool focus, void* method) {
    if (OkPtr(self)) g_gameMgr.store(self);
    if (g_origFocus) g_origFocus(self, focus, method);

    uint64_t n = g_focusHits.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 3 || (n % 50) == 0) {
        LOGI(TPL_LOG " focus hit=%llu mgr=%p focus=%d", (unsigned long long)n, self, focus ? 1 : 0);
    }
    if (!g_inAction.load(std::memory_order_acquire)) RunPendingOnGameThread();
}

// ---- GameMgr::Init @0x1046A10：启动必调，一次性拿到 GameMgr 实例（无参实例方法） ----
typedef void (*Init_t)(void* self, void* method);
static Init_t g_origMgrInit = nullptr;
static std::atomic<bool> g_initHooked{false};

static void Hooked_MgrInit(void* self, void* method) {
    if (OkPtr(self)) {
        void* old = g_gameMgr.exchange(self);
        if (old != self) LOGI(TPL_LOG " captured GameMgr via Init self=%p", self);
    }
    if (g_origMgrInit) g_origMgrInit(self, method);
}

static int InstallMgrInitHook() {
    if (g_initHooked.load()) return 1;
    std::lock_guard<std::mutex> lk(g_installMutex);
    if (g_initHooked.load()) return 1;
    if (g_origMgrInit) { g_initHooked.store(true); return 1; }
    bool ok = HookAtRva("GameMgr::Init", kRvaMgrInit, kInsnMgrInit,
                        (void*)Hooked_MgrInit, (void**)&g_origMgrInit);
    if (ok) g_initHooked.store(true);
    LOGI(TPL_LOG " install mgr-init-hook ok=%d (rva 0x%lX)", ok ? 1 : 0, (unsigned long)kRvaMgrInit);
    return ok ? 1 : 0;
}

static int InstallFocusHook() {
    if (g_focusHooked.load()) return 1;
    std::lock_guard<std::mutex> lk(g_installMutex);
    if (g_focusHooked.load()) return 1;
    if (g_origFocus) { g_focusHooked.store(true); return 1; }
    bool ok = HookAtRva("GameMgr::OnApplicationFocus", kRvaMgrFocus, kInsnMgrFocus,
                        (void*)Hooked_Focus, (void**)&g_origFocus);
    if (ok) g_focusHooked.store(true);
    LOGI(TPL_LOG " install focus-hook ok=%d (rva 0x%lX)", ok ? 1 : 0, (unsigned long)kRvaMgrFocus);
    return ok ? 1 : 0;
}

// ---- 解除属性上限（体力槽 t2）----
//  默认开启：只对「点功能」生效，不改存档；关掉开关即恢复游戏原上限判断。
typedef bool (*JudgeLimit_t)(void* self, int type, void* method);
static JudgeLimit_t g_origJudgeLimit = nullptr;
static std::atomic<bool> g_noAttrLimit{true};
static std::atomic<bool> g_limitHooked{false};

static bool Hooked_JudgeLimit(void* self, int type, void* method) {
    if (g_noAttrLimit.load()) return false;          // 恒「无上限」
    return g_origJudgeLimit ? g_origJudgeLimit(self, type, method) : false;
}

static int InstallLimitHook() {
    if (g_limitHooked.load()) return 1;
    std::lock_guard<std::mutex> lk(g_installMutex);
    if (g_limitHooked.load()) return 1;
    if (g_origJudgeLimit) { g_limitHooked.store(true); return 1; }
    bool ok = HookAtRva("CharacterLogic::JudgeAttrHasLimit", kRvaJudgeLimit, kInsnJudgeLimit,
                        (void*)Hooked_JudgeLimit, (void**)&g_origJudgeLimit);
    if (ok) g_limitHooked.store(true);
    LOGI(TPL_LOG " install attr-limit-hook ok=%d (rva 0x%lX)", ok ? 1 : 0, (unsigned long)kRvaJudgeLimit);
    return ok ? 1 : 0;
}

// ---- 免广告（B 站轨：拦 ShowAds，直接把奖励回调叫起来，不播放视频）----
//  实证：WingjoyMultiAdConfig.Runtime.WingjoyMultiAdRuntime::ShowAds @0x2AD8664
//        反汇编 `mov x21,x1 / mov x23,x2 / mov x22,x3` ⇒ 3 个托管实参
//        （推测 (string 广告位, Action 成功回调, Action 失败回调) 之类）。
//  做法：开启时把「像委托」的实参（类名含 Action）的 Invoke 直接调一次，然后不播广告。
//       参数/回调形态每次打日志，若判定不准则自动退回播放原广告（绝不静默吞掉奖励）。
typedef void (*ShowAds_t)(void* self, void* a1, void* a2, void* a3, void* method);
static ShowAds_t g_origShowAds = nullptr;
static void* g_miShowAds = nullptr;
static std::atomic<bool> g_noAd{false};
static std::atomic<bool> g_showAdsHooked{false};
static std::atomic<uint64_t> g_showAdsHits{0};

static const char* KlassNameOf(void* obj) {
    auto& api = GetApi();
    if (!OkPtr(obj) || !api.object_class || !api.class_name) return "";
    void* k = api.object_class(obj);
    const char* n = k ? api.class_name(k) : nullptr;
    return n ? n : "";
}

static bool InvokeDelegateLike(void* del, const char* tag) {
    auto& api = GetApi();
    if (!OkPtr(del) || !api.object_class) return false;
    void* k = api.object_class(del);
    if (!k) return false;
    const char* kn = api.class_name ? api.class_name(k) : "";
    if (!kn || !std::strstr(kn, "Action")) { LOGI(TPL_LOG " 免广告: %s 非委托(%s)", tag, kn ? kn : "?"); return false; }
    void* exc = nullptr;
    void* mi0 = api.GetMethod(k, "Invoke", 0);
    void* mi1 = mi0 ? nullptr : api.GetMethod(k, "Invoke", 1);
    LOGI(TPL_LOG " 免广告: %s type=%s Invoke0=%p Invoke1=%p", tag, kn, mi0, mi1);
    if (mi0) { api.runtime_invoke(mi0, del, nullptr, &exc); LOGI(TPL_LOG " 免广告: %s Invoke() exc=%p", tag, exc); return exc == nullptr; }
    if (mi1) { int32_t one = 1; void* argv[1] = {&one}; api.runtime_invoke(mi1, del, argv, &exc); LOGI(TPL_LOG " 免广告: %s Invoke(1) exc=%p", tag, exc); return exc == nullptr; }
    return false;
}

static void Hooked_ShowAds(void* self, void* a1, void* a2, void* a3, void* method) {
    uint64_t n = g_showAdsHits.fetch_add(1, std::memory_order_relaxed) + 1;
    if (n <= 5) {
        LOGI(TPL_LOG " ShowAds hit=%llu self=%p a1=%p(%s) a2=%p(%s) a3=%p(%s)",
             (unsigned long long)n, self, a1, KlassNameOf(a1), a2, KlassNameOf(a2),
             a3, KlassNameOf(a3));
        LogSig("ShowAds", g_miShowAds);
    }
    if (g_noAd.load()) {
        // 实证签名（2026-11-02 真机 LogSig）：
        //   ShowAds argc=3: System.Action  System.Action  AdsPlatform.Runtime.RewardVideoPlayEvent
        //   ⇒ a1 = 成功回调（发奖），a2 = 失败回调，a3 = 事件枚举（值类型，可为 0）
        //   之前误调 a2 才导致界面弹「广告播放失败，请重试」。
        bool done = InvokeDelegateLike(a1, "a1(成功)");
        if (done) { LOGI(TPL_LOG " 免广告: 已调成功回调发奖，跳过广告播放"); return; }
        LOGW(TPL_LOG " 免广告: a1 不可调 → 本次照常播放（不放空奖励）");
    }
    if (g_origShowAds) g_origShowAds(self, a1, a2, a3, method);
}

static int InstallShowAdsHook() {
    if (g_showAdsHooked.load()) return 1;
    std::lock_guard<std::mutex> lk(g_installMutex);
    if (g_showAdsHooked.load()) return 1;
    if (g_origShowAds) { g_showAdsHooked.store(true); return 1; }
    bool ok = HookAtRva("WingjoyMultiAdRuntime::ShowAds", kRvaShowAds, 0 /*首指令非固定序言，不校验*/,
                        (void*)Hooked_ShowAds, (void**)&g_origShowAds);
    if (ok && EnsureApi()) {
        void* k = FindClassNs("WingjoyMultiAdConfig.Runtime", "WingjoyMultiAdRuntime");
        if (!k) k = GetApi().FindClassByShortName("WingjoyMultiAdRuntime");
        if (k) g_miShowAds = GetApi().GetMethod(k, "ShowAds", 3);
    }
    if (ok) g_showAdsHooked.store(true);
    LOGI(TPL_LOG " install showads-hook ok=%d (rva 0x%lX)", ok ? 1 : 0, (unsigned long)kRvaShowAds);
    return ok ? 1 : 0;
}

// ---- 立即执行：菜单按钮线程 attach 到 il2cpp 后同步执行（不依赖钩子时机） ----
static std::atomic<bool> g_threadAttached{false};

static bool ImmediateExec(int type, bool all) {
    if (!EnsureApi()) return false;
    if (!g_threadAttached.load()) {
        void* dom = GetApi().Domain();
        if (!dom || !GetApi().thread_attach) {
            LOGW(TPL_LOG " 无 thread_attach/domain（dom=%p attach=%p）→ 退回钩子队列",
                 dom, (void*)GetApi().thread_attach);
            return false;
        }
        GetApi().thread_attach(dom);
        g_threadAttached.store(true);
        LOGI(TPL_LOG " UI 线程已 attach 到 il2cpp domain=%p", dom);
    }
    if (!ResolveMethods()) return false;
    void* player = EnsurePlayer();
    if (!OkPtr(player)) { LOGW(TPL_LOG " ImmediateExec: 玩家实例还没拿到 → 退回钩子队列"); return false; }

    g_inAction.store(true);
    if (all) {
        ProbeAttrs(player);
        // ★ 顺序关键：t4 是「体力上限」（GetAttrLimitValue 反汇编写死 w1=#4），
        //   t2（体力）写入会被夹到 t4 的值上。实测 t2/t4 同步 100→110 ⇒ 必须先抬 t4。
        ApplyOne(player, 4);
        for (int i = 0; i < kAttrTypes; ++i) ApplyOne(player, i);
        ApplyOne(player, 2);          // 上限放开后再补一次体力
        ProbeAttrs(player);
    }
    else ApplyOne(player, type);
    g_inAction.store(false);
    return true;
}

// ============================================================================
//  ⑨ 对外接口（ModEntry.h 注册表契约，签名不可改）
// ============================================================================
bool Bysxy_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;

    InstallFaultGuard();
    g_libReady.store(true);
    LOGI(TPL_LOG " lib ready (%s)", TPL_TARGET_LIB);

    InstallPlayerHook();
    InstallMgrInitHook();
    InstallFocusHook();
    InstallShowAdsHook();
    InstallLimitHook();
    // ★ 刻意不在此处 ResolveMethods()：TryInit 时 il2cpp 往往还没 init 完，
    //   提前调 Assemblies()/class_from_name 会 SIGABRT（实测事故见 InstallPlayerHook 注释）。
    //   方法表推迟到「第一次点功能、且已在游戏线程（get_Player 钩子内）」再解析。
    LOGI(TPL_LOG " tryInit done（方法表延迟到游戏线程首用时解析）");
    return true;
}

bool Bysxy_IsReady() {
    return g_libReady.load() && g_updHooked.load();
}

// 探针：判断某槽是「可写但被上限夹住」还是「根本不走 SetAttr 字典」
static void ProbeWritable(void* player, int type) {
    int before = 0, mid = 0, back = 0;
    if (!InvokeGetAttr(player, type, &before)) return;
    InvokeSetAttr(g_m.setAttrBase, player, type, -50, true);
    InvokeGetAttr(player, type, &mid);
    InvokeSetAttr(g_m.setAttrBase, player, type, 50, true);
    InvokeGetAttr(player, type, &back);
    LOGI(TPL_LOG " 探针 t%d: before=%d 减50后=%d 回补后=%d => %s",
         type, before, mid, back,
         (mid != before) ? "可写(被上限夹住)" : "不可写(不走SetAttr字典)");
}

// 对照方（斌哥）同款：一个按钮，四项各加一次
static void RequestOneKey() {
    LOGI(TPL_LOG " 一键四维 amount=%lld", (long long)g_amount.load());
    if (ImmediateExec(0, true)) return;
    g_pending.fetch_or(kActAll, std::memory_order_acq_rel);
    LOGI(TPL_LOG " 已排队 一键四维（等游戏线程钩子）");
}

void Bysxy_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == BYSXY_FEAT_NOAD) {
        g_noAd.store(enabled);
        if (enabled) InstallShowAdsHook();
        LOGI(TPL_LOG " 免广告 %s", enabled ? "ON" : "OFF");
        return;
    }
    if (featNum == BYSXY_FEAT_ONEKEY && enabled) RequestOneKey();
}

void Bysxy_OnValueChanged(int featNum, int value) {
    if (featNum == BYSXY_FEAT_AMOUNT && value > 0) g_amount.store(value);
}

void Bysxy_OnInputChanged(int featNum, const char* text) {
    if (!text || !*text) return;
    long long v = std::strtoll(text, nullptr, 10);
    if (v <= 0) return;
    if (v > 2000000000LL) v = 2000000000LL;
    if (featNum == BYSXY_FEAT_AMOUNT) {
        g_amount.store(v);
        LOGI(TPL_LOG " amount=%lld", v);
    }
}

void Bysxy_Tick(const bool* /*features*/, const int* /*values*/) {
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) {
        InstallFaultGuard();
        g_libReady.store(true);
        LOGI(TPL_LOG " lib ready (via Tick fallback)");
    }
    if (!g_updHooked.load() && g_libReady.load()) InstallPlayerHook();
    if (!g_initHooked.load() && g_libReady.load()) InstallMgrInitHook();
    if (!g_focusHooked.load() && g_libReady.load()) InstallFocusHook();
    if (!g_showAdsHooked.load() && g_libReady.load()) InstallShowAdsHook();
    if (!g_limitHooked.load() && g_libReady.load()) InstallLimitHook();
    // ★ 禁止在此 ResolveMethods()：Tick 跑在 mod 后台线程且可能在 il2cpp 就绪前，
    //   会重演 SIGABRT。方法表只在游戏线程（get_Player 钩子内）解析。
}

void Bysxy_ForceClear() {
    g_pending.store(0);
    g_pendingType.store(0);
    LOGI(TPL_LOG " force clear");
}
