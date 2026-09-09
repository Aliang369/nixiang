#!/usr/bin/env python3.11
# 定位 LoadDLL.* 引用的字符串槽 + yyzy 密钥模板 xref
import struct

SO = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/03_decompiled_libil2cpp.so"
META = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_v29_dec.dat"
d = open(SO, "rb").read()
md = open(META, "rb").read()
SL, LD, NLIT = 0x100, 0x228C8, 17657

def lit(i):
    if not (0 <= i < NLIT):
        return None
    a, b = struct.unpack_from("<ii", md, SL + i * 8)
    if a <= 0 or a > 4096:
        return None
    return md[LD + b: LD + b + a].decode("utf-8", "replace")

TARGET = {17197, 17198, 17199, 17200, 2429, 2430, 5562, 5570}

# 1) usage 记录扫描
rec2lit = {}
lo, hi = 0x5100000, 0x5400000
for va in range(lo, hi, 8):
    t = struct.unpack_from("<Q", d, va)[0]
    ty = (t >> 29) & 7
    idx = (t & 0x1FFFFFFE) >> 1
    if ty == 5 and 0 < idx < NLIT:
        if idx in TARGET:
            rec2lit[va] = idx
print("== 目标字面量的 usage 记录 ==")
for va, idx in sorted(rec2lit.items()):
    print("  rec@0x%X -> [%d] %r" % (va, idx, lit(idx)))

# 2) RELA: 记录VA -> 槽VA
import re, subprocess
out = subprocess.run(["/opt/homebrew/opt/llvm/bin/llvm-readelf", "-r", SO],
                     capture_output=True, text=True).stdout
relmap = {}
pat = re.compile(r"^([0-9a-f]+)\s+[0-9a-f]+\s+R_AARCH64_RELATIVE\s+([0-9a-f]+)\s*$")
for line in out.splitlines():
    mo = pat.match(line.strip())
    if mo:
        slot = int(mo.group(1), 16)
        addend = int(mo.group(2), 16)
        if addend in rec2lit:
            relmap.setdefault(addend, []).append(slot)
slots2rec = {}
for rec, ss in relmap.items():
    for s in ss:
        slots2rec[s] = rec
print("\n== 记录 → 槽 ==")
for rec, ss in sorted(relmap.items()):
    print("  [%d] %r <- %s" % (rec2lit[rec], lit(rec2lit[rec]), [hex(x) for x in ss]))

# 3) 函数区间 ADRP+LDR 扫描
from capstone import *
from capstone.arm64 import ARM64_OP_MEM
cs = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
cs.detail = True
FUNCS = {
    "LoadDLL.Start": (0x1D9F120, 0x1D9F134),
    "LoadDLL.LoadHotfix": (0x1D9F134, 0x1D9F2C4),
    "LoadDLL.LoadMetaData": (0x1D9F2C4, 0x1D9F454),
    "LoadDLL.DecompressBytes": (0x1D9F454, 0x1D9F990),
    "LoadDLL.LoadAssemble": (0x1D9F990, 0x1DA0F00),
    "LoadDLL.<>c.<LoadHotfix>b__1_0": (0x3264110, 0x3264264),
    "LoadDLL.<>c.<LoadMetaData>b__3_0": (0x3264264, 0x3264400),
}
func_uses = {}
for fn, (a, b) in FUNCS.items():
    adrp_val = {}
    for i in cs.disasm(d[a:b], a):
        dst = i.operands[0].reg if i.operands else None
        if i.mnemonic == "adrp":
            op = i.operands[1]
            tv = getattr(op, "imm", getattr(op, "value", 0))
            if isinstance(tv, bytes):
                tv = int.from_bytes(tv, "little")
            adrp_val[i.operands[0].reg] = int(tv) & 0xFFFFFFFFFFFFFFFF
            continue
        if i.mnemonic == "ldr" and len(i.operands) == 2 and i.operands[1].type == ARM64_OP_MEM:
            base = i.operands[1].mem.base
            if base in adrp_val:
                va = adrp_val[base] + i.operands[1].mem.disp
                func_uses.setdefault(va, set()).add(fn)
                if base == dst:
                    del adrp_val[base]
                    continue
        if dst is not None and dst in adrp_val:
            del adrp_val[dst]

print("\n== 函数 ∩ 目标字符串槽 ==")
hit = False
for va, fns in sorted(func_uses.items()):
    if va in slots2rec:
        hit = True
        idx = rec2lit[slots2rec[va]]
        print("  slot 0x%X [%d] %r <- %s" % (va, idx, lit(idx), ",".join(sorted(fns))))
if not hit:
    print("  (无交集)")

print("\n== LoadDLL.* 引用的全部字符串槽 ==")
for va, fns in sorted(func_uses.items()):
    fv = struct.unpack_from("<Q", d, va)[0] if va + 8 <= len(d) else 0
    ty = (fv >> 29) & 7
    idx = (fv & 0x1FFFFFFE) >> 1
    if ty == 5 and 0 < idx < NLIT:
        s = lit(idx)
        if s:
            print("  0x%-8X (%-28s) [%5d] %r" % (va, ",".join(sorted(fns)), idx, s[:60]))
