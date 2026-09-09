#!/usr/bin/env python3.11
# libcybrex 解包 ELF 的窗口反汇编工具 (无 section 表, 用文件偏移=VA)
# 用法: /tmp/fridaenv/bin/python3.11 disasm_win.py <so> <va_hex> [before] [after]
import sys
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

path = sys.argv[1]
va = int(sys.argv[2], 16)
before = int(sys.argv[3], 0) if len(sys.argv) > 3 else 0x40
after = int(sys.argv[4], 0) if len(sys.argv) > 4 else 0xC0

data = open(path, "rb").read()
# libcybrex: 第一 LOAD off=va 对齐; 第二三 LOAD 有偏移
PHDRS = [(0x0, 0x0, 0x3e1920), (0x3e1920, 0x3e2920, 0x1e728), (0x400050, 0x402050, 0xeb70)]

def va2off(v):
    for off, vaddr, sz in PHDRS:
        if vaddr <= v < vaddr + sz:
            return off + (v - vaddr)
    return None

md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
md.detail = False
start = va - before
o = va2off(start)
if o is None:
    print("VA 超出范围"); sys.exit(1)
code = data[o:o + before + after]
for ins in md.disasm(code, start):
    mark = " <<<" if ins.address == va else ""
    print(f"0x{ins.address:x}: {ins.mnemonic:<10} {ins.op_str}{mark}")
