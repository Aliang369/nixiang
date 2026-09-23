#!/usr/bin/env python3.11
# -*- coding: utf-8 -*-
"""无悔华夏 · RVA 重校工具

用途：热更换掉运行时 libil2cpp.so 后，把 WhrhxMod.cpp 里所有硬编码 RVA 与新 so/dump 对齐。
输入：
  --src      WhrhxMod.cpp
  --dump     Il2CppDumper 生成的 dump.cs（新运行时 so 对应的那份）
  --so       新运行时 libil2cpp.so
输出：每个 kExpect 条目的 命中/失配 + 新 RVA + 新首指令；并生成可直接粘贴的 kExpect 块。
只读，不修改任何文件（除非 --write）。
"""
import argparse
import re
import struct
import sys
from pathlib import Path

SIG_RE = re.compile(r'^\s*//\s*RVA:\s*(0x[0-9A-Fa-f]+)\s+Offset:\s*(0x[0-9A-Fa-f]+)')
CLASS_RE = re.compile(r'^\s*(?:public|internal)\s+(?:sealed\s+|abstract\s+|static\s+)*(class|struct|enum|interface)\s+([A-Za-z_][\w`<>,\.\[\] ]*?)\s*(?::|//|$)')
NS_RE = re.compile(r'^\s*//\s*Namespace:\s*(.*)$')
MEMBER_RE = re.compile(r'^\s*(?:public|private|protected|internal|static|virtual|override|sealed|abstract|\s)+[\w<>\[\],\.]+\s+([A-Za-z_]\w*)\s*\(')


def parse_dump(path):
    """返回 {(ns, class, method): [rva, ...]}，同时记录只有类名无命名空间的键。"""
    table = {}
    ns = ''
    cls = None
    pending = None
    with open(path, encoding='utf-8', errors='replace') as fh:
        for line in fh:
            m = NS_RE.match(line)
            if m:
                ns = m.group(1).strip()
                continue
            m = CLASS_RE.match(line)
            if m:
                cls = m.group(2).strip().split('<')[0].strip()
                pending = None
                continue
            m = SIG_RE.match(line)
            if m:
                pending = int(m.group(1), 16)
                continue
            if pending is not None:
                mm = MEMBER_RE.match(line)
                if mm and cls:
                    name = mm.group(1)
                    table.setdefault((ns, cls, name), []).append(pending)
                    pending = None
                elif line.strip() and not line.strip().startswith('//'):
                    pending = None
    return table


def parse_src(path):
    """返回 (常量 {名: 旧值}, kExpect 列表 [(常量名, 旧前缀, tag)])"""
    text = Path(path).read_text(encoding='utf-8')
    consts = {m.group(1): int(m.group(2), 16) for m in
              re.finditer(r'(RVA_[A-Z0-9_]+)\s*=\s*(0x[0-9A-Fa-f]+)', text)}
    rows = []
    for m in re.finditer(r'\{\s*(RVA_[A-Z0-9_]+)\s*,\s*(0x[0-9A-Fa-f]+)\s*,\s*"([^"]+)"\s*\}', text):
        rows.append((m.group(1), int(m.group(2), 16), m.group(3)))
    return consts, rows


def first_insn(so_path, off):
    with open(so_path, 'rb') as fh:
        fh.seek(off)
        b = fh.read(4)
    if len(b) < 4:
        return None
    return struct.unpack('<I', b)[0]


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--src', default='/Users/aliang/Documents/nixiang/mod菜单/starry-mod-menu/app/src/main/jni/Games/WhrhxMod.cpp')
    ap.add_argument('--dump', default='/Users/aliang/Documents/nixiang/06_dumped/无悔华夏/current_20260923/runtime/il2cpp_dump/dump.cs')
    ap.add_argument('--so', default='/Users/aliang/Documents/nixiang/06_dumped/无悔华夏/current_20260923/runtime/libil2cpp_runtime_20260923.so')
    args = ap.parse_args()

    consts, rows = parse_src(args.src)
    print(f'源码常量 {len(consts)} 个；kExpect 条目 {len(rows)} 条')
    table = parse_dump(args.dump)
    print(f'dump.cs 方法条目 {len(table)} 个\n')

    hit = miss = unknown = 0
    new_rows = []
    for const, old_prefix, tag in rows:
        old_rva = consts.get(const)
        if '.' in tag:
            cname, mname = tag.rsplit('.', 1)
        else:
            cname, mname = '', tag
        cands = []
        for (ns, cls, meth), rvas in table.items():
            if meth == mname and cls == cname:
                cands.extend(rvas)
        if not cands:
            print(f'??  {const:34s} {tag:44s} 旧=0x{old_rva:X} dump 中找不到方法')
            unknown += 1
            continue
        best = None
        for rva in cands:
            ins = first_insn(args.so, rva)
            if ins == old_prefix:
                best = (rva, ins)
                break
            if best is None:
                best = (rva, ins)
        rva, ins = best
        ok = (ins == old_prefix)
        if ok:
            hit += 1
            print(f'OK  {const:34s} {tag:44s} 旧=0x{old_rva:08X} 新=0x{rva:08X} 首指令=0x{ins:08X}')
        else:
            miss += 1
            print(f'!!  {const:34s} {tag:44s} 旧=0x{old_rva:08X}(0x{old_prefix:08X}) 新=0x{rva:08X}(0x{ins:08X})'
                  + (f' 候选{len(cands)}个' if len(cands) > 1 else ''))
        new_rows.append((const, rva, ins, tag, ok))

    print(f'\n汇总：命中 {hit} / 失配 {miss} / 未找到 {unknown}')
    print('\n--- 可直接替换的 kExpect 块（仅含需要改动的行）---')
    for const, rva, ins, tag, ok in new_rows:
        if ok:
            continue
        print(f'    {{ {const}, 0x{ins:08X}, "{tag}" }},')
    return 0


if __name__ == '__main__':
    sys.exit(main())
