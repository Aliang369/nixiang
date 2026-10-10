#!/usr/bin/env python3
import os, re, sys

D = "/Users/aliang/Documents/nixiang/06_dumped/崩坏3/mem_20261008/bh3_shell"

def strings(data, minlen=4):
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

def utf16_strings(data, minlen=3):
    out = []
    cur = []
    i = 0
    while i + 1 < len(data):
        lo, hi = data[i], data[i+1]
        if hi == 0 and 0x20 <= lo < 0x7f:
            cur.append(chr(lo))
            i += 2
        else:
            if len(cur) >= minlen:
                out.append(''.join(cur))
            cur = []
            i += 2
    if len(cur) >= minlen:
        out.append(''.join(cur))
    return out

# Chinese CJK utf8 / utf16 scan
def has_cjk(s):
    return any('\u4e00' <= c <= '\u9fff' for c in s)

targets = ['秒杀', '无敌', '暴击', '血量', '超级', '伤害', '秒', '锁血']
dll_pat = re.compile(r'[A-Za-z0-9_./+-]{2,}\.dll')
cls_pat = re.compile(r'^[A-Z][A-Za-z0-9_]{3,40}$')

files = sorted(os.listdir(D))
print("SEG\tSIZE\t.dll\tn_ascii\tn_utf16\tcjk")
allfiles = [f for f in files if f.endswith('.bin')]
for f in allfiles:
    p = os.path.join(D, f)
    size = os.path.getsize(p)
    if size > 8*1024*1024:
        continue
    data = open(p, 'rb').read()
    asc = strings(data, 4)
    u16 = utf16_strings(data, 3)
    dlls = set(dll_pat.findall(' '.join(asc)))
    cjk = [s for s in (asc+u16) if has_cjk(s)]
    if dlls or cjk:
        print(f"{f}\t{size}\t{sorted(dlls)}\t{len(asc)}\t{len(u16)}\t{cjk[:10]}")
