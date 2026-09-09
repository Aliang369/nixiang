#!/usr/bin/env python3.11
# 修复 MHP 打花的 .NET DLL PE 头: pe_dump_2.bin (yyzy_hotfix) → 可反编译的合法 DLL
# 思路: BSJB 元数据完好 @0x28ad8; 重建 DOS/PE/COFF/Optional + 单节 .text (RVA=off+0x1e00) + 自写 CLI header
import struct, sys

SRC = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/pe_dump_2.bin"
OUT = "/Users/aliang/Documents/nixiang/06_dumped/月圆之夜/yyzy_hotfix_repaired.dll"

d = bytearray(open(SRC, "rb").read())
MD = d.find(b"BSJB")
assert MD > 0, "BSJB 未找到"
print(f"元数据根 @0x{MD:x}")

# 1) 解析 BSJB 总大小
ver_len = struct.unpack_from("<I", d, MD + 12)[0]
p = MD + 16 + ver_len
flags, nstreams = struct.unpack_from("<HH", d, p)
p += 4
streams = {}
for _ in range(nstreams):
    off, sz = struct.unpack_from("<II", d, p)
    e = d.find(b"\x00", p + 8)
    nm = d[p + 8:e].decode()
    streams[nm] = (off, sz)
    p = p + 8 + ((e + 1 - (p + 8) + 3) & ~3)  # 名字按 4 字节对齐
    print(f"  流 {nm}: off=0x{off:x} size=0x{sz:x}")
md_total = max(off + sz for off, sz in streams.values())
print(f"  元数据总大小: 0x{md_total:x}")

DELTA = 0x1E00  # RVA = fileoff + 0x1e00 (.text raw 0x200, RVA 0x2000)
md_rva = MD + DELTA

# 2) CLI header 写在元数据前的零区 (0x28a90)
cli_off = MD - 0x48
cli = struct.pack("<IHHIIIIIIIIII",
    0x48,           # cb
    2, 5,           # runtime 2.5
    md_rva, md_total,
    0x00000001,     # flags: ILONLY (DLL 由 PE characteristics 表达)
    0,              # entrypoint token (库=0)
    0, 0,           # resources
    0, 0,           # strongname
    0, 0)           # vtablefixups / extdata (8+8 字节: 上面 13 个字段覆盖 0x40, 再补 8)
cli += b"\x00" * (0x48 - len(cli))
d[cli_off:cli_off + 0x48] = cli
cli_rva = cli_off + DELTA

# 3) 重建头: DOS(0x40) + pad 到 0x80 + PE + COFF + Optional(0xE0) + 1 节表(0x28)
dos = bytearray(0x40)
dos[0:2] = b"MZ"
struct.pack_into("<I", dos, 0x3c, 0x80)

coff = struct.pack("<HHIIIHH",
    0x014C,   # i386
    1,        # sections
    0, 0, 0,  # timestamp/symtab/numsym
    0xE0,     # sizeof optional
    0x2102)   # DLL|EXECUTABLE_IMAGE|32BIT

filelen = len(d)
text_vsize = filelen  # 全文件都在 .text
text_rva = 0x2000
image_size = (max(text_rva + text_vsize, 0x1000) + 0x1FFF) & ~0x1FFF

opt = struct.pack("<HBBIIIIII",
    0x10B, 8, 0,          # PE32, linker 8.0
    text_vsize, 0, 0,     # code/init/uninit sizes
    0x2000,               # entrypoint RVA (占位, 库无入口)
    0x2000, 0,            # base of code / data
)  # 28B
opt += struct.pack("<III",
    0x400000,             # imagebase
    0x2000, 0x200,        # section/file align
)  # 12B
opt += struct.pack("<HHHHHHI",
    4, 0, 0, 0, 4, 0, 0,  # OS/Image/Subsys versions + Win32Version
)  # 16B
opt += struct.pack("<IIIHH",
    image_size, 0x200, 0, # SizeOfImage / SizeOfHeaders / CheckSum
    3, 0,                 # subsystem CUI, dllchars
)  # 16B
opt += struct.pack("<IIII", 0x100000, 0x1000, 0x100000, 0)  # stack/heap
opt += struct.pack("<I", 0)   # loader flags
opt += struct.pack("<I", 16)  # NumberOfRvaAndSizes
assert len(opt) == 96, f"opt 固定段 {len(opt)}"
dirs = [(0, 0)] * 16
dirs[14] = (cli_rva, 0x48)  # CLR header
for rva, sz in dirs:
    opt += struct.pack("<II", rva, sz)
assert len(opt) == 0xE0, f"optional 头大小 {len(opt)}"

sec = struct.pack("<8sIIIIIIHHI",
    b".text", text_vsize, text_rva, text_vsize,
    0x200,          # pointer to raw data
    0, 0,
    0, 0,
    0x60000020)     # code|exec|read

pe = b"PE\x00\x00" + coff + opt + sec
# 写入: DOS 头 + 0x40-0x7f pad + 0x80 起 PE
d[0:0x40] = dos
d[0x40:0x80] = b"\x00" * 0x40
d[0x80:0x80 + len(pe)] = pe

open(OUT, "wb").write(bytes(d))
print(f"→ {OUT} ({len(d)} B), CLI RVA=0x{cli_rva:x}, mdRVA=0x{md_rva:x}, SizeOfImage=0x{image_size:x}")
