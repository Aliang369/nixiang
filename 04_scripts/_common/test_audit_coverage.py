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
    """第六轮：覆盖审计器重写后的行为测试（分类 / 递归 / 隔离 / 逃生门 / 漂移）。"""

    def setUp(self):
        self.root = tempfile.mkdtemp()
        self.common = os.path.join(self.root, '04_scripts', '_common')
        os.makedirs(self.common, exist_ok=True)
        w(os.path.join(self.common, 'harden_apk.py'),
          'GAMES = {\n    "勇士之心": {\n        "tag": "ysxz",\n    },\n}\n')
        w(os.path.join(self.common, 'pre_install_gate.sh'),
          'python3 harden_apk.py check --apk "$APK"\nstamp="09_test/x/harden/1.stamp"\n')
        w(os.path.join(self.common, 'finalize_game_delivery.sh'), 'echo finalize\n')

    def final_apk_script(self, rel, extra=''):
        # 判定为 final_apk 的必要证据：apksigner/jarsigner + 产物落 08_output
        w(os.path.join(self.root, rel),
          'apksigner sign --out 08_output/x/out.apk in.apk\n' + extra)
        return rel

    # ---------- 分类 ----------
    def test_final_apk_unwired_warns_and_strict_fails(self):
        self.final_apk_script('04_scripts/勇士之心/ysxz_surgical_103.sh')
        data = harden_cov.scan(self.root)
        e = [x for x in data['entries'] if x['script'].endswith('ysxz_surgical_103.sh')][0]
        self.assertEqual(e['kind'], 'final_apk')
        errors, warns = harden_cov.check(data)
        self.assertEqual(errors, [])
        self.assertTrue(any('未接入加固门禁' in w_ for w_ in warns))
        strict_errors, _ = harden_cov.check(data, strict=True)
        self.assertTrue(any('未接入加固门禁' in x for x in strict_errors))

    def test_wired_final_apk_passes(self):
        self.final_apk_script('04_scripts/勇士之心/ysxz_surgical_103.sh',
                              'bash 04_scripts/_common/harden_gate.sh 08_output/x/out.apk\n')
        data = harden_cov.scan(self.root)
        errors, warns = harden_cov.check(data, strict=True)
        self.assertEqual([x for x in errors if '未接入' in x], [])
        # 临时 fixture 的 pre_install_gate 没有理由机制，只允许这一条 warn
        self.assertEqual([w_ for w_ in warns if '未强制要求理由' not in w_], [])

    def test_finalizer_chain_counts_as_protected(self):
        self.final_apk_script('04_scripts/勇士之心/ysxz_surgical_103.sh',
                              'bash 04_scripts/_common/finalize_game_delivery.sh --game 勇士之心\n')
        data = harden_cov.scan(self.root)
        e = [x for x in data['entries'] if x['script'].endswith('ysxz_surgical_103.sh')][0]
        self.assertTrue(e['protected'])

    def test_component_build_not_flagged(self):
        w(os.path.join(self.root, '04_scripts', '勇士之心', 'build_so_only.sh'),
          'ndk-build NDK_PROJECT_PATH=. APP_ABI=arm64-v8a\ncp libs/arm64-v8a/libx.so /tmp/x\n')
        data = harden_cov.scan(self.root)
        e = [x for x in data['entries'] if x['script'].endswith('build_so_only.sh')][0]
        self.assertEqual(e['kind'], 'component')
        errors, warns = harden_cov.check(data)
        self.assertFalse(any('build_so_only.sh' in x for x in errors + warns))

    def test_python_assemble_entry_detected(self):
        w(os.path.join(self.root, '04_scripts', '勇士之心', 'ysxz_assemble_103.py'),
          'import subprocess\nsubprocess.run(["apksigner","sign","--out","08_output/勇士之心/a.apk","b.apk"])\n')
        data = harden_cov.scan(self.root)
        e = [x for x in data['entries'] if x['script'].endswith('ysxz_assemble_103.py')][0]
        self.assertEqual(e['kind'], 'final_apk')

    def test_nested_and_modmenu_scanned(self):
        w(os.path.join(self.root, 'mod菜单', 'starry-mod-menu', 'scripts', 'surgical_abc_1.sh'),
          'apksigner sign --out 08_output/abc/out.apk in.apk\n')
        w(os.path.join(self.root, '04_scripts', '某游戏', 'deep', 'nested_sign.sh'),
          'apksigner sign --out 08_output/某/out.apk in.apk\n')
        data = harden_cov.scan(self.root)
        got = {x['script'] for x in data['entries'] if x['kind'] == 'final_apk'}
        self.assertTrue(any('mod菜单' in g for g in got), got)
        self.assertTrue(any(os.path.join('deep', 'nested_sign.sh') in g for g in got), got)

    # ---------- 注册表与漂移 ----------
    def test_wired_script_for_unregistered_game_fails(self):
        w(os.path.join(self.root, '04_scripts', '陌生游戏', 'abc_surgical_1.sh'),
          'apksigner sign --out 08_output/abc/out.apk in.apk\nbash 04_scripts/_common/harden_gate.sh out.apk\n')
        data = harden_cov.scan(self.root)
        errors, _ = harden_cov.check(data)
        self.assertTrue(any('GAMES 登记' in e for e in errors))

    def test_registry_drift_fails(self):
        self.final_apk_script('04_scripts/勇士之心/ysxz_surgical_103.sh')
        w(os.path.join(self.root, '10_docs', '防御加固_覆盖登记.md'),
          '| `x` | `04_scripts/勇士之心/ysxz_surgical_103.sh` | 已接入 | — |\n')
        data = harden_cov.scan(self.root)
        errors, _ = harden_cov.check(data)
        self.assertTrue(any('登记漂移' in e for e in errors))

    def test_missing_install_gate_fails(self):
        w(os.path.join(self.common, 'pre_install_gate.sh'), 'echo nothing\n')
        data = harden_cov.scan(self.root)
        errors, _ = harden_cov.check(data)
        self.assertTrue(any('查11' in e for e in errors))

    # ---------- 逃生门 ----------
    def test_escape_hatch_without_reason_fails(self):
        self.final_apk_script('04_scripts/勇士之心/ysxz_surgical_103.sh',
                              'bash 04_scripts/_common/harden_gate.sh --allow-no-harden out.apk\n')
        data = harden_cov.scan(self.root)
        errors, _ = harden_cov.check(data)
        self.assertTrue(any('没有理由参数' in e for e in errors))

    def test_escape_hatch_with_reason_passes(self):
        self.final_apk_script('04_scripts/勇士之心/ysxz_surgical_103.sh',
                              'bash 04_scripts/_common/harden_gate.sh --allow-no-harden '
                              '--harden-reason "用户授权临时包" out.apk\n')
        data = harden_cov.scan(self.root)
        errors, _ = harden_cov.check(data)
        self.assertFalse(any('没有理由参数' in e for e in errors))

    def test_stage_controller_env_not_flagged(self):
        w(os.path.join(self.root, '04_scripts', 'pipeline.sh'),
          'STAGE_NEXT_FORCE=1 bash 04_scripts/_common/stage_next.sh "$@"\n')
        data = harden_cov.scan(self.root)
        errors, _ = harden_cov.check(data)
        self.assertFalse(any('pipeline.sh' in e for e in errors))

    # ---------- --root 隔离 ----------
    def test_root_isolation_does_not_read_real_registry(self):
        data = harden_cov.scan(self.root)
        self.assertEqual(os.path.abspath(data['root']), os.path.abspath(self.root))
        reg_real = os.path.join(harden_cov.os.path.dirname(harden_cov.os.path.abspath(__file__)),
                                '..', '..', '10_docs', '防御加固_覆盖登记.md')
        self.assertTrue(os.path.isfile(reg_real))  # 真实仓库的登记表存在，但不应被读进临时 root 的判据
        errors, _ = harden_cov.check(data)
        self.assertFalse(any('真实' in e for e in errors))


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
