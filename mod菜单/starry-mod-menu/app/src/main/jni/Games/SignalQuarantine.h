// Games/SignalQuarantine.h
// 信号隔离（AB-30 实验结论的产品化实现）——见 SignalQuarantine.cpp 顶部的背景说明。
#pragma once

#include <android/log.h>

// 日志 tag 与工程其它模块一致，便于 logcat 过滤
#ifndef SQ_TAG
#define SQ_TAG "Mod_Menu"
#endif

#ifdef __cplusplus
extern "C" {
#endif

// 在 JNI_OnLoad 的**最早期**调用：屏蔽 7 个致命信号（SIGBUS/SIGSEGV/SIGABRT/
// SIGILL/SIGFPE/SIGSYS/SIGTRAP），并起清扫线程把屏蔽集扩散到进程内所有线程。
// 幂等：重复调用直接返回。
void SignalQuarantine_Start(void);

#ifdef __cplusplus
}
#endif
