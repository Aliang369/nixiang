#!/usr/bin/env node
/* ============================================================================
 * 星空过检测层 离线验收台 (不需要真机, 不依赖 Cocos)
 *
 * 关键点：命中的 hY 用的是【游戏真实代码】——
 *   从 06_dumped/勇士之心/hotupdate_20261005/assets/battleLogic/index.js
 *   里按大括号配对抠出 prototype.hY 的原文，注入 M/N 桩后当作原型方法装载。
 * 这样验的不是我写的 mock，而是"我的指纹在真实方法上到底挂不挂得上"。
 *
 * 覆盖 4 层 + 配对纪律 + 可观测口径：
 *   T1 指纹在真实 hY 上命中(T1 若失败, 整个伤害还原层就是静默失效)
 *   T2 放大伤害后累加器被还原成 atkBefore+原值 / vicBefore+原值
 *   T3 没放大(未开功能)时绝不改动(hY 原样行为)
 *   T4 GD 调用了但 hY 没走到 → pending 不外溢到下一笔(clearPending 纪律)
 *   T5 hY 因 e<=0 早退时不误补偿(靠 finalDamage!==atkB 自校验)
 *   T6 闸门: 模块导出上的 ANTI_CHEAT_ENABLED 被焊成 getter false
 *   T7 出口: $eZ.battleServer.sendMsg / $eZ.gsMgr.cQA 都挂上
 *   T8 结算: useTime 只观测不修改(observe-only), 墙钟/逻辑钟都记下来
 *   T9 __AC_STATE__ 字段齐全
 * 用法: node test_ac_layer.js [ac_layer.js] [battleLogic/index.js]
 * ========================================================================== */
"use strict";
const fs = require("fs");
const path = require("path");

const HERE = __dirname;
const ROOT = path.resolve(HERE, "..", "..", "..");
const ACL = process.argv[2] || path.join(HERE, "ac_layer.js");
const BL = process.argv[3] ||
  path.join(ROOT, "06_dumped/勇士之心/hotupdate_20261005/assets/battleLogic/index.js");

let pass = 0, fail = 0;
function chk(good, msg, extra) {
  if (good) { pass++; console.log("  ok   " + msg); }
  else { fail++; console.log("  FAIL " + msg + (extra === undefined ? "" : "   -> " + JSON.stringify(extra))); }
}

/* ---------------------------------------------- 1. 抠出游戏真实的 hY 原文 -- */
function extractRealHY(file) {
  const s = fs.readFileSync(file, "utf8");
  const m = /prototype\.hY\s*=\s*function/.exec(s);
  if (!m) throw new Error("dump 里找不到 prototype.hY（dump 换版本了？）");
  const braceAt = s.indexOf("{", m.index + m[0].length);
  let d = 0, k = braceAt;
  for (; k < s.length; k++) {
    const c = s[k];
    if (c === "{") d++;
    else if (c === "}") { d--; if (d === 0) break; }
  }
  const body = s.slice(braceAt, k + 1);              // { ... }
  const head = s.slice(m.index + "prototype.hY = ".length, braceAt); // function(t, e, n)
  return { head: head.trim(), body, full: s.slice(m.index, k + 1) };
}
const real = extractRealHY(BL);
console.log(`[hY] 真实源码 ${real.full.length} B  ${real.head}`);

/* ------------------------------------------------------------- 2. 装载环境 -- */
const win = {};
win.window = win;
win.console = console;
win.MOD_LOG = [];
win.setTimeout = setTimeout;
win.__starry_nlog = function (s) { win.MOD_LOG.push(s); };

// 载入被测的 AC 层（IIFE，靠 window 参数注入）
new Function("window", fs.readFileSync(ACL, "utf8"))(win);
const AC = win.__starry_ac;
chk(!!AC && typeof win.__AC_STATE__ === "function", "AC 层装载成功, window.__starry_ac / __AC_STATE__ 都在");

// $eZ 全局（game/index.js @138686 才有，所以这里模拟"稍后才出现"）
const sent = [];
const cqa = [];
const eZ = {
  battleServer: { sendMsg: function (proto, payload) { sent.push([proto, payload]); return proto; } },
  gsMgr: { cQA: function (proto, payload, opts) { cqa.push([proto, payload, opts]); return proto; } }
};

// 游戏侧桩：M / N 是 hY 依赖的外部引用
const M = { $uG: { $vv: { b: "vvB" }, $vD: { v: "v", y: "y" }, $vA: { f: "f" } } };
const N = { default: { Gv() {}, Gt() {}, Hu() {}, He() {} } };
const makeUnit = (st) => {
  const u = Object.create(Body.prototype);   // 真机里 hY 在战斗单位原型上, this = 受击者
  u.hw = st; u.hF = null; u.hT = false; u.hh = 1;
  u.hj = { bnR: { gv() {}, fI: null } };
  return u;
};

/* --------------------------------------- 3. 把真实 hY 装到原型上并跑扫描 -- */
function makeBodyClass() {
  function Body() {}
  Body.prototype.hY = new Function("M", "N", "return (" + real.head + real.body + ");")(M, N);
  return Body;
}
const Body = makeBodyClass();
chk(real.full.indexOf("finalHurtValue") >= 0 && real.full.indexOf("lockMinHP") >= 0 &&
    real.full.indexOf("finalDamage") >= 0, "真实 hY 含三个指纹明文(finalHurtValue/lockMinHP/finalDamage)");

// 模块 exports 形态：bundle 里类通常挂在 exports 成员(常见是 default)上
const exportsObj = { default: Body };
AC.scan(exportsObj, "$KK");

console.log("--- T1 命中 ---");
chk(AC.hitHooked === true, "T1 在真实 hY 上挂上了(T1 挂了=伤害还原层静默失效)");
chk(AC.hitKey === "hY", "T1 命中的键就是 hY", AC.hitKey);
chk(/hit-dispatch hooked/.test(win.MOD_LOG.join("\n")), "T1 命中日志已写(window.__starry_nlog)");

console.log("--- T1b 诱饵歧义: bgN 与真 hY 并存时必须选 hY ---");
/* 真机实测回归: 老版「首个三串命中即 break」在 battleLogic 上挂到了诱饵 prototype.bgN
 * (真机日志 `hit-dispatch hooked: prototype.bgN`), 而 bgN 只是字段重置方法 —— 于是
 * dmgNoted 涨、dmgFixed 恒为 0 = 伤害还原层【静默失效】。这里用真 hY + 一个逐字复刻的
 * bgN 诱饵(三串明文齐备但无 Math.max(...lockMinHP))复现该场景, 且故意把诱饵放在前面。 */
const win2 = {};
win2.window = win2;
win2.console = console;
win2.MOD_LOG = [];
win2.setTimeout = setTimeout;
win2.__starry_nlog = function (s) { win2.MOD_LOG.push(s); };
new Function("window", fs.readFileSync(ACL, "utf8"))(win2);
const AC2 = win2.__starry_ac;
function Body2() {}
Body2.prototype.bgN = function () {          // 诱饵: 含 finalHurtValue/finalDamage/lockMinHP 三串, 无 Math.max
  this.hw.finalDamage = 0;
  this.hw.finalHurtValue = 0;
  this.hw.lockMinHP = 0;
};
Body2.prototype.hY = new Function("M", "N", "return (" + real.head + real.body + ");")(M, N);
AC2.scan({ default: Body2 }, "$KK2");
chk(AC2.hitHooked === true, "T1b 挂上了");
chk(AC2.hitKey === "hY", "T1b 命中真 hY 而非诱饵 bgN(挂了 bgN=伤害还原静默失效)", AC2.hitKey);
chk(AC2.hitScore >= 13, "T1b 分数达硬门槛(Math.max(...lockMinHP) 至少 +10)", AC2.hitScore);

console.log("--- T2 放大后还原成 原值 ---");
function fresh(ad, vh) {
  return { atk: makeUnit({ finalDamage: ad, finalHurtValue: 0, bhd: 9999, bhe: 9999, lockMinHP: 0, bhi: 1 }),
           vic: makeUnit({ finalDamage: 0, finalHurtValue: vh, bhd: 9999, bhe: 9999, lockMinHP: 0, bhi: 1 }) };
}
let t = fresh(100, 200);
AC.clearPending(); AC.noteDamage(50, 5000);           // 主补丁 E1/E6 的两步
t.vic.hY(t.atk, 5000, null);
chk(t.atk.hw.finalDamage === 150, "T2 攻击者 finalDamage = 100(旧)+50(原) = 150", t.atk.hw.finalDamage);
chk(t.vic.hw.finalHurtValue === 250, "T2 受击者 finalHurtValue = 200(旧)+50(原) = 250", t.vic.hw.finalHurtValue);
chk(AC.fixedN === 1, "T2 dmgFixed 计数 = 1", AC.fixedN);
chk(t.vic.hw.bhd === 9999 - 5000, "T2 血量仍按放大后的伤害扣(我们没有改伤害本身)", t.vic.hw.bhd);

console.log("--- T3 未放大时不改动 ---");
t = fresh(100, 200);
AC.clearPending(); AC.noteDamage(50, 50);
t.vic.hY(t.atk, 50, null);
chk(t.atk.hw.finalDamage === 150, "T3 攻击者 = 100+50(游戏原样)", t.atk.hw.finalDamage);
chk(t.vic.hw.finalHurtValue === 250, "T3 受击者 = 200+50(游戏原样)", t.vic.hw.finalHurtValue);
chk(AC.fixedN === 1, "T3 dmgFixed 没有增加", AC.fixedN);

console.log("--- T4 pending 不外溢 ---");
t = fresh(100, 200);
AC.clearPending(); AC.noteDamage(50, 5000);            // 第 1 笔被放大
AC.clearPending(); AC.noteDamage(7, 7);                // 第 2 笔(主补丁每笔 GD 都会先 clearPending)
t.vic.hY(t.atk, 5000, null);                           // 却来了个不匹配的伤害
chk(t.atk.hw.finalDamage === 5100 && AC.fixedN === 1, "T4 自校验拦住误配(没补偿)", [t.atk.hw.finalDamage, AC.fixedN]);
t = fresh(100, 200);
AC.clearPending(); AC.noteDamage(50, 5000);            // GD 放了话, 但这一笔没走到 hY
AC.clearPending();                                     // 下一笔 GD 立刻清空 -> 不会误配上一笔
t.vic.hY(t.atk, 5000, null);
chk(t.atk.hw.finalDamage === 5100 && AC.fixedN === 1, "T4 上一笔残留不会误配到这一笔", [t.atk.hw.finalDamage, AC.fixedN]);

console.log("--- T5 hY 早退(e<=0)时不误补偿 ---");
t = fresh(100, 200);
AC.clearPending(); AC.noteDamage(0, 5000);             // orig=0 不会被 noteDamage 记(boosted>orig 且 orig 是数)
AC.pending = { orig: 50, boosted: 5000 };              // 人为塞一个 pending 制造最坏情况
t.vic.hY(t.atk, 0, null);                              // e<=0 -> hY 直接 return, 两个累加器都没动
chk(t.atk.hw.finalDamage === 100 && t.vic.hw.finalHurtValue === 200 && AC.fixedN === 1,
    "T5 早退时靠 finalDamage!==atkB 自校验跳过", [t.atk.hw.finalDamage, t.vic.hw.finalHurtValue]);

console.log("--- T6 闸门 ---");
const gateMod = { ANTI_CHEAT_ENABLED: true, other: 1 };
AC.scan({ gateMod: gateMod }, "$TEST");
chk(gateMod.ANTI_CHEAT_ENABLED === false, "T6 ANTI_CHEAT_ENABLED 读出来是 false");
gateMod.ANTI_CHEAT_ENABLED = true;
chk(gateMod.ANTI_CHEAT_ENABLED === false, "T6 再赋值也被 set 吞掉");
chk(AC.gateArmed === true && AC.gateCount >= 1, "T6 gateArmed 置位(说明原本是 true, 属真保险)",
    [AC.gateArmed, AC.gateCount]);

console.log("--- T7 出口挂钩 ---");
win.$eZ = eZ;
AC.tryInstall();
chk(AC.sinkSend === true && AC.sinkCqa === true, "T7 两个出口都挂上", [AC.sinkSend, AC.sinkCqa]);
/* 补一帧墙钟: 主补丁在移动系统 update 里调 AC.tick(this.dt) */
for (let i = 0; i < 600; i++) AC.tick(1 / 60);

console.log("--- T8 结算观测(只观测不修改) ---");
const payload = { completeResult: { id: 1, useTime: 123456, result: 1, signData: "", signCode: "s" } };
eZ.battleServer.sendMsg(1001, payload);          // battleMgr.bER 的第 1 条出口
chk(payload.completeResult.useTime === 123456, "T8 useTime 未被改写(observe-only)", payload.completeResult.useTime);
chk(AC.reportN === 1, "T8 reportN = 1", AC.reportN);
chk(AC.logicalMs === 123456, "T8 逻辑钟(useTime)被记下", AC.logicalMs);
chk(/wall=10000/.test(AC.note), "T8 结算时看到的墙钟 = 10000ms(600 帧 x 1/60)", AC.note);
chk(AC.wallMs === 0, "T8 墙钟按局归零(下一局的区间重新累计)", AC.wallMs);
for (let i = 0; i < 300; i++) AC.tick(1 / 60);
chk(Math.round(AC.wallMs) === 5000, "T8 归零后继续累计 = 5000ms", Math.round(AC.wallMs));
const payload2 = { completeResult: { id: 2, useTime: 90000 } };
eZ.gsMgr.cQA(1001, payload2, { isBlockEvent: false, merge: false });   // 第 2 条出口
chk(AC.reportN === 2, "T8 cQA 出口也被计数(两条出口都覆盖)", AC.reportN);
chk(payload2.completeResult.useTime === 90000, "T8 第二条 useTime 也未被改写", payload2.completeResult.useTime);
eZ.battleServer.sendMsg(1002, { damage: 1 });     // 非结算报文
chk(AC.reportN === 2, "T8 非 completeResult 报文不计数(热路径早退)", AC.reportN);

console.log("--- T9 可观测口径 ---");
const st = win.__AC_STATE__();
const keys = ["gateArmed", "gateMod", "gateCount", "sinkSendMsg", "sinkGsMgrCQA", "hitHooked", "hitKey",
              "dmgNoted", "dmgFixed", "dmgHit", "dmgEvNote", "dmgEvFix",
              "reportN", "logicalMs", "wallMs", "tickSeen", "tickN", "tickBad",
              "clampArmed", "clamped", "installN", "note"];
chk(keys.every((k) => k in st), "T9 诊断字段齐全", keys.filter((k) => !(k in st)));
chk(st.tickN === 900 && st.tickBad === 0, "T9 墙钟计数 = 900 帧(清零后不再重置计数器)", [st.tickN, st.tickBad]);
chk(st.logicalMs === 90000, "T9 logicalMs = 最后一笔 useTime = 90000", st.logicalMs);
chk(st.tickSeen === 900, "T9 tickSeen = 900(与 tickN 相等说明 update 上确实有 this.dt)",
    [st.tickSeen, st.tickN]);
chk(st.clampArmed === false && st.clamped === 0, "T9 钳制默认关闭(default off)");
console.log("     __AC_STATE__ = " + JSON.stringify(st));

console.log("--- T10 baseline 仲裁接入(adoptHit) + 包装次序 ---");
const win3 = {};
win3.window = win3;
win3.console = { log() {}, error() {} };
win3.MOD_LOG = [];
win3.setTimeout = setTimeout;
win3.__starry_nlog = function (s) { win3.MOD_LOG.push(s); };
new Function("window", fs.readFileSync(ACL, "utf8"))(win3);
const AC3 = win3.__starry_ac;
chk(!!AC3 && typeof win3.__AC_STATE__ === "function", "T10 新沙箱 AC3 装载");

function Body3() {}
Body3.prototype.hY = new Function("M", "N", "return (" + real.head + real.body + ");")(M, N);

AC3.adoptHit(Body3, "hY");
chk(AC3.hitHooked === true, "T10 adoptHit 后 hitHooked = true");
chk(AC3.hitKey === "hY", "T10 hitKey = hY", AC3.hitKey);
chk(AC3.hitScore === -1, "T10 hitScore = -1(标记由 baseline 仲裁, 非本地打分)", AC3.hitScore);
chk(/hit-dispatch adopted from baseline: prototype\.hY/.test(win3.MOD_LOG.join("\n")),
    "T10 打出 adopted from baseline 日志");
chk(Body3.prototype.hY.__acHit === true, "T10 hY 被打上 __acHit 戳");

/* 模拟 baseline 在外层再包一层(feature2 秒杀/无敌用的包裹) */
const acWrapped3 = Body3.prototype.hY;
function baselineWrap3(feature2, isMine) {
  return function (atk, dmg, hit) {
    if (feature2 && isMine) { if (hit) hit.finalDamage = 0; return; }
    return acWrapped3.apply(this, arguments);
  };
}
function unit3(st) {
  const u = Object.create(Body3.prototype);
  u.hw = st; u.hF = null; u.hT = false; u.hh = 1; u.hj = { bnR: { gv() {}, fI: null } };
  return u;
}
function fresh3(ad, vh) {
  return {
    atk: unit3({ finalDamage: ad, finalHurtValue: 0, bhd: 9999, bhe: 9999, lockMinHP: 0, bhi: 1 }),
    vic: unit3({ finalDamage: 0, finalHurtValue: vh, bhd: 9999, bhe: 9999, lockMinHP: 0, bhi: 1 })
  };
}

/* feature2 关: baseline 外层 -> AC 内层 -> 原 hY ⇒ AC 照常记账/还原 */
Body3.prototype.hY = baselineWrap3(false, false);
let t3 = fresh3(100, 200);
AC3.clearPending();
AC3.noteDamage(50, 5000);
t3.vic.hY(t3.atk, 5000, null);
chk(t3.atk.hw.finalDamage === 150 && t3.vic.hw.finalHurtValue === 250,
    "T10 feature2 关: AC 在最内层, 累加器照常还原", [t3.atk.hw.finalDamage, t3.vic.hw.finalHurtValue]);
chk(AC3.fixedN === 1, "T10 feature2 关: fixedN 增长", AC3.fixedN);

/* feature2 开: baseline 提前 return ⇒ AC 不执行 ⇒ 累加器不被污染 */
Body3.prototype.hY = baselineWrap3(true, true);
t3 = fresh3(100, 200);
AC3.clearPending();
AC3.noteDamage(50, 5000);
t3.vic.hY(t3.atk, 5000, null);
chk(t3.atk.hw.finalDamage === 100 && t3.vic.hw.finalHurtValue === 200 && AC3.fixedN === 1,
    "T10 feature2 开: baseline 早退, AC 不执行, 累加器不被污染",
    [t3.atk.hw.finalDamage, t3.vic.hw.finalHurtValue, AC3.fixedN]);

/* 重复 adopt: hitHooked 已置位 ⇒ no-op */
const key3 = AC3.hitKey;
AC3.adoptHit(Body3, "hY");
chk(AC3.hitKey === key3 && AC3.hitHooked === true, "T10 二次 adoptHit 是 no-op(hitHooked 守卫)");

/* __acHit 守卫: 已包过的方法不再二次包装 */
const win4 = {};
win4.window = win4;
win4.console = { log() {}, error() {} };
win4.MOD_LOG = [];
win4.setTimeout = setTimeout;
win4.__starry_nlog = function (s) { win4.MOD_LOG.push(s); };
new Function("window", fs.readFileSync(ACL, "utf8"))(win4);
const AC4 = win4.__starry_ac;
function Body4() {}
Body4.prototype.hY = acWrapped3;      /* 已是 AC 包装器(带 __acHit) */
AC4.adoptHit(Body4, "hY");
chk(AC4.hitHooked === false, "T10 __acHit 守卫: 已包过的方法不再二次包装", AC4.hitHooked);

console.log("--- T11 证据链一次性日志(真机可证伪性) ---");
/* 起因: dmgNoted/dmgFixed 只在装载期那张一次性快照里出现, 而快照必定早于进战斗 =>
 * 真机上永远看不到战斗中的记账。没有 T11 这三条日志, "补偿链路到底跑没跑"
 * 就是一个无法证伪的说法。此处验证日志内容正确 + 条数有上限。 */
const win5 = {};
win5.window = win5;
win5.console = { log() {}, error() {} };
win5.NLOG = [];
win5.NLOG = [];                       /* 单独通道: log() 会同时写 __starry_nlog 与 MOD_LOG, 计数只认其一 */
win5.setTimeout = setTimeout;
win5.__starry_nlog = function (s) { win5.NLOG.push(s); };
new Function("window", fs.readFileSync(ACL, "utf8"))(win5);
const AC5 = win5.__starry_ac;
function Body5() {}
Body5.prototype.hY = new Function("M", "N", "return (" + real.head + real.body + ");")(M, N);
AC5.adoptHit(Body5, "hY");
function unit5(st) {
  const u = Object.create(Body5.prototype);
  u.hw = st; u.hF = null; u.hT = false; u.hh = 1; u.hj = { bnR: { gv() {}, fI: null } };
  return u;
}
function fresh5(ad, vh) {
  return {
    atk: unit5({ finalDamage: ad, finalHurtValue: 0, bhd: 9999, bhe: 9999, lockMinHP: 0, bhi: 1 }),
    vic: unit5({ finalDamage: 0, finalHurtValue: vh, bhd: 9999, bhe: 9999, lockMinHP: 0, bhi: 1 })
  };
}

/* ①"未放大"不该打日志(噪声控制) */
AC5.clearPending();
AC5.noteDamage(50, 50);
chk(AC5.evNote === 0, "T11 未放大时 noteDamage 不打证据日志", AC5.evNote);

/* ②放大 → 打 dmg boosted, 内容含 orig/boosted */
AC5.clearPending();
AC5.noteDamage(50, 5000);
const logTxt1 = win5.NLOG.join("\n");
chk(AC5.evNote === 1, "T11 放大时 evNote 计数 = 1", AC5.evNote);
chk(/dmg boosted #1: orig=50 -> boosted=5000/.test(logTxt1), "T11 证据链⓪ 内容正确", logTxt1.slice(-200));

/* ③命中 → dmg hit 含 pending 配对 */
let t5 = fresh5(100, 200);
t5.vic.hY(t5.atk, 5000, null);
const logTxt2 = win5.NLOG.join("\n");
chk(AC5.evHit === 1, "T11 hY 命中时 evHit 计数 = 1", AC5.evHit);
chk(/dmg hit #1: dmg=5000 key=hY pending=50->5000 atkBefore=100 vicBefore=200/.test(logTxt2),
    "T11 证据链① 内容正确(含 pending 配对与命中前值)", logTxt2.slice(-300));

/* ④还原 → dmg fix 记录 A->B->C 三段(还原前 / 放大后 / 还原后) */
chk(AC5.evFix === 1, "T11 还原时 evFix 计数 = 1", AC5.evFix);
chk(/dmg fix #1: orig=50 boosted=5000 finalDamage 100->5100->150 {2}finalHurtValue 200->5200->250 {2}fixedN=1/.test(logTxt2),
    "T11 证据链② 内容正确(finalDamage 100->5100->150, finalHurtValue 200->5200->250)",
    (logTxt2.match(/dmg fix[^\n]*/) || [""])[0]);
chk(t5.atk.hw.finalDamage === 150 && t5.vic.hw.finalHurtValue === 250,
    "T11 证据链② 还原后累加器 = 原始伤害", [t5.atk.hw.finalDamage, t5.vic.hw.finalHurtValue]);

/* ⑤ 新字段进 __AC_STATE__ */
const st5 = win5.__AC_STATE__();
chk("dmgHit" in st5 && "dmgEvNote" in st5 && "dmgEvFix" in st5,
    "T11 证据计数已暴露给 __AC_STATE__", Object.keys(st5).join(","));

/* ⑥ 上限 3: 连打 5 笔, 日志条数封顶 */
for (let i = 0; i < 5; i++) {
  t5 = fresh5(100, 200);
  AC5.clearPending();
  AC5.noteDamage(50, 5000);
  t5.vic.hY(t5.atk, 5000, null);
}
const nHit = (win5.NLOG.join("\n").match(/dmg hit #/g) || []).length;
const nFix = (win5.NLOG.join("\n").match(/dmg fix #/g) || []).length;
const nNote = (win5.NLOG.join("\n").match(/dmg boosted #/g) || []).length;
chk(nHit === 3 && nFix === 3 && nNote === 3,
    "T11 三条证据日志各自封顶 3 条(不刷屏)", [nHit, nFix, nNote]);
chk(AC5.fixedN === 6, "T11 封顶只限日志, 计数照常累加(6 笔全补偿)", AC5.fixedN);

console.log("--- T12 证据链③ 结算出口一次性日志 ---");
/* 与 T11 同因: reportN/logicalMs/wallMs 只活在"装载期那张一次性快照"里,
 * 而快照打于进战斗之前 => 真机上永远看不到结算。这条日志是唯一的真机证据。 */
const win6 = {};
win6.window = win6;
win6.console = { log() {}, error() {} };
win6.MOD_LOG = [];
win6.NLOG = [];
win6.setTimeout = setTimeout;
win6.__starry_nlog = function (s) { win6.NLOG.push(s); };
new Function("window", fs.readFileSync(ACL, "utf8"))(win6);
const AC6 = win6.__starry_ac;
const eZ6 = {
  battleServer: {
    sendMsg(proto, payload) { return proto + ":" + (payload ? "ok" : "nil"); },
    __acSweepDone: false
  },
  gsMgr: { cQA(proto, payload) { return "cqa:" + proto; } }
};
win6.$eZ = eZ6;
AC6.tryInstall();
chk(AC6.sinkSend === true && AC6.sinkCqa === true, "T12 两出口挂上", [AC6.sinkSend, AC6.sinkCqa]);

for (let i = 0; i < 600; i++) AC6.tick(1 / 60);        // 10.000s 墙钟
const pl6 = { completeResult: { id: 7, useTime: 22500 } };
eZ6.battleServer.sendMsg(7777, pl6);
chk(pl6.completeResult.useTime === 22500, "T12 useTime 未被改写(observe-only)", pl6.completeResult.useTime);
chk(AC6.evReport === 1, "T12 repEv 计数 = 1", AC6.evReport);
const t12 = win6.NLOG.join("\n");
chk(/report #1: useTime=22500 wallMs=10000 delta=12500ms tag=sendMsg clamped=no clampArmed=false reportN=1/.test(t12),
    "T12 证据链③ 内容正确(逻辑钟 vs 墙钟并排 + 未钳制)", (t12.match(/report #[^\n]*/) || [""])[0]);
chk("repEv" in win6.__AC_STATE__(), "T12 repEv 已暴露给 __AC_STATE__");

/* 非结算报文不该打日志 */
eZ6.battleServer.sendMsg(7778, { damage: 5 });
chk(AC6.evReport === 1, "T12 非 completeResult 不打证据日志", AC6.evReport);

/* 封顶 3 */
for (let i = 0; i < 5; i++) {
  for (let k = 0; k < 300; k++) AC6.tick(1 / 60);
  eZ6.gsMgr.cQA(7779, { completeResult: { id: 8 + i, useTime: 30000 } }, { merge: false });
}
chk((win6.NLOG.join("\n").match(/report #/g) || []).length === 3,
    "T12 结算证据日志封顶 3 条", (win6.NLOG.join("\n").match(/report #/g) || []).length);
chk(AC6.reportN === 6, "T12 封顶只限日志, reportN 照常累加(共 6 局)", AC6.reportN);

console.log(`\n结果: ${pass} pass / ${fail} fail`);
process.exit(fail === 0 ? 0 : 1);
