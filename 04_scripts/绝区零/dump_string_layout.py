#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""dump_string_layout.py — 查看内存/文件范围内 ASCII 串与串间字节的布局。

用法:
  python dump_string_layout.py FILE --from 0x706e100 --to 0x706e400 [--min 3]
"""
import argparse


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("file")
    ap.add_argument("--from", dest="frm", type=lambda x: int(x, 0), required=True)
    ap.add_argument("--to", dest="to", type=lambda x: int(x, 0), required=True)
    ap.add_argument("--min", type=int, default=3)
    a = ap.parse_args()

    d = open(a.file, "rb").read()
    seg = d[a.frm:a.to]
    pos = 0
    last_end = 0
    runs = []
    i = 0
    while i < len(seg):
        b = seg[i]
        if 32 <= b < 127:
            j = i
            while j < len(seg) and 32 <= seg[j] < 127:
                j += 1
            if j - i >= a.min:
                runs.append((i, j))
            i = j
        else:
            i += 1
    for (s, e) in runs:
        gap = seg[last_end:s]
        print("0x%08x len=%2d  %-40s   gap[%d]=%s" % (
            a.frm + s, e - s, seg[s:e].decode("ascii"),
            len(gap), gap[:40].hex()))
        last_end = e


if __name__ == "__main__":
    main()
