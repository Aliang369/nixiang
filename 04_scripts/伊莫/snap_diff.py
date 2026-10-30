"""解析 logcat 里的 spd SNAP 快照，找出「随时间变化」的 float 字段（= 实时速度候选）。"""
import re, sys, os
from collections import defaultdict

path = sys.argv[1] if len(sys.argv) > 1 else r'E:\nixiang\09_test\_yimo_il2cpp\log_snap.txt'
txt = open(path, encoding='utf-8', errors='replace').read()
# spd SNAP t=22227393 this=0x40000d22a000 [60E0] XXXX XXXX ...
pat = re.compile(r'spd SNAP\s+t=(\d+)\s+this=(0x[0-9a-f]+)\s+\[([0-9A-F]+)\]\s+([0-9A-F ]+)')
series = defaultdict(dict)      # (this, off) -> {t: word}
thiss = set()
for m in pat.finditer(txt):
    t, this, lo, hexw = int(m.group(1)), m.group(2), int(m.group(3), 16), m.group(4).split()
    thiss.add(this)
    for i, w in enumerate(hexw):
        series[(this, lo + i * 4)][t] = w
print(f"this 数 {len(thiss)}，字段序列 {len(series)}")

def as_float(w):
    import struct
    return struct.unpack('<f', struct.pack('<I', int(w, 16)))[0]

for this in sorted(thiss):
    rows = []
    for (th, off), s in series.items():
        if th != this: continue
        vals = list(s.values())
        uniq = set(vals)
        if len(uniq) <= 1: continue
        fv = [as_float(v) for v in vals]
        plausible = [x for x in fv if -100 < x < 100]
        if not plausible: continue
        rows.append((off, len(uniq), min(plausible), max(plausible), sorted(set(round(x, 3) for x in plausible))[:6]))
    print(f"\n===== this={this} 变化的字段 =====")
    for off, n, lo, hi, smp in sorted(rows):
        print(f"  +{off:#05x}: {n:3d} 种取值  {lo:8.3f} .. {hi:8.3f}   样例 {smp}")
