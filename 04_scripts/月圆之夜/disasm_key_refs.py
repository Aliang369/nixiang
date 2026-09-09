#!/usr/bin/env python3.11
# 反汇编密码槽引用点上下文, 找 string.Format 的参数
import struct, re, sys
from capstone import *
from capstone.arm64 import ARM64_OP_MEM

SO = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/03_decompiled_libil2cpp.so"
DUMP = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/dump_out3/dump.cs"
d = open(SO, "rb").read()

rva2name = {}
cur = None
for line in open(DUMP, encoding="utf-8", errors="replace"):
    m = re.match(r"^(?:\S+\s+)*(class|struct|enum|interface)\s+(\S+)", line)
    if m:
        cur = m.group(2)
    mr = re.search(r"// RVA: 0x([0-9A-Fa-f]+)", line)
    if mr:
        rva2name[int(mr.group(1), 16)] = f"{cur}::{line.split('// RVA')[0].strip()}"

REFS = [0x119B34C, 0x119B364, 0x1FAE880, 0x2EBE90C, 0x3963B44]
cs = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
cs.detail = True


def func_start(pc, back=0x800):
    # 向前找最近的函数头: 前一条是 ret 且本条 stp/sub sp 开头, 简化: 直接回退 0x100 反汇编
    a = pc - back
    best = a
    for i in cs.disasm(d[a:pc + 0x40], a):
        if i.mnemonic in ("stp", "sub", "str") and i.address >= a and b"sp, #" in i.op_str and i.address >= pc - back:
            pass
    return a


for ref in REFS:
    a = ref - 0x140
    b = ref + 0x140
    print(f"\n===== ref 0x{ref:X} =====")
    adrp = {}
    for i in cs.disasm(d[a:b], a):
        note = ""
        if i.mnemonic == "adrp" and len(i.operands) == 2:
            try:
                adrp[i.operands[0].reg] = i.operands[1].imm & 0xFFFFFFFFFFFFFFFF
                continue
            except Exception:
                pass
        if i.operands and i.operands[-1].type == ARM64_OP_MEM:
            base = i.operands[-1].mem.base
            if base in adrp:
                va = (adrp[base] + i.operands[-1].mem.disp) & 0xFFFFFFFFFFFFFFFF
                note = f"; @0x{va:X}"
                del adrp[base]
        if i.mnemonic in ("bl", "b") and i.operands[0].type == 2:
            tgt = i.operands[0].imm
            note = f"; -> {rva2name.get(tgt, hex(tgt))}"
        if i.mnemonic == "movz" and i.operands[1].type == 2:
            v = i.operands[1].imm & 0xFFFF
            if v:
                note += f"; imm=0x{v:X}({v})"
        mark = " <<<" if i.address == ref else ""
        print(f"  0x{i.address:X}: {i.mnemonic:<7} {i.op_str}{note}{mark}")
