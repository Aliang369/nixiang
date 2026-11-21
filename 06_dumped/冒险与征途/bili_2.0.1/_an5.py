
import os,sys,struct
sys.path.insert(0,'/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1')
from _mdread2 import Meta
DLL='/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1/Assembly-CSharp.dll'
m=Meta(DLL)
tds=m.tbl['TypeDef']; mds=m.tbl['MethodDef']; fds=m.tbl['Field']
nfield=len(fds)+1; nmeth=len(mds)+1
def find_rid(fullname):
    for rid in range(1,len(tds)+1):
        if m.td_fullname(rid)==fullname: return rid
    return None
def fields_of(rid):
    s=tds[rid-1][4]; e=tds[rid][4] if rid<len(tds) else nfield
    out=[]
    for fid in range(s,e):
        row=fds[fid-1]; flags=row[0]; nm=m.s(row[1]); blob,_=m.blob(row[2])
        saved=m.data; m.data=blob
        try:
            p=0
            if blob and blob[0]==0x06: p=1
            t,_=m.parse_type(p)
        except Exception as ex: t='<err>'
        finally: m.data=saved
        out.append((fid,flags,nm,t))
    return out
# constants map: field rid -> (type,value)
const={}
for c in m.tbl['Constant']:
    ctype=c[0]; parent=c[1]; blobidx=c[2]
    tag=parent&3; rid=parent>>2
    if tag==1:
        blob,_=m.blob(blobidx)
        if ctype in (0x08,): val=struct.unpack('<i',blob)[0]
        elif ctype in (0x09,): val=struct.unpack('<I',blob)[0]
        elif ctype in (0x07,): val=struct.unpack('<H',blob)[0]
        elif ctype in (0x05,): val=blob[0]
        elif ctype in (0x02,): val=bool(blob[0])
        else: val=blob.hex()
        const[rid]=(ctype,val)
def show(full, filt=None):
    rid=find_rid(full)
    print('=== %s  rid=%s token=%s ==='%(full,rid,('0x%08X'%(0x02000000|rid)) if rid else '-'))
    if rid is None: return
    for i,(fid,flags,nm,t) in enumerate(fields_of(rid)):
        if filt and not any(k.lower() in nm.lower() for k in filt): continue
        k=[]
        if flags&0x10:k.append('static')
        if flags&0x40:k.append('literal')
        if constants:=const.get(fid):
            k.append('= %r'%(constants[1],))
        print('   #%-3d %-36s %-30s %s'%(i,nm,t,' '.join(k)))
show('ActionData.ActionCommandData')
show('EventItemInt')
show('ProtocolManager.EventItemInt')
show('DS_Advertising')
show('DataService',['advert','Advertising'])
show('ActionGameLibrary.ActionState',['immune','invincible','damage'])
show('BigInt')
print('=== CharacterType 字面量（序）===')
rid=find_rid('CharacterType')
fs=fields_of(rid)
for i,(fid,flags,nm,t) in enumerate(fs):
    print('   #%-3d %-28s %-20s %s'%(i,nm,t,('= %r'%(const[fid][1],)) if fid in const else ''))
print('=== DamageEnumerate.DamageType 字面量 ===')
rid=find_rid('DamageEnumerate.DamageType')
if rid is None: rid=find_rid('DamageEnumerate+DamageType')
if rid:
    for i,(fid,flags,nm,t) in enumerate(fields_of(rid)):
        print('   #%-3d %-24s %s'%(i,nm,('= %r'%(const[fid][1],)) if fid in const else ''))

