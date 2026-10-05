#!/usr/bin/env python3
"""Find little-endian 64-bit value occurrences in a binary file."""
import struct
import sys

path = sys.argv[1]
val = int(sys.argv[2], 16)
data = open(path, "rb").read()
pat = struct.pack("<Q", val)
hits = []
i = 0
while True:
    j = data.find(pat, i)
    if j < 0:
        break
    hits.append(j)
    i = j + 1
print(f"{len(hits)} hit(s) for {val:#x}")
for h in hits[:40]:
    print(f"  0x{h:08x}")
