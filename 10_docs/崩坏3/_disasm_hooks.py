#!/usr/bin/env python3
import struct
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN

SO = "/tmp/bh3_recon/so_x/lib/arm64-v8a/libil2cpp.so"
so = open(SO, "rb").read()

# ---- ELF section headers ----
e_shoff = struct.unpack_from("<Q", so, 0x28)[0]
e_shentsize = struct.unpack_from("<H", so, 0x3a)[0]
e_shnum = struct.unpack_from("<H", so, 0x3c)[0]
e_shstrndx = struct.unpack_from("<H", so, 0x3e)[0]
def sh(i):
    o = e_shoff + i * e_shentsize
    name, typ, flags, addr, off, size, link, info, align, entsize = struct.unpack_from("<IIQQQQIIQQ", so, o)
    return dict(name=name, typ=typ, flags=flags, addr=addr, off=off, size=size, link=link, info=info, entsize=entsize)
strtab = sh(e_shstrndx)
def sname(n):
    e = so.index(b"\0", strtab["off"] + n)
    return so[strtab["off"] + n:e].decode()
print("=== sections ===")
for i in range(e_shnum):
    s = sh(i)
    print(f"  [{i:2}] {sname(s['name']):20} type={s['typ']:2} addr={s['addr']:#012x} off={s['off']:#012x} size={s['size']:#x} entsize={s['entsize']}")

# ---- symbol tables ----
print("\n=== symbols mentioning Il2Cpp / metadata ===")
for i in range(e_shnum):
    s = sh(i)
    nm = sname(s["name"])
    if nm in (".symtab", ".dynsym"):
        strsh = sh(s["link"])
        for k in range(s["size"] // s["entsize"]):
            o = s["off"] + k * s["entsize"]
            st_name, st_info, st_other, st_shndx, st_value, st_size = struct.unpack_from("<IBBHQQ", so, o)
            e = so.index(b"\0", strsh["off"] + st_name)
            n = so[strsh["off"] + st_name:e].decode("latin1")
            if "Il2Cpp" in n or "il2cpp" in n or "CodeRegistration" in n or "MetadataRegistration" in n:
                print(f"  {nm}: {n} value={st_value:#x} size={st_size:#x}")

# ---- disasm windows ----
md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)
def disasm(start, length, label):
    print(f"\n=== {label}: file_off {start:#x} .. {start+length:#x} ===")
    b = so[start:start+length]
    for ins in md.disasm(b, start):
        print(f"  {ins.address:#010x}: {ins.mnemonic:<9} {ins.op_str}")

# find nearest prologue backward
def find_prologue(off, back=0x4000):
    b = so[off-back:off+4]
    prologues = []
    for i in range(0, len(b)-4, 4):
        insn = struct.unpack_from("<I", b, i)[0]
        a = off - back + i
        # stp x29, x30, [sp, #-imm]!  = 0xa9??????  (opcode 1010100110)
        if (insn & 0xFFC003FF) == 0xA9807BFD:  # stp x29,x30,[sp,#-imm]! variant
            prologues.append((a, insn))
        # sub sp, sp, #imm  = 0xd10???
        elif (insn & 0xFF8003FF) == 0xD10003FF:
            prologues.append((a, insn))
    return prologues[-6:]

for V in (0x5F16228, 0x5F156A8, 0x5F16828):
    print("=" * 78)
    print(f"### value {V:#x}")
    print("  nearest prologues (as file offsets):")
    for a, insn in find_prologue(V):
        print(f"    {a:#x}  insn={insn:#010x}  delta={V-a:#x}")
    disasm(V - 0x40, 0x120, f"{V:#x} window (file-offset interp)")
