/*starry_zzbx2_v1*/
;(function () {
  if (window.__starry_zzbx2) { return; }
  window.__starry_zzbx2 = 1;
  var ACT = "com.starry.magic.cocos.Zzbx2Hooks";
  var S = { atkMul: 1, inv: 0, spdVal: 1, ad: 0, spdApplied: 0, prevSpeed: 1, n: 0, err: 0 };
  function RET(n) {
    try {
      return !!jsb.reflection.callStaticMethod(ACT, "getRet", "(I)Z", n);
    } catch (e) { return false; }
  }
  function VAL(n) {
    try {
      return jsb.reflection.callStaticMethod(ACT, "getVal", "(I)I", n) || 0;
    } catch (e) { return 0; }
  }
  function refresh() {
    S.atkMul = VAL(1);
    if (S.atkMul < 1) { S.atkMul = 1; }
    if (S.atkMul > 100) { S.atkMul = 100; }
    S.inv = RET(2) ? 1 : 0;
    S.spdVal = VAL(3);
    if (S.spdVal < 1) { S.spdVal = 1; }
    if (S.spdVal > 5) { S.spdVal = 5; }
    S.ad = RET(4) ? 1 : 0;
  }
  function zeroLike(v) {
    return (v && typeof v.mulInt === "function") ? v.mulInt(0) : v;
  }
  function getBattle() {
    try { if (typeof Battle !== "undefined" && Battle) { return Battle; } } catch (e) {}
    try { return window.Battle || null; } catch (e) { return null; }
  }
  function getReward() {
    try { if (typeof Reward !== "undefined" && Reward) { return Reward; } } catch (e) {}
    try { return window.Reward || null; } catch (e) { return null; }
  }
  function logError(where, e) {
    if (S.err >= 3) { return; }
    S.err += 1;
    try { cc.log("[zzbx2] " + where + " error: " + e); } catch (_) {}
  }
  function applyBattle() {
    try {
    var B = getBattle();
    if (!B || !B.BaseObjectComponent || !B.BaseObjectComponent.prototype) { return; }
    var P = B.BaseObjectComponent.prototype;
    if (typeof P.getBuffCanBeAttack === "function" && !P.__starry_zzbx2_buff) {
      var origBuff = P.getBuffCanBeAttack;
      P.getBuffCanBeAttack = function (dmg, bullet) {
        try {
          if (dmg && typeof dmg.GTInt === "function" && this && this.entity && B.EntitySide) {
            if (S.inv && this.entity.side === B.EntitySide.Own && dmg.GTInt(0)) {
              return zeroLike(dmg);
            }
          }
        } catch (e) {}
        return origBuff.call(this, dmg, bullet);
      };
      P.__starry_zzbx2_buff = 1;
      S.n += 1;
    }
    if (typeof P.changeHp === "function" && !P.__starry_zzbx2_hp) {
      var origHp = P.changeHp;
      P.changeHp = function (amount, damageType, isSkill) {
        try {
          if (this && this.entity && B.EntitySide && amount) {
            var isDamage = (damageType === B.DamageType.damage || damageType === B.DamageType.bulletDamage);
            if (isDamage && this.entity.side === B.EntitySide.Enemy && S.atkMul > 1) {
              if (amount && typeof amount.mulInt === "function") { amount = amount.mulInt(S.atkMul); }
            } else if (isDamage && this.entity.side === B.EntitySide.Own && S.inv) {
              amount = zeroLike(amount);
            }
          }
        } catch (e) {}
        return origHp.call(this, amount, damageType, isSkill);
      };
      P.__starry_zzbx2_hp = 1;
      S.n += 1;
    }
    } catch (e) { logError("applyBattle", e); }
  }
  function applyAds() {
    try {
    var R = getReward();
    if (!R || !R.AdvertMgr || !R.AdvertMgr.prototype) { return; }
    var P = R.AdvertMgr.prototype;
    if (typeof P.adsPlay !== "function" || P.__starry_zzbx2_ad) { return; }
    var origAds = P.adsPlay;
    P.adsPlay = function (adType, extra, modelId) {
      if (!S.ad) { return origAds.apply(this, arguments); }
      var args = Array.prototype.slice.call(arguments);
      var self = this;
      try {
        self._isPlaying = false;
        if (typeof self.startAdverView === "function") { self.startAdverView(adType, true, modelId); }
        if (typeof self.finishAdverViewed !== "function") {
          return Promise.resolve({ succ: false, rdata: null, reason: "starry_ad_bridge_missing" });
        }
        return Promise.resolve(self.finishAdverViewed(adType, true, modelId)).then(function () {
          try {
            var d = window.DataStatistcsMgr || (typeof DataStatistcsMgr !== "undefined" ? DataStatistcsMgr : null);
            if (d && d.inst && typeof d.inst.viewAdsFinished === "function") {
              d.inst.viewAdsFinished.apply(d.inst, args.slice(3));
            }
          } catch (e) {}
          return { succ: true, rdata: null };
        }).catch(function (e) {
          logError("adFinish", e);
          return { succ: false, rdata: null, reason: "starry_ad_bridge_error" };
        });
      } catch (e) {
        return Promise.resolve({ succ: false, rdata: null, reason: "starry_ad_bridge_error" });
      }
    };
    P.adsPlay.__starry_zzbx2_ad = 1;
    S.n += 1;
    } catch (e) { logError("applyAds", e); }
  }
  function applySpeed() {
    var B = getBattle();
    if (!B || !B.GameWorld || !B.GameWorld.inst) { return; }
    var inst = B.GameWorld.inst;
    try {
      if (S.spdVal > 1) {
        if (!S.spdApplied) {
          S.prevSpeed = (typeof inst.gameSpeed === "number" && inst.gameSpeed > 0) ? inst.gameSpeed : 1;
          S.spdApplied = 1;
        }
        if (inst.gameSpeed !== S.spdVal) { inst.gameSpeed = S.spdVal; }
      } else if (S.spdApplied) {
        S.spdApplied = 0;
        inst.gameSpeed = (S.prevSpeed > 0) ? S.prevSpeed : 1;
      }
    } catch (e) {}
  }
  function tick() {
    try {
    refresh();
    applyBattle();
    applyAds();
    applySpeed();
    } catch (e) { logError("tick", e); }
  }
  tick();
  var scheduled = 0;
  try {
    var sch = cc.director.getScheduler();
    sch.schedule(tick, cc.director, 0.5, cc.macro.REPEAT_FOREVER, 0, false);
    scheduled = 1;
  } catch (e) {}
  if (!scheduled) { try { setInterval(tick, 500); } catch (e) {} }
  try { cc.log("[zzbx2] patch applied n=" + S.n); } catch (e) {}
})();
