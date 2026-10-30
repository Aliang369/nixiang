/* ================================================================
 *  Games/WlwwEarlyBoot.cpp — 为了吾王专用「dlopen 即初始化」垫片
 *
 *  为什么需要它（2026-10-30 实测，见 10_docs/为了吾王/过检结论_20261030.md）：
 *    本作 TP 只拦「AndroidManifest.xml 被改」⇒ 不能走 StarryInitProvider 直装路，
 *    改由 libil2cpp.so 的 DT_INIT（code cave，见 07_tmp/patch_libil2cpp_load.py）
 *    在加载期 dlopen 本 so。
 *    但 **native 侧的 dlopen 不会触发 JNI_OnLoad**（只有 System.loadLibrary/ART 路径才会），
 *    实测 +2 s 时 libstarry_magic.so 已在 maps 里，却零条 Mod_Menu 日志、+11 s 仍被 TP 击杀
 *    —— 因为 SignalQuarantine_Start() 一次都没执行。
 *
 *  本文件把初始化挪到构造函数里：
 *    ① 先 SignalQuarantine_Start()（屏蔽 7 个致命信号 + 扩散到全线程），必须最靠前；
 *    ② 用 JNI_GetCreatedJavaVMs 取进程 JavaVM，手工调 MainModern.cpp 导出的 JNI_OnLoad，
 *       后续流程（注册 bridge native / FgoBoot 抽 assets/starry_dex/starry.dex 注入 /
 *       g_mod->onLoad）与正常 loadLibrary 路径完全一致。
 *
 *  铁律：MainModern.cpp / Main.cpp / GameConfig.h / 公共 Android.mk 一律不动，
 *        所以这里只 declare，不重写。
 * ================================================================ */
#include <jni.h>
#include <dlfcn.h>
#include <pthread.h>
#include <unistd.h>
#include <android/log.h>

#include "SignalQuarantine.h"

// MainModern.cpp 的导出实现（JNIEXPORT jint JNICALL JNI_OnLoad）
extern "C" jint JNI_OnLoad(JavaVM* vm, void* reserved);

namespace {

typedef jint (*FnGetCreatedJavaVMs)(JavaVM**, jsize, jsize*);

// 取 JNI_GetCreatedJavaVMs：本 so 的 namespace 里 RTLD_DEFAULT 看不到 libart
// （2026-10-30 实测：RTLD_DEFAULT 返回空），所以逐个候选 so 显式 dlopen 再 dlsym。
FnGetCreatedJavaVMs FindGetCreatedVMs() {
    FnGetCreatedJavaVMs f =
        (FnGetCreatedJavaVMs)dlsym(RTLD_DEFAULT, "JNI_GetCreatedJavaVMs");
    if (f) return f;
    static const char* kCandidates[] = {
        "libart.so", "libnativehelper.so", "libopenjdkjvm.so", "libdvm.so",
    };
    for (size_t i = 0; i < sizeof(kCandidates) / sizeof(kCandidates[0]); ++i) {
        const char* name = kCandidates[i];
        void* h = dlopen(name, RTLD_NOW | RTLD_NOLOAD);
        if (!h) h = dlopen(name, RTLD_NOW);
        if (!h) continue;
        f = (FnGetCreatedJavaVMs)dlsym(h, "JNI_GetCreatedJavaVMs");
        if (f) {
            __android_log_print(ANDROID_LOG_INFO, "Mod_Menu",
                                "[wlww] 早期引导：从 %s 取到 JNI_GetCreatedJavaVMs", name);
            return f;
        }
    }
    return nullptr;
}

// 引导线程：延迟后自己做 JavaVM 查找 + 手工 JNI_OnLoad。
//
// 为什么必须挪出构造函数（H1，2026-10-30 第二轮实测）：
//   构造函数是在 libil2cpp 的 DT_INIT 里被调用的 —— 此刻**链接器持锁**、调用栈在 dlopen 内部，
//   在这种状态下做 FindClass / 反射注入 dex（MakeDexElements）会把 ART 的类加载路径和链接器锁
//   缠在一起，实测 +1.6 s UnityMain 崩 SIGSEGV（tombstone pc 在 houdini 翻译段）。
//   所以构造函数只做两件最低风险的事：装信号隔离（纯 syscall）+ 起线程；重活留到线程里、
//   等 200 ms 让 DT_INIT 走完、链接器放锁之后再干。
void* BootThread(void* /*arg*/) {
    usleep(200 * 1000);  // 等 libil2cpp 的 DT_INIT 链走完、链接器放锁

    FnGetCreatedJavaVMs getVMs = FindGetCreatedVMs();
    if (!getVMs) {
        __android_log_print(ANDROID_LOG_WARN, "Mod_Menu",
                            "[wlww] 早期引导：找不到 JNI_GetCreatedJavaVMs，初始化跳过");
        return nullptr;
    }
    JavaVM* vm = nullptr;
    jsize n = 0;
    if (getVMs(&vm, 1, &n) != JNI_OK || !vm || n < 1) {
        __android_log_print(ANDROID_LOG_WARN, "Mod_Menu",
                            "[wlww] 早期引导：JNI_GetCreatedJavaVMs 没给出 JavaVM");
        return nullptr;
    }

    // 本线程是新起的 pthread：必须先 Attach，否则 MainModern 的 JNI_OnLoad 里
    // vm->GetEnv 拿不到 env，会走"[桥接] JNI_OnLoad 无 JNIEnv，注册延后"分支，
    // dex 注入与 RegisterNatives 都不会发生。
    JNIEnv* env = nullptr;
    bool attached = false;
    if (vm->GetEnv((void**)&env, JNI_VERSION_1_6) != JNI_OK) {
        if (vm->AttachCurrentThread(&env, nullptr) == JNI_OK) attached = true;
    }
    if (!env) {
        __android_log_print(ANDROID_LOG_WARN, "Mod_Menu",
                            "[wlww] 早期引导：AttachCurrentThread 失败，初始化跳过");
        return nullptr;
    }

    // 等 Application 就绪再做重活（第三次实测教训）：
    //   0.7 s 就注入时 FgoBoot 的 AppClassLoader 反查失败 ⇒ 退回 getSystemClassLoader，
    //   dex 被塞进不相干的 ClassLoader，紧接着 0.25 s 后进程硬崩（SIGSEGV，信号被屏蔽所以连
    //   tombstone 都没有）。改成轮询 ActivityThread.currentApplication()，拿到才继续。
    bool appReady = false;
    for (int i = 0; i < 240; ++i) {  // 最多 12 s
        jclass atCls = env->FindClass("android/app/ActivityThread");
        if (atCls) {
            jmethodID cur = env->GetStaticMethodID(
                atCls, "currentApplication", "()Landroid/app/Application;");
            if (cur) {
                jobject app = env->CallStaticObjectMethod(atCls, cur);
                if (app) {
                    env->DeleteLocalRef(app);
                    appReady = true;
                }
            }
            env->DeleteLocalRef(atCls);
        }
        if (env->ExceptionCheck()) env->ExceptionClear();
        if (appReady) break;
        usleep(50 * 1000);
    }
    __android_log_print(ANDROID_LOG_INFO, "Mod_Menu",
                        "[wlww] 早期引导：Application %s（等 %.1f s 后触发 JNI_OnLoad）",
                        appReady ? "已就绪" : "超时未就绪，硬着头皮继续", appReady ? 0.0 : 12.0);

    __android_log_print(ANDROID_LOG_INFO, "Mod_Menu",
                        "[wlww] 早期引导：JavaVM=%p env=%p，手工触发 JNI_OnLoad", (void*)vm, (void*)env);
    JNI_OnLoad(vm, nullptr);

    if (attached) vm->DetachCurrentThread();
    return nullptr;
}
}  // namespace

__attribute__((constructor)) static void wlww_early_boot() {
    // ① 最早期、最轻量：只装信号隔离（libtprt 的「伪装崩溃」是 SIGSEGV/SIGBUS，屏蔽即不生效）
    SignalQuarantine_Start();

    // ② 重活丢线程（见 BootThread 注释）。分离线程，失败也不影响进程。
    pthread_t th;
    if (pthread_create(&th, nullptr, BootThread, nullptr) == 0) {
        pthread_detach(th);
    } else {
        __android_log_print(ANDROID_LOG_WARN, "Mod_Menu",
                            "[wlww] 早期引导：引导线程创建失败");
    }
}
