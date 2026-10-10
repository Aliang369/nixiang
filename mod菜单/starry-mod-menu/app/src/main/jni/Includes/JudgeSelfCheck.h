#pragma once
// ================================================================
//  JudgeSelfCheck.h — 诊断日志总闸 + 一次性判据自检（2026-10-10 复盘 P0-2 / P1-1）
//
//  两道闸，一上一下，缺一不可：
//
//  【上闸 SDIAG】诊断行的编译期总闸
//      STARRY_DIAG=1（默认，诊断包）→ 诊断行编进 so；
//      STARRY_DIAG=0（交付包）      → 诊断行**连字符串都不进包**。
//    写法必须用 `#if` 而不是 `if (STARRY_DIAG)`：只有预处理丢弃才能保证字符串不残留
//    （交付包要能通过「无诊断串」负向断言，见 04_scripts/<游戏>/<游戏>_build_so.sh 的 STARRY_DIAG=0 分支）。
//    用法：SDIAG(LOGI(TAG " diag: …"));   // 分号照写
//
//  【下闸 证据层】以下日志**不受 SDIAG 影响，交付包里永远保留**：
//      `[tag] hooked <方法名> argc=…`、`[tag] hit <功能键>`（首调用各一行）、
//      `method miss` / `nested miss` / `dobby fail` / `… not resolvable yet (will retry)`、
//      `lib ready`、`GiveUp` 之类的失败行。
//    原因：04_scripts/<游戏>/实测判读_*.sh 就靠它们自证「装上了 / 被游戏调用了」。
//    诊断行一旦混进证据层，交付包就没法自证，L3 判读会退化成「只能看画面」。
//    ⇒ 新增日志时先问：这是「证据」还是「诊断」？证据无条件打，诊断套 SDIAG。
//
//  【SDIAG_ONCE】一次性自检行（照 Games/GgbhMod.cpp 的 EmitSelfCheckOnce 先例）：
//    每个进程最多打一行，用来在装机后第一时间暴露判据健康度（API ready / 类解析 / 判据标定 /
//    钩子装上数），避免"看起来装上了、其实判据全假"这类静默失效。
//    例：SDIAG_ONCE(mrfzz_selfcheck,
//          LOGI(MRFZZ_LOG " SELFCHECK api=%d asm=%zu cls=%d rd=%d hk=%d/7", …));
// ================================================================

#include <atomic>

#ifndef STARRY_DIAG
#define STARRY_DIAG 1
#endif

#if STARRY_DIAG
#define SDIAG(...) do { __VA_ARGS__; } while (0)
#else
#define SDIAG(...) do { } while (0)
#endif

// 只在诊断包里出现的「一次性」块：`SDIAG_ONCE(名字, 语句…)`
#define SDIAG_ONCE(name, ...)                          \
    do {                                               \
        static std::atomic<bool> name##_done{false};   \
        if (!name##_done.exchange(true)) {             \
            SDIAG(__VA_ARGS__);                        \
        }                                              \
    } while (0)
