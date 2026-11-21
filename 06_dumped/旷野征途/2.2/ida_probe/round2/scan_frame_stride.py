import array
so=open("06_dumped/旷野征途/2.2/libil2cpp.so","rb").read()
TEXT_OFF=0x1748604; TEXT_VA=0x174c604; TEXT_SZ=0x229d09c
text=so[TEXT_OFF:TEXT_OFF+TEXT_SZ]; n=len(text)//4
w=array.array("I"); w.frombytes(text[:n*4])

# add xD, xN, wM, uxtw #6  → 64 字节步进（InterpFrame 结构大小候选）
cands=[]
for i in range(n-8):
    ins=w[i]
    if (ins & 0xFF200000)!=0x8B200000: continue
    if ((ins>>10)&7)!=6: continue
    if ((ins>>13)&7)!=6: continue
    rd=ins & 0x1F
    # 后续 6 条内是否有对 [rd] 的写入（str/stp）
    for k in range(1,7):
        nx=w[i+k]
        if (nx & 0xFFC00000) in (0xB9000000, 0xA9000000, 0xF9000000):
            if ((nx>>5)&0x1F)==rd or ((nx & 0x1F)==rd):
                cands.append(TEXT_VA+i*4); break
print("lsl#6 + store candidates:", len(cands))
for c in cands[:25]: print("  va=0x%x file=0x%x" % (c, c-0x4000))
