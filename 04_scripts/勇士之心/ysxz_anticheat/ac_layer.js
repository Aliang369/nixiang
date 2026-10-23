/* ============================================================================
 * 星空过检测层 v1  —— 对照卡密包(勇士之心 1.0.3 渠道包 / res-1.1.3.2)四层实现复刻
 * 复刻来源：03_decompiled/勇士之心_cardkey_20261007/提取_补丁_v66_原始.js
 *   L6908 acGateMod / L6919 installAntiCheatGate    -> 本文件 (1) 闸门
 *   L7001 acHookSink / L7015 installReportIntegrity -> 本文件 (2) 出口挂钩
 *   L6975 acFixReport                              -> 本文件 (3) 结算字段观测/守卫
 *   L2578 HitBus.hookPrototype(proto.hY)           -> 本文件 (4) 伤害字段还原
 *   L7033 __WX_AC_STATE__                          -> 本文件 (6) 可观测
 *
 * ---------------------------------------------------------------- 字段账本 --
 * 全部行号为 06_dumped/勇士之心/hotupdate_20261005 实测(与他的版本同代 res-1.1.3.2)
 *
 * 字段                                | 谁写                                | 谁读(消费点)
 * ------------------------------------|-------------------------------------|--------------------------------
 * attacker.hw.finalDamage             | battleLogic hY: t.hw.finalDamage += e| game@4646511 pe.nP -> bET({damage}) -> sendMsg
 * victim.hw.finalHurtValue            | battleLogic hY: o.finalHurtValue += e| game@4646511 pe.nP -> bET({takenDamage}) -> sendMsg
 * (同两字段的 UI 消费)                 | 同上                                | game@5711602 -> 伤害面板 o.cZq / o.cZr
 * victim.hw.bhd (当前HP)              | battleLogic hY: bhd = max(bhd-a, lockMinHP) | 结算 bDK.e / 比率 bDK.d
 * victim.hw.bhe (HP上限)              | 同上                                |
 * victim.hw.deathCount                | 死亡流程                            | bET({deathCount})
 * payload.completeResult.useTime      | game@5716445: floor(1e3 * battleTime) | bER@4682536 -> sendMsg / gsMgr.cQA
 * battleTime(fk)                      | battleLogic update: fk += dt*hitPauseTimeScale*worldTimeScale*mainTimeScale
 *
 * 结论(与我们现有菜单功能对照)：
 *  - 我们【有】伤害倍率(1..50)与秒杀(x100)，两者共用 battleLogic .GD 伤害公式咽喉
 *    -> 会同时污染 hw.finalDamage 与 hw.finalHurtValue -> 必须补偿。(本文件第 4 节，已武装)
 *  - 我们【没有】任何游戏加速/时间缩放功能(菜单只有 秒杀/无敌/无限技能/移速/自动战斗/伤害倍率)
 *    -> battleTime 不被我们拉伸 -> useTime 无需补偿。(本文件第 3 节，只观测不修改，原因见下)
 *  - ANTI_CHEAT_ENABLED 在 main/index.js@343726 出厂即 !1(false)
 *    -> 闸门层在当前版本是空操作，保留为"官方热更把它翻成 true 时的保险"。(本文件第 1 节)
 *
 * 第 3 节为什么不照抄他的钳制公式：
 *   他的触发条件是"玩家在菜单里开的加速倍率"(lsGet("wx_gspeed"))。
 *   我们版本里 worldTimeScale 是【游戏原生】可设的(game/index.js setWorldTimeScale)，
 *   也就是说正常玩家用游戏自带的加速也会让 fk 跑在墙钟前面。
 *   此时若按墙钟把 useTime 钳小，反而制造了一个"客户端上报时长比真实对局短"的新异常。
 *   故本层在【没有我们自己造成的时间缩放】这一前提下只记录不修改；
 *   将来给菜单加"加速"功能时，必须同时打开本层的钳制并只补偿我们自己的那个倍率。
 * ========================================================================== */
;(function () {
  /* ===== 2026-10-07 版本仲裁（真机实测补丁）==============================
   * 真机加载顺序：引擎先加载 files/res/main.js（自举 blob，里面内嵌了本层的一份
   * 【旧副本】），原生 evalString splice 的【新副本】随后才到。
   * 旧守卫写的是 `window.__starry_ac 存在就 return` —— 于是旧副本永久胜出：
   *   真机日志 `[AC] hit-dispatch hooked: prototype.bgN (fingerprint
   *   finalHurtValue+lockMinHP+finalDamage)` 就是旧逻辑（首个三指纹命中即 break）
   *   的产物，新副本进来直接被挡回，dmgFixed 恒为 0 = 静默失效。
   * 改为版本号仲裁：只有「同版或更新版」才让位，低版本一律接管覆盖。
   * 注意 _prev 的旧包装器拿不到原始函数（旧版没留 __acOrig），
   * 故 sinkHook 改为「允许套一层」，这也是唯一能拿到新计数的办法。 */
  var AC_VER = 2;
  if (typeof window === "undefined") return;
  var _prev = window.__starry_ac;
  if (_prev && (_prev.__v | 0) >= AC_VER) return;
  var TAG = "[AC] ";
  function log(s) {
    try { if (window.__starry_nlog) window.__starry_nlog(TAG + s); } catch (e) {}
    try { if (window.MOD_LOG) window.MOD_LOG.push(TAG + s); } catch (e) {}
    try { if (window.console && console.log) console.log(TAG + s); } catch (e) {}
  }
  function srcOf(f) { try { return Function.prototype.toString.call(f); } catch (e) { return ""; } }

  var AC = {
    /* 0 版本 */
    __v: AC_VER,
    /* 1 闸门 */
    gateArmed: false, gateMod: "", gateCount: 0,
    /* 2 出口 */
    sinkSend: false, sinkCqa: false,
    /* 3 结算 */
    reportN: 0, logicalMs: 0, wallMs: 0, clampArmed: false, clamped: 0,
    /* 4 伤害 */
    hitHooked: false, hitKey: "", hitScore: 0, noteN: 0, fixedN: 0,
    /* 4b 一次性证据日志（各上限 3 条，绝不刷屏）
     * 起因：dmgNoted/dmgFixed 只出现在「装载期那一张一次性快照」里，而快照必定
     * 早于进战斗 => 真机上根本看不到战斗中的记账。没有这几条日志，
     * "补偿链路到底跑没跑" 就永远是一个无法证伪的说法。 */
    evHit: 0, evNote: 0, evFix: 0, evReport: 0,
    /* 7 战斗加速（全局时间缩放） */
    speedMul: 1, speedHits: 0, speedPath: "", schedClock: 1, schedHits: 0,
    dtScaled: false, dtHits: 0, speedHolder: null, speedLoopOn: false,
    tapFilterOn: false, tapFilterHits: 0,
    /* 杂项 */
    pending: null, note: "", reg: null, installN: 0, loopStarted: false
  };
  window.__starry_ac = AC;

  /* 战斗加速的施加链：**载入即启动**，不依赖 capture（实测：主包 capture 迟迟不来时
   * 之前只有 tick 一条路，而 tick 只在战斗里跑 ⇒ 菜单开到 3 也毫无反应）。
   * 幂等：只允许一条自重排链。 */
  AC.startSpeedLoop = function () {
    if (AC.speedLoopOn) return;
    AC.speedLoopOn = true;
    log("battle-speed: apply loop started (load-time)");
    var tick = function () {
      try { AC.applySpeed(); } catch (e) {}
      try { setTimeout(tick, 1000); } catch (e) {}
    };
    try { setTimeout(tick, 1500); } catch (e) {}
  };
  try { AC.startSpeedLoop(); } catch (e) {}

  /* ------------------------------------------------------------ 6 可观测 --
   * 卡密包对应 L7033 W.__WX_AC_STATE__。诊断输出是这套东西的命门：
   * 挂不上 / 字段名变了 / 条件不满足，全部不会自己报错(静默失效)。 */
  window.__AC_STATE__ = function () {
    return {
      gateArmed: !!AC.gateArmed, gateMod: AC.gateMod || "", gateCount: AC.gateCount,
      sinkSendMsg: !!AC.sinkSend, sinkGsMgrCQA: !!AC.sinkCqa,
      hitHooked: !!AC.hitHooked, hitKey: AC.hitKey || "",
      dmgNoted: AC.noteN, dmgFixed: AC.fixedN,
      dmgHit: AC.evHit, dmgEvNote: AC.evNote, dmgEvFix: AC.evFix, repEv: AC.evReport,
      reportN: AC.reportN, logicalMs: Math.round(AC.logicalMs), wallMs: Math.round(AC.wallMs),
      tickSeen: AC.tickSeen, tickN: AC.tickN, tickBad: AC.tickBad,
      clampArmed: !!AC.clampArmed, clamped: AC.clamped,
      speedMul: AC.speedMul, speedHits: AC.speedHits, speedPath: AC.speedPath || "",
      schedClock: AC.schedClock, schedHits: AC.schedHits,
      dtScaled: !!AC.dtScaled, dtHits: AC.dtHits | 0, speedLoopOn: !!AC.speedLoopOn,
      installN: AC.installN, note: AC.note
    };
  };

  /* --------------------------------------------------------- 1 闸门(保险) --
   * 卡密包 L6908 用 Object.defineProperty 把模块导出上的 ANTI_CHEAT_ENABLED 焊成 false
   * (get 恒 false / set 空 / configurable 保留，便于我们反复覆盖且不破坏枚举)。
   * 他的三面覆盖 = 包 __require + 15 个已知模块名 + globalThis 全局扫描(5 秒节流)。
   * 我们的覆盖面不同：直接把含该明文的模块加进 targets(见 patch 主文件的 targets 判定)，
   * 模块工厂跑完立刻 scan 到它的 exports —— 比照抄他的名字表可靠
   * (实测他的 15 个名字里 $DOB/$DOC/$DOD/$DOE/$hV/$hW 在我们版本全部不存在)。 */
  var GATE_KEY = "ANTI_CHEAT_ENABLED";
  function gateModule(m, tag) {
    if (!m || typeof m !== "object") return false;
    var has = false;
    try { has = (GATE_KEY in m); } catch (e) { return false; }
    if (!has) return false;
    var was = false;
    try { was = !!m[GATE_KEY]; } catch (e) {}
    try {
      Object.defineProperty(m, GATE_KEY, {
        get: function () { return false; },
        set: function () {},
        configurable: true
      });
    } catch (e) { return false; }
    AC.gateCount++;
    if (was) { AC.gateArmed = true; AC.gateMod = String(tag); log("gate ARMED on module " + tag + " (was true)"); }
    else log("gate covered module " + tag + " (already false — 本版本出厂即 false，属保险层)");
    return true;
  }
  function gateSweep(obj, tag) {
    /* 兜底：exports 上没直接挂，但它的成员对象上挂着(卡密包的 __require 包装等价物)。 */
    if (!obj || typeof obj !== "object") return 0;
    var n = 0;
    if (gateModule(obj, tag)) n++;
    try {
      for (var k in obj) {
        var v; try { v = obj[k]; } catch (e) { continue; }
        if (v && typeof v === "object" && v !== window) { if (gateModule(v, tag + "." + k)) n++; }
      }
    } catch (e) {}
    return n;
  }

  /* ------------------------------------------------------------- 2 出口挂钩 --
   * 卡密包 L7001 acHookSink + L7015 installReportIntegrity：只挂
   *   $eZ.battleServer.sendMsg 与 $eZ.gsMgr.cQA
   * 他的原注释："只挂命名的稳定入口，不碰逐版本重混淆的短名方法"。
   * 我们版本独立验证了这个挂点选得对：battleMgr.bER@4682536 就是这两个出口的分流器
   *   e.prototype.bER = function (t, e) {
   *     return t ? $eZ.battleServer.sendMsg(FightInstance.reportResult, e)
   *              : $eZ.gsMgr.cQA(FightInstance.reportResult, e, {isBlockEvent:!1, merge:!1});
   *   };
   * 帧伤害上报 battleMgr.bET 也走 $eZ.battleServer.sendMsg。 */
  function sinkHook(obj, key, tag) {
    if (!obj) return false;
    var fn; try { fn = obj[key]; } catch (e) { return false; }
    /* 版本仲裁：本版或更新版已挂过 -> 让位；旧版挂过 -> 允许再套一层。
     * 旧版没留 __acOrig，原始函数拿不回来，套一层是唯一能拿到新计数的办法；
     * §3/§4 都不改报文本身，重复观测无副作用。 */
    if (typeof fn !== "function" || (fn.__acSink && (fn.__acVer | 0) >= AC_VER)) return false;
    var of = fn.__acOrig || fn;
    var w = function () {
      /* 热路径：cQA 是全协议出口(全包 509 处调用)，早退必须便宜。
       * sendMsg(proto, payload, ...) 与 cQA(proto, payload, opts) 的 payload 都在第 2 个实参。 */
      var p = null;
      try { p = arguments[1]; } catch (e) {}
      if (p && typeof p === "object" && p.completeResult) {
        try { onReport(p, tag); } catch (e) {}
      }
      return of.apply(this, arguments);
    };
    w.__acSink = true;
    w.__acVer = AC_VER;
    w.__acOrig = of;
    try { obj[key] = w; } catch (e) { return false; }
    return true;
  }

  /* ------------------------------------------------- 3 结算字段观测/守卫 --
   * 卡密包 L6975 acFixReport：
   *   fk  = 游戏逻辑战斗时长(秒)   spd = 玩家加速倍率   fkMs = floor(fk*1000)
   *   开加速: realMs = Date.now()-riStartWall; est = floor(fk/spd*1000);
   *           realMs = max(realMs, est); want = max(1000, min(realMs, fkMs))
   *   未加速: want = min(cur, fkMs)
   * 我们版本：useTime = floor(1e3 * battleTime)，battleTime 就是 battleLogic 的 fk
   * (@5716445 实测 floor(1e3*y)，y = f.fW.battleTime)。
   * 本层保留同一套计算与诊断，但默认【只算不改】(AC.clampArmed=false)，理由见文件头。 */
  function onReport(payload, tag) {
    if (!payload || typeof payload !== "object") return;
    var cr = null;
    try { cr = payload.completeResult; } catch (e) { return; }
    if (!cr || typeof cr.useTime !== "number") return;
    AC.reportN++;
    AC.logicalMs = cr.useTime;
    var wall = AC.wallMs;
    AC.wallMs = 0;                       /* 一局一归零：区间只会多算(空闲帧)，不会少算 */
    var cur = cr.useTime;
    var want = cur;
    try {
      if (AC.clampArmed) {
        /* 战斗加速打开后，逻辑钟被我们放大了 k 倍 ⇒ 只折算我们自己的那个倍率：
         * base = 逻辑时长 / k ≈ 真实墙钟时长；墙钟可用时再取二者较小值兜底。 */
        var kk = AC.speedMul > 1 ? AC.speedMul : 1;
        var base = cur / kk;
        if (wall > 0 && Math.abs(base - wall) > Math.max(1500, base * 0.5)) {
          /* 墙钟与折算值差太远（多半是没进战斗就结算 / tick 没量到）⇒ 只按倍率折算 */
          AC.note = "wall off (wall " + Math.round(wall) + " vs base " + Math.round(base) + "), mul-only";
          want = Math.max(1000, base);
        } else if (wall > 0) {
          want = Math.max(1000, Math.min(wall, base));
        } else {
          want = Math.max(1000, base);
        }
      }
    } catch (e) {}
    want = Math.floor(want);
    if (want !== cur) {
      cr.useTime = want;
      AC.clamped++;
      log("useTime compensated " + cur + " -> " + want + " (wall " + Math.round(wall) + ")");
    } else {
      AC.note = "useTime=" + cur + " wall=" + Math.round(wall) + " (" + tag + ")" + (AC.clampArmed ? "" : " [observe-only]");
    }
    /* 证据链 ③：结算出口真的被调到（前 3 次）。
     * 与 §4 同理：reportN/logicalMs/wallMs 只活在一次性快照里，而快照打于进战斗之前，
     * 真机上根本看不到结算。没有这条，"出口挂钩到底有没有在工作"无法证明。
     * 同时把"逻辑钟 vs 墙钟"两个数并排打出来 —— 这正是他 §3 补偿公式的输入，
     * 也是我们判断"将来要不要打开 clampArmed"的唯一依据。 */
    if (AC.evReport < 3) {
      AC.evReport++;
      log("report #" + AC.evReport + ": useTime=" + cur + " wallMs=" + Math.round(wall) +
          " delta=" + Math.round(cur - wall) + "ms tag=" + tag +
          " clamped=" + (want !== cur ? ("yes->" + want) : "no") +
          " clampArmed=" + !!AC.clampArmed + " reportN=" + AC.reportN);
    }
  }
  /* 结算出口暴露一份给离线单测/诊断用（真机路径仍由出口钩子调用同一个函数）。 */
  AC.onReport = onReport;

  /* -------------------------------------------------------- 4 伤害字段还原 --
   * 卡密包 L2578 HitBus.hookPrototype(proto,"hY")：
   *   改前记 atkStat.finalDamage / vicStat.finalHurtValue；
   *   放大 ctx.damage 后调原生；若 ctx.damage 被改过，写回
   *     atkStat.finalDamage = atkBefore + originalDamage   (不是清零！)
   *     vicStat.finalHurtValue = vicBefore + originalDamage
   *   原因(他 L2597 原注释)：这两个字段有两个消费者 ——
   *     battleMgr.bET({damage,takenDamage,...}) 上报 与 伤害统计面板 o.cZq/c.cZr。
   *   我们版本逐字复现：
   *     battleLogic hY: o.finalHurtValue += e;   /  t.hw.finalDamage += e;
   *     game@4646511 pe.nP: $eZ.battleMgr.bET({damage:n, takenDamage:i, heal:0, deathCount:o});
   *     game@5711602: o.cZq = e.hw.finalDamage; o.cZr = e.hw.finalHurtValue;
   * 挂钩点用"明文属性名指纹"而非硬编码方法名(hY 是短名，逐版本可能变)：
   *   lockMinHP 是我们自己补丁早就依赖的明文锚点(无敌功能)，跨版本稳定。
   *
   * !! 2026-10-07 真机实测修正（离线验收台漏掉的歧义）!!
   *   只要求三个明文串同时出现是不够的：battleLogic 里还躺着诱饵 bgN
   *   (@120602，长 477) —— 它是一堆字段重置(this.bgy=0; this.finalDamage=0;
   *   this.finalHurtValue=0; this.lockMinHP=0; this.hpRateRecord=-1; ...)，
   *   三个明文串全都有，但**没有任何 Math.max、也没有 bhd**。
   *   原先"首个命中即 break"在真机上先扫到 bgN 就挂上去了 —— 挂完之后
   *   dmgNoted 会涨而 dmgFixed 恒为 0，即"静默失效"，正是最危险的那种失败。
   *   修正：改成**打分制**，并且**必须**含真扣血表达式 Math.max(...lockMinHP...)
   *   （hY 体内是 o.bhd = Math.max(o.bhd - a, o.lockMinHP)），缺它一律不认。
   *   实测 gT(@104821) / gU(@105076) / bgN(@120602) 均无 Math.max(...lockMinHP)，
   *   只有 hY(@114312) 有；该正则在真机上唯一命中 hY。 */
  function hookHitDispatch(Cls) {
    if (AC.hitHooked || !Cls || !Cls.prototype) return;
    var p = Cls.prototype, key = null, best = 0;
    try { for (var k in p) {
      var v; try { v = p[k]; } catch (e) { continue; }
      if (typeof v !== "function") continue;
      var t = srcOf(v);
      var sc = 0;
      if (t.indexOf("finalHurtValue") >= 0) sc++;
      if (t.indexOf("lockMinHP") >= 0) sc++;
      if (t.indexOf("finalDamage") >= 0) sc++;
      if (sc < 3) continue;
      /* 硬门槛：真扣血表达式。诱饵 bgN 在这一步被挡掉。 */
      if (!/Math\.max\([^()]*lockMinHP[^()]*\)/.test(t)) continue;
      sc += 10;
      if (t.indexOf("finalHurtValue +=") >= 0 || t.indexOf("finalHurtValue+=") >= 0) sc += 3;
      if (t.indexOf("finalDamage +=") >= 0 || t.indexOf("finalDamage+=") >= 0) sc += 3;
      if (sc > best) { best = sc; key = k; }
    } } catch (e) {}
    if (!key) return;
    var oHit = p[key];
    p[key] = AC.wrapHit(oHit, key);
    AC.hitHooked = true; AC.hitKey = key; AC.hitScore = best;
    log("hit-dispatch hooked: prototype." + key + "  (score " + best + ", fingerprint Math.max(...lockMinHP))");
  }

  /* hY 包装器（本层打分命中与 baseline 交割共用同一份实现，避免两套还原逻辑漂移） */
  AC.wrapHit = function (oHit, key) {
    var w = function (attacker, damage, ctx) {
      var atkB = null, vicB = null;
      try { if (attacker && attacker.hw) atkB = attacker.hw.finalDamage; } catch (e) {}
      try { if (this && this.hw) vicB = this.hw.finalHurtValue; } catch (e) {}
      var pd = AC.pending; AC.pending = null;
      var r = oHit.apply(this, arguments);
      /* 证据链 ①：hY 真的被调到了（前 3 次）。没有这条，"钩子没跑" 和
       * "钩子跑了但没有放大" 在真机上无法区分 —— 这正是上一轮踩的坑。 */
      if (AC.evHit < 3) {
        AC.evHit++;
        log("dmg hit #" + AC.evHit + ": dmg=" + damage + " key=" + key +
            " pending=" + (pd ? (pd.orig + "->" + pd.boosted) : "none") +
            " atkBefore=" + atkB + " vicBefore=" + vicB);
      }
      /* 自校验：只有"这一次命中确实吃到了我们放大的伤害"才补偿(等价于他 ctx.damage !== originalDamage) */
      if (pd && pd.boosted === damage && pd.orig !== pd.boosted) {
        var atkA = null, vicA = null;
        try { if (attacker && attacker.hw) atkA = attacker.hw.finalDamage; } catch (e) {}
        try { if (this && this.hw) vicA = this.hw.finalHurtValue; } catch (e) {}
        try { if (atkB !== null && attacker.hw.finalDamage !== atkB) attacker.hw.finalDamage = atkB + pd.orig; } catch (e) {}
        try { if (vicB !== null && this.hw.finalHurtValue !== vicB) this.hw.finalHurtValue = vicB + pd.orig; } catch (e) {}
        AC.fixedN++;
        /* 证据链 ②：还原真的写回了（前 3 次）。atkAfter -> atkFixed 必须等于原始伤害。 */
        if (AC.evFix < 3) {
          AC.evFix++;
          var atkF = null, vicF = null;
          try { if (attacker && attacker.hw) atkF = attacker.hw.finalDamage; } catch (e) {}
          try { if (this && this.hw) vicF = this.hw.finalHurtValue; } catch (e) {}
          log("dmg fix #" + AC.evFix + ": orig=" + pd.orig + " boosted=" + pd.boosted +
              " finalDamage " + String(atkB) + "->" + String(atkA) + "->" + String(atkF) +
              "  finalHurtValue " + String(vicB) + "->" + String(vicA) + "->" + String(vicF) +
              "  fixedN=" + AC.fixedN);
        }
      }
      return r;
    };
    w.__acHit = true;
    /* 版本戳：被本版包过的方法天然不会进打分循环(包装器源码里没有 lockMinHP)。
     * 打戳是为了诊断"这个 hY 到底是哪一版 AC 包的"——旧层若还在，戳会是旧值。 */
    w.__acHitVer = AC_VER;
    return w;
  };

  /* !! 2026-10-07 真机修正：baseline 的 tryEntity 先用 RE_APPLY（真扣血表达式）找到 hY，
   * 并把 Cls.prototype[hx] 换成它自己的 feature2 包装器。包装器源码里【没有】
   * Math.max(...lockMinHP)，于是本层的打分硬门槛会把"已被 baseline 包过的真 hY"
   * 判为不合格 -> hitHooked 恒 false（静默失效，正是最危险的失败形态）。
   * 实机证据：同一 tick 内 MODMOD 打 "entity class hit: damage-apply = .prototype.hY"，
   * 本层一行都不打；install loop 跑满 120 次 hitHooked=false。
   *
   * 结论：指纹仲裁权交给 baseline（它用 RE_APPLY，比本层的打分更精确），
   * 本层只负责"包装顺序 + 累加器还原"。adoptHit 由 baseline 在命中处调用，
   * 且必须发生在 baseline 取走 oHX 之前 => 本层包装在最内层、baseline 包装在最外层：
   *   feature2 关: baseline wrapper -> oHX(=本层) -> 原 hY   本层正常记账/还原
   *   feature2 开: baseline 提前 return -> 本层根本不执行    不会凭空写累加器
   * 反向（本层在外层）时 feature2 开会先让本层补 pd.orig、再被 baseline 归零 -> 累加器被写脏。 */
  AC.adoptHit = function (Cls, key) {
    if (AC.hitHooked || !Cls || !Cls.prototype) return;
    if (typeof key !== "string" || !key) return;
    var fn;
    try { fn = Cls.prototype[key]; } catch (e) { return; }
    if (typeof fn !== "function" || fn.__acHit) return;
    Cls.prototype[key] = AC.wrapHit(fn, key);
    /* hitScore = -1: 名字由 baseline 的 RE_APPLY 仲裁，未经本层打分（区别于 hitScore>=13） */
    AC.hitHooked = true; AC.hitKey = key; AC.hitScore = -1;
    log("hit-dispatch adopted from baseline: prototype." + key);
  };

  /* 由主补丁的伤害公式钩子调用：把"未放大值"记下来，给上面 hY 钩子做还原依据。
   * 配对纪律：每次进入伤害公式钩子先 clearPending()，只有"确实放大了"才重新置位。
   * 这样 pending 永远不会跨过一次 GD 调用存活 —— 即便某次伤害没走到 hY，
   * 下一笔伤害也不会误配(再加上 hY 侧的 pd.boosted === damage 自校验，双重保险)。 */
  AC.noteDamage = function (orig, boosted) {
    if (typeof orig === "number" && typeof boosted === "number" && boosted > orig) {
      AC.pending = { orig: orig, boosted: boosted };
      AC.noteN++;
      /* 证据链 ⓪：伤害公式确实产出了"被我们放大的值"（前 3 次）。
       * 没有这条，菜单滑块到底有没有作用到 GD 上也无法证明。 */
      if (AC.evNote < 3) {
        AC.evNote++;
        log("dmg boosted #" + AC.evNote + ": orig=" + orig + " -> boosted=" + boosted +
            " (x" + (orig ? Math.round((boosted / orig) * 100) / 100 : "inf") + "), noteN=" + AC.noteN);
      }
    } else {
      AC.pending = null;
    }
  };
  AC.clearPending = function () { AC.pending = null; };

  /* 由主补丁的移动系统 update 钩子调用：用【未缩放】的帧 dt 累计墙钟，
   * 使"逻辑钟是否跑在墙钟前面"变成实测值，而不是像他那样靠玩家配置的倍率反推。
   * tickSeen/tickBad 是给真机诊断用的：如果 tickSeen 一直在涨而 tickN 不涨，
   * 说明我们钩到的 update 身上没有 this.dt(bundle 换版本/换类) -> 墙钟不可信，
   * 此时绝不能打开钳制(默认也是关的)。 */
  AC.tickSeen = 0; AC.tickN = 0; AC.tickBad = 0;
  AC.tick = function (dt) {
    AC.tickSeen++;
    if (typeof dt === "number" && dt > 0 && dt < 1) {
      AC.tickN++;
      /* 战斗加速打开后引擎 dt 已被 ×k ⇒ 这里除回去才是真实墙钟，
       * 否则 wallMs 与逻辑钟同比例放大，结算补偿会误判成"没加速"。 */
      var _kk = AC.speedMul > 1 ? AC.speedMul : 1;
      AC.wallMs += (dt * 1000) / _kk;
    }
    else AC.tickBad++;
    try { AC.applySpeed(); } catch (e) {}
  };

  /* --------------------------------------------- 7 战斗加速（全局时间缩放） --
   * 用户点名的新功能：一个滑条，1=关、2..5=倍速。
   * 落点两块，互斥使用，避免同一个 k 被算两次（k² 会直接把战斗打成幻灯片）：
   *   ① 战斗内：battle world 的 worldTimeScale —— battleLogic update 里
   *      fk += dt*hitPauseTimeScale*worldTimeScale*mainTimeScale，这是战斗节奏真正的乘数；
   *   ② 战斗外：cc.director.getScheduler().setTimeScale(k) —— 全局调度器加速（挂机/动画/剧情）。
   * 铁律：不写死 ctrl/world 的混淆短名（hook-rules §24），world 用**明文名**
   * worldTimeScale(混淆器不动) 做结构发现，命中路径记进 AC.speedPath 便于复现。
   * 账：我们自己拉长了逻辑时钟 ⇒ 结算 useTime 必须按同一倍率补偿（clampArmed 打开），
   * 否则服务端看到的是「对局时长 >> 墙钟」的异常。 */
  /* 有界广度扫描：从 $eZ.battleMgr 起按 BFS 找「自己身上带数值型 worldTimeScale」的对象。
   * 深度上限 3（battleLogic 实测形态：world 挂在 ctrl 下、或再套一层），
   * 命中就缓存（AC.speedHolder），每轮只做一次 key 扫描 + 一次属性比对，代价可忽略。
   * 找不到就返回 null —— 调用方**不猜也不写**，绝不拿短名硬取值。 */
  function findSpeedHolder() {
    var g = null;
    try { g = window.$eZ; } catch (e) {}
    if (!g) return null;
    var root = g.battleMgr || g;
    var seen = [];
    var queue = [{ o: root, p: "battleMgr" }];
    var depth = 0;
    while (queue.length && depth <= 3) {
      var next = [];
      for (var i = 0; i < queue.length; i++) {
        var cur = queue[i];
        var o = cur.o;
        if (!o || typeof o !== "object") continue;
        var dup = false;
        for (var s = 0; s < seen.length; s++) { if (seen[s] === o) { dup = true; break; } }
        if (dup) continue;
        seen.push(o);
        try {
          if (typeof o.worldTimeScale === "number") { AC.speedPath = cur.p; return o; }
        } catch (e0) {}
        var keys = [];
        try { keys = Object.keys(o); } catch (e1) { keys = []; }
        for (var j = 0; j < keys.length; j++) {
          var v = null;
          try { v = o[keys[j]]; } catch (e2) { continue; }
          if (v && typeof v === "object" && typeof v !== "function") {
            next.push({ o: v, p: cur.p + "." + keys[j] });
          }
        }
      }
      queue = next;
      depth++;
    }
    return null;
  }

  /* ---- 真·全局变速：挂引擎的 calculateDeltaTime，把每帧 _deltaTime ×k ----
   * 引擎 mainLoop（cocos2d-jsb 1.1.8.1 src/cocos2d-jsb.js@6406）：
   *   this.calculateDeltaTime(t); var dt = this._deltaTime;
   *   → compScheduler.updatePhase(dt) / _scheduler.update(dt) / ActionManager / 物理 / 组件 update
   * 一处放大即全局生效：战斗、过场、挂机、动画、UI 动效全部 k 倍。
   * 不动 _lastUpdate（仍按真实墙钟推进），所以每帧只放大「本帧间隔」，不累积漂移。
   * 兜底顺序：dt 放大装不上 → 战斗内 worldTimeScale；再不行 → 调度器 setTimeScale。
   * 三条路线**互斥**，否则同一个 k 会被平方。 */
  function installDeltaScale() {
    try {
      if (AC.dtScaled) return true;
      var d = (window.cc && cc.director) ? cc.director : null;
      if (!d || typeof d.calculateDeltaTime !== "function") return false;
      if (d.__starryDtWrapped) { AC.dtScaled = true; return true; }
      var orig = d.calculateDeltaTime;
      d.calculateDeltaTime = function (t) {
        var r = orig.apply(this, arguments);
        try {
          var k = 1;
          try { k = (window.MOD && window.MOD.bspeed) ? (window.MOD.bspeed | 0) : 1; } catch (e0) { k = 1; }
          if (k > 1) {
            var dt = this._deltaTime;
            if (typeof dt === "number" && dt > 0) {
              this._deltaTime = dt * k;
              AC.dtHits = (AC.dtHits | 0) + 1;
            }
          }
        } catch (e1) {}
        return r;
      };
      d.__starryDtWrapped = true;
      AC.dtScaled = true;
      log("battle-speed: engine dt scaling armed (calculateDeltaTime x k)");
      return true;
    } catch (e) { return false; }
  }
  AC.installDeltaScale = installDeltaScale;

  /* ------------------------------------- 8 压掉 TapSDK 的「包名/签名」提示 --
   * 背景（2026-10-24 实测）：本作 TapSDK 的签名校验读的是 **APK v2 签名块**（不是
   * PackageManager，故 Java 侧 spoof 覆盖不到；META-INF 探针也证明它不读 v1）。
   * 包层面无解（v2 块只能是我们星空密钥签的），用户选择「只压提示」。
   * 做法：把 ui 文本里同时出现 TapSDK 与（包名/签名/clientId）的字符串过滤掉。
   * 覆盖 cc.Label / cc.RichText 的 string setter；只在命中时改写，其它一律原样透传。 */
  AC.tapFilterHits = 0;
  function isTapWarn(v) {
    if (typeof v !== "string" || v.length < 6 || v.length > 400) return false;
    if (v.indexOf("TapSDK") < 0 && v.indexOf("TapTap") < 0) return false;
    return v.indexOf("包名") >= 0 || v.indexOf("签名") >= 0 || v.indexOf("clientId") >= 0 ||
           v.indexOf("client_id") >= 0 || v.indexOf("certificate") >= 0;
  }
  function patchTextProp(Proto, label) {
    try {
      if (!Proto) return false;
      var d = Object.getOwnPropertyDescriptor(Proto, "string");
      if (!d || !d.set || Proto.__starryTapFilter) return !!Proto.__starryTapFilter;
      Object.defineProperty(Proto, "string", {
        configurable: true,
        enumerable: d.enumerable,
        get: d.get,
        set: function (v) {
          try {
            if (isTapWarn(v)) {
              AC.tapFilterHits = (AC.tapFilterHits | 0) + 1;
              if (AC.tapFilterHits <= 3) log("tap-warn text suppressed #" + AC.tapFilterHits + " (" + label + ")");
              v = "";
            }
          } catch (e) {}
          return d.set.call(this, v);
        }
      });
      Proto.__starryTapFilter = true;
      log("tap-warn filter armed on " + label);
      return true;
    } catch (e) { return false; }
  }
  AC.installTapFilter = function () {
    if (AC.tapFilterOn) return true;
    try {
      if (typeof cc === "undefined" || !cc) return false;
      var ok = false;
      if (cc.Label) ok = patchTextProp(cc.Label.prototype, "cc.Label") || ok;
      if (cc.RichText) ok = patchTextProp(cc.RichText.prototype, "cc.RichText") || ok;
      if (ok) AC.tapFilterOn = true;
      return ok;
    } catch (e) { return false; }
  };

  AC.applySpeed = function () {
    var k = 1;
    try { k = (window.MOD && window.MOD.bspeed) ? (window.MOD.bspeed | 0) : 1; } catch (e) { k = 1; }
    if (!(k > 1)) k = 1;
    if (AC.speedMul !== k) {
      AC.speedMul = k;
      AC.clampArmed = k > 1;   /* 只有我们自己加速时才钳 useTime，别的来源一律不碰 */
      log("battle-speed = " + k + "x (useTime clamp " + (AC.clampArmed ? "armed" : "off") + ")");
    }
    /* 主路线：引擎 dt 放大（真·全局变速）。装上后调度器/worldTimeScale 一律保持 1，防叠乘。 */
    try { installDeltaScale(); } catch (e) {}
    try { AC.installTapFilter(); } catch (e) {}
    var holder = null;
    try {
      /* 缓存优先：战斗换局时 world 会换对象，取不到数值就重扫一次 */
      if (AC.speedHolder && typeof AC.speedHolder.worldTimeScale === "number") holder = AC.speedHolder;
      else { holder = findSpeedHolder(); if (holder) AC.speedHolder = holder; }
    } catch (e) { holder = null; }
    if (holder) {
      /* 战斗内兜底：只有在「引擎 dt 放大」没装上的时候才用 worldTimeScale，
       * 否则 dt 已经 ×k，再乘一次就是 k²（2026-10-24 用户实测"没效果"的教训：
       * 光改 worldTimeScale 不影响 UI/过场/挂机，且 battle world 常常找不到）。 */
      try {
        if (!AC.dtScaled && holder.worldTimeScale !== k) {
          if (typeof holder.setWorldTimeScale === "function") holder.setWorldTimeScale(k);
          else holder.worldTimeScale = k;
          AC.speedHits = (AC.speedHits | 0) + 1;
          if (AC.speedHits <= 3) log("battle-speed world=" + k + "x @ " + (AC.speedPath || "battleMgr"));
        }
      } catch (e) {}
      return;
    }
    /* 战斗外兜底：调度器（dt 放大可用时保持 1，避免叠乘） */
    try {
      if (window.cc && cc.director && typeof cc.director.getScheduler === "function") {
        var sch = cc.director.getScheduler();
        var want = AC.dtScaled ? 1 : k;
        if (sch && typeof sch.setTimeScale === "function" && AC.schedClock !== want) {
          sch.setTimeScale(want);
          AC.schedClock = want;
          if (!AC.dtScaled) {
            AC.schedHits = (AC.schedHits | 0) + 1;
            if (AC.schedHits <= 3) log("battle-speed scheduler=" + k + "x (fallback, out of battle)");
          }
        }
      }
    } catch (e) {}
  };


  /* --------------------------------------------------------------- 安装调度 --
   * 主补丁在 bundle 载入瞬间就被调用(__starry_capture)，那时 $eZ 还没被赋值
   * (globalThis.$eZ 在 main/index.js 的模块工厂里才 Object.assign 出来)。
   * 所以出口挂钩必须惰性安装：每次 scan / 每次 capture 都试一次，再挂一个低频重试。 */
  function tryInstall() {
    var z = null;
    try { z = window.$eZ; } catch (e) {}
    if (!z) return false;
    AC.installN++;
    if (!AC.sinkSend && z.battleServer) { if (sinkHook(z.battleServer, "sendMsg", "sendMsg")) { AC.sinkSend = true; log("sink hooked: $eZ.battleServer.sendMsg"); } }
    if (!AC.sinkCqa && z.gsMgr) { if (sinkHook(z.gsMgr, "cQA", "gsMgr.cQA")) { AC.sinkCqa = true; log("sink hooked: $eZ.gsMgr.cQA"); } }
    if (z.battleServer && z.gsMgr && !z.battleServer.__acSweepDone) {
      try { gateSweep(z, "$eZ"); z.battleServer.__acSweepDone = true; } catch (e) {}
    }
    return !!(AC.sinkSend && AC.sinkCqa);
  }
  AC.tryInstall = tryInstall;

  /* 主补丁每扫到一个目标模块都会调这里 */
  AC.scan = function (exp, tag) {
    try { gateSweep(exp, tag || "module"); } catch (e) {}
    if (exp) {
      for (var k in exp) {
        var V; try { V = exp[k]; } catch (e) { continue; }
        if (typeof V === "function") { try { hookHitDispatch(V); } catch (e) {} continue; }
        /* 兜底：类通常挂在 exports 的成员的成员上(bundle 打包器把每个源文件包成
         * 一层命名空间)，所以再下探一层。命中后 hookHitDispatch 内部会自我短路。 */
        if (AC.hitHooked) continue;
        if (V && typeof V === "object") {
          for (var k2 in V) {
            var V2; try { V2 = V[k2]; } catch (e) { continue; }
            if (typeof V2 === "function") { try { hookHitDispatch(V2); } catch (e) {} }
          }
        }
      }
    }
    tryInstall();
  };

  /* 主补丁在 __starry_capture 里把模块注册表交进来(留着做兜底扫描)。
   * 用自重排的 setTimeout 而不是 setInterval：主补丁自己就只用 setTimeout，
   * 不依赖 JSB 是否暴露 setInterval。 */
  AC.capture = function (reg) {
    AC.reg = reg;
    try { tryInstall(); } catch (e) {}
    if (AC.loopStarted) return;      /* bundle 可能多次调捕获口, 只允许一条轮询链 */
    AC.loopStarted = true;
    try { AC.startSpeedLoop(); } catch (e) {}
    var n = 0;
    var loop = function () {
      n++;
      var ok = false;
      try { ok = tryInstall(); } catch (e) {}
      /* 两个出口都挂上、且命中钩子有结论后停；最多 120 轮(约 60 秒) */
      if ((ok && AC.hitHooked) || n >= 120) {
        log("install loop done after " + n + " tries  state=" + JSON.stringify(window.__AC_STATE__()));
        return;
      }
      try { setTimeout(loop, 500); } catch (e) {}
    };
    try { setTimeout(loop, 500); } catch (e) {}
  };

  log("ac layer v1 loaded (observe-only useTime; damage accumulator compensation armed)");
})();
