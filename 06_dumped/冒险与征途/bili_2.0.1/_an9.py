
import os,struct,hashlib
OUT='/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1'
def pe_names(body, offs):
    d=open(body,'rb').read()
    for o in offs:
        ctx=d[max(0,o-64):o]
        # find printable ascii run ending near o
        s=ctx.rstrip(b'\x00')
        i=len(s)-1
        while i>=0 and (32<=s[i]<127): i-=1
        name=s[i+1:]
        print('   PE@0x%X  preceding-name=%r  size_hint=%s'%(o,name, struct.unpack_from('<I',d,o-4)[0] if o>=4 else '-'))
for fn,offs in [('_body_9e2fe9a8.bin',[0xb14,0x2353c,0xf3b6c]),('_body_697730d0.bin',[0xb08])]:
    print('==',fn)
    pe_names(os.path.join(OUT,fn),offs)
print()
for fn in ['_body_748e419b.bin','_body_998eb4e9.bin','_body_14f6e1a8.bin']:
    p=os.path.join(OUT,fn); d=open(p,'rb').read()
    i=d.find(b'BSJB')
    print('==',fn,'len',len(d),'BSJB@0x%X'%i)
    print('   head32:',d[:32].hex())
    # printable strings near BSJB
    print('   ctx:',repr(d[max(0,i-16):i+32]))
print()
for f in ['95d30e2bdf8fbf25564bd77272c8c242']:
    p='/Users/aliang/Documents/nixiang/13_scratch/bili_201/assets/dataroot/%s.unity3d'%f
    b=open(p,'rb').read()
    print('container',f,'size',len(b),'sha256',hashlib.sha256(b).hexdigest())
d=open(os.path.join(OUT,'_body_95d30e2b.bin'),'rb').read()
print('body size',len(d),'dll entry ctx:',d[0xAE0:0xB20].hex())

