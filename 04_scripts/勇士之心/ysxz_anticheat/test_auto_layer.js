#!/usr/bin/env node
/* ============================================================================
 * 星空自动化层 离线验收台 (不需要真机, 不依赖 Cocos)
 *
 * 为什么必须有这个：这一层全是【联网协议 + 定时器 + 原型包装】，任何一处挂错
 * 都不会抛异常，只会"什么都不发生"。上一轮过检测层就是这么静默失效的。
 * 所以这里把时钟、菜单开关、$eZ 全套都做成可控桩，逐条断言可观测的副作用。
 *
 * 覆盖:
 *   T1  装载 + 版本仲裁(同版第二次装载必须让位, 不能双份 tick)
 *   T2  解锁金卡: 开关关不装钩子 / 开关开包装 fDc / 只放行 gold / 开关实时生效 / 证据日志封顶
 *   T3  自动重复挑战: 游戏内 autoBattleFlag 位标记(幂等, 保留其它位)
 *   T4  自动重复挑战: 结算面板 efi() 点击 + token/scheduled/handled 三重去重
 *   T5  出售/分解: 品质取自游戏内勾选 + 空兜底 [1,2,3] + 总账日志
 *   T6  自动穿戴: ftA 空则不动 / 有则 fvH(list) / resolved code 15 重试 / rejected 不重试(照抄原版)
 *   T7  时序铁律: 只在"战斗 -> 城镇"那一拍触发, >=5s 节流, 900/2100 错峰
 *   T8  面板辅助(panelCfg/panelOpen/panelCandidates)按他的名字走
 *   T9  __AUTO_STATE__ 字段齐全
 *   T10 错误面: 没有 $eZ / 缺 API 时不抛异常, 只记 lastErr
 * 用法: node test_auto_layer.js [auto_layer.js]
 * ========================================================================== */
"use strict";
const fs = require("fs");
const path = require("path");

const HERE = __dirname;
const AUT = process.argv[2] || path.join(HERE, "auto_layer.js");

let pass = 0, fail = 0;
function chk(good, msg, extra) {
  if (good) { pass++; console.log("  ok   " + msg); }
  else { fail++; console.log("  FAIL " + msg + (extra === undefined ? "" : "   -> " + JSON.stringify(extra))); }
}
const SRC = fs.readFileSync(AUT, "utf8");
const flush = () => new Promise((r) => setTimeout(r, 0));   // 冲掉微任务队列(真机 setTimeout)

/* ------------------------------------------------------ 可控沙箱(假时钟) -- */
function makeSandbox() {
  let now = 1000000;
  let tid = 0;
  const timers = [];

  const win = {};
  win.window = win;
  /* 注意：log() 会【同时】写 window.__starry_nlog 与 window.MOD_LOG。
   * 若把两者指向同一个数组，每条日志会被记两次（上一轮踩过，误报 [6,6,6]）。
   * 这里刻意分成两个独立通道：断言一律读 NLOG。 */
  const NLOG = [];
  win.__starry_nlog = (s) => NLOG.push(s);
  win.MOD_LOG = [];                       // 另一条通道，只用于确认它也能收到
  win.console = { log() {} };             // 关掉 console 噪声（log() 里裸用的 console 仍是真实全局）

  const sw = {};
  const MOD = {};
  const FEATS = { feature6: "gold", feature7: "repeat", feature8: "sell", feature9: "deco", feature10: "wear" };
  Object.keys(FEATS).forEach((k) => {
    Object.defineProperty(MOD, k, { get: () => !!sw[FEATS[k]] });
  });
  win.MOD = MOD;

  const has = (re) => NLOG.some((s) => re.test(s));
  const count = (re) => NLOG.filter((s) => re.test(s)).length;

  function fakeSetTimeout(fn, ms) {
    const t = { fn, at: now + (ms || 0), id: ++tid };
    timers.push(t);
    return t.id;
  }
  const FakeDate = { now: () => now };

  // 让被测代码用的 setTimeout / Date 都指向桩（否则会拿到 Node 的真实全局）
  new Function("window", "setTimeout", "Date", SRC)(win, fakeSetTimeout, FakeDate);
  const A = win.__starry_auto;
  const state = () => (win.__AUTO_STATE__ ? win.__AUTO_STATE__() : null);

  /* 推进假时钟。按 时间 -> 注册顺序 依次执行到期回调（含回调里新注册的）。 */
  function advance(ms) {
    const target = now + ms;
    let guard = 0;
    for (;;) {
      let pick = -1;
      for (let i = 0; i < timers.length; i++) {
        if (timers[i].at > target) continue;
        if (pick < 0 || timers[i].at < timers[pick].at ||
            (timers[i].at === timers[pick].at && timers[i].id < timers[pick].id)) pick = i;
      }
      if (pick < 0) break;
      if (++guard > 20000) throw new Error("定时器 runaway");
      const t = timers.splice(pick, 1)[0];
      now = t.at;
      t.fn();
    }
    now = target;
  }
  const pendingTimers = () => timers.length;

  return { win, A, sw, state, advance, pendingTimers, has, count, NLOG, MOD_LOG: win.MOD_LOG,
           getNow: () => now };
}

/* ---------------------------------------------------------------- 游戏桩 -- */
function makeGame(sb, opt) {
  opt = opt || {};
  const calls = { sell: [], deco: [], wear: [] };
  const eZ = {};
  eZ.battleMgr = { bEu: { no: () => !!opt.inBattle } };

  const lm = { cms: { autoBattleFlag: opt.flag === undefined ? 0 : opt.flag } };
  lm.cnp = (q) => (opt.sellQ || []).indexOf(q) >= 0;
  lm.cnq = (q) => (opt.decoQ || []).indexOf(q) >= 0;
  eZ.localMgr = lm;

  eZ.bag = {
    bYK: (q) => { calls.sell.push(q.slice()); return 0; },
    bYJ: (q) => { calls.deco.push(q.slice()); return 0; }
  };
  eZ.equip = {
    ftA: () => (opt.wearList || []).slice(),
    fvH: function (list) {
      calls.wear.push(list.slice());
      if (opt.wearRejectWith !== undefined) return Promise.reject({ code: opt.wearRejectWith });
      if (opt.wearCode !== undefined) return Promise.resolve({ code: opt.wearCode });
      return Promise.resolve({ code: 0 });
    }
  };

  let top = { vJ: { cUV: opt.panel || "" } };
  eZ.fguiMgr = { cTU: () => top, cTK: () => !!opt.panelOpen };

  // 金卡：data 是实例，fDc 在【原型】上（照抄真机形态）
  const cardData = {};
  const cardProto = { fDc: function (t) { return t === "silver"; } };
  Object.setPrototypeOf(cardData, cardProto);
  eZ.monCard = { fDn: cardData };

  sb.win.$eZ = eZ;
  sb.win.__require = (id) => {
    if (id === "$JX") return { $fb: { cvConfig: { panels: { AdventureFinishPanel: { n: 1 } } } } };
    return null;
  };
  return { eZ, calls, lm, cardProto, cardData,
           setTop: (t) => { top = t; } };
}

/* 1.1.6.0 真机形态：$eZ.monCard.fDs(明文 Basic/Premium/buffId 配置) + fDq 数据对象，
 * 原型上同时有 fDf(持有,boolean) / fDi(剩余秒数,number) / fDh(配置存在,含 findByKey)。
 * 函数体用 new Function 生成，保证 toString() 里带着真机同款明文锚点。 */
function makeGame116(sb) {
  const eZ = {};
  eZ.battleMgr = { bEu: { no: () => false } };
  eZ.localMgr = { cms: { autoBattleFlag: 0 }, cnp: () => false, cnq: () => false };
  eZ.bag = {};
  eZ.monCard = {};
  eZ.monCard.fDs = { Basic: { buffId: 11, goodsId: 80101 }, Premium: { buffId: 22, goodsId: 80102 } };
  eZ.buff = { bYZ: { bYS: () => false, bYU: () => undefined } };
  eZ.cfgMgr = { tbl: { BuffChildTable: { findByKey: () => null }, ShopGoodsTable: { findByKey: () => null } } };
  const mk = (body) => new Function("$eZ", "return function (cardType) {" + body + "};")(eZ);
  const fDf = mk('var e = "silver" === cardType ? $eZ.monCard.fDs.Basic.buffId : $eZ.monCard.fDs.Premium.buffId; return $eZ.buff.bYZ.bYS(e);');
  const fDi = mk('var e = "silver" === cardType ? $eZ.monCard.fDs.Basic.buffId : $eZ.monCard.fDs.Premium.buffId; var n = $eZ.buff.bYZ.bYU(e); return n ? 1 : 0;');
  const fDh = mk('var e = "silver" === cardType ? $eZ.monCard.fDs.Basic.buffId : $eZ.monCard.fDs.Premium.buffId; return null != $eZ.cfgMgr.tbl.BuffChildTable.findByKey(e);');
  const proto = { fDf, fDi, fDh, fDg: function (t) { return this.fDf(t) || this.fDf("gold"); } };
  const data = Object.create(proto);
  eZ.monCard.fDq = data;
  sb.win.$eZ = eZ;
  return { eZ, proto, data };
}

/* 战斗信号通道②：过检测层的 tickSeen 在涨 = 在战斗里（真机已验证这条链是通的） */
function acTick(sb, n) { sb.win.__starry_ac = { tickSeen: n }; }

console.log("=== 星空自动化层 离线验收台 ===");
console.log("[src] " + AUT + "  " + SRC.length + " B\n");

(async function main() {

/* ================================================================ T1 ===== */
console.log("T1 装载 + 版本仲裁");
{
  const sb = makeSandbox();
  chk(!!sb.A && typeof sb.win.__AUTO_STATE__ === "function", "装载成功, __starry_auto / __AUTO_STATE__ 都在");
  chk(sb.win.__starry_auto.__v === 2, "版本号 __v === 2(1.1.6.0 结构化发现)");
  chk(sb.count(/auto layer v1 loaded/) === 1, "装载日志已打 1 条(不重复计数 = 两个日志通道是独立的)", sb.count(/auto layer v1 loaded/));
  chk(sb.MOD_LOG.some((s) => /auto layer v1 loaded/.test(s)), "MOD_LOG 通道也收到了同一条");
  chk(sb.pendingTimers() === 1, "主循环已挂上一条 1s 定时器(常驻驱动)", sb.pendingTimers());

  // 同版第二次装载必须让位（否则双份 tick，倒计时/去重全乱）
  new Function("window", "setTimeout", "Date", SRC)(sb.win, () => 0, { now: () => 0 });
  chk(sb.count(/auto layer v1 loaded/) === 1, "同版本第二次装载让位(装载日志仍只有 1 条)");
  chk(sb.pendingTimers() === 1, "第二次装载没有多挂一条循环定时器", sb.pendingTimers());
}

/* ================================================================ T2 ===== */
console.log("\nT2 解锁金卡");
{
  const sb = makeSandbox();
  const g = makeGame(sb, {});                        // 全程用同一个 $eZ（钩子挂在它的原型上）
  sb.advance(1000);
  chk(sb.A.goldHooked === false, "开关关: 不装钩子(goldHooked=false)");
  chk(sb.has(/gold-card hooked/) === false, "开关关: 不打 hooked 日志");
  chk(g.cardProto.fDc("gold") === false, "开关关: fDc('gold') 走原逻辑返回 false");

  sb.sw.gold = true;
  sb.advance(1000);
  chk(sb.A.goldHooked === true, "开关开: 钩子装上");
  chk(sb.A.goldKey === "monCard.fDn.fDc", "记录的方法名 = monCard.fDn.fDc(不是写死的短名)");
  chk(sb.has(/gold-card hooked: \$eZ\.monCard\.fDn\.fDc \[发现方式=legacy-name\]/), "打了 hooked 日志(带发现方式)");
  chk(g.cardProto.fDc.__starryGold === true, "包装器打了 __starryGold 戳");
  chk(g.cardProto.fDc.__starryGoldOrig !== undefined, "留了 __starryGoldOrig(原函数)");

  chk(g.cardProto.fDc("gold") === true, "fDc('gold') -> true (放行)");
  chk(sb.A.goldAllowN === 1, "放行计数 +1");
  chk(sb.has(/gold: 放行 gold 卡判定 \(第 1 次调用\)/), "放了第 1 条放行证据日志");
  chk(g.cardProto.fDc("silver") === true, "fDc('silver') 走原逻辑(原函数对 silver 返回 true)");
  chk(sb.A.goldAllowN === 1, "非 gold 卡类型不计数");

  // 开关实时生效（不是只认装载那一刻）
  sb.sw.gold = false;
  chk(g.cardProto.fDc("gold") === false, "开关关掉后 fDc('gold') 立刻回原值 false");
  sb.sw.gold = true;

  // 证据日志封顶 3 条
  for (let i = 0; i < 5; i++) g.cardProto.fDc("gold");
  chk(sb.count(/gold: 放行 gold 卡判定/) === 3, "放行证据日志封顶 3 条(不刷屏)", sb.count(/gold: 放行 gold 卡判定/));
  // 计数口径：第 1 次(gold) + 开关关掉那一次【不计】 + 这 5 次 = 6
  chk(sb.A.goldAllowN === 6, "计数照常累加(与日志封顶无关)", sb.A.goldAllowN);
}

/* =============================================================== T2b ===== */
/* 2026-10-19 真机回归：res 1.1.6.0 把该类的短名整体漂移
 *   monCard.fDn -> fDq   /   原型 fDc(持有,boolean) -> fDf
 *   同族混淆项: fDi(剩余秒数,number) / fDh(商品配置是否存在,含 findByKey)
 * 断言：结构+行为两级发现必须挑中 fDf，而不是靠旧短名，也不能挑到 number 那条。 */
console.log("\nT2b 解锁金卡 · 1.1.6.0 短名整体漂移(真实形态)");
{
  const sb = makeSandbox();
  const g = makeGame116(sb);
  sb.sw.gold = true;
  sb.advance(1000);
  chk(sb.A.goldHooked === true, "结构+行为两级发现挂上了钩子");
  chk(sb.A.goldHow === "struct+behaviour", "发现方式 = struct+behaviour(不是靠旧短名)", sb.A.goldHow);
  chk(sb.A.goldKey === "monCard.fDq.fDf", "定位到 fDq 的 fDf(持有判定)", sb.A.goldKey);
  chk(g.proto.fDf.__starryGold === true, "包装器打在 fDf 上");
  chk(g.proto.fDi.__starryGold === undefined, "没有误挂到 fDi(剩余秒数那条)");
  chk(g.proto.fDh.__starryGold === undefined, "没有误挂到 fDh(配置存在那条)");
  chk(g.proto.fDf("gold") === true, "fDf('gold') -> true(放行)");
  chk(g.proto.fDf("silver") === false, "fDf('silver') 走原逻辑(未持有时仍是 false)");
  chk(g.proto.fDi("gold") === 0, "fDi 未被改动(仍返回 number)");
  sb.sw.gold = false;
  chk(g.proto.fDf("gold") === false, "关掉开关 -> 立刻回原值 false");
}

/* =============================================================== T2c ===== */
/* 原型被冻结时不许假成功：goldHooked 必须是 false 且留一条 pin 告警。 */
console.log("\nT2c 解锁金卡 · 原型冻结 -> 不许假成功");
{
  const sb = makeSandbox();
  const g = makeGame116(sb);
  Object.freeze(g.proto);
  sb.sw.gold = true;
  sb.advance(1000);
  chk(sb.A.goldHooked === false, "写不进去 => goldHooked 保持 false(不假成功)");
  chk(sb.has(/warn: 解锁金卡: 定位到 monCard\.fDq\.fDf 但原型被冻结/), "留了 pin 告警", sb.MOD_LOG.slice(-3));
  chk(sb.A.goldKey === "", "没有登记 goldKey");
}

/* ============================================================== T2d ==== */
/* 诊断自身不许把功能锁死：第 20 秒在登录/选角阶段扫到"0 件"时不能定稿，
 * 否则进游戏后永远走"跳过:0 件可分解"这条分支。 */
console.log("\nT2d 分解诊断 · 空容器不定稿(不许自己把功能锁死)");
{
  const sb = makeSandbox();
  const g = makeGame(sb, {});
  const items = [];
  const box = { d: { bdn: (cb) => items.forEach(cb) } };
  g.eZ.bag.bXk = () => box;
  g.eZ.bag.bXh = () => ({ quality: 1 });
  g.eZ.bag.bXY = () => true;
  sb.sw.deco = true;
  sb.advance(20000);                              // 第 20 秒探容器（此时 0 件）
  chk(sb.A.sdFinal === 0, "空容器不定稿");
  chk(sb.A.sdDump === "wait", "标记为 wait", sb.A.sdDump);
  chk(sb.has(/还是空的/), "打了一条‘还是空的’");

  items.push({ gridId: 1, itemId: 100, amount: 1 });   // 进游戏后背包才有件
  acTick(sb, 100); sb.advance(1000);                   // 进战斗
  sb.advance(1000);                                    // 回城触发清理
  sb.advance(900);
  chk(sb.A.sdFinal === 1, "读到真实件数后定稿", sb.A.sdTotal);
  chk(sb.A.sdDecoable === 1, "可分解 1 件", sb.A.sdDecoable);
  chk(g.calls.deco.length === 1, "分解协议真的发出 1 次", g.calls.deco);
}

/* ================================================================ T3 ===== */
console.log("\nT3 自动重复挑战 · 游戏内位标记");
{
  const sb = makeSandbox();
  const g = makeGame(sb, { flag: 8 });               // 0b1000，只占别的位
  sb.sw.repeat = true;
  sb.advance(1000);
  chk(g.lm.cms.autoBattleFlag === 10, "开: bit1 置 1 且保留其它位 (8 -> 10)", g.lm.cms.autoBattleFlag);
  chk(sb.has(/repeat: 游戏内自动挑战位 置1 autoBattleFlag 8 -> 10/), "打了位标记变更日志");

  sb.advance(1000);                                  // 幂等
  chk(g.lm.cms.autoBattleFlag === 10 && sb.A.repeatFlagSync === 1, "再 tick 不重复写(幂等)");

  sb.sw.repeat = false;
  sb.advance(1000);
  chk(g.lm.cms.autoBattleFlag === 8, "关: bit1 清 0 且其它位仍在 (10 -> 8)", g.lm.cms.autoBattleFlag);

  // 本版本没有 lm.cms 时整段跳过，不许抛异常、更不许凭空造对象
  const sb2 = makeSandbox();
  const g2 = makeGame(sb2, {});
  delete g2.lm.cms;
  sb2.sw.repeat = true;
  sb2.advance(1000);
  chk(g2.lm.cms === undefined, "lm.cms 不存在 -> 不主动造对象(保守)");
  chk(sb2.A.errN === 0, "lm.cms 不存在 -> 不记错误", sb2.state().lastErr);
  chk(sb2.A.warnN === 1 && sb2.has(/warn: 自动重复挑战: \$eZ\.localMgr\.cms 不存在/),
      "lm.cms 不存在 -> 一次性告警(改名可被发现)", sb2.state().warns);
  sb2.advance(3000);
  chk(sb2.A.warnN === 1, "同一个缺失位只告警一次(不刷屏)");
}

/* ================================================================ T4 ===== */
console.log("\nT4 自动重复挑战 · 结算面板 efi() 点击 + 三重去重");
{
  const sb = makeSandbox();
  let clicks = 0;
  const g = makeGame(sb, { panel: "AdventureFinishPanel", panelOpen: true });
  const topA = { vJ: { cUV: "AdventureFinishPanel" }, efi: () => { clicks++; } };
  g.setTop(topA);                                    // 面板顶层就是那个带 efi 的控件
  sb.sw.repeat = true;

  sb.advance(1000);                                  // tick: 发现按钮, 排 1500ms 后点击
  chk(sb.A.repeatScheduled === topA, "发现「再次挑战」按钮并记入 scheduled");
  chk(sb.A.repeatToken === 1, "token 置 1");
  chk(clicks === 0, "此刻还没点(延时 1500ms)");

  sb.advance(1500);
  chk(clicks === 1, "1500ms 后点了一次 efi()", clicks);
  chk(sb.A.repeatClickN === 1, "点击计数 = 1");
  chk(sb.has(/repeat: 点击「再次挑战」 efi\(\) \(第 1 次\)/), "打了点击证据日志");
  chk(sb.A.repeatHandled === topA, "同一实例记入 handled(防重复点)");

  sb.advance(5000);                                  // 面板还开着，反复 tick
  chk(clicks === 1, "同一实例不再重复点击(去重生效)", clicks);

  // 控件换实例 -> 本次放弃（防点到重建后的别的按钮上）
  const sb2 = makeSandbox();
  let clicks2 = 0;
  const g2 = makeGame(sb2, { panel: "AdventureFinishPanel", panelOpen: true });
  g2.setTop({ vJ: { cUV: "AdventureFinishPanel" }, efi: () => { clicks2++; } });
  sb2.sw.repeat = true;
  sb2.advance(1000);                                 // scheduled
  g2.setTop({ vJ: { cUV: "AdventureFinishPanel" }, efi: () => { clicks2++; } });  // 面板重建
  sb2.advance(1500);
  chk(clicks2 === 0, "面板内控件换了实例 -> 放弃本次点击(不误点)");

  // 面板在延时期间关了 -> 不点
  const sb3 = makeSandbox();
  let clicks3 = 0;
  const g3 = makeGame(sb3, { panel: "AdventureFinishPanel", panelOpen: true });
  g3.setTop({ vJ: { cUV: "AdventureFinishPanel" }, efi: () => { clicks3++; } });
  sb3.sw.repeat = true;
  sb3.advance(1000);
  g3.eZ.fguiMgr.cTK = () => false;                   // 面板关闭
  sb3.advance(1500);
  chk(clicks3 === 0, "延时期间面板已关 -> 不点");

  // 开关关掉 -> 清 scheduled / handled 并让 token 失效
  const sb4 = makeSandbox();
  let clicks4 = 0;
  const g4 = makeGame(sb4, { panel: "AdventureFinishPanel", panelOpen: true });
  g4.setTop({ vJ: { cUV: "AdventureFinishPanel" }, efi: () => { clicks4++; } });
  sb4.sw.repeat = true;
  sb4.advance(1000);
  sb4.sw.repeat = false;
  sb4.advance(1000);
  chk(sb4.A.repeatScheduled === null && sb4.A.repeatHandled === null, "关开关 -> 清 scheduled/handled");
  sb4.advance(2000);
  chk(clicks4 === 0, "关开关后不再点击");

  // 证据日志封顶 3 条：连续 5 次"发现->点击"只打 3 条
  const sb5 = makeSandbox();
  let clicks5 = 0;
  const g5 = makeGame(sb5, { panel: "AdventureFinishPanel", panelOpen: true });
  sb5.sw.repeat = true;
  for (let i = 0; i < 5; i++) {
    g5.setTop({ vJ: { cUV: "AdventureFinishPanel" }, efi: () => { clicks5++; } });
    sb5.advance(1000);
    sb5.advance(1500);
  }
  chk(clicks5 === 5, "每次换新实例都能点(5 次)", clicks5);
  chk(sb5.count(/repeat: 点击「再次挑战」/) === 3, "点击证据日志封顶 3 条", sb5.count(/repeat: 点击「再次挑战」/));
}

/* ================================================================ T5 ===== */
console.log("\nT5 自动出售 / 自动分解 / 自动穿戴（回城一拍）");
{
  const sb = makeSandbox();
  const g = makeGame(sb, { sellQ: [2, 3], decoQ: [1], wearList: [11, 22] });
  acTick(sb, 0);
  sb.sw.sell = true; sb.sw.deco = true; sb.sw.wear = true;

  sb.advance(1000);                                  // 还没进过战斗
  chk(g.calls.sell.length === 0, "从没进过战斗 -> 不触发(必须等已回城)");

  acTick(sb, 600);                                   // tickSeen 涨 = 在战斗里
  sb.advance(1000);
  chk(sb.A.battleN === 1, "战斗被记录(battles=1)");
  chk(sb.A.inBattleLatch === true, "战斗 latch 已置位");
  chk(g.calls.sell.length === 0, "战斗中绝不发协议");

  sb.advance(1000);                                  // tickSeen 不涨 = 回到城镇
  chk(sb.A.inBattleLatch === false, "回到城镇 -> latch 落下, 触发这一拍");
  chk(sb.A.busy === true, "触发中 busy=true");

  sb.advance(900);
  chk(g.calls.sell.length === 1 && JSON.stringify(g.calls.sell[0]) === "[2,3]", "900ms: bag.bYK([2,3])", g.calls.sell);
  chk(g.calls.deco.length === 1 && JSON.stringify(g.calls.deco[0]) === "[1]", "900ms: bag.bYJ([1])", g.calls.deco);
  chk(g.calls.wear.length === 0, "穿戴还没到点(错峰)");

  sb.advance(1200);
  chk(g.calls.wear.length === 1 && JSON.stringify(g.calls.wear[0]) === "[11,22]", "2100ms: 轮到穿戴(900 -> 2100 错峰)", g.calls.wear);
  chk(sb.A.busy === false, "穿戴结束后释放 busy");
  chk(sb.has(/\[sd\] 清理 #1 开关 出售=开 分解=开/), "打了每次清理的总账日志");

  // 5 秒节流：立刻再来一次"战斗->城镇"不许重复发协议
  acTick(sb, 900);
  sb.advance(1000);
  sb.advance(1000);
  chk(g.calls.sell.length === 1, "距上次不足 5s -> 不重复清理(节流生效)", g.calls.sell.length);

  sb.advance(6000);
  acTick(sb, 3000);
  sb.advance(1000);
  sb.advance(1000);
  sb.advance(2100);
  chk(g.calls.sell.length === 2, "超过 5s 后允许第二次清理", g.calls.sell.length);
  chk(sb.has(/\[sd\] 清理 #2/), "总账日志按次编号");
}

/* ================================================================ T5b ==== */
console.log("\nT5b 品质空兜底 [1,2,3]");
{
  const sb = makeSandbox();
  const g = makeGame(sb, { sellQ: [], decoQ: [] });   // 游戏内一个都没勾
  acTick(sb, 0);
  sb.sw.sell = true; sb.sw.deco = true;
  acTick(sb, 600); sb.advance(1000);
  sb.advance(1000);
  sb.advance(900);
  chk(JSON.stringify(g.calls.sell[0]) === "[1,2,3]", "出售品质空 -> 兜底 [1,2,3]", g.calls.sell);
  chk(JSON.stringify(g.calls.deco[0]) === "[1,2,3]", "分解品质空 -> 兜底 [1,2,3]", g.calls.deco);
  chk(sb.has(/\[sd\] 出售品质为空\(游戏内未勾选\) -> 兜底 \[1,2,3\]/), "打了兜底说明日志");
  chk(sb.has(/\[sd\] 分解品质为空\(游戏内未勾选\) -> 兜底 \[1,2,3\]/), "分解兜底日志也在");
  chk(sb.A.sellDef === 1 && sb.A.decoDef === 1, "兜底只提示一次(不刷屏)");

  // 品质非空时不许走兜底
  const sb2 = makeSandbox();
  const g2 = makeGame(sb2, { sellQ: [5], decoQ: [4] });
  acTick(sb2, 0);
  sb2.sw.sell = true; sb2.sw.deco = true;
  acTick(sb2, 600); sb2.advance(1000);
  sb2.advance(1000);
  sb2.advance(900);
  chk(JSON.stringify(g2.calls.sell[0]) === "[5]", "有勾选就走勾选的品质 [5]", g2.calls.sell);
  chk(sb2.A.sellDef === 0, "有勾选时兜底标志不动");
}

/* ================================================================ T6 ===== */
console.log("\nT6 自动穿戴");
{
  const sb = makeSandbox();
  const g = makeGame(sb, { wearList: [] });
  acTick(sb, 0);
  sb.sw.wear = true;
  acTick(sb, 600); sb.advance(1000);
  sb.advance(1000);
  sb.advance(2100);
  chk(g.calls.wear.length === 0, "ftA() 返回空 -> 不提交");
  chk(sb.has(/穿戴: 当前已是最优装备（无需替换）/), "打了「已是最优」日志");
  chk(sb.A.wearEmptyN === 1, "wearEmpty 计数 +1");

  const sb2 = makeSandbox();
  const g2 = makeGame(sb2, { wearList: [11, 22, 33] });
  acTick(sb2, 0);
  sb2.sw.wear = true;
  acTick(sb2, 600); sb2.advance(1000);
  sb2.advance(1000);
  sb2.advance(2100);
  chk(g2.calls.wear.length === 1 && JSON.stringify(g2.calls.wear[0]) === "[11,22,33]",
      "有候选 -> equip.fvH([11,22,33])", g2.calls.wear);
  chk(sb2.has(/穿戴: 待替换 3 件 -> equip\.fvH/), "打了穿戴提交日志");
  await flush();
  chk(sb2.A.wearOkN === 1, "resolved code=0 -> wearOk +1");

  // resolved code 15(网络未就绪) -> 延时重试（这条照抄原版 L4580-4608）
  const sb3 = makeSandbox();
  const g3 = makeGame(sb3, { wearList: [7], wearCode: 15 });
  acTick(sb3, 0);
  sb3.sw.wear = true;
  acTick(sb3, 600); sb3.advance(1000);
  sb3.advance(1000);
  sb3.advance(2100);                                  // 第一次提交
  await flush();                                      // resolved 在微任务里，必须冲一下
  chk(g3.calls.wear.length === 1, "第一次提交发生");
  chk(sb3.has(/穿戴: code=15\(网络未就绪\) -> 1500ms 后重试\(1\/3\)/), "resolved code=15 -> 排了 1500ms 重试");
  sb3.advance(1500);
  await flush();
  chk(g3.calls.wear.length === 2, "重试后第二次提交发生", g3.calls.wear.length);

  // rejected -> 原版【不】重试，只记一行（我们照抄，不自创行为）
  const sb4 = makeSandbox();
  const g4 = makeGame(sb4, { wearList: [9], wearRejectWith: 15 });
  acTick(sb4, 0);
  sb4.sw.wear = true;
  acTick(sb4, 600); sb4.advance(1000);
  sb4.advance(1000);
  sb4.advance(2100);
  await flush();
  chk(g4.calls.wear.length === 1, "rejected: 只提交一次");
  sb4.advance(10000);
  await flush();
  chk(g4.calls.wear.length === 1, "rejected 不重试(与原版一致: 原版只 log「拒绝」)", g4.calls.wear.length);
  chk(sb4.A.errN >= 1 && /wear\/reject/.test(sb4.A.lastErr), "rejected 记进 lastErr(可诊断)", sb4.A.lastErr);
}

/* ============================================================ T8 ===== */
console.log("\nT8 面板辅助(按卡密包的名字走)");
{
  const sbx = makeSandbox();
  let clicks = 0;
  const gx = makeGame(sbx, { panel: "AdventureFinishPanel", panelOpen: true });
  chk(sbx.win.__require("$JX").$fb.cvConfig.panels.AdventureFinishPanel !== undefined, "$JX 面板配置桩可用");
  const topX = { vJ: { cUV: "AdventureFinishPanel" }, efi: () => { clicks++; } };
  gx.setTop(topX);
  sbx.sw.repeat = true;
  sbx.advance(1000);
  chk(sbx.A.repeatScheduled === topX, "panelCandidates 通过 cTU().vJ.cUV 找到了候选控件");
  sbx.advance(1500);
  chk(clicks === 1, "面板路径完整走通(发现->1500ms->点击)");

  // cTK(cfg) 说没开就整条不触发
  const sby = makeSandbox();
  let clicksY = 0;
  const gy = makeGame(sby, { panel: "AdventureFinishPanel", panelOpen: false });
  gy.setTop({ vJ: { cUV: "AdventureFinishPanel" }, efi: () => { clicksY++; } });
  sby.sw.repeat = true;
  sby.advance(1000);
  chk(sby.A.repeatScheduled === null, "cTK(cfg)=false -> 面板没开, 不排点击");
  sby.advance(2000);
  chk(clicksY === 0, "面板没开 -> 一次都不点");
}

/* ============================================================ T7 ===== */
console.log("\nT7 时序铁律");
{
  const sb = makeSandbox();
  const g = makeGame(sb, { sellQ: [1], wearList: [5] });
  acTick(sb, 0);
  sb.sw.sell = true; sb.sw.wear = true;
  acTick(sb, 600);
  sb.advance(1000);                                // 战斗中
  sb.advance(1000);                                // 回城 -> 触发
  sb.advance(900);
  const atSell = sb.getNow();
  chk(g.calls.sell.length === 1, "900ms 出售/分解已发");
  chk(g.calls.wear.length === 0, "此时穿戴还没发");
  sb.advance(1200);
  chk(g.calls.wear.length === 1, "2100ms 穿戴才发", g.calls.wear);
  chk(sb.getNow() - atSell === 1200, "两次协议错开 1200ms(900 -> 2100)");

  // 三个开关全关 -> 连 latch 都不留
  const sb2 = makeSandbox();
  const g2 = makeGame(sb2, {});
  acTick(sb2, 0);
  acTick(sb2, 600); sb2.advance(1000);
  sb2.advance(1000);
  chk(sb2.A.inBattleLatch === false && sb2.A.lastCleanup === 0, "开关全关 -> 一次都不触发");
}

/* ============================================================ T9 ===== */
console.log("\nT9 __AUTO_STATE__ 字段齐全");
{
  const sb = makeSandbox();
  makeGame(sb, {});
  sb.advance(1000);
  const st = sb.state();
  const want = ["v", "gold", "goldKey", "goldHow", "goldN", "repeatFlag", "repeatClicks", "sell", "deco",
                "sellQ", "decoQ", "sdDump", "sdTotal", "sdLocked", "sdDecoable",
                "protoSent", "protoCode", "wear", "wearOk", "wearEmpty", "ticks", "battles",
                "inLatch", "busy", "errs", "lastErr", "warns", "note"];
  const miss = want.filter((k) => !(k in st));
  chk(miss.length === 0, "28 个诊断字段都在", miss);
  chk(Object.keys(st).length === want.length, "没有多余字段(口径稳定)", Object.keys(st).length);
  chk(st.ticks === 1, "ticks 随 tick 增长", st.ticks);
  sb.advance(1000);
  chk(sb.state().ticks === 2, "第二拍 ticks=2");

  // 第 15 / 60 拍各打一张一次性快照
  sb.advance(13000);
  chk(sb.has(/\[AUTO\] state@15s /), "第 15 拍打了状态快照");
  chk(sb.has(/\[AUTO\] state@60s /) === false, "第 60 拍还没到, 没提前打");
}

/* ============================================================ T10 ==== */
console.log("\nT10 错误面(缺 API 不许抛)");
{
  const sb = makeSandbox();
  // 完全没有 $eZ：tick 必须照跑不炸
  let threw = null;
  try { sb.advance(3000); } catch (e) { threw = e; }
  chk(threw === null, "无 $eZ 时 tick 不抛异常", threw && threw.message);
  chk(sb.state().ticks === 3, "无 $eZ 时照常 tick", sb.state().ticks);
  chk(sb.state().warns === 0, "还没到 $eZ 就是 0 告警(不能把「还没加载」误报成「改名了」)", sb.state().warns);

  // $eZ 在, 但 bag 缺 API -> 只记 lastErr
  const sb2 = makeSandbox();
  const g2 = makeGame(sb2, {});
  delete g2.eZ.bag.bYK;
  acTick(sb2, 0);
  sb2.sw.sell = true;
  acTick(sb2, 600); sb2.advance(1000); sb2.advance(1000); sb2.advance(900);
  chk(sb2.state().errs >= 1 && /bYK/.test(sb2.state().lastErr), "缺 bag.bYK -> 记 lastErr 而不是抛", sb2.state().lastErr);

  // 有 $eZ 但 battleMgr 不可用 -> 靠通道②仍能判定战斗（这正是双通道的意义）
  const sb3 = makeSandbox();
  const g3 = makeGame(sb3, {});
  g3.eZ.battleMgr = null;
  acTick(sb3, 0);
  sb3.sw.sell = true;
  acTick(sb3, 600); sb3.advance(1000);
  chk(sb3.A.battleN === 1,
      "battleMgr 没了但 __starry_ac.tickSeen 在涨 -> 通道②照样判出战斗(双通道兜底)", sb3.A.battleN);
}

/* ============================================================ T11 ==== */
console.log("\nT11 改名诊断(把静默失效变成一条日志)");
{
  const sb = makeSandbox();
  const g = makeGame(sb, { sellQ: [1], wearList: [1] });
  // 模拟官方热更把这几个【逐版本重混淆的短名】全改名了
  Object.setPrototypeOf(g.cardData, {});   // monCard.fDn.prototype.fDc 没了
  delete g.eZ.bag;                         // bag.bYK/bYJ 没了
  delete g.eZ.equip;                       // equip.ftA/fvH 没了
  delete g.eZ.fguiMgr;                     // 面板管理器没了
  acTick(sb, 0);
  sb.sw.gold = true; sb.sw.repeat = true; sb.sw.sell = true; sb.sw.wear = true;
  sb.advance(1000);
  chk(sb.has(/warn: 解锁金卡: monCard 下找不到「持有判定」方法/), "金卡短名改名 -> 告警");
  chk(sb.has(/warn: 自动重复挑战: \$eZ\.fguiMgr 不存在/), "面板管理器改名 -> 告警");
  chk(sb.A.goldHooked === false, "改名后绝不假装挂上了(goldHooked 保持 false)");
  chk(g.cardData.fDc === undefined, "改名后没有往原型上硬塞 fDc(不污染游戏对象)");

  acTick(sb, 600); sb.advance(1000);       // 进战斗
  sb.advance(1000);                        // 回城 -> 触发清理
  sb.advance(900);
  chk(sb.has(/warn: 出售\/分解: \$eZ\.bag 不存在/), "bag 改名 -> 告警");
  sb.advance(1200);
  chk(sb.has(/warn: 自动穿戴: \$eZ\.equip 不存在/), "equip 改名 -> 告警");
  chk(sb.A.warnN === 4, "四个改名位各一条告警", sb.state().warns);
  sb.advance(8000);
  chk(sb.A.warnN === 4, "每个改名位只告警一次(不刷屏)", sb.state().warns);
}

console.log(`\n结果: ${pass} pass / ${fail} fail`);
process.exit(fail === 0 ? 0 : 1);

})().catch((e) => { console.error("harness crashed:", e); process.exit(2); });
