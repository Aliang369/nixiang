#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import os, re
D = "/Users/aliang/Documents/nixiang/06_dumped/崩坏3/mem_20261008/bh3_shell"

# keywords: detection / reporting / patch mechanism
KW = [b"mprotect", b"mremap", b"mmap", b"memcpy", b"__memcpy", b"pthread",
      b"tersafe", b"ACE", b"tp2", b"Tss", b"Tencent", b"anti", b"Anti",
      b"frida", b"Frida", b"xposed", b"Xposed", b"magisk", b"Magisk",
      b"TracerPid", b"ptrace", b"root", b"su", b"busybox",
      b"debug", b"Debug", b"emulator", b"x86", b"qemu",
      b"libil2cpp", b"libSDKloading", b"libtoolChecker",
      b"report", b"Report", b"check", b"Check", b"verify", b"Verify",
      b"log", b"Log", b"http", b"https", b"myqcloud", b"file.myqcloud"]

def strings(b, minlen=5):
    out = []
    cur = bytearray()
    for c in b:
        if 32 <= c < 127:
            cur.append(c)
        else:
            if len(cur) >= minlen:
                out.append(cur.decode("latin1"))
            cur = bytearray()
    if len(cur) >= minlen:
        out.append(cur.decode("latin1"))
    return out

for f in sorted(os.listdir(D)):
    if not f.endswith(".bin"):
        continue
    b = open(os.path.join(D, f), "rb").read()
    ss = strings(b)
    hits = {}
    for s in ss:
        for k in KW:
            kk = k.decode()
            if kk in s and 4 < len(s) < 120:
                hits.setdefault(kk, set()).add(s)
    if not hits:
        continue
    print("\n########## %s (%d bytes) ##########" % (f, len(b)))
    for kk in sorted(hits):
        vals = sorted(hits[kk])[:6]
        print("  [%s] %s" % (kk, " || ".join(vals)))
