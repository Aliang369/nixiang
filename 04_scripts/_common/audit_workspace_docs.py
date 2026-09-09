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
    audit.add_argument('paths', nargs='*', type=Path,
                       help='Override default files; relative paths resolve against root')
    file_parser = commands.add_parser('artifact', help='JSON file bytes only; no APK signature or build identity proof')
    file_parser.add_argument('path', type=Path)
    file_parser.add_argument('--expect-sha256', type=sha256_value)
    args = parser.parse_args(argv)
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
    for item in issues:
        print('{path}:{line}: {kind}: {message}'.format(**item))
    print(f'Audited {len(paths)} files; {len(issues)} issue(s). '
          'Relative link files only; anchors NOT validated. No files modified.')
    return 1 if issues else 0


if __name__ == '__main__':
    sys.exit(main())
