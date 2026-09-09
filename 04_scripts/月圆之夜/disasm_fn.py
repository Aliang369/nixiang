#!/usr/bin/env python3.11
# 通用函数反汇编: disasm_fn.py <start> <end>
import struct, re, sys
from capstone import *
from capstone.arm64 import ARM64_OP_MEM

SO = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/03_decompiled_libil2cpp.so"
DUMP = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/dump_out3/dump.cs"
d = open(SO, 'rb').read()
rva2name = {}
cur = None
for line in open(DUMP, encoding='utf-8', errors='replace'):
    m = re.match(r"^(?:\S+\s+)*(class|struct|enum|interface)\s+(\S+)", line)
    if m:
        cur = m.group(2)
    mr = re.search(r"// RVA: 0x([0-9A-Fa-f]+)", line)
    if mr:
        rva2name[int(mr.group(1), 16)] = f"{cur}::{line.split('// RVA')[0].strip()}"
cs = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
cs.detail = True
a = int(sys.argv[1], 16)
b = int(sys.argv[2], 16)
adrp = {}
for i in cs.disasm(d[a:b], a):
    note = ""
    if i.mnemonic == "adrp" and len(i.operands) == 2:
        try:
            adrp[i.operands[0].reg] = i.operands[1].imm & 0xFFFFFFFFFFFFFFFF
            note = f"; page=0x{i.operands[1].imm & 0xFFFFFFFFFFFFFFFF:X}"
        except Exception:
            pass
    elif i.operands and i.operands[-1].type == ARM64_OP_MEM:
        base = i.operands[-1].mem.base
        if base in adrp:
            va = (adrp[base] + i.operands[-1].mem.disp) & 0xFFFFFFFFFFFFFFFF
            note = f"; @0x{va:X}"
            if va + 8 <= len(d):
                q = struct.unpack_from('<Q', d, va)[0]
                note += f" =0x{q:X}"
                if q in rva2name:
                    note += f" ->{rva2name[q]}"
    elif i.mnemonic in ("bl", "b") and i.operands[0].type == 2:
        t = i.operands[0].imm
        note = f"; -> {rva2name.get(t, hex(t))}"
    if i.mnemonic == "movz" and len(i.operands) > 1 and i.operands[1].type == 2:
        v = i.operands[1].imm & 0xFFFFFFFFFFFF
        if v:
            note += f"; imm={v}(0x{v:X})"
    print(f"  0x{i.address:X}: {i.mnemonic:<7} {i.op_str} {note}")
