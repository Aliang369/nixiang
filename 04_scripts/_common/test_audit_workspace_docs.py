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

    def test_default_scope(self):
        for name in audit.DEFAULT_FILES:
            self.write(name, '# Valid\n')
        for name in audit.DEFAULT_DIRS:
            self.write(name + '/test.md', '# Valid\n')
        result = self.cli('--root', self.root)
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        (self.root / 'AGENTS.md').unlink()
        self.assertEqual(self.cli('--root', self.root).returncode, 1)


if __name__ == '__main__':
    unittest.main()
