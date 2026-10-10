#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import re, os

def cjk_strings(data, minrun=2):
    out = []
    i = 0
    n = len(data)
    cur = bytearray()
    start = 0
    while i < n:
        b = data[i]
        if b >= 0x80:
            if not cur:
                start = i
            cur.append(b)
            i += 1
        else:
            if len(cur) >= 3:
                try:
                    s = bytes(cur).decode("utf-8")
                    # keep only if it has CJK
                    if re.search(r"[\u4e00-\u9fff\u3000-\u303f\uff00-\uffef]", s):
                        out.append((start, s))
                except Exception:
                    pass
            cur = bytearray()
            i += 1
    if len(cur) >= 3:
        try:
            s = bytes(cur).decode("utf-8")
            if re.search(r"[\u4e00-\u9fff]", s):
                out.append((start, s))
        except Exception:
            pass
    return out

BASE = "/Users/aliang/Documents/nixiang/06_dumped/崩坏3/mem_20261008/bh3_shell"
FILES = [
    ("libSDKloading.so", "/tmp/bh3_recon/shell/libSDKloading.so"),
    ("seg_21(shell-rx)", BASE + "/seg_21_r-xp_734983c000.bin"),
    ("seg_31(anon-rx)",  BASE + "/seg_31_r-xp_7368115000.bin"),
    ("seg_23(table-rw)", BASE + "/seg_23_rw-p_7349938000.bin"),
    ("seg_20(sdk-rw)",   BASE + "/seg_20_rw-p_7349768000.bin"),
    ("seg_34(anon-rw)",  BASE + "/seg_34_rw-p_7368195000.bin"),
    ("seg_24(rwxp)",     BASE + "/seg_24_rwxp_734995e000.bin"),
]

for name, path in FILES:
    if not os.path.exists(path):
        print("!! missing", path); continue
    d = open(path, "rb").read()
    ss = cjk_strings(d)
    print("\n########## %s  (%d bytes, %d CJK runs) ##########" % (name, len(d), len(ss)))
    for off, s in ss[:80]:
        print("  [0x%06x] %s" % (off, s[:160]))
    if len(ss) > 80:
        print("  ... (%d total, truncated)" % len(ss))
