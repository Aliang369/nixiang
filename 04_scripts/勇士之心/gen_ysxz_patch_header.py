#!/usr/bin/env python3.11
"""Generate YsxzPatchJs.h from the extracted binge injector.

The original memory string was captured at a source boundary and includes a
window.__require replacement that is not safe to replay as-is.  This generator
keeps only MOD_boot, exposes a pre-execution registry capture hook, and applies
the menu slider value to the movement multiplier.
"""
from __future__ import annotations

from pathlib import Path

ROOT = Path("/Users/aliang/Documents/nixiang")
SRC = ROOT / "10_docs/勇士之心/斌哥注入脚本_提取_999999_20260915.js"
OUT = (
    ROOT
    / "mod菜单/starry-mod-menu/app/src/main/jni/Games/YsxzPatchJs.h"
)

SHIM = r'''
/*starry_ysxz_v1*/
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
  Object.defineProperty(M, "feature1", { get: function () { return R(1); } });
  Object.defineProperty(M, "feature2", { get: function () { return R(2); } });
  Object.defineProperty(M, "feature3", { get: function () { return R(3); } });
  Object.defineProperty(M, "feature4", { get: function () { return R(4); } });
  Object.defineProperty(M, "feature5", { get: function () { return R(5); } });
  Object.defineProperty(M, "speed", { get: function () { return V(4); } });
  globalThis.__starry_nlog = J;
  globalThis.MOD = M;
})();
'''

TAIL = r'''
window.__starry_capture = function (reg) {
  try { MOD_boot(reg); } catch (e) {
    try { if (window.__starry_nlog) window.__starry_nlog("[MODMOD] boot ERR: " + e); } catch (ignored) {}
  }
  return reg;
};

try {
  if (window.__starry_nlog) window.__starry_nlog("[MODMOD] capture installed");
} catch (e) {}
'''


def main() -> int:
    body = SRC.read_text(encoding="utf-8").rstrip() + "\n"
    cut = body.rfind("window.__require = function t(e, i, n)")
    if cut < 0:
        raise SystemExit("window.__require capture marker not found")
    body = body[:cut].rstrip() + "\n" + TAIL

    old_log = "        try { if (!window.MOD_LOG) window.MOD_LOG = []; window.MOD_LOG.push(msg); } catch (e) {}"
    new_log = old_log + "\n        try { if (window.__starry_nlog) window.__starry_nlog(msg); } catch (e) {}"
    if body.count(old_log) != 1:
        raise SystemExit("expected exactly one log bridge line")
    body = body.replace(old_log, new_log)

    old_mul = "hy.hTimeScale = hy.hTimeScale * 3;"
    new_mul = (
        "var _sp = (MOD.speed > 0 ? MOD.speed : 1); "
        "hy.hTimeScale = hy.hTimeScale * _sp;"
    )
    old_push = "touched.push(hy);"
    new_push = "touched.push([ hy, _sp ]);"
    old_restore = "touched[j].hTimeScale = touched[j].hTimeScale / 3;"
    new_restore = (
        "try { touched[j][0].hTimeScale = "
        "touched[j][0].hTimeScale / (touched[j][1] || 1); } catch (e) {}"
    )
    for old, new in ((old_mul, new_mul), (old_push, new_push), (old_restore, new_restore)):
        if body.count(old) != 1:
            raise SystemExit(f"expected exactly one source pattern: {old}")
        body = body.replace(old, new)
    if body.count("if (MOD.feature4) {") != 1:
        raise SystemExit("expected exactly one feature4 condition")
    body = body.replace("if (MOD.feature4) {", "if (MOD.speed > 1) {")

    old_cost = r'''        if (fg && N.cd) {
            var oFg = K[fg];
            K[fg] = function (ent, sk) {
                var on = false;
                try { on = !!(MOD.feature3 && sk && isMine(ent)); } catch (e) {}
                if (!on) return oFg.apply(this, arguments);
                var da = N.Da ? sk[N.Da] : 0;
                var db = N.Db ? sk[N.Db] : 0;
                var cz = N.CZ ? sk[N.CZ] : 0;
                if (N.Da) sk[N.Da] = 0;
                if (N.Db) sk[N.Db] = 0;
                var r = oFg.apply(this, arguments);
                if (N.Da) sk[N.Da] = da;
                if (N.Db) sk[N.Db] = db;
                sk[N.cd] = 0;
                if (N.CY && cz > 0) sk[N.CY] = cz;
                return r;
            };
        }'''
    new_cost = r'''        if (fg && N.cd) {
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
        }'''
    if body.count(old_cost) != 1:
        raise SystemExit("expected exactly one skill-cost wrapper")
    body = body.replace(old_cost, new_cost)

    auto_impl = r'''
    /* ---------- 6b. 自动战斗特权: 仅在开关开启时放行 AUTO_BATTLE 判定 ---------- */
    /* 指纹只依赖混淆器不动的明文: 枚举成员 AUTO_BATTLE / BuffTableEnum。
     * 2026-09-21 热更(res 1.0.98.10)把 buff 管理器方法 $fc.buff.bVy.bVs 改名为
     * bXP, 旧指纹写死 "bVs(" 后 tryAuto 全部落空 -> 自动战斗失效。
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


'''
    boot_marker = "    /* ---------- 7. 启动: 先扫全部模块源码取名, 再懒包装目标模块 ---------- */"
    if body.count(boot_marker) != 1:
        raise SystemExit("expected exactly one boot marker")
    body = body.replace(boot_marker, auto_impl + boot_marker)
    scan_marker = "            try { trySystem(V); } catch (e) { log(\"trySystem ERR: \" + e); }"
    scan_new = scan_marker + "\n            try { tryAuto(V); } catch (e) { log(\"tryAuto ERR: \" + e); }"
    if body.count(scan_marker) != 1:
        raise SystemExit("expected exactly one scan marker")
    body = body.replace(scan_marker, scan_new)
    old_target_filter = "if (s.indexOf(\"maxApDefRate\") < 0 && s.indexOf(\"lockMinHP\") < 0 && s.indexOf(\"hitPauseTimeScale\") < 0) continue;"
    new_target_filter = "if (s.indexOf(\"maxApDefRate\") < 0 && s.indexOf(\"lockMinHP\") < 0 && s.indexOf(\"hitPauseTimeScale\") < 0 && s.indexOf(\"AUTO_BATTLE\") < 0) continue;"
    if body.count(old_target_filter) != 1:
        raise SystemExit("expected exactly one target filter")
    body = body.replace(old_target_filter, new_target_filter)
    old_report = "                \" cost=\" + N.Fg + \" death=\" + N.GJ + \" movesys=\" + (N.sysDone ? \"yes\" : \"NO\"));"
    new_report = "                \" cost=\" + N.Fg + \" death=\" + N.GJ + \" movesys=\" + (N.sysDone ? \"yes\" : \"NO\") + \" auto=\" + (N.autoDone ? \"yes\" : \"NO\"));"
    if body.count(old_report) != 1:
        raise SystemExit("expected exactly one report line")
    body = body.replace(old_report, new_report)
    old_mod_report = "                (\"f1=\" + MOD.feature1 + \" f2=\" + MOD.feature2 + \" f3=\" + MOD.feature3 + \" f4=\" + MOD.feature4)));"
    new_mod_report = "                (\"f1=\" + MOD.feature1 + \" f2=\" + MOD.feature2 + \" f3=\" + MOD.feature3 + \" f4=\" + MOD.feature4 + \" f5=\" + MOD.feature5)));"
    if body.count(old_mod_report) != 1:
        raise SystemExit("expected exactly one MOD report line")
    body = body.replace(old_mod_report, new_mod_report)


    text = SHIM + "\n" + body
    if ')YSXZJS"' in text:
        raise SystemExit("raw string delimiter collision")
    header = (
        "/* Auto-generated from 10_docs/勇士之心/斌哥注入脚本_提取_999999_20260915.js. */\n"
        "#pragma once\n\n"
        "static const char* kYsxzPatchJs = R\"YSXZJS(\n"
        + text
        + "\n)YSXZJS\";\n"
    )
    OUT.write_text(header, encoding="utf-8")
    print(f"wrote {OUT} bytes={len(header)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
