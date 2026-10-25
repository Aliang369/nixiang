/* ============================================================================
 * 星空自动化层 v1  —— 对照卡密包(勇士之心 v66)同名功能复刻
 * 复刻来源：03_decompiled/勇士之心_cardkey_20261007/提取_补丁_v66_原始.js
 *   GoldCardFeature        L2975-2996  -> (1)  解锁金卡        $eZ.monCard.fDn.prototype.fDc
 *   syncEmbeddedAutoConfig L6813-6838  -> (2a) 游戏内自动挑战位(lm.cms.autoBattleFlag bit1)
 *   autoRepeatFinishTick   L6840-6888  -> (2b) 自动重复挑战(结算面板 efi 按钮，token 三重去重)
 *   autoSellDecomposeNow   L4729-4780  -> (3)(4) 自动出售 / 自动分解 (bag.bYK / bag.bYJ)
 *   _sdQualitiesOf         L4658-4687  -> 出售/分解品质来源
 *   autoWearBestEquip      L4580-4608  -> (5)  自动穿戴        (equip.ftA -> equip.fvH)
 *   autoPostCleanupTick    L6800-6820  -> 驱动与错峰节流
 *
 * ------------------------------------------------------------------ 主动偏离 --
 * 1. 不引入他的远程菜单框架（__REMOTE_COMMANDS__ / RemoteUI / __WX_CARD_PERMANENT__）。
 *    我们只搬【功能逻辑】，开关一律读【我们自己的原生菜单】：globalThis.MOD.feature6..10
 *    -> Java com.starry.magic.cocos.YsxzHooks.getRet(8..13)。
 * 2. 金卡与自动战斗【拆成两个开关】。他那边是同一个 CFG.autoFight（菜单项叫
 *    「解锁金卡与自动战斗」）；我们分开，避免"只想解锁金卡却被顺手开了自动战斗"。
 * 3. 出售/分解品质【只读游戏内自己的勾选】(localMgr.cnp / localMgr.cnq)，
 *    不学他那套 localStorage 勾选框；游戏内一个都没勾时兜底 [1,2,3]
 *    （兜底照抄他，否则"开关开着却空转"——他为此写过 v33N 修）。
 * 4. 【只写游戏内自动挑战那一位】，不碰 autoBattleFlag 的其它位，也不调 lm.cnr 反写
 *    出售/分解品质（把游戏内的选择权留给玩家）。
 *
 * ---------------------------------------------------------------- 时序铁律 --
 * 卡密包作者原注释 L6228-6232 逐字：
 *   「这些都是【联网协议】，必须等【已回城】且错峰执行，否则会撞车产生网络异常：
 *     0.8s 出售/分解 → 2.0s 穿戴 → 3.2s 技能」
 * 本层照做：战斗中绝不发协议；只在该局【战斗→城镇】的那一次触发，
 * 一次触发内错峰 900ms / 2100ms，两次触发之间至少间隔 5 秒。
 *
 * ---------------------------------------------------------------- 静默失效 --
 * 上一轮过检测层踩过"钩子没挂上却什么都不打印"的坑（那个 bug 靠"一行日志都没有"
 * 才被抓出来）。本层同样给每一步留【有界证据日志】：金卡让一次打一条(≤3)、
 * 重复挑战点一次打一条(≤3)、回城清理每次打一行(≤3)、穿戴提交一次打一条(≤3)。
 * 另外每 tick 计数，第 15 / 60 tick 各打一张状态快照；`window.__AUTO_STATE__()` 随时可读。
 * ========================================================================== */
;(function () {
  var AUTO_VER = 4;   /* 4 = 2026-10-25：补回 autoBattleTick() 驱动调用（3 = 自动化四项改挂官方口 + 结算面板时机） */
  if (typeof window === "undefined") return;
  var _prev = window.__starry_auto;
  if (_prev && (_prev.__v | 0) >= AUTO_VER) return;
  var TAG = "[AUTO] ";
  function log(s) {
    try { if (window.__starry_nlog) window.__starry_nlog(TAG + s); } catch (e) {}
    try { if (window.MOD_LOG) window.MOD_LOG.push(TAG + s); } catch (e) {}
    try { if (window.console && console.log) console.log(TAG + s); } catch (e) {}
  }
  function msgOf(e) { try { return e && e.message ? e.message : String(e); } catch (x) { return "?"; } }
  /* 【一次性诊断】开关开着、但目标 API 在本版本不存在时，必须留下一条日志。
   * 理由：本层依赖的 fDc / autoBattleFlag / ftA / fvH / bYK / bYJ / efi 全是
   * 【逐版本重混淆的短名】。官方热更改名后，钩子挂不上、协议发不出去，
   * 而菜单开关照样显示"开"——这正是上一轮过检测层栽过的静默失效。
   * 每个点位只打一次，热路径保持冷。 */
  var MISS = {};
  function miss(key, s) {
    if (MISS[key]) return;
    MISS[key] = 1;
    A.warnN++;
    log("warn: " + s);
  }

  var A = {
    __v: AUTO_VER,
    /* (1) 解锁金卡 */
    goldHooked: false, goldKey: "", goldHow: "", goldAllowN: 0,
    /* (2) 自动重复挑战 */
    repeatFlagSync: 0, repeatClickN: 0,
    repeatToken: 0, repeatScheduled: null, repeatHandled: null,
    /* (3)(4) 出售 / 分解 */
    sellN: 0, decoN: 0, sellDef: 0, decoDef: 0, lastSellQ: "", lastDecoQ: "",
    sdDump: "", sdLast: null, sdTotal: 0, sdLocked: 0, sdDecoable: 0,
    sdFinal: 0, sdEmptyLogged: 0,
    protoSent: 0, protoCode: "", pkHooked: false, evProto: 0,
    /* (5) 穿戴 */
    wearN: 0, wearOkN: 0, wearEmptyN: 0,
    /* 驱动 / 节流 */
    tickN: 0, inBattleLatch: false, busy: false, lastCleanup: 0, battleN: 0,
    acTickSeen: 0, evGold: 0, evRepeat: 0, evClean: 0, evWear: 0, sdProbe: 0,
    errN: 0, lastErr: "", note: "", loopStarted: false, warnN: 0
  };

  /* ------------------------------------------------------------------ 基础 -- */
  function MOD() { try { return window.MOD || null; } catch (e) { return null; } }
  /* 菜单开关：feature6=解锁金卡 feature7=自动重复挑战 feature8=自动出售
   *           feature9=自动分解 feature10=自动穿戴  （序号见 YsxzEntry.cpp） */
  function FEAT(n) {
    try { var m = MOD(); return !!(m && m["feature" + n]); } catch (e) { return false; }
  }
  function F() {
    try { var z = window.$eZ; return z && z.battleMgr ? z : null; } catch (e) { return null; }
  }
  function err(tag, e) {
    A.errN++;
    var s = tag + ": " + msgOf(e);
    A.lastErr = s;
    if (A.errN <= 6) log("ERR " + s);
  }

  /* 战斗信号。两条独立通道取【或】：
   *  ① 游戏自己的战斗管理器 $eZ.battleMgr.bEu.no()  —— 照抄他 hasBattleActive L3669
   *  ② 过检测层的战斗帧计数在涨            —— 真机已验证这条链是通的(v4 日志 wallMs=32333)
   * 只用①有风险：若 bEu.no 在本版本不存在，信号恒 false => 回城清理永远不触发（静默失效）。 */
  function battleSignal() {
    try {
      var JE = F() && F().battleMgr && F().battleMgr.bEu;
      if (JE && typeof JE.no === "function") { if (JE.no()) return true; }
    } catch (e) {}
    try {
      var ac = window.__starry_ac;
      if (ac) {
        var t = ac.tickSeen | 0;
        var grow = t > (A.acTickSeen | 0);
        A.acTickSeen = t;
        if (grow) return true;
      }
    } catch (e) {}
    return false;
  }

  /* ======================================================== (1) 解锁金卡 ====
   * 他的实现 GoldCardFeature L2975-2996：取 $eZ.monCard.fDn 的【原型】上的 fDc(卡类型)，
   * 判 gold 时直接改返 true。我们用独立开关 feature6（他是 CFG.autoFight）。
   *
   * 【2026-10-19 / res 1.1.6.0 修复：该类短名整体漂移】
   *   实测对照（1.1.4.0 -> 1.1.6.0，同一 bundle 只差 721 字节，其它功能锚点一个没变）：
   *     $eZ.monCard.fDn (持有数据对象)      -> fDq
   *     原型 fDc (是否持有, 返回 boolean)   -> fDf
   *     原型 fDf (剩余秒数, 返回 number)    -> fDi
   *     原型 fDe/fDb/fDj (可购/是否永久/任务) -> fDh/fDj/fDk
   *   ⇒ 旧写法 data.fDn / proto.fDc 全部落空：开关开着、钩子挂不上、无任何提示（静默失效）。
   *   按 hook-rules §24 不写死短标识符，改两级发现：
   *     tier1 结构锚点：monCard 下「原型方法体含明文 "silver" + Premium + buffId + $eZ.buff」的对象
   *                     （明文属性 Basic/Premium/buffId 与 $eZ.buff 这类公共管理器不参与混淆）
   *     tier2 行为甄别：候选里唯一【拿 "gold" 调用返回 boolean】的才是"持有判定"
   *                      （同族的"剩余秒数"返回 number；"商品配置是否存在"含 findByKey 已排除）
   *   都落空时退回历史短名列表（fDf/fDc…）并做同样的 boolean 行为校验。
   *   命中哪一级都写进 goldKey + 一行日志 ⇒ 下一版热更直接看日志就知道走没走对。 */
  var GOLD_LEGACY = ["fDf", "fDc", "fDe", "fDn"];
  function goldCandidates(v) {
    var out = [];
    var p;
    try { p = Object.getPrototypeOf(v); } catch (e) { return out; }
    if (!p || p === Object.prototype) return out;
    var pn;
    try { pn = Object.getOwnPropertyNames(p); } catch (e) { return out; }
    for (var j = 0; j < pn.length; j++) {
      var f;
      try { f = p[pn[j]]; } catch (e1) { continue; }
      if (typeof f !== "function") continue;
      var src = "";
      try { src = Function.prototype.toString.call(f); } catch (e2) { continue; }
      if (src.indexOf('"silver"') < 0 || src.indexOf("Premium") < 0) continue;
      if (src.indexOf("buffId") < 0 || src.indexOf("$eZ.buff") < 0) continue;
      if (src.indexOf("findByKey") >= 0) continue;     /* 同族里"商品/子buff配置是否存在"那条 */
      out.push({ name: pn[j], fn: f });
    }
    return out;
  }
  function goldAcceptBool(o, v) {
    var r;
    try { r = o.fn.call(v, "gold"); } catch (e) { return false; }
    return typeof r === "boolean";                      /* 剩余秒数那条返回 number, 这里被刷掉 */
  }
  function goldScan(z) {
    var mc = z && z.monCard;
    if (!mc) return null;
    var names;
    try { names = Object.getOwnPropertyNames(mc); } catch (e) { return null; }
    /* 第一趟：结构锚点 + 行为甄别 */
    for (var i = 0; i < names.length; i++) {
      var v;
      try { v = mc[names[i]]; } catch (e1) { continue; }
      if (!v || typeof v !== "object" || v === mc) continue;
      var cands = goldCandidates(v);
      for (var k = 0; k < cands.length; k++) {
        if (goldAcceptBool(cands[k], v)) {
          return { data: v, dataKey: names[i], name: cands[k].name, how: "struct+behaviour" };
        }
      }
    }
    /* 第二趟：历史短名 + 同样的行为校验（只认 boolean，防挂到"剩余秒数"上） */
    for (var i2 = 0; i2 < names.length; i2++) {
      var v2;
      try { v2 = mc[names[i2]]; } catch (e3) { continue; }
      if (!v2 || typeof v2 !== "object") continue;
      var p2;
      try { p2 = Object.getPrototypeOf(v2); } catch (e4) { continue; }
      if (!p2) continue;
      for (var n2 = 0; n2 < GOLD_LEGACY.length; n2++) {
        var f2 = p2[GOLD_LEGACY[n2]];
        if (typeof f2 !== "function") continue;
        if (!goldAcceptBool({ fn: f2 }, v2)) continue;
        return { data: v2, dataKey: names[i2], name: GOLD_LEGACY[n2], how: "legacy-name" };
      }
    }
    return null;
  }
  function tryGoldCard() {
    if (A.goldHooked) return true;
    if (!FEAT(6)) return false;
    try {
      var z = window.$eZ;
      if (!z) return false;                          /* 还没加载到 $eZ：不算缺失, 别误报 */
      if (!z.monCard) { miss("gold.data", "解锁金卡: 开关开着但 $eZ.monCard 不存在 —— 本版本改名了？该功能不会生效"); return false; }
      var hit = goldScan(z);
      if (!hit) {
        miss("gold.fn", "解锁金卡: monCard 下找不到「持有判定」方法（结构+行为两级全落空）—— 本版本改了结构？该功能不会生效");
        return false;
      }
      var data = hit.data, proto = Object.getPrototypeOf(data), cur = proto[hit.name];
      var key = "monCard." + hit.dataKey + "." + hit.name;
      if (typeof cur === "function" && cur.__starryGold && (cur.__starryGoldVer | 0) >= AUTO_VER) {
        A.goldHooked = true; A.goldKey = key;
        return true;
      }
      var orig = (typeof cur === "function" && cur.__starryGoldOrig) || cur;
      if (typeof orig !== "function") { miss("gold.orig", "解锁金卡: 定位到 " + key + " 但不是函数"); return false; }
      var w = function (cardType) {
        var owned = orig.apply(this, arguments);
        try {
          if (cardType === "gold" && FEAT(6)) {
            A.goldAllowN++;
            if (A.evGold < 3) { A.evGold++; log("gold: 放行 gold 卡判定 (第 " + A.goldAllowN + " 次调用)"); }
            return true;
          }
        } catch (e) {}
        return owned;
      };
      w.__starryGold = true;
      w.__starryGoldVer = AUTO_VER;
      w.__starryGoldOrig = orig;
      /* 挂上去必须能【读回验证】：游戏 bundle 里存在 sealMethods（部分原型被冻结/被包装），
       * 直接赋值可能静默失败 —— 那样 goldHooked=true 就是假成功，正是要根治的形态。 */
      var placed = false;
      try { proto[hit.name] = w; placed = (proto[hit.name] === w); } catch (e1) {}
      if (!placed) {
        try {
          Object.defineProperty(proto, hit.name, { value: w, writable: true, configurable: true, enumerable: false });
          placed = (proto[hit.name] === w);
        } catch (e2) {}
      }
      if (!placed) {
        miss("gold.pin", "解锁金卡: 定位到 " + key + " 但原型被冻结/写不进去 —— 该功能不会生效（本版本给原型上了 seal）");
        return false;
      }
      A.goldHooked = true;
      A.goldKey = key;
      A.goldHow = hit.how;
      log("gold-card hooked: $eZ." + key + " [发现方式=" + hit.how + "] (cardType===gold -> true, 受 feature6 控制)");
      return true;
    } catch (e) { err("gold", e); return false; }
  }

  /* ============================================ (1b) 结算「特权翻卡」放行 ====
   * 2026-10-19 用户反馈：「解锁金卡后，结算翻卡额外获得装备怎么没有？」
   * 代码事实（assets/game/index.js 1.1.6.0）：
   *   · 翻卡面板 AdventureTurnCardPanel 把卡分三组：$DD.c=1 免费 / $DD.d=2 特权 / $DD.e=3 道具
   *   · 特权组每张卡能否翻，只看【服务端下发的那张卡对象上的 hasPrivilege】
   *     （L51761 `this.egP.hasPrivilege || (n = !1)`；L51822 无该字段则显示「购买月卡后获得」）
   *   · 该字段全包只有 3 处【读】、0 处【写】⇒ 不是客户端按 monCard.fDf("gold") 现算的
   *     ⇒ 本地伪造金卡 buff 改不动它，这是「怎么没有」的直接原因。
   *   · 特权翻卡【不消耗道具】：L51893 `if (egR == a.d) e = a.c` —— 特权卡按「免费」发请求，
   *     服务端才是最终裁决方（协议 FightInstance.selectCard）。
   * 因此这里做两件事（都只在金卡开关开着时）：
   *   ① 把面板里 type=2 的卡对象 hasPrivilege 置 true，让客户端愿意发请求；
   *   ② 包一层 battleMgr.bFh(selectCard)，把服务端返回的 code 打出来 ⇒ 能不能拿到由证据说话。
   * 拿不到也只是回一个错误码，不会扣道具/脏数据（特权组本身无 cost）。 */
  var TC = { seen: null, patched: 0, dumpN: 0, flipN: 0, flipCode: "", hooked: false };
  function tcPanel() {
    try {
      var fgm = F() && F().fguiMgr;
      if (!fgm) return null;
      var cfg = getPanelCfg("AdventureTurnCardPanel");
      if (cfg && typeof fgm.cTO === "function") { try { var p = fgm.cTO(cfg); if (p) return p; } catch (e) {} }
      if (cfg && typeof fgm.cTK === "function" && fgm.cTK(cfg) && typeof fgm.cTU === "function") return fgm.cTU();
      var top = typeof fgm.cTU === "function" ? fgm.cTU() : null;
      var tk = top ? String((top.vJ && top.vJ.cUV) || "") : "";
      if (tk && tk.indexOf("TurnCard") >= 0) return top;
    } catch (e) {}
    return null;
  }
  function tcHookFlip() {
    if (TC.hooked) return;
    try {
      var bm = F() && F().battleMgr;
      if (!bm || typeof bm.bFh !== "function") return;
      var of = bm.bFh;
      bm.bFh = function (t) {
        var pr;
        TC.flipN++;
        try { pr = of.apply(this, arguments); } catch (e) { throw e; }
        try {
          if (pr && pr.then) {
            pr.then(function (r) {
              var code = r && r.code;
              TC.flipCode = "code=" + code;
              if (TC.flipN <= 6) log("[tc] 翻卡协议(selectCard type=" + (t && t.type) + ") 返回 code=" + code);
            }, function (e2) {
              TC.flipCode = "reject";
              if (TC.flipN <= 6) log("[tc] 翻卡协议 rejected: " + msgOf(e2));
            });
          }
        } catch (e3) {}
        return pr;
      };
      TC.hooked = true;
      log("[tc] 翻卡协议已挂钩：能拿到服务端 code（0=成功，其它=被拒）");
    } catch (e) { err("tc/hook", e); }
  }
  function tcSweep() {
    if (!FEAT(6)) return;
    tcHookFlip();
    var p = tcPanel();
    if (!p) return;
    var found = { 1: 0, 2: 0, 3: 0 }, patched = 0, nodes = 0;
    var seen = new Map(), q = [p];
    while (q.length && nodes < 4000) {
      var o = q.shift(); nodes++;
      if (!o || typeof o !== "object" || seen.has(o)) continue;
      seen.set(o, 1);
      if (typeof o.type === "number" && o.rewards) {
        found[o.type] = (found[o.type] || 0) + 1;
        if (o.type === 2 && o.hasPrivilege !== true) { o.hasPrivilege = true; patched++; }
        continue;
      }
      /* Map / Array 必须显式展开：for-in 在 Map 上拿不到任何元素，
       * 少这一步的表现是"面板找到了但 0 张卡"（离线验收台当场抓到的）。 */
      try {
        if (o instanceof Map) {
          o.forEach(function (v) { if (v && typeof v === "object") q.push(v); });
          continue;
        }
        if (Array.isArray(o)) {
          for (var ai = 0; ai < o.length && ai < 500; ai++) {
            var av = o[ai]; if (av && typeof av === "object") q.push(av);
          }
          continue;
        }
      } catch (e0) {}
      for (var k in o) {
        try { var v = o[k]; if (v && typeof v === "object") q.push(v); } catch (e) {}
      }
    }
    if (!TC.dumpN) {
      TC.dumpN = 1;
      log("[tc] 翻卡面板卡组: 免费=" + found[1] + " 特权=" + found[2] + " 道具=" + found[3]
          + (found[2] ? " -> 已把 " + patched + " 张特权卡的 hasPrivilege 置真(等点一次看服务端 code)" : " -> 本副本没有特权卡组(配置里就没有)"));
    } else if (patched) {
      log("[tc] 又放行 " + patched + " 张特权卡");
    }
    if (patched) TC.patched += patched;
  }

  /* ============================================== (2) 自动重复挑战 =========
   * 两路并用，照抄他：
   *  (2a) 写游戏自己的自动挑战位 lm.cms.autoBattleFlag bit1（幂等，只在变化时写）
   *  (2b) 结算面板 AdventureFinishPanel 上的「再次挑战」按钮 efi()，
   *       用 token 三重去重（scheduled / handled / token 自增）防止重复点击。
   * 面板相关辅助函数照抄他 L6780-6808（getPanelCfg / isPanelOpen / getPanelCandidates）。 */
  var _JX = null, _PANEL_CFG = null;
  function getJX() {
    try {
      if (_JX) return _JX;
      var r = (window.__require && window.__require("$JX")) || null;
      if (r && r.$fb) _JX = r;
    } catch (e) {}
    return _JX;
  }
  function getGamePanelCfg() {
    try {
      if (_PANEL_CFG) return _PANEL_CFG;
      var mod = window.__require && window.__require("$RJ");
      _PANEL_CFG = mod && (mod.default || mod);
      return _PANEL_CFG;
    } catch (e) {}
    return null;
  }
  function getPanelCfg(name) {
    try {
      var jx = getJX();
      var p = jx && jx.$fb && jx.$fb.cvConfig && jx.$fb.cvConfig.panels;
      if (p && p[name]) return p[name];
      var gp = getGamePanelCfg();
      if (gp && gp[name]) return gp[name];
      var fc = F();
      if (fc && fc.cvConfig && fc.cvConfig.panels && fc.cvConfig.panels[name]) return fc.cvConfig.panels[name];
    } catch (e) {}
    return null;
  }
  function panelOpen(name) {
    try {
      var fgm = F() && F().fguiMgr;
      if (!fgm || !name) return false;
      var cfg = getPanelCfg(name);
      if (cfg && typeof fgm.cTK === "function") return !!fgm.cTK(cfg);
      var top = fgm.cTU ? fgm.cTU() : null;
      var tk = top ? String((top.vJ && top.vJ.cUV) || "") : "";
      return tk === name;
    } catch (e) { return false; }
  }
  function panelCandidates(fgm, prefix) {
    var out = [];
    try {
      if (!fgm) return out;
      var push = function (o) { if (o && typeof o === "object" && out.indexOf(o) < 0) out.push(o); };
      if (prefix) {
        var cfg0 = getPanelCfg(prefix);
        if (cfg0 && typeof fgm.cTO === "function") { try { push(fgm.cTO(cfg0)); } catch (e) {} }
      }
      var top = fgm.cTU ? fgm.cTU() : null;
      if (top) {
        var tk = String((top.vJ && top.vJ.cUV) || "");
        if (!prefix || (tk && tk.indexOf(prefix) >= 0)) push(top);
      }
    } catch (e) {}
    return out;
  }
  /* ============================================== (6) 自动战斗（buff 查询口）======
   * 2026-10-25（1.1.9.1 实测 auto=NO）：AUTO_BATTLE 判定函数所在类不在模块导出面上，
   * 类扫描永远看不到它。改挂**游戏自己的查询口**：官方实现就是
   *   $eZ.buff.bZL.bZF(tblEnum.BuffTableEnum.AUTO_BATTLE)
   * 我们只在该入参 === AUTO_BATTLE 时返回 true（feature5），其它 buff 原样透传。
   * 枚举值在运行时从 $eZ 里按 BuffTableEnum 反查（不写死数字）。 */
  var autoHooked = false;
  /* 2026-10-25（用户口径纠正 + 官方源码取证）：本功能是**解锁游戏内自动战斗按钮**，
     不是替玩家点按钮/代打。按钮可点判定（1.1.9.1 game@bBl）：
         this.btnAutoBattle.active && !KC.nn &&
         $eZ.monCard.fNc.fMY() &&                                   // AUTO_BATTLE(800) 权益
         $eZ.funcOpen.chW(FunctionUnlockTableEnum.MONTH_CARD_OPEN,false) &&
         !$eZ.localMgr.col()
     所以我们只放行这两处判定：monCard.fNc.fMY() 与 funcOpen.chW(MONTH_CARD_OPEN)。
     菜单开关（FEAT(5)）关掉时全部条件原样透传，游戏行为不变。 */
  var unlockHooked = false;
  function unlockAutoBattle() {
    if (unlockHooked) return;
    try {
      var z = F();
      if (!z) return;
      var okCount = 0;
      /* ① monCard.fNc.fMY() -> true */
      try {
        var nc = z.monCard && z.monCard.fNc;
        var proto = nc ? Object.getPrototypeOf(nc) : null;
        if (proto && typeof proto.fMY === "function") {
          var ofMY = proto.fMY;
          proto.fMY = function () {
            try { if (FEAT(5)) { if (!A.evUnlock1) { A.evUnlock1 = 1; log("auto-battle unlock: fMY() 放行 (AUTO_BATTLE 权益)"); } return true; } } catch (e) {}
            return ofMY.apply(this, arguments);
          };
          okCount++;
        }
      } catch (e1) { err("unlock/fMY", e1); }
      /* ② funcOpen.chW(MONTH_CARD_OPEN) -> true */
      try {
        var fo = z.funcOpen;
        var mc = null;
        try { var NN = window.__starry_N; if (NN && typeof NN.monthCardEnum === "number") mc = NN.monthCardEnum; } catch (eN) {}
        if (fo && typeof fo.chW === "function") {
          var ochW = fo.chW;
          fo.chW = function (id) {
            try {
              if (FEAT(5) && mc !== null && id === mc) {
                if (!A.evUnlock2) { A.evUnlock2 = 1; log("auto-battle unlock: funcOpen.chW(MONTH_CARD_OPEN=" + mc + ") 放行"); }
                return true;
              }
            } catch (e) {}
            return ochW.apply(this, arguments);
          };
          okCount++;
        } else if (!A.evNoChw) { A.evNoChw = 1; log("auto-battle unlock: funcOpen.chW 不存在（只放行 fMY）"); }
      } catch (e2) { err("unlock/chW", e2); }
      if (okCount > 0) { unlockHooked = true; log("auto-battle unlock: hooked " + okCount + " 处判定（菜单开关控制）"); }
      else if (!A.evUnlockMiss) { A.evUnlockMiss = 1; log("auto-battle unlock: 两处判定都没找到（monCard.fNc/funcOpen 结构变了？）"); }
    } catch (e) { err("unlock", e); }
  }

  function autoBattleTick() {
    try { unlockAutoBattle(); } catch (e) {}
    if (autoHooked) return;
    try {
      var z = F();
      if (!z || !z.buff) return;
      var b = z.buff.bZL;
      if (!b || typeof b.bZF !== "function") return;
      var enumVal = null;
      /* 2026-10-25（1.1.9.1 官方源码）：AUTO_BATTLE=800 / PREMIUM_AUTO_BATTLE_1=801，定义在
         assets/main/index.js 枚举表（e[e.AUTO_BATTLE = 800]）。运行时 $eZ 上遍历不到，源码
         正则也可能只覆盖部分模块 ⇒ 本版加字面兜底；换版本时以日志 "bZF ids=[...]" 实测为准。 */
      var AUTO_IDS = [800, 801];
      var seen = {};
      function findEnum(o, depth) {
        if (!o || typeof o !== "object" || depth < 0 || seen[o]) return null;
        seen[o] = 1;
        try {
          if (o.BuffTableEnum && typeof o.BuffTableEnum.AUTO_BATTLE === "number") return o.BuffTableEnum.AUTO_BATTLE;
          if (o.AUTO_BATTLE !== undefined && typeof o.AUTO_BATTLE === "number") return o.AUTO_BATTLE;
        } catch (e0) {}
        var ks = [];
        try { for (var k in o) ks.push(k); } catch (e1) {}
        for (var i = 0; i < ks.length && i < 200; i++) {
          var v = null;
          try { v = o[ks[i]]; } catch (e2) { continue; }
          var r = findEnum(v, depth - 1);
          if (r !== null) return r;
        }
        return null;
      }
      try {
        var NN = window.__starry_N;
        if (NN && typeof NN.autoBattleEnum === "number") enumVal = NN.autoBattleEnum;
      } catch (eN) {}
      if (enumVal === null) enumVal = findEnum(z, 3);
      if (enumVal === null) {
        enumVal = AUTO_IDS[0];
        if (!A.evAutoMiss) { A.evAutoMiss = 1; log("auto-battle: 枚举未反查到，用本版字面兜底 " + JSON.stringify(AUTO_IDS)); }
      }
      var AUTO_SET = {};
      for (var ai = 0; ai < AUTO_IDS.length; ai++) AUTO_SET[AUTO_IDS[ai]] = 1;
      AUTO_SET[enumVal] = 1;
      var orig = b.bZF;
      b.bZF = function (id) {
        try {
          A.evBzf = (A.evBzf || 0) + 1;
          if (A.evBzf < 40) {
            if (!A.bzfIds) A.bzfIds = [];
            if (A.bzfIds.indexOf(id) < 0) { A.bzfIds.push(id); log("bZF ids=[" + A.bzfIds.join(",") + "]"); }
          }
        } catch (eLog) {}
        try {
          if (AUTO_SET[id] && FEAT(5)) {
            if (!A.evAutoGrant) { A.evAutoGrant = 1; log("auto-battle: 放行 AUTO_BATTLE 查询 (feature5)"); }
            return true;
          }
        } catch (e) {}
        return orig.apply(this, arguments);
      };
      autoHooked = true;
      log("auto-battle: hooked $eZ.buff.bZL.bZF ids=" + JSON.stringify(Object.keys(AUTO_SET)) + " (feature5 控制)");
    } catch (e) { err("auto/buff", e); }
  }

  /* ================================================ (2) 自动重复挑战 =======
   * 2026-10-25（res 1.1.9.1 官方源码取证；取代 1.1.6 时代「点结算面板 efi」那一路）：
   * 游戏里它本身就是**金卡特权里的一档设置位**，不是外部代点按钮：
   *   localMgr.cnh.autoBattleFlag 的 bit(1<<t)，t = $OY.b = 1
   *   写：localMgr.cod(1, true/false)（内部还会 cno 保存设置）
   *   读：localMgr.coc(1)
   *   消费：AdventureFinishPanel.wA（game@52119）
   *         `coc($OY.b) && (btn_again.visible || btn_toNext.visible) && (...)`
   *         => 起 3 秒倒计时后由游戏自己再战。
   * 所以我们只点亮这一位、行为全交给游戏；另留一路兜底：面板开了 6.5 秒还没走
   * （说明游戏自己那一路被前置条件挡住），才替它点一次「再次挑战」
   * （面板方法 ekx = btn_again 的 onClick 绑定，1.1.9.1 已核 game@52255）。
   */
  var REPEAT_IDX = 1;          /* $OY.b；对应位 = 1<<1 = 2 */
  function repeatRead(lm) {
    try { if (lm && typeof lm.coc === "function") return !!lm.coc(REPEAT_IDX); } catch (e) {}
    return null;
  }
  function repeatFlagSync() {
    var on = FEAT(7);
    try {
      var z = F(), lm = z && z.localMgr;
      if (!lm) return;                                  /* $eZ 还没加载：不算缺失，别误报 */
      var holder = lm.cnh || lm.cms || null;            /* 1.1.9.1 = cnh */
      if (!holder) {
        if (on) miss("rflag", "自动重复挑战: localMgr 下没有 autoBattleFlag 容器（cnh/cms 都不在）");
        return;
      }
      var via = "";
      if (typeof lm.cod === "function") {
        try { lm.cod(REPEAT_IDX, on); via = "cod"; } catch (e) { err("repeat/cod", e); }
      }
      if (!via) {                                       /* 兜底：直接写位（公式同 cod） */
        var mask = 1 << REPEAT_IDX, f = Number(holder.autoBattleFlag) || 0;
        var want = on ? (f | mask) : (f & ~mask);
        if (want !== f) holder.autoBattleFlag = want;
        via = "flag";
      }
      var now = repeatRead(lm);
      if (A.repeatLast !== now) {
        A.repeatLast = now;
        A.repeatFlagSync++;
        if (A.repeatFlagSync <= 3) {
          log("repeat: 自动重复挑战位 " + via + " 置" + (on ? "1" : "0")
              + " -> 读回 coc(1)=" + now + " flag=" + (Number(holder.autoBattleFlag) || 0));
        }
      }
    } catch (e) { err("repeat/flag", e); }
  }

  /* ============================================ (3)(4) 自动出售 / 自动分解 ===
   * 2026-10-25（res 1.1.9.1 反查，修「开关开着却没效果」）：
   * 旧实现调的 bag.bYK / bag.bYJ 在这一版**是判定函数**——
   *   bYK(node) = 能否分解（EquipTable.decompose 非空）
   *   bYJ(node) = 是否带融合石
   * 把品质数组传进去只会返回布尔值，一个协议都不会发。这一版必须用官方自己的
   * 两条批量口（结算面板 wA 里游戏自己也是这么调的，game@52122-52131）：
   *   bag.bZw([quality..])  按品质批量出售（内部 pg 协议）
   *   bag.bZv([quality..])  按品质批量分解（内部 bXM 协议；自带 isLock + bYK 过滤）
   * 品质勾选读 localMgr.coe / localMgr.cof（自动出售 / 自动分解的品质位），
   * 游戏内一个都没勾时兜底 [1,2,3]（照卡密包 v33N：否则开了等于空转）。
   * 兜底两条（bZw/bZv 改名时用）：bag.bXJ(200,[{gridId,num}]) / bag.bXM({bagType,gridList})。
   */
  var EQUIP_BAG = 200;
  var WORE_BAG = 500;
  function equipNodes(bag) {
    var out = [];
    try {
      var c = typeof bag.bXW === "function" ? bag.bXW(EQUIP_BAG) : null;
      if (c && c.d && typeof c.d.bdL === "function") {
        c.d.bdL(function (n) { if (n && n.itemId && n.amount > 0) out.push(n); });
        return out;
      }
    } catch (e) {}
    try {
      var q = typeof bag.bYZ === "function" ? bag.bYZ(EQUIP_BAG) : null;
      var arr = (q && q.c) || [];
      for (var i = 0; i < arr.length; i++) if (arr[i] && arr[i].itemId && arr[i].amount > 0) out.push(arr[i]);
    } catch (e) {}
    return out;
  }
  function qualitiesOf(lm, useSell) {
    var set = {};
    /* 1.1.9.1：coe = 自动出售品质位，cof = 自动分解品质位；cnp/cnq 为旧版名 */
    var names = useSell ? ["coe", "cnp"] : ["cof", "cnq"];
    for (var i = 0; i < names.length; i++) {
      var fn = lm && lm[names[i]];
      if (typeof fn !== "function") continue;
      for (var q = 1; q <= 6; q++) { try { if (fn.call(lm, q)) set[q] = 1; } catch (e) {} }
      break;
    }
    return Object.keys(set).map(Number).filter(function (x) { return x >= 1 && x <= 6; })
                 .sort(function (a, b) { return a - b; });
  }
  function itemQuality(bag, node) {
    try {
      var i = bag.bYG ? bag.bYG(node.itemId) : null;
      var cfg = i && i.cfg;
      return cfg && cfg.quality !== undefined ? (cfg.quality | 0) : null;
    } catch (e) { return null; }
  }
  function manualSell(bag, nodes, qs) {
    var list = [];
    for (var i = 0; i < nodes.length; i++) {
      var n = nodes[i];
      if (!n || !n.gridId) continue;
      if (n.equip && n.equip.isLock) continue;
      var q = itemQuality(bag, n);
      if (q === null || qs.indexOf(q) < 0) continue;
      list.push({ gridId: n.gridId, num: n.amount });
    }
    if (!list.length) return 0;
    bag.bXJ(EQUIP_BAG, list);
    return list.length;
  }
  function manualDeco(bag, nodes, qs) {
    var gids = [];
    for (var i = 0; i < nodes.length; i++) {
      var n = nodes[i];
      if (!n || !n.gridId) continue;
      if (n.equip && n.equip.isLock) continue;
      var ok = false;
      try { ok = typeof bag.bYK === "function" ? !!bag.bYK(n) : true; } catch (e) {}
      if (!ok) continue;
      var q = itemQuality(bag, n);
      if (q === null || qs.indexOf(q) < 0) continue;
      gids.push(n.gridId);
    }
    if (!gids.length) return 0;
    bag.bXM({ bagType: EQUIP_BAG, gridList: gids });
    return gids.length;
  }
  function cleanupSellDeco() {
    var wantSell = FEAT(8), wantDeco = FEAT(9);
    if (!wantSell && !wantDeco) return false;
    try {
      var z = F(), bag = z && z.bag, lm = z && z.localMgr;
      if (!bag) { miss("sd.bag", "出售/分解: $eZ.bag 不存在 —— 该功能不会生效"); return false; }
      var nodes = equipNodes(bag);
      A.sdTotal = nodes.length;
      if (!nodes.length) {
        if (A.evClean < 3) { A.evClean++; log("[sd] 背包(" + EQUIP_BAG + ") 里没有装备 —— 本次不发协议（数据未加载或真为空）"); }
        return false;
      }
      var sellQ = wantSell ? qualitiesOf(lm, true) : [];
      var decoQ = wantDeco ? qualitiesOf(lm, false) : [];
      if (wantSell && !sellQ.length) {
        sellQ = [1, 2, 3];
        if (!A.sellDef) { A.sellDef = 1; log("[sd] 出售品质为空(游戏内未勾选) -> 兜底 [1,2,3]"); }
      }
      if (wantDeco && !decoQ.length) {
        decoQ = [1, 2, 3];
        if (!A.decoDef) { A.decoDef = 1; log("[sd] 分解品质为空(游戏内未勾选) -> 兜底 [1,2,3]"); }
      }
      var did = "";
      if (wantSell) {
        var n1 = -1;
        if (typeof bag.bZw === "function") { try { bag.bZw(sellQ); n1 = -2; } catch (e) { err("sell", e); } }
        else if (typeof bag.bXJ === "function") { try { n1 = manualSell(bag, nodes, sellQ); } catch (e) { err("sell/manual", e); } }
        else miss("sell.api", "自动出售: bag.bZw / bag.bXJ 都不可用 —— 该功能不会生效");
        if (n1 !== -1 && n1 !== 0) {
          A.sellN++; A.lastSellQ = sellQ.join(",");
          did += "出售[" + A.lastSellQ + (n1 > 0 ? " x" + n1 : "") + "] ";
        } else if (n1 === 0) { did += "出售[无可售件] "; }
      }
      if (wantDeco) {
        var n2 = -1;
        if (typeof bag.bZv === "function") { try { bag.bZv(decoQ); n2 = -2; } catch (e) { err("deco", e); } }
        else if (typeof bag.bXM === "function") { try { n2 = manualDeco(bag, nodes, decoQ); } catch (e) { err("deco/manual", e); } }
        else miss("deco.api", "自动分解: bag.bZv / bag.bXM 都不可用 —— 该功能不会生效");
        if (n2 !== -1 && n2 !== 0) {
          A.decoN++; A.lastDecoQ = decoQ.join(",");
          did += "分解[" + A.lastDecoQ + (n2 > 0 ? " x" + n2 : "") + "]";
        } else if (n2 === 0) { did += "分解[无符合件]"; }
      }
      if (A.evClean < 3) {
        A.evClean++;
        log("[sd] 清理 #" + A.evClean + " 装备 " + nodes.length + " 件 | 开关 出售=" + (wantSell ? "开" : "关")
            + " 分解=" + (wantDeco ? "开" : "关") + " | 品质 出售[" + sellQ.join(",") + "] 分解[" + decoQ.join(",")
            + "] | 结果:" + (did || "无动作"));
      }
      return !!did;
    } catch (e) { err("sell/deco", e); return false; }
  }

  /* ==================================================== (5) 自动穿戴 ======
   * 2026-10-25（res 1.1.9.1 反查）：旧实现 equip.ftA / equip.fvH 在这一版不存在
   * （ftA 是 UI 控件名，不是 API）。这一版走官方「装备」按钮 dHi 的三段口径：
   *   equip.fCV(node) -> {code,errMsg}     穿戴资格（0 = 可穿；1 职业不匹配 / 2 等级不足）
   *   equip.fCW(node) -> boolean           是否比当前同部位更强（或可便捷继承）
   *   equip.fFu(itemUid) -> promise{code}  穿戴提交
   * 逐个串行、一次最多 6 件；缺失 API 一律留一条 miss 日志，不静默。
   */
  function wearBest() {
    try {
      var z = F();
      if (!z) return false;
      var eq = z.equip, bag = z.bag;
      if (!eq) { miss("wear.eq", "自动穿戴: $eZ.equip 不存在 —— 该功能不会生效"); return false; }
      if (typeof eq.fFu !== "function") { miss("wear.fFu", "自动穿戴: equip.fFu 不是函数（本版改名了）—— 该功能不会生效"); return false; }
      if (!bag) { miss("wear.bag", "自动穿戴: $eZ.bag 不存在"); return false; }
      var nodes = equipNodes(bag);
      var cands = [];
      for (var i = 0; i < nodes.length; i++) {
        var n = nodes[i];
        if (!n || !n.itemUid) continue;
        try { if (typeof bag.bYB === "function" && bag.bYB(n.itemUid, WORE_BAG)) continue; } catch (e) {}
        if (n.equip && n.equip.isLock) continue;          /* 上锁 = 玩家想留着 */
        var ok = true;
        if (typeof eq.fCV === "function") {
          var chk = null;
          try { chk = eq.fCV(n); } catch (e) {}
          if (!chk || chk.code !== 0) ok = false;
        }
        if (ok && typeof eq.fCW === "function") {
          try { if (!eq.fCW(n)) ok = false; } catch (e) {}
        }
        if (ok) cands.push(n);
      }
      if (!cands.length) {
        A.wearEmptyN++;
        if (A.evWear < 3) { A.evWear++; log("穿戴: 当前没有更优装备（装备 " + nodes.length + " 件，候选 0）"); }
        return false;
      }
      var lim = Math.min(cands.length, 6);
      if (A.evWear < 3) { A.evWear++; log("穿戴: 待换 " + cands.length + " 件，先提交 " + lim + " 件 -> equip.fFu"); }
      var idx = 0;
      var step = function () {
        if (idx >= lim) return;
        var n = cands[idx++];
        var pr = null;
        try { pr = eq.fFu(n.itemUid); } catch (e) { err("wear/fu", e); setTimeout(step, 300); return; }
        A.wearN++;
        if (!pr || !pr.then) { A.wearOkN++; setTimeout(step, 300); return; }
        pr.then(function (res) {
          var code = res && res.code;
          if (code === 0 || code === undefined) {
            A.wearOkN++;
            if ((A.evWearOk | 0) < 3) { A.evWearOk = (A.evWearOk | 0) + 1; log("穿戴: 成功 itemUid=" + n.itemUid); }
          } else {
            A.wearLastCode = code;
            err("wear/code", "code=" + code + " itemUid=" + n.itemUid);
          }
          setTimeout(step, 350);
        }, function () { err("wear/reject", "promise rejected"); setTimeout(step, 350); });
      };
      step();
      return true;
    } catch (e) { err("wear", e); return false; }
  }

  /* =============================================== 驱动 / 结算面板时机 =====
   * 2026-10-25：旧的「战斗→城镇」latch 换成**结算面板打开那一刻**——和游戏自己的
   * 自动出售/分解同一个时机（AdventureFinishPanel.wA，game@52119）。
   * 时序照卡密包：面板打开 +0.4s 出售/分解 → +1.6s 穿戴 → +6.5s 重复挑战兜底。
   * 面板实例来源：fguiMgr.cUU.bdG(name) -> {cWn: fgui 组件}（1.1.9.1 已核）。
   */
  var RESULT_PANELS = ["AdventureFinishPanel", "AdventureResultPanel", "AdventureSpeResultPanel"];
  function panelEntry(name) {
    try {
      var fgm = F() && F().fguiMgr;
      if (!fgm) return null;
      if (!fgm.cUU || typeof fgm.cUU.bdG !== "function") {
        miss("panel.api", "结算面板: fguiMgr.cUU.bdG 不可用 —— 拿不到结算时机");
        return null;
      }
      return fgm.cUU.bdG(name) || null;
    } catch (e) { return null; }
  }
  function panelTick() {
    var inst = null, name = "";
    for (var i = 0; i < RESULT_PANELS.length; i++) {
      var e = panelEntry(RESULT_PANELS[i]);
      var c = e && e.cWn;
      /* cXg 是面板状态（main/index.js $to：0 初始 / 1 打开中 / 2 已打开 / 3 关闭中 / 4 关闭）。
         只认 1/2：关闭后面包仍可能留在 cUU 里，拿旧实例会在错时机发协议。 */
      var st = c ? (c.cXg | 0) : -1;
      if (c && (st === 1 || st === 2)) { inst = c; name = RESULT_PANELS[i]; break; }
    }
    if (!inst) {
      if (A.finInst) { A.finInst = null; A.finName = ""; A.finToken = (A.finToken | 0) + 1; }
      return;
    }
    if (A.finInst === inst) return;
    A.finInst = inst; A.finName = name;
    onResultPanel(inst, name);
  }
  function onResultPanel(inst, name) {
    var token = A.finToken = (A.finToken | 0) + 1;
    A.finN = (A.finN | 0) + 1;
    if ((A.evFin | 0) < 4) {
      A.evFin = (A.evFin | 0) + 1;
      var gold = null, c1 = null, ba = null, bn = null;
      try { var mc = F().monCard; gold = mc && mc.fNc && typeof mc.fNc.fMR === "function" ? mc.fNc.fMR("gold") : "?"; } catch (e) {}
      try { c1 = repeatRead(F().localMgr); } catch (e) {}
      try { ba = inst.btn_again ? !!inst.btn_again.visible : "无"; } catch (e) {}
      try { bn = inst.btn_toNext ? !!inst.btn_toNext.visible : "无"; } catch (e) {}
      log("[fin] " + name + " #" + A.finN + " 打开 | 自动挑战位=" + c1 + " 金卡=" + gold
          + " btn_again=" + ba + " btn_toNext=" + bn
          + " | 自动化 重复=" + FEAT(7) + " 出售=" + FEAT(8) + " 分解=" + FEAT(9) + " 穿戴=" + FEAT(10));
    }
    setTimeout(function () {
      if (token !== A.finToken) return;
      if (!FEAT(8) && !FEAT(9)) return;
      try { cleanupSellDeco(); } catch (e) { err("clean", e); }
    }, 400);
    setTimeout(function () {
      if (token !== A.finToken) return;
      if (!FEAT(10)) return;
      try { wearBest(); } catch (e) { err("wear", e); }
    }, 1600);
    setTimeout(function () {
      if (token !== A.finToken) return;
      if (!FEAT(7)) return;
      if (name !== "AdventureFinishPanel") return;       /* 只有这张面板带「再次挑战」 */
      var e2 = panelEntry(name), cur = e2 && e2.cWn;
      if (!cur || cur !== inst) return;                  /* 面板已关/已换 => 游戏自己走了 */
      var cst = cur.cXg | 0;
      if (cst !== 1 && cst !== 2) return;                /* 已进入关闭中/关闭 => 别补点 */
      if (typeof inst.ekx !== "function") {
        miss("repeat.ekx", "自动重复挑战: 结算面板没有 ekx（再次挑战）方法 —— 兜底点击不可用");
        return;
      }
      try {
        inst.ekx();
        A.repeatClickN++;
        if (A.evRepeat < 3) {
          A.evRepeat++;
          log("repeat: 游戏自带重复 6.5s 未触发 -> 兜底点击「再次挑战」ekx() (第 " + A.repeatClickN + " 次)");
        }
      } catch (e) { err("repeat/click", e); }
    }, 6500);
  }

  function tick() {
    A.tickN++;
    try { tryGoldCard(); } catch (e) { err("gold/tick", e); }
    /* 自动战斗（解锁游戏内按钮）：挂在 $eZ.buff.bZL.bZF(AUTO_BATTLE) 与
       monCard.fNc.fMY()/funcOpen.chW(MONTH_CARD_OPEN) 上。
       【事故复盘 2026-10-25】这个调用点原来藏在旧的 repeatTick() 里，我把 repeat 段
       整段换成官方设置位后它跟着消失 —— 菜单开关照样显示开、但钩子一次都没挂上。
       驱动里必须显式调用，别再依赖别的功能顺带。 */
    try { autoBattleTick(); } catch (e) { err("auto/tick", e); }
    try { repeatFlagSync(); } catch (e) {}
    try { panelTick(); } catch (e) { err("panel/tick", e); }
    try { tcSweep(); } catch (e) { err("tc", e); }
    /* 两张一次性快照，便于真机上区分「没进战斗」和「进了但没触发」 */
    if (A.tickN === 15 || A.tickN === 60) {
      try { log("state@" + A.tickN + "s " + JSON.stringify(window.__AUTO_STATE__())); } catch (e) {}
    }
  }

  /* 可观测口（对齐卡密包的 __WX_AUTO_STATE__，字段换成我们自己的） */
  window.__AUTO_STATE__ = function () {
    return {
      v: A.__v,
      gold: A.goldHooked, goldKey: A.goldKey, goldN: A.goldAllowN,
      repeatFlag: A.repeatFlagSync, repeatClicks: A.repeatClickN, repeatOn: A.repeatLast,
      panels: A.finN | 0, panelName: A.finName || "",
      sell: A.sellN, deco: A.decoN, sellQ: A.lastSellQ, decoQ: A.lastDecoQ, equipN: A.sdTotal | 0,
      wear: A.wearN, wearOk: A.wearOkN, wearEmpty: A.wearEmptyN, wearCode: A.wearLastCode,
      tcPatched: TC.patched, tcFlips: TC.flipN, tcCode: TC.flipCode,
      ticks: A.tickN, errs: A.errN, lastErr: A.lastErr, warns: A.warnN
    };
  };

  /* 自重排 setTimeout（不用 setInterval：主补丁也只信 setTimeout，见 ac_layer.js:424 同理由）。
   * 本层需要【常驻】驱动，不像过检测层挂完钩子就能停。 */
  if (!A.loopStarted) {
    A.loopStarted = true;
    var loop = function () {
      try { tick(); } catch (e) { err("tick", e); }
      try { setTimeout(loop, 1000); } catch (e) {}
    };
    try { setTimeout(loop, 1000); } catch (e) {}
  }

  window.__starry_auto = A;
  log("auto layer v3 loaded (gold / auto-repeat(设置位 cod) / auto-sell(bZw) / auto-decompose(bZv) / auto-wear(fFu); 开关读 MOD.feature6..10)");
})();
