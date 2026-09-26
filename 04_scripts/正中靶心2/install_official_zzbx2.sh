#!/usr/bin/env bash
# 正中靶心2 官方 1.9 星空包安装阶段。
# 设备现有官方 1.9（CN=elliot lee）与本星空包（CN=StarryMagic）异签名，
# `pm install -r` 必然 INSTALL_FAILED_UPDATE_INCOMPATIBLE；
# 按 AGENTS.md「异签名先报告、不自动转卸载」：本脚本只尝试覆盖，
# 失败即停并保留现场，卸载官方包（丢本地数据）须用户另行确认后单独执行。
# 后续星空重建包之间同签名，只允许覆盖，不卸载/不清数据。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
VER="${ZZBX2_VER:-1.9}"
APK="${ZZBX2_SIGNED:-$NIXIANG_ROOT/08_output/正中靶心2/正中靶心2_${VER}_starry-releaseSigned.apk}"
PKG="com.openew.monster"
ADB_BIN="${ADB_BIN:-adb}"
REMOTE_APK="/data/local/tmp/zzbx2_${VER}_starry.apk"

[[ -f "$STATE_FILE" ]] || { echo "ERROR: 缺少 $STATE_FILE" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "ERROR: 缺少星空包 $APK" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$STATE_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: serial 为空" >&2; exit 1; }
command -v "$ADB_BIN" >/dev/null || { echo "ERROR: adb 不可用" >&2; exit 1; }
"$ADB_BIN" -s "$SERIAL" get-state >/dev/null

"$ADB_BIN" -s "$SERIAL" shell am force-stop "$PKG" 2>/dev/null || true
INSTALLED_PATH="$("$ADB_BIN" -s "$SERIAL" shell pm path "$PKG" 2>/dev/null | tr -d '\r' || true)"
echo "[install] force-stop: $PKG"
if [[ "$INSTALLED_PATH" == package:* ]]; then
  echo "[install] 检测到已安装 com.openew.monster，尝试同签名覆盖更新（不卸载、不清数据）"
else
  echo "[install] 未安装，执行首次安装"
fi

echo "[install] push 星空包"
"$ADB_BIN" -s "$SERIAL" push "$APK" "$REMOTE_APK"
echo "[install] pm install"
set +e
INSTALL_OUT="$("$ADB_BIN" -s "$SERIAL" shell pm install -r -d --user 0 "$REMOTE_APK" 2>&1)"
INSTALL_RC=$?
set -e
printf '%s\n' "$INSTALL_OUT"
if [[ $INSTALL_RC -ne 0 ]]; then
  echo "ERROR: 覆盖安装失败；已停止，未卸载、未清数据。异签名安装需另行授权。" >&2
  exit 1
fi
"$ADB_BIN" -s "$SERIAL" shell rm -f "$REMOTE_APK" || true

echo "[install] 设备包指纹闭环"
bash "$NIXIANG_ROOT/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su
"$ADB_BIN" -s "$SERIAL" shell dumpsys package "$PKG" | tr -d '\r' | \
  grep -E 'versionName=|versionCode=|firstInstallTime=|lastUpdateTime=' | head -n 8

echo "[install] 完成: $APK"
