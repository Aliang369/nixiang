#!/usr/bin/env python3.11
# 通过 RELA + usage 记录 + 字面量表, 定位 yyzy 密码/热更文件名字符串被哪些槽引用
import struct, subprocess, re

SO = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/03_decompiled_libil2cpp.so"
MEM = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/il2cpp_mem_full.bin"
META = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_v29_dec.dat"
d = open(SO, "rb").read()
m = open(MEM, "rb").read()
md = open(META, "rb").read()
SL, LD, NLIT = 0x100, 0x228C8, 141256 // 8


def lit(i):
    if not (0 <= i < NLIT):
        return None
    a, b = struct.unpack_from("<ii", md, SL + i * 8)
    return md[LD + b:LD + b + a].decode("utf-8", "replace")


# 1) 扫 .so 中 usage 记录区: 8B token, 高 nibble 0xA/0x2/0x4/0x6/0xC0000000 型
#    记录区范围: 从槽的 file 值看约 0x5280000..0x5320000
rec2lit = {}
lo, hi = 0x5200000, 0x5320000
for va in range(lo, hi, 8):
    t = struct.unpack_from("<Q", d, va)[0]
    ty = (t >> 29) & 7
    idx = (t & 0x1FFFFFFE) >> 1
    if ty == 5 and 0 < idx < NLIT:
        s = lit(idx)
        if s and ("yyzy" in s or "HybridCLR" in s or "hotfix" in s.lower() or "base.bytes" in s or s == "GameFramework"):
            rec2lit[va] = s
print("== 命中 yyzy/HybridCLR 相关 usage 记录 ==")
for va, s in sorted(rec2lit.items()):
    print("  rec@0x%X -> %r" % (va, s))

# 2) RELA: 槽(addend=记录VA) 映射
out = subprocess.run(["/opt/homebrew/opt/llvm/bin/llvm-readelf", "-r", SO],
                     capture_output=True, text=True).stdout
relmap = {}   # recordVA -> [slotVA...]
pat = re.compile(r"^([0-9a-f]+)\s+[0-9a-f]+\s+R_AARCH64_RELATIVE\s+([0-9a-f]+)\s*$")
for line in out.splitlines():
    mo = pat.match(line.strip())
    if mo:
        slot = int(mo.group(1), 16)
        addend = int(mo.group(2), 16)
        if addend in rec2lit:
            relmap.setdefault(addend, []).append(slot)
print("\n== 记录 → 引用槽 (RELA) ==")
for rec, slots in sorted(relmap.items()):
    print("  rec@0x%X %r <- slots %s" % (rec, rec2lit[rec], [hex(s) for s in slots]))

# 3) LoadDLL.* 的槽 (来自 resolve_slots.py 的收集)
FUNCS = {"LoadHotfix": (0x1D9F134, 0x1D9F2C4), "DecompressBytes": (0x1D9F454, 0x1D9F990),
         "LoadAssemble": (0x1D9F990, 0x1DA0F00)}
from capstone import *
from capstone.arm64 import ARM64_OP_MEM
cs = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
cs.detail = True
used = {}
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
                used.setdefault(adrp_val[base] + i.operands[1].mem.disp, set()).add(fn)
                if base == dst:
                    del adrp_val[base]
                    continue
        if dst is not None and dst in adrp_val:
            del adrp_val[dst]
print("\n== LoadDLL.* 槽 ∩ yyzy 记录槽 ==")
hit = False
for rec, slots in relmap.items():
    for s in slots:
        if s in used:
            hit = True
            print("  slot 0x%X (%s) -> %r" % (s, ",".join(used[s]), rec2lit[rec]))
if not hit:
    print("  (无交集)")
print("\n== LoadDLL.* 引用的全部字符串槽 ==")
for va, fns in sorted(used.items()):
    fv = struct.unpack_from("<Q", d, va)[0] if va + 8 <= len(d) else 0
    s = rec2lit.get(fv)
    if s:
        print("  0x%-8X (%s) -> %r" % (va, ",".join(sorted(fns)), s))
