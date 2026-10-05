#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""so_strings.py — 从二进制里抽取可打印 ASCII 串并按关键字过滤/计数。

用法:
  python so_strings.py <file> [--min 5] [--kw ultimate,timescale,enemy] [--max 200]
                        [--all]
"""
import argparse
import re
from collections import Counter

ap = argparse.ArgumentParser()
ap.add_argument("file")
ap.add_argument("--min", type=int, default=5)
ap.add_argument("--kw", default=None, help="逗号分隔关键字，不区分大小写")
ap.add_argument("--max", type=int, default=200)
ap.add_argument("--all", action="store_true")
a = ap.parse_args()

d = open(a.file, "rb").read()
rx = re.compile(rb"[\x20-\x7e]{%d,}" % a.min)
strs = [m.group(0).decode("ascii", "replace") for m in rx.finditer(d)]
if a.kw:
    kws = [k.lower() for k in a.kw.split(",") if k]
    hits = [s for s in strs if any(k in s.lower() for k in kws)]
else:
    hits = strs

c = Counter(hits)
print("file=%s total_strings=%d hits=%d uniq=%d" % (a.file, len(strs), len(hits), len(c)))
for s, n in c.most_common(a.max if not a.all else len(c)):
    print("%4d  %s" % (n, s))
