#!/usr/bin/env python3
"""从 dump 里按明文定位并打印上下文（反混淆取证用）"""
import sys, re, os, glob
ROOT="/Users/aliang/Documents/nixiang/06_dumped/勇士之心/hotupdate_20261005"
def load(f=None):
    if f:
        p=os.path.join(ROOT,f) if not f.startswith('/') else f
        return {p:open(p,encoding='utf-8',errors='replace').read()}
    d={}
    for root,_,fs in os.walk(ROOT):
        for x in fs:
            if x.endswith('.js'):
                p=os.path.join(root,x); d[p]=open(p,encoding='utf-8',errors='replace').read()
    return d
def find(pat, before=200, after=400, limit=6, f=None):
    for p,t in load(f).items():
        for i,m in enumerate(re.finditer(pat,t)):
            if i>=limit: break
            print(f"--- {p} @{m.start()}")
            print(t[max(0,m.start()-before):m.start()+after])
            print()
if __name__=="__main__":
    pat=sys.argv[1]; b=int(sys.argv[2]) if len(sys.argv)>2 else 200
    a=int(sys.argv[3]) if len(sys.argv)>3 else 400
    l=int(sys.argv[4]) if len(sys.argv)>4 else 6
    f=sys.argv[5] if len(sys.argv)>5 else None
    find(pat,b,a,l,f)
