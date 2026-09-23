/* ================================================================
 *  Games/NoAceStubs.cpp — 非 ACE 游戏的 MainModern 入口兜底
 *
 *  背景（2026-09-22 起）：共享入口 MainModern.cpp 的 JNI_OnLoad 无条件引用
 *    SignalQuarantine_Start()            （Games/SignalQuarantine.cpp）
 *    FgoBoot_InjectEarlyFromOnLoad(env)  （Games/FgoBoot.cpp）
 *  于是所有未把这两个文件加入 LOCAL_SRC_FILES 的 Android_<tag>.mk 都会
 *  在链接期报 ld.lld: undefined symbol。
 *
 *  两条正确修法（按游戏形态二选一）：
 *    A) 有 ACE/libtprt.so 或 payload 形态（包内无菜单 dex）的游戏：
 *       在 mk 里加入 Games/SignalQuarantine.cpp Games/FgoBoot.cpp
 *       （参考 Android_jjdm.mk / Android_mygwzd.mk）。
 *    B) 无 ACE 且菜单 dex 已在 APK 内（Manifest 注册 StarryInitProvider，
 *       外科 zip 组装）的游戏：加入本文件即可。
 *       本文件把两者定义成 no-op —— 既不装 seccomp / 不屏蔽致命信号，
 *       也不做早期 dex 注入（避免重复注入 dex 或读取不存在的 assets/starry.dex）。
 *
 *  弹壳战机 1.1.6（com.survivor.acecn，TapTap 单机）属于 B 类：
 *  包内只有 libsecsdk/libsgcore/libthemis/libBugly_Native/libtapsdkcore，
 *  无 libtprt.so；菜单类在 classes5.dex 且已由 Manifest 的 StarryInitProvider 拉起。
 * ================================================================ */
#include <jni.h>

// 无 ACE 看门狗：不安装 seccomp 过滤器，也不改写致命信号处理器。
extern "C" void SignalQuarantine_Start(void) {}

// 菜单 dex 已在 APK 内：不做早期注入。
extern "C" void FgoBoot_InjectEarlyFromOnLoad(JNIEnv* /*env*/) {}
