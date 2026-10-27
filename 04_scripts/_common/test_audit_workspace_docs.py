#!/usr/bin/env python3
"""Temporary-fixture tests. Run with python3 -B to avoid bytecode files."""

import hashlib
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest

import audit_workspace_docs as audit


class AuditTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)

    def write(self, name, text):
        path = self.root / name
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(text, encoding='utf-8')
        return path

    def cli(self, *args):
        return subprocess.run(
            [sys.executable, '-B', str(Path(audit.__file__).resolve()), *map(str, args)],
            capture_output=True, text=True, check=False)

    def test_bad_link(self):
        path = self.write('doc.md', '[bad](absent.md#section)\n')
        issues = audit.audit_file(path)
        self.assertEqual([i['kind'] for i in issues], ['missing-link'])
        self.assertEqual(issues[0]['line'], 1)

    def test_fences(self):
        path = self.write('doc.md', '```python\n[ignored](absent.md)\n~~~\n')
        self.assertEqual([i['kind'] for i in audit.audit_file(path)], ['fence'])
        path.write_text('````text\n```\n[ignored](absent.md)\n`````\n~~~\n~~~\n')
        self.assertEqual(audit.audit_file(path), [])

    def test_long_cursor_pointer_and_config(self):
        path = self.write('.cursor/rules/test.mdc',
                          '---\ndescription: test\nalwaysApply: true\nglobs: "*"\n---\n'
                          '# Heading\n\none\ntwo\nthree\nfour\n')
        self.assertEqual([i['kind'] for i in audit.audit_file(path, True)], ['pointer-length'])
        self.assertEqual(audit.audit_file(path, True, 4), [])
        result = self.cli('--root', self.root, 'audit', '--pointer-max-lines', 4, path)
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)

    def test_short_pointer_and_setext_heading(self):
        path = self.write('test.mdc', '---\ndescription: x\n---\nTitle\n=====\none\n\ntwo\nthree\n')
        self.assertEqual(audit.audit_file(path, True), [])

    def test_unclosed_frontmatter(self):
        path = self.write('test.mdc', '---\ndescription: x\n')
        self.assertEqual(audit.audit_file(path, True)[0]['kind'], 'frontmatter')

    def test_success_links_and_skips(self):
        self.write('target (1).md', '# Existing\n')
        path = self.write('doc.md',
                          '[ok](target%20(1).md#not-validated)\n'
                          '[angle](<target (1).md> "Title")\n'
                          '[external](https://example.invalid/no) [anchor](#missing)\n'
                          '[mail](mailto:nobody@example.invalid)\n'
                          '`[code](absent.md)`\n'
                          '[ref]: <target (1).md>\n[reference][ref]\n')
        self.assertEqual(audit.audit_file(path), [])
        result = self.cli('audit', '--root', self.root, path)
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn('anchors NOT validated', result.stdout)

    def test_reference_and_image_missing(self):
        path = self.write('doc.md', '[x]: missing.md\n![image](missing.png)\n')
        self.assertEqual(len(audit.audit_file(path)), 2)

    def test_missing_document(self):
        result = self.cli('audit', '--root', self.root, 'absent.md')
        self.assertEqual(result.returncode, 1)
        self.assertIn('read-error', result.stdout)

    def test_hash_match(self):
        path = self.write('artifact.bin', 'abc')
        expected = hashlib.sha256(b'abc').hexdigest()
        result = self.cli('artifact', path, '--expect-sha256', expected.upper())
        self.assertEqual(result.returncode, 0, result.stderr)
        data = json.loads(result.stdout)
        self.assertEqual(data['path'], str(path.resolve()))
        self.assertEqual(data['size'], 3)
        self.assertEqual(data['sha256'], expected)
        self.assertTrue(data['matches_expected'])
        self.assertEqual(path.read_bytes(), b'abc')

    def test_hash_mismatch(self):
        path = self.write('artifact.bin', 'abc')
        result = self.cli('artifact', path, '--expect-sha256', '0' * 64)
        self.assertEqual(result.returncode, 1)
        self.assertFalse(json.loads(result.stdout)['matches_expected'])

    def test_artifact_missing_and_directory(self):
        for path in (self.root / 'absent.bin', self.root):
            with self.subTest(path=path):
                result = self.cli('artifact', path)
                self.assertEqual(result.returncode, 1)
                self.assertIn('error', json.loads(result.stdout))

    def test_artifact_empty_success(self):
        path = self.write('empty.bin', '')
        result = self.cli('artifact', path)
        self.assertEqual(result.returncode, 0)
        data = json.loads(result.stdout)
        self.assertEqual(data['size'], 0)
        self.assertEqual(data['sha256'], hashlib.sha256(b'').hexdigest())

    def test_invalid_hash(self):
        path = self.write('file.bin', 'abc')
        self.assertEqual(self.cli('artifact', path, '--expect-sha256', 'bad').returncode, 2)

    def state_index(self, text):
        return self.write(audit.STATE_INDEX, text)

    def test_state_index_row_limit_counts_characters_not_bytes(self):
        # 200 Chinese characters = ~640 UTF-8 bytes on one row. The declared
        # limit is 400 *characters*, so byte counting would wrongly reject it.
        row = '| 游戏甲 | 2026-10-15 | ' + '汉' * 200 + ' | [摘要](games/a.md) |\n'
        self.assertGreater(len(row.encode('utf-8')), audit.INDEX_ROW_MAX_CHARS)
        self.assertLessEqual(len(row), audit.INDEX_ROW_MAX_CHARS)
        path = self.state_index('# 索引\n\n| 游戏 | 最近更新 | 状态 | 详情 |\n'
                                '|---|---|---|---|\n' + row)
        self.assertEqual(audit.audit_state_index(path), [])

    def test_state_index_flags_overlong_row_and_header_is_ignored(self):
        row = '| 游戏甲 | 2026-10-15 | ' + '汉' * 400 + ' | [x](y.md) |\n'
        path = self.state_index('# 索引\n\n| 游戏 | 最近更新 | 状态 | 详情 |\n'
                                '|---|---|---|---|\n' + row)
        issues = audit.audit_state_index(path)
        self.assertEqual([i['kind'] for i in issues], ['index-row-length'])
        self.assertEqual(issues[0]['line'], 5)
        # Widening the per-row cap clears it without touching the header rows.
        self.assertEqual(audit.audit_state_index(path, 4000), [])

    def test_state_index_flags_total_size(self):
        path = self.state_index('# 索引\n\n| 游戏 | 最近更新 | 状态 | 详情 |\n'
                                '|---|---|---|---|\n| 甲 | 2026-10-15 | 已收尾 | [x](y.md) |\n')
        self.assertEqual(audit.audit_state_index(path, 400, 10)[0]['kind'], 'index-size')

    def test_state_index_scope_is_the_declared_path_only(self):
        body = ('| 游戏 | 最近更新 | 状态 | 详情 |\n|---|---|---|---|\n'
                '| 甲 | 2026-10-15 | ' + '汉' * 400 + ' | [x](y.md) |\n')
        declared = self.state_index(body)
        lookalike = self.write('10_docs/_state/games/other.md', body)
        self.write('10_docs/_state/games/y.md', '# link target\n')
        self.assertTrue(audit.is_state_index(self.root, declared))
        self.assertFalse(audit.is_state_index(self.root, lookalike))
        # The same over-long table outside the declared path is not index-checked.
        result = self.cli('--root', self.root, 'audit', lookalike)
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)

    def test_state_index_cli_end_to_end(self):
        self.state_index('# 索引\n\n| 游戏 | 最近更新 | 状态 | 详情 |\n|---|---|---|---|\n'
                         '| 甲 | 2026-10-15 | ' + '汉' * 400 + ' | [x](y.md) |\n')
        result = self.cli('--root', self.root)
        self.assertEqual(result.returncode, 1)
        self.assertIn('index-row-length', result.stdout)
        self.assertIn('State index', result.stdout)
        result = self.cli('--root', self.root, 'audit',
                          '--state-index-max-chars', 4000)
        self.assertNotIn('index-row-length', result.stdout)

    def test_default_scope(self):
        for name in audit.DEFAULT_FILES:
            self.write(name, '# Valid\n')
        for name in audit.DEFAULT_DIRS:
            self.write(name + '/test.md', '# Valid\n')
        result = self.cli('--root', self.root)
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        (self.root / 'AGENTS.md').unlink()
        self.assertEqual(self.cli('--root', self.root).returncode, 1)


HEADER_AB = ('| # | 类别 | 强度 | 原话/来源（逐字，含出处） | 可执行含义 | 适用范围与生效条件 | '
             '例外/解除 | 状态 | 证据/权威落点 |\n|---|---|---|---|---|---|---|---|---|\n')


def constraint_file(ab_rows=None, ab_table=None, d_rows='（无）', e_rows='（无）', f_rows='（无）',
                    counts='A=1 / B=0 / D=0 / E=0 / F=0', meta=True, include_c=True):
    row = ('| 1 | A | 必须 | 「保持 1.0.3」(m00003) | 只维护 1.0.3 线 | 该版本 | 无 | 生效 | 变更记录 |\n'
           if ab_rows is None else ab_rows)
    parts = ['# 演示游戏 硬约束', '']
    if meta:
        parts += ['## 0. 元信息', '', f'- 行数：{counts}', '']
    parts += ['## A/B. 当前生效约束与一次性要求（开工必读）', '',
              HEADER_AB + row if ab_table is None else ab_table]
    if include_c:
        parts += ['## C. 默认偏好', '', '（无）', '']
    parts += ['## D. 技术事实与已验证经验', '', d_rows, '',
              '## E. 待核验（默认不生效）', '', e_rows, '',
              '## F. 历史与失效（保留原文，不默认生效）', '', f_rows, '']
    return '\n'.join(parts)


class ConstraintAuditTests(unittest.TestCase):
    """10_docs/*/硬约束.md 结构审计：全部用临时文件，不碰真实游戏文档。"""

    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.path = self.root / '10_docs' / '演示游戏' / '硬约束.md'
        self.path.parent.mkdir(parents=True)

    def write(self, text):
        self.path.write_text(text, encoding='utf-8')
        return self.path

    def cli(self, *args):
        return subprocess.run(
            [sys.executable, '-B', str(Path(audit.__file__).resolve()), *map(str, args)],
            capture_output=True, text=True, check=False)

    def kinds(self, strict=False):
        _paths, errors, warnings = audit.audit_constraint_files(self.root, strict=strict)
        return [i['kind'] for i in errors], [i['kind'] for i in warnings]

    def test_valid_file_passes_clean_and_strict(self):
        self.write(constraint_file())
        self.assertEqual(self.kinds(), ([], []))
        self.assertEqual(self.kinds(strict=True), ([], []))

    def test_missing_c_row_lists_every_required_section(self):
        self.write(constraint_file(include_c=False))
        errors, _warnings = self.kinds()
        self.assertIn('missing-section', errors)

    def test_missing_meta_section_is_error(self):
        self.write(constraint_file(meta=False))
        errors, _warnings = self.kinds()
        self.assertIn('missing-section', errors)

    def test_missing_required_column_is_error(self):
        broken = ('| # | 类别 | 强度 | 原话/来源（逐字，含出处） | 可执行含义 | 状态 |\n|---|---|---|---|---|---|\n'
                  '| 1 | A | 必须 | 「x」(m00001) | y | 生效 |\n')
        self.write(constraint_file(ab_table=broken))
        errors, _warnings = self.kinds()
        self.assertIn('missing-column', errors)

    def test_completed_row_left_in_ab_is_conflict(self):
        row = ('| 2 | A | 必须 | 「先过环境」(m00001) | 阶段闸门 | 该轮 | 无 | 已完成 | m00001 |\n')
        self.write(constraint_file(ab_rows=row, counts='A=1 / B=0 / D=0 / E=0 / F=0'))
        errors, _warnings = self.kinds()
        self.assertIn('status-class-conflict', errors)

    def test_invalid_status_word_is_reported(self):
        row = ('| 1 | A | 必须 | 「重新出一版」(m00007) | 出包 | 该轮 | 无 | 进行中 | m00007 |\n')
        self.write(constraint_file(ab_rows=row))
        errors, _warnings = self.kinds()
        self.assertIn('bad-status', errors)

    def test_history_row_marked_effective_is_error(self):
        f_rows = ('| # | 原话/来源 | 曾经的执行含义 | 状态与失效依据 | 日期 |\n|---|---|---|---|---|\n'
                  '| 5 | 「x」 | y | 生效 | 2026-10-01 |\n')
        self.write(constraint_file(f_rows=f_rows, counts='A=1 / B=0 / D=0 / E=0 / F=1'))
        errors, _warnings = self.kinds()
        self.assertIn('history-row-effective', errors)

    def test_history_row_completed_passes(self):
        f_rows = ('| # | 原话/来源 | 曾经的执行含义 | 状态与失效依据 | 日期 |\n|---|---|---|---|---|\n'
                  '| 5 | 「x」 | y | 已完成（该轮交付闭环） | 2026-10-01 |\n')
        self.write(constraint_file(f_rows=f_rows, counts='A=1 / B=0 / D=0 / E=0 / F=1'))
        self.assertEqual(self.kinds(strict=True), ([], []))

    def test_duplicate_row_id_is_error(self):
        rows = ('| 1 | A | 必须 | 「a」(m00001) | x | 范围 | 无 | 生效 | m00001 |\n'
                '| 1 | A | 必须 | 「b」(m00002) | y | 范围 | 无 | 生效 | m00002 |\n')
        self.write(constraint_file(ab_rows=rows, counts='A=2 / B=0 / D=0 / E=0 / F=0'))
        errors, _warnings = self.kinds()
        self.assertIn('duplicate-id', errors)

    def test_missing_required_field_is_error(self):
        row = '| 1 | A | 必须 |  | x | 范围 | 无 | 生效 | m00001 |\n'
        self.write(constraint_file(ab_rows=row))
        errors, _warnings = self.kinds()
        self.assertIn('missing-field', errors)

    def test_orphan_rows_without_header_are_error(self):
        text = constraint_file() + '\n## 落地检查\n\n| 9 | 「孤立行」 | 必须 | 说明 | 证据 | 生效 |\n'
        self.write(text)
        errors, _warnings = self.kinds()
        self.assertIn('orphan-rows', errors)

    def test_legacy_columns_warn_then_fail_under_strict(self):
        legacy = ('| # | 原话（逐字） | 类型 | 可执行化 | 违反一次的实例 | 状态 |\n|---|---|---|---|---|---|\n'
                  '| 1 | 「x」 | 必须 | y | — | 生效 |\n')
        text = ('## 0. 元信息\n\n- 行数：A=1 / B=0 / D=0 / E=0 / F=0\n\n'
                '## A/B. 当前生效约束与一次性要求（开工必读）\n\n' + legacy +
                '\n## C. 默认偏好\n\n（无）\n\n## D. 技术事实与已验证经验\n\n（无）\n\n'
                '## E. 待核验（默认不生效）\n\n（无）\n\n## F. 历史与失效（保留原文，不默认生效）\n\n（无）\n')
        self.write(text)
        errors, warnings = self.kinds()
        self.assertEqual(errors, [])
        self.assertIn('legacy-format', warnings)
        self.assertIn('legacy-file', warnings)
        strict_errors, strict_warnings = self.kinds(strict=True)
        self.assertIn('legacy-format', strict_errors)
        self.assertEqual(strict_warnings, [])

    def test_count_drift_is_warning(self):
        self.write(constraint_file(counts='A=0 / B=0 / D=0 / E=0 / F=0'))
        errors, warnings = self.kinds()
        self.assertIn('count-drift', warnings)
        self.assertNotIn('count-drift', errors)

    def test_escaped_pipe_does_not_shift_columns(self):
        row = ('| 1 | A | 必须 | 「logcat 出现 `signal 6\\|FATAL`」(m00001) | x | 范围 | 无 | 生效 | m00001 |\n')
        self.write(constraint_file(ab_rows=row))
        self.assertEqual(self.kinds(strict=True), ([], []))

    def test_empty_history_section_needs_no_table(self):
        self.write(constraint_file(f_rows='（无）'))
        self.assertEqual(self.kinds(strict=True), ([], []))

    def test_cli_exit_codes(self):
        self.write(constraint_file(include_c=False))
        result = self.cli('constraints', '--root', self.root)
        self.assertEqual(result.returncode, 1, result.stdout)
        self.assertIn('missing-section', result.stdout)
        self.write(constraint_file())
        ok = self.cli('constraints', '--root', self.root, '--strict')
        self.assertEqual(ok.returncode, 0, ok.stdout + ok.stderr)
        self.assertIn('files=1', ok.stdout)

    def test_template_file_is_not_audited(self):
        template = self.root / '10_docs' / '_templates' / '硬约束.md'
        template.parent.mkdir(parents=True)
        template.write_text('# 模板（故意不完整）\n', encoding='utf-8')
        self.write(constraint_file())
        paths, errors, _warnings = audit.audit_constraint_files(self.root)
        self.assertEqual(len(paths), 1)
        self.assertEqual(errors, [])


if __name__ == '__main__':
    unittest.main()
