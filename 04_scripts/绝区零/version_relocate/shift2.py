import sys, collections, difflib
from capstone import *
a=open('v320/lib/arm64-v8a/libil2cpp.so','rb').read()
b=open('lib/arm64-v8a/libil2cpp.so','rb').read()
md=Cs(CS_ARCH_ARM64,CS_MODE_ARM); md.detail=True
REL={'adrp','adr','b','bl','cbz','cbnz','tbz','tbnz'}
def norm(i):
    mn=i.mnemonic; ops=[]
    br=i.group(CS_GRP_JUMP) or mn in ('cbz','cbnz','tbz','tbnz','bl','b')
    for o in i.operands:
        if o.type==CS_OP_IMM: ops.append('T' if (mn in ('adrp','adr') or br) else '#%d'%o.imm)
        elif o.type==CS_OP_MEM:
            m=o.mem
            ops.append('[sp%+d]'%m.disp if (m.base and i.reg_name(m.base)=='sp') else '[m%+d]'%m.disp)
        elif o.type==CS_OP_REG: ops.append('r')
    return mn+':'+','.join(ops)
def insns(data,start,ln=0x800):
    return list(md.disasm(data[start:start+ln],start))
def shift_vote(start,ln):
    T=insns(a,start,ln)
    votes=collections.Counter()
    seen=set(); n=0
    for i in T:
        if i.mnemonic in REL or i.group(CS_GRP_JUMP) or i.mnemonic.startswith('b.'): continue
        bb=bytes(i.bytes)
        if len(bb)<4 or bb in seen: continue
        seen.add(bb); n+=1
        j=0
        while True:
            j=b.find(bb,j)
            if j<0: break
            votes[j-i.address]+=1
            j+=1
        if n>=50: break
    return votes,T
for name,start in [(s,{'无敌':0x1152104c,'无敌site':0x11521138,'无限技能':0x11541340,'倍率':0xc02a21c,'定怪':0x19ce6084}[s]) for s in sys.argv[1:]]:
    votes,T=shift_vote(start,0x800)
    print('===',name,hex(start+0x4000),'anchors',sum(1 for _ in T))
    for sh,c in votes.most_common(4):
        D=insns(b,start+sh,0x800)
        A=[norm(x) for x in T]; B=[norm(x) for x in D]
        r=difflib.SequenceMatcher(None,A,B,autojunk=False).ratio()
        print('   shift=%#x votes=%d new_rva=%#x ratio=%.3f'%(sh,c,(start+sh)+0x4000,r))
