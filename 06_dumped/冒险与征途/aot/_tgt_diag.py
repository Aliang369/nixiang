# -*- coding: utf-8 -*-
"""[T3] Diagnose: verify TypeDef/Field row parsing widths and blob deltas."""
import sys, os, struct
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _mdread import Meta, TABLES

OUT = r'E:\nixiang\06_dumped\冒险与征途\aot'
m = Meta(os.path.join(OUT, 'Assembly-CSharp.dll'))

print('heapsizes=0x%X (bit0 str4=%d bit1 guid4=%d bit2 blob4=%d)' % (
    m.heapsizes, m.heapsizes & 1, (m.heapsizes >> 1) & 1, (m.heapsizes >> 2) & 1))
print('rows: TypeDef=%d Field=%d MethodDef=%d Param=%d' % (
    len(m.tbl['TypeDef']), len(m.tbl['Field']), len(m.tbl['MethodDef']), len(m.tbl['Param'])))

print('\n--- TypeDef col widths ---')
for cn, w in m._sizes[m._tid('TypeDef')]:
    print('   %-14s %d' % (cn, w))
print('--- Field col widths ---')
for cn, w in m._sizes[m._tid('Field')]:
    print('   %-14s %d' % (cn, w))
print('--- MethodDef col widths ---')
for cn, w in m._sizes[m._tid('MethodDef')]:
    print('   %-14s %d' % (cn, w))

print('\n--- first 3 TypeDef rows (raw) ---')
for i in range(3):
    print('  ', m.tbl['TypeDef'][i])

print('\n--- raw bytes for AcceptData TypeDef row ---')
# find row index of AcceptData
tgt = None
for i in range(len(m.tbl['TypeDef'])):
    if m.td_fullname(i + 1) == 'ActionGameLibrary.AcceptData':
        tgt = i
        break
print('AcceptData row index(0-based)=', tgt, 'row=', m.tbl['TypeDef'][tgt])

# recompute its byte offset
toff, tsize = m.streams['#~']
d = m.data
p = toff + 24
for i in range(64):
    if m.valid >> i & 1:
        p += 4
base = p
for i, (nm, cols) in enumerate(TABLES):
    if i not in m.rows:
        continue
    if i == m._tid('TypeDef'):
        rowlen = sum(w for _, w in m._sizes[i])
        print('TypeDef rowlen =', rowlen, 'base=0x%X' % base)
        off = base + tgt * rowlen
        print('row bytes @0x%X:' % off, d[off:off + rowlen].hex(' '))
        break
    rowlen = sum(w for _, w in m._sizes[i])
    base += m.rows[i] * rowlen

print('\n--- Field blob deltas for AcceptData fields ---')
fl = m.tbl['TypeDef'][tgt][4]
for fid in range(fl, fl + 12):
    r = m.tbl['Field'][fid - 1]
    b, _ = m.blob(r[2])
    print('  fid=%d name=%-28s sigidx=0x%X  blob=%s' % (fid, m.s(r[1]), r[2], b.hex(' ')))

print('\n--- Field blob deltas for AttackData fields ---')
for i in range(len(m.tbl['TypeDef'])):
    if m.td_fullname(i + 1) == 'ActionGameLibrary.AttackData':
        fl = m.tbl['TypeDef'][i][4]
        for fid in range(fl, fl + 34):
            r = m.tbl['Field'][fid - 1]
            b, _ = m.blob(r[2])
            print('  fid=%d name=%-28s sigidx=0x%X  blob=%s' % (fid, m.s(r[1]), r[2], b.hex(' ')))
        break
