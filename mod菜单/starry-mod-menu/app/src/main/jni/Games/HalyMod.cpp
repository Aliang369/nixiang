/* ================================================================
 *  黑暗领域 MOD — Cocos Creator 3.8.7 (libcocos.so)
 *
 *  路径：CocosJsBridge.evalOnGLThread → System.get 模块原型补丁
 *  1) 免广告：AdSkipService / PlayerPrefService
 *  2) 不羁怒火：开关即给主角叠无敌 Buff（无需学会/点技能）
 *  3) 先祖之魂：开关即召唤强化先祖魂（skill_summon_request）
 *  4) 雷罚打击：普攻命中时自动触发强化雷罚（无需被动栏有技能）
 * ================================================================ */

#include "HalyMod.h"
#include "Includes/obfuscate.h"

#include <cstdio>
#include <cstring>

#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "GameConfig.h"

static JavaVM* g_jvm = nullptr;
static bool g_ready = false;
static volatile bool g_noAd = false;
static volatile bool g_nuhuo = false;
static volatile bool g_xianzu = false;
static volatile bool g_leifa = false;

static JNIEnv* GetEnv(bool* needDetach) {
    *needDetach = false;
    if (!g_jvm) return nullptr;
    JNIEnv* env = nullptr;
    int st = g_jvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_jvm->AttachCurrentThread(&env, nullptr) != 0) return nullptr;
        *needDetach = true;
    } else if (st != JNI_OK) {
        return nullptr;
    }
    return env;
}

static void EvalJs(const char* js) {
    if (!g_jvm || !js) return;
    bool detach = false;
    JNIEnv* env = GetEnv(&detach);
    if (!env) return;
    jclass cls = env->FindClass(OBFUSCATE("com/starry/magic/cocos/CocosJsBridge"));
    if (!cls) {
        env->ExceptionClear();
        if (detach) g_jvm->DetachCurrentThread();
        return;
    }
    jmethodID mid = env->GetStaticMethodID(cls, "evalOnGLThread", "(Ljava/lang/String;)V");
    if (mid) {
        jstring jjs = env->NewStringUTF(js);
        env->CallStaticVoidMethod(cls, mid, jjs);
        env->DeleteLocalRef(jjs);
    }
    env->DeleteLocalRef(cls);
    if (detach) g_jvm->DetachCurrentThread();
}

/* 主补丁：挂一次，之后只改全局旗标 + clearCache */
static const char* kInstallJs = R"JS(
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
  function log(){
    try{ if(typeof console!=="undefined"&&console.log) console.log.apply(console,arguments); }catch(e){}
  }
  if(g.__starryHalyInstalled) return;
  g.__starryHalyInstalled=1;
  if(typeof g.__starryHalyNoAd==="undefined") g.__starryHalyNoAd=false;
  if(typeof g.__starryHalyNuhuo==="undefined") g.__starryHalyNuhuo=false;
  if(typeof g.__starryHalyXianzu==="undefined") g.__starryHalyXianzu=false;
  if(typeof g.__starryHalyLeifa==="undefined") g.__starryHalyLeifa=false;

  function patchCfg(cfg){
    if(!cfg||typeof cfg!=="object") return cfg;
    var id=cfg.id|0;
    var sid=cfg.skill_id|0;
    try{
      if(g.__starryHalyNuhuo&&(sid===1005||id===100501||id===100502||id===100503)){
        cfg.cd=0;
        if(!cfg.buff_data) cfg.buff_data=[];
        var hasInv=false;
        for(var i=0;i<cfg.buff_data.length;i++){
          var b=cfg.buff_data[i];
          if(b&&(b.id===1002||b.id==="1002")){
            b.duration=100000;
            hasInv=true;
          }
        }
        if(!hasInv) cfg.buff_data.push({duration:100000,id:1002});
        if(cfg.desc) cfg.desc="[星光] 无敌100秒，冷却0";
      }
      if(g.__starryHalyXianzu&&(sid===1002||id===100201||id===100202||id===100203)){
        if(!cfg.effect_data) cfg.effect_data=[];
        for(var j=0;j<cfg.effect_data.length;j++){
          var e=cfg.effect_data[j];
          if(!e) continue;
          if(e.type==="summon"||typeof e.inherit_attack!=="undefined"||typeof e.duration!=="undefined"){
            e.type=e.type||"summon";
            e.inherit_attack=100000000;
            e.duration=100000;
            if(!(e.summon_count>0)) e.summon_count=(id===100203?3:(id===100202?2:1));
          }
        }
        if(cfg.desc) cfg.desc="[星光] 先祖魂魄继承1000000%攻击，持续100秒";
      }
      if(g.__starryHalyLeifa&&(sid===1009||id===100901||id===100902||id===100903)){
        if(!cfg.effect_data) cfg.effect_data=[];
        for(var k=0;k<cfg.effect_data.length;k++){
          var d=cfg.effect_data[k];
          if(!d) continue;
          if(typeof d.damage_percent==="number"){
            d.damage_percent=Math.max(d.damage_percent,9900)*50;
          }
          if(typeof d.count==="number"){
            d.count=Math.max(d.count,11)*3;
          }
        }
        if(cfg.desc) cfg.desc="[星光] 雷罚打击伤害/段数增强";
      }
    }catch(err){ log("[Haly] patchCfg",err); }
    return cfg;
  }

  function cls(name,key){
    var m=mod(name);
    if(!m) return null;
    if(key&&m[key]) return m[key];
    return m[name]||m.default||m;
  }
  function getBattlePlayer(){
    try{
      var ECS=cls("ECSManager","ECSManager");
      var scene=ECS&&ECS.instance&&ECS.instance.scene;
      return scene&&scene.playerEntity||null;
    }catch(e){ return null; }
  }
  function isAliveEntity(ent){
    if(!ent||!ent.getComponent) return false;
    try{
      var HC=cls("HealthComponent","HealthComponent");
      var hp=HC&&ent.getComponent(HC);
      return !hp||hp.isAlive();
    }catch(e){ return true; }
  }
  function ensureSkillLoaded(id){
    try{
      var SCM=mod("SkillConfigManager");
      var inst=SCM&&(SCM.SkillConfigManager&&SCM.SkillConfigManager.instance);
      if(!inst) return;
      if(!inst.getSkillDataSync(id)&&typeof inst.preloadSkill==="function") inst.preloadSkill(id);
    }catch(e){}
  }
  function removeInvincible(player){
    try{
      var pack=mod("StatusEffectComponent");
      var SEC=pack&&pack.StatusEffectComponent;
      var SET=pack&&pack.StatusEffectType;
      if(!SEC||!SET||!player.getComponent) return;
      var st=player.getComponent(SEC);
      if(!st||!st.effects) return;
      st.effects=st.effects.filter(function(e){
        return !(e&&e.id===1002&&e.type===SET.INVINCIBLE);
      });
    }catch(e){}
  }
  function applyInvincible(player){
    try{
      var pack=mod("StatusEffectComponent");
      var SEC=pack&&pack.StatusEffectComponent;
      var SET=pack&&pack.StatusEffectType;
      if(!SEC||!SET||!player.getOrCreateComponent) return;
      var st=player.getOrCreateComponent(SEC);
      for(var i=0;i<st.effects.length;i++){
        var e=st.effects[i];
        if(e&&e.id===1002&&e.type===SET.INVINCIBLE&&e.timeLeft>8) return;
      }
      removeInvincible(player);
      st.addEffect(1002, SET.INVINCIBLE, 100, 0, player.id);
    }catch(e){ log("[Haly] applyInvincible",e); }
  }
  function applySummon(player){
    try{
      var ECS=cls("ECSManager","ECSManager");
      var scene=ECS&&ECS.instance&&ECS.instance.scene;
      if(!scene||!scene.eventSystem) return;
      var now=Date.now();
      if(g.__starryHalyLastSummon&&now-g.__starryHalyLastSummon<12000) return;
      g.__starryHalyLastSummon=now;
      ensureSkillLoaded(100203);
      scene.eventSystem.emitSync("skill_summon_request",{
        casterId:player.id,
        skillId:100203,
        effect:{
          type:"summon",
          summon_count:3,
          duration:100000,
          inherit_attack:100000000,
          share_buff:true
        }
      });
    }catch(e){ log("[Haly] applySummon",e); }
  }
  function emitThunder(player,targetId){
    try{
      var ECS=cls("ECSManager","ECSManager");
      var scene=ECS&&ECS.instance&&ECS.instance.scene;
      if(!scene||!scene.eventSystem||targetId<=0) return;
      ensureSkillLoaded(100903);
      scene.eventSystem.emitSync("skill_passive_trigger",{
        casterId:player.id,
        targetId:targetId,
        skillId:100903
      });
    }catch(e){ log("[Haly] emitThunder",e); }
  }
  function clearSkillCache(){
    try{
      var SCM=mod("SkillConfigManager");
      var inst=SCM&&(SCM.SkillConfigManager&&SCM.SkillConfigManager.instance);
      if(inst&&typeof inst.clearCache==="function") inst.clearCache();
    }catch(e){}
  }
  g.__starryHalyClearSkillCache=clearSkillCache;
  g.__starryHalyApplyNow=function(){
    var player=getBattlePlayer();
    if(player&&isAliveEntity(player)){
      if(g.__starryHalyNuhuo) applyInvincible(player);
      else removeInvincible(player);
      if(g.__starryHalyXianzu) applySummon(player);
    }else if(!g.__starryHalyNuhuo){
      g.__starryHalyLastSummon=0;
    }
  };
  function starryHalyAutoTick(){
    if(!g.__starryHalyNuhuo&&!g.__starryHalyXianzu&&!g.__starryHalyLeifa) return;
    g.__starryHalyApplyNow();
  }
  if(!g.__starryHalyAutoTimer){
    g.__starryHalyAutoTimer=setInterval(starryHalyAutoTick,500);
  }

  try{
    var Pref=mod("PlayerPrefService");
    var PrefCls=Pref&&(Pref.PlayerPrefService||Pref);
    if(PrefCls&&PrefCls.prototype){
      if(!g.__starryHalyPrefHooked){
        g.__starryHalyPrefHooked=1;
        var _hap=PrefCls.prototype.hasAdPrivilege;
        var _csa=PrefCls.prototype.canSkipAd;
        PrefCls.prototype.hasAdPrivilege=function(){
          if(g.__starryHalyNoAd) return true;
          return _hap?_hap.apply(this,arguments):false;
        };
        PrefCls.prototype.canSkipAd=function(){
          if(g.__starryHalyNoAd) return true;
          return _csa?_csa.apply(this,arguments):false;
        };
      }
    }
  }catch(e){ log("[Haly] Pref hook",e); }

  try{
    var Ad=mod("AdSkipService");
    var AdCls=Ad&&(Ad.AdSkipService||Ad);
    if(AdCls&&AdCls.prototype&&!g.__starryHalyAdHooked){
      g.__starryHalyAdHooked=1;
      var _can=AdCls.prototype.canSkipAd;
      var _has=AdCls.prototype.hasAdPrivilege;
      var _try=AdCls.prototype.tryShowSkipPopup;
      AdCls.prototype.canSkipAd=function(){
        if(g.__starryHalyNoAd) return true;
        return _can?_can.apply(this,arguments):false;
      };
      AdCls.prototype.hasAdPrivilege=function(){
        if(g.__starryHalyNoAd) return true;
        return _has?_has.apply(this,arguments):false;
      };
      AdCls.prototype.tryShowSkipPopup=function(){
        if(g.__starryHalyNoAd) return Promise.resolve(true);
        return _try?_try.apply(this,arguments):Promise.resolve(false);
      };
    }
  }catch(e){ log("[Haly] AdSkip hook",e); }

  try{
    var Guard=mod("SdkRewardAdGuard");
    var GCls=Guard&&(Guard.SdkRewardAdGuard||Guard);
    if(GCls&&!g.__starryHalyRewardHooked){
      g.__starryHalyRewardHooked=1;
      var keys=["showRewardAd","requestRewardAd","playRewardAd","runRewardAd"];
      for(var qi=0;qi<keys.length;qi++){
        var kn=keys[qi];
        if(typeof GCls[kn]==="function"&&!GCls[kn].__starry){
          (function(name,orig){
            GCls[name]=function(){
              if(g.__starryHalyNoAd){
                var args=arguments;
                return Promise.resolve({success:true,skipped:true});
              }
              return orig.apply(this,arguments);
            };
            GCls[name].__starry=1;
          })(kn,GCls[kn]);
        }
        if(GCls.prototype&&typeof GCls.prototype[kn]==="function"&&!GCls.prototype[kn].__starry){
          (function(name,orig){
            GCls.prototype[name]=function(){
              if(g.__starryHalyNoAd) return Promise.resolve({success:true,skipped:true});
              return orig.apply(this,arguments);
            };
            GCls.prototype[name].__starry=1;
          })(kn,GCls.prototype[kn]);
        }
      }
    }
  }catch(e){ log("[Haly] RewardGuard",e); }

  try{
    var SCM=mod("SkillConfigManager");
    var S=SCM&&(SCM.SkillConfigManager||SCM);
    var proto=S&&S.prototype;
    if(proto&&!g.__starryHalySkillHooked){
      g.__starryHalySkillHooked=1;
      if(typeof proto.parseSkillData==="function"){
        var _parse=proto.parseSkillData;
        proto.parseSkillData=function(cfg){
          return _parse.call(this, patchCfg(cfg));
        };
      }
      if(typeof proto.loadSkillData==="function"){
        var _load=proto.loadSkillData;
        proto.loadSkillData=async function(skillId){
          var data=await _load.apply(this,arguments);
          return data;
        };
      }
    }
  }catch(e){ log("[Haly] SkillConfig",e); }

  try{
    var TR=mod("TableReaderService");
    var T=TR&&(TR.TableReaderService||TR);
    if(T&&T.prototype&&typeof T.prototype.readOne==="function"&&!g.__starryHalyTableHooked){
      g.__starryHalyTableHooked=1;
      var _ro=T.prototype.readOne;
      T.prototype.readOne=async function(tableName){
        var row=await _ro.apply(this,arguments);
        if(tableName==="base_skill"&&row) patchCfg(row);
        return row;
      };
    }
  }catch(e){ log("[Haly] TableReader",e); }

  try{
    var Spd=mod("SpeedHackDetector");
    if(Spd&&typeof Spd.enabled!=="undefined"){
      if(!g.__starryHalySpdHooked){
        g.__starryHalySpdHooked=1;
        try{
          Object.defineProperty(Spd,"enabled",{
            configurable:true,
            enumerable:true,
            get:function(){ return !(g.__starryHalyNuhuo||g.__starryHalyXianzu||g.__starryHalyLeifa); },
            set:function(){}
          });
        }catch(e){}
      }
    }
  }catch(e){}

  try{
    var PST=cls("PassiveSkillTriggerSystem","PassiveSkillTriggerSystem");
    var proto=PST&&PST.prototype;
    if(proto&&!g.__starryHalyPstHooked){
      g.__starryHalyPstHooked=1;
      var _trig=proto.triggerAttackerPassiveSkills;
      proto.triggerAttackerPassiveSkills=function(data){
        if(_trig) _trig.apply(this,arguments);
        if(!g.__starryHalyLeifa) return;
        try{
          var attacker=this.findAliveEntity((data&&data.sourceId)||0);
          var player=getBattlePlayer();
          if(!attacker||!player||attacker.id!==player.id) return;
          var targetId=Number((data&&data.targetId)||0)||0;
          if(targetId<=0) return;
          emitThunder(player,targetId);
        }catch(e){ log("[Haly] thunder hook",e); }
      };
    }
  }catch(e){ log("[Haly] PST hook",e); }

  clearSkillCache();
  if(typeof g.__starryHalyApplyNow==="function") g.__starryHalyApplyNow();
  log("[Haly] hooks installed (auto-buff mode)");
})();
)JS";

static const char* kForceClearJs = R"JS(
(function(){
  var g=(typeof globalThis!=="undefined")?globalThis:((typeof window!=="undefined")?window:this);
  try{
    g.__starryHalyNoAd=false;
    g.__starryHalyNuhuo=false;
    g.__starryHalyXianzu=false;
    g.__starryHalyLeifa=false;
    g.__starryHalyLastSummon=0;
    if(typeof g.__starryHalyClearSkillCache==="function") g.__starryHalyClearSkillCache();
    if(typeof g.__starryHalyApplyNow==="function") g.__starryHalyApplyNow();
  }catch(e){}
})();
)JS";

static void SyncFlagsJs() {
    char buf[384];
    snprintf(buf, sizeof(buf),
             "(function(){var g=(typeof globalThis!==\"undefined\")?globalThis:"
             "((typeof window!==\"undefined\")?window:this);"
             "g.__starryHalyNoAd=%s;g.__starryHalyNuhuo=%s;"
             "g.__starryHalyXianzu=%s;g.__starryHalyLeifa=%s;"
             "if(typeof g.__starryHalyClearSkillCache===\"function\")g.__starryHalyClearSkillCache();"
             "if(typeof g.__starryHalyApplyNow===\"function\")g.__starryHalyApplyNow();"
             "})();",
             g_noAd ? "true" : "false",
             g_nuhuo ? "true" : "false",
             g_xianzu ? "true" : "false",
             g_leifa ? "true" : "false");
    EvalJs(buf);
    EvalJs(kInstallJs);
}

bool Haly_TryInit(JavaVM* jvm) {
    g_jvm = jvm;
    if (!isLibraryLoaded(GameCocos::TARGET_LIB)) return false;
    g_ready = true;
    SyncFlagsJs();
    LOGI("[Haly] ready (libcocos + JS bridge)");
    return true;
}

bool Haly_IsReady() { return g_ready; }

void Haly_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case HALY_FEAT_NO_AD: g_noAd = enabled; break;
        case HALY_FEAT_NUHUO: g_nuhuo = enabled; break;
        case HALY_FEAT_XIANZU: g_xianzu = enabled; break;
        case HALY_FEAT_LEIFA: g_leifa = enabled; break;
        default: break;
    }
    if (g_ready) SyncFlagsJs();
}

void Haly_Tick(const bool* features) {
    if (!g_ready || !features) return;
    bool n0 = features[HALY_FEAT_NO_AD];
    bool n1 = features[HALY_FEAT_NUHUO];
    bool n2 = features[HALY_FEAT_XIANZU];
    bool n3 = features[HALY_FEAT_LEIFA];
    if (n0 != g_noAd || n1 != g_nuhuo || n2 != g_xianzu || n3 != g_leifa) {
        g_noAd = n0;
        g_nuhuo = n1;
        g_xianzu = n2;
        g_leifa = n3;
        SyncFlagsJs();
    } else {
        // 热更/重载后 System 模块可能重建，周期性补装
        static int tick = 0;
        if ((++tick % 30) == 0) EvalJs(kInstallJs);
    }
}

void Haly_ForceClear() {
    g_noAd = g_nuhuo = g_xianzu = g_leifa = false;
    if (g_ready) EvalJs(kForceClearJs);
}
