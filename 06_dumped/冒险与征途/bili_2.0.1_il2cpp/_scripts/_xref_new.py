# -*- coding: utf-8 -*-
"""ADRP+ADD / ADRP+LDR 数据引用扫描（只读，自写解码器，无 capstone）
用法: python3 _xref_new.py 0xD1CA18 0xCF2929 0xCF294D ...
"""
import sys, struct, json
SO="/Users/aliang/Documents/nixiang/13_scratch/bili_201/lib/arm64-v8a/libil2cpp.so"
ELF=json.load(open("/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1_il2cpp/_elf_new.json"))

EXEC=[(p["vaddr"],p["vaddr"]+p["filesz"],p["off"]) for p in ELF["ph"] if p["type"]==1 and (p["flags"]&1)]

def sxt(v,bits):
    m=1<<(bits-1)
    return (v^m)-m

def decode_stream(data, base_va, off):
    n=(len(data)//4)*4
    for i in range(0,n,4):
        insn=struct.unpack_from("<I",data,i)[0]
        yield base_va+i, insn

def main():
    targets=set(int(x,16) for x in sys.argv[1:])
    pages=set(t&~0xFFF for t in targets)
    print("targets:",["%#x"%t for t in sorted(targets)])
    print("pages:",["%#x"%p for p in sorted(pages)])
    with open(SO,"rb") as f:
        blob=f.read()
    hits=[]
    scanned=0
    for vs,ve,off in EXEC:
        data=blob[off:off+(ve-vs)]
        # collect adrp candidates then look ahead 6 insns
        insns=[]
        for va,insn in decode_stream(data,vs,off):
            scanned+=1
            insns.append((va,insn))
        for idx,(va,insn) in enumerate(insns):
            if (insn & 0x9F000000)!=0x90000000: continue
            rd=insn&0x1F
            immlo=(insn>>29)&3; immhi=(insn>>5)&0x7FFFF
            imm=sxt((immhi<<2)|immlo,21)
            page=(va & ~0xFFF)+(imm<<12)
            if page not in pages: continue
            for k in range(1,7):
                if idx+k>=len(insns): break
                va2,i2=insns[idx+k]
                # ADD (imm, 64/32-bit) rn==rd
                if (i2 & 0x7F000000)==0x11000000 and ((i2>>5)&0x1F)==rd:
                    imm12=(i2>>10)&0xFFF
                    sh=(i2>>22)&3
                    if sh==1: imm12<<=12
                    sub=1 if (i2>>30)&1 else 0
                    addr = page + (-imm12 if sub else imm12)
                    if addr in targets:
                        hits.append((va,"adrp+add",va2,addr,insn,i2))
                # ADRP+LDR (immediate, unsigned offset) - pointer table
                if (i2 & 0xFFC00000)==0xF9400000 and ((i2>>5)&0x1F)==rd:
                    off12=((i2>>10)&0xFFF)*8
                    addr=page+off12
                    if addr in targets:
                        hits.append((va,"adrp+ldr",va2,addr,insn,i2))
                # stop at branch/ret boundaries
                if (i2 & 0x7C000000)==0x14000000: break
                if i2==0xD65F03C0: break
    print("scanned insns:",scanned)
    print("hits:",len(hits))
    hitaddrs=set(h[3] for h in hits)
    for t in sorted(targets):
        got=[h for h in hits if h[3]==t]
        print("  %#x -> %d hit(s)  %s"%(t,len(got),["adrp@%#x add@%#x"%(h[0],h[2]) for h in got[:12]]))
    json.dump([[h[0],h[1],h[2],h[3]] for h in hits],open("/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1_il2cpp/_xref_hits.json","w"),indent=0)

main()
