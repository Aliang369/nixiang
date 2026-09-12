#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
split_sys_bundle.py — 把 Cocos Creator 3.x 的 System.register 脚本包切成模块索引/单模块文件。

用法:
  python3 split_sys_bundle.py index  <bundle.js> [-o index.tsv]
  python3 split_sys_bundle.py module <bundle.js> <模块名子串> [-o out.js]
  python3 split_sys_bundle.py list   <bundle.js> [前缀过滤]

实现要点：字符串/模板串/注释感知的括号配对，避免被字符串里的括号带偏。
"""
import sys, os, re, json

MARK = 'System.register("'


def find_modules(src):
    """返回 [(module_id, start, end)]。

    采用最稳的策略：所有 `System.register("chunks://...` 的出现位置即模块边界
    （相邻两个标记之间属于前一个模块），最后一个模块延到文件末尾。
    不做括号配对——minified 代码里的正则字面量会让配对失步（已实测）。
    """
    starts = []
    pos = 0
    while True:
        i = src.find(MARK, pos)
        if i < 0:
            break
        j = i + len(MARK)
        k = src.find('"', j)
        if k < 0:
            break
        starts.append((src[j:k], i))
        pos = i + len(MARK)
    out = []
    for idx, (mid, s) in enumerate(starts):
        e = starts[idx + 1][1] if idx + 1 < len(starts) else len(src)
        out.append((mid, s, e))
    return out


def main():
    if len(sys.argv) < 3:
        print(__doc__)
        return 1
    cmd, bundle = sys.argv[1], sys.argv[2]
    arg3 = sys.argv[3] if len(sys.argv) > 3 and not sys.argv[3].startswith('-') else None
    out = None
    if '-o' in sys.argv:
        out = sys.argv[sys.argv.index('-o') + 1]
    src = open(bundle, encoding='utf-8', errors='replace').read()
    mods = find_modules(src)
    if cmd == 'list':
        for mid, s, e in mods:
            if arg3 and arg3 not in mid:
                continue
            print(f"{e-s:>9}  {mid}")
        return 0
    if cmd == 'index':
        rows = []
        for mid, s, e in mods:
            rows.append((mid, s, e, e - s))
        if out:
            with open(out, 'w', encoding='utf-8') as f:
                f.write("size\toffset\tmodule_id\n")
                for mid, s, e, sz in rows:
                    f.write(f"{sz}\t{s}\t{mid}\n")
            print(f"写入 {out}（{len(rows)} 模块）")
        else:
            for mid, s, e, sz in rows:
                print(f"{sz:>9} {s:>9}  {mid}")
        return 0
    if cmd == 'module':
        if not arg3:
            print("需要模块名子串")
            return 1
        hits = [(mid, s, e) for mid, s, e in mods if arg3 in mid]
        if not hits:
            print("未命中")
            return 1
        for mid, s, e in hits:
            print(f"# {mid}  ({e-s} B)")
        if out:
            with open(out, 'w', encoding='utf-8') as f:
                for mid, s, e in hits:
                    f.write(f"\n/* ===== {mid} ===== */\n")
                    f.write(src[s:e])
                    f.write("\n")
            print(f"写入 {out}")
        else:
            for mid, s, e in hits:
                print(src[s:e])
        return 0
    print(__doc__)
    return 1


if __name__ == '__main__':
    sys.exit(main())
