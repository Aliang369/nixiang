# -*- coding: utf-8 -*-
"""[T5] Reusable scanners built on _tgt_q.il_disasm (text-based, robust).
   tokuse  <0xTOKEN> [...]  -> who touches these Field/Method tokens
   callers <0x0600...> [...] -> who calls these MethodDef tokens
Usage: python _tgt_scan.py tokuse 0x0401764D 0x0401762C
"""
import sys, os, re
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import _tgt_q as Q


def iter_methods():
    MD = Q.m.tbl['MethodDef']
    for rid in range(1, len(MD) + 1):
        r = MD[rid - 1]
        if r[0] == 0:
            continue
        code, st = Q.method_body(rid)
        if not code:
            continue
        yield rid, r, code


def scan(patterns):
    """patterns: dict tok(int) -> compiled re to search in IL text"""
    hits = {k: [] for k in patterns}
    for rid, r, code in iter_methods():
        try:
            lines = Q.il_disasm(code)
        except Exception:
            continue
        o = Q.method_owner_type(rid)
        full = Q.m.td_fullname(o) if o else '?'
        mn = Q.m.s(r[3])
        for ln in lines:
            for k, rx in patterns.items():
                if rx.search(ln):
                    hits[k].append((full, mn, rid, ln.strip()))
                    break
    return hits


def scan_ctx(tok, ctx=6, owner_rx=None):
    """yield (full, method, rid, [context lines]) for every IL body referencing tok"""
    rx = re.compile('0x%08X' % tok, re.I)
    for rid, r, code in iter_methods():
        o = Q.method_owner_type(rid)
        full = Q.m.td_fullname(o) if o else '?'
        if owner_rx and not owner_rx.search(full):
            continue
        try:
            lines = Q.il_disasm(code)
        except Exception:
            continue
        idx = [i for i, ln in enumerate(lines) if rx.search(ln)]
        if not idx:
            continue
        win = []
        for i in idx:
            win.extend(lines[max(0, i - ctx):min(len(lines), i + ctx + 1)])
            win.append('  ---')
        yield full, Q.m.s(r[3]), rid, win


if __name__ == '__main__':
    cmd = sys.argv[1]
    if cmd == 'ctx':
        tok = int(sys.argv[2], 16)
        cx = int(sys.argv[3]) if len(sys.argv) > 3 else 6
        orx = re.compile(sys.argv[4]) if len(sys.argv) > 4 else None
        for full, mn, rid, win in scan_ctx(tok, cx, orx):
            print('##### %s :: %s   0x%08X' % (full, mn, 0x06000000 | rid))
            for ln in win:
                print('   ' + ln)
        sys.exit(0)
    toks = [int(x, 16) for x in sys.argv[2:]]
    pats = {t: re.compile('0x%08X' % t, re.I) for t in toks}
    res = scan(pats)
    for t in toks:
        lst = res[t]
        print('==== 0x%08X  hits=%d' % (t, len(lst)))
        for full, mn, rid, ln in lst[:100]:
            print('   %-58s %-26s %s' % (full, mn, ln))
