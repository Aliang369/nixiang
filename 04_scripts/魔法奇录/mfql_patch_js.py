#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""魔法奇录 index.js 静态补丁（免广告）v4

用法:
    python 04_scripts/魔法奇录/mfql_patch_js.py --src <index.js> --out <index.patched.js>

判据（不满足即非零退出，不凑合）:
    1. marker 在输出中恰好 1 次
    2. 每条 anchor 在**原始**脚本里恰好 1 次（全文子串计数，不是行数）
    3. 输出比输入大（确认真插入了）

v4 变更（2026-11-26）：桥调用改成「四种格式全试 + 每次结果落日志」，并加启动自检。
    v3 的 outer_sdk 补丁只在命中时打日志，gate=false / 桥抛异常时完全静默 ⇒
    无法区分「路径没走到」与「判据 false」。v4 三个入口一律无条件打印 gate。
"""
import argparse
import hashlib
import sys

MARKER = "starry_mfql_freead_v1"

# 注入到脚本最前面：
#   1) __starryMfqlOn(i) 四通道：JS 缓存 → 斜杠 getRet → 点分 getRet → 斜杠/点分 getFreeAd
#      任一返回 **严格 true** 才算开；每次尝试结果全部进 window.__mfqlBridgeLog
#   2) 启动 2.5s 后自检一次，把 gate / jsb 可见性 / 桥异常一次性打进 logcat
PREFIX = """window.__mfqlBridgeLog=window.__mfqlBridgeLog||[];
window.__starryMfqlOn=function(i){
try{if(window.__starryMfqlCache&&typeof window.__starryMfqlCache[i]!=="undefined"){window.__mfqlBridgeLog.push("cache["+i+"]="+window.__starryMfqlCache[i]);return !!window.__starryMfqlCache[i];}}catch(e){}
var L=window.__mfqlBridgeLog,r=false,J=(typeof jsb!=="undefined")?jsb:null;
if(!J||!J.reflection||typeof J.reflection.callStaticMethod!=="function"){L.push("no-jsb-reflection");if(L.length>60)L.splice(0,L.length-60);return false;}
var f=J.reflection.callStaticMethod,self=J.reflection;
var A=[["com/starry/magic/cocos/MfqlHooks","getRet","(I)Z",[i]],["com.starry.magic.cocos.MfqlHooks","getRet","(I)Z",[i]],["com/starry/magic/cocos/MfqlHooks","getFreeAd","()Z",[]],["com.starry.magic.cocos.MfqlHooks","getFreeAd","()Z",[]]];
for(var k=0;k<A.length;k++){var v;try{v=f.apply(self,[A[k][0],A[k][1],A[k][2]].concat(A[k][3]));}catch(e2){L.push(A[k][1]+"@"+k+"EX:"+e2);continue;}L.push(A[k][1]+"@"+k+"="+v);if(v===true){r=true;break;}}
if(L.length>60)L.splice(0,L.length-60);
return r;
};
try{console.log("[mfql] patch loaded v4");}catch(e){}
try{setTimeout(function(){try{var __p=window.__starryMfqlOn(1);console.log("[mfql] boot gate="+__p+" jsb="+(typeof jsb)+" log="+window.__mfqlBridgeLog.slice(-6).join(" ; "));}catch(e){console.log("[mfql] boot probe EX:"+e);}},2500);}catch(e){}
/*__MARKER__*/
""".replace("__MARKER__", MARKER)

PATCHES = [
    {
        "name": "free_ad",
        "anchor": "this._playAD.show(t, n, o, i);",
        "insert": (
            "try{var __mfqlOn=window.__starryMfqlOn?window.__starryMfqlOn(1):false;"
            "console.log(\"[mfql] playAD gate=\"+__mfqlOn);"
            "if(__mfqlOn){var __mfqlPd=this._playAD;"
            "setTimeout(function(){try{__mfqlPd.SDKADRewardCanGet();__mfqlPd.SDKADClosed();}catch(__mfqlE1){}},150);"
            "return;}}catch(__mfqlE0){}"
        ),
    },
    {
        "name": "outer_sdk",
        "anchor": 'e.prototype.showAd = function(t, e, n, o, i) {\nvoid 0 === i && (i = !1);\n',
        "insert": 'try{var __g=window.__starryMfqlOn?window.__starryMfqlOn(1):false;console.log("[mfql] outer gate="+__g+" log="+(window.__mfqlBridgeLog||[]).slice(-4).join(" ; "));if(__g&&e&&n){e.call(n);return;}}catch(__mfqlO){console.log("[mfql] outer EX:"+__mfqlO);}',
    },
    {
        "name": "builtin_ad",
        "optional": True,   # 老版本 bundle 无内置广告类（insideAd 是后加的），缺失即跳过
        "anchor": 't.prototype.showAd = function(t, e, n) {\nvar i = c.default.getTime();',
        "insert": 'try{var __gb=window.__starryMfqlOn?window.__starryMfqlOn(1):false;console.log("[mfql] builtin gate="+__gb);if(__gb&&t&&n){t.call(n);return;}}catch(__mfqlB){}',
    },
]


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--src", required=True)
    ap.add_argument("--out", required=True)
    args = ap.parse_args()

    with open(args.src, "r", encoding="utf-8") as f:
        text = f.read()
    if MARKER in text:
        print("[mfql] 输入已打过补丁（marker 已存在），拒绝重复打")
        return 2

    out = text
    for p in PATCHES:
        n = text.count(p["anchor"])
        if n == 0 and p.get("optional"):
            print("[mfql] skip %-10s anchor x0（该版本无此路径）" % p["name"])
            continue
        if n != 1:
            print("[mfql] FAIL anchor %r 命中 %d 次（要求恰好 1）" % (p["anchor"], n))
            return 3
        out = out.replace(p["anchor"], p["anchor"] + p["insert"], 1)
        print("[mfql] ok %-10s anchor x1 -> insert %d B" % (p["name"], len(p["insert"])))

    out = PREFIX + out

    if out.count(MARKER) != 1:
        print("[mfql] FAIL marker 计数 != 1")
        return 4
    if len(out) <= len(text):
        print("[mfql] FAIL 输出未变长")
        return 5

    with open(args.out, "w", encoding="utf-8", newline="") as f:
        f.write(out)

    def sha(b: bytes) -> str:
        return hashlib.sha256(b).hexdigest()

    src_b = text.encode("utf-8")
    out_b = out.encode("utf-8")
    print("[mfql] src  %10d B sha256 %s" % (len(src_b), sha(src_b)))
    print("[mfql] out  %10d B sha256 %s" % (len(out_b), sha(out_b)))
    print("[mfql] PASS")
    return 0


if __name__ == "__main__":
    sys.exit(main())
