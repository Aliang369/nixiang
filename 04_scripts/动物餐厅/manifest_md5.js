#!/usr/bin/env node
/*
 * manifest_md5.js -- 更新 Cocos 热更 manifest 中指定资源的 md5/size
 * 语义镜像 12.42 包内 DwctIapGate.updateManifest：只改 assets[rel] 的 md5 与 size，其余字段原样保留。
 * 纯离线：不联网、不碰设备；写盘走 tmp + fsync + rename，失败保留原件。
 *
 * 用法:
 *   node manifest_md5.js <manifest.json> <relPath> <fileToHash> [--dry-run]
 * 退出码: 0=已更新(或 dry-run 可更新)；1=写失败；2=用法/输入错。
 */
"use strict";
const fs = require("fs");
const crypto = require("crypto");
const path = require("path");

function die(code, msg) {
  process.stderr.write("ERROR: " + msg + "\n");
  process.exit(code);
}

const argv = process.argv.slice(2);
const dryRun = argv.includes("--dry-run");
const args = argv.filter((a) => a !== "--dry-run");
if (args.length !== 3) {
  die(2, "用法: node manifest_md5.js <manifest.json> <relPath> <fileToHash> [--dry-run]");
}
const manifestPath = args[0], rel = args[1], filePath = args[2];

let raw;
try {
  raw = fs.readFileSync(manifestPath, "utf8");
} catch (e) {
  die(2, "读不到 manifest: " + manifestPath);
}
let doc;
try {
  doc = JSON.parse(raw);
} catch (e) {
  die(2, "manifest 不是合法 JSON: " + e.message);
}
if (!doc.assets || typeof doc.assets !== "object") die(2, "manifest 缺 assets 对象");
if (!Object.prototype.hasOwnProperty.call(doc.assets, rel)) die(2, "assets 里没有条目: " + rel);

let bytes;
try {
  bytes = fs.readFileSync(filePath);
} catch (e) {
  die(2, "读不到待哈希文件: " + filePath);
}
const md5 = crypto.createHash("md5").update(bytes).digest("hex");
const size = bytes.length;
const entry = doc.assets[rel];
const before = { md5: entry.md5, size: entry.size };
const changed = before.md5 !== md5 || before.size !== size;
if (changed) {
  entry.md5 = md5;
  entry.size = size;
}

const out = JSON.stringify(doc);
const summary = {
  manifest: path.basename(manifestPath),
  rel: rel,
  file: path.basename(filePath),
  before: before,
  after: { md5: md5, size: size },
  changed: changed,
  manifestBytesBefore: Buffer.byteLength(raw),
  manifestBytesAfter: Buffer.byteLength(out),
};

if (!changed) {
  summary.result = "NOOP";
  process.stdout.write(JSON.stringify(summary) + "\n");
  process.exit(0);
}
if (dryRun) {
  summary.result = "DRY_RUN";
  process.stdout.write(JSON.stringify(summary) + "\n");
  process.exit(0);
}

const tmp = manifestPath + ".manifestmd5.tmp";
try {
  const fd = fs.openSync(tmp, "w");
  try {
    fs.writeFileSync(fd, out);
    fs.fsyncSync(fd);
  } finally {
    fs.closeSync(fd);
  }
  fs.renameSync(tmp, manifestPath);
} catch (e) {
  try { if (fs.existsSync(tmp)) fs.unlinkSync(tmp); } catch (_) {}
  die(1, "写 manifest 失败: " + e.message);
}
summary.result = "UPDATED";
process.stdout.write(JSON.stringify(summary) + "\n");
