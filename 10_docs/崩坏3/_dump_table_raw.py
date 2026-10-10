#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import struct
p = "/Users/aliang/Documents/nixiang/06_dumped/崩坏3/mem_20261008/bh3_shell/seg_23_rw-p_7349938000.bin"
d = open(p, "rb").read()

def hexdump(a, b, stride=16):
    print("--- hexdump 0x%05x .. 0x%05x ---" % (a, b))
    for o in range(a, b, stride):
        chunk = d[o:o+stride]
        hx = " ".join("%02x" % c for c in chunk)
        asc = "".join(chr(c) if 32 <= c < 127 else "." for c in chunk)
        print("0x%05x  %-47s  %s" % (o, hx, asc))

# table region: hooks + patch + UI keys + JNI names
hexdump(0x24c80, 0x24f40)
# string/JNI region around menu URL
hexdump(0x24600, 0x24890)
