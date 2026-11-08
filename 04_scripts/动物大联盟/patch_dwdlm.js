/*starry_dwdlm_v1*/
;(function () {
  var ACT = "com.starry.magic.cocos.DwdlmHooks";
  var DEF = { 1: 10, 2: 1, 3: 1, 4: 1, 5: 3, 6: 1 };
  var S = { atk: 10, speed: 3, god: 1, skill: 1, auto: 1, ad: 1, wasAuto: 0, reqs: [], err: 0, mods: {} };
  function RET(n) { try { return !!jsb.reflection.callStaticMethod(ACT, "getRet", "(I)Z", n); } catch (e) { return !!DEF[n]; } }
  function VAL(n) { try { return jsb.reflection.callStaticMethod(ACT, "getVal", "(I)I", n) || 0; } catch (e) { return DEF[n] || 0; } }
  function note() { var r = window.__require; if (r && S.reqs.indexOf(r) < 0) S.reqs.push(r); }
  note();
  if (window.__starry_dwdlm) { window.__starry_dwdlm.note(); return; }
  window.__starry_dwdlm = { note: note };
  function findReq(ids, check) {
    for (var i = 0; i < S.reqs.length; i++) for (var j = 0; j < ids.length; j++) {
      try { var m = S.reqs[i](ids[j]); if (m && (!check || check(m))) return { req: S.reqs[i], mod: m }; } catch (e) {}
    }
    return null;
  }
  function scriptReq() {
    if (!S.mods.sreq) S.mods.sreq = findReq(["cv", "../core/cv", "../../../scripts/core/cv"], function (m) { return m && m.default && m.default._VY; });
    return S.mods.sreq;
  }
  function battleReq() {
    if (!S.mods.breq) S.mods.breq = findReq(["_2X", "_4"], function (m) { return (m && m._gFv) || (m && m.default && m.default._VH); });
    return S.mods.breq;
  }
  function cv() { var x = scriptReq(); return x ? x.mod.default : null; }
  function ctrl() { var x = cv(); return x && x._VY ? x._VY : null; }
  function localId() { var b = ctrl(); return b && b._VF ? b._VF.localPlayerId : -1; }
  function localEntity() { var b = ctrl(), id = localId(); return (b && b._VY && b._VY._Vh && id >= 0) ? b._VY._Vh(id) : null; }
  function isLocalEntity(e) { var id = localId(); return id >= 0 && e && e._VQF && e._VQF._VQe === id; }
  function logErr(where, e) { if (S.err >= 4) return; S.err += 1; try { cc.log("[dwdlm] " + where + " error: " + e); } catch (_) {} }
  function refresh() {
    S.atk = VAL(1); if (S.atk < 1) S.atk = 1; if (S.atk > 100) S.atk = 100;
    S.speed = VAL(5); if (S.speed < 1) S.speed = 1; if (S.speed > 5) S.speed = 5;
    S.god = RET(2) ? 1 : 0; S.skill = RET(3) ? 1 : 0; S.auto = RET(4) ? 1 : 0; S.ad = RET(6) ? 1 : 0;
  }
  function installModePermission() {
    var x = scriptReq(); if (!x) return;
    try { var m = x.req("_22"); if (m && m._gPA) for (var k in m._gPA) if (m._gPA[k]) m._gPA[k].canAutoBattle = true; } catch (e) { logErr("mode", e); }
  }
  function installDamage() {
    var x = battleReq(); if (!x || !x.mod._gFv || x.mod._gFv.__starry_dwdlm) return;
    var P = x.mod._gFv.prototype, orig = P && P._VQv; if (typeof orig !== "function") return;
    P._VQv = function (events) {
      var touched = [];
      try {
        if (S.atk > 1 && events && events.length) {
          var id = -1; try { id = this._VQi._Ve._Vl; } catch (e) {}
          for (var i = 0; i < events.length; i++) {
            var a = events[i] && events[i]._VQd;
            if (!a || a._VP !== 5 || a._VQ === undefined || a._VQ === null) continue;
            var attacker = null; try { attacker = this._Vi(a._VQ); } catch (e) {}
            if (!attacker || !attacker._VQF || attacker._VQF._VQe !== id) continue;
            var old = a._Vq || 0; touched.push([a, old]); a._Vq = old ? old * S.atk : S.atk;
          }
        }
      } catch (e) { logErr("damage-pre", e); }
      var rv = orig.apply(this, arguments);
      for (var j = 0; j < touched.length; j++) touched[j][0]._Vq = touched[j][1];
      if (S.god) { try { var id2 = this._VQi._Ve._Vl, ent = this._Vi(id2), at = ent && ent._VQQ && ent._VQQ._Ve; if (at && at[0] < at[1]) at[0] = at[1]; } catch (e) { logErr("damage-post", e); } }
      return rv;
    };
    P._VQv.__starry_dwdlm = 1; x.mod._gFv.__starry_dwdlm = 1;
  }
  function installSkillsAndAuto() {
    var x = battleReq(); if (!x || !x.mod.default) return;
    var H = x.mod.default, origSkill = H._VH, origConsume = H._Vg;
    if (!H.__starry_dwdlm) {
      H._VH = function (e, s) { if (S.skill && isLocalEntity(e)) { if (s) { s._Ve = 0; s._VP = 0; if (s._VQ > 0) s._VF = s._VQ; } return true; } return typeof origSkill === "function" ? origSkill.apply(this, arguments) : false; };
      H._Vg = function (e, s, opt) { if (S.skill && isLocalEntity(e)) { if (s) { s._Ve = 0; s._VP = 0; if (s._VQ > 0) s._VF = s._VQ; } return true; } return typeof origConsume === "function" ? origConsume.apply(this, arguments) : true; };
      H.__starry_dwdlm = 1;
    }
    var b = ctrl(), view = b && b._VY, data = b && b._VF;
    if (!view || !data || typeof H._ViZ !== "function") return;
    var lp = null; try { lp = data.getLocalPlayer && data.getLocalPlayer(); } catch (e) {}
    if (!lp) return;
    if (S.auto) { if (!lp.scoringCompetitionAutoSkill) H._ViZ(view, lp._Ve, true); S.wasAuto = 1; }
    else if (S.wasAuto) { H._ViZ(view, lp._Ve, false); S.wasAuto = 0; }
  }
  function installMoveSpeed() {
    var x = scriptReq(); if (!x || S.mods.speedClass) return;
    try {
      var m = x.req("_O"), C = m && m._gPm, P = C && C.prototype, orig = P && P.updateSelfRoleMoveSpeed;
      if (typeof orig !== "function") return;
      P.updateSelfRoleMoveSpeed = function () { var rv = orig.apply(this, arguments); this.__starry_base_speed = this.moveSpeed; if (S.speed > 1) this.moveSpeed = Math.ceil(this.moveSpeed * S.speed); return rv; };
      P.updateSelfRoleMoveSpeed.__starry_dwdlm = 1; S.mods.speedClass = 1;
    } catch (e) { logErr("speed-install", e); }
  }
  function applyMoveSpeed() { var e = localEntity(); if (e && typeof e.updateSelfRoleMoveSpeed === "function") { try { e.updateSelfRoleMoveSpeed(); } catch (err) { logErr("speed-apply", err); } } }
  function applyGodData() {
    var e = localEntity(); if (!e || !e._VQQ || !e._VQQ._Ve) return;
    var a = e._VQQ._Ve;
    if (S.god) { if (!e.__starry_dwdlm_god) e.__starry_dwdlm_god = { hp: a[0], max: a[1] }; var max = Math.max(e.__starry_dwdlm_god.max || 1, 1e9); a[1] = max; a[0] = max; }
    else if (e.__starry_dwdlm_god) { a[0] = e.__starry_dwdlm_god.hp; a[1] = e.__starry_dwdlm_god.max; delete e.__starry_dwdlm_god; }
  }
  function installAds() {
    var a = window.rewardVideoAd; if (!a || a.__starry_dwdlm) return;
    var oc = a._VQe, ol = a._VQi, os = a._VQQ, osc = a._VQ;
    a._VQe = function () { return S.ad ? a : (typeof oc === "function" ? oc.apply(a, arguments) : a); };
    a._VQi = function (cb) { if (S.ad) { if (cb) setTimeout(function () { cb({ code: 0, msg: "", data: null }); }, 0); return; } return typeof ol === "function" ? ol.apply(a, arguments) : undefined; };
    a._VQQ = function (cb) { if (S.ad) { if (cb) setTimeout(function () { cb({ code: 0, msg: "", data: null }); }, 0); var close = a._VQP; if (close) setTimeout(function () { close({ code: 0, msg: "", data: null }); }, 0); return; } return typeof os === "function" ? os.apply(a, arguments) : undefined; };
    a._VQ = function (cb) { a._VQP = cb; return S.ad ? undefined : (typeof osc === "function" ? osc.apply(a, arguments) : undefined); };
    a.__starry_dwdlm = 1;
  }
  function tick() { try { note(); refresh(); installModePermission(); installDamage(); installSkillsAndAuto(); installMoveSpeed(); applyMoveSpeed(); applyGodData(); installAds(); } catch (e) { logErr("tick", e); } }
  tick();
  var scheduled = 0;
  try { cc.director.getScheduler().schedule(tick, cc.director, 0.2, cc.macro.REPEAT_FOREVER, 0, false); scheduled = 1; } catch (e) {}
  if (!scheduled) { try { setInterval(tick, 200); } catch (e) {} }
  try { cc.log("[dwdlm] patch applied"); } catch (e) {}
})();
