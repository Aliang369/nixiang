/* ================================================================
 *  动物餐厅 MOD — Cocos Creator
 *
 *  免广告（笔记 §6.2 + Frida 验证）：
 *    1) JS：patch userData.isAdReady / showAd / realShowAd
 *    2) Java smali：AppActivity.showRewardAd / isRewardVideoADReady（DwctHooks）
 *    3) Dobby：evalString 改写 SDKRewardVideoAd 回调
 *
 *  免内购（笔记 §8）：
 *    1) JS：patch PayMentModel 渠道支付 → fakepay
 *    2) JS：patch PayMentSystem.changeOrderStatus → true（发奖关键！）
 *    3) Dobby：wechatPay 回调改写
 * ================================================================ */

#include "DwctMod.h"
#include "Includes/obfuscate.h"

#include <cstring>
#include <string>
#include <cstdio>

#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"
#include "Dobby/dobby.h"

static JavaVM* g_dwctJvm = nullptr;
static bool g_ready = false;
static bool g_evalHooked = false;
static volatile bool g_noAd = false;
static volatile bool g_noIap = false;
static void CallJavaIapGate();

static JNIEnv* GetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_dwctJvm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_dwctJvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_dwctJvm->AttachCurrentThread(&env, nullptr) != 0) return nullptr;
        *needDetach = true;
    } else if (st != JNI_OK) {
        return nullptr;
    }
    return env;
}

static void SyncJavaFlags() {
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    jclass cls = env->FindClass(OBFUSCATE("com/starry/magic/cocos/DwctHooks"));
    if (!cls) {
        env->ExceptionClear();
        if (detach) g_dwctJvm->DetachCurrentThread();
        return;
    }
    jmethodID setAd = env->GetStaticMethodID(cls, "setNoAd", "(Z)V");
    jmethodID setIap = env->GetStaticMethodID(cls, "setNoIap", "(Z)V");
    if (setAd) env->CallStaticVoidMethod(cls, setAd, g_noAd ? JNI_TRUE : JNI_FALSE);
    if (setIap) env->CallStaticVoidMethod(cls, setIap, g_noIap ? JNI_TRUE : JNI_FALSE);
    env->DeleteLocalRef(cls);
    if (detach) g_dwctJvm->DetachCurrentThread();
}

static void EvalJs(const char* js) {
    if (!g_dwctJvm || !js) return;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    jclass cls = env->FindClass(OBFUSCATE("com/starry/magic/cocos/CocosJsBridge"));
    if (!cls) {
        env->ExceptionClear();
        if (detach) g_dwctJvm->DetachCurrentThread();
        return;
    }
    jmethodID mid = env->GetStaticMethodID(cls, "evalOnGLThread", "(Ljava/lang/String;)V");
    if (mid) {
        jstring jjs = env->NewStringUTF(js);
        env->CallStaticVoidMethod(cls, mid, jjs);
        env->DeleteLocalRef(jjs);
    }
    env->DeleteLocalRef(cls);
    if (detach) g_dwctJvm->DetachCurrentThread();
}

// 官方 hot-update 会换掉 Script/index.jsc（冲掉静态 __starryNoIap 门）。
// 旧版用 window.__starryDwct*Patched 只打一次：热更重载模块后 flag 仍在 → runtime 免内购失效。
// 现改为函数自身 __starryW 标记：热更后的新函数无标记 → Tick 会重新 wrap。
static const char* kInstallJs = R"JS(
(function(){
  try{
    if(typeof window.__starryNoAd==="undefined")window.__starryNoAd=false;
    if(typeof window.__starryNoIap==="undefined")window.__starryNoIap=false;
    // 废弃全局 sticky，避免热更后假阳性
    window.__starryDwctAdPatched=0;
    window.__starryDwctPayPatched=0;
    window.__starryDwctPaySysPatched=0;

    var adOk=0,payOk=0,sysOk=0;

    var ud=require("userData");
    if(!(ud&&typeof ud.isAdReady==="function"))return "wait:userData";
    if(!ud.isAdReady.__starryW){
      var oReady=ud.isAdReady.bind(ud);
      var oShowAd=ud.showAd&&ud.showAd.bind(ud);
      var oReal=ud.realShowAd&&ud.realShowAd.bind(ud);
      ud.isAdReady=function(){return window.__starryNoAd?true:oReady();};
      ud.isAdReady.__starryW=1;
      ud.showAd=function(t,i,n,a){
        if(window.__starryNoAd){
          n&&(i=i.bind(n));
          try{ud.sendMessage&&ud.sendMessage("onAdOpen",!1);}catch(e){}
          i&&i(0);
          return;
        }
        return oShowAd?oShowAd(t,i,n,a):void 0;
      };
      ud.showAd.__starryW=1;
      ud.realShowAd=function(t,i,n,a){
        if(window.__starryNoAd){
          n&&(i=i.bind(n));
          try{ud.sendMessage&&ud.sendMessage("onAdOpen",!1);}catch(e){}
          i&&i(0);
          return;
        }
        return oReal?oReal(t,i,n,a):void 0;
      };
      ud.realShowAd.__starryW=1;
    }
    adOk=ud.isAdReady.__starryW?1:0;

    var pm=require("PayMentModel");
    var inst=pm&&(pm.payMentModel||(pm.default&&pm.default.payMentModel));
    if(!inst)return "wait:PayMentModel";
    var proto=Object.getPrototypeOf(inst)||inst;
    var names=["androidWxPay","sdkboxPay","wxMiNiGamePay","qqMiNiPay","wxIOSPay","huaweiPMSPay","googlePay","globalIosPay","inlandIosPay"];
    var needPay=0,gotPay=0;
    for(var i=0;i<names.length;i++){
      (function(name){
        if(typeof proto[name]!=="function")return;
        needPay++;
        if(proto[name].__starryW){gotPay++;return;}
        var orig=proto[name];
        var wrapped=function(a,b){
          if(window.__starryNoIap){
            return Promise.resolve({status:0,orderItem:{orderId:"fakepay",payType:a,pay_situation:b}});
          }
          return orig.apply(this,arguments);
        };
        wrapped.__starryW=1;
        proto[name]=wrapped;
        gotPay++;
      })(names[i]);
    }
    payOk=(needPay>0&&gotPay===needPay)?1:0;

    var PS=require("PayMentSystem");
    var Cls=PS&&(PS.default||PS);
    if(!(Cls&&Cls.prototype&&typeof Cls.prototype.changeOrderStatus==="function")){
      return "wait:PayMentSystem";
    }
    if(!Cls.prototype.changeOrderStatus.__starryW){
      var oCOS=Cls.prototype.changeOrderStatus;
      var wCOS=function(e){
        if(window.__starryNoIap)return Promise.resolve(!0);
        return oCOS.apply(this,arguments);
      };
      wCOS.__starryW=1;
      Cls.prototype.changeOrderStatus=wCOS;
    }
    sysOk=Cls.prototype.changeOrderStatus.__starryW?1:0;

    window.__starryDwctInstalled=(adOk&&payOk&&sysOk)?1:0;
    return window.__starryDwctInstalled?"ok":("partial:ad="+adOk+",pay="+payOk+",sys="+sysOk);
  }catch(e){
    return "err:"+e;
  }
})();
)JS";

static void SyncFlagsJs() {
    char buf[128];
    snprintf(buf, sizeof(buf),
             "window.__starryNoAd=%s;window.__starryNoIap=%s;",
             g_noAd ? "true" : "false",
             g_noIap ? "true" : "false");
    EvalJs(buf);
}

static void TryInstallPatch() {
    SyncJavaFlags();
    EvalJs(kInstallJs);
    SyncFlagsJs();
}

typedef jint (*EvalString_t)(JNIEnv*, jclass, jstring);
static EvalString_t orig_evalString = nullptr;

static jint hooked_evalString(JNIEnv* env, jclass cls, jstring js) {
    if (!js || (!g_noAd && !g_noIap)) {
        return orig_evalString(env, cls, js);
    }
    const char* utf = env->GetStringUTFChars(js, nullptr);
    if (!utf) return orig_evalString(env, cls, js);
    std::string s(utf);
    env->ReleaseStringUTFChars(js, utf);
    bool changed = false;

    if (g_noAd) {
        size_t p = s.find("SDKRewardVideoAd(");
        if (p != std::string::npos) {
            size_t a = s.find('(', p);
            size_t b = s.find(')', a);
            if (a != std::string::npos && b != std::string::npos && b > a) {
                s.replace(a + 1, b - a - 1, "0");
                changed = true;
            }
        }
    }
    if (g_noIap) {
        size_t p = s.find("wechatPay(");
        if (p != std::string::npos && s.find("wechatPay(\"true\")") == std::string::npos) {
            size_t q1 = s.find('"', p);
            size_t q2 = (q1 != std::string::npos) ? s.find('"', q1 + 1) : std::string::npos;
            if (q1 != std::string::npos && q2 != std::string::npos) {
                s.replace(q1 + 1, q2 - q1 - 1, "true");
                changed = true;
            }
        }
    }
    if (!changed) return orig_evalString(env, cls, js);
    jstring nj = env->NewStringUTF(s.c_str());
    jint r = orig_evalString(env, cls, nj);
    env->DeleteLocalRef(nj);
    return r;
}

static void TryHookEvalString() {
    if (g_evalHooked) return;
    const char* lib = GameCocos::TARGET_LIB;
    if (!isLibraryLoaded(lib)) return;
    void* sym = DobbySymbolResolver(lib, GameCocos::Symbols::EVAL_STRING);
    if (!sym) {
        LOGW("[Dwct] evalString symbol not found");
        return;
    }
    if (DobbyHook(sym, (dobby_dummy_func_t)hooked_evalString,
                  (dobby_dummy_func_t*)&orig_evalString) == 0) {
        g_evalHooked = true;
        LOGI("[Dwct] hooked evalString");
    } else {
        LOGW("[Dwct] DobbyHook evalString failed");
    }
}

bool Dwct_TryInit(JavaVM* jvm) {
    g_dwctJvm = jvm;
    const char* lib = GameCocos::TARGET_LIB;
    if (!isLibraryLoaded(lib)) {
        LOGI("[Dwct] waiting for %s", lib);
        return false;
    }
    TryHookEvalString();
    g_ready = true;
    CallJavaIapGate();
    TryInstallPatch();
    LOGI("[Dwct] init ok lib=%s", lib);
    return true;
}

bool Dwct_IsReady() { return g_ready; }

void Dwct_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == DWCT_FEAT_NO_AD) g_noAd = enabled;
    if (featNum == DWCT_FEAT_NO_IAP) g_noIap = enabled;
    SyncJavaFlags();
    if (!g_ready) return;
    TryInstallPatch();
    LOGI("[Dwct] feature #%d=%s noAd=%d noIap=%d",
         featNum, enabled ? "ON" : "OFF", (int)g_noAd, (int)g_noIap);
}

void Dwct_Tick(const bool* features) {
    if (!features) return;
    bool ad = features[DWCT_FEAT_NO_AD];
    bool iap = features[DWCT_FEAT_NO_IAP];
    if (ad != g_noAd || iap != g_noIap) {
        g_noAd = ad;
        g_noIap = iap;
        SyncJavaFlags();
    }
    if (!g_ready) return;
    if (!g_evalHooked) TryHookEvalString();
    static int tick = 0;
    ++tick;
    if ((tick % 2) == 0) CallJavaIapGate();
    if ((tick % 2) == 0) TryInstallPatch();
    else SyncFlagsJs();
}

static void CallJavaIapGate() {
    if (!g_dwctJvm) return;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    jclass cls = env->FindClass(OBFUSCATE("com/starry/magic/cocos/DwctIapGate"));
    if (!cls) {
        env->ExceptionClear();
        if (detach) g_dwctJvm->DetachCurrentThread();
        return;
    }
    jmethodID mid = env->GetStaticMethodID(cls, "applyFromNative", "()V");
    if (mid) env->CallStaticVoidMethod(cls, mid);
    env->ExceptionClear();
    env->DeleteLocalRef(cls);
    if (detach) g_dwctJvm->DetachCurrentThread();
}
