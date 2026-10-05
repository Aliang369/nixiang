#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""rva_scan.py — 在二进制里找落在目标 RVA 区间内的对齐 32/64 位数值（找 RVA 表用）。

用法:
  python rva_scan.py <file> --lo 0x4000000 --hi 0x1b100000 [--bits 32,64] [--max 60]
输出: 文件偏移 + 数值（按文件顺序）
"""
import argparse
import struct

ap = argparse.ArgumentParser()
ap.add_argument("file")
ap.add_argument("--lo", required=True)
ap.add_argument("--hi", required=True)
ap.add_argument("--bits", default="32")
ap.add_argument("--max", type=int, default=60)
a = ap.parse_args()

lo, hi = int(a.lo, 0), int(a.hi, 0)
d = open(a.file, "rb").read()
bits = [int(x) for x in a.bits.split(",")]
print("file=%s size=%d range=[0x%x,0x%x) bits=%s" % (a.file, len(d), lo, hi, bits))
n = 0
for bit in bits:
    step = bit // 8
    fmt = "<I" if bit == 32 else "<Q"
    print("-- %d-bit --" % bit)
    for off in range(0, len(d) - step + 1, step):
        v = struct.unpack_from(fmt, d, off)[0]
        if lo <= v < hi:
            print("  off=0x%-10x val=0x%x" % (off, v))
            n += 1
            if n >= a.max:
                print("hits_shown=%d (截断)" % n)
                raise SystemExit
print("hits_shown=%d" % n)
