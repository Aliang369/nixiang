
import os,sys,struct
sys.path.insert(0,'/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1')
from _mdread2 import Meta
OUT='/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1'
tmp=os.path.join(OUT,'_tmp_sib.dll')
jobs=[('_body_9e2fe9a8.bin',[0xb14,0x2353c,0xf3b6c]),('_body_697730d0.bin',[0xb08])]
for fn,offs in jobs:
    d=open(os.path.join(OUT,fn),'rb').read()
    for o in offs:
        size=struct.unpack_from('<I',d,o-4)[0]
        blob=d[o:o+size]
        open(tmp,'wb').write(blob)
        try:
            m=Meta(tmp)
            print('%s PE@0x%X size=%d asm=%r TypeDef=%d MethodDef=%d md=%s'%(fn,o,size,m.asm_name,len(m.tbl['TypeDef']),len(m.tbl['MethodDef']),m.md_version))
        except Exception as e:
            print('%s PE@0x%X size=%d PARSE ERR %s'%(fn,o,size,e))
os.remove(tmp)

