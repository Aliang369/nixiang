#!/usr/bin/env bash
# 勇士之心：只读抓取斌哥壳运行时内存，固定使用本游戏管线记录的 serial。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
PKG="${PKG:-com.fightingstars.games.warrior}"
OUT="$NIXIANG_ROOT/09_test/勇士之心/memdump_on"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少 .device_serial，请先跑 device_ready" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "错误: serial 为空" >&2; exit 1; }
command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }
[[ -f "$NIXIANG_ROOT/04_scripts/_common/dump_binge_shell_mem.sh" ]] || {
  echo "错误: 缺少通用斌哥内存抓取脚本" >&2
  exit 1
}

rm -rf "$OUT"
mkdir -p "$OUT"
export ANDROID_SERIAL="$SERIAL"
export ADB_BIN
bash "$NIXIANG_ROOT/04_scripts/_common/dump_binge_shell_mem.sh" "$PKG" "$OUT"
