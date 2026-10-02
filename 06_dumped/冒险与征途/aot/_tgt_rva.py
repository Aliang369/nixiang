# -*- coding: utf-8 -*-
"""[T1] RVA statistics: is Assembly-CSharp.dll real IL bodies or metadata-only AOT supplement?"""
import sys, os, io, collections
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _mdread import Meta, TABLES

OUT = r'E:\nixiang\06_dumped\冒险与征途\aot'
m = Meta(os.path.join(OUT, 'Assembly-CSharp.dll'))

md = m.tbl['MethodDef']
tot = len(md)
nz = sum(1 for r in md if r[0] != 0)
print('assembly        =', repr(m.asm_name))
print('runtime         =', m.runtime)
print('md_version      =', m.md_version)
print('TypeDef         =', len(m.tbl['TypeDef']))
print('MethodDef       =', tot)
print('RVA != 0        =', nz)
print('RVA == 0        =', tot - nz)
print('pct with body   = %.3f%%' % (100.0 * nz / tot))

# ImplFlags distribution
imp = collections.Counter(r[1] for r in md)
print('ImplFlags dist  =', dict(imp))

# Method flags distribution
fl = collections.Counter(r[2] for r in md)
print('Flags top       =', fl.most_common(8))

# per-asm MethodDef only has this one module; check ModuleRef / AssemblyRef
print('AssemblyRef     =', [m.s(r[6]) for r in m.tbl['AssemblyRef']][:40])
print('ModuleRef       =', [m.s(r[0]) for r in m.tbl['ModuleRef']][:40])
print('Module[0].Name  =', repr(m.s(m.tbl['Module'][0][1])))
print('streams         =', {k: (hex(v[0]), v[1]) for k, v in m.streams.items()})
print('tbl_end         = 0x%X' % m.tbl_end)
print('file size       =', len(m._file))
