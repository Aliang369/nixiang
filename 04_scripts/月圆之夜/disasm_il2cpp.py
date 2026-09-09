#!/usr/bin/env python3.11
# libil2cpp.so 窗口反汇编 (按 PHDR 映射 VA<->file offset)
# 用法: python3.11 disasm_il2cpp.py <va_hex> [before=0x40] [after=0xC0]
import sys, struct
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

SO = "/Users/aliang/Documents/nixiang/03_decompiled/月圆之夜/apktool_1.6.30.1/lib/arm64-v8a/libil2cpp.so"
data = open(SO, "rb").read()

# 解析 ELF64 PHDR
e_phoff = struct.unpack_from("<Q", data, 0x20)[0]
e_phentsize = struct.unpack_from("<H", data, 0x36)[0]
e_phnum = struct.unpack_from("<H", data, 0x38)[0]
PHDRS = []
for i in range(e_phnum):
    p = e_phoff + i * e_phentsize
    p_type, p_flags = struct.unpack_from("<II", data, p)
    p_offset, p_vaddr, p_paddr, p_filesz, p_memsz = struct.unpack_from("<QQQQQ", data, p + 8)
    if p_type == 1:  # PT_LOAD
        PHDRS.append((p_offset, p_vaddr, p_filesz))

def va2off(v):
    for off, vaddr, sz in PHDRS:
        if vaddr <= v < vaddr + sz:
            return off + (v - vaddr)
    return None

va = int(sys.argv[1], 16)
before = int(sys.argv[2], 0) if len(sys.argv) > 2 else 0x40
after = int(sys.argv[3], 0) if len(sys.argv) > 3 else 0xC0
start = va - before
o = va2off(start)
if o is None:
    print("VA 超出范围", [hex(x[1]) for x in PHDRS]); sys.exit(1)
md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
code = data[o:o + before + after]
for ins in md.disasm(code, start):
    mark = " <<<" if ins.address == va else ""
    print(f"0x{ins.address:x}: {ins.mnemonic:<10} {ins.op_str}{mark}")
