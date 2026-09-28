/* ================================================================
 *  山海幻境 MOD — 对齐斌哥 GXR/GXD（无敌 GXR(2) / 百倍 GXR(3)）
 *  新增五倍伤害（BOSS）GXR(4)，在 RoleBase.getHurt 扣血前放大真实伤害值
 *  路径：se::ScriptEngine::evalString 明文拼接
 * ================================================================ */

#include "ShhjMod.h"

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

static JavaVM* g_shhjJvm = nullptr;
static bool g_ready = false;
static bool g_evalHooked = false;
static thread_local bool g_inEvalSplice = false;

static jclass g_hooksCls = nullptr;
static jmethodID g_hooksSetRet = nullptr;
static jmethodID g_hooksClear = nullptr;

static volatile bool g_inv = false;
static volatile bool g_dmg100 = false;
static volatile bool g_dmg5 = false;
static bool g_syncedInv = false;
static bool g_syncedDmg100 = false;
static bool g_syncedDmg5 = false;
static std::atomic<int> g_spliceHits{0};

static const char* kAct = "com.chx.shhj.AppActivity";

static const char* kBossGuard =
    "(_.default.ActivityWorldBoss||_.default.guildBoss||_.default.daoyun||_.default.waterFireBoss)";
static const char* kBossGuardSp =
    "(_.default.ActivityWorldBoss || _.default.guildBoss || _.default.daoyun || "
    "_.default.waterFireBoss)";

static JNIEnv* GetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_shhjJvm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_shhjJvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_shhjJvm->AttachCurrentThread(&env, nullptr) != 0) return nullptr;
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
    jclass local = LoadAppClass(env, "com.starry.magic.cocos.ShhjHooks");
    if (!local) local = env->FindClass("com/starry/magic/cocos/ShhjHooks");
    if (local) {
        g_hooksCls = (jclass)env->NewGlobalRef(local);
        g_hooksSetRet = env->GetStaticMethodID(g_hooksCls, "setRet", "(IZ)V");
        g_hooksClear = env->GetStaticMethodID(g_hooksCls, "clearAll", "()V");
        env->DeleteLocalRef(local);
    } else {
        env->ExceptionClear();
    }
}

static void SyncJavaRets(bool force = false) {
    const bool inv = g_inv;
    const bool dmg100 = g_dmg100;
    const bool dmg5 = g_dmg5;
    if (!force && inv == g_syncedInv && dmg100 == g_syncedDmg100 && dmg5 == g_syncedDmg5)
        return;

    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls && g_hooksSetRet) {
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 1, JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 2, inv ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 3, dmg100 ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 4, dmg5 ? JNI_TRUE : JNI_FALSE);
        g_syncedInv = inv;
        g_syncedDmg100 = dmg100;
        g_syncedDmg5 = dmg5;
    }
    if (detach) g_shhjJvm->DetachCurrentThread();
}

static bool ReplaceAll(std::string& s, const char* from, const char* to) {
    size_t fl = strlen(from);
    size_t pos = 0;
    bool any = false;
    while ((pos = s.find(from, pos)) != std::string::npos) {
        s.replace(pos, fl, to);
        pos += strlen(to);
        any = true;
    }
    return any;
}

static std::string SpliceCombatJs(const char* src) {
    std::string s(src);
    bool changed = false;

    static char kPreamble[1400];
    static char kGetHurtFrom[100];
    static char kGetHurtFromSp[100];
    static char kGetHurtTo[1000];
    static char kGetHurtToSp[1000];
    static char kCurHp[700];
    static char kCurHpSp[750];
    static char kCreate[900];
    static char kCreateSp[950];
    static char kInvFrom[200];
    static char kInvTo[320];
    static char kBossFrom[120];
    static char kBossTo[280];
    static bool once = false;
    if (!once) {
        snprintf(
            kPreamble, sizeof(kPreamble),
            "/*starry_shhj_gxr*/window.__hack_ret=window.__hack_ret||{};"
            "window.__gxc=window.__gxc||{t:{},v:{}};"
            "window.GXR=function(n){if(window.__hack_ret[n])return!0;"
            "var c=window.__gxc,m=Date.now();"
            "if(!c.t[n]||m-c.t[n]>250){c.t[n]=m;try{c.v[n]=!!jsb.reflection."
            "callStaticMethod(\"%s\",\"get_ret\",\"(I)Z\",n)}catch(e){c.v[n]=!1}}"
            "return c.v[n];};"
            "window.GXD=function(){return window.GXR(3)?100:1;};"
            "window.GX5=function(m){if(!window.GXR(4)||!m)return 1;"
            "var b=m.ActivityWorldBoss||m.guildBoss||m.daoyun||m.waterFireBoss;"
            "return window.GXR(3)&&!b?1:5;};\n",
            kAct);

        snprintf(kGetHurtFrom, sizeof(kGetHurtFrom),
                 "e.prototype.getHurt=function(t,e,i){");
        snprintf(kGetHurtFromSp, sizeof(kGetHurtFromSp),
                 "e.prototype.getHurt = function(t, e, i) {");
        snprintf(
            kGetHurtTo, sizeof(kGetHurtTo),
            "e.prototype.getHurt=function(t,e,i){if(t&&t[2]<0&&t[3]&&"
            "t[3].camp==d.default.SELFCAMP&&this.camp!=d.default.SELFCAMP){"
            "var _m=window.GX5(E.default);_m>1&&(t[2]=Math.floor(_m*t[2]));}");
        snprintf(
            kGetHurtToSp, sizeof(kGetHurtToSp),
            "e.prototype.getHurt = function(t, e, i) {if (t && t[2] < 0 && t[3] && "
            "t[3].camp == d.default.SELFCAMP && this.camp != d.default.SELFCAMP) { "
            "var _m = window.GX5(E.default); _m > 1 && "
            "(t[2] = Math.floor(_m * t[2])); }");

        snprintf(
            kCurHp, sizeof(kCurHp),
            "this.curHp+=(f<0&&this.view.camp!=a.default.SELFCAMP&&!%s&&window.GXD()>1?"
            "Math.floor(window.GXD()*f):f);",
            kBossGuard);
        snprintf(
            kCurHpSp, sizeof(kCurHpSp),
            "this.curHp += (f < 0 && this.view.camp != a.default.SELFCAMP && !%s && "
            "window.GXD() > 1 ? Math.floor(window.GXD() * f) : f);",
            kBossGuardSp);

        // createNum 伤害参数：压缩 / 带空格
        snprintf(
            kCreate, sizeof(kCreate),
            "createNum(t[0],t[1],t[2]<0&&!(p.default.ActivityWorldBoss||p.default.guildBoss||"
            "p.default.daoyun||p.default.waterFireBoss)&&window.GXD()>1?Math.floor(window.GXD()*"
            "t[2]):t[2],t[4]");
        snprintf(
            kCreateSp, sizeof(kCreateSp),
            "createNum(t[0], t[1], t[2] < 0 && !(p.default.ActivityWorldBoss || "
            "p.default.guildBoss || p.default.daoyun || p.default.waterFireBoss) && "
            "window.GXD() > 1 ? Math.floor(window.GXD() * t[2]) : t[2], t[4]");

        snprintf(kInvFrom, sizeof(kInvFrom),
                 "if(this._hasEffectType[r.default.SKILLEFFECTSUBTYPE77]){");
        snprintf(
            kInvTo, sizeof(kInvTo),
            "if(this._hasEffectType[r.default.SKILLEFFECTSUBTYPE77]||"
            "(this.view.camp==a.default.SELFCAMP&&window.GXR(2))){");

        snprintf(kBossFrom, sizeof(kBossFrom),
                 "this.curHp+f<=0&&this.view.isBoss");
        snprintf(
            kBossTo, sizeof(kBossTo),
            "this.curHp+f*(!%s?window.GXD():1)<=0&&this.view.isBoss", kBossGuard);

        once = true;
    }

    // 1) 主包头注入 GXR/GXD
    if (s.find("starry_shhj_gxr") == std::string::npos &&
        (s.find("window.__require") != std::string::npos ||
         s.find("window.__require=") != std::string::npos)) {
        // 插在 __require 定义之前
        size_t p = s.find("window.__require");
        if (p != std::string::npos) {
            s.insert(p, kPreamble);
            changed = true;
            LOGI("[Shhj] spliced GXR/GXD/GX5 preamble");
        }
    }

    // 2) RoleBase 在真实扣血 / 跳字 / 伤害累计前放大五倍
    if (s.find("window.GX5(") == std::string::npos) {
        if (ReplaceAll(s, kGetHurtFrom, kGetHurtTo)) {
            changed = true;
            LOGI("[Shhj] spliced getHurt dmg5 (compact)");
        }
        if (ReplaceAll(s, kGetHurtFromSp, kGetHurtToSp)) {
            changed = true;
            LOGI("[Shhj] spliced getHurt dmg5 (spaced)");
        }
    }

    // 3) 扣血（仅未拼接过的原版 `curHp += f`）
    if (ReplaceAll(s, "this.curHp+=f;", kCurHp)) {
        changed = true;
        LOGI("[Shhj] spliced curHp (compact)");
    }
    if (ReplaceAll(s, "this.curHp += f;", kCurHpSp)) {
        changed = true;
        LOGI("[Shhj] spliced curHp (spaced)");
    }

    // 4) 跳字 createNum
    if (ReplaceAll(s, "createNum(t[0],t[1],t[2],t[4]", kCreate)) {
        changed = true;
        LOGI("[Shhj] spliced createNum (compact)");
    }
    if (ReplaceAll(s, "createNum(t[0], t[1], t[2], t[4]", kCreateSp)) {
        changed = true;
        LOGI("[Shhj] spliced createNum (spaced)");
    }

    // 5) 无敌 tHurt
    if (s.find("window.GXR(2)") == std::string::npos) {
        if (ReplaceAll(s, kInvFrom, kInvTo)) {
            changed = true;
            LOGI("[Shhj] spliced tHurt inv");
        }
        // spaced variant
        const char* fromSp =
            "if (this._hasEffectType[r.default.SKILLEFFECTSUBTYPE77]) {";
        const char* toSp =
            "if (this._hasEffectType[r.default.SKILLEFFECTSUBTYPE77] || "
            "(this.view.camp == a.default.SELFCAMP && window.GXR(2))) {";
        if (ReplaceAll(s, fromSp, toSp)) {
            changed = true;
            LOGI("[Shhj] spliced tHurt inv (spaced)");
        }
    }

    // 6) Boss 斩杀辅助倍率
    if (s.find("this.view.isBoss") != std::string::npos &&
        s.find("window.GXD()") == std::string::npos) {
        if (ReplaceAll(s, kBossFrom, kBossTo)) {
            changed = true;
            LOGI("[Shhj] spliced boss kill check");
        }
        if (ReplaceAll(s, "this.curHp + f <= 0 && this.view.isBoss",
                       "this.curHp + f * (!(_.default.ActivityWorldBoss || "
                       "_.default.guildBoss || _.default.daoyun || "
                       "_.default.waterFireBoss) ? window.GXD() : 1) <= 0 && "
                       "this.view.isBoss")) {
            changed = true;
            LOGI("[Shhj] spliced boss kill check (spaced)");
        }
    }

    (void)changed;
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
        LOGI("[Shhj] se::eval #%d len=%ld file=%s", calls, n,
             fileName ? fileName : "(null)");
    }

    if (n < 24 || n > 48 * 1024 * 1024) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    auto has = [&](const char* needle) -> bool {
        size_t nl = strlen(needle);
        if ((size_t)n < nl) return false;
        return memmem(script, (size_t)n, needle, nl) != nullptr;
    };

    if (has("starry_shhj_")) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    bool interesting = has("window.__require") || has("curHp") || has("tHurt") ||
                       has("createNum") || has("SKILLEFFECTSUBTYPE77") ||
                       has("SELFCAMP");
    if (!interesting) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    std::string src(script, (size_t)n);
    std::string patched = SpliceCombatJs(src.c_str());
    if (patched == src) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    int hits = ++g_spliceHits;
    LOGI("[Shhj] splice #%d len %zu→%zu file=%s", hits, src.size(), patched.size(),
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
            LOGI("[Shhj] hooked se::evalString");
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

bool Shhj_TryInit(JavaVM* jvm) {
    g_shhjJvm = jvm;
    if (!isLibraryLoaded(GameCocos::TARGET_LIB)) return false;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (detach) g_shhjJvm->DetachCurrentThread();
    }
    TryHookEvalString();
    if (!g_seHooked) return false;
    g_ready = true;
    SyncJavaRets(true);
    return true;
}

static void* EarlyHookThread(void* arg) {
    g_shhjJvm = (JavaVM*)arg;
    for (int i = 0; i < 2000; i++) {
        if (isLibraryLoaded(GameCocos::TARGET_LIB)) {
            TryHookEvalString();
            if (g_seHooked) {
                g_ready = true;
                LOGI("[Shhj] early se hook ok iter=%d", i);
                bool detach = false;
                JNIEnv* env = GetEnv(&detach);
                if (env) {
                    CacheJava(env);
                    SyncJavaRets(true);
                    if (detach) g_shhjJvm->DetachCurrentThread();
                }
                return nullptr;
            }
        }
        usleep(20 * 1000);
    }
    LOGW("[Shhj] early hook timeout");
    return nullptr;
}

void Shhj_StartEarlyHookThread(JavaVM* jvm) {
    static std::atomic_bool started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, jvm) == 0) pthread_detach(th);
    else started.store(false);
}

bool Shhj_IsReady() { return g_ready; }

void Shhj_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == SHHJ_FEAT_INVINCIBLE) g_inv = enabled;
    if (featNum == SHHJ_FEAT_DMG100) g_dmg100 = enabled;
    if (featNum == SHHJ_FEAT_DMG5) g_dmg5 = enabled;
    SyncJavaRets(true);
    LOGI("[Shhj] feature #%d=%s inv=%d dmg100=%d dmg5=%d", featNum,
         enabled ? "ON" : "OFF", g_inv ? 1 : 0, g_dmg100 ? 1 : 0, g_dmg5 ? 1 : 0);
}

void Shhj_ForceClear() {
    g_inv = g_dmg100 = g_dmg5 = false;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_hooksClear) env->CallStaticVoidMethod(g_hooksCls, g_hooksClear);
        g_syncedInv = false;
        g_syncedDmg100 = false;
        g_syncedDmg5 = false;
        if (detach) g_shhjJvm->DetachCurrentThread();
    }
}

void Shhj_Tick(const bool* features) {
    if (!features) return;
    g_inv = features[SHHJ_FEAT_INVINCIBLE];
    g_dmg100 = features[SHHJ_FEAT_DMG100];
    g_dmg5 = features[SHHJ_FEAT_DMG5];
    if (!g_ready) return;
    if (!g_evalHooked) TryHookEvalString();
    // 开关未变则跳过 JNI，避免菜单 tick 空转
    SyncJavaRets(false);
}
