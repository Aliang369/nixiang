/*starry_rsjy_v1*/
;(function () {
  var G = (typeof window !== "undefined") ? window :
          ((typeof globalThis !== "undefined") ? globalThis : this);
  if (!G) return;
  if (G.__starry_rsjy && G.__starry_rsjy.v) {
    try { if (typeof G.__starry_rsjy.tick === "function") G.__starry_rsjy.tick(); } catch (e) {}
    return;
  }
  var S = G.__starry_rsjy || (G.__starry_rsjy = {});
  S.v = 1;
  S.kill = 1;
  S.god = 0;
  S.speed = 1;
  S.ad = 0;

  var ACT = "com.starry.magic.cocos.RsjyHooks";

  function ret(n) {
    try {
      return !!jsb.reflection.callStaticMethod(ACT, "getRet", "(I)Z", n);
    } catch (e) { return false; }
  }

  function val(n) {
    try {
      return jsb.reflection.callStaticMethod(ACT, "getVal", "(I)I", n) | 0;
    } catch (e) { return 0; }
  }

  function req(name) {
    try {
      return (typeof G.__require === "function") ? G.__require(name) : null;
    } catch (e) { return null; }
  }

  function unwrap(m) {
    return (m && m.default) ? m.default : m;
  }

  function cls(name) {
    return unwrap(req(name));
  }

  function marked(fn) { return !!(fn && fn.__starry_rsjy); }
  function mark(fn) { try { fn.__starry_rsjy = 1; } catch (e) {} }

  function refresh() {
    S.kill = val(1);
    if (S.kill < 1) S.kill = 1;
    if (S.kill > 100) S.kill = 100;
    S.god = ret(2) ? 1 : 0;
    S.speed = val(3);
    if (S.speed < 1) S.speed = 1;
    if (S.speed > 10) S.speed = 10;
    S.ad = ret(4) ? 1 : 0;
  }

  function applyUnit() {
    try {
      var U = cls("Unit");
      var P = U && U.prototype;
      if (!P || typeof P.changeHp !== "function" || marked(P.changeHp)) return;
      var orig = P.changeHp;
      P.changeHp = function (info, src, skillType, dmgType) {
        try {
          if (info && typeof info.delta === "number" && info.delta < 0) {
            if (S.god && this.group === 0) {
              info.delta = 0;
            } else if (S.kill > 1 && src && src.group === 0 && this.group !== 0) {
              info.delta = info.delta * S.kill;
            }
          }
        } catch (e) {}
        return orig.apply(this, arguments);
      };
      mark(P.changeHp);
    } catch (e) {}
  }

  function applySpeed() {
    var names = [
      "ChapterFbBattlePanel",
      "OnHookBossBattlePanel",
      "OnHookSettingModel",
      "OutlandBattlePanel",
      "PveBattlePanel"
    ];
    for (var i = 0; i < names.length; i++) {
      try {
        var C = cls(names[i]);
        var P = C && C.prototype;
        if (!P || typeof P.getBattleSpeed !== "function" || marked(P.getBattleSpeed)) continue;
        var orig = P.getBattleSpeed;
        P.getBattleSpeed = function () {
          try { if (S.speed > 1) return S.speed; } catch (e) {}
          return orig.apply(this, arguments);
        };
        mark(P.getBattleSpeed);
      } catch (e) {}
    }
  }

  function applyAd() {
    try {
      var M = req("AdHelper");
      var A = (M && (M.AdHelper || (M.default && M.default.AdHelper))) || cls("AdHelper");
      var P = A && A.prototype;
      if (!P || typeof P.showVideoAd !== "function" || marked(P.showVideoAd)) return;
      var orig = P.showVideoAd;
      P.showVideoAd = function (cb) {
        if (!S.ad) return orig.apply(this, arguments);
        var fn = cb;
        try {
          setTimeout(function () {
            try { if (typeof fn === "function") fn(2); } catch (e) {}
          }, 0);
        } catch (e) {
          try { if (typeof fn === "function") fn(2); } catch (_) {}
        }
      };
      mark(P.showVideoAd);
    } catch (e) {}
  }

  function tick() {
    try {
      refresh();
      applyUnit();
      applySpeed();
      applyAd();
    } catch (e) {}
  }

  S.tick = tick;
  tick();

  if (!S.scheduled) {
    S.scheduled = 1;
    var scheduled = 0;
    try {
      var sch = cc.director.getScheduler();
      sch.schedule(tick, cc.director, 0.5, cc.macro.REPEAT_FOREVER, 0, false);
      scheduled = 1;
    } catch (e) {}
    if (!scheduled) {
      try { setInterval(tick, 500); } catch (e) {}
    }
  }
})();
