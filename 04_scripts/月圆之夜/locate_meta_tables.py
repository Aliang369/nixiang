#!/usr/bin/env python3.11
# 在 runtime metadata 镜像中定位 imageDefs / typeDefs 表并生成 v24 头部补丁
import struct, sys

P = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_runtime.dat"
d = open(P, "rb").read()
FS = len(d)

def u32(o):
    return struct.unpack_from("<I", d, o)[0]

NAMES_OFF, NAMES_SZ = 0x23000, 0x7C000

# 1) 名字区收集 dll 名索引
dll_idx = []
o = NAMES_OFF
end = NAMES_OFF + NAMES_SZ
while o < end:
    e = d.find(b"\x00", o, min(o + 120, end))
    if e < 0:
        break
    seg = d[o:e]
    if seg.endswith(b".dll") and all(32 < c < 127 for c in seg):
        dll_idx.append(o - NAMES_OFF)
    o = e + 1
print("dll 名:", len(dll_idx), "索引范围 0x%x..0x%x" % (dll_idx[0], dll_idx[-1]), flush=True)

# 2) imageDefs: (nameIndex u32, assemblyIndex u32==k) 102 条定长记录
#    用最稀有的几个 nameIndex 定位命中点, 再验证 stride
hits = []
for v in sorted(set(dll_idx))[-4:]:
    pat = struct.pack("<I", v)
    s = 0
    n = 0
    while n < 30:
        i = d.find(pat, s, FS)
        if i < 0:
            break
        # 必须落在合理表头偏移范围(表区 0x100..NAMES_OFF 通常)
        hits.append(i)
        s = i + 1
        n += 1
print("稀有名字索引命中点:", [hex(x) for x in hits], flush=True)

found = None
for base_hint in hits:
    for stride in (24, 28, 32, 36, 40):
        # 找到该名字在 image 表中的 k: 需要 base + k*stride = base_hint, base ≡ 0 (名字)
        # 直接假设命中点就是某条记录的 field0
        base = base_hint
        ok = True
        for k in range(102):
            ai = u32(base + k * stride + 4)
            ni = u32(base + k * stride)
            if ai != k or ni > NAMES_SZ:
                ok = False
                break
        if ok:
            found = (base, stride)
            break
    if found:
        break

if not found:
    print("image 表未直接命中, 尝试前向对齐搜索...", flush=True)
    # 从命中点向前找 field1==0 的位置
    for h in hits:
        for back in range(0, 48, 4):
            base = h - back
            if u32(base + 4) != 0:
                continue
            for stride in (28, 32, 36, 40):
                ok = all(u32(base + k * stride + 4) == k and u32(base + k * stride) < NAMES_SZ for k in range(102))
                if ok:
                    found = (base, stride)
                    break
            if found:
                break
        if found:
            break

if found:
    base, stride = found
    tott = sum(u32(base + k * stride + 12) for k in range(102))
    print("✓ imageDefs 表 @0x%x stride=%d size=0x%x typeCount合计=%d" % (base, stride, stride * 102, tott), flush=True)
    print("  第一条字段:", [hex(u32(base + j * 4)) for j in range(stride // 4)], flush=True)
    print("  末条字段:", [hex(u32(base + 101 * stride + j * 4)) for j in range(stride // 4)], flush=True)
else:
    print("✗ imageDefs 未定位", flush=True)
