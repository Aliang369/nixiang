#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import struct
SEG = "/Users/aliang/Documents/nixiang/06_dumped/崩坏3/mem_20261008/bh3_shell/seg_23_rw-p_7349938000.bin"
d = open(SEG, "rb").read()
print("seg_23 size = %d (0x%x)" % (len(d), len(d)))

def asc(b):
    return "".join(chr(c) if 32 <= c < 127 else "." for c in b)

def cjk(b):
    try:
        s = b.decode("utf-8", "ignore")
        return s
    except Exception:
        return ""

TAG_HOOK = 0x000066b6
TAG_UIKEY = 0x00006662

# scan whole segment for 24-byte-aligned records whose u32@16 looks like a tag
print("\n=== records with u32@+16 in {0x101, tag} ===")
hits = []
for off in range(0, len(d) - 24, 4):
    u1, tag = struct.unpack_from("<II", d, off + 16)
    if tag in (TAG_HOOK, TAG_UIKEY) or u1 == 0x00000101:
        nm = d[off:off + 16].split(b"\x00")[0]
        if nm and all(32 <= c < 127 for c in nm):
            hits.append((off, nm.decode(), u1, tag))
for off, nm, u1, tag in hits:
    print("0x%06x  %-18s u1=0x%08x tag=0x%08x %s" % (
        off, nm, u1, tag,
        "<HOOK>" if tag == TAG_HOOK else ("<UIKEY>" if tag == TAG_UIKEY else "")))

print("\n=== raw 0x1700..0x1900 (early HOOK cluster) ===")
for i in range(0x1700, 0x1900, 16):
    c = d[i:i + 16]
    print("0x%06x  %-47s  %s" % (i, " ".join("%02x" % x for x in c), asc(c)))
