import array, struct
so=open("06_dumped/旷野征途/2.2/libil2cpp.so","rb").read()
TEXT_OFF=0x1748604; TEXT_VA=0x174c604; TEXT_SZ=0x229d09c
text=so[TEXT_OFF:TEXT_OFF+TEXT_SZ]; n=len(text)//4
w=array.array("I"); w.frombytes(text[:n*4])

def is_ldr_w(ins):  return (ins & 0xFFC00000)==0xB9400000
def ldr_rt(ins):    return ins & 0x1F
def ldr_rn(ins):    return (ins>>5) & 0x1F
def is_sub_w(ins):  return (ins & 0xFFE00000)==0x4B000000
def is_bl(ins):     return (ins & 0xFC000000)==0x94000000

hits=[]
for i in range(n-3):
    a,b,c,d = w[i],w[i+1],w[i+2],w[i+3]
    if not (is_ldr_w(a) and is_ldr_w(b) and is_sub_w(c) and is_bl(d)): continue
    # 两次 ldr 必须同基址寄存器（imi），sub 的目标是 w1，且减数/被减数正是这两次载入的寄存器
    if ldr_rn(a) != ldr_rn(b): continue
    rd = c & 0x1F; rn = (c>>5)&0x1F; rm = (c>>16)&0x1F
    if rd != 1: continue
    if {rn, rm} != {ldr_rt(a), ldr_rt(b)}: continue
    hits.append(TEXT_VA + i*4)
print("pattern hits (ldr w / ldr w / sub w1 / bl):", len(hits))
for h in hits[:20]: print("  va=0x%x file=0x%x" % (h, h-0x4000))
