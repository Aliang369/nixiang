#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""range_strings.py — 打印二进制文件指定范围内的可打印串（含偏移），UTF-8 宽容解码。

用法:
  python range_strings.py FILE --from 0xe7400 --to 0xe7b00 [--min 4]
"""
import argparse
import re


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("file")
    ap.add_argument("--from", dest="frm", type=lambda x: int(x, 0), required=True)
    ap.add_argument("--to", dest="to", type=lambda x: int(x, 0), required=True)
    ap.add_argument("--min", type=int, default=4)
    a = ap.parse_args()

    d = open(a.file, "rb").read()[a.frm:a.to]
    rx = re.compile(rb"[\x20-\x7e]{%d,}" % a.min)
    for m in rx.finditer(d):
        print("0x%x  %s" % (a.frm + m.start(), m.group(0).decode("ascii", "replace")))


if __name__ == "__main__":
    main()
