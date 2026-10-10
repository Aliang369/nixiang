/* ================================================================
 *  Includes/ModThread.h  —  mod 线程 attach 到 il2cpp 的收口实现（P1-⑧）
 *
 *  背景（真机实锤 + 两个先例）：
 *   · mod 侧线程多是 Main.cpp 里裸 pthread_create 起的，**不在** il2cpp 线程表里；
 *     在这种线程上直接 runtime_invoke / 调 il2cpp 托管 getter 会异常或 abort。
 *   · 但 `il2cpp_thread_attach` 在 `il2cpp_init` **完成之前**调用也会 abort
 *     （WorMod 真机实锤）。
 *   · 安全判据：`il2cpp_domain_get()` 非空 ⇔ il2cpp_init 已完成。domain 为空就
 *     本拍跳过、下拍再试 —— 绝不硬调。
 *  先例：Games/EndfieldMod.cpp:385、Games/HzsjMod.cpp:369、Games/WorMod.cpp:2274。
 *  本头文件把这四处复制收成一处；新游戏直接调用，别再抄一遍。
 *
 *  用法（每个 Games/*.cpp 是独立 TU，attach 状态也就每文件一份）：
 *    if (!StarryEnsureIl2cppThreadAttached(GetApi().domain_get, GetApi().thread_attach,
 *                                          "Whrhx", IdentityMiss)) return nullptr;
 *    void* obj = GetApi().runtime_invoke(mi, nullptr, nullptr, &exc);
 *
 *  配套：04_scripts/_common/lint_mod_thread.sh 的 R3-attach 规则会拦
 *  「函数里 runtime_invoke 但同函数没有任何 attach/domain_get」（豁免 `// lint:ok R3-attach`）。
 * ================================================================ */
#pragma once

#include <android/log.h>

#include <atomic>

// tag：日志标签（保留各游戏原样，如 "Whrhx" ⇒ `[Whrhx] tick thread attached to il2cpp domain`）。
// onDefer：可选回调，用于把「本拍没 attach 成功」的原因写进各自的限流日志。
// 返回 true 表示「当前线程可安全 invoke」。
static inline bool StarryEnsureIl2cppThreadAttached(void* (*domain_get)(),
                                                    void* (*thread_attach)(void*),
                                                    const char* tag,
                                                    void (*onDefer)(const char* why) = nullptr) {
    static std::atomic<bool> s_attached{false};
    if (s_attached.load()) return true;
    if (!domain_get || !thread_attach) {
        if (onDefer) onDefer("attach unavailable: api missing");
        return false;
    }
    void* d = domain_get();
    if (!d) {
        if (onDefer) onDefer("attach deferred: il2cpp domain null");
        return false;
    }
    if (!thread_attach(d)) {
        if (onDefer) onDefer("thread_attach failed");
        return false;
    }
    s_attached.store(true);
    __android_log_print(ANDROID_LOG_INFO, "StarryModThread",
                        "[%s] tick thread attached to il2cpp domain", tag ? tag : "mod");
    return true;
}
