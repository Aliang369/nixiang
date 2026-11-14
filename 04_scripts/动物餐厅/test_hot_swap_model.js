#!/usr/bin/env node
/* test_hot_swap_model.js — 双文件替换事务的离线故障注入测试（不碰设备）
 * 覆盖：happy / 已打 / 未知 hash / 混态 / 第二 rename 失败回滚 / 第二 rename 前中断(crash)
 *       / 两文件都换完但 journal 未删的中断 / 残留 tmp / manifest 不变
 * 局限：只证明模型逻辑；native 是否在非下载路径回调 verifyCallback、服务端是否接受、
 *       真机是否生效 均未证。
 */
"use strict";
const fs = require("fs");
const path = require("path");
const assert = require("assert");
const M = require(path.join(__dirname, "hot_swap_model.js"));
const FIX = path.join(__dirname, "fixtures", "hot_swap");
const SB = path.join(__dirname, "..", "..", ".local", "dwct1242_route", "swap_sandbox");
const hashes = JSON.parse(fs.readFileSync(path.join(FIX, "hashes.json"), "utf8"));

let pass = 0, fail = 0;
function check(name, fn) {
  try { fn(); console.log("PASS  " + name); pass++; }
  catch (e) { console.log("FAIL  " + name + " :: " + e.message); fail++; }
}
const h = (p) => M.sha(fs.readFileSync(p));
function sandbox(state) {
  fs.rmSync(SB, { recursive: true, force: true });
  fs.mkdirSync(path.join(SB, "assets/main"), { recursive: true });
  fs.mkdirSync(path.join(SB, "assets/Script"), { recursive: true });
  fs.copyFileSync(path.join(FIX, "targets/assets/main/index.jsc"), path.join(SB, "assets/main/index.jsc"));
  fs.copyFileSync(path.join(FIX, "targets/assets/Script/index.jsc"), path.join(SB, "assets/Script/index.jsc"));
  fs.copyFileSync(path.join(FIX, "targets/project.manifest"), path.join(SB, "project.manifest"));
  if (state === "patched") {
    fs.copyFileSync(path.join(FIX, "incoming/main.jsc"), path.join(SB, "assets/main/index.jsc"));
    fs.copyFileSync(path.join(FIX, "incoming/Script.jsc"), path.join(SB, "assets/Script/index.jsc"));
  }
  if (state === "mixed") {
    fs.copyFileSync(path.join(FIX, "incoming/main.jsc"), path.join(SB, "assets/main/index.jsc"));
  }
  if (state === "unknown") {
    fs.writeFileSync(path.join(SB, "assets/main/index.jsc"), "official pushed a newer hot main");
  }
  return {
    root: SB,
    incoming: { main: path.join(FIX, "incoming/main.jsc"), script: path.join(FIX, "incoming/Script.jsc") },
    hashes,
  };
}

check("happy path：双文件替换成功、journal 清理、manifest 字节不变", () => {
  const o = sandbox("original");
  const mfBefore = h(path.join(SB, "project.manifest"));
  const r = M.runSwap(o);
  assert.strictEqual(r.status, "APPLIED");
  assert.strictEqual(r.after.main, "patched");
  assert.strictEqual(r.after.script, "patched");
  assert.strictEqual(r.manifestUntouched, true);
  assert.strictEqual(h(path.join(SB, "project.manifest")), mfBefore);
  assert.ok(!fs.existsSync(path.join(SB, M.JOURNAL_DIR)), "journal 应被清理");
});

check("幂等：已是 patched ⇒ NOOP，不写 journal", () => {
  const o = sandbox("patched");
  const r = M.runSwap(o);
  assert.strictEqual(r.status, "NOOP_ALREADY_PATCHED");
  assert.ok(!fs.existsSync(path.join(SB, M.JOURNAL_DIR)));
});

check("未知 hash ⇒ ABORT_UNKNOWN，一个字节都不写", () => {
  const o = sandbox("unknown");
  const before = h(path.join(SB, "assets/main/index.jsc"));
  const r = M.runSwap(o);
  assert.strictEqual(r.status, "ABORT_UNKNOWN");
  assert.strictEqual(h(path.join(SB, "assets/main/index.jsc")), before);
  assert.ok(!fs.existsSync(path.join(SB, M.JOURNAL_DIR)));
});

check("起始混态 ⇒ ABORT_MIXED，不盲写", () => {
  const o = sandbox("mixed");
  const r = M.runSwap(o);
  assert.strictEqual(r.status, "ABORT_MIXED");
  assert.strictEqual(M.snapshot(SB, hashes).main, "patched");
  assert.strictEqual(M.snapshot(SB, hashes).script, "original");
});

check("第二文件 rename 失败 ⇒ 回滚第一文件、双原件保全、journal 记 failed_rolled_back", () => {
  const o = sandbox("original");
  o.hooks = { beforeRename: (tag) => { if (tag === "script") throw new Error("inject rename fail"); } };
  const r = M.runSwap(o);
  assert.strictEqual(r.status, "FAILED");
  assert.strictEqual(r.failedAt, "script");
  assert.strictEqual(r.rolledBack, true);
  const after = M.snapshot(SB, hashes);
  assert.strictEqual(after.main, "original");
  assert.strictEqual(after.script, "original");
  const j = JSON.parse(fs.readFileSync(path.join(SB, M.JOURNAL_DIR, "journal.json"), "utf8"));
  assert.strictEqual(j.status, "failed_rolled_back");
  const r2 = M.recover({ root: SB, hashes });
  assert.strictEqual(r2.status, "CLEANED_ROLLED_BACK");
});

check("第二文件之前中断（模拟崩溃，无回滚）⇒ 混态；recover 回滚到双原件", () => {
  const o = sandbox("original");
  o.hooks = { crashOn: true, beforeRename: (tag) => { if (tag === "script") throw new Error("simulated crash"); } };
  const r = M.runSwap(o);
  assert.strictEqual(r.status, "CRASHED");
  assert.strictEqual(M.snapshot(SB, hashes).main, "patched");
  assert.strictEqual(M.snapshot(SB, hashes).script, "original");
  const rr = M.recover({ root: SB, hashes });
  assert.strictEqual(rr.status, "RECOVERED_ROLLBACK");
  assert.strictEqual(M.snapshot(SB, hashes).main, "original");
  assert.strictEqual(M.snapshot(SB, hashes).script, "original");
});

check("两文件都换完但 journal 未删（模拟删除前崩溃）⇒ recover 判 done 并清理，不改文件", () => {
  const o = sandbox("original");
  let crashAfterScript = false;
  o.hooks = { crashOn: true, afterRename: (tag) => { if (tag === "script" && !crashAfterScript) { crashAfterScript = true; throw new Error("crash before journal cleanup"); } } };
  const r = M.runSwap(o);
  assert.strictEqual(r.status, "CRASHED");
  const rr = M.recover({ root: SB, hashes });
  assert.strictEqual(rr.status, "CLEANED_DONE");
  assert.strictEqual(M.snapshot(SB, hashes).main, "patched");
  assert.strictEqual(M.snapshot(SB, hashes).script, "patched");
});

check("残留 .dwct.tmp ⇒ 拒绝覆盖（防半写入）", () => {
  const o = sandbox("original");
  fs.writeFileSync(path.join(SB, "assets/main/index.jsc.dwct.tmp"), "half written");
  const r = M.runSwap(o);
  assert.strictEqual(r.status, "FAILED");
  assert.strictEqual(r.failedAt, "main");
  assert.ok(String(r.error).indexOf("pending tmp") >= 0);
});

console.log("----");
console.log("PASS=" + pass + " FAIL=" + fail + " sandbox=" + SB);
process.exit(fail === 0 ? 0 : 1);
