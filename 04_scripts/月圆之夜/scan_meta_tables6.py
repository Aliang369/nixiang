#!/usr/bin/env python3.11
# names 布局穷举定位 + methods 参数布局穷举 + assemblies
import struct

P = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_runtime.dat"
d = open(P, "rb").read()
FS = len(d)

def u32(o):
    return struct.unpack_from("<I", d, o)[0] if 0 <= o <= FS - 4 else 0xFFFFFFFF

def u16(o):
    return struct.unpack_from("<H", d, o)[0] if 0 <= o <= FS - 2 else 0xFFFF

TB, IB = 0x8AFD34, 0x9BF164

print("== methods 布局穷举 (目标 sum parameterCount=113689) ==", flush=True)
for stride in (32, 36, 40):
    for tokoff in (20, 24, 28):
        for poff in (26, 28, 30, 32, 34):
            if poff + 2 > stride:
                continue
            base = 0x278A30
            # token 链长度
            n = 0
            while base + (n + 1) * stride + tokoff <= FS:
                if (u32(base + n * stride + tokoff) >> 24) != 0x06:
                    break
                n += 1
            if n < 90000:
                continue
            tot = sum(u16(base + i * stride + poff) for i in range(n))
            tag = " <<<" if tot == 113689 else ""
            print("  stride=%d tok@%d n=%d param@%d sum=%d%s" % (stride, tokoff, n, poff, tot, tag), flush=True)

print("== names 布局定位 ==", flush=True)
hits = []
s = 0
while True:
    i = d.find(b"<Module>", s, FS)
    if i < 0:
        break
    hits.append(i)
    s = i + 1
print("  '<Module>' 位置 %d 个: %s" % (len(hits), [hex(x) for x in hits[:12]]), flush=True)

tds = [(u32(TB + i * 88), u32(TB + i * 88 + 4)) for i in range(0, 12626, 25)]  # (name, ns) 抽样

def check_layout(B, mode):
    """mode: classic / u8len / u32len / u16len ; 返回通过的抽样约束数"""
    ok = 0
    for ni, nsi in tds:
        if mode == "classic":
            p = B + ni
            e = d.find(b"\x00", p, p + 120)
            ok += e >= 0 and e > p
        elif mode == "u8len":
            L = d[B + ni]
            ok += 0 < L < 120 and 0 not in d[B + ni + 1:B + ni + 1 + L]
        elif mode == "u16len":
            L = u16(B + ni)
            ok += 0 < L < 120 and 0 not in d[B + ni + 2:B + ni + 2 + L]
        elif mode == "u32len":
            L = u32(B + ni)
            ok += 0 < L < 120 and 0 not in d[B + ni + 4:B + ni + 4 + L]
    return ok

seen = set()
for hpos in hits[:40]:
    for mode, delta in (("classic", 42), ("u8len", 43), ("u16len", 44), ("u32len", 46)):
        B = hpos - delta
        if B < 0 or B in seen:
            continue
        seen.add(B)
        ok = check_layout(B, mode)
        if ok > len(tds) * 0.9:
            print("  ✓ 候选 mode=%s B=0x%x (锚@0x%x): %d/%d" % (mode, B, hpos, ok, len(tds)), flush=True)
print("  (抽样总数 %d)" % len(tds), flush=True)

print("== assemblies 定位 ==", flush=True)
# v29 AssemblyDef 64B: imageIndex@0, token@4, refStart@8, refCount@12, aname@16(name,culture,publicKey,hash_alg,hash_len,flags,major,minor,build,revision,pkt[8])
found = 0
base = 0x100
while base < FS - 102 * 64 and found < 3:
    if u32(base) == 0 and u32(base + 64) == 1 and u32(base + 128) == 2:
        ok = all(u32(base + k * 64) == k for k in range(102))
        if ok:
            alg = u32(base + 16 + 12)   # aname.hash_alg
            hlen = u32(base + 16 + 16)  # aname.hash_len
            print("  ✓ assemblies @0x%x hash_alg=0x%x hash_len=%d" % (base, alg, hlen), flush=True)
            print("    [0]: %s" % [hex(u32(base + j * 4)) for j in range(16)], flush=True)
            found += 1
            base += 102 * 64
            continue
    base += 4
if not found:
    print("  64B 网格未命中, 试 60B", flush=True)
