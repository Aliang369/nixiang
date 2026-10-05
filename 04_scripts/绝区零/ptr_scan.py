#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""ptr_scan.py — 在一段内存 dump 里找指向目标地址范围的 8 字节指针。

用法:
  python ptr_scan.py <dump.bin> --base 0x5f80001000 --lo 0x5f814c0000 --hi 0x5f81500000
                       [--max 20] [--align8]
输出: 命中处（dump 内偏移 + 绝对地址）+ 指针值
"""
import argparse
import struct

ap = argparse.ArgumentParser()
ap.add_argument("dump")
ap.add_argument("--base", required=True)
ap.add_argument("--lo", required=True)
ap.add_argument("--hi", required=True)
ap.add_argument("--max", type=int, default=20)
ap.add_argument("--align8", action="store_true")
a = ap.parse_args()

base = int(a.base, 0)
lo, hi = int(a.lo, 0), int(a.hi, 0)
d = open(a.dump, "rb").read()
print("dump=%s size=%d base=0x%x target=[0x%x,0x%x)" % (a.dump, len(d), base, lo, hi))
n = 0
step = 8 if a.align8 else 1
for off in range(0, len(d) - 7, step):
    v = struct.unpack_from("<Q", d, off)[0]
    if lo <= v < hi:
        print("  off=0x%-10x addr=0x%-14x -> 0x%x" % (off, base + off, v))
        n += 1
        if n >= a.max:
            break
print("hits_shown=%d" % n)
