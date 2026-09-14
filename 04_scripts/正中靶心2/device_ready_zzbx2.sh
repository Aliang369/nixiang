#!/usr/bin/env bash
# 正中靶心2：识别唯一真机并确认 root 可用；只读设备，不安装、不清数据。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
ADB_BIN="${ADB_BIN:-adb}"
REQUESTED_SERIAL="${SERIAL:-}"

command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }

if [[ -n "$REQUESTED_SERIAL" ]]; then
  SERIALS="$REQUESTED_SERIAL"
else
  SERIALS="$("$ADB_BIN" devices | awk 'NR > 1 && $2 == "device" {print $1}')"
fi

COUNT="$(printf '%s\n' "$SERIALS" | sed '/^$/d' | wc -l | tr -d ' ')"
if [[ "$COUNT" -ne 1 ]]; then
  echo "错误: 需要唯一在线设备，当前识别到 $COUNT 台" >&2
  printf '%s\n' "$SERIALS" >&2
  exit 1
fi

SERIAL="$(printf '%s\n' "$SERIALS" | sed -n '1p')"
"$ADB_BIN" -s "$SERIAL" get-state >/dev/null

ROOT_ID="$("$ADB_BIN" -s "$SERIAL" shell "su -c id" 2>/dev/null | tr -d '\r' || true)"
if [[ "$ROOT_ID" != *"uid=0"* ]]; then
  echo "错误: root 不可用或未授权: $SERIAL" >&2
  echo "实际: ${ROOT_ID:-<empty>}" >&2
  exit 1
fi

printf '%s\n' "$SERIAL" > "$STATE_FILE"
printf 'serial=%s\nroot=%s\nstate=%s\n' "$SERIAL" "$ROOT_ID" "$STATE_FILE"
