#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import re, struct, os, glob

BASE = "/Users/aliang/Documents/nixiang/06_dumped/崩坏3/mem_20261008/bh3_shell"

def strings(buf, minlen=5):
    out = []
    cur = bytearray(); start = 0
    for i, c in enumerate(buf):
        if 32 <= c < 127:
            if not cur: start = i
            cur.append(c)
        else:
            if len(cur) >= minlen:
                out.append((start, cur.decode("latin1")))
            cur = bytearray()
    if len(cur) >= minlen:
        out.append((start, cur.decode("latin1")))
    return out

def cjk_strings(buf, minlen=2):
    # decode UTF-8 sequences containing CJK
    out = []
    i = 0
    while i < len(buf):
        b = buf[i]
        if b >= 0xe0:
            j = i
            chunk = bytearray()
            while j < len(buf) and buf[j] >= 0x80:
                chunk.append(buf[j]); j += 1
            try:
                s = chunk.decode("utf-8")
                if len(s) >= minlen and any('\u4e00' <= ch <= '\u9fff' for ch in s):
                    out.append((i, s))
            except Exception:
                pass
            i = j
        else:
            i += 1
    return out

# ---- seg_23 full ASCII + CJK ----
p = os.path.join(BASE, "seg_23_rw-p_7349938000.bin")
d = open(p, "rb").read()
print("=== seg_23 size=%d ASCII strings (len>=6) ===" % len(d))
for off, s in strings(d, 6):
    print("  0x%06x %s" % (off, s))
print("=== seg_23 CJK ===")
for off, s in cjk_strings(d, 2):
    print("  0x%06x %s" % (off, s))

# ---- seg_20 (.data/BSS of libSDKloading) ----
p20 = os.path.join(BASE, "seg_20_rw-p_7349768000.bin")
if os.path.exists(p20):
    d20 = open(p20, "rb").read()
    print("\n=== seg_20 size=%d ASCII strings (len>=6) ===" % len(d20))
    for off, s in strings(d20, 6):
        print("  0x%06x %s" % (off, s))
    print("=== seg_20 CJK ===")
    for off, s in cjk_strings(d20, 2):
        print("  0x%06x %s" % (off, s))
else:
    print("\n(seg_20 file not found; listing seg files)")
    for f in sorted(glob.glob(os.path.join(BASE, "seg_*.bin"))):
        print("  ", os.path.basename(f))
