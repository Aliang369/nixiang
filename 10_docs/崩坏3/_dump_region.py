#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import re, glob, os

SEG = "/Users/aliang/Documents/nixiang/06_dumped/崩坏3/mem_20261008/bh3_shell/seg_23_rw-p_7349938000.bin"
data = open(SEG, "rb").read()

def hexdump(d, base, start, end, width=16):
    for off in range(start, end, width):
        chunk = d[off:off + width]
        hexs = " ".join("%02x" % b for b in chunk)
        asc = "".join(chr(b) if 32 <= b < 127 else "." for b in chunk)
        print("0x%06x  %-47s  %s" % (off, hexs, asc))

print("=== seg_23 raw hexdump 0x24cd0..0x24f40 ===")
hexdump(data, 0, 0x24cd0, 0x24f40)
print()
print("=== seg_23 raw hexdump 0x24f40..0x25000 ===")
hexdump(data, 0, 0x24f40, 0x25000)

# ---- scan ALL segments for feature-name / method-name / CJK strings ----
print("\n=== ALL-SEG string scan ===")
CJK_WORDS = "秒杀无敌血量暴击伤害攻击功能菜单开启关闭血量修改不死无限生命"
EN_WORDS = ["Damage", "Attack", "Hurt", "Hp", "HP", "Crit", "Critical", "Immortal", "Invinc",
            "Kill", "Death", "Die", "Monster", "Enemy", "Player", "Avatar", "Actor", "Battle",
            "get_ret", "getRet", "featNum", "feat_num", "SetValue", "get_value"]
segfiles = sorted(glob.glob("/Users/aliang/Documents/nixiang/06_dumped/崩坏3/mem_20261008/bh3_shell/seg_*.bin"))
cjk = re.compile(rb"(?:[\xe4-\xe9][\x80-\xbf][\x80-\xbf]){1,}")
for f in segfiles:
    d = open(f, "rb").read()
    hits = []
    for m in cjk.finditer(d):
        try:
            s = m.group().decode("utf-8")
        except Exception:
            continue
        if any(w in s for w in CJK_WORDS) and len(s) >= 2:
            hits.append((m.start(), s))
    if hits:
        print("[%s]" % os.path.basename(f))
        for off, s in hits[:20]:
            print("    0x%x: %s" % (off, s))
print("\n=== .dll / C# name-like ASCII across segs ===")
for f in segfiles:
    d = open(f, "rb").read()
    hits = set()
    for m in re.finditer(rb"[\x20-\x7e]{4,60}\.dll", d):
        hits.add(m.group().decode("latin1"))
    if hits:
        print("[%s] %s" % (os.path.basename(f), sorted(hits)[:10]))
