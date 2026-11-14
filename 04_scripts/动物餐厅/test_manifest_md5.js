#!/usr/bin/env node
/* test_manifest_md5.js — manifest_md5.js 的真实 JSON fixture 门禁（纯离线）
 * 用 fixtures/project.manifest.sample.json（取自设备 12.42 热更 project.manifest 的真实条目）
 * 覆盖：首次更新 / 幂等 NOOP / 其它字段不变 / 未知条目拒绝且原件不动 / 坏 JSON 拒绝 / version.manifest 同语义
 */
"use strict";
const fs = require("fs");
const path = require("path");
const crypto = require("crypto");
const { spawnSync } = require("child_process");

const HERE = __dirname;
const HELPER = path.join(HERE, "manifest_md5.js");
const FIXTURE = path.join(HERE, "fixtures", "project.manifest.sample.json");
const TMP = path.join(HERE, "..", "..", ".local", "dwct1242_route", "manifest_fixture");

let pass = 0, fail = 0;
function check(name, fn) {
  try { fn(); console.log("PASS  " + name); pass++; }
  catch (e) { console.log("FAIL  " + name + " :: " + e.message); fail++; }
}
const md5 = (b) => crypto.createHash("md5").update(b).digest("hex");
const run = (args) => spawnSync("node", [HELPER].concat(args), { encoding: "utf8" });

fs.mkdirSync(TMP, { recursive: true });
const baseManifest = fs.readFileSync(FIXTURE);
const MAIN_REL = "assets/main/index.jsc";
const SCRIPT_REL = "assets/Script/index.jsc";

function reset() {
  for (const f of ["project.manifest", "version.manifest", "main.payload", "Script.payload", "bad.manifest"]) {
    const p = path.join(TMP, f);
    if (fs.existsSync(p)) fs.unlinkSync(p);
  }
  fs.writeFileSync(path.join(TMP, "project.manifest"), baseManifest);
  fs.writeFileSync(path.join(TMP, "version.manifest"), baseManifest);
  fs.writeFileSync(path.join(TMP, "main.payload"), Buffer.from("main-payload-v1"));
  fs.writeFileSync(path.join(TMP, "Script.payload"), Buffer.from("script-payload-v1-longer"));
}

check("首次更新：md5/size 被写入且与 payload 实测一致", () => {
  reset();
  const mf = path.join(TMP, "project.manifest");
  const r1 = run([mf, MAIN_REL, path.join(TMP, "main.payload")]);
  const r2 = run([mf, SCRIPT_REL, path.join(TMP, "Script.payload")]);
  if (r1.status !== 0 || r2.status !== 0) throw new Error("helper 退出码 " + r1.status + "/" + r2.status + " " + r1.stderr + r2.stderr);
  const doc = JSON.parse(fs.readFileSync(mf, "utf8"));
  const mp = fs.readFileSync(path.join(TMP, "main.payload"));
  const sp = fs.readFileSync(path.join(TMP, "Script.payload"));
  if (doc.assets[MAIN_REL].md5 !== md5(mp)) throw new Error("main md5 不符");
  if (doc.assets[MAIN_REL].size !== mp.length) throw new Error("main size 不符");
  if (doc.assets[SCRIPT_REL].md5 !== md5(sp)) throw new Error("Script md5 不符");
  if (doc.assets[SCRIPT_REL].size !== sp.length) throw new Error("Script size 不符");
});

check("其它字段保持原值（version/packageUrl/sign/searchPaths/其它条目）", () => {
  const before = JSON.parse(baseManifest.toString("utf8"));
  const after = JSON.parse(fs.readFileSync(path.join(TMP, "project.manifest"), "utf8"));
  for (const k of ["version", "packageUrl", "remoteManifestUrl", "sign", "updating"]) {
    if (JSON.stringify(before[k]) !== JSON.stringify(after[k])) throw new Error("字段被改: " + k);
  }
  if (JSON.stringify(before.searchPaths) !== JSON.stringify(after.searchPaths)) throw new Error("searchPaths 被改");
  const untouched = Object.keys(before.assets).filter((k) => k !== MAIN_REL && k !== SCRIPT_REL);
  for (const k of untouched) {
    if (JSON.stringify(before.assets[k]) !== JSON.stringify(after.assets[k])) throw new Error("无关条目被改: " + k);
  }
});

check("幂等：同一 payload 再跑一次 = NOOP，文件字节不变", () => {
  const mf = path.join(TMP, "project.manifest");
  const bytesBefore = fs.readFileSync(mf);
  const r = run([mf, MAIN_REL, path.join(TMP, "main.payload")]);
  if (r.status !== 0) throw new Error("退出码 " + r.status + " " + r.stderr);
  let out;
  try { out = JSON.parse((r.stdout || "").trim().split("\n").pop()); } catch (e) { throw new Error("stdout 不是 JSON: " + r.stdout); }
  if (out.result !== "NOOP") throw new Error("期望 NOOP，实际 " + out.result);
  if (!bytesBefore.equals(fs.readFileSync(mf))) throw new Error("NOOP 却改了文件字节");
});

check("dry-run 不写盘", () => {
  reset();
  const mf = path.join(TMP, "project.manifest");
  const before = fs.readFileSync(mf);
  const r = run([mf, MAIN_REL, path.join(TMP, "main.payload"), "--dry-run"]);
  if (r.status !== 0) throw new Error("退出码 " + r.status);
  let out; try { out = JSON.parse((r.stdout || "").trim().split("\n").pop()); } catch (e) { throw new Error("stdout 不是 JSON"); }
  if (out.result !== "DRY_RUN") throw new Error("期望 DRY_RUN，实际 " + out.result);
  if (!before.equals(fs.readFileSync(mf))) throw new Error("dry-run 改了文件");
});

check("未知条目：退出码 2，原件字节不变", () => {
  const mf = path.join(TMP, "project.manifest");
  const before = fs.readFileSync(mf);
  const r = run([mf, "assets/not/there.jsc", path.join(TMP, "main.payload")]);
  if (r.status !== 2) throw new Error("期望退出码 2，实际 " + r.status);
  if (!before.equals(fs.readFileSync(mf))) throw new Error("失败却改了文件");
});

check("坏 JSON：退出码 2，原件字节不变", () => {
  const mf = path.join(TMP, "bad.manifest");
  fs.writeFileSync(mf, "{oops");
  const before = fs.readFileSync(mf);
  const r = run([mf, MAIN_REL, path.join(TMP, "main.payload")]);
  if (r.status !== 2) throw new Error("期望退出码 2，实际 " + r.status);
  if (!before.equals(fs.readFileSync(mf))) throw new Error("失败却改了文件");
});

check("version.manifest 同语义（改完与 project.manifest 的对应条目一致）", () => {
  reset();
  const vm = path.join(TMP, "version.manifest");
  const pm = path.join(TMP, "project.manifest");
  const r = run([vm, MAIN_REL, path.join(TMP, "main.payload")]);
  if (r.status !== 0) throw new Error("退出码 " + r.status + " " + r.stderr);
  const r2 = run([pm, MAIN_REL, path.join(TMP, "main.payload")]);
  if (r2.status !== 0) throw new Error("project 更新退出码 " + r2.status);
  const v = JSON.parse(fs.readFileSync(vm, "utf8"));
  const m = JSON.parse(fs.readFileSync(path.join(TMP, "project.manifest"), "utf8"));
  if (v.assets[MAIN_REL].md5 !== m.assets[MAIN_REL].md5) throw new Error("双 manifest 不一致");
});

console.log("----");
console.log("PASS=" + pass + " FAIL=" + fail + " tmp=" + TMP);
process.exit(fail === 0 ? 0 : 1);
