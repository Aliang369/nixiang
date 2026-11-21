
import os,struct,hashlib
OUT='/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1'

def carve(body, name_off, tag):
    d=open(body,'rb').read()
    name=b'Assembly-CSharp.dll'
    print('== %s  body=%d  name@0x%X'%(tag,len(d),name_off))
    print('   ctx:', d[name_off-8:name_off+40].hex())
    sf=name_off+len(name)+1
    size=struct.unpack_from('<I',d,sf)[0]
    dll_off=sf+4
    print('   size field @0x%X = %d ; dll @0x%X  magic=%r'%(sf,size,dll_off,d[dll_off:dll_off+2]))
    # PE parse
    mz=dll_off
    pe=mz+struct.unpack_from('<I',d,mz+0x3C)[0]
    assert d[pe:pe+4]==b'PE\0\0'
    mach=struct.unpack_from('<H',d,pe+4)[0]
    nsec=struct.unpack_from('<H',d,pe+6)[0]
    ts=struct.unpack_from('<I',d,pe+8)[0]
    optsz=struct.unpack_from('<H',d,pe+20)[0]
    opt=pe+24
    magic=struct.unpack_from('<H',d,opt)[0]
    nrva=struct.unpack_from('<I',d,opt+92)[0]
    sects=[]; raw_end=0
    for i in range(nsec):
        s=opt+optsz+i*40
        nm=d[s:s+8].rstrip(b'\0').decode('latin-1')
        vsize,vaddr,rsize,raddr=struct.unpack_from('<IIII',d,s+8)
        sects.append((nm,vsize,vaddr,rsize,raddr)); raw_end=max(raw_end,raddr+rsize)
    print('   e_lfanew=0x%X machine=0x%X sections=%d ts=%d optmagic=0x%X nrva=%d rawend=%d'%(pe-mz,mach,nsec,ts,magic,nrva,raw_end))
    for s in sects: print('     %-8s vsize=0x%-8X vaddr=0x%-8X rawsz=0x%-8X rawoff=0x%-8X'%s)
    def r2o(rva):
        for nm,vsize,vaddr,rsize,raddr in sects:
            if vaddr<=rva<vaddr+max(vsize,rsize): return raddr+(rva-vaddr)
        return None
    clr=None
    for i in range(nrva):
        rva,sz=struct.unpack_from('<II',d,opt+96+i*8)
        if i==14: clr=(rva,sz)
    print('   CLR dir:',clr)
    co=r2o(clr[0])
    cbsz,cbmaj,cbmin,mrva,msz=struct.unpack_from('<IHHII',d,mz+co)
    print('   COR20 cb=0x%X runtime=%d.%d MetaDataRVA=0x%X MetaDataSize=0x%X'%(cbsz,cbmaj,cbmin,mrva,msz))
    mo=mz+r2o(mrva)
    print('   BSJB@0x%X bytes=%r'%(mo,d[mo:mo+4]))
    verlen=struct.unpack_from('<I',d,mo+12)[0]
    ver=d[mo+16:mo+16+verlen].rstrip(b'\0').decode('latin-1')
    flags,nstr=struct.unpack_from('<HH',d,mo+16+((verlen+3)//4)*4)
    p=mo+16+((verlen+3)//4)*4+4
    streams={}
    for i in range(nstr):
        so,ss=struct.unpack_from('<II',d,p); p+=8
        e=d.index(b'\0',p); nm=d[p:e].decode('latin-1'); p=(e+1+3)&~3
        streams[nm]=(so,ss)
    print('   metadata version=%r streams=%s'%(ver,streams))
    clen=size if size else raw_end
    dll=d[dll_off:dll_off+clen]
    h=hashlib.sha256(dll).hexdigest()
    out=os.path.join(OUT, tag+'.dll')
    open(out,'wb').write(dll)
    print('   WROTE %s size=%d sha256=%s'%(out,len(dll),h))
    return out,len(dll),h

carve(os.path.join(OUT,'_body_95d30e2b.bin'),0xaf4,'Assembly-CSharp')

