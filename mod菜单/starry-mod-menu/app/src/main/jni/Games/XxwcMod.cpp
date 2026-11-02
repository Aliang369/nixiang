/* ================================================================
 *  XxwcMod — 修仙王朝模拟器 1.0.4（Cocos Creator 3.8.7）
 *
 *  通道：com.starry.magic.cocos.CocosJsBridge.evalOnGLThread(String)
 *        （框架自带；JNI_OnLoad 内缓存 global ref，避免 native 线程 FindClass 失败）
 *  补丁：Games/XxwcPatch.js（内嵌 kXxwcInstallJs，幂等，含 3s 自愈定时器）
 *  开关：JS 侧 window.__starryXxwc.setFlags(noAd, inv)
 * ================================================================ */
#include "XxwcMod.h"
#include "XxwcPatchJs.h"

#include <atomic>
#include <cstdio>
#include <cstring>

#include "Includes/Logger.h"
#include "Includes/Utils.hpp"

#define XXWC_LOG "[xxwc]"

static JavaVM*    g_jvm      = nullptr;
static jclass     g_bridge   = nullptr;
static jmethodID  g_evalMid  = nullptr;
static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_noAd{false};
static std::atomic<bool> g_inv{false};

void Xxwc_OnLoad(JavaVM* vm) {
    if (vm) g_jvm = vm;
    if (g_bridge) return;
    if (!vm) { LOGW(XXWC_LOG " onLoad: vm null"); return; }
    JNIEnv* env = nullptr;
    if (vm->GetEnv((void**)&env, JNI_VERSION_1_6) != JNI_OK || !env) {
        LOGW(XXWC_LOG " onLoad: no env");
        return;
    }
    jclass c = env->FindClass("com/starry/magic/cocos/CocosJsBridge");
    if (!c) {
        env->ExceptionClear();
        LOGW(XXWC_LOG " onLoad: FindClass CocosJsBridge fail");
        return;
    }
    jmethodID m = env->GetStaticMethodID(c, "evalOnGLThread", "(Ljava/lang/String;)V");
    if (!m) {
        env->ExceptionClear();
        env->DeleteLocalRef(c);
        LOGW(XXWC_LOG " onLoad: evalOnGLThread id fail");
        return;
    }
    g_bridge  = (jclass)env->NewGlobalRef(c);
    g_evalMid = m;
    env->DeleteLocalRef(c);
    LOGI(XXWC_LOG " onLoad: CocosJsBridge cached");
}

static bool EvalJs(const char* js) {
    if (!g_jvm || !js || !*js) return false;
    bool detach = false;
    JNIEnv* env = nullptr;
    int st = g_jvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_jvm->AttachCurrentThread(&env, nullptr) != 0) return false;
        detach = true;
    } else if (st != JNI_OK || !env) {
        return false;
    }

    jclass cls = g_bridge;
    jmethodID mid = g_evalMid;
    bool local = false;
    if (!cls || !mid) {
        jclass lc = env->FindClass("com/starry/magic/cocos/CocosJsBridge");
        if (!lc) { env->ExceptionClear(); if (detach) g_jvm->DetachCurrentThread(); return false; }
        mid = env->GetStaticMethodID(lc, "evalOnGLThread", "(Ljava/lang/String;)V");
        if (!mid) { env->ExceptionClear(); env->DeleteLocalRef(lc); if (detach) g_jvm->DetachCurrentThread(); return false; }
        cls = lc; local = true;
    }

    bool ok = false;
    jstring jjs = env->NewStringUTF(js);
    if (jjs) {
        env->CallStaticVoidMethod(cls, mid, jjs);
        if (env->ExceptionCheck()) {
            env->ExceptionClear();
        } else {
            ok = true;
        }
        env->DeleteLocalRef(jjs);
    }
    if (local) env->DeleteLocalRef(cls);
    if (detach) g_jvm->DetachCurrentThread();
    return ok;
}

static bool SyncFlagsJs(bool forceInstall) {
    if (forceInstall) EvalJs(kXxwcInstallJs);

    char flags[320];
    snprintf(flags, sizeof(flags),
             "(function(){var g=(typeof globalThis!==\"undefined\")?globalThis:"
             "((typeof window!==\"undefined\")?window:this);"
             "if(g.__starryXxwc&&typeof g.__starryXxwc.setFlags===\"function\"){"
             "g.__starryXxwc.setFlags(%s,%s);"
             "}})();",
             g_noAd.load() ? "true" : "false",
             g_inv.load() ? "true" : "false");
    return EvalJs(flags);
}

bool Xxwc_TryInit(JavaVM* vm) {
    if (vm && !g_jvm) g_jvm = vm;
    if (!isLibraryLoaded("libcocos.so")) return false;
    g_ready.store(true);
    SyncFlagsJs(true);
    LOGI(XXWC_LOG " ready (libcocos + JS bridge)");
    return true;
}

bool Xxwc_IsReady() { return g_ready.load(); }

void Xxwc_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case XXWC_FEAT_NOAD: g_noAd.store(enabled); break;
        case XXWC_FEAT_INV:  g_inv.store(enabled);  break;
        default: return;
    }
    if (g_ready.load()) SyncFlagsJs(false);
    LOGI(XXWC_LOG " feat=%d -> %d", featNum, enabled ? 1 : 0);
}

void Xxwc_OnValueChanged(int /*featNum*/, int /*value*/) {}

void Xxwc_Tick(const bool* features, const int* /*values*/) {
    if (!g_ready.load() || !features) return;

    bool n0 = features[XXWC_FEAT_NOAD];
    bool n1 = features[XXWC_FEAT_INV];
    bool changed = (n0 != g_noAd.load()) || (n1 != g_inv.load());
    if (changed) {
        g_noAd.store(n0);
        g_inv.store(n1);
        SyncFlagsJs(false);
        return;
    }

    // 模块懒加载/切场景后可能重建：周期性幂等重装
    static int tick = 0;
    if ((++tick % 15) == 0) SyncFlagsJs(true);
}

void Xxwc_ForceClear() {
    g_noAd.store(false);
    g_inv.store(false);
    if (g_ready.load()) {
        EvalJs("(function(){var g=(typeof globalThis!==\"undefined\")?globalThis:"
               "((typeof window!==\"undefined\")?window:this);"
               "if(g.__starryXxwc&&typeof g.__starryXxwc.setFlags===\"function\")"
               "g.__starryXxwc.setFlags(false,false);})();");
    }
}
