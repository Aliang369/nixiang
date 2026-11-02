/* 自动生成，勿手改：源 = Games/XxwcPatch.js */
#ifndef STARRY_XXWC_PATCH_JS_H
#define STARRY_XXWC_PATCH_JS_H

static const char kXxwcInstallJs[] = R"XXWCJS(
(function () {
  var root = (typeof globalThis !== "undefined") ? globalThis : ((typeof window !== "undefined") ? window : this);
  var VERSION = 1;
  var S = root.__starryXxwc;
  if (!S || S.version !== VERSION) {
    S = root.__starryXxwc = { version: VERSION, noAd: false, inv: false, installed: 0, warned: {} };
  }
  if (typeof S.setFlags !== "function") {
    S.setFlags = function (noAd, inv) { S.noAd = !!noAd; S.inv = !!inv; };
  }
  function log() {
    try { if (typeof console !== "undefined" && console.log) { var a = [].slice.call(arguments); a.unshift("[xxwc]"); console.log.apply(console, a); } } catch (e) {}
  }
  function mod(id) {
    try {
      if (typeof System === "undefined" || !System.get) return null;
      var paths = [id, "chunks:///_virtual/" + id + ".ts", "chunks:///_virtual/" + id + ".js"];
      for (var i = 0; i < paths.length; i++) { var m = System.get(paths[i]); if (m) return m; }
    } catch (e) {}
    return null;
  }
  var AD_MOD = "zghbhh45g4jh5d4fh5dfh45h1f";
  var AD_FN = "fgdfhgdfh45dfh45df4hr8hht";
  function patchAd() {
    var m = mod(AD_MOD); if (!m) return 0;
    var C = m[AD_MOD] || m["default"]; if (!C) return 0;
    var f = C[AD_FN]; if (typeof f !== "function") return 0;
    if (f.__xxwcNoAd) return 1;
    var w = function () {
      var cfg = root.g_config;
      if (S.noAd && cfg) {
        var old = cfg.checkPlayVideo;
        cfg.checkPlayVideo = false;
        try { return f.apply(this, arguments); } finally { cfg.checkPlayVideo = old; }
      }
      return f.apply(this, arguments);
    };
    w.__xxwcNoAd = 1;
    C[AD_FN] = w;
    return 2;
  }
  var WAR_MOD = "WWarQingjiao";
  var WAR_FNS = ["dealqjTanziResult", "dealShenhaiResult"];
  function patchWar() {
    var m = mod(WAR_MOD); if (!m) return 0;
    var C = m[WAR_MOD]; if (!C || !C.prototype) return 0;
    var n = 0;
    for (var i = 0; i < WAR_FNS.length; i++) {
      (function (name) {
        var proto = C.prototype, f = proto[name];
        if (typeof f !== "function" || f.__xxwcInv) return;
        var w = function () {
          if (S.inv) {
            var a = [].slice.call(arguments);
            if (typeof a[0] === "number") a[0] = 100;
            return f.apply(this, a);
          }
          return f.apply(this, arguments);
        };
        w.__xxwcInv = 1;
        proto[name] = w;
        n++;
      })(WAR_FNS[i]);
    }
    return n ? 2 : 1;
  }
  function install() {
    var a = 0, b = 0;
    try { a = patchAd(); } catch (e) { log("ad patch fail", e && e.message); }
    try { b = patchWar(); } catch (e) { log("war patch fail", e && e.message); }
    S.installed = (a && b) ? 1 : 0;
    return S.installed;
  }
  S.install = install;
  install();
  if (!S.timer && typeof setInterval === "function") {
    S.timer = setInterval(function () { try { install(); } catch (e) {} }, 3000);
  }
})();
)XXWCJS";

#endif  // STARRY_XXWC_PATCH_JS_H
