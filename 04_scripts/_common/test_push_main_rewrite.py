#!/usr/bin/env python3
"""Tests for git_push_main.sh's HTTPS-route URL rewrite. Run with python3 -B.

Regression guard for the 2026-10-15 defect: when `origin` is an SSH URL and no
`url.insteadOf` mapping exists, the https-proxy / https-direct routes still
transported over SSH, so `-c http.proxy` / `-c credential.helper` had no effect
and the "four self-healing channels" were really two.

These tests execute the real shell function via `bash -c` rather than
re-implementing its parsing in Python.
"""

from pathlib import Path
import subprocess
import unittest

SCRIPT = Path(__file__).resolve().parent / 'git_push_main.sh'


def rewrite(url):
    """Call the shell helper with $1=url and return its stdout (trailing newline kept off)."""
    script = f'source "$1" >/dev/null 2>&1; _pg_https_insteadof_cfg "$2"'
    result = subprocess.run(['bash', '-c', script, '_', str(SCRIPT), url],
                            capture_output=True, text=True, check=False)
    if result.returncode != 0:
        raise AssertionError(f'helper failed rc={result.returncode}: {result.stderr}')
    return result.stdout.rstrip('\n')


class InsteadOfRewriteTests(unittest.TestCase):
    def test_scp_style_ssh_url(self):
        self.assertEqual(
            rewrite('git@github.com:Aliang369/nixiang.git'),
            'url.https://github.com/Aliang369/nixiang.git.insteadOf='
            'git@github.com:Aliang369/nixiang.git')

    def test_ssh_scheme_url(self):
        self.assertEqual(
            rewrite('ssh://git@github.com/Aliang369/nixiang.git'),
            'url.https://github.com/Aliang369/nixiang.git.insteadOf='
            'ssh://git@github.com/Aliang369/nixiang.git')

    def test_other_host_is_rewritten_too(self):
        self.assertEqual(
            rewrite('git@gitlab.com:group/repo.git'),
            'url.https://gitlab.com/group/repo.git.insteadOf=git@gitlab.com:group/repo.git')

    def test_https_url_needs_no_rewrite(self):
        for url in ('https://github.com/Aliang369/nixiang.git',
                    'http://example.invalid/x.git'):
            with self.subTest(url=url):
                self.assertEqual(rewrite(url), '')

    def test_degenerate_inputs_are_inert(self):
        # No host / no path / no argument at all must stay silent instead of
        # emitting a half-built config that would break the git invocation.
        for url in ('', 'git@github.com:', 'ssh://git@github.com/', 'origin', 'gitlab.com:x'):
            with self.subTest(url=url):
                self.assertEqual(rewrite(url), '')

    def test_helper_is_safe_under_set_u_and_bash32(self):
        # archive_scoped_git.sh runs `set -euo pipefail`, and macOS ships bash
        # 3.2: an unset/empty array must not blow up the caller.
        script = ('set -euo pipefail; source "$1" >/dev/null 2>&1; '
                  'rw=(); printf "[%s]" ${rw[@]+"${rw[@]}"}; '
                  'printf "|%s|" "$(_pg_https_insteadof_cfg git@github.com:o/r.git)"')
        result = subprocess.run(['bash', '-c', script, '_', str(SCRIPT)],
                                capture_output=True, text=True, check=False)
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual(
            result.stdout,
            '[]|url.https://github.com/o/r.git.insteadOf=git@github.com:o/r.git|')

    def test_low_speed_bounding_is_nonzero(self):
        # lowSpeedLimit=0 disables the stall check entirely, which is what let a
        # single channel hang for ~10 minutes before v2.2.
        script = ('source "$1" >/dev/null 2>&1; printf "%s\\n" "${_PG_GIT_OPTS[@]}"')
        result = subprocess.run(['bash', '-c', script, '_', str(SCRIPT)],
                                capture_output=True, text=True, check=False)
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn('http.lowSpeedLimit=1000', result.stdout)
        self.assertIn('http.lowSpeedTime=60', result.stdout)
        self.assertNotIn('http.lowSpeedLimit=0', result.stdout)


class CredentialHelperTests(unittest.TestCase):
    """The inline helper must emit a line git parses as `password=<token>`.

    2026-10-16 A/B: the v2.2 first cut escaped the quotes as `\\"`, so git received
    the line `"password=<token>` — the key was `"password`, git saw no password and
    prompted ("could not read Password for 'https://x-access-token@github.com'").
    """

    def helper_output(self, token):
        """Return [command, stdout-of-command]. `token` may be a stub or None.

        When `token` is None the token lookup itself is stubbed out, because an
        empty _PG_TOKEN still falls back to `gh auth token` by design.
        """
        if token is None:
            stub = '_pg_token() { printf "%s" ""; }; '
        else:
            stub = ''
        script = (f'_PG_TOKEN="$2"; source "$1" >/dev/null 2>&1; {stub}'
                  'h="$(_pg_cred_helper)"; '
                  'printf "%s\\n" "$h"; '
                  'printf "%s\\n" "$h" | sed "s/^!//" | sh')
        result = subprocess.run(['bash', '-c', script, '_', str(SCRIPT),
                                 token or ''],
                                capture_output=True, text=True, check=False)
        self.assertEqual(result.returncode, 0, result.stderr)
        return result.stdout.split('\n')

    def test_helper_emits_unquoted_password_key(self):
        lines = self.helper_output('gho_exampletoken')
        command, username_line, password_line = lines[0], lines[1], lines[2]
        self.assertTrue(command.startswith('!'), command)
        self.assertEqual(username_line, 'username=x-access-token')
        self.assertEqual(password_line, 'password=gho_exampletoken')
        # The regression itself: no stray quote in front of the key, and no
        # backslash-escaped quotes left in the command git will run.
        self.assertFalse(password_line.startswith('"'))
        self.assertNotIn('\\"', command)

    def test_no_token_means_no_helper(self):
        # Boolean assertions only: never print a real token into test output.
        command = self.helper_output(None)[0]
        self.assertFalse(command)


if __name__ == '__main__':
    unittest.main()
