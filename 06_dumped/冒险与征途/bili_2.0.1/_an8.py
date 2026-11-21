
import os,sys
sys.path.insert(0,'/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1')
from _mdread2 import Meta
DLL='/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1/Assembly-CSharp.dll'
m=Meta(DLL)
tds=m.tbl['TypeDef']; mds=m.tbl['MethodDef']; fds=m.tbl['Field']
nmeth=len(mds)+1; nfield=len(fds)+1
def sp(rid):
    s=tds[rid-1][5]; e=tds[rid][5] if rid<len(tds) else nmeth
    return (s,e)
def fp(rid):
    s=tds[rid-1][4]; e=tds[rid][4] if rid<len(tds) else nfield
    return (s,e)
def msig(mid):
    row=mds[mid-1]; blob,_=m.blob(row[4])
    try: ret,params,_=m.sig_blob(blob)
    except Exception: ret,params='<err>',[]
    return ret,params
print('=== 类型名含 WatchAd ===')
for rid in range(1,len(tds)+1):
    nm=m.td_name[rid-1]
    if 'watchad' in nm.lower() or 'FloatingAd' in nm:
        s,e=sp(rid)
        print('  %-46s token=0x%08X methods=%d'%(m.td_fullname(rid),0x02000000|rid,e-s))
        for mid in range(s,e):
            n2=m.s(mds[mid-1][3])
            if any(k in n2 for k in ('result','ad_id','uuid','sign')):
                ret,params=msig(mid); print('      %-30s argc=%d ret=%s'%(n2,len(params),ret))
print()
print('=== CHitNotifier 方法中含 HitHurt/Hurt ===')
rid=None
for r in range(1,len(tds)+1):
    if m.td_fullname(r)=='ActionGameLibrary.CHitNotifier': rid=r
s,e=sp(rid)
for mid in range(s,e):
    n2=m.s(mds[mid-1][3])
    if 'hurt' in n2.lower() or 'hit' in n2.lower():
        ret,params=msig(mid); print('   %-34s argc=%d ret=%s'%(n2,len(params),ret))
print()
print('=== Character / InteractiveGameObject 字段（找 hitter/ctype/cdata）===')
for tn in ('ActionGameLibrary.Character','ActionGameLibrary.InteractiveGameObject','ActionGameLibrary.SimpleCharacter'):
    r=None
    for x in range(1,len(tds)+1):
        if m.td_fullname(x)==tn: r=x
    if not r: continue
    print(' ',tn,'token=0x%08X'%(0x02000000|r))
    a,b=fp(r)
    for fid in range(a,b):
        row=fds[fid-1]; nm=m.s(row[1]); blob,_=m.blob(row[2])
        saved=m.data; m.data=blob
        try:
            p=1 if (blob and blob[0]==6) else 0
            t,_=m.parse_type(p)
        except Exception: t='?'
        finally: m.data=saved
        print('    %-40s %-40s %s'%(nm,t,'static' if row[0]&0x10 else ''))

