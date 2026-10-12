#!/usr/bin/env bash
# 明日方舟：锁定真机、确认 root，并从当前前台 Activity 解析实际游戏包名。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
PACKAGE_FILE="$SCRIPT_DIR/.device_package"
INFO_FILE="$SCRIPT_DIR/device_info.json"
ADB_BIN="${ADB_BIN:-adb}"
EXPECTED_SERIAL="${EXPECTED_SERIAL:-f9a116a2}"

command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }
DEVICES="$("$ADB_BIN" devices | tr -d '\r' | awk 'NR > 1 && $2 == "device" {print $1}')"
COUNT="$(printf '%s\n' "$DEVICES" | sed '/^$/d' | wc -l | tr -d ' ')"
[[ "$COUNT" -eq 1 ]] || {
  echo "错误: 需要恰好一台在线设备，当前为 $COUNT" >&2
  "$ADB_BIN" devices -l >&2
  exit 1
}

SERIAL="$(printf '%s\n' "$DEVICES" | sed -n '1p')"
if [[ -n "$EXPECTED_SERIAL" && "$SERIAL" != "$EXPECTED_SERIAL" ]]; then
  echo "错误: 在线设备为 ${SERIAL}，期望 ${EXPECTED_SERIAL}；拒绝误操作其他设备" >&2
  exit 1
fi

MODEL="$("$ADB_BIN" -s "$SERIAL" shell getprop ro.product.model | tr -d '\r')"
ANDROID="$("$ADB_BIN" -s "$SERIAL" shell getprop ro.build.version.release | tr -d '\r')"
ABI="$("$ADB_BIN" -s "$SERIAL" shell getprop ro.product.cpu.abi | tr -d '\r')"

ROOT_ID="$("$ADB_BIN" -s "$SERIAL" shell 'su -c id' 2>/dev/null | tr -d '\r' || true)"
if ! printf '%s\n' "$ROOT_ID" | grep -q 'uid=0'; then
  ROOT_ID="$("$ADB_BIN" -s "$SERIAL" shell 'su 0 id' 2>/dev/null | tr -d '\r' || true)"
fi
printf '%s\n' "$ROOT_ID" | grep -q 'uid=0' || { echo "错误: 设备未取得 root shell" >&2; exit 1; }

ACTIVITY_DUMP="$("$ADB_BIN" -s "$SERIAL" shell dumpsys activity activities 2>/dev/null | tr -d '\r' || true)"
FOREGROUND_ACTIVITY="$(printf '%s\n' "$ACTIVITY_DUMP" | awk '
  /topResumedActivity|mResumedActivity|ResumedActivity/ {
    for (i = 1; i <= NF; i++) {
      if ($i ~ /^[A-Za-z0-9._]+\//) {
        print $i
        exit
      }
    }
  }
')"

if [[ -z "$FOREGROUND_ACTIVITY" ]]; then
  FOREGROUND_ACTIVITY="$("$ADB_BIN" -s "$SERIAL" shell dumpsys window windows 2>/dev/null | tr -d '\r' |
    sed -n 's/.*mCurrentFocus=.* \([A-Za-z0-9._]*\/[^ }]*\).*/\1/p' | head -1)"
fi

[[ -n "$FOREGROUND_ACTIVITY" ]] || {
  echo "错误: 未解析到前台 Activity；请把斌哥改包的游戏画面切到前台后重试" >&2
  exit 1
}

PKG="${FOREGROUND_ACTIVITY%%/*}"
case "$PKG" in
  com.android.systemui|com.android.launcher*|com.oppo.launcher|com.coloros.*)
    echo "错误: 当前前台为系统界面: ${FOREGROUND_ACTIVITY}；请先把游戏切到前台" >&2
    exit 1
    ;;
esac
[[ "$PKG" =~ ^[A-Za-z0-9._]+$ ]] || { echo "错误: 非预期包名: $PKG" >&2; exit 1; }

PACKAGE_PATHS="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's/^package://p')"
[[ -n "$PACKAGE_PATHS" ]] || { echo "错误: 设备未安装前台包: $PKG" >&2; exit 1; }
PATH_COUNT="$(printf '%s\n' "$PACKAGE_PATHS" | sed '/^$/d' | wc -l | tr -d ' ')"
[[ "$PATH_COUNT" -eq 1 ]] || {
  echo "错误: 检测到 $PATH_COUNT 个 APK 路径；抓点第一版只接受单一完整 base APK" >&2
  printf '%s\n' "$PACKAGE_PATHS" >&2
  exit 1
}
REMOTE_APK="$(printf '%s\n' "$PACKAGE_PATHS" | sed -n '1p')"

printf '%s\n' "$SERIAL" > "$STATE_FILE"
printf '%s\n' "$PKG" > "$PACKAGE_FILE"
python3 - "$INFO_FILE" "$SERIAL" "$MODEL" "$ANDROID" "$ABI" "$PKG" "$FOREGROUND_ACTIVITY" "$REMOTE_APK" "$ROOT_ID" <<'PY'
import json
import sys
from pathlib import Path

path, serial, model, android, abi, package, activity, remote_apk, root_id = sys.argv[1:]
data = {
    "serial": serial,
    "model": model,
    "android": android,
    "abi": abi,
    "package": package,
    "foreground_activity": activity,
    "remote_apk": remote_apk,
    "root_id": root_id,
}
Path(path).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

cat <<EOF
serial=$SERIAL
model=$MODEL
android=$ANDROID
abi=$ABI
package=$PKG
foreground_activity=$FOREGROUND_ACTIVITY
remote_apk=$REMOTE_APK
root=$ROOT_ID
state_file=$STATE_FILE
package_file=$PACKAGE_FILE
info_file=$INFO_FILE
EOF
