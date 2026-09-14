#!/usr/bin/env bash
# 正中靶心2 官方 1.7 星空包安装阶段。
# 用户 2026-09-14 明确授权：卸载官方 1.7，不保留数据，安装星空包。
# 首轮卸载官方数据已由用户单独授权；后续星空重建包只允许同签名覆盖，不卸载/不清数据。
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
NIXIANG_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
STATE_FILE="$SCRIPT_DIR/.device_serial"
APK="$NIXIANG_ROOT/08_output/正中靶心2/正中靶心2_1.7_starry-releaseSigned.apk"
PKG="com.openew.monster"
ADB_BIN="${ADB_BIN:-adb}"
REMOTE_APK="/data/local/tmp/zzbx2_1.7_starry.apk"

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
  echo "[install] 检测到已安装星空包，执行同签名覆盖更新（不卸载、不清数据）"
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
