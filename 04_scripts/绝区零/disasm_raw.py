#!/usr/bin/env python3
"""Disassemble a raw range at a supplied virtual address."""
import sys

from capstone import CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN, Cs

path = sys.argv[1]
start = int(sys.argv[2], 16)
length = int(sys.argv[3], 16)
va = int(sys.argv[4], 16)
data = open(path, "rb").read()[start:start + length]

md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
for ins in md.disasm(data, va):
    print(f"{ins.address:#x}  {ins.bytes.hex():8s} {ins.mnemonic:8s} {ins.op_str}")
