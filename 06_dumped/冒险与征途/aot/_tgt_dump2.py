# -*- coding: utf-8 -*-
"""[T7] Dump one type's methods IL bodies, or a list of types, to a UTF-8 file."""
import sys, os, io
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import _tgt_q as Q

OUT = r'E:\nixiang\06_dumped\冒险与征途\aot'
types = sys.argv[1:-1]
outname = sys.argv[-1]
res = io.StringIO()
for tre in types:
    res.write('\n\n############ TYPE %s ############\n' % tre)
    for rid in range(1, len(Q.m.tbl['MethodDef']) + 1):
        o = Q.method_owner_type(rid)
        if o is None:
            continue
        if Q.m.td_fullname(o) != tre:
            continue
        r = Q.m.tbl['MethodDef'][rid - 1]
        if r[0] == 0:
            continue
        res.write('\n' + Q.fmt_method(rid) + '\n')
        code, st = Q.method_body(rid)
        if not code:
            res.write('  <no body>\n')
            continue
        for l in Q.il_disasm(code):
            res.write('   ' + l + '\n')
open(os.path.join(OUT, outname), 'w', encoding='utf-8').write(res.getvalue())
print('written', len(res.getvalue()), 'chars ->', outname)
