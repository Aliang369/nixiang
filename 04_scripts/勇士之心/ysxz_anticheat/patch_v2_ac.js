

/*starry_ysxz_v1*/
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
;(function () {
  var H = "com.starry.magic.cocos.YsxzHooks";
  function R(n) {
    try {
      if (typeof jsb === "undefined" || !jsb.reflection) return false;
      return !!jsb.reflection.callStaticMethod(H, "getRet", "(I)Z", n);
    } catch (e) { return false; }
  }
  function V(n) {
    try {
      if (typeof jsb === "undefined" || !jsb.reflection) return 1;
      var v = jsb.reflection.callStaticMethod(H, "getVal", "(I)I", n) | 0;
      return v > 0 ? v : 1;
    } catch (e) { return 1; }
  }
  function J(s) {
    try {
      if (typeof jsb === "undefined" || !jsb.reflection) return false;
      jsb.reflection.callStaticMethod(H, "logStatus", "(Ljava/lang/String;)V", s);
      return true;
    } catch (e) { return false; }
  }
  var M = {};
  /* 序号 = 菜单里的位置（0=分类标题），必须与 Games/YsxzEntry.cpp::kYsxz_Features 严格一致。
     2026-10-04 移植：把 1.0.4 的菜单序号搬到 1.0.3 包 —— 1 号位新增伤害倍率滑块，
     秒杀/无敌/无限技能/自动战斗 顺延为 2/3/4/6，移动速度滑块在 5 号位、上限 1..10。 */
  Object.defineProperty(M, "feature1", { get: function () { return R(2); } });   /* 秒杀 */
  Object.defineProperty(M, "feature2", { get: function () { return R(3); } });   /* 无敌 */
  Object.defineProperty(M, "feature3", { get: function () { return R(4); } });   /* 无限技能 */
  Object.defineProperty(M, "feature4", { get: function () { return R(0); } });   /* 占位：移动速度是滑块，仅状态日志用 */
  Object.defineProperty(M, "feature5", { get: function () { return R(7); } });   /* 自动战斗 */
  Object.defineProperty(M, "speed", { get: function () { return V(5); } });      /* 移动速度 1..10 */
  Object.defineProperty(M, "damage", { get: function () { return V(1); } });     /* 伤害倍率 1..50 */
  /* 2026-10-07 新增功能(卡密包同名功能复刻)。序号=菜单位置, 必须与 kYsxz_Features 严格一致:
       8  解锁金卡(已并入「基础功能」分类)  /  9  Category_自动化(分组占位)
       10 自动重复挑战 / 11 自动出售 / 12 自动分解 / 13 自动穿戴
     全部由 automation 层(auto_layer.js)消费, 过检测层不读这些。 */
  Object.defineProperty(M, "feature6",  { get: function () { return R(8); } });  /* 解锁金卡 */
  Object.defineProperty(M, "feature7",  { get: function () { return R(10); } }); /* 自动重复挑战 */
  Object.defineProperty(M, "feature8",  { get: function () { return R(11); } }); /* 自动出售 */
  Object.defineProperty(M, "feature9",  { get: function () { return R(12); } }); /* 自动分解 */
  Object.defineProperty(M, "feature10", { get: function () { return R(13); } }); /* 自动穿戴 */
  /* 2026-10-24 新增：战斗加速（1=关，2..5=倍率），放在「基础功能」里、紧跟移动速度（序号 6）。
     消费方是 ac 层：引擎 calculateDeltaTime 的 _deltaTime ×k（真·全局变速），
     结算 useTime 按同一倍率折回。 */
  Object.defineProperty(M, "bspeed",    { get: function () { return V(6); } }); /* 战斗加速 */
  globalThis.__starry_nlog = J;
  globalThis.MOD = M;
})();

/* ================== MOD 自愈安装器 (勇士之心) ==================
 * 挂载点 = Cocos 自己生成的 window.__require(永不混淆),由它拿到模块注册表。
 * 全部混淆名在运行时发现,不写死任何一个:
 *   1) 用【明文锚点】正则从模块源码里把混淆名捕获出来
 *      —— 字符串字面量("isPlayer")和明文字段名(lockMinHP / maxCdTime /
 *         maxApDefRate / worldTimeScale ...)混淆器不会动
 *   2) 用 Function.prototype.toString() 按指纹认出目标函数,再包装
 *
 * ★ 本 bundle 的入口列表包含【全部模块】,即所有模块在 bundle 加载瞬间就执行完了,
 *   那时菜单还没注入 MOD。所以:
 *     发现 + 挂钩 一律无条件执行;
 *     if (MOD.featureN) 一律放在钩子【内部】(调用时才读),这样菜单随时开关都生效。
 *
 * 日志: 统一 [MODMOD] 前缀, 走 cc.log(原生)→console.log(WebView) 双通道,
 *       同时存一份到 window.MOD_LOG。抓法: adb logcat | grep MODMOD
 * ============================================================== */
function MOD_boot(REG) {
    var N = {};        /* 发现到的混淆名 */
    var CTX = null;    /* 战斗上下文(从系统 update 里捎带缓存) */

    function log(s) {
        var msg = "[MODMOD] " + s;
        try { if (!window.MOD_LOG) window.MOD_LOG = []; window.MOD_LOG.push(msg); } catch (e) {}
        try { if (window.__starry_nlog) window.__starry_nlog(msg); } catch (e) {}
        try { if (typeof cc !== "undefined" && cc && typeof cc.log === "function") { cc.log(msg); return; } } catch (e) {}
        try { if (typeof console !== "undefined" && console && console.log) console.log(msg); } catch (e) {}
    }
    function srcOf(f) { try { return Function.prototype.toString.call(f); } catch (e) { return ""; } }

    /* ---------- 1. 明文锚点 → 捕获混淆名 ---------- */
    function harvestA(s) {
        var m;
        if (!N.hx) {
            m = /case\s*["']isPlayer["']\s*:\s*return\s+\w+\.(\w+)\.(\w+)\s*[;,}]/.exec(s);
            if (m) { N.hx = m[1]; N.isPlayer = m[2]; }
        }
        if (!N.hD) {
            m = /\w+\.(\w+)\.(\w+)\s*===\s*\w+\s*!==\s*\w+\.isPlayerCamp/.exec(s);
            if (m) { N.hD = m[1]; N.camp = m[2]; }
        }
        if (!N.ctxKey) {
            m = /this\.context\.(\w+)\.(\w+)\[0\]\.(\w+)/.exec(s);
            if (m) { N.ctxKey = m[1]; N.teams = m[2]; N.teamCamp = m[3]; }
        }
        if (!N.hp) {
            m = /(\w+)\.(\w+)\s*=\s*Math\.max\(\1\.\2\s*-\s*\w+,\s*\1\.lockMinHP\)/.exec(s);
            if (m) { N.hp = m[2]; }
        }
    }
    /* 需要先有 ctxKey, 故第二遍再扫: 实体 → 世界 的属性名 */
    function harvestB(s) {
        if (N.world || !N.ctxKey) return;
        /* 2026-10-26（res 1.1.10.0 静态取证）：旧版要求本模块含 lockMinHP 才扫，而实体模块
         * 不一定带这个明文（世界引用出现在别的模块）⇒ N.world 恒 undefined ⇒ myCamp 只能降级。
         * 现在只认结构「<变量>.<世界名>.bou.」且前缀不限定 this.（实体侧写法是 e.hj.bou.）。 */
        /* 必须遍历所有匹配: 同一模块里可能先出现 this.context.<ctx>(系统的写法) */
        var re = new RegExp("this\\.([A-Za-z_$][\\w$]*)\\." + N.ctxKey + "\\.", "g"), m;
        while ((m = re.exec(s)) !== null) {
            if (m[1] !== "context") { N.world = m[1]; return; }
        }
        var re2 = new RegExp("[\\w$)\\]]\\.([A-Za-z_$][\\w$]*)\\." + N.ctxKey + "\\.", "g");
        while ((m = re2.exec(s)) !== null) {
            if (m[1] !== "context") { N.world = m[1]; return; }
        }
    }

    /* ---------- 2. 敌我判定(全部基于发现到的名字, 带降级) ---------- */
    function ctxOf(ent) {
        if (!ent) return CTX;
        var w = N.world ? ent[N.world] : null;
        var c = w ? w[N.ctxKey] : null;
        if (c) return c;
        /* 结构兜底（不写死短名）：实体上任意组件只要带 <ctxKey> 且里面有 teams 数组，
           那就是世界/上下文对象。每个实体最多试 3 次，命中即缓存槽名。 */
        if (N.ctxKey && N.teams && (!N.wTries || N.wTries < 3)) {
            N.wTries = (N.wTries || 0) + 1;
            for (var k in ent) {
                var v;
                try { v = ent[k]; } catch (e5) { continue; }
                if (!v || typeof v !== "object") continue;
                var cc = null;
                try { cc = v[N.ctxKey]; } catch (e4) { continue; }
                if (cc && cc[N.teams]) { N.world = k; log("world comp resolved by structure = ent." + k); return cc; }
            }
        }
        return CTX;
    }
    function myCamp(ent) {
        var c = ctxOf(ent);
        var t0 = c && N.teams && c[N.teams] ? c[N.teams][0] : null;
        var v = t0 && N.teamCamp ? t0[N.teamCamp] : null;
        if (typeof v === "number" && v >= 0) return v;
        return 1;                 /* 降级: 本作阵营 1 = 我方(gF() 取 camp!=1 当敌人) */
    }
    /* 2026-10-26（res 1.1.10.0 静态取证）：阵营字段从 bundle 源码唯一命中 hE.bma
     * （锚点 x.<hD>.<camp> === y !== z.isPlayerCamp，y 来自 this.context.bou.fF[0].dga）。
     * 旧版写死的 blW/blX/blY 是 1.1.9.x 遗留候选，在 1.1.10.0 完全不存在（hook-rules §24
     * 禁止写死混淆短名），换成“类型校验 + 用世界里的队伍阵营值做结构对拍”的兜底。 */
    function campOf(ent) {
        if (!ent || !N.hD) return null;
        var h = ent[N.hD];
        if (!h) return null;
        var v = N.camp ? h[N.camp] : null;
        if (typeof v === "number") return v;
        /* 结构兜底：世界上各队伍的阵营值是“真值集合”，在 h 上找值落在集合里的数字字段 */
        var set = [];
        try {
            var c = ctxOf(ent);
            var ts = c && N.teams ? c[N.teams] : null;
            if (ts) for (var i = 0; i < ts.length; i++) {
                var cv = ts[i] && N.teamCamp ? ts[i][N.teamCamp] : null;
                if (typeof cv === "number") set.push(cv);
            }
        } catch (e1) {}
        if (set.length) {
            for (var k in h) {
                var w2;
                try { w2 = h[k]; } catch (e2) { continue; }
                if (typeof w2 !== "number") continue;
                for (var j = 0; j < set.length; j++) {
                    if (w2 === set[j]) { N.camp = k; log("camp field resolved by structure = " + N.hD + "." + k + "=" + w2); return w2; }
                }
            }
        }
        if (!N.dbgCamp) {
            N.dbgCamp = 1;
            var ks = [];
            try { for (var kk in h) ks.push(kk + ":" + (typeof h[kk])); } catch (e3) {}
            log("!! camp probe fail: ent." + N.hD + " keys=[" + ks.slice(0, 40).join(",") + "] world=" + N.world + " teams=" + N.teams);
        }
        return null;
    }
    function campProbe(ent) {
        var out = ["hD=" + N.hD, "camp=" + N.camp, "world=" + N.world];
        try {
            var h = ent && N.hD ? ent[N.hD] : null;
            if (h && N.camp) out.push("val=" + (typeof h[N.camp]) + ":" + h[N.camp]);
            out.push("my=" + (ent ? myCamp(ent) : "?"));
        } catch (e) {}
        return out.join(" ");
    }
    function isMine(ent) {
        var c = campOf(ent);
        if (c === null || c < 0) return false;
        return c === myCamp(ent);
    }
    function isMyHero(ent) {
        if (!isMine(ent) || !N.hx) return false;
        var x = ent[N.hx];
        if (!x) return false;
        /* 2026-10-26（res 1.1.10.0）：只信 bundle 源码采集到的 isPlayer 字段名
         * （唯一命中 `case "isPlayer": return t.hy.bnq` ⇒ N.hx=hy / N.isPlayer=bnq）；
         * 1.1.9.x 的写死候选 bmN/bnm 已不存于本版（hook-rules §24）。
         * 仍保留**严格真值**校验：只认 true/1，避免把别的布尔当命中。 */
        var cands = [N.isPlayer];
        for (var i = 0; i < cands.length; i++) {
            var k = cands[i];
            if (!k) continue;
            var v = x[k];
            if (v === true || v === 1) return true;
        }
        if (!N.dbgHero) {
            N.dbgHero = 1;
            var hk = [];
            try { for (var kk in x) hk.push(kk + ":" + (typeof x[kk])); } catch (e0) {}
            log("hero probe: sideKeys=[" + hk.join(",") + "] tried=" + cands.join("/"));
        }
        return false;
    }
    /* 时间缩放组件: 靠明文字段 hTimeScale 认, 认到后缓存槽名 */
    function tsComp(ent) {
        if (N.hy) return ent[N.hy];
        for (var k in ent) {
            var v;
            try { v = ent[k]; } catch (e) { continue; }
            if (v && typeof v === "object" && typeof v.hTimeScale === "number") {
                N.hy = k; log("timeScale comp = ent." + k); return v;
            }
        }
        return null;
    }

    /* ---------- 3. 按指纹找函数 ---------- */
    function findFn(obj, test) {
        if (!obj) return null;
        for (var k in obj) {
            var v;
            try { v = obj[k]; } catch (e) { continue; }
            if (typeof v !== "function") continue;
            if (test(srcOf(v))) return k;
        }
        return null;
    }

    /* ---------- 4. 战斗静态逻辑类: 伤害公式 / 技能门 / 技能消耗 / 死亡 ---------- */
    function tryLogic(K) {
        if (N.logicDone) return;
        /* 指纹必须同时含 maxApDefRate + baseCritDamage + atkEntity:
           atkEntity/target 是本作明文字段(钩子本身已依赖)。游戏包里属性初始化方法
           .bJV 也含前两个明文(AttributeParamTable 初始化)，只按前两个会被它抢占，
           真伤害公式(battleLogic .GD)再也挂不上 -> 秒杀/伤害倍率同时失效
           (2026-10-06 MuMu 自举 exports 线实测 formula=.bJV)。 */
        var fv = findFn(K, function (s) {
            return s.indexOf("maxApDefRate") >= 0 && s.indexOf("baseCritDamage") >= 0 &&
                   s.indexOf("atkEntity") >= 0;
        });
        if (!fv) {
            var dec = findFn(K, function (s) {
                return s.indexOf("maxApDefRate") >= 0 && s.indexOf("baseCritDamage") >= 0;
            });
            if (dec) log("!! damage-formula candidate ." + dec + " rejected by fingerprint (no atkEntity)");
            return;
        }
        N.logicDone = 1;
        N.FV = fv;
        log("battle-logic class hit: damage formula = ." + fv);

        /* ---- 挂钩(无条件安装, 开关在钩子内部读) ---- */
        var oFV = K[fv];
        K[fv] = function (ctx, hit, ratio, canCrit) {
            /* 过检测配对纪律: 先清空, 只有本笔伤害被我们放大过才在下面重新置位 */
            try { if (window.__starry_ac) window.__starry_ac.clearPending(); } catch (acErr) {}
            var d = oFV.apply(this, arguments);
            try {
                if (N.dbgFv === undefined) N.dbgFv = 0;
                if (N.dbgFv < 3 && hit) {
                    N.dbgFv++;
                    var _ak = [], _sk = [];
                    try { for (var _k in hit.atkEntity) _ak.push(_k); } catch (e1) {}
                    try { var _h = hit.atkEntity && hit.atkEntity[N.hD]; for (var _k2 in _h) _sk.push(_k2); } catch (e2) {}
                    log("formula call #" + N.dbgFv + " d=" + d +
                        " mineAtk=" + isMine(hit.atkEntity) + " mineTgt=" + isMine(hit.target) +
                        " hD=" + N.hD + " camp=" + N.camp + " ctx=" + N.teams + "/" + N.teamCamp +
                        " atkKeys=[" + _ak.join(",") + "]" +
                        " sideKeys=[" + _sk.join(",") + "]" +
                        " campVals[" + campProbe(hit.atkEntity) + "]");
                }
                if (hit && d > 0 && isMine(hit.atkEntity) && !isMine(hit.target)) {
                // 咽喉 = 唯一伤害数值公式(指纹: 明文 maxApDefRate + baseCritDamage)。
                // 在公式里放大而不是在扣血口放大, 飘字/结算数值才会同步。
                // 命中上下文的 atkEntity / target 本身是明文字段, 不需要发现。
                // 伤害倍率(滑块 1..50, 1=原倍率) 与 秒杀(x100) 共用这一个咽喉。
                var _f1 = false, _dm = 1, _d0 = d;   /* _d0 = 未放大值(过检测还原用) */
                try { _f1 = !!MOD.feature1; _dm = MOD.damage; } catch (readErr) {}
                if (_dm > 1) d = d * _dm;            /* 1 伤害倍率滑块 1..50, 1=原倍率 */
                if (_f1) d = Math.ceil(d * 100);     /* 2 秒杀: 玩家 x100 伤害 */
                if (d > 1e9) d = 1e9;                /* 上限, 防数值溢出 */
                /* 过检测(卡密包 L2578 的等价物): 把这一笔"本该上报的伤害"记下来,
                   hY 扣血口用把 hw.finalDamage / hw.finalHurtValue 还原成原值。 */
                try { if (window.__starry_ac) window.__starry_ac.noteDamage(_d0, d); } catch (acErr) {}
                }
            } catch (e) {}
            return d;
        };
        log("feature1 hooked");

        /* 技能消耗函数(指纹: 明文 maxCdTime + "充能 -= 1" 结构) */
        var fg = findFn(K, function (s) {
            return s.indexOf("maxCdTime") >= 0 && /\w+\.\w+\s*>\s*0\s*\)\s*\w+\.\w+\s*-=\s*1/.test(s);
        });
        if (fg) {
            N.Fg = fg;
            var fgs = srcOf(K[fg]), m;
            m = /\w+\.(\w+)\s*>\s*0\s*\)\s*\w+\.(\w+)\s*-=\s*1/.exec(fgs);
            if (m) { N.CZ = m[1]; N.CY = m[2]; }
            m = /\w+\.(\w+)\s*=\s*\w+\.maxCdTime/.exec(fgs);
            if (m) { N.cd = m[1]; }
            m = /if\s*\(\s*\w+\.(\w+)\s*\)\s*\{\s*\w+\.\w+\s*=\s*Math\.max\(0,/.exec(fgs);
            if (m) { N.Da = m[1]; }
            if (N.hp) {
                m = new RegExp("if\\s*\\(\\s*\\w+\\.(\\w+)\\s*\\)\\s*\\{\\s*var\\s+\\w+\\s*=\\s*\\w+\\." + N.hp + "\\s*,").exec(fgs);
                if (m) { N.Db = m[1]; }
            }
            log("skill cost = ." + fg + "  fields: maxCharge=" + N.CZ + " charge=" + N.CY +
                " cd=" + N.cd + " mpCost=" + N.Da + " hpCost=" + N.Db);
        } else log("!! skill cost fn NOT FOUND");

        /* 技能门(指纹: 用上面拿到的 CZ/CY/cd 拼出它特有的三元式) */
        var ff = null;
        if (N.CZ && N.CY && N.cd) {
            var re = new RegExp("\\w+\\." + N.CZ + "\\s*>\\s*0\\s*\\?\\s*\\w+\\." + N.CY + "\\s*<=\\s*0\\s*:\\s*\\w+\\." + N.cd);
            ff = findFn(K, function (s) { return re.test(s); });
            if (ff) {
                N.Ff = ff;
                var m2 = /null\s*==\s*\w+\s*\|\|\s*!\w+\.(\w+)/.exec(srcOf(K[ff]));
                if (m2) { N.Dc = m2[1]; }
                log("skill gate = ." + ff + "  unlockedField=" + N.Dc);
            } else log("!! skill gate NOT FOUND");
        }

        //// 无限技能
        // 咽喉1 = 技能门(战斗逻辑与 UI 技能按钮共用同一个函数, 改这里按钮同步亮起)
        // 咽喉2 = 释放消耗: 临时清零蓝耗/血耗, 事后把 cd 归零、充能补满
        if (ff) {
            var oFf = K[ff];
            K[ff] = function (ent, sk) {
                try {
                    if (MOD.feature3) {
                    if (sk && (!N.Dc || sk[N.Dc]) && isMine(ent)) return true;
                    }
                } catch (e) {}
                return oFf.apply(this, arguments);
            };
        }
        if (fg && N.cd) {
            var oFg = K[fg];
            K[fg] = function (ent, sk) {
                var on = false;
                try { on = !!(MOD.feature3 && sk && isMine(ent)); } catch (e) {}
                if (!on) return oFg.apply(this, arguments);
                var da = 0, db = 0, cz = 0, hasDa = false, hasDb = false, called = false, r;
                function restoreCosts() {
                    try { if (hasDa) sk[N.Da] = da; } catch (restoreDaErr) {}
                    try { if (hasDb) sk[N.Db] = db; } catch (restoreDbErr) {}
                }
                try {
                    if (N.Da) { da = sk[N.Da]; sk[N.Da] = 0; hasDa = true; }
                    if (N.Db) { db = sk[N.Db]; sk[N.Db] = 0; hasDb = true; }
                    if (N.CZ) cz = sk[N.CZ];
                    called = true;
                    r = oFg.apply(this, arguments);
                } catch (costErr) {
                    restoreCosts();
                    if (!called) {
                        try { return oFg.apply(this, arguments); } catch (fallbackErr) { return; }
                    }
                    return r;
                }
                restoreCosts();
                try { sk[N.cd] = 0; } catch (cdErr) {}
                try { if (N.CY && cz > 0) sk[N.CY] = cz; } catch (chargeErr) {}
                return r;
            };
        }
        log("feature3 hooked (gate=" + !!ff + " cost=" + !!fg + ")");

        //// 敌人0伤害的代码逻辑(第二处: 死亡例程)
        // 死亡例程内部直接把当前HP写 0, 绕过扣血口。
        // 指纹 = 明文 hitDirType + "把HP字段置0"
        var gj = null;
        /* 2026-10-25（1.1.9.1 实测）：名字发现会串类 —— 本轮 N.hp 落到 bhd，而死亡例程里真正
         * 被清 0 的字段是同一实体的 e.hw.bhC，于是旧「单名匹配」直接落空（无敌少一条挂点）。
         * 改成结构判据：函数体同时含明文 hitDirType 且把某个短字段置 0（实测唯一命中 HQ：
         *   e.hw.bhC = 0; e.hF.blj = 1; ... hitDirType: 0 ...），旧名匹配保留作兜底。 */
        gj = findFn(K, function (s) {
            if (s.indexOf("hitDirType") < 0) return false;
            /* 必须打在**属性组件**上：死亡例程实测形态 e.hw.bhC = 0。
             * 只写 /\.\w+ = 0;/ 会先命中击退例程 FX（1.1.9.1 实测误挂 FX）。 */
            return /\.hw\.[A-Za-z_$][\w$]{1,4}\s*=\s*0\s*;/.test(s);
        });
        if (!gj && N.hp) {
            var reHp = new RegExp("\\.\\s*" + N.hp + "\\s*=\\s*0\\s*;");
            gj = findFn(K, function (s) { return s.indexOf("hitDirType") >= 0 && reHp.test(s); });
        }
        if (gj) {
            N.GJ = gj;
            var oGJ = K[gj];
            K[gj] = function (ent) {
                try {
                    if (MOD.feature2) {
                    if (isMine(ent)) return;
                    }
                } catch (e) {}
                return oGJ.apply(this, arguments);
            };
            log("feature2 death-route hooked = ." + gj);
        } else log("!! death route NOT FOUND (hp=" + N.hp + ")");
    }

    /* ---------- 5. 实体类: 唯一扣血口 ---------- */
    /* ☠ 只用 lockMinHP + finalHurtValue 会撞车: 属性组件有个"清零"方法
       (this.finalDamage = 0; this.finalHurtValue = 0; this.lockMinHP = 0;)
       三个词全含, 而且它所在模块【比实体模块先加载】→ 会把 entDone 占掉,
       真正的扣血口就再也挂不上(实机表现: 只有无敌不生效)。
       判据必须是【当前HP 减 伤害 再 clamp 到 lockMinHP】这个减法。 */
    var RE_APPLY = /(\w+)\.(\w+)\s*=\s*Math\.max\(\1\.\2\s*-\s*\w+,\s*\1\.lockMinHP\)/;
    function tryEntity(Cls) {
        if (N.entDone || !Cls.prototype) return;
        var hx = findFn(Cls.prototype, function (s) {
            return s.indexOf("finalHurtValue") >= 0 && RE_APPLY.test(s);
        });
        if (!hx) {
            /* 撞车时留一条线索, 便于以后排查 */
            var decoy = findFn(Cls.prototype, function (s) {
                return s.indexOf("lockMinHP") >= 0 && s.indexOf("finalHurtValue") >= 0;
            });
            if (decoy) log("skip decoy .prototype." + decoy + " (no hp-subtraction)");
            return;
        }
        N.entDone = 1;
        N.hX = hx;
        /* 过检测(E8): baseline 用 RE_APPLY 仲裁出的 hY 才是真扣血口, 把它交给 AC 层。
        AC 必须包在最内层(此处早于下面 var oHX = Cls.prototype[hx];):
          feature2 关: baseline wrapper -> AC wrapper -> 原 hY  ⇒ AC 正常记账/还原
          feature2 开: baseline 提前 return ⇒ AC 不执行 ⇒ 不会凭空写累加器
        (若反序, feature2 开时 AC 先加 pd.orig 再被 baseline 清零, 累加器会被污染。) */
        try { if (window.__starry_ac) window.__starry_ac.adoptHit(Cls, hx); } catch (acErr) {}
        log("entity class hit: damage-apply = .prototype." + hx);

        var oHX = Cls.prototype[hx];
        Cls.prototype[hx] = function (atk, dmg, hit) {
            try {
                if (MOD.feature2) {
                //// 敌人0伤害的代码逻辑
                // 咽喉 = 全局唯一扣血口(指纹: 明文 lockMinHP + finalHurtValue)。
                //        普攻 / DOT / 反伤 / 技能自扣血 / GM 全部走这里。
                if (isMine(this)) {
                    if (hit) hit.finalDamage = 0;      /* 飘字显示 0(明文字段) */
                    return;
                }
                }
            } catch (e) {}
            return oHX.apply(this, arguments);
        };
        log("feature2 hooked");
    }

    /* ---------- 6. 移动系统: 移速 ---------- */
    function trySystem(Cls) {
        if (N.sysDone || !Cls.prototype) return;
        var up = Cls.prototype.update;                 /* update 是明文 ECS API 名 */
        if (typeof up !== "function") return;
        var s = srcOf(up);
        if (s.indexOf("worldTimeScale") < 0 || s.indexOf("hitPauseTimeScale") < 0 || s.indexOf("hTimeScale") < 0) return;
        N.sysDone = 1;
        log("movement system hit");

        var oUp = up;
        Cls.prototype.update = function (list) {
            try { if (window.__starry_ac) window.__starry_ac.tick(this.dt); } catch (acErr) {}
            try {
                if (this.context && N.ctxKey && this.context[N.ctxKey]) CTX = this.context[N.ctxKey];
            } catch (e) {}
            var touched = null;
            try {
                if (MOD.speed > 1) {
                //// 移速
                // 水平位移 = 速度 * (dt * worldTimeScale * hitPauseTimeScale * hTimeScale)。
                // 进 update 前把我方英雄的 hTimeScale 乘 3、出来还原, 等价于把水平
                // 步长放大 3 倍;垂直/跳跃走 vTimeScale 分支, 不受影响。
                if (list && list.length) for (var i = 0; i < list.length; i++) {
                    var ent = list[i];
                    if (!isMyHero(ent)) continue;
                    var hy = tsComp(ent);
                    if (!hy) continue;
                    var _sp = (MOD.speed > 0 ? MOD.speed : 1); hy.hTimeScale = hy.hTimeScale * _sp;
                    if (!touched) touched = [];
                    touched.push([ hy, _sp ]);
                }
                }
            } catch (e) {}
            try {
                return oUp.apply(this, arguments);
            } finally {
                if (touched) for (var j = 0; j < touched.length; j++) {
                    try { touched[j][0].hTimeScale = touched[j][0].hTimeScale / (touched[j][1] || 1); } catch (e) {}
                }
            }
        };
        log("feature4 hooked");
    }


    /* ---------- 6b. 自动战斗特权: 仅在开关开启时放行 AUTO_BATTLE 判定 ---------- */
    /* 指纹只依赖混淆器不动的明文: 枚举成员 AUTO_BATTLE / BuffTableEnum。
     * 2026-09-21 热更(res 1.0.98.10)把 buff 管理器方法改名为 bXP, 旧指纹写死 "bVs(" 后全部落空;
     * 2026-10-05 本覆盖文件漏同步该修法 -> 1.0.3 正式包在热更 res 1.1.4.0 上再次失效($CBN)。
     * 现在两级指纹: 主指纹 BuffTableEnum.AUTO_BATTLE 结构, 兜底指纹 = 短函数里
     * 直接 return 一次 buff 查询调用; 并对"含 AUTO_BATTLE 但被拒"的函数留日志。 */
    function tryAuto(Cls) {
        if (N.autoDone || !Cls || !Cls.prototype) return;
        var f = findFn(Cls.prototype, function (s) {
            return s.indexOf("AUTO_BATTLE") >= 0 && s.indexOf("BuffTableEnum") >= 0 &&
                   s.indexOf("return") >= 0 && s.length < 600;
        });
        if (!f) {
            f = findFn(Cls.prototype, function (s) {
                return s.indexOf("AUTO_BATTLE") >= 0 && s.length < 400 &&
                       /return\s+[\w$.]+\.[\w$]+\s*\(/.test(s);
            });
        }
        if (!f) {
            var cand = findFn(Cls.prototype, function (s) { return s.indexOf("AUTO_BATTLE") >= 0; });
            if (cand) {
                var clen = -1;
                try { clen = srcOf(Cls.prototype[cand]).length; } catch (e) {}
                log("!! auto-battle candidate .prototype." + cand + " rejected by fingerprint len=" + clen);
            }
            return;
        }
        if (!Object.prototype.hasOwnProperty.call(Cls.prototype, f)) return;
        var orig = Cls.prototype[f];
        if (typeof orig !== "function") return;
        N.autoDone = 1;
        N.autoFn = f;
        Cls.prototype[f] = function () {
            try {
                if (MOD.feature5) {
                    if (!N.autoGranted) { N.autoGranted = 1; log("auto-battle privilege granted"); }
                    return true;
                }
            } catch (e) {}
            return orig.apply(this, arguments);
        };
        log("auto-battle privilege hooked = .prototype." + f);
    }


    /* ---------- 7. 启动: 先扫全部模块源码取名, 再懒包装目标模块 ---------- */

    /* ---------- 6c. 结算面板打开时机（自动出售/分解/穿戴/兵底重复的触发器） ----------
     * 2026-10-26（res 1.1.10.0）：fguiMgr 的「按名取面板实例」链已完全消失
     * （main 包里 cUU.bdG 那个短名已改作它用）⇒ 旧轮询方式只能永远拿到 null
     * （实机表现：panels=0，自动出售/分解/穿戴永不触发，自动重复仅靠游戏自带设置位）。
     * 改成钩【面板自己的打开方法】：指纹只用 fgui 子节点名 btnList/btn_again/btn_toNext
     * （这些名字写在 UI 资产里，混淆器不动）。命中后往 window.__starry_fin 写一个事件，
     * 自动化层（auto_layer.js）只读这个事件，不再自己去翻 UI 管理器。 */
    var PANEL_N = 0;
    var PANEL_HOOKED = 0;
    function tryPanel(Cls) {
        if (!Cls || !Cls.prototype || Cls.__starryPanelHooked) return;
        var op = findFn(Cls.prototype, function (s) {
            /* battleMgr 是明文管理器名：同模块的 onUILoad 也带三个 btn 名，只有打开方法带它 */
            return s.indexOf("battleMgr") >= 0 && s.indexOf("btn_toNext") >= 0 &&
                   s.indexOf("btn_again") >= 0 && s.indexOf("btnList") >= 0;
        });
        if (!op) return;
        var orig = Cls.prototype[op];
        if (typeof orig !== "function") return;
        try { Cls.__starryPanelHooked = 1; } catch (e0) {}
        PANEL_HOOKED = 1;
        Cls.prototype[op] = function () {
            var r = orig.apply(this, arguments);
            try {
                PANEL_N++;
                var tk = null;
                try { tk = findFn(this, function (s) { return s.indexOf("auto_btn_again") >= 0; }); } catch (e2) {}
                window.__starry_fin = { n: PANEL_N, t: Date.now(), inst: this, again: tk || "" };
            } catch (e) {}
            return r;
        };
        log("result panel open hook = .prototype." + op + " (btnList/btn_again/btn_toNext)");
    }
    var srcs = {}, id, nmod = 0;
    for (id in REG) {
        if (!REG[id] || typeof REG[id][0] !== "function") continue;
        var s0 = srcOf(REG[id][0]);
        if (s0) { srcs[id] = s0; nmod++; }
    }
    log("boot: " + nmod + " modules, toString " + (nmod > 0 ? "OK" : "UNAVAILABLE — 自愈无法工作"));
    /* 2026-10-25（1.1.9.1 实测）：短名必须**在战斗包里采**。全局采集会被 game/main 包里
     * 同名形态抢走（本轮实测采到 hx=hy.bmN / camp=hE.blx / ctx=bnR.../hp=bhd，而战斗包真名是
     * hy.bnm / hE.blW / boq.../bhC）⇒ isMine() 判反 ⇒ 伤害倍率/秒杀/无敌/无限技能全部静默失效
     * （dmg hit pending=none 就是这个）。这里只从含「伤害公式三指纹」的模块采集，采不到再回退全局。 */
    /* 2026-10-26（res 1.1.10.0 静态取证 + 实机 ld8 日志）：上面那条「只看三指纹模块」的规则
     * 在 1.1.10.0 上适得其反 —— 伤害公式模块只剩 atkEntity，实体/阵营/血量/世界引用全在
     * **别的模块**里（实测 harvest scope=$KC 单个模块 ⇒ names: camp/hp/ctx/world 全 undefined
     * ⇒ isMine 恒 false，四项静默全灭）。
     * 现在改成：本 bundle 内**全量模块**两轮采集，含战斗明文的模块排前面（防串类）。
     * N 是 per-bundle 的（MOD_boot 每次被某个包调用就新建一份），所以全量扫也不会把
     * game/main 包里的同名形态采进战斗包这一份（跨包串名才是 2026-10-25 那次真因）。 */
    var harvestIds = [], _restIds = [];
    for (id in srcs) {
        var _hs = srcs[id];
        if (_hs.indexOf("maxApDefRate") >= 0 || _hs.indexOf("baseCritDamage") >= 0 ||
            _hs.indexOf("atkEntity") >= 0 || _hs.indexOf("lockMinHP") >= 0 ||
            _hs.indexOf("hitPauseTimeScale") >= 0 || _hs.indexOf("isPlayerCamp") >= 0) harvestIds.push(id);
        else _restIds.push(id);
    }
    harvestIds = harvestIds.concat(_restIds);
    for (var _hi = 0; _hi < harvestIds.length; _hi++) harvestA(srcs[harvestIds[_hi]]);
    for (var _hj = 0; _hj < harvestIds.length; _hj++) harvestB(srcs[harvestIds[_hj]]);
    log("harvest scope: " + harvestIds.join(","));
    MOD_boot.names = N;                /* 排查用: 控制台看 MOD_boot.names */
    /* 2026-10-25（1.1.9.1）：自动战斗的解锁判定是 buff 查询 bZF(BuffTableEnum.AUTO_BATTLE)，
     * 枚举值运行时不一定能遍历到（实测 findEnum 失败）。这里直接从**模块源码文本**抠数字：
     * 打包产物里枚举表是字面量 AUTO_BATTLE: <n>，一次 regex 即可，不写死数字。 */
    if (N.autoBattleEnum === undefined) {
        for (id in srcs) {
            var _mm = /AUTO_BATTLE\s*[:=]\s*(\d+)/.exec(srcs[id]);
            if (_mm) { N.autoBattleEnum = Number(_mm[1]); break; }
        }
        for (id in srcs) {
            var _mf = /MONTH_CARD_OPEN\s*[:=]\s*(\d+)/.exec(srcs[id]);
            if (_mf) { N.monthCardEnum = Number(_mf[1]); break; }
        }
        log("auto-battle enum from src: " + (N.autoBattleEnum === undefined ? "NOT FOUND" : N.autoBattleEnum) +
            " monthCardOpen=" + (N.monthCardEnum === undefined ? "NOT FOUND" : N.monthCardEnum));
    }
    log("names: hx=" + N.hx + "." + N.isPlayer + "  camp=" + N.hD + "." + N.camp +
        "  ctx=" + N.ctxKey + "/" + N.teams + "/" + N.teamCamp + "  hp=" + N.hp + "  world=" + N.world);

    function scan(exp) {
        if (!exp) return;
        for (var k in exp) {
            var V;
            try { V = exp[k]; } catch (e) { continue; }
            /* 2026-10-25：cocos bundle 常见 exports.default = 类 / 命名空间对象，旧扫描只认
             * typeof === "function"，整包被静默跳过（1.1.9.1 自动战斗所在导出就是这种形态
             * ⇒ auto=NO）。这里拆一层 default 再判函数。 */
            if (typeof V !== "function") {
                try { if (V && typeof V["default"] === "function") V = V["default"]; } catch (e2) {}
            }
            if (typeof V !== "function") continue;
            try { tryLogic(V); } catch (e) { log("tryLogic ERR: " + e); }
            try { tryEntity(V); } catch (e) { log("tryEntity ERR: " + e); }
            try { trySystem(V); } catch (e) { log("trySystem ERR: " + e); }
            try { tryAuto(V); } catch (e) { log("tryAuto ERR: " + e); }
            try { tryPanel(V); } catch (e) { log("tryPanel ERR: " + e); }
        }
    }
    /* 过检测: main bundle 独有明文 sealMethods(实测仅 main/index.js 命中, 宿主模块 $LZ),
       用它判断"入口表是 main" —— 此时只跑闸门层(见 E2/E3), 不跑伤害/移速/自动战斗功能扫描。
       原生 IsTargetBundle 已放行 assets/main/index.(2026-10-07), 否则这段永远进不来。 */
    var _gateOnly = false;
    for (id in srcs) { if (srcs[id] && srcs[id].indexOf("sealMethods") >= 0) { _gateOnly = true; break; } }
    if (_gateOnly) log("bundle=MAIN (sealMethods) -> gate-only scan");
    var targets = [];
    for (id in srcs) {
        var s = srcs[id];
        /* 2026-10-26：结算面板模块（自动出售/分解/穿戴/兵底重复的触发时机）只带 fgui 子节点名
         * btn_toNext，四个战斗明文一个都没有 ⇒ 过滤里必须加上它，否则面板永远进不了扫描列表。 */
        if (_gateOnly) {
            /* main 包: 只包装含闸门明文(sealMethods 是 main 独有标记)的模块 */
            if (s.indexOf("sealMethods") < 0 && s.indexOf("ANTI_CHEAT_ENABLED") < 0) continue;
        } else if (s.indexOf("maxApDefRate") < 0 && s.indexOf("lockMinHP") < 0 &&
            s.indexOf("hitPauseTimeScale") < 0 && s.indexOf("AUTO_BATTLE") < 0 &&
            s.indexOf("btn_toNext") < 0 &&
            s.indexOf("ANTI_CHEAT_ENABLED") < 0 && s.indexOf("finalHurtValue") < 0) continue;
        targets.push(id);
        (function (mid, entry) {
            var orig = entry[0];
            entry[0] = function (req, mod, exp) {
                orig.apply(this, arguments);
                var _ex = mod && mod.exports ? mod.exports : exp;
                /* main 包(gate-only): 只把 exports 交给闸门层, 不跑 tryLogic/tryEntity/trySystem/tryAuto,
                   否则 main 里的 $VN(maxApDefRate + 18 处 AUTO_BATTLE) 会被误钩。 */
                if (!_gateOnly) { try { scan(_ex); } catch (e) { log("scan ERR: " + e); } }
                try { if (window.__starry_ac) window.__starry_ac.scan(_ex, mid); } catch (e) { log("ac.scan ERR: " + e); }
            };
        })(id, REG[id]);
    }
    log("wrapped " + targets.length + " target modules: " + targets.join(","));

    /* 全部模块跑完后报一次总账(入口列表会立刻同步执行完所有模块) */
    try {
        setTimeout(function () {
            log("REPORT  formula=" + N.FV + " apply=" + N.hX + " gate=" + N.Ff +
                " cost=" + N.Fg + " death=" + N.GJ + " movesys=" + (N.sysDone ? "yes" : "NO") +
                " auto=" + (N.autoDone ? "yes" : "NO") + " panelHook=" + (PANEL_HOOKED ? "yes" : "NO"));
            log("REPORT  MOD=" + (typeof MOD === "undefined" ? "undefined" :
                ("f1=" + MOD.feature1 + " f2=" + MOD.feature2 + " f3=" + MOD.feature3 + " f4=" + MOD.feature4 + " f5=" + MOD.feature5 + " dmg=" + MOD.damage)));
        }, 3000);
    } catch (e) {}
}

window.__starry_capture = function (reg) {
  try { MOD_boot(reg); } catch (e) {
    try { if (window.__starry_nlog) window.__starry_nlog("[MODMOD] boot ERR: " + e); } catch (ignored) {}
  }
  try { if (window.__starry_ac) window.__starry_ac.capture(reg); } catch (e) {
    try { if (window.__starry_nlog) window.__starry_nlog("[AC] capture ERR: " + e); } catch (ignored) {}
  }
  return reg;
};

try {
  if (window.__starry_nlog) window.__starry_nlog("[MODMOD] capture installed");
} catch (e) {}
