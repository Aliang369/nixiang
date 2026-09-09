#!/usr/bin/env python3.11
# sum 交叉验证 + names 基址(<Module>锚点) + on-disk 对比
import struct

P = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_runtime.dat"
d = open(P, "rb").read()
FS = len(d)

def u32(o):
    return struct.unpack_from("<I", d, o)[0] if 0 <= o <= FS - 4 else 0xFFFFFFFF

def u16(o):
    return struct.unpack_from("<H", d, o)[0] if 0 <= o <= FS - 2 else 0xFFFF

TB, IB = 0x8AFD34, 0x9BF164
MB, FB, PB, PRB, EB = 0x278A30, 0x740AE8, 0x5F39BC, 0x223C4C, 0x2227DC

print("== sum 交叉验证 ==", flush=True)
sm = sf = sp = se = sv = 0
for i in range(12626):
    b = TB + i * 88
    sm += u16(b + 64)   # method_count
    sp += u16(b + 66)   # property_count
    sf += u16(b + 68)   # field_count
    se += u16(b + 70)   # event_count
    sv += u16(b + 74)   # vtable_count
print("  sum method_count=%d (表实测 98644)  sum field_count=%d (55037)" % (sm, sf), flush=True)
print("  sum property_count=%d (17381)  sum event_count=%d (218)  sum vtable=%d" % (sp, se, sv), flush=True)
mp = 0
for i in range(98644):
    mp += u16(MB + i * 32 + 34)  # parameterCount
print("  sum parameterCount=%d (params 表实测 113689)" % mp, flush=True)

print("== names 基址 (<Module> 锚点) ==", flush=True)
hits = []
s = 0
while True:
    i = d.find(b"<Module>\x00", s, FS)
    if i < 0:
        break
    hits.append(i)
    s = i + 1
print("  '<Module>\\0' 位置:", [hex(x) for x in hits[:20]], "共 %d" % len(hits), flush=True)
for hpos in hits[:10]:
    B = hpos - 42
    ok0 = d[B + 9] == 0  # typeDef[0].namespaceIndex=9 → 空串
    # image0 nameIdx=29 → B+29 起 12 字节非零 + \0 (mscorlib.dll, 可能密文)
    seg = d[B + 29:B + 41]
    ok1 = 0 not in seg and B + 41 < FS and d[B + 41] == 0
    # typeDef nameIndex 抽样: B+ni 指向 90 字节内有 \0
    ok_td = 0
    for i in range(0, 12626, 63):
        ni = u32(TB + i * 88)
        e = d.find(b"\x00", B + ni, B + ni + 90)
        if e >= 0:
            ok_td += 1
    print("  B=0x%x: ns9零=%s mscorlib段=%s typeDef ok=%d/201" % (B, ok0, ok1, ok_td), flush=True)

print("== on-disk 对比 ==", flush=True)
Q = "/Users/aliang/Documents/nixiang/03_decompiled/月圆之夜/apktool_1.6.30.1/assets/bin/Data/Managed/Metadata/global-metadata.dat"
e = open(Q, "rb").read()
print("  on-disk size=0x%x (runtime 0x%x)" % (len(e), FS), flush=True)
same = sum(1 for a, b in zip(e[:0x100], d[:0x100]) if a == b)
print("  头部 0x100 字节相同数: %d/256" % same, flush=True)
diff_blocks = []
for off in range(0, min(len(e), FS), 4096):
    if e[off:off + 4096] != d[off:off + 4096]:
        diff_blocks.append(off)
print("  4K 差异块数: %d / %d" % (len(diff_blocks), min(len(e), FS) // 4096), flush=True)
if diff_blocks:
    print("  前 20 个差异块:", [hex(x) for x in diff_blocks[:20]], flush=True)
    # 差异块内相同字节比例
    o = diff_blocks[0]
    eq = sum(1 for a, b in zip(e[o:o + 4096], d[o:o + 4096]) if a == b)
    print("  首个差异块相同字节: %d/4096" % eq, flush=True)
