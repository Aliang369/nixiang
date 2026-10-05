#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""bss_refs.py — 扫描一个 so 的可执行段，列出所有对指定 ADRP 页内地址的访问
（用于把「运行期 .bss 配置槽」的读者/写者全量列出来）。

用法:
  python bss_refs.py SO --page 0x130000
"""
import argparse
import collections
import struct

from capstone import CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN, Cs
from capstone.arm64 import ARM64_OP_IMM, ARM64_OP_MEM, ARM64_OP_REG


def elf_exec_segments(d):
    e_phoff = struct.unpack_from("<Q", d, 0x20)[0]
    e_phentsize = struct.unpack_from("<H", d, 0x36)[0]
    e_phnum = struct.unpack_from("<H", d, 0x38)[0]
    segs = []
    for i in range(e_phnum):
        off = e_phoff + i * e_phentsize
        p_type, p_flags = struct.unpack_from("<II", d, off)
        p_offset, p_vaddr, p_paddr, p_filesz, p_memsz, p_align = struct.unpack_from(
            "<QQQQQQ", d, off + 8)
        if p_type == 1 and (p_flags & 1) and p_filesz:
            segs.append((p_offset, p_vaddr, p_filesz))
    return segs


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("so")
    ap.add_argument("--page", type=lambda x: int(x, 0), default=0x130000)
    ap.add_argument("--dump", action="store_true", help="逐条打印（默认只汇总）")
    a = ap.parse_args()
    d = open(a.so, "rb").read()
    md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
    md.detail = True
    md.skipdata = True  # 遇到 ELF 头等非指令字节时跳过而不是停表

    refs = collections.defaultdict(list)
    for p_off, p_vaddr, p_filesz in elf_exec_segments(d):
        page_reg = {}
        for ins in md.disasm(d[p_off:p_off + p_filesz], p_vaddr):
            if ins.id == 0:  # SKIPDATA 伪指令，取 operands 会抛 CS_ERR_SKIPDATA
                continue
            m = ins.mnemonic
            ops = ins.operands
            if m == "adrp" and len(ops) == 2 and ops[0].type == ARM64_OP_REG and ops[1].type == ARM64_OP_IMM:
                page_reg[ops[0].reg] = ops[1].imm
                continue
            if m == "add" and len(ops) == 3 and ops[0].type == ARM64_OP_REG and ops[1].type == ARM64_OP_REG \
                    and ops[2].type == ARM64_OP_IMM and ops[1].reg in page_reg:
                page_reg[ops[0].reg] = page_reg[ops[1].reg] + ops[2].imm
                continue
            if m.startswith(("ldr", "ldrb", "ldrh", "ldrsb", "ldrsh", "ldrsw", "str", "strb", "strh",
                             "ldar", "ldarb", "ldxr", "stxr", "stlr", "ldur", "stur", "ldaxr", "stlxr",
                             "ldp", "stp")):
                for op in ops:
                    if op.type == ARM64_OP_MEM and op.mem.base in page_reg:
                        base = page_reg[op.mem.base]
                        tgt = base + op.mem.disp
                        if a.page - 0x8000 <= tgt < a.page + 0x8000:
                            refs[tgt].append((ins.address, m, ins.op_str))
    print("distinct bss slots referenced: %d" % len(refs))
    for tgt in sorted(refs):
        hits = refs[tgt]
        print("%#x  x%d  %s" % (tgt, len(hits), "; ".join("%s@%#x" % (h[1], h[0]) for h in hits[:6])))
        if a.dump:
            for h in hits:
                print("      %#x  %s" % (h[0], h[2]))


if __name__ == "__main__":
    main()
