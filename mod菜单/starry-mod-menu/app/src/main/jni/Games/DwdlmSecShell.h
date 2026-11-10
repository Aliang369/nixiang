#ifndef STARRY_DWDLM_SEC_SHELL_H
#define STARRY_DWDLM_SEC_SHELL_H

// 动物大联盟 1.0.25（TapTap 原版 / APKSecure 壳）：SecShell(libsecret.so)
// 运行态最小状态归一化看门狗 —— **独立模块**。
//
// 与同族的 Games/YsxzSecShell.* 只共享思路（函数序言反推状态字 + 双映像分别解析），
// 不共享符号、不复制其版本表、不参与其链接：本模块不 include YsxzSecShell.h，
// Ysxz 侧也不 include 本文件。
//
// 幂等：重复调用只启动一个后台线程，**主线程不等待、不做任何解析**（本函数内无文件 IO、
// 无 syscall、无 maps 读），因此永远不会阻塞启动路径。只做一件事：把壳的状态字
// dirty / failedMask 归零，让壳按"自检通过"走它自己的 unshell 流程。不写 ready / checked /
// created / descriptor，不改 .text / .plt / .got，不调用壳的任何导出函数。
//
// 写入通道（2026-11-05 修复 D1，第三轮补强）：**只经** syscall(__NR_process_vm_writev,
// getpid(), ...) 写这 2 个 4 字节状态字，按返回值判定成败（必须 == 4）；不直接解引用目标
// 地址、不装 SIGSEGV handler、不 mprotect、不写 /proc/self/mem。系统调用不可用时记明确日志
// 并 fail-closed 转 ABORT，不降级为直接写。
// 通道可用性由**真正 fork 出来的子进程**一次性见证（子进程只用自身栈缓冲跑 write+read 往返
// 后 _exit；父进程 waitpid 有界等待 1s，只有退出码 0 才动手）。原因：seccomp 的 TRAP 策略以
// SIGSYS 终止**整个进程**，同进程探针在那种策略下会先打死游戏，根本走不到返回值分支。
void DwdlmSecShell_Start();

// 状态机（任意线程可读，无锁）：
//   0 IDLE      未启动
//   1 RESOLVING 正在等 libsecret 映像出现并解析状态字（首次窗口 kResolveTimeoutMs），
//               或全部 target 都失效后处于有界重解析窗口（kReResolveWindowMs）
//   2 ACTIVE    至少有一个可信 target，正在周期性归一化（写失败会淘汰该 target，
//               其余继续；剩 0 个时回到 1）
//   3 ABORT     未解析出可信状态字 / 写入通道不可用 / 隔离探针被拒（信号·SIGSYS·超时·
//               非 0 退出·fork 或 waitpid 失败）/ 全部 target 失效且重解析窗口到期 /
//               写返回非短写类致命错误 —— **不再写入任何内存**且不再重试（终态）
//   4 DONE      看门狗到时限正常结束（kTotalMs=600000），之后不再归一化（终态）
// 只用于日志与集成侧判断，不参与任何放行判定。
int DwdlmSecShell_State();

// 说明：Start 之后的合法取值是 1（已请求，后台解析中）、2 或 3（通道/解析失败）。
// Java 侧据此把「已请求」与「已 ABORT」分开打印，不把失败伪装成已启动。

#endif  // STARRY_DWDLM_SEC_SHELL_H
