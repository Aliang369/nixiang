#!/usr/bin/env bash
# 追迹迷城设备准备：要求恰好一台在线真机，并记录 serial/系统/root 状态。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
ADB_BIN="${ADB_BIN:-adb}"

command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }

DEVICES="$("$ADB_BIN" devices | tr -d '\r' | awk 'NR > 1 && $2 == "device" {print $1}')"
COUNT="$(printf '%s\n' "$DEVICES" | sed '/^$/d' | wc -l | tr -d ' ')"
if [[ "$COUNT" -ne 1 ]]; then
  echo "错误: 需要恰好一台 device 状态设备，当前为 $COUNT" >&2
  "$ADB_BIN" devices -l >&2
  exit 1
fi

SERIAL="$(printf '%s\n' "$DEVICES" | sed -n '1p')"
MODEL="$("$ADB_BIN" -s "$SERIAL" shell getprop ro.product.model | tr -d '\r')"
ANDROID="$("$ADB_BIN" -s "$SERIAL" shell getprop ro.build.version.release | tr -d '\r')"
ABI="$("$ADB_BIN" -s "$SERIAL" shell getprop ro.product.cpu.abi | tr -d '\r')"

ROOT_ID="$("$ADB_BIN" -s "$SERIAL" shell "su -c id" 2>/dev/null | tr -d '\r' || true)"
if ! printf '%s\n' "$ROOT_ID" | grep -q 'uid=0'; then
  ROOT_ID="$("$ADB_BIN" -s "$SERIAL" shell "su 0 id" 2>/dev/null | tr -d '\r' || true)"
fi
if ! printf '%s\n' "$ROOT_ID" | grep -q 'uid=0'; then
  echo "错误: 设备未取得 root shell（su -c id / su 0 id 均失败）" >&2
  exit 1
fi

printf '%s\n' "$SERIAL" > "$STATE_FILE"
cat <<EOF
serial=$SERIAL
model=$MODEL
android=$ANDROID
abi=$ABI
root=$ROOT_ID
state_file=$STATE_FILE
EOF
