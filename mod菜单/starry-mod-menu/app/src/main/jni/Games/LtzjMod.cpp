/* ================================================================
 *  雷霆战机：集结 MOD — Cocos Creator3 (libcocos.so)
 *  明文挂点（index.f8365.jsc 解密）：
 *    百倍 → EnemyPlane/Boss.prototype.hurt 伤害 ×100
 *    无敌 → XPlane.prototype.hurt 直接 return false
 *    免广告 → ATAndroidRewardedVideoJS.showAd → LtzjHooks.fakeRewardShow
 *  旗标：LtzjHooks.getRet(1/2/3)（zip 注入无 AppActivity 时勿依赖 get_ret）
 *  ACE：grant 不自动 hook；满 60s 且开功能后再 eval（无 Dobby）
 * ================================================================ */

#include "LtzjMod.h"
#include "Includes/obfuscate.h"

#include <atomic>
#include <cstdio>
#include <cstring>
#include <ctime>
#include <pthread.h>
#include <time.h>
#include <unistd.h>

#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Dobby/dobby.h"

/** 对齐斌哥：菜单可先亮，战斗 JS / Dobby 等 ACE 早期窗过后再动（约 60s） */
static constexpr int kAceSafeDelaySec = 60;

static JavaVM* g_ltzjJvm = nullptr;
static bool g_ready = false;
static bool g_jniHooked = false;
static std::atomic<int64_t> g_aceGateEpochSec{0};
static std::atomic_bool g_combatArmed{false};
static std::atomic_bool g_gateLogOnce{false};

static jclass g_hooksCls = nullptr;
static jmethodID g_hooksSetRet = nullptr;
static jmethodID g_hooksClear = nullptr;

static volatile bool g_dmg100 = false;
static volatile bool g_inv = false;
static volatile bool g_noAd = false;

static int64_t MonoSec() {
    struct timespec ts {};
    if (clock_gettime(CLOCK_MONOTONIC, &ts) != 0) return 0;
    return (int64_t)ts.tv_sec;
}

static void ArmAceGate() {
    int64_t expected = 0;
    int64_t now = MonoSec();
    if (now <= 0) now = 1;
    if (g_aceGateEpochSec.compare_exchange_strong(expected, now)) {
        LOGI("[Ltzj] ACE gate armed: combat hooks after %ds", kAceSafeDelaySec);
    }
}

static int AceRemainSec() {
    int64_t ep = g_aceGateEpochSec.load();
    if (ep == 0) {
        ArmAceGate();
        ep = g_aceGateEpochSec.load();
    }
    int64_t now = MonoSec();
    int left = kAceSafeDelaySec - (int)(now - ep);
    return left > 0 ? left : 0;
}

int Ltzj_AceRemainSec() { return AceRemainSec(); }

static bool AceSafeForCombat() {
    return AceRemainSec() == 0;
}

static JNIEnv* GetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_ltzjJvm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_ltzjJvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_ltzjJvm->AttachCurrentThread(&env, nullptr) != 0) return nullptr;
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
    jclass local = LoadAppClass(env, "com.starry.magic.cocos.LtzjHooks");
    if (!local) local = env->FindClass(OBFUSCATE("com/starry/magic/cocos/LtzjHooks"));
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
        env->CallStaticVoidMethod(g_hooksCls, g_hooksSetRet, 3, g_noAd ? JNI_TRUE : JNI_FALSE);
    }
    if (detach) g_ltzjJvm->DetachCurrentThread();
}

static void EvalJs(const char* js) {
    if (!g_ltzjJvm || !js) return;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    jclass cls = env->FindClass(OBFUSCATE("com/starry/magic/cocos/CocosJsBridge"));
    if (!cls) {
        env->ExceptionClear();
        if (detach) g_ltzjJvm->DetachCurrentThread();
        return;
    }
    jmethodID mid = env->GetStaticMethodID(cls, "evalOnGLThread", "(Ljava/lang/String;)V");
    if (mid) {
        jstring jjs = env->NewStringUTF(js);
        env->CallStaticVoidMethod(cls, mid, jjs);
        env->DeleteLocalRef(jjs);
    }
    env->DeleteLocalRef(cls);
    if (detach) g_ltzjJvm->DetachCurrentThread();
}

/* 解密明文挂点：XPlane / EnemyPlane / Boss / ATAndroidRewardedVideoJS */
static const char* kCombatJs = R"JS(
(function(){
  var g=(typeof globalThis!=="undefined")?globalThis:((typeof window!=="undefined")?window:this);
  function ret(i){
    try{
      if(typeof jsb==="undefined"||!jsb.reflection) return false;
      try{
        if(jsb.reflection.callStaticMethod("com.starry.magic.cocos.LtzjHooks","getRet","(I)Z",i))
          return true;
      }catch(e0){}
      try{
        return !!jsb.reflection.callStaticMethod("com.cocos.game.AppActivity","get_ret","(I)Z",i);
      }catch(e1){}
    }catch(e){}
    return false;
  }
  function log(msg){
    try{
      if(typeof jsb!=="undefined"&&jsb.reflection)
        jsb.reflection.callStaticMethod("com.starry.magic.cocos.LtzjHooks","logStatus","(Ljava/lang/String;)V",String(msg));
    }catch(e){}
  }
  g.__starryDmg100=!!(ret(1)||g.__starryDmg100);
  g.__starryInv=!!(ret(2)||g.__starryInv);
  g.__starryNoAd=!!(ret(3)||g.__starryNoAd);
  function getExp(modName, expName){
    try{
      if(typeof System==="undefined"||!System.get) return null;
      var m=System.get("chunks:///_virtual/"+modName+".ts");
      if(!m) return null;
      return m[expName]||null;
    }catch(e){return null;}
  }
  function hookPlayerInv(){
    var C=getExp("XPlane","XPlane");
    var P=C&&C.prototype;
    if(!P||typeof P.hurt!=="function") return 0;
    if(P.hurt.__starryInv) return 1;
    var orig=P.hurt;
    P.hurt=function(t,e,i){
      if(g.__starryInv||ret(2)) return false;
      return orig.apply(this,arguments);
    };
    P.hurt.__starryInv=1;
    return 1;
  }
  function hookEnemyDmg(modName, expName){
    var C=getExp(modName, expName);
    var P=C&&C.prototype;
    if(!P||typeof P.hurt!=="function") return 0;
    if(P.hurt.__starryDmg) return 1;
    var orig=P.hurt;
    P.hurt=function(t,e,i){
      if((g.__starryDmg100||ret(1))&&typeof t==="number") t=t*100;
      return orig.call(this,t,e,i);
    };
    P.hurt.__starryDmg=1;
    return 1;
  }
  function fakeAd(pid){
    try{
      jsb.reflection.callStaticMethod(
        "com.starry.magic.cocos.LtzjHooks","fakeRewardShow","(Ljava/lang/String;)V",
        String(pid||""));
    }catch(e){}
  }
  function hookAds(){
    var n=0;
    var o=getExp("ATAndroidRewardedVideoTS","ATAndroidRewardedVideoJS");
    if(o&&typeof o.showAd==="function"){
      if(o.showAd.__starryAd) n++;
      else {
        var s1=o.showAd, s2=o.showAdInScenario, rdy=o.hasAdReady;
        o.showAd=function(pid){
          if(g.__starryNoAd||ret(3)){ fakeAd(pid); return; }
          return s1.apply(this,arguments);
        };
        o.showAd.__starryAd=1;
        if(typeof s2==="function"){
          o.showAdInScenario=function(pid,scene){
            if(g.__starryNoAd||ret(3)){ fakeAd(pid); return; }
            return s2.apply(this,arguments);
          };
          o.showAdInScenario.__starryAd=1;
        }
        if(typeof rdy==="function"){
          o.hasAdReady=function(pid){
            if(g.__starryNoAd||ret(3)) return true;
            return rdy.apply(this,arguments);
          };
          o.hasAdReady.__starryAd=1;
        }
        n++;
      }
    }
    var sdk=getExp("ATRewardedVideoTSSDK","ATRewardedVideoSDK");
    if(sdk&&typeof sdk.showAd==="function"){
      if(sdk.showAd.__starryAd) n++;
      else {
        var ss=sdk.showAd, ss2=sdk.showAdInScenario, sr=sdk.hasAdReady;
        sdk.showAd=function(pid){
          if(g.__starryNoAd||ret(3)){ fakeAd(pid); return; }
          return ss.apply(this,arguments);
        };
        sdk.showAd.__starryAd=1;
        if(typeof ss2==="function"){
          sdk.showAdInScenario=function(pid,scene){
            if(g.__starryNoAd||ret(3)){ fakeAd(pid); return; }
            return ss2.apply(this,arguments);
          };
          sdk.showAdInScenario.__starryAd=1;
        }
        if(typeof sr==="function"){
          sdk.hasAdReady=function(pid){
            if(g.__starryNoAd||ret(3)) return true;
            return sr.apply(this,arguments);
          };
          sdk.hasAdReady.__starryAd=1;
        }
        n++;
      }
    }
    return n;
  }
  var hooked=0;
  try{ hooked+=hookPlayerInv(); }catch(e){}
  try{ hooked+=hookEnemyDmg("EnemyPlane","EnemyPlane"); }catch(e){}
  try{ hooked+=hookEnemyDmg("Boss","Boss"); }catch(e){}
  try{ hooked+=hookAds(); }catch(e){}
  g.__starryLtzjHooked=hooked;
  log("combat boot hooked="+hooked+
      " dmg="+(g.__starryDmg100?1:0)+" inv="+(g.__starryInv?1:0)+" ad="+(g.__starryNoAd?1:0));
})();
)JS";

/** 开功能后：旗标 + 明文原型 hook；仍不开 Dobby。 */
static void SyncCombatJs() {
    if (!g_dmg100 && !g_inv && !g_noAd) return;
    if (!AceSafeForCombat()) {
        if (!g_gateLogOnce.exchange(true)) {
            LOGI("[Ltzj] combat JS deferred, ACE gate %ds left", AceRemainSec());
        }
        return;
    }
    static bool s_loggedOnce = false;
    if (!s_loggedOnce) {
        s_loggedOnce = true;
        LOGI("[Ltzj] feature ON → XPlane/EnemyPlane/Boss/Ad JS (no Dobby)");
    }
    char buf[512];
    snprintf(buf, sizeof(buf),
             "(function(){var g=(typeof globalThis!==\"undefined\")?globalThis:"
             "((typeof window!==\"undefined\")?window:this);"
             "function r(i){try{if(typeof jsb!==\"undefined\"&&jsb.reflection){"
             "try{if(jsb.reflection.callStaticMethod(\"com.starry.magic.cocos.LtzjHooks\","
             "\"getRet\",\"(I)Z\",i))return true;}catch(e0){}"
             "try{return!!jsb.reflection.callStaticMethod(\"com.cocos.game.AppActivity\","
             "\"get_ret\",\"(I)Z\",i);}catch(e1){}}}catch(e){}return false;}"
             "g.__starryDmg100=r(1)||%s;g.__starryInv=r(2)||%s;g.__starryNoAd=r(3)||%s;})();",
             g_dmg100 ? "true" : "false", g_inv ? "true" : "false",
             g_noAd ? "true" : "false");
    EvalJs(buf);
    EvalJs(kCombatJs);
    g_combatArmed.store(true);
}

typedef jint (*JniEvalString_t)(JNIEnv*, jclass, jstring);
static JniEvalString_t orig_jniEval = nullptr;
static std::atomic<int> g_jniCalls{0};

static jint hooked_jniEval(JNIEnv* env, jclass cls, jstring js) {
    if (!orig_jniEval) return 0;
    int n = ++g_jniCalls;
    if (js && (n <= 12 || n % 300 == 0)) {
        const char* c = env->GetStringUTFChars(js, nullptr);
        if (c) {
            LOGI("[Ltzj] jni eval #%d len=%zu head=%.80s", n, strlen(c), c);
            env->ReleaseStringUTFChars(js, c);
        }
    }
    return orig_jniEval(env, cls, js);
}

/** 本轮加压关闭：开功能也不 Dobby。 */
static void TryHookJniEval() {
    (void)0;
}

bool Ltzj_TryInit(JavaVM* jvm) {
    g_ltzjJvm = jvm;
    ArmAceGate();
    if (!isLibraryLoaded(GameCocos::TARGET_LIB)) return false;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (detach) g_ltzjJvm->DetachCurrentThread();
    }
    // grant / 初始化：只就绪 + 同步免广告 Java 旗标；不自动 Dobby / 战斗 JS
    g_ready = true;
    SyncJavaRets();
    LOGI("[Ltzj] init ok lib=%s jni=%d ace_remain=%ds (combat lazy until feature ON)",
         GameCocos::TARGET_LIB, g_jniHooked ? 1 : 0, AceRemainSec());
    return true;
}

static void* EarlyHookThread(void* arg) {
    g_ltzjJvm = (JavaVM*)arg;
    ArmAceGate();
    // 只等到 ACE 窗并标记 ready；真正 hook 等用户开功能（Ltzj_Tick / OnFeatureChanged）
    while (!AceSafeForCombat()) {
        usleep(500 * 1000);
    }
    for (int i = 0; i < 2000; i++) {
        if (isLibraryLoaded(GameCocos::TARGET_LIB)) {
            g_ready = true;
            SyncJavaRets();
            LOGI("[Ltzj] early ready iter=%d (no auto combat/JNI)", i);
            return nullptr;
        }
        usleep(20 * 1000);
    }
    LOGW("[Ltzj] early ready timeout");
    return nullptr;
}

void Ltzj_StartEarlyHookThread(JavaVM* jvm) {
    static std::atomic_bool started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    ArmAceGate();
    pthread_t th;
    if (pthread_create(&th, nullptr, EarlyHookThread, jvm) == 0) pthread_detach(th);
    else started.store(false);
}

bool Ltzj_IsReady() { return g_ready; }

void Ltzj_OnFeatureChanged(int featNum, bool enabled) {
    ArmAceGate();
    if (featNum == LTZJ_FEAT_DMG100) g_dmg100 = enabled;
    if (featNum == LTZJ_FEAT_INVINCIBLE) g_inv = enabled;
    if (featNum == LTZJ_FEAT_NO_AD) g_noAd = enabled;
    SyncJavaRets();
    if (featNum == LTZJ_FEAT_DMG100 || featNum == LTZJ_FEAT_INVINCIBLE ||
        featNum == LTZJ_FEAT_NO_AD) {
        if (g_ready && AceSafeForCombat() && (g_dmg100 || g_inv || g_noAd)) {
            SyncCombatJs();
        } else if (g_dmg100 || g_inv || g_noAd) {
            LOGI("[Ltzj] feature #%d queued, ACE gate %ds left (no combat JS yet)",
                 featNum, AceRemainSec());
        }
    }
    LOGI("[Ltzj] feature #%d=%s dmg=%d inv=%d noAd=%d ace_remain=%ds", featNum,
         enabled ? "ON" : "OFF", g_dmg100 ? 1 : 0, g_inv ? 1 : 0, g_noAd ? 1 : 0,
         AceRemainSec());
}

void Ltzj_ForceClear() {
    g_dmg100 = g_inv = g_noAd = false;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (env) {
        CacheJava(env);
        if (g_hooksCls && g_hooksClear) env->CallStaticVoidMethod(g_hooksCls, g_hooksClear);
        if (detach) g_ltzjJvm->DetachCurrentThread();
    }
    if (g_ready && AceSafeForCombat()) {
        EvalJs("(function(){var g=(typeof globalThis!==\"undefined\")?globalThis:"
               "((typeof window!==\"undefined\")?window:this);"
               "g.__starryDmg100=false;g.__starryInv=false;g.__starryNoAd=false;})();");
    }
}

void Ltzj_Tick(const bool* features) {
    if (!features) return;
    ArmAceGate();
    g_dmg100 = features[LTZJ_FEAT_DMG100];
    g_inv = features[LTZJ_FEAT_INVINCIBLE];
    g_noAd = features[LTZJ_FEAT_NO_AD];
    if (!g_ready) return;
    SyncJavaRets();
    if (!g_dmg100 && !g_inv && !g_noAd) return;
    if (!AceSafeForCombat()) return;
    // 周期刷新旗标；hooked 未齐时持续重试（进战斗后 System 模块才齐）
    static int tick = 0;
    if ((++tick % 3) == 0 || !g_combatArmed.load()) SyncCombatJs();
}

// ---- assets dex → DexClassLoader → MenuService.start（官方 zip 路径无改游戏 dex）----

static bool CopyAssetToFile(JNIEnv* env, jobject app, const char* assetPath, const char* destPath) {
    jclass ctxCls = env->GetObjectClass(app);
    jmethodID getAssets = env->GetMethodID(ctxCls, "getAssets", "()Landroid/content/res/AssetManager;");
    jobject assets = env->CallObjectMethod(app, getAssets);
    env->DeleteLocalRef(ctxCls);
    if (!assets) {
        env->ExceptionClear();
        return false;
    }
    jclass amCls = env->GetObjectClass(assets);
    jmethodID open = env->GetMethodID(amCls, "open", "(Ljava/lang/String;)Ljava/io/InputStream;");
    jstring jasset = env->NewStringUTF(assetPath);
    jobject in = env->CallObjectMethod(assets, open, jasset);
    env->DeleteLocalRef(jasset);
    env->DeleteLocalRef(amCls);
    env->DeleteLocalRef(assets);
    if (!in) {
        env->ExceptionClear();
        return false;
    }

    jclass fosCls = env->FindClass("java/io/FileOutputStream");
    jmethodID fosInit = env->GetMethodID(fosCls, "<init>", "(Ljava/lang/String;)V");
    jstring jdest = env->NewStringUTF(destPath);
    jobject out = env->NewObject(fosCls, fosInit, jdest);
    env->DeleteLocalRef(jdest);
    if (!out) {
        env->ExceptionClear();
        env->DeleteLocalRef(in);
        env->DeleteLocalRef(fosCls);
        return false;
    }

    jclass isCls = env->FindClass("java/io/InputStream");
    jmethodID read = env->GetMethodID(isCls, "read", "([B)I");
    jmethodID closeIn = env->GetMethodID(isCls, "close", "()V");
    jmethodID write = env->GetMethodID(fosCls, "write", "([BII)V");
    jmethodID closeOut = env->GetMethodID(fosCls, "close", "()V");
    jbyteArray buf = env->NewByteArray(8192);
    bool ok = true;
    while (true) {
        jint n = env->CallIntMethod(in, read, buf);
        if (env->ExceptionCheck()) {
            env->ExceptionClear();
            ok = false;
            break;
        }
        if (n <= 0) break;
        env->CallVoidMethod(out, write, buf, 0, n);
        if (env->ExceptionCheck()) {
            env->ExceptionClear();
            ok = false;
            break;
        }
    }
    env->CallVoidMethod(in, closeIn);
    env->CallVoidMethod(out, closeOut);
    env->DeleteLocalRef(buf);
    env->DeleteLocalRef(in);
    env->DeleteLocalRef(out);
    env->DeleteLocalRef(isCls);
    env->DeleteLocalRef(fosCls);
    return ok;
}

static jobject GetApplication(JNIEnv* env) {
    jclass atCls = env->FindClass("android/app/ActivityThread");
    if (!atCls) {
        env->ExceptionClear();
        return nullptr;
    }
    jmethodID curApp = env->GetStaticMethodID(atCls, "currentApplication", "()Landroid/app/Application;");
    jobject app = curApp ? env->CallStaticObjectMethod(atCls, curApp) : nullptr;
    env->DeleteLocalRef(atCls);
    if (!app) env->ExceptionClear();
    return app;
}

static bool FileExistsNative(const char* path) {
    return path && access(path, R_OK) == 0;
}

static bool BootstrapMenuOnce(JNIEnv* env) {
    jobject app = GetApplication(env);
    if (!app) {
        LOGW("[Ltzj] menu bootstrap: no Application yet");
        return false;
    }

    char dexPath[512];
    const char* tmpDex = "/data/local/tmp/ltzj_st/starry.dex";
    if (FileExistsNative(tmpDex)) {
        snprintf(dexPath, sizeof(dexPath), "%s", tmpDex);
        LOGI("[Ltzj] menu dex from tmp %s", dexPath);
    } else {
        jclass ctxCls = env->GetObjectClass(app);
        jmethodID getCcd = env->GetMethodID(ctxCls, "getCodeCacheDir", "()Ljava/io/File;");
        jobject ccd = env->CallObjectMethod(app, getCcd);
        env->DeleteLocalRef(ctxCls);
        if (!ccd) {
            env->ExceptionClear();
            env->DeleteLocalRef(app);
            return false;
        }
        jclass fileCls = env->GetObjectClass(ccd);
        jmethodID getPath = env->GetMethodID(fileCls, "getAbsolutePath", "()Ljava/lang/String;");
        jstring jpath = (jstring)env->CallObjectMethod(ccd, getPath);
        const char* ccdPath = env->GetStringUTFChars(jpath, nullptr);
        snprintf(dexPath, sizeof(dexPath), "%s/starry.dex", ccdPath);
        env->ReleaseStringUTFChars(jpath, ccdPath);
        env->DeleteLocalRef(jpath);
        env->DeleteLocalRef(fileCls);
        env->DeleteLocalRef(ccd);
        if (!FileExistsNative(dexPath)) {
            if (!CopyAssetToFile(env, app, "starry_dex/starry.dex", dexPath)) {
                LOGE("[Ltzj] copy assets/starry_dex/starry.dex failed");
                env->DeleteLocalRef(app);
                return false;
            }
            LOGI("[Ltzj] copied assets dex → %s", dexPath);
        }
    }

    jclass ctxCls = env->GetObjectClass(app);
    jmethodID getCl = env->GetMethodID(ctxCls, "getClassLoader", "()Ljava/lang/ClassLoader;");
    jobject parent = env->CallObjectMethod(app, getCl);
    jmethodID getCcd = env->GetMethodID(ctxCls, "getCodeCacheDir", "()Ljava/io/File;");
    jobject ccd = env->CallObjectMethod(app, getCcd);
    env->DeleteLocalRef(ctxCls);
    if (!parent || !ccd) {
        env->ExceptionClear();
        env->DeleteLocalRef(app);
        return false;
    }
    jclass fileCls = env->GetObjectClass(ccd);
    jmethodID getPath = env->GetMethodID(fileCls, "getAbsolutePath", "()Ljava/lang/String;");
    jstring jopt = (jstring)env->CallObjectMethod(ccd, getPath);
    env->DeleteLocalRef(fileCls);
    env->DeleteLocalRef(ccd);

    jclass dclCls = env->FindClass("dalvik/system/DexClassLoader");
    jmethodID dclInit = env->GetMethodID(
        dclCls, "<init>",
        "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/ClassLoader;)V");
    jstring jdex = env->NewStringUTF(dexPath);
    jobject dcl = env->NewObject(dclCls, dclInit, jdex, jopt, nullptr, parent);
    env->DeleteLocalRef(jdex);
    env->DeleteLocalRef(jopt);
    env->DeleteLocalRef(dclCls);
    env->DeleteLocalRef(parent);
    if (!dcl) {
        env->ExceptionDescribe();
        env->ExceptionClear();
        env->DeleteLocalRef(app);
        LOGE("[Ltzj] DexClassLoader failed");
        return false;
    }

    jclass clCls = env->GetObjectClass(dcl);
    jmethodID loadClass = env->GetMethodID(clCls, "loadClass", "(Ljava/lang/String;)Ljava/lang/Class;");
    jstring jname = env->NewStringUTF("com.starry.magic.ltzj.DexInstaller");
    jclass bootCls = (jclass)env->CallObjectMethod(dcl, loadClass, jname);
    env->DeleteLocalRef(jname);
    env->DeleteLocalRef(clCls);
    env->DeleteLocalRef(dcl);
    if (!bootCls) {
        env->ExceptionDescribe();
        env->ExceptionClear();
        env->DeleteLocalRef(app);
        LOGE("[Ltzj] loadClass DexInstaller failed");
        return false;
    }

    jmethodID boot = env->GetStaticMethodID(
        bootCls, "boot", "(Landroid/content/Context;Ljava/lang/String;)V");
    if (!boot) {
        env->ExceptionClear();
        env->DeleteLocalRef(bootCls);
        env->DeleteLocalRef(app);
        LOGE("[Ltzj] DexInstaller.boot missing");
        return false;
    }
    jstring jdexPath = env->NewStringUTF(dexPath);
    env->CallStaticVoidMethod(bootCls, boot, app, jdexPath);
    env->DeleteLocalRef(jdexPath);
    if (env->ExceptionCheck()) {
        env->ExceptionDescribe();
        env->ExceptionClear();
        env->DeleteLocalRef(bootCls);
        env->DeleteLocalRef(app);
        LOGE("[Ltzj] DexInstaller.boot threw");
        return false;
    }
    LOGI("[Ltzj] DexInstaller.boot OK (MenuService via PathClassLoader)");
    env->DeleteLocalRef(bootCls);
    env->DeleteLocalRef(app);
    return true;
}

static void* MenuBootstrapThread(void* arg) {
    g_ltzjJvm = (JavaVM*)arg;
    // so 晚加载后立刻起菜单（对齐斌哥秒出）；失败再短间隔重试
    for (int i = 0; i < 80; i++) {
        if (i > 0) usleep(250 * 1000);
        bool detach = false;
        JNIEnv* env = GetEnv(&detach);
        if (!env) continue;
        bool ok = BootstrapMenuOnce(env);
        if (detach) g_ltzjJvm->DetachCurrentThread();
        if (ok) {
            Ltzj_TryInit(g_ltzjJvm);
            return nullptr;
        }
    }
    LOGE("[Ltzj] menu bootstrap gave up");
    return nullptr;
}

void Ltzj_StartMenuBootstrap(JavaVM* jvm) {
    static std::atomic_bool started{false};
    bool expected = false;
    if (!started.compare_exchange_strong(expected, true)) return;
    g_ltzjJvm = jvm;
    ArmAceGate();
    pthread_t th;
    if (pthread_create(&th, nullptr, MenuBootstrapThread, jvm) == 0) {
        pthread_detach(th);
        LOGI("[Ltzj] menu bootstrap thread started (ACE gate %ds)", kAceSafeDelaySec);
    } else {
        started.store(false);
        LOGE("[Ltzj] menu bootstrap thread create failed");
    }
}
