#!/usr/bin/env python3
"""libsecret.so 自检旁路 + 签名开关 + 摘要重算（一次到位）
1) .data:0x660cd 1->0（签名路径切 PackageManager）
2) .text:0x55754 cbz -> b 0x55784（跳过"摘要不符"错误分支）
3) .text:0x557d0 cset w0,ne -> movz w0,#0（函数恒返回"通过"）
4) 重算 .text 段 SHA-256 写回 .rodata:0x17910
"""
import hashlib, struct, sys
from pathlib import Path

src = Path(sys.argv[1]); dst = Path(sys.argv[2])
d = bytearray(src.read_bytes())
print("in ", len(d), hashlib.sha256(d).hexdigest()[:16])

# 1) 签名开关
i = d.find(b"SCFGBLOB01")
assert i > 0, "SCFGBLOB01 not found"
base = i - 4
sw = base + 0xBD
print("switch@0x%x old=%d" % (sw, d[sw]))
d[sw] = 0

# 2) 0x55754: cbz w19,0x55784  ->  b 0x55784
def put(va, insn):
    old = struct.unpack_from("<I", d, va)[0]
    struct.pack_into("<I", d, va, insn)
    print("patch 0x%x: 0x%08x -> 0x%08x" % (va, old, insn))
put(0x55754, 0x1400000C)      # b 0x55784
put(0x557D0, 0x52800000)      # movz w0, #0

# 3) 重算 .text 摘要
start = struct.unpack_from("<Q", d, 0x17900)[0]
length = struct.unpack_from("<Q", d, 0x17908)[0]
print("text range va=0x%x len=0x%x" % (start, length))
digest = hashlib.sha256(bytes(d[start:start + length])).digest()
old = bytes(d[0x17910:0x17910 + 32])
d[0x17910:0x17910 + 32] = digest
print("digest old", old.hex()[:16], "-> new", digest.hex()[:16])
dst.write_bytes(bytes(d))
print("out", dst, hashlib.sha256(bytes(d)).hexdigest())
