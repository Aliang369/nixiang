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
  var AUTO_VER = 1;
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
    goldHooked: false, goldKey: "", goldAllowN: 0,
    /* (2) 自动重复挑战 */
    repeatFlagSync: 0, repeatClickN: 0,
    repeatToken: 0, repeatScheduled: null, repeatHandled: null,
    /* (3)(4) 出售 / 分解 */
    sellN: 0, decoN: 0, sellDef: 0, decoDef: 0, lastSellQ: "", lastDecoQ: "",
    /* (5) 穿戴 */
    wearN: 0, wearOkN: 0, wearEmptyN: 0,
    /* 驱动 / 节流 */
    tickN: 0, inBattleLatch: false, busy: false, lastCleanup: 0, battleN: 0,
    acTickSeen: 0, evGold: 0, evRepeat: 0, evClean: 0, evWear: 0,
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
   * 判 gold 时直接改返 true。我们用独立开关 feature6（他是 CFG.autoFight）。 */
  function tryGoldCard() {
    if (A.goldHooked) return true;
    if (!FEAT(6)) return false;
    try {
      var z = window.$eZ, data = z && z.monCard && z.monCard.fDn;
      if (!z) return false;                          /* 还没加载到 $eZ：不算缺失, 别误报 */
      if (!data) { miss("gold.data", "解锁金卡: 开关开着但 $eZ.monCard.fDn 不存在 —— 本版本改名了？该功能不会生效"); return false; }
      var proto = Object.getPrototypeOf(data);
      if (!proto || typeof proto.fDc !== "function") {
        miss("gold.fn", "解锁金卡: monCard.fDn 原型上的 fDc 不是函数 —— 本版本改名了？该功能不会生效");
        return false;
      }
      if (proto.fDc.__starryGold && (proto.fDc.__starryGoldVer | 0) >= AUTO_VER) {
        A.goldHooked = true; A.goldKey = "monCard.fDn.fDc";
        return true;
      }
      var orig = proto.fDc.__starryGoldOrig || proto.fDc;
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
      proto.fDc = w;
      A.goldHooked = true;
      A.goldKey = "monCard.fDn.fDc";
      log("gold-card hooked: $eZ.monCard.fDn.prototype.fDc (cardType===gold -> true, 受 feature6 控制)");
      return true;
    } catch (e) { err("gold", e); return false; }
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
  function cleanupSellDeco() {
    var wantSell = FEAT(8), wantDeco = FEAT(9);
    if (!wantSell && !wantDeco) return false;
    try {
      var z = F(), bag = z && z.bag, lm = z && z.localMgr;
      if (!bag) { miss("sd.bag", "出售/分解: $eZ.bag 不存在 —— 该功能不会生效"); return false; }
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
          try {
            bag.bYJ(decoQ);
            A.decoN++;
            A.lastDecoQ = decoQ.join(",");
            did += "分解[" + A.lastDecoQ + "]";
          } catch (e) { err("deco", e); }
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
    /* 两张一次性快照，便于真机上"根本没进战斗"和"进了但没触发"能区分开 */
    if (A.tickN === 15 || A.tickN === 60) {
      try { log("state@" + A.tickN + "s " + JSON.stringify(window.__AUTO_STATE__())); } catch (e) {}
    }
  }

  /* 可观测口（对齐卡密包的 __WX_AUTO_STATE__ L7033，字段换成我们自己的） */
  window.__AUTO_STATE__ = function () {
    return {
      v: A.__v,
      gold: A.goldHooked, goldKey: A.goldKey, goldN: A.goldAllowN,
      repeatFlag: A.repeatFlagSync, repeatClicks: A.repeatClickN,
      sell: A.sellN, deco: A.decoN, sellQ: A.lastSellQ, decoQ: A.lastDecoQ,
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
