#!/usr/bin/env python3
"""Find code xrefs (ADRP+ADD / ADRP+LDR) to given target addresses in an ELF .text."""
import sys

from capstone import CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN, Cs
from elftools.elf.elffile import ELFFile

path = sys.argv[1]
targets = {int(t, 16) for t in sys.argv[2:]}
elf = ELFFile(open(path, "rb"))
text = elf.get_section_by_name(".text")
data = text.data()
base = text.header["sh_addr"]

md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
md.detail = True
page = {}
for ins in md.disasm(data, base):
    try:
        ops = ins.operands
        if ins.mnemonic == "adrp" and len(ops) == 2:
            page[ops[0].reg] = ops[1].imm
        elif ins.mnemonic == "add" and len(ops) == 3 and ops[2].type == 2:  # CS_OP_IMM
            p = page.get(ops[1].reg)
            if p is not None:
                t = p + ops[2].imm
                if t in targets:
                    print(f"{ins.address:#08x}  add -> {t:#x}")
        elif ins.mnemonic == "ldr" and len(ops) == 2 and ops[1].type == 3:  # CS_OP_MEM
            mem = ops[1].mem
            p = page.get(mem.base)
            if p is not None:
                t = p + mem.disp
                if t in targets:
                    print(f"{ins.address:#08x}  ldr -> {t:#x}")
    except Exception:
        pass
