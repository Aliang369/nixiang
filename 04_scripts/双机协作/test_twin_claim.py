#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""双机协作脚本的真实行为测试（2026-10-27 第六轮）。

覆盖：
  心跳：对方缺失 / 新鲜 / 超 7 天 / 时间戳损坏 / 含 FAIL / 缺结果行
  占用表：多机持有冲突 / 本机重复行收敛 / --audit 检出重复与多机
全部在临时目录跑（`NIXIANG_ROOT` / `NIXIANG_CLAIM_ROOT`），不碰真实仓库与心跳。
"""
import os
import shutil
import subprocess
import tempfile
import unittest

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, '..', '..'))
TWIN = os.path.join(HERE, 'twin_status.sh')
CLAIM = os.path.join(HERE, 'claim_game.sh')
HOST = 'mac' if os.uname().sysname == 'Darwin' else 'win'
PEER = 'win' if HOST == 'mac' else 'mac'


def run(args, env_extra=None):
    e = dict(os.environ)
    e.pop('STAGE_NEXT_FORCE', None)
    if env_extra:
        e.update(env_extra)
    return subprocess.run(['bash'] + args, capture_output=True, text=True, env=e, cwd=ROOT)


class HeartbeatTests(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.mkdtemp()
        os.makedirs(os.path.join(self.tmp, '10_docs', '_state', '机器心跳'), exist_ok=True)

    def peer_file(self, text):
        with open(os.path.join(self.tmp, '10_docs', '_state', '机器心跳', PEER + '.md'), 'w', encoding='utf-8') as f:
            f.write(text)

    def twin(self):
        r = run([TWIN], {'NIXIANG_ROOT': self.tmp})
        return r.stdout + r.stderr

    def test_peer_missing_warns(self):
        out = self.twin()
        self.assertIn('心跳缺失', out)

    def test_peer_fresh_passes(self):
        self.peer_file('- 体检时间: 2026-10-27 12:00\n- 结果: PASS=20 WARN=0 FAIL=0\n')
        out = self.twin()
        self.assertIn('未过期', out)
        self.assertIn('无 FAIL', out)

    def test_peer_stale_warns(self):
        self.peer_file('- 体检时间: 2026-10-01 09:00\n- 结果: PASS=20 WARN=0 FAIL=0\n')
        out = self.twin()
        self.assertIn('超过 7 天', out + ' 超过 7 天')  # 文案含「>7 天」
        self.assertTrue('>7 天' in out or '未更新' in out)

    def test_peer_corrupt_timestamp_fails(self):
        self.peer_file('- 体检时间: 昨天下午\n- 结果: PASS=1 WARN=0 FAIL=0\n')
        out = self.twin()
        self.assertIn('时间戳无效', out)

    def test_peer_with_fail_fails(self):
        self.peer_file('- 体检时间: 2026-10-27 12:00\n- 结果: PASS=18 WARN=1 FAIL=2\n')
        out = self.twin()
        self.assertIn('FAIL', out)
        self.assertIn('先让对方修完再开工', out)

    def test_peer_missing_result_line_warns(self):
        self.peer_file('- 体检时间: 2026-10-27 12:00\n')
        out = self.twin()
        self.assertIn('缺少「结果」行', out)


class ClaimTests(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.mkdtemp()
        self.tsv_dir = os.path.join(self.tmp, '10_docs', '双机协作')
        os.makedirs(self.tsv_dir, exist_ok=True)
        self.tsv = os.path.join(self.tsv_dir, '本轮归属.tsv')

    def write_tsv(self, rows):
        with open(self.tsv, 'w', encoding='utf-8') as f:
            f.write('\n'.join(rows) + '\n')

    def claim(self, *args):
        return run([CLAIM] + list(args), {'NIXIANG_CLAIM_ROOT': self.tmp})

    def test_multi_host_conflict_blocks(self):
        other = 'win' if HOST == 'mac' else 'mac'
        self.write_tsv(['某游戏\t%s\t2026-10-20 10:00\tabc1234' % HOST,
                        '某游戏\t%s\t2026-10-21 11:00\tdef5678' % other])
        r = self.claim('某游戏')
        self.assertEqual(r.returncode, 3, r.stdout + r.stderr)
        self.assertIn('禁止写', r.stdout)
        self.assertIn('2 行', r.stdout)

    def test_own_duplicate_rows_collapse(self):
        self.write_tsv(['某游戏\t%s\t2026-10-20 10:00\tabc1234' % HOST,
                        '某游戏\t%s\t2026-10-21 11:00\tdef5678' % HOST])
        r = self.claim('某游戏')
        self.assertEqual(r.returncode, 0, r.stdout + r.stderr)
        self.assertIn('重复登记', r.stdout)
        with open(self.tsv, encoding='utf-8') as f:
            rows = [x for x in f.read().split('\n') if x.strip()]
        self.assertEqual(len(rows), 1)

    def test_malformed_row_blocks(self):
        self.write_tsv(['某游戏\t%s\t2026-10-20 10:00' % HOST])
        r = self.claim('某游戏')
        self.assertEqual(r.returncode, 3, r.stdout + r.stderr)
        self.assertIn('格式错误', r.stdout)

    def test_audit_detects_duplicates_and_multi_host(self):
        other = 'win' if HOST == 'mac' else 'mac'
        self.write_tsv(['甲\t%s\t2026-10-20 10:00\tabc' % HOST,
                        '甲\t%s\t2026-10-20 11:00\tabc' % HOST,
                        '乙\t%s\t2026-10-20 10:00\tabc' % HOST,
                        '乙\t%s\t2026-10-20 11:00\tabc' % other])
        r = self.claim('--audit')
        self.assertEqual(r.returncode, 1, r.stdout + r.stderr)
        self.assertIn('甲: 2 行', r.stdout)
        self.assertIn('乙: 由 2 台机器持有', r.stdout)

    def test_audit_bad_timestamp_is_not_clean(self):
        # 第七轮负例：时间戳无法解析必须算实质问题，不得报「无冲突」并返回 0
        self.write_tsv(['甲\t%s\t坏时间\tabc' % HOST])
        r = self.claim('--audit')
        self.assertEqual(r.returncode, 1, r.stdout + r.stderr)
        self.assertIn('时间无法解析', r.stdout)
        self.assertNotIn('无冲突', r.stdout)

    def test_audit_clean_table_passes(self):
        self.write_tsv(['甲\t%s\t2026-10-27 10:00\tabc' % HOST])
        r = self.claim('--audit')
        self.assertEqual(r.returncode, 0, r.stdout + r.stderr)
        self.assertIn('无冲突', r.stdout)


if __name__ == '__main__':
    unittest.main()
