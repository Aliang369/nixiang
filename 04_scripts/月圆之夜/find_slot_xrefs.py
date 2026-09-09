#!/usr/bin/env python3.11
# 扫描 libil2cpp.so 可执行区, 找引用指定 usage 槽 (adrp+ldr) 的指令, 归属到 dump.cs 方法 v2
import struct, bisect, re, subprocess

SO = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/03_decompiled_libil2cpp.so"
DUMP = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/dump_out/dump.cs"
d = open(SO, "rb").read()

SLOTS = {
    0x5139188: "yyzy{0}yyzy0",
    0x513db20: "yyzy_hotfix",
    0x5140d80: "yyzy_base",
    0x511a5d0: "yyzy_{0}.log",
    0x514ff68: "Assets/HybridCLRData/hotfix.bytes",
    0x5162108: "Assets/HybridCLRData/base.bytes",
}

# ---- dump.cs 方法表: RVA -> 签名 (RVA 行下面第一条非注释行) ----
lines = open(DUMP, "r", errors="replace").read().splitlines()
rvas = []
names = {}
for i, line in enumerate(lines):
    mo = re.search(r"// RVA: 0x([0-9A-Fa-f]+) Offset", line)
    if mo:
        rva = int(mo.group(1), 16)
        sig = ""
        for j in range(i + 1, min(i + 4, len(lines))):
            s = lines[j].strip()
            if s and not s.startswith("//") and not s.startswith("|"):
                sig = s
                break
        rvas.append(rva)
        names[rva] = sig[:130]
rvas.sort()
print("methods: %d" % len(rvas))


def owner(rva):
    i = bisect.bisect_right(rvas, rva) - 1
    return (rvas[i], names.get(rvas[i], "?")) if i >= 0 else None


# ---- 可执行段: .text + il2cpp ----
EXEC = [(0xD9FDB0, 0xD9FDB0 + 0x7BF904), (0x155F6B4, 0x155F6B4 + 0x2789FA0)]
by_off = {}
for s in SLOTS:
    off = s & 0xFFF
    assert off % 8 == 0
    by_off.setdefault(off // 8, {})[s] = 1

res = []
for T0, T1 in EXEC:
    n = (T1 - T0) // 4
    words = struct.unpack_from("<%dI" % n, d, T0)
    hits = []
    for j, w in enumerate(words):
        if (w & 0xFFC00000) == 0xF9400000:
            imm = (w >> 10) & 0xFFF
            if imm in by_off:
                hits.append((j, imm, w & 0x1F))
    print("段 0x%X..0x%X 候选 LDR: %d" % (T0, T1, len(hits)))
    for j, imm, rd in hits:
        ldr_va = T0 + j * 4
        for k in range(j - 1, max(j - 64, -1), -1):
            w = words[k]
            if (w & 0x9F000000) == 0x90000000:  # ADRP
                rdd = w & 0x1F
                if rdd != rd:
                    continue
                immlo = (w >> 29) & 3
                immhi = (w >> 5) & 0x7FFFF
                simm = (immhi << 2) | immlo
                if simm & (1 << 20):
                    simm -= 1 << 21
                adrp_va = T0 + k * 4
                page = (adrp_va & ~0xFFF) + (simm << 12)
                for s in by_off[imm]:
                    if (s & ~0xFFF) == page:
                        res.append((s, adrp_va, ldr_va))
            elif (w & 0xFFC00000) in (0xF9400000, 0xB9400000) and (w & 0x1F) == rd:
                break  # 同寄存器被重新加载 → adrp 链断
            elif (w & 0xFFE0FFE0) == 0xAA0003E0 or ((w >> 31) == 1 and (w & 0x1F) == rd and (w & 0x7F800000) in (0x52800000, 0x11000000)):
                break

seen = {}
for s, a, l in res:
    o = owner(l)
    seen.setdefault(s, []).append((l, a, o))
print("\n== 槽引用方法 ==")
for s, lst in sorted(seen.items()):
    print("slot 0x%X (%s):" % (s, SLOTS[s]))
    for l, a, o in lst:
        print("   ldr@0x%X adrp@0x%X → 方法 0x%X %s" % (l, a, o[0] if o else 0, o[1] if o else "?"))
