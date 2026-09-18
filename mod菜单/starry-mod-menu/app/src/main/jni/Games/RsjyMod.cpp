/* ================================================================
 *  燃烧纪元 (com.rsjy.yf) — Cocos2d-x JS 模块
 *
 *  注入通道：
 *    Dobby hook se::ScriptEngine::evalString（libcocos2djs.so 导出符号）
 *    主 browserify bundle 末尾追加 JS 补丁，补丁在模块执行后包裹：
 *      Unit.prototype.changeHp       — 攻击倍率 / 上帝模式
 *      *BattlePanel.getBattleSpeed   — 战斗速度 ×1..50
 *      AdHelper.prototype.showVideoAd— 免广告
 *
 * ================================================================ */

#include "RsjyMod.h"

#include <atomic>
#include <cstring>
#include <dlfcn.h>
#include <pthread.h>
#include <string>
#include <unistd.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/SecStrings.hpp"
#include "Includes/Utils.hpp"
#include "RsjyPatchJs.h"

#define RSJY_LOG "[rsjy]"

static const char* kTargetLib() { return sec_lib_cocosjs(); }
static const char* kSeEvalSym() {
    return "_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_";
}

static std::atomic<bool> g_feat[RSJY_FEAT_COUNT];
static std::atomic<int> g_killValue{RSJY_KILL_DEFAULT};
static std::atomic<int> g_speedValue{RSJY_SPEED_DEFAULT};
static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_seHooked{false};
static thread_local bool t_inSplice = false;

static JavaVM* g_jvm = nullptr;
static jclass g_hooksCls = nullptr;
static jmethodID g_setRet = nullptr;
static jmethodID g_setVal = nullptr;
static jmethodID g_clearAll = nullptr;

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
    jmethodID curApp = env->GetStaticMethodID(
        atCls, "currentApplication", "()Landroid/app/Application;");
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
    jmethodID loadClass = env->GetMethodID(
        clCls, "loadClass", "(Ljava/lang/String;)Ljava/lang/Class;");
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
    jclass local = LoadAppClass(env, "com.starry.magic.cocos.RsjyHooks");
    if (!local) local = env->FindClass("com/starry/magic/cocos/RsjyHooks");
    if (local) {
        g_hooksCls = (jclass)env->NewGlobalRef(local);
        g_setRet = env->GetStaticMethodID(g_hooksCls, "setRet", "(IZ)V");
        g_setVal = env->GetStaticMethodID(g_hooksCls, "setVal", "(II)V");
        g_clearAll = env->GetStaticMethodID(g_hooksCls, "clearAll", "()V");
        env->DeleteLocalRef(local);
        if (env->ExceptionCheck()) env->ExceptionClear();
    } else {
        env->ExceptionClear();
    }
}

static void SyncJava() {
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls && g_setRet) {
        for (int i = 1; i < RSJY_FEAT_COUNT; i++) {
            env->CallStaticVoidMethod(g_hooksCls, g_setRet, i,
                                      g_feat[i].load() ? JNI_TRUE : JNI_FALSE);
        }
    }
    if (g_hooksCls && g_setVal) {
        env->CallStaticVoidMethod(g_hooksCls, g_setVal,
                                  RSJY_FEAT_KILL, g_killValue.load());
        env->CallStaticVoidMethod(g_hooksCls, g_setVal,
                                  RSJY_FEAT_SPEED, g_speedValue.load());
    }
    if (env->ExceptionCheck()) env->ExceptionClear();
    if (detach) g_jvm->DetachCurrentThread();
}

static bool MemHas(const char* buf, size_t len, const char* needle) {
    if (!buf || !needle) return false;
    const size_t n = strlen(needle);
    return n > 0 && len >= n && memmem(buf, len, needle, n) != nullptr;
}

typedef bool (*SeEvalString_t)(void*, const char*, long, void*, const char*);
static SeEvalString_t g_origSeEval = nullptr;

static bool hooked_seEval(void* thiz, const char* script, long length, void* ret,
                          const char* fileName) {
    if (!g_origSeEval) return false;
    if (!script || t_inSplice || length <= 0) {
        return g_origSeEval(thiz, script, length, ret, fileName);
    }

    const size_t n = (size_t)length;
    if (n < 512u * 1024u || n > 64u * 1024u * 1024u) {
        return g_origSeEval(thiz, script, length, ret, fileName);
    }

    // 只处理游戏主 browserify bundle；其它脚本原样放行。
    if (!MemHas(script, n, "window.__require") ||
        !MemHas(script, n, "changeHp") ||
        !MemHas(script, n, "getBattleSpeed") ||
        MemHas(script, n, "starry_rsjy_v1")) {
        return g_origSeEval(thiz, script, length, ret, fileName);
    }

    try {
        std::string patched(script, n);
        patched.reserve(n + strlen(kPatchJs) + 16);
        patched += "\n;\n";
        patched += kPatchJs;
        LOGI("%s evalString splice len=%zu->%zu file=%s", RSJY_LOG,
             n, patched.size(), fileName ? fileName : "(null)");
        t_inSplice = true;
        const bool ok = g_origSeEval(thiz, patched.c_str(), (long)patched.size(), ret, fileName);
        t_inSplice = false;
        return ok;
    } catch (...) {
        t_inSplice = false;
        LOGW("%s splice exception; fallback original script", RSJY_LOG);
        return g_origSeEval(thiz, script, length, ret, fileName);
    }
}

static bool TryHookEvalString() {
    if (g_seHooked.load()) return true;
    const char* lib = kTargetLib();
    if (!isLibraryLoaded(lib)) return false;

    void* handle = dlopen(lib, RTLD_NOW);
    if (!handle) {
        LOGW("%s dlopen %s failed: %s", RSJY_LOG, lib, dlerror());
        return false;
    }
    void* sym = dlsym(handle, kSeEvalSym());
    if (!sym) {
        LOGW("%s se::evalString dlsym missing: %s", RSJY_LOG, dlerror());
        dlclose(handle);
        return false;
    }
    if (DobbyHook(sym, (dobby_dummy_func_t)hooked_seEval,
                  (dobby_dummy_func_t*)&g_origSeEval) != 0) {
        LOGW("%s DobbyHook se::evalString failed", RSJY_LOG);
        dlclose(handle);
        return false;
    }
    g_seHooked.store(true);
    LOGI("%s hooked se::evalString @%p", RSJY_LOG, sym);
    return true;
}

static void* EarlyHookThread(void* arg) {
    g_jvm = (JavaVM*)arg;
    for (int i = 0; i < 3000; i++) {
        if (isLibraryLoaded(kTargetLib()) && TryHookEvalString()) {
            g_ready.store(true);
            SyncJava();
            LOGI("%s early hook ready iter=%d", RSJY_LOG, i);
            return nullptr;
        }
        usleep(20 * 1000);
    }
    LOGW("%s early hook timeout", RSJY_LOG);
    return nullptr;
}

void Rsjy_StartEarlyHookThread(JavaVM* vm) {
    static std::atomic_bool started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, vm) == 0) {
        pthread_detach(th);
    } else {
        started.store(false);
    }
}

bool Rsjy_TryInit(JavaVM* vm) {
    if (vm) g_jvm = vm;
    if (!TryHookEvalString()) return false;
    g_ready.store(true);
    SyncJava();
    return true;
}

bool Rsjy_IsReady() { return g_ready.load(); }

void Rsjy_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum > 0 && featNum < RSJY_FEAT_COUNT) {
        g_feat[featNum].store(enabled);
        LOGI("%s feature #%d=%s", RSJY_LOG, featNum, enabled ? "ON" : "OFF");
    }
    SyncJava();
}

void Rsjy_OnValueChanged(int featNum, int value) {
    if (featNum == RSJY_FEAT_KILL) {
        if (value < RSJY_KILL_MIN) value = RSJY_KILL_MIN;
        if (value > RSJY_KILL_MAX) value = RSJY_KILL_MAX;
        g_killValue.store(value);
        LOGI("%s kill slider=%d", RSJY_LOG, value);
    } else if (featNum == RSJY_FEAT_SPEED) {
        if (value < RSJY_SPEED_MIN) value = RSJY_SPEED_MIN;
        if (value > RSJY_SPEED_MAX) value = RSJY_SPEED_MAX;
        g_speedValue.store(value);
        LOGI("%s speed slider=%d", RSJY_LOG, value);
    } else {
        return;
    }
    SyncJava();
}

void Rsjy_Tick(const bool* features, const int* values) {
    if (features) {
        for (int i = 1; i < RSJY_FEAT_COUNT; i++) {
            g_feat[i].store(features[i]);
        }
    }
    if (values) {
        int kill = values[RSJY_FEAT_KILL];
        if (kill >= RSJY_KILL_MIN && kill <= RSJY_KILL_MAX) {
            g_killValue.store(kill);
        }
        int speed = values[RSJY_FEAT_SPEED];
        if (speed >= RSJY_SPEED_MIN && speed <= RSJY_SPEED_MAX) {
            g_speedValue.store(speed);
        }
    }
    if (!g_seHooked.load()) TryHookEvalString();
    SyncJava();
}

void Rsjy_ForceClear() {
    for (int i = 1; i < RSJY_FEAT_COUNT; i++) g_feat[i].store(false);
    g_killValue.store(RSJY_KILL_DEFAULT);
    g_speedValue.store(RSJY_SPEED_DEFAULT);
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_clearAll) {
            env->CallStaticVoidMethod(g_hooksCls, g_clearAll);
        }
        if (g_hooksCls && g_setVal) {
            env->CallStaticVoidMethod(g_hooksCls, g_setVal,
                                      RSJY_FEAT_KILL, RSJY_KILL_DEFAULT);
            env->CallStaticVoidMethod(g_hooksCls, g_setVal,
                                      RSJY_FEAT_SPEED, RSJY_SPEED_DEFAULT);
        }
        if (env->ExceptionCheck()) env->ExceptionClear();
        if (detach) g_jvm->DetachCurrentThread();
    }
}
