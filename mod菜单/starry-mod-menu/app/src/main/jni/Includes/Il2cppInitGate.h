/* ================================================================
 *  Includes/Il2cppInitGate.h — 「首次触碰 il2cpp 必须有真就绪信号」的收口实现（FW-20）
 *
 *  背景（两例同型实锤，2026-10-16）：
 *   · Games/ZhmrMod.cpp 3.6.0：mod 线程在 `il2cpp_init` 完成前触碰 il2cpp API ⇒ 该线程进
 *     Boehm GC「未知线程」毒态 ⇒ UnityMain 一旦触发 GC 收集即 `abort()`。
 *   · Games/EndfieldMod.cpp v14.5→v14.6（终末地 1.6.4 / 小米 96f0d883）实测：
 *     我们 `21:03:06.681` 调 il2cpp API，Unity `21:03:07.162` 才起 `MemoryManager`
 *     ⇒ `21:03:07.722` 游戏在 UnityMain `abort()`（27 帧 `libc abort` ← `libil2cpp`×7 ← `libunity`×4）。
 *     指纹：**首启常幸存、重开必崩**（首启 Tick 首探落在 init 之后）。本机 `ApplicationExitInfo`
 *     佐证：12:44 首启存活 5 分钟（RSS 1.5GB）→ 12:50 起每次开必崩（status 11→6）。
 *   · 反例教训：终末地 v14.5 曾用 `Il2Cpp::SafeProbe`(siglongjmp) 把 SIGSEGV 接住 —— 但
 *     **接住 ≠ 没碰**，毒态已种下，0.5s 后换成游戏自己 abort。⇒「捕获异常 + 退避重试」
 *     **不能**替代就绪门（FW-19/FW-20 契约）。
 *
 *  用法（每个 Games/*.cpp 是独立 TU，门状态也就每文件一份）：
 *    static StarryIl2cppInitGate g_gate;
 *    // ① 目标库刚命中、**还没碰任何 il2cpp API** 时先挂门：
 *    g_gate.Arm(sec_lib_il2cpp(), "Endfield");
 *    // ② 之后每拍（框架 tick / tryInit）先问门：
 *    if (!g_gate.Open()) return;      // 门未开：本拍一次 il2cpp 调用都不做
 *    // ③ 门开后再 EnsureApi() / 解析 / 装钩；SafeProbe 仍保留为最后一层兜底。
 *
 *  语义要点（照抄已验证实现，勿改）：
 *   · `Arm` 只做「解析符号 + 打补丁」，**不调用任何 il2cpp API** ⇒ 可在 init 前安全执行。
 *   · 解析顺序：`dlopen(..., RTLD_LAZY|RTLD_NOLOAD)` + `dlsym("il2cpp_init")` → 失败退
 *     `Il2Cpp::ResolveExport`（dynsym 自解析 + 诱饵过滤），再过 `Il2Cpp::IsExecutableRange(fn,16)`
 *     校验；不在可执行段就不挂（宁可退化盲等，不挂错地址）。
 *   · 回调**先调原函数、后置位**：`il2cpp_init` 真正返回且 `r == 1` 才算就绪。
 *   · 时间兜底 30s：挂钩失败或错过调用点也不死锁（实测 init 只需 ~2s，30s 是数十倍余量）。
 *   · 状态全 `std::atomic`；`Arm` 只允许一次（`exchange` 守卫，可重复调用）。
 *
 *  ⚠️ 当前状态（2026-10-16）：**已就绪、暂未接线**。终末地实际用的是
 *     `Games/EndfieldMod.cpp` 内同一份私有实现 —— 该包已过用户 L3 验收，为免「改源码 ⇒
 *     交付物与实测脱钩」（FW-17）故意不重构。下一个需要就绪门的游戏直接改用本头（那时
 *     可把 `EndfieldMod.cpp` 的私有实现删掉，并重走 L2）。
 * ================================================================ */
#pragma once

#include <atomic>
#include <cstdint>
#include <dlfcn.h>
#include <time.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Unity/Il2CppApi.h"

class StarryIl2cppInitGate {
public:
    /** 只在首次真正挂门（幂等）。libName 建议传 `sec_lib_il2cpp()`。 */
    void Arm(const char* libName, const char* tag) {
        if (armed_.exchange(true)) return;
        s_self = this;                 // ★回调要靠它找到实例（每 TU 一份门状态）
        tag_ = tag;
        void* fn = nullptr;
        const char* how = "?";
        void* h = libName ? dlopen(libName, RTLD_LAZY | RTLD_NOLOAD) : nullptr;
        if (h) {
            fn = dlsym(h, "il2cpp_init");
            if (fn) how = "dlsym";
        }
        if (!fn) {
            fn = Il2Cpp::ResolveExport("il2cpp_init");
            if (fn) how = "dynsym";
        }
        if (!fn || !Il2Cpp::IsExecutableRange((uintptr_t)fn, 16)) {
            LOGW("[%s] il2cpp_init 解析失败(fn=%p) ⇒ 退化为时间兜底盲等", tag_ ? tag_ : "mod", fn);
            hookFailed_.store(true);
            return;
        }
        if (DobbyHook(fn, (void*)&StarryIl2cppInitGate::Hooked, (void**)&orig_) == 0) {
            LOGI("[%s] il2cpp_init 就绪门已挂 @%p (%s)", tag_ ? tag_ : "mod", fn, how);
        } else {
            LOGW("[%s] il2cpp_init DobbyHook 失败 ⇒ 退化为时间兜底盲等", tag_ ? tag_ : "mod");
            hookFailed_.store(true);
        }
    }

    /** 门是否已开；未开时调用方**必须直接 return**（不要触碰任何 il2cpp API）。 */
    bool Open() {
        if (open_.load()) return true;
        static const int64_t kCapMs = 30000;
        static const int64_t s_startMs = NowMs();
        const int64_t waited = NowMs() - s_startMs;
        if (waited < kCapMs) {
            static int s_logDiv = 0;
            if ((s_logDiv++ % 20) == 0)
                LOGI("[%s] 等 il2cpp_init 完成（已等 %lldms / 上限 %lldms，hookFailed=%d）—— 本拍不触碰 il2cpp",
                     tag_ ? tag_ : "mod", (long long)waited, (long long)kCapMs, hookFailed_.load() ? 1 : 0);
            return false;
        }
        LOGW("[%s] 就绪门时间兜底：等 %lldms 未见 il2cpp_init 返回（hookFailed=%d）⇒ 放行",
             tag_ ? tag_ : "mod", (long long)waited, hookFailed_.load() ? 1 : 0);
        open_.store(true);
        return true;
    }

private:
    static int64_t NowMs() {
        struct timespec ts;
        clock_gettime(CLOCK_MONOTONIC, &ts);
        return (int64_t)ts.tv_sec * 1000 + ts.tv_nsec / 1000000;
    }
    typedef int (*Fn)(const char*);
    static int Hooked(const char* domainName) {
        int r = s_self ? (s_self->orig_ ? s_self->orig_(domainName) : 0) : 0;
        if (s_self) {
            LOGI("[%s] il2cpp_init 返回 %d ⇒ 就绪门打开（此后才允许触碰 il2cpp）",
                 s_self->tag_ ? s_self->tag_ : "mod", r);
            if (r == 1) s_self->open_.store(true);
        }
        return r;
    }

    std::atomic<bool> armed_{false};
    std::atomic<bool> open_{false};
    std::atomic<bool> hookFailed_{false};
    Fn orig_ = nullptr;
    const char* tag_ = nullptr;
    inline static StarryIl2cppInitGate* s_self = nullptr;   // 单实例（每 TU 一份）
};
