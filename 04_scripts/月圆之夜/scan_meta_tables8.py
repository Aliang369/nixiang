#!/usr/bin/env python3.11
# names 基址超强约束扫描(prev byte=0) + XOR key 恢复
import struct

d = open("/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_runtime.dat", "rb").read()
FS = len(d)

def u32(o):
    return struct.unpack_from("<I", d, o)[0] if 0 <= o <= FS - 4 else 0xFFFFFFFF

TB, IB = 0x8AFD34, 0x9BF164
img_ni = [u32(IB + k * 40) for k in range(102)]
# 全部 12626 个 typeDef nameIndex + namespaceIndex (不抽样!)
td_all = [(u32(TB + i * 88), u32(TB + i * 88 + 4)) for i in range(12626)]
ni_all = [x for pair in td_all for x in pair]
print("typeDef nameIndex/ns 总数 %d, min=%d max=%d" % (len(ni_all), min(ni_all), max(ni_all)), flush=True)

print("== 强约束扫描: nameIndex 前字节=0 ==", flush=True)
# 预计算: 位置 p 是否"串首"(d[p-1]==0, d[p]!=0) — 用位图加速
first = bytearray(FS)
for p in range(1, FS):
    if d[p - 1] == 0 and d[p] != 0:
        first[p] = 1

# 对候选 B 评分: 抽 2000 个 nameIndex 检查 first[B+ni]
import random
random.seed(7)
sample = random.sample(ni_all, 2000)
best = []
for B in range(0x100, FS - max(ni_all) - 2, 1):
    pass  # 全扫太慢, 先缩小: 用 image0 nameIdx=29 的约束 d[B+8]==0,d[B+9]==0

# 缩小: d[B+8]==0 (mscorlib 串尾) 且 d[B+9]==0 (空串) 且 d[B+28]==0, d[B+41]==0, d[B+50]==0
cands = []
for B in range(0x100, FS - 2048576):
    if d[B + 8] == 0 and d[B + 9] == 0 and d[B + 28] == 0 and d[B + 41] == 0 and d[B + 50] == 0:
        cands.append(B)
print("  缩小后候选 %d" % len(cands), flush=True)

res = []
for B in cands:
    ok = sum(1 for ni in sample if B + ni < FS and first[B + ni])
    res.append((ok, B))
res.sort(reverse=True)
for ok, B in res[:10]:
    okimg = sum(1 for ni in img_ni if first[B + ni])
    # XOR 测试: names[42] 应为 <Module> (8B)
    seg = d[B + 42:B + 50]
    keys = set(seg[j] ^ b"<Module>"[j] for j in range(8))
    print("  B=0x%x: typeDef first=%d/2000 img=%d/102 xorkeys=%s" % (B, ok, okimg, [hex(k) for k in sorted(keys)]), flush=True)
