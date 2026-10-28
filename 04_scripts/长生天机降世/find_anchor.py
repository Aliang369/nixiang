# -*- coding: utf-8 -*-
"""在 libil2cpp.so 中按 1.0.3 记录的指令签名定位 EnterFrameFromInterpreter（线性反汇编版）。

签名（1.0.3 实测）：
    str  x30, [sp, #-0x30]!      ← 函数入口
    mov  x20, x1
    ldr  x8,  [x20, #0x58]
    ldr  w9,  [x8,  #0x2c]
    bl   <A>
    bl   <B>
    stp  x20, x19, [x0]          ← 帧压入
"""
import struct
import sys

from capstone import Cs, CS_ARCH_ARM64, CS_MODE_ARM

PATH = sys.argv[1]
REGION = sys.argv[2] if len(sys.argv) > 2 else "0x2600000:0x2700000"
lo, hi = [int(x, 16) for x in REGION.split(":")]

so = open(PATH, "rb").read()
e_phoff = struct.unpack_from("<Q", so, 32)[0]
e_phentsize = struct.unpack_from("<H", so, 54)[0]
e_phnum = struct.unpack_from("<H", so, 56)[0]
loads = []
for i in range(e_phnum):
    off = e_phoff + i * e_phentsize
    p_type, p_flags = struct.unpack_from("<II", so, off)
    if p_type != 1:
        continue
    p_offset, p_vaddr = struct.unpack_from("<QQ", so, off + 8)
    p_filesz = struct.unpack_from("<Q", so, off + 32)[0]
    loads.append((p_offset, p_vaddr, p_filesz, p_flags))
t_off, t_va, t_sz, _ = max((l for l in loads if l[3] & 1), key=lambda t: t[2])
delta = t_va - t_off
print(f"text va=0x{t_va:x} file=0x{t_off:x} size=0x{t_sz:x} delta=0x{delta:x}")

f_lo = lo - delta
f_hi = hi - delta
buf = so[f_lo:f_hi]

md = Cs(CS_ARCH_ARM64, CS_MODE_ARM)
md.skipdata = True

insns = list(md.disasm(buf, lo))
print(f"disassembled {len(insns)} insns in region")

# 收集候选入口：str x30, [sp, #-0x30]!
cands = []
for k, ins in enumerate(insns):
    if ins.mnemonic == "str" and ins.op_str.replace(" ", "").startswith("x30,[sp,#-0x"):
        cands.append(k)
print(f"prologue(str x30,[sp,#-N]!) candidates: {len(cands)}")

def clean(s):
    return s.replace(" ", "")

found = []
for k in cands:
    win = insns[k:k + 26]
    ops = [(x.mnemonic, clean(x.op_str)) for x in win]
    has_mov = ("mov", "x20,x1") in ops
    has_ldr58 = ("ldr", "x8,[x20,#0x58]") in ops
    has_w9 = ("ldr", "w9,[x8,#0x2c]") in ops
    has_stp = any(m == "stp" and o.startswith("x20,x19,[x0]") for m, o in ops)
    nbl = sum(1 for m, _ in ops if m == "bl")
    score = sum([has_mov, has_ldr58, has_w9, has_stp]) + (1 if nbl >= 2 else 0)
    if score >= 4:
        found.append((win[0].address, win, score))

print(f"\n=== signature matches: {len(found)} ===")
for va, win, score in found:
    print(f"\n-- va=0x{va:x}  file=0x{va - delta:x}  score={score}")
    for x in win[:12]:
        print(f"   0x{x.address:x}  {x.mnemonic:<8}{x.op_str}")
