/* js_only_flags.js — 动物餐厅"原签热更"两开关门（ES5，无 Java/JSB 反射依赖）
 *
 * 背景：12.42 现行 patched jsc 用 window.__starryNoAd/__starryNoIap 读
 *   jsb.reflection.callStaticMethod("com/starry/magic/cocos/DwctHooks", ...)
 * 官方原签包内没有 StarryMagic 的 Java 类 ⇒ 该读取恒抛异常 ⇒ try/catch 恒 false，
 * 两开关永远 OFF。本模块给"原签 + 只改热更 JS"路线提供**纯 JS** 的开关来源：
 *   <writablePath>/dwct_flags.json  {"noAd":true,"noIap":false}
 * 任一条不满足（无 jsb / 无文件 / 非 JSON / 字段非 true）⇒ 开关 false ⇒ 走游戏原流程。
 * 开关语义与 12.42 现行补丁一致：ON 才拦截，OFF 时插入代码是纯 no-op。
 *
 * 用法（由 patch_1242.js 在 DWCT_FLAG_SOURCE=file 时注入）：
 *   if (window.__starryGate && window.__starryGate.noAd()) { ... }
 */
(function (root) {
  "use strict";
  var FILE = "dwct_flags.json";
  var TTL_MS = 1000;
  var cache = { at: 0, noAd: false, noIap: false, ok: false };

  function readRaw() {
    try {
      if (typeof jsb === "undefined" || !jsb.fileUtils) return null;
      var p = jsb.fileUtils.getWritablePath() + FILE;
      if (typeof jsb.fileUtils.isFileExist === "function" && !jsb.fileUtils.isFileExist(p)) return null;
      return jsb.fileUtils.getStringFromFile(p);
    } catch (e) {
      return null;
    }
  }

  function parse(text) {
    var out = { noAd: false, noIap: false, ok: false };
    if (!text) return out;
    var v;
    try { v = JSON.parse(text); } catch (e) { return out; }
    if (!v || typeof v !== "object" || Object.prototype.toString.call(v) !== "[object Object]") return out;
    out.noAd = v.noAd === true;
    out.noIap = v.noIap === true;
    out.ok = true;
    return out;
  }

  function refresh(force) {
    var now = Date.now();
    if (!force && cache.at && (now - cache.at) < TTL_MS) return;
    var p = parse(readRaw());
    cache.at = now;
    cache.noAd = p.noAd;
    cache.noIap = p.noIap;
    cache.ok = p.ok;
  }

  root.__starryGate = {
    noAd: function () { refresh(false); return cache.noAd === true; },
    noIap: function () { refresh(false); return cache.noIap === true; },
    reload: function () { refresh(true); },
    status: function () { refresh(false); return { noAd: cache.noAd, noIap: cache.noIap, ok: cache.ok, file: FILE }; }
  };
  if (typeof module !== "undefined" && module.exports) module.exports = root.__starryGate;
})(typeof window !== "undefined" ? window : globalThis);
