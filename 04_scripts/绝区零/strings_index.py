#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""strings_index.py — 抽取二进制里所有可打印串（含偏移）写成 TSV，供后续 grep。

用法:
  python strings_index.py FILE OUT.tsv [--min 4] [--max 200]
"""
import argparse
import re


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("file")
    ap.add_argument("out")
    ap.add_argument("--min", type=int, default=4)
    ap.add_argument("--max", type=int, default=200)
    a = ap.parse_args()

    d = open(a.file, "rb").read()
    rx = re.compile(rb"[\x20-\x7e]{%d,%d}" % (a.min, a.max))
    n = 0
    with open(a.out, "w", encoding="utf-8") as f:
        for m in rx.finditer(d):
            f.write("%d\t%s\n" % (m.start(), m.group(0).decode("ascii")))
            n += 1
    print("strings=%d -> %s" % (n, a.out))


if __name__ == "__main__":
    main()
