import struct, json, collections
SO="/Users/aliang/Documents/nixiang/13_scratch/bili_201/lib/arm64-v8a/libil2cpp.so"
ELF=json.load(open("/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1_il2cpp/_elf_new.json"))
blob=open(SO,"rb").read()
LOADS=[(p["off"],p["vaddr"],p["filesz"]) for p in ELF["ph"] if p["type"]==1]
def v2f(v):
    for off,va,sz in LOADS:
        if va<=v<va+sz: return v+(off-va)
A,B=0x207F798,0x208D9B8
fo=v2f(A); n=(B-A)//4
br=[0,0]  # br xN via register, and blr
brsites=[]
for i in range(n):
    insn=struct.unpack_from("<I",blob,fo+i*4)[0]
    if (insn & 0xFFFFFC1F)==0xD61F0000: brsites.append(A+i*4)   # br xN
print("entry range %#x..%#x insns=%d BR(count)=%d"%(A,B,n,len(brsites)))
print("  BR sites (first 12):",["%#x"%s for s in brsites[:12]])
