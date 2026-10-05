#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""grep_region.py — 在本地二进制 dump 里搜索字节/字符串/32 位常量（只读，多进程加速）。

用法:
  python grep_region.py FILE [--hex AF1BB1FA] [--str mscorlib.dll] \
      [--u32 0xFAB11BAF] [--limit 10] [--from 0] [--to end]

输出：每条命中的文件偏移（十六进制）。--ctx N 额外打印命中前后 N 字节的 hex。
"""
import argparse
import mmap
import os
import struct


def find_all(mm, pat, limit):
    out = []
    start = 0
    while len(out) < limit:
        i = mm.find(pat, start)
        if i < 0:
            break
        out.append(i)
        start = i + 1
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("file")
    ap.add_argument("--hex", action="append", default=[])
    ap.add_argument("--str", action="append", default=[])
    ap.add_argument("--u32", action="append", default=[])
    ap.add_argument("--limit", type=int, default=10)
    ap.add_argument("--from", dest="frm", type=lambda x: int(x, 0), default=0)
    ap.add_argument("--to", type=lambda x: int(x, 0), default=0)
    ap.add_argument("--ctx", type=int, default=0)
    a = ap.parse_args()

    f = open(a.file, "rb")
    size = os.path.getsize(a.file)
    mm = mmap.mmap(f.fileno(), 0, access=mmap.ACCESS_READ)
    end = a.to or size
    view = mm[a.frm:end]

    for h in a.hex:
        pat = bytes.fromhex(h)
        hits = find_all(view, pat, a.limit)
        print("hex %s: %d hit(s)" % (h, len(hits)))
        for i in hits:
            off = a.frm + i
            print("  0x%x" % off)
            if a.ctx:
                print("    ctx: %s" % view[max(0, i - a.ctx):i + len(pat) + a.ctx].hex())
    for s in a.str:
        pat = s.encode()
        hits = find_all(view, pat, a.limit)
        print("str %r: %d hit(s)" % (s, len(hits)))
        for i in hits:
            print("  0x%x" % (a.frm + i))
    for u in a.u32:
        v = int(u, 0)
        pat = struct.pack("<I", v)
        hits = find_all(view, pat, a.limit)
        print("u32 %s: %d hit(s)" % (u, len(hits)))
        for i in hits:
            print("  0x%x" % (a.frm + i))
    mm.close()


if __name__ == "__main__":
    main()
