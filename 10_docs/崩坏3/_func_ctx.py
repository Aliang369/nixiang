#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import struct
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

SO = "/tmp/bh3_recon/so_x/lib/arm64-v8a/libil2cpp.so"
so = open(SO, "rb").read()
md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
md.detail = False

def is_prologue(insn):
    # stp x29,x30,[sp,#-imm]!  -> 0xA9?0?7BFD family ; sub sp,sp,#imm -> 0xD10?03FF
    if (insn & 0xFFC003FF) in (0xA9807BFD, 0xA9007BFD):
        return True
    if (insn & 0xFF8003FF) == 0xD10003FF:
        return True
    # stp x29,x30,[sp,#-imm]! generic: bits
    if (insn & 0xFFE003FF) == 0xA9807BFD:
        return True
    return False

def find_func_start(off, back=0x6000):
    # walk backwards for a prologue that is a genuine function head:
    # preceded by ret(0xd65f03c0)/b/brk/padding
    start = off - back
    best = None
    o = off
    while o > start:
        o -= 4
        insn = struct.unpack_from("<I", so, o)[0]
        if is_prologue(insn):
            prev = struct.unpack_from("<I", so, o - 4)[0] if o - 4 >= 0 else 0
            # prev instruction terminates prior function?
            if (prev & 0xFFFFFFFF) in (0xD65F03C0, 0xD503201F) or (prev >> 26) == 0x05 or prev == 0:
                best = o
                break
            best = o  # keep as fallback
    return best if best else off

def disasm(a, b):
    print("  --- disasm 0x%x .. 0x%x ---" % (a, b))
    for ins in md.disasm(so[a:b], a):
        mark = "   <== HOOK" if ins.address in HOOKS else ""
        print("    %08x: %-9s %s%s" % (ins.address, ins.mnemonic, ins.op_str, mark))

HOOKS = {0x5F16228, 0x5F156A8, 0x5F16828}

for V in (0x5F16228, 0x5F156A8, 0x5F16828):
    print("=" * 90)
    print("### hook file-offset %#x" % V)
    fs = find_func_start(V)
    print("  enclosing func start (heuristic): %#x  (delta=%#x)" % (fs, V - fs))
    disasm(fs, V + 0x60)
