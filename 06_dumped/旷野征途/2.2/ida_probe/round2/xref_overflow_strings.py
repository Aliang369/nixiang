import array
so=open("06_dumped/旷野征途/2.2/libil2cpp.so","rb").read()
TEXT_OFF=0x1748604; TEXT_VA=0x174c604; TEXT_SZ=0x229d09c
text=so[TEXT_OFF:TEXT_OFF+TEXT_SZ]; n=len(text)//4
w=array.array("I"); w.frombytes(text[:n*4])
TARGETS={0xa0bcc0:"AllocStackSlot",0xa5f595:"AllocLocalloc",0xa2e8b0:"AllocFrame"}
def adrp(ins, pc):
    immlo=(ins>>29)&3; immhi=(ins>>5)&0x7FFFF
    imm=(immhi<<2)|immlo
    if imm & (1<<20): imm -= (1<<21)
    return (pc & ~0xFFF) + (imm<<12)
hits={}
for i in range(n-1):
    a,b=w[i],w[i+1]
    if (a & 0x9F000000)!=0x90000000: continue
    if (b & 0x7F800000)!=0x11000000: continue
    if (b & 0x1F)!=((a)&0x1F): continue
    page=adrp(a, TEXT_VA+i*4); imm=(b>>10)&0xFFF
    tgt=page+imm
    if tgt in TARGETS:
        hits.setdefault(TARGETS[tgt],[]).append(TEXT_VA+i*4)
for k,v in hits.items():
    print("%s xrefs=%d" % (k, len(v)), [hex(x) for x in v[:5]])
