//
//  StarPtr.h  —— 设备无关的「像样指针 / 可解引用对象」判据（框架级唯一实现）
//
//  为什么要有这个文件（事故记录）：
//    · 多个游戏在各自 Mod.cpp 里手抄过一份热路径对象门闩，写法是
//          uintptr_t u = (uintptr_t)p;
//          if (u < 0x100000000ULL || u > 0x800000000000ULL) return false;
//      这个判定**没有清掉顶字节**。
//    · 真机（arm64 + Android 12 及以上）的堆由 Scudo 分配在 0xb4 开头的地址上，
//      Cortex 的 TBI 让硬件访问时忽略顶字节，指针形如 0xb4000076c1646d58。
//      这类指针按上面的死区间判 → 永远为 false → 功能在真机上**静默全灭**；
//      而 x86_64 + Houdini 的模拟器（MuMu）地址不会跑到 0xb4… 之上，所以模拟器一切正常。
//      ⇒ 现象就是「同一份包：模拟器有效、真机没效果」。
//    · 仓库内实证：境界守望者 63 个程序集全被判空；无悔华夏 `mi=0xb4000076c1646d58`；
//      境界刀鸣 lua state `0xb4000076…`。
//
//  使用规约（写进 10_docs 的《设备无关注入规约》）：
//    1. 任何「这个指针像不像堆指针」的判断，**只允许**用本文件的函数，
//       不允许在游戏 Mod.cpp 里再手抄区间判定。
//    2. 热路径用 LiveFast()（只做区间判定，不做 maps 查询，零 I/O）。
//    3. 需要「确认可读」时再叠加 Il2Cpp::IsValidHeapPtr() / IsReadableRange()（走 maps 快照）。
//    4. 判否失败必须可观测：StarPtr::Verdict() 会在首次满足条件时打一行统计，
//       让你能从 logcat 判断「是指针判据把设备判死了」还是「逻辑没跑到」。
//
#ifndef STARRY_STARPTR_H
#define STARRY_STARPTR_H

#include <cstdint>
#include <cstddef>
#include <atomic>
#include <android/log.h>

#ifndef STARPTR_TAG
#define STARPTR_TAG "StarPtr"
#endif

#ifndef STARPTR_LOGI
#define STARPTR_LOGI(...) ((void)__android_log_print(ANDROID_LOG_INFO, STARPTR_TAG, __VA_ARGS__))
#endif
#ifndef STARPTR_LOGW
#define STARPTR_LOGW(...) ((void)__android_log_print(ANDROID_LOG_WARN, STARPTR_TAG, __VA_ARGS__))
#endif

namespace starptr {

// ---- 顶字节（MTE/TBI/PAC 高位）掩码：ARM64 用户态合法地址 < 2^56 ----
static constexpr uintptr_t kTopByteMask = 0x00FFFFFFFFFFFFFFull;
// ---- ARM64 用户态地址区间（清标签之后） ----
static constexpr uintptr_t kUserVaMin = 0x100000000ull;      // 4GiB
static constexpr uintptr_t kUserVaMax = 0x800000000000ull;   // 128TiB
// ---- 模拟器侧证据用：0xb4… 堆标签的高字节 ----
static constexpr uint8_t kTagHintTop = 0xb4;

static inline uintptr_t Untag(uintptr_t u) { return u & kTopByteMask; }
static inline uintptr_t Untag(const void* p) { return Untag(reinterpret_cast<uintptr_t>(p)); }

static inline bool InUserVa(uintptr_t u) {
    const uintptr_t v = Untag(u);
    return v >= kUserVaMin && v < kUserVaMax;
}
static inline bool InUserVa(const void* p) { return p && InUserVa(reinterpret_cast<uintptr_t>(p)); }

// 热路径判据：像不像用户态指针。不做任何 maps 查询、不阻塞、不分配。
// 注意：它只判区间，不保证可读；解引用前若需要保证，请叠加 Il2Cpp::IsReadableRange()。
static inline bool LiveFast(const void* p) { return InUserVa(p); }
static inline bool LiveFast(uintptr_t u) { return InUserVa(u); }

// 对齐判定（挡掉打包 int / 脏值；ARM64 堆对象至少 8 字节对齐）
static inline bool IsAligned(const void* p, uintptr_t align = 8) {
    return p != nullptr && (reinterpret_cast<uintptr_t>(p) & (align - 1)) == 0;
}
static inline bool IsAligned(uintptr_t u, uintptr_t align = 8) {
    return u != 0 && (u & (align - 1)) == 0;
}

// 热路径对象门闩 = 对齐 + 区间（等价于各游戏原来手抄的 SoftObjPtr/SafeObjPtr，
// 但已修掉「真机 0xb4… 指针被判否」这个致命缺陷）。
static inline bool LiveObj(const void* p) { return IsAligned(p) && LiveFast(p); }

// ---- 诊断统计（默认开启，开销 = 每判一次一条 relaxed 加；可在编译期 -DSTARPTR_STATS=0 关掉） ----
#ifndef STARPTR_STATS
#define STARPTR_STATS 1
#endif

#if STARPTR_STATS
struct Stats {
    std::atomic<uint64_t> checks{0};
    std::atomic<uint64_t> ok{0};
    std::atomic<uint64_t> rejected{0};
    std::atomic<uint64_t> taggedSeen{0};   // 顶字节非 0（真机堆标签）
    std::atomic<uint64_t> untagSaved{0};   // 不清标签就会误判为否、清了才活下来的次数
};
inline Stats& stats() { static Stats s; return s; }

inline void note(bool passed, uintptr_t raw) {
    Stats& s = stats();
    s.checks.fetch_add(1, std::memory_order_relaxed);
    if (passed) s.ok.fetch_add(1, std::memory_order_relaxed);
    else s.rejected.fetch_add(1, std::memory_order_relaxed);
    if (raw & ~kTopByteMask) {
        s.taggedSeen.fetch_add(1, std::memory_order_relaxed);
        // 死区间判据会否掉它，而我们放过了 ⇒ 证明「清标签」是这台设备上活下来的原因
        const uintptr_t masked = Untag(raw);
        if (masked >= kUserVaMin && masked < kUserVaMax) {
            s.untagSaved.fetch_add(1, std::memory_order_relaxed);
        }
    }
}
inline bool LiveFastCounted(const void* p) {
    const uintptr_t raw = reinterpret_cast<uintptr_t>(p);
    const bool r = InUserVa(raw);
    note(r, raw);
    return r;
}

// 一行 verdict：让「设备被判据判死」这件事在 logcat 里看得见，而不是静默无效果。
// 由调用方在启动/第一次用到钩子时调一次即可（内部有 once 保护）。
inline void Verdict(const char* who) {
    static std::atomic<bool> done{false};
    bool expected = false;
    if (!done.compare_exchange_strong(expected, true)) return;
    Stats& s = stats();
    const uint64_t c = s.checks.load(std::memory_order_relaxed);
    const uint64_t t = s.taggedSeen.load(std::memory_order_relaxed);
    const uint64_t u = s.untagSaved.load(std::memory_order_relaxed);
    const uint64_t okv = s.ok.load(std::memory_order_relaxed);
    STARPTR_LOGI("[Ptr] %s verdict: checks=%llu ok=%llu rejected=%llu tagged=%llu untag_saved=%llu mode=%s",
                 who ? who : "?",
                 (unsigned long long)c, (unsigned long long)okv,
                 (unsigned long long)s.rejected.load(std::memory_order_relaxed),
                 (unsigned long long)t, (unsigned long long)u,
                 t ? "tagged-heap(真机/MTE 环境)" : "plain-va(模拟器/Houdini 环境)");
    if (c > 0 && okv == 0) {
        STARPTR_LOGW("[Ptr] %s verdict: 全部判否 —— 若功能无效果，先怀疑指针判据/注入面绑定了某个运行环境", who ? who : "?");
    }
}
#else
inline void note(bool, uintptr_t) {}
inline bool LiveFastCounted(const void* p) { return LiveFast(p); }
inline void Verdict(const char*) {}
#endif  // STARPTR_STATS

// ---- 兼容各游戏原来的函数名（避免大改调用点，但实现统一） ----
// 旧名：LivePtr / PlausiblePtr / SoftObjPtr / SafeObjPtr
static inline bool CompatLivePtr(const void* p) { return LiveFastCounted(p); }

}  // namespace starptr

#endif  // STARRY_STARPTR_H
