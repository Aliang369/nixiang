import struct, sys

SO = "/tmp/bh3_recon/so_x/lib/arm64-v8a/libil2cpp.so"
RVAS = [0x5F16228, 0x5F156A8, 0x5F16828]

f = open(SO, "rb")
data = f.read()

# ELF64 header
assert data[:4] == b"\x7fELF", "not ELF"
e_phoff = struct.unpack_from("<Q", data, 0x20)[0]
e_phentsize = struct.unpack_from("<H", data, 0x36)[0]
e_phnum = struct.unpack_from("<H", data, 0x38)[0]
print(f"phoff={e_phoff:#x} phentsize={e_phentsize} phnum={e_phnum}")

segs = []
for i in range(e_phnum):
    off = e_phoff + i * e_phentsize
    p_type, p_flags = struct.unpack_from("<II", data, off)
    p_offset, p_vaddr, p_paddr, p_filesz, p_memsz, p_align = struct.unpack_from("<QQQQQQ", data, off + 8)
    segs.append((p_type, p_flags, p_offset, p_vaddr, p_filesz, p_memsz))
    if p_type == 1:  # PT_LOAD
        print(f"PT_LOAD flags={p_flags} off={p_offset:#x} vaddr={p_vaddr:#x} filesz={p_filesz:#x} memsz={p_memsz:#x}")

def vaddr_to_off(v):
    for p_type, p_flags, p_offset, p_vaddr, p_filesz, p_memsz in segs:
        if p_type == 1 and p_vaddr <= v < p_vaddr + p_memsz:
            return p_offset + (v - p_vaddr)
    return None

print("\n=== decode 3 RVAs (as vaddr) ===")
for rva in RVAS:
    off = vaddr_to_off(rva)
    print(f"\nRVA {rva:#x} -> file off {hex(off) if off else None}")
    if off is None:
        continue
    b = data[off:off+32]
    print("  bytes:", b.hex(" "))
    # decode first few arm64 insns
    for k in range(0, 16, 4):
        insn = struct.unpack_from("<I", b, k)[0]
        print(f"    +{k}: {insn:08x}")

# also show raw at file-offset interpretation
print("\n=== raw bytes at FILE OFFSET == rva (no mapping) ===")
for rva in RVAS:
    b = data[rva:rva+16]
    print(f"  off {rva:#x}: {b.hex(' ')}")
