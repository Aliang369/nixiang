#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""游戏文档覆盖率审计（2026-10-27 第五轮）。

扫描 `10_docs/{游戏}/` 全部目录，报告每个游戏的文档覆盖状态：
  - 是否有 `硬约束.md`
  - 是否有 `路线锁定.md`
  - 是否有 `_state/games/{游戏}.md` 状态摘要
  - 该目录是否在某个 pipeline / 出包脚本里（即「在用」）
并按「活动但缺必要文档」输出阻断清单。

设计原则（对应 AGENTS.md §1.1 / 硬约束模板）：
  **不批量创建空文件**。缺文档不等于错误：只有「在用」且「缺**必须**文档」才阻断。
  - 在用 = 有 `pipeline.json` 或有生产出包脚本，或状态摘要里出现「进行中/推进/待验收」关键字。
  - 必须文档：在用游戏必须有 `硬约束.md`（用户要求/授权边界）与状态摘要；
    `路线锁定.md` 只在管线阶段要求时（pipeline.json 含 `route_lock` 或 `路线锁定`）才是必须。

用法：
    python3 -B 04_scripts/_common/audit_game_docs_coverage.py scan [--write] [--json]
    python3 -B 04_scripts/_common/audit_game_docs_coverage.py check   # 只在「活动且缺必须文档」时 rc=1
    python3 -B 04_scripts/_common/audit_game_docs_coverage.py scan --root <tmp>   # 测试
退出码：0 通过 / 1 有阻断项 / 2 环境问题。
"""
import argparse
import json
import os
import re
import sys

ACTIVE_HINT = re.compile(r'进行中|推进中|正在推进|待验收|未收尾|待测|待装机')
STATUS_BUILTIN = '（无状态摘要）'
RECENT = re.compile(r'2026-10-2[0-9]')
DELIVERED = re.compile(r'已交付|已收尾|已结案|收官完成|已判停')


def read(path):
    try:
        with open(path, encoding='utf-8', errors='ignore') as f:
            return f.read()
    except OSError:
        return ''


def scan(root):
    docs = os.path.join(root, '10_docs')
    state = os.path.join(docs, '_state', 'games')
    scripts = os.path.join(root, '04_scripts')
    rows = []
    if not os.path.isdir(docs):
        return rows

    # 有出包入口的游戏
    entry_games = set()
    for name in os.listdir(scripts) if os.path.isdir(scripts) else []:
        d = os.path.join(scripts, name)
        if not os.path.isdir(d):
            continue
        if os.path.isfile(os.path.join(d, 'pipeline.json')):
            entry_games.add(name)
        for f in os.listdir(d):
            if f.endswith('.sh') and any(k in f.lower() for k in ('surgical', 'so_only', 'inject', 'build_')):
                entry_games.add(name)

    for game in sorted(os.listdir(docs)):
        gdir = os.path.join(docs, game)
        if not os.path.isdir(gdir) or game.startswith('_') or game in ('双机协作', 'tools'):
            continue
        summary_path = os.path.join(state, game + '.md')
        summary = read(summary_path)
        has_summary = os.path.isfile(summary_path)
        pipeline = os.path.join(scripts, game, 'pipeline.json')
        pipeline_txt = read(pipeline)
        need_route_lock = bool(re.search(r'route_lock|路线锁定', pipeline_txt))
        # 「活动」只认状态摘要里的进行中信号（出包脚本存在不等于在推进：多数是已收尾游戏的历史脚本）
        head = summary[:1200]
        # 活动 = 摘要头部有「进行中/待验收」类词，或最近 7 天有更新日期（2026-10-2x）
        delivered = bool(DELIVERED.search(head))
        active = bool(has_summary and (ACTIVE_HINT.search(head) or (RECENT.search(head) and not delivered)))
        rows.append({
            'game': game,
            'has_hard_constraints': os.path.isfile(os.path.join(gdir, '硬约束.md')),
            'has_route_lock': os.path.isfile(os.path.join(gdir, '路线锁定.md')),
            'has_summary': has_summary,
            'summary_head': (summary.split('\n')[0][:60] if has_summary else STATUS_BUILTIN),
            'active': active,
            'need_route_lock': need_route_lock,
            'has_entry': game in entry_games,
        })
    return rows


def evaluate(rows):
    blockers, notes = [], []
    for r in rows:
        if not r['active']:
            continue
        if not r['has_hard_constraints'] and r['has_entry']:
            blockers.append(f"{r['game']}: 活动游戏缺 `10_docs/{r['game']}/硬约束.md`（用户要求/授权边界无落点）")
        if not r['has_summary'] and r['has_entry']:
            blockers.append(f"{r['game']}: 活动游戏缺 `_state/games/{r['game']}.md` 状态摘要")
        if r['need_route_lock'] and not r['has_route_lock']:
            blockers.append(f"{r['game']}: 管线要求路线锁定但缺 `10_docs/{r['game']}/路线锁定.md`")
    inactive_missing = [r['game'] for r in rows if not r['active'] and not (r['has_hard_constraints'] or r['has_route_lock'])]
    # 活动但无出包入口、且缺硬约束：不阻断，列人工确认（无入口＝未必有用户要求可循）
    need_confirm = [r['game'] for r in rows
                    if r['active'] and not r['has_entry'] and not r['has_hard_constraints']]
    if need_confirm:
        notes.append(f'{len(need_confirm)} 个摘要显示活动但无出包入口且无硬约束，需人工确认是否需要补：'
                     + '、'.join(need_confirm))
    note = f"{len(inactive_missing)} 个非活动目录无硬约束/路线锁定（按规则**不批量创建**，需要时按证据补）"
    return blockers, note, notes


def render(rows):
    need = sum(1 for r in rows if r['active'])
    out = ['# 游戏文档覆盖清单（机器生成，勿手改）', '',
           '> 生成命令：`python3 -B 04_scripts/_common/audit_game_docs_coverage.py scan --write`（2026-10-27 第五轮）。',
           '> 规则：**不批量创建空 `硬约束.md`/`路线锁定.md`**；只有「在用且缺必须文档」才阻断。', '',
           f'- 游戏目录总数：**{len(rows)}**',
           f'- 有 `硬约束.md`：**{sum(1 for r in rows if r["has_hard_constraints"])}**',
           f'- 有 `路线锁定.md`：**{sum(1 for r in rows if r["has_route_lock"])}**',
           f'- 有状态摘要：**{sum(1 for r in rows if r["has_summary"])}**',
           f'- 在用（有出包入口或摘要显示在推进）：**{need}**', '',
           '| 游戏 | 硬约束 | 路线锁定 | 状态摘要 | 在用 | 需路线锁定 |', '|---|---|---|---|---|---|']
    for r in rows:
        out.append('| `%s` | %s | %s | %s | %s | %s |' % (
            r['game'],
            '✅' if r['has_hard_constraints'] else '—',
            '✅' if r['has_route_lock'] else '—',
            '✅' if r['has_summary'] else '—',
            '是' if r['active'] else '否',
            '是' if r['need_route_lock'] else '—'))
    return '\n'.join(out)


def main():
    # 2026-10-27 双机：win 下 stdout 默认 cp936，打印 ✅ 会 UnicodeEncodeError 崩；
    # 只调本进程自己的流，mac 侧无变化。
    for _s in (sys.stdout, sys.stderr):
        try:
            _s.reconfigure(encoding='utf-8')
        except (AttributeError, ValueError):
            pass
    ap = argparse.ArgumentParser()
    ap.add_argument('cmd', choices=['scan', 'check'])
    ap.add_argument('--root', default=os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
    ap.add_argument('--write', action='store_true')
    ap.add_argument('--json', action='store_true')
    a = ap.parse_args()
    root = os.path.abspath(a.root)
    rows = scan(root)
    if not rows:
        print('FAIL 未找到 10_docs/{游戏} 目录（root 是否正确？）')
        return 2
    if a.cmd == 'scan':
        print(json.dumps(rows, ensure_ascii=False, indent=2) if a.json else render(rows))
        blockers, note, notes = evaluate(rows)
        print('\n' + note, file=sys.stderr)
        for n in notes:
            print(n, file=sys.stderr)
        if a.write:
            out = os.path.join(root, '10_docs', '_state', '游戏文档覆盖清单.md')
            # 2026-10-27 双机：强制 LF，避免 win 侧生成物带上 CRLF（跨机逐字节不等）
            with open(out, 'w', encoding='utf-8', newline='\n') as f:
                f.write(render(rows) + '\n')
            print(f'[written] {out}', file=sys.stderr)
        return 0
    blockers, note, notes = evaluate(rows)
    print(f'NOTE {note}')
    for n in notes:
        print(f'NOTE {n}')
    for b in blockers:
        print(f'FAIL {b}')
    print(f'游戏文档覆盖: dirs={len(rows)} active={sum(1 for r in rows if r["active"])} blockers={len(blockers)}')
    return 1 if blockers else 0


if __name__ == '__main__':
    sys.exit(main())
