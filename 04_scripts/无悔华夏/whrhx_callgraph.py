#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Whrhx 政策面板调用图分析（只读）
用法: python3 whrhx_callgraph.py [--so <so>] [--map <rva_map.tsv>] [--class <类名>] [--method 类.方法]
输出: 每个目标方法的 bl/b 目标，按「最近前置 RVA」反查类.方法名（偏移 >0 表示落在函数体内）。
"""
import subprocess, re, bisect, sys, argparse, os

ROOT = os.path.dirname(os.path.abspath(__file__))
DEFAULT_DUMP = os.path.normpath(os.path.join(
    ROOT, '..', '..', '06_dumped', '无悔华夏', 'current_20260923', 'runtime'))

ap = argparse.ArgumentParser()
ap.add_argument('--so', default=os.path.join(DEFAULT_DUMP, 'libil2cpp_runtime_20260923.so'))
ap.add_argument('--map', default=os.path.join(DEFAULT_DUMP, 'il2cpp_dump', 'rva_map.tsv'))
ap.add_argument('--class', dest='cls', default=None, help='列出该类全部方法后退出')
ap.add_argument('--method', dest='methods', action='append', default=None,
                help='类.方法（可多次）')
args = ap.parse_args()

m = {}
for ln in open(args.map, encoding='utf-8'):
    parts = ln.rstrip('\n').split('\t')
    if len(parts) < 4:
        continue
    a, ns, c, sig = parts[0], parts[1], parts[2], parts[3]
    m.setdefault(int(a, 16), []).append('%s.%s' % (c, sig[:90]))
keys = sorted(m)


def near(t):
    i = bisect.bisect_right(keys, t) - 1
    if i < 0:
        return '?0x%X' % t
    d = t - keys[i]
    return m[keys[i]][0] + ('' if d == 0 else ' +0x%X' % d)


if args.cls:
    for k in keys:
        for n in m[k]:
            if n.startswith(args.cls + '.'):
                print('0x%08X  %s' % (k, n))
    sys.exit(0)


def method_end(s):
    i = bisect.bisect_right(keys, s)
    return keys[i] if i < len(keys) else s + 0x400


for spec in (args.methods or []):
    if '.' not in spec:
        print('!! bad spec', spec)
        continue
    c, meth = spec.split('.', 1)
    cands = [k for k in keys
             if any(n.startswith(c + '.') and re.search(r'\b%s\b' % re.escape(meth), n)
                    for n in m[k])]
    if not cands:
        print('=== %s === NOT FOUND' % spec)
        continue
    s = cands[0]
    e = method_end(s)
    print('=== %s  0x%08X..0x%08X (%d B) ===' % (spec, s, e, e - s))
    out = subprocess.run(['objdump', '-d', '--start-address=%d' % s,
                          '--stop-address=%d' % e, args.so],
                         capture_output=True, text=True, errors='replace').stdout
    first = []
    seen = []
    for line in out.splitlines():
        mm = re.match(r'\s*([0-9a-f]+):\s+([0-9a-f]{8})\s', line)
        if mm and len(first) < 2:
            first.append(mm.group(2))
        for op in ('bl', 'b'):
            mo = re.search(r'\b%s\s+(?:0x)?([0-9a-fA-F]+)' % op, line)
            if not mo:
                continue
            if op == 'b' and re.search(r'\bb\.(eq|ne|lt|gt|le|ge|hi|ls|cs|cc|mi|pl|vs|vc)\b', line):
                continue
            t = int(mo.group(1), 16)
            if t < s or t > e:
                tag = near(t)
                if tag not in seen:
                    seen.append(tag)
                    print('    %s -> %s' % (op, tag))
            break
    print('    首指令: %s' % ' '.join(first))
