#!/usr/bin/env python3.11
# 1) 原始 header 全 pair dump; 2) names 区 per-string key 暴力破译 + 模式分析
import struct

P = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_runtime.dat"
d = open(P, "rb").read()
FS = len(d)

print("== 1) 原始 header ==", flush=True)
print("sanity=0x%08x version=%d" % struct.unpack_from("<II", d, 0), flush=True)
for i in range(31):
    off, size = struct.unpack_from("<ii", d, 8 + i * 8)
    print("  pair[%02d] off=0x%08x size=0x%08x (%d)" % (i, off, size, size), flush=True)

B = 0xA23B8
print("== 2) names 原始 hex ==", flush=True)
for row in range(8):
    o = B + row * 16
    print("  +0x%03x: %s" % (row * 16, d[o:o + 16].hex(" ")), flush=True)

print("== 3) typeDef name/ns per-string key 破译 ==", flush=True)
TB = 0x8AFD34
NT = 12626


def u32(o):
    return struct.unpack_from("<I", d, o)[0]


idxs = set()
for i in range(NT):
    idxs.add(u32(TB + i * 88))
    idxs.add(u32(TB + i * 88 + 4))
offs = sorted(idxs)
ALLOWED = set(b"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_.<>`()+/[]-")
res = []
amb = 0
for off in offs:
    p = B + off
    e = d.index(b"\x00", p)
    ln = e - p
    if ln < 4:
        continue
    seg = d[p:e]
    cands = [k for k in range(256) if all((b ^ k) in ALLOWED for b in seg)]
    if len(cands) == 1:
        k = cands[0]
        res.append((off, ln, k, bytes(b ^ k for b in seg).decode()))
    elif len(cands) > 1:
        amb += 1
print("  唯一key串=%d 歧义串=%d (总idx=%d)" % (len(res), amb, len(offs)), flush=True)
for off, ln, k, s in res[:36]:
    print("  off=%7d len=%3d key=0x%02x str=%r" % (off, ln, k, s), flush=True)

print("== 4) key 模式 ==", flush=True)
for off, ln, k, s in res[:20]:
    print("  off=%7d key=0x%02x off&ff=0x%02x k^off=0x%02x k-(off&ff)=0x%02x k+off=0x%02x"
          % (off, k, off & 0xFF, k ^ (off & 0xFF), (k - (off & 0xFF)) & 0xFF, (k + off) & 0xFF), flush=True)

print("== 5) image/asm 名候选 ==", flush=True)
IB, AB = 0x9BF164, 0x9C0154
for k in range(8):
    ni = u32(IB + k * 40)
    p = B + ni
    e = d.index(b"\x00", p)
    seg = d[p:e]
    cands = [(kk, bytes(b ^ kk for b in seg)) for kk in range(256)
             if all((b ^ kk) in ALLOWED or b == 0 for b in seg)]
    print("  image%d ni=%d cands=%s" % (k, ni, [(hex(kk), s[:24]) for kk, s in cands[:6]]), flush=True)
for k in range(8):
    ni = u32(AB + k * 64 + 16)
    p = B + ni
    e = d.index(b"\x00", p)
    seg = d[p:e]
    cands = [(kk, bytes(b ^ kk for b in seg)) for kk in range(256)
             if all((b ^ kk) in ALLOWED or b == 0 for b in seg)]
    print("  asm%d ni=%d cands=%s" % (k, ni, [(hex(kk), s[:24]) for kk, s in cands[:6]]), flush=True)
