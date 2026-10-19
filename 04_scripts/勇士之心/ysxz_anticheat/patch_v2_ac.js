

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
    /* 杂项 */
    pending: null, note: "", reg: null, installN: 0, loopStarted: false
  };
  window.__starry_ac = AC;

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
      if (AC.clampArmed && wall > 0) {
        var realMs = wall;
        if (realMs >= cur * 0.5) {        /* 墙钟明显偏小 = 我们没量准，宁可不改 */
          want = Math.max(1000, Math.min(realMs, cur));
        } else {
          AC.note = "clamp skipped (wall " + Math.round(wall) + " << logical " + cur + ")";
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
    if (typeof dt === "number" && dt > 0 && dt < 1) { AC.tickN++; AC.wallMs += dt * 1000; }
    else AC.tickBad++;
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
  Object.defineProperty(M, "feature5", { get: function () { return R(6); } });   /* 自动战斗 */
  Object.defineProperty(M, "speed", { get: function () { return V(5); } });      /* 移动速度 1..10 */
  Object.defineProperty(M, "damage", { get: function () { return V(1); } });     /* 伤害倍率 1..50 */
  /* 2026-10-07 新增功能(卡密包同名功能复刻)。序号=菜单位置, 必须与 kYsxz_Features 严格一致:
       7  解锁金卡(已并入「基础功能」分类)  /  8  Category_自动化(分组占位)
       9  自动重复挑战 / 10 自动出售 / 11 自动分解 / 12 自动穿戴
     全部由 automation 层(auto_layer.js)消费, 过检测层不读这些。 */
  Object.defineProperty(M, "feature6",  { get: function () { return R(7); } });  /* 解锁金卡 */
  Object.defineProperty(M, "feature7",  { get: function () { return R(9); } });  /* 自动重复挑战 */
  Object.defineProperty(M, "feature8",  { get: function () { return R(10); } }); /* 自动出售 */
  Object.defineProperty(M, "feature9",  { get: function () { return R(11); } }); /* 自动分解 */
  Object.defineProperty(M, "feature10", { get: function () { return R(12); } }); /* 自动穿戴 */
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
        if (s.indexOf("lockMinHP") < 0) return;           /* 只认实体/属性模块 */
        /* 必须遍历所有匹配: 同一模块里可能先出现 this.context.<ctx>(系统的写法) */
        var re = new RegExp("this\\.([A-Za-z_$][\\w$]*)\\." + N.ctxKey + "\\.", "g"), m;
        while ((m = re.exec(s)) !== null) {
            if (m[1] !== "context") { N.world = m[1]; return; }
        }
    }

    /* ---------- 2. 敌我判定(全部基于发现到的名字, 带降级) ---------- */
    function ctxOf(ent) {
        var w = N.world && ent ? ent[N.world] : null;
        var c = w ? w[N.ctxKey] : null;
        return c || CTX;
    }
    function myCamp(ent) {
        var c = ctxOf(ent);
        var t0 = c && N.teams && c[N.teams] ? c[N.teams][0] : null;
        var v = t0 && N.teamCamp ? t0[N.teamCamp] : null;
        if (typeof v === "number" && v >= 0) return v;
        return 1;                 /* 降级: 本作阵营 1 = 我方(gF() 取 camp!=1 当敌人) */
    }
    function isMine(ent) {
        if (!ent || !N.hD || !N.camp) return false;
        var h = ent[N.hD];
        if (!h) return false;
        var c = h[N.camp];
        if (typeof c !== "number" || c < 0) return false;
        return c === myCamp(ent);
    }
    function isMyHero(ent) {
        if (!isMine(ent) || !N.hx) return false;
        var x = ent[N.hx];
        return !!(x && x[N.isPlayer]);
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
        if (N.hp) {
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
    var srcs = {}, id, nmod = 0;
    for (id in REG) {
        if (!REG[id] || typeof REG[id][0] !== "function") continue;
        var s0 = srcOf(REG[id][0]);
        if (s0) { srcs[id] = s0; nmod++; }
    }
    log("boot: " + nmod + " modules, toString " + (nmod > 0 ? "OK" : "UNAVAILABLE — 自愈无法工作"));
    for (id in srcs) harvestA(srcs[id]);
    for (id in srcs) harvestB(srcs[id]);
    MOD_boot.names = N;                /* 排查用: 控制台看 MOD_boot.names */
    log("names: hx=" + N.hx + "." + N.isPlayer + "  camp=" + N.hD + "." + N.camp +
        "  ctx=" + N.ctxKey + "/" + N.teams + "/" + N.teamCamp + "  hp=" + N.hp + "  world=" + N.world);

    function scan(exp) {
        if (!exp) return;
        for (var k in exp) {
            var V;
            try { V = exp[k]; } catch (e) { continue; }
            if (typeof V !== "function") continue;
            try { tryLogic(V); } catch (e) { log("tryLogic ERR: " + e); }
            try { tryEntity(V); } catch (e) { log("tryEntity ERR: " + e); }
            try { trySystem(V); } catch (e) { log("trySystem ERR: " + e); }
            try { tryAuto(V); } catch (e) { log("tryAuto ERR: " + e); }
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
        if (_gateOnly) {
            /* main 包: 只包装含闸门明文(sealMethods 是 main 独有标记)的模块 */
            if (s.indexOf("sealMethods") < 0 && s.indexOf("ANTI_CHEAT_ENABLED") < 0) continue;
        } else if (s.indexOf("maxApDefRate") < 0 && s.indexOf("lockMinHP") < 0 && s.indexOf("hitPauseTimeScale") < 0 && s.indexOf("AUTO_BATTLE") < 0 &&
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
                " cost=" + N.Fg + " death=" + N.GJ + " movesys=" + (N.sysDone ? "yes" : "NO") + " auto=" + (N.autoDone ? "yes" : "NO"));
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
