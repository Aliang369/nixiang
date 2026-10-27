#!/usr/bin/env python3
"""Read-only, standard-library documentation and file-byte audit.

Links: inline Markdown links/images and reference definitions, outside fenced
and inline code. Bare paths in backticks are not links. This is a lightweight
Markdown subset, not a CommonMark validator; anchors are never validated.
"""

import argparse
import hashlib
import json
from pathlib import Path
import re
import stat
import sys
from urllib.parse import unquote, urlsplit


DEFAULT_FILES = (
    'AGENTS.md', 'skill/SKILL.md',
    'skill/re-implement/hook-rules.md',
    'skill/re-implement/code-review-checklist.md',
    'skill/re-analyze/SKILL.md', 'skill/re-implement/SKILL.md',
    'skill/ida-plugin-flow/SKILL.md',
    'PLAYBOOK.md', 'WORKFLOW.md', 'TOOLBOOK.md',
    '10_docs/_state/current.md',
)
DEFAULT_DIRS = ('.cursor', '10_docs/_state/games', '10_docs/_templates')
# Byte/character caps the cross-game index declares about itself (AGENTS.md
# section "状态索引"): one row per game, <=400 Unicode characters per row.
STATE_INDEX = '10_docs/_state/current.md'
INDEX_ROW_MAX_CHARS = 400
INDEX_MAX_BYTES = 15000
TABLE_ROW = re.compile(r'^\s*\|')
FENCE = re.compile(r'^ {0,3}(`{3,}|~{3,})(.*)$')
INLINE_CODE = re.compile(r'(`+)(?!`)(.+?)(?<!`)\1(?!`)', re.DOTALL)
LINK_START = re.compile(r'(?<!!)\[[^\]\n]*\]\(|!\[[^\]\n]*\]\(')
REFERENCE = re.compile(r'^ {0,3}\[[^\]]+\]:\s*(.*)$')


def destination(text):
    """Read an angle-wrapped or balanced-parenthesis Markdown destination."""
    text = text.lstrip()
    if text.startswith('<'):
        end = text.find('>')
        return text[1:end] if end >= 0 else None
    depth = 0
    result = []
    i = 0
    while i < len(text):
        char = text[i]
        if char == '\\' and i + 1 < len(text):
            result.append(text[i + 1])
            i += 2
            continue
        if char.isspace() or (char == ')' and depth == 0):
            break
        if char == '(':
            depth += 1
        elif char == ')':
            depth -= 1
        result.append(char)
        i += 1
    return ''.join(result)


def pointer_body(lines):
    if lines and lines[0].strip() == '---':
        for index in range(1, len(lines)):
            if lines[index].strip() in ('---', '...'):
                return lines[index + 1:], index + 1, True
        return [], 0, False
    return lines, 0, True


def audit_file(path, pointer=False, pointer_max_lines=3):
    """Return line-oriented diagnostics without writing to disk."""
    issues = []

    def issue(line, kind, message):
        issues.append({'path': str(path.resolve()), 'line': line,
                       'kind': kind, 'message': message})

    try:
        lines = path.read_text(encoding='utf-8-sig').splitlines()
    except (OSError, UnicodeError) as exc:
        issue(0, 'read-error', str(exc))
        return issues
    if pointer:
        body, offset, closed = pointer_body(lines)
        if not closed:
            issue(1, 'frontmatter', 'Unclosed Cursor frontmatter')
        counted = [(offset + i + 1, line) for i, line in enumerate(body)
                   if line.strip() and not re.match(r'^\s{0,3}#{1,6}(?:\s|$)', line)
                   and not (i + 1 < len(body) and
                            re.fullmatch(r' {0,3}(?:=+|-+)\s*', body[i + 1]))
                   and not (i > 0 and body[i - 1].strip() and
                            re.fullmatch(r' {0,3}(?:=+|-+)\s*', line))]
        if closed and len(counted) > pointer_max_lines:
            issue(counted[pointer_max_lines][0], 'pointer-length',
                  f'{len(counted)} body lines exceed limit {pointer_max_lines}')
    else:
        body, offset = lines, 0
    opened = None
    for number, line in enumerate(body, offset + 1):
        match = FENCE.match(line)
        if opened:
            if (match and match[1][0] == opened[0] and
                    len(match[1]) >= opened[1] and not match[2].strip()):
                opened = None
            continue
        if match and not (match[1][0] == '`' and '`' in match[2]):
            opened = (match[1][0], len(match[1]), number)
            continue
        line = INLINE_CODE.sub('', line)
        candidates = [line[m.end():] for m in LINK_START.finditer(line)]
        reference = REFERENCE.match(line)
        if reference:
            candidates.append(reference[1])
        for candidate in candidates:
            target = destination(candidate)
            if not target or target.startswith(('#', '/', '\\')):
                continue
            try:
                parts = urlsplit(target)
                if parts.scheme or parts.netloc or not parts.path:
                    continue
                resolved = path.parent / unquote(parts.path)
                if not resolved.is_file():
                    issue(number, 'missing-link', f'{target} -> {resolved.resolve()}')
            except (ValueError, OSError) as exc:
                issue(number, 'invalid-link', f'{target}: {exc}')
    if opened:
        issue(opened[2], 'fence', 'Unclosed code fence')
    return issues


def is_state_index(root, path):
    """True only for the declared cross-game index, wherever it was reached from.

    Both sides are resolved because callers may pass a symlinked root (macOS
    /var -> /private/var) and relative_to would then silently never match.
    """
    try:
        return Path(path).resolve().relative_to(Path(root).resolve()) == Path(STATE_INDEX)
    except (ValueError, OSError):
        return False


def audit_state_index(path, max_chars=INDEX_ROW_MAX_CHARS, max_bytes=INDEX_MAX_BYTES):
    """Return index-shape diagnostics: per-row character cap and total byte cap.

    Row length is counted in Unicode characters, not UTF-8 bytes: the declared
    limit is written for human-readable one-line summaries, and counting bytes
    would make it far stricter for Chinese text than the rule states.
    """
    issues = []

    def issue(line, kind, message):
        issues.append({'path': str(path.resolve()), 'line': line,
                       'kind': kind, 'message': message})

    try:
        data = path.read_bytes()
        lines = data.decode('utf-8-sig').splitlines()
    except (OSError, UnicodeError) as exc:
        issue(0, 'read-error', str(exc))
        return issues
    if len(data) >= max_bytes:
        issue(1, 'index-size',
              f'{len(data)} bytes reaches or exceeds limit {max_bytes}')
    for number, line in enumerate(lines, 1):
        if not TABLE_ROW.match(line):
            continue
        cells = line.split('|')
        if len(cells) < 4:
            continue
        name = cells[1].strip()
        # Skip the header row and the |---|---| separator line.
        if not name or name == '游戏' or not set(name) - set('-: '):
            continue
        if len(line) > max_chars:
            issue(number, 'index-row-length',
                  f'{len(line)} characters exceed limit {max_chars} ({name})')
    return issues


def default_paths(root):
    paths = [root / name for name in DEFAULT_FILES]
    for name in DEFAULT_DIRS:
        directory = root / name
        found = sorted(p for p in directory.rglob('*')
                       if p.suffix.lower() in ('.md', '.mdc') and p.is_file())
        # Do not silently succeed when an expected document group is absent.
        paths.extend(found or [directory])
    return list(dict.fromkeys(paths))


def artifact(path, expected=None):
    """Hash a regular file in bounded memory; reject observed concurrent changes."""
    path = path.expanduser().resolve()
    result = {'path': str(path)}
    try:
        if not path.is_file():
            raise OSError('Not an existing regular file')
        digest = hashlib.sha256()
        size = 0
        with path.open('rb') as stream:
            import os
            before = os.fstat(stream.fileno())
            if not stat.S_ISREG(before.st_mode):
                raise OSError('Not a regular file')
            for block in iter(lambda: stream.read(1024 * 1024), b''):
                size += len(block)
                digest.update(block)
            after = os.fstat(stream.fileno())
        if (before.st_size, before.st_mtime_ns, before.st_ctime_ns) != (
                after.st_size, after.st_mtime_ns, after.st_ctime_ns) or size != after.st_size:
            raise OSError('File changed while hashing')
        result.update(size=size, sha256=digest.hexdigest())
        if expected is not None:
            result.update(expect_sha256=expected.lower(),
                          matches_expected=digest.hexdigest() == expected.lower())
        return result, 0 if expected is None or result['matches_expected'] else 1
    except (OSError, ValueError) as exc:
        result['error'] = str(exc)
        return result, 1


def nonnegative(value):
    number = int(value)
    if number < 0:
        raise argparse.ArgumentTypeError('Must be nonnegative')
    return number


def sha256_value(value):
    if not re.fullmatch(r'[a-fA-F0-9]{64}', value):
        raise argparse.ArgumentTypeError('Expected 64 hexadecimal SHA-256 characters')
    return value.lower()


CONSTRAINT_FILES = '10_docs/*/硬约束.md'
AB_COLS = ('类别', '强度', '原话', '可执行含义', '适用范围', '例外', '状态', '证据')
ZONE_COLS = {
    'D': ('内容', '适用条件', '证据'),
    'E': ('内容（原文）', '不确定点', '已做的保守处置'),
    'F': ('原话/来源', '曾经的执行含义', '状态与失效依据', '日期'),
}
CONSTRAINT_CLASSES = ('A', 'B', 'C', 'D')
STATUS_BASES = ('生效', '一次性', '待核验', '已完成', '已失效', '已被', '已作废')
ACTIVE_BASES = ('生效', '一次性')
REQUIRED_SECTIONS = (('元信息', '元信息'), ('A/B', 'A/B 当前生效'), ('C. 默认偏好', 'C 默认偏好'),
                     ('D. 技术事实', 'D 技术事实'), ('E. 待核验', 'E 待核验'), ('F. 历史与失效', 'F 历史与失效'))
SEPARATOR = re.compile(r'\s*\|[-:|\s]+\|?\s*')
COUNT_LINE = re.compile(r'行数：\s*A=(\d+)\s*/\s*B=(\d+)\s*/\s*D=(\d+)\s*/\s*E=(\d+)\s*/\s*F=(\d+)')


def split_cells(line):
    """按未转义的 `|` 切格；`\\|` 视为单元格内的字面竖线。"""
    body = line.strip()
    if body.startswith('|'):
        body = body[1:]
    if body.endswith('|'):
        body = body[:-1]
    return [c.strip().replace('\\|', '|') for c in re.split(r'(?<!\\)\|', body)]


def scan_tables(lines):
    """返回 [(line_no, heading, header, [(line_no, cells)])]；同时标出无表头的数据块。"""
    tables, orphans = [], []
    heading = ''
    i = 0
    while i < len(lines):
        line = lines[i]
        if line.startswith('#'):
            heading = line.lstrip('# ').strip()
        if not line.lstrip().startswith('|'):
            i += 1
            continue
        start = i
        while i < len(lines) and lines[i].lstrip().startswith('|'):
            i += 1
        block = lines[start:i]
        has_header = len(block) > 1 and SEPARATOR.fullmatch(block[1])
        if not has_header:
            orphans.append((start + 1, block[0]))
            continue
        header = split_cells(block[0])
        rows = [(start + 2 + k, split_cells(row)) for k, row in enumerate(block[2:])]
        tables.append((start + 1, heading, header, rows))
    return tables, orphans


def zone_of(heading):
    if 'A/B' in heading or ('A 区' in heading and 'B 区' in heading):
        return 'AB'
    if re.match(r'^[AB][.、： ]', heading):
        return 'AB'
    for key in ('D. 技术事实', 'E. 待核验', 'F. 历史与失效'):
        if key in heading:
            return key[0]
    return ''


def base_status(value):
    return re.split(r'[（(]', (value or '').strip())[0].strip()


def audit_constraints(path):
    """结构审计：分区齐全性、列齐全性、逐行必填、状态词表与 A/B 一致性、重复行号。

    旧格式（无 类别 列）只警告，--strict 才升级为错误；结构断裂（缺表头）始终是错误。
    """
    errors, warnings = [], []

    def issue(bucket, line, kind, message):
        bucket.append({'path': str(path.resolve()), 'line': line, 'kind': kind, 'message': message})

    try:
        text = path.read_text(encoding='utf-8-sig')
    except (OSError, UnicodeError) as exc:
        issue(errors, 0, 'read-error', str(exc))
        return errors, warnings
    lines = text.splitlines()
    tables, orphans = scan_tables(lines)
    for line_no, head in orphans:
        issue(errors, line_no, 'orphan-rows', '表外数据行（该表缺表头或分隔线）：' + head[:60])
    if not tables:
        issue(errors, 1, 'missing-table', '没有找到任何有效表格（见模板 §5 骨架）')
        return errors, warnings

    headings = [line.lstrip('# ').strip() for line in lines if line.startswith('#')]
    for key, label in REQUIRED_SECTIONS:
        if key == 'A/B':
            ok = any('A/B' in h or re.match(r'^A[.、： ]', h) for h in headings)
        elif key == '元信息':
            ok = any('元信息' in h for h in headings)
        else:
            ok = any(h.startswith(key) for h in headings)
        if not ok:
            issue(errors, 1, 'missing-section', f'缺少分区「{label}」')

    counted = {'A': 0, 'B': 0, 'D': 0, 'E': 0, 'F': 0}
    seen_ab = False
    seen_ids = {}

    for line_no, heading, header, rows in tables:
        zone = zone_of(heading)
        joined = ' '.join(header)
        if zone == 'AB':
            missing = [c for c in AB_COLS if c not in joined]
            if missing:
                if '类别' in missing:
                    issue(warnings, line_no, 'legacy-format',
                          '旧格式 A/B 表（无 类别 列），按模板 §7 迁移：' + ','.join(missing))
                    for row_no, cells in rows:
                        if any(cells):
                            counted['A'] += 1
                    continue
                issue(errors, line_no, 'missing-column', 'A/B 表头缺列: ' + ','.join(missing))
            seen_ab = True

            def col(name, cells):
                for idx, cell in enumerate(header):
                    if name in cell:
                        return cells[idx] if idx < len(cells) else ''
                return ''

            for row_no, cells in rows:
                if not any(cells):
                    continue
                rid = cells[0].strip()
                if rid:
                    if rid in seen_ids:
                        issue(errors, row_no, 'duplicate-id',
                              f'行号重复: {rid}（首次出现在第 {seen_ids[rid]} 行）')
                    else:
                        seen_ids[rid] = row_no
                cls = col('类别', cells).strip().upper()[:1]
                if cls not in CONSTRAINT_CLASSES:
                    issue(errors, row_no, 'bad-class', f'类别必须是 A/B/C/D，实际: {cls or "<空>"}')
                    continue
                counted[cls if cls in counted else 'A'] += 1
                for name in ('强度', '原话', '可执行含义', '适用范围', '例外', '状态'):
                    if not col(name, cells).strip():
                        issue(errors, row_no, 'missing-field', f'{rid or "?"} 缺字段: {name}')
                status = base_status(col('状态', cells))
                if status and not any(status.startswith(b) for b in STATUS_BASES):
                    issue(errors, row_no, 'bad-status',
                          f'{rid or "?"} 状态值非法: {status}（允许 生效/一次性/待核验/已完成/已失效/已被 #N 替代）')
                elif status and cls in ('A', 'B') and not any(status.startswith(b) for b in ACTIVE_BASES):
                    issue(errors, row_no, 'status-class-conflict',
                          f'{rid or "?"} 类别 {cls} 却标「{status}」：已完成/已失效的行应移入 F 区')
            continue

        if zone in ZONE_COLS:
            need = ZONE_COLS[zone]
            missing = [c for c in need if c not in joined]
            if missing:
                issue(errors, line_no, 'missing-column', f'{zone} 区表头缺列: ' + ','.join(missing))
                continue

            def zcol(name, cells):
                for idx, cell in enumerate(header):
                    if name in cell:
                        return cells[idx] if idx < len(cells) else ''
                return ''

            for row_no, cells in rows:
                if not any(cells):
                    continue
                counted[zone] += 1
                rid = cells[0].strip() or '?'
                if rid != '?':
                    if rid in seen_ids:
                        issue(errors, row_no, 'duplicate-id',
                              f'行号重复: {rid}（首次出现在第 {seen_ids[rid]} 行）')
                    else:
                        seen_ids[rid] = row_no
                for name in need[:3]:
                    if not zcol(name, cells).strip():
                        issue(errors, row_no, 'missing-field', f'{rid} 缺字段: {name}')
                if zone == 'F':
                    state = zcol('状态与失效依据', cells).strip()
                    if state and base_status(state) in ACTIVE_BASES:
                        issue(errors, row_no, 'history-row-effective',
                              f'{rid} 在 F 区却标「{state}」：F 区只能放过期/被替代/已完成的行')
                    if not zcol('日期', cells).strip():
                        issue(warnings, row_no, 'missing-date', f'{rid} 缺日期')

    if not seen_ab:
        issue(warnings, 1, 'legacy-file', '本文件尚无 9 列 A/B 表（未迁移到新结构）')

    declared = COUNT_LINE.search(text)
    if declared:
        want = dict(zip('ABDEF', (int(x) for x in declared.groups())))
        diff = {k: (want[k], counted[k]) for k in want if want[k] != counted[k]}
        if diff:
            issue(warnings, 1, 'count-drift',
                  '元信息行数与实际不一致: ' + ', '.join(f'{k} 声明{want[k]}/实际{counted[k]}' for k in diff))
    return errors, warnings


def audit_constraint_files(root, strict=False):
    errors, warnings = [], []
    paths = sorted(p for p in Path(root).glob(CONSTRAINT_FILES) if p.parent.name != '_templates')
    if not paths:
        errors.append({'path': str(root / CONSTRAINT_FILES), 'line': 0,
                       'kind': 'missing-group', 'message': '未找到任何 10_docs/*/硬约束.md'})
    for path in paths:
        errs, warns = audit_constraints(path)
        errors.extend(errs)
        warnings.extend(warns)
    if strict:
        errors.extend(warnings)
        warnings = []
    return paths, errors, warnings


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest='command')
    audit = commands.add_parser('audit', help='Audit Markdown; default command')
    # Root options also permit the default command without an explicit subcommand.
    for target in (parser, audit):
        target.add_argument('--root', type=Path,
                            default=Path(__file__).resolve().parents[2] if target is parser else argparse.SUPPRESS)
        target.add_argument('--pointer-max-lines', type=nonnegative,
                            default=3 if target is parser else argparse.SUPPRESS,
                            help='Maximum Cursor body lines, excluding frontmatter/headings')
        target.add_argument('--state-index-max-chars', type=nonnegative,
                            default=INDEX_ROW_MAX_CHARS if target is parser else argparse.SUPPRESS,
                            help='Maximum Unicode characters per row of the cross-game index')
        target.add_argument('--state-index-max-bytes', type=nonnegative,
                            default=INDEX_MAX_BYTES if target is parser else argparse.SUPPRESS,
                            help='Maximum total bytes of the cross-game index')
    audit.add_argument('paths', nargs='*', type=Path,
                       help='Override default files; relative paths resolve against root')
    cons = commands.add_parser('constraints',
                               help='Audit 10_docs/*/硬约束.md structure (template 10_docs/_templates/硬约束.md)')
    cons.add_argument('--root', type=Path, default=Path(__file__).resolve().parents[2])
    cons.add_argument('--strict', action='store_true',
                      help='Treat legacy-format warnings as errors (default: progressive)')
    file_parser = commands.add_parser('artifact', help='JSON file bytes only; no APK signature or build identity proof')
    file_parser.add_argument('path', type=Path)
    file_parser.add_argument('--expect-sha256', type=sha256_value)
    args = parser.parse_args(argv)
    if args.command == 'constraints':
        root = args.root.expanduser().resolve()
        paths, errors, warnings = audit_constraint_files(root, strict=args.strict)
        for item in errors + warnings:
            print('{path}:{line}: {kind}: {message}'.format(**item))
        print(f'硬约束结构审计: files={len(paths)} errors={len(errors)} warnings={len(warnings)}'
              '（旧格式只警告，不阻断；--strict 可升级为失败）')
        return 1 if errors or (args.strict and warnings) else 0
    if args.command == 'artifact':
        result, code = artifact(args.path, args.expect_sha256)
        print(json.dumps(result, ensure_ascii=False, sort_keys=True))
        return code
    root = args.root.expanduser().resolve()
    selected = getattr(args, 'paths', [])
    paths = [p if p.is_absolute() else root / p for p in selected] if selected else default_paths(root)
    issues = []
    for path in paths:
        pointer = (root / '.cursor') in path.resolve().parents
        issues.extend(audit_file(path, pointer, args.pointer_max_lines))
        if is_state_index(root, path):
            issues.extend(audit_state_index(path, args.state_index_max_chars,
                                            args.state_index_max_bytes))
    for item in issues:
        print('{path}:{line}: {kind}: {message}'.format(**item))
    print(f'Audited {len(paths)} files; {len(issues)} issue(s). '
          'Relative link files only; anchors NOT validated. '
          f'State index {STATE_INDEX}: rows <= {args.state_index_max_chars} chars, '
          f'file < {args.state_index_max_bytes} bytes. No files modified.')
    return 1 if issues else 0


if __name__ == '__main__':
    sys.exit(main())
