#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""maps_top.py — 解析 /proc/<pid>/maps 文本，按大小列出映射（供内存定位用）。

用法: python maps_top.py <maps文件> [--min-mb 8] [--grep 关键字] [--top 40]
"""
import argparse
import re

ap = argparse.ArgumentParser()
ap.add_argument("maps")
ap.add_argument("--min-mb", type=float, default=8.0)
ap.add_argument("--grep", default=None)
ap.add_argument("--perm", default=None, help="按权限前缀过滤，如 rw / r--p / ---p")
ap.add_argument("--max-mb", type=float, default=1e9)
ap.add_argument("--top", type=int, default=40)
a = ap.parse_args()

rows = []
for line in open(a.maps, errors="replace"):
    m = re.match(r"([0-9a-f]+)-([0-9a-f]+) (\S{4}) ([0-9a-f]+) (\S+) (\d+)\s*(.*)$", line.strip())
    if not m:
        continue
    s, e = int(m.group(1), 16), int(m.group(2), 16)
    path = m.group(7).strip() or "anon"
    if a.grep and a.grep not in path:
        continue
    if a.perm and not m.group(3).startswith(a.perm):
        continue
    if not (a.min_mb * 1048576 <= e - s <= a.max_mb * 1048576):
        continue
    rows.append((e - s, s, e, m.group(3), m.group(4), path))

rows.sort(reverse=True)
print("total regions matched=%d" % len(rows))
for sz, s, e, perm, off, path in rows[: a.top]:
    print("%9.1fMB %012x-%012x %s off=%-9s %s" % (sz / 1048576.0, s, e, perm, off, path[:80]))
