/* ================================================================
 *  Includes/HotLog.h  —  热路径日志闸（2026-10-11 新增，P1-⑧ 收口）
 *
 *  背景（真机实锤）：Dobby inline hook 的回调会在战斗/面板里**逐帧**被调用，
 *  而 LOGI 写 logcat 是 syscall（单条数百微秒，缓冲区满时还能阻塞调用线程）
 *  ⇒ 逐帧打日志等于给游戏掉帧。凡是在 Hooked_* 里打日志，一律走这两个闸。
 *
 *  用法：
 *    static std::atomic<uint32_t> s_n{0};
 *    if (HotLog(s_n.fetch_add(1) + 1)) LOGI("[Tag] ... %u", s_n.load());
 *    // 或按时间：if (HotLogEvery(1000)) LOGI("[Tag] 每秒一条");
 *
 *  语义：HotLog(n) = 前 3 条全打，其后每 200 条打 1 条（保留异常早期样本，
 *  长跑时只留稀疏心跳）。限流计数与时间戳都是本 TU 内的 static，跨游戏文件
 *  互不干扰（每个 Games/*.cpp 是独立 TU）。
 *
 *  配套：04_scripts/_common/lint_mod_thread.sh 的 R2-hotlog 规则会拦
 *  「Hooked_* 里直接 LOG*」的写法；豁免写法 `// lint:ok R2-hotlog` 需写明理由。
 * ================================================================ */
#pragma once

#include <atomic>
#include <cstdint>
#include <ctime>

static inline bool HotLog(uint32_t n) { return n <= 3 || (n % 200) == 0; }

// 按毫秒节流：同一调用点每 ms 最多打一条。首次调用必打。
static inline bool HotLogEvery(uint32_t ms) {
    static std::atomic<int64_t> s_last{0};
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    const int64_t now = static_cast<int64_t>(ts.tv_sec) * 1000 + ts.tv_nsec / 1000000;
    const int64_t prev = s_last.load();
    if (prev != 0 && now - prev < static_cast<int64_t>(ms)) return false;
    s_last.store(now);
    return true;
}
