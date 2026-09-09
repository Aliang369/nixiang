/* ================================================================
 *  王国大作战：前线 1.1.7646 — AOT 桥接 Trace（HybridCLR 热更前置）
 *  STARRY_TRACE=1 → adb logcat -s StarryTrace StarryMagic:V
 *  RVA 来自 Il2CppDumper dump.cs（03_decompiled/王国大作战前线）
 * ================================================================ */

#include "WgdzzMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <fcntl.h>
#include <unistd.h>
#include <pthread.h>
#include <sys/mman.h>
#include <sys/stat.h>
#include <sys/types.h>

#include <jni.h>
#include <android/log.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"

#if defined(STARRY_TRACE)
#define WGDZZ_TRACE(...) \
    __android_log_print(ANDROID_LOG_INFO, "StarryTrace", "[Wgdzz] " __VA_ARGS__)
#else
#define WGDZZ_TRACE(...) ((void)0)
#endif

enum : uintptr_t {
    ELF_BIAS = 0x1000,
    RVA_ON_MODIFY_PROP      = 0x1979F24,  // rkt_EventBridgeWrap.OnEvent_ModifyPropEffect
    RVA_ON_CALC_DMG_BEFORE  = 0x19795F4,
    RVA_ON_BEFORE_REDUCE_HP = 0x19796B8,
    RVA_VALIDATE_DAMAGE     = 0x193EDAC,  // rkt_BattleValidateBridgeWrap.ValidateDamage
    RVA_RECORD_SET_ACTIVE   = 0x193DA88,
    RVA_RECORD_GET_ACTIVE   = 0x193D9C4,
    RVA_WRITE_RESULT_QUIT   = 0x193DFBC,
    RVA_RESET_VALIDATE      = 0x193E9D8,
    RVA_CHANGE_TIMESCALE    = 0x19E1C78,  // rkt_TimeScaleManagerWrap.ChangeTimeScale
};

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_coreHooksDone{false};
static std::atomic<bool> g_refApkReady{false};

static const char kRefBaseApk[] =
    "/data/user/0/com.xr.wgdzztap/files/starry_ref/base.apk";
static const char kRefBaseApkStaging[] =
    "/data/local/tmp/wgdzz_official_ref.apk";

typedef int (*Open2Fn_t)(const char* path, int flags);
typedef int (*OpenAt2Fn_t)(int dirfd, const char* path, int flags);
typedef void* (*MmapFn_t)(void* addr, size_t len, int prot, int flags, int fd, off_t offset);

static Open2Fn_t g_origOpen = nullptr;
static OpenAt2Fn_t g_origOpenAt = nullptr;
static MmapFn_t g_origMmap = nullptr;
static JavaVM* g_wgdzzJvm = nullptr;

#if defined(STARRY_TRACE)
typedef void (*ExitFn_t)(int code);
typedef void* (*DlopenFn_t)(const char* name, int flags);
typedef int (*KillFn_t)(pid_t pid, int sig);
typedef void (*JniSystemExitFn_t)(JNIEnv*, jclass, jint);
typedef void (*JniRuntimeExitFn_t)(JNIEnv*, jobject, jint);
typedef void (*JniKillProcessFn_t)(JNIEnv*, jclass, jint);
static ExitFn_t g_origExit = nullptr;
static ExitFn_t g_origUnderscoreExit = nullptr;
static DlopenFn_t g_origDlopen = nullptr;
static KillFn_t g_origKill = nullptr;
static JniSystemExitFn_t g_origJniSystemExit = nullptr;
static JniRuntimeExitFn_t g_origJniRuntimeExit = nullptr;
static JniKillProcessFn_t g_origJniKillProcess = nullptr;
static std::atomic<bool> g_traceHooksDone{false};
static std::atomic<uint32_t> g_javaExitLogCount{0};
#endif

static bool PathInteresting(const char* path) {
    if (!path) return false;
    return strstr(path, ".jiagu") || strstr(path, "classes.dex") || strstr(path, "wgdzztap")
        || strstr(path, ".apk") || strstr(path, "mini.dex") || strstr(path, "wholedex");
}

static void LogPath(const char* tag, const char* path) {
#if defined(STARRY_TRACE)
    if (!PathInteresting(path)) return;
    WGDZZ_TRACE("%s %s", tag, path);
#else
    (void)tag;
    (void)path;
#endif
}

static void InitRefApk() {
    if (access(kRefBaseApkStaging, F_OK) == 0 || access(kRefBaseApk, F_OK) == 0) {
        g_refApkReady.store(true);
        const char* use = access(kRefBaseApkStaging, F_OK) == 0 ? kRefBaseApkStaging : kRefBaseApk;
        LOGI("[Wgdzz] official ref apk ready: %s", use);
        WGDZZ_TRACE("ref apk ok %s", use);
    } else {
        LOGE("[Wgdzz] missing ref apk (run wgdzz_push_official_ref.sh)");
    }
}

static const char* RefApkPath() {
    if (!g_refApkReady.load()) return nullptr;
    if (access(kRefBaseApkStaging, F_OK) == 0) return kRefBaseApkStaging;
    if (access(kRefBaseApk, F_OK) == 0) return kRefBaseApk;
    return nullptr;
}

static bool ShouldRedirectBaseApk(const char* path) {
    if (!path || !g_refApkReady.load()) return false;
    const size_t n = strlen(path);
    if (n < 8 || strcmp(path + n - 8, "base.apk") != 0) return false;
    return strstr(path, "wgdzztap") != nullptr;
}

static const char* ResolveOpenPath(const char* path) {
    if (!ShouldRedirectBaseApk(path)) return path;
    const char* ref = RefApkPath();
    return ref ? ref : path;
}

static bool FdPath(char* out, size_t outLen, int fd) {
    if (fd < 0 || outLen < 32) return false;
    char link[64];
    snprintf(link, sizeof(link), "/proc/self/fd/%d", fd);
    const ssize_t n = readlink(link, out, outLen - 1);
    if (n <= 0) return false;
    out[n] = '\0';
    return true;
}

static bool IsReadOnlyOpen(int flags) { return (flags & O_ACCMODE) == O_RDONLY; }

static int Hook_Open(const char* path, int flags) {
    const char* real = IsReadOnlyOpen(flags) ? ResolveOpenPath(path) : path;
    if (real != path) {
        LOGI("[Wgdzz] open redirect -> official ref");
        WGDZZ_TRACE("open redirect %s -> %s", path, real);
    } else {
        LogPath("open", path);
    }
    return g_origOpen ? g_origOpen(real, flags) : -1;
}

static int Hook_OpenAt(int dirfd, const char* path, int flags) {
    const char* real = IsReadOnlyOpen(flags) ? ResolveOpenPath(path) : path;
    if (real != path) {
        LOGI("[Wgdzz] openat redirect -> official ref");
        WGDZZ_TRACE("openat redirect %s -> %s", path, real);
    } else {
        LogPath("openat", path);
    }
    return g_origOpenAt ? g_origOpenAt(dirfd, real, flags) : -1;
}

static void* Hook_Mmap(void* addr, size_t len, int prot, int flags, int fd, off_t offset) {
    if (fd >= 0 && g_refApkReady.load() && g_origOpen) {
        char path[512];
        if (FdPath(path, sizeof(path), fd) && ShouldRedirectBaseApk(path)) {
            const char* ref = RefApkPath();
            if (!ref) return g_origMmap ? g_origMmap(addr, len, prot, flags, fd, offset) : MAP_FAILED;
            const int refFd = g_origOpen(ref, O_RDONLY);
            if (refFd >= 0) {
                LOGI("[Wgdzz] mmap redirect -> official ref");
                WGDZZ_TRACE("mmap redirect %s off=%ld len=%zu", path, (long)offset, len);
                void* ret = g_origMmap(addr, len, prot, flags, refFd, offset);
                close(refFd);
                return ret;
            }
        }
    }
    return g_origMmap ? g_origMmap(addr, len, prot, flags, fd, offset) : MAP_FAILED;
}

static bool InstallCoreHooks() {
    if (g_coreHooksDone.load()) return true;
    bool ok = false;
    if (!g_origOpen) {
        void* openSym = DobbySymbolResolver("libc.so", "open");
        if (openSym && DobbyHook(openSym, (void*)Hook_Open, (void**)&g_origOpen) == 0) {
            WGDZZ_TRACE("hook ok libc open");
            ok = true;
        }
    }
    if (!g_origOpenAt) {
        void* openatSym = DobbySymbolResolver("libc.so", "openat");
        if (openatSym && DobbyHook(openatSym, (void*)Hook_OpenAt, (void**)&g_origOpenAt) == 0) {
            WGDZZ_TRACE("hook ok libc openat");
            ok = true;
        }
    }
    if (!g_origMmap) {
        void* mmapSym = DobbySymbolResolver("libc.so", "mmap");
        if (mmapSym && DobbyHook(mmapSym, (void*)Hook_Mmap, (void**)&g_origMmap) == 0) {
            WGDZZ_TRACE("hook ok libc mmap");
            ok = true;
        }
    }
    if (g_origOpen || g_origOpenAt) {
        g_coreHooksDone.store(true);
        return true;
    }
    return ok;
}

#if defined(STARRY_TRACE)
static bool IsLibLoaded(const char* needle) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return false;
    char line[768];
    bool hit = false;
    while (fgets(line, sizeof(line), fp)) {
        if (strstr(line, needle)) {
            hit = true;
            break;
        }
    }
    fclose(fp);
    return hit;
}

static void LogJavaStack(const char* tag) {
    if (!g_wgdzzJvm) return;
    JNIEnv* env = nullptr;
    const bool attached = (g_wgdzzJvm->GetEnv((void**)&env, JNI_VERSION_1_6) == JNI_EDETACHED);
    if (attached && g_wgdzzJvm->AttachCurrentThread(&env, nullptr) != JNI_OK) return;
    if (!env) {
        if (attached) g_wgdzzJvm->DetachCurrentThread();
        return;
    }
    jclass logCls = env->FindClass("android/util/Log");
    jmethodID getStack = env->GetStaticMethodID(
        logCls, "getStackTraceString", "(Ljava/lang/Throwable;)Ljava/lang/String;");
    if (logCls && getStack) {
        jclass thrCls = env->FindClass("java/lang/Exception");
        jobject thr = env->NewObject(thrCls, env->GetMethodID(thrCls, "<init>", "()V"));
        jstring msg = (jstring)env->CallStaticObjectMethod(logCls, getStack, thr);
        if (msg) {
            const char* utf = env->GetStringUTFChars(msg, nullptr);
            if (utf) {
                WGDZZ_TRACE("%s stack:\n%s", tag, utf);
                env->ReleaseStringUTFChars(msg, utf);
            }
        }
        env->DeleteLocalRef(thr);
    }
    if (attached) g_wgdzzJvm->DetachCurrentThread();
}

static int Hook_Kill(pid_t pid, int sig) {
    WGDZZ_TRACE("kill(%d, %d)", (int)pid, sig);
    if (sig == 9 || sig == 6) LogJavaStack("kill");
    return g_origKill ? g_origKill(pid, sig) : -1;
}

static void Hook_Exit(int code) {
    WGDZZ_TRACE("exit(%d)", code);
    if (g_origExit) g_origExit(code);
}

static void Hook_UnderscoreExit(int code) {
    WGDZZ_TRACE("_exit(%d)", code);
    if (g_origUnderscoreExit) g_origUnderscoreExit(code);
}

static void Hook_JniSystemExit(JNIEnv* env, jclass cls, jint code) {
    const uint32_t n = ++g_javaExitLogCount;
    WGDZZ_TRACE("System.exit(%d) (#%u)", (int)code, n);
    if (n <= 5) LogJavaStack("System.exit");
    if (g_origJniSystemExit) g_origJniSystemExit(env, cls, code);
}

static void Hook_JniRuntimeExit(JNIEnv* env, jobject self, jint code) {
    const uint32_t n = ++g_javaExitLogCount;
    WGDZZ_TRACE("Runtime.exit(%d) (#%u)", (int)code, n);
    if (n <= 5) LogJavaStack("Runtime.exit");
    if (g_origJniRuntimeExit) g_origJniRuntimeExit(env, self, code);
}

static void Hook_JniKillProcess(JNIEnv* env, jclass cls, jint pid) {
    WGDZZ_TRACE("Process.killProcess(%d)", (int)pid);
    LogJavaStack("killProcess");
    if (g_origJniKillProcess) g_origJniKillProcess(env, cls, pid);
}

static bool HookJniExitOnce() {
    bool any = false;
    if (!g_origJniSystemExit) {
        void* sym = DobbySymbolResolver("libandroid_runtime.so", "Java_java_lang_System_exit__I");
        if (sym && DobbyHook(sym, (void*)Hook_JniSystemExit, (void**)&g_origJniSystemExit) == 0) {
            WGDZZ_TRACE("hook ok System.exit JNI");
            any = true;
        }
    }
    if (!g_origJniRuntimeExit) {
        void* sym = DobbySymbolResolver("libandroid_runtime.so", "Java_java_lang_Runtime_exit__I");
        if (!sym) sym = DobbySymbolResolver("libandroid_runtime.so", "Java_java_lang_Runtime_exitInternal__I");
        if (sym && DobbyHook(sym, (void*)Hook_JniRuntimeExit, (void**)&g_origJniRuntimeExit) == 0) {
            WGDZZ_TRACE("hook ok Runtime.exit JNI");
            any = true;
        }
    }
    if (!g_origJniKillProcess) {
        void* sym = DobbySymbolResolver("libandroid_runtime.so", "Java_android_os_Process_killProcess__I");
        if (sym && DobbyHook(sym, (void*)Hook_JniKillProcess, (void**)&g_origJniKillProcess) == 0) {
            WGDZZ_TRACE("hook ok Process.killProcess JNI");
            any = true;
        }
    }
    return any;
}

static bool InstallTraceHooks() {
    if (g_traceHooksDone.load()) return true;
    if (!g_origKill) {
        void* killSym = DobbySymbolResolver("libc.so", "kill");
        if (killSym && DobbyHook(killSym, (void*)Hook_Kill, (void**)&g_origKill) == 0) {
            WGDZZ_TRACE("hook ok libc kill");
        }
    }
    HookJniExitOnce();
    if (!g_origExit) {
        void* exitSym = DobbySymbolResolver("libc.so", "exit");
        if (exitSym && DobbyHook(exitSym, (void*)Hook_Exit, (void**)&g_origExit) == 0) {
            WGDZZ_TRACE("hook ok libc exit");
        }
    }
    if (!g_origUnderscoreExit) {
        void* uexit = DobbySymbolResolver("libc.so", "_exit");
        if (uexit && DobbyHook(uexit, (void*)Hook_UnderscoreExit, (void**)&g_origUnderscoreExit) == 0) {
            WGDZZ_TRACE("hook ok libc _exit");
        }
    }
    g_traceHooksDone.store(true);
    return true;
}
#endif  // STARRY_TRACE

static const char kSidecarDex[] = "/data/local/tmp/wgdzz_classes5.dex";

static JNIEnv* MenuGetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_wgdzzJvm) return nullptr;
    JNIEnv* env = nullptr;
    const int st = g_wgdzzJvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_wgdzzJvm->AttachCurrentThread(&env, nullptr) != 0) return nullptr;
        *needDetach = true;
    } else if (st != JNI_OK) {
        return nullptr;
    }
    return env;
}

static jobject GetApplication(JNIEnv* env) {
    jclass atCls = env->FindClass("android/app/ActivityThread");
    if (!atCls) {
        env->ExceptionClear();
        return nullptr;
    }
    jobject app = nullptr;
    jmethodID curApp = env->GetStaticMethodID(atCls, "currentApplication", "()Landroid/app/Application;");
    if (curApp) app = env->CallStaticObjectMethod(atCls, curApp);
    if (!app) {
        env->ExceptionClear();
        jmethodID curAt = env->GetStaticMethodID(atCls, "currentActivityThread", "()Landroid/app/ActivityThread;");
        jobject at = curAt ? env->CallStaticObjectMethod(atCls, curAt) : nullptr;
        if (at) {
            jmethodID getApp = env->GetMethodID(atCls, "getApplication", "()Landroid/app/Application;");
            if (getApp) app = env->CallObjectMethod(at, getApp);
            if (!app) {
                env->ExceptionClear();
                jfieldID initFld = env->GetFieldID(atCls, "mInitialApplication", "Landroid/app/Application;");
                if (initFld) app = env->GetObjectField(at, initFld);
            }
            if (!app) {
                env->ExceptionClear();
                jclass clsCls = env->FindClass("java/lang/Class");
                jmethodID getDecl = env->GetMethodID(clsCls, "getDeclaredField", "(Ljava/lang/String;)Ljava/lang/reflect/Field;");
                jstring jfld = env->NewStringUTF("mInitialApplication");
                jobject field = env->CallObjectMethod(atCls, getDecl, jfld);
                env->DeleteLocalRef(jfld);
                if (field) {
                    jclass fieldCls = env->FindClass("java/lang/reflect/Field");
                    jmethodID setAcc = env->GetMethodID(fieldCls, "setAccessible", "(Z)V");
                    jmethodID fldGet = env->GetMethodID(fieldCls, "get", "(Ljava/lang/Object;)Ljava/lang/Object;");
                    env->CallVoidMethod(field, setAcc, JNI_TRUE);
                    app = env->CallObjectMethod(field, fldGet, at);
                    env->DeleteLocalRef(fieldCls);
                    env->DeleteLocalRef(field);
                }
                env->DeleteLocalRef(clsCls);
            }
            env->DeleteLocalRef(at);
        }
        if (!app) env->ExceptionClear();
    }
    env->DeleteLocalRef(atCls);
    return app;
}

/** Kiwi StubApplication 常长期无 Application；Unity 已起时从 mActivities 取 Activity 作 Context。 */
static jobject ReflectGetField(JNIEnv* env, jobject obj, const char* name) {
    if (!obj) return nullptr;
    jclass objCls = env->GetObjectClass(obj);
    jclass clsCls = env->FindClass("java/lang/Class");
    jmethodID getDeclField =
        env->GetMethodID(clsCls, "getDeclaredField", "(Ljava/lang/String;)Ljava/lang/reflect/Field;");
    jstring jname = env->NewStringUTF(name);
    jobject field = getDeclField ? env->CallObjectMethod(objCls, getDeclField, jname) : nullptr;
    env->DeleteLocalRef(jname);
    env->DeleteLocalRef(objCls);
    if (!field || env->ExceptionCheck()) {
        env->ExceptionClear();
        env->DeleteLocalRef(clsCls);
        return nullptr;
    }
    jclass fieldCls = env->FindClass("java/lang/reflect/Field");
    jmethodID setAcc = env->GetMethodID(fieldCls, "setAccessible", "(Z)V");
    jmethodID fldGet = env->GetMethodID(fieldCls, "get", "(Ljava/lang/Object;)Ljava/lang/Object;");
    env->CallVoidMethod(field, setAcc, JNI_TRUE);
    jobject val = fldGet ? env->CallObjectMethod(field, fldGet, obj) : nullptr;
    env->DeleteLocalRef(fieldCls);
    env->DeleteLocalRef(field);
    env->DeleteLocalRef(clsCls);
    if (env->ExceptionCheck()) {
        env->ExceptionClear();
        return nullptr;
    }
    return val;
}

static bool ReflectGetBooleanField(JNIEnv* env, jobject obj, const char* name, jboolean* out) {
    if (!obj) return false;
    jclass objCls = env->GetObjectClass(obj);
    jclass clsCls = env->FindClass("java/lang/Class");
    jmethodID getDeclField =
        env->GetMethodID(clsCls, "getDeclaredField", "(Ljava/lang/String;)Ljava/lang/reflect/Field;");
    jstring jname = env->NewStringUTF(name);
    jobject field = getDeclField ? env->CallObjectMethod(objCls, getDeclField, jname) : nullptr;
    env->DeleteLocalRef(jname);
    env->DeleteLocalRef(objCls);
    if (!field || env->ExceptionCheck()) {
        env->ExceptionClear();
        env->DeleteLocalRef(clsCls);
        return false;
    }
    jclass fieldCls = env->FindClass("java/lang/reflect/Field");
    jmethodID setAcc = env->GetMethodID(fieldCls, "setAccessible", "(Z)V");
    jmethodID getBoolean = env->GetMethodID(fieldCls, "getBoolean", "(Ljava/lang/Object;)Z");
    env->CallVoidMethod(field, setAcc, JNI_TRUE);
    *out = getBoolean ? env->CallBooleanMethod(field, getBoolean, obj) : JNI_FALSE;
    env->DeleteLocalRef(fieldCls);
    env->DeleteLocalRef(field);
    env->DeleteLocalRef(clsCls);
    if (env->ExceptionCheck()) {
        env->ExceptionClear();
        return false;
    }
    return true;
}

static jobject FindActivityInThread(JNIEnv* env, bool requireNotPaused) {
    jclass atCls = env->FindClass("android/app/ActivityThread");
    if (!atCls) {
        env->ExceptionClear();
        return nullptr;
    }
    jmethodID curAt = env->GetStaticMethodID(atCls, "currentActivityThread", "()Landroid/app/ActivityThread;");
    jobject at = curAt ? env->CallStaticObjectMethod(atCls, curAt) : nullptr;
    env->DeleteLocalRef(atCls);
    if (!at) {
        env->ExceptionClear();
        return nullptr;
    }

    jobject actsMap = ReflectGetField(env, at, "mActivities");
    env->DeleteLocalRef(at);
    if (!actsMap) return nullptr;

    jclass amCls = env->GetObjectClass(actsMap);
    jmethodID sizeMid = env->GetMethodID(amCls, "size", "()I");
    jmethodID valueAt = env->GetMethodID(amCls, "valueAt", "(I)Ljava/lang/Object;");
    env->DeleteLocalRef(amCls);
    if (!sizeMid || !valueAt) {
        env->ExceptionClear();
        env->DeleteLocalRef(actsMap);
        return nullptr;
    }

    jclass actCls = env->FindClass("android/app/Activity");
    jmethodID isFinishing = env->GetMethodID(actCls, "isFinishing", "()Z");
    const jsize n = env->CallIntMethod(actsMap, sizeMid);
    for (jsize i = 0; i < n; i++) {
        jobject rec = env->CallObjectMethod(actsMap, valueAt, i);
        if (!rec) continue;
        jboolean paused = JNI_TRUE;
        if (!ReflectGetBooleanField(env, rec, "paused", &paused)) paused = JNI_FALSE;
        if (requireNotPaused && paused) {
            env->DeleteLocalRef(rec);
            continue;
        }
        jobject act = ReflectGetField(env, rec, "activity");
        env->DeleteLocalRef(rec);
        if (!act) continue;
        if (isFinishing && env->CallBooleanMethod(act, isFinishing)) {
            env->DeleteLocalRef(act);
            continue;
        }
        env->DeleteLocalRef(actCls);
        env->DeleteLocalRef(actsMap);
        return act;
    }
    env->DeleteLocalRef(actCls);
    env->DeleteLocalRef(actsMap);
    return nullptr;
}

static jobject FindApplicationInThread(JNIEnv* env) {
    jclass atCls = env->FindClass("android/app/ActivityThread");
    if (!atCls) {
        env->ExceptionClear();
        return nullptr;
    }
    jmethodID curAt = env->GetStaticMethodID(atCls, "currentActivityThread", "()Landroid/app/ActivityThread;");
    jobject at = curAt ? env->CallStaticObjectMethod(atCls, curAt) : nullptr;
    env->DeleteLocalRef(atCls);
    if (!at) {
        env->ExceptionClear();
        return nullptr;
    }
    jobject allApps = ReflectGetField(env, at, "mAllApplications");
    env->DeleteLocalRef(at);
    if (!allApps) return nullptr;

    jclass listCls = env->GetObjectClass(allApps);
    jmethodID sizeMid = env->GetMethodID(listCls, "size", "()I");
    jmethodID getMid = env->GetMethodID(listCls, "get", "(I)Ljava/lang/Object;");
    env->DeleteLocalRef(listCls);
    if (!sizeMid || !getMid) {
        env->ExceptionClear();
        env->DeleteLocalRef(allApps);
        return nullptr;
    }
    const jsize n = env->CallIntMethod(allApps, sizeMid);
    jobject app = (n > 0) ? env->CallObjectMethod(allApps, getMid, (jsize)0) : nullptr;
    env->DeleteLocalRef(allApps);
    return app;
}

static jobject GetContextFromLoadedApk(JNIEnv* env) {
    jclass atCls = env->FindClass("android/app/ActivityThread");
    if (!atCls) {
        env->ExceptionClear();
        return nullptr;
    }
    jmethodID curAt = env->GetStaticMethodID(atCls, "currentActivityThread", "()Landroid/app/ActivityThread;");
    jobject at = curAt ? env->CallStaticObjectMethod(atCls, curAt) : nullptr;
    env->DeleteLocalRef(atCls);
    if (!at) {
        env->ExceptionClear();
        return nullptr;
    }
    jobject mPackages = ReflectGetField(env, at, "mPackages");
    if (!mPackages) {
        env->DeleteLocalRef(at);
        return nullptr;
    }
    jclass amCls = env->GetObjectClass(mPackages);
    jmethodID sizeMid = env->GetMethodID(amCls, "size", "()I");
    jmethodID valueAt = env->GetMethodID(amCls, "valueAt", "(I)Ljava/lang/Object;");
    env->DeleteLocalRef(amCls);
    if (!sizeMid || !valueAt) {
        env->ExceptionClear();
        env->DeleteLocalRef(mPackages);
        env->DeleteLocalRef(at);
        return nullptr;
    }
    jobject loadedApk = nullptr;
    const jsize n = env->CallIntMethod(mPackages, sizeMid);
    for (jsize i = 0; i < n; i++) {
        jobject val = env->CallObjectMethod(mPackages, valueAt, i);
        if (!val) continue;
        jobject apk = val;
        jclass wrCls = env->FindClass("java/lang/ref/WeakReference");
        if (wrCls && env->IsInstanceOf(val, wrCls)) {
            jmethodID wrGet = env->GetMethodID(wrCls, "get", "()Ljava/lang/Object;");
            apk = wrGet ? env->CallObjectMethod(val, wrGet) : nullptr;
            env->DeleteLocalRef(wrCls);
            env->DeleteLocalRef(val);
            if (!apk) continue;
        } else {
            env->DeleteLocalRef(val);
        }
        loadedApk = apk;
        break;
    }
    env->DeleteLocalRef(mPackages);
    if (!loadedApk) {
        env->DeleteLocalRef(at);
        return nullptr;
    }
    jclass ctxImplCls = env->FindClass("android/app/ContextImpl");
    if (!ctxImplCls) {
        env->ExceptionClear();
        env->DeleteLocalRef(loadedApk);
        env->DeleteLocalRef(at);
        return nullptr;
    }
    jmethodID createCtx = env->GetStaticMethodID(
        ctxImplCls, "createAppContext",
        "(Landroid/app/ActivityThread;Landroid/app/LoadedApk;)Landroid/app/ContextImpl;");
    jobject ctx = createCtx ? env->CallStaticObjectMethod(ctxImplCls, createCtx, at, loadedApk) : nullptr;
    env->DeleteLocalRef(ctxImplCls);
    env->DeleteLocalRef(loadedApk);
    env->DeleteLocalRef(at);
    if (env->ExceptionCheck()) {
        env->ExceptionClear();
        return nullptr;
    }
    return ctx;
}

static jobject GetBootstrapContext(JNIEnv* env, const char** src, bool allowLoadedApk) {
    jobject ctx = GetApplication(env);
    if (ctx) {
        if (src) *src = "Application";
        return ctx;
    }
    ctx = FindApplicationInThread(env);
    if (ctx) {
        if (src) *src = "mAllApplications";
        return ctx;
    }
    ctx = FindActivityInThread(env, true);
    if (!ctx) ctx = FindActivityInThread(env, false);
    if (ctx) {
        if (src) *src = "Activity";
        return ctx;
    }
    if (allowLoadedApk) {
        ctx = GetContextFromLoadedApk(env);
        if (ctx) {
            if (src) *src = "LoadedApk";
            return ctx;
        }
    }
    if (src) *src = nullptr;
    return nullptr;
}

static bool BootstrapMenuOnce(JNIEnv* env, bool allowLoadedApk) {
    if (access(kSidecarDex, R_OK) != 0) {
        LOGE("[Wgdzz] sidecar dex missing: %s (run wgdzz_post_install_swap.sh)", kSidecarDex);
        return false;
    }
    const char* ctxSrc = nullptr;
    jobject app = GetBootstrapContext(env, &ctxSrc, allowLoadedApk);
    if (!app) {
        return false;
    }
    LOGI("[Wgdzz] menu bootstrap ctx from %s", ctxSrc ? ctxSrc : "?");

    jclass ctxCls = env->GetObjectClass(app);
    jmethodID getCl = env->GetMethodID(ctxCls, "getClassLoader", "()Ljava/lang/ClassLoader;");
    jobject parent = env->CallObjectMethod(app, getCl);
    jmethodID getCcd = env->GetMethodID(ctxCls, "getCodeCacheDir", "()Ljava/io/File;");
    jobject ccd = env->CallObjectMethod(app, getCcd);
    env->DeleteLocalRef(ctxCls);
    if (!parent || !ccd) {
        env->ExceptionClear();
        env->DeleteLocalRef(app);
        return false;
    }
    jclass fileCls = env->GetObjectClass(ccd);
    jmethodID getPath = env->GetMethodID(fileCls, "getAbsolutePath", "()Ljava/lang/String;");
    jstring jopt = (jstring)env->CallObjectMethod(ccd, getPath);
    env->DeleteLocalRef(fileCls);
    env->DeleteLocalRef(ccd);

    jclass dclCls = env->FindClass("dalvik/system/DexClassLoader");
    jmethodID dclInit = env->GetMethodID(
        dclCls, "<init>",
        "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/ClassLoader;)V");
    jstring jdex = env->NewStringUTF(kSidecarDex);
    jobject dcl = env->NewObject(dclCls, dclInit, jdex, jopt, nullptr, parent);
    env->DeleteLocalRef(jdex);
    env->DeleteLocalRef(jopt);
    env->DeleteLocalRef(dclCls);
    env->DeleteLocalRef(parent);
    if (!dcl) {
        env->ExceptionDescribe();
        env->ExceptionClear();
        env->DeleteLocalRef(app);
        LOGE("[Wgdzz] DexClassLoader failed");
        return false;
    }

    jclass clCls = env->GetObjectClass(dcl);
    jmethodID loadClass = env->GetMethodID(clCls, "loadClass", "(Ljava/lang/String;)Ljava/lang/Class;");
    jstring jname = env->NewStringUTF("com.starry.magic.wgdzz.DexInstaller");
    jclass bootCls = (jclass)env->CallObjectMethod(dcl, loadClass, jname);
    env->DeleteLocalRef(jname);
    env->DeleteLocalRef(clCls);
    env->DeleteLocalRef(dcl);
    if (!bootCls) {
        env->ExceptionDescribe();
        env->ExceptionClear();
        env->DeleteLocalRef(app);
        LOGE("[Wgdzz] loadClass DexInstaller failed");
        return false;
    }

    jmethodID boot = env->GetStaticMethodID(
        bootCls, "boot", "(Landroid/content/Context;Ljava/lang/String;)V");
    if (!boot) {
        env->ExceptionClear();
        env->DeleteLocalRef(bootCls);
        env->DeleteLocalRef(app);
        LOGE("[Wgdzz] DexInstaller.boot missing");
        return false;
    }
    jstring jdexPath = env->NewStringUTF(kSidecarDex);
    env->CallStaticVoidMethod(bootCls, boot, app, jdexPath);
    env->DeleteLocalRef(jdexPath);
    if (env->ExceptionCheck()) {
        env->ExceptionDescribe();
        env->ExceptionClear();
        env->DeleteLocalRef(bootCls);
        env->DeleteLocalRef(app);
        LOGE("[Wgdzz] DexInstaller.boot threw");
        return false;
    }
    LOGI("[Wgdzz] DexInstaller.boot OK (sidecar classes5)");
    env->DeleteLocalRef(bootCls);
    env->DeleteLocalRef(app);
    return true;
}

static void* MenuBootstrapThread(void* /*arg*/) {
    for (int i = 0; i < 360; i++) {
        if (i > 0) usleep(500 * 1000);
        bool detach = false;
        JNIEnv* env = MenuGetEnv(&detach);
        if (!env) continue;
        const bool ok = BootstrapMenuOnce(env, i >= 30);
        if (detach) g_wgdzzJvm->DetachCurrentThread();
        if (ok) return nullptr;
        if (i > 0 && (i % 40) == 0) {
            LOGW("[Wgdzz] menu bootstrap waiting ctx (try %d)", i + 1);
        }
    }
    LOGE("[Wgdzz] menu bootstrap gave up");
    return nullptr;
}

void Wgdzz_StartMenuBootstrap(JavaVM* jvm) {
    static std::atomic_bool started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    g_wgdzzJvm = jvm;
    pthread_t th;
    if (pthread_create(&th, nullptr, MenuBootstrapThread, nullptr) == 0) {
        pthread_detach(th);
        LOGI("[Wgdzz] menu bootstrap thread started");
    } else {
        started.store(false);
        LOGE("[Wgdzz] menu bootstrap thread create failed");
    }
}

void Wgdzz_StartEarlyHooks(JavaVM* jvm) {
    static std::atomic_bool started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    g_wgdzzJvm = jvm;
    InitRefApk();
    if (access(kSidecarDex, R_OK) == 0) {
        LOGI("[Wgdzz] sidecar swap mode: skip IO redirect hooks");
    } else {
        InstallCoreHooks();
    }
#if defined(STARRY_TRACE)
    InstallTraceHooks();
#endif
    LOGI("[Wgdzz] early hooks ready (ref=%d)", g_refApkReady.load() ? 1 : 0);
}

static std::atomic<uint32_t> g_hitsModifyProp{0};
static std::atomic<uint32_t> g_hitsCalcBefore{0};
static std::atomic<uint32_t> g_hitsReduceHp{0};
static std::atomic<uint32_t> g_hitsValidate{0};
static std::atomic<uint32_t> g_hitsRecordSet{0};
static std::atomic<uint32_t> g_hitsWriteResult{0};
static std::atomic<uint32_t> g_hitsTimeScale{0};
static std::atomic<uint32_t> g_summaryTick{0};

typedef int32_t (*LuaCsFn_t)(intptr_t L);

static LuaCsFn_t g_origModifyProp = nullptr;
static LuaCsFn_t g_origCalcBefore = nullptr;
static LuaCsFn_t g_origReduceHp = nullptr;
static LuaCsFn_t g_origValidate = nullptr;
static LuaCsFn_t g_origRecordSet = nullptr;
static LuaCsFn_t g_origWriteResult = nullptr;
static LuaCsFn_t g_origChangeTs = nullptr;

static void* FileOffToVa(const char* lib, uintptr_t fileOff) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return nullptr;
    char line[768];
    void* found = nullptr;
    while (fgets(line, sizeof(line), fp)) {
        if (!strstr(line, lib)) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x')) continue;
        if (fileOff >= off && fileOff < off + (end - start)) {
            found = (void*)(start + (fileOff - off));
            break;
        }
    }
    fclose(fp);
    return found;
}

static void* Abs(uintptr_t rva) {
    void* va = FileOffToVa(GameUnity::TARGET_LIB, rva - ELF_BIAS);
    if (va) return va;
    char buf[32];
    snprintf(buf, sizeof(buf), "0x%lX", (unsigned long)rva);
    return getAbsoluteAddress(GameUnity::TARGET_LIB, buf);
}

static bool HookOne(const char* tag, uintptr_t rva, void* hook, void** orig) {
    void* addr = Abs(rva);
    if (!addr) {
        LOGE("[Wgdzz] %s resolve failed (rva=0x%lX)", tag, (unsigned long)rva);
        return false;
    }
    if (*orig) return true;
    if (DobbyHook(addr, hook, orig) != 0) {
        LOGE("[Wgdzz] DobbyHook %s failed @ %p", tag, addr);
        return false;
    }
    LOGI("[Wgdzz] hooked %s @ %p", tag, addr);
    return true;
}

static int32_t Hook_ModifyProp(intptr_t L) {
    int32_t r = g_origModifyProp ? g_origModifyProp(L) : 0;
    const uint32_t n = ++g_hitsModifyProp;
    if ((n & 0x1F) == 1) WGDZZ_TRACE("ModifyPropEffect hits=%u", n);
    return r;
}

static int32_t Hook_CalcBefore(intptr_t L) {
    int32_t r = g_origCalcBefore ? g_origCalcBefore(L) : 0;
    const uint32_t n = ++g_hitsCalcBefore;
    if ((n & 0x1F) == 1) WGDZZ_TRACE("CalcDamageBefore hits=%u", n);
    return r;
}

static int32_t Hook_ReduceHp(intptr_t L) {
    int32_t r = g_origReduceHp ? g_origReduceHp(L) : 0;
    const uint32_t n = ++g_hitsReduceHp;
    if ((n & 0x1F) == 1) WGDZZ_TRACE("BeforeReduceHp hits=%u", n);
    return r;
}

static int32_t Hook_Validate(intptr_t L) {
    int32_t r = g_origValidate ? g_origValidate(L) : 0;
    const uint32_t n = ++g_hitsValidate;
    if ((n & 0xF) == 1) WGDZZ_TRACE("ValidateDamage hits=%u ret=%d", n, r);
    return r;
}

static int32_t Hook_RecordSetActive(intptr_t L) {
    int32_t r = g_origRecordSet ? g_origRecordSet(L) : 0;
    const uint32_t n = ++g_hitsRecordSet;
    if (n <= 8 || (n & 0x7) == 0)
        WGDZZ_TRACE("BattleRecord.set_Active hits=%u ret=%d", n, r);
    return r;
}

static int32_t Hook_WriteResult(intptr_t L) {
    int32_t r = g_origWriteResult ? g_origWriteResult(L) : 0;
    const uint32_t n = ++g_hitsWriteResult;
    WGDZZ_TRACE("WriteResultAndQuit hits=%u ret=%d", n, r);
    return r;
}

static int32_t Hook_ChangeTs(intptr_t L) {
    int32_t r = g_origChangeTs ? g_origChangeTs(L) : 0;
    ++g_hitsTimeScale;
    return r;
}

static void LogSummary() {
    WGDZZ_TRACE(
        "SUM modify=%u calcBefore=%u reduceHp=%u validate=%u recordSet=%u writeResult=%u "
        "timeScale=%u",
        g_hitsModifyProp.load(), g_hitsCalcBefore.load(), g_hitsReduceHp.load(),
        g_hitsValidate.load(), g_hitsRecordSet.load(), g_hitsWriteResult.load(),
        g_hitsTimeScale.load());
}

bool Wgdzz_TryInit() {
    if (g_ready.load()) return true;
    if (!isLibraryLoaded(GameUnity::TARGET_LIB)) return false;

    LOGI("[Wgdzz] trace init (AOT bridge RVA)");

    int hooked = 0;
    if (HookOne("ModifyPropEffect", RVA_ON_MODIFY_PROP, (void*)Hook_ModifyProp,
                (void**)&g_origModifyProp))
        ++hooked;
    if (HookOne("CalcDamageBefore", RVA_ON_CALC_DMG_BEFORE, (void*)Hook_CalcBefore,
                (void**)&g_origCalcBefore))
        ++hooked;
    if (HookOne("BeforeReduceHp", RVA_ON_BEFORE_REDUCE_HP, (void*)Hook_ReduceHp,
                (void**)&g_origReduceHp))
        ++hooked;
    if (HookOne("ValidateDamage", RVA_VALIDATE_DAMAGE, (void*)Hook_Validate,
                (void**)&g_origValidate))
        ++hooked;
    if (HookOne("RecordSetActive", RVA_RECORD_SET_ACTIVE, (void*)Hook_RecordSetActive,
                (void**)&g_origRecordSet))
        ++hooked;
    if (HookOne("WriteResultAndQuit", RVA_WRITE_RESULT_QUIT, (void*)Hook_WriteResult,
                (void**)&g_origWriteResult))
        ++hooked;
    if (HookOne("ChangeTimeScale", RVA_CHANGE_TIMESCALE, (void*)Hook_ChangeTs,
                (void**)&g_origChangeTs))
        ++hooked;

    if (hooked < 3) {
        LOGE("[Wgdzz] too few hooks (%d/7)", hooked);
        return false;
    }

    g_ready.store(true);
    WGDZZ_TRACE("init ok hooks=%d — 请进单人局：杀怪/花银币/结算", hooked);
    return true;
}

bool Wgdzz_IsReady() { return g_ready.load(); }

void Wgdzz_ForceClear() { g_ready.store(false); }

void Wgdzz_Tick(const bool* /*features*/) {
    if (!g_ready.load()) return;
#if defined(STARRY_TRACE)
    const uint32_t t = ++g_summaryTick;
    if ((t % 20) == 0) LogSummary();
#endif
}

void Wgdzz_OnFeatureChanged(int featNum, bool enabled) {
    WGDZZ_TRACE("feat #%d = %s", featNum, enabled ? "ON" : "OFF");
}
