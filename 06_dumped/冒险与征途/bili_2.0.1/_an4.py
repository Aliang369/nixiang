
import os,sys,struct
sys.path.insert(0,'/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1')
from _mdread2 import Meta
DLL='/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1/Assembly-CSharp.dll'
m=Meta(DLL)
tds=m.tbl['TypeDef']; mds=m.tbl['MethodDef']; fds=m.tbl['Field']
nmeth=len(mds)+1; nfield=len(fds)+1
tname=m.td_name
def tinfo(rid):
    row=tds[rid-1]
    return dict(rid=rid,flags=row[0],name=m.s(row[1]),ns=m.s(row[2]),fstart=row[4],mstart=row[5])
def find_rid(fullname):
    for rid in range(1,len(tds)+1):
        if m.td_fullname(rid)==fullname: return rid
    return None
def fields_of(rid):
    s=tds[rid-1][4]; e=tds[rid][4] if rid<len(tds) else nfield
    out=[]
    for fid in range(s,e):
        row=fds[fid-1]
        flags=row[0]; nm=m.s(row[1]); blob,_=m.blob(row[2])
        saved=m.data; m.data=blob
        try:
            p=0
            if blob and blob[0]==0x06: p=1
            t,_=m.parse_type(p)
        except Exception as ex: t='<err %s>'%ex
        finally: m.data=saved
        out.append((fid,flags,nm,t))
    return out
print('--- BaseSDKAdapter 子类（正确解码 Extends）---')
base=find_rid('BaseSDKAdapter')
for rid in range(1,len(tds)+1):
    ext=tds[rid-1][3]
    tag=ext&3; r=ext>>2
    if tag==0 and r==base:
        t=tinfo(rid); s,e=t['mstart'],(tds[rid][5] if rid<len(tds) else nmeth)
        names=[m.s(mds[mid-1][3]) for mid in range(s,e)]
        print('  subclass %-34s token=0x%08X methods=%d PlayADVideo=%s'%(m.td_fullname(rid),0x02000000|rid,e-s,'PlayADVideo' in names))
print()
print('--- 名字含 SDK / Bili / Ad 的类型（去重，含方法数）---')
pats=('Bili','SDK','Ad')
for rid in range(1,len(tds)+1):
    t=tinfo(rid); nm=t['name']
    if any(p.lower() in nm.lower() for p in ('bili','sdkproxy','sdkadapter')):
        s,e=t['mstart'],(tds[rid][5] if rid<len(tds) else nmeth)
        print('  %-40s ns=%-18s token=0x%08X methods=%d'%(m.td_fullname(rid),t['ns'],0x02000000|rid,e-s))
print()
for full in ['ActionGameLibrary.CtagHurtHPPara','BigInt','ActionData.LAddProjectile','ActionData.ActionCommandData','EventItemInt','ActionGameLibrary.ActionState']:
    rid=find_rid(full)
    print('=== fields of %s (rid=%s) ==='%(full,rid))
    if rid is None:
        print('  TYPE MISSING'); continue
    for i,(fid,flags,nm,t) in enumerate(fields_of(rid)):
        kind=[]
        if flags&0x10: kind.append('static')
        if flags&0x40: kind.append('literal')
        if flags&0x20: kind.append('initonly')
        print('   #%-3d %-34s %-24s %s'%(i,nm,t,','.join(kind)))

