/* ================================================================
 *  堡垒之战 MOD — Cocos Creator 3.8.7 (libcocos.so)
 *
 *  无敌/倍攻会让服务端 STAGE_VERIFY 不一致 →「结算失败」。
 *  只吞 onVerifyFail 踢局弹窗，不伪造 game_stage_end。
 *  菜单关闭时必须硬控标志位 + 还原角色属性（不能只写 false）。
 * ================================================================ */

#include "BlzzMod.h"
#include "Includes/obfuscate.h"

#include <cstring>
#include <cstdio>

#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"

static JavaVM* g_blzzJvm = nullptr;
static bool g_ready = false;
static volatile bool g_inv = false;
static volatile bool g_atk10 = false;
static volatile bool g_xuli = false;

static JNIEnv* GetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_blzzJvm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_blzzJvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_blzzJvm->AttachCurrentThread(&env, nullptr) != 0) return nullptr;
        *needDetach = true;
    } else if (st != JNI_OK) {
        return nullptr;
    }
    return env;
}

static void EvalJs(const char* js) {
    if (!g_blzzJvm || !js) return;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    jclass cls = env->FindClass(OBFUSCATE("com/starry/magic/cocos/CocosJsBridge"));
    if (!cls) {
        env->ExceptionClear();
        if (detach) g_blzzJvm->DetachCurrentThread();
        return;
    }
    jmethodID mid = env->GetStaticMethodID(cls, "evalOnGLThread", "(Ljava/lang/String;)V");
    if (mid) {
        jstring jjs = env->NewStringUTF(js);
        env->CallStaticVoidMethod(cls, mid, jjs);
        env->DeleteLocalRef(jjs);
    }
    env->DeleteLocalRef(cls);
    if (detach) g_blzzJvm->DetachCurrentThread();
}

static const char* kSyncJs = R"JS(
(function(){
  var g=(typeof globalThis!=="undefined")?globalThis:((typeof window!=="undefined")?window:this);
  function mod(name){
    try{
      if(typeof System!=="undefined"&&System.get){
        var m=System.get("chunks:///_virtual/"+name+".ts");
        if(m) return m.default||m;
      }
    }catch(e){}
    return null;
  }
  function tip(msg){
    try{
      var Tip=mod("mgrTip");
      if(Tip&&typeof Tip.showMsgTip==="function") Tip.showMsgTip(msg);
    }catch(e){}
  }
  function captureRole(ctrl){
    try{
      if(ctrl){
        g.__starryBattleCtrl=ctrl;
        if(ctrl.role) g.__starryBattleRole=ctrl.role;
      }
    }catch(e){}
  }
  function ensureHooks(){
    if(g.__starryHooksOk) return;
    var GC=mod("GameController");
    if(!GC||!GC.prototype) return;
    g.__starryHooksOk=1;

    if(typeof GC.prototype.onMsgRoleTen==="function"&&!g.__starryRoleTenHooked){
      g.__starryRoleTenHooked=1;
      var _ten=GC.prototype.onMsgRoleTen;
      GC.prototype.onMsgRoleTen=function(){
        captureRole(this);
        // 菜单关闭时禁止再叠层（避免外部/残留 MSG）
        if(!g.__starryAtk10) return;
        if(!g.__starryAtkBase&&this.role&&this.role._baseAttribute){
          try{
            g.__starryAtkBase={
              ATK:this.role._baseAttribute.ATTACK,
              HP:this.role._baseAttribute.HP
            };
          }catch(e){}
        }
        var r=_ten.apply(this,arguments);
        g.__starryAtkStacks=(g.__starryAtkStacks|0)+1;
        return r;
      };
    }

    if(typeof GC.prototype.update==="function"&&!g.__starryGcUpdateHooked){
      g.__starryGcUpdateHooked=1;
      var _upd=GC.prototype.update;
      GC.prototype.update=function(){
        captureRole(this);
        return _upd.apply(this,arguments);
      };
    }

    g.__starryRestoreAtk=function(){
      try{
        var role=g.__starryBattleRole;
        var base=g.__starryAtkBase;
        if(role&&role._baseAttribute&&base){
          role._baseAttribute.ATTACK=base.ATK;
          role._baseAttribute.HP=base.HP;
          if(typeof role.markPropertyDirty==="function"){
            role.markPropertyDirty("ATTACK");
            role.markPropertyDirty("HP");
          }
        }else if(role&&role._baseAttribute){
          var n=g.__starryAtkStacks|0;
          for(var i=0;i<n;i++){
            role._baseAttribute.ATTACK/=10;
            role._baseAttribute.HP/=10;
          }
          if(n>0&&typeof role.markPropertyDirty==="function"){
            role.markPropertyDirty("ATTACK");
            role.markPropertyDirty("HP");
          }
        }
      }catch(e){}
      g.__starryAtkStacks=0;
      g.__starryAtkBase=null;
    };

    g.__starryApplyAtk=function(times){
      times=times|0;
      if(times<=0) return false;
      try{
        var ctrl=g.__starryBattleCtrl;
        var role=g.__starryBattleRole||(ctrl&&ctrl.role);
        if(!role||!role._baseAttribute) return false;
        if(!g.__starryAtkBase){
          g.__starryAtkBase={ATK:role._baseAttribute.ATTACK,HP:role._baseAttribute.HP};
        }
        // 直接改属性，避免 sendMsg 在无战斗时虚增 stacks
        for(var i=0;i<times;i++){
          role._baseAttribute.ATTACK*=10;
          role._baseAttribute.HP*=10;
        }
        if(typeof role.markPropertyDirty==="function"){
          role.markPropertyDirty("ATTACK");
          role.markPropertyDirty("HP");
        }
        g.__starryAtkStacks=(g.__starryAtkStacks|0)+times;
        return true;
      }catch(e){}
      return false;
    };
  }
  function lockInvincible(F){
    if(!F||g.__starryInvLocked) return;
    try{
      Object.defineProperty(F,"bRoleInvincible",{
        configurable:true,
        enumerable:true,
        get:function(){ return !!g.__starryInv; },
        set:function(){ /* 仅菜单 __starryInv 生效 */ }
      });
      g.__starryInvLocked=1;
    }catch(e){
      try{ F.bRoleInvincible=!!g.__starryInv; }catch(_e){}
    }
  }
  function lockXuli(D){
    if(!D||g.__starryXuliLocked) return;
    try{
      Object.defineProperty(D,"bDebugXuliNoCd",{
        configurable:true,
        enumerable:true,
        get:function(){ return !!g.__starryXuli; },
        set:function(){}
      });
      g.__starryXuliLocked=1;
    }catch(e){
      try{ D.bDebugXuliNoCd=!!g.__starryXuli; }catch(_e){}
    }
  }
  function ensureVerifyTipPatch(){
    if(g.__starryVerifyTipPatched) return;
    var V=mod("mgrVerifyBattle");
    if(!V) return;
    g.__starryVerifyTipPatched=1;
    V.onVerifyFail=function(){
      try{ if(typeof console!=="undefined"&&console.log) console.log("[Blzz] verify-fail kick suppressed"); }catch(e){}
    };
  }
  try{
    if(typeof g.__starryInv==="undefined") g.__starryInv=false;
    if(typeof g.__starryAtk10==="undefined") g.__starryAtk10=false;
    if(typeof g.__starryXuli==="undefined") g.__starryXuli=false;
    if(typeof g.__starryAtkStacks==="undefined") g.__starryAtkStacks=0;

    ensureHooks();
    ensureVerifyTipPatch();

    var D=mod("mgrDebug");
    if(D){
      D.gameTimeScale=0;
      lockXuli(D);
      if(!g.__starryXuliLocked) D.bDebugXuliNoCd=!!g.__starryXuli;
    }

    var F=mod("mgrFiringRange");
    if(F){
      lockInvincible(F);
      if(!g.__starryInvLocked) F.bRoleInvincible=!!g.__starryInv;
    }

    var G=mod("_GameContext");
    var Role=mod("mgrRole");
    var R=Role&&(Role.default||Role);

    if(G){
      if(g.__starryAtk10){
        // 与官方 GM「十倍」一致：scale 供下次建角；战斗中靠属性叠层
        G.attackHpScale=10;
        if(!(g.__starryAtkStacks>0)){
          if(!g.__starryApplyAtk(3)){
            // 尚无 role：等下一帧 update 捕获后再叠
          }
        }
        if(!g.__starryAtkWarnShown){
          g.__starryAtkWarnShown=1;
          tip("倍攻/无敌会导致结算失败，打完请关闭后再结算");
        }
      }else{
        G.attackHpScale=1;
        if((g.__starryAtkStacks|0)>0||g.__starryAtkBase){
          try{ if(typeof g.__starryRestoreAtk==="function") g.__starryRestoreAtk(); }catch(e){}
          try{ if(R&&typeof R.setRoleDataDirty==="function") R.setRoleDataDirty(true); }catch(e){}
        }
        g.__starryAtkWarnShown=0;
      }
    }

    if(g.__starryInv&&!g.__starryInvWarnShown){
      g.__starryInvWarnShown=1;
      tip("无敌开启：本局结算大概率失败");
    }
    if(!g.__starryInv) g.__starryInvWarnShown=0;

    g.__starryBlzzOk=1;
  }catch(e){
    try{ if(typeof console!=="undefined"&&console.error) console.error("[Blzz]",e); }catch(_e){}
  }
})();
)JS";

static const char* kForceClearJs = R"JS(
(function(){
  var g=(typeof globalThis!=="undefined")?globalThis:((typeof window!=="undefined")?window:this);
  function mod(name){
    try{
      if(typeof System!=="undefined"&&System.get){
        var m=System.get("chunks:///_virtual/"+name+".ts");
        if(m) return m.default||m;
      }
    }catch(e){}
    return null;
  }
  try{
    g.__starryInv=false;
    g.__starryAtk10=false;
    g.__starryXuli=false;
    g.__starryInvWarnShown=0;
    g.__starryAtkWarnShown=0;
    try{ if(typeof g.__starryRestoreAtk==="function") g.__starryRestoreAtk(); }catch(e){}
    g.__starryAtkStacks=0;
    g.__starryAtkBase=null;
    var G=mod("_GameContext");
    if(G) G.attackHpScale=1;
    var D=mod("mgrDebug");
    if(D){
      D.gameTimeScale=0;
      try{ D.bDebugXuliNoCd=false; }catch(e){}
    }
    var F=mod("mgrFiringRange");
    if(F){
      try{ F.bRoleInvincible=false; }catch(e){}
    }
    var Role=mod("mgrRole");
    var R=Role&&(Role.default||Role);
    if(R&&typeof R.setRoleDataDirty==="function") R.setRoleDataDirty(true);
    try{ if(typeof console!=="undefined"&&console.log) console.log("[Blzz] force clear"); }catch(e){}
  }catch(e){}
})();
)JS";

static const char* kUnlockJs = R"JS(
(function(){
  function mod(name){
    try{
      if(typeof System!=="undefined"&&System.get){
        var m=System.get("chunks:///_virtual/"+name+".ts");
        if(m) return m.default||m;
      }
    }catch(e){}
    return null;
  }
  try{
    var P=mod("mgrPlayMethod");
    if(P&&typeof P._debugUnlockAll==="function"){
      P._debugUnlockAll();
      var Tip=mod("mgrTip");
      if(Tip&&typeof Tip.showMsgTip==="function") Tip.showMsgTip("已解锁所有玩法入口");
    }
  }catch(e){}
})();
)JS";

static void SyncFlagsJs() {
    char buf[256];
    snprintf(buf, sizeof(buf),
             "(function(){var g=(typeof globalThis!==\"undefined\")?globalThis:"
             "((typeof window!==\"undefined\")?window:this);"
             "g.__starryInv=%s;g.__starryAtk10=%s;g.__starryXuli=%s;})();",
             g_inv ? "true" : "false",
             g_atk10 ? "true" : "false",
             g_xuli ? "true" : "false");
    EvalJs(buf);
    EvalJs(kSyncJs);
}

bool Blzz_TryInit(JavaVM* jvm) {
    g_blzzJvm = jvm;
    const char* lib = GameCocos::TARGET_LIB;
    if (!isLibraryLoaded(lib)) {
        LOGI("[Blzz] waiting for %s", lib);
        return false;
    }
    g_ready = true;
    SyncFlagsJs();
    LOGI("[Blzz] init ok lib=%s", lib);
    return true;
}

bool Blzz_IsReady() { return g_ready; }

void Blzz_ForceClear() {
    g_inv = false;
    g_atk10 = false;
    g_xuli = false;
    if (!g_ready) return;
    EvalJs(kForceClearJs);
    // 再跑一轮 Sync，确保 defineProperty getter 读到 false
    SyncFlagsJs();
    LOGI("[Blzz] force clear synced");
}

void Blzz_OnFeatureChanged(int featNum, bool enabled) {
    if (featNum == BLZZ_FEAT_INVINCIBLE) g_inv = enabled;
    if (featNum == BLZZ_FEAT_ATK10) g_atk10 = enabled;
    if (featNum == BLZZ_FEAT_XULI_NOCD) g_xuli = enabled;
    if (!g_ready) return;
    if (featNum == BLZZ_FEAT_UNLOCK && enabled) {
        EvalJs(kUnlockJs);
        LOGI("[Blzz] unlock all");
        return;
    }
    if (!g_inv && !g_atk10 && !g_xuli) {
        Blzz_ForceClear();
        LOGI("[Blzz] all off → force clear");
        return;
    }
    SyncFlagsJs();
    LOGI("[Blzz] #%d=%s inv=%d atk=%d xuli=%d",
         featNum, enabled ? "ON" : "OFF",
         (int)g_inv, (int)g_atk10, (int)g_xuli);
}

void Blzz_Tick(const bool* features) {
    if (!features) return;
    bool inv = features[BLZZ_FEAT_INVINCIBLE];
    bool atk = features[BLZZ_FEAT_ATK10];
    bool xuli = features[BLZZ_FEAT_XULI_NOCD];
    bool any = inv || atk || xuli;
    bool wasAny = g_inv || g_atk10 || g_xuli;

    g_inv = inv;
    g_atk10 = atk;
    g_xuli = xuli;

    if (!g_ready) return;

    // 全部关闭：立即清一次，并周期性重同步（防止局内又被改回去）
    if (!any) {
        if (wasAny) {
            Blzz_ForceClear();
            return;
        }
        static int offTick = 0;
        if ((++offTick % 4) == 0) SyncFlagsJs();
        return;
    }

    static int tick = 0;
    if ((++tick % 2) == 0) SyncFlagsJs();
}
