
import os,struct,sys
import lz4.block as LZ
ROOT='/Users/aliang/Documents/nixiang/13_scratch/bili_201/assets/dataroot'
OUT='/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1'
def cstr(b,o):
    e=b.index(b'\0',o); return b[o:e].decode('utf-8','replace'), e+1
def dec(blob,usize):
    try: return LZ.decompress(blob, uncompressed_size=usize)
    except Exception: return None
def parse(b):
    sigs=[]; i=b.find(b'UnityFS')
    while i>=0 and i<0x40:
        sigs.append(i); i=b.find(b'UnityFS',i+1)
    for sig in sigs:
        o=sig
        try:
            s,o=cstr(b,o)
            ver=struct.unpack_from('>I',b,o)[0]; o+=4
            uver,o=cstr(b,o); urev,o=cstr(b,o)
            size=struct.unpack_from('>q',b,o)[0]; o+=8
            cbs=struct.unpack_from('>I',b,o)[0]; o+=4
            ubs=struct.unpack_from('>I',b,o)[0]; o+=4
            flags=struct.unpack_from('>I',b,o)[0]; o+=4
        except Exception: continue
        if ver>20 or cbs>10_000_000 or ubs>50_000_000: continue
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
                return dict(ver=ver,urev=urev,flags=flags,cflag=cflag,blocks=blocks,ds=ds,sig=sig,bioff=s2,bi=d)
            except Exception: continue
    return None

def find_pe(d):
    out=[]; k=0
    while True:
        j=d.find(b'MZ',k)
        if j<0 or j+0x40>len(d): break
        pe=struct.unpack_from('<I',d,j+0x3C)[0]
        if 0<pe<0x800 and d[j+pe:j+pe+4]==b'PE\0\0': out.append(j)
        k=j+1
    return out

files=sorted(f for f in os.listdir(ROOT) if f.endswith('.unity3d'))
found=[]
for idx,fn in enumerate(files):
    b=open(os.path.join(ROOT,fn),'rb').read()
    I=parse(b)
    if not I: print('PARSE FAIL',fn); continue
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
    pe=find_pe(d)
    bsjb=d.find(b'BSJB')
    asm=d.find(b'Assembly-CSharp.dll')
    dllc=d.count(b'.dll')
    if pe or bsjb>=0 or asm>=0:
        print('HIT %s size=%d bad=%d PE=%s BSJB=%s asmDll=%s .dll=%d'%(fn,len(b),bad,[hex(x) for x in pe[:3]],hex(bsjb) if bsjb>=0 else '-',hex(asm) if asm>=0 else '-',dllc))
        found.append(fn)
        open(os.path.join(OUT,'_body_%s.bin'%fn[:8]),'wb').write(d)
print('hits',len(found),found)

