#!/usr/bin/env python3.11
# 验证 XOR 0xAA 解密 + literalData 尺寸闭合 + 生成 v29 头部 metadata
import struct

P = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_runtime.dat"
d = bytearray(open(P, "rb").read())
FS = len(d)

def u32(o):
    return struct.unpack_from("<I", d, o)[0]

NAMES = 0xA23B8
KEY = 0xAA

def getstr(off):
    p = NAMES + off
    e = d.index(b"\x00", p)
    return bytes(c ^ KEY for c in d[p:e]).decode("utf-8", "replace")

print("== XOR 0xAA 解密验证 ==", flush=True)
print("  names[0]  =", getstr(0), flush=True)
print("  names[9]  = %r" % getstr(9), flush=True)
print("  names[10] =", getstr(10), flush=True)
print("  names[29] =", getstr(29), flush=True)
print("  names[42] =", getstr(42), flush=True)
IB = 0x9BF164
for k in range(6):
    print("  image%d = %s" % (k, getstr(u32(IB + k * 40))), flush=True)
print("  image101 = %s" % getstr(u32(IB + 101 * 40)), flush=True)
# 程序集名 (aname.nameIndex)
AB = 0x9C0154
for k in range(5):
    print("  asm%d = %s" % (k, getstr(u32(AB + k * 64 + 16))), flush=True)

TB = 0x8AFD34
print("  typeDef[0..8] name/ns:", [(getstr(u32(TB + i * 88)), getstr(u32(TB + i * 88 + 4))) for i in range(9)], flush=True)

# nameIndex 越界检查
mx = 0
out = 0
for i in range(12626):
    for o in (0, 4):
        v = u32(TB + i * 88 + o)
        if v > 1572388:
            out += 1
        mx = max(mx, v)
print("  names 区间 [0, 0x%x), typeDef name/ns max=%d 越界数=%d" % (0x2227DC - NAMES, mx, out), flush=True)

# literalData 闭合
SL = 0x100
last = u32(SL + 17657 * 8 - 8), u32(SL + 17657 * 8 - 4)
print("  stringLiteral last entry: len=%d dataIdx=%d → data end=%d (0x228C8+0x7FAF0=0xA23B8? %s)" % (
    last[0], last[1], last[0] + last[1], 0x228C8 + 0x7FAF0 == 0xA23B8), flush=True)
print("  literalData 样例:", d[0x228C8:0x228C8 + 60], flush=True)

# 解密 names 区并写回 → 明文 metadata
print("== 生成明文+重排头部 metadata ==", flush=True)
nd = bytearray(d)
for i in range(NAMES, 0x2227DC):
    if nd[i] != 0:
        nd[i] ^= KEY
print("  names 区已解密 (0x%x-0x%x)" % (NAMES, 0x2227DC), flush=True)

# v29 头部字段顺序 (Il2CppDumper MetadataClass.cs)
fields = [
    ("stringLiteral", 0x100, 17657 * 8),
    ("stringLiteralData", 0x228C8, 0x7FAF0),
    ("string", NAMES, 0x2227DC - NAMES),
    ("events", 0x2227DC, 218 * 24),
    ("properties", 0x223C4C, 17381 * 20),
    ("methods", 0x278A30, 98644 * 32),
    ("parameterDefaultValues", 0, 0),
    ("fieldDefaultValues", 0, 0),
    ("fieldAndParameterDefaultValueData", 0, 0),
    ("fieldMarshaledSizes", 0, 0),
    ("parameters", 0x5F39BC, 113689 * 12),
    ("fields", 0x740AE8, 55037 * 12),
    ("genericParameters", 0, 0),
    ("genericParameterConstraints", 0, 0),
    ("genericContainers", 0, 0),
    ("nestedTypes", 0, 0),
    ("interfaces", 0, 0),
    ("vtableMethods", 0, 0),
    ("interfaceOffsets", 0, 0),
    ("typeDefinitions", 0x8AFD34, 12626 * 88),
    ("images", 0x9BF164, 102 * 40),
    ("assemblies", 0x9C0154, 102 * 64),
    ("fieldRefs", 0, 0),
    ("referencedAssemblies", 0, 0),
    ("attributeData", 0, 0),
    ("attributeDataRange", 0, 0),
    ("unresolvedVirtualCallParameterTypes", 0, 0),
    ("unresolvedVirtualCallParameterRanges", 0, 0),
    ("windowsRuntimeTypeNames", 0, 0),
    ("windowsRuntimeStrings", 0, 0),
    ("exportedTypeDefinitions", 0, 0),
]
hdr = struct.pack("<II", 0xFAB11BAF, 29)
for name, off, size in fields:
    hdr += struct.pack("<ii", off, size)
print("  header %d 对, 共 %d 字节" % (len(fields), len(hdr)), flush=True)
nd[0:len(hdr)] = hdr
out = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_v29_dec.dat"
open(out, "wb").write(nd)
print("  写出:", out, flush=True)
