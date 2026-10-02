/* ================================================================
 *  星光菜单游戏模块 — 疾风骑士 (com.emay.wxsy.topon) v60.0.1 TapTap 渠道
 *
 *  目标库：libil2cpp.so（Unity IL2CPP, Metadata v31, arm64-v8a）
 *  原版与斌哥改包 libil2cpp.so md5 均为 e8def0676740a16a06cac155c7cf87d2
 *  ⇒ 抓点 RVA 零重定位。本文件全部【按名挂载】(rva=0, expectInsn=0)，
 *    规避跨版本 RVA 漂移；各目标 RVA 仅作注释留档。
 *
 *  ★ 防闪退设施是代码，不是注释。删任何一节都可能复现历史闪退：
 *    默认 0 hook（懒装）/ 双路径挂点 / RVA 漂移拒绝 / 幂等安装 /
 *    orig 必调 / reenter 防递归 / SafeWrite 校验写 / 类名缓存 / Tick 轻量。
 *
 *  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
 * ================================================================
 */
#include "JfqsMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
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

#define TPL_LOG       "[jfqs]"
#define TPL_TARGET_LIB sec_lib_il2cpp()

/* ---------------------------------------------------------------- ① 状态 */
static std::atomic<bool> g_libReady{false};        // 库已映射 ≠ 已挂钩
static std::atomic<bool> g_anyHooked{false};
static std::atomic<bool> g_reenter{false};
static std::atomic<int>  g_installedCount{0};

/* ★ 装钩串行锁（2026-10-02 实测闪退后补）。
 *   Jfqs_OnFeatureChanged 来自菜单 UI 线程，Jfqs_Tick 来自游戏线程，
 *   两者会【同时】进 SyncHooks()。HookSlotInstall 的幂等守卫
 *   `if (s.origPtr && *s.origPtr) return true;` 是非原子 check-then-act：
 *   两线程同时通过检查 → 对同一地址调两次 DobbyHook → 第二次在已被改写的
 *   prologue 上重定位 → DobbyCodePatch 越界 memcpy → SIGSEGV(SEGV_ACCERR)。
 *   实测崩溃栈：__memcpy_aarch64_simd ← DobbyCodePatch+96 ← DobbyHook+216。
 *   删掉这把锁 = 复现闪退。 */
static std::mutex g_installMutex;

/* 数值型开关：1 = 关闭（铁律：默认必须 1，禁止默认开） */
static std::atomic<int> g_dmgMul{1};               // 伤害倍率  1..JFQS_DMG_MUL_MAX
static std::atomic<int> g_moveMul{1};              // 移动速度  1..JFQS_MOVE_MUL_MAX

/* 布尔开关 */
static std::atomic<bool> g_inv{false};             // 超级无敌
static std::atomic<bool> g_money{false};           // 无限货币
static std::atomic<bool> g_skill{false};           // 技能无限
static std::atomic<bool> g_jump{false};            // 无限跳跃
static std::atomic<bool> g_noad{false};            // 免广告
static std::atomic<bool> g_char{false};            // 解锁全角色
static std::atomic<bool> g_skin{false};            // 解锁全皮肤
static std::atomic<bool> g_weapon{false};          // 解锁全武器
static std::atomic<bool> g_pet{false};             // 解锁全宠物
static std::atomic<bool> g_artifact{false};        // 解锁全神器
static std::atomic<bool> g_skillUnlock{false};     // 解锁全部技能
static std::atomic<bool> g_synergy{false};         // 解锁协同
static std::atomic<bool> g_statue{false};          // 百分百改造成功

static const int kCurrencyMax   = 999999999;
static const int kMoreJumpCount = 999;

/* ------------------------------------------------------- ② 偏移与常量 */
/* 本模块纯行为型（改判定/伤害结算），未使用数据型直写；
 * 以下助手保留给后续按 GAME_MOD_GUIDE 分层补充数据型功能时使用。
 * 铁律：偏移必须 dump.cs / IDA / Trace 实测，禁止猜；换包必须重 dump。 */
enum : size_t {
    kOffHurtValue = 0x30,     // 示例：伤害字段偏移（未实测，勿直接使用）
};

/* ================================================================
 *  ★★ 致命信号兜底 —— 装钩期的 SIGSEGV 接住器
 *  ---------------------------------------------------------------
 *  为什么必须要有这一节（2026-10-02 实测）：
 *      `DobbyHook(...) != 0` 只能接住 Dobby【返回】的错误码，
 *      接不住 Dobby【内部】的崩溃。实测崩溃栈：
 *          __memcpy_aarch64_simd  ←  DobbyCodePatch+96  ←  DobbyHook+216
 *      SIGSEGV 是进程级致命信号 —— C++ try/catch、返回值判断、
 *      std::lock_guard、std::atomic 统统拦不住，进程当场死。
 *      ⇒ 装钩必须整段跑在 sigsetjmp 保护区里：出信号就 siglongjmp
 *        回到保护区，把该槽位【永久熔断】，游戏继续活着。
 *
 *  设计约束：
 *   1) 只在装钩这一小段生效。保护区外一律交回旧 handler 并重新抛出，
 *      保证游戏自身的崩溃仍能产出完整 tombstone（不掩盖真问题）。
 *   2) 每个线程要自己的备用信号栈（sigaltstack 是 per-thread 的），
 *      因为装钩可能发生在菜单 UI 线程或游戏线程。
 *   3) 连环炸（handler 内部再炸）超过阈值就放弃保护、还原默认，
 *      避免无限 longjmp 打转。
 * ================================================================ */
/* ★ 这几个必须是 thread_local：hook 体里也要用保护区（见 RunGuarded），
 *   而 hook 体会在菜单 UI 线程 / 游戏线程 / SDK 回调线程上并发执行。
 *   全局 jmp_buf 会被两线程互相踩 → 跳回错误栈帧 → 二次崩溃。
 *   都是常量初始化的 POD，信号处理器里访问是 async-signal-safe 的。 */
static thread_local sigjmp_buf            t_faultJmp;
static thread_local volatile sig_atomic_t t_inGuard    = 0;
static thread_local volatile sig_atomic_t t_faultDepth = 0;
static thread_local volatile int          t_faultSig   = 0;
static thread_local volatile void*        t_faultAddr  = nullptr;

static struct sigaction g_oldSegv;
static struct sigaction g_oldBus;
static struct sigaction g_oldIll;

static std::mutex g_guardMutex;        // 只保护"武装 handler"这一次性动作
static bool       g_guardArmed = false;

/* 本槽位是否发生了【致命信号】（区别于普通的"方法缺失降级"）。
 * 只在 g_installMutex 下读写；由 DobbyHookGuarded/ReadU32Guarded 置位。 */
static bool g_lastFatal = false;

static thread_local char t_altStack[65536];
static thread_local bool t_altStackSet = false;

/* 保护区外发生信号 → 交回旧 handler，保持游戏自身崩溃可见 */
static void JfqsFaultHandler(int sig, siginfo_t* si, void* uc) {
    if (t_inGuard) {
        if (++t_faultDepth > 4) {           // 保护区内连环炸 → 认输，还原默认
            signal(sig, SIG_DFL);
            raise(sig);
            return;
        }
        t_faultSig  = sig;
        t_faultAddr = si ? si->si_addr : nullptr;
        siglongjmp(t_faultJmp, 1);          // 永不返回
    }

    struct sigaction* old = (sig == SIGSEGV) ? &g_oldSegv
                          : (sig == SIGBUS)  ? &g_oldBus : &g_oldIll;
    if ((old->sa_flags & SA_SIGINFO) && old->sa_sigaction) {
        old->sa_sigaction(sig, si, uc);
    } else if (old->sa_handler != SIG_DFL && old->sa_handler != SIG_IGN) {
        old->sa_handler(sig);
    }
    signal(sig, SIG_DFL);
    raise(sig);
}

static void ArmFaultGuardForThread() {
    if (t_altStackSet) return;
    t_altStackSet = true;
    stack_t ss{};
    ss.ss_sp    = t_altStack;
    ss.ss_size  = sizeof(t_altStack);
    ss.ss_flags = 0;
    if (sigaltstack(&ss, nullptr) != 0) {
        LOGW(TPL_LOG " sigaltstack failed — 兜底降级为无备用栈");
    }
}

static void InstallFaultGuard() {
    ArmFaultGuardForThread();

    std::lock_guard<std::mutex> lk(g_guardMutex);
    if (g_guardArmed) return;
    g_guardArmed = true;

    struct sigaction sa{};
    sa.sa_sigaction = JfqsFaultHandler;
    sa.sa_flags     = SA_SIGINFO | SA_ONSTACK | SA_NODEFER;
    sigemptyset(&sa.sa_mask);

    sigaction(SIGSEGV, &sa, &g_oldSegv);
    sigaction(SIGBUS,  &sa, &g_oldBus);
    sigaction(SIGILL,  &sa, &g_oldIll);
    LOGI(TPL_LOG " fault guard armed (SIGSEGV/SIGBUS/SIGILL, altstack=%zu)", sizeof(t_altStack));
}

/* 在信号保护下跑 DobbyHook。
 * 返回 false = 没挂上（可能是 Dobby 返回错误，也可能是崩溃被接住）。 */
static bool DobbyHookGuarded(const char* tag, void* target, void* hook, void** orig) {
    InstallFaultGuard();

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

    /* 落到这里 = 保护区内炸了，进程已经被 siglongjmp 救回来 */
    t_inGuard = 0;
    g_lastFatal = true;
    LOGW(TPL_LOG " %s dobby SIG(%d) @%p — 槽位熔断，游戏继续", tag,
         (int)t_faultSig, const_cast<void*>(t_faultAddr));
    if (orig) *orig = nullptr;   // 半写坏的 orig 必须清掉，否则 hook 体里会调野指针
    return false;
}

/* 读序言也要保护：目标在可执行段，理论上可读，但不能赌 */
static bool ReadU32Guarded(const char* tag, const void* p, uint32_t* out) {
    InstallFaultGuard();

    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        memcpy(out, p, sizeof(*out));
        t_inGuard = 0;
        return true;
    }
    t_inGuard = 0;
    g_lastFatal = true;
    LOGW(TPL_LOG " %s prologue read SIG(%d) @%p — 跳过该槽位", tag,
         (int)t_faultSig, p);
    return false;
}

/* ★ hook 体里跑"别人的代码"也要保护（2026-10-02 新增）。
 * 典型场景：免广告直接 Invoke 游戏的奖励回调 —— 该回调可能读 ADXManager 上
 * 只有 SDK 回调才会填的字段，踩空就是 SIGSEGV。接住它，最多本次不发奖，
 * 绝不让整个游戏陪着一起死。返回 false = 回调内炸了（已吞掉）。 */
static bool RunGuarded(const char* tag, void (*fn)(void*), void* arg) {
    if (!fn) return false;
    InstallFaultGuard();

    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        fn(arg);
        t_inGuard = 0;
        return true;
    }
    t_inGuard = 0;
    LOGW(TPL_LOG " %s 回调内 SIG(%d) @%p — 已吞掉，游戏继续", tag,
         (int)t_faultSig, const_cast<void*>(t_faultAddr));
    return false;
}

/* ------------------------------------------------------- ③ 安全助手 */
/* 防闪退核心，禁止绕开它们裸读写内存 */
static bool OkPtr(void* p) { return p && Il2Cpp::IsValidHeapPtr(p); }

static void SafeWriteI32(void* obj, size_t off, int32_t v) {
    if (!OkPtr(obj)) return;
    Il2Cpp::WriteI32(obj, off, v);
}

static void SafeWriteF32(void* obj, size_t off, float v) {
    if (!OkPtr(obj)) return;
    memcpy(static_cast<uint8_t*>(obj) + off, &v, sizeof(v));
}

static int32_t SafeReadI32(void* obj, size_t off, int32_t defv) {
    if (!OkPtr(obj)) return defv;
    int32_t v = defv;
    memcpy(&v, static_cast<uint8_t*>(obj) + off, sizeof(v));
    return v;
}

/* 热路径禁止每帧 object_class + class_name 解析 */
struct ClassNameCache {
    void* klass  = nullptr;
    char  name[40] = {};
    bool  valid  = false;
};

static const char* CacheClassName(ClassNameCache& cache, void* obj) {
    if (!OkPtr(obj)) return nullptr;
    void* k = GetApi().object_class(obj);
    if (!k) return nullptr;
    if (cache.valid && cache.klass == k) return cache.name;
    const char* n = GetApi().class_name(k);
    if (!n) return nullptr;
    cache.klass = k;
    cache.valid = true;
    snprintf(cache.name, sizeof(cache.name), "%s", n);
    return cache.name;
}

static bool NameHas(const char* got, const char* needle) {
    return got && needle && strstr(got, needle) != nullptr;
}

/* --------------------------------------------------- ④ 双路径挂点 */
/* 提炼自 WhrhxMod：禁止 base+RVA，禁止全 RX 盲扫 */
static bool IsLibMapLine(const char* line, const char* lib) {
    return line && strstr(line, lib) && strstr(line, ".so");
}

/* libil2cpp 是分段映射，base+RVA 会挂错 —— 必须走 /proc/self/maps，
 * 只在含 'x' 的段里找，并取能覆盖 fileOff 的最大 span。 */
static void* FileOffToVa(const char* lib, uintptr_t fileOff) {
    FILE* fp = fopen("/proc/self/maps", "r");
    if (!fp) return nullptr;

    char     line[512];
    void*    best      = nullptr;
    uintptr_t bestSpan = 0;

    while (fgets(line, sizeof(line), fp)) {
        if (!IsLibMapLine(line, lib)) continue;
        uintptr_t start = 0, end = 0, off = 0;
        char      perm[8] = {};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x')) continue;              // 只要可执行段
        if (fileOff < off || fileOff >= off + (end - start)) continue;
        uintptr_t span = end - start;
        if (span > bestSpan) {
            bestSpan = span;
            best     = reinterpret_cast<void*>(start + (fileOff - off));
        }
    }
    fclose(fp);
    return best;
}

static bool HookAtRva(const char* tag, uintptr_t rva, uint32_t expectInsn,
                      void* hook, void** orig) {
    void* va = FileOffToVa(TPL_TARGET_LIB, rva - 0x4000);   // file_off = RVA − 0x4000
    if (!va) {
        LOGW(TPL_LOG " %s rva 0x%lX not in RX — try by-name", tag,
             static_cast<unsigned long>(rva));
        return false;
    }
    if (expectInsn != 0) {
        uint32_t got = 0;
        if (!ReadU32Guarded(tag, va, &got)) return false;   // 读序言也走保护区
        if (got == 0 || got == 0xFFFFFFFFu || got != expectInsn) {
            LOGW(TPL_LOG " %s rva drift got=0x%08X want=0x%08X — try by-name",
                 tag, got, expectInsn);
            return false;
        }
    }
    if (!DobbyHookGuarded(tag, va, hook, orig)) {           // ★ 崩溃也接得住
        LOGW(TPL_LOG " %s dobby fail @rva 0x%lX", tag,
             static_cast<unsigned long>(rva));
        return false;
    }
    LOGI(TPL_LOG " %s hooked @rva 0x%lX", tag, static_cast<unsigned long>(rva));
    return true;
}

static bool EnsureApi() {
    if (GetApi().ready) return true;
    return GetApi().Load(TPL_TARGET_LIB);
}

static void* FindClassNs(const char* ns, const char* name) {
    size_t n = 0;
    void** asms = GetApi().Assemblies(&n);
    if (!asms) return nullptr;
    for (size_t i = 0; i < n; ++i) {
        void* img = GetApi().ImageOf(asms[i]);
        if (!img) continue;
        void* k = GetApi().ClassFromName(img, ns, name);
        if (k) return k;
    }
    return nullptr;
}

/* 按名挂载：类缺 / 方法缺只降级并继续装其它（hook-rules §17），绝不整包失败 */
static bool HookByName(const char* tag, const char* ns, const char* cls,
                       const char* method, int argc, void* hook, void** orig) {
    if (!EnsureApi()) {
        LOGW(TPL_LOG " %s api not ready — %s will retry via Tick", tag, cls);
        return false;
    }
    void* klass = FindClassNs(ns, cls);
    if (!klass) {
        klass = GetApi().FindClassByShortName(cls);
        if (klass) LOGW(TPL_LOG " %s class miss %s — degrade", tag, cls);
    }
    if (!klass) {
        LOGW(TPL_LOG " %s class miss %s — degrade", tag, cls);
        return false;
    }
    void* mi = GetApi().GetMethod(klass, method, argc);
    if (!mi) {
        LOGW(TPL_LOG " %s method miss %s.%s — degrade, install others", tag, cls, method);
        return false;
    }
    void* fn = *reinterpret_cast<void**>(mi);
    if (!OkPtr(fn)) {
        LOGW(TPL_LOG " %s fn bad %s.%s — degrade", tag, cls, method);
        return false;
    }
    if (!DobbyHookGuarded(tag, fn, hook, orig)) {   // ★ 崩溃也接得住
        LOGW(TPL_LOG " %s dobby fail %s.%s", tag, cls, method);
        return false;
    }
    LOGI(TPL_LOG " %s hooked %s.%s(argc=%d)", tag, cls, method, argc);
    return true;
}

/* rva = 0 → 纯按名；expectInsn = 0 → 不校验序言 */
struct HookSlot {
    const char* tag;
    uintptr_t   rva;
    uint32_t    expectInsn;
    const char* ns;
    const char* cls;
    const char* method;
    int         argc;
    void*       hookFn;
    void**      origPtr;
};

/* 幂等：orig 非空说明已挂过，直接成功返回 —— 防二次 Dobby 叠加 → SIGSEGV */
static bool HookSlotInstall(const HookSlot& s) {
    if (s.origPtr && *s.origPtr) return true;
    g_lastFatal = false;                       // 每槽位重置致命标记
    if (s.rva != 0) {
        if (HookAtRva(s.tag, s.rva, s.expectInsn, s.hookFn, s.origPtr)) return true;
        /* RVA 路径崩过 → 别再拿按名路径去撞同一个地址，直接熔断本槽位 */
        if (g_lastFatal) return false;
    }
    return HookByName(s.tag, s.ns, s.cls, s.method, s.argc, s.hookFn, s.origPtr);
}

/* --------------------------------- ⑤-A 真实写入工具（2026-10-02 新增） */
/* 反汇编结论（v2 实测"解锁全角色没效果 / 宠物重启后消失并卡死"的根因）：
 *
 *   DataController.LoadPlayerUnlock(CharacterName)          RVA 0x390BB38  ← 角色【真门禁】
 *     IsCharacterUnlockedForPurchase 0x390BBCC 只是外壳，尾部 bl #0x390bb38。
 *     0x390BB38 内部：cmp w19,#0x11 → cset w9,lo（角色 0..16 合法）；
 *                     cbz w19 → 角色 0 恒真；其余走时限判定。
 *   DataController.LoadPlayerSkinUnlock(CharacterName,int)  RVA 0x390C4EC
 *   DataController.LoadLobbySkinUnlock(LobbySkins)          RVA 0x390C7FC
 *   DataController.LoadPlayerWeaponUnlock(CharacterName,int)RVA 0x38FE118
 *   DataController.LoadPetUnlock(int)                       RVA 0x390B060
 *
 * 每一对都有配套 Save*（同为 DataController 实例方法，self 可直接透传）：
 *   SavePlayerUnlock(CharacterName,bool=False) 0x390BCC8
 *   SavePlayerSkinUnlock(CharacterName,int)    0x390C6D8
 *   SaveLobbySkinUnlock(LobbySkins)            0x390C988
 *   SavePlayerWeaponUnlock(CharacterName,int)  0x390CF3C
 *   SavePetUnlock(int)                         0x390B0DC（HashSet<int> 落库）
 *
 * ★ 设计：谓词返 true 的【同时】调游戏自己的 Save*，把解锁真正写进存档。
 *   旧版只"撒谎"不写档 ⇒ UI 允许装备实际未拥有的东西 ⇒ 宠物 ID 落进存档 ⇒
 *   重启后功能关闭，游戏读到"拥有集合里没有、存档却引用了"⇒ 弹「尚未拥有该宠物」
 *   并卡死 UI。真实写入同时消灭了这个不一致。 */

/* 记忆已写过的 id：Save* 会落库（+云同步），不能每帧都写。8 组独立命名空间。 */
static const int kMemoMax = 4096;
static uint8_t   g_memo[8][kMemoMax];
static std::mutex g_memoMutex;

static bool MemoFirst(int bucket, int idx) {
    if (bucket < 0 || bucket >= 8) return false;
    if (idx < 0 || idx >= kMemoMax) return true;      // 越界不记忆，每次都写
    std::lock_guard<std::mutex> lk(g_memoMutex);
    if (g_memo[bucket][idx]) return false;
    g_memo[bucket][idx] = 1;
    return true;
}

/* IL2CPP MethodInfo 首字段即函数指针（布局稳定）。按名取方法后直接调用，
 * 比 runtime_invoke 少一层装箱，且不必构造 Il2CppObject* 参数数组。 */
static void CallVoid1(void* klass, const char* name, void* self, int a0) {
    if (!OkPtr(klass) || !OkPtr(self) || !name) return;
    void* mi = GetApi().GetMethod(klass, name, 1);
    if (!mi) return;
    void* fn = *reinterpret_cast<void**>(mi);
    if (!OkPtr(fn)) return;
    reinterpret_cast<void (*)(void*, int, void*)>(fn)(self, a0, mi);
}

static void CallVoid2I(void* klass, const char* name, void* self, int a0, int a1) {
    if (!OkPtr(klass) || !OkPtr(self) || !name) return;
    void* mi = GetApi().GetMethod(klass, name, 2);
    if (!mi) return;
    void* fn = *reinterpret_cast<void**>(mi);
    if (!OkPtr(fn)) return;
    reinterpret_cast<void (*)(void*, int, int, void*)>(fn)(self, a0, a1, mi);
}

/* 类指针按名缓存（与 HookByName 同序：先 ClassFromName，再短名兜底） */
static void* ClassCached(const char* name) {
    static char  s_name[6][40] = {};
    static void* s_klass[6]    = {};
    static int   s_n           = 0;
    if (!name || !EnsureApi()) return nullptr;
    for (int i = 0; i < s_n; ++i) {
        if (strcmp(s_name[i], name) == 0) return s_klass[i];
    }
    void* k = FindClassNs("", name);
    if (!k) k = GetApi().FindClassByShortName(name);
    if (k && s_n < 6) {
        snprintf(s_name[s_n], sizeof(s_name[s_n]), "%s", name);
        s_klass[s_n] = k;
        ++s_n;
    }
    return k;
}

/* 免广告用：直接触发 System.Action.Invoke（跳过广告播放，但照常发奖）。
 * ★ 分两层：Raw 是裸调用，InvokeAction 把它套进信号保护区 ——
 *   游戏侧奖励回调可能读 ADXManager 上只有 SDK 回调才填的字段，踩空就是 SIGSEGV；
 *   接住它最多"本次不发奖"，绝不让整个游戏陪着一起死。 */
static void InvokeActionRaw(void* action) {
    void* k = GetApi().object_class(action);
    if (!OkPtr(k)) return;
    void* mi = GetApi().GetMethod(k, "Invoke", 0);
    if (!mi) return;
    void* fn = *reinterpret_cast<void**>(mi);
    if (!OkPtr(fn)) return;
    reinterpret_cast<void (*)(void*, void*)>(fn)(action, mi);
}

static void InvokeAction(void* action) {
    if (!OkPtr(action)) return;
    g_reenter.store(true);
    RunGuarded("InvokeAction", InvokeActionRaw, action);
    g_reenter.store(false);
}

/* ------------------------------------------------- ⑤ 钩子体与槽表 */
/* 统一范式：先调 orig（铁律）→ reenter 守卫 → 全关不动 → 仅在开关开时改返回值。 */

/* ★ 诊断探针（v5，2026-10-02 新增）
 * 问题：42 个槽全部 hooked 成功、无崩溃，但"技能无限 / 免广告"实测仍无效 ——
 *       说明我们挂的函数根本不是游戏真正走的那条路径，而 logcat 看不出哪条被调过。
 * 做法：每个 hook 前 3 次被调用时打一条日志（带 orig 返回值），之后静默。
 *       只读写一个静态 int，无锁无分配；确认完把 JFQS_TRACE 改成 0 即整体关闭。 */
#define JFQS_TRACE 1

#if JFQS_TRACE
static void TraceHit(int* counter, const char* tag, int val) {
    if (!counter || *counter >= 3) return;
    ++*counter;
    LOGI(TPL_LOG " trace %s orig=%d", tag, val);
}
#define JFQS_TR(fn, v) TraceHit(&s_tr_##fn, #fn, (int)(v))
#else
#define JFQS_TR(fn, v) do { } while (0)
#endif

#define JFQS_DEF_BOOL0(fn, flagExpr)                                          \
    static bool (*g_orig_##fn)(void*, void*) = nullptr;                       \
    static int s_tr_##fn = 0;                                                 \
    static bool Hooked_##fn(void* self, void* method) {                       \
        bool orig = g_orig_##fn ? g_orig_##fn(self, method) : false;          \
        JFQS_TR(fn, orig);                                                    \
        if (g_reenter.load()) return orig;                                    \
        if (!(flagExpr)) return orig;                                         \
        return true;                                                          \
    }

#define JFQS_DEF_BOOL1(fn, flagExpr)                                          \
    static bool (*g_orig_##fn)(void*, void*, void*) = nullptr;                \
    static int s_tr_##fn = 0;                                                 \
    static bool Hooked_##fn(void* self, void* a0, void* method) {             \
        bool orig = g_orig_##fn ? g_orig_##fn(self, a0, method) : false;      \
        JFQS_TR(fn, orig);                                                    \
        if (g_reenter.load()) return orig;                                    \
        if (!(flagExpr)) return orig;                                         \
        return true;                                                          \
    }

#define JFQS_DEF_BOOL2(fn, flagExpr)                                          \
    static bool (*g_orig_##fn)(void*, void*, void*, void*) = nullptr;         \
    static int s_tr_##fn = 0;                                                 \
    static bool Hooked_##fn(void* self, void* a0, void* a1, void* method) {   \
        bool orig = g_orig_##fn ? g_orig_##fn(self, a0, a1, method) : false;  \
        JFQS_TR(fn, orig);                                                    \
        if (g_reenter.load()) return orig;                                    \
        if (!(flagExpr)) return orig;                                         \
        return true;                                                          \
    }

#define JFQS_DEF_BOOL3(fn, flagExpr)                                          \
    static bool (*g_orig_##fn)(void*, void*, void*, void*, void*) = nullptr;  \
    static int s_tr_##fn = 0;                                                 \
    static bool Hooked_##fn(void* self, void* a0, void* a1, void* a2,         \
                            void* method) {                                   \
        bool orig = g_orig_##fn ? g_orig_##fn(self, a0, a1, a2, method)       \
                                : false;                                      \
        JFQS_TR(fn, orig);                                                    \
        if (g_reenter.load()) return orig;                                    \
        if (!(flagExpr)) return orig;                                         \
        return true;                                                          \
    }

#define JFQS_DEF_INT0(fn, flagExpr, constExpr)                                \
    static int (*g_orig_##fn)(void*, void*) = nullptr;                        \
    static int s_tr_##fn = 0;                                                 \
    static int Hooked_##fn(void* self, void* method) {                        \
        int orig = g_orig_##fn ? g_orig_##fn(self, method) : 0;               \
        JFQS_TR(fn, orig);                                                    \
        if (g_reenter.load()) return orig;                                    \
        if (!(flagExpr)) return orig;                                         \
        return (constExpr);                                                   \
    }

#define JFQS_DEF_FMUL0(fn, mulExpr)                                           \
    static float (*g_orig_##fn)(void*, void*) = nullptr;                      \
    static int s_tr_##fn = 0;                                                 \
    static float Hooked_##fn(void* self, void* method) {                      \
        float orig = g_orig_##fn ? g_orig_##fn(self, method) : 0.0f;          \
        JFQS_TR(fn, orig);                                                    \
        if (g_reenter.load()) return orig;                                    \
        int m = (mulExpr);                                                    \
        if (m <= 1) return orig;                                              \
        return orig * static_cast<float>(m);                                  \
    }

#define JFQS_DEF_FCONST0(fn, flagExpr, constExpr)                             \
    static float (*g_orig_##fn)(void*, void*) = nullptr;                      \
    static int s_tr_##fn = 0;                                                 \
    static float Hooked_##fn(void* self, void* method) {                      \
        float orig = g_orig_##fn ? g_orig_##fn(self, method) : 0.0f;          \
        JFQS_TR(fn, orig);                                                    \
        if (g_reenter.load()) return orig;                                    \
        if (!(flagExpr)) return orig;                                         \
        return (constExpr);                                                   \
    }

/* --- 1. 伤害倍率 ------------------------------------------------------ */
JFQS_DEF_FMUL0(PA_GetNormalAttackDamage,   g_dmgMul.load())   // RVA 0x37E68E4
JFQS_DEF_FMUL0(Proj_GetNormalAttackDamage, g_dmgMul.load())   // RVA 0x379E6D4

/* --- 2. 超级无敌 ------------------------------------------------------ */
/* ★ 唯一特许「不调 orig」的 hook：对齐斌哥 H1 handler
 *   （bit0 置位 → stur wzr,[x29,#-4] → 直接返 HPType 0）。 */
typedef int (*PD_TakeDamage_t)(void* self, int damage, void* info, void* method);
static PD_TakeDamage_t g_orig_PD_TakeDamage = nullptr;
static int Hooked_PD_TakeDamage(void* self, int damage, void* info, void* method) {
    if (g_inv.load() && !g_reenter.load()) return 0;   // 特许：吞掉伤害，不调 orig
    return g_orig_PD_TakeDamage ? g_orig_PD_TakeDamage(self, damage, info, method) : 0;
}

/* --- 3. 无限货币 ------------------------------------------------------ */
/* 货币为局内/存档读接口（DataController 无 LoadGold/SaveGold），
 * 读满 + 吞掉消费 ⇒ 实际无限。云同步不一致风险已在单机 CTF 目标下接受。 */
JFQS_DEF_INT0(DC_LoadGem,         g_money.load(), kCurrencyMax)   // RVA 0x3907570
JFQS_DEF_INT0(DC_LoadSoulStone,   g_money.load(), kCurrencyMax)   // RVA 0x39075E0
JFQS_DEF_INT0(DC_LoadPrayerStone, g_money.load(), kCurrencyMax)   // RVA 0x3907650
JFQS_DEF_INT0(DC_LoadBreachStone, g_money.load(), kCurrencyMax)   // RVA 0x3907870
JFQS_DEF_INT0(DC_LoadMileagePoint,g_money.load(), kCurrencyMax)   // RVA 0x3906610

typedef void (*PD_UseGold_t)(void* self, int gold, void* method);
static PD_UseGold_t g_orig_PD_UseGold = nullptr;
static void Hooked_PD_UseGold(void* self, int gold, void* method) {
    if (g_money.load()) return;                        // 免费消费：不调 orig
    if (g_orig_PD_UseGold) g_orig_PD_UseGold(self, gold, method);
}

/* --- 4. 移动速度 ------------------------------------------------------ */
JFQS_DEF_FMUL0(PM_GetMoveSpeedPower, g_moveMul.load())        // RVA 0x37C0B4C

/* --- 5. 技能无限 ------------------------------------------------------ */
/* ★ 用户实测"技能还有 cd 限制"。v2 只挂了 3 个谓词 + SkillCoolTime：
 *   反汇编 CheckCanSubSkillByCoolTime 0x3817E6C 的真实语义其实是
 *     return CheckCanAttackSkill() && !CheckEquipNormalArtifact(0x23) && !CheckEquipNormalArtifact(0x15b)
 *   —— 与"冷却"无关，名字骗人。真正的冷却门禁在 PlayerSkillManager 的
 *   CheckCanSubSkill / CheckCanAttackSkill / CheckCanBuffSkill / CheckSkillReady，
 *   以及 SkillManager 的 SkillCoolTimeRealtime（SkillCoolTime 只喂 UI 显示）。 */
JFQS_DEF_BOOL0(PSM_CheckMainSkillAble,          g_skill.load())  // 0x3810568
JFQS_DEF_BOOL0(PSM_CheckCanAllSkill,            g_skill.load())  // 0x3810E74
JFQS_DEF_BOOL0(PSM_CheckCanSubSkillByCoolTime,  g_skill.load())  // 0x3817E6C
JFQS_DEF_BOOL0(PSM_CheckCanSubSkill,            g_skill.load())  // 0x3811130
JFQS_DEF_BOOL0(PSM_CheckCanAttackSkill,         g_skill.load())  // 0x38110C0
JFQS_DEF_BOOL0(PSM_CheckCanBuffSkill,           g_skill.load())  // 0x3816604
JFQS_DEF_BOOL1(PSM_CheckSkillReady,             g_skill.load())  // 0x3811158

typedef float (*SM_SkillCoolTime_t)(void* self, int a0, int a1, void* a2, void* method);
static SM_SkillCoolTime_t g_orig_SM_SkillCoolTime = nullptr;
static int s_tr_SM_SkillCoolTime = 0;
static float Hooked_SM_SkillCoolTime(void* self, int a0, int a1, void* a2, void* method) {
    float orig = g_orig_SM_SkillCoolTime
                     ? g_orig_SM_SkillCoolTime(self, a0, a1, a2, method)
                     : 0.0f;
    TraceHit(&s_tr_SM_SkillCoolTime, "SM_SkillCoolTime", (int)orig);
    if (g_reenter.load()) return orig;
    if (!g_skill.load()) return orig;
    return 0.0f;                                       // 冷却归零
}                                                      // RVA 0x34D2850

static SM_SkillCoolTime_t g_orig_SM_SkillCoolTimeRealtime = nullptr;
static int s_tr_SM_SkillCoolTimeRealtime = 0;
static float Hooked_SM_SkillCoolTimeRealtime(void* self, int a0, int a1, void* a2,
                                             void* method) {
    float orig = g_orig_SM_SkillCoolTimeRealtime
                     ? g_orig_SM_SkillCoolTimeRealtime(self, a0, a1, a2, method)
                     : 0.0f;
    TraceHit(&s_tr_SM_SkillCoolTimeRealtime, "SM_SkillCoolTimeRealtime", (int)orig);
    if (g_reenter.load()) return orig;
    if (!g_skill.load()) return orig;
    return 0.0f;
}                                                      // RVA 0x34D35D0

/* ★ v5 追加：武器技能冷却。
 *   用户说"放技能还是有 CD"，而上面 7 条主/副技能谓词已全部恒 true ——
 *   说明他按下的是「武器技能」按钮，冷却圈画在武器技能自己的计时器上。
 *   玩家身上的武器技能冷却读写全在 PlayerMovement（cid 123660）：
 *     public float get_WeaponSkillCoolTime()          0x37AA27C  ← 属性 getter
 *     public float GetWeaponSkillCoolTime()           0x37D3CA0
 *     public void  SetWeaponSkillCoolTime(bool more)  0x37B596C  ← 起冷却（先只观察）
 *     public void  SetPlusWeaponSkillCoolTime(float)  0x37D3D6C  ← 加时长（先只观察）
 *   各角色脚本与 UI 侧另有若干读取点，一并归零。 */
JFQS_DEF_FCONST0(PM_get_WeaponSkillCoolTime,           g_skill.load(), 0.0f) // 0x37AA27C
JFQS_DEF_FCONST0(PM_GetWeaponSkillCoolTime,            g_skill.load(), 0.0f) // 0x37D3CA0
JFQS_DEF_FCONST0(WS_GetWeaponSkillCoolTime,            g_skill.load(), 0.0f) // 0x37A0B34 WeaponSkill
JFQS_DEF_FCONST0(CIS_GetWeaponSkillCoolTime,           g_skill.load(), 0.0f) // 0x3799108 CharacterInfoSelector
JFQS_DEF_FCONST0(HR_GetWeaponSkillCoolTimeFillAmount,  g_skill.load(), 0.0f) // 0x36E20F8 HighRollerScript
JFQS_DEF_FCONST0(SUM_get_WeaponSkillCoolTime,          g_skill.load(), 0.0f) // 0x3771BFC SummonerScript
JFQS_DEF_FCONST0(SUM_GetWeaponSkillCoolTime,           g_skill.load(), 0.0f) // 0x3774DB4 SummonerScript
JFQS_DEF_BOOL0(PSM_CheckCanSubSkillByAttack,           g_skill.load())       // 0x3817D7C
JFQS_DEF_BOOL1(PSM_CheckUse,                           g_skill.load())       // 0x381503C

/* ★★ v5 关键：武器技能冷却的**唯一计时字段** this+0x588。
 * 反汇编三条证实了这一点：
 *   PlayerMovement.get_WeaponSkillCoolTime() 0x37AA27C（8 字节 getter）
 *       ldr s0, [x0, #0x588] ; ret          ← 剩余冷却秒数
 *   PlayerMovement.SetWeaponSkillCoolTime(bool more) 0x37B596C（w1 实际未被使用）
 *       bl #0x37d3ca0        ; s0 = GetWeaponSkillCoolTime()（算总时长）
 *       str s0, [x19, #0x588]               ← 起冷却 = 把剩余量填满
 *   PlayerMovement.SetPlusWeaponSkillCoolTime(float time) 0x37D3D6C
 *       ldr s1,[x0,#0x588]; fadd s1,s1,s0; fmax s1,s1,#0
 *       str s1,[x0,#0x588]                  ← 加/减冷却
 * ⇒ 想"无冷却"，堵住写入点即可（读取点 getter 也一并归零，保证 UI 冷却圈是满的）。
 *   注意：SetWeaponSkillCoolTime 忽略 more 参数，语义就是"把冷却重置为满"，
 *   所以开技能无限时必须**跳过 orig**，否则每次放完技能又被填满。 */
static const size_t kOff_WeaponSkillCoolTime = 0x588;

typedef void (*PM_SetWSCD_t)(void* self, bool more, void* method);
static PM_SetWSCD_t g_orig_PM_SetWeaponSkillCoolTime = nullptr;
static int s_tr_PM_SetWeaponSkillCoolTime = 0;
static void Hooked_PM_SetWeaponSkillCoolTime(void* self, bool more, void* method) {
    TraceHit(&s_tr_PM_SetWeaponSkillCoolTime, "PM_SetWeaponSkillCoolTime", more ? 1 : 0);
    if (g_reenter.load() || !g_skill.load()) {
        if (g_orig_PM_SetWeaponSkillCoolTime)
            g_orig_PM_SetWeaponSkillCoolTime(self, more, method);
        return;
    }
    if (self) SafeWriteF32(self, kOff_WeaponSkillCoolTime, 0.0f);  // 冷却永不起
}                                                      // RVA 0x37B596C

typedef void (*PM_SetPlusWSCD_t)(void* self, float time, void* method);
static PM_SetPlusWSCD_t g_orig_PM_SetPlusWeaponSkillCoolTime = nullptr;
static int s_tr_PM_SetPlusWeaponSkillCoolTime = 0;
static void Hooked_PM_SetPlusWeaponSkillCoolTime(void* self, float time, void* method) {
    TraceHit(&s_tr_PM_SetPlusWeaponSkillCoolTime, "PM_SetPlusWeaponSkillCoolTime", (int)time);
    if (g_reenter.load() || !g_skill.load()) {
        if (g_orig_PM_SetPlusWeaponSkillCoolTime)
            g_orig_PM_SetPlusWeaponSkillCoolTime(self, time, method);
        return;
    }
    if (self) SafeWriteF32(self, kOff_WeaponSkillCoolTime, 0.0f);  // 加减冷却一律归零
}                                                      // RVA 0x37D3D6C

/* --- 6. 无限跳跃 ------------------------------------------------------ */
/* ★★ v5 最终结论（用户实测反馈 m02751 定案）：
 *   用户原话："无限跳跃打开后，自动跳一下，落地后会自动一直无限跳跃…
 *              跳跃本身是限制只能跳两次，让不限制不就可以无限跳了"。
 *   反汇编证实用户说的完全正确 —— 游戏原生就是「地面 1 跳 + 空中 1 跳」：
 *
 *   PlayerMoreJumpCountReset(int moreCount) 0x37C46C0   ← 每次落地/重置时调用
 *       0x37C46D4: bl  #0x37c4728          ; w0 = GetMaxMoreJumpCount()
 *       0x37C46D8: add w0, w0, w19         ; + moreCount
 *       0x37C46FC: str w9, [x20, #0x450]   ; ObscuredInt(24B) @ 0x440 = MoreJumpCount
 *
 *   GetMaxMoreJumpCount() 0x37C4728
 *       return CheckEquipNormalArtifact(0x6d) ? (artifactLevel + 1) : 1;
 *       ⇒ 默认 1 ⇒ 空中只能再跳 1 次 = 总共 2 跳（正是用户描述的"只能跳两次"）
 *
 *   CanMoreJump() 0x37C47E4 = (ObscuredInt@0x440 > 0)   —— 纯派生谓词
 *   CheckCanJump() 0x37BB970 —— 状态门禁（检查 0x470/0x4d0/0x4dc 等多个 Obscured 状态位）
 *
 * ★ 教训：v2~v4 把 CanMoreJump()/CheckCanJump() 一律撒谎成 true，等价于把
 *   "现在允许起跳"永远置真 ⇒ 状态机每帧都认为可以跳 ⇒ 自动连跳。
 *   正确做法是**只改数量上限**（GetMaxMoreJumpCount → 999），
 *   谓词与状态门禁一律交回 orig —— 数量变了，限制自然消失，且不会触发自动跳。 */
JFQS_DEF_INT0 (PM_GetMaxMoreJumpCount, g_jump.load(), kMoreJumpCount)  // 0x37C4728
/* 注：CanMoreJump() / CheckCanJump() / PlayerJumpCooltime() 均已从 kSlots 移除。
 *     它们分别是"派生谓词""状态门禁""节流"，撒谎都会破坏状态机。 */

/* --- 7. 免广告 -------------------------------------------------------- */
/* ★ 实测"点了还是看广告"的根因：下面两个谓词只管"是否已购买免广告道具"，
 *   与"要不要弹广告"无关。真正的广告收口在 ADXManager（cid 138016）：
 *     public void ShowRewardedAd(Action callback)       RVA 0x38B5324
 *     public void ShowInterstitialAd(Action callback)   RVA 0x38B4FB0
 *   反汇编 ShowRewardedAd：先 bl #0x2d96fc8 分配 <>c__DisplayClass29_0，
 *   把 callback 存 [obj+0x10]、this 存 [obj+0x18]，再 bl #0x6b5d97c 做静态判定，
 *   之后才向 SDK 发播放请求。⇒ 我们不发播放请求，改为立刻 Invoke(callback) 发奖。 */
JFQS_DEF_BOOL0(GS_IsNoAd,         g_noad.load())   // RVA 0x3A82944（函数体仅 16 B）
JFQS_DEF_BOOL0(DC_LoadRemoveAds,  g_noad.load())   // RVA 0x3909CB8

typedef void (*ADX_ShowAd_t)(void* self, void* callback, void* method);
static ADX_ShowAd_t g_orig_ADX_ShowRewardedAd = nullptr;
static int s_tr_ADX_ShowRewardedAd = 0;
static void Hooked_ADX_ShowRewardedAd(void* self, void* callback, void* method) {
    TraceHit(&s_tr_ADX_ShowRewardedAd, "ADX_ShowRewardedAd", 1);
    if (g_reenter.load() || !g_noad.load()) {
        if (g_orig_ADX_ShowRewardedAd) g_orig_ADX_ShowRewardedAd(self, callback, method);
        return;
    }
    LOGI(TPL_LOG " ad skipped (rewarded) → invoke callback");
    InvokeAction(callback);                 // 不播广告，直接发奖
}                                           // RVA 0x38B5324

static ADX_ShowAd_t g_orig_ADX_ShowInterstitialAd = nullptr;
static int s_tr_ADX_ShowInterstitialAd = 0;
static void Hooked_ADX_ShowInterstitialAd(void* self, void* callback, void* method) {
    TraceHit(&s_tr_ADX_ShowInterstitialAd, "ADX_ShowInterstitialAd", 1);
    if (g_reenter.load() || !g_noad.load()) {
        if (g_orig_ADX_ShowInterstitialAd) g_orig_ADX_ShowInterstitialAd(self, callback, method);
        return;
    }
    LOGI(TPL_LOG " ad skipped (interstitial) → invoke callback");
    InvokeAction(callback);
}                                           // RVA 0x38B4FB0

/* ★★ v5 关键补挂：真正的 Unity→SDK 广告桥。
 * v4 只挂了 ADXManager，实测 logcat 里 "ad skipped" 一条都没有 ⇒ 用户那次根本没走 ADXManager。
 * 真实链路：广告按钮 → AdsPanel / NewMissionSlotView → MarSDKManager.ShowReward(...)
 *   → MarSDKInterfaceAndroid.ShowReward(string) → Java com.mar.sdk.gg.topon.ToponAd → 快手/穿山甲。
 * 反汇编 NewMissionSlotView.OnClickWatchAd() 0x31D8E58：
 *   bl #0x38b4958 (ADXManager.get_Instance) → bl #0x38b4f8c (ResetAdsCoolTimeForMission)
 *   → 分配闭包 → 再取 Instance → ShowRewardedAd。⇒ ADXManager 只是一层薄包装，
 *   但并非所有入口都经过它，所以两头都挂才保险。
 *
 * MarSDKManager（cid 201842）两个重载：
 *   public void ShowReward(Action success, Action fail, Action onAdShowSuccess,
 *                          Action onAdShowFailed, Action onAdClicked, Action OnAdClosed)
 *        RVA 0x3BD1B90  argc=6，序言 stp x30,x27,[sp,#-0x50]! (0xA9BB6FFE)
 *        寄存器分配：x0=self, x1=success, x2=fail, x3=onAdShowSuccess,
 *                    x4=onAdShowFailed, x5=onAdClicked, x6=OnAdClosed
 *        ⇒ 8 个参数正好塞满 x0–x7，可直接用 C 签名 hook。
 *   public void ShowReward(string data, ...同 6 个 Action)
 *        RVA 0x3BDA06C  argc=7
 *
 * 开免广告时：跳过 orig（不发播放请求），按"广告正常播完"的顺序补齐回调 ——
 *   onAdShowSuccess（广告开始）→ success（发奖）→ OnAdClosed（广告结束）。
 * 每个 InvokeAction 内部都套 RunGuarded，回调里踩空只会被吞掉并记日志，不会带走进程。 */
typedef void (*MRSDK_ShowReward6_t)(void* self, void* success, void* fail,
                                    void* onShow, void* onShowFailed,
                                    void* onClicked, void* onClosed, void* method);
static MRSDK_ShowReward6_t g_orig_MRSDK_ShowReward6 = nullptr;
static int s_tr_MRSDK_ShowReward6 = 0;
static void Hooked_MRSDK_ShowReward6(void* self, void* success, void* fail,
                                     void* onShow, void* onShowFailed,
                                     void* onClicked, void* onClosed, void* method) {
    TraceHit(&s_tr_MRSDK_ShowReward6, "MRSDK_ShowReward6", 1);
    if (g_reenter.load() || !g_noad.load()) {
        if (g_orig_MRSDK_ShowReward6)
            g_orig_MRSDK_ShowReward6(self, success, fail, onShow, onShowFailed,
                                     onClicked, onClosed, method);
        return;
    }
    LOGI(TPL_LOG " ad skipped (MarSDKManager.ShowReward/6) → invoke callbacks");
    InvokeAction(onShow);
    InvokeAction(success);
    InvokeAction(onClosed);
}                                           // RVA 0x3BD1B90

typedef void (*MRSDK_ShowReward7_t)(void* self, void* data, void* success, void* fail,
                                    void* onShow, void* onShowFailed,
                                    void* onClicked, void* onClosed, void* method);
static MRSDK_ShowReward7_t g_orig_MRSDK_ShowReward7 = nullptr;
static int s_tr_MRSDK_ShowReward7 = 0;
static void Hooked_MRSDK_ShowReward7(void* self, void* data, void* success, void* fail,
                                     void* onShow, void* onShowFailed,
                                     void* onClicked, void* onClosed, void* method) {
    TraceHit(&s_tr_MRSDK_ShowReward7, "MRSDK_ShowReward7", 1);
    if (g_reenter.load() || !g_noad.load()) {
        if (g_orig_MRSDK_ShowReward7)
            g_orig_MRSDK_ShowReward7(self, data, success, fail, onShow, onShowFailed,
                                     onClicked, onClosed, method);
        return;
    }
    LOGI(TPL_LOG " ad skipped (MarSDKManager.ShowReward/7) → invoke callbacks");
    InvokeAction(onShow);
    InvokeAction(success);
    InvokeAction(onClosed);
}                                           // RVA 0x3BDA06C

/* --- 8. 解锁全角色 ---------------------------------------------------- */
/* ★ 真门禁是 LoadPlayerUnlock(CharacterName) 0x390BB38（IsCharacterUnlockedForPurchase
 *   0x390BBCC 只是外壳，尾部 bl #0x390bb38）。v2 只 hook 外壳 ⇒ 用户实测"没效果"。
 *   现在：谓词恒 true 的同时调 SavePlayerUnlock 真落库（见 ⑤-A 节）。 */
typedef bool (*DC_LoadPlayerUnlock_t)(void* self, int character, void* method);
static DC_LoadPlayerUnlock_t g_orig_DC_LoadPlayerUnlock = nullptr;
static bool Hooked_DC_LoadPlayerUnlock(void* self, int character, void* method) {
    bool orig = g_orig_DC_LoadPlayerUnlock
                    ? g_orig_DC_LoadPlayerUnlock(self, character, method)
                    : false;
    if (g_reenter.load()) return orig;
    if (!g_char.load()) return orig;
    if (MemoFirst(0, character)) {
        g_reenter.store(true);
        CallVoid2I(ClassCached("DataController"), "SavePlayerUnlock", self, character, 0);
        g_reenter.store(false);
    }
    return true;
}                                                                  // RVA 0x390BB38

JFQS_DEF_BOOL2(DC_IsCharacterUnlockedForPurchase, g_char.load())  // 0x390BBCC
JFQS_DEF_BOOL0(DC_IsAllCharacterOwned,            g_char.load())  // 0x390C0B8

/* --- 9. 解锁全皮肤 ---------------------------------------------------- */
typedef bool (*DC_LoadPlayerSkinUnlock_t)(void* self, int character, int skinID,
                                          void* method);
static DC_LoadPlayerSkinUnlock_t g_orig_DC_LoadPlayerSkinUnlock = nullptr;
static bool Hooked_DC_LoadPlayerSkinUnlock(void* self, int character, int skinID,
                                           void* method) {
    bool orig = g_orig_DC_LoadPlayerSkinUnlock
                    ? g_orig_DC_LoadPlayerSkinUnlock(self, character, skinID, method)
                    : false;
    if (g_reenter.load()) return orig;
    if (!g_skin.load()) return orig;
    if (MemoFirst(1, character * 64 + (skinID & 63))) {
        g_reenter.store(true);
        CallVoid2I(ClassCached("DataController"), "SavePlayerSkinUnlock",
                   self, character, skinID);
        g_reenter.store(false);
    }
    return true;
}                                                                  // RVA 0x390C4EC

typedef bool (*DC_LoadLobbySkinUnlock_t)(void* self, int lobbySkin, void* method);
static DC_LoadLobbySkinUnlock_t g_orig_DC_LoadLobbySkinUnlock = nullptr;
static bool Hooked_DC_LoadLobbySkinUnlock(void* self, int lobbySkin, void* method) {
    bool orig = g_orig_DC_LoadLobbySkinUnlock
                    ? g_orig_DC_LoadLobbySkinUnlock(self, lobbySkin, method)
                    : false;
    if (g_reenter.load()) return orig;
    if (!g_skin.load()) return orig;
    if (MemoFirst(2, lobbySkin)) {
        g_reenter.store(true);
        CallVoid1(ClassCached("DataController"), "SaveLobbySkinUnlock", self, lobbySkin);
        g_reenter.store(false);
    }
    return true;
}                                                                  // RVA 0x390C7FC

/* --- 10. 解锁全武器 --------------------------------------------------- */
typedef bool (*DC_LoadPlayerWeaponUnlock_t)(void* self, int character, int weaponID,
                                            void* method);
static DC_LoadPlayerWeaponUnlock_t g_orig_DC_LoadPlayerWeaponUnlock = nullptr;
static bool Hooked_DC_LoadPlayerWeaponUnlock(void* self, int character, int weaponID,
                                             void* method) {
    bool orig = g_orig_DC_LoadPlayerWeaponUnlock
                    ? g_orig_DC_LoadPlayerWeaponUnlock(self, character, weaponID, method)
                    : false;
    if (g_reenter.load()) return orig;
    if (!g_weapon.load()) return orig;
    if (MemoFirst(3, character * 256 + (weaponID & 255))) {
        g_reenter.store(true);
        CallVoid2I(ClassCached("DataController"), "SavePlayerWeaponUnlock",
                   self, character, weaponID);
        g_reenter.store(false);
    }
    return true;
}                                                                  // RVA 0x38FE118

/* --- 11. 解锁全宠物 --------------------------------------------------- */
/* ★ 用户实测 bug：v2 只让 LoadPetUnlock 恒返 true（撒谎、不写档）⇒ 装备后宠物 ID
 *   落进存档，重启关闭功能 → "拥有集合里没有、存档却引用" → 弹「尚未拥有该宠物」
 *   并卡死 UI。现调游戏自己的 SavePetUnlock(petID) 真落库（DataController
 *   内部即 petUnlockData HashSet<int>.Add + SavePetUnlockData）。 */
typedef bool (*DC_LoadPetUnlock_t)(void* self, int petID, void* method);
static DC_LoadPetUnlock_t g_orig_DC_LoadPetUnlock = nullptr;
static bool Hooked_DC_LoadPetUnlock(void* self, int petID, void* method) {
    bool orig = g_orig_DC_LoadPetUnlock
                    ? g_orig_DC_LoadPetUnlock(self, petID, method)
                    : false;
    if (g_reenter.load()) return orig;
    if (!g_pet.load()) return orig;
    if (MemoFirst(4, petID)) {
        g_reenter.store(true);
        CallVoid1(ClassCached("DataController"), "SavePetUnlock", self, petID);
        g_reenter.store(false);
    }
    return true;
}                                                                  // RVA 0x390B060

/* --- 12. 解锁全神器 --------------------------------------------------- */
JFQS_DEF_BOOL3(AM_CheckHaveArtifact,    g_artifact.load())        // 0x3404990
JFQS_DEF_BOOL2(AM_CheckHaveArtifactAll, g_artifact.load())        // 0x33F6EA8

/* static 方法：签名不含 this */
typedef bool (*AM_GetIsDefaultUnlockArtifact_t)(int id, void* method);
static AM_GetIsDefaultUnlockArtifact_t g_orig_AM_GetIsDefaultUnlockArtifact = nullptr;
static bool Hooked_AM_GetIsDefaultUnlockArtifact(int id, void* method) {
    bool orig = g_orig_AM_GetIsDefaultUnlockArtifact
                    ? g_orig_AM_GetIsDefaultUnlockArtifact(id, method)
                    : false;
    if (g_reenter.load()) return orig;
    if (!g_artifact.load()) return orig;
    return true;
}                                                                  // RVA 0x340FB54

/* --- 13. 解锁全部技能 ------------------------------------------------- */
JFQS_DEF_BOOL1(UM_UnlockSkillsCheck, g_skillUnlock.load())        // 0x3A39380
/* CheckUnlock 另有 (UnlockItems,int) argc=2 重载 ⇒ 靠 argc 消歧取 argc=1。
 * 注：这是通用解锁判定，可能连带解锁神器 —— 两者都是解锁开关，已接受。 */
JFQS_DEF_BOOL1(UM_CheckUnlock,       g_skillUnlock.load())        // 0x3A38AD4

/* --- 14. 解锁协同 ----------------------------------------------------- */
JFQS_DEF_BOOL1(SYN_CheckHaveSynergy,            g_synergy.load()) // 0x3837178
JFQS_DEF_BOOL1(SYN_CheckUseSynergy,             g_synergy.load()) // 0x3835FB0
JFQS_DEF_BOOL1(SYN_CheckHaveItemSynergy,        g_synergy.load()) // 0x383729C
JFQS_DEF_BOOL1(SYN_CheckCanObtainSynergyBySkill,g_synergy.load()) // 0x3837704

/* --- 15. 百分百改造成功 ----------------------------------------------- */
/* StatueofEnhancement.GetChance() = base + slope * level（0..1 概率）
 * hook 返 1.0f ⇒ 必成。
 * 死路备忘：ArtifactManager.UpgradeChance() @ 0x34010A4 是死桩
 *          （fmov s0,wzr; mov x0,xzr; ret 恒返 false），不是目标。 */
JFQS_DEF_FCONST0(SOE_GetChance, g_statue.load(), 1.0f)            // 0x38AE260

/* 槽表：全部按名挂载（rva=0），ns 全空。一 slot 一具体方法，
 * 禁止 hook 共享 invoke 入口。 */
static const HookSlot kSlots[] = {
    /* 1  伤害倍率 */
    {"PA_GetNormalAttackDamage",    0, 0, "", "PlayerAttackColl",   "GetNormalAttackDamage",     0, (void*)Hooked_PA_GetNormalAttackDamage,    (void**)&g_orig_PA_GetNormalAttackDamage},
    {"Proj_GetNormalAttackDamage",  0, 0, "", "CharacterProjectile","GetNormalAttackDamage",     0, (void*)Hooked_Proj_GetNormalAttackDamage,  (void**)&g_orig_Proj_GetNormalAttackDamage},
    /* 2  无敌 */
    {"PD_TakeDamage",               0, 0, "", "PlayerData",         "TakeDamage",                2, (void*)Hooked_PD_TakeDamage,               (void**)&g_orig_PD_TakeDamage},
    /* 3  无限货币 */
    {"DC_LoadGem",                  0, 0, "", "DataController",     "LoadGem",                   0, (void*)Hooked_DC_LoadGem,                  (void**)&g_orig_DC_LoadGem},
    {"DC_LoadSoulStone",            0, 0, "", "DataController",     "LoadSoulStone",             0, (void*)Hooked_DC_LoadSoulStone,            (void**)&g_orig_DC_LoadSoulStone},
    {"DC_LoadPrayerStone",          0, 0, "", "DataController",     "LoadPrayerStone",           0, (void*)Hooked_DC_LoadPrayerStone,          (void**)&g_orig_DC_LoadPrayerStone},
    {"DC_LoadBreachStone",          0, 0, "", "DataController",     "LoadBreachStone",           0, (void*)Hooked_DC_LoadBreachStone,          (void**)&g_orig_DC_LoadBreachStone},
    {"DC_LoadMileagePoint",         0, 0, "", "DataController",     "LoadMileagePoint",          0, (void*)Hooked_DC_LoadMileagePoint,         (void**)&g_orig_DC_LoadMileagePoint},
    {"PD_UseGold",                  0, 0, "", "PlayerData",         "UseGold",                   1, (void*)Hooked_PD_UseGold,                  (void**)&g_orig_PD_UseGold},
    /* 4  移动速度 */
    {"PM_GetMoveSpeedPower",        0, 0, "", "PlayerMovement",     "GetMoveSpeedPower",         0, (void*)Hooked_PM_GetMoveSpeedPower,        (void**)&g_orig_PM_GetMoveSpeedPower},
    /* 5  技能无限 */
    {"PSM_CheckMainSkillAble",      0, 0, "", "PlayerSkillManager", "CheckMainSkillAble",        0, (void*)Hooked_PSM_CheckMainSkillAble,      (void**)&g_orig_PSM_CheckMainSkillAble},
    {"PSM_CheckCanAllSkill",        0, 0, "", "PlayerSkillManager", "CheckCanAllSkill",          0, (void*)Hooked_PSM_CheckCanAllSkill,        (void**)&g_orig_PSM_CheckCanAllSkill},
    {"PSM_CheckCanSubSkillByCoolTime",0,0,"", "PlayerSkillManager", "CheckCanSubSkillByCoolTime",0, (void*)Hooked_PSM_CheckCanSubSkillByCoolTime,(void**)&g_orig_PSM_CheckCanSubSkillByCoolTime},
    {"PSM_CheckCanSubSkill",        0, 0, "", "PlayerSkillManager", "CheckCanSubSkill",          0, (void*)Hooked_PSM_CheckCanSubSkill,        (void**)&g_orig_PSM_CheckCanSubSkill},
    {"PSM_CheckCanAttackSkill",     0, 0, "", "PlayerSkillManager", "CheckCanAttackSkill",       0, (void*)Hooked_PSM_CheckCanAttackSkill,     (void**)&g_orig_PSM_CheckCanAttackSkill},
    {"PSM_CheckCanBuffSkill",       0, 0, "", "PlayerSkillManager", "CheckCanBuffSkill",         0, (void*)Hooked_PSM_CheckCanBuffSkill,       (void**)&g_orig_PSM_CheckCanBuffSkill},
    {"PSM_CheckSkillReady",         0, 0, "", "PlayerSkillManager", "CheckSkillReady",           1, (void*)Hooked_PSM_CheckSkillReady,         (void**)&g_orig_PSM_CheckSkillReady},
    {"SM_SkillCoolTime",            0, 0, "", "SkillManager",       "SkillCoolTime",             3, (void*)Hooked_SM_SkillCoolTime,            (void**)&g_orig_SM_SkillCoolTime},
    {"SM_SkillCoolTimeRealtime",    0, 0, "", "SkillManager",       "SkillCoolTimeRealtime",     3, (void*)Hooked_SM_SkillCoolTimeRealtime,    (void**)&g_orig_SM_SkillCoolTimeRealtime},
    {"PSM_CheckCanSubSkillByAttack",0, 0, "", "PlayerSkillManager", "CheckCanSubSkillByAttack",  0, (void*)Hooked_PSM_CheckCanSubSkillByAttack,(void**)&g_orig_PSM_CheckCanSubSkillByAttack},
    {"PSM_CheckUse",                0, 0, "", "PlayerSkillManager", "CheckUse",                  1, (void*)Hooked_PSM_CheckUse,                (void**)&g_orig_PSM_CheckUse},
    /* 5b 武器技能冷却（v5 追加，见第 5 节注释） */
    {"PM_get_WeaponSkillCoolTime",  0, 0, "", "PlayerMovement",     "get_WeaponSkillCoolTime",   0, (void*)Hooked_PM_get_WeaponSkillCoolTime,  (void**)&g_orig_PM_get_WeaponSkillCoolTime},
    {"PM_GetWeaponSkillCoolTime",   0, 0, "", "PlayerMovement",     "GetWeaponSkillCoolTime",    0, (void*)Hooked_PM_GetWeaponSkillCoolTime,   (void**)&g_orig_PM_GetWeaponSkillCoolTime},
    {"WS_GetWeaponSkillCoolTime",   0, 0, "", "WeaponSkill",        "GetWeaponSkillCoolTime",    0, (void*)Hooked_WS_GetWeaponSkillCoolTime,   (void**)&g_orig_WS_GetWeaponSkillCoolTime},
    {"CIS_GetWeaponSkillCoolTime",  0, 0, "", "CharacterInfoSelector","GetWeaponSkillCoolTime",  0, (void*)Hooked_CIS_GetWeaponSkillCoolTime,  (void**)&g_orig_CIS_GetWeaponSkillCoolTime},
    {"HR_GetWeaponSkillCoolTimeFillAmount",0,0,"","HighRollerScript","GetWeaponSkillCoolTimeFillAmount",0,(void*)Hooked_HR_GetWeaponSkillCoolTimeFillAmount,(void**)&g_orig_HR_GetWeaponSkillCoolTimeFillAmount},
    {"SUM_get_WeaponSkillCoolTime", 0, 0, "", "SummonerScript",     "get_WeaponSkillCoolTime",   0, (void*)Hooked_SUM_get_WeaponSkillCoolTime, (void**)&g_orig_SUM_get_WeaponSkillCoolTime},
    {"SUM_GetWeaponSkillCoolTime",  0, 0, "", "SummonerScript",     "GetWeaponSkillCoolTime",    0, (void*)Hooked_SUM_GetWeaponSkillCoolTime,  (void**)&g_orig_SUM_GetWeaponSkillCoolTime},
    /* 5c 武器技能冷却写入点：v5 已改为真拦截（[this+0x588] 归零，跳过 orig） */
    {"PM_SetWeaponSkillCoolTime",   0, 0, "", "PlayerMovement",     "SetWeaponSkillCoolTime",    1, (void*)Hooked_PM_SetWeaponSkillCoolTime,   (void**)&g_orig_PM_SetWeaponSkillCoolTime},
    {"PM_SetPlusWeaponSkillCoolTime",0,0, "", "PlayerMovement",     "SetPlusWeaponSkillCoolTime",1, (void*)Hooked_PM_SetPlusWeaponSkillCoolTime,(void**)&g_orig_PM_SetPlusWeaponSkillCoolTime},
    /* 6  无限跳跃（v5 定案：只改数量上限；CanMoreJump/CheckCanJump/PlayerJumpCooltime 已移除） */
    {"PM_GetMaxMoreJumpCount",      0, 0, "", "PlayerMovement",     "GetMaxMoreJumpCount",       0, (void*)Hooked_PM_GetMaxMoreJumpCount,      (void**)&g_orig_PM_GetMaxMoreJumpCount},
    /* 7  免广告 */
    {"GS_IsNoAd",                   0, 0, "", "GameSetting",        "IsNoAd",                    0, (void*)Hooked_GS_IsNoAd,                   (void**)&g_orig_GS_IsNoAd},
    {"DC_LoadRemoveAds",            0, 0, "", "DataController",     "LoadRemoveAds",             0, (void*)Hooked_DC_LoadRemoveAds,            (void**)&g_orig_DC_LoadRemoveAds},
    {"ADX_ShowRewardedAd",          0, 0, "", "ADXManager",         "ShowRewardedAd",            1, (void*)Hooked_ADX_ShowRewardedAd,          (void**)&g_orig_ADX_ShowRewardedAd},
    {"ADX_ShowInterstitialAd",      0, 0, "", "ADXManager",         "ShowInterstitialAd",        1, (void*)Hooked_ADX_ShowInterstitialAd,      (void**)&g_orig_ADX_ShowInterstitialAd},
    /* 7b 真广告桥（v5 追加，见第 7 节注释） */
    {"MRSDK_ShowReward6",           0, 0, "", "MarSDKManager",      "ShowReward",                6, (void*)Hooked_MRSDK_ShowReward6,           (void**)&g_orig_MRSDK_ShowReward6},
    {"MRSDK_ShowReward7",           0, 0, "", "MarSDKManager",      "ShowReward",                7, (void*)Hooked_MRSDK_ShowReward7,           (void**)&g_orig_MRSDK_ShowReward7},
    /* 8  解锁全角色 */
    {"DC_LoadPlayerUnlock",         0, 0, "", "DataController",     "LoadPlayerUnlock",          1, (void*)Hooked_DC_LoadPlayerUnlock,         (void**)&g_orig_DC_LoadPlayerUnlock},
    {"DC_IsCharacterUnlockedForPurchase",0,0,"","DataController",   "IsCharacterUnlockedForPurchase",2,(void*)Hooked_DC_IsCharacterUnlockedForPurchase,(void**)&g_orig_DC_IsCharacterUnlockedForPurchase},
    {"DC_IsAllCharacterOwned",      0, 0, "", "DataController",     "IsAllCharacterOwned",       0, (void*)Hooked_DC_IsAllCharacterOwned,      (void**)&g_orig_DC_IsAllCharacterOwned},
    /* 9  解锁全皮肤 */
    {"DC_LoadPlayerSkinUnlock",     0, 0, "", "DataController",     "LoadPlayerSkinUnlock",      2, (void*)Hooked_DC_LoadPlayerSkinUnlock,     (void**)&g_orig_DC_LoadPlayerSkinUnlock},
    {"DC_LoadLobbySkinUnlock",      0, 0, "", "DataController",     "LoadLobbySkinUnlock",       1, (void*)Hooked_DC_LoadLobbySkinUnlock,      (void**)&g_orig_DC_LoadLobbySkinUnlock},
    /* 10 解锁全武器 */
    {"DC_LoadPlayerWeaponUnlock",   0, 0, "", "DataController",     "LoadPlayerWeaponUnlock",    2, (void*)Hooked_DC_LoadPlayerWeaponUnlock,   (void**)&g_orig_DC_LoadPlayerWeaponUnlock},
    /* 11 解锁全宠物 */
    {"DC_LoadPetUnlock",            0, 0, "", "DataController",     "LoadPetUnlock",             1, (void*)Hooked_DC_LoadPetUnlock,            (void**)&g_orig_DC_LoadPetUnlock},
    /* 12 解锁全神器 */
    {"AM_CheckHaveArtifact",        0, 0, "", "ArtifactManager",    "CheckHaveArtifact",         3, (void*)Hooked_AM_CheckHaveArtifact,        (void**)&g_orig_AM_CheckHaveArtifact},
    {"AM_CheckHaveArtifactAll",     0, 0, "", "ArtifactManager",    "CheckHaveArtifactAll",      2, (void*)Hooked_AM_CheckHaveArtifactAll,     (void**)&g_orig_AM_CheckHaveArtifactAll},
    {"AM_GetIsDefaultUnlockArtifact",0,0, "", "ArtifactManager",    "GetIsDefaultUnlockArtifact",1, (void*)Hooked_AM_GetIsDefaultUnlockArtifact,(void**)&g_orig_AM_GetIsDefaultUnlockArtifact},
    /* 13 解锁全部技能 */
    {"UM_UnlockSkillsCheck",        0, 0, "", "UnlockManager",      "UnlockSkillsCheck",         1, (void*)Hooked_UM_UnlockSkillsCheck,        (void**)&g_orig_UM_UnlockSkillsCheck},
    {"UM_CheckUnlock",              0, 0, "", "UnlockManager",      "CheckUnlock",               1, (void*)Hooked_UM_CheckUnlock,              (void**)&g_orig_UM_CheckUnlock},
    /* 14 解锁协同 */
    {"SYN_CheckHaveSynergy",        0, 0, "", "SynergyManager",     "CheckHaveSynergy",          1, (void*)Hooked_SYN_CheckHaveSynergy,        (void**)&g_orig_SYN_CheckHaveSynergy},
    {"SYN_CheckUseSynergy",         0, 0, "", "SynergyManager",     "CheckUseSynergy",           1, (void*)Hooked_SYN_CheckUseSynergy,         (void**)&g_orig_SYN_CheckUseSynergy},
    {"SYN_CheckHaveItemSynergy",    0, 0, "", "SynergyManager",     "CheckHaveItemSynergy",      1, (void*)Hooked_SYN_CheckHaveItemSynergy,    (void**)&g_orig_SYN_CheckHaveItemSynergy},
    {"SYN_CheckCanObtainSynergyBySkill",0,0,"","SynergyManager",    "CheckCanObtainSynergyBySkill",1,(void*)Hooked_SYN_CheckCanObtainSynergyBySkill,(void**)&g_orig_SYN_CheckCanObtainSynergyBySkill},
    /* 15 百分百改造成功 */
    {"SOE_GetChance",               0, 0, "", "StatueofEnhancement","GetChance",                 0, (void*)Hooked_SOE_GetChance,               (void**)&g_orig_SOE_GetChance},
};

static const int kSlotsCount = static_cast<int>(sizeof(kSlots) / sizeof(kSlots[0]));
static const int kSlotsMax   = 64;

/* ★ 槽位熔断表：装钩时炸过（收到致命信号）的槽位永久拉黑，绝不重试 ——
 *   否则下一次 Tick 会拿同一个地址再撞一次，闪退变成每 5 秒一次。
 *   只在 g_installMutex 下读写。 */
static bool g_slotDead[kSlotsMax];

/* ------------------------------------------------- ⑥ 安装与惰性同步 */
/* 只在 g_installMutex 保护下调用。 */
static int InstallBattleHooks() {
    int ok = 0;
    int dead = 0;
    for (int i = 0; i < kSlotsCount && i < kSlotsMax; ++i) {
        if (g_slotDead[i]) { ++dead; continue; }          // 熔断，跳过
        if (HookSlotInstall(kSlots[i])) {
            ++ok;
        } else if (g_lastFatal) {
            g_slotDead[i] = true;                          // 永久拉黑
            LOGW(TPL_LOG " slot[%d] %s 已熔断（装钩期致命信号），本次会话不再重试",
                 i, kSlots[i].tag);
        }
    }
    if (dead > 0) {
        LOGW(TPL_LOG " install pass: ok=%d dead=%d/%d", ok, dead, kSlotsCount);
    }
    if (ok > 0) g_anyHooked.store(true);
    return ok;
}

/* 任一功能开（含滑块 > 1）才安装；全关 = 0 hook */
static bool AnyFeatureOn() {
    return g_dmgMul.load()  > 1
        || g_moveMul.load() > 1
        || g_inv.load()      || g_money.load()     || g_skill.load()
        || g_jump.load()     || g_noad.load()      || g_char.load()
        || g_skin.load()     || g_weapon.load()    || g_pet.load()
        || g_artifact.load() || g_skillUnlock.load()
        || g_synergy.load()  || g_statue.load();
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    if (!AnyFeatureOn()) return;                 // 全关：一个钩子都不装
    /* 快路径：已全部装齐 —— 纯 atomic 读，不加锁（Tick 每 5s 走这里） */
    if (g_installedCount.load() >= kSlotsCount) return;

    /* ★ 串行化：菜单 UI 线程 与 游戏线程 会同时到达。
     *   没有这把锁 = 同一地址被 DobbyHook 两次 = DobbyCodePatch 越界 memcpy
     *   = SIGSEGV(SEGV_ACCERR)，实测在 DobbyHook+216 崩。 */
    std::lock_guard<std::mutex> lk(g_installMutex);
    if (g_installedCount.load() >= kSlotsCount) return;   // double-check
    g_installedCount.store(InstallBattleHooks());
}

static int ClampInt(int v, int lo, int hi) {
    if (v < lo) return lo;
    if (v > hi) return hi;
    return v;
}

/* --------------------------------- ⑦ 注册表接口（签名不得修改） */
bool Jfqs_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;

    g_libReady.store(true);
    SyncHooks();                                  // 全关则保持 0 hook
    LOGI(TPL_LOG " lib ready (dual-path, lazy hooks)");
    return true;
}

bool Jfqs_IsReady() {
    return g_libReady.load();
}

void Jfqs_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case JFQS_FEAT_INV:          g_inv.store(enabled);         break;
        case JFQS_FEAT_MONEY:        g_money.store(enabled);       break;
        case JFQS_FEAT_SKILL:        g_skill.store(enabled);       break;
        case JFQS_FEAT_JUMP:         g_jump.store(enabled);        break;
        case JFQS_FEAT_NOAD:         g_noad.store(enabled);        break;
        case JFQS_FEAT_CHAR:         g_char.store(enabled);        break;
        case JFQS_FEAT_SKIN:         g_skin.store(enabled);        break;
        case JFQS_FEAT_WEAPON:       g_weapon.store(enabled);      break;
        case JFQS_FEAT_PET:          g_pet.store(enabled);         break;
        case JFQS_FEAT_ARTIFACT:     g_artifact.store(enabled);    break;
        case JFQS_FEAT_UNLOCKSKILL:  g_skillUnlock.store(enabled); break;
        case JFQS_FEAT_SYNERGY:      g_synergy.store(enabled);     break;
        case JFQS_FEAT_ENHANCE:      g_statue.store(enabled);      break;
        default: break;
    }
    SyncHooks();
}

void Jfqs_OnValueChanged(int featNum, int value) {
    switch (featNum) {
        case JFQS_FEAT_DMG:
            g_dmgMul.store(ClampInt(value, 1, JFQS_DMG_MUL_MAX));
            break;
        case JFQS_FEAT_MOVE:
            g_moveMul.store(ClampInt(value, 1, JFQS_MOVE_MUL_MAX));
            break;
        default: break;
    }
    SyncHooks();
}

/* 只做同步开关 + 兜底安装。
 * ★ 禁止在这里 SafeProbe / 扫堆 / 遍历对象 / 逐对象写。 */
void Jfqs_Tick(const bool* features, const int* values) {
    if (features) {
        g_inv.store(features[JFQS_FEAT_INV]);
        g_money.store(features[JFQS_FEAT_MONEY]);
        g_skill.store(features[JFQS_FEAT_SKILL]);
        g_jump.store(features[JFQS_FEAT_JUMP]);
        g_noad.store(features[JFQS_FEAT_NOAD]);
        g_char.store(features[JFQS_FEAT_CHAR]);
        g_skin.store(features[JFQS_FEAT_SKIN]);
        g_weapon.store(features[JFQS_FEAT_WEAPON]);
        g_pet.store(features[JFQS_FEAT_PET]);
        g_artifact.store(features[JFQS_FEAT_ARTIFACT]);
        g_skillUnlock.store(features[JFQS_FEAT_UNLOCKSKILL]);
        g_synergy.store(features[JFQS_FEAT_SYNERGY]);
        g_statue.store(features[JFQS_FEAT_ENHANCE]);
    }
    if (values) {
        g_dmgMul.store(ClampInt(values[JFQS_FEAT_DMG],  1, JFQS_DMG_MUL_MAX));
        g_moveMul.store(ClampInt(values[JFQS_FEAT_MOVE], 1, JFQS_MOVE_MUL_MAX));
    }

    if (!g_libReady.load()) {
        if (isLibraryLoaded(TPL_TARGET_LIB)) {
            g_libReady.store(true);
            SyncHooks();
        }
        return;
    }
    /* 兜底：首次挂载时类可能尚未注册，这里补装（幂等，已挂的直接跳过） */
    if (AnyFeatureOn() && g_installedCount.load() < kSlotsCount) {
        SyncHooks();
    }
}

void Jfqs_ForceClear() {
    g_dmgMul.store(1);
    g_moveMul.store(1);
    g_inv.store(false);
    g_money.store(false);
    g_skill.store(false);
    g_jump.store(false);
    g_noad.store(false);
    g_char.store(false);
    g_skin.store(false);
    g_weapon.store(false);
    g_pet.store(false);
    g_artifact.store(false);
    g_skillUnlock.store(false);
    g_synergy.store(false);
    g_statue.store(false);
}
