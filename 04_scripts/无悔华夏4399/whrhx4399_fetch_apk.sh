#!/usr/bin/env bash
# 无悔华夏 4399：从真机只读提取当前安装的单一 base APK。
# 不安装、不启动、不清数据；若 adb pull 受阻，仅临时使用 root 复制到 /data/local/tmp。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
META_FILE="$SCRIPT_DIR/fetched_apk.json"
PKG="${PKG:-com.yhhy.whrhx.m4399}"
GAME="${GAME:-无悔华夏4399}"
SERIAL="${SERIAL:-f9a116a2}"
ADB_BIN="${ADB_BIN:-adb}"

command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }
"$ADB_BIN" -s "$SERIAL" get-state >/dev/null 2>&1 || {
  echo "错误: 设备不可用: $SERIAL" >&2
  exit 1
}

DUMP="$("$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r')"
VERSION_NAME="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionName=//p' | head -1)"
VERSION_CODE="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionCode=//p' | head -1 | awk '{print $1}')"
INSTALLER="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*installerPackageName=//p' | head -1)"
PRIMARY_ABI="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*primaryCpuAbi=//p' | head -1)"
FIRST_INSTALL="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*firstInstallTime=//p' | head -1)"
LAST_UPDATE="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*lastUpdateTime=//p' | head -1)"
PACKAGE_PATHS="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's/^package://p')"

[[ -n "$VERSION_NAME" ]] || { echo "错误: 无法读取 versionName: $PKG" >&2; exit 1; }
[[ -n "$VERSION_CODE" ]] || { echo "错误: 无法读取 versionCode: $PKG" >&2; exit 1; }
[[ -n "$PACKAGE_PATHS" ]] || { echo "错误: 设备未安装包: $PKG" >&2; exit 1; }
[[ "$VERSION_NAME" =~ ^[A-Za-z0-9._-]+$ ]] || {
  echo "错误: versionName 含不安全字符: $VERSION_NAME" >&2
  exit 1
}

PATH_COUNT="$(printf '%s\n' "$PACKAGE_PATHS" | sed '/^$/d' | wc -l | tr -d ' ')"
if [[ "$PATH_COUNT" -ne 1 ]]; then
  echo "错误: 检测到 $PATH_COUNT 个 APK 路径；当前流程只支持单一 base APK。" >&2
  printf '%s\n' "$PACKAGE_PATHS" >&2
  exit 1
fi

REMOTE_APK="$(printf '%s\n' "$PACKAGE_PATHS" | sed -n '1p')"
case "$REMOTE_APK" in
  /data/app/*) ;;
  *) echo "错误: 非预期安装路径: $REMOTE_APK" >&2; exit 1 ;;
esac

OUT_DIR="$NIXIANG_ROOT/02_apks/$GAME"
OUT="$OUT_DIR/${GAME}_${VERSION_NAME}_m4399_from_device.apk"
TMP="$OUT.part"
REMOTE_TMP="/data/local/tmp/whrhx4399_base.apk"

if [[ -e "$OUT" ]]; then
  echo "错误: 目标 APK 已存在，拒绝覆盖: $OUT" >&2
  exit 1
fi
mkdir -p "$OUT_DIR"
rm -f "$TMP"
trap 'rm -f "$TMP"' EXIT

echo "==> 尝试直接只读拉取"
if ! "$ADB_BIN" -s "$SERIAL" pull "$REMOTE_APK" "$TMP"; then
  echo "==> 直接拉取失败，使用 root 临时复制"
  rm -f "$TMP"
  "$ADB_BIN" -s "$SERIAL" shell "su -c 'cp \"$REMOTE_APK\" \"$REMOTE_TMP\" && chmod 644 \"$REMOTE_TMP\"'"
  "$ADB_BIN" -s "$SERIAL" pull "$REMOTE_TMP" "$TMP"
  "$ADB_BIN" -s "$SERIAL" shell "su -c 'rm -f \"$REMOTE_TMP\"'" || true
fi

mv "$TMP" "$OUT"
SIZE="$(stat -f '%z' "$OUT")"
SHA256="$(shasum -a 256 "$OUT" | awk '{print $1}')"
MD5="$(md5 -q "$OUT")"

python3 - "$META_FILE" "$OUT" "$VERSION_NAME" "$VERSION_CODE" "$INSTALLER" \
  "$PRIMARY_ABI" "$SERIAL" "$REMOTE_APK" "$FIRST_INSTALL" "$LAST_UPDATE" \
  "$SIZE" "$SHA256" "$MD5" <<'PY'
import json
import sys
from pathlib import Path

(
    path,
    apk,
    version_name,
    version_code,
    installer,
    abi,
    serial,
    remote,
    first_install,
    last_update,
    size,
    sha256,
    md5,
) = sys.argv[1:]
data = {
    "package": "com.yhhy.whrhx.m4399",
    "game": "无悔华夏4399",
    "apk_path": apk,
    "version_name": version_name,
    "version_code": int(version_code) if version_code.isdigit() else version_code,
    "installer": installer,
    "primary_cpu_abi": abi,
    "serial": serial,
    "remote_path": remote,
    "first_install_time": first_install,
    "last_update_time": last_update,
    "size": int(size),
    "sha256": sha256,
    "md5": md5,
}
Path(path).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

cat <<EOF
package=$PKG
versionName=$VERSION_NAME
versionCode=$VERSION_CODE
installer=$INSTALLER
primaryCpuAbi=$PRIMARY_ABI
serial=$SERIAL
remote=$REMOTE_APK
firstInstallTime=$FIRST_INSTALL
lastUpdateTime=$LAST_UPDATE
output=$OUT
size=$SIZE
sha256=$SHA256
md5=$MD5
meta=$META_FILE
EOF
