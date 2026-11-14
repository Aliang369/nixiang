#!/usr/bin/env node
/* test_js_only_flags.js — js_only_flags.js 的真实运行 fixture（不联网、不碰设备）
 * 覆盖：无 jsb / 无文件 / ON / 切回 OFF / 坏 JSON / 非 true 值 / 缺字段 / 缓存 TTL / reload
 * 退出码 0=全过；1=有失败
 */
"use strict";
const path = require("path");
const assert = require("assert");
const MOD = path.join(__dirname, "js_only_flags.js");

let pass = 0, fail = 0;
function check(name, fn) {
  try { fn(); console.log("PASS  " + name); pass++; }
  catch (e) { console.log("FAIL  " + name + " :: " + e.message); fail++; }
}

function freshGate(files) {
  delete globalThis.__starryGate;
  delete require.cache[require.resolve(MOD)];
  globalThis.jsb = {
    fileUtils: {
      getWritablePath: () => "/data/user/0/inland.twgame.restaurant/files/",
      isFileExist: (p) => Object.prototype.hasOwnProperty.call(files, p),
      getStringFromFile: (p) => (Object.prototype.hasOwnProperty.call(files, p) ? files[p] : ""),
    },
  };
  require(MOD);
  return globalThis.__starryGate;
}
const P = "/data/user/0/inland.twgame.restaurant/files/dwct_flags.json";

check("无 jsb 时两开关恒 false（不抛异常）", () => {
  delete globalThis.__starryGate;
  delete require.cache[require.resolve(MOD)];
  delete globalThis.jsb;
  require(MOD);
  const g = globalThis.__starryGate;
  assert.strictEqual(g.noAd(), false);
  assert.strictEqual(g.noIap(), false);
});

check("无配置文件时两开关 false", () => {
  const g = freshGate({});
  assert.strictEqual(g.noAd(), false);
  assert.strictEqual(g.noIap(), false);
  assert.strictEqual(g.status().ok, false);
});

check("noAd=true 生效、noIap 缺省为 false（两开关独立）", () => {
  const g = freshGate({ [P]: '{"noAd":true}' });
  assert.strictEqual(g.noAd(), true);
  assert.strictEqual(g.noIap(), false);
});

check("两开关同时 ON", () => {
  const g = freshGate({ [P]: '{"noAd":true,"noIap":true}' });
  assert.strictEqual(g.noAd(), true);
  assert.strictEqual(g.noIap(), true);
});

check("切回 OFF：同一进程内 reload 后恢复原流程", () => {
  const files = { [P]: '{"noAd":true,"noIap":true}' };
  const g = freshGate(files);
  assert.strictEqual(g.noAd(), true);
  files[P] = '{"noAd":false,"noIap":false}';
  g.reload();
  assert.strictEqual(g.noAd(), false);
  assert.strictEqual(g.noIap(), false);
});

check("缓存 TTL：改文件后未 reload 仍读旧值，超过 TTL 自动刷新", () => {
  const files = { [P]: '{"noAd":true}' };
  const g = freshGate(files);
  assert.strictEqual(g.noAd(), true);
  files[P] = '{"noAd":false}';
  assert.strictEqual(g.noAd(), true, "TTL 内应保持缓存值");
  const realNow = Date.now;
  Date.now = () => realNow() + 5000;
  try { assert.strictEqual(g.noAd(), false, "超过 TTL 应刷新为 false"); }
  finally { Date.now = realNow; }
});

check("坏 JSON 时两开关 false（不崩）", () => {
  const g = freshGate({ [P]: '{noAd:true,' });
  assert.strictEqual(g.noAd(), false);
  assert.strictEqual(g.noIap(), false);
  assert.strictEqual(g.status().ok, false);
});

check("非布尔 true（字符串/数字）不算开启", () => {
  const g = freshGate({ [P]: '{"noAd":"true","noIap":1}' });
  assert.strictEqual(g.noAd(), false);
  assert.strictEqual(g.noIap(), false);
});

check("顶层非对象（数组/字符串）不算开启", () => {
  const g = freshGate({ [P]: '[true]' });
  assert.strictEqual(g.noAd(), false);
  assert.strictEqual(g.status().ok, false);
});

console.log("----");
console.log("PASS=" + pass + " FAIL=" + fail);
process.exit(fail === 0 ? 0 : 1);
