# -*- coding: utf-8 -*-
"""从 dump.cs 统计 RVA 直方图（流式逐行，不整读内存）"""
import re, collections, sys
DUMP="/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1_il2cpp/dump.cs"
rx=re.compile(r"RVA: (0x[0-9A-Fa-f]+) Offset: (0x[0-9A-Fa-f]+) VA: (0x[0-9A-Fa-f]+)")
hist=collections.Counter(); n=0; zero=0
with open(DUMP,"r",encoding="utf-8",errors="replace") as f:
    for line in f:
        m=rx.search(line)
        if not m: continue
        n+=1; v=int(m.group(1),16)
        if v==0: zero+=1
        hist[v]+=1
print("methods with RVA line:",n," RVA==0:",zero)
print("\n== top 15 most common RVA ==")
for v,c in hist.most_common(15):
    print("  0x%X  count=%d"%(v,c))
print("\n== specific targets ==")
for t in (0x207F04C,0x207F798,0x207DCCC,0x207DE04,0x207DD40,0x207DDAC,0x207F064):
    print("  0x%X -> %d"%(t,hist.get(t,0)))
print("\n== RVA window 0x207E000..0x2081000 ==")
win=[(v,c) for v,c in hist.items() if 0x207E000<=v<0x2090000]
win.sort()
tot=sum(c for v,c in win)
print("  distinct=%d total methods=%d"%(len(win),tot))
for v,c in win[:25]:
    print("    0x%X count=%d"%(v,c))
