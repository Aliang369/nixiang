/* ================================================================
 *  异世界当欧皇 (com.cxgame.ysjdoh 1.0.104) — Cocos Creator 2.x
 *
 *  机制：**不装任何 native hook**。游戏逻辑已由 04_scripts/异世界当欧皇/
 *        ysjdoh_patch_js.py 直接改 assets/assets/scripts/index.jsc（XXTEA+gzip），
 *        补丁里读 window.__sby（由 cc.sys.localStorage 的 key=starry_ysjdoh 每 3s 轮询）。
 *        本文件只负责把菜单状态写进同一个 localStorage 库（经 Java YsjdohHooks）。
 *
 *  为什么不在 native 侧注入：Cocos 的 evalString hook 在多机型上不稳（先例
 *  不一样的修仙宗门2 实测 houdini 转译层 SIGSEGV）；改脚本 = 唯一稳定路径。
 * ================================================================ */
#include "YsjdohMod.h"

#include <atomic>
#include <dlfcn.h>
#include <time.h>
#include <android/log.h>

#define YSJ_LOG(...) __android_log_print(ANDROID_LOG_INFO, "ysjdoh", __VA_ARGS__)

static std::atomic<bool> g_ready{false};
static std::atomic<int>  g_mult{1};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_spd{false};
static std::atomic<bool> g_noad{false};
static std::atomic<long> g_lastWriteMs{0};
static long g_startMs = 0;

static JavaVM*   g_jvm = nullptr;
static jclass    g_hooksCls = nullptr;
static jmethodID g_mSetFeature = nullptr;
static jmethodID g_mSyncFeature = nullptr;
static jmethodID g_mSetValue = nullptr;
static jmethodID g_mSyncValue = nullptr;
static jmethodID g_mWriteCfg = nullptr;

static long NowMs() {
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return (long)ts.tv_sec * 1000L + ts.tv_nsec / 1000000L;
}

static JNIEnv* GetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_jvm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_jvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_jvm->AttachCurrentThread(&env, nullptr) != 0) return nullptr;
        *needDetach = true;
    } else if (st != JNI_OK) {
        return nullptr;
    }
    return env;
}

static jclass LoadAppClass(JNIEnv* env, const char* name) {
    jclass atCls = env->FindClass("android/app/ActivityThread");
    if (!atCls) { env->ExceptionClear(); return nullptr; }
    jmethodID curApp = env->GetStaticMethodID(atCls, "currentApplication", "()Landroid/app/Application;");
    if (!curApp) { env->ExceptionClear(); env->DeleteLocalRef(atCls); return nullptr; }
    jobject app = env->CallStaticObjectMethod(atCls, curApp);
    env->DeleteLocalRef(atCls);
    if (!app) { env->ExceptionClear(); return nullptr; }
    jclass appCls = env->GetObjectClass(app);
    jmethodID getCl = env->GetMethodID(appCls, "getClassLoader", "()Ljava/lang/ClassLoader;");
    jobject cl = getCl ? env->CallObjectMethod(app, getCl) : nullptr;
    env->DeleteLocalRef(appCls);
    env->DeleteLocalRef(app);
    if (!cl) { env->ExceptionClear(); return nullptr; }
    jclass clCls = env->GetObjectClass(cl);
    jmethodID loadClass = env->GetMethodID(clCls, "loadClass", "(Ljava/lang/String;)Ljava/lang/Class;");
    jstring jname = env->NewStringUTF(name);
    jclass cls = nullptr;
    if (loadClass && jname) {
        cls = (jclass)env->CallObjectMethod(cl, loadClass, jname);
        if (env->ExceptionCheck()) { env->ExceptionClear(); cls = nullptr; }
    }
    if (jname) env->DeleteLocalRef(jname);
    env->DeleteLocalRef(clCls);
    env->DeleteLocalRef(cl);
    return cls;
}

static void CacheJava(JNIEnv* env) {
    if (g_hooksCls) return;
    jclass local = LoadAppClass(env, "com.starry.magic.cocos.YsjdohHooks");
    if (!local) local = env->FindClass("com/starry/magic/cocos/YsjdohHooks");
    if (!local) { env->ExceptionClear(); return; }
    g_hooksCls = (jclass)env->NewGlobalRef(local);
    g_mSetFeature  = env->GetStaticMethodID(g_hooksCls, "setFeature",  "(IZ)V");
    g_mSyncFeature = env->GetStaticMethodID(g_hooksCls, "syncFeature", "(IZ)V");
    g_mSetValue    = env->GetStaticMethodID(g_hooksCls, "setValue",    "(II)V");
    g_mSyncValue   = env->GetStaticMethodID(g_hooksCls, "syncValue",   "(II)V");
    g_mWriteCfg    = env->GetStaticMethodID(g_hooksCls, "writeCfg",    "()V");
    env->DeleteLocalRef(local);
    if (env->ExceptionCheck()) env->ExceptionClear();
    YSJ_LOG("hooks class cached: %p", (void*)g_hooksCls);
}

bool Ysjdoh_TryInit(JavaVM* vm) {
    g_jvm = vm;
    g_ready = true;
    if (g_startMs == 0) g_startMs = NowMs();
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (detach) g_jvm->DetachCurrentThread();
    }
    YSJ_LOG("tryInit ready (no native hook; js patch in index.jsc)");
    return true;
}

bool Ysjdoh_IsReady() { return g_ready.load(); }

void Ysjdoh_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == YSJDOH_FEAT_INV) g_inv = enabled;
    else if (featNum == YSJDOH_FEAT_SPD) g_spd = enabled;
    else if (featNum == YSJDOH_FEAT_NOAD) g_noad = enabled;
    else if (featNum == YSJDOH_FEAT_MULT) g_mult = enabled ? 2 : 1;
    else return;

    long t = NowMs();
    bool early = (g_startMs && (t - g_startMs) < 5000);
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls) {
        if (featNum == YSJDOH_FEAT_MULT) {
            if (early) env->CallStaticVoidMethod(g_hooksCls, g_mSyncValue, featNum, (jint)g_mult.load());
            else       env->CallStaticVoidMethod(g_hooksCls, g_mSetValue,  featNum, (jint)g_mult.load());
        } else {
            if (early) env->CallStaticVoidMethod(g_hooksCls, g_mSyncFeature, featNum, (jboolean)enabled);
            else       env->CallStaticVoidMethod(g_hooksCls, g_mSetFeature,  featNum, (jboolean)enabled);
        }
        if (env->ExceptionCheck()) env->ExceptionClear();
        g_lastWriteMs = t;
    }
    if (detach) g_jvm->DetachCurrentThread();
}

void Ysjdoh_OnValueChanged(int featNum, int value) {
    if (featNum != YSJDOH_FEAT_MULT) return;
    if (value < 1) value = 1;
    if (value > 100) value = 100;
    g_mult = value;

    long t = NowMs();
    bool early = (g_startMs && (t - g_startMs) < 5000);
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls) {
        if (early) env->CallStaticVoidMethod(g_hooksCls, g_mSyncValue, featNum, (jint)value);
        else       env->CallStaticVoidMethod(g_hooksCls, g_mSetValue,  featNum, (jint)value);
        if (env->ExceptionCheck()) env->ExceptionClear();
        g_lastWriteMs = t;
    }
    if (detach) g_jvm->DetachCurrentThread();
}

void Ysjdoh_Tick(const bool* features, const int* values) {
    /* 只做兜底：jsb.sqlite 可能晚于本进程创建（Cocos 首次 setItem 才建表），
       所以每 2s 补写一次，保证游戏内 3s 轮询能读到最新状态。 */
    long t = NowMs();
    if (t - g_lastWriteMs < 2000) return;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls && g_mWriteCfg) {
        if (features) {
            g_inv = features[YSJDOH_FEAT_INV];
            g_spd = features[YSJDOH_FEAT_SPD];
            g_noad = features[YSJDOH_FEAT_NOAD];
        }
        if (values) {
            int v = values[YSJDOH_FEAT_MULT];
            if (v >= 1 && v <= 100) g_mult = v;
        }
        env->CallStaticVoidMethod(g_hooksCls, g_mWriteCfg);
        if (env->ExceptionCheck()) env->ExceptionClear();
        g_lastWriteMs = t;
    }
    if (detach) g_jvm->DetachCurrentThread();
}

void Ysjdoh_ForceClear() {
    g_inv = false; g_spd = false; g_noad = false; g_mult = 1;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls && g_mWriteCfg) {
        env->CallStaticVoidMethod(g_hooksCls, g_mWriteCfg);
        if (env->ExceptionCheck()) env->ExceptionClear();
    }
    if (detach) g_jvm->DetachCurrentThread();
}
