# -*- coding: utf-8 -*-
"""从 R2e 的 libil2cpp.so 上摘掉 F 点（专注点不消耗），回到 R2d 四点形态。

F 点原改法：0x1F5CA80 的 adds w8,w22,w21 改成 b 0x4720000，cave 里把 delta 夹 >=0。
撤销 = 恢复 adds + 清空 cave（0x44E6000 起 0x40 字节）。
其余 B/C/D/E 四点保持不变。
"""
import struct, sys, zlib
F_OFF = 0x1F5CA80
EXPECT = 0x149F0960        # b 0x4720000（R2e 形态）
ORIG   = 0x2B1502C8        # adds w8, w22, w21
CAVE   = 0x44E6000

# 1) 从 apk 里解 so
import zipfile
apk, out = sys.argv[1], sys.argv[2]
z = zipfile.ZipFile(apk)
info = z.getinfo("lib/arm64-v8a/libil2cpp.so")
f = open(apk, "rb"); f.seek(info.header_offset)
hdr = f.read(30)
import struct as st
nlen, elen = st.unpack_from("<HH", hdr, 26)
f.seek(info.header_offset + 30 + nlen + elen)
d = zlib.decompressobj(-15); buf = bytearray(); rem = info.compress_size
while rem > 0:
    ch = f.read(min(1 << 22, rem)); rem -= len(ch); buf += d.decompress(ch)
buf += d.flush()
print("解出 so", len(buf), "B")

data = bytearray(buf)
got = struct.unpack_from("<I", data, F_OFF)[0]
assert got == EXPECT, f"F 点不是 R2e 形态: {got:#010x}"
struct.pack_into("<I", data, F_OFF, ORIG)
print(f"  F@{F_OFF:#x}: {EXPECT:#010x} -> {ORIG:#010x} (adds w8,w22,w21)")
data[CAVE:CAVE+0x40] = bytes(0x40)
print("  cave 已清零")
open(out, "wb").write(bytes(data))
print("输出", out, len(data))
