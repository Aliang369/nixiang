#!/usr/bin/env python3
"""Find candidate (rva, 16-byte fingerprint) pairs inside libcymod and verify
them against libil2cpp's on-disk bytes."""
import struct
import sys

from capstone import Cs, CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN
from elftools.elf.elffile import ELFFile

cymod = open(sys.argv[1], "rb").read()
il2cpp_path = sys.argv[2]
il2 = ELFFile(open(il2cpp_path, "rb"))

# build (vaddr -> file offset) segments for il2cpp
segs = []
for ph in il2.iter_segments():
    if ph["p_type"] == "PT_LOAD":
        segs.append((ph["p_vaddr"], ph["p_offset"], ph["p_filesz"]))


def v2o(va):
    for v, o, sz in segs:
        if v <= va < v + sz:
            return o + (va - v)
    return None


il = open(il2cpp_path, "rb")
md = Cs(CS_ARCH_ARM64, CS_MODE_LITTLE_ENDIAN)

print("=== candidate tables ===")
seen = set()
for off in range(0, len(cymod) - 24, 8):
    q = struct.unpack_from("<Q", cymod, off)[0]
    if not (0x40000 < q < 0x1D2F2C268):
        continue
    fp = cymod[off + 8:off + 24]
    if len(fp) < 16:
        continue
    # all four instructions must decode cleanly
    insns = list(md.disasm(fp, q))
    if len(insns) != 4:
        continue
    if any(i.mnemonic in ("udf", ".byte") for i in insns):
        continue
    fo = v2o(q)
    if fo is None:
        continue
    il.seek(fo)
    real = il.read(16)
    match = real == fp
    key = (q, fp)
    if key in seen:
        continue
    seen.add(key)
    tag = "MATCH " if match else "differ"
    print(f"{tag} table_off={off:#08x} rva={q:#x} file_off={fo:#x}")
    for i in insns:
        print(f"    {i.address:#x}: {i.mnemonic} {i.op_str}")
    if not match:
        print(f"    file: {real.hex()}")
        print(f"    cymod:{fp.hex()}")
