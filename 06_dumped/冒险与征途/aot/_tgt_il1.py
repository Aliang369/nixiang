# -*- coding: utf-8 -*-
"""[T4] Dump selected IL bodies to UTF-8 files (avoids console GBK mangling)."""
import sys, os, io
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import _tgt_q as Q

OUT = r'E:\nixiang\06_dumped\冒险与征途\aot'
res = io.StringIO()

TARGETS = [
    ("StarsCollider.DoAttack", "ActionGameLibrary.StarsCollider", "DoAttack"),
    ("StarsCollider.DamageTo", "ActionGameLibrary.StarsCollider", "DamageTo"),
    ("StarsCollider.GetOriginalDamage", "ActionGameLibrary.StarsCollider", "GetOriginalDamage"),
    ("StarsCollider.HitTestClientLocal", "ActionGameLibrary.StarsCollider", "HitTestClientLocal"),
    ("StarsCollider.CheckHitType", "ActionGameLibrary.StarsCollider", "CheckHitType"),
    ("Character.Damage_long", "ActionGameLibrary.Character", "Damage"),
]
for label, treg, mreg in TARGETS:
    res.write('\n\n########## %s ##########\n' % label)
    for rid in range(1, len(Q.m.tbl['MethodDef']) + 1):
        o = Q.method_owner_type(rid)
        if o is None:
            continue
        full = Q.m.td_fullname(o)
        nm = Q.m.s(Q.m.tbl['MethodDef'][rid - 1][3])
        if full == treg and nm == mreg:
            res.write('\n' + Q.fmt_method(rid) + '\n')
            code, st = Q.method_body(rid)
            res.write('body: %s len=%d\n' % (st, len(code) if code else 0))
            if code:
                for l in Q.il_disasm(code):
                    res.write('   ' + l + '\n')

open(os.path.join(OUT, '_tgt_il_damage.txt'), 'w', encoding='utf-8').write(res.getvalue())
print('written', len(res.getvalue()), 'chars')
