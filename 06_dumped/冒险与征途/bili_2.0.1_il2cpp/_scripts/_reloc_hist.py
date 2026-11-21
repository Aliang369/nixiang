# -*- coding: utf-8 -*-
"""统计 .rela.dyn 中 R_AARCH64_RELATIVE 的 addend 直方图（只读）
目的：找出被 methodPointers 表大量共同指向的代码地址（共享桩）。"""
import struct, collections, json
SO="/Users/aliang/Documents/nixiang/13_scratch/bili_201/lib/arm64-v8a/libil2cpp.so"
ELF=json.load(open("/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1_il2cpp/_elf_new.json"))
sec={s["name"]:s for s in ELF["sh"]}
rel=sec[".rela.dyn"]
blob=open(SO,"rb").read()
n=rel["size"]//24
print("rela.dyn entries:",n,"off=%#x"%rel["off"])
hist=collections.Counter()
loc_by_target=collections.defaultdict(list)
for i in range(n):
    off,r_info,add=struct.unpack_from("<QQq",blob,rel["off"]+i*24)
    t=r_info & 0xFFFFFFFF
    if t==1027:
        hist[add]+=1
code_lo,code_hi=0x1d7408c,0x4cfccf0
print("\n== RELATIVE addend top 20 in code range ==")
cnt=0
for a,c in hist.most_common(20000):
    if code_lo<=a<code_hi:
        print("  addend=%#x  count=%d"%(a,c)); cnt+=1
        if cnt>=20: break
print("\n== targets of interest ==")
for t in (0x207f04c,0x207f798,0x207dccc,0x207de04,0x207dd40,0x207ddac,0x208e018,0x207f640):
    print("  %#x : reloc-addend count=%d"%(t,hist.get(t,0)))
print("\n== raw 8-byte occurrences in file (no reloc) ==")
for t in (0x207f04c,0x207f798,0x207dccc):
    pat=struct.pack("<Q",t)
    c=blob.count(pat)
    print("  %#x raw occurrences=%d"%(t,c))
