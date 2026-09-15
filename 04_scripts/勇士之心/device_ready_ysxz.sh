#!/usr/bin/env bash
# 勇士之心：识别唯一真机、确认 root，并核验斌哥改包是否安装。只读设备。
set -euo pipefail
trap 'rc=$?; echo "错误: device_ready_ysxz.sh 第 ${LINENO} 行失败 rc=${rc}" >&2' ERR

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
INFO_FILE="$SCRIPT_DIR/device_info.json"
PACKAGES_FILE="$SCRIPT_DIR/device_packages.txt"
ADB_BIN="${ADB_BIN:-adb}"
PKG="${PKG:-com.yongshi.tenojo.ys}"
REQUESTED_SERIAL="${SERIAL:-}"

command -v "$ADB_BIN" >/dev/null || { echo "错误: adb 不可用" >&2; exit 1; }

if [[ -n "$REQUESTED_SERIAL" ]]; then
  SERIALS="$REQUESTED_SERIAL"
else
  SERIALS="$("$ADB_BIN" devices | awk 'NR > 1 && $2 == "device" {print $1}')"
fi

COUNT="$(printf '%s\n' "$SERIALS" | sed '/^$/d' | wc -l | tr -d ' ')"
if [[ "$COUNT" -ne 1 ]]; then
  echo "错误: 需要唯一在线设备，当前识别到 $COUNT 台" >&2
  printf '%s\n' "$SERIALS" >&2
  exit 1
fi

SERIAL="$(printf '%s\n' "$SERIALS" | sed -n '1p')"
"$ADB_BIN" -s "$SERIAL" get-state >/dev/null

ROOT_ID="$("$ADB_BIN" -s "$SERIAL" shell "su -c id" 2>/dev/null | tr -d '\r' || true)"
if [[ "$ROOT_ID" != *"uid=0"* ]]; then
  echo "错误: root 不可用或未授权: $SERIAL" >&2
  echo "实际: ${ROOT_ID:-<empty>}" >&2
  exit 1
fi

PACKAGE_PATH="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" 2>/dev/null | tr -d '\r' | sed -n 's/^package://p' || true)"
if [[ -z "$PACKAGE_PATH" ]]; then
  echo "错误: 设备未安装预期斌哥包 $PKG" >&2
  echo "候选包（名称含 yongshi）:" >&2
  "$ADB_BIN" -s "$SERIAL" shell pm list packages | tr -d '\r' | sed -n 's/^package://p' | grep -i yongshi >&2 || true
  "$ADB_BIN" -s "$SERIAL" shell pm list packages -3 | tr -d '\r' | sed -n 's/^package://p' | sort > "$PACKAGES_FILE"
  echo "第三方包清单: $PACKAGES_FILE"
  echo "当前前台 Activity:"
  "$ADB_BIN" -s "$SERIAL" shell dumpsys activity activities | tr -d '\r' | grep -E 'mResumedActivity|topResumedActivity' | head -5 || true
  exit 1
fi

DUMP="$("$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r')"
VERSION_NAME="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionName=//p' | head -1)"
VERSION_CODE="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*versionCode=//p' | head -1 | awk '{print $1}')"
INSTALLER="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*installerPackageName=//p' | head -1)"
PRIMARY_ABI="$(printf '%s\n' "$DUMP" | sed -n 's/^[[:space:]]*primaryCpuAbi=//p' | head -1)"
MODEL="$("$ADB_BIN" -s "$SERIAL" shell getprop ro.product.model | tr -d '\r')"
ANDROID_VERSION="$("$ADB_BIN" -s "$SERIAL" shell getprop ro.build.version.release | tr -d '\r')"

printf '%s\n' "$SERIAL" > "$STATE_FILE"
python3 - "$INFO_FILE" "$SERIAL" "$PKG" "$VERSION_NAME" "$VERSION_CODE" "$INSTALLER" "$PRIMARY_ABI" "$MODEL" "$ANDROID_VERSION" <<'PY'
import json
import sys
from pathlib import Path

path, serial, package, version_name, version_code, installer, abi, model, android = sys.argv[1:]
data = {
    "serial": serial,
    "package": package,
    "version_name": version_name,
    "version_code": int(version_code) if version_code.isdigit() else version_code,
    "installer": installer,
    "primary_cpu_abi": abi,
    "device_model": model,
    "android_version": android,
    "root": True,
}
Path(path).write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
PY

printf 'serial=%s\npackage=%s\nversionName=%s\nversionCode=%s\ninstaller=%s\nprimaryCpuAbi=%s\nmodel=%s\nandroid=%s\nstate=%s\ninfo=%s\n' \
  "$SERIAL" "$PKG" "$VERSION_NAME" "$VERSION_CODE" "$INSTALLER" "$PRIMARY_ABI" "$MODEL" "$ANDROID_VERSION" "$STATE_FILE" "$INFO_FILE"
