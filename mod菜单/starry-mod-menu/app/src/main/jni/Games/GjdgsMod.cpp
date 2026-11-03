/* ================================================================
 *  GjdgsMod — 挂机打怪兽（org.cocos2d.guajidaguaishou.tap 1.1.65）
 *
 *  形态：零 native hook。功能全部落在游戏 JS 层（assets/main.js 追加的功能层），
 *        本文件只负责「菜单状态 → JNI → Java GjdgsHooks.setFeature/setValue」。
 *  依据：10_docs/_batch_binge6/三款新版菜单_抓点总表_20261103.md §四.2
 *        三款原版基线_与斌哥包差异_20261103.md §三.1（斌哥 JS 实现全文，已逐项对照）
 *
 *  ★ 为什么不做 hook：本游戏存档在 cc.sys.localStorage['data']（明文 JSON），
 *    改数值只需操作 JS 活对象，no libil2cpp、无 RVA、无 ABI 依赖；hook 反而增加崩溃面。
 * ================================================================ */
#include "GjdgsMod.h"

#include <android/log.h>
#include <atomic>
#include <dlfcn.h>
#include <jni.h>
#include <time.h>

#define LOG_TAG "StarryGjdgs"
#define LOGI(...) __android_log_print(ANDROID_LOG_INFO,  LOG_TAG, __VA_ARGS__)
#define LOGW(...) __android_log_print(ANDROID_LOG_WARN,  LOG_TAG, __VA_ARGS__)

// ---------------------------------------------------------------- 状态
static JavaVM* g_vm = nullptr;

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_god{false};
static std::atomic<int>  g_spd{1};
static std::atomic<int>  g_dmg{1};
static std::atomic<int>  g_gold{1000000};
static std::atomic<int>  g_gem{5000};
static std::atomic<int>  g_shard{5000};
static std::atomic<int>  g_atk{50};
static std::atomic<int>  g_hp{50};
static std::atomic<bool> g_noad{false};
static std::atomic<bool> g_bootSynced{false};

// ---------------------------------------------------------------- JNI 通道
static jclass    g_cls = nullptr;
static jmethodID g_mSetFeature = nullptr;
static jmethodID g_mSetValue   = nullptr;

static JNIEnv* GetEnv(bool* needDetach) {
    if (!g_vm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_vm->GetEnv(reinterpret_cast<void**>(&env), JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_vm->AttachCurrentThread(&env, nullptr) != JNI_OK) return nullptr;
        if (needDetach) *needDetach = true;
        return env;
    }
    if (st != JNI_OK) return nullptr;
    return env;
}

// 通过 ActivityThread.currentApplication().getClassLoader() 取类，
// 避免 FindClass 在非 Java 线程用 Bootstrap loader 找不到 app 类。
static jclass LoadAppClass(JNIEnv* env, const char* name) {
    jclass atCls = env->FindClass("android/app/ActivityThread");
    if (!atCls) { env->ExceptionClear(); return nullptr; }
    jmethodID curApp = env->GetStaticMethodID(atCls, "currentApplication",
                                              "()Landroid/app/Application;");
    if (!curApp) { env->ExceptionClear(); env->DeleteLocalRef(atCls); return nullptr; }
    jobject app = env->CallStaticObjectMethod(atCls, curApp);
    env->DeleteLocalRef(atCls);
    if (!app) { env->ExceptionClear(); return nullptr; }
    jclass appCls = env->GetObjectClass(app);
    jmethodID getCl = env->GetMethodID(appCls, "getClassLoader",
                                       "()Ljava/lang/ClassLoader;");
    jobject cl = getCl ? env->CallObjectMethod(app, getCl) : nullptr;
    env->DeleteLocalRef(appCls);
    env->DeleteLocalRef(app);
    if (!cl) { env->ExceptionClear(); return nullptr; }
    jclass clCls = env->GetObjectClass(cl);
    jmethodID loadCls = env->GetMethodID(clCls, "loadClass",
                                         "(Ljava/lang/String;)Ljava/lang/Class;");
    jstring jname = env->NewStringUTF(name);
    jobject k = loadCls ? env->CallObjectMethod(cl, loadCls, jname) : nullptr;
    env->DeleteLocalRef(jname);
    env->DeleteLocalRef(clCls);
    env->DeleteLocalRef(cl);
    if (!k) { env->ExceptionClear(); return nullptr; }
    jclass out = static_cast<jclass>(k);
    return out;
}

static void CacheJava(JNIEnv* env) {
    if (g_cls && g_mSetFeature && g_mSetValue) return;
    if (!g_cls) {
        jclass local = LoadAppClass(env,
            "com.starry.magic.cocos.GjdgsHooks");
        if (!local) {
            local = env->FindClass("com/starry/magic/cocos/GjdgsHooks");
            env->ExceptionClear();
        }
        if (!local) { LOGW("GjdgsHooks class not found (not merged yet?)"); return; }
        g_cls = static_cast<jclass>(env->NewGlobalRef(local));
        env->DeleteLocalRef(local);
    }
    if (!g_mSetFeature) {
        g_mSetFeature = env->GetStaticMethodID(g_cls, "setFeature", "(IZ)V");
        env->ExceptionClear();
    }
    if (!g_mSetValue) {
        g_mSetValue = env->GetStaticMethodID(g_cls, "setValue", "(II)V");
        env->ExceptionClear();
    }
}

static void PushFeature(int feat, bool on) {
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_cls && g_mSetFeature) {
            env->CallStaticVoidMethod(g_cls, g_mSetFeature, feat, on ? JNI_TRUE : JNI_FALSE);
            if (env->ExceptionCheck()) env->ExceptionClear();
        }
    }
    if (detach && g_vm) g_vm->DetachCurrentThread();
}

static void PushValue(int feat, int value) {
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_cls && g_mSetValue) {
            env->CallStaticVoidMethod(g_cls, g_mSetValue, feat, value);
            if (env->ExceptionCheck()) env->ExceptionClear();
        }
    }
    if (detach && g_vm) g_vm->DetachCurrentThread();
}

static int ClampInt(int v, int lo, int hi) {
    if (v < lo) return lo;
    if (v > hi) return hi;
    return v;
}

// ---------------------------------------------------------------- 对外接口
bool Gjdgs_TryInit(JavaVM* vm) {
    if (vm) g_vm = vm;
    if (!g_vm) return false;
    g_ready.store(true);
    LOGI("init ok (js-layer mod, zero native hook)");
    return true;
}

bool Gjdgs_IsReady() {
    return g_ready.load();
}

void Gjdgs_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case GJDGS_FEAT_GOLD:  g_gold.store(enabled); break;
        case GJDGS_FEAT_GEM:   g_gem.store(enabled); break;
        case GJDGS_FEAT_SHARD: g_shard.store(enabled); break;
        case GJDGS_FEAT_GOD:   g_god.store(enabled); break;
        case GJDGS_FEAT_NOAD:  g_noad.store(enabled); break;
        default: return;
    }
    PushFeature(featNum, enabled);
}

void Gjdgs_OnValueChanged(int featNum, int value) {
    switch (featNum) {
        case GJDGS_FEAT_SPD:
            g_spd.store(ClampInt(value, 1, 10));
            PushValue(featNum, g_spd.load());
            break;
        case GJDGS_FEAT_DMG:
            g_dmg.store(ClampInt(value, 1, 100));
            PushValue(featNum, g_dmg.load());
            break;
        default: break;
    }
}

void Gjdgs_Tick(const bool* features, const int* values) {
    if (!g_ready.load()) return;

    // 开机首次：把持久化的菜单状态整份推给 JS 层（游戏可能已带着上次开关启动）
    if (!g_bootSynced.load() && features && values) {
        g_bootSynced.store(true);
        g_god.store(features[GJDGS_FEAT_GOD]);
        g_gold.store(features[GJDGS_FEAT_GOLD]);
        g_gem.store(features[GJDGS_FEAT_GEM]);
        g_shard.store(features[GJDGS_FEAT_SHARD]);
        g_noad.store(features[GJDGS_FEAT_NOAD]);
        g_spd.store(ClampInt(values[GJDGS_FEAT_SPD], 1, 10));
        g_dmg.store(ClampInt(values[GJDGS_FEAT_DMG], 1, 100));
        PushFeature(GJDGS_FEAT_GOD, g_god.load());
        PushFeature(GJDGS_FEAT_GOLD, g_gold.load());
        PushFeature(GJDGS_FEAT_GEM, g_gem.load());
        PushFeature(GJDGS_FEAT_SHARD, g_shard.load());
        PushFeature(GJDGS_FEAT_NOAD, g_noad.load());
        PushValue(GJDGS_FEAT_SPD, g_spd.load());
        PushValue(GJDGS_FEAT_DMG, g_dmg.load());
        LOGI("boot sync push done god=%d spd=%d dmg=%d",
             (int)g_god.load(), g_spd.load(), g_dmg.load());
    }
    // 其余情况不在此轮询：InputValue 是「用户点一次加一次」的语义，
    // Tick 重复推送会把累加变成按帧累加（爆炸）。
}

void Gjdgs_ForceClear() {
    g_god.store(false);
    g_gold.store(false);
    g_gem.store(false);
    g_shard.store(false);
    g_noad.store(false);
    g_spd.store(1);
    g_dmg.store(1);
    g_gold.store(1000000);
    g_gem.store(5000);
    g_shard.store(5000);
    g_atk.store(50);
    g_hp.store(50);
}
