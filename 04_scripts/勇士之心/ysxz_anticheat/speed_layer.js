/* ============================================================================
 * 星光「战斗加速层」(speed layer) —— 2026-11-23 从 ac_layer.js 第 7 节原样抽出
 *
 * 为什么单独成文件：战斗加速（菜单第 6 项 Slider_战斗加速:1:5:1）的实现体原本长在
 * 「星空过检测层」ac_layer.js 里（旧 L421-556）。移除过检测层时这一项是功能层必须
 * 保留的，所以把加速实现迁到本文件；本层【不含】任何检测/闸门/出口挂钩/伤害还原/
 * 结算补偿代码，也不写任何上报字段。
 *
 * 落点三条路线，互斥使用（同一个 k 被算两次 = k²，战斗会打成幻灯片）：
 *   主路线  挂引擎 calculateDeltaTime，把每帧 _deltaTime ×k  ⇒ 真·全局变速
 *   兜底①  战斗内 battle world 的 worldTimeScale（明文名做结构发现）
 *   兜底②  战斗外 cc.director.getScheduler().setTimeScale(k)
 *
 * 铁律：不写死 ctrl/world 的混淆短名（hook-rules §24），world 用明文名 worldTimeScale
 * 做 BFS 结构发现，命中路径记进 SPEED.speedPath 便于复现；找不到就不猜、不硬取值。
 *
 * 对外唯一接口：window.__starry_speed
 *   { applySpeed, installDeltaScale, startSpeedLoop,
 *     tickSeen, dtScaled, speedMul, dtHits, speedPath, schedClock }
 *   tickSeen 由 calculateDeltaTime 包装器每帧自增（替代原 AC 层的墙钟 tickSeen，
 *   自动化层的 battleSignal 次通道读它判断"是否在战斗中"）。
 * ========================================================================== */
;(function () {
  /* 版本仲裁：真机加载顺序 = 引擎先跑 files/res/main.js（内嵌旧副本），原生 evalString
   * splice 的新副本随后才到。只有「同版或更新版」才让位，低版本一律接管覆盖。 */
  var SPEED_VER = 1;
  if (typeof window === "undefined") return;
  var _prev = window.__starry_speed;
  if (_prev && (_prev.__v | 0) >= SPEED_VER) return;
  var TAG = "[SPEED] ";
  function log(s) {
    try { if (window.__starry_nlog) window.__starry_nlog(TAG + s); } catch (e) {}
    try { if (window.MOD_LOG) window.MOD_LOG.push(TAG + s); } catch (e) {}
    try { if (window.console && console.log) console.log(TAG + s); } catch (e) {}
  }

  var SPEED = {
    __v: SPEED_VER,
    /* 战斗加速（全局时间缩放）字段账本 */
    speedMul: 1, speedHits: 0, speedPath: "", schedClock: 1, schedHits: 0,
    dtScaled: false, dtHits: 0, speedHolder: null, speedLoopOn: false,
    /* 帧计数：由 calculateDeltaTime 包装器自增 */
    tickSeen: 0
  };
  window.__starry_speed = SPEED;

  /* 战斗加速的施加链：**载入即启动**，不依赖任何 capture 入口（实测：主包 capture
   * 迟迟不来时之前只有 tick 一条路，而 tick 只在战斗里跑 ⇒ 菜单开到 3 也毫无反应）。
   * 幂等：只允许一条自重排链。 */
  SPEED.startSpeedLoop = function () {
    if (SPEED.speedLoopOn) return;
    SPEED.speedLoopOn = true;
    log("battle-speed: apply loop started (load-time)");
    var tick = function () {
      try { SPEED.applySpeed(); } catch (e) {}
      try { setTimeout(tick, 1000); } catch (e) {}
    };
    try { setTimeout(tick, 1500); } catch (e) {}
  };
  try { SPEED.startSpeedLoop(); } catch (e) {}

  /* 有界广度扫描：从 $eZ.battleMgr 起按 BFS 找「自己身上带数值型 worldTimeScale」的对象。
   * 深度上限 3（battleLogic 实测形态：world 挂在 ctrl 下、或再套一层），
   * 命中就缓存（SPEED.speedHolder），每轮只做一次 key 扫描 + 一次属性比对，代价可忽略。
   * 找不到就返回 null —— 调用方**不猜也不写**，绝不拿短名硬取值。 */
  function findSpeedHolder() {
    var g = null;
    try { g = window.$eZ; } catch (e) {}
    if (!g) return null;
    var root = g.battleMgr || g;
    var seen = [];
    var queue = [{ o: root, p: "battleMgr" }];
    var depth = 0;
    while (queue.length && depth <= 3) {
      var next = [];
      for (var i = 0; i < queue.length; i++) {
        var cur = queue[i];
        var o = cur.o;
        if (!o || typeof o !== "object") continue;
        var dup = false;
        for (var s = 0; s < seen.length; s++) { if (seen[s] === o) { dup = true; break; } }
        if (dup) continue;
        seen.push(o);
        try {
          if (typeof o.worldTimeScale === "number") { SPEED.speedPath = cur.p; return o; }
        } catch (e0) {}
        var keys = [];
        try { keys = Object.keys(o); } catch (e1) { keys = []; }
        for (var j = 0; j < keys.length; j++) {
          var v = null;
          try { v = o[keys[j]]; } catch (e2) { continue; }
          if (v && typeof v === "object" && typeof v !== "function") {
            next.push({ o: v, p: cur.p + "." + keys[j] });
          }
        }
      }
      queue = next;
      depth++;
    }
    return null;
  }

  /* ---- 真·全局变速：挂引擎的 calculateDeltaTime，把每帧 _deltaTime ×k ----
   * 引擎 mainLoop（cocos2d-jsb 1.1.8.1 src/cocos2d-jsb.js@6406）：
   *   this.calculateDeltaTime(t); var dt = this._deltaTime;
   *   → compScheduler.updatePhase(dt) / _scheduler.update(dt) / ActionManager / 物理 / 组件 update
   * 一处放大即全局生效：战斗、过场、挂机、动画、UI 动效全部 k 倍。
   * 不动 _lastUpdate（仍按真实墙钟推进），所以每帧只放大「本帧间隔」，不累积漂移。
   * 兜底顺序：dt 放大装不上 → 战斗内 worldTimeScale；再不行 → 调度器 setTimeScale。
   * 三条路线**互斥**，否则同一个 k 会被平方。
   * tickSeen 也在这里自增：它是"这一帧引擎真的跑了"的证据，自动化层拿它做战斗信号。 */
  function installDeltaScale() {
    try {
      if (SPEED.dtScaled) return true;
      var d = (window.cc && cc.director) ? cc.director : null;
      if (!d || typeof d.calculateDeltaTime !== "function") return false;
      if (d.__starryDtWrapped) { SPEED.dtScaled = true; return true; }
      var orig = d.calculateDeltaTime;
      d.calculateDeltaTime = function (t) {
        var r = orig.apply(this, arguments);
        SPEED.tickSeen = (SPEED.tickSeen | 0) + 1;
        try {
          var k = 1;
          try { k = (window.MOD && window.MOD.bspeed) ? (window.MOD.bspeed | 0) : 1; } catch (e0) { k = 1; }
          if (k > 1) {
            var dt = this._deltaTime;
            if (typeof dt === "number" && dt > 0) {
              this._deltaTime = dt * k;
              SPEED.dtHits = (SPEED.dtHits | 0) + 1;
            }
          }
        } catch (e1) {}
        return r;
      };
      d.__starryDtWrapped = true;
      SPEED.dtScaled = true;
      log("battle-speed: engine dt scaling armed (calculateDeltaTime x k)");
      return true;
    } catch (e) { return false; }
  }
  SPEED.installDeltaScale = installDeltaScale;

  SPEED.applySpeed = function () {
    var k = 1;
    try { k = (window.MOD && window.MOD.bspeed) ? (window.MOD.bspeed | 0) : 1; } catch (e) { k = 1; }
    if (!(k > 1)) k = 1;
    if (SPEED.speedMul !== k) {
      SPEED.speedMul = k;
      log("battle-speed = " + k + "x");
    }
    /* 主路线：引擎 dt 放大（真·全局变速）。装上后调度器/worldTimeScale 一律保持 1，防叠乘。 */
    try { installDeltaScale(); } catch (e) {}
    var holder = null;
    try {
      /* 缓存优先：战斗换局时 world 会换对象，取不到数值就重扫一次 */
      if (SPEED.speedHolder && typeof SPEED.speedHolder.worldTimeScale === "number") holder = SPEED.speedHolder;
      else { holder = findSpeedHolder(); if (holder) SPEED.speedHolder = holder; }
    } catch (e) { holder = null; }
    if (holder) {
      /* 战斗内兜底：只有在「引擎 dt 放大」没装上的时候才用 worldTimeScale，
       * 否则 dt 已经 ×k，再乘一次就是 k²（2026-10-24 用户实测"没效果"的教训：
       * 光改 worldTimeScale 不影响 UI/过场/挂机，且 battle world 常常找不到）。 */
      try {
        if (!SPEED.dtScaled && holder.worldTimeScale !== k) {
          if (typeof holder.setWorldTimeScale === "function") holder.setWorldTimeScale(k);
          else holder.worldTimeScale = k;
          SPEED.speedHits = (SPEED.speedHits | 0) + 1;
          if (SPEED.speedHits <= 3) log("battle-speed world=" + k + "x @ " + (SPEED.speedPath || "battleMgr"));
        }
      } catch (e) {}
      return;
    }
    /* 战斗外兜底：调度器（dt 放大可用时保持 1，避免叠乘） */
    try {
      if (window.cc && cc.director && typeof cc.director.getScheduler === "function") {
        var sch = cc.director.getScheduler();
        var want = SPEED.dtScaled ? 1 : k;
        if (sch && typeof sch.setTimeScale === "function" && SPEED.schedClock !== want) {
          sch.setTimeScale(want);
          SPEED.schedClock = want;
          if (!SPEED.dtScaled) {
            SPEED.schedHits = (SPEED.schedHits | 0) + 1;
            if (SPEED.schedHits <= 3) log("battle-speed scheduler=" + k + "x (fallback, out of battle)");
          }
        }
      }
    } catch (e) {}
  };

  log("speed layer v1 loaded (global dt scaling; no anti-cheat / no damage code)");
})();
