#ifndef STARRY_YSXZ_SEC_SHELL_H
#define STARRY_YSXZ_SEC_SHELL_H

// 勇士之心 1.0.4：SecShell（libsecret.so）运行态净化看门狗。
// 启动一次（幂等），后台线程持续把壳的 dirty / failedMask 清零，使壳恢复
// 「自己调用真实 Application.onCreate」的正常 unshell 流程（详见 .cpp 头注释）。
void YsxzSecShell_Start();

#endif  // STARRY_YSXZ_SEC_SHELL_H
