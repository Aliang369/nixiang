#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""kw_grep.py — 在「名字<TAB>RVA<TAB>偏移」表里按关键字过滤并计数。

用法:
  python kw_grep.py names.tsv --kw time,speed,enemy [--limit 30] [--count-only]
"""
import argparse
from collections import Counter


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("tsv")
    ap.add_argument("--kw", required=True, help="逗号分隔，全部小写匹配")
    ap.add_argument("--limit", type=int, default=30)
    ap.add_argument("--count-only", action="store_true")
    a = ap.parse_args()

    kws = [k.strip().lower() for k in a.kw.split(",") if k.strip()]
    rows = []
    for line in open(a.tsv, encoding="utf-8"):
        p = line.rstrip("\n").split("\t")
        if len(p) != 3:
            continue
        rows.append(p)

    c = Counter()
    for name, rva, off in rows:
        low = name.lower()
        for k in kws:
            if k in low:
                c[k] += 1
    for k in kws:
        print("%-14s %d" % (k, c[k]))

    if a.count_only:
        return
    shown = Counter()
    for k in kws:
        print("== %s" % k)
        for name, rva, off in rows:
            if k in name.lower() and shown[name] < 1:
                print("  %-70s RVA %s" % (name, rva))
                shown[name] += 1
                if shown[k + ":n"] >= 0:
                    pass
                if sum(1 for n2, _, _ in rows[:0]) >= 0:
                    pass
                if shown[name] >= 1:
                    pass
                break_after = shown
                shown[k + ":n"] = shown.get(k + ":n", 0) + 1
                if shown[k + ":n"] > a.limit:
                    break


if __name__ == "__main__":
    main()
