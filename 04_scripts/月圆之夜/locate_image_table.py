#!/usr/bin/env python3.11
# 结构相位扫描定位 imageDefs: field1=assemblyIndex 0..101, field0 小名字索引, typeCount 合计≈typeDef 数
import struct

d = open("/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_runtime.dat", "rb").read()
FS = len(d)
def u32(o): return struct.unpack_from("<I", d, o)[0]

MOD_COUNT = 102
HIT = []
for stride in (24, 28, 32, 36, 40, 44):
    base = 0x100
    while base < FS - MOD_COUNT * stride:
        if u32(base + 4) == 0 and u32(base + stride + 4) == 1 and u32(base + 2 * stride + 4) == 2:
            ok = all(u32(base + k * stride + 4) == k for k in (50, 101))
            if ok and all(u32(base + k * stride + 4) == k for k in range(MOD_COUNT)):
                nameok = all(u32(base + k * stride) < 0x30000 for k in range(MOD_COUNT))
                starts = [u32(base + k * stride + 8) for k in range(MOD_COUNT)]
                counts = [u32(base + k * stride + 12) for k in range(MOD_COUNT)]
                mono = all(starts[i] <= starts[i + 1] for i in range(MOD_COUNT - 1))
                tot = sum(counts)
                HIT.append((base, stride, nameok, mono, tot))
                print("HIT base=0x%x stride=%d nameok=%s mono=%s typeCount合计=%d" % (base, stride, nameok, mono, tot), flush=True)
                print("  第一条:", [hex(u32(base + j * 4)) for j in range(stride // 4)], flush=True)
        base += 4

print("done, hits:", len(HIT), flush=True)
