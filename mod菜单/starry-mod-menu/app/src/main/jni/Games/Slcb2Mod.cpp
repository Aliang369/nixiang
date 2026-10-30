// ============================================================================
//  星光菜单游戏模块 — 失落城堡2（`com.leiting.slcb2`）1.0.3 / vc10
//
//  底包：**官方原版**（`CN=lt`，`libil2cpp.so` md5 `9aede69e…`，metadata md5 `6e94e96b…`）
//  引擎：Unity IL2CPP metadata v31 + HybridCLR；保护＝网易易盾 4.23 + 乐变 LBApplication
//
//  功能集（5 项，用户 2026-10-30 定稿）：
//    1 伤害倍率（滑块 1-100，1＝关）  2 无敌  3 无限技能（免CD + 不耗蓝）
//    5 无限魔铁锭                    6 无限魂晶
//    ⚠️ 2026-10-30 用户明令：「菜单内的全装扮部分给相关代码都删除掉」⇒ 本文件
//       **不含** SLCB2_FEAT_SKIN / 装扮相关槽位；也**不含**任何诊断探针
//       （K_COUNT / K_MIDPROBE / K_PROBE_LR）、frida-gadget 装载、现场指令自证等
//       测试脚手架。要复现当时的取证形态请查 git 历史（a5e9a46e 及更早）。
//
//  ★ 内置防闪退设施是「代码」不是「文档约定」，删任何一节都可能复现历史闪退：
//      默认 0 hook（懒装） / RVA 漂移拒绝 / 幂等安装 / reenter 防递归 /
//      SafeWrite 校验写 / Tick 轻量 / 并发装钩互斥 / 致命信号兜底 / 槽位熔断
//
//  ★ 本作铁律（`10_docs/失落城堡2/路线锁定.md` §11/§12）：
//      ①功能一律做「**图片内自足补丁**」——入口 4B `b` → libil2cpp 自己的 code cave
//        里改 s0/w0 再跳回；**禁止跳进 libstarry_magic.so**（实测：任何进我们 so 的
//        形态都会在挂钩命中后 ~390ms 崩）。
//      ②Dobby trampoline 在本包不可用（匿名非执行页）⇒ **禁止回跳 orig**，
//        带副作用的函数**禁止整段跳过函数体**，只改需要的那一处。
//  铁律全文：docs/GAME_MOD_GUIDE.md + skill/re-implement/hook-rules.md
// ============================================================================

#include "Slcb2Mod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>

#include <unistd.h>
#include <fcntl.h>
#include <pthread.h>
#include <mutex>      // §④ 装钩互斥
#include <setjmp.h>   // §④ sigsetjmp 保护区
#include <signal.h>   // §④ 致命信号兜底

#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"

// 日志过滤：adb logcat -d | grep "\[Slcb2\]"
#define TPL_LOG "[Slcb2]"

// 目标库名（易盾壳下同失落城堡 1：模块名不带明文全称，走 SecStrings）
#define TPL_TARGET_LIB sec_lib_il2cpp()

// ============================================================================
//  ① 开关与状态
//     g_libReady = 「库已映射」≠「已挂钩」；全开关关闭时保持 0 个补丁。
// ============================================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_reenter{false};
static std::atomic<bool> g_dmg{false};      // 伤害倍率「功能开启」（滑块值 >1 才算生效）
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_skill{false};    // 无限技能（免CD + 不耗蓝）
static std::atomic<bool> g_iron{false};     // 无限魔铁锭
static std::atomic<bool> g_crystal{false};  // 无限魂晶
static std::atomic<int>  g_mult{1};         // 伤害倍率 1..100（1 = 关闭）

// ============================================================================
//  ④ 致命信号兜底（sigsetjmp 保护区 + sigaltstack）
// ============================================================================
static std::mutex g_installMutex;   // 保护「装钩全过程」，只在 SyncHooks 里取
static std::mutex g_guardMutex;

static thread_local sigjmp_buf              t_faultJmp;
static thread_local volatile sig_atomic_t   t_inGuard    = 0;
static thread_local volatile sig_atomic_t   t_faultDepth = 0;
static thread_local volatile int            t_faultSig   = 0;
static thread_local volatile void*          t_faultAddr  = nullptr;
static thread_local char t_altStack[65536];
static thread_local bool t_altStackSet = false;

static struct sigaction g_oldSegv;
static struct sigaction g_oldBus;
static struct sigaction g_oldIll;
static std::atomic<bool> g_guardArmed{false};

static void TplFaultHandler(int sig, siginfo_t* si, void* /*uc*/) {
    if (t_inGuard) {
        if (++t_faultDepth > 4) { signal(sig, SIG_DFL); raise(sig); return; }
        t_faultSig  = sig;
        t_faultAddr = si ? si->si_addr : nullptr;
        siglongjmp(t_faultJmp, 1);
    }
    const struct sigaction* old = (sig == SIGSEGV) ? &g_oldSegv
                               : (sig == SIGBUS)  ? &g_oldBus : &g_oldIll;
    if (old->sa_sigaction && old->sa_sigaction != TplFaultHandler) {
        old->sa_sigaction(sig, si, nullptr);
        return;
    }
    signal(sig, SIG_DFL);
    raise(sig);
}

static void ArmFaultGuardForThread() {
    if (t_altStackSet) return;
    t_altStackSet = true;
    stack_t ss; std::memset(&ss, 0, sizeof(ss));
    ss.ss_sp = t_altStack; ss.ss_size = sizeof(t_altStack); ss.ss_flags = 0;
    if (sigaltstack(&ss, nullptr) != 0) LOGW(TPL_LOG " sigaltstack failed — 降级为无备用栈");
}

static void InstallFaultGuard() {
    if (g_guardArmed.load()) return;
    std::lock_guard<std::mutex> lk(g_guardMutex);
    if (g_guardArmed.load()) return;
    struct sigaction sa; std::memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = TplFaultHandler;
    sa.sa_flags     = SA_SIGINFO | SA_ONSTACK | SA_NODEFER;
    sigemptyset(&sa.sa_mask);
    sigaction(SIGSEGV, &sa, &g_oldSegv);
    sigaction(SIGBUS,  &sa, &g_oldBus);
    sigaction(SIGILL,  &sa, &g_oldIll);
    g_guardArmed.store(true);
    LOGI(TPL_LOG " fault guard armed（SIGSEGV/SIGBUS/SIGILL）");
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
    LOGW(TPL_LOG " %s 读序言 SIG(%d) @%p — 跳过该槽位", tag, (int)t_faultSig, (void*)t_faultAddr);
    return false;
}

// ---- file_off → 运行时 VA（分段映射，禁 base+RVA） ----
static bool IsLibMapLine(const char* line, const char* lib) {
    return std::strstr(line, lib) != nullptr && std::strstr(line, ".so") != nullptr;
}

static void* FileOffToVa(const char* lib, uintptr_t fileOff) {
    FILE* fp = std::fopen("/proc/self/maps", "r");
    if (!fp) return nullptr;
    char line[768];
    void* best = nullptr; uintptr_t bestSpan = 0;
    while (std::fgets(line, sizeof(line), fp)) {
        if (!IsLibMapLine(line, lib)) continue;
        uintptr_t start = 0, end = 0, off = 0; char perm[8] = {};
        if (std::sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!std::strchr(perm, 'x')) continue;
        if (end <= start) continue;
        uintptr_t span = end - start;
        if (fileOff >= off && fileOff < off + span && span > bestSpan) {
            bestSpan = span;
            best = reinterpret_cast<void*>(start + (fileOff - off));
        }
    }
    std::fclose(fp);
    return best;
}

// ---- 代码写：/proc/self/mem + pwrite（不 mprotect(RW)，不留页属性痕迹） ----
static bool WriteCode(void* addr, const void* data, size_t n) {
    static int fd = -1;
    if (fd < 0) fd = ::open("/proc/self/mem", O_RDWR);
    if (fd < 0) return false;
    if (::pwrite(fd, data, n, (off_t)(uintptr_t)addr) != (ssize_t)n) return false;
    __builtin___clear_cache((char*)addr, (char*)addr + n);
    return true;
}

// ---- code cave 分配（16B 对齐 + 区间登记防重叠） ----
static const int kCaveCap = 32;
static uintptr_t g_caveAddr[kCaveCap];
static size_t    g_caveLen[kCaveCap];
static int       g_caveUsed = 0;

static bool CaveOverlaps(uintptr_t a, size_t n) {
    for (int i = 0; i < g_caveUsed; ++i) {
        uintptr_t b = g_caveAddr[i], e = b + g_caveLen[i];
        if (a < e && b < a + n) return true;
    }
    return false;
}
static void CaveReserve(uintptr_t a, size_t n) {
    if (g_caveUsed < kCaveCap) { g_caveAddr[g_caveUsed] = a; g_caveLen[g_caveUsed] = n; ++g_caveUsed; }
}
static void* FindCodeCave(void* near, size_t need) {
    FILE* fp = std::fopen("/proc/self/maps", "r");
    if (!fp) return nullptr;
    char line[768]; uintptr_t lo = 0, hi = 0, cur = (uintptr_t)near;
    while (std::fgets(line, sizeof(line), fp)) {
        uintptr_t s = 0, e = 0; char perm[8] = {};
        if (std::sscanf(line, "%lx-%lx %7s", &s, &e, perm) != 3) continue;
        if (cur >= s && cur < e && std::strchr(perm, 'x')) { lo = s; hi = e; break; }
    }
    std::fclose(fp);
    if (!hi) return nullptr;
    // 先在后面找，再在前面找
    for (int pass = 0; pass < 2; ++pass) {
        uintptr_t from = pass == 0 ? (cur + 0x1000) : (lo + 0x10);
        uintptr_t to   = pass == 0 ? hi : cur;
        for (uintptr_t a = (from + 15) & ~(uintptr_t)15; a + need <= to; a += 16) {
            if (a + need > hi) break;
            if (CaveOverlaps(a, need)) continue;
            const volatile uint8_t* p = (const volatile uint8_t*)a;
            size_t k = 0;
            for (; k < need; ++k) if (p[k] != 0x00 && p[k] != 0xFF) break;
            if (k == need) return (void*)a;
        }
    }
    return nullptr;
}

// ============================================================================
//  ⑤ 槽位表（**本版唯一权威**，与 `10_docs/失落城堡2/路线锁定.md` §1 一一对应）
//     全部图片内补丁、零外部调用；kind 决定补丁形态。
// ============================================================================
enum SlotKind {
    K_SCALE_DMG = 0,   // 入口 → cave：乘倍率后回放序言（伤害倍率 0x110829A0）
    K_SCALE_INV,       // 入口 → cave：乘 0 后回放序言（无敌 0x110649CC）
    K_RESULT,          // 只改结果指令（免CD / 不耗蓝，带副作用的函数禁止整段跳过）
    K_STUB9999,        // 入口 → cave：mov w0,#9999; ret（货币读侧 pin）
};

struct HookSlot {
    const char* tag;
    uintptr_t   rva;      // 入口（结果类槽位仅作就绪判断）
    int         feat;
    SlotKind    kind;
    uintptr_t   rva2;     // 结果指令地址（仅 K_RESULT）
    uint32_t    oldW;     // 期望原字（0＝不做指纹校验）
    uint32_t    newW;     // 目标字
};

static const HookSlot kSlots[] = {
    // ── 1 伤害倍率：怪物掉血函数入口。ABI `float f(void* self, u64, u64, u64, void*, float dmg)`
    //    首浮点参数在 s0；cave 里 `fmul s0, s0, 倍率` 后回放 4 条序言再跳回 entry+0x10。
    //    倍率字面量在 cave+40，滑块变化由 RefreshScaleLiteral() 原地刷新（不重装）。
    { "DmgMul",     0x110829A0UL, SLCB2_FEAT_DMG,     K_SCALE_DMG, 0,            0,          0          },
    // ── 2 无敌：玩家受伤函数入口，同上形状（入口 `fmov s8,s0`），cave 里乘 0.0f ⇒ 返 0。
    { "Inv",        0x110649CCUL, SLCB2_FEAT_INV,     K_SCALE_INV, 0,            0,          0          },
    // ── 3 无限技能·免CD：返回 bool（尾部 `cset w0,ge`）；把结果指令改成 `mov w0,#1`。
    { "SkillCd",    0x10CE8C24UL, SLCB2_FEAT_SKILL,   K_RESULT,    0x10CE8C80UL, 0x1A9FB7E0u, 0x52800020u },
    // ── 3 无限技能·不耗蓝：返回 int32（尾部 `csel w0,w8,w9,eq`，**不是 float**；
    //    首版按 float 返 w0 垃圾 ⇒ 战斗闪退，勿回退）。结果指令改 `mov w0,wzr`。
    { "SkillMp",    0x118DD824UL, SLCB2_FEAT_SKILL,   K_RESULT,    0x118DD904UL, 0x1A890100u, 0x2A1F03E0u },
    // ── 5 无限魔铁锭：`LC2.BagSystem::get_IronPowder`（rva_map: 0x06001659 / 0x114d97d0）
    //    读侧 pin，不写 Obscured 字段、不进 RPC（ACTk ObscuredCheatingDetector 只抓混淆字段被写）。
    { "GetIron",    0x114D97D0UL, SLCB2_FEAT_IRON,    K_STUB9999,  0,            0,          0          },
    // ── 6 无限魂晶：`LC2.BagSystem::get_Crystal`（rva_map: 0x06001657 / 0x114ca960）
    { "GetCrystal", 0x114CA960UL, SLCB2_FEAT_CRYSTAL, K_STUB9999,  0,            0,          0          },
};
static const int kSlotsCount = (int)(sizeof(kSlots) / sizeof(kSlots[0]));

static const int kSlotCap = 24;
static void*         g_slotTarget[kSlotCap]    = {};
static bool          g_slotInstalled[kSlotCap] = {};
static bool          g_slotPatched[kSlotCap]   = {};
static unsigned char g_slotSaved[kSlotCap][4]  = {};
static void*         g_slotCave[kSlotCap]      = {};
static void*         g_lastHookTarget          = nullptr;
static int           g_lastWant = -1, g_lastHave = -1;
static std::atomic<int> g_installedCount{0};
static const int kSlotsMax = 64;
static bool g_slotDead[kSlotsMax];

static bool FeatOn(int feat) {
    switch (feat) {
        case SLCB2_FEAT_DMG:     return g_mult.load() > 1;
        case SLCB2_FEAT_INV:     return g_inv.load();
        case SLCB2_FEAT_SKILL:   return g_skill.load();
        case SLCB2_FEAT_IRON:    return g_iron.load();
        case SLCB2_FEAT_CRYSTAL: return g_crystal.load();
        default: return false;
    }
}

// 伤害倍率字面量刷新（cave+40）
static void RefreshScaleLiteral() {
    for (int i = 0; i < kSlotsCount; ++i) {
        if (kSlots[i].kind != K_SCALE_DMG) continue;
        if (!g_slotInstalled[i] || !g_slotCave[i]) return;
        float m = (float)g_mult.load();
        WriteCode((char*)g_slotCave[i] + 40, &m, 4);
        return;
    }
}

// 入口 4B 补丁 → cave（各形态）
static bool PatchJumpSlot(int i) {
    const HookSlot& sl = kSlots[i];
    void* va = FileOffToVa(TPL_TARGET_LIB, sl.rva);
    if (!va) return false;

    // ---- 结果指令类：只改一条指令 ----
    if (sl.kind == K_RESULT) {
        void* pa = FileOffToVa(TPL_TARGET_LIB, sl.rva2);
        if (!pa) return false;
        uint32_t now = 0;
        if (!ReadU32Guarded(sl.tag, pa, &now)) return false;
        if (now == sl.newW) { g_slotTarget[i] = pa; g_lastHookTarget = pa; return true; }
        if (now != sl.oldW) { return false; }            // 页面未解密或版本不符 → 下一拍再试
        memcpy(g_slotSaved[i], &now, 4);
        if (!WriteCode(pa, &sl.newW, 4)) return false;
        g_slotTarget[i] = pa; g_lastHookTarget = pa;
        LOGI(TPL_LOG " slot[%d] %s 结果指令已改：@rva 0x%lX 0x%08X → 0x%08X",
             i, sl.tag, (unsigned long)sl.rva2, sl.oldW, sl.newW);
        return true;
    }

    uint32_t w0 = 0;
    if (!ReadU32Guarded(sl.tag, va, &w0)) return false;
    if ((w0 & 0xFC000000u) == 0x14000000u) return false;   // 已是 b（我们或跳板）→ 稍后再判

    // ---- 常量 stub（货币：mov w0,#9999; ret）----
    if (sl.kind == K_STUB9999) {
        // 入口指纹（hook-rules §22 第 4 步）：oldW 非 0 时必须与现读字面量一致，
        // 否则判为版本漂移/页面未解密，本拍不装（绝不盲改）。
        if (sl.oldW && w0 != sl.oldW) return false;
        void* sc = g_slotCave[i] ? g_slotCave[i] : FindCodeCave(va, 16);
        if (!sc) { LOGW(TPL_LOG " slot[%d] %s 找不到 16B cave", i, sl.tag); return false; }
        g_slotCave[i] = sc; CaveReserve((uintptr_t)sc, 16);
        const uint32_t imm = 9999u;
        uint32_t code[2];
        code[0] = 0x52800000u | ((imm & 0xFFFFu) << 5);   // mov w0, #9999
        code[1] = 0xD65F03C0u;                             // ret
        if (!WriteCode(sc, code, 8)) return false;
        int64_t d = (int64_t)((uintptr_t)sc - (uintptr_t)va);
        if ((d & 3) || d > (128LL << 20) || d < -(128LL << 20)) return false;
        uint32_t bi = 0x14000000u | ((uint32_t)((d >> 2) & 0x03FFFFFFu));
        memcpy(g_slotSaved[i], &w0, 4);
        if (!WriteCode(va, &bi, 4)) return false;
        g_slotTarget[i] = va; g_lastHookTarget = va;
        LOGI(TPL_LOG " slot[%d] %s stub 已装：入口 0x%08X → cave %p（mov w0,#%u; ret）",
             i, sl.tag, bi, sc, imm);
        return true;
    }

    // ---- 缩放类（伤害/无敌）：乘倍率 + 回放 4 条序言 + 跳回 entry+0x10 ----
    uint32_t orig[4];
    orig[0] = w0;
    for (int k = 1; k < 4; ++k)
        if (!ReadU32Guarded(sl.tag, (const char*)va + 4 * k, &orig[k])) return false;

    void* sc = g_slotCave[i] ? g_slotCave[i] : FindCodeCave(va, 48);
    if (!sc) { LOGW(TPL_LOG " slot[%d] %s 找不到 48B cave", i, sl.tag); return false; }
    g_slotCave[i] = sc; CaveReserve((uintptr_t)sc, 48);

    float mult = (sl.kind == K_SCALE_INV) ? 0.0f : (float)g_mult.load();
    unsigned char b[48]; std::memset(b, 0, sizeof b);
    uint32_t cw[7];
    cw[0] = 0x1C000141u;   // ldr  s1, [pc, #40]   → +40
    cw[1] = 0x1E210800u;   // fmul s0, s0, s1
    cw[2] = orig[0]; cw[3] = orig[1]; cw[4] = orig[2]; cw[5] = orig[3];
    cw[6] = 0x58000051u;   // ldr  x17, #8         → +32
    memcpy(b, cw, sizeof cw);
    uint32_t br = 0xD61F0220u; memcpy(b + 28, &br, 4);
    uint64_t back = (uint64_t)(uintptr_t)((const char*)va + 16);
    memcpy(b + 32, &back, 8);
    memcpy(b + 40, &mult, 4);
    if (!WriteCode(sc, b, sizeof b)) return false;

    int64_t d = (int64_t)((uintptr_t)sc - (uintptr_t)va);
    if ((d & 3) || d > (128LL << 20) || d < -(128LL << 20)) return false;
    uint32_t bi = 0x14000000u | ((uint32_t)((d >> 2) & 0x03FFFFFFu));
    memcpy(g_slotSaved[i], &w0, 4);
    if (!WriteCode(va, &bi, 4)) return false;
    g_slotTarget[i] = va; g_lastHookTarget = va;
    LOGI(TPL_LOG " slot[%d] %s 缩放 stub 已装：入口 0x%08X → cave %p（×%.2f）", i, sl.tag, bi, sc, (double)mult);
    return true;
}

static bool InstallSlot(int i) {
    if (i < 0 || i >= kSlotsCount) return false;
    if (g_slotInstalled[i]) return true;
    if (i < kSlotsMax && g_slotDead[i]) return false;
    g_lastHookTarget = nullptr;
    g_slotPatched[i] = true;
    bool ok = PatchJumpSlot(i);
    if (!ok) g_slotPatched[i] = false;
    if (ok) g_slotInstalled[i] = true;
    return ok;
}

static void UninstallSlot(int i) {
    if (i < 0 || i >= kSlotsCount) return;
    if (!g_slotInstalled[i] || !g_slotTarget[i]) return;
    WriteCode(g_slotTarget[i], g_slotSaved[i], 4);
    g_slotInstalled[i] = false;
    g_slotPatched[i] = false;
    LOGI(TPL_LOG " slot[%d] %s 已还原", i, kSlots[i].tag);
}

// 洗校验：易盾可能把我们的补丁刷回去 → 发现不符就标未装，下一拍重装
static void WashCheckSlots() {
    for (int i = 0; i < kSlotsCount; ++i) {
        if (!g_slotInstalled[i] || !g_slotTarget[i]) continue;
        uint32_t now = 0;
        if (!ReadU32Guarded(kSlots[i].tag, g_slotTarget[i], &now)) continue;
        uint32_t want = 0;
        if (kSlots[i].kind == K_RESULT) want = kSlots[i].newW;
        else {
            int64_t d = (int64_t)((uintptr_t)g_slotCave[i] - (uintptr_t)g_slotTarget[i]);
            if (!(d & 3)) want = 0x14000000u | ((uint32_t)((d >> 2) & 0x03FFFFFFu));
        }
        if (want && now != want) {
            LOGW(TPL_LOG " slot[%d] %s 补丁被刷回（0x%08X）→ 下一拍重装", i, kSlots[i].tag, now);
            g_slotInstalled[i] = false;
            g_slotPatched[i] = false;
        }
    }
}

// ============================================================================
//  ⑫ 自行读 prefs（框架回调晚于首帧时对齐开关状态）
// ============================================================================
static void LoadOwnPrefs() {
    FILE* f = std::fopen("/data/user/0/com.leiting.slcb2/shared_prefs/starry_magic_prefs.xml", "r");
    if (!f) { LOGW(TPL_LOG " 读不到 prefs，沿用框架回调状态"); return; }
    char line[512];
    int nSet = 0;
    while (std::fgets(line, sizeof line, f)) {
        int idx = -1, val = 0; char bv[8] = {0};
        if (std::sscanf(line, " <int name=\"%d\" value=\"%d\"", &idx, &val) == 2) {
            if (idx == SLCB2_FEAT_DMG && val >= 1 && val <= 100) { g_mult.store(val); ++nSet; }
        } else if (std::sscanf(line, " <boolean name=\"%d\" value=\"%7[a-z]\"", &idx, bv) == 2) {
            const bool on = (bv[0] == 't');
            switch (idx) {
                case SLCB2_FEAT_INV:     g_inv.store(on);     ++nSet; break;
                case SLCB2_FEAT_SKILL:   g_skill.store(on);   ++nSet; break;
                case SLCB2_FEAT_IRON:    g_iron.store(on);    ++nSet; break;
                case SLCB2_FEAT_CRYSTAL: g_crystal.store(on); ++nSet; break;
                default: break;
            }
        }
    }
    std::fclose(f);
    LOGI(TPL_LOG " 自行读 prefs：恢复 %d 项（倍率=%d 无敌=%d 技能=%d 魔铁=%d 魂晶=%d）",
         nSet, g_mult.load(), (int)g_inv.load(), (int)g_skill.load(),
         (int)g_iron.load(), (int)g_crystal.load());
}

static void SyncHooks();

// ============================================================================
//  ⑨ 早装线程：lib 映射后自带重试（易盾会在启动后重写/解密代码页，必须反复试）
// ============================================================================
static std::atomic<bool> g_stopEarly{false};
static std::atomic<bool> g_earlyStarted{false};

static void* EarlyHookThread(void*) {
    int waits = 0;
    while (!g_stopEarly.load()) {
        if (!isLibraryLoaded(TPL_TARGET_LIB)) {
            if (++waits > 20000) break;
            usleep(2000);
            continue;
        }
        InstallFaultGuard();
        g_libReady.store(true);
        LOGI(TPL_LOG " 早装线程：lib 已映射 → 进入懒装重试循环");
        int tickCount = 0;
        for (;;) {
            usleep(500000);
            if (g_stopEarly.load()) return nullptr;
            SyncHooks();
            if (tickCount == 2) { LoadOwnPrefs(); SyncHooks(); }   // 1s 后按 prefs 自身状态对齐
            ++tickCount;
        }
    }
    return nullptr;
}

void Slcb2_StartEarlyHook() {
    if (g_earlyStarted.exchange(true)) return;
    pthread_t t;
    if (pthread_create(&t, nullptr, EarlyHookThread, nullptr) == 0) {
        pthread_detach(t);
        LOGI(TPL_LOG " 早装线程已启动");
    }
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    std::lock_guard<std::mutex> lk(g_installMutex);
    WashCheckSlots();
    int want = 0, have = 0;
    for (int i = 0; i < kSlotsCount && i < kSlotsMax; ++i) {
        if (FeatOn(kSlots[i].feat)) {
            ++want;
            if (InstallSlot(i)) ++have;
        } else {
            UninstallSlot(i);
        }
    }
    g_installedCount.store(have);
    if (want != g_lastWant || have != g_lastHave) {
        LOGI(TPL_LOG " hook 状态: want=%d have=%d/%d", want, have, kSlotsCount);
        g_lastWant = want; g_lastHave = have;
    }
}

// ============================================================================
//  ⑧ 对外接口（ModEntry.h 注册表契约，签名不可改）
// ============================================================================
bool Slcb2_TryInit(JavaVM* /*vm*/) {
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(TPL_TARGET_LIB)) return false;

    InstallFaultGuard();          // §④ 先装兜底，再装钩
    g_libReady.store(true);
    SyncHooks();                  // 已 ON 的开关立即装；全关保持 0 hook
    LOGI(TPL_LOG " lib ready (lazy hooks, in-image cave patches)");
    return true;
}

bool Slcb2_IsReady() {
    return g_libReady.load();
}

void Slcb2_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case SLCB2_FEAT_DMG:     g_dmg.store(enabled); break;
        case SLCB2_FEAT_INV:     g_inv.store(enabled); break;
        case SLCB2_FEAT_SKILL:   g_skill.store(enabled); break;
        case SLCB2_FEAT_IRON:    g_iron.store(enabled); break;
        case SLCB2_FEAT_CRYSTAL: g_crystal.store(enabled); break;
        default: break;
    }
    SyncHooks();
}

void Slcb2_OnValueChanged(int featNum, int value) {
    if (featNum != SLCB2_FEAT_DMG) return;
    int m = value;
    if (m < 1) m = 1;
    if (m > 100) m = 100;
    g_mult.store(m);
    g_dmg.store(m > 1);
    LOGI(TPL_LOG " 伤害倍率 = x%d", m);
    SyncHooks();
    RefreshScaleLiteral();          // 已装则把新倍率写进 cave 字面量
}

void Slcb2_Tick(const bool* features, const int* values) {
    if (!features) return;

    // ★滑块类功能：菜单里 features[] 对它恒为 false，真正的开关语义在 values[] 上。
    //   首版把 g_dmg 直接写 features[] ⇒ 每 200ms 被冲成 false、OnValueChanged 又置 true，
    //   形成「装了又拆」抖动（实测日志：hooked → 已拆钩 → hooked 反复），战斗里表现为没效果/崩。
    if (values) {
        int m = values[SLCB2_FEAT_DMG];
        if (m < 1) m = 1;
        if (m > 100) m = 100;
        g_mult.store(m);
    }
    {
        static std::atomic<int> tk{0};
        if ((tk.fetch_add(1) % 10) == 0) WashCheckSlots();
    }
    g_dmg.store(g_mult.load() > 1);
    g_inv.store(features[SLCB2_FEAT_INV]);
    g_skill.store(features[SLCB2_FEAT_SKILL]);
    g_iron.store(features[SLCB2_FEAT_IRON]);
    g_crystal.store(features[SLCB2_FEAT_CRYSTAL]);

    // 兜底：库晚于首帧映射时，这里补一次
    if (!g_libReady.load() && isLibraryLoaded(TPL_TARGET_LIB)) {
        InstallFaultGuard();
        g_libReady.store(true);
        LOGI(TPL_LOG " lib ready (via Tick fallback)");
    }
    SyncHooks();

    // ★ 禁止在此 SafeProbe / 扫堆 / 遍历对象 / 逐对象写（Tick 频率高，代价爆炸）
}

void Slcb2_ForceClear() {
    g_dmg.store(false);
    g_inv.store(false);
    g_skill.store(false);
    g_iron.store(false);
    g_crystal.store(false);
    g_mult.store(1);
    g_reenter.store(false);
}
