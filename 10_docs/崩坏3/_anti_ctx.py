#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import re, os

def strings(path, minlen=5, limit=400):
    d = open(path, "rb").read()
    out = []
    for m in re.finditer(rb"[\x20-\x7e]{%d,}" % minlen, d):
        out.append((m.start(), m.group().decode("ascii", "ignore")))
    return d, out

def ctx(path, off, before=64, after=160):
    d = open(path, "rb").read()
    s = max(0, off - before); e = min(len(d), off + after)
    seg = d[s:e]
    txt = "".join(chr(c) if 32 <= c < 127 else ("\n" if c in (10, 13) else ".") for c in seg)
    return txt

BASE = "/Users/aliang/Documents/nixiang/06_dumped/崩坏3/mem_20261008/bh3_shell"
TARGETS = [
    ("seg_21(shell-rx)", BASE + "/seg_21_r-xp_734983c000.bin"),
    ("seg_31(anon-rx)",  BASE + "/seg_31_r-xp_7368115000.bin"),
    ("seg_34(anon-rw)",  BASE + "/seg_34_rw-p_7368195000.bin"),
]

KEYS = ["mprotect", "Frida", "frida", "TracerPid", "ptrace", "eglSwapBuffers",
        "libEGL", "BW_MEMORYTOOL", "libtprt", "tersafe", "signature", "apk_hash",
        "report", "check", "root", "magisk", "xposed", "/proc/self", "Toast",
        "anticheat", "gamesafe"]

for name, path in TARGETS:
    if not os.path.exists(path):
        print("!! missing", path); continue
    d, ss = strings(path, 5, 5000)
    print("\n########## %s  (%d bytes, %d strings) ##########" % (name, len(d), len(ss)))
    seen = set()
    for off, s in ss:
        low = s.lower()
        for k in KEYS:
            if k.lower() in low:
                if s in seen:
                    break
                seen.add(s)
                print("  [0x%06x] %s" % (off, s[:140]))
                break
    if len(seen) > 90:
        print("  ... (%d matches, truncated)" % len(seen))

print("\n\n########## context: seg_21 mprotect-failed / Frida ##########")
for off in (0xd4a21, 0xd53d6, 0xb1730):
    print("\n--- seg_21 @0x%x ---" % off)
    print(ctx(BASE + "/seg_21_r-xp_734983c000.bin", off, 96, 220))
