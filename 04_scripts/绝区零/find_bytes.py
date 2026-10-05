#!/usr/bin/env python3
"""Find a hex byte pattern in a binary file.

usage: find_bytes.py <file> <hexpattern> [start] [len]
"""
import sys

path = sys.argv[1]
pat = bytes.fromhex(sys.argv[2])
start = int(sys.argv[3], 16) if len(sys.argv) > 3 else 0
length = int(sys.argv[4], 16) if len(sys.argv) > 4 else None

data = open(path, "rb").read()
if length is None:
    length = len(data) - start
chunk = data[start:start + length]

hits = []
i = 0
while True:
    j = chunk.find(pat, i)
    if j < 0:
        break
    hits.append(start + j)
    i = j + 1
print(f"{len(hits)} hit(s)")
for h in hits[:40]:
    print(f"  0x{h:08x}")
