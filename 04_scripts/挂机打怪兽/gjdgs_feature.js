/* ================================================================
 * 星空版功能层 — 挂机打怪兽（org.cocos2d.guajidaguaishou.tap 1.1.65）
 * 注入：追加到 APK 的 assets/main.js（明文），入口 window.__sst(key, value)
 * 调用方：Java GjdgsHooks（evalString）
 *
 * 依据：index.17a32.jsc 解密源码（key 74f38ae4-b95f-42）
 *   存档 cc.sys.localStorage['data'] → gameStorage.juese（127 字段）
 *   免广告：t.bofang() 里 if (gameStorage.juese.jrggcs >= 28) 直接发奖，否则 native("playVideo")
 * ================================================================ */
(function () {
  'use strict';
  if (window.__sst) return;

  function log(m) { try { console.log('[SST] ' + m); } catch (e) {}
  }
  function CC() { return window.cc; }

  /* ---------------- 存档对象 ---------------- */
  var RT = null;

  (function () {
    try {
      var op = JSON.parse;
      JSON.parse = function (t, r) {
        var o = op.call(JSON, t, r);
        try {
          if (!RT && o && o.juese && typeof o.juese.gold === 'number') {
            RT = o; window.__sstRT = o; log('capture(parse) gold=' + o.juese.gold);
          }
        } catch (e) {}
        return o;
      };
    } catch (e) { log('hook parse err ' + e); }
  })();

  (function () {
    try {
      var os = JSON.stringify;
      JSON.stringify = function (o) {
        try {
          if (o && o.juese && typeof o.juese.gold === 'number' && RT !== o) {
            RT = o; window.__sstRT = o; log('capture(stringify) gold=' + o.juese.gold);
          }
        } catch (e) {}
        return os.apply(JSON, arguments);
      };
    } catch (e) { log('hook stringify err ' + e); }
  })();

  function LS() { try { return CC().sys.localStorage; } catch (e) { return null; } }

  function loadSave() {
    var ls = LS(); if (!ls) return null;
    try { var raw = ls.getItem('data'); return raw ? JSON.parse(raw) : null; } catch (e) { return null; }
  }
  function saveSave(d) {
    var ls = LS(); if (!ls) return false;
    try { ls.setItem('data', JSON.stringify(d)); return true; } catch (e) { log('save err ' + e); return false; }
  }
  function rtSave() {
    if (RT && RT.juese) return RT;
    var d = loadSave();
    if (d && d.juese) { RT = d; window.__sstRT = d; }
    return RT;
  }
  function persist() {
    try { var ls = LS(); if (ls && RT) ls.setItem('data', JSON.stringify(RT)); }
    catch (e) { log('persist err ' + e); }
  }

  /* 直接改盘上存档的某个字段（内存对象不可靠时用） */
  function patchDisk(field, value) {
    var d = loadSave();
    if (!d || !d.juese) return false;
    d.juese[field] = value;
    d.juese.saveTime = Date.now();
    return saveSave(d);
  }

  /* 拿到游戏当前持有的 gameStorage（可能是重新 load 过的新实例） */
  function getGS() {
    try {
      var names = ['gamestorage', 'GameStorage'];
      for (var i = 0; i < names.length; i++) {
        try {
          var m = window.__require && window.__require(names[i]);
          if (m && m.gameStorage && m.gameStorage.juese) return m.gameStorage;
          if (m && m.default && m.default.gameStorage && m.default.gameStorage.juese) return m.default.gameStorage;
        } catch (e) {}
      }
    } catch (e) {}
    try {
      var sc = CC().director.getScene();
      var stack = [sc], n = 0;
      while (stack.length && n < 900) {
        var nd = stack.pop(); n++;
        var cs = nd._components || [];
        for (var k = 0; k < cs.length; k++) {
          var c = cs[k]; if (!c) continue;
          if (c.gameStorage && c.gameStorage.juese) return c.gameStorage;
          if (c._gameStorage && c._gameStorage.juese) return c._gameStorage;
        }
        var ch = nd._children || [];
        for (var q = 0; q < ch.length; q++) stack.push(ch[q]);
      }
    } catch (e) {}
    return rtSave();
  }

  /* ---------------- 战斗加速 ---------------- */
  var SPEED = 1;
  function setSpeed(n) {
    n = Math.max(1, Math.min(10, n | 0));
    SPEED = n;
    try {
      var dir = CC().director, done = false, how = '';
      try {
        var sch = dir.getScheduler ? dir.getScheduler() : dir._scheduler;
        if (sch) {
          if (typeof sch.setTimeScale === 'function') sch.setTimeScale(n);
          sch._timeScale = n; sch.timeScale = n; done = true; how = 'scheduler';
        }
      } catch (e1) { log('sched err ' + e1); }
      if (!done) {
        try { if (dir.setAnimationInterval) { dir.setAnimationInterval(1 / (60 * n)); done = true; how = 'animInterval'; } } catch (e2) {}
      }
      if (!done) {
        var ot = dir.tick;
        if (!window.__sstTick && typeof ot === 'function') {
          window.__sstTick = 1;
          dir.tick = function (dt) { return ot.call(this, dt * (window.__sstSpeed || 1)); };
        }
        done = true; how = 'tick';
      }
      window.__sstSpeed = n;
      log('speed=' + n + ' via ' + how);
      return true;
    } catch (e) { log('speed err ' + e); return false; }
  }

  /* ---------------- 无敌 ---------------- */
  var HERO_TIMER = 0, GOD_BASE = null;

  function installGodLock() {
    var d = rtSave();
    if (!d || !d.juese) return false;
    var j = d.juese;
    if (j.__sstGodLock) return true;
    try {
      var base = (typeof j.nowHp === 'number' && j.nowHp > 0) ? j.nowHp : 999999999;
      Object.defineProperty(j, 'nowHp', {
        configurable: true, enumerable: true,
        get: function () { return window.__sstGodOn ? 999999999 : base; },
        set: function (v) {
          if (window.__sstGodOn) base = 999999999;
          else base = (typeof v === 'number') ? v : base;
        }
      });
      Object.defineProperty(j, '__sstGodLock', { value: 1, enumerable: false, configurable: true });
      log('god lock on');
      return true;
    } catch (e) { log('god lock err ' + e); return false; }
  }

  function removeGodLock(hp) {
    var d = rtSave();
    if (!d || !d.juese) return;
    var j = d.juese;
    if (!j.__sstGodLock) return;
    try {
      delete j.nowHp;
      Object.defineProperty(j, 'nowHp', { value: hp || 999999, writable: true, enumerable: true, configurable: true });
      delete j.__sstGodLock;
    } catch (e) {}
  }

  function forceHeroBars() {
    try {
      var cc0 = window.cc, sc = cc0.director.getScene();
      if (!sc) return;
      var stack = [sc], n = 0;
      while (stack.length && n < 1500) {
        var node = stack.pop(); n++;
        var cs = node._components || [];
        for (var i = 0; i < cs.length; i++) {
          var c = cs[i]; if (!c) continue;
          if (c instanceof cc0.ProgressBar) {
            var nm = node.name || '';
            if (/hp|xue|tiao|blood/i.test(nm) && c.progress < 1) { try { c.progress = 1; } catch (e) {} }
          }
          var cn = '';
          try { cn = c.__classname__ || (c.constructor && c.constructor.name) || ''; } catch (e) {}
          if (/kszzplayer/i.test(cn)) { c.maxHp = 999999; c.def = 999999; c.addHp = 999999; }
        }
        var kids = node._children || [];
        for (var k = 0; k < kids.length; k++) stack.push(kids[k]);
      }
    } catch (e) {}
  }

  function startHeroWatch() {
    if (HERO_TIMER) return;
    HERO_TIMER = setInterval(function () {
      if (!window.__sstGodOn) return;
      forceHeroBars();
      var d = rtSave();
      if (d && d.juese && d.juese.nowHp < 100000000) { try { d.juese.nowHp = 999999999; } catch (e) {} }
    }, 120);
  }
  function stopHeroWatch() { if (HERO_TIMER) { clearInterval(HERO_TIMER); HERO_TIMER = 0; } }

  function applyGod(on) {
    var d = rtSave();
    if (!d || !d.juese) return false;
    var j = d.juese;
    if (on) {
      window.__sstGodOn = 1;
      if (GOD_BASE === null) GOD_BASE = j.hpLv;
      j.hpLv = 99999;
      installGodLock();
      j.nowHp = 999999999;
      startHeroWatch();
    } else {
      window.__sstGodOn = 0;
      stopHeroWatch();
      removeGodLock(999999);
      if (GOD_BASE !== null) { j.hpLv = GOD_BASE; GOD_BASE = null; }
    }
    persist();
    log('god=' + (on ? 1 : 0) + ' hpLv=' + j.hpLv);
    return true;
  }

  /* ---------------- 百倍伤害 ---------------- */
  var ATK_BASE = null;
  function applyDmg(mult) {
    var d = rtSave();
    if (!d || !d.juese) return false;
    var j = d.juese;
    if ((j.atkLv || 0) > 1e12) { j.atkLv = 99999; log('dmg: atkLv overflow reset'); }
    if (mult > 1) {
      if (ATK_BASE === null) ATK_BASE = (typeof j.atkLv === 'number' && j.atkLv > 0) ? j.atkLv : 1;
      j.atkLv = Math.round(ATK_BASE * mult);
    } else if (ATK_BASE !== null) {
      j.atkLv = ATK_BASE; ATK_BASE = null;
    }
    persist();
    log('dmg x' + mult + ' atkLv=' + j.atkLv);
    return true;
  }

  /* ---------------- 资源：只加不改 ---------------- */
  var ADD_FIELD = { gold: 'gold', gem: 'baoshi', shard: 'suip' };
  var ADD_VALUE = { gold: 10000000, gem: 10000000, shard: 10000000 };

  function addRes(name, on) {
    if (!on) return true;
    var f = ADD_FIELD[name]; if (!f) return false;
    var v = ADD_VALUE[name];
    var d = rtSave();
    if (d && d.juese) {
      d.juese[f] = (d.juese[f] || 0) + v;
      d.juese.saveTime = Date.now();
      persist();
      log('add ' + name + ' +' + v + ' -> ' + d.juese[f]);
    } else {
      var sd = loadSave();
      if (!sd || !sd.juese) { log('add ' + name + ' no save'); return false; }
      sd.juese[f] = (sd.juese[f] || 0) + v;
      sd.juese.saveTime = Date.now();
      saveSave(sd);
    }
    return true;
  }

  /* ---------------- 免广告：jrggcs >= 28 ---------------- */
  var NOAD_TIMER = 0;

  function bumpJrg() {
    var gs = getGS();
    if (!gs || !gs.juese) { log('noad: no gameStorage'); return false; }
    if ((gs.juese.jrggcs || 0) >= 28) return true;
    gs.juese.jrggcs = 28;
    patchDisk('jrggcs', 28);          // 双写：内存 + localStorage
    log('noad: jrggcs set 28 (mem+disk)');
    return true;
  }

  function setNoAd(on) {
    window.__sstNoAd = on ? 1 : 0;
    if (on) {
      bumpJrg();
      if (!NOAD_TIMER) {
        NOAD_TIMER = setInterval(function () {
          if (!window.__sstNoAd) return;
          bumpJrg();
        }, 2000);
      }
    } else if (NOAD_TIMER) {
      clearInterval(NOAD_TIMER); NOAD_TIMER = 0;
    }
    log('noad=' + (on ? 1 : 0));
    return true;
  }

  /* ---------------- 入口 ---------------- */
  window.__sst = function (k, v) {
    v = parseInt(v, 10);
    if (isNaN(v) || v < 0) return false;
    try {
      if (k === 'spd') return setSpeed(v);
      if (k === 'god') return applyGod(!!v);
      if (k === 'dmg') return applyDmg(v);
      if (k === 'noad') return setNoAd(!!v);
      if (k.indexOf('lock_') === 0) return addRes(k.slice(5), !!v);
      return false;
    } catch (e) { log('sst err ' + e); return false; }
  };

  log('js layer loaded');
})();
