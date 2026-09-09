#!/usr/bin/env python3.11
# 解密 typeDefs 12156-12625 的名字与命名空间（OUT 文件 names 区已解密, 直接读）
import struct

P = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_v29_dec.dat"
d = open(P, "rb").read()
u32 = lambda o: struct.unpack_from("<I", d, o)[0]
i32 = lambda o: struct.unpack_from("<i", d, o)[0]

TD = 0x8AFD34
NB, NAMES_SZ = 0xA23B8, 0x2227DC - 0xA23B8

def gstr(off):
    if not (0 <= off < NAMES_SZ):
        return "<OOB:0x%x>" % off
    e = d.index(b"\x00", NB + off)
    return d[NB + off:e].decode("utf-8", "replace")

print("names[9] =", repr(gstr(9)))
print("== 尾部类型名 12156-12625 ==")
rows = []
for i in range(12156, 12626):
    b = TD + i * 88
    nm = gstr(i32(b))
    ns = gstr(i32(b + 4))
    rows.append((i, ns, nm))
# 打印前 60 + 按命名空间统计
from collections import Counter
c = Counter(ns for _, ns, _ in rows)
print("命名空间分布:", c.most_common(12))
for i, ns, nm in rows[:80]:
    print(i, ns + "." + nm if ns else nm)
