# -*- coding: utf-8 -*-
# 查询 TypeDef 的 Extends（基类）；纯 stdlib，输出 UTF-8。
import sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
import _tgt_q as q

m = q.m
T = m.tbl['TypeDef']          # 行主序：每行 [Flags, Name, Namespace, Extends, FieldList, MethodList]
N = len(T)


def tname(rid):
    try:
        return m.td_fullname(rid)
    except Exception:
        return '?rid%d' % rid


def dec_extends(v):
    if v == 0:
        return '(none)'
    tag = v & 3
    rid = v >> 2
    if tag == 0:
        return tname(rid)
    if tag == 1:
        tr = m.tbl['TypeRef']
        try:
            return 'TypeRef:%s.%s' % (m.s(tr[rid - 1][2]), m.s(tr[rid - 1][1]))
        except Exception:
            return 'TypeRef#%d' % rid
    return 'TypeSpec#%d' % rid


for pat in sys.argv[1:]:
    hit = 0
    for rid in range(1, N + 1):
        fn = m.td_fullname(rid)
        if fn == pat or fn.endswith('.' + pat):
            print('%-55s [0x%08X] extends %s' % (fn, 0x02000000 | rid, dec_extends(T[rid - 1][3])))
            hit += 1
    if not hit:
        print('NO HIT ' + pat)
