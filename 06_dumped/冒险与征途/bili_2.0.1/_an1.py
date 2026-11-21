
import os,sys,json
sys.path.insert(0,'/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1')
from _mdread2 import Meta, TABLES

DLL='/Users/aliang/Documents/nixiang/06_dumped/冒险与征途/bili_2.0.1/Assembly-CSharp.dll'
m=Meta(DLL)
print('asm name      =',repr(m.asm_name))
print('runtime       =',m.runtime,' md_version =',m.md_version)
print('heapsizes     = 0x%X'%m.heapsizes)
nTD=len(m.tbl['TypeDef']); nMD=len(m.tbl['MethodDef']); nFD=len(m.tbl['Field'])
print('TypeDef=%d MethodDef=%d Field=%d Param=%d Property=%d MemberRef=%d TypeRef=%d NestedClass=%d CustomAttribute=%d TypeSpec=%d MethodSpec=%d'%(
  nTD,nMD,nFD,len(m.tbl['Param']),len(m.tbl['Property']),len(m.tbl['MemberRef']),len(m.tbl['TypeRef']),
  len(m.tbl['NestedClass']),len(m.tbl['CustomAttribute']),len(m.tbl['TypeSpec']),len(m.tbl['MethodSpec'])))
print('AssemblyRef=%d'%len(m.tbl['AssemblyRef']))
print('tables_end=0x%X  #~ off=0x%X size=%d'%(m.tbl_end,m.streams['#~'][0],m.streams['#~'][1]))
nz=sum(1 for r in m.tbl['MethodDef'] if r[0]!=0)
print('MethodDef RVA!=0: %d / %d = %.2f%%'%(nz,nMD,100.0*nz/nMD))
from collections import Counter
c=Counter()
for i,row in enumerate(m.tbl['TypeDef']):
    ns=m.s(row[2])
    c[ns if ns else '<global>']+=1
print('--- namespace histogram top20 ---')
for k,v in c.most_common(20): print('  %-45s %d'%(k,v))
print('  total namespaces', len(c))

