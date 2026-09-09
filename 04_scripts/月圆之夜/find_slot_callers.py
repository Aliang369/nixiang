#!/usr/bin/env python3.11
# 全 so 扫 ADRP+LDR, 找引用关键字符串槽的函数
import struct, pickle, bisect

SO = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/03_decompiled_libil2cpp.so"
d = open(SO, "rb").read()

SLOTS = {
    0x5139188: "yyzy{0}yyzy0",
    0x513db20: "yyzy_hotfix",
    0x5140d80: "yyzy_base",
    0x514ff68: "Assets/HybridCLRData/hotfix.bytes",
    0x5162108: "Assets/HybridCLRData/base.bytes",
    0x511a5d0: "yyzy_{0}.log",
}
BY_PAGE = {}
for va, name in SLOTS.items():
    BY_PAGE.setdefault(va >> 12, []).append((va, name))

hits = []  # (pc, slotva, name)
pc = 0
N = (len(d) - 8) & ~3
while pc < N:
    insn = struct.unpack_from("<I", d, pc)[0]
    if (insn & 0x9F000000) == 0x90000000:  # ADRP
        rd = insn & 0x1F
        immhi = (insn >> 5) & 0x7FFFF
        immlo = (insn >> 29) & 3
        imm = (immhi << 2 | immlo) << 12
        if imm & (1 << 32):
            imm -= 1 << 33
        page = ((pc & ~0xFFF) + imm) >> 12
        if page in BY_PAGE:
            # 向后扫 64 条找 LDR xM,[xR,#off]
            for j in range(1, 64):
                pc2 = pc + j * 4
                if pc2 >= N:
                    break
                insn2 = struct.unpack_from("<I", d, pc2)[0]
                if (insn2 & 0xFFC00000) == 0xF9400000:  # LDR Xt,[Xn,#imm]
                    rn = (insn2 >> 5) & 0x1F
                    if rn != rd:
                        continue
                    imm12 = ((insn2 >> 10) & 0xFFF) << 3
                    va = ((pc & ~0xFFF) + imm) + imm12
                    for sva, name in BY_PAGE[page]:
                        if sva == va:
                            hits.append((pc2, va, name))
                    break
    pc += 4

m = pickle.load(open("/Users/aliang/Documents/nixiang/04_scripts/月圆之夜/rva_map.pkl", "rb"))
keys = sorted(k for k in m if isinstance(k, int))


def who(pc):
    i = bisect.bisect_right(keys, pc) - 1
    if i < 0:
        return "?"
    base = keys[i]
    return "%s %s" % (m[base][0], m[base][1].split("\n")[0][:80])


by_fn = {}
for pc, va, name in hits:
    by_fn.setdefault(name, []).append(pc)
for name, lst in by_fn.items():
    print("== %s ==" % name)
    fns = {}
    for pc in lst:
        fns.setdefault(who(pc), []).append(hex(pc))
    for fn, pcs in fns.items():
        print("   %s <- %s" % (fn, ",".join(pcs[:6])))
