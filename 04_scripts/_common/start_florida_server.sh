#!/usr/bin/env bash
# 启动魔改 Frida server（Florida / strongR 系），默认随机化进程名痕迹用 fs-arm64
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
BIN="${FLORIDA_BIN:-$ROOT/01_tools/florida/florida-server-17.9.1-android-arm64}"
REMOTE_NAME="${FLORIDA_REMOTE_NAME:-fs-arm64}"
REMOTE_PATH="/data/local/tmp/$REMOTE_NAME"

if [[ ! -f "$BIN" ]]; then
  echo "missing $BIN" >&2
  exit 1
fi

adb shell "su -c 'killall frida-server fs-arm64 florida-server 2>/dev/null || true'"
adb push "$BIN" "$REMOTE_PATH"
adb shell "su -c 'chmod 755 $REMOTE_PATH; nohup $REMOTE_PATH -D >/data/local/tmp/${REMOTE_NAME}.log 2>&1 &'"
sleep 1
adb shell "su -c 'ps -A | grep -E \"$REMOTE_NAME|frida\" | grep -v grep || true'"
echo "Florida server pushed as $REMOTE_PATH (listen default 27042)"
echo "client must be frida 17.9.1: python -c 'import frida;print(frida.__version__)'"
