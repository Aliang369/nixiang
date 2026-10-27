#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""覆盖类审计的正/负测试（2026-10-27 第五轮）。全部使用临时目录，不碰真实游戏产物。"""
import os
import sys
import tempfile
import unittest

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)

import audit_game_docs_coverage as docs_cov   # noqa: E402
import harden_coverage as harden_cov          # noqa: E402


def w(path, text='x\n'):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, 'w', encoding='utf-8') as f:
        f.write(text)


class HardenCoverageTests(unittest.TestCase):
    def setUp(self):
        self.root = tempfile.mkdtemp()
        self.common = os.path.join(self.root, '04_scripts', '_common')
        os.makedirs(self.common, exist_ok=True)
        w(os.path.join(self.common, 'harden_apk.py'),
          'GAMES = {\n    "勇士之心": {\n        "tag": "ysxz",\n    },\n}\n')
        w(os.path.join(self.common, 'pre_install_gate.sh'),
          'python3 harden_apk.py check --apk "$APK"\nstamp="09_test/x/harden/1.stamp"\n')
        harden_cov._ROOT = self.root

    def test_wired_script_passes(self):
        w(os.path.join(self.root, '04_scripts', '勇士之心', 'ysxz_surgical_103.sh'),
          'bash 04_scripts/_common/harden_gate.sh out.apk\n')
        data = harden_cov.scan(self.root)
        errors, _ = harden_cov.check(data)
        self.assertEqual([e for e in errors if 'GAMES 登记' in e], [])

    def test_wired_script_for_unregistered_game_fails(self):
        w(os.path.join(self.root, '04_scripts', '陌生游戏', 'abc_surgical_1.sh'),
          'bash 04_scripts/_common/harden_gate.sh out.apk\n')
        data = harden_cov.scan(self.root)
        errors, _ = harden_cov.check(data)
        self.assertTrue(any('GAMES 登记' in e for e in errors))

    def test_escape_hatch_without_exemption_fails(self):
        w(os.path.join(self.root, '04_scripts', '勇士之心', 'ysxz_surgical_103.sh'),
          'bash 04_scripts/_common/harden_gate.sh --allow-no-harden out.apk\n')
        data = harden_cov.scan(self.root)
        errors, _ = harden_cov.check(data)
        self.assertTrue(any('逃生门' in e for e in errors))

    def test_escape_hatch_with_exemption_passes(self):
        script = os.path.join(self.root, '04_scripts', '勇士之心', 'ysxz_surgical_103.sh')
        w(script, 'bash harden_gate.sh --allow-no-harden out.apk\n')
        harden_cov.EXEMPTIONS['04_scripts/勇士之心/ysxz_surgical_103.sh'] = '测试用显式豁免'
        try:
            data = harden_cov.scan(self.root)
            errors, _ = harden_cov.check(data)
            self.assertFalse(any('逃生门' in e for e in errors))
        finally:
            harden_cov.EXEMPTIONS.pop('04_scripts/勇士之心/ysxz_surgical_103.sh', None)

    def test_unwired_scripts_warn_and_strict_fails(self):
        w(os.path.join(self.root, '04_scripts', '勇士之心', 'ysxz_surgical_103.sh'), 'echo build\n')
        data = harden_cov.scan(self.root)
        errors, warns = harden_cov.check(data)
        self.assertEqual(errors, [])
        self.assertTrue(any('未接入加固门禁' in x for x in warns))
        errors_strict, _ = harden_cov.check(data, strict=True)
        self.assertTrue(any('未接入加固门禁' in x for x in errors_strict))

    def test_missing_install_gate_fails(self):
        w(os.path.join(self.common, 'pre_install_gate.sh'), 'echo nothing\n')
        data = harden_cov.scan(self.root)
        errors, _ = harden_cov.check(data)
        self.assertTrue(any('查11' in e for e in errors))

    def test_registry_drift_fails(self):
        w(os.path.join(self.root, '04_scripts', '勇士之心', 'ysxz_surgical_103.sh'), 'echo build\n')
        w(os.path.join(self.root, '10_docs', '防御加固_覆盖登记.md'),
          '| `x` | `04_scripts/勇士之心/ysxz_surgical_103.sh` | 已接入 | — |\n')
        data = harden_cov.scan(self.root)
        errors, _ = harden_cov.check(data)
        self.assertTrue(any('登记与实现漂移' in e for e in errors))


class GameDocsCoverageTests(unittest.TestCase):
    def build(self, games, active_head=True):
        root = tempfile.mkdtemp()
        for name, opts in games.items():
            w(os.path.join(root, '10_docs', name, '硬约束.md'), 'x\n')
            if opts.get('no_hard'):
                os.remove(os.path.join(root, '10_docs', name, '硬约束.md'))
            if opts.get('route'):
                w(os.path.join(root, '10_docs', name, '路线锁定.md'), 'x\n')
            if opts.get('summary'):
                head = '> 最近更新：2026-10-27 ｜ 状态：**进行中**\n' if active_head else '> 2026-09-13 收官完成\n'
                w(os.path.join(root, '10_docs', '_state', 'games', name + '.md'), head)
            if opts.get('entry'):
                w(os.path.join(root, '04_scripts', name, 'pipeline.json'), '{"stages":{"harden":{}}}\n')
        return root

    def test_active_game_with_entry_missing_constraints_blocks(self):
        root = self.build({'甲游戏': {'no_hard': True, 'summary': True, 'entry': True}})
        rows = docs_cov.scan(root)
        blockers, _note, _notes = docs_cov.evaluate(rows)
        self.assertTrue(any('硬约束' in b for b in blockers))

    def test_delivered_game_is_not_active(self):
        root = self.build({'乙游戏': {'no_hard': True, 'summary': True, 'entry': True}}, active_head=False)
        rows = docs_cov.scan(root)
        blockers, _note, _notes = docs_cov.evaluate(rows)
        self.assertEqual(blockers, [])

    def test_route_lock_required_only_when_pipeline_says_so(self):
        root = self.build({'丙游戏': {'summary': True, 'entry': True}})
        w(os.path.join(root, '04_scripts', '丙游戏', 'pipeline.json'), '{"stages":{"route_lock":{}}}\n')
        rows = docs_cov.scan(root)
        blockers, _note, _notes = docs_cov.evaluate(rows)
        self.assertTrue(any('路线锁定' in b for b in blockers))

    def test_non_active_missing_docs_are_not_blockers(self):
        root = self.build({'丁游戏': {'no_hard': True}})
        rows = docs_cov.scan(root)
        blockers, note, _notes = docs_cov.evaluate(rows)
        self.assertEqual(blockers, [])
        self.assertIn('不批量创建', note)


if __name__ == '__main__':
    unittest.main()
