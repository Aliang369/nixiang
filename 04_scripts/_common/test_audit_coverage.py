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


class HardenCoverageRound7Tests(unittest.TestCase):
    """第七轮：探针误判、假保护、mod菜单注册表、限定游戏验收。"""

    def setUp(self):
        self.root = tempfile.mkdtemp()
        self.common = os.path.join(self.root, '04_scripts', '_common')
        os.makedirs(self.common, exist_ok=True)
        w(os.path.join(self.common, 'harden_apk.py'),
          'GAMES = {\n    "勇士之心": {\n        "tag": "ysxz",\n    },\n'
          '    "冒险与征途": {\n        "tag": "mxyzt",\n    },\n}\n')
        w(os.path.join(self.common, 'pre_install_gate.sh'),
          'python3 harden_apk.py check --apk "$APK" --harden-reason hi\nstamp=09_test/x/harden/1.stamp\n')
        w(os.path.join(self.common, 'finalize_game_delivery.sh'),
          'echo gate\nbash "$ROOT/04_scripts/_common/prune_output_final.sh" "$GAME_DIR"\n')

    def entry(self, rel, body):
        w(os.path.join(self.root, rel), body)
        data = harden_cov.scan(self.root)
        return data, [x for x in data['entries'] if x['script'].endswith(os.path.basename(rel))][0]

    # ---- 误分类：探针包不是交付入口 ----
    def test_probe_apk_not_final(self):
        _, e = self.entry('04_scripts/勇士之心/ysxz_probe_metainf.sh',
                          '# 探针包，不作交付\napksigner sign --out 08_output/勇士之心/x-probe-signed.apk in.apk\n')
        self.assertEqual(e['kind'], 'probe')
        self.assertFalse(e['protected'])

    def test_probe_by_filename(self):
        _, e = self.entry('04_scripts/绝区零/fr_probe.py',
                          'subprocess.run(["apksigner","sign","--out","08_output/zzz/a.apk","b.apk"])\n')
        self.assertEqual(e['kind'], 'probe')

    # ---- 假保护：注释提到门禁 ≠ 调用门禁 ----
    def test_comment_mention_is_not_gate_call(self):
        _, e = self.entry('04_scripts/勇士之心/ysxz_assemble_103.py',
                          '# 参考 harden_gate.sh 的用法，这里没调用\n'
                          'subprocess.run(["apksigner","sign","--out","08_output/勇士之心/a.apk","b.apk"])\n')
        self.assertEqual(e['kind'], 'final_apk')
        self.assertFalse(e['gate_call'])

    def test_pipeline_harden_alone_is_not_protection(self):
        # 同游戏 pipeline 里有 harden 阶段，但该脚本没被任何阶段调用 ⇒ 不算保护
        w(os.path.join(self.root, '04_scripts/勇士之心/pipeline.json'),
          '{"stages":[{"id":"sign","command":"bash 04_scripts/勇士之心/other.sh"},'
          '{"id":"harden","command":"python3.11 04_scripts/_common/harden_apk.py redteam"}]}')
        _, e = self.entry('04_scripts/勇士之心/ysxz_release_103.sh',
                          'apksigner sign --out 08_output/勇士之心/rel.apk in.apk\n')
        self.assertFalse(e['protected'])

    def test_pipeline_stage_before_harden_is_protection(self):
        w(os.path.join(self.root, '04_scripts/勇士之心/pipeline.json'),
          '{"stages":[{"id":"sign","command":"bash 04_scripts/勇士之心/ysxz_sign.sh"},'
          '{"id":"harden","command":"python3.11 04_scripts/_common/harden_apk.py redteam"}]}')
        data, e = self.entry('04_scripts/勇士之心/ysxz_sign.sh',
                             'apksigner sign --out 08_output/勇士之心/s.apk in.apk\n')
        self.assertTrue(e['protected'])
        self.assertEqual(e['pipeline_harden_stage'], 'harden')

    def test_called_by_finalizer_is_protection(self):
        data, e = self.entry('04_scripts/_common/prune_output_final.sh',
                             'apksigner verify 08_output/x/a.apk\necho prune\n')
        self.assertTrue(e['protected'])

    # ---- mod菜单 注册表校验 ----
    def test_modmenu_unknown_game_fails(self):
        data, e = self.entry('mod菜单/starry-mod-menu/scripts/surgical_abc_1.sh',
                             'apksigner sign --out 08_output/abc/o.apk in.apk\n'
                             'bash 04_scripts/_common/harden_gate.sh o.apk --game 不存在的游戏\n')
        errors, _ = harden_cov.check(data)
        self.assertTrue(any('既不是 GAMES 游戏名' in x for x in errors), errors)

    def test_modmenu_known_tag_passes(self):
        data, e = self.entry('mod菜单/starry-mod-menu/scripts/build_x.sh',
                             'apksigner sign --out 08_output/x/o.apk in.apk\n'
                             'bash 04_scripts/_common/harden_gate.sh o.apk --game ysxz\n')
        errors, _ = harden_cov.check(data)
        self.assertFalse([x for x in errors if 'GAMES' in x])

    def test_modmenu_known_name_passes(self):
        data, e = self.entry('mod菜单/starry-mod-menu/scripts/surgical_y.sh',
                             'apksigner sign --out 08_output/y/o.apk in.apk\n'
                             'bash 04_scripts/_common/harden_gate.sh o.apk --game 冒险与征途\n')
        errors, _ = harden_cov.check(data)
        self.assertFalse([x for x in errors if 'GAMES' in x])

    # ---- 限定游戏验收 ----
    def test_scoped_check_ignores_other_games_backlog(self):
        w(os.path.join(self.root, '04_scripts/勇士之心/ysxz_sign.sh'),
          'apksigner sign --out 08_output/勇士之心/s.apk in.apk\n'
          'bash 04_scripts/_common/harden_gate.sh s.apk --game ysxz\n')
        w(os.path.join(self.root, '04_scripts/陌生游戏/abc_sign.sh'),
          'apksigner sign --out 08_output/陌生/o.apk in.apk\n')
        data = harden_cov.scan(self.root)
        errors_all, warns_all = harden_cov.check(data, strict=True)
        self.assertTrue(any('陌生游戏' in x for x in errors_all))
        errors_scoped, _ = harden_cov.check(data, strict=True, only_game='勇士之心')
        self.assertFalse([x for x in errors_scoped if '陌生游戏' in x])

    def test_scoped_check_still_flags_own_gap(self):
        w(os.path.join(self.root, '04_scripts/勇士之心/ysxz_assemble_103.py'),
          'subprocess.run(["apksigner","sign","--out","08_output/勇士之心/a.apk","b.apk"])\n')
        data = harden_cov.scan(self.root)
        errors, _ = harden_cov.check(data, strict=True, only_game='勇士之心')
        self.assertTrue(any('ysxz_assemble_103.py' in x for x in errors), errors)


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
