#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import re, os

FILES = {
    "libSDKloading.so": "/tmp/bh3_recon/shell/libSDKloading.so",
    "libtersafe2.so":   "/tmp/bh3_recon/shell/libtersafe2.so",
    "libtoolChecker.so":"/tmp/bh3_recon/shell/libtoolChecker.so",
    "seg_21(shell)":    "/Users/aliang/Documents/nixiang/06_dumped/崩坏3/mem_20261008/bh3_shell/seg_21_r-xp_734983c000.bin",
    "seg_24(rwxp)":     "/Users/aliang/Documents/nixiang/06_dumped/崩坏3/mem_20261008/bh3_shell/seg_24_rwxp_734995e000.bin",
}
KW = [b"tersafe", b"tprt", b"ACE", b"Tss", b"Tencent", b"gamesafe", b"ano", b"anticheat",
      b"frida", b"xposed", b"magisk", b"ptrace", b"TracerPid", b"debug", b"root", b"su",
      b"emulator", b"x86", b"eglSwapBuffers", b"libEGL", b"overlay", b"mprotect", b"inline",
      b"hook", b"patch", b"report", b"check", b"signature", b"apk_hash", b"BW_MEMORYTOOL",
      b"Frida", b"maps", b"libSDKloading", b"libil2cpp", b"mprotect failed", b"ptrace"]

for name, path in FILES.items():
    if not os.path.exists(path):
        print("!! missing %s" % path); continue
    d = open(path, "rb").read()
    print("\n===== %s  (%d bytes) =====" % (name, len(d)))
    for kw in KW:
        idxs = [m.start() for m in re.finditer(re.escape(kw), d)]
        if idxs:
            print("  %-16s x%-3d %s" % (kw.decode(), len(idxs), [hex(i) for i in idxs[:6]]))
