#!/usr/bin/env python3.11
# 实测各表长度 + typeDef 计数交叉验证 + assemblies 定位 + names 验证
import struct

P = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_runtime.dat"
d = open(P, "rb").read()
FS = len(d)

def u32(o):
    return struct.unpack_from("<I", d, o)[0] if 0 <= o <= FS - 4 else 0xFFFFFFFF

def u16(o):
    return struct.unpack_from("<H", d, o)[0] if 0 <= o <= FS - 2 else 0xFFFF

def run_len(base, stride, tokoff, hi, limit=300000):
    """从 base 起连续 token 高字节==hi 的条数"""
    n = 0
    while base + (n + 1) * stride + tokoff <= FS and n < limit:
        if (u32(base + n * stride + tokoff) >> 24) != hi:
            break
        n += 1
    return n

print("== 1. names 验证 (S=0x98b23) ==", flush=True)
S = 0x98B23
ib = 0x9BF164
img = []
for k in range(102):
    b = ib + k * 40
    img.append((u32(b), u32(b + 4), u32(b + 8), u32(b + 12)))  # nameIdx, asmIdx, typeStart, typeCount
ok = 0
for ni, ai, ts, tc in img:
    p = S + ni
    e = d.find(b"\x00", p, p + 80)
    name = d[p:e]
    if name.endswith(b".dll"):
        ok += 1
    elif ok < 3:
        print("  非dll名 ni=%d: %r" % (ni, name[:40]), flush=True)
print("  102 image nameIdx → dll 名: %d/102" % ok, flush=True)
# names 终点估计: max(nameIdx)+末串长
mx = max(ni for ni, _, _, _ in img)
p = S + mx
e = d.find(b"\x00", p, p + 100)
print("  max nameIdx=%d → %r ; names 区间 [0x98b23, ~0x%x]" % (mx, d[p:e][:60], e), flush=True)

print("== 2. 表长度实测 ==", flush=True)
methods_base, methods_n = 0x278A2C, run_len(0x278A2C, 32, 24, 0x06)
fields_base, fields_n = 0x740AE8, run_len(0x740AE8, 12, 8, 0x04)
params_base, params_n = 0x5F39BC, run_len(0x5F39BC, 12, 4, 0x08)
props_base, props_n = 0x223C4C, run_len(0x223C4C, 20, 16, 0x17)
events_base, events_n = 0x2227DC, run_len(0x2227DC, 24, 20, 0x14)
print("  methods  @0x%x stride32 n=%d (end 0x%x)" % (methods_base, methods_n, methods_base + methods_n * 32), flush=True)
print("  fields   @0x%x stride12 n=%d (end 0x%x)" % (fields_base, fields_n, fields_base + fields_n * 12), flush=True)
print("  params   @0x%x stride12 n=%d (end 0x%x)" % (params_base, params_n, params_base + params_n * 12), flush=True)
print("  props    @0x%x stride20 n=%d (end 0x%x)" % (props_base, props_n, props_base + props_n * 20), flush=True)
print("  events   @0x%x stride24 n=%d (end 0x%x)" % (events_base, events_n, events_base + events_n * 24), flush=True)

print("== 3. typeDefs 定位 (多段检测) ==", flush=True)
# 收集 0x02000001 链全部命中
pat = struct.pack("<I", 0x02000001)
hits = []
s = 0
while True:
    i = d.find(pat, s, FS)
    if i < 0:
        break
    base = i - 84
    if base >= 0 and all(u32(base + k * 88 + 84) == 0x02000001 + k for k in range(3)):
        hits.append(base)
    s = i + 1
hits.sort()
print("  链命中 %d 处" % len(hits), flush=True)
# 从每个命中向后测段长，看分段结构
segs = []
for h in hits:
    n = run_len(h, 88, 84, 0x02)
    segs.append((h, n))
    if n > 0:
        print("    seg @0x%x len=%d end=0x%x" % (h, n, h + n * 88), flush=True)

print("== 4. assemblies 定位 ==", flush=True)
p0 = struct.pack("<II", 0, 0x20000001)
ahits = []
s = 0
while True:
    i = d.find(p0, s, FS)
    if i < 0:
        break
    base = i
    if all(u32(base + k * 64 + 4) == 0x20000001 + k for k in range(3)):
        ahits.append(base)
    s = i + 1
for h in ahits:
    n = 0
    while u32(h + n * 64 + 4) == 0x20000001 + n:
        n += 1
    print("  assemblies 候选 @0x%x 连续=%d" % (h, n), flush=True)
