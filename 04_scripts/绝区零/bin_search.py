#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""bin_search.py — 在本地二进制文件里找字节模式（hex 或 ascii），打印全部/前 N 个偏移。

用法:
  python bin_search.py <file> --hex af1bb1fa [--max 5]
  python bin_search.py <file> --ascii MHY [--max 5] [--ctx 32]
"""
import argparse

ap = argparse.ArgumentParser()
ap.add_argument("file")
ap.add_argument("--hex")
ap.add_argument("--ascii", dest="asc")
ap.add_argument("--max", type=int, default=5)
ap.add_argument("--ctx", type=int, default=0, help="命中处前后各打印多少字节 hex")
a = ap.parse_args()

pat = bytes.fromhex(a.hex) if a.hex else a.asc.encode()
d = open(a.file, "rb").read()
print("file=%s size=%d pattern=%s" % (a.file, len(d), pat.hex()))
pos = 0
n = 0
while n < a.max:
    i = d.find(pat, pos)
    if i < 0:
        break
    if a.ctx:
        s = max(0, i - a.ctx)
        print("0x%x  %s" % (i, d[s:i + a.ctx + len(pat)].hex()))
    else:
        print("0x%x" % i)
    pos = i + 1
    n += 1
print("hits_shown=%d" % n)
