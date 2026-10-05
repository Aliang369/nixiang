#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""scan_names.py — 扫描内存 dump 里的「方法名 entry」结构（实测布局）。

entry (0x40 bytes):
  +0x00 : qword 指针（0x5f.. 区）
  +0x08 : 24B hash
  +0x20 : 名称 ASCIIZ（≤0x1f 字符），其后 0 填充到 +0x40
输出: entry_va\tname
用法: python scan_names.py out.tsv [dumpdir]
"""
import glob
import os
import re
import struct
import sys

DEFAULT_DUMPS = {
    "region_5f80_v2.bin": 0x5F80001000,
    "region_7800.bin": 0x7800000000,
    "r45.bin": 0x5FA9800000,
    "r5fa9.bin": 0x5FA9800000,
    "r5fa5.bin": 0x5FA5800000,
}

name_chars = set("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_`<>$.-[]+=,:/@?#!%~^&*()|;'\" ")


def main():
    out = sys.argv[1]
    d = sys.argv[2] if len(sys.argv) > 2 else os.path.join(
        os.environ.get("TEMP", "/tmp"), "nx_zzz_a_20261005")
    dumps = []
    for f in sorted(glob.glob(os.path.join(d, "*.bin"))):
        base = DEFAULT_DUMPS.get(os.path.basename(f))
        if base is None:
            continue
        dumps.append((f, base, open(f, "rb").read()))
    res = {}
    for f, base, data in dumps:
        n = len(data) - 0x40
        for i in range(0, n, 8):
            p = struct.unpack_from("<Q", data, i)[0]
            if not (0x5F00000000 <= p < 0x8000000000):
                continue
            blk = data[i + 0x08:i + 0x20]
            if blk.count(0) > 16:
                continue
            seg = data[i + 0x20:i + 0x40]
            z = seg.find(b"\x00")
            if z < 1 or z > 0x1F:
                continue
            if any(seg[z + 1:]):
                continue
            try:
                nm = seg[:z].decode("ascii")
            except UnicodeDecodeError:
                continue
            if not all(c in name_chars for c in nm):
                continue
            if not (nm[0].isalpha() or nm[0] == "_"):
                continue
            res[base + i] = nm
    with open(out, "w", encoding="utf-8") as fh:
        for k in sorted(res):
            fh.write("%#x\t%s\n" % (k, res[k]))
    print("entries=%d -> %s" % (len(res), out), file=sys.stderr)


if __name__ == "__main__":
    main()
