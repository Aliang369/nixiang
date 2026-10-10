#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import struct
SEG = "/Users/aliang/Documents/nixiang/06_dumped/崩坏3/mem_20261008/bh3_shell/seg_23_rw-p_7349938000.bin"
d = open(SEG, "rb").read()

def asc(b):
    return "".join(chr(c) if 32 <= c < 127 else "." for c in b)

print("seg_23 size = %d (0x%x)" % (len(d), len(d)))
print("=== raw 24-byte rows 0x24c00..0x25200 ===")
for i in range(0x24c00, 0x25200, 24):
    c = d[i:i + 24]
    u1, tag = struct.unpack_from("<II", c, 16)
    tagm = ""
    if tag == 0x66b6:
        tagm = "HOOK"
    elif tag == 0x6662:
        tagm = "UIKEY"
    print("0x%06x  %-47s | %s | u1=0x%08x tag=0x%08x %s" % (
        i, " ".join("%02x" % x for x in c), asc(c), u1, tag, tagm))
