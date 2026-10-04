// ============================================================================
//  仙逆：本尊（com.qqsy.xnbz 0.0.14 / vc14 TapTap 渠道）— 星光菜单游戏模块
//
//  引擎：Cocos Creator 3.x / libcocos.so（JSB 脚本层，非 IL2CPP）
//  包内实测：无加固壳（无 libjiagu/libtup/libtprt/libDexHelper 等），
//            Application = com.cocos.game.GameApplication（classes10.dex），
//            TapSDK = 新版 ClientLoginUtils 路径（classes7.dex）。
//
//  ★ 当前阶段 = ①过环境：**零 hook 骨架**
//      只做「目标库映射判定 + 开关状态同步 + 信号兜底」，
//      不含任何 hook / 内存改写 / JS 注入；开关置 ON 只写日志。
//      【无敌】【伤害倍率】的真实实现待用户明确放行后填 §TODO 区，
//      并先读 skill/re-implement/hook-rules.md（§16 懒装 / §17 缺失降级）。
//
//  保留的防闪退设施（代码而非文档约定，删了可能复现历史闪退）：
//      §④ 致命信号兜底（sigsetjmp 保护区 + sigaltstack）+ 装钩互斥 + 槽位熔断。
//      阶段② 加 hook 时必须连 §⑤ 双路径挂点机器一起恢复（RVA 优先 + 按名回落，
//      RVA 漂移拒绝、幂等安装、orig 必调、reenter 防递归）。
//
//  铁律：Tick 只做同步+兜底，禁 SafeProbe / 扫堆 / 遍历对象（代价爆炸）。
// ============================================================================

#include "XnbzMod.h"

#include <atomic>
#include <cstdint>
#include <cstdio>
#include <cstring>

#include <pthread.h>  // 抓脚本线程
#include <unistd.h>   // sleep
#include <mutex>      // §④ 装钩互斥
#include <setjmp.h>   // §④ sigsetjmp 保护区
#include <signal.h>   // §④ 致命信号兜底

// §④ 的 DobbyHookGuarded 需要 DobbyHook 声明：即便本阶段零 hook，
// 兜底机器也必须能编译（阶段② 直接复用，勿删）。
#include "Dobby/dobby.h"
#include "Includes/obfuscate.h"
#include "Includes/Logger.h"
#include "Includes/Utils.hpp"
#include "Includes/SecStrings.hpp"

// 日志锚点：装机自检用 adb logcat -s Mod_Menu:V StarryMagic:V 过滤 [Xnbz]
#define XNBZ_LOG "[Xnbz]"

// Cocos Creator 3.x 引擎库（混淆字面量，见 SecStrings.hpp）
#define XNBZ_TARGET_LIB sec_lib_cocos()

// ============================================================================
//  ① 开关状态（全关 = 零副作用；本阶段无 hook 可装，只做状态镜像）
//     语义与 XnbzEntry.cpp 的 featureList 一致：
//       XNBZ_FEAT_INV = 1  Toggle_无敌
//       XNBZ_FEAT_DMG = 2  Slider_伤害倍率（1 = 关闭，2..100 = 倍率）
// ============================================================================
static std::atomic<bool> g_libReady{false};
static std::atomic<bool> g_inv{false};
static std::atomic<int>  g_dmgMul{1};      // 1 = 关闭
static std::atomic<bool> g_retryOn{false}; // 重试判胜：服务端判负就自动重开（博服务端算出的胜）
static std::atomic<bool> g_reenter{false};

// ============================================================================
//  ①b Cocos JS 注入桥（阶段② 第 1 步：先把「已解密的游戏脚本」抓到手）
//
//  为什么这么走：底包 assets/assets/main/index.jsc 是 XXTEA 整文件加密，
//  key 编译进 libcocos.so，离线解密不可行；但运行时引擎会解密后交给
//  System.register 注册模块。因此：
//    1) native → 反射 com.starry.magic.cocos.CocosJsBridge.evalOnGLThread(String)
//       （框架既有能力：com.cocos.lib.CocosHelper.runOnGameThread + CocosJavascriptJavaBridge.evalString）
//    2) 注入的驱动 JS 有两条抓源码通道：
//       (a) 挂 System.register，把每个模块的 declare 函数 Function.prototype.toString()
//       (b) System.registerRegistry[name][1]（未实例化模块）与 System.get(name) 命名空间快照
//    3) 落盘 jsb.fileUtils.getWritablePath()+"xnbz_dump.js"，用 root 拉回本地分析
//  先例：Games/BlzzMod.cpp（同 Cocos 3.x，走 CocosJsBridge.evalOnGLThread + System.get("chunks:///_virtual/<M>.ts")）
// ============================================================================
static JavaVM* g_jvm = nullptr;
static std::atomic<int> g_dumpTries{40};   // Tick 轮询次数（tickIntervalMs=1000 ⇒ 约 40s 窗口）

/**
 * 同步互斥 + 异常安全地执行一段 JS（走框架 CocosJsBridge，内部投递到游戏线程）。
 * 任何失败只打日志，绝不抛出到调用方（Tick/UI 线程都不能被拖崩）。
 */
// ============================================================
//  ★ Java 诊断壳句柄（只在 JNI_OnLoad 的 Java 栈帧内解析）
//     native 线程直接 FindClass("com/starry/...") 会失败（classloader 不含应用类，
//     实测日志：eval#0 FindClass XnbzHooks FAIL（classloader））⇒ 必须缓存全局引用。
// ============================================================
static jclass    g_hooksCls  = nullptr;
static jmethodID g_hooksPump = nullptr;
static jmethodID g_hooksIsPatched = nullptr;

void Xnbz_OnLoad(JavaVM* vm) {
    if (vm) g_jvm = vm;
    if (g_hooksCls) return;
    if (!vm) { LOGW(XNBZ_LOG " onLoad: vm null"); return; }
    JNIEnv* env = nullptr;
    if (vm->GetEnv((void**)&env, JNI_VERSION_1_6) != JNI_OK || !env) {
        LOGW(XNBZ_LOG " onLoad: no env（非 Java 栈帧）");
        return;
    }
    jclass c = env->FindClass(OBFUSCATE("com/starry/magic/cocos/XnbzHooks"));
    if (!c) {
        env->ExceptionClear();
        LOGW(XNBZ_LOG " onLoad: FindClass XnbzHooks 失败");
        return;
    }
    jmethodID m = env->GetStaticMethodID(c, "pump", "(Ljava/lang/String;)V");
    if (!m) {
        env->ExceptionClear();
        env->DeleteLocalRef(c);
        LOGW(XNBZ_LOG " onLoad: pump 方法 ID 获取失败");
        return;
    }
    g_hooksCls = (jclass)env->NewGlobalRef(c);
    g_hooksPump = m;
    jmethodID mp = env->GetStaticMethodID(c, "isPatched", "()Z");
    if (mp) g_hooksIsPatched = mp; else env->ExceptionClear();
    env->DeleteLocalRef(c);
    LOGI(XNBZ_LOG " onLoad: XnbzHooks 已缓存（classloader ok）");
}

/**
 * 查一次「JS 补丁是否已装上」（Java 侧记录最近一条 JS 回传）。
 * 只在 Tick 里低频调用；失败一律返回 false，不抛不崩。
 */
static bool QueryJsPatched() {
    if (!g_jvm || !g_hooksCls || !g_hooksIsPatched) return false;
    bool detach = false;
    JNIEnv* env = nullptr;
    int st = g_jvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_jvm->AttachCurrentThread(&env, nullptr) != 0) return false;
        detach = true;
    } else if (st != JNI_OK || !env) {
        return false;
    }
    jboolean r = env->CallStaticBooleanMethod(g_hooksCls, g_hooksIsPatched);
    if (env->ExceptionCheck()) { env->ExceptionClear(); r = JNI_FALSE; }
    if (detach) g_jvm->DetachCurrentThread();
    return r == JNI_TRUE;
}

static void EvalJs(const char* js) {
    if (!g_jvm || !js) { LOGW(XNBZ_LOG " eval skip: jvm=%p js=%p", (void*)g_jvm, (const void*)js); return; }
    bool detach = false;
    JNIEnv* env = nullptr;
    int st = g_jvm->GetEnv((void**)&env, JNI_VERSION_1_6);
    if (st == JNI_EDETACHED) {
        if (g_jvm->AttachCurrentThread(&env, nullptr) != 0) { LOGW(XNBZ_LOG " eval skip: attach failed"); return; }
        detach = true;
    } else if (st != JNI_OK || !env) {
        LOGW(XNBZ_LOG " eval skip: GetEnv st=%d", st);
        return;
    }
    static std::atomic<int> s_callSeq{0};
    int seq = s_callSeq.fetch_add(1);
    // Java 侧诊断壳：XnbzHooks.pump() 逐步打 StarryTrace（可控失败点定位）
    // 句柄优先用 JNI_OnLoad 缓存的全局引用（native 线程 FindClass 解析不到应用类）
    jclass cls = g_hooksCls;
    jmethodID mid = g_hooksPump;
    bool localCls = false;
    if (!cls || !mid) {
        jclass lc = env->FindClass(OBFUSCATE("com/starry/magic/cocos/XnbzHooks"));
        if (!lc) {
            env->ExceptionClear();
            if (seq < 3) LOGW(XNBZ_LOG " eval#%d FindClass XnbzHooks FAIL（classloader）", seq);
            if (detach) g_jvm->DetachCurrentThread();
            return;
        }
        cls = lc;
        localCls = true;
        mid = env->GetStaticMethodID(cls, "pump", "(Ljava/lang/String;)V");
        if (!mid) {
            env->ExceptionClear();
            if (seq < 3) LOGW(XNBZ_LOG " eval#%d GetStaticMethodID pump FAIL", seq);
            env->DeleteLocalRef(lc);
            if (detach) g_jvm->DetachCurrentThread();
            return;
        }
    }
    jstring jjs = env->NewStringUTF(js);
    if (jjs) {
        if (seq < 3) LOGI(XNBZ_LOG " eval#%d call pump (len=%d)", seq, (int)strlen(js));
        env->CallStaticVoidMethod(cls, mid, jjs);
        if (env->ExceptionCheck()) {
            env->ExceptionClear();
            LOGW(XNBZ_LOG " eval#%d pump threw", seq);
        }
        env->DeleteLocalRef(jjs);
    }
    if (localCls) env->DeleteLocalRef(cls);
    if (detach) g_jvm->DetachCurrentThread();
}
// 抓取已解密游戏脚本（幂等；可重复 eval，内部有版本守卫）
static const char* kDumpJs = R"JS(
(function(){
  var g=(typeof globalThis!=="undefined")?globalThis:((typeof window!=="undefined")?window:this);
  var S=(typeof System!=="undefined")?System:null;
  var dirs=[];
  try{ var wp=jsb.fileUtils.getWritablePath(); if(wp) dirs.push(wp); }catch(e){}
  dirs.push("/data/user/0/com.qqsy.xnbz/files/");
  dirs.push("/sdcard/Android/data/com.qqsy.xnbz/files/");
  function wf(name,txt){
    for(var i=0;i<dirs.length;i++){
      try{ if(jsb.fileUtils.writeStringToFile(txt, dirs[i]+name)) return dirs[i]+name; }catch(e){}
    }
    return "";
  }
  var st=g.__xnbzStatus||(g.__xnbzStatus=[]);
  function say(m){
    try{ st.push(m); if(st.length>300) st.shift(); }catch(e){}
    try{ console.log("[XnbzDump] "+m); }catch(e){}
    try{ wf("xnbz_status.txt", st.join("\n")+"\n"); }catch(e){}
  }
  say("inject dirs="+dirs.join(",")+" hasSystem="+(!!S));
  if(!S) return;
  if(g.__xnbzDumpVer>=3) return;
  g.__xnbzDumpVer=3;
  var store=g.__xnbzMods||(g.__xnbzMods={});
  function safeStr(f){ try{ return Function.prototype.toString.call(f); }catch(e){ return ""; } }
  function put(name,src){ if(!name) return; if(typeof src==="string"&&src.length>32) store[name]=src; else if(!(name in store)) store[name]=""; }
  var names=[];
  try{ var it=S.entries(),c; while(!(c=it.next()).done) names.push(c[0]); }catch(e){ say("entries err "+e); }
  say("modules="+names.length);
  say("MODLIST::"+names.join(" | "));
  var nreg=0;
  try{
    var R=S.registerRegistry;
    if(R){ for(var k in R){ var v=R[k]; if(v&&typeof v[1]==="function"){ put(k,safeStr(v[1])); nreg++; } } }
  }catch(e){ say("reg err "+e); }
  var walked=0;
  for(var i=0;i<names.length;i++){
    try{
      var ns=S.get(names[i]); if(!ns) continue;
      var keys=Object.keys(ns), parts=[], got=0;
      for(var j=0;j<keys.length;j++){
        var vv; try{ vv=ns[keys[j]]; }catch(e2){ continue; }
        if(typeof vv==="function"){ var sv=safeStr(vv); if(sv){ parts.push("// ns."+keys[j]+"\n"+sv); got++; } }
      }
      if(got>0&&!store[names[i]]){ store[names[i]]="///// NAMESPACE "+names[i]+" /////\n"+parts.join("\n"); walked++; }
    }catch(e3){}
  }
  if(!g.__xnbzRegHooked&&typeof S.register==="function"){
    var orig=S.register;
    S.register=function(name,deps,declare){
      try{ var d=(typeof deps==="function")?deps:declare; if(typeof d==="function") put(name,safeStr(d)); }catch(e){}
      return orig.apply(this,arguments);
    };
    g.__xnbzRegHooked=1;
  }
  say("reg="+nreg+" walked="+walked+" hooked="+!!g.__xnbzRegHooked);
  var rounds=0;
  function flush(){
    rounds++;
    try{
      var out=["///// MODULE NAMES ("+names.length+") /////\n"+names.join("\n")+"\n"];
      var cnt=0,bytes=0;
      for(var k in store){ var v=store[k]; if(typeof v!=="string"||!v) continue; out.push("///// MOD "+k+" /////\n"+v); cnt++; bytes+=v.length; }
      var p2=wf("xnbz_dump.js", out.join("\n"));
      say("flush#"+rounds+" mods="+cnt+" bytes="+bytes+" path="+p2);
    }catch(e){ say("flush err "+e); }
  }
  flush();
  var t=setInterval(function(){ flush(); if(rounds>=12) clearInterval(t); },3000);
})();
)JS";

// 独立催抓线程：不依赖框架 Tick 循环（Tick 只在 session_live() 授权通过后才跑）
static std::atomic<bool> g_dumpThreadStarted{false};

static void* XnbzDumpThread(void*) {
    LOGI(XNBZ_LOG " dump thread start (1s x 300)");
    for (int i = 0; i < 300; i++) {
        EvalJs(kDumpJs);
        if ((i % 30) == 0) LOGI(XNBZ_LOG " dump pump i=%d", i);
        sleep(1);
    }
    LOGI(XNBZ_LOG " dump thread end");
    return nullptr;
}

static void StartDumpThread() {
    if (g_dumpThreadStarted.exchange(true)) return;
    pthread_t tid;
    if (pthread_create(&tid, nullptr, XnbzDumpThread, nullptr) == 0) {
        pthread_detach(tid);
    } else {
        LOGW(XNBZ_LOG " dump thread create failed");
    }
}

// ============================================================================
//  ② 阶段② 功能注入（无敌 / 伤害倍率）—— JS 层补丁
//
//  为什么走 JS 层而不是 native hook：
//    底包 assets/assets/main/index.jsc 是整文件加密，key 在 libcocos.so 里；
//    运行时由引擎解密后经 System.register 注册为模块（模块名 chunks:///_virtual/<X>.ts）。
//    libcocos.so 28MB 且符号全隐藏（dynsym 仅 3132 项，无 se::ScriptEngine::evalString），
//    native 侧找不到可挂的稳定符号 ⇒ 改在 JS 层按「模块名 + 方法」打补丁，
//    native 只负责用框架 CocosJsBridge.evalOnGLThread 注入补丁与同步开关。
//
//  补丁锚点（证据来自运行时 dump：/tmp/xnbz/dump/xnbz_dump.js，模块名与行内实现均已核对）：
//    · chunks:///_virtual/BattleUnit.ts  → class BattleUnit
//        get isAlive(){return isUnitAlive(this.unitStateStore)}   // isUnitAlive = currentHp>0
//        get health(){return getUnitCurrentHp(...)} / get maxHealth(){return getUnitMaxHp(...)}
//        applySequentialDamage(v){ i=Math.max(0, hp-v); attributes[AttributeId.Hp]=i }   // Hp=2
//        updateState(st){ this.unitStateStore={...st,...} }   // ← 服务端 stateAfter 覆盖 HP 的唯一入口
//    · chunks:///_virtual/BattleManager.ts → class BattleManager（单例 instance）
//        get playerUnits()/enemyUnits()/getUnitById(id)
//        applyActionEffect(t){ ...; t.stateAfter.forEach(x=>getUnitById(x.unitId).updateState(x)) }
//        applyActionEffectSegmentOnly(e){ e.effectType===EffectType.Damage && e.value>0 ? unit.applySequentialDamage(e.value) : ... }
//      EffectType.Damage = "damage"（字符串枚举）；EffectType.Heal = "heal"
//
//  无敌（三处联动，缺一处就会被服务端 stateAfter 冲掉）：
//    ① applySequentialDamage：我方单位直接吞掉，不扣血
//    ② updateState：服务端下发 stateAfter 后把我方 currentHp 拉回 maxHp（attributes[Hp] 同步）
//    ③ isAlive / health 取值器：我方恒 true / 恒上限（视图与「能否行动」判定都用它）
//  伤害倍率：applyActionEffect / applyActionEffectSegmentOnly 里把「打在敌方单位身上」的
//    damage 效果 value 乘倍率（同一对象被伤害飘字与扣血共用 ⇒ 数字与血条同时变化）。
//
//  ★ 服务端权威结果的处理点（阶段② 追加，用户明确要求）：
//    本作战斗是「服务端算好 battleResult → 客户端回放」，服务端结果在 fightbegin
//    就已定稿，客户端唯一能改的是**本地处理点**，因此补丁 v3 同时压三处：
//      ① BattleUnit.updateState：用「服务端 HP 差值」做本地血量账本
//         （敌方差值 × 倍率放大，我方差值在无敌时清零）⇒ HP/死亡/提前结束真实生效
//      ② BattleManager.startBattle：本地 battleResult.result.winner = 1（攻击方胜），
//         让回放结束时的胜负展示与本地判定一致
//      ③ Network.requestFightResult：把服务端 fightresult 返回的 result.isWin
//         强制为 true ⇒ 结算界面走胜利分支（不再弹失败礼包/失败页）
//    边界如实告知：奖励明细仍来自服务端那份数据；若服务端判定为败，客户端拿到的是
//    败方数据包，这里只改胜负判定与展示分支，不改服务端账目。
// ============================================================================
static std::atomic<bool> g_syncThreadStarted{false};
static std::atomic<bool> g_patchDone{false};

// 开关同步（每次注入都刷新 JS 侧全局量；补丁读这两个量，因此切换零延迟）
static void SyncFlagsJs() {
    char buf[320];
    snprintf(buf, sizeof(buf),
             "(function(){var g=(typeof window!=='undefined')?window:globalThis;"
             "g.__xnbzInv=%s;g.__xnbzDmgMul=%d;g.__xnbzRetryOn=%s;})();",
             g_inv.load() ? "true" : "false", (int)g_dmgMul.load(),
             g_retryOn.load() ? "true" : "false");
    EvalJs(buf);
}

// 补丁本体 v5（幂等：g.__xnbzPatchVer>=5 直接返回；模块未到齐则等下一轮再注入）
// v4/v5「重试判胜」：服务端在 game.fightbegin 时就把它算出的整场战报+胜负给客户端，
// 奖励（changes.items）也由服务端那份回包决定 ⇒ 客户端改 isWin 只能改显示，改不了账目。
// 唯一不动服务端数据就能拿到真奖励的路子：重开这场战斗，让服务端重新算一次。
// v5 修正触发时机：v4 在「回放开始」就重开（战斗还没打完就被打断，实测没产生第二场），
// 改成在「结算回包到手」之后重开（此时 UI 已退出战斗），并且这一场不伪造胜利、不弹失败礼包。
static const char* kPatchJs = R"JS(
(function(){
  var g=(typeof window!=='undefined')?window:globalThis;
  if(typeof g.__xnbzInv==='undefined') g.__xnbzInv=false;
  if(typeof g.__xnbzDmgMul==='undefined') g.__xnbzDmgMul=1;
  if(typeof g.__xnbzRetryOn==='undefined') g.__xnbzRetryOn=false;
  if(typeof g.__xnbzRetry==='undefined') g.__xnbzRetry=0;   // ★ 不能用赋值重置：Tick 每 2s 重注入
  if(!g.__xnbzStat) g.__xnbzStat={inv:0,dmg:0,err:0,win:0,seg:0};
  if(!g.__xnbzSrvHp) g.__xnbzSrvHp={};      // unitId -> 最近一次看到的服务端 HP
  if(!g.__xnbzLocHp) g.__xnbzLocHp={};      // unitId -> 本地账本 HP（真正生效的那个）
  function log(m){
    try{ console.log("[XnbzPatch] "+m); }catch(e){}
    try{ jsb.reflection.callStaticMethod("com/starry/magic/cocos/XnbzHooks","logStatus","(Ljava/lang/String;)V",m); }catch(e){}
  }
  var S=(typeof System!=='undefined')?System:null;
  if(!S||!S.get) return;
  if(g.__xnbzPatchVer>=5) return;
  var bm=null,mm=null,nn=null,md=null;
  try{ bm=S.get("chunks:///_virtual/BattleUnit.ts"); }catch(e){}
  try{ mm=S.get("chunks:///_virtual/BattleManager.ts"); }catch(e){}
  try{ nn=S.get("chunks:///_virtual/Network.ts"); }catch(e){}
  try{ md=S.get("chunks:///_virtual/BattleModule.ts"); }catch(e){}
  if(!bm||!mm) return;                                  // 模块还没注册，等下一轮
  if(!bm.BattleUnit||!mm.BattleManager) return;
  var BU=bm.BattleUnit.prototype;
  var BM=mm.BattleManager.prototype;
  var CtorBU=bm.BattleUnit, CtorBM=mm.BattleManager;
  var HP=2;                                             // AttributeId.Hp
  function ours(u){
    try{
      var M=CtorBM.instance;
      return !!(M&&M.playerUnits&&M.playerUnits.indexOf(u)>=0);
    }catch(e){ return false; }
  }
  function inv(){ return !!g.__xnbzInv; }
  function mul(){ var v=g.__xnbzDmgMul|0; return v>1?v:1; }
  function active(){ return inv()||mul()>1; }

  // ================= ① 本地血量账本：让扣血/死亡真正发生 =================
  // 服务端 stateAfter 是绝对血量，直接透传会把本地改动一次次冲掉（用户实测的「假生效」）。
  // 因此改为：服务端 HP 只用来算「这一段扣了多少」，本地 HP 自己记账。
  var _us=BU.updateState;
  BU.updateState=function(st){
    if(!active()||!st) return _us.apply(this,arguments);
    var isOurs=ours(this);
    if(!( (isOurs&&inv()) || (!isOurs&&mul()>1) )) return _us.apply(this,arguments);
    try{
      var id=st.unitId;
      var srvNext=st.currentHp|0;
      var prevLocal=(this.unitStateStore&&typeof this.unitStateStore.currentHp==='number')
                      ? (this.unitStateStore.currentHp|0) : srvNext;
      var seen=g.__xnbzSrvHp[id];
      var lastSrv=(typeof seen==='number')?seen:prevLocal;   // 首次介入：本地==服务端
      var cur=(typeof g.__xnbzLocHp[id]==='number')?g.__xnbzLocHp[id]:prevLocal;
      var dmg=lastSrv-srvNext;                               // >0 = 这一段服务端扣的血
      var maxHp=(st.maxHp|0)||(this.unitStateStore&&this.unitStateStore.maxHp|0)||0;
      if(isOurs){
        // 无敌：只涨不跌（治疗照常，扣血归零）
        if(dmg<0) cur=cur+(-dmg);
      }else{
        if(dmg>0){
          var m=mul();
          cur=cur-dmg*m;
          g.__xnbzStat.dmg++;
          if((g.__xnbzStat.dmg%50)===1) log("enemy hp -"+dmg+" x"+m+" -> "+Math.max(0,cur));
        }else if(dmg<0){
          cur=cur+(-dmg);
        }
      }
      if(maxHp>0&&cur>maxHp) cur=maxHp;
      if(cur<0) cur=0;
      g.__xnbzSrvHp[id]=srvNext;
      g.__xnbzLocHp[id]=cur;
      var s={};                                            // 拷贝一份，别改服务端共享对象
      for(var k in st) if(Object.prototype.hasOwnProperty.call(st,k)) s[k]=st[k];
      var attrs={};
      if(st.attributes) for(var a in st.attributes) if(Object.prototype.hasOwnProperty.call(st.attributes,a)) attrs[a]=st.attributes[a];
      s.currentHp=cur;
      attrs[HP]=cur;
      if(cur<=0) attrs[HP]=0;
      s.attributes=attrs;
      return _us.call(this,s);
    }catch(e){ g.__xnbzStat.err++; return _us.apply(this,arguments); }
  };

  // 分段回放（弹道）走的是逐段扣血，没有 stateAfter 跟进 ⇒ 在这里就按倍率放大 / 无敌吞掉
  var _sd=BU.applySequentialDamage;
  BU.applySequentialDamage=function(v){
    if(!active()) return _sd.apply(this,arguments);
    try{
      var isOurs=ours(this);
      if(isOurs&&inv()){ g.__xnbzStat.inv++; if((g.__xnbzStat.inv%50)===1) log("inv blocked dmg="+v); return; }
      if(!isOurs&&mul()>1&&v>0){
        var m=mul();
        g.__xnbzStat.seg++;
        if((g.__xnbzStat.seg%50)===1) log("seg dmg x"+m+" -> "+(v*m));
        return _sd.call(this,v*m);
      }
    }catch(e){ g.__xnbzStat.err++; }
    return _sd.apply(this,arguments);
  };

  // 无敌兜底：本地账本已保证不掉血，这里再锁一次判定取值器（视图读的是这两个 getter）
  function desc(o,k){ try{ return Object.getOwnPropertyDescriptor(o,k); }catch(e){ return null; } }
  var dAlive=desc(BU,'isAlive'), dHp=desc(BU,'health');
  if(dAlive&&dAlive.get){
    Object.defineProperty(BU,'isAlive',{configurable:true,get:function(){
      if(inv()&&ours(this)) return true;
      return dAlive.get.call(this);
    }});
  }
  if(dHp&&dHp.get){
    Object.defineProperty(BU,'health',{configurable:true,get:function(){
      if(inv()&&ours(this)){
        var v=dHp.get.call(this);
        return v;
      }
      return dHp.get.call(this);
    }});
  }

  // 伤害数字（表现层）：与放大后的扣血保持一致
  function scale(t){
    try{
      var m=mul();
      if(m<=1||!t||!t.effects) return;
      for(var i=0;i<t.effects.length;i++){
        var e=t.effects[i];
        if(!e||e.effectType!=='damage'||!(e.value>0)) continue;
        var u=null;
        try{ u=this.getUnitById(e.targetId); }catch(err){}
        if(u&&!ours(u)) e.value=e.value*m;
      }
    }catch(err){ g.__xnbzStat.err++; }
  }
  var _ae=BM.applyActionEffect;
  BM.applyActionEffect=function(t){ scale.call(this,t); return _ae.apply(this,arguments); };
  var _ase=BM.applyActionEffectSegmentOnly;
  if(_ase){
    BM.applyActionEffectSegmentOnly=function(e){
      scale.call(this,{effects:[e]});
      return _ase.apply(this,arguments);
    };
  }

  // ================= ② 本地胜负标记 + ④ 重试判胜 =================
  // 服务端在 fightbegin 时就把整场战报连胜负一起给过来（applyBattleResultAndEnterBattle
  // 里 msgpack 解出的就是它）⇒ 这里能读到「服务端真实胜负 rawWin」，强制改写前先留证。
  function retryOn(){ return !!g.__xnbzRetryOn; }
  var RETRY_MAX=3;
  // 「本次战斗服务端判负，且开着重试」→ 这次不伪造胜利，等结果页出来后重开本场
  function pendingLoss(){
    var w=g.__xnbzSrvWin;
    return retryOn()&&(typeof w==='number')&&w!==1;
  }
  function retryNow(){
    var st=g.__xnbzLastStart, mo=g.__xnbzBmod;
    if(!st||!mo){ log("retry 无法执行：缺 BattleModule 实例或关卡参数"); return; }
    try{ mo.startBattle(st.type,st.id,st.extra); }
    catch(e){ log("retry err "+e); }
  }
  var _sb=BM.startBattle;
  BM.startBattle=function(br){
    try{
      var rawWin=(br&&br.result&&typeof br.result.winner!=='undefined')?br.result.winner:null;
      var acts=(br&&br.actions&&br.actions.length)?br.actions.length:0;
      g.__xnbzSrvWin=rawWin;                         // 服务端真实胜负（强制改写前留证）
      if(rawWin!==null){
        var fp="win="+rawWin+" actions="+acts;
        if(g.__xnbzLastFp!==fp){ g.__xnbzLastFp=fp; log("replay try#"+(g.__xnbzRetry|0)+" "+fp); }
      }
      if(active()){
        g.__xnbzSrvHp={}; g.__xnbzLocHp={};          // 新一场战斗：账本清零（单位会从对象池复用）
        if(rawWin!==null&&rawWin!==1&&!pendingLoss()){
          br.result.winner=1;                          // 1 = 攻击方（我方）胜利
          log("local winner -> attacker");
        }
      }
    }catch(e){ g.__xnbzStat.err++; }
    return _sb.apply(this,arguments);
  };

  // ================= ③ 服务端权威结算结果的处理点 =================
  // 结算回包里的 result.isWin 与 changes（奖励明细）都是服务端给的：
  //   · 强制胜利 = 只改显示分支（拿不到奖励）
  //   · 重试判胜 = 关掉这一场的伪造，先让服务端判负的结果如实出来，然后重开本场让服务端重算
  if(nn&&nn.Network){
    var NP=nn.Network.prototype;
    var _rf=NP.requestFightResult;
    if(_rf){
      NP.requestFightResult=function(req){
        var p=_rf.apply(this,arguments);
        if((!active()&&!retryOn())||!p||typeof p.then!=='function') return p;
        return p.then(function(res){
          try{
            var raw=g.__xnbzSrvWin;
            var items=(res&&res.data&&res.data.changes&&res.data.changes.items&&res.data.changes.items.length)
                        ? res.data.changes.items.length : 0;
            log("settle: rawWin="+raw+" rewardItems="+items+" retry="+(g.__xnbzRetry|0));
            if(!res||!res.data||!res.data.result) return res;
            if(pendingLoss()&&(g.__xnbzRetry|0)<RETRY_MAX){
              g.__xnbzRetry=(g.__xnbzRetry|0)+1;
              res.data.result.isWin=false;                       // 诚实显示：不伪造这一场的胜利
              log("server judged LOSS -> retry #"+g.__xnbzRetry+"/"+RETRY_MAX+" (重开本场，让服务端重算)");
              setTimeout(retryNow,1500);
              return res;
            }
            if(pendingLoss()){
              if(!g.__xnbzRetryCapHit){
                g.__xnbzRetryCapHit=true;
                log("retry cap hit ("+RETRY_MAX+") -> 几次重算的战报一模一样，重试改不了胜负");
              }
            }else if(raw===1&&(g.__xnbzRetry|0)>0){
              log("server judged WIN after retry #"+g.__xnbzRetry+" -> 奖励应由服务端正常下发");
              g.__xnbzRetry=0; g.__xnbzRetryCapHit=false;
            }
            if(!pendingLoss()){
              res.data.result.isWin=true;
              if(res.data.result.winner!==1) res.data.result.winner=1;
              g.__xnbzStat.win++;
              log("force win #"+g.__xnbzStat.win+" (server result.isWin -> true)");
            }
          }catch(e){ g.__xnbzStat.err++; }
          return res;
        });
      };
      log("fightresult hook armed (Network.requestFightResult)");
    }else{
      log("WARN: Network.requestFightResult 不存在，结算强制胜利未挂上");
    }
  }else{
    log("WARN: Network 模块未取到，结算强制胜利未挂上");
  }

  // ================= ④ 战斗入口：记下本次战斗的 type/id/extra，重试时原样重放 =================
  if(md&&md.BattleModule&&md.BattleModule.prototype&&typeof md.BattleModule.prototype.startBattle==='function'){
    var MDP=md.BattleModule.prototype;
    var _mdStart=MDP.startBattle;
    MDP.startBattle=function(t,e,n){
      try{
        var prev=g.__xnbzLastStart;
        if(!prev||prev.type!==t||prev.id!==e){
          g.__xnbzRetry=0; g.__xnbzRetryCapHit=false; g.__xnbzLastFp=null;   // 换关卡：重试计数清零
        }
        g.__xnbzLastStart={type:t,id:e,extra:(typeof n==='undefined'?0:n)};
        g.__xnbzBmod=this;
        g.__xnbzSrvWin=null;                          // 本场胜负以本场的战报为准
        if((g.__xnbzRetry|0)>0) log("retry 重开本场 type="+t+" id="+e+" (第 "+(g.__xnbzRetry|0)+" 次)");
      }catch(x){}
      return _mdStart.apply(this,arguments);
    };
    // 重试期间不弹「失败礼包」弹窗（否则会挡住自动重开）
    if(typeof MDP.requestFailGift==='function'){
      var _rfg=MDP.requestFailGift;
      MDP.requestFailGift=function(){
        if(retryOn()&&(g.__xnbzRetry|0)>0){ log("suppress fail gift (retrying)"); return; }
        return _rfg.apply(this,arguments);
      };
      log("fail-gift suppressed while retrying");
    }
    log("retry hook armed (BattleModule.startBattle)");
  }else{
    log("WARN: BattleModule.startBattle 未取到，重试判胜不可用");
  }

  g.__xnbzPatchVer=5;
  log("patch v5 installed (hp-ledger + inv + dmg-mul + force-win + retry-until-server-win)");
})();
)JS";

// 全关兜底（ForceClear / 掉线）：立即把 JS 侧开关归零
static const char* kForceClearJs = R"JS(
(function(){
  var g=(typeof window!=='undefined')?window:globalThis;
  g.__xnbzInv=false; g.__xnbzDmgMul=1; g.__xnbzRetryOn=false;
  try{ console.log("[XnbzPatch] force clear"); }catch(e){}
})();
)JS";

// 补丁保活线程：不依赖框架 Tick 循环（Tick 只在 session_live() 授权通过后才跑，
// 且首帧时游戏 bundle 可能还没注册 BattleUnit/BattleManager 模块 ⇒ 需要重试）。
static void* XnbzSyncThread(void*) {
    LOGI(XNBZ_LOG " sync thread start (2s x 300)");
    for (int i = 0; i < 300; i++) {
        SyncFlagsJs();
        EvalJs(kPatchJs);
        if ((i % 30) == 0) LOGI(XNBZ_LOG " sync pump i=%d inv=%d mul=%d",
                                 i, (int)g_inv.load(), (int)g_dmgMul.load());
        // 补丁是否真装上：由 JS 回传（Java 侧记录），每 10s 查一次，成功只报一次
        if (!g_patchDone.load() && (i % 5) == 0 && QueryJsPatched()) {
            g_patchDone.store(true);
            LOGI(XNBZ_LOG " js patch confirmed (BattleUnit/BattleManager)");
        }
        sleep(2);
    }
    LOGI(XNBZ_LOG " sync thread end");
    return nullptr;
}

static void StartSyncThread() {
    if (g_syncThreadStarted.exchange(true)) return;
    pthread_t tid;
    if (pthread_create(&tid, nullptr, XnbzSyncThread, nullptr) == 0) {
        pthread_detach(tid);
    } else {
        LOGW(XNBZ_LOG " sync thread create failed");
    }
}

// ============================================================================
//  ④ 并发 + 致命信号兜底 + 槽位熔断
//  为什么需要（三段真实事故，删任一段都会复现）：
//    (1) 并发装钩竞态：UI 线程与游戏线程同时进 SyncHooks，
//        HookSlotInstall 的幂等判断是 check-then-act 非原子
//        ⇒ 同址 DobbyHook 两次 ⇒ DobbyCodePatch 越界 memcpy ⇒ SIGSEGV(SEGV_ACCERR)。
//    (2) DobbyHook 返回码接不住 Dobby 内部的进程级 SIGSEGV
//        ⇒ 必须用 sigsetjmp 保护区把「装钩」和「读序言」包起来。
//    (3) hook 体内会执行游戏/第三方代码 ⇒ 回调里的 null-deref 不该整包崩。
//  sigaltstack 是 per-thread 的，备用栈与故障态必须 thread_local。
// ============================================================================

// ---- 4.1 互斥：装钩串行 ----
static std::mutex g_installMutex;   // 保护「装钩全过程」，只在 SyncHooks 里取
static std::mutex g_guardMutex;     // 只保护一次性「安装信号处理器」动作

// ---- 4.2 本线程故障态 ----
static thread_local sigjmp_buf              t_faultJmp;
static thread_local volatile sig_atomic_t   t_inGuard    = 0;
static thread_local volatile sig_atomic_t   t_faultDepth = 0;
static thread_local volatile int            t_faultSig   = 0;
static thread_local volatile void*          t_faultAddr  = nullptr;

// 备用信号栈（SIGSEGV 时原栈可能已不可用）；64KB/线程
static thread_local char t_altStack[65536];
static thread_local bool t_altStackSet = false;

// ---- 4.3 被链式接管的旧处理器 ----
static struct sigaction g_oldSegv;
static struct sigaction g_oldBus;
static struct sigaction g_oldIll;
static std::atomic<bool> g_guardArmed{false};
static bool g_lastFatal = false;    // 装钩期是否发生过致命信号（只在 g_installMutex 下读写）

// ---- 4.4 故障处理器 ----
static void XnbzFaultHandler(int sig, siginfo_t* si, void* /*uc*/) {
    if (t_inGuard) {
        // 保护区内连炸：保护区本身不可靠 ⇒ 交回系统默认行为（宁可崩，不要死循环）
        if (++t_faultDepth > 4) {
            signal(sig, SIG_DFL);
            raise(sig);
            return;
        }
        t_faultSig  = sig;
        t_faultAddr = si ? si->si_addr : nullptr;
        siglongjmp(t_faultJmp, 1);   // 永不返回
    }

    // 保护区外：链回旧处理器，再走默认
    const struct sigaction* old = (sig == SIGSEGV) ? &g_oldSegv
                               : (sig == SIGBUS)  ? &g_oldBus
                                                  : &g_oldIll;
    if (old->sa_sigaction && old->sa_sigaction != XnbzFaultHandler) {
        old->sa_sigaction(sig, si, nullptr);
        return;
    }
    signal(sig, SIG_DFL);
    raise(sig);
}

// ---- 4.5 为本线程挂备用信号栈（懒执行，失败降级） ----
static void ArmFaultGuardForThread() {
    if (t_altStackSet) return;
    t_altStackSet = true;
    stack_t ss;
    std::memset(&ss, 0, sizeof(ss));
    ss.ss_sp    = t_altStack;
    ss.ss_size  = sizeof(t_altStack);
    ss.ss_flags = 0;
    if (sigaltstack(&ss, nullptr) != 0) {
        LOGW(XNBZ_LOG " sigaltstack failed — 兜底降级为无备用栈");
    }
}

// ---- 4.6 一次性安装进程级处理器 ----
static void InstallFaultGuard() {
    if (g_guardArmed.load()) return;
    std::lock_guard<std::mutex> lk(g_guardMutex);
    if (g_guardArmed.load()) return;

    struct sigaction sa;
    std::memset(&sa, 0, sizeof(sa));
    sa.sa_sigaction = XnbzFaultHandler;
    sa.sa_flags     = SA_SIGINFO | SA_ONSTACK | SA_NODEFER;
    sigemptyset(&sa.sa_mask);

    sigaction(SIGSEGV, &sa, &g_oldSegv);
    sigaction(SIGBUS,  &sa, &g_oldBus);
    sigaction(SIGILL,  &sa, &g_oldIll);

    g_guardArmed.store(true);
    LOGI(XNBZ_LOG " fault guard armed (SIGSEGV/SIGBUS/SIGILL, altstack=%zu)",
         sizeof(t_altStack));
}

// ---- 4.7 保护区包装器（阶段② 的 Dobby 装钩/读序言/跑回调全部走这里） ----
// 装钩：Dobby 内部崩（返回码接不住的那种）⇒ 返回 false + 置 g_lastFatal
static bool DobbyHookGuarded(const char* tag, void* target, void* hook, void** orig) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        int rc = DobbyHook(target, hook, orig);
        t_inGuard = 0;
        if (rc != 0) {
            LOGW(XNBZ_LOG " %s dobby fail rc=%d", tag, rc);
            return false;
        }
        return true;
    }
    // longjmp 回来：保护区内的致命信号已被吞掉
    t_inGuard = 0;
    g_lastFatal = true;
    LOGW(XNBZ_LOG " %s dobby SIG(%d) @%p — 槽位熔断，游戏继续",
         tag, (int)t_faultSig, (void*)t_faultAddr);
    if (orig) *orig = nullptr;   // ★ 必须清掉可能被半写的 orig，否则后续调用跳野指针
    return false;
}

// 读序言（RVA 漂移校验）：读坏页也会炸，同样走保护区
static bool ReadU32Guarded(const char* tag, const void* p, uint32_t* out) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        *out = *reinterpret_cast<const volatile uint32_t*>(p);
        t_inGuard = 0;
        return true;
    }
    t_inGuard = 0;
    LOGW(XNBZ_LOG " %s prologue read SIG(%d) @%p — 跳过该槽位",
         tag, (int)t_faultSig, (void*)t_faultAddr);
    return false;
}

// 跑第三方代码（游戏回调 / SDK 回调）：炸了吞掉，只记日志
static bool RunGuarded(const char* tag, void (*fn)(void*), void* arg) {
    ArmFaultGuardForThread();
    t_faultDepth = 0;
    if (sigsetjmp(t_faultJmp, 1) == 0) {
        t_inGuard = 1;
        fn(arg);
        t_inGuard = 0;
        return true;
    }
    t_inGuard = 0;
    LOGW(XNBZ_LOG " %s 回调内 SIG(%d) @%p — 已吞掉，游戏继续",
         tag, (int)t_faultSig, (void*)t_faultAddr);
    return false;
}

// ---- 4.8 槽位熔断（阶段② 装满钩槽位时启用） ----
static const int kSlotsMax = 64;
static bool g_slotDead[kSlotsMax];              // 只在 g_installMutex 下读写
static std::atomic<int> g_installedCount{0};

// ============================================================================
//  ⑤/⑥/⑦ TODO（阶段② 用户放行后填）
//    【无敌】     数据型嫌疑：战斗实体 HP 回写 / 受击结算旁路 → 先定 JS 层还是 native 层
//    【伤害倍率】 行为型嫌疑：伤害结算入口 × 倍率
//    Cocos 系候选路线（本阶段禁止展开，仅登记待办）：
//      (a) JS 层：hook se::ScriptEngine::evalString / JSB 求值桥 splice 已解密脚本
//          （求值桥符号见 SecStrings.hpp 的 sec_sym_cocos_jsb_eval()）
//      (b) native 层：libcocos.so 内战斗函数 RVA（需 dump 后回填 expectInsn 校验序言）
//    恢复 §⑤ 双路径挂点机器（HookAtRva + HookByName + HookSlotInstall）时，
//    参照同引擎先例的①开关/orig②偏移与 RVA⑤挂点表⑥功能组四段结构。
// ============================================================================

// 装钩入口（阶段② 已改走 JS 层补丁，见 §② 的 kPatchJs；native 侧保持 0 hook）
static int InstallBattleHooks() {
    // 阶段② 的功能不装 native hook（符号不可用，见 §② 注释）；
    // 返回 1 作为「补丁已接入」的哨兵，避免 SyncHooks 每 tick 重入并刷屏。
    static std::atomic<bool> s_logged{false};
    if (!s_logged.exchange(true)) {
        LOGI(XNBZ_LOG " install pass: native hooks=0, JS patch armed（pump=2s）");
    }
    return 1;
}

static bool AnyFeatureOn() {
    return g_inv.load() || g_dmgMul.load() > 1;
}

static void SyncHooks() {
    if (!g_libReady.load()) return;
    if (!AnyFeatureOn()) return;                        // 全关：不装、也不卸载
    if (g_installedCount.load() > 0) return;            // 快路径：已装

    std::lock_guard<std::mutex> lk(g_installMutex);     // ★ 串行化，防并发双挂
    if (g_installedCount.load() > 0) return;            // double-check
    g_lastFatal = false;
    g_installedCount.store(InstallBattleHooks());
}

// ============================================================================
//  ⑧ 对外接口（ModEntry.h 注册表契约，签名不可改）
// ============================================================================
bool Xnbz_TryInit(JavaVM* vm) {
    if (vm) g_jvm = vm;
    if (g_libReady.load()) return true;
    if (!isLibraryLoaded(XNBZ_TARGET_LIB)) return false;

    InstallFaultGuard();          // §④ 先装兜底，再谈功能
    g_libReady.store(true);
    SyncHooks();                  // 已 ON 的开关立即装；全关保持 0 hook
    LOGI(XNBZ_LOG " lib ready (cocos jsb, js-bridge armed, patch armed)");
    // 阶段② 功能注入：同步开关 + 装 JS 补丁（独立线程重试，不依赖 auth-gated 的 Tick 循环）
    SyncFlagsJs();
    EvalJs(kPatchJs);
    StartSyncThread();
    return true;
}

bool Xnbz_IsReady() {
    return g_libReady.load();
}

void Xnbz_OnFeatureChanged(int featNum, bool enabled) {
    switch (featNum) {
        case XNBZ_FEAT_INV:
            g_inv.store(enabled);
            LOGI(XNBZ_LOG " feat inv=%d", (int)enabled);
            break;
        case XNBZ_FEAT_DMG:
            g_dmgMul.store(enabled ? 2 : 1);
            LOGI(XNBZ_LOG " feat dmg toggle=%d", (int)enabled);
            break;
        case XNBZ_FEAT_RETRY:
            g_retryOn.store(enabled);
            LOGI(XNBZ_LOG " feat retry=%d", (int)enabled);
            break;
        default:
            break;
    }
    SyncFlagsJs();     // 立即把开关推给 JS（补丁常驻，切换零延迟）
    SyncHooks();
}

void Xnbz_OnValueChanged(int featNum, int value) {
    // 滑块类功能在此钳制并存入原子量（语义：1 = 关闭）
    if (featNum != XNBZ_FEAT_DMG) return;
    if (value < 1)   value = 1;
    if (value > 100) value = 100;
    g_dmgMul.store(value);
    LOGI(XNBZ_LOG " feat dmg mul=%d", value);
    SyncFlagsJs();
}

void Xnbz_Tick(const bool* features, const int* values) {
    if (!features) return;

    g_inv.store(features[XNBZ_FEAT_INV]);
    g_retryOn.store(features[XNBZ_FEAT_RETRY]);
    if (values) {
        int v = values[XNBZ_FEAT_DMG];
        if (v < 1)   v = 1;
        if (v > 100) v = 100;
        g_dmgMul.store(v);
    }

    // 兜底：库晚于首帧映射时，这里补一次
    if (!g_libReady.load() && isLibraryLoaded(XNBZ_TARGET_LIB)) {
        InstallFaultGuard();
        g_libReady.store(true);
        LOGI(XNBZ_LOG " lib ready (via Tick fallback)");
    }
    SyncHooks();

    // 阶段② 保活：每 ~2s（200ms × 10）重注入一次「开关同步 + JS 补丁」。
    // 补丁幂等（g.__xnbzPatchVer 守卫）；开局场景切换/热更重载后能自动补回。
    static std::atomic<int> s_tickSeq{0};
    if (g_libReady.load() && (s_tickSeq.fetch_add(1) % 10) == 0) {
        SyncFlagsJs();
        EvalJs(kPatchJs);
        if (!g_patchDone.load()) {
            // 直接问 Java 侧「JS 是否回过 patch installed」；连打 20 轮（约 40s）仍无才提示
            if (QueryJsPatched()) {
                g_patchDone.store(true);
                LOGI(XNBZ_LOG " js patch confirmed (BattleUnit/BattleManager)");
            } else {
                static std::atomic<int> s_retry{0};
                if (s_retry.fetch_add(1) == 20) LOGW(XNBZ_LOG " patch 未见 XnbzPatch 日志，确认模块名是否仍为 BattleUnit/BattleManager");
            }
        }
    }

    // ★ 禁止在此 SafeProbe / 扫堆 / 遍历对象 / 逐对象写（Tick 频率高，代价爆炸）
}

void Xnbz_ForceClear() {
    g_inv.store(false);
    g_dmgMul.store(1);
    g_retryOn.store(false);
    g_reenter.store(false);
    EvalJs(kForceClearJs);      // JS 侧开关同步归零（补丁常驻但立即失效）
}
