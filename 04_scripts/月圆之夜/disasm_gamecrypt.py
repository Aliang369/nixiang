#!/usr/bin/env python3.11
# 反汇编 GameDecryptionServices 全部方法
import struct
from capstone import *
from capstone.arm64 import ARM64_OP_MEM

SO = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/03_decompiled_libil2cpp.so"
d = open(SO, "rb").read()
cs = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
cs.detail = True

FUNCS = {
    "LoadFromFileOffset": (0x1D9DC9C, 0x1D9DCA4),
    "LoadFromMemory": (0x1D9DCA4, 0x1D9DCE4),
    "LoadFromStream": (0x1D9DCE4, 0x1D9DD50),
    "GetManagedReadBufferSize": (0x1D9DD50, 0x1D9DD58),
}
for fn, (a, b) in FUNCS.items():
    print(f"\n===== {fn} 0x{a:X}-0x{b:X} =====")
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
                    q = struct.unpack_from("<Q", d, va)[0]
                    note += f" =0x{q:X}"
        elif i.mnemonic in ("movz", "mov") and i.operands[0].type == 1 and len(i.operands) > 1 and i.operands[1].type == 2:
            v = i.operands[1].imm
            if v > 0x100:
                note = f"; imm=0x{v:X} ({v})"
        print(f"  0x{i.address:X}: {i.mnemonic:<8} {i.op_str} {note}")
