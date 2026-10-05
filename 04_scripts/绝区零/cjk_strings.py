#!/usr/bin/env python3
"""Extract UTF-8 runs containing CJK from a binary, with offsets."""
import re
import sys

path = sys.argv[1]
lo = int(sys.argv[2], 16) if len(sys.argv) > 2 else 0
hi = int(sys.argv[3], 16) if len(sys.argv) > 3 else None
mincjk = int(sys.argv[4]) if len(sys.argv) > 4 else 1

data = open(path, "rb").read()
if hi is None:
    hi = len(data)
data = data[lo:hi]

# UTF-8 printable runs: allow ASCII printable + all multi-byte sequences
rx = re.compile(rb"(?:[\x20-\x7e]|[\xc2-\xf4][\x80-\xbf]+){8,}")
seen = set()
for m in rx.finditer(data):
    try:
        s = m.group().decode("utf-8")
    except UnicodeDecodeError:
        continue
    cjk = len(re.findall(r"[\u4e00-\u9fff]", s))
    if cjk >= mincjk:
        key = s
        if key in seen:
            continue
        seen.add(key)
        print(f"{lo + m.start():#08x}  {s}")
