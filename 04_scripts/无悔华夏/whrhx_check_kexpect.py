#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Whrhx kExpect 全表序言门禁（只读、不依赖 dump.cs）
解析 WhrhxMod.cpp 的 RVA_* 枚举常量 + kExpect 表，逐行读目标 so 在 RVA 处的
首 4 字节小端，与声明的序言比对；同时按 --also 追加其它 so（如 APK 内置官版）。
用法: python3 whrhx_check_kexpect.py [--cpp <WhrhxMod.cpp>] [--so <so>] [--also <so2> ...]
退出码: 0 全部命中；1 存在失配。
"""
import re, os, sys, argparse

ROOT = os.path.dirname(os.path.abspath(__file__))
NIXIANG = os.path.normpath(os.path.join(ROOT, '..', '..'))
DEF_CPP = os.path.join(NIXIANG, 'mod菜单', 'starry-mod-menu', 'app', 'src', 'main', 'jni',
                       'Games', 'WhrhxMod.cpp')
DEF_SO = os.path.join(NIXIANG, '06_dumped', '无悔华夏', 'current_20260923', 'runtime',
                      'libil2cpp_runtime_20260923.so')

ap = argparse.ArgumentParser()
ap.add_argument('--cpp', default=DEF_CPP)
ap.add_argument('--so', default=DEF_SO)
ap.add_argument('--also', action='append', default=[])
args = ap.parse_args()

src = open(args.cpp, encoding='utf-8').read()

# 1) 枚举常量
consts = {}
for m in re.finditer(r'\b(RVA_[A-Z0-9_]+)\s*=\s*(0x[0-9A-Fa-f]+)', src):
    consts[m.group(1)] = int(m.group(2), 16)

# 2) kExpect 行
rows = []
for m in re.finditer(r'\{\s*(RVA_[A-Z0-9_]+)\s*,\s*(0x[0-9A-Fa-f]{8})\s*,\s*"([^"]*)"\s*\}', src):
    rows.append((m.group(1), int(m.group(2), 16), m.group(3)))

print('常量 %d 个；kExpect %d 条' % (len(consts), len(rows)))


def first_insn(path, rva):
    with open(path, 'rb') as f:
        f.seek(rva)
        b = f.read(4)
    if len(b) != 4:
        return None
    return int.from_bytes(b, 'little')


def check(so, label):
    ok = bad = miss = 0
    fails = []
    for name, exp, tag in rows:
        if name not in consts:
            miss += 1
            fails.append('?? %-30s %-34s 常量未定义' % (name, tag))
            continue
        got = first_insn(so, consts[name])
        if got is None:
            miss += 1
            fails.append('?? %-30s %-34s RVA 越界' % (name, tag))
        elif got == exp:
            ok += 1
        else:
            bad += 1
            fails.append('!! %-30s %-34s RVA=0x%08X got=0x%08X want=0x%08X'
                         % (name, tag, consts[name], got, exp))
    print('%s: 命中 %d / 失配 %d / 未知 %d' % (label, ok, bad, miss))
    for f in fails:
        print('   ' + f)
    return bad


total_bad = 0
total_bad += check(args.so, os.path.basename(args.so))
for extra in args.also:
    if os.path.isfile(extra):
        total_bad += check(extra, os.path.basename(extra))
    else:
        print('skip (missing): %s' % extra)

sys.exit(1 if total_bad else 0)
