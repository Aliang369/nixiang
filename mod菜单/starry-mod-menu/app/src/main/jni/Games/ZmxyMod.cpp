/* ================================================================
 *  造梦西游·黎尤浩劫 (com.xm4399.zmxy6hj) — Cocos2djs / libcocos2djs
 *
 *  机制（2026-08-10 版实证路径，本轮沿用）：
 *    native Dobby hook se::ScriptEngine::evalString，在运行时明文
 *    战斗 JS 上做锚点拼接；开关/数值由 JS 经
 *    AppActivity.get_ret / get_val → com.starry.magic.cocos.ZmxyHooks 读。
 *
 *  菜单（featureList 下标 = featNum，见 ZmxyEntry.cpp）：
 *    1 Slider_伤害倍率:1:100:1  敌营承伤 e *= mul（mul>1 才动）
 *    2 Toggle_无敌              我方承伤 return 0
 *    3 Toggle_无限技能          无 CD / 不耗蓝
 *
 *  依赖 app 内 dex 的 ZmxyHooks（外科包 classes7.dex），不在 native 注册。
 * ================================================================ */

#include "ZmxyMod.h"

#include <atomic>
#include <cstdio>
#include <cstring>
#include <mutex>
#include <pthread.h>
#include <string>
#include <unistd.h>

#include "Dobby/dobby.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "ZmxyPatchJs.h"

#define ZMXY_LOG "[Zmxy]"

static const char kTargetLib[] = "libcocos2djs.so";
// se::ScriptEngine::evalString(char const*, long, se::Value*, char const*)
static const char kSeEvalSym[] = "_ZN2se12ScriptEngine10evalStringEPKclPNS_5ValueES2_";

static JavaVM* g_jvm = nullptr;
static std::atomic<bool> g_seHooked{false};
static std::atomic<bool> g_ready{false};
static thread_local bool g_inEvalSplice = false;

static jclass g_hooksCls = nullptr;
static jmethodID g_setRet = nullptr;
static jmethodID g_setVal = nullptr;
static jmethodID g_clearAll = nullptr;

static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_ultra{false};
static std::atomic<int> g_dmgMul{1};
static std::atomic<int> g_spliceHits{0};

static JNIEnv* GetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_jvm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_jvm->GetEnv(reinterpret_cast<void**>(&env), JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_jvm->AttachCurrentThread(&env, nullptr) != 0) return nullptr;
        *needDetach = true;
    } else if (st != JNI_OK) {
        return nullptr;
    }
    return env;
}

// app dex 内的类要走 Application 的 ClassLoader（native 线程裸 FindClass 不可靠）
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
        cls = static_cast<jclass>(env->CallObjectMethod(cl, loadClass, jname));
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
    jclass local = LoadAppClass(env, "com.starry.magic.cocos.ZmxyHooks");
    if (!local) local = env->FindClass("com/starry/magic/cocos/ZmxyHooks");
    if (local) {
        g_hooksCls = static_cast<jclass>(env->NewGlobalRef(local));
        g_setRet = env->GetStaticMethodID(g_hooksCls, "setRet", "(IZ)V");
        g_setVal = env->GetStaticMethodID(g_hooksCls, "setVal", "(II)V");
        g_clearAll = env->GetStaticMethodID(g_hooksCls, "clearAll", "()V");
        env->DeleteLocalRef(local);
        LOGI(ZMXY_LOG " ZmxyHooks cached");
    } else {
        env->ExceptionClear();
        LOGW(ZMXY_LOG " ZmxyHooks not found yet");
    }
}

static void SyncJavaState() {
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls && g_setRet) {
        const int mul = g_dmgMul.load();
        env->CallStaticVoidMethod(g_hooksCls, g_setRet, 1,
                                  mul > 1 ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_setRet, 2,
                                  g_inv.load() ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_setRet, 3,
                                  g_ultra.load() ? JNI_TRUE : JNI_FALSE);
        if (g_setVal) env->CallStaticVoidMethod(g_hooksCls, g_setVal, 1, mul);
    }
    if (detach) g_jvm->DetachCurrentThread();
}

// ----------------------------------------------------------------
//  JS 拼接（锚点来自 2026-08-09 抓点 + 2026-08-10 实证，未变）
// ----------------------------------------------------------------
static const char* kDmgAnchor1 = "if (0 == e || this.isDead()) return 0;";
static const char* kDmgAnchor2 = "if(0==e||this.isDead())return 0;";

static const char* kSkillAnchor1 =
    "if (this.mPlayer.isDead() || this.mPlayer.mBuffMgr && "
    "!this.mPlayer.mBuffMgr.isCanReleaseSkill()) return \"1\";";
static const char* kSkillAnchor2 =
    "if(this.mPlayer.isDead()||this.mPlayer.mBuffMgr&&!this.mPlayer.mBuffMgr."
    "isCanReleaseSkill())return\"1\";";

static char kDmgHook[1100];
static char kSkillHook[1100];
static std::once_flag g_hooksOnce;  // 拼接体只格式化一次（多线程 eval 也不撕裂）

// 首次调用时格式化拼接体（%s = ZMXY_JS_ACT，正文见 ZmxyPatchJs.h）
static void FormatHooksOnce() {
    std::call_once(g_hooksOnce, [] {
        snprintf(kDmgHook, sizeof(kDmgHook), kZmxyDmgHookFmt, ZMXY_JS_ACT, ZMXY_JS_ACT);
        snprintf(kSkillHook, sizeof(kSkillHook), kZmxySkillHookFmt, ZMXY_JS_ACT);
    });
}

// 幂等：锚点前插入（已插过则按 tag 距离去重）
static bool SpliceBefore(std::string& s, const char* anchor, const char* tag) {
    const size_t p = s.find(anchor);
    if (p == std::string::npos) return false;
    const size_t prev = s.rfind(tag, p);
    if (prev != std::string::npos && p - prev < 1400) return true;
    s.insert(p, kSkillHook);
    return true;
}

// 幂等：锚点后插入
static bool SpliceAfter(std::string& s, const char* anchor, const char* tag) {
    const size_t p = s.find(anchor);
    if (p == std::string::npos) return false;
    const size_t end = p + strlen(anchor);
    const size_t next = s.find(tag, end);
    if (next != std::string::npos && next < end + 200) return true;
    s.insert(end, kDmgHook);
    return true;
}

static std::string SpliceCombatJs(const char* src, size_t len, int* hitMask) {
    FormatHooksOnce();
    std::string s(src, len);

    const bool dmg = SpliceAfter(s, kDmgAnchor1, "starry_zmxy_dmg") ||
                     SpliceAfter(s, kDmgAnchor2, "starry_zmxy_dmg");
    const bool uls = SpliceBefore(s, kSkillAnchor1, "starry_zmxy_uls") ||
                     SpliceBefore(s, kSkillAnchor2, "starry_zmxy_uls");

    if (hitMask) *hitMask = (dmg ? 1 : 0) | (uls ? 2 : 0);
    return s;
}

// ----------------------------------------------------------------
//  evalString hook
// ----------------------------------------------------------------
typedef bool (*SeEvalString_t)(void*, const char*, long, void*, const char*);
static SeEvalString_t g_orig_seEval = nullptr;
static std::atomic<int> g_seCalls{0};

static bool hooked_seEval(void* thiz, const char* script, long length, void* ret,
                          const char* fileName) {
    if (!g_orig_seEval) return false;
    if (!script || g_inEvalSplice) {
        return g_orig_seEval(thiz, script, length, ret, fileName);
    }

    long n = length;
    if (n < 0) n = static_cast<long>(strlen(script));
    const int calls = ++g_seCalls;
    if (calls <= 8 || calls % 500 == 0) {
        LOGI(ZMXY_LOG " se::eval #%d len=%ld file=%s", calls, n,
             fileName ? fileName : "(null)");
    }
    if (n < 32 || n > 48L * 1024L * 1024L) {
        return g_orig_seEval(thiz, script, length, ret, fileName);
    }

    auto has = [&](const char* needle) -> bool {
        const size_t nl = strlen(needle);
        if (static_cast<size_t>(n) < nl) return false;
        return memmem(script, static_cast<size_t>(n), needle, nl) != nullptr;
    };

    if (has(kZmxyJsMarker)) {
        return g_orig_seEval(thiz, script, length, ret, fileName);
    }

    const bool relevant = has("isCanReleaseSkill") || has("getMainPlayer") ||
                          has("if (0 == e || this.isDead())") ||
                          has("if(0==e||this.isDead())");
    if (!relevant) {
        return g_orig_seEval(thiz, script, length, ret, fileName);
    }

    int hitMask = 0;
    std::string patched = SpliceCombatJs(script, static_cast<size_t>(n), &hitMask);
    if (patched.size() == static_cast<size_t>(n) &&
        memcmp(patched.data(), script, static_cast<size_t>(n)) == 0) {
        LOGW(ZMXY_LOG " combat script but no splice hit len=%ld file=%s", n,
             fileName ? fileName : "(null)");
        return g_orig_seEval(thiz, script, length, ret, fileName);
    }

    const int hits = ++g_spliceHits;
    LOGI(ZMXY_LOG " combat splice #%d len %ld->%zu dmg=%d uls=%d file=%s", hits, n,
         patched.size(), (hitMask & 1) ? 1 : 0, (hitMask & 2) ? 1 : 0,
         fileName ? fileName : "(null)");

    g_inEvalSplice = true;
    const bool ok = g_orig_seEval(thiz, patched.c_str(), static_cast<long>(patched.size()),
                                  ret, fileName);
    g_inEvalSplice = false;
    return ok;
}

static bool TryHookSeEval() {
    if (g_seHooked.load()) return true;
    if (!isLibraryLoaded(kTargetLib)) return false;
    void* sym = DobbySymbolResolver(kTargetLib, kSeEvalSym);
    if (!sym) {
        LOGW(ZMXY_LOG " se::evalString symbol not found in %s", kTargetLib);
        return false;
    }
    if (DobbyHook(sym, reinterpret_cast<dobby_dummy_func_t>(hooked_seEval),
                  reinterpret_cast<dobby_dummy_func_t*>(&g_orig_seEval)) != 0) {
        LOGW(ZMXY_LOG " dobby hook se::evalString failed");
        return false;
    }
    g_seHooked.store(true);
    LOGI(ZMXY_LOG " hooked se::evalString @%p", sym);
    return true;
}

// ----------------------------------------------------------------
//  生命周期
// ----------------------------------------------------------------
static void* EarlyHookThread(void* arg) {
    g_jvm = static_cast<JavaVM*>(arg);
    for (int i = 0; i < 3000; i++) {
        if (isLibraryLoaded(kTargetLib) && TryHookSeEval()) {
            bool detach = false;
            JNIEnv* env = GetEnv(&detach);
            if (env) {
                CacheJava(env);
                SyncJavaState();
                if (detach) g_jvm->DetachCurrentThread();
            }
            g_ready.store(true);
            LOGI(ZMXY_LOG " early hook ok iter=%d", i);
            return nullptr;
        }
        usleep(20 * 1000);
    }
    LOGW(ZMXY_LOG " early hook timeout");
    return nullptr;
}

void Zmxy_StartEarlyHookThread(JavaVM* jvm) {
    g_jvm = jvm;
    static std::atomic_bool started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, jvm) == 0) {
        pthread_detach(th);
    } else {
        started.store(false);
    }
}

bool Zmxy_TryInit(JavaVM* jvm) {
    g_jvm = jvm;
    if (!isLibraryLoaded(kTargetLib)) return false;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (detach) g_jvm->DetachCurrentThread();
    }
    if (!TryHookSeEval()) return false;
    g_ready.store(true);
    SyncJavaState();
    return true;
}

bool Zmxy_IsReady() { return g_ready.load(); }

void Zmxy_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case ZMXY_FEAT_INVINCIBLE:
            g_inv.store(enabled);
            break;
        case ZMXY_FEAT_ULTRA_SKILL:
            g_ultra.store(enabled);
            break;
        default:
            break;
    }
    SyncJavaState();
    LOGI(ZMXY_LOG " feature #%d=%s mul=%d inv=%d uls=%d", featNum, enabled ? "ON" : "OFF",
         g_dmgMul.load(), g_inv.load() ? 1 : 0, g_ultra.load() ? 1 : 0);
}

void Zmxy_OnValueChanged(int featNum, int value) {
    if (featNum != ZMXY_FEAT_DMG_MUL) return;
    if (value < 1) value = 1;
    if (value > 100) value = 100;
    g_dmgMul.store(value);
    SyncJavaState();
    LOGI(ZMXY_LOG " dmgMul=%d", value);
}

void Zmxy_Tick(const bool* features, const int* values) {
    if (features) {
        g_inv.store(features[ZMXY_FEAT_INVINCIBLE]);
        g_ultra.store(features[ZMXY_FEAT_ULTRA_SKILL]);
    }
    if (values && values[ZMXY_FEAT_DMG_MUL] > 0) {
        int mul = values[ZMXY_FEAT_DMG_MUL];
        if (mul > 100) mul = 100;
        g_dmgMul.store(mul);
    }
    if (!g_seHooked.load() && isLibraryLoaded(kTargetLib) && TryHookSeEval()) {
        g_ready.store(true);
    }
    SyncJavaState();
}

void Zmxy_ForceClear() {
    g_dmgMul.store(1);
    g_inv.store(false);
    g_ultra.store(false);
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_clearAll) env->CallStaticVoidMethod(g_hooksCls, g_clearAll);
        if (detach) g_jvm->DetachCurrentThread();
    }
}
// JS 侧门控入口（app dex 内补静态转发方法）见 ZmxyPatchJs.h 的 ZMXY_JS_ACT
