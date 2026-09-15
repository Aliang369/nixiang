#!/usr/bin/env bash
# 黑暗领域 v2：同签名覆盖安装并核对设备 base.apk 哈希。不卸载、不清数据。
set -euo pipefail

trap 'rc=$?; echo "ERROR: haly_install.sh line ${LINENO} rc=${rc}" >&2' ERR

NIXIANG="/Users/aliang/Documents/nixiang"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
RESULT_FILE="$SCRIPT_DIR/install_result.json"
PKG="${PKG:-com.haly.tap}"
APK="$NIXIANG/08_output/黑暗领域/黑暗领域_1.0_combo_fix-releaseSigned.apk"
REMOTE="/data/local/tmp/haly_combo_fix.apk"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$STATE_FILE" ]] || { echo "ERROR: missing device state; run device_ready first" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "ERROR: missing APK $APK" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty device serial" >&2; exit 1; }

APK_MD5="$(md5 -q "$APK")"
APK_SHA256="$(shasum -a 256 "$APK" | awk '{print $1}')"

"$ADB_BIN" -s "$SERIAL" push "$APK" "$REMOTE" >/dev/null
"$ADB_BIN" -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
"$ADB_BIN" -s "$SERIAL" shell "pm install -r -d --user 0 $REMOTE"
"$ADB_BIN" -s "$SERIAL" shell rm -f "$REMOTE" 2>/dev/null || true

VERIFY_OUT="$(bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su)"
printf '%s\n' "$VERIFY_OUT"
BASE="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | grep '^package:' | grep 'base\.apk' | head -1 | sed 's/^package://')"
[[ -n "$BASE" ]] || { echo "ERROR: cannot resolve device base.apk" >&2; exit 1; }
DEVICE_MD5="$("$ADB_BIN" -s "$SERIAL" shell "su -c 'md5sum $BASE'" 2>/dev/null | tr -d '\r\n' | awk '{print $1}')"
[[ "$DEVICE_MD5" == "$APK_MD5" ]] || {
  echo "ERROR: device md5 mismatch local=$APK_MD5 device=${DEVICE_MD5:-unknown}" >&2
  exit 1
}

VERSION_NAME="$("$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r' | sed -n 's/^[[:space:]]*versionName=//p' | head -1)"
python3 - "$RESULT_FILE" "$SERIAL" "$PKG" "$APK" "$APK_MD5" "$APK_SHA256" "$DEVICE_MD5" "$VERSION_NAME" <<'PY'
import json
import sys
from pathlib import Path

path, serial, package, apk, apk_md5, apk_sha256, device_md5, version_name = sys.argv[1:]
data = {
    "game": "黑暗领域",
    "serial": serial,
    "package": package,
    "apk_path": apk,
    "apk_md5": apk_md5,
    "apk_sha256": apk_sha256,
    "device_md5": device_md5,
    "version_name": version_name,
    "install_mode": "cover_install_same_signature",
    "data_cleared": False,
}
Path(path).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

printf '[install] device_md5=%s\n[install] result=%s\n' "$DEVICE_MD5" "$RESULT_FILE"
