#!/usr/bin/env python3.11
# 诊断: gapB 各区原始 u32 形态 + C/F 区解密方案试探
import struct

P = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_runtime.dat"
d = open(P, "rb").read()
u32 = lambda o: struct.unpack_from("<I", d, o)[0]

regions = {
    "A gp   ": (0x7E1EC4, 106448), "B gpc  ": (0x7FBE94, 3292),
    "C gc?  ": (0x7FCB70, 58704), "D nt   ": (0x80B0C0, 18228),
    "E if   ": (0x80F7F4, 21624), "F vtm? ": (0x814C6C, 531552),
    "G iof  ": (0x8968CC, 103528),
}
for name, (off, sz) in regions.items():
    n = 12 if sz < 100000 else 10
    vals = [u32(off + i * 4) for i in range(n)]
    print("%s @0x%x: %s" % (name, off, [hex(v) for v in vals]), flush=True)

# C 区逐 entry (16B) 看 4 个 i32
print("\nC 区前 8 entry (ownerIndex, type_argc, is_method, gps):", flush=True)
for i in range(8):
    b = 0x7FCB70 + i * 16
    print("  ", struct.unpack_from("<iiii", d, b), flush=True)

# C 区按字 XOR ((i&0xFF)|0x80) 后的形态
print("\nC 区 byte-XOR 解码后前 8 entry:", flush=True)
base = 0x7FCB70
for i in range(8):
    vals = []
    for j in range(4):
        o = base + i * 16 + j * 4
        v = 0
        for k in range(4):
            c = d[o + k] ^ (((i * 16 + j * 4 + k) & 0xFF) | 0x80)
            v |= c << (8 * k)
        vals.append(struct.unpack("<i", struct.pack("<I", v))[0])
    print("  ", vals, flush=True)

# F 区: 试 4B word-XOR (word 索引) / byte-XOR / word-XOR(表内相对) 后是否 < 98644*8 且低3位=usage
print("\nF 区解码试验 (前 10 word):", flush=True)
F, FSZ = 0x814C6C, 531552
raw = [u32(F + i * 4) for i in range(10)]
print("  raw:", [hex(v) for v in raw], flush=True)
for label, fn in [
    ("byteXor(rel)", lambda i: d[F + i * 4:i * 4 + F + 4] and bytes(d[F + i * 4 + k] ^ (((i * 4 + k) & 0xFF) | 0x80) for k in range(4))),
    ("byteXor(abs)", lambda i: bytes(d[F + i * 4 + k] ^ (((F + i * 4 + k) & 0xFF) | 0x80) for k in range(4))),
]:
    vals = [struct.unpack("<I", fn(i))[0] for i in range(10)]
    ok = sum(1 for v in vals if v < 98644 * 8)
    print("  %s: %s ok=%d/10" % (label, [hex(v) for v in vals], ok), flush=True)

# 全表统计: F 区 raw 值 < 789152 比例 & 每种解码方案全表命中率(抽样5000)
import random
random.seed(1)
sample = random.sample(range(FSZ // 4), 5000)
def ratio(fn):
    ok = 0
    for i in sample:
        v = struct.unpack("<I", fn(i))[0]
        if v < 98644 * 8:
            ok += 1
    return ok / 5000
print("  raw 命中率:", sum(1 for i in sample if u32(F + i * 4) < 98644 * 8) / 5000, flush=True)
print("  byteXor(rel) 命中率:", ratio(lambda i: bytes(d[F + i * 4 + k] ^ (((i * 4 + k) & 0xFF) | 0x80) for k in range(4))), flush=True)
print("  byteXor(abs) 命中率:", ratio(lambda i: bytes(d[F + i * 4 + k] ^ (((F + i * 4 + k) & 0xFF) | 0x80) for k in range(4))), flush=True)

# F 区若按 (i&0xFF)|0x80 以"表起始"为0 — 同 byteXor(rel)。再看低3位分布
dec = [struct.unpack("<I", bytes(d[F + i * 4 + k] ^ (((i * 4 + k) & 0xFF) | 0x80) for k in range(4)))[0] for i in range(20)]
print("  byteXor(rel) 前20:", [(hex(v), v & 7) for v in dec], flush=True)
