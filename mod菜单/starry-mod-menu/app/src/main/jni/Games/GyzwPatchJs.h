/* ================================================================
 *  GyzwPatchJs.h — 光阴之外 星光注入 JS（手写，非生成器产物）
 *
 *  机制：native Dobby hook se::ScriptEngine::evalString，在 boot JS
 *        （assets/script/index.jsc 解密后 ~1.87MB）文本上做
 *        「前缀追加 + 4 处锚点原地替换」，再交回原函数执行。
 *
 *  锚点来源：斌哥改包运行时 dump（10_docs/光阴之外/斌哥hook锚点_20260926.json）
 *           —— 只取锚点串与语义，注入体全部为本实现重写。
 *
 *  ★ 动态绑定：功能 1/2 需要脚本内 `X.BtConst.EUnitType` 的模块别名 X。
 *    该别名不固定，运行期在锚点附近捕获后填入 %s（rb 段占位）。
 *    捕获失败 ⇒ 放弃该条替换（BuildHotPatch 整体失败），不改盘不崩溃。
 *
 *  ★ 所有替换体必须自带 try/catch：即便 JS 抛错也只丢功能，不阻断游戏。
 * ================================================================ */
#pragma once

// 幂等标记：热更重载后若脚本已含此串则直通，不重复打补丁
static const char* kGyzwPatchMarker = "starry_gyzw_v1";

// 入场标记：三串齐全才认定「这是目标 boot JS」
static const char* kGyzwSign1 = "var u = t.isDie();";
static const char* kGyzwSign2 = "this.setCurHp(this.curHp + e, t);";
static const char* kGyzwSign3 = "return r && r[o] || this._oneSpeed;";

// ----------------------------------------------------------------
// 前缀（追加到脚本尾部）：定义 JS 侧开关查询全局，并回写自检日志
//   %s = 运行期捕获的 BtConst 模块别名
// ----------------------------------------------------------------
static const char* kGyzwPatchPrefixFmt = R"GYZWJS(
/*starry_gyzw_v1*/
;(function () {
  var H = "com.starry.magic.cocos.GyzwHooks";
  var g = (typeof globalThis !== "undefined") ? globalThis : window;
  function R(n) {
    try {
      var s = g.__starryGyzwState;
      if (s && s.ret && typeof s.ret[n] !== "undefined") return !!s.ret[n];
    } catch (e) {}
    try {
      if (typeof jsb === "undefined" || !jsb.reflection) return false;
      return !!jsb.reflection.callStaticMethod(H, "getRet", "(I)Z", n);
    } catch (e2) { return false; }
  }
  function V(n) {
    try {
      var s = g.__starryGyzwState;
      if (s && s.val && typeof s.val[n] !== "undefined") {
        var v0 = s.val[n] | 0;
        return v0 > 0 ? v0 : 1;
      }
    } catch (e) {}
    try {
      if (typeof jsb === "undefined" || !jsb.reflection) return 1;
      var v = jsb.reflection.callStaticMethod(H, "getVal", "(I)I", n) | 0;
      return v > 0 ? v : 1;
    } catch (e2) { return 1; }
  }
  g.__starryGyzwOn = R;
  g.__starryGyzwVal = V;
  g.__starryGyzwBV = "%s";
  try {
    var s = g.__starryGyzwState = g.__starryGyzwState || {};
    s.ret = s.ret || {};
    s.val = s.val || {};
    if (!s.logged && typeof jsb !== "undefined" && jsb.reflection) {
      s.logged = 1;
      jsb.reflection.callStaticMethod(H, "logStatus", "(Ljava/lang/String;)V",
        "gyzw js patched bv=" + g.__starryGyzwBV);
    }
  } catch (e) {}
})();
)GYZWJS";

// ----------------------------------------------------------------
// 功能1 百倍伤害  BtDmgMgr.updateTargetHp(t, e, o)
//   锚点：var u = t.isDie();
//   语义：受击方 t、伤害 e(<0 即伤害)、攻击方 o
//         o 为 Hero/Lord 且 t 为 Monster ⇒ e 放大 100 倍
// ----------------------------------------------------------------
static const char* kGyzwAnchor1 = "var u = t.isDie();";

static const char* kGyzwRepl1Fmt =
    "var u = t.isDie();"
    "try{if(window.__starryGyzwOn(1)&&e<0&&o&&t){"
    "var _g1t=%s.BtConst.EUnitType;"
    "if((o.type===_g1t.Hero||o.type===_g1t.Lord)&&t.type===_g1t.Monster){"
    "e=Math.floor(100*e);}}}catch(_g1e){}\n";

// ----------------------------------------------------------------
// 功能2 超级无敌  BtRoleData.updateHp(e, t)
//   锚点：this.setCurHp(this.curHp + e, t);
//   语义：不硬无敌，把英雄单次掉血夹到「当前HP - 91%~95% 上限」之内
//         （留残血 + 每次随机，降低服务端对账命中率）
// ----------------------------------------------------------------
static const char* kGyzwAnchor2 = "this.setCurHp(this.curHp + e, t);";

static const char* kGyzwRepl2Fmt =
    "try{if(window.__starryGyzwOn(2)&&e<0&&this.type===%s.BtConst.EUnitType.Hero){"
    "if(typeof this.__mfl!=='number'){"
    "this.__mfl=Math.floor(this.maxHp*(0.91+0.04*Math.random()));}"
    "e=Math.max(e,Math.min(0,this.__mfl-this.curHp));}}catch(_g2e){}\n"
    "this.setCurHp(this.curHp + e, t);";

// ----------------------------------------------------------------
// 功能3 战斗加速（滑块 1..10，1=关）
//   3a  BattleViewHelper.getBattleSpeed
//        锚点：return r && r[o] || this._oneSpeed;
//        命中时返回放大 trueSpeed 的新对象（不改原对象，避免污染缓存）
//   3b  BattleView 逻辑步上限
//        锚点：for (var e = Math.min(3, Math.floor(this._logicAccumulator / this._logicFrameRate)), t = 0; t < e; t++) {
//        不放开这条，3a 会被逻辑步 min(3) 卡在约 3×
// ----------------------------------------------------------------
static const char* kGyzwAnchor3 = "return r && r[o] || this._oneSpeed;";

static const char* kGyzwRepl3 =
    "var _g3v=(r&&r[o]||this._oneSpeed);"
    "try{if(window.__starryGyzwOn(3)){var _g3m=window.__starryGyzwVal(3);"
    "if(_g3m>1&&_g3v&&typeof _g3v==='object'){"
    "return{trueSpeed:Math.max(_g3v.trueSpeed*_g3m,_g3m),"
    "displaySpeed:_g3v.displaySpeed,openId:_g3v.openId};}}}catch(_g3e){}\n"
    "return _g3v;";

static const char* kGyzwAnchor4 =
    "for (var e = Math.min(3, Math.floor(this._logicAccumulator / this._logicFrameRate)), t = 0; t < e; t++) {";

static const char* kGyzwRepl4 =
    "for (var _g4s=(window.__starryGyzwOn(3)?Math.max(window.__starryGyzwVal(3),3):3), "
    "e = Math.min(_g4s, Math.floor(this._logicAccumulator / this._logicFrameRate)), t = 0; t < e; t++) {";
