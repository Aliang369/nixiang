#!/usr/bin/env python3
import struct, os
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

SEG = "/Users/aliang/Documents/nixiang/06_dumped/崩坏3/mem_20261008/bh3_shell/seg_23_rw-p_7349938000.bin"
data = open(SEG, "rb").read()

print("=== hexdump 0x24cc0 .. 0x24f60 ===")
for base in range(0x24cc0, 0x24f60, 16):
    chunk = data[base:base+16]
    hexs = " ".join(f"{b:02x}" for b in chunk)
    asc = "".join(chr(b) if 0x20 <= b < 0x7f else "." for b in chunk)
    print(f"{base:06x}  {hexs:<47}  {asc}")

print("\n=== hexdump 0x24f20 .. 0x25140 ===")
for base in range(0x24f20, 0x25140, 16):
    chunk = data[base:base+16]
    hexs = " ".join(f"{b:02x}" for b in chunk)
    asc = "".join(chr(b) if 0x20 <= b < 0x7f else "." for b in chunk)
    print(f"{base:06x}  {hexs:<47}  {asc}")

# ELF segment map of libil2cpp.so
SO = "/tmp/bh3_recon/so_x/lib/arm64-v8a/libil2cpp.so"
so = open(SO, "rb").read()
e_phoff = struct.unpack_from("<Q", so, 0x20)[0]
e_phentsize = struct.unpack_from("<H", so, 0x36)[0]
e_phnum = struct.unpack_from("<H", so, 0x38)[0]
segs = []
for i in range(e_phnum):
    off = e_phoff + i * e_phentsize
    p_type, p_flags = struct.unpack_from("<II", so, off)
    p_offset, p_vaddr, p_paddr, p_filesz, p_memsz, p_align = struct.unpack_from("<QQQQQQ", so, off + 8)
    if p_type == 1:
        segs.append((p_flags, p_offset, p_vaddr, p_filesz, p_memsz))

def vaddr_to_off(v):
    for fl, o, va, fs, ms in segs:
        if va <= v < va + ms:
            return o + (v - va)
    return None

md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)

RVAS = [0x5F16228, 0x5F156A8, 0x5F16828]
for rva in RVAS:
    print("=" * 70)
    print(f"VALUE = {rva:#x}")
    for label, off in (("as FILE OFFSET", rva), ("as VADDR", vaddr_to_off(rva))):
        if off is None:
            print(f"  [{label}] no mapping")
            continue
        b = so[off:off+64]
        print(f"  [{label}] file_off={off:#x}  bytes={b[:16].hex(' ')}")
        for ins in md.disasm(b, 0):
            print(f"      {ins.address:#06x}: {ins.mnemonic:<8} {ins.op_str}")
            if ins.address > 0x30:
                break
