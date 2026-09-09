#!/usr/bin/env python3.11
# 反汇编 XXTEA 核心函数, 找 delta 常量
import struct
from capstone import *

SO = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/03_decompiled_libil2cpp.so"
d = open(SO, "rb").read()
cs = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)

for a, b, name in [(0x1DB8FD0, 0x1DB9188, "Encrypt(v,k)"), (0x1DB9188, 0x1DB935C, "Decrypt(v,k)"),
                   (0x1DB940C, 0x1DB951C, "ToUInt32Array"), (0x1DB951C, 0x1DB9628, "ToByteArray"),
                   (0x1DB935C, 0x1DB940C, "FixKey"), (0x1DB8BD0, 0x1DB8BF0, "Decrypt(data,key)"),
                   (0x1D9FB0C, 0x1D9FBA4, "Decrypt(data,string)")]:
    print(f"\n===== {name} 0x{a:X}-0x{b:X} =====")
    for i in cs.disasm(d[a:b], a):
        print(f"  0x{i.address:X}: {i.mnemonic:<7} {i.op_str}")
