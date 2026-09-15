/* ================================================================
 *  黑暗领域 MOD — Cocos Creator 3.8.7 (libcocos.so)
 *
 *  v2 四项功能完整重写：
 *  1) 免广告：查询、弹窗、统一奖励入口全部覆盖；
 *  2) 不羁怒火：主角长时无敌，开/关都能正确恢复；
 *  3) 先祖之魂：自动召唤高继承攻击分身，限制分身堆积；
 *  4) 雷罚打击：主角每次普攻稳定触发，单点异常只隔离本功能。
 *
 *  同时移除一次性的 JS installed 锁，热更模块晚到时允许幂等补装。
 * ================================================================ */

#include "HalyMod.h"
#include "HalyPatchJs.h"
#include "Includes/obfuscate.h"

#include <atomic>
#include <cstdio>
#include <cstring>

#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"

static JavaVM* g_jvm = nullptr;
static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_noAd{false};
static std::atomic<bool> g_nuhuo{false};
static std::atomic<bool> g_xianzu{false};
static std::atomic<bool> g_leifa{false};

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

static bool EvalJs(const char* js) {
    if (!g_jvm || !js || !*js) return false;

    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return false;

    bool ok = false;
    jclass cls = env->FindClass(OBFUSCATE("com/starry/magic/cocos/CocosJsBridge"));
    if (!cls) {
        if (env->ExceptionCheck()) env->ExceptionClear();
    } else {
        jmethodID mid = env->GetStaticMethodID(cls, "evalOnGLThread", "(Ljava/lang/String;)V");
        if (!mid) {
            if (env->ExceptionCheck()) env->ExceptionClear();
        } else {
            jstring jjs = env->NewStringUTF(js);
            if (jjs) {
                env->CallStaticVoidMethod(cls, mid, jjs);
                jthrowable ex = env->ExceptionOccurred();
                if (ex) {
                    env->ExceptionDescribe();
                    env->ExceptionClear();
                    env->DeleteLocalRef(ex);
                } else {
                    ok = true;
                }
                env->DeleteLocalRef(jjs);
            }
        }
        env->DeleteLocalRef(cls);
    }

    if (detach) g_jvm->DetachCurrentThread();
    return ok;
}

static bool SyncFlagsJs() {
    char flags[384];
    snprintf(flags, sizeof(flags),
             "(function(){var g=(typeof globalThis!==\"undefined\")?globalThis:"
             "((typeof window!==\"undefined\")?window:this);"
             "if(typeof g.__starryHalyV2SetFlags===\"function\"){"
             "g.__starryHalyV2SetFlags(%s,%s,%s,%s);"
             "}})();",
             g_noAd.load() ? "true" : "false",
             g_nuhuo.load() ? "true" : "false",
             g_xianzu.load() ? "true" : "false",
             g_leifa.load() ? "true" : "false");

    bool installed = EvalJs(kHalyInstallJs);
    bool synced = EvalJs(flags);
    if (!installed || !synced) {
        LOGW("[Haly] JS sync deferred (install=%d flags=%d)", installed ? 1 : 0, synced ? 1 : 0);
    }
    return installed && synced;
}

static void ClearJs() {
    static const char* clearJs =
        "(function(){var g=(typeof globalThis!==\"undefined\")?globalThis:"
        "((typeof window!==\"undefined\")?window:this);"
        "if(typeof g.__starryHalyV2Clear===\"function\")g.__starryHalyV2Clear();"
        "})();";
    EvalJs(clearJs);
}

bool Haly_TryInit(JavaVM* jvm) {
    g_jvm = jvm;
    if (!isLibraryLoaded(GameCocos::TARGET_LIB)) return false;
    g_ready.store(true);
    SyncFlagsJs();
    LOGI("[Haly] ready (libcocos + JS bridge) build=starry_haly_v2");
    return true;
}

bool Haly_IsReady() {
    return g_ready.load();
}

void Haly_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case HALY_FEAT_NO_AD: g_noAd.store(enabled); break;
        case HALY_FEAT_NUHUO: g_nuhuo.store(enabled); break;
        case HALY_FEAT_XIANZU: g_xianzu.store(enabled); break;
        case HALY_FEAT_LEIFA: g_leifa.store(enabled); break;
        default: break;
    }
    if (g_ready.load()) SyncFlagsJs();
}

void Haly_Tick(const bool* features) {
    if (!g_ready.load() || !features) return;

    bool n0 = features[HALY_FEAT_NO_AD];
    bool n1 = features[HALY_FEAT_NUHUO];
    bool n2 = features[HALY_FEAT_XIANZU];
    bool n3 = features[HALY_FEAT_LEIFA];
    bool changed = n0 != g_noAd.load() || n1 != g_nuhuo.load() ||
                   n2 != g_xianzu.load() || n3 != g_leifa.load();
    if (changed) {
        g_noAd.store(n0);
        g_nuhuo.store(n1);
        g_xianzu.store(n2);
        g_leifa.store(n3);
        SyncFlagsJs();
        return;
    }

    // JS 热更或页面切换后模块可能重建；保持幂等补装，不在 native 侧永久封死。
    static int tick = 0;
    if ((++tick % 10) == 0) EvalJs(kHalyInstallJs);
}

void Haly_ForceClear() {
    g_noAd.store(false);
    g_nuhuo.store(false);
    g_xianzu.store(false);
    g_leifa.store(false);
    if (g_ready.load()) ClearJs();
}
