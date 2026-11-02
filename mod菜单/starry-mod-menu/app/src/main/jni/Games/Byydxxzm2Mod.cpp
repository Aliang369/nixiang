/* ================================================================
 *  不一样的修仙宗门2 — Cocos Creator 2.x（libcocos2djs.so）
 *
 *  机制：Dobby hook se::ScriptEngine::evalString(...)，识别游戏主脚本
 *        （project.js：含 qingyunzhi / donghuaFuhao / cc._RF.push），
 *        在末尾追加补丁 JS，暴露 window.__sby。
 *        菜单开关 -> JNI -> Java Byydxxzm2Hooks.setFeature -> CocosJsBridge 实时下发。
 *
 *  游戏侧事实（2026-11-02 实测，见 10_docs/不一样的修仙宗门2/）：
 *    - 存档对象 window.qingyunzhi；货币 qingyunzhi.base.gold / .diamond
 *    - 广告开关 window.donghuaFuhao（true=真实播放；false 时 TdFuzhu.kaishiDonghua 直接回调成功）
 * ================================================================ */
#include "Byydxxzm2Mod.h"

#include <atomic>
#include <cstring>
#include <string>
#include <dlfcn.h>
#include <pthread.h>
#include <unistd.h>
#include <time.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"

#define SBY_LOG "[byydxxzm2]"

static const char* kTargetLib() { return "libcocos2djs.so"; }
static const char* kEvalSym() {
    return "_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_";
}

/* 注入的补丁脚本（幂等；可被 Java 反复下发） */
static const char* kInitJs =
    "\n;(function(){try{if(window.__sby)return;\n"
    "var S={yu:false,ling:false,noad:false};\n"
    "S.patch=function(){try{var q=window.qingyunzhi;if(!q||!q.base)return;var b=q.base;if(b.__sbyBase)return;\n"
    "var g=b.gold,d=b.diamond;\n"
    "Object.defineProperty(b,'gold',{configurable:true,enumerable:true,get:function(){return S.ling?999999999:g;},set:function(v){g=v;}});\n"
    "Object.defineProperty(b,'diamond',{configurable:true,enumerable:true,get:function(){return S.yu?999999999:d;},set:function(v){d=v;}});\n"
    "b.__sbyBase=1;}catch(e){}};\n"
    "S.apply=function(){try{window.donghuaFuhao=!S.noad;}catch(e){}S.patch();};\n"
    "window.__sby=S;\n"
    "try{if(window.cc&&cc.director&&cc.director.getScheduler){cc.director.getScheduler().schedule(function(){S.patch();},null,3,cc.macro.REPEAT_FOREVER,0,false);}}catch(e){}\n"
    "S.apply();\n"
    "}catch(e){}})();\n";

static std::atomic<bool> g_feat[BYYDXXZM2_FEAT_COUNT];
static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_hooked{false};
static std::atomic<int>  g_spliceHits{0};
static thread_local bool  t_inSplice = false;
static JavaVM* g_jvm = nullptr;
static jclass  g_hooksCls = nullptr;
static jmethodID g_syncFeature = nullptr;
static jmethodID g_setFeatureW = nullptr;

static long NowMs() {
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return (long)ts.tv_sec * 1000L + ts.tv_nsec / 1000000L;
}
static std::atomic<long> g_lastSyncMs{0};

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
    jclass local = LoadAppClass(env, "com.starry.magic.cocos.Byydxxzm2Hooks");
    if (!local) local = env->FindClass("com/starry/magic/cocos/Byydxxzm2Hooks");
    if (local) {
        g_hooksCls = (jclass)env->NewGlobalRef(local);
        g_syncFeature = env->GetStaticMethodID(g_hooksCls, "syncFeature", "(IZ)V");
        g_setFeatureW = env->GetStaticMethodID(g_hooksCls, "setFeature", "(IZ)V");
        env->DeleteLocalRef(local);
        if (env->ExceptionCheck()) env->ExceptionClear();
    } else {
        env->ExceptionClear();
    }
}

static void SyncJava(bool force) {
    long now = NowMs();
    if (!force) {
        long last = g_lastSyncMs.load();
        if (now - last < 5000) return;
    }
    g_lastSyncMs.store(now);
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls && g_syncFeature) {
        for (int i = 1; i < BYYDXXZM2_FEAT_COUNT; i++) {
            env->CallStaticVoidMethod(g_hooksCls, g_syncFeature, i,
                                      g_feat[i].load() ? JNI_TRUE : JNI_FALSE);
        }
    }
    if (env->ExceptionCheck()) env->ExceptionClear();
    if (detach) g_jvm->DetachCurrentThread();
}

static bool MemHas(const char* buf, size_t len, const char* needle) {
    size_t n = strlen(needle);
    return buf && n > 0 && len >= n && memmem(buf, len, needle, n) != nullptr;
}

typedef bool (*SeEvalString_t)(void*, const char*, long, void*, const char*);
static SeEvalString_t g_origEval = nullptr;

static bool HookedEval(void* thiz, const char* script, long length, void* ret, const char* fileName) {
    if (!g_origEval) return false;
    if (!script || t_inSplice || length < 0) return g_origEval(thiz, script, length, ret, fileName);
    const size_t n = (size_t)length;
    if (n < 256u * 1024u || n > 32u * 1024u * 1024u) return g_origEval(thiz, script, length, ret, fileName);
    if (!MemHas(script, n, "qingyunzhi") || !MemHas(script, n, "donghuaFuhao") ||
        !MemHas(script, n, "cc._RF.push") || MemHas(script, n, "__sby")) {
        return g_origEval(thiz, script, length, ret, fileName);
    }
    try {
        std::string patched(script, n);
        patched.reserve(n + 4096);
        patched += kInitJs;
        LOGI("%s splice #%d len %zu->%zu file=%s", SBY_LOG, ++g_spliceHits, n, patched.size(),
             fileName ? fileName : "(null)");
        t_inSplice = true;
        bool ok = g_origEval(thiz, patched.c_str(), (long)patched.size(), ret, fileName);
        t_inSplice = false;
        if (ok) SyncJava(true);
        return ok;
    } catch (...) {
        t_inSplice = false;
        LOGW("%s splice exception, fallback", SBY_LOG);
        return g_origEval(thiz, script, length, ret, fileName);
    }
}

static bool TryHookEval() {
    if (g_hooked.load()) return true;
    if (!isLibraryLoaded(kTargetLib())) return false;
    void* handle = dlopen(kTargetLib(), RTLD_NOW);
    if (!handle) { LOGW("%s dlopen failed", SBY_LOG); return false; }
    void* sym = dlsym(handle, kEvalSym());
    if (!sym) { LOGW("%s dlsym evalString missing", SBY_LOG); dlclose(handle); return false; }
    if (DobbyHook(sym, (dobby_dummy_func_t)HookedEval, (dobby_dummy_func_t*)&g_origEval) != 0) {
        LOGW("%s DobbyHook failed", SBY_LOG);
        dlclose(handle);
        return false;
    }
    g_hooked.store(true);
    LOGI("%s hooked se::evalString @%p", SBY_LOG, sym);
    return true;
}

static void* EarlyHookThread(void* arg) {
    g_jvm = (JavaVM*)arg;
    for (int i = 0; i < 3000; i++) {
        if (TryHookEval()) {
            g_ready.store(true);
            SyncJava(true);
            LOGI("%s early hook ready iter=%d", SBY_LOG, i);
            return nullptr;
        }
        usleep(20 * 1000);
    }
    LOGW("%s early hook timeout", SBY_LOG);
    return nullptr;
}

void Byydxxzm2_StartEarlyHookThread(JavaVM* vm) {
    static std::atomic_bool started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, vm) == 0) pthread_detach(th);
    else started.store(false);
}

bool Byydxxzm2_TryInit(JavaVM* vm) {
    if (vm) g_jvm = vm;
    // 雷电 houdini 转译下 Dobby inline hook 必崩（2026-11-02 实测 SIGSEGV@libhoudini），
    // 改为纯 Java 侧 eval 注入；Getenv STARRY_SBY_HOOK=1 时才装 hook（真机调试用）。
    if (getenv("STARRY_SBY_HOOK")) { if (!TryHookEval()) return false; }
    g_ready.store(true);
    SyncJava(true);
    return true;
}

bool Byydxxzm2_IsReady() { return g_ready.load(); }

void Byydxxzm2_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum > 0 && featNum < BYYDXXZM2_FEAT_COUNT) {
        g_feat[featNum].store(enabled);
        LOGI("%s feature %d = %d", SBY_LOG, featNum, (int)enabled);
    }
    // 菜单点击：直接落库（setFeature -> jsb.sqlite），游戏内 3s 轮询生效
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_setFeatureW) {
            env->CallStaticVoidMethod(g_hooksCls, g_setFeatureW, featNum,
                                      enabled ? JNI_TRUE : JNI_FALSE);
        }
        if (env->ExceptionCheck()) env->ExceptionClear();
        if (detach) g_jvm->DetachCurrentThread();
    }
    SyncJava(true);
}

void Byydxxzm2_OnValueChanged(int featNum, int value) { (void)featNum; (void)value; }

void Byydxxzm2_Tick(const bool* features, const int* values) {
    (void)values;
    if (features) {
        for (int i = 1; i < BYYDXXZM2_FEAT_COUNT; i++) g_feat[i].store(features[i]);
    }
    SyncJava(false);
}

void Byydxxzm2_ForceClear() {
    for (int i = 0; i < BYYDXXZM2_FEAT_COUNT; i++) g_feat[i].store(false);
    SyncJava(true);
}
