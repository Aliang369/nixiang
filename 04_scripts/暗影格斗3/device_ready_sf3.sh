#!/usr/bin/env bash
# 暗影格斗3：锁定真机、确认 root，并只读核对斌哥改包已安装。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
ADB_BIN="${ADB_BIN:-adb}"
PKG="com.wanda.sf3.wd"
DEFAULT_SERIAL="f9a116a2"
# EXPECTED_SERIAL 区分「显式点名」与「默认值」（2026-11-24 模拟器支持）：
#   · 显式点名（EXPECTED_SERIAL=emulator-5554 从环境传入）⇒ 只认那一台，允许同时有多台在线设备
#     （真机 + 模拟器共存时不再因 COUNT≠1 报错）。
#   · 不点名（默认 f9a116a2）⇒ 保持旧行为：必须恰好一台在线且必须是默认真机，绝不误操作别台。
EXPECTED_SERIAL_SET="${EXPECTED_SERIAL:-}"
EXPECTED_SERIAL="${EXPECTED_SERIAL:-$DEFAULT_SERIAL}"

command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }
DEVICES="$("$ADB_BIN" devices | tr -d '\r' | awk 'NR > 1 && $2 == "device" {print $1}')"
COUNT="$(printf '%s\n' "$DEVICES" | sed '/^$/d' | wc -l | tr -d ' ')"

if [[ -n "$EXPECTED_SERIAL_SET" ]]; then
  printf '%s\n' "$DEVICES" | grep -Fxq "$EXPECTED_SERIAL" || {
    echo "错误: 点名设备 ${EXPECTED_SERIAL} 不在线（当前在线 ${COUNT} 台）；拒绝误操作其他设备" >&2
    "$ADB_BIN" devices -l >&2
    exit 1
  }
  SERIAL="$EXPECTED_SERIAL"
else
  [[ "$COUNT" -eq 1 ]] || {
    echo "错误: 需要恰好一台在线设备，当前为 $COUNT" >&2
    "$ADB_BIN" devices -l >&2
    exit 1
  }
  SERIAL="$(printf '%s\n' "$DEVICES" | sed -n '1p')"
  if [[ "$SERIAL" != "$EXPECTED_SERIAL" ]]; then
    echo "错误: 在线设备为 ${SERIAL}，期望 ${EXPECTED_SERIAL}；拒绝误操作其他设备" >&2
    exit 1
  fi
fi

MODEL="$("$ADB_BIN" -s "$SERIAL" shell getprop ro.product.model | tr -d '\r')"
ANDROID="$("$ADB_BIN" -s "$SERIAL" shell getprop ro.build.version.release | tr -d '\r')"
ABI="$("$ADB_BIN" -s "$SERIAL" shell getprop ro.product.cpu.abi | tr -d '\r')"
PACKAGE_PATH="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's/^package://p' | head -1)"
[[ -n "$PACKAGE_PATH" ]] || { echo "错误: 设备未安装 $PKG" >&2; exit 1; }

ROOT_ID="$("$ADB_BIN" -s "$SERIAL" shell 'su -c id' 2>/dev/null | tr -d '\r' || true)"
if ! printf '%s\n' "$ROOT_ID" | grep -q 'uid=0'; then
  ROOT_ID="$("$ADB_BIN" -s "$SERIAL" shell 'su 0 id' 2>/dev/null | tr -d '\r' || true)"
fi
printf '%s\n' "$ROOT_ID" | grep -q 'uid=0' || { echo "错误: 设备未取得 root shell" >&2; exit 1; }

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
