#!/usr/bin/env bash
# 正中靶心2：只读提取真机当前安装的官方 1.7 base APK。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
source "$NIXIANG_ROOT/04_scripts/_common/env.sh"
STATE_FILE="$SCRIPT_DIR/.device_serial"
META_FILE="$SCRIPT_DIR/official_apk.json"
PKG="${PKG:-com.openew.monster}"
GAME="正中靶心2"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少设备状态文件" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "错误: serial 为空" >&2; exit 1; }

DUMP="$("$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r')"
VERSION_NAME="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionName=//p' | head -1)"
VERSION_CODE="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionCode=//p' | head -1 | awk '{print $1}')"
INSTALLER="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*installerPackageName=//p' | head -1)"
PRIMARY_ABI="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*primaryCpuAbi=//p' | head -1)"
PACKAGE_PATHS="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's/^package://p')"

[[ "$VERSION_NAME" == "1.7" ]] || { echo "错误: 真机版本不是 1.7: $VERSION_NAME" >&2; exit 1; }
[[ -n "$VERSION_CODE" && "$PACKAGE_PATHS" ]] || { echo "错误: 包信息不完整" >&2; exit 1; }
PATH_COUNT="$(printf '%s\n' "$PACKAGE_PATHS" | sed '/^$/d' | wc -l | tr -d ' ')"
[[ "$PATH_COUNT" -eq 1 ]] || { echo "错误: 检测到 $PATH_COUNT 个 APK 路径" >&2; exit 1; }
REMOTE_APK="$(printf '%s\n' "$PACKAGE_PATHS" | sed -n '1p')"
case "$REMOTE_APK" in /data/app/*) ;; *) echo "错误: 非预期路径 $REMOTE_APK" >&2; exit 1;; esac

OUT_DIR="$NIXIANG_ROOT/02_apks/$GAME"
OUT="$OUT_DIR/${GAME}_${VERSION_NAME}_official.apk"
TMP="$OUT.part"
REMOTE_TMP="/data/local/tmp/zzbx2_official_base.apk"
mkdir -p "$OUT_DIR"; rm -f "$TMP"; trap 'rm -f "$TMP"' EXIT

if [[ -e "$OUT" ]]; then
  echo "[pull] 复用已拉取文件: $OUT"
  unzip -tq "$OUT" >/dev/null
else
  if ! "$ADB_BIN" -s "$SERIAL" pull "$REMOTE_APK" "$TMP"; then
    "$ADB_BIN" -s "$SERIAL" shell "su -c 'cp \"$REMOTE_APK\" \"$REMOTE_TMP\" && chmod 644 \"$REMOTE_TMP\"'"
    "$ADB_BIN" -s "$SERIAL" pull "$REMOTE_TMP" "$TMP"
    "$ADB_BIN" -s "$SERIAL" shell "su -c 'rm -f \"$REMOTE_TMP\"'" || true
  fi
  unzip -tq "$TMP" >/dev/null
  mv "$TMP" "$OUT"
fi
SIZE="$(stat -f '%z' "$OUT")"
SHA256="$(shasum -a 256 "$OUT" | awk '{print $1}')"
MD5="$(md5 -q "$OUT")"
SIG="$("$NIXIANG_APKSIGNER" verify --verbose --print-certs "$OUT" 2>&1)"
SIGNER_DN="$(printf '%s\n' "$SIG" | sed -n 's/^Signer #1 certificate DN: //p' | head -1)"
SIGNER_SHA256="$(printf '%s\n' "$SIG" | sed -n 's/^Signer #1 certificate SHA-256 digest: //p' | head -1)"
if [[ "$SIGNER_DN" == *"CN=StarryMagic"* ]]; then
  KIND="starry_rebuild_base"
else
  KIND="official"
fi
python3 - "$META_FILE" "$OUT" "$VERSION_NAME" "$VERSION_CODE" "$INSTALLER" "$PRIMARY_ABI" "$SERIAL" "$REMOTE_APK" "$SIZE" "$SHA256" "$MD5" "$KIND" "$SIGNER_DN" "$SIGNER_SHA256" <<'PY'
import json, sys
from pathlib import Path
path, apk, version_name, version_code, installer, abi, serial, remote, size, sha256, md5, kind, signer_dn, signer_sha256 = sys.argv[1:]
data = {
    'package': 'com.openew.monster', 'game': '正中靶心2', 'kind': kind,
    'apk_path': apk, 'version_name': version_name,
    'version_code': int(version_code) if version_code.isdigit() else version_code,
    'installer': installer, 'primary_cpu_abi': abi, 'serial': serial,
    'remote_path': remote, 'size': int(size), 'sha256': sha256, 'md5': md5,
    'signer_dn': signer_dn, 'signer_sha256': signer_sha256,
}
Path(path).write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
PY
printf 'package=%s\nversionName=%s\nversionCode=%s\nkind=%s\noutput=%s\nsize=%s\nsha256=%s\nmd5=%s\nsignerDN=%s\nmeta=%s\n' \
  "$PKG" "$VERSION_NAME" "$VERSION_CODE" "$KIND" "$OUT" "$SIZE" "$SHA256" "$MD5" "$SIGNER_DN" "$META_FILE"
