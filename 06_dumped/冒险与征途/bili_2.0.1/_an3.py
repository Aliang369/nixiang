
import os,sys
sys.path.insert(0,'/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1')
from _mdread2 import Meta
DLL='/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1/Assembly-CSharp.dll'
m=Meta(DLL)
tds=m.tbl['TypeDef']; mds=m.tbl['MethodDef']
nmeth=len(mds)+1
# build type spans
spans={}
for rid in range(1,len(tds)+1):
    row=tds[rid-1]
    s=row[5]; e=tds[rid][5] if rid<len(tds) else nmeth
    spans[rid]=(s,e)
def msig(rid):
    row=mds[rid-1]
    rva,impl,flags=row[0],row[1],row[2]
    name=m.s(row[3])
    blob,_=m.blob(row[4])
    try: ret,params,gen=m.sig_blob(blob)
    except Exception as e: ret,params,gen=('<sigerr %s>'%e),[], []
    return dict(rid=rid,rva=rva,flags=flags,impl=impl,name=name,ret=ret,params=params)
# name -> list of (typefullname, rid)
byname={}
for rid in range(1,len(tds)+1):
    s,e=spans[rid]
    fn=m.td_fullname(rid)
    for mid in range(s,e):
        nm=m.s(mds[mid-1][3])
        byname.setdefault(nm,[]).append((fn,mid))
def lookup(cls, meth):
    # resolve class
    cands=[]
    for rid in range(1,len(tds)+1):
        fn=m.td_fullname(rid)
        if fn==cls or fn.endswith('.'+cls) or m.td_name[rid-1]==cls:
            if cls in (fn, m.td_name[rid-1]) or fn.endswith('.'+cls):
                cands.append((rid,fn))
    out=[]
    for rid,fn in cands:
        s,e=spans[rid]
        for mid in range(s,e):
            if m.s(mds[mid-1][3])==meth:
                out.append((fn,msig(mid)))
    return out
targets=[
 ('ActionGameLibrary.CBattleActorUtil','Hurt'),
 ('ActionGameLibrary.StarsCollider','DoAttack'),
 ('ActionGameLibrary.Character','Damage'),
 ('ActionGameLibrary.BattleElementCenter','get_localDamage'),
 ('ActionGameLibrary.ActionCommand_AddProjectile','Execute'),
 ('ActionGameLibrary.ActionCommand_AddProjectile','AddProjectile'),
 ('DS_Advertising','OnGetADInfoRsp'),
 ('DS_Advertising','OnADVideoResult'),
 ('DS_Advertising','DispatchResult'),
 ('ProtocolManager','sendEvent'),
 ('BaseSDKAdapter','PlayADVideo'),
]
for cls,meth in targets:
    res=lookup(cls,meth)
    if not res:
        others=byname.get(meth,[])
        print('MISS  %s::%s  -> 同名方法存在于: %s'%(cls,meth,[o[0] for o in others][:12]))
    else:
        for fn,sig in res:
            st='static' if (sig['flags']&0x10) else 'instance'
            print('HIT   %s::%s  token=0x%06X argc=%d %s ret=%s params=(%s) rva=0x%X'%(
                fn,meth,0x06000000|sig['rid'],len(sig['params']),st,sig['ret'],', '.join(sig['params']),sig['rva']))
print()
print('=== 所有名为 PlayADVideo 的方法 ===')
for fn,mid in byname.get('PlayADVideo',[]):
    sig=msig(mid)
    st='static' if (sig['flags']&0x10) else 'instance'
    print('  %-40s token=0x%06X argc=%d %s ret=%s params=(%s) rva=0x%X'%(fn,0x06000000|mid,len(sig['params']),st,sig['ret'],', '.join(sig['params']),sig['rva']))
print()
print('=== BaseSDKAdapter 子类（扫描 Extends==BaseSDKAdapter 的类型）===')
bid=[rid for rid in range(1,len(tds)+1) if m.td_fullname(rid)=='BaseSDKAdapter']
print('  BaseSDKAdapter rid', bid)
if bid:
    base=bid[0]
    coded=(1<<2)|base   # TypeDefOrRef tag 0 => TypeDef, coded index = rid<<2
    for rid in range(1,len(tds)+1):
        ext=tds[rid-1][3]
        if ext==coded:
            s,e=spans[rid]
            names=[m.s(mds[mid-1][3]) for mid in range(s,e)]
            print('  subclass %-36s token=0x%08X methods=%d hasPlayADVideo=%s'%(m.td_fullname(rid),0x02000000|rid,e-s,'PlayADVideo' in names))

