#!/usr/bin/env bash
# 山海幻境：安装星空候选包并核对设备 base.apk 指纹。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
UNINSTALL_MARK="$SCRIPT_DIR/uninstall_done.json"
VERIFY_FILE="${SHHJ_VERIFY_FILE:-$SCRIPT_DIR/candidate_verify.json}"
APK="${SHHJ_APK:-$NIXIANG_ROOT/08_output/山海幻境/山海幻境_4_surgical-releaseSigned.apk}"
RESULT="${SHHJ_INSTALL_RESULT:-$SCRIPT_DIR/install_result.json}"
PKG="com.chx.shhj"
REMOTE="/data/local/tmp/shhj_4_surgical.apk"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少设备状态文件" >&2; exit 1; }
[[ -f "$UNINSTALL_MARK" ]] || { echo "错误: 缺少卸载记录 $UNINSTALL_MARK" >&2; exit 1; }
[[ -f "$VERIFY_FILE" ]] || { echo "错误: 缺少候选包核验 $VERIFY_FILE" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "错误: 星空包不存在: $APK" >&2; exit 1; }

SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
"$ADB_BIN" -s "$SERIAL" get-state >/dev/null

echo "==> 设备余量"
"$ADB_BIN" -s "$SERIAL" shell df -h /data | sed -n '1,4p'

echo "==> push 星空包"
"$ADB_BIN" -s "$SERIAL" push "$APK" "$REMOTE"

echo "==> 安装"
"$ADB_BIN" -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 \"$REMOTE\"'"
"$ADB_BIN" -s "$SERIAL" shell rm -f "$REMOTE" || true

echo "==> 校验设备 base.apk"
bash "$NIXIANG_ROOT/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su

VERSION_NAME="$("$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r' | sed -n 's/^[[:space:]]*versionName=//p' | head -1)"
VERSION_CODE="$("$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r' | sed -n 's/^[[:space:]]*versionCode=//p' | head -1 | awk '{print $1}')"
python3 - "$RESULT" "$SERIAL" "$APK" "$VERSION_NAME" "$VERSION_CODE" <<'PY'
import hashlib
import json
import sys
from pathlib import Path

path, serial, apk, version_name, version_code = sys.argv[1:]
h = hashlib.md5()
with open(apk, "rb") as f:
    for chunk in iter(lambda: f.read(8 * 1024 * 1024), b""):
        h.update(chunk)
data = {
    "serial": serial,
    "package": "com.chx.shhj",
    "apk_path": apk,
    "apk_md5": h.hexdigest(),
    "device_base_md5_matches": True,
    "version_name": version_name,
    "version_code": int(version_code) if version_code.isdigit() else version_code,
    "install": "ok",
}
Path(path).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

cat <<EOF
serial=$SERIAL
package=$PKG
versionName=$VERSION_NAME
versionCode=$VERSION_CODE
apk=$APK
install=ok
result=$RESULT
EOF
