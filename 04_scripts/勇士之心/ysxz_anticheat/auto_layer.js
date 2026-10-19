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
  var AUTO_VER = 2;   /* 2 = 2026-10-19 res 1.1.6.0：金卡结构化发现 + 出售/分解决定性诊断 */
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
  function repeatFlagSync() {
    var on = FEAT(7);
    try {
      var z = F();
      if (!z) return;                                /* 还没加载到 $eZ：不算缺失, 别误报 */
      var lm = z.localMgr;
      /* 不主动造 lm.cms（他是造的）；本版本没这个对象就整段跳过，只留面板那一路。
       * 告警只在【开关开着】时打：开关关着的时候缺 API 完全正常，不该刷日志。 */
      if (!lm || !lm.cms) {
        if (on) miss("rflag", "自动重复挑战: $eZ.localMgr.cms 不存在 —— 游戏内位标记那一路不可用（只剩面板点击那一路）");
        return;
      }
      var flag = Number(lm.cms.autoBattleFlag) || 0;
      var want = on ? (flag | (1 << 1)) : (flag & ~(1 << 1));
      if (want === flag) return;
      lm.cms.autoBattleFlag = want;
      A.repeatFlagSync++;
      if (A.repeatFlagSync <= 3) {
        log("repeat: 游戏内自动挑战位 " + (on ? "置1" : "清0") + " autoBattleFlag " + flag + " -> " + want);
      }
    } catch (e) { err("repeat/flag", e); }
  }
  function repeatTick() {
    if (!FEAT(7)) {
      if (A.repeatScheduled || A.repeatHandled) {
        A.repeatScheduled = null; A.repeatHandled = null;
        A.repeatToken = (A.repeatToken || 0) + 1;
      }
      return;
    }
    /* 先看管理器在不在：panelOpen() 在 fguiMgr 缺失时只会静默返回 false，
     * 那样"面板管理器改名了"就永远诊断不出来 —— 必须在这里先判一次。 */
    var fgm = F() && F().fguiMgr;
    if (!fgm) { miss("panel", "自动重复挑战: $eZ.fguiMgr 不存在 —— 找不到面板，面板点击那一路不会生效"); return; }
    if (!panelOpen("AdventureFinishPanel")) {
      if (A.repeatScheduled || A.repeatHandled) {
        A.repeatScheduled = null; A.repeatHandled = null;
        A.repeatToken = (A.repeatToken || 0) + 1;
      }
      return;
    }
    var cands = panelCandidates(fgm, "AdventureFinishPanel"), ctrl = null;
    for (var i = 0; i < cands.length; i++) {
      if (typeof cands[i].efi === "function") { ctrl = cands[i]; break; }
    }
    if (!ctrl || A.repeatScheduled === ctrl || A.repeatHandled === ctrl) return;
    A.repeatScheduled = ctrl;
    var token = A.repeatToken = (A.repeatToken || 0) + 1;
    setTimeout(function () {
      try {
        if (token !== A.repeatToken) return;
        if (A.repeatScheduled === ctrl) A.repeatScheduled = null;
        if (!FEAT(7) || !panelOpen("AdventureFinishPanel")) return;
        var fgm2 = F() && F().fguiMgr;
        if (!fgm2) return;
        var now = panelCandidates(fgm2, "AdventureFinishPanel"), cur = null;
        for (var j = 0; j < now.length; j++) {
          if (typeof now[j].efi === "function") { cur = now[j]; break; }
        }
        if (!cur || cur !== ctrl) return;      /* 控件换实例了 => 面板重建过，放弃本次 */
        A.repeatScheduled = null;
        A.repeatHandled = cur;
        cur.efi();
        A.repeatClickN++;
        if (A.evRepeat < 3) { A.evRepeat++; log("repeat: 点击「再次挑战」 efi() (第 " + A.repeatClickN + " 次)"); }
      } catch (e) { err("repeat/click", e); }
    }, 1500);
  }

  /* ============================================ (3)(4) 出售 / 分解 ========= */
  /* 他的 _sdQualitiesOf L4658-4687 是「游戏内位标记 ∪ UI 勾选」的并集。
   * 我们去掉 UI 勾选那一半（没有那个勾选框），只读游戏内的 lm.cnp / lm.cnq。 */
  function qualitiesOf(lm, useSell) {
    var set = {};
    try {
      var fn = useSell ? lm.cnp : lm.cnq;
      if (typeof fn === "function") {
        for (var q = 1; q <= 6; q++) { try { if (fn.call(lm, q)) set[q] = 1; } catch (e) {} }
      }
    } catch (e) {}
    return Object.keys(set).map(Number).filter(function (x) { return x >= 1 && x <= 6; })
                 .sort(function (a, b) { return a - b; });
  }
  /* ---- 出售/分解：容器与方法解析 + 一次性诊断（2026-10-19 加） ----
   * 排查纪律：上一版「开关开着、什么都不发、也没有一行日志」正是最难查的形态。
   * 现在把四种可能各留一行有界日志，一次真机回城就能定案：
   *   ① 根本没触发（没进过战斗 / 回城那一拍没赶上）
   *   ② 触发了但装备容器读不到（bXk 改名或数据未加载 → 回退 bYn）
   *   ③ 读到了但 0 件可分解（EquipTable.decompose 为空 = 卡密包作者记录的
   *      「分解要 50 级」根因；也可能全被 isLock 锁住）
   *   ④ 协议发了但服务端拒（pk/pf 的 promise code）
   * EQUIP=200 是 main 包 TblEnum.BagTableEnum.EQUIP 的硬编号，1.1.4.0/1.1.6.0 实测一致。 */
  var EQUIP_BAG = 200;
  function bagContainer(bag) {
    var order = ["bXk", "bYn"];        /* 卡密包 _sdDump 的同一回落顺序 */
    for (var i = 0; i < order.length; i++) {
      var fn = bag[order[i]];
      if (typeof fn !== "function") continue;
      try { var c = fn.call(bag, EQUIP_BAG); if (c && c.d) return { c: c, via: order[i] }; } catch (e) {}
    }
    return null;
  }
  /* 统计一次装备容器：总数 / 锁定 / 品质分布 / 可分解数 */
  function sdCount(bag) {
    var r = { ok: false, via: "", total: 0, locked: 0, decoable: 0, q: {}, qfail: 0 };
    var got = bagContainer(bag);
    if (!got) return r;
    r.ok = true; r.via = got.via;
    var box = got.c.d;
    try {
      var walk = typeof box.bdn === "function" ? box.bdn : (typeof box.bdm === "function" ? box.bdm : null);
      if (!walk) return r;
      walk.call(box, function (node) {
        r.total++;
        var isLock = !!(node && node.equip && node.equip.isLock);
        if (isLock) { r.locked++; return; }
        var cfg = null;
        try { cfg = bag.bXh(node.itemId); } catch (e) {}
        if (!cfg || cfg.quality === undefined) { r.qfail++; return; }
        var q = cfg.quality | 0;
        r.q[q] = (r.q[q] || 0) + 1;
        try { if (typeof bag.bXY === "function" && bag.bXY(node)) r.decoable++; } catch (e2) {}
      });
    } catch (e) {}
    return r;
  }
  function qDist(q) {
    var out = [];
    for (var k in q) { if (Object.prototype.hasOwnProperty.call(q, k)) out.push(k + ":" + q[k]); }
    out.sort();
    return out.join(" ");
  }
  function sdDump(bag) {
    /* 定稿条件 = 【真读到件数】。登录/选角阶段 bag 容器存在但 0 件，
     * 那时若把 0 件当结论缓存下来，进游戏后就永远"跳过:0 件可分解" —— 自己把功能锁死。
     * 所以 total=0 只打一条"还在等"，不进定稿。 */
    if (A.sdFinal) return A.sdLast;
    var st = sdCount(bag);
    if (!st.ok) {
      /* 容器读不到 ≠ 结论：可能是 bag 数据还没加载。只留一条日志，下次调用再试。 */
      A.sdLast = st;
      miss("sd.cont", "出售/分解: 读不到装备容器（bXk/bYn 都没有 .d）—— 这两个也改名了？该功能不会生效");
      return st;
    }
    A.sdLast = st;
    A.sdTotal = st.total; A.sdLocked = st.locked; A.sdDecoable = st.decoable;
    if (st.total === 0) {
      A.sdDump = "wait";
      if (!A.sdEmptyLogged) {
        A.sdEmptyLogged = 1;
        log("[sd] 装备容器@" + st.via + " 还是空的（角色/背包数据未加载）—— 先不定稿，后续 tick 再扫");
      }
      return st;
    }
    A.sdDump = "ok";
    A.sdFinal = 1;          /* 定稿：已读到真实件数，后续不再重扫（sdLocked 是"锁定件数"，别混用） */
    log("[sd] 装备容器@" + st.via + " 共 " + st.total + " 件 | 锁定 " + st.locked + " | 可取品质失败 " + st.qfail
        + " | 可分解 " + st.decoable + " | 品质分布[" + (qDist(st.q) || "空") + "]"
        + " | api bYJ=" + (typeof bag.bYJ) + " bYK=" + (typeof bag.bYK) + " bXY=" + (typeof bag.bXY));
    if (st.decoable === 0) {
      log("[sd] 判定: 容器里有装备但【没有可分解件】—— EquipTable.decompose 为空(低品质/低等级)或全部 isLock，"
          + "这不是钩子失效；分解协议不会发出。");
    }
    return st;
  }
  /* 协议可观测：包一层 pk(分解) / pf(出售)，把请求与 promise 结果留成日志（有界） */
  function sdHookProto(bag) {
    if (A.pkHooked) return;
    A.pkHooked = true;
    function wrap(name, tag) {
      var of = bag[name];
      if (typeof of !== "function") { miss("sd." + name, "出售/分解: bag." + name + " 不是函数（协议层改名了？）"); return; }
      var w = function () {
        var pr;
        A.protoSent++;
        try { pr = of.apply(this, arguments); }
        catch (e) { A.protoCode = tag + " throw"; err("proto/" + tag, e); throw e; }
        try {
          if (pr && pr.then) {
            pr.then(function (r) {
              var code = r && r.code;
              A.protoCode = tag + " code=" + code;
              if (A.evProto < 4) { A.evProto++; log("[sd] " + tag + " 协议返回 code=" + code); }
            }, function (e2) {
              A.protoCode = tag + " reject";
              if (A.evProto < 4) { A.evProto++; log("[sd] " + tag + " 协议 rejected: " + msgOf(e2)); }
            });
          }
        } catch (e3) {}
        return pr;
      };
      w.__starryProto = tag;
      bag[name] = w;
    }
    wrap("pk", "分解");
    wrap("pf", "出售");
  }
  function cleanupSellDeco() {
    var wantSell = FEAT(8), wantDeco = FEAT(9);
    if (!wantSell && !wantDeco) return false;
    try {
      var z = F(), bag = z && z.bag, lm = z && z.localMgr;
      if (!bag) { miss("sd.bag", "出售/分解: $eZ.bag 不存在 —— 该功能不会生效"); return false; }
      sdHookProto(bag);
      var sellQ = wantSell ? qualitiesOf(lm, true) : [];
      var decoQ = wantDeco ? qualitiesOf(lm, false) : [];
      /* 兜底照抄他 v33N：开关开着却一个品质都没有 -> [1,2,3]，否则等于空转 */
      if (wantSell && !sellQ.length) {
        sellQ = [1, 2, 3];
        if (!A.sellDef) { A.sellDef = 1; log("[sd] 出售品质为空(游戏内未勾选) -> 兜底 [1,2,3]"); }
      }
      if (wantDeco && !decoQ.length) {
        decoQ = [1, 2, 3];
        if (!A.decoDef) { A.decoDef = 1; log("[sd] 分解品质为空(游戏内未勾选) -> 兜底 [1,2,3]"); }
      }
      var look = sdDump(bag);       /* 一次性：容器/件数/可分解数/api 类型 */
      var did = "";
      if (wantSell && sellQ.length) {
        if (typeof bag.bYK === "function") {
          try {
            bag.bYK(sellQ);
            A.sellN++;
            A.lastSellQ = sellQ.join(",");
            did += "出售[" + A.lastSellQ + "] ";
          } catch (e) { err("sell", e); }
        } else { err("sell/noapi", "bag.bYK 不可用"); }
      }
      if (wantDeco && decoQ.length) {
        if (typeof bag.bYJ === "function") {
          if (look && look.ok && look.decoable === 0) {
            did += "分解[跳过:0 件可分解]";
          } else {
            try {
              bag.bYJ(decoQ);
              A.decoN++;
              A.lastDecoQ = decoQ.join(",");
              did += "分解[" + A.lastDecoQ + "]";
            } catch (e) { err("deco", e); }
          }
        } else { err("deco/noapi", "bag.bYJ 不可用"); }
      }
      /* 每次清理打一行总账（有界） */
      if (A.evClean < 3) {
        A.evClean++;
        log("[sd] 清理 #" + A.evClean + " 开关 出售=" + (wantSell ? "开" : "关") + " 分解=" + (wantDeco ? "开" : "关")
            + " | 出售品质[" + sellQ.join(",") + "] 分解品质[" + decoQ.join(",") + "] | 结果:" + (did || "无动作"));
      }
      return !!did;
    } catch (e) { err("sell/deco", e); return false; }
  }

  /* ==================================================== (5) 自动穿戴 ====== */
  /* 他的 autoWearBestEquip L4580-4608：equip.ftA() 算待替换 itemUid 列表，
   * equip.fvH(list) 提交；code 15/3（网络未就绪）最多重试 3 次。 */
  function wearBest(retry) {
    retry = retry || 0;
    try {
      var eq = F() && F().equip;
      if (!eq) { miss("wear.eq", "自动穿戴: $eZ.equip 不存在 —— 该功能不会生效"); return false; }
      if (typeof eq.ftA !== "function") {
        miss("wear.ftA", "自动穿戴: equip.ftA 不是函数（本版本改名了？）—— 该功能不会生效");
        return false;
      }
      var list = [];
      try { list = eq.ftA() || []; }
      catch (e) { err("wear/ftA", e); }
      if (!list || !list.length) {
        A.wearEmptyN++;
        if (A.evWear < 3) { A.evWear++; log("穿戴: 当前已是最优装备（无需替换）"); }
        return false;
      }
      if (typeof eq.fvH !== "function") { err("wear/noapi", "equip.fvH 不可用"); return false; }
      if (A.evWear < 3) { A.evWear++; log("穿戴: 待替换 " + list.length + " 件 -> equip.fvH"); }
      var pr = null;
      try { pr = eq.fvH.call(eq, list); } catch (e) { err("wear/fvH", e); }
      A.wearN++;
      if (pr && pr.then) {
        pr.then(function (res) {
          var code = res && res.code;
          if (code === 0 || code === undefined) { A.wearOkN++; return; }
          if ((code === 15 || code === 3) && retry < 3) {
            log("穿戴: code=" + code + "(网络未就绪) -> " + (1500 + retry * 1000) + "ms 后重试(" + (retry + 1) + "/3)");
            setTimeout(function () { try { wearBest(retry + 1); } catch (e) {} }, 1500 + retry * 1000);
          } else { err("wear/code", "code=" + code); }
        }, function () { err("wear/reject", "promise rejected"); });
      } else { A.wearOkN++; }
      return true;
    } catch (e) { err("wear", e); return false; }
  }

  /* ==================================================== 驱动 / 节流 ======= */
  /* 照抄他的 autoPostCleanupTick L6800-6820：必须在【战斗中】记一笔，
   * 回到城镇那一拍才触发；两次触发至少间隔 5s；一次触发内错峰 900 / 2100ms。 */
  function cleanupTick() {
    if (!FEAT(8) && !FEAT(9) && !FEAT(10)) { A.inBattleLatch = false; return; }
    if (battleSignal()) {
      if (!A.inBattleLatch) { A.inBattleLatch = true; A.battleN++; }
      return;
    }
    if (!A.inBattleLatch) return;          /* 从没进过战斗 -> 不触发（他的原话：必须等已回城） */
    A.inBattleLatch = false;               /* 战斗 -> 城镇，触发这一拍 */
    if (A.busy) return;
    var now = Date.now();
    if (now - A.lastCleanup < 5000) return;
    A.lastCleanup = now;
    A.busy = true;
    try {
      setTimeout(function () { try { cleanupSellDeco(); } catch (e) {} }, 900);
      setTimeout(function () {
        try { if (FEAT(10)) wearBest(0); } catch (e) {}
        A.busy = false;
      }, 2100);
    } catch (e) { A.busy = false; err("cleanup/timer", e); }
  }

  function tick() {
    A.tickN++;
    try { tryGoldCard(); } catch (e) { err("gold/tick", e); }
    try { repeatFlagSync(); } catch (e) {}
    try { repeatTick(); } catch (e) {}
    try { cleanupTick(); } catch (e) {}
    try { tcSweep(); } catch (e) { err("tc", e); }
    /* 第 20 秒探一次装备容器（不发任何协议）：
     * 用户可能还没打一局，而「分解到底有没有可分解件」是静态就能答的问题。
     * 这样即使 cleanupTick 的"回城那一拍"没触发，日志里也有容器事实可判。 */
    if (A.tickN === 20 && (FEAT(8) || FEAT(9))) {
      try { var zp = F(); if (zp && zp.bag) { sdHookProto(zp.bag); sdDump(zp.bag); } } catch (e) {}
    }
    /* 两张一次性快照，便于真机上"根本没进战斗"和"进了但没触发"能区分开 */
    if (A.tickN === 15 || A.tickN === 60) {
      try { log("state@" + A.tickN + "s " + JSON.stringify(window.__AUTO_STATE__())); } catch (e) {}
    }
  }

  /* 可观测口（对齐卡密包的 __WX_AUTO_STATE__ L7033，字段换成我们自己的） */
  window.__AUTO_STATE__ = function () {
    return {
      v: A.__v,
      gold: A.goldHooked, goldKey: A.goldKey, goldHow: A.goldHow, goldN: A.goldAllowN,
      repeatFlag: A.repeatFlagSync, repeatClicks: A.repeatClickN,
      tcPatched: TC.patched, tcFlips: TC.flipN, tcCode: TC.flipCode,
      sell: A.sellN, deco: A.decoN, sellQ: A.lastSellQ, decoQ: A.lastDecoQ,
      sdDump: A.sdDump, sdTotal: A.sdTotal, sdLocked: A.sdLocked, sdDecoable: A.sdDecoable,
      protoSent: A.protoSent, protoCode: A.protoCode,
      wear: A.wearN, wearOk: A.wearOkN, wearEmpty: A.wearEmptyN,
      ticks: A.tickN, battles: A.battleN, inLatch: A.inBattleLatch, busy: A.busy,
      errs: A.errN, lastErr: A.lastErr, warns: A.warnN, note: A.note
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
  log("auto layer v1 loaded (gold / auto-repeat / auto-sell / auto-decompose / auto-wear; 开关读 MOD.feature6..10)");
})();
