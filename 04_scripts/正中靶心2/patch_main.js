/*starry_zzbx2_v1*/
;(function () {
  var G = (typeof window !== "undefined" && window) ? window
        : (typeof globalThis !== "undefined" && globalThis) ? globalThis : this;
  if (G.__starry_zzbx2) { dbgRaw("skip: already applied"); return; }
  G.__starry_zzbx2 = 1;

  /* ---- 诊断通道：先走 android.util.Log（logcat tag ZZBX2JS），失败再退 console ---- */
  function dbgRaw(msg) {
    try {
      if (typeof jsb !== "undefined" && jsb && jsb.reflection && jsb.reflection.callStaticMethod) {
        jsb.reflection.callStaticMethod("android.util.Log", "i",
          "(Ljava/lang/String;Ljava/lang/String;)I", "ZZBX2JS", String(msg));
        return;
      }
    } catch (e) {}
    try { console.log("[ZZBX2JS] " + msg); } catch (e) {}
  }
  var DBG_MIN = 2000;          // 两条日志最小间隔（ms），防刷屏
  var dbgLast = {};
  function dbg(msg, force) {
    try {
      var k = String(msg).split("=")[0];
      var now = Date.now();
      if (!force && dbgLast[k] && now - dbgLast[k] < DBG_MIN) { return; }
      dbgLast[k] = now;
      dbgRaw(msg);
    } catch (e) {}
  }

  dbgRaw("boot: jsb=" + (typeof jsb) + " cc=" + (typeof cc) + " egret=" + (typeof egret)
    + " Shop=" + (typeof Shop) + " Battle=" + (typeof Battle)
    + " window=" + (typeof window));

  var ACT = "com.starry.magic.cocos.Zzbx2Hooks";
  var S = {
    atkMul: 1,
    spdUnlock: 0, spdApplied: 0, prevSpeed: 1,
    gateInstalled: 0, n: 0, err: 0, ticks: 0, retFail: 0
  };
  function RET(n) {
    try {
      return !!jsb.reflection.callStaticMethod(ACT, "getRet", "(I)Z", n);
    } catch (e) { S.retFail += 1; return false; }
  }
  function VAL(n) {
    try {
      return jsb.reflection.callStaticMethod(ACT, "getVal", "(I)I", n) || 0;
    } catch (e) { S.retFail += 1; return 0; }
  }
  function refresh() {
    var prev = [S.spdUnlock, S.atkMul];
    S.atkMul = VAL(1);
    if (S.atkMul < 1) { S.atkMul = 1; }
    if (S.atkMul > 10) { S.atkMul = 10; }
    S.spdUnlock = RET(2) ? 1 : 0;
    if (prev[0] !== S.spdUnlock || prev[1] !== S.atkMul) {
      dbg("state changed: atkMul=" + S.atkMul + " spdUnlock=" + S.spdUnlock
        + " retFail=" + S.retFail, true);
    }
  }
  function getBattle() {
    try { if (typeof Battle !== "undefined" && Battle) { return Battle; } } catch (e) {}
    try { return G.Battle || null; } catch (e) { return null; }
  }
  function getShop() {
    try { if (typeof Shop !== "undefined" && Shop) { return Shop; } } catch (e) {}
    try { return G.Shop || null; } catch (e) { return null; }
  }
  function logError(where, e) {
    if (S.err >= 5) { return; }
    S.err += 1;
    dbgRaw("ERROR " + where + ": " + e + (e && e.stack ? (" | " + e.stack) : ""));
  }
  function applyBattle() {
    try {
    var B = getBattle();
    if (!B || !B.BaseObjectComponent || !B.BaseObjectComponent.prototype) { return; }
    var P = B.BaseObjectComponent.prototype;
    if (typeof P.changeHp === "function" && !P.__starry_zzbx2_hp) {
      var origHp = P.changeHp;
      P.changeHp = function (amount, damageType, isSkill) {
        try {
          if (this && this.entity && B.EntitySide && amount) {
            var isDamage = (damageType === B.DamageType.damage || damageType === B.DamageType.bulletDamage);
            if (isDamage && this.entity.side === B.EntitySide.Enemy && S.atkMul > 1) {
              if (amount && typeof amount.mulInt === "function") { amount = amount.mulInt(S.atkMul); }
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
  /* 功能2：解锁月卡特权（服务端月卡态 VIP.ADRVIP / StatusId=="advip"）。
   * 游戏把月卡态集中暴露为 Shop.ShopVipMgr.inst.outStatusAdvip，全文件 24 处
   * 引用都是真值判断；放行后连带可用：战斗加速 1x/1.8x（Battle.SpeedUpBtn）、
   * 广告直接跳过播放（AdvertMgr.adsPlay case 3，等价于原「免广告」开关）、
   * 自动战斗/体力上限/广告特权入口等**纯客户端**判定项。
   * 反例（已实测证伪）：扫荡不发奖 —— 它与服务端 C2S_SPEED_PASS_LEVEL 交互，
   * 回包 errcode!=0 即拒绝，客户端放行只能把请求发出去、拿不到奖励，故不做。
   * 此处只在该 getter 返回假值时接管，真月卡账号仍走原逻辑；不改写任何服务端数据。
   * 注：原独立的「免广告」(RET 4) 开关已删除——月卡门本身就覆盖了广告跳过。 */
  function speedItemKey(B) {
    try {
      if (B && B.BattleDataKey && typeof B.BattleDataKey.getKey === "function") {
        return B.BattleDataKey.getKey(B.BattleDataKey.SPEED_BTN);
      }
    } catch (e) {}
    return null;
  }
  /* 沿原型链找 getter（Object.getOwnPropertyDescriptor 只看自有属性，
   * 真机上 ShopVipMgr.prototype 自有没有该描述符 ⇒ 必须链式搜）。 */
  function findGetterInChain(obj, prop) {
    var o = obj;
    while (o) {
      var d = null;
      try { d = Object.getOwnPropertyDescriptor(o, prop); } catch (e) { d = null; }
      if (d) {
        return { owner: o, get: (typeof d.get === "function") ? d.get : null, data: !!(d.get || d.set) ? null : d.value };
      }
      try { o = Object.getPrototypeOf(o); } catch (e) { o = null; }
    }
    return null;
  }
  var gateDumped = 0;
  function gateDump(why) {
    if (gateDumped) { return; }
    gateDumped = 1;
    var Sh = getShop();
    if (!Sh || !Sh.ShopVipMgr) { dbgRaw("dump(" + why + "): no ShopVipMgr"); return; }
    var out = "dump(" + why + ")";
    try { out += " statics=" + Object.getOwnPropertyNames(Sh.ShopVipMgr).join("|"); } catch (e) { out += " statics=throw"; }
    var P = Sh.ShopVipMgr.prototype;
    try { out += " protoOwn=" + (P ? Object.getOwnPropertyNames(P).join("|") : "noP"); } catch (e) { out += " protoOwn=throw"; }
    var f = findGetterInChain(P, "outStatusAdvip");
    out += " chain=" + (f ? ("yes(data=" + (f.data === undefined ? "undef" : String(f.data)) + ")") : "no");
    try {
      var inst = Sh.ShopVipMgr.inst;
      out += " instOwn=" + (inst ? Object.getOwnPropertyNames(inst).join("|") : "noInst");
    } catch (e) { out += " instOwn=throw"; }
    dbgRaw(out);
  }
  function gateProbe() {
    var Sh = getShop();
    if (!Sh) { return "Shop=none"; }
    if (!Sh.ShopVipMgr) { return "ShopVipMgr=none"; }
    var P = Sh.ShopVipMgr.prototype;
    var d = P ? Object.getOwnPropertyDescriptor(P, "outStatusAdvip") : null;
    var inst = null;
    try { inst = Sh.ShopVipMgr.inst; } catch (e) { return "inst throw:" + e; }
    var live = null;
    try { live = inst ? inst.outStatusAdvip : null; } catch (e) { live = "throw:" + e; }
    return "own=" + (!!d) + " isOurs=" + (!!(d && d.get && d.get.__starry))
      + " inst=" + (!!inst) + " live=" + (live === null ? "null" : (live === undefined ? "undef" : (live ? "TRUTHY" : "falsy")));
  }
  function applyVipGate() {
    try {
    if (S.gateInstalled) { return; }
    var Sh = getShop();
    if (!Sh || !Sh.ShopVipMgr || !Sh.ShopVipMgr.prototype) { return; }
    var P = Sh.ShopVipMgr.prototype;
    /* 原始取值：沿链找到的 getter 优先；找不到就直接读实例上的 _outStatusAdvip
     * （官方源码里 outStatusAdvip 的 getter 就是 return this._outStatusAdvip）。 */
    var found = findGetterInChain(P, "outStatusAdvip");
    var origGet = (found && found.get) ? found.get : null;
    var fake = {
      StatusId: "advip",
      EndTime: String(Math.floor(Date.now() / 1000) + 3153600000)
    };
    function readOrig(self) {
      if (origGet) {
        try { return origGet.call(self); } catch (e) {}
      }
      try { return self ? self._outStatusAdvip : undefined; } catch (e) { return undefined; }
    }
    function makeGet(tag) {
      var g = function () {
        var v = readOrig(this);
        if (v) { return v; }
        return S.spdUnlock ? fake : v;
      };
      g.__starry = 1;
      g.__starryTag = tag;
      return g;
    }
    var protoGet = makeGet("proto");
    Object.defineProperty(P, "outStatusAdvip", {
      configurable: true,
      enumerable: true,
      get: protoGet
    });
    /* 双保险：单例实例上也直接盖自有 getter（游戏全部走 Shop.ShopVipMgr.inst） */
    var inst = null;
    try { inst = Sh.ShopVipMgr.inst; } catch (e) { inst = null; }
    if (inst) {
      try {
        Object.defineProperty(inst, "outStatusAdvip", {
          configurable: true,
          enumerable: true,
          get: makeGet("inst")
        });
      } catch (e) { logError("gateInst", e); }
    }
    P.__starry_zzbx2_advip = 1;
    P.__starry_zzbx2_fake = fake;
    S.gateInstalled = 1;
    S.n += 1;
    dbgRaw("gate installed: chainFound=" + (!!found) + " inst=" + (!!inst) + " | " + gateProbe());
    gateDump("installed");
    } catch (e) { logError("applyVipGate", e); }
  }
  function applySpeed() {
    try {
    if (S.spdUnlock === S.spdApplied) { return; }
    var B = getBattle();
    var inst = (B && B.GameWorld) ? B.GameWorld.inst : null;
    var key = speedItemKey(B);
    if (S.spdUnlock) {
      S.spdApplied = 1;
      if (inst) {
        S.prevSpeed = (typeof inst.gameSpeed === "number" && inst.gameSpeed > 0) ? inst.gameSpeed : 1;
      }
      if (key) {
        try {
          if (!(egret.localStorage.getItem(key) > 0)) { egret.localStorage.setItem(key, 2); }
        } catch (e) {}
      }
      if (inst) { inst.gameSpeed = 1.8; }
    } else {
      S.spdApplied = 0;
      if (key) { try { egret.localStorage.setItem(key, 0); } catch (e) {} }
      if (inst && inst.gameSpeed > 1) { inst.gameSpeed = 1; }
    }
    dbg("applySpeed spdApplied=" + S.spdApplied + " key=" + key + " gameSpeed=" + (inst ? inst.gameSpeed : "noinst"), true);
    } catch (e) { logError("applySpeed", e); }
  }
  function tick() {
    try {
    S.ticks += 1;
    refresh();
    applyBattle();
    applyVipGate();
    applySpeed();
    if (S.ticks % 12 === 1) {
      dbg("hb: spdUnlock=" + S.spdUnlock + " gate=" + S.gateInstalled + " n=" + S.n
        + " err=" + S.err + " retFail=" + S.retFail + " | " + gateProbe());
      if (!S.gateInstalled) { gateDump("hb-noinstall#tick" + S.ticks); }
    }
    } catch (e) { logError("tick", e); }
  }
  tick();
  var scheduled = 0;
  try {
    var sch = cc.director.getScheduler();
    sch.schedule(tick, cc.director, 0.5, cc.macro.REPEAT_FOREVER, 0, false);
    scheduled = 1;
  } catch (e) { dbgRaw("scheduler failed: " + e); }
  if (!scheduled) { try { setInterval(tick, 500); dbgRaw("fallback setInterval ok"); } catch (e) { dbgRaw("setInterval failed: " + e); } }
  dbgRaw("patch applied n=" + S.n + " scheduled=" + scheduled + " spdUnlock=" + S.spdUnlock
    + " gate=" + S.gateInstalled + " | " + gateProbe());
})();
