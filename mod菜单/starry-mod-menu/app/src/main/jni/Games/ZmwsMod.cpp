/* ================================================================
 *  造梦无双 MOD — 对齐斌哥 BSS JS（prefs 0..3 ↔ get_ret 1..4）
 * ================================================================ */

#include "ZmwsMod.h"
#include "Includes/obfuscate.h"

#include <algorithm>
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

static JavaVM* g_zmwsJvm = nullptr;
static bool g_ready = false;
static bool g_evalHooked = false;
static thread_local bool g_inEvalSplice = false;

static jclass g_hooksCls = nullptr;
static jmethodID g_hooksSetRet = nullptr;
static jmethodID g_hooksClear = nullptr;

static volatile bool g_ultra = false;
static volatile bool g_fabao = false;
static volatile bool g_seckill = false;
static volatile bool g_inv = false;
static std::atomic<int> g_spliceHits{0};

static const char* kAct = "com.xm4399.zmws.AppActivity";

static JNIEnv* GetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_zmwsJvm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_zmwsJvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_zmwsJvm->AttachCurrentThread(&env, nullptr) != 0) return nullptr;
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
    jclass local = LoadAppClass(env, "com.starry.magic.cocos.ZmwsHooks");
    if (!local) local = env->FindClass(OBFUSCATE("com/starry/magic/cocos/ZmwsHooks"));
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
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 1, g_ultra ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 2, g_fabao ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 3, g_seckill ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 4, g_inv ? JNI_TRUE : JNI_FALSE);
    }
    if (detach) g_zmwsJvm->DetachCurrentThread();
}

static std::string SpliceCombatJs(const char* src) {
    std::string s(src);

    static char kSkillHook[700];
    static char kFabaoHook[700];
    static char kAtkOutHook[700];
    static char kAtkInHook[900];
    static bool once = false;
    if (!once) {
        // get_ret(1) 无限技能 — 锚在 isCanReleaseSkill 失败 return 之后
        snprintf(
            kSkillHook, sizeof(kSkillHook),
            "/*starry_zmws_uls*/ var isChecked_RET=jsb.reflection.callStaticMethod("
            "\"%s\",\"get_ret\",\"(I)Z\",1); if(isChecked_RET){return null;} ",
            kAct);
        // get_ret(2) 法宝 — 锚在 getMagicCurEneryCount()-1 之后
        snprintf(
            kFabaoHook, sizeof(kFabaoHook),
            "/*starry_zmws_fabao*/ var isChecked_RET=jsb.reflection.callStaticMethod("
            "\"%s\",\"get_ret\",\"(I)Z\",2); if(isChecked_RET){return null;} ",
            kAct);
        // get_ret(3) 秒杀 — s.getAtk(e,i) 命中系统输出攻击
        snprintf(
            kAtkOutHook, sizeof(kAtkOutHook),
            "/*starry_zmws_sk*/ var isChecked_RET=jsb.reflection.callStaticMethod("
            "\"%s\",\"get_ret\",\"(I)Z\",3); if(isChecked_RET){return 9999999999999;} ",
            kAct);
        // get_ret(4) 无敌 — 挂承伤入口（与 Zmxy 同锚），勿挂 p.getAtk(e,t)（那是子弹 ATK，会双方变 1）
        snprintf(
            kAtkInHook, sizeof(kAtkInHook),
            "/*starry_zmws_inv*/ try{if(\"undefined\"!=typeof jsb&&jsb.reflection&&"
            "jsb.reflection.callStaticMethod(\"%s\",\"get_ret\",\"(I)Z\",4)){"
            "var _gl=this.mGameLogic||(this.mPlayer&&this.mPlayer.mGameLogic)||null;"
            "var _mp=_gl&&_gl.getMainPlayer?_gl.getMainPlayer(0):null;"
            "if(_mp&&this.mCamp==_mp.mCamp){return 0;}"
            "}}catch(_zi){} ",
            kAct);
        once = true;
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
        size_t end = p + strlen(kSkillAnchors[i]);
        if (s.find("starry_zmws_uls", end) != std::string::npos &&
            s.find("starry_zmws_uls", end) < end + 80)
            break;
        s.insert(end, kSkillHook);
        break;
    }

    static const char* kFabaoAnchors[] = {
        "var e = this.mPlayer.mProp.getMagicCurEneryCount() - 1;",
        "var e=this.mPlayer.mProp.getMagicCurEneryCount()-1;",
        nullptr,
    };
    for (int i = 0; kFabaoAnchors[i]; i++) {
        size_t p = s.find(kFabaoAnchors[i]);
        if (p == std::string::npos) continue;
        size_t end = p + strlen(kFabaoAnchors[i]);
        if (s.find("starry_zmws_fabao", end) != std::string::npos &&
            s.find("starry_zmws_fabao", end) < end + 80)
            break;
        s.insert(end, kFabaoHook);
        break;
    }

    auto spliceAfter = [&](const char* open, const char* tag, const char* hook) -> bool {
        size_t p = s.find(open);
        if (p == std::string::npos) return false;
        size_t end = p + strlen(open);
        if (s.find(tag, end) != std::string::npos && s.find(tag, end) < end + 160) return true;
        s.insert(end, hook);
        LOGI("[Zmws] spliced %s after '%s'", tag, open);
        return true;
    };

    // 秒杀：只挂 HitSystem 的 s.getAtk(e,i)，避免落到其它 getAtk
    bool sk = spliceAfter("s.getAtk = function(e, i) {", "starry_zmws_sk", kAtkOutHook) ||
              spliceAfter("getAtk = function(e, i) {", "starry_zmws_sk", kAtkOutHook);

    // 无敌：承伤函数头（Zmxy 同构），禁止再挂 p.getAtk(e,t)
    bool inv = spliceAfter("if (0 == e || this.isDead()) return 0;", "starry_zmws_inv", kAtkInHook) ||
               spliceAfter("if(0==e||this.isDead())return 0;", "starry_zmws_inv", kAtkInHook);
    if (!sk || !inv) {
        LOGW("[Zmws] combat splice sk=%d inv=%d", sk ? 1 : 0, inv ? 1 : 0);
    }

    // 反举报（对齐斌哥）
    if (s.find("copyHandler.addCheat = function") != std::string::npos &&
        s.find("starry_zmws_nocheat") == std::string::npos) {
        size_t p = s.find("copyHandler.addCheat = function");
        size_t brace = s.find('{', p);
        if (brace != std::string::npos) {
            s.insert(brace + 1, "/*starry_zmws_nocheat*/ return; ");
        }
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
        LOGI("[Zmws] se::eval #%d len=%ld file=%s", calls, n,
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

    if (has("starry_zmws_")) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    bool combat = has("isCanReleaseSkill") || has("getMagicCurEneryCount") ||
                  has("getAtk = function") || has("getAtk=function") ||
                  has("if (0 == e || this.isDead()) return 0") ||
                  has("copyHandler.addCheat");
    if (!combat) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

#ifdef STARRY_ZMWS_PROBE
    // 探针模式（锚点重定位诊断用，正式构建不编译）：dump 命中 combat 门限的明文脚本
    {
        static std::atomic<int> probeIdx{0};
        int idx = ++probeIdx;
        if (idx <= 6) {
            char path[256];
            snprintf(path, sizeof(path),
                     "/sdcard/Android/data/com.xm4399.zmws.m4399/files/starry_eval_%d.js", idx);
            FILE* f = fopen(path, "wb");
            if (f) {
                fwrite(script, 1, (size_t)n, f);
                fclose(f);
            }
            LOGI("[Zmws] probe dump #%d len=%ld file=%s", idx, n,
                 fileName ? fileName : "(null)");
        }
    }
#endif

    std::string src(script, (size_t)n);
    std::string patched = SpliceCombatJs(src.c_str());
    if (patched == src) {
        if (n < 8192) LOGW("[Zmws] splice miss len=%ld", n);
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    int hits = ++g_spliceHits;
    LOGI("[Zmws] combat splice #%d len %zu→%zu file=%s", hits, src.size(), patched.size(),
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
            LOGI("[Zmws] hooked se::evalString");
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

bool Zmws_TryInit(JavaVM* jvm) {
    g_zmwsJvm = jvm;
    if (!isLibraryLoaded(GameCocos::TARGET_LIB)) return false;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (detach) g_zmwsJvm->DetachCurrentThread();
    }
    TryHookEvalString();
    if (!g_seHooked) return false;
    g_ready = true;
    SyncJavaRets();
    return true;
}

static void* EarlyHookThread(void* arg) {
    g_zmwsJvm = (JavaVM*)arg;
    for (int i = 0; i < 2000; i++) {
        if (isLibraryLoaded(GameCocos::TARGET_LIB)) {
            TryHookEvalString();
            if (g_seHooked) {
                g_ready = true;
                LOGI("[Zmws] early se hook ok iter=%d", i);
                bool detach = false;
                JNIEnv* env = GetEnv(&detach);
                if (env) {
                    CacheJava(env);
                    SyncJavaRets();
                    if (detach) g_zmwsJvm->DetachCurrentThread();
                }
                return nullptr;
            }
        }
        usleep(20 * 1000);
    }
    LOGW("[Zmws] early hook timeout");
    return nullptr;
}

void Zmws_StartEarlyHookThread(JavaVM* jvm) {
    static std::atomic_bool started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, jvm) == 0) pthread_detach(th);
    else started.store(false);
}

bool Zmws_IsReady() { return g_ready; }

void Zmws_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == ZMWS_FEAT_ULTRA_SKILL) g_ultra = enabled;
    if (featNum == ZMWS_FEAT_FABAO) g_fabao = enabled;
    if (featNum == ZMWS_FEAT_SECKILL) g_seckill = enabled;
    if (featNum == ZMWS_FEAT_INVINCIBLE) g_inv = enabled;
    SyncJavaRets();
    LOGI("[Zmws] feature #%d=%s uls=%d fabao=%d sk=%d inv=%d", featNum,
         enabled ? "ON" : "OFF", g_ultra ? 1 : 0, g_fabao ? 1 : 0, g_seckill ? 1 : 0,
         g_inv ? 1 : 0);
}

void Zmws_ForceClear() {
    g_ultra = g_fabao = g_seckill = g_inv = false;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_hooksClear) env->CallStaticVoidMethod(g_hooksCls, g_hooksClear);
        if (detach) g_zmwsJvm->DetachCurrentThread();
    }
}

void Zmws_Tick(const bool* features) {
    if (!features) return;
    g_ultra = features[ZMWS_FEAT_ULTRA_SKILL];
    g_fabao = features[ZMWS_FEAT_FABAO];
    g_seckill = features[ZMWS_FEAT_SECKILL];
    g_inv = features[ZMWS_FEAT_INVINCIBLE];
    if (!g_ready) return;
    if (!g_evalHooked) TryHookEvalString();
    SyncJavaRets();
}
