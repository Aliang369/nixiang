import os
B=r"E:\nixiang\06_dumped\冒险与征途"
apk=open(os.path.join(B,r"apk_extract\lib\arm64-v8a\libil2cpp.so"),"rb").read()
raw=b"".join(open(os.path.join(B,"mem_il2cpp",f"il2cpp_mem_{i}.bin"),"rb").read() for i in range(1,7))
BASE=0x400023200000
# PT_LOAD (off, vaddr, filesz) from phdrs
LOADS=[(0x00000000,0x00000000,0x1D7308C),
       (0x01D7308C,0x01D7408C,0x2F8B204),
       (0x04CFE290,0x04D00290,0x347388),
       (0x05045618,0x05048618,0x2B5AB0)]
# dump covers runtime BASE..BASE+len(raw)
print("dump covers runtime 0x%X..0x%X"%(BASE,BASE+len(raw)))
tot=0; regions=[]
for off,vaddr,filesz in LOADS:
    delta=off-vaddr
    # runtime range for this load
    rs=BASE+vaddr; re_=rs+filesz
    rs=max(rs,BASE); re_=min(re_,BASE+len(raw))
    if re_<=rs: continue
    fo=rs-BASE+delta  # file offset of rs
    seg=bytes(raw[rs-BASE:re_-BASE])
    disk=apk[fo:fo+len(seg)]
    d=[i for i in range(len(seg)) if seg[i]!=disk[i]]
    print("LOAD off=0x%X vaddr=0x%X runtime 0x%X..0x%X fileoff 0x%X  ndiff=%d"%(off,vaddr,rs,re_,fo,len(d)))
    tot+=len(d)
    i=0
    while i<len(d):
        j=i
        while j+1<len(d) and d[j+1]==d[j]+1: j+=1
        regions.append((fo+d[i], d[j]-d[i]+1)); i=j+1
print("total diff bytes",tot,"regions",len(regions))
regions.sort()
for o,l in regions[:40]:
    print("  0x%08X len=%d"%(o,l))
    print("     disk:",apk[o:o+min(24,l+8)].hex())
    print("     mem :",raw[o-(0 if o<0x1D7408C else 0):0][:0].hex() or "")
