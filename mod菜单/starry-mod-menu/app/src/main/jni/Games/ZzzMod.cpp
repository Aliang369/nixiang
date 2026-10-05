/* ============================================================================
 *  星光菜单游戏模块 — 绝区零 3.2.0 (vc429) arm64-v8a  [ZzzMod.cpp]
 *
 *  站点全部来自 10_docs/绝区零/星空版站点定位_20261005.md 的实测反汇编：
 *    ① 无敌     file 0x1151d138 = JNCLILAONEF.HLAOJKPGHIO（池鸳同款站点 0x11521138）
 *              池鸳 handler 过滤条件：事件 +0x16(u16)==0x85（伤害事件） 且
 *              +0x38(u32)==玩家 actorId → 不调原函数 ⇒ 本模块同款过滤
 *    ② 伤害倍率 file 0xc0269f0 入口 fmov s8,s0 ⇒ s0 即倍率入参；
 *              唯一调用者 file 0x8238550（先取 x0，再 fmov s0,s8 后 bl）⇒ (self, float)
 *    ③ 移动速度 file 0xfb2700c 函数内 3 条写指令（str s8,[x0,#0xf8] / str s8,[x0,#0x100] /
 *              strb w8,[x0,#0x104]），入口 hook 拿不到 s8 ⇒ 走 12B 原地桩 + 自带 handler
 *    ④ 定怪     file 0x19ce2164 = BehaviorManager.Tick（唯一调用者 0x1a7f0710）
 *    ⑤ 无限技能 v1 未定稿（候选 0x1391de04 三簇调用者，扣费点未确证）→ 只记开关，不挂 hook
 *
 *  ⚠️ 地址口径（2026-10-06 修正，MuMu 实战零命中事故）：
 *     站点用【文件偏移】空间；VA 必须取自 loader 真实模块（dl_iterate_phdr）的
 *     PT_LOAD 换算：base + p_vaddr + (fileOff - p_offset)（本例各段 p_vaddr-p_offset=0x4000）；
 *     【教训】MuMu 启动早期 /proc/self/maps 有游戏校验/预读同名临时分块映射（0x77xx 宿主堆区），
 *     按它装钩 = 装上了但永不命中。maps 分段口径仅作回退（真机/稳定期与 phdr 同值）。
 * ============================================================================ */

#include "ZzzMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <cerrno>
#include <ctime>
#include <fcntl.h>
#include <elf.h>
#include <link.h>
#include <mutex>
#include <setjmp.h>
#include <signal.h>
#include <sys/mman.h>
#include <unistd.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"
#include "Unity/Il2CppApi.h"

using Il2Cpp::GetApi;

#define TPL_LOG "[jql]"
#define TPL_TARGET_LIB sec_lib_il2cpp()
#define TPL_LIB_NAME "libil2cpp.so"

// ============================================================================
//  ① 站点常量（文件偏移空间，禁改——换包必须重 dump 并回填序言校验）
// ============================================================================
enum : uintptr_t {
    kFileOffInvEntry    = 0x1151d138,
    kFileOffDmgEntry    = 0xc0269f0,
    kFileOffMoveWrite   = 0xfb2700c,   // str s8,[x0,#0xf8]
    kFileOffMoveResume  = 0xfb27018,   // 原函数收尾（ldp x20,x19,[sp,#0x30]）
    kFileOffFreezeEntry = 0x19ce2164,
    kFileOffActorRoot   = 0x1d749418,  // libil2cpp 尾部 BSS 全局（玩家 actorId 链根）
};

// ⚠️ RVA（池鸳/loader 口径）＝ 模块首段映射起点起的偏移；本 so 各段 p_vaddr-p_offset 不同
//    （代码段 0x4000 / 数据段 0x8000 / BSS 段 0xC000）⇒ BSS 全局【不能】套代码段公式。
//    2026-10-06 R3 实证：actor 链根正确 VA = 首段起点 + 0x1d749418；套 phdr 文件偏移公式得 +0xC000 → NULL。
enum : uintptr_t {
    kRvaActorRoot       = 0x1d749418,  // 玩家 actorId 链根（RVA 空间）
    kRvaUltGate         = 0x11541510,  // 池鸳「大招」补丁点：cset w0,ls → mov w0,#1
};
static const uint32_t kUltGateOrig  = 0x1a9f87e0u;   // cset  w0, ls（原字节）
static const uint32_t kUltGatePatch = 0x52800020u;   // mov   w0, #1（池鸳写法，强制返回 true）

// 序言校验（磁盘文件实测原始字节，u32 小端）
static const uint32_t kPrologueInv[4]    = {0xa9be57feu, 0xa9014ff4u, 0xd0060388u, 0x2a0203f5u};
static const uint32_t kPrologueDmg[4]    = {0xfc1d0fe8u, 0xa90157feu, 0xa9024ff4u, 0xf008aac8u};
static const uint32_t kPrologueMove[3]   = {0xbd00f808u, 0xbd010008u, 0x39041008u};
static const uint32_t kPrologueFreeze[4] = {0xa9be57feu, 0xa9014ff4u, 0xd001c288u, 0xaa0103f4u};

// R5b（2026-10-06）：移速类「把速度重置为常量 1.0f」的两处站点（方法3 / 方法4 内 3 条写指令）。
//   反汇编实证（file 0xfb270e4 / 0xfb2720c 两个函数体）：
//     0x...16c  strb wzr,[x0,#0x104]  (0x3904101f)
//     0x...170  str  w8,[x0,#0xf8]   (0xb900f808)   w8 = 0x3f800000 = 1.0f
//     0x...174  str  w8,[x0,#0x100]  (0xb9001008)
//   x0 = 按 [x19+0x60] 查表得到的「速度组件」；x19 = 实体（函数入口 mov x19,x0）。
//   与 apply 方法（file 0xfb26f14，已打桩的 0xfb2700c）同属一个速度比率子系统：
//   apply 写 ratio/flag=1，本两处 reset 写 ratio=1.0f/flag=0。
static const uintptr_t kFileOffMoveResetA     = 0xfb2716c;
static const uintptr_t kFileOffMoveResetAEnd  = 0xfb27178;
static const uintptr_t kFileOffMoveResetB     = 0xfb27294;
static const uintptr_t kFileOffMoveResetBEnd  = 0xfb272a0;
static const uint32_t  kPrologueMoveReset[3]  = {0x3904101fu, 0xb900f808u, 0xb9001008u};

// ============================================================================
//  ② 开关 / 数值（铁律：滑块 1 = 关闭，默认必须 1）
// ============================================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_reenter{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_skill{false};        // 无限技能 = 无限大招（池鸳 kRvaUltGate 补丁）
static std::atomic<bool> g_freeze{false};
static std::atomic<int>  g_moveMul{1};          // 1..ZZZ_MOVE_MUL_MAX
static std::atomic<int>  g_dmgMul{1};           // 1..ZZZ_DMG_MUL_MAX
static std::atomic<bool> g_ultApplied{false};   // 大招补丁是否已落下（回读看护）
static std::atomic<unsigned> g_moveHitsLast{0}; // 移速 handler 命中计数快照（每 5s 对比）
static std::atomic<unsigned> g_moveResetALast{0};   // R5b：resetA 命中快照
static std::atomic<unsigned> g_moveResetBLast{0};   // R5b：resetB 命中快照
static std::atomic<int>  g_moveIdleTicks{0};    // 开了倍率但长期零命中 → 打一次告警

static std::atomic<bool> g_invHooked{false};
static std::atomic<bool> g_dmgHooked{false};
static std::atomic<bool> g_movePatched{false};
static std::atomic<bool> g_moveResetAPatched{false};   // R5b：reset 方法3 内 12B 桩
static std::atomic<bool> g_moveResetBPatched{false};   // R5b：reset 方法4 内 12B 桩
static std::atomic<bool> g_freezeHooked{false};

// ---- 移动速度原地桩 handler 用的 C 链接全局（asm 直接 adrp/add 引用，名字必须稳定） ----
extern "C" {
volatile unsigned char g_zzzMoveOn     = 0;      // 0 = 只跑原生 3 条指令
volatile float         g_zzzMoveMul    = 1.0f;   // 乘区（1.0 = 不放大）
volatile void*         g_zzzMoveResume = nullptr; // 原函数收尾 VA
volatile unsigned int  g_zzzMoveHits   = 0;      // handler 进入次数（诊断：站点是否真跑）
volatile unsigned int  g_zzzMoveOnHits = 0;      // 其中真正乘过倍率的次数
// R5b：两个 reset 站点各自的回收地址 / 命中数 / 学到的实体 id（全为诊断 + 回收用）
volatile void*         g_zzzMoveResetAResume = nullptr;
volatile void*         g_zzzMoveResetBResume = nullptr;
volatile unsigned int  g_zzzMoveResetAHits   = 0;
volatile unsigned int  g_zzzMoveResetBHits   = 0;
volatile unsigned int  g_zzzMoveSiteIdA      = 0;
volatile unsigned int  g_zzzMoveSiteIdB      = 0;
}

// 12B 原地桩跳进来的 handler：
//   入参沿用站点处寄存器现场：x0 = 目标组件，x8 = 1（strb 用），s8 = 待写入的比率
//   只在需要时对 s8 乘倍率，随后原样执行被替换掉的 3 条指令并跳回收尾。
extern "C" __attribute__((naked)) void Asm_ZzzMoveApply() {
    __asm__ volatile(
        "adrp x9, g_zzzMoveOn\n"
        "add  x9, x9, :lo12:g_zzzMoveOn\n"
        "ldrb w10, [x9]\n"
        "cbz  w10, 1f\n"
        "adrp x9, g_zzzMoveMul\n"
        "add  x9, x9, :lo12:g_zzzMoveMul\n"
        "ldr  s0, [x9]\n"
        "fmul s8, s8, s0\n"
        "adrp x9, g_zzzMoveOnHits\n"
        "add  x9, x9, :lo12:g_zzzMoveOnHits\n"
        "ldr  w10, [x9]\n"
        "add  w10, w10, #1\n"
        "str  w10, [x9]\n"
        "1:\n"
        "adrp x9, g_zzzMoveHits\n"
        "add  x9, x9, :lo12:g_zzzMoveHits\n"
        "ldr  w10, [x9]\n"
        "add  w10, w10, #1\n"
        "str  w10, [x9]\n"
        "str  s8, [x0, #0xf8]\n"
        "str  s8, [x0, #0x100]\n"
        "strb w8, [x0, #0x104]\n"
        "adrp x16, g_zzzMoveResume\n"
        "add  x16, x16, :lo12:g_zzzMoveResume\n"
        "ldr  x16, [x16]\n"
        "br   x16\n");
}

// R5b：两处「把速度重置为 1.0f」站点各自的 handler（方法3 / 方法4 内的 3 条写指令）。
//   开倍率：ratio=g_zzzMoveMul 写 [+0xf8]/[+0x100]，[+0x104]=1（与 apply 同语义＝覆盖生效）
//   关倍率：原样执行被替换的 3 条指令（ratio=1.0f / flag=0）
//   两路都记录 [x19+0x60]（实体 id）与命中计数，供诊断与按实体收窄
//   ⚠️ 只用 x8/x9/x10/x16/s0（caller-saved）；v8~v15 是被调用者保存，碰了会污染上层返回值
extern "C" __attribute__((naked)) void Asm_ZzzMoveResetA() {
    __asm__ volatile(
        "adrp x9, g_zzzMoveOn\n"
        "add  x9, x9, :lo12:g_zzzMoveOn\n"
        "ldrb w10, [x9]\n"
        "cbz  w10, 1f\n"
        "adrp x9, g_zzzMoveMul\n"
        "add  x9, x9, :lo12:g_zzzMoveMul\n"
        "ldr  s0, [x9]\n"
        "str  s0, [x0, #0xf8]\n"
        "str  s0, [x0, #0x100]\n"
        "mov  w10, #1\n"
        "strb w10, [x0, #0x104]\n"
        "adrp x9, g_zzzMoveResetAHits\n"
        "add  x9, x9, :lo12:g_zzzMoveResetAHits\n"
        "ldr  w10, [x9]\n"
        "add  w10, w10, #1\n"
        "str  w10, [x9]\n"
        "b    2f\n"
        "1:\n"
        "strb wzr, [x0, #0x104]\n"
        "mov  w8, #0x3f800000\n"
        "str  w8, [x0, #0xf8]\n"
        "str  w8, [x0, #0x100]\n"
        "2:\n"
        "ldr  w10, [x19, #0x60]\n"
        "adrp x9, g_zzzMoveSiteIdA\n"
        "add  x9, x9, :lo12:g_zzzMoveSiteIdA\n"
        "str  w10, [x9]\n"
        "adrp x16, g_zzzMoveResetAResume\n"
        "add  x16, x16, :lo12:g_zzzMoveResetAResume\n"
        "ldr  x16, [x16]\n"
        "br   x16\n");
}

extern "C" __attribute__((naked)) void Asm_ZzzMoveResetB() {
    __asm__ volatile(
        "adrp x9, g_zzzMoveOn\n"
        "add  x9, x9, :lo12:g_zzzMoveOn\n"
        "ldrb w10, [x9]\n"
        "cbz  w10, 1f\n"
        "adrp x9, g_zzzMoveMul\n"
        "add  x9, x9, :lo12:g_zzzMoveMul\n"
        "ldr  s0, [x9]\n"
        "str  s0, [x0, #0xf8]\n"
        "str  s0, [x0, #0x100]\n"
        "mov  w10, #1\n"
        "strb w10, [x0, #0x104]\n"
        "adrp x9, g_zzzMoveResetBHits\n"
        "add  x9, x9, :lo12:g_zzzMoveResetBHits\n"
        "ldr  w10, [x9]\n"
        "add  w10, w10, #1\n"
        "str  w10, [x9]\n"
        "b    2f\n"
        "1:\n"
        "strb wzr, [x0, #0x104]\n"
        "mov  w8, #0x3f800000\n"
        "str  w8, [x0, #0xf8]\n"
        "str  w8, [x0, #0x100]\n"
        "2:\n"
        "ldr  w10, [x19, #0x60]\n"
        "adrp x9, g_zzzMoveSiteIdB\n"
        "add  x9, x9, :lo12:g_zzzMoveSiteIdB\n"
        "str  w10, [x9]\n"
        "adrp x16, g_zzzMoveResetBResume\n"
        "add  x16, x16, :lo12:g_zzzMoveResetBResume\n"
        "ldr  x16, [x16]\n"
        "br   x16\n");
}

// ============================================================================
//  ③ /proc/self/maps → 文件偏移换算 VA（分段版；MuMu 分块映射实测口径）
// ============================================================================
struct MapSeg {
    uintptr_t start = 0, end = 0, off = 0;
    char      perm[8] = {};
};

static int ReadLibSegs(const char* lib, MapSeg* out, int maxOut) {
    FILE* fp = std::fopen("/proc/self/maps", "r");
    if (!fp) return 0;
    char line[1024];
    int n = 0;
    while (std::fgets(line, sizeof(line), fp)) {
        if (!std::strstr(line, lib)) continue;
        uintptr_t s = 0, e = 0, o = 0;
        char perm[8] = {};
        if (std::sscanf(line, "%lx-%lx %7s %lx", &s, &e, &perm[0], &o) != 4) continue;
        if (n < maxOut) {
            out[n].start = s;
            out[n].end   = e;
            out[n].off   = o;
            std::snprintf(out[n].perm, sizeof(out[n].perm), "%s", perm);
            ++n;
        }
    }
    std::fclose(fp);
    // 按文件偏移升序（段数很少，插入排序足够）
    for (int i = 1; i < n; ++i) {
        MapSeg key = out[i];
        int j = i - 1;
        while (j >= 0 && out[j].off > key.off) { out[j + 1] = out[j]; --j; }
        out[j + 1] = key;
    }
    return n;
}

// 尾部匿名 BSS：MuMu 下 arm64 so 的 .bss 落在最后一段之后的匿名 rw-p 映射里
static uintptr_t FindTailAnonBss(const char* lib, uintptr_t tailEnd) {
    FILE* fp = std::fopen("/proc/self/maps", "r");
    if (!fp) return 0;
    char line[1024];
    bool sawLib = false;
    uintptr_t anon = 0;
    while (std::fgets(line, sizeof(line), fp)) {
        if (std::strstr(line, lib)) { sawLib = true; continue; }
        if (!sawLib) continue;
        uintptr_t s = 0, e = 0, o = 0;
        char perm[8] = {};
        if (std::sscanf(line, "%lx-%lx %7s %lx", &s, &e, &perm[0], &o) == 4) {
            if (s == tailEnd && std::strchr(perm, 'w') && !std::strstr(line, "/")) {
                anon = s;   // 紧跟其后、无路径、可写 ⇒ BSS 延伸
            }
        }
        break;              // 只看紧随 lib 最后一段之后的那一行
    }
    std::fclose(fp);
    return anon;
}

// 文件偏移 → VA（含 MuMu 分块 + 尾部 BSS）
static void* FileOffToVa(const char* lib, uintptr_t fileOff) {
    static MapSeg segs[32];
    int n = ReadLibSegs(lib, segs, 32);
    if (n <= 0) return nullptr;

    for (int i = 0; i < n; ++i) {
        uintptr_t span = segs[i].end - segs[i].start;
        if (fileOff >= segs[i].off && fileOff < segs[i].off + span) {
            return reinterpret_cast<void*>(segs[i].start + (fileOff - segs[i].off));
        }
    }
    // 超出文件段范围 ⇒ 试尾部匿名 BSS
    MapSeg& tail = segs[n - 1];
    uintptr_t tailEnd = tail.off + (tail.end - tail.start);
    if (fileOff >= tailEnd) {
        uintptr_t bss = FindTailAnonBss(lib, tail.end);
        if (bss) return reinterpret_cast<void*>(bss + (fileOff - tailEnd));
    }
    return nullptr;
}

// ============================================================================
//  ④ 致命信号兜底 + 槽位熔断（v3.1 内建设施，删任何一段都可能复现历史闪退）
// ============================================================================
static std::mutex g_installMutex;
static std::mutex g_guardMutex;

static thread_local sigjmp_buf            t_faultJmp;
static thread_local volatile sig_atomic_t t_inGuard    = 0;
static thread_local volatile sig_atomic_t t_faultDepth = 0;
static thread_local volatile int          t_faultSig   = 0;
static thread_local volatile void*        t_faultAddr  = nullptr;
static thread_local char                  t_altStack[65536];
static thread_local bool                  t_altStackSet = false;

static struct sigaction g_oldSegv;
static struct sigaction g_oldBus;
static struct sigaction g_oldIll;
static std::atomic<bool> g_guardArmed{false};
static bool g_lastFatal = false;   // 只在 g_installMutex 下读写

static void ZzzFaultHandler(int sig, siginfo_t* si, void* uc) {
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
    if (g_guardArmed.load()) return;
    std::lock_guard<std::mutex> lk(g_guardMutex);
    if (g_guardArmed.load()) return;

    struct sigaction sa;
    std::memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = ZzzFaultHandler;
    sa.sa_flags     = SA_SIGINFO | SA_ONSTACK | SA_NODEFER;
    sigemptyset(&sa.sa_mask);

    sigaction(SIGSEGV, &sa, &g_oldSegv);
    sigaction(SIGBUS,  &sa, &g_oldBus);
    sigaction(SIGILL,  &sa, &g_oldIll);

    g_guardArmed.store(true);
    LOGI(TPL_LOG " fault guard armed (SIGSEGV/SIGBUS/SIGILL)");
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
    LOGW(TPL_LOG " %s read SIG(%d) @%p — 跳过", tag, (int)t_faultSig, (void*)t_faultAddr);
    return false;
}

static bool ReadPtrGuarded(const char* tag, const void* p, uint64_t* out) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        *out = *reinterpret_cast<const volatile uint64_t*>(p);
        t_inGuard = 0;
        return true;
    }
    t_inGuard = 0;
    LOGW(TPL_LOG " %s read SIG(%d) @%p — 跳过", tag, (int)t_faultSig, (void*)t_faultAddr);
    return false;
}

static bool DobbyHookGuarded(const char* tag, void* target, void* hook, void** orig) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        int rc = DobbyHook(target, (dobby_dummy_func_t)hook, (dobby_dummy_func_t*)orig);
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

// 序言校验：读地址前 16B
static bool CheckPrologue(const char* tag, void* va, const uint32_t* want, int words) {
    for (int i = 0; i < words; ++i) {
        uint32_t got = 0;
        if (!ReadU32Guarded(tag, reinterpret_cast<const char*>(va) + i * 4, &got)) return false;
        if (got != want[i]) {
            LOGW(TPL_LOG " %s drift [+%d] got=0x%08X want=0x%08X — 拒绝挂钩", tag, i * 4, got, want[i]);
            return false;
        }
    }
    return true;
}

// 静默读（就绪探测/丢失看护用：读失败不刷日志）
static bool ReadU32Quiet(const void* p, uint32_t* out) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        *out = *reinterpret_cast<const volatile uint32_t*>(p);
        t_inGuard = 0;
        return true;
    }
    t_inGuard = 0;
    return false;
}

// 静默序言比对（无日志；就绪门/丢失看护用）
static bool PrologueMatches(void* va, const uint32_t* want, int words) {
    for (int i = 0; i < words; ++i) {
        uint32_t got = 0;
        if (!ReadU32Quiet(reinterpret_cast<const char*>(va) + i * 4, &got)) return false;
        if (got != want[i]) return false;
    }
    return true;
}

// ============================================================================
//  ⑤a 真实模块 VA 解析（2026-10-06，MuMu 实战零命中根因修正）
//   MuMu 启动早期 maps 有游戏校验/预读 .so 的【同名临时分块映射】（0x77xx 宿主堆区，
//   随后被回收/复用）——isLibraryLoaded 按名字命中是「假就绪」，按那些映射装钩
//   = 装上了永不命中（实测：0x771014230138 零命中；同一函数后来按稳定 maps 口径
//   0x40002f62a9f0 装钩，30ms 内即打印命中日志）。
//   真实模块以 loader 记录（dl_iterate_phdr）为准；VA = base + p_vaddr + (fileOff - p_offset)。
// ============================================================================
struct PhdrCtx {
    const char*       lib;
    uintptr_t         base[4];
    const Elf64_Phdr* phdr[4];
    int               phnum[4];
    int               n;
};

static int PhdrCallback(struct dl_phdr_info* info, size_t, void* data) {
    PhdrCtx* ctx = static_cast<PhdrCtx*>(data);
    const char* n = info->dlpi_name;
    if (!n || !ctx->lib || ctx->n >= 4) return 0;
    size_t ln = std::strlen(ctx->lib), l = std::strlen(n);
    if (l < ln || std::strcmp(n + (l - ln), ctx->lib) != 0) return 0;
    ctx->base[ctx->n]  = static_cast<uintptr_t>(info->dlpi_addr);
    ctx->phdr[ctx->n]  = info->dlpi_phdr;
    ctx->phnum[ctx->n] = info->dlpi_phnum;
    ctx->n++;
    return 0;   // 收全（同名假库用序言筛）
}

// pro=nullptr：返回首个候选；pro 给定：返回首个序言匹配的候选（全不匹配 = nullptr，fail-closed）
static void* PhdrVaForFileOff(const char* lib, uintptr_t fileOff,
                              const uint32_t* pro = nullptr, int words = 0) {
    PhdrCtx ctx = {};
    ctx.lib = lib;
    dl_iterate_phdr(PhdrCallback, &ctx);
    void* first = nullptr;
    for (int m = 0; m < ctx.n; ++m) {
        const Elf64_Phdr* ph = ctx.phdr[m];
        if (!ph) continue;
        for (int i = 0; i < ctx.phnum[m]; ++i) {
            const Elf64_Phdr& p = ph[i];
            if (p.p_type != PT_LOAD) continue;
            uintptr_t span = p.p_filesz > p.p_memsz ? p.p_filesz : p.p_memsz;   // BSS 计入 memsz
            if (fileOff < p.p_offset || fileOff >= p.p_offset + span) continue;
            void* va = reinterpret_cast<void*>(ctx.base[m] + p.p_vaddr + (fileOff - p.p_offset));
            if (!first) first = va;
            if (!pro) return va;
            if (PrologueMatches(va, pro, words)) return va;
        }
    }
    return pro ? nullptr : first;
}

// RVA 空间原点：libil2cpp 里【文件偏移 0】那个 PT_LOAD 的映射起点。
// 池鸳与我们的常量（0x11541510 / 0x1d749418 …）都是相对它的偏移 ⇒ VA = origin + RVA。
// 实证 2026-10-06 MuMu：origin=0x400023600000，origin+0x1d749418 链通（actorId=5）；
// 同一常量套 phdr 文件偏移公式（该段 p_vaddr-p_offset=0xC000）读到 NULL ⇒ 必须分开两条公式。
static uintptr_t PhdrRvaOrigin(const char* lib) {
    PhdrCtx ctx = {};
    ctx.lib = lib;
    dl_iterate_phdr(PhdrCallback, &ctx);
    uintptr_t best = 0;
    for (int m = 0; m < ctx.n; ++m) {
        const Elf64_Phdr* ph = ctx.phdr[m];
        if (!ph) continue;
        for (int i = 0; i < ctx.phnum[m]; ++i) {
            const Elf64_Phdr& p = ph[i];
            if (p.p_type != PT_LOAD || p.p_offset != 0) continue;
            uintptr_t va = ctx.base[m] + p.p_vaddr;
            if (!best || va < best) best = va;
        }
    }
    return best;
}

// ============================================================================
//  ⑤ 玩家 actorId（池鸳 libcymod 0x4ed30 同款链；读失败 = fail-open 调原函数）
// ============================================================================
static bool ReadPtrQuiet(const void* p, uint64_t* out) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        *out = *reinterpret_cast<const volatile uint64_t*>(p);
        t_inGuard = 0;
        return true;
    }
    t_inGuard = 0;
    return false;
}

// R5（2026-10-06）：进程内自读第二路径 —— /proc/self/mem。
//   R4 实测：同一地址 PC 侧 /proc/<pid>/mem 读得到（actorId=5/6），进程内 direct load 却 10/10 走不通
//   ⇒ 链读取改 direct → pread(/proc/self/mem) 双路径；下面的自检会把两条路径逐级结果都打出来。
//   R5b：一旦失败就永久关闭（MuMu 实测这条路被拒）——否则伤害热路径上每个事件都要白开 7 个 fd。
static std::atomic<bool> g_selfMemDead{false};

static bool ReadMemSelf(void* dst, const void* src, size_t len) {
    if (g_selfMemDead.load()) return false;
    int fd = open("/proc/self/mem", O_RDONLY | O_CLOEXEC);
    if (fd < 0) { g_selfMemDead.store(true); return false; }
    ssize_t got = pread(fd, dst, len, static_cast<off_t>(reinterpret_cast<uintptr_t>(src)));
    close(fd);
    if (got != static_cast<ssize_t>(len)) { g_selfMemDead.store(true); return false; }
    return true;
}

static bool ReadPtrRobust(const void* p, uint64_t* out) {
    if (ReadPtrQuiet(p, out)) return true;
    return ReadMemSelf(out, p, sizeof(*out));
}

static bool ReadU32Robust(const void* p, uint32_t* out) {
    if (ReadU32Quiet(p, out)) return true;
    return ReadMemSelf(out, p, sizeof(*out));
}
static const size_t kActorChain[7] = {0x08, 0x18, 0x10, 0x78, 0x00, 0xb8, 0x00};

// ============================================================================
//  R5b：玩家侧 id 集（多源学习 + 跨场景缓存）
//   R4 实测教训：actor 链只在【大世界】成立，战斗里（01:57–01:59 连续 10 次重选）全候选失败
//   ⇒ me=0 ⇒ 无敌/伤害倍率同时哑。R5b 修法：
//     ① 链成功就把 id 记进集合，【不随场景清空】——战斗里链失效也能用缓存判归属；
//     ② 事件自带的所有权字段（+0x1C 攻击方 / +0x3C 受击方）继续按集合判定（R2/R4 两局实测：
//        玩家攻击事件 atk 所有权=5、怪物受害方所有权=0 ⇒ 0 是“无玩家归属”的 AI 侧）；
//     ③ 集合为空时用「本局连续出现最多的攻击方所有权值」兑底（≥3 次才认），并落日志。
// ============================================================================
static std::mutex   g_meMutex;
static uint32_t     g_meIds[4] = {0, 0, 0, 0};
static int          g_meIdN    = 0;
static std::atomic<unsigned> g_ownVoteVal{0};   // +0x1C 最近连续同值
static std::atomic<unsigned> g_ownVoteN{0};     // 该值连续出现次数

static void LearnMeId(uint32_t id) {
    if (!id) return;
    std::lock_guard<std::mutex> lk(g_meMutex);
    for (int i = 0; i < g_meIdN; ++i) if (g_meIds[i] == id) return;
    if (g_meIdN >= 4) {
        static std::atomic<int> n{0};
        if (n.fetch_add(1) < 3) LOGW(TPL_LOG " 玩家 id 集已满（%u 忽略；现有 %u/%u/%u/%u）",
                                     (unsigned)id, (unsigned)g_meIds[0], (unsigned)g_meIds[1],
                                     (unsigned)g_meIds[2], (unsigned)g_meIds[3]);
        return;
    }
    g_meIds[g_meIdN++] = id;
    LOGI(TPL_LOG " 玩家 id 集 +%u（现 %d 个：%u/%u/%u/%u）", (unsigned)id, g_meIdN,
         (unsigned)g_meIds[0], (unsigned)g_meIds[1], (unsigned)g_meIds[2], (unsigned)g_meIds[3]);
}

static bool IsMineId(uint32_t id) {
    if (!id) return false;
    std::lock_guard<std::mutex> lk(g_meMutex);
    for (int i = 0; i < g_meIdN; ++i) if (g_meIds[i] == id) return true;
    return false;
}

static int MeIdCount() {
    std::lock_guard<std::mutex> lk(g_meMutex);
    return g_meIdN;
}

static uint32_t FirstMeId() {
    std::lock_guard<std::mutex> lk(g_meMutex);
    return g_meIdN > 0 ? g_meIds[0] : 0u;
}

// +0x1C（攻击方所有权）：连续同值计票。链已经给出 id 的场合只作参考。
static void ObserveAttackerOwner(uint32_t own) {
    if (!own) return;
    if (g_ownVoteVal.load() == own) { g_ownVoteN.fetch_add(1); return; }
    g_ownVoteVal.store(own);
    g_ownVoteN.store(1);
}

// 某个所有权字段是不是「玩家侧」：① 命中 id 集；② 集合为空且它是本局连续×3 以上的投价值。
static bool IsMineOwner(uint32_t own, const char** why) {
    if (!own) return false;
    if (IsMineId(own)) { if (why) *why = "set"; return true; }
    if (MeIdCount() == 0 && g_ownVoteVal.load() == own && g_ownVoteN.load() >= 3) {
        if (why) *why = "vote"; return true;
    }
    return false;
}

// 链本体（池鸳 libcymod 0x4ed30 反汇编同款）；R5：走 Robust 双路径读，读不到不算成功
static bool WalkActorChain(void* root, uint32_t* outId) {
    uint64_t p = 0;
    if (!ReadPtrRobust(root, &p) || !p) return false;
    for (size_t i = 0; i < sizeof(kActorChain) / sizeof(kActorChain[0]); ++i) {
        if (!ReadPtrRobust(reinterpret_cast<const char*>(p) + kActorChain[i], &p) || !p) return false;
    }
    uint32_t id = 0;
    if (!ReadU32Robust(reinterpret_cast<const char*>(p) + 0x38, &id)) return false;
    if (!id) return false;
    *outId = id;
    return true;
}

// R5 自检：逐级把 direct / pread 两条路径的结果落日志（verbose=true 时）。
//  目的：把 R4 那句笼统的「链根全候选失败」变成可追溯到具体级别的证据。
//  R5b：加 verbose 开关 —— 30s 周期自检走简明档（只 direct、不打逐级日志），
//  结果翻转时再补一份逐级详情，避免刷屏。
static bool ChainSelfTest(bool verbose, uint32_t* outId) {
    uintptr_t org = PhdrRvaOrigin(TPL_LIB_NAME);
    void* root = org ? reinterpret_cast<void*>(org + kRvaActorRoot) : nullptr;
    if (verbose) LOGI(TPL_LOG " 链自检 org=0x%lX root=%p", (unsigned long)org, root);
    if (!root) return false;
    const int paths = verbose ? 2 : 1;   // selfmem 兜底已实证被拒（/proc/self/mem EPERM），只在详查时再看
    for (int path = 0; path < paths; ++path) {
        const char* pt = (path == 0) ? "direct" : "selfmem";
        uint64_t cur = 0;
        bool ok = (path == 0) ? ReadPtrQuiet(root, &cur) : ReadMemSelf(&cur, root, 8);
        if (verbose) LOGI(TPL_LOG "  链[%s] root=%p ok=%d val=0x%llX", pt, root, (int)ok, (unsigned long long)cur);
        if (!ok || !cur) continue;
        bool good = true;
        for (int i = 0; i < 7; ++i) {
            const void* a = reinterpret_cast<const char*>(cur) + kActorChain[i];
            uint64_t nxt = 0;
            bool ok2 = (path == 0) ? ReadPtrQuiet(a, &nxt) : ReadMemSelf(&nxt, a, 8);
            if (verbose) LOGI(TPL_LOG "  链[%s] +0x%02X @%p ok=%d val=0x%llX", pt,
                                   (unsigned)kActorChain[i], a, (int)ok2, (unsigned long long)nxt);
            if (!ok2 || !nxt) { good = false; break; }
            cur = nxt;
        }
        if (!good) continue;
        const void* ia = reinterpret_cast<const char*>(cur) + 0x38;
        uint32_t id = 0;
        bool ok3 = (path == 0) ? ReadU32Quiet(ia, &id) : ReadMemSelf(&id, ia, 4);
        if (verbose) LOGI(TPL_LOG "  链[%s] id@%p ok=%d id=%u", pt, ia, (int)ok3, (unsigned)id);
        if (ok3 && id) { if (outId) *outId = id; LearnMeId(id); return true; }
    }
    return false;
}

static bool ChainSelfTestVerbose(uint32_t* outId) { return ChainSelfTest(true, outId); }

// 候选交叉校验（能用就用，不能用就换）：① RVA 口径（实证正确）② phdr 文件偏移口径（旧实现）
// ③ maps 分段口径。逐个走链，取首个能得到非 0 actorId 的根地址并落盘日志；全失败则返回首个候选。
static void* ResolveActorRoot() {
    void* cand[3] = {nullptr, nullptr, nullptr};
    int n = 0;
    uintptr_t org = PhdrRvaOrigin(TPL_LIB_NAME);
    if (org) cand[n++] = reinterpret_cast<void*>(org + kRvaActorRoot);
    auto push = [&](void* v) {
        if (!v) return;
        for (int i = 0; i < n; ++i) if (cand[i] == v) return;
        if (n < 3) cand[n++] = v;
    };
    push(PhdrVaForFileOff(TPL_LIB_NAME, kFileOffActorRoot));   // BSS 段：memsz 口径
    push(FileOffToVa(TPL_LIB_NAME, kFileOffActorRoot));
    for (int i = 0; i < n; ++i) {
        uint32_t id = 0;
        if (WalkActorChain(cand[i], &id)) {
            LOGI(TPL_LOG " actor 链根选定 cand#%d @%p (RVA 0x%lX, 首个非0 actorId=%u)",
                 i, cand[i], (unsigned long)kRvaActorRoot, (unsigned)id);
            return cand[i];
        }
    }
    LOGW(TPL_LOG " actor 链根全候选失败（cand#0=%p）— 改用 id 集/所有权字段（R5b）",
         n > 0 ? cand[0] : nullptr);
    return n > 0 ? cand[0] : nullptr;
}

static bool ResolvePlayerActorId(uint32_t* outId) {
    static void* root = nullptr;
    static std::atomic<int> fails{0};
    if (!root) root = ResolveActorRoot();
    if (!root) return false;
    if (WalkActorChain(root, outId)) { fails.store(0); LearnMeId(*outId); return true; }
    // 场景切换后链根可能重生：连续失败达阈值就重选候选（不频繁重算）
    if (fails.fetch_add(1) >= 64) { fails.store(0); root = nullptr; }
    return false;
}

// ============================================================================
//  ⑥ hook 本体
// ============================================================================
typedef void (*FnInv_t)(void* self, void* ev, size_t a2, void* a3, void* mth);
typedef void (*FnDmg_t)(void* self, float mult, void* mth);
typedef void (*FnFreeze_t)(void* self, void* a1, void* mth);

static FnInv_t    g_origInv    = nullptr;
static FnDmg_t    g_origDmg    = nullptr;
static FnFreeze_t g_origFreeze = nullptr;

// 诊断（有界，仅前若干次调用）：dump 伤害事件头部，用于定位「伤害数值」字段。
// 找到字段后由 damage 倍率实现接管，本段删除。
static std::atomic<int> g_evtCalls{0};
static std::atomic<int> g_evtDumps{0};

static float U32AsF(uint32_t u) { float f = 0.0f; std::memcpy(&f, &u, sizeof(f)); return f; }

// 2026-10-06 口径修正：只在功能开启后采样；窗口 0x100（事件头 0x40 + 首条伤害记录 0x40）；
// 前 6 个伤害事件全量 dump，之后只打一行摘要 —— 不再让 R1/R2 那种“开局就被采空”重演。
// 2026-10-06 R5b：诊断口径再收。每事件一行紧凑摘要（前 48 个），带全部候选字段；
// 前 2 个事件另加全量 dump（保留记录布局证据）。字段语义（R2+R4 两局实测）：
//   +0x18 攻击方 actor / +0x1C 攻击方所有权（玩家攻击时为玩家 id，怪物为 0）
//   +0x38 受击方 actor / +0x3C 受击方所有权 / +0x16(u16) 事件类型 0x85 = 伤害
static void DiagDumpEvent(void* ev, uint16_t type, uint32_t victim, uint32_t atk,
                          uint32_t atkOwn, uint32_t vicOwn, uint32_t me, const char* meSrc) {
    int n = g_evtCalls.fetch_add(1);
    if (n < 48) {
        uint32_t c10 = 0, c14 = 0;
        ReadU32Quiet(reinterpret_cast<const char*>(ev) + 0x10, &c10);
        ReadU32Quiet(reinterpret_cast<const char*>(ev) + 0x14, &c14);
        LOGI(TPL_LOG " EVT#%d type=0x%X atk=%u own=%u vic=%u vow=%u c10=0x%08X c14=0x%08X me=%u(%s) set=%d vote=%u/%u",
             n, (unsigned)type, (unsigned)atk, (unsigned)atkOwn, (unsigned)victim,
             (unsigned)vicOwn, (unsigned)c10, (unsigned)c14, (unsigned)me, meSrc ? meSrc : "-",
             MeIdCount(), (unsigned)g_ownVoteVal.load(), (unsigned)g_ownVoteN.load());
    }
    if (n >= 2 || g_evtDumps.fetch_add(1) >= 2) return;
    for (int base = 0; base < 0x100; base += 16) {
        uint32_t v[4] = {0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu, 0xFFFFFFFFu};
        for (int i = 0; i < 4; ++i) {
            ReadU32Quiet(reinterpret_cast<const char*>(ev) + base + i * 4, &v[i]);
        }
        LOGI(TPL_LOG " EVT#%d +0x%02X %08X %08X %08X %08X | f=%.4g %.4g %.4g %.4g",
             n, base, v[0], v[1], v[2], v[3],
             U32AsF(v[0]), U32AsF(v[1]), U32AsF(v[2]), U32AsF(v[3]));
    }
}

// 伤害记录（R3 实测布局）：事件缓冲每 0x40 一条，数值在记录 +0x18；首条 = ev+0x40。
// 记录形如 {config ptr, 0, obj ptr, value(u32), obj ptr, 0, id(0x104), 0}（实测 508 / 516 / 1512）。
static void ScaleDamageRecord(void* ev, int mul) {
    char* rec = reinterpret_cast<char*>(ev) + 0x40;
    uint32_t v = 0;
    if (!ReadU32Quiet(rec + 0x18, &v) || v == 0 || v > 100000000u) return;
    uint64_t scaled = static_cast<uint64_t>(v) * static_cast<uint64_t>(mul);
    uint32_t nv = scaled > 0x7FFFFFFFu ? 0x7FFFFFFFu : static_cast<uint32_t>(scaled);
    if (nv == v) return;
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        *reinterpret_cast<volatile uint32_t*>(rec + 0x18) = nv;
        t_inGuard = 0;
    } else {
        t_inGuard = 0;
        return;
    }
    static std::atomic<int> n{0};
    if (n.fetch_add(1) < 12)
        LOGI(TPL_LOG " 伤害倍率: 记录 %u x%d -> %u（atk=me, victim!=me）", v, mul, nv);
}

// 事件字段（R2+R4 两局实测）：+0x14 高 16 位 = 事件类型；+0x16==0x85 即伤害事件；
//   +0x18 攻击方 actorId / +0x1C 攻击方所有权（玩家攻击时=玩家 id，怪物=0）
//   +0x38 受击方 actorId / +0x3C 受击方所有权（怪物=0）
// ① 无敌：受击方是玩家侧 ⇒ 不调原函数（= 池鸳 0x4e2c8 的过滤，但归属判定扩成多源，见下）
// ② 伤害倍率：攻击方是玩家侧且受击方不是 ⇒ 放大该次伤害记录
static void Hook_Inv(void* self, void* ev, size_t a2, void* a3, void* mth) {
    if (!ev) {
        if (g_origInv) g_origInv(self, ev, a2, a3, mth);
        return;
    }

    uint32_t wt = 0, victim = 0, atk = 0, atkOwn = 0, vicOwn = 0;
    bool okT = ReadU32Quiet(reinterpret_cast<const char*>(ev) + 0x14, &wt);
    uint16_t type = okT ? static_cast<uint16_t>(wt >> 16) : 0xFFFFu;
    bool okK = ReadU32Quiet(reinterpret_cast<const char*>(ev) + 0x18, &atk);
    bool okO = ReadU32Quiet(reinterpret_cast<const char*>(ev) + 0x1C, &atkOwn);
    bool okV = ReadU32Quiet(reinterpret_cast<const char*>(ev) + 0x38, &victim);
    bool okW = ReadU32Quiet(reinterpret_cast<const char*>(ev) + 0x3C, &vicOwn);

    const bool invOn  = g_inv.load();
    const int  mul    = g_dmgMul.load();

    // +0x1C（攻击方所有权）连续同值计票：id 集为空的兜底（R2/R4 实测玩家攻击时为 5）
    if (type == 0x85 && okO) ObserveAttackerOwner(atkOwn);

    uint32_t me = 0;
    const char* meSrc = "none";
    bool haveMe = false;
    if (invOn || mul > 1) {   // 只在功能开启时走链（省热路径）
        if (ResolvePlayerActorId(&me)) { haveMe = true; meSrc = "chain"; }
        else { me = FirstMeId(); if (me) { haveMe = true; meSrc = "cache"; } }
    }

    if ((invOn || mul > 1) && type == 0x85) {   // 诊断：功能开启后才采样
        DiagDumpEvent(ev, type, okV ? victim : 0u, okK ? atk : 0u,
                      okO ? atkOwn : 0u, okW ? vicOwn : 0u, me, meSrc);
    }

    // 归属判定（R5b 多源）：① actor id 命中 id 集；② 所有权字段命中（+0x3C 受击方 / +0x1C 攻击方）
    if (type == 0x85 && !g_reenter.load()) {
        const char* whyV = nullptr;
        const bool vicMine = (okV && IsMineId(victim)) || (okW && IsMineOwner(vicOwn, &whyV));
        const char* whyA = nullptr;
        const bool atkMine = (okK && IsMineId(atk)) || (okO && IsMineOwner(atkOwn, &whyA));

        // ① 无敌：受击方是玩家侧 ⇒ 拦截，不调原函数
        if (invOn && vicMine) {
            static std::atomic<int> n{0};
            if (n.fetch_add(1) < 12) {
                LOGI(TPL_LOG " 无敌拦截: type=0x%X 受击=%u vow=%u(why=%s) atk=%u own=%u(why=%s) me=%u(%s)",
                     (unsigned)type, (unsigned)victim, (unsigned)vicOwn, whyV ? whyV : "-",
                     (unsigned)atk, (unsigned)atkOwn, whyA ? whyA : "-", (unsigned)me, meSrc);
            }
            return;
        }

        // ② 伤害倍率：攻击方是玩家侧 且 受击方不是 ⇒ 放大该次伤害记录
        if (mul > 1 && atkMine && !vicMine) {
            ScaleDamageRecord(ev, mul);
        }
    }

    if (g_origInv) g_origInv(self, ev, a2, a3, mth);
}

// ② 伤害倍率：入口 s0 即倍率入参，乘滑块后透传（x1 原样转发，语义不变）
// ②b 该站点 R2/R3 实测是每帧 Update(dt)（入参 0.030~0.061 = 帧间隔），乘 dt 只会搞乱时序。
// 本轮改回纯透传；真正伤害倍率改走事件记录（Hook_Inv ②）。此桩保留作为回遇/上下文。
static void Hook_Dmg(void* self, float mult, void* mth) {
    if (g_dmgMul.load() > 1) {
        static std::atomic<int> n{0};
        if (n.fetch_add(1) < 3) LOGI(TPL_LOG " [诊断] Dmg 站 dt=%.3f（本桩只透传）", mult);
    }
    if (g_origDmg) g_origDmg(self, mult, mth);
}

// 定怪：BehaviorManager 是全局单例（调用者自全局取 x0），Tick(self, entity, ...) 的 entity 才是「谁的行为」。
// v1（2026-10-06）：entity 类名可读且像玩家 ⇒ 不冻；其余跳过原函数（怪物行为停摆）。
// entity 类名前 12 次进日志，用于按实测类名收窄过滤。
static const char* SafeClassName(const char* tag, void* obj) {
    if (!obj) return nullptr;
    if (!GetApi().ready && !GetApi().Load(TPL_TARGET_LIB)) return nullptr;
    void* k = GetApi().object_class(obj);
    if (!k) return nullptr;
    const char* n = GetApi().class_name(k);
    if (!n || !*n) return nullptr;
    static std::atomic<int> nLog{0};
    if (nLog.fetch_add(1) < 12) LOGI(TPL_LOG " %s 候选类名: %s", tag, n);
    return n;
}

static void Hook_Freeze(void* self, void* a1, void* mth) {
    if (g_freeze.load()) {
        const char* n = SafeClassName("定怪 entity", a1);
        bool isPlayer = n && (std::strstr(n, "Avatar") || std::strstr(n, "Player") ||
                              std::strstr(n, "Hero")   || std::strstr(n, "Agent"));
        if (!isPlayer) {
            static std::atomic<int> n2{0};
            if (n2.fetch_add(1) < 10) LOGI(TPL_LOG " 定怪拦截 BehaviorManager.Tick");
            return;   // 跳过更新 ⇒ 该实体行为不再推进
        }
    }
    if (g_origFreeze) g_origFreeze(self, a1, mth);
}

// ============================================================================
//  ⑦ 安装器（幂等；lib ready 即全装，开关由 handler 内门控 — MuMu 转译缓存见下文 SyncHooks）
// ============================================================================
static bool InstallEntryHook(const char* tag, uintptr_t fileOff, const uint32_t* pro, int words,
                             void* hookFn, void** origOut, std::atomic<bool>* doneFlag) {
    if (doneFlag->load()) return true;
    // ① loader 真实模块（序言交叉校验；假模块/未装好 = nullptr 走②）
    void* va = PhdrVaForFileOff(TPL_LIB_NAME, fileOff, pro, words);
    const char* src = "phdr";
    // ② 回退 maps 分段口径（真机/稳定期与 phdr 同值；libReady 已要求真模块门）
    if (!va) {
        va = FileOffToVa(TPL_LIB_NAME, fileOff);
        src = "maps";
    }
    if (!va) {
        LOGW(TPL_LOG " %s: file 0x%lX 未解析到 VA（phdr 无条目且 maps 无覆盖）", tag, (unsigned long)fileOff);
        return false;
    }
    if (!CheckPrologue(tag, va, pro, words)) return false;
    if (!DobbyHookGuarded(tag, va, hookFn, origOut)) return false;
    doneFlag->store(true);
    LOGI(TPL_LOG " %s hooked @va %p (file 0x%lX src=%s)", tag, va, (unsigned long)fileOff, src);
    return true;
}

// 移动速度：12B 原地桩（adrp/add/br → 近距跳板 → 自有 asm handler），handler 内乘倍率后跳回收尾
//
// 2026-10-06 L2 实测：MuMu/Houdini 下本 so 与 libil2cpp 常相距 >4GB（delta ≈ -0x371A…），
//   12B 桩里的 adrp 够不到 handler ⇒ 先在 target ±4GB 内分配一页「近距跳板」：
//   跳板用 movz/movk 组装 64 位 handler 地址再 br；目标处只 adrp/add/br → 跳板。
static uintptr_t AllocNearIsland(void* target, size_t* outSize) {
    const long ps = sysconf(_SC_PAGESIZE);
    const size_t sz = static_cast<size_t>(ps > 0 ? ps : 4096);
    *outSize = sz;
    const uintptr_t base = reinterpret_cast<uintptr_t>(target) & ~static_cast<uintptr_t>(sz - 1);
    const int64_t kLimit = static_cast<int64_t>(0x7FF00000);   // < ±4GB，留余量
    for (int i = 0; i < 32; ++i) {
        uintptr_t hint = 0;
        if (i > 0) {
            uintptr_t step = static_cast<uintptr_t>(0x4000000ULL) * static_cast<uintptr_t>((i + 1) / 2);
            hint = (i & 1) ? ((base > step) ? (base - step) : 0) : (base + step);
        }
        // MuMu 下 execmem 可用（Houdini 自身就有大块 rwxp）；直接 RWX 避免 mprotect 被 SELinux 拦
        void* p = mmap(reinterpret_cast<void*>(hint), sz, PROT_READ | PROT_WRITE | PROT_EXEC,
                       MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
        if (p == MAP_FAILED) {
            p = mmap(reinterpret_cast<void*>(hint), sz, PROT_READ | PROT_WRITE,
                     MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
        }
        if (p == MAP_FAILED) continue;
        int64_t d = static_cast<int64_t>(reinterpret_cast<uintptr_t>(p)) - static_cast<int64_t>(base);
        if (d > -kLimit && d < kLimit) return reinterpret_cast<uintptr_t>(p);
        munmap(p, sz);
    }
    return 0;
}

//   R5b：抽成通用安装器（apply 站点 + 两个 reset 站点，共 3 处同构 12B 桩）。
static bool InstallInlineStub(const char* tag, uintptr_t fileOff, uintptr_t resumeOff,
                              const uint32_t pro[3], void* handler, volatile void** resumeSlot,
                              std::atomic<bool>* doneFlag) {
    if (doneFlag->load()) return true;
    // target 与 resume 必须同一口径（混口径 ⇒ stub 跳回假副本必闪退）
    const char* src = "phdr";
    void* target = PhdrVaForFileOff(TPL_LIB_NAME, fileOff, pro, 3);
    void* resume = PhdrVaForFileOff(TPL_LIB_NAME, resumeOff);
    if (!target || !resume) {
        target = FileOffToVa(TPL_LIB_NAME, fileOff);
        resume = FileOffToVa(TPL_LIB_NAME, resumeOff);
        src = "maps";
    }
    if (!target || !resume) {
        LOGW(TPL_LOG " %s: 站点未映射（target=%p resume=%p）", tag, target, resume);
        return false;
    }
    if (!CheckPrologue(tag, target, pro, 3)) return false;

    *resumeSlot = resume;

    uintptr_t h = reinterpret_cast<uintptr_t>(handler);

    size_t islandSize = 0;
    uintptr_t island = AllocNearIsland(target, &islandSize);
    if (!island) {
        LOGW(TPL_LOG " %s: 跳板页分配失败（target=%p handler=%p）", tag,
             target, reinterpret_cast<void*>(h));
        return false;
    }

    uint32_t* isl = reinterpret_cast<uint32_t*>(island);
    static const uint32_t kMovk[4] = {0xD2800000u, 0xF2A00000u, 0xF2C00000u, 0xF2E00000u};
    for (int i = 0; i < 4; ++i) {
        isl[i] = kMovk[i] | (static_cast<uint32_t>((h >> (16 * i)) & 0xFFFFu) << 5) | 16u;
    }
    isl[4] = 0xD61F0200u;   // br x16
    __builtin___clear_cache(reinterpret_cast<char*>(island), reinterpret_cast<char*>(island) + 20);
    mprotect(reinterpret_cast<void*>(island), islandSize, PROT_READ | PROT_EXEC);

    uintptr_t herePage = reinterpret_cast<uintptr_t>(target) & ~static_cast<uintptr_t>(0xFFF);
    int64_t pageDelta = static_cast<int64_t>(island & ~static_cast<uintptr_t>(0xFFF)) - static_cast<int64_t>(herePage);
    if (pageDelta > static_cast<int64_t>(0xFFFFF000) || pageDelta < static_cast<int64_t>(-0x100000000)) {
        LOGW(TPL_LOG " %s: 跳板超出 adrp 范围 (%lld)", tag, (long long)pageDelta);
        return false;
    }
    uint32_t immPages = static_cast<uint32_t>(pageDelta >> 12);
    uint32_t immlo = immPages & 0x3u;
    uint32_t immhi = (immPages >> 2) & 0x7FFFFu;
    uint32_t lo12  = static_cast<uint32_t>(island & 0xFFFu);

    uint32_t stub[3];
    stub[0] = 0x90000010u | (immlo << 29) | (immhi << 5);   // adrp x16, <island page>
    stub[1] = 0x91000210u | (lo12 << 10);                   // add  x16, x16, #lo12
    stub[2] = 0xD61F0200u;                                  // br   x16

    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        int rc = DobbyCodePatch(target, reinterpret_cast<uint8_t*>(stub), sizeof(stub));
        t_inGuard = 0;
        if (rc != 0) {
            LOGW(TPL_LOG " %s: DobbyCodePatch rc=%d", tag, rc);
            return false;
        }
    } else {
        t_inGuard = 0;
        g_lastFatal = true;
        LOGW(TPL_LOG " %s: patch SIG(%d) @%p — 熔断", tag, (int)t_faultSig, (void*)t_faultAddr);
        return false;
    }

    // 写回自检：确认 12B 桩真的落下去（防"改了但没写进去"的假成功）
    uint32_t rb = 0;
    for (int i = 0; i < 3; ++i) {
        if (!ReadU32Guarded(tag, reinterpret_cast<const char*>(target) + i * 4, &rb) ||
            rb != stub[i]) {
            LOGW(TPL_LOG " %s: 自检失败 [+%d] got=0x%08X want=0x%08X", tag, i * 4, rb, stub[i]);
            return false;
        }
    }
    doneFlag->store(true);
    LOGI(TPL_LOG " %s patched @va %p (file 0x%lX src=%s) -> island %p -> handler %p",
         tag, target, (unsigned long)fileOff, src, reinterpret_cast<void*>(island), (void*)h);
    return true;
}

static bool InstallMovePatch() {
    return InstallInlineStub("MoveSpeed", kFileOffMoveWrite, kFileOffMoveResume, kPrologueMove,
                             reinterpret_cast<void*>(&Asm_ZzzMoveApply), &g_zzzMoveResume,
                             &g_movePatched);
}

// R5b：两处 reset 站点。命中就说明该实体的「速度被重置」，我们改成写自己的倍率。
static bool InstallMoveResetA() {
    return InstallInlineStub("MoveResetA", kFileOffMoveResetA, kFileOffMoveResetAEnd,
                             kPrologueMoveReset, reinterpret_cast<void*>(&Asm_ZzzMoveResetA),
                             &g_zzzMoveResetAResume, &g_moveResetAPatched);
}

static bool InstallMoveResetB() {
    return InstallInlineStub("MoveResetB", kFileOffMoveResetB, kFileOffMoveResetBEnd,
                             kPrologueMoveReset, reinterpret_cast<void*>(&Asm_ZzzMoveResetB),
                             &g_zzzMoveResetBResume, &g_moveResetBPatched);
}

// ============================================================================
//  ⑦b 无限大招（借池鸳的「改哪一条指令」结论，实现走我们自己的字节补丁 + 自检 + 看护）
//    池鸳 Nap32Extra：把 RVA 0x11541510（原字节 0x1a9f87e0 = cset w0,ls）改成 0x52800020
//    = mov w0,#1，强制该“大招可用”判定恒真；其自检逻辑（libcymod 0x50520 附近）也按这两个
//    值区分“已补丁 / 未补丁” ⇒ 确认这就是它「大招」的唯一落点。
//    开关关掉时还原原字节；每 5s 回读，被游戏还原/重映射则自动重打。
// ============================================================================
static void* UltGateVa() {
    static void* cached = nullptr;
    if (cached) return cached;
    uintptr_t org = PhdrRvaOrigin(TPL_LIB_NAME);
    if (org) {
        void* cand = reinterpret_cast<void*>(org + kRvaUltGate);
        uint32_t cur = 0;
        if (ReadU32Quiet(cand, &cur) && (cur == kUltGateOrig || cur == kUltGatePatch)) {
            cached = cand;
            return cached;
        }
    }
    // 回退：文件偏移口径（代码段 p_vaddr-p_offset=0x4000 ⇒ fileOff = RVA - 0x4000）
    void* fb = PhdrVaForFileOff(TPL_LIB_NAME, kRvaUltGate - 0x4000, &kUltGateOrig, 1);
    if (fb) cached = fb;
    return cached;
}

static bool WriteUltGate(uint32_t expect, uint32_t want, const char* what) {
    void* va = UltGateVa();
    if (!va) {
        LOGW(TPL_LOG " 无限大招: 站点未解析（RVA 0x%lX）", (unsigned long)kRvaUltGate);
        return false;
    }
    uint32_t cur = 0;
    if (!ReadU32Quiet(va, &cur)) {
        LOGW(TPL_LOG " 无限大招: 站点不可读 @%p", va);
        return false;
    }
    if (cur == want) return true;
    if (cur != expect) {
        LOGW(TPL_LOG " 无限大招: 字节漂移 @%p got=0x%08X want=0x%08X — 拒绝写入",
             va, (unsigned)cur, (unsigned)expect);
        return false;
    }
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        uint32_t v = want;
        int rc = DobbyCodePatch(va, reinterpret_cast<uint8_t*>(&v), 4);
        t_inGuard = 0;
        if (rc != 0) {
            LOGW(TPL_LOG " 无限大招: DobbyCodePatch rc=%d", rc);
            return false;
        }
    } else {
        t_inGuard = 0;
        g_lastFatal = true;
        LOGW(TPL_LOG " 无限大招: patch SIG(%d) @%p — 熔断", (int)t_faultSig, (void*)t_faultAddr);
        return false;
    }
    uint32_t rb = 0;
    if (!ReadU32Quiet(va, &rb) || rb != want) {
        LOGW(TPL_LOG " 无限大招: 自检失败 got=0x%08X want=0x%08X", (unsigned)rb, (unsigned)want);
        return false;
    }
    LOGI(TPL_LOG " 无限大招: %s @%p (0x%08X)", what, va, (unsigned)want);
    return true;
}

static void SyncUltGate() {
    if (g_skill.load()) {
        if (g_ultApplied.load()) {
            void* va = UltGateVa();
            uint32_t cur = 0;
            if (va && ReadU32Quiet(va, &cur) && cur == kUltGatePatch) return;   // 还在，不动
            g_ultApplied.store(false);   // 被还原/外部修改 ⇒ 重打
        }
        if (WriteUltGate(kUltGateOrig, kUltGatePatch, "补丁已应用")) g_ultApplied.store(true);
    } else if (g_ultApplied.load()) {
        if (WriteUltGate(kUltGatePatch, kUltGateOrig, "已还原原字节")) g_ultApplied.store(false);
    }
}

// 2026-10-06（MuMu 实战零命中真因，修正前一版「转译缓存不回读」误判）：
//   同一函数在激战中晚装钩也能 30ms 内命中（Dmg@0x40002f62a9f0 十条日志）⇒ 不是缓存问题。
//   真因是【口径选错】：MuMu 早期 maps 有游戏校验/预读同名临时分块映射（0x77xx），
//   isLibraryLoaded 早命中 ⇒ 桩全落在随即被回收的假副本上 ⇒ 功能全哑。
//   现在：就绪门=phdr 真模块+序言交叉校验；VA=phdr PT_LOAD 换算（maps 回退）；
//   每 Tick 读回看护，原始序言重现即自动重挂。
static void SyncHooks() {
    if (!g_libReady.load()) return;

    std::lock_guard<std::mutex> lk(g_installMutex);
    g_lastFatal = false;

    if (!g_invHooked.load()) {
        InstallEntryHook("Inv", kFileOffInvEntry, kPrologueInv, 4,
                         (void*)Hook_Inv, (void**)&g_origInv, &g_invHooked);
    }
    if (!g_dmgHooked.load()) {
        InstallEntryHook("Dmg", kFileOffDmgEntry, kPrologueDmg, 4,
                         (void*)Hook_Dmg, (void**)&g_origDmg, &g_dmgHooked);
    }
    if (!g_freezeHooked.load()) {
        InstallEntryHook("Freeze", kFileOffFreezeEntry, kPrologueFreeze, 4,
                         (void*)Hook_Freeze, (void**)&g_origFreeze, &g_freezeHooked);
    }
    if (!g_movePatched.load())       InstallMovePatch();
    if (!g_moveResetAPatched.load()) InstallMoveResetA();   // R5b：reset 方法3（写 1.0f → 写倍率）
    if (!g_moveResetBPatched.load()) InstallMoveResetB();   // R5b：reset 方法4
    SyncUltGate();   // 无限大招：开关跟随；被还原则重打（内部自带回读）
}

// ============================================================================
//  ⑧ 对外接口（ModEntry.h 注册表契约，签名不可改）
// ============================================================================
// 真就绪判据（2026-10-06）：maps 名字命中 ≠ 真加载（早期有校验/预读同名临时映射）。
// 必须：① loader（dl_iterate_phdr）有 libil2cpp；② Dmg 站序言 RVA 交叉校验通过
// ⇒ 真实模块最终映射就位。任一不满足继续等（2s 一次，不写内存）。
static bool RealModuleReady() {
    return PhdrVaForFileOff(TPL_LIB_NAME, kFileOffDmgEntry, kPrologueDmg, 4) != nullptr;
}

bool Zzz_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;
    if (!RealModuleReady()) return false;

    InstallFaultGuard();
    g_libReady.store(true);
    LOGI(TPL_LOG " lib ready (real module via dl_iterate_phdr; PT_LOAD VA)");
    SyncHooks();
    ChainSelfTestVerbose(nullptr);   // R5：就绪即自检链读路径（标题界面即可判定，不需战斗）
    return true;
}

bool Zzz_IsReady() {
    return g_libReady.load();
}

void Zzz_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case ZZZ_FEAT_INV:
            g_inv.store(enabled);
            break;
        case ZZZ_FEAT_SKILL:
            g_skill.store(enabled);
            // 无限技能 = 无限大招：走池鸳同款站点（RVA 0x11541510）的字节补丁，见 SyncUltGate
            LOGI(TPL_LOG " 无限大招开关 = %s", enabled ? "on" : "off");
            break;
        case ZZZ_FEAT_FREEZE:
            g_freeze.store(enabled);
            break;
        default:
            break;
    }
    SyncHooks();
}

void Zzz_OnValueChanged(int featNum, int value) {
    if (featNum == ZZZ_FEAT_MOVE) {
        if (value < 1) value = 1;
        if (value > ZZZ_MOVE_MUL_MAX) value = ZZZ_MOVE_MUL_MAX;
        g_moveMul.store(value);
        g_zzzMoveOn  = (unsigned char)(value > 1 ? 1 : 0);
        g_zzzMoveMul = static_cast<float>(value);
        LOGI(TPL_LOG " 移动速度倍率 = %d（%s）", value, value > 1 ? "on" : "off");
    } else if (featNum == ZZZ_FEAT_DMG) {
        if (value < 1) value = 1;
        if (value > ZZZ_DMG_MUL_MAX) value = ZZZ_DMG_MUL_MAX;
        g_dmgMul.store(value);
        LOGI(TPL_LOG " 伤害倍率 = %d（%s）", value, value > 1 ? "on" : "off");
    }
    SyncHooks();
}

// 装钩丢失看护（每 Tick 5s 一次）：目标处原始序言重新出现 ⇒ 映射被替换/还原，
// 认丢失并清 doneFlag，交给 SyncHooks 重挂（覆盖 MuMu 迟到重映射场景）。
static void WatchHooks() {
    struct Watch { const char* tag; uintptr_t off; const uint32_t* pro; int words; std::atomic<bool>* done; };
    static Watch kWatch[] = {
        {"Inv",    kFileOffInvEntry,    kPrologueInv,    4, &g_invHooked},
        {"Dmg",    kFileOffDmgEntry,    kPrologueDmg,    4, &g_dmgHooked},
        {"Freeze", kFileOffFreezeEntry, kPrologueFreeze, 4, &g_freezeHooked},
    };
    for (Watch& w : kWatch) {
        if (!w.done->load()) continue;
        void* va = PhdrVaForFileOff(TPL_LIB_NAME, w.off, w.pro, w.words);
        if (va) {   // 序言匹配 ⇒ 该 VA 上是原始代码 ⇒ 我们的桩不在了
            LOGW(TPL_LOG " %s 装钩丢失（原始序言 @%p）— 重新装钩", w.tag, va);
            w.done->store(false);
        }
    }
    // R5b：三处移速桩（apply + 两个 reset）统一做丢失看护：原序言重现 ⇒ 映射被替换/还原
    struct MStub { const char* tag; uintptr_t off; const uint32_t* pro; std::atomic<bool>* done; };
    static MStub kMStub[] = {
        {"MoveSpeed",  kFileOffMoveWrite,  kPrologueMove,      &g_movePatched},
        {"MoveResetA", kFileOffMoveResetA, kPrologueMoveReset, &g_moveResetAPatched},
        {"MoveResetB", kFileOffMoveResetB, kPrologueMoveReset, &g_moveResetBPatched},
    };
    for (MStub& m : kMStub) {
        if (!m.done->load()) continue;
        uint32_t w0 = 0;
        void* va = PhdrVaForFileOff(TPL_LIB_NAME, m.off);
        if (va && ReadU32Quiet(va, &w0) && w0 == m.pro[0]) {
            LOGW(TPL_LOG " %s 桩丢失 @%p — 重新打桩", m.tag, va);
            m.done->store(false);
        }
    }
    // 移速三站点命中计数（诊断）：开了倍率却长期零命中 ⇒ 这些站点不在（玩家）移动路径上
    if (g_zzzMoveOn) {
        unsigned h  = g_zzzMoveHits;
        unsigned ho = g_zzzMoveOnHits;
        unsigned ra = g_zzzMoveResetAHits;
        unsigned rb = g_zzzMoveResetBHits;
        if (h  != g_moveHitsLast.load() || ra != g_moveResetALast.load() ||
            rb != g_moveResetBLast.load()) {
            LOGI(TPL_LOG " MoveSpeed 命中 apply=%u(乘倍率 %u) resetA=%u(id=%u) resetB=%u(id=%u)",
                 h, ho, ra, (unsigned)g_zzzMoveSiteIdA, rb, (unsigned)g_zzzMoveSiteIdB);
            g_moveHitsLast.store(h);
            g_moveResetALast.store(ra);
            g_moveResetBLast.store(rb);
            g_moveIdleTicks.store(0);
        } else if (++g_moveIdleTicks == 6) {
            LOGW(TPL_LOG " MoveSpeed: 已开倍率但 30s 三站点零命中 — 可能在别的移动路径上（待换站）");
        }
    } else {
        g_moveIdleTicks.store(0);
    }

    // R5b 链自检：改 wall-clock 节流（Tick 远快于 5s，R5a 的 12 次配额 1 秒内就烧光了）。
    //   30s 一次走简明档；结果翻转时补一份逐级详情 + 一行结论。
    static std::atomic<long long> s_lastTest{0};
    static int s_lastOkState = -1;   // 仅 WatchHooks 线程读写
    const long long nowS = static_cast<long long>(time(nullptr));
    if (nowS - s_lastTest.load() >= 30) {
        s_lastTest.store(nowS);
        uint32_t id = 0;
        bool ok = ChainSelfTest(false, &id);
        int st = ok ? 1 : 0;
        if (st != s_lastOkState) {
            s_lastOkState = st;
            if (!ok) ChainSelfTest(true, &id);   // 翻转才补逐级详情
            LOGI(TPL_LOG " 链自检结果 ok=%d id=%u set=%d vote=%u/%u",
                 (int)ok, (unsigned)id, MeIdCount(),
                 (unsigned)g_ownVoteVal.load(), (unsigned)g_ownVoteN.load());
        }
    }
}

void Zzz_Tick(const bool* features, const int* values) {
    if (features && values) {
        g_inv.store(features[ZZZ_FEAT_INV]);
        g_skill.store(features[ZZZ_FEAT_SKILL]);
        g_freeze.store(features[ZZZ_FEAT_FREEZE]);
        int mv = values[ZZZ_FEAT_MOVE];
        int dm = values[ZZZ_FEAT_DMG];
        if (mv < 1) mv = 1;
        if (mv > ZZZ_MOVE_MUL_MAX) mv = ZZZ_MOVE_MUL_MAX;
        if (dm < 1) dm = 1;
        if (dm > ZZZ_DMG_MUL_MAX) dm = ZZZ_DMG_MUL_MAX;
        g_moveMul.store(mv);
        g_dmgMul.store(dm);
        g_zzzMoveOn  = (unsigned char)(mv > 1 ? 1 : 0);
        g_zzzMoveMul = static_cast<float>(mv);
    }

    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB) && RealModuleReady()) {
        InstallFaultGuard();
        g_libReady.store(true);
        LOGI(TPL_LOG " lib ready (via Tick fallback, real module)");
    }
    if (g_libReady.load()) WatchHooks();
    SyncHooks();
}

void Zzz_ForceClear() {
    g_inv.store(false);
    g_skill.store(false);
    g_freeze.store(false);
    g_moveMul.store(1);
    g_dmgMul.store(1);
    g_zzzMoveOn  = 0;
    g_zzzMoveMul = 1.0f;
    g_reenter.store(false);
}
