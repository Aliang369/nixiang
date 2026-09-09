#!/usr/bin/env python3.11
# 全 so 扫描 BL 指令, 找调用关键函数的调用者
import struct

SO = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/03_decompiled_libil2cpp.so"
d = open(SO, "rb").read()

TARGETS = {
    0x1D9F120: "LoadDLL.Start",
    0x1D9F134: "LoadDLL.LoadHotfix",
    0x1D9F2C4: "LoadDLL.?",
    0x1D9F454: "LoadDLL.DecompressBytes",
    0x1D9F990: "LoadDLL.LoadAssemble",
    0x1D9FB0C: "XXTEA.Decrypt(data,string)",
    0x1DB8BD0: "XXTEA.Decrypt(data,bytes)",
    0x1DB87B4: "XXTEA.Encrypt(bytes,string)",
    0x1DB8720: "XXTEA.Encrypt(string,bytes)",
    0x1DB8700: "XXTEA.Encrypt(bytes,bytes)",
}
found = {v: [] for v in TARGETS.values()}
N = len(d) - 4
for pc in range(0, N, 4):
    insn = struct.unpack_from("<I", d, pc)[0]
    if (insn & 0xFC000000) == 0x94000000:  # BL
        imm = insn & 0x3FFFFFF
        if imm & (1 << 25):
            imm -= 1 << 26
        tgt = pc + imm * 4
        if tgt in TARGETS:
            found[TARGETS[tgt]].append(pc)

for name, sites in found.items():
    print(f"{name}: {len(sites)} callers")
    for pc in sites[:15]:
        print(f"   bl@0x{pc:X}")
