import sys, difflib
from capstone import *
a=open('v320/lib/arm64-v8a/libil2cpp.so','rb').read()
b=open('lib/arm64-v8a/libil2cpp.so','rb').read()
md=Cs(CS_ARCH_ARM64,CS_MODE_ARM); md.detail=True
def ins(data,start,ln=0x900):
    return list(md.disasm(data[start:start+ln],start))
def disps(il):
    out=[]
    for i in il:
        for o in i.operands:
            if o.type==CS_OP_MEM:
                m=o.mem
                base='sp' if (m.base and i.reg_name(m.base)=='sp') else 'm'
                out.append('%s%+d'%(base,m.disp))
    return out
def show(tag,data,start,n=8):
    il=ins(data,start)
    print(' %-22s rva=%#x insns=%d disps~%d'%(tag,start+0x4000,len(il),len(disps(il))))
    for i in il[:n]: print('     %s %s'%(i.mnemonic,i.op_str))
    return il
for line in sys.argv[1:]:
    name,ts,cs=line.split(',')
    t=int(ts,16); c=int(cs,16)
    ti=show('3.2.0 '+name,a,t)
    ci=show('3.3.0 '+name,b,c,0)
    print('   disp-seq ratio=%.3f'%difflib.SequenceMatcher(None,disps(ti),disps(ci),autojunk=False).ratio())
    print()
