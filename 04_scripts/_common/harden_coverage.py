#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""加固门禁覆盖率审计（2026-10-27 第六轮重写）。

目标：把「哪些入口真的产出正式 APK」「哪个入口真的执行加固门禁」用代码判出来，
不再靠文件名子串或脚本里出现 `harden` 字样。

扫描范围（递归）：
  - `04_scripts/**`               （全部 .sh / .py）
  - `mod菜单/**`                  （全部 .sh / .py，含 surgical_*.sh）
  - 其它顶层生产目录下的 .sh/.py（`01_tools/` 除外：那是工具，不是出包入口）

入口分类（每项只归一类，按证据强弱判定）：
  final_apk   —— 产出/交付正式 APK：出现 apksigner/jarsigner 且有 .apk 产物写到 08_output，
                 或显式把 .apk 复制进 08_output / delivery 目录；
                 或脚本名/内容表明组装成品 APK（`*_assemble_*.py`、`zip ... .apk`）。
  finalizer   —— 公共最终化/交付脚本（finalize_game_delivery.sh 等），内部强制加固门禁。
  component   —— 只产出 .so / .dex / 中间产物（ndk-build、javac/d8、cp 到 libs_* / libs_<tag>）。
  test_diag   —— dump / check / compare / verify / fetch / install / cleanup / env 等诊断与辅助入口。

保护判定（只对 final_apk 强制）：
  protected = 脚本内调用 `harden_gate.sh` 或 `harden_apk.py check`
            = 或调用 `finalize_game_delivery.sh`（该脚本内部强制门禁 → 链条保护）
            = 或所属游戏 `pipeline.json` 有 harden 阶段且脚本由 pipeline 驱动
  其它情况一律未保护。

用法：
    python3 -B 04_scripts/_common/harden_coverage.py scan   [--write] [--json]
    python3 -B 04_scripts/_common/harden_coverage.py check  [--strict]
    python3 -B 04_scripts/_common/harden_coverage.py islands   # 只列未保护的正式 APK 入口
    （--root 用于测试临时仓库；登记表只从 <root> 内读取，不会读真实仓库）
退出码：0 通过 / 1 有违规（--strict 下未保护的 final_apk 也算失败）/ 2 环境问题。
"""
import argparse
import json
import os
import re
import sys

SIGN = re.compile(r'\bapksigner\b|\bjarsigner\b')
ASSEMBLE = re.compile(r'zip\s+[^\n]*\.apk|_assemble_|assemble_.*\.apk|cp\s+[^\n]*\.apk')
OUT08 = re.compile(r'08_output')
GATE = re.compile(r'harden_gate\.sh|harden_apk\.py\s+check')
FINALIZER = re.compile(r'finalize_game_delivery\.sh')
COMPONENT = re.compile(r'ndk-build|cmake\s+--build|javac\b|d8\s|dx\s|libs_[a-z0-9_]+|libstarry_magic\.so')
DIAG = re.compile(r'^(dump_|check_|compare_|verify_|fetch_|install_|cleanup_|env|archive|case_)', re.I)
ESCAPE = re.compile(r'--allow-no-harden|STAGE_NEXT_FORCE=1')
REASON = re.compile(r'--harden-reason|HARDEN_REASON')
GAMES_RE = re.compile(r'^    "([^"]+)": \{', re.M)
SKIP_DIRS = {'.git', '01_tools', '06_dumped', '07_patched', '08_output', '09_test', '02_apks', '__pycache__'}
# 审计/门禁工具自身不是出包入口（否则会把正则里的门禁字符串误判成调用）
SKIP_FILES = {'04_scripts/_common/harden_coverage.py', '04_scripts/_common/harden_gate.sh',
              '04_scripts/_common/harden_apk.py', '04_scripts/_common/audit_rule_gate_map.py'}
EXEMPTIONS = {
    '04_scripts/_common/pre_install_gate.sh': '装机侧查11 的显式逃生门；必须同时传 --harden-reason 才放行',
    '04_scripts/_common/finalize_game_delivery.sh': '交付侧显式逃生门；必须同时传 --harden-reason 才放行',
    '04_scripts/_common/harden_gate.sh': '门禁自身实现',
    '04_scripts/_common/harden_coverage.py': '审计工具自身',
}


def read(path):
    try:
        with open(path, encoding='utf-8', errors='ignore') as f:
            return f.read()
    except OSError:
        return ''


def iter_entries(root):
    for base in ('04_scripts', 'mod菜单'):
        d = os.path.join(root, base)
        if not os.path.isdir(d):
            continue
        for dp, dn, fn in os.walk(d):
            dn[:] = [x for x in dn if x not in SKIP_DIRS]
            for f in sorted(fn):
                rel = os.path.relpath(os.path.join(dp, f), root)
                if not f.endswith(('.sh', '.py')) or f.endswith('_test.py') or f.startswith('test_'):
                    continue
                if rel in SKIP_FILES:
                    continue
                yield rel


def classify(rel, body):
    name = os.path.basename(rel)
    signs = bool(SIGN.search(body))
    out08 = bool(OUT08.search(body))
    asm = bool(ASSEMBLE.search(body))
    if os.path.basename(rel) == 'finalize_game_delivery.sh':
        return 'finalizer'
    if DIAG.match(name) and not (signs and out08):
        return 'test_diag'
    if signs and (out08 or asm):
        return 'final_apk'
    if asm and out08:
        return 'final_apk'
    if COMPONENT.search(body) and not signs:
        return 'component'
    if signs and not out08:
        # 只签名不落 08_output：可能是临时验证包
        return 'test_diag'
    return 'other'


def scan(root):
    root = os.path.abspath(root)
    entries = []
    for rel in iter_entries(root):
        body = read(os.path.join(root, rel))
        kind = classify(rel, body)
        game_dir = rel.split(os.sep)[1] if rel.startswith('04_scripts' + os.sep) and rel.count(os.sep) > 1 else ''
        if rel.startswith('mod菜单'):
            game_dir = 'mod菜单'
        gate = bool(GATE.search(body))
        fin = bool(FINALIZER.search(body))
        pipe = os.path.join(root, '04_scripts', game_dir, 'pipeline.json')
        pipeline_harden = 'harden' in read(pipe) if os.path.isfile(pipe) else False
        entries.append({
            'script': rel, 'game_dir': game_dir, 'kind': kind,
            'gate_call': gate, 'via_finalizer': fin,
            'escape': sorted(set(ESCAPE.findall(body))),
            'has_reason': bool(REASON.search(body)),
            'protected': bool(gate or fin or (pipeline_harden and kind == 'final_apk')),
        })

    apk = read(os.path.join(root, '04_scripts', '_common', 'harden_apk.py'))
    games = {}
    if 'GAMES = {' in apk:
        block = apk[apk.index('GAMES = {'):]
        games['harden_apk.py'] = GAMES_RE.findall(block)
    tag_map = {}
    for m in re.finditer(r'"([^"]+)":\s*\{\s*\n(?:\s*#[^\n]*\n)*\s*"tag":\s*"([^"]+)"', apk):
        tag_map[m.group(1)] = m.group(2)

    pig = read(os.path.join(root, '04_scripts', '_common', 'pre_install_gate.sh'))
    return {
        'root': root,
        'entries': entries,
        'games': games,
        'tag_map': tag_map,
        'install_gate': bool('harden_apk.py' in pig and 'stamp' in pig),
        'install_gate_reason': bool(REASON.search(pig)),
        'pipelines': sorted(
            os.path.basename(os.path.dirname(os.path.join(root, p)))
            for p in _pipelines(root)),
    }


def _pipelines(root):
    out = []
    base = os.path.join(root, '04_scripts')
    if not os.path.isdir(base):
        return out
    for name in sorted(os.listdir(base)):
        p = os.path.join(base, name, 'pipeline.json')
        if os.path.isfile(p):
            out.append(p)
    return out


def check(data, strict=False):
    errors, warns = [], []
    gnames = set(data['games'].get('harden_apk.py', []))
    finals = [e for e in data['entries'] if e['kind'] == 'final_apk']

    for e in finals:
        if not e['protected']:
            msg = f"{e['script']}: 正式 APK 入口未接入加固门禁（无 harden_gate.sh / harden_apk.py check / finalize 链条）"
            (errors if strict else warns).append(msg)
        if e['gate_call'] and e['game_dir'] not in gnames and e['game_dir'] not in ('mod菜单', '_common', ''):
            errors.append(f"{e['script']}: 调用门禁但游戏「{e['game_dir']}」未在 harden_apk.py GAMES 登记")
    harden_escape = re.compile(r'--allow-no-harden')
    for e in data['entries']:
        if e['kind'] not in ('final_apk', 'finalizer'):
            continue
        # 只有「绕过加固」的逃生门才必须带理由；STAGE_NEXT_FORCE 属阶段控制器
        if any(harden_escape.search(x) for x in e['escape']) or '--allow-no-harden' in read(os.path.join(data['root'], e['script'])):
            if e['script'] in EXEMPTIONS:
                continue
            if not e['has_reason']:
                errors.append(f"{e['script']}: 使用 --allow-no-harden 但没有理由参数（--harden-reason / HARDEN_REASON）")
    if not data['install_gate']:
        errors.append('pre_install_gate.sh 未校验加固凭证（查11 失效）')
    if data['install_gate_reason'] is False:
        warns.append('pre_install_gate.sh 的 --allow-no-harden 尚未强制要求理由（待修）')
    reg = read(os.path.join(data['root'], '10_docs', '防御加固_覆盖登记.md'))
    for line in reg.splitlines():
        if line.startswith('|') and '已接入' in line:
            cells = [c.strip() for c in line.strip('|').split('|')]
            if len(cells) >= 2:
                rel = cells[1].strip('`')
                ent = next((x for x in data['entries'] if x['script'] == rel), None)
                if ent and not ent['protected']:
                    errors.append(f'{rel}: 登记为「已接入」但脚本里没有实际门禁调用（登记漂移）')
    return errors, warns


def summary(data):
    finals = [e for e in data['entries'] if e['kind'] == 'final_apk']
    prot = [e for e in finals if e['protected']]
    comps = [e for e in data['entries'] if e['kind'] == 'component']
    diag = [e for e in data['entries'] if e['kind'] == 'test_diag']
    return finals, prot, comps, diag


def render(data):
    finals, prot, comps, diag = summary(data)
    out = ['# 加固门禁覆盖登记（机器生成，勿手改）', '',
           '> 生成：`python3 -B 04_scripts/_common/harden_coverage.py scan --write`（第六轮重写：递归扫描 + 分类）。',
           '> 判定口径：只有「产出/交付正式 APK」的入口才强制加固；组件构建与诊断入口不计未接入。', '',
           '## 1. 汇总（实测）', '',
           f'- 扫描入口总数：**{len(data["entries"])}**（`04_scripts/**` + `mod菜单/**`，跳过 01_tools/产物目录）',
           f'- 正式 APK 入口（final_apk）：**{len(finals)}**，其中已受门禁保护 **{len(prot)}**、未保护 **{len(finals)-len(prot)}**',
           f'- 组件构建入口（component）：{len(comps)}；诊断/辅助入口（test_diag）：{len(diag)}',
           f'- `harden_apk.py` GAMES 登记：{len(data["games"].get("harden_apk.py", []))} 个 —— '
           + '、'.join(f'`{g}`' for g in data['games'].get('harden_apk.py', [])),
           f'- 装机侧凭证校验（查11）：{"✅ 在位" if data["install_gate"] else "❌ 缺失"}；'
           f'理由强制：{"✅" if data.get("install_gate_reason") else "❌ 未强制"}',
           f'- pipeline.json 数量：{len(data["pipelines"])}', '',
           '## 2. 正式 APK 入口（必须受门禁保护）', '',
           '| 入口 | 游戏目录 | 门禁调用 | 经 finalize | 逃生门 | 状态 |', '|---|---|---|---|---|---|']
    for e in finals:
        out.append('| `%s` | `%s` | %s | %s | %s | %s |' % (
            e['script'], e['game_dir'] or '—',
            '✅' if e['gate_call'] else '—', '✅' if e['via_finalizer'] else '—',
            '、'.join(e['escape']) or '—',
            '已接入' if e['protected'] else '**未接入**'))
    out += ['', '## 3. 组件构建入口（不计未接入；但须证明其产物流入受保护的正式 APK 路径）', '',
            '| 入口 | 游戏目录 |', '|---|---|']
    for e in comps:
        out.append(f"| `{e['script']}` | `{e['game_dir'] or '—'}` |")
    out += ['', '## 4. 诊断/辅助入口（不产出正式 APK）', '',
            f'共 {len(diag)} 个，逐项见 `scan --json`。', '']
    return '\n'.join(out)


def render_islands(data):
    finals, prot, _c, _d = summary(data)
    out = ['未受加固保护的正式 APK 入口（`islands`）：', '']
    for e in finals:
        if not e['protected']:
            out.append(f"- `{e['script']}`（游戏目录 `{e['game_dir'] or '—'}`）")
    if len(out) == 2:
        out.append('（无）')
    return '\n'.join(out)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('cmd', choices=['scan', 'check', 'islands'])
    ap.add_argument('--root', default=os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..'))
    ap.add_argument('--json', action='store_true')
    ap.add_argument('--write', action='store_true')
    ap.add_argument('--strict', action='store_true')
    a = ap.parse_args()
    data = scan(a.root)

    if a.cmd == 'scan':
        print(json.dumps(data, ensure_ascii=False, indent=2) if a.json else render(data))
        if a.write:
            out = os.path.join(data['root'], '10_docs', '防御加固_覆盖登记.md')
            with open(out, 'w', encoding='utf-8') as f:
                f.write(render(data).rstrip('\n') + '\n')
            print(f'[written] {out}', file=sys.stderr)
        return 0
    if a.cmd == 'islands':
        print(render_islands(data))
        return 0

    errors, warns = check(data, strict=a.strict)
    for w in warns:
        print(f'WARN {w}')
    for e in errors:
        print(f'FAIL {e}')
    finals, prot, _c, _d = summary(data)
    print(f'加固覆盖审计: entries={len(data["entries"])} final_apk={len(finals)} protected={len(prot)} '
          f'errors={len(errors)} warnings={len(warns)} strict={int(bool(a.strict))}')
    return 1 if errors else 0


if __name__ == '__main__':
    sys.exit(main())
