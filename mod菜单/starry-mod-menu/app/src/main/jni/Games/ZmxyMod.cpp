/* ================================================================
 *  造梦西游·黎尤浩劫 MOD
 *  仅三功能：百倍伤害 / 无敌 / 无限技能（无CD+不耗蓝）
 * ================================================================ */

#include "ZmxyMod.h"
#include "Includes/obfuscate.h"

#include <atomic>
#include <cstdio>
#include <cstring>
#include <pthread.h>
#include <string>
#include <unistd.h>

#if defined(__linux__) || defined(__ANDROID__)
#include <string.h>
#endif

#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Dobby/dobby.h"

static JavaVM* g_zmxyJvm = nullptr;
static bool g_ready = false;
static bool g_evalHooked = false;
static thread_local bool g_inEvalSplice = false;

static jclass g_hooksCls = nullptr;
static jmethodID g_hooksSetRet = nullptr;
static jmethodID g_hooksClear = nullptr;

static volatile bool g_dmg100 = false;
static volatile bool g_inv = false;
static volatile bool g_ultra = false;
static std::atomic<int> g_spliceHits{0};

static const char* kAct = "com.xm4399.zm6.AppActivity";

static JNIEnv* GetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_zmxyJvm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_zmxyJvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_zmxyJvm->AttachCurrentThread(&env, nullptr) != 0) return nullptr;
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
    jclass local = LoadAppClass(env, "com.starry.magic.cocos.ZmxyHooks");
    if (!local) local = env->FindClass(OBFUSCATE("com/starry/magic/cocos/ZmxyHooks"));
    if (local) {
        g_hooksCls = (jclass)env->NewGlobalRef(local);
        g_hooksSetRet = env->GetStaticMethodID(g_hooksCls, "setRet", "(IZ)V");
        g_hooksClear = env->GetStaticMethodID(g_hooksCls, "clearAll", "()V");
        env->DeleteLocalRef(local);
    } else {
        env->ExceptionClear();
    }
}

static void SyncJavaRets() {
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls && g_hooksSetRet) {
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 1, g_dmg100 ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 2, g_inv ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 3, g_ultra ? JNI_TRUE : JNI_FALSE);
    }
    if (detach) g_zmxyJvm->DetachCurrentThread();
}

static std::string SpliceCombatJs(const char* src) {
    std::string s(src);

    static char kDmgHook[1200];
    static char kSkillHook[1200];
    static bool once = false;
    if (!once) {
        snprintf(
            kDmgHook, sizeof(kDmgHook),
            "/*starry_zmxy_dmg*/ if (e < 0 && this.mGameLogic && this.mGameLogic.getMainPlayer "
            "&& \"undefined\" != typeof jsb && jsb.reflection) { try { var _mp = "
            "this.mGameLogic.getMainPlayer(0); if (_mp) { if (this.mCamp == _mp.mCamp) { if "
            "(jsb.reflection.callStaticMethod(\"%s\",\"get_ret\",\"(I)Z\",2)) return 0; } else { "
            "if (jsb.reflection.callStaticMethod(\"%s\",\"get_ret\",\"(I)Z\",1)) e *= 100; } } } "
            "catch (_e) {} } ",
            kAct, kAct);

        snprintf(
            kSkillHook, sizeof(kSkillHook),
            "/*starry_zmxy_uls*/ if (this.mPlayer && this.mPlayer.isPlayer && this.mGameLogic) "
            "{ var _f = this.mGameLogic.mGameTimeFrame; if (this.__ulsF !== _f) { this.__ulsF = _f; "
            "this.__uls = false; try { if (\"undefined\" != typeof jsb && jsb.reflection) "
            "this.__uls = !!jsb.reflection.callStaticMethod(\"%s\",\"get_ret\",\"(I)Z\",3); } "
            "catch (_e) {} } if (this.__uls) { try { var _pl=this.mPlayer; "
            "_pl.mConsumeMpRet=0; _pl.mRetCountSkillComsumeMp=0; if (_pl.setMp) { var "
            "_v=999999; try{ if(_pl.getMaxMp)_v=_pl.getMaxMp(); else "
            "if(_pl.getMp)_v=Math.max(_pl.getMp()||0,999999);}catch(_m){} _pl.setMp(_v); } } "
            "catch(_m2){} return null; } } ",
            kAct);
        once = true;
    }

    static const char* kDmgAnchors[] = {
        "if (0 == e || this.isDead()) return 0;",
        "if(0==e||this.isDead())return 0;",
        nullptr,
    };
    for (int i = 0; kDmgAnchors[i]; i++) {
        const char* a = kDmgAnchors[i];
        size_t p = s.find(a);
        if (p == std::string::npos) continue;
        if (s.find("starry_zmxy_dmg", p) != std::string::npos &&
            s.find("starry_zmxy_dmg", p) < p + 80)
            break;
        size_t end = p + strlen(a);
        while (end < s.size() && (s[end] == ' ' || s[end] == '\n' || s[end] == '\r')) end++;
        s.insert(end, kDmgHook);
        break;
    }

    static const char* kSkillAnchors[] = {
        "if (this.mPlayer.isDead() || this.mPlayer.mBuffMgr && "
        "!this.mPlayer.mBuffMgr.isCanReleaseSkill()) return \"1\";",
        "if(this.mPlayer.isDead()||this.mPlayer.mBuffMgr&&!this.mPlayer.mBuffMgr."
        "isCanReleaseSkill())return\"1\";",
        nullptr,
    };
    for (int i = 0; kSkillAnchors[i]; i++) {
        size_t p = s.find(kSkillAnchors[i]);
        if (p == std::string::npos) continue;
        if (p >= 40 && s.rfind("starry_zmxy_uls", p) != std::string::npos &&
            p - s.rfind("starry_zmxy_uls", p) < 500)
            break;
        s.insert(p, kSkillHook);
        break;
    }

    return s;
}

typedef jint (*JniEvalString_t)(JNIEnv*, jclass, jstring);
static JniEvalString_t orig_jniEval = nullptr;
static jint hooked_jniEval(JNIEnv* env, jclass cls, jstring js) {
    if (!orig_jniEval) return 0;
    return orig_jniEval(env, cls, js);
}

typedef bool (*SeEvalString_t)(void*, const char*, long, void*, const char*);
static SeEvalString_t orig_seEval = nullptr;
static bool g_seHooked = false;
static bool g_jniHooked = false;
static std::atomic<int> g_seCalls{0};

static bool hooked_seEval(void* thiz, const char* script, long length, void* ret,
                          const char* fileName) {
    if (!orig_seEval) return false;
    if (!script || g_inEvalSplice) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    long n = length;
    if (n < 0) n = (long)strlen(script);
    int calls = ++g_seCalls;
    if (calls <= 8 || calls % 200 == 0) {
        LOGI("[Zmxy] se::eval #%d len=%ld file=%s", calls, n,
             fileName ? fileName : "(null)");
    }

    if (n < 32 || n > 48 * 1024 * 1024) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    auto has = [&](const char* needle) -> bool {
        size_t nl = strlen(needle);
        if ((size_t)n < nl) return false;
        return memmem(script, (size_t)n, needle, nl) != nullptr;
    };

    if (has("starry_zmxy_")) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    bool combat = has("isCanReleaseSkill") ||
                  (has("getMainPlayer") && (has("isDead()") || has("mBuffMgr"))) ||
                  has("if (0 == e || this.isDead())") || has("if(0==e||this.isDead())");
    if (!combat) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    std::string src(script, (size_t)n);
    std::string patched = SpliceCombatJs(src.c_str());
    if (patched == src) {
        if (n < 4096) {
            LOGW("[Zmxy] splice miss len=%ld", n);
        }
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    int hits = ++g_spliceHits;
    LOGI("[Zmxy] combat splice #%d len %zu→%zu file=%s", hits, src.size(), patched.size(),
         fileName ? fileName : "(null)");

    g_inEvalSplice = true;
    bool ok = orig_seEval(thiz, patched.c_str(), (long)patched.size(), ret, fileName);
    g_inEvalSplice = false;
    return ok;
}

static void TryHookEvalString() {
    const char* lib = GameCocos::TARGET_LIB;
    if (!isLibraryLoaded(lib)) return;
    if (!g_seHooked) {
        void* sym = DobbySymbolResolver(lib, GameCocos::Symbols::SE_EVAL_STRING);
        if (sym &&
            DobbyHook(sym, (dobby_dummy_func_t)hooked_seEval,
                      (dobby_dummy_func_t*)&orig_seEval) == 0) {
            g_seHooked = true;
            g_evalHooked = true;
            LOGI("[Zmxy] hooked se::evalString");
        }
    }
    if (!g_jniHooked) {
        void* jni = DobbySymbolResolver(lib, GameCocos::Symbols::EVAL_STRING);
        if (jni &&
            DobbyHook(jni, (dobby_dummy_func_t)hooked_jniEval,
                      (dobby_dummy_func_t*)&orig_jniEval) == 0) {
            g_jniHooked = true;
        }
    }
}

bool Zmxy_TryInit(JavaVM* jvm) {
    g_zmxyJvm = jvm;
    if (!isLibraryLoaded(GameCocos::TARGET_LIB)) return false;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (detach) g_zmxyJvm->DetachCurrentThread();
    }
    TryHookEvalString();
    if (!g_seHooked) return false;
    g_ready = true;
    SyncJavaRets();
    return true;
}

static void* EarlyHookThread(void* arg) {
    g_zmxyJvm = (JavaVM*)arg;
    for (int i = 0; i < 2000; i++) {
        if (isLibraryLoaded(GameCocos::TARGET_LIB)) {
            TryHookEvalString();
            if (g_seHooked) {
                g_ready = true;
                LOGI("[Zmxy] early se hook ok iter=%d", i);
                bool detach = false;
                JNIEnv* env = GetEnv(&detach);
                if (env) {
                    CacheJava(env);
                    SyncJavaRets();
                    if (detach) g_zmxyJvm->DetachCurrentThread();
                }
                return nullptr;
            }
        }
        usleep(20 * 1000);
    }
    LOGW("[Zmxy] early hook timeout");
    return nullptr;
}

void Zmxy_StartEarlyHookThread(JavaVM* jvm) {
    static std::atomic_bool started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, jvm) == 0) pthread_detach(th);
    else started.store(false);
}

bool Zmxy_IsReady() { return g_ready; }

void Zmxy_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == ZMXY_FEAT_DMG100) g_dmg100 = enabled;
    if (featNum == ZMXY_FEAT_INVINCIBLE) g_inv = enabled;
    if (featNum == ZMXY_FEAT_ULTRA_SKILL) g_ultra = enabled;
    SyncJavaRets();
    LOGI("[Zmxy] feature #%d=%s dmg=%d inv=%d uls=%d", featNum, enabled ? "ON" : "OFF",
         g_dmg100 ? 1 : 0, g_inv ? 1 : 0, g_ultra ? 1 : 0);
}

void Zmxy_ForceClear() {
    g_dmg100 = g_inv = g_ultra = false;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_hooksClear) env->CallStaticVoidMethod(g_hooksCls, g_hooksClear);
        if (detach) g_zmxyJvm->DetachCurrentThread();
    }
}

void Zmxy_Tick(const bool* features) {
    if (!features) return;
    g_dmg100 = features[ZMXY_FEAT_DMG100];
    g_inv = features[ZMXY_FEAT_INVINCIBLE];
    g_ultra = features[ZMXY_FEAT_ULTRA_SKILL];
    if (!g_ready) return;
    if (!g_evalHooked) TryHookEvalString();
    SyncJavaRets();
}
