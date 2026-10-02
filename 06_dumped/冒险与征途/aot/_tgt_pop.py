# -*- coding: utf-8 -*-
"""[T6] Fast byte-level token popularity scan.
   For each Field/Method token, count 4-byte little-endian occurrences in the DLL.
   Only tokens whose bytes appear are then disassembled for owner attribution.
Usage:
   python _tgt_pop.py field <name_re>
   python _tgt_pop.py method <owner_re> <name_re>
   python _tgt_pop.py rankfield            -> top fields by occurrence
"""
import sys, os, re, struct
from collections import defaultdict
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import _tgt_q as Q

DATA = Q.m.data


def count_token(tok):
    pat = struct.pack('<I', tok)
    n = 0
    start = 0
    while True:
        i = DATA.find(pat, start)
        if i < 0:
            return n
        n += 1
        start = i + 1


def main():
    cmd = sys.argv[1]
    if cmd == 'field':
        rx = re.compile(sys.argv[2])
        out = []
        for i, r in enumerate(Q.m.tbl['Field']):
            nm = Q.m.s(r[1])
            if rx.search(nm):
                tok = 0x04000000 | (i + 1)
                out.append((count_token(tok), tok, nm, Q.field_owner_type(i + 1)))
        out.sort(reverse=True)
        for n, tok, nm, own in out:
            print('%6d  0x%08X  %-40s %s' % (n, tok, nm, own))
    elif cmd == 'method':
        orx = re.compile(sys.argv[2]); mrx = re.compile(sys.argv[3])
        out = []
        for rid in range(1, len(Q.m.tbl['MethodDef']) + 1):
            r = Q.m.tbl['MethodDef'][rid - 1]
            nm = Q.m.s(r[3])
            if not mrx.search(nm):
                continue
            o = Q.method_owner_type(rid)
            full = Q.m.td_fullname(o) if o else '?'
            if not orx.search(full):
                continue
            tok = 0x06000000 | rid
            out.append((count_token(tok), tok, nm, full))
        out.sort(reverse=True)
        for n, tok, nm, full in out[:60]:
            print('%6d  0x%08X  %-34s %s' % (n, tok, nm, full))


main()
