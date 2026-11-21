
import os,sys
sys.path.insert(0,'/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1')
from _mdread2 import Meta
DLL='/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1/Assembly-CSharp.dll'
m=Meta(DLL)
tds=m.tbl['TypeDef']; mds=m.tbl['MethodDef']; fds=m.tbl['Field']
nfield=len(fds)+1; nmeth=len(mds)+1
spans={}
for rid in range(1,len(tds)+1):
    s=tds[rid-1][5]; e=tds[rid][5] if rid<len(tds) else nmeth
    spans[rid]=(s,e)
fspans={}
for rid in range(1,len(tds)+1):
    s=tds[rid-1][4]; e=tds[rid][4] if rid<len(tds) else nfield
    fspans[rid]=(s,e)
def find_rid(fn):
    for rid in range(1,len(tds)+1):
        if m.td_fullname(rid)==fn: return rid
    return None
def methods(rid):
    s,e=spans[rid]; return [(m.s(mds[k-1][3]),k) for k in range(s,e)]
def msig(mid):
    row=mds[mid-1]; blob,_=m.blob(row[4])
    try: ret,params,gen=m.sig_blob(blob)
    except Exception: ret,params='<err>',[]
    return ret,params
def fields(rid):
    s,e=fspans[rid]; out=[]
    for fid in range(s,e):
        row=fds[fid-1]; blob,_=m.blob(row[2])
        saved=m.data; m.data=blob
        try:
            p=1 if (blob and blob[0]==6) else 0
            t,_=m.parse_type(p)
        except Exception: t='<err>'
        finally: m.data=saved
        out.append((m.s(row[1]),t,row[0],fid))
    return out
print('=== Proto_CSRsp 中的 watch_ad 访问器/字段 ===')
rid=find_rid('proto_ff.Proto_CSRsp')
print('  Proto_CSRsp rid',rid,'token',('0x%08X'%(0x02000000|rid)))
hits=[x for x in methods(rid) if 'watch_ad' in x[0].lower() or 'adinfo' in x[0].lower()]
for nm,mid in hits:
    ret,params=msig(mid); print('   %-40s argc=%d ret=%s'%(nm,len(params),ret))
flds=[x for x in fields(rid) if 'watch_ad' in x[0].lower()]
for nm,t,fl,fid in flds: print('   FIELD %-44s %s'%(nm,t))
print()
for tn in ('proto_ff.Proto_CSStartWatchAdRsp','proto_ff.Proto_CSFinishWatchAdRsp','proto_ff.Proto_CSStartWatchAdReq'):
    rid=find_rid(tn)
    print('=== %s rid=%s ==='%(tn,rid))
    if rid:
        for nm,mid in methods(rid):
            if any(k in nm for k in ('result','ad_id','uuid','sign')):
                ret,params=msig(mid); print('   %-34s argc=%d ret=%s'%(nm,len(params),ret))
        for nm,t,fl,fid in fields(rid):
            if any(k in nm for k in ('result','ad_id','uuid','sign','BackingField')):
                print('   FIELD %-46s %s'%(nm,t))
print()
print('=== 广告视频相关方法名（全程序集）===')
pat=('playadvideo','showreward','playvideo','showvideo','rewardvideo','advideo')
seen=0
for rid in range(1,len(tds)+1):
    for nm,mid in methods(rid):
        l=nm.lower()
        if any(p in l for p in pat):
            ret,params=msig(mid)
            print('   %-32s :: %-30s argc=%d ret=%s'%(m.td_fullname(rid),nm,len(params),ret)); seen+=1
print('  total',seen)
print()
print('=== 支撑字段（#1/#2/#3 身份链）===')
def showfield(tn, keys):
    rid=find_rid(tn)
    print(' ',tn,'rid',rid)
    if not rid: return
    for nm,t,fl,fid in fields(rid):
        if any(k.lower() in nm.lower() for k in keys):
            print('    %-42s %-44s %s'%(nm,t,'static' if fl&0x10 else ''))
showfield('ActionGameLibrary.BattleElementCenter',['gameObjectManager','localDamage'])
showfield('ActionGameLibrary.GameObjectManager',['_player','player'])
showfield('ActionGameLibrary.Character',['_characterData','characterData','_hitter','hitter','_ctype','characterType'])
showfield('ActionGameLibrary.CBattleActorUtil',[''])
showfield('ActionGameLibrary.CHitNotifier',['AddOneHitHurt'])

