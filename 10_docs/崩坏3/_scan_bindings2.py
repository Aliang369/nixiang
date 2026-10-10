#!/usr/bin/env python3
import os, re

D = "/Users/aliang/Documents/nixiang/06_dumped/崩坏3/mem_20261008/bh3_shell"

def ascii_strings(data, minlen=4):
    out = []
    cur = bytearray()
    for b in data:
        if 0x20 <= b < 0x7f:
            cur.append(b)
        else:
            if len(cur) >= minlen:
                out.append(cur.decode('ascii', 'ignore'))
            cur = bytearray()
    if len(cur) >= minlen:
        out.append(cur.decode('ascii', 'ignore'))
    return out

def utf8_strings(data, minlen=3):
    out = []
    cur = bytearray()
    for b in data:
        if b >= 0x20 and b != 0x7f:
            cur.append(b)
        else:
            if len(cur) >= minlen:
                try:
                    s = cur.decode('utf-8')
                    out.append(s)
                except Exception:
                    pass
            cur = bytearray()
    if len(cur) >= minlen:
        try:
            out.append(cur.decode('utf-8'))
        except Exception:
            pass
    return out

pats = {
    'hexaddr': re.compile(r'0x[0-9A-Fa-f]{5,}'),
    'feat': re.compile(r'(?i)feat'),
    'cjk': re.compile(r'[\u4e00-\u9fff]{2,}'),
    'bind': re.compile(r'[A-Za-z_][A-Za-z0-9_]{2,}(\.[A-Za-z_][A-Za-z0-9_]*)+'),
}
kw = ['秒','伤','血','暴','无敌','kill','damage','Damage','Hp','HP','Crit','crit','health','Health',
      'get_ret','featNum','怪','盾','防御','倍','攻击','speed','Speed','dmg','Dmg']

files = [f for f in sorted(os.listdir(D)) if f.endswith('.bin')]
for f in files:
    p = os.path.join(D, f)
    size = os.path.getsize(p)
    if size > 8*1024*1024:
        continue
    data = open(p, 'rb').read()
    asc = ascii_strings(data, 4)
    u8 = utf8_strings(data, 2)
    alls = asc + u8
    hits = {}
    for k in kw:
        got = sorted(set(s for s in alls if k in s))
        if got:
            hits[k] = got[:6]
    cjk = sorted(set(s for s in alls if pats['cjk'].search(s)))
    hexa = sorted(set(s for s in asc if pats['hexaddr'].search(s) and ('=' in s or '0x' in s)))[:6]
    if hits or cjk or hexa:
        print("="*70)
        print(f"{f}  size={size}")
        if cjk:
            print("  CJK:", cjk[:15])
        if hexa:
            print("  HEXSTR:", hexa)
        for k, v in hits.items():
            print(f"  [{k}]:", v)
