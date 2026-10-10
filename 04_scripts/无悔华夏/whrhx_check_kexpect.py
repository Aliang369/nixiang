#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Whrhx 序言门禁（只读、不依赖 dump.cs）

读两类「RVA + 期望首指令」表，逐行 seek 到目标 so 的该 RVA 读 4 字节小端做比对：

  1) 手写表  WhrhxMod.cpp 的 `RVA_*` 枚举常量 + `kExpect[]` 行
  2) 自动生成表 `Games/WhrhxRvaGen.h` 的 `kExpectGen[]`（由 whrhx_rebaseline.sh 产出，
     见 P0-②；ResolveRva() 优先用它 —— 所以这张表必须 100% 命中）

用法:
  python3 whrhx_check_kexpect.py                                   # 默认 cpp + 本机运行时 so + 生成表
  python3 whrhx_check_kexpect.py --so <so> [--also <so2> ...]      # 换基准 so（如 APK 内置官版）
  python3 whrhx_check_kexpect.py --no-gen                          # 只查手写 kExpect
  python3 whrhx_check_kexpect.py --gen <WhrhxRvaGen.h>

退出码: 0 全部命中；1 存在失配/越界/常量未定义。
"""
import re
import os
import sys
import argparse

ROOT = os.path.dirname(os.path.abspath(__file__))
NIXIANG = os.path.normpath(os.path.join(ROOT, '..', '..'))
GAMES = os.path.join(NIXIANG, 'mod菜单', 'starry-mod-menu', 'app', 'src', 'main', 'jni', 'Games')
DEF_CPP = os.path.join(GAMES, 'WhrhxMod.cpp')
DEF_GEN = os.path.join(GAMES, 'WhrhxRvaGen.h')
# ★ 2026-10-11：默认基准从 current_20260923 换到 current_20261011（79adda77… 那份运行时 so）。
#   旧默认值会让门禁一直对着已漂移的基准报失配，属于「门禁自己失准」。
DEF_SO = os.path.join(NIXIANG, '06_dumped', '无悔华夏', 'current_20261011', 'runtime',
                      'libil2cpp_runtime_20261011.so')

ap = argparse.ArgumentParser()
ap.add_argument('--cpp', default=DEF_CPP)
ap.add_argument('--gen', default=DEF_GEN)
ap.add_argument('--no-gen', action='store_true', help='不查自动生成表')
ap.add_argument('--so', default=DEF_SO)
ap.add_argument('--also', action='append', default=[])
args = ap.parse_args()

src = open(args.cpp, encoding='utf-8').read()

# 1) 枚举常量
consts = {}
for m in re.finditer(r'\b(RVA_[A-Z0-9_]+)\s*=\s*(0x[0-9A-Fa-f]+)', src):
    consts[m.group(1)] = int(m.group(2), 16)

# 2) 手写 kExpect 行
rows = []
for m in re.finditer(r'\{\s*(RVA_[A-Z0-9_]+)\s*,\s*(0x[0-9A-Fa-f]{8})\s*,\s*"([^"]*)"\s*\}', src):
    rows.append((m.group(1), int(m.group(2), 16), m.group(3)))

# 3) 自动生成表 kExpectGen 行（字面量 rva，不用常量名）
gen_rows = []
deny_rows = []   # kExpectStale[]：明确拒用（不是期望序言）
if not args.no_gen and os.path.isfile(args.gen):
    g = open(args.gen, encoding='utf-8').read()
    for m in re.finditer(r'\{\s*0x([0-9A-Fa-f]+)\s*,\s*0x([0-9A-Fa-f]{8})\s*,\s*"([^"]*)"\s*\}', g):
        rva, insn, tag = int(m.group(1), 16), int(m.group(2), 16), m.group(3)
        # kExpectStale[] 的 firstInsn 恒为 0（拒用名单，不是期望值）⇒ 不计入命中/失配。
        if insn == 0:
            deny_rows.append((rva, tag))
            continue
        gen_rows.append((rva, insn, tag))

print('常量 %d 个；kExpect %d 条；kExpectGen %d 条；kExpectStale %d 条%s'
      % (len(consts), len(rows), len(gen_rows), len(deny_rows),
         '' if gen_rows else '（--no-gen 或生成表缺失）'))


def first_insn(path, rva):
    with open(path, 'rb') as f:
        f.seek(rva)
        b = f.read(4)
    if len(b) != 4:
        return None
    return int.from_bytes(b, 'little')


def check_hand(so, label):
    # 手写 kExpect 已于 2026-10-11 删除（并入生成表）⇒ 正常情况下 rows 为空、本函数只是留档。

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
    print('[hand] %s: 命中 %d / 失配 %d / 未知 %d' % (label, ok, bad, miss))
    for f in fails:
        print('   ' + f)
    return bad


def check_gen(so, label):
    ok = bad = miss = 0
    fails = []
    for rva, exp, tag in gen_rows:
        got = first_insn(so, rva)
        if got is None:
            miss += 1
            fails.append('?? %-34s RVA=0x%08X 越界' % (tag, rva))
        elif got == exp:
            ok += 1
        else:
            bad += 1
            fails.append('!! %-34s RVA=0x%08X got=0x%08X want=0x%08X' % (tag, rva, got, exp))
    print('[gen]  %s: 命中 %d / 失配 %d / 未知 %d' % (label, ok, bad, miss))
    for f in fails:
        print('   ' + f)
    return bad


def run(so, label):
    n = check_hand(so, label)
    if gen_rows:
        n += check_gen(so, label)
    return n


total_bad = run(args.so, os.path.basename(args.so))
for extra in args.also:
    if os.path.isfile(extra):
        total_bad += run(extra, os.path.basename(extra))
    else:
        print('skip (missing): %s' % extra)

print('KEXPECT_RESULT=%s bad=%d deny=%d' % ('PASS' if total_bad == 0 else 'FAIL', total_bad,
                                        len(deny_rows)))
sys.exit(1 if total_bad else 0)
