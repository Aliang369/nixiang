#!/usr/bin/env python3
"""Disassemble a range of an ELF with ADRP/ADD/LDR string annotation."""
import re
import struct
import sys

from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN, CS_OP_IMM, CS_OP_MEM

path = sys.argv[1]
start = int(sys.argv[2], 16)
length = int(sys.argv[3], 16)
data = open(path, "rb").read()

md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
md.detail = True

insns = list(md.disasm(data[start:start + length], start))
reg_page = {}

for ins in insns:
    comment = ""
    mnem = ins.mnemonic
    ops = ins.op_str
    try:
        if mnem == "adrp" and len(ins.operands) == 2 and ins.operands[1].type == CS_OP_IMM:
            reg_page[ins.operands[0].reg] = ins.operands[1].imm  # capstone prints absolute page
        elif mnem == "add" and len(ins.operands) == 3 and ins.operands[2].type == CS_OP_IMM:
            base = reg_page.get(ins.operands[1].reg)
            if base is not None:
                tgt = base + ins.operands[2].imm
                comment = f"-> {tgt:#x}"
                if 0 <= tgt < len(data):
                    raw = data[tgt:tgt + 80].split(b"\x00")[0]
                    try:
                        txt = raw.decode("utf-8")
                    except UnicodeDecodeError:
                        txt = raw.decode("latin1")
                    if re.fullmatch(r"[\x20-\x7e\u3000-\u9fff\uff00-\uffef]*", txt) and txt:
                        comment += f' "{txt}"'
        elif mnem.startswith("ldr") and len(ins.operands) >= 2:
            mem = None
            for op in ins.operands:
                if op.type == CS_OP_MEM:
                    mem = op.mem
            if mem is not None and mem.base == 0:  # RIP-relative style literal load
                tgt = ins.address + mem.disp
                comment = f"-> [{tgt:#x}]"
                if 0 <= tgt < len(data) - 8:
                    val = struct.unpack_from("<Q", data, tgt)[0]
                    comment += f" = {val:#x}"
                    if 0 <= val < len(data):
                        raw = data[val:val + 60].split(b"\x00")[0]
                        try:
                            comment += f' "{raw.decode("utf-8")}"'
                        except UnicodeDecodeError:
                            pass
    except Exception:
        pass
    print(f"{ins.address:#08x}  {ins.bytes.hex():8s} {mnem:8s} {ops:40s} {comment}")
