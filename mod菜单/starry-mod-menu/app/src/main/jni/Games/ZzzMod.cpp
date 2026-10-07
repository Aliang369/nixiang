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
 *              【R20】功能已删（2026-10-07 用户令）：桩与看护全部停装，注释仅备查，禁止复活
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
#include <cmath>
#include <time.h>
#include <fcntl.h>
#include <elf.h>
#include <link.h>
#include <mutex>
#include <chrono>
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
    kFileOffMoveApplyEntry = 0xfb26f14, // SetMoveSpeedRatio.apply entry (R5f hit diag)
    kFileOffActorRoot   = 0x1d749418,  // libil2cpp 尾部 BSS 全局（玩家 actorId 链根）
    // R19（2026-10-07）：R14/R16/R17/R18 的伤害倍率诊断站点（DmgSettle / VicLoopTail /
    //   VicMid）整条移除。MuMu/Houdini 实测卡死栈 = Hook_VicTail -> VicMidDump ->
    //   ReadU32Quiet：热路径上的 0x100 深读 + 故障守卫会把 UnityMain 卡死。
    //   详见 10_docs/绝区零/轮次台账.tsv 的 R18b/R19。
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
static const uint32_t kPrologueMoveApply[4] = {0xd10103ffu, 0xfd0013e8u, 0xf90017feu, 0xa9034ff4u};
static const uint32_t kPrologueFreeze[4] = {0xa9be57feu, 0xa9014ff4u, 0xd001c288u, 0xaa0103f4u};

// R5b（2026-10-06）：移速类「把速度重置为常量 1.0f」的两处站点（方法3 / 方法4 内 3 条写指令）。
//   反汇编实证（file 0xfb270e4 / 0xfb2720c 两个函数体）：
//     0x...16c  strb wzr,[x0,#0x104]  (0x3904101f)
//     0x...170  str  w8,[x0,#0xf8]   (0xb900f808)   w8 = 0x3f800000 = 1.0f
//     0x...174  str  w8,[x0,#0x100]  (0xb9010008)  ★R5b 勘误：原注释/常量误写 0xb9001008（偏移 0x10）
//   x0 = 按 [x19+0x60] 查表得到的「速度组件」；x19 = 实体（函数入口 mov x19,x0）。
//   与 apply 方法（file 0xfb26f14，已打桩的 0xfb2700c）同属一个速度比率子系统：
//   apply 写 ratio/flag=1，本两处 reset 写 ratio=1.0f/flag=0。
static const uintptr_t kFileOffMoveResetA     = 0xfb2716c;
static const uintptr_t kFileOffMoveResetAEnd  = 0xfb27178;
static const uintptr_t kFileOffMoveResetB     = 0xfb27294;
static const uintptr_t kFileOffMoveResetBEnd  = 0xfb272a0;
static const uint32_t  kPrologueMoveReset[3]  = {0x3904101fu, 0xb900f808u, 0xb9010008u};   // 0xb9010008 = str w8,[x0,#0x100]

// ============================================================================
//  ② 开关 / 数值（铁律：滑块 1 = 关闭，默认必须 1）
// ============================================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_reenter{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_skill{false};        // 无限技能 = 无限大招（池鸳 kRvaUltGate 补丁）
static std::atomic<bool> g_freeze{false};
static std::atomic<int>  g_moveMul{1};          // 移速功能已删（2026-10-07），此值恒为 1，仅供给保留的桩做诊断
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
static std::atomic<bool> g_moveEntryHooked{false};   // R5f: SetMoveSpeedRatio.apply entry hook (diag)
static std::atomic<unsigned> g_moveEntryHits{0};
static std::atomic<unsigned> g_moveEntryLast{0};

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
//  ④ 读取原语（R20：故障信号守卫整段移除）
//  R19 现场（09_test/_recorder/20261007_142643）：四项功能全关仍冻死，UnityMain /
//  UnityGfxDeviceW / SRPSubmitThread 全卡在 libhoudini 同一组帧，而菜单层（Android UI
//  线程）还活着 ⇒ 冻的是 Houdini 翻译层，不是我们的 handler。
//  头号根因：进程级替换 SIGSEGV/SIGBUS/SIGILL 处理器 + 热路径 sigsetjmp/siglongjmp。
//  Houdini 的惰性翻译/自修改代码处理本身依赖信号，guard 的 longjmp 会打断翻译器
//  内部状态机 ⇒ 全进程卡死。池鸳与常规做法都是裸读。
//  R20 铁律：本文件禁止再出现 sigaction / sigsetjmp / siglongjmp；所有读取只允许指向
//  「活对象或本进程已装载模块」的地址（调用点自行保证），裸读本身即安全。
// ============================================================================
static std::mutex g_installMutex;

static bool ReadU32Guarded(const char* tag, const void* p, uint32_t* out) {
    (void)tag;
    *out = *reinterpret_cast<const volatile uint32_t*>(p);
    return true;
}

static bool ReadPtrGuarded(const char* tag, const void* p, uint64_t* out) {
    (void)tag;
    *out = *reinterpret_cast<const volatile uint64_t*>(p);
    return true;
}

static bool DobbyHookGuarded(const char* tag, void* target, void* hook, void** orig) {
    int rc = DobbyHook(target, (dobby_dummy_func_t)hook, (dobby_dummy_func_t*)orig);
    if (rc != 0) {
        LOGW(TPL_LOG " %s dobby fail rc=%d", tag, rc);
        if (orig) *orig = nullptr;
        return false;
    }
    return true;
}

// 同一 key 的重复日志限制（防「每 tick 重试 + 刷日志」把主循环拖垮）
static bool RateLimitLog(const char* key, long long intervalSec) {
    constexpr int kMaxKeys = 16;
    static const char* s_keys[kMaxKeys] = {};
    static long long s_last[kMaxKeys] = {};
    int slot = -1;
    for (int i = 0; i < kMaxKeys; ++i) {
        if (s_keys[i] == key) { slot = i; break; }
        if (!s_keys[i]) { s_keys[i] = key; slot = i; break; }
    }
    if (slot < 0) slot = 0;
    const long long now = static_cast<long long>(time(nullptr));
    if (now - s_last[slot] < intervalSec) return false;
    s_last[slot] = now;
    return true;
}

// 序言校验：读地址前 16B；首访打印每条期望/实际，重试路径静默（由调用方限流）
static bool CheckPrologue(const char* tag, void* va, const uint32_t* want, int words, bool firstAttempt) {
    for (int i = 0; i < words; ++i) {
        uint32_t got = 0;
        if (!ReadU32Guarded(tag, reinterpret_cast<const char*>(va) + i * 4, &got)) return false;
        if (got != want[i]) {
            if (firstAttempt) {
                LOGW(TPL_LOG " %s drift [+%d] got=0x%08X want=0x%08X — 拒绝挂钩", tag, i * 4, got, want[i]);
            }
            return false;
        }
    }
    return true;
}

// 静默读（就绪探测/丢失看护用）。R20：裸读——故障守卫已移除（见上方 ④ 说明）。
static bool ReadU32Quiet(const void* p, uint32_t* out) {
    *out = *reinterpret_cast<const volatile uint32_t*>(p);
    return true;
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
// R20：裸读。调用点只在功能开启时走链（见 Hook_Inv 门控）。
static bool ReadPtrQuiet(const void* p, uint64_t* out) {
    *out = *reinterpret_cast<const volatile uint64_t*>(p);
    return true;
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
static std::atomic<unsigned> g_meTeam{0};      // 玩家方所有权 id（R5e：与 actor id 分开存，实测 =5）

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

// 链学到的 id 同时喂给「对攻关系表」（锁序：g_meMutex → g_sideMutex，单向）

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

[[maybe_unused]] static uint32_t FirstMeId() {
    std::lock_guard<std::mutex> lk(g_meMutex);
    return g_meIdN > 0 ? g_meIds[0] : 0u;
}

// R5e：改成「频次投票」而不是「连续同值」——怪打我方时会插入 own=0 的受击事件，
//   连续口径容易被非玩家值抢跑；频次口径下玩家攻击占绝对多数，稳。
//   投票结果只写 g_meTeam（所有权口径），【不再】混进 actor id 集。
// ============================================================================
//  R8：多角色归属（ZZZ 一队最多 3 人）。
//   R7b 实测（09_test/_recorder/20261007_015629）：同局我方出现两个 actor id
//     —— 5 和 6，两者都打同一条怪 12；而事件 +0x1C(own) 恒为 5。
//     只靠 own 只能保护 5 号（用户实测：角色2 无敌、角色1 不无敌）。
//   做法：不依赖链、不依赖新场景，用「对攻关系」在线扩我方集合：
//     种子：own（投票值）视为我方（实测 = 5）。
//     规则1：已知我方当攻击方，受击方未知 ⇒ 受击方 = 怪
//     规则2：已知我是怪打未知 ⇒ 未知记「被怪打过」
//     规则3：未知 id 打已知怪，且它被怪打过 ⇒ 同阵营 = 我方（互相对攻才是敌对关系）
//   安全边界：只有规则 3 才扩我方，且需「互相对攻」两条证据；怪打怪才会误判，本作单队对怪，风险低。
// ============================================================================
struct SideRec { uint32_t id; uint8_t mine, monster, hitByMonster, hitMonster; };
static SideRec    g_side[16] = {};
static std::mutex g_sideMutex;

// 调用方持 g_sideMutex
static SideRec* SideSlotLocked(uint32_t id) {
    SideRec* empty = nullptr;
    for (int i = 0; i < 16; ++i) {
        if (g_side[i].id == id) return &g_side[i];
        if (!g_side[i].id && !empty) empty = &g_side[i];
    }
    return empty;
}

static void SideMarkMine(uint32_t id) {
    if (!id) return;
    std::lock_guard<std::mutex> lk(g_sideMutex);
    SideRec* s = SideSlotLocked(id);
    if (!s) return;
    s->id = id;
    s->mine = 1;
    s->monster = 0;
}

static void SideResetHitStats() {
    std::lock_guard<std::mutex> lk(g_sideMutex);
    for (int i = 0; i < 16; ++i) {
        g_side[i].hitMonster = 0;
        g_side[i].hitByMonster = 0;
    }
}

static bool SideIsMine(uint32_t id) {
    if (!id) return false;
    std::lock_guard<std::mutex> lk(g_sideMutex);
    for (int i = 0; i < 16; ++i) if (g_side[i].id == id) return g_side[i].mine != 0;
    return false;
}

// 只吃 g_sideMutex（绝不碰 g_meMutex，避免与 IsMineActor 的锁序交叉）
static void ObserveDamagePair(uint32_t atk, uint32_t vic, uint32_t ownId) {
    if (!atk || !vic || atk == vic) return;
    std::lock_guard<std::mutex> lk(g_sideMutex);
    SideRec* A = SideSlotLocked(atk);
    SideRec* V = SideSlotLocked(vic);
    if (!A || !V) return;
    A->id = atk;
    V->id = vic;
    if (ownId) {
        if (atk == ownId) { A->mine = 1; A->monster = 0; }
        if (vic == ownId) { V->mine = 1; V->monster = 0; }
    }
    // 规则1：已知我方 ⇒ 对方是怪
    if (A->mine && !V->mine) V->monster = 1;
    if (V->mine && !A->mine) A->monster = 1;
    // 规则2：已知怪打未知 ⇒ 未知记「被怪打过」
    if (A->monster && !V->mine) V->hitByMonster = 1;
    // 规则3（R12 放宽）：未知 id 打已知怪 ⇒ 累计「打过怪」次数，≥2 次认定我方。
    //   R11 实测：旧口径还要求「被怪打过」（互相对攻）才扩列，角色 2（id 6）整局没被怪打中
    //   就永远进不了我方集合 ⇒ 切到该角色照样掉血。现在以「打过怪」为主证据，连续 2 次即扩
    //   （防单次误判）；「被怪打过」这条旧证据保留，命中即可一次生效。
    if (V->monster && !A->mine && !A->monster) {
        if (A->hitMonster < 255) ++A->hitMonster;
        if (A->hitMonster >= 2 || A->hitByMonster) {
            A->mine = 1;
            LOGI(TPL_LOG " 我方 id +%u（对攻判定：打过怪 %u ×%u，被怪打过=%u）",
                 (unsigned)atk, (unsigned)vic, (unsigned)A->hitMonster, (unsigned)A->hitByMonster);
        }
    }
}

struct OwnVote { uint32_t val; unsigned n; };
static OwnVote    g_ownVotes[4] = {};
static std::mutex g_ownVoteMutex;

static void ObserveAttackerOwner(uint32_t own) {
    if (!own) return;
    std::lock_guard<std::mutex> lk(g_ownVoteMutex);
    int slot = -1, empty = -1, minI = 0;
    for (int i = 0; i < 4; ++i) {
        if (g_ownVotes[i].val == own) { slot = i; break; }
        if (!g_ownVotes[i].val) { if (empty < 0) empty = i; }
        else if (g_ownVotes[i].n < g_ownVotes[minI].n) minI = i;
    }
    if (slot < 0) {
        slot = (empty >= 0) ? empty : minI;   // 表满时淘汰计数最小的槽
        g_ownVotes[slot].val = own;
        g_ownVotes[slot].n = 0;
    }
    const unsigned n = ++g_ownVotes[slot].n;
    int best = 0;
    for (int i = 1; i < 4; ++i) if (g_ownVotes[i].n > g_ownVotes[best].n) best = i;
    g_ownVoteVal.store(own);
    g_ownVoteN.store(n);
    const unsigned bestN = g_ownVotes[best].n;
    const uint32_t bestVal = g_ownVotes[best].val;
    if (bestN >= 3 && (g_meTeam.load() == 0 || bestN == 50)) {
        g_meTeam.store(bestVal);
        LOGI(TPL_LOG " 玩家方所有权 id = %u（投票 %u/%u/%u/%u，本次 own=%u）",
             (unsigned)bestVal, (unsigned)g_ownVotes[0].n, (unsigned)g_ownVotes[1].n,
             (unsigned)g_ownVotes[2].n, (unsigned)g_ownVotes[3].n, (unsigned)own);
    }
}

// NOTE: g_meTeam == the voted value of ev+0x1C (own) == the local player actor id.
// R5f (2026-10-07): the single rule for enemy/ally separation.
//   Evidence (R5e: two full event dumps + 40 INV-EVT lines):
//     enemy hits me  atk=12 own=5 vic=5 vow=0  |  I hit enemy  atk=5 own=5 vic=12 vow=0
//     => ev+0x1C(own) is 5 on BOTH sides, ev+0x3C(vow) is always 0.
//     own means "local player actor id" (the game's own SetMoveSpeedRatio / MonitorMoveSpeed
//     dereference the same global to find the local player), NOT the attacker's owner.
//     R5e bug: treating atkOwn==team as "attacker is mine" is always true => monster id 12
//     got learned as mine => monsters were invincible and the damage path was swallowed.
//   id is mine <=> (a) chain-derived player actor id | (b) own (local player id)
//                 (c) id set learned ONLY from 0x104 local-damage-record events
static bool IsMineActor(uint32_t id, uint32_t chainId, const char** why) {
    if (!id) return false;
    if (chainId && id == chainId) { if (why) *why = "chain"; return true; }
    const uint32_t own = g_meTeam.load();
    if (own && id == own) { if (why) *why = "own"; return true; }
    // R7b: the id set is a valid source again, but ONLY because its feed was cut down
    //   to chain successes (LearnMeId is now called from WalkActorChain / ChainSelfTest
    //   only - the 0x104-record learner that polluted it with enemy id 12 is gone).
    //   Why we need it: R6 battle evidence shows me=0(none) on all 40 INV-EVT lines,
    //   i.e. the actor chain does NOT resolve inside combat (root deref = 0x0), yet the
    //   player actor id learned earlier (hub / open world, e.g. 6) stays valid in
    //   combat - the event fields atk=6/vic=12 prove it. The 30s ChainSelfTest keeps
    //   feeding this set whenever the chain is alive, so one trip through a resolvable
    //   scene is enough for the whole session.
    if (IsMineId(id)) { if (why) *why = "set"; return true; }
    // R24: 暂停使用 pair 学习结果；实测出现过怪物 id 被扩成我方（vic=270/273）。
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
        if (ok3 && id) { if (outId) *outId = id; LearnMeId(id); SideMarkMine(id); return true; }
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
    if (WalkActorChain(root, outId)) { fails.store(0); LearnMeId(*outId); SideMarkMine(*outId); return true; }
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

static std::atomic<int> g_evtCalls{0};

static float U32AsF(uint32_t u) { float f = 0.0f; std::memcpy(&f, &u, sizeof(f)); return f; }

// R19：只保留每事件一行紧凑摘要（前 48 条），事件对象 0x100 全量 dump 已删除。
//   字段语义（R2+R4 实测）：+0x18 攻击方 actor / +0x1C 所有权；+0x38 受击方 / +0x3C 所有权。
static void DiagDumpEvent(void* ev, uint16_t type, uint32_t victim, uint32_t atk,
                          uint32_t atkOwn, uint32_t vicOwn, uint32_t me, const char* meSrc) {
    int n = g_evtCalls.fetch_add(1);
    if (n >= 48) return;
    uint32_t c10 = 0, c14 = 0;
    ReadU32Quiet(reinterpret_cast<const char*>(ev) + 0x10, &c10);
    ReadU32Quiet(reinterpret_cast<const char*>(ev) + 0x14, &c14);
    LOGI(TPL_LOG " EVT#%d type=0x%X atk=%u own=%u vic=%u vow=%u c10=0x%08X c14=0x%08X me=%u(%s) set=%d vote=%u/%u",
         n, (unsigned)type, (unsigned)atk, (unsigned)atkOwn, (unsigned)victim,
         (unsigned)vicOwn, (unsigned)c10, (unsigned)c14, (unsigned)me, meSrc ? meSrc : "-",
         MeIdCount(), (unsigned)g_ownVoteVal.load(), (unsigned)g_ownVoteN.load());
}

// R19（2026-10-07）：0x104 事件记录路线按 R15 定案为死路（写回成功但画面伤害不变），
//   配套的写回/还原/回读诊断一律删除，不再对事件记录做任何字段级扫描或写入。
//   伤害倍率在找到新的数值消费点前保持 no-op，禁止复活本段。

// R19：R9/R10 的 HP 探测/差分/学习整块删除（R12 卡死根因，0x200B×12 对象扫描）。
//   禁止以任何形式复活；伤害倍率新路线不得对任意对象做深读扫描。

// 事件字段（R2+R4 两局实测）：+0x14 高 16 位 = 事件类型；+0x16==0x85 即伤害事件；
//   +0x18 攻击方 actorId / +0x1C 攻击方所有权（玩家攻击时=玩家 id，怪物=0）
//   +0x38 受击方 actorId / +0x3C 受击方所有权（怪物=0）
// ① 无敌：受击方是玩家侧 ⇒ 不调原函数（= 池鸳 0x4e2c8 的过滤，但归属判定扩成多源，见下）
// ② 伤害倍率：R19 起暂时 no-op。R14~R18 的 DmgSettle/VicTail/VicMid 只读诊断
//   与 0x104 写回全部删除（前者在 MuMu/Houdini 热路径卡死，后者已证无效果）。
// R19: 诊断段（VicProbeEntity / VicMidDump / Hook_VicTail / Hook_VicMid /
//   DiagDumpAttackEvent）已整段删除，禁止以任何形式重新启用：卡死栈证据见轮次台账 R18b。
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
    (void)okW;   // R5f: vow(+0x3C) is always 0 in practice; kept only for the event dump

    const bool invOn  = g_inv.load();
    const int  mul    = g_dmgMul.load();

    // +0x1C（攻击方所有权）连续同值计票：id 集为空的兜底（R2/R4 实测玩家攻击时为 5）
    if (type == 0x85 && okO) ObserveAttackerOwner(atkOwn);

    uint32_t me = 0;
    const char* meSrc = "none";
    if (invOn) {   // R20：伤害倍率已 no-op，链只在无敌开启时走（全关态不碰游戏堆）
        if (ResolvePlayerActorId(&me)) { meSrc = "chain"; }
        else { me = 0; meSrc = "none"; }   // R5f: stale id cache no longer counts as the player
    }

    // R5f identity rule (see IsMineActor): chain id | own(local player id) | 0x104-learned set.
    //   R5e's own/vow based owner comparison is gone - own is constant 5 on both sides。
    if (type == 0x85 && !g_reenter.load()) {
        const uint32_t team = g_meTeam.load();

        // R8：先喂「对攻关系」表（可能在本次事件里把我方第二/第三个角色 id 认出来），
        //   再算出归属 —— 切换角色后新角色被打一次 + 回打一次就会入列。
        if (okK && okV) ObserveDamagePair(atk, victim, team);
        // R24: 新战斗的怪物 id 往往换一批，被怪打一次就清掉上一场的统计，
        // 避免旧的“打过怪”证据把新怪也拉进我方集合。
        if (team == 0) {
            static std::atomic<int> s_pairHits{0};
            const int hit = s_pairHits.fetch_add(1);
            if (hit < 3 || hit % 200 == 0) {
                SideResetHitStats();
            }
        }

        const uint32_t chainId = me;                   // 0 = chain did not resolve this time
        const char* whyV = nullptr;
        const char* whyA = nullptr;
        const bool vicMine = okV && IsMineActor(victim, chainId, &whyV);
        const bool atkMine = okK && IsMineActor(atk, chainId, &whyA);

        // 诊断（限流）：无敌开启时把伤害事件的归属判定打进日志；
        //   未判为我方的受击事件优先记录（这就是漏保护的失败形态）。
        if (invOn) {
            static std::atomic<int> nInvEvt{0}, nInvLog{0};
            // R11 lesson: the old cap (40) hid the interesting events (the unprotected victim).
            //   Always log a compact line when the victim is NOT recognised as mine (that IS the
            //   failure mode we hunt); keep a small budget for the rest.
            const bool interesting = !vicMine;
            if ((interesting || nInvEvt.fetch_add(1) < 24) && nInvLog.fetch_add(1) < 400) {
                LOGI(TPL_LOG " INV-EVT type=0x%X atk=%u own=%u vic=%u vow=%u team=%u set=%d me=%u(%s) atkMine=%d vicMine=%d",
                     (unsigned)type, (unsigned)atk, (unsigned)atkOwn, (unsigned)victim, (unsigned)vicOwn,
                     (unsigned)team, MeIdCount(), (unsigned)me, meSrc, (int)atkMine, (int)vicMine);
            }
        }

        // ① 无敌：受击方是玩家侧 ⇒ 不调原函数（= 池鸳 libcymod 0x4e2c8 的同款过滤）
        if (invOn && vicMine) {
            static std::atomic<int> n{0};
            const int k = n.fetch_add(1);
            if (k < 12) {
                LOGI(TPL_LOG " 无敌拦截: type=0x%X 受击=%u vow=%u(why=%s) atk=%u own=%u(why=%s) me=%u(%s)",
                     (unsigned)type, (unsigned)victim, (unsigned)vicOwn, whyV ? whyV : "-",
                     (unsigned)atk, (unsigned)atkOwn, whyA ? whyA : "-", (unsigned)me, meSrc);
            }
            if (k < 2) DiagDumpEvent(ev, type, victim, atk, atkOwn, vicOwn, me, meSrc);
            return;
        }

        // ② 伤害倍率：R22 起走属性写回（DmgAttrSync，挂在 0xc02a9f0 每帧站点，见下方 R22 段）。
        //   旧 0x104 写回（R15 实证无效）与 VicTail 深读（R18b 卡死）已全部移除，不在此处做任何事。
        if (mul > 1) {
            static std::atomic<int> nDmgHint{0};
            if (nDmgHint.fetch_add(1) == 0)
                LOGI(TPL_LOG " 伤害倍率 R22：属性写回已接管（本钩子不再参与伤害倍率）");
        }
    }

    if (g_origInv) g_origInv(self, ev, a2, a3, mth);
}

// R19: R9/R10 的 HP 差分/补扣后处理（Hook_InvPostDmg）已删除，禁止复活。

// ============================================================================
//  R22（2026-10-07）：伤害倍率 = 玩家「伤害加成属性」写回（池鸳 attack-writeback 机制，自研实现）
//
//  R21 失败的根因（本轮逐条反汇编池鸳 libcymod 后定案 —— 不是路线问题，是实现漏了两级）：
//    ① 记录解析漏了 5 级指针链：R21 直接读 *(P+0xb8)（P = *(base+0x1d749418)）⇒ 恒为 0；
//       池鸳 0x4f684 是从同一个根走 +0x08,+0x18,+0x10,+0x78,+0x00 五级后才 +0xb8 取 R。
//    ② selfSet/selfGet 取错来源：不是 R+0x08/R+0x10，而是 getComponent(R,0x1a) 派生出来的两个对象。
//    ③ actor id 在 R+0x38（不是 R+0x20）。
//  池鸳语义（libcymod 反汇编，RVA 全部为 libil2cpp）：
//    0x4e45c（挂在 0xc02a9f0 的 handler）→ 0x4f684(0x1d749418) 走 5 级 → +0xB8 = R；
//    0x4f118(R, out)：actor = *(u32*)(R+0x38)（为 0 即放弃）；
//        comp = getComponent(R, 0x1a, 0)（槽 0x130f50 = base+0xd937a30）；
//        C = *(comp+0xc8)；out+0x08 = C（写对象 selfSet）；out+0x10 = *(C+0x20)（读对象 selfGet）；
//    0x4f350(delta)：method = *(base+0x1d9299f8)；
//        get(selfGet, out2, propId=2) → cvt(out2+0x20, out2+0x28, 0) = 当前加成值；
//        set(selfSet, propId=2, method, mode=2, delta)；再 get 回读确认。
//    池鸳把滑块值 clamp 到 2000（0x7d0）后传 value-1，即「1 = 关闭」。
//  实测（2026-10-07 MuMu pid 26844，adb 只读步进）：修正后的链解析成功 ⇒
//    R = 0x780C22EC60、*(u32*)(R+0x38) = 5（我方玩家）⇒ 这条链在当前状态下是活的。
//
//  我们的差异（自研，不照抄池鸳）：
//    ① delta = 目标 - 当前（自校正），不是池鸳「上一次写入值」的缓存；
//    ② 解析结果缓存复用：战斗中链根可能为空（池鸳同样只写一次），解析失败时回落到上次成功的记录；
//    ③ 写前 actor 必须是我方；写后回读不符 ⇒ 永久停写（fail-closed，绝不盲写）；
//    ④ 只用定长裸读 + 有效性检查；无深读、无扫描、无信号守卫。
//  R19/R20 已否决路线（禁止复活）：0x104 事件记录写回、DmgSettle、VicTail/VicMid 深读、HP 探测。
// ============================================================================
enum : uintptr_t {
    kRvaAttrRecordRoot = 0x1d749418,   // 玩家记录链根（与 actor 链同根，RVA 空间）
    kRvaAttrGetComp    = 0xd937a30,    // getComponent(R, 0x1a, 0) → 属性组件
    kRvaAttrMethod     = 0x1d9299f8,   // 全局 method 对象槽
    kRvaAttrGet        = 0x10a02394,   // 读属性（out 结构，值在 +0x20）
    kRvaAttrCvt        = 0x13f27c60,   // 结构 → double
    kRvaAttrSet        = 0xddc8600,    // (self, propId, method, mode, double delta)
};
// 记录链：池鸳 0x4f684 的五级 + 调用方的 0xb8（R21 漏掉前五级 ⇒ 恒空）
static const uintptr_t kAttrChain[6] = {0x08, 0x18, 0x10, 0x78, 0x00, 0xb8};
static const uint32_t kAttrPropId = 2;
static const uint32_t kAttrMode   = 2;
// R24：2000 是池鸳自家滑块映射的 clamp，不是游戏端上限（0xddc8600 setter 反汇编无 2000 校验）。
// 我们仍防呆，但允许更高等级的伤害加成落到属性上。
static const double   kAttrMaxBonus = 9900.0;

static std::atomic<uintptr_t> g_rvaOrigin{0};        // libil2cpp RVA 原点（一次解析后缓存）
static std::atomic<bool>      g_dmgWriteDead{false};  // 写后校验失败 ⇒ 永久停写（fail-closed）
static std::atomic<bool>      g_dmgEverApplied{false};// 是否曾成功写入（关倍率时要回 0）
static std::atomic<unsigned>  g_dmgApplyCount{0};     // 成功写回次数
// R25：不再缓存 DmgAttrCtx；重新挑战/切战斗会释放旧对象，旧缓存就是本轮 SIGSEGV 根因。

// 用户态指针有效性：池鸳同款 x<0x10000 拒绝 + 8 字节对齐 + 高位上限（防止把垃圾当对象调）
static inline bool ValidUserPtr(uint64_t p) {
    return p >= 0x10000ULL && (p & 7ULL) == 0 && p < 0x0000800000000000ULL;
}

struct DmgAttrCtx { uint64_t rec, selfSet, selfGet, method; uint32_t actor; };

static long long MonoMs() {
    struct timespec ts;
    if (clock_gettime(CLOCK_MONOTONIC, &ts) != 0) return 0;
    return static_cast<long long>(ts.tv_sec) * 1000 + ts.tv_nsec / 1000000;
}

// 逐级裸读解析玩家记录（池鸳 0x4f684 + 0x4f118 同口径）；
//   任何一级不合法即返回 false（调用方限流重试 / 回落缓存，不写内存）。
//   注：getComponent 是本段唯一一次「进入游戏代码」的调用，与池鸳在同一个钩子上下文里做，
//   调用前已把 R 逐级校验过；返回值再校验一次才使用。
// 返回值：0 = 成功；非 0 = 失败的级号（落日志用，省得下次只能看到笼统的「未就绪」）
//   1=根指针空 2=链第 i 级断 3=链尾非法 4=actor=0 5=getComponent 失败
//   6=*(comp+0xc8) 非法 7=*(C+0x20) 非法 8=method 槽为空
static int DmgAttrResolve(uintptr_t org, DmgAttrCtx* c) {
    uint64_t p = 0, method = 0;
    if (!ReadPtrQuiet(reinterpret_cast<const void*>(org + kRvaAttrRecordRoot), &p) || !ValidUserPtr(p)) return 1;
    for (size_t i = 0; i < sizeof(kAttrChain) / sizeof(kAttrChain[0]); ++i) {
        if (!ValidUserPtr(p)) return 2;
        uint64_t nxt = 0;
        if (!ReadPtrQuiet(reinterpret_cast<const char*>(p) + kAttrChain[i], &nxt)) return 2;
        p = nxt;
    }
    if (!ValidUserPtr(p)) return 3;
    const uint64_t rec = p;
    uint32_t actor = 0;
    if (!ReadU32Quiet(reinterpret_cast<const void*>(rec + 0x38), &actor) || !actor) return 4;

    typedef void* (*FnGetComp)(void*, uint32_t, void*);
    FnGetComp getComp = reinterpret_cast<FnGetComp>(org + kRvaAttrGetComp);
    const uint64_t comp = reinterpret_cast<uint64_t>(getComp(reinterpret_cast<void*>(rec), 0x1a, nullptr));
    if (!ValidUserPtr(comp)) return 5;
    uint64_t selfSet = 0, selfGet = 0;
    if (!ReadPtrQuiet(reinterpret_cast<const void*>(comp + 0xc8), &selfSet) || !ValidUserPtr(selfSet)) return 6;
    if (!ReadPtrQuiet(reinterpret_cast<const void*>(selfSet + 0x20), &selfGet) || !ValidUserPtr(selfGet)) return 7;
    if (!ReadPtrQuiet(reinterpret_cast<const void*>(org + kRvaAttrMethod), &method) || !ValidUserPtr(method)) return 8;
    c->rec = rec; c->selfSet = selfSet; c->selfGet = selfGet; c->method = method; c->actor = actor;
    return 0;
}

// 读当前加成值（getter → 结构 → cvt 转 double）；任何一步不合法返回 false
//   self 只传「读对象 selfGet」（池鸳 0x4f350 同口径：getter 只用在 selfGet 上，
//   selfSet 是另一个对象，用 getter 去探它反而是多余的崩溃面，已删）。
static bool DmgAttrRead(uintptr_t org, uint64_t self, double* outVal) {
    typedef bool   (*FnGet)(void*, void*, uint32_t);
    typedef double (*FnCvt)(uint64_t, uint64_t, void*);
    if (!ValidUserPtr(self)) return false;
    alignas(16) unsigned char buf[0x80];
    std::memset(buf, 0, sizeof(buf));
    FnGet getFn = reinterpret_cast<FnGet>(org + kRvaAttrGet);
    if (!getFn(reinterpret_cast<void*>(self), buf, kAttrPropId)) return false;
    uint64_t v0 = 0, v1 = 0;
    std::memcpy(&v0, buf + 0x20, 8);
    std::memcpy(&v1, buf + 0x28, 8);
    FnCvt cvtFn = reinterpret_cast<FnCvt>(org + kRvaAttrCvt);
    const double v = cvtFn(v0, v1, nullptr);
    if (!std::isfinite(v) || v > 1e12 || v < -1e12) return false;   // NaN/异常值一律拒绝
    *outVal = v;
    return true;
}

// 把「伤害加成」同步到 desired（0 = 还原）。全程 fail-closed：解析不到/校验不过只打日志，不写。
// R25：解析失败绝不复用旧记录。重新挑战/换战斗会释放旧 rec/selfSet/selfGet；链断后继续调
// 游戏 getter/setter 就是 UnityMain SIGSEGV（fault 0x38）。宁可在过渡期不加成，也不用已释放对象。
static void DmgAttrSync(double desired) {
    if (g_dmgWriteDead.load()) return;
    if (desired <= 0.0 && !g_dmgEverApplied.load()) return;   // 从未写过 + 要 0 ⇒ 完全不动游戏内存
    const uintptr_t org = g_rvaOrigin.load();
    if (!org) return;

    DmgAttrCtx c = {};
    const int stage = DmgAttrResolve(org, &c);
    if (stage == 0) {
        if (g_dmgEverApplied.exchange(true)) {
            if (RateLimitLog("dmgResolveOk", 30))
                LOGI(TPL_LOG " 伤害倍率: 记录重新解析成功 actor=%u rec=%p selfSet=%p selfGet=%p method=%p",
                     (unsigned)c.actor, reinterpret_cast<void*>(c.rec), reinterpret_cast<void*>(c.selfSet),
                     reinterpret_cast<void*>(c.selfGet), reinterpret_cast<void*>(c.method));
        }
    } else {
        // R25：解析失败（战斗释放/过场/链根清空）时必须完全停手。
        // fault addr 0x38 的 SIGSEGV 就是旧 selfGet 继续传进游戏 getter/setter。
        if (RateLimitLog("dmgResolve", 5))
            LOGW(TPL_LOG " 伤害倍率: 玩家记录未就绪 stage=%d（本轮跳过，会自动重试）", stage);
        return;
    }
    // 铁律：记录里的 actor 必须是我方，否则拒写（防止把怪物记录当成玩家属性写）
    if (!IsMineActor(c.actor, 0, nullptr)) {
        if (RateLimitLog("dmgActor", 5)) LOGW(TPL_LOG " 伤害倍率: 记录 actor=%u 非我方，拒写", (unsigned)c.actor);
        return;
    }

    double cur = 0.0;
    if (!DmgAttrRead(org, c.selfGet, &cur)) {
        if (RateLimitLog("dmgRead", 5)) LOGW(TPL_LOG " 伤害倍率: 读属性失败（actor=%u），本轮不写", (unsigned)c.actor);
        return;
    }

    // R23 诊断：每 2s 一行状态 —— 「游戏会不会自己把加成清回 0」「滑块值有没有真的跟过去」
    //   全看这一行（R22 的 200ms 限频下，游戏自己清 0 时我们只看到零星几条 OK）。
    static std::atomic<long long> s_lastState{0};
    const long long nowStateMs = MonoMs();
    if (nowStateMs - s_lastState.load() >= 2000) {
        s_lastState.store(nowStateMs);
        LOGI(TPL_LOG " 伤害倍率: 状态 mul=%d cur=%.3f target=%.3f actor=%u",
             g_dmgMul.load(), cur, desired, (unsigned)c.actor);
    }

    const double delta = desired - cur;
    if (delta < 1e-6 && delta > -1e-6) { g_dmgEverApplied.store(true); return; }   // 已经等于目标

    // 首写/首次读回都把「当前值」落日志：属性单位（百分比 / 小数 / 原始点数）以它为校准依据
    if (g_dmgApplyCount.load() == 0)
        LOGI(TPL_LOG " 伤害倍率: 写前 cur=%.6f target=%.6f delta=%.6f (actor=%u)",
             cur, desired, delta, (unsigned)c.actor);

    typedef double (*FnSet)(void*, uint32_t, uint64_t, uint32_t, double);
    FnSet setFn = reinterpret_cast<FnSet>(org + kRvaAttrSet);
    setFn(reinterpret_cast<void*>(c.selfSet), kAttrPropId, c.method, kAttrMode, delta);

    double after = 0.0;
    if (!DmgAttrRead(org, c.selfGet, &after) || std::fabs(after - desired) > 1e-3) {
        g_dmgWriteDead.store(true);   // 写后校验失败：永久停写，绝不反复盲写
        LOGW(TPL_LOG " 伤害倍率: 写后校验失败 actor=%u cur=%.6f delta=%.6f after=%.6f（已永久停写）",
             (unsigned)c.actor, cur, delta, after);
        return;
    }

    g_dmgEverApplied.store(true);
    const unsigned n = g_dmgApplyCount.fetch_add(1) + 1;
    if (n <= 6 || n % 60 == 0)
        LOGI(TPL_LOG " 伤害倍率: 属性写回 OK #%u actor=%u %.6f -> %.6f (delta=%.6f)",
             n, (unsigned)c.actor, cur, after, delta);
}

// 菜单滑块是「倍率」（1..100，1=关）；池鸳那个属性是「加成值」（它的滑块默认 3000、clamp 2000）。
//   映射：bonus = (倍率 - 1) × 100（百分比口径），上限 9900（防呆，非游戏端 clamp）。
//   单位若不符，首写日志里的 cur/after 就是校准依据 —— 下一轮只改这一个常量。
static double DmgBonusFor(int mul) {
    if (mul <= 1) return 0.0;
    double v = static_cast<double>(mul - 1) * 100.0;
    if (v > kAttrMaxBonus) v = kAttrMaxBonus;
    return v;
}

// 每帧站点作为触发。R23：**去掉 200ms 限频** —— 实测（R22 记录仪 16:07–16:08）游戏会自己把
//   加成清回 0（#3 写 1600 → 8s 后 #4 读到 cur=0），限频会让加成只存在于 1/12 的帧里，
//   这正是「伤害不跟着滑块走」的直接原因。池鸳也是每帧都跑同一套（resolve + get + 必要时 set），
//   真正的写只在 cur != target 时发生，所以稳态下每帧只多一次 get。调用次数由状态日志控制。
static void DmgSyncTick() {
    const int mul = g_dmgMul.load();
    if (mul <= 1 && !g_dmgEverApplied.load()) return;   // 关着且从没写过 ⇒ 零开销
    DmgAttrSync(DmgBonusFor(mul));
}

// ② 伤害倍率：入口 s0 即倍率入参，乘滑块后透传（x1 原样转发，语义不变）
// ②b 该站点 R2/R3 实测是每帧 Update(dt)（入参 0.030~0.061 = 帧间隔），乘 dt 只会搞乱时序。
// R22：本站点只当「每帧触发」，不改入参；真正的伤害倍率走属性写回（DmgSyncTick）。
static void Hook_Dmg(void* self, float mult, void* mth) {
    if (g_origDmg) g_origDmg(self, mult, mth);
    (void)self; (void)mult; (void)mth;
    DmgSyncTick();
}

// R19: R14 的 DmgSettle 只读诊断钩已删除（R14b 定案：它是帧时间累积器，不是伤害结算）。

// 定怪：BehaviorManager 是全局单例（调用者自全局取 x0），Tick(self, entity, ...) 的 entity 才是「谁的行为」。
// v1（2026-10-06）：entity 类名可读且像玩家 ⇒ 不冻；其余跳过原函数（怪物行为停摆）。
// entity 类名前 12 次进日志，用于按实测类名收窄过滤。
static const char* SafeClassName(const char* tag, void* obj) {
    if (!obj) return nullptr;
    // R7: 本包 xdl+dynsym 解析 il2cpp 核心符号持续失败（R6 实测刷屏 347 行，且每次都是
    //   一次 xdl_open + 全量符号解析，落在每帧热路径上）。熔断：前 24 次允许重试
    //   （覆盖「首次调用早于 libil2cpp 映射完成」），之后不再进 Load。
    if (!GetApi().ready) {
        static std::atomic<int> nTry{0};
        if (nTry.fetch_add(1) >= 24) {
            static std::atomic<int> nFuse{0};
            if (nFuse.fetch_add(1) == 0)
                LOGW(TPL_LOG " %s: il2cpp api 不可用，类名过滤熔断（后续不再重试）", tag);
            return nullptr;
        }
        if (!GetApi().Load(TPL_TARGET_LIB)) return nullptr;
    }
    void* k = GetApi().object_class(obj);
    if (!k) return nullptr;
    const char* n = GetApi().class_name(k);
    if (!n || !*n) return nullptr;
    static std::atomic<int> nLog{0};
    if (nLog.fetch_add(1) < 12) LOGI(TPL_LOG " %s 候选类名: %s", tag, n);
    return n;
}

// ---- MoveSpeed entry hit diagnostic (R5f) -----------------------------------
//   The three 12B inline stubs (apply write site + two reset write sites) showed ZERO
//   hits over two 30s windows in R5e. This probe only counts, it changes nothing:
//   entry hits but no write-site hits => the method runs but bails out early (component
//   lookup failed); no entry hits either => the whole SetMoveSpeedRatio subsystem is
//   not on the player's movement path and the station has to be replaced.
typedef void (*FnMoveEntry_t)(void* self, void* a1, void* mth);
static FnMoveEntry_t g_origMoveEntry = nullptr;
static void Hook_MoveEntry(void* self, void* a1, void* mth) {
    const unsigned n = g_moveEntryHits.fetch_add(1);
    if (n < 5) {
        uint32_t ru = 0;
        float ratio = -1.0f;
        if (self && ReadU32Quiet(reinterpret_cast<const char*>(self) + 0x64, &ru)) ratio = U32AsF(ru);
        LOGI(TPL_LOG " MoveEntry hit #%u self=%p ratio(+0x64)=%.4f mul=%d",
             n, self, ratio, g_moveMul.load());
    }
    if (g_origMoveEntry) g_origMoveEntry(self, a1, mth);
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
    if (!CheckPrologue(tag, va, pro, words, true)) return false;
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
    const bool firstAttempt = RateLimitLog(tag, 5);
    // 幂等：若桩已在本轮落地（12B 为 adrp/add/br），直接认领，不重打（避免反复 DobbyCodePatch）
    {
        void* va0 = PhdrVaForFileOff(TPL_LIB_NAME, fileOff);
        if (!va0) va0 = FileOffToVa(TPL_LIB_NAME, fileOff);
        uint32_t g0 = 0, g1 = 0, g2 = 0;
        if (va0 && ReadU32Quiet(va0, &g0) &&
            ReadU32Quiet(reinterpret_cast<const char*>(va0) + 4, &g1) &&
            ReadU32Quiet(reinterpret_cast<const char*>(va0) + 8, &g2) &&
            g2 == 0xD61F0200u && (g0 & 0x9F00001Fu) == 0x90000010u &&
            (g1 & 0xFFC003FFu) == 0x91000210u) {
            *resumeSlot = resume;
            doneFlag->store(true);
            return true;
        }
    }
    static bool s_loggedDrift = false;
    const bool wantLog = firstAttempt && !s_loggedDrift;
    if (!CheckPrologue(tag, target, pro, 3, wantLog)) {
        if (wantLog) {
            uint32_t g0 = 0;
            ReadU32Guarded(tag, reinterpret_cast<const char*>(target), &g0);
            LOGW(TPL_LOG " %s: 序言不符（首指令 got=0x%08X want=0x%08X）— 降级重试（5s 一次）", tag, g0, pro[0]);
        }
        if (wantLog) s_loggedDrift = true;
        return false;
    }
    s_loggedDrift = false;

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

    int rc = DobbyCodePatch(target, reinterpret_cast<uint8_t*>(stub), sizeof(stub));
    if (rc != 0) {
        LOGW(TPL_LOG " %s: DobbyCodePatch rc=%d", tag, rc);
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

static bool InstallMoveEntryHook() {
    return InstallEntryHook("MoveEntry", kFileOffMoveApplyEntry, kPrologueMoveApply, 4,
                            (void*)Hook_MoveEntry, (void**)&g_origMoveEntry, &g_moveEntryHooked);
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
    uint32_t v = want;
    int rc = DobbyCodePatch(va, reinterpret_cast<uint8_t*>(&v), 4);
    if (rc != 0) {
        LOGW(TPL_LOG " 无限大招: DobbyCodePatch rc=%d", rc);
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
    // R19: DmgSettle / VicTail / VicMid 诊断钩一律不装（卡死根因，见 kFileOff* 注释）。
    // R20: 移速三桩 + MoveEntry 诊断钩全部停装（功能已删；残留桩/看护＝全关态活动源，禁止复活）。
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

    g_libReady.store(true);
    // R21：缓存 libil2cpp RVA 原点（伤害倍率属性写回用；dl_iterate_phdr 只在此处跑一次）
    if (!g_rvaOrigin.load()) g_rvaOrigin.store(PhdrRvaOrigin(TPL_LIB_NAME));
    LOGI(TPL_LOG " lib ready (real module via dl_iterate_phdr; PT_LOAD VA)");
    SyncHooks();
    // R20：启动期 ChainSelfTestVerbose 已移除——全关态不碰游戏堆（守卫已删，裸读链上指针不受控）。
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
    if (featNum == ZZZ_FEAT_DMG) {
        if (value < 1) value = 1;
        if (value > ZZZ_DMG_MUL_MAX) value = ZZZ_DMG_MUL_MAX;
        g_dmgMul.store(value);
        LOGI(TPL_LOG " 伤害倍率 = %d（%s）", value, value > 1 ? "on" : "off");
    }
    SyncHooks();
}

// 装钩丢失看护：目标处原始序言重新出现 ⇒ 映射被替换/还原，认丢失并清 doneFlag，
// 交给 SyncHooks 重挂（覆盖 MuMu 迟到重映射场景）。
// R20：① 30s 一轮（每轮 3 次 dl_iterate_phdr——loader 锁，禁止按 Tick 频率碰）；
//       ② 移速桩与命中诊断整块移除（功能已删，禁止复活）；
//       ③ 链自检只在无敌开启时跑（全关态不碰游戏堆对象）。
static void WatchHooks() {
    const long long nowS = static_cast<long long>(time(nullptr));

    static std::atomic<long long> s_lastWatch{0};
    if (nowS - s_lastWatch.load() >= 30) {
        s_lastWatch.store(nowS);
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
    }

    // 链自检：无敌的归属判定需要「玩家 id 集」，每 30s 借存活的 actor 链补喂一次。
    //   只在无敌开启时跑；结果翻转时补一份逐级详情 + 一行结论。
    static std::atomic<long long> s_lastTest{0};
    static int s_lastOkState = -1;   // 仅 WatchHooks 线程读写
    if (g_inv.load() && nowS - s_lastTest.load() >= 30) {
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
        int dm = values[ZZZ_FEAT_DMG];
        if (dm < 1) dm = 1;
        if (dm > ZZZ_DMG_MUL_MAX) dm = ZZZ_DMG_MUL_MAX;
        g_dmgMul.store(dm);
    }

    if (!g_libReady.load()) {
        // R20：就绪探测降频到 1s（dl_iterate_phdr 是 loader 锁，禁止按 Tick 频率碰）。
        static std::atomic<long long> s_lastReadyProbe{0};
        const long long nowS = static_cast<long long>(time(nullptr));
        if (nowS - s_lastReadyProbe.load() >= 1) {
            s_lastReadyProbe.store(nowS);
            if (isLibraryLoaded(TPL_TARGET_LIB) && RealModuleReady()) {
                g_libReady.store(true);
                if (!g_rvaOrigin.load()) g_rvaOrigin.store(PhdrRvaOrigin(TPL_LIB_NAME));
                LOGI(TPL_LOG " lib ready (via Tick fallback, real module)");
            }
        }
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
