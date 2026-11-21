import re
from capstone import Cs, CS_ARCH_ARM64, CS_MODE_ARM

SO="/Users/aliang/Documents/nixiang/06_dumped/旷野征途/2.2/libil2cpp.so"
TEXT_OFF=0x1748604; TEXT_VA=0x174c604
md=Cs(CS_ARCH_ARM64, CS_MODE_ARM)
so=open(SO,"rb").read()

funcs=[]
for line in open("06_dumped/旷野征途/2.2/ida_probe/funcs.txt"):
    m=re.match(r"0x([0-9a-f]+) 0x([0-9a-f]+) 0x([0-9a-f]+) n=(\d+)", line)
    if m: funcs.append((int(m.group(1),16), int(m.group(2),16), int(m.group(3),16), int(m.group(4))))

rows=[]
for s,e,sz,nc in funcs:
    code=so[TEXT_OFF+(s-TEXT_VA):TEXT_OFF+(e-TEXT_VA)]
    subs=0; bls=[]; stores={}; rets=0
    last_bl_idx=[]
    for k,ins in enumerate(md.disasm(code, s)):
        if ins.mnemonic=="sub" and ins.op_str.startswith("w") and ins.op_str.count(",")>=2 and "#" not in ins.op_str:
            subs+=1
        if ins.mnemonic=="bl":
            bls.append(ins.op_str)
        if ins.mnemonic in ("stp","str","stur"):
            m=re.search(r"\[x(\d+)", ins.op_str)
            if m: stores[m.group(1)]=stores.get(m.group(1),0)+1
        if ins.mnemonic=="ret": rets+=1
    maxstore=max(stores.values()) if stores else 0
    score=0
    if subs>0: score+=1
    if len(bls) in (2,3,4): score+=1
    if maxstore>=4: score+=1
    if nc<=3: score+=1
    if sz<=0x220: score+=1
    if score>=4: rows.append((score, len(bls), maxstore, subs, s, sz, nc, bls))

rows.sort(key=lambda r:(-r[0], r[5]))
print("candidates:", len(rows))
for r in rows[:10]:
    print("  score=%d bls=%d maxstore=%d subs=%d va=0x%x size=0x%x ncallers=%d %s" % (r[0],r[1],r[2],r[3],r[4],r[5],r[6], r[7]))
