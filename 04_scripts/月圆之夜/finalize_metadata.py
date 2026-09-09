#!/usr/bin/env python3.11
# 终版v2: names 逐串解密(key=(start&0xFF)|0x80) + 全表定位验证 + v29 头 metadata 生成
import struct

P = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_runtime.dat"
OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/global-metadata_v29_dec.dat"
d = bytearray(open(P, "rb").read())
FS = len(d)
u32 = lambda o: struct.unpack_from("<I", d, o)[0]
i32 = lambda o: struct.unpack_from("<i", d, o)[0]
u16 = lambda o: struct.unpack_from("<H", d, o)[0]

SL, SLD, NB = 0x100, 0x228C8, 0xA23B8
NAMES_SZ = 0x2227DC - NB
EV, EVSZ = 0x2227DC, 218 * 24
PR, PRSZ = 0x223C4C, 17381 * 20
ME, MESZ = 0x278A30, 98644 * 32
PA, PASZ = 0x5F39BC, 113689 * 12
FD, FDSZ = 0x740AE8, 55037 * 12
TD, TDSZ, NTYPES, NMETHODS = 0x8AFD34, 12626 * 88, 12626, 98644
IM, IMSZ = 0x9BF164, 102 * 40
AS, ASSZ = 0x9C0154, 102 * 64
ME_E, FD_E = ME + MESZ, FD + FDSZ
assert PA == ME_E + 492812 and TD == FD_E + 843376

# ---- gapB(已全部验证) ----
gapB = dict(genericParameters=(0x7E1EC4, 106448), genericParameterConstraints=(0x7FBE94, 3292),
            genericContainers=(0x7FCB70, 58704), nestedTypes=(0x80B0C0, 18228),
            interfaces=(0x80F7F4, 21624), vtableMethods=(0x814C6C, 531552),
            interfaceOffsets=(0x8968CC, 103528))

# ---- gapA (24排列语义扫描唯一胜出: cover=1.0 fms_ok=0.80 fdv/pdv=7.0) ----
GA = ME_E
ga = dict(parameterDefaultValues=(GA, 24948), fieldDefaultValues=(GA + 24948, 175104),
          fieldAndParameterDefaultValueData=(GA + 24948 + 175104, 240860),
          fieldMarshaledSizes=(GA + 24948 + 175104 + 240860, 51900))

# ---- gapC: fra/ra 已验证; attributeData=u32[38982]数组(非标准字节blob, DumpAttribute=false 绕开);
#      attributeDataRange 游戏为12B{token,start,len} → 转换为 dumper 8B{token,start*4} 追加文件尾 ----
GC0, exp_sz = AS + ASSZ, 4696
fra_sz, ra_sz = 9216, 2144
fra0 = GC0
ra0 = fra0 + fra_sz
RNG0 = ra0 + ra_sz                    # 游戏 12B range 表 {token,start,len}
NE_RNG = 29666                        # = ΣimageDef.customAttributeCount
RNG_SZ = NE_RNG * 12
AD0 = RNG0 + RNG_SZ                   # attributeData = u32[38982] 数组(非标准字节blob)
AD_SZ = 155928                        # = 38982*4, 与 max(start+len)=38982 一致
bad_fra = sum(1 for i in range(fra_sz // 8) if not (0 <= i32(fra0 + i * 8 + 4) < 55037))
bad_ra = sum(1 for i in range(ra_sz // 4) if u32(ra0 + i * 4) >= 102)
# 原始 range 表(12B) 校验: Σcount + token 合法 + 每镜像 token 唯一(dumper dic.Add 要求)
cnt_sum = sum(u32(IM + k * 40 + 36) for k in range(102))
assert cnt_sum == NE_RNG, "ΣcustomAttributeCount=%d != %d" % (cnt_sum, NE_RNG)
tok_bad, dup_tok = 0, 0
for i in range(NE_RNG):
    t, s, l = u32(RNG0 + i * 12), u32(RNG0 + i * 12 + 4), u32(RNG0 + i * 12 + 8)
    if (t >> 24) not in (2, 4, 6, 8, 0x11, 0x14, 0x17, 0x1B, 0x1F, 0x20):
        tok_bad += 1
    if s + l > 38982:
        tok_bad += 1
for k in range(102):
    st = i32(IM + k * 40 + 32)
    c = u32(IM + k * 40 + 36)
    toks = [u32(RNG0 + (st + j) * 12) for j in range(c)]
    dup_tok += len(toks) - len(set(toks))
print("gapC 验证: fieldRefs越界=%d/%d refAsm越界=%d/%d range异常=%d/%d 同镜像重复token=%d"
      % (bad_fra, fra_sz // 8, bad_ra, ra_sz // 4, tok_bad, NE_RNG, dup_tok), flush=True)

# ---- names 逐串解密 ----
nd = bytearray(d)
o = 0
nstr = 0
while o < NAMES_SZ:
    if nd[NB + o] == 0:
        o += 1
        continue
    e = o
    while e < NAMES_SZ and nd[NB + e] != 0:
        e += 1
    K = (o & 0xFF) | 0x80
    for i in range(o, e):
        nd[NB + i] ^= K
    nstr += 1
    o = e + 1
print("names 解密: %d 串" % nstr, flush=True)


def gstr(off):
    p = NB + off
    e = nd.index(b"\x00", p)
    return nd[p:e].decode("utf-8", "replace")


print("names[0]=%r [29]=%r [42]=%r" % (gstr(0), gstr(29), gstr(42)), flush=True)
for k in range(4):
    print("  image%d=%r asm%d=%r" % (k, gstr(u32(IM + k * 40)), k, gstr(u32(AS + k * 64 + 16))), flush=True)
print("  typeDef[0..6]:", [(gstr(u32(TD + i * 88)), gstr(u32(TD + i * 88 + 4))) for i in range(7)], flush=True)
# 每张 image 的类型名样本(识别 BCL vs 游戏程序集)
for k in range(0, 102, 17):
    ts, tc = i32(IM + k * 40 + 8), u32(IM + k * 40 + 12)
    nm = gstr(u32(IM + k * 40))
    if tc:
        print("  img[%d]=%r n=%d: %s" % (k, nm, tc, [gstr(u32(TD + (ts + i) * 88)) for i in range(min(4, tc))]), flush=True)

# Assembly-CSharp 类型样本(游戏逻辑主体)
for k in range(102):
    if gstr(u32(IM + k * 40)) == "Assembly-CSharp.dll":
        ts, tc = i32(IM + k * 40 + 8), u32(IM + k * 40 + 12)
        print("Assembly-CSharp.dll: %d 类, 前20:" % tc, flush=True)
        print("  ", [gstr(u32(TD + (ts + i) * 88)) for i in range(min(20, tc))], flush=True)
        break

# ---- 写 v29 头 ----
fields = [
    ("stringLiteral", SL, 17657 * 8), ("stringLiteralData", SLD, 0x7FAF0),
    ("string", NB, NAMES_SZ), ("events", EV, EVSZ), ("properties", PR, PRSZ),
    ("methods", ME, MESZ), ("parameterDefaultValues", *ga['parameterDefaultValues']),
    ("fieldDefaultValues", *ga['fieldDefaultValues']),
    ("fieldAndParameterDefaultValueData", *ga['fieldAndParameterDefaultValueData']),
    ("fieldMarshaledSizes", *ga['fieldMarshaledSizes']),
    ("parameters", PA, PASZ), ("fields", FD, FDSZ),
    ("genericParameters", *gapB['genericParameters']),
    ("genericParameterConstraints", *gapB['genericParameterConstraints']),
    ("genericContainers", *gapB['genericContainers']), ("nestedTypes", *gapB['nestedTypes']),
    ("interfaces", *gapB['interfaces']), ("vtableMethods", *gapB['vtableMethods']),
    ("interfaceOffsets", *gapB['interfaceOffsets']), ("typeDefinitions", TD, TDSZ),
    ("images", IM, IMSZ), ("assemblies", AS, ASSZ),
    ("fieldRefs", fra0, fra_sz), ("referencedAssemblies", ra0, ra_sz),
    ("attributeData", AD0, AD_SZ),
    ("attributeDataRange", FS, NE_RNG * 8),   # 转换后的 8B 表追加在原文件尾
    ("unresolvedVirtualCallParameterTypes", 0, 0),
    ("unresolvedVirtualCallParameterRanges", 0, 0),
    ("windowsRuntimeTypeNames", 0, 0), ("windowsRuntimeStrings", 0, 0),
    ("exportedTypeDefinitions", 0, 0),
]
# 追加转换后的 8B attributeDataRange {token, start*4} (start=u32 下标→字节偏移)
rng_tab = b"".join(struct.pack("<II", u32(RNG0 + i * 12), u32(RNG0 + i * 12 + 4) * 4)
                   for i in range(NE_RNG))
hdr = struct.pack("<II", 0xFAB11BAF, 29)
for name, off, sz in fields:
    hdr += struct.pack("<ii", off, sz)
assert len(hdr) == 0x100
hdr += rng_tab
assert len(hdr) == 0x100 + NE_RNG * 8
nd[0:0x100] = hdr[:0x100]
nd += hdr[0x100:]
open(OUT, "wb").write(nd)
print("== 写出 %s (头31对 + 追加range表%dB, 总%dB) ==" % (OUT, NE_RNG * 8, len(nd)), flush=True)
