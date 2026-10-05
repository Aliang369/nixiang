#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""scan_dir_strings.py — 扫描目录里的文件，统计给定 ASCII 关键字命中数。

用法:
  python scan_dir_strings.py <dir> --kw global-metadata,MHY --ext .so [--max 30]
"""
import argparse
import os

ap = argparse.ArgumentParser()
ap.add_argument("dir")
ap.add_argument("--kw", required=True)
ap.add_argument("--ext", default=None)
ap.add_argument("--max", type=int, default=30)
a = ap.parse_args()

kws = [(k, k.encode()) for k in a.kw.split(",") if k]
rows = []
for name in sorted(os.listdir(a.dir)):
    p = os.path.join(a.dir, name)
    if not os.path.isfile(p):
        continue
    if a.ext and not name.endswith(a.ext):
        continue
    d = open(p, "rb").read()
    counts = [(k, d.count(b)) for k, b in kws]
    if any(c for _, c in counts):
        rows.append((name, len(d), counts))

print("files matched=%d" % len(rows))
for name, size, counts in rows[: a.max]:
    print("%-46s %10d  %s" % (name[:46], size, "  ".join("%s=%d" % (k, c) for k, c in counts)))
