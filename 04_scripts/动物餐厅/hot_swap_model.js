#!/usr/bin/env node
/*
 * hot_swap_model.js -- 动物餐厅 12.42「原签热更」双文件替换的**离线事务模型**（不碰设备）
 *
 * 为什么要模型：设备侧动作（root cp + rename）无法离线复现故障，而"两个文件各自 rename"
 * 不是原子事务。本文件把落地算法写成一个可注入故障的纯模型，用 fixture 把
 * 三态/第二文件失败/中断恢复/原件保全 全部跑出来；设备侧脚本按同一顺序实现。
 *
 * 算法（与审查约定一致）
 *  1) 读两个目标实测 sha256 分类三态：original / patched / unknown
 *     - 任一 unknown           -> ABORT_UNKNOWN（一个字节都不写）
 *     - 两个都 patched         -> NOOP_ALREADY_PATCHED
 *     - 一 patched 一 original -> ABORT_MIXED（要求先 recover，不盲写）
 *  2) 备份两份原件到 <root>/.dwct_swap_journal/backup/，写 journal（含期望哈希、顺序）
 *  3) 逐个 tmp + rename（顺序：main 先、Script 后；理由见下）
 *  4) 全部成功 -> 校验 + 断言 project.manifest 字节未变 -> 删除 journal
 *  5) 任一步失败 -> 用备份回滚已替换的文件 -> journal 标 failed_rolled_back（保留取证）
 *  6) recover() -> 有 journal 时按实测哈希判：双 original / 双 patched / 混合
 *     - 混合一律回滚到备份并报告，不继续
 *
 * 顺序理由：main 的 bootstrap 定义了 __starryGate；Script 的门写作
 *   window.__starryGate && window.__starryGate.noIap()
 * 两种半套状态都只会让门恒 false（不会崩），但语义不一致，仍按"混合即回滚"处理。
 *
 * 边界（必须随结果一起引用）：本模型只证明 JS 门命中位置、替换后哈希/语法与回滚流程；
 * 不能证明 native jsb.AssetsManager 是否在非下载路径调用 verifyCallback、不能证明服务端
 * 接受、不能证明真机生效。signed manifest 一律不动。
 */
"use strict";
const fs = require("fs");
const path = require("path");
const crypto = require("crypto");

const sha = (b) => crypto.createHash("sha256").update(b).digest("hex");
const REL_MAIN = "assets/main/index.jsc";
const REL_SCRIPT = "assets/Script/index.jsc";
const JOURNAL_DIR = ".dwct_swap_journal";
const JOURNAL = JOURNAL_DIR + "/journal.json";
const BACKUP = JOURNAL_DIR + "/backup";

function readMaybe(p) {
  try { return fs.readFileSync(p); } catch (e) { return null; }
}

function classify(bytes, hashes) {
  if (!bytes) return "missing";
  const h = sha(bytes);
  if (hashes.original.indexOf(h) >= 0) return "original";
  if (hashes.patched.indexOf(h) >= 0) return "patched";
  return "unknown:" + h;
}

function snapshot(root, hashes) {
  return {
    main: classify(readMaybe(path.join(root, REL_MAIN)), hashes),
    script: classify(readMaybe(path.join(root, REL_SCRIPT)), hashes),
  };
}

function writeAtomic(file, bytes, hooks, tag) {
  const tmp = file + ".dwct.tmp";
  if (hooks && hooks.beforeAtomic) hooks.beforeAtomic(tag, file);
  if (readMaybe(tmp)) throw new Error("pending tmp exists: " + tmp);
  const fd = fs.openSync(tmp, "w");
  try { fs.writeFileSync(fd, bytes); fs.fsyncSync(fd); } finally { fs.closeSync(fd); }
  if (sha(fs.readFileSync(tmp)) !== sha(bytes)) { fs.unlinkSync(tmp); throw new Error("tmp verify failed: " + tag); }
  if (hooks && hooks.beforeRename) hooks.beforeRename(tag, file);
  fs.renameSync(tmp, file);
  if (hooks && hooks.afterRename) hooks.afterRename(tag, file);
}

function writeJournal(root, doc) {
  const jp = path.join(root, JOURNAL);
  fs.mkdirSync(path.dirname(jp), { recursive: true });
  writeAtomic(jp, Buffer.from(JSON.stringify(doc, null, 1)), null, "journal");
}

function listJournal(root) {
  const jp = path.join(root, JOURNAL);
  const b = readMaybe(jp);
  if (!b) return null;
  try { return JSON.parse(b.toString("utf8")); } catch (e) { return { corrupt: true }; }
}

function runSwap(opts) {
  const { root, incoming, hashes } = opts;
  const hooks = opts.hooks || {};
  const report = { root };
  const before = snapshot(root, hashes);
  report.before = before;

  const vals = [before.main, before.script];
  if (vals.some((v) => v.indexOf("unknown") === 0 || v === "missing")) {
    report.status = "ABORT_UNKNOWN";
    report.unknown = before;
    return report;
  }
  if (before.main === "patched" && before.script === "patched") {
    report.status = "NOOP_ALREADY_PATCHED";
    return report;
  }
  if (before.main !== before.script) {
    report.status = "ABORT_MIXED";
    report.hint = "先跑 recover()";
    return report;
  }

  const manifestBefore = readMaybe(path.join(root, "project.manifest"));
  const plan = [
    { tag: "main", rel: REL_MAIN, src: incoming.main },
    { tag: "script", rel: REL_SCRIPT, src: incoming.script },
  ];
  const jdoc = {
    createdAt: new Date(0).toISOString(),
    expected: hashes,
    order: plan.map((p) => p.tag),
    status: "in_progress",
    swapped: [],
  };
  // 备份 + journal
  fs.mkdirSync(path.join(root, BACKUP, "assets/main"), { recursive: true });
  fs.mkdirSync(path.join(root, BACKUP, "assets/Script"), { recursive: true });
  for (const p of plan) {
    fs.writeFileSync(path.join(root, BACKUP, p.rel), fs.readFileSync(path.join(root, p.rel)));
  }
  writeJournal(root, jdoc);

  for (const p of plan) {
    const bytes = fs.readFileSync(p.src);
    try {
      writeAtomic(path.join(root, p.rel), bytes, hooks, p.tag);
    } catch (e) {
      report.status = hooks.crashOn ? "CRASHED" : "FAILED";
      report.error = String(e.message || e);
      report.failedAt = p.tag;
      if (!hooks.crashOn) {
        // 回滚已替换的文件
        for (const done of jdoc.swapped) {
          fs.copyFileSync(path.join(root, BACKUP, done.rel), path.join(root, done.rel));
        }
        jdoc.status = "failed_rolled_back";
        jdoc.failedAt = p.tag;
        writeJournal(root, jdoc);
        report.rolledBack = true;
      }
      report.after = snapshot(root, hashes);
      return report;
    }
    jdoc.swapped.push({ tag: p.tag, rel: p.rel });
    writeJournal(root, jdoc);
  }

  const after = snapshot(root, hashes);
  report.after = after;
  const manifestAfter = readMaybe(path.join(root, "project.manifest"));
  report.manifestUntouched = manifestBefore && manifestAfter ? sha(manifestBefore) === sha(manifestAfter) : null;
  if (after.main !== "patched" || after.script !== "patched" || report.manifestUntouched === false) {
    report.status = "POST_VERIFY_FAILED";
    return report;
  }
  jdoc.status = "done";
  writeJournal(root, jdoc);
  fs.unlinkSync(path.join(root, JOURNAL));
  fs.rmSync(path.join(root, JOURNAL_DIR), { recursive: true, force: true });
  report.status = "APPLIED";
  return report;
}

function recover(opts) {
  const { root, hashes } = opts;
  const jdoc = listJournal(root);
  const report = { root, haveJournal: !!jdoc };
  if (!jdoc) { report.status = "NO_JOURNAL"; report.state = snapshot(root, hashes); return report; }
  const state = snapshot(root, hashes);
  report.state = state;
  const vals = [state.main, state.script];
  if (vals.some((v) => v.indexOf("unknown") === 0 || v === "missing")) {
    report.status = "MANUAL_REVIEW_UNKNOWN";
    return report;
  }
  if (state.main === "patched" && state.script === "patched") {
    fs.rmSync(path.join(root, JOURNAL_DIR), { recursive: true, force: true });
    report.status = "CLEANED_DONE";
    return report;
  }
  if (state.main === "original" && state.script === "original") {
    fs.rmSync(path.join(root, JOURNAL_DIR), { recursive: true, force: true });
    report.status = "CLEANED_ROLLED_BACK";
    return report;
  }
  // 混态：回滚到备份
  let restored = 0;
  for (const rel of [REL_MAIN, REL_SCRIPT]) {
    const bp = path.join(root, BACKUP, rel);
    if (readMaybe(bp)) { fs.copyFileSync(bp, path.join(root, rel)); restored++; }
  }
  const after = snapshot(root, hashes);
  report.restored = restored;
  report.after = after;
  report.status = after.main === "original" && after.script === "original" ? "RECOVERED_ROLLBACK" : "RECOVER_FAILED";
  if (report.status === "RECOVERED_ROLLBACK") fs.rmSync(path.join(root, JOURNAL_DIR), { recursive: true, force: true });
  return report;
}

module.exports = { runSwap, recover, snapshot, sha, REL_MAIN, REL_SCRIPT, JOURNAL_DIR };

if (require.main === module) {
  const p = process.argv[2];
  if (p !== "recover") { console.error("用法: node hot_swap_model.js recover --root <沙盒>"); process.exit(2); }
  const root = process.argv[process.argv.indexOf("--root") + 1];
  const hashes = JSON.parse(fs.readFileSync(path.join(__dirname, "fixtures/hot_swap/hashes.json"), "utf8"));
  console.log(JSON.stringify(recover({ root, hashes }), null, 1));
}
