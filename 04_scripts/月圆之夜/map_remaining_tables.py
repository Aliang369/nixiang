#!/usr/bin/env python3.11
# 指认 runtime 头部 31 对 (off,size) 的 v29 语义，重建完整头部
import struct

RT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_runtime.dat"
d = open(RT, "rb").read()
FS = len(d)

pairs = []
for i in range(31):
    off, size = struct.unpack_from("<ii", d, 8 + i * 8)
    pairs.append((i, off, size))

print("文件大小: 0x%x (%d)" % (FS, FS))
print("runtime 头部 31 对:")
for i, off, size in pairs:
    print("  pair%2d  off=0x%07x size=%9d (0x%x)  end=0x%07x" % (i, off, size, size, off + size))

# 已知映射（由 build_v29_metadata.py 验证过）
KNOWN = {
    0x100:     ("stringLiteral", 17657 * 8, 8),
    0x228C8:   ("stringLiteralData", 0x7FAF0, None),
    0xA23B8:   ("string", 0x2227DC - 0xA23B8, None),
    0x2227DC:  ("events", 218 * 24, 24),
    0x223C4C:  ("properties", 17381 * 20, 20),
    0x278A30:  ("methods", 98644 * 32, 32),
    0x5F39BC:  ("parameters", 113689 * 12, 12),
    0x740AE8:  ("fields", 55037 * 12, 12),
    0x8AFD34:  ("typeDefinitions", 12626 * 88, 88),
    0x9BF164:  ("images", 102 * 40, 40),
    0x9C0154:  ("assemblies", 102 * 64, 64),
}

def u32(o):
    return struct.unpack_from("<I", d, o)[0]

def s32(o):
    return struct.unpack_from("<i", d, o)[0]

NTYPES, NMETHODS, NFIELDS, NPARAMS = 12626, 98644, 55037, 113689

print("\n== 未指认对的分析 ==")
unmatched = []
for i, off, size in pairs:
    if off in KNOWN:
        nm, esz, stride = KNOWN[off]
        print("  pair%2d -> %-20s (已知) size/expect=%d/%d %s" % (
            i, nm, size, esz, "✓" if size == esz else "✗尺寸不符"))
        continue
    unmatched.append((i, off, size))
    if size == 0:
        print("  pair%2d off=0x%07x size=0 (空表)" % (i, off))
        continue
    notes = []
    for stride in (4, 8, 12, 16, 20, 24):
        if size % stride == 0:
            cnt = size // stride
            notes.append("%dB×%d" % (stride, cnt))
    # 4B 值域抽样
    if size % 4 == 0 and size >= 16:
        vals = [s32(off + 4 * k) for k in range(min(size // 4, 8))]
        in_types = sum(1 for v in vals if 0 <= v < NTYPES)
        notes.append("4B首值=%s" % (vals[:6],))
    print("  pair%2d off=0x%07x size=%-9d %s" % (i, off, size, " | ".join(notes)))

# 区域重叠检查
print("\n== 区域覆盖图 ==")
segs = sorted((off, off + size, i) for i, off, size in pairs if size > 0)
prev_end = 0
for s, e, i in segs:
    gap = s - prev_end
    if gap > 0:
        print("  GAP 0x%07x..0x%07x (%d B)" % (prev_end, s, gap))
    print("  pair%2d 0x%07x..0x%07x" % (i, s, e))
    prev_end = max(prev_end, e)
if prev_end < FS:
    print("  GAP 0x%07x..EOF (%d B)" % (prev_end, FS - prev_end))
