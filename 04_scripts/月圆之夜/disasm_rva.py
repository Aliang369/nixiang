#!/usr/bin/env python3.11
# 反汇编 libil2cpp_arm64.so 指定 RVA 区间，解析 BL 目标
import sys
import capstone

SO = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/libil2cpp_arm64.so"
d = open(SO, "rb").read()

# ELF: file offset == VA for this .so（此前 dumper RVA 即用），如偏移不符再调
start = int(sys.argv[1], 16)
end = int(sys.argv[2], 16)
md = capstone.Cs(capstone.CS_ARCH_ARM64, capstone.CS_MODE_LITTLE_ENDIAN)
md.detail = False
code = d[start:end]
for ins in md.disasm(code, start):
    line = "0x%x:\t%s\t%s" % (ins.address, ins.mnemonic, ins.op_str)
    if ins.mnemonic in ("bl", "b"):
        tgt = int(ins.op_str.replace("#", ""), 16)
        line += "   ; -> 0x%x" % tgt
    print(line)
