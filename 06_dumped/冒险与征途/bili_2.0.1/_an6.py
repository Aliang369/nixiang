
import os,sys,struct
sys.path.insert(0,'/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1')
from _mdread2 import Meta
DLL='/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1/Assembly-CSharp.dll'
m=Meta(DLL)
tds=m.tbl['TypeDef']; mds=m.tbl['MethodDef']; fds=m.tbl['Field']
nfield=len(fds)+1; nmeth=len(mds)+1
def find_rid(fn):
    for rid in range(1,len(tds)+1):
        if m.td_fullname(rid)==fn: return rid
    return None
def fields_of(rid):
    s=tds[rid-1][4]; e=tds[rid][4] if rid<len(tds) else nfield
    out=[]
    for fid in range(s,e):
        row=fds[fid-1]; flags=row[0]; nm=m.s(row[1]); blob,_=m.blob(row[2])
        saved=m.data; m.data=blob
        try:
            p=1 if (blob and blob[0]==6) else 0
            t,_=m.parse_type(p)
        except Exception: t='<err>'
        finally: m.data=saved
        out.append((fid,flags,nm,t))
    return out
const={}
for c in m.tbl['Constant']:
    ctype=c[0]; parent=c[1]; blobidx=c[2]
    tag=parent&3; rid=parent>>2
    if tag!=0: continue     # HasConstant: 0=Field,1=Param,2=Property
    blob,_=m.blob(blobidx)
    try:
        if ctype==0x08: val=struct.unpack('<i',blob)[0]
        elif ctype==0x09: val=struct.unpack('<I',blob)[0]
        elif ctype==0x07: val=struct.unpack('<H',blob)[0]
        elif ctype==0x05: val=blob[0]
        elif ctype==0x02: val=bool(blob[0])
        elif ctype in (0x0A,): val=struct.unpack('<q',blob)[0]
        else: val=blob.hex()
    except Exception: val=blob.hex()
    const[rid]=(ctype,val)
print('constant rows field-tagged:',len(const))
print('=== CharacterType literals ===')
rid=find_rid('CharacterType')
for i,(fid,flags,nm,t) in enumerate(fields_of(rid)):
    v=const.get(fid)
    print('   #%-3d %-32s %s'%(i,nm,('= %r'%(v[1],)) if v else ''))
print()
for cand in ('ActionGameLibrary.ActionCommandData','ActionData.ActionCommandData','ActionCommandData'):
    rid=find_rid(cand)
    if rid:
        print('=== %s rid=%d token=0x%08X ==='%(cand,rid,0x02000000|rid))
        for i,(fid,flags,nm,t) in enumerate(fields_of(rid)):
            if nm in ('data','_data','actionGameObject','action','_actionGameObject') or 'gameobject' in nm.lower():
                print('   #%-3d %-32s %-40s'%(i,nm,t))
        print('   (all field names sample)',[x[2] for x in fields_of(rid)][:18])
        break
print()
print('=== FieldLayout rows for target types ===')
fl=m.tbl.get('FieldLayout',[])
print('  FieldLayout rows:',len(fl))
print('  ClassLayout rows:',len(m.tbl.get('ClassLayout',[])))
print()
print('=== DS_Advertising 全部方法 ===')
rid=find_rid('DS_Advertising')
s=tds[rid-1][5]; e=tds[rid][5] if rid<len(tds) else nmeth
for mid in range(s,e):
    row=mds[mid-1]; blob,_=m.blob(row[4])
    try: ret,params,gen=m.sig_blob(blob)
    except Exception: ret,params=' <err>',[]
    print('   %-34s argc=%d ret=%-28s %s'%(m.s(row[3]),len(params),ret,'static' if row[2]&0x10 else 'instance'))

