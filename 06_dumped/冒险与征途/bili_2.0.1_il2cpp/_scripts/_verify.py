# -*- coding: utf-8 -*-
"""① 旧记录反汇编样例 vs 新 so 逐字节比对  ② NotSupport* 桩的字符串 xref  ③ 桩指针所在数据槽"""
import re, struct, json, collections
so_path="/Users/aliang/Documents/nixiang/13_scratch/bili_201/lib/arm64-v8a/libil2cpp.so"
old_dis="/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/interp_entry_disasm.txt"
blob=open(so_path,"rb").read()
ELF=json.load(open("/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1_il2cpp/_elf_new.json"))
LOADS=[(p["off"],p["vaddr"],p["filesz"]) for p in ELF["ph"] if p["type"]==1]
def v2f(v):
    for off,va,sz in LOADS:
        if va<=v<va+sz: return v+(off-va)
    return None
rx=re.compile(r"^\s+([0-9a-f]{6,8}):\s+([0-9a-f]{8})\s")
tot=0; same=0; diffs=[]
for line in open(old_dis,encoding="utf-8",errors="replace"):
    m=rx.match(line)
    if not m: continue
    rva=int(m.group(1),16); word=int(m.group(2),16)
    fo=v2f(rva)
    if fo is None or fo+4>len(blob): continue
    tot+=1
    new=struct.unpack_from("<I",blob,fo)[0]
    if new==word: same+=1
    else: diffs.append((rva,word,new,line.strip()[:90]))
print("== 旧记录反汇编样例 vs 新 so ==")
print("  旧记录指令数(可定位):",tot," 逐字节一致:",same," 不一致:",len(diffs))
for d in diffs[:10]:
    print("   RVA %#x old=%08x new=%08x | %s"%(d[0],d[1],d[2],d[3]))

# ② 字符串 xref
def sxt(v,b): m=1<<(b-1); return (v^m)-m
targets={}
for s in ("NotSupportNative2Managed","NotSupportAdjustorThunk"):
    i=blob.find(s.encode()); targets[i]=s
print("\n== NotSupport* 字符串 ==")
for a,s in targets.items(): print("  %-26s @ %#x"%(s,a))
EXEC=[(p["vaddr"],p["vaddr"]+p["filesz"],p["off"]) for p in ELF["ph"] if p["type"]==1 and (p["flags"]&1)]
hits=[]
for vs,ve,off in EXEC:
    data=blob[off:off+(ve-vs)]
    n=len(data)//4
    for i in range(n):
        insn=struct.unpack_from("<I",data,i*4)[0]
        if (insn&0x9F000000)!=0x90000000: continue
        rd=insn&0x1F
        imm=sxt((((insn>>5)&0x7FFFF)<<2)|((insn>>29)&3),21)
        page=(vs+i*4 & ~0xFFF)+(imm<<12)
        if not (0xCF0000<=page<=0xD70000): continue
        for k in range(1,5):
            if i+k>=n: break
            i2=struct.unpack_from("<I",data,(i+k)*4)[0]
            if (i2&0x7F000000)==0x11000000 and ((i2>>5)&0x1F)==rd:
                imm12=(i2>>10)&0xFFF; sh=(i2>>22)&3
                if sh==1: imm12<<=12
                a=page+(-imm12 if (i2>>30)&1 else imm12)
                if a in targets: hits.append((vs+i*4,targets[a]))
print("  adrp+add hits:",[("%#x"%h[0],h[1]) for h in hits])

# ③ 0x207f04c 的指针槽
sec={s["name"]:s for s in ELF["sh"]}
rel=sec[".rela.dyn"]; n=rel["size"]//24
for i in range(n):
    off,r_info,add=struct.unpack_from("<QQq",blob,rel["off"]+i*24)
    if (r_info&0xFFFFFFFF)==1027 and add in (0x207f04c,0x207f064):
        secof=[s["name"] for s in ELF["sh"] if s["addr"] and s["addr"]<=off<s["addr"]+max(s["size"],1)]
        print("  桩 %#x 的指针槽: r_offset=%#x section=%s"%(add,off,secof))
