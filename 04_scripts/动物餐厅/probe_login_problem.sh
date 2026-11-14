#!/usr/bin/env bash
# Bounded read-only problem window derived from crash_forensics logcat collector.
set -euo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"
source "$HERE/../_common/env.sh"
export MSYS_NO_PATHCONV=1
OUT="$NIXIANG_ROOT/09_test/动物餐厅/problem_official_version_20261114"
mkdir -p "$OUT"
adb -s emulator-5554 logcat -d -t 4000 -v threadtime | "$NIXIANG_PYTHON" "$(nixiang_win_path "$HERE/redact_login_log.py")" > "$OUT/login_window.redacted.txt"
echo "BOUNDED_LOGIN_CAPTURE $OUT/login_window.redacted.txt"
