#!/usr/bin/env bash
# 勇士之心：抓取指定运行时状态的斌哥壳内存和 mod_sw.txt。
# 用法: dump_mem_state_ysxz.sh <state_name>
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
PKG="${PKG:-com.fightingstars.games.warrior}"
ADB_BIN="${ADB_BIN:-adb}"
STATE_NAME="${1:-}"
OUT="$NIXIANG_ROOT/09_test/勇士之心/$STATE_NAME"

[[ -n "$STATE_NAME" && "$STATE_NAME" =~ ^[A-Za-z0-9._-]+$ ]] || {
  echo "用法: $0 <state_name>" >&2
  exit 1
}
[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少 .device_serial，请先跑 device_ready" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "错误: serial 为空" >&2; exit 1; }
command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }

rm -rf "$OUT"
mkdir -p "$OUT"
export ANDROID_SERIAL="$SERIAL"
export ADB_BIN
bash "$NIXIANG_ROOT/04_scripts/_common/dump_binge_shell_mem.sh" "$PKG" "$OUT"
"$ADB_BIN" -s "$SERIAL" exec-out "su -c 'cat /data/data/$PKG/files/mod_sw.txt'" > "$OUT/mod_sw.txt"
[[ -s "$OUT/mod_sw.txt" ]] || { echo "错误: 未读取到 mod_sw.txt" >&2; exit 1; }
printf 'state=%s\nout=%s\nmod_sw=%s\n' "$STATE_NAME" "$OUT" "$(cat "$OUT/mod_sw.txt")"
