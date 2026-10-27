#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""规则 ↔ 门禁映射校验（2026-10-27 第五轮）。

校验 `10_docs/_state/规则门禁映射.md` 表里引用的每个路径真实存在：
  - 「唯一权威正文」列的 `路径` / `路径` §节
  - 「机器门禁 / 审计」列的反引号里的脚本路径
  - 「回归测试」列里的测试文件
路径既支持仓根相对，也支持 `_common/xxx.py` 这类简写（自动补 `04_scripts/_common/`）。

同时检查：
  - 每条现行硬规则是否都有「机器门禁 / 审计」或显式「人工复核」标注（两者都空 ⇒ FAIL）
  - 映射里出现的 `.py`/`.sh` 审计脚本是否可执行存在

用法：python3 -B 04_scripts/_common/audit_rule_gate_map.py check [--root <tmp>]
退出码：0 通过 / 1 有缺项。
"""
import argparse
import os
import re
import sys

MAP_DOC = os.path.join('10_docs', '_state', '规则门禁映射.md')
TICK = re.compile(r'`([^`]+)`')
SECTION = re.compile(r'^(.*?)\s*§')


def candidate(prefix, path_text):
    """把映射里写的路径解析成真实文件候选。"""
    p = path_text.strip()
    p = SECTION.match(p).group(1) if SECTION.match(p) else p
    p = p.strip().strip('`')
    out = []
    if not p:
        return out
    if '/' in p:
        out.append(os.path.join(prefix, p))
    out.append(os.path.join(prefix, '04_scripts', '_common', p))
    out.append(os.path.join(prefix, '04_scripts', p))
    out.append(os.path.join(prefix, p.lstrip('/')))
    out.append(os.path.join(prefix, '10_docs', p))
    out.append(os.path.join(prefix, '04_scripts', '双机协作', p))
    out.append(os.path.join(prefix, '04_scripts', '双机协作', p.split()[0] if p.split() else p))
    return out


def resolve(prefix, p):
    tokens = [p] + p.split()
    for t in tokens:
        for c in candidate(prefix, t):
            if os.path.exists(c):
                return c
    return None


def parse_rows(doc):
    rows = []
    for line in doc.splitlines():
        if not line.startswith('| ') or line.startswith('| # |') or set(line) <= set('|-: '):
            continue
        cells = [c.strip() for c in line.strip().strip('|').split('|')]
        if len(cells) < 8:
            continue
        rows.append(cells)
    return rows


def check(root):
    errors = []
    path = os.path.join(root, MAP_DOC)
    if not os.path.isfile(path):
        return [f'缺少映射文件: {MAP_DOC}']
    doc = open(path, encoding='utf-8').read()
    rows = parse_rows(doc)
    if not rows:
        return ['映射表里没有解析到任何规则行']

    for cells in rows:
        rid, rule, authority, _scope, gate, tests, _cover, manual = cells[:8]
        # 权威正文必须存在
        refs = [t for t in TICK.findall(authority)]
        if not refs:
            errors.append(f'#{rid} {rule}: 没写唯一权威正文路径')
        for r in refs:
            if r.startswith('§'):
                continue
            if not resolve(root, r):
                errors.append(f'#{rid} {rule}: 权威正文不存在 → `{r}`')
        # 门禁或人工复核必须至少有一个
        gate_refs = [t for t in TICK.findall(gate) if any(k in t for k in ('.py', '.sh', '/', '子命令'))]
        for g in gate_refs:
            if '子命令' in g or g.startswith('无'):
                continue
            if not resolve(root, g):
                errors.append(f'#{rid} {rule}: 门禁脚本不存在 → `{g}`')
        gate_ok = bool(gate_refs) or any(k in gate for k in ('人工', '子命令', '本表校验', '无脚本'))
        if not gate_ok and '人工' not in manual:
            errors.append(f'#{rid} {rule}: 既没有机器门禁也没有标注人工复核')
        if manual.strip() in ('', '—') and not gate_ok:
            errors.append(f'#{rid} {rule}: 人工复核列为空且无机器门禁')
        # 回归测试文件必须存在
        for t in TICK.findall(tests):
            if t.endswith('.py') or t.endswith('.sh'):
                if not resolve(root, t):
                    errors.append(f'#{rid} {rule}: 回归测试不存在 → `{t}`')
    return errors


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('cmd', choices=['check'])
    ap.add_argument('--root', default=os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
    a = ap.parse_args()
    root = os.path.abspath(a.root)
    errors = check(root)
    for e in errors:
        print(f'FAIL {e}')
    rows = len(parse_rows(open(os.path.join(root, MAP_DOC), encoding='utf-8').read())) if os.path.isfile(os.path.join(root, MAP_DOC)) else 0
    print(f'规则门禁映射: rules={rows} errors={len(errors)}')
    return 1 if errors else 0


if __name__ == '__main__':
    sys.exit(main())
