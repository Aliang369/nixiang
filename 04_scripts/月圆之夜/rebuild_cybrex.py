#!/usr/bin/env python3.11
# libcybrex 内存镜像(VA 布局) → 回写到文件 ELF 布局, 产出可 IDA 的解包 so
# 用法: python3.11 rebuild_cybrex.py
import struct, sys

SRC = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/cybrex_mem.bin"
FILE = "03_decompiled/月圆之夜/apktool_1.6.30.1/lib/arm64-v8a/libcybrex.so"
OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/libcybrex_unpacked.so"

d = open(SRC, "rb").read()
f = bytearray(open(FILE, "rb").read())
print(f"镜像 {len(d)} B, 文件 {len(f)} B")

e_phoff = struct.unpack_from("<Q", f, 0x20)[0]
e_phnum = struct.unpack_from("<H", f, 0x38)[0]
patched = 0
holes = []
for i in range(e_phnum):
    off = e_phoff + i * 56
    p_type, p_flags = struct.unpack_from("<II", f, off)
    p_offset, p_vaddr, _pa, p_filesz = struct.unpack_from("<QQQQ", f, off + 8)
    if p_type != 1:
        continue
    for j in range(p_filesz):
        va = p_vaddr + j
        fo = p_offset + j
        if va < len(d):
            if d[va] != f[fo]:
                f[fo] = d[va]
                patched += 1
        else:
            holes.append(va)
print(f"回写字节数: {patched}")
if holes:
    print(f"镜像未覆盖的尾部地址: {len(holes)} (示例 {hex(holes[0])}..{hex(holes[-1])})")
open(OUT, "wb").write(f)
print(f"→ {OUT}")
# 校验: 解包后两个原高熵区的熵
import math
from collections import Counter
def ent(b):
    c = Counter(b); n = len(b)
    return -sum((v/n)*math.log2(v/n) for v in c.values()) if n else 0
for name, a, b in [("0x6e000-0x94000", 0x6e000, 0x94000), ("0x10f000-0x2d0000", 0x10f000, 0x2d0000)]:
    print(f"解包文件 {name}: ent={ent(bytes(f[a:b])):.2f}")
