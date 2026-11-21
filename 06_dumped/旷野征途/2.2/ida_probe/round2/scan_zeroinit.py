import array
so=open("06_dumped/旷野征途/2.2/libil2cpp.so","rb").read()
TEXT_OFF=0x1748604; TEXT_VA=0x174c604; TEXT_SZ=0x229d09c
text=so[TEXT_OFF:TEXT_OFF+TEXT_SZ]; n=len(text)//4
w=array.array("I"); w.frombytes(text[:n*4])
def stp_zr(ins):  # stp xzr, xzr, [xN] / stp xzr,xzr,[xN,#imm]  (64-bit)
    return (ins & 0xFFC07FFF)==0xA9007FFF
def is_bl(ins): return (ins & 0xFC000000)==0x94000000
def is_store(ins): return (ins & 0x3B000000)==0x39000000  # str/stp family 简化
out=[]
for i in range(n-2):
    if not stp_zr(w[i]): continue
    bl_near = any(is_bl(w[j]) for j in range(max(0,i-12), min(n,i+3)))
    stores = sum(1 for j in range(max(0,i-8), min(n,i+8)) if is_store(w[j]))
    if bl_near and stores>=4:
        out.append((TEXT_VA+i*4, stores))
print("zero-init frame candidates:", len(out))
for va,st in out[:20]: print("  va=0x%x file=0x%x stores_near=%d" % (va, va-0x4000, st))
