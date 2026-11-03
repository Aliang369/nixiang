/* ================================================================
 * 星空版功能层 — 修仙大陆模拟器（com.sky.xxdl.dlt 1.0.5，Cocos Creator 3.6.3）
 * 注入：追加到 APK 的 assets/main.js（明文 815 B），入口 window.__sst(key, value, seq)
 * 调用方：Java com.starry.magic.cocos.XxdlHooks → CocosJsBridge.evalString（游戏线程）
 *
 * 抓点依据：10_docs/修仙大陆模拟器/抓点_20261104.md
 *   window.g_server = MServer（静态数据类）
 *     base.gold     资源   （addTableReward type 1；顶栏 k_goldtext）
 *     base.diamond  灵石   （type 2；k_diamondtext）
 *     base.gem      仙晶   （type 3；k_gemtext）
 *     assets.new    体力   （上限 assets.max，按钮消耗 -1）
 *   免广告：MBridge.playVideo(type, cb) → window.PlatformManager.a_na_playShiPing(id, cb)
 *           回调 cb(true) 即走成功分支（welfare.count++ 并发放奖励）
 * ================================================================ */
(function () {
  'use strict';
  if (window.__sst) return;

  function log(m) { try { console.log('[SST] ' + m); } catch (e) {} }

  // 资源类：只加不改（点一次加一次）
  var ADD = {
    res:      ['gold',     10000000],   // 资源
    lingshi:  ['diamond',  10000000],   // 灵石
    xianjing: ['gem',       1000000]    // 仙晶
  };

  // 同一 seq 只执行一次：native 侧补推不会重复加值；按钮每次点击都是新 seq
  var SEEN = {};

  function G() { try { return window.g_server || null; } catch (e) { return null; } }

  function persist() {
    var g = G(); if (!g) return;
    try { if (typeof g.saveData0 === 'function') g.saveData0(); } catch (e) {}
    try { if (typeof g.saveData1 === 'function') g.saveData1(); } catch (e) {}
  }

  function addRes(key) {
    var m = ADD[key]; if (!m) return false;
    var g = G();
    if (!g || !g.base) { log('no g_server.base'); return false; }
    var f = m[0];
    var before = Number(g.base[f]) || 0;
    g.base[f] = before + m[1];
    persist();
    log(key + ' ' + before + ' -> ' + g.base[f]);
    return true;
  }

  function fillTili() {
    var g = G();
    if (!g || !g.assets) { log('no g_server.assets'); return false; }
    var cap = Number(g.assets.max) || 10;
    g.assets.new = cap;
    persist();
    log('tili ' + g.assets.new + '/' + cap);
    return true;
  }

  /* ---------------- 免广告：激励视频直通成功回调 ---------------- */
  var NOAD = { on: 0, timer: 0, wrapper: null };

  // 每次补绑都重新确认函数身份：PlatformManager 若被重建成新对象，旧包装会丢
  function bindNoAd() {
    var pm = null;
    try { pm = window.PlatformManager || null; } catch (e) { pm = null; }
    if (!pm) return false;
    if (!NOAD.wrapper) {
      NOAD.wrapper = function (id, cb) {
        log('noad: skip ad ' + id);
        try { if (typeof cb === 'function') cb(true); } catch (e) {}
      };
    }
    var changed = false;
    try {
      if (typeof pm.a_na_playShiPing === 'function' && pm.a_na_playShiPing !== NOAD.wrapper) {
        pm.a_na_playShiPing = NOAD.wrapper; changed = true;
      }
      if (typeof pm.i_na_playSP === 'function' && pm.i_na_playSP !== NOAD.wrapper) {
        pm.i_na_playSP = NOAD.wrapper; changed = true;
      }
    } catch (e) { log('noad bind err ' + e); return false; }
    if (changed) log('noad bound');
    return true;
  }

  function setNoAd(on) {
    NOAD.on = on ? 1 : 0;
    if (NOAD.on) {
      bindNoAd();
      if (!NOAD.timer) {
        // PlatformManager 可能晚于本层加载：每 2s 补绑一次
        NOAD.timer = setInterval(function () {
          if (!NOAD.on) return;
          bindNoAd();
        }, 2000);
      }
    } else if (NOAD.timer) {
      clearInterval(NOAD.timer); NOAD.timer = 0;
    }
    log('noad=' + NOAD.on + ' bound=' + (NOAD.wrapper ? 1 : 0));
    return true;
  }

  window.__sst = function (k, v, seq) {
    try {
      seq = seq | 0;
      if (seq > 0) {
        if (SEEN[k] && SEEN[k] >= seq) return false;   // 补推去重
        SEEN[k] = seq;
      }
      if (k === 'res' || k === 'lingshi' || k === 'xianjing') return addRes(k);
      if (k === 'tili') return fillTili();
      if (k === 'noad') return setNoAd(!!v);
      return false;
    } catch (e) { log('sst err ' + e); return false; }
  };

  log('js layer loaded');
})();
