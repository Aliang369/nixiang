# -*- coding: utf-8 -*-
"""BL/B 调用点全库统计（只读）。用法: python3 _blscan.py 0x207f798 0x207dccc ...
输出各目标的调用点数、调用点分布区间、AOT 桩聚类。"""
import sys, struct, json, collections
SO="/Users/aliang/Documents/nixiang/13_scratch/bili_201/lib/arm64-v8a/libil2cpp.so"
ELF=json.load(open("/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1_il2cpp/_elf_new.json"))
EXEC=[(p["vaddr"],p["vaddr"]+p["filesz"],p["off"]) for p in ELF["ph"] if p["type"]==1 and (p["flags"]&1)]
OUT="/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1_il2cpp"

def sxt(v,b): m=1<<(b-1); return (v^m)-m

def main():
    targets=set(int(x,16) for x in sys.argv[1:])
    blob=open(SO,"rb").read()
    callers=collections.defaultdict(list)   # target -> [(site, kind)]
    allbl=0
    for vs,ve,off in EXEC:
        data=blob[off:off+(ve-vs)]
        n=len(data)//4
        for i in range(n):
            insn=struct.unpack_from("<I",data,i*4)[0]
            va=vs+i*4
            if (insn & 0xFC000000)==0x94000000:   # BL
                allbl+=1
                t=va+sxt(insn & 0x3FFFFFF,26)*4
                if t in targets: callers[t].append((va,"bl"))
            elif (insn & 0xFC000000)==0x14000000: # B
                t=va+sxt(insn & 0x3FFFFFF,26)*4
                if t in targets: callers[t].append((va,"b"))
            elif (insn & 0xFF000010)==0x54000000: # B.cond
                t=va+sxt((insn>>5)&0x7FFFF,19)*4
                if t in targets: callers[t].append((va,"b.cond"))
    print("total BL in exec:",allbl)
    res={}
    for t in sorted(targets):
        cl=sorted(callers.get(t,[]))
        addrs=[c[0] for c in cl]
        print("\n== %#x : %d call site(s) =="%(t,len(cl)))
        if addrs:
            print("  min=%#x max=%#x"%(addrs[0],addrs[-1]))
            # cluster by 4KB page
            pg=collections.Counter(a>>12 for a in addrs)
            print("  pages involved: %d ; top pages: %s"%(len(pg),[( "%#x"% (p<<12), c) for p,c in pg.most_common(6)]))
            # contiguous clusters (gap > 0x1000 starts new cluster)
            clus=[]; start=addrs[0]; prev=addrs[0]; cnt=1
            for a in addrs[1:]:
                if a-prev>0x1000:
                    clus.append((start,prev,cnt)); start=a; cnt=1
                else: cnt+=1
                prev=a
            clus.append((start,prev,cnt))
            print("  clusters (gap>0x1000): %d"%len(clus))
            for s,e,c in clus[:40]:
                print("    %#x..%#x  count=%d"%(s,e,c))
            if len(clus)>40: print("    ...")
            print("  first 40 sites:", ["%#x(%s)"%(a,k) for a,k in cl[:40]])
        res["%#x"%t]=cl
    json.dump(res,open(OUT+"/_callers.json","w"),indent=0)
    print("\nwrote _callers.json")

main()
