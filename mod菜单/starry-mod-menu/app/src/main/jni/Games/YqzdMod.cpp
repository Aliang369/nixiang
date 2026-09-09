/* ================================================================
 *  遗弃之地 (com.hortor.yqzd) 0.1.57 TapTap — Cocos Creator 3.8.5
 *
 *  路线：零 native hook（ACE/TSS），全 JS 运行时 wrap（DwctMod 同款骨架）：
 *    - CocosJsBridge.evalOnGLThread 执行安装脚本（GL 线程安全）
 *    - System 模块注册表按后缀找四个目标模块，原型方法 wrap（__starryW 防重）
 *      battle-time.ts      BattleTime.prototype.timeScale setter ×spd
 *      formula.ts          formula.calcDamage 伤害 ×atk（isSeckill 跳过）
 *      character-helper.ts characterHelper.subLifeValue 友方直接跳过（无敌）
 *      shop-helper.ts      shopHelper.playRewardVideo 免广告直接发奖回调
 *    - Tick 每次推 globalThis.__yqzd 旗标；wrap 丢失自动重装（热更自愈）
 *
 *  风险模型（2026-09-07 分析）：服务端校验=下发操作记录本地重放，
 *  补丁客户端重放结果自洽可过；PVP 帧同步有 stateDigest，勿在 PVP 开。
 * ================================================================ */
#include "YqzdMod.h"
#include "Includes/obfuscate.h"

#include <cstring>
#include <string>
#include <cstdio>
#include <mutex>

#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"

static JavaVM* g_yqzdJvm = nullptr;
static bool    g_ready   = false;

struct Flags {
    int inv    = 0;   // 无敌
    int atkOn  = 0;   // 倍攻开关
    int atk    = 10;  // 倍攻倍数 10-100
    int spdOn  = 0;   // 加速开关
    int spd    = 1;   // 加速倍率 1-5
    int adfree = 0;   // 免广告
};
static Flags        g_flags;
static std::mutex   g_mtx;

static JNIEnv* GetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_yqzdJvm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_yqzdJvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_yqzdJvm->AttachCurrentThread(&env, nullptr) != 0) return nullptr;
        *needDetach = true;
    } else if (st != JNI_OK) {
        return nullptr;
    }
    return env;
}

static void EvalJs(const char* js) {
    if (!g_yqzdJvm || !js) return;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    jclass cls = env->FindClass(OBFUSCATE("com/starry/magic/cocos/CocosJsBridge"));
    if (!cls) {
        env->ExceptionClear();
        if (detach) g_yqzdJvm->DetachCurrentThread();
        return;
    }
    jmethodID mid = env->GetStaticMethodID(cls, "evalOnGLThread", "(Ljava/lang/String;)V");
    if (mid) {
        jstring jjs = env->NewStringUTF(js);
        env->CallStaticVoidMethod(cls, mid, jjs);
        env->DeleteLocalRef(jjs);
    }
    env->ExceptionClear();
    env->DeleteLocalRef(cls);
    if (detach) g_yqzdJvm->DetachCurrentThread();
}

// ---- JS：旗标推送（每次 Tick 执行，覆盖式） ----
static void SyncFlagsJs() {
    char buf[192];
    std::lock_guard<std::mutex> lk(g_mtx);
    snprintf(buf, sizeof(buf),
             "globalThis.__yqzd={inv:%d,atkOn:%d,atk:%d,spdOn:%d,spd:%d,adfree:%d};",
             g_flags.inv, g_flags.atkOn, g_flags.atk,
             g_flags.spdOn, g_flags.spd, g_flags.adfree);
    EvalJs(buf);
}

// ---- JS：wrap 安装（幂等；结果走 console.log → logcat 可见） ----
static const char* kInstallJs = R"JS(
(function(){
  var __r=(function(){
  try{
    if(typeof System==="undefined")return "wait:System";
    if(!globalThis.__yqzd)globalThis.__yqzd={inv:0,atkOn:0,atk:10,spdOn:0,spd:1,adfree:0};
    var res={bt:0,fm:0,ch:0,sh:0};

    // 按后缀从 System 注册表找已执行模块导出
    function getMod(suffix){
      try{
        var keys=[];
        if(System.entries){var it=System.entries();for(var e=it.next();!e.done;e=it.next())keys.push(e.value[0]);}
        else if(System.registry&&System.registry.forEach){System.registry.forEach(function(v,k){keys.push(k);});}
        for(var i=0;i<keys.length;i++){
          if(keys[i].indexOf(suffix)!==-1){
            var m=System.get?System.get(keys[i]):null;
            if(m)return m;
          }
        }
      }catch(e){}
      return null;
    }

    function wrapInst(mod,modKey,prop,makeWrapper){
      var inst=mod&&(mod[modKey]||(mod.default&&(mod.default[modKey]||mod.default)));
      if(!inst)return 0;
      var proto=Object.getPrototypeOf(inst)||inst;
      var fn=proto[prop];
      if(typeof fn!=="function")return 0;
      if(fn.__starryW)return 2;
      var w=makeWrapper(fn);
      w.__starryW=1;
      try{proto[prop]=w;}catch(e){return 0;}
      return 1;
    }

    // ① 加速：BattleTime.prototype.timeScale setter 乘 spd
    (function(){
      var m=getMod("battle-time.ts");if(!m)return;
      var BT=m.BattleTime||(m.default&&m.default.BattleTime);
      if(!BT||!BT.prototype)return;
      var d=Object.getOwnPropertyDescriptor(BT.prototype,"timeScale");
      if(!d||!d.set)return;
      if(d.set.__starryW){res.bt=2;return;}
      var oset=d.set,oget=d.get;
      var ns=function(v){
        try{var y=globalThis.__yqzd;if(y&&y.spdOn&&typeof v==="number")v=v*y.spd;}catch(e){}
        oset.call(this,v);
      };
      ns.__starryW=1;
      try{Object.defineProperty(BT.prototype,"timeScale",{configurable:true,enumerable:d.enumerable,get:oget,set:ns});res.bt=1;}catch(e){}
    })();

    // ② 倍攻：formula.calcDamage 伤害 ×atk（isSeckill 跳过）
    res.fm=wrapInst(getMod("formula.ts"),"formula","calcDamage",function(fn){
      return function(t,e){
        var r=fn.apply(this,arguments);
        try{
          var y=globalThis.__yqzd;
          if(y&&y.atkOn&&t&&!t.isSeckill&&t.damage&&t.damage.mul)t.damage=t.damage.mul(y.atk);
        }catch(err){}
        return r;
      };
    });

    // ③ 无敌：characterHelper.subLifeValue 友方目标直接跳过
    res.ch=wrapInst(getMod("character-helper.ts"),"characterHelper","subLifeValue",function(fn){
      return function(t,e,l,i){
        try{var y=globalThis.__yqzd;if(y&&y.inv&&t&&t.compCampFriend)return;}catch(err){}
        return fn.apply(this,arguments);
      };
    });

    // ④ 免广告：shopHelper.playRewardVideo 直接走成功回调（同终身卡路径）
    res.sh=wrapInst(getMod("shop-helper.ts"),"shopHelper","playRewardVideo",function(fn){
      return function(e,t,r){
        try{var y=globalThis.__yqzd;if(y&&y.adfree){if(e)e();return;}}catch(err){}
        return fn.apply(this,arguments);
      };
    });

    var ok=(res.bt>0&&res.fm>0&&res.ch>0&&res.sh>0);
    globalThis.__yqzdInstalled=ok?1:0;
    if(!ok)return "wait:bt="+res.bt+",fm="+res.fm+",ch="+res.ch+",sh="+res.sh;
    return "ok";
  }catch(e){
    return "err:"+e;
  }
  })();
  try{console.log("[Yqzd-install] "+__r);}catch(e2){}
  return __r;
})();
)JS";

static void TryInstallPatch() {
    SyncFlagsJs();
    EvalJs(kInstallJs);
}

bool Yqzd_TryInit(JavaVM* jvm) {
    g_yqzdJvm = jvm;
    const char* lib = GameCocos::TARGET_LIB;
    if (!isLibraryLoaded(lib)) {
        LOGI("[Yqzd] waiting for %s", lib);
        return false;
    }
    g_ready = true;
    LOGI("[Yqzd] init ok lib=%s", lib);
    return true;
}

bool Yqzd_IsReady() { return g_ready; }

void Yqzd_Tick() {
    if (!g_ready) return;
    SyncFlagsJs();
    EvalJs(kInstallJs); // 幂等重装：热更/重载后 wrap 丢失自动补
}

void Yqzd_OnFeatureChanged(int featNum, bool enabled) {
    std::lock_guard<std::mutex> lk(g_mtx);
    switch (featNum) {
        case YQZD_FEAT_INV:    g_flags.inv    = enabled ? 1 : 0; break;
        case YQZD_FEAT_ATK_ON: g_flags.atkOn  = enabled ? 1 : 0; break;
        case YQZD_FEAT_SPD_ON: g_flags.spdOn  = enabled ? 1 : 0; break;
        case YQZD_FEAT_ADFREE: g_flags.adfree = enabled ? 1 : 0; break;
        default: return; // Category / 滑块
    }
    LOGI("[Yqzd] feature #%d=%s inv=%d atkOn=%d spdOn=%d adfree=%d",
         featNum, enabled ? "ON" : "OFF",
         g_flags.inv, g_flags.atkOn, g_flags.spdOn, g_flags.adfree);
}

void Yqzd_OnValueChanged(int featNum, int value) {
    std::lock_guard<std::mutex> lk(g_mtx);
    switch (featNum) {
        case YQZD_FEAT_ATK_VAL:
            if (value < 10)  value = 10;
            if (value > 100) value = 100;
            g_flags.atk = value;
            break;
        case YQZD_FEAT_SPD_VAL:
            if (value < 1) value = 1;
            if (value > 5) value = 5;
            g_flags.spd = value;
            break;
        default: return;
    }
    LOGI("[Yqzd] value #%d = %d", featNum, value);
}
