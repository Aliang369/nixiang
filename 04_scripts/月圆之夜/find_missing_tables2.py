#!/usr/bin/env python3.11
# numpy 滑窗版：定位 v29 缺失表
import struct
import numpy as np

P = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_v29_dec.dat"
d = open(P, "rb").read()
FS = len(d)
TB, NTYPES = 0x8AFD34, 12626
NTYPEIDX = 49223
NFIELDS, NMETHODS, NPARAMS = 55037, 98644, 113689

def u16(o): return struct.unpack_from("<H", d, o)[0]
def u32(o): return struct.unpack_from("<I", d, o)[0]
def s32(o): return struct.unpack_from("<i", d, o)[0]

sum_if = sum_ifoff = sum_nest = sum_vt = 0
for i in range(NTYPES):
    b = TB + i * 88
    sum_if += u16(b + 76); sum_ifoff += u16(b + 78)
    sum_nest += u16(b + 72); sum_vt += u16(b + 74)
IB = 0x9BF164
sum_exp = sum(u32(IB + k*40 + 20) for k in range(102))
AB = 0x9C0154
sum_ref = sum(s32(AB + k*64 + 12) for k in range(102))
print("期望: if=%d ifoff=%d nest=%d vtbl=%d exp=%d refAsm=%d"
      % (sum_if, sum_ifoff, sum_nest, sum_vt, sum_exp, sum_ref))

REGIONS = {
    "gap1": (0x278A30 + 98644*32, 0x5F39BC),
    "gap2": (0x740AE8 + 55037*12, 0x8AFD34),
    "tail": (0x9C0154 + 102*64, FS),
}
for nm, (s, e) in REGIONS.items():
    print("  %s 0x%x..0x%x (%d B)" % (nm, s, e, e-s))

def runs_of_valid(vals_i32, lo, hi, minlen):
    """返回所有 valid 连续段 (start_slot, length)，valid=值在[lo,hi)"""
    valid = (vals_i32 >= lo) & (vals_i32 < hi)
    # 找连续 True 段
    dv = np.diff(np.concatenate(([0], valid.view(np.int8), [0])))
    starts = np.where(dv == 1)[0]
    ends = np.where(dv == -1)[0]
    out = []
    for s, e in zip(starts, ends):
        if e - s >= minlen:
            out.append((s, e - s))
    return out

def arr(off, n):
    return np.frombuffer(d, dtype="<i4", count=n, offset=off)

for rname, (rs, re_) in REGIONS.items():
    print("\n== %s ==" % rname)
    region = np.frombuffer(d, dtype="<i4", offset=rs, count=(re_ - rs)//4)
    def report(lo, hi, cnt, name, stride=4):
        if cnt == 0:
            print("  %-18s count=0 跳过" % name); return
        need = cnt * stride // 4
        if need > len(region):
            print("  %-18s 需%d槽>区间%d" % (name, need, len(region))); return
        rr = runs_of_valid(region, lo, hi, need)
        print("  %-18s cnt=%-7d 候选段: %s" % (name, cnt,
              ["slot%d=0x%x(len%d)" % (s, rs + s*4, L) for s, L in rr[:5]] or "无"))
    report(0, NTYPEIDX, sum_if, "interfaces")
    report(0, NTYPES, sum_nest, "nestedTypes")
    report(0, NTYPES, sum_exp, "exportedTypes")
    report(0, 102, sum_ref, "referencedAsm")
    # vtableMethods: 编码索引，值域宽，先看有符号非负且 < 3M
    report(0, 3000000, sum_vt, "vtableMethods")
    # interfaceOffsets 8B: 偶数槽∈types（offset 槽任意）
    if sum_ifoff:
        even = region[0::2]
        valid = (even >= 0) & (even < NTYPEIDX)
        need = sum_ifoff
        dv = np.diff(np.concatenate(([0], valid.view(np.int8), [0])))
        starts = np.where(dv == 1)[0]; ends = np.where(dv == -1)[0]
        rr = [(s, e-s) for s, e in zip(starts, ends) if e-s >= need]
        print("  %-18s cnt=%-7d 候选段: %s" % ("interfaceOffsets", sum_ifoff,
              ["0x%x(len%d)" % (rs + s*8, L) for s, L in rr[:5]] or "无"))
