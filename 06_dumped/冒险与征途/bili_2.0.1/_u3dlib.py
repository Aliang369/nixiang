
import os, struct, sys, hashlib

ROOT='/Users/aliang/Documents/nixiang/13_scratch/bili_201/assets/dataroot'

def lz4_dec(src, limit=1<<30):
    out=bytearray(); i=0; n=len(src)
    while i<n:
        token=src[i]; i+=1
        ll=token>>4
        if ll==15:
            while True:
                c=src[i]; i+=1; ll+=c
                if c!=255: break
        out+=src[i:i+ll]; i+=ll
        if i>=n: break
        if i+2>n: break
        off=src[i]|(src[i+1]<<8); i+=2
        ml=token&0xF
        if ml==15:
            while True:
                c=src[i]; i+=1; ml+=c
                if c!=255: break
        ml+=4
        st=len(out)-off
        if off<=0 or st<0: raise ValueError('bad-off %d at %d'%(off,len(out)))
        if len(out)+ml>limit: raise ValueError('limit')
        for k in range(ml): out.append(out[st+k])
    return bytes(out)

def cstr(b,o):
    e=b.index(b'\0',o); return b[o:e].decode('utf-8','replace'), e+1

def try_bi(chunk,cflag,ubs):
    try:
        if cflag==0: d=chunk
        elif cflag in (2,3): d=lz4_dec(chunk)
        elif cflag==1:
            import lzma; d=lzma.decompress(chunk)
        else: return None
    except Exception:
        return None
    return d if len(d)==ubs else None

def parse(path):
    b=open(path,'rb').read()
    sigoff=b.find(b'UnityFS')
    if sigoff<0 or sigoff>0x100: raise ValueError('no UnityFS sig')
    o=sigoff
    sig,o=cstr(b,o)
    ver=struct.unpack_from('>I',b,o)[0]; o+=4
    uver,o=cstr(b,o); urev,o=cstr(b,o)
    size=struct.unpack_from('>q',b,o)[0]; o+=8
    cbs=struct.unpack_from('>I',b,o)[0]; o+=4
    ubs=struct.unpack_from('>I',b,o)[0]; o+=4
    flags=struct.unpack_from('>I',b,o)[0]; o+=4
    hdr_end=o; cflag=flags&0x3F
    best=None
    for s in range(hdr_end, min(hdr_end+80,len(b))):
        d=try_bi(b[s:s+cbs],cflag,ubs)
        if d is None: continue
        try:
            p=16 if ver>=7 else 0
            nb=struct.unpack_from('>I',d,p)[0]
            if not (0<nb<20000): continue
            q=p+4
            blocks=[struct.unpack_from('>IIH',d,q+i*10) for i in range(nb)]
            q+=nb*10
            nn=struct.unpack_from('>I',d,q)[0]
            if nn>1000: continue
            tc=sum(x[1] for x in blocks)
            ds=len(b)-tc
            if ds < s+cbs: continue
            if ds - (s+cbs) > 32: continue
            best=(s,d,blocks,nn); break
        except Exception:
            continue
    if best is None: raise ValueError('blocksInfo not found')
    s,bi,blocks,nn=best
    p=16 if ver>=7 else 0
    p+=4+len(blocks)*10
    nn2=struct.unpack_from('>I',bi,p)[0]; p+=4
    nodes=[]
    for i in range(nn2):
        fo,fs,ff=struct.unpack_from('>qqI',bi,p); p+=16
        s2,p=cstr(bi,p); nodes.append((fo,fs,ff,s2))
    B={}
    B.update(dict(b=b,ver=ver,uver=uver,urev=urev,size=size,flags=flags,cflag=cflag,
                  hdr_end=hdr_end,bi_off=s,cbs=cbs,ubs=ubs,blocks=blocks,nodes=nodes,
                  data_start=len(b)-sum(x[1] for x in blocks),path=path,
                  fname=os.path.basename(path)))
    return B

def decompress(B):
    b=B['b']; data=bytearray(); pos=B['data_start']; bad=0; codecs={}
    for i,(u,c,f) in enumerate(B['blocks']):
        blob=b[pos:pos+c]; pos+=c
        cf=f&0x3F; codecs[cf]=codecs.get(cf,0)+1
        try:
            if cf==0: d=blob
            elif cf in (2,3): d=lz4_dec(blob)
            elif cf==1:
                import lzma; d=lzma.decompress(blob)
            else: raise ValueError('codec %d'%cf)
        except Exception as e:
            d=b''; bad+=1
        if len(d)!=u: bad+=1
        data+=d
    return bytes(data), bad, codecs

def find_pe(d):
    out=[]; idx=0
    while True:
        i=d.find(b'MZ',idx)
        if i<0 or i+0x40>len(d): break
        peoff=struct.unpack_from('<I',d,i+0x3C)[0]
        if 0<peoff<0x800 and d[i+peoff:i+peoff+4]==b'PE\0\0':
            out.append(i)
        idx=i+1
    return out

if __name__=='__main__':
    cands=sys.argv[1:] or ['748e419b8fac64fd48919573141c5cef','95d30e2bdf8fbf25564bd77272c8c242','998eb4e91af19a4c14e3f9089b711f35','9e2fe9a8495723eaa95c33a5c6e0d635']
    for fn in cands:
        p=os.path.join(ROOT,fn+'.unity3d')
        try: B=parse(p)
        except Exception as e:
            print('==',fn,'PARSE ERR',e); continue
        print('==',fn,os.path.getsize(p),'ver',B['ver'],B['urev'],'flags 0x%X'%B['flags'],'cflag',B['cflag'],'bi_off 0x%X'%B['bi_off'],'ds 0x%X'%B['data_start'],'nb',len(B['blocks']),'nn',len(B['nodes']))
        print('   nodes',[(n[0],n[1],n[3]) for n in B['nodes']])
        d,bad,codecs=decompress(B)
        print('   decompressed',len(d),'bad',bad,'codecs',codecs)
        pe=find_pe(d)
        print('   PE:',[hex(x) for x in pe[:6]],'total',len(pe))
        print('   BSJB:',hex(d.find(b'BSJB')))
        print('   asm-cs:',hex(d.find(b'Assembly-CSharp\x00')),'dll:',hex(d.find(b'Assembly-CSharp.dll')))
        open('/tmp/%s.body'%fn,'wb').write(d)

