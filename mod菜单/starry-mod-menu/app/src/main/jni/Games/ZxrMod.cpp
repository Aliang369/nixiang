/* ================================================================
 *  镇邪人 MOD
 *
 *  战斗主路径：静态 patch 解密后的热更 src bundle（index.<ver>.jsc）并钉进包内，
 *  由 jsb-engine 的「包内命中优先」规则加载。
 *  native se::evalString hook 是热更兜底：**按内容指纹判定**（体积窗口 +
 * 三个战斗函数名 + 三个锚点各命中一次），不吃 bundle 版本号、不吃落盘文件名——
 *  热更缓存是 gamecaches/<数字>.jsc，按文件名白名单永远匹配不上（2026-10-01 事故）。
 *  命中即在内存中注入同源补丁；锚点不足直接放行原脚本，执行失败先停用再回退原脚本。
 * ================================================================ */

#include "ZxrMod.h"
#include "ZxrPatchJs.h"
#include "ZxrDerive.h"

#include <atomic>
#include <cstdio>
#include <cstring>
#include <mutex>
#include <string>
#include <pthread.h>
#include <unistd.h>

#if defined(__linux__) || defined(__ANDROID__)
#include <string.h> // memmem
#endif

#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Dobby/dobby.h"

static JavaVM* g_zxrJvm = nullptr;
static std::mutex g_hookMutex;
static std::mutex g_javaMutex;

static std::atomic<bool> g_ready{false};
static std::atomic<bool> g_seHooked{false};
static std::atomic<bool> g_spliceDisabled{false};
static std::atomic<int> g_spliceHits{0};
static std::atomic<int> g_seCalls{0};
static std::atomic<int> g_speedLvl{1};
static std::atomic<int> g_hotState{0};  // 0=未知 1=热更补丁已挂上 2=锚点未命中(功能会静默失效)
static thread_local bool t_inEvalSplice = false;

static std::atomic<bool> g_seckill{false};
static std::atomic<bool> g_inv{false};
static std::atomic<bool> g_atkSpd{false};
static std::atomic<bool> g_aoe{false};
static std::atomic<bool> g_noAd{false};

static jclass g_bridgeCls = nullptr;
static jmethodID g_bridgeEval = nullptr;
static jclass g_hooksCls = nullptr;
static jmethodID g_hooksSetRet = nullptr;
static jmethodID g_hooksSetNoAd = nullptr;
static jmethodID g_hooksClear = nullptr;
static jmethodID g_hooksLogStatus = nullptr;

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
    if (!env || !name) return nullptr;
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
    jmethodID getCl = appCls
        ? env->GetMethodID(appCls, "getClassLoader", "()Ljava/lang/ClassLoader;")
        : nullptr;
    jobject cl = getCl ? env->CallObjectMethod(app, getCl) : nullptr;
    if (appCls) env->DeleteLocalRef(appCls);
    env->DeleteLocalRef(app);
    if (!cl) {
        env->ExceptionClear();
        return nullptr;
    }
    jclass clCls = env->GetObjectClass(cl);
    jmethodID loadClass = clCls
        ? env->GetMethodID(clCls, "loadClass", "(Ljava/lang/String;)Ljava/lang/Class;")
        : nullptr;
    jclass cls = nullptr;
    if (loadClass) {
        jstring jname = env->NewStringUTF(name);
        if (jname) {
            cls = (jclass)env->CallObjectMethod(cl, loadClass, jname);
            if (env->ExceptionCheck()) {
                env->ExceptionClear();
                cls = nullptr;
            }
            env->DeleteLocalRef(jname);
        }
    }
    if (clCls) env->DeleteLocalRef(clCls);
    env->DeleteLocalRef(cl);
    return cls;
}

static void CacheJava(JNIEnv* env) {
    if (!env) return;
    std::lock_guard<std::mutex> lock(g_javaMutex);
    if (!g_bridgeCls) {
        jclass local = LoadAppClass(env, "com.starry.magic.cocos.CocosJsBridge");
        if (!local) {
            local = env->FindClass(OBFUSCATE("com/starry/magic/cocos/CocosJsBridge"));
        }
        if (local) {
            g_bridgeCls = (jclass)env->NewGlobalRef(local);
            g_bridgeEval = env->GetStaticMethodID(
                g_bridgeCls, "evalOnGLThread", "(Ljava/lang/String;)V");
            env->DeleteLocalRef(local);
            if (env->ExceptionCheck()) env->ExceptionClear();
        } else {
            env->ExceptionClear();
        }
    }
    if (!g_hooksCls) {
        jclass local = LoadAppClass(env, "com.starry.magic.cocos.ZxrHooks");
        if (!local) {
            local = env->FindClass(OBFUSCATE("com/starry/magic/cocos/ZxrHooks"));
        }
        if (local) {
            g_hooksCls = (jclass)env->NewGlobalRef(local);
            g_hooksSetRet = env->GetStaticMethodID(g_hooksCls, "setRet", "(IZ)V");
            g_hooksSetNoAd = env->GetStaticMethodID(g_hooksCls, "setNoAd", "(Z)V");
            g_hooksClear = env->GetStaticMethodID(g_hooksCls, "clearAll", "()V");
            g_hooksLogStatus = env->GetStaticMethodID(
                g_hooksCls, "logStatus", "(Ljava/lang/String;)V");
            env->DeleteLocalRef(local);
            if (env->ExceptionCheck()) env->ExceptionClear();
        } else {
            env->ExceptionClear();
        }
    }
}

static void EvalJs(const char* js) {
    if (!js || !*js) return;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_bridgeCls && g_bridgeEval) {
        jstring jjs = env->NewStringUTF(js);
        if (jjs) {
            env->CallStaticVoidMethod(g_bridgeCls, g_bridgeEval, jjs);
            env->DeleteLocalRef(jjs);
        }
    }
    if (env->ExceptionCheck()) env->ExceptionClear();
    if (detach) g_zxrJvm->DetachCurrentThread();
}

static void SyncJavaRets() {
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls && g_hooksSetRet) {
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, ZXR_FEAT_SECKILL,
                                  g_seckill.load() ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, ZXR_FEAT_INVINCIBLE,
                                  g_inv.load() ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, ZXR_FEAT_ATK_SPEED,
                                  g_atkSpd.load() ? JNI_TRUE : JNI_FALSE);
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, ZXR_FEAT_AOE_KILL,
                                  g_aoe.load() ? JNI_TRUE : JNI_FALSE);
    }
    if (g_hooksCls && g_hooksSetNoAd) {
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetNoAd,
                                  g_noAd.load() ? JNI_TRUE : JNI_FALSE);
    }
    if (env->ExceptionCheck()) env->ExceptionClear();
    if (detach) g_zxrJvm->DetachCurrentThread();
}

static void SyncJsState() {
    if (!g_ready.load()) return;
    char buf[2048];
    snprintf(buf, sizeof(buf),
             "(function(){try{var g=typeof globalThis!=='undefined'?globalThis:window;"
             "var s=g.__starryZxrState=g.__starryZxrState||{};s.ret=s.ret||{};"
             "s.ret[1]=%d;s.ret[2]=%d;s.ret[3]=%d;s.ret[4]=%d;"
             "if(!s.logged){s.logged=1;try{jsb.reflection.callStaticMethod("
             "\"com.starry.magic.cocos.ZxrHooks\",\"logStatus\",\"(Ljava/lang/String;)V\","
             "\"zxr jsstate patched=\"+(typeof window.__starryZxrOn==='function'));}catch(e){}}"
             "if(!s.bundleGuard&&typeof cc!=='undefined'&&cc.assetManager){s.bundleGuard=1;try{var am=cc.assetManager;if(am&&am.downloader){am.downloader.bundleVers=am.downloader.bundleVers||{};am.downloader.bundleVers.src=\"d5507\";var oa=am.addRemoteBundle;if(typeof oa===\"function\"){am.addRemoteBundle=function(n){if(n===\"src\")return;return oa.apply(this,arguments);};}var ol=am.loadBundle;if(typeof ol===\"function\"){am.loadBundle=function(n,o,c){if(n===\"src\"){try{am.downloader.bundleVers.src=\"d5507\";}catch(e){}}return ol.apply(this,arguments);};}}}catch(e){}"
             "}catch(e){}})();",
             g_seckill.load() ? 1 : 0, g_inv.load() ? 1 : 0,
             g_atkSpd.load() ? 1 : 0, g_aoe.load() ? 1 : 0);
    EvalJs(buf);
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

static bool MemHas(const char* data, size_t size, const char* needle) {
    if (!data || !needle) return false;
    const size_t n = strlen(needle);
    return n > 0 && size >= n && memmem(data, size, needle, n) != nullptr;
}

static void ReportHotUnpatched(const std::string& why) {
    g_hotState.store(2);
    static std::atomic<bool> logged{false};
    if (logged.exchange(true)) return;
    LOGW("[Zxr] hot-patch MISS: %s", why.c_str());
    // 双通道告警：既进 Mod_Menu 日志，也走 Java ZxrHooks.logStatus（只打日志，不改状态）
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    CacheJava(env);
    if (g_hooksCls && g_hooksLogStatus) {
        char msg[640];
        snprintf(msg, sizeof(msg),
                 "[Zxr] 战斗补丁未命中热更脚本，四项战斗功能将静默失效（请重新出包）: %s",
                 why.c_str());
        jstring jmsg = env->NewStringUTF(msg);
        if (jmsg) {
            env->CallStaticVoidMethod(g_hooksCls, g_hooksLogStatus, jmsg);
            env->DeleteLocalRef(jmsg);
        }
    }
    if (env->ExceptionCheck()) env->ExceptionClear();
    if (detach) g_zxrJvm->DetachCurrentThread();
}

struct ZxrHookBinding {
    const char* name;
    bool (*derive)(const std::string&, zxrderive::Plan*, std::string*);
    const ZxrPatchVariant* variants;
    int variantCount;
};

// 先结构派生（不认标识符），再逐条试历史锚点变体；两条都不成就报原因。
static bool ApplyOneHook(std::string& script, const ZxrHookBinding& hb, bool* derivedOut) {
    std::string why;
    if (hb.derive) {
        zxrderive::Plan plan;
        std::string dwhy;
        if (hb.derive(script, &plan, &dwhy) && !plan.anchor.empty()) {
            const size_t first = script.find(plan.anchor);
            if (first != std::string::npos &&
                script.find(plan.anchor, first + plan.anchor.size()) == std::string::npos) {
                script.replace(first, plan.anchor.size(), plan.anchor + plan.insert);
                if (derivedOut) *derivedOut = true;
                return true;
            }
            why = "derived anchor hits != 1";
        } else {
            why = dwhy.empty() ? "derive failed" : dwhy;
        }
    } else {
        why = "no derive fn";
    }
    for (int i = 0; i < hb.variantCount; ++i) {
        const char* a = hb.variants[i].anchor;
        if (!a || !*a) continue;
        const size_t n = strlen(a);
        const size_t pos = script.find(a);
        if (pos == std::string::npos) continue;
        if (script.find(a, pos + n) != std::string::npos) continue;  // 多命中不赌
        script.replace(pos, n, hb.variants[i].replacement);
        LOGI("[Zxr] hotfallback variant hit: %s #%d", hb.name ? hb.name : "?", i);
        return true;
    }
    LOGW("[Zxr] hotfallback anchor missing: %s (%s)", hb.name ? hb.name : "?", why.c_str());
    return false;
}

static bool BuildHotPatch(const char* src, size_t size, std::string* out,
                          std::string* whyOut) {
    if (!src || !out) return false;
    try {
        std::string script;
        script.reserve(size + strlen(kZxrPatchPrefix) + 4096);
        script.append(src, size);
        script.append("\n");
        script.append(kZxrPatchPrefix);
        const ZxrHookBinding hooks[] = {
            {"damage_and_invincible", zxrderive::DeriveDamage,
             kZxrVariants_damage_and_invincible, kZxrVariants_damage_and_invincible_n},
            {"attack_speed", zxrderive::DeriveAttackSpeed,
             kZxrVariants_attack_speed, kZxrVariants_attack_speed_n},
            {"full_screen_kill", zxrderive::DeriveFullScreenKill,
             kZxrVariants_full_screen_kill, kZxrVariants_full_screen_kill_n},
        };
        int derived = 0;
        std::string fails;
        for (const auto& hb : hooks) {
            bool byDerive = false;
            if (!ApplyOneHook(script, hb, &byDerive)) {
                if (!fails.empty()) fails += "; ";
                fails += hb.name ? hb.name : "?";
            } else if (byDerive) {
                ++derived;
            }
        }
        if (!fails.empty()) {
            if (whyOut) *whyOut = "hooks failed: " + fails;
            return false;
        }
        LOGI("[Zxr] hotfallback plan ok: derive=%d/3 variantTbl=%d", derived,
             kZxrVariants_full_screen_kill_n);
        out->swap(script);
        return true;
    } catch (...) {
        LOGW("[Zxr] hotfallback build exception");
        if (whyOut) *whyOut = "build exception";
        return false;
    }
}

typedef bool (*SeEvalString_t)(void*, const char*, long, void*, const char*);
static SeEvalString_t orig_seEval = nullptr;

static bool hooked_seEval(void* thiz, const char* script, long length, void* ret,
                          const char* fileName) {
    if (!orig_seEval) return false;
    if (!script || t_inEvalSplice || g_spliceDisabled.load()) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    long n = length;
    if (n < 0) n = (long)strlen(script);
    if (n < 1024 * 1024 || n > 48 * 1024 * 1024) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    // 不按文件名/版本号放行：热更 bundle 在设备上落盘为
    // files/gamecaches/<数字>.jsc，evalString 收到的 fileName 既不含 index.<ver>
    // 也不是包内路径，旧白名单会把热更脚本整个挡掉（2026-10-01 事故根因）。
    // 判定完全交给下面的内容指纹。
    int calls = ++g_seCalls;
#if defined(STARRY_TRACE)
    if (calls <= 8 || calls % 200 == 0) {
        LOGI("[Zxr] se::eval #%d len=%ld file=%s", calls, n,
             fileName ? fileName : "(null)");
    }
#else
    (void)calls;
#endif

    if (MemHas(script, (size_t)n, kZxrPatchMarker)) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    if (!MemHas(script, (size_t)n, "FightChangeEntityHp") ||
        !MemHas(script, (size_t)n, "DamageCommonCalcFinal") ||
        !MemHas(script, (size_t)n, "CalcFinalAtkSpeedValue")) {
        return orig_seEval(thiz, script, length, ret, fileName);
    }

    std::string patched;
    std::string why;
    if (!BuildHotPatch(script, (size_t)n, &patched, &why)) {
        g_spliceDisabled.store(true);
        LOGW("[Zxr] hotfallback disabled: anchor set incomplete");
        ReportHotUnpatched(why);
        return orig_seEval(thiz, script, length, ret, fileName);
    }
    g_hotState.store(1);

    int hit = ++g_spliceHits;
    if (hit <= 5) {
        LOGI("[Zxr] hotfallback splice #%d len=%ld->%zu file=%s", hit, n,
             patched.size(), fileName ? fileName : "(null)");
    }
    t_inEvalSplice = true;
    bool ok = false;
    try {
        ok = orig_seEval(thiz, patched.c_str(), (long)patched.size(), ret, fileName);
    } catch (...) {
        LOGW("[Zxr] hotfallback eval exception");
        ok = false;
    }
    t_inEvalSplice = false;
    if (!ok) {
        // 补丁脚本跑不起来就停用兜底，并立刻用原脚本重跑一次：
        // 宁可战斗功能不生效，也不能让整个 bundle 加载失败（会黑屏）。
        g_spliceDisabled.store(true);
        LOGW("[Zxr] hotfallback splice eval failed, retry original script");
        t_inEvalSplice = true;
        try {
            ok = orig_seEval(thiz, script, length, ret, fileName);
        } catch (...) {
            ok = false;
        }
        t_inEvalSplice = false;
    }
    return ok;
}

static bool TryHookEvalString() {
    if (g_seHooked.load() || g_spliceDisabled.load()) return g_seHooked.load();
    std::lock_guard<std::mutex> lock(g_hookMutex);
    if (g_seHooked.load()) return true;
    const char* lib = GameCocos::TARGET_LIB;
    if (!isLibraryLoaded(lib)) return false;
    void* sym = DobbySymbolResolver(lib, GameCocos::Symbols::SE_EVAL_STRING);
    if (!sym) {
        LOGW("[Zxr] se::evalString symbol missing");
        return false;
    }
    if (DobbyHook(sym, (dobby_dummy_func_t)hooked_seEval,
                  (dobby_dummy_func_t*)&orig_seEval) != 0) {
        LOGW("[Zxr] DobbyHook se::evalString failed");
        return false;
    }
    g_seHooked.store(true);
    LOGI("[Zxr] hooked se::evalString for hot-update fallback");
    return true;
}

bool Zxr_TryInit(JavaVM* jvm) {
    if (jvm) g_zxrJvm = jvm;
    if (!isLibraryLoaded(GameCocos::TARGET_LIB)) return false;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (detach) g_zxrJvm->DetachCurrentThread();
    }
    if (!TryHookEvalString()) return false;
    g_ready.store(true);
    SyncJavaRets();
    SyncJsState();
    SyncSpeedJs();
    LOGI("[Zxr] init ok se=%d splice=%d seCalls=%d",
         g_seHooked.load() ? 1 : 0, g_spliceHits.load(), g_seCalls.load());
    return true;
}

static void* EarlyHookThread(void* arg) {
    JavaVM* jvm = (JavaVM*)arg;
    if (jvm) g_zxrJvm = jvm;
    LOGI("[Zxr] early hook thread start");
    for (int i = 0; i < 3000; i++) { // ~60s @ 20ms
        if (isLibraryLoaded(GameCocos::TARGET_LIB) && TryHookEvalString()) {
            g_ready.store(true);
            SyncJavaRets();
            SyncJsState();
            SyncSpeedJs();
            LOGI("[Zxr] early hook ready iter=%d", i);
            return nullptr;
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

bool Zxr_IsReady() { return g_ready.load(); }

void Zxr_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case ZXR_FEAT_SECKILL: g_seckill.store(enabled); break;
        case ZXR_FEAT_INVINCIBLE: g_inv.store(enabled); break;
        case ZXR_FEAT_ATK_SPEED: g_atkSpd.store(enabled); break;
        case ZXR_FEAT_AOE_KILL: g_aoe.store(enabled); break;
        case ZXR_FEAT_NO_AD: g_noAd.store(enabled); break;
        case ZXR_FEAT_SPEEDUP:
            if (!enabled) g_speedLvl.store(1);
            break;
        default: break;
    }
    SyncJavaRets();
    SyncJsState();
    SyncSpeedJs();
    LOGI("[Zxr] feature #%d=%s noAd=%d splice=%d hot=%d", featNum,
         enabled ? "ON" : "OFF", g_noAd.load() ? 1 : 0, g_spliceHits.load(),
         g_hotState.load());
}

void Zxr_OnValueChanged(int featNum, int value) {
    if (featNum != ZXR_FEAT_SPEEDUP) return;
    if (value < 1) value = 1;
    if (value > 5) value = 5;
    int prev = g_speedLvl.exchange(value);
    if (prev == value) return;
    SyncSpeedJs();
    LOGI("[Zxr] speedLvl=%d", value);
}

void Zxr_ForceClear() {
    g_seckill.store(false);
    g_inv.store(false);
    g_atkSpd.store(false);
    g_aoe.store(false);
    g_noAd.store(false);
    g_speedLvl.store(1);
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_hooksClear) {
            env->CallStaticVoidMethod(g_hooksCls, g_hooksClear);
        }
        if (env->ExceptionCheck()) env->ExceptionClear();
        if (detach) g_zxrJvm->DetachCurrentThread();
    }
    SyncJsState();
    SyncSpeedJs();
}

void Zxr_Tick(const bool* features) {
    if (!features) return;
    bool s = features[ZXR_FEAT_SECKILL];
    bool i = features[ZXR_FEAT_INVINCIBLE];
    bool a = features[ZXR_FEAT_ATK_SPEED];
    bool o = features[ZXR_FEAT_AOE_KILL];
    bool ad = features[ZXR_FEAT_NO_AD];
    bool dirty = (s != g_seckill.load() || i != g_inv.load() ||
                  a != g_atkSpd.load() || o != g_aoe.load() ||
                  ad != g_noAd.load());
    g_seckill.store(s);
    g_inv.store(i);
    g_atkSpd.store(a);
    g_aoe.store(o);
    g_noAd.store(ad);
    if (!g_ready.load()) {
        TryHookEvalString();
        return;
    }
    if (dirty) {
        SyncJavaRets();
        SyncJsState();
        LOGI("[Zxr] feature sync dirty splice=%d hot=%d", g_spliceHits.load(),
             g_hotState.load());
    }
    static int tickN = 0;
    if ((++tickN % 40) == 0 && g_speedLvl.load() > 1) {
        SyncSpeedJs();
#if defined(STARRY_TRACE)
        LOGI("[Zxr] tick splice=%d seCalls=%d speedLvl=%d",
             g_spliceHits.load(), g_seCalls.load(), g_speedLvl.load());
#endif
    }
}
