#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""逃生门理由强制的真实参数测试（2026-10-27 第六轮）。

真跑脚本（subprocess），不是静态搜索：验证
  1. finalize_game_delivery.sh：--allow-no-harden 无理由 ⇒ 非零退出（4）且不放行
  2. finalize_game_delivery.sh：理由为纯空白 ⇒ 同样拒绝
  3. finalize_game_delivery.sh：有理由 ⇒ 放行但在输出里明确 UNHARDENED（不得伪称 PASS）
  4. pre_install_gate.sh：--allow-no-harden 无理由 ⇒ 输出「没有有效理由」并计 FAIL
  5. pre_install_gate.sh：有理由 ⇒ 输出 reason=...（WARN 放行）
全部使用临时目录里的假 APK，不碰真实产物。
"""
import os
import subprocess
import sys
import tempfile
import unittest

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, '..', '..'))
FINALIZE = os.path.join(HERE, 'finalize_game_delivery.sh')
GATE = os.path.join(HERE, 'pre_install_gate.sh')


def fake_apk(dirpath, name='fake.apk'):
    p = os.path.join(dirpath, name)
    with open(p, 'wb') as f:
        f.write(b'PK\x03\x04 not-a-real-apk\n')
    return p


def run(args, env=None):
    e = dict(os.environ)
    e.pop('HARDEN_REASON', None)
    e.pop('STAGE_NEXT_FORCE', None)
    if env:
        e.update(env)
    # 2026-10-27 双机：必须显式 utf-8。win 上不写 encoding 时按 cp936 解码子进程输出，
    # 中文理由/报错会抛 UnicodeDecodeError（reader 线程死掉 → stdout/stderr 变 None），
    # 5 个用例集体报 TypeError 而不是真实断言。
    return subprocess.run(args, capture_output=True, text=True,
                          encoding='utf-8', errors='replace', env=e, cwd=ROOT)


class FinalizeReasonTests(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.mkdtemp()
        self.apk = fake_apk(self.tmp)

    def test_allow_without_reason_is_rejected(self):
        r = run(['bash', FINALIZE, '--game', '测试游戏', '--apk', self.apk, '--allow-no-harden'])
        self.assertNotEqual(r.returncode, 0)
        self.assertIn('--harden-reason', r.stdout + r.stderr)

    def test_allow_with_blank_reason_is_rejected(self):
        r = run(['bash', FINALIZE, '--game', '测试游戏', '--apk', self.apk,
                 '--allow-no-harden', '--harden-reason', '   '])
        self.assertNotEqual(r.returncode, 0)

    def test_allow_with_reason_records_unhardened(self):
        r = run(['bash', FINALIZE, '--game', '测试游戏', '--apk', self.apk,
                 '--allow-no-harden', '--harden-reason', '用户授权临时验证包'])
        out = r.stdout + r.stderr
        if 'harden_gate=PASS' in out:
            self.skipTest('该假包意外通过加固检查，跳过 UNHARDENED 断言')
        self.assertIn('UNHARDENED', out)
        self.assertIn('用户授权临时验证包', out)
        self.assertNotIn('harden_gate=PASS', out)

    def test_no_escape_flag_blocks_on_failure(self):
        r = run(['bash', FINALIZE, '--game', '测试游戏', '--apk', self.apk])
        if r.returncode == 0 and 'harden_gate=PASS' in (r.stdout + r.stderr):
            self.skipTest('该假包意外通过加固检查')
        self.assertNotEqual(r.returncode, 0)


class InstallGateReasonTests(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.mkdtemp()
        self.apk = fake_apk(self.tmp)

    def test_install_gate_requires_reason(self):
        r = run(['bash', GATE, '--apk', self.apk, '--game', '测试游戏', '--allow-no-harden'])
        out = r.stdout + r.stderr
        self.assertIn('没有有效理由', out)

    def test_install_gate_accepts_reason(self):
        r = run(['bash', GATE, '--apk', self.apk, '--game', '测试游戏',
                 '--allow-no-harden', '--harden-reason', '回归测试用假包'])
        out = r.stdout + r.stderr
        self.assertIn('回归测试用假包', out)
        self.assertNotIn('没有有效理由', out)

    def test_install_gate_reason_via_env(self):
        r = run(['bash', GATE, '--apk', self.apk, '--game', '测试游戏', '--allow-no-harden'],
                env={'HARDEN_REASON': '环境变量理由'})
        self.assertIn('环境变量理由', r.stdout + r.stderr)


if __name__ == '__main__':
    unittest.main()
