#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import struct
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

SEG = "/Users/aliang/Documents/nixiang/06_dumped/崩坏3/mem_20261008/bh3_shell/seg_23_rw-p_7349938000.bin"
data = open(SEG, "rb").read()

def hexdump(b, base):
    for i in range(0, len(b), 16):
        chunk = b[i:i + 16]
        hx = " ".join("%02x" % c for c in chunk)
        asc = "".join(chr(c) if 32 <= c < 127 else "." for c in chunk)
        print("0x%06x  %-47s  %s" % (base + i, hx, asc))

print("=== seg_23 table region 0x24d00..0x24f60 (HOOK records) ===")
hexdump(data[0x24d00:0x24f60], 0x24d00)

print()
print("=== seg_23 UIKEY region 0x25020..0x251a0 ===")
hexdump(data[0x25020:0x251a0], 0x25020)

# ---- disassemble the three hook host functions ----
SO = "/tmp/bh3_recon/so_x/lib/arm64-v8a/libil2cpp.so"
so = open(SO, "rb").read()
md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
md.detail = True

def show(name, start, end):
    print()
    print("=== %s : file 0x%x..0x%x ===" % (name, start, end))
    code = so[start:end]
    for ins in md.disasm(code, start):
        line = "0x%08x  %-8s %s" % (ins.address, ins.mnemonic, ins.op_str)
        # flag adrp/add string refs
        print(line)

show("hook#1 @0x5f16228 (host 0x5f161fc)", 0x5f161d0, 0x5f16270)
show("hook#2 @0x5f156a8 (host 0x5f155c4)", 0x5f155c4, 0x5f156e0)
show("hook#3 @0x5f16828 (host 0x5f16268)", 0x5f16268, 0x5f16320)
