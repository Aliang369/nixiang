/* ================================================================
 *  青鸾繁华录 MOD (com.hdg4.g4qlfh) — Cocos Creator 2.x jsb
 *  明文 JS bundle（logic/main/cocos2d-jsb）过 se::evalString 时拼接，
 *  开关经 AppActivity.get_ret(n) / get_speed() 读取（QlfhHooks）。
 *  锚点均经 9.8.146 包逐字节验证，见 10_docs/青鸾繁华录/抓点定稿_20260829.md
 * ================================================================ */

#include "QlfhMod.h"
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

static JavaVM* g_qlfhJvm = nullptr;
static bool g_ready = false;
static bool g_evalHooked = false;
static thread_local bool g_inEvalSplice = false;

static jclass g_hooksCls = nullptr;
static jmethodID g_hooksSetRet = nullptr;
static jmethodID g_hooksSetSpeed = nullptr;
static jmethodID g_hooksClear = nullptr;

static volatile bool g_noAd = false;
static volatile bool g_dmg100 = false;
static volatile bool g_inv = false;
static std::atomic<int> g_speed{1};

static const char* kAct = "com.hdg4.h5microclient.AppActivity";

static JNIEnv* GetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_qlfhJvm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_qlfhJvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_qlfhJvm->AttachCurrentThread(&env, nullptr) != 0) return nullptr;
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
    if (!g_hooksCls) {
        jclass local = LoadAppClass(env, "com.starry.magic.cocos.QlfhHooks");
        if (!local) local = env->FindClass(OBFUSCATE("com/starry/magic/cocos/QlfhHooks"));
        if (local) {
            g_hooksCls = (jclass)env->NewGlobalRef(local);
            g_hooksSetRet = env->GetStaticMethodID(g_hooksCls, "setRet", "(IZ)V");
            g_hooksSetSpeed = env->GetStaticMethodID(g_hooksCls, "setSpeed", "(I)V");
            g_hooksClear = env->GetStaticMethodID(g_hooksCls, "clearAll", "()V");
            env->DeleteLocalRef(local);
        } else {
            env->ExceptionClear();
        }
    }
}

static void SyncJavaRets() {
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls && g_hooksSetRet) {
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 1, g_noAd ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 2, g_dmg100 ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 3, g_inv ? JNI_TRUE : JNI_FALSE);
    }
    if (g_hooksCls && g_hooksSetSpeed) {
        int sp = g_speed.load();
        if (sp < 1) sp = 1;
        if (sp > 10) sp = 10;
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetSpeed, sp);
    }
    if (detach) g_qlfhJvm->DetachCurrentThread();
}

// ---------- JS 拼接 ----------

static const std::string& GetPreamble() {
    static std::string s;
    if (s.empty()) {
        s = "/*starry_qlfh_gxr*/window.__hack_ret=window.__hack_ret||{};";
        s += "window.__gxc=window.__gxc||{t:{},v:{}};";
        s += "window.GXR=function(n){if(window.__hack_ret[n])return!0;";
        s += "var c=window.__gxc,m=Date.now();";
        s += "if(!c.t[n]||m-c.t[n]>250){c.t[n]=m;try{c.v[n]=!!jsb.reflection.";
        s += std::string("callStaticMethod(\"") + kAct + "\",\"get_ret\",\"(I)Z\",n)}catch(e){c.v[n]=!1}}";
        s += "return c.v[n];};";
        s += "window.GXS=function(){var c=window.__gxc,m=Date.now();";
        s += "if(!c.t[90]||m-c.t[90]>250){c.t[90]=m;try{c.v[90]=jsb.reflection.";
        s += std::string("callStaticMethod(\"") + kAct + "\",\"get_speed\",\"()I\")}catch(e){c.v[90]=1}}";
        s += "return c.v[90]||1;};";
    }
    return s;
}

static bool ReplaceAllChecked(std::string& s, const char* from, const char* to,
                              const char* tag) {
    if (!from || !to) return false;
    size_t pos = 0;
    bool changed = false;
    size_t flen = strlen(from);
    while (true) {
        size_t p = s.find(from, pos);
        if (p == std::string::npos) break;
        s.replace(p, flen, to);
        pos = p + strlen(to);
        changed = true;
    }
    if (changed) LOGI("[Qlfh] splice %s", tag);
    return changed;
}

static std::string SpliceQlfhJs(const std::string& srcIn) {
    std::string s(srcIn);
    const std::string& pre = GetPreamble();

    // 1) 包头部注入 GXR/GXS（幂等：含标记则跳过）
    if (s.find("starry_qlfh_gxr") == std::string::npos &&
        s.find("starry_qlfh_spd") == std::string::npos) {
        size_t p = s.find("window.__require");
        if (p != std::string::npos) {
            s.insert(p, pre);
            LOGI("[Qlfh] spliced preamble @%zu", p);
        }
    }

    // 2) 免广告直领：AdvCommand（月卡判定旁路）
    ReplaceAllChecked(s,
        "isNoAd() ? [ 4, l() ]",
        "isNoAd() || window.GXR(1) ? [ 4, l() ]",
        "noAd/advCmd");

    // 3) 免广告直领：商店免广告弹窗
    ReplaceAllChecked(s,
        "isNoAd() ? o() :",
        "isNoAd() || window.GXR(1) ? o() :",
        "noAd/shop");

    // 4) 免广告直领：灯影阁组件（NoAd → 本地广告分支强制）
    ReplaceAllChecked(s,
        "!this._model.NoAd || l.default.SdkMgr.isSupportAd()",
        "window.GXR(1) ? false : (!this._model.NoAd || l.default.SdkMgr.isSupportAd())",
        "noAd/dyg");

    // 5) 真广告改本地：渠道白名单强制不命中
    ReplaceAllChecked(s,
        "indexOf(i.CHANNEL_ID) >= 0",
        "indexOf(i.CHANNEL_ID) >= 0 && !window.GXR(1)",
        "noAd/gate");

    // 6) 本地广告秒过：System(1476) 倒计时 → 1 秒
    ReplaceAllChecked(s,
        "init(d.default.System(1476)",
        "init(window.GXR(1) ? 1 : d.default.System(1476)",
        "noAd/sys1476");

    // 7) 本地广告秒过：LocalAdTime 倒计时 → 1 秒
    ReplaceAllChecked(s,
        "init(this._model.LocalAdTime",
        "init(window.GXR(1) ? 1 : this._model.LocalAdTime",
        "noAd/localTime");

    // 7b) 战斗（客户端模拟器 BattleController）：百倍伤害 / 无敌
    //     f=防守方 GroupModel；E=攻击方 GroupModel（main/index.js 热更版锚点已验证）
    ReplaceAllChecked(s,
        "B.targetDamage = Math.ceil(T);\nf.hp -= B.targetDamage;",
        "B.targetDamage = Math.ceil(T);\n"
        "if(f===this._enemy&&window.GXR&&window.GXR(2))B.targetDamage*=100;\n"
        "if(f===this._player&&window.GXR&&window.GXR(3))B.targetDamage=0;\n"
        "f.hp -= B.targetDamage;",
        "battle/main");

    ReplaceAllChecked(s,
        "s.hp -= this.Damage;",
        "if(s===this._enemy&&window.GXR&&window.GXR(2))this.Damage*=100;\n"
        "if(s===this._player&&window.GXR&&window.GXR(3))this.Damage=0;\n"
        "s.hp -= this.Damage;",
        "battle/fastpath");

    ReplaceAllChecked(s,
        "E.hp -= B.selfDamage;",
        "if(!(E===this._player&&window.GXR&&window.GXR(3)))E.hp -= B.selfDamage;",
        "battle/reflect");

    // 8) 全局变速：引擎 mainLoop deltaTime 缩放
    if (s.find("starry_qlfh_spd") == std::string::npos) {
        ReplaceAllChecked(s,
            "this.calculateDeltaTime(t);",
            "this.calculateDeltaTime(t);this._deltaTime*=window.GXS?window.GXS():1;/*starry_qlfh_spd*/",
            "speed/mainLoop");
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
        LOGI("[Qlfh] se::eval #%d len=%ld file=%s", calls, n,
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

    // 自家产物过引擎时不二次处理
    if (has("starry_qlfh_gxr") || has("starry_qlfh_spd")) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    bool interesting =
        has("window.__require") || has("isNoAd") || has("CHANNEL_ID") ||
        has("System(1476)") || has("_model.NoAd") || has("calculateDeltaTime") ||
        has("targetDamage") || has("getBaseDamage");
    if (!interesting) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    std::string src(script, (size_t)n);
    std::string patched = SpliceQlfhJs(src);
    if (patched == src) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    LOGI("[Qlfh] splice ok len %zu→%zu file=%s", src.size(), patched.size(),
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
            LOGI("[Qlfh] hooked se::evalString");
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

bool Qlfh_TryInit(JavaVM* jvm) {
    g_qlfhJvm = jvm;
    if (!isLibraryLoaded(GameCocos::TARGET_LIB)) return false;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (detach) g_qlfhJvm->DetachCurrentThread();
    }
    TryHookEvalString();
    if (!g_seHooked) return false;
    g_ready = true;
    SyncJavaRets();
    return true;
}

static void* EarlyHookThread(void* arg) {
    g_qlfhJvm = (JavaVM*)arg;
    for (int i = 0; i < 2000; i++) {
        if (isLibraryLoaded(GameCocos::TARGET_LIB)) {
            TryHookEvalString();
            if (g_seHooked) {
                g_ready = true;
                LOGI("[Qlfh] early se hook ok iter=%d", i);
                bool detach = false;
                JNIEnv* env = GetEnv(&detach);
                if (env) {
                    CacheJava(env);
                    SyncJavaRets();
                    if (detach) g_qlfhJvm->DetachCurrentThread();
                }
                return nullptr;
            }
        }
        usleep(20 * 1000);
    }
    LOGW("[Qlfh] early hook timeout");
    return nullptr;
}

void Qlfh_StartEarlyHookThread(JavaVM* jvm) {
    static std::atomic_bool started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, jvm) == 0) pthread_detach(th);
    else started.store(false);
}

bool Qlfh_IsReady() { return g_ready; }

void Qlfh_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == QLFH_FEAT_NO_AD) g_noAd = enabled;
    if (featNum == QLFH_FEAT_DMG100) g_dmg100 = enabled;
    if (featNum == QLFH_FEAT_INVINCIBLE) g_inv = enabled;
    SyncJavaRets();
    LOGI("[Qlfh] feature #%d=%s noAd=%d dmg=%d inv=%d speed=%d", featNum,
         enabled ? "ON" : "OFF", g_noAd ? 1 : 0, g_dmg100 ? 1 : 0,
         g_inv ? 1 : 0, g_speed.load());
}

void Qlfh_OnValueChanged(int featNum, int value) {
    if (featNum == QLFH_FEAT_SPEED) {
        if (value < 1) value = 1;
        if (value > 10) value = 10;
        int prev = g_speed.exchange(value);
        if (prev == value) return;  // Tick 高频轮询：值没变不打日志不重推
        SyncJavaRets();
        LOGI("[Qlfh] speed %d→%d", prev, value);
    }
}

void Qlfh_ForceClear() {
    g_noAd = g_dmg100 = g_inv = false;
    g_speed.store(1);
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_hooksClear) env->CallStaticVoidMethod(g_hooksCls, g_hooksClear);
        if (detach) g_qlfhJvm->DetachCurrentThread();
    }
}

void Qlfh_Tick(const bool* features) {
    if (!features) return;
    g_noAd = features[QLFH_FEAT_NO_AD];
    g_dmg100 = features[QLFH_FEAT_DMG100];
    g_inv = features[QLFH_FEAT_INVINCIBLE];
    if (!g_ready) return;
    if (!g_evalHooked) TryHookEvalString();
    SyncJavaRets();
}
