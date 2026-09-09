#!/usr/bin/env python3.11
# gap1 精确扫描: PDV(2079) FDV(14592) blob(240860B) FMS(4325) 的真实排列
import struct
import numpy as np

P = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_runtime.dat"
d = open(P, "rb").read()
G1 = 0x278A30 + 98644 * 32          # 0x57B4B0
G1E = 0x5F39BC
SIZE = G1E - G1                     # 492812
print("gap1 0x%x..0x%x (%d B)" % (G1, G1E, SIZE))

A = np.frombuffer(d, dtype="<i4", offset=G1, count=SIZE // 4)
n = len(A)
print("slots:", n)

# 三种 12B 对齐下的三元组有效性
def runs_for(cond_fn, expect_cnt, name):
    """cond_fn(w0,w1,w2)->bool, 找连续 k 段"""
    for align in (0, 1, 2):
        idx = np.arange(align, n - 3, 3)
        w0, w1, w2 = A[idx], A[idx + 1], A[idx + 2]
        ok = cond_fn(w0, w1, w2)
        dv = np.diff(np.concatenate(([0], ok.view(np.int8), [0])))
        starts = np.where(dv == 1)[0]
        ends = np.where(dv == -1)[0]
        best = []
        for s, e in zip(starts, ends):
            if e - s >= expect_cnt:
                off = G1 + (align + s * 3) * 4
                best.append((off, e - s))
        if best:
            print("%s align=%d 候选: %s" % (name, align,
                  ["0x%x(len%d)" % (o, L) for o, L in best[:6]]))

# FDV: fieldIndex<55037, typeIndex<49223, -1<=dataIndex<240860
runs_for(lambda a, b, c: (a >= 0) & (a < 55037) & (b >= 0) & (b < 49223) & (c >= -1) & (c < 240860),
         14592, "FDV ")
# PDV: parameterIndex<113689, typeIndex<49223, dataIndex<240860
runs_for(lambda a, b, c: (a >= 0) & (a < 113689) & (b >= 0) & (b < 49223) & (c >= -1) & (c < 240860),
         2079, "PDV ")
# FMS: typeIndex<49223, fieldIndex<55037, 1<=size<256
runs_for(lambda a, b, c: (a >= 0) & (a < 49223) & (b >= 0) & (b < 55037) & (c >= 1) & (c < 256),
         4325, "FMS ")

# 双重验证：选定偏移后逐条精确复核 + 与 typeDef/field 定义交叉
def verify(off, cnt, kind):
    bad = 0
    for k in range(cnt):
        w0, w1, w2 = struct.unpack_from("<iii", d, off + k * 12)
        if kind == "FDV" and not (0 <= w0 < 55037 and 0 <= w1 < 49223 and -1 <= w2 < 240860):
            bad += 1
        if kind == "PDV" and not (0 <= w0 < 113689 and 0 <= w1 < 49223 and -1 <= w2 < 240860):
            bad += 1
        if kind == "FMS" and not (0 <= w0 < 49223 and 0 <= w1 < 55037 and 1 <= w2 < 256):
            bad += 1
    return bad
