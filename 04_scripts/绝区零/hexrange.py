#!/usr/bin/env python3
"""Hexdump + UTF-8 decode of a range, written with explicit utf-8 stdout."""
import sys

path = sys.argv[1]
start = int(sys.argv[2], 16)
length = int(sys.argv[3], 16)
data = open(path, "rb").read()[start:start + length]
for i in range(0, len(data), 16):
    row = data[i:i + 16]
    hexs = " ".join(f"{b:02x}" for b in row)
    txt = "".join(chr(b) if 32 <= b < 127 else "." for b in row)
    print(f"{start + i:#08x}  {hexs:47s}  {txt}")
print("=== utf8 decode (ignore errors) ===")
print(data.decode("utf-8", "replace"))
