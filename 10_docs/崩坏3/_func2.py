#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import struct
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

SO = "/tmp/bh3_recon/so_x/lib/arm64-v8a/libil2cpp.so"
so = open(SO, "rb").read()
md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)

# ---- program headers (which PT_LOAD has X flag, and does it cover the hooks?) ----
e_phoff = struct.unpack_from("<Q", so, 0x20)[0]
e_phentsize = struct.unpack_from("<H", so, 0x36)[0]
e_phnum = struct.unpack_from("<H", so, 0x38)[0]
print("=== program headers ===")
loads = []
for i in range(e_phnum):
    o = e_phoff + i * e_phentsize
    p_type, p_flags = struct.unpack_from("<II", so, o)
    p_offset, p_vaddr, p_paddr, p_filesz, p_memsz, p_align = struct.unpack_from("<QQQQQQ", so, o + 8)
    fl = ("R" if p_flags & 4 else "-") + ("W" if p_flags & 2 else "-") + ("X" if p_flags & 1 else "-")
    if p_type == 1:
        loads.append((p_offset, p_filesz, p_vaddr, fl))
        print("  LOAD off=%#012x filesz=%#012x vaddr=%#012x flags=%s  covers_file=[%#x,%#x)" %
              (p_offset, p_filesz, p_vaddr, fl, p_offset, p_offset + p_filesz))
    else:
        print("  PH type=%d flags=%s" % (p_type, fl))

# ---- section header flags for il2cpp ----
e_shoff = struct.unpack_from("<Q", so, 0x28)[0]
e_shentsize = struct.unpack_from("<H", so, 0x3a)[0]
e_shnum = struct.unpack_from("<H", so, 0x3c)[0]
for i in range(e_shnum):
    o = e_shoff + i * e_shentsize
    name, typ, flags, addr, off, size = struct.unpack_from("<IIQQQQ", so, o)
    if i == 15:
        print("=== section[15] il2cpp: flags=%#x (SHF_EXECINSTR=%s) off=%#x size=%#x ===" %
              (flags, bool(flags & 0x4), off, size))

HOOKS = {0x5F16228, 0x5F156A8, 0x5F16828}
TERM = {0xd65f03c0, 0xd4200000, 0x00000000, 0xd503201f, 0xd5033fdf}

def find_start(off, back=0x3000):
    o = off
    while o > off - back and o > 0:
        o -= 4
        w = struct.unpack_from("<I", so, o)[0]
        if w in TERM:
            return o + 4
    return off - back

def disasm(a, b):
    o = a
    while o < b:
        ins = list(md.disasm(so[o:o + 4], o))
        if not ins:
            print("    %08x: .word %08x" % (o, struct.unpack_from("<I", so, o)[0]))
            o += 4
            continue
        i = ins[0]
        mark = "   <== HOOK" if i.address in HOOKS else ""
        print("    %08x: %-8s %s%s" % (i.address, i.mnemonic, i.op_str, mark))
        o += 4

for V in (0x5F16228, 0x5F156A8, 0x5F16828):
    print("=" * 84)
    fs = find_start(V)
    print("### hook %#x   nearest function start (after last terminator) = %#x  (size~%#x)" % (V, fs, V - fs))
    print("  --- prologue (first 0x20) ---")
    disasm(fs, fs + 0x20)
    print("  --- around hook ---")
    disasm(V - 0x90, V + 0x34)
