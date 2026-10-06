#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""勇士之心 1.0.3 —— 由原生补丁源生成「模拟器自举版」引导 JS（ysxz_boot.js）。

为什么需要它（2026-10-05 实测）：
  原生路线靠 Dobby inline hook 改写 libcocos2djs.so 的 evalString 机器码。
  在 MuMu(Houdini/x86_64 翻译层) 下 ARM 代码段是 r--p、真正执行的是翻译后的
  代码，改 ARM 字节不生效 ⇒ 钩子「装上但一次都不命中」（rd=1 hook=1，
  logic splice 0 次）。因此模拟器侧改走纯 JS 路线：

    把本脚本产物追加到 files/res/main.js 尾部 →
      1) 属性陷阱接管 window.__require（bundle 求值时会赋值它）
      2) 用 window.__require(id) 拿 exports，用导出函数 toString() 反推"模块源码"
      3) 复用原生补丁的全部指纹发现/挂钩逻辑（tryLogic/Entity/System/Auto）

与原生路线的关系（运行时判据，禁机型分叉）：
  * so 在【真正截获到 evalString】时写 pid 证明文件 files/ysxz_native_ok；
  * 本驱动启动时若读到「同 pid 的证明文件」⇒ 原生 splice 已生效 ⇒ 自动让位；
  * 读不到（含 MuMu 这种钩子不命中的情形）⇒ 自举生效。
  两条路线共享同一份补丁源（YsxzPatchJs.h），不复制逻辑。

额外修正：
  * MOD_boot 原版每次调用 `var N = {}`（局部），增量装钩会重复包同一个方法
    ⇒ 这里让 N 挂在 globalThis.__starry_N 上，增量扫描只补没装过的钩子。

用法：
  python3.11 gen_ysxz_boot_js.py [--out FILE] [--ids FILE] [--patch-js FILE]
"""
from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
MENU = HERE / "ysxz_1.0.3_menu"
HEADER = MENU / "YsxzPatchJs.h"
IDS = MENU / "boot_ids.json"
OUT = HERE.parents[1] / "mod菜单/starry-mod-menu/libs_ysxz/ysxz_boot.js"

SEC7_HEAD = "    /* ---------- 7. 启动: 先扫全部模块源码取名, 再懒包装目标模块 ---------- */"
SCAN_DEF = "    function scan(exp) {"
TARGETS_HEAD = "    var targets = [];"
TARGETS_TAIL = '    log("wrapped " + targets.length + " target modules: " + targets.join(","));'
TAIL_HEAD = "window.__starry_capture = function (reg) {"
N_DECL = "function MOD_boot(REG) {\n    var N = {};"

SCAN_BLOCK = '''    function scan(exp) {
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
    }'''

NEW_N_DECL = '''function MOD_boot(REG) {
    /* 增量扫描多趟调用：N 必须跨趟共享，否则第二趟会把第一趟包过的方法再包一层
       （2026-10-05 实测：第二趟把 damage formula 从 .bJV 换成 .GD 又重新挂钩）。 */
    var N = (typeof globalThis !== "undefined" && globalThis)
          ? (globalThis.__starry_N = globalThis.__starry_N || {}) : {};'''

NEW_SCAN = '''    /* scan 必须递归: 模块 exports 常是 { default: Class } 或多层命名空间, 原版只扫一层会漏掉类 */
    /* 与原生 scan 同语义: tryLogic 只看类的【静态】方法(伤害公式是 .GD 这类静态方法);
       tryEntity/trySystem/tryAuto 自己取 .prototype。
       2026-10-06 MuMu 实测: 把 prototype 方法并进 tryLogic 的扫描面, 会让游戏包属性初始化
       方法 .bJV(含 maxApDefRate/baseCritDamage 明文, 但无 atkEntity)抢占逻辑钩子, 真伤害
       公式 battleLogic .GD 再也挂不上 -> 秒杀/伤害倍率同时失效。禁止再合并 prototype。 */
    function hit(V) {
        try { tryLogic(V); } catch (e) { log("tryLogic ERR: " + e); }
        try { tryEntity(V); } catch (e) { log("tryEntity ERR: " + e); }
        try { trySystem(V); } catch (e) { log("trySystem ERR: " + e); }
        try { tryAuto(V); } catch (e) { log("tryAuto ERR: " + e); }
    }
    function scanObj(exp, depth, seen) {
        if (!exp) return;
        var t = typeof exp;
        if (t !== "object" && t !== "function") return;
        if (seen.indexOf(exp) >= 0) return;
        seen.push(exp);
        if (t === "function") hit(exp);
        for (var k in exp) {
            var V;
            try { V = exp[k]; } catch (e) { continue; }
            if (typeof V === "function") hit(V);
            else if (V && typeof V === "object" && depth > 0) scanObj(V, depth - 1, seen);
        }
    }
    function scan(exp) { scanObj(exp, 2, []); }'''

NEW_SEC7_HEAD = '''    /* ---------- 7. 模拟器路线: 无 REG 可用, 改从 window.__require 的 exports 反推源码 ---------- */
    /* 原生 evalString 挂钩在 Houdini(x86 模拟器)下不可行 —— ARM 代码段没有 x 权限,
       且代码由翻译层执行, 补丁指令不会生效。这里改用引导脚本自举:
       拿 bundle 的 window.__require(id) 取 exports, 用导出函数(含静态方法与原型方法)
       的 Function.prototype.toString() 拼出"伪模块源码", 再走完全相同的指纹发现逻辑。 */
    function walk(x, depth, out, seen) {
        if (!x || depth < 0) return;
        var t = typeof x;
        if (t !== "function" && t !== "object") return;
        if (seen.indexOf(x) >= 0) return;
        seen.push(x);
        var k, v, pk, pv;
        if (t === "function") {
            try { out.push(srcOf(x)); } catch (e0) {}
            try {
                if (x.prototype) for (k in x.prototype) {
                    try { pv = x.prototype[k]; if (typeof pv === "function") out.push(srcOf(pv)); } catch (e1) {}
                }
            } catch (e2) {}
        }
        for (k in x) {
            try { v = x[k]; } catch (e3) { continue; }
            if (typeof v === "function") {
                try { out.push(srcOf(v)); } catch (e4) {}
                try {
                    if (v.prototype) for (pk in v.prototype) {
                        try { pv = v.prototype[pk]; if (typeof pv === "function") out.push(srcOf(pv)); } catch (e5) {}
                    }
                } catch (e6) {}
            } else if (v && typeof v === "object") walk(v, depth - 1, out, seen);
        }
    }
    function srcOfExports(e) {
        var out = [], seen = [];
        walk(e, 2, out, seen);
        return out.join("\\n");
    }
    var srcs = {}, id, nmod = 0;
    var exps = (REG && REG.exps) || {}, idList = (REG && REG.ids) || [];
    for (var ii = 0; ii < idList.length; ii++) {
        id = idList[ii];
        var s0 = srcOfExports(exps[id]);
        if (s0) { srcs[id] = s0; nmod++; }
    }
    log("boot(exports): " + nmod + "/" + idList.length + " 个模块拿到源码, toString " +
        (nmod > 0 ? "OK" : "UNAVAILABLE — 自愈无法工作"));
    for (id in srcs) harvestA(srcs[id]);
    for (id in srcs) harvestB(srcs[id]);
    /* ---- 1.1.4.0 离线兜底: exports 反推的源码里缺 A1 片段时 harvestA 抓不到 hx/isPlayer ----
       (离线 _t_harvest_1140.py 实测: $KC @75430 case "isPlayer": return t.hy.bmN;
        其它锚点: hE.blx / bnR.fF.ddQ / hp=bhd / world=hj)
       只在真正 undefined 时才填, 不覆盖运行时抓到的值。 */
    var FIXED_1140 = { hx: "hy", isPlayer: "bmN", hD: "hE", camp: "blx",
                       ctxKey: "bnR", teams: "fF", teamCamp: "ddQ", hp: "bhd", world: "hj" };
    if (N.hx === undefined || N.isPlayer === undefined) {
        N.hx = FIXED_1140.hx; N.isPlayer = FIXED_1140.isPlayer;
        log("fallback A1(1.1.4.0): hx=" + N.hx + " isPlayer=" + N.isPlayer);
    }
    if (N.hp === undefined) { N.hp = FIXED_1140.hp; log("fallback hp=" + N.hp); }
    if (N.world === undefined) { N.world = FIXED_1140.world; log("fallback world=" + N.world); }
    if (N.ctxKey === undefined) {
        N.ctxKey = FIXED_1140.ctxKey; N.teams = FIXED_1140.teams; N.teamCamp = FIXED_1140.teamCamp;
        log("fallback ctx=" + N.ctxKey + "/" + N.teams + "/" + N.teamCamp);
    }
    if (N.hD === undefined) { N.hD = FIXED_1140.hD; N.camp = FIXED_1140.camp;
        log("fallback camp=" + N.hD + "." + N.camp); }
    MOD_boot.names = N;                /* 排查用: 控制台看 MOD_boot.names */
    log("names: hx=" + N.hx + "." + N.isPlayer + "  camp=" + N.hD + "." + N.camp +
        "  ctx=" + N.ctxKey + "/" + N.teams + "/" + N.teamCamp + "  hp=" + N.hp + "  world=" + N.world);
'''

NEW_TARGETS = '''    var targets = [], missed = [];
    /* exports 反推的源码只覆盖导出成员, 无法预判哪个模块藏着死亡流程/技能消耗
       (1.1.4.0 实测: 死亡流程 t.Hs、技能消耗 t.FL 都在 $KC, 而 $KC 不含分类指纹)
       ⇒ 不再按指纹筛模块, 直接把所有拿到源码的模块导出都扫一遍(含原型链一层)。 */
    for (id in srcs) {
        targets.push(id);
        try { scan(exps[id]); } catch (e) { missed.push(id + ":" + e); }
    }
    for (id in exps) if (srcs[id] === undefined) {
        try { scan(exps[id]); } catch (e) {}
    }
    log("scanned all " + targets.length + " modules with source (exps=" + (function () {
        var c = 0; for (var k in exps) c++; return c;
    })() + "), hooks: logic=" + (N.logicDone ? 1 : 0) + " ent=" + (N.entDone ? 1 : 0) +
        " sys=" + (N.sysDone ? 1 : 0) + " auto=" + (N.autoDone ? 1 : 0));
    if (missed.length) log("scan ERR " + missed.join(" | "));
'''

DRIVER = '''/* ---------- 模拟器自举驱动: 不挂钩原生, 直接借 window.__require ---------- */
(function () {
  if (window.__starry_mumu_v1) return;
  window.__starry_mumu_v1 = 1;
  var IDS = __IDS__;
  var cur = window.__require;
  var exps = {}, pass = 0, lastReady = 0, stable = 0, bootedReady = -1, pending = IDS.slice(0);
  var stoodDown = false;

  function nlog(s) {
    var m = "[MODMOD] " + s;
    try { if (window.__starry_nlog) window.__starry_nlog(m); } catch (e) {}
    try { if (typeof console !== "undefined" && console.log) console.log(m); } catch (e) {}
  }

  /* ---- 运行时判据: 原生 splice 是否真的生效 ----
     so 只有在【真的截获到 evalString】时才写 files/ysxz_native_ok（内容=pid）。
     写不出/读不到 ⇒ 原生路线在这台机器上不生效（模拟器翻译层）⇒ 自举接管；
     读到同 pid ⇒ 原生路线已在工作 ⇒ 自举让位，避免同一方法被包两层。 */
  function nativeActive() {
    try {
      if (typeof jsb === "undefined" || !jsb.fileUtils || !jsb.reflection) return false;
      var p = jsb.fileUtils.getWritablePath() + "ysxz_native_ok";
      if (!jsb.fileUtils.isFileExist(p)) return false;
      var s = jsb.fileUtils.getStringFromFile(p) || "";
      var m = /[0-9]+/.exec(s);
      if (!m) return false;
      var pid = jsb.reflection.callStaticMethod("android.os.Process", "myPid", "()I");
      if (!pid) return false;
      var same = ("" + pid) === m[0];
      if (!same) nlog("native proof pid=" + m[0] + " != my pid=" + pid + " (stale, ignored)");
      return same;
    } catch (e) { return false; }
  }
  if (nativeActive()) { nlog("native splice active (proof pid ok) -> bootstrap stand-down"); return; }

  function resolve() {
    if (stoodDown) return;
    if (typeof cur !== "function") return;
    var still = [], i, id, e;
    for (i = 0; i < pending.length; i++) {
      id = pending[i]; e = null;
      try { e = cur(id); } catch (er) { e = null; }
      if (e && (typeof e === "object" || typeof e === "function")) exps[id] = e;
      else still.push(id);
    }
    pending = still;
    var ready = IDS.length - pending.length;
    if (ready !== lastReady) {
      nlog("resolve: " + ready + "/" + IDS.length + " 模块就绪" +
           (pending.length ? "  未就绪=" + pending.join(",") : ""));
    } else if (pass % 40 === 0) {
      nlog("resolve: 仍 " + ready + "/" + IDS.length + " 模块就绪 (pass " + pass + ")");
    }
    /* ready 还在长 -> 等它稳住; 稳住 3 个 tick 后按当前 exports 装一次钩子。
       后续模块到位会再装一次 —— MOD_boot 内部 N 跨趟共享, 只补没装的钩子。 */
    if (ready !== lastReady) { lastReady = ready; stable = pass; return; }
    if (ready > 0 && ready !== bootedReady && (ready === IDS.length || pass - stable >= 2)) {
      /* 装钩前再判一次原生证明: 防 driver 启动时证明还没写出、随后原生才命中的竞态 */
      if (nativeActive()) {
        nlog("native splice active (late) -> bootstrap stand-down");
        stoodDown = true;
        return;
      }
      bootedReady = ready;
      try { MOD_boot({ __exportsMode: 1, ids: IDS, exps: exps }); }
      catch (err) { nlog("boot ERR: " + (err && err.stack ? err.stack : err)); }
    }
  }
  function tick() {
    pass++;
    try { resolve(); } catch (e) { nlog("tick ERR: " + e); }
    setTimeout(tick, pass < 400 ? 1500 : 3000);
  }
  try {
    Object.defineProperty(window, "__require", {
      configurable: true,
      get: function () { return cur; },
      set: function (v) { cur = v; if (typeof v === "function") setTimeout(tick, 600); }
    });
    nlog("require trap installed (cur=" + (typeof cur) + ")");
  } catch (e) { nlog("defineProperty ERR: " + e); }
  setTimeout(tick, 1500);
})();
'''


def extract_patch_js(header: Path) -> str:
    text = header.read_text(encoding="utf-8")
    m = re.search(r'R"YSXZJS\((.*?)\)YSXZJS"', text, re.S)
    if not m:
        raise SystemExit("ERROR: %s 里找不到 R\"YSXZJS(...)\" 补丁源" % header)
    return m.group(1)


def build(src: str, ids: list) -> str:
    for m in (SEC7_HEAD, SCAN_DEF, TARGETS_HEAD, TARGETS_TAIL, TAIL_HEAD, N_DECL, SCAN_BLOCK):
        n = src.count(m)
        if n != 1:
            raise SystemExit("ERROR: 锚点不唯一/缺失: %r (%d 次)" % (m[:48], n))

    i0 = src.index(SEC7_HEAD)
    i1 = src.index(SCAN_DEF)
    src = src[:i0] + NEW_SEC7_HEAD + "\n" + src[i1:]

    s0 = src.index(SCAN_BLOCK)
    src = src[:s0] + NEW_SCAN + src[s0 + len(SCAN_BLOCK):]

    j0 = src.index(TARGETS_HEAD)
    j1 = src.index(TARGETS_TAIL) + len(TARGETS_TAIL)
    src = src[:j0] + NEW_TARGETS.rstrip("\n") + src[j1:]

    k0 = src.index(TAIL_HEAD)
    src = src[:k0] + DRIVER.replace("__IDS__", json.dumps(ids))

    src = src.replace(N_DECL, NEW_N_DECL, 1)
    return src


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--out", default=str(OUT))
    ap.add_argument("--ids", default=str(IDS))
    ap.add_argument("--patch-js", default=None, help="直接给补丁 JS 文本（默认从 YsxzPatchJs.h 抽）")
    ap.add_argument("--quiet", action="store_true")
    a = ap.parse_args()

    src = Path(a.patch_js).read_text(encoding="utf-8") if a.patch_js else extract_patch_js(HEADER)
    ids = json.loads(Path(a.ids).read_text(encoding="utf-8"))["ids"]
    out = Path(a.out)
    out.parent.mkdir(parents=True, exist_ok=True)
    text = build(src, ids)
    out.write_text(text, encoding="utf-8")

    probes = ("require trap installed", "native proof pid=", "bootstrap stand-down",
              "srcOfExports", "MOD_boot({", "boot(exports)")
    if not a.quiet:
        print("ids=%d" % len(ids))
        print("out=%s size=%d" % (out, out.stat().st_size))
        for p in probes:
            print("  %-30s %s" % (p, "OK" if p in text else "MISSING"))
    for p in probes:
        if p not in text:
            print("ERROR: 产物缺少标记 %r" % p, file=sys.stderr)
            return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
