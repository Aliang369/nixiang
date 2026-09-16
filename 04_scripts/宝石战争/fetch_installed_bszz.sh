#!/usr/bin/env bash
# 宝石战争：从真机只读提取当前安装的 TapTap 官方单 base APK。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
META_FILE="$SCRIPT_DIR/official_apk.json"
PKG="com.acing.gow.adv"
GAME="宝石战争"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少设备状态文件，请先跑 device_ready" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
DUMP="$("$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r')"
VERSION_NAME="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionName=//p' | head -1)"
VERSION_CODE="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionCode=//p' | head -1 | awk '{print $1}')"
INSTALLER="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*installerPackageName=//p' | head -1)"
ORIGINATING="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*originatingPackageName=//p' | head -1)"
PRIMARY_ABI="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*primaryCpuAbi=//p' | head -1)"
PACKAGE_PATHS="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's/^package://p')"
[[ -n "$VERSION_NAME" && "$VERSION_CODE" == "6000012" ]] || { echo "错误: 版本不符，期望 6.0.012/vc6000012" >&2; exit 1; }
[[ "$(printf '%s\n' "$PACKAGE_PATHS" | sed '/^$/d' | wc -l | tr -d ' ')" -eq 1 ]] || { echo "错误: 非单 base APK，拒绝" >&2; exit 1; }
REMOTE_APK="$(printf '%s\n' "$PACKAGE_PATHS" | sed -n '1p')"

OUT_DIR="$NIXIANG_ROOT/02_apks/$GAME"
OUT="$OUT_DIR/${GAME}_${VERSION_NAME}_official_from_device.apk"
TMP="$OUT.part"
[[ ! -e "$OUT" ]] || { echo "错误: 目标 APK 已存在，拒绝覆盖: $OUT" >&2; exit 1; }
mkdir -p "$OUT_DIR"
trap 'rm -f "$TMP"' EXIT

echo "==> 只读提取 $REMOTE_APK"
if ! "$ADB_BIN" -s "$SERIAL" pull "$REMOTE_APK" "$TMP"; then
  REMOTE_TMP="/data/local/tmp/bszz_official_base.apk"
  "$ADB_BIN" -s "$SERIAL" shell "su -c 'cp \"$REMOTE_APK\" \"$REMOTE_TMP\" && chmod 644 \"$REMOTE_TMP\"'"
  "$ADB_BIN" -s "$SERIAL" pull "$REMOTE_TMP" "$TMP"
  "$ADB_BIN" -s "$SERIAL" shell "su -c 'rm -f \"$REMOTE_TMP\"'" || true
fi
unzip -tq "$TMP" >/dev/null
mv "$TMP" "$OUT"

SIZE="$(stat -f '%z' "$OUT")"
SHA256="$(shasum -a 256 "$OUT" | awk '{print $1}')"
MD5="$(md5 -q "$OUT")"
python3 - "$META_FILE" "$OUT" "$VERSION_NAME" "$VERSION_CODE" "$INSTALLER" "$ORIGINATING" "$PRIMARY_ABI" "$SERIAL" "$REMOTE_APK" "$SIZE" "$SHA256" "$MD5" <<'PY'
import json, sys
from pathlib import Path
path, apk, version_name, version_code, installer, originating, abi, serial, remote, size, sha256, md5 = sys.argv[1:]
data = {
    "package": "com.acing.gow.adv",
    "game": "宝石战争",
    "kind": "official_from_device",
    "apk_path": apk,
    "version_name": version_name,
    "version_code": int(version_code),
    "installer": installer,
    "originating_package": originating,
    "primary_cpu_abi": abi,
    "serial": serial,
    "remote_path": remote,
    "size": int(size),
    "sha256": sha256,
    "md5": md5,
}
Path(path).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY
cat <<EOF
output=$OUT
size=$SIZE
md5=$MD5
sha256=$SHA256
meta=$META_FILE
EOF
