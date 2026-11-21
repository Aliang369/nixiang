
import os,struct,sys,hashlib
import lz4.block as LZ
ROOT='/Users/aliang/Documents/nixiang/13_scratch/bili_201/assets/dataroot'
OUT='/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1'
def cstr(b,o):
    e=b.index(b'\0',o); return b[o:e].decode('utf-8','replace'), e+1
def dec(blob,usize):
    try:
        return LZ.decompress(blob, uncompressed_size=usize)
    except Exception:
        return None
def parse(b):
    i=b.find(b'UnityFS')
    if i<0 or i>0x100: return None
    o=i
    sig,o=cstr(b,o)
    ver=struct.unpack_from('>I',b,o)[0]; o+=4
    uver,o=cstr(b,o); urev,o=cstr(b,o)
    size=struct.unpack_from('>q',b,o)[0]; o+=8
    cbs=struct.unpack_from('>I',b,o)[0]; o+=4
    ubs=struct.unpack_from('>I',b,o)[0]; o+=4
    flags=struct.unpack_from('>I',b,o)[0]; o+=4
    hdr=o; cflag=flags&0x3F
    best=None
    for s in range(hdr, min(hdr+80,len(b))):
        blob=b[s:s+cbs]
        if cflag==0: d=blob
        elif cflag in (2,3): d=dec(blob,ubs)
        else: d=None
        if not d or len(d)!=ubs: continue
        try:
            p=16 if ver>=7 else 0
            nb=struct.unpack_from('>I',d,p)[0]
            if not (0<nb<200000): continue
            q=p+4
            blocks=[struct.unpack_from('>IIH',d,q+k*10) for k in range(nb)]
            q+=nb*10
            nn=struct.unpack_from('>I',d,q)[0]
            if nn>20000: continue
            tc=sum(x[1] for x in blocks)
            ds=len(b)-tc
            if ds<s+cbs or ds-(s+cbs)>64: continue
            best=(s,blocks,nn,d); break
        except Exception:
            continue
    if not best: return None
    return dict(ver=ver,urev=urev,flags=flags,cflag=cflag,bi=best[0],blocks=best[1],nn=best[2],ds=len(b)-sum(x[1] for x in best[1]),biraw=best[3],cbs=cbs,ubs=ubs,size=size,fsize=len(b))

hits=[]
files=sorted(os.listdir(ROOT))
print('total files',len(files))
for idx,fn in enumerate(files):
    if not fn.endswith('.unity3d'): continue
    p=os.path.join(ROOT,fn)
    b=open(p,'rb').read()
    try: I=parse(b)
    except Exception as e:
        continue
    if not I: 
        print('NOHEADER',fn); continue
    data=bytearray(); pos=I['ds']; bad=0
    for (u,c,f) in I['blocks']:
        blob=b[pos:pos+c]; pos+=c
        cf=f&0x3F
        if cf==0: d=blob
        elif cf in (2,3):
            d=dec(blob,u)
            if d is None: d=b''; bad+=1
        else: d=b''; bad+=1
        data+=d
    d=bytes(data)
    pes=[]; k=0
    while True:
        j=d.find(b'MZ',k)
        if j<0 or j+0x40>len(d): break
        pe=struct.unpack_from('<I',d,j+0x3C)[0]
        if 0<pe<0x800 and d[j+pe:j+pe+4]==b'PE\0\0': pes.append(j)
        k=j+1
    has_bsjb=d.find(b'BSJB')
    asm=d.find(b'Assembly-CSharp.dll')
    if pes or (has_bsjb>=0 and asm>=0) or asm>=0:
        print('HIT %s size=%d bad=%d PE=%s BSJB=%s asm=%s'%(fn,len(b),bad,[hex(x) for x in pes[:3]],hex(has_bsjb) if has_bsjb>=0 else None,hex(asm) if asm>=0 else None))
        open(os.path.join(OUT,'_body_%s.bin'%fn[:8]),'wb').write(d)
        hits.append((fn,len(b),pes,has_bsjb,asm))
print('done hits',len(hits))

