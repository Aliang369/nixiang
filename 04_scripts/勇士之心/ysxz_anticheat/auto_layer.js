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
  var AUTO_VER = 5;   /* 5 = 2026-10-26：res 1.1.10.0 短名换语义 ⇒ 全部改明文锚点/结构指纹发现，
                       *     面板时机改吃 baseline 的 window.__starry_fin 事件；出售/分解/穿戴改用
                       *     游戏自己的「一键」口（bag 按品质批量、equip 一键穿戴）。 */
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
  /* 发现缓存（1.1.10.0）：三个 API 表都是“第一次用的时候在真实对象上按指纹找”，不写死短名 */
  A.lmApi = null; A.bagApi = null; A.eqApi = null; A.finSeen = 0;

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

  /* ---------------------------------------------- 1.1.10.0 发现层（不写死短名）--
   * res 1.1.10.0 把本层依赖的短名整体**换了语义**（不是改名）：
   *   bag.bZw / bag.bZv      -> 变成无参的“背包/红点检查”（不发协议）
   *   localMgr.coe / cof     -> 变成音效/音乐音量；localMgr.cod -> 城镇显示人数
   *   equip.fFu              -> 变成“宝石镶嵌”（fCV/fCW 同理换了靶子）
   *   fguiMgr.cUU.bdG        -> 整条“按名取面板实例”链消失
   * 所以本层不再写死任何短名，一律用【明文锚点 + 结构指纹】在真实对象上找：
   *   存档字段名 autoBattleFlag / autoSellEquipFlag / autoDecomposeEquipFlag（混淆器不动）；
   *   协议负载明文 num: / gridList / items:；UI 子节点名 btn_again / btn_toNext。
   * 找不到就 miss() 一条日志，绝不静默。 */
  function srcOf(f) { try { return Function.prototype.toString.call(f); } catch (e) { return ""; } }
  function findFnOn(obj, arity, test) {
    if (!obj) return null;
    for (var k in obj) {
      var v;
      try { v = obj[k]; } catch (e) { continue; }
      if (typeof v !== "function") continue;
      if (arity !== null && (v.length | 0) !== arity) continue;
      var s = srcOf(v);
      if (test(s)) return k;
    }
    return null;
  }
  /* localMgr 的自动开关口（1.1.10.0 实测语义，均按存档字段名柄指纹）：
   *   1 参 + autoBattleFlag           = 读重复挑战位
   *   2 参 + autoBattleFlag           = 写重复挑战位(idx,on)
   *   1 参 + autoSellEquipFlag        = 读自动出售品质位(q)
   *   1 参 + autoDecomposeEquipFlag   = 读自动分解品质位(q)
   *   2 参 + 两个 flag 同时出现         = 整体写两套品质位( sellArr, decoArr ) */
  function LM() {
    var z = F(), lm = z && z.localMgr;
    if (!lm) return null;
    if (!A.lmApi) {
      var d = {};
      d.repRead = findFnOn(lm, 1, function (s) { return s.indexOf("autoBattleFlag") >= 0; });
      d.repWrite = findFnOn(lm, 2, function (s) { return s.indexOf("autoBattleFlag") >= 0; });
      d.sellRead = findFnOn(lm, 1, function (s) { return s.indexOf("autoSellEquipFlag") >= 0; });
      d.decoRead = findFnOn(lm, 1, function (s) { return s.indexOf("autoDecomposeEquipFlag") >= 0; });
      d.qualWrite = findFnOn(lm, 2, function (s) {
        return s.indexOf("autoSellEquipFlag") >= 0 && s.indexOf("autoDecomposeEquipFlag") >= 0;
      });
      A.lmApi = d;
      log("localMgr api: repeatRead=" + d.repRead + " repeatWrite=" + d.repWrite +
          " sellRead=" + d.sellRead + " decoRead=" + d.decoRead + " qualWrite=" + d.qualWrite);
    }
    return A.lmApi;
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
  var REPEAT_IDX = 1;          /* 结算面板 wA 读的同一位（$OY.b = 1） */
  function repeatRead(lm) {
    var api = LM();
    try { if (api && api.repRead) return !!lm[api.repRead](REPEAT_IDX); } catch (e) {}
    return null;
  }
  function repeatFlagSync() {
    var on = FEAT(7);
    try {
      var z = F(), lm = z && z.localMgr;
      if (!lm) return;                                  /* $eZ 还没加载：不算缺失，别误报 */
      var api = LM();
      if (!api || !api.repWrite) {
        if (on) miss("rflag", "自动重复挑战: localMgr 上找不到 autoBattleFlag 写入口 —— 该功能不会生效");
        return;
      }
      /* 写入口会触发存档，只在读取值与开关不一致时写一次 */
      var cur = repeatRead(lm);
      if (cur === on) { A.repeatLast = on; return; }
      lm[api.repWrite](REPEAT_IDX, on);
      var now = repeatRead(lm);
      A.repeatLast = now;
      A.repeatFlagSync++;
      if (A.repeatFlagSync <= 3) {
        log("repeat: autoBattleFlag 位 " + REPEAT_IDX + " 置" + (on ? 1 : 0) + " -> 读回=" + now);
      }
    } catch (e) { err("repeat/flag", e); }
  }

  /* ============================================ (3)(4) 自动出售 / 自动分解 ===
   * 2026-10-26（res 1.1.10.0 反查）：游戏自己在 AdventureFinishPanel.wA 里就是这么做的——
   *   x=[COMMON,SENIOR,RARE]; N=x.filter(q=>localMgr.<读 autoSellEquipFlag 位>(q));
   *   N.length && $eZ.bag.<按品质批量出售>(N);
   *   L=x.filter(q=>localMgr.<读 autoDecomposeEquipFlag 位>(q)); L.length && $eZ.bag.<按品质批量分解>(L);
   * 所以只需（a）按存档字段名找到那两个「按品质批量口」，（b）品质位从 autoSellEquipFlag /
   * autoDecomposeEquipFlag 读，游戏内一个都没勾时用默认 [1,2,3]（同卡密包 v33N）。
   * 1.1.9.1 时代的 bZw/bZv 在 1.1.10.0 已经变成无参的背包检查函数，直接调等于空转。 */
  var QUAL_DEF = [1, 2, 3];        /* COMMON / SENIOR / RARE */
  function BAGAPIS() {
    var z = F(), bag = z && z.bag;
    if (!bag) return null;
    if (!A.bagApi) {
      var d = {};
      /* 出售：单参、内部拼 {gridId, num: amount} 并走 pg 协议，还带 quality 过滤 */
      d.sell = findFnOn(bag, 1, function (s) {
        return s.indexOf("num:") >= 0 && s.indexOf(".pg(") >= 0 && s.indexOf("quality") >= 0;
      });
      /* 分解：单参、内部拼 gridList，自带 isLock + 可分解过滤 */
      d.deco = findFnOn(bag, 1, function (s) {
        return s.indexOf("gridList") >= 0 && s.indexOf("quality") >= 0 && s.indexOf("isLock") >= 0;
      });
      A.bagApi = d;
      log("bag api: sellByQuality=" + d.sell + " decomposeByQuality=" + d.deco);
    }
    return A.bagApi;
  }
  function qualitiesOf(lm, api, which) {
    var out = [];
    var fn = api ? (which === "sell" ? api.sellRead : api.decoRead) : null;
    if (!fn) return out;
    for (var q = 1; q <= 6; q++) { try { if (lm[fn](q)) out.push(q); } catch (e) {} }
    return out;
  }
  function cleanupSellDeco() {
    var wantSell = FEAT(8), wantDeco = FEAT(9);
    if (!wantSell && !wantDeco) return false;
    try {
      var z = F(), bag = z && z.bag, lm = z && z.localMgr;
      if (!bag) { miss("sd.bag", "出售/分解: $eZ.bag 不存在 —— 该功能不会生效"); return false; }
      var api = BAGAPIS(), lma = LM();
      var did = "", sellQ = [], decoQ = [];
      if (wantSell) {
        if (!api || !api.sell) miss("sell.api", "自动出售: bag 上找不到「按品质批量出售」口 —— 该功能不会生效");
        else {
          sellQ = lma ? qualitiesOf(lm, lma, "sell") : [];
          if (!sellQ.length) {
            sellQ = QUAL_DEF.slice();
            if (!A.sellDef) { A.sellDef = 1; log("[sd] 游戏内未勾选出售品质 -> 用默认 [1,2,3]"); }
          }
          try { bag[api.sell](sellQ); A.sellN++; A.lastSellQ = sellQ.join(","); did += "出售[" + A.lastSellQ + "] "; }
          catch (e) { err("sell", e); }
        }
      }
      if (wantDeco) {
        if (!api || !api.deco) miss("deco.api", "自动分解: bag 上找不到「按品质批量分解」口 —— 该功能不会生效");
        else {
          decoQ = lma ? qualitiesOf(lm, lma, "deco") : [];
          if (!decoQ.length) {
            decoQ = QUAL_DEF.slice();
            if (!A.decoDef) { A.decoDef = 1; log("[sd] 游戏内未勾选分解品质 -> 用默认 [1,2,3]"); }
          }
          try { bag[api.deco](decoQ); A.decoN++; A.lastDecoQ = decoQ.join(","); did += "分解[" + A.lastDecoQ + "]"; }
          catch (e) { err("deco", e); }
        }
      }
      if (A.evClean < 3) {
        A.evClean++;
        log("[sd] 清理 #" + A.evClean + " | 开关 出售=" + (wantSell ? "开" : "关") + " 分解=" + (wantDeco ? "开" : "关")
            + " | 品质 出售[" + sellQ.join(",") + "] 分解[" + decoQ.join(",") + "] | 结果:" + (did || "无动作"));
      }
      return !!did;
    } catch (e) { err("sell/deco", e); return false; }
  }

  /* ==================================================== (5) 自动穿戴 ======
   * 2026-10-26（res 1.1.10.0 反查）：1.1.9.1 的 equip.fCV/fCW/fFu 在这一版**换了靶子**
   * （fFu 现在是“宝石镶嵌”，体里是 gemInfoCfg / “当前能量已满”；fCV 是强化材料适用性）
   * ⇒ 旧实现只会报“不是函数/永远不会成功”。这一版直接用游戏自己的「一键穿戴」两口：
   *   equip.<候选列表>()   -> [itemUid,...]   更优且可穿的装备（内部按部位比强度、排已穿）
   *   equip.<批量穿戴>(uids) -> promise<件数>  内部按 10 件一批发协议
   * 指纹：候选 = 0 参 + 明文 WORE_EQUIP + new Map() 且不含 SOUL（那是魂魄那一路）；
   *       批量 = 1 参 + 明文 items: + .slice( + “+ 10”（分片）。
   * 缺失 API 一律留一条 miss 日志，不静默。
   */
  function EQAPI() {
    var z = F(), eq = z && z.equip;
    if (!eq) return null;
    if (!A.eqApi) {
      var d = {};
      /* itemUid 是协议负载明文；SOUL 那条是“一键穿戴魂魄”，不能算进来 */
      d.cands = findFnOn(eq, 0, function (s) {
        return s.indexOf("WORE_EQUIP") >= 0 && s.indexOf("new Map()") >= 0 &&
               s.indexOf("itemUid") >= 0 && s.indexOf("SOUL") < 0;
      });
      d.wear = findFnOn(eq, 1, function (s) {
        return s.indexOf("items:") >= 0 && s.indexOf(".slice(") >= 0 && s.indexOf("+ 10") >= 0;
      });
      A.eqApi = d;
      log("equip api: candidates=" + d.cands + " wearAll=" + d.wear);
    }
    return A.eqApi;
  }
  function wearBest() {
    try {
      var z = F();
      if (!z) return false;
      var eq = z.equip;
      if (!eq) { miss("wear.eq", "自动穿戴: $eZ.equip 不存在 —— 该功能不会生效"); return false; }
      var api = EQAPI();
      if (!api || !api.cands || !api.wear) {
        miss("wear.api", "自动穿戴: equip 上找不到「一键穿戴」口 —— 该功能不会生效");
        return false;
      }
      var uids = null;
      try { uids = eq[api.cands]() || []; } catch (eC) { err("wear/cands", eC); return false; }
      if (!uids.length) {
        A.wearEmptyN++;
        if (A.evWear < 3) { A.evWear++; log("穿戴: 当前没有更优装备"); }
        return false;
      }
      if (A.evWear < 3) { A.evWear++; log("穿戴: 待换 " + uids.length + " 件 -> 一键穿戴"); }
      A.wearN++;
      var pr = null;
      try { pr = eq[api.wear](uids); } catch (e) { err("wear/submit", e); return false; }
      var okN = function (n) {
        A.wearOkN++;
        if ((A.evWearOk | 0) < 3) { A.evWearOk = (A.evWearOk | 0) + 1; log("穿戴: 成功 " + (n | 0) + " 件"); }
      };
      if (pr && pr.then) pr.then(function (n) { okN(n); }, function () { err("wear/reject", "promise rejected"); });
      else okN(uids.length);
      return true;
    } catch (e) { err("wear", e); return false; }
  }

  /* =============================================== 驱动 / 结算面板时机 =====
   * 2026-10-26（res 1.1.10.0）：旧的 fguiMgr.cUU.bdG(name) 查询链整条消失（cUU 已改作他用），
   * 实测表现 = panels 恒 0、自动出售/分解/穿戴永不触发。改吃 baseline 的事件：
   *   baseline 钩住面板自己的打开方法（指纹 btnList/btn_again/btn_toNext），
   *   命中后写 window.__starry_fin = {n, t, inst, again}；本层每秒读一次序号变化。
   * 时序照卡密包：面板打开 +0.4s 出售/分解 → +1.6s 穿戴 → +6.5s 重复挑战兜底。
   */
  function panelTick() {
    var fin = null;
    try { fin = window.__starry_fin; } catch (e) {}
    if (!fin || !fin.inst) {
      if (FEAT(7) || FEAT(8) || FEAT(9) || FEAT(10)) {
        miss("panel.hook", "结算面板时机: baseline 没钩到面板打开方法（btnList/btn_again/btn_toNext）—— 自动化四项都不会触发");
      }
      return;
    }
    if (A.finSeen === fin.n) return;
    A.finSeen = fin.n;
    A.finInst = fin.inst;
    onResultPanel(fin.inst, fin.again || "");
  }
  function onResultPanel(inst, againKey) {
    var token = A.finToken = (A.finToken | 0) + 1;
    A.finN = (A.finN | 0) + 1;
    if ((A.evFin | 0) < 4) {
      A.evFin = (A.evFin | 0) + 1;
      var c1 = null, ba = null, bn = null;
      try { c1 = repeatRead(F().localMgr); } catch (e) {}
      try { ba = inst && inst.btn_again ? !!inst.btn_again.visible : "无"; } catch (e) {}
      try { bn = inst && inst.btn_toNext ? !!inst.btn_toNext.visible : "无"; } catch (e) {}
      log("[fin] #" + A.finN + " 结算面板打开 | 自动挑战位=" + c1 + " btn_again=" + ba
          + " btn_toNext=" + bn + " againApi=" + (againKey || "无")
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
      if (!againKey) {
        miss("repeat.api", "自动重复挑战: 结算面板上没有「再次挑战」倒计时方法 —— 兜底点击不可用（游戏自带位见 repeat 日志）");
        return;
      }
      try {
        inst[againKey]();
        A.repeatClickN++;
        if (A.evRepeat < 3) {
          A.evRepeat++;
          log("repeat: 游戏自带重复 6.5s 未触发 -> 兜底点一次「再次挑战」(" + againKey + ") 第 " + A.repeatClickN + " 次");
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
      panels: A.finN | 0,
      sell: A.sellN, deco: A.decoN, sellQ: A.lastSellQ, decoQ: A.lastDecoQ,
      api: { lm: A.lmApi || 0, bag: A.bagApi || 0, eq: A.eqApi || 0 },
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
  log("auto layer v" + AUTO_VER + " loaded (gold / repeat(autoBattleFlag 位) / sell+decompose(bag 按品质批量口) / wear(equip 一键穿戴) / 面板时机=window.__starry_fin; 开关读 MOD.feature6..10)");
})();
