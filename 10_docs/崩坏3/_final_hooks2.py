#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import struct
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

SO = "/tmp/bh3_recon/so_x/lib/arm64-v8a/libil2cpp.so"
so = open(SO, "rb").read()
md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
md.detail = True

HOOKS = [0x5F16228, 0x5F156A8, 0x5F16828]

def dis(start, end, tag):
    print("\n=== %s file 0x%x..0x%x ===" % (tag, start, end))
    for ins in md.disasm(so[start:end], start):
        ref = ""
        # detect adrp+add/ldr pairs referencing rodata strings
        if ins.mnemonic in ("adrp",):
            ref = "   ; ADRP"
        print("0x%08x  %-9s %s%s" % (ins.address, ins.mnemonic, ins.op_str, ref))

# interpretation A: offset = FILE offset
print("########## INTERPRETATION A: offset = FILE offset ##########")
for h in HOOKS:
    # search backward for prologue
    o = h
    start = None
    for _ in range(0x1000):
        o -= 4
        w = struct.unpack_from("<I", so, o)[0]
        # stp x29,x30,[sp,#-imm]!  prologue patterns
        if (w & 0xFFC003FF) == 0xA9807BFD or (w & 0xFFC07FFF) == 0xA9BF7BFD:
            start = o
            break
    if start is None:
        start = h - 0x200
    dis(start, h + 0x40, "A hook@0x%x (prologue 0x%x)" % (h, start))

# interpretation B: offset = VADDR (file = vaddr - 0x4000)
print("\n########## INTERPRETATION B: offset = VADDR (file=vaddr-0x4000) ##########")
for h in HOOKS:
    f = h - 0x4000
    dis(f - 8, f + 24, "B hook vaddr0x%x -> file0x%x" % (h, f))
