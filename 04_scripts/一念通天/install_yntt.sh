#!/usr/bin/env bash
# 一念通天 720 管线 install 阶段：经用户明确授权卸载旧包并安装星光候选包。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="一念通天"
VERSION="720"
PKG="com.qinglv.Immortal"
SERIAL_FILE="$NIXIANG/04_scripts/$GAME/.device_serial"
APK="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_surgical-releaseSigned.apk"
REMOTE="/data/local/tmp/yntt_surgical.apk"

[[ -f "$SERIAL_FILE" ]] || { echo "ERROR: missing $SERIAL_FILE" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty serial" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "ERROR: missing $APK" >&2; exit 1; }

adb -s "$SERIAL" get-state >/dev/null
adb -s "$SERIAL" shell am force-stop "$PKG" || true


echo "[install] push $APK"
adb -s "$SERIAL" push "$APK" "$REMOTE" >/dev/null
trap 'adb -s "$SERIAL" shell rm -f "$REMOTE" >/dev/null 2>&1 || true' EXIT

echo "[install] try cover install (same StarryMagic cert)"
if ! adb -s "$SERIAL" shell pm install -r -d --user 0 "$REMOTE"; then
  echo "[install] cover failed; user authorized uninstall+reinstall, retrying clean"
  adb -s "$SERIAL" shell pm uninstall --user 0 "$PKG" >/dev/null
  adb -s "$SERIAL" shell pm install --user 0 "$REMOTE"
fi


echo "[install] verify device base.apk == local"
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su

echo "[install] done: $PKG"
