#!/usr/bin/env python3.11
# 1) libil2cpp 内存 dump 搜 header 副本/真实偏移常量  2) metadata 里 names 基址结构扫描
import struct

MEM = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/il2cpp_mem_full.bin"
META = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_runtime.dat"
m = open(MEM, "rb").read()
d = open(META, "rb").read()
FS = len(d)
print("mem=0x%x meta=0x%x" % (len(m), FS), flush=True)

def u32b(buf, o):
    return struct.unpack_from("<I", buf, o)[0]

print("== 内存 dump 搜 FAB11BAF ==", flush=True)
s = 0
while True:
    i = m.find(b"\xaf\x1b\xb1\xfa", s)
    if i < 0:
        break
    ver = u32b(m, i + 4)
    print("  @0x%x version=%d" % (i, ver), flush=True)
    if 0x100 <= i + 0x100 <= len(m):
        # 打印前 6 对
        print("    pairs:", [(hex(u32b(m, i + 8 + k * 8)), hex(u32b(m, i + 12 + k * 8))) for k in range(6)], flush=True)
    s = i + 1

print("== 内存 dump 搜已知表偏移常量 (u32 LE) ==", flush=True)
known = {
    0x8AFD34: "typeDefs", 0x9BF164: "images", 0x9C0154: "assemblies",
    0x278A30: "methods", 0x5F39BC: "params", 0x740AE8: "fields",
    0x2227DC: "events", 0x223C4C: "props",
}
for val, name in known.items():
    pat = struct.pack("<I", val)
    cnt = 0
    poss = []
    s = 0
    while cnt < 5:
        i = m.find(pat, s)
        if i < 0:
            break
        poss.append(hex(i))
        cnt += 1
        s = i + 1
    print("  %-11s 0x%x: %s" % (name, val, poss), flush=True)

print("== metadata names 基址结构扫描 ==", flush=True)
TB, IB = 0x8AFD34, 0x9BF164
img_ni = [u32b(d, IB + k * 40) for k in range(102)]
tds = [(u32b(d, TB + i * 88), u32b(d, TB + i * 88 + 4)) for i in range(0, 12626, 25)]

cands = []
for B in range(0, FS - 200):
    # names[0..8]="mscorlib"(8字符密文)+\0 → d[B+9]==0? 不对: names[0] 是串起点偏移0, "mscorlib"8字符占 B..B+7, \0@B+8, names[9]=""@\x0 → d[B+9]==0?
    # 若 names[0] 8 字符+\0 = 9B (0..8), names[9]="" = \0@B+9
    if d[B + 9] != 0:
        continue
    if 0 in d[B + 29:B + 41] or d[B + 41] != 0:
        continue
    if 0 in d[B + 42:B + 50] or d[B + 50] != 0:
        continue
    cands.append(B)
print("  约束候选 %d 个: %s" % (len(cands), [hex(x) for x in cands[:20]]), flush=True)

def score(B):
    ok = 0
    for ni, nsi in tds:
        e = d.find(b"\x00", B + ni, B + ni + 120)
        if e >= 0:
            ok += 1
    okimg = 0
    for ni in img_ni:
        e = d.find(b"\x00", B + ni, B + ni + 90)
        if e >= 0:
            okimg += 1
    return ok, okimg

for B in cands[:20]:
    ok, okimg = score(B)
    print("  B=0x%x: typeDef抽样 %d/%d, image名 %d/102" % (B, ok, len(tds), okimg), flush=True)
