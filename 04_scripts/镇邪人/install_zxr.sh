#!/usr/bin/env bash
# 镇邪人候选包同签名覆盖安装。优先走 root pm install，避免 OPPO 安装器高风险确认框。
# 失败不自动卸载，不清数据。
set -euo pipefail

ROOT="/Users/aliang/Documents/nixiang"
SERIAL="${SERIAL:-f9a116a2}"
PKG="com.zxr.dlxmz.zxrwz37zgx10.gg9b"
APK="${1:-$ROOT/08_output/镇邪人/镇邪人_1.0.3_safe-releaseSigned.apk}"
REMOTE="/data/local/tmp/zxr_safe_install.apk"

[[ -f "$APK" ]] || { echo "ERROR: APK missing: $APK" >&2; exit 1; }
adb -s "$SERIAL" get-state >/dev/null

echo "[install] before:"
adb -s "$SERIAL" shell dumpsys package "$PKG" 2>/dev/null | tr -d '\r' \
  | grep -E 'versionName=|versionCode=|firstInstallTime=|lastUpdateTime=' | head -8 || true

adb -s "$SERIAL" push "$APK" "$REMOTE" >/dev/null
set +e
INSTALL_OUT="$(adb -s "$SERIAL" shell "su -c 'pm install -r -d --user 0 $REMOTE'" 2>&1 | tr -d '\r')"
RC=$?
set -e
printf '%s\n' "$INSTALL_OUT"
adb -s "$SERIAL" shell rm -f "$REMOTE" >/dev/null 2>&1 || true
if [[ $RC -ne 0 ]] || ! printf '%s\n' "$INSTALL_OUT" | grep -Fq 'Success'; then
  echo "ERROR: root pm install failed rc=$RC；未自动卸载、未清数据" >&2
  exit 1
fi

bash "$ROOT/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su

echo "[install] after:"
adb -s "$SERIAL" shell dumpsys package "$PKG" 2>/dev/null | tr -d '\r' \
  | grep -E 'versionName=|versionCode=|firstInstallTime=|lastUpdateTime=' | head -8 || true
echo "[install] done: $APK"
