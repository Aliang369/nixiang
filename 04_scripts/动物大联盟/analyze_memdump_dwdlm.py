#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""dwdlm — memdump 本机分析器（配合 dump_mem_dd_dwdlm.py 的 raw/ + manifest.jsonl）

子命令：
  cjk    提取段内所有 CJK 串（UTF-8 与 UTF-16LE 各扫一遍），按出现次数排序
  kw     按给定关键词扫描并输出上下文（UTF-8 + UTF-16LE）
  code   提取像 JS/URL 的 ASCII 片段（含 jsb./cc./manifest/http/load 等）
"""
import argparse
import collections
import json
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", ".."))
OUT = os.path.join(ROOT, "09_test", "动物大联盟", "memscan_20261106")

CJK_UTF8 = re.compile(b"(?:[\xe4-\xe9][\x80-\xbf][\x80-\xbf]){2,}")
CJK_U16 = re.compile(b"(?:[\x00-\xff][\x30-\x9f]){2,}")


def load_manifest():
    p = os.path.join(OUT, "manifest.jsonl")
    return [json.loads(l) for l in open(p, encoding="utf-8")]


def seg_bytes(m):
    with open(os.path.join(OUT, "raw", m["file"]), "rb") as fh:
        fh.seek(m["off"])
        return fh.read(m["size"])


def cmd_cjk(args):
    cnt8, cnt16 = collections.Counter(), collections.Counter()
    for m in load_manifest():
        d = seg_bytes(m)
        for b in CJK_UTF8.findall(d):
            try:
                cnt8[b.decode("utf-8")] += 1
            except Exception:
                pass
        for b in CJK_U16.findall(d):
            try:
                cnt16[b.decode("utf-16-le")] += 1
            except Exception:
                pass
    print("### UTF-8 CJK 串 %d 种 / %d 次" % (len(cnt8), sum(cnt8.values())))
    for s, c in cnt8.most_common(args.top):
        print("  %5d  %s" % (c, s[:80]))
    print("\n### UTF-16LE CJK 串 %d 种 / %d 次" % (len(cnt16), sum(cnt16.values())))
    for s, c in cnt16.most_common(args.top):
        print("  %5d  %s" % (c, s[:80]))
    return 0


def cmd_kw(args):
    pats = []
    for kw in args.kw:
        pats.append((kw + "|u8", kw.encode("utf-8")))
        pats.append((kw + "|u16", kw.encode("utf-16-le")))
    n = 0
    for m in load_manifest():
        d = seg_bytes(m)
        for tag, pat in pats:
            st = d.find(pat)
            while st >= 0:
                n += 1
                cs, ce = max(0, st - args.ctx), min(len(d), st + len(pat) + args.ctx)
                sub = d[cs:ce]
                addr = m["addr"] + st
                print("=" * 78)
                print("[%s] addr=0x%x seg=0x%x %s" % (tag, addr, m["addr"], m["path"][:44]))
                # 双视图：UTF-8 视图 与 UTF-16LE 视图
                v8 = re.sub(rb"[^\x20-\x7e\n]", b".", sub).decode("latin1")
                print("-- ascii/utf8 --")
                for i in range(0, len(v8), 400):
                    print("   " + v8[i:i + 400])
                print("-- utf16le --")
                s16 = sub.decode("utf-16-le", errors="replace")
                s16 = "".join(ch if (ch.isprintable() or ch == "\n") else "." for ch in s16)
                for i in range(0, len(s16), 400):
                    print("   " + s16[i:i + 400])
                st = d.find(pat, st + 1)
    print("\n总命中 %d" % n)
    return 0


def cmd_code(args):
    pats = [b"jsb.", b"cc.assetManager", b"cc.asset", b"hotUpdate", b"manifest", b"XMLHttp",
            b"xhr", b"download", b"Download", b"checkUpdate", b"cacheDir", b"storagePath",
            b"writablePath", b"loadScript", b"postMessage", b"native.reflection", b"jsb.reflection"]
    seen = collections.Counter()
    for m in load_manifest():
        d = seg_bytes(m)
        for p in pats:
            if p in d:
                seen[p.decode()] += d.count(p)
    for k, v in seen.most_common():
        print("%-24s %d" % (k, v))
    return 0


def main():
    ap = argparse.ArgumentParser()
    sub = ap.add_subparsers(dest="cmd", required=True)
    a1 = sub.add_parser("cjk")
    a1.add_argument("--top", type=int, default=80)
    a2 = sub.add_parser("kw")
    a2.add_argument("kw", nargs="+")
    a2.add_argument("--ctx", type=int, default=1200)
    sub.add_parser("code")
    args = ap.parse_args()
    return {"cjk": cmd_cjk, "kw": cmd_kw, "code": cmd_code}[args.cmd](args)


if __name__ == "__main__":
    sys.exit(main())
