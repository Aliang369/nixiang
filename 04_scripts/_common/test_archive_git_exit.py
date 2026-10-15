#!/usr/bin/env python3
"""Exit-code contract tests for the two archive git entry points. Run python3 -B.

Regression guard for the 2026-10-16 defect: both scripts ended with

    push_main main || echo "[push] 未推送：本地提交已保留，汇报里写明 ahead"

so a failed push still exited 0. Callers that check the exit code --
`finalize_round.sh`'s `rc=$?; [ "$rc" -eq 0 ] || step_fail "$rc"` and the
pipeline `archive` / `archive_close` stages -- therefore treated "committed but
not pushed" as success, and `finalize_round.sh`'s step_fail branch was dead code.

The real scripts run inside a throwaway repository with a stub `push_main`, so
the commit path is exercised for real while the network is not.
"""

from pathlib import Path
import os
import shutil
import subprocess
import tempfile
import unittest

HERE = Path(__file__).resolve().parent

STUB_PUSH_MAIN = '''#!/usr/bin/env bash
# Test stub: stands in for the real four-channel push.
push_main() {
  echo "[stub] push_main called branch=${1:-<none>} rc=${STUB_PUSH_RC:-0}"
  if [ -n "${STUB_MARKER:-}" ]; then echo "called branch=${1:-}" >> "$STUB_MARKER"; fi
  return "${STUB_PUSH_RC:-0}"
}
'''

# Generates a manifest whose content differs on every call, so repeat runs still
# produce a non-empty commit (otherwise the script correctly reports "no change").
STUB_DELIVERY_MANIFEST = '''#!/usr/bin/env python3
import argparse, pathlib, sys, time
p = argparse.ArgumentParser()
p.add_argument('--game')
p.add_argument('--output')
p.add_argument('--check', action='store_true')
a = p.parse_args()
if a.check:
    sys.exit(0)
pathlib.Path(a.output).write_text(
    '{"game": "%s", "seq": %d}\\n' % (a.game, time.time_ns()), encoding='utf-8')
'''


class GitFixture(unittest.TestCase):
    """Throwaway repo holding copies of the real scripts plus stubbed siblings."""

    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.common = self.root / '04_scripts' / '_common'
        self.common.mkdir(parents=True)
        (self.common / 'git_push_main.sh').write_text(STUB_PUSH_MAIN, encoding='utf-8')
        self.marker = self.root / 'push_called.txt'
        self.git('init', '-q')
        self.git('config', 'user.email', 'test@example.invalid')
        self.git('config', 'user.name', 'test')
        self.write('seed.txt', 'seed\n')
        self.git('add', '--', 'seed.txt')
        self.git('commit', '-q', '-m', 'seed')

    def install(self, *names):
        for name in names:
            shutil.copy(HERE / name, self.common / name)

    def git(self, *args):
        result = subprocess.run(['git', *args], cwd=self.root,
                                capture_output=True, text=True, check=False)
        self.assertEqual(result.returncode, 0, result.stderr)
        return result.stdout

    def write(self, name, text):
        path = self.root / name
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(text, encoding='utf-8')
        return path

    def run_script(self, script, *args, push_rc=None, extra_env=None):
        env = dict(os.environ)
        env['STUB_MARKER'] = str(self.marker)
        if push_rc is not None:
            env['STUB_PUSH_RC'] = str(push_rc)
        env.update(extra_env or {})
        return subprocess.run(['bash', str(self.common / script), *args], cwd=self.root,
                              capture_output=True, text=True, check=False, env=env)

    def commit_count(self):
        return len(self.git('log', '--oneline').splitlines())


class ArchiveScopedGitExitTests(GitFixture):
    def setUp(self):
        super().setUp()
        self.install('archive_scoped_git.sh')

    def test_failed_push_still_commits_but_returns_nonzero(self):
        self.write('a.txt', 'one\n')
        result = self.run_script('archive_scoped_git.sh',
                                 '--message', 't', '--path', 'a.txt', push_rc=1)
        self.assertEqual(result.returncode, 1, result.stdout + result.stderr)
        # The commit must survive: that is why the exit code is the signal
        # instead of rolling the commit back.
        self.assertEqual(self.commit_count(), 2)
        self.assertIn('未推送（push_main rc=1）', result.stdout)
        self.assertIn('未同步', result.stderr)
        self.assertIn('提交完成', result.stdout)

    def test_successful_push_returns_zero(self):
        self.write('a.txt', 'one\n')
        result = self.run_script('archive_scoped_git.sh',
                                 '--message', 't', '--path', 'a.txt', push_rc=0)
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertNotIn('未推送', result.stdout)
        self.assertNotIn('未同步', result.stderr)

    def test_diverged_remote_propagates_three(self):
        self.write('a.txt', 'one\n')
        result = self.run_script('archive_scoped_git.sh',
                                 '--message', 't', '--path', 'a.txt', push_rc=3)
        self.assertEqual(result.returncode, 3, result.stdout + result.stderr)
        self.assertIn('退出码 3', result.stderr)
        self.assertEqual(self.commit_count(), 2)

    def test_no_push_skips_push_main_and_returns_zero(self):
        self.write('a.txt', 'one\n')
        result = self.run_script('archive_scoped_git.sh',
                                 '--message', 't', '--path', 'a.txt', '--no-push',
                                 extra_env={'STUB_PUSH_RC': '1'})  # fails if called
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertEqual(self.commit_count(), 2)
        self.assertFalse(self.marker.exists(), 'push_main must not run under --no-push')

    def test_nothing_to_commit_returns_zero_without_pushing(self):
        result = self.run_script('archive_scoped_git.sh', '--message', 't', '--path', 'seed.txt')
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn('没有待提交的改动', result.stdout)
        self.assertFalse(self.marker.exists())

    def test_dry_run_returns_zero_without_committing_or_pushing(self):
        # --dry-run implies --staged-only: it validates the *current* index, so a
        # change must be staged first for the dry-run banner to be reached.
        self.write('a.txt', 'one\n')
        self.git('add', '--', 'a.txt')
        result = self.run_script('archive_scoped_git.sh', '--message', 't',
                                 '--path', 'a.txt', '--dry-run', push_rc=1)
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn('dry-run', result.stdout)
        self.assertEqual(self.commit_count(), 1)
        self.assertFalse(self.marker.exists())

    def test_dry_run_with_empty_index_reports_nothing_to_commit(self):
        # Documents observed behaviour rather than a banner a caller cannot rely on.
        result = self.run_script('archive_scoped_git.sh', '--message', 't',
                                 '--path', 'seed.txt', '--dry-run')
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn('没有待提交的改动', result.stdout)
        self.assertFalse(self.marker.exists())

    def test_whitelist_violation_still_exits_one(self):
        # Pre-existing contract: scope violations keep their own non-zero exit.
        self.write('a.txt', 'one\n')
        self.git('add', '--', 'a.txt')
        result = self.run_script('archive_scoped_git.sh', '--message', 't', '--path', 'seed.txt')
        self.assertEqual(result.returncode, 1, result.stdout + result.stderr)
        self.assertIn('白名单外路径', result.stderr)
        self.assertFalse(self.marker.exists())


class ArchiveCloseGitExitTests(GitFixture):
    GAME = '试玩'

    def setUp(self):
        super().setUp()
        self.install('archive_close_git.sh')
        (self.common / 'delivery_manifest.py').write_text(
            STUB_DELIVERY_MANIFEST, encoding='utf-8')
        self.write(f'04_scripts/{self.GAME}/pipeline.json', '{"game": "%s"}\n' % self.GAME)
        (self.root / '10_docs' / self.GAME).mkdir(parents=True, exist_ok=True)
        self.git('add', '--', f'04_scripts/{self.GAME}/pipeline.json')
        self.git('commit', '-q', '-m', 'register game')

    def run_close(self, *args, **kwargs):
        return self.run_script('archive_close_git.sh', '--game', self.GAME, *args, **kwargs)

    def test_failed_push_still_commits_but_returns_nonzero(self):
        result = self.run_close('--message', 't', push_rc=1)
        self.assertEqual(result.returncode, 1, result.stdout + result.stderr)
        self.assertEqual(self.commit_count(), 3)
        self.assertIn('未推送（push_main rc=1）', result.stdout)
        self.assertIn('未同步', result.stderr)

    def test_successful_push_returns_zero(self):
        result = self.run_close('--message', 't', push_rc=0)
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertIn('状态提交完成', result.stdout)
        self.assertNotIn('未同步', result.stderr)

    def test_diverged_remote_propagates_three(self):
        result = self.run_close('--message', 't', push_rc=3)
        self.assertEqual(result.returncode, 3, result.stdout + result.stderr)
        self.assertIn('退出码 3', result.stderr)

    def test_no_push_skips_push_main_and_returns_zero(self):
        result = self.run_close('--message', 't', '--no-push',
                                extra_env={'STUB_PUSH_RC': '1'})
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        self.assertFalse(self.marker.exists(), 'push_main must not run under --no-push')


if __name__ == '__main__':
    unittest.main()
