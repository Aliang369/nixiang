#!/usr/bin/env python3.11
# libcybrex PLT 桩 → 外部符号名解析 + 任意 VA 反汇编（复用 disasm 逻辑）
# 用法: /tmp/fridaenv/bin/python3.11 plt_resolve.py <va1> [va2 ...]
import struct, sys
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

PATH = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/libcybrex_unpacked.so"
data = open(PATH, "rb").read()

# .dynamic: JMPREL=0x48180 PLTRELSZ=0x1bc0 SYMTAB=0x2f8 STRTAB=0x2edc (均 VA=off, 第一 LOAD off=va)
JMPREL, PLTRELSZ = 0x48180, 0x1bc0
SYMTAB, STRTAB = 0x2f8, 0x2edc
PLT_GOT = 0x4006f0  # DT_PLTGOT

def sym_name(idx):
    o = SYMTAB + idx * 24
    st_name = struct.unpack_from("<I", data, o)[0]
    end = data.find(b"\x00", STRTAB + st_name)
    return data[STRTAB + st_name:end].decode(errors="replace")

# rela.plt: r_offset(GOT槽) → symbol
got2sym = {}
for i in range(PLTRELSZ // 24):
    o = JMPREL + i * 24
    r_offset, r_info, r_addend = struct.unpack_from("<QQq", data, o)
    got2sym[r_offset] = (r_info >> 32)

md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)

def plt_target(stub_va):
    # 反汇编桩前 16B: adrp x16, page / ldr x17,[x16,#off] → GOT 槽
    o = stub_va  # off=va (RX 段)
    adrp_va = None
    page = off = None
    for ins in md.disasm(data[o:o + 16], stub_va):
        if ins.mnemonic == "adrp" and "x16" in ins.op_str:
            page = int(ins.op_str.split("#")[1], 0)
        elif ins.mnemonic == "ldr" and ins.op_str.startswith("x17, [x16"):
            off = int(ins.op_str.split("#")[1].rstrip("]"), 0)
    if page is None or off is None:
        return None, None
    slot = page + off
    if slot in got2sym:
        return sym_name(got2sym[slot]), slot
    return None, slot

for arg in sys.argv[1:]:
    va = int(arg, 16)
    name, slot = plt_target(va)
    if name is None:
        print(f"0x{va:x} → 未匹配 (反汇编如下)")
        for ins in md.disasm(data[va:va + 16], va):
            print(f"   0x{ins.address:x}: {ins.mnemonic} {ins.op_str}")
    else:
        print(f"0x{va:x} → GOT 0x{slot:x} = {name}")
