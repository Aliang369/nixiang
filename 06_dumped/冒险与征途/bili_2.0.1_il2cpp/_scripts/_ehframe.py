# -*- coding: utf-8 -*-
"""顺序解析 .eh_frame 全部 FDE -> 函数区间表（只读）"""
import struct, json, bisect
SO="/Users/aliang/Documents/nixiang/13_scratch/bili_201/lib/arm64-v8a/libil2cpp.so"
ELF=json.load(open("/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1_il2cpp/_elf_new.json"))
sec={s["name"]:s for s in ELF["sh"]}
h=sec[".eh_frame"]
blob=open(SO,"rb").read()
base=h["off"]; base_va=h["addr"]; end=base+h["size"]

def uleb(p):
    r=0;s=0
    while True:
        b=blob[p];p+=1
        r|=(b&0x7f)<<s;s+=7
        if not b&0x80: return r,p
def sleb(p):
    r=0;s=0
    while True:
        b=blob[p];p+=1
        r|=(b&0x7f)<<s;s+=7
        if not b&0x80:
            if b&0x40: r-=1<<s
            return r,p
def rd_enc(p,enc,field_va):
    f=enc&0x0f
    if f==0x00: v=struct.unpack_from("<Q",blob,p)[0]; return v,p+8
    if f==0x01: v=struct.unpack_from("<H",blob,p)[0]; return v,p+2
    if f==0x02: v=struct.unpack_from("<I",blob,p)[0]; return v,p+4
    if f==0x03: v=struct.unpack_from("<Q",blob,p)[0]; return v,p+8
    if f==0x09: v=struct.unpack_from("<q",blob,p)[0]; return v,p+8
    if f==0x0a: v=struct.unpack_from("<h",blob,p)[0]; return v,p+2
    if f==0x0b:
        v=struct.unpack_from("<i",blob,p)[0]
        if enc&0x70==0x10: v+=field_va
        return v,p+4
    raise SystemExit("enc %#x"%enc)

cies={}
fdes=[]
p=base
while p+4<=end:
    ln=struct.unpack_from("<I",blob,p)[0]
    if ln==0: break
    rec_end=p+4+ln
    cid=struct.unpack_from("<I",blob,p+4)[0]
    if cid==0:
        q=p+8
        ver=blob[q];q+=1
        e=blob.find(b"\0",q); aug=blob[q:e].decode("latin1"); q=e+1
        if ver>=4: q+=2
        _,q=uleb(q); _,q=sleb(q)
        if ver==1: q+=1
        else: _,q=uleb(q)
        enc=0
        if aug.startswith("z"):
            alen,q=uleb(q); aend=q+alen
            for ch in aug[1:]:
                if ch=="R": enc=blob[q]; q+=1
                elif ch=="L": q+=1
                elif ch=="P":
                    e2=blob[q]; q+=1; _,q=rd_enc(q,e2,0)
                elif ch=="S": pass
                else: break
            q=aend
        cies[base-p]=dict(ver=ver,enc=enc)
    else:
        cie_off=p+4-cid
        c=cies.get(cie_off)
        if c and c["enc"]:
            fb,fv=rd_enc(p+8,c["enc"],p+8)
            fr,_=rd_enc(fv,c["enc"]&0x0f,0)
            if fr:
                fdes.append((fb,fr))
    p=rec_end
fdes.sort()
print("FDE count:",len(fdes))
starts=[f[0] for f in fdes]
def lookup(x):
    i=bisect.bisect_right(starts,x)-1
    if i<0: return None
    a,l=fdes[i]
    if a<=x<a+l: return (a,l,a+l)
    return (a,l,a+l,"NOT-COVERED")
for t in (0x207F798,0x208AD70,0x208BD78,0x208E018,0x207F640,0x207DCCC,0x207DE04,0x207F04C,0x207F064,0x207DD40,0x207DDAC,0x205FF04,0x21C7950):
    r=lookup(t)
    print("  %#x -> %s"%(t, ("%#x..%#x len=%#x%s"%(r[0],r[2],r[1],"  <-- NOT COVERED" if len(r)>3 else "")) if r else "?"))
json.dump([[a,l] for a,l in fdes],open("/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1_il2cpp/_fde.json","w"))
print("wrote _fde.json")
