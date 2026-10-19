#!/usr/bin/env python3
"""fg_calls.py — 字节级 BL 扫描：统计调用目标 / 找系统调用桩的调用者（只读）"""
import sys, struct, collections
img = open(sys.argv[1],'rb').read()

def bl_targets(buf, base=0):
    out = []
    for i in range(0, len(buf) - 4, 4):
        w = struct.unpack_from('<I', buf, i)[0]
        if (w & 0xFC000000) == 0x94000000:
            imm = w & 0x03FFFFFF
            if imm & 0x02000000: imm -= 0x04000000
            out.append((base + i, base + i + (imm << 2)))
    return out

bls = bl_targets(img)
print('BL count: %d' % len(bls))
tc = collections.Counter(t for _, t in bls)
print('top targets: %s' % [(hex(t), c) for t, c in tc.most_common(15)])

# 桩表范围：0x35B000..0x360000（payload 自带）
callers = [(a, t) for a, t in bls if 0x35b000 <= t < 0x360000]
print('BL -> payload stub table: %d' % len(callers))
for a, t in callers[:40]:
    print('   caller %#x -> stub %#x' % (a, t))

# 找 svc 桩的 4 字节表（每 0x100）
seen = collections.Counter()
for a, t in callers: seen[t] += 1
print('per-stub call counts: %s' % sorted((hex(t), c) for t, c in seen.items())[:20])