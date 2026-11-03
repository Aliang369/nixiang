/* ================================================================
 *  XxdlMod — 修仙大陆模拟器（com.sky.xxdl.dlt 1.0.5，Cocos Creator 3.6.3）
 *
 *  形态：零 native hook（与挂机打怪兽同一条路子）。功能全部落在游戏 JS 层
 *        （assets/main.js 追加的 window.__sst），本文件只负责
 *        「菜单动作 → JNI → Java XxdlHooks.setFeature → evalString」。
 *
 *  抓点依据：10_docs/修仙大陆模拟器/抓点_20261104.md
 *    window.g_server = MServer（静态数据类）
 *      g_server.base.gold    资源（addTableReward type 1）
 *      g_server.base.diamond 灵石（type 2）
 *      g_server.base.gem     仙晶（type 3）
 *      g_server.assets.new   体力（上限 assets.max）
 *    存档：localStorage 键 xiuxiandalu<名字>（MServer.texts0/texts1），顶栏每帧比对
 *          cache.updateBaseValue 自动刷新，改字段即生效并随 saveData0 落盘。
 *
 *  ★ 为什么不做 hook：数据全在 JS 活对象里，改值不需要 RVA/ABI；hook 只增加崩溃面。
 * ================================================================ */
#include "XxdlMod.h"

#include <android/log.h>
#include <atomic>
#include <dlfcn.h>
#include <jni.h>

#define LOG_TAG "StarryXxdl"
#define LOGI(...) __android_log_print(ANDROID_LOG_INFO,  LOG_TAG, __VA_ARGS__)
#define LOGW(...) __android_log_print(ANDROID_LOG_WARN,  LOG_TAG, __VA_ARGS__)

static JavaVM* g_vm = nullptr;
static std::atomic<bool> g_ready{false};

static jclass    g_cls = nullptr;
static jmethodID g_mSetFeature = nullptr;

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

// 非 Java 线程的 FindClass 会用 Bootstrap loader 找不到 app 类，
// 统一经 ActivityThread.currentApplication().getClassLoader() 取。
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
    return static_cast<jclass>(k);
}

static void CacheJava(JNIEnv* env) {
    if (g_cls && g_mSetFeature) return;
    if (!g_cls) {
        jclass local = LoadAppClass(env, "com.starry.magic.cocos.XxdlHooks");
        if (!local) {
            local = env->FindClass("com/starry/magic/cocos/XxdlHooks");
            env->ExceptionClear();
        }
        if (!local) { LOGW("XxdlHooks class not found (not merged yet?)"); return; }
        g_cls = static_cast<jclass>(env->NewGlobalRef(local));
        env->DeleteLocalRef(local);
    }
    if (!g_mSetFeature) {
        g_mSetFeature = env->GetStaticMethodID(g_cls, "setFeature", "(IZ)V");
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

bool Xxdl_TryInit(JavaVM* vm) {
    if (vm) g_vm = vm;
    if (!g_vm) return false;
    g_ready.store(true);
    LOGI("init ok (js-layer mod, zero native hook)");
    return true;
}

bool Xxdl_IsReady() { return g_ready.load(); }

void Xxdl_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case XXDL_FEAT_RES:
        case XXDL_FEAT_LINGSHI:
        case XXDL_FEAT_XIANJING:
        case XXDL_FEAT_TILI:
        case XXDL_FEAT_NOAD:
            PushFeature(featNum, enabled);
            break;
        default:
            break;
    }
}

void Xxdl_OnValueChanged(int featNum, int value) {
    (void)featNum; (void)value;  // 本作无滑块型功能
}

void Xxdl_Tick(const bool* features, const int* values) {
    (void)features; (void)values;
    // 不做开机重放：四项都是「点一次加一次」的动作语义，重放会重复加资源。
}

void Xxdl_ForceClear() {
    LOGI("forceClear (no persistent native state)");
}
