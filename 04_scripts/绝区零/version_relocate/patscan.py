import numpy as np, sys
path=sys.argv[1] if len(sys.argv)>1 else 'lib/arm64-v8a/libil2cpp.so'
data=open(path,'rb').read()
n=len(data)//4*4
w=np.frombuffer(data[:n],dtype='<u4')
def imm12(a): return (a>>10)&0xFFF
def is_ldr64(a,off): return ((a&0xFFC00000)==0xF9400000)&(imm12(a)==off//8)
def is_ldr32(a,off): return ((a&0xFFC00000)==0xB9400000)&(imm12(a)==off//4)
def is_cbz(a): return (a&0x7F000000)==0x34000000
def reg(a,p): return (int(a[p])>>5)&0x1F
def rt(a,p): return int(a[p])&0x1F
m=[is_ldr64(w,0x28),is_ldr32(w,0x38),is_cbz(w),is_ldr64(w,0x60),is_cbz(w),is_ldr64(w,0x20)]
idx=np.nonzero(m[0])[0]
print('ldr64 #0x28 count:',len(idx))
res=[]
for p in idx:
    p=int(p)
    if p+5>=len(w): continue
    if not(m[1][p+1] and m[2][p+2] and m[3][p+3] and m[4][p+4] and m[5][p+5]): continue
    if reg(w,p)==reg(w,p+1) and rt(w,p)==reg(w,p+3) and rt(w,p+3)==reg(w,p+5):
        res.append(p)
print('chain matches:',len(res))
for p in res[:20]: print('  file_off=%#x rva=%#x'%(p*4,p*4+0x4000))
