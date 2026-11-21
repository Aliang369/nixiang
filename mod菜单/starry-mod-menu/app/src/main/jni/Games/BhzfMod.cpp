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

#include "BhzfMod.h"

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

// 生成器把 [bhzf] 替换为本游戏标签（如 [jfqs]），日志过滤用 adb logcat -s Mod_Menu
#define TPL_LOG "[bhzf]"

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
static std::atomic<bool> g_reenter{false};

typedef void (*TakeDamage_t)(void* self, void* damageData, void* method);
static TakeDamage_t g_origTakeDamage = nullptr;

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
//  ⑥ 热更程序集边界换字节（Assembly.Load(byte[])）
//     说明：主战斗逻辑在 HybridCLR 解释层（mygame 程序集），native 不 hook 方法，
//     改为在程序集装载边界把「已核版本」的字节替换成 Cecil 补丁版；
//     补丁体命中条件 = 菜单开关（god/kill），关闭时走原逻辑 ⇒ 无需还原代码。
//     只按 sha256 白名单替换；未命中/长度不符 ⇒ 原样放行（不崩、不闪退）。
// ============================================================================
#include <sys/stat.h>
#include <unistd.h>

extern "C" const unsigned char _binary_mygame_bhzf_dev_dll_start[];
extern "C" const unsigned char _binary_mygame_bhzf_dev_dll_end[];
extern "C" const unsigned char _binary_mygame_bhzf_base_dll_start[];
extern "C" const unsigned char _binary_mygame_bhzf_base_dll_end[];

// 每个「已核官方版本」→ 同版本补丁体，一一绑定（禁止拿设备补丁顶基线）
struct PatchEntry {
    const char* tag;
    size_t      officialLen;
    const char* officialSha;
    const char* patchedSha;
    const unsigned char* blob;
    size_t      blobLen;
};

static const PatchEntry kPatches[] = {
    { "dev-18,655,744",  18655744u,
      "f44f75b8fc2be6e14ea073ad8513ff167ec45ae4091cbce97c4359d6983429ea",
      "81f7963951f2244c5b8df19ffd6be1ebe43b7f619cf1937e4dae66907880b064",
      _binary_mygame_bhzf_dev_dll_start,
      (size_t)(_binary_mygame_bhzf_dev_dll_end - _binary_mygame_bhzf_dev_dll_start) },
    { "base-18,009,088", 18009088u,
      "e58aa7ef725f776003404022f12a570eab33da32eb9fc83def44ada0f56d5bfb",
      "7f2d381184e7cb14380a9f83c87af67331bc43473260e02a8d59868ed69ef8c4",
      _binary_mygame_bhzf_base_dll_start,
      (size_t)(_binary_mygame_bhzf_base_dll_end - _binary_mygame_bhzf_base_dll_start) },
};
static const int kPatchCount = (int)(sizeof(kPatches) / sizeof(kPatches[0]));

// ---- 极简 SHA-256（只用于版本绑定，不做安全用途） ----
struct Sha256 {
    uint32_t h[8]; uint64_t len; unsigned char buf[64]; size_t n;
    static uint32_t ror(uint32_t x, int c) { return (x >> c) | (x << (32 - c)); }
    void init() {
        static const uint32_t iv[8] = {0x6a09e667,0xbb67ae85,0x3c6ef372,0xa54ff53a,0x510e527f,0x9b05688c,0x1f83d9ab,0x5be0cd19};
        memcpy(h, iv, sizeof(h)); len = 0; n = 0;
    }
    void block(const unsigned char* p) {
        static const uint32_t K[64] = {
            0x428a2f98,0x71374491,0xb5c0fbcf,0xe9b5dba5,0x3956c25b,0x59f111f1,0x923f82a4,0xab1c5ed5,
            0xd807aa98,0x12835b01,0x243185be,0x550c7dc3,0x72be5d74,0x80deb1fe,0x9bdc06a7,0xc19bf174,
            0xe49b69c1,0xefbe4786,0x0fc19dc6,0x240ca1cc,0x2de92c6f,0x4a7484aa,0x5cb0a9dc,0x76f988da,
            0x983e5152,0xa831c66d,0xb00327c8,0xbf597fc7,0xc6e00bf3,0xd5a79147,0x06ca6351,0x14292967,
            0x27b70a85,0x2e1b2138,0x4d2c6dfc,0x53380d13,0x650a7354,0x766a0abb,0x81c2c92e,0x92722c85,
            0xa2bfe8a1,0xa81a664b,0xc24b8b70,0xc76c51a3,0xd192e819,0xd6990624,0xf40e3585,0x106aa070,
            0x19a4c116,0x1e376c08,0x2748774c,0x34b0bcb5,0x391c0cb3,0x4ed8aa4a,0x5b9cca4f,0x682e6ff3,
            0x748f82ee,0x78a5636f,0x84c87814,0x8cc70208,0x90befffa,0xa4506ceb,0xbef9a3f7,0xc67178f2};
        uint32_t w[64];
        for (int i = 0; i < 16; ++i) w[i] = (p[i*4]<<24)|(p[i*4+1]<<16)|(p[i*4+2]<<8)|p[i*4+3];
        for (int i = 16; i < 64; ++i) {
            uint32_t s0 = ror(w[i-15],7) ^ ror(w[i-15],18) ^ (w[i-15] >> 3);
            uint32_t s1 = ror(w[i-2],17) ^ ror(w[i-2],19) ^ (w[i-2] >> 10);
            w[i] = w[i-16] + s0 + w[i-7] + s1;
        }
        uint32_t a=h[0],b=h[1],c=h[2],d=h[3],e=h[4],f=h[5],g=h[6],hh=h[7];
        for (int i = 0; i < 64; ++i) {
            uint32_t S1 = ror(e,6) ^ ror(e,11) ^ ror(e,25);
            uint32_t ch = (e & f) ^ (~e & g);
            uint32_t t1 = hh + S1 + ch + K[i] + w[i];
            uint32_t S0 = ror(a,2) ^ ror(a,13) ^ ror(a,22);
            uint32_t mj = (a & b) ^ (a & c) ^ (b & c);
            uint32_t t2 = S0 + mj;
            hh=g; g=f; f=e; e=d+t1; d=c; c=b; b=a; a=t1+t2;
        }
        h[0]+=a; h[1]+=b; h[2]+=c; h[3]+=d; h[4]+=e; h[5]+=f; h[6]+=g; h[7]+=hh;
    }
    void update(const unsigned char* p, size_t l) {
        len += (uint64_t)l * 8;
        while (l) {
            size_t take = 64 - n; if (take > l) take = l;
            memcpy(buf + n, p, take); n += take; p += take; l -= take;
            if (n == 64) { block(buf); n = 0; }
        }
    }
    void hex(char out[65]) {
        unsigned char pad[128]; size_t pn = 0;
        uint64_t bits = len;
        memset(pad, 0, sizeof(pad));
        pad[pn++] = 0x80;
        size_t rem = (n + 1) % 64;
        size_t zeros = (rem <= 56) ? (56 - rem) : (120 - rem);
        pn += zeros;
        for (int i = 7; i >= 0; --i) pad[pn++] = (unsigned char)((bits >> (i*8)) & 0xff);
        update(pad, pn);
        static const char* hx = "0123456789abcdef";
        for (int i = 0; i < 8; ++i)
            for (int j = 0; j < 4; ++j) {
                unsigned char b = (unsigned char)((h[i] >> (24 - j*8)) & 0xff);
                out[i*8 + j*2] = hx[b >> 4];
                out[i*8 + j*2 + 1] = hx[b & 0xf];
            }
        out[64] = 0;
    }
};

static void Sha256Of(const unsigned char* p, size_t len, char out[65]) {
    Sha256 s; s.init(); s.update(p, len); s.hex(out);
}

// ---- 开关持久化（native 与 C# 共用同一 ini） ----
static const char* kConfigDir  = "/data/user/0/com.bhzf.zbml/files";
static const char* kConfigPath = "/data/user/0/com.bhzf.zbml/files/starry_config.ini";

static void PersistConfig() {
    ::mkdir(kConfigDir, 0770);
    std::string s;
    s += "god=";  s += g_inv.load() ? "1" : "0"; s += "\n";
    s += "kill="; s += g_dmg.load() ? "1" : "0"; s += "\n";
    FILE* f = ::fopen(kConfigPath, "wb");
    if (!f) { LOGW(TPL_LOG " config open fail %s", kConfigPath); return; }
    ::fwrite(s.data(), 1, s.size(), f);
    ::fclose(f);
    ::chmod(kConfigPath, 0660);
}

static void* HookByNameRaw(const char* ns, const char* cls, const char* method, int argc);

// ---- Assembly.Load(byte[]) 边界替换 ----
typedef void* (*AssemblyLoad_t)(void* rawAssembly, void* method);
typedef void* (*array_new_t)(void* elementKlass, uintptr_t length);
static AssemblyLoad_t g_origAsmLoad = nullptr;
static std::atomic<bool> g_asmHooked{false};

static void* Hooked_AssemblyLoad(void* arr, void* method) {
    if (!arr || !g_origAsmLoad) return g_origAsmLoad ? g_origAsmLoad(arr, method) : nullptr;
    if (!Il2Cpp::IsManagedObjectPtr(arr)) {
        LOGW(TPL_LOG " 入参非托管对象 ✓ PASSTHROUGH");
        return g_origAsmLoad(arr, method);
    }
    {
        const char* cn = GetApi().class_name(GetApi().object_class(arr));
        if (!cn || (std::strcmp(cn, "System.Byte[]") != 0 && std::strcmp(cn, "Byte[]") != 0)) {
            LOGW(TPL_LOG " 入参类=%s 非 Byte[] ✓ PASSTHROUGH", cn ? cn : "(null)");
            return g_origAsmLoad(arr, method);
        }
    }

    const uintptr_t ulen = *reinterpret_cast<const uintptr_t*>(reinterpret_cast<const char*>(arr) + 0x18);
    if (ulen == 0 || ulen > (uintptr_t)64 * 1024 * 1024) {
        LOGW(TPL_LOG " 数组长度异常 %lu ✓ PASSTHROUGH", (unsigned long)ulen);
        return g_origAsmLoad(arr, method);
    }

    // 先按长度筛候选，只有候选才 hash（避免每次 Load 算 18 MB）
    const PatchEntry* hit = nullptr;
    bool idem = false;
    for (int i = 0; i < kPatchCount; ++i) {
        if (ulen == (uintptr_t)kPatches[i].officialLen) { hit = &kPatches[i]; break; }
        if (ulen == (uintptr_t)kPatches[i].blobLen) { hit = &kPatches[i]; idem = true; break; }
    }
    if (!hit) {
        LOGI(TPL_LOG " 非候选长度 len=%lu ✓ PASSTHROUGH", (unsigned long)ulen);
        return g_origAsmLoad(arr, method);
    }

    const unsigned char* data = reinterpret_cast<const unsigned char*>(reinterpret_cast<const char*>(arr) + 0x20);
    char sha[65] = {0};
    Sha256Of(data, (size_t)ulen, sha);

    if (idem && std::strcmp(sha, hit->patchedSha) == 0) {
        LOGI(TPL_LOG " 已是补丁体[%s] ✓ PASSTHROUGH", hit->tag);
        return g_origAsmLoad(arr, method);
    }
    if (!idem && std::strcmp(sha, hit->officialSha) == 0) {
        static array_new_t s_arrayNew = nullptr;
        static void* s_byteKlass = nullptr;
        if (!s_arrayNew) s_arrayNew = (array_new_t)Il2Cpp::ResolveExport("il2cpp_array_new");
        if (!s_byteKlass) {
            size_t n = 0;
            void** asms = GetApi().Assemblies(&n);
            for (size_t i = 0; asms && i < n; ++i) {
                void* img = GetApi().ImageOf(asms[i]);
                if (!img) continue;
                void* k = GetApi().ClassFromName(img, "System", "Byte");
                if (k) { s_byteKlass = k; break; }
            }
        }
        if (!s_arrayNew || !s_byteKlass) {
            LOGW(TPL_LOG " array_new/Byte 解析失败 ✓ PASSTHROUGH(alloc)");
            return g_origAsmLoad(arr, method);
        }
        void* newArr = s_arrayNew(s_byteKlass, (uintptr_t)hit->blobLen);
        if (!newArr) {
            LOGW(TPL_LOG " il2cpp_array_new 失败 ✓ PASSTHROUGH(alloc)");
            return g_origAsmLoad(arr, method);
        }
        memcpy(reinterpret_cast<char*>(newArr) + 0x20, hit->blob, hit->blobLen);
        LOGI(TPL_LOG " 换字节[%s]: official=%lu sha=%s → patched=%zu sha=%s（新数组，原数组未改）",
             hit->tag, (unsigned long)ulen, hit->officialSha, hit->blobLen, hit->patchedSha);
        return g_origAsmLoad(newArr, method);
    }

    LOGI(TPL_LOG " SHA 未命中[%s] len=%lu sha=%s ✓ PASSTHROUGH", hit->tag, (unsigned long)ulen, sha);
    return g_origAsmLoad(arr, method);
}

static bool ShaSelfTest() {
    char h[65];
    Sha256Of((const unsigned char*)"", 0, h);
    const bool ok1 = std::strcmp(h, "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855") == 0;
    Sha256Of((const unsigned char*)"abc", 3, h);
    const bool ok2 = std::strcmp(h, "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad") == 0;
    bool ok3 = true;
    for (int i = 0; i < kPatchCount; ++i) {
        char hb[65];
        Sha256Of(kPatches[i].blob, kPatches[i].blobLen, hb);
        const bool ok = std::strcmp(hb, kPatches[i].patchedSha) == 0;
        LOGI(TPL_LOG " SHA 自检[%s]: blob=%s expect=%s → %s", kPatches[i].tag, hb,
             kPatches[i].patchedSha, ok ? "PASS" : "FAIL");
        ok3 = ok3 && ok;
    }
    LOGI(TPL_LOG " SHA 自检: empty=%d abc=%d blobs=%d → %s", (int)ok1, (int)ok2, (int)ok3,
         (ok1 && ok2 && ok3) ? "PASS" : "FAIL");
    return ok1 && ok2 && ok3;
}

static bool InstallAsmLoadHook() {
    if (g_asmHooked.load()) return true;
    if (!ShaSelfTest()) { LOGW(TPL_LOG " SHA 自检失败 ⇒ 禁用替换（不装钩）"); return false; }
    std::lock_guard<std::mutex> lk(g_installMutex);
    if (g_asmHooked.load()) return true;
    if (!EnsureApi()) { LOGW(TPL_LOG " api 未就绪（Assembly 解析跳过，待 Tick 重试）"); return false; }
    void* k = FindClassNs("System.Reflection", "Assembly");
    if (!k) k = FindClassNs("", "Assembly");
    if (!k) k = GetApi().FindClassByShortName("Assembly");
    if (!k) {
        size_t n = 0;
        void** asms = GetApi().Assemblies(&n);
        for (size_t i = 0; asms && i < n && !k; ++i) {
            void* img = GetApi().ImageOf(asms[i]);
            if (img) k = GetApi().ClassFromName(img, "System.Reflection", "Assembly");
        }
        LOGW(TPL_LOG " Assembly 类未解析到（images=%zu）", n);
    }
    if (!k) return false;

    typedef void* (*get_param_t)(void* mi, int i);
    typedef void* (*get_ret_t)(void* mi);
    typedef uint32_t (*get_pcount_t)(void* mi);
    static get_param_t getParam = nullptr;
    static get_ret_t   getRet   = nullptr;
    static get_pcount_t getPCount = nullptr;
    if (!getPCount) getPCount = (get_pcount_t)Il2Cpp::ResolveExport("il2cpp_method_get_param_count");
    if (!getParam) getParam = (get_param_t)Il2Cpp::ResolveExport("il2cpp_method_get_param");
    if (!getRet)   getRet   = (get_ret_t)Il2Cpp::ResolveExport("il2cpp_method_get_return_type");

    // 候选：argc=1 且 参数类型名包含 "Byte[]"，返回类型名包含 "Assembly"；多候选即放弃
    void* picked = nullptr;
    int hits = 0;
    int total = 0;
    size_t nMethods = 0;
    void** methods = nullptr;
    typedef size_t (*class_methods_t)(void* klass, void** iter);
    static class_methods_t classMethods = nullptr;
    if (!classMethods) classMethods = (class_methods_t)Il2Cpp::ResolveExport("il2cpp_class_get_methods");
    if (classMethods && getParam && getRet) {
        void* iter = nullptr;
        void* m = nullptr;
        while ((m = (void*)classMethods(k, &iter)) != nullptr) {
            ++total;
            typedef const char* (*mname_t)(void* mi);
            static mname_t mname = nullptr;
            if (!mname) mname = (mname_t)Il2Cpp::ResolveExport("il2cpp_method_get_name");
            if (!mname || !mname(m) || std::strcmp(mname(m), "Load") != 0) continue;
            if (!getPCount || getPCount(m) != 1) continue;   // 严格 1 参数
            void* p0 = getParam(m, 0);
            static void* (*classFromType)(void*) = nullptr;
            static bool looked = false;
            if (!looked) { classFromType = (void* (*)(void*))Il2Cpp::ResolveExport("il2cpp_class_from_type"); looked = true; }
            auto typeNameOf = [](void* ty) -> const char* {
                if (!ty) return nullptr;
                if (GetApi().type_name) return GetApi().type_name(ty);
                if (classFromType) { void* c = classFromType(ty); return c ? GetApi().class_name(c) : nullptr; }
                return nullptr;
            };
            const char* pn = typeNameOf(p0);
            void* rt = getRet(m);
            const char* rn = typeNameOf(rt);
            typedef uint32_t (*mflags_t)(void* mi, uint32_t* iflags);
            static mflags_t mflags = nullptr;
            if (!mflags) mflags = (mflags_t)Il2Cpp::ResolveExport("il2cpp_method_get_flags");
            uint32_t iflags = 0;
            const bool isStatic = mflags ? ((mflags(m, &iflags) & 0x0010u) != 0) : false;
            bool pOk = isStatic && pn && (std::strcmp(pn, "System.Byte[]") == 0 || std::strcmp(pn, "Byte[]") == 0);
            bool rOk = rn && (std::strcmp(rn, "System.Reflection.Assembly") == 0 || std::strcmp(rn, "Assembly") == 0);
            if (pOk && rOk) { ++hits; picked = m; }
            LOGI(TPL_LOG " Load 候选: p0=%s ret=%s → %s", pn ? pn : "(null)", rn ? rn : "(null)",
                 (pOk && rOk) ? "命中" : "跳过");
        }
    }
    (void)nMethods; (void)methods;
    if (!picked || hits != 1) {
        LOGW(TPL_LOG " Assembly.Load(byte[]) 唯一候选失败 hits=%d total=%d ⇒ 不装钩", hits, total);
        return false;
    }
    LOGI(TPL_LOG " 选定 Assembly.Load(System.Byte[]) 唯一候选");
    void* mi = picked;
    void* fn = *reinterpret_cast<void**>(mi);
    if (!OkPtr(fn) || !Il2Cpp::IsExecutableRange((uintptr_t)fn, 16)) {
        LOGW(TPL_LOG " Assembly::Load 指针/可执行范围无效 ⇒ 不装钩");
        return false;
    }
    if (!DobbyHookGuarded("AsmLoad", fn, (void*)Hooked_AssemblyLoad, (void**)&g_origAsmLoad)) return false;
    g_asmHooked.store(true);
    LOGI(TPL_LOG " Assembly.Load(byte[]) hook 已安装（sha 白名单绑定）");
    return true;
}

// ============================================================================
//  ⑦ 按名解析（返回 MethodInfo*，供边界换字节使用）
// ============================================================================
static void* HookByNameRaw(const char* ns, const char* cls, const char* method, int argc) {
    if (!EnsureApi()) return nullptr;
    void* k = FindClassNs(ns, cls);
    if (!k) k = GetApi().FindClassByShortName(cls);
    if (!k) { LOGW(TPL_LOG " class miss %s", cls); return nullptr; }
    void* mi = GetApi().GetMethod(k, method, argc);
    if (!mi) LOGW(TPL_LOG " method miss %s.%s(argc=%d)", cls, method, argc);
    return mi;
}

// ============================================================================
//  ⑧ 对外接口（ModEntry.h 注册表契约，签名不可改）
// ============================================================================
bool Bhzf_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;

    InstallFaultGuard();
    g_libReady.store(true);
    InstallAsmLoadHook();     // 边界换字节：与功能开关无关，必须尽早安装
    PersistConfig();          // 初始同步（默认关）
    LOGI(TPL_LOG " lib ready; asm-hook=%d", (int)g_asmHooked.load());
    return true;
}

bool Bhzf_IsReady() { return g_libReady.load(); }

void Bhzf_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case BHZF_FEAT_DMG: g_dmg.store(enabled); break;
        case BHZF_FEAT_INV: g_inv.store(enabled); break;
        default: break;
    }
    PersistConfig();
    LOGI(TPL_LOG " feat=%d on=%d → ini god=%d kill=%d", featNum, (int)enabled,
         (int)g_inv.load(), (int)g_dmg.load());
}

void Bhzf_OnValueChanged(int /*featNum*/, int /*value*/) {}

void Bhzf_Tick(const bool* features, const int* /*values*/) {
    if (!features) return;

    const bool dmg = features[BHZF_FEAT_DMG];
    const bool inv = features[BHZF_FEAT_INV];
    if (dmg != g_dmg.load() || inv != g_inv.load()) {
        g_dmg.store(dmg);
        g_inv.store(inv);
        PersistConfig();
    }

    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) {
        InstallFaultGuard();
        g_libReady.store(true);
        LOGI(TPL_LOG " lib ready (via Tick fallback)");
    }
    if (g_libReady.load() && !g_asmHooked.load()) InstallAsmLoadHook();
}

void Bhzf_ForceClear() {
    g_dmg.store(false);
    g_inv.store(false);
    g_reenter.store(false);
    PersistConfig();
}
