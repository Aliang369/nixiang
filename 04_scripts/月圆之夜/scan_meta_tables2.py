#!/usr/bin/env python3.11
# 综合内容扫描: 在 runtime metadata 中按 token 链定位所有 v29 表
import struct

P = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_runtime.dat"
d = open(P, "rb").read()
FS = len(d)
print("FS=0x%x (%d)" % (FS, FS), flush=True)

def u32(o):
    return struct.unpack_from("<I", d, o)[0] if 0 <= o <= FS - 4 else 0xFFFFFFFF

def scan_chain(tok0, stride, tokoff, n=3, maxhits=20):
    """找连续 n 条记录 token 递增的表基址"""
    pat = struct.pack("<I", tok0)
    hits = []
    s = 0
    while len(hits) < maxhits:
        i = d.find(pat, s, FS)
        if i < 0:
            break
        base = i - tokoff
        if base >= 0 and base + n * stride <= FS:
            if all(u32(base + k * stride + tokoff) == (tok0 + k) & 0xFFFFFFFF for k in range(n)):
                hits.append(base)
        s = i + 1
    return hits

print("== token 链扫描 ==", flush=True)
candidates = [
    # name, token_base, stride, token_off, strides_to_try
    ("typeDefs", 0x02000001, 88, 84, [88, 92, 96, 104]),
    ("methods", 0x06000001, 32, 24, [32, 36, 40, 44]),
    ("fields", 0x04000001, 12, 8, [12, 16]),
    ("params", 0x08000001, 12, 4, [12, 16]),
    ("properties", 0x17000001, 20, 16, [20, 16, 24]),
    ("events", 0x14000001, 24, 20, [24, 28, 32]),
    ("assemblies", 0x20000001, 64, 4, [64, 60, 56, 52, 48]),
]
results = {}
for name, tok0, stride0, tokoff, strides in candidates:
    for st in strides:
        hits = scan_chain(tok0, st, tokoff)
        if hits:
            print("%-11s stride=%-3d tokoff=%-3d hits: %s" % (name, st, tokoff, [hex(h) for h in hits[:8]]), flush=True)
            results[name] = (hits[0], st, tokoff)
            break
    else:
        print("%-11s 未命中" % name, flush=True)

# images 已知 @0x9bf164 stride40 — 复核
ib = 0x9BF164
print("images @0x9bf164:", [u32(ib + k * 40 + 4) for k in (0, 1, 101)], flush=True)

# 名字表基址: mscorlib.dll (image0 nameIdx=29) 等
print("== dll 名与名字表基址 ==", flush=True)
dlls = []
s = 0
while True:
    i = d.find(b".dll", s, FS)
    if i < 0:
        break
    j = i
    while j > 0 and 33 <= d[j - 1] < 127:
        j -= 1
    dlls.append((j, i + 4))
    s = i + 1
print("*.dll 串个数:", len(dlls), "样例:", [d[a:b].decode() for a, b in dlls[:5]], flush=True)

# image1 nameIdx=253252, image2 nameIdx=254402 (来自 0x9bf164 记录)
ni1 = u32(ib + 40)
ni2 = u32(ib + 80)
img_nameidx = [u32(ib + k * 40) for k in range(102)]
print("image nameIdx 样例:", img_nameidx[:3], "max:", max(img_nameidx), flush=True)
starts = [a for a, b in dlls]
S = starts[0] - 29  # image0 nameIdx=29
ok = all((S + n) in starts for n in (ni1, ni2)) if ni1 else False
print("候选 names 基址 0x%x (由 mscorlib.dll@0x%x - 29), image1/2 nameIdx 校验: %s" % (S, starts[0], ok), flush=True)
if ok:
    all_img = all((S + n) in starts for n in img_nameidx)
    print("全部 102 个 image nameIdx 均落在 dll 名起点: %s" % all_img, flush=True)
    # names 表边界: 名字区向前找第一个串
    print("names 首串: %r" % d[S:S + 40].split(b"\x00")[0], flush=True)

# 头部 pair 内容抽查 (pair E=0x6dc1b8, pair F=0x160)
print("== 头部 pair 区域抽查 ==", flush=True)
def show(off, n=6, label=""):
    print("%s @0x%x: %s" % (label, off, [hex(u32(off + k * 4)) for k in range(n)]), flush=True)

show(0x6DC1B8, 6, "pairE(0x6dc1b8)")   # 疑似 stringLiterals?
show(0x160, 6, "pairF(0x160)")          # 疑似 literalData?
show(0xA3B568, 6, "pairA(0xa3b568)")
