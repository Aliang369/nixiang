#!/usr/bin/env bash
# 启动魔改 Frida server（phantom-frida，旧名 start_florida_server.sh 已替换）
# phantom 特性：随机名 ipcmodule / 随机端口（见 01_tools/phantom-frida/README.md），客户端 frida==17.16.4 原生匹配
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
BIN="${FRIDA_BIN:-$ROOT/01_tools/phantom-frida/ipcmodule-server-17.16.4-android-arm64}"
REMOTE_NAME="${FRIDA_REMOTE_NAME:-fs-arm64}"
REMOTE_PATH="/data/local/tmp/$REMOTE_NAME"

if [[ ! -f "$BIN" ]]; then
  echo "missing $BIN" >&2
  exit 1
fi

adb shell "su -c 'killall frida-server fs-arm64 ipcmodule-server 2>/dev/null || true'"
adb push "$BIN" "$REMOTE_PATH"
adb shell "su -c 'chmod 755 $REMOTE_PATH; nohup $REMOTE_PATH -D -l 127.0.0.1:27042 >/dev/null 2>&1 &'"
sleep 1
adb shell "su -c 'ps -A | grep -E \"$REMOTE_NAME|frida\" | grep -v grep || true'"
echo "phantom-frida server pushed as $REMOTE_PATH, listening 127.0.0.1:27042 (must be 27042: client -U only probes DEFAULT_CONTROL_PORT=27042; random port silently degrades to jailed adb-shell fallback, see 01_tools/phantom-frida/README.md)"
echo "client frida 17.16.4 matches: python -c 'import frida;print(frida.__version__)'"
