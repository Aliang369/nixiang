
import os,struct,sys
import lz4.block as LZ
ROOT='/Users/aliang/Documents/nixiang/13_scratch/bili_201/assets/dataroot'
OUT='/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1'
def cstr(b,o):
    e=b.index(b'\0',o); return b[o:e].decode('utf-8','replace'), e+1
def dec(blob,usize):
    try: return LZ.decompress(blob, uncompressed_size=usize)
    except Exception: return None
def parse(b, sig):
    o=sig
    try:
        s,o=cstr(b,o)
        ver=struct.unpack_from('>I',b,o)[0]; o+=4
        uver,o=cstr(b,o); urev,o=cstr(b,o)
        size=struct.unpack_from('>q',b,o)[0]; o+=8
        cbs=struct.unpack_from('>I',b,o)[0]; o+=4
        ubs=struct.unpack_from('>I',b,o)[0]; o+=4
        flags=struct.unpack_from('>I',b,o)[0]; o+=4
    except Exception:
        return None
    if ver>20 or cbs>10_000_000 or ubs>50_000_000: return None
    hdr=o; cflag=flags&0x3F
    for s2 in range(hdr, min(hdr+80,len(b))):
        blob=b[s2:s2+cbs]
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
            tc=sum(x[1] for x in blocks)
            ds=len(b)-tc
            if ds<s2+cbs or ds-(s2+cbs)>64: continue
            return dict(ver=ver,urev=urev,flags=flags,cflag=cflag,bi=s2,blocks=blocks,ds=ds,sig=sig,biraw=d)
        except Exception:
            continue
    return None

files=sorted(f for f in os.listdir(ROOT) if f.endswith('.unity3d'))
print('files',len(files))
ok=0; bad=[]
gl=[]
for fn in files:
    b=open(os.path.join(ROOT,fn),'rb').read()
    sigs=[]
    k=b.find(b'UnityFS',0)
    isig=0
    while isig>=0 and isig<0x40:
        sigs.append(isig); isig=b.find(b'UnityFS',isig+1)
    I=None
    for s in sigs:
        I=parse(b,s)
        if I: break
    if not I:
        bad.append(fn); continue
    ok+=1
    gl.append((fn,len(b),I['flags'],I['cflag'],I['sig'],len(I['blocks'])))
    # quick check on uncompressed blocks only
print('parsed',ok,'failed',len(bad),bad)
from collections import Counter
print('flags hist', Counter(x[2] for x in gl))
print('sigpos hist', Counter(x[4] for x in gl))
print('cflag hist', Counter(x[3] for x in gl))

