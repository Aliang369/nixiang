#!/usr/bin/env python3.11
# names 基址定位 + 表长度 + assemblies
import struct

P = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_runtime.dat"
d = open(P, "rb").read()
FS = len(d)

def u32(o):
    return struct.unpack_from("<I", d, o)[0] if 0 <= o <= FS - 4 else 0xFFFFFFFF

def u16(o):
    return struct.unpack_from("<H", d, o)[0] if 0 <= o <= FS - 2 else 0xFFFF

def run_len(base, stride, tokoff, hi, limit=300000):
    n = 0
    while base + (n + 1) * stride + tokoff <= FS and n < limit:
        if (u32(base + n * stride + tokoff) >> 24) != hi:
            break
        n += 1
    return n

print("== 表长度 ==", flush=True)
for name, b, st, to, hi in [
    ("methods", 0x278A2C, 32, 24, 0x06),
    ("fields", 0x740AE8, 12, 8, 0x04),
    ("params", 0x5F39BC, 12, 4, 0x08),
    ("props", 0x223C4C, 20, 16, 0x17),
    ("events", 0x2227DC, 24, 20, 0x14),
]:
    n = run_len(b, st, to, hi)
    print("  %-8s @0x%x stride%d n=%d end=0x%x" % (name, b, st, n, b + n * st), flush=True)

ib = 0x9BF164
img = [(u32(ib + k * 40), u32(ib + k * 40 + 8), u32(ib + k * 40 + 12)) for k in range(102)]  # nameIdx, typeStart, typeCount
TB = 0x8AFD34  # typeDefs
print("\n== typeDefs[0..3] 记录抽查 ==", flush=True)
for i in range(4):
    b = TB + i * 88
    print("  [%d] name=%d ns=%d byval=%d parent=%d flags=0x%x fieldStart=%d methodStart=%d ev=%d prop=%d vtab=%d counts=(%d,%d,%d,%d) bit=0x%x tok=0x%x" % (
        i, u32(b), u32(b + 4), u32(b + 8), u32(b + 16), u32(b + 28), u32(b + 32), u32(b + 36),
        u32(b + 40), u32(b + 44), u32(b + 56), u16(b + 64), u16(b + 66), u16(b + 68), u16(b + 70),
        u32(b + 80), u32(b + 84)), flush=True)

print("\n== names 基址定位 ==", flush=True)
# 候选1: images 表尾 0x9c0154
for cand in (0x9C0154,):
    print("  cand 0x%x head: %r" % (cand, d[cand:cand + 120][:120]), flush=True)
# 全文件 mscorlib.dll\0
hits = []
s = 0
while True:
    i = d.find(b"mscorlib.dll\x00", s, FS)
    if i < 0:
        break
    hits.append(i)
    s = i + 1
print("  'mscorlib.dll\\0' 位置:", [hex(x) for x in hits], flush=True)

def validate(B, sample=102):
    ok_img = 0
    for ni, _, _ in img:
        p = B + ni
        e = d.find(b"\x00", p, p + 90)
        if e >= 0 and d[p:e].endswith(b".dll"):
            ok_img += 1
    # typeDef nameIndex 抽样校验: 指向 null 结尾串
    ok_td = 0
    tot = 0
    for i in range(0, 12626, max(1, 12626 // sample)):
        ni = u32(TB + i * 88)
        p = B + ni
        e = d.find(b"\x00", p, p + 150)
        if e >= 0 and all(0x20 <= c < 0x7F or c >= 0x80 for c in d[p:min(e, p + 60)]) and e > p:
            ok_td += 1
        tot += 1
    return ok_img, ok_td, tot

for h in hits:
    B = h - 29
    ok_img, ok_td, tot = validate(B)
    print("  B=0x%x (mscorlib@0x%x): img %d/102, typeDef nameIdx ok %d/%d" % (B, h, ok_img, ok_td, tot), flush=True)
