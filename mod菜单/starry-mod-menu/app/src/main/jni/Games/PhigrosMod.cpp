/* ================================================================
 *  Phigros 3.20.0 — 「自动跳」v1（探测版）
 *
 *  hook libil2cpp.so 计分函数（file=0x1AECA7C / RVA 0x1AF0A7C）：
 *  开关 ON 先调斌哥增强函数（file=0x1AE8318 / RVA 0x1AEC318，
 *  原型 void f(void* this, int32_t flag)，实际 x0/x1 透传）再调原函数；
 *  OFF 直调原函数。带 Trace 日志。
 *
 *  ELF 注意：vaddr = 文件偏移 + 0x4000（LOAD2 off=0x17597bc vaddr=0x175d7bc）；
 *  FileOffToVa 按 /proc/self/maps 段 offset 匹配，直接传文件偏移即可。
 *  真 DobbyHook 延迟到开关开启后由 Tick 安装（同 Ywgn MaybeInstall 模式）。
 * ================================================================ */

#include "PhigrosMod.h"
#include "Includes/SecStrings.hpp"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"

enum : uintptr_t {
    FILE_SCORE_FN   = 0x1AECA7C,  // RVA 0x1AF0A7C 计分函数（每帧驱动，v1 实测）
    FILE_ENHANCE_FN = 0x1AE8318,  // RVA 0x1AEC318 Perfect 计数（自然调用≈特殊事件）
    FILE_HOLD_FN    = 0x1AE80D0,  // RVA 0x1AEC4D0 hold 释放处理
    FILE_NOTE_FN    = 0x1AE2C48,  // RVA 0x1AE6C48 note 更新（遍历循环每 note 调）
    RVA_SCORE_FN    = 0x1AF0A7C,
    RVA_ENHANCE_FN  = 0x1AEC318,
    RVA_HOLD_FN     = 0x1AEC4D0,
    RVA_NOTE_FN     = 0x1AE6C48,
};

// hook 函数 4 参保守签名：斌哥壳 trampoline 只透传 x0/x1，
// hooked 函数把全部参数原样转发给 orig
typedef void (*ScoreFn_t)(void* a0, int32_t a1, void* a2, void* a3);
typedef void (*EnhanceFn_t)(void* a0, int32_t a1);
typedef void (*NoteFn_t)(void* note);

static std::atomic<bool> g_inited{false};
static std::atomic<bool> g_hooked{false};
static std::atomic<bool> g_traceHooked{false};
static std::atomic<bool> g_auto{false};
static std::atomic<uint32_t> g_hits{0};
static std::atomic<uint32_t> g_traceCount{0};
static std::atomic<uint32_t> g_ticks{0};
static std::atomic<uint32_t> g_judgeHits{0};
static std::atomic<uint32_t> g_holdHits{0};
static std::atomic<int32_t> g_lastJudgeW1{-999};

static ScoreFn_t g_origScoreFn = nullptr;
static EnhanceFn_t g_origJudgeFn = nullptr;
static EnhanceFn_t g_origHoldFn = nullptr;
static NoteFn_t g_origNoteFn = nullptr;
static EnhanceFn_t g_enhanceFn = nullptr;
static uintptr_t g_libBase = 0;  // 运行时 so 基址（vaddr0）

/** 本地轻量堆指针校验（同 YwgnMod SoftPtr 思路；不依赖 Il2CppApi.cpp） */
static bool HeapOk(void* p) {
    if (!p) return false;
    uintptr_t u = (uintptr_t)p & 0x00FFFFFFFFFFFFFFULL;
    if (u & 7) return false;
    return u >= 0x1000000000ULL && u <= 0x8000000000ULL;
}

static void* FileOffToVa(const char* lib, uintptr_t fileOff) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return nullptr;
    char line[768];
    void* found = nullptr;
    while (fgets(line, sizeof(line), fp)) {
        if (!strstr(line, lib)) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x')) continue;
        if (end <= start) continue;
        if (fileOff >= off && fileOff < off + (end - start)) {
            found = (void*)(start + (fileOff - off));
            break;
        }
    }
    fclose(fp);
    return found;
}

static void Hooked_ScoreFn(void* a0, int32_t a1, void* a2, void* a3) {
    // v3 定稿（用户命名「满分」）：与斌哥原版行为一致——
    // 开关 ON 时调用 Perfect 事件处理函数（驱动分数/连击），再走原函数
    if (g_auto.load() && g_enhanceFn) g_enhanceFn(a0, a1);
    if (g_origScoreFn) g_origScoreFn(a0, a1, a2, a3);

    uint32_t n = g_hits.fetch_add(1) + 1;
    if (n <= 10 || (n % 100) == 0) {
        g_traceCount.fetch_add(1);
        if (HeapOk(a0)) {
            LOGI("[Phigros] score fn a0=%p a1=%d this+0x40=%d +0x4C=%d +0x54=%d +0x58=%d",
                 a0, a1,
                 *(int32_t*)((uintptr_t)a0 + 0x40),
                 *(int32_t*)((uintptr_t)a0 + 0x4C),
                 *(int32_t*)((uintptr_t)a0 + 0x54),
                 *(int32_t*)((uintptr_t)a0 + 0x58));
        } else {
            LOGI("[Phigros] score fn a0=%p a1=%d (a0 未过堆校验，跳过字段读取)", a0, a1);
        }
    }
}

/** note 更新 Trace：每 note 每帧调用，dump 状态字段找判定写入点 */
static std::atomic<uint32_t> g_noteHits{0};
static void Hooked_NoteFn(void* note) {
    if (g_origNoteFn) g_origNoteFn(note);
    uint32_t n = g_noteHits.fetch_add(1) + 1;
    if ((n % 60) == 1 && HeapOk(note)) {
        // note 对象关键字段：0x50/0x51/0x59 字节标志，0xF0 int，0x108-0x128 状态区
        uint8_t* b = (uint8_t*)note;
        LOGI("[Phigros] note #%u p=%p b50=%u b51=%u b59=%u f0x0F0=%d i108=%d i110=%d i118=%d i120=%p",
             n, note, b[0x50], b[0x51], b[0x59],
             *(int32_t*)(b + 0xF0),
             *(int32_t*)(b + 0x108), *(int32_t*)(b + 0x110),
             *(int32_t*)(b + 0x118), *(void**)(b + 0x120));
    }
}

static bool TryHookAt(uintptr_t fileOff, void* hookFn, void** origOut, const char* tag) {
    void* addr = FileOffToVa(sec_lib_il2cpp(), fileOff);
    if (!addr) return false;
    void* orig = nullptr;
    if (DobbyHook(addr, hookFn, &orig) != 0) return false;
    if (origOut) *origOut = orig;
    LOGI("[Phigros] hooked %s @ %p file=0x%lX", tag, addr, (unsigned long)fileOff);
    return true;
}

/** 判定计数函数 Trace：log (w1, 调用者 RVA)。v2 探测核心。 */
static void Hooked_JudgeFn(void* a0, int32_t a1) {
    // 注意：v1 增强路径（g_auto ON 时 Hooked_ScoreFn 每帧调 g_enhanceFn）也会走到这里，
    // LR = score fn 的事件属自我触发，仅计数不打日志
    void* lr = __builtin_return_address(0);
    uintptr_t callerRva = g_libBase ? ((uintptr_t)lr - g_libBase) : 0;
    uint32_t n = g_judgeHits.fetch_add(1) + 1;
    int32_t last = g_lastJudgeW1.exchange(a1);
    bool selfCall = (callerRva == RVA_SCORE_FN);
    if (!selfCall && g_libBase && (a1 != last || (n % 200) == 1)) {
        LOGI("[Phigros] judge fn this=%p w1=%d caller_rva=0x%lX #%u", a0, a1,
             (unsigned long)callerRva, n);
    }
    if (g_origJudgeFn) g_origJudgeFn(a0, a1);
}

static void Hooked_HoldFn(void* a0, int32_t a1) {
    void* lr = __builtin_return_address(0);
    uintptr_t callerRva = g_libBase ? ((uintptr_t)lr - g_libBase) : 0;
    uint32_t n = g_holdHits.fetch_add(1) + 1;
    if (g_libBase && ((n % 100) == 1)) {
        LOGI("[Phigros] hold fn this=%p w1=%d caller_rva=0x%lX #%u", a0, a1,
             (unsigned long)callerRva, n);
    }
    if (g_origHoldFn) g_origHoldFn(a0, a1);
}

static void MaybeInstall() {
    if (!g_auto.load() || g_hooked.load() || g_ticks.load() < 25) return;
    if (TryHookAt(FILE_SCORE_FN, (void*)Hooked_ScoreFn, (void**)&g_origScoreFn, "score fn"))
        g_hooked.store(true);
}

/** 判定链 Trace hooks：TryInit 后即装（与功能开关解耦，采集为主） */
static void MaybeInstallTrace() {
    if (g_traceHooked.load()) return;
    if (!g_enhanceFn) return;
    g_libBase = (uintptr_t)g_enhanceFn - RVA_ENHANCE_FN;
    bool ok = TryHookAt(FILE_ENHANCE_FN, (void*)Hooked_JudgeFn, (void**)&g_origJudgeFn, "judge fn");
    ok = TryHookAt(FILE_HOLD_FN, (void*)Hooked_HoldFn, (void**)&g_origHoldFn, "hold fn") && ok;
    ok = TryHookAt(FILE_NOTE_FN, (void*)Hooked_NoteFn, (void**)&g_origNoteFn, "note fn") && ok;
    if (ok) g_traceHooked.store(true);
}

bool Phigros_IsReady() { return g_hooked.load(); }

bool Phigros_TryInit() {
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;
    if (!g_inited.exchange(true)) {
        g_enhanceFn = (EnhanceFn_t)FileOffToVa(sec_lib_il2cpp(), FILE_ENHANCE_FN);
        LOGI("[Phigros] init enhance fn @ %p file=0x1AE8318 score file=0x1AECA7C",
             (void*)g_enhanceFn);
    }
    return true;
}

void Phigros_SecurityTick() {}

void Phigros_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum != PHIGROS_FEAT_AUTOPLAY) return;
    g_auto.store(enabled);
    LOGI("[Phigros] 自动跳=%s hits=%u hooked=%d", enabled ? "ON" : "OFF", g_hits.load(),
         g_hooked.load() ? 1 : 0);
}

void Phigros_ForceClear() {
    g_auto.store(false);
}

void Phigros_Tick(const bool* features) {
    if (!features) return;
    g_auto.store(features[PHIGROS_FEAT_AUTOPLAY]);
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return;
    MaybeInstallTrace();
    if (!g_auto.load()) {
        g_ticks.store(0);
        return;
    }
    uint32_t st = g_ticks.fetch_add(1) + 1;
    if ((st % 5) == 0) MaybeInstall();
    if ((st % 60) == 0)
        LOGI("[Phigros] hb hits=%u trace=%u hooked=%d judge=%u hold=%u", g_hits.load(),
             g_traceCount.load(), g_hooked.load() ? 1 : 0, g_judgeHits.load(),
             g_holdHits.load());
}
