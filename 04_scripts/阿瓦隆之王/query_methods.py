#!/usr/bin/env python3
"""Search Il2CppDumper script.json for methods at (or spanning) given RVAs."""
import json
import sys
import bisect

RVA_TARGETS = [int(x, 16) if x.startswith(("0x", "0X")) else int(x) for x in sys.argv[1:]]
IDX = "/Users/aliang/Documents/nixiang/06_dumped/阿瓦隆之王/dump_out/script.json"

with open(IDX) as f:
    idx = json.load(f)
methods = idx["ScriptMethod"]
print(f"total methods: {len(methods)}")
addrs = [m["Address"] for m in methods]
mn, mx = min(addrs), max(addrs)
print(f"address range: 0x{mn:x} .. 0x{mx:x}")

keyed = sorted(methods, key=lambda m: m["Address"])
addrs2 = [m["Address"] for m in keyed]
for t in RVA_TARGETS:
    pos = bisect.bisect_left(addrs2, t)
    print(f"\n=== target 0x{t:x} ===")
    lo = max(0, pos - 2)
    hi = min(len(keyed), pos + 3)
    for cand in range(lo, hi):
        m = keyed[cand]
        d = m["Address"] - t
        print(f"  addr=0x{m['Address']:x} (d={d:+d}) {m['Name'][:120]}")