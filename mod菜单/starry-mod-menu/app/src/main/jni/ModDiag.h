#pragma once
// ============================================================
//  ModDiag.h — 诊断脚手架编译期隔离（2026-09-30 伊莫会话复盘 P0-1 固化）
//
//  背景：功能调试期的诊断（日志/计数/探针）如果直接进产物包，收尾时就得
//  「手术式拆除」——伊莫实测代价：Games/YimoMod.cpp 115 行 / 35 处手术
//  + 一整轮全量重建重装（389 MB），且手术本身有回归风险。
//
//  铁律：
//    1) 诊断代码必须能被**一个开关整体关掉**，且默认关；
//    2) 正式包构建（不带 STARRY_DIAG）＝零诊断代码（不是"运行时短路"，
//       是编译期就不进 so：DLOG 宏展开成 ((void)0)）；
//    3) 诊断包显式 STARRY_DIAG=1，产物名带 -diag 后缀，两者可并存；
//    4) 诊断上限**不许写死**：走运行时配置文件（见 ModDiagLua.h），
//       改一行文本重启游戏即可，零重建零重装。
//
//  用法（native 侧）：
//    #include "ModDiag.h"
//    DLOG("patch installed state=%p", (void*)L);
//    DLOGN("tp", "plan=%.1f max=%.1f", plan, maxd);   // 带 tag，便于 logcat 过滤
//
//  用法（Lua 侧）：见 ModDiagLua.h（整段诊断 chunk + 运行时上限配置）。
//  构建：ndk-build ... STARRY_DIAG=1（Android_<tag>.mk 里 ifdef STARRY_DIAG）
// ============================================================

#include <android/log.h>

#ifdef STARRY_DIAG

#ifndef MOD_DIAG_TAG
#define MOD_DIAG_TAG "StarryDiag"
#endif

#define DLOG(fmt, ...) \
  __android_log_print(ANDROID_LOG_INFO, MOD_DIAG_TAG, fmt, ##__VA_ARGS__)
#define DLOGN(tag, fmt, ...) \
  __android_log_print(ANDROID_LOG_INFO, MOD_DIAG_TAG "/" tag, fmt, ##__VA_ARGS__)
#define DWARN(fmt, ...) \
  __android_log_print(ANDROID_LOG_WARN, MOD_DIAG_TAG, fmt, ##__VA_ARGS__)

// Lua 侧诊断 chunk 是否随 patch 一起注入（供 patch 组装处 %s 使用）
#define MOD_DIAG_LUA 1

#else

#define DLOG(fmt, ...) ((void)0)
#define DLOGN(tag, fmt, ...) ((void)0)
#define DWARN(fmt, ...) ((void)0)

// 0 = 诊断 chunk 不注入（正式包：Lua 侧零诊断代码）
#define MOD_DIAG_LUA 0

#endif

// ------------------------------------------------------------
// 证据日志 ≠ 诊断脚手架
// 「补丁到底装上没有 / 符号没找到」这类是**判据**，正式包也必须保留：
//   LOGI(MOD_EVIDENCE "[%s] patch installed (state=%p)", GAME_TAG, (void*)L);
// 只把「过程性观测」（每条 hook 命中、每次重发次数、每帧数值…）放进 DLOG。
// ------------------------------------------------------------
#define MOD_EVIDENCE "Mod_Menu"

// 诊断产物名后缀（构建脚本用同一约定，避免两处不一致）
#define MOD_DIAG_APK_SUFFIX "-diag"

// 干净包门禁扫描时允许出现的诊断标记（供 check_release_clean.sh 交叉核对）
#define MOD_DIAG_MARKERS "DLOG,DLOGN,DWARN,MOD_DIAG_LUA,STARRY_DIAG"
