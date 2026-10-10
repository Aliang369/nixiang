#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import re, struct

SEG = "/Users/aliang/Documents/nixiang/06_dumped/崩坏3/mem_20261008/bh3_shell/seg_23_rw-p_7349938000.bin"
data = open(SEG, "rb").read()
print("=== seg_23 size=%d ===" % len(data))

# ---- 24-byte record walker over the binding region ----
def asciiz(b):
    z = b.find(b"\x00")
    raw = b if z < 0 else b[:z]
    return raw.decode("latin1", "replace")

START = 0x24c80
END = 0x25200
print("--- 24-byte record walk 0x%x..0x%x ---" % (START, END))
off = START
while off + 24 <= END:
    rec = data[off:off + 24]
    name = asciiz(rec[:16])
    tail = rec[16:24]
    u1, tag = struct.unpack_from("<II", rec, 16)
    printable = all(32 <= c < 127 or c == 0 for c in rec[:16])
    marker = ""
    if tag in (0x66b6, 0x6662):
        marker = "  <== TAG %s" % ("HOOK" if tag == 0x66b6 else "UIKEY")
    if printable and (name or marker):
        print("0x%06x  name=%-16s u1=0x%08x tag=0x%08x %s%s" % (off, repr(name), u1, tag, tail.hex(), marker))
    off += 24

# ---- raw scan: locate every occurrence of the tag dwords ----
print("--- tag byte-pattern occurrences ---")
for pat, lab in [(b"\xb6\x66\x00\x00", "tag HOOK b666"), (b"\x62\x66\x00\x00", "tag UIKEY 6266"),
                 (b"\xc0\x03\x5f\xd6", "RET C0035FD6")]:
    idxs = [m.start() for m in re.finditer(re.escape(pat), data)]
    print("%-16s x%d %s" % (lab, len(idxs), [hex(i) for i in idxs[:12]]))

# ---- locate hook offset dwords ----
print("--- hook offset LE dwords ---")
for V in (0x5F16228, 0x5F156A8, 0x5F16828):
    pat = struct.pack("<I", V)
    idxs = [m.start() for m in re.finditer(re.escape(pat), data)]
    print("0x%08x -> %s" % (V, [hex(i) for i in idxs[:8]]))

# ---- disk shell search ----
SO = "/tmp/bh3_recon/shell/libSDKloading.so"
so = open(SO, "rb").read()
print("--- disk libSDKloading.so pattern search ---")
for pat, lab in [(b"\xb6\x66\x00\x00", "tag HOOK b666"), (b"\x62\x66\x00\x00", "tag UIKEY 6266"),
                 (b"\xc0\x03\x5f\xd6", "RET C0035FD6"), (b"libil2cpp.so", "libil2cpp.so"),
                 (b"0x5F16228", "ascii 0x5F16228")]:
    idxs = [m.start() for m in re.finditer(re.escape(pat), so)]
    print("%-16s x%d %s" % (lab, len(idxs), [hex(i) for i in idxs[:12]]))
for V in (0x5F16228, 0x5F156A8, 0x5F16828):
    pat = struct.pack("<I", V)
    idxs = [m.start() for m in re.finditer(re.escape(pat), so)]
    print("dword 0x%08x -> %s" % (V, [hex(i) for i in idxs[:8]]))
