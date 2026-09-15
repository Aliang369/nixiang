/* ================================================================
 *  勇士之心 MOD - se::ScriptEngine::evalString splice
 *
 *  目标：TapTap com.fightingstars.games.warrior 1.0.3 / vc5
 *  引擎：Cocos Creator 2.4.15 / libcocos2djs.so
 *
 *  本包与斌哥包的核心资产一致：
 *    - libcocos2djs.so / assets/main.js / assets/src/cocos2d-jsb.js
 *    - assets/assets/main/index.js
 *  因此直接复用斌哥自愈式 JS 注入体，只把 MOD.featureN 的来源替换为
 *  Starry YsxzHooks.getRet(n)。
 *
 *  se::ScriptEngine::evalString 在本包被 strip，按静态 RVA 0x3138e4 定位，
 *  序言校验 0xD10543FF；不匹配则拒绝挂载并等待后续 Tick 重试。
 * ================================================================ */

#include "YsxzMod.h"
#include "YsxzPatchJs.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>
#include <string>
#include <pthread.h>
#include <unistd.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"

#define YSXZ_LOG "[Ysxz]"

static const char* kTargetLib() { return sec_lib_cocosjs(); }

static constexpr uintptr_t kEvalRva = 0x3138e4;
static constexpr uint32_t kEvalInsn = 0xD10543FFu;
static constexpr long kBundleMinLen = 200L * 1024L;
static constexpr long kBundleMaxLen = 64L * 1024L * 1024L;

static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_hooked{false};
static std::atomic<int> g_spliceHits{0};
static std::atomic<bool> g_feat[YSXZ_FEAT_COUNT];
static std::atomic<int> g_val[YSXZ_FEAT_COUNT];
static thread_local bool t_inSplice = false;

typedef bool (*SeEvalString_t)(void*, const char*, long, void*, const char*);
static SeEvalString_t g_origSeEval = nullptr;

static bool FindBytes(const char* buf, size_t len, const char* needle) {
    if (!buf || !needle) return false;
    const size_t n = strlen(needle);
    if (n == 0 || len < n) return false;
    for (size_t i = 0; i + n <= len; i++) {
        if (memcmp(buf + i, needle, n) == 0) return true;
    }
    return false;
}

static bool IsLibMapLine(const char* line, const char* lib) {
    return line && lib && strstr(line, lib) && strstr(line, ".so");
}

static bool IsTargetBundle(const char* fileName) {
    return fileName && (strstr(fileName, "assets/battleLogic/index.") ||
                        strstr(fileName, "assets/game/index."));
}

static void* FileOffToVa(const char* lib, uintptr_t fileOff) {
    FILE* fp = fopen("/proc/self/maps", "rt");
    if (!fp) return nullptr;
    char line[768];
    void* found = nullptr;
    size_t bestSpan = 0;
    while (fgets(line, sizeof(line), fp)) {
        if (!IsLibMapLine(line, lib)) continue;
        unsigned long start = 0, end = 0, off = 0;
        char perm[8] = {0};
        if (sscanf(line, "%lx-%lx %7s %lx", &start, &end, perm, &off) != 4) continue;
        if (!strchr(perm, 'x') || end <= start) continue;
        if (fileOff < off || fileOff - off >= static_cast<uintptr_t>(end - start)) continue;
        const size_t span = static_cast<size_t>(end - start);
        if (span > bestSpan) {
            bestSpan = span;
            found = reinterpret_cast<void*>(start + (fileOff - off));
        }
    }
    fclose(fp);
    return found;
}

static bool HookedEvalString(void* thiz, const char* script, long length, void* ret,
                             const char* fileName);

static bool HookEvalString() {
    if (g_hooked.load()) return true;
    const char* lib = kTargetLib();
    if (!isLibraryLoaded(lib)) return false;

    void* va = FileOffToVa(lib, kEvalRva);
    if (!va) {
        LOGW("%s evalString rva 0x%lX not mapped", YSXZ_LOG, (unsigned long)kEvalRva);
        return false;
    }
    uint32_t got = 0;
    memcpy(&got, va, sizeof(got));
    if (got != kEvalInsn) {
        LOGW("%s evalString prologue drift got=0x%08X want=0x%08X", YSXZ_LOG, got, kEvalInsn);
        return false;
    }
    if (DobbyHook(va, (dobby_dummy_func_t)HookedEvalString, (dobby_dummy_func_t*)&g_origSeEval) != 0) {
        LOGW("%s DobbyHook evalString failed @%p", YSXZ_LOG, va);
        return false;
    }
    g_hooked.store(true);
    LOGI("%s hooked se::evalString @%p rva=0x%lX", YSXZ_LOG, va, (unsigned long)kEvalRva);
    return true;
}

static bool PatchRegistryCall(std::string* src) {
    const std::string begin = "\n}({\n";
    const std::string beginNew = "\n}(window.__starry_capture({\n";
    const std::string end = "\n}, {}, [";
    const std::string endNew = "\n}), {}, [";
    const size_t p = src->find(begin);
    if (p == std::string::npos) return false;
    src->replace(p, begin.size(), beginNew);
    const size_t q = src->find(end, p + beginNew.size());
    if (q == std::string::npos) return false;
    src->replace(q, end.size(), endNew);
    return true;
}

static bool HookedEvalString(void* thiz, const char* script, long length, void* ret,
                             const char* fileName) {
    if (!g_origSeEval) return false;
    if (!script || t_inSplice) return g_origSeEval(thiz, script, length, ret, fileName);

    long n = length;
    if (n < 0) n = (long)strlen(script);
    if (n < kBundleMinLen || n > kBundleMaxLen) {
        return g_origSeEval(thiz, script, length, ret, fileName);
    }
    if (!IsTargetBundle(fileName)) {
        return g_origSeEval(thiz, script, length, ret, fileName);
    }
    if (!FindBytes(script, (size_t)n, "window.__require") ||
        (!FindBytes(script, (size_t)n, "maxApDefRate") &&
         !FindBytes(script, (size_t)n, "AUTO_BATTLE")) ||
        FindBytes(script, (size_t)n, "starry_ysxz_v1")) {
        return g_origSeEval(thiz, script, length, ret, fileName);
    }

    std::string patched;
    try {
        std::string source(script, (size_t)n);
        if (!PatchRegistryCall(&source)) {
            LOGW("%s registry call pattern missing in bundle=%s", YSXZ_LOG, fileName);
            return g_origSeEval(thiz, script, length, ret, fileName);
        }
        patched.reserve(source.size() + strlen(kYsxzPatchJs) + 64);
        patched.assign(kYsxzPatchJs, strlen(kYsxzPatchJs));
        patched += "\n";
        patched += source;
    } catch (...) {
        LOGW("%s patch build exception; fallback original script", YSXZ_LOG);
        return g_origSeEval(thiz, script, length, ret, fileName);
    }

    int hits = ++g_spliceHits;
    LOGI("%s logic splice #%d len %ld->%zu file=%s", YSXZ_LOG, hits, n, patched.size(),
         fileName ? fileName : "(null)");

    t_inSplice = true;
    bool ok = g_origSeEval(thiz, patched.c_str(), (long)patched.size(), ret, fileName);
    t_inSplice = false;
    return ok;
}

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
    if (!atCls) {
        env->ExceptionClear();
        return nullptr;
    }
    jmethodID curApp = env->GetStaticMethodID(
        atCls, "currentApplication", "()Landroid/app/Application;");
    if (!curApp) {
        env->ExceptionClear();
        env->DeleteLocalRef(atCls);
        return nullptr;
    }
    jobject app = env->CallStaticObjectMethod(atCls, curApp);
    env->DeleteLocalRef(atCls);
    if (!app) {
        env->ExceptionClear();
        return nullptr;
    }
    jclass appCls = env->GetObjectClass(app);
    jmethodID getCl = env->GetMethodID(appCls, "getClassLoader", "()Ljava/lang/ClassLoader;");
    jobject cl = getCl ? env->CallObjectMethod(app, getCl) : nullptr;
    env->DeleteLocalRef(appCls);
    env->DeleteLocalRef(app);
    if (!cl) {
        env->ExceptionClear();
        return nullptr;
    }
    jclass clCls = env->GetObjectClass(cl);
    jmethodID loadClass =
        env->GetMethodID(clCls, "loadClass", "(Ljava/lang/String;)Ljava/lang/Class;");
    jstring jname = env->NewStringUTF(name);
    jclass cls = nullptr;
    if (loadClass && jname) {
        cls = (jclass)env->CallObjectMethod(cl, loadClass, jname);
        if (env->ExceptionCheck()) {
            env->ExceptionClear();
            cls = nullptr;
        }
    }
    if (jname) env->DeleteLocalRef(jname);
    env->DeleteLocalRef(clCls);
    env->DeleteLocalRef(cl);
    return cls;
}

static void CacheJava(JNIEnv* env) {
    if (g_hooksCls) return;
    jclass local = LoadAppClass(env, "com.starry.magic.cocos.YsxzHooks");
    if (!local) local = env->FindClass("com/starry/magic/cocos/YsxzHooks");
    if (local) {
        g_hooksCls = (jclass)env->NewGlobalRef(local);
        g_setRet = env->GetStaticMethodID(g_hooksCls, "setRet", "(IZ)V");
        g_setVal = env->GetStaticMethodID(g_hooksCls, "setVal", "(II)V");
        g_clearAll = env->GetStaticMethodID(g_hooksCls, "clearAll", "()V");
        env->DeleteLocalRef(local);
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
        for (int i = 0; i < YSXZ_FEAT_COUNT; i++) {
            env->CallStaticVoidMethod(g_hooksCls, g_setRet, i,
                                      g_feat[i].load() ? JNI_TRUE : JNI_FALSE);
            if (g_setVal) env->CallStaticVoidMethod(g_hooksCls, g_setVal, i, g_val[i].load());
        }
    }
    if (detach) g_jvm->DetachCurrentThread();
}

static void* EarlyHookThread(void* arg) {
    g_jvm = (JavaVM*)arg;
    for (int i = 0; i < 3000; i++) {
        if (isLibraryLoaded(kTargetLib())) {
            g_libReady.store(true);
            if (HookEvalString()) {
                SyncJava();
                LOGI("%s early hook ok iter=%d", YSXZ_LOG, i);
                return nullptr;
            }
        }
        usleep(20 * 1000);
    }
    LOGW("%s early hook timeout", YSXZ_LOG);
    return nullptr;
}

void Ysxz_OnLoad(JavaVM* vm) {
    g_jvm = vm;
    static std::atomic<bool> started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, vm) == 0) {
        pthread_detach(th);
    } else {
        started.store(false);
    }
}

bool Ysxz_TryInit(JavaVM* vm) {
    g_jvm = vm;
    if (!isLibraryLoaded(kTargetLib())) return false;
    g_libReady.store(true);
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (detach) g_jvm->DetachCurrentThread();
    }
    if (!HookEvalString()) return false;
    SyncJava();
    return true;
}

bool Ysxz_IsReady() { return g_hooked.load(); }

void Ysxz_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum >= 0 && featNum < YSXZ_FEAT_COUNT) g_feat[featNum].store(enabled);
    SyncJava();
    LOGI("%s feature #%d=%s", YSXZ_LOG, featNum, enabled ? "ON" : "OFF");
}

void Ysxz_OnValueChanged(int featNum, int value) {
    if (featNum >= 0 && featNum < YSXZ_FEAT_COUNT) g_val[featNum].store(value);
    SyncJava();
}

void Ysxz_Tick(const bool* features, const int* values) {
    if (features) {
        for (int i = 0; i < YSXZ_FEAT_COUNT; i++) g_feat[i].store(features[i]);
    }
    if (values) {
        for (int i = 0; i < YSXZ_FEAT_COUNT; i++) g_val[i].store(values[i]);
    }
    if (!g_hooked.load() && isLibraryLoaded(kTargetLib())) {
        g_libReady.store(true);
        HookEvalString();
    }
    SyncJava();
}

void Ysxz_ForceClear() {
    for (int i = 0; i < YSXZ_FEAT_COUNT; i++) {
        g_feat[i].store(false);
        g_val[i].store(0);
    }
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_clearAll) env->CallStaticVoidMethod(g_hooksCls, g_clearAll);
        if (detach) g_jvm->DetachCurrentThread();
    }
}
