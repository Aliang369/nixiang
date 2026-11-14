#!/usr/bin/env node
/* test_js_only_wiring.js — 用**真实产物**验证原签热更两开关的接线语义（纯离线）
 * 输入：patch_1242_jsonly.js 产出的 <outdir>/main.jsonly.patched.js 与 Script.jsonly.patched.js
 * 做法：从产物里 regex 抽出实际插入语句，放进最小 harness 执行，断言
 *   ON  ⇒ 走拦截分支（不落到游戏原流程）
 *   OFF ⇒ 落到原流程（插入语句是 no-op）
 * 同时统计每个锚点只插一次（9 iap 渠道 + 1 验单 + 3 广告），并跑 vm 语法门。
 * 退出码 0=全过；1=有失败
 */
"use strict";
const fs = require("fs");
const path = require("path");
const vm = require("vm");

const OUT = process.argv[2] || path.join(__dirname, "..", "..", "07_patched", "动物餐厅", "dwct1242", "jsonly_devicehot");
const mainJs = path.join(OUT, "main.jsonly.patched.js");
const scriptJs = path.join(OUT, "Script.jsonly.patched.js");

let pass = 0, fail = 0;
function check(name, fn) {
  try { fn(); console.log("PASS  " + name); pass++; }
  catch (e) { console.log("FAIL  " + name + " :: " + e.message); fail++; }
}
const main = fs.readFileSync(mainJs, "utf8");
const script = fs.readFileSync(scriptJs, "utf8");
const count = (s, sub) => s.split(sub).length - 1;
const stripComments = (s) => s.replace(/\/\*[\s\S]*?\*\//g, "").split("\n").filter((l) => !/^\s*\/\//.test(l)).join("\n");

check("语法门：两份产物都可被 vm 编译", () => {
  new vm.Script(main);
  new vm.Script(script);
});

check("接线计数：noAd() 3 处（isAdReady/showAd/realShowAd）、noIap() 9 处（9 渠道；验单走 __starryLocal 标记）", () => {
  const ad = count(stripComments(main), "__starryGate.noAd()");
  const iap = count(stripComments(script), "__starryGate.noIap()");
  if (ad !== 3) throw new Error("noAd 接线数=" + ad);
  if (iap !== 9) throw new Error("noIap 接线数=" + iap);
});

check("产物不依赖 StarryMagic Java 类（无 DwctHooks 引用）", () => {
  if (count(stripComments(main), "com/starry/magic/cocos/DwctHooks") !== 0) throw new Error("main 仍引用 DwctHooks");
  if (count(stripComments(script), "com/starry/magic/cocos/DwctHooks") !== 0) throw new Error("Script 仍引用 DwctHooks");

});

const gateStub = (on) => ({ __starryGate: { noAd: () => on, noIap: () => on }, Date, JSON });

function extractLine(src, needle) {
  const lines = src.split("\n");
  const hit = Array.from(new Set(lines.filter((l) => l.indexOf(needle) >= 0 && l.trim().indexOf("if(") === 0).map((l) => l.trim())));
  if (hit.length !== 1) throw new Error("抽取失败 needle=" + needle + " 去重命中=" + hit.length);
  return hit[0];
}

check("广告拦截：ON 时不进原流程且回调只触发一次；OFF 时进原流程", () => {
  const line = extractLine(main, 'sendMessage("onAdOpen"');
  for (const on of [true, false]) {
    const sent = [];
    const body = new Function("window", "sent", "orig", "return function(t,i,n,a){" + line + "\nreturn orig.apply(this,arguments);}");
    const wrapper = body(gateStub(on), sent, function () { this.origCalled = (this.origCalled || 0) + 1; return "orig"; });
    const self = { sendMessage: function () { sent.push([].slice.call(arguments)); } };
    let cbCount = 0;
    const r = wrapper.call(self, "tag", function () { cbCount++; }, null, null);
    if (on) {
      if (r !== undefined) throw new Error("ON 应提前 return，实际返回 " + r);
      if (cbCount !== 1) throw new Error("ON 回调次数=" + cbCount);
      if (self.origCalled) throw new Error("ON 却进了原流程");
      if (sent.length !== 1 || sent[0][0] !== "onAdOpen") throw new Error("ON 未上报 onAdOpen");
    } else {
      if (r !== "orig") throw new Error("OFF 应进原流程，实际返回 " + r);
      if (self.origCalled !== 1) throw new Error("OFF 原流程调用次数=" + self.origCalled);
      if (cbCount !== 0) throw new Error("OFF 不应由插入代码触发回调");
    }
  }
});

check("内购拦截：ON 返回本地订单标记且不建真实单；OFF 走原渠道流程", () => {
  const line = extractLine(script, '"starry-local"');
  const body = new Function("window", "orig", "return function(t,i){" + line + "\nreturn orig.apply(this,arguments);}");
  const onFn = body(gateStub(true), function () { return Promise.resolve({ from: "real-channel" }); });
  const offFn = body(gateStub(false), function () { return Promise.resolve({ from: "real-channel" }); });
  return Promise.all([onFn("diamond", 1), offFn("diamond", 1)]).then(([a, b]) => {
    if (a.status !== 0 || !a.orderItem || a.orderItem.__starryLocal !== true) throw new Error("ON 未返回本地订单: " + JSON.stringify(a));
    if (a.orderItem.orderId !== "starry-local") throw new Error("本地订单 id 漂移");
    if (b.from !== "real-channel") throw new Error("OFF 未走原渠道");
  });
});

// Promise 断言异步：等 tick 后再收口
setTimeout(() => {
  console.log("----");
  console.log("PASS=" + pass + " FAIL=" + fail + " 产物=" + OUT);
  process.exit(fail === 0 ? 0 : 1);
}, 50);
