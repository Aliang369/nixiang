#!/usr/bin/env bash
# 宝石战争：设备锁定与 root 只读检查。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
ADB_BIN="${ADB_BIN:-adb}"
PKG="com.acing.gow.adv"
EXPECTED_SERIAL="${EXPECTED_SERIAL:-f9a116a2}"

command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }
DEVICES="$("$ADB_BIN" devices | tr -d '\r' | awk 'NR > 1 && $2 == "device" {print $1}')"
COUNT="$(printf '%s\n' "$DEVICES" | sed '/^$/d' | wc -l | tr -d ' ')"
[[ "$COUNT" -eq 1 ]] || { echo "错误: 需要恰好一台在线设备，当前 $COUNT" >&2; exit 1; }
SERIAL="$(printf '%s\n' "$DEVICES" | sed -n '1p')"
if [[ -n "$EXPECTED_SERIAL" && "$SERIAL" != "$EXPECTED_SERIAL" ]]; then
  echo "错误: 在线设备为 $SERIAL，期望 $EXPECTED_SERIAL" >&2
  exit 1
fi

MODEL="$("$ADB_BIN" -s "$SERIAL" shell getprop ro.product.model | tr -d '\r')"
ANDROID="$("$ADB_BIN" -s "$SERIAL" shell getprop ro.build.version.release | tr -d '\r')"
ABI="$("$ADB_BIN" -s "$SERIAL" shell getprop ro.product.cpu.abi | tr -d '\r')"
PACKAGE_PATH="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's/^package://p' | head -1)"
[[ -n "$PACKAGE_PATH" ]] || { echo "错误: 设备未安装 $PKG" >&2; exit 1; }
ROOT_ID="$("$ADB_BIN" -s "$SERIAL" shell 'su -c id' 2>/dev/null | tr -d '\r' || true)"
[[ "$ROOT_ID" == *"uid=0"* ]] || { echo "错误: 设备未取得 root shell" >&2; exit 1; }

printf '%s\n' "$SERIAL" > "$STATE_FILE"
cat <<EOF
serial=$SERIAL
model=$MODEL
android=$ANDROID
abi=$ABI
package=$PKG
package_path=$PACKAGE_PATH
root=$ROOT_ID
state_file=$STATE_FILE
EOF
