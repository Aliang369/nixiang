#!/usr/bin/env python3.11
# 从 il2cpp_mem_full.bin 扫描已解密的热更 DLL PE 镜像 (MZ..PE\0\0..BSJB) 并抠出
import struct, os, sys

MEM = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/il2cpp_mem_full.bin"
OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/hotfix_carved"
os.makedirs(OUT, exist_ok=True)
d = open(MEM, "rb").read()
N = len(d)
print("mem size = %d (0x%x)" % (N, N), flush=True)

hits = []
i = 0
MZ = b"MZ"
while True:
    i = d.find(MZ, i)
    if i < 0:
        break
    if i + 0x40 <= N:
        e_lfanew = struct.unpack_from("<I", d, i + 0x3C)[0]
        if 0x40 <= e_lfanew <= 0x400 and i + e_lfanew + 6 <= N and d[i + e_lfanew:i + e_lfanew + 4] == b"PE\x00\x00":
            # .NET?: 检查数据目录14 (CLR header)
            opt = i + e_lfanew + 24
            magic = struct.unpack_from("<H", d, opt)[0] if opt + 2 <= N else 0
            is_pe32p = magic == 0x20B
            dd = opt + (112 if is_pe32p else 96)   # 数据目录起点
            clr_rva, clr_sz = struct.unpack_from("<II", d, dd + 14 * 8) if dd + 15 * 8 <= N else (0, 0)
            bsjb = d.find(b"BSJB", i, min(i + 0x100000, N)) >= 0
            hits.append((i, e_lfanew, magic, clr_rva, clr_sz, bsjb))
    i += 2

print("MZ+PE hits: %d" % len(hits))
for (off, lf, mg, cr, cs, bs) in hits:
    print("  @0x%X lfanew=0x%X magic=%04X clrRVA=0x%X clrSz=0x%X BSJB_in_1MB=%s" % (off, lf, mg, cr, cs, bs))

# 只抠带 CLR 头或 1MB 内含 BSJB 的
n = 0
for (off, lf, mg, cr, cs, bs) in hits:
    if not (cs > 0 or bs):
        continue
    # 判定 PE 尺寸: 节表计算
    pesig = off + lf
    nsec_off = pesig + 6
    optsz = struct.unpack_from("<H", d, pesig + 20)[0]
    nsec = struct.unpack_from("<H", d, nsec_off)[0]
    sect = pesig + 24 + optsz
    end = sect + nsec * 40
    for s in range(nsec):
        so = sect + s * 40
        if so + 40 > N:
            end = N
            break
        vsz, va, rsz, ra = struct.unpack_from("<IIII", d, so + 8)
        file_end = ra + rsz if rsz else va + vsz
        end = max(end, file_end)
    end = min(end, N - off)
    blob = d[off:off + end]
    name = "pe_%06x_%d.bin" % (off, len(blob))
    open(os.path.join(OUT, name), "wb").write(blob)
    n += 1
    print("carved %s (%d bytes) clr=0x%X bsjb=%s" % (name, len(blob), cs, bs), flush=True)
print("done, carved %d" % n)
