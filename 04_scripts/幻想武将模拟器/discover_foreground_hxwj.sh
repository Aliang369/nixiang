#!/usr/bin/env bash
# 幻想武将模拟器：从当前前台 Activity 只读识别目标包，并记录版本/安装路径。
set -euo pipefail

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

ROOT_ID="$("$ADB_BIN" -s "$SERIAL" shell 'su -c id' 2>/dev/null | tr -d '\r' || true)"
if ! printf '%s\n' "$ROOT_ID" | grep -q 'uid=0'; then
  ROOT_ID="$("$ADB_BIN" -s "$SERIAL" shell 'su 0 id' 2>/dev/null | tr -d '\r' || true)"
fi
printf '%s\n' "$ROOT_ID" | grep -q 'uid=0' || { echo "错误: 设备未取得 root shell" >&2; exit 1; }

ACTIVITY_DUMP="$("$ADB_BIN" -s "$SERIAL" shell dumpsys activity activities | tr -d '\r')"
PKG="$(printf '%s\n' "$ACTIVITY_DUMP" | sed -nE 's/.*topResumedActivity=.* u[0-9]+ ([A-Za-z0-9_.]+)\/([^ }]+).*/\1/p' | head -1)"
if [[ -z "$PKG" ]]; then
  PKG="$(printf '%s\n' "$ACTIVITY_DUMP" | sed -nE 's/.*mResumedActivity=.* u[0-9]+ ([A-Za-z0-9_.]+)\/([^ }]+).*/\1/p' | head -1)"
fi
if [[ -z "$PKG" ]]; then
  WINDOW_DUMP="$("$ADB_BIN" -s "$SERIAL" shell dumpsys window windows | tr -d '\r')"
  PKG="$(printf '%s\n' "$WINDOW_DUMP" | sed -nE 's/.*mCurrentFocus=.* u[0-9]+ ([A-Za-z0-9_.]+)\/([^ }]+).*/\1/p' | head -1)"
fi
[[ -n "$PKG" ]] || { echo "错误: 未识别到前台包名；请保持游戏在前台后重试" >&2; exit 1; }

DUMP="$("$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r')"
VERSION_NAME="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionName=//p' | head -1)"
VERSION_CODE="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionCode=//p' | head -1 | awk '{print $1}')"
INSTALLER="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*installerPackageName=//p' | head -1)"
PRIMARY_ABI="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*primaryCpuAbi=//p' | head -1)"
PACKAGE_PATHS="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's/^package://p')"
[[ -n "$PACKAGE_PATHS" ]] || { echo "错误: 设备未安装包: $PKG" >&2; exit 1; }
PATH_COUNT="$(printf '%s\n' "$PACKAGE_PATHS" | sed '/^$/d' | wc -l | tr -d ' ')"
[[ "$PATH_COUNT" -eq 1 ]] || {
  echo "错误: 检测到 $PATH_COUNT 个 APK 路径；当前只支持完整单 base APK" >&2
  printf '%s\n' "$PACKAGE_PATHS" >&2
  exit 1
}
REMOTE_APK="$(printf '%s\n' "$PACKAGE_PATHS" | sed -n '1p')"
DEV_APK_MD5="$("$ADB_BIN" -s "$SERIAL" shell "su -c 'md5sum $REMOTE_APK'" 2>/dev/null | tr -d '\r\n' | awk '{print $1}' || true)"
DEV_APK_SIZE="$("$ADB_BIN" -s "$SERIAL" shell "su -c 'stat -c %s $REMOTE_APK'" 2>/dev/null | tr -d '\r' || true)"

cat <<EOF
serial=$SERIAL
root=$ROOT_ID
package=$PKG
versionName=${VERSION_NAME:-<unknown>}
versionCode=${VERSION_CODE:-<unknown>}
installer=${INSTALLER:-<none>}
primaryCpuAbi=${PRIMARY_ABI:-<none>}
remote_apk=$REMOTE_APK
device_apk_size=${DEV_APK_SIZE:-<unknown>}
device_apk_md5=${DEV_APK_MD5:-<unknown>}
EOF
