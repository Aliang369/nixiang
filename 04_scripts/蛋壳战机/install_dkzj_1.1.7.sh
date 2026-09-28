#!/usr/bin/env bash
# 弹壳战机 1.1.7 (vc32) 星光候选包安装后端。
#
# 策略：
#   1) 先尝试同签覆盖安装（pm install -r -d），保留现有星光存档。
#   2) 覆盖失败（异签/包状态异常）时，完整卸载再安装星光包。
#   3) 不备份数据、不保留旧数据、不自动回滚或重装官方包。
set -euo pipefail

NIXIANG="/Users/aliang/Documents/nixiang"
GAME="蛋壳战机"
VERSION="1.1.7"
PKG="com.survivor.acecn"
SERIAL_FILE="$NIXIANG/04_scripts/$GAME/.device_serial"
APK="$NIXIANG/08_output/$GAME/${GAME}_${VERSION}_starry-releaseSigned.apk"
EXPECT_MD5="${DKZJ_EXPECT_MD5:-}"
INSTALL_OK="$NIXIANG/04_scripts/$GAME/.install_ok"
REMOTE_APK="/data/local/tmp/dkzj_starry_1.1.7.apk"

[[ -f "$SERIAL_FILE" ]] || { echo "ERROR: missing $SERIAL_FILE" >&2; exit 1; }
SERIAL="$(tr -d '[:space:]' < "$SERIAL_FILE")"
[[ -n "$SERIAL" ]] || { echo "ERROR: empty serial" >&2; exit 1; }
[[ -f "$APK" ]] || { echo "ERROR: missing $APK" >&2; exit 1; }

LOCAL_MD5="$(md5 -q "$APK")"
if [[ -n "$EXPECT_MD5" && "$LOCAL_MD5" != "$EXPECT_MD5" ]]; then
  echo "ERROR: candidate md5 mismatch: $LOCAL_MD5 != $EXPECT_MD5" >&2
  exit 1
fi

rm -f "$INSTALL_OK"
adb -s "$SERIAL" get-state >/dev/null
echo "[install] local candidate md5=$LOCAL_MD5"
if adb -s "$SERIAL" shell pm path "$PKG" >/dev/null 2>&1; then
  INSTALLED=1
  echo "[install] device current package:"
  adb -s "$SERIAL" shell dumpsys package "$PKG" \
    | grep -E "versionName=|versionCode=|userId=" | sed 's/^/  /'
else
  INSTALLED=0
  echo "[install] package is not currently installed"
fi


adb -s "$SERIAL" shell am force-stop "$PKG" >/dev/null 2>&1 || true
echo "[install] push $APK"
adb -s "$SERIAL" push "$APK" "$REMOTE_APK" >/dev/null

cleanup() {
  adb -s "$SERIAL" shell rm -f "$REMOTE_APK" >/dev/null 2>&1 || true
}
trap 'rc=$?; cleanup; exit $rc' EXIT

echo "[install] try same-signature cover install"
if [[ "$INSTALLED" == "1" ]]; then
  if ! adb -s "$SERIAL" shell pm install -r -d --user 0 "$REMOTE_APK"; then
    echo "[install] cover install failed; full uninstall and reinstall"
    adb -s "$SERIAL" shell pm uninstall "$PKG" >/dev/null
    adb -s "$SERIAL" shell pm install --user 0 "$REMOTE_APK"
  fi
else
  adb -s "$SERIAL" shell pm install --user 0 "$REMOTE_APK"
fi

echo "[install] verify device base.apk == local candidate"
bash "$NIXIANG/04_scripts/_common/verify_installed_apk.sh" "$SERIAL" "$PKG" "$APK" --su
printf '%s %s\n' "$LOCAL_MD5" "$(date '+%Y-%m-%d %H:%M:%S')" > "$INSTALL_OK"
echo "[install] done: $PKG"
