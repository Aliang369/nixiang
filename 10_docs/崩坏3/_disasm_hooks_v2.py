#!/usr/bin/env python3
import struct
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

SO = "/tmp/bh3_recon/so_x/lib/arm64-v8a/libil2cpp.so"
so = open(SO, "rb").read()

e_shoff = struct.unpack_from("<Q", so, 0x28)[0]
e_shentsize = struct.unpack_from("<H", so, 0x3a)[0]
e_shnum = struct.unpack_from("<H", so, 0x3c)[0]
e_shstrndx = struct.unpack_from("<H", so, 0x3e)[0]

def sh(i):
    o = e_shoff + i * e_shentsize
    name, typ, flags, addr, off, size, link, info, align, entsize = struct.unpack_from("<IIQQQQIIQQ", so, o)
    return dict(name=name, typ=typ, flags=flags, addr=addr, off=off, size=size, entsize=entsize)

strtab = sh(e_shstrndx)
def sname(n):
    e = so.index(b"\0", strtab["off"] + n)
    return so[strtab["off"] + n:e].decode()

sects = {}
for i in range(e_shnum):
    s = sh(i)
    s["sname"] = sname(s["name"])
    sects[s["sname"]] = s

def v2f(v):
    for nm, s in sects.items():
        if s["typ"] != 8 and s["addr"] <= v < s["addr"] + s["size"]:
            return s["off"] + (v - s["addr"]), nm
    return None, None

md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)

def disasm(foff, length, label):
    print(f"\n=== {label}  (file_off {foff:#x} .. {foff+length:#x}) ===")
    b = so[foff:foff + length]
    n = 0
    for ins in md.disasm(b, foff):
        print(f"  {ins.address:#010x}: {ins.mnemonic:<9} {ins.op_str}")
        n += 1
    if n == 0:
        print("  <no decodable instructions>")

for V in (0x5F16228, 0x5F156A8, 0x5F16828):
    foff, nm = v2f(V)
    print("=" * 78)
    print(f"### hook RVA {V:#x}  -> file_off {foff:#x}  section={nm}")
    print(f"    bytes at hook site: {so[foff:foff+16].hex(' ')}")
    # find function prologue scanning backward
    pro = None
    for a in range(foff, foff - 0x3000, -4):
        insn = struct.unpack_from("<I", so, a)[0]
        # stp x29,x30,[sp,#-imm]!   opcode 0xa9??7bfd family
        if (insn & 0xFFC003FF) == 0xA9807BFD or (insn & 0xFFC003FF) == 0xA9007BFD:
            pro = a
            break
        if (insn & 0xFF8003FF) == 0xD10003FF:
            pro = a
            break
        if (insn & 0x7F000000) == 0x14000000:  # b (tail jump to func)
            pro = a
            break
    if pro is not None:
        print(f"    nearest prologue file_off {pro:#x} (delta {foff-pro:#x})")
        disasm(pro, min(foff - pro + 0x60, 0x200), f"prologue->hook {V:#x}")
    else:
        disasm(foff - 0x80, 0x120, f"around hook {V:#x}")
