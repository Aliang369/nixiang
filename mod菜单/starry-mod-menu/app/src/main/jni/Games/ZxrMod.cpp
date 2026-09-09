/* ================================================================
 *  镇邪人 MOD
 *  战斗：对齐斌哥 — 在 Cocos evalString 明文中拼接 get_ret 钩子
 *  免广告：Java SdkManager.SdkShowAdReward（见 inject 脚本）
 * ================================================================ */

#include "ZxrMod.h"
#include "Includes/obfuscate.h"

#include <atomic>
#include <cstdio>
#include <cstring>
#include <pthread.h>
#include <string>
#include <unistd.h>

#if defined(__linux__) || defined(__ANDROID__)
#include <string.h> // memmem
#endif

#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Dobby/dobby.h"

static JavaVM* g_zxrJvm = nullptr;
static bool g_ready = false;
static bool g_evalHooked = false;
static thread_local bool g_inEvalSplice = false;

static jclass g_bridgeCls = nullptr;
static jmethodID g_bridgeEval = nullptr;
static jclass g_hooksCls = nullptr;
static jmethodID g_hooksSetRet = nullptr;
static jmethodID g_hooksSetNoAd = nullptr;
static jmethodID g_hooksClear = nullptr;

static volatile bool g_seckill = false;
static volatile bool g_inv = false;
static volatile bool g_atkSpd = false;
static volatile bool g_aoe = false;
static volatile bool g_noAd = false;
static std::atomic<int> g_speedLvl{1}; // 1=关, 2..5 → timeScale
static std::atomic<int> g_spliceHits{0};

static JNIEnv* GetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_zxrJvm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_zxrJvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_zxrJvm->AttachCurrentThread(&env, nullptr) != 0) return nullptr;
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
    if (!g_bridgeCls) {
        jclass local = LoadAppClass(env, "com.starry.magic.cocos.CocosJsBridge");
        if (!local) local = env->FindClass(OBFUSCATE("com/starry/magic/cocos/CocosJsBridge"));
        if (local) {
            g_bridgeCls = (jclass)env->NewGlobalRef(local);
            g_bridgeEval =
                env->GetStaticMethodID(g_bridgeCls, "evalOnGLThread", "(Ljava/lang/String;)V");
            env->DeleteLocalRef(local);
            LOGI("[Zxr] CocosJsBridge cached ok=%d", g_bridgeEval ? 1 : 0);
        } else {
            env->ExceptionClear();
            LOGW("[Zxr] CocosJsBridge not found");
        }
    }
    if (!g_hooksCls) {
        jclass local = LoadAppClass(env, "com.starry.magic.cocos.ZxrHooks");
        if (!local) local = env->FindClass(OBFUSCATE("com/starry/magic/cocos/ZxrHooks"));
        if (local) {
            g_hooksCls = (jclass)env->NewGlobalRef(local);
            g_hooksSetRet = env->GetStaticMethodID(g_hooksCls, "setRet", "(IZ)V");
            g_hooksSetNoAd = env->GetStaticMethodID(g_hooksCls, "setNoAd", "(Z)V");
            g_hooksClear = env->GetStaticMethodID(g_hooksCls, "clearAll", "()V");
            env->DeleteLocalRef(local);
        } else {
            env->ExceptionClear();
        }
    }
}

static void EvalJs(const char* js) {
    if (!g_zxrJvm || !js) return;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_bridgeCls && g_bridgeEval) {
        jstring jjs = env->NewStringUTF(js);
        if (jjs) {
            env->CallStaticVoidMethod(g_bridgeCls, g_bridgeEval, jjs);
            if (env->ExceptionCheck()) env->ExceptionClear();
            env->DeleteLocalRef(jjs);
        }
    }
    if (detach) g_zxrJvm->DetachCurrentThread();
}

static void SyncJavaRets() {
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls && g_hooksSetRet) {
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 1, g_seckill ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 2, g_inv ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 3, g_atkSpd ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 4, g_aoe ? JNI_TRUE : JNI_FALSE);
    }
    if (g_hooksCls && g_hooksSetNoAd) {
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetNoAd, g_noAd ? JNI_TRUE : JNI_FALSE);
    }
    if (detach) g_zxrJvm->DetachCurrentThread();
}

static void SyncSpeedJs() {
    char buf[384];
    int lvl = g_speedLvl.load();
    if (lvl < 1) lvl = 1;
    if (lvl > 5) lvl = 5;
    snprintf(buf, sizeof(buf),
             "(function(){var g=typeof globalThis!=='undefined'?globalThis:window;"
             "g.__starryZxrSpeedLvl=%d;"
             "try{var s=1;if(%d<=1)s=1;else if(%d===2)s=1.5;else if(%d===3)s=2;"
             "else if(%d===4)s=3;else s=4;"
             "if(typeof cc!=='undefined'&&cc.director&&cc.director.getScheduler)"
             "cc.director.getScheduler().setTimeScale(s);}catch(e){}"
             "})();",
             lvl, lvl, lvl, lvl, lvl);
    EvalJs(buf);
}

/* 斌哥式拼接：锚点后插入 JMYBTActivity.get_ret 门控 */
static bool NeedsCombatSplice(const char* s) {
    if (!s) return false;
    if (strstr(s, "isChecked_RET_dmg100")) return false; // 已拼
    return strstr(s, "baseDmg") || strstr(s, "FightChangeEntityHp") ||
           strstr(s, "Math.max(e,.01)") || strstr(s, "Math.max(e, .01)") ||
           strstr(s, "getAtk=function") || strstr(s, "getAtk = function");
}

static void ReplaceOnce(std::string& s, const char* from, const char* to) {
    size_t p = s.find(from);
    if (p == std::string::npos) return;
    s.replace(p, strlen(from), to);
}

static std::string SpliceCombatJs(const char* src) {
    std::string s(src);
    static const char* kDmgHook =
        "var isChecked_RET_dmg100=jsb.reflection.callStaticMethod("
        "\"com.sanqi.jmybt.game37.JMYBTActivity\",\"get_ret\",\"(I)Z\",1);"
        "if(isChecked_RET_dmg100){n&&n.heroCfg&&(t*=100);}"
        "var isChecked_RET_dmg0=jsb.reflection.callStaticMethod("
        "\"com.sanqi.jmybt.game37.JMYBTActivity\",\"get_ret\",\"(I)Z\",2);"
        "if(isChecked_RET_dmg0){r&&r.heroCfg&&(t=0);}";

    static const char* kAtkHook =
        "var isChecked_RET_atkspd10=jsb.reflection.callStaticMethod("
        "\"com.sanqi.jmybt.game37.JMYBTActivity\",\"get_ret\",\"(I)Z\",3);"
        "if(isChecked_RET_atkspd10){t&&t.heroCfg&&!t.CheckEntityHaveStatus("
        "l.EnumEntityStatus.bossFakeControl)&&(e*=10);}";

    static const char* kAoeHook =
        "var isChecked_RET_aoeAll=jsb.reflection.callStaticMethod("
        "\"com.sanqi.jmybt.game37.JMYBTActivity\",\"get_ret\",\"(I)Z\",4);"
        "if(isChecked_RET_aoeAll&&!this._aoeAllEnemiesRunning&&i<0&&t&&t.monsterCfg){"
        "var aoeCaster=d.EntityManager.Inst().GetEntity(e);"
        "if(aoeCaster&&aoeCaster.heroCfg){this._aoeAllEnemiesRunning=!0;try{"
        "var aoeMap=d.EntityManager.Inst().entityMap;"
        "aoeMap&&aoeMap.forEach(function(aoeEnt){"
        "aoeEnt&&aoeEnt!==t&&!aoeEnt.IsDead&&aoeEnt.monsterCfg&&"
        "this.FightChangeEntityHp(aoeEnt,e,i,o,n);},this);"
        "}finally{this._aoeAllEnemiesRunning=!1;}}}";

    // 伤害：t=c.baseDmg → 其后插入秒杀/无敌
    if (s.find("t=c.baseDmg") != std::string::npos) {
        std::string rep = std::string("t=c.baseDmg;") + kDmgHook;
        ReplaceOnce(s, "t=c.baseDmg", rep.c_str());
    } else if (s.find("t = c.baseDmg") != std::string::npos) {
        std::string rep = std::string("t = c.baseDmg;") + kDmgHook;
        ReplaceOnce(s, "t = c.baseDmg", rep.c_str());
    }

    // 攻速：e=Math.max(e,.01)
    if (s.find("e=Math.max(e,.01)") != std::string::npos) {
        std::string rep = std::string("e=Math.max(e,.01);") + kAtkHook;
        ReplaceOnce(s, "e=Math.max(e,.01)", rep.c_str());
    } else if (s.find("e = Math.max(e, .01)") != std::string::npos) {
        std::string rep = std::string("e = Math.max(e, .01);") + kAtkHook;
        ReplaceOnce(s, "e = Math.max(e, .01)", rep.c_str());
    } else if (s.find("e=Math.max(e, .01)") != std::string::npos) {
        std::string rep = std::string("e=Math.max(e, .01);") + kAtkHook;
        ReplaceOnce(s, "e=Math.max(e, .01)", rep.c_str());
    }

    // 全屏：FightChangeEntityHp 体内 var f=o&&o.force 前插入
    if (s.find("FightChangeEntityHp") != std::string::npos) {
        if (s.find("var f=o&&o.force") != std::string::npos) {
            std::string rep = std::string(kAoeHook) + "var f=o&&o.force";
            ReplaceOnce(s, "var f=o&&o.force", rep.c_str());
        } else if (s.find("var f = o && o.force") != std::string::npos) {
            std::string rep = std::string(kAoeHook) + "var f = o && o.force";
            ReplaceOnce(s, "var f = o && o.force", rep.c_str());
        }
    }

    return s;
}

/* JNI evalString：仅 Java→JS（加速/SDK）；战斗 .jsc 不走此路径 */
typedef jint (*JniEvalString_t)(JNIEnv*, jclass, jstring);
static JniEvalString_t orig_jniEval = nullptr;

static jint hooked_jniEval(JNIEnv* env, jclass cls, jstring js) {
    if (!orig_jniEval) return 0;
    return orig_jniEval(env, cls, js);
}

/* 斌哥主路径：se::ScriptEngine::evalString(this, script, len, ret, fileName) */
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
#if defined(STARRY_TRACE)
    if (calls <= 8 || calls % 200 == 0) {
        LOGI("[Zxr] se::eval #%d len=%ld file=%s", calls, n,
             fileName ? fileName : "(null)");
    }
#else
    (void)calls;
#endif

    if (n < 32 || n > 48 * 1024 * 1024) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    // 大包勿整份拷贝进 std::string 两次；先 memmem 探关键词
    auto has = [&](const char* needle) -> bool {
        size_t nl = strlen(needle);
        if ((size_t)n < nl) return false;
        return memmem(script, (size_t)n, needle, nl) != nullptr;
    };
    bool already = has("isChecked_RET_dmg100");
    bool combat = !already && (has("baseDmg") || has("FightChangeEntityHp") ||
                               has("Math.max(e,.01)") || has("Math.max(e, .01)") ||
                               has("getAtk=function") || has("getAtk = function"));
#if defined(STARRY_TRACE)
    if (n > 1024 * 1024) {
        LOGI("[Zxr] big eval len=%ld combat=%d baseDmg=%d Fight=%d atkMax=%d file=%s", n,
             combat ? 1 : 0, has("baseDmg") ? 1 : 0, has("FightChangeEntityHp") ? 1 : 0,
             (has("Math.max(e,.01)") || has("Math.max(e, .01)")) ? 1 : 0,
             fileName ? fileName : "(null)");
    }
#endif
    if (!combat) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    std::string src(script, (size_t)n);
    std::string patched = SpliceCombatJs(src.c_str());
    if (patched == src) {
        LOGW("[Zxr] combat keywords hit but splice anchors missed len=%ld", n);
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    int hits = ++g_spliceHits;
    LOGI("[Zxr] combat splice #%d len %zu→%zu file=%s", hits, src.size(), patched.size(),
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
            LOGI("[Zxr] hooked se::ScriptEngine::evalString (combat splice)");
        } else if (!sym) {
            LOGW("[Zxr] se::evalString symbol missing");
        }
    }

    if (!g_jniHooked) {
        void* jni = DobbySymbolResolver(lib, GameCocos::Symbols::EVAL_STRING);
        if (jni &&
            DobbyHook(jni, (dobby_dummy_func_t)hooked_jniEval,
                      (dobby_dummy_func_t*)&orig_jniEval) == 0) {
            g_jniHooked = true;
            LOGI("[Zxr] hooked JNI evalString (passthrough)");
        }
    }
}

bool Zxr_TryInit(JavaVM* jvm) {
    g_zxrJvm = jvm;
    const char* lib = GameCocos::TARGET_LIB;
    if (!isLibraryLoaded(lib)) {
        return false;
    }
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (detach) g_zxrJvm->DetachCurrentThread();
    }
    TryHookEvalString();
    if (!g_seHooked) return false;
    g_ready = true;
    SyncJavaRets();
    SyncSpeedJs();
    LOGI("[Zxr] init ok se=%d jni=%d splice=%d seCalls=%d", g_seHooked ? 1 : 0,
         g_jniHooked ? 1 : 0, g_spliceHits.load(), g_seCalls.load());
    return true;
}

static void* EarlyHookThread(void* arg) {
    JavaVM* jvm = (JavaVM*)arg;
    g_zxrJvm = jvm;
    LOGI("[Zxr] early hook thread start");
    for (int i = 0; i < 2000; i++) { // ~40s @ 20ms
        if (isLibraryLoaded(GameCocos::TARGET_LIB)) {
            TryHookEvalString();
            if (g_seHooked) {
                g_ready = true;
                LOGI("[Zxr] early se hook ok at iter=%d splice=%d", i, g_spliceHits.load());
                bool detach = false;
                JNIEnv* env = GetEnv(&detach);
                if (env) {
                    CacheJava(env);
                    SyncJavaRets();
                    if (detach) g_zxrJvm->DetachCurrentThread();
                }
                return nullptr;
            }
        }
        usleep(20 * 1000);
    }
    LOGW("[Zxr] early hook timeout");
    return nullptr;
}

void Zxr_StartEarlyHookThread(JavaVM* jvm) {
    static std::atomic_bool started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, jvm) == 0) {
        pthread_detach(th);
    } else {
        started.store(false);
        LOGW("[Zxr] early hook pthread_create failed");
    }
}

bool Zxr_IsReady() { return g_ready; }

void Zxr_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == ZXR_FEAT_SECKILL) g_seckill = enabled;
    if (featNum == ZXR_FEAT_INVINCIBLE) g_inv = enabled;
    if (featNum == ZXR_FEAT_ATK_SPEED) g_atkSpd = enabled;
    if (featNum == ZXR_FEAT_AOE_KILL) g_aoe = enabled;
    if (featNum == ZXR_FEAT_NO_AD) g_noAd = enabled;
    if (featNum == ZXR_FEAT_SPEEDUP && !enabled) g_speedLvl.store(1);
    SyncJavaRets();
    if (g_ready) SyncSpeedJs();
    LOGI("[Zxr] feature #%d=%s noAd=%d splice=%d", featNum, enabled ? "ON" : "OFF",
         g_noAd ? 1 : 0, g_spliceHits.load());
}

void Zxr_OnValueChanged(int featNum, int value) {
    if (featNum == ZXR_FEAT_SPEEDUP) {
        if (value < 1) value = 1;
        if (value > 5) value = 5;
        int prev = g_speedLvl.exchange(value);
        if (prev == value) return;
        if (g_ready) SyncSpeedJs();
        LOGI("[Zxr] speedLvl=%d", value);
    }
}

void Zxr_ForceClear() {
    g_seckill = g_inv = g_atkSpd = g_aoe = false;
    g_noAd = false;
    g_speedLvl.store(1);
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_hooksClear) {
            env->CallStaticVoidMethod(g_hooksCls, g_hooksClear);
        }
        if (detach) g_zxrJvm->DetachCurrentThread();
    }
    if (g_ready) SyncSpeedJs();
}

void Zxr_Tick(const bool* features) {
    if (!features) return;
    bool s = features[ZXR_FEAT_SECKILL];
    bool i = features[ZXR_FEAT_INVINCIBLE];
    bool a = features[ZXR_FEAT_ATK_SPEED];
    bool o = features[ZXR_FEAT_AOE_KILL];
    bool ad = features[ZXR_FEAT_NO_AD];
    bool dirty = (s != g_seckill || i != g_inv || a != g_atkSpd || o != g_aoe || ad != g_noAd);
    g_seckill = s;
    g_inv = i;
    g_atkSpd = a;
    g_aoe = o;
    g_noAd = ad;
    if (!g_ready) return;
    if (!g_evalHooked) TryHookEvalString();
    if (dirty) {
        SyncJavaRets();
        LOGI("[Zxr] feature sync dirty splice=%d", g_spliceHits.load());
    }
    // 加速仅在开启时周期刷 timeScale；关着不打 JNI/JS（防掉帧）
    static int tickN = 0;
    if ((++tickN % 40) == 0 && g_speedLvl.load() > 1) {
        SyncSpeedJs();
#if defined(STARRY_TRACE)
        LOGI("[Zxr] tick splice=%d seCalls=%d speedLvl=%d", g_spliceHits.load(),
             g_seCalls.load(), g_speedLvl.load());
#endif
    }
}
