#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""_parse_dumpcs.py — Il2CppDumper 的 dump.cs → _methods.tsv / _types_index.tsv

背景：鹦鹉团 1.0.7 的 _methods.tsv（213,927 条）/ _types_index.tsv（26,391 条）原先由
一次性内联命令生成，脚本没留档，导致这两张表只能靠本地文件存在，无法复现（2026-11-21
用户点名批评的正是这件事）。本脚本把该步正式化，输出与当初逐字节一致。

用法:
  python3 06_dumped/鹦鹉团/il2cpp/_parse_dumpcs.py <dump.cs> <输出目录>

输出（无表头，制表符分隔）:
  _methods.tsv      RVA \t file_off \t 类名 \t 签名 \t dump.cs 行号
  _types_index.tsv  行号 \t 命名空间(空则 __Generated) \t 原始命名空间 \t 种类 \t 类型名

约定:
  * `_methods.tsv` 第五列 = **方法签名行**在 dump.cs 的物理行号（1 基，= `// RVA:` 注释行 + 1）。
    文档里写的 `014:行号` / `015:777-830` 经此列换算，**不是** Lua 源码行号。
  * `_types_index.tsv` 第一列 = 类型声明行行号。名字含 `<` 的编译器生成类型（`<Module>`、
    `<>c__DisplayClass*`、`<>f__AnonymousType*`）一律跳过——实测旧表里 0 条含 `<`。
  * 只收 RVA > 0 的方法：dump.cs 中 `RVA: -1` 表示无实现（abstract/extern），实测旧表里 0 条。
  * RVA / Offset 一律小写十六进制，与旧表一致。

自证（2026-11-21 实测）:
  python3 _parse_dumpcs.py dump.cs /tmp/x
  md5 -q /tmp/x/_methods.tsv 06_dumped/鹦鹉团/il2cpp/_methods.tsv   # 应一致
"""
import os
import re
import sys

TYPE_RE = re.compile(
    r'^(?:public |private |protected |internal )?'
    r'(?:static |sealed |abstract |readonly |ref )*'
    r'(class|struct|enum|interface) (\S+)'
)
RVA_RE = re.compile(r'^\s*// RVA: (\S+) Offset: (\S+)')


def to_int(tok):
    """'0x3719508' / '-1' → int；无法解析返回 None。"""
    try:
        return int(tok, 16) if tok.lower().startswith('0x') else int(tok)
    except ValueError:
        return None


def main():
    if len(sys.argv) != 3:
        print(__doc__)
        return 2
    src, outdir = sys.argv[1], sys.argv[2]
    os.makedirs(outdir, exist_ok=True)

    with open(src, encoding='utf-8', errors='replace') as fh:
        lines = fh.read().splitlines()

    methods, types = [], []
    ns = ''
    cur_type = ''
    pending_rva = None  # (rva, off, lineno)

    for i, line in enumerate(lines, 1):
        if line.startswith('// Namespace:'):
            ns = line.split(':', 1)[1].strip()
            continue

        if '// TypeDefIndex:' in line:
            m = TYPE_RE.match(line)
            if m:
                kind, name = m.group(1), m.group(2)
                # 泛型实参在名字里以 <...> 出现；旧表一律截断到 '<' 前，
                # 截断后为空（<Module>/<>c__DisplayClass*）即编译器生成类型，整类不入索引。
                name = name.split('<')[0]
                cur_type = name
                if not name:
                    continue
                types.append(f"{i}\t__Generated\t{ns}\t{kind}\t{name}")
                continue

        m = RVA_RE.match(line)
        if m:
            pending_rva = (m.group(1), m.group(2), i)
            continue

        if pending_rva:
            rva, off, lineno = pending_rva
            pending_rva = None
            val = to_int(rva)
            if val is None or val <= 0 or not cur_type:
                continue
            methods.append(f"{rva.lower()}\t{off.lower()}\t{cur_type}\t{line.strip()}\t{lineno + 1}")

    with open(os.path.join(outdir, '_methods.tsv'), 'w', encoding='utf-8') as fh:
        fh.write('\n'.join(methods) + ('\n' if methods else ''))
    with open(os.path.join(outdir, '_types_index.tsv'), 'w', encoding='utf-8') as fh:
        fh.write('\n'.join(types) + ('\n' if types else ''))

    print(f"dump.cs={len(lines)} 行  methods={len(methods)}  types={len(types)}  out={outdir}")
    return 0


if __name__ == '__main__':
    sys.exit(main())
