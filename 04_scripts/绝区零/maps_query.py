#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""maps_query.py — 解析 /proc/<pid>/maps，按条件筛选区域（只读分析）。

用法:
  python maps_query.py maps.txt [--path metadata] [--perms rw] [--min-mb 8] [--top 30]
"""
import argparse
import re


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("maps")
    ap.add_argument("--path", default=None)
    ap.add_argument("--perms", default=None, help="子串匹配，如 rw / r-x")
    ap.add_argument("--min-mb", type=float, default=0)
    ap.add_argument("--top", type=int, default=30)
    ap.add_argument("--sort", action="store_true", default=True)
    a = ap.parse_args()

    rows = []
    for line in open(a.maps, encoding="utf-8", errors="replace"):
        m = re.match(r"([0-9a-f]+)-([0-9a-f]+) (\S{4}) ([0-9a-f]+) (\S+) (\d+)\s*(.*)", line.rstrip("\n"))
        if not m:
            continue
        s, e = int(m.group(1), 16), int(m.group(2), 16)
        perms, path = m.group(3), m.group(7)
        if a.path and a.path.lower() not in path.lower():
            continue
        if a.perms and a.perms not in perms:
            continue
        rows.append((e - s, perms, s, e, m.group(4), path))

    rows.sort(reverse=True)
    total = sum(r[0] for r in rows)
    print("matched=%d total=%.1fMB" % (len(rows), total / 1048576))
    for sz, perms, s, e, off, path in rows[:a.top]:
        print("%8.1fMB %s %012x-%012x off=%s %s" % (sz / 1048576, perms, s, e, off, path))


if __name__ == "__main__":
    main()
