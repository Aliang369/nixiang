#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""加固门禁覆盖率审计（2026-10-27 第八轮重写：可信证据版）。

目标：用**代码证据**判出「哪些入口真的产出正式 APK」「哪个入口真的执行了加固门禁」，
不靠文件名子串、不靠注释提及、不靠「同游戏 pipeline 里有 harden 字样」。

扫描范围（递归）：
  - `04_scripts/**` 与 `mod菜单/**` 下的 .sh / .py（跳过产物目录、测试与工具自身）

入口分类（每项只归一类）：
  final_apk   —— 产出/交付正式 APK（代码里 apksigner/jarsigner + .apk 产物落 08_output，
                 或 zip 组装成品，或 *_assemble_*.py）
  finalizer   —— 公共最终化/交付脚本
  component   —— 只产出 .so / .dex 等中间产物
  probe       —— 脚本自述「探针包 / 不作交付」或文件名含 probe 段
  test_diag   —— dump/check/compare/verify/fetch/install/cleanup/env 等辅助入口
  other       —— 其余（不作为出包入口）

保护判定（只对 final_apk/finalizer 强制）：
  1) 真实门禁调用：`.sh` 里 `harden_gate.sh` / `harden_apk.py check`；
     `.py` 里必须同时有**执行上下文**（subprocess.run / check_call / check_output / Popen /
     os.system 等）与门禁参数（`harden_apk.py` + `check` + `--apk`），注释与帮助文字不算。
  2) 管线证据：脚本被某阶段 command 引用，且其后存在**真调用门禁的 harden 阶段**，
     并且 harden 阶段实际检查的 APK 与产出阶段声明的 APK **是同一个文件**；
     无法静态证明同一份 APK ⇒ 记 `unproven`（不算已保护）。
  3) 反向链条：被 `finalize_game_delivery.sh` 以**真实调用**（非注释/帮助/示例）调用的
     交付辅助入口，随该链条受保护。

用法：
    harden_coverage.py scan   [--write] [--json]
    harden_coverage.py check   [--strict] [--game <游戏名|tag>]
    harden_coverage.py islands [--game <游戏名|tag>]
退出码：0 通过 / 1 有违规（--strict 下未保护 final_apk 也算）/ 2 未知游戏名或 tag。
"""
import argparse
import json
import os
import re
import sys

SIGN = re.compile(r'\bapksigner\b|\bjarsigner\b')
ASSEMBLE = re.compile(r'zip\s+[^\n]*\.apk|_assemble_|assemble_.*\.apk|cp\s+[^\n]*\.apk')
OUT08 = re.compile(r'08_output')
GATE_SH = re.compile(r'harden_gate\.sh|harden_apk\.py\s+check')
PY_EXEC = re.compile(r'subprocess\.(?:run|call|check_call|check_output|Popen)|os\.system|os\.exec')
COMPONENT = re.compile(r'ndk-build|cmake\s+--build|javac\b|d8\s|dx\s|libs_[a-z0-9_]+|libstarry_magic\.so')
DIAG = re.compile(r'^(dump_|check_|compare_|verify_|fetch_|install_|cleanup_|env|archive|case_)', re.I)
PROBE_NAME = re.compile(r'(?:^|[_\-])probe(?:[_\-.]|$)', re.I)
PROBE_TEXT = re.compile(r'探针包|不作交付|不用于交付')
FINALIZER = re.compile(r'^[^#\n]*\b(?:bash|sh|python3?|\.)\s*[^\n]*finalize_game_delivery\.sh', re.M)
FINALIZE_VAR = re.compile(r'^\s*([A-Za-z_][A-Za-z0-9_]*)=["\']?[^\n#]*finalize_game_delivery\.sh', re.M)
ESCAPE = re.compile(r'--allow-no-harden')
REASON = re.compile(r'--harden-reason|HARDEN_REASON')
GATE_GAME = re.compile(r'--game\s+([^\s"\'<>）)]+)')
APK_TOKEN = re.compile(r'[\w./\-*]+\.apk')
GAMES_RE = re.compile(r'^    "([^"]+)": \{', re.M)
SKIP_DIRS = {'.git', '01_tools', '06_dumped', '07_patched', '08_output', '09_test', '02_apks', '__pycache__'}
SKIP_FILES = {'04_scripts/_common/harden_coverage.py', '04_scripts/_common/harden_gate.sh',
              '04_scripts/_common/harden_apk.py', '04_scripts/_common/audit_rule_gate_map.py'}
TOOL_DIRS = {'_common', '双机协作', ''}
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


def code_only(body):
    """去掉整行注释与行尾注释（引号外的 #），避免注释里的示例被当成真代码。"""
    out = []
    for line in body.splitlines():
        stripped = line.lstrip()
        if stripped.startswith('#'):
            continue
        hash_at = line.find('#')
        if hash_at > 0:
            before = line[:hash_at]
            if before.count('"') % 2 == 0 and before.count("'") % 2 == 0:
                line = before
        out.append(line)
    return '\n'.join(out)


def flat_tokens(text):
    """把 Python 参数列表等文本压平成可搜索串：去引号、去逗号。"""
    return re.sub(r'["\',()\[\]]', ' ', text)


def has_gate_call(rel, code):
    """真实门禁调用判定（第八轮：区分注释/帮助文字/真执行）。

    .sh：出现 `harden_gate.sh` 或 `harden_apk.py check` 即算（脚本本身就是执行体）。
    .py：必须在**同一执行语句窗口**里同时出现执行上下文（subprocess/os.system）、
         `harden_apk.py`+`check`、以及 `--apk`；单纯字符串赋值/帮助文字不算。
    """
    if not rel.endswith('.py'):
        return bool(GATE_SH.search(code))
    for m in PY_EXEC.finditer(code):
        # 取该执行调用起点后的一段（覆盖跨行参数列表），到下一个空行为止
        start = m.start()
        window = code[start:start + 900]
        stop = window.find('\n\n')
        if stop > 0:
            window = window[:stop]
        flat = flat_tokens(window)
        if re.search(r'harden_apk\.py\s+check\b', flat) and re.search(r'--apk\b', flat):
            return True
        if 'harden_gate.sh' in flat and re.search(r'\.apk\b', flat):
            return True
    return False


def iter_entries(root):
    for base in ('04_scripts', 'mod菜单'):
        d = os.path.join(root, base)
        if not os.path.isdir(d):
            continue
        for dp, dn, fn in os.walk(d):
            dn[:] = [x for x in dn if x not in SKIP_DIRS]
            for f in sorted(fn):
                if not f.endswith(('.sh', '.py')) or f.endswith('_test.py') or f.startswith('test_'):
                    continue
                rel = os.path.relpath(os.path.join(dp, f), root)
                if rel in SKIP_FILES:
                    continue
                yield rel


def classify(rel, raw):
    """分类只看**去注释后的代码**；探针自述允许来自注释（那是脚本的明确声明）。"""
    name = os.path.basename(rel)
    code = code_only(raw)
    if PROBE_NAME.search(name) or PROBE_TEXT.search(raw):
        return 'probe'
    signs = bool(SIGN.search(code))
    out08 = bool(OUT08.search(code))
    asm = bool(ASSEMBLE.search(code))
    if name == 'finalize_game_delivery.sh':
        return 'finalizer'
    if DIAG.match(name) and not (signs and out08):
        return 'test_diag'
    if signs and (out08 or asm):
        return 'final_apk'
    if asm and out08:
        return 'final_apk'
    if COMPONENT.search(code) and not signs:
        return 'component'
    if signs and not out08:
        return 'test_diag'
    return 'other'


def games_table(root):
    """解析 harden_apk.py 的 GAMES：名称 → tag，以及 --apk 默认值。"""
    apk = read(os.path.join(root, '04_scripts', '_common', 'harden_apk.py'))
    names = []
    tags = {}
    if 'GAMES = {' in apk:
        block = apk[apk.index('GAMES = {'):]
        end = block.find('\n}\n')
        block = block[:end if end > 0 else len(block)]
        names = GAMES_RE.findall(block)
        for m in re.finditer(r'"([^"]+)":\s*\{\s*\n(?:\s*#[^\n]*\n)*\s*"tag":\s*"([^"]+)"', block):
            tags[m.group(1)] = m.group(2)
    default_apk = ''
    m = re.search(r'"--apk",\s*default=os\.path\.join\(([^)]*)\)', apk, re.S)
    if m:
        parts = re.findall(r'"([^"]+)"', m.group(1))
        # 第一段通常是 ROOT 下的相对目录，最后一段是文件名
        if len(parts) >= 2:
            default_apk = '/'.join(parts[-2:])
    return {'names': names, 'tags': tags, 'default_apk': default_apk}


def canon_game(gt, wanted):
    """游戏名或 tag → 规范游戏名；未知返回 None。"""
    w = (wanted or '').strip()
    if not w:
        return None
    if w in gt['names']:
        return w
    for name, tag in gt['tags'].items():
        if w == tag:
            return name
    return None


def stage_apks(stage, gt, is_harden=False):
    """取一个阶段涉及的 APK（命令 + verify 里的 file_exists 路径）。"""
    paths = set()
    cmd = str(stage.get('command', ''))
    has_apk_flag = '--apk' in cmd
    explicit = re.findall(r'--apk\s+([^\s"\']*\.apk)', cmd)
    for p in explicit:
        paths.add(os.path.basename(p))
    # 命令里出现的 APK 路径（含 glob）都算候选；--apk 指向变量时靠这些 glob 证明同包
    for p in APK_TOKEN.findall(cmd):
        paths.add(os.path.basename(p))
    for v in (stage.get('verify') or []):
        if isinstance(v, dict) and str(v.get('type')) == 'file_exists':
            p = str(v.get('path', ''))
            if p.endswith('.apk'):
                paths.add(os.path.basename(p))
    if is_harden and not has_apk_flag and gt.get('default_apk'):
        paths.add(os.path.basename(gt['default_apk']))
    return paths


def pipeline_evidence(root, game_dir, script_name, gt):
    """同包证据：脚本被某阶段引用 + 其后有真调用门禁的 harden 阶段 + 两者指向同一 APK。"""
    if not game_dir or game_dir in TOOL_DIRS:
        return {}
    pipe = os.path.join(root, '04_scripts', game_dir, 'pipeline.json')
    if not os.path.isfile(pipe):
        return {}
    try:
        data = json.loads(read(pipe))
    except ValueError:
        return {}
    stages = data.get('stages') if isinstance(data, dict) else data
    if not isinstance(stages, list):
        return {}

    own_idx = own_id = None
    for i, st in enumerate(stages):
        if isinstance(st, dict) and script_name and script_name in str(st.get('command', '')):
            own_idx, own_id = i, st.get('id')
            break
    harden_idx = harden_id = None
    for i, st in enumerate(stages):
        if not isinstance(st, dict):
            continue
        cmd = str(st.get('command', ''))
        if re.search(r'harden_gate\.sh|harden_apk\.py\s+(?:check|redteam)', cmd):
            harden_idx, harden_id = i, st.get('id')
            break

    out = {'stage': own_id, 'harden_stage': harden_id}
    if own_idx is None or harden_idx is None:
        return out
    if harden_idx <= own_idx:
        out['unproven'] = 'harden 阶段在同包入口之前，无法证明覆盖'
        return out
    produced = stage_apks(stages[own_idx], gt)
    checked = stage_apks(stages[harden_idx], gt, is_harden=True)
    out['produced_apk'] = sorted(produced)
    out['checked_apk'] = sorted(checked)

    def same_apk(a_set, b_set):
        import fnmatch
        for a in a_set:
            for b in b_set:
                if a == b or fnmatch.fnmatch(a, b) or fnmatch.fnmatch(b, a):
                    return True
        return False

    if produced and checked and same_apk(produced, checked):
        out['protected'] = True
    else:
        out['unproven'] = ('同包证据不足：产出 %s / 门禁检查 %s'
                           % (sorted(produced) or ['(未声明)'], sorted(checked) or ['(未声明)']))
    return out


def scan(root):
    root = os.path.abspath(root)
    gt = games_table(root)
    entries = []
    for rel in iter_entries(root):
        raw = read(os.path.join(root, rel))
        code = code_only(raw)
        kind = classify(rel, raw)
        game_dir = ''
        if rel.startswith('04_scripts' + os.sep) and rel.count(os.sep) > 1:
            game_dir = rel.split(os.sep)[1]
        elif rel.startswith('mod菜单'):
            game_dir = 'mod菜单'
        pev = pipeline_evidence(root, game_dir, os.path.basename(rel), gt)
        gate = has_gate_call(rel, code)
        fin = bool(FINALIZER.search(code))
        if not fin:
            for var in FINALIZE_VAR.findall(code):
                if re.search(r'^[^#\n]*\b(?:bash|sh)\b[^\n]*"?\$?%s"?' % re.escape(var), code, re.M):
                    fin = True
                    break
        entries.append({
            'script': rel, 'game_dir': game_dir, 'kind': kind,
            'gate_call': gate, 'via_finalizer': fin,
            'gate_games': sorted(set(GATE_GAME.findall(code))),
            'pipeline_stage': pev.get('stage'),
            'pipeline_harden_stage': pev.get('harden_stage'),
            'pipeline_evidence': pev.get('unproven', ''),
            'produced_apk': pev.get('produced_apk', []),
            'checked_apk': pev.get('checked_apk', []),
            'escape': sorted(set(ESCAPE.findall(code))),
            'has_reason': bool(REASON.search(code)),
            'protected': (bool(gate or fin or pev.get('protected'))
                          if kind in ('final_apk', 'finalizer') else False),
            'protected_by': ('gate' if gate else 'finalize-chain' if fin
                             else 'pipeline' if pev.get('protected') else ''),
        })

    # 反向链条：被 finalize_game_delivery.sh 以真实调用行引用的交付辅助入口
    fin_body = code_only(read(os.path.join(root, '04_scripts', '_common', 'finalize_game_delivery.sh')))
    for e in entries:
        if e['kind'] in ('final_apk', 'finalizer') and not e['protected']:
            base = os.path.basename(e['script'])
            if base == 'finalize_game_delivery.sh':
                continue
            call = re.compile(
                r'(?:^\s*|[;&|]\s*|\bthen\s+|\bdo\s+)(?:bash|sh|python3(?:\.\d+)?)\b[^\n]*\b%s\b'
                % re.escape(base), re.M)
            if call.search(fin_body):
                e['protected'] = True
                e['protected_by'] = 'called-by-finalizer'

    pig = code_only(read(os.path.join(root, '04_scripts', '_common', 'pre_install_gate.sh')))
    return {
        'root': root,
        'entries': entries,
        'games': gt,
        'install_gate': bool('harden_apk.py' in pig and 'stamp' in pig),
        'install_gate_reason': bool(REASON.search(pig)),
        'pipelines': sorted(os.path.basename(os.path.dirname(p))
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


def belongs(entry, game, gt):
    """入口是否属于该游戏（目录、门禁 --game 参数、mod菜单 tag 前缀）。"""
    name = game
    tag = gt['tags'].get(game, '')
    if entry['game_dir'] == name:
        return True
    if name in entry['gate_games'] or (tag and tag in entry['gate_games']):
        return True
    if entry['game_dir'] == 'mod菜单' and tag:
        base = os.path.basename(entry['script'])
        token = tag.split('_')[0]
        if token and (base.startswith(token + '_') or ('_' + token + '_') in base):
            return True
    return False


def check(data, strict=False, only_game=None):
    errors, warns = [], []
    gt = data['games']
    known = set(gt['names']) | set(gt['tags'].values())
    if only_game:
        game = canon_game(gt, only_game)
        if not game:
            return ['未知游戏名或 tag：%s（已登记：%s）'
                    % (only_game, ', '.join('%s/%s' % (n, t) for n, t in gt['tags'].items()))], []
        keep = []
        for e in data['entries']:
            if belongs(e, game, gt):
                keep.append(e)
            elif any(e['script'].endswith(x) for x in
                     ('finalize_game_delivery.sh', 'prune_output_final.sh', 'pre_install_gate.sh')):
                keep.append(e)
        data = dict(data, entries=keep)

    finals = [e for e in data['entries'] if e['kind'] == 'final_apk']
    for e in finals:
        if not e['protected']:
            msg = f"{e['script']}: 正式 APK 入口未接入加固门禁"
            if e['pipeline_evidence']:
                msg += f"（{e['pipeline_evidence']}）"
            (errors if strict else warns).append(msg)
    for e in data['entries']:
        if not e['gate_call'] or e['game_dir'] in TOOL_DIRS:
            continue  # 工具/门禁自身的帮助文字不算调用方
        if e['gate_games']:
            for g in e['gate_games']:
                if g not in known:
                    errors.append(f"{e['script']}: 门禁 --game 「{g}」 既不是 GAMES 游戏名也不是已登记 tag")
        elif e['game_dir'] not in TOOL_DIRS and e['game_dir'] not in known:
            errors.append(f"{e['script']}: 调用门禁但游戏「{e['game_dir']}」未登记且没有 --game 指明")
    for e in data['entries']:
        if e['kind'] not in ('final_apk', 'finalizer'):
            continue
        if any(ESCAPE.search(x) for x in e['escape']):
            if e['script'] in EXEMPTIONS or e['has_reason']:
                continue
            errors.append(f"{e['script']}: 使用 --allow-no-harden 但没有理由参数（--harden-reason / HARDEN_REASON）")
    if not data['install_gate']:
        errors.append('pre_install_gate.sh 未校验加固凭证（查11 失效）')
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
    probes = [e for e in data['entries'] if e['kind'] == 'probe']
    diag = [e for e in data['entries'] if e['kind'] == 'test_diag']
    return finals, prot, comps, probes, diag


def render(data):
    finals, prot, comps, probes, diag = summary(data)
    gt = data['games']
    out = ['# 加固门禁覆盖登记（机器生成，勿手改）', '',
           '> 生成：`python3 -B 04_scripts/_common/harden_coverage.py scan --write`（第八轮：代码证据版）。',
           '> 判定口径：只有「产出/交付正式 APK」的入口才强制加固；组件/诊断/探针入口不计未接入。',
           '> **迁移规则**：历史未触碰游戏的入口保持「未接入」，按游戏逐个迁移——某游戏被实际修改并准备正式出包/交付时才接入并验证它的最终 APK 入口，然后重跑本表。',
           '> **限定验收**：`python3 -B 04_scripts/_common/harden_coverage.py check --strict --game <游戏名|tag>`（名称与 tag 等价，只验该游戏入口 + 公共交付检查）。', '',
           '## 1. 汇总（实测）', '',
           f'- 扫描入口总数：**{len(data["entries"])}**',
           f'- 正式 APK 入口（final_apk）：**{len(finals)}**，已保护 **{len(prot)}**、未接入 **{len(finals)-len(prot)}**',
           f'- 组件构建 {len(comps)}｜探针（不作交付）{len(probes)}｜诊断/辅助 {len(diag)}',
           f'- GAMES 登记 {len(gt["names"])} 个：' + '、'.join(f'`{n}`(tag `{gt["tags"].get(n,"?")}`)' for n in gt['names']),
           f'- `harden_apk.py` 默认 `--apk`：`{gt["default_apk"] or "(未解析)"}`（管线省略 --apk 时按此路径检查）',
           f'- 装机侧凭证校验（查11）：{"✅" if data["install_gate"] else "❌"}；理由强制：{"✅" if data["install_gate_reason"] else "❌"}',
           f'- pipeline.json 数量：{len(data["pipelines"])}', '',
           '## 2. 正式 APK 入口（必须受门禁保护）', '',
           '| 入口 | 游戏目录 | 状态 | 保护依据 | 管线证据 |', '|---|---|---|---|---|']
    for e in finals:
        ev = e['pipeline_evidence'] or ''
        if not ev and e['produced_apk']:
            ev = '同包：%s → %s' % (','.join(e['produced_apk']), ','.join(e['checked_apk']))
        out.append('| `%s` | `%s` | %s | %s | %s |' % (
            e['script'], e['game_dir'] or '—',
            '**已接入**' if e['protected'] else '**未接入**',
            e['protected_by'] or '—', ev or '—'))
    out += ['', '## 3. 组件构建入口（不计未接入）', '', '| 入口 | 游戏目录 |', '|---|---|']
    for e in comps:
        out.append(f"| `{e['script']}` | `{e['game_dir'] or '—'}` |")
    out += ['', '## 4. 探针/诊断入口（非交付，不计未接入）', '',
            f'探针 {len(probes)} 个、诊断 {len(diag)} 个；逐项见 `scan --json`。', '']
    return '\n'.join(out)


def render_islands(data):
    finals, prot, _c, _p, _d = summary(data)
    out = ['未受加固保护的正式 APK 入口（`islands`）：', '']
    for e in finals:
        if not e['protected']:
            extra = f"（{e['pipeline_evidence']}）" if e['pipeline_evidence'] else ''
            out.append(f"- `{e['script']}`（`{e['game_dir'] or '—'}`）{extra}")
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
    ap.add_argument('--game', default='', help='限定游戏名或 tag（两者等价）')
    a = ap.parse_args()

    data = scan(a.root)
    if a.game and not canon_game(data['games'], a.game):
        known = ', '.join('%s/%s' % (n, t) for n, t in data['games']['tags'].items())
        print(f'FAIL 未知游戏名或 tag：{a.game}（已登记：{known}）')
        return 2

    if a.cmd == 'scan':
        print(json.dumps(data, ensure_ascii=False, indent=2) if a.json else render(data))
        if a.write:
            out = os.path.join(data['root'], '10_docs', '防御加固_覆盖登记.md')
            with open(out, 'w', encoding='utf-8') as f:
                f.write(render(data).rstrip('\n') + '\n')
            print(f'[written] {out}', file=sys.stderr)
        return 0
    if a.cmd == 'islands':
        if a.game:
            game = canon_game(data['games'], a.game)
            data = dict(data, entries=[e for e in data['entries'] if belongs(e, game, data['games'])])
        print(render_islands(data))
        return 0

    errors, warns = check(data, strict=a.strict, only_game=(a.game or None))
    for w in warns:
        print(f'WARN {w}')
    for e in errors:
        print(f'FAIL {e}')
    if a.game:
        game = canon_game(data['games'], a.game)
        scoped = dict(data, entries=[e for e in data['entries'] if belongs(e, game, data['games'])]
                      + [e for e in data['entries'] if any(e['script'].endswith(x) for x in
                         ('finalize_game_delivery.sh', 'prune_output_final.sh', 'pre_install_gate.sh'))])
        finals, prot, _c, _p, _d = summary(scoped)
        print(f'加固覆盖审计: scope={a.game}->{game} scope_final_apk={len(finals)} '
              f'scope_protected={len(prot)} scope_unprotected={len(finals)-len(prot)} '
              f'errors={len(errors)} warnings={len(warns)} strict={int(bool(a.strict))}')
    else:
        finals, prot, _c, _p, _d = summary(data)
        print(f'加固覆盖审计: entries={len(data["entries"])} final_apk={len(finals)} '
              f'protected={len(prot)} unprotected={len(finals)-len(prot)} '
              f'errors={len(errors)} warnings={len(warns)} strict={int(bool(a.strict))}')
    return 1 if errors else 0


if __name__ == '__main__':
    sys.exit(main())
