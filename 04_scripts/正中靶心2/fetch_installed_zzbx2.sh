#!/usr/bin/env bash
# 正中靶心2：只读提取真机当前安装的斌哥改包单一 base APK。
# 仅拉取，不安装、不卸载、不清数据、不启动游戏。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
META_FILE="$SCRIPT_DIR/binge_apk.json"
PKG="${PKG:-com.openew.monster}"
GAME="正中靶心2"
ADB_BIN="${ADB_BIN:-adb}"

[[ -f "$STATE_FILE" ]] || { echo "错误: 缺少设备状态文件，请先跑 device_ready 阶段" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "错误: 设备 serial 为空" >&2; exit 1; }
command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }

DUMP="$("$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r')"
VERSION_NAME="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionName=//p' | head -1)"
VERSION_CODE="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionCode=//p' | head -1 | awk '{print $1}')"
INSTALLER="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*installerPackageName=//p' | head -1)"
PRIMARY_ABI="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*primaryCpuAbi=//p' | head -1)"
PACKAGE_PATHS="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" | tr -d '\r' | sed -n 's/^package://p')"

[[ -n "$VERSION_NAME" ]] || { echo "错误: 无法读取 versionName: $PKG" >&2; exit 1; }
[[ -n "$VERSION_CODE" ]] || { echo "错误: 无法读取 versionCode: $PKG" >&2; exit 1; }
[[ "$VERSION_NAME" =~ ^[A-Za-z0-9._-]+$ ]] || { echo "错误: versionName 含不安全字符: $VERSION_NAME" >&2; exit 1; }
[[ -n "$PACKAGE_PATHS" ]] || { echo "错误: 设备未安装包: $PKG" >&2; exit 1; }

PATH_COUNT="$(printf '%s\n' "$PACKAGE_PATHS" | sed '/^$/d' | wc -l | tr -d ' ')"
if [[ "$PATH_COUNT" -ne 1 ]]; then
  echo "错误: 检测到 $PATH_COUNT 个 APK 路径；当前只允许完整单 base APK，拒绝漏拉 split。" >&2
  printf '%s\n' "$PACKAGE_PATHS" >&2
  exit 1
fi

REMOTE_APK="$(printf '%s\n' "$PACKAGE_PATHS" | sed -n '1p')"
case "$REMOTE_APK" in
  /data/app/*) ;;
  *) echo "错误: 非预期安装路径: $REMOTE_APK" >&2; exit 1 ;;
esac

OUT_DIR="$NIXIANG_ROOT/02_apks/$GAME"
OUT="$OUT_DIR/${GAME}_${VERSION_NAME}_binge_mod.apk"
TMP="$OUT.part"
REMOTE_TMP="/data/local/tmp/zzbx2_binge_base.apk"

if [[ -e "$OUT" ]]; then
  echo "错误: 目标 APK 已存在，拒绝覆盖: $OUT" >&2
  exit 1
fi
mkdir -p "$OUT_DIR"
rm -f "$TMP"
trap 'rm -f "$TMP"' EXIT

echo "==> 尝试直接只读拉取"
if ! "$ADB_BIN" -s "$SERIAL" pull "$REMOTE_APK" "$TMP"; then
  echo "==> 直接拉取失败，使用 root 复制到 /data/local/tmp 后再拉取"
  rm -f "$TMP"
  "$ADB_BIN" -s "$SERIAL" shell "su -c 'cp \"$REMOTE_APK\" \"$REMOTE_TMP\" && chmod 644 \"$REMOTE_TMP\"'"
  "$ADB_BIN" -s "$SERIAL" pull "$REMOTE_TMP" "$TMP"
  "$ADB_BIN" -s "$SERIAL" shell "su -c 'rm -f \"$REMOTE_TMP\"'" || true
fi

unzip -tq "$TMP" >/dev/null
mv "$TMP" "$OUT"
SIZE="$(stat -f '%z' "$OUT")"
SHA256="$(shasum -a 256 "$OUT" | awk '{print $1}')"
MD5="$(md5 -q "$OUT")"

python3 - "$META_FILE" "$OUT" "$VERSION_NAME" "$VERSION_CODE" "$INSTALLER" "$PRIMARY_ABI" "$SERIAL" "$REMOTE_APK" "$SIZE" "$SHA256" "$MD5" <<'PY'
import json
import sys
from pathlib import Path

path, apk, version_name, version_code, installer, abi, serial, remote, size, sha256, md5 = sys.argv[1:]
data = {
    "package": "com.openew.monster",
    "game": "正中靶心2",
    "kind": "binge_mod",
    "apk_path": apk,
    "version_name": version_name,
    "version_code": int(version_code) if version_code.isdigit() else version_code,
    "installer": installer,
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
package=$PKG
versionName=$VERSION_NAME
versionCode=$VERSION_CODE
installer=$INSTALLER
primaryCpuAbi=$PRIMARY_ABI
serial=$SERIAL
remote=$REMOTE_APK
output=$OUT
size=$SIZE
sha256=$SHA256
md5=$MD5
meta=$META_FILE
EOF
