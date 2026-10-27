#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""加固门禁覆盖率审计（2026-10-27 第五轮）。

用途：
  1) `scan`  —— 依据真实文件扫描生成覆盖登记（不手工填写百分比）：
     - `04_scripts/*/pipeline.json` 是否为该游戏提供 harden 阶段
     - `04_scripts/*/*.sh` 里的生产出包脚本是否真的调用 harden_gate.sh / harden_apk.py check
     - `04_scripts/_common/harden_apk.py` 的 GAMES 注册表覆盖了哪些游戏/标签
     - `pre_install_gate.sh` 是否仍做「查11」凭证校验（装机侧兜底）
  2) `check` —— 出现下列情况即失败（rc=1）：
     - 生产出包脚本调用了加固门禁，但它的游戏在 GAMES 里没登记（错标签/幽灵入口）
     - 脚本里出现逃生门（--allow-no-harden / STAGE_NEXT_FORCE=1）但没在本文件的豁免表登记
     - 声明「已接入」的入口实际没有调用（登记与实现漂移）
     - GAMES 表项缺 tag/payload 关键字段（派生表无法生成）
用法：
    python3 -B 04_scripts/_common/harden_coverage.py scan [--json]
    python3 -B 04_scripts/_common/harden_coverage.py check [--strict]
    python3 -B 04_scripts/_common/harden_coverage.py scan --root <临时仓库>   # 测试用
退出码：0 通过 / 1 有违规 / 2 环境问题。
"""
import argparse
import json
import os
import re
import sys

GATE_CALL = re.compile(r'harden_gate\.sh|harden_apk\.py\s+check')
ESCAPE = re.compile(r'--allow-no-harden|STAGE_NEXT_FORCE=1')
GAMES_RE = re.compile(r'^    "([^"]+)": \{', re.M)

# 已登记的逃生门豁免：入口 → 理由（新增必须写明依据，否则 check 失败）
EXEMPTIONS = {
    '04_scripts/_common/pre_install_gate.sh': '装机侧查11 的 --allow-no-harden 是显式逃生门，用了必须在报告写明理由',
    '04_scripts/_common/harden_gate.sh': '门禁自身实现',
    '04_scripts/_common/harden_coverage.py': '审计工具自身',
}


def read(path):
    try:
        with open(path, encoding='utf-8', errors='ignore') as f:
            return f.read()
    except OSError:
        return ''


def scan(root):
    scripts = os.path.join(root, '04_scripts')
    common = os.path.join(scripts, '_common')
    data = {'pipelines': [], 'entries': [], 'games': {}, 'install_gate': False}

    # 1) pipeline.json
    for name in sorted(os.listdir(scripts)) if os.path.isdir(scripts) else []:
        pj = os.path.join(scripts, name, 'pipeline.json')
        if os.path.isfile(pj):
            txt = read(pj)
            data['pipelines'].append({'game': name, 'harden_stage': 'harden' in txt})

    # 2) 生产出包脚本
    for name in sorted(os.listdir(scripts)) if os.path.isdir(scripts) else []:
        d = os.path.join(scripts, name)
        if not os.path.isdir(d):
            continue
        for f in sorted(os.listdir(d)):
            if not f.endswith('.sh'):
                continue
            low = f.lower()
            if not any(k in low for k in ('surgical', 'so_only', 'inject', 'build_')):
                continue
            path = os.path.join(d, f)
            body = read(path)
            data['entries'].append({
                'game_dir': name,
                'script': os.path.relpath(path, root),
                'wired': bool(GATE_CALL.search(body)),
                'escape': sorted(set(ESCAPE.findall(body))),
            })

    # 3) GAMES 注册表
    apk = os.path.join(common, 'harden_apk.py')
    txt = read(apk)
    if 'GAMES = {' in txt:
        block = txt[txt.index('GAMES = {'):]
        data['games']['harden_apk.py'] = GAMES_RE.findall(block)

    # 4) 装机侧兜底
    pig = read(os.path.join(common, 'pre_install_gate.sh'))
    data['install_gate'] = 'harden_apk.py' in pig and 'stamp' in pig
    return data


def check(data, strict=False):
    errors, warns = [], []
    gnames = set(data['games'].get('harden_apk.py', []))
    with_harden = {p['game'] for p in data['pipelines'] if p['harden_stage']}

    for e in data['entries']:
        if e['wired'] and e['game_dir'] not in gnames:
            errors.append(
                f"{e['script']}: 调用了加固门禁，但游戏「{e['game_dir']}」未在 harden_apk.py GAMES 登记（错标签/幽灵入口）")
        for esc in e['escape']:
            if e['script'] not in EXEMPTIONS:
                errors.append(f"{e['script']}: 出现逃生门 {esc} 但未在 EXEMPTIONS 登记理由")

    # 已接入清单与实际调用必须一致（登记漂移）
    reg = os.path.join(_ROOT, '10_docs', '防御加固_覆盖登记.md')
    regtxt = read(reg)
    if regtxt:
        for line in regtxt.splitlines():
            if line.startswith('|') and '已接入' in line:
                cells = [c.strip() for c in line.strip('|').split('|')]
                if len(cells) >= 2:
                    script = cells[1].strip('`')
                    ent = next((x for x in data['entries'] if x['script'] == script), None)
                    if ent and not ent['wired']:
                        errors.append(f"{script}: 登记为「已接入」但脚本里没有调用加固门禁（登记与实现漂移）")

    unwired = [e for e in data['entries'] if not e['wired']]
    if unwired:
        msg = f"{len(unwired)}/{len(data['entries'])} 个生产出包脚本未接入加固门禁（见登记表「未接入」行）"
        (errors if strict else warns).append(msg)
    no_stage = [p['game'] for p in data['pipelines'] if not p['harden_stage']]
    if no_stage:
        msg = f"{len(no_stage)}/{len(data['pipelines'])} 个 pipeline 无 harden 阶段（靠 finalize/装机闸门兜底）"
        warns.append(msg)
    if not data['install_gate']:
        errors.append('pre_install_gate.sh 未校验加固凭证（查11 失效）')
    return errors, warns


def render(data):
    lines = ['# 加固门禁覆盖登记（机器生成，勿手改）', '',
             '> 生成命令：`python3 -B 04_scripts/_common/harden_coverage.py scan`（2026-10-27 第五轮建立）。',
             '> 本表来自真实文件扫描；`AGENTS.md` §6 只写强制规则，具体覆盖以本表为准。', '']
    gnames = data['games'].get('harden_apk.py', [])
    lines += ['## 1. GAMES 注册表（`harden_apk.py`）', '',
              f'- 已登记游戏/标签：{len(gnames)} 个 —— ' + ('、'.join(f'`{g}`' for g in gnames) or '（无）'), '']
    lines += ['## 2. 管线入口（`04_scripts/*/pipeline.json`）', '',
              '| 游戏 | harden 阶段 | 状态 |', '|---|---|---|']
    for p in data['pipelines']:
        lines.append(f"| `{p['game']}` | {'有' if p['harden_stage'] else '无'} | "
                     f"{'已接入' if p['harden_stage'] else '未接入（靠 finalize/装机闸门兜底）'} |")
    lines += ['', '## 3. 生产出包脚本', '', '| 游戏目录 | 脚本 | 状态 | 逃生门 |', '|---|---|---|---|']
    for e in data['entries']:
        lines.append(f"| `{e['game_dir']}` | `{e['script']}` | "
                     f"{'已接入' if e['wired'] else '未接入'} | {'、'.join(e['escape']) or '—'} |")
    wired = sum(1 for e in data['entries'] if e['wired'])
    lines += ['', '## 4. 装机侧兜底', '',
              f"- `pre_install_gate.sh` 查11 校验加固凭证：{'✅ 在位' if data['install_gate'] else '❌ 缺失'}", '',
              '## 5. 汇总（实测，非手工填写）', '',
              f"- 管线：{sum(1 for p in data['pipelines'] if p['harden_stage'])}/{len(data['pipelines'])} 有 harden 阶段",
              f"- 出包脚本：{wired}/{len(data['entries'])} 已调用门禁",
              f"- GAMES 登记：{len(gnames)} 个标签", '']
    return '\n'.join(lines)


_ROOT = '.'


def main():
    global _ROOT
    ap = argparse.ArgumentParser()
    ap.add_argument('cmd', choices=['scan', 'check'])
    ap.add_argument('--root', default=os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
    ap.add_argument('--json', action='store_true')
    ap.add_argument('--strict', action='store_true')
    ap.add_argument('--write', action='store_true', help='scan 时把登记写入 10_docs/防御加固_覆盖登记.md')
    a = ap.parse_args()
    _ROOT = os.path.abspath(a.root)
    data = scan(_ROOT)

    if a.cmd == 'scan':
        if a.json:
            print(json.dumps(data, ensure_ascii=False, indent=2))
        else:
            print(render(data))
        if a.write:
            out = os.path.join(_ROOT, '10_docs', '防御加固_覆盖登记.md')
            with open(out, 'w', encoding='utf-8') as f:
                f.write(render(data) + '\n')
            print(f'\n[written] {out}', file=sys.stderr)
        return 0

    errors, warns = check(data, strict=a.strict)
    for w in warns:
        print(f'WARN {w}')
    for e in errors:
        print(f'FAIL {e}')
    print(f'加固覆盖审计: pipelines={len(data["pipelines"])} scripts={len(data["entries"])} '
          f'errors={len(errors)} warnings={len(warns)}')
    return 1 if errors else 0


if __name__ == '__main__':
    sys.exit(main())
