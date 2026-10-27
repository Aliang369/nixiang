#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""zmws_splice_syntax_check.py — 造梦无双「注入串 → 明文脚本」离线语法关（2026-10-27 黑屏事故固化）

为什么需要它：
  造梦无双玩法是往 18 MB 的 `assets/main/index.jsc`（XXTEA 加密的 Cocos 脚本，运行时解密）里
  用字符串插入的方式打补丁。注入串只要多一个 `}`，整段 eval 直接抛 SyntaxError ⇒ **引擎不渲染 = 黑屏**，
  而 splice 日志全是「spliced ... 成功」。所以必须在装机前用真产物把「插入后的完整脚本」过一遍 node --check。

做法（不依赖会话记忆、不复制 JS 正文）：
  1) 从**已构建的 so** 里按注释标签（`/*starry_zmws_*​/`）把注入串原样抠出来（`%s` 换成真实包名）；
  2) 用与 `Games/ZmwsMod.cpp` 相同的锚点、相同顺序插进**探针 dump 的明文 index.js**；
  3) `node --check` 全量语法关；失败就把出错行打印出来。

用法：
  python3 04_scripts/造梦无双/zmws_splice_syntax_check.py \\
      --so mod菜单/starry-mod-menu/libs_zmws/arm64-v8a/libstarry_magic.so \\
      --index 06_dumped/造梦无双/probe_1.34/probe_04_index.jsc.js [--out /tmp/zmws_patched_index.js]

退出码：0 语法通过；1 语法失败；2 用法/输入缺失。
"""
import argparse
import pathlib
import subprocess
import sys

PKG = "com.xm4399.zmws.m4399"

# 与 Games/ZmwsMod.cpp SpliceCombatJs 的注入顺序、锚点一一对应（改 C++ 必须同步改这里）
TAGS = [
    "/*starry_zmws_uls*/",
    "/*starry_zmws_fabao*/",
    "/*starry_zmws_sk*/",
    "/*starry_zmws_inv*/",
    "/*starry_zmws_nocap*/",
    "/*starry_zmws_timepad*/",
    "/*starry_zmws_killtime*/",
]
ANCHORS = [
    'if (this.mPlayer.isDead() || this.mPlayer.mBuffMgr && '
    '!this.mPlayer.mBuffMgr.isCanReleaseSkill()) return "1";',
    "var e = this.mPlayer.mProp.getMagicCurEneryCount() - 1;",
    "s.getAtk = function(e, i) {",
    "if (0 == e || this.isDead()) return 0;",
    "var u = p.mData.uid;",
    "gameSettle: function(e, t, i, a) {",
    "e.killTime = this.mFightScene.mRecordKillTime || 0;",
]
CATCH_TAIL_MARKERS = ("catch", "finally")


def scan_end(s: str, i: int) -> int:
    """从 i（注释标签位置）扫一段 JS，含尾部 try{...}catch(x){...}，返回语句结束下标。"""
    par = cur = 0
    in_str = esc = line = blk = False
    j, n, started = i, len(s), False
    while j < n:
        c = s[j]
        nx = s[j + 1] if j + 1 < n else ""
        if line:
            if c == "\n":
                line = False
            j += 1
            continue
        if blk:
            if c == "*" and nx == "/":
                blk = False
                j += 2
                continue
            j += 1
            continue
        if in_str:
            if esc:
                esc = False
            elif c == "\\":
                esc = True
            elif c == '"':
                in_str = False
            j += 1
            continue
        if c == "/" and nx == "/":
            line = True
            j += 2
            continue
        if c == "/" and nx == "*":
            blk = True
            j += 2
            continue
        if c == '"':
            in_str = True
            j += 1
            continue
        if c == "(":
            par += 1
            started = True
        elif c == ")":
            par -= 1
        elif c == "{":
            cur += 1
            started = True
        elif c == "}":
            cur -= 1
            if started and par == 0 and cur == 0:
                k = j + 1
                while k < n and s[k] in " \t":
                    k += 1
                if any(s.startswith(m, k) for m in CATCH_TAIL_MARKERS):
                    j += 1
                    continue
                return j + 1
        j += 1
    return n


def braces_balanced(s: str) -> bool:
    par = cur = 0
    in_str = esc = line = blk = False
    j, n = 0, len(s)
    while j < n:
        c = s[j]
        nx = s[j + 1] if j + 1 < n else ""
        if line:
            if c == "\n":
                line = False
            j += 1
            continue
        if blk:
            if c == "*" and nx == "/":
                blk = False
                j += 2
                continue
            j += 1
            continue
        if in_str:
            if esc:
                esc = False
            elif c == "\\":
                esc = True
            elif c == '"':
                in_str = False
            j += 1
            continue
        if c == "/" and nx == "/":
            line = True
            j += 2
            continue
        if c == "/" and nx == "*":
            blk = True
            j += 2
            continue
        if c == '"':
            in_str = True
            j += 1
            continue
        if c == "(":
            par += 1
        elif c == ")":
            par -= 1
        elif c == "{":
            cur += 1
        elif c == "}":
            cur -= 1
        if par < 0 or cur < 0:
            return False
        j += 1
    return par == 0 and cur == 0 and not in_str and not blk


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--so", required=True)
    ap.add_argument("--index", required=True, help="探针 dump 的明文 index.js")
    ap.add_argument("--out", default="/tmp/zmws_patched_index.js")
    a = ap.parse_args()

    so_p, idx_p = pathlib.Path(a.so), pathlib.Path(a.index)
    for p in (so_p, idx_p):
        if not p.is_file():
            print("ERROR: 缺文件 %s" % p, file=sys.stderr)
            return 2

    so_txt = so_p.read_bytes().decode("utf-8", errors="replace")
    hooks = []
    for t in TAGS:
        i = so_txt.find(t)
        if i < 0:
            print("ERROR: so 内找不到注入串标签 %s（so 与源码不匹配？）" % t, file=sys.stderr)
            return 2
        h = so_txt[i:scan_end(so_txt, i)].replace("%s", PKG)
        if not braces_balanced(h):
            print("ERROR: 注入串括号不配平：%s" % t, file=sys.stderr)
            return 1
        hooks.append(h)
        print("  hook %s len=%d ok" % (t, len(h)))

    out = idx_p.read_text(encoding="utf-8", errors="replace")
    for anchor, hook in zip(ANCHORS, hooks):
        if out.count(anchor) < 1:
            print("ERROR: 明文脚本里找不到锚点：%r" % anchor[:60], file=sys.stderr)
            return 2
        out = out.replace(anchor, anchor + " " + hook, 1)

    k = "copyHandler.addCheat = function"
    i = out.find(k)
    if i >= 0:
        b = out.find("{", i)
        out = out[: b + 1] + " /*starry_zmws_nocheat*/ return;" + out[b + 1 :]

    pathlib.Path(a.out).write_text(out, encoding="utf-8")
    rc = subprocess.run(["node", "--check", a.out]).returncode
    if rc != 0:
        print("FAIL: node --check 没过（上面是出错行）——禁止装机", file=sys.stderr)
        return 1
    print("PASS: 插入后 %d B 明文脚本 node --check 通过（%s）" % (len(out), a.out))
    return 0


if __name__ == "__main__":
    sys.exit(main())
