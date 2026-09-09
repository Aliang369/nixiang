#!/usr/bin/env python3.11
# 用计数恒等式+值域签名在未映射区间定位 v29 缺失表
import struct

P = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_v29_dec.dat"
d = open(P, "rb").read()
FS = len(d)
TB, NTYPES = 0x8AFD34, 12626
NAMES_SZ = 0x2227DC - 0xA23B8
NTYPEIDX = 49223      # types 数组条目数（RELA 验证过）
NFIELDS, NMETHODS, NPARAMS = 55037, 98644, 113689

def u16(o): return struct.unpack_from("<H", d, o)[0]
def u32(o): return struct.unpack_from("<I", d, o)[0]
def s32(o): return struct.unpack_from("<i", d, o)[0]

# 1) 从 typeDefs 汇总期望计数
sum_if = sum_ifoff = sum_nest = sum_vt = 0
for i in range(NTYPES):
    b = TB + i * 88
    sum_if += u16(b + 76)
    sum_ifoff += u16(b + 78)
    sum_nest += u16(b + 72)
    sum_vt += u16(b + 74)
print("期望: interfaces=%d (%dB)  interfaceOffsets=%d (%dB)  nestedTypes=%d (%dB)  vtableMethods=%d (%dB)"
      % (sum_if, sum_if*4, sum_ifoff, sum_ifoff*8, sum_nest, sum_nest*4, sum_vt, sum_vt*4))

# imageDef exportedType 汇总
IB = 0x9BF164
sum_exp = sum(u32(IB + k*40 + 20) for k in range(102))
print("期望: exportedTypeDefinitions=%d (%dB)" % (sum_exp, sum_exp*4))

# assemblyDef referencedAssemblies
AB = 0x9C0154
sum_ref = sum(s32(AB + k*64 + 12) for k in range(102))
print("期望: referencedAssemblies=%d (%dB)" % (sum_ref, sum_ref*4))

# 2) 三个未映射区间
REGIONS = {
    "gap1(方法→参数)": (0x278A30 + 98644*32, 0x5F39BC),
    "gap2(字段→typeDef)": (0x740AE8 + 55037*12, 0x8AFD34),
    "tail(程序集→EOF)": (0x9C0154 + 102*64, FS),
}
for nm, (s, e) in REGIONS.items():
    print("  %s 0x%x..0x%x (%d B)" % (nm, s, e, e - s))

def scan4(region_s, region_e, lo, hi, count, name, extra=None):
    """扫描 4B 数组：长度=count，全部值在 [lo,hi)"""
    hits = []
    need = count * 4
    if count == 0 or need == 0:
        return hits
    for off in range(region_s, region_e - need + 1, 4):
        ok = True
        for k in range(count):
            v = s32(off + k*4)
            if not (lo <= v < hi):
                ok = False
                break
        if ok:
            if extra and not extra(off, count):
                continue
            hits.append(off)
    return hits

def scan_multi(region_s, region_e, count, check, stride, name):
    hits = []
    need = count * stride
    if count == 0 or need == 0 or need > region_e - region_s:
        return hits
    for off in range(region_s, region_e - need + 1, 4):
        if check(off, count):
            hits.append(off)
    return hits

# 值域检查函数（全量）
def all_in(off, cnt, lo, hi, stride=4):
    for k in range(cnt):
        v = s32(off + k*stride)
        if not (lo <= v < hi):
            return False
    return True

results = {}
for rname, (rs, re_) in REGIONS.items():
    print("\n== 区间 %s ==" % rname)
    # interfaces: TypeIndex
    h = scan4(rs, re_, 0, NTYPEIDX, sum_if, "interfaces")
    print("  interfaces        候选:", ["0x%x" % x for x in h][:6])
    # nestedTypes: TypeDefIndex
    h = scan4(rs, re_, 0, NTYPES, sum_nest, "nested")
    print("  nestedTypes       候选:", ["0x%x" % x for x in h][:6])
    # exportedTypeDefinitions
    if sum_exp:
        h = scan4(rs, re_, 0, NTYPES, sum_exp, "exported")
        print("  exportedTypes     候选:", ["0x%x" % x for x in h][:6])
    # referencedAssemblies: AssemblyIndex < 102
    if sum_ref:
        h = scan4(rs, re_, 0, 102, sum_ref, "refAsm")
        print("  refAssemblies     候选:", ["0x%x" % x for x in h][:6])
    # interfaceOffsets: 8B {typeIndex ∈ types, offset 任意}
    if sum_ifoff:
        def chk(off, cnt, _c=[0]):
            for k in range(cnt):
                if not (0 <= s32(off + k*8) < NTYPEIDX):
                    return False
            return True
        h = scan_multi(rs, re_, sum_ifoff, chk, 8, "ifoff")
        print("  interfaceOffsets  候选:", ["0x%x" % x for x in h][:6])
    # fieldRefs: 8B {typeIndex<NTYPEIDX, fieldIndex<NFIELDS}
    def chk_fr(off, cnt):
        for k in range(cnt):
            if not (0 <= s32(off + k*8) < NTYPEIDX and 0 <= s32(off + k*8 + 4) < NFIELDS):
                return False
        return True
    h = scan_multi(rs, re_, 4096, chk_fr, 8, "fieldRefs")  # 计数未知，试探4096
    print("  fieldRefs(试4096) 候选:", ["0x%x" % x for x in h][:4])
