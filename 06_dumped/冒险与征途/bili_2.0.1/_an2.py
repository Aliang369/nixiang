
import os,sys
sys.path.insert(0,'/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1')
from _mdread2 import Meta
DLL='/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1/Assembly-CSharp.dll'
m=Meta(DLL)
tds=m.tbl['TypeDef']; mds=m.tbl['MethodDef']; fds=m.tbl['Field']
nmeth=len(mds)+1; nfield=len(fds)+1
def tinfo(rid):
    row=tds[rid-1]
    flags=row[0]; name=m.s(row[1]); ns=m.s(row[2])
    fstart=row[4]; mstart=row[5]
    return dict(rid=rid,flags=flags,name=name,ns=ns,fstart=fstart,mstart=mstart)
def mrange(rid):
    t=tinfo(rid)
    nxt=tds[rid][5] if rid<len(tds) else nmeth
    return t['mstart'], nxt
def frange(rid):
    t=tinfo(rid)
    nxt=tds[rid][4] if rid<len(tds) else nfield
    return t['fstart'], nxt
def full(rid): return m.td_fullname(rid)
# index by fullname
idx={}
for rid in range(1,len(tds)+1):
    idx.setdefault(full(rid),[]).append(rid)
def find_type(name, ns=None):
    cands=[]
    for rid in range(1,len(tds)+1):
        t=tinfo(rid)
        if t['name']!=name: continue
        if ns is not None and t['ns']!=ns: continue
        cands.append(rid)
    return cands

targets_agl=['CBattleActorUtil','CtagHurtHPPara','CHitNotifier','DamageFactory','Character','SimpleCharacter',
 'InteractiveGameObject','StarsCollider','GameObjectManager','ActionGameObjectData','ActionState',
 'SecondaryAttribute','BattleElementCenter','DamageInfo','TSpellHitPara','ActionGameLibraryConfig','ActionCommand_AddProjectile']
targets_glob=['BigInt','DamageEnumerate','CharacterType','DS_Advertising','DS_Privilege','SDKProxy','BaseSDKAdapter',
 'MiaoYueADCallBack','AdMascotManager','ProtocolManager','DataService','EventItemInt','Proto_CSRsp']
res={}
print('=== A. ActionGameLibrary 类型（17）===')
for nm in targets_agl:
    r=find_type(nm,'ActionGameLibrary')
    if not r:
        r2=find_type(nm)
        print('  %-28s NOT in ActionGameLibrary; other-ns hits: %s'%(nm,[(full(x),tinfo(x)['ns']) for x in r2][:5]))
    else:
        for rid in r:
            t=tinfo(rid); a,b=mrange(rid); fa,fb=frange(rid)
            print('  %-28s OK token=0x%08X methods=%d fields=%d nested?=%s'%(nm,0x02000000|rid,b-a,fb-fa,'yes' if (rid-1) in m.nested_of else 'no'))
print()
print('=== B. 无命名空间类型（13）===')
for nm in targets_glob:
    r=find_type(nm,'')
    if not r:
        r2=find_type(nm)
        print('  %-28s NOT global; hits: %s'%(nm,[(full(x),tinfo(x)['ns']) for x in r2][:6]))
    else:
        for rid in r:
            t=tinfo(rid); a,b=mrange(rid); fa,fb=frange(rid)
            print('  %-28s OK token=0x%08X methods=%d fields=%d'%(nm,0x02000000|rid,b-a,fb-fa))

